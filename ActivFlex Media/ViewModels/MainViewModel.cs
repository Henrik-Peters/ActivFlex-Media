#region License
// ActivFlex Media - Manage your media libraries
// Copyright(C) 2017 Henrik Peters
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.
#endregion
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using static System.IO.Path;
using ActivFlex.Configuration;
using ActivFlex.Converters;
using ActivFlex.Localization;
using ActivFlex.Navigation;
using ActivFlex.FileSystem;
using ActivFlex.Views;
using ActivFlex.Media;
using System.Diagnostics;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for the main application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        /// <summary>
        /// This thread will load the thumbnails from the loading queue.
        /// </summary>
        Thread thumbnailThread;

        /// <summary>
        /// Enqueue thumbnails to set them up for loading.
        /// </summary>
        Queue<IThumbnailViewModel> loadingQueue = new Queue<IThumbnailViewModel>();

        /// <summary>
        /// Interrupt flag to stop the thumbnail loading thread.
        /// </summary>
        public volatile bool loadThumbsInterrupt = true;

        /// <summary>
        /// Thread for preloading images to the left side of the active image.
        /// </summary>
        Thread leftPreloadThread;

        /// <summary>
        /// Thread for preloading images to the right side of the active image.
        /// </summary>
        Thread rightPreloadThread;

        /// <summary>
        /// Interrupt flag to stop the image preloading threads.
        /// </summary>
        public volatile bool preloadInterrupt = true;

        /// <summary>
        /// Used for synchronization with the left preload thread.
        /// </summary>
        private readonly object preloadLeftSync = new object();

        /// <summary>
        /// Used for synchronization with the right preload thread.
        /// </summary>
        private readonly object preloadRightSync = new object();

        /// <summary>
        /// The number of images that will be preloaded to 
        /// the left and right side of the active image.
        /// </summary>
        private const int preloadRange = 10;

        /// <summary>
        /// When an unloaded image is closer than this value
        /// to the active image a new preload will start.
        /// </summary>
        private const int preloadInitDistance = 2;

        /// <summary>
        /// The maximum number of in memory loaded 
        /// images when no preloading takes place.
        /// </summary>
        private const int maxInPlaceLoadedImages = 10;

        /// <summary>
        /// This queue holds images that can be disposed
        /// when no preloading takes place.
        /// </summary>
        Queue<MediaImage> inPlaceDisposeQueue = new Queue<MediaImage>();

        /// <summary>
        /// Variable for the image index property.
        /// </summary>
        private volatile int imageIndex = 0;

        /// <summary>
        /// The media playback control of the ui.
        /// </summary>
        private MediaElement mediaPlayer;

        /// <summary>
        /// Timer for updating the data of the media playback.
        /// </summary>
        private DispatcherTimer mediaTimer;

        /// <summary>
        /// Time in milliseconds of the media data update timer.
        /// </summary>
        private const int mediaTimerUpdateInterval = 200;

        /// <summary>
        /// Reference to the label for displaying the current playback time.
        /// </summary>
        private Label currentTimeLabel;

        /// <summary>
        /// Reference to the label for displaying the maximum playback time.
        /// </summary>
        private Label maxTimeLabel;

        /// <summary>
        /// Converter for the media playback times.
        /// </summary>
        private TimeSpanStringConverter timeSpanStringConverter = new TimeSpanStringConverter();

        /// <summary>
        /// The info icon in the media playback area.
        /// </summary>
        private ContentPresenter mediaInfoIcon;

        /// <summary>
        /// Index of the currently presented image.
        /// </summary>
        private int ImageIndex {
            get => imageIndex;
            set {
                imageIndex = value;

                //Check if the image preloading must be started
                if (Config.PreloadPresenterImages) {
                    foreach (MediaImage image in RightImages().Take(preloadInitDistance + 1)) {
                        if (image.LoadState == ImageLoadState.Waiting) {

                            lock (preloadRightSync) {
                                Monitor.Pulse(preloadRightSync);
                            }
                            break;
                        }
                    }

                    foreach (MediaImage image in LeftImages().Take(preloadInitDistance + 1)) {
                        if (image.LoadState == ImageLoadState.Waiting) {
                            
                            lock (preloadLeftSync) {
                                Monitor.Pulse(preloadLeftSync);
                            }
                            break;
                        }
                    }
                }
            }
        }

        #region Properties
        private ObservableCollection<NavItem> _navItems;
        public ObservableCollection<NavItem> NavItems {
            get => _navItems;
            set => SetProperty(ref _navItems, value);
        }
        
        private bool _navVisible;
        public bool NavVisible {
            get => _navVisible;
            set => SetProperty(ref _navVisible, value);
        }

        private bool _mediaBarVisible;
        public bool MediaBarVisible {
            get => _mediaBarVisible;
            set => SetProperty(ref _mediaBarVisible, value);
        }

        private ObservableCollection<IThumbnailViewModel> _fileSystemItems;
        public ObservableCollection<IThumbnailViewModel> FileSystemItems {
            get => _fileSystemItems;
            set {
                if (_fileSystemItems != value) {
                    _fileSystemItems = value;
                    _fileSystemItems.CollectionChanged += FileSystemItems_CollectionChanged;
                    FileSystemItems_CollectionChanged(this, null);
                    NotifyPropertyChanged();
                }
            }
        }

        private TranslateManager _translateManager;
        public TranslateManager Localize {
            get => _translateManager;
            set {
                SetProperty(ref _translateManager, value);
                UpdateNavLocalization(NavItems);
            }
        }

        private double _zoom;
        public double Zoom {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        private double _zoomDelta;
        public double ZoomDelta {
            get => _zoomDelta;
            set => SetProperty(ref _zoomDelta, value);
        }

        private string _path;
        public string Path {
            get => _path;
            set {
                SetProperty(ref _path, value);
                NotifyPropertyChanged(nameof(PathAvailable));
                NotifyPropertyChanged(nameof(BrowseUpAvailable));
            }
        }

        public bool PathAvailable {
            get => !string.IsNullOrEmpty(Path);
        }

        public bool BrowseUpAvailable {
            get => string.IsNullOrEmpty(Path) 
                ? false 
                : !Path.EndsWith(DirectorySeparatorChar.ToString());
        }

        private bool _imagePresentActive;
        public bool ImagePresentActive {
            get => _imagePresentActive;
            set => SetProperty(ref _imagePresentActive, value);
        }

        private ImageSource _imagePresentData;
        public ImageSource ImagePresentData {
            get => _imagePresentData;
            set => SetProperty(ref _imagePresentData, value);
        }
        
        private MediaImage[] _activeImages;
        public MediaImage[] ActiveImages {
            get => _activeImages;
            set {
                StopPreloadingThreads();
                SetProperty(ref _activeImages, value);
            }
        }

        /// <summary>
        /// Iterate over all images to the left side of the active image.
        /// When an overflow happens the iteration will continue at the end
        /// of the array. The iteration will start on the first image to the left
        /// side of the active image (exclusive) and end after the active image.
        /// </summary>
        /// <returns>Current image of the iteration</returns>
        IEnumerable<MediaImage> LeftImages()
        {
            int index = ImageIndex;
            while (index != ImageIndex + 1) {
                index--;
                if (index < 0)
                    index = _activeImages.Length - 1;

                yield return _activeImages[index];
            }
        }

        /// <summary>
        /// Iterate over all images to the right side of the active image.
        /// When an overflow happens the iteration will continue at the beginning
        /// of the array. The iteration will start on the first image to the right
        /// side of the active image (exclusive) and end before the active image.
        /// </summary>
        /// <returns>Current image of the iteration</returns>
        IEnumerable<MediaImage> RightImages()
        {
            int index = ImageIndex;
            while (index != ImageIndex - 1) {
                index++;
                if (index >= _activeImages.Length)
                    index = 0;

                yield return _activeImages[index];
            }
        }

        private ConfigData _config;
        public ConfigData Config {
            get => _config;
            set => SetProperty(ref _config, value);
        }

        private double _currentPlaybackTime;
        public double CurrentPlaybackTime {
            get => _currentPlaybackTime;
            set {
                if (!TimelineDragActive) {
                    mediaPlayer.Position = TimeSpan.FromMilliseconds(value);
                }

                SetProperty(ref _currentPlaybackTime, value);
            }
        }

        private double _maxPlaybackTime;
        public double MaxPlaybackTime {
            get => _maxPlaybackTime;
            set {
                maxTimeLabel.Content = timeSpanStringConverter.Convert(
                    mediaPlayer.NaturalDuration.TimeSpan, typeof(string), null, null);
                SetProperty(ref _maxPlaybackTime, value);
            }
        }

        private bool _timelineDragActive = false;
        public bool TimelineDragActive {
            get => _timelineDragActive;
            set => SetProperty(ref _timelineDragActive, value);
        }

        private Visibility _ShowTimelineSideLabels = Visibility.Collapsed;
        public Visibility ShowTimelineSideLabels {
            get => _ShowTimelineSideLabels;
            set => SetProperty(ref _ShowTimelineSideLabels, value);
        }

        private string _mediaInfoName = String.Empty;
        public string MediaInfoName {
            get => _mediaInfoName;
            set => SetProperty(ref _mediaInfoName, value);
        }

        private bool _playmode = false;
        public bool PlayMode {
            get => _playmode;
            set {
                if (mediaPlayer.HasAudio || mediaPlayer.HasVideo) {
                    if (value) {
                        mediaTimer.Start();
                        mediaPlayer.Play();
                        SetProperty(ref _playmode, value);

                        if (Config.ShowTimelineSideLabels) {
                            ShowTimelineSideLabels = Visibility.Visible;
                        } else {
                            ShowTimelineSideLabels = Visibility.Collapsed;
                        }

                    } else if (mediaPlayer.CanPause) {
                        mediaTimer.Stop();
                        mediaPlayer.Pause();
                        SetProperty(ref _playmode, value);
                    }
                }
            }
        }

        #endregion
        #region Commands

        /// <summary>
        /// Toggle the NavVisible property
        /// </summary>
        public ICommand ToggleNavVisibility { get; set; }

        /// <summary>
        /// Toggle the MediaBarVisible property
        /// </summary>
        public ICommand ToggleMediaBarVisibility { get; set; }

        /// <summary>
        /// Run the file system browser with the
        /// passed path. Requires a path as argument.
        /// </summary>
        public ICommand BrowseFileSystem { get; set; }

        /// <summary>
        /// Browse back to the last path. When the last
        /// path is not available nothing will be executed.
        /// </summary>
        public ICommand BrowseUp { get; set; }

        /// <summary>
        /// Increase the zoom level by the zoom delta.
        /// </summary>
        public ICommand IncreaseZoom { get; set; }

        /// <summary>
        /// Decrease the zoom level by the zoom delta.
        /// </summary>
        public ICommand DecreaseZoom { get; set; }

        /// <summary>
        /// Reset the zoom level to neutral (1.0).
        /// </summary>
        public ICommand ResetZoom { get; set; }

        /// <summary>
        /// Start the presenting mode with the
        /// media object passed as argument.
        /// </summary>
        public ICommand PresentImage { get; set; }

        /// <summary>
        /// Start the presenting mode with the
        /// argument as first media image.
        /// </summary>
        public ICommand LaunchPresenter { get; set; }

        /// <summary>
        /// Start the default music item launch
        /// method specificed by the config.
        /// </summary>
        public ICommand DefaultMusicLaunch { get; set; }

        /// <summary>
        /// Start the playback mode with the
        /// argument as the music item to play.
        /// </summary>
        public ICommand LaunchMusicPlayback { get; set; }

        /// <summary>
        /// Start the default media player for
        /// the passed argument as music item.
        /// </summary>
        public ICommand LaunchMusicProgramm { get; set; }

        /// <summary>
        /// Run an escape action. Depending on the
        /// current mode an action will be chosen.
        /// </summary>
        public ICommand ExitMode { get; set; }

        /// <summary>
        /// Switches to the next image of the media
        /// collection when in presentation mode.
        /// </summary>
        public ICommand NextImage { get; set; }

        /// <summary>
        /// Switches to the previous image of the media
        /// collection when in presentation mode.
        /// </summary>
        public ICommand PreviousImage { get; set; }

        /// <summary>
        /// Show the application info window dialog.
        /// </summary>
        public ICommand ShowInfo { get; set; }

        /// <summary>
        /// Stop the current media playback control.
        /// </summary>
        public ICommand Stop { get; set; }

        /// <summary>
        /// Next track of the media playback control.
        /// </summary>
        public ICommand Next { get; set; }

        /// <summary>
        /// Previous track of the media playback control.
        /// </summary>
        public ICommand Previous { get; set; }

        #endregion

        /// <summary>
        /// Creates a new ViewModel for the MainArea
        /// and instantiate all fields and commands.
        /// </summary>
        /// <param name="mediaPlayer">Instance of the media playback control</param>
        /// <param name="currentTimeLabel">Reference to the label for displaying the current playback time</param>
        /// <param name="maxTimeLabel">Reference to the label for displaying the maximum playback time</param>
        /// <param name="mediaInfoIcon">The info icon in the media playback areae</param>
        public MainViewModel(MediaElement mediaPlayer, Label currentTimeLabel, Label maxTimeLabel, ContentPresenter mediaInfoIcon)
        {
            //Configuration
            if (this.Config == null) {

                //Create a default config when no config exists
                if (!ConfigProvider.ConfigExists) {
                    ConfigProvider.SaveConfig(ConfigData.DefaultConfig);
                }

                this.Config = ConfigProvider.LoadConfig();
            }

            this.mediaPlayer = mediaPlayer;
            this.currentTimeLabel = currentTimeLabel;
            this.maxTimeLabel = maxTimeLabel;
            this.mediaInfoIcon = mediaInfoIcon;
            this.mediaInfoIcon.Visibility = Visibility.Hidden;
            this.mediaTimer = new DispatcherTimer(DispatcherPriority.Send) {
                Interval = TimeSpan.FromMilliseconds(mediaTimerUpdateInterval)
            };
            this.mediaTimer.Tick += new EventHandler(MediaTimerUpdate);
            this.Localize = new TranslateManager(Config.Language);

            //Navigation items
            this.NavVisible = true;
            this.MediaBarVisible = true;
            this.NavItems = new ObservableCollection<NavItem>(
                new List<NavItem>(new[] {
                    new GroupNavItem(Localize["MediaLibraries"], "MediaLibraries", "MediaLibraryIcon", true, NavTag.MediaLibraryRoot),
                    new GroupNavItem(Localize["MyComputer"], "MyComputer", "MyComputerIcon", true)
                })
            );

            this.NavItems[0].NavChildren = new ObservableCollection<NavItem>(
                new List<NavItem>(new[] {
                    new DirectoryNavItem(Localize["Pictures"], "PictureIcon", 
                                         Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), NavTag.None, "Pictures")
                })
            );

            this.NavItems[1].NavChildren = new ObservableCollection<NavItem>(
                                FileSystemBrowser.GetLogicalDrives()
                                .Select(drive => new LogicalDriveNavItem(drive)));

            //Default Zoom
            this.Zoom = 1.0;
            this.ZoomDelta = 0.1;

            //Presentation startup
            this.ActiveImages = new MediaImage[0];
            this.ImagePresentActive = false;

            //Threads
            this.thumbnailThread = new Thread(LoadThumbnails);

            //Commands
            this.ToggleNavVisibility = new RelayCommand(() => NavVisible = !NavVisible);
            this.ToggleMediaBarVisibility = new RelayCommand(() => MediaBarVisible = !MediaBarVisible);
            this.BrowseFileSystem = new RelayCommand<string>(BrowseToPath);
            this.ShowInfo = new RelayCommand(() => new InfoWindow().ShowDialog());
            this.ResetZoom = new RelayCommand(() => Zoom = 1.0);
            this.IncreaseZoom = new RelayCommand(() => Zoom += ZoomDelta);
            this.DecreaseZoom = new RelayCommand(() => Zoom -= ZoomDelta);
            
            this.BrowseUp = new RelayCommand(() => {
                if (ImagePresentActive) {
                    this.ExitMode?.Execute(null);
                } else {
                    BrowseFileSystem.Execute(FileSystemBrowser.GetParentPath(Path));
                }
            });

            this.ExitMode = new RelayCommand(ExitCurrentMode);
            this.NextImage = new RelayCommand(() => ChangeActiveImage(true));
            this.PreviousImage = new RelayCommand(() => ChangeActiveImage(false));
            this.LaunchPresenter = new RelayCommand<MediaImage>(LaunchImagePresenter);
            this.PresentImage = new RelayCommand<MediaImage>(PresentMediaImage);
            this.LaunchMusicPlayback = new RelayCommand<MediaMusic>(StartMusicPlayback);
            this.LaunchMusicProgramm = new RelayCommand<MediaMusic>(music => Process.Start(music.Path));
            this.DefaultMusicLaunch = new RelayCommand<MediaMusic>(music => {
                if (Config.MusicLaunchBehavior == LaunchBehavior.Self) {
                    this.LaunchMusicPlayback.Execute(music);
                } else {
                    this.LaunchMusicProgramm.Execute(music);
                }
            });

            //Media playback commands
            this.Stop = new RelayCommand(StopCurrentPlayback);
            this.Next = new RelayCommand(() => ChangeActiveImage(true));
            this.Previous = new RelayCommand(() => ChangeActiveImage(false));
        }

        /// <summary>
        /// Launch the media playback for the passed music
        /// item. Running playbacks will be stopped.
        /// </summary>
        /// <param name="music">Music item to play</param>
        private void StartMusicPlayback(MediaMusic music)
        {
            if (File.Exists(music.Path)) {
                Stop.Execute(null);
                MediaInfoName = music.Name;
                this.mediaInfoIcon.Visibility = Visibility.Visible;
                mediaPlayer.Source = new Uri(music.Path);
                mediaTimer.Start();
                mediaPlayer.Play();

                if (Config.ShowTimelineSideLabels) {
                    ShowTimelineSideLabels = Visibility.Visible;
                } else {
                    ShowTimelineSideLabels = Visibility.Collapsed;
                }

                _playmode = true;
                NotifyPropertyChanged(nameof(PlayMode));
            }
        }

        /// <summary>
        /// Stop the current media playback.
        /// </summary>
        private void StopCurrentPlayback()
        {
            if (CurrentPlaybackTime > 0) {
                mediaTimer.Stop();
                mediaPlayer.Stop();

                _currentPlaybackTime = 0;
                _playmode = false;
                currentTimeLabel.Content = "00:00";
                NotifyPropertyChanged(nameof(PlayMode));
                NotifyPropertyChanged(nameof(CurrentPlaybackTime));

            } else if (ImagePresentActive) {
                ImagePresentActive = false;
                preloadInterrupt = true;
            }
        }

        /// <summary>
        /// Triggered every time when the media data
        /// of the current playback should be updated.
        /// </summary>
        private void MediaTimerUpdate(object sender, EventArgs e)
        {
            if (!TimelineDragActive) {
                _currentPlaybackTime = mediaPlayer.Position.TotalMilliseconds;
                NotifyPropertyChanged(nameof(CurrentPlaybackTime));
            }

            currentTimeLabel.Content = timeSpanStringConverter.Convert(mediaPlayer.Position, typeof(string), null, null);
        }

        /// <summary>
        /// Start the image preloading threads. Make sure the
        /// correct image index is set before starting a preload.
        /// </summary>
        private void StartPreloadingThreads()
        {
            StopPreloadingThreads();

            if (Config.PreloadPresenterImages) {
                preloadInterrupt = false;

                leftPreloadThread = new Thread(PreloadLeftImages);
                rightPreloadThread = new Thread(PreloadRightImages);

                leftPreloadThread.Start();
                rightPreloadThread.Start();
            }
        }

        /// <summary>
        /// Interrupt the preloading threads and block the current
        /// thread until both threads have stopped their execution.
        /// </summary>
        public void StopPreloadingThreads()
        {
            if (leftPreloadThread != null && rightPreloadThread != null) {
                if (leftPreloadThread.IsAlive || rightPreloadThread.IsAlive) {

                    preloadInterrupt = true;
                    leftPreloadThread.Interrupt();
                    rightPreloadThread.Interrupt();

                    try {
                        if (leftPreloadThread.IsAlive)
                            leftPreloadThread.Join();

                        if (rightPreloadThread.IsAlive)
                            rightPreloadThread.Join();

                    } catch { }
                }
            }
        }

        /// <summary>
        /// This method will be executed by the left preload thread.
        /// Images out of the loading range will be disposed.
        /// </summary>
        private void PreloadLeftImages()
        {
            while (!preloadInterrupt) {
                //Dispose images out of the loading range
                foreach (MediaImage image in RightImages().Take(2 * preloadRange - preloadInitDistance).Skip(preloadRange)) {
                    if (image.LoadState == ImageLoadState.Successful) {
                        image.DisposeImage();
                    }
                }

                //Preload images to the left side
                foreach (MediaImage image in LeftImages().Take(preloadRange)) {
                    if (preloadInterrupt) break;
                    if (image.LoadState == ImageLoadState.Waiting) {
                        image.LoadImage();
                    }
                }

                //Wait for a new preload request or an interrupt
                lock (preloadLeftSync) {
                    try {
                        Monitor.Wait(preloadLeftSync);
                    } catch (ThreadInterruptedException) { }
                }
            }
        }

        /// <summary>
        /// This method will be executed by the right preload thread.
        /// Images out of the loading range will be disposed.
        /// </summary>
        private void PreloadRightImages()
        {
            while (!preloadInterrupt) {
                //Dispose images out of the loading range
                foreach (MediaImage image in LeftImages().Take(2 * preloadRange - preloadInitDistance).Skip(preloadRange)) {
                    if (image.LoadState == ImageLoadState.Successful) {
                        image.DisposeImage();
                    }
                }

                //Preload images to the right side
                foreach (MediaImage image in RightImages().Take(preloadRange)) {
                    if (preloadInterrupt) break;
                    if (image.LoadState == ImageLoadState.Waiting) {
                        image.LoadImage();
                    }
                }

                //Wait for a new preload request or an interrupt
                lock (preloadRightSync) {
                    try {
                        Monitor.Wait(preloadRightSync);
                    } catch (ThreadInterruptedException) { }
                }
            }
        }

        /// <summary>
        /// Update the thumbnail loading queue when
        /// the FileSystemItems collection changed.
        /// </summary>
        private void FileSystemItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            loadThumbsInterrupt = true;
            if (thumbnailThread.IsAlive)
                thumbnailThread.Join();

            loadingQueue.Clear();
            
            foreach (var item in FileSystemItems.Where(item => item is ImageItemViewModel)) {
                loadingQueue.Enqueue(item);
            }

            thumbnailThread = new Thread(LoadThumbnails);
            loadThumbsInterrupt = false;
            thumbnailThread.Start();
        }

        /// <summary>
        /// This method will be executed by the thumbnail loading thread.
        /// The loaded thumbnails will be stored in the related view models.
        /// </summary>
        private void LoadThumbnails()
        {
            while (!loadThumbsInterrupt && loadingQueue.Any()) {
                IThumbnailViewModel viewModelItem = loadingQueue.Dequeue();

                if (viewModelItem is ImageItemViewModel item) {
                    item.LoadThumbnail(Config.ThumbnailDecodeSize);
                }
            }
        }

        /// <summary>
        /// Run the FileSystemBrowser for the provided path.
        /// This will also create the related view model
        /// implementation for the object and store them 
        /// in the FileSystemItems collection.
        /// </summary>
        /// <param name="path">Filesystem path for the browser</param>
        private void BrowseToPath(string path)
        {
            this.Path = path;
            FileSystemItems = new ObservableCollection<IThumbnailViewModel>(FileSystemBrowser.Browse(path)
                .Where(item => item is DirectoryItem || item is MediaImage || item is MediaMusic)
                .Select<IFileObject, IThumbnailViewModel>(item => {

                    if (item is MediaImage imageItem) {
                        return new ImageItemViewModel(imageItem);
                    }

                    if (item is MediaMusic musicItem) {
                        return new MusicItemViewModel(musicItem);
                    }

                    return new DirectoryItemViewModel((DirectoryItem)item);
                })
            );
        }

        /// <summary>
        /// Enqueue a media image for disposing when no preload is active.
        /// When the queue will be too long images from the queue will be disposed.
        /// </summary>
        /// <param name="image">Image to enqueue for later disposing</param>
        private void EnqueueInPlaceDispose(MediaImage image)
        {
            if (!Config.PreloadPresenterImages && image.LoadState == ImageLoadState.Successful) {

                if (inPlaceDisposeQueue.Count > maxInPlaceLoadedImages) {
                    MediaImage disposeImage = inPlaceDisposeQueue.Dequeue();
                    disposeImage.DisposeImage();
                }

                inPlaceDisposeQueue.Enqueue(image);
            }
        }

        /// <summary>
        /// Start the image presentation mode with an image
        /// from the current FileSystemItems collection. 
        /// All Images will be stored in the ActiveImage collection 
        /// and the imageIndex will be set to the first image.
        /// </summary>
        /// <param name="mediaImage">Image to display first</param>
        private void LaunchImagePresenter(MediaImage mediaImage)
        {
            PresentMediaImage(mediaImage);

            //Store all images of the parent location in the active media collection
            ActiveImages = FileSystemItems
                .Select(item => item.Proxy)
                .Where(item => item is MediaImage)
                .Cast<MediaImage>()
                .ToArray();

            ImageIndex = Array.IndexOf(ActiveImages, mediaImage);
            StartPreloadingThreads();
        }

        /// <summary>
        /// Start the presentation mode for an image.
        /// The image will be loaded when the image is 
        /// still waiting for loading.
        /// </summary>
        /// <param name="mediaImage">Image data for the presenter</param>
        private void PresentMediaImage(MediaImage mediaImage)
        {
            if (mediaImage.LoadState == ImageLoadState.Waiting) {
                mediaImage.LoadImage();

                //Dispose images for in-place-loading
                if (!Config.PreloadPresenterImages) {
                    EnqueueInPlaceDispose(mediaImage);
                }
            }

            if (mediaImage.LoadState == ImageLoadState.Successful) {
                ImagePresentData = mediaImage.Image;
                ImagePresentActive = true;
            }
        }

        /// <summary>
        /// Change the active image to an image next to
        /// the actual image. ActiveImages is used as the
        /// image collection. Overflows will be handled.
        /// </summary>
        /// <param name="next">Use the next or previous image</param>
        /// <returns>True when another image could be used</returns>
        private bool ChangeActiveImage(bool next)
        {
            MediaImage nextImage = null;
            int loadingTries = 0;

            if (!ImagePresentActive)
                return false;

            do {
                if (loadingTries++ > ActiveImages.Length)
                    return false;

                //Calculate the next index and handle overflows
                if (next) ImageIndex++;
                else ImageIndex--;

                if (ImageIndex < 0) {
                    ImageIndex = ActiveImages.Length - 1;

                } else if (ImageIndex >= ActiveImages.Length) {
                    ImageIndex = 0;
                }

                if (ImageIndex >= 0 && ImageIndex < ActiveImages.Length) {
                    nextImage = ActiveImages[ImageIndex];
                }

                //Check if the image is still waiting for loading
                if (nextImage?.LoadState == ImageLoadState.Waiting) {
                    nextImage.LoadImage();

                    //Dispose images for in-place-loading
                    if (!Config.PreloadPresenterImages) {
                        EnqueueInPlaceDispose(nextImage);
                    }
                }

            } while (!(nextImage?.LoadState == ImageLoadState.Successful));

            PresentMediaImage(nextImage);
            return true;
        }

        /// <summary>
        /// This function will update the language 
        /// translations for the main navigation.
        /// </summary>
        /// <param name="Items">Navigation items to update</param>
        private void UpdateNavLocalization(ICollection<NavItem> Items)
        {
            if (Items != null && Items.Any()) {
                foreach (NavItem item in Items) {
                    if (!String.IsNullOrEmpty(item.LocalizeKey)) {
                        
                        try {
                            string translation = _translateManager[item.LocalizeKey];
                            item.DisplayName = translation;
                        } catch { }
                    }

                    if (item.NavChildren != null && item.NavChildren.Any()) {
                        UpdateNavLocalization(item.NavChildren);
                    }
                }
            }
        }

        /// <summary>
        /// Choose an exit action depending on the current
        /// application mode. On the Top-Level this will
        /// shutdown the complete application instance.
        /// </summary>
        private void ExitCurrentMode()
        {
            if (ImagePresentActive) {
                ImagePresentActive = false;
                preloadInterrupt = true;
            } else {
                Application.Current.Shutdown();
            }
        }
    }
}
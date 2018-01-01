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
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using ActivFlex.Configuration;
using ActivFlex.Converters;
using ActivFlex.Localization;
using ActivFlex.Navigation;
using ActivFlex.FileSystem;
using ActivFlex.Libraries;
using ActivFlex.Storage;
using ActivFlex.Views;
using ActivFlex.Media;

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
        Queue<ViewModel> loadingQueue = new Queue<ViewModel>();

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
        /// The navigation area tree view control.
        /// </summary>
        private TreeView navView;

        /// <summary>
        /// The tree view item with an active edit box.
        /// </summary>
        public TreeViewItem editItem;

        /// <summary>
        /// The last item that was selected.
        /// </summary>
        private ILibraryItemViewModel lastSelection;

        /// <summary>
        /// The last filesystem item that was selected.
        /// </summary>
        private IThumbnailViewModel lastFileSelection;

        /// <summary>
        /// The media player to generate the thumbnail image.
        /// </summary>
        MediaPlayer thumbnailPlayer;

        /// <summary>
        /// The time of the video for the thumbnail shot.
        /// </summary>
        private static readonly TimeSpan DefaultThumbnailTime = TimeSpan.FromSeconds(0.1);

        /// <summary>
        /// This queue holds video items for thumbnail loading.
        /// </summary>
        Queue<ViewModel> vidLoadQueue;

        /// <summary>
        /// The storage provider for the persistent data storage.
        /// </summary>
        public static IStorageProvider StorageEngine;

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

        private bool _libraryBrowsing;
        public bool LibraryBrowsing {
            get => _libraryBrowsing;
            set {
                if (!value) {
                    EmptyContainerInfo = Visibility.Collapsed;
                }
                SetProperty(ref _libraryBrowsing, value);
            }
        }

        private ObservableCollection<IThumbnailViewModel> _fileSystemItems;
        public ObservableCollection<IThumbnailViewModel> FileSystemItems {
            get => _fileSystemItems;
            set {
                if (_fileSystemItems != value) {
                    _fileSystemItems = value;
                    _fileSystemItems.CollectionChanged += Items_CollectionChanged;
                    Items_CollectionChanged(this, null);
                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<ILibraryItemViewModel> _libraryItems;
        public ObservableCollection<ILibraryItemViewModel> LibraryItems {
            get => _libraryItems;
            set {
                if (_libraryItems != value) {
                    _libraryItems = value;
                    LibraryItems.CollectionChanged += Items_CollectionChanged;
                    Items_CollectionChanged(this, null);
                    NotifyPropertyChanged();
                }

                //Empty media container info box
                if (LibraryBrowsing && _libraryItems.Count == 0) {
                    EmptyContainerInfo = Visibility.Visible;
                } else {
                    EmptyContainerInfo = Visibility.Collapsed;
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
                BrowseUpAvailable = true;
                NotifyPropertyChanged(nameof(PathAvailable));
            }
        }

        public bool PathAvailable {
            get => !string.IsNullOrEmpty(Path);
        }

        private bool _browseUpAvailable;
        public bool BrowseUpAvailable {
            get => _browseUpAvailable;
            set => SetProperty(ref _browseUpAvailable, value);
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

        private MediaContainer _activeContainer;
        public MediaContainer ActiveContainer {
            get => _activeContainer;
            set => SetProperty(ref _activeContainer, value);
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
        
        public static ConfigData Config { get; set; }

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
            set {
                if (value == Visibility.Visible) {
                    mediaInfoIcon.Width = 40;
                    mediaInfoIcon.Margin = new Thickness(5, 0, 0, 0);
                }

                SetProperty(ref _ShowTimelineSideLabels, value);
            }
        }

        private Visibility _ShowVideoPlayback = Visibility.Hidden;
        public Visibility ShowVideoPlayback {
            get => _ShowVideoPlayback;
            set => SetProperty(ref _ShowVideoPlayback, value);
        }

        private Visibility _emptyContainerInfo;
        public Visibility EmptyContainerInfo {
            get => _emptyContainerInfo;
            set => SetProperty(ref _emptyContainerInfo, value);
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

                        if (mediaPlayer.HasVideo) {
                            this.ShowVideoPlayback = Visibility.Visible;
                        }

                    } else if (mediaPlayer.CanPause) {
                        mediaTimer.Stop();
                        mediaPlayer.Pause();
                        SetProperty(ref _playmode, value);
                    }
                }
            }
        }

        public bool HasItemSelection {
            get => (LibraryItems != null && LibraryItems.Any(item => item.IsSelected))
                || (FileSystemItems != null && FileSystemItems.Any(item => item.IsSelected));
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
        /// Start the default image item launch
        /// method specificed by the config.
        /// </summary>
        public ICommand DefaultImageLaunch { get; set; }

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
        /// Start the default process player for
        /// the passed argument as music item.
        /// </summary>
        public ICommand LaunchDefault { get; set; }

        /// <summary>
        /// Start the default video item launch
        /// method specificed by the config.
        /// </summary>
        public ICommand DefaultVideoLaunch { get; set; }

        /// <summary>
        /// Start the playback mode with the
        /// argument as the video item to play.
        /// </summary>
        public ICommand LaunchVideoPlayback { get; set; }

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

        /// <summary>
        /// Show the config window to create a new media library.
        /// </summary>
        public ICommand CreateMediaLibrary { get; set; }

        /// <summary>
        /// Select the passed media library in the navigation view.
        /// </summary>
        public ICommand SelectNavigationLibrary { get; set; }
        
        /// <summary>
        /// Display the passed media library with the item browser.
        /// </summary>
        public ICommand OpenMediaLibrary { get; set; }

        /// <summary>
        /// Show the settings dialog to change the passed media library.
        /// </summary>
        public ICommand ConfigureMediaLibrary { get; set; }

        /// <summary>
        /// Show the delete dialog for the passed media library.
        /// </summary>
        public ICommand DeleteMediaLibrary { get; set; }

        /// <summary>
        /// Create a new media container in the passed container.
        /// </summary>
        public ICommand CreateMediaContainer { get; set; }

        /// <summary>
        /// Select the passed media container in the navigation view.
        /// </summary>
        public ICommand SelectMediaContainer { get; set; }

        /// <summary>
        /// Open the passed media container instance.
        /// </summary>
        public ICommand OpenMediaContainer { get; set; }

        /// <summary>
        /// Open the rename dialog for the passed container.
        /// </summary>
        public ICommand RenameMediaContainer { get; set; }

        /// <summary>
        /// Open the delete dialog for the passed container.
        /// </summary>
        public ICommand DeleteMediaContainer { get; set; }

        /// <summary>
        /// Show the library item import dialog. The items
        /// will be added to the passed media container.
        /// </summary>
        public ICommand LaunchMediaImport { get; set; }

        /// <summary>
        /// Start the explorer and select the passed file item.
        /// </summary>
        public ICommand ExplorerItemLaunch { get; set; }

        /// <summary>
        /// Open the delete dialog for a single library item.
        /// </summary>
        public ICommand DeleteLibraryItem { get; set; }

        /// <summary>
        /// Executed when an image library item is clicked.
        /// </summary>
        public ICommand LibraryImageClick { get; set; }

        /// <summary>
        /// Executed when a music library item is clicked.
        /// </summary>
        public ICommand LibraryMusicClick { get; set; }

        /// <summary>
        /// Executed when a video library item is clicked.
        /// </summary>
        public ICommand LibraryVideoClick { get; set; }

        /// <summary>
        /// Executed when a filesystem image item is clicked.
        /// </summary>
        public ICommand FileImageClick { get; set; }

        /// <summary>
        /// Executed when a filesystem music item is clicked.
        /// </summary>
        public ICommand FileMusicClick { get; set; }

        /// <summary>
        /// Executed when a filesystem video item is clicked.
        /// </summary>
        public ICommand FileVideoClick { get; set; }

        /// <summary>
        /// Executed when the mouse is pressed on a filesystem item.
        /// </summary>
        public ICommand FileItemMouseDown { get; set; }

        #endregion

        /// <summary>
        /// Creates a new ViewModel for the MainArea
        /// and instantiate all fields and commands.
        /// </summary>
        /// <param name="mediaPlayer">Instance of the media playback control</param>
        /// <param name="navView">Instance of the navigation tree view control</param>
        /// <param name="currentTimeLabel">Reference to the label for displaying the current playback time</param>
        /// <param name="maxTimeLabel">Reference to the label for displaying the maximum playback time</param>
        /// <param name="mediaInfoIcon">The info icon in the media playback areae</param>
        public MainViewModel(MediaElement mediaPlayer, TreeView navView, Label currentTimeLabel, Label maxTimeLabel, ContentPresenter mediaInfoIcon)
        {
            //Configuration
            if (Config == null) {

                //Create a default config when no config exists
                if (!ConfigProvider.ConfigExists) {
                    ConfigProvider.SaveConfig(ConfigData.DefaultConfig);
                }

                Config = ConfigProvider.LoadConfig();
            }

            this.navView = navView;
            this.mediaPlayer = mediaPlayer;
            this.currentTimeLabel = currentTimeLabel;
            this.maxTimeLabel = maxTimeLabel;
            this.mediaInfoIcon = mediaInfoIcon;
            this.EmptyContainerInfo = Visibility.Collapsed;
            this.mediaInfoIcon.Visibility = Visibility.Hidden;
            this.mediaTimer = new DispatcherTimer(DispatcherPriority.Send) {
                Interval = TimeSpan.FromMilliseconds(mediaTimerUpdateInterval)
            };
            this.mediaTimer.Tick += new EventHandler(MediaTimerUpdate);
            this.Localize = new TranslateManager(Config.Language);
            StorageEngine = new SQLiteProvider();

            //Navigation items
            this.NavVisible = true;
            this.MediaBarVisible = true;
            this.BrowseUpAvailable = false;
            this.NavItems = new ObservableCollection<NavItem>(
                new List<NavItem>(new[] {
                    new GroupNavItem(Localize["MediaLibraries"], "MediaLibraries", "MediaLibraryIcon", true, NavTag.MediaLibraryRoot),
                    new GroupNavItem(Localize["MyComputer"], "MyComputer", "MyComputerIcon", true)
                })
            );
            
            LoadMediaLibraries();
            this.NavItems[1].NavChildren = new ObservableCollection<NavItem>(
                                FileSystemBrowser.GetLogicalDrives()
                                .Select(drive => new LogicalDriveNavItem(drive)));

            //Default Zoom
            this.Zoom = 1.0;
            this.ZoomDelta = 0.1;

            //Presentation startup
            this.ActiveImages = new MediaImage[0];
            this.ImagePresentActive = false;
            this.LibraryBrowsing = false;

            //Threads
            this.thumbnailThread = new Thread(LoadThumbnails);

            //Commands
            this.ToggleNavVisibility = new RelayCommand(() => NavVisible = !NavVisible);
            this.ToggleMediaBarVisibility = new RelayCommand(() => MediaBarVisible = !MediaBarVisible);
            this.BrowseFileSystem = new RelayCommand<string>(BrowseToPath);
            this.CreateMediaLibrary = new RelayCommand(CreateLibrary);
            this.ShowInfo = new RelayCommand(() => new InfoWindow().ShowDialog());
            this.ResetZoom = new RelayCommand(() => Zoom = 1.0);
            this.IncreaseZoom = new RelayCommand(() => Zoom += ZoomDelta);
            this.DecreaseZoom = new RelayCommand(() => Zoom -= ZoomDelta);
            
            this.BrowseUp = new RelayCommand(() => {
                if (ImagePresentActive) {
                    this.ExitMode?.Execute(null);

                } else if (LibraryBrowsing) {
                    if (ActiveContainer.Parent != null) {
                        SelectMediaContainer.Execute(ActiveContainer.Parent);
                    }

                } else {
                    BrowseFileSystem.Execute(FileSystemBrowser.GetParentPath(Path));
                }
            });

            this.ExitMode = new RelayCommand(ExitCurrentMode);
            this.NextImage = new RelayCommand(() => ChangeActiveImage(true));
            this.PreviousImage = new RelayCommand(() => ChangeActiveImage(false));
            this.OpenMediaLibrary = new RelayCommand<MediaLibrary>(BrowseMediaLibrary);
            this.SelectNavigationLibrary = new RelayCommand<MediaLibrary>(SelectMediaLibrary);
            this.ConfigureMediaLibrary = new RelayCommand<MediaLibrary>(ShowMediaLibraryConfig);
            this.DeleteMediaLibrary = new RelayCommand<MediaLibrary>(RemoveMediaLibrary);
            this.CreateMediaContainer = new RelayCommand<MediaContainer>(NewMediaContainer);
            this.SelectMediaContainer = new RelayCommand<MediaContainer>(MediaContainerSelect);
            this.OpenMediaContainer = new RelayCommand<MediaContainer>(BrowseMediaContainer);
            this.RenameMediaContainer = new RelayCommand<MediaContainer>(MediaContainerRename);
            this.DeleteMediaContainer = new RelayCommand<MediaContainer>(RemoveMediaContainer);
            this.LaunchMediaImport = new RelayCommand<MediaContainer>(StartMediaImport);
            this.ExplorerItemLaunch = new RelayCommand<IFileObject>(LaunchFileExplorer);
            this.DeleteLibraryItem = new RelayCommand<ILibraryItem>(RemoveLibraryItem);
            this.LaunchPresenter = new RelayCommand<MediaImage>(LaunchImagePresenter);
            this.PresentImage = new RelayCommand<MediaImage>(PresentMediaImage);
            this.LaunchMusicPlayback = new RelayCommand<MediaMusic>(StartMusicPlayback);
            this.LaunchVideoPlayback = new RelayCommand<MediaVideo>(StartVideoPlayback);
            this.LibraryImageClick = new RelayCommand<LibraryImageViewModel>(ImageItemClick);
            this.LibraryMusicClick = new RelayCommand<LibraryMusicViewModel>(MusicItemClick);
            this.LibraryVideoClick = new RelayCommand<LibraryVideoViewModel>(VideoItemClick);
            this.FileImageClick = new RelayCommand<ImageItemViewModel>(ImageFileItemClick);
            this.FileMusicClick = new RelayCommand<MusicItemViewModel>(MusicFileItemClick);
            this.FileVideoClick = new RelayCommand<VideoItemViewModel>(VideoFileItemClick);
            this.FileItemMouseDown = new RelayCommand<IThumbnailViewModel>(FileItemDragInit);
            this.LaunchDefault = new RelayCommand<IFileObject>(media => {
                if (File.Exists(media.Path)) {
                    Process.Start(media.Path);
                }
            });
            this.DefaultImageLaunch = new RelayCommand<MediaImage>(image => {
                if (Config.ImageLaunchBehavior == LaunchBehavior.Self) {
                    this.LaunchPresenter.Execute(image);
                } else {
                    this.LaunchDefault.Execute(image);
                }
            });
            this.DefaultMusicLaunch = new RelayCommand<MediaMusic>(music => {
                if (Config.MusicLaunchBehavior == LaunchBehavior.Self) {
                    this.LaunchMusicPlayback.Execute(music);
                } else {
                    this.LaunchDefault.Execute(music);
                }
            });
            this.DefaultVideoLaunch = new RelayCommand<MediaVideo>(video => {
                if (Config.VideoLaunchBehavior == LaunchBehavior.Self) {
                    this.LaunchVideoPlayback.Execute(video);
                } else {
                    this.LaunchDefault.Execute(video);
                }
            });

            //Media playback commands
            this.Stop = new RelayCommand(StopCurrentPlayback);
            this.Next = new RelayCommand(() => ChangeActiveImage(true));
            this.Previous = new RelayCommand(() => ChangeActiveImage(false));
        }

        /// <summary>
        /// Drag and drop initializer for filesystem items.
        /// </summary>
        private void FileItemDragInit(IThumbnailViewModel dragItem)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift)) {
                string[] fileList;
                if (FileSystemItems.Any(item => item.IsSelected)) {

                    if (dragItem.IsSelected) {
                        fileList = FileSystemItems
                                    .Where(item => item.IsSelected)
                                    .Select(item => item.Path)
                                    .ToArray();

                    } else {
                        ResetItemSelection();
                        dragItem.IsSelected = true;
                        fileList = new string[] { dragItem.Path };
                    }

                } else {
                    dragItem.IsSelected = true;
                    fileList = new string[] { dragItem.Path };
                }

                var dropData = new DataObject(DataFormats.FileDrop, fileList);
                DragDrop.DoDragDrop(navView, dropData, DragDropEffects.All);
            }
        }

        /// <summary>
        /// Click on an image library item during library browsing.
        /// </summary>
        private void ImageItemClick(LibraryImageViewModel image)
        {
            if (!HandleItemSelection(image)) {
                DefaultImageLaunch.Execute(image.Proxy);
            }
        }

        /// <summary>
        /// Click on a music library item during library browsing.
        /// </summary>
        private void MusicItemClick(LibraryMusicViewModel music)
        {
            if (!HandleItemSelection(music)) {
                DefaultMusicLaunch.Execute(music.Proxy);
            }
        }

        /// <summary>
        /// Click on a video library item during library browsing.
        /// </summary>
        private void VideoItemClick(LibraryVideoViewModel video)
        {
            if (!HandleItemSelection(video)) {
                DefaultVideoLaunch.Execute(video.Proxy);
            }
        }

        /// <summary>
        /// Click on an image filesystem item during file browsing.
        /// </summary>
        private void ImageFileItemClick(ImageItemViewModel image)
        {
            if (!HandleFileSystemSelection(image)) {
                DefaultImageLaunch.Execute(image.Proxy);
            }
        }

        /// <summary>
        /// Click on a music filesystem item during file browsing.
        /// </summary>
        private void MusicFileItemClick(MusicItemViewModel music)
        {
            if (!HandleFileSystemSelection(music)) {
                DefaultMusicLaunch.Execute(music.Proxy);
            }
        }

        /// <summary>
        /// Click on a video filesystem item during file browsing.
        /// </summary>
        private void VideoFileItemClick(VideoItemViewModel video)
        {
            if (!HandleFileSystemSelection(video)) {
                DefaultVideoLaunch.Execute(video.Proxy);
            }
        }

        /// <summary>
        /// Handle the selection of media items.
        /// </summary>
        /// <param name="item">Current media item</param>
        /// <returns>True when a selection was made</returns>
        private bool HandleItemSelection(ILibraryItemViewModel item) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift)) {
                ShiftItemSelection(item, false);
                return true;

            } else if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                item.IsSelected = !item.IsSelected;
                lastSelection = item;
                return true;

            } else if (Keyboard.IsKeyDown(Key.LeftShift)) {
                ShiftItemSelection(item);
                return true;

            } else {
                return false;
            }
        }

        /// <summary>
        /// Create a new selection area between
        /// the last item and the passed item.
        /// </summary>
        /// <param name="item">Target item of new selection</param>
        /// <param name="resetSelection">Remove an existing selection</param>
        private void ShiftItemSelection(ILibraryItemViewModel item, bool resetSelection = true)
        {
            if (lastSelection == null) {
                item.IsSelected = !item.IsSelected;
                lastSelection = item;

            } else {
                bool selectItems = false;

                if (resetSelection) {
                    ResetItemSelection(false);
                }

                //Select all items between lastSelection and item
                if (item.ItemID != lastSelection.ItemID) {
                    foreach (ILibraryItemViewModel curItem in LibraryItems) {

                        if (curItem.ItemID == lastSelection.ItemID || curItem.ItemID == item.ItemID) {
                            selectItems = !selectItems;
                        }

                        if (selectItems) {
                            curItem.IsSelected = true;
                        }
                    }
                }

                item.IsSelected = true;
                lastSelection.IsSelected = true;
            }
        }

        /// <summary>
        /// Handle the selection of filesystem items.
        /// </summary>
        /// <param name="item">Current media item</param>
        /// <returns>True when a selection was made</returns>
        private bool HandleFileSystemSelection(IThumbnailViewModel item)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift)) {
                ShiftFileSystemSelection(item, false);
                return true;

            } else if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                item.IsSelected = !item.IsSelected;
                lastFileSelection = item;
                return true;

            } else if (Keyboard.IsKeyDown(Key.LeftShift)) {
                ShiftFileSystemSelection(item);
                return true;

            } else {
                return false;
            }
        }

        /// <summary>
        /// Create a new selection area between
        /// the last item and the passed item.
        /// </summary>
        /// <param name="item">Target item of new selection</param>
        /// <param name="resetSelection">Remove an existing selection</param>
        private void ShiftFileSystemSelection(IThumbnailViewModel item, bool resetSelection = true)
        {
            if (lastFileSelection == null) {
                item.IsSelected = !item.IsSelected;
                lastFileSelection = item;

            } else {
                bool selectItems = false;

                if (resetSelection) {
                    ResetItemSelection(false);
                }

                //Select all items between lastFileSelection and item
                if (item != lastFileSelection) {
                    foreach (IThumbnailViewModel curItem in FileSystemItems) {

                        if (curItem == lastFileSelection || curItem == item) {
                            selectItems = !selectItems;
                        }

                        if (selectItems) {
                            curItem.IsSelected = true;
                        }
                    }
                }

                item.IsSelected = true;
                lastFileSelection.IsSelected = true;
            }
        }

        /// <summary>
        /// Set the selection of all media items to false.
        /// </summary>
        /// <param name="resetLastSelection">Enable the reset for the last selection</param>
        public void ResetItemSelection(bool resetLastSelection = true)
        {
            if (resetLastSelection) {
                lastSelection = null;
                lastFileSelection = null;
            }
            
            if (LibraryItems != null) {
                foreach (ILibraryItemViewModel item in LibraryItems) {
                    item.IsSelected = false;
                }
            }

            if (FileSystemItems != null) {
                foreach (IThumbnailViewModel item in FileSystemItems) {
                    item.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Load the media libraries from the storage provider 
        /// and convert them to navigation view models.
        /// </summary>
        private void LoadMediaLibraries()
        {
            this.NavItems[0].NavChildren = new ObservableCollection<NavItem>(
                StorageEngine.ReadMediaLibraries()
                .Select(library => new LibraryNavItem(library))
            ) {
                new DirectoryNavItem(Localize["Pictures"], "PictureIcon",
                                         Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), NavTag.None, "Pictures"),

                new DirectoryNavItem(Localize["Music"], "MusicIcon",
                                         Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), NavTag.None, "Music"),

                new DirectoryNavItem(Localize["Videos"], "VideoIcon",
                                         Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), NavTag.None, "Videos")
            };
        }

        /// <summary>
        /// Select the passed media library in the
        /// navigation tree view and open the library.
        /// </summary>
        /// <param name="library">Library to select</param>
        private void SelectMediaLibrary(MediaLibrary library)
        {
            TreeViewItem libraryRootItem = navView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
            
            //Find the view model of the library
            LibraryNavItem libraryNavItem =
                libraryRootItem.Items
                .Cast<object>()
                .Where(item => item is LibraryNavItem)
                .Cast<LibraryNavItem>()
                .First(navItem => navItem.MediaLibrary.LibraryID == library.LibraryID);

            //Get the tree view navigation item for selection
            int navIndex = libraryRootItem.Items.IndexOf(libraryNavItem);
            TreeViewItem targetItem = libraryRootItem.ItemContainerGenerator.ContainerFromIndex(navIndex) as TreeViewItem;
            targetItem.IsSelected = true;
            OpenMediaLibrary.Execute(library);
        }

        /// <summary>
        /// Launch the media library item browser. The browser
        /// will use the root media container of the library.
        /// </summary>
        /// <param name="library">Library to display</param>
        private void BrowseMediaLibrary(MediaLibrary library)
        {
            BrowseMediaContainer(library.RootContainer);
        }

        /// <summary>
        /// Show the configuration window for the passed library.
        /// Changes will be forwarded to the storage engine.
        /// </summary>
        /// <param name="library">Library to configure</param>
        private void ShowMediaLibraryConfig(MediaLibrary library)
        {
            LibraryWindow confWindow = new LibraryWindow(Localize);
            var libraryContext = confWindow.DataContext as LibraryWindowViewModel;
            libraryContext.LibraryName = library.Name;
            libraryContext.OwnerName = library.Owner;
            confWindow.ShowDialog();

            if (libraryContext.ApplySuccess) {
                //Update the media library data
                var name = libraryContext.LibraryName;
                var owner = libraryContext.OwnerName;

                StorageEngine.UpdateMediaLibrary(library.LibraryID, name, owner);
                NavItems[0].NavChildren
                    .Where(item => item is LibraryNavItem)
                    .Cast<LibraryNavItem>()
                    .First(item => item.MediaLibrary.LibraryID == library.LibraryID)
                    .DisplayName = name;

                //Update the path segment navigator
                if (LibraryBrowsing) {
                    MediaContainer oldActiveContainer = ActiveContainer;
                    ActiveContainer = null;
                    ActiveContainer = oldActiveContainer;
                }
            }
        }

        /// <summary>
        /// Show the delete dialog window for the passed library.
        /// </summary>
        /// <param name="library">Library to delete</param>
        private void RemoveMediaLibrary(MediaLibrary library)
        {
            DeleteDialog deleteDialog = new DeleteDialog(Localize);
            var deleteContext = deleteDialog.DataContext as DeleteDialogViewModel;
            deleteDialog.DeleteTextBefore = Localize["DeleteBeforeLibrary"];
            deleteDialog.DeleteTextAfter = Localize["DeleteAfterLibrary"];
            deleteContext.DeleteName = library.Name;
            deleteDialog.ShowDialog();

            if (deleteContext.DeleteConfirm) {
                //Delete the media library
                StorageEngine.DeleteMediaLibrary(library.LibraryID);

                LibraryNavItem navItem = NavItems[0].NavChildren
                    .Where(item => item is LibraryNavItem)
                    .Cast<LibraryNavItem>()
                    .First(item => item.MediaLibrary.LibraryID == library.LibraryID);
                    
                this.NavItems[0].NavChildren.Remove(navItem);

                //The active container was deleted
                if (ActiveContainer != null && ActiveContainer.Library.LibraryID == navItem.MediaLibrary.LibraryID) {
                    BrowseUpAvailable = false;
                    LibraryBrowsing = false;
                    var navSelection = navView.SelectedItem;

                    if (navSelection != null) {
                        TreeViewItem treeItem = FindTreeItem(item => item.DataContext == navSelection);

                        if (treeItem != null) {
                            treeItem.IsSelected = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a new media container in the source container.
        /// </summary>
        /// <param name="source">Container to hold the new container</param>
        private void NewMediaContainer(MediaContainer source)
        {
            TreeViewItem treeItem = FindNavItem(source);
            MediaContainer newContainer = new MediaContainer(-1, "", source, source.Library, false);
            ContainerNavItem newItem = new ContainerNavItem(newContainer) {
                NameBox = Visibility.Collapsed
            };

            //Append the new container item to the navigation
            source.Containers.Add(newContainer);
            NavItem navContainer = treeItem.DataContext as NavItem;

            if (!navContainer.IsExpanded) {
                navContainer.IsExpanded = true;
            }

            navContainer.NavChildren.Add(newItem);

            //Find the tree view item of the new container item
            TreeViewItem navItem = FindTreeItem(item => {
                return (item.DataContext is ContainerNavItem containerItem &&
                       containerItem.MediaContainer.ContainerID == -1);
            });

            navItem.ApplyTemplate();
            ContentPresenter presenter = navItem.Template.FindName("PART_Header", navItem) as ContentPresenter;

            presenter.ApplyTemplate();
            TextBox editBox = presenter.ContentTemplate.FindName("EditBox", presenter) as TextBox;

            editBox.Focus();
            editBox.Height = presenter.ActualHeight;

            //Store the tree item after setting the focus to prevent double updating
            editItem = navItem;
        }

        /// <summary>
        /// Select the passed media container in the
        /// navigation tree view and open the container.
        /// </summary>
        /// <param name="container">Container to select</param>
        private void MediaContainerSelect(MediaContainer container)
        {
            TreeViewItem treeItem = FindNavItem(container);

            if (treeItem.DataContext is ContainerNavItem navItem) {
                OpenMediaContainer.Execute(navItem.MediaContainer);

            } else if (treeItem.DataContext is LibraryNavItem libraryItem) {
                OpenMediaContainer.Execute(libraryItem.MediaLibrary.RootContainer);
            }

            treeItem.IsSelected = true;
        }

        /// <summary>
        /// Launch the media container item browser.
        /// </summary>
        /// <param name="container">Container to display</param>
        private void BrowseMediaContainer(MediaContainer container)
        {
            ActiveContainer = container;
            BrowseUpAvailable = true;
            LibraryBrowsing = true;

            LibraryItems = new ObservableCollection<ILibraryItemViewModel>(
                StorageEngine.ReadItemsFromContainer(container)
                .Where(item => item is LibraryImage || item is LibraryMusic || item is LibraryVideo)
                .Select<ILibraryItem, ILibraryItemViewModel>(item => {

                    if (item is LibraryImage imageItem) {
                        return new LibraryImageViewModel(imageItem);
                    }

                    if (item is LibraryMusic musicItem) {
                        return new LibraryMusicViewModel(musicItem);
                    }

                    if (item is LibraryVideo videoItem) {
                        return new LibraryVideoViewModel(videoItem);
                    }

                    return null;
                })
            );
        }

        /// <summary>
        /// Show the rename dialog of the passed media container.
        /// </summary>
        /// <param name="container">Container to configure</param>
        private void MediaContainerRename(MediaContainer container)
        {
            TreeViewItem treeItem = FindNavItem(container);
            ContainerNavItem navItem = (ContainerNavItem)treeItem.DataContext;
            navItem.NameBox = Visibility.Collapsed;

            //Find the edit box of the tree item
            treeItem.ApplyTemplate();
            ContentPresenter presenter = treeItem.Template.FindName("PART_Header", treeItem) as ContentPresenter;

            presenter.ApplyTemplate();
            TextBox editBox = presenter.ContentTemplate.FindName("EditBox", presenter) as TextBox;

            editBox.Focus();
            editBox.Text = navItem.MediaContainer.Name;
            editBox.Height = presenter.ActualHeight;
            editBox.SelectionStart = 0;
            editBox.SelectionLength = editBox.Text.Length;

            //Store the tree item after setting the focus to prevent double updating
            editItem = treeItem;
        }

        /// <summary>
        /// Show the delete dialog for the passed media container.
        /// </summary>
        /// <param name="container">Container to delete</param>
        private void RemoveMediaContainer(MediaContainer container)
        {
            DeleteDialog deleteDialog = new DeleteDialog(Localize);
            var deleteContext = deleteDialog.DataContext as DeleteDialogViewModel;
            deleteDialog.DeleteTextBefore = Localize["DeleteBeforeContainer"];
            deleteDialog.DeleteTextAfter = Localize["DeleteAfterContainer"];
            deleteContext.DeleteName = container.Name;
            deleteDialog.ShowDialog();

            if (deleteContext.DeleteConfirm) {
                //Delete the media container
                StorageEngine.DeleteContainer(container.ContainerID);

                TreeViewItem treeItem = FindTreeItem(item => {
                    return (item.DataContext is ContainerNavItem containerItem &&
                           containerItem.MediaContainer.ContainerID == container.Parent.ContainerID) ||
                           (item.DataContext is LibraryNavItem libraryItem &&
                           libraryItem.MediaLibrary.RootContainer.ContainerID == container.Parent.ContainerID);
                });

                NavItem parentItem = (NavItem)treeItem.DataContext;

                if (parentItem is ContainerNavItem containerParent) {
                    containerParent.MediaContainer.Containers.Remove(container);

                } else if (parentItem is LibraryNavItem libraryParent) {
                    libraryParent.MediaLibrary.RootContainer.Containers.Remove(container);
                }

                //Delete the navigation item
                ContainerNavItem navItem = parentItem.NavChildren
                    .Where(item => item is ContainerNavItem)
                    .Cast<ContainerNavItem>()
                    .First(item => item.MediaContainer.ContainerID == container.ContainerID);

                parentItem.NavChildren.Remove(navItem);

                //When the the active container is deleted try to browse up
                bool activeContainerDelete = false;
                MediaContainer curContainer = ActiveContainer;

                while (curContainer.Parent != null) {

                    if (curContainer == container) {
                        activeContainerDelete = true;
                        break;
                    }

                    curContainer = curContainer.Parent;
                }

                if (activeContainerDelete && container.Parent != null) {
                    SelectMediaContainer.Execute(container.Parent);
                }
            }
        }

        /// <summary>
        /// Open the library item import dialog. The selection can only contain
        /// files and no folders. The import container will be selected afterwards.
        /// </summary>
        /// <param name="container">Container to store the new items</param>
        private void StartMediaImport(MediaContainer container)
        {
            string filterS = GetMediaImportFilter();

            OpenFileDialog importDialog = new OpenFileDialog {
                Title = Localize["ImportDialogTitle"],
                InitialDirectory = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}",
                Filter = GetMediaImportFilter(),
                Multiselect = true
            };

            if (importDialog.ShowDialog() == true) {

                //Store the new items in the database
                foreach (string filePath in importDialog.FileNames) {
                    string name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    StorageEngine.CreateLibraryItem(name, filePath, container, DateTime.Now);
                }

                //Select the import container
                if (container.Parent == null) {
                    SelectNavigationLibrary.Execute(container.Library);
                } else {
                    SelectMediaContainer.Execute(container);
                }
            }
        }

        /// <summary>
        /// Create the library item dialog filter string with the
        /// current localization and the specific item extensions.
        /// </summary>
        /// <returns>Filter string for the media import dialog</returns>
        private string GetMediaImportFilter()
        {
            int maxAmount = 8;
            StringBuilder sb = new StringBuilder();
            string[] mediaExtensions = MediaImage.ImageExtensions
                .Concat(MediaMusic.MusicExtensions)
                .Concat(MediaVideo.VideoExtensions)
                .ToArray();

            //Media extensions
            sb.Append(Localize["MediaFiles"]);
            sb.Append(" (");
            AppendFilterExtension(mediaExtensions, sb, maxAmount);
            sb.Append(")|");
            AppendFilterExtension(mediaExtensions, sb);

            //Image extensions
            sb.Append("|");
            sb.Append(Localize["ImageFiles"]);
            sb.Append(" (");
            AppendFilterExtension(MediaImage.ImageExtensions, sb, maxAmount);
            sb.Append(")|");
            AppendFilterExtension(MediaImage.ImageExtensions, sb);

            //Audio extensions
            sb.Append("|");
            sb.Append(Localize["AudioFiles"]);
            sb.Append(" (");
            AppendFilterExtension(MediaMusic.MusicExtensions, sb, maxAmount);
            sb.Append(")|");
            AppendFilterExtension(MediaMusic.MusicExtensions, sb);

            //Video extensions
            sb.Append("|");
            sb.Append(Localize["VideoFiles"]);
            sb.Append(" (");
            AppendFilterExtension(MediaVideo.VideoExtensions, sb, maxAmount);
            sb.Append(")|");
            AppendFilterExtension(MediaVideo.VideoExtensions, sb);

            //All extensions
            sb.Append("|");
            sb.Append(Localize["AllFiles"]);
            sb.Append(" (*.*)|*.*");

            return sb.ToString();
        }

        /// <summary>
        /// Append maxAmount extensions to the string builder.
        /// </summary>
        private void AppendFilterExtension(string[] extensions, StringBuilder builder, int maxAmount = -1)
        {
            bool firstExtension = true;
            foreach (string extension in extensions) {

                if (maxAmount != -1) {
                    if (maxAmount == 0) {
                        builder.Append(";...");
                        return;
                    } else {
                        maxAmount--;
                    }
                }

                if (firstExtension) {
                    firstExtension = false;
                } else {
                    builder.Append(";");
                }

                builder.Append("*.");
                builder.Append(extension);
            }
        }

        /// <summary>
        /// Start the explorer and select the passed file item.
        /// </summary>
        /// <param name="fileItem">Path of this item will be used</param>
        private void LaunchFileExplorer(IFileObject fileItem)
        {
            Process.Start("explorer.exe", "/select, " + fileItem.Path);
        }

        /// <summary>
        /// Open the delete dialog for a single library item.
        /// </summary>
        /// <param name="item">Target item for deletion</param>
        private void RemoveLibraryItem(ILibraryItem item)
        {
            bool deleteConfirm = false;
            if (Config.DirectLibraryItemDelete) {
                deleteConfirm = true;

            } else {
                DeleteDialog deleteDialog = new DeleteDialog(Localize);
                var deleteContext = deleteDialog.DataContext as DeleteDialogViewModel;
                deleteDialog.DeleteTextBefore = Localize["DeleteBeforeItem"];
                deleteDialog.DeleteTextAfter = Localize["DeleteAfterItem"];
                deleteContext.DeleteName = item.Name;
                deleteDialog.ShowDialog();

                if (deleteContext.DeleteConfirm) {
                    deleteConfirm = true;
                }
            }

            if (deleteConfirm) {
                //Delete the library item
                ILibraryItemViewModel viewModel = LibraryItems.First(vm => vm.ItemID == item.ItemID);
                StorageEngine.DeleteLibraryItem(item.ItemID);
                LibraryItems.Remove(viewModel);

                //Empty media container info box
                if (LibraryBrowsing && LibraryItems.Count == 0) {
                    EmptyContainerInfo = Visibility.Visible;
                } else {
                    EmptyContainerInfo = Visibility.Collapsed;
                }
            }
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
        /// Find the tree view item of a media container
        /// by searching in the navigation tree view.
        /// </summary>
        /// <param name="container">ID of this container is used for matching</param>
        public TreeViewItem FindNavItem(MediaContainer container)
        {
            return FindTreeItem(item => {
                return (item.DataContext is ContainerNavItem containerItem &&
                       containerItem.MediaContainer.ContainerID == container.ContainerID) ||
                       (item.DataContext is LibraryNavItem libraryItem &&
                       libraryItem.MediaLibrary.RootContainer.ContainerID == container.ContainerID);
            });
        }

        /// <summary>
        /// Find the tree view item in the navigation 
        /// that contains a specific media container.
        /// </summary>
        /// <param name="predicate">Condition for the item to be returned</param>
        /// <returns>The item or null when no item was found</returns>
        private TreeViewItem FindTreeItem(Predicate<TreeViewItem> predicate)
        {
            return FindTreeItem(predicate, (TreeViewItem)navView.ItemContainerGenerator.ContainerFromIndex(0));
        }

        private TreeViewItem FindTreeItem(Predicate<TreeViewItem> predicate, TreeViewItem item)
        {
            if (item is TreeViewItem treeItem && predicate.Invoke(item)) {
                return item;

            } else if (item == null || !item.HasItems) {
                return null;

            } else {
                int length = item.Items.Count;

                for (int i = 0; i < length; i++) {
                    item.UpdateLayout();
                    TreeViewItem subItem = item.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                    TreeViewItem itemFound = FindTreeItem(predicate, subItem);

                    if (itemFound != null) {
                        return itemFound;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Launch the media playback for the passed video
        /// item. Running playbacks will be stopped.
        /// </summary>
        /// <param name="video">Video item to play</param>
        private void StartVideoPlayback(MediaVideo video)
        {
            if (File.Exists(video.Path)) {
                Stop.Execute(null);
                MediaInfoName = "";
                this.mediaInfoIcon.Visibility = Visibility.Hidden;
                this.ShowVideoPlayback = Visibility.Visible;

                mediaPlayer.Source = new Uri(video.Path);
                mediaTimer.Start();
                mediaPlayer.Play();

                if (Config.ShowTimelineSideLabels) {
                    ShowTimelineSideLabels = Visibility.Visible;
                } else {
                    ShowTimelineSideLabels = Visibility.Collapsed;
                }
                
                _playmode = true;
                NotifyPropertyChanged(nameof(PlayMode));
                ResetItemSelection();
            }
        }

        /// <summary>
        /// Stop the current media playback.
        /// </summary>
        private void StopCurrentPlayback()
        {
            if (CurrentPlaybackTime > 0) {
                this.ShowVideoPlayback = Visibility.Hidden;
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
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            loadThumbsInterrupt = true;
            if (thumbnailThread.IsAlive)
                thumbnailThread.Join();

            loadingQueue.Clear();

            if (LibraryBrowsing) {
                foreach (var item in LibraryItems.Where(item => item is LibraryImageViewModel)) {
                    loadingQueue.Enqueue((ViewModel)item);
                }
            } else {
                foreach (var item in FileSystemItems.Where(item => item is ImageItemViewModel)) {
                    loadingQueue.Enqueue((ViewModel)item);
                }
            }

            thumbnailThread = new Thread(LoadThumbnails);
            loadThumbsInterrupt = false;
            thumbnailThread.Start();
            LoadVideoThumbnails();
        }

        /// <summary>
        /// This method will be executed by the thumbnail loading thread.
        /// The loaded thumbnails will be stored in the related view models.
        /// </summary>
        private void LoadThumbnails()
        {
            while (!loadThumbsInterrupt && loadingQueue.Any()) {
                ViewModel viewModelItem = loadingQueue.Dequeue();

                if (viewModelItem is ImageItemViewModel item) {
                    item.LoadThumbnail(Config.ThumbnailDecodeSize);

                } else if (viewModelItem is LibraryImageViewModel LibraryItem) {
                    LibraryItem.LoadThumbnail(Config.ThumbnailDecodeSize);
                }
            }
        }

        /// <summary>
        /// Load the video thumbnails which are not loaded yet (lazily).
        /// A new video loading queue will be created by this function.
        /// </summary>
        private void LoadVideoThumbnails()
        {
            if (LibraryBrowsing) {
                vidLoadQueue = new Queue<ViewModel>(
                    LibraryItems
                    .Where(item => item is LibraryVideoViewModel)
                    .Cast<LibraryVideoViewModel>()
                    .Where(videoItem => videoItem.ThumbImage == null)
                );
            } else {
                vidLoadQueue = new Queue<ViewModel>(
                    FileSystemItems
                    .Where(item => item is VideoItemViewModel)
                    .Cast<VideoItemViewModel>()
                    .Where(videoItem => videoItem.ThumbImage == null)
                );
            }

            if (vidLoadQueue.Any()) {
                LoadNextVideoThumbnail();
            }
        }

        /// <summary>
        /// Load the next thumbnail from the video loading queue and 
        /// store it in the related view model. This function will 
        /// call itself recursively until the loading queue is empty.
        /// </summary>
        private void LoadNextVideoThumbnail()
        {
            if (vidLoadQueue.Any()) {
                ViewModel videoItem = vidLoadQueue.Dequeue();
                thumbnailPlayer = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };

                //Because the media opening process is done asynchronously by another thread, the loading
                //queue has to be emptied recursively to only use one media instance at once for saving memory
                thumbnailPlayer.MediaOpened += (sender, e) => {
                    if (thumbnailPlayer.NaturalVideoWidth != 0 && thumbnailPlayer.NaturalVideoHeight != 0) {

                        //Keep the aspect ratio of the video in the thumbnail
                        double aspectRatio = thumbnailPlayer.NaturalVideoWidth / (double)thumbnailPlayer.NaturalVideoHeight;
                        int DecodePixelHeight = (int)(Config.ThumbnailDecodeSize / aspectRatio);

                        RenderTargetBitmap renderTarget = new RenderTargetBitmap(Config.ThumbnailDecodeSize, DecodePixelHeight, 96, 96, PixelFormats.Pbgra32);
                        DrawingVisual drawingVisual = new DrawingVisual();
                        using (DrawingContext drawContext = drawingVisual.RenderOpen()) {
                            drawContext.DrawVideo(thumbnailPlayer, new Rect(0, 0, Config.ThumbnailDecodeSize, DecodePixelHeight));
                        }

                        renderTarget.Render(drawingVisual);
                        thumbnailPlayer.Close();

                        //no image freezing necessary; assignment is on the UI-thread
                        if (videoItem is VideoItemViewModel vidItem) {
                            vidItem.ThumbImage = renderTarget;
                            vidItem.IndicatorVisibility = Visibility.Visible;

                        } else if (videoItem is LibraryVideoViewModel libraryItem) {
                            libraryItem.ThumbImage = renderTarget;
                            libraryItem.IndicatorVisibility = Visibility.Visible;
                        }

                        LoadNextVideoThumbnail();
                    }
                };

                if (videoItem is VideoItemViewModel item) {
                    thumbnailPlayer.Open(new Uri(item.Path));

                } else if (videoItem is LibraryVideoViewModel libraryItem) {
                    thumbnailPlayer.Open(new Uri(libraryItem.Path));
                }

                thumbnailPlayer.Pause();
                thumbnailPlayer.Position = DefaultThumbnailTime;
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
            this.LibraryBrowsing = false;
            FileSystemItems = new ObservableCollection<IThumbnailViewModel>(FileSystemBrowser.Browse(path)
                .Where(item => item is DirectoryItem || item is MediaImage || item is MediaMusic || item is MediaVideo)
                .Select<IFileObject, IThumbnailViewModel>(item => {

                    if (item is MediaImage imageItem) {
                        return new ImageItemViewModel(imageItem);
                    }

                    if (item is MediaMusic musicItem) {
                        return new MusicItemViewModel(musicItem);
                    }

                    if (item is MediaVideo videoItem) {
                        return new VideoItemViewModel(videoItem);
                    }

                    return new DirectoryItemViewModel((DirectoryItem)item);
                })
            );
        }

        /// <summary>
        /// Show the dialog for creating a new media library.
        /// </summary>
        private void CreateLibrary()
        {
            LibraryWindow libWindow = new LibraryWindow(Localize);
            libWindow.ApplyBtn.Content = Localize["Create"];

            var libraryContext = libWindow.DataContext as LibraryWindowViewModel;
            libraryContext.OwnerName = Config.Username;
            libWindow.ShowDialog();

            if (libraryContext.ApplySuccess) {
                //Create the new media library
                var name = libraryContext.LibraryName;
                var owner = libraryContext.OwnerName;

                MediaLibrary library = StorageEngine.CreateMediaLibrary(name, owner);
                int index = NavItems[0].NavChildren.IndexOf(NavItems[0].NavChildren
                                                   .FirstOrDefault(item => item is DirectoryNavItem));

                if (index == -1) index = NavItems[0].NavChildren.Count;
                NavItems[0].NavChildren.Insert(index, new LibraryNavItem(library));
            }
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
            if (LibraryBrowsing) {
                ActiveImages = LibraryItems
                    .Select(item => item.Proxy)
                    .Where(item => item is MediaImage)
                    .Cast<MediaImage>()
                    .ToArray();

            } else {
                ActiveImages = FileSystemItems
                    .Select(item => item.Proxy)
                    .Where(item => item is MediaImage)
                    .Cast<MediaImage>()
                    .ToArray();
            }

            ResetItemSelection();
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
            if (ShowVideoPlayback == Visibility.Visible) {
                Stop.Execute(null);

            } else if (ImagePresentActive) {
                ImagePresentActive = false;
                preloadInterrupt = true;

            } else {
                Application.Current.Shutdown();
            }
        }
    }
}
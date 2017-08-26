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
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using static System.IO.Path;
using ActivFlex.Configuration;
using ActivFlex.Localization;
using ActivFlex.Navigation;
using ActivFlex.FileSystem;
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
        Queue<IThumbnailViewModel> loadingQueue = new Queue<IThumbnailViewModel>();

        /// <summary>
        /// Interrupt flag to stop the thumbnail loading thread.
        /// </summary>
        public volatile bool loadThumbsInterrupt = true;

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

        private int imageIndex = 0;

        private MediaImage[] _activeImages;
        public MediaImage[] ActiveImages {
            get => _activeImages;
            set => SetProperty(ref _activeImages, value);
        }

        private ConfigData _config;
        public ConfigData Config {
            get => _config;
            set => SetProperty(ref _config, value);
        }

        #endregion
        #region Commands

        /// <summary>
        /// Toggle the NavVisible property
        /// </summary>
        public ICommand ToggleNavVisibility { get; set; }

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

        #endregion

        /// <summary>
        /// Creates a new ViewModel for the MainArea
        /// and instantiate all fields and commands.
        /// </summary>
        public MainViewModel()
        {
            //Configuration
            if (this.Config == null) {

                //Create a default config when no config exists
                if (!ConfigProvider.ConfigExists) {
                    ConfigProvider.SaveConfig(ConfigData.DefaultConfig);
                }

                this.Config = ConfigProvider.LoadConfig();
            }

            this.Localize = new TranslateManager(Config.Language);

            //Navigation items
            this.NavVisible = true;
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
            this.BrowseFileSystem = new RelayCommand<string>(BrowseToPath);
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
                    item.LoadThumbnail(512);
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
                .Where(item => item is DirectoryItem || item is MediaImage)
                .Select<IFileObject, IThumbnailViewModel>(item => {

                    if (item is DirectoryItem directoryItem) {
                        return new DirectoryItemViewModel(directoryItem);
                    }

                    return new ImageItemViewModel((MediaImage)item);
                })
            );
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

            imageIndex = Array.IndexOf(ActiveImages, mediaImage);
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
                if (next) imageIndex++;
                else imageIndex--;

                if (imageIndex < 0) {
                    imageIndex = ActiveImages.Length - 1;

                } else if (imageIndex >= ActiveImages.Length) {
                    imageIndex = 0;
                }

                if (imageIndex >= 0 && imageIndex < ActiveImages.Length) {
                    nextImage = ActiveImages[imageIndex];
                }

                //Check if the image is still waiting for loading
                if (nextImage?.LoadState == ImageLoadState.Waiting) {
                    nextImage.LoadImage();
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
            } else {
                Application.Current.Shutdown();
            }
        }
    }
}
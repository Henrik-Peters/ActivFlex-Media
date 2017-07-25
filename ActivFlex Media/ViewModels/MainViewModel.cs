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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.IO.Path;
using ActivFlex.FileSystem;
using ActivFlex.Media;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for the main application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
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

        private ObservableCollection<IFileObject> _fileSystemItems;
        public ObservableCollection<IFileObject> FileSystemItems {
            get => _fileSystemItems;
            set => SetProperty(ref _fileSystemItems, value);
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
            //Navigation items
            this.NavVisible = true;
            this.NavItems = new ObservableCollection<NavItem>(
                new List<NavItem>(new[] { new GroupNavItem("My Computer", "MyComputerIcon", true) })
            );
            
            this.NavItems[0].NavChildren = new ObservableCollection<NavItem>(
                                FileSystemBrowser.GetLogicalDrives()
                                .Select(drive => new LogicalDriveNavItem(drive)));

            //Default Zoom
            this.Zoom = 1.0;
            this.ZoomDelta = 0.1;

            //Presentation startup
            this.ActiveImages = new MediaImage[0];
            this.ImagePresentActive = false;

            //Commands
            this.ToggleNavVisibility = new RelayCommand(() => NavVisible = !NavVisible);
            this.BrowseFileSystem = new RelayCommand<string>(path => {
                this.Path = path;
                FileSystemItems = new ObservableCollection<IFileObject>(FileSystemBrowser.Browse(path));
            });
            
            this.ResetZoom = new RelayCommand(() => Zoom = 1.0);
            this.IncreaseZoom = new RelayCommand(() => Zoom += ZoomDelta);
            this.DecreaseZoom = new RelayCommand(() => Zoom -= ZoomDelta);
            
            this.BrowseUp = new RelayCommand(() => {
                BrowseFileSystem.Execute(GetParentPath(Path));
            });

            this.ExitMode = new RelayCommand(ExitCurrentMode);
            this.NextImage = new RelayCommand(() => ChangeActiveImage(true));
            this.PreviousImage = new RelayCommand(() => ChangeActiveImage(false));
            this.LaunchPresenter = new RelayCommand<MediaImage>(LaunchImagePresenter);
            this.PresentImage = new RelayCommand<MediaImage>(PresentMediaImage);
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
                mediaImage.LoadImageSync();
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
                    nextImage.LoadImageSync();
                }

            } while (!(nextImage?.LoadState == ImageLoadState.Successful));

            PresentMediaImage(nextImage);
            return true;
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

        /// <summary>
        /// Get the parent location of a path by using
        /// string operations. When a root path is provided
        /// the unchanged root path will be returned. Root
        /// paths always end with the directory separator.
        /// </summary>
        /// <param name="path">Current path location</param>
        /// <returns>The parent location of the path</returns>
        private string GetParentPath(string path)
        {
            string parentPath = path;
            int lastSeperator = path.LastIndexOf(DirectorySeparatorChar);

            if (lastSeperator != -1) {
                parentPath = path.Substring(0, lastSeperator);

                if (!parentPath.Contains(DirectorySeparatorChar))
                    parentPath += DirectorySeparatorChar;
            }

            return parentPath;
        }
    }
}
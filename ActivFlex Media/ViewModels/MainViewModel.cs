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
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActivFlex.FileSystem;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for the MainArea.
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

        private string _lastPath;
        public string LastPath {
            get => _lastPath;
            set {
                SetProperty(ref _lastPath, value);
                NotifyPropertyChanged(nameof(BrowseBackAvailable));
            }
        }

        private string _path;
        public string Path {
            get => _path;
            set {
                LastPath = _path;
                SetProperty(ref _path, value);
            }
        }
        
        public bool BrowseBackAvailable {
            get => LastPath != null;
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
                if (BrowseBackAvailable) {
                    BrowseFileSystem.Execute(LastPath);
                }
            });
        }
    }
}
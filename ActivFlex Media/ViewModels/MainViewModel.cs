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
        /// Creates a new ViewModel for the MainArea
        /// and instantiate all fields and commands.
        /// </summary>
        public MainViewModel()
        {
            //Navigation items
            this.NavVisible = true;
            this.NavItems = new ObservableCollection<NavItem>(
                new List<NavItem>(new[] { new GroupNavItem("My Computer", "MyComputerIcon") })
            );

            this.NavItems[0].NavChildren = new ObservableCollection<NavItem>(
                                FileSystemBrowser.GetLogicalDrives()
                                .Select(drive => new LogicalDriveNavItem(drive)));
            
            //Commands
            this.ToggleNavVisibility = new RelayCommand(() => NavVisible = !NavVisible);
            this.BrowseFileSystem = new RelayCommand<string>(path => {
                FileSystemItems = new ObservableCollection<IFileObject>(FileSystemBrowser.Browse(path));
            });
        }
    }
}
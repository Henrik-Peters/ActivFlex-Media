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
using System.Collections.ObjectModel;
using ActivFlex.FileSystem;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for items in the navigation area.
    /// </summary>
    public abstract class NavItem : ViewModel
    {
        /// <summary>
        /// Text that represents the item.
        /// </summary>
        public abstract string DisplayName { get; set; }
        
        private ObservableCollection<NavItem> _navChildren = new ObservableCollection<NavItem>();
        public ObservableCollection<NavItem> NavChildren {
            get => _navChildren;
            set => SetProperty(ref _navChildren, value);
        }

        public NavItem(ObservableCollection<NavItem> Children = null)
        {
            this.NavChildren = Children ?? _navChildren;
        }
    }

    /// <summary>
    /// ViewModel implementation for a headline as nav item.
    /// </summary>
    public class GroupNavItem : NavItem
    {
        private string _displayName;

        public override string DisplayName {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public GroupNavItem(string DisplayName)
        {
            this.DisplayName = DisplayName;
        }
    }

    /// <summary>
    /// ViewModel implementation for a file system related nav item.
    /// </summary>
    public class FileSystemNavItem : NavItem
    {
        private string _displayName;
        private FileSystemItem _fileItem;

        public override string DisplayName {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public FileSystemNavItem(FileSystemItem fileItem)
        {
            this._fileItem = fileItem;
            this.DisplayName = fileItem.FullPath;
        }
    }
}

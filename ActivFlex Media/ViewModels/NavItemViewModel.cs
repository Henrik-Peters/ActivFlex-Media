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
using System.IO;
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
        private string _iconResource;

        /// <summary>
        /// Text that represents the item.
        /// </summary>
        public override string DisplayName {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Resource key identifier to load the
        /// icon from the resource dictionary.
        /// </summary>
        public string IconResource {
            get => _iconResource;
            set => SetProperty(ref _iconResource, value);
        }

        /// <summary>
        /// Create a nav item to group other items together.
        /// This should be used to represent a navigation category.
        /// </summary>
        /// <param name="DisplayName">Text to display for the group</param>
        /// <param name="IconResource">Key of the resource for the icon</param>
        public GroupNavItem(string DisplayName, string IconResource)
        {
            this.DisplayName = DisplayName;
            this.IconResource = IconResource;
        }
    }

    /// <summary>
    /// ViewModel implementation for a file system related nav item.
    /// </summary>
    public class LogicalDriveNavItem : NavItem
    {
        private string _displayName;
        private LogicalDriveItem _driveItem;

        /// <summary>
        /// Text that represents the item.
        /// </summary>
        public override string DisplayName {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Get the type of the represented drive.
        /// </summary>
        public DriveType DriveType {
            get => _driveItem.DriveType;
        }

        /// <summary>
        /// Create a nav item to represent a logical drive.
        /// This should be used as an entry point for the
        /// file browsing system (no tree children).
        /// </summary>
        /// <param name="driveItem"></param>
        public LogicalDriveNavItem(LogicalDriveItem driveItem)
        {
            this._driveItem = driveItem;
            this.DisplayName = driveItem.FullPath;
        }
    }
}

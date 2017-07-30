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
    /// This state provides additional context
    /// about the usage of navigation items.
    /// </summary>
    public enum NavTag { None, MediaLibraryRoot }

    /// <summary>
    /// ViewModel implementation for items in the navigation area.
    /// </summary>
    public abstract class NavItem : ViewModel
    {
        private bool _isExpanded;
        private ObservableCollection<NavItem> _navChildren;
        private NavTag _tag;

        /// <summary>
        /// Text that represents the item.
        /// </summary>
        public abstract string DisplayName { get; set; }

        /// <summary>
        /// Current expand state of the item.
        /// </summary>
        public bool IsExpanded {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Provides additional metainformation.
        /// </summary>
        public NavTag Tag {
            get => _tag;
            set {
                if (_tag == value)
                    return;

                _tag = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The navigation children of the item.
        /// This property is used to build up the
        /// navigation tree recursively.
        /// </summary>
        public ObservableCollection<NavItem> NavChildren {
            get => _navChildren;
            set => SetProperty(ref _navChildren, value);
        }

        /// <summary>
        /// This constructor will initialize the children
        /// collection. You may just use the default parameters.
        /// </summary>
        /// <param name="Children">The navigation children of the item</param>
        /// <param name="IsExpanded">The startup expand state of the item</param>
        protected NavItem(ObservableCollection<NavItem> Children = null, bool IsExpanded = false)
        {
            this._navChildren = new ObservableCollection<NavItem>();
            this.NavChildren = Children ?? _navChildren;
            this.IsExpanded = IsExpanded;
            this.Tag = NavTag.None;
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
        /// <param name="IsExpanded">The startup expand state of the group</param>
        /// <param name="Tag">Used for additional meta information</param>
        public GroupNavItem(string DisplayName, string IconResource, bool IsExpanded = false, NavTag Tag = NavTag.None) : base()
        {
            this.DisplayName = DisplayName;
            this.IconResource = IconResource;
            this.IsExpanded = IsExpanded;
            this.Tag = Tag;
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
        /// <param name="driveItem">The represented drive item</param>
        /// <param name="Tag">Used for additional meta information</param>
        public LogicalDriveNavItem(LogicalDriveItem driveItem, NavTag tag = NavTag.None) : base()
        {
            this._driveItem = driveItem;
            this.DisplayName = driveItem.Path;
            this.Tag = tag;
        }
    }

    /// <summary>
    /// ViewModel implementation for a directory related nav item.
    /// </summary>
    public class DirectoryNavItem : NavItem
    {
        private string _displayName;
        private string _iconResource;
        private string _path;

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
        /// Get the path of the represented directory.
        /// </summary>
        public string Path {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        /// <summary>
        /// Create a nav item to represent a directory.
        /// This should be used to point to a directory
        /// for file browsing (no tree children).
        /// </summary>
        /// <param name="displayName">Text to display in the item</param>
        /// <param name="iconResource">Key of the resource for the icon</param>
        /// <param name="path">Path to the directory</param>
        /// <param name="tag">Used for additional meta information</param>
        public DirectoryNavItem(string displayName, string iconResource, string path, NavTag tag = NavTag.None) : base()
        {
            this.DisplayName = displayName;
            this.IconResource = iconResource;
            this.Path = path;
            this.Tag = tag;
        }
    }
}
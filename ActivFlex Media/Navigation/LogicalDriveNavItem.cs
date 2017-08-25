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
using ActivFlex.FileSystem;

namespace ActivFlex.Navigation
{
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
}

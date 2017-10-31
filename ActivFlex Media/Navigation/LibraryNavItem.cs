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
using ActivFlex.Libraries;

namespace ActivFlex.Navigation
{
    /// <summary>
    /// ViewModel implementation for a media library related nav item.
    /// </summary>
    public class LibraryNavItem : NavItem
    {
        private string _displayName;
        private MediaLibrary _mediaLibrary;

        /// <summary>
        /// Text that represents the library.
        /// </summary>
        public override string DisplayName {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// The represented library of the item.
        /// </summary>
        public MediaLibrary MediaLibrary {
            get => _mediaLibrary;
            set => SetProperty(ref _mediaLibrary, value);
        }

        /// <summary>
        /// Create a nav item to represent a media library.
        /// The display text will set by the library name.
        /// </summary>
        /// <param name="mediaLibrary">The represented media library</param>
        public LibraryNavItem(MediaLibrary mediaLibrary)
        {
            this._mediaLibrary = mediaLibrary;
            this.DisplayName = mediaLibrary.Name;
        }

        /// <summary>
        /// Create a nav item to represent a media library.
        /// The display text will set by the passed name.
        /// </summary>
        /// <param name="mediaLibrary">The represented media library</param>
        /// <param name="name">Custom display text for the nav item</param>
        public LibraryNavItem(MediaLibrary mediaLibrary, string name)
        {
            this._mediaLibrary = mediaLibrary;
            this.DisplayName = name;
        }
    }
}
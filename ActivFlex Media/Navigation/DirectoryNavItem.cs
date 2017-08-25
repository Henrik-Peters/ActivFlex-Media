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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivFlex.Navigation
{
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
        /// <param name="localizeKey">Key to use for translation when the language changes</param>
        public DirectoryNavItem(string displayName, string iconResource, string path, NavTag tag = NavTag.None, string localizeKey = null) : base()
        {
            this.DisplayName = displayName;
            this.IconResource = iconResource;
            this.LocalizeKey = localizeKey;
            this.Path = path;
            this.Tag = tag;
        }
    }
}

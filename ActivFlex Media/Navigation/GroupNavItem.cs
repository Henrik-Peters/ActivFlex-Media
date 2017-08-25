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
        /// <param name="LocalizeKey">Key to use for translation when the language changes</param>
        /// <param name="IconResource">Key of the resource for the icon</param>
        /// <param name="IsExpanded">The startup expand state of the group</param>
        /// <param name="Tag">Used for additional meta information</param>
        public GroupNavItem(string DisplayName, string LocalizeKey, string IconResource,
                            bool IsExpanded = false, NavTag Tag = NavTag.None) : base()
        {
            this.DisplayName = DisplayName;
            this.LocalizeKey = LocalizeKey;
            this.IconResource = IconResource;
            this.IsExpanded = IsExpanded;
            this.Tag = Tag;
        }
    }
}

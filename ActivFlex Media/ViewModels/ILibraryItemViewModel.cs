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

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// All library item view models have to provide these properties.
    /// Changes should be forwarded to the library proxy item.
    /// </summary>
    public interface ILibraryItemViewModel
    {
        /// <summary>
        /// The represented library item.
        /// </summary>
        ILibraryItem Proxy { get; set; }

        /// <summary>
        /// Media container that stores this item.
        /// </summary>
        MediaContainer Container { get; }

        /// <summary>
        /// Unique global identifier of the item.
        /// </summary>
        int ItemID { get; }

        /// <summary>
        /// Short text to describe this item.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Absolute filesystem path of this item.
        /// </summary>
        string Path { get; }
    }
}
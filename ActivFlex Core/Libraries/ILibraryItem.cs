#region License
// ActivFlex Core - Core logic module for ActivFlex
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

namespace ActivFlex.Libraries
{
    /// <summary>
    /// Defines all operations for media items
    /// used by media libraries and containers.
    /// </summary>
    public interface ILibraryItem
    {
        /// <summary>
        /// Unique global identifier of the item.
        /// </summary>
        int ItemID { get; set; }

        /// <summary>
        /// Media container that stores this item.
        /// </summary>
        MediaContainer Container { get; }

        /// <summary>
        /// Short text to describe this item.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Absolute filesystem path of this item.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Number of opening accesses of this item.
        /// </summary>
        ulong AccessCount { get; set; }

        /// <summary>
        /// Time of adding this item to the container.
        /// </summary>
        DateTime CreationTime { get; }

        /// <summary>
        /// Time of the last opening accesses.
        /// </summary>
        DateTime LastAccessTime { get; set; }
    }
}
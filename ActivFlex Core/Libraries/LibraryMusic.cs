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
using ActivFlex.Media;

namespace ActivFlex.Libraries
{
    /// <summary>
    /// Represents a media library item that stores music data.
    /// Notice that changes will not update the media container.
    /// </summary>
    public class LibraryMusic : MediaMusic, ILibraryItem
    {
        /// <summary>
        /// Unique global identifier of the item.
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        /// Media container that stores this item.
        /// </summary>
        public MediaContainer Container { get; }

        /// <summary>
        /// Number of opening accesses of this item.
        /// </summary>
        public ulong AccessCount { get; set; }

        /// <summary>
        /// Rating of the item in stars.
        /// </summary>
        public StarRating Rating { get; set; }

        /// <summary>
        /// Time of adding this item to the container.
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Time of the last opening accesses.
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Create a new library music item for the first time. This constructor will not insert
        /// the item into the media container, it will only store a reference to that container.
        /// The access counter will be initialized with zero and the current time will be used 
        /// as the creation time of the item.
        /// </summary>
        /// <param name="itemID">Unique identifier of the item</param>
        /// <param name="name">Short description of the item</param>
        /// <param name="path">Absolute filesystem path of the item</param>
        /// <param name="container">Media container to store this item</param>
        public LibraryMusic(int itemID, string name, string path, MediaContainer container) : base(path, name)
        {
            this.ItemID = itemID;
            this.Container = container;
            this.AccessCount = 0;
            this.Rating = StarRating.NoRating;
            this.CreationTime = DateTime.Now;
        }

        /// <summary>
        /// Create a new library music instance for an existing item. This constructor should
        /// be used to load an item from a datastorage. No default values are used here.
        /// </summary>
        /// <param name="itemID">Unique identifier of the item</param>
        /// <param name="name">Short description of the item</param>
        /// <param name="path">Absolute filesystem path of the item</param>
        /// <param name="container">Media container to store this item</param>
        /// <param name="accessCount">Number of opening accesses</param>
        /// <param name="rating">Rating of the item in stars</param>
        /// <param name="creationTime">Time when the item was added to the container</param>
        /// <param name="lastAccessTime">Time of the last opening accesses</param>
        public LibraryMusic(int itemID, string name, string path, MediaContainer container,
                            ulong accessCount, StarRating rating, DateTime creationTime, DateTime lastAccessTime) : base(path, name)
        {
            this.ItemID = itemID;
            this.Container = container;
            this.AccessCount = accessCount;
            this.Rating = rating;
            this.CreationTime = creationTime;
            this.LastAccessTime = lastAccessTime;
        }
    }
}
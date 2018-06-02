#region License
// ActivFlex Storage - Media library data storage module
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
using System.Windows.Media.Imaging;
using ActivFlex.Libraries;

namespace ActivFlex.Storage
{
    /// <summary>
    /// Defines the operations that a storage engine must
    /// provide in order to be used for media library data.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// This method should be called during the construction
        /// process of a storage provider for preparation purposes.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Used to clean up all used resources before the
        /// shutdown of the application or the storage engine.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Create a new media library in the database. This will also
        /// create the root container for the library. The new library 
        /// will have the same ID as in the database.
        /// </summary>
        /// <param name="name">Display name of the library</param>
        /// <param name="owner">Owner just represented just by a name</param>
        /// <returns></returns>
        MediaLibrary CreateMediaLibrary(string name, string owner);

        /// <summary>
        /// Get all media libraries from the data storage. This will create 
        /// new library instances and also resolve all media containers.
        /// </summary>
        /// <returns>List with all stored media libraries</returns>
        List<MediaLibrary> ReadMediaLibraries();

        /// <summary>
        /// Update an existing media library with new data. The libraryID 
        /// can not be changed, it will be used to identify to the library.
        /// </summary>
        /// <param name="libraryID">Unique identifier of the library</param>
        /// <param name="name">New name of the library</param>
        /// <param name="owner">New owner of the library</param>
        void UpdateMediaLibrary(int libraryID, string name, string owner);

        /// <summary>
        /// Delete a complete media library and all related data. 
        /// The libraryID will be used to identify to correct library.
        /// </summary>
        /// <param name="libraryID">Unique identifier of the library</param>
        void DeleteMediaLibrary(int libraryID);

        /// <summary>
        /// Create a new media container in the database. To link the
        /// container to the hierarchy the parent container is required.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent">Parent container of the new container</param>
        /// <param name="library">Media library instance for the container</param>
        /// <param name="expanded">Expand state of the container in the navigation</param>
        /// <returns>The new media container instance</returns>
        MediaContainer CreateContainer(string name, MediaContainer parent, MediaLibrary library, bool expanded = false);

        /// <summary>
        /// Update an existing media container in the database.
        /// </summary>
        /// <param name="containerID">ID of the target container</param>
        /// <param name="name">New name for the container</param>
        /// <param name="parentID">ID of the new parent container</param>
        /// <param name="expanded">New expand state for the container</param>
        void UpdateContainer(int containerID, string name, int parentID, bool expanded);

        /// <summary>
        /// Update a media container only with a new expansion state.
        /// </summary>
        /// <param name="containerID">ID of the target media container</param>
        /// <param name="expanded">New expansion state for the container</param>
        void UpdateContainerExpansion(int containerID, bool expanded);

        /// <summary>
        /// Delete a media container from the database. This will also
        /// delete all children media containers and linked data.
        /// </summary>
        /// <param name="containerID">ID of the container for deletion</param>
        void DeleteContainer(int containerID);

        /// <summary>
        /// Create a new media item for a library.
        /// </summary>
        /// <param name="name">Name for displaying the item</param>
        /// <param name="path">Absolute filesystem path</param>
        /// <param name="container">Container to store the item</param>
        /// <param name="creationTime">Time of adding the item to the container</param>
        /// <param name="thumbnail">Thumbnail data of the item</param>
        ILibraryItem CreateLibraryItem(string name, string path, MediaContainer container, DateTime creationTime, BitmapFrame thumbnail);

        /// <summary>
        /// Read all items that are in a specific media container.
        /// </summary>
        /// <param name="container">Read the items of this container</param>
        /// <param name="loadThumbnails">Load the thumbnails directly</param>
        /// <param name="sortMode">Mode to be used for sorting media items</param>
        /// <param name="sortOrder">Order of the media item sorting</param>
        /// <returns>List with the library items of the container</returns>
        List<ILibraryItem> ReadItemsFromContainer(MediaContainer container, bool loadThumbnails, LibrarySortMode sortMode, LibrarySortOrder sortOrder);

        /// <summary>
        /// Update a media library item with a new name.
        /// </summary>
        /// <param name="itemID">ID of the library item</param>
        /// <param name="name">New name for the item</param>
        void UpdateLibraryItemName(int itemID, string name);

        /// <summary>
        /// Update the thumbnail data for a specific library item.
        /// </summary>
        /// <param name="itemID">ID of the library item</param>
        /// <param name="thumbnail">New thumbnail data for the item</param>
        void UpdateLibraryItemThumbnail(int itemID, BitmapFrame thumbnail);

        /// <summary>
        /// Update a media library item with a new rating.
        /// </summary>
        /// <param name="itemID">ID of the library item</param>
        /// <param name="rating">New rating for the item</param>
        void UpdateLibraryItemRating(int itemID, StarRating rating);

        /// <summary>
        /// Delete a media library item by the identifier.
        /// </summary>
        /// <param name="itemID">ID of the library item</param>
        void DeleteLibraryItem(int itemID);
    }
}
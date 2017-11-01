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
using System.Collections.Generic;
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
    }
}
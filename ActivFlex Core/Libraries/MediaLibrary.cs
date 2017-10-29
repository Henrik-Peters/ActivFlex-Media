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
namespace ActivFlex.Libraries
{
    /// <summary>
    /// Represents the metadata of a media library. The media
    /// data themselves will be stored in a media root container.
    /// </summary>
    public class MediaLibrary
    {
        /// <summary>
        /// Unique numeric identifier for the current library.
        /// </summary>
        public int LibraryID { get; private set; }

        /// <summary>
        /// The display name of the library.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Owner of the library represented just by the name.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The root location of media storage for the library.
        /// </summary>
        public MediaContainer RootContainer { get; set; }

        /// <summary>
        /// In order to create a new media library a root media container 
        /// must be created first and passed in by the constructor.
        /// </summary>
        /// <param name="libraryID">Unique numeric identifier</param>
        /// <param name="name">Display name of the library</param>
        /// <param name="owner">Owner just represented just by the name</param>
        /// <param name="rootContainer">root location of media storage</param>
        public MediaLibrary(int libraryID, string name, string owner, MediaContainer rootContainer)
        {
            this.LibraryID = libraryID;
            this.Name = name;
            this.Owner = owner;
            this.RootContainer = rootContainer;
        }
    }
}
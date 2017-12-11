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
using System.Windows.Media.Imaging;
using ActivFlex.FileSystem;

namespace ActivFlex.Media
{
    /// <summary>
    /// Abstract implementations for media objects.
    /// Media objects must have a path to their location
    /// in the filesystem. If a custom name is not provided
    /// the fileName of the path is used as name.
    /// </summary>
    public abstract class MediaObject : IFileObject
    {
        /// <summary>
        /// Text to describe the media object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Location of the media object in the filesystem.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Set the path of the media object in the filesystem
        /// and use Path.GetFileName() to create the name.
        /// </summary>
        /// <param name="path">Path to the media object</param>
        protected MediaObject(string path)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
        }

        /// <summary>
        /// Create a new media object by the 
        /// filesystem path and a custom name.
        /// </summary>
        /// <param name="path">Path to the media object</param>
        /// <param name="name">Name for the media object</param>
        protected MediaObject(string path, string name)
        {
            this.Path = path;
            this.Name = name;
        }
    }
}
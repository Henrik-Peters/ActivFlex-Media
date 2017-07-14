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

namespace ActivFlex.FileSystem
{
    /// <summary>
    /// The base class for objects in the filesystem.
    /// Every subclass has to pass the full path.
    /// </summary>
    public abstract class FileSystemItem : IFileObject
    {
        /// <summary>
        /// The complete path identifier.
        /// </summary>
        public String Path { get; private set; }

        /// <summary>
        /// The short name of the object. It does 
        /// not contain path elements or extensions.
        /// </summary>
        public abstract String Name { get; set; }

        /// <summary>
        /// Check if the object still exists.
        /// </summary>
        public abstract bool Exists { get; }

        /// <summary>
        /// Create an abstract file system object 
        /// representation by using the full path.
        /// </summary>
        /// <param name="FullPath">The complete path identifier</param>
        protected FileSystemItem(String FullPath)
        {
            this.Path = FullPath;
        }
    }
}

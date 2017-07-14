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
using System.IO;

namespace ActivFlex.FileSystem
{
    /// <summary>
    /// Provides the data of a directory. This can 
    /// represent a root-directory or sub-directory.
    /// </summary>
    public class DirectoryItem : FileSystemItem
    {
        /// <summary>
        /// Create a representation of a file system directory.
        /// The constructor will try to create a complete directory info.
        /// Examples of valid FullPaths: "C:\\folder" or "C:/folder".
        /// </summary>
        /// <param name="FullPath">The path of the directory.</param>
        public DirectoryItem(string FullPath) : base(FullPath)
        {
            try {
                DirectoryInfo directoryInfo = new DirectoryInfo(FullPath);
                this.Name = directoryInfo.Name;

            } catch {
                Name = FullPath;
            }
        }

        /// <summary>
        /// The name of the directory (includes any extensions).
        /// </summary>
        public override string Name { get; set; }
        
        /// <summary>
        /// Check if the directory still exists on the file system.
        /// </summary>
        public override bool Exists {
            get => Directory.Exists(Path);
        }
    }
}
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
    /// Provides the representation of a file on the file system.
    /// </summary>
    public class FileItem : FileSystemItem
    {
        /// <summary>
        /// Create a representation of a file.
        /// The constructor will create the name and extension by the path.
        /// Examples of valid FullPaths: "C:\\text.txt" or "C:/text.txt".
        /// </summary>
        /// <param name="FullPath">The path of the file.</param>
        public FileItem(string FullPath) : base(FullPath)
        {
            this.Name = System.IO.Path.GetFileName(FullPath);
            this.Extension = System.IO.Path.GetExtension(FullPath);
        }

        /// <summary>
        /// The extension of the file.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// The name of the file (without extension).
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// Check if the file still exists on the file system.
        /// </summary>
        public override bool Exists {
            get => File.Exists(Path);
        }
    }
}
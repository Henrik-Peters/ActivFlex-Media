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
using System.Linq;
using System.Collections.Generic;
using static System.IO.Path;
using ActivFlex.Media;

namespace ActivFlex.FileSystem
{
    /// <summary>
    /// Provides a link to the file systems to allow
    /// a basic browsing of directories and files.
    /// </summary>
    public static class FileSystemBrowser
    {
        /// <summary>
        /// Create a list of all available drives on the system.
        /// </summary>
        /// <returns>List with all logical drives on the system.</returns>
        public static List<LogicalDriveItem> GetLogicalDrives()
        {
            return Directory.GetLogicalDrives()
                .Select(drive => new LogicalDriveItem(drive))
                .ToList();
        }

        /// <summary>
        /// Create a list of all files, media objects and directories
        /// that can be found via the path (does not work recursively).
        /// If the Path is not valid or an error occurs the list will
        /// be empty. Media Objects are detected by the file extension.
        /// </summary>
        /// <param name="Path">Location in the file system to browse</param>
        /// <returns>List of all objects located at the path</returns>
        public static List<IFileObject> Browse(string Path)
        {
            List<IFileObject> list = new List<IFileObject>();

            if (Directory.Exists(Path)) {
                string[] files = new string[0];
                string[] directories = new string[0];

                try {
                    files = Directory.GetFiles(Path);
                } catch { }

                try {
                    directories = Directory.GetDirectories(Path);
                } catch { }
                
                list.AddRange(files
                    .Where(file => MediaImage.ImageExtensions.Contains(GetExtension(file)))
                    .Select(file => new MediaImage(file))
                    .ToList());

                list.AddRange(files
                    .Where(file => MediaMusic.MusicExtensions.Contains(GetExtension(file)))
                    .Select(file => new MediaMusic(file))
                    .ToList());

                list.AddRange(files
                    .Where(file => MediaVideo.VideoExtensions.Contains(GetExtension(file)))
                    .Select(file => new MediaVideo(file))
                    .ToList());

                list.AddRange(files
                    .Where(file => !MediaImage.ImageExtensions.Contains(GetExtension(file)))
                    .Where(file => !MediaMusic.MusicExtensions.Contains(GetExtension(file)))
                    .Where(file => !MediaVideo.VideoExtensions.Contains(GetExtension(file)))
                    .Select(file => new FileItem(file))
                    .ToList());

                list.AddRange(directories
                    .Select(directory => new DirectoryItem(directory))
                    .ToList());
            }

            return list
                .OrderBy(entry => entry.Name)
                .ToList();
        }

        /// <summary>
        /// Get the parent location of a path by using
        /// string operations. When a root path is provided
        /// the unchanged root path will be returned. Root
        /// paths always end with the directory separator.
        /// </summary>
        /// <param name="path">Current path location</param>
        /// <returns>The parent location of the path</returns>
        public static string GetParentPath(string path)
        {
            string parentPath = path;
            int lastSeperator = path.LastIndexOf(DirectorySeparatorChar);

            if (lastSeperator != -1) {
                parentPath = path.Substring(0, lastSeperator);

                if (!parentPath.Contains(DirectorySeparatorChar))
                    parentPath += DirectorySeparatorChar;
            }

            return parentPath;
        }

        /// <summary>
        /// Get the extension of a path without the dot and
        /// convert the extension into lower case format.
        /// </summary>
        /// <param name="Path">Path to find the extension</param>
        /// <returns>Extension in lower case format</returns>
        public static string GetExtension(string Path)
        {
            return System.IO.Path.GetExtension(Path)
                .Replace(".", "")
                .ToLower();
        }
    }
}
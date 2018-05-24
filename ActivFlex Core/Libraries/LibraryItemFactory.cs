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
using System.Linq;
using System.Windows.Media.Imaging;
using ActivFlex.Media;

namespace ActivFlex.Libraries
{
    public static class LibraryItemFactory
    {
        /// <summary>
        /// Create a new media library item. The concrete type of
        /// the item will be chosen by the path extension. This will
        /// be done only by inspecting the path variable without any
        /// filesystem interaction.
        /// </summary>
        /// <param name="itemID">Unique identifier of the item</param>
        /// <param name="name">Short description of the item</param>
        /// <param name="path">Absolute filesystem path of the item</param>
        /// <param name="container">Media container to store this item</param>
        /// <param name="creationTime">Time when the item was added to the container</param>
        /// <param name="accessCount">Number of opening accesses</param>
        /// <param name="lastAccessTime">Time of the last opening accesses</param>
        /// <param name="thumbnail">Optional thumbnail data</param>
        /// <returns>The concrete library item or null for an invalid path</returns>
        public static ILibraryItem CreateItemByExtension(int itemID, string name, string path, MediaContainer container, DateTime creationTime,
                                                         ulong accessCount = 0, DateTime lastAccessTime = default(DateTime), BitmapSource thumbnail = null)
        {
            ILibraryItem item = null;

            if (MediaImage.ImageExtensions.Contains(GetPathExtension(path))) {
                item = new LibraryImage(itemID, name, path, container, accessCount, creationTime, lastAccessTime);

            } else if (MediaMusic.MusicExtensions.Contains(GetPathExtension(path))) {
                item = new LibraryMusic(itemID, name, path, container, accessCount, creationTime, lastAccessTime);

            } else if (MediaVideo.VideoExtensions.Contains(GetPathExtension(path))) {
                item = new LibraryVideo(itemID, name, path, container, accessCount, creationTime, lastAccessTime);
            }

            return item;
        }

        public static string GetPathExtension(string path)
        {
            int lastIndex = path.LastIndexOf('.');

            if (lastIndex < 0 || lastIndex > path.Length) {
                return String.Empty;
            } else {
                return path.Substring(lastIndex + 1).ToLower();
            }
        }
    }
}
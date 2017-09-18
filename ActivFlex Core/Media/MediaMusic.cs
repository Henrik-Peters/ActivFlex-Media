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
using System.Windows.Media.Imaging;

namespace ActivFlex.Media
{
    /// <summary>
    /// This class represents a music item that can be used as
    /// a media object. Thumbnail loading is not supported.
    /// </summary>
    public class MediaMusic : MediaObject
    {
        /// <summary>
        /// All file extensions that are valid for the
        /// creation and playback of music items.
        /// </summary>
        public static readonly string[] MusicExtensions = new string[] {
            "mp3", "wav", "wma", "aac", "m4a" };

        /// <summary>
        /// Get the extension of the music item by the
        /// file path and convert it to uppercase.
        /// </summary>
        public string Extension {
            get => System.IO.Path.GetExtension(Path)
                .Replace(".", "")
                .ToUpper();
        }

        /// <summary>
        /// Create a new music item only by the filesystem path.
        /// The file name will be used to generate the name.
        /// </summary>
        /// <param name="path">Path of the music item</param>
        public MediaMusic(string path) : base(path) { }

        /// <summary>
        /// Create a new music item by the filesystem path
        /// and a custom name to display the music item.
        /// </summary>
        /// <param name="path">Path of the music file</param>
        /// <param name="name">Custom name of the music item</param>
        public MediaMusic(string path, string name) : base(path, name) { }
        
        public override BitmapImage LoadThumbnail(int DecodePixelWidth)
        {
            throw new NotSupportedException("Loading of music thumbnails is not supported!");
        }
    }
}
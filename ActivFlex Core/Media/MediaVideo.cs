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
using System;
using System.Windows.Media.Imaging;

namespace ActivFlex.Media
{
    /// <summary>
    /// This class represents a video that can be used as
    /// a media object. For the thumbnail image generation
    /// the video must be loaded.
    /// </summary>
    public class MediaVideo : MediaObject
    {
        /// <summary>
        /// All file extensions that are valid for the
        /// creation and display of a video item.
        /// </summary>
        public static readonly string[] VideoExtensions = new string[] {
            "avi", "mp4", "wmv", "webm", "mkv", "flv"};

        /// <summary>
        /// The thumbnail data of the video.
        /// </summary>
        public BitmapSource Thumbnail { get; set; }

        /// <summary>
        /// Create a new video only by the filesystem path.
        /// The file name will be used to generate the name.
        /// </summary>
        /// <param name="path">Path of the video</param>
        public MediaVideo(string path) : base(path) {}

        /// <summary>
        /// Create a new video by the filesystem path
        /// and a custom name to display the video.
        /// </summary>
        /// <param name="path">Path of the video</param>
        /// <param name="name">Custom name of the video</param>
        public MediaVideo(string path, string name) : base(path, name) {}
    }
}
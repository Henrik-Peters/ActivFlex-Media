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

namespace ActivFlex.Media
{
    /// <summary>
    /// Defines methods for abstract media objects.
    /// </summary>
    public interface IMediaObject
    {
        /// <summary>
        /// Text to describe the media object.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Location of the media object in the filesystem.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Load a thumbnail image representing the
        /// content of the media object by the path.
        /// </summary>
        /// <param name="DecodePixelWidth">Width in pixels to decode the thumbnail</param>
        /// <returns>The loaded bitmap image instance</returns>
        BitmapImage LoadThumbnail(int DecodePixelWidth);
    }
}
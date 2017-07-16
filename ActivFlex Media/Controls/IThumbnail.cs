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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ActivFlex.FileSystem;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Defines methods for abstract thumbnail controls.
    /// </summary>
    public interface IThumbnail
    {
        /// <summary>
        /// The represented object of the thumbnail control.
        /// This data may be used to generate the thumbnail
        /// or related data like the name or path of files.
        /// </summary>
        IFileObject Proxy { get; set; }

        /// <summary>
        /// Should be executed by the thumbnail control
        /// when a click event is raised by the thumbnail.
        /// </summary>
        ICommand Click { get; set; }

        /// <summary>
        /// Should be executed by the thumbnail control
        /// when a double click event is raised by the thumbnail.
        /// </summary>
        ICommand DoubleClick { get; set; }

        /// <summary>
        /// Change the content of the image inside
        /// the thumbnail control to a specific source.
        /// </summary>
        /// <param name="image">Data of the thumbnail image</param>
        void SetThumbnail(BitmapSource image);

        /// <summary>
        /// Change the rendering options of the image
        /// inside the thumbnail control.
        /// </summary>
        /// <param name="scalingMode">Image rendering mode</param>
        void SetRenderOptions(BitmapScalingMode scalingMode);
    }
}
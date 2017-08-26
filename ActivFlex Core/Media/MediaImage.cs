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
using System.IO;
using System.Windows.Media.Imaging;

namespace ActivFlex.Media
{
    /// <summary>
    /// This class represents an image that can be used as
    /// a media object. The thumbnail image will be generated
    /// by the image itself with the DecodePixelWidth.
    /// </summary>
    public class MediaImage : MediaObject
    {
        /// <summary>
        /// All file extensions that are valid for the
        /// creation and display of a media image.
        /// </summary>
        public static readonly string[] ImageExtensions = new string[] {
            "bmp", "png", "gif", "jpg", "jpeg", "jpe", "jif", "jfif", "jfi",
            "jp2", "j2k", "jpf", "jpx", "jpm", "dib", "tif", "tiff", "dds" };

        /// <summary>
        /// The current loading state of the image data.
        /// </summary>
        public ImageLoadState LoadState { get; private set; }

        /// <summary>
        /// The image data of the represented image.
        /// </summary>
        public BitmapImage Image { get; private set; }

        /// <summary>
        /// The thumbnail data of the image itself.
        /// </summary>
        public BitmapImage Thumbnail { get; private set; }
        
        /// <summary>
        /// Create a new image only by the filesystem path.
        /// The file name will be used to generate the name.
        /// </summary>
        /// <param name="path">Path of the image</param>
        public MediaImage(string path) : base(path)
        {
            this.LoadState = ImageLoadState.Waiting;
        }

        /// <summary>
        /// Create a new image by the filesystem path
        /// and a custom name to display the image.
        /// </summary>
        /// <param name="path">Path of the image</param>
        /// <param name="name">Custom name of the image</param>
        public MediaImage(string path, string name) : base(path, name)
        {
            this.LoadState = ImageLoadState.Waiting;
        }

        /// <summary>
        /// Load a thumbnail image representing the
        /// content of the media object by the path.
        /// </summary>
        /// <param name="DecodePixelWidth">Width in pixels to decode the thumbnail</param>
        /// <returns>The loaded bitmap image instance</returns>
        public override BitmapImage LoadThumbnail(int DecodePixelWidth)
        {
            if (File.Exists(Path)) {
                try {
                    this.Thumbnail = new BitmapImage();
                    this.Thumbnail.BeginInit();
                    this.Thumbnail.UriSource = new Uri(@Path);
                    this.Thumbnail.DecodePixelWidth = DecodePixelWidth;
                    this.Thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    this.Thumbnail.EndInit();

                } catch {
                    this.Thumbnail = null;
                    this.LoadState = ImageLoadState.Error;
                }
            }

            return this.Thumbnail;
        }

        /// <summary>
        /// Load the image data. The function will block
        /// the current thread until the image is loaded
        /// sucessfully or loading has failed.
        /// </summary>
        public void LoadImage()
        {
            if (!File.Exists(Path)) {
                this.LoadState = ImageLoadState.InvalidPath;
            } else {

                try {
                    this.LoadState = ImageLoadState.Loading;

                    this.Image = new BitmapImage();
                    this.Image.BeginInit();
                    this.Image.UriSource = new Uri(@Path);
                    this.Image.CacheOption = BitmapCacheOption.OnLoad;
                    this.Image.EndInit();
                    
                    this.LoadState = ImageLoadState.Successful;

                } catch {
                    this.Image = null;
                    this.LoadState = ImageLoadState.Error;
                }
            }
        }
    }
}
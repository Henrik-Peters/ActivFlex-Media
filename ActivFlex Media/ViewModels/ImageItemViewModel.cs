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
using ActivFlex.Media;
using System.Windows.Media.Imaging;
using ActivFlex.FileSystem;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for thumbnail image controls.
    /// Setters may change the inner proxy object.
    /// </summary>
    public class ImageItemViewModel : ViewModel, IThumbnailViewModel
    {
        private MediaImage _proxy;

        /// <summary>
        /// Represented image of the thumbnail.
        /// </summary>
        public IFileObject Proxy {
            get => _proxy;
            set {
                if (_proxy != value && value is MediaImage) {
                    _proxy = (MediaImage)value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the displayed name
        /// of the represented image.
        /// </summary>
        public string Name {
            get => Proxy.Name;
            set {
                if (Proxy.Name != value) {
                    Proxy.Name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Get the file system path of the image.
        /// </summary>
        public string Path {
            get => Proxy.Path;
        }

        /// <summary>
        /// True when the item is selected in a view.
        /// </summary>
        public bool IsSelected {
            get => _proxy.IsSelected;
            set {
                if (_proxy.IsSelected != value) {
                    _proxy.IsSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The thumbnail image of the represented image.
        /// Will be null when the thumbnail is not loaded.
        /// </summary>
        public BitmapSource ThumbImage {
            get => _proxy.Thumbnail;
        }

        /// <summary>
        /// Load the thumbnail of the represented image.
        /// This operation will block the current thread.
        /// The loaded thumbnail will be stored in ThumbImage.
        /// </summary>
        /// <param name="DecodePixelWidth"></param>
        public void LoadThumbnail(int DecodePixelWidth)
        {
            _proxy.LoadThumbnail(DecodePixelWidth);
            NotifyPropertyChanged(nameof(ThumbImage));
        }

        /// <summary>
        /// Create a new view model for a thumbnail image.
        /// </summary>
        /// <param name="Proxy">The represented image</param>
        public ImageItemViewModel(MediaImage Proxy)
        {
            this.Proxy = Proxy;
        }
    }
}

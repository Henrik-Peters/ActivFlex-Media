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
using System.Windows;
using System.Windows.Media.Imaging;
using ActivFlex.Libraries;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for library video thumbnail controls.
    /// </summary>
    public class LibraryVideoViewModel : ViewModel, ILibraryItemViewModel
    {
        private LibraryVideo _proxy;

        /// <summary>
        /// The represented library item.
        /// </summary>
        public ILibraryItem Proxy {
            get => _proxy;
            set {
                if (_proxy != value && value is LibraryVideo) {
                    _proxy = (LibraryVideo)value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The thumbnail image of the represented video.
        /// Will be null when the thumbnail is not loaded.
        /// </summary>
        public BitmapSource ThumbImage {
            get => _proxy.Thumbnail;
            set {
                _proxy.Thumbnail = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _indicatorVisibility = Visibility.Collapsed;
        public Visibility IndicatorVisibility {
            get => _indicatorVisibility;
            set => SetProperty(ref _indicatorVisibility, value);
        }

        /// <summary>
        /// Media container that stores the item.
        /// </summary>
        public MediaContainer Container => _proxy.Container;

        /// <summary>
        /// Unique global identifier of the item.
        /// </summary>
        public int ItemID => _proxy.ItemID;

        /// <summary>
        /// Short text to describe the item.
        /// </summary>
        public string Name {
            get => _proxy.Name;
            set {
                if (_proxy.Name != value) {
                    _proxy.Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Absolute filesystem path of the item.
        /// </summary>
        public string Path => _proxy.Path;

        /// <summary>
        /// Create a new view model for a library video item.
        /// </summary>
        /// <param name="proxy">The represented video music</param>
        public LibraryVideoViewModel(LibraryVideo proxy)
        {
            this._proxy = proxy;
        }
    }
}
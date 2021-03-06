﻿#region License
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
using ActivFlex.FileSystem;
using ActivFlex.Media;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for thumbnail video controls.
    /// Setters may change the inner proxy object.
    /// </summary>
    public class VideoItemViewModel : ViewModel, IThumbnailViewModel
    {
        private MediaVideo _proxy;

        /// <summary>
        /// Represented video of the thumbnail.
        /// </summary>
        public IFileObject Proxy {
            get => _proxy;
            set {
                if (_proxy != value && value is MediaVideo) {
                    _proxy = (MediaVideo)value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the displayed name
        /// of the represented video.
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
        /// Get the file system path of the video.
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
        /// The thumbnail image of the represented video.
        /// Will be null when the thumbnail is not loaded.
        /// </summary>
        public BitmapSource ThumbImage {
            get => _proxy.Thumbnail;
            set {
                _proxy.Thumbnail = value;
                NotifyPropertyChanged(nameof(ThumbImage));
            }
        }

        private Visibility _indicatorVisibility = Visibility.Collapsed;
        public Visibility IndicatorVisibility {
            get => _indicatorVisibility;
            set => SetProperty(ref _indicatorVisibility, value);
        }

        /// <summary>
        /// Create a new view model for a thumbnail video item.
        /// </summary>
        /// <param name="Proxy">The represented video</param>
        public VideoItemViewModel(MediaVideo Proxy)
        {
            this.Proxy = Proxy;
        }
    }
}
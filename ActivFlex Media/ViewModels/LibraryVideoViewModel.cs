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

                    //Video indicator for cached thumbnails
                    if (_proxy.Thumbnail != null) {
                        IndicatorVisibility = Visibility.Visible;
                    }

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
                //Video indicator
                if (value != null) {
                    IndicatorVisibility = Visibility.Visible;
                }

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
        /// Checks or sets the rating for one star.
        /// </summary>
        public bool RatingOneStar {
            get => _proxy.Rating == StarRating.OneStar;
            set {
                if (value && _proxy.Rating != StarRating.OneStar) {
                    _proxy.Rating = StarRating.OneStar;
                    MainViewModel.StorageEngine.UpdateLibraryItemRating(ItemID, StarRating.OneStar);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Checks or sets the rating for two stars.
        /// </summary>
        public bool RatingTwoStars {
            get => _proxy.Rating == StarRating.TwoStars;
            set {
                if (value && _proxy.Rating != StarRating.TwoStars) {
                    _proxy.Rating = StarRating.TwoStars;
                    MainViewModel.StorageEngine.UpdateLibraryItemRating(ItemID, StarRating.TwoStars);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Checks or sets the rating for three stars.
        /// </summary>
        public bool RatingThreeStars {
            get => _proxy.Rating == StarRating.ThreeStars;
            set {
                if (value && _proxy.Rating != StarRating.ThreeStars) {
                    _proxy.Rating = StarRating.ThreeStars;
                    MainViewModel.StorageEngine.UpdateLibraryItemRating(ItemID, StarRating.ThreeStars);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Checks or sets the rating for four stars.
        /// </summary>
        public bool RatingFourStars {
            get => _proxy.Rating == StarRating.FourStars;
            set {
                if (value && _proxy.Rating != StarRating.FourStars) {
                    _proxy.Rating = StarRating.FourStars;
                    MainViewModel.StorageEngine.UpdateLibraryItemRating(ItemID, StarRating.FourStars);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Checks or sets the rating for five stars.
        /// </summary>
        public bool RatingFiveStars {
            get => _proxy.Rating == StarRating.FiveStars;
            set {
                if (value && _proxy.Rating != StarRating.FiveStars) {
                    _proxy.Rating = StarRating.FiveStars;
                    MainViewModel.StorageEngine.UpdateLibraryItemRating(ItemID, StarRating.FiveStars);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Checks or sets the rating for no rating.
        /// </summary>
        public bool RatingNotRated {
            get => _proxy.Rating == StarRating.NoRating;
            set {
                if (value && _proxy.Rating != StarRating.NoRating) {
                    _proxy.Rating = StarRating.NoRating;
                    MainViewModel.StorageEngine.UpdateLibraryItemRating(ItemID, StarRating.NoRating);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Create a new view model for a library video item.
        /// </summary>
        /// <param name="proxy">The represented video music</param>
        public LibraryVideoViewModel(LibraryVideo proxy)
        {
            this.Proxy = proxy;
        }
    }
}
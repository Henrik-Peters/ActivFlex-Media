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
using ActivFlex.FileSystem;
using ActivFlex.Media;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for thumbnail music controls.
    /// Setters may change the inner proxy object.
    /// </summary>
    public class MusicItemViewModel : ViewModel, IThumbnailViewModel
    {
        private MediaMusic _proxy;

        /// <summary>
        /// Represented music of the thumbnail.
        /// </summary>
        public IFileObject Proxy {
            get => _proxy;
            set {
                if (_proxy != value && value is MediaMusic) {
                    _proxy = (MediaMusic)value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get or set the displayed name
        /// of the represented music item.
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
        /// Get the file system path of the music item.
        /// </summary>
        public string Path {
            get => Proxy.Path;
        }

        /// <summary>
        /// Get the current extension of the music item.
        /// </summary>
        public string Extension {
            get => _proxy.Extension;
        }

        /// <summary>
        /// Create a new view model for a thumbnail music item.
        /// </summary>
        /// <param name="Proxy">The represented music item</param>
        public MusicItemViewModel(MediaMusic Proxy)
        {
            this.Proxy = Proxy;
        }
    }
}
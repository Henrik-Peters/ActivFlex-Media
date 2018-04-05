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
using ActivFlex.Libraries;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for library container thumbnail controls.
    /// </summary>
    public class LibraryContainerViewModel : ViewModel, ILibraryItemViewModel
    {
        private MediaContainer _proxyContainer;

        /// <summary>
        /// The represented media container.
        /// </summary>
        public MediaContainer ProxyContainer {
            get => _proxyContainer;
            set {
                if (_proxyContainer != value) {
                    _proxyContainer = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The proxy item is not used.
        /// </summary>
        public ILibraryItem Proxy {
            get => null;
            set { }
        }

        /// <summary>
        /// Media container that stores the container (parent container).
        /// </summary>
        public MediaContainer Container => _proxyContainer.Parent;

        /// <summary>
        /// Unique global identifier of the container.
        /// </summary>
        public int ItemID => _proxyContainer.ContainerID;

        /// <summary>
        /// Short text to describe the container.
        /// </summary>
        public string Name {
            get => _proxyContainer.Name;
            set {
                if (_proxyContainer.Name != value) {
                    _proxyContainer.Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Selection is not used for media containers.
        /// </summary>
        public bool IsSelected {
            get => false;
            set { }
        }

        /// <summary>
        /// The path field is not used for containers.
        /// </summary>
        public string Path => String.Empty;

        /// <summary>
        /// Create a new view model for a library container item.
        /// </summary>
        /// <param name="proxyContainer">Represented media container</param>
        public LibraryContainerViewModel(MediaContainer proxyContainer)
        {
            this.ProxyContainer = proxyContainer;
        }
    }
}

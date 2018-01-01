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
using System.Collections.ObjectModel;
using ActivFlex.ViewModels;
using ActivFlex.Libraries;

namespace ActivFlex.Navigation
{
    /// <summary>
    /// ViewModel implementation for a media container navigation item.
    /// </summary>
    public class ContainerNavItem : NavItem
    {
        private MediaContainer _mediaContainer;

        /// <summary>
        /// Text that represents the container.
        /// </summary>
        public override string DisplayName {
            get => _mediaContainer.Name;
            set {
                _mediaContainer.Name = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Current expand state of the item.
        /// </summary>
        public override bool IsExpanded {
            get => _mediaContainer.Expanded;
            set {
                if (_mediaContainer != null && _mediaContainer.Expanded != value) {
                    _mediaContainer.Expanded = value;
                    NotifyPropertyChanged();

                    //Save the current expansion state
                    if (MainViewModel.Config.RestoreNavExpansions) {
                        MainViewModel.StorageEngine.UpdateContainerExpansion(_mediaContainer.ContainerID, value);
                    }
                }
            }
        }

        /// <summary>
        /// The represented media container of the item.
        /// </summary>
        public MediaContainer MediaContainer {
            get => _mediaContainer;
            set => SetProperty(ref _mediaContainer, value);
        }

        /// <summary>
        /// Create a nav item to represent a media container.
        /// The display text will set by the container name.
        /// </summary>
        /// <param name="container">The represented media container</param>
        public ContainerNavItem(MediaContainer container) : base(null, false, true)
        {
            this._mediaContainer = container;
            this.DisplayName = container.Name;
            this.Tag = NavTag.MediaContainer;
            UpdateContainers();

            if (!MainViewModel.Config.RestoreNavExpansions) {
                this.IsExpanded = false;
            }
        }

        /// <summary>
        /// Update the navigation children collection with the
        /// current media containers stored in this container.
        /// </summary>
        public void UpdateContainers()
        {
            this.NavChildren = new ObservableCollection<NavItem>();

            foreach (MediaContainer container in _mediaContainer.Containers) {
                this.NavChildren.Add(new ContainerNavItem(container));
            }
        }
    }
}
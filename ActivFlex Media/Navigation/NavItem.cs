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
using System.Collections.ObjectModel;
using ActivFlex.ViewModels;

namespace ActivFlex.Navigation
{
    /// <summary>
    /// This state provides additional context
    /// about the usage of navigation items.
    /// </summary>
    public enum NavTag { None, MediaLibraryRoot, MediaLibrary, MediaContainer }

    /// <summary>
    /// ViewModel implementation for items in the navigation area.
    /// </summary>
    public abstract class NavItem : ViewModel
    {
        private bool _isExpanded;
        private string _localizeKey;
        private ObservableCollection<NavItem> _navChildren;
        private Visibility _editBox;
        private Visibility _nameBox;
        private NavTag _tag;

        /// <summary>
        /// Text that represents the item.
        /// </summary>
        public abstract string DisplayName { get; set; }

        /// <summary>
        /// This key is used to translate the display name
        /// when the translate manager has changed. It can
        /// be empty when no localization should be done.
        /// </summary>
        public string LocalizeKey {
            get => _localizeKey;
            set => SetProperty(ref _localizeKey, value);
        }

        /// <summary>
        /// Current expand state of the item.
        /// </summary>
        public virtual bool IsExpanded {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// The visibility of the name editing box.
        /// </summary>
        public virtual Visibility EditBox {
            get => _editBox;
            set => SetProperty(ref _editBox, value);
        }

        /// <summary>
        /// The visibility of the normal name box.
        /// </summary>
        public virtual Visibility NameBox {
            get => _nameBox;
            set => SetProperty(ref _nameBox, value);
        }

        /// <summary>
        /// Provides additional metainformation.
        /// </summary>
        public NavTag Tag {
            get => _tag;
            set {
                if (_tag == value)
                    return;

                _tag = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The navigation children of the item.
        /// This property is used to build up the
        /// navigation tree recursively.
        /// </summary>
        public ObservableCollection<NavItem> NavChildren {
            get => _navChildren;
            set => SetProperty(ref _navChildren, value);
        }

        /// <summary>
        /// This constructor will initialize the children
        /// collection. You may just use the default parameters.
        /// </summary>
        /// <param name="Children">The navigation children of the item</param>
        /// <param name="IsExpanded">The startup expand state of the item</param>
        protected NavItem(ObservableCollection<NavItem> Children = null, bool IsExpanded = false)
        {
            this._navChildren = new ObservableCollection<NavItem>();
            this.NavChildren = Children ?? _navChildren;
            this.IsExpanded = IsExpanded;
            this.Tag = NavTag.None;
            this._editBox = Visibility.Collapsed;
            this._nameBox = Visibility.Visible;
        }
    }
}
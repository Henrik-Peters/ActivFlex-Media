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
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ActivFlex.FileSystem;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Interaktionslogik für MusicThumbnail.xaml
    /// </summary>
    public partial class MusicThumbnail : UserControl
    {
        #region DependencyProperties

        /// <summary>
        /// Identifies the ExtensionProperty dependency property
        /// </summary>
        public static readonly DependencyProperty ExtensionProperty = DependencyProperty.Register(
            "Extension", typeof(string), typeof(MusicThumbnail), new PropertyMetadata(String.Empty));

        /// <summary>
        /// Gets or sets the extension of the music display.
        /// </summary>
        public string Extension {
            get => (string)this.GetValue(ExtensionProperty);
            set => this.SetValue(ExtensionProperty, value);
        }

        /// <summary>
        /// Identifies the ClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            "Click", typeof(ICommand), typeof(MusicThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command to be execute
        /// when the user clicks the thumbnail image
        /// </summary>
        public ICommand Click {
            get => (ICommand)this.GetValue(ClickCommandProperty);
            set => this.SetValue(ClickCommandProperty, value);
        }

        /// <summary>
        /// Identifies the DoubleClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
            "DoubleClick", typeof(ICommand), typeof(MusicThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command to be execute
        /// when the user double clicks the thumbnail
        /// </summary>
        public ICommand DoubleClick {
            get => (ICommand)this.GetValue(DoubleClickCommandProperty);
            set => this.SetValue(DoubleClickCommandProperty, value);
        }

        /// <summary>
        /// Identifies the IsSelectedProperty dependency property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(MusicThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the selected value of the control.
        /// The thumbnail will be highlighted when selected.
        /// </summary>
        public bool IsSelected {
            get => (bool)this.GetValue(IsSelectedProperty);
            set {
                if (CanSelect)
                    this.SetValue(IsSelectedProperty, value);
            }
        }

        /// <summary>
        /// Identifies the CanSelect dependency property
        /// </summary>
        public static readonly DependencyProperty CanSelectProperty = DependencyProperty.Register(
            "CanSelect", typeof(bool), typeof(MusicThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Enables or disables the select ability. When set
        /// to false on an active selection, the selection
        /// will be lost (IsSelected will be set to false).
        /// </summary>
        public bool CanSelect {
            get => (bool)this.GetValue(CanSelectProperty);
            set {
                if (!value && IsSelected) IsSelected = false;
                this.SetValue(CanSelectProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TextProperty dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(MusicThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the content of the text label.
        /// </summary>
        public string Text {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        /// <summary>
        /// Identifies the ProxyProperty dependency property
        /// </summary>
        public static readonly DependencyProperty ProxyProperty = DependencyProperty.Register(
            "Proxy", typeof(IFileObject), typeof(MusicThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the represented object for this
        /// thumbnail control. The proxy object should never
        /// be empty. The proxy will be used in the commands.
        /// </summary>
        public IFileObject Proxy {
            get => (IFileObject)this.GetValue(ProxyProperty);
            set => this.SetValue(ProxyProperty, value);
        }

        #endregion

        public MusicThumbnail()
        {
            InitializeComponent();
        }

        private void Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            if (CanSelect) {
                IsSelected = !IsSelected;
            }

            Click?.Execute(Proxy);
        }

        private void Thumbnail_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClick?.Execute(Proxy);
            e.Handled = true;
        }
    }
}

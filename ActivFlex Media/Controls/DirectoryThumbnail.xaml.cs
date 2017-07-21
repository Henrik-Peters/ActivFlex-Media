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
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using ActivFlex.FileSystem;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Viewlogic for DirectoryThumbnail.xaml
    /// </summary>
    public partial class DirectoryThumbnail : UserControl
    {
        #region DependencyProperties

        /// <summary>
        /// Identifies the ClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            "Click", typeof(ICommand), typeof(DirectoryThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the DoubleClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
            "DoubleClick", typeof(ICommand), typeof(DirectoryThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the IsSelectedProperty dependency property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(DirectoryThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the CanSelect dependency property
        /// </summary>
        public static readonly DependencyProperty CanSelectProperty = DependencyProperty.Register(
            "CanSelect", typeof(bool), typeof(DirectoryThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the TextProperty dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(DirectoryThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the ProxyProperty dependency property
        /// </summary>
        public static readonly DependencyProperty ProxyProperty = DependencyProperty.Register(
            "Proxy", typeof(IFileObject), typeof(DirectoryThumbnail), new PropertyMetadata(ProxyProperty_PropertyChanged));

        /// <summary>
        /// Gets or sets the command to be execute
        /// when the user clicks the thumbnail image
        /// </summary>
        [Bindable(true)]
        public ICommand Click {
            get => (ICommand)this.GetValue(ClickCommandProperty);
            set => this.SetValue(ClickCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to be execute
        /// when the user double clicks the thumbnail
        /// </summary>
        [Bindable(true)]
        public ICommand DoubleClick {
            get => (ICommand)this.GetValue(DoubleClickCommandProperty);
            set => this.SetValue(DoubleClickCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected value of the control.
        /// The thumbnail will be highlighted when selected.
        /// </summary>
        [Bindable(true)]
        public bool IsSelected {
            get => (bool)this.GetValue(IsSelectedProperty);
            set {
                if (CanSelect)
                    this.SetValue(IsSelectedProperty, value);
            }
        }

        /// <summary>
        /// Enables or disables the select ability. When set
        /// to false on an active selection, the selection
        /// will be lost (IsSelected will be set to false).
        /// </summary>
        [Bindable(true)]
        public bool CanSelect {
            get => (bool)this.GetValue(CanSelectProperty);
            set {
                if (!value && IsSelected) IsSelected = false;
                this.SetValue(CanSelectProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the content of the text label.
        /// </summary>
        [Bindable(true)]
        public string Text {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        /// <summary>
        /// Gets or sets the represented object for this
        /// thumbnail control. The proxy object should never
        /// be empty. The proxy will be used in the commands.
        /// </summary>
        [Bindable(true)]
        public IFileObject Proxy {
            get => (IFileObject)this.GetValue(ProxyProperty);
            set => this.SetValue(ProxyProperty, value);
        }

        #endregion

        public DirectoryThumbnail()
        {
            InitializeComponent();
        }

        private static void ProxyProperty_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var vm = (DirectoryThumbnail)obj;
            vm.Text = vm.Proxy.Name;
        }

        private void Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            if (CanSelect) {
                IsSelected = !IsSelected;
            }

            Click?.Execute(Proxy?.Path);
        }

        private void Thumbnail_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClick?.Execute(Proxy?.Path);
        }
    }
}
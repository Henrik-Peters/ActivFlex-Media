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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Viewlogic for IconButton.xaml
    /// </summary>
    public partial class IconButton : UserControl
    {
        public IconButton()
        {
            InitializeComponent();
            this.controlBtn.Click += (s, e) => 
            {
                Click?.Invoke(this, e);
                Command?.Execute(this);
            };
        }

        /// <summary>
        /// Will be fired when the inner control button is clicked
        /// </summary>
        [Browsable(true)]
        public event EventHandler Click;
        

        #region DependencyProperties

        /// <summary>
        /// Identifies the ContentDefault dependency property
        /// </summary>
        public static readonly DependencyProperty ContentDefaultProperty = DependencyProperty.Register(
            "ContentDefault", typeof(object), typeof(IconButton), new FrameworkPropertyMetadata(ContentDefaultPropertyChanged));

        /// <summary>
        /// Identifies the ContentHover dependency property
        /// </summary>
        public static readonly DependencyProperty ContentHoverProperty = DependencyProperty.Register(
            "ContentHover", typeof(object), typeof(IconButton), new FrameworkPropertyMetadata(ContentHoverPropertyChanged));

        /// <summary>
        /// Identifies the ContentPressed dependency property
        /// </summary>
        public static readonly DependencyProperty ContentPressedProperty = DependencyProperty.Register(
            "ContentPressed", typeof(object), typeof(IconButton), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the ContentDisabled dependency property
        /// </summary>
        public static readonly DependencyProperty ContentDisabledProperty = DependencyProperty.Register(
            "ContentDisabled", typeof(object), typeof(IconButton), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the CommandProperty dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(IconButton), new FrameworkPropertyMetadata(null));
        

        /// <summary>
        /// Gets or sets the content to be displayed
        /// on the default state (also used as fallback)
        /// </summary>
        [Bindable(true)]
        public object ContentDefault {
            get => this.GetValue(ContentDefaultProperty);
            set => this.SetValue(ContentDefaultProperty, value);
        }

        /// <summary>
        /// Gets or sets the hover state content
        /// </summary>
        [Bindable(true)]
        public object ContentHover {
            get => this.GetValue(ContentHoverProperty);
            set => this.SetValue(ContentHoverProperty, value);
        }   

        /// <summary>
        /// Gets or sets the pressed state content
        /// </summary>
        [Bindable(true)]
        public object ContentPressed {
            get => this.GetValue(ContentPressedProperty);
            set => this.SetValue(ContentPressedProperty, value);
        }

        /// <summary>
        /// Gets or sets the disabled state content
        /// </summary>
        [Bindable(true)]
        public object ContentDisabled {
            get => this.GetValue(ContentDisabledProperty);
            set => this.SetValue(ContentDisabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the command executed on a click
        /// </summary>
        [Bindable(true)]
        public ICommand Command {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        private static void ContentDefaultPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Set the default values to the default content property
            var viewModel = (IconButton)d;

            if (viewModel.ContentHover == null) {
                viewModel.ContentHover = viewModel.ContentDefault;
            }

            if (viewModel.ContentPressed == null) {
                viewModel.ContentPressed = viewModel.ContentDefault;
            }

            if (viewModel.ContentDisabled == null) {
                viewModel.ContentDisabled = viewModel.ContentDefault;
            }
        }

        private static void ContentHoverPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Set the pressed default value to the hover content property (if available as non default-value)
            var viewModel = (IconButton)d;

            if (viewModel.ContentPressed == null || viewModel.ContentPressed == viewModel.ContentDefault) {
                viewModel.ContentPressed = viewModel.ContentHover ?? viewModel.ContentDefault;
            }
        }

        #endregion
    }
}
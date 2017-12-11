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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using ActivFlex.FileSystem;
using ActivFlex.Media;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Viewlogic for ImageThumbnail.xaml
    /// </summary>
    public partial class ImageThumbnail : UserControl
    {
        #region DependencyProperties

        /// <summary>
        /// Identifies the ClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            "Click", typeof(ICommand), typeof(ImageThumbnail), new PropertyMetadata(null));

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
        /// Identifies the DoubleClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
            "DoubleClick", typeof(ICommand), typeof(ImageThumbnail), new PropertyMetadata(null));

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
        /// Identifies the IsSelectedProperty dependency property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(ImageThumbnail), new PropertyMetadata(null));

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
        /// Identifies the CanSelect dependency property
        /// </summary>
        public static readonly DependencyProperty CanSelectProperty = DependencyProperty.Register(
            "CanSelect", typeof(bool), typeof(ImageThumbnail), new PropertyMetadata(null));

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
        /// Identifies the TextProperty dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(ImageThumbnail), new PropertyMetadata(null));
        
        /// <summary>
        /// Gets or sets the content of the text label.
        /// </summary>
        [Bindable(true)]
        public string Text {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        /// <summary>
        /// Identifies the ProxyProperty dependency property
        /// </summary>
        public static readonly DependencyProperty ProxyProperty = DependencyProperty.Register(
            "Proxy", typeof(IFileObject), typeof(ImageThumbnail), new PropertyMetadata(ProxyProperty_PropertyChanged));

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

        /// <summary>
        /// Identifies the DefaultContent dependency property
        /// </summary>
        public static readonly DependencyProperty DefaultContentProperty = DependencyProperty.Register(
            "DefaultContent", typeof(object), typeof(ImageThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the content resource to be displayed
        /// when no image is set (default content of the control).
        /// </summary>
        public object DefaultContent {
            get => this.GetValue(DefaultContentProperty);
            set => this.SetValue(DefaultContentProperty, value);
        }

        /// <summary>
        /// Identifies the VideoIndicatorProperty dependency property
        /// </summary>
        public static readonly DependencyProperty VideoIndicatorProperty = DependencyProperty.Register(
            "VideoIndicator", typeof(Visibility), typeof(ImageThumbnail), new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets or sets the visibility of the video indicator.
        /// </summary>
        public Visibility VideoIndicator {
            get => (Visibility)this.GetValue(VideoIndicatorProperty);
            set => this.SetValue(VideoIndicatorProperty, value);
        }

        /// <summary>
        /// Identifies the ImageProperty dependency property
        /// </summary>
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof(BitmapSource), typeof(ImageThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the image to the displayed as thumbnail.
        /// The default content will be hidden after setting an image.
        /// </summary>
        public BitmapSource Image {
            get => (BitmapSource)this.GetValue(ImageProperty);
            set => this.SetValue(ImageProperty, value);
        }

        #endregion

        /// <summary>
        /// Create a new control to display a simple thumbnail
        /// image. The image itself will not be loaded.
        /// </summary>
        public ImageThumbnail()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create a new control to display a simple thumbnail
        /// image. The image itself will not be set by the
        /// constructor. Use <see cref="SetThumbnail(BitmapSource)"/>
        /// to set the thumbnail image data.
        /// </summary>
        /// <param name="proxy">Represented object of the thumbnail</param>
        /// <param name="CanSelect">Enables or disables the select ability</param>
        public ImageThumbnail(IFileObject proxy, bool CanSelect = false)
        {
            InitializeComponent();
            this.CanSelect = CanSelect;
            this.Proxy = proxy;

            Text = proxy != null ? proxy.Name : "";
        }

        private static void ProxyProperty_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageThumbnail)obj;
            control.Text = control.Proxy.Name;
        }

        /// <summary>
        /// Change the rendering options of the image control.
        /// </summary>
        /// <param name="scalingMode">Image rendering mode</param>
        public void SetRenderOptions(BitmapScalingMode scalingMode)
        {
            RenderOptions.SetBitmapScalingMode(GetImageControl(), scalingMode);
        }

        /// <summary>
        /// Change the content of the image inside
        /// the thumbnail control to a specific source.
        /// </summary>
        /// <param name="image">Data of the thumbnail image</param>
        public void SetThumbnail(BitmapSource image)
        {
            this.Image = image;
        }

        #region Helper Functions

        private Image GetImageControl()
        {
            btnThumbnail.ApplyTemplate();
            return (Image)btnThumbnail.Template.FindName("imgDisplay", btnThumbnail);
        }

        private void Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            Click?.Execute(Proxy);
        }

        private void Thumbnail_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClick?.Execute(Proxy);
            e.Handled = true;
        }

        #endregion
    }
}
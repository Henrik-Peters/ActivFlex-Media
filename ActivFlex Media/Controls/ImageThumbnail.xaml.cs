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
    public partial class ImageThumbnail : UserControl, IThumbnail
    {
        #region DependencyProperties

        /// <summary>
        /// Identifies the ClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            "Click", typeof(ICommand), typeof(SimpleThumbnail), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the DoubleClickCommand dependency property
        /// </summary>
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
            "DoubleClick", typeof(ICommand), typeof(SimpleThumbnail), new PropertyMetadata(null));

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

        #endregion

        /// <summary>
        /// Gets or sets the represented object for this
        /// thumbnail control. The proxy object should never
        /// be empty and will be set by the constructor.
        /// The proxy will be passed as a command parameter.
        /// </summary>
        public IFileObject Proxy { get; set; }

        /// <summary>
        /// Create a new control to display a simple thumbnail
        /// image. The image itself will not be set by the
        /// constructor. Use <see cref="SetThumbnail(BitmapSource)"/>
        /// to set the thumbnail image data.
        /// </summary>
        /// <param name="proxy">Represented object of the thumbnail</param>
        public ImageThumbnail(IMediaObject proxy)
        {
            InitializeComponent();
            this.Proxy = proxy;

            SetText(proxy != null ? proxy.Name : "");
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
            GetImageControl().Source = image;
        }

        /// <summary>
        /// Change the content of the text label.
        /// </summary>
        /// <param name="text">Text to display below the thumbnail</param>
        public void SetText(string text)
        {
            this.nameDisplay.Content = text;
        }

        #region Helper Functions

        private Image GetImageControl()
        {
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

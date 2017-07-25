#region License
// ActivFlex Presenter - Presentation controls module for ActivFlex
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

namespace ActivFlex.Presentation
{
    /// <summary>
    /// Viewlogic for ImagePresenter.xaml
    /// </summary>
    public partial class ImagePresenter : UserControl
    {
        //Points for the drag translate with the mouse
        private Point mouseStart;
        private Point translateOrigin;

        //Zoom constants
        private const double zoomSpeed = .2D;

        #region DependencyProperties

        /// <summary>
        /// Identifies the PathProperty dependency property
        /// </summary>
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Image", typeof(ImageSource), typeof(ImagePresenter), new FrameworkPropertyMetadata(Image_PropertyChanged));

        /// <summary>
        /// Gets or sets the image data source 
        /// used to present the current image.
        /// </summary>
        public ImageSource Image {
            get => (ImageSource)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        #endregion

        public ImagePresenter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Reset all image rendering transformations
        /// back to their default values. Will be called 
        /// automatically when the image has changed.
        /// </summary>
        public void ResetRenderTransform()
        {
            tScale.ScaleX = 1.0;
            tScale.ScaleY = 1.0;
            tTranslate.X = 0;
            tTranslate.Y = 0;
            tRotate.Angle = 0;
            tSkew.AngleX = 0;
            tSkew.AngleY = 0;
        }

        private static void Image_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (ImagePresenter)d;
            presenter.ImgDisplay.Source = presenter.Image;

            //Reset the rendering options
            presenter.ResetRenderTransform();
            presenter.ImgDisplay_SizeChanged(presenter, null);
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double deltaZoom = e.Delta > 0 ? zoomSpeed : -zoomSpeed;
            tScale.ScaleX += deltaZoom;
            tScale.ScaleY += deltaZoom;
        }

        private void ImgDisplay_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Use the center of the image as scaling transform center
            tScale.CenterX = ImgDisplay.ActualWidth / 2;
            tScale.CenterY = ImgDisplay.ActualHeight / 2;
        }

        private void ImgDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Handle mouse events only for the image during a drag
            ImgDisplay.CaptureMouse();

            mouseStart = Mouse.GetPosition(GridPresenter);
            translateOrigin = new Point(tTranslate.X, tTranslate.Y);
        }

        private void ImgDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImgDisplay.ReleaseMouseCapture();
        }

        private void ImgDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ImgDisplay.IsMouseCaptured)
                return;

            //Move the image based on the mouse drag vector
            Vector dragVector = mouseStart - Mouse.GetPosition(GridPresenter);
            tTranslate.X = translateOrigin.X - dragVector.X;
            tTranslate.Y = translateOrigin.Y - dragVector.Y;
        }
    }
}
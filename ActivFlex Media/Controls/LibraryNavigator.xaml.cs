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
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using ActivFlex.Libraries;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Viewlogic for LibraryNavigator.xaml
    /// </summary>
    public partial class LibraryNavigator : UserControl
    {
        #region DependencyProperties

        /// <summary>
        /// Identifies the ContainerProperty dependency property
        /// </summary>
        public static readonly DependencyProperty ContainerProperty = DependencyProperty.Register(
            "Container", typeof(MediaContainer), typeof(LibraryNavigator), new FrameworkPropertyMetadata(Container_PropertyChanged));

        /// <summary>
        /// Identifies the Command dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(LibraryNavigator), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the media container to
        /// display on the path navigation segments.
        /// </summary>
        public MediaContainer Container {
            get => (MediaContainer)this.GetValue(ContainerProperty);
            set => this.SetValue(ContainerProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to be executed
        /// when the user clicks on a path segment.
        /// </summary>
        public ICommand Command {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        #endregion

        public LibraryNavigator()
        {
            InitializeComponent();
        }

        private static void Container_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LibraryNavigator)d;
            List<MediaContainer> segments = new List<MediaContainer>();
            MediaContainer curContainer = control.Container;

            while (curContainer.Parent != null) {
                segments.Add(curContainer);
                curContainer = curContainer.Parent;
            }

            segments.Add(curContainer);
            segments.Reverse();

            //Add the container segments to the navigation
            control.ContainerStackPanel.Children.Clear();
            bool rootSegment = true;

            foreach (MediaContainer segment in segments) {

                Button pathElement = new Button() {
                    Style = (Style)Application.Current.Resources["PathElement"],
                    Content = segment.Name,
                    Tag = segment
                };

                pathElement.Click += (sender, args) => {
                    control.Command?.Execute(((Button)sender).Tag);
                };

                if (!rootSegment) {
                    control.ContainerStackPanel.Children.Add(new Path() {
                        Style = (Style)Application.Current.Resources["PathSeperator"]
                    });
                } else {
                    rootSegment = false;
                }
                
                control.ContainerStackPanel.Children.Add(pathElement);
            }
        }
    }
}
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
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Viewlogic for PathNavigator.xaml
    /// </summary>
    public partial class PathNavigator : UserControl
    {
        #region DependencyProperties

        /// <summary>
        /// Identifies the PathProperty dependency property
        /// </summary>
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(PathNavigator), new FrameworkPropertyMetadata(Path_PropertyChanged));

        /// <summary>
        /// Identifies the Command dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(PathNavigator), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the path to be displayed
        /// on the path navigation segments.
        /// </summary>
        [Bindable(true)]
        public string Path {
            get => (string)this.GetValue(PathProperty);
            set => this.SetValue(PathProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to be execute
        /// when the user clicks on a path segment.
        /// </summary>
        [Bindable(true)]
        public ICommand Command {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        #endregion

        /// <summary>
        /// Parts of the path seperated by '/'.
        /// Will be set when path has changed.
        /// </summary>
        public string[] PathSegments { get; set; }

        public PathNavigator()
        {
            InitializeComponent();
        }

        private static void Path_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Split the path into segments
            var control = (PathNavigator)d;
            control.PathSegments = control.Path.Split('/');
            control.PathStackPanel.Children.Clear();

            for (int i = 0; i < control.PathSegments.Length; i++) {

                Button pathElement = new Button() {
                    Style = (Style)Application.Current.Resources["PathElement"],
                    Content = control.PathSegments[i],
                    Tag = control.GetPathElements(i + 1)
                };
                
                pathElement.Click += (sender, args) => {
                    control.Command?.Execute(((Button)sender).Tag);
                };

                control.PathStackPanel.Children.Add(pathElement);

                //Add the seperator until we reach the last element
                if (i < control.PathSegments.Length - 1) {
                    control.PathStackPanel.Children.Add(new Path() {
                        Style = (Style)Application.Current.Resources["PathSeperator"]
                    });
                }
            }
        }

        private string GetPathElements(int amount)
        {
            return String.Join("/", PathSegments.Take(amount));
        }
    }
}
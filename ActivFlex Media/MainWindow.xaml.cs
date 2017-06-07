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

// You should have received a copy of the GNU General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ActivFlex
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Will be true when the window 
        /// was set into fullscreen state
        /// </summary>
        private bool Fullscreen { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //titlebar button events
            btnMinimize.Click += (s, e) => this.WindowState = WindowState.Minimized;
            btnMaximize.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized && Fullscreen)
                    Fullscreen = false;

                this.WindowState = (this.WindowState == WindowState.Maximized
                                                        ? WindowState.Normal
                                                        : WindowState.Maximized);

            };
            btnClose.Click += (s, e) => this.Close();

            //fullscreen key bindings
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F11 || (e.SystemKey == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt))
                {
                    Fullscreen = !Fullscreen;

                    if (this.WindowState == WindowState.Maximized && Fullscreen)
                        this.WindowState = WindowState.Normal;
                    
                    //toggle the window state
                    this.WindowState = (Fullscreen ? WindowState.Maximized : WindowState.Normal);
                }
            };
        }


    }
}

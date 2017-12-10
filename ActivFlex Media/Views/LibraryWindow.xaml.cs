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
using System.Windows.Media;
using System.Windows.Controls;
using ActivFlex.ViewModels;
using ActivFlex.Localization;

namespace ActivFlex.Views
{
    /// <summary>
    /// Viewlogic for LibraryWindow.xaml
    /// </summary>
    public partial class LibraryWindow : Window
    {
        /// <summary>
        /// View model instance for this window.
        /// </summary>
        LibraryWindowViewModel vm;

        /// <summary>
        /// Create a new library config window instance.
        /// </summary>
        /// <param name="localizeManager">Reference to the current localization</param>
        public LibraryWindow(TranslateManager localizeManager)
        {
            InitializeComponent();
            this.vm = new LibraryWindowViewModel(localizeManager);
            this.DataContext = vm;
            this.LibraryNameBox.Focus();
        }

        private void LibraryConfig_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) {
                vm.LibraryName = LibraryNameBox.Text;
                vm.OwnerName = LibraryOwnerBox.Text;
                vm.Apply.Execute(this);
            }
        }

        private void LibraryNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.LibraryNameBrush = Brushes.White;
        }

        private void LibraryOwnerBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.OwnerNameBrush = Brushes.White;
        }
    }
}
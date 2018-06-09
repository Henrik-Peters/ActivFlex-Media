#region License
// ActivFlex Media - Manage your media libraries
// Copyright(C) 2018 Henrik Peters
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
using ActivFlex.Localization;
using ActivFlex.ViewModels;

namespace ActivFlex.Views
{
    /// <summary>
    /// Interaktionslogik für MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        /// <summary>
        /// View model instance for this window.
        /// </summary>
        MessageBoxViewModel vm;

        /// <summary>
        /// Create a new message box window instance.
        /// </summary>
        /// <param name="localizeManager">Reference to the current localization</param>
        public MessageBox(TranslateManager localizeManager)
        {
            InitializeComponent();
            this.vm = new MessageBoxViewModel(localizeManager);
            this.DataContext = vm;
        }

        private void MessageBoxWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) {
                vm.Close.Execute(this);
            }
        }
    }
}

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

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// ViewModel implementation for the info window.
    /// </summary>
    public class InfoWindowViewModel : ViewModel
    {
        /// <summary>
        /// Close the passed window instance.
        /// </summary>
        public ICommand Close { get; set; }

        /// <summary>
        /// Create a new view model instance for
        /// the application info window view.
        /// </summary>
        public InfoWindowViewModel()
        {
            this.Close = new RelayCommand<Window>(CloseWindow);
        }

        private void CloseWindow(Window window)
        {
            if (window != null) {
                window.Close();
            }
        }
    }
}
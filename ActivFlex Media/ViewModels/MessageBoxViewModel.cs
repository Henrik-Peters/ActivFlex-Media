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

namespace ActivFlex.ViewModels
{
    public class MessageBoxViewModel : ViewModel
    {
        /// <summary>
        /// Reference to the localization manager.
        /// </summary>
        private TranslateManager _translateManager;
        public TranslateManager Localize {
            get => _translateManager;
            set => SetProperty(ref _translateManager, value);
        }

        /// <summary>
        /// The text displayed in the titlebar of the messagebox.
        /// </summary>
        private string _title;
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// The text displayed in the main box.
        /// </summary>
        private string _message;
        public string Message {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// Close the passed window instance.
        /// </summary>
        public ICommand Close { get; set; }

        /// <summary>
        /// Create a new view model instance for
        /// the message box window view.
        /// </summary>
        /// <param name="localizeManager">Reference to the current localization</param>
        public MessageBoxViewModel(TranslateManager localizeManager)
        {
            this.Localize = localizeManager;
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

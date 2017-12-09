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
using ActivFlex.Localization;
using ActivFlex.ViewModels;

namespace ActivFlex.Views
{
    /// <summary>
    /// Viewlogic for DeleteDialog.xaml
    /// </summary>
    public partial class DeleteDialog : Window
    {
        private string _deleteTextBefore;
        private string _deleteTextAfter;

        /// <summary>
        /// View model instance for this window.
        /// </summary>
        DeleteDialogViewModel vm;

        /// <summary>
        /// The text before the name of the object.
        /// </summary>
        public string DeleteTextBefore {
            get => _deleteTextBefore;
            set {
                BeforeTextField.Text = value;
                _deleteTextBefore = value;
            }
        }

        /// <summary>
        /// The text after the name of the object.
        /// </summary>
        public string DeleteTextAfter {
            get => _deleteTextAfter;
            set {
                AfterTextField.Text = value;
                _deleteTextAfter = value;
            }
        }

        /// <summary>
        /// Create a new delete dialog window instance.
        /// </summary>
        /// <param name="localizeManager">Reference to the current localization</param>
        public DeleteDialog(TranslateManager localizeManager)
        {
            InitializeComponent();
            this.vm = new DeleteDialogViewModel(localizeManager);
            this.DataContext = vm;
        }

        private void DeleteDialogWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) {
                vm.Confirm.Execute(this);
            }
        }
    }
}
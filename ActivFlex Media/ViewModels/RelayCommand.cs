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
using System.Windows.Input;

namespace ActivFlex.ViewModels
{
    /// <summary>
    /// This class can be used to link a command property to
    /// an executable action in the viewModel. This allows a
    /// simple forward execution. No parameters are used.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private Action _targetMethod;
        private Func<bool> _executeValidator;

        /// <summary>
        /// Is raised when <see cref="canExecute(object)"/> has changed.
        /// The CommandManager is not connected to save performance.
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add { /*CommandManager.RequerySuggested += value;*/ }
            remove { /*CommandManager.RequerySuggested -= value;*/ }
        }

        /// <summary>
        /// Create a new relay command that will run the passed
        /// action when <see cref="Execute(object)"/> is called.
        /// </summary>
        /// <param name="execute">Action for relaying to</param>
        /// <param name="canExecute">Function to call for canExecute</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this._targetMethod = execute;
            this._executeValidator = canExecute;
        }

        /// <summary>
        /// Check if this command can be executed by the
        /// canExecute function (passed by the constructor).
        /// </summary>
        /// <param name="parameter">Parameters are not used</param>
        /// <returns>True when the command can execute</returns>
        public bool CanExecute(object parameter)
        {
            return _executeValidator == null ? true : _executeValidator.Invoke();
        }

        /// <summary>
        /// Execute the action passed by the constructor.
        /// </summary>
        /// <param name="parameter">Parameters are not used</param>
        public void Execute(object parameter)
        {
            _targetMethod?.Invoke();
        }
    }

    /// <summary>
    /// This class can be used to link a command property to
    /// an executable action in the viewModel. This allows a
    /// simple forward execution. It uses one parameter.
    /// </summary>
    /// <typeparam name="T">Type of the the parameter</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _targetMethod;
        private readonly Predicate<T> _canExecute;

        /// <summary>
        /// Is raised when <see cref="canExecute(object)"/> has changed.
        /// The CommandManager is not connected to save performance.
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add { /*CommandManager.RequerySuggested += value;*/ }
            remove { /*CommandManager.RequerySuggested -= value;*/ }
        }

        /// <summary>
        /// Create a new relay command that will run the passed
        /// action when <see cref="Execute(object)"/> is called.
        /// </summary>
        /// <param name="execute">Action for relaying to</param>
        /// <param name="canExecute">Predicate to call for canExecute</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this._targetMethod = execute;
            this._canExecute = canExecute;
        }

        /// <summary>
        /// Check if this command can be executed by the
        /// canExecute function (passed by the constructor).
        /// </summary>
        /// <param name="parameter">Has to be of the type T</param>
        /// <returns>True when the command can execute</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute.Invoke((T)parameter);
        }

        /// <summary>
        /// Execute the action passed by the constructor.
        /// </summary>
        /// <param name="parameter">Has to be of the type T</param>
        public void Execute(object parameter)
        {
            _targetMethod?.Invoke((T)parameter);
        }
    }
}
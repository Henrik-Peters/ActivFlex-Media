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
namespace ActivFlex.Configuration
{
    /// <summary>
    /// Possible window states in restore mode.
    /// </summary>
    public enum WindowRestoreState
    {
        Default,
        Maximised,
        Fullscreen
    }

    /// <summary>
    /// Possible options for the window layout
    /// when the application will be started.
    /// </summary>
    public enum WindowStartupState
    {
        /// <summary>
        /// Use the startup options of the main window.
        /// </summary>
        Default,

        /// <summary>
        /// Set the window state to maximised.
        /// </summary>
        Maximised,

        /// <summary>
        /// Set the window into fullscreen mode.
        /// </summary>
        Fullscreen,

        /// <summary>
        /// Use the window size from the last session,
        /// but center the window on the screen.
        /// </summary>
        RestoreSizeCentered,

        /// <summary>
        /// Use the window size and position of the last session.
        /// </summary>
        RestoreAll
    }
}

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
namespace ActivFlex.Media
{
    /// <summary>
    /// This state provides information about 
    /// the loading situation for an image.
    /// </summary>
    public enum ImageLoadState
    {
        /// <summary>
        /// The image was loaded correctly
        /// </summary>
        Successful,

        /// <summary>
        /// The loading has not been launched
        /// </summary>
        Waiting,

        /// <summary>
        /// The image is currently loading
        /// </summary>
        Loading,

        /// <summary>
        /// The image could not be loaded,
        /// because a bad path was provided
        /// </summary>
        InvalidPath,

        /// <summary>
        /// An error has occurred during loading
        /// </summary>
        Error
    }
}
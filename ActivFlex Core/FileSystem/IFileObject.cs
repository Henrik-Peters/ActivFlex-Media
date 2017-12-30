#region License
// ActivFlex Core - Core logic module for ActivFlex
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
namespace ActivFlex.FileSystem
{
    /// <summary>
    /// Defines the basic information for an object in
    /// the filesystem by the path and a custom name.
    /// </summary>
    public interface IFileObject
    {
        /// <summary>
        /// Text to describe the object.
        /// Most likely the filename.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Location of the object in the filesystem.
        /// This should be a full path (absolute).
        /// </summary>
        string Path { get; }

        /// <summary>
        /// True when the item is selected in a view.
        /// </summary>
        bool IsSelected { get; set; }
    }
}
#region License
// ActivFlex Storage - Media library data storage module
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
namespace ActivFlex.Storage
{
    /// <summary>
    /// Defines the operations that a storage engine must
    /// provide in order to be used for media library data.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// This method should be called during the construction
        /// process of a storage provider for preparation purposes.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Used to clean up all used resources before the
        /// shutdown of the application or the storage engine.
        /// </summary>
        void Dispose();
    }
}
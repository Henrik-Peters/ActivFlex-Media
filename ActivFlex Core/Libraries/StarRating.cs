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
namespace ActivFlex.Libraries
{
    /// <summary>
    /// Different rating states for a library item.
    /// </summary>
    public enum StarRating
    {
        /// <summary>
        /// The item has not been rated yet
        /// </summary>
        NoRating = 0,

        /// <summary>
        /// 1 star (lowest) rating
        /// </summary>
        OneStar = 1,

        /// <summary>
        /// 2 stars rating
        /// </summary>
        TwoStars = 2,

        /// <summary>
        /// 3 stars rating
        /// </summary>
        ThreeStars = 3,

        /// <summary>
        /// 4 stars rating
        /// </summary>
        FourStars = 4,

        /// <summary>
        /// 5 stars (highest) rating
        /// </summary>
        FiveStars = 5
    }
}

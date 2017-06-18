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
using System.Windows.Controls;
using System.Windows.Media;

namespace ActivFlex.Controls
{
    /// <summary>
    /// Contains all extension methods
    /// for the TreeViewItem-Control.
    /// </summary>
    public static class TreeViewItemExtensions
    {
        /// <summary>
        /// Returns the depth of a TreeViewItem in the TreeView.
        /// The root-element will have a depth of zero.
        /// </summary>
        /// <param name="item">Return the depth of this instance</param>
        /// <returns>A positive number of the depth, zero for the root element</returns>
        public static int GetDepth(this TreeViewItem item)
        {
            TreeViewItem parent = GetParent(item);
            int depth = 0;

            while (parent != null) {
                parent = GetParent(parent);
                depth++;
            }

            return depth;
        }

        private static TreeViewItem GetParent(TreeViewItem item)
        {
            DependencyObject parent = item;

            do {
                parent = VisualTreeHelper.GetParent(parent);

            } while (!(parent is TreeViewItem || parent is TreeView));

            return parent as TreeViewItem;
        }
    }
}

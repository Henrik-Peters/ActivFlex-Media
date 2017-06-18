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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ActivFlex.Controls;

namespace ActivFlex.Converters
{
    /// <summary>
    /// Converts a TreeViewItem to a Margin, to display the
    /// item at the correct position based on the item depth.
    /// </summary>
    [ValueConversion(typeof(TreeViewItem), typeof(Thickness))]
    public class TreeViewItemMarginConverter : IValueConverter
    {
        /// <summary>
        /// This length will be multiplied by the depth of
        /// the TreeViewItem during the value conversion.
        /// </summary>
        public double MarginMultiplier { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(MarginMultiplier * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Invalid conversion: Margin can not be converted to a TreeViewItem");
        }
    }
}

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
using System.Windows.Data;

namespace ActivFlex.Converters
{
    /// <summary>
    /// Converts a timespan represented in milliseconds
    /// to a readable string format. Hours and days will
    /// only be displayed when they are not then zero.
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class MillisecondsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double milliseconds) {
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

                if (timeSpan.Days > 0) {
                    return timeSpan.ToString(@"dd\:hh\:mm\:ss");
                }

                if (timeSpan.Hours > 0) {
                    return timeSpan.ToString(@"hh\:mm\:ss");
                }

                return timeSpan.ToString(@"mm\:ss");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && TimeSpan.TryParse((string)value, out TimeSpan span)) {
                return span.TotalMilliseconds;
            } else {
                return null;
            }
        }
    }
}
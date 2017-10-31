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
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using ActivFlex.Navigation;

namespace ActivFlex.Converters
{
    /// <summary>
    /// Converts a NavItem to an icon, that will be loaded
    /// from the application resources. Make sure to include
    /// these resources when using this converter.
    /// </summary>
    [ValueConversion(typeof(NavItem), typeof(object))]
    public class NavItemIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string resourceIdentifier = "GeneralIcon";

            var item = value as NavItem;
            if (item == null)
                return Application.Current.Resources[resourceIdentifier];

            //Load the correct resource by the type of the nav item
            if (item.GetType() == typeof(GroupNavItem)) {
                resourceIdentifier = ((GroupNavItem)item).IconResource;
                
            } else if (item.GetType() == typeof(DirectoryNavItem)) {
                resourceIdentifier = ((DirectoryNavItem)item).IconResource;
                
            } else if (item.GetType() == typeof(LibraryNavItem)) {
                resourceIdentifier = "MediaLibraryNavIcon";

            } else if (item.GetType() == typeof(LogicalDriveNavItem)) {

                //Use the drive type to load the resource
                switch (((LogicalDriveNavItem)item).DriveType) {
                    case DriveType.Fixed:
                    case DriveType.Network:
                        resourceIdentifier = "HardDiskIcon";
                        break;

                    case DriveType.Removable:
                        resourceIdentifier = "HardDiskUSBIcon";
                        break;

                    case DriveType.CDRom:
                    case DriveType.Unknown:
                        resourceIdentifier = "DiskDriveIcon";
                        break;
                }
            }

            return Application.Current.Resources[resourceIdentifier];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Invalid conversion: Icon can not be converted to a NavItem");
        }
    }
}
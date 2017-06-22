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
using System.IO;
using System.Linq;

namespace ActivFlex.FileSystem
{
    /// <summary>
    /// Provides the data of a logical drive like
    /// hard disk, USB-sticks, removable media, etc.
    /// </summary>
    public class LogicalDriveItem : FileSystemItem
    {
        private string _name;

        /// <summary>
        /// Create a representation of a logical file system drive.
        /// The constructor will try to create a complete drive info.
        /// </summary>
        /// <param name="FullPath">The path of the drive. For example: "C:\\"</param>
        public LogicalDriveItem(string FullPath) : base(FullPath)
        {
            try {
                DriveInfo driveInfo = new DriveInfo(FullPath);

                this.Name = driveInfo.VolumeLabel;
                this.TotalSize = driveInfo.TotalSize;
                this.DriveType = driveInfo.DriveType;
                this.AvailableFreeSpace = driveInfo.AvailableFreeSpace;

            } catch {
                _name = FullPath;
                this.TotalSize = 0;
                this.AvailableFreeSpace = 0;
                this.DriveType = DriveType.Unknown;
            }
        }

        /// <summary>
        /// Indicates the amount of available free space on a drive, in bytes.
        /// </summary>
        public long AvailableFreeSpace { get; private set; }

        /// <summary>
        /// Gets the total size of storage space on a drive, in bytes.
        /// </summary>
        public long TotalSize { get; private set; }

        /// <summary>
        /// Gets the drive type, such as CD-ROM, removabsle, network, or fixed.
        /// </summary>
        public DriveType DriveType { get; private set; }
        
        /// <summary>
        /// The volume-label of the logical drive.
        /// </summary>
        public override string Name {
            get { return _name; }
            protected set { _name = value; }
        }

        /// <summary>
        /// Check if the drive is still registered in the system.
        /// </summary>
        public override bool Exists {
            get {
                try {
                    return DriveInfo.GetDrives()
                            .Where(drive => drive.Name == FullPath)
                            .Count()
                            .Equals(1);
                } catch {
                    return false;
                }
            }
        }
    }
}

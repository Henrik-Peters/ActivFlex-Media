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
using System.Collections.Generic;

namespace ActivFlex.Libraries
{
    /// <summary>
    /// Used to store media items inside a media library.
    /// This may also include subcontainers with more items.
    /// </summary>
    public class MediaContainer
    {
        /// <summary>
        /// Unique numeric identifier for the current container.
        /// </summary>
        public int ContainerID { get; private set; }

        /// <summary>
        /// The display name of the container.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All subcontainers below the current container.
        /// </summary>
        public List<MediaContainer> Containers { get; set; }

        /// <summary>
        /// Create a new media container without any subcontainers.
        /// </summary>
        /// <param name="containerID">Unique numeric identifier</param>
        /// <param name="name">Displayname for the container</param>
        public MediaContainer(int containerID, string name)
        {
            this.ContainerID = containerID;
            this.Name = name;
            this.Containers = new List<MediaContainer>();
        }
    }
}
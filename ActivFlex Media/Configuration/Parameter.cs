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
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static ActivFlex.FileSystem.FileSystemBrowser;
using ActivFlex.Media;

namespace ActivFlex.Configuration
{
    /// <summary>
    /// Provides access to the parsed command line arguments
    /// for the application by using a singleton. The arguments
    /// will be parsed lazily. This class is thread-safe.
    /// </summary>
    public sealed class Parameter
    {
        private static readonly Lazy<Parameter> lazy =
            new Lazy<Parameter>(() => new Parameter(), true);

        private Parameter()
        {
            string[] args = Environment.GetCommandLineArgs();

            List <string> directoryPaths = new List<string>();
            List<string> imagePaths = new List<string>();
            List<string> musicPaths = new List<string>();

            //Try to parse all command line arguments
            for (int i = 1; i < args.Length; i++) {
                string path = args[i].Trim(' ');

                if (Directory.Exists(path)) {
                    directoryPaths.Add(path);

                } else if (File.Exists(path) && MediaImage.ImageExtensions.Contains(GetExtension(path))) {
                    imagePaths.Add(path);

                } else if (File.Exists(path) && MediaMusic.MusicExtensions.Contains(GetExtension(path))) {
                    musicPaths.Add(path);
                }
            }

            //Create read-only collections from the lists
            this.ImagePaths = Array.AsReadOnly(imagePaths.ToArray());
            this.DirectoryPaths = Array.AsReadOnly(directoryPaths.ToArray());
            this.MusicPaths = Array.AsReadOnly(musicPaths.ToArray());
        }

        /// <summary>
        /// Gets the parsed command line arguments of the
        /// application startup. All properties should be
        /// read-only to make them thread-safe.
        /// </summary>
        public static Parameter StartupOptions {
            get => lazy.Value;
        }

        /// <summary>
        /// Contains all arguments that are valid directory paths.
        /// </summary>
        public ReadOnlyCollection<string> DirectoryPaths { get; set; }

        /// <summary>
        /// Contains all arguments that are valid paths to media images.
        /// </summary>
        public ReadOnlyCollection<string> ImagePaths { get; set; }

        /// <summary>
        /// Contains all arguments that are valid paths to music items.
        /// </summary>
        public ReadOnlyCollection<string> MusicPaths { get; set; }

        /// <summary>
        /// Get the number of all parsed arguments.
        /// </summary>
        public int OptionsAmount {
            get => DirectoryPaths.Count + ImagePaths.Count + MusicPaths.Count;
        }

        /// <summary>
        /// Will be true when at least one argument is
        /// valid and some parsed data will be available.
        /// </summary>
        public bool HasOptions {
            get => OptionsAmount > 0;
        }
    }
}
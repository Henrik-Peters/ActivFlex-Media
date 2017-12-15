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
using System.Reflection;
using System.Xml.Serialization;
using ActivFlex.Localization;

namespace ActivFlex.Configuration
{
    /// <summary>
    /// This class contains all data of a concrete configuration.
    /// All properties should be used as read-only to be thread-safe.
    /// The setters should only be used in the serialisation process.
    /// </summary>
    [Serializable]
    [XmlRoot("ActivFlex-Config")]
    public sealed class ConfigData
    {
        #region Config Metadata

        /// <summary>
        /// Contains a configuration with default values.
        /// This instance should be used as a fallback.
        /// </summary>
        public static readonly ConfigData DefaultConfig = new ConfigData(
            Environment.UserName, 
            Language.English, 
            WindowStartupState.Default, 
            WindowStartupState.Fullscreen,
            WindowRestoreState.Default,
            1640, 835, 200, 100,
            512,
            true,
            LaunchBehavior.Self,
            LaunchBehavior.Self,
            LaunchBehavior.Self,
            0.5,
            true,
            true,
            false
        );

        /// <summary>
        /// The version of ActivFlex Media when this config
        /// was created. Can be used to detect old config versions.
        /// </summary>
        [XmlAttribute("ActivFlex-Version")]
        public string Version { get; set; }

        /// <summary>
        /// Constructor for serialization
        /// </summary>
        private ConfigData() { }

        #endregion

        /// <summary>
        /// Name to be used when modifying media items.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Localization language to use for translations.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Options for the size and position of the window
        /// on a startup without any media arguments.
        /// </summary>
        public WindowStartupState NormalStartup { get; set; }

        /// <summary>
        /// Options for the size and position of the window
        /// on a startup with media arguments.
        /// </summary>
        public WindowStartupState PresenterStartup { get; set; }

        /// <summary>
        /// The state of the window for restoring the layout.
        /// </summary>
        public WindowRestoreState RestoreState { get; set; }

        /// <summary>
        /// Width of the window used for restoring the window size.
        /// </summary>
        public double RestoreWidth { get; set; }

        /// <summary>
        /// Height of the window used for restoring the window size.
        /// </summary>
        public double RestoreHeight { get; set; }

        /// <summary>
        /// Left position of the window used for restoring.
        /// </summary>
        public double RestoreLeft { get; set; }

        /// <summary>
        /// Top position of the window used for restoring.
        /// </summary>
        public double RestoreTop { get; set; }

        /// <summary>
        /// The DecodePixelWidth for thumbnail images.
        /// </summary>
        public int ThumbnailDecodeSize { get; set; }

        /// <summary>
        /// When set to true images will be preloaded
        /// to display them faster in the presentation mode.
        /// </summary>
        public bool PreloadPresenterImages { get; set; }

        /// <summary>
        /// Default launch option for image items.
        /// </summary>
        public LaunchBehavior ImageLaunchBehavior { get; set; }

        /// <summary>
        /// Default launch option for music items.
        /// </summary>
        public LaunchBehavior MusicLaunchBehavior { get; set; }

        /// <summary>
        /// Default launch option for video items.
        /// </summary>
        public LaunchBehavior VideoLaunchBehavior { get; set; }

        /// <summary>
        /// The last volume of the media playback.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Display or hide the media playback time side labels.
        /// </summary>
        public bool ShowTimelineSideLabels { get; set; }

        /// <summary>
        /// Restore the navigation tree expansions 
        /// states when the application restarts.
        /// </summary>
        public bool RestoreNavExpansions { get; set; }

        /// <summary>
        /// Directly delete items without the confirmation dialog.
        /// </summary>
        public bool DirectLibraryItemDelete { get; set; }

        /// <summary>
        /// Create a new dataset for a concrete configuration. 
        /// All properties should only be set with this constructor.
        /// </summary>
        /// <param name="Username">Name to be used when modifying media items</param>
        /// <param name="Language">Localization language to use for translations</param>
        /// <param name="NormalStartup">Window layout options for a startup</param>
        /// <param name="PresenterStartup">Window layout options for a startup in presentation mode</param>
        /// <param name="RestoreWidth">Width of the window used for restoring</param>
        /// <param name="RestoreHeight">Height of the window used for restoring</param>
        /// <param name="RestoreLeft">Left position of the window used for restoring</param>
        /// <param name="RestoreTop">Top position of the window used for restoring</param>
        /// <param name="ThumbnailDecodeSize">The width size to use for thumbnail images</param>
        /// <param name="PreloadPresenterImages">Enable or disable preloading of images in the presentation mode</param>
        /// <param name="ImageLaunchBehavior">Default launch option for image items</param>
        /// <param name="MusicLaunchBehavior">Default launch option for music items</param>
        /// <param name="VideoLaunchBehavior">Default launch option for video items</param>
        /// <param name="Volume">The last volume of the media playback</param>
        /// <param name="ShowTimelineSideLabels">Display or hide the media playback time side labels</param>
        /// <param name="RestoreNavExpansions">Restore the navigation tree view item expansions</param>
        /// <param name="DirectLibraryItemDelete">Directly delete items without the confirmation dialog</param>
        public ConfigData(string Username, Language Language, WindowStartupState NormalStartup, WindowStartupState PresenterStartup, 
                          WindowRestoreState RestoreState, double RestoreWidth, double RestoreHeight, double RestoreLeft, double RestoreTop,
                          int ThumbnailDecodeSize, bool PreloadPresenterImages, LaunchBehavior ImageLaunchBehavior, LaunchBehavior MusicLaunchBehavior,
                          LaunchBehavior VideoLaunchBehavior, double Volume, bool ShowTimelineSideLabels, bool RestoreNavExpansions, bool DirectLibraryItemDelete)
        {
            this.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Username = Username;
            this.Language = Language;
            this.NormalStartup = NormalStartup;
            this.PresenterStartup = PresenterStartup;
            this.RestoreState = RestoreState;
            this.RestoreWidth = RestoreWidth;
            this.RestoreHeight = RestoreHeight;
            this.RestoreLeft = RestoreLeft;
            this.RestoreTop = RestoreTop;
            this.ThumbnailDecodeSize = ThumbnailDecodeSize;
            this.PreloadPresenterImages = PreloadPresenterImages;
            this.ImageLaunchBehavior = ImageLaunchBehavior;
            this.MusicLaunchBehavior = MusicLaunchBehavior;
            this.VideoLaunchBehavior = VideoLaunchBehavior;
            this.Volume = Volume;
            this.ShowTimelineSideLabels = ShowTimelineSideLabels;
            this.RestoreNavExpansions = RestoreNavExpansions;
            this.DirectLibraryItemDelete = DirectLibraryItemDelete;
        }
    }
}
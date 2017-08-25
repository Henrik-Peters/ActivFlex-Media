﻿#region License
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
            WindowStartupState.Fullscreen
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
        /// Create a new dataset for a concrete configuration. 
        /// All properties should only be set with this constructor.
        /// </summary>
        /// <param name="Username">Name to be used when modifying media items</param>
        /// <param name="Language">Localization language to use for translations</param>
        /// <param name="NormalStartup">Window layout options for a startup</param>
        /// <param name="PresenterStartup">Window layout options for a startup in presentation mode</param>
        public ConfigData(string Username, Language Language, WindowStartupState NormalStartup, WindowStartupState PresenterStartup)
        {
            this.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Username = Username;
            this.Language = Language;
            this.NormalStartup = NormalStartup;
            this.PresenterStartup = PresenterStartup;
        }
    }
}
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
using System;
using ActivFlex.ViewModels;

namespace ActivFlex.Configuration
{
    /// <summary>
    /// This class is used to create a new config
    /// from an existing one and only update the
    /// new fields in a lightweight way. This class
    /// is thread-safe.
    /// </summary>
    public sealed class ConfigUpdater
    {
        /// <summary>
        /// The current configuration with all updates.
        /// </summary>
        private ConfigData Config { get; set; }

        /// <summary>
        /// Create a new configuration updater by an
        /// existing configuration. This configuration
        /// data will be copied in order not to change
        /// the old configuration.
        /// </summary>
        /// <param name="prototype">Clone this configuration</param>
        private ConfigUpdater(ConfigData prototype)
        {
            this.Config = prototype.Clone() as ConfigData;
        }

        /// <summary>
        /// Save the new configuration with the config provider.
        /// </summary>
        private void SaveConfig()
        {
            ConfigProvider.SaveConfig(this.Config);
        }

        /// <summary>
        /// Create a new config and save it with the config provider.
        /// The new config will be assigned to the main view model.
        /// The old config is only changed in the final assigment.
        /// This operation is thread-safe.
        /// </summary>
        /// <param name="updateAction">Function that transforms the old config into a new config</param>
        public static void UpdateConfig(Action<ConfigData> updateAction)
        {
            ConfigUpdater configUpdater = new ConfigUpdater(MainViewModel.Config);
            updateAction.Invoke(configUpdater.Config);
            configUpdater.SaveConfig();
            MainViewModel.Config = configUpdater.Config;
        }
    }
}

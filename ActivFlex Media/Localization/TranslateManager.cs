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
using System.Collections.Generic;

namespace ActivFlex.Localization
{
    /// <summary>
    /// This class provides the translate functionality
    /// for different languages. In order to switch the
    /// language, a new instance must be created.
    /// All operations are thread safe.
    /// </summary>
    public sealed class TranslateManager
    {
        /// <summary>
        /// The current target language for the translation.
        /// </summary>
        public Language Language { get; }
        private int LanguageIndex => (int)Language;
        
        static IDictionary<string, List<string>> TranslateTable = new Dictionary<string, List<string>> {
            ["Open"] = new List<string> { "Open", "Öffnen" },
            ["NewLibrary"] = new List<string> { "New library", "Neue Biblothek" },
            ["MediaLibraries"] = new List<string> { "Media libraries", "Biblotheken" },
            ["MyComputer"] = new List<string> { "My Computer", "Mein Computer" },
            ["Pictures"] = new List<string> { "Pictures", "Bilder" },
            ["Music"] = new List<string> { "Music", "Musik" },
            ["Videos"] = new List<string> { "Videos", "Videos" },
            ["ShowInfo"] = new List<string> { "Show info", "Info anzeigen" },
            ["ToggleFullscreen"] = new List<string> { "Toggle fullscreen", "Vollbildmodus wechseln" },
            ["Exit"] = new List<string> { "Exit", "Beenden" },
            ["Play"] = new List<string> { "Play", "Wiedergabe" },
            ["PlayProcess"] = new List<string> { "Play (default player)", "Wiedergabe (Standardprogramm)" },
            ["OpenProcess"] = new List<string> { "Open (default program)", "Öffnen (Standardprogramm)" },
            ["LibraryWindowTitle"] = new List<string> { "ActivFlex Media - Library configuration", "ActivFlex Media - Bibliotheksoptionen" },
            ["Cancel"] = new List<string> { "Cancel", "Abbrechen" },
            ["Apply"] = new List<string> { "Apply", "Übernehmen" },
            ["New"] = new List<string> { "New", "Neu" },
            ["Delete"] = new List<string> { "Delete", "Löschen" },
            ["Rename"] = new List<string> { "Rename", "Umbenennen" },
            ["Create"] = new List<string> { "Create", "Erstellen" },
            ["Name:"] = new List<string> { "Name:", "Name:" },
            ["Owner:"] = new List<string> { "Owner:", "Besitzer:" },
            ["Configure"] = new List<string> { "Configure", "Konfigurieren" },
            ["DeleteDialogTitle"] = new List<string> { "Confirm deletion", "Löschvorgang bestätigen" },
            ["Yes"] = new List<string> { "Yes", "Ja" },
            ["No"] = new List<string> { "No", "Nein" },
            ["DeleteFirst"] = new List<string> { "Are you sure that", "Soll die Bibliothek" },
            ["DeleteSecound"] = new List<string> { "should be deleted?", "wirklich gelöscht werden?" },
            ["MediaContainer"] = new List<string> { "Media container", "Mediencontainer" }
        };

        /// <summary>
        /// Get the translation for a specific key.
        /// </summary>
        /// <exception cref="KeyNotFoundException">When the key is not in the translation table</exception>
        /// <exception cref="ArgumentOutOfRangeException">When no translation is available</exception>
        /// <param name="key">Key to use for translation</param>
        /// <returns>Translated string for the key</returns>
        public string this[string key] {
            get => TranslateTable[key][LanguageIndex];
        }

        /// <summary>
        /// Create a new translation manager instance.
        /// The lookup table is static, only the new
        /// language will consume memory.
        /// </summary>
        /// <param name="language">Target translation language</param>
        public TranslateManager(Language language)
        {
            this.Language = language;
        }
    }
}
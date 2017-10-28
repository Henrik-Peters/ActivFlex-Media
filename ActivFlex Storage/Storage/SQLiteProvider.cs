#region License
// ActivFlex Storage - Media library data storage module
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
using System.Diagnostics;
using System.Data.SQLite;

namespace ActivFlex.Storage
{
    /// <summary>
    /// Implementation of the database storage engine
    /// provider with SQLite. The database will be
    /// stored in a local file by this provider.
    /// </summary>
    public class SQLiteProvider : IStorageProvider
    {
        /// <summary>
        /// Connection instance for interaction with the database.
        /// </summary>
        SQLiteConnection connection;

        /// <summary>
        /// Path for the ActivFlex application data.
        /// </summary>
        const string ApplicationFolderName = "ActivFlex Media";

        /// <summary>
        /// Name of the local database file.
        /// </summary>
        const string DatabaseFileName = "MediaLibraries.db";

        /// <summary>
        /// Create a new SQLite database engine provider.
        /// The database connection will use a local database
        /// file exclusively. This means there should only be
        /// one instance of this provider at once.
        /// </summary>
        public SQLiteProvider()
        {
            Initialize();
        }

        public void Initialize()
        {
            //Make sure the environment for the database file is available
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + ApplicationFolderName;
            string dbPath = appDataPath + "\\" + DatabaseFileName;
            bool schemaInit = false;

            if (!Directory.Exists(appDataPath)) {
                Directory.CreateDirectory(appDataPath);
            }

            if (!File.Exists(dbPath)) {
                SQLiteConnection.CreateFile(dbPath);
                schemaInit = true;
            }

            //Open the connection to the database
            connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;UseUTF16Encoding=True;PRAGMA foreign_keys = ON;");
            connection.Open();

            //Setting PRAGMAs via the connection string is sometimes not possible
            new SQLiteCommand(@"PRAGMA foreign_keys = ON;", connection).ExecuteNonQuery();

            if (schemaInit) InitSchema();
        }

        private void InitSchema()
        {
            Debug.WriteLine("Initializing database schema");

            var sqlContainers = @"CREATE TABLE Containers (
                CID INTEGER PRIMARY KEY,
                name VARCHAR(80),
                parent INT,
                FOREIGN KEY (parent) REFERENCES Containers(CID)
                ON UPDATE CASCADE
                ON DELETE CASCADE
            )";

            var sqlLibraries = @"CREATE TABLE Libraries (
                LID INTEGER PRIMARY KEY,
                name VARCHAR(80),
                owner VARCHAR(80),
                rootContainer INT,
                FOREIGN KEY (rootContainer) REFERENCES Containers(CID)
                ON UPDATE CASCADE
                ON DELETE CASCADE
            )";

            SQLiteCommand cmd = new SQLiteCommand(sqlContainers, connection);
            int queryCode = cmd.ExecuteNonQuery();
            Debug.WriteLine("CREATE TABLE Containers: " + queryCode.ToString());

            cmd = new SQLiteCommand(sqlLibraries, connection);
            queryCode = cmd.ExecuteNonQuery();
            Debug.WriteLine("CREATE TABLE Libraries: " + queryCode.ToString());
        }

        public void Dispose()
        {
            if (connection != null) {
                connection.Close();
            }
        }
    }
}
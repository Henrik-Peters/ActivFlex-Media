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
using System.Globalization;
using System.Collections.Generic;
using System.Data.SQLite;
using ActivFlex.Libraries;

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
        /// The format string used to store dates and times.
        /// </summary>
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

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
                expanded BOOL,
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

            var sqlItems = @"CREATE TABLE Items (
                IID INTEGER PRIMARY KEY,
                name TEXT,
                path VARCHAR(255),
                accessCount LONG,
                creationTime TEXT,
                lastAccessTime TEXT
            )";

            var sqlContainerItems = @"CREATE TABLE ContainerItems (
                IID INTEGER,
                CID INTEGER,
                PRIMARY KEY(IID, CID),
                FOREIGN KEY (CID) REFERENCES Containers(CID)
                ON UPDATE CASCADE
                ON DELETE CASCADE
                FOREIGN KEY (IID) REFERENCES Items(IID)
                ON UPDATE CASCADE
                ON DELETE CASCADE
            )";

            SQLiteCommand cmd = new SQLiteCommand(sqlContainers, connection);
            int queryCode = cmd.ExecuteNonQuery();
            Debug.WriteLine("CREATE TABLE Containers: " + queryCode.ToString());

            cmd = new SQLiteCommand(sqlLibraries, connection);
            queryCode = cmd.ExecuteNonQuery();
            Debug.WriteLine("CREATE TABLE Libraries: " + queryCode.ToString());

            cmd = new SQLiteCommand(sqlItems, connection);
            queryCode = cmd.ExecuteNonQuery();
            Debug.WriteLine("CREATE TABLE Items: " + queryCode.ToString());

            cmd = new SQLiteCommand(sqlContainerItems, connection);
            queryCode = cmd.ExecuteNonQuery();
            Debug.WriteLine("CREATE TABLE ContainerItems: " + queryCode.ToString());
        }

        public MediaLibrary CreateMediaLibrary(string name, string owner)
        {
            var sqlContainer = @"INSERT INTO Containers(name, expanded)
                                 VALUES('Root-Container', 0);";

            new SQLiteCommand(sqlContainer, connection).ExecuteNonQuery();

            //Get the ID of the new container
            var sqlContainerID = @"SELECT CID FROM Containers ORDER BY CID DESC LIMIT 1;";
            int rootContainerID = Convert.ToInt32(new SQLiteCommand(sqlContainerID, connection).ExecuteScalar());

            //Create the new library and link to the root container
            var sqlLibrary = @"INSERT INTO Libraries(name, owner, rootContainer)
                               VALUES(@Name, @Owner, @RootContainer);";
            
            var command = new SQLiteCommand(sqlLibrary, connection);
            command.Parameters.AddWithValue("Name", name);
            command.Parameters.AddWithValue("Owner", owner);
            command.Parameters.AddWithValue("RootContainer", rootContainerID);
            command.ExecuteNonQuery();
            
            //Get the ID of the library
            var sqlLibraryID = @"SELECT LID FROM Libraries ORDER BY LID DESC LIMIT 1;";
            int LibraryID = Convert.ToInt32(new SQLiteCommand(sqlLibraryID, connection).ExecuteScalar());

            MediaLibrary library = new MediaLibrary(LibraryID, name, owner, null);
            library.RootContainer = new MediaContainer(rootContainerID, "Root-Container", null, library);
            return library;
        }

        public List<MediaLibrary> ReadMediaLibraries()
        {
            var sql = @"SELECT l.LID, c.CID, l.name AS 'LibName', l.owner, c.name AS 'ContainerName', c.expanded
                        FROM Libraries l
                        INNER JOIN Containers c ON l.rootContainer = c.CID;";

            List<MediaLibrary> libraries = new List<MediaLibrary>();

            //Create the list from the query
            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {

                    while (reader.Read()) {
                        int LID = Convert.ToInt32(reader["LID"]);
                        string libName = reader["LibName"] as string;
                        string owner = reader["owner"] as string;

                        int CID = Convert.ToInt32(reader["CID"]);
                        string containerName = reader["ContainerName"] as string;
                        bool expanded = Convert.ToBoolean(reader["expanded"]);

                        MediaLibrary library = new MediaLibrary(LID, libName, owner, null);
                        MediaContainer rootContainer = new MediaContainer(CID, containerName, null, library, expanded);
                        library.RootContainer = rootContainer;

                        UpdateContainers(rootContainer, library);
                        libraries.Add(library);
                    }
                }
            }

            return libraries;
        }

        private void UpdateContainers(MediaContainer container, MediaLibrary library)
        {
            //Create a new empty container list
            container.Containers = new List<MediaContainer>();
            
            //Resolve all sub media containers
            var sql = @"SELECT CID, name, expanded
                        FROM Containers
                        WHERE parent=@Parent";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("Parent", container.ContainerID);

            //Add all sub containers to the container list
            using (SQLiteDataReader reader = command.ExecuteReader()) {

                while (reader.Read()) {
                    int subID = Convert.ToInt32(reader["CID"]);
                    string subName = reader["name"] as string;
                    bool expanded = Convert.ToBoolean(reader["expanded"]);

                    MediaContainer subContainer = new MediaContainer(subID, subName, container, library, expanded);
                    UpdateContainers(subContainer, library);
                    container.Containers.Add(subContainer);
                }
            }
        }

        public void UpdateMediaLibrary(int libraryID, string name, string owner)
        {
            var sql = @"UPDATE Libraries 
                        SET name=@Name, owner=@OWNER 
                        WHERE LID=@LibraryID";
            
            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("LibraryID", libraryID);
            command.Parameters.AddWithValue("Name", name);
            command.Parameters.AddWithValue("Owner", owner);
            command.ExecuteNonQuery();
        }

        public void DeleteMediaLibrary(int libraryID)
        {
            //Get the root container ID
            var sql = @"SELECT rootContainer
                        FROM Libraries
                        WHERE LID=@LibraryID;";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("LibraryID", libraryID);
            int rootContainerID = Convert.ToInt32(command.ExecuteScalar());
            
            //Delete the library
            sql = @"DELETE FROM Libraries
                    WHERE LID=@LibraryID";

            command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("LibraryID", libraryID);
            command.ExecuteNonQuery();
            
            //Delete the container
            sql = @"DELETE FROM Containers
                    WHERE CID=@ContainerID";

            command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ContainerID", rootContainerID);
            command.ExecuteNonQuery();
        }

        public MediaContainer CreateContainer(string name, MediaContainer parent, MediaLibrary library, bool expanded = false)
        {
            var sql = @"INSERT INTO Containers(name, parent, expanded)
                        VALUES(@Name, @Parent, @Expanded);";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("Name", name);
            command.Parameters.AddWithValue("Parent", parent.ContainerID);
            command.Parameters.AddWithValue("Expanded", expanded);
            command.ExecuteNonQuery();

            //Get the ID of the new container
            var sqlContainerID = @"SELECT CID FROM Containers ORDER BY CID DESC LIMIT 1;";
            int containerID = Convert.ToInt32(new SQLiteCommand(sqlContainerID, connection).ExecuteScalar());
            return new MediaContainer(containerID, name, parent, library, expanded);
        }

        public void UpdateContainer(int containerID, string name, int parentID, bool expanded)
        {
            var sql = @"UPDATE Containers
                        SET name=@Name, parent=@Parent, expanded=@Expanded
                        WHERE CID=@ContainerID";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ContainerID", containerID);
            command.Parameters.AddWithValue("Name", name);
            command.Parameters.AddWithValue("Parent", parentID);
            command.Parameters.AddWithValue("Expanded", expanded);
            command.ExecuteNonQuery();
        }

        public void UpdateContainerExpansion(int containerID, bool expanded)
        {
            var sql = @"UPDATE Containers
                        SET expanded=@Expanded
                        WHERE CID=@ContainerID";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ContainerID", containerID);
            command.Parameters.AddWithValue("Expanded", expanded);
            command.ExecuteNonQuery();
        }

        public void DeleteContainer(int containerID)
        {
            var sql = @"DELETE FROM Containers
                        WHERE CID=@ContainerID";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ContainerID", containerID);
            command.ExecuteNonQuery();
        }

        public ILibraryItem CreateLibraryItem(string name, string path, MediaContainer container, DateTime creationTime)
        {
            var sql = @"INSERT INTO Items(name, path, accessCount, creationTime, lastAccessTime)
                        VALUES(@Name, @Path, @AccessCount, @CreationTime, @LastAccessTime);";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("Name", name);
            command.Parameters.AddWithValue("Path", path);
            command.Parameters.AddWithValue("AccessCount", 0);
            command.Parameters.AddWithValue("CreationTime", creationTime.ToString(DateTimeFormat));
            command.Parameters.AddWithValue("LastAccessTime", default(DateTime));
            command.ExecuteNonQuery();

            //Get the ID of the new item
            var sqlItemID = @"SELECT IID FROM Items ORDER BY IID DESC LIMIT 1;";
            int itemID = Convert.ToInt32(new SQLiteCommand(sqlItemID, connection).ExecuteScalar());

            //Link the item to the container
            sql = @"INSERT INTO ContainerItems(IID, CID)
                    VALUES(@ItemID, @ContainerID);";

            command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ItemID", itemID);
            command.Parameters.AddWithValue("ContainerID", container.ContainerID);
            command.ExecuteNonQuery();

            return LibraryItemFactory.CreateItemByExtension(itemID, name, path, container, creationTime);
        }

        public List<ILibraryItem> ReadItemsFromContainer(MediaContainer container)
        {
            List<ILibraryItem> items = new List<ILibraryItem>();

            var sql = @"SELECT Items.IID, name, path, accessCount, creationTime, lastAccessTime
                        FROM ContainerItems
                        INNER JOIN Items ON Items.IID = ContainerItems.IID
                        WHERE CID=@ContainerID";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ContainerID", container.ContainerID);
            
            //Create all library items and add them to the list
            using (SQLiteDataReader reader = command.ExecuteReader()) {

                while (reader.Read()) {
                    int itemID = Convert.ToInt32(reader["IID"]);
                    string name = reader["name"] as string;
                    string path = reader["path"] as string;
                    ulong accessCount = Convert.ToUInt64(reader["accessCount"]);
                    DateTime creationTime = DateTime.ParseExact((string)reader["creationTime"], DateTimeFormat, CultureInfo.InvariantCulture);
                    DateTime lastAccessTime = DateTime.ParseExact((string)reader["lastAccessTime"], DateTimeFormat, CultureInfo.InvariantCulture);

                    items.Add(LibraryItemFactory.CreateItemByExtension(itemID, name, path, container, creationTime, accessCount, lastAccessTime));
                }
            }

            return items;
        }

        public void DeleteLibraryItem(int itemID)
        {
            var sql = @"DELETE FROM Items
                        WHERE IID=@ItemID";

            var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("ItemID", itemID);
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            if (connection != null) {
                connection.Close();
            }
        }
    }
}
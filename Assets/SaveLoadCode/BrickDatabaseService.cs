using System.Data;
using Unity.VisualScripting.Dependencies.Sqlite;

namespace SaveLoadCode
{
    public class BrickDatabaseService
    {
        private void InitialiseDatabase()
        {
            using IDbConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();

            ExecuteNonQuery(connection,
                @"CREATE TABLE IF NOT EXISTS `SavedModels` (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );"
            );

            ExecuteNonQuery(connection,
                @"CREATE TABLE IF NOT EXISTS `Bricks` (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ModelId INTEGER NOT NULL,
                    PartId INTEGER NOT NULL,
                    PartName TEXT NOT NULL,
                    ColourR REAL NOT NULL,
                    ColourG REAL NOT NULL,
                    ColourB REAL NOT NULL,
                    ColourA REAL NOT NULL,
                    StudWidth INTEGER NOT NULL,
                    StudLength INTEGER NOT NULL,
                    TileHeight INTEGER NOT NULL,
                    PosX REAL NOT NULL,
                    PosY REAL NOT NULL,
                    PosZ REAL NOT NULL,
                    RotX REAL NOT NULL,
                    RotY REAL NOT NULL,
                    RotZ REAL NOT NULL,
                    RotW REAL NOT NULL,
                    FOREIGN KEY (ModelId) REFERENCES SavedModels(ID)
                );"
            );

            ExecuteNonQuery(connection,
                @"CREATE TABLE IF NOT EXISTS `StudConnections` (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ModelId INTEGER NOT NULL,
                    UpperBrickId INTEGER NOT NULL,
                    UpperStudX INTEGER NOT NULL,
                    UpperStudZ INTEGER NOT NULL,
                    LowerBrickId INTEGER NOT NULL,
                    LowerStudX INTEGER NOT NULL,
                    LowerStudZ INTEGER NOT NULL,
                    FOREIGN KEY (ModelId) REFERENCES SavedModels(Id),
                    FOREIGN KEY (UpperBrickId) REFERENCES Bricks(Id),
                    FOREIGN KEY (LowerBrickId) REFERENCES Bricks(Id),
                );"
            );
        }
        
        private void ExecuteNonQuery(IDbConnection connection, string sql)
        {
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}
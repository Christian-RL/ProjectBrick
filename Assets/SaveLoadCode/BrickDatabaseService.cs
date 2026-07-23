using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using BrickCode;
using ModelCode;
using UnityEngine;
using Mono.Data.Sqlite;
namespace SaveLoadCode
{
    public class BrickDatabaseService : MonoBehaviour
    {
        
        public static BrickDatabaseService Instance { get; private set; }
        private string _connectionString;
        private string _databasePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _databasePath = Path.Combine(Application.persistentDataPath, "brick_models.db");
            _connectionString = "URI=file:" + _databasePath;
            InitialiseDatabase();
            Debug.Log("SQLite database: " + _databasePath);
        }
        private void InitialiseDatabase()
        {
            using IDbConnection connection = new SqliteConnection(_connectionString);
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
                    PartId TEXT NOT NULL,
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
                    FOREIGN KEY (LowerBrickId) REFERENCES Bricks(Id)
                );"
            );
        }
        
        private void ExecuteNonQuery(IDbConnection connection, string sql)
        {
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public int SaveCurrentModel(string modelName)
        {
            BrickObjectData[] sceneBricks = FindObjectsOfType<BrickObjectData>();
            if (sceneBricks.Length == 0)
            {
                Debug.LogWarning("No bricks in scene to save");
                return -1;
            }
            using IDbConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            int modelId = InsertModel(connection, modelName);
            Dictionary<Brick, int> databaseIdByBrick = new();
            foreach (BrickObjectData brickData in sceneBricks)
            {
                if (!brickData || brickData.Brick == null) continue;
                int brickDatabaseId = InsertBrick(connection, modelId, brickData);
                databaseIdByBrick[brickData.Brick] = brickDatabaseId;
            }

            foreach (BrickConnection brickConnection in BrickModelRegistry.GetConnections())
            {
                if (!databaseIdByBrick.ContainsKey(brickConnection.UpperBrick)) continue;
                if (!databaseIdByBrick.ContainsKey(brickConnection.LowerBrick)) continue;
                InsertStudConnection(
                    connection,
                    modelId,
                    databaseIdByBrick[brickConnection.UpperBrick],
                    brickConnection.UpperStudX,
                    brickConnection.UpperStudZ,
                    databaseIdByBrick[brickConnection.LowerBrick],
                    brickConnection.LowerStudX,
                    brickConnection.LowerStudZ
                );
            }
            Debug.Log($"Saved model '{modelName}' with ID {modelId}.");
            return modelId;
        }

        private int InsertModel(IDbConnection connection, string modelName)
        {
            using IDbCommand command = connection.CreateCommand();
            command.CommandText =
                @"INSERT INTO SavedModels (Name, CreatedAt)
                VALUES (@Name, @CreatedAt);
                SELECT last_insert_rowid();";
            AddParameter(command, "@Name", modelName);
            AddParameter(command, "@CreatedAt", DateTime.UtcNow.ToString("o"));
            object result  = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        
        private int InsertBrick(IDbConnection connection, int modelId, BrickObjectData brickData)
        {
            Brick brick = brickData.Brick;
            Color colour = brick.GetColour();
            Vector3 position = brickData.transform.position;
            Quaternion rotation = brickData.transform.rotation;

            using IDbCommand command = connection.CreateCommand();

            command.CommandText =
                @"INSERT INTO Bricks (
                    ModelId,
                    PartId,
                    PartName,
                    ColourR,
                    ColourG,
                    ColourB,
                    ColourA,
                    StudWidth,
                    StudLength,
                    TileHeight,
                    PosX,
                    PosY,
                    PosZ,
                    RotX,
                    RotY,
                    RotZ,
                    RotW
                )
                VALUES (
                    @ModelId,
                    @PartId,
                    @PartName,
                    @ColourR,
                    @ColourG,
                    @ColourB,
                    @ColourA,
                    @StudWidth,
                    @StudLength,
                    @TileHeight,
                    @PosX,
                    @PosY,
                    @PosZ,
                    @RotX,
                    @RotY,
                    @RotZ,
                    @RotW
                );
            SELECT last_insert_rowid();";

            AddParameter(command, "@ModelId", modelId);
            AddParameter(command, "@PartId", brick.GetPartID());
            AddParameter(command, "@PartName", brick.GetName());

            AddParameter(command, "@ColourR", colour.r);
            AddParameter(command, "@ColourG", colour.g);
            AddParameter(command, "@ColourB", colour.b);
            AddParameter(command, "@ColourA", colour.a);

            AddParameter(command, "@StudWidth", brick.GetStudWidth());
            AddParameter(command, "@StudLength", brick.GetStudLength());
            AddParameter(command, "@TileHeight", brick.GetTileHeight());

            AddParameter(command, "@PosX", position.x);
            AddParameter(command, "@PosY", position.y);
            AddParameter(command, "@PosZ", position.z);

            AddParameter(command, "@RotX", rotation.x);
            AddParameter(command, "@RotY", rotation.y);
            AddParameter(command, "@RotZ", rotation.z);
            AddParameter(command, "@RotW", rotation.w);

            object result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        
        private void InsertStudConnection(
            IDbConnection connection,
            int modelId,
            int upperBrickId,
            int upperStudX,
            int upperStudZ,
            int lowerBrickId,
            int lowerStudX,
            int lowerStudZ
        )
        {
            using IDbCommand command = connection.CreateCommand();

            command.CommandText =
                @"INSERT INTO StudConnections (
            ModelId,
            UpperBrickId,
            UpperStudX,
            UpperStudZ,
            LowerBrickId,
            LowerStudX,
            LowerStudZ
        )
        VALUES (
            @ModelId,
            @UpperBrickId,
            @UpperStudX,
            @UpperStudZ,
            @LowerBrickId,
            @LowerStudX,
            @LowerStudZ
        );";

            AddParameter(command, "@ModelId", modelId);

            AddParameter(command, "@UpperBrickId", upperBrickId);
            AddParameter(command, "@UpperStudX", upperStudX);
            AddParameter(command, "@UpperStudZ", upperStudZ);

            AddParameter(command, "@LowerBrickId", lowerBrickId);
            AddParameter(command, "@LowerStudX", lowerStudX);
            AddParameter(command, "@LowerStudZ", lowerStudZ);

            command.ExecuteNonQuery();
        }
        
        private void AddParameter(IDbCommand command, string name, object value)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
        
        public void LoadModel(int modelId)
        {
            ClearSceneBricks();
            using IDbConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            Dictionary<int, BrickObjectData> brickDataByDatabaseId = LoadBricks(connection, modelId);
            LoadStudConnections(connection, modelId, brickDataByDatabaseId);
            Debug.Log($"Loaded model ID {modelId}.");
        }
        
        private Dictionary<int, BrickObjectData> LoadBricks(IDbConnection connection, int modelId)
        {
            Dictionary<int, BrickObjectData> brickDataByDatabaseId = new();

            using IDbCommand command = connection.CreateCommand();

            command.CommandText =
                @"SELECT
                    Id,
                    PartId,
                    PartName,
                    ColourR,
                    ColourG,
                    ColourB,
                    ColourA,
                    StudWidth,
                    StudLength,
                    TileHeight,
                    PosX,
                    PosY,
                    PosZ,
                    RotX,
                    RotY,
                    RotZ,
                    RotW
                FROM Bricks
                WHERE ModelId = @ModelId;";

            AddParameter(command, "@ModelId", modelId);

            using IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int databaseBrickId = Convert.ToInt32(reader["Id"]);

                string partId = reader["PartId"].ToString();
                string partName = reader["PartName"].ToString();

                Color colour = new Color(
                    Convert.ToSingle(reader["ColourR"]),
                    Convert.ToSingle(reader["ColourG"]),
                    Convert.ToSingle(reader["ColourB"]),
                    Convert.ToSingle(reader["ColourA"])
                );

                int studWidth = Convert.ToInt32(reader["StudWidth"]);
                int studLength = Convert.ToInt32(reader["StudLength"]);
                int tileHeight = Convert.ToInt32(reader["TileHeight"]);

                Vector3 position = new Vector3(
                    Convert.ToSingle(reader["PosX"]),
                    Convert.ToSingle(reader["PosY"]),
                    Convert.ToSingle(reader["PosZ"])
                );

                Quaternion rotation = new Quaternion(
                    Convert.ToSingle(reader["RotX"]),
                    Convert.ToSingle(reader["RotY"]),
                    Convert.ToSingle(reader["RotZ"]),
                    Convert.ToSingle(reader["RotW"])
                );

                Brick brick = new BasicBrick(
                    partId,
                    partName,
                    colour,
                    studWidth,
                    studLength,
                    tileHeight
                );

                GameObject brickObject = new GameObject("Loaded Brick");

                BrickVisual visual = brickObject.AddComponent<BrickVisual>();
                visual.BuildFromBrick(brick);

                brickObject.transform.SetPositionAndRotation(position, rotation);

                if (!brickObject.GetComponent<DraggableBrick3D>())
                {
                    brickObject.AddComponent<DraggableBrick3D>();
                }

                BrickObjectData brickData = brickObject.GetComponent<BrickObjectData>();

                brickDataByDatabaseId[databaseBrickId] = brickData;
         }

            return brickDataByDatabaseId;
        }
        
        private void LoadStudConnections(
            IDbConnection connection,
            int modelId,
            Dictionary<int, BrickObjectData> brickDataByDatabaseId
        )
        {
            using IDbCommand command = connection.CreateCommand();

            command.CommandText =
                @"SELECT
            UpperBrickId,
            UpperStudX,
            UpperStudZ,
            LowerBrickId,
            LowerStudX,
            LowerStudZ
          FROM StudConnections
          WHERE ModelId = @ModelId;";

            AddParameter(command, "@ModelId", modelId);

            using IDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int upperBrickId = Convert.ToInt32(reader["UpperBrickId"]);
                int lowerBrickId = Convert.ToInt32(reader["LowerBrickId"]);

                if (!brickDataByDatabaseId.ContainsKey(upperBrickId))
                {
                    continue;
                }

                if (!brickDataByDatabaseId.ContainsKey(lowerBrickId))
                {
                    continue;
                }

                BrickObjectData upperBrickData = brickDataByDatabaseId[upperBrickId];
                BrickObjectData lowerBrickData = brickDataByDatabaseId[lowerBrickId];

                int upperStudX = Convert.ToInt32(reader["UpperStudX"]);
                int upperStudZ = Convert.ToInt32(reader["UpperStudZ"]);

                int lowerStudX = Convert.ToInt32(reader["LowerStudX"]);
                int lowerStudZ = Convert.ToInt32(reader["LowerStudZ"]);

                BrickModelRegistry.ConnectStuds(
                    upperBrickData,
                    upperStudX,
                    upperStudZ,
                    lowerBrickData,
                    lowerStudX,
                    lowerStudZ
                );
            }
        }
        
        private void ClearSceneBricks()
        {
            BrickModelRegistry.ClearAll();
            BrickObjectData[] sceneBricks = FindObjectsOfType<BrickObjectData>();
        
            foreach (BrickObjectData brickData in sceneBricks)
            {
                if (!brickData)
                {
                    continue;
                }

                Destroy(brickData.gameObject);
            }

            if (BrickSelectionManager.Instance)
            {
                BrickSelectionManager.Instance.ClearSelection();
            }
        }
        
        public void LoadMostRecentModel()
        {
            using IDbConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            using IDbCommand command = connection.CreateCommand();

            command.CommandText =
                @"SELECT Id
          FROM SavedModels
          ORDER BY Id DESC
          LIMIT 1;";

            object result = command.ExecuteScalar();

            if (result == null)
            {
                Debug.LogWarning("No saved models found.");
                return;
            }

            int modelId = Convert.ToInt32(result);

            LoadModel(modelId);
        }
    }
}
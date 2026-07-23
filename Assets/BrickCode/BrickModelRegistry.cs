using System.Collections.Generic;
using UnityEngine;
using ModelCode;
namespace BrickCode
{
    /**
     * Link logic bricks to Unity game objects.
     */
    public static class BrickModelRegistry
    {
        private static readonly Dictionary<Brick, BrickObjectData> ObjectByBrick = new(); //maps each logic brick to their object data
        private static readonly List<BrickConnection> Connections = new();
        
        
        public static List<BrickConnection> GetConnections()
        {
            return new List<BrickConnection>(Connections);
        }
        
        /**
         * Add brick object to registry.
         */
        public static void Register(BrickObjectData data)
        {
            if (!data || data.Brick == null) return;
            ObjectByBrick[data.Brick] = data;
        }

        /**
         * Remove brick object from registry.
         */
        public static void Unregister(BrickObjectData data)
        {
            if (!data || data.Brick == null) return;
            if (ObjectByBrick.TryGetValue(data.Brick, out BrickObjectData existing) && existing == data) ObjectByBrick.Remove(data.Brick);
        }

        /**
         * Gets the BrickObjectData for a given logic brick.
         */
        public static BrickObjectData GetObjectForBrick(Brick brick)
        {
            if (brick == null) return null;
            ObjectByBrick.TryGetValue(brick, out BrickObjectData data);
            return data;
        }

        /**
         * Collect all visible Unity brick objects connected to a starting logic brick.
         */
        public static List<BrickObjectData> GetConnectedObjects(Brick startingBrick)
        {
            List<BrickObjectData> connectedObjects = new();
            if (startingBrick == null) return connectedObjects;
            BrickModelGraph graph = new BrickModelGraph(startingBrick);
            foreach (BrickModelGraph.BrickModelNode node in graph.GetNodes())
            {
                Brick brick = node.GetNodeBrick();
                BrickObjectData data = GetObjectForBrick(brick);
                if (data && !connectedObjects.Contains(data)) connectedObjects.Add(data);
            }
            return connectedObjects;
        }

        /**
         * Update logic model if bricks connect through studs
         */
        public static void ConnectStuds(
            BrickObjectData upperBrickData,
            int upperStudX,
            int upperStudZ,
            BrickObjectData lowerBrickData,
            int lowerStudX,
            int lowerStudZ
        )
        {
            if (!upperBrickData || !lowerBrickData) return;
            Brick upperBrick = upperBrickData.Brick;
            Brick lowerBrick = lowerBrickData.Brick;
            if (upperBrick == null || lowerBrick == null) return;
            Stud upperStud = upperBrick.GetStud(upperStudX, upperStudZ);
            Stud lowerStud = lowerBrick.GetStud(lowerStudX, lowerStudZ);
            if (upperStud == null || lowerStud == null) return;
            upperStud.SetNeighbourBrick(lowerBrick);
            lowerStud.SetNeighbourBrick(upperBrick);
            bool alreadyExists = Connections.Exists(connection =>
                connection.UpperBrick == upperBrick &&
                connection.UpperStudX == upperStudX &&
                connection.UpperStudZ == upperStudZ &&
                connection.LowerBrick == lowerBrick &&
                connection.LowerStudX == lowerStudX &&
                connection.LowerStudZ == lowerStudZ
            );
            if (!alreadyExists)
            {
                Connections.Add(new BrickConnection(
                    upperBrick,
                    upperStudX,
                    upperStudZ,
                    lowerBrick,
                    lowerStudX,
                    lowerStudZ
                ));
            }
            Debug.Log( //remove later
                $"Connected studs: upper {upperBrick.GetName()}[{upperStudX},{upperStudZ}] " +
                $"to lower {lowerBrick.GetName()}[{lowerStudX},{lowerStudZ}]"
            );
        }
        
        /**
         * Update logic model if bricks disconnect from studs
         */
        public static void DisconnectBrick(BrickObjectData brickData)
        {
            if (!brickData || brickData.Brick == null) return;
            Brick brick = brickData.Brick;
            Connections.RemoveAll(connection =>
                connection.UpperBrick == brick ||
                connection.LowerBrick == brick
            );
            Stud[,] studs = brick.GetStuds();
            for (int x = 0; x < studs.GetLength(0); x++)
            {
                for (int z = 0; z < studs.GetLength(1); z++)
                {
                    Stud stud = studs[x, z];
                    if (stud == null) continue;
                    Brick neighbourBrick = stud.GetNeighbourBrick();
                    if (neighbourBrick == null) continue;
                    stud.ClearNeighbourBrick();
                    Stud[,] neighbourStuds = neighbourBrick.GetStuds();
                    for (int nx = 0; nx < neighbourStuds.GetLength(0); nx++)
                    {
                        for (int nz = 0; nz < neighbourStuds.GetLength(1); nz++)
                        {
                            Stud neighbourStud = neighbourStuds[nx, nz];
                            if (neighbourStud == null) continue;
                            if (neighbourStud.GetNeighbourBrick() == brick) neighbourStud.ClearNeighbourBrick();
                        }
                    }
                }
            }
            Debug.Log("Disconnected brick from existing model: " + brick.GetName()); //remove later
        }
        
        public static void ClearAll()
        {
            ObjectByBrick.Clear();
            Connections.Clear();
        }
    }
    
    
}
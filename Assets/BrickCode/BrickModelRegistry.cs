using System.Collections.Generic;
using UnityEngine;
using ModelCode;
namespace BrickCode
{
    public static class BrickModelRegistry
    {
        private static readonly Dictionary<Brick, BrickObjectData> ObjectByBrick = new();

        public static void Register(BrickObjectData data)
        {
            if (data == null || data.Brick == null)
            {
                return;
            }

            ObjectByBrick[data.Brick] = data;
        }

        public static void Unregister(BrickObjectData data)
        {
            if (data == null || data.Brick == null)
            {
                return;
            }

            if (ObjectByBrick.TryGetValue(data.Brick, out BrickObjectData existing) && existing == data)
            {
                ObjectByBrick.Remove(data.Brick);
            }
        }

        public static BrickObjectData GetObjectForBrick(Brick brick)
        {
            if (brick == null)
            {
                return null;
            }

            ObjectByBrick.TryGetValue(brick, out BrickObjectData data);
            return data;
        }

        public static List<BrickObjectData> GetConnectedObjects(Brick startingBrick)
        {
            List<BrickObjectData> connectedObjects = new();

            if (startingBrick == null)
            {
                return connectedObjects;
            }

            BrickModelGraph graph = new BrickModelGraph(startingBrick);

            foreach (BrickModelGraph.BrickModelNode node in graph.GetNodes())
            {
                Brick brick = node.GetNodeBrick();
                BrickObjectData data = GetObjectForBrick(brick);

                if (data != null && !connectedObjects.Contains(data))
                {
                    connectedObjects.Add(data);
                }
            }

            return connectedObjects;
        }

        public static void ConnectStuds(
            BrickObjectData upperBrickData,
            int upperStudX,
            int upperStudZ,
            BrickObjectData lowerBrickData,
            int lowerStudX,
            int lowerStudZ
        )
        {
            if (upperBrickData == null || lowerBrickData == null)
            {
                return;
            }

            Brick upperBrick = upperBrickData.Brick;
            Brick lowerBrick = lowerBrickData.Brick;

            if (upperBrick == null || lowerBrick == null)
            {
                return;
            }

            Stud upperStud = upperBrick.GetStud(upperStudX, upperStudZ);
            Stud lowerStud = lowerBrick.GetStud(lowerStudX, lowerStudZ);

            if (upperStud == null || lowerStud == null)
            {
                return;
            }

            upperStud.SetNeighbourBrick(lowerBrick);
            lowerStud.SetNeighbourBrick(upperBrick);

            Debug.Log(
                $"Connected studs: upper {upperBrick.GetName()}[{upperStudX},{upperStudZ}] " +
                $"to lower {lowerBrick.GetName()}[{lowerStudX},{lowerStudZ}]"
            );
        }
    }
}
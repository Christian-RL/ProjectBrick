using UnityEngine;

namespace BrickCode
{
    /**
     * Stores information about one possible snap position for a brick.
     */
    public struct BrickSnapCandidate
    {
        public Vector3 Position; //target world position brick should move to if candidate chosen
        public Quaternion Rotation; //target rotation brick should have after snapping
        public float Score; //how good this snap candidate is, lower = better

        public bool IsStudSnap; //true if stud-to-stud snap, false if side snap or alignment

        public BrickObjectData UpperBrickData; //brick being placed on top
        public int UpperStudX;
        public int UpperStudZ;

        public BrickObjectData LowerBrickData; //brick underneath
        public int LowerStudX;
        public int LowerStudZ;

        /**
         * Constructor for snap candidates that do not connect studs.
         */
        public BrickSnapCandidate(Vector3 position, Quaternion rotation, float score)
        {
            Position = position;
            Rotation = rotation;
            Score = score;
            IsStudSnap = false;
            UpperBrickData = null; //default null values
            UpperStudX = -1;
            UpperStudZ = -1;
            LowerBrickData = null;
            LowerStudX = -1;
            LowerStudZ = -1;
        }

        /**
         * Constructor for stud-to-stud snap
         */
        public BrickSnapCandidate(
            Vector3 position,
            Quaternion rotation,
            float score,
            BrickObjectData upperBrickData,
            int upperStudX,
            int upperStudZ,
            BrickObjectData lowerBrickData,
            int lowerStudX,
            int lowerStudZ
        )
        {
            Position = position;
            Rotation = rotation;
            Score = score;
            IsStudSnap = true;
            UpperBrickData = upperBrickData;
            UpperStudX = upperStudX;
            UpperStudZ = upperStudZ;
            LowerBrickData = lowerBrickData;
            LowerStudX = lowerStudX;
            LowerStudZ = lowerStudZ;
        }
    }
}
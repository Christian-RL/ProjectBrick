using System.Collections.Generic;
using UnityEngine;

namespace BrickCode
{
    /**
     * Unity-side data component for one placed brick.
     * Connects visible brick GameObject to the logical Brick object.
     * Stores information needed for snapping, collision, movement, and model graph lookup.
     */
    public class BrickObjectData : MonoBehaviour
    {
        public const float StudSpacing = 1.0f; //scale of bricks
        public const float BrickHeightUnit = 0.4f;
        public const float StudHeight = 0.15f;

        public Brick Brick { get; private set; } //logical brick object
        public int StudWidth { get; private set; } //size properties
        public int StudLength { get; private set; }
        public int TileHeight { get; private set; }
        public float BodyHeight => TileHeight * BrickHeightUnit;
        public float TotalHeight => BodyHeight + StudHeight;
        public float WidthWorld => StudWidth * StudSpacing;
        public float LengthWorld => StudLength * StudSpacing;

        private Transform _bodyTransform;
        private Collider _bodyCollider;

        private readonly List<Transform> _studTransforms = new(); //Unity transforms of the visual studs

        /**
         * A stud position used for snapping.
         */
        public readonly struct StudAnchor
        {
            public readonly int X; //stud x index in brick stud array
            public readonly int Z; //stud y index in brick stud array
            public readonly Vector3 Position; //vector position of stud anchor

            public StudAnchor(int x, int z, Vector3 position)
            {
                X = x;
                Z = z;
                Position = position;
            }
        }

        /**
         * Set up BrickObjectData component.
         * Stores logical brick and its dimensions.
         * Adds itself to the global model registry.
         */
        public void Initialise(Brick brick)
        {
            Brick = brick;
            StudWidth = brick.GetStudWidth();
            StudLength = brick.GetStudLength();
            TileHeight = brick.GetTileHeight();
            BrickModelRegistry.Register(this);
        }

        /**
         * Remove from registry when destroyed.
         */
        private void OnDestroy()
        {
            BrickModelRegistry.Unregister(this);
        }

        /**
         * Tells which child object is the main brick body.
         */
        public void SetBody(GameObject body)
        {
            _bodyTransform = body.transform;
            _bodyCollider = body.GetComponent<Collider>();
        }

        /**
         * Removes stored stud transform references.
         */
        public void ClearStuds()
        {
            _studTransforms.Clear();
        }

        /**
         * Stores a visual stud's transform.
         */
        public void RegisterStud(Transform studTransform)
        {
            _studTransforms.Add(studTransform);
        }

        /**
         * Returns the world-space bounds of the brick body.
         * A Bounds contains center, size, min, max.
         * If there is no body collider, creates the fallback bounds manually.
         */
        public Bounds BodyBounds
        {
            get
            {
                if (_bodyCollider != null)
                {
                    return _bodyCollider.bounds;
                }
                Vector3 fallbackSize = new Vector3(WidthWorld, BodyHeight, LengthWorld);
                Vector3 fallbackCenter = transform.position + Vector3.up * (BodyHeight / 2f);
                return new Bounds(fallbackCenter, fallbackSize);
            }
        }

        public float BodyTopY => BodyBounds.max.y;
        public float BodyBottomY => BodyBounds.min.y;

        /**
         * Returns all bottom stud anchor positions in local space.
         */
        public IEnumerable<StudAnchor> GetLocalBottomStudAnchors()
        {
            for (int x = 0; x < StudWidth; x++)
            {
                for (int z = 0; z < StudLength; z++)
                {
                    float xPos = x - (StudWidth - 1) / 2f;
                    float zPos = z - (StudLength - 1) / 2f;

                    Vector3 localPosition = new Vector3(
                        xPos * StudSpacing,
                        0f,
                        zPos * StudSpacing
                    );
                    yield return new StudAnchor(x, z, localPosition);
                }
            }
        }

        /**
         * Returns all the top stud anchor positions in the world space.
         */
        public IEnumerable<StudAnchor> GetWorldTopStudAnchors()
        {
            if (_studTransforms.Count > 0)
            {
                for (int i = 0; i < _studTransforms.Count; i++)
                {
                    Transform stud = _studTransforms[i];

                    if (stud == null)
                    {
                        continue;
                    }

                    int x = i / StudLength;
                    int z = i % StudLength;

                    Vector3 worldPosition = new Vector3(
                        stud.position.x,
                        BodyTopY,
                        stud.position.z
                    );

                    yield return new StudAnchor(x, z, worldPosition);
                }
                yield break;
            }
            for (int x = 0; x < StudWidth; x++)
            {
                for (int z = 0; z < StudLength; z++)
                {
                    float xPos = x - (StudWidth - 1) / 2f;
                    float zPos = z - (StudLength - 1) / 2f;

                    Vector3 localPosition = new Vector3(
                        xPos * StudSpacing,
                        BodyHeight,
                        zPos * StudSpacing
                    );
                    yield return new StudAnchor(x, z, transform.TransformPoint(localPosition));
                }
            }
        }

        /**
         * checks whether the brick is upright.
         */
        public bool IsMostlyUpright()
        {
            return Vector3.Dot(transform.up, Vector3.up) > 0.95f;
        }
        
        public IEnumerable<StudAnchor> GetLocalTopStudAnchors()
        {
            for (int x = 0; x < StudWidth; x++)
            {
                for (int z = 0; z < StudLength; z++)
                {
                    float xPos = x - (StudWidth - 1) / 2f;
                    float zPos = z - (StudLength - 1) / 2f;

                    Vector3 localPosition = new Vector3(
                        xPos * StudSpacing,
                        BodyHeight,
                        zPos * StudSpacing
                    );

                    yield return new StudAnchor(x, z, localPosition);
                }
            }
        }
    }
}
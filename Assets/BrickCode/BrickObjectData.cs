using System.Collections.Generic;
using UnityEngine;

namespace BrickCode
{
    public class BrickObjectData : MonoBehaviour
    {
        public const float StudSpacing = 1.0f;
        public const float BrickHeightUnit = 0.4f;
        public const float StudHeight = 0.15f;

        public Brick Brick { get; private set; }

        public int StudWidth { get; private set; }
        public int StudLength { get; private set; }
        public int TileHeight { get; private set; }

        public float BodyHeight => TileHeight * BrickHeightUnit;
        public float TotalHeight => BodyHeight + StudHeight;

        public float WidthWorld => StudWidth * StudSpacing;
        public float LengthWorld => StudLength * StudSpacing;

        private Transform _bodyTransform;
        private Collider _bodyCollider;

        private readonly List<Transform> _studTransforms = new();

        public readonly struct StudAnchor
        {
            public readonly int X;
            public readonly int Z;
            public readonly Vector3 Position;

            public StudAnchor(int x, int z, Vector3 position)
            {
                X = x;
                Z = z;
                Position = position;
            }
        }

        public void Initialise(Brick brick)
        {
            Brick = brick;

            StudWidth = brick.GetStudWidth();
            StudLength = brick.GetStudLength();
            TileHeight = brick.GetTileHeight();

            BrickModelRegistry.Register(this);
        }

        private void OnDestroy()
        {
            BrickModelRegistry.Unregister(this);
        }

        public void SetBody(GameObject body)
        {
            _bodyTransform = body.transform;
            _bodyCollider = body.GetComponent<Collider>();
        }

        public void ClearStuds()
        {
            _studTransforms.Clear();
        }

        public void RegisterStud(Transform studTransform)
        {
            _studTransforms.Add(studTransform);
        }

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

        public bool IsMostlyUpright()
        {
            return Vector3.Dot(transform.up, Vector3.up) > 0.95f;
        }
    }
}
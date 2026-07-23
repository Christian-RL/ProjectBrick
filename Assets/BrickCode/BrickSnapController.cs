using System.Collections.Generic;
using UnityEngine;
using ModelCode;

namespace BrickCode
{
    /**
     * Decides whether a moving brick should snap to another brick.
     * Decides where a moving brick should snap.
     * Decides whether a successful stud snap should update the logical brick model.
     */
    public class BrickSnapController : MonoBehaviour
    {
        [Header("Snapping")]
        [SerializeField] private float topSnapDistance = 1.4f;
        [SerializeField] private float studSnapDistance = 1.0f;
        [SerializeField] private float faceSnapDistance = 1.0f;
        [SerializeField] private float verticalFaceSnapDistance = 0.8f;
        [SerializeField] private float studConnectionTolerance = 0.12f;

        private BrickObjectData _brickData; //brick data for brick controller belongs too
        private bool _hasPendingStudConnection; 
        private BrickSnapCandidate _pendingStudConnection; //stud connection detected but not yet committed

        /**
         * Get BrickObjectData from the GameObject
         */
        private void Awake()
        {
            _brickData = GetComponent<BrickObjectData>();
        }

        /**
         * Clears remembered stud connection.
         */
        public void ClearPendingStudConnection()
        {
            _hasPendingStudConnection = false;
        }

        /**
         * Try to find a better snapped position/rotation for the moving brick.
         * Return true if found a snap.
         */
        public bool TryGetSnappedPose(
            Vector3 targetPosition,
            Quaternion targetRotation,
            BrickModelMover mover,
            out Vector3 snappedPosition,
            out Quaternion snappedRotation
        )
        {
            snappedPosition = targetPosition; //assume snapped pose is the target pose
            snappedRotation = targetRotation;

            if (!_brickData || !_brickData.IsMostlyUpright())
            {
                ClearPendingStudConnection();
                return false;
            }

            Quaternion gridRotation = SnapRotationToNearestRightAngle(targetRotation);
            BrickObjectData[] allBricks = FindObjectsOfType<BrickObjectData>(); //find every brick object in the scene
            List<BrickSnapCandidate> studCandidates = new();
            List<BrickSnapCandidate> sideCandidates = new();
            foreach (BrickObjectData otherBrick in allBricks)
            {
                if (!IsValidSnapTarget(otherBrick, mover))
                {
                    continue;
                }

                AddTopSnapCandidates(targetPosition, gridRotation, otherBrick, studCandidates);
                AddUndersideSnapCandidates(targetPosition, gridRotation, otherBrick, studCandidates);
                AddSideSnapCandidates(targetPosition, gridRotation, otherBrick, sideCandidates);
            }

            if (TryChooseBestCandidate(studCandidates, out BrickSnapCandidate bestStudCandidate))
            {
                snappedPosition = bestStudCandidate.Position;
                snappedRotation = bestStudCandidate.Rotation;

                _hasPendingStudConnection = bestStudCandidate.IsStudSnap;
                _pendingStudConnection = bestStudCandidate;

                return true;
            }
            if (TryChooseBestCandidate(sideCandidates, out BrickSnapCandidate bestSideCandidate)) //if no top snap candidate then chose best side snap candidate
            {
                snappedPosition = bestSideCandidate.Position;
                snappedRotation = bestSideCandidate.Rotation;
                ClearPendingStudConnection();
                return true;
            }
            ClearPendingStudConnection();
            return false;
        }

        /**
         * Commit the logic stud connection after a visual stud snap succeeded.
         */
        public void CommitPendingStudConnection()
        {
            if (!_hasPendingStudConnection) return;
            if (!_pendingStudConnection.IsStudSnap)
            {
                ClearPendingStudConnection();
                return;
            }
            BrickObjectData upperBrickData = _pendingStudConnection.UpperBrickData;
            BrickObjectData lowerBrickData = _pendingStudConnection.LowerBrickData;
            if (!upperBrickData || !lowerBrickData)
            {
                ClearPendingStudConnection();
                return;
            }
            int connectionsMade = 0;
            foreach (BrickObjectData.StudAnchor upperStud in upperBrickData.GetLocalBottomStudAnchors())
            {
                Vector3 upperStudWorld = upperBrickData.transform.TransformPoint(upperStud.Position);

                foreach (BrickObjectData.StudAnchor lowerStud in lowerBrickData.GetWorldTopStudAnchors())
                {
                    Vector3 horizontalDifference = lowerStud.Position - upperStudWorld;
                    horizontalDifference.y = 0f;
                    if (horizontalDifference.magnitude > studConnectionTolerance) continue;
                    BrickModelRegistry.ConnectStuds(
                        upperBrickData,
                        upperStud.X,
                        upperStud.Z,
                        lowerBrickData,
                        lowerStud.X,
                        lowerStud.Z
                    );
                    connectionsMade++;
                }
            }

            Debug.Log($"Committed stud snap connection. Stud connections made: {connectionsMade}");
            ClearPendingStudConnection();
        }

        /**
         * Checks if another brick should be considered a snapping target.
         */
        private bool IsValidSnapTarget(BrickObjectData otherBrick, BrickModelMover mover)
        {
            if (!otherBrick || otherBrick == _brickData) return false;
            if (!otherBrick.GetComponent<DraggableBrick3D>()) return false;
            if (!otherBrick.IsMostlyUpright()) return false;
            if (mover && mover.IsMovingObject(otherBrick)) return false;
            return true;
        }

        /**
         * Adds valid top snap candidates to candidates list.
         */
        private void AddTopSnapCandidates(
            Vector3 targetPosition,
            Quaternion targetRotation,
            BrickObjectData otherBrick,
            List<BrickSnapCandidate> candidates
        )
        {
            float snapY = otherBrick.BodyTopY;
            float verticalDistance = Mathf.Abs(targetPosition.y - snapY);
            if (verticalDistance > topSnapDistance) return;
            foreach (BrickObjectData.StudAnchor ownBottomStud in _brickData.GetLocalBottomStudAnchors())
            {
                Vector3 ownBottomStudOffset = targetRotation * ownBottomStud.Position;
                foreach (BrickObjectData.StudAnchor otherTopStud in otherBrick.GetWorldTopStudAnchors())
                {
                    Vector3 candidatePosition = otherTopStud.Position - ownBottomStudOffset;
                    candidatePosition.y = snapY;
                    Vector3 horizontalDifference = candidatePosition - targetPosition;
                    horizontalDifference.y = 0f;
                    float horizontalDistance = horizontalDifference.magnitude;
                    if (horizontalDistance > studSnapDistance) continue;
                    float score = horizontalDistance + verticalDistance * 0.25f;
                    candidates.Add(new BrickSnapCandidate(
                        candidatePosition,
                        targetRotation,
                        score,
                        _brickData,
                        ownBottomStud.X,
                        ownBottomStud.Z,
                        otherBrick,
                        otherTopStud.X,
                        otherTopStud.Z
                    ));
                }
            }
        }

        /**
         * Adds valid side snap candidates to candidates list.
         */
        private void AddSideSnapCandidates(
            Vector3 targetPosition,
            Quaternion targetRotation,
            BrickObjectData otherBrick,
            List<BrickSnapCandidate> candidates
        )
        {
            Bounds otherBounds = otherBrick.BodyBounds;
            float snapY = otherBrick.BodyBottomY;
            float verticalDistance = Mathf.Abs(targetPosition.y - snapY);
            if (verticalDistance > verticalFaceSnapDistance) return;
            Vector2 ownHalf = GetWorldFootprintHalfSize(_brickData, targetRotation);
            float otherCenterX = otherBounds.center.x;
            float otherCenterZ = otherBounds.center.z;
            Vector3 rightCandidate = new Vector3(
                otherBounds.max.x + ownHalf.x,
                snapY,
                SnapToStudGrid(targetPosition.z, otherCenterZ)
            );
            Vector3 leftCandidate = new Vector3(
                otherBounds.min.x - ownHalf.x,
                snapY,
                SnapToStudGrid(targetPosition.z, otherCenterZ)
            );
            Vector3 frontCandidate = new Vector3(
                SnapToStudGrid(targetPosition.x, otherCenterX),
                snapY,
                otherBounds.max.z + ownHalf.y
            );
            Vector3 backCandidate = new Vector3(
                SnapToStudGrid(targetPosition.x, otherCenterX),
                snapY,
                otherBounds.min.z - ownHalf.y
            );
            AddSideCandidateIfClose(targetPosition, targetRotation, rightCandidate, candidates);
            AddSideCandidateIfClose(targetPosition, targetRotation, leftCandidate, candidates);
            AddSideCandidateIfClose(targetPosition, targetRotation, frontCandidate, candidates);
            AddSideCandidateIfClose(targetPosition, targetRotation, backCandidate, candidates);
        }

        /**
         * Checks one side snap candidate.
         * Measures how far candidate is from current target position, skips if it is too far away.
         */
        private void AddSideCandidateIfClose(
            Vector3 targetPosition,
            Quaternion targetRotation,
            Vector3 candidatePosition,
            List<BrickSnapCandidate> candidates
        )
        {
            float distance = Vector3.Distance(candidatePosition, targetPosition);
            if (distance > faceSnapDistance) return;
            candidates.Add(new BrickSnapCandidate(
                candidatePosition,
                targetRotation,
                distance
            ));
        }
        
        /**
         * Selects the candidate with the lowest score.
         * Returns true if it found a candidate, false if list was empty.
         */
        private bool TryChooseBestCandidate(List<BrickSnapCandidate> candidates, out BrickSnapCandidate bestCandidate)
        {
            bestCandidate = default;
            if (candidates.Count == 0) return false;
            float bestScore = float.MaxValue;
            foreach (BrickSnapCandidate candidate in candidates)
            {
                if (candidate.Score < bestScore)
                {
                    bestScore = candidate.Score;
                    bestCandidate = candidate;
                }
            }
            return true;
        }

        /**
         * Snaps brick rotation to nearest 90 degree Y rotation.
         */
        private Quaternion SnapRotationToNearestRightAngle(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            float snappedY = Mathf.Round(euler.y / 90f) * 90f;
            return Quaternion.Euler(0f, snappedY, 0f);
        }

        /**
         * Calculates moving brick's half-size in world space with rotation taken into account.
         */
        private Vector2 GetWorldFootprintHalfSize(BrickObjectData data, Quaternion rotation)
        {
            float halfWidth = data.WidthWorld / 2f;
            float halfLength = data.LengthWorld / 2f;
            Vector3 right = rotation * Vector3.right;
            Vector3 forward = rotation * Vector3.forward;
            float halfX =
                Mathf.Abs(right.x) * halfWidth +
                Mathf.Abs(forward.x) * halfLength;
            float halfZ =
                Mathf.Abs(right.z) * halfWidth +
                Mathf.Abs(forward.z) * halfLength;
            return new Vector2(halfX, halfZ);
        }

        /**
         * Snaps coordinate to nearest stud grid position.
         */
        private float SnapToStudGrid(float value, float gridOrigin)
        {
            float spacing = BrickObjectData.StudSpacing;
            return gridOrigin + Mathf.Round((value - gridOrigin) / spacing) * spacing;
        }
        
        private void AddUndersideSnapCandidates(
            Vector3 targetPosition,
            Quaternion targetRotation,
            BrickObjectData otherBrick,
            List<BrickSnapCandidate> candidates
        )
        {
            float snapTopY = otherBrick.BodyBottomY;
            float movingBrickTopY = targetPosition.y + _brickData.BodyHeight;

            float verticalDistance = Mathf.Abs(movingBrickTopY - snapTopY);

            if (verticalDistance > topSnapDistance)
            {
                return;
            }

            foreach (BrickObjectData.StudAnchor ownTopStud in _brickData.GetLocalTopStudAnchors())
            {
                Vector3 ownTopStudOffset = targetRotation * ownTopStud.Position;

                foreach (BrickObjectData.StudAnchor otherBottomStud in otherBrick.GetLocalBottomStudAnchors())
                {
                    Vector3 otherBottomWorld = otherBrick.transform.TransformPoint(otherBottomStud.Position);
                    otherBottomWorld.y = snapTopY;

                    Vector3 candidatePosition = otherBottomWorld - ownTopStudOffset;

                    candidatePosition.y = snapTopY - _brickData.BodyHeight;

                    Vector3 horizontalDifference = candidatePosition - targetPosition;
                    horizontalDifference.y = 0f;

                    float horizontalDistance = horizontalDifference.magnitude;

                    if (horizontalDistance > studSnapDistance)
                    {
                        continue;
                    }

                    float score = horizontalDistance + verticalDistance * 0.25f;

                    candidates.Add(new BrickSnapCandidate(
                        candidatePosition,
                        targetRotation,
                        score,

                        // The existing brick is above.
                        otherBrick,
                        otherBottomStud.X,
                        otherBottomStud.Z,

                        // The moving brick is below.
                        _brickData,
                        ownTopStud.X,
                        ownTopStud.Z
                    ));
                }
            }
        }
    }
}
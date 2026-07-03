using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ModelCode;
namespace BrickCode
{
    public class DraggableBrick3D : MonoBehaviour
    {
        public static bool IsAnyBrickBeingDragged { get; private set; }
        public static bool IsAnyBrickBeingMoved { get; private set; }
        public static bool IsAnyBrickInRotateMode { get; private set; }

        [Header("Dragging")]
        [SerializeField] private float scrollDistanceSpeed = 0.08f;
        [SerializeField] private float minHoldDistance = 0.5f;
        [SerializeField] private float maxHoldDistance = 100f;

        [Header("Mouse Rotation")]
        [SerializeField] private float mouseRotationSensitivity = 0.35f;
        [SerializeField] private float rotationSnapAngle = 12f;

        [Header("Snapping")]
        [SerializeField] private float topSnapDistance = 1.4f;
        [SerializeField] private float studSnapDistance = 1.0f;
        [SerializeField] private float faceSnapDistance = 1.0f;
        [SerializeField] private float verticalFaceSnapDistance = 0.8f;
        [SerializeField] private float studConnectionTolerance = 0.12f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private int collisionResolveIterations = 6;
        [SerializeField] private float collisionSkin = 0.001f;

        private Camera _camera;
        private BrickObjectData _brickData;

        private bool _isDragging;
        private bool _isRotateMode;
        private bool _isMouseRotating;

        private float _holdDistance;
        private Vector3 _dragOffset;

        private Vector2 _rotationStartMousePosition;
        private Quaternion _rotationStartRotation;

        private bool _hasPendingStudConnection;
        private SnapCandidate _pendingStudConnection;

        private readonly HashSet<Collider> _ownColliders = new();

        private readonly List<BrickObjectData> _movingModelObjects = new();
        private readonly HashSet<Collider> _movingModelColliders = new();

        private struct SnapCandidate
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public float Score;

            public bool IsStudSnap;

            public BrickObjectData UpperBrickData;
            public int UpperStudX;
            public int UpperStudZ;

            public BrickObjectData LowerBrickData;
            public int LowerStudX;
            public int LowerStudZ;

            public SnapCandidate(Vector3 position, Quaternion rotation, float score)
            {
                Position = position;
                Rotation = rotation;
                Score = score;

                IsStudSnap = false;

                UpperBrickData = null;
                UpperStudX = -1;
                UpperStudZ = -1;

                LowerBrickData = null;
                LowerStudX = -1;
                LowerStudZ = -1;
            }

            public SnapCandidate(
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

        private void Start()
        {
            _camera = Camera.main;
            _brickData = GetComponent<BrickObjectData>();

            if (_camera == null)
            {
                Debug.LogError("No MainCamera found. Make sure your camera is tagged MainCamera.");
            }

            if (_brickData == null)
            {
                Debug.LogWarning("No BrickObjectData found on " + gameObject.name);
            }

            CacheOwnColliders();
        }

        private void Update()
        {
            if (_camera == null || Mouse.current == null)
            {
                return;
            }

            HandleRotateModeToggle();

            if (_isRotateMode)
            {
                HandleRotateMode();
                return;
            }

            HandleMoveMode();
        }

        private void HandleRotateModeToggle()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (!Keyboard.current.rKey.wasPressedThisFrame)
            {
                return;
            }

            if (_isRotateMode)
            {
                ExitRotateMode();
                return;
            }

            if (_isDragging)
            {
                EnterRotateMode();
            }
        }

        private void EnterRotateMode()
        {
            _isRotateMode = true;
            _isDragging = false;
            _isMouseRotating = false;

            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;
            IsAnyBrickInRotateMode = true;

            Debug.Log("Entered brick rotate mode. Left click + drag to rotate. Hold Alt/Option to move camera. Press R again to exit.");
        }

        private void ExitRotateMode()
        {
            _isRotateMode = false;
            _isDragging = false;
            _isMouseRotating = false;

            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;
            IsAnyBrickInRotateMode = false;

            ClearMovingModel();

            Debug.Log("Exited brick rotate mode.");
        }

        private void HandleMoveMode()
        {
            if (IsAnyBrickInRotateMode)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryStartDragging();
            }

            if (Mouse.current.leftButton.isPressed && _isDragging)
            {
                UpdateHoldDistanceFromScroll();
                DragAtHoldDistance();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                StopDragging();
            }
        }

        private void HandleRotateMode()
        {
            if (CameraControlInputIsActive())
            {
                _isMouseRotating = false;
                IsAnyBrickBeingMoved = false;
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isMouseRotating = true;
                IsAnyBrickBeingMoved = true;

                _rotationStartMousePosition = Mouse.current.position.ReadValue();
                _rotationStartRotation = transform.rotation;
            }

            if (Mouse.current.leftButton.isPressed && _isMouseRotating)
            {
                IsAnyBrickBeingMoved = true;
                RotateWithMouseDrag();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isMouseRotating = false;
                IsAnyBrickBeingMoved = false;
            }
        }

        private bool CameraControlInputIsActive()
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                return false;
            }

            bool altHeld =
                Keyboard.current.leftAltKey.isPressed ||
                Keyboard.current.rightAltKey.isPressed;

            bool middleMouseHeld = Mouse.current.middleButton.isPressed;
            bool rightMouseHeld = Mouse.current.rightButton.isPressed;

            return altHeld || middleMouseHeld || rightMouseHeld;
        }

        private void RotateWithMouseDrag()
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePosition - _rotationStartMousePosition;

            float yawAmount = mouseDelta.x * mouseRotationSensitivity;
            float pitchAmount = -mouseDelta.y * mouseRotationSensitivity;

            Quaternion yawRotation = Quaternion.AngleAxis(yawAmount, Vector3.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitchAmount, _camera.transform.right);

            Quaternion targetRotation = yawRotation * pitchRotation * _rotationStartRotation;

            if (RotationSnappingEnabled())
            {
                targetRotation = SnapRotationToDefaultPositionsIfClose(targetRotation);
            }

            RotateModelWithSlidingCollision(targetRotation);
        }

        private bool RotationSnappingEnabled()
        {
            if (Keyboard.current == null)
            {
                return true;
            }

            bool ctrlHeld =
                Keyboard.current.leftCtrlKey.isPressed ||
                Keyboard.current.rightCtrlKey.isPressed;

            return !ctrlHeld;
        }

        private Quaternion SnapRotationToDefaultPositionsIfClose(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;

            Vector3 snappedEuler = new Vector3(
                Mathf.Round(euler.x / 90f) * 90f,
                Mathf.Round(euler.y / 90f) * 90f,
                Mathf.Round(euler.z / 90f) * 90f
            );

            Quaternion snappedRotation = Quaternion.Euler(snappedEuler);

            float angleDifference = Quaternion.Angle(rotation, snappedRotation);

            if (angleDifference <= rotationSnapAngle)
            {
                return snappedRotation;
            }

            return rotation;
        }

        private void TryStartDragging()
        {
            _hasPendingStudConnection = false;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, collisionMask))
            {
                return;
            }

            DraggableBrick3D draggable = hit.collider.GetComponentInParent<DraggableBrick3D>();

            if (draggable != this)
            {
                return;
            }

            CacheOwnColliders();

            _isDragging = true;
            IsAnyBrickBeingDragged = true;
            IsAnyBrickBeingMoved = true;

            _holdDistance = Vector3.Distance(_camera.transform.position, hit.point);

            Vector3 holdPoint = ray.GetPoint(_holdDistance);
            _dragOffset = transform.position - holdPoint;

            PrepareMovingModel();
        }

        private void PrepareMovingModel()
        {
            _movingModelObjects.Clear();
            _movingModelColliders.Clear();

            if (_brickData == null || _brickData.Brick == null)
            {
                if (_brickData != null)
                {
                    _movingModelObjects.Add(_brickData);
                }

                CacheMovingModelColliders();
                return;
            }

            List<BrickObjectData> connectedObjects =
                BrickModelRegistry.GetConnectedObjects(_brickData.Brick);

            foreach (BrickObjectData connectedObject in connectedObjects)
            {
                if (connectedObject == null)
                {
                    continue;
                }

                if (!_movingModelObjects.Contains(connectedObject))
                {
                    _movingModelObjects.Add(connectedObject);
                }
            }

            if (_movingModelObjects.Count == 0)
            {
                _movingModelObjects.Add(_brickData);
            }

            CacheMovingModelColliders();
        }

        private void CacheMovingModelColliders()
        {
            _movingModelColliders.Clear();

            foreach (BrickObjectData movingObject in _movingModelObjects)
            {
                if (movingObject == null)
                {
                    continue;
                }

                Collider[] colliders = movingObject.GetComponentsInChildren<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (collider != null)
                    {
                        _movingModelColliders.Add(collider);
                    }
                }
            }
        }

        private void ClearMovingModel()
        {
            _movingModelObjects.Clear();
            _movingModelColliders.Clear();
        }

        private void UpdateHoldDistanceFromScroll()
        {
            float scrollAmount = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scrollAmount) < 0.01f)
            {
                return;
            }

            _holdDistance += scrollAmount * scrollDistanceSpeed;
            _holdDistance = Mathf.Clamp(_holdDistance, minHoldDistance, maxHoldDistance);
        }

        private void DragAtHoldDistance()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            Vector3 holdPoint = ray.GetPoint(_holdDistance);
            Vector3 targetPosition = holdPoint + _dragOffset;
            Quaternion targetRotation = transform.rotation;

            _hasPendingStudConnection = false;

            if (SnappingEnabled())
            {
                if (TryGetSnappedPose(
                        targetPosition,
                        targetRotation,
                        out Vector3 snappedPosition,
                        out Quaternion snappedRotation
                    ))
                {
                    targetPosition = snappedPosition;
                    targetRotation = snappedRotation;
                }
            }

            bool movedSuccessfully = MoveModelWithSlidingCollision(targetPosition, targetRotation);

            if (!movedSuccessfully)
            {
                _hasPendingStudConnection = false;
            }
        }

        private bool SnappingEnabled()
        {
            if (Keyboard.current == null)
            {
                return true;
            }

            bool ctrlHeld =
                Keyboard.current.leftCtrlKey.isPressed ||
                Keyboard.current.rightCtrlKey.isPressed;

            return !ctrlHeld;
        }

        private bool TryGetSnappedPose(
            Vector3 targetPosition,
            Quaternion targetRotation,
            out Vector3 snappedPosition,
            out Quaternion snappedRotation
        )
        {
            snappedPosition = targetPosition;
            snappedRotation = targetRotation;

            if (_brickData == null || !_brickData.IsMostlyUpright())
            {
                _hasPendingStudConnection = false;
                return false;
            }

            Quaternion gridRotation = SnapRotationToNearestRightAngle(targetRotation);

            BrickObjectData[] allBricks = FindObjectsOfType<BrickObjectData>();

            List<SnapCandidate> topCandidates = new();
            List<SnapCandidate> sideCandidates = new();

            foreach (BrickObjectData otherBrick in allBricks)
            {
                if (!IsValidSnapTarget(otherBrick))
                {
                    continue;
                }

                AddTopSnapCandidates(targetPosition, gridRotation, otherBrick, topCandidates);
                AddSideSnapCandidates(targetPosition, gridRotation, otherBrick, sideCandidates);
            }

            if (TryChooseBestCandidate(topCandidates, out SnapCandidate bestTopCandidate))
            {
                snappedPosition = bestTopCandidate.Position;
                snappedRotation = bestTopCandidate.Rotation;

                _hasPendingStudConnection = bestTopCandidate.IsStudSnap;
                _pendingStudConnection = bestTopCandidate;

                return true;
            }

            if (TryChooseBestCandidate(sideCandidates, out SnapCandidate bestSideCandidate))
            {
                snappedPosition = bestSideCandidate.Position;
                snappedRotation = bestSideCandidate.Rotation;

                _hasPendingStudConnection = false;

                return true;
            }

            _hasPendingStudConnection = false;
            return false;
        }

        private bool IsValidSnapTarget(BrickObjectData otherBrick)
        {
            if (otherBrick == null || otherBrick == _brickData)
            {
                return false;
            }

            if (otherBrick.GetComponent<DraggableBrick3D>() == null)
            {
                return false;
            }

            if (!otherBrick.IsMostlyUpright())
            {
                return false;
            }

            // Do not snap to another brick already inside the model currently being moved.
            if (_movingModelObjects.Contains(otherBrick))
            {
                return false;
            }

            return true;
        }

        private void AddTopSnapCandidates(
            Vector3 targetPosition,
            Quaternion targetRotation,
            BrickObjectData otherBrick,
            List<SnapCandidate> candidates
        )
        {
            float snapY = otherBrick.BodyTopY;
            float verticalDistance = Mathf.Abs(targetPosition.y - snapY);

            if (verticalDistance > topSnapDistance)
            {
                return;
            }

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

                    if (horizontalDistance > studSnapDistance)
                    {
                        continue;
                    }

                    float score = horizontalDistance + verticalDistance * 0.25f;

                    candidates.Add(new SnapCandidate(
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

        private void AddSideSnapCandidates(
            Vector3 targetPosition,
            Quaternion targetRotation,
            BrickObjectData otherBrick,
            List<SnapCandidate> candidates
        )
        {
            Bounds otherBounds = otherBrick.BodyBounds;

            float snapY = otherBrick.BodyBottomY;
            float verticalDistance = Mathf.Abs(targetPosition.y - snapY);

            if (verticalDistance > verticalFaceSnapDistance)
            {
                return;
            }

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

        private void AddSideCandidateIfClose(
            Vector3 targetPosition,
            Quaternion targetRotation,
            Vector3 candidatePosition,
            List<SnapCandidate> candidates
        )
        {
            float distance = Vector3.Distance(candidatePosition, targetPosition);

            if (distance > faceSnapDistance)
            {
                return;
            }

            candidates.Add(new SnapCandidate(
                candidatePosition,
                targetRotation,
                distance
            ));
        }

        private bool TryChooseBestCandidate(List<SnapCandidate> candidates, out SnapCandidate bestCandidate)
        {
            bestCandidate = default;

            if (candidates.Count == 0)
            {
                return false;
            }

            float bestScore = float.MaxValue;

            foreach (SnapCandidate candidate in candidates)
            {
                if (candidate.Score < bestScore)
                {
                    bestScore = candidate.Score;
                    bestCandidate = candidate;
                }
            }

            return true;
        }

        private Quaternion SnapRotationToNearestRightAngle(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            float snappedY = Mathf.Round(euler.y / 90f) * 90f;

            return Quaternion.Euler(0f, snappedY, 0f);
        }

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

        private float SnapToStudGrid(float value, float gridOrigin)
        {
            float spacing = BrickObjectData.StudSpacing;
            return gridOrigin + Mathf.Round((value - gridOrigin) / spacing) * spacing;
        }

        private bool MoveModelWithSlidingCollision(Vector3 selectedBrickTargetPosition, Quaternion selectedBrickTargetRotation)
        {
            if (_movingModelObjects.Count <= 1)
            {
                return MoveSingleBrickWithSlidingCollision(selectedBrickTargetPosition, selectedBrickTargetRotation);
            }

            Dictionary<BrickObjectData, Vector3> oldPositions = new();
            Dictionary<BrickObjectData, Quaternion> oldRotations = new();

            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                oldPositions[obj] = obj.transform.position;
                oldRotations[obj] = obj.transform.rotation;
            }

            Vector3 selectedMoveDelta = selectedBrickTargetPosition - transform.position;

            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                obj.transform.position += selectedMoveDelta;
            }

            // When moving a connected model, keep the model's relative rotations.
            // Rotation mode handles rotating the whole model separately.
            Physics.SyncTransforms();

            bool resolved = ResolveOverlapsByPushingOut();

            if (!resolved)
            {
                foreach (BrickObjectData obj in _movingModelObjects)
                {
                    if (obj == null)
                    {
                        continue;
                    }

                    obj.transform.SetPositionAndRotation(oldPositions[obj], oldRotations[obj]);
                }

                Physics.SyncTransforms();
                return false;
            }

            return true;
        }

        private bool MoveSingleBrickWithSlidingCollision(Vector3 targetPosition, Quaternion targetRotation)
        {
            Vector3 oldPosition = transform.position;
            Quaternion oldRotation = transform.rotation;

            transform.SetPositionAndRotation(targetPosition, targetRotation);
            Physics.SyncTransforms();

            bool resolved = ResolveOverlapsByPushingOut();

            if (!resolved)
            {
                transform.SetPositionAndRotation(oldPosition, oldRotation);
                Physics.SyncTransforms();
                return false;
            }

            return true;
        }

        private bool RotateModelWithSlidingCollision(Quaternion selectedTargetRotation)
        {
            if (_movingModelObjects.Count <= 1)
            {
                return MoveSingleBrickWithSlidingCollision(transform.position, selectedTargetRotation);
            }

            Dictionary<BrickObjectData, Vector3> oldPositions = new();
            Dictionary<BrickObjectData, Quaternion> oldRotations = new();

            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                oldPositions[obj] = obj.transform.position;
                oldRotations[obj] = obj.transform.rotation;
            }

            Quaternion rotationDelta = selectedTargetRotation * Quaternion.Inverse(transform.rotation);
            Vector3 pivot = transform.position;

            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                Vector3 relativePosition = obj.transform.position - pivot;

                obj.transform.position = pivot + rotationDelta * relativePosition;
                obj.transform.rotation = rotationDelta * obj.transform.rotation;
            }

            Physics.SyncTransforms();

            bool resolved = ResolveOverlapsByPushingOut();

            if (!resolved)
            {
                foreach (BrickObjectData obj in _movingModelObjects)
                {
                    if (obj == null)
                    {
                        continue;
                    }

                    obj.transform.SetPositionAndRotation(oldPositions[obj], oldRotations[obj]);
                }

                Physics.SyncTransforms();
                return false;
            }

            return true;
        }

        private bool ResolveOverlapsByPushingOut()
        {
            for (int i = 0; i < collisionResolveIterations; i++)
            {
                Vector3 totalCorrection = Vector3.zero;
                int correctionCount = 0;

                foreach (Collider ownCollider in GetActiveMovingColliders())
                {
                    if (ownCollider == null || !ownCollider.enabled)
                    {
                        continue;
                    }

                    Collider[] nearbyColliders = Physics.OverlapBox(
                        ownCollider.bounds.center,
                        ownCollider.bounds.extents,
                        Quaternion.identity,
                        collisionMask,
                        QueryTriggerInteraction.Ignore
                    );

                    foreach (Collider otherCollider in nearbyColliders)
                    {
                        if (!IsValidCollisionTarget(otherCollider))
                        {
                            continue;
                        }

                        bool overlapping = Physics.ComputePenetration(
                            ownCollider,
                            ownCollider.transform.position,
                            ownCollider.transform.rotation,
                            otherCollider,
                            otherCollider.transform.position,
                            otherCollider.transform.rotation,
                            out Vector3 direction,
                            out float distance
                        );

                        if (!overlapping)
                        {
                            continue;
                        }

                        totalCorrection += direction * (distance + collisionSkin);
                        correctionCount++;
                    }
                }

                if (correctionCount == 0)
                {
                    return true;
                }

                MoveActiveModelBy(totalCorrection);
                Physics.SyncTransforms();
            }

            return !IsOverlappingOtherObjects();
        }

        private IEnumerable<Collider> GetActiveMovingColliders()
        {
            if (_movingModelColliders.Count > 0)
            {
                return _movingModelColliders;
            }

            return _ownColliders;
        }

        private void MoveActiveModelBy(Vector3 correction)
        {
            if (_movingModelObjects.Count > 0)
            {
                foreach (BrickObjectData obj in _movingModelObjects)
                {
                    if (obj == null)
                    {
                        continue;
                    }

                    obj.transform.position += correction;
                }

                return;
            }

            transform.position += correction;
        }

        private bool IsValidCollisionTarget(Collider otherCollider)
        {
            if (otherCollider == null || !otherCollider.enabled)
            {
                return false;
            }

            if (_ownColliders.Contains(otherCollider))
            {
                return false;
            }

            if (_movingModelColliders.Contains(otherCollider))
            {
                return false;
            }

            if (otherCollider.GetComponentInParent<DraggableBrick3D>() == null)
            {
                return false;
            }

            return true;
        }

        private bool IsOverlappingOtherObjects()
        {
            foreach (Collider ownCollider in GetActiveMovingColliders())
            {
                if (ownCollider == null || !ownCollider.enabled)
                {
                    continue;
                }

                Collider[] nearbyColliders = Physics.OverlapBox(
                    ownCollider.bounds.center,
                    ownCollider.bounds.extents,
                    Quaternion.identity,
                    collisionMask,
                    QueryTriggerInteraction.Ignore
                );

                foreach (Collider otherCollider in nearbyColliders)
                {
                    if (!IsValidCollisionTarget(otherCollider))
                    {
                        continue;
                    }

                    bool overlapping = Physics.ComputePenetration(
                        ownCollider,
                        ownCollider.transform.position,
                        ownCollider.transform.rotation,
                        otherCollider,
                        otherCollider.transform.position,
                        otherCollider.transform.rotation,
                        out _,
                        out _
                    );

                    if (overlapping)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CacheOwnColliders()
        {
            _ownColliders.Clear();

            Collider[] colliders = GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
            {
                if (collider != null)
                {
                    _ownColliders.Add(collider);
                }
            }
        }

        private void CommitPendingStudConnection()
        {
            if (!_hasPendingStudConnection)
            {
                return;
            }

            if (!_pendingStudConnection.IsStudSnap)
            {
                _hasPendingStudConnection = false;
                return;
            }

            BrickObjectData upperBrickData = _pendingStudConnection.UpperBrickData;
            BrickObjectData lowerBrickData = _pendingStudConnection.LowerBrickData;

            if (upperBrickData == null || lowerBrickData == null)
            {
                _hasPendingStudConnection = false;
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

                    if (horizontalDifference.magnitude > studConnectionTolerance)
                    {
                        continue;
                    }

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

            _hasPendingStudConnection = false;
        }

        private void StopDragging()
        {
            CommitPendingStudConnection();

            _isDragging = false;

            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;

            ClearMovingModel();
        }

        private void OnDisable()
        {
            _isDragging = false;
            _isRotateMode = false;
            _isMouseRotating = false;

            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;
            IsAnyBrickInRotateMode = false;

            _hasPendingStudConnection = false;

            ClearMovingModel();
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace BrickCode
{
    /**
     * Main coordinator/controller for brick interaction.
     * Determines:
     * -    Is brick being dragged.
     * -    Is brick in rotate mode.
     * -    Should it start movement.
     * -    Should it apply snapping.
     * -    Should it rotate.
     * -    Should it stop and commit snap connection.
     * Requires:
     * -    BrickInputBindings
     * -    BrickModelMover
     * -    BrickSnapController
     * -    BrickRotationController
     */
    [RequireComponent(typeof(BrickInputBindings))]
    [RequireComponent(typeof(BrickModelMover))]
    [RequireComponent(typeof(BrickSnapController))]
    [RequireComponent(typeof(BrickRotationController))]
    public class DraggableBrick3D : MonoBehaviour
    {
        public static bool IsAnyBrickBeingDragged { get; private set; }
        public static bool IsAnyBrickBeingMoved { get; private set; }
        public static bool IsAnyBrickInRotateMode { get; private set; }

        [Header("Dragging")] //how far away the brick is held from camera while dragging
        [SerializeField] private float scrollDistanceSpeed = 0.08f;
        [SerializeField] private float minHoldDistance = 0.5f;
        [SerializeField] private float maxHoldDistance = 100f;

        private Camera _camera;
        private BrickObjectData _brickData;

        private BrickInputBindings _input;
        private BrickModelMover _mover;
        private BrickSnapController _snapper;
        private BrickRotationController _rotator;

        private bool _isDragging;
        private bool _isRotateMode;
        private bool _isMouseRotating;
        private bool _isSingleBrickDrag;

        private float _holdDistance;
        private Vector3 _dragOffset;

        /**
         * Get references to all helper components.
         */
        private void Awake()
        {
            _input = GetOrAdd<BrickInputBindings>();
            _mover = GetOrAdd<BrickModelMover>();
            _snapper = GetOrAdd<BrickSnapController>();
            _rotator = GetOrAdd<BrickRotationController>();
            _brickData = GetComponent<BrickObjectData>();
        }

        /**
         * Checks required components exist.
         */
        private void Start()
        {
            _camera = Camera.main;
            if (!_camera) Debug.LogError("No MainCamera found. Make sure your camera is tagged MainCamera.");
            if (!_brickData) Debug.LogWarning("No BrickObjectData found on " + gameObject.name);
        }

        /**
         * Runs every frame.
         * Flow:
         * -    Check valid input.
         * -    Check if rotate mode should cancel.
         * -    Check if toggled into rotate mode.
         * -    Handle rotation or movement.
         */
        private void Update()
        {
            if (!_camera || Mouse.current == null) return;
            ExitRotateModeIfDeselected();
            HandleRotateModeToggle();
            if (_isRotateMode)
            {
                HandleRotateMode();
                return;
            }
            HandleMoveMode();
        }

        /**
         * Exit rotate mode if in rotate mode.
         */
        private void ExitRotateModeIfDeselected()
        {
            if (!_isRotateMode) return;
            if (!BrickSelectionManager.Instance) return;
            if (BrickSelectionManager.Instance.IsSelected(_brickData)) return;
            ExitRotateMode();
        }

        /**
         * Handles pressing the rotate mode key.
         */
        private void HandleRotateModeToggle()
        {
            if (!_input.RotateModePressedThisFrame()) return;
            if (_isRotateMode)
            {
                ExitRotateMode();
                return;
            }
            if (_isDragging) EnterRotateMode();
        }

        /**
         * Handle normal brick movement.
         */
        private void HandleMoveMode()
        {
            if (IsAnyBrickInRotateMode)
            {
                return;
            }

            if (!_isDragging)
            {
                if (_input.LeftMousePressedThisFrame())
                {
                    TryStartDragging(false);
                }
                else if (_input.RightMousePressedThisFrame())
                {
                    TryStartDragging(true);
                }
            }

            if (!_isDragging)
            {
                return;
            }

            bool dragButtonHeld = _isSingleBrickDrag
                ? _input.RightMouseHeld()
                : _input.LeftMouseHeld();

            bool dragButtonReleased = _isSingleBrickDrag
                ? _input.RightMouseReleasedThisFrame()
                : _input.LeftMouseReleasedThisFrame();

            if (dragButtonHeld)
            {
                UpdateHoldDistanceFromScroll();
                DragAtHoldDistance();
            }

            if (dragButtonReleased)
            {
                StopDragging();
            }
        }

        /**
         * Handle mouse rotation while in rotate mode.
         */
        private void HandleRotateMode()
        {
            if (_input.CameraControlInputIsActive())
            {
                _isMouseRotating = false;
                IsAnyBrickBeingMoved = false;
                return;
            }
            if (_input.LeftMousePressedThisFrame())
            {
                _isMouseRotating = true;
                IsAnyBrickBeingMoved = true;
                _rotator.BeginRotation(
                    _input.MousePosition(),
                    transform.rotation
                );
            }
            if (_input.LeftMouseHeld() && _isMouseRotating)
            {
                IsAnyBrickBeingMoved = true;
                Quaternion targetRotation = _rotator.GetTargetRotation(
                    _camera,
                    _input.MousePosition(),
                    RotationSnappingEnabled()
                );
                _mover.RotateModelWithSlidingCollision(targetRotation);
            }
            if (_input.LeftMouseReleasedThisFrame())
            {
                _isMouseRotating = false;
                IsAnyBrickBeingMoved = false;
            }
        }

        /**
         * Try to begin dragging the brick.
         * Checks if the user actually clicked the brick first.
         */
        private void TryStartDragging(bool singleBrickOnly)
        {
            _snapper.ClearPendingStudConnection();

            Ray ray = _camera.ScreenPointToRay(_input.MousePosition());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _mover.CollisionMask))
            {
                return;
            }

            DraggableBrick3D draggable = hit.collider.GetComponentInParent<DraggableBrick3D>();

            if (draggable != this)
            {
                return;
            }

            _isSingleBrickDrag = singleBrickOnly;

            if (singleBrickOnly)
            {
                BrickModelRegistry.DisconnectBrick(_brickData);

                if (BrickSelectionManager.Instance)
                {
                    BrickSelectionManager.Instance.SelectSingleBrick(_brickData);
                }
            }
            else
            {
                if (BrickSelectionManager.Instance)
                {
                    BrickSelectionManager.Instance.HandleBrickClicked(_brickData);
                }
            }

            _isDragging = true;
            IsAnyBrickBeingDragged = true;
            IsAnyBrickBeingMoved = true;

            _holdDistance = Vector3.Distance(_camera.transform.position, hit.point);

            Vector3 holdPoint = ray.GetPoint(_holdDistance);
            _dragOffset = transform.position - holdPoint;

            if (singleBrickOnly)
            {
                _mover.PrepareSingleBrick(_brickData);
            }
            else
            {
                _mover.PrepareMovingModel(_brickData);
            }
        }

        /**
         * Let user move brick closer/further away while dragging.
         */
        private void UpdateHoldDistanceFromScroll()
        {
            float scrollAmount = _input.ScrollY();
            if (Mathf.Abs(scrollAmount) < 0.01f) return;
            _holdDistance += scrollAmount * scrollDistanceSpeed;
            _holdDistance = Mathf.Clamp(_holdDistance, minHoldDistance, maxHoldDistance);
        }

        /**
         * Move brick/model to mouse ray position at current hold distance.
         */
        private void DragAtHoldDistance()
        {
            Ray ray = _camera.ScreenPointToRay(_input.MousePosition());
            Vector3 holdPoint = ray.GetPoint(_holdDistance);
            Vector3 targetPosition = holdPoint + _dragOffset;
            Quaternion targetRotation = transform.rotation;
            _snapper.ClearPendingStudConnection();
            if (SnappingEnabled())
            {
                if (_snapper.TryGetSnappedPose(
                        targetPosition,
                        targetRotation,
                        _mover,
                        out Vector3 snappedPosition,
                        out Quaternion snappedRotation
                    ))
                {
                    targetPosition = snappedPosition;
                    targetRotation = snappedRotation;
                }
            }
            bool movedSuccessfully =
                _mover.MoveModelWithSlidingCollision(targetPosition, targetRotation);
            if (!movedSuccessfully) _snapper.ClearPendingStudConnection();
        }

        private bool SnappingEnabled()
        {
            return !_input.DisableSnappingHeld();
        }

        private bool RotationSnappingEnabled()
        {
            return !_input.DisableSnappingHeld();
        }

        /**
         * Switch brick into rotate mode.
         */
        private void EnterRotateMode()
        {
            _isSingleBrickDrag = false;
            _isRotateMode = true;
            _isDragging = false;
            _isMouseRotating = false;
            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;
            IsAnyBrickInRotateMode = true;
            Debug.Log("Entered brick rotate mode. Left click + drag to rotate. Hold Alt/Option to move camera. Press R again to exit.");
        }

        /**
         * Leave rotate mode.
         */
        private void ExitRotateMode()
        {
            _isSingleBrickDrag = false;
            _isRotateMode = false;
            _isDragging = false;
            _isMouseRotating = false;
            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;
            IsAnyBrickInRotateMode = false;
            _mover.ClearMovingModel();
            Debug.Log("Exited brick rotate mode.");
        }

        /**
         * Runs when mouse released after dragging.
         * If brick snapped to another by studs, update logical stud neighbour links.
         * Then clear dragging state and tell mover to forget current moving model.
         */
        private void StopDragging()
        {
            _snapper.CommitPendingStudConnection();

            _isDragging = false;
            _isSingleBrickDrag = false;

            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;

            _mover.ClearMovingModel();
        }

        /**
         * Run when component or GameObject is disabled.
         */
        private void OnDisable()
        {
            _isSingleBrickDrag = false;
            _isDragging = false;
            _isRotateMode = false;
            _isMouseRotating = false;
            IsAnyBrickBeingDragged = false;
            IsAnyBrickBeingMoved = false;
            IsAnyBrickInRotateMode = false;
            _snapper?.ClearPendingStudConnection();
            _mover?.ClearMovingModel();
        }

        /**
         * Try to get component of T.
         * If component does not exist, create one.
         */
        private T GetOrAdd<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();
            return component;
        }
    }
}
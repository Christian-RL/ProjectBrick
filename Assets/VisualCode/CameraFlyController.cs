using UnityEngine;
using UnityEngine.InputSystem;
using BrickCode;
public class CameraFlyController : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private float orbitSensitivity = 0.35f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Pan")]
    [SerializeField] private float panSensitivity = 0.01f;

    [Header("Zoom")]
    [SerializeField] private float zoomSensitivity = 0.08f;
    [SerializeField] private float minFocusDistance = 1f;
    [SerializeField] private float maxFocusDistance = 100f;

    private Vector3 _focusPoint;
    private float _focusDistance = 10f;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;

        _yaw = angles.y;
        _pitch = angles.x;

        _focusPoint = transform.position + transform.forward * _focusDistance;
    }

    private void Update()
    {
        if (Mouse.current == null || Keyboard.current == null)
        {
            return;
        }

        // If a brick is being held, scroll should control the brick distance,
        // not the camera zoom.
        if (DraggableBrick3D.IsAnyBrickBeingDragged)
        {
            return;
        }

        // Prevent camera controls while interacting with the sidebar.
        if (MenuCode.BrickSidebarSpawner.IsMouseOverSidebar)
        {
            return;
        }

        HandleZoom();
        HandleOrbit();
        HandlePan();

        UpdateCameraPosition();
    }

    private void HandleOrbit()
    {
        bool altHeld =
            Keyboard.current.leftAltKey.isPressed ||
            Keyboard.current.rightAltKey.isPressed;

        bool shiftHeld =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        bool leftMouseHeld = Mouse.current.leftButton.isPressed;

        if (!altHeld || shiftHeld || !leftMouseHeld)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        _yaw += mouseDelta.x * orbitSensitivity;
        _pitch -= mouseDelta.y * orbitSensitivity;

        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void HandlePan()
    {
        bool altHeld =
            Keyboard.current.leftAltKey.isPressed ||
            Keyboard.current.rightAltKey.isPressed;

        bool shiftHeld =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        bool leftMouseHeld = Mouse.current.leftButton.isPressed;

        if (!altHeld || !shiftHeld || !leftMouseHeld)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        Vector3 panMovement =
            -transform.right * mouseDelta.x * panSensitivity * _focusDistance
            - transform.up * mouseDelta.y * panSensitivity * _focusDistance;

        _focusPoint += panMovement;
    }

    private void HandleZoom()
    {
        float scrollAmount = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollAmount) < 0.01f)
        {
            return;
        }

        _focusDistance -= scrollAmount * zoomSensitivity;
        _focusDistance = Mathf.Clamp(_focusDistance, minFocusDistance, maxFocusDistance);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -_focusDistance);

        transform.position = _focusPoint + cameraOffset;
        transform.rotation = rotation;
    }
}
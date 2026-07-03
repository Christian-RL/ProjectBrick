using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFlyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float fastMoveMultiplier = 2f;
    [SerializeField] private float lookSensitivity = 0.55f;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        float speed = moveSpeed;

        if (Keyboard.current.leftShiftKey.isPressed)
        {
            speed *= fastMoveMultiplier;
        }

        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            movement += transform.forward;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            movement -= transform.forward;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            movement -= transform.right;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            movement += transform.right;
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            movement += Vector3.up;
        }

        if (Keyboard.current.cKey.isPressed)
        {
            movement += Vector3.down;
        }

        transform.position += movement.normalized * speed * Time.deltaTime;
    }

    private void HandleLook()
    {
        if (Mouse.current == null)
        {
            return;
        }

        // Hold right click to look around
        if (!Mouse.current.rightButton.isPressed)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        _yaw += mouseDelta.x * lookSensitivity;
        _pitch -= mouseDelta.y * lookSensitivity;

        _pitch = Mathf.Clamp(_pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
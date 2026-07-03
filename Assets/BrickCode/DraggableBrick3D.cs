using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableBrick3D : MonoBehaviour
{
    [SerializeField] private float scrollDistanceSpeed = 0.06f;
    [SerializeField] private float minHoldDistance = 0.5f;
    [SerializeField] private float maxHoldDistance = 100f;

    private Camera _camera;
    private bool _isDragging;

    private float _holdDistance;
    private Vector3 _dragOffset;

    private void Start()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("No MainCamera found. Make sure your camera is tagged MainCamera.");
        }
    }

    private void Update()
    {
        if (_camera == null || Mouse.current == null)
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
            _isDragging = false;
        }
    }

    private void TryStartDragging()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _camera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return;
        }

        DraggableBrick3D draggable = hit.collider.GetComponentInParent<DraggableBrick3D>();

        if (draggable != this)
        {
            return;
        }

        _isDragging = true;

        // Distance from camera to the point you clicked on the brick.
        _holdDistance = Vector3.Distance(_camera.transform.position, hit.point);

        // Keeps the brick from snapping its centre directly to the mouse ray.
        Vector3 holdPoint = ray.GetPoint(_holdDistance);
        _dragOffset = transform.position - holdPoint;
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
        transform.position = holdPoint + _dragOffset;
    }
}
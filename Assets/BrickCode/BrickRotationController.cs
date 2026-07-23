using UnityEngine;

namespace BrickCode
{
    /**
     * Calculates what rotation a brick should have while the user is dragging the mouse in rotate mode.
     */
    public class BrickRotationController : MonoBehaviour
    {
        [Header("Mouse Rotation")]
        [SerializeField] private float mouseRotationSensitivity = 0.35f;
        [SerializeField] private float rotationSnapAngle = 12f;

        private Vector2 _rotationStartMousePosition;
        private Quaternion _rotationStartRotation;

        /**
         * Stores where the mouse started and what rotation the brick started with.
         */
        public void BeginRotation(Vector2 startMousePosition, Quaternion startRotation)
        {
            _rotationStartMousePosition = startMousePosition;
            _rotationStartRotation = startRotation;
        }

        /**
         * Calculates the new target rotation based on the current mouse position.
         */
        public Quaternion GetTargetRotation(Camera camera, Vector2 currentMousePosition, bool snappingEnabled)
        {
            Vector2 mouseDelta = currentMousePosition - _rotationStartMousePosition;
            float yawAmount = mouseDelta.x * mouseRotationSensitivity;
            float pitchAmount = -mouseDelta.y * mouseRotationSensitivity;
            Quaternion yawRotation = Quaternion.AngleAxis(yawAmount, Vector3.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitchAmount, camera.transform.right);
            Quaternion targetRotation = yawRotation * pitchRotation * _rotationStartRotation;
            if (snappingEnabled) targetRotation = SnapRotationToDefaultPositionsIfClose(targetRotation);
            return targetRotation;
        }

        /**
         * Snaps rotation if it is close enough to a clean 90-degree orientation.
         */
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
            if (angleDifference <= rotationSnapAngle) return snappedRotation;
            return rotation;
        }
    }
}
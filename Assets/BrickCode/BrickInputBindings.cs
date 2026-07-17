using UnityEngine;
using UnityEngine.InputSystem;

namespace BrickCode
{
    /**
     * Controls keybinds
     */
    public class BrickInputBindings : MonoBehaviour
    {
        [Header("Main Actions")] //Switches modes of brick movement.
        [SerializeField] private Key rotateModeKey = Key.R;

        [Header("Snapping Modifier")] //Disables/enables snapping.
        [SerializeField] private Key[] disableSnappingKeys =
        {
            Key.LeftCtrl,
            Key.RightCtrl,
            Key.LeftMeta,
            Key.RightMeta
        };

        [Header("Camera Control Modifier")] //Determines use of camera.
        [SerializeField] private Key[] cameraModifierKeys =
        {
            Key.LeftAlt,
            Key.RightAlt
        };

        /**
         * Returns true if the rotate mode key is pressed this frame.
         */
        public bool RotateModePressedThisFrame()
        {
            return KeyWasPressedThisFrame(rotateModeKey);
        }

        /**
         * Returns true if the disable snapping key is held.
         */
        public bool DisableSnappingHeld()
        {
            return AnyKeyHeld(disableSnappingKeys);
        }

        /**
         * Returns true if current inputs relate to the camera.
         */
        public bool CameraControlInputIsActive()
        {
            if (Mouse.current == null) return false;
            bool middleMouseHeld = Mouse.current.middleButton.isPressed;
            bool rightMouseHeld = Mouse.current.rightButton.isPressed;
            return AnyKeyHeld(cameraModifierKeys) || middleMouseHeld || rightMouseHeld;
        }
        
        public bool LeftMousePressedThisFrame()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        public bool LeftMouseHeld()
        {
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
        }

        public bool LeftMouseReleasedThisFrame()
        {
            return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        }

        /**
         * Returns the current position of the mouse as a Vector2.
         */
        public Vector2 MousePosition()
        {
            if (Mouse.current == null) return Vector2.zero;
            return Mouse.current.position.ReadValue();
        }

        /**
         * Returns the value of vertical scroll.
         */
        public float ScrollY()
        {
            if (Mouse.current == null) return 0f;
            return Mouse.current.scroll.ReadValue().y;
        }

        /**
         * Returns true if a key from the given set is currently held.
         */
        private bool AnyKeyHeld(Key[] keys)
        {
            if (Keyboard.current == null || keys == null) return false;
            foreach (Key key in keys)
            {
                if (KeyHeld(key)) return true;
            }
            return false;
        }

        /**
         * Returns true if the given key is currently held.
         */
        private bool KeyHeld(Key key)
        {
            if (Keyboard.current == null || key == Key.None) return false;
            return Keyboard.current[key].isPressed;
        }

        /**
         * Returns true if the given key was pressed in the current frame.
         */
        private bool KeyWasPressedThisFrame(Key key)
        {
            if (Keyboard.current == null || key == Key.None) return false;
            return Keyboard.current[key].wasPressedThisFrame;
        }
    }
}
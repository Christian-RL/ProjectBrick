using UnityEngine.InputSystem;

namespace BrickCode
{
    /**
     * Determines what type of input is currently activated.
     */
    public static class BrickInput
    {
        public static bool CtrlOrCommandHeld()
        {
            if (Keyboard.current == null) return false;
            return
                Keyboard.current.leftCtrlKey.isPressed ||
                Keyboard.current.rightCtrlKey.isPressed ||
                Keyboard.current.leftCommandKey.isPressed ||
                Keyboard.current.rightCommandKey.isPressed;
        }

        public static bool ShiftHeld()
        {
            if (Keyboard.current == null) return false;
            return
                Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed;
        }

        public static bool AltOrOptionHeld()
        {
            if (Keyboard.current == null) return false;
            return
                Keyboard.current.leftAltKey.isPressed ||
                Keyboard.current.rightAltKey.isPressed;
        }
        
        
        public static bool CameraControlInputIsActive()
        {
            if (Mouse.current == null)
            {
                return false;
            }

            bool middleMouseHeld = Mouse.current.middleButton.isPressed;

            return AltOrOptionHeld() || middleMouseHeld;
        }
    }
}
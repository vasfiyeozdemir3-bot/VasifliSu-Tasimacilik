using UnityEngine;
using UnityEngine.InputSystem;

namespace VasifliSu
{
    /// <summary>
    /// Klavye, gamepad ve ekran (dokunmatik/fare) butonlarını tek bir gaz değerine çevirir.
    /// </summary>
    public static class InputBridge
    {
        public static bool GasHeld;
        public static bool BrakeHeld;
        // Tanıtım videosu kaydında otomatik sürüş
        public static bool Demo;
        public static float DemoThrottle;

        /// <summary>-1 (fren/geri) ile +1 (gaz) arası.</summary>
        public static float Throttle()
        {
            if (Demo) return Mathf.Clamp(DemoThrottle, -1f, 1f);
            float t = 0f;
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.shiftKey.isPressed || k.rightArrowKey.isPressed || k.dKey.isPressed || k.upArrowKey.isPressed || k.wKey.isPressed) t += 1f;
                if (k.ctrlKey.isPressed || k.spaceKey.isPressed || k.leftArrowKey.isPressed || k.aKey.isPressed || k.downArrowKey.isPressed || k.sKey.isPressed) t -= 1f;
            }
            var g = Gamepad.current;
            if (g != null)
            {
                t += g.rightTrigger.ReadValue();
                t -= g.leftTrigger.ReadValue();
                t += g.leftStick.ReadValue().x;
            }
            if (GasHeld) t += 1f;
            if (BrakeHeld) t -= 1f;
            return Mathf.Clamp(t, -1f, 1f);
        }

        public static bool PausePressed()
        {
            var k = Keyboard.current;
            if (k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame)) return true;
            var g = Gamepad.current;
            return g != null && g.startButton.wasPressedThisFrame;
        }

        public static bool RestartPressed()
        {
            var k = Keyboard.current;
            return k != null && k.rKey.wasPressedThisFrame;
        }

        public static void Reset() { GasHeld = false; BrakeHeld = false; }
    }
}

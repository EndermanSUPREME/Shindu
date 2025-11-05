namespace ControllerInputs
{
    // scoped inclusion
    using UnityEngine;

    public static class ControllerInput
    {
        public static bool PressedA()
        {
            return Input.GetButtonDown("Jump");
        }
        public static bool PressedB()
        {
            return Input.GetButtonDown("B");
        }
        public static bool PressedX()
        {
            return Input.GetButtonDown("X");
        }
        public static bool PressedY()
        {
            return Input.GetButtonDown("Y");
        }

        public static bool HoldingLeftBumper()
        {
            return Input.GetButton("LeftBumper");
        }
        public static bool HoldingRightBumper()
        {
            return Input.GetButton("RightBumper");
        }
    }
}
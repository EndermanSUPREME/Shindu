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
        public static bool HoldingA()
        {
            return Input.GetButton("Jump");
        }

        public static bool PressedB()
        {
            return Input.GetButtonDown("B");
        }
        public static bool HoldingB()
        {
            return Input.GetButton("B");
        }

        public static bool PressedX()
        {
            return Input.GetButtonDown("X");
        }
        public static bool HoldingX()
        {
            return Input.GetButton("X");
        }

        public static bool PressedY()
        {
            return Input.GetButtonDown("Y");
        }
        public static bool HoldingY()
        {
            return Input.GetButton("Y");
        }

        public static bool PressedLeftBumper()
        {
            return Input.GetButtonDown("LeftBumper");
        }
        public static bool HoldingLeftBumper()
        {
            return Input.GetButton("LeftBumper");
        }

        public static bool PressedRightBumper()
        {
            return Input.GetButtonDown("RightBumper");
        }
        public static bool HoldingRightBumper()
        {
            return Input.GetButton("RightBumper");
        }

        public static bool HoldingLeftTrigger()
        {
            float leftTrigger = Input.GetAxis("LeftTrigger");
            return leftTrigger > 0.1f;
        }

        public static bool HoldingRightTrigger()
        {
            float rightTrigger = Input.GetAxis("RightTrigger");
            return rightTrigger > 0.1f;
        }
    }
}
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR;

namespace OculusReportMenu {
    internal class Input {
        /*
         * This handles binding report menu buttons
        */

        internal static float Sensitivity = 0.5f; 

        internal static string OpenButton1 = "LS";
        internal static string OpenButton2 = "RS";

        internal static bool EnableTabOpening, UseCustomKeybinds;

        internal static bool Activated {
            get {
                bool normal = !UseCustomKeybinds
                    && ControllerInputPoller.instance.leftControllerSecondaryButton
                    && ControllerInputPoller.instance.rightControllerSecondaryButton
                ;
                
                bool custom = UseCustomKeybinds
                    && CheckButtonPressedStatus(_openButton1)
                    && CheckButtonPressedStatus(_openButton2)
                ;
                
                bool tab = EnableTabOpening
                    && Keyboard.current.tabKey.wasPressedThisFrame
                ;
                
                return normal || custom || tab;
            }
        }

        private static bool CheckButtonPressedStatus(string thisEntry)
        {
            bool temporarySClick;

            switch (thisEntry.ToUpper())
            {
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > Sensitivity;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > Sensitivity;
                case "LJ":
                    if (_platformSteam)
                        temporarySClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // right hand
                case "RP": return ControllerInputPoller.instance.rightControllerPrimaryButton;
                case "RS": return ControllerInputPoller.instance.rightControllerSecondaryButton;
                case "RT": return ControllerInputPoller.instance.rightControllerIndexFloat > Sensitivity;
                case "RG": return ControllerInputPoller.instance.rightControllerGripFloat > Sensitivity;
                case "RJ":
                    if (Plugin._platformSteam)
                        temporarySClick = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // NAN
                case "NAN":
                    return true;
            }
            
            return false;
        }
    }
}

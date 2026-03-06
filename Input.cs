// OculusReportMenu
// (C) Copyright 2024 - 2026 SirKingBinx (Bingus)
// MIT License

using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR;

namespace OculusReportMenu {
    internal class Input {
        /*
         * This handles binding report menu buttons
        */

        internal static float Sensitivity = 0.5f; 

        internal static ORM_Button OpenButton1 = "LS";
        internal static ORM_Button OpenButton2 = "RS";

        internal static bool EnableTabOpening, UseCustomKeybinds;

        internal static bool Activated {
            get {
                bool normal = !UseCustomKeybinds
                    && ControllerInputPoller.instance.leftControllerSecondaryButton
                    && ControllerInputPoller.instance.rightControllerSecondaryButton
                ;
                
                bool custom = UseCustomKeybinds
                    && CheckButtonPressedStatus(OpenButton1)
                    && CheckButtonPressedStatus(OpenButton2)
                ;
                
                bool tab = EnableTabOpening
                    && Keyboard.current.tabKey.wasPressedThisFrame
                ;
                
                return normal || custom || tab;
            }
        }

        private static bool CheckButtonPressedStatus(ORM_Button thisEntry)
        {
            bool temporarySClick;

            switch (thisEntry)
            {
                case ORM_Button.LeftPrimary: return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case ORM_Button.LeftSecondary: return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case ORM_Button.LeftTrigger: return ControllerInputPoller.instance.leftControllerIndexFloat > Sensitivity;
                case ORM_Button.LeftGrip: return ControllerInputPoller.instance.leftControllerGripFloat > Sensitivity;
                case ORM_Button.LeftJoystickClick:
                    if (Plugin.Instance._platformSteam)
                        temporarySClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // right hand
                case ORM_Button.RightPrimary: return ControllerInputPoller.instance.rightControllerPrimaryButton;
                case ORM_Button.RightSecondary: return ControllerInputPoller.instance.rightControllerSecondaryButton;
                case ORM_Button.RightTrigger: return ControllerInputPoller.instance.rightControllerIndexFloat > Sensitivity;
                case ORM_Button.RightGrip: return ControllerInputPoller.instance.rightControllerGripFloat > Sensitivity;
                case ORM_Button.RightJoystickClick:
                    if (Plugin.Instance._platformSteam)
                        temporarySClick = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // NAN
                case ORM_Button.None:
                    return true;
            }
            
            return false;
        }
    }

    public enum ORM_Button {
        None = -1,
        
        LeftPrimary = 0,
        LeftSecondary,
        LeftGrip,
        LeftTrigger,
        LeftJoystickClick,

        RightPrimary = 10,
        RightSecondary,
        RightGrip,
        RightTrigger,
        RightJoystickClick
    }
}

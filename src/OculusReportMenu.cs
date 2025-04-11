// OculusReportMenu
// (C) Copyright 2024 - 2025 binx
// MIT License

using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using GorillaNetworking;
using GorillaLocomotion;
using BepInEx.Configuration;
using Valve.VR;
using System.Collections;

// build target
// uncomment one to build for it

// #define BUILD_TARGET_WIN
// #define BUILD_TARGET_LINUX

#if (!BUILD_TARGET_WIN && !BUILD_TARGET_LINUX)
    #error No build target defined. Please uncomment BUILD_TARGET_(x).
#endif

namespace OculusReportMenu {
    public class ModInfo {
        public static string UUID = "kingbingus.oculusreportmenu";
        public static string Name = "OculusReportMenu";
        public static string Version = "1.2.1";
    }

    [BepInPlugin(ModInfo.UUID, ModInfo.Name, ModInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {

#if (BUILD_TARGET_WIN)
        // custom stuff
        public static ConfigEntry<string> OpenButton1, OpenButton2 {get; internal set;}
#endif

        // base things
        public static bool Menu, ModEnabled { get; internal set; }
        public static GorillaMetaReport MetaReportMenu { get; internal set; }

        internal static bool usingSteamVR;

        MethodInfo CheckDistance, CheckReportSubmit, ShowMenu;

        void Update()
        {
            if (Menu)
            {
                // hide the fact that they're in report menu to prevent comp cheating
                GTPlayer.Instance.disableMovement = false;
                GTPlayer.Instance.inOverlay = false;

                // get stuff
                GameObject occluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");// (GameObject)Traverse.Create(typeof(GorillaMetaReport)).Field("occluder").GetValue()
                GameObject metaLeftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent"); 
                GameObject metaRightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");

                occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;
                metaRightHand.transform.SetPositionAndRotation(GTPlayer.Instance.rightControllerTransform.position, GTPlayer.Instance.rightControllerTransform.rotation);
                metaLeftHand.transform.SetPositionAndRotation(GTPlayer.Instance.leftControllerTransform.position, GTPlayer.Instance.leftControllerTransform.rotation);

                CheckDistance.Invoke(MetaReportMenu, null);
                CheckReportSubmit.Invoke(MetaReportMenu, null);
            }
            else if (GetControllerPressed() && ModEnabled) { ShowMenu(); }
        }

#if (BUILD_TARGET_WIN)
        internal bool GetControllerPressed() => CheckButtonPressedStatus(OpenButton1) && CheckButtonPressedStatus(OpenButton2) || Keyboard.current.tabKey.wasPressedThisFrame;
#elif (BUILD_TARGET_LINUX)
        internal bool GetControllerPressed() => ControllerInputPoller.instance.leftControllerSecondaryButton || Keyboard.current.tabKey.wasPressedThisFrame;
#else
        internal bool GetControllerPressed() => false;
#endif

        internal static void ShowMenu()
        {
            if (!Menu)
            {
                MetaReportMenu.gameObject.SetActive(true);
                MetaReportMenu.enabled = true;
                
                ShowMenu.Invoke(MetaReportMenu, null);
                Menu = true;
            }
        }

        public void OnEnable() { ModEnabled = true; HarmonyPatches.ApplyHarmonyPatches(ModInfo.UUID); }
        public void OnDisable() { ModEnabled = false; HarmonyPatches.RemoveHarmonyPatches(); }

        void Awake()
        {
#if (BUILD_TARGET_WIN)
            OpenButton1 = Config.Bind("Keybinds",
                                      "OpenButton1",
                                      "LS",
                                      "One of the buttons you use to open ORM (NAN for none)");

            OpenButton2 = Config.Bind("Keybinds",
                                      "OpenButton2",
                                      "RJ",
                                      "One of the buttons you use to open ORM (NAN for none)");
        }
#endif
        // checks for the right key

#if (BUILD_TARGET_WIN)
        internal static bool CheckButtonPressedStatus(ConfigEntry<string> thisEntry)
        {
            bool temporarySClick = false;

            switch (thisEntry.Value.ToUpper())
            {
                // left hand
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > 0.5f;
                case "LJ":
                    if (usingSteamVR)
                        temporarySClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // right hand
                case "RP": return ControllerInputPoller.instance.rightControllerPrimaryButton;
                case "RS": return ControllerInputPoller.instance.rightControllerSecondaryButton;
                case "RT": return ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
                case "RG": return ControllerInputPoller.instance.rightControllerGripFloat > 0.5f;
                case "RJ":
                    if (usingSteamVR)
                        temporarySClick = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                case "NAN":
                    return true;
            }

            return false;
        }
#endif
    }
}

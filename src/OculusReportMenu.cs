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

using OculusReportMenu.Patches;

namespace OculusReportMenu {
    [BepInPlugin("kingbingus.oculusreportmenu", "OculusReportMenu", "1.2.1")]
    public class Plugin : BaseUnityPlugin
    {
        // custom stuff
        internal static bool Menu;
        internal static ConfigEntry<string> OpenButton1, OpenButton2;

        // base things
        internal static GorillaMetaReport MetaReportMenu;

        internal static bool usingSteamVR;
        internal static MethodInfo CheckDistance, CheckReportSubmit;

        internal static GameObject occluder, metaLeftHand, metaRightHand;

        bool IsNull(object thing) => thing != null ? false : true;

        void Update()
        {
            if (Menu)
            {
                GTPlayer.Instance.disableMovement = false;
                GTPlayer.Instance.inOverlay = false;

                Plugin.occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;

                Plugin.metaLeftHand.transform.SetPositionAndRotation(GorillaTagger.Instance.offlineVRRig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.leftHandTransform.rotation);
                Plugin.metaRightHand.transform.SetPositionAndRotation(GorillaTagger.Instance.offlineVRRig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.rightHandTransform.rotation); ;

                CheckDistance.Invoke(MetaReportMenu, null);
                CheckReportSubmit.Invoke(MetaReportMenu, null);
            }
            else if (GetControllerPressed()) { ShowMenu(); }
        }

        internal static bool GetControllerPressed() => (CheckButtonPressedStatus(OpenButton1) && CheckButtonPressedStatus(OpenButton2)) | Keyboard.current.tabKey.wasPressedThisFrame;

        internal static void ShowMenu()
        {
            if (!Menu && MetaReportMenu != null)
            {
                if (MetaReportMenu != null)
                {
                    MetaReportMenu.gameObject.SetActive(true);
                    MetaReportMenu.enabled = true;

                    object[] args = { false };
                    typeof(GorillaMetaReport).GetMethod("StartOverlay").Invoke(MetaReportMenu, args);

                    Menu = true;
                } else
                {
                    Debug.Log("tried invoking ShowMenu but there is no MetaReportMenu!!!!!!!!!!!!!!!!!");
                }
            }
        }

        public void Start()
        {
            HarmonyPatches.ApplyHarmonyPatches(this);

            occluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");
            metaLeftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent");
            metaRightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");

            MetaReportMenu = occluder.GetComponent<GorillaMetaReport>();

            CheckDistance = typeof(GorillaMetaReport).GetMethod("CheckDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            CheckReportSubmit = typeof(GorillaMetaReport).GetMethod("CheckReportSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        void Awake()
        {
            OpenButton1 = Config.Bind("Keybinds",
                                      "OpenButton1",
                                      "LS",
                                      "One of the buttons you use to open ORM (NAN for none)");

            OpenButton2 = Config.Bind("Keybinds",
                                      "OpenButton2",
                                      "RJ",
                                      "One of the buttons you use to open ORM (NAN for none)");
        }

        internal static bool CheckButtonPressedStatus(ConfigEntry<string> thisEntry)
        {
            bool temporarySClick = false;

            switch (thisEntry.Value.ToUpper())
            {
                // left hand
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > 0.75f;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > 0.75f;
                case "LJ":
                    if (usingSteamVR)
                        temporarySClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // right hand
                case "RP": return ControllerInputPoller.instance.rightControllerPrimaryButton;
                case "RS": return ControllerInputPoller.instance.rightControllerSecondaryButton;
                case "RT": return ControllerInputPoller.instance.rightControllerIndexFloat > 0.75f;
                case "RG": return ControllerInputPoller.instance.rightControllerGripFloat > 0.75f;
                case "RJ":
                    if (usingSteamVR)
                        temporarySClick = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                case "NAN": return true;
                default: return true;
            }
        }
    }

    public class GamePatches
    {
        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        static void SteamPatch() => GTPlayer.Instance.InReportMenu = false;

        [HarmonyPatch(typeof(GorillaMetaReport), "Teardown")]
        static void MenuCloseHook() => Plugin.Menu = false;

        [HarmonyPatch(typeof(GorillaComputer), "Initialise")]
        static void CheckPlatform() => Plugin.usingSteamVR = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
    }
}

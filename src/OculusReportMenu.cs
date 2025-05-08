// OculusReportMenu
// (C) Copyright 2024 - 2025 binx
// MIT License

using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using GorillaNetworking;
using GorillaLocomotion;

namespace OculusReportMenu
{
    [BepInPlugin("kingbingus.oculusreportmenu", "OculusReportMenu", "2.0.0")]
    internal class Plugin
    {
        internal static Plugin instance;

        internal GorillaMetaReport Menu;
        internal GameObject ORMOccluder, ORMLeftHand, ORMRightHand;
        internal MethodInfo UpdatePosition, CheckReports, OpenMenu;

        internal bool ShowingMenu = false;
        internal bool ModEnabled = false;

        // plugin

        internal void Start() => new Harmony("kingbingus.oculusreportmenu");PatchAll(Assembly.GetExecutingAssembly());

        internal void OnEnable() => ModEnabled = true;
        internal void OnDisable() => ModEnabled = false;

        internal void Update()
        {
            if (!ShowingMenu && ButtonsPressed())
            {
                MetaReportMenu.gameObject.SetActive(true);
                MetaReportMenu.enabled = true;

                OpenMenu.Invoke(MetaReportMenu, null);
                ShowingMenu = true;
            } else {
                GTPlayer.Instance.disableMovement = false;
                GTPlayer.Instance.inOverlay = false;

                Occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;

                ORMLeftHand.transform.SetPositionAndRotation(GorillaTagger.Instance.offlineVRRig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.leftHandTransform.rotation);
                ORMRightHand.transform.SetPositionAndRotation(GorillaTagger.Instance.offlineVRRig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.rightHandTransform.rotation);

                UpdatePosition.Invoke(MetaReportMenu, null);
                CheckReports.Invoke(MetaReportMenu, null);
            }
        }

        internal void ButtonsPressed() => (ControllerInputPoller.instance.leftControllerSecondaryButton && ControllerInputPoller.instance.leftControllerSecondaryButton) | Keyboard.current.tabKey.wasPressedThisFrame;

        // patches

        [HarmonyPatch(typeof(GorillaMetaReport), "Start")]
        static void OnOculusInit(GorillaMetaReport creatingMenu)
        {
            Menu = creatingMenu;

            ORMOccluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");
            ORMLeftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent");
            ORMRightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");

            UpdatePosition = typeof(GorillaMetaReport).GetMethod("CheckDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            OpenMenu = typeof(GorillaMetaReport).GetMethod("StartOverlay", BindingFlags.NonPublic | BindingFlags.Instance);
            CheckReports = typeof(GorillaMetaReport).GetMethod("CheckReportSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        static void OnOculusUpdate() => GTPlayer.Instance.InReportMenu = false;

        [HarmonyPatch(typeof(GorillaMetaReport), "Teardown")]
        static void OnOculusClose() => Plugin.Menu = false;

        [HarmonyPatch(typeof(GorillaComputer), "Initialise")]
        static void OnComputerInit() => Plugin.usingSteamVR = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
    }
}

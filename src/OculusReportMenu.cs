// OculusReportMenu
// (C) Copyright 2024 - 2025 bingus
// MIT License

// Patchers
using BepInEx;
using HarmonyLib;

// System
using System.Reflection;

// Engine
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR;

// GT
using GorillaNetworking;
using GorillaLocomotion;

namespace OculusReportMenu
{
    [BepInPlugin("kingbingus.oculusreportmenu", "OculusReportMenu", "2.1.0")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static Plugin instance;

        internal GorillaMetaReport Menu;
        internal GameObject ORMOccluder, ORMLeftHand, ORMRightHand;
        internal MethodInfo UpdatePosition, CheckReports, OpenMenu, Teardown;
        
        internal bool ShowingMenu, ButtonsPressed, PlatformSteam;

        internal string OpenButton1, OpenButton2;
        internal float Sensitivity;

        // plugin

        internal void Start() {
            Harmony.CreateAndPatchAll(GetType().Assembly, "kingbingus.oculusreportmenu");

            OpenButton1 = Config.Bind("Keybinds", "OpenButton1", "LS", "One of the buttons you use to open ORM (NAN for none)").Value;
            OpenButton2 = Config.Bind("Keybinds", "OpenButton2", "RJ", "One of the buttons you use to open ORM (NAN for none)").Value;
            Sensitivity = Config.Bind("Input", "Sensitivity", 0.5f, "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;

            GorillaTagger.OnPlayerSpawned(delegate
            {
                /*
                i have no idea why this was the best way to do it, menu is now set in Harmony Patches
                
                    var metaReportMenu = Resources.FindObjectsOfTypeAll<GorillaMetaReport>();
                    
                    foreach (GorillaMetaReport m in metaReportMenu)
                    {
                        Menu = m;
                    }
                */
            
                ORMOccluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");
                ORMLeftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent");
                ORMRightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");

                UpdatePosition =
                    typeof(GorillaMetaReport).GetMethod("CheckDistance",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                OpenMenu = typeof(GorillaMetaReport).GetMethod("StartOverlay",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                CheckReports = typeof(GorillaMetaReport).GetMethod("CheckReportSubmit",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Teardown = typeof(GorillaMetaReport).GetMethod("CheckReportSubmit",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            });
        }

        internal void Update() {
            if (!Menu) {
                ButtonsPressed =
                    (CheckButtonPressedStatus(OpenButton1) && CheckButtonPressedStatus(OpenButton2))
                    | Keyboard.current.tabKey.wasPressedThisFrame;

                if (ShowingMenu) {
                    GTPlayer.Instance.disableMovement = false;
                    GTPlayer.Instance.inOverlay = false;

                    ORMOccluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;

                    ORMRightHand.transform.SetPositionAndRotation(
                        GTPlayer.Instance.rightControllerTransform.position,
                        GTPlayer.Instance.rightControllerTransform.rotation);
                    ORMLeftHand.transform.SetPositionAndRotation(
                        GTPlayer.Instance.leftControllerTransform.position,
                        GTPlayer.Instance.leftControllerTransform.rotation);

                    UpdatePosition.Invoke(Menu, null);
                    CheckReports.Invoke(Menu, null);
                } else if (ButtonsPressed)
                {
                    object[] args = { true };

                    Menu.gameObject.SetActive(true);
                    Menu.enabled = true;

                    OpenMenu.Invoke(Menu, args);
                    ShowingMenu = true;
                }
            }

            if (!Menu.gameObject.activeInHierarchy && ShowingMenu)
                ShowingMenu = false;
        }

        internal bool CheckButtonPressedStatus(string thisEntry)
        {
            bool temporarySClick = false;

            switch (thisEntry.ToUpper())
            {
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > Sensitivity;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > Sensitivity;
                case "LJ":
                    if (PlatformSteam)
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
                    if (PlatformSteam)
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

        // GorillaMetaReport
        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        internal class OnOculusUpdate
        {
            static void Postfix() => GTPlayer.Instance.InReportMenu = false;
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "Start")]
        internal class OnReportInit
        {
            static void Postfix(GorillaMetaReport __instance) => Menu = __instance;
        }

        // GorillaComputer
        [HarmonyPatch(typeof(GorillaComputer), "Initialise")]
        internal class OnComputerInit
        {
            static void Postfix() =>
                instance.PlatformSteam =
                    PlayFabAuthenticator.instance.platform.PlatformTag
                    .ToLower()
                    .Contains("steam");
        }
    }
}

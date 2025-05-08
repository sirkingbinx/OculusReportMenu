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
using UnityEngine.XR;
using Valve.VR;
using BepInEx.Configuration;

namespace OculusReportMenu
{
    [BepInPlugin("kingbingus.oculusreportmenu", "OculusReportMenu", "2.0.0")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static Plugin instance;

        internal GorillaMetaReport Menu;
        internal GameObject ORMOccluder, ORMLeftHand, ORMRightHand;
        internal MethodInfo UpdatePosition, CheckReports, OpenMenu;

        internal bool ShowingMenu = false;
        internal bool isSteam;
        internal bool ModEnabled, ButtonsPressed;

        internal string OpenButton1, OpenButton2;

        // plugin

        internal void Start()
        {
            Harmony.CreateAndPatchAll(GetType().Assembly, "kingbingus.oculusreportmenu");

            GorillaTagger.OnPlayerSpawned(delegate
            {
                var MetaReportMenu = Resources.FindObjectsOfTypeAll<GorillaMetaReport>();
                foreach (GorillaMetaReport m in MetaReportMenu)
                {
                    Menu = m;
                }

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
            });
        }

        internal void OnEnable() => ModEnabled = true;
        internal void OnDisable() => ModEnabled = false;

        internal void Update()
        {
            if (Menu != null)
            {
                ButtonsPressed =
                    (CheckButtonPressedStatus(OpenButton1) && CheckButtonPressedStatus(OpenButton2))
                    | Keyboard.current.tabKey.wasPressedThisFrame;

                if (ShowingMenu)
                {
                    GTPlayer.Instance.disableMovement = false;
                    GTPlayer.Instance.inOverlay = false;

                    ORMOccluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;

                    ORMRightHand.transform.SetPositionAndRotation(GTPlayer.Instance.rightControllerTransform.position,
                        GTPlayer.Instance.rightControllerTransform.rotation);
                    ORMLeftHand.transform.SetPositionAndRotation(GTPlayer.Instance.leftControllerTransform.position,
                        GTPlayer.Instance.leftControllerTransform.rotation);

                    UpdatePosition.Invoke(Menu, null);
                    CheckReports.Invoke(Menu, null);
                }
                else if (ButtonsPressed)
                {
                    object[] args = { false };

                    Menu.gameObject.SetActive(true);
                    Menu.enabled = true;

                    OpenMenu.Invoke(Menu, args);
                    ShowingMenu = true;
                }
            }

            if (!Menu.gameObject.activeInHierarchy && ShowingMenu)
            { 
                ShowingMenu = false;
            }
        }

        void Awake()
        {
            /* key to configs
             * P - primary
             * S - secondary
             * J - thumbstick
             * T - trigger
             * G - grip
             * 
             * L - left
             * R - right
             * 
             * N - none (no keybind, make sure to set a key to the other one though)
             * 
             * examples: right trigger = RT, left secondary = LS
             */

            OpenButton1 = Config.Bind("Keybinds",
                                      "OpenButton1",
                                      "LS",
                                      "One of the buttons you use to open ORM (NAN for none)").Value;

            OpenButton2 = Config.Bind("Keybinds",
                                      "OpenButton2",
                                      "RJ",
                                      "One of the buttons you use to open ORM (NAN for none)").Value;
        }

        // checks for the right key

        internal bool CheckButtonPressedStatus(string thisEntry)
        {
            bool temporarySClick = false;

            switch (thisEntry.ToUpper())
            {
                // left hand
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > 0.5f;
                case "LJ":
                    if (isSteam)
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
                    if (isSteam)
                        temporarySClick = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                case "NAN":
                    return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        internal class OnOculusUpdate
        {
            static void Postfix() => GTPlayer.Instance.InReportMenu = false;
        }

        [HarmonyPatch(typeof(GorillaComputer), "Initialise")]
        internal class OnComputerInit
        {
            static void Postfix() => Plugin.instance.isSteam = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
        }
    }
}

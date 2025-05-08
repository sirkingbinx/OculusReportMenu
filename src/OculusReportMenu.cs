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
        internal bool ModEnabled, RJ;

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
                if (isSteam)
                    RJ = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                else
                    InputDevices.GetDeviceAtXRNode(XRNode.RightHand)
                        .TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out RJ);

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
                else if (RJ && ControllerInputPoller.instance.leftControllerSecondaryButton)
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
    }

    [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
    internal class OnOculusUpdate
    {
        static void Postfix() => GTPlayer.Instance.InReportMenu = false;
    }

    [HarmonyPatch(typeof(GorillaComputer), "Initialise")]
    public class OnComputerInit
    {
        static void Postfix() => Plugin.instance.isSteam = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
    }
}

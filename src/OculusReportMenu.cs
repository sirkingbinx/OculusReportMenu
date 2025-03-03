using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;

namespace OculusReportMenu {
    [BepInPlugin("org.stickmaster1-.gorillatag.oculusreportmenu", "OculusReportMenu", "1.0.7")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool Menu;
        public static GorillaMetaReport MetaReportMenu;
        public static bool usingSteamVR;

        public void Update()
        {
            if (!usingSteamVR) riftStickClick = InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out riftStickClick);
            
            if (Menu)
            {
                // hide the fact that they're in report menu to prevent comp cheating
                GorillaLocomotion.Player.Instance.disableMovement = false;
                GorillaLocomotion.Player.Instance.inOverlay = false;
            }
            else if (GetControllerPressed() || Keyboard.current.rightAltKey.wasPressedThisFrame)
            {
                ShowMenu();
            }
        }

        private static bool riftStickClick;
        
        bool GetControllerPressed() => (usingSteamVR ? SteamVR_Actions.gorillaTag_RightJoystickClick.state : riftStickClick) && ControllerInputPoller.instance.leftControllerSecondaryButton;

        public static void ShowMenu()
        {
            if (!Menu)
            {
                MetaReportMenu.gameObject.SetActive(true);
                MetaReportMenu.enabled = true;
                MethodInfo inf = typeof(GorillaMetaReport).GetMethod("StartOverlay", BindingFlags.NonPublic | BindingFlags.Instance);
                inf.Invoke(MetaReportMenu, null);
                Menu = true;
            }
        }

        public void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();

            // do not qmod
            // rift users remain unaffected (your platform tag is "OCULUS PC")
            if (PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("quest"))
                Application.Quit();
            
            usingSteamVR = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
        }

        public void OnDisable() 
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }
    }

    [HarmonyPatch(typeof(GorillaMetaReport), "Teardown")] // GorillaMetaReport.Teardown() is called when X is pressed
    public class CheckMenuClosed
    {
        static void Postfix()
        {
            Plugin.Menu = false;
        }
    }
    [HarmonyPatch(typeof(GorillaMetaReport), "Start")] // Getting the Script when it starts
    public class CheckMenuStart
    {
        static void Postfix(GorillaMetaReport __instance)//has to be called this
        {
            Plugin.MetaReportMenu = __instance;
        }
    }
}

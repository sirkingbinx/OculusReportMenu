using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace OculusReportMenu {
    [BepInPlugin("org.oatsalmon.gorillatag.oculusreportmenu", "OculusReportMenu", "1.0.6")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool Menu, NoSecondary, ValuesInitialized, CanOpen;
        public static GorillaMetaReport MetaReportMenu;

        public void Update()
        {
            if (!ValuesInitialized || !CanOpen) return;
            
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
        // saying this is the value of:  //bool      //true                                                    //false
        bool GetControllerPressed() => NoSecondary ? ControllerInputPoller.instance.leftControllerPrimaryButton : ControllerInputPoller.instance.leftControllerSecondaryButton;

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

        public void Start()
        {
            CanOpen = false;
            ValuesInitialized = false;
        }

        public void OnEnable()
        {
            CanOpen = true;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        public void OnDisable() 
        {
            HarmonyPatches.RemoveHarmonyPatches();
            CanOpen = false;
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

            // check for HTC vive headset
            XRDisplaySubsystem displaySubsystems = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
            Debug.Log("VR Headset detected by Unity: " + displaySubsystems.SubsystemDescriptor.id);
            NoSecondary = displaySubsystems.SubsystemDescriptor.id.ToLower().Contains("htc");

            ValuesInitialized = true;
        }
    }
}

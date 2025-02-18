using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;

namespace OculusReportMenu {
    [BepInPlugin("org.oatsalmon.gorillatag.oculusreportmenu", "OculusReportMenu", "1.0.6")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool Menu, NoSecondary;
        public static GorillaMetaReport MetaReportMenu;

        public void Update()
        {
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

        public void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();

            // check for HTC vive headset
            var displaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displaySubsystems);
        
            XRDisplaySubsystem displaySubsystem = displaySubsystems[0];
            Debug.Log("VR Headset detected by Unity: " + displaySubsystem.SubsystemDescriptor.id);

            if (displaySubsystem.SubsystemDescriptor.id.Contains("HTC Vive")) {
                NoSecondary = true;
            } else {
                NoSecondary = false;
            }

            NoSecondary = displaySubsystems.SubsystemDescriptor.id.Contains("HTC Vive");
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

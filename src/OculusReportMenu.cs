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

/*
 * Dear Another Axiom,
 * 
 * Please stop patching this mod.
 * As of right now, this is the only way to bypass anti-Oculus report on
 * cheat menus such as ii's Stupid Menu, ShibaGT Gold, and others.
 * 
 * By patching this mod, you only commit more to making the cheater problem
 * worse.
 * 
 * You can ban me from your servers or anything, just please leave this
 * mod functional. (my username is @sirkingbinx if you are considering this)
 * 
 * - bingus
 */

namespace OculusReportMenu
{
    [BepInPlugin("kingbingus.oculusreportmenu", "OculusReportMenu", "2.1.0")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static Plugin instance;

        internal GorillaMetaReport Menu;
        internal GameObject ORMOccluder, ORMLeftHand, ORMRightHand;

        internal FieldInfo closeButton;
        internal MethodInfo UpdatePosition, CheckReports, OpenMenu, Teardown;

        internal bool ShowingMenu, ButtonsPressed;

        internal string OpenButton1, OpenButton2;
        internal float Sensitivity, blockButtonsUntilTimestamp;

        internal void Start() {
            Harmony.CreateAndPatchAll(GetType().Assembly, Info.Metadata.GUID);
            instance = this;

            OpenButton1 = Config.Bind("Keybinds", "OpenButton1", "LS", "One of the buttons you use to open ORM (NAN for none)").Value;
            OpenButton2 = Config.Bind("Keybinds", "OpenButton2", "RS", "One of the buttons you use to open ORM (NAN for none)").Value;
            Sensitivity = Config.Bind("Input", "Sensitivity", 0.5f, "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;

            GorillaTagger.OnPlayerSpawned(delegate
            {
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
                closeButton = typeof(GorillaMetaReport).GetField("closeButton",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            });
        }

        internal void Update() {
            blockButtonsUntilTimestamp = (float)
                typeof(GorillaMetaReport).GetField("blockButtonsUntilTimestamp",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Menu);

            if (blockButtonsUntilTimestamp > Time.time)
                return;

            if (Menu != null) {
                ButtonsPressed =
                    (CheckButtonPressedStatus(OpenButton1) && CheckButtonPressedStatus(OpenButton2))
                    | Keyboard.current.tabKey.wasPressedThisFrame;

                if (ShowingMenu) {
                    GTPlayer.Instance.disableMovement = false;
                    GTPlayer.Instance.inOverlay = false;
                    GTPlayer.Instance.InReportMenu = false;

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
                    Menu.gameObject.SetActive(true);
                    Menu.enabled = true;

                    object[] stuff = { false };
                    OpenMenu.Invoke(Menu, stuff);
                    ShowingMenu = true;
                }
            }

            if (!Menu.gameObject.activeInHierarchy && ShowingMenu)
                ShowingMenu = false;

            if (((GorillaReportButton)closeButton.GetValue(Menu)).selected)
                Teardown.Invoke(Menu, null);
        }

        internal bool CheckButtonPressedStatus(string thisEntry)
        {
            switch (thisEntry.ToUpper())
            {
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > Sensitivity;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > Sensitivity;
                case "LJ": return ControllerInputPoller.instance.leftControllerSecondaryButton;

                // right hand
                case "RP": return ControllerInputPoller.instance.rightControllerPrimaryButton;
                case "RS": return ControllerInputPoller.instance.rightControllerSecondaryButton;
                case "RT": return ControllerInputPoller.instance.rightControllerIndexFloat > Sensitivity;
                case "RG": return ControllerInputPoller.instance.rightControllerGripFloat > Sensitivity;
                case "RJ": return ControllerInputPoller.instance.rightControllerSecondaryButton;

                // NAN
                case "NAN": return true;
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
            static void Postfix(GorillaMetaReport __instance) => instance.Menu = __instance;
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        internal class StopUpdate
        {
            static void Prefix() { return; }
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "StartOverlay")]
        internal class LetGGWPCook
        {
            // This is necessary because the GGWP moderation uses the report menu to show stuff. This should make sure we don't make your game break; don't change it plz
            static void Postfix() => instance.ShowingMenu = true;
        }
    }
}

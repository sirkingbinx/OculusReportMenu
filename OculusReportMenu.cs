// OculusReportMenu
// (C) Copyright 2024 - 2025 SirKingBinx (Bingus)
// MIT License

using BepInEx;
using HarmonyLib;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR;

using GorillaNetworking;
using GorillaLocomotion;

namespace OculusReportMenu
{
    [BepInPlugin("bingus.oculusreportmenu", "OculusReportMenu", "2.2.5")]
    internal class Plugin : BaseUnityPlugin
    {
        private static GorillaMetaReport _menu;
        private static bool _menuInit;
        
        private GameObject _occluder, _leftHand, _rightHand;
        private bool _showingMenu, _buttonsPressed, _platformSteam, _useCustomKeybinds, _allowTabOpen;
        private string _openButton1, _openButton2;
        private float _sensitivity;

        internal void Start() {
            Harmony.CreateAndPatchAll(GetType().Assembly, Info.Metadata.GUID);

            // Keybinds
            _useCustomKeybinds  = Config.Bind("Keybinds", "UseCustomKeybinds", true, "Use your custom keybind settings (when off, press left + right secondaries)").Value;
            _openButton1        = Config.Bind("Keybinds", "OpenButton1", "LS", "One of the buttons you use to open ORM (NAN for none)").Value;
            _openButton2        = Config.Bind("Keybinds", "OpenButton2", "RS", "One of the buttons you use to open ORM (NAN for none)").Value;
            _allowTabOpen       = Config.Bind("Keybinds", "AllowTabOpen", true, "Allows you to press TAB to open the report menu (mostly used for testing)").Value;
            _sensitivity        = Config.Bind("Keybinds", "Sensitivity", 0.5f, "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;

            GorillaTagger.OnPlayerSpawned(delegate
            {
                _occluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");
                _leftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent");
                _rightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");
                
                _platformSteam = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
            });
        }

        internal void Update() {
            if (_menuInit)
            {
                if (_menu.blockButtonsUntilTimestamp > Time.time)
                    return;

                _buttonsPressed = 
                    (_useCustomKeybinds ? 
                        (CheckButtonPressedStatus(_openButton1) & CheckButtonPressedStatus(_openButton2)) :
                        (ControllerInputPoller.instance.leftControllerSecondaryButton & ControllerInputPoller.instance.rightControllerSecondaryButton)
                    ) | (_allowTabOpen & Keyboard.current.tabKey.wasPressedThisFrame);

                if (_showingMenu) {
                    GTPlayer.Instance.disableMovement = false;
                    GTPlayer.Instance.inOverlay = false;
                    GTPlayer.Instance.InReportMenu = false;
                    
                    _occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;

                    _rightHand.transform.SetPositionAndRotation(
                        GTPlayer.Instance.RightHand.controllerTransform.position,
                        GTPlayer.Instance.RightHand.controllerTransform.rotation);

                    _leftHand.transform.SetPositionAndRotation(
                        GTPlayer.Instance.LeftHand.controllerTransform.position,
                        GTPlayer.Instance.LeftHand.controllerTransform.rotation);

                    if (!_platformSteam)
                    {
                        // make the hands turn right on rift PCVR
                        _leftHand.transform.Rotate(90, 0, 0);
                        _rightHand.transform.Rotate(90, 0, 0);
                    }

                    _menu.CheckDistance();
                    _menu.CheckReportSubmit();
                } else if (_buttonsPressed && !_menu.gameObject.activeInHierarchy)
                {
                    _menu.gameObject.SetActive(true);
                    _menu.enabled = true;

                    _menu.StartOverlay();
                    _showingMenu = true;
                }
            }

            _showingMenu = _menu.gameObject.activeInHierarchy && _showingMenu;
            
            if (_menu.closeButton.selected || _menu.closeButton.testPress)
                _menu.Teardown();
        }

        private bool CheckButtonPressedStatus(string thisEntry)
        {
            bool temporarySClick;

            switch (thisEntry.ToUpper())
            {
                case "LP": return ControllerInputPoller.instance.leftControllerPrimaryButton;
                case "LS": return ControllerInputPoller.instance.leftControllerSecondaryButton;
                case "LT": return ControllerInputPoller.instance.leftControllerIndexFloat > _sensitivity;
                case "LG": return ControllerInputPoller.instance.leftControllerGripFloat > _sensitivity;
                case "LJ":
                    if (_platformSteam)
                        temporarySClick = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                    else
                        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out temporarySClick);

                    return temporarySClick;

                // right hand
                case "RP": return ControllerInputPoller.instance.rightControllerPrimaryButton;
                case "RS": return ControllerInputPoller.instance.rightControllerSecondaryButton;
                case "RT": return ControllerInputPoller.instance.rightControllerIndexFloat > _sensitivity;
                case "RG": return ControllerInputPoller.instance.rightControllerGripFloat > _sensitivity;
                case "RJ":
                    if (_platformSteam)
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
        
        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        internal class OnOculusUpdate
        {
            static void Postfix() => GTPlayer.Instance.InReportMenu = false;
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "Start")]
        internal class OnReportInit
        {
            static void Postfix(GorillaMetaReport __instance)
            {
                _menu = __instance;
                _menuInit = true;
            }
        }
    }
}

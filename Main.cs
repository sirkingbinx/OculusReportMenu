// OculusReportMenu
// (C) Copyright 2024 - 2026 SirKingBinx (Bingus)
// MIT License

using BepInEx;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using System;
using UnityEngine;

namespace OculusReportMenu
{
    [BepInPlugin("bingus.oculusreportmenu", "OculusReportMenu", "2.3.0")]
    internal class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        public static GorillaMetaReport _menu;

        private static bool __menuInit = false;
        public static bool _menuInit {
            get {
                if (!__menuInit)
                    __menuInit = (_menu != null);
                
                return __menuInit;
            }
            
            set => __menuInit = value; // just in case
        }
        
        private GameObject _occluder, _leftHand, _rightHand;
        public bool _platformSteam;

        public bool _showingMenu => _menu.gameObject.activeInHierarchy;

        internal void Start() {
            Instance = this;
            Harmony.CreateAndPatchAll(GetType().Assembly, Info.Metadata.GUID);

            Input.UseCustomKeybinds = Config.Bind("Keybinds",
                "UseCustomKeybinds", true,
                "Use your custom keybind settings (when off, press left + right secondaries)"
            ).Value;

            Input.EnableTabOpening = Config.Bind("Keybinds", "AllowTabOpen", false, "Press TAB to open").Value;

            Input.OpenButton1 = Config.Bind("Keybinds", "OpenButton1", "LS",
                "One of the buttons you use to open ORM (NAN for none)").Value;
            Input.OpenButton2 = Config.Bind("Keybinds", "OpenButton2", "RS",
                "One of the buttons you use to open ORM (NAN for none)").Value;
            Input.Sensitivity = Config.Bind("Keybinds", "Sensitivity", 0.5f,
                "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;

            GorillaTagger.OnPlayerSpawned(delegate
            {
                try {
                    _occluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");
                    _leftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent");
                    _rightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");
                    
                    _platformSteam = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");
                } catch (Exception ex) {
                    Debug.Log($"Failed to load OculusReportMenu: ${ex}");
                }
            });
        }

        private bool inputActivatedBefore;

        internal void Update() {
            if (_menuInit)
            {
                GTPlayer.Instance.inOverlay = _showingMenu;

                if (_menu.blockButtonsUntilTimestamp > Time.time)
                    return;

                if (_showingMenu) {
                    GTPlayer.Instance.InReportMenu = false;
                    
                    _occluder.transform.position = GorillaTagger.Instance.mainCamera.transform.position;

                    _rightHand.transform.SetPositionAndRotation(
                        GorillaTagger.Instance.rightHandTransform.position,
                        GorillaTagger.Instance.rightHandTransform.rotation);

                    _leftHand.transform.SetPositionAndRotation(
                        GorillaTagger.Instance.leftHandTransform.position,
                        GorillaTagger.Instance.leftHandTransform.rotation);

                    if (!_platformSteam)
                    {
                        _leftHand.transform.Rotate(90, 0, 0);
                        _rightHand.transform.Rotate(90, 0, 0);
                    }

                    _menu.CheckDistance();
                    _menu.CheckReportSubmit();
                } else if (!_showingMenu && !inputActivatedBefore && Input.Activated)
                {
                    inputActivatedBefore = true;
                    _menu.gameObject.SetActive(true);
                    _menu.enabled = true;

                    _menu.StartOverlay();
                }

                if (inputActivatedBefore && !Input.Activated)
                    inputActivatedBefore = false;
            }

            // these logics are seperated to save the eyes of whoever ends up needing to edit the code
            //                                                          (that's you)
            if (_showingMenu && (_menu.closeButton.selected || _menu.closeButton.testPress))
            {
                _menu.Teardown();
                inputActivatedBefore = true;
            }

            // toggle the menu off by pressing the buttons again
            if (!inputActivatedBefore && Input.Activated)
            {
                _menu.Teardown();
                inputActivatedBefore = true;
            }
        }
    }
}

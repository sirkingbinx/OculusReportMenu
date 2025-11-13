// OculusReportMenu
// (C) Copyright 2024 - 2026 SirKingBinx (Bingus)
// MIT License

/*
 * TODO: This may be a terrible idea but I want to bind the report menu to Link's "Report Abuse" button
 *       May be a little strange but this would make opening the report menu super straight forward
 *       and feel like an actual intentional feature
 *       (Still leave keybinds, but give them another way to do it)
 */

using BepInEx;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;

namespace OculusReportMenu
{
    [BepInPlugin("bingus.oculusreportmenu", "OculusReportMenu", "2.2.5")]
    internal class Plugin : BaseUnityPlugin
    {
        private static GorillaMetaReport _menu;

        private static bool __menuInit = false;
        private static bool _menuInit {
            get {
                if (!__menuInit)
                    __menuInit == (_menu != null);
                
                return __menuInit;
            };
            
            set => __menuInit = value; // just in case
        }
        
        private GameObject _occluder, _leftHand, _rightHand;
        private bool _showingMenu, _platformSteam;

        internal void Start() {
            Harmony.CreateAndPatchAll(GetType().Assembly, Info.Metadata.GUID);

            Input.UseCustomKeybinds = Config.Bind("Keybinds",
                "UseCustomKeybinds", true,
                "Use your custom keybind settings (when off, press left + right secondaries)"
            ).Value;

            Input.AllowTabOpening = Config.Bind("Keybinds", "AllowTabOpen", false, "Press TAB to open").Value;

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
                } catch (var ex) {
                    Debug.Log($"Failed to load OculusReportMenu: ${ex}");
                }
            });
        }

        internal void Update() {
            if (_menuInit)
            {
                if (_menu.blockButtonsUntilTimestamp > Time.time)
                    return;

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
                        _leftHand.transform.Rotate(90, 0, 0);
                        _rightHand.transform.Rotate(90, 0, 0);
                    }

                    _menu.CheckDistance();
                    _menu.CheckReportSubmit();
                } else if (Input.Activated && !_showingMenu)
                {
                    _menu.gameObject.SetActive(true);
                    _menu.enabled = true;

                    _menu.StartOverlay();
                }
            }

            _showingMenu = _menu.gameObject.activeInHierarchy;

            if ((_menu.closeButton.selected || _menu.closeButton.testPress) && _showingMenu)
                _menu.Teardown();
        }
    }
}

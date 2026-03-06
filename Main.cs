// OculusReportMenu
// (C) Copyright 2024 - 2026 SirKingBinx (Bingus)
// MIT License

using GorillaLocomotion;
using GorillaNetworking;
using System;

#if MELONLOADER
using System.IO;
using MelonLoader;
#endif


using UnityEngine;

namespace OculusReportMenu
{
    internal class Main : MonoBehaviour
    {
        public static Main Instance;

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
            HarmonyLib.Harmony.CreateAndPatchAll(GetType().Assembly, Constants.Guid);

#if MELONLOADER
            var configCategory = MelonPreferences.CreateCategory("Keybinds");
            configCategory.SetFilePath($"UserData{Path.DirectorySeparatorChar}OculusReportMenu.cfg", true);

            Input.OpenButton1 = configCategory.CreateEntry("OpenButton1", ORM_Button.LeftSecondary,
                description: "Button you use to open the report menu").Value;
            Input.OpenButton2 = configCategory.CreateEntry("OpenButton2", ORM_Button.RightSecondary,
                description: "Button you use to open the report menu").Value;

            Input.Sensitivity = configCategory.CreateEntry("Sensitivity", 0.5f,
                description: "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;

            Input.EnableTabOpening =
                configCategory.CreateEntry("AllowTabOpen", false, description: "Press TAB to open the report menu").Value;

            configCategory.SaveToFile();
#elif BEPINEX
            Input.UseCustomKeybinds = BepPlugin.Instance.Config.Bind("Keybinds",
                "UseCustomKeybinds", true,
                "Use your custom keybind settings (when off, press left + right secondaries)"
            ).Value;

            Input.EnableTabOpening = BepPlugin.Instance.Config.Bind("Keybinds", "AllowTabOpen", false, "Press TAB to open").Value;

            Input.OpenButton1 = BepPlugin.Instance.Config.Bind("Keybinds", "OpenButton1", ORM_Button.LeftSecondary,
                "Button you use to open the report menu").Value;
            Input.OpenButton2 = BepPlugin.Instance.Config.Bind("Keybinds", "OpenButton2", ORM_Button.RightSecondary,
                "Button you use to open the report menu").Value;
            Input.Sensitivity = BepPlugin.Instance.Config.Bind("Keybinds", "Sensitivity", 0.5f,
                "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;
#endif
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
                if (_menu.blockButtonsUntilTimestamp > Time.time)
                    return;

                if (_showingMenu) {
                    GTPlayer.Instance.InReportMenu = false;
                    GTPlayer.Instance.inOverlay = true;

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

// OculusReportMenu/Main.cs - The Main class, that handles ORM-related stuff
// (C) Copyright 2024 - 2026 SirKingBinx - MIT License

using GorillaLocomotion;
using GorillaNetworking;
using System;

#if MELONLOADER
/*
 * MelonLoader also contains some extra dependencies that we need here, all of them are
 * just for allowing custom configurations.
 */
using System.IO;
using MelonLoader;
#elif BEPINEX
using BepInEx;
#endif

using UnityEngine;

namespace OculusReportMenu;

/*
 * Here is the bulk of the code that makes reporting work, in our Main class.
 * This handles opening/closing the report menu, updating controller input,
 * loading config, and much more.
 */

internal class Main
{
#nullable enable
    public static Main? Instance;
#nullable disable
    public static GorillaMetaReport Menu;

    /*
     * This variable tells OculusReportMenu when to start updating itself.
     * If the GorillaMetaReport system isn't initialized, then we aren't needed
     * and can halt ourselves until it is.
     */
    public static bool _menuInit {
        get {
            if (!field)
                field = (Menu != null);
            
            return field;
        }
    }
    
    /*
     * These are saved for faster updating.
     * The report menu's hands, the black background (called the occluder),
     * and if you're on Steam or not (since custom input code is required if you are)
     */
    private GameObject _occluder, _leftHand, _rightHand;
    public bool _platformSteam;

    /*
     * This is just a shortcut for us, not really required.
     */
    public bool _showingMenu => Menu.gameObject.activeInHierarchy;

    internal void Start() {
        Instance = this;
        HarmonyLib.Harmony.CreateAndPatchAll(GetType().Assembly, Constants.Guid);

#if MELONLOADER
        /*
         * MelonLoader config is set up here.
         * We specify a custom path so it's easy to find it.
         */
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
        /*
         * Same thing as above, but for BepInEx.
         */

        Input.EnableTabOpening = Plugin.Instance.Config.Bind("Keybinds", "AllowTabOpen", false, "Press TAB to open").Value;

        Input.OpenButton1 = Plugin.Instance.Config.Bind("Keybinds", "OpenButton1", ORM_Button.LeftSecondary,
            "Button you use to open the report menu").Value;
        Input.OpenButton2 = Plugin.Instance.Config.Bind("Keybinds", "OpenButton2", ORM_Button.RightSecondary,
            "Button you use to open the report menu").Value;
        Input.Sensitivity = Plugin.Instance.Config.Bind("Keybinds", "Sensitivity", 0.5f,
            "Sensitivity of trigger / grip detection (0.5f = 50%)").Value;
#endif

        GorillaTagger.OnPlayerSpawned(delegate
        {
            /*
             * 99% of the time, this code doesn't require a try {} block, but we do anyway just in case.
             * This makes it so other mods will load properly, even if ours doesn't.
             */
            try {
                _platformSteam = PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam");

                _occluder = GameObject.Find("Miscellaneous Scripts/MetaReporting/ReportOccluder");
                _leftHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/LeftHandParent");
                _rightHand = GameObject.Find("Miscellaneous Scripts/MetaReporting/CollisionRB/RightHandParent");
            } catch (Exception ex) {
                Debug.Log($"Failed to load OculusReportMenu: ${ex}");
            }
        });
    }

    /*
     * This variable stops the menu opening and closing every single frame your button is held down.
     * This holds the last (updated) state of your controller inputs, so when you release the buttons
     * it will allow you to press them again, allowing for a toggle button.
     */
    private bool inputActivatedBefore;

    internal void Update() {
        if (!_menuInit)
            return; // GorillaMetaReport isn't alive yet, keep waiting

        if (Menu.blockButtonsUntilTimestamp > Time.time)
            return; // Waiting until buttons can be pressed again to update stuff

        if (_showingMenu) {
            /*
             * Make us in the report menu and in an overlay, which turns off movement and
             * stops our hands from colliding, so we don't hit or tag anybody while reporting.
             */
            GTPlayer.Instance.inOverlay = true;

            /*
             * Since the menu doesn't update itself, here is where we do that.
             * In the next 18 lines, we:
             * - update positions of the background (occluder) and your hands,
             * - fix the distance of the report menu so you can always reach it
             * - submit reports if you are done reporting stuff
             */
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

            // If the menu close button is being activated, close the menu
            if (_showingMenu && (Menu.closeButton.selected || Menu.closeButton.testPress))
                goto teardown;

            Menu.CheckDistance();
            Menu.CheckReportSubmit();
        } else if (!inputActivatedBefore && Input.Activated)
        {
            /*
             * Here's where the menu gets activated.
             */
            inputActivatedBefore = true;
            Menu.gameObject.SetActive(true);
            Menu.ToggleLevelVisibility(false);
            Menu.enabled = true;

            Menu.StartOverlay();
        }

        // This `if` block resets "inputActivatedBefore" so you can press the buttons again.
        if (inputActivatedBefore && !Input.Activated)
            inputActivatedBefore = false;

        // If you pressed your activation buttons again on the controller, close the menu
        if (!inputActivatedBefore && Input.Activated)
            goto teardown;

        return; // Stop us from reaching the "teardown" block if we don't want to close the menu yet

        /*
        * The code only reaches this `teardown` block when the menu is being closed to save code.
        * It skips the "return" statement above, don't worry.
        * This will let you move again and close the menu so your game returns.
        */
    teardown:
        Menu.Teardown();
        Menu.ToggleLevelVisibility(true);
        GTPlayer.Instance.InReportMenu = false;
        GTPlayer.Instance.inOverlay = false;

        inputActivatedBefore = true;
    }
}
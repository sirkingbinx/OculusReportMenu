// OculusReportMenu/Plugin.cs - Plugins for both mod loaders we support
// (C) Copyright 2024 - 2026 SirKingBinx - MIT License

using UnityEngine;

/*
 * This looks funny, but I promise it makes sense: cross-loader support goes here
 * This allows you to run OculusReportMenu on basically any mod loader you desire, as long
 * as some correctly set code is here.
 */


#if MELONLOADER
// Stuff for MelonLoader
using OculusReportMenu;
using MelonLoader;
[assembly: MelonInfo(typeof(Plugin), OculusReportMenu.Constants.Name, OculusReportMenu.Constants.Version, OculusReportMenu.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: HarmonyDontPatchAll]
#elif BEPINEX
// Stuff for BepInEx (a lot less compared to ML)
using BepInEx;
#endif

namespace OculusReportMenu;

#if MELONLOADER
public class Plugin : MelonMod
{
    /*
    * MelonLoader doesn't have a great "start" call that we can hook onto.
    * Instead, we wait for the first scene to load before adding OculusReportMenu to it.
    */
    public override void OnLateInitializeMelon() {
        Main.Instance ??= new Main();
        Main.Instance.Start();
    }

    public override void OnUpdate() => Main.Instance?.Update();
}
#elif BEPINEX

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    // This part is pretty straightforward.
    public static Plugin Instance;

    public void Awake()
    {
        Instance = this;
        Main.Instance = new Main();
    }

    public void Start() => Main.Instance.Start();
    public void Update() => Main.Instance.Update();
}

#endif
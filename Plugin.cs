// OculusReportMenu/Plugin.cs - Main plugin file
// (C) Copyright 2024 - 2026 SirKingBinx - MIT License

using BepInEx;

namespace OculusReportMenu;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    // This part is pretty straightforward.
    public static Plugin Instance;

    public void Awake()
    {
        // This is where we define Instance so our Main class can use them
        Instance = this;

        // Apply our patches (see Patches.cs)
        HarmonyLib.Harmony.CreateAndPatchAll(GetType().Assembly, Constants.Guid);
        Main.Instance = new Main();
    }

    public void Start() => Main.Instance.Start();
    public void Update() => Main.Instance.Update();
}
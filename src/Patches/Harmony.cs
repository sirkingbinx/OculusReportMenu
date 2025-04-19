// OculusReportMenu
// (C) Copyright 2024 - 2025 Bingus Bingusington
// MIT License

using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;

namespace OculusReportMenu.Patches
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public static string InstanceId = "";

        internal static void ApplyHarmonyPatches(BaseUnityPlugin pluginInstance)
        {
            if (!IsPatched)
            {
                if (InstanceId == "") InstanceId = pluginInstance.Info.Metadata.GUID;

                if (instance == null)
                    instance = new Harmony(InstanceId);

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            } else {
                return;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
                instance.UnpatchSelf();
            
            IsPatched = false;
        }
    }
}

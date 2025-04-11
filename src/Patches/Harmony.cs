// OculusReportMenu
// (C) Copyright 2024 - 2025 Bingus Bingusington
// MIT License

using HarmonyLib;
using System;
using System.Reflection;

namespace OculusReportMenu
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public const string InstanceId = "";

        internal static void ApplyHarmonyPatches(string thisInstanceId = "com.author.gorillatag.mod")
        {
            if (!IsPatched)
            {
                if (InstanceId == "") InstanceId = thisInstanceId;

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

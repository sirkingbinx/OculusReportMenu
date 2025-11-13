// OculusReportMenu
// (C) Copyright 2024 - 2026 SirKingBinx (Bingus)
// MIT License

using GorillaLocomotion;
using HarmonyLib;

namespace OculusReportMenu {
    internal class Patches {
        [HarmonyPatch(typeof(GorillaMetaReport), "Update")]
        internal class OnOculusUpdate
        {
            static void Postfix() => GTPlayer.Instance.InReportMenu = false;
        }

        [HarmonyPatch(typeof(GorillaMetaReport), "Start")]
        internal class OnReportInit
        {
            static void Postfix(GorillaMetaReport __instance) => Plugin._menu = __instance;
        }
    }
}
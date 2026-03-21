// OculusReportMenu/Patches.cs - Harmony Patches go here
// (C) Copyright 2024 - 2026 SirKingBinx - MIT License

using HarmonyLib;

namespace OculusReportMenu;

/*
 * Here is where Harmony Patches are stored. Harmony Patches let you call other method before, after,
 * or during other methods being called. This is useful for many use cases:
 * 
 * - You want to update something with the game loop so it runs smoother
 * - You want to update some variables passed to a method to get your own funny results
 * - In our case, You want to completely cancel a method call so you can do stuff yourself. (See OnMetaReportUpdate)
 */
internal class Patches {
    [HarmonyPatch(typeof(GorillaMetaReport), nameof(GorillaMetaReport.Update))]
    internal class OnMetaReportUpdate
    {
        /* 
         * We stop the report menu from updating itself here for a very important reason:
         *
         * - GorillaMetaReport will check it is running on Steam if not triggered via Automod.
         *     The solution here is to manually move the report menu ourselves and check for input
         *     independently (without the Steam checks obviously).
         * - This means that OculusReportMenu requires little to no updates as long as a core
         *     feature of Gorilla Tag has changed.
         * - It also shaves a tiny bit of clock cycles (but it isn't 1995 so that doesn't really
         *     matter)
         */
        static bool Prefix(GorillaMetaReport __instance)
        {
            return false;
            // return false to cancel the fn call
        }
    }

    [HarmonyPatch(typeof(GorillaMetaReport), nameof(GorillaMetaReport.Start))]
    internal class OnMetaReportInit
    {
        /*
         * This matters much less than our other Harmony patch. This method will just update
         * the main code, telling it where the GorillaMetaReport is located.
         */
        static void Postfix(GorillaMetaReport __instance) => Main.Menu = __instance;
    }
}
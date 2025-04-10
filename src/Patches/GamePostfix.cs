using UnityEngine;
using GorillaLocomotion;
using GorillaNetworking;

namespace OculusReportMenu.Patches {
    [HarmonyPatch(typeof(GorillaMetaReport), "Teardown")] // GorillaMetaReport.Teardown() is called when X is pressed
    public class CheckMenuClosed
    {
        static void Postfix()
        {
            Plugin.Menu = false;
        }
    }

    [HarmonyPatch(typeof(GorillaMetaReport), "Start")] // Getting the Script when it starts
    public class CheckMenuStart
    {
        static void Postfix(GorillaMetaReport __instance) //has to be called this
        {
            Plugin.MetaReportMenu = __instance;

            CheckDistance = typeof(GorillaMetaReport).GetMethod("CheckDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            CheckReportSubmit = typeof(GorillaMetaReport).GetMethod("CheckReportSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
            ShowMenu = typeof(GorillaMetaReport).GetMethod("StartOverlay", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    [HarmonyPatch(typeof(GorillaMetaReport), "Update")] // when gorilla tag for SteamVR detects this it automatically closes it for some reason, this fixes that problem
    public class ForceDontSetHandsManually
    {
        static void Postfix()
        {
            GTPlayer.Instance.InReportMenu = false;
        }
    }

    [HarmonyPatch(typeof(GorillaComputer), "Initialise")]
    public class GetPlayfabGameVersionPatch
    {
        static void Postfix()
        {
            if (PlayFabAuthenticator.instance.platform.PlatformTag.ToLower().Contains("steam"))
            {
                Plugin.usingSteamVR = true;
            }
        }
    }
}
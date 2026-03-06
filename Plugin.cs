using OculusReportMenu;
using UnityEngine;

#if MELONLOADER
using MelonLoader;
[assembly: MelonInfo(typeof(MelonLoaderPlugin), OculusReportMenu.Constants.Name, OculusReportMenu.Constants.Version, OculusReportMenu.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
#elif BEPINEX
using BepInEx;
#endif

namespace OculusReportMenu
{
#if MELONLOADER
    public class MelonLoaderPlugin : MelonMod
    {
        public override void OnSceneWasLoaded(int buildindex, string sceneName) =>
            new GameObject(Constants.Name).AddComponent<Main>();
    }
#elif BEPINEX
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class BepPlugin : BaseUnityPlugin
    {
        public static BepPlugin Instance;

        public void Awake()
        {
            Instance = this;
            new GameObject(Constants.Name).AddComponent<Main>();
        }
    }
#endif
}
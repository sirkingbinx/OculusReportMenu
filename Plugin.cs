using UnityEngine;

#if MELONLOADER
using MelonLoader;
[assembly: MelonInfo(typeof(MLPlugin), Constants.Name, Constants.Version, Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
#elif BEPINEX
using BepInEx;
#endif

namespace OculusReportMenu
{
#if MELONLOADER
    public class MLPlugin : MelonMod
    {
        public override void OnMelonInitialize() =>
            new GameObject(Constants.Name).AddComponent<Main>();
    }
#elif BEPINEX
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class BepPlugin : BepInPlugin
    {
        public override void Awake() =>
            new GameObject(Constants.Name).AddComponent<Main>();
    }
#endif
}
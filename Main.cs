// OculusReportMenu
// (C) Copyright 2024 - 2026 SirKingBinx (Bingus)
// MIT License

/*
 * NOTE: I have the mod currently laid out how I want it, but it is still not functional (yet).
 *	 The rest will come later, be patient
*/

using BepInEx;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace OculusReportMenu;

[BepInPlugin("bingus.oculusreportmenu", "OculusReportMenu", "3.0.0")]
public class Main : BaseUnityPlugin
{
    public static Main Instance { get; private set; }

    public bool Initialized;

	private bool _inReportMenu;
    public bool InReportMenu {
        get => _inReportMenu;
        internal set {
            GTPlayer.Instance.InReportMenu = value;
            _inReportMenu = value;
        }
    }

    void Start() {
        Instance = this;

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Info.Metadata.GUID);
        GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);

		// NOTE: Still deciding between two different options
		// relating to faking quest in competitive scenes for the new version:
		// 
		// 	A). Change the value of InReportMenu to hide hands, network the fact that
		//      OculusReportMenu is installed to prevent competitive players from
		//      cheating.
		//  B). Change the value of InReportMenu to hide hands, don't network the
		//      mod's existance, comp players have to deal with it.
		//  C). Don't hide hands, don't network stuff, so comp players get what
		//      they want. 
    }

    void OnPlayerSpawned() {
        Initialized = true;
    }

	void LateUpdate() {
		if (!Initialized) return;
	}

    public static T Load<T>(string path, string name) where T : Object
	{
		var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(path));
		var obj = ab.LoadAsset<T>(name);
		
		if (obj.Uninitialized())
			Debug.LogError($"Cannot load assetbundle \"{path}\" object \"{name}\" to type \"{typeof(T).FullName}.\nValid streams: \n\t{Assembly.GetExecutingAssembly().GetManifestResourceNames().Join("\n\t")}");
			
		ab.Unload(false);

		return obj;
	}
}
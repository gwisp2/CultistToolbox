using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MoMEssentials.DeterministicRandom;
using MoMEssentials.Patch;
using MoMEssentials.UI;
using UnityEngine;

namespace MoMEssentials
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        internal static ConfigEntry<KeyboardShortcut> ConfigUiKey { get; set; }

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            // Configuration
            ConfigUiKey = Config.Bind("General", "UIKey", new KeyboardShortcut(KeyCode.F6));
            // Patch methods
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            // Make random deterministic
            TrackActionPatch.OnActionMethodStart += (action) => DeterministicRandomFacade.OpenContext(action);
            TrackActionPatch.OnActionMethodEnd += DeterministicRandomFacade.CloseContext;
            // Create UI
            var pluginObject = new GameObject(MyPluginInfo.PLUGIN_GUID);
            DontDestroyOnLoad(pluginObject);
            pluginObject.AddComponent<EssentialsUI>();
            pluginObject.AddComponent<IconFontLocator>();
        }
    }
}
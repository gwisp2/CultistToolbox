﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MoMEssentials.AdvancedCollectionManager;
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
        internal static ConfigEntry<bool> ConfigShowExpansionIcon { get; set; }
        internal static ConfigEntry<KeyboardShortcut> ConfigSkipPuzzleShortcut { get; set; }
        internal static ConfigEntry<AdvancedUserCollection> ConfigCollection { get; set; }
        internal static ConfigEntry<ItemComponentTypes> ConfigScenarioRestrictedComponentTypes { get; set; }

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            // Add TOML converters
            AdvancedUserCollection.RegisterTomlConverter();
            // Configuration
            ConfigUiKey = Config.Bind("General", "UIKey", new KeyboardShortcut(KeyCode.F6));
            ConfigShowExpansionIcon = Config.Bind("General", "ShowExpansionIcon", true);
            ConfigSkipPuzzleShortcut = Config.Bind("General", "SkipPuzzleKey", KeyboardShortcut.Empty);
            ConfigCollection = Config.Bind("General", "Collection", new AdvancedUserCollection(),
                new ConfigDescription("available expansions", null,
                    new ConfigurationManagerAttributes
                        { CustomDrawer = AdvancedCollectionManagerUi.DrawCollectionEditor, HideDefaultButton = true }));

            var restrictableTypes =
                ItemComponentTypes.Items | ItemComponentTypes.Monsters | ItemComponentTypes.MythosEvents;
            ConfigScenarioRestrictedComponentTypes = Config.Bind(
                section: "General",
                key: "ScenarioRestrictedComponentTypes",
                defaultValue: ItemComponentTypes.None,
                new ConfigDescription(
                    "Use these components only from expansions that are required by a scenario",
                    new AcceptableValueFlags<ItemComponentTypes>(restrictableTypes),
                    new ConfigurationManagerAttributes()
                    {
                        CustomDrawer = AdvancedCollectionManagerUi.DrawScenarioRestrictedComponents
                    })); // Patch methods
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            // Make random deterministic
            TrackActionPatch.OnActionMethodStart += (action) => DeterministicRandomFacade.OpenContext(action);
            TrackActionPatch.OnActionMethodEnd += DeterministicRandomFacade.CloseContext;
            // Setup collection manager
            UserCollectionManagerPatch.Setup();
            // Create UI
            var pluginObject = new GameObject(MyPluginInfo.PLUGIN_GUID);
            DontDestroyOnLoad(pluginObject);
            pluginObject.AddComponent<EssentialsUI>();
            pluginObject.AddComponent<IconFontLocator>();
            pluginObject.AddComponent<PuzzleSkipper>();
        }
    }
}
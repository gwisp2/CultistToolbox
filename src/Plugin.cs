using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CultistToolbox.AdvancedCollectionManager;
using CultistToolbox.DeterministicRandom;
using CultistToolbox.Patch;
using CultistToolbox.UI;
using HarmonyLib;
using UnityEngine;

namespace CultistToolbox;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    internal static ConfigEntry<KeyboardShortcut> ConfigUiKey { get; set; }
    internal static ConfigEntry<bool> ConfigShowExpansionIcon { get; set; }
    internal static ConfigEntry<KeyboardShortcut> ConfigSkipPuzzleShortcut { get; set; }
    internal static ConfigEntry<AdvancedUserCollection> ConfigCollection { get; set; }
    internal static ConfigEntry<ItemComponentTypes> ConfigScenarioRestrictedComponentTypes { get; set; }

    internal static GameObject PluginObject;

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
        ConfigUiKey = Config.Bind("General", "UIKey", new KeyboardShortcut(KeyCode.F6));
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
        TrackFsmActionsPatch.OnActionMethodStart += (action) => DeterministicRandomFacade.OpenContext(action);
        TrackFsmActionsPatch.OnActionMethodEnd += DeterministicRandomFacade.CloseContext;
        // Setup collection manager
        UserCollectionManagerPatch.Setup();
        // Create UI
        PluginObject = new GameObject(MyPluginInfo.PLUGIN_GUID);
        DontDestroyOnLoad(PluginObject);
        PluginObject.AddComponent<ToolboxWindow>();
        PluginObject.AddComponent<IconFontLocator>();
        PluginObject.AddComponent<PuzzleSkipper>();
    }
}
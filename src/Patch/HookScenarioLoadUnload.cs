using System;
using FFG.MoM;
using HarmonyLib;

namespace CultistToolbox.Patch;

/**
 * CultistToolbox UI often updates its information when a scenario is loaded.
 * This class provides a hook for that.
 */
[HarmonyPatch]
public class HookScenarioLoadUnload
{
    public static bool ScenarioLoaded { get; private set; }
    public static event Action ScenarioLoadingComplete;
    public static event Action ScenarioShutdown;

    [HarmonyPatch(typeof(TransitionController), nameof(TransitionController.LoadingComplete))]
    [HarmonyPostfix]
    private static void PostLoadingComplete()
    {
        ScenarioLoaded = true;
        ScenarioLoadingComplete?.Invoke();
    }

    [HarmonyPatch(typeof(GameData), "Shutdown")]
    [HarmonyPostfix]
    private static void PostShutdown()
    {
        ScenarioLoaded = false;
        ScenarioShutdown?.Invoke();
    }
}
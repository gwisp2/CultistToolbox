using System;
using FFG.MoM;
using HarmonyLib;

namespace CultistToolbox.Patch;

/**
 * CultistToolbox UI often updates its information when a scenario is loaded.
 * This class provides a hook for that.
 */
[HarmonyPatch(typeof(TransitionController), nameof(TransitionController.LoadingComplete))]
public class HookScenarioLoadingCompletePatch
{
    public static event Action ScenarioLoadingComplete;

    private static void Postfix()
    {
        ScenarioLoadingComplete?.Invoke();
    }
}
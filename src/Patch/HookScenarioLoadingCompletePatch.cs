using System;
using FFG.MoM;
using HarmonyLib;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(TransitionController), nameof(TransitionController.LoadingComplete))]
public class HookScenarioLoadingCompletePatch
{
    public static event Action ScenarioLoadingComplete;

    private static void Postfix()
    {
        ScenarioLoadingComplete?.Invoke();
    }
}
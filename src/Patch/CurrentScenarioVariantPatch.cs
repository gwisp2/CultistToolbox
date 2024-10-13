using FFG.MoM;
using HarmonyLib;

namespace MoMEssentials.Patch;

[HarmonyPatch]
public class CurrentScenarioVariantPatch
{
    private static ScenarioVariant _currentScenarioVariant;

    public static ScenarioVariant CurrentScenarioVariant
    {
        get => _currentScenarioVariant;
        private set
        {
            _currentScenarioVariant = value;
            Plugin.Logger.LogDebug("CurrentScenarioVariant: " + (_currentScenarioVariant?.name ?? "[null]"));
        }
    }

    [HarmonyPatch(typeof(SetupViewController), nameof(SetupViewController.CoroutineContinueFromInvestigator))]
    [HarmonyPrefix]
    private static void PreCoroutineContinueFromInvestigator(SetupViewController __instance)
    {
        ScenarioVariant scenarioVariant = __instance.SelectedVariant;
        if (scenarioVariant == null || scenarioVariant.name == "Random Variant")
        {
            // Select a bit earlier
            // We need to limit available starting items (see usages of Plugin.ConfigLimitAvailableItems)
            var selectedScenario = __instance.PanelScenarioSelect.SelectedScenario;
            __instance.SelectedVariant = CurrentScenarioVariant = __instance
                .FindPossibleVariants(selectedScenario)
                .GetRandomElement();
        }
        else
        {
            CurrentScenarioVariant = scenarioVariant;
        }
    }

    [HarmonyPatch(typeof(TransitionController), "TransitionToSavedGameScreen")]
    [HarmonyPostfix]
    private static void PostTransitionToSavedGameScreen(ErrorCode __result)
    {
        if (__result == ErrorCode.NO_ERROR)
        {
            CurrentScenarioVariant = GameData.ScenarioVariant;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "ImplQuitToTitlescreen")]
    private static void PostImplQuitToTitlescreen()
    {
        CurrentScenarioVariant = null;
    }
}
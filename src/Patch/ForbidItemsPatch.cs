using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using HarmonyLib;

namespace MoMEssentials.Patch;

[HarmonyPatch]
public class ForbidItemsPatch
{
    private static ScenarioVariant _selectedScenarioVariant;

    [HarmonyPatch(typeof(SetupViewController), nameof(SetupViewController.CoroutineContinueFromInvestigator))]
    [HarmonyPrefix]
    private static void PreCoroutineContinueFromInvestigator(SetupViewController __instance)
    {
        ScenarioVariant scenarioVariant = __instance.SelectedVariant;
        if (scenarioVariant == null || scenarioVariant.name == "Random Variant")
        {
            // Select a variant now so it will be used in PostGetAvailableItems when starting items are generated
            var selectedScenario = __instance.PanelScenarioSelect.SelectedScenario;
            __instance.SelectedVariant = _selectedScenarioVariant = __instance
                .FindPossibleVariants(selectedScenario)
                .GetRandomElement();
        }
        else
        {
            _selectedScenarioVariant = scenarioVariant;
        }
    }

    [HarmonyPatch(typeof(AvailableComponentsManager), nameof(AvailableComponentsManager.GetAvailableItems))]
    [HarmonyPostfix]
    private static void PostGetAvailableItems(ref IEnumerable<ItemModel> __result)
    {
        if (!Plugin.ConfigLimitAvailableItems.Value)
        {
            return;
        }

        var selectedVariant = GameData.ScenarioVariant ?? _selectedScenarioVariant;
        if (selectedVariant == null)
        {
            Plugin.Logger.LogError("AvailableComponentsManager.GetAvailableItems was called outside of scenario.");
            return;
        }

        List<ProductModel> productWhileList =
            [MoMDBManager.DB.GetProductByCode("MAD20"), ..selectedVariant.RequiredAdditionalProducts];

        bool IsItemModelAllowed(ItemModel itemModel)
        {
            var products = ItemDatabase.Instance.GetProducts(itemModel).ToList();
            return !products.Any() || products.Any(product => productWhileList.Contains(product));
        }

        __result = __result.Where(IsItemModelAllowed);
    }
}
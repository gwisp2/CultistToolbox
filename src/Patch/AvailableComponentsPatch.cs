using System.Collections.Generic;
using System.Linq;
using CultistToolbox.AdvancedCollectionManager;
using FFG.MoM;
using HarmonyLib;

namespace CultistToolbox.Patch;

/**
 * Patch available components and investigators based on CultistToolbox.advanced collection management
 */
[HarmonyPatch]
public class AvailableComponentsPatch
{
    private static readonly AccessTools.FieldRef<AvailableComponentsManager, Dictionary<FeatureModel, int>>
        FeatureCountsRef =
            AccessTools.FieldRefAccess<AvailableComponentsManager, Dictionary<FeatureModel, int>>("_featureCounts");

    [HarmonyPatch(typeof(MoMDatabase), "GetAvailableInvestigators")]
    [HarmonyPrefix]
    public static bool PreGetAvailableInvestigators(MoMDatabase __instance, ref IEnumerable<InvestigatorModel> __result)
    {
        HashSet<InvestigatorModel> source = new HashSet<InvestigatorModel>();
        var products = AdvancedCollectionFacade.GetCurrentAdvancedUserCollection().Items
            .Where(i => i.HasInvestigators)
            .Select(i => i.ProductModel);
        foreach (var product in products)
        {
            source.UnionWith(product.Investigators);
        }

        __result = source.OrderBy(i => Localization.Get(i.Name.Key));
        return false;
    }

    [HarmonyPatch(typeof(AvailableComponentsManager), "Reset")]
    [HarmonyPrefix]
    private static bool PreReset(AvailableComponentsManager __instance)
    {
        if (FeatureCountsRef(__instance) == null)
        {
            Plugin.Logger.LogWarning("AvailableComponentsManagerPatch.PreReset: FeatureCountsRef(__instance) is null");
            return false;
        }

        // Clear feature counts
        var featureCounts = FeatureCountsRef(__instance);
        featureCounts.Clear();

        // Get product collection
        var collection = AdvancedCollectionFacade.GetEffectiveCollectionForCurrentScenario();

        // Add monsters
        foreach (var productModel in collection.Items.Where(i => i.HasMonsters).Select(i => i.ProductModel))
        {
            foreach (MonsterComponentEntry monster in productModel.Monsters)
            {
                featureCounts[monster.monster] = featureCounts.GetValueSafe(monster.monster) + monster.count;
            }
        }

        // Add items
        foreach (var productModel in collection.Items.Where(i => i.HasItems).Select(i => i.ProductModel))
        {
            foreach (ItemComponentEntry item in productModel.Items)
            {
                featureCounts[item.item] = featureCounts.GetValueSafe(item.item) + item.count;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(MythosEventDeckManager), "HasProductRequirements")]
    [HarmonyPrefix]
    public static bool PreHasProductRequirements(MythosEventModel model, ref bool __result)
    {
        if (model.RequiredProducts == null || model.RequiredProducts.Length == 0)
        {
            __result = true;
            return false;
        }

        var effectiveCollection = AdvancedCollectionFacade.GetEffectiveCollectionForCurrentScenario();
        __result = model.RequiredProducts == null || effectiveCollection.HasAllProducts(model.RequiredProducts,
            ItemComponentTypes.MythosEvents);
        return false;
    }
}
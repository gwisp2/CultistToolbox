using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MoMEssentials.AdvancedCollectionManager;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(AvailableComponentsManager))]
public class AvailableComponentsManagerPatch
{
    private static readonly AccessTools.FieldRef<AvailableComponentsManager, Dictionary<FeatureModel, int>>
        FeatureCountsRef =
            AccessTools.FieldRefAccess<AvailableComponentsManager, Dictionary<FeatureModel, int>>("_featureCounts");

    [HarmonyPatch("Reset")]
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
        var collection = AdvancedCollectionFacade.GetEffectiveCollection();

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
}
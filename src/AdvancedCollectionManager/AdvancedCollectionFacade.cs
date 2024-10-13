using System.Linq;
using MoMEssentials.Patch;

namespace MoMEssentials.AdvancedCollectionManager;

public static class AdvancedCollectionFacade
{
    public static AdvancedUserCollection GetCurrentAdvancedUserCollection()
    {
        return new AdvancedUserCollection(Plugin.ConfigCollection.Value);
    }

    public static AdvancedUserCollection GetEffectiveCollectionForCurrentScenario()
    {
        var collection = GetCurrentAdvancedUserCollection();
        var scenarioVariant = CurrentScenarioVariantPatch.CurrentScenarioVariant;
        if (scenarioVariant == null)
        {
            Plugin.Logger.LogWarning("AdvancedCollectionFacade.GetEffectiveCollection: CurrentScenarioVariant is null");
            return collection;
        }

        // Remove items that are not required in this scenario
        if (Plugin.ConfigLimitAvailableItems.Value)
        {
            foreach (var collectionItem in collection.Items)
            {
                if (collectionItem.ProductModel.CanToggle &&
                    !scenarioVariant.RequiredAdditionalProducts.Contains(collectionItem.ProductModel))
                    collectionItem.HasItems = false;
            }
        }

        return collection;
    }
}
using System.Linq;
using CultistToolbox.Patch;

namespace CultistToolbox.AdvancedCollectionManager;

public static class AdvancedCollectionFacade
{
    private static AdvancedUserCollection _effectiveCollection;
    private static AdvancedUserCollection _lastCollection;
    private static ScenarioVariant _lastScenarioVariant;

    public static AdvancedUserCollection GetCurrentAdvancedUserCollection()
    {
        return Plugin.ConfigCollection.Value;
    }

    public static AdvancedUserCollection GetEffectiveCollectionForCurrentScenario()
    {
        if (_lastCollection == Plugin.ConfigCollection.Value &&
            _lastScenarioVariant == CurrentScenarioVariantPatch.CurrentScenarioVariant) return _effectiveCollection;

        var collection = GetCurrentAdvancedUserCollection();
        var scenarioVariant = CurrentScenarioVariantPatch.CurrentScenarioVariant;
        if (scenarioVariant == null)
        {
            Plugin.Logger.LogWarning("AdvancedCollectionFacade.GetEffectiveCollection: CurrentScenarioVariant is null");
            return collection;
        }

        // Remove items from products that are not required in the current scenario
        var collectionCopy = collection.Copy();
        var isFilteringItems = Plugin.ConfigScenarioRestrictedComponentTypes.Value.HasFlag(ItemComponentTypes.Items);
        var isFilteringMonsters =
            Plugin.ConfigScenarioRestrictedComponentTypes.Value.HasFlag(ItemComponentTypes.Monsters);
        var isFilteringMythosEvents =
            Plugin.ConfigScenarioRestrictedComponentTypes.Value.HasFlag(ItemComponentTypes.MythosEvents);

        foreach (var collectionItem in collectionCopy.Items)
        {
            if (!collectionItem.ProductModel.CanToggle ||
                scenarioVariant.RequiredAdditionalProducts.Contains(collectionItem.ProductModel)) continue;
            if (isFilteringItems)
                collectionItem.HasItems = false;
            if (isFilteringMonsters)
                collectionItem.HasMonsters = false;
            if (isFilteringMythosEvents)
                collectionItem.HasMythosEvents = false;
            collectionItem.HasTiles = false;
        }

        _lastCollection = collection;
        _lastScenarioVariant = scenarioVariant;
        _effectiveCollection = collectionCopy.Freeze();

        return collection;
    }
}
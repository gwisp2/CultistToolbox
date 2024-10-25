using System.Collections.Generic;
using System.Linq;
using FFG.MoM;

namespace CultistToolbox.DeterministicRandom;

public static class Extensions
{
    public static IEnumerable<ItemModel> Filter(this IEnumerable<ItemModel> items, ItemTraits includeTraits,
        ItemTraits excludeTraits, ItemFilter filter)
    {
        IEnumerable<ItemModel> itemModels = items;
        switch (filter)
        {
            case ItemFilter.Spells_And_Common_Items:
                itemModels = itemModels.Where(p => p.Type != ItemType.Unique);
                break;
            case ItemFilter.Spells:
                itemModels = itemModels.Where(p => p.Type == ItemType.Spell);
                break;
            case ItemFilter.Common_Items:
                itemModels = itemModels.Where(p => p.Type == ItemType.Common);
                break;
        }

        if (includeTraits != 0)
            itemModels = itemModels.Where((p => p.HasAllTraits(includeTraits)));
        if (excludeTraits != 0)
            itemModels = itemModels.Where((p => !p.HasAnyTraits(excludeTraits)));
        return itemModels;
    }
}
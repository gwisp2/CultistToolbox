using System.Collections.Generic;
using System.Linq;

namespace CultistToolbox.DeterministicRandom;

public class ItemSpawnPriorities
{
    // First item is spawned if available (i.e. not spawned before)
    // Otherwise first available item is spawned
    public IReadOnlyList<ItemModel> Items { get; }

    public ItemSpawnPriorities(IEnumerable<ItemModel> items)
    {
        Items = items.Select(item => ItemDatabase.Instance.GetPrimaryItemModel(item)).ToList().AsReadOnly();
    }

    public ItemModel Primary()
    {
        return Items.Count > 0 ? Items[0] : null;
    }
}
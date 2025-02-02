using System;
using System.Collections.Generic;
using CultistToolbox.DeterministicRandom;
using CultistToolbox.FsmExport;
using FFG.MoM;
using FFG.MoM.Actions;
using HarmonyLib;
using HutongGames.PlayMaker;

namespace CultistToolbox.FsmTools;

public class ItemMentionParameters
{
    public ItemModel Item;
    public ItemFilter Filter;
    public ItemTraits RequiredTraits = 0;
    public ItemTraits ExcludeTraits = 0;

    public override string ToString()
    {
        if (Item != null)
        {
            return Item.Name.Key;
        }

        var filterName = Enum.GetName(typeof(ItemFilter), Filter);
        var requiredTraits = ActionE.GetFlagNames(RequiredTraits).Join(delimiter: ",");
        var excludedTraits = ActionE.GetFlagNames(ExcludeTraits).Join(delimiter: ",");
        requiredTraits = requiredTraits == "" ? "" : " +" + requiredTraits;
        excludedTraits = excludedTraits == "" ? "" : " -" + excludedTraits;
        return $"[{filterName}{requiredTraits}{excludedTraits}]";
    }

    public static IEnumerable<ItemMentionParameters> FromAction(IFsmStateAction action)
    {
        if (action is SpawnItem spawnItem)
        {
            yield return FromSpawnItem(spawnItem);
        }
        else
        {
            var message = Predictor.ExtractMessage(action);
            if (message != null)
            {
                foreach (var itemMentionParameters in FromLocalization(message))
                {
                    yield return itemMentionParameters;
                }
            }
        }
    }

    public static ItemMentionParameters FromSpawnItem(SpawnItem spawnItem)
    {
        var p = new ItemMentionParameters
        {
            Item = spawnItem.Item,
            ExcludeTraits = spawnItem.ExcludeTraits,
            Filter = spawnItem.Filter,
            RequiredTraits = spawnItem.RequiredTraits
        };
        return p;
    }

    public static IEnumerable<ItemMentionParameters> FromLocalization(MoM_LocalizationPacket packet)
    {
        int nArgs = packet.CalculateArguments();
        for (int i = 0; i < nArgs; i++)
        {
            var insert = packet.Inserts[i];
            switch (insert.Filter)
            {
                case LocalizationFilterType.RandomItem:
                    yield return new ItemMentionParameters
                    {
                        Filter = insert.ItemFilter,
                        ExcludeTraits = insert.ExcludeItemTraits,
                        RequiredTraits = insert.IncludeItemTraits
                    };
                    break;
                case LocalizationFilterType.SpecificLocalizationKey:
                {
                    if (insert.SpecificLocalizationKey?.Key == null) break;
                    var item = ItemDatabase.Instance.GetItemByNameKey(insert.SpecificLocalizationKey.Key);
                    if (item != null)
                    {
                        yield return new ItemMentionParameters
                        {
                            Item = item,
                        };
                    }

                    break;
                }
            }
        }
    }
}
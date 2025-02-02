using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using FFG.MoM.Actions;
using HutongGames.PlayMaker;

namespace CultistToolbox.DeterministicRandom;

public static class Predictor
{
    public static List<ItemSpawnPriorities> PredictSpawnedItems(IFsmStateAction action)
    {
        using (DeterministicRandomFacade.OpenContextForSimulation(action))
        {
            if (action is SpawnItem spawnItem)
            {
                var priorities = PredictSpawnItemPriorities(spawnItem);
                return priorities != null ? [priorities] : [];
            }

            return PredictMessage(action)?.ItemSpawnPriorities ?? [];
        }
    }

    public static string PredictDisplayedText(IFsmStateAction action)
    {
        using (DeterministicRandomFacade.OpenContextForSimulation(action))
        {
            return PredictMessage(action)?.LocalizedText;
        }
    }

    private static LocalizationPredictor PredictMessage(IFsmStateAction action)
    {
        var packet = ExtractMessage(action);
        if (packet == null) return null;
        var predictor = new LocalizationPredictor(packet);
        switch (action)
        {
            case DisplayMessageBase:
            case ShowMenuMessage:
                predictor.PredictInCurrentActionContext();
                break;
            case SpawnItem spawnItem:
                var item = PredictSpawnItemPriorities(spawnItem).Primary();
                if (item == null) return null;
                predictor.PredictInCurrentActionContext(recentItem: item);
                break;
            case SpawnMonster spawnMonster:
                var monster = PredictSpawnedMonsterInCurrentContext(spawnMonster);
                if (monster == null) return null;
                predictor.PredictInCurrentActionContext(recentMonster: monster);
                break;
        }

        return predictor;
    }

    public static MoM_LocalizationPacket ExtractMessage(IFsmStateAction action)
    {
        switch (action)
        {
            case DisplayMessageBase displayMessageBase:
                return displayMessageBase.Message;
            case SpawnItem spawnItem:
                return spawnItem.Message;
            case SpawnMonster spawnMonster:
                return spawnMonster.Message;
            case ShowMenuMessage showMenuMessage:
                return showMenuMessage.Message;
        }

        return null;
    }

    private static MonsterModel PredictSpawnedMonsterInCurrentContext(SpawnMonster spawnMonster)
    {
        return MoM_MonsterManager.GetRandomMonster(spawnMonster.RequiredTraits, spawnMonster.ExcludeTraits);
    }

    private static ItemSpawnPriorities PredictSpawnItemPriorities(SpawnItem spawnItem)
    {
        if (spawnItem.Item != null)
        {
            return new ItemSpawnPriorities([spawnItem.Item]);
        }

        var candidates = (MoM_ItemManager.GetAllValidItems() ?? []).Filter(spawnItem.RequiredTraits,
            spawnItem.ExcludeTraits, spawnItem.Filter).ToList();
        if (!candidates.Any()) return null;
        var sortedElements = DeterministicRandomFacade.SortElementsByRandomPriority(candidates);

        return new ItemSpawnPriorities(sortedElements);
    }
}
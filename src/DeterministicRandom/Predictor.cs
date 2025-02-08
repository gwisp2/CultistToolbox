using System.Linq;
using FFG.MoM;
using FFG.MoM.Actions;
using HutongGames.PlayMaker;

namespace CultistToolbox.DeterministicRandom;

public static class Predictor
{
    public static string PredictDisplayedText(IFsmStateAction action)
    {
        using (DeterministicRandomFacade.OpenContextForSimulation(action))
        {
            return PredictMessage(action)?.LocalizedText;
        }
    }

    public static LocalizationPredictor PredictMessage(IFsmStateAction action)
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
                var monsterModel = PredictSpawnedMonsterInCurrentContext(spawnMonster);
                if (monsterModel == null) return null;
                predictor.PredictInCurrentActionContext(recentMonsterName: Coalesce(spawnMonster.MonsterName,
                    monsterModel.Name));
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

    private static LocalizationPacket Coalesce(params LocalizationPacket[] packets) =>
        packets
            .FirstOrDefault(p => p != null && Localization.Has(p.Key));

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
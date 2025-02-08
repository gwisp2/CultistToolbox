using System;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;

namespace CultistToolbox.DeterministicRandom;

public class LocalizationPredictor
{
    private MoM_LocalizationPacket _packet;
    private List<MoM_LocalizationPacket.PacketInsert> _inserts;
    private List<InvestigatorModel> _investigatorPool;
    private List<MoM_MapTile> _tilePool;
    private List<ItemModel> _itemPool;
    private List<RoomModel> _roomPool;
    private ItemModel _recentItem = null;
    private LocalizationPacket _recentMonsterName = null;

    private bool _hasUntranslatableKeys = false;
    private string _localizedText = null;
    private List<ItemSpawnPriorities> _itemSpawnPriorities = new();
    private List<ItemModel> _spawnedItems = new();
    private List<MonsterModel> _spawnedMonsters = new();

    public IReadOnlyList<MoM_LocalizationPacket.PacketInsert> UsedInserts => _inserts.AsReadOnly();

    public List<ItemSpawnPriorities> ItemSpawnPriorities => _itemSpawnPriorities.ToList();
    public List<ItemModel> SpawnedItems => _spawnedItems.ToList();
    public List<MonsterModel> SpawnedMonsters => _spawnedMonsters.ToList();
    public string LocalizedText => _localizedText;
    public bool HasUntranslatableKeys => _hasUntranslatableKeys;

    public LocalizationPredictor(MoM_LocalizationPacket packet)
    {
        _packet = packet;
        var nArgs = packet.CalculateArguments();
        _inserts = packet.Inserts.Where((_, index) => index < nArgs).ToList();
    }

    public string PredictInCurrentActionContext(ItemModel recentItem = null,
        LocalizationPacket recentMonsterName = null)
    {
        this._recentItem = recentItem;
        this._recentMonsterName = recentMonsterName;
        this._spawnedItems.Clear();
        this._spawnedMonsters.Clear();
        this._itemSpawnPriorities.Clear();

        if (_inserts.Count is 0 or > 5)
        {
            _localizedText = LocalizeKey(_packet.Key);
            return _localizedText;
        }

        string str = "";
        try
        {
            string key = this._packet.Key;
            string[] strArray = new string[this._inserts.Count];
            _investigatorPool = null;
            _tilePool = MoM_MapTileManager.GetCurrentTilesCopy();
            _roomPool = MoM_RoomManager.GetAllRoomsCopy(true);
            _itemPool = MoM_ItemManager.GetAllValidItems().ToList();
            int numRandomInvestigators =
                this._inserts.Count(insert => insert.Filter == LocalizationFilterType.RandomInvestigator);
            if (numRandomInvestigators > 1)
                this._investigatorPool =
                    new List<InvestigatorModel>(MoM_InvestigatorManager.Investigators);
            for (int index = 0; index < this._inserts.Count; ++index)
            {
                var insert = _inserts[index];
                strArray[index] = ResolveInsert(insert);
                if (insert.Filter == LocalizationFilterType.RandomInvestigator &&
                    insert.Gender == InvestigatorGender.Female)
                    key = this._packet.Key + "_ALT";
            }

            _localizedText = string.Format(LocalizeKey(key), strArray);
            return _localizedText;
        }
        catch (FormatException ex)
        {
            Plugin.Logger.LogWarning(ex);
            _localizedText = null;
            return null;
        }
    }

    private string LocalizeKey(string key)
    {
        if (!Localization.Has(key))
        {
            _hasUntranslatableKeys = true;
            return key;
        }

        return Localization.Get(key);
    }

    private string ResolveInsert(MoM_LocalizationPacket.PacketInsert insert)
    {
        string str1 = string.Empty;
        string str2;
        switch (insert.Filter)
        {
            case LocalizationFilterType.RandomInvestigator:
                str1 = "(Random Investigator)";
                InvestigatorModel investigatorModel =
                    !_investigatorPool.Any()
                        ? MoM_InvestigatorManager.GetRandomInvestigator(true)
                        : DeterministicRandomFacade.GetRandomElementAndRemove(_investigatorPool);
                if (investigatorModel != null)
                {
                    int num = MoM_InvestigatorManager.Investigators.IndexOf(investigatorModel);
                    insert.CompressedIntData = num;
                    insert.CompressedExtraData = (int)investigatorModel.Gender;
                    insert.Gender = investigatorModel.Gender;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.MostRecentItem:
                if (_recentItem != null)
                {
                    insert.CompressedStringData = _recentItem.Name.Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                    break;
                }

                str1 = ResolveInsertWithRandomItem(insert) ?? "(Random Item)";
                break;
            case LocalizationFilterType.RandomItem:
                str1 = ResolveInsertWithRandomItem(insert) ?? "(Random Item)";
                break;
            case LocalizationFilterType.MostRecentMonster:
                str1 = "(Most Recent Monster)";
                if (_recentMonsterName != null && Localization.Has(_recentMonsterName.Key))
                {
                    insert.CompressedStringData = _recentMonsterName.Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.RandomMonster:
                str1 = "(Random Monster)";
                MonsterModel randomMonster =
                    MoM_MonsterManager.GetRandomMonster(insert.IncludeMonsterTraits, insert.ExcludeMonsterTraits);
                if (randomMonster != null)
                {
                    _spawnedMonsters.Add(randomMonster);
                    insert.CompressedStringData = randomMonster.Name.Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.RandomTile:
                str1 = "(Random Tile)";
                MoM_MapTile moMMapTile = _tilePool is not { Count: > 0 }
                    ? MoM_MapTileManager.GetRandomCurrentTile()
                    : DeterministicRandomFacade.GetRandomElementAndRemove(_tilePool);
                if (moMMapTile != null &&
                    !(moMMapTile.Model == null))
                {
                    insert.CompressedStringData = moMMapTile.Model.Name.Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.RandomRoom:
                str1 = "(Random Room)";
                RoomModel roomModel = this._roomPool == null || this._roomPool.Count <= 0
                    ? MoM_RoomManager.GetRandomCurrentRoom(false)
                    : DeterministicRandomFacade.GetRandomElementAndRemove(_roomPool);
                if (roomModel != null)
                {
                    insert.CompressedStringData = roomModel.Name.Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.Variable_Bool:
                str2 = "(Variable Bool)";
                bool flag = insert.VariableBool is { Value: true };
                insert.CompressedIntData = flag ? 1 : 0;
                str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                break;
            case LocalizationFilterType.Variable_Int:
                str1 = "(Variable Int)";
                if (insert.VariableInt != null)
                {
                    insert.CompressedIntData = insert.VariableInt.Value;
                    insert.CompressedExtraData = insert.Delta;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.PlayerCount:
                str2 = "(Player Count)";
                insert.CompressedIntData = GameData.PlayerCount;
                insert.CompressedExtraData = insert.Delta;
                str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                break;
            case LocalizationFilterType.RandomLocalizationKey:
                str1 = "(Random Localization Key)";
                var options = insert.LocalizationOptions
                    .Where(opt => opt != null && !string.IsNullOrEmpty(opt.Key))
                    .Select(opt => opt.Weight);
                var weightedIndex = DeterministicRandomFacade.GetRandomWeightedIndex(options);
                int randomWeightedIndex = weightedIndex;
                if (randomWeightedIndex >= 0)
                {
                    insert.CompressedStringData = insert.LocalizationOptions[randomWeightedIndex].Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.SpecificLocalizationKey:
                str1 = "(Specific Localization Key)";
                if (insert.SpecificLocalizationKey != null && !string.IsNullOrEmpty(insert.SpecificLocalizationKey.Key))
                {
                    insert.CompressedStringData = insert.SpecificLocalizationKey.Key;
                    str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                }

                break;
            case LocalizationFilterType.Variable_String:
                str2 = "(Variable String)";
                insert.CompressedStringData =
                    insert.VariableString?.Value;
                str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                break;
            case LocalizationFilterType.SpecificInvestigatorId:
                str2 = "(Investigator Id)";
                insert.CompressedIntData = insert.VariableInt?.Value ?? -1;
                str1 = MoM_LocalizationPacket.DecompressInsert(insert);
                break;
        }

        return str1;
    }

    private string ResolveInsertWithRandomItem(MoM_LocalizationPacket.PacketInsert insert)
    {
        var availableItems = _itemPool.Filter(insert.IncludeItemTraits, insert.ExcludeItemTraits, insert.ItemFilter);
        var spawnPriorities = DeterministicRandomFacade.SortElementsByRandomPriority(availableItems);
        if (spawnPriorities.Count >= 1)
        {
            var item = spawnPriorities[0];
            _spawnedItems.Add(item);
            _itemSpawnPriorities.Add(new ItemSpawnPriorities(spawnPriorities));
            _itemPool.Remove(item);
            insert.CompressedStringData = item.Name.Key;
            return MoM_LocalizationPacket.DecompressInsert(insert);
        }

        return null;
    }
}
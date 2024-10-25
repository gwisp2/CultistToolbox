using System;
using System.Collections.Generic;
using System.Linq;
using CultistToolbox.Patch;
using HarmonyLib;
using MonoMod.Utils;

namespace CultistToolbox;

public class MonsterDatabase
{
    private static readonly Lazy<MonsterDatabase> InstanceLazy = new(() => new MonsterDatabase());
    public static MonsterDatabase Instance => InstanceLazy.Value;

    private readonly Dictionary<int, MonsterModel> _monstersById;
    private readonly Dictionary<string, MonsterModel> _monstersByNameKey;
    private readonly Dictionary<string, MonsterModel> _monstersByName;
    private readonly ILookup<int, ProductModel> _productsContainingMonster;

    private MonsterDatabase()
    {
        var allMonsters = MoMDBManager.DB.GetProducts()
            .SelectMany(product => product.Monsters)
            .Select(item => item.monster)
            .Distinct()
            .ToList();
        _monstersById = allMonsters.ToDictionary(monster => monster.Id);
        _monstersByNameKey = allMonsters.ToDictionary(monster => monster.Name.Key);
        _monstersByName = new();
        _monstersByName.AddRange(allMonsters.ToDictionary(monster =>
            LocalizationPatch.OriginalGet(monster.Name.Key).ToLower()));
        _monstersByName.AddRange(allMonsters.ToDictionary(monster => LocalizationPatch.OriginalGet(monster.Name.Key)));
        _productsContainingMonster = MoMDBManager.DB.GetProducts()
            .SelectMany(product => product.Monsters.Select(monster => new { monster.monster, product }))
            .ToLookup(monster => monster.monster.Id, item => item.product);
    }

    public MonsterModel GetMonsterByNameKey(string nameKey)
    {
        return _monstersByNameKey.TryGetValue(nameKey, out var value) ? value : null;
    }

    public MonsterModel GetMonsterByName(string name)
    {
        if (_monstersByName.TryGetValue(name, out var value)) return value;
        if (_monstersByName.TryGetValue(name.ToLower(), out value)) return value;
        return null;
    }

    public MonsterModel GetPrimaryMonsterModel(MonsterModel model)
    {
        if (_monstersById.TryGetValue(model.Id, out var value)) return value;

        _monstersById[model.Id] = model;
        Plugin.Logger.LogWarning($"Unknown monster id: {model.Id} ({model.Name.Key}).");
        return model;
    }

    public IEnumerable<ProductModel> GetProducts(MonsterModel model)
    {
        return !_productsContainingMonster.Contains(model.Id) ? [] : _productsContainingMonster[model.Id];
    }

    public string GetProductIcons(MonsterModel model)
    {
        if (!_productsContainingMonster.Contains(model.Id))
        {
            return "";
        }

        return _productsContainingMonster[model.Id].Join(Utilities.GetProductIcons, "/");
    }
}
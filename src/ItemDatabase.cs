using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace MoMEssentials;

public class ItemDatabase
{
    private static readonly Lazy<ItemDatabase> InstanceLazy = new(() => new ItemDatabase());
    public static ItemDatabase Instance => InstanceLazy.Value;

    private readonly Dictionary<int, ItemModel> _itemsById;
    private readonly Dictionary<string, ItemModel> _itemsByNameKey;
    private readonly ILookup<int, ProductModel> _productsContainingItem;

    private ItemDatabase()
    {
        var allItems = MoMDBManager.DB.GetProducts()
            .SelectMany(product => product.Items)
            .Select(item => item.item)
            .Distinct()
            .ToList();
        _itemsById = allItems.ToDictionary(item => item.Id);
        _itemsByNameKey = allItems.ToDictionary(item => item.Name.Key);
        _productsContainingItem = MoMDBManager.DB.GetProducts()
            .SelectMany(product => product.Items.Select(item => new { item.item, product }))
            .ToLookup(item => item.item.Id, item => item.product);
    }

    public ItemModel GetItemByNameKey(string nameKey)
    {
        return _itemsByNameKey.TryGetValue(nameKey, out var value) ? value : null;
    }

    public ItemModel GetPrimaryItemModel(ItemModel model)
    {
        if (_itemsById.TryGetValue(model.Id, out var value)) return value;

        _itemsById[model.Id] = model;
        Plugin.Logger.LogWarning($"Unknown item id: {model.Id} ({model.Name.Key}).");
        return model;
    }

    public IEnumerable<ProductModel> GetProducts(ItemModel model)
    {
        return !_productsContainingItem.Contains(model.Id) ? [] : _productsContainingItem[model.Id];
    }

    public string GetProductIcons(ItemModel model)
    {
        if (!_productsContainingItem.Contains(model.Id))
        {
            return "";
        }

        return _productsContainingItem[model.Id].Join(Utilities.GetProductIcons, "/");
    }
}
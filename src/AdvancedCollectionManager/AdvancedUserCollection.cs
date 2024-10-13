using System;
using System.Collections.Generic;
using System.Linq;

namespace MoMEssentials.AdvancedCollectionManager;

public class AdvancedUserCollection
{
    [Flags]
    public enum ItemComponentTypes
    {
        Investigators = 1,
        Items = 2,
        Monsters = 4,
        MythosEvents = 8,
        Tiles = 16,
        All = 31
    }

    public class Item(ProductModel productModel)
    {
        private ItemComponentTypes _presentComponents = !productModel.CanToggle ? ItemComponentTypes.All : 0;
        public ProductModel ProductModel { get; } = productModel;
        public bool CanToggle => ProductModel.CanToggle;

        public bool HasInvestigators
        {
            get => AllPresent(ItemComponentTypes.Investigators);
            set => Set(ItemComponentTypes.Investigators, value);
        }

        public bool HasMonsters
        {
            get => AllPresent(ItemComponentTypes.Monsters);
            set => Set(ItemComponentTypes.Monsters, value);
        }

        public bool HasItems
        {
            get => AllPresent(ItemComponentTypes.Items);
            set => Set(ItemComponentTypes.Items, value);
        }

        public bool HasMythosEvents
        {
            get => AllPresent(ItemComponentTypes.MythosEvents);
            set => Set(ItemComponentTypes.MythosEvents, value);
        }

        public bool HasTiles
        {
            get => AllPresent(ItemComponentTypes.Tiles);
            set => Set(ItemComponentTypes.Tiles, value);
        }

        public bool AllPresent(ItemComponentTypes types)
        {
            return (_presentComponents & types) == types;
        }

        private ItemComponentTypes Set(ItemComponentTypes types, bool present)
        {
            if (!CanToggle)
                return 0;

            var oldValue = _presentComponents;
            if (present)
            {
                _presentComponents |= types;
            }
            else
            {
                _presentComponents &= ~types;
            }

            return oldValue ^ _presentComponents;
        }

        public void SetEverything(bool value)
        {
            Set(ItemComponentTypes.All, value);
        }

        public bool IsAnythingSelected => _presentComponents != 0;
        public bool IsEverythingSelected => _presentComponents == ItemComponentTypes.All;

        public string SaveToString()
        {
            return FormatBool(HasItems, "i") + FormatBool(HasMonsters, "m") + FormatBool(HasInvestigators, "I") +
                   FormatBool(HasMythosEvents, "M") + FormatBool(HasTiles, "t");
        }

        public ItemComponentTypes LoadFromString(string value)
        {
            if (!CanToggle) return 0;
            ItemComponentTypes target = 0;
            target |= value.Contains("i") ? ItemComponentTypes.Items : 0;
            target |= value.Contains("m") ? ItemComponentTypes.Monsters : 0;
            target |= value.Contains("I") ? ItemComponentTypes.Investigators : 0;
            target |= value.Contains("M") ? ItemComponentTypes.MythosEvents : 0;
            target |= value.Contains("t") ? ItemComponentTypes.Tiles : 0;
            var changeMask = _presentComponents & target;
            _presentComponents = target;
            return changeMask;
        }

        private string FormatBool(bool value, string letter) => value ? letter : "";
    }

    private List<Item> _items;
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    public AdvancedUserCollection()
    {
        _items = MoMDBManager.DB.GetProducts().OrderBy(p => p.ProductCode)
            .Select(p => new Item(p)).ToList();
        Reset();
    }

    public AdvancedUserCollection(string value) : this()
    {
        this.LoadFromString(value);
    }

    public void AddCompleteProduct(ProductModel product)
    {
        Get(product.ProductCode).SetEverything(true);
    }

    public void RemoveProduct(ProductModel product)
    {
        Get(product.ProductCode).SetEverything(false);
    }

    public Dictionary<ProductModel, int> GetCompleteProductQuantities()
    {
        return _items.ToDictionary(i => i.ProductModel, i => i.IsEverythingSelected ? 1 : 0);
    }

    public Item Get(string productCode)
    {
        return _items.FirstOrDefault(item => item.ProductModel.ProductCode == productCode);
    }

    public Item Get(ProductModel productModel)
    {
        return Get(productModel.ProductCode);
    }

    public bool HasCompleteProduct(ProductModel productModel)
    {
        return Get(productModel)?.IsEverythingSelected ?? false;
    }

    public bool HasAllProducts(IEnumerable<ProductModel> products, ItemComponentTypes componentTypes)
    {
        return products.All(p => Get(p)?.AllPresent(componentTypes) ?? false);
    }

    public string SaveToString()
    {
        var itemsInCollection = _items.Where(i => i.IsAnythingSelected).ToList();
        return string.Join(",",
            itemsInCollection.Select(item => item.ProductModel.ProductCode + ":" + item.SaveToString()));
    }

    public void Reset()
    {
        foreach (var item in _items)
        {
            if (!item.CanToggle) continue;
            item.SetEverything(!item.ProductModel.CanToggle);
        }
    }

    public ItemComponentTypes LoadFromString(string value)
    {
        ItemComponentTypes changeType = 0;
        var valueAsDict = new Dictionary<string, string>();
        foreach (var parts in value.Split([',']))
        {
            var kv = parts.Split([':'], 2);
            if (kv.Length != 2) continue;
            valueAsDict[kv[0]] = kv[1];
        }

        foreach (var item in _items)
        {
            if (!item.CanToggle) continue;
            if (valueAsDict.TryGetValue(item.ProductModel.ProductCode, out var v))
            {
                changeType |= item.LoadFromString(v);
            }
            else
            {
                changeType |= item.LoadFromString("");
            }
        }

        return changeType;
    }
}
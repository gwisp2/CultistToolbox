using System;
using System.Collections.Generic;
using System.Linq;

namespace MoMEssentials.AdvancedCollectionManager;

public class AdvancedUserCollection
{
    [Flags]
    public enum CollectionChangeType
    {
        Investigators = 1,
        Items = 2,
        Monsters = 4,
        OtherContent = 8,
    }

    public class Item(ProductModel productModel)
    {
        private bool _hasInvestigators = !productModel.CanToggle;
        private bool _hasMonsters = !productModel.CanToggle;
        private bool _hasItems = !productModel.CanToggle;
        private bool _hasOtherContent = !productModel.CanToggle;
        public ProductModel ProductModel { get; } = productModel;
        public bool CanToggle => ProductModel.CanToggle;

        public bool HasInvestigators
        {
            get => _hasInvestigators;
            set
            {
                if (!CanToggle && !value)
                    throw new InvalidOperationException("Cannot change value when CanToggle is false");
                _hasInvestigators = value;
            }
        }

        public bool HasMonsters
        {
            get => _hasMonsters;
            set
            {
                if (!CanToggle && !value)
                    throw new InvalidOperationException("Cannot change value when CanToggle is false");
                _hasMonsters = value;
            }
        }

        public bool HasItems
        {
            get => _hasItems;
            set
            {
                if (!CanToggle && !value)
                    throw new InvalidOperationException("Cannot change value when CanToggle is false");
                _hasItems = value;
            }
        }

        public bool HasOtherContent
        {
            get => _hasOtherContent;
            set
            {
                if (!CanToggle && !value)
                    throw new InvalidOperationException("Cannot change value when CanToggle is false");
                _hasOtherContent = value;
            }
        }

        public void SetEverything(bool value)
        {
            this.HasItems = value;
            this.HasMonsters = value;
            this.HasOtherContent = value;
            this.HasInvestigators = value;
        }

        public bool IsAnythingSelected => HasInvestigators || HasMonsters || HasItems || HasOtherContent;
        public bool IsEverythingSelected => HasInvestigators && HasMonsters && HasItems && HasOtherContent;

        public string SaveToString()
        {
            return FormatBool(HasItems, "i") + FormatBool(HasMonsters, "m") + FormatBool(HasInvestigators, "I") +
                   FormatBool(HasOtherContent, "o");
        }

        public CollectionChangeType LoadFromString(string value)
        {
            bool newHasItems = value.Contains("i");
            bool newHasMonsters = value.Contains("m");
            bool newHasInvestigators = value.Contains("I");
            bool newHasOtherContent = value.Contains("o");
            CollectionChangeType changeType = 0;
            if (HasItems != newHasItems)
            {
                changeType |= CollectionChangeType.Items;
                HasItems = newHasItems;
            }

            if (HasMonsters != newHasMonsters)
            {
                changeType |= CollectionChangeType.Monsters;
                HasMonsters = newHasMonsters;
            }

            if (HasInvestigators != newHasInvestigators)
            {
                changeType |= CollectionChangeType.Investigators;
                HasInvestigators = newHasInvestigators;
            }

            if (HasOtherContent != newHasOtherContent)
            {
                changeType |= CollectionChangeType.OtherContent;
                HasOtherContent = newHasOtherContent;
            }

            return changeType;
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

    public CollectionChangeType LoadFromString(string value)
    {
        CollectionChangeType changeType = 0;
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
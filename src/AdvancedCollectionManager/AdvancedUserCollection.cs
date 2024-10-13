using System.Collections.Generic;
using System.Linq;

namespace MoMEssentials;

public class AdvancedUserCollection
{
    public class Item(ProductModel productModel)
    {
        public ProductModel ProductModel { get; } = productModel;
        public bool HasInvestigators { get; set; }
        public bool HasMonsters { get; set; }
        public bool HasItems { get; set; }
        public bool HasOtherContent { get; set; }

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

        public void LoadFromString(string value)
        {
            HasItems = value.Contains("i");
            HasMonsters = value.Contains("m");
            HasInvestigators = value.Contains("I");
            HasOtherContent = value.Contains("o");
        }

        private string FormatBool(bool value, string letter) => value ? letter : "";
    }

    private List<Item> _items;
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    public AdvancedUserCollection()
    {
        _items = MoMDBManager.DB.GetProducts().OrderBy(p => p.ProductCode)
            .Select(p => new Item(p)).ToList();
        ForceIncludeBaseGame();
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

    public IEnumerable<ProductModel> GetIncludedCompleteProducts()
    {
        return _items.Where(item => item.IsEverythingSelected).Select(i => i.ProductModel);
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
            item.SetEverything(false);
        }

        ForceIncludeBaseGame();
    }

    public void LoadFromString(string value)
    {
        Reset();
        foreach (var parts in value.Split([',']))
        {
            var kv = parts.Split([':'], 2);
            if (kv.Length != 2) continue;
            var k = kv[0];
            var v = kv[1];
            var item = _items.FirstOrDefault(i => i.ProductModel.ProductCode == k);
            if (item == null) continue;
            item.LoadFromString(v);
        }

        ForceIncludeBaseGame();
    }

    private void ForceIncludeBaseGame()
    {
        Get("MAD20").SetEverything(true);
    }
}
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
            if (value.Contains("i")) HasItems = true;
            if (value.Contains("m")) HasMonsters = true;
            if (value.Contains("I")) HasInvestigators = true;
            if (value.Contains("o")) HasOtherContent = true;
        }

        private string FormatBool(bool value, string letter) => value ? letter : "";
    }

    private List<Item> _items;
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    public AdvancedUserCollection()
    {
        _items = MoMDBManager.DB.GetProducts().Where(p => p.ProductCode != "MAD20").OrderBy(p => p.ProductCode)
            .Select(p => new Item(p)).ToList();
    }

    public string SaveToString()
    {
        var itemsInCollection = _items.Where(i => i.IsAnythingSelected).ToList();
        return string.Join(",",
            itemsInCollection.Select(item => item.ProductModel.ProductCode + ":" + item.SaveToString()));
    }

    public void LoadFromString(string value)
    {
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
    }
}
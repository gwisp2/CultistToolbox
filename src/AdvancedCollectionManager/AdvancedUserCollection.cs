using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace MoMEssentials.AdvancedCollectionManager;

public class AdvancedUserCollection
{
    private static readonly TypeConverter TypeConverter = new()
    {
        ConvertToObject = (value, type) => new AdvancedUserCollection(value),
        ConvertToString = (value, type) => ((AdvancedUserCollection)value).SaveToString()
    };

    public static void RegisterTomlConverter()
    {
        TomlTypeConverter.AddConverter(typeof(AdvancedUserCollection), TypeConverter);
    }

    private List<AdvancedUserCollectionProduct> _items = new();
    public IReadOnlyList<AdvancedUserCollectionProduct> Items => _items.AsReadOnly();

    public AdvancedUserCollection()
    {
        Reset();
    }

    public AdvancedUserCollection(AdvancedUserCollection other)
    {
        _items = other._items.Select(i => new AdvancedUserCollectionProduct(i)).ToList();
    }

    public AdvancedUserCollection(string value) : this()
    {
        LoadFromString(value);
    }

    public AdvancedUserCollection AddCompleteProduct(ProductModel product)
    {
        Get(product.ProductCode).SetEverything(true);
        return this;
    }

    public AdvancedUserCollection AddEmptyProduct(ProductModel product)
    {
        GetOrCreate(product.ProductCode);
        return this;
    }


    public AdvancedUserCollection RemoveProduct(ProductModel product)
    {
        Get(product.ProductCode).SetEverything(false);
        return this;
    }

    public Dictionary<ProductModel, int> GetCompleteProductQuantities()
    {
        return _items.ToDictionary(i => i.ProductModel, i => i.IsEverythingSelected ? 1 : 0);
    }

    public AdvancedUserCollectionProduct Get(string productCode)
    {
        return _items.FirstOrDefault(item => item.ProductModel.ProductCode == productCode);
    }

    private AdvancedUserCollectionProduct GetOrCreate(string productCode)
    {
        var item = Get(productCode);
        if (item == null)
        {
            item = new AdvancedUserCollectionProduct(productCode);
            _items.Add(item);
        }

        return item;
    }

    public AdvancedUserCollectionProduct Get(ProductModel productModel)
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
            itemsInCollection.Select(item => item.ProductCode + ":" + item.SaveToString()));
    }

    public void Reset()
    {
        _items.Clear();
        _items.Add(new AdvancedUserCollectionProduct("MAD20", ItemComponentTypes.All));
    }

    public void LoadFromString(string value)
    {
        Reset();
        foreach (var parts in value.Split([',']))
        {
            var kv = parts.Split([':'], 2);
            if (kv.Length != 2) continue;
            GetOrCreate(kv[0]).LoadFromString(kv[1]);
        }
    }

    public AdvancedUserCollection Freeze()
    {
        foreach (var item in _items)
        {
            item.Freeze();
        }

        return this;
    }

    public AdvancedUserCollection Copy()
    {
        return new AdvancedUserCollection(this);
    }
}
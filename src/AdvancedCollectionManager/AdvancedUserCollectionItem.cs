namespace MoMEssentials.AdvancedCollectionManager;

public class AdvancedUserCollectionItem(string productCode, ItemComponentTypes componentTypes = ItemComponentTypes.None)
{
    public string ProductCode { get; } = productCode;
    public bool IsFrozen { get; private set; }

    private ItemComponentTypes _presentComponents = componentTypes;

    public AdvancedUserCollectionItem(AdvancedUserCollectionItem other) : this(other.ProductCode)
    {
        _presentComponents = other._presentComponents;
    }

    public ProductModel ProductModel => MoMDBManager.DB.GetProductByCode(ProductCode);

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

    public void Set(ItemComponentTypes types, bool present)
    {
        if (IsFrozen) return;

        if (present)
        {
            _presentComponents |= types;
        }
        else
        {
            _presentComponents &= ~types;
        }
    }

    public void SetEverything(bool value)
    {
        Set(ItemComponentTypes.All, value);
    }

    public bool IsAnythingSelected => _presentComponents != 0;
    public bool IsEverythingSelected => _presentComponents == ItemComponentTypes.All;

    public string SaveToString()
    {
        return _presentComponents.ToShortString();
    }

    public ItemComponentTypes LoadFromString(string value)
    {
        if (!IsFrozen) return 0;
        ItemComponentTypes target = ItemComponentTypesExtensions.FromShortString(value);
        var changeMask = _presentComponents & target;
        _presentComponents = target;
        return changeMask;
    }

    public void Freeze()
    {
        IsFrozen = true;
    }
}
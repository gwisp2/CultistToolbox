using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace CultistToolbox.AdvancedCollectionManager;

[Flags]
public enum ItemComponentTypes
{
    None = 0,
    Investigators = 1,
    Items = 2,
    Monsters = 4,
    MythosEvents = 8,
    Tiles = 16,
    IsShared = 32,
    All = 31
}

public static class ItemComponentTypesExtensions
{
    private static readonly TypeConverter TomlConverter = new TypeConverter()
    {
        ConvertToObject = (value, type) => FromShortString(value),
        ConvertToString = (value, type) => ((ItemComponentTypes)value).ToShortString()
    };

    private static readonly Dictionary<ItemComponentTypes, char> EnumToCharMap = new()
    {
        { ItemComponentTypes.Items, 'i' },
        { ItemComponentTypes.Investigators, 'I' },
        { ItemComponentTypes.Monsters, 'm' },
        { ItemComponentTypes.MythosEvents, 'M' },
        { ItemComponentTypes.Tiles, 't' },
        { ItemComponentTypes.IsShared, '/' },
    };

    private static readonly Dictionary<char, ItemComponentTypes> CharToEnumMap =
        EnumToCharMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static void RegisterTomlConverter()
    {
        TomlTypeConverter.AddConverter(typeof(ItemComponentTypes), TomlConverter);
    }

    public static string ToShortString(this ItemComponentTypes value)
    {
        var result = new List<char>();
        foreach (var flag in EnumToCharMap.Keys)
        {
            if (value.HasFlag(flag))
            {
                result.Add(EnumToCharMap[flag]);
            }
        }

        return new string(result.ToArray());
    }

    public static ItemComponentTypes FromShortString(string value)
    {
        if (value == null) return ItemComponentTypes.None;
        ItemComponentTypes result = 0;
        foreach (var c in value)
        {
            if (CharToEnumMap.TryGetValue(c, out var flag))
            {
                result |= flag;
            }
        }

        return result;
    }
}
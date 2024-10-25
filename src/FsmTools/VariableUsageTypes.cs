using System;

namespace CultistToolbox.FsmTools;

[Flags]
public enum VariableUsageTypes
{
    None = 0,
    Read = 1,
    Write = 2,
    Display = 4,
    Unknown = 8,
}

public static class VariableUsageTypesExtensions
{
    private const string Letters = "RWDU";

    public static string ToFlagString(this VariableUsageTypes value)
    {
        var result = new char[Letters.Length];
        for (int i = 0; i < Letters.Length; i++)
        {
            result[i] = value.HasFlag((VariableUsageTypes)(1 << i)) ? Letters[i] : '-';
        }

        return new string(result);
    }
}
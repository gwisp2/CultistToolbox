using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace MoMEssentials.AdvancedCollectionManager;

public class AcceptableValueFlags<T> : AcceptableValueBase
    where T : Enum
{
    public AcceptableValueFlags(T allowedFlags) : base(typeof(T))
    {
        if (!typeof(T).GetTypeInfo().IsDefined(typeof(FlagsAttribute), false))
            throw new ArgumentException("Type must be a flags enum", nameof(allowedFlags));
        _allowedFlags = allowedFlags;
    }

    private readonly T _allowedFlags;

    public override object Clamp(object value)
    {
        var v = (T)value;
        var allowed = _allowedFlags;
        var vInt = Convert.ToInt64(v);
        var allowedInt = Convert.ToInt64(allowed);
        return (T)Enum.ToObject(typeof(T), vInt & allowedInt);
    }

    public override bool IsValid(object value)
    {
        var v = (T)value;
        var allowed = _allowedFlags;
        var vInt = Convert.ToInt64(v);
        var allowedInt = Convert.ToInt64(allowed);
        return (vInt & ~allowedInt) == 0;
    }

    public override string ToDescriptionString()
    {
        var allowedFlags = Enum.GetValues(typeof(T)).Cast<T>().Where(x => _allowedFlags.HasFlag(x));
        var allowedFlagNames = string.Join(", ", allowedFlags.Select(x => Enum.GetName(typeof(T), x)));
        return "# Acceptable values: " + allowedFlagNames;
    }
}
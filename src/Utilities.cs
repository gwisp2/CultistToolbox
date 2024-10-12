using System;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;

namespace MoMEssentials;

public static class Utilities
{
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.RandomRangeInt(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static IEnumerable<MoM_LocalizationPacket.PacketInsert> GetUsedInserts(
        this MoM_LocalizationPacket packet
    )
    {
        int nArgs = packet.CalculateArguments();
        return packet.Inserts.Where((_, index) => index < nArgs);
    }

    public static IEnumerable<MoM_LocalizationPacket.PacketInsert> GetUsedInserts(
        this MoM_LocalizationPacket packet,
        LocalizationFilterType type
    )
    {
        return packet.GetUsedInserts().Where(insert => insert.Filter == type);
    }

    public static string GetProductIcons(ProductModel model)
    {
        return Localization.Get("ICON_PRODUCT_" + model.ProductCode) ?? model.ProductCode;
    }
}
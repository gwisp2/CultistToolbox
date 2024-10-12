using System;
using HarmonyLib;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(Localization), nameof(Localization.Get), [typeof(string), typeof(bool)])]
public class LocalizationPatch
{
    [HarmonyReversePatch]
    public static string OriginalGet(string key, bool warnIfMissing = true)
    {
        throw new NotImplementedException("stub");
    }

    [HarmonyPostfix]
    public static void Postfix(string key, ref string __result)
    {
        if (!Plugin.ConfigShowExpansionIcon.Value) return;
        var item = ItemDatabase.Instance.GetItemByNameKey(key);
        if (item != null)
        {
            var productIcons = ItemDatabase.Instance.GetProductIcons(item);
            __result = AddProductIcon(__result, productIcons);
            return;
        }

        var monster = MonsterDatabase.Instance.GetMonsterByNameKey(key);
        if (monster != null)
        {
            var productIcons = MonsterDatabase.Instance.GetProductIcons(monster);
            __result = AddProductIcon(__result, productIcons);
            return;
        }
    }

    private static string AddProductIcon(string objectName, string productIcon)
    {
        if (!objectName.Contains(productIcon))
        {
            return objectName + " " + productIcon;
        }

        return objectName;
    }
}
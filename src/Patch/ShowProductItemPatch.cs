using System;
using CultistToolbox.UI;
using HarmonyLib;

namespace CultistToolbox.Patch;

/**
 * This class adds product icons to localizations of item and monster names.
 * It is only enabled if Plugin.ConfigShowExpansionIcon is enabled.
 */
[HarmonyPatch]
public class ShowProductItemPatch
{
    [HarmonyPatch(typeof(StartingItem), nameof(StartingItem.Initialize))]
    [HarmonyPostfix]
    public static void PostStartingItemInitialize(StartingItem __instance)
    {
        // Fix font when displaying starting items. Default font can't display product icons.
        __instance.LabelTitle.trueTypeFont = IconFontLocator.IconFont;
    }

    [HarmonyPatch(typeof(Localization), nameof(Localization.Get), [typeof(string), typeof(bool)])]
    [HarmonyReversePatch]
    public static string OriginalGet(string key, bool warnIfMissing = true)
    {
        throw new NotImplementedException("stub");
    }

    [HarmonyPatch(typeof(Localization), nameof(Localization.Get), [typeof(string), typeof(bool)])]
    [HarmonyPostfix]
    public static void PostLocalizationGet(string key, ref string __result)
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
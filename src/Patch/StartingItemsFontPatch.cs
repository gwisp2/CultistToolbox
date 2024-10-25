using CultistToolbox.UI;
using HarmonyLib;

namespace CultistToolbox.Patch;

[HarmonyPatch(typeof(StartingItem), nameof(StartingItem.Initialize))]
public class StartingItemsFontPatch
{
    [HarmonyPostfix]
    public static void Postfix(StartingItem __instance)
    {
        __instance.LabelTitle.trueTypeFont = IconFontLocator.IconFont;
    }
}
using HarmonyLib;
using MoMEssentials.UI;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(StartingItem), nameof(StartingItem.Initialize))]
public class StartingItemsFontPatch
{
    [HarmonyPostfix]
    public static void Postfix(StartingItem __instance)
    {
        __instance.LabelTitle.trueTypeFont = IconFontLocator.IconFont;
    }
}
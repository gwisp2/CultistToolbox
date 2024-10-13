using FFG.MoM;
using HarmonyLib;
using MoMEssentials.AdvancedCollectionManager;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(MythosEventDeckManager))]
public class MythosEventDeckManagerPatch
{
    [HarmonyPatch("HasProductRequirements")]
    [HarmonyPrefix]
    public static bool PreHasProductRequirements(MythosEventModel model, ref bool __result)
    {
        if (model.RequiredProducts == null || model.RequiredProducts.Length == 0)
        {
            __result = true;
            return false;
        }

        var effectiveCollection = AdvancedCollectionFacade.GetEffectiveCollectionForCurrentScenario();
        __result = model.RequiredProducts == null || effectiveCollection.HasAllProducts(model.RequiredProducts,
            ItemComponentTypes.MythosEvents);
        return false;
    }
}
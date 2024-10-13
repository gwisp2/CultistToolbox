using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MoMEssentials.AdvancedCollectionManager;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(MoMDatabase))]
public class AvailableInvestigatorsPatch
{
    [HarmonyPatch("GetAvailableInvestigators")]
    [HarmonyPrefix]
    public static bool PreGetAvailableInvestigators(MoMDatabase __instance, ref IEnumerable<InvestigatorModel> __result)
    {
        HashSet<InvestigatorModel> source = new HashSet<InvestigatorModel>();
        var products = AdvancedCollectionFacade.GetCurrentAdvancedUserCollection().Items
            .Where(i => i.HasInvestigators)
            .Select(i => i.ProductModel);
        foreach (var product in products)
        {
            source.UnionWith(product.Investigators);
        }

        __result = source.OrderBy(i => Localization.Get(i.Name.Key));
        return false;
    }
}
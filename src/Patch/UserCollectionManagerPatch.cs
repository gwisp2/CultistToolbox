using System;
using System.Collections.Generic;
using System.Linq;
using FFG.Common;
using FFG.MoM;
using HarmonyLib;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(UserCollectionManager))]
public class UserCollectionManagerPatch
{
    private static AccessTools.FieldRef<CollectionProductMenu, ProductModel> _curModelRef =
        AccessTools.FieldRefAccess<CollectionProductMenu, ProductModel>("_curModel");

    private static AccessTools.FieldRef<ScenarioSelectionController, Scenario> _curScenarioRef =
        AccessTools.FieldRefAccess<ScenarioSelectionController, Scenario>("_curScenario");

    private static readonly AccessTools.FieldRef<InvestigatorSelectionManager, IEnumerable<InvestigatorModel>>
        AvailableInvestigatorsRef =
            AccessTools.FieldRefAccess<InvestigatorSelectionManager, IEnumerable<InvestigatorModel>>(
                "_availableInvestigators");

    [HarmonyPatch("LoadUserCollection")]
    [HarmonyPrefix]
    private static bool PreLoadUserCollection()
    {
        // Initialize from plugin config
        var collection = Plugin.ConfigCollection.Value.Copy();

        // Apply original game settings where possible
        foreach (ProductModel product in MoMDBManager.DB.GetProducts())
        {
            if (product == null) continue;
            collection.AddEmptyProduct(product); // add it to collection for further edit
            bool available = FFGPlayerPrefs.GetInt(product.ProductCode) > 0;
            if (available)
            {
                collection.AddCompleteProduct(product);
            }
            else if (collection.Get(product).IsEverythingSelected)
            {
                collection.RemoveProduct(product);
            }
        }

        // Save applied changes
        Plugin.ConfigCollection.Value = collection.Freeze();

        return false;
    }

    [HarmonyPatch("GetProductCollection")]
    [HarmonyPrefix]
    private static bool PreGetProductCollection(ref Dictionary<ProductModel, int> __result)
    {
        __result = Plugin.ConfigCollection.Value.GetCompleteProductQuantities();
        return false;
    }

    [HarmonyPatch(typeof(UserCollectionManager), "AddProduct", [typeof(ProductModel), typeof(int)])]
    [HarmonyPrefix]
    private static bool PreAddProduct(ProductModel p, int quantity)
    {
        if (!p.CanToggle)
            return false; // AddProduct is called once before LoadUserCollection for the base game, ignore that
        if (quantity >= 1)
            Plugin.ConfigCollection.Value = Plugin.ConfigCollection.Value.Copy().AddCompleteProduct(p).Freeze();
        return false;
    }

    [HarmonyPatch("SetProductQuantity")]
    [HarmonyPrefix]
    private static bool PreSetProductQuantity(ProductModel product, int quantity)
    {
        Plugin.ConfigCollection.Value = quantity >= 1
            ? Plugin.ConfigCollection.Value.Copy().AddCompleteProduct(product).Freeze()
            : Plugin.ConfigCollection.Value.Copy().RemoveProduct(product).Freeze();
        return false;
    }

    [HarmonyPatch("RemoveProduct")]
    [HarmonyPrefix]
    private static bool PreRemoveProduct(ProductModel product)
    {
        Plugin.ConfigCollection.Value = Plugin.ConfigCollection.Value.Copy().RemoveProduct(product).Freeze();
        return false;
    }

    [HarmonyPatch("IsOwned")]
    [HarmonyPrefix]
    public static bool IsOwned(ProductModel product, bool checkEquivalentProduct, ref bool __result)
    {
        bool flag = Plugin.ConfigCollection.Value.HasCompleteProduct(product);
        if (!flag & checkEquivalentProduct && product.HasEquivalentProductCodes)
        {
            foreach (string equivalentProductCode in product.EquivalentProductCodes)
            {
                ProductModel productByCode = MoMDBManager.DB.GetProductByCode(equivalentProductCode);
                if (productByCode != null && Plugin.ConfigCollection.Value.HasCompleteProduct(productByCode))
                    return true;
            }
        }

        __result = flag;
        return false;
    }

    [HarmonyPatch("Quantity")]
    [HarmonyPrefix]
    public static bool PreQuantity(ProductModel p, ref int __result)
    {
        __result = Plugin.ConfigCollection.Value.HasCompleteProduct(p) ? 1 : 0;
        return false;
    }

    public static void Setup()
    {
        Plugin.ConfigCollection.SettingChanged += (object _, EventArgs _) => HandleCollectionChange();
    }

    private static void HandleCollectionChange()
    {
        // Fix in UI
        foreach (var collectionProduct in UI.Utilities.FindComponents<CollectionProduct>(false))
        {
            collectionProduct.UpdateVisuals();
        }

        foreach (var collectionProductMenu in UI.Utilities.FindComponents<CollectionProductMenu>(false))
        {
            var product = _curModelRef(collectionProductMenu);
            if (product == null) continue;
            collectionProductMenu.UpdateVisuals(product.Owned);
        }

        foreach (var scenarioSelectionController in UI.Utilities.FindComponents<ScenarioSelectionController>(false))
        {
            // Update scenario availability
            var scenario = _curScenarioRef(scenarioSelectionController);
            if (scenario == null) continue;
            scenarioSelectionController.LoadScenario(scenario);
        }

        foreach (var setupViewController in UI.Utilities.FindComponents<SetupViewController>(false))
        {
            var investigatorSelectionManager = setupViewController.PanelInvestigatorSelect;
            if (investigatorSelectionManager == null) continue;
            var oldInvestigators = AvailableInvestigatorsRef(investigatorSelectionManager);
            var newInvestigators = MoMDBManager.DB.GetAvailableInvestigators();
            if (!oldInvestigators.SequenceEqual(newInvestigators))
            {
                setupViewController.LoadInvestigators();
            }
        }
    }
}
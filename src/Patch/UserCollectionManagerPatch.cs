using System;
using System.Collections.Generic;
using FFG.Common;
using FFG.MoM;
using HarmonyLib;
using MoMEssentials.AdvancedCollectionManager;

namespace MoMEssentials.Patch;

[HarmonyPatch(typeof(UserCollectionManager))]
public class UserCollectionManagerPatch
{
    // Synchronized with Plugin.ConfigCollection.Value
    private static AdvancedUserCollection _collection;

    private static AccessTools.FieldRef<CollectionProductMenu, ProductModel> _curModelRef =
        AccessTools.FieldRefAccess<CollectionProductMenu, ProductModel>("_curModel");

    private static AccessTools.FieldRef<ScenarioSelectionController, Scenario> _curScenarioRef =
        AccessTools.FieldRefAccess<ScenarioSelectionController, Scenario>("_curScenario");

    [HarmonyPatch("LoadUserCollection")]
    [HarmonyPrefix]
    private static bool PreLoadUserCollection()
    {
        // Initialize from plugin config
        _collection ??= new AdvancedUserCollection();
        _collection.LoadFromString(Plugin.ConfigCollection.Value);

        // Apply original game settings where possible
        foreach (ProductModel product in MoMDBManager.DB.GetProducts())
        {
            if (product == null) continue;
            bool available = FFGPlayerPrefs.GetInt(product.ProductCode) > 0;
            if (available)
            {
                _collection.AddCompleteProduct(product);
            }
            else if (_collection.Get(product).IsEverythingSelected)
            {
                _collection.RemoveProduct(product);
            }
        }

        // Save applied changes
        SaveToPluginConfig();

        return false;
    }

    [HarmonyPatch("GetProductCollection")]
    [HarmonyPrefix]
    private static bool PreGetProductCollection(ref Dictionary<ProductModel, int> __result)
    {
        __result = _collection.GetCompleteProductQuantities();
        return false;
    }

    [HarmonyPatch(typeof(UserCollectionManager), "AddProduct", [typeof(ProductModel), typeof(int)])]
    [HarmonyPrefix]
    private static bool PreAddProduct(ProductModel p, int quantity)
    {
        if (_collection == null) return false; // AddProduct is called once before LoadUserCollection, ignore that
        if (quantity >= 1) _collection.AddCompleteProduct(p);
        SaveToPluginConfig();
        return false;
    }

    [HarmonyPatch("SetProductQuantity")]
    [HarmonyPrefix]
    private static bool PreSetProductQuantity(ProductModel product, int quantity)
    {
        if (quantity >= 1) _collection.AddCompleteProduct(product);
        else _collection.RemoveProduct(product);
        SaveToPluginConfig();
        return false;
    }

    [HarmonyPatch("RemoveProduct")]
    [HarmonyPrefix]
    private static bool PreRemoveProduct(ProductModel product)
    {
        _collection.RemoveProduct(product);
        SaveToPluginConfig();
        return false;
    }

    [HarmonyPatch("IsOwned")]
    [HarmonyPrefix]
    public static bool IsOwned(ProductModel product, bool checkEquivalentProduct, ref bool __result)
    {
        bool flag = _collection.HasCompleteProduct(product);
        if (!flag & checkEquivalentProduct && product.HasEquivalentProductCodes)
        {
            foreach (string equivalentProductCode in product.EquivalentProductCodes)
            {
                ProductModel productByCode = MoMDBManager.DB.GetProductByCode(equivalentProductCode);
                if (productByCode != null && _collection.HasCompleteProduct(productByCode))
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
        __result = _collection.HasCompleteProduct(p) ? 1 : 0;
        return false;
    }

    public static void Setup()
    {
        Plugin.ConfigCollection.SettingChanged += (object _, EventArgs _) => UpdateFromPluginConfig();
    }

    private static void SaveToPluginConfig()
    {
        Plugin.ConfigCollection.Value = _collection.SaveToString();
    }

    private static void UpdateFromPluginConfig()
    {
        // Load _collection from config
        _collection.LoadFromString(Plugin.ConfigCollection.Value);

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
    }
}
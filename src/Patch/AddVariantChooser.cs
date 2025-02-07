using System;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using HarmonyLib;
using UnityEngine;

namespace CultistToolbox.Patch;

/**
 * Adds ability to select a variant for a scenario instead of choosing a random one
 */
[HarmonyPatch(typeof(ScenarioSelectionController))]
public class AddVariantChooser
{
    private static readonly Lazy<ScenarioVariant> RandomScenarioVariant = new(() =>
    {
        ScenarioVariant scenarioVariant = ScriptableObject.CreateInstance<ScenarioVariant>();
        scenarioVariant.name = "Random Variant";
        return scenarioVariant;
    });

    [HarmonyPatch(nameof(ScenarioSelectionController.LoadScenario))]
    [HarmonyPostfix]
    private static void PostLoadScenario(ScenarioSelectionController __instance, Scenario scenario)
    {
        // Fix the variant selection font
        __instance.VariantSelection.trueTypeFont = __instance.ExpansionIcon.trueTypeFont;
        // Show variant selection box
        __instance.VariantParent.gameObject.SetActive(true);
        __instance.VariantSelection.gameObject.SetActive(true);
        var variantChooserTitle = __instance.VariantParent.GetComponentsInChildren<UILabel>()
            .FirstOrDefault(label => label.text.Contains("Debug"));
        if (variantChooserTitle != null)
        {
            variantChooserTitle.text = "Variant";
        }

        PopulateVariants(__instance, scenario);
        __instance.ViewController.SelectedVariant = null;
        __instance.SelectedVariantLabel.text = "Random Variant";
    }

    private static void PopulateVariants(ScenarioSelectionController ctrl, Scenario scenario)
    {
        List<object> objectList = new List<object>();
        List<string> stringList = new List<string>();
        objectList.Add(RandomScenarioVariant.Value);
        stringList.Add(RandomScenarioVariant.Value.name);
        foreach (ScenarioVariant variant in ctrl.SelectedScenario.Variants)
        {
            if (variant != null &&
                UserCollectionManager.IsAllOwned(variant.RequiredAdditionalProducts, true))
            {
                objectList.Add(variant);
                stringList.Add(variant.name + " (" + Utilities.GetProductIconsForScenarioVariant(variant) + ")");
            }
        }

        ctrl.VariantSelection.itemData = objectList;
        ctrl.VariantSelection.items = stringList;
    }
}
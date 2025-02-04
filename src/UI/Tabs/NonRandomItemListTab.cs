using System;
using System.Collections.Generic;
using System.Linq;
using CultistToolbox.FsmTools;
using CultistToolbox.Patch;
using FFG.MoM;
using HutongGames.PlayMaker;
using UnityEngine;

namespace CultistToolbox.UI.Tabs;

public class NonRandomItemListTab() : ToolboxTab("Items")
{
    private Vector2 _scrollPosition;
    private List<ItemModel> _possibleNonRandomItems = new();

    public override void OnScenarioLoaded()
    {
        // Get potential random items
        _possibleNonRandomItems = Utilities.EnumerateAllActions<IFsmStateAction>()
            .SelectMany(ItemMentionParameters.FromAction)
            .Where(p => p.Item != null)
            .Select(p => p.Item)
            .Distinct()
            .ToList();
    }

    public override void OnScenarioShutdown()
    {
        if (!HookScenarioLoadUnload.ScenarioLoaded) return;
    }

    public override void Render()
    {
        if (!GameData.IsInitialized) return;
        GUILayout.Label("Non-random items:");
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        DrawItemGroup(_possibleNonRandomItems);
        GUILayout.EndScrollView();
    }

    private void DrawItemGroup(List<ItemModel> items)
    {
        items.Sort((a, b) => String.Compare(FormatItem(a), FormatItem(b), StringComparison.Ordinal));
        foreach (var itemModel in items)
        {
            var isItemAcquired = MoM_ItemManager.IsItemInPlay(itemModel);
            var texture = itemModel.Image?.Asset?.texture;
            GUILayout.BeginHorizontal();
            if (texture)
            {
                Common.DrawTexture(texture, 100);
            }

            GUILayout.Label(FormatItem(itemModel),
                isItemAcquired ? Common.HighlightLabelStyle.Value : Common.LabelStyle.Value);
            GUILayout.EndHorizontal();
        }
    }

    private static string FormatItem(ItemModel model)
    {
        var itemTypePrefix = model.Type switch
        {
            ItemType.Unique => "[U]",
            ItemType.Spell => "[S]",
            ItemType.Common => "[C]",
            _ => "[-] "
        };
        var name = Localization.Get(model.Name.Key) ?? model.Name.Key;
        var text = itemTypePrefix + " " + name;
        return text;
    }
}
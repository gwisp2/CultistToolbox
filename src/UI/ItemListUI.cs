using System;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using HutongGames.PlayMaker;
using MoMEssentials.DeterministicRandom;
using UnityEngine;

namespace MoMEssentials.UI;

public class ItemListUI
{
    private Vector2 _scrollPosition;
    private List<ItemModel> _potentialItems = new();
    private readonly WindowController _window;
    private readonly TooltipWindow _tooltipWindow;

    public ItemListUI()
    {
        _window =
            new WindowController(Windows.ItemsWindowId, "Items", DrawWindowContent, Windows.ItemsWindowRect);
        _tooltipWindow = new TooltipWindow(Windows.ItemTooltipWindowId);
    }

    public void Update()
    {
        // Get items that can't be given by random
        Scenario scenario = GameData.Scenario;
        List<IEnumerable<ItemModel>> itemGroupsToExclude =
        [
            scenario.BlacklistedItems, scenario.ReservedItems, scenario.InitialItemsAlreadyInScene,
            scenario.InitialItemsToSpawn, scenario.GeneratedStartingItems
        ];
        var itemsExcludedFromRandom = new HashSet<ItemModel>(
            itemGroupsToExclude
                .SelectMany(group => (group ?? []))
                .Select(i => ItemDatabase.Instance.GetPrimaryItemModel(i))
        );

        // Get potential random items
        var priorityLists = Utilities.EnumerateAllActions<IFsmStateAction>()
            .SelectMany(Predictor.PredictSpawnedItems)
            .ToList();
        _potentialItems = FindMaxUsedItemSet(priorityLists, itemsExcludedFromRandom).ToList();
        _potentialItems.AddRange(scenario.ReservedItems ?? []);
        _potentialItems.AddRange(scenario.GeneratedStartingItems ?? []);

        // Gather all items
        MoMEssentials.Utilities.Shuffle(_potentialItems);
    }

    public void OnGUI()
    {
        if (_potentialItems.Any())
        {
            _window.RenderWindow();
        }
    }

    public void OnGUISecondPass()
    {
        _tooltipWindow.OnGUI();
    }

    private void DrawWindowContent()
    {
        _tooltipWindow.Hide();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        var items = _potentialItems;
        var commonItems = items.Where(p => p.Type == ItemType.Common).ToList();
        var uniqueItems = items.Where(p => p.Type == ItemType.Unique).ToList();
        var spellItems = items.Where(p => p.Type == ItemType.Spell).ToList();
        DrawItemGroup(commonItems);
        DrawItemGroup(uniqueItems);
        DrawItemGroup(spellItems);
        GUILayout.EndScrollView();
    }

    private void DrawItemGroup(List<ItemModel> items)
    {
        items.Sort((a, b) => String.Compare(FormatItem(a), FormatItem(b), StringComparison.Ordinal));
        foreach (var itemModel in items)
        {
            var isItemAcquired = MoM_ItemManager.IsItemInPlay(itemModel);
            GUILayout.Label(FormatItem(itemModel),
                isItemAcquired ? Common.HighlightLabelStyle.Value : Common.LabelStyle.Value);
            var lastRect = GUILayoutUtility.GetLastRect();
            var texture = itemModel.Image?.Asset?.texture;
            if (lastRect.Contains(_window.GetRelativeMousePosition() + _scrollPosition) && texture)
            {
                _tooltipWindow.SetTooltip(FormatItem(itemModel), texture, 300, Utilities.GetMousePosition());
            }
        }
    }

    private static string FormatItem(ItemModel model)
    {
        var prefixes = model.Type switch
        {
            ItemType.Unique => "[U]",
            ItemType.Spell => "[S]",
            ItemType.Common => "[C]",
            _ => "[-] "
        };
        var name = Localization.Get(model.Name.Key) ?? model.Name.Key;
        var text = prefixes + " " + name;
        return text;
    }

    private List<ItemModel> FindMaxUsedItemSet(List<ItemSpawnPriorities> placesP, HashSet<ItemModel> restrictedItems)
    {
        var places = placesP.Select(pr => pr.Items.Except(restrictedItems).ToList()).Where(l => l.Any()).ToList();
        int[] maxTaken = new int[places.Count()];
        bool[,] hasCommonItems = new bool[places.Count(), places.Count()];

        void IncrementMaxTaken(int placeIndex)
        {
            maxTaken[placeIndex]++;
            var newItem = places[placeIndex][maxTaken[placeIndex] - 1];
            for (int i = 0; i < places.Count(); i++)
            {
                if (i == placeIndex) continue;
                for (int j = 0; j <= maxTaken[i]; j++)
                {
                    if (places[i][j] == newItem)
                    {
                        hasCommonItems[i, placeIndex] = true;
                        hasCommonItems[placeIndex, i] = true;
                    }
                }
            }
        }

        bool IsEnough(int placeIndex)
        {
            int nOtherPlacesWithCommonItems = 0;
            for (int i = 0; i < places.Count(); i++)
                if (hasCommonItems[i, placeIndex])
                    nOtherPlacesWithCommonItems++;
            return nOtherPlacesWithCommonItems < maxTaken[placeIndex];
        }

        bool hadChanges;
        do
        {
            hadChanges = false;
            for (int i = 0; i < places.Count(); i++)
            {
                if (!IsEnough(i) && maxTaken[i] + 1 < places[i].Count)
                {
                    IncrementMaxTaken(i);
                    hadChanges = true;
                }
            }
        } while (hadChanges);

        var result = new List<ItemModel>();
        for (int i = 0; i < places.Count(); i++)
        for (int j = 0; j <= maxTaken[i]; j++)
            result.Add(places[i][j]);

        return result.Distinct().ToList();
    }
}
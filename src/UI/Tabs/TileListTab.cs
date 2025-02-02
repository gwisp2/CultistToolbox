using System;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using UnityEngine;

namespace CultistToolbox.UI.Tabs;

public class TileListTab() : ToolboxTab("Tiles")
{
    private class TileInfo
    {
        public readonly string NameKey;
        public readonly string Name;
        public readonly string Size;
        public string SizeWithName => $"[{Size}] {Name}";
        public readonly MoM_MapTile MapTile;
        public readonly Texture Texture;
        public readonly float MinSideSize;

        public TileInfo(MoM_MapTile mapTile)
        {
            MapTile = mapTile;

            var renderer = mapTile.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Texture = renderer.sharedMaterial?.mainTexture;
                MinSideSize = Math.Min(renderer.bounds.extents.x, renderer.bounds.extents.y);
                float maxSideSize = Math.Max(renderer.bounds.extents.x, renderer.bounds.extents.y);
                float sizeRatio = maxSideSize / MinSideSize;
                Size = Mathf.Approximately(sizeRatio, 2) ? "S" : Mathf.Approximately(sizeRatio, 1.5f) ? "L" : "M";
            }

            List<string> keyCandidates = [Texture?.name?.ToUpper(), MapTile.Name, mapTile.Model?.Name?.Key];
            NameKey = keyCandidates.FirstOrDefault(key => key != null && Localization.Get(key) != null);
            Name = NameKey != null ? Localization.Get(NameKey) : "[unknown]";
        }

        protected bool Equals(TileInfo other)
        {
            return NameKey == other.NameKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TileInfo)obj);
        }

        public override int GetHashCode()
        {
            return (NameKey != null ? NameKey.GetHashCode() : 0);
        }
    }

    private List<TileInfo> _tiles = new();
    private Vector2 _scrollPosition;

    public override void OnScenarioLoaded()
    {
        _tiles = Utilities.FindComponents<MoM_MapTile>()
            .Select(mapTile => new TileInfo(mapTile))
            .Distinct()
            .OrderBy(tile => tile.SizeWithName)
            .ToList();
    }

    public override void Render()
    {
        if (!GameData.IsInitialized) return;
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        foreach (var tile in _tiles)
        {
            GUILayout.BeginHorizontal();
            Common.DrawTexture(tile.Texture, tile.MinSideSize * 100);
            GUILayout.Label(tile.SizeWithName, Common.HighlightOnHoverLabelStyle.Value);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using UnityEngine;

namespace MoMEssentials.UI;

public class TileListUI : Renderable
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
    private readonly WindowController _window;
    private readonly TooltipWindow _tooltipWindow;
    private Vector2 _scrollPosition;

    public TileListUI()
    {
        _window = new(Windows.TilesWindowId, "Tiles", DrawWindowContent, Windows.TilesWindowRect);
        _tooltipWindow = new TooltipWindow(Windows.TileTooltipWindowId);
    }

    public void Update()
    {
        _tiles = Utilities.FindComponents<MoM_MapTile>()
            .Select(mapTile => new TileInfo(mapTile))
            .Distinct()
            .OrderBy(tile => tile.SizeWithName)
            .ToList();
    }

    public override void RenderFirstPass()
    {
        _window.RenderWindow();
    }

    public override void RenderSecondPass()
    {
        _tooltipWindow.OnGUI();
    }

    private void DrawWindowContent()
    {
        if (!GameData.IsInitialized) return;
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        _tooltipWindow.Hide();
        foreach (var tile in _tiles)
        {
            GUILayout.Label(tile.SizeWithName, Common.HighlightOnHoverLabelStyle.Value);
            var lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(_window.GetRelativeMousePosition() + _scrollPosition) && tile.Texture)
            {
                _tooltipWindow.SetTooltip(tile.Name, tile.Texture, tile.MinSideSize * 500,
                    Utilities.GetMousePosition());
            }
        }

        GUILayout.EndScrollView();
    }
}
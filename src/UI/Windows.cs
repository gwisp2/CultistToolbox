using UnityEngine;

namespace MoMEssentials.UI
{
    public static class Windows
    {
        // Window IDs
        public const int TilesWindowId = 10;
        public const int TileTooltipWindowId = 11;
        public const int ItemsWindowId = 20;
        public const int ItemTooltipWindowId = 21;

        // Default window positions
        public static readonly Rect TilesWindowRect = new(10, 10, 400, 400);
        public static readonly Rect ItemsWindowRect = new(430, 10, 400, 400);
    }
}
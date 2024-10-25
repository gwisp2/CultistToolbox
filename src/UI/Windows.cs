using UnityEngine;

namespace MoMEssentials.UI
{
    public static class Windows
    {
        // Window IDs
        public const int BaseWindowId = 48000;
        public const int MenuWindowId = BaseWindowId + 0;
        public const int TilesWindowId = BaseWindowId + 10;
        public const int TileTooltipWindowId = BaseWindowId + 11;
        public const int ItemsWindowId = BaseWindowId + 20;
        public const int ItemTooltipWindowId = BaseWindowId + 21;
        public const int InvestigatorsWindowId = BaseWindowId + 30;
        public const int FsmToolsWindowId = BaseWindowId + 40;

        // Default window positions
        public static readonly Rect MenuWindowRect = new(10, 10, 130, 130);
        public static readonly Rect TilesWindowRect = new(430, 10, 400, 400);
        public static readonly Rect ItemsWindowRect = new(830, 10, 400, 400);
        public static readonly Rect InvestigatorsWindowRect = new(10, 430, 400, 400);
        public static readonly Rect FsmToolsWindowRect = new(10, 500, 400, 400);
    }
}
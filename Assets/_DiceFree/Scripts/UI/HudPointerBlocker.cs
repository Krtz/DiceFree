using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.UI
{
    public static class HudPointerBlocker
    {
        private static readonly List<HudWidget> widgets = new();
        public static void Register(HudWidget widget) => widgets.Add(widget);
        public static void Unregister(HudWidget widget) => widgets.Remove(widget);
        public static bool Covers(Vector2 screenPoint)
        {
            if (TraversalOverlay.CoversPointer(screenPoint)) return true;
            var point = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            foreach (var widget in widgets) if (widget.Bounds.Contains(point)) return true;
            return false;
        }
    }
}

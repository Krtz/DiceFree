using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.UI
{
    public static class HudPointerBlocker
    {
        private static readonly List<HudWidget> widgets = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionRegistry() => widgets.Clear();
        public static void Register(HudWidget widget) { if (widget != null && !widgets.Contains(widget)) widgets.Add(widget); }
        public static void Unregister(HudWidget widget) => widgets.Remove(widget);
        public static bool ModalOpen => widgets.Exists(w => w is OptionsPanel options && options.isActiveAndEnabled && options.Open);
        public static bool Covers(Vector2 screenPoint)
        {
            if (ModalOpen) return true;
            if (TraversalOverlay.CoversPointer(screenPoint)) return true;
            var point = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            for (int i = widgets.Count - 1; i >= 0; i--)
            {
                if (widgets[i] == null) { widgets.RemoveAt(i); continue; }
                if (!widgets[i].BlocksPointer) continue;
                if (widgets[i].Bounds.Contains(point)) return true;
            }
            return false;
        }
    }
}

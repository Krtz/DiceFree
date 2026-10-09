using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.UI
{
    public static class HudPointerBlocker
    {
        private static readonly List<HudWidget> widgets = new();
        // Registered by application-level overlays without creating a UI -> Application assembly dependency.
        public static System.Func<Vector2, bool> AdditionalPointerCover;
        public static event System.Func<Vector2, bool> OverlayCovers;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionRegistry() { widgets.Clear(); OverlayCovers = null; }
        public static void Register(HudWidget widget) { if (widget != null && !widgets.Contains(widget)) widgets.Add(widget); }
        public static void Unregister(HudWidget widget) => widgets.Remove(widget);
        public static bool ModalOpen => widgets.Exists(w => w is OptionsPanel options && options.isActiveAndEnabled && options.Open);
        public static bool Covers(Vector2 screenPoint)
        {
            if (ModalOpen) return true;
            if (OverlayCovers != null)
                foreach (System.Func<Vector2, bool> cover in OverlayCovers.GetInvocationList())
                    if (cover(screenPoint)) return true;
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

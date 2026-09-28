using UnityEngine;

namespace DiceFree.UI
{
    public abstract class HudWidget : MonoBehaviour
    {
        public abstract Rect Bounds { get; }
        protected virtual void OnEnable() => HudPointerBlocker.Register(this);
        protected virtual void OnDisable() => HudPointerBlocker.Unregister(this);
        protected static void Hp(Rect rect, float current, float maximum)
        {
            GUI.Box(rect, GUIContent.none);
            var color = GUI.color;
            GUI.color = current / maximum < 0.3f ? new Color(1, 0.25f, 0.2f) : new Color(0.3f, 0.85f, 0.35f);
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * current / maximum, rect.height - 4), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.Label(rect, $"  HP {current:0.0} / {maximum:0.0}");
        }
    }
}

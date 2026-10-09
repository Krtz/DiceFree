using DiceFree.Items;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Paint only: callers retain their slot, tooltip and button/input handling.</summary>
    public static class ItemIconGUI
    {
        public static Rect ImageRect(Rect slot, Sprite sprite, float inset = 3f)
        {
            float width = Mathf.Max(0, slot.width - inset * 2);
            float height = Mathf.Max(0, slot.height - inset * 2);
            float scale = Mathf.Min(width / sprite.rect.width, height / sprite.rect.height);
            var size = sprite.rect.size * scale;
            return new Rect(slot.center - size * .5f, size);
        }

        public static bool Draw(Rect slot, ItemDefinition item, float inset = 3f)
        {
            if (item == null || item.icon == null) return false;
            var sprite = item.icon;
            Rect pixels = sprite.textureRect;
            var uv = new Rect(pixels.x / sprite.texture.width, pixels.y / sprite.texture.height,
                pixels.width / sprite.texture.width, pixels.height / sprite.texture.height);
            Color previous = GUI.color;
            try
            {
                GUI.color = Color.white;
                GUI.DrawTextureWithTexCoords(ImageRect(slot, sprite, inset), sprite.texture, uv, true);
            }
            finally { GUI.color = previous; }
            return true;
        }
    }
}

using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class ResourceBar : HudWidget
    {
        [SerializeField] private ActorResourceController resources;

        public override Rect Bounds => new Rect(16, Screen.height - 118, 300, 24);

        public void Configure(ActorResourceController value) => resources = value;

        private void OnGUI()
        {
            if (resources == null || !resources.Has(ResourceIds.Mana) || HudPointerBlocker.ModalOpen) return;

            var r = Bounds;
            float current = resources.Current(ResourceIds.Mana);
            float maximum = resources.Maximum(ResourceIds.Mana);
            GUI.Box(r, GUIContent.none);
            float fraction = maximum <= 0 ? 0 : Mathf.Clamp01(current / maximum);
            GUI.Box(new Rect(r.x + 2, r.y + 2, (r.width - 4) * fraction, r.height - 4), GUIContent.none);
            GUI.Label(r, "Mana " + current.ToString("0.0") + " / " + maximum.ToString("0.0") +
                         "   +" + resources.Regeneration(ResourceIds.Mana).ToString("0.0") + "/s");
        }
    }
}

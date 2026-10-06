using DiceFree.Combat;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class ResourceBar : HudWidget
    {
        [SerializeField] private ActorResourceController resources;
        [SerializeField] private PhysicalSkillProgression physical;

        public override Rect Bounds => new Rect(16, Screen.height - 118, 300, 24);

        public void Configure(ActorResourceController value, PhysicalSkillProgression physicalProgression)
        {
            resources = value;
            physical = physicalProgression;
        }

        private void OnGUI()
        {
            if (resources == null || physical == null || !physical.ActiveForCurrentClass ||
                !resources.Has(PhysicalSkillCaster.ManaResourceId) || HudPointerBlocker.ModalOpen) return;

            var r = Bounds;
            float current = resources.Current(PhysicalSkillCaster.ManaResourceId);
            float maximum = resources.Maximum(PhysicalSkillCaster.ManaResourceId);
            GUI.Box(r, GUIContent.none);
            float fraction = maximum <= 0 ? 0 : Mathf.Clamp01(current / maximum);
            GUI.Box(new Rect(r.x + 2, r.y + 2, (r.width - 4) * fraction, r.height - 4), GUIContent.none);
            GUI.Label(r, "Mana " + current.ToString("0.0") + " / " + maximum.ToString("0.0") +
                         "   +" + resources.Regeneration(PhysicalSkillCaster.ManaResourceId).ToString("0.0") + "/s");
        }
    }
}

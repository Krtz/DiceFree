using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.Skills;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class NoviceSkillBar : HudWidget
    {
        [SerializeField] private NoviceSkillProgression progression;
        [SerializeField] private NoviceSkillCaster caster;
        [SerializeField] private SkillTargetingController targeting;

        private InputBindings bindings;
        private InputAction[] slots;

        public override Rect Bounds => new Rect(Screen.width - 430, Screen.height - 250, 414, 78);

        public void Configure(
            NoviceSkillProgression value,
            NoviceSkillCaster skillCaster,
            SkillTargetingController targeter = null)
        {
            progression = value;
            caster = skillCaster;
            targeting = targeter != null ? targeter : GetComponent<SkillTargetingController>();
        }

        private void Awake()
        {
            targeting ??= GetComponent<SkillTargetingController>();
            bindings = InputBindings.Current;
            slots = new[]
            {
                bindings.Action("Gameplay/Novice skill 1"),
                bindings.Action("Gameplay/Novice skill 2"),
                bindings.Action("Gameplay/Novice skill 3"),
                bindings.Action("Gameplay/Novice skill 4")
            };
        }

        private void Update()
        {
            if (bindings == null || bindings.Suppressed || progression == null || caster == null ||
                !progression.ActiveForCurrentClass) return;

            for (int i = 0; i < slots.Length; i++)
                if (slots[i].WasPressedThisFrame())
                    Prepare(progression.ActiveAtSlot(i));
        }

        private void Prepare(NoviceSkillDefinition definition)
        {
            if (definition == null || targeting == null || !caster.CanPrepare(definition)) return;

            bool hostile = definition.kind == NoviceSkillKind.StrengthMeleeStun ||
                           definition.kind == NoviceSkillKind.MagicSand;
            if (hostile)
                targeting.BeginHostile(
                    definition.displayName,
                    target => caster.Cast(definition, target),
                    definition.range);
            else
                targeting.BeginFriendly(
                    definition.displayName,
                    target => caster.Cast(definition, target),
                    definition.range);
        }

        private void OnGUI()
        {
            if (progression == null || caster == null || !progression.ActiveForCurrentClass ||
                HudPointerBlocker.ModalOpen) return;

            var r = Bounds;
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 8, r.y + 4, r.width - 16, 20),
                targeting != null && targeting.Active
                    ? targeting.SkillLabel + " — choose target"
                    : string.IsNullOrEmpty(caster.Feedback) ? "Novice skills" : caster.Feedback);

            const float gap = 4;
            float width = (r.width - 16 - gap * 3) / 4;
            for (int i = 0; i < 4; i++)
            {
                var definition = progression.ActiveAtSlot(i);
                if (definition == null) continue;
                int rank = progression.Rank(definition.stableId);
                float remaining = caster.CooldownRemaining(definition);
                string key = InputBindings.Display(slots[i]);
                string label = "[" + key + "] " + definition.displayName + "\nR" + rank + "/" + definition.maxRank;
                if (remaining > 0) label += "  " + remaining.ToString("0.0") + "s";
                var button = new Rect(r.x + 8 + i * (width + gap), r.y + 26, width, 44);
                if (GUI.Button(button, new GUIContent(label,
                    SkillTooltips.Describe(definition,rank,key)))) Prepare(definition);
            }
            HudTooltip.DrawCurrent();
        }
    }
}

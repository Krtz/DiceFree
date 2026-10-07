using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class CharacterStatsHud : CustomizableHudWidget
    {
        [SerializeField] private CombatActor player;
        [SerializeField] private ExperienceProgression progression;
        [SerializeField] private BasicAttack basicAttack;
        [SerializeField] private TraversalMotor motor;

        public override string LayoutId => "stats";
        public override string DisplayName => "Stats + XP";
        public override Rect DefaultNormalizedBounds => new(0.228f, 0.715f, 0.305f, 0.275f);
        public override Vector2 MinimumPixelSize => new(350, 215);

        public void Configure(CombatActor actor)
        {
            player = actor;
            progression = actor == null ? null : actor.GetComponent<ExperienceProgression>();
            basicAttack = actor == null ? null : actor.GetComponent<BasicAttack>();
            motor = actor == null ? null : actor.GetComponent<TraversalMotor>();
        }

        private void Awake()
        {
            if (player == null) player = GetComponent<CombatActor>();
            if (player != null) Configure(player);
        }

        private void OnGUI()
        {
            if (player == null || progression == null || HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            var stats = player.Stats;
            var attributes = stats.Attributes;

            float titleHeight = Mathf.Clamp(inner.height * 0.14f, 24, 36);
            float xpHeight = Mathf.Clamp(inner.height * 0.10f, 17, 24);
            var titleRect = new Rect(inner.x, inner.y, inner.width, titleHeight);
            HudChrome.DrawHeader(
                titleRect,
                Theme,
                $"{stats.Definition.displayName}   Level {stats.Level}");

            float bodyY = titleRect.yMax + Theme.gap;
            float bodyHeight = inner.yMax - bodyY - xpHeight - Theme.gap;
            float columnGap = Mathf.Max(8, Theme.gap * 3);
            float columnWidth = (inner.width - columnGap) * 0.5f;
            var left = new Rect(inner.x, bodyY, columnWidth, bodyHeight);
            var right = new Rect(left.xMax + columnGap, bodyY, columnWidth, bodyHeight);

            float attackMin = 0f;
            float attackMax = 0f;
            string attackTooltip = "No basic attack.";
            if (basicAttack?.Definition != null)
            {
                Vector2 range = basicAttack.Definition.RawDamageRange(attributes)
                                * stats.BasicAttackDamageMultiplier;
                attackMin = range.x;
                attackMax = range.y;

                float interval = Mathf.Max(
                    basicAttack.Definition.interval,
                    basicAttack.Definition.windup) / Mathf.Max(0.01f, stats.AttackSpeed);
                attackTooltip =
                    $"Attack Speed: {stats.AttackSpeed:0.###}x\n" +
                    $"Basic attack interval: {interval:0.###} sec\n" +
                    "Damage shown is raw pre-mitigation damage.";
            }

            float magicalDefense = stats.Defense(DamageChannel.Magical);
            float physicalDefense = stats.Defense(DamageChannel.Physical);
            float effectiveMoveSpeed = motor == null ? stats.MoveSpeed : motor.Speed;
            float contextualBonus = stats.MoveSpeed <= 0.001f
                ? 0f
                : (effectiveMoveSpeed / stats.MoveSpeed - 1f) * 100f;

            DrawRows(left, new[]
            {
                new Row("Attack", $"{attackMin:0.#}–{attackMax:0.#}", attackTooltip),
                new Row(
                    "M Def",
                    magicalDefense.ToString("0.#"),
                    DefenseTooltip(stats, DamageChannel.Magical, magicalDefense, "Magical")),
                new Row(
                    "P Def",
                    physicalDefense.ToString("0.#"),
                    DefenseTooltip(stats, DamageChannel.Physical, physicalDefense, "Physical")),
                new Row(
                    "Move Speed",
                    MovementUnits.DisplaySpeed(effectiveMoveSpeed).ToString("0.#"),
                    $"Effective movement speed: {effectiveMoveSpeed:0.###} m/s.\n" +
                    $"Character speed before surface/context bonuses: {stats.MoveSpeed:0.###} m/s.\n" +
                    (Mathf.Abs(contextualBonus) < 0.05f
                        ? "No contextual movement bonus is active."
                        : $"Current surface/context bonus: {contextualBonus:+0.#;-0.#}%."))
            });

            DrawRows(right, new[]
            {
                new Row("STR", attributes.strength.ToString("0.#"), AttributeTooltip(stats, attributes, AttributeKind.Strength)),
                new Row("AGI", attributes.agility.ToString("0.#"), AttributeTooltip(stats, attributes, AttributeKind.Agility)),
                new Row("INT", attributes.intelligence.ToString("0.#"), AttributeTooltip(stats, attributes, AttributeKind.Intelligence)),
                new Row("SPI", attributes.spirit.ToString("0.#"), AttributeTooltip(stats, attributes, AttributeKind.Spirit)),
                new Row("VIT", attributes.vitality.ToString("0.#"), AttributeTooltip(stats, attributes, AttributeKind.Vitality))
            });

            float required = Mathf.Max(1, progression.RequiredXp);
            var xpRect = new Rect(inner.x, inner.yMax - xpHeight, inner.width, xpHeight);
            DrawBar(
                xpRect,
                progression.CurrentXp / required,
                new Color(0.48f, 0.12f, 0.72f, 1f),
                $"XP {progression.CurrentXp:0} / {progression.RequiredXp:0}");

            HudTooltip.DrawCurrent();
        }

        private enum AttributeKind
        {
            Strength,
            Agility,
            Intelligence,
            Spirit,
            Vitality
        }

        private string AttributeTooltip(
            ActorStats stats,
            AttributeValues attributes,
            AttributeKind kind)
        {
            var boosted = attributes;
            string label;
            string benefit;

            switch (kind)
            {
                case AttributeKind.Strength:
                    boosted.strength += 1f;
                    label = "Strength";
                    benefit =
                        $"+{stats.SecondaryCoefficient(SecondaryStat.PhysicalDefense):0.###} Physical Defense per point.";
                    break;

                case AttributeKind.Agility:
                    boosted.agility += 1f;
                    label = "Agility";
                    benefit =
                        $"+{stats.SecondaryCoefficient(SecondaryStat.AttackSpeed) * 100f:0.###}% base Attack Speed per point.\n" +
                        $"+{stats.SecondaryCoefficient(SecondaryStat.MoveSpeed) * 100f:0.###}% base Move Speed per point.";
                    break;

                case AttributeKind.Intelligence:
                    boosted.intelligence += 1f;
                    label = "Intelligence";
                    benefit =
                        $"+{stats.SecondaryCoefficient(SecondaryStat.MagicalDefense):0.###} Magical Defense per point.";
                    break;

                case AttributeKind.Spirit:
                    boosted.spirit += 1f;
                    label = "Spirit";
                    benefit =
                        $"+{stats.SecondaryCoefficient(SecondaryStat.HealingDone) * 100f:0.###}% Healing Done per point.\n" +
                        $"+{stats.SecondaryCoefficient(SecondaryStat.HealingReceived) * 100f:0.###}% Healing Received per point.";
                    break;

                case AttributeKind.Vitality:
                    boosted.vitality += 1f;
                    label = "Vitality";
                    benefit =
                        $"+{stats.HpPerVitality:0.###} maximum HP per point.\n" +
                        $"+{stats.RegenerationPerVitality:0.###} HP/sec regeneration per point.";
                    break;

                default:
                    return "";
            }

            if (basicAttack?.Definition != null)
            {
                Vector2 before = basicAttack.Definition.RawDamageRange(attributes) *
                                 stats.BasicAttackDamageMultiplier;
                Vector2 after = basicAttack.Definition.RawDamageRange(boosted) *
                                stats.BasicAttackDamageMultiplier;
                float minGain = after.x - before.x;
                float maxGain = after.y - before.y;
                if (Mathf.Abs(minGain) > 0.001f || Mathf.Abs(maxGain) > 0.001f)
                    benefit +=
                        $"\nCurrent basic attack gains {minGain:0.##}–{maxGain:0.##} raw damage from the next point.";
            }

            return label + "\n" + benefit;
        }

        private static string DefenseTooltip(
            ActorStats stats,
            DamageChannel channel,
            float underlying,
            string label)
        {
            var targetModifiers = stats.DefenseModifiers(channel);
            float effective = DefenseMath.Effective(underlying, targetModifiers, default);
            float multiplier = DefenseMath.DamageMultiplier(effective, stats.Definition.tuning);
            float percent = (1f - multiplier) * 100f;

            if (percent >= 0)
                return $"{label} Defense currently reduces {label.ToLowerInvariant()} damage by {percent:0.#}% before resistances and separate damage-taken effects.";

            return $"{label} Defense currently increases {label.ToLowerInvariant()} damage taken by {-percent:0.#}% before resistances and separate damage-taken effects.";
        }

        private static void DrawRows(Rect rect, Row[] rows)
        {
            float row = rect.height / Mathf.Max(1, rows.Length);
            for (int i = 0; i < rows.Length; i++)
            {
                var line = new Rect(rect.x, rect.y + row * i, rect.width, row);
                var content = new GUIContent(rows[i].label, rows[i].tooltip);
                GUI.Label(
                    new Rect(line.x, line.y, line.width * 0.68f, line.height),
                    content);
                GUI.Label(
                    new Rect(line.x + line.width * 0.68f, line.y, line.width * 0.32f, line.height),
                    new GUIContent(rows[i].value, rows[i].tooltip),
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight });
            }
        }

        private readonly struct Row
        {
            public readonly string label;
            public readonly string value;
            public readonly string tooltip;

            public Row(string label, string value, string tooltip)
            {
                this.label = label;
                this.value = value;
                this.tooltip = tooltip;
            }
        }
    }
}

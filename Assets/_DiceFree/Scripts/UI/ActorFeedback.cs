using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class ActorFeedback : MonoBehaviour
    {
        public enum NameplateMode { Always, WhenHurt, Never }
        [SerializeField] private Transform visual;
        [SerializeField] private NameplateMode nameplates = NameplateMode.Always;
        [SerializeField] private bool floatingNumbers = true;
        public Transform Visual => visual;
        private CombatActor actor;
        private BasicAttack attack;
        private OverworldRespawn respawn;
        private WorldFogOfWar fog;
        private Vector3 normalScale;
        private readonly CombatTextBuffer combatText = new();
        private int observedMisses;
        private GUIStyle numberStyle, nameStyle;
        public CombatTextBuffer CombatText => combatText;
        private void Awake()
        {
            actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>();
            respawn = GetComponent<OverworldRespawn>();
            fog = FindAnyObjectByType<WorldFogOfWar>();
            normalScale = visual != null ? visual.localScale : Vector3.one;
            observedMisses = attack != null ? attack.Misses : 0;
        }
        private void OnEnable()
        {
            actor.Health.Damaged += OnDamage;
            actor.Health.PacketResolved += OnPacket;
            actor.Health.Healed += OnHeal;
            actor.Health.Restored += ClearFeedback;
        }
        private void OnDisable()
        {
            actor.Health.Damaged -= OnDamage;
            actor.Health.PacketResolved -= OnPacket;
            actor.Health.Healed -= OnHeal;
            actor.Health.Restored -= ClearFeedback;
            ClearFeedback();
        }
        private void ClearFeedback()
        {
            combatText.Clear();
            observedMisses = attack != null ? attack.Misses : 0;
        }
        private void AddText(CombatTextKind kind, float amount)
        {
            if (!floatingNumbers) return;
            combatText.Add(kind, amount, transform.position + Vector3.up *
                (OverheadUnitHealthBars.OverheadHeight(actor) + 1.05f), Time.time);
        }
        private void OnHeal(float amount) => AddText(CombatTextKind.Healing, amount);
        private void OnPacket(CombatActor source, DamageResult result)
        {
            if (result.applied > 0) AddText(result.critical?.Triggered == true ? CombatTextKind.Critical : CombatTextKind.Damage, result.applied);
            if (result.absorbed > 0) AddText(CombatTextKind.Shield, result.absorbed);
            if (result.restored > 0) AddText(CombatTextKind.Resistance, result.restored);
        }
        private void OnDamage(CombatActor source, DamageResult result)
        {
            if (visual == null) return; // Readout-only units don't produce legacy hit-log spam.
            Debug.Log($"COMBAT_HIT {source?.Stats.Definition.stableId ?? "environment"} -> {actor.Stats.Definition.stableId}: " +
                $"raw={result.raw:0.00} defense={result.defense:0.00} element={result.element?.stableId ?? "none"} " +
                $"resistance={result.resistance:0.00} final={result.mitigated:0.00} hpDamage={result.applied:0.00}");
        }
        private void LateUpdate()
        {
            combatText.Tick(Time.time);
            // BasicAttack deliberately doesn't publish missed packets to Health.
            // Observe its committed miss counter; never reroll accuracy for visuals.
            if (attack != null && attack.Misses != observedMisses)
            {
                if (attack.Misses > observedMisses && attack.Target != null)
                    attack.Target.GetComponent<ActorFeedback>()?.AddText(CombatTextKind.Miss, 0);
                observedMisses = attack.Misses;
            }
            if (visual == null) return;
            var motion=GetComponent<PlayerMotionPresentation>();
            if(motion!=null&&motion.ControllerClassId==actor.Stats.Definition?.stableId){visual.localScale=normalScale;return;}
            visual.localScale = Vector3.Scale(normalScale, !actor.Alive ? new Vector3(1.3f,0.18f,1.3f) :
                attack != null && attack.State == "Wind-up" ? new Vector3(1.08f,0.86f,1.08f) : Vector3.one);
        }
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen || DeathRespawnHud.HasFallenPlayer || (HudLayoutManager.Current?.EditMode ?? false)) return;
            if (fog == null) fog=FindAnyObjectByType<WorldFogOfWar>();
            if (fog != null && !fog.IsVisible(transform.position)) return;
            if (Camera.main == null) return;
            var p = Camera.main.WorldToScreenPoint(transform.position +
                Vector3.up * (OverheadUnitHealthBars.OverheadHeight(actor) + .42f));
            if (p.z <= 0) return;
            if (HudPointerBlocker.Covers(new Vector2(p.x,p.y))) return;
            string label = $"{actor.DisplayName} L{actor.Stats.Level}";
            if (nameStyle == null) nameStyle = new GUIStyle(GUI.skin.label)
            {
                alignment=TextAnchor.MiddleCenter,
                fontStyle=FontStyle.Bold,
                normal={textColor=Color.white},
                richText=false
            };
            float width = Mathf.Max(180, nameStyle.CalcSize(new GUIContent(label)).x + 12);
            var rect = new Rect(p.x-width*.5f,Screen.height-p.y-15,width,24);
            if (nameplates == NameplateMode.Always || (nameplates == NameplateMode.WhenHurt && actor.Health.Current < actor.Health.Maximum))
                DrawOutlined(rect, label, nameStyle, Color.white);
            if (floatingNumbers) DrawCombatText(Camera.main);
            if (!actor.Alive && respawn != null) GUI.Label(new Rect(p.x-60,rect.y+24,160,24),$"Respawn in {respawn.Remaining:0.0}s");
        }
        private void DrawCombatText(Camera camera)
        {
            if (numberStyle == null) numberStyle = new GUIStyle(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, richText = false, clipping = TextClipping.Overflow };
            float scale = Mathf.Clamp(Screen.height / 900f, .85f, 1.4f);
            foreach (var entry in combatText.Entries)
            {
                float age = (Time.time - entry.born) / CombatTextBuffer.Lifetime;
                if (age < 0 || age >= 1) continue;
                var point = camera.WorldToScreenPoint(entry.origin);
                if (point.z <= .01f || point.z > 75) continue;
                Color tint = entry.kind switch
                {
                    CombatTextKind.Critical => new Color(1f, .82f, .25f),
                    CombatTextKind.Healing or CombatTextKind.Resistance => new Color(.42f, 1f, .55f),
                    CombatTextKind.Shield => new Color(.45f, .82f, 1f),
                    CombatTextKind.Miss => new Color(.78f, .82f, .9f),
                    _ => actor.Faction == 0 ? new Color(1f, .48f, .40f) : new Color(1f, .96f, .83f)
                };
                tint.a = Mathf.Clamp01((1 - age) / .35f);
                numberStyle.fontSize = Mathf.RoundToInt((entry.kind == CombatTextKind.Critical ? 20 : 16) * scale);
                float drift = (entry.lane % 2 == 0 ? -1 : 1) * (8 + age * 14);
                var rect = new Rect(point.x - 95 * scale + drift * scale,
                    Screen.height - point.y - (36 + age * 54 + entry.lane * 26) * scale, 190 * scale, 30 * scale);
                if (HudPointerBlocker.Covers(new Vector2(rect.center.x,Screen.height-rect.center.y))) continue;
                DrawOutlined(rect, entry.Text, numberStyle, tint);
            }
        }
        private static void DrawOutlined(Rect rect, string text, GUIStyle style, Color tint)
        {
            style.normal.textColor = new Color(.015f, .012f, .01f, tint.a * .95f);
            GUI.Label(new Rect(rect.x - 1, rect.y, rect.width, rect.height), text, style);
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, style);
            style.normal.textColor = tint;
            GUI.Label(rect, text, style);
        }
        public void Configure(Transform model) => visual = model;
        public void ConfigureReadoutOnly() { visual = null; nameplates = NameplateMode.WhenHurt; }
    }
}

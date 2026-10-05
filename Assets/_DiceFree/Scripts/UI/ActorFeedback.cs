using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    [RequireComponent(typeof(CombatActor))]
    public sealed class ActorFeedback : MonoBehaviour
    {
        public enum NameplateMode { Always, WhenHurt, Never }
        [SerializeField] private Transform visual;
        [SerializeField] private NameplateMode nameplates = NameplateMode.Always;
        [SerializeField] private bool floatingNumbers = true;
        private CombatActor actor;
        private BasicAttack attack;
        private OverworldRespawn respawn;
        private Vector3 normalScale;
        private string lastHit;
        private float hitUntil;
        private void Awake()
        {
            actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>();
            respawn = GetComponent<OverworldRespawn>();
            normalScale = visual.localScale;
        }
        private void OnEnable() => actor.Health.Damaged += OnDamage;
        private void OnDisable() => actor.Health.Damaged -= OnDamage;
        private void OnDamage(CombatActor source, DamageResult result)
        {
            lastHit = $"-{result.applied:0.0}"; hitUntil = Time.time + 0.8f;
            Debug.Log($"COMBAT_HIT {source?.Stats.Definition.stableId ?? "environment"} -> {actor.Stats.Definition.stableId}: " +
                $"raw={result.raw:0.00} defense={result.defense:0.00} element={result.element?.stableId ?? "none"} " +
                $"resistance={result.resistance:0.00} final={result.mitigated:0.00} hpDamage={result.applied:0.00}");
        }
        private void LateUpdate()
        {
            visual.localScale = Vector3.Scale(normalScale, !actor.Alive ? new Vector3(1.3f,0.18f,1.3f) :
                attack != null && attack.State == "Wind-up" ? new Vector3(1.08f,0.86f,1.08f) : Vector3.one);
        }
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            if (Camera.main == null) return;
            var p = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.1f);
            if (p.z <= 0) return;
            var rect = new Rect(p.x-90,Screen.height-p.y-15,180,24);
            if (nameplates == NameplateMode.Always || (nameplates == NameplateMode.WhenHurt && actor.Health.Current < actor.Health.Maximum))
                GUI.Box(rect, $"{actor.Stats.Definition.displayName} L{actor.Stats.Level} {actor.Health.Current:0}/{actor.Health.Maximum:0}");
            if (floatingNumbers && Time.time < hitUntil) GUI.Label(new Rect(rect.x+65,rect.y-24,60,24), lastHit);
            if (!actor.Alive && respawn != null) GUI.Label(new Rect(rect.x+30,rect.y+24,160,24),$"Respawn in {respawn.Remaining:0.0}s");
        }
        public void Configure(Transform model) => visual = model;
    }
}

using UnityEngine;

namespace DiceFree.Combat
{
    [RequireComponent(typeof(CombatActor))]
    public sealed class BasicAttack : MonoBehaviour
    {
        private CombatActor actor;
        private float readyAt, impactAt, repathAt;
        private bool winding;
        public CombatActor Target { get; private set; }
        public string State { get; private set; } = "Idle";
        public int Hits { get; private set; }
        public int Misses { get; private set; }
        public event System.Action<CombatActor,DamageResult> Connected;
        public float CooldownRemaining => Mathf.Max(0, readyAt - Time.time);
        /// <summary>Extra damage only for actual auto-attack impacts.
        /// Abilities, scripted lunges and boss special hits are unaffected.</summary>
        public float BasicAttackDamageScale { get; set; } = 1f;
        public AttackDefinition Definition => actor.Stats.Definition.basicAttack;
        public Vector2 DamageRange
        {
            get
            {
                if(Definition==null)return Vector2.zero;
                var stats=actor.Stats;
                Vector2 raw=Definition.RawDamageRange(stats.Attributes)*BasicAttackDamageScale;
                return new Vector2(
                    Mathf.Max(0,raw.x*stats.BasicAttackDamageMultiplier+stats.BasicAttackFlatBonus+stats.Definition.basicAttackMinimumOffset),
                    Mathf.Max(0,(raw.y+stats.BasicAttackMaximumBonus)*stats.BasicAttackDamageMultiplier+stats.BasicAttackFlatBonus+stats.Definition.basicAttackMaximumOffset));
            }
        }
        private void Awake() => actor = GetComponent<CombatActor>();
        private void OnDisable() => Cancel();
        public bool Order(CombatActor target)
        {
            if (!actor.CanAct || !actor.IsHostileTo(target) || Definition == null) return false;
            if (Target != target) { Cancel(); Target = target; }
            return true;
        }
        public void Cancel()
        {
            Target = null; winding = false; State = "Idle";
            if (actor != null) actor.Motor.Stop();
            // Deliberately preserve the cooldown: changing targets cannot grant free hits.
        }
        public void ResetForSpawn() { Cancel(); readyAt = impactAt = repathAt = 0; }
        public bool InRange(CombatActor target) => Vector3.Distance(transform.position, target.transform.position)
            <= actor.Radius + target.Radius + Definition.reach;
        private void Update()
        {
            if (Target == null) return;
            if (!actor.CanAct) { winding = false; actor.Motor.Stop(); State = "Disabled"; return; }
            if (!actor.IsHostileTo(Target)) { Cancel(); return; }
            if (!InRange(Target) || !actor.HasSightOf(Target))
            {
                winding = false; State = "Approaching";
                if (Time.time >= repathAt)
                {
                    repathAt = Time.time + 0.2f;
                    if (!actor.Motor.MoveTo(Target.transform.position)) Cancel();
                }
                return;
            }
            actor.Motor.Stop();
            var facing = Vector3.ProjectOnPlane(Target.transform.position-transform.position, Vector3.up);
            if (facing.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(facing);
            if (winding)
            {
                State = "Wind-up";
                if (Time.time < impactAt) return;
                winding = false;
                var resolution = AccuracyResolver.Resolve(actor, Definition);
                var packet = DamageResolver.CalculatePacket(
                    actor.Stats,
                    Target.Stats,
                    Definition,
                    resolution,
                    Definition.RollDamageMultiplier()*BasicAttackDamageScale,
                    Random.Range(0f, actor.Stats.BasicAttackMaximumBonus),
                    Random.Range(actor.Stats.Definition.basicAttackMinimumOffset,actor.Stats.Definition.basicAttackMaximumOffset));
                packet = Target.Health.ApplyPacket(actor, packet);
                if (!packet.Missed) Connected?.Invoke(Target, packet);
                if (packet.Missed) Misses++;
                else
                {
                    Hits++;
                    actor.Effects?.ResolveBasicAttackAugments(actor, Target);
                }
                State = packet.Missed ? "Miss" : "Recovery";
            }
            else if (Time.time >= readyAt)
            {
                winding = true; State = "Wind-up";
                impactAt = Time.time + Definition.windup / actor.Stats.AttackSpeed;
                MagicalBasicMissileVfx.Launch(actor, Target, impactAt - Time.time);
                readyAt = Time.time + Mathf.Max(Definition.interval, Definition.windup) / actor.Stats.AttackSpeed;
            }
            else State = "Recovery";
        }
    }
}

using UnityEngine;

namespace DiceFree.Combat
{
    public interface IBasicAttackHitReceiver
    {
        void OnBasicAttackHit(CombatActor source,DamageResult packet);
    }
    /// <summary>Optional tactical approach point for an actor pursuing a target.</summary>
    public interface IBasicAttackApproachProvider
    {
        bool TryGetApproachPosition(CombatActor target, out Vector3 position);
    }

    [RequireComponent(typeof(CombatActor))]
    public sealed class BasicAttack : MonoBehaviour
    {
        private CombatActor actor;
        private DiceFree.Skills.RangerSkillCaster ranger;
        private DiceFree.Skills.MarksmanCaster marksman;
        private DiceFree.Skills.PsionSkillCaster psion;
        private Vector3 windupOrigin;
        private IBasicAttackApproachProvider approachProvider;
        private float readyAt, impactAt, repathAt;
        private bool winding;
        public CombatActor Target { get; private set; }
        public string State { get; private set; } = "Idle";
        public int Hits { get; private set; }
        public int Misses { get; private set; }
        // Per-actor override for the authored melee stance of a ranged attacker.
        // Negative values retain the attack definition's reach.
        public float ReachOverride { get; set; } = -1f;
        public float Reach => (ReachOverride >= 0 ? ReachOverride : Definition?.reach ?? 0f) + (ranger?.RangeBonus ?? 0f) + (marksman?.RangeBonus ?? 0f);
        internal CriticalRule CriticalRule => marksman?.CriticalRule() ?? GetComponent<DiceFree.Skills.PsionEvolutionCaster>()?.BasicCriticalRule() ?? psion?.BasicCriticalRule() ?? ranger?.CriticalRule() ??
            GetComponent<DiceFree.Skills.ThiefSkillCaster>()?.BasicCriticalRule() ?? actor.Stats.ExternalCriticalRule();
        public event System.Action<CombatActor,DamageResult> Connected;
        public float CooldownRemaining => Mathf.Max(0, readyAt - Time.time);
        /// <summary>Extra damage only for actual auto-attack impacts.
        /// Abilities, scripted lunges and boss special hits are unaffected.</summary>
        public float BasicAttackDamageScale { get; set; } = 1f;
        public AttackDefinition Definition => actor.Stats.Definition.basicAttack;
        private bool HasClassDamageSpread =>
            actor.Stats.Definition.stableId?.StartsWith("class.", System.StringComparison.Ordinal) == true;
        private const float ClassMinimumDamage = .95f;
        private const float ClassMaximumDamage = 1.05f;
        // Skill/summon snapshots use DamageRange; the ranged aura belongs only to
        // the eventual basic packet and its HUD preview, never ability formulas.
        public Vector2 DisplayDamageRange=>DamageRange*DiceFree.Skills.MarksmanCaster.AuraMultiplier(actor)*(GetComponent<DiceFree.Skills.BerserkerCaster>()?.DamageMultiplier??1);
        public Vector2 DamageRange
        {
            get
            {
                if(Definition==null)return Vector2.zero;
                var stats=actor.Stats;
                Vector2 raw=Definition.RawDamageRange(stats.Attributes)*BasicAttackDamageScale;
                var range = new Vector2(
                    Mathf.Max(0,raw.x*stats.BasicAttackDamageMultiplier+stats.BasicAttackFlatBonus+stats.Definition.basicAttackMinimumOffset),
                    Mathf.Max(0,(raw.y+stats.BasicAttackMaximumBonus)*stats.BasicAttackDamageMultiplier+stats.BasicAttackFlatBonus+stats.Definition.basicAttackMaximumOffset));
                var result = HasClassDamageSpread
                    ? new Vector2(range.x * ClassMinimumDamage, range.y * ClassMaximumDamage)
                    : range;
                return result;
            }
        }
        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            ranger = GetComponent<DiceFree.Skills.RangerSkillCaster>();
            marksman = GetComponent<DiceFree.Skills.MarksmanCaster>();
            psion = GetComponent<DiceFree.Skills.PsionSkillCaster>();
            approachProvider = GetComponent<IBasicAttackApproachProvider>();
        }
        private void OnDisable() => Cancel();
        public bool Order(CombatActor target)
        {
            if (!actor.CanAct || !actor.CanTargetHostile(target) || Definition == null) return false;
            ranger?.CancelChannel();
            marksman?.CancelChannel();
            psion?.CancelChannel();
            GetComponent<DiceFree.Skills.PsionEvolutionCaster>()?.CancelChannel();
            if (Target != target) { Cancel(); Target = target; }
            return true;
        }
        public void Cancel()
        {
            Target = null; winding = false; State = "Idle";
            if (actor != null) actor.Motor.Stop();
            // Deliberately preserve the cooldown: changing targets cannot grant free hits.
        }
        public void ResetForSpawn() { Cancel(); ReachOverride=-1f; readyAt = impactAt = repathAt = 0; }
        public bool InRange(CombatActor target) => Vector3.Distance(transform.position, target.transform.position)
            <= actor.Radius + target.Radius + Reach;
        private void Update()
        {
            if (Target == null) return;
            if (!actor.CanAct) { winding = false; actor.Motor.Stop(); State = "Disabled"; return; }
            if (!actor.CanTargetHostile(Target)) { Cancel(); return; }
            if (!InRange(Target) || !actor.HasSightOf(Target))
            {
                winding = false; State = "Approaching";
                if (Time.time >= repathAt)
                {
                    repathAt = Time.time + 0.2f;
                    var targetPosition = Target.transform.position;
                    bool moved = approachProvider != null &&
                        approachProvider.TryGetApproachPosition(Target, out var tacticalPosition) &&
                        actor.Motor.MoveTo(tacticalPosition);
                    if (!moved) moved = actor.Motor.MoveTo(targetPosition);
                    if (!moved) Cancel();
                }
                return;
            }
            if ((ranger?.Active == true || marksman?.Active == true) && winding &&
                (actor.Motor.Travelling || (transform.position - windupOrigin).sqrMagnitude > .025f))
            {
                Cancel();
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
                var target = Target;
                if (ranger?.Active == true)
                {
                    ranger.Launch(target, DiceFree.Skills.RangerVfx.ArrowTint, () => ResolveImpact(target));
                    State = "Recovery";
                }
                else if (marksman?.Active == true)
                {
                    marksman.Launch(target,()=>ResolveImpact(target));State="Recovery";
                }
                else if (psion?.Active == true)
                {
                    psion.LaunchBlade(target, recipient => ResolveImpact(recipient));
                    State = "Recovery";
                }
                else State = ResolveImpact(target) ? "Miss" : "Recovery";
            }
            else if (Time.time >= readyAt)
            {
                winding = true; State = "Wind-up";
                impactAt = Time.time + Definition.windup / actor.Stats.AttackSpeed;
                windupOrigin = transform.position;
                if (ranger?.Active != true && marksman?.Active != true) MagicalBasicMissileVfx.Launch(actor, Target, impactAt - Time.time);
                readyAt = Time.time + Mathf.Max(Definition.interval, Definition.windup) / actor.Stats.AttackSpeed;
            }
            else State = "Recovery";
        }

        private bool ResolveImpact(CombatActor target)
        {
            var thief = GetComponent<DiceFree.Skills.ThiefSkillCaster>();
            float opener = thief?.ConsumeOpener() ?? 1;
            var resolution = AccuracyResolver.Resolve(actor, Definition,
                critical: CriticalRule, target: target);
            var packet = DamageResolver.CalculatePacket(
                actor.Stats,
                target.Stats,
                Definition,
                resolution,
                Definition.RollDamageMultiplier(actor.Stats.Attributes,BasicAttackDamageScale),
                Random.Range(0f, actor.Stats.BasicAttackMaximumBonus),
                Random.Range(actor.Stats.Definition.basicAttackMinimumOffset,actor.Stats.Definition.basicAttackMaximumOffset),
                finalRawMultiplier: opener * (ranger?.DamageMultiplier(target) ?? 1) * DiceFree.Skills.MarksmanCaster.AuraMultiplier(actor) * (GetComponent<DiceFree.Skills.BerserkerCaster>()?.DamageMultiplier??1) *
                    (HasClassDamageSpread ? Random.Range(ClassMinimumDamage, ClassMaximumDamage) : 1),
                flatDefensePenetration: (psion?.BasicPenetration ?? 0) + (GetComponent<DiceFree.Skills.PsionEvolutionCaster>()?.BasicPenetration ?? 0));
            packet = target.Health.ApplyPacket(actor, packet);
            if (packet.Missed) Misses++;
            else
            {
                Connected?.Invoke(target, packet);
                target.GetComponent<IBasicAttackHitReceiver>()?.OnBasicAttackHit(actor,packet);
                Hits++;
                actor.Effects?.ResolveBasicAttackAugments(actor, target);
            }
            return packet.Missed;
        }
    }
}

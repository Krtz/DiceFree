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
        public AttackDefinition Definition => actor.Stats.Definition.basicAttack;
        private void Awake() => actor = GetComponent<CombatActor>();
        private void OnDisable() => Cancel();
        public bool Order(CombatActor target)
        {
            if (!actor.IsHostileTo(target) || Definition == null) return false;
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
                DamageResolver.Hit(actor, Target, Definition); Hits++;
                State = "Recovery";
            }
            else if (Time.time >= readyAt)
            {
                winding = true; State = "Wind-up";
                impactAt = Time.time + Definition.windup / actor.Stats.AttackSpeed;
                readyAt = Time.time + Mathf.Max(Definition.interval, Definition.windup) / actor.Stats.AttackSpeed;
            }
            else State = "Recovery";
        }
    }
}

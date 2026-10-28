using System.Collections.Generic;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Briarback
{
    /// <summary>Single-phase outdoor boss behavior for the upper Briarback court.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(BasicAttack), typeof(AggroBehaviour))]
    public sealed class BriarbackChieftainEncounter : MonoBehaviour
    {
        public AttackDefinition overheadSmash;
        public AttackDefinition axeThrow;
        public CombatActor eliteWarrior;
        public CombatActor eliteBrute;
        [Min(0)] public float smashDamage = 520f;
        [Min(0)] public float axeDamage = 320f;
        [Min(0.1f)] public float axeSpeed = 16f;
        [Min(0.1f)] public float axeMaxRange = 24f;

        const string SmashBlock = "briarback.chieftain.smash";
        readonly Dictionary<CombatActor, float> threat = new();
        CombatActor actor;
        BasicAttack basic;
        AggroBehaviour aggro;
        float smashReadyAt, axeReadyAt, smashImpactAt;
        Vector3 smashForward;
        CombatActor smashTarget;
        GameObject telegraph;
        Mesh telegraphMesh;
        Material telegraphMaterial;
        bool winding;

        public bool Winding => winding;
        public float SmashCooldownRemaining => Mathf.Max(0, smashReadyAt - Time.time);
        public float AxeCooldownRemaining => Mathf.Max(0, axeReadyAt - Time.time);
        public bool AxeThrowStarted { get; private set; }
        public int SmashImpacts { get; private set; }
        public int SmashAttempts { get; private set; }
        public int SmashHostileCandidates { get; private set; }
        public int SmashLineCandidates { get; private set; }
        public int AxeHits { get; private set; }

        void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            aggro = GetComponent<AggroBehaviour>();
        }

        void OnEnable()
        {
            actor.Health.Damaged += OnDamaged;
            actor.Health.Died += OnDied;
            actor.Health.Restored += OnRestored;
        }

        void OnDisable()
        {
            if (actor?.Health != null)
            {
                actor.Health.Damaged -= OnDamaged;
                actor.Health.Died -= OnDied;
                actor.Health.Restored -= OnRestored;
            }
            CancelSmash();
        }

        void OnDamaged(CombatActor source, DamageResult result)
        {
            if (source == null || source.Faction != 0 || !source.Alive) return;
            threat[source] = threat.TryGetValue(source, out var total) ? total + Mathf.Max(1, result.applied) : Mathf.Max(1, result.applied);
            EngageGuards(source);
        }

        void OnDied()
        {
            CancelSmash();
            ResetLivingGuards();
        }

        void OnRestored()
        {
            CancelSmash();
            threat.Clear();
            smashReadyAt = axeReadyAt = 0;
            ResetLivingGuards(); // Dead guards are intentionally left to their own respawn timers.
        }

        void Update()
        {
            if (!actor.Alive) return;
            if (aggro.Returning)
            {
                CancelSmash();
                return;
            }

            PruneThreat();
            if (winding)
            {
                if (!actor.Alive || (actor.Effects != null && actor.Effects.Stunned) || smashTarget == null || !smashTarget.Alive || aggro.Returning) { CancelSmash(); return; }
                if (Time.time < smashImpactAt) return;
                ResolveSmash();
                CancelSmash();
                return;
            }

            var target = basic.Target;
            if (target == null) return;
            EngageGuards(target);

            float distance = Vector3.Distance(actor.transform.position, target.transform.position);
            if (distance <= 8f && Time.time >= smashReadyAt && overheadSmash != null)
            {
                BeginSmash(target);
                return;
            }

            if (distance > (basic.Definition?.reach ?? 1.1f) + actor.Radius + target.Radius &&
                Time.time >= axeReadyAt && axeThrow != null)
            {
                var ranged = HighestThreatOutsideMelee();
                if (ranged != null && Vector3.Distance(transform.position, ranged.transform.position) <= axeMaxRange &&
                    actor.HasSightOf(ranged))
                {
                    axeReadyAt = Time.time + 15f;
                    AxeThrowStarted = true;
                    StartCoroutine(ThrowAxe(ranged));
                }
            }
        }

        void BeginSmash(CombatActor target)
        {
            Vector3 toward = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            if (toward.sqrMagnitude < .001f) return;
            smashForward = toward.normalized;
            smashTarget = target;
            transform.rotation = Quaternion.LookRotation(smashForward);
            winding = true;
            smashReadyAt = Time.time + 12f;
            smashImpactAt = Time.time + 1.5f;
            basic.Cancel();
            actor.Motor.Stop();
            actor.SetActionBlocked(SmashBlock, true);
            actor.Motor.SetMotionBlocked(SmashBlock, true);
            ShowSmashTelegraph();
        }

        void ResolveSmash()
        {
            SmashAttempts++;
            SmashHostileCandidates = SmashLineCandidates = 0;
            foreach (var victim in CombatActor.All.ToArray())
            {
                if (!actor.IsHostileTo(victim)) continue;
                SmashHostileCandidates++;
                Vector3 delta = Vector3.ProjectOnPlane(victim.transform.position - transform.position, Vector3.up);
                float along = Vector3.Dot(delta, smashForward);
                float across = Mathf.Abs(Vector3.Dot(delta, Vector3.Cross(Vector3.up, smashForward)));
                if (along + victim.Radius < 0f || along - victim.Radius > 6f || across > .5f + victim.Radius) continue;
                SmashLineCandidates++;

                var packet = DamageResolver.Calculate(actor.Stats, victim.Stats, overheadSmash,
                    rawMultiplier: overheadSmash.RollDamageMultiplier(actor.Stats.Attributes), skillFlatDamage: smashDamage);
                packet = victim.Health.ApplyPacket(actor, packet);
                if (packet.Missed) continue;
                SmashImpacts++;
                // Shared status rules resolve shield absorption, resistance and immunity.
                victim.Effects?.TryApplyStun(1f, false);
            }
        }

        System.Collections.IEnumerator ThrowAxe(CombatActor target)
        {
            basic.Cancel();
            actor.Motor.Stop();
            Vector3 facing = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            if (facing.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(facing);
            yield return new WaitForSeconds(.65f); // Provisional wind-up; the design leaves Axe Throw wind-up open.
            AxeThrowStarted = false;
            if (!actor.Alive || !target.Alive || aggro.Returning) yield break;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Briarback Chieftain homing axe";
            go.transform.localScale = new Vector3(.42f, .12f, .68f);
            go.transform.position = transform.position + transform.forward * .9f + Vector3.up * 1.3f;
            go.GetComponent<Renderer>().sharedMaterial = AxeMaterial;
            var collider = go.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            var projectile = go.AddComponent<BriarbackHomingAxe>();
            projectile.Launch(actor, target, axeThrow, axeDamage, axeSpeed);
        }

        CombatActor HighestThreatOutsideMelee()
        {
            float melee = (basic.Definition?.reach ?? 1.1f) + actor.Radius;
            return threat.Where(pair => pair.Key != null && pair.Key.Alive && pair.Key.Faction == 0 &&
                    Vector3.Distance(transform.position, pair.Key.transform.position) > melee)
                .OrderByDescending(pair => pair.Value).ThenBy(pair => (pair.Key.transform.position - transform.position).sqrMagnitude)
                .Select(pair => pair.Key).FirstOrDefault();
        }

        void PruneThreat()
        {
            foreach (var key in threat.Keys.Where(key => key == null || !key.Alive || key.Faction != 0).ToArray()) threat.Remove(key);
        }

        void EngageGuards(CombatActor target)
        {
            if (target == null || !target.Alive) return;
            foreach (var guard in new[] { eliteWarrior, eliteBrute })
                if (guard != null && guard.Alive) guard.GetComponent<BasicAttack>()?.Order(target);
        }

        void ResetLivingGuards()
        {
            foreach (var guard in new[] { eliteWarrior, eliteBrute })
            {
                if (guard == null || !guard.Alive) continue;
                guard.GetComponent<AggroBehaviour>()?.ResetEncounter();
            }
        }

        void ShowSmashTelegraph()
        {
            telegraph = new GameObject("Chieftain smash - red fracture warning");
            telegraph.layer = 2;
            telegraph.transform.position = transform.position + Vector3.up * .07f;
            telegraph.transform.rotation = Quaternion.LookRotation(smashForward);
            telegraphMesh = new Mesh { name = "6m x 1m smash warning" };
            telegraphMesh.vertices = new[] { new Vector3(-.5f, 0, 0), new Vector3(.5f, 0, 0), new Vector3(-.5f, 0, 6), new Vector3(.5f, 0, 6) };
            telegraphMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            telegraphMesh.RecalculateNormals();
            telegraph.AddComponent<MeshFilter>().sharedMesh = telegraphMesh;
            telegraphMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = new Color(1f, .04f, .02f, .68f) };
            telegraph.AddComponent<MeshRenderer>().sharedMaterial = telegraphMaterial;
        }

        void CancelSmash()
        {
            winding = false;
            smashTarget = null;
            AxeThrowStarted = false;
            if (actor != null) { actor.SetActionBlocked(SmashBlock, false); actor.Motor.SetMotionBlocked(SmashBlock, false); }
            if (telegraph != null) Destroy(telegraph);
            if (telegraphMesh != null) Destroy(telegraphMesh);
            if (telegraphMaterial != null) Destroy(telegraphMaterial);
            telegraph = null; telegraphMesh = null; telegraphMaterial = null;
        }

        static Material axeMaterial;
        static Material AxeMaterial => axeMaterial != null ? axeMaterial : axeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.62f, .48f, .28f) };
    }

    /// <summary>Homing axe using the same swept collision policy as the existing Ironpine homing quill.</summary>
    public sealed class BriarbackHomingAxe : MonoBehaviour
    {
        CombatActor owner, target;
        AttackDefinition definition;
        float damage, speed, expires;
        Vector3 velocity;

        public void Launch(CombatActor source, CombatActor recipient, AttackDefinition attack, float flatDamage, float projectileSpeed)
        {
            owner = source; target = recipient; definition = attack; damage = flatDamage;
            speed = projectileSpeed; expires = Time.time + 8f;
            velocity = (recipient.transform.position + Vector3.up * .85f - transform.position).normalized * speed;
        }

        void Update()
        {
            if (target == null || !target.Alive || owner == null || !owner.Alive || Time.time >= expires) { Destroy(gameObject); return; }
            Vector3 desired = (target.transform.position + Vector3.up * .85f - transform.position).normalized;
            velocity = Vector3.RotateTowards(velocity.normalized, desired, 240f * Mathf.Deg2Rad * Time.deltaTime, 0) * speed;
            float length = velocity.magnitude * Time.deltaTime;
            Vector3 direction = velocity.normalized;
            var hits = Physics.SphereCastAll(transform.position, .22f, direction, length, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(owner.transform)) continue;
                var hitActor = hit.collider.GetComponentInParent<CombatActor>();
                if (hitActor == target)
                {
                    if (owner.IsHostileTo(target))
                    {
                        var packet = DamageResolver.Calculate(owner.Stats, target.Stats, definition,
                            rawMultiplier: definition.RollDamageMultiplier(owner.Stats.Attributes), skillFlatDamage: damage);
                        target.Health.ApplyPacket(owner, packet);
                    }
                    Destroy(gameObject); return;
                }
                if (hitActor != null || hit.collider.gameObject.layer == 9) { Destroy(gameObject); return; }
            }
            transform.position += direction * length;
            if (direction.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}

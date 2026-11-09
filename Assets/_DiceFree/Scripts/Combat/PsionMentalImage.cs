using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    public sealed class PsionMentalImage : MonoBehaviour
    {
        private static readonly Color Glass = new(.53f, .39f, .95f, .72f);
        private static readonly Color Edge = new(.83f, .76f, 1f, .85f);
        private CombatActor actor, windupTarget;
        private BasicAttack ownerBasic;
        private TargetSelection selection;
        private ActorDefinition definition;
        private AttackDefinition blade, blast;
        private CriticalRule crit;
        private float expires, interval, windup, launchAt, nextAttack, blastRaw, attackRange;
        private Vector2 bladeDamage;
        private int faction;
        private bool detonated, dismissed;

        public CombatActor Actor => actor;
        public float Remaining => Mathf.Max(0, expires - Time.time);
        public Vector2 SnapshotDamage => bladeDamage;
        public float SnapshotAttackInterval => interval;

        public static PsionMentalImage Summon(CombatActor owner, BasicAttack basic, Vector3 point, int rank,
            AttackDefinition blade, AttackDefinition blast, CriticalRule crit, float masteryPenetration)
        {
            var root = new GameObject("Psion Mental Image") { layer = 10 };
            root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, owner.gameObject.scene);
            root.transform.SetPositionAndRotation(point, owner.transform.rotation);
            var image = root.AddComponent<PsionMentalImage>();
            image.ownerBasic = basic;
            image.selection = owner.GetComponent<TargetSelection>();
            image.faction = owner.Faction;
            image.blade = blade;
            image.blast = blast;
            image.crit = crit;
            image.bladeDamage = basic.DamageRange * .1f;
            image.attackRange = basic.Definition.reach;
            image.interval = Mathf.Max(basic.Definition.interval, basic.Definition.windup) / owner.Stats.AttackSpeed;
            image.windup = basic.Definition.windup / owner.Stats.AttackSpeed;
            var attributes = owner.Stats.Attributes;
            image.blastRaw = 30 + 10 * rank + .6f * (attributes.agility + attributes.intelligence);
            image.definition = ScriptableObject.CreateInstance<ActorDefinition>();
            image.definition.stableId = "summon.psion.mental-image";
            image.definition.displayName = "Mental Image";
            image.definition.baseHp = owner.Health.Maximum;
            image.definition.hpPerVitality = 0;
            image.definition.overrideVitalityHp = true;
            image.definition.overrideVitalityRegeneration = true;
            image.definition.regenerationPerVitality = 0;
            image.definition.tuning = owner.Stats.Definition.tuning;
            image.definition.moveSpeed = owner.Stats.Definition.moveSpeed;
            image.definition.baseAttributes = attributes;
            image.definition.basicAttack = basic.Definition;
            image.definition.physicalDefense = Mathf.Max(0, owner.Stats.Defense(DamageChannel.Physical) -
                attributes.strength * owner.Stats.Definition.SecondaryCoefficient(SecondaryStat.PhysicalDefense));
            image.definition.magicalDefense = Mathf.Max(0, owner.Stats.Defense(DamageChannel.Magical) -
                attributes.intelligence * owner.Stats.Definition.SecondaryCoefficient(SecondaryStat.MagicalDefense));
            image.actor = root.AddComponent<CombatActor>();
            image.actor.Configure(owner.Faction, .32f);
            var stats = root.GetComponent<ActorStats>();
            stats.Configure(image.definition);
            stats.ConfigureIncomingDamageScale(3.5f);
            var penetration = owner.Stats.DefenseModifiers(DamageChannel.Physical);
            stats.SetDefenseModifier("psion.image.basic", new DefenseModifier { channel = DamageChannel.Physical,
                penetrationFlat = penetration.penetrationFlat + masteryPenetration, penetrationPercent = penetration.penetrationPercent });
            root.GetComponent<NavMeshAgent>().enabled = false;
            root.AddComponent<CombatStatusController>();
            root.AddComponent<DefeatReporter>().SetCreditOwner(owner);
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = Vector3.up * .77f;
            capsule.height = 1.54f;
            capsule.radius = .32f;
            image.BuildVisual();
            image.expires = Time.time + PsionSkillCaster.ImageDuration(rank);
            image.nextAttack = Time.time;
            root.SetActive(true);
            return image;
        }

        private void OnEnable()
        {
            actor.Health.Died += Detonate;
            actor.Motor.SetMotionBlocked("psion.mental-image", true);
        }

        private void OnDisable() => actor.Health.Died -= Detonate;
        private void OnDestroy() { if (definition != null) Destroy(definition); }

        private void Update()
        {
            if (detonated) return;
            if (Time.time >= expires)
            {
                Detonate();
                return;
            }
            if (!actor.CanAct) { windupTarget = null; return; }
            if (windupTarget != null)
            {
                if (Time.time < launchAt) return;
                var target = windupTarget;
                windupTarget = null;
                if (Valid(target)) Launch(target);
                return;
            }
            if (Time.time < nextAttack) return;
            var preferred = selection?.Selected;
            if (!Valid(preferred)) preferred = ownerBasic?.Target;
            if (!Valid(preferred))
            {
                float closest = float.PositiveInfinity;
                preferred = null;
                foreach (var candidate in CombatActor.All)
                {
                    if (!Valid(candidate)) continue;
                    float distance = (transform.position - candidate.transform.position).sqrMagnitude;
                    if (distance >= closest) continue;
                    closest = distance;
                    preferred = candidate;
                }
            }
            if (preferred == null) return;
            windupTarget = preferred;
            launchAt = Time.time + windup;
            nextAttack = Time.time + interval;
            var heading = Vector3.ProjectOnPlane(preferred.transform.position - transform.position, Vector3.up);
            if (heading.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(heading);
        }

        private bool Valid(CombatActor candidate) => actor.CanTargetHostile(candidate) && actor.HasSightOf(candidate) &&
            Vector3.Distance(transform.position, candidate.transform.position) <= attackRange + actor.Radius + candidate.Radius;

        private void Launch(CombatActor target)
        {
            // Capture the already-fired packet's offensive inputs. Keep the source stats alive
            // long enough for that projectile after the image itself leaves the battlefield.
            float raw = Random.Range(bladeDamage.x, bladeDamage.y);
            var sourceStats = actor.Stats;
            var source = actor;
            var definition = blade;
            var action = AccuracyResolver.Resolve(actor, blade, critical: crit, target: target, isBasicAttack: true);
            TargetedCombatProjectile.Launch(actor, target, "Mental Image blade", Edge, .14f, 25, recipient =>
            {
                if (dismissed || sourceStats == null) return;
                var packet = action.Hit.missed ? new DamageResult { channel = definition.channel } :
                    DamageResolver.CalculateRaw(sourceStats, recipient.Stats, definition, raw, action.Critical);
                packet.action = action;
                recipient.Health.ApplyPacket(source, packet);
            });
        }

        private void Detonate()
        {
            if (detonated) return;
            detonated = true;
            var center = transform.position;
            var victims = new List<CombatActor>();
            foreach (var candidate in CombatActor.All)
                if (candidate.Alive && candidate.Faction != faction && (candidate.transform.position - center).sqrMagnitude <= 16)
                    victims.Add(candidate);
            foreach (var victim in victims)
                if (victim.Alive)
                    victim.Health.ApplyPacket(actor, DamageResolver.CalculateRaw(actor.Stats, victim.Stats, blast, blastRaw));
            EarlySkillVfx.PlayPriestPulse("Mental Image rupture", center, 4, Glass);
            // Remove targeting and collision immediately. Keep the inactive source until
            // every projectile that already left the image has had time to resolve.
            gameObject.SetActive(false);
            Destroy(gameObject, 1.6f);
        }

        public void DismissForSessionReset()
        {
            dismissed = true;
            if (detonated) return;
            detonated = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void BuildVisual()
        {
            var material = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            var tint = new MaterialPropertyBlock();
            tint.SetColor("_BaseColor", Glass);
            Part("Faceted image body", new Vector3(0, .83f, 0), new Vector3(.47f, .82f, .30f), 0);
            Part("Image head", new Vector3(0, 1.39f, 0), Vector3.one * .30f, 0);
            Part("Left image shoulder", new Vector3(-.32f, .99f, 0), new Vector3(.23f, .53f, .25f), -24);
            Part("Right image shoulder", new Vector3(.32f, .99f, 0), new Vector3(.23f, .53f, .25f), 24);

            void Part(string name, Vector3 point, Vector3 size, float roll)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                piece.name = name;
                piece.layer = 2;
                var collider = piece.GetComponent<Collider>();
                collider.enabled = false;
                Destroy(collider);
                piece.transform.SetParent(transform, false);
                piece.transform.localPosition = point;
                piece.transform.localScale = size;
                piece.transform.localRotation = Quaternion.Euler(0,0,roll);
                var renderer = piece.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
                renderer.SetPropertyBlock(tint);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
}
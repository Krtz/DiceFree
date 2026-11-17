using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Dungeons
{
    [RequireComponent(typeof(CombatActor), typeof(BasicAttack), typeof(AggroBehaviour))]
    public sealed class ShamanRitualCasts : MonoBehaviour, IEncounterHudSource, IInterruptibleCombatCast
    {
        private const string CastLock = "fourfold.ritual-cast";
        private const float Windup = 1.65f;
        private static readonly Color[] ElementColors =
        {
            new(1f, .38f, .07f), new(.75f, .94f, .24f),
            new(.85f, .46f, 1f), new(.24f, .85f, 1f)
        };
        private static readonly string[] CastNames =
        {
            "Ember eruption · Leave the circle", "Splitroot quake · Move away from the caster",
            "Stormcall chain · Leave the marked circles", "Rimeglass burst · Leave the large circle"
        };

        public AttackDefinition[] spells;
        public Material warningMaterial;
        private CombatActor actor, target;
        private BasicAttack attack;
        private AggroBehaviour aggro;
        private readonly Vector3[] centers = new Vector3[3];
        private readonly LineRenderer[] warnings = new LineRenderer[3];
        private float nextCast, impactAt, radius;
        private int spellIndex, nextIndex, element, count;
        private bool winding;

        public CombatActor EncounterActor => actor;
        public int LivingGuards => 0;
        public bool Winding => winding;
        public string CastLabel => CastNames[element];
        public float CastRemaining => winding ? Mathf.Max(0, impactAt - Time.time) : 0;
        public float CastDuration => Windup;
        public bool EngagedWith(CombatActor viewer) =>
            viewer != null && (attack.Target == viewer || winding && target == viewer);

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            aggro = GetComponent<AggroBehaviour>();
        }

        private void OnEnable()
        {
            if (spells.Length == 4) EncounterHud.Register(this);
            actor.Health.Died += ClearCast;
            nextCast = Time.time + 2.8f;
        }

        private void OnDisable()
        {
            EncounterHud.Unregister(this);
            actor.Health.Died -= ClearCast;
            ClearCast();
        }

        private void Update()
        {
            if (winding)
            {
                if (!actor.Alive || actor.Effects?.Stunned == true || aggro.Returning)
                {
                    ClearCast();
                    return;
                }
                for (int i = 0; i < count; i++)
                    warnings[i].widthMultiplier = .11f + .16f * (1 - CastRemaining / Windup);
                if (Time.time >= impactAt) ResolveCast();
                return;
            }

            var opponent = attack.Target;
            if (Time.time >= nextCast && actor.CanAct && !aggro.Returning && actor.CanTargetHostile(opponent) &&
                actor.HasSightOf(opponent) && Vector3.Distance(transform.position, opponent.transform.position) < 13)
                BeginCast(opponent);
        }

        private void BeginCast(CombatActor opponent)
        {
            target = opponent;
            spellIndex = nextIndex;
            nextIndex = (spellIndex + 1) % spells.Length;
            winding = true;
            impactAt = Time.time + Windup;
            nextCast = impactAt + (spells.Length == 4 ? 2.3f : 4.8f);
            attack.Cancel();
            actor.Motor.Stop();
            actor.Motor.SetMotionBlocked(CastLock, true);
            actor.SetActionBlocked(CastLock, true);

            element = spells[spellIndex].stableId switch
            {
                "fourfold.ember" => 0, "fourfold.earth" => 1, "fourfold.storm" => 2, _ => 3
            };
            var aim = opponent.transform.position;
            var toward = Vector3.ProjectOnPlane(aim - transform.position, Vector3.up).normalized;
            if (toward == Vector3.zero) toward = transform.forward;
            radius = element switch { 0 => 2.9f, 1 => 5f, 2 => 1.95f, _ => 4.1f };
            count = element == 2 ? 3 : 1;
            centers[0] = element == 1 ? transform.position : aim;
            centers[1] = aim - toward * 4;
            centers[2] = aim + Vector3.Cross(Vector3.up, toward) * 4;
            for (int i = 0; i < count; i++) warnings[i] = CreateRing(centers[i], ElementColors[element]);
        }

        private LineRenderer CreateRing(Vector3 center, Color tint)
        {
            var go = new GameObject("Ritual danger circle");
            go.layer = 2;
            go.transform.SetParent(transform.parent, true);
            go.transform.SetPositionAndRotation(center + Vector3.up * .13f, Quaternion.Euler(90, 0, 0));
            var ring = go.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.alignment = LineAlignment.TransformZ;
            ring.positionCount = 56;
            ring.widthMultiplier = .11f;
            ring.sharedMaterial = warningMaterial;
            var style = new MaterialPropertyBlock();
            style.SetColor("_BaseColor", tint * 1.5f);
            ring.SetPropertyBlock(style);
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i * 2 * Mathf.PI / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
            }
            return ring;
        }

        private void ResolveCast()
        {
            foreach (var victim in CombatActor.All)
            {
                if (!actor.IsHostileTo(victim) || !actor.HasSightOf(victim)) continue;
                for (int i = 0; i < count; i++)
                {
                    var offset = victim.transform.position - centers[i];
                    float reach = radius + victim.Radius;
                    if (Mathf.Abs(offset.y) > 2 || offset.x * offset.x + offset.z * offset.z > reach * reach) continue;
                    DamageResolver.Hit(actor, victim, spells[spellIndex]);
                    break;
                }
            }

            for (int i = 0; i < count; i++)
            {
                warnings[i].widthMultiplier = .46f;
                Destroy(warnings[i].gameObject, .22f);
                warnings[i] = null;
            }
            ClearCast();
        }

        public bool TryInterruptCast()
        {
            if (!winding) return false;
            ClearCast();
            return true;
        }

        private void ClearCast()
        {
            winding = false;
            target = null;
            actor.Motor.SetMotionBlocked(CastLock, false);
            actor.SetActionBlocked(CastLock, false);
            for (int i = 0; i < count; i++)
            {
                if (warnings[i] != null) Destroy(warnings[i].gameObject);
                warnings[i] = null;
            }
            count = 0;
        }
    }
}

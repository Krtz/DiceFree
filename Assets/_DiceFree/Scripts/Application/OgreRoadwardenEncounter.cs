using System.Linq;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Trimery
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(BasicAttack), typeof(AggroBehaviour))]
    public sealed class OgreRoadwardenEncounter : MonoBehaviour, IEncounterHudSource
    {
        private enum Move { None, Hammerfall, Roadbreak }

        private const string Brace = "roadwarden.cast";
        private const float HammerLength = 9f;
        private const float HammerHalfWidth = 1.65f;
        private const float StompRadius = 4.8f;

        [SerializeField] private AttackDefinition hammerfall, roadbreak;
        [SerializeField] private Transform rig, hammerArm, freeArm;
        [SerializeField] private LineRenderer outline;
        [SerializeField] private LayerMask ground = 1 << 8;

        private MaterialPropertyBlock warningColor;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private CombatActor actor;
        private BasicAttack basic;
        private AggroBehaviour aggro;
        private CombatActor castTarget;
        private Move move, nextMove;
        private float readyAt, impactAt, recoveredAt;
        private Vector3 castOrigin, castForward;

        public CombatActor EncounterActor => actor;
        public int LivingGuards => 0;
        public string CastLabel => move == Move.Hammerfall ? "Hammerfall · Sidestep the lane" : "Roadbreak · Leave the ring";
        public float CastDuration => move == Move.Hammerfall ? 1.55f : 1.8f;
        public float CastRemaining => move == Move.None ? 0 : Mathf.Max(0, impactAt - Time.time);
        public bool EngagedWith(CombatActor viewer) => isActiveAndEnabled && actor.Alive &&
            !aggro.Returning && viewer != null && (basic.Target == viewer || castTarget == viewer);

        private void Awake()
        {
            warningColor = new MaterialPropertyBlock();
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            aggro = GetComponent<AggroBehaviour>();
        }

        private void OnEnable()
        {
            EncounterHud.Register(this);
            actor.Health.Died += CancelCast;
            actor.Health.Restored += ResetCycle;
            ResetCycle();
        }

        private void OnDisable()
        {
            EncounterHud.Unregister(this);
            actor.Health.Died -= CancelCast;
            actor.Health.Restored -= ResetCycle;
            CancelCast();
        }

        private void ResetCycle()
        {
            CancelCast();
            nextMove = Move.Hammerfall;
            readyAt = Time.time + 1.4f;
            recoveredAt = 0;
        }

        private void Update()
        {
            if (!actor.Alive) return;
            if (move != Move.None)
            {
                if (aggro.Returning || actor.Effects?.Stunned == true || castTarget == null ||
                    !castTarget.Alive || Vector3.Distance(castTarget.transform.position, aggro.Home) > aggro.Leash)
                {
                    CancelCast();
                    return;
                }
                if (Time.time < impactAt) return;
                ResolveCast();
                recoveredAt = Time.time + .48f;
                CancelCast();
                return;
            }

            var target = basic.Target;
            if (aggro.Returning || !actor.CanAct || target == null || Time.time < readyAt || !actor.HasSightOf(target))
                return;

            Vector3 toward = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            float reach = nextMove == Move.Hammerfall ? HammerLength : StompRadius;
            if (toward.sqrMagnitude < .01f || toward.sqrMagnitude > reach * reach) return;
            BeginCast(target, toward.normalized);
        }

        private void BeginCast(CombatActor target, Vector3 forward)
        {
            move = nextMove;
            nextMove = move == Move.Hammerfall ? Move.Roadbreak : Move.Hammerfall;
            castTarget = target;
            castOrigin = transform.position;
            castForward = forward;
            transform.rotation = Quaternion.LookRotation(forward);
            impactAt = Time.time + CastDuration;
            readyAt = impactAt + (move == Move.Hammerfall ? 5.8f : 6.5f);
            actor.Motor.Stop();
            actor.SetActionBlocked(Brace, true);
            actor.Motor.SetMotionBlocked(Brace, true);
            DrawTelegraph();
        }

        private void CancelCast()
        {
            move = Move.None;
            castTarget = null;
            outline.enabled = false;
            actor.SetActionBlocked(Brace, false);
            actor.Motor.SetMotionBlocked(Brace, false);
        }

        private void ResolveCast()
        {
            var attack = move == Move.Hammerfall ? hammerfall : roadbreak;
            Vector3 right = Vector3.Cross(Vector3.up, castForward);
            foreach (var victim in CombatActor.All.ToArray())
            {
                if (!actor.IsHostileTo(victim) || !actor.HasSightOf(victim)) continue;
                Vector3 delta = victim.transform.position - castOrigin;
                if (Mathf.Abs(delta.y) > 2f) continue;
                delta.y = 0;
                float along = Vector3.Dot(delta, castForward);
                bool inShape = move == Move.Hammerfall
                    ? along >= 0 && along <= HammerLength && Mathf.Abs(Vector3.Dot(delta, right)) <= HammerHalfWidth
                    : delta.sqrMagnitude <= StompRadius * StompRadius;
                if (!inShape) continue;

                var packet = DamageResolver.Calculate(actor.Stats, victim.Stats, attack,
                    rawMultiplier: attack.RollDamageMultiplier(actor.Stats.Attributes));
                victim.Health.ApplyPacket(actor, packet);
            }

            var impact = move == Move.Hammerfall ? castOrigin + castForward * 2.5f : castOrigin;
            BriarbackChieftainEffects.SmashImpact(impact + Vector3.up * .12f, castForward);
        }

        private void DrawTelegraph()
        {
            outline.enabled = true;
            outline.loop = true;
            int points = move == Move.Hammerfall ? 22 : 40;
            outline.positionCount = points;
            Vector3 right = Vector3.Cross(Vector3.up, castForward);
            for (int i = 0; i < points; i++)
            {
                Vector3 point;
                if (move == Move.Hammerfall)
                {
                    // Follow the uneven road surface with intermediate samples along both edges.
                    bool left = i < 11;
                    float fraction = (left ? i : 21 - i) / 10f;
                    point = castOrigin + castForward * (fraction * HammerLength) +
                        right * (left ? -HammerHalfWidth : HammerHalfWidth);
                }
                else
                {
                    float angle = i * Mathf.PI * 2f / points;
                    point = castOrigin + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * StompRadius;
                }

                if (Physics.Raycast(point + Vector3.up * 3f, Vector3.down, out var hit, 6f, ground, QueryTriggerInteraction.Ignore))
                    point.y = hit.point.y;
                outline.SetPosition(i, point + Vector3.up * .14f);
            }
        }

        private void LateUpdate()
        {
            if (!actor.Alive)
            {
                rig.localRotation = Quaternion.RotateTowards(rig.localRotation, Quaternion.Euler(71, 0, -23), Time.deltaTime * 145);
                rig.localPosition = Vector3.MoveTowards(rig.localPosition, new Vector3(0, -.42f, -.6f), Time.deltaTime * 1.5f);
                return;
            }

            float stride = actor.Motor.Travelling ? Mathf.Sin(Time.time * 8f) : Mathf.Sin(Time.time * 2f) * .2f;
            float charge = move == Move.None ? 0 : Mathf.SmoothStep(0, 1, 1 - CastRemaining / CastDuration);
            float recovery = Mathf.Clamp01((recoveredAt - Time.time) / .48f);
            float hammerPitch = move == Move.Hammerfall ? -148 * charge :
                move == Move.Roadbreak ? -68 * charge :
                recovery > 0 ? 64 * recovery : basic.State == "Wind-up" ? -51 : -12 + 13 * stride;
            float roll = move == Move.Roadbreak ? 17 * charge : 4 * stride;

            rig.localPosition = Vector3.up * (.04f * Mathf.Abs(stride) + (move == Move.Roadbreak ? .16f * charge : 0));
            rig.localRotation = Quaternion.Euler(-14 * charge + 15 * recovery, -8 * charge, roll);
            hammerArm.localRotation = Quaternion.Euler(hammerPitch, -8 * charge, -15 + 24 * charge);
            freeArm.localRotation = Quaternion.Euler(move == Move.Roadbreak ? -125 * charge : 10 * stride, 0, 14 + 18 * charge);

            if (move != Move.None)
            {
                var color = Color.Lerp(new Color(1f, .65f, .17f), new Color(1f, .24f, .08f), charge);
                warningColor.SetColor(BaseColor, color);
                outline.SetPropertyBlock(warningColor);
                outline.widthMultiplier = .13f + charge * .09f;
            }
        }

        public void Configure(AttackDefinition hammer, AttackDefinition stomp, Transform body, Transform weaponArm,
            Transform otherArm, LineRenderer marker)
        {
            hammerfall = hammer;
            roadbreak = stomp;
            rig = body;
            hammerArm = weaponArm;
            freeArm = otherArm;
            outline = marker;
        }
    }
}

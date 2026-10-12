using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Gameplay;
using DiceFree.Progression;
using DiceFree.Quests;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Ironpine
{
    public enum WildlifeKind { Quillrat, GreyWolf, DarkWolf, AlphaWolf }

    // Lives on a real CombatActor: all attacks and kills use the ordinary DiceFree combat authority.
    // No hidden wolf summons, loot tables, or character-level scaling.
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(BasicAttack))]
    public sealed class IronpineWildlife : MonoBehaviour
    {
        [SerializeField] private WildlifeKind kind;
        [SerializeField] private Transform model;
        [SerializeField] private Vector3[] respawnSites = Array.Empty<Vector3>();
        [SerializeField] private Vector3[] huntingGrounds = Array.Empty<Vector3>();
        [SerializeField] private float habitatRadius = 9;
        private CombatActor actor;
        private BasicAttack attack;
        private Vector3 home, investigation;
        private Vector3 lockedImpact;
        private CombatActor target;
        private float nextBite, nextSpecial, nextHowl, nextWander, specialEnds, nextPath;
        private float deadUntil = float.PositiveInfinity, unseenSince = -1, frenzyUntil;
        private float leapStarted;
        private Vector3 leapOrigin;
        private bool airborne;
        private GameObject warning;
        private bool investigating, responding, specialPending, howling, pouncing, lunging;
        private int huntingIndex;
        private string familyId;
        public WildlifeKind Kind => kind;
        // Optional licensed creature visual; AI and collision stay unchanged.
        public void UseVisualModel(Transform visual) { if(visual!=null) model=visual; }
        public CombatActor Actor => actor;
        public bool IsBiting => attack != null && attack.Target != null && attack.State == "Wind-up";
        public void Configure(WildlifeKind species, Transform visual, Vector3[] sites, Vector3[] grounds = null)
        {
            kind = species; model = visual;
            respawnSites = sites ?? Array.Empty<Vector3>();
            huntingGrounds = grounds ?? Array.Empty<Vector3>();
        }
        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            home = transform.position;
            familyId = kind == WildlifeKind.Quillrat ? "ironpine.quillrat" :
                       kind == WildlifeKind.AlphaWolf ? "ironpine.alpha-wolf" : "ironpine.wolf";
        }
        private void Start()
        {
            GetComponent<DiceFree.AI.AggroBehaviour>().enabled = false;
            attack.enabled = kind != WildlifeKind.Quillrat;
            actor.Configure(1, kind == WildlifeKind.AlphaWolf ? .75f :
                kind == WildlifeKind.Quillrat ? .35f : .52f, familyId);
            // Playtest tuning: wildlife was dying before basic attack patterns
            // could be learned. Scale all three wolf types and quillrats 10x.
            float multiplier = kind == WildlifeKind.Quillrat ? 7.0f :
                kind == WildlifeKind.AlphaWolf ? 65.0f : 12.0f;
            actor.Stats.SetEncounterMaximumHp(actor.Stats.MaximumHp * multiplier);
            nextHowl = Time.time + 7f;
            nextSpecial = Time.time + 6f;
            nextWander = Time.time + UnityEngine.Random.Range(4f, 10f);
            if (kind == WildlifeKind.AlphaWolf && huntingGrounds.Length > 0)
                huntingIndex = ClosestGround(transform.position);
        }
        private void OnEnable()
        {
            if (actor == null) actor = GetComponent<CombatActor>();
            actor.Health.Died += OnDeath;
            actor.Health.Restored += OnRestore;
            actor.Health.Damaged += OnDamaged;
        }
        private void OnDisable()
        {
            if (actor == null) return;
            actor.Health.Died -= OnDeath;
            actor.Health.Restored -= OnRestore;
            actor.Health.Damaged -= OnDamaged;
            EndFrenzy();
        }
        private void OnDeath()
        {
            attack.Cancel();
            ClearWarning();
            target = null;
            specialPending = howling = pouncing = lunging = airborne = false;
            EndFrenzy();
            deadUntil = Time.time + (kind == WildlifeKind.AlphaWolf ? 600f : 90f);
        }
        private void OnRestore()
        {
            attack.ResetForSpawn();
            deadUntil = float.PositiveInfinity;
            target = null;
            investigating = responding = false;
            ClearWarning();
            specialPending = howling = pouncing = lunging = airborne = false;
            nextHowl = Time.time + 7f;
            nextSpecial = Time.time + 7f;
            nextWander = Time.time + UnityEngine.Random.Range(5f, 10f);
            unseenSince = -1;
            EndFrenzy();
        }
        private void OnDamaged(CombatActor source, DamageResult result)
        {
            if (source != null && actor.IsHostileTo(source) && source.Faction == 0 &&
                InsideTerritory(source.transform.position))
            {
                target = source;
                EnsureBleedReceiver(source);
            }
        }
        private bool IsWolf => kind != WildlifeKind.Quillrat;
        private bool IsAlpha => kind == WildlifeKind.AlphaWolf;
        private void Update()
        {
            if (!actor.Alive) { TryRespawn(); return; }
            if (frenzyUntil > 0 && Time.time >= frenzyUntil) EndFrenzy();
            Animate();
            if (specialPending)
            {
                if (!actor.CanAct) { CancelSpecial(); return; }
                if (Time.time >= specialEnds)
                {
                    if ((lunging || pouncing) && !airborne)
                    {
                        airborne = true;
                        leapOrigin = transform.position;
                        leapStarted = Time.time;
                        specialEnds = Time.time + (pouncing ? .48f : .36f);
                    }
                    else FinishSpecial();
                }
                return;
            }
            if (target != null && (!actor.IsHostileTo(target) || !InsideTerritory(target.transform.position)))
                Disengage();
            if (target != null && IsAlpha)
            {
                bool attackOpportunity = actor.HasSightOf(target) &&
                    Vector3.Distance(transform.position, target.transform.position) <= 3.5f;
                if (attackOpportunity || actor.HasSightOf(target)) unseenSince = -1;
                else if (unseenSince < 0) unseenSince = Time.time;
                else if (Time.time - unseenSince >= 6f) Disengage();
            }
            if (target == null)
            {
                Acquire();
                if (target == null) { Wander(); return; }
            }
            if (target == null || !actor.CanAct) return;
            EnsureBleedReceiver(target);
            if (IsWolf)
            {
                if (Time.time >= nextHowl && (IsAlpha || NearbyResponders() > 0))
                { StartHowl(); return; }
                float lungeRange = IsAlpha ? 9f : 6f;
                if (Time.time >= nextSpecial &&
                    Vector3.Distance(transform.position, target.transform.position) <= lungeRange &&
                    actor.HasSightOf(target)) { StartLeap(); return; }
                attack.Order(target);
            }
            else QuillratCombat();
        }
        private void Acquire()
        {
            CombatActor best = null;
            float bestDistance = IsAlpha ? 15f : 10f;
            foreach (var candidate in CombatActor.All)
            {
                if (candidate.Faction != 0 || !actor.IsHostileTo(candidate) ||
                    !InsideTerritory(candidate.transform.position) || !actor.HasSightOf(candidate)) continue;
                float distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < bestDistance) { best = candidate; bestDistance = distance; }
            }
            if (best != null)
            {
                target = best; investigating = false; unseenSince = -1;
                // A disabled BasicAttack still exposes its target to existing combat/party HUDs.
                if (kind == WildlifeKind.Quillrat) attack.Order(best);
            }
        }
        private bool InsideTerritory(Vector3 point)
        {
            if (IsAlpha && huntingGrounds.Length >= 2)
            {
                float min = float.PositiveInfinity;
                for (int i = 0; i < huntingGrounds.Length; ++i)
                {
                    Vector3 a = huntingGrounds[i], b = huntingGrounds[(i + 1) % huntingGrounds.Length];
                    Vector3 ab = b - a; ab.y = 0;
                    Vector3 ap = point - a; ap.y = 0;
                    float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Mathf.Max(.001f, ab.sqrMagnitude));
                    min = Mathf.Min(min, Vector3.Distance(new Vector3(point.x, 0, point.z),
                        new Vector3(a.x + ab.x * t, 0, a.z + ab.z * t)));
                }
                return min <= 45f;
            }
            Vector3 off = point - home; off.y = 0;
            return off.magnitude <= habitatRadius + 25f;
        }
        private void Disengage()
        {
            target = null; unseenSince = -1;
            attack.Cancel();
            investigation = home;
            responding = false;
            investigating = false;
            if (float.IsPositiveInfinity(nextHowl)) nextHowl = Time.time + 35f;
            nextWander = Time.time;
        }
        private int ClosestGround(Vector3 position)
        {
            int choice = 0; float distance = float.PositiveInfinity;
            for (int i = 0; i < huntingGrounds.Length; i++)
                if ((huntingGrounds[i] - position).sqrMagnitude < distance)
                { choice = i; distance = (huntingGrounds[i] - position).sqrMagnitude; }
            return choice;
        }
        private void Wander()
        {
            if (Time.time < nextWander || Time.time < nextPath) return;
            nextPath = Time.time + .5f;
            if (investigating)
            {
                if (Vector3.Distance(transform.position, investigation) <= 2f)
                { investigating = false; nextWander = Time.time + 6f; responding = false; return; }
                if (!actor.Motor.MoveTo(investigation)) investigating = false;
                return;
            }
            if (IsAlpha && huntingGrounds.Length >= 2)
            {
                if (Vector3.Distance(transform.position, huntingGrounds[huntingIndex]) < 2.5f)
                {
                    int offset = UnityEngine.Random.Range(1, huntingGrounds.Length);
                    huntingIndex = (huntingIndex + offset) % huntingGrounds.Length;
                    nextWander = Time.time + UnityEngine.Random.Range(15f, 30f);
                }
                else actor.Motor.MoveTo(huntingGrounds[huntingIndex]);
                return;
            }
            Vector3 delta = UnityEngine.Random.insideUnitSphere * habitatRadius;
            delta.y = 0;
            if (NavMesh.SamplePosition(home + delta, out var hit, 4f, NavMesh.AllAreas))
                actor.Motor.MoveTo(hit.position);
            nextWander = Time.time + UnityEngine.Random.Range(8f, 16f);
        }
        private int NearbyResponders()
        {
            if (responding) return 0;
            int count = 0;
            foreach (var other in FindObjectsByType<IronpineWildlife>(FindObjectsSortMode.None))
                if (other != this && other.IsWolf && other.actor != null && other.actor.Alive &&
                    other.target == null && Vector3.Distance(other.transform.position, transform.position) <= 25f)
                    count++;
            return count;
        }
        private void StartHowl()
        {
            attack.Cancel(); actor.Motor.Stop();
            howling = specialPending = true;
            GetComponent<IronpineCreaturePresentation>()?.PlayAction("Howl", 1.2f);
            specialEnds = Time.time + 1.2f;
        }
        private void StartLeap()
        {
            if (target == null) return;
            attack.Cancel(); actor.Motor.Stop();
            var position = target.transform.position;
            if (IsAlpha)
            {
                Vector3 velocity = Vector3.zero;
                var agent = target.GetComponent<NavMeshAgent>();
                if (agent != null) velocity = agent.velocity;
                position += Vector3.ClampMagnitude(velocity * .55f, 3f);
            }
            if (!NavMesh.SamplePosition(position, out var found, 2.5f, NavMesh.AllAreas)) return;
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(transform.position, found.position, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete) return;
            lockedImpact = found.position;
            ShowWarning(IsAlpha ? 2.5f : 1.25f);
            lunging = !IsAlpha; pouncing = IsAlpha; specialPending = true;
            GetComponent<IronpineCreaturePresentation>()?.PlayAction("Pounce", IsAlpha ? 1.4f : 1.1f);
            specialEnds = Time.time + (IsAlpha ? .9f : .7f);
            nextSpecial = Time.time + (IsAlpha ? 16f : kind == WildlifeKind.DarkWolf ? 10f : 11f);
        }
        private void FinishSpecial()
        {
            ClearWarning();
            specialPending = false;
            if (howling)
            {
                howling = false;
                if (IsAlpha)
                {
                    ApplyFrenzy(10f);
                    foreach (var other in FindObjectsByType<IronpineWildlife>(FindObjectsSortMode.None))
                        if (other != this && other.IsWolf && other.actor != null && other.actor.Alive &&
                            Vector3.Distance(other.transform.position, transform.position) <= 15f)
                            other.ApplyFrenzy(10f);
                }
                int count = 0;
                foreach (var other in FindObjectsByType<IronpineWildlife>(FindObjectsSortMode.None))
                {
                    if (count == 2) break;
                    if (other != this && other.IsWolf && other.actor != null && other.actor.Alive &&
                        other.target == null && !other.responding &&
                        Vector3.Distance(other.transform.position, transform.position) <= 25f)
                    { other.Investigate(transform.position); count++; }
                }
                nextHowl = Time.time + (IsAlpha ? 45f : 35f);
                Observe(IsAlpha ? "hunting-frenzy" : "alerting-howl");
            }
            if (pouncing || lunging)
            {
                float radius = pouncing ? 2.5f : 1.25f;
                if (actor.Motor.Teleport(lockedImpact))
                {
                    foreach (var victim in CombatActor.All)
                    {
                        if (!actor.IsHostileTo(victim)) continue;
                        Vector3 gap = victim.transform.position - lockedImpact; gap.y = 0;
                        if (gap.magnitude > radius + victim.Radius) continue;
                        float dealt = DamageResolver.Hit(actor, victim, actor.Stats.Definition.basicAttack,
                            null, null, pouncing ? 0f : 0f);
                        if (dealt > 0 && pouncing)
                        {
                            float bite = DamageResolver.Calculate(actor.Stats, victim.Stats,
                                actor.Stats.Definition.basicAttack).mitigated;
                            EnsureBleedReceiver(victim).Apply(actor, bite);
                        }
                    }
                }
                Observe(pouncing ? "crushing-pounce" : "lunge");
                lunging = pouncing = airborne = false;
                if (model != null) model.localPosition = Vector3.zero;
            }
        }
        private void CancelSpecial()
        {
            ClearWarning();
            specialPending = airborne = false;
            if (model != null) model.localPosition = Vector3.zero;
            if (howling) nextHowl = Time.time + 10f;
            howling = lunging = pouncing = false;
        }
        private void ClearWarning()
        {
            if (warning != null) Destroy(warning);
            warning = null;
        }
        private void ShowWarning(float radius)
        {
            ClearWarning();
            warning = new GameObject("Ironpine attack landing marker");
            warning.layer = 2; // IgnoreRaycast; marker never captures a click.
            warning.transform.position = lockedImpact + Vector3.up * .18f;
            var line = warning.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.startWidth = line.endWidth = .12f;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null) line.material = new Material(shader);
            line.startColor = line.endColor = new Color(1f, .18f, .15f, .93f);
            for (int i = 0; i < 48; i++)
            {
                float a = 2 * Mathf.PI * i / 48f;
                line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius));
            }
        }
        private void Investigate(Vector3 point)
        {
            investigation = point;
            investigating = responding = true;
            nextWander = Time.time;
            // Responders are not allowed to create a secondary reinforcement chain.
            nextHowl = float.PositiveInfinity;
        }
        private void ApplyFrenzy(float seconds)
        {
            frenzyUntil = Time.time + seconds;
            actor.Stats.SetBasicAttackDamagePercentContribution("ironpine.frenzy", 20f);
            actor.Effects?.ApplyMovementSpeedBuff("ironpine.frenzy", 15f, seconds);
        }
        private void EndFrenzy()
        {
            if (frenzyUntil <= 0) return;
            frenzyUntil = 0;
            actor?.Stats.RemoveBasicAttackDamagePercentContribution("ironpine.frenzy");
        }
        private void QuillratCombat()
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < 4f && Time.time >= nextSpecial)
            {
                var delta = transform.position - target.transform.position;
                delta.y = 0;
                if (delta.sqrMagnitude > .01f &&
                    NavMesh.SamplePosition(transform.position + delta.normalized *
                        UnityEngine.Random.Range(3f, 4f), out var hit, 2f, NavMesh.AllAreas))
                { actor.Motor.MoveTo(hit.position); nextSpecial = Time.time + 7f; return; }
            }
            if (distance > 15f || !actor.HasSightOf(target))
            { if (Time.time >= nextPath) { nextPath = Time.time + .4f; actor.Motor.MoveTo(target.transform.position); } return; }
            actor.Motor.Stop();
            if (Time.time < nextBite) return;
            nextBite = Time.time + 2.8f;
            GetComponent<IronpineCreaturePresentation>()?.PlayAction("QuillShot", .65f);
            IronpineQuill.Launch(actor, target, actor.Stats.Definition.basicAttack);
            Observe("homing-quill");
        }
        private IronpineBleed EnsureBleedReceiver(CombatActor victim)
        {
            var bleed = victim.GetComponent<IronpineBleed>();
            return bleed != null ? bleed : victim.gameObject.AddComponent<IronpineBleed>();
        }
        private void Observe(string ability)
        {
            foreach (var player in CombatActor.All)
            {
                if (player.Faction != 0 || !player.Alive ||
                    Vector3.Distance(player.transform.position, transform.position) > 25f ||
                    !player.HasSightOf(actor)) continue;
                player.GetComponent<CodexProgression>()?.RecordBossMechanic(familyId, ability);
            }
        }
        private void TryRespawn()
        {
            if (Time.time < deadUntil || respawnSites == null || respawnSites.Length == 0) return;
            int start = UnityEngine.Random.Range(0, respawnSites.Length);
            for (int n = 0; n < respawnSites.Length; n++)
            {
                Vector3 site = respawnSites[(start + n) % respawnSites.Length];
                if (!NavMesh.SamplePosition(site, out var hit, 2f, NavMesh.AllAreas)) continue;
                bool safe = true;
                foreach (var p in CombatActor.All)
                {
                    if (p.Faction != 0 || !p.Alive) continue;
                    if (Vector3.Distance(p.transform.position, hit.position) < 30f) { safe = false; break; }
                    var camera = Camera.main;
                    if (camera != null)
                    {
                        Vector3 screen = camera.WorldToViewportPoint(hit.position + Vector3.up);
                        if (screen.z > 0 && screen.x >= 0 && screen.x <= 1 && screen.y >= 0 && screen.y <= 1 &&
                            !Physics.Linecast(camera.transform.position, hit.position + Vector3.up, 1 << 9))
                        { safe = false; break; }
                    }
                }
                if (!safe || !actor.Motor.Teleport(hit.position)) continue;
                home = hit.position;
                actor.Stats.ResetTransientModifiers();
                actor.Health.Restore();
                return;
            }
        }
        private void Animate()
        {
            if (model == null) return;
            float phase = Time.time * (target == null ? 2f : 8f);
            float bob = Mathf.Sin(phase) * (target == null ? .025f : .06f);
            float crouch = specialPending && (lunging || pouncing) && !airborne ? -.13f : 0f;
            if (airborne)
            {
                float duration = pouncing ? .48f : .36f;
                float t = Mathf.Clamp01((Time.time - leapStarted) / duration);
                var travel = Vector3.Lerp(leapOrigin, lockedImpact, t) - transform.position;
                model.localPosition = travel + Vector3.up * (Mathf.Sin(t * Mathf.PI) * 1.45f);
            }
            else
                model.localPosition = new Vector3(0, bob + crouch, 0);
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = kind == WildlifeKind.AlphaWolf ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, kind == WildlifeKind.AlphaWolf ? 15f : 10f);
            if (huntingGrounds != null && huntingGrounds.Length > 1)
                for (int i = 0; i < huntingGrounds.Length; i++)
                    Gizmos.DrawLine(huntingGrounds[i], huntingGrounds[(i + 1) % huntingGrounds.Length]);
        }
    }

    [DisallowMultipleComponent]
    public sealed class IronpineBleed : MonoBehaviour
    {
        private struct Stack { public CombatActor source; public float expires, nextTick, tickDamage; }
        private readonly List<Stack> stacks = new();
        private CombatActor victim;
        private void Awake() => victim = GetComponent<CombatActor>();
        private void OnEnable() { victim.Health.Damaged += Damaged; victim.Health.Restored += Clear; }
        private void OnDisable() { victim.Health.Damaged -= Damaged; victim.Health.Restored -= Clear; }
        private void Clear() => stacks.Clear();
        private void Damaged(CombatActor source, DamageResult result)
        {
            var wildlife = source != null ? source.GetComponent<IronpineWildlife>() : null;
            if (wildlife != null && wildlife.Kind == WildlifeKind.DarkWolf &&
                wildlife.IsBiting && result.applied > 0 && UnityEngine.Random.value < .25f)
                Apply(source, result.mitigated);
        }
        public void Apply(CombatActor source, float mitigatedBite)
        {
            if (!victim.Alive || mitigatedBite <= 0f) return;
            var entry = new Stack { source = source, expires = Time.time + 5f,
                nextTick = Time.time + 1f, tickDamage = mitigatedBite * .08f };
            if (stacks.Count < 3) stacks.Add(entry);
            else
            {
                int replace = 0;
                for (int i = 1; i < stacks.Count; i++)
                    if (stacks[i].expires < stacks[replace].expires) replace = i;
                stacks[replace] = entry;
            }
        }
        private void Update()
        {
            if (!victim.Alive) { stacks.Clear(); return; }
            for (int i = stacks.Count - 1; i >= 0; i--)
            {
                var s = stacks[i];
                if (Time.time >= s.nextTick && s.nextTick <= s.expires + .001f)
                {
                    victim.Health.ApplyPacket(s.source, new DamageResult
                    { channel = DamageChannel.Physical, mitigated = s.tickDamage });
                    s.nextTick += 1f;
                }
                if (Time.time >= s.expires || !victim.Alive) stacks.RemoveAt(i);
                else stacks[i] = s;
            }
        }
    }

    // World-space movement with collision-checked travel; zero guaranteed-hit visual-only missiles.
    public sealed class IronpineQuill : MonoBehaviour
    {
        private CombatActor owner, target;
        private AttackDefinition definition;
        private Vector3 velocity;
        private float expires;
        public static void Launch(CombatActor source, CombatActor recipient, AttackDefinition attack)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Ironpine homing quill";
            Destroy(go.GetComponent<Collider>());
            go.transform.position = source.transform.position + Vector3.up * .85f;
            go.transform.localScale = new Vector3(.12f, .23f, .12f);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(.75f, .56f, .28f);
            go.GetComponent<Renderer>().material = material;
            var quill = go.AddComponent<IronpineQuill>();
            quill.owner = source; quill.target = recipient; quill.definition = attack;
            quill.velocity = (recipient.transform.position + Vector3.up * .85f - go.transform.position).normalized * 15f;
            quill.expires = Time.time + 8f;
        }
        private void Update()
        {
            if (target == null || !target.Alive || owner == null || Time.time >= expires)
            { Destroy(gameObject); return; }
            Vector3 desired = (target.transform.position + Vector3.up * .85f - transform.position).normalized;
            velocity = Vector3.RotateTowards(velocity.normalized, desired, 240f * Mathf.Deg2Rad * Time.deltaTime, 0f) * 15f;
            float length = velocity.magnitude * Time.deltaTime;
            Vector3 direction = velocity.normalized;
            var hits = Physics.SphereCastAll(transform.position, .10f, direction, length,
                ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(owner.transform)) continue;
                var hitActor = hit.collider.GetComponentInParent<CombatActor>();
                if (hitActor == target)
                {
                    if (owner.IsHostileTo(target))
                        DamageResolver.Hit(owner, target, definition);
                    Destroy(gameObject); return;
                }
                if (hitActor != null || hit.collider.gameObject.layer == 9)
                { Destroy(gameObject); return; }
            }
            transform.position += direction * length;
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0, 0);
        }
    }

    // Applies the existing local-party membership contract without awarding a second kill to the killer.
    public sealed class IronpinePartyCredit : MonoBehaviour
    {
        private void OnEnable() => DefeatEvents.Reported += Credit;
        private void OnDisable() => DefeatEvents.Reported -= Credit;
        private static void Credit(ActorDefeated defeat)
        {
            if (defeat.victim == null || defeat.victim.GetComponent<IronpineWildlife>() == null ||
                defeat.creditOwner == null || defeat.creditOwner.Faction != 0) return;
            var party = defeat.creditOwner.GetComponent<LocalPartyMember>();
            if (party == null) return;
            foreach (var member in CombatActor.All)
            {
                if (member == defeat.creditOwner || !member.Alive || member.Faction != 0 ||
                    Vector3.Distance(member.transform.position, defeat.position) > 30f ||
                    !party.SameParty(member)) continue;
                member.GetComponent<ExperienceProgression>()?.Grant(defeat.experience);
                member.GetComponent<CodexProgression>()?.RecordMonsterKill(defeat.contentId);
            }
        }
    }
}

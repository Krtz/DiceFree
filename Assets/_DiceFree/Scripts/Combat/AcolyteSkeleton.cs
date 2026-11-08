using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    public sealed class AcolyteSkeleton : MonoBehaviour
    {
        private static readonly Color Bone = new(.78f, .71f, .55f);
        private static readonly Color Iron = new(.27f, .29f, .36f);
        private static readonly Color Violet = new(.70f, .32f, 1);
        private ActorDefinition definition;
        private AttackDefinition weapon;
        private CombatActor actor;
        private BasicAttack attack;
        private Material bone, iron, violet, shadow;
        private Transform[] arms, legs;
        private LineRenderer ring;
        private float expires;
        private Vector3 previous;
        private System.Func<bool> autoRetaliate;

        public CombatActor Owner { get; private set; }
        public CombatActor Actor => actor;
        public float Remaining => Mathf.Max(0, expires - Time.time);

        public static AcolyteSkeleton Raise(CombatActor owner, int rank, System.Func<bool> autoRetaliate)
        {
            if (!owner.Motor.Ready || !SpawnPoint(owner, out var point)) return null;
            var attributes = owner.Stats.Attributes;
            var root = new GameObject("Lanternrest Skeleton") { layer = 10 };
            root.SetActive(false);
            root.transform.SetPositionAndRotation(point, owner.transform.rotation);
            var skeleton = root.AddComponent<AcolyteSkeleton>();
            skeleton.Owner = owner;
            skeleton.autoRetaliate = autoRetaliate;
            skeleton.weapon = ScriptableObject.CreateInstance<AttackDefinition>();
            skeleton.weapon.stableId = "attack.acolyte.skeleton";
            skeleton.weapon.displayName = "Rusty Blade";
            skeleton.weapon.channel = DamageChannel.Physical;
            skeleton.weapon.baseDamage = (3 + .18f * attributes.intelligence) * (1 + .06f * (rank - 1));
            skeleton.weapon.coefficient = 0;
            skeleton.weapon.reach = .85f;
            skeleton.weapon.windup = .32f;
            skeleton.weapon.interval = 1.1f;
            skeleton.definition = ScriptableObject.CreateInstance<ActorDefinition>();
            skeleton.definition.stableId = "summon.acolyte.skeleton";
            skeleton.definition.displayName = "Lanternrest Skeleton";
            skeleton.definition.familyId = "family.acolyte-summon";
            skeleton.definition.baseAttributes = default;
            skeleton.definition.baseHp = (55 + 2 * attributes.spirit) * (1 + .05f * (rank - 1));
            skeleton.definition.moveSpeed = owner.Stats.Definition.moveSpeed;
            skeleton.definition.basicAttack = skeleton.weapon;
            skeleton.definition.tuning = owner.Stats.Definition.tuning;
            skeleton.actor = root.AddComponent<CombatActor>();
            root.AddComponent<CombatStatusController>();
            root.GetComponent<ActorStats>().Configure(skeleton.definition);
            skeleton.actor.Configure(owner.Faction, .31f);
            root.GetComponent<NavMeshAgent>().enabled = false;
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0, .8f, 0);
            capsule.height = 1.6f;
            skeleton.attack = root.AddComponent<BasicAttack>();
            root.AddComponent<DefeatReporter>().SetCreditOwner(owner);
            skeleton.BuildVisual();
            skeleton.expires = Time.time + 15;
            skeleton.previous = point;
            root.SetActive(true);
            return skeleton;
        }

        private static bool SpawnPoint(CombatActor owner, out Vector3 point)
        {
            for (int i = 0; i < 8; i++)
            {
                var offset = Quaternion.Euler(0, i * 45, 0) * owner.transform.forward * 2;
                if (!NavMesh.SamplePosition(owner.transform.position + offset, out var hit, .75f, NavMesh.AllAreas) ||
                    NavMesh.Raycast(owner.transform.position, hit.position, out _, NavMesh.AllAreas)) continue;
                bool occupied = false;
                foreach (var candidate in CombatActor.All)
                    if (candidate.Alive && (candidate.transform.position - hit.position).sqrMagnitude <
                        (candidate.Radius + .7f) * (candidate.Radius + .7f)) { occupied = true; break; }
                if (!occupied) { point = hit.position; return true; }
            }
            point = default;
            return false;
        }

        private void OnEnable()
        {
            actor.Health.Damaged += Retaliate;
        }

        private void OnDisable()
        {
            actor.Health.Damaged -= Retaliate;
            attack.Cancel();
        }

        private void Retaliate(CombatActor source, DamageResult packet)
        {
            if (autoRetaliate?.Invoke() == true && actor.CanAct && attack.Target == null &&
                !actor.Motor.Travelling && actor.IsHostileTo(source))
                attack.Order(source);
        }

        public bool OrderAttack(CombatActor target) => actor.CanAct && actor.IsHostileTo(target) && attack.Order(target);

        public bool OrderMove(Vector3 point)
        {
            if (!actor.CanAct || !actor.Motor.MoveTo(point)) return false;
            if (attack.Target == null) return true;
            attack.Cancel();
            return actor.Motor.MoveTo(point);
        }

        public void Stop()
        {
            attack.Cancel();
            actor.Motor.Stop();
        }

        public void ShowSelected(bool value) => ring.enabled = value;

        public void Dismiss()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (Owner == null || !Owner.Alive || !actor.Alive || Remaining <= 0)
            {
                Dismiss();
                return;
            }
            bool moving = (transform.position - previous).sqrMagnitude > .00002f;
            previous = transform.position;
            float stride = moving ? Mathf.Sin(Time.time * 12) * 32 : 0;
            legs[0].localRotation = Quaternion.Euler(stride, 0, 0);
            legs[1].localRotation = Quaternion.Euler(-stride, 0, 0);
            arms[0].localRotation = Quaternion.Euler(-stride * .6f, 0, -10);
            float strike = attack.State == "Wind-up" ? -100 : attack.State == "Recovery" ? -32 : stride * .6f;
            arms[1].localRotation = Quaternion.Slerp(arms[1].localRotation, Quaternion.Euler(strike, 0, 14), Time.deltaTime * 14);
        }

        private void OnDestroy()
        {
            Destroy(definition);
            Destroy(weapon);
            Destroy(bone);
            Destroy(iron);
            Destroy(violet);
            Destroy(shadow);
        }

        private void BuildVisual()
        {
            bone = MakeMaterial("Lanternrest old bone", Bone);
            iron = MakeMaterial("Lanternrest blade and mail", Iron);
            violet = MakeMaterial("Lanternrest soul fire", Violet, true);
            shadow = MakeMaterial("Lanternrest black cavities", new Color(.07f, .055f, .10f));
            var body = Pivot(transform, "Skeleton", Vector3.zero);
            Part(body, "Pelvis", new Vector3(0,.77f,0), new Vector3(.35f,.22f,.22f), bone);
            Rod(body, "Spine", new Vector3(0,.82f,0), new Vector3(0,1.36f,0), .085f, bone);
            for (int i = 0; i < 5; i++)
            {
                float z = .93f + .065f * i;
                float width = .15f + .016f * i;
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 back = new(0,z,.0f), edge = new(width*side,z+.02f,.015f),
                        front = new(width*.72f*side,z-.035f,.12f);
                    Rod(body, "Rib", back, edge, .039f, bone);
                    Rod(body, "Rib", edge, front, .043f, bone);
                }
            }
            Part(body, "Skull", new Vector3(0,1.47f,.027f), new Vector3(.30f,.29f,.26f), bone);
            Part(body, "Jaw", new Vector3(0,1.35f,.077f), new Vector3(.215f,.11f,.17f), bone, PrimitiveType.Cube);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(body, "Eye socket", new Vector3(side*.065f,1.46f,.142f), new Vector3(.082f,.077f,.024f), shadow);
                Part(body, "Soul flame", new Vector3(side*.065f,1.46f,.160f), new Vector3(.047f,.041f,.028f), violet);
            }
            Part(body, "Nasal cavity", new Vector3(0,1.408f,.15f), new Vector3(.051f,.064f,.024f), shadow);
            Part(body, "Vigil charm", new Vector3(0,1.075f,.18f), new Vector3(.10f,.16f,.026f), violet);
            arms = new Transform[2]; legs = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                int side = i == 0 ? -1 : 1;
                var arm = arms[i] = Pivot(body, "Arm", new Vector3(.235f*side,1.255f,0));
                Part(arm, "Shoulder", Vector3.zero, new Vector3(.15f,.145f,.14f), i == 0 ? iron : bone);
                Rod(arm, "Upper arm", Vector3.zero, new Vector3(0,-.24f,0), .077f, bone);
                for (int j = -1; j <= 1; j += 2)
                    Rod(arm, "Forearm bone", new Vector3(j*.024f,-.235f,0), new Vector3(j*.024f,-.465f,.053f), .039f, bone);
                Part(arm, "Hand", new Vector3(0,-.49f,.054f), new Vector3(.11f,.13f,.08f), bone);
                var leg = legs[i] = Pivot(body, "Leg", new Vector3(.115f*side,.74f,0));
                Rod(leg, "Femur", Vector3.zero, new Vector3(0,-.35f,0), .084f, bone);
                Part(leg, "Knee", new Vector3(0,-.36f,.018f), new Vector3(.12f,.115f,.105f), bone);
                Rod(leg, "Shin", new Vector3(0,-.37f,0), new Vector3(0,-.67f,0), .07f, bone);
                Part(leg, "Foot", new Vector3(0,-.693f,.065f), new Vector3(.145f,.09f,.245f), bone);
            }
            var right = arms[1];
            Rod(right, "Sword grip", new Vector3(0,-.49f,0), new Vector3(0,-.49f,.18f), .072f, shadow);
            Part(right, "Crossguard", new Vector3(0,-.49f,.18f), new Vector3(.18f,.05f,.065f), iron, PrimitiveType.Cube);
            Part(right, "Grave iron blade", new Vector3(0,-.49f,.43f), new Vector3(.102f,.041f,.46f), iron, PrimitiveType.Cube);
            Part(right, "Blade edge", new Vector3(.043f,-.475f,.425f), new Vector3(.02f,.014f,.44f), bone, PrimitiveType.Cube);
            ring = body.gameObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.widthMultiplier = .035f;
            ring.sharedMaterial = violet;
            ring.positionCount = 40;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = 2 * Mathf.PI * i / ring.positionCount;
                ring.SetPosition(i,new Vector3(Mathf.Cos(angle)*.69f,.045f,Mathf.Sin(angle)*.69f));
            }
            ring.enabled = false;
        }

        private static Transform Pivot(Transform parent, string label, Vector3 position)
        {
            var node = new GameObject(label).transform;
            node.SetParent(parent, false);
            node.localPosition = position;
            return node;
        }

        private static Transform Part(Transform parent, string label, Vector3 position, Vector3 scale, Material material,
            PrimitiveType primitive = PrimitiveType.Sphere)
        {
            var node = GameObject.CreatePrimitive(primitive);
            node.name = label;
            node.transform.SetParent(parent,false);
            node.transform.localPosition = position;
            node.transform.localScale = scale;
            var collider = node.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            node.GetComponent<Renderer>().sharedMaterial = material;
            return node.transform;
        }

        private static void Rod(Transform parent, string label, Vector3 a, Vector3 b, float width, Material material)
        {
            var limb = Part(parent,label,(a+b)*.5f,new Vector3(width,Vector3.Distance(a,b)*.5f,width),material,PrimitiveType.Capsule);
            limb.localRotation = Quaternion.FromToRotation(Vector3.up,b-a);
        }

        private static Material MakeMaterial(string label, Color color, bool glowing = false)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = label, color = color };
            material.SetFloat("_Smoothness",.28f);
            if (glowing)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",color*1.7f);
            }
            return material;
        }
    }
}

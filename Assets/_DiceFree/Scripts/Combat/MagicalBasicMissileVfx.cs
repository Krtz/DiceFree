using UnityEngine;

namespace DiceFree.Combat
{
    /// <summary>
    /// Visual-only Arcane Spark missile for Magically Touched basic attacks.
    /// BasicAttack retains all targeting, damage, miss and wind-up decisions.
    /// Never adds a collider, light, attack packet or travel-time delay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MagicalBasicMissileVfx : MonoBehaviour
    {
        private static Material unlit;
        private Transform target;
        private Vector3 start, lastDestination;
        private Transform[] orbiters;
        private float started, duration;
        private TrailRenderer tail;

        public static void Launch(CombatActor source, CombatActor recipient, float seconds)
        {
            if (!Application.isPlaying || source == null || recipient == null) return;
            if (source.Stats.Definition == null ||
                source.Stats.Definition.stableId != "class.magically-touched-novice") return;
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Magically Touched - Arcane Spark missile";
            var collider = sphere.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            var bolt = sphere.AddComponent<MagicalBasicMissileVfx>();
            bolt.Configure(source, recipient, seconds);
        }

        private static Material Material()
        {
            if (unlit == null) unlit = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            if (unlit == null)
                unlit = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "Arcane Spark fallback" };
            return unlit;
        }

        private static void Tint(Renderer renderer, Color tint)
        {
            renderer.sharedMaterial = Material();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_BaseColor", tint);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void Configure(CombatActor source, CombatActor recipient, float windup) =>
            Initialize(source.transform.position + Vector3.up * 1.30f +
                source.transform.forward * .26f, recipient.transform,
                recipient.transform.position + Vector3.up * .96f, windup);

        /// <summary>Isolated editor preview of the same authored missile mesh and wisps.</summary>
        public static GameObject EditorPreview(Vector3 origin, Vector3 endpoint)
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Edit-mode preview only");
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Arcane Spark missile preview";
            DestroyImmediate(sphere.GetComponent<Collider>());
            var vfx = sphere.AddComponent<MagicalBasicMissileVfx>();
            vfx.Initialize(origin, null, endpoint, .42f);
            vfx.enabled = false;
            vfx.transform.position = Vector3.Lerp(origin, endpoint, .6f) + Vector3.up * .16f;
            for (int i = 0; i < vfx.orbiters.Length; i++)
            {
                float a = i * Mathf.PI * 2f / 3f;
                vfx.orbiters[i].localPosition = new Vector3(Mathf.Cos(a)*.92f,
                    Mathf.Sin(a)*.74f, Mathf.Sin(a)*.57f);
            }
            var trail = new GameObject("Arcane trail sample").AddComponent<LineRenderer>();
            trail.transform.SetParent(sphere.transform, true);
            trail.useWorldSpace = true;
            trail.positionCount = 9;
            trail.widthMultiplier = .075f;
            trail.sharedMaterial = Material();
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", new Color(.56f,.39f,1f));
            trail.SetPropertyBlock(block);
            for(int i=0;i<9;i++)
            {
                float t=i/8f;
                trail.SetPosition(i,Vector3.Lerp(origin,vfx.transform.position,t));
            }
            return sphere;
        }

        private void Initialize(Vector3 origin, Transform tracked, Vector3 endpoint, float windup)
        {
            start = origin;
            target = tracked;
            lastDestination = endpoint;
            transform.position = start;
            transform.localScale = Vector3.one * .19f;
            started = Time.time;
            duration = Mathf.Clamp(windup, .18f, .68f);
            Tint(GetComponent<Renderer>(), new Color(.54f, .76f, 1f, .98f));

            tail = gameObject.AddComponent<TrailRenderer>();
            tail.time = .29f;
            tail.minVertexDistance = .015f;
            tail.startWidth = .17f;
            tail.endWidth = .008f;
            tail.sharedMaterial = Material();
            tail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tail.receiveShadows = false;
            tail.colorGradient = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(new Color(.58f, .76f, 1f), 0),
                    new GradientColorKey(new Color(.50f, .31f, 1f), 1)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(.85f, 0),
                    new GradientAlphaKey(0, 1)
                }
            };
            orbiters = new Transform[3];
            for (int i = 0; i < orbiters.Length; i++)
            {
                var mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mote.name = "Arcane Spark orbiting wisp";
                var moteCollider = mote.GetComponent<Collider>();
                moteCollider.enabled = false;
                if (Application.isPlaying) Destroy(moteCollider);
                else DestroyImmediate(moteCollider);
                mote.transform.SetParent(transform, false);
                // Parent is 0.19 units, so these sizes are relative to the missile.
                mote.transform.localScale = Vector3.one * .38f;
                Tint(mote.GetComponent<Renderer>(), i == 0
                    ? new Color(.91f,.86f,1f) : new Color(.58f,.38f,1f));
                orbiters[i] = mote.transform;
            }
        }

        private void Update()
        {
            if (target != null) lastDestination = target.position + Vector3.up * .96f;
            float t = Mathf.Clamp01((Time.time - started) / duration);
            float eased = t * t * (3f - 2f * t);
            var center = Vector3.Lerp(start, lastDestination, eased);
            center.y += Mathf.Sin(t * Mathf.PI) * .18f;
            transform.position = center;
            for (int i = 0; i < orbiters.Length; i++)
            {
                float a = Time.time * 20f + i * Mathf.PI * 2f / 3f;
                orbiters[i].localPosition = new Vector3(
                    Mathf.Cos(a) * .92f,
                    Mathf.Sin(a) * .74f,
                    Mathf.Sin(a * .73f) * .57f);
            }
            if (t < 1f) return;
            EarlySkillVfx.Play(EarlySkillVfx.Cue.MagicalAutoImpact, lastDestination - Vector3.up * .96f);
            Destroy(gameObject);
        }
    }
}

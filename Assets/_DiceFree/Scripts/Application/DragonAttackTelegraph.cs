using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.Dungeons
{
    internal readonly struct DragonDangerZone
    {
        internal readonly Vector3 Center, Forward;
        internal readonly float Radius, HalfAngle;

        internal DragonDangerZone(Vector3 center, Vector3 forward, float radius, float halfAngle = 180)
        {
            Center = center;
            Forward = forward;
            Radius = radius;
            HalfAngle = halfAngle;
        }

        internal bool Contains(Vector3 point)
        {
            var offset = point - Center;
            if (Mathf.Abs(offset.y) > 2.5f) return false;
            offset.y = 0;
            if (offset.sqrMagnitude > Radius * Radius) return false;
            return HalfAngle >= 180 || offset.sqrMagnitude < .01f ||
                Vector3.Dot(offset.normalized, Forward) >= Mathf.Cos(HalfAngle * Mathf.Deg2Rad);
        }
    }

    internal sealed class DragonAttackTelegraph : MonoBehaviour
    {
        private Material fill, edge;
        private Mesh shape;
        private LineRenderer outline;
        private Color tint;
        private float impactTime = float.PositiveInfinity;
        private const float ImpactDuration = .45f;

        internal static DragonAttackTelegraph Create(Transform parent, DragonDangerZone zone, Color color, Material source)
        {
            var marker = new GameObject("Ancient Road Dragon attack warning");
            marker.layer = 2;
            marker.transform.SetParent(parent, true);
            marker.transform.SetPositionAndRotation(zone.Center + Vector3.up * .075f, Quaternion.LookRotation(zone.Forward));
            var effect = marker.AddComponent<DragonAttackTelegraph>();
            effect.Initialize(zone, color, source);
            return effect;
        }

        private void Initialize(DragonDangerZone zone, Color color, Material source)
        {
            const int steps = 48;
            bool circle = zone.HalfAngle >= 180;
            tint = color;
            var vertices = new Vector3[steps + 2];
            var triangles = new int[steps * 3];
            for (int i = 0; i <= steps; i++)
            {
                float angle = Mathf.Lerp(-zone.HalfAngle, zone.HalfAngle, i / (float)steps) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * zone.Radius;
                if (i == steps) continue;
                int triangle = i * 3;
                triangles[triangle + 1] = i + 1;
                triangles[triangle + 2] = i + 2;
            }
            shape = new Mesh { name = "Ritual danger footprint", vertices = vertices, triangles = triangles };
            shape.RecalculateBounds();
            fill = new Material(source);
            edge = new Material(source);
            gameObject.AddComponent<MeshFilter>().sharedMesh = shape;
            var surface = gameObject.AddComponent<MeshRenderer>();
            surface.shadowCastingMode = ShadowCastingMode.Off;
            surface.sharedMaterial = fill;

            var rim = new GameObject("Attack outline");
            rim.layer = 2;
            rim.transform.SetParent(transform, false);
            rim.transform.localPosition = Vector3.up * .016f;
            rim.transform.localRotation = Quaternion.Euler(90, 0, 0);
            outline = rim.AddComponent<LineRenderer>();
            outline.shadowCastingMode = ShadowCastingMode.Off;
            outline.useWorldSpace = false;
            outline.loop = true;
            outline.alignment = LineAlignment.TransformZ;
            outline.sharedMaterial = edge;
            outline.positionCount = circle ? steps : steps + 2;
            for (int i = 0; i < outline.positionCount; i++)
            {
                var point = vertices[circle ? i + 1 : i];
                outline.SetPosition(i, new Vector3(point.x, point.z, 0));
            }
            Windup(0);
        }

        internal void Windup(float charge)
        {
            SetColor(fill, tint, Mathf.Lerp(.15f, .34f, charge));
            SetColor(edge, tint, .95f);
            outline.widthMultiplier = Mathf.Lerp(.13f, .30f, charge);
        }

        internal void Impact()
        {
            impactTime = Time.time;
            SetColor(fill, Color.Lerp(tint, Color.white, .28f), .78f);
            outline.widthMultiplier = .47f;
            Destroy(gameObject, ImpactDuration);
        }

        private void Update()
        {
            if (float.IsPositiveInfinity(impactTime)) return;
            float remaining = 1 - Mathf.Clamp01((Time.time - impactTime) / ImpactDuration);
            SetColor(fill, Color.Lerp(tint, Color.white, .28f), remaining * .78f);
            SetColor(edge, tint, remaining);
        }

        private static void SetColor(Material material, Color color, float alpha)
        {
            color.a = alpha;
            material.SetColor("_BaseColor", color);
        }

        internal static Material CreateImpactMaterial()
        {
            var material = new Material(SlimeDungeonVisuals.TelegraphMaterial) { name = "Dragon breath and dust" };
            material.SetTexture("_BaseMap", Resources.Load<Texture2D>("EarlySkillVfx/SoftGlow"));
            material.SetFloat("_Surface", 1);
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent + 3;
            return material;
        }

        internal static void EmitImpact(Transform parent, Vector3 point, Vector3 direction, Material art, bool breath)
        {
            var go = new GameObject(breath ? "Verdant breath plume" : "Stonefall dust");
            go.SetActive(false);
            go.layer = 2;
            go.transform.SetParent(parent, true);
            go.transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction,
                breath ? Vector3.up : Vector3.forward));
            var particles = go.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = .36f;
            main.startLifetime = breath ? new ParticleSystem.MinMaxCurve(.34f, .72f) : new ParticleSystem.MinMaxCurve(.24f, .53f);
            main.startSpeed = breath ? new ParticleSystem.MinMaxCurve(12, 27) : new ParticleSystem.MinMaxCurve(3, 9);
            main.startSize = breath ? new ParticleSystem.MinMaxCurve(.48f, 1.36f) : new ParticleSystem.MinMaxCurve(.28f, 1.06f);
            main.startColor = breath ? new Color(.68f, 1, .32f, .82f) : new Color(.84f, .52f, .84f, .72f);
            main.gravityModifier = breath ? .16f : .77f;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = particles.emission;
            emission.rateOverTime = breath ? 270 : 0;
            if (!breath) emission.SetBursts(new[] { new ParticleSystem.Burst(0, 28) });
            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(.12f, 0), new GradientAlphaKey(.9f, .19f), new GradientAlphaKey(0, 1) });
            fade.color = new ParticleSystem.MinMaxGradient(gradient);
            var cone = particles.shape;
            cone.shapeType = ParticleSystemShapeType.Cone;
            cone.angle = breath ? 21 : 64;
            cone.radius = breath ? .19f : 1.5f;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = art;
            renderer.renderMode = breath ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            renderer.lengthScale = breath ? 1.9f : 1;
            go.SetActive(true);
            particles.Play();
        }

        private void OnDestroy()
        {
            Destroy(fill);
            Destroy(edge);
            Destroy(shape);
        }
    }
}


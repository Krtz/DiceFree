using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.World
{
    public enum LandmarkElement { Fire, Earth, Lightning, Ice }

    /// <summary>Visual-only cloth movement and restrained elemental light variation.</summary>
    public sealed class ElementalLandmarkAmbience : MonoBehaviour
    {
        [SerializeField] private LandmarkElement element;
        [SerializeField] private MeshFilter cloth;
        [SerializeField] private Renderer[] emissiveDetails;
        private Mesh originalCloth, animatedCloth, sparkMesh;
        private Vector3[] restingVertices, movingVertices;
        private MaterialPropertyBlock[] originals, changes;
        private Color[] emission;
        private GameObject sparks;

        public void Configure(LandmarkElement value, MeshFilter banner, params Renderer[] details)
        {
            Cleanup();
            element = value;
            cloth = banner;
            emissiveDetails = details;
            if (Application.isPlaying && isActiveAndEnabled) Initialize();
        }

        private void OnEnable()
        {
            if (Application.isPlaying) Initialize();
        }

        private void Initialize()
        {
            if (animatedCloth != null || cloth == null || cloth.sharedMesh == null) return;
            originalCloth = cloth.sharedMesh;
            animatedCloth = Instantiate(originalCloth);
            animatedCloth.name = originalCloth.name + " (runtime wind)";
            animatedCloth.MarkDynamic();
            restingVertices = originalCloth.vertices;
            movingVertices = new Vector3[restingVertices.Length];
            var bounds = originalCloth.bounds;
            bounds.Expand(.3f);
            animatedCloth.bounds = bounds;
            cloth.sharedMesh = animatedCloth;
            emissiveDetails ??= System.Array.Empty<Renderer>();
            originals = new MaterialPropertyBlock[emissiveDetails.Length];
            changes = new MaterialPropertyBlock[emissiveDetails.Length];
            emission = new Color[emissiveDetails.Length];
            for (int i = 0; i < emissiveDetails.Length; i++)
            {
                originals[i] = new MaterialPropertyBlock();
                changes[i] = new MaterialPropertyBlock();
                var renderer = emissiveDetails[i];
                if (renderer == null) continue;
                renderer.GetPropertyBlock(originals[i]);
                renderer.GetPropertyBlock(changes[i]);
                emission[i] = renderer.sharedMaterial.GetColor("_EmissionColor");
            }
            if (element == LandmarkElement.Fire && emissiveDetails.Length > 0 && emissiveDetails[0] != null)
                MakeEmbers(emissiveDetails[0].sharedMaterial);
        }

        private void Update()
        {
            if (animatedCloth == null) return;
            float time = Time.time;
            for (int i = 0; i < restingVertices.Length; i++)
            {
                var vertex = restingVertices[i];
                // Both standards have the same free-edge width. Pin their pole edges.
                float pole = Mathf.Abs(vertex.x + 4.45f) < Mathf.Abs(vertex.x - 3.55f) ? -4.45f : 3.55f;
                float free = Mathf.Clamp01((vertex.x - pole - .12f) / 1.24f);
                vertex.z += free * (.08f * Mathf.Sin(time * 1.7f + vertex.x * 2.1f + vertex.y)
                    + .025f * Mathf.Sin(time * 3.1f + vertex.y * 3f));
                movingVertices[i] = vertex;
            }
            animatedCloth.vertices = movingVertices;
            float glow = element switch
            {
                LandmarkElement.Fire => .95f + .10f * Mathf.Sin(time * 5.3f) + .05f * Mathf.Sin(time * 13.7f),
                LandmarkElement.Lightning => 1f + .15f * Mathf.Pow(.5f + .5f * Mathf.Sin(time * 1.7f), 8),
                LandmarkElement.Ice => 1f + .035f * Mathf.Sin(time * .7f),
                _ => 1f
            };
            for (int i = 0; i < emissiveDetails.Length; i++)
                if (emissiveDetails[i] != null)
                {
                    changes[i].SetColor("_EmissionColor", emission[i] * glow);
                    emissiveDetails[i].SetPropertyBlock(changes[i]);
                }
        }

        private void MakeEmbers(Material material)
        {
            sparks = new GameObject("Contained altar embers");
            sparks.transform.SetParent(transform, false);
            sparks.transform.localPosition = new Vector3(0, 1.6f, -.7f);
            var system = sparks.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.maxParticles = 12;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.1f, .25f);
            main.startSize = new ParticleSystem.MinMaxCurve(.055f, .10f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startRotation3D = true;
            main.startRotationY = new ParticleSystem.MinMaxCurve(0, 6.28f);
            main.gravityModifier = -.025f;
            var emit = system.emission;
            emit.rateOverTime = 3f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .28f;
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.y = .45f;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));
            sparkMesh = new Mesh { name = "Runtime ember triangle" };
            sparkMesh.vertices = new[] { new Vector3(-.4f, 0, 0), new Vector3(.4f, 0, 0), new Vector3(0, .9f, 0) };
            sparkMesh.triangles = new[] { 0, 1, 2, 2, 1, 0 };
            // Shared front/back vertices cancel if normals are recalculated.
            sparkMesh.normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward };
            sparkMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
            sparkMesh.RecalculateBounds();
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = sparkMesh;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            system.Play();
        }

        private void OnDisable() => Cleanup();
        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            if (cloth != null && animatedCloth != null && cloth.sharedMesh == animatedCloth)
                cloth.sharedMesh = originalCloth;
            if (originals != null && emissiveDetails != null)
                for (int i = 0; i < emissiveDetails.Length && i < originals.Length; i++)
                    if (emissiveDetails[i] != null) emissiveDetails[i].SetPropertyBlock(originals[i]);
            if (animatedCloth != null) Destroy(animatedCloth);
            if (sparks != null) Destroy(sparks);
            if (sparkMesh != null) Destroy(sparkMesh);
            animatedCloth = null;
            originalCloth = null;
            sparks = null;
            sparkMesh = null;
            originals = null;
        }
    }
}

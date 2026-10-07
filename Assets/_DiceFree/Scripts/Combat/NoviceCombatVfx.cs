using System.Collections;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent]
    public sealed class NoviceCombatVfx : MonoBehaviour
    {
        private static readonly int AttackId = Animator.StringToHash("Attack");
        private static Material glowMaterial;

        public void PlayMagicSand(CombatActor source, CombatActor target)
        {
            if (source == null || target == null) return;
            StartCoroutine(MagicSandRoutine(source, target));
        }

        public void PlayAttackSpeedGlow(CombatActor target, float duration)
        {
            if (target == null) return;
            Animator animator = ResolveAnimator(target);
            Transform left = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.LeftHand)
                : null;
            Transform right = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightHand)
                : null;

            if (left != null) StartCoroutine(HandGlowRoutine(left, duration));
            if (right != null) StartCoroutine(HandGlowRoutine(right, duration));
            if (left == null && right == null)
                StartCoroutine(HandGlowRoutine(target.transform, duration, new Vector3(0, 1f, 0)));
        }

        public void PlayHealLight(CombatActor target)
        {
            if (target == null) return;
            StartCoroutine(HealLightRoutine(target));
        }

        public void PlayMeleeStun(CombatActor source, CombatActor target)
        {
            if (source == null || target == null) return;

            Animator animator = ResolveAnimator(source);
            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.ResetTrigger(AttackId);
                animator.SetTrigger(AttackId);
            }

            StartCoroutine(MeleeImpactRoutine(target));
        }

        private IEnumerator MagicSandRoutine(CombatActor source, CombatActor target)
        {
            const int grainCount = 10;
            Vector3 start = source.transform.position + Vector3.up * 1.15f +
                            source.transform.forward * 0.25f;
            float duration = 0.30f;
            float started = Time.time;

            var grains = new GameObject[grainCount];
            var offsets = new Vector3[grainCount];
            var phase = new float[grainCount];

            for (int i = 0; i < grainCount; i++)
            {
                offsets[i] = new Vector3(
                    UnityEngine.Random.Range(-0.10f, 0.10f),
                    UnityEngine.Random.Range(-0.07f, 0.07f),
                    UnityEngine.Random.Range(-0.10f, 0.10f));
                phase[i] = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float scale = UnityEngine.Random.Range(0.032f, 0.055f);
                float warmth = UnityEngine.Random.Range(0f, 0.08f);
                grains[i] = CreateGlowSphere(
                    "Magic Sand grain",
                    start + offsets[i],
                    scale,
                    new Color(0.90f + warmth, 0.60f + warmth, 0.19f, 1f));
            }

            while (Time.time - started < duration && target != null)
            {
                float t = Mathf.Clamp01((Time.time - started) / duration);
                Vector3 end = target.transform.position + Vector3.up * 0.85f;
                Vector3 center = Vector3.Lerp(start, end, t);
                center.y += Mathf.Sin(t * Mathf.PI) * 0.20f;

                for (int i = 0; i < grainCount; i++)
                {
                    if (grains[i] == null) continue;
                    float flutter = Mathf.Sin(t * 20f + phase[i]) * 0.018f;
                    Vector3 clump = offsets[i] + new Vector3(flutter, flutter * 0.5f, -flutter);
                    grains[i].transform.position = center + clump;
                }

                yield return null;
            }

            Vector3 hit = target != null
                ? target.transform.position + Vector3.up * 0.85f
                : start;

            for (int i = 0; i < grainCount; i++)
                if (grains[i] != null) Destroy(grains[i]);

            StartCoroutine(PulseRoutine(
                hit,
                new Color(0.95f, 0.72f, 0.28f, 1f),
                0.16f,
                0.18f));
        }

        private IEnumerator MeleeImpactRoutine(CombatActor target)
        {
            yield return new WaitForSeconds(0.12f);
            if (target != null)
                yield return PulseRoutine(
                    target.transform.position + Vector3.up * 0.95f,
                    new Color(1f, 0.92f, 0.72f, 1f),
                    0.16f,
                    0.14f);
        }

        private IEnumerator HealLightRoutine(CombatActor target)
        {
            var lightObject = CreateGlowSphere(
                "Novice heal light",
                target.transform.position + Vector3.up * 1.55f,
                0.09f,
                new Color(1f, 0.95f, 0.72f, 1f));

            var point = lightObject.AddComponent<Light>();
            point.type = LightType.Point;
            point.range = 1.4f;
            point.intensity = 1.1f;
            point.color = new Color(1f, 0.95f, 0.72f);

            float duration = 0.65f;
            float started = Time.time;
            while (lightObject != null && target != null && Time.time - started < duration)
            {
                float t = Mathf.Clamp01((Time.time - started) / duration);
                lightObject.transform.position =
                    target.transform.position + Vector3.up * (1.55f + t * 0.20f);
                float pulse = Mathf.Sin(t * Mathf.PI);
                lightObject.transform.localScale =
                    Vector3.one * Mathf.Lerp(0.04f, 0.12f, pulse);
                point.intensity = 1.1f * (1f - t);
                yield return null;
            }

            if (lightObject != null) Destroy(lightObject);
        }

        private IEnumerator HandGlowRoutine(
            Transform parent,
            float duration,
            Vector3 localOffset = default)
        {
            var glow = CreateGlowSphere(
                "Novice attack speed hand glow",
                parent.position,
                0.07f,
                new Color(0.95f, 0.82f, 0.32f, 0.85f));
            glow.transform.SetParent(parent, false);
            glow.transform.localPosition = localOffset;

            float started = Time.time;
            duration = Mathf.Max(0.2f, duration);
            while (glow != null && parent != null && Time.time - started < duration)
            {
                float t = Time.time - started;
                float pulse = 0.055f + (Mathf.Sin(t * 12f) * 0.5f + 0.5f) * 0.035f;
                glow.transform.localScale = Vector3.one * pulse;
                glow.transform.localPosition =
                    localOffset + new Vector3(0, Mathf.Sin(t * 9f) * 0.018f, 0);
                yield return null;
            }

            if (glow != null) Destroy(glow);
        }

        private IEnumerator PulseRoutine(
            Vector3 position,
            Color color,
            float maximumScale,
            float duration)
        {
            var pulse = CreateGlowSphere("Novice impact pulse", position, 0.03f, color);
            float started = Time.time;

            while (pulse != null && Time.time - started < duration)
            {
                float t = Mathf.Clamp01((Time.time - started) / duration);
                pulse.transform.localScale = Vector3.one * Mathf.Lerp(0.03f, maximumScale, t);
                yield return null;
            }

            if (pulse != null) Destroy(pulse);
        }

        private static GameObject CreateGlowSphere(
            string name,
            Vector3 position,
            float scale,
            Color color)
        {
            var value = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            value.name = name;
            value.transform.position = position;
            value.transform.localScale = Vector3.one * scale;
            var collider = value.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            var renderer = value.GetComponent<Renderer>();
            renderer.sharedMaterial = GlowMaterial();
            renderer.material.color = color;
            return value;
        }

        private static Animator ResolveAnimator(CombatActor target) =>
            target == null ? null : target.GetComponentInChildren<Animator>(true);

        private static Material GlowMaterial()
        {
            if (glowMaterial != null) return glowMaterial;
            Shader shader =
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color");
            glowMaterial = new Material(shader) { name = "Runtime Novice Glow" };
            return glowMaterial;
        }
    }
}

using System.Collections;
using UnityEngine;

namespace DiceFree.Skills
{
    public sealed class RangerVfx : MonoBehaviour
    {
        public static readonly Color ArrowTint = new(.96f, .86f, .49f);
        public static readonly Color FireTint = new(1f, .37f, .10f);
        private static readonly Color Leaf = new(.51f, .86f, .47f);
        private static Material stroke;

        private static LineRenderer Line(Transform parent, string name, float width, Color color, int count)
        {
            if (stroke == null)
            {
                var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
                stroke = new Material(shader) { name = "Ranger trail stroke" };
            }
            var line = new GameObject(name).AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false);
            line.useWorldSpace = false;
            line.sharedMaterial = stroke;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.positionCount = count;
            line.numCapVertices = 2;
            return line;
        }

        private static RangerVfx Effect(string name, Vector3 point, float duration, Transform parent = null)
        {
            var effect = new GameObject(name).AddComponent<RangerVfx>();
            effect.transform.SetParent(parent, false);
            effect.transform.position = point;
            Destroy(effect.gameObject, duration);
            return effect;
        }

        public static GameObject TrackArrow(Vector3 start, Vector3 end, Color tint, float lifetime, float scale = 1f)
        {
            var effect = Effect("Ranger flying arrow", start, lifetime + .08f);
            var shaft = Line(effect.transform, "Yew shaft", .042f * scale, tint, 2);
            shaft.SetPosition(0, Vector3.back * .70f * scale);
            shaft.SetPosition(1, Vector3.forward * .04f * scale);
            var head = Line(effect.transform, "Arrowhead", .046f * scale, Color.Lerp(tint, Color.white, .5f), 3);
            head.SetPositions(new[] { new Vector3(-.10f, 0, -.13f) * scale, Vector3.forward * .05f * scale,
                new Vector3(.10f, 0, -.13f) * scale });
            var feathers = Line(effect.transform, "Feathers", .073f * scale, Color.Lerp(tint, Color.white, .26f), 3);
            feathers.SetPositions(new[] { new Vector3(-.095f, 0, -.76f) * scale, Vector3.back * .51f * scale,
                new Vector3(.095f, 0, -.76f) * scale });
            PlaceArrow(effect.gameObject, start, end, 0);
            return effect.gameObject;
        }

        public static void PlaceArrow(GameObject arrow, Vector3 start, Vector3 end, float fraction)
        {
            if (arrow == null) return;
            Vector3 delta = end - start;
            float arc = Mathf.Abs(delta.y) > 7 ? 0 : .45f;
            arrow.transform.position = Vector3.Lerp(start, end, fraction) + Vector3.up * (4 * arc * fraction * (1 - fraction));
            var tangent = delta + Vector3.up * (4 * arc * (1 - 2 * fraction));
            if (tangent.sqrMagnitude > .001f) arrow.transform.rotation = Quaternion.LookRotation(tangent);
        }

        public static void Arrow(Vector3 start, Vector3 end, float time, float scale, Color tint)
        {
            var arrow = TrackArrow(start, end, tint, time, scale);
            var animation = arrow.GetComponent<RangerVfx>();
            animation.StartCoroutine(animation.StaticFlight(start, end, time));
        }

        private IEnumerator StaticFlight(Vector3 start, Vector3 end, float time)
        {
            for (float elapsed = 0; elapsed < time; elapsed += Time.deltaTime)
            {
                PlaceArrow(gameObject, start, end, elapsed / time);
                yield return null;
            }
            PlaceArrow(gameObject, start, end, 1);
        }

        public static void Rain(Vector3 point, float radius, int wave)
        {
            if (wave == 0) Combat.EarlySkillVfx.PlayPriestPulse("Ranger rain boundary", point, radius, Leaf);
            for (int i = 0; i < 10; i++)
            {
                float angle = (i + wave * .47f) * 2.39996f;
                float distance = radius * Mathf.Sqrt((i + .3f) / 10f);
                Vector3 tip = point + new Vector3(Mathf.Cos(angle) * distance, .14f, Mathf.Sin(angle) * distance);
                Arrow(tip + new Vector3(1.05f, 7f + .22f * i, -.8f), tip, .38f + i * .011f, .76f, ArrowTint);
            }
        }

        public static void Explosion(Vector3 point) =>
            Combat.EarlySkillVfx.PlayPriestPulse("Gunpowder burst", point, RangerSkillCaster.BlastRadius, FireTint);

        public static void Step(Vector3 start, Vector3 end)
        {
            var effect = Effect("Evasive Step trail", start + Vector3.up * .24f, .36f);
            var line = Line(effect.transform, "Leaf afterimage", .115f, Leaf, 3);
            line.SetPositions(new[] { Vector3.zero, (end - start) * .47f + Vector3.up * .34f, end - start });
            Combat.EarlySkillVfx.PlayPriestPulse("Evasive Step landing", end, .7f, Leaf);
        }

        public static GameObject Mark(Transform target, float radius)
        {
            var effect = Effect("Hunter's Mark", target.position + Vector3.up * Mathf.Max(1.55f, radius * 2.5f), 8.05f, target);
            var line = Line(effect.transform, "Tracking chevron", .045f, ArrowTint, 4);
            line.SetPositions(new[] { new Vector3(-.27f, .22f, 0), new Vector3(0, -.05f, 0),
                new Vector3(.27f, .22f, 0), new Vector3(0, .12f, 0) });
            effect.StartCoroutine(effect.FaceCamera());
            return effect.gameObject;
        }

        private IEnumerator FaceCamera()
        {
            while (true)
            {
                if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
                yield return null;
            }
        }

        public static void Hawk(Vector3 owner, Vector3 target, float time)
        {
            Vector3 start = Vector3.Lerp(owner, target, .47f) + Vector3.up * 5.3f;
            var effect = Effect("Hawk dive silhouette", start, time + .24f);
            var wing = Line(effect.transform, "Hawk wings", .145f, new Color(.85f, .64f, .34f), 7);
            effect.StartCoroutine(effect.HawkFlight(wing, start, target + Vector3.up, time));
        }

        private IEnumerator HawkFlight(LineRenderer wing, Vector3 start, Vector3 end, float time)
        {
            for (float elapsed = 0; elapsed < time + .2f; elapsed += Time.deltaTime)
            {
                float t = elapsed / time;
                transform.position = t < 1 ? Vector3.Lerp(start, end, t * t) :
                    end + new Vector3(.3f, 3.1f, 1.1f) * (t - 1);
                Vector3 forward = t < 1 ? end - start : new Vector3(.3f, 3.1f, 1.1f);
                transform.rotation = Quaternion.LookRotation(forward);
                float flap = Mathf.Sin(elapsed * 24f) * .28f;
                wing.SetPositions(new[] { new Vector3(-.84f, flap, -.36f), new Vector3(-.57f, flap * .7f, .08f),
                    new Vector3(-.28f, .08f, .21f), Vector3.forward * .35f, new Vector3(.28f, .08f, .21f),
                    new Vector3(.57f, flap * .7f, .08f), new Vector3(.84f, flap, -.36f) });
                yield return null;
            }
        }
    }
}
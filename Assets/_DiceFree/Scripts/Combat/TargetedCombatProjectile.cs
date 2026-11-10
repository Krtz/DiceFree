using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent]
    public sealed class TargetedCombatProjectile : MonoBehaviour
    {
        private CombatActor recipient;
        private Action<CombatActor> onImpact;
        private float speed, expires;

        private static Vector3 Origin(CombatActor unit) => unit.transform.position + Vector3.up * 1.10f;
        private static Vector3 Destination(CombatActor unit) => unit.transform.position + Vector3.up * .84f;
        public static bool HasClearLaunch(CombatActor source, CombatActor target) =>
            !Physics.Linecast(Origin(source), Destination(target), 1 << 9, QueryTriggerInteraction.Ignore);

        public static void Launch(CombatActor source, CombatActor target, string name, Color tint, float scale, float speed,
            Action<CombatActor> impact)
        {
            if (target == null || !target.Alive) return;
            var missile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            missile.name = name;
            missile.layer = 2;
            SceneManager.MoveGameObjectToScene(missile, source.gameObject.scene);
            var collider = missile.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            missile.transform.position = Origin(source);
            missile.transform.localScale = Vector3.one * scale;
            var renderer = missile.GetComponent<Renderer>();
            renderer.sharedMaterial = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            var color = new MaterialPropertyBlock();
            color.SetColor("_BaseColor", tint);
            renderer.SetPropertyBlock(color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var trail = missile.AddComponent<TrailRenderer>();
            trail.sharedMaterial = renderer.sharedMaterial;
            trail.startColor = tint;
            trail.endColor = new Color(tint.r, tint.g, tint.b, 0);
            trail.startWidth = scale * .72f;
            trail.endWidth = 0;
            trail.time = .16f;
            trail.minVertexDistance = .06f;
            var motion = missile.AddComponent<TargetedCombatProjectile>();
            motion.recipient = target;
            motion.onImpact = impact;
            motion.speed = speed;
            motion.expires = Time.time + 1.5f;
        }

        private void Update()
        {
            if (recipient == null || !recipient.Alive || Time.time >= expires)
            {
                Destroy(gameObject);
                return;
            }
            Vector3 start = transform.position;
            Vector3 destination = Destination(recipient);
            Vector3 next = Vector3.MoveTowards(start, destination, speed * Time.deltaTime);
            if (Physics.Linecast(start, next, 1 << 9, QueryTriggerInteraction.Ignore))
            {
                Destroy(gameObject);
                return;
            }
            transform.position = next;
            if ((next - destination).sqrMagnitude > .0001f) return;
            onImpact?.Invoke(recipient);
            Destroy(gameObject);
        }
    }
}
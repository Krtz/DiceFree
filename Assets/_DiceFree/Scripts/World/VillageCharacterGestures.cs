using System;
using UnityEngine;

namespace DiceFree.World
{
    /// <summary>
    /// Role-specific secondary animation for Cornberg's current unrigged village characters.
    /// Uses existing separately authored prop/hair meshes; never moves NPC gameplay roots.
    /// Arm/leg articulated animation will be supplied by future humanoid NPC rigs.
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(90)]
    public sealed class VillageCharacterGestures : MonoBehaviour
    {
        public enum Personality { Farmer, Villager, Merchant, Banker, Alchemist, Blacksmith, Guard, Herbalist }
        [SerializeField] private Personality role;
        [SerializeField] private int phaseSeed = 1;
        private VillageVisualIdle idle;
        private VillageHomeWander wander;
        private Transform hair, hat, prop;
        private Vector3 hairPosition, hatPosition, propPosition;
        private Quaternion hairRotation, hatRotation, propRotation;
        private bool captured;
        private float ease;

        public Personality Role => role;
        public bool HasSecondaryProp => prop != null;
        public void Configure(Personality personality, int seed)
        {
            ResetPose();
            role = personality; phaseSeed = seed;
            Capture();
        }

        private void OnEnable() => Capture();
        private void Capture()
        {
            if (captured) return;
            idle = GetComponent<VillageVisualIdle>();
            wander = GetComponent<VillageHomeWander>();
            var visual = idle != null ? idle.Visual : null;
            if (visual == null) return;
            var parts = visual.GetComponentsInChildren<Transform>(true);
            foreach (var t in parts)
            {
                if (hair == null && t.name == "Hair_mesh") hair = t;
                if (hat == null && t.name == "HatBrim") hat = t;
                if (prop == null && (role == Personality.Alchemist && t.name == "Vial" ||
                    role == Personality.Blacksmith && t.name == "HammerHead" ||
                    role == Personality.Merchant && t.name == "ClothTrim_mesh"))
                    prop = t;
            }
            if (hair != null) { hairPosition = hair.localPosition; hairRotation = hair.localRotation; }
            if (hat != null) { hatPosition = hat.localPosition; hatRotation = hat.localRotation; }
            if (prop != null) { propPosition = prop.localPosition; propRotation = prop.localRotation; }
            captured = true;
        }

        public void Sample(float seconds, bool moving, float dt)
        {
            if (!captured) Capture();
            if (!captured) return;
            float t = seconds + ((uint)phaseSeed % 991) * .184f;
            ease = Mathf.MoveTowards(ease, moving ? 1f : 0f, Mathf.Max(dt, 0f) * 3.5f);
            float idleWeight = 1f - ease;
            float breath = Mathf.Sin(t * 1.55f);
            // Rare but deterministic gestures, each role on a slightly different rhythm.
            float gestureFrequency = role == Personality.Guard ? .33f :
                role == Personality.Banker ? .42f : role == Personality.Alchemist ? .75f : .54f;
            float gesture = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * gestureFrequency)), 9f) * idleWeight;
            float footstep = Mathf.Sin(seconds * 9.1f + (phaseSeed % 17)) * ease;
            float headAngle = role == Personality.Guard ? .8f : role == Personality.Banker ? 1.2f : 2.2f;
            if (hair != null)
            {
                hair.localRotation = hairRotation * Quaternion.Euler(
                    gesture * -headAngle, gesture * headAngle * 1.3f, breath * .6f + footstep * .35f);
                hair.localPosition = hairPosition + new Vector3(0f, .002f * breath, 0f);
            }
            if (hat != null)
            {
                hat.localRotation = hatRotation * Quaternion.Euler(0, gesture * 1.6f, footstep * .65f);
                hat.localPosition = hatPosition + new Vector3(0f, .003f * breath, 0f);
            }
            if (prop != null)
            {
                float amount = role == Personality.Blacksmith ? 8f : role == Personality.Alchemist ? 6f : 3f;
                prop.localRotation = propRotation * Quaternion.Euler(gesture * amount, gesture * amount * .2f,
                    footstep * 1.3f);
                prop.localPosition = propPosition + Vector3.up * (gesture * .018f);
            }
        }

        private void LateUpdate() => Sample(Time.time, wander != null && wander.enabled && wander.Walking,
            Time.deltaTime);
        private void OnDisable() => ResetPose();
        private void OnDestroy() => ResetPose();
        private void ResetPose()
        {
            if (!captured) return;
            if (hair != null) { hair.localRotation = hairRotation; hair.localPosition = hairPosition; }
            if (hat != null) { hat.localRotation = hatRotation; hat.localPosition = hatPosition; }
            if (prop != null) { prop.localRotation = propRotation; prop.localPosition = propPosition; }
            captured = false;
            hair = hat = prop = null;
            ease = 0f;
        }
    }
}

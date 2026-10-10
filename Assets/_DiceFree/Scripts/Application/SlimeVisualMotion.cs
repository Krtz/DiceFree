using System;
using System.Linq;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Dungeons
{
    /// <summary>
    /// Procedural squash/stretch for the existing slime families. Includes a moving hop
    /// cycle, sleepy idle, attack anticipation/recoil, blinking and floating internal gems.
    /// Visual transforms only; root/physics/nav, attacks and dungeon telegraphs are unchanged.
    /// Boss-scripted mechanics remain the authority whenever DungeonSlime.Busy.
    /// </summary>
    [DefaultExecutionOrder(80), DisallowMultipleComponent]
    public sealed class SlimeVisualMotion : MonoBehaviour
    {
        private Transform visual, leftEye, rightEye, gem;
        private Vector3 baseScale, basePosition, leftScale, rightScale, gemPosition;
        private Quaternion baseRotation, gemRotation;
        private BasicAttack attack;
        private DungeonSlime dungeon;
        private CombatActor actor;
        private string previousAttack;
        private float impactUntil, moveBlend, hopClock, previousVertical, lastSampleTime;
        private Vector3 lastPosition;
        private float frequency, phase;
        private bool captured, wasBusy;

        public bool HasVisual => visual != null;
        public Transform Visual => visual;
        public float MovementBlend => moveBlend;

        private void Start() => Capture();

        private void Capture()
        {
            if (captured) return;
            attack = GetComponent<BasicAttack>();
            dungeon = GetComponent<DungeonSlime>();
            actor = GetComponent<CombatActor>();
            visual = transform.Find("Presentation") ??
                     transform.Cast<Transform>().FirstOrDefault(t => t.name.StartsWith("Slimes/", StringComparison.Ordinal)) ??
                     transform.Find("Slime body");
            if (visual == null) return;

            basePosition = visual.localPosition;
            baseRotation = visual.localRotation;
            baseScale = visual.localScale;
            lastPosition = transform.position;
            int seed = 17;
            foreach (char c in name) seed = unchecked(seed * 31 + c);
            phase = ((uint)seed % 911) * .0143f;
            frequency = dungeon != null && dungeon.IsBoss ? 2.35f :
                name.IndexOf("Tiny", StringComparison.OrdinalIgnoreCase) >= 0 ? 6.3f :
                name.IndexOf("Elite", StringComparison.OrdinalIgnoreCase) >= 0 ? 3f :
                name.IndexOf("Crop", StringComparison.OrdinalIgnoreCase) >= 0 ? 4.9f : 4.2f;

            // These parts are authored separately in the existing slime FBXs.
            var children = visual.GetComponentsInChildren<Transform>(true);
            leftEye = children.FirstOrDefault(t => t.name == "Left eye");
            rightEye = children.FirstOrDefault(t => t.name == "Right eye");
            gem = children.FirstOrDefault(t => t.name == "FloatingGem" ||
                t.name == "FloatingCrystals_mesh" || t.name == "GelCore");
            if (leftEye != null) leftScale = leftEye.localScale;
            if (rightEye != null) rightScale = rightEye.localScale;
            if (gem != null) { gemPosition = gem.localPosition; gemRotation = gem.localRotation; }
            captured = true;
        }

        private void LateUpdate()
        {
            // The dungeon may remove/replace its Presentation during puzzle transitions.
            // A cached Unity Transform can become a destroyed-object handle even while
            // this actor and component are alive. Rebind or skip that frame safely.
            if (!captured || visual == null)
            {
                captured = false;
                leftEye = rightEye = gem = null;
                visual = null;
                Capture();
            }
            if (!captured || visual == null) return;
            float dt = Mathf.Min(Time.deltaTime, .08f);
            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;

            // Boss Slam, Bounce, Roll and Divide directly animate their presentation.
            if (dungeon != null && dungeon.Busy)
            {
                wasBusy = true;
                moveBlend = Mathf.MoveTowards(moveBlend, 0f, dt * 3f);
                return;
            }
            if (wasBusy)
            {
                // Scripted mechanics normally restore the base pose themselves.
                // Use their current transform as a transient start pose, not an offset.
                visual.localScale = baseScale;
                visual.localPosition = basePosition;
                visual.localRotation = baseRotation;
                wasBusy = false;
            }
            float planarSpeed = new Vector2(delta.x, delta.z).magnitude / Mathf.Max(dt, .001f);
            // Teleports must not produce a 20-metre cartoon hop.
            if (delta.magnitude > 1.3f) planarSpeed = 0f;
            Sample(Time.time, planarSpeed, dt, dungeon != null && dungeon.IsBoss ? dungeon.BossBumpState : attack == null ? null : attack.State,
                actor == null || actor.Alive);
        }

        /// <summary>Deterministic animation sampling for isolated Editor tests and playback.</summary>
        public void Sample(float seconds, float planarSpeed, float deltaTime, string attackState, bool alive)
        {
            if (!captured || visual == null)
            {
                captured = false;
                leftEye = rightEye = gem = null;
                visual = null;
                Capture();
            }
            if (!captured || visual == null) return;
            float dt = Mathf.Clamp(deltaTime, 0f, .1f);
            bool isMoving = planarSpeed > .12f;
            moveBlend = Mathf.MoveTowards(moveBlend, isMoving ? 1f : 0f, dt * 4f);
            if (isMoving) hopClock += dt * frequency * (1f + Mathf.Clamp(planarSpeed * .065f, 0, .5f));

            if (previousAttack == "Wind-up" && (attackState == "Recovery" || attackState == "Miss"))
                impactUntil = seconds + .32f;
            previousAttack = attackState;
            float t = seconds + phase;
            float restingBreath = Mathf.Sin(t * 1.9f);
            float idleWiggle = Mathf.Sin(t * .81f) * .018f;
            float stride = hopClock * Mathf.PI * 2f;
            float spring = Mathf.Max(0f, Mathf.Sin(stride));
            float squash = Mathf.Max(0f, -Mathf.Sin(stride));
            float winding = attackState == "Wind-up" ? 1f : 0f;
            float recoil = Mathf.Clamp01((impactUntil - seconds) / .32f);
            float settle = 1f - Mathf.Pow(1f - recoil, 2f);
            float hop = spring * .15f * moveBlend;
            float vertical = Mathf.Lerp(restingBreath * .028f, hop, moveBlend);

            float wide = 1f + idleWiggle + restingBreath * .021f * (1f - moveBlend) -
                         .12f * spring * moveBlend + .07f * squash * moveBlend + winding * .21f -
                         settle * .17f;
            float tall = 1f - restingBreath * .037f * (1f - moveBlend) +
                         .15f * spring * moveBlend - .08f * squash * moveBlend - winding * .34f +
                         settle * .36f;
            if (!alive)
            {
                // Defeated slimes melt away in place. Gameplay death is unchanged.
                wide = Mathf.Lerp(wide, 1.30f, .8f);
                tall = .27f;
                vertical = -.08f;
            }
            visual.localScale = Vector3.Scale(baseScale,
                new Vector3(Mathf.Max(.61f, wide), Mathf.Max(.20f, tall), Mathf.Max(.61f, wide)));
            visual.localPosition = basePosition + Vector3.up * (vertical + settle * .16f);
            visual.localRotation = baseRotation * Quaternion.Euler(
                moveBlend * Mathf.Sin(stride) * 3.6f, 0f,
                Mathf.Sin(t * 1.35f) * 1.6f * (1f - moveBlend));
            previousVertical = vertical;
            lastSampleTime = seconds;

            // Quick natural blinks; different slimes never blink together.
            float blink = Mathf.Pow(Mathf.Max(0, Mathf.Sin(t * .92f + .37f)), 75f);
            if (leftEye != null) leftEye.localScale =
                Vector3.Scale(leftScale, new Vector3(1f, 1f - blink * .83f, 1f));
            if (rightEye != null) rightEye.localScale =
                Vector3.Scale(rightScale, new Vector3(1f, 1f - blink * .83f, 1f));
            if (gem != null)
            {
                gem.localPosition = gemPosition + Vector3.up * (.025f * Mathf.Sin(t * 2.9f));
                gem.localRotation = gemRotation * Quaternion.Euler(0f, Mathf.Sin(t * 1.15f) * 9f, 0f);
            }
        }

        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();
        private void Restore()
        {
            if (!captured) return;
            if (visual != null)
            {
                visual.localScale = baseScale;
                visual.localPosition = basePosition;
                visual.localRotation = baseRotation;
            }
            if (leftEye != null) leftEye.localScale = leftScale;
            if (rightEye != null) rightEye.localScale = rightScale;
            if (gem != null) { gem.localPosition = gemPosition; gem.localRotation = gemRotation; }
            captured = false;
        }
    }
}

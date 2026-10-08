using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    [RequireComponent(typeof(CombatActor), typeof(BasicAttack))]
    public sealed class RespawnAtAnchor : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private ResurrectionAnchor[] registeredAnchors = Array.Empty<ResurrectionAnchor>();
        [SerializeField, Range(0.01f, 1)] private float restoredFraction = 0.5f;
        [SerializeField, Min(0)] private float delay = 1.5f;

        private CombatActor actor;
        private BasicAttack attack;
        private float availableAt;
        private Transform fallbackAnchor;
        private Vector3 emergencyFallbackPosition;
        private bool emergencyFallbackReady;

        public bool CanReturn => actor != null && !actor.Health.Alive && Time.time >= availableAt;
        public float SecondsUntilReturn => actor == null || actor.Health == null || actor.Health.Alive
            ? 0f
            : Mathf.Max(0f, availableAt - Time.time);
        public string AnchorName => anchor != null
            ? anchor.name
            : fallbackAnchor != null
                ? fallbackAnchor.name
                : "Emergence point";
        public string AnchorId => anchor == null ? null : anchor.GetComponent<ResurrectionAnchor>()?.StableId;

        public bool LoadAtAnchor(string id)
        {
            var destination = fallbackAnchor; // Authored Cornberg anchor remains the save/load fallback.
            var currentAnchor = anchor != null ? anchor.GetComponent<ResurrectionAnchor>() : null;
            if (currentAnchor != null && currentAnchor.StableId == id) destination = currentAnchor.transform;

            foreach (var candidate in registeredAnchors ?? Array.Empty<ResurrectionAnchor>())
                if (candidate != null && candidate.StableId == id)
                {
                    destination = candidate.transform;
                    break;
                }

            if (!TryTeleport(destination))
            {
                destination = fallbackAnchor;
                if (!TryTeleport(destination) && !TryEmergencyFallback()) return false;
            }

            if (destination != null) anchor = destination;
            ResetAfterRelocation(fullRestore: true);
            return true;
        }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            fallbackAnchor = anchor;
            emergencyFallbackPosition = transform.position;
            emergencyFallbackReady = true;
        }

        private void OnEnable() => actor.Health.Died += OnDeath;
        private void OnDisable() => actor.Health.Died -= OnDeath;

        private void OnDeath()
        {
            attack.Cancel();
            GetComponent<TargetSelection>()?.Select(null);
            GetComponent<Interactor>()?.Cancel();
            availableAt = Time.time + delay;
        }

        public bool Return()
        {
            if (!CanReturn) return false;
            var dungeonReturn = GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.DeathReturnHandler;
            if (dungeonReturn != null) return dungeonReturn();

            // Runtime death-return should never strand the player. Prefer the selected/authored
            // anchor, then the authored fallback, then the scene-entry position captured at Awake.
            bool moved = TryTeleport(anchor);
            if (!moved && fallbackAnchor != anchor) moved = TryTeleport(fallbackAnchor);
            if (!moved) moved = TryEmergencyFallback();
            if (!moved) return false;

            ResetAfterRelocation(fullRestore: false);
            return true;
        }

        public void Configure(Transform value) => anchor = value;
        public void ConfigureRegisteredAnchors(params ResurrectionAnchor[] values) =>
            registeredAnchors = values ?? Array.Empty<ResurrectionAnchor>();

        private bool TryTeleport(Transform destination) =>
            destination != null && actor.Motor.Teleport(destination.position);

        private bool TryEmergencyFallback() =>
            emergencyFallbackReady && actor.Motor.Teleport(emergencyFallbackPosition);

        private void ResetAfterRelocation(bool fullRestore)
        {
            attack.ResetForSpawn();
            GetComponent<TargetSelection>()?.Select(null);
            GetComponent<Interactor>()?.Cancel();
            actor.Stats.ResetTransientModifiers();

            if (fullRestore) actor.Health.Restore();
            else actor.Health.Restore(restoredFraction);
        }
    }
}

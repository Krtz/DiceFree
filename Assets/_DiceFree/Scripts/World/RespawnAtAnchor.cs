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
        private float nextAnchorCheck;
        public event Action AnchorChanged;
        public bool HasHomeAnchor => anchor != null || fallbackAnchor != null;

        // A revive stone is activated by reaching it, not by dying. This
        // authoring contract also works for future villages and other maps.
        public bool Activate(ResurrectionAnchor stone)
        {
            if(stone==null || !stone.isActiveAndEnabled ||
               string.IsNullOrEmpty(stone.StableId))return false;
            if(anchor==stone.transform)return true;
            anchor=stone.transform;
            AnchorChanged?.Invoke();
            return true;
        }
        private void Update()
        {
            if(actor==null || !actor.Alive || Time.time<nextAnchorCheck)return;
            nextAnchorCheck=Time.time+.5f;
            foreach(var stone in FindObjectsByType<ResurrectionAnchor>())
            {
                if(stone==null || !stone.isActiveAndEnabled)continue;
                if(Vector3.ProjectOnPlane(stone.transform.position-transform.position,
                    Vector3.up).sqrMagnitude<=12.25f)
                    Activate(stone);
            }
        }

        // Travel only. Does not heal, reset stats, or invoke death return.
        public bool TeleportHome()
        {
            if(actor==null || !actor.Alive)return false;
            var lockout=GetComponent<DiceFree.Foundation.RunLoadoutLock>();
            if(lockout!=null && lockout.InsideDungeon)return false;
            bool moved=TryTeleport(anchor);
            if(!moved && fallbackAnchor!=anchor)moved=TryTeleport(fallbackAnchor);
            return moved;
        }

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
            // Newly authored village stones need not be manually listed
            // in each actor prefab, but their saved stable IDs still resolve.
            foreach(var candidate in FindObjectsByType<ResurrectionAnchor>())
                if(candidate!=null && candidate.StableId==id)
                {
                    destination=candidate.transform;
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

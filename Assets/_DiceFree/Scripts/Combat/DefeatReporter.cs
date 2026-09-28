using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class DefeatReporter : MonoBehaviour
    {
        [Tooltip("Optional owner for summoned/controlled actors. Null credits this actor.")]
        [SerializeField] private CombatActor creditOwner;
        private CombatActor actor;
        private bool reported;
        private void Awake() => actor = GetComponent<CombatActor>();
        private void OnEnable() { actor.Health.Defeated += OnDefeated; actor.Health.Restored += OnRestored; }
        private void OnDisable() { actor.Health.Defeated -= OnDefeated; actor.Health.Restored -= OnRestored; }
        private void OnRestored() => reported = false;
        private void OnDefeated(CombatActor killer)
        {
            if (reported) return;
            reported = true;
            var source = killer == null ? null : killer.GetComponent<DefeatReporter>();
            var owner = source != null && source.creditOwner != null ? source.creditOwner : killer;
            DefeatEvents.Report(actor, killer, owner);
        }
    }
}

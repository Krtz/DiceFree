using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Combat
{
    [RequireComponent(typeof(CombatActor))]
    public sealed class TargetSelection : MonoBehaviour
    {
        [SerializeField, Min(1)] private float scope = 35;
        private CombatActor owner;
        public CombatActor Selected { get; private set; }
        private void Awake() => owner = GetComponent<CombatActor>();
        private void Update() { if (Selected != null && !ValidSelection(Selected)) Selected = null; }
        public bool Valid(CombatActor target) => ValidHostile(target);
        public bool ValidHostile(CombatActor target) => owner.IsHostileTo(target) && InScope(target);
        public bool ValidFriendly(CombatActor target) => owner.IsFriendlyTo(target) && target != owner && InScope(target);
        public bool ValidSelection(CombatActor target) => target != null && target != owner && target.Alive && InScope(target);
        public void Select(CombatActor target) => Selected = ValidSelection(target) ? target : null;
        private bool InScope(CombatActor target) => target != null &&
            Vector3.Distance(transform.position, target.transform.position) <= scope;
        public void Cycle(bool backwards)
        {
            var candidates = new List<CombatActor>();
            foreach (var actor in CombatActor.All) if (Valid(actor) && owner.HasSightOf(actor)) candidates.Add(actor);
            candidates.Sort((a,b) => (a.transform.position-transform.position).sqrMagnitude.CompareTo((b.transform.position-transform.position).sqrMagnitude));
            if (candidates.Count == 0) { Selected = null; return; }
            int index = candidates.IndexOf(Selected);
            Selected = candidates[(index + (backwards ? candidates.Count - 1 : 1) + candidates.Count) % candidates.Count];
        }
    }
}

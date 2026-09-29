using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public sealed class ReachArea : MonoBehaviour
    {
        [SerializeField] private string stableId;
        [SerializeField, Min(0.1f)] private float radius = 4;
        private readonly HashSet<CombatActor> inside = new();
        public string StableId => stableId;
        public float Radius => radius;
        public bool Contains(CombatActor actor) => actor != null && actor.Alive &&
            (actor.transform.position - transform.position).sqrMagnitude <= radius * radius;
        private void Update()
        {
            inside.RemoveWhere(actor => !Contains(actor));
            foreach (var actor in CombatActor.All)
                if (Contains(actor) && inside.Add(actor)) AreaEvents.Report(stableId, actor);
        }
        private void OnDisable() => inside.Clear();
        public void Configure(string id, float range) { stableId = id; radius = range; }
        private void OnDrawGizmosSelected() { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, radius); }
    }
}

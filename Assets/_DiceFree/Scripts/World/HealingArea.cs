using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public sealed class HealingArea : MonoBehaviour
    {
        [SerializeField, Min(0)] private float radius = 4;
        [SerializeField, Min(0)] private float hpPerSecond = 5;
        public bool Contains(CombatActor actor) => actor.Alive &&
            Vector3.Distance(transform.position, actor.transform.position) <= radius;
        private void Update()
        {
            foreach (var actor in CombatActor.All)
                if (Contains(actor)) actor.Health.Heal(hpPerSecond * Time.deltaTime);
        }
    }
}

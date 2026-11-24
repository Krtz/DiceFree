using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public sealed class HealingArea : MonoBehaviour
    {
        [SerializeField, Min(0)] private float radius = 4;
        [SerializeField, Min(0)] private float hpPerSecond = 25;
        public float Radius => radius;
        public float HpPerSecond => hpPerSecond;
        public void Configure(float healingPerSecond, float reach)
        { hpPerSecond=Mathf.Max(0,healingPerSecond); radius=Mathf.Max(0,reach); }
        public bool Contains(CombatActor actor) => actor.Alive &&
            Vector3.Distance(transform.position, actor.transform.position) <= radius;
        private void Update()
        {
            foreach (var actor in CombatActor.All)
                if (Contains(actor)) actor.Health.Heal(hpPerSecond * Time.deltaTime);
        }
    }
}

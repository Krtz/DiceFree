using System;
using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Items
{
    [RequireComponent(typeof(CombatActor))]
    public sealed class FixedWorldDrop : MonoBehaviour
    {
        public ItemDefinition item;
        [Range(0, 1)] public float chance = 0.2f;
        private CombatActor actor;
        private bool rolled;
        private IRandomSource random;
        public int RollCount { get; private set; }
        public event Action<WorldEquipmentPickup> Dropped;
        public void SetRandom(IRandomSource value) => random = value ?? throw new ArgumentNullException(nameof(value));
        private void Awake() => actor = GetComponent<CombatActor>();
        private void OnEnable() { DefeatEvents.Reported += OnDefeat; actor.Health.Restored += ResetLife; }
        private void OnDisable() { DefeatEvents.Reported -= OnDefeat; actor.Health.Restored -= ResetLife; }
        private void ResetLife() => rolled = false;
        private void OnDefeat(ActorDefeated fact)
        {
            if (fact.victim != actor || rolled || item == null) return;
            rolled = true; random ??= new SeededRandomSource(Guid.NewGuid().GetHashCode()); RollCount++;
            if (random.NextUnit() >= chance) return;
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "World equipment: " + item.stableId; root.layer = 11;
            root.transform.position = fact.position + Vector3.up * 0.3f;
            root.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            var pickup = root.AddComponent<WorldEquipmentPickup>(); pickup.Configure(item);
            Dropped?.Invoke(pickup);
        }
    }
}

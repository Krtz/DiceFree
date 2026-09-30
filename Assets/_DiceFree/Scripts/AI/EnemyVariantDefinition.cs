using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.AI
{
    [CreateAssetMenu(menuName = "DiceFree/Enemies/Variant")]
    public sealed class EnemyVariantDefinition : ScriptableObject
    {
        public EnemyArchetype archetype;
        public ActorDefinition stats;
        [Min(1)] public int level = 1;
        [Min(0.1f)] public float bodyScale = 1;
        public bool overrideAwareness, overrideLeash;
        public bool overrideAutoAggro;
        [SerializeReference] public AutoAggroPolicy autoAggroPolicy = new();
        public AutoAggroPolicy AutoAggroPolicy => overrideAutoAggro ? autoAggroPolicy : archetype.autoAggroPolicy;
        [Min(1)] public float awareness = 7, leash = 13;
        public float Awareness => overrideAwareness ? awareness : archetype.awareness;
        public float Leash => overrideLeash ? leash : archetype.leash;
    }
}

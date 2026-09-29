using UnityEngine;

namespace DiceFree.AI
{
    [CreateAssetMenu(menuName = "DiceFree/Enemies/Archetype")]
    public sealed class EnemyArchetype : ScriptableObject
    {
        public string familyId;
        public GameObject prefab;
        [Min(1)] public float awareness = 7, leash = 13;
        [Min(0.1f)] public float radius = 0.65f, height = 1.8f;
    }
}

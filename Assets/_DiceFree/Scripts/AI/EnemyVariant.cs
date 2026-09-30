using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.AI
{
    // Runs before stats/health initialize. The existing actor engine remains the only combat pipeline.
    [DefaultExecutionOrder(-150)]
    public sealed class EnemyVariant : MonoBehaviour
    {
        [SerializeField] private EnemyVariantDefinition definition;
        [SerializeField] private Transform visual;
        [SerializeField] private Vector3 baseScale, basePosition;
        public EnemyVariantDefinition Definition => definition;
        private void Awake() => Apply();
        public void Configure(EnemyVariantDefinition value, Transform model = null)
        {
            definition = value;
            if (model != null) { visual = model; baseScale = model.localScale; basePosition = model.localPosition; }
            Apply();
        }
        private void Apply()
        {
            var archetype = definition.archetype;
            GetComponent<ActorStats>().Configure(definition.stats, definition.level);
            GetComponent<AggroBehaviour>().Configure(definition.Awareness, definition.Leash);
            GetComponent<AggroBehaviour>().ConfigureAutoAggro(definition.AutoAggroPolicy);
            float radius = archetype.radius * definition.bodyScale, height = archetype.height * definition.bodyScale;
            GetComponent<CombatActor>().Configure(1, radius, archetype.familyId);
            var agent = GetComponent<NavMeshAgent>(); agent.radius = radius; agent.height = height;
            var collider = GetComponent<CapsuleCollider>(); collider.radius = radius; collider.height = height;
            collider.center = Vector3.up * height * 0.5f;
            visual.localScale = baseScale * definition.bodyScale;
            visual.localPosition = basePosition * definition.bodyScale;
        }
    }
}

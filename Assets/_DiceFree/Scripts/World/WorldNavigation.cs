using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.World
{
    [DefaultExecutionOrder(-200)]
    public sealed class WorldNavigation : MonoBehaviour
    {
        [SerializeField] private NavMeshData data;
        private NavMeshDataInstance instance;
        public NavMeshData Data => data;
        private void OnEnable() { if (data != null) instance = NavMesh.AddNavMeshData(data); }
        private void OnDisable() { if (instance.valid) instance.Remove(); }
        public void Configure(NavMeshData navigation) => data = navigation;
    }
}

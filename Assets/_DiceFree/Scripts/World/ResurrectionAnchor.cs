using UnityEngine;

namespace DiceFree.World
{
    public sealed class ResurrectionAnchor : MonoBehaviour
    {
        [SerializeField] private string stableId;
        public string StableId => stableId;
        public void Configure(string id) => stableId = id;
    }
}

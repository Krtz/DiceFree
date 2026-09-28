using UnityEngine;

namespace DiceFree.World
{
    [CreateAssetMenu(menuName = "DiceFree/World/Overworld respawn")]
    public sealed class RespawnDefinition : ScriptableObject
    {
        [Min(0.1f)] public float delaySeconds = 10;
    }
}

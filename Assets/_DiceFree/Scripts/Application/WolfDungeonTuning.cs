using UnityEngine;
using DiceFree.World;
using DiceFree.Combat;
namespace DiceFree.Dungeons
{
    [CreateAssetMenu(menuName="DiceFree/Dungeons/Wolf Survival")]
    public sealed class WolfDungeonTuning : ScriptableObject
    {
        public DungeonDefinition definition;
        public GameObject weak, strong, alpha;
        public ActorDefinition dummy;
        [Min(1)] public float intermissionSeconds=4;
        [Min(1)] public float warningSeconds=2;
        [Min(1)] public float victorySeconds=5;
        public GameObject Template(WolfRank rank)=>rank switch{WolfRank.Weak=>weak,WolfRank.Strong=>strong,_=>alpha};
    }
}

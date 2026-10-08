using UnityEngine;
using DiceFree.Items;
namespace DiceFree.Dungeons
{
    [CreateAssetMenu(menuName="DiceFree/Dungeons/Reward pool")]
    public sealed class DungeonRewardPool : ScriptableObject
    {
        public string stableId;
        public ItemDefinition rare;
        public ItemDefinition[] items;
        public GameObject visualVariant;
        public ItemDefinition[] Roll()
        {
            if (Random.value < .05f && rare != null) return new[]{rare};
            if (items == null || items.Length == 0) return System.Array.Empty<ItemDefinition>();
            int first=Random.Range(0,items.Length);
            if (Random.value < .75f || items.Length==1) return new[]{items[first]};
            int second=Random.Range(0,items.Length-1); if(second>=first) second++;
            return new[]{items[first],items[second]};
        }
    }
}

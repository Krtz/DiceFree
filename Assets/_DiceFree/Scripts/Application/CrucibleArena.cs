using UnityEngine;

namespace DiceFree.Dungeons
{
    public sealed class CrucibleArena : MonoBehaviour
    {
        public CrucibleBoss[] bosses;
        public CrucibleGate[] gates;
        public DungeonRewardPool rewards;
        public static Vector3 CellCenter(int index)=>CrimsonCruciblePortal.Origin+Vector3.forward*(index*48);
        public static bool InCell(Vector3 at,int index,float margin=0)
        {var p=at-CellCenter(index);return Mathf.Abs(p.x)<=18+margin&&Mathf.Abs(p.z)<=18+margin;}
    }
}

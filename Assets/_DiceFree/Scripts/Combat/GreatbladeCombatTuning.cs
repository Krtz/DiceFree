using UnityEngine;
namespace DiceFree.Combat
{
    [CreateAssetMenu(menuName="DiceFree/Combat/Greatblade tuning")]
    public sealed class GreatbladeCombatTuning:ScriptableObject
    {
        public AttackDefinition wideArc,spinDash,overheadBreaker;
        public float arcCooldown=5,arcWindup=.35f,arcReach=3,arcAngle=110;
        public float spinCooldown=10,spinRange=12,spinSpeed=13,spinRadius=1.8f;
        public float breakerCooldown=7,breakerWindup=.55f,breakerReach=1.2f,breakerMinimumMomentum=20;
        public float cryCooldown=24,cryDuration=6,cryDamagePercent=12;
        public float basicMomentum=8,skillMomentum=10,areaMomentumCap=24;
        public float decayDelay=5,decayPerSecond=8;
        public float ArcCoefficient(int rank)=>1.8f+.2f*Mathf.Max(0,rank-1);
        public float SpinCoefficient(int rank)=>1.2f+.2f*Mathf.Max(0,rank-1);
        public float BreakerCoefficient(int rank)=>2.5f+.3f*Mathf.Max(0,rank-1);
    }
}

using UnityEngine;
using DiceFree.Combat;
using DiceFree.Items;
namespace DiceFree.Dungeons
{
    [CreateAssetMenu(menuName="DiceFree/Dungeons/Slime playtest tuning")]
    public sealed class SlimeDungeonTuning : ScriptableObject
    {
        public float stagingSeconds=60, victorySeconds=15, slamWarning=3, slamInterval=10f;
        [Min(1)] public float bossDamageMultiplier=10f;
        public float minibossHp=800, bossHp=3000, regentHp=16000, fragmentHpFraction=.12f;
        public float bossFragmentHp=300, regentFragmentHpFraction=.10f, minibossFragmentHpFraction=.10f, minibossScale=6.9f;
        // Preserve fragment durability when increasing only parent encounter health.
        public float minibossFragmentReferenceHp=160, bossFragmentReferenceHp=420;
        public float trashHp=28, puzzleHp=20, addHp=14, regentAddHp=35;
        public float reunionRadius=7, minibossReunion=10, bossReunion=12, regentReunion=15;
        public float minibossSlamRadius=2.5f, bossSlamRadius=4, regentSlamRadius=5.5f;
        [Range(0,1)] public float regentTrailMultiplier=.18f;
        public AttackDefinition slam, regentSlam, addAttack, regentAddAttack;
        public ActorDefinition green, puzzleGreen, blue, miniboss, boss, regent, fragment, add, regentAdd, dummy;
        public GameObject slimeTemplate, bossPresentation, regentPresentation;
        // All payouts are deliberately provisional serialized playtest placeholders.
        public int trashXp=4, trashGold=2, minibossXp=30, minibossGold=15;
        public int bossXp=70, bossGold=35, regentXp=3600, regentGold=280;
        public int puzzleXp=30, bluePuzzleXp=80, bonusXp=50, bonusGold=25, regentBonusXp=1800, regentBonusGold=140;
        public DungeonRewardPool normalRewards, regentRewards;
    }
}

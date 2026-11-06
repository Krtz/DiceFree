using DiceFree.Combat;
using DiceFree.Items;

namespace DiceFree.Dungeons
{
    // Reward choices share presentation and ownership rules across dungeon types.
    public interface IDungeonRewardRun
    {
        void ObserveReward(CombatActor actor, ItemDefinition item);
        void Drop(CombatActor actor, ItemDefinition item);
        void ReturnPlayer(CombatActor actor, bool atEntrance);
        void Finish();
    }
}

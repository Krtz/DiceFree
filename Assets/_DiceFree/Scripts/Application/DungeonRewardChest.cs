using DiceFree.Combat;
using DiceFree.Characters;
using DiceFree.World;
using DiceFree.Items;
using UnityEngine;

namespace DiceFree.Dungeons
{
    /// <summary>One owner-only choice: pick a chest and leave the reward map.</summary>
    public sealed class DungeonRewardChest : InteractionTarget
    {
        private DungeonRewardRoom room;
        private CombatActor owner;
        private int choiceIndex;
        private int number;
        public ItemDefinition OfferedItem { get; private set; }
        public int ChoiceIndex=>choiceIndex;

        public void Configure(DungeonRewardRoom rewardRoom, CombatActor actor, int option, string label,int chestNumber=1,ItemDefinition item=null)
        {
            room = rewardRoom; owner = actor; choiceIndex = option;
            number=chestNumber;OfferedItem=item;
            ConfigureName(label);
        }
        public override bool Available(CombatActor actor) =>
            actor == owner && room != null && room.HasChoice(owner) && base.Available(actor);

        public override void Interact(CombatActor actor)
        {
            if (CanInteract(actor)) room.Choose(actor, choiceIndex);
        }

        private void OnGUI()
        {
            if (!Available(owner) || Camera.main == null) return;
            var local = FindAnyObjectByType<TraversalInput>();
            if (local != null && local.GetComponent<CombatActor>() != owner) return;
            var world = transform.position + Vector3.up * 1.25f;
            var point = Camera.main.WorldToScreenPoint(world);
            if (point.z <= 0 || point.z > 150) return;
            GUI.Box(new Rect(point.x - 18, Screen.height - point.y, 36, 26),number.ToString());
        }
    }
}

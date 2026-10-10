using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Items
{
    /// <summary>Physical chest holding the exact original item instance, not a duplicate reward.</summary>
    [DisallowMultipleComponent]
    public sealed class DroppedItemPickup : InteractionTarget
    {
        private ItemInstance item;
        private ItemDefinition definition;
        private bool claimed;
        private WorldDroppedItemLedger ledger;

        public ItemInstance Item => item?.Copy();
        public bool Claimed => claimed;

        public void Configure(ItemInstance owned,ItemDefinition resolved,
            WorldDroppedItemLedger owner)
        {
            item=owned?.Copy();
            definition=resolved;
            ledger=owner;
            ConfigureName("Pick up "+(definition?.displayName??"dropped item"));
        }

        public override bool CanInteract(CombatActor actor) =>
            !claimed && item!=null && definition!=null &&
            base.CanInteract(actor) &&
            actor.GetComponent<CarriedInventory>() is { } inventory &&
            inventory.Resolve(item.definitionId)!=null &&
            inventory.Find(item.instanceId)==null;

        public override void Interact(CombatActor actor) => TryPickup(actor);

        public bool TryPickup(CombatActor actor)
        {
            if(!CanInteract(actor))return false;
            var inventory=actor.GetComponent<CarriedInventory>();
            claimed=true;
            if(ledger!=null)
            {
                if(ledger.TryClaim(this,inventory))return true;
                claimed=false;
                return false;
            }
            if(!inventory.RestoreDroppedItem(item))
            {
                claimed=false;
                return false;
            }
            Destroy(gameObject);
            return true;
        }

        private void OnGUI()
        {
            if(claimed||definition==null||Camera.main==null)return;
            var screen=Camera.main.WorldToScreenPoint(transform.position+Vector3.up);
            if(screen.z>0 && screen.z<35f)
                GUI.Box(new Rect(screen.x-95,Screen.height-screen.y,190,37),
                    definition.displayName+"\nRight-click / I to pick up");
        }
    }
}

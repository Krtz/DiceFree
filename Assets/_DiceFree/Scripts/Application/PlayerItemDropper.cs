using System;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.UI;
using DiceFree.Banking;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Items
{
    /// <summary>
    /// Safe one-to-one bag-to-world transfer. Chest is spawned before inventory
    /// ownership changes, and the same instance ID is used on recovery.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CarriedInventory),typeof(Equipment))]
    public sealed class PlayerItemDropper : MonoBehaviour, IInventoryItemActions
    {
        private CarriedInventory inventory;
        private Equipment equipment;
        private CombatActor actor;
        private WorldDroppedItemLedger ledger;

        private void Awake()
        {
            inventory=GetComponent<CarriedInventory>();
            equipment=GetComponent<Equipment>();
            actor=GetComponent<CombatActor>();
            ledger=GetComponent<WorldDroppedItemLedger>();
        }

        public bool Perform(string instanceId,string action,out string message)
        {
            switch(action)
            {
                case "Drop": return Drop(instanceId,out message);
                case "Send to Bank":
                    var bank=GetComponent<EchoSharedBank>();
                    if(bank==null)
                    {message="Shared bank is unavailable.";return false;}
                    return bank.Deposit(instanceId,out message);
                default:
                    message="Unsupported bag action.";
                    return false;
            }
        }

        public bool Drop(string instanceId,out string message)
        {
            message="";
            var item=inventory.Find(instanceId);
            if(item==null){message="That item is not in your bag.";return false;}
            var definition=inventory.Resolve(item.definitionId);
            if(definition==null){message="Unknown item; nothing dropped.";return false;}
            if(equipment.IsEquipped(instanceId))
            {
                message="Unequip before dropping.";
                return false;
            }
            if(definition.questItem)
            {
                message="Quest items cannot be dropped.";
                return false;
            }
            if(GetComponent<RunLoadoutLock>()?.Locked==true)
            {
                message="You cannot change your loadout during this dungeon.";
                return false;
            }
            if(actor!=null && !actor.Alive)
            {
                message="You cannot drop items while defeated.";
                return false;
            }
            var ahead=transform.position+transform.forward*1.4f+Vector3.up*8f;
            if(!Physics.Raycast(ahead,Vector3.down,out var ground,30f,
                1<<8,QueryTriggerInteraction.Ignore) ||
               !NavMesh.SamplePosition(ground.point,out var hit,1.8f,NavMesh.AllAreas))
            {
                message="Stand on walkable ground to drop that item.";
                return false;
            }
            if(ledger==null)ledger=GetComponent<WorldDroppedItemLedger>();
            if(ledger==null)
            {
                message="World drop persistence unavailable; item retained.";
                return false;
            }
            return ledger.TryDrop(item,hit.position,out message);
        }
    }
}

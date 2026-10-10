using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Items
{
    /// <summary>
    /// Echo-wide save record for deliberately dropped items. A drop cannot vanish
    /// on restart and recovery gives back its original instance ID and bound flag.
    /// </summary>
    [DisallowMultipleComponent,RequireComponent(typeof(CarriedInventory))]
    public sealed class WorldDroppedItemLedger : MonoBehaviour, IEchoWideDurableState
    {
        [Serializable] private sealed class Record
        {
            public ItemInstance item;
            public Vector3 position;
            public string scene;
        }
        [Serializable] private sealed class State
        {
            public Record[] records=Array.Empty<Record>();
        }
        private State state=new();
        private readonly Dictionary<string,DroppedItemPickup> active=new();
        private CarriedInventory inventory;
        private ManifestationPersistence persistence;
        public string SectionId=>"echo.world-dropped-items";
        public int Version=>1;
        public event Action Changed;
        public int Count=>state.records.Length;
        public string CaptureJson()=>JsonUtility.ToJson(state);

        private void Awake()
        {
            inventory=GetComponent<CarriedInventory>();
            persistence=GetComponent<ManifestationPersistence>();
        }
        public void ResetToDefault()
        {
            Despawn();
            state=new State();
            Changed?.Invoke();
        }
        public void RestoreJson(int version,string json)
        {
            if(version!=Version)throw new NotSupportedException("Unsupported world drops version.");
            var restored=JsonUtility.FromJson<State>(json);
            if(restored==null||restored.records==null||
               !CarriedInventory.Valid(restored.records.Select(r=>r.item).ToArray())||
               restored.records.Any(r=>r==null||string.IsNullOrWhiteSpace(r.scene)))
                throw new ArgumentException("Invalid world-dropped item save records.");
            Despawn();
            state=restored;
            foreach(var entry in state.records)
            {
                if(entry.scene==SceneManager.GetActiveScene().name)
                    Spawn(entry);
            }
            Changed?.Invoke();
        }
        private DroppedItemPickup Spawn(Record entry)
        {
            var definition=inventory.Resolve(entry.item.definitionId);
            if(definition==null)
            {
                Debug.LogWarning("Dropped item definition not loaded yet: "+
                    entry.item.definitionId+"; record kept for future recovery.");
                return null;
            }
            var chest=WorldLootVisual.Create(entry.position,
                "Dropped item chest: "+definition.displayName,.66f);
            var pickup=chest.AddComponent<DroppedItemPickup>();
            pickup.Configure(entry.item,definition,this);
            active[entry.item.instanceId]=pickup;
            return pickup;
        }

        public bool TryDrop(ItemInstance item,Vector3 point,out string message)
        {
            message="";
            if(item==null || inventory.Find(item.instanceId)==null ||
               state.records.Any(x=>x.item.instanceId==item.instanceId))
            {message="Item unavailable or already dropped.";return false;}
            var definition=inventory.Resolve(item.definitionId);
            if(definition==null){message="Unknown equipment.";return false;}
            var entry=new Record{item=item.Copy(),position=point,
                scene=SceneManager.GetActiveScene().name};
            DroppedItemPickup pickup=null;
            try
            {
                pickup=Spawn(entry);
                if(pickup==null)
                {message="Unable to spawn a dropped chest.";return false;}
                using(persistence?.DeferDurableWrites())
                {
                    // Spawn first. Then transfer the existing ID into the ledger.
                    if(!inventory.Remove(item.instanceId))
                    {
                        active.Remove(item.instanceId);
                        Destroy(pickup.gameObject);
                        message="Drop failed; item retained.";
                        return false;
                    }
                    state.records=state.records.Concat(new[]{entry}).ToArray();
                    Changed?.Invoke();
                }
                message="Dropped "+definition.displayName+".";
                return true;
            }
            catch(Exception error)
            {
                Debug.LogException(error);
                message="Drop failed: the original item was preserved if not transferred.";
                return false;
            }
        }

        public bool TryClaim(DroppedItemPickup pickup,CarriedInventory destination)
        {
            if(pickup==null || destination==null)return false;
            var item=pickup.Item;
            if(item==null || !state.records.Any(x=>x.item.instanceId==item.instanceId))
                return false;
            using(persistence?.DeferDurableWrites())
            {
                if(!destination.RestoreDroppedItem(item))return false;
                state.records=state.records.Where(x=>x.item.instanceId!=item.instanceId).ToArray();
                active.Remove(item.instanceId);
                Changed?.Invoke();
                Destroy(pickup.gameObject);
            }
            return true;
        }

        private void Despawn()
        {
            foreach(var pickup in active.Values)
                if(pickup!=null)
                {
                    pickup.gameObject.SetActive(false);
                    Destroy(pickup.gameObject);
                }
            active.Clear();
        }
    }
}

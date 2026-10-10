using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceFree.Items
{
    public sealed class CarriedInventory : MonoBehaviour
    {
        [SerializeField] private ItemDefinition[] definitions = Array.Empty<ItemDefinition>();
        private readonly List<ItemInstance> owned = new();
        public event Action Changed;
        public ItemInstance[] Items => owned.Select(i => i.Copy()).ToArray();
        public IReadOnlyList<ItemDefinition> Definitions => definitions;
        public void Configure(ItemDefinition[] values) => definitions = values;
        public ItemDefinition Resolve(string id) => Array.Find(definitions, d => d != null && d.stableId == id);
        public ItemInstance Find(string id) => owned.Find(i => i.instanceId == id)?.Copy();
        public ItemInstance Grant(string definitionId)
        {
            if (Resolve(definitionId) == null) throw new ArgumentException("Unresolved item cannot be granted.");
            var item = new ItemInstance { instanceId = Guid.NewGuid().ToString("D"), definitionId = definitionId };
            owned.Add(item); Changed?.Invoke(); return item.Copy();
        }
        /// <summary>Recover a world-dropped item without changing its unique identity.</summary>
        public bool RestoreDroppedItem(ItemInstance item)
        {
            if(item==null || !Guid.TryParse(item.instanceId,out _) ||
               Resolve(item.definitionId)==null ||
               owned.Any(i=>i.instanceId==item.instanceId))return false;
            owned.Add(item.Copy());
            Changed?.Invoke();
            return true;
        }

        public bool Remove(string instanceId)
        {
            int index = owned.FindIndex(i => i.instanceId == instanceId);
            if (index < 0) return false;
            owned.RemoveAt(index); Changed?.Invoke(); return true;
        }
        public bool ExchangeForGold(string instanceId,GoldWallet wallet,long price)
        {
            int index=owned.FindIndex(i=>i.instanceId==instanceId);
            if(index<0||wallet==null||price<0)return false;
            var item=owned[index];var definition=Resolve(item.definitionId);
            if(definition==null||!definition.canSell||definition.questItem||definition.bound||item.bound||GetComponent<Equipment>()?.IsEquipped(instanceId)==true||GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.Locked==true)return false;
            // No callbacks between wallet validation/credit and inventory removal. Observers see both states committed.
            if(!wallet.CreditWithoutNotification(price))return false;
            owned.RemoveAt(index);
            try{Changed?.Invoke();}finally{wallet.NotifyChanged();}
            return true;
        }
        public bool PurchaseForGold(string definitionId,GoldWallet wallet,long price)
        {
            if(Resolve(definitionId)==null||wallet==null||!wallet.CanSpend(price)||GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.Locked==true)return false;
            var item=new ItemInstance{instanceId=Guid.NewGuid().ToString("D"),definitionId=definitionId};
            if(!wallet.DebitWithoutNotification(price))return false;
            owned.Add(item);
            try{Changed?.Invoke();}finally{wallet.NotifyChanged();}
            return true;
        }
        public static bool Valid(ItemInstance[] items) => items != null &&
            items.All(i => i != null && Guid.TryParse(i.instanceId, out _) && !string.IsNullOrWhiteSpace(i.definitionId)) &&
            items.Select(i => i.instanceId).Distinct(StringComparer.Ordinal).Count() == items.Length;
        public void Restore(ItemInstance[] items)
        {
            ReplaceWithoutNotification(items); NotifyChanged();
        }
        // Transaction coordinators publish only after every ownership record has committed.
        public void ReplaceWithoutNotification(ItemInstance[] items)
        {
            if (!Valid(items)) throw new ArgumentException("Invalid owned item records.");
            owned.Clear(); owned.AddRange(items.Select(i => i.Copy()));
        }
        public void NotifyChanged() => Changed?.Invoke();
    }
}

using System;
using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Items
{
    /// <summary>Reusable authored shop. Ownership is granted only by an explicit funded purchase.</summary>
    public sealed class ItemShop : InteractionTarget
    {
        [Serializable] public sealed class Offer
        {
            public ItemDefinition item;
            [Min(0)] public int goldPrice = 15;
            public bool provisionalPrice = true;
        }
        [SerializeField] private Offer[] offers = Array.Empty<Offer>();
        private bool purchasing;
        [SerializeField,Range(0,1)] private float resaleFraction=.5f;
        [SerializeField,Min(0)] private int provisionalFallbackResaleGold=1;
        public long ResalePrice(ItemDefinition item)
        {
            foreach(var offer in offers)if(offer?.item==item)return (long)Math.Floor(offer.goldPrice*Mathf.Clamp01(resaleFraction));
            return provisionalFallbackResaleGold;
        }
        public bool CanSell(CombatActor seller,string instanceId)
        {
            if(purchasing||!CanInteract(seller))return false;
            var inventory=seller.GetComponent<CarriedInventory>();var item=inventory?.Find(instanceId);var definition=item==null?null:inventory.Resolve(item.definitionId);
            return definition!=null&&definition.canSell&&!definition.questItem&&!definition.bound&&!item.bound&&seller.GetComponent<Equipment>()?.IsEquipped(instanceId)!=true&&seller.GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.Locked!=true;
        }
        public bool Sell(CombatActor seller,string instanceId,out string message)
        {
            message="Item cannot be sold.";if(!CanSell(seller,instanceId))return false;
            var inventory=seller.GetComponent<CarriedInventory>();var item=inventory.Find(instanceId);var definition=inventory.Resolve(item.definitionId);long price=ResalePrice(definition);
            purchasing=true;
            try{if(!inventory.ExchangeForGold(instanceId,seller.GetComponent<GoldWallet>(),price))return false;message="Sold "+definition.displayName+" for "+price+" gold (provisional).";return true;}
            finally{purchasing=false;}
        }
        public Offer[] Offers => offers;
        public void Configure(Offer[] values) => offers = values ?? Array.Empty<Offer>();
        public bool Purchase(CombatActor buyer, int index, out string message)
        {
            message = "Purchase unavailable.";
            if (purchasing || !CanInteract(buyer) || index < 0 || index >= offers.Length) return false;
            var offer = offers[index];
            var inventory = buyer.GetComponent<CarriedInventory>();
            var wallet = buyer.GetComponent<GoldWallet>();
            if (offer?.item == null || offer.goldPrice < 0 || inventory == null || wallet == null ||
                inventory.Resolve(offer.item.stableId) != offer.item) return false;
            purchasing = true;
            try
            {
                // Guard spans synchronous Changed callbacks, preventing reentrant double purchases.
                if(!wallet.CanSpend(offer.goldPrice)){message="Not enough gold.";return false;}
                if (!inventory.PurchaseForGold(offer.item.stableId,wallet,offer.goldPrice)) { message = "Inventory is locked."; return false; }
                message = "Purchased " + offer.item.displayName + ". Equip it from Inventory.";
                return true;
            }
            finally { purchasing = false; }
        }
    }
}

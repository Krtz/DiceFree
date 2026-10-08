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
                if (!wallet.Spend(offer.goldPrice)) { message = "Not enough gold."; return false; }
                inventory.Grant(offer.item.stableId);
                message = "Purchased " + offer.item.displayName + ". Equip it from Inventory.";
                return true;
            }
            finally { purchasing = false; }
        }
    }
}

using System;
using UnityEngine;
namespace DiceFree.Items
{
    public sealed class GoldWallet : MonoBehaviour
    {
        public long Gold { get; private set; }
        public event Action Changed;
        public void Grant(long amount) { if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount)); Gold = checked(Gold + amount); Changed?.Invoke(); }
        public bool CanSpend(long amount) => amount >= 0 && Gold >= amount;
        public bool Spend(long amount) { if (!CanSpend(amount)) return false; Gold -= amount; Changed?.Invoke(); return true; }
        public void Restore(long amount) { if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount)); Gold = amount; Changed?.Invoke(); }
    }
    public static class FixedItemGrant
    {
        // Caller owns completion/idempotency. Load restores records, never calls Grant.
        public static ItemInstance Grant(CarriedInventory recipient, ItemDefinition item) => recipient.Grant(item.stableId);
    }
}

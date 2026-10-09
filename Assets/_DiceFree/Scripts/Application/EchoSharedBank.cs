using System;
using System.Linq;
using System.IO;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Banking
{
    [DisallowMultipleComponent, RequireComponent(typeof(CarriedInventory), typeof(GoldWallet))]
    public sealed class EchoSharedBank : MonoBehaviour, IEchoWideDurableState
    {
        [Serializable] private sealed class State
        {
            public ItemInstance[] items = Array.Empty<ItemInstance>();
            public long gold;
            public int purchasedSlots;
        }
        [SerializeField, Min(1)] private int initialSlots = 60;
        [SerializeField, Min(1)] private int expansionSlots = 30;
        [SerializeField, Min(1)] private int expansionBaseGold = 100;
        private State state = new();
        private bool busy;
        public string SectionId => "echo.shared-bank";
        public int Version => 1;
        public event Action Changed;
        public ItemInstance[] Items => state.items.Select(i => i.Copy()).ToArray();
        public long Gold => state.gold;
        public int Capacity => checked(initialSlots + state.purchasedSlots);
        public long ExpansionPrice => checked((long)expansionBaseGold * (1L + state.purchasedSlots / expansionSlots));
        public int ExpansionSlots => expansionSlots;
        public bool TownAccess => GetComponent<RunLoadoutLock>()?.InsideDungeon != true &&
            GetComponent<Interactor>()?.Active is BankInteraction banker &&
            banker.gameObject.scene.name == "Cornberg" && banker.CanInteract(GetComponent<CombatActor>());
        public string CaptureJson() => JsonUtility.ToJson(state);
        public void ResetToDefault() { if (busy) throw new InvalidOperationException("Bank transaction active."); state = new(); Changed?.Invoke(); }
        public void RestoreJson(int version, string json)
        {
            if (busy) throw new InvalidOperationException("Bank transaction active.");
            if (version != Version) throw new NotSupportedException("Unsupported bank section; save preserved.");
            var candidate = JsonUtility.FromJson<State>(json);
            if (candidate == null || !CarriedInventory.Valid(candidate.items) || candidate.gold < 0 ||
                candidate.purchasedSlots < 0 || candidate.purchasedSlots > int.MaxValue - initialSlots ||
                candidate.items.Length > initialSlots + candidate.purchasedSlots)
                throw new InvalidDataException("Invalid bank records; save preserved.");
            state = candidate; Changed?.Invoke();
        }
        public bool Deposit(string id, out string error) => Transfer(id, false, out error);
        public bool Withdraw(string id, out string error) => Transfer(id, true, out error);
        private bool Transfer(string id, bool withdraw, out string error)
        {
            error = null;
            if (!Ready(out error)) return false;
            var inventory = GetComponent<CarriedInventory>();
            var carry = inventory.Items;
            var source = withdraw ? state.items : carry;
            var destination = withdraw ? carry : state.items;
            var item = source.SingleOrDefault(i => i.instanceId == id);
            if (item == null || destination.Any(i => i.instanceId == id)) return Reject("Item missing or already transferred.", out error);
            var definition = inventory.Resolve(item.definitionId);
            if (definition == null) return Reject("Item catalog entry unavailable; item retained.", out error);
            if (withdraw)
            {
                if (!TownAccess) return Reject("Withdraw at Peter Banker in Cornberg.", out error);
                if (definition.allowedClassIds?.Length > 0 && !definition.allowedClassIds.Contains(GetComponent<ActorStats>()?.Definition?.stableId))
                    return Reject("This equipment belongs to another class.", out error);
            }
            else
            {
                if (state.items.Length >= Capacity) return Reject("Bank full; buy more slots in town.", out error);
                if (GetComponent<Equipment>()?.IsEquipped(id) == true) return Reject("Unequip this item before depositing.", out error);
            }
            var remaining = source.Where(i => i.instanceId != id).Select(i => i.Copy()).ToArray();
            var added = destination.Concat(new[] { item.Copy() }).ToArray();
            var oldBank = state.items;
            busy = true;
            using (GetComponent<ManifestationPersistence>()?.DeferDurableWrites())
            {
                try
                {
                    inventory.ReplaceWithoutNotification(withdraw ? added : remaining);
                    state.items = withdraw ? remaining : added;
                }
                catch (Exception e)
                {
                    inventory.ReplaceWithoutNotification(carry); state.items = oldBank;
                    busy = false; return Reject(e.Message, out error);
                }
                try { Publish(inventory.NotifyChanged); Publish(() => Changed?.Invoke()); }
                finally { busy = false; }
            }
            return true;
        }
        public bool DepositGold(long amount, out string error) => Currency(amount, false, false, out error);
        public bool WithdrawGold(long amount, out string error) => Currency(amount, true, false, out error);
        public bool Expand(out string error) => Currency(ExpansionPrice, false, true, out error);
        private bool Currency(long amount, bool withdraw, bool expand, out string error)
        {
            error = null;
            if (!Ready(out error)) return false;
            if (!TownAccess) return Reject("Gold and capacity changes require Peter Banker in town.", out error);
            var wallet = GetComponent<GoldWallet>();
            if (amount <= 0) return Reject("Enter a positive gold amount.", out error);
            if (withdraw ? state.gold < amount || wallet.Gold > long.MaxValue - amount : !wallet.CanSpend(amount) || (!expand && state.gold > long.MaxValue - amount))
                return Reject("Insufficient gold or currency limit reached.", out error);
            if (expand && state.purchasedSlots > int.MaxValue - initialSlots - expansionSlots)
                return Reject("Capacity limit reached.", out error);
            busy = true;
            using (GetComponent<ManifestationPersistence>()?.DeferDurableWrites())
            {
                try
                {
                    bool success = withdraw ? wallet.CreditWithoutNotification(amount) : wallet.DebitWithoutNotification(amount);
                    if (!success) return Reject("Wallet transfer failed.", out error);
                    if (expand) state.purchasedSlots += expansionSlots;
                    else state.gold += withdraw ? -amount : amount;
                    Publish(wallet.NotifyChanged); Publish(() => Changed?.Invoke());
                }
                finally { busy = false; }
            }
            return true;
        }
        private bool Ready(out string error)
        {
            error = null;
            if (busy) return Reject("Bank transaction already active.", out error);
            if (GetComponent<CombatActor>()?.Alive == false) return Reject("Cannot bank while defeated.", out error);
            if (GetComponent<ManifestationPersistence>() is { Ready: false }) return Reject("Save system is not ready.", out error);
            return true;
        }
        private static bool Reject(string message, out string error) { error = message; return false; }
        // A throwing observer must not turn an already committed transfer into a retry/duplicate.
        private static void Publish(Action action) { try { action(); } catch (Exception e) { Debug.LogException(e); } }
    }
}

using System;
using DiceFree.Foundation;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Items
{
    // One synchronous manifestation transaction. The caller supplies its completion-state commit/rollback.
    public static class FixedRewardGrant
    {
        public static bool TryGrant(GameObject recipient, int experience, FixedRewardDefinition reward,
            Action commit, Action rollback)
        {
            var xp = recipient.GetComponent<ExperienceProgression>();
            var inventory = recipient.GetComponent<CarriedInventory>();
            var wallet = recipient.GetComponent<GoldWallet>();
            if (xp == null || experience < 0) return false;
            if (reward != null)
            {
                if (inventory == null || wallet == null || reward.gold < 0 || reward.gold > long.MaxValue - wallet.Gold || reward.items == null) return false;
                foreach (var item in reward.items)
                    if (item == null || inventory.Resolve(item.stableId) == null) return false;
            }
            int level = xp.Level, priorXp = xp.CurrentXp;
            long gold = wallet == null ? 0 : wallet.Gold;
            var items = inventory?.Items;
            using var scope = recipient.GetComponent<IDurableMutationCoordinator>()?.DeferDurableWrites();
            try
            {
                commit(); // Blocks repeated turn-in before any reward event is published.
                if (reward != null)
                {
                    foreach (var item in reward.items) FixedItemGrant.Grant(inventory, item);
                    wallet.Grant(reward.gold);
                }
                xp.Grant(experience);
                return true;
            }
            catch
            {
                // Preserve the pre-transaction durable model if a downstream event handler fails.
                rollback();
                xp.RestoreState(level, priorXp);
                if (reward != null) { inventory.Restore(items); wallet.Restore(gold); }
                throw;
            }
        }
    }
}

using DiceFree.Combat;
using UnityEngine;
namespace DiceFree.Quests
{
    public enum QuestCreditMode { Owner, NearbyParty }
    public interface IQuestPartyMembership { bool SameParty(CombatActor other); }
    public static class QuestCreditPolicy
    {
        public static bool Eligible(QuestLeaf objective, CombatActor recipient, ActorDefeated fact)
        {
            if (fact.creditOwner == null || recipient == null || recipient == fact.victim) return false;
            if (recipient == fact.creditOwner) return true;
            if (objective.creditMode != QuestCreditMode.NearbyParty) return false;
            var party = recipient.GetComponent<IQuestPartyMembership>();
            // Inclusive Euclidean radius around defeat position, not a camera/ground-plane distance.
            return party != null && party.SameParty(fact.creditOwner) &&
                (recipient.transform.position - fact.position).sqrMagnitude <= objective.creditRadius * objective.creditRadius;
        }
    }
}

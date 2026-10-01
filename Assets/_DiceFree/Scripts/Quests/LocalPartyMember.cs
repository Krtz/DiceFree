using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Quests
{
    // Local membership only; network/session adapters can implement IQuestPartyMembership later.
    public sealed class LocalPartyMember : MonoBehaviour, IQuestPartyMembership
    {
        public string partyId;
        public bool SameParty(CombatActor other) => !string.IsNullOrEmpty(partyId) && other != null &&
            other.TryGetComponent<LocalPartyMember>(out var member) && member.partyId == partyId;
    }
}

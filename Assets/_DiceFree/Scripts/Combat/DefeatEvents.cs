using System;
using UnityEngine;

namespace DiceFree.Combat
{
    public readonly struct ActorDefeated
    {
        public readonly long sequence;
        public readonly string contentId, familyId;
        public readonly CombatActor victim, killer, creditOwner;
        public readonly Vector3 position;
        public readonly int experience;
        public ActorDefeated(long id, CombatActor defeated, CombatActor source, CombatActor owner)
        {
            sequence = id; victim = defeated; killer = source; creditOwner = owner;
            contentId = defeated.Stats.Definition.stableId; familyId = defeated.Stats.Definition.familyId;
            position = defeated.transform.position; experience = defeated.Stats.Definition.experienceReward;
        }
    }

    // Local simulation authority. A future host/credit policy can replace this boundary without changing attacks.
    public static class DefeatEvents
    {
        private static long sequence;
        public static event Action<ActorDefeated> Reported;
        internal static void Report(CombatActor victim, CombatActor killer, CombatActor owner) =>
            Reported?.Invoke(new ActorDefeated(++sequence, victim, killer, owner));
    }
}

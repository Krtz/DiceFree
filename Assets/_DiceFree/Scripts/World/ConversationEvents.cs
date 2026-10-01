using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public readonly struct ConversationCompleted
    {
        public readonly long sequence;
        public readonly string npcId, conversationId;
        public readonly CombatActor actor;
        public readonly Vector3 position;
        public ConversationCompleted(long sequence, string npcId, string conversationId, CombatActor actor)
        { this.sequence = sequence; this.npcId = npcId; this.conversationId = conversationId; this.actor = actor; position = actor.transform.position; }
    }
    public static class ConversationEvents
    {
        private static long sequence;
        public static event Action<ConversationCompleted> Completed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionLifetime() { sequence = 0; Completed = null; }
        internal static void Report(string npc, string conversation, CombatActor actor) =>
            Completed?.Invoke(new ConversationCompleted(++sequence, npc, conversation, actor));
    }
}

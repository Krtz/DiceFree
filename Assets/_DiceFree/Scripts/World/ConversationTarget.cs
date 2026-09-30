using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public interface IConversationAvailability { bool Available(CombatActor actor); }

    // Reports an acknowledged conversation, not a click or a quest completion.
    public sealed class ConversationTarget : InteractionTarget
    {
        [SerializeField] private string npcId, conversationId;
        [SerializeField, TextArea] private string dialogue;
        private CombatActor listener;
        private bool acknowledged;
        public string NpcId => npcId;
        public string ConversationId => conversationId;
        public string Dialogue => dialogue;
        public bool Available(CombatActor actor)
        {
            if (actor == null) return false;
            foreach (var component in GetComponents<MonoBehaviour>())
                if (component is IConversationAvailability gate && !gate.Available(actor)) return false;
            return true;
        }
        public override bool CanInteract(CombatActor actor) => Available(actor) && base.CanInteract(actor);
        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            listener = actor; acknowledged = false;
        }
        public bool Acknowledge(CombatActor actor)
        {
            if (actor != listener || acknowledged || !CanInteract(actor) || actor.GetComponent<Interactor>()?.Active != this) return false;
            acknowledged = true;
            ConversationEvents.Report(npcId, conversationId, actor);
            return true;
        }
        public void Configure(string npc, string conversation, string text)
        { npcId = npc; conversationId = conversation; dialogue = text; }
    }
}

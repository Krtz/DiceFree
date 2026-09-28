using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Quests
{
    public sealed class QuestGiver : InteractionTarget
    {
        [SerializeField] private QuestDefinition quest;
        public QuestDefinition Quest => quest;
        public bool Accept(CombatActor actor) => CanInteract(actor) && actor.GetComponent<QuestJournal>() != null &&
            actor.GetComponent<QuestJournal>().Accept(quest);
        public bool TurnIn(CombatActor actor) => CanInteract(actor) && actor.GetComponent<QuestJournal>() != null &&
            actor.GetComponent<QuestJournal>().TurnIn(quest);
        public void Configure(QuestDefinition value) => quest = value;
    }
}

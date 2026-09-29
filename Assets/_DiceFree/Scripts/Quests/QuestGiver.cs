using DiceFree.Combat;
using DiceFree.World;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Quests
{
    public sealed class QuestGiver : InteractionTarget
    {
        [SerializeField] private QuestDefinition quest;
        [SerializeField] private QuestDefinition[] additionalQuests = Array.Empty<QuestDefinition>();
        public QuestDefinition Quest => quest;
        public IEnumerable<QuestDefinition> Offers
        {
            get { yield return quest; foreach (var value in additionalQuests) yield return value; }
        }
        public QuestDefinition CurrentQuest(CombatActor actor)
        {
            var journal = actor.GetComponent<QuestJournal>();
            foreach (var value in Offers)
            {
                var state = journal.GetProgress(value.stableId);
                if (state != null && (state.status == QuestStatus.Active || state.status == QuestStatus.ReadyToTurnIn)) return value;
            }
            foreach (var value in Offers) if (journal.CanAccept(value)) return value;
            return quest;
        }
        public bool Accept(CombatActor actor) => Accept(actor, CurrentQuest(actor));
        public bool TurnIn(CombatActor actor) => TurnIn(actor, CurrentQuest(actor));
        public bool Accept(CombatActor actor, QuestDefinition value) => Offered(value) && CanInteract(actor) &&
            actor.GetComponent<QuestJournal>().Accept(value);
        public bool TurnIn(CombatActor actor, QuestDefinition value) => Offered(value) && CanInteract(actor) &&
            actor.GetComponent<QuestJournal>().TurnIn(value);
        private bool Offered(QuestDefinition value) => value != null && (value == quest || Array.IndexOf(additionalQuests, value) >= 0);
        public void Configure(QuestDefinition value) => quest = value;
        public void ConfigureAdditional(params QuestDefinition[] values) => additionalQuests = values;
    }
}

using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Quests
{
    // Optional quest adapter; generic conversations have no quest IDs or journal dependency.
    public sealed class QuestConversationGate : MonoBehaviour, IConversationAvailability
    {
        [SerializeField] private QuestDefinition beat;
        [SerializeField] private CombatActor localPlayer;
        [SerializeField] private GameObject presentation;
        public bool Available(CombatActor actor)
        {
            var journal = actor != null ? actor.GetComponent<QuestJournal>() : null;
            var state = journal != null ? journal.GetProgress(beat.stableId) : null;
            return state != null && state.definitionVersion == beat.version && journal.PrerequisitesMet(beat);
        }
        private void Update()
        {
            if (presentation != null) presentation.SetActive(Available(localPlayer));
        }
        public void Configure(QuestDefinition value, CombatActor player, GameObject body)
        { beat = value; localPlayer = player; presentation = body; }
    }
}

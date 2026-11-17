using System.Linq;
using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Quests
{
    public sealed class QuestWorkGate : MonoBehaviour,IWorkAvailability
    {
        [SerializeField] QuestDefinition quest;
        [SerializeField] string objectiveId;
        public QuestDefinition Quest=>quest;
        public string ObjectiveId=>objectiveId;
        public bool Completed(QuestJournal journal)
        {
            var state=journal?.GetProgress(quest.stableId);
            return state!=null&&state.definitionVersion==quest.version&&
                (state.stage>0||state.status is QuestStatus.Completed or QuestStatus.ReadyToTurnIn||ObjectiveProgress.Count(state,objectiveId)>0);
        }
        public bool WorkAvailable(CombatActor actor)
        {
            if (actor == null || (!string.IsNullOrEmpty(quest.requiredClassId) &&
                actor.Stats.Definition.stableId != quest.requiredClassId)) return false;
            var journal=actor.GetComponent<QuestJournal>();var state=journal?.GetProgress(quest.stableId);
            if (state==null || state.definitionVersion!=quest.version || state.status!=QuestStatus.Active ||
                state.stage!=0 || Completed(journal)) return false;
            var objective=quest.stages[0];
            return objective.kind==ObjectiveKind.Interact && objective.objectiveId==objectiveId ||
                objective.alternatives.Any(leaf=>leaf.objectiveId==objectiveId);
        }
        public void Configure(QuestDefinition value,string id){quest=value;objectiveId=id;}
    }
}

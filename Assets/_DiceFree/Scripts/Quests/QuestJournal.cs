using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Quests
{
    public enum QuestStatus { Available, Active, ReadyToTurnIn, Completed }
    [Serializable]
    public sealed class QuestProgress
    {
        public string questId;
        public int definitionVersion;
        public QuestStatus status;
        public int stage, count;
        public QuestProgress Copy() => (QuestProgress)MemberwiseClone();
    }

    [DisallowMultipleComponent, RequireComponent(typeof(KillCreditReceiver), typeof(ExperienceProgression))]
    public sealed class QuestJournal : MonoBehaviour
    {
        [SerializeField] private QuestDefinition[] definitions;
        [SerializeField] private List<QuestProgress> progress = new();
        private KillCreditReceiver credit;
        private CombatActor owner;
        public IReadOnlyList<QuestDefinition> Definitions => definitions;
        public event Action Changed;
        private void Awake()
        {
            credit = GetComponent<KillCreditReceiver>();
            owner = GetComponent<CombatActor>();
            foreach (var definition in definitions)
                if (Find(definition.stableId) == null) progress.Add(new QuestProgress {
                    questId = definition.stableId, definitionVersion = definition.version
                });
        }
        private void OnEnable() { credit.Credited += OnCredit; AreaEvents.Entered += OnArea; }
        private void OnDisable() { credit.Credited -= OnCredit; AreaEvents.Entered -= OnArea; }
        private QuestProgress Find(string id) => progress.Find(value => value.questId == id);
        public QuestProgress GetProgress(string id) => Find(id)?.Copy();
        // Detached stable-ID records form the future persistence seam; no scene references or UI names.
        public QuestProgress[] CaptureState() => progress.ConvertAll(value => value.Copy()).ToArray();
        public bool CanRestore(QuestProgress[] records)
        {
            if (records == null) return false;
            var ids = new HashSet<string>();
            foreach (var record in records)
            {
                if (record == null || string.IsNullOrEmpty(record.questId) || !ids.Add(record.questId)) return false;
                var definition = Array.Find(definitions, value => value.stableId == record.questId);
                // Unresolved definitions are retained inertly, including their original version.
                if (definition == null || record.definitionVersion != definition.version) continue;
                if (!Enum.IsDefined(typeof(QuestStatus), record.status) || record.stage < 0 ||
                    record.stage >= definition.stages.Length || record.count < 0) return false;
                if (record.status == QuestStatus.Available && (record.stage != 0 || record.count != 0)) return false;
                if (record.status == QuestStatus.Active && record.count >= definition.stages[record.stage].count) return false;
                if ((record.status == QuestStatus.ReadyToTurnIn || record.status == QuestStatus.Completed) &&
                    (record.stage != definition.stages.Length - 1 || record.count != definition.stages[record.stage].count)) return false;
            }
            return true;
        }
        public void RestoreState(QuestProgress[] records)
        {
            if (!CanRestore(records)) throw new ArgumentException("Invalid saved quest state.");
            progress = new List<QuestProgress>();
            foreach (var record in records) progress.Add(record.Copy());
            foreach (var definition in definitions)
                if (Find(definition.stableId) == null) progress.Add(new QuestProgress {
                    questId = definition.stableId, definitionVersion = definition.version
                });
            Changed?.Invoke();
        }
        public bool PrerequisitesMet(QuestDefinition definition)
        {
            foreach (var id in definition.completedQuestIds ?? Array.Empty<string>())
            {
                var required = Array.Find(definitions, value => value.stableId == id);
                var state = Find(id);
                if (required == null || state == null || state.definitionVersion != required.version || state.status != QuestStatus.Completed) return false;
            }
            return true;
        }
        public bool CanAccept(QuestDefinition definition)
        {
            if (definition == null || Array.IndexOf(definitions,definition) < 0) return false;
            var state = Find(definition.stableId);
            return state != null && state.definitionVersion == definition.version && state.status == QuestStatus.Available && PrerequisitesMet(definition);
        }
        public bool Accept(QuestDefinition definition)
        {
            if (!CanAccept(definition)) return false;
            var state = Find(definition.stableId);
            state.status = QuestStatus.Active; state.stage = state.count = 0; Changed?.Invoke(); return true;
        }
        private void OnCredit(ActorDefeated defeat) => Advance(ObjectiveKind.Kill, defeat.contentId, defeat.familyId, null);
        private void OnArea(AreaEntered entered)
        {
            if (entered.actor == owner) Advance(ObjectiveKind.ReachArea, null, null, entered.areaId);
        }
        private void Advance(ObjectiveKind kind, string content, string family, string area)
        {
            foreach (var definition in definitions)
            {
                var state = Find(definition.stableId);
                if (state.definitionVersion != definition.version || state.status != QuestStatus.Active) continue;
                var objective = definition.stages[state.stage];
                if (objective.kind != kind ||
                    (!string.IsNullOrEmpty(objective.contentId) && objective.contentId != content) ||
                    (!string.IsNullOrEmpty(objective.familyId) && objective.familyId != family) ||
                    (kind == ObjectiveKind.ReachArea && objective.areaId != area)) continue;
                state.count++;
                if (state.count >= objective.count)
                {
                    if (state.stage + 1 < definition.stages.Length) { state.stage++; state.count = 0; }
                    else state.status = QuestStatus.ReadyToTurnIn;
                }
                Changed?.Invoke();
            }
        }
        public bool TurnIn(QuestDefinition definition)
        {
            if (definition == null || Array.IndexOf(definitions,definition) < 0) return false;
            var state = Find(definition.stableId);
            if (state == null || state.definitionVersion != definition.version || state.status != QuestStatus.ReadyToTurnIn) return false;
            state.status = QuestStatus.Completed; // Commit state before publishing the reward/events.
            GetComponent<ExperienceProgression>().Grant(definition.rewardXp);
            Changed?.Invoke(); return true;
        }
        public void Configure(params QuestDefinition[] values) => definitions = values;
    }
}

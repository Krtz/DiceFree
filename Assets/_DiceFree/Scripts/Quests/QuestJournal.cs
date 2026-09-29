using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Progression;
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
        public IReadOnlyList<QuestDefinition> Definitions => definitions;
        public event Action Changed;
        private void Awake()
        {
            credit = GetComponent<KillCreditReceiver>();
            foreach (var definition in definitions)
                if (Find(definition.stableId) == null) progress.Add(new QuestProgress {
                    questId = definition.stableId, definitionVersion = definition.version
                });
        }
        private void OnEnable() => credit.Credited += OnCredit;
        private void OnDisable() => credit.Credited -= OnCredit;
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
        public bool Accept(QuestDefinition definition)
        {
            if (definition == null || Array.IndexOf(definitions,definition) < 0) return false;
            var state = Find(definition.stableId);
            if (state == null || state.definitionVersion != definition.version || state.status != QuestStatus.Available) return false;
            state.status = QuestStatus.Active; state.stage = state.count = 0; Changed?.Invoke(); return true;
        }
        private void OnCredit(ActorDefeated defeat)
        {
            foreach (var definition in definitions)
            {
                var state = Find(definition.stableId);
                if (state.definitionVersion != definition.version || state.status != QuestStatus.Active) continue;
                var objective = definition.stages[state.stage];
                if ((!string.IsNullOrEmpty(objective.contentId) && objective.contentId != defeat.contentId) ||
                    (!string.IsNullOrEmpty(objective.familyId) && objective.familyId != defeat.familyId)) continue;
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

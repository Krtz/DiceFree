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
        public bool Accept(QuestDefinition definition)
        {
            if (definition == null || Array.IndexOf(definitions,definition) < 0) return false;
            var state = Find(definition.stableId);
            if (state == null || state.status != QuestStatus.Available) return false;
            state.status = QuestStatus.Active; state.stage = state.count = 0; Changed?.Invoke(); return true;
        }
        private void OnCredit(ActorDefeated defeat)
        {
            foreach (var definition in definitions)
            {
                var state = Find(definition.stableId);
                if (state.status != QuestStatus.Active) continue;
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
            if (state == null || state.status != QuestStatus.ReadyToTurnIn) return false;
            state.status = QuestStatus.Completed; // Commit state before publishing the reward/events.
            GetComponent<ExperienceProgression>().Grant(definition.rewardXp);
            Changed?.Invoke(); return true;
        }
        public void Configure(params QuestDefinition[] values) => definitions = values;
    }
}

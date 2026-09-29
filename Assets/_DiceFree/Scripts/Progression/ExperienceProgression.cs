using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Progression
{
    [DisallowMultipleComponent, RequireComponent(typeof(ActorStats), typeof(KillCreditReceiver))]
    public sealed class ExperienceProgression : MonoBehaviour
    {
        [SerializeField] private ExperienceCurve curve;
        [SerializeField] private int currentXp;
        private ActorStats stats;
        private KillCreditReceiver credit;
        public int CurrentXp => currentXp;
        public int RequiredXp => curve.ToNextLevel(stats.Level);
        public int Level => stats.Level;
        public event Action Changed;
        public event Action<int> LeveledUp;
        public bool CanRestore(int level, int xp) => level >= 1 && level <= curve.levelLimit &&
            xp >= 0 && (level == curve.levelLimit ? xp == 0 : xp < curve.ToNextLevel(level));
        public void RestoreState(int level, int xp)
        {
            if (!CanRestore(level, xp)) throw new ArgumentException("Invalid saved level/XP.");
            stats.SetLevel(level);
            currentXp = xp;
            Changed?.Invoke(); // Loading never grants XP or replays level-up rewards.
        }
        private void Awake() { stats = GetComponent<ActorStats>(); credit = GetComponent<KillCreditReceiver>(); }
        private void OnEnable() => credit.Credited += OnCredit;
        private void OnDisable() => credit.Credited -= OnCredit;
        private void OnCredit(ActorDefeated defeat) => Grant(defeat.experience);
        public void Grant(int amount)
        {
            if (amount <= 0 || stats.Level >= curve.levelLimit) return;
            int startingLevel = stats.Level;
            long pool = (long)currentXp + amount;
            while (pool >= RequiredXp && stats.Level < curve.levelLimit)
            {
                pool -= RequiredXp;
                stats.SetLevel(stats.Level + 1);
            }
            currentXp = stats.Level >= curve.levelLimit ? 0 : (int)pool;
            int endingLevel = stats.Level;
            for (int level = startingLevel + 1; level <= endingLevel; level++) LeveledUp?.Invoke(level);
            Changed?.Invoke();
        }
        public void Configure(ExperienceCurve value) => curve = value;
    }
}

using System;
using System.Collections.Generic;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(ActorStats))]
    public sealed class ResurrectionSickness : MonoBehaviour, IEchoWideDurableState
    {
        private static readonly long Duration = TimeSpan.FromMinutes(5).Ticks;
        [Serializable] private sealed class Save { public long[] expirations = Array.Empty<long>(); }
        private readonly List<long> expirations = new();
        private ActorStats stats;
        public int Stacks => expirations.Count;
        public string SectionId => "echo.resurrection-sickness";
        public int Version => 1;
        public event Action Changed;

        private void Awake() => stats = GetComponent<ActorStats>();
        private void Update()
        {
            if (expirations.Count > 0 && expirations[0] <= DateTime.UtcNow.Ticks)
                Expire(DateTime.UtcNow.Ticks, true);
        }

        public void AddStack()
        {
            long now = DateTime.UtcNow.Ticks;
            Expire(now, false);
            expirations.Add(now + Duration);
            expirations.Sort();
            stats.SetResurrectionSicknessStacks(Stacks);
            Changed?.Invoke();
        }

        public string CaptureJson() => JsonUtility.ToJson(new Save { expirations = expirations.ToArray() });

        public void RestoreJson(int version, string json)
        {
            if (version != Version) throw new NotSupportedException("Unsupported resurrection sickness save version.");
            var state = JsonUtility.FromJson<Save>(json);
            if (state?.expirations == null) throw new ArgumentException("Invalid resurrection sickness save.");
            long now = DateTime.UtcNow.Ticks;
            expirations.Clear();
            foreach (long expiry in state.expirations)
                if (expiry > now && expiry <= DateTime.MaxValue.Ticks) expirations.Add(expiry);
            expirations.Sort();
            stats.SetResurrectionSicknessStacks(Stacks);
        }

        public void ResetToDefault()
        {
            expirations.Clear();
            stats.SetResurrectionSicknessStacks(0);
        }

        private void Expire(long now, bool notify)
        {
            if (expirations.RemoveAll(expiry => expiry <= now) == 0) return;
            stats.SetResurrectionSicknessStacks(Stacks);
            if (notify) Changed?.Invoke();
        }
    }
}

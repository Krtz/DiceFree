using System;
using System.Collections.Generic;

namespace DiceFree.Gameplay
{
    public interface ISemanticGameEvent { }

    public readonly struct MonsterKilledEvent : ISemanticGameEvent
    {
        public readonly string monsterId;
        public MonsterKilledEvent(string value) => monsterId = value;
    }

    public readonly struct DropObservedEvent : ISemanticGameEvent
    {
        public readonly string sourceEntryId;
        public readonly string dropId;
        public readonly bool dungeonLoot;

        public DropObservedEvent(string sourceEntryId, string dropId, bool dungeonLoot = false)
        {
            this.sourceEntryId = sourceEntryId;
            this.dropId = dropId;
            this.dungeonLoot = dungeonLoot;
        }
    }

    public readonly struct BossMechanicWitnessedEvent : ISemanticGameEvent
    {
        public readonly string bossId;
        public readonly string mechanicId;

        public BossMechanicWitnessedEvent(string bossId, string mechanicId)
        {
            this.bossId = bossId;
            this.mechanicId = mechanicId;
        }
    }

    public readonly struct DungeonWipedEvent : ISemanticGameEvent
    {
        public readonly string dungeonId;
        public DungeonWipedEvent(string value) => dungeonId = value;
    }

    public readonly struct DungeonPlayerDiedEvent : ISemanticGameEvent
    {
        public readonly string dungeonId;
        public DungeonPlayerDiedEvent(string value) => dungeonId = value;
    }

    public readonly struct DungeonBossKilledEvent : ISemanticGameEvent
    {
        public readonly string dungeonId;
        public readonly string bossId;

        public DungeonBossKilledEvent(string dungeonId, string bossId)
        {
            this.dungeonId = dungeonId;
            this.bossId = bossId;
        }
    }

    public readonly struct DungeonSecretDiscoveredEvent : ISemanticGameEvent
    {
        public readonly string dungeonId;
        public readonly string secretId;

        public DungeonSecretDiscoveredEvent(string dungeonId, string secretId)
        {
            this.dungeonId = dungeonId;
            this.secretId = secretId;
        }
    }

    public readonly struct DungeonCompletedEvent : ISemanticGameEvent
    {
        public readonly string dungeonId;
        public readonly string modeId;
        public readonly int partySize;
        public readonly float durationSeconds;
        public readonly bool noPlayerDeaths;

        public DungeonCompletedEvent(
            string dungeonId,
            string modeId,
            int partySize,
            float durationSeconds,
            bool noPlayerDeaths)
        {
            this.dungeonId = dungeonId;
            this.modeId = modeId;
            this.partySize = partySize;
            this.durationSeconds = durationSeconds;
            this.noPlayerDeaths = noPlayerDeaths;
        }
    }

    /// <summary>
    /// Session-owned semantic event stream. It is deliberately not static so hosted
    /// sessions/tests can own separate event domains.
    /// </summary>
    public sealed class SemanticGameEventHub
    {
        private readonly Dictionary<Type, List<Delegate>> listeners = new();

        public IDisposable Subscribe<T>(Action<T> listener) where T : struct, ISemanticGameEvent
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            Type type = typeof(T);
            if (!listeners.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                listeners.Add(type, list);
            }
            list.Add(listener);
            return new Subscription<T>(this, listener);
        }

        public void Publish<T>(T value) where T : struct, ISemanticGameEvent
        {
            if (!listeners.TryGetValue(typeof(T), out var list)) return;
            var snapshot = list.ToArray();
            foreach (var listener in snapshot)
                ((Action<T>)listener)(value);
        }

        private void Unsubscribe<T>(Action<T> listener) where T : struct, ISemanticGameEvent
        {
            if (!listeners.TryGetValue(typeof(T), out var list)) return;
            list.Remove(listener);
            if (list.Count == 0) listeners.Remove(typeof(T));
        }

        private sealed class Subscription<T> : IDisposable where T : struct, ISemanticGameEvent
        {
            private SemanticGameEventHub owner;
            private Action<T> listener;

            public Subscription(SemanticGameEventHub owner, Action<T> listener)
            {
                this.owner = owner;
                this.listener = listener;
            }

            public void Dispose()
            {
                if (owner == null) return;
                owner.Unsubscribe(listener);
                owner = null;
                listener = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Gameplay
{
    [Serializable]
    public sealed class CodexDropRecord
    {
        public string dropId;
        public int timesSeen;
    }

    [Serializable]
    public sealed class CodexMechanicRecord
    {
        public string mechanicId;
    }

    [Serializable]
    public sealed class CodexMonsterRecord
    {
        public string monsterId;
        public int kills;
        public CodexDropRecord[] drops = Array.Empty<CodexDropRecord>();
        public CodexMechanicRecord[] mechanics = Array.Empty<CodexMechanicRecord>();
    }

    [Serializable]
    public sealed class CodexBossKillRecord
    {
        public string bossId;
        public int kills;
    }

    [Serializable]
    public sealed class CodexFastestClearRecord
    {
        public int partySize;
        public float seconds;
    }

    [Serializable]
    public sealed class CodexDungeonRecord
    {
        public string dungeonId;
        public int clears;
        public int wipes;
        public int playerDeaths;
        public int soloClears;
        public int noDeathClears;
        public string[] completedModes = Array.Empty<string>();
        public string[] lootDiscoveries = Array.Empty<string>();
        public string[] secretDiscoveries = Array.Empty<string>();
        public CodexBossKillRecord[] bossKills = Array.Empty<CodexBossKillRecord>();
        public CodexFastestClearRecord[] fastestClears = Array.Empty<CodexFastestClearRecord>();
    }

    [Serializable]
    public sealed class CodexSaveData
    {
        public CodexMonsterRecord[] monsters = Array.Empty<CodexMonsterRecord>();
        public CodexDungeonRecord[] dungeons = Array.Empty<CodexDungeonRecord>();
    }

    /// <summary>
    /// Echo-wide discovery/completion state. Content definitions remain authored elsewhere;
    /// this stores only what this Echo has legitimately discovered/recorded.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CodexProgression : MonoBehaviour, IEchoWideDurableState
    {
        public const string CodexSectionId = "echo:codex";
        public const int CodexVersion = 1;

        [SerializeField] private CodexSaveData state = new();

        private readonly List<IDisposable> subscriptions = new();

        public string SectionId => CodexSectionId;
        public int Version => CodexVersion;
        public event Action Changed;

        public CodexMonsterRecord[] Monsters =>
            state.monsters?.Where(value => value != null)
                .Select(CloneMonster).ToArray() ?? Array.Empty<CodexMonsterRecord>();

        public CodexDungeonRecord[] Dungeons =>
            state.dungeons?.Where(value => value != null)
                .Select(CloneDungeon).ToArray() ?? Array.Empty<CodexDungeonRecord>();

        public string CaptureJson() => JsonUtility.ToJson(state ?? new CodexSaveData());

        public void RestoreJson(int version, string json)
        {
            if (version != CodexVersion)
                throw new NotSupportedException("Unsupported Codex version " + version + ".");

            var restored = string.IsNullOrWhiteSpace(json)
                ? new CodexSaveData()
                : JsonUtility.FromJson<CodexSaveData>(json);
            state = restored ?? new CodexSaveData();
            Normalize();
        }

        public void ResetToDefault()
        {
            state = new CodexSaveData();
            Changed?.Invoke();
        }

        public void Bind(SemanticGameEventHub hub)
        {
            Unbind();
            if (hub == null) return;

            subscriptions.Add(hub.Subscribe<MonsterKilledEvent>(value =>
                RecordMonsterKill(value.monsterId)));
            subscriptions.Add(hub.Subscribe<DropObservedEvent>(value =>
            {
                if (value.dungeonLoot) RecordDungeonLoot(value.sourceEntryId, value.dropId);
                else RecordMonsterDrop(value.sourceEntryId, value.dropId);
            }));
            subscriptions.Add(hub.Subscribe<BossMechanicWitnessedEvent>(value =>
                RecordBossMechanic(value.bossId, value.mechanicId)));
            subscriptions.Add(hub.Subscribe<DungeonWipedEvent>(value =>
                RecordDungeonWipe(value.dungeonId)));
            subscriptions.Add(hub.Subscribe<DungeonPlayerDiedEvent>(value =>
                RecordDungeonPlayerDeath(value.dungeonId)));
            subscriptions.Add(hub.Subscribe<DungeonBossKilledEvent>(value =>
                RecordDungeonBossKill(value.dungeonId, value.bossId)));
            subscriptions.Add(hub.Subscribe<DungeonSecretDiscoveredEvent>(value =>
                RecordDungeonSecret(value.dungeonId, value.secretId)));
            subscriptions.Add(hub.Subscribe<DungeonCompletedEvent>(value =>
                RecordDungeonComplete(
                    value.dungeonId,
                    value.modeId,
                    value.partySize,
                    value.durationSeconds,
                    value.noPlayerDeaths)));
        }

        public void Unbind()
        {
            foreach (var subscription in subscriptions) subscription?.Dispose();
            subscriptions.Clear();
        }

        private void OnDestroy() => Unbind();

        public int RecordMonsterKill(string monsterId)
        {
            RequireId(monsterId, nameof(monsterId));
            var record = EnsureMonster(monsterId);
            record.kills++;
            Touch();
            return record.kills;
        }

        public int RecordMonsterDrop(string monsterId, string dropId)
        {
            RequireId(monsterId, nameof(monsterId));
            RequireId(dropId, nameof(dropId));
            var record = EnsureMonster(monsterId);
            var drops = record.drops?.Where(value => value != null).ToList() ?? new List<CodexDropRecord>();
            var drop = drops.FirstOrDefault(value => value.dropId == dropId);
            if (drop == null)
            {
                drop = new CodexDropRecord { dropId = dropId };
                drops.Add(drop);
            }

            drop.timesSeen++;
            record.drops = drops.OrderBy(value => value.dropId, StringComparer.Ordinal).ToArray();
            Touch();
            return drop.timesSeen;
        }

        public bool RecordBossMechanic(string bossId, string mechanicId)
        {
            RequireId(bossId, nameof(bossId));
            RequireId(mechanicId, nameof(mechanicId));
            var record = EnsureMonster(bossId);
            var mechanics = record.mechanics?.Where(value => value != null).ToList()
                            ?? new List<CodexMechanicRecord>();
            if (mechanics.Any(value => value.mechanicId == mechanicId)) return false;

            mechanics.Add(new CodexMechanicRecord { mechanicId = mechanicId });
            record.mechanics = mechanics.OrderBy(value => value.mechanicId, StringComparer.Ordinal).ToArray();
            Touch();
            return true;
        }

        public int RecordDungeonWipe(string dungeonId)
        {
            var record = EnsureDungeonChecked(dungeonId);
            record.wipes++;
            Touch();
            return record.wipes;
        }

        public int RecordDungeonPlayerDeath(string dungeonId)
        {
            var record = EnsureDungeonChecked(dungeonId);
            record.playerDeaths++;
            Touch();
            return record.playerDeaths;
        }

        public int RecordDungeonBossKill(string dungeonId, string bossId)
        {
            RequireId(bossId, nameof(bossId));
            var dungeon = EnsureDungeonChecked(dungeonId);
            var kills = dungeon.bossKills?.Where(value => value != null).ToList()
                        ?? new List<CodexBossKillRecord>();
            var boss = kills.FirstOrDefault(value => value.bossId == bossId);
            if (boss == null)
            {
                boss = new CodexBossKillRecord { bossId = bossId };
                kills.Add(boss);
            }

            boss.kills++;
            dungeon.bossKills = kills.OrderBy(value => value.bossId, StringComparer.Ordinal).ToArray();
            Touch();
            return boss.kills;
        }

        public bool RecordDungeonSecret(string dungeonId, string secretId)
        {
            RequireId(secretId, nameof(secretId));
            var dungeon = EnsureDungeonChecked(dungeonId);
            var secrets = new HashSet<string>(
                dungeon.secretDiscoveries?.Where(value => !string.IsNullOrWhiteSpace(value))
                ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            if (!secrets.Add(secretId)) return false;

            dungeon.secretDiscoveries = secrets.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Touch();
            return true;
        }

        public bool RecordDungeonLoot(string dungeonId, string lootId)
        {
            RequireId(lootId, nameof(lootId));
            var dungeon = EnsureDungeonChecked(dungeonId);
            var loot = new HashSet<string>(
                dungeon.lootDiscoveries?.Where(value => !string.IsNullOrWhiteSpace(value))
                ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            if (!loot.Add(lootId)) return false;

            dungeon.lootDiscoveries = loot.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Touch();
            return true;
        }

        public void RecordDungeonComplete(
            string dungeonId,
            string modeId,
            int partySize,
            float durationSeconds,
            bool noPlayerDeaths)
        {
            if (partySize < 1) throw new ArgumentOutOfRangeException(nameof(partySize));
            if (durationSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            var dungeon = EnsureDungeonChecked(dungeonId);
            dungeon.clears++;
            if (partySize == 1) dungeon.soloClears++;
            if (noPlayerDeaths) dungeon.noDeathClears++;

            if (!string.IsNullOrWhiteSpace(modeId))
            {
                var modes = new HashSet<string>(
                    dungeon.completedModes?.Where(value => !string.IsNullOrWhiteSpace(value))
                    ?? Array.Empty<string>(),
                    StringComparer.Ordinal) { modeId };
                dungeon.completedModes = modes.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            }

            var fastest = dungeon.fastestClears?.Where(value => value != null).ToList()
                          ?? new List<CodexFastestClearRecord>();
            var party = fastest.FirstOrDefault(value => value.partySize == partySize);
            if (party == null)
            {
                party = new CodexFastestClearRecord
                {
                    partySize = partySize,
                    seconds = durationSeconds
                };
                fastest.Add(party);
            }
            else if (party.seconds <= 0f || durationSeconds < party.seconds)
            {
                party.seconds = durationSeconds;
            }

            dungeon.fastestClears = fastest.OrderBy(value => value.partySize).ToArray();
            Touch();
        }

        private CodexMonsterRecord EnsureMonster(string monsterId)
        {
            Normalize();
            var monsters = state.monsters.ToList();
            var record = monsters.FirstOrDefault(value => value.monsterId == monsterId);
            if (record != null) return record;

            record = new CodexMonsterRecord { monsterId = monsterId };
            monsters.Add(record);
            state.monsters = monsters.OrderBy(value => value.monsterId, StringComparer.Ordinal).ToArray();
            return record;
        }

        private CodexDungeonRecord EnsureDungeonChecked(string dungeonId)
        {
            RequireId(dungeonId, nameof(dungeonId));
            Normalize();
            var dungeons = state.dungeons.ToList();
            var record = dungeons.FirstOrDefault(value => value.dungeonId == dungeonId);
            if (record != null) return record;

            record = new CodexDungeonRecord { dungeonId = dungeonId };
            dungeons.Add(record);
            state.dungeons = dungeons.OrderBy(value => value.dungeonId, StringComparer.Ordinal).ToArray();
            return record;
        }

        private void Normalize()
        {
            state ??= new CodexSaveData();
            state.monsters = state.monsters?.Where(value => value != null &&
                                    !string.IsNullOrWhiteSpace(value.monsterId))
                                 .GroupBy(value => value.monsterId, StringComparer.Ordinal)
                                 .Select(group => group.First())
                                 .OrderBy(value => value.monsterId, StringComparer.Ordinal)
                                 .ToArray()
                             ?? Array.Empty<CodexMonsterRecord>();
            state.dungeons = state.dungeons?.Where(value => value != null &&
                                    !string.IsNullOrWhiteSpace(value.dungeonId))
                                 .GroupBy(value => value.dungeonId, StringComparer.Ordinal)
                                 .Select(group => group.First())
                                 .OrderBy(value => value.dungeonId, StringComparer.Ordinal)
                                 .ToArray()
                             ?? Array.Empty<CodexDungeonRecord>();

            foreach (var monster in state.monsters)
            {
                monster.drops ??= Array.Empty<CodexDropRecord>();
                monster.mechanics ??= Array.Empty<CodexMechanicRecord>();
            }

            foreach (var dungeon in state.dungeons)
            {
                dungeon.completedModes ??= Array.Empty<string>();
                dungeon.lootDiscoveries ??= Array.Empty<string>();
                dungeon.secretDiscoveries ??= Array.Empty<string>();
                dungeon.bossKills ??= Array.Empty<CodexBossKillRecord>();
                dungeon.fastestClears ??= Array.Empty<CodexFastestClearRecord>();
            }
        }

        private void Touch() => Changed?.Invoke();

        private static void RequireId(string value, string parameter)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Codex IDs must be stable non-empty strings.", parameter);
        }

        private static CodexMonsterRecord CloneMonster(CodexMonsterRecord value) =>
            JsonUtility.FromJson<CodexMonsterRecord>(JsonUtility.ToJson(value));

        private static CodexDungeonRecord CloneDungeon(CodexDungeonRecord value) =>
            JsonUtility.FromJson<CodexDungeonRecord>(JsonUtility.ToJson(value));
    }
}

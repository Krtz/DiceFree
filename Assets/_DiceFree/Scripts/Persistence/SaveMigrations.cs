using System;
using DiceFree.Quests;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Persistence
{
    public sealed class SaveMigrationException : Exception
    {
        public SaveMigrationException(string message, Exception inner = null) : base(message, inner) { }
    }
    public static class SaveMigrations
    {
        public const int CurrentSchema = 7;
        public const int ManifestationVersion = 7;
        public const int ClassSkillRankMigrationVersion = 1;
        [Serializable] private sealed class ManifestationV1
        {
            public string classId;
            public int level, xp;
            public string anchorId;
            public float healthFraction;
            public QuestProgress[] quests;
        }
        // Each old save contains progress WITHIN a level, not lifetime XP.
        // Preserve the same fraction of the next level when upgrading all
        // manifestations, including inactive classes in the Echo roster.
        // These historical constants must never change in a later balance pass.
        public static int RebaseLegacyXp(int level, int oldXp)
        {
            if (level < 1 || level > 200 || oldXp < 0)
                throw new SaveMigrationException("Invalid legacy XP/level; original save preserved.");
            if (level == 200)
            {
                if (oldXp != 0)
                    throw new SaveMigrationException("Max-level manifestation cannot have XP.");
                return 0;
            }
            long priorThreshold = 30L + 10L * (level - 1);
            if (oldXp >= priorThreshold)
                throw new SaveMigrationException("Legacy XP exceeds its historical level threshold.");
            long step = level - 1;
            long nextThreshold = 30L + 3L * step * step;
            // Nearest whole XP maintains progress even for higher-level
            // characters while never manufacturing a free level-up.
            return (int)System.Math.Min(nextThreshold - 1,
                ((long)oldXp * nextThreshold + priorThreshold / 2) / priorThreshold);
        }

        public static EchoSave Upgrade(EchoSave source)
        {
            if (source.schemaVersion < 1 || source.schemaVersion > CurrentSchema)
                throw new NotSupportedException("Unsupported Echo schema " + source.schemaVersion);
            // Never partially mutate the loaded source if any record fails migration.
            var result = JsonUtility.FromJson<EchoSave>(JsonUtility.ToJson(source));
            if (result.schemaVersion == CurrentSchema) return result;
            try
            {
                foreach (var section in result.sections)
                {
                    if (!section.id.StartsWith("manifestation:", StringComparison.Ordinal)) continue;
                    if (section.version >= 2 && section.version <= 6)
                    {
                        var previous = JsonUtility.FromJson<ManifestationSave>(section.json);
                        if (previous == null || section.id != "manifestation:" + previous.classId || previous.level < 1 || previous.xp < 0 || previous.quests == null)
                            throw new SaveMigrationException("Invalid previous manifestation " + section.id);
                        if (section.version == 2)
                        {
                            previous.inventory = Array.Empty<DiceFree.Items.ItemInstance>();
                            previous.equipment = Array.Empty<DiceFree.Items.EquippedItem>(); previous.gold = 0;
                        }
                        if (previous.classSkills == null) previous.classSkills = Array.Empty<SkillRankState>();
                        if (section.version < 4)
                        {
                            foreach (var quest in previous.quests)
                            {
                                if (quest == null) throw new SaveMigrationException("Null previous quest record.");
                                quest.alternatives = Array.Empty<ObjectiveCount>();
                            }
                        }
                        if (section.version < 6) previous.xp = RebaseLegacyXp(previous.level, previous.xp);
                        ApplyClassSkillRankMigration(previous, section.version);
                        section.json = JsonUtility.ToJson(previous); section.version = ManifestationVersion;
                        continue;
                    }
                    if (section.version != 1) continue;
                    var old = JsonUtility.FromJson<ManifestationV1>(section.json);
                    if (old == null || string.IsNullOrEmpty(old.classId) || section.id != "manifestation:" + old.classId ||
                        old.level < 1 || old.xp < 0 || old.quests == null)
                        throw new SaveMigrationException("Invalid v1 manifestation " + section.id);
                    var current = new ManifestationSave {
                        classId = old.classId, level = old.level, xp = RebaseLegacyXp(old.level, old.xp), anchorId = old.anchorId, quests = old.quests,
                        classSkills = Array.Empty<SkillRankState>()
                    };
                    ApplyClassSkillRankMigration(current, section.version);
                    foreach (var quest in current.quests)
                    {
                        if (quest == null) throw new SaveMigrationException("Null v1 quest record.");
                        quest.alternatives = Array.Empty<ObjectiveCount>();
                    }
                    // HP was session state. It is deliberately retired, not translated into a resource.
                    section.json = JsonUtility.ToJson(current);
                    section.version = ManifestationVersion;
                }
                result.schemaVersion = CurrentSchema;
                return result;
            }
            catch (SaveMigrationException) { throw; }
            catch (Exception error) { throw new SaveMigrationException("Echo migration to v7 failed; original preserved.", error); }
        }

        public static bool RequiresClassSkillRankReset(EchoSave source)
        {
            if (source?.sections == null) return false;
            foreach (var section in source.sections)
            {
                if (section == null || section.version > 6 ||
                    !section.id.StartsWith("manifestation:", StringComparison.Ordinal)) continue;
                var state = JsonUtility.FromJson<ManifestationSave>(section.json);
                if (state != null && IsRankResetClass(state.classId) &&
                    state.classSkillRankMigrationVersion < ClassSkillRankMigrationVersion)
                    return true;
            }
            return false;
        }

        private static void ApplyClassSkillRankMigration(ManifestationSave state, int previousVersion)
        {
            if (state == null || previousVersion > 6 || !IsRankResetClass(state.classId) ||
                state.classSkillRankMigrationVersion >= ClassSkillRankMigrationVersion) return;

            string prefix = state.classId == "class.greatblade" ? "skill.greatblade." : "skill.mage.";
            foreach (var rank in state.classSkills ?? Array.Empty<SkillRankState>())
                if (rank != null && rank.stableId != null && rank.stableId.StartsWith(prefix, StringComparison.Ordinal))
                    rank.rank = 0;
            state.classSkills ??= Array.Empty<SkillRankState>();
            state.classSkillRankMigrationVersion = ClassSkillRankMigrationVersion;
        }

        private static bool IsRankResetClass(string classId) =>
            classId == "class.greatblade" || classId == "class.mage";
    }
}

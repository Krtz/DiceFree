using System;
using DiceFree.Quests;
using UnityEngine;

namespace DiceFree.Persistence
{
    public sealed class SaveMigrationException : Exception
    {
        public SaveMigrationException(string message, Exception inner = null) : base(message, inner) { }
    }
    public static class SaveMigrations
    {
        public const int CurrentSchema = 3;
        public const int ManifestationVersion = 3;
        [Serializable] private sealed class ManifestationV1
        {
            public string classId;
            public int level, xp;
            public string anchorId;
            public float healthFraction;
            public QuestProgress[] quests;
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
                    if (section.version == 2)
                    {
                        var previous = JsonUtility.FromJson<ManifestationSave>(section.json);
                        if (previous == null || section.id != "manifestation:" + previous.classId || previous.level < 1 || previous.xp < 0 || previous.quests == null)
                            throw new SaveMigrationException("Invalid v2 manifestation " + section.id);
                        previous.inventory = Array.Empty<DiceFree.Items.ItemInstance>();
                        previous.equipment = Array.Empty<DiceFree.Items.EquippedItem>(); previous.gold = 0;
                        section.json = JsonUtility.ToJson(previous); section.version = ManifestationVersion;
                        continue;
                    }
                    if (section.version != 1) continue;
                    var old = JsonUtility.FromJson<ManifestationV1>(section.json);
                    if (old == null || string.IsNullOrEmpty(old.classId) || section.id != "manifestation:" + old.classId ||
                        old.level < 1 || old.xp < 0 || old.quests == null)
                        throw new SaveMigrationException("Invalid v1 manifestation " + section.id);
                    var current = new ManifestationSave {
                        classId = old.classId, level = old.level, xp = old.xp, anchorId = old.anchorId, quests = old.quests
                    };
                    // HP was session state. It is deliberately retired, not translated into a resource.
                    section.json = JsonUtility.ToJson(current);
                    section.version = ManifestationVersion;
                }
                result.schemaVersion = CurrentSchema;
                return result;
            }
            catch (SaveMigrationException) { throw; }
            catch (Exception error) { throw new SaveMigrationException("Echo migration to v3 failed; original preserved.", error); }
        }
    }
}

using System;
using System.Collections.Generic;
using DiceFree.Quests;

namespace DiceFree.Persistence
{
    // One atomic revision per Echo. Unknown sections/classes remain opaque and round-trip unchanged.
    [Serializable] public sealed class SaveSection
    {
        public string id;
        public int version = 1;
        public string json;
    }
    [Serializable] public sealed class EchoSave
    {
        public int schemaVersion = SaveMigrations.CurrentSchema;
        public string userId = Guid.NewGuid().ToString("D");
        public string echoId = Guid.NewGuid().ToString("D");
        public long revision;
        public string writtenUtc;
        public List<SaveSection> sections = new();
    }
    [Serializable] public sealed class ManifestationSave
    {
        public string classId;
        public int level = 1;
        public int xp;
        public string anchorId;
        public QuestProgress[] quests = Array.Empty<QuestProgress>();
        public ResourceSaveValue[] resources = Array.Empty<ResourceSaveValue>();
    }
    [Serializable] internal sealed class SaveEnvelope
    {
        public string checksum;
        public string payload;
    }
}

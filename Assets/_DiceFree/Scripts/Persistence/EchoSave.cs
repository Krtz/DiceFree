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
        public int schemaVersion = 1;
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
        public float healthFraction = 1;
        public QuestProgress[] quests = Array.Empty<QuestProgress>();
    }
    [Serializable] internal sealed class SaveEnvelope
    {
        public string checksum;
        public string payload;
    }
}

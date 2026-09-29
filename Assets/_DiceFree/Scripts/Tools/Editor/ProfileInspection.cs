using System;
using System.Collections.Generic;
using System.IO;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;

namespace DiceFree.EditorTools
{
    // Detached editor-only view model shared by the window and automated inspection checks.
    public sealed class ProfileInspection
    {
        public string userId, echoId, classId, anchorId, path, savedUtc, status, migration, recovery;
        public int schema, level, xp;
        public long revision;
        public bool autosaveReady;
        public QuestProgress[] quests;
        public string[] preserved;
        public SaveFileDiagnostics[] files;
        public static ProfileInspection Capture(ManifestationPersistence source)
        {
            var profile = source.CaptureProfile();
            var progression = source.GetComponent<ExperienceProgression>();
            var journal = source.GetComponent<QuestJournal>();
            string classId = source.GetComponent<ActorStats>().Definition.stableId;
            var other = new List<string>();
            if (profile != null)
                foreach (var section in profile.sections)
                {
                    if (section.id != "manifestation:" + classId) other.Add($"Section: {section.id} (v{section.version}, preserved)");
                    else
                    {
                        if (section.version != SaveMigrations.ManifestationVersion)
                        { other.Add($"Manifestation: {section.id} (unsupported v{section.version}; preserved)"); continue; }
                        try
                        {
                            var state = UnityEngine.JsonUtility.FromJson<ManifestationSave>(section.json);
                            if (state?.resources != null)
                                foreach (var resource in state.resources) other.Add("Resource: " + resource?.resourceId + " (no runtime definition; preserved)");
                        }
                        catch (Exception error) { other.Add("Unreadable manifestation: " + error.Message); }
                    }
                }
            var quests = journal.CaptureState();
            foreach (var quest in quests)
            {
                bool resolved = false;
                foreach (var definition in journal.Definitions)
                    if (definition.stableId == quest.questId && definition.version == quest.definitionVersion) resolved = true;
                if (!resolved) other.Add($"Quest: {quest.questId} (v{quest.definitionVersion}, unresolved)");
            }
            return new ProfileInspection {
                userId = profile?.userId, echoId = profile?.echoId, schema = profile?.schemaVersion ?? 0,
                revision = profile?.revision ?? 0, savedUtc = profile?.writtenUtc, classId = classId,
                level = progression.Level, xp = progression.CurrentXp, anchorId = source.GetComponent<RespawnAtAnchor>().AnchorId,
                path = source.SavePath, status = source.Status, autosaveReady = source.Ready,
                migration = source.MigrationStatus, recovery = source.RecoveryStatus, quests = quests, preserved = other.ToArray(),
                files = source.SavePath == null ? Array.Empty<SaveFileDiagnostics>() :
                    new LocalEchoStore(Path.GetDirectoryName(source.SavePath)).InspectCopies()
            };
        }
    }
}

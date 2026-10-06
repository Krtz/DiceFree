using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DiceFree.Persistence;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    public static class SaveMigrationValidation
    {
        public const string Fixture = "Assets/_DiceFree/Tests/Fixtures/echo-v1.json";
        [Serializable] private sealed class Envelope { public string checksum, payload; }
        // Fixture encoding only: production storage must never commit an old-schema candidate.
        public static void WriteFixture(string path, EchoSave value)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string payload = JsonUtility.ToJson(value);
            using var hash = SHA256.Create();
            File.WriteAllText(path, JsonUtility.ToJson(new Envelope {
                payload = payload, checksum = Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(payload)))
            }));
        }
        public static void Run()
        {
            ValidateV3();
            var directory = Path.Combine(Path.GetTempPath(), "DiceFree-migration-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var store = new LocalEchoStore(directory);
            File.Copy(Fixture, store.Path);
            var original = File.ReadAllText(store.Path);
            var old = JsonUtility.FromJson<EchoSave>(JsonUtility.FromJson<Envelope>(original).payload);
            Require(old.schemaVersion == 1 && old.sections[0].json.Contains("healthFraction"), "Fixture must be the genuine HP-bearing v1 format.");
            old.sections.Add(new SaveSection { id = "unknown:event", version = 73, json = " {\"preserve\": [1, 2, 3]} " });
            old.sections.Add(new SaveSection { id = "manifestation:class.future", version = 81, json = "opaque future representation" });
            WriteFixture(store.Path, old);
            var before = File.ReadAllText(store.Path);
            var migrated = store.Load();
            Require(migrated.schemaVersion == SaveMigrations.CurrentSchema && migrated.sections[0].version == SaveMigrations.ManifestationVersion, "Schema/record migration missing.");
            Require(migrated.userId == old.userId && migrated.echoId == old.echoId && migrated.revision == old.revision, "Migration changed identity/revision.");
            var state = JsonUtility.FromJson<ManifestationSave>(migrated.sections[0].json);
            Require(state.level == 3 && state.xp == 7 && state.quests[0].count == 2 && state.quests[1].questId == "quest.unresolved", "Migration lost progression/unknown quest.");
            Require(!migrated.sections[0].json.Contains("healthFraction"), "HP survived migration.");
            Require(migrated.sections[1].json == old.sections[1].json && migrated.sections[2].json == old.sections[2].json, "Unknown sections were rewritten.");
            Require(File.ReadAllText(store.Path) == before && old.sections[0].version == 1, "Load migration mutated source or file before commit.");
            Require(JsonUtility.ToJson(SaveMigrations.Upgrade(migrated)) == JsonUtility.ToJson(migrated), "Migration is not idempotent.");
            migrated.revision++; store.Commit(migrated);
            Require(File.ReadAllText(store.Path + ".bak1") == before, "Original v1 backup not retained.");
            Require(store.Load().schemaVersion == SaveMigrations.CurrentSchema && store.MigrationMessage == null, "Current save repeatedly migrated.");
            File.WriteAllText(store.Path, "corrupt current primary");
            Require(store.Load().schemaVersion == SaveMigrations.CurrentSchema && store.RecoveryMessage != null, "Legacy backup recovery/migration failed.");
            old.sections[0].json = "{}";
            WriteFixture(store.Path, old);
            before = File.ReadAllText(store.Path);
            bool failed = false;
            try { store.Load(); } catch (SaveMigrationException) { failed = true; }
            Require(failed && File.ReadAllText(store.Path) == before, "Failed migration destroyed/replaced original or silently rolled back.");

            var ordinary = new ResourceLoadRule { resourceId = "resource.test.ordinary", initialValue = 10 };
            var durable = new ResourceLoadRule { resourceId = "resource.test.durable", policy = ResourceLoadPolicy.Persist };
            Require(ResourcePersistence.Capture(ordinary, 77) == null, "Ordinary resources persisted by default.");
            var records = new[] { ResourcePersistence.Capture(durable, 42), new ResourceSaveValue { resourceId = ordinary.resourceId, value = 77 } };
            Require(ResourcePersistence.Resolve(ordinary, records) == 10 && ResourcePersistence.Resolve(durable, records) == 42,
                "Resource reset/explicit persistence policy failed.");
            Debug.Log("SAVE_MIGRATION_OK: real v1 fixture, idempotent current schema, unknown records, original backup, failed migration and resource policy.");
        }
        private static void ValidateV3()
        {
            var directory = Path.Combine(Path.GetTempPath(), "DiceFree-v3-migration-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); var store = new LocalEchoStore(directory);
            File.Copy("Assets/_DiceFree/Tests/Fixtures/echo-v3.json", store.Path);
            string original = File.ReadAllText(store.Path);
            var old = JsonUtility.FromJson<EchoSave>(JsonUtility.FromJson<Envelope>(original).payload);
            Require(old.schemaVersion == 3, "Genuine v3 fixture required");
            var upgraded = store.Load();
            Require(upgraded.schemaVersion == SaveMigrations.CurrentSchema && upgraded.echoId == old.echoId && upgraded.userId == old.userId, "v3 identity/schema");
            foreach (var section in old.sections)
            {
                var next = upgraded.sections.Find(s => s.id == section.id);
                if (section.version != 3 || !section.id.StartsWith("manifestation:"))
                { Require(next.json == section.json && next.version == section.version, "Unknown section changed"); continue; }
                var before = JsonUtility.FromJson<ManifestationSave>(section.json);
                var after = JsonUtility.FromJson<ManifestationSave>(next.json);
                Require(next.version == SaveMigrations.ManifestationVersion && before.level == after.level && before.xp == after.xp && before.gold == after.gold, "v3 durable state");
                Require(after.classSkills != null && after.classSkills.Length == 0, "v3 migration did not initialize empty class skills.");
                Require(JsonUtility.ToJson(before) == JsonUtility.ToJson(after), "v3 ownership/equipment/resources/quests changed");
                foreach (var quest in after.quests) Require(quest.alternatives != null && quest.alternatives.Length == 0, "v3 alternative defaults");
            }
            Require(File.ReadAllText(store.Path) == original && JsonUtility.ToJson(SaveMigrations.Upgrade(upgraded)) == JsonUtility.ToJson(upgraded), "v3 migration mutation/idempotency");
            upgraded.revision++; store.Commit(upgraded); Require(File.ReadAllText(store.Path + ".bak1") == original, "v3 original backup");
            Debug.Log("SAVE_V3_MIGRATION_OK: genuine item-bearing fixture, identity/unknowns, idempotency and original backup.");
        }
    }
}

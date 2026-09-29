using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DiceFree.Persistence
{
    // Storage owns integrity/atomic replacement; gameplay never knows paths or platform APIs.
    public sealed class LocalEchoStore
    {
        public string Path { get; }
        public string RecoveryMessage { get; private set; }
        public string MigrationMessage { get; private set; }
        public LocalEchoStore(string directory) => Path = System.IO.Path.Combine(directory, "primary-echo.json");
        public SaveFileDiagnostics[] InspectCopies()
        {
            var result = new List<SaveFileDiagnostics>();
            foreach (var suffix in new[] { "", ".bak1", ".bak2", ".bak3" })
            {
                var info = new SaveFileDiagnostics { path = Path + suffix, integrity = "Not present" };
                info.exists = File.Exists(info.path);
                if (info.exists)
                    try
                    {
                        var value = Read(info.path);
                        info.valid = true; info.integrity = "Checksum/header verified";
                        info.schema = value.schemaVersion; info.revision = value.revision; info.writtenUtc = value.writtenUtc;
                    }
                    catch (Exception error) { info.integrity = error.Message; }
                result.Add(info);
            }
            return result.ToArray();
        }
        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
        private static EchoSave Read(string path)
        {
            var envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
            if (envelope == null || envelope.payload == null || envelope.checksum != Hash(envelope.payload))
                throw new InvalidDataException("Save checksum failed.");
            var save = JsonUtility.FromJson<EchoSave>(envelope.payload);
            if (save == null) throw new InvalidDataException("Empty Echo save.");
            // A future schema is not corruption: never roll it back to an older backup or overwrite it.
            if (save.schemaVersion != 1 && save.schemaVersion != SaveMigrations.CurrentSchema)
                throw new NotSupportedException("Unsupported Echo schema " + save.schemaVersion);
            if (!Guid.TryParse(save.userId, out _) || !Guid.TryParse(save.echoId, out _) ||
                save.revision < 1 || save.sections == null) throw new InvalidDataException("Invalid Echo header.");
            var ids = new HashSet<string>();
            foreach (var section in save.sections)
                if (section == null || string.IsNullOrEmpty(section.id) || !ids.Add(section.id) ||
                    section.version < 1 || section.json == null) throw new InvalidDataException("Invalid/duplicate save section.");
            return save;
        }
        public EchoSave Load()
        {
            RecoveryMessage = null;
            MigrationMessage = null;
            bool found = false;
            foreach (var candidate in new[] { Path, Path + ".bak1", Path + ".bak2", Path + ".bak3" })
            {
                if (!File.Exists(candidate)) continue;
                found = true;
                try
                {
                    var save = Read(candidate);
                    if (candidate != Path) RecoveryMessage = "Recovered Echo from " + candidate;
                    var migrated = SaveMigrations.Upgrade(save);
                    if (migrated.schemaVersion != save.schemaVersion)
                        MigrationMessage = $"Echo schema {save.schemaVersion} -> {migrated.schemaVersion}; pending next successful commit";
                    return migrated;
                }
                catch (NotSupportedException) { throw; }
                catch (Exception error) when (error is IOException || error is ArgumentException)
                { RecoveryMessage = "Unreadable save: " + candidate + ": " + error.Message; }
            }
            if (found) throw new InvalidDataException("No readable Echo revision. Existing files preserved; autosave disabled.");
            return null;
        }
        public void Commit(EchoSave save)
        {
            if (save.schemaVersion != SaveMigrations.CurrentSchema)
                throw new NotSupportedException("Only current-schema saves can be committed.");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            using var writeLock = new FileStream(Path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if (File.Exists(Path))
            {
                EchoSave current = null;
                try { current = Read(Path); }
                catch (Exception error) when (error is IOException || error is ArgumentException) { }
                if (current != null && (current.echoId != save.echoId || current.userId != save.userId || current.revision >= save.revision))
                    throw new InvalidOperationException("Echo changed on disk; reload before saving. Conflicting revision preserved.");
            }
            string pending = Path + ".pending";
            var payload = JsonUtility.ToJson(save);
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new SaveEnvelope { payload = payload, checksum = Hash(payload) }, true));
            using (var stream = new FileStream(pending, FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            Read(pending); // Validate the entire candidate before replacing the committed revision.
            if (!File.Exists(Path)) File.Move(pending, Path);
            else
            {
                // Rotate valid committed copies only. Preserve an invalid primary for diagnosis/recovery.
                bool valid = true;
                try { Read(Path); }
                catch (Exception error) when (error is IOException || error is ArgumentException) { valid = false; }
                if (valid)
                {
                    if (File.Exists(Path + ".bak2")) File.Copy(Path + ".bak2", Path + ".bak3", true);
                    if (File.Exists(Path + ".bak1")) File.Copy(Path + ".bak1", Path + ".bak2", true);
                }
                File.Replace(pending, Path, valid ? Path + ".bak1" : Path + ".corrupt-" + Guid.NewGuid().ToString("N"));
            }
            if (MigrationMessage != null) MigrationMessage = MigrationMessage.Replace("pending next successful commit", "committed");
        }
    }
}

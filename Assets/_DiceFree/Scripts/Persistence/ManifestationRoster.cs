using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceFree.Persistence
{
    [Serializable]
    public sealed class ManifestationBranchRecord
    {
        public string parentClassId;
        public string targetClassId;
        public string createdUtc;

        public ManifestationBranchRecord Copy() => new()
        {
            parentClassId = parentClassId,
            targetClassId = targetClassId,
            createdUtc = createdUtc
        };
    }

    [Serializable]
    public sealed class ManifestationRosterSave
    {
        public string activeClassId;
        public ManifestationBranchRecord[] branches = Array.Empty<ManifestationBranchRecord>();
    }

    public static class ManifestationRoster
    {
        public const string SectionId = "echo:manifestations";
        public const int Version = 1;
        public const string ManifestationPrefix = "manifestation:";

        public static string SectionIdFor(string classId) => ManifestationPrefix + classId;

        public static bool HasManifestation(EchoSave profile, string classId) =>
            profile != null && !string.IsNullOrWhiteSpace(classId) &&
            profile.sections.Any(section => section != null && section.id == SectionIdFor(classId));

        public static ManifestationSave ReadManifestation(EchoSave profile, string classId)
        {
            if (profile == null || string.IsNullOrWhiteSpace(classId)) return null;
            var section = profile.sections.Find(value => value != null && value.id == SectionIdFor(classId));
            if (section == null) return null;
            if (section.version != SaveMigrations.ManifestationVersion)
                throw new NotSupportedException("Unsupported manifestation version for " + classId + ".");
            var state = JsonUtility.FromJson<ManifestationSave>(section.json);
            if (state == null || state.classId != classId)
                throw new InvalidOperationException("Manifestation section/class identity mismatch for " + classId + ".");
            return state;
        }

        public static ManifestationRosterSave Read(EchoSave profile, string fallbackClassId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var section = profile.sections.Find(value => value != null && value.id == SectionId);
            if (section == null)
                return new ManifestationRosterSave
                {
                    activeClassId = fallbackClassId,
                    branches = Array.Empty<ManifestationBranchRecord>()
                };
            if (section.version != Version)
                throw new NotSupportedException("Unsupported manifestation roster version " + section.version + ".");
            var roster = JsonUtility.FromJson<ManifestationRosterSave>(section.json);
            Validate(roster, profile, fallbackClassId);
            return roster;
        }

        public static void Write(EchoSave profile, ManifestationRosterSave roster)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            Validate(roster, profile, roster?.activeClassId);
            var section = profile.sections.Find(value => value != null && value.id == SectionId);
            if (section == null)
            {
                section = new SaveSection { id = SectionId };
                profile.sections.Add(section);
            }
            section.version = Version;
            section.json = JsonUtility.ToJson(roster);
        }

        public static ManifestationRosterSave AddBranch(
            EchoSave profile,
            ManifestationRosterSave source,
            string parentClassId,
            string targetClassId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(parentClassId) || string.IsNullOrWhiteSpace(targetClassId) ||
                parentClassId == targetClassId)
                throw new ArgumentException("Advancement branch requires distinct stable parent/target class IDs.");
            if (HasManifestation(profile, targetClassId))
                throw new InvalidOperationException("Target manifestation already exists: " + targetClassId);

            var branches = new List<ManifestationBranchRecord>(source.branches?.Where(value => value != null)
                .Select(value => value.Copy()) ?? Enumerable.Empty<ManifestationBranchRecord>());
            if (branches.Any(value => value.targetClassId == targetClassId))
                throw new InvalidOperationException("Target manifestation already has branch history: " + targetClassId);

            branches.Add(new ManifestationBranchRecord
            {
                parentClassId = parentClassId,
                targetClassId = targetClassId,
                createdUtc = DateTime.UtcNow.ToString("O")
            });
            return new ManifestationRosterSave
            {
                activeClassId = targetClassId,
                branches = branches.ToArray()
            };
        }

        public static void Validate(ManifestationRosterSave roster, EchoSave profile, string fallbackClassId)
        {
            if (roster == null) throw new InvalidOperationException("Manifestation roster is empty.");
            roster.branches ??= Array.Empty<ManifestationBranchRecord>();
            if (string.IsNullOrWhiteSpace(roster.activeClassId))
                roster.activeClassId = fallbackClassId;
            if (string.IsNullOrWhiteSpace(roster.activeClassId))
                throw new InvalidOperationException("Manifestation roster requires an active class ID.");
            if (profile != null && profile.sections.Any(section => section != null && section.id.StartsWith(ManifestationPrefix, StringComparison.Ordinal)) &&
                !HasManifestation(profile, roster.activeClassId))
                throw new InvalidOperationException("Manifestation roster points at a missing active class: " + roster.activeClassId);

            var targets = new HashSet<string>(StringComparer.Ordinal);
            foreach (var branch in roster.branches)
            {
                if (branch == null || string.IsNullOrWhiteSpace(branch.parentClassId) ||
                    string.IsNullOrWhiteSpace(branch.targetClassId) ||
                    branch.parentClassId == branch.targetClassId ||
                    !targets.Add(branch.targetClassId))
                    throw new InvalidOperationException("Invalid/duplicate manifestation branch record.");
            }
        }
    }
}

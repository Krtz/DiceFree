using System;
using System.Collections.Generic;
using DiceFree.Items;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Persistence
{
    /// <summary>
    /// Pure Echo-profile mutations for manifestation persistence.
    /// Disk I/O and live actor state remain owned by ManifestationPersistence.
    /// </summary>
    public static class ManifestationProfileTransactions
    {
        public static ManifestationSave CreateChildSnapshot(ManifestationSave parent, string targetClassId)
        {
            if (parent == null || string.IsNullOrWhiteSpace(targetClassId))
                throw new ArgumentException("Manifestation fork requires parent state and target class ID.");

            var child = JsonUtility.FromJson<ManifestationSave>(JsonUtility.ToJson(parent));
            child.classId = targetClassId;
            child.level = 1;
            child.xp = 0;
            child.classSkills = Array.Empty<SkillRankState>();

            var remap = new Dictionary<string, string>(StringComparer.Ordinal);
            child.inventory ??= Array.Empty<ItemInstance>();
            foreach (var item in child.inventory)
            {
                string oldId = item.instanceId;
                string newId = Guid.NewGuid().ToString("D");
                remap[oldId] = newId;
                item.instanceId = newId;
            }

            child.equipment ??= Array.Empty<EquippedItem>();
            foreach (var slot in child.equipment)
                if (remap.TryGetValue(slot.instanceId, out string mapped))
                    slot.instanceId = mapped;

            return child;
        }

        public static void CommitFork(
            EchoSave profile,
            ManifestationSave parent,
            ManifestationSave child,
            string parentClassId)
        {
            if (profile == null || parent == null || child == null)
                throw new ArgumentNullException("Fork profile/state cannot be null.");
            if (parent.classId != parentClassId || child.classId == parentClassId)
                throw new InvalidOperationException("Fork class identities are inconsistent.");

            Upsert(profile, parent);
            var roster = ManifestationRoster.Read(profile, parentClassId);
            roster = ManifestationRoster.AddBranch(profile, roster, parentClassId, child.classId);
            Upsert(profile, child);
            ManifestationRoster.Write(profile, roster);
        }

        public static void RecordActive(
            EchoSave profile,
            ManifestationSave current,
            string currentClassId)
        {
            if (profile == null || current == null || current.classId != currentClassId)
                throw new InvalidOperationException("Active manifestation state is inconsistent.");

            Upsert(profile, current);
            var roster = ManifestationRoster.Read(profile, currentClassId);
            roster.activeClassId = currentClassId;
            ManifestationRoster.Write(profile, roster);
        }

        public static void SelectExisting(
            EchoSave profile,
            ManifestationSave current,
            string currentClassId,
            string targetClassId)
        {
            if (!ManifestationRoster.HasManifestation(profile, targetClassId))
                throw new InvalidOperationException("Target manifestation does not exist: " + targetClassId);

            Upsert(profile, current);
            var roster = ManifestationRoster.Read(profile, currentClassId);
            roster.activeClassId = targetClassId;
            ManifestationRoster.Write(profile, roster);
        }

        public static void Upsert(EchoSave profile, ManifestationSave state)
        {
            if (profile == null || state == null || string.IsNullOrWhiteSpace(state.classId))
                throw new ArgumentException("Manifestation upsert requires stable profile/state identity.");

            string id = ManifestationRoster.SectionIdFor(state.classId);
            var section = profile.sections.Find(value => value.id == id);
            if (section == null)
            {
                section = new SaveSection { id = id };
                profile.sections.Add(section);
            }

            section.version = SaveMigrations.ManifestationVersion;
            section.json = JsonUtility.ToJson(state);
        }
    }
}

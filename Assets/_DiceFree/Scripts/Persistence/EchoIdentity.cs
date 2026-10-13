using System;
using System.IO;
using UnityEngine;

namespace DiceFree.Persistence
{
    /// <summary>One name shared by every manifestation of an Echo.</summary>
    public static class EchoIdentity
    {
        public const string SectionId = "echo.identity";
        public const int Version = 1;
        public const int MaximumNameLength = 32;
        public const string DefaultName = "Echo";

        [Serializable]
        private sealed class IdentitySave
        {
            public string name;
        }

        private static string pendingName;
        private static string pendingRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearPendingName()
        {
            pendingName = null;
            pendingRoot = null;
        }

        public static bool TryNormalizeName(string value, out string normalized, out string error)
        {
            normalized = value?.Trim() ?? "";
            error = null;
            if (normalized.Length < 1 || normalized.Length > MaximumNameLength)
                error = "Enter an Echo name of 1 to " + MaximumNameLength + " characters.";
            else
                foreach (char character in normalized)
                    if (char.IsControl(character))
                    {
                        error = "The Echo name must stay on one line.";
                        break;
                    }
            return error == null;
        }

        public static string ReadName(EchoSave profile)
        {
            var section = profile?.sections?.Find(value => value != null && value.id == SectionId);
            if (section == null || section.version != Version) return DefaultName;
            try
            {
                var identity = JsonUtility.FromJson<IdentitySave>(section.json);
                return TryNormalizeName(identity?.name, out string name, out _) ? name : DefaultName;
            }
            catch (ArgumentException) { return DefaultName; }
        }

        public static bool CanRename(EchoSave profile)
        {
            var section = profile?.sections?.Find(value => value != null && value.id == SectionId);
            return section == null || section.version == Version;
        }

        public static void WriteName(EchoSave profile, string value)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!TryNormalizeName(value, out string name, out string error))
                throw new ArgumentException(error, nameof(value));
            if (!CanRename(profile))
                throw new InvalidDataException("This Echo name was saved by a newer version of the game.");
            var section = profile.sections.Find(item => item != null && item.id == SectionId);
            if (section == null)
            {
                section = new SaveSection { id = SectionId };
                profile.sections.Add(section);
            }
            section.version = Version;
            section.json = JsonUtility.ToJson(new IdentitySave { name = name });
        }

        public static string Format(string echoName, string classTitle)
        {
            string name = string.IsNullOrWhiteSpace(echoName) ? DefaultName : echoName.Trim();
            return string.IsNullOrWhiteSpace(classTitle) ? name : name + " the " + classTitle.Trim();
        }

        public static void QueueNewEchoName(string value, string saveRoot)
        {
            if (!TryNormalizeName(value, out string name, out string error))
                throw new ArgumentException(error, nameof(value));
            if (string.IsNullOrEmpty(saveRoot)) throw new ArgumentException("Echo save location is unavailable.");
            pendingName = name;
            pendingRoot = saveRoot;
        }

        public static void ApplyPendingName(EchoSave profile, string saveRoot)
        {
            try
            {
                if (pendingName != null && string.Equals(pendingRoot, saveRoot, StringComparison.Ordinal))
                    WriteName(profile, pendingName);
            }
            finally { ClearPendingName(); }
        }
    }
}

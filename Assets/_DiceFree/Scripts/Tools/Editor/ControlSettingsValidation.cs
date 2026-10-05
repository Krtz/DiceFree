using System;
using System.IO;
using System.Linq;
using DiceFree.Foundation;
using DiceFree.Persistence;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class ControlSettingsValidation
    {
        [MenuItem("DiceFree/Validate/Options Settings Boundaries")]
        public static void ValidateFromMenu() => Run();

        [CliCommand("dicefree.controls.settings.validate", "Validate settings fallback, version preservation, reset removal and load without saving.", Tags = new[] { "tests" })]
        public static object Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Run settings boundary validation in Edit Mode.");
            string directory = Path.Combine(Path.GetTempPath(), "DiceFree-settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                ControlBindings.Initialize();
                var entry = ControlBindings.Entries.Single(e => e.Id == "interact/0");
                Check(ControlBindings.Allowed("<Keyboard>/j"), "Layout path should be valid without device lookup.");
                Check(!ControlBindings.Allowed("<Keyboard>/*") && !ControlBindings.Allowed("<Keyboard>/escape"), "Wildcard or reserved key accepted.");
                Check(ControlBindings.Set(entry, "<Keyboard>/j", out _), "Rebind failed.");
                Check(ControlBindings.Reset(entry, out _) && ControlBindings.Snapshot().bindings.Count == 0, "Reset one retained an override.");
                Check(ControlBindings.Set(entry, "<Keyboard>/j", out _), "Rebind failed.");
                var store = new LocalControlSettingsStore(directory);
                store.Save(ControlBindings.Snapshot());
                int notifications = 0;
                ControlBindings.Changed += () => notifications++;
                Check(store.Load() != null && notifications == 0, "Load triggered a settings save notification.");
                Check(new LocalControlSettingsStore(directory + "-missing").Load() == null && entry.Path == "<Keyboard>/i", "Missing settings retained prior overrides.");
                File.WriteAllText(store.Path, "broken");
                Check(store.Load() == null && entry.Path == "<Keyboard>/i", "Corrupt settings fallback failed.");
                string future = "{\"version\":99,\"payload\":\"\",\"checksum\":\"\"}";
                File.WriteAllText(store.Path, future);
                Check(store.Load() == null && store.ReadOnly && entry.Path == "<Keyboard>/i", "Unknown settings version fallback failed.");
                store.Save(ControlBindings.Snapshot());
                Check(File.ReadAllText(store.Path) == future, "Unknown settings version was overwritten.");

                var conflictStore = new LocalControlSettingsStore(directory + "-conflict");
                var conflicting = new ControlSettings();
                conflicting.bindings.Add(new BindingRecord { id = "interact/0", path = "<Keyboard>/w" });
                conflictStore.Save(conflicting);
                Check(conflictStore.Load() == null && entry.Path == "<Keyboard>/i", "Conflicting settings accepted.");
                Check(!conflictStore.ReadOnly, "Read-only status leaked into a later load.");
                Debug.Log("DICEFREE_CONTROL_SETTINGS_BOUNDARIES_OK");
                return new { success = true, finalMarker = "DICEFREE_CONTROL_SETTINGS_BOUNDARIES_OK" };
            }
            finally { ControlBindings.Initialize(); Directory.Delete(directory, true); }
        }
        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}

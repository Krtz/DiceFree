using DiceFree.Persistence;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public sealed class ProfileInspectorWindow : EditorWindow
    {
        private ProfileInspection snapshot;
        private Vector2 scroll;
        private double nextRefresh;
        [MenuItem("DiceFree/Debug/Profile Inspector")]
        public static void Open() => GetWindow<ProfileInspectorWindow>("Profile Inspector");
        private void OnInspectorUpdate()
        {
            if (EditorApplication.timeSinceStartup < nextRefresh) return;
            nextRefresh = EditorApplication.timeSinceStartup + 1;
            var source = Application.isPlaying ? FindAnyObjectByType<ManifestationPersistence>() : null;
            snapshot = source == null ? null : ProfileInspection.Capture(source);
            Repaint();
        }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Developer diagnostics only. Read-only; does not modify, reload or delete profiles.", MessageType.Info);
            if (snapshot == null) { EditorGUILayout.LabelField("Enter Play Mode with persistence enabled to inspect the loaded profile."); return; }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            Field("Autosave", (snapshot.autosaveReady ? "Ready: " : "Stopped/inactive: ") + snapshot.status);
            Field("User UUID", snapshot.userId); Field("Echo UUID", snapshot.echoId);
            Field("Schema / revision", snapshot.schema + " / " + snapshot.revision);
            Field("Last successful save UTC", snapshot.savedUtc); Field("Primary path", snapshot.path);
            Field("Manifestation", snapshot.classId); Field("Level / XP", snapshot.level + " / " + snapshot.xp);
            Field("Registered anchor", snapshot.anchorId); Field("Migration", snapshot.migration); Field("Recovery", snapshot.recovery);
            Field("Carried gold / inventory count", snapshot.gold + " / " + snapshot.inventory.Length);
            foreach (var item in snapshot.inventory) Field(item.instanceId, item.definitionId);
            foreach (var slot in snapshot.equipment) Field("Equipped " + slot.slotId, slot.instanceId);
            EditorGUILayout.Space(); EditorGUILayout.LabelField("Quest state", EditorStyles.boldLabel);
            foreach (var quest in snapshot.quests)
            {
                Field(quest.questId, $"v{quest.definitionVersion}: {quest.status}, stage {quest.stage}, count {quest.count}");
                foreach (var alternative in quest.alternatives) Field("  " + alternative.objectiveId, alternative.count.ToString());
            }
            EditorGUILayout.Space(); EditorGUILayout.LabelField("Integrity / backups", EditorStyles.boldLabel);
            foreach (var file in snapshot.files)
                Field(file.path, $"{file.integrity}; schema {file.schema}, revision {file.revision}, UTC {file.writtenUtc}");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("Other preserved / unresolved records", EditorStyles.boldLabel);
            foreach (var record in snapshot.preserved) EditorGUILayout.SelectableLabel(record, EditorStyles.wordWrappedLabel, GUILayout.Height(36));
            EditorGUILayout.EndScrollView();
        }
        private static void Field(string label, string value)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(value) ? "—" : value, EditorStyles.wordWrappedLabel, GUILayout.Height(36));
        }
    }
}

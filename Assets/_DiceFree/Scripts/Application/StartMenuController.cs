using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Persistence;
using DiceFree.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Advancement
{
    public sealed class StartMenuController : MonoBehaviour
    {
        [Serializable]
        public sealed class ManifestationEntry
        {
            public string classId;
            public string displayName;
            public int level;
            public bool available;
            public bool active;
        }

        [SerializeField] private ClassCatalog classCatalog;
        [SerializeField] private string gameplaySceneName = "Cornberg";

        private LocalEchoStore store;
        private EchoSave profile;
        private string status = "";
        private bool loadBlocked;
        private Vector2 scroll;
        private string echoName = EchoIdentity.DefaultName;

        public ClassCatalog Catalog => classCatalog;
        public string GameplaySceneName => gameplaySceneName;
        public bool HasLoadedProfile => profile != null;
        public bool ReadyForNewEcho => !loadBlocked && store != null && profile == null;
        public bool LoadBlocked => loadBlocked;
        public string Status => status;
        public ManifestationEntry[] Entries => BuildEntries();

        public void Configure(ClassCatalog catalog, string sceneName)
        {
            classCatalog = catalog;
            gameplaySceneName = string.IsNullOrWhiteSpace(sceneName) ? "Cornberg" : sceneName;
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            status = "";
            loadBlocked = false;
            try
            {
                classCatalog?.Validate();
                string root = EchoSaveLocation.ResolveRoot();
                if (root == null)
                {
                    store = null;
                    profile = null;
                    loadBlocked = true;
                    status = "Persistence is disabled for this session.";
                    return;
                }

                store = new LocalEchoStore(root);
                profile = store.Load();
                echoName = EchoIdentity.ReadName(profile);
                if (!string.IsNullOrEmpty(store.RecoveryMessage)) status = store.RecoveryMessage;
                if (!string.IsNullOrEmpty(store.MigrationMessage))
                    status = string.IsNullOrEmpty(status) ? store.MigrationMessage : status + "\n" + store.MigrationMessage;
            }
            catch (Exception exception)
            {
                store = null;
                profile = null;
                loadBlocked = true;
                status = "Could not read Echo save: " + exception.Message;
            }
        }

        private void OnGUI()
        {
            const float width = 660;
            float height = Mathf.Min(Screen.height - 80, 620);
            var outer = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(outer, GUIContent.none);

            GUI.Label(new Rect(outer.x + 24, outer.y + 20, outer.width - 48, 36), "DiceFree");
            GUI.Label(new Rect(outer.x + 24, outer.y + 58, outer.width - 48, 24),
                profile == null ? "Begin a new Echo" : "Choose a manifestation");

            var content = new Rect(outer.x + 20, outer.y + 94, outer.width - 40, outer.height - 236);
            GUILayout.BeginArea(content);
            scroll = GUILayout.BeginScrollView(scroll);

            if (loadBlocked)
            {
                GUILayout.Label("The Echo save cannot be used safely in this session.");
                GUILayout.Label("Existing files have not been changed.");
                if (GUILayout.Button("Retry", GUILayout.Height(34))) Refresh();
            }
            else if (profile == null)
            {
                DrawNewEcho();
            }
            else
            {
                DrawExistingEcho();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (GUI.Button(new Rect(outer.x + 24, outer.yMax - 134, outer.width - 48, 34), "Multiplayer development test"))
                DiceFree.Development.NetworkWorldLab.OpenSelection(this);

            if (GUI.Button(new Rect(outer.x + 24, outer.yMax - 94, outer.width - 48, 34), "Development room"))
                TryOpenDevelopmentRoom();

            if (!string.IsNullOrEmpty(status))
                GUI.Label(new Rect(outer.x + 24, outer.yMax - 54, outer.width - 48, 44), status);
        }

        private void DrawNewEcho()
        {
            var novice = classCatalog?.Resolve("class.novice");
            if (novice == null)
            {
                GUILayout.Label("Novice content is unavailable. This build cannot create a new Echo.");
                return;
            }

            GUILayout.Label("Every Echo begins as a Novice.");
            GUILayout.Space(8);
            GUILayout.Label("Echo name");
            echoName = GUILayout.TextField(echoName, EchoIdentity.MaximumNameLength);
            GUILayout.Label(EchoIdentity.Format(echoName, novice.displayName));
            GUILayout.Space(12);
            if (GUILayout.Button("Start adventure", GUILayout.Height(52)))
                TryStartNewEcho();
        }

        private void DrawExistingEcho()
        {
            string fallback = InferFallbackClassId(profile);
            ManifestationRosterSave roster;
            try
            {
                roster = ManifestationRoster.Read(profile, fallback);
            }
            catch (Exception exception)
            {
                GUILayout.Label("The manifestation roster could not be read: " + exception.Message);
                return;
            }

            GUILayout.Label("Echo name");
            GUI.enabled = EchoIdentity.CanRename(profile);
            echoName = GUILayout.TextField(echoName, EchoIdentity.MaximumNameLength);
            if (GUILayout.Button("Save name", GUILayout.Height(30)))
                TryRenameEcho(echoName);
            GUI.enabled = true;
            GUILayout.Space(8);

            var entries = BuildEntries();
            if (entries.Length == 0)
            {
                GUILayout.Label("This Echo contains no playable manifestation saves.");
                return;
            }

            GUILayout.Label(EchoIdentity.ReadName(profile));
            GUILayout.Space(8);

            foreach (var entry in entries)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(entry.displayName + "  —  Level " + entry.level);
                GUILayout.Label(entry.active ? "Last active manifestation" : "Preserved manifestation");

                GUI.enabled = entry.available;
                if (GUILayout.Button(entry.available ? "Play " + entry.displayName : "Content unavailable in this build", GUILayout.Height(38)))
                    TryPlayManifestation(entry.classId);
                GUI.enabled = true;
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }

            if (GUILayout.Button("Refresh saves", GUILayout.Height(30)))
                Refresh();
        }

        public bool TryOpenDevelopmentRoom()
        {
            try
            {
                DiceFree.Foundation.DevelopmentSession.Begin();
                SceneManager.LoadScene("DevelopmentRoom");
                return true;
            }
            catch (Exception exception)
            {
                DiceFree.Foundation.DevelopmentSession.End();
                status = "Could not open development room: " + exception.Message;
                return false;
            }
        }

        public bool TryStartNewEcho() => TryStartNewEcho(echoName);

        public bool TryStartNewEcho(string name)
        {
            if (loadBlocked || store == null || profile != null)
            {
                status = "A new Echo cannot be started from the current save state.";
                return false;
            }
            if (classCatalog?.Resolve("class.novice") == null)
            {
                status = "Novice content is unavailable in this build.";
                return false;
            }

            try
            {
                EchoIdentity.QueueNewEchoName(name, EchoSaveLocation.ResolveRoot());
                SceneManager.LoadScene(gameplaySceneName);
                return true;
            }
            catch (Exception exception)
            {
                EchoIdentity.ClearPendingName();
                status = exception.Message;
                return false;
            }
        }

        public bool TryRenameEcho(string name)
        {
            try
            {
                if (store == null || profile == null || loadBlocked)
                    throw new InvalidOperationException("Echo save is not loaded safely.");
                var candidate = JsonUtility.FromJson<EchoSave>(JsonUtility.ToJson(profile));
                EchoIdentity.WriteName(candidate, name);
                if (EchoIdentity.ReadName(candidate) == EchoIdentity.ReadName(profile))
                {
                    echoName = EchoIdentity.ReadName(profile);
                    status = "Echo name saved.";
                    return true;
                }
                candidate.revision++;
                candidate.writtenUtc = DateTime.UtcNow.ToString("O");
                store.Commit(candidate);
                profile = candidate;
                echoName = EchoIdentity.ReadName(profile);
                status = "Echo name saved.";
                return true;
            }
            catch (Exception exception)
            {
                status = "Could not save Echo name: " + exception.Message;
                return false;
            }
        }

        public bool TryPlayManifestation(string classId)
        {
            try
            {
                if (store == null || profile == null || loadBlocked)
                    throw new InvalidOperationException("Echo save is not loaded safely.");
                if (classCatalog?.Resolve(classId) == null)
                    throw new InvalidOperationException("Class content is unavailable in this build: " + classId);
                if (!ManifestationRoster.HasManifestation(profile, classId))
                    throw new InvalidOperationException("Manifestation does not exist: " + classId);

                string fallback = InferFallbackClassId(profile);
                var roster = ManifestationRoster.Read(profile, fallback);
                if (roster.activeClassId != classId)
                {
                    var candidate = JsonUtility.FromJson<EchoSave>(JsonUtility.ToJson(profile));
                    var candidateRoster = ManifestationRoster.Read(candidate, roster.activeClassId);
                    candidateRoster.activeClassId = classId;
                    ManifestationRoster.Write(candidate, candidateRoster);
                    candidate.revision++;
                    candidate.writtenUtc = DateTime.UtcNow.ToString("O");
                    store.Commit(candidate);
                    profile = candidate;
                }

                SceneManager.LoadScene(gameplaySceneName);
                return true;
            }
            catch (Exception exception)
            {
                status = "Could not activate manifestation: " + exception.Message;
                return false;
            }
        }

        private ManifestationEntry[] BuildEntries()
        {
            if (profile == null) return Array.Empty<ManifestationEntry>();
            string fallback = InferFallbackClassId(profile);
            ManifestationRosterSave roster;
            try { roster = ManifestationRoster.Read(profile, fallback); }
            catch { return Array.Empty<ManifestationEntry>(); }

            return ManifestationEntries(profile)
                .Select(value =>
                {
                    var definition = classCatalog?.Resolve(value.state.classId);
                    return new ManifestationEntry
                    {
                        classId = value.state.classId,
                        displayName = EchoIdentity.Format(EchoIdentity.ReadName(profile),
                            definition != null ? definition.displayName : value.state.classId),
                        level = value.state.level,
                        available = definition != null,
                        active = value.state.classId == roster.activeClassId
                    };
                })
                .OrderByDescending(value => value.active)
                .ThenBy(value => value.displayName, StringComparer.Ordinal)
                .ToArray();
        }

        private IEnumerable<(ManifestationSave state, SaveSection section)> ManifestationEntries(EchoSave save)
        {
            foreach (var section in save.sections
                         .Where(value => value != null &&
                                         value.id.StartsWith(ManifestationRoster.ManifestationPrefix, StringComparison.Ordinal))
                         .OrderBy(value => value.id, StringComparer.Ordinal))
            {
                if (section.version != SaveMigrations.ManifestationVersion) continue;
                ManifestationSave state = null;
                try { state = JsonUtility.FromJson<ManifestationSave>(section.json); }
                catch (Exception) { }
                if (state != null && !string.IsNullOrWhiteSpace(state.classId))
                    yield return (state, section);
            }
        }

        private static string InferFallbackClassId(EchoSave save)
        {
            string novice = ManifestationRoster.SectionIdFor("class.novice");
            if (save.sections.Any(value => value != null && value.id == novice)) return "class.novice";
            var first = save.sections.FirstOrDefault(value => value != null &&
                value.id.StartsWith(ManifestationRoster.ManifestationPrefix, StringComparison.Ordinal));
            return first == null ? null : first.id.Substring(ManifestationRoster.ManifestationPrefix.Length);
        }

    }
}

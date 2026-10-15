using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Persistence;
using DiceFree.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Advancement
{
    public sealed partial class StartMenuController : MonoBehaviour
    {
        [Serializable]
        public sealed class ManifestationEntry
        {
            public string classId;
            public string displayName;
            public int level;
            public int tier;
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
            menuPresentation ??= Resources.Load<MenuPresentationCatalog>("MenuPresentationCatalog");
            portraitStage ??= GetComponent<MenuPortraitStage>()??gameObject.AddComponent<MenuPortraitStage>();
            portraitStage.Clear();
            options ??= FindAnyObjectByType<DiceFree.UI.OptionsPanel>()??gameObject.AddComponent<DiceFree.UI.OptionsPanel>();
            options.ShowLauncher=false;
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
                var last=BuildEntries().FirstOrDefault(e=>e.active);
                SelectedTier=last?.tier??0;
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
                        tier = menuPresentation?.TierOf(value.state.classId)??-1,
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

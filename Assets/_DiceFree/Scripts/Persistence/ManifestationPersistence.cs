using System;
using System.Collections;
using System.IO;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using DiceFree.Items;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Persistence
{
    [RequireComponent(typeof(ExperienceProgression), typeof(QuestJournal), typeof(RespawnAtAnchor))]
    public sealed class ManifestationPersistence : MonoBehaviour, IDurableMutationCoordinator
    {
        [SerializeField, Min(0)] private float debounceSeconds = 0.25f;
        private ExperienceProgression xp;
        private QuestJournal journal;
        private CombatActor actor;
        private RespawnAtAnchor anchor;
        private CarriedInventory inventory;
        private Equipment equipment;
        private GoldWallet wallet;
        private LocalEchoStore store;
        private EchoSave save;
        private bool ready, dirty;
        private int mutationDepth;
        // Synchronous reward mutations publish several model events. Never persist their intermediate state.
        public IDisposable DeferDurableWrites()
        {
            mutationDepth++;
            return new WriteScope(this);
        }
        private sealed class WriteScope : IDisposable
        {
            private ManifestationPersistence owner;
            public WriteScope(ManifestationPersistence value) => owner = value;
            public void Dispose() { if (owner == null) return; owner.mutationDepth--; owner = null; }
        }
        private float due;
        public string Status { get; private set; } = "Persistence inactive";
        public string SavePath => store?.Path;
        public long Revision => save?.revision ?? 0;
        public bool Ready => ready;
        public string MigrationStatus => store?.MigrationMessage;
        public string RecoveryStatus => store?.RecoveryMessage;
        public EchoSave CaptureProfile() => save == null ? null : JsonUtility.FromJson<EchoSave>(JsonUtility.ToJson(save));
        private string SectionId => "manifestation:" + actor.Stats.Definition.stableId;
        private IEnumerator Start()
        {
            // Automated suites cannot touch player data. An explicit root opts isolated save tests in.
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-diceFreeSaveRoot");
            if (index < 0 && Application.isBatchMode) yield break;
            if (index < 0 && Environment.GetEnvironmentVariable("DICEFREE_DISABLE_PERSISTENCE") == "1") yield break;
            if (index >= 0 && (index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1])))
            {
                Debug.LogError("-diceFreeSaveRoot requires an absolute isolated directory; persistence disabled.");
                yield break;
            }
            string root = index >= 0 && index + 1 < args.Length ? args[index + 1] :
                Path.Combine(Application.persistentDataPath, Application.isEditor ? "EditorProfiles" : "Profiles");
            xp = GetComponent<ExperienceProgression>(); journal = GetComponent<QuestJournal>();
            actor = GetComponent<CombatActor>(); anchor = GetComponent<RespawnAtAnchor>();
            inventory = GetComponent<CarriedInventory>(); equipment = GetComponent<Equipment>(); wallet = GetComponent<GoldWallet>();
            store = new LocalEchoStore(root);
            // All actors and the authored NavMesh must have completed Start before applying a saved spawn.
            yield return null;
            try
            {
                save = store.Load();
                bool existing = save != null;
                save ??= new EchoSave();
                var section = save.sections.Find(value => value.id == SectionId);
                if (section != null)
                {
                    if (section.version != SaveMigrations.ManifestationVersion) throw new NotSupportedException("Unsupported manifestation version; original preserved.");
                    var state = JsonUtility.FromJson<ManifestationSave>(section.json);
                    if (state == null || state.classId != actor.Stats.Definition.stableId ||
                        !xp.CanRestore(state.level, state.xp) || !journal.CanRestore(state.quests) ||
                        !CarriedInventory.Valid(state.inventory) || !Equipment.Valid(state.equipment, state.inventory) || state.gold < 0)
                        throw new InvalidDataException("Invalid manifestation; original preserved.");
                    xp.RestoreState(state.level, state.xp);
                    journal.RestoreState(state.quests);
                    if (inventory == null || equipment == null || wallet == null) throw new InvalidOperationException("Manifestation item components missing.");
                    inventory.Restore(state.inventory); equipment.Restore(state.equipment); wallet.Restore(state.gold);
                    if (!anchor.LoadAtAnchor(state.anchorId))
                        throw new InvalidOperationException("Saved/fallback resurrection point is not navigable.");
                }
                else if (existing) throw new InvalidDataException("Current manifestation missing; original Echo preserved.");
                ready = true;
                xp.Changed += MarkDirty; journal.Changed += MarkDirty;
                inventory.Changed += MarkDirty; equipment.Changed += MarkDirty; wallet.Changed += MarkDirty;
                Status = store.RecoveryMessage ?? "Local autosave ready";
                if (store.RecoveryMessage != null) Debug.LogWarning(store.RecoveryMessage);
                MarkDirty();
            }
            catch (Exception error) { Fail(error); }
        }
        private void MarkDirty()
        {
            if (!ready) return;
            if (!dirty) due = Time.unscaledTime + debounceSeconds;
            dirty = true;
        }
        private void LateUpdate() { if (ready && dirty && Time.unscaledTime >= due) Flush(); }
        public bool Flush()
        {
            if (!ready || mutationDepth != 0) return false;
            try
            {
                var candidate = CaptureProfile();
                var section = candidate.sections.Find(value => value.id == SectionId);
                var preserved = section == null ? null : JsonUtility.FromJson<ManifestationSave>(section.json);
                var state = new ManifestationSave {
                    classId = actor.Stats.Definition.stableId, level = xp.Level, xp = xp.CurrentXp,
                    quests = journal.CaptureState(), anchorId = anchor.AnchorId,
                    inventory = inventory.Items, equipment = equipment.Slots, gold = wallet.Gold,
                    // No resources exist on the current actor. Preserve unresolved opted-in records inertly.
                    resources = preserved?.resources ?? Array.Empty<ResourceSaveValue>()
                };
                if (section == null) { section = new SaveSection { id = SectionId }; candidate.sections.Add(section); }
                section.version = SaveMigrations.ManifestationVersion;
                section.json = JsonUtility.ToJson(state);
                candidate.revision++; candidate.writtenUtc = DateTime.UtcNow.ToString("O");
                store.Commit(candidate);
                save = candidate; // Diagnostics only advance after a successful commit.
                dirty = false; Status = "Local save revision " + save.revision;
                return true;
            }
            catch (Exception error) { Fail(error); return false; }
        }
        private void Fail(Exception error)
        {
            ready = false;
            Status = "Autosave stopped: " + error.Message + " Existing save files preserved.";
            Debug.LogWarning(Status);
        }
        private void OnApplicationPause(bool paused) { if (paused && ready) Flush(); }
        private void OnApplicationQuit() { if (ready) Flush(); }
        private void OnDestroy()
        {
            if (xp != null) xp.Changed -= MarkDirty;
            if (journal != null) journal.Changed -= MarkDirty;
            if (inventory != null) inventory.Changed -= MarkDirty;
            if (equipment != null) equipment.Changed -= MarkDirty;
            if (wallet != null) wallet.Changed -= MarkDirty;
        }
        private void OnGUI()
        {
            if (store != null && !ready) GUI.Box(new Rect(20, Screen.height / 2f, Screen.width - 40, 70), Status);
        }
    }
}

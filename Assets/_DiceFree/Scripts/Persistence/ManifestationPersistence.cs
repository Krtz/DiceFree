using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Persistence
{
    [DefaultExecutionOrder(100), RequireComponent(typeof(ExperienceProgression), typeof(QuestJournal), typeof(RespawnAtAnchor))]
    public sealed class ManifestationPersistence : MonoBehaviour, IDurableMutationCoordinator, IActorDisplayName
    {
        [SerializeField, Min(0)] private float debounceSeconds = 0.25f;
        [SerializeField] private ClassCatalog classCatalog;

        private ExperienceProgression xp;
        private QuestJournal journal;
        private CombatActor actor;
        private RespawnAtAnchor anchor;
        private CarriedInventory inventory;
        private Equipment equipment;
        private GoldWallet wallet;
        private ActorResourceController resources;
        private IClassSkillState[] skillStates = Array.Empty<IClassSkillState>();
        private IManifestationSessionState[] sessionState = Array.Empty<IManifestationSessionState>();
        private IEchoWideDurableState[] echoWideState = Array.Empty<IEchoWideDurableState>();
        private LocalEchoStore store;
        private EchoSave save;
        private bool ready, dirty;
        private int mutationDepth;
        private float due;

        public string Status { get; private set; } = "Persistence inactive";
        public string SavePath => store?.Path;
        public long Revision => save?.revision ?? 0;
        public bool Ready => ready;
        public string MigrationStatus => store?.MigrationMessage;
        public string RecoveryStatus => store?.RecoveryMessage;
        public string CurrentClassId => actor?.Stats?.Definition?.stableId;
        public string EchoName => EchoIdentity.ReadName(save);
        public string DisplayName => EchoIdentity.Format(EchoName, actor?.Stats?.Definition?.displayName);
        public event Action<string> ActiveManifestationChanged;

        private string CurrentSectionId => ManifestationRoster.SectionIdFor(CurrentClassId);
        private IClassSkillState ActiveSkillState => FindSkillState(CurrentClassId);

        public EchoSave CaptureProfile() =>
            save == null ? null : JsonUtility.FromJson<EchoSave>(JsonUtility.ToJson(save));

        public ManifestationRosterSave CaptureRoster() =>
            save == null ? null : ManifestationRoster.Read(save, CurrentClassId);

        public bool HasManifestation(string classId) => ManifestationRoster.HasManifestation(save, classId);

        public void ConfigureClassCatalog(ClassCatalog value)
        {
            classCatalog = value;
            classCatalog?.Validate();
        }

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
            public void Dispose()
            {
                if (owner == null) return;
                owner.mutationDepth--;
                owner = null;
            }
        }

        private void Start()
        {
            try { BindRuntime(); }
            catch (Exception error) { Fail(error); return; }

            string root;
            try { root = EchoSaveLocation.ResolveRoot(); }
            catch (Exception error)
            {
                Status = "Persistence disabled: " + error.Message;
                Debug.LogError(Status);
                return;
            }
            if (root == null)
            {
                Status = "Persistence disabled for this session.";
                return;
            }
            store = new LocalEchoStore(root);
            Status = "Persistence loading";

            try
            {
                save = store.Load();
                bool existing = save != null;
                save ??= new EchoSave();
                if (!existing) EchoIdentity.ApplyPendingName(save, root);
                else EchoIdentity.ClearPendingName();

                RestoreEchoWideState(save);

                string sceneDefaultClassId = CurrentClassId;
                var roster = ManifestationRoster.Read(save, sceneDefaultClassId);
                var activeDefinition = ResolveClass(roster.activeClassId, actor.Stats.Definition);
                if (activeDefinition == null)
                    throw new InvalidDataException("Active manifestation class is not available in the current class catalog: " + roster.activeClassId);

                SetActiveDefinition(activeDefinition);
                var section = save.sections.Find(value => value.id == CurrentSectionId);
                if (section != null)
                {
                    var state = ReadCurrentVersion(section, CurrentClassId);
                    ValidateState(activeDefinition, state);
                    ApplyState(activeDefinition, state);
                }
                else if (existing)
                {
                    throw new InvalidDataException("Current manifestation missing; original Echo preserved.");
                }

                ready = true;
                SubscribeDurableState();
                Status = store.RecoveryMessage ?? "Local autosave ready";
                if (store.RecoveryMessage != null) Debug.LogWarning(store.RecoveryMessage);
                MarkDirty();
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private void BindRuntime()
        {
            xp = GetComponent<ExperienceProgression>();
            journal = GetComponent<QuestJournal>();
            actor = GetComponent<CombatActor>();
            anchor = GetComponent<RespawnAtAnchor>();
            inventory = GetComponent<CarriedInventory>();
            equipment = GetComponent<Equipment>();
            wallet = GetComponent<GoldWallet>();
            resources = GetComponent<ActorResourceController>();

            var skills = new List<IClassSkillState>();
            var transient = new List<IManifestationSessionState>();
            var echo = new List<IEchoWideDurableState>();
            foreach (var behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IClassSkillState skill) skills.Add(skill);
                if (behaviour is IManifestationSessionState state) transient.Add(state);
                if (behaviour is IEchoWideDurableState durable) echo.Add(durable);
            }
            skillStates = skills.ToArray();
            sessionState = transient.ToArray();
            echoWideState = echo.ToArray();
            classCatalog?.Validate();
        }

        private void SubscribeDurableState()
        {
            xp.Changed += MarkDirty;
            if(anchor!=null)anchor.AnchorChanged+=MarkDirty;
            journal.Changed += MarkDirty;
            inventory.Changed += MarkDirty;
            equipment.Changed += MarkDirty;
            wallet.Changed += MarkDirty;
            if (resources != null) resources.DurableChanged += MarkDirty;
            foreach (var skill in skillStates) skill.Changed += MarkDirty;
            foreach (var durable in echoWideState) durable.Changed += MarkDirty;
        }

        private void UnsubscribeDurableState()
        {
            if (xp != null) xp.Changed -= MarkDirty;
            if (anchor != null) anchor.AnchorChanged -= MarkDirty;
            if (journal != null) journal.Changed -= MarkDirty;
            if (inventory != null) inventory.Changed -= MarkDirty;
            if (equipment != null) equipment.Changed -= MarkDirty;
            if (wallet != null) wallet.Changed -= MarkDirty;
            if (resources != null) resources.DurableChanged -= MarkDirty;
            foreach (var skill in skillStates) skill.Changed -= MarkDirty;
            foreach (var durable in echoWideState) durable.Changed -= MarkDirty;
        }

        private void RestoreEchoWideState(EchoSave profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var durable in echoWideState)
            {
                if (durable == null) continue;
                if (string.IsNullOrWhiteSpace(durable.SectionId))
                    throw new InvalidDataException("Echo-wide durable state has no section ID.");
                if (!seen.Add(durable.SectionId))
                    throw new InvalidDataException("Duplicate Echo-wide durable section: " + durable.SectionId);

                var section = profile.sections.Find(value =>
                    value != null && value.id == durable.SectionId);
                if (section == null)
                {
                    durable.ResetToDefault();
                    continue;
                }

                durable.RestoreJson(section.version, section.json);
            }
        }

        private void CaptureEchoWideState(EchoSave profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var durable in echoWideState)
            {
                if (durable == null) continue;
                if (string.IsNullOrWhiteSpace(durable.SectionId) || !seen.Add(durable.SectionId))
                    throw new InvalidDataException("Invalid or duplicate Echo-wide durable section ID.");

                var section = profile.sections.Find(value =>
                    value != null && value.id == durable.SectionId);
                if (section == null)
                {
                    section = new SaveSection { id = durable.SectionId };
                    profile.sections.Add(section);
                }

                section.version = durable.Version;
                section.json = durable.CaptureJson();
            }
        }

        private IClassSkillState FindSkillState(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId)) return null;
            foreach (var state in skillStates)
                if (state != null && state.ClassId == classId) return state;
            return null;
        }

        private ActorDefinition ResolveClass(string classId, ActorDefinition fallback = null)
        {
            if (fallback != null && fallback.stableId == classId) return fallback;
            return classCatalog?.Resolve(classId);
        }

        private void SetActiveDefinition(ActorDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.stableId))
                throw new ArgumentException("Manifestation requires a stable class definition.");

            foreach (var skill in skillStates)
                skill.SetClassActive(skill.ClassId == definition.stableId);

            if (actor.Stats.Definition != definition || actor.Stats.Definition?.stableId != definition.stableId)
                actor.Stats.Configure(definition, 1);
        }

        private static ManifestationSave ReadCurrentVersion(SaveSection section, string expectedClassId)
        {
            if (section.version != SaveMigrations.ManifestationVersion)
                throw new NotSupportedException("Unsupported manifestation version; original preserved.");
            var state = JsonUtility.FromJson<ManifestationSave>(section.json);
            if (state == null || state.classId != expectedClassId)
                throw new InvalidDataException("Manifestation section/class identity mismatch; original preserved.");
            return state;
        }

        private void ValidateState(ActorDefinition definition, ManifestationSave state)
        {
            if (definition == null || state == null || state.classId != definition.stableId ||
                !xp.CanRestore(state.level, state.xp) || !journal.CanRestore(state.quests) ||
                !CarriedInventory.Valid(state.inventory) || !Equipment.Valid(state.equipment, state.inventory) ||
                state.gold < 0)
                throw new InvalidDataException("Invalid manifestation; original preserved.");

            var savedSkills = state.classSkills ?? Array.Empty<SkillRankState>();
            var skillState = FindSkillState(definition.stableId);
            if (skillState != null && !skillState.CanRestore(savedSkills, state.level))
                throw new InvalidDataException("Invalid class skill state; original preserved.");
            if (resources != null && !resources.CanRestoreForClass(definition.stableId,
                    ToRuntimeResources(state.resources), definition, state.level))
                throw new InvalidDataException("Invalid class resource state; original preserved.");
        }

        private void ApplyState(ActorDefinition definition, ManifestationSave state)
        {
            bool keepDungeonPosition = GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.InsideDungeon == true;
            Vector3 dungeonPosition = transform.position;
            ValidateState(definition, state);
            SetActiveDefinition(definition);
            xp.RestoreState(state.level, state.xp);

            var skillState = FindSkillState(definition.stableId);
            skillState?.RestoreState(state.classSkills ?? Array.Empty<SkillRankState>());

            journal.RestoreState(state.quests);
            inventory.Restore(state.inventory);
            equipment.Restore(state.equipment);
            // Restore resources after gear: INT and passive bonuses define the live capacity.
            resources?.RestoreForClass(definition.stableId, ToRuntimeResources(state.resources));
            wallet.Restore(state.gold);
            if (!anchor.LoadAtAnchor(state.anchorId))
                throw new InvalidOperationException("Saved/fallback resurrection point is not navigable.");
            if (keepDungeonPosition && !actor.Motor.TeleportAcrossMaps(dungeonPosition))
                throw new InvalidOperationException("Could not preserve staging position after class preparation.");

            foreach (var transient in sessionState) transient.ResetForManifestationLoad();
        }

        private ManifestationSave CaptureCurrentState(ManifestationSave preserved = null)
        {
            var skillState = ActiveSkillState;
            return new ManifestationSave
            {
                classId = CurrentClassId,
                level = xp.Level,
                xp = xp.CurrentXp,
                quests = journal.CaptureState(),
                anchorId = anchor.AnchorId,
                inventory = inventory.Items,
                equipment = equipment.Slots,
                gold = wallet.Gold,
                classSkills = skillState != null
                    ? PreserveUnknownSkillRanks(skillState.CaptureState(), preserved?.classSkills)
                    : preserved?.classSkills ?? Array.Empty<SkillRankState>(),
                classSkillRankMigrationVersion = CurrentClassId == "class.greatblade" || CurrentClassId == "class.mage"
                    ? SaveMigrations.ClassSkillRankMigrationVersion
                    : preserved?.classSkillRankMigrationVersion ?? 0,
                resources = resources != null
                    ? ToSaveResources(resources.CaptureForActive(ToRuntimeResources(preserved?.resources)))
                    : preserved?.resources ?? Array.Empty<ResourceSaveValue>()
            };
        }

        private static SkillRankState[] PreserveUnknownSkillRanks(SkillRankState[] captured, SkillRankState[] preserved)
        {
            var result = new List<SkillRankState>(captured ?? Array.Empty<SkillRankState>());
            var knownIds = new HashSet<string>(result.Where(value => !string.IsNullOrWhiteSpace(value.stableId))
                .Select(value => value.stableId), StringComparer.Ordinal);
            foreach (var value in preserved ?? Array.Empty<SkillRankState>())
                if (!string.IsNullOrWhiteSpace(value.stableId) && knownIds.Add(value.stableId))
                    result.Add(value);
            return result.ToArray();
        }

        private static RuntimeResourceValue[] ToRuntimeResources(ResourceSaveValue[] saved)
        {
            if (saved == null || saved.Length == 0) return Array.Empty<RuntimeResourceValue>();
            var result = new RuntimeResourceValue[saved.Length];
            for (int i = 0; i < saved.Length; i++)
                result[i] = saved[i] == null ? null : new RuntimeResourceValue { resourceId = saved[i].resourceId, value = saved[i].value };
            return result;
        }

        private static ResourceSaveValue[] ToSaveResources(RuntimeResourceValue[] runtime)
        {
            if (runtime == null || runtime.Length == 0) return Array.Empty<ResourceSaveValue>();
            var result = new ResourceSaveValue[runtime.Length];
            for (int i = 0; i < runtime.Length; i++)
                result[i] = runtime[i] == null ? null : new ResourceSaveValue { resourceId = runtime[i].resourceId, value = runtime[i].value };
            return result;
        }

        public bool TryForkAndActivate(ActorDefinition targetDefinition, out string error)=>
            TryForkInternal(targetDefinition,null,out error);

        public bool TryCompleteAdvancementQuest(ActorDefinition targetDefinition,QuestDefinition quest,out string error)=>
            TryForkInternal(targetDefinition,quest,out error);

        private bool TryForkInternal(ActorDefinition targetDefinition,QuestDefinition completedQuest,out string error)
        {
            if (GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.Locked == true) { error = "Class is locked during this dungeon run."; return false; }
            error = null;
            if (!ready) return Reject("Persistence is not ready.", out error);
            if (mutationDepth != 0) return Reject("A durable mutation is already in progress.", out error);
            if (targetDefinition == null || string.IsNullOrWhiteSpace(targetDefinition.stableId))
                return Reject("Target class definition is missing.", out error);
            if (targetDefinition.stableId == CurrentClassId)
                return Reject("Cannot advance into the current manifestation.", out error);
            if (classCatalog == null || classCatalog.Resolve(targetDefinition.stableId) == null)
                return Reject("Target class is not present in the class catalog.", out error);
            if (ManifestationRoster.HasManifestation(save, targetDefinition.stableId))
                return Reject("That class manifestation already exists.", out error);

            try
            {
                var candidate = CaptureProfile();
                var currentSection = candidate.sections.Find(value => value.id == CurrentSectionId);
                var preserved = currentSection == null ? null : ReadCurrentVersion(currentSection, CurrentClassId);
                var parent = CaptureCurrentState(preserved);
                if(completedQuest!=null)
                {
                    if(!journal.Definitions.Contains(completedQuest)||completedQuest.rewardXp!=0||completedQuest.reward!=null||
                        (!string.IsNullOrEmpty(completedQuest.requiredClassId)&&completedQuest.requiredClassId!=CurrentClassId)||
                        xp.Level<completedQuest.requiredLevel)
                        return Reject("Advancement quest configuration or class eligibility is invalid.",out error);
                    var record=Array.Find(parent.quests,q=>q.questId==completedQuest.stableId);
                    if(record==null||record.definitionVersion!=completedQuest.version||record.status!=QuestStatus.ReadyToTurnIn)
                        return Reject("Complete the advancement quest before returning to your mentor.",out error);
                    record.status=QuestStatus.Completed;
                }
                ValidateState(actor.Stats.Definition, parent);

                var child = ManifestationProfileTransactions.CreateChildSnapshot(parent, targetDefinition.stableId);
                ValidateState(targetDefinition, child);
                ManifestationProfileTransactions.CommitFork(candidate, parent, child, CurrentClassId);
                CaptureEchoWideState(candidate);

                candidate.revision++;
                candidate.writtenUtc = DateTime.UtcNow.ToString("O");
                store.Commit(candidate);

                save = candidate;
                dirty = false;
                ApplyState(targetDefinition, child);
                Status = "Advanced into " + targetDefinition.displayName + " (revision " + save.revision + ")";
                ActiveManifestationChanged?.Invoke(CurrentClassId);
                return true;
            }
            catch (Exception exception)
            {
                return Reject(exception.Message, out error);
            }
        }

        public bool TryActivateExisting(string classId, out string error)
        {
            if (GetComponent<DiceFree.Foundation.RunLoadoutLock>()?.Locked == true) { error = "Class is locked during this dungeon run."; return false; }
            error = null;
            if (!ready) return Reject("Persistence is not ready.", out error);
            if (mutationDepth != 0) return Reject("A durable mutation is already in progress.", out error);
            if (string.IsNullOrWhiteSpace(classId)) return Reject("Target class ID is missing.", out error);
            if (classId == CurrentClassId) return true;

            try
            {
                var targetDefinition = ResolveClass(classId);
                if (targetDefinition == null)
                    return Reject("Target class is not present in the class catalog.", out error);
                if (!ManifestationRoster.HasManifestation(save, classId))
                    return Reject("Target manifestation does not exist.", out error);

                var candidate = CaptureProfile();
                var currentSection = candidate.sections.Find(value => value.id == CurrentSectionId);
                var preserved = currentSection == null ? null : ReadCurrentVersion(currentSection, CurrentClassId);
                var current = CaptureCurrentState(preserved);
                ValidateState(actor.Stats.Definition, current);
                var target = ManifestationRoster.ReadManifestation(candidate, classId);
                ValidateState(targetDefinition, target);
                ManifestationProfileTransactions.SelectExisting(candidate, current, CurrentClassId, classId);
                CaptureEchoWideState(candidate);
                candidate.revision++;
                candidate.writtenUtc = DateTime.UtcNow.ToString("O");
                store.Commit(candidate);

                save = candidate;
                dirty = false;
                ApplyState(targetDefinition, target);
                Status = "Loaded manifestation " + targetDefinition.displayName + " (revision " + save.revision + ")";
                ActiveManifestationChanged?.Invoke(CurrentClassId);
                return true;
            }
            catch (Exception exception)
            {
                return Reject(exception.Message, out error);
            }
        }

        private static bool Reject(string message, out string error)
        {
            error = message;
            return false;
        }

        private void MarkDirty()
        {
            if (!ready) return;
            if (!dirty) due = Time.unscaledTime + debounceSeconds;
            dirty = true;
        }

        private void LateUpdate()
        {
            if (ready && dirty && Time.unscaledTime >= due) Flush();
        }

        public bool Flush()
        {
            if (!ready || mutationDepth != 0) return false;
            try
            {
                var candidate = CaptureProfile();
                var section = candidate.sections.Find(value => value.id == CurrentSectionId);
                var preserved = section == null ? null : ReadCurrentVersion(section, CurrentClassId);
                var state = CaptureCurrentState(preserved);
                ValidateState(actor.Stats.Definition, state);
                ManifestationProfileTransactions.RecordActive(candidate, state, CurrentClassId);
                CaptureEchoWideState(candidate);

                candidate.revision++;
                candidate.writtenUtc = DateTime.UtcNow.ToString("O");
                store.Commit(candidate);
                save = candidate;
                dirty = false;
                Status = "Local save revision " + save.revision;
                return true;
            }
            catch (Exception error)
            {
                Fail(error);
                return false;
            }
        }

        private void Fail(Exception error)
        {
            ready = false;
            Status = "Autosave stopped: " + error.Message + " Existing save files preserved.";
            Debug.LogWarning(Status);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && ready) Flush();
        }

        private void OnApplicationQuit()
        {
            if (ready) Flush();
        }

        private void OnDestroy()
        {
            UnsubscribeDurableState();
        }

        private void OnGUI()
        {
            if (store != null && !ready)
                GUI.Box(new Rect(20, Screen.height / 2f, Screen.width - 40, 70), Status);
        }
    }
}

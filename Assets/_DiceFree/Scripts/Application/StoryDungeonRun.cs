using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.ApplicationFlow;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Foundation;
using DiceFree.Gameplay;
using DiceFree.Items;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Dungeons
{
    public abstract class StoryDungeonRun : MonoBehaviour, IDungeonRewardRun
    {
        protected enum RunPhase { Waiting, Loading, Active, Victory, LoadingRewards, Rewards, Defeated, Leaving }
        private sealed class Member
        {
            public CombatActor actor;
            public Vector3 returnPoint, entryPoint;
            public string mapName;
            public RunLoadoutLock loadout;
            public Action death;
            public int slot;
            public readonly SemanticGameEventHub events = new();
        }

        private const string TreasuryScene = "DungeonRewardRoom";
        private const string OverworldFace = "world.face.1";
        private static readonly Rect TreasuryBounds = new(29988, 29988, 24, 24);
        private static readonly float[] SpawnOffsets = { 0, -2, 2, 4 };
        private static readonly Rect Panel = new(15, 80, 540, 65);
        private static SessionMapRegistry registry = new();

        private readonly Dictionary<CombatActor, Member> members = new();
        private readonly List<CombatActor> roster = new();
        private DungeonWaitingRoom waitingRoom;
        private DungeonOccupancySnapshot lease;
        private InteractionTarget[] arenaTargets = Array.Empty<InteractionTarget>();
        private float deadline, began;
        private bool sufferedDeath;

        protected abstract string DefinitionId { get; }
        protected abstract string ArenaScene { get; }
        protected abstract string Title { get; }
        protected abstract Vector3 ArenaArrival { get; }
        protected abstract Rect ArenaBounds { get; }
        protected abstract DungeonRewardPool RewardPool { get; }
        protected abstract int BonusXp { get; }
        protected abstract int BonusGold { get; }
        protected abstract string ActiveStatus { get; }
        protected abstract void SetupArena(Scene scene);
        protected virtual void StopEncounter() { }
        protected virtual void TickEncounter() { }

        public CombatActor Initiator { get; private set; }
        public bool StagingReady => Phase == RunPhase.Waiting && waitingRoom != null;
        public float StagingSecondsRemaining => StagingReady ? Mathf.Max(0, deadline - Time.time) : 0;
        protected IReadOnlyList<CombatActor> Participants => roster;
        protected RunPhase Phase { get; private set; }
        protected bool IsParticipant(CombatActor actor) => actor != null && members.ContainsKey(actor);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => registry = new SessionMapRegistry();

        protected string Join(CombatActor actor, Vector3 returnPoint, Vector3 roomOrigin)
        {
            if (actor == null || !actor.Alive || actor.GetComponent<TraversalInput>() == null)
                return "Only a living player can enter.";
            if (IsParticipant(actor)) return "You are already in this run.";
            if (Phase != RunPhase.Waiting || LoadingScreen.Busy || actor.GetComponent<RunLoadoutLock>()?.Owner != null)
                return "Finish the current journey before entering.";
            if (members.Count >= SpawnOffsets.Length) return "The waiting room is full.";

            string party = actor.GetComponent<LocalPartyMember>()?.partyId;
            if (string.IsNullOrEmpty(party)) party = "solo:" + actor.GetEntityId();
            string actorId = actor.GetEntityId().ToString();
            if (!registry.TryClaimDungeonStaging(DefinitionId, party, actorId, DateTime.UtcNow, out var claim, out var error))
                return error;
            lease = claim;

            if (waitingRoom == null)
            {
                var prefab = Resources.Load<GameObject>(DungeonWaitingRoom.ResourcePath);
                if (prefab == null || !prefab.TryGetComponent<DungeonWaitingRoom>(out var layout))
                {
                    ReleaseOccupancy();
                    return "The waiting room is unavailable.";
                }
                waitingRoom = Instantiate(layout, roomOrigin, Quaternion.identity);
                SceneManager.MoveGameObjectToScene(waitingRoom.gameObject, gameObject.scene);
                if (!waitingRoom.Ready)
                {
                    DestroyWaitingRoom();
                    ReleaseOccupancy();
                    return "The waiting room has no walkable arrival.";
                }
                waitingRoom.CreatePracticeTarget();
                Initiator = actor;
                deadline = Time.time + DungeonWaitingRoom.Seconds;
            }

            var entry = actor.transform.position;
            var mapName = actor.GetComponent<MinimapHud>()?.MapName;
            int slot = Enumerable.Range(0, SpawnOffsets.Length).First(index => !members.Values.Any(member => member.slot == index));
            if (!Relocate(actor, waitingRoom.Arrival + Vector3.right * SpawnOffsets[slot],
                    Title + " · Waiting Room", waitingRoom.Bounds, true))
            {
                registry.TryLeaveDungeon(DefinitionId, lease.leaseToken, actorId, OverworldFace, out _);
                if (members.Count == 0) { DestroyWaitingRoom(); ReleaseOccupancy(); }
                return "Could not reach the waiting room.";
            }

            var member = new Member
            {
                actor = actor, returnPoint = returnPoint, entryPoint = entry, mapName = mapName, slot = slot,
                loadout = actor.GetComponent<RunLoadoutLock>() ?? actor.gameObject.AddComponent<RunLoadoutLock>()
            };
            members.Add(actor, member);
            roster.Add(actor);
            member.loadout.Owner = this;
            member.loadout.InsideDungeon = true;
            member.loadout.Locked = false;
            member.loadout.DeathReturnHandler = () => RequestDeathReturn(actor);
            member.death = () => OnDeath(member);
            actor.Health.Died += member.death;
            actor.GetComponent<QuestJournal>()?.BindDungeonEvents(member.events);
            actor.GetComponent<CodexProgression>()?.Bind(member.events);
            return null;
        }

        public bool StartNow(CombatActor requester)
        {
            if (!StagingReady || requester == null || requester != Initiator || !requester.Alive || LoadingScreen.Busy)
                return false;
            var living = roster.Where(actor => actor != null && actor.Alive).ToArray();
            var context = new DungeonConditionContext
            {
                participants = living.Select(actor => new DungeonParticipantSnapshot
                {
                    participantId = actor.GetEntityId().ToString(),
                    classId = actor.Stats.Definition.stableId,
                    level = actor.Stats.Level
                }).ToArray()
            };
            var variant = new DungeonRunVariantSnapshot { dungeonId = DefinitionId, averagePartyLevel = context.AveragePartyLevel };
            if (!registry.TryBeginDungeon(DefinitionId, lease.leaseToken, variant, DateTime.UtcNow, out var activeLease, out _))
                return false;
            lease = activeLease;
            foreach (var actor in living) members[actor].loadout.Locked = true;
            Publish(new DungeonStartedEvent(DefinitionId,roster.Count));
            Phase = RunPhase.Loading;
            if (!LoadingScreen.TryLoadAdditive(ArenaScene, "Entering " + Title, ArenaLoaded, LoadFailed))
                LoadFailed(new InvalidOperationException("Travel is busy."));
            return Phase == RunPhase.Loading;
        }

        private void ArenaLoaded(Scene scene)
        {
            if (this == null || Phase != RunPhase.Loading)
            {
                if (scene.isLoaded) SceneManager.UnloadSceneAsync(scene);
                return;
            }

            SetupArena(scene);
            arenaTargets = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<InteractionTarget>(true)).ToArray();
            var living = roster.Where(actor => actor != null && actor.Alive).ToArray();
            if (living.Length == 0) { Finish(); return; }
            for (int i = 0; i < living.Length; i++)
            {
                var actor = living[i];
                if (!Relocate(actor, ArenaArrival + Vector3.right * SpawnOffsets[i], Title, ArenaBounds, true))
                    throw new InvalidOperationException("No reachable arena arrival.");
                var interaction = actor.GetComponent<InteractionRegistry>();
                if (interaction != null)
                    foreach (var target in arenaTargets) interaction.Register(target);
            }
            foreach (var actor in roster.Where(actor => actor != null && !actor.Alive).ToArray())
                ReturnPlayer(actor, false);
            DestroyWaitingRoom();
            began = Time.time;
            Phase = RunPhase.Active;
        }

        private void Update()
        {
            if (Phase == RunPhase.Leaving) return;
            if (lease != null) registry.TouchDungeon(DefinitionId, lease.leaseToken, DateTime.UtcNow, out _);
            if (StagingReady)
            {
                if (Initiator == null || !Initiator.Alive) Initiator = roster.FirstOrDefault(actor => actor != null && actor.Alive);
                if (Time.time >= deadline) StartNow(Initiator);
            }
            else if (Phase == RunPhase.Active) TickEncounter();
            else if (Phase == RunPhase.Victory && Time.time >= deadline && !LoadingScreen.Busy) OpenTreasury();
        }

        protected void Complete(string bossId, string variant, int xp = 0, int gold = 0)
        {
            if (Phase != RunPhase.Active) return;
            if (!registry.TryCompleteDungeon(DefinitionId, lease.leaseToken, DateTime.UtcNow, out var error))
            {
                LoadFailed(new InvalidOperationException(error));
                return;
            }
            Phase = RunPhase.Victory;
            deadline = Time.time + 15;
            Publish(new DungeonBossKilledEvent(DefinitionId, bossId));
            Publish(new DungeonCompletedEvent(DefinitionId, variant, roster.Count, Time.time - began, !sufferedDeath));
            foreach (var actor in roster)
            {
                if (actor == null) continue;
                if (xp > 0) actor.GetComponent<ExperienceProgression>()?.Grant(xp);
                if (gold > 0) actor.GetComponent<GoldWallet>()?.Grant(gold);
            }
        }

        protected void Publish<T>(T fact) where T : struct, ISemanticGameEvent
        {
            foreach (var participant in members.Values) participant.events.Publish(fact);
        }
        // Intermediate guardians grant quest facts without completing the run.
        protected void PublishBossKill(string bossId,Func<CombatActor,bool> eligible)
        {
            foreach(var member in members.Values)
                if(member.actor!=null&&eligible(member.actor))member.events.Publish(new DungeonBossKilledEvent(DefinitionId,bossId));
        }

        public void RequestLeave(CombatActor actor)
        {
            if (!IsParticipant(actor)) return;
            if (Phase is RunPhase.Waiting or RunPhase.Active)
            {
                ReturnPlayer(actor, true);
                CheckRemaining();
            }
        }

        private void OpenTreasury()
        {
            Phase = RunPhase.LoadingRewards;
            var existing = SceneManager.GetSceneByName(TreasuryScene);
            if (existing.isLoaded) TreasuryLoaded(existing);
            else if (!LoadingScreen.TryLoadAdditive(TreasuryScene, "Opening the dungeon treasury", TreasuryLoaded, LoadFailed))
                Phase = RunPhase.Victory;
        }

        private void TreasuryLoaded(Scene scene)
        {
            if (this == null || Phase != RunPhase.LoadingRewards) return;
            try
            {
                var room = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DungeonRewardRoom>()).Single();
                var pool = RewardPool;
                if (pool == null) throw new InvalidOperationException("No dungeon rewards were authored.");
                Phase = RunPhase.Rewards;
                int index = 0;
                foreach (var actor in roster.ToArray())
                {
                    if (actor == null) continue;
                    if (!actor.Alive) { ReturnPlayer(actor, false); continue; }
                    var arrival = SlimeDungeonRun.RewardOrigin + new Vector3(SpawnOffsets[index++], 0, -2);
                    if (!Relocate(actor, arrival, "Dungeon Treasury", TreasuryBounds, true))
                        throw new InvalidOperationException("No reachable treasury arrival.");
                    members[actor].loadout.Locked = false;
                    room.Offer(actor, pool, this, BonusXp, BonusGold);
                }
                UnloadArena();
                if (members.Count == 0) Finish();
            }
            catch (Exception error) { LoadFailed(error); }
        }

        private void OnDeath(Member member)
        {
            if (Phase == RunPhase.Leaving) return;
            sufferedDeath = true;
            member.events.Publish(new DungeonPlayerDiedEvent(DefinitionId));
            CheckRemaining();
        }

        private void CheckRemaining()
        {
            if (members.Count == 0) { Finish(); return; }
            if (Phase is not (RunPhase.Active or RunPhase.Waiting or RunPhase.Loading) ||
                roster.Any(actor => actor != null && actor.Alive)) return;
            Phase = RunPhase.Defeated;
            StopEncounter();
            Publish(new DungeonWipedEvent(DefinitionId));
        }

        private bool RequestDeathReturn(CombatActor actor)
        {
            if (!IsParticipant(actor) || actor.Alive) return false;
            if (Phase == RunPhase.Rewards && FindAnyObjectByType<DungeonRewardRoom>()?.DiscardForSessionExit(actor) == true)
                return true;
            ReturnPlayer(actor, false);
            CheckRemaining();
            return !IsParticipant(actor);
        }

        public void ObserveReward(CombatActor actor, ItemDefinition item)
        {
            if (members.TryGetValue(actor, out var member))
                member.events.Publish(new DropObservedEvent(DefinitionId, item.stableId, true));
        }

        public void Drop(CombatActor actor, ItemDefinition item)
        {
            if (!IsParticipant(actor) || !actor.TryGetComponent<CarriedInventory>(out var inventory)) return;
            if (inventory.Resolve(item.stableId) == null)
                inventory.Configure(inventory.Definitions.Append(item).ToArray());
            inventory.Grant(item.stableId);
        }

        public void ReturnPlayer(CombatActor actor, bool atEntrance)
        {
            if (actor == null || !members.TryGetValue(actor, out var member)) return;
            var respawn = actor.GetComponent<RespawnAtAnchor>();
            var world = OverworldMapBounds.ForScene(actor.gameObject.scene, new Rect(-56, -44, 181, 116));
            bool moved = actor.Alive && atEntrance
                ? Relocate(actor, member.returnPoint, member.mapName, world, false) ||
                  Relocate(actor, member.entryPoint, member.mapName, world, false)
                : !actor.Alive && respawn != null && respawn.ReturnAtInstanceExit(member.entryPoint);
            if (!moved) moved = respawn != null && respawn.LoadAtAnchor(respawn.AnchorId);
            if (!moved) return;

            ConfigureView(actor, member.mapName, world, false);
            registry.TryLeaveDungeon(DefinitionId, lease?.leaseToken, actor.GetEntityId().ToString(), OverworldFace, out _);
            ClearMember(member);
            roster.Remove(actor);
            members.Remove(actor);
        }

        private static bool Relocate(CombatActor actor, Vector3 target, string map, Rect bounds, bool inside)
        {
            actor.GetComponent<PlayerCommandQueue>()?.ClearOrders();
            actor.GetComponent<BasicAttack>()?.Cancel();
            actor.GetComponent<TargetSelection>()?.Select(null);
            actor.GetComponent<Interactor>()?.Cancel();
            if (!actor.Motor.TeleportAcrossMaps(target)) return false;
            ConfigureView(actor, map, bounds, inside);
            return true;
        }

        private static void ConfigureView(CombatActor actor, string title, Rect bounds, bool inside)
        {
            if (actor.GetComponent<MinimapHud>() is { } map) map.MapName = title ?? "Cornberg";
            if (actor.GetComponent<CombatInput>()?.isActiveAndEnabled != true) return;
            FindAnyObjectByType<WorldDayNightCycle>()?.SetInterior(inside);
            if (FindAnyObjectByType<ExplorationCamera>() is { } camera)
            {
                camera.ConfigureMapBounds(bounds);
                camera.RecenterForTravel(actor.transform);
            }
        }

        private void ClearMember(Member member)
        {
            var actor = member.actor;
            if (actor == null) return;
            actor.Health.Died -= member.death;
            var interaction = actor.GetComponent<InteractionRegistry>();
            if (interaction != null)
                foreach (var target in arenaTargets)
                    if (target != null) interaction.Unregister(target);
            actor.GetComponent<QuestJournal>()?.UnbindDungeonEvents(member.events);
            actor.GetComponent<CodexProgression>()?.Unbind();
            if (member.loadout.Owner != this) return;
            member.loadout.Locked = false;
            member.loadout.InsideDungeon = false;
            member.loadout.DeathReturnHandler = null;
            member.loadout.Owner = null;
        }

        private void LoadFailed(Exception error)
        {
            Debug.LogError(Title + " travel failed. " + error.Message, this);
            Finish();
        }

        public void Finish()
        {
            if (Phase == RunPhase.Leaving) return;
            Phase = RunPhase.Leaving;
            StopEncounter();
            var room = FindAnyObjectByType<DungeonRewardRoom>();
            foreach (var actor in roster.ToArray())
            {
                if (actor == null) continue;
                if (room != null && room.HasChoice(actor)) room.DiscardForSessionExit(actor);
                if (IsParticipant(actor)) ReturnPlayer(actor, actor.Alive);
            }
            ReleaseOccupancy();
            DestroyWaitingRoom();
            UnloadArena();
            var treasury = SceneManager.GetSceneByName(TreasuryScene);
            if (treasury.isLoaded && room?.Pending == 0) SceneManager.UnloadSceneAsync(treasury);
            Destroy(gameObject);
        }

        public void ReleaseForSessionExit()
        {
            Phase = RunPhase.Leaving;
            StopEncounter();
            ReleaseOccupancy();
            foreach (var member in members.Values) ClearMember(member);
            roster.Clear();
            members.Clear();
        }

        private void ReleaseOccupancy()
        {
            if (lease == null) return;
            registry.ReleaseDungeon(DefinitionId, lease.leaseToken, OverworldFace, out _);
            lease = null;
        }

        private void DestroyWaitingRoom()
        {
            if (waitingRoom == null) return;
            Destroy(waitingRoom.gameObject);
            waitingRoom = null;
        }

        private void UnloadArena()
        {
            var arena = SceneManager.GetSceneByName(ArenaScene);
            if (arena.isLoaded) SceneManager.UnloadSceneAsync(arena);
        }

        private CombatActor LocalPlayer =>
            roster.FirstOrDefault(actor => actor != null && actor.GetComponent<CombatInput>()?.isActiveAndEnabled == true);

        private bool ShowsPanel => StagingReady || Phase is RunPhase.Active or RunPhase.Victory;
        private bool CoversPointer(Vector2 point) => ShowsPanel && LocalPlayer != null &&
            Panel.Contains(new Vector2(point.x, Screen.height - point.y));

        protected virtual void OnEnable() => HudPointerBlocker.OverlayCovers += CoversPointer;
        protected virtual void OnDisable() => HudPointerBlocker.OverlayCovers -= CoversPointer;

        private void OnGUI()
        {
            if (!ShowsPanel || HudPointerBlocker.ModalOpen || LocalPlayer is not { } local) return;
            string status = StagingReady
                ? $"{Title} · Shared staging · {Mathf.CeilToInt(StagingSecondsRemaining)}s\nPractise on the dummy and prepare your gear."
                : Phase == RunPhase.Victory ? $"Clear! Resurrection window · {Mathf.CeilToInt(Mathf.Max(0, deadline - Time.time))}s"
                : ActiveStatus;
            GUI.Box(Panel, status);
            if (Phase is RunPhase.Waiting or RunPhase.Active && GUI.Button(new Rect(25, 115, 150, 25), "Abandon dungeon"))
                RequestLeave(local);
            if (StagingReady)
            {
                var enabled = GUI.enabled;
                GUI.enabled = enabled && local == Initiator && local.Alive;
                if (GUI.Button(new Rect(185, 115, 150, 25), "Start Now")) StartNow(local);
                GUI.enabled = enabled;
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var member in members.Values) ClearMember(member);
            ReleaseOccupancy();
            DestroyWaitingRoom();
        }
    }
}

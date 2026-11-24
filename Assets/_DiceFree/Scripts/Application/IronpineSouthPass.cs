using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Foundation;
using DiceFree.Gameplay;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.ApplicationFlow
{
    public sealed class IronpineSouthPass : InteractionTarget
    {
        public const string DestinationScene = "WorldTwoArrival";
        public const string DestinationName = "World 2 · Ashfall Reach";
        public static readonly Rect DestinationBounds = AshfallReach.Bounds;
        private const float FireDuration = 5f;
        private const float TreesClearAfter = 3.45f;

        [SerializeField] private GameObject trees;
        [SerializeField] private GameObject fire;
        [SerializeField] private GameObject cleared;
        [SerializeField] private Transform returnPoint;
        [SerializeField] private Transform arrivalPoint;
        [SerializeField] private bool returnToIronpine;

        private CombatActor player;
        private QuestJournal journal;
        private InteractionRegistry registry;
        private IronpineSouthPass entrance;
        private IronpineSouthPass exit;
        private ExplorationCamera explorationCamera;
        private MinimapHud minimap;
        private Scene destination;
        private string originalMapName;
        private readonly List<InteractionTarget> destinationTargets = new();
        private bool loading;
        private bool burnPending;
        private bool progressChanged;
        private float fireEndsAt;

        public bool IsOpen => cleared != null && cleared.activeSelf;
        public bool IsBurning => fire != null && fire.activeSelf;

        private static bool DragonQuestComplete(QuestJournal value) =>
            value?.GetProgress(AncientRoadDragonLairPortal.QuestId)?.status == QuestStatus.Completed;

        public void ConfigureEntrance(GameObject grove, GameObject controlledFire, GameObject ashes, Transform back)
        {
            trees = grove;
            fire = controlledFire;
            cleared = ashes;
            returnPoint = back;
        }

        public void ConfigureExit(Transform arrival)
        {
            returnToIronpine = true;
            arrivalPoint = arrival;
        }

        private void Start()
        {
            if (returnToIronpine) return;
            player = FindObjectsByType<TraversalInput>()
                .FirstOrDefault(input => input.gameObject.scene == gameObject.scene)?.GetComponent<CombatActor>();
            if (player == null) return;

            journal = player.GetComponent<QuestJournal>();
            registry = player.GetComponent<InteractionRegistry>();
            explorationCamera = FindAnyObjectByType<ExplorationCamera>();
            minimap = player.GetComponent<MinimapHud>();
            registry?.Register(this);
            if (journal == null) return;
            journal.Changed += OnJournalChanged;
            journal.GameplayProgressed += OnQuestProgressed;
            RefreshGate();
        }

        private void OnDestroy()
        {
            if (journal == null) return;
            journal.Changed -= OnJournalChanged;
            journal.GameplayProgressed -= OnQuestProgressed;
        }

        private void OnJournalChanged() => progressChanged = true;

        private void OnQuestProgressed(QuestDefinition quest, QuestProgress before, QuestProgress after)
        {
            if (quest.stableId != AncientRoadDragonLairPortal.QuestId ||
                before.status == QuestStatus.Completed || after.status != QuestStatus.Completed) return;
            // The clerk and the pass are far apart. Play the short burn when the hero reaches the grove.
            burnPending = true;
        }

        private void RefreshGate()
        {
            progressChanged = false;
            var unlocked = DragonQuestComplete(journal);
            if (!unlocked)
            {
                burnPending = false;
                fireEndsAt = 0;
                fire.SetActive(false);
            }
            var open = unlocked && !burnPending && fireEndsAt == 0;
            if (!IsBurning) trees.SetActive(!open);
            cleared.SetActive(open);
        }

        private void Update()
        {
            if (returnToIronpine || player == null) return;
            if (progressChanged) RefreshGate();
            if (burnPending && player.Alive &&
                (player.transform.position - fire.transform.position).sqrMagnitude <= 12f * 12f)
            {
                burnPending = false;
                fireEndsAt = Time.time + FireDuration;
                fire.transform.localScale = Vector3.one;
                fire.SetActive(true);
            }
            if (fireEndsAt > 0)
            {
                float elapsed = FireDuration - (fireEndsAt - Time.time);
                float flicker = 1 + .085f*Mathf.Sin(Time.time*19) + .045f*Mathf.Sin(Time.time*33);
                float height = Mathf.Lerp(flicker, .1f, Mathf.InverseLerp(TreesClearAfter, FireDuration, elapsed));
                fire.transform.localScale = new Vector3(1, height, 1);
                if (elapsed >= TreesClearAfter && trees.activeSelf) trees.SetActive(false);
            }
            if (fireEndsAt > 0 && Time.time >= fireEndsAt)
            {
                fireEndsAt = 0;
                fire.SetActive(false);
                RefreshGate();
            }

            // Return spells, resurrection and manifestation changes can relocate the hero directly.
            if (exit != null)
            {
                var position = player.transform.position;
                if (!DestinationBounds.Contains(new Vector2(position.x, position.z)))
                    ReleaseDestination();
            }
        }

        public override bool Available(CombatActor actor)
        {
            if (!base.Available(actor)) return false;
            if (returnToIronpine) return entrance != null && entrance.player == actor && !entrance.loading;
            return actor == player && IsOpen && DragonQuestComplete(journal) && !loading && !destination.isLoaded;
        }

        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor) || LoadingScreen.Busy || WorldFaceCrossing.Busy ||
                HudPointerBlocker.ModalOpen || !actor.CanAct || actor.InCombat) return;
            if (returnToIronpine)
            {
                entrance.Return();
                return;
            }
            if (actor.GetComponent<RunLoadoutLock>()?.Owner != null) return;

            StartCrossing(true);
        }

        private void StartCrossing(bool towardWorldTwo)
        {
            if (loading) return;
            loading = true;
            CancelOrders();
            StartCoroutine(Cross(towardWorldTwo));
        }

        private IEnumerator Cross(bool towardWorldTwo)
        {
            var presentation = GetComponent<WorldFaceCrossing>() ?? gameObject.AddComponent<WorldFaceCrossing>();
            bool ready = !towardWorldTwo, cancelled = false;
            Exception failure = null;
            try
            {
                try { presentation.Begin(player, explorationCamera, towardWorldTwo); }
                catch (Exception error) { failure = error; }
                if (failure != null) { OnLoadFailed(failure); yield break; }
                yield return presentation.PullBack();
                if (towardWorldTwo)
                {
                    presentation.WaitingForScene = true;
                    bool requested = LoadingScreen.TryLoadAdditive(DestinationScene, "Crossing to World 2", scene =>
                    {
                        if (cancelled || this == null || !isActiveAndEnabled || player == null)
                        {
                            if (scene.isLoaded) SceneManager.UnloadSceneAsync(scene);
                            return;
                        }
                        destination = scene;
                        ready = true;
                    }, error => failure = error, false);
                    if (!requested) failure = new InvalidOperationException("The World 2 load could not start.");
                }
                yield return presentation.Turn();
                float deadline = Time.realtimeSinceStartup + 45;
                while (!ready && failure == null && Time.realtimeSinceStartup < deadline) yield return null;
                if (!ready && failure == null) failure = new TimeoutException("World 2 did not finish loading.");
                if (failure == null)
                {
                    try
                    {
                        if (towardWorldTwo) Arrive(destination);
                        else
                        {
                            if (!player.Motor.TeleportAcrossMaps(returnPoint.position))
                                throw new InvalidOperationException("The World 1 return point is unavailable.");
                            CancelOrders();
                            ReleaseDestination();
                        }
                    }
                    catch (Exception error) { failure = error; }
                }
                if (failure != null) { OnLoadFailed(failure); yield break; }
                yield return presentation.Settle();
            }
            finally
            {
                cancelled = true;
                presentation.End();
                loading = false;
            }
        }

        private void Arrive(Scene scene)
        {
            destination = scene;
            if (this == null || player == null || !player.Alive || player.InCombat || !DragonQuestComplete(journal))
            {
                if (scene.isLoaded) SceneManager.UnloadSceneAsync(scene);
                destination = default;
                throw new InvalidOperationException("The crossing is no longer available for this hero.");
            }

            var landing = scene.GetRootGameObjects()
                .Select(root => root.GetComponentInChildren<IronpineSouthPass>())
                .FirstOrDefault(portal => portal != null && portal.returnToIronpine);
            if (landing == null || landing.arrivalPoint == null ||
                !player.Motor.TeleportAcrossMaps(landing.arrivalPoint.position))
                throw new InvalidOperationException("The World 2 landing could not place the hero.");

            CancelOrders();
            exit = landing;
            exit.entrance = this;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var target in root.GetComponentsInChildren<InteractionTarget>())
            {
                if (target.WorldTarget != target || registry == null || registry.Contains(target)) continue;
                registry.Register(target);
                destinationTargets.Add(target);
            }
            originalMapName = minimap?.MapName;
            SetView(DestinationBounds, DestinationName);
        }

        private void OnLoadFailed(Exception error)
        {
            if (this == null) return;
            if (destination.isLoaded) SceneManager.UnloadSceneAsync(destination);
            destination = default;
            loading = false;
            Debug.LogError($"Could not open the Ironpine passage to World 2. {error.Message}", this);
        }

        private void Return()
        {
            StartCrossing(false);
        }

        private void CancelOrders()
        {
            player.GetComponent<PlayerCommandQueue>()?.ClearOrders();
            player.GetComponent<BasicAttack>()?.Cancel();
            player.GetComponent<TargetSelection>()?.Select(null);
            player.GetComponent<SkillTargetingController>()?.Cancel();
            player.GetComponent<Interactor>()?.Cancel();
            player.Motor.Stop();
        }

        private void ReleaseDestination()
        {
            exit.entrance = null;
            foreach (var target in destinationTargets) registry?.Unregister(target);
            destinationTargets.Clear();
            exit = null;
            SetView(OverworldMapBounds.ForScene(gameObject.scene, new Rect(-56, -44, 181, 116)),
                originalMapName ?? "Cornberg");
            if (destination.isLoaded) SceneManager.UnloadSceneAsync(destination);
        }

        private void SetView(Rect bounds, string mapName)
        {
            if (explorationCamera != null)
            {
                explorationCamera.ConfigureMapBounds(bounds);
                explorationCamera.RecenterForTravel(player.transform);
            }
            if (minimap != null) minimap.MapName = mapName;
        }
    }
}

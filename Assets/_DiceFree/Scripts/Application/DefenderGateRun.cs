using System;
using System.Collections;
using System.Linq;
using DiceFree.ApplicationFlow;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Foundation;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Advancement
{
    public enum DefenderGatePhase { Loading, Muster, Assault, Returning }

    [DisallowMultipleComponent]
    public sealed class DefenderGateRun : MonoBehaviour
    {
        public static DefenderGateRun Current { get; private set; }
        public DefenderGatePhase Phase { get; private set; } = DefenderGatePhase.Loading;
        public DefenderGateSite Site { get; private set; }
        public int WaveNumber { get; private set; }
        public int RaidersLeft => Phase == DefenderGatePhase.Assault ? CurrentWave.raiders.Count(actor => actor.Alive) : 0;

        private CombatActor player;
        private QuestDefinition quest;
        private RunLoadoutLock loadout;
        private ExplorationCamera cameraRig;
        private MinimapHud minimap;
        private Scene scene;
        private Vector3 departure;
        private Rect previousBounds;
        private string previousMapName;
        private float nextWaveAt, ordersAt, waveDeadline;
        private bool relocated;
        private DefenderGateSite.Wave CurrentWave => Site.waves[WaveNumber - 1];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => Current = null;

        internal static bool TryBegin(CombatActor actor, QuestDefinition trial, Vector3 returnPoint, out string feedback)
        {
            feedback = "";
            if (actor == null || !actor.CanAct || actor.InCombat || Current != null || LoadingScreen.Busy ||
                actor.GetComponent<RunLoadoutLock>()?.Owner != null ||
                SceneManager.GetSceneByName(DefenderGateSite.SceneName).isLoaded)
            {
                feedback = "Finish your current fight or journey before joining the watch.";
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(DefenderGateSite.SceneName))
            {
                feedback = "The gate yard is unavailable in this build.";
                return false;
            }

            var run = new GameObject("Trimery gate defense session").AddComponent<DefenderGateRun>();
            SceneManager.MoveGameObjectToScene(run.gameObject, actor.gameObject.scene);
            Current = run;
            run.player = actor;
            run.quest = trial;
            run.departure = returnPoint;
            run.cameraRig = FindAnyObjectByType<ExplorationCamera>();
            run.previousBounds = run.cameraRig != null ? run.cameraRig.MapBounds : default;
            run.minimap = actor.GetComponent<MinimapHud>();
            run.previousMapName = run.minimap?.MapName;
            run.loadout = actor.GetComponent<RunLoadoutLock>() ?? actor.gameObject.AddComponent<RunLoadoutLock>();
            run.loadout.Owner = run;
            run.loadout.Locked = run.loadout.InsideDungeon = true;
            run.loadout.DeathReturnHandler = () => false;
            if (LoadingScreen.TryLoadAdditive(DefenderGateSite.SceneName, "Mustering at Trimery's north gate",
                run.OnLoaded, run.OnLoadFailed))
            {
                DefenderGateNotice.Clear();
                return true;
            }
            run.ReleaseLock();
            Current = null;
            Destroy(run.gameObject);
            feedback = "Please wait for the current journey.";
            return false;
        }

        private void OnLoaded(Scene loaded)
        {
            if (this == null) return;
            scene = loaded;
            Site = loaded.GetRootGameObjects().Select(root => root.GetComponent<DefenderGateSite>()).Single(value => value != null);
            if (player == null || !player.Alive || !MovePlayer(Site.playerSpawn))
                throw new InvalidOperationException("No usable entry to the Trimery gate yard.");
            relocated = true;
            SetMap(Site.mapBounds, "Trimery North Gate");
            Muster(4);
        }

        private void OnLoadFailed(Exception error)
        {
            if (this == null) return;
            Debug.LogError("Trimery gate defense failed to load: " + error.Message);
            StartCoroutine(Exit(false, "The gate yard could not open. Speak to the veteran to retry."));
        }

        private void Muster(float seconds)
        {
            Phase = DefenderGatePhase.Muster;
            nextWaveAt = Time.time + seconds;
        }

        private void Update()
        {
            if (Phase != DefenderGatePhase.Muster && Phase != DefenderGatePhase.Assault) return;
            if (player == null || !player.Alive || Site.workers.Any(worker => !worker.Alive) ||
                Site.guards.All(guard => !guard.Alive))
            {
                StartCoroutine(Exit(false, "The watch fell. Speak to the veteran to try again."));
                return;
            }

            if (Phase == DefenderGatePhase.Muster)
            {
                if (Time.time >= nextWaveAt) StartWave();
                return;
            }

            if (RaidersLeft == 0)
            {
                foreach (var guard in Site.guards) guard.GetComponent<BasicAttack>().Cancel();
                if (WaveNumber == Site.waves.Length)
                    StartCoroutine(Exit(true, "The north gate held. Report back to the veteran."));
                else Muster(3);
                return;
            }

            if (Time.time > waveDeadline)
            {
                StartCoroutine(Exit(false, "The attackers broke through. Speak to the veteran to try again."));
                return;
            }

            if (Time.time < ordersAt) return;
            ordersAt = Time.time + .30f;
            SetOrders();
        }

        private void StartWave()
        {
            Phase = DefenderGatePhase.Assault;
            WaveNumber++;
            waveDeadline = Time.time + 95;
            foreach (var enemy in CurrentWave.raiders)
            {
                enemy.gameObject.SetActive(true);
                var attack = enemy.GetComponent<BasicAttack>();
                enemy.Health.Damaged += (source, _) =>
                {
                    if (source == player && enemy.Alive && player.Alive) attack.Order(player);
                };
            }
            SetOrders();
        }

        private void SetOrders()
        {
            var attackers = CurrentWave.raiders;
            for (int i = 0; i < attackers.Length; i++)
            {
                var raider = attackers[i];
                if (!raider.Alive) continue;
                var attack = raider.GetComponent<BasicAttack>();
                if (attack.Target != null && attack.Target.Alive) continue;
                // The outside lanes head for the workers. A guard ties up the centre.
                var charge = i % 3 == 1 ? Site.guards[i % Site.guards.Length] : Site.workers[i % Site.workers.Length];
                if (!charge.Alive) charge = Site.workers.First(worker => worker.Alive);
                attack.Order(charge);
            }

            foreach (var guard in Site.guards)
            {
                if (!guard.Alive) continue;
                var attack = guard.GetComponent<BasicAttack>();
                if (attack.Target != null && attack.Target.Alive) continue;
                var nearby = attackers.Where(enemy => enemy.Alive &&
                    (enemy.transform.position - guard.transform.position).sqrMagnitude <= 9f * 9f)
                    .OrderBy(enemy => (enemy.transform.position - guard.transform.position).sqrMagnitude).FirstOrDefault();
                if (nearby != null) attack.Order(nearby);
            }
        }

        public void Abandon()
        {
            if (Phase is DefenderGatePhase.Muster or DefenderGatePhase.Assault)
                StartCoroutine(Exit(false, "You left the watch. The veteran can send you back."));
        }

        private IEnumerator Exit(bool success, string message)
        {
            if (Phase == DefenderGatePhase.Returning) yield break;
            Phase = DefenderGatePhase.Returning;
            if (Site != null)
            {
                foreach (var guard in Site.guards) guard.GetComponent<BasicAttack>().Cancel();
                foreach (var wave in Site.waves)
                    foreach (var enemy in wave.raiders) enemy.gameObject.SetActive(false);
            }

            if (player != null)
            {
                if (relocated || !player.Alive)
                {
                    bool returned = player.Alive ? MovePlayer(departure) :
                        player.GetComponent<RespawnAtAnchor>().ReturnAtInstanceExit(departure);
                    if (!returned)
                    {
                        success = false;
                        message = "You returned to your resurrection stone. Speak to the veteran to retry.";
                        if (!player.GetComponent<RespawnAtAnchor>().LoadAtAnchor(null))
                        {
                            message = "The gate exit could not find a return point. Reload your Echo to return to the world.";
                            Debug.LogError("Trimery gate defense has no usable return point.");
                        }
                    }
                }

                if (success && !RecordCompletion()) message = "The defense could not be saved. Speak to the veteran to retry.";
                SetMap(previousBounds, previousMapName);
                ReleaseLock();
                DefenderGateNotice.Show(message);
            }
            if (scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            Destroy(gameObject);
        }

        private bool RecordCompletion()
        {
            var journal = player.GetComponent<QuestJournal>();
            var before = journal.CaptureState();
            journal.CreditEscort(DefenderGateSite.TrialId);
            if (journal.GetProgress(quest.stableId)?.status == QuestStatus.ReadyToTurnIn &&
                player.GetComponent<ManifestationPersistence>().Flush()) return true;
            journal.RestoreState(before);
            return false;
        }

        private bool MovePlayer(Vector3 point)
        {
            player.GetComponent<BasicAttack>()?.Cancel();
            player.GetComponent<TargetSelection>()?.Select(null);
            player.GetComponent<SkillTargetingController>()?.Cancel();
            player.GetComponent<Interactor>()?.Cancel();
            foreach (var component in player.GetComponents<MonoBehaviour>())
                if (component is IQueuedOrders orders) orders.ClearOrders();
            return player.Motor.TeleportAcrossMaps(point);
        }

        private void SetMap(Rect bounds, string mapName)
        {
            if (cameraRig != null)
            {
                cameraRig.ConfigureMapBounds(bounds);
                cameraRig.RecenterForTravel(player.transform);
            }
            if (minimap != null) minimap.MapName = mapName;
        }

        private void ReleaseLock()
        {
            if (loadout == null || loadout.Owner != this) return;
            loadout.Locked = loadout.InsideDungeon = false;
            loadout.Owner = null;
            loadout.DeathReturnHandler = null;
        }

        private static Rect Overlay => new(15, 78, Mathf.Min(Screen.width - 30, 424), 132);
        private bool Covers(Vector2 pointer) => (Phase is DefenderGatePhase.Muster or DefenderGatePhase.Assault) &&
            Overlay.Contains(new Vector2(pointer.x, Screen.height - pointer.y));
        private void OnEnable() => HudPointerBlocker.OverlayCovers += Covers;
        private void OnDisable() => HudPointerBlocker.OverlayCovers -= Covers;
        private void OnDestroy()
        {
            ReleaseLock();
            if (Current == this) Current = null;
        }

        private void OnGUI()
        {
            if (Site == null || LoadingScreen.Busy || HudPointerBlocker.ModalOpen || Phase == DefenderGatePhase.Returning) return;
            var box = Overlay;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            HudChrome.DrawPanel(box, theme);
            HudChrome.DrawHeader(new Rect(box.x + 5, box.y + 5, box.width - 10, 25), theme, "Hold the North Gate");
            string progress = Phase == DefenderGatePhase.Muster
                ? $"Wave {WaveNumber + 1}/{Site.waves.Length} in {Mathf.CeilToInt(nextWaveAt - Time.time)}s  ·  {Site.waves[WaveNumber].warning}"
                : $"Wave {WaveNumber}/{Site.waves.Length}  ·  Raiders left {RaidersLeft}";
            GUI.Label(new Rect(box.x + 12, box.y + 34, box.width - 24, 23), progress);
            GUI.Label(new Rect(box.x + 12, box.y + 59, box.width - 24, 23),
                $"Workers  {Percent(Site.workers[0])}% / {Percent(Site.workers[1])}%  ·  Watch  {Site.guards.Count(guard => guard.Alive)}/{Site.guards.Length}");
            GUI.Label(new Rect(box.x + 12, box.y + 89, box.width - 140, 23), "Keep both workers and a guard alive.");
            if (GUI.Button(new Rect(box.xMax - 108, box.y + 88, 96, 25), "Leave trial")) Abandon();
        }

        private static int Percent(CombatActor actor) => Mathf.CeilToInt(100 * actor.Health.Current / actor.Health.Maximum);
    }
}
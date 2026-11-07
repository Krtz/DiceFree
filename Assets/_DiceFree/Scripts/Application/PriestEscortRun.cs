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
    public enum PriestEscortPhase { Loading, Walking, Fighting, Returning }

    public sealed class PriestEscortRun : MonoBehaviour
    {
        public static PriestEscortRun Current { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => Current = null;

        private CombatActor player;
        private QuestDefinition quest;
        private RunLoadoutLock loadout;
        private ExplorationCamera cameraRig;
        private MinimapHud minimap;
        private string previousMapName;
        private Rect previousBounds;
        private Scene instance;
        private Vector3 departure, arrival;
        private PriestEscortSite site;
        private int waypoint, nextEncounter;
        private float nextOrder, stalledSince, previousDistance;
        private bool relocated;
        public PriestEscortPhase Phase { get; private set; } = PriestEscortPhase.Loading;
        public PriestEscortSite Site => site;
        public int EncounterNumber => Phase == PriestEscortPhase.Fighting ? nextEncounter + 1 : nextEncounter;
        public int RouteWaypoint => waypoint;
        public bool WaitingForPlayer { get; private set; }
        private static Rect Overlay => new(15, 78, 390, 106);

        internal static bool TryBegin(CombatActor player, QuestDefinition quest, Vector3 departure, Vector3 arrival, out string message)
        {
            message = "";
            if (player == null || !player.Alive || quest == null || player.InCombat ||
                Current != null || LoadingScreen.Busy || SceneManager.GetSceneByName(PriestEscortSite.SceneName).isLoaded ||
                player.GetComponent<RunLoadoutLock>()?.Owner != null)
            {
                message = "Finish your current combat or journey before departing.";
                return false;
            }
            if (!Application.CanStreamedLevelBeLoaded(PriestEscortSite.SceneName))
            {
                message = "The escort road is unavailable in this build.";
                return false;
            }

            var run = new GameObject("Priest escort session").AddComponent<PriestEscortRun>();
            SceneManager.MoveGameObjectToScene(run.gameObject, player.gameObject.scene);
            Current = run;
            run.player = player;
            run.quest = quest;
            run.departure = departure;
            run.arrival = arrival;
            run.cameraRig = FindAnyObjectByType<ExplorationCamera>();
            run.previousBounds = run.cameraRig != null ? run.cameraRig.MapBounds : default;
            run.minimap = player.GetComponent<MinimapHud>();
            run.previousMapName = run.minimap?.MapName;
            run.loadout = player.GetComponent<RunLoadoutLock>() ?? player.gameObject.AddComponent<RunLoadoutLock>();
            run.loadout.Owner = run;
            run.loadout.InsideDungeon = run.loadout.Locked = true;
            run.loadout.DeathReturnHandler = () => false;
            if (LoadingScreen.TryLoadAdditive(PriestEscortSite.SceneName, "Joining the Cornberg road inspection", run.OnLoaded, run.OnLoadFailed))
                return true;
            run.ReleaseLock();
            Current = null;
            Destroy(run.gameObject);
            message = "Please wait for the current journey.";
            return false;
        }

        private void OnLoaded(Scene scene)
        {
            if (this == null) return;
            instance = scene;
            site = scene.GetRootGameObjects().Select(root => root.GetComponent<PriestEscortSite>()).Single(value => value != null);
            if (player == null || !player.Alive || !MovePlayer(site.playerSpawn))
                throw new InvalidOperationException("The party could not enter the escort road.");
            relocated = true;
            SetMap(site.mapBounds, "Trimery–Cornberg escort");
            Phase = PriestEscortPhase.Walking;
            ResetProgress();
        }

        private void OnLoadFailed(Exception error)
        {
            if (this == null) return;
            Debug.LogError("Priest escort could not load: " + error.Message);
            StartCoroutine(Exit(false, "The escort could not start. Speak to the healer to try again."));
        }

        private void Update()
        {
            if (Phase != PriestEscortPhase.Walking && Phase != PriestEscortPhase.Fighting) return;
            if (player == null || !player.Alive || !site.inspector.Alive)
            {
                StartCoroutine(Exit(false, "Escort failed. The Trimery healer can arrange another attempt."));
                return;
            }
            if (Time.time < nextOrder) return;
            nextOrder = Time.time + .25f;
            if (Phase == PriestEscortPhase.Walking) March();
            else Fight();
        }

        private void March()
        {
            var inspector = site.inspector;
            var destination = site.route[waypoint];
            WaitingForPlayer = Vector3.Distance(player.transform.position, inspector.transform.position) > 23;
            if (WaitingForPlayer)
            {
                inspector.Motor.Stop();
                foreach (var guard in site.guards.Where(actor => actor.Alive)) guard.Motor.Stop();
                ResetProgress();
                return;
            }

            float distance = Vector3.Distance(inspector.transform.position, destination);
            if (distance <= .85f)
            {
                if (nextEncounter < site.encounters.Length && site.encounters[nextEncounter].waypoint == waypoint)
                {
                    inspector.Motor.Stop();
                    foreach (var guard in site.guards.Where(actor => actor.Alive)) guard.Motor.Stop();
                    foreach (var hostile in site.encounters[nextEncounter].hostiles)
                    {
                        hostile.gameObject.SetActive(true);
                        var attack = hostile.GetComponent<BasicAttack>();
                        hostile.Health.Damaged += (source, _) =>
                        {
                            if (source == player && hostile.Alive) attack.Order(player);
                        };
                    }
                    Phase = PriestEscortPhase.Fighting;
                    return;
                }
                if (++waypoint >= site.route.Length)
                {
                    StartCoroutine(Exit(true, "The inspector found nothing unusual. Return to the Trimery healer."));
                    return;
                }
                destination = site.route[waypoint];
                ResetProgress();
            }
            else if (distance < previousDistance - .12f)
            {
                previousDistance = distance;
                stalledSince = Time.time;
            }
            else if (Time.time - stalledSince > 16)
            {
                StartCoroutine(Exit(false, "The party could not continue. Speak to the Trimery healer to retry."));
                return;
            }

            if (!inspector.Motor.Travelling) inspector.Motor.MoveTo(destination);
            Vector3 heading = Vector3.ProjectOnPlane(destination - inspector.transform.position, Vector3.up).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, heading) * 1.8f;
            for (int i = 0; i < site.guards.Length; i++)
            {
                var guard = site.guards[i];
                if (!guard.Alive) continue;
                var position = inspector.transform.position - heading * .8f + side * (i == 0 ? -1 : 1);
                if ((guard.transform.position - position).sqrMagnitude > 1.1f) guard.Motor.MoveTo(position);
                else guard.Motor.Stop();
            }
        }

        private void Fight()
        {
            var group = site.encounters[nextEncounter].hostiles;
            var living = group.Where(hostile => hostile.Alive).ToArray();
            if (living.Length == 0)
            {
                foreach (var hostile in group) hostile.gameObject.SetActive(false);
                foreach (var guard in site.guards.Where(actor => actor.Alive)) guard.GetComponent<BasicAttack>().Cancel();
                nextEncounter++;
                Phase = PriestEscortPhase.Walking;
                ResetProgress();
                return;
            }

            foreach (var guard in site.guards.Where(actor => actor.Alive))
            {
                var attack = guard.GetComponent<BasicAttack>();
                if (attack.Target == null || !attack.Target.Alive)
                    attack.Order(living.OrderBy(enemy => (enemy.transform.position - guard.transform.position).sqrMagnitude).First());
            }
            for (int i = 0; i < group.Length; i++)
            {
                var enemy = group[i];
                if (!enemy.Alive) continue;
                var attack = enemy.GetComponent<BasicAttack>();
                if (attack.Target != null && attack.Target.Alive) continue;
                // The first attacker pressures the inspector. The guards absorb the rest as long as they stand.
                var guard = site.guards[(i - 1 + site.guards.Length) % site.guards.Length];
                var target = i == 0 ? site.inspector : guard.Alive ? guard : site.guards.FirstOrDefault(actor => actor.Alive) ?? site.inspector;
                attack.Order(target);
            }
        }

        private void ResetProgress()
        {
            previousDistance = float.PositiveInfinity;
            stalledSince = Time.time;
        }

        public void Abandon()
        {
            if (Phase == PriestEscortPhase.Walking || Phase == PriestEscortPhase.Fighting)
                StartCoroutine(Exit(false, "Escort abandoned. Speak to the healer to try again."));
        }

        private IEnumerator Exit(bool success, string notice)
        {
            if (Phase == PriestEscortPhase.Returning) yield break;
            Phase = PriestEscortPhase.Returning;
            if (site != null)
            {
                site.inspector.Motor.Stop();
                foreach (var guard in site.guards) guard.GetComponent<BasicAttack>().Cancel();
                foreach (var group in site.encounters)
                    foreach (var hostile in group.hostiles) hostile.gameObject.SetActive(false);
            }

            if (player != null)
            {
                if (success && !RecordCompletion())
                {
                    success = false;
                    notice = "The escort could not be saved. Speak to the healer to retry.";
                }
                if (relocated || !player.Alive)
                {
                    Vector3 exit = success ? arrival : departure;
                    bool returned = player.Alive ? MovePlayer(exit) : player.GetComponent<RespawnAtAnchor>().ReturnAtInstanceExit(exit);
                    if (!returned && !player.GetComponent<RespawnAtAnchor>().LoadAtAnchor(null))
                    {
                        ReleaseLock();
                        PriestEscortNotice.Show("The road exit is unavailable. Reload your Echo to return to the world.");
                        Debug.LogError("The Priest escort has no usable return point.");
                        yield break;
                    }
                }
                SetMap(previousBounds, previousMapName);
                ReleaseLock();
                PriestEscortNotice.Show(notice);
            }
            if (instance.isLoaded) yield return SceneManager.UnloadSceneAsync(instance);
            Destroy(gameObject);
        }

        private bool RecordCompletion()
        {
            var journal = player.GetComponent<QuestJournal>();
            var snapshot = journal.CaptureState();
            journal.CreditEscort(PriestEscortSite.RouteId);
            if (journal.GetProgress(quest.stableId)?.status == QuestStatus.ReadyToTurnIn &&
                player.GetComponent<ManifestationPersistence>().Flush()) return true;
            journal.RestoreState(snapshot);
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

        private void SetMap(Rect bounds, string name)
        {
            if (cameraRig != null)
            {
                cameraRig.ConfigureMapBounds(bounds);
                cameraRig.RecenterForTravel(player.transform);
            }
            if (minimap != null) minimap.MapName = name;
        }

        private void ReleaseLock()
        {
            if (loadout == null || loadout.Owner != this) return;
            loadout.Locked = loadout.InsideDungeon = false;
            loadout.Owner = null;
            loadout.DeathReturnHandler = null;
        }

        private void OnEnable() => HudPointerBlocker.OverlayCovers += Covers;
        private void OnDisable() => HudPointerBlocker.OverlayCovers -= Covers;
        private void OnDestroy()
        {
            if (Current == this) Current = null;
            ReleaseLock();
        }
        private bool Covers(Vector2 pointer) => (Phase == PriestEscortPhase.Walking || Phase == PriestEscortPhase.Fighting) &&
            Overlay.Contains(new Vector2(pointer.x, Screen.height - pointer.y));

        private void OnGUI()
        {
            if (site == null || LoadingScreen.Busy || HudPointerBlocker.ModalOpen || Phase == PriestEscortPhase.Returning) return;
            var box = Overlay;
            GUI.Box(box, "THE CORNBERG ROAD INSPECTION");
            int livingGuards = site.guards.Count(guard => guard.Alive);
            string status = Phase == PriestEscortPhase.Fighting ?
                livingGuards == 0 ? "Defeat the remaining attackers. Protect the inspector." :
                $"Ambush {nextEncounter + 1} of {site.encounters.Length}  •  Keep the inspector alive" :
                WaitingForPlayer ? "The party is waiting for you" : "Follow the inspector toward Cornberg";
            GUI.Label(new Rect(box.x + 12, box.y + 26, box.width - 24, 22), status);
            int hp = Mathf.CeilToInt(100 * site.inspector.Health.Current / site.inspector.Health.Maximum);
            GUI.Label(new Rect(box.x + 12, box.y + 51, box.width - 24, 22),
                $"Inspector  {hp}%   •   Guards  {livingGuards}/{site.guards.Length}");
            if (GUI.Button(new Rect(box.xMax - 130, box.y + 76, 118, 23), "Leave escort")) Abandon();
        }
    }
}

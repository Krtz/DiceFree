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
    public enum MirrorTrialPhase { Entering, Observe, Duel, Leaving }

    [DisallowMultipleComponent]
    public sealed class PsionMirrorRun : MonoBehaviour
    {
        public static PsionMirrorRun Current { get; private set; }
        public MirrorTrialPhase Phase { get; private set; }
        public PsionMirrorSite Site { get; private set; }
        public CombatActor RealReflection { get; private set; }

        private CombatActor player;
        private RunLoadoutLock loadout;
        private ExplorationCamera cameraRig;
        private MinimapHud minimap;
        private Rect previousBounds;
        private string previousMap, outcome;
        private Vector3 departure;
        private Scene trialScene;
        private BasicAttack duelAttack;
        private int realIndex, struck;
        private float observedAt, ordersAt, previousZoom;
        private bool relocated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => Current = null;

        internal static bool TryBegin(CombatActor actor, Vector3 returnPoint, out string feedback)
        {
            if (actor == null || !actor.CanAct || actor.InCombat || Current != null || LoadingScreen.Busy ||
                actor.GetComponent<RunLoadoutLock>()?.Owner != null ||
                SceneManager.GetSceneByName(PsionMirrorSite.SceneName).isLoaded)
            {
                feedback = "Finish your current fight or journey before entering the chamber.";
                return false;
            }
            if (!Application.CanStreamedLevelBeLoaded(PsionMirrorSite.SceneName))
            {
                feedback = "The mirror chamber is unavailable in this build.";
                return false;
            }

            var run = new GameObject("Arborium mirror trial session").AddComponent<PsionMirrorRun>();
            SceneManager.MoveGameObjectToScene(run.gameObject,actor.gameObject.scene);
            Current = run;
            run.player = actor;
            run.departure = returnPoint;
            run.cameraRig = FindAnyObjectByType<ExplorationCamera>();
            run.previousBounds = run.cameraRig != null ? run.cameraRig.MapBounds : default;
            run.previousZoom = run.cameraRig != null ? run.cameraRig.ZoomDistance : 0;
            run.minimap = actor.GetComponent<MinimapHud>();
            run.previousMap = run.minimap?.MapName;
            run.loadout = actor.GetComponent<RunLoadoutLock>() ?? actor.gameObject.AddComponent<RunLoadoutLock>();
            run.loadout.Owner = run;
            run.loadout.Locked = run.loadout.InsideDungeon = true;
            run.loadout.DeathReturnHandler = () => false;
            if (LoadingScreen.TryLoadAdditive(PsionMirrorSite.SceneName,"Entering the Arborium mirror chamber",
                run.OnLoaded,run.OnLoadFailed))
            {
                feedback = "";
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
            trialScene = loaded;
            Site = loaded.GetRootGameObjects().Select(root=>root.GetComponent<PsionMirrorSite>()).Single(site=>site != null);
            if (Site.reflections.Length != 3 || !MovePlayer(Site.playerSpawn))
                throw new InvalidOperationException("The mirror chamber is missing its reflections or entry.");
            relocated = true;
            if (cameraRig != null) cameraRig.ZoomDistance = Mathf.Min(previousZoom,29);
            SetMap(Site.mapBounds,"Arborium Mirror Chamber");

            realIndex = UnityEngine.Random.Range(0,Site.reflections.Length);
            RealReflection = Site.reflections[realIndex];
            for (int i=0;i<Site.reflections.Length;i++)
            {
                int mask = 1 << i;
                var reflection = Site.reflections[i];
                reflection.gameObject.SetActive(true);
                reflection.Health.PacketResolved += (source,packet) =>
                {
                    if (Phase == MirrorTrialPhase.Observe && source == player && packet.applied > 0) struck |= mask;
                };
            }
            duelAttack = RealReflection.GetComponent<BasicAttack>();
            observedAt = Time.time;
            Phase = MirrorTrialPhase.Observe;
        }

        private void OnLoadFailed(Exception error)
        {
            if (this == null) return;
            Debug.LogError("Mirror trial load failed: "+error.Message);
            StartCoroutine(Exit(false,"The chamber could not open. Ilyra can send you back."));
        }

        private void Update()
        {
            if (Phase is not (MirrorTrialPhase.Observe or MirrorTrialPhase.Duel)) return;
            if (player == null || !player.Alive)
            {
                StartCoroutine(Exit(false,"The reflection prevailed. Speak to Ilyra to try again."));
                return;
            }
            if (Phase == MirrorTrialPhase.Observe)
            {
                var copiedTurn = Quaternion.Euler(0,180+48*Mathf.Sin((Time.time-observedAt)*1.85f),0);
                foreach (var reflection in Site.reflections)
                {
                    var toPlayer = Vector3.ProjectOnPlane(player.transform.position-reflection.transform.position,Vector3.up);
                    var turn = reflection == RealReflection && toPlayer.sqrMagnitude > .001f
                        ? Quaternion.LookRotation(toPlayer) : copiedTurn;
                    reflection.transform.rotation = Quaternion.RotateTowards(reflection.transform.rotation,turn,290*Time.deltaTime);
                }
                return;
            }
            if (!RealReflection.Alive)
            {
                StartCoroutine(Exit(true,"The real reflection fell. Report to Ilyra at the Guild."));
                return;
            }
            if (Time.time < ordersAt) return;
            ordersAt = Time.time+.30f;
            duelAttack.Order(player);
        }

        private void LateUpdate()
        {
            if (Phase != MirrorTrialPhase.Observe || struck == 0) return;
            if (struck != 1 << realIndex)
            {
                StartCoroutine(Exit(false,"A false reflection shattered. Watch for the one that tracks you."));
                return;
            }
            foreach (var reflection in Site.reflections)
                if (reflection != RealReflection) reflection.gameObject.SetActive(false);
            Phase = MirrorTrialPhase.Duel;
        }

        public void Abandon()
        {
            if (Phase is MirrorTrialPhase.Observe or MirrorTrialPhase.Duel)
                StartCoroutine(Exit(false,"You left the chamber. Ilyra can send you back."));
        }

        private IEnumerator Exit(bool success, string message)
        {
            if (Phase == MirrorTrialPhase.Leaving) yield break;
            Phase = MirrorTrialPhase.Leaving;
            if (Site != null)
                foreach (var reflection in Site.reflections)
                    reflection.gameObject.SetActive(false);

            if (player != null)
            {
                if (relocated || !player.Alive)
                {
                    var anchor = player.GetComponent<RespawnAtAnchor>();
                    bool returned = player.Alive ? MovePlayer(departure) : anchor.ReturnAtInstanceExit(departure);
                    if (!returned)
                    {
                        success = false;
                        message = "The trial return point was blocked. Speak to Ilyra to retry.";
                        if (!anchor.LoadAtAnchor(null))
                            Debug.LogError("The mirror trial could not find a valid return point.");
                    }
                }
                if (success && !RecordCompletion()) message = "The trial could not be saved. Speak to Ilyra to retry.";
                if (relocated && cameraRig != null) cameraRig.ZoomDistance = previousZoom;
                SetMap(previousBounds,previousMap);
                ReleaseLock();
            }
            if (trialScene.isLoaded) yield return SceneManager.UnloadSceneAsync(trialScene);
            if (Current == this) Current = null;
            outcome = message;
            yield return new WaitForSecondsRealtime(5);
            Destroy(gameObject);
        }

        private bool RecordCompletion()
        {
            var journal = player.GetComponent<QuestJournal>();
            var before = journal.CaptureState();
            journal.CreditPuzzle(PsionMirrorSite.PuzzleId);
            if (journal.GetProgress(PsionMentor.QuestId)?.status == QuestStatus.ReadyToTurnIn &&
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
            foreach (var behaviour in player.GetComponents<MonoBehaviour>())
            {
                if (behaviour is IQueuedOrders orders) orders.ClearOrders();
                if (behaviour is IInterruptibleCombatCast cast) cast.TryInterruptCast();
            }
            return player.Motor.TeleportAcrossMaps(point);
        }

        private void SetMap(Rect bounds,string name)
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

        private static Rect Overlay => new(16,78,Mathf.Min(Screen.width-32,410),108);
        private bool Covers(Vector2 point) => Phase is MirrorTrialPhase.Observe or MirrorTrialPhase.Duel &&
            Overlay.Contains(new Vector2(point.x,Screen.height-point.y));
        private void OnEnable() => HudPointerBlocker.OverlayCovers += Covers;
        private void OnDisable() => HudPointerBlocker.OverlayCovers -= Covers;
        private void OnDestroy()
        {
            ReleaseLock();
            if (Current == this) Current = null;
        }

        private void OnGUI()
        {
            if (LoadingScreen.Busy || HudPointerBlocker.ModalOpen) return;
            if (!string.IsNullOrEmpty(outcome))
            {
                float width = Mathf.Min(Screen.width-24,520);
                GUI.Box(new Rect((Screen.width-width)*.5f,74,width,31),outcome);
                return;
            }
            if (Phase is not (MirrorTrialPhase.Observe or MirrorTrialPhase.Duel)) return;
            var box = Overlay;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            HudChrome.DrawPanel(box,theme);
            HudChrome.DrawHeader(new Rect(box.x+5,box.y+5,box.width-10,25),theme,"The Third Reflection");
            string text = Phase == MirrorTrialPhase.Observe
                ? "Move sideways. Two turn together, one follows you. Strike that one."
                : "You found it. Defeat the real reflection.";
            GUI.Label(new Rect(box.x+12,box.y+35,box.width-24,40),text,new GUIStyle(GUI.skin.label) { wordWrap = true });
            if (GUI.Button(new Rect(box.xMax-110,box.y+72,98,26),"Leave trial")) Abandon();
        }
    }
}

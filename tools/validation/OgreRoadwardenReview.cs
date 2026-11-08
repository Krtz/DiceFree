using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.ApplicationFlow;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Advancement;
using DiceFree.Dungeons;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Trimery;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class OgreRoadwardenReview
{
    const string SurveyId = "quest.arborium.road-survey";
    const string LogPath = "Logs/OgreRoadwardenReview.json";
    const string Visuals = "VisualBaselines/OgreRoadwarden";
    static readonly Vector3 EastGate = new(184,0,305), NorthOfJam = new(203,0,197),
        SouthOfJam = new(204,0,189), TrimeryGate = new(217,0,180);
    static readonly List<string> errors = new();
    static CombatActor player;
    static int checks;
    static string phase;
    static float closedMeters, openMeters, walkedMeters;
    static QuestJournal Journal => player.GetComponent<QuestJournal>();
    static Interactor Input => player.GetComponent<Interactor>();
    static QuestDefinition Quest => Journal.Definitions.Single(q => q.stableId == OgreRoadwardenRoute.QuestId);
    static QuestProgress State => Journal.GetProgress(OgreRoadwardenRoute.QuestId);
    static OgreRoadwardenRoute Route => Object.FindAnyObjectByType<OgreRoadwardenRoute>();
    static TimedInteractionTarget Repair => Route.GetComponent<TimedInteractionTarget>();
    static OgreRoadwardenEncounter Boss => Object.FindAnyObjectByType<OgreRoadwardenEncounter>();

    public static string Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Use a saved scene in Edit Mode.");
        var guard = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t => t != null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,
            new object[] { Path.Combine(Path.GetTempPath(), "DiceFree-Roadwarden-" + Guid.NewGuid()) });
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        EditorApplication.EnterPlaymode();
        return "Entered Play Mode with isolated saves";
    }

    public static string Run()
    {
        checks = 0;
        errors.Clear();
        Application.logMessageReceived += Log;
        _ = Execute();
        return "Roadwarden review is running. See " + LogPath;
    }

    static void Need(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }

    static void Log(string message, string trace, LogType kind)
    {
        if (kind is LogType.Assert or LogType.Error or LogType.Exception) errors.Add(message);
    }

    static void Record(string status, string error = null)
    {
        File.WriteAllText(LogPath, Newtonsoft.Json.JsonConvert.SerializeObject(
            new { status, phase, checks, closedMeters, openMeters, walkedMeters, error, unityErrors = errors },
            Newtonsoft.Json.Formatting.Indented));
    }

    static void Phase(string name) { phase = name; Record("running"); }

    static async Task Until(Func<bool> condition, string message, int seconds = 35)
    {
        var until = DateTime.UtcNow.AddSeconds(seconds);
        while (!condition())
        {
            if (DateTime.UtcNow > until) throw new TimeoutException(message);
            await Task.Delay(20);
        }
    }

    static Vector3 Sample(Vector3 position)
    {
        if (!NavMesh.SamplePosition(position, out var hit, 1.1f, 1))
            throw new InvalidOperationException("No navigation at " + position);
        return hit.position;
    }

    static float Distance(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        Need(NavMesh.CalculatePath(Sample(from), Sample(to), 1, path) && path.status == NavMeshPathStatus.PathComplete,
            "Complete path from " + from + " to " + to);
        return path.corners.Zip(path.corners.Skip(1), Vector3.Distance).Sum();
    }

    static void Move(Vector3 position)
    {
        Need(player.Motor.TeleportAcrossMaps(Sample(position)), "Fixture travel to " + position);
        Physics.SyncTransforms();
        Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(player.transform);
    }

    static async Task Walk(Vector3 position)
    {
        position = Sample(position);
        Need(player.Motor.MoveTo(position), "Walk order to " + position);
        var previous = player.transform.position;
        await Until(() =>
        {
            var now = player.transform.position;
            walkedMeters += Vector3.Distance(previous, now);
            previous = now;
            return Vector3.Distance(now, position) < .48f;
        }, "Actual walk to " + position);
        player.Motor.Stop();
    }

    static async Task BindPlayer()
    {
        await Until(() => (player = Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>()) != null &&
            player.Motor.Ready && player.GetComponent<ManifestationPersistence>().Ready && !LoadingScreen.Busy,
            "Ready isolated player");
        player.GetComponent<TraversalInput>().enabled = false;
        player.GetComponent<CombatInput>().enabled = false;
    }

    static async Task Resume()
    {
        Need(player.GetComponent<ReturnToStartMenu>().Return(), "Save and return to start menu");
        StartMenuController menu = null;
        await Until(() => (menu = Object.FindAnyObjectByType<StartMenuController>()) != null && !LoadingScreen.Busy, "Start menu ready");
        Need(menu.TryPlayManifestation(menu.Entries.First(e => e.active).classId), "Resume manifestation");
        await BindPlayer();
    }

    static async Task Capture(string name)
    {
        Directory.CreateDirectory(Visuals);
        ScreenCapture.CaptureScreenshot(Path.Combine(Visuals, name + ".png"));
        await Task.Delay(170);
    }

    static bool CrossingBlocked()
    {
        var path = new NavMeshPath();
        return !NavMesh.CalculatePath(Sample(NorthOfJam), Sample(SouthOfJam), 1, path) ||
            path.status != NavMeshPathStatus.PathComplete ||
            path.corners.Zip(path.corners.Skip(1), Vector3.Distance).Sum() > 12f;
    }

    static async Task BeginRepair()
    {
        Need(Input.Order(Repair), "Order normal crew interaction");
        await Until(() => Repair.Worker == player, "Player began repairing");
    }

    static async Task Execute()
    {
        var viewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var view = Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();
        var gizmos = viewType?.GetProperty("drawGizmos", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var originalGizmos = view == null ? null : gizmos?.GetValue(view);
        if (view != null) gizmos?.SetValue(view, false);
        try
        {
            Phase("locked approach and available detour");
            await BindPlayer();
            await Until(CrossingBlocked, "Wreck carves the old passage");
            closedMeters = Distance(EastGate, TrimeryGate);
            Need(!Route.Reopened && Boss == null && !Repair.Available(player), "Closed route and hidden encounter without the contract");
            Need(!Journal.CanAccept(Quest), "Complete the survey first");
            Need(Distance(new Vector3(256,0,205),TrimeryGate) < 65, "Old north route stays connected");
            Move(new Vector3(203.5f,0,196.2f));
            await Capture("01-closed-highway");

            Phase("Guild offer and Arborium-side approach");
            var survey = Journal.Definitions.Single(q => q.stableId == SurveyId);
            var record = new QuestProgress { questId = survey.stableId, definitionVersion = survey.version,
                status = QuestStatus.Completed, stage = survey.stages.Length-1, count = survey.stages.Last().count,
                alternatives = survey.stages.Last().alternatives.Select(a => new ObjectiveCount { objectiveId = a.objectiveId, count = a.count }).ToArray() };
            Journal.RestoreState(Journal.CaptureState().Where(p => p.questId != SurveyId).Append(record).ToArray());
            var guild = Object.FindObjectsByType<QuestGiver>().Single(g => g.Offers.Contains(Quest));
            Move(guild.ApproachPosition);
            Need(Input.Order(guild), "Open the Guild Steward interaction");
            await Until(() => Input.Active == guild, "Talk to the Guild Steward");
            Need(guild.Accept(player,Quest), "Accept Roadwarden contract from the Steward");
            Input.Cancel();
            await Until(() => Boss != null && Boss.EncounterActor.Motor.Ready, "Boss spawns at the active contract site");
            player.Stats.SetEncounterMaximumHp(450);
            player.Health.SetEncounterHp(450);
            Move(new Vector3(202.4f,0,221));
            await Walk(new Vector3(202.2f,0,213.5f));

            Phase("telegraphed attacks and dodges");
            var fight = Boss;
            var ogre = fight.EncounterActor;
            var outline = fight.GetComponentInChildren<LineRenderer>();
            player.Health.SetEncounterHp(player.Health.Maximum);
            float hits = 0;
            player.Health.Damaged += (source, damage) => { if (source == ogre) hits += damage.applied; };
            await Until(() => fight.CastRemaining > 1f && fight.CastLabel.StartsWith("Hammerfall"), "Hammerfall windup");
            Need(outline.enabled && outline.positionCount == 22 && !ogre.Motor.MotionAllowed, "Visible locked hammer lane");
            var side = (outline.GetPosition(21) - outline.GetPosition(0)).normalized;
            var start = player.transform.position;
            var dodge = start + side * 4.1f;
            if (!NavMesh.SamplePosition(dodge,out var safe,.7f,1) || NavMesh.Raycast(start,safe.position,out _,1))
                dodge = start - side * 4.1f;
            await Capture("02-hammerfall");
            float beforeDodge = hits;
            await Walk(dodge);
            await Until(() => fight.CastRemaining == 0, "Hammerfall resolves");
            Need(hits == beforeDodge && !outline.enabled && ogre.Motor.MotionAllowed, "Sidestepping the lane avoids its damage");
            await Until(() => fight.CastRemaining > 1f && fight.CastLabel.StartsWith("Roadbreak"), "Roadbreak windup");
            Need(outline.enabled && outline.positionCount == 40, "Visible stomp ring");
            float beforeHit = hits;
            await Capture("03-roadbreak");
            await Until(() => fight.CastRemaining == 0, "Roadbreak resolves");
            Need(hits > beforeHit && player.Alive, "Standing in the stomp ring takes damage");

            Phase("defeat, reload before repair, cancel work");
            ogre.Health.ApplyPacket(player, new DamageResult { mitigated = ogre.Health.Current });
            Need(!ogre.Alive && State.stage == 1 && State.status == QuestStatus.Active && Repair.Available(player),
                "Normal credited defeat unlocks repair");
            Need(CrossingBlocked() && !Route.Reopened, "Killing the Ogre leaves the wreck in place");
            await Resume();
            await Until(CrossingBlocked, "Wreck carves after pre-repair resume");
            Need(State.stage == 1 && State.status == QuestStatus.Active && Boss == null && Repair.Available(player),
                "Defeat survives reload and leaves repair pending");
            Move(Repair.ApproachPosition);
            await BeginRepair();
            Input.Cancel();
            await Until(() => Repair.Worker == null, "Cancel work");
            Need(State.status == QuestStatus.Active && CrossingBlocked(), "Cancelled repair cannot open the road");

            Phase("complete repair and cross physically");
            int xp = player.GetComponent<ExperienceProgression>().CurrentXp;
            await BeginRepair();
            await Capture("04-repair-crew");
            await Until(() => Route.Reopened && !CrossingBlocked(), "Repair opens a walkable route");
            Need(State.status == QuestStatus.Completed && !Repair.Available(player) && !Input.Order(Repair),
                "Complete quest once and hide completed interaction");
            Need(player.GetComponent<ExperienceProgression>().CurrentXp != xp, "Repair rewards XP");
            openMeters = Distance(EastGate, TrimeryGate);
            Need(closedMeters - openMeters > 12, $"Repaired highway shortens the route ({closedMeters} vs {openMeters})");
            Move(new Vector3(203.5f,0,196.2f));
            await Capture("05-repaired-highway");
            float before = walkedMeters;
            await Walk(NorthOfJam);
            await Walk(SouthOfJam);
            var road = TravelSurface.Active.Single(s => s.StableId == "surface-area.trimery.reopened-highway");
            Need(road.Contains(player.transform.position) && player.Motor.Speed >= player.Motor.BaseSpeed * 1.149f,
                "Reopened pavement grants the standard road movement bonus");
            await Walk(TrimeryGate);
            Need(walkedMeters - before < 36, "Player physically walks the shortcut and reaches Trimery");

            Phase("completed repair survives reload");
            await Resume();
            Need(Route.Reopened && State.status == QuestStatus.Completed && Boss == null &&
                !Repair.Available(player) && !CrossingBlocked(), "Repair, quest and clear navigation survive loading");
            Need(Mathf.Abs(Distance(EastGate,TrimeryGate) - openMeters) < 1, "Saved open route has the same travel distance");
            Need(errors.Count == 0, "Unity errors: " + string.Join(" / ", errors));
            Record("passed");
            Debug.Log("DICEFREE_OGRE_ROADWARDEN_OK");
        }
        catch (Exception e)
        {
            Record("failed", e.ToString());
            Debug.LogError(e);
        }
        finally
        {
            if (view != null && originalGizmos != null) gizmos?.SetValue(view, originalGizmos);
            Application.logMessageReceived -= Log;
        }
    }
}

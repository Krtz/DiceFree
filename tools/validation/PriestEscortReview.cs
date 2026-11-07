using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Advancement;
using DiceFree.ApplicationFlow;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class PriestEscortReview
{
    private const string Report = "Logs/PriestEscortReview.json";
    private static int checks;
    private static string phase;

    public static string Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Clean Edit Mode required.");
        var guard = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t => t != null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,
            new object[] { Path.Combine(Path.GetTempPath(), "DiceFree-PriestEscort-" + Guid.NewGuid()) });
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
        EditorApplication.EnterPlaymode();
        return "Started isolated world escort review.";
    }

    public static string Run()
    {
        checks = 0;
        _ = Execute();
        return "Read " + Report + " for the result.";
    }

    private static async Task Execute()
    {
        var errors = new List<string>();
        void Observe(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Assert || type == LogType.Error) errors.Add(message);
        }
        Application.logMessageReceived += Observe;
        var gameView = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.GetType().Name == "GameView");
        var showGizmos = gameView?.GetType().GetProperty("showGizmos", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        object originalGizmos = showGizmos?.CanWrite == true ? showGizmos.GetValue(gameView) : null;
        try
        {
            if (originalGizmos != null) showGizmos.SetValue(gameView, false);
            Phase("normal menu, healer and class gate");
            await Until(() => Object.FindAnyObjectByType<StartMenuController>() != null && !LoadingScreen.Busy, "Start menu");
            var menu = Object.FindAnyObjectByType<StartMenuController>();
            Need(menu.TryStartNewEcho("Escort Review"), "Create Echo through normal menu: " + menu.Status);
            var player = await WorldPlayer();
            var mentor = Object.FindAnyObjectByType<PriestMentor>();
            Need(mentor != null && !mentor.Available(player), "Novice cannot take the trial");
            Need(player.Motor.TeleportAcrossMaps(mentor.ApproachPosition), "Reach Trimery healer");
            var ordinary = mentor.GetComponent<DiceFree.World.HealerInteraction>();
            Need(mentor.WorldTarget.Resolve(player) == ordinary, "Ordinary heal remains the default for Novice");
            player.Health.ApplyDamage(null, new DamageResult { mitigated = player.Health.Maximum * .40f });
            ordinary.Interact(player);
            Need(player.Health.Current == player.Health.Maximum, "Ordinary treatment restores health");

            var advancement = player.GetComponent<AdvancementController>();
            var xp = player.GetComponent<ExperienceProgression>();
            xp.RestoreState(10, 0);
            var tierOne = advancement.Definitions.Single(edge => edge.targetClass.stableId == PriestMentor.ParentClassId);
            Need(advancement.TryAdvance(tierOne), "Become Magically Touched: " + advancement.Feedback);
            xp.RestoreState(29, 0);
            Need(!mentor.Available(player), "Level 29 cannot take the trial");
            xp.RestoreState(30, 0);
            Need(player.Motor.TeleportAcrossMaps(mentor.ApproachPosition) &&
                mentor.WorldTarget.Resolve(player) == mentor && mentor.CanInteract(player), "Mentor available at level 30");
            var edge = advancement.Definitions.Single(value => value.requiredQuestId == PriestMentor.QuestId);
            Need(!advancement.CanAdvance(edge), "Class panel cannot skip the escort");
            mentor.Interact(player);
            Need(mentor.AcceptTrial(player) && Status(player) == QuestStatus.Active, "Accept and save the escort");
            Need(!mentor.CompleteTrial(player), "Mentor rejects early turn-in");

            var skills = player.GetComponent<MagicalSkillProgression>();
            var mend = skills.Definitions.Single(definition => definition.kind == MagicalSkillKind.Mend);
            for (int i = 0; i < mend.maxRank; i++) Need(skills.Spend(mend), "Learn Mend rank " + (i + 1));
            var passive = skills.Definitions.FirstOrDefault(definition => !definition.Active);
            if (passive != null)
                while (skills.Spend(passive)) { }
            var resources = player.GetComponent<ActorResourceController>();
            resources.RefillAll();

            Phase("inspector death and player death retries");
            var run = await Depart(mentor, player);
            Need(run.Site.guards.Length == 2 && run.Site.inspector.GetComponent<BasicAttack>() == null, "Two guards and a defenseless inspector");
            Need(!mentor.Depart(player) && player.GetComponent<RunLoadoutLock>().Locked, "Private run and locked class");
            var savedAnchor = player.GetComponent<DiceFree.World.RespawnAtAnchor>().AnchorId;
            Kill(run.Site.inspector);
            await Returned(player, mentor);
            Need(Status(player) == QuestStatus.Active, "Inspector loss leaves active quest for a retry");
            run = await Depart(mentor, player);
            Kill(player);
            await Returned(player, mentor);
            Need(player.Alive && player.Health.Current >= .49f * player.Health.Maximum &&
                player.Health.Current < .60f * player.Health.Maximum, "Player death returns to Trimery with standard health");
            Need(player.GetComponent<DiceFree.World.RespawnAtAnchor>().AnchorId == savedAnchor && Status(player) == QuestStatus.Active,
                "Death preserves the active stone and retry");
            ordinary.Treat(player);
            Need(player.Health.Current == player.Health.Maximum, "Mentor dialogue can still offer treatment");

            Time.timeScale = 5;
            Phase("guard loss and free restart");
            run = await Depart(mentor, player);
            await FollowUntil(run, player, () => run.Phase == PriestEscortPhase.Fighting, 30);
            Vector3 stoppedAt = run.Site.inspector.transform.position;
            foreach (var guard in run.Site.guards) Kill(guard);
            await Task.Delay(230);
            Need(PriestEscortRun.Current == run && run.Phase == PriestEscortPhase.Fighting &&
                Vector3.Distance(stoppedAt, run.Site.inspector.transform.position) < .7f, "Both guards can fall and the inspector stays put");
            var hostile = run.Site.encounters[0].hostiles.First(value => value.Alive);
            hostile.Health.ApplyDamage(player, new DamageResult { mitigated = 1 });
            Need(hostile.GetComponent<BasicAttack>().Target == player, "A hostile retaliates when the player attacks");
            run.Abandon();
            await Returned(player, mentor);
            Need(Status(player) == QuestStatus.Active, "Abandonment leaves the trial available without a fee");

            Phase("full route with Mend as the only player ability");
            run = await Depart(mentor, player);
            var caster = player.GetComponent<MagicalSkillCaster>();
            var crew = run.Site.guards.Append(run.Site.inspector).ToArray();
            int casts = 0, maximumWave = 0, survivingGuards = 2;
            bool captured = false;
            float start = Time.time;
            float nextMove = 0;
            float walkingSeconds = 0, combatSeconds = 0, prior = start;
            var attacks = player.GetComponent<BasicAttack>();
            int originalHits = attacks.Hits;
            while (run != null && PriestEscortRun.Current == run && Time.time - start < 400)
            {
                float delta = Time.time - prior;
                prior = Time.time;
                if (run.Phase == PriestEscortPhase.Walking) walkingSeconds += delta;
                if (run.Phase == PriestEscortPhase.Fighting) combatSeconds += delta;
                if (run.Phase != PriestEscortPhase.Returning) survivingGuards = run.Site.guards.Count(guard => guard.Alive);
                if (run.Phase == PriestEscortPhase.Walking) Follow(run, player, ref nextMove);
                if (run.Phase == PriestEscortPhase.Fighting)
                {
                    maximumWave = Math.Max(maximumWave, run.EncounterNumber);
                    var target = crew.Where(actor => actor != null && actor.Alive && actor.Health.Maximum - actor.Health.Current >= 58)
                        .OrderBy(actor => actor.Health.Current / actor.Health.Maximum).FirstOrDefault();
                    if (target != null && caster.CooldownRemaining(mend) <= 0 && caster.Cast(mend, target))
                    {
                        casts++;
                        if (!captured)
                        {
                            Directory.CreateDirectory("VisualBaselines/PriestEscort");
                            ScreenCapture.CaptureScreenshot("VisualBaselines/PriestEscort/road-ambush.png");
                            captured = true;
                        }
                    }
                }
                await Task.Delay(60);
            }
            float escortSeconds = Time.time - start;
            Time.timeScale = 1;
            Need(PriestEscortRun.Current == null && Status(player) == QuestStatus.ReadyToTurnIn,
                $"Escort arrived with Mend only. Wave {maximumWave}, casts {casts}, time {escortSeconds:0}s, status {Status(player)}, position {player.transform.position}.");
            Need(maximumWave == 4 && casts >= 3 && attacks.Hits == originalHits, "Four ambushes and no player attacks");
            Need(escortSeconds >= 240 && escortSeconds <= 345,
                $"Escort should be about five minutes. Time {escortSeconds:0}s, marching {walkingSeconds:0}s, combat {combatSeconds:0}s.");
            Need(survivingGuards > 0, "Mend keeps at least one guard alive through the route");
            Need(Vector3.Distance(player.transform.position, mentor.CornbergArrival) < 4 &&
                advancement.CurrentClassId == PriestMentor.ParentClassId && !advancement.CanAdvance(edge), "Arrival in Cornberg without auto-promotion");
            var save = new LocalEchoStore(Path.GetDirectoryName(player.GetComponent<ManifestationPersistence>().SavePath)).Load();
            Need(ManifestationRoster.ReadManifestation(save, PriestMentor.ParentClassId).quests
                .Single(value => value.questId == PriestMentor.QuestId).status == QuestStatus.ReadyToTurnIn, "Escort completion is on disk");

            Phase("reload and turn in");
            Need(LoadingScreen.TryLoadScene("StartMenu", "Reloading the escort review", null), "Return to menu");
            await Until(() => Object.FindAnyObjectByType<StartMenuController>() != null && !LoadingScreen.Busy, "Reloaded menu");
            menu = Object.FindAnyObjectByType<StartMenuController>();
            Need(menu.TryPlayManifestation(PriestMentor.ParentClassId), "Load saved parent through menu");
            player = await WorldPlayer();
            mentor = Object.FindAnyObjectByType<PriestMentor>();
            Need(Status(player) == QuestStatus.ReadyToTurnIn, "Ready-to-turn-in stage survives reload");
            Need(player.Motor.TeleportAcrossMaps(mentor.ApproachPosition) && !mentor.Depart(player), "Completed stage cannot start another run");
            mentor.Interact(player);
            Need(mentor.CompleteTrial(player), "Promote via Trimery mentor: " + mentor.Feedback);
            advancement = player.GetComponent<AdvancementController>();
            Need(advancement.CurrentClassId == PriestSkillProgression.PriestClassId && player.Stats.Level == 1 &&
                advancement.HasManifestation(PriestMentor.ParentClassId), "New level-one Priest preserves the parent");
            var bar = player.GetComponent<AbilityBarSource>();
            Need(bar.AbilityCount == 2 && bar.UnspentPoints == 1 && bar.Upgrade(0), "The Priest preview kit is playable after promotion");
            Need(bar.Activate(0) && player.GetComponent<SkillTargetingController>().ConfirmSelfFromPortrait(), "Holy Light works in the world");
            Need(advancement.TrySwitch(PriestMentor.ParentClassId) && Status(player) == QuestStatus.Completed &&
                player.GetComponent<MagicalSkillProgression>().Rank(mend.stableId) == mend.maxRank,
                "Parent quest completion and Mend rank persist");
            Need(!mentor.Available(player), "Completed promotion cannot be farmed");
            Need(errors.Count == 0, string.Join(" | ", errors));
            Write(new { status = "passed", checks, waves = maximumWave, mendCasts = casts,
                escortSeconds = Mathf.Round(escortSeconds), marchingSeconds = Mathf.Round(walkingSeconds),
                combatSeconds = Mathf.Round(combatSeconds), survivingGuards, playerAttackHits = 0, unityErrors = errors });
        }
        catch (Exception error) { Write(new { status = "failed", phase, checks, error = error.ToString(), unityErrors = errors }); }
        finally
        {
            Time.timeScale = 1;
            Application.logMessageReceived -= Observe;
            if (originalGizmos != null && gameView != null) showGizmos.SetValue(gameView, originalGizmos);
        }
    }

    private static async Task<PriestEscortRun> Depart(PriestMentor mentor, CombatActor player)
    {
        mentor.Interact(player);
        Need(mentor.Depart(player), "Depart from Trimery: " + mentor.Feedback);
        await Until(() => PriestEscortRun.Current?.Phase == PriestEscortPhase.Walking && !LoadingScreen.Busy, "Private road ready");
        return PriestEscortRun.Current;
    }

    private static async Task Returned(CombatActor player, PriestMentor mentor)
    {
        await Until(() => PriestEscortRun.Current == null && !SceneManager.GetSceneByName(PriestEscortSite.SceneName).isLoaded, "Escort unloaded");
        Need(Vector3.Distance(player.transform.position, mentor.ApproachPosition) < 3 &&
            player.GetComponent<RunLoadoutLock>().Owner == null, "Returned to Trimery and released run lock");
    }

    private static async Task FollowUntil(PriestEscortRun run, CombatActor player, Func<bool> condition, int seconds)
    {
        float nextMove = 0;
        await Until(() =>
        {
            if (run != null && run.Phase == PriestEscortPhase.Walking) Follow(run, player, ref nextMove);
            return condition();
        }, "Reach the first ambush on foot", seconds);
    }

    private static void Follow(PriestEscortRun run, CombatActor player, ref float nextMove)
    {
        if (Time.time < nextMove || run.Site.inspector == null) return;
        nextMove = Time.time + .6f;
        var inspector = run.Site.inspector.transform;
        if (Vector3.Distance(player.transform.position, inspector.position) > 4)
            player.Motor.MoveTo(inspector.position - inspector.forward * 2.6f);
    }

    private static async Task<CombatActor> WorldPlayer()
    {
        TraversalInput input = null;
        await Until(() => (input = Object.FindAnyObjectByType<TraversalInput>()) != null && !LoadingScreen.Busy &&
            input.GetComponent<ManifestationPersistence>().Ready, "World manifestation ready");
        await Task.Delay(150);
        return input.GetComponent<CombatActor>();
    }

    private static QuestStatus? Status(CombatActor player) => player.GetComponent<QuestJournal>().GetProgress(PriestMentor.QuestId)?.status;
    private static void Kill(CombatActor actor) => actor.Health.ApplyDamage(null, new DamageResult { mitigated = 100000 });
    private static void Need(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException(message); }
    private static void Phase(string value) { phase = value; Write(new { status = "running", phase, checks }); }
    private static void Write(object value) => File.WriteAllText(Report, Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
    private static async Task Until(Func<bool> predicate, string description, int seconds = 25)
    {
        var deadline = DateTime.UtcNow.AddSeconds(seconds);
        while (!predicate())
        {
            if (DateTime.UtcNow >= deadline) throw new TimeoutException(description);
            await Task.Delay(35);
        }
    }
}

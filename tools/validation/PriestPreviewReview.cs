using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Advancement;
using DiceFree.ApplicationFlow;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Development;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.World;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class PriestPreviewReview
{
    private const string Report = "Logs/PriestPreviewReview.json";
    private const string ClassId = PriestSkillProgression.PriestClassId;
    private const string ParentId = "class.magically-touched-novice";
    private static int checks;
    private static string phase;

    public static string Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Clean Edit Mode required.");
        var guard = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t => t != null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null, new object[] { Path.Combine(Path.GetTempPath(), "DiceFree-Priest-" + Guid.NewGuid()) });
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
        EditorApplication.EnterPlaymode();
        return "Started isolated Priest preview review.";
    }

    public static string Run()
    {
        checks = 0;
        _ = Execute();
        return "Read Logs/PriestPreviewReview.json for the result.";
    }

    private static async Task Execute()
    {
        var errors = new List<string>();
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message);
        }
        Application.logMessageReceived += Log;
        Mouse mouse = null;
        try
        {
            var gameView = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.GetType().Name == "GameView");
            if (gameView != null)
                gameView.GetType().GetProperty("showGizmos", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(gameView, false);
            Phase("Development Room entry");
            StartMenuController menu = null;
            await Until(() => (menu = Object.FindAnyObjectByType<StartMenuController>()) != null && !LoadingScreen.Busy, "Start menu");
            Need(menu.Catalog.Resolve(ClassId) != null, "Priest resolves for saved world manifestations");
            Need(menu.TryOpenDevelopmentRoom(), "Open normal Development Room entry");
            await Until(() => DevelopmentRoomController.Current?.Status == "Test Echo ready" && !LoadingScreen.Busy, "Development Room ready");
            await Task.Delay(150);
            var room = DevelopmentRoomController.Current;
            var player = room.player;
            var advancement = player.GetComponent<AdvancementController>();
            var options = string.Join(", ", advancement.Definitions.Select(edge => edge.targetClass.stableId));
            Need(room.SelectClass(ClassId), $"Select {ClassId} through development class controls ({options}). " +
                $"Busy={room.Busy}, class={advancement.CurrentClassId}, level={player.Stats.Level}, feedback={advancement.Feedback}");
            var persistence = player.GetComponent<ManifestationPersistence>();
            var skills = player.GetComponent<PriestSkillProgression>();
            var caster = player.GetComponent<PriestSkillCaster>();
            var bar = player.GetComponent<AbilityBarSource>();
            var targeting = player.GetComponent<SkillTargetingController>();
            var resources = player.GetComponent<ActorResourceController>();
            var targets = CombatActor.All.Where(a => a.Stats.Definition.stableId == "dev.practice-target").OrderBy(a => a.transform.position.x).ToArray();
            var allies = CombatActor.All.Where(a => a.Stats.Definition.stableId == "dev.priest-ally").OrderBy(a => a.transform.position.x).ToArray();
            Need(allies.Length == 2 && targets.Length == 3 && player.IsFriendlyTo(allies[0]) && player.IsHostileTo(targets[0]), "Training factions");
            Need(persistence.HasManifestation(ParentId) && player.Stats.Level == 1, "Fresh level-one Priest and retained parent");
            Need(bar.AbilityCount == 6 && bar.UnspentPoints == 1 && bar.View(0).rank == 0 && bar.View(1).rank == 0, "Rank-zero starter budget");
            Need(!bar.CanUpgrade(1) && !bar.Activate(0), "Grace level gate and unlearned spell");
            Need(bar.Upgrade(0) && bar.UnspentPoints == 0, "Learn Holy Light from action bar");
            Need(!skills.CanRestore(new[] { new SkillRankState("skill.priest.holy-light", 19) }, 80), "Holy Light rank cap");
            Need(!skills.CanRestore(new[] { new SkillRankState("skill.priest.grace", 2) }, 9), "Grace rank gates");

            Phase("self and pointer targeting");
            player.Health.Restore(.30f);
            float hp = player.Health.Current;
            float mana = resources.Current(ResourceIds.Mana);
            Need(bar.Activate(0) && targeting.Mode == SkillTargetingMode.AnyUnit && targeting.AllowsSelfTarget, "Dual allegiance targeting from action bar");
            Need(targeting.ConfirmSelfFromPortrait() && player.Health.Current > hp, "Self-heal through portrait");
            Near(mana - resources.Current(ResourceIds.Mana), PriestSkillCaster.ManaCost(1), .01f, "Self-heal Mana cost");
            Need(caster.Grace == 0 && caster.CooldownRemaining > 12 && !bar.Activate(0), "Rank-zero Grace and ordinary cooldown");
            Need(room.SetLevel(5) && bar.Upgrade(0) && bar.Upgrade(1) && caster.GraceMaximum == 10, "Level-five Grace and Holy Light rank two");
            player.Health.Restore();
            Need(player.Motor.TeleportAcrossMaps(allies[1].transform.position + Vector3.back * 3), "Practice positioning");
            room.explorationCamera.RecenterForTravel(player.transform);
            await Task.Delay(180);
            mouse = InputSystem.AddDevice<Mouse>("PriestPreviewMouse");
            float enemyHp = targets[2].Health.Current;
            Ready(caster, resources);
            await PointerCast(bar, targeting, mouse, targets[2]);
            Need(targets[2].Health.Current < enemyHp && caster.Grace == 1, "Enemy click deals Light damage and grants one Grace");
            float allyHp = allies[1].Health.Current;
            Ready(caster, resources);
            await PointerCast(bar, targeting, mouse, allies[1]);
            Need(allies[1].Health.Current > allyHp && caster.Grace == 3, "Ally click heals and earns two Grace");

            Phase("Grace and bounce priority");
            CastWithHealth(caster, resources, allies[1], 1f);
            Need(caster.Grace == 4, "Full-health heal earns one Grace");
            CastWithHealth(caster, resources, allies[1], .97f);
            Need(caster.Grace == 5, "Partial overheal earns one Grace");
            for (int attempt = 0; attempt < 10 && !caster.GraceFull; attempt++)
                CastWithHealth(caster, resources, allies[1], .2f);
            Need(bar.Activate(1) && caster.GraceArmed, "Manual full-Grace activation");
            Ready(caster, resources);
            Need(bar.Activate(0), "Prepare empowered spell");
            targeting.Cancel();
            mana = resources.Current(ResourceIds.Mana);
            Need(!caster.CastHolyLight(null) && caster.GraceArmed && caster.Grace == 10 && caster.CooldownRemaining == 0, "Cancel and invalid cast preserve armed Grace");
            Near(resources.Current(ResourceIds.Mana), mana, .01f, "Invalid spell spends no Mana");
            allies[0].Health.Restore(.15f);
            allies[1].Health.Restore(.45f);
            float firstBefore = allies[0].Health.Current;
            float secondBefore = allies[1].Health.Current;
            float amount = caster.BaseAmount(2) * player.Stats.HealingDone * allies[0].Stats.HealingReceived;
            Need(caster.CastHolyLight(targets[2]), "Empowered hostile cast");
            Near(allies[0].Health.Current - firstBefore, amount * .9f, .02f, "Most wounded ally selected first");
            Near(allies[1].Health.Current - secondBefore, amount * .81f, .02f, "Second hop has multiplicative falloff");
            Need(!caster.GraceArmed && caster.Grace == 1, "Whole empowered chain generates only once");
            Near(mana - resources.Current(ResourceIds.Mana), PriestSkillCaster.ManaCost(2), .01f, "Empowered cast still spends Mana");
            Directory.CreateDirectory("VisualBaselines/PriestPreview");
            ScreenCapture.CaptureScreenshot("VisualBaselines/PriestPreview/holy-light-chain.png");
            await Task.Delay(180);

            Phase("staff and saved manifestation");
            var inventory = player.GetComponent<CarriedInventory>();
            var equipment = player.GetComponent<Equipment>();
            var staff = inventory.Grant("item.wolf-survival.moonwell-staff");
            var wand = inventory.Grant("item.wolf-survival.starfall-wand");
            Need(equipment.Equip(staff.instanceId, EquipmentSlot.MainHand) && equipment.UsesTwoHandedStance, "Moonwell Staff and two-handed stance");
            Need(!equipment.Equip(wand.instanceId, EquipmentSlot.MainHand), "Mage wand rejected");
            Need(player.Stats.Definition.basicAttack.element == null, "Staff strike has no inherent element");
            Need(player.Motor.TeleportAcrossMaps(targets[2].transform.position + Vector3.back * 1.9f), "Staff target position");
            var basic = player.GetComponent<BasicAttack>();
            int attempts = basic.Hits + basic.Misses;
            Need(basic.Order(targets[2]), "Staff strike order");
            await Until(() => basic.Hits + basic.Misses > attempts, "Staff attack resolves");
            basic.Cancel();
            Need(caster.Grace == 1, "Ordinary staff attack earns no Grace");
            for (int attempt = 0; attempt < 10 && !caster.GraceFull; attempt++)
                CastWithHealth(caster, resources, allies[1], .2f);
            Need(caster.ArmGrace(), "Arm Grace before save");
            Need(persistence.Flush(), "Flush Priest preview save");
            var disk = new LocalEchoStore(Path.GetDirectoryName(persistence.SavePath)).Load();
            var saved = ManifestationRoster.ReadManifestation(disk, ClassId);
            Need(saved.classSkills.Length == 6 && saved.resources.Any(value => value.resourceId == ResourceIds.Mana) &&
                saved.resources.All(value => value.resourceId != PriestSkillProgression.GraceResourceId), "Save ranks and Mana without transient Grace");
            Need(room.SelectClass(ParentId) && room.SelectClass(ClassId), "Restore saved Priest after switching to parent");
            Need(skills.Rank(PriestSkill.HolyLight) == 2 && skills.Rank(PriestSkill.Grace) == 1 && caster.Grace == 0 && !caster.GraceArmed, "Ranks persist and Grace resets");
            Need(equipment.UsesTwoHandedStance, "Saved staff restored");
            await ReviewCompleteKit(room, player, skills, caster, bar, targeting, resources, mouse, allies, targets);
            Need(errors.Count == 0, "Unity errors: " + string.Join(" | ", errors));
            Write(new { status = "passed", checks, abilities = 6, targetClicks = 8, parentPreserved = true, manaAndRanksSaved = true, graceReset = true, unityErrors = errors });
        }
        catch (Exception error)
        {
            Write(new { status = "failed", phase, checks, error = error.ToString(), unityErrors = errors });
        }
        finally
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            Application.logMessageReceived -= Log;
        }
    }

    private static async Task ReviewCompleteKit(DevelopmentRoomController room, CombatActor player, PriestSkillProgression skills,
        PriestSkillCaster caster, AbilityBarSource bar, SkillTargetingController targeting, ActorResourceController resources,
        Mouse mouse, CombatActor[] allies, CombatActor[] enemies)
    {
        Phase("Dawnwarden presentation and rank gates");
        var visual = player.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;
        var model = ClassPresentationSwitcher.FindVisualAnimator(visual);
        Need(visual.name == "Dawnwarden Priest" && visual.gameObject.activeInHierarchy &&
            model.avatar != null && model.avatar.isValid && model.runtimeAnimatorController.animationClips.Length == 10,
            "Promoted Priest uses Dawnwarden with ten playable animations");
        Need(visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 2, "Skinned robes and held Dawn Lantern");
        var moonwell = visual.GetComponentsInChildren<Transform>(true).Single(t => t.name == "Wolf reward - Moonwell Staff");
        var lantern = visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == "Dawn Lantern");
        Need(moonwell.gameObject.activeInHierarchy && !lantern.gameObject.activeInHierarchy,
            "Equipping Moonwell Staff replaces Dawn Lantern on Dawnwarden");
        var equipment = player.GetComponent<Equipment>();
        Need(equipment.Unequip(EquipmentSlot.MainHand.ToString()) && lantern.gameObject.activeInHierarchy,
            "Unequipping Moonwell Staff restores Dawn Lantern");
        await Task.Delay(140);
        var portrait = player.GetComponent<HudPortraitRenderer>().Texture;
        Need(portrait != null && portrait.IsCreated(), "Dawnwarden portrait renders to the normal HUD texture");
        var previous = RenderTexture.active;
        var snapshot = new Texture2D(portrait.width, portrait.height);
        try
        {
            RenderTexture.active = portrait;
            snapshot.ReadPixels(new Rect(0, 0, portrait.width, portrait.height), 0, 0);
            snapshot.Apply();
            File.WriteAllBytes("Logs/PriestCompletePortrait.png", snapshot.EncodeToPNG());
            Need(snapshot.GetPixels32().Count(c => c.r > 55 || c.g > 55 || c.b > 55) > 500,
                "Dawnwarden is visible inside the HUD portrait frame");
        }
        finally { RenderTexture.active = previous; Object.Destroy(snapshot); }
        Need(skills.CanRestore(new[] { new SkillRankState("skill.priest.holy-light", 2), new SkillRankState("skill.priest.grace", 1) }, 5),
            "Older two-skill Priest saves still load");
        Need(room.SetLevel(19) && !bar.CanUpgrade((int)PriestSkill.Revive), "Revive requires level 20");
        Need(room.SetLevel(20) && bar.Upgrade((int)PriestSkill.LightBurst) && bar.Upgrade((int)PriestSkill.IceAegis) &&
            bar.Upgrade((int)PriestSkill.WildGrowth) && bar.Upgrade((int)PriestSkill.Revive) &&
            !bar.CanUpgrade((int)PriestSkill.Revive), "All four spells learn through the action bar with the one-rank Revive cap");

        var trainingCorpse = CombatActor.All.Single(a => a.Stats.Definition.stableId == "dev.priest-fallen");
        Need(!trainingCorpse.Alive && caster.IsValidCorpse(trainingCorpse),
            "Revive practice is available without developer commands");
        var ally = allies[1];
        var enemy = enemies[2];
        Vector3 center = enemy.transform.position;
        Need(ally.Motor.TeleportAcrossMaps(center + Vector3.back * 2.4f) &&
            player.Motor.TeleportAcrossMaps(center + Vector3.back * 6), "Place the party at a practice target");
        room.explorationCamera.RecenterForTravel(player.transform);
        await Task.Delay(200);

        Phase("Light Burst follows the original center");
        Ready(caster, resources);
        Arm(caster, bar, resources);
        ally.Health.Restore(.18f);
        enemy.Health.Restore();
        int heals = 0, hits = 0;
        void Healed(float _) => heals++;
        void Damaged(CombatActor source, DamageResult _) { if (source == player) hits++; }
        ally.Health.Healed += Healed;
        enemy.Health.Damaged += Damaged;
        try
        {
            await PointerCast(bar, targeting, mouse, ally, PriestSkill.LightBurst);
            Need(heals == 1 && hits == 1 && !caster.GraceArmed && caster.Grace == 2, "Initial area pulse heals and damages, granting Grace once");
            Vector3 destination = center + Vector3.back * 9f;
            Need(ally.Motor.TeleportAcrossMaps(destination), "Move the original Light Burst center");
            await Until(() => heals == 3, "Both delayed pulses follow the moving ally");
            Need(hits == 1 && caster.Grace == 2, "Moving pulses leave the original area and grant no extra Grace");
        }
        finally { ally.Health.Healed -= Healed; enemy.Health.Damaged -= Damaged; }

        Phase("Ice Aegis absorption and shield-bound damage");
        Need(ally.Motor.TeleportAcrossMaps(center + Vector3.back * 2.4f), "Place the shield beside an enemy");
        Ready(caster, resources);
        Arm(caster, bar, resources);
        float beforeHp = enemy.Health.Current;
        await PointerCast(bar, targeting, mouse, ally, PriestSkill.IceAegis);
        Near(ally.Health.TemporaryShield, caster.ShieldAmount(1), .05f, "Aegis scales from Spirit");
        Need(caster.Grace == 1, "Aegis earns one Grace");
        var ice = AssetDatabase.LoadAssetAtPath<ElementDefinition>("Assets/_DiceFree/Settings/Combat/Elements/Ice.asset");
        float shield = ally.Health.TemporaryShield;
        ally.Health.ApplyDamage(enemy, new DamageResult { mitigated = 10, element = ice });
        Near(shield - ally.Health.TemporaryShield, 8, .05f, "Ten Ice damage spends eight shield HP");
        shield = ally.Health.TemporaryShield;
        ally.Health.ApplyDamage(enemy, new DamageResult { mitigated = 10 });
        Near(shield - ally.Health.TemporaryShield, 12, .05f, "Ten non-Ice damage spends twelve shield HP");
        await Until(() => enemy.Health.Current < beforeHp, "Empowered shield pulses against nearby enemies");
        ally.Health.ApplyDamage(enemy, new DamageResult { mitigated = ally.Health.TemporaryShield / 1.2f + 1f });
        Need(ally.Health.TemporaryShield == 0 && ally.Alive, "Break the shield while the ally survives");
        beforeHp = enemy.Health.Current;
        await Task.Delay(420);
        Near(enemy.Health.Current, beforeHp, .15f, "Broken shield stops its aura");

        Phase("Wild Growth healing, slowing and refresh");
        Ready(caster, resources);
        float speed = enemy.Stats.MoveSpeed;
        Arm(caster, bar, resources);
        await PointerCast(bar, targeting, mouse, enemy, PriestSkill.WildGrowth);
        Need(enemy.Stats.MoveSpeed < speed && caster.Grace == 1, "Empowered Nature damage slows its enemy");
        Ready(caster, resources);
        Need(caster.Cast(PriestSkill.WildGrowth, enemy), "Recast Wild Growth from the same caster");
        Near(enemy.Stats.MoveSpeed, speed, .01f, "Normal refresh replaces the previous empowered slow");
        beforeHp = enemy.Health.Current;
        await Until(() => enemy.Health.Current < beforeHp, "Refreshed Wild Growth continues ticking");
        Ready(caster, resources);
        Arm(caster, bar, resources);
        ally.Health.Restore(.08f);
        int growthTicks = 0;
        void GrowthHeal(float _) => growthTicks++;
        ally.Health.Healed += GrowthHeal;
        float oldScale = Time.timeScale;
        try
        {
            await PointerCast(bar, targeting, mouse, ally, PriestSkill.WildGrowth);
            Time.timeScale = 4;
            await Until(() => growthTicks >= 34, "Empowered ally HoT continues after the normal 32 ticks");
            Need(caster.Grace == 2, "Healing ticks do not refill Grace after the first");
        }
        finally { Time.timeScale = oldScale; ally.Health.Healed -= GrowthHeal; }

        Phase("Revive corpse targeting, cancellation, success and sickness");
        Ready(caster, resources);
        var party = ally.GetComponent<LocalPartyMember>();
        var sickness = ally.GetComponent<ResurrectionSickness>();
        var mana = ally.gameObject.AddComponent<ActorResourceController>();
        var profile = Object.Instantiate(resources.Profiles.Single(p => p.classId == ClassId && p.resource.stableId == ResourceIds.Mana));
        profile.classId = ally.Stats.Definition.stableId;
        profile.baseRegenPerSecond = profile.regenPerIntelligence = profile.regenPerSpirit = 0;
        mana.Configure(profile);
        mana.RestoreForClass(profile.classId, Array.Empty<RuntimeResourceValue>());
        var position = ally.transform.position;
        float spirit = ally.Stats.Attributes.spirit;
        ally.Health.ApplyDamage(null, new DamageResult { mitigated = 100000 });
        Need(!ally.Alive && caster.IsValidCorpse(ally), "Dead party member is targetable with physical colliders disabled");
        string partyId = party.partyId;
        party.partyId = "other";
        Need(!caster.IsValidCorpse(ally) && !caster.Cast(PriestSkill.Revive, ally), "Reject a same-faction corpse from outside the party");
        party.partyId = partyId;
        resources.SetCurrentForValidation(ResourceIds.Mana, resources.Maximum(ResourceIds.Mana) * .5f);
        Need(!bar.Activate((int)PriestSkill.Revive), "Exactly half Mana is below the Revive requirement");
        Ready(caster, resources);
        await PointerCast(bar, targeting, mouse, ally, PriestSkill.Revive);
        Need(caster.IsChanneling && Mathf.Approximately(caster.ChannelDuration, 8) &&
            Object.FindAnyObjectByType<ChannelBarHud>()?.TryActive(out _) == true, "Eight-second normal Revive reaches the shared HUD");
        await Until(() => player.GetComponent<PlayerMotionPresentation>().State == "Revive", "Dawnwarden plays the Revive channel pose");
        await Until(() => Object.FindAnyObjectByType<ChannelBarHud>()?.TryActive(out _) == true,
            "Channel HUD remains active while capturing gameplay");
        ScreenCapture.CaptureScreenshot("Logs/PriestCompleteGameplay.png");
        await Task.Delay(100);
        player.Health.ApplyDamage(enemy, new DamageResult { mitigated = 1 });
        Need(!caster.IsChanneling && !ally.Alive && caster.Remaining(PriestSkill.Revive) == 0 &&
            resources.Current(ResourceIds.Mana) == resources.Maximum(ResourceIds.Mana), "Damage interrupts without spending Mana or cooldown");

        Arm(caster, bar, resources);
        await PointerCast(bar, targeting, mouse, ally, PriestSkill.Revive);
        Need(caster.IsChanneling && Mathf.Approximately(caster.ChannelDuration, 4), "Grace shortens the channel to four seconds");
        var keyboard = InputSystem.AddDevice<Keyboard>("PriestMovementTest");
        try
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            await Until(() => !caster.IsChanneling, "Movement input interrupts Revive");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            await Task.Delay(80);
            player.Motor.Stop();
            Need(!ally.Alive && caster.GraceArmed && caster.Remaining(PriestSkill.Revive) == 0, "Movement preserves armed Grace");
        }
        finally { InputSystem.RemoveDevice(keyboard); }

        Need(player.GetComponent<BasicAttack>().Order(enemy), "Enter combat before starting Revive");
        Need(player.InCombat && caster.Cast(PriestSkill.Revive, ally), "Revive can start while in combat");

        try
        {
            await Until(() => ally.Alive, "Revive channel completes in place");
            Near(Vector3.Distance(position, ally.transform.position), 0, .05f, "Revive preserves corpse location");
            Near(ally.Health.Current / ally.Health.Maximum, .15f, .015f, "Revive restores 15% HP");
            Near(mana.Current(ResourceIds.Mana) / mana.Maximum(ResourceIds.Mana), .15f, .001f, "Revive restores 15% Mana");
            Need(resources.Current(ResourceIds.Mana) < 1.5f && caster.Remaining(PriestSkill.Revive) > 58 &&
                !caster.GraceArmed && caster.Grace == 1, "Only successful Revive spends all Mana and Grace and starts cooldown");
            Need(sickness.Stacks == 1, "Revive adds one sickness stack");
            Near(ally.Stats.Attributes.spirit, spirit * .9f, .01f, "Sickness reduces Spirit by 10%");
        }
        finally { Object.Destroy(profile); }

        Ready(caster, resources);
        ally.Health.ApplyDamage(null, new DamageResult { mitigated = 100000 });
        Need(caster.Cast(PriestSkill.Revive, ally), "Start a second successful Revive");
        try
        {
            Time.timeScale = 4;
            await Until(() => ally.Alive, "Normal channel completes");
        }
        finally { Time.timeScale = oldScale; }
        Need(sickness.Stacks == 2, "Two real resurrections stack their sickness");
        Near(ally.Stats.Attributes.spirit, spirit * .8f, .01f, "Two stacks reduce Spirit by 20%");
        string saved = sickness.CaptureJson();
        sickness.ResetToDefault();
        sickness.RestoreJson(sickness.Version, saved);
        Need(sickness.Stacks == 2, "Both independent expiry timestamps round-trip");
        long now = DateTime.UtcNow.Ticks;
        sickness.RestoreJson(sickness.Version, "{\"expirations\":[" + (now - 1) + "," + (now + TimeSpan.FromMinutes(4).Ticks) + "]}");
        Need(sickness.Stacks == 1, "An expired stack is dropped while the later stack remains");
    }

    private static void Arm(PriestSkillCaster caster, AbilityBarSource bar, ActorResourceController resources)
    {
        resources.Restore(PriestSkillProgression.GraceResourceId, caster.GraceMaximum);
        Need(bar.Activate((int)PriestSkill.Grace) && caster.GraceArmed, "Arm full Grace through the action bar");
    }

    private static async Task PointerCast(AbilityBarSource bar, SkillTargetingController targeting, Mouse mouse, CombatActor target, PriestSkill skill = PriestSkill.HolyLight)
    {
        Need(bar.Activate((int)skill), "Activate " + skill);
        Vector2 point = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * (skill == PriestSkill.Revive ? .38f : 1f));
        Need(!HudPointerBlocker.Covers(point), "Practice target visible outside HUD");
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point, buttons = 1 });
        await Until(() => !targeting.Active, "Resolve real pointer cast on " + target.DisplayName);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
        await Task.Delay(60);
    }

    private static void Ready(PriestSkillCaster caster, ActorResourceController resources)
    {
        var cooldowns = (float[])typeof(PriestSkillCaster).GetField("readyAt", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(caster);
        Array.Clear(cooldowns, 0, cooldowns.Length);
        resources.Restore(ResourceIds.Mana, resources.Maximum(ResourceIds.Mana));
    }

    private static void CastWithHealth(PriestSkillCaster caster, ActorResourceController resources, CombatActor target, float health)
    {
        Ready(caster, resources);
        target.Health.Restore(health);
        Need(caster.CastHolyLight(target), "Apply eligible Holy Light");
    }

    private static void Phase(string value) { phase = value; Write(new { status = "running", phase, checks }); }
    private static void Write(object value) => File.WriteAllText(Report, Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
    private static void Need(bool value, string message) { checks++; if (!value) throw new InvalidOperationException(message); }
    private static void Near(float actual, float expected, float tolerance, string message) => Need(Mathf.Abs(actual - expected) <= tolerance, $"{message}. Expected {expected}, got {actual}.");
    private static async Task Until(Func<bool> condition, string message)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException(message);
            await Task.Delay(20);
        }
    }
}

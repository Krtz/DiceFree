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
            Need(bar.AbilityCount == 2 && bar.UnspentPoints == 1 && bar.View(0).rank == 0 && bar.View(1).rank == 0, "Rank-zero starter budget");
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
            Need(saved.classSkills.Length == 2 && saved.resources.Any(value => value.resourceId == ResourceIds.Mana) &&
                saved.resources.All(value => value.resourceId != PriestSkillProgression.GraceResourceId), "Save ranks and Mana without transient Grace");
            Need(room.SelectClass(ParentId) && room.SelectClass(ClassId), "Restore saved Priest after switching to parent");
            Need(skills.Rank(PriestSkill.HolyLight) == 2 && skills.Rank(PriestSkill.Grace) == 1 && caster.Grace == 0 && !caster.GraceArmed, "Ranks persist and Grace resets");
            Need(equipment.UsesTwoHandedStance, "Saved staff restored");
            Need(errors.Count == 0, "Unity errors: " + string.Join(" | ", errors));
            Write(new { status = "passed", checks, abilities = 2, targetClicks = 2, parentPreserved = true, manaAndRanksSaved = true, graceReset = true, unityErrors = errors });
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

    private static async Task PointerCast(AbilityBarSource bar, SkillTargetingController targeting, Mouse mouse, CombatActor target)
    {
        Need(bar.Activate(0), "Activate Holy Light");
        Vector2 point = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up);
        Need(!HudPointerBlocker.Covers(point), "Practice target visible outside HUD");
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point, buttons = 1 });
        await Until(() => !targeting.Active, "Resolve real pointer cast on " + target.DisplayName);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
        await Task.Delay(60);
    }

    private static void Ready(PriestSkillCaster caster, ActorResourceController resources)
    {
        typeof(PriestSkillCaster).GetField("readyAt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(caster, 0f);
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

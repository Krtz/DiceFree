using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class MagicalTouchedPlaymodeValidation
    {
        private const string Prefix = "DiceFree.MagicalPlaymode.";
        private const string SaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";
        private const string PriorFastPlayFlag = Prefix + "prior-fast-play";
        private const string PriorPlayOptionsKey = Prefix + "prior-play-options";
        private const float PersistedManaCheckpoint = 44f;

        private static float deadline;
        private static bool ticking;
        private static IEnumerator routine;

        static MagicalTouchedPlaymodeValidation() =>
            EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.magical.playmode-test", "Run real Magically Touched advancement, spell, aura and StartMenu reload validation; poll dicefree.magical.playmode-status.", Tags = new[] { "tests", "magical" })]
        private static object Start()
        {
            MagicalTouchedValidation.Require(
                !EditorApplication.isPlayingOrWillChangePlaymode,
                "Magical Play Mode validation requires idle Edit Mode.");

            SessionState.SetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled = false;

            string root = Path.Combine(
                Path.GetTempPath(),
                "DiceFree-Magical-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Prefix + "root", root);
            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 1);

            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.magical.playmode-status", "Read or continue the current Magically Touched Play Mode validation.", Tags = new[] { "tests", "magical" })]
        private static object Status()
        {
            string status = SessionState.GetString(Prefix + "status", "idle");
            if (status == "running" &&
                SessionState.GetInt(Prefix + "phase", 0) == 2 &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
                BeginMenuPhase();

            return new
            {
                status,
                success = status == "passed",
                finalMarker = status == "passed" ? "DICEFREE_MAGICAL_PLAYMODE_OK" : "",
                error = SessionState.GetString(Prefix + "error", ""),
                temporarySaveRoot = SessionState.GetString(Prefix + "root", ""),
                phase = SessionState.GetInt(Prefix + "phase", 0),
                stage = SessionState.GetString(Prefix + "stage", "")
            };
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running")
            {
                if (state == PlayModeStateChange.EnteredEditMode) Cleanup();
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Validation must not inherit a paused/scaled Editor session from a prior Play Mode suite.
                // Gameplay delays intentionally use scaled time; make this harness start from normal gameplay time.
                EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                deadline = Time.realtimeSinceStartup + 60f;
                routine = null;
                if (!ticking)
                {
                    ticking = true;
                    EditorApplication.update += Tick;
                    Application.logMessageReceived += OnLog;
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                StopTick();
                if (SessionState.GetInt(Prefix + "phase", 0) == 2)
                    EditorApplication.delayCall += BeginMenuPhase;
                else
                    Cleanup();
            }
        }

        private static void BeginMenuPhase()
        {
            if (SessionState.GetString(Prefix + "status", "") != "running" ||
                SessionState.GetInt(Prefix + "phase", 0) != 2 ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(
                SessionState.GetString(Prefix + "root", ""));
            EditorSceneManager.OpenScene(AdvancementAuthoring.StartMenuScenePath);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            try
            {
                MagicalTouchedValidation.Require(
                    Time.realtimeSinceStartup < deadline,
                    "Magical Play Mode validation timed out: " + DescribeState());

                if (!EditorApplication.isPlaying) return;

                int phase = SessionState.GetInt(Prefix + "phase", 0);
                if (phase == 2)
                {
                    var menu = UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if (menu == null || !menu.HasLoadedProfile || menu.LoadBlocked) return;
                    RunMenuPhase(menu);
                    return;
                }

                var progression = UnityEngine.Object.FindAnyObjectByType<MagicalSkillProgression>();
                if (progression == null) return;
                var persistence = progression.GetComponent<ManifestationPersistence>();
                if (persistence == null || !persistence.Ready) return;

                if (phase == 1)
                {
                    routine ??= PhaseOne(progression, persistence);
                    if (!routine.MoveNext()) routine = null;
                }
                else if (phase == 3)
                {
                    StopTick();
                    RunReloadPhase(progression, persistence);
                }
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static IEnumerator PhaseOne(
            MagicalSkillProgression magicalSkills,
            ManifestationPersistence persistence)
        {
            var actor = magicalSkills.GetComponent<CombatActor>();
            var xp = magicalSkills.GetComponent<ExperienceProgression>();
            var controller = magicalSkills.GetComponent<AdvancementController>();
            var noviceSkills = magicalSkills.GetComponent<NoviceSkillProgression>();
            var physicalSkills = magicalSkills.GetComponent<PhysicalSkillProgression>();
            var resources = magicalSkills.GetComponent<ActorResourceController>();
            var caster = magicalSkills.GetComponent<MagicalSkillCaster>();
            var switcher = magicalSkills.GetComponent<ClassPresentationSwitcher>();
            var basic = magicalSkills.GetComponent<BasicAttack>();

            MagicalTouchedValidation.Require(
                actor != null && xp != null && controller != null &&
                noviceSkills != null && physicalSkills != null &&
                resources != null && caster != null && switcher != null && basic != null,
                "Magical gameplay fixture is incomplete.");

            MagicalTouchedValidation.Require(
                controller.CurrentClassId == NoviceSkillProgression.ClassStableId,
                "Fresh Magical fixture did not begin as Novice.");

            xp.RestoreState(10, 0);
            var physicalEdge = controller.Definitions.Single(
                value => value.targetClass.stableId == PhysicalBlessedAuthoring.ClassId);
            var magicalEdge = controller.Definitions.Single(
                value => value.targetClass.stableId == MagicalTouchedAuthoring.ClassId);

            MagicalTouchedValidation.Require(
                controller.TryAdvance(physicalEdge),
                "Could not create the Physical sibling before Magical validation: " +
                controller.Feedback);
            MagicalTouchedValidation.Require(
                controller.TrySwitch(NoviceSkillProgression.ClassStableId),
                "Could not return to preserved Novice before creating Magical: " +
                controller.Feedback);

            string physicalBefore = SectionJson(
                persistence.CaptureProfile(),
                PhysicalBlessedAuthoring.ClassId);

            SessionState.SetString(Prefix + "stage", "advance-magical");
            MagicalTouchedValidation.Require(
                controller.TryAdvance(magicalEdge),
                "Could not advance the preserved Novice into Magical: " +
                controller.Feedback);

            yield return null;
            switcher.SendMessage("Update");

            MagicalTouchedValidation.Require(
                controller.CurrentClassId == MagicalTouchedAuthoring.ClassId &&
                xp.Level == 1 && xp.CurrentXp == 0 &&
                magicalSkills.ActiveForCurrentClass &&
                !noviceSkills.ActiveForCurrentClass &&
                !physicalSkills.ActiveForCurrentClass,
                "Magical advancement did not activate the correct level-1 class systems.");

            MagicalTouchedValidation.Require(
                resources.Has(ResourceIds.Mana),
                "Magical child did not activate Mana.");
            Near(resources.Maximum(ResourceIds.Mana), 119, "level-1 Magical Mana maximum");
            Near(resources.Regeneration(ResourceIds.Mana), 8.9f, "level-1 Magical Mana regeneration");

            MagicalTouchedValidation.Require(
                switcher.CurrentClassId == MagicalTouchedAuthoring.ClassId &&
                switcher.ActiveVisualRoot != null &&
                switcher.ActiveVisualRoot.name == "MagicallyTouchedPresentation",
                "Magical advancement did not activate the dedicated presentation.");

            MagicalTouchedValidation.Require(
                basic.Definition != null &&
                basic.Definition.stableId == "attack.magical.basic" &&
                basic.Definition.channel == DamageChannel.Magical &&
                basic.Definition.reach >= 8,
                "Magical ranged basic attack did not become active.");

            xp.RestoreState(5, 0);
            resources.RefillAll();

            var sand = Definition(magicalSkills, MagicalTouchedAuthoring.SandId);
            var mend = Definition(magicalSkills, MagicalTouchedAuthoring.MendId);
            var fire = Definition(magicalSkills, MagicalTouchedAuthoring.FireId);
            var ice = Definition(magicalSkills, MagicalTouchedAuthoring.IceId);
            var attune = Definition(magicalSkills, MagicalTouchedAuthoring.AttunementId);

            magicalSkills.RestoreState(new[]
            {
                new SkillRankState(sand.stableId, 1),
                new SkillRankState(mend.stableId, 1),
                new SkillRankState(fire.stableId, 1),
                new SkillRankState(ice.stableId, 1)
            });

            MagicalTouchedValidation.Require(
                magicalSkills.SpentPoints == 4 && magicalSkills.UnspentPoints == 1,
                "Level-5 Magical did not expose its fifth skill point.");

            float maxBeforeAttune = resources.Maximum(ResourceIds.Mana);
            float regenBeforeAttune = resources.Regeneration(ResourceIds.Mana);
            MagicalTouchedValidation.Require(
                magicalSkills.Spend(attune),
                "Could not spend the fifth point into Mana Attunement.");
            Near(resources.Maximum(ResourceIds.Mana), maxBeforeAttune + 20,
                "Mana Attunement R1 maximum");
            Near(resources.Regeneration(ResourceIds.Mana), regenBeforeAttune + .75f,
                "Mana Attunement R1 personal regeneration");
            MagicalTouchedValidation.Require(
                magicalSkills.SpentPoints == 5 && magicalSkills.UnspentPoints == 0,
                "Magical five-point allocation accounting changed.");

            var enemies = CombatActor.All
                .Where(value =>
                    value != null &&
                    actor.IsHostileTo(value) &&
                    value.Effects != null &&
                    value.GetComponent<BasicAttack>() != null)
                .OrderByDescending(value => value.Health.Maximum)
                .Take(3)
                .ToArray();

            MagicalTouchedValidation.Require(
                enemies.Length >= 2,
                "Magical validation needs at least two real Cornberg hostiles.");

            foreach (var enemy in enemies)
            {
                var ai = enemy.GetComponent<AggroBehaviour>();
                if (ai != null) ai.enabled = false;
                enemy.GetComponent<BasicAttack>()?.Cancel();
                enemy.Health.Restore();
                enemy.Effects.ClearTransient();
            }

            CombatActor target = enemies[0];
            Vector3 targetPoint = actor.transform.position + actor.transform.forward * 2.5f;
            MagicalTouchedValidation.Require(
                target.Motor.Teleport(targetPoint),
                "Could not place Magic Sand validation target.");

            var sandPackets = new List<DamageResult>();
            Action<CombatActor, DamageResult> sandPacketHandler =
                (_, packet) => sandPackets.Add(packet);
            target.Health.PacketResolved += sandPacketHandler;

            resources.RefillAll();
            float manaBefore = resources.Current(ResourceIds.Mana);
            float targetHealthBefore = target.Health.Current;
            MagicalTouchedValidation.Require(
                caster.Cast(sand, target),
                "Magic Sand failed.");
            target.Health.PacketResolved -= sandPacketHandler;

            Near(manaBefore - resources.Current(ResourceIds.Mana), 10, "Magic Sand R1 Mana cost");
            MagicalTouchedValidation.Require(
                target.Health.Current < targetHealthBefore &&
                sandPackets.Any(packet =>
                    packet.element != null &&
                    packet.element.stableId == "element.nature"),
                "Magic Sand did not deal Nature damage.");
            Near(target.Effects.AccuracyMissChance, .075f,
                "Magic Sand R1 miss chance");
            target.Effects.ClearTransient();
            caster.ResetForManifestationLoad();

            actor.Health.ApplyDamage(
                null,
                new DamageResult { mitigated = Mathf.Min(25, actor.Health.Maximum * .25f) });
            float selfBeforeMend = actor.Health.Current;
            bool selfHealed = false;
            Action<float> selfHealHandler = _ => selfHealed = true;
            actor.Health.Healed += selfHealHandler;
            resources.RefillAll();
            MagicalTouchedValidation.Require(
                caster.Cast(mend, actor),
                "Self Mend failed.");
            actor.Health.Healed -= selfHealHandler;
            MagicalTouchedValidation.Require(
                selfHealed && actor.Health.Current > selfBeforeMend,
                "Mend did not use ordinary healing on self.");
            caster.ResetForManifestationLoad();

            target.Health.Restore();
            target.Effects.ClearTransient();
            MagicalTouchedValidation.Require(
                target.Motor.Teleport(actor.transform.position + actor.transform.forward * 3f),
                "Could not place Fire Imbuement target.");

            SessionState.SetString(Prefix + "stage", "fire-basic");
            var firePackets = new List<DamageResult>();
            Action<CombatActor, DamageResult> firePacketHandler =
                (_, packet) => firePackets.Add(packet);
            target.Health.PacketResolved += firePacketHandler;

            resources.RefillAll();
            MagicalTouchedValidation.Require(
                caster.Cast(fire, actor),
                "Fire Elemental Imbuement failed.");

            basic.Cancel();
            int hitsBefore = basic.Hits;
            MagicalTouchedValidation.Require(
                basic.Order(target),
                "Could not order the real Magical basic attack after Fire Imbuement.");

            DriveBasicAttackImpact(basic);

            basic.Cancel();
            target.Health.PacketResolved -= firePacketHandler;

            MagicalTouchedValidation.Require(
                basic.Hits > hitsBefore,
                "Fire Imbuement validation basic attack never landed.");
            MagicalTouchedValidation.Require(
                firePackets.Count >= 2 &&
                firePackets.Any(packet => packet.element == null) &&
                firePackets.Any(packet =>
                    packet.element != null &&
                    packet.element.stableId == "element.fire"),
                "Fire Imbuement did not add a separate Fire packet to the landed basic attack.");

            actor.Effects.ClearTransient();
            target.Health.Restore();
            caster.ResetForManifestationLoad();

            SessionState.SetString(Prefix + "stage", "ice-burst");
            CombatActor second = enemies[1];
            Vector3 burstCenter =
                actor.transform.position +
                actor.transform.forward * 3.5f;
            MagicalTouchedValidation.Require(
                target.Motor.Teleport(burstCenter),
                "Could not place first Ice Burst target.");
            MagicalTouchedValidation.Require(
                second.Motor.Teleport(
                    burstCenter +
                    actor.transform.right * 1.3f),
                "Could not place second Ice Burst target.");

            target.Health.Restore();
            second.Health.Restore();
            target.Effects.ClearTransient();
            second.Effects.ClearTransient();

            float firstIceHealth = target.Health.Current;
            float secondIceHealth = second.Health.Current;
            float firstMoveSpeed = target.Stats.MoveSpeed;

            resources.RefillAll();
            manaBefore = resources.Current(ResourceIds.Mana);
            MagicalTouchedValidation.Require(
                caster.CastIceBurst(ice, burstCenter),
                "Ice Burst failed.");
            Near(manaBefore - resources.Current(ResourceIds.Mana), 18,
                "Ice Burst R1 Mana cost");
            MagicalTouchedValidation.Require(
                Mathf.Approximately(target.Health.Current, firstIceHealth) &&
                Mathf.Approximately(second.Health.Current, secondIceHealth),
                "Ice Burst dealt damage before its authored delay.");

            ResolveIceBurstDeterministically(caster, ice, 1, burstCenter);

            MagicalTouchedValidation.Require(
                target.Health.Current < firstIceHealth &&
                second.Health.Current < secondIceHealth,
                "Delayed Ice Burst did not damage multiple real Cornberg enemies.");
            Near(
                target.Stats.MoveSpeed,
                firstMoveSpeed * .98f,
                "Ice Burst R1 movement slow");

            target.Effects.ClearTransient();
            second.Effects.ClearTransient();
            target.Health.Restore();
            second.Health.Restore();
            caster.ResetForManifestationLoad();

            SessionState.SetString(Prefix + "stage", "aura");
            // Reuse real actors to prove self/ally healing and strongest-wins Mana aura semantics
            // without introducing synthetic test-only combat prefabs.
            target.Configure(actor.Faction, target.Radius, target.FamilyId);
            second.Configure(actor.Faction, second.Radius, second.FamilyId);
            MagicalTouchedValidation.Require(
                target.Motor.Teleport(actor.transform.position + actor.transform.right * 2f) &&
                second.Motor.Teleport(actor.transform.position - actor.transform.right * 2f),
                "Could not place friendly aura/heal validation actors.");

            target.Health.ApplyDamage(
                null,
                new DamageResult { mitigated = Mathf.Min(20, target.Health.Maximum * .25f) });
            float allyBeforeMend = target.Health.Current;
            bool allyHealed = false;
            Action<float> allyHealHandler = _ => allyHealed = true;
            target.Health.Healed += allyHealHandler;

            resources.RefillAll();
            MagicalTouchedValidation.Require(
                caster.Cast(mend, target),
                "Ally Mend failed.");
            target.Health.Healed -= allyHealHandler;
            MagicalTouchedValidation.Require(
                allyHealed && target.Health.Current > allyBeforeMend,
                "Mend did not heal a friendly ally.");
            caster.ResetForManifestationLoad();

            var recipientResources =
                target.GetComponent<ActorResourceController>() ??
                target.gameObject.AddComponent<ActorResourceController>();
            var magicalProfile =
                AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(
                    MagicalTouchedAuthoring.MagicalManaPath);
            recipientResources.Configure(magicalProfile);
            recipientResources.RestoreForClass(
                MagicalTouchedAuthoring.ClassId,
                Array.Empty<RuntimeResourceValue>());

            float ownAura =
                ResourceRegenerationAura.StrongestFor(
                    target,
                    ResourceIds.Mana);
            Near(ownAura, .25f, "Mana Attunement R1 ally aura");

            var strongerAura =
                second.GetComponent<ResourceRegenerationAura>() ??
                second.gameObject.AddComponent<ResourceRegenerationAura>();
            strongerAura.Configure(ResourceIds.Mana, .6f, 8);

            float strongest =
                ResourceRegenerationAura.StrongestFor(
                    target,
                    ResourceIds.Mana);
            Near(strongest, .6f, "strongest-wins Mana aura");
            MagicalTouchedValidation.Require(
                Mathf.Abs(strongest - .85f) > .1f,
                "Mana auras incorrectly stacked additively.");

            MagicalTouchedValidation.Require(
                second.Motor.Teleport(actor.transform.position + actor.transform.forward * 20f),
                "Could not move stronger Mana aura out of range.");
            Near(
                ResourceRegenerationAura.StrongestFor(
                    target,
                    ResourceIds.Mana),
                .25f,
                "Mana aura fallback after stronger source leaves range");

            // Save the real Magical manifestation and prove the sibling timelines remain untouched.
            string physicalAfterMagic =
                SectionJson(
                    persistence.CaptureProfile(),
                    PhysicalBlessedAuthoring.ClassId);
            MagicalTouchedValidation.Require(
                physicalAfterMagic == physicalBefore,
                "Magical gameplay mutated the preserved Physical sibling manifestation.");

            SessionState.SetString(Prefix + "stage", "save");
            resources.SetCurrentForValidation(
                ResourceIds.Mana,
                PersistedManaCheckpoint);
            MagicalTouchedValidation.Require(
                persistence.Flush(),
                "Could not save the fully configured Magical manifestation.");

            var saved = ReadMagicalState(persistence.CaptureProfile());
            RequireRanks(saved.classSkills);
            MagicalTouchedValidation.Require(
                saved.level == 5,
                "Saved Magical level changed.");
            MagicalTouchedValidation.Require(
                saved.resources != null &&
                saved.resources.Single(value =>
                    value.resourceId == ResourceIds.Mana).value ==
                PersistedManaCheckpoint,
                "Saved Magical Mana did not capture the exact checkpoint.");

            Debug.Log(
                "DICEFREE_MAGICAL_PHASE1_OK: three-manifestation roster, all four active spells, Mana Attunement and strongest-wins aura passed.");
            SessionState.SetInt(Prefix + "phase", 2);
            EditorApplication.ExitPlaymode();
        }

        private static void RunMenuPhase(StartMenuController menu)
        {
            var entries = menu.Entries;
            MagicalTouchedValidation.Require(
                entries.Length == 3,
                "StartMenu did not show Novice + Physical + Magical.");
            MagicalTouchedValidation.Require(
                entries.Single(value =>
                    value.classId == NoviceSkillProgression.ClassStableId).level == 10,
                "StartMenu displayed the wrong preserved Novice level.");
            MagicalTouchedValidation.Require(
                entries.Single(value =>
                    value.classId == PhysicalBlessedAuthoring.ClassId).level == 1,
                "StartMenu displayed the wrong preserved Physical level.");

            var magical = entries.SingleOrDefault(value =>
                value.classId == MagicalTouchedAuthoring.ClassId);
            MagicalTouchedValidation.Require(
                magical != null &&
                magical.available &&
                magical.active &&
                magical.level == 5,
                "StartMenu did not show the saved level-5 Magical manifestation as active.");

            string root = SessionState.GetString(Prefix + "root", "");
            var menuDisk = new LocalEchoStore(root).Load();
            double menuMana =
                ReadMagicalState(menuDisk).resources
                    .Single(value => value.resourceId == ResourceIds.Mana)
                    .value;
            MagicalTouchedValidation.Require(
                menuMana >= PersistedManaCheckpoint,
                "Quit-time Magical Mana regressed below the explicit checkpoint.");
            SessionState.SetFloat(
                Prefix + "menu-mana",
                (float)menuMana);

            SessionState.SetInt(Prefix + "phase", 3);
            deadline = Time.realtimeSinceStartup + 30f;
            MagicalTouchedValidation.Require(
                menu.TryPlayManifestation(
                    MagicalTouchedAuthoring.ClassId),
                "StartMenu could not load the Magical manifestation: " +
                menu.Status);
        }

        private static void RunReloadPhase(
            MagicalSkillProgression skills,
            ManifestationPersistence persistence)
        {
            var xp = skills.GetComponent<ExperienceProgression>();
            var resources = skills.GetComponent<ActorResourceController>();
            var caster = skills.GetComponent<MagicalSkillCaster>();
            var switcher = skills.GetComponent<ClassPresentationSwitcher>();
            var actor = skills.GetComponent<CombatActor>();

            MagicalTouchedValidation.Require(
                persistence.CurrentClassId ==
                MagicalTouchedAuthoring.ClassId &&
                xp.Level == 5 &&
                skills.ActiveForCurrentClass,
                "StartMenu reload did not restore the saved Magical class/level.");

            RequireRanks(skills.CaptureState());
            MagicalTouchedValidation.Require(
                skills.SpentPoints == 5 &&
                skills.UnspentPoints == 0,
                "Reloaded Magical skill accounting changed.");

            string root = SessionState.GetString(Prefix + "root", "");
            var disk = new LocalEchoStore(root).Load();
            var saved = ReadMagicalState(disk);
            float menuMana =
                SessionState.GetFloat(
                    Prefix + "menu-mana",
                    -1);
            double diskMana =
                saved.resources.Single(value =>
                    value.resourceId == ResourceIds.Mana).value;

            MagicalTouchedValidation.Require(
                Math.Abs(diskMana - menuMana) < .001,
                "StartMenu selection changed persisted Magical Mana.");
            MagicalTouchedValidation.Require(
                resources.Has(ResourceIds.Mana) &&
                resources.Current(ResourceIds.Mana) >= menuMana &&
                resources.Current(ResourceIds.Mana) <=
                resources.Maximum(ResourceIds.Mana),
                "Runtime Magical Mana did not restore from the persisted value.");

            switcher.SendMessage("Update");
            MagicalTouchedValidation.Require(
                switcher.CurrentClassId ==
                MagicalTouchedAuthoring.ClassId &&
                switcher.ActiveVisualRoot != null &&
                switcher.ActiveVisualRoot.name ==
                "MagicallyTouchedPresentation",
                "Magical presentation did not survive StartMenu reload.");

            var attune = Definition(
                skills,
                MagicalTouchedAuthoring.AttunementId);
            Near(
                resources.Maximum(ResourceIds.Mana),
                BaseMaximumAtLevel(
                    persistence,
                    5) +
                attune.MaximumManaBonus(1),
                "reloaded Mana Attunement maximum");

            foreach (var definition in
                     skills.Definitions.Where(value => value.Active))
                Near(
                    caster.CooldownRemaining(definition),
                    0,
                    "transient Magical cooldown reset");

            Near(
                actor.Effects.AccuracyMissChance,
                0,
                "transient accuracy effects reset");

            Debug.Log(
                "DICEFREE_MAGICAL_PLAYMODE_OK: full magical Tier-1 kit, three-manifestation coexistence, aura semantics and StartMenu save/reload passed.");
            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 4);
            EditorApplication.ExitPlaymode();
        }

        private static void DriveBasicAttackImpact(BasicAttack attack)
        {
            attack.SendMessage("Update");
            var impact = typeof(BasicAttack).GetField(
                "impactAt",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            MagicalTouchedValidation.Require(impact != null,
                "BasicAttack impact timing field could not be resolved by the validation harness.");
            impact.SetValue(attack, Time.time - 1f);
            attack.SendMessage("Update");
        }

        private static void ResolveIceBurstDeterministically(
            MagicalSkillCaster caster,
            MagicalSkillDefinition definition,
            int rank,
            Vector3 point)
        {
            // The cast above proves damage is delayed. Stop the runtime coroutine, then invoke the same
            // gameplay resolution method directly so Remote/Pipeline validation does not depend on an
            // Editor PlayerLoop frame pump.
            caster.StopAllCoroutines();
            var resolver = typeof(MagicalSkillCaster).GetMethod(
                "ResolveIceBurst",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            MagicalTouchedValidation.Require(resolver != null,
                "MagicalSkillCaster Ice Burst resolver could not be resolved by the validation harness.");
            resolver.Invoke(caster, new object[] { definition, rank, point });
        }

        private static float BaseMaximumAtLevel(
            ManifestationPersistence persistence,
            int level)
        {
            var definition =
                AssetDatabase.LoadAssetAtPath<ActorDefinition>(
                    MagicalTouchedAuthoring.MagicalClassPath);
            var profile =
                AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(
                    MagicalTouchedAuthoring.MagicalManaPath);
            return profile.Maximum(
                AttributeValues.AtLevel(
                    definition.baseAttributes,
                    definition.growth,
                    level));
        }

        private static MagicalSkillDefinition Definition(
            MagicalSkillProgression skills,
            string id) =>
            skills.Definitions.Single(
                value => value.stableId == id);

        private static ManifestationSave ReadMagicalState(
            EchoSave profile) =>
            JsonUtility.FromJson<ManifestationSave>(
                profile.sections.Single(value =>
                    value.id ==
                    ManifestationRoster.SectionIdFor(
                        MagicalTouchedAuthoring.ClassId))
                    .json);

        private static string SectionJson(
            EchoSave profile,
            string classId) =>
            profile.sections.Single(value =>
                value.id ==
                ManifestationRoster.SectionIdFor(classId))
                .json;

        private static void RequireRanks(
            SkillRankState[] state)
        {
            MagicalTouchedValidation.Require(
                state != null &&
                state.Length == 5,
                "Expected five saved Magical skill ranks.");

            foreach (string id in new[]
                     {
                         MagicalTouchedAuthoring.SandId,
                         MagicalTouchedAuthoring.MendId,
                         MagicalTouchedAuthoring.FireId,
                         MagicalTouchedAuthoring.IceId,
                         MagicalTouchedAuthoring.AttunementId
                     })
                MagicalTouchedValidation.Require(
                    state.Single(value =>
                        value.stableId == id).rank == 1,
                    "Saved/reloaded Magical rank changed for " +
                    id);
        }

        private static void Near(
            float actual,
            float expected,
            string label)
        {
            MagicalTouchedValidation.Require(
                Mathf.Abs(actual - expected) <= .03f,
                label + ": expected " +
                expected +
                ", got " +
                actual);
        }

        private static void OnLog(
            string message,
            string stack,
            LogType type)
        {
            if (SessionState.GetString(Prefix + "status", "") !=
                "running")
                return;
            if (type != LogType.Error &&
                type != LogType.Exception &&
                type != LogType.Assert)
                return;

            if (stack.Contains(
                    "UnityEditor.Search.SearchInit.IndexationOnStartup") &&
                stack.Contains(
                    "UnityEditor.Search.SearchDatabase"))
                return;

            Fail(
                new InvalidOperationException(
                    message + "\n" + stack));
        }

        private static void Fail(Exception error)
        {
            if (SessionState.GetString(Prefix + "status", "") !=
                "running")
                return;

            SessionState.SetString(Prefix + "status", "failed");
            SessionState.SetString(Prefix + "error", error.ToString());
            SessionState.SetInt(Prefix + "phase", 4);
            Debug.LogException(error);
            StopTick();

            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
                Cleanup();
        }

        private static void StopTick()
        {
            if (!ticking) return;
            ticking = false;
            routine = null;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
        }

        private static string DescribeState()
        {
            int phase =
                SessionState.GetInt(Prefix + "phase", 0);
            var skills =
                UnityEngine.Object.FindAnyObjectByType<
                    MagicalSkillProgression>();
            if (skills == null)
                return "phase=" +
                       phase +
                       "; magical-skills=missing";

            var persistence =
                skills.GetComponent<ManifestationPersistence>();
            return "phase=" +
                   phase +
                   "; class=" +
                   persistence?.CurrentClassId +
                   "; persistence=" +
                   (persistence == null
                       ? "missing"
                       : persistence.Status) +
                   "; ready=" +
                   (persistence != null &&
                    persistence.Ready);
        }

        private static void Cleanup()
        {
            EditorSettings.enterPlayModeOptions =
                (EnterPlayModeOptions)SessionState.GetInt(
                    PriorPlayOptionsKey,
                    (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled =
                SessionState.GetBool(
                    PriorFastPlayFlag,
                    EditorSettings.enterPlayModeOptionsEnabled);
            Environment.SetEnvironmentVariable(
                SaveRootVariable,
                null);

            string root =
                SessionState.GetString(Prefix + "root", "");
            try
            {
                if (!string.IsNullOrEmpty(root) &&
                    Directory.Exists(root))
                    Directory.Delete(root, true);
            }
            catch
            {
                // Best-effort cleanup after the result is already recorded.
            }
        }
    }
}

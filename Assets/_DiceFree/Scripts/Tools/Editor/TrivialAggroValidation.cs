using System;
using System.Collections;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEditor;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class TrivialAggroValidation
    {
        private const string Running = "DiceFree.TrivialAggroValidation";
        private static CombatActor player, enemy;
        private static AggroBehaviour brain;
        private static BasicAttack attack;
        private static IEnumerator routine;
        private static float deadline;
        static TrivialAggroValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                CornbergValidation.ValidateNavigation();
                PersistenceTestGuard.DisableForNextPlay();
                SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
            brain = CombatValidationActors.IsolateCropDuel(); enemy = brain.GetComponent<CombatActor>(); attack = enemy.GetComponent<BasicAttack>();
            Time.timeScale = 5; deadline = Time.realtimeSinceStartup + 90;
            routine = Flow(); Application.logMessageReceived += OnLog; EditorApplication.update += Tick;
        }
        private static IEnumerator SetGap(int gap)
        {
            player.GetComponent<BasicAttack>().Cancel(); player.Motor.Stop();
            player.GetComponent<ExperienceProgression>().RestoreState(enemy.Stats.Level + gap, 0); player.Health.Restore();
            Require(brain.ResetEncounter(), "Aggro fixture reset.");
            Require(player.Motor.Teleport(brain.Home + Vector3.left * 3), "Aggro fixture placement.");
            yield return null; yield return null; yield return null;
        }
        private static IEnumerator Flow()
        {
            while (!player.Motor.Ready || !enemy.Motor.Ready) yield return null;
            int fixedLevel = enemy.Stats.Level;
            var ordinary = brain.AutoAggroPolicy;
            Require(ordinary != null && ordinary.trivialLevelGap == 10 && !ordinary.alwaysAutoAggro, "Default policy.");
            foreach (int gap in new[] { 0, 9, 10, 20 })
            {
                for (var wait = SetGap(gap); wait.MoveNext();) yield return null;
                Require((attack.Target == player) == (gap < 10), "Wrong awareness boundary at gap " + gap);
                Require(enemy.Stats.Level == fixedLevel, "Enemy scaled to player.");
                Debug.Log("TRIVIAL_AGGRO_BOUNDARY_OK " + gap);
            }
            // Lowering the player level permits acquisition without recreating the actor.
            player.Stats.SetLevel(fixedLevel + 9); yield return null; yield return null;
            Require(attack.Target == player, "Level decrease did not restore acquisition.");
            player.Stats.SetLevel(fixedLevel + 20); yield return null; yield return null;
            Require(attack.Target == player, "Level increase cancelled legitimate combat.");
            for (var wait = SetGap(10); wait.MoveNext();) yield return null;
            enemy.Health.ApplyDamage(player, new DamageResult { mitigated = .1f });
            Require(attack.Target == player, "Trivial enemy failed to retaliate.");
            yield return null; Require(attack.Target == player, "Retaliation target was suppressed.");
            // Existing leash/return logic remains authoritative after retaliation.
            player.Motor.Teleport(brain.Home + Vector3.left * (brain.Leash + 4));
            yield return null; yield return null;
            while (brain.Returning) yield return null;
            Require(attack.Target == null && Vector3.Distance(enemy.transform.position, brain.Home) < .7f &&
                enemy.Health.Current == enemy.Health.Maximum, "Leash/home reset changed.");
            var authored = new AutoAggroPolicy { alwaysAutoAggro = true, trivialLevelGap = 10 };
            brain.ConfigureAutoAggro(authored);
            for (var wait = SetGap(20); wait.MoveNext();) yield return null;
            Require(attack.Target == player, "Authored always-aggro exception ignored.");
            // Override only bypasses level suppression, not awareness, hostility or LOS.
            brain.ResetEncounter(); player.Motor.Teleport(brain.Home + Vector3.left * (brain.Awareness + 2));
            yield return null; yield return null; Require(attack.Target == null, "Override bypassed awareness.");
            player.Motor.Teleport(brain.Home + Vector3.left * 3); player.Configure(1, player.Radius);
            yield return null; yield return null; Require(attack.Target == null, "Override bypassed hostility.");
            player.Configure(0, player.Radius);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.layer = 9;
            wall.transform.position = brain.Home + Vector3.left * 1.5f + Vector3.up;
            wall.transform.localScale = new Vector3(.3f, 3, 3); Physics.SyncTransforms();
            yield return null; yield return null; Require(attack.Target == null, "Override bypassed LOS.");
            UnityEngine.Object.Destroy(wall); yield return null; yield return null;
            Require(attack.Target == player, "Removing LOS blocker did not restore acquisition.");
            brain.ConfigureAutoAggro(new AutoAggroPolicy { trivialLevelGap = 3 });
            for (var wait = SetGap(3); wait.MoveNext();) yield return null;
            Require(attack.Target == null, "Configured threshold ignored.");
            brain.ConfigureAutoAggro(ordinary);
            for (var wait = SetGap(10); wait.MoveNext();) yield return null;
            int xp = player.GetComponent<ExperienceProgression>().CurrentXp;
            player.GetComponent<BasicAttack>().Order(enemy);
            while (enemy.Alive) yield return null;
            Require(player.GetComponent<ExperienceProgression>().CurrentXp == xp + enemy.Stats.Definition.experienceReward,
                "Explicit attack/kill/XP on trivial enemy failed.");
            player.Motor.Teleport(brain.Home + Vector3.left * 3);
            while (!enemy.Alive) yield return null;
            yield return null; yield return null;
            Require(ReferenceEquals(brain.AutoAggroPolicy, ordinary) && attack.Target == null && enemy.Stats.Level == fixedLevel &&
                enemy.Health.Current == enemy.Health.Maximum, "Respawn lost policy or resumed suppressed aggro.");
            brain.ConfigureAutoAggro(authored); brain.ResetEncounter(); yield return null; yield return null;
            Require(ReferenceEquals(brain.AutoAggroPolicy, authored) && attack.Target == player, "Reset lost explicit override.");
            // Variant selection is shallow: explicit variant override, otherwise archetype policy.
            var archetype = ScriptableObject.CreateInstance<EnemyArchetype>(); archetype.autoAggroPolicy = ordinary;
            var variant = ScriptableObject.CreateInstance<EnemyVariantDefinition>(); variant.archetype = archetype;
            Require(ReferenceEquals(variant.AutoAggroPolicy, ordinary), "Archetype policy inheritance.");
            variant.overrideAutoAggro = true; variant.autoAggroPolicy = authored;
            Require(ReferenceEquals(variant.AutoAggroPolicy, authored), "Variant policy override.");
            UnityEngine.Object.Destroy(variant); UnityEngine.Object.Destroy(archetype);
            Debug.Log("TRIVIAL_AGGRO_PLAYMODE_OK: 0/9/10/20, live level changes, retaliation, explicit attack/XP, override, hostility/LOS/radius, leash, reset/respawn, fixed levels, tuning and archetype/variant selection.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Trivial aggro test timeout."); if (!routine.MoveNext()) Finish(0); }
            catch (Exception error) { Debug.LogException(error); Finish(1); }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (message.StartsWith("ArgumentOutOfRangeException") && trace.Contains("UnityEditor.Search.SearchDatabase") &&
                trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
            Finish(1);
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog; Time.timeScale = 1; EditorApplication.Exit(code);
        }
    }
}

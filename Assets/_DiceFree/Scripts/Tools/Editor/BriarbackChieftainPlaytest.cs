using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    /// <summary>Focused Chieftain encounter test using an isolated Echo save.</summary>
    [InitializeOnLoad]
    public static class BriarbackChieftainPlaytest
    {
        const string Key = "DiceFree.Briarback.Chieftain.";
        static IEnumerator flow;
        static double deadline;
        static int checks;
        static string error;

        static BriarbackChieftainPlaytest() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            checks++;
        }

        [CliCommand("dicefree.briarback.chieftain.test", "Exercise guard response and the Chieftain smash in Play Mode using an isolated save.")]
        public static object Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before starting this isolated encounter test.");
            SessionState.SetString(Key + "scene", EditorSceneManager.GetActiveScene().path);
            SessionState.SetBool(Key + "options", EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(Key + "flags", (int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Key + "cleanup", true);
            SessionState.SetString(Key + "status", "running");
            SessionState.SetString(Key + "error", "");
            SessionState.SetInt(Key + "checks", 0);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(), "DiceFree-Chieftain-" + Guid.NewGuid().ToString("N")));
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.briarback.chieftain.status", "Read the focused isolated Chieftain Play Mode test result.")]
        public static object Status() => new { status = SessionState.GetString(Key + "status", "idle"), checks = SessionState.GetInt(Key + "checks", 0), error = SessionState.GetString(Key + "error", "") };

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetString(Key + "status", "") == "running")
            {
                checks = 0;
                error = "";
                deadline = EditorApplication.timeSinceStartup + 90;
                flow = Run();
                EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key + "cleanup", false))
            {
                SessionState.SetBool(Key + "cleanup", false);
                EditorApplication.update -= Tick;
                PersistenceTestGuard.ClearValidationOverrides();
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(Key + "flags", 0);
                EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(Key + "options", false);
                var scene = SessionState.GetString(Key + "scene", "");
                if (!string.IsNullOrWhiteSpace(scene)) EditorSceneManager.OpenScene(scene);
            }
        }

        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Chieftain encounter test timed out.");
                if (flow != null && flow.MoveNext()) return;
                Finish(true, "");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }

        static void Finish(bool passed, string detail)
        {
            EditorApplication.update -= Tick;
            error = detail;
            SessionState.SetString(Key + "status", passed ? "passed" : "failed");
            SessionState.SetString(Key + "error", detail);
            SessionState.SetInt(Key + "checks", checks);
            Directory.CreateDirectory("Library");
            File.WriteAllText("Library/BriarbackChieftainPlaytestResults.json", JsonUtility.ToJson(new Result { status = passed ? "passed" : "failed", checks = checks, error = detail, isolatedSave = true }, true));
            EditorApplication.ExitPlaymode();
        }

        [Serializable]
        sealed class Result { public string status, error; public int checks; public bool isolatedSave; }

        static IEnumerator Run()
        {
            CombatActor player = null;
            while (player == null)
            {
                player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();
                yield return null;
            }
            var persistence = player.GetComponent<ManifestationPersistence>();
            while (!persistence.Ready || !player.Motor.Ready) yield return null;
            player.GetComponent<TraversalInput>().enabled = false;
            player.GetComponent<CombatInput>().enabled = false;
            player.GetComponent<BasicAttack>().enabled = false;
            player.Motor.Stop();
            player.Motor.SetMotionAllowed(false);
            player.Health.Invulnerable = true;

            var region = GameObject.Find("14 - Briarback quilboar frontier");
            Need(region != null, "Briarback region is missing during Play Mode.");
            var chief = region.GetComponentInChildren<BriarbackChieftainEncounter>(true);
            Need(chief != null && chief.gameObject.activeInHierarchy, "Level-40 Chieftain is not active.");
            Need(chief.eliteWarrior != null && chief.eliteBrute != null, "Both elite guards must be linked to the boss.");
            var bossActor = chief.GetComponent<CombatActor>();
            var bossAttack = chief.GetComponent<BasicAttack>();
            var guardAttacks = new[] { chief.eliteWarrior.GetComponent<BasicAttack>(), chief.eliteBrute.GetComponent<BasicAttack>() };

            Vector3 origin = bossActor.transform.position;
            Vector3[] offsets = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, (Vector3.forward + Vector3.right).normalized, (Vector3.forward + Vector3.left).normalized };
            Vector3 destination = default;
            bool found = false;
            foreach (var offset in offsets)
            {
                var desired = origin + offset * 4.5f;
                if (!NavMesh.SamplePosition(desired, out var hit, 1.5f, NavMesh.AllAreas)) continue;
                if (Vector3.Distance(hit.position, origin) < 3.2f || Vector3.Distance(hit.position, origin) > 6f) continue;
                if (Physics.CheckCapsule(hit.position + Vector3.up * .5f, hit.position + Vector3.up * 1.5f, .3f, 1 << 8, QueryTriggerInteraction.Ignore)) continue;
                destination = hit.position; found = true; break;
            }
            Need(found && player.Motor.Teleport(destination), "Could not move the isolated test player to a clear, walkable boss position.");
            Physics.SyncTransforms();
            yield return null;

            Need(bossAttack.Order(player), "Chieftain could not acquire the test player.");
            for (int i = 0; i < 20 && guardAttacks.Any(attack => attack.Target != player); i++) yield return null;
            Need(guardAttacks.All(attack => attack.Target == player), "Boss aggro did not immediately engage both living bodyguards.");
            for (int i = 0; i < 40 && !chief.Winding; i++) yield return null;
            var bossAggro = bossActor.GetComponent<AggroBehaviour>();
            Need(chief.Winding, $"Chieftain did not begin the overhead smash: target={(bossAttack.Target == player)}, distance={Vector3.Distance(bossActor.transform.position, player.transform.position):F2}, returning={bossAggro.Returning}, canAct={bossActor.CanAct}, state={bossAttack.State}.");
            Need(GameObject.Find("Chieftain smash - red fracture warning") != null, "Smash did not show its red ground warning.");
            float impactDeadline = Time.time + 2f;
            while (Time.time < impactDeadline && chief.SmashImpacts == 0) yield return null;
            Need(chief.SmashImpacts > 0, $"Smash did not resolve: attempts={chief.SmashAttempts}, hostile={chief.SmashHostileCandidates}, inLine={chief.SmashLineCandidates}, player={player.transform.position:F2}, boss={bossActor.transform.position:F2}, forward={bossActor.transform.forward:F2}.");
            Need(player.Effects != null && player.Effects.Stunned, "The smash did not apply the shared one-second stun.");
            Need(chief.SmashCooldownRemaining > 10, "Smash cooldown did not start after the impact.");
            player.Health.Invulnerable = false;
            player.Health.Restore();
            bossActor.Health.Restore(); // Resets living guards and cancels the encounter cleanly.
            Need(guardAttacks.All(attack => attack.Target == null), "Living bodyguards did not reset when the Chieftain returned.");

            player.Motor.SetMotionAllowed(true);
            bossActor.Health.Restore();
            found = false;
            foreach (var offset in offsets)
            {
                var desired = bossActor.transform.position + offset * 12f;
                if (!NavMesh.SamplePosition(desired, out var hit, 2f, NavMesh.AllAreas)) continue;
                if (Vector3.Distance(hit.position, bossActor.transform.position) < 9f || Vector3.Distance(hit.position, bossActor.transform.position) > chief.axeMaxRange) continue;
                if (!player.Motor.Teleport(hit.position)) continue;
                Physics.SyncTransforms();
                if (!bossActor.HasSightOf(player)) continue;
                destination = hit.position; found = true; break;
            }
            Need(found, "Could not place the isolated player on a clear, visible ranged position for Axe Throw.");
            player.Motor.SetMotionAllowed(false);
            bossActor.Health.ApplyPacket(player, new DamageResult { mitigated = 10f });
            Need(bossAttack.Order(player), "Chieftain could not acquire the ranged test player.");
            float axeDeadline = Time.time + 3f;
            while (Time.time < axeDeadline && GameObject.Find("Briarback Chieftain homing axe") == null) yield return null;
            Need(GameObject.Find("Briarback Chieftain homing axe") != null, "Chieftain did not launch its homing axe at a visible ranged threat.");
            Need(chief.AxeCooldownRemaining > 14f, "Axe Throw did not start its 15-second cooldown.");
        }
    }
}

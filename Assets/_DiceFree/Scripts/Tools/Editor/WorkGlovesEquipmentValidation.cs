using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using static DiceFree.EditorTools.WorkGlovesArtValidation;

namespace DiceFree.EditorTools
{
    /// <summary>Focused integration harness using real Q4, Equipment and schema-v4 persistence.</summary>
    [InitializeOnLoad]
    internal static class WorkGlovesEquipmentValidation
    {
        private const string Key = "DiceFree.WorkGlovesValidation.";
        private static IEnumerator flow;
        private static double deadline;
        private static GameObject player;
        private static ManifestationPersistence persistence;
        private static Equipment equipment;
        private static CarriedInventory inventory;
        private static EquipmentPresentation presentation;
        private sealed class NoDrop : DiceFree.World.IRandomSource { public double NextUnit() => .9; }

        static WorkGlovesEquipmentValidation()
        {
            EditorApplication.playModeStateChanged += OnPlay;
        }

        [CliCommand("dicefree.art.gloves.equipment-test", "Start isolated Q4 reward, equip/unequip, stat and scene-reload validation; poll gloves.status.")]
        private static object Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Requires idle Edit Mode.");
            var root = Path.Combine(Path.GetTempPath(), "DiceFree-WorkGloves-" + Guid.NewGuid().ToString("N"));
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Key + "root", root);
            SessionState.SetString(Key + "status", "running");
            SessionState.SetString(Key + "error", "");
            SessionState.SetBool(Key + "active", true);
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.art.gloves.status", "Read the most recent work-glove equipment validation result.")]
        private static object Status() => new { status = SessionState.GetString(Key + "status", "idle"),
            success = SessionState.GetString(Key + "status", "idle") == "passed",
            finalMarker = SessionState.GetString(Key + "status", "idle") == "passed" ? "WORK_GLOVES_EQUIPMENT_OK" : "",
            error = SessionState.GetString(Key + "error", ""), temporarySaveRoot = SessionState.GetString(Key + "root", "") };

        private static void OnPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key + "active", false)) return;
            flow = Flow(); deadline = EditorApplication.timeSinceStartup + 90;
            EditorApplication.update += Tick;
            Application.logMessageReceived += Log;
        }

        private static void Tick()
        {
            try { Require(EditorApplication.timeSinceStartup < deadline, "Glove equipment validation timed out."); if (!flow.MoveNext()) Finish(null); }
            catch (Exception error) { Finish(error.ToString()); }
        }

        private static void Log(string message, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (message.Contains("ArgumentOutOfRangeException") && stack.Contains("SearchDatabase") && stack.Contains("IndexationOnStartup")) return;
            Finish(message + "\n" + stack);
        }

        private static void Finish(string error)
        {
            EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
            SessionState.SetBool(Key + "active", false);
            SessionState.SetString(Key + "error", error ?? "");
            SessionState.SetString(Key + "status", error == null ? "passed" : "failed");
            if (error == null) Debug.Log("WORK_GLOVES_EQUIPMENT_OK"); else Debug.LogError(error);
            EditorApplication.ExitPlaymode();
        }

        private static IEnumerator BindPlayer()
        {
            do { yield return null; persistence = UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>(); }
            while (persistence == null || !persistence.Ready);
            player = persistence.gameObject;
            equipment = player.GetComponent<Equipment>(); inventory = player.GetComponent<CarriedInventory>();
            presentation = player.GetComponent<EquipmentPresentation>();
            foreach (var ai in UnityEngine.Object.FindObjectsByType<AggroBehaviour>()) ai.enabled = false;
        }

        private static void Visible(bool expected)
        {
            Require(presentation.Bindings.Single().visuals.All(g => g.activeSelf == expected), "Equipment/presentation visibility mismatch.");
        }

        private static IEnumerator Reload()
        {
            Require(persistence.Flush(), "Could not flush isolated save.");
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var bind = BindPlayer(); bind.MoveNext();) yield return null;
        }

        private static void Near(float actual, float expected, string label) => Require(Mathf.Abs(actual - expected) < .0001f, label + ": " + actual + " != " + expected);

        private static IEnumerator Flow()
        {
            for (var bind = BindPlayer(); bind.MoveNext();) yield return null;
            Require(inventory.Items.Length == 0 && equipment.Slots.Length == 0, "Fresh profile was not empty."); Visible(false);
            var definition = inventory.Resolve(ItemId);
            Require(definition.slot == EquipmentSlot.Hands && definition.stats.attackSpeedPercent == 5f &&
                definition.stats.physicalDefense == 0 && definition.stats.magicalDefense == 0 && definition.stats.movementSpeedPercent == 0,
                "Gloves must have exactly +5% Attack Speed and no other stats.");
            var journal = player.GetComponent<QuestJournal>();
            var quest = journal.Definitions.Single(q => q.stableId == "quest.cornberg.break-slime-surge");
            Require(quest.reward.items.Length == 1 && quest.reward.items.Single().stableId == ItemId && quest.reward.gold == 100 && quest.rewardXp == 200,
                "Q4 reward package changed.");
            // Unlock the existing Q4 prerequisites without bypassing its actual
            // accept, defeat-credit, turn-in or reward transaction implementation.
            var records = journal.CaptureState();
            foreach (var record in records.Where(r => r.questId != quest.stableId && r.questId != "quest.cornberg.become-runner"))
            {
                var q = journal.Definitions.Single(d => d.stableId == record.questId);
                record.status = QuestStatus.Completed; record.stage = q.stages.Length - 1; record.count = q.stages.Last().count;
            }
            journal.RestoreState(records);
            Require(journal.Accept(quest), "Q4 accept failed.");
            var elite = CombatValidationActors.Find("enemy.forest-elite-slime");
            elite.GetComponent<FixedWorldDrop>().SetRandom(new NoDrop());
            elite.Health.ApplyDamage(player.GetComponent<CombatActor>(), new DamageResult { mitigated = 100000 });
            Require(journal.GetProgress(quest.stableId).status == QuestStatus.ReadyToTurnIn && journal.TurnIn(quest) && !journal.TurnIn(quest), "Q4 exactly-once turn-in failed.");
            Require(inventory.Items.Length == 1 && inventory.Items.Single().definitionId == ItemId, "Q4 did not grant exactly one glove instance.");
            string id = inventory.Items.Single().instanceId;
            Visible(false); // Quest completion alone must never equip the appearance.
            var stats = player.GetComponent<ActorStats>();
            float speed = stats.AttackSpeed, move = stats.MoveSpeed, physical = stats.Defense(DamageChannel.Physical), magical = stats.Defense(DamageChannel.Magical);
            var novice = player.GetComponent<NovicePresentationDriver>().VisualRoot;
            var baseline = novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => !r.transform.IsChildOf(presentation.Bindings.Single().visuals.Single().transform)).ToArray();
            var meshes = baseline.Select(r => r.sharedMesh).ToArray();
            var materials = baseline.Select(r => r.sharedMaterials).ToArray();
            var states = baseline.Select(r => r.enabled).ToArray();
            Require(!equipment.Equip("missing", EquipmentSlot.Hands) && !equipment.Equip(id, EquipmentSlot.Feet), "Invalid/wrong-slot equip accepted.");
            for (int i = 0; i < 3; i++)
            {
                Require(equipment.Equip(id, EquipmentSlot.Hands), "Equip failed."); Visible(true);
                Near(stats.AttackSpeed, speed * 1.05f, "Exactly 1.05 attack multiplier");
                Near(stats.MoveSpeed, move, "Move speed changed"); Near(stats.Defense(DamageChannel.Physical), physical, "Physical defense changed");
                Near(stats.Defense(DamageChannel.Magical), magical, "Magical defense changed");
                Require(equipment.Unequip("Hands"), "Unequip failed."); Visible(false); Near(stats.AttackSpeed, speed, "Attack baseline restore");
                for (int r = 0; r < baseline.Length; r++) Require(baseline[r].sharedMesh == meshes[r] && baseline[r].sharedMaterials.SequenceEqual(materials[r]) && baseline[r].enabled == states[r], "Baseline renderer mutated.");
            }
            equipment.Equip(id, EquipmentSlot.Hands); Visible(true);
            presentation.enabled = false; Visible(false);
            presentation.enabled = true; Visible(true);
            var driver = player.GetComponent<NovicePresentationDriver>(); driver.enabled = false;
            ValidateSkin(novice.gameObject, presentation.Bindings.Single().visuals.Single()); driver.enabled = true;
            Debug.Log("WORK_GLOVES_EQUIP_STATS_BASELINE_OK multiplier=1.05 noExtraStats=true baselineMeshesUnchanged=true");
            for (var reload = Reload(); reload.MoveNext();) yield return null;
            Require(inventory.Items.Length == 1 && inventory.Items.Single().instanceId == id && equipment.Slots.Single().instanceId == id, "Equipped reload lost identity/state.");
            Visible(true);
            Require(!player.GetComponent<QuestJournal>().TurnIn(quest), "Reload replayed Q4 reward.");
            equipment.Unequip("Hands"); Visible(false);
            for (var reload = Reload(); reload.MoveNext();) yield return null;
            Require(equipment.Slots.Length == 0 && inventory.Items.Single().instanceId == id, "Unequipped reload changed ownership."); Visible(false);
            equipment.Equip(id, EquipmentSlot.Hands);
            var future = new ItemInstance { instanceId = Guid.NewGuid().ToString("D"), definitionId = "item.future.unresolved" };
            inventory.Restore(inventory.Items.Concat(new[] { future }).ToArray());
            equipment.Restore(new[] { new EquippedItem { slotId = "Hands", instanceId = future.instanceId } }); Visible(false);
            equipment.Equip(id, EquipmentSlot.Hands); Visible(true);
            inventory.Remove(id); Visible(false);
            Require(equipment.Slots.Length == 0, "Removing equipped glove retained a reference.");
            Debug.Log("WORK_GLOVES_Q4_RELOAD_OK equippedAndUnequipped=true stableInstance=true rewardExactlyOnce=true unresolvedInert=true removalRestoresBaseline=true");
        }
    }
}

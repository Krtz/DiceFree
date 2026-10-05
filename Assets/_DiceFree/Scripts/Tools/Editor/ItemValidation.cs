using System;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;
namespace DiceFree.EditorTools
{
    [InitializeOnLoad] public static class ItemValidation
    {
        private const string Running = "DiceFree.ItemValidation";
        private const string Pipeline = "DiceFree.ItemValidation.Pipeline";
        private static float deadline;
        static ItemValidation() => EditorApplication.playModeStateChanged += OnPlay;
        internal static void RunFromPipeline(string root)
        {
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            CornbergValidation.ValidateNavigation(); ValidateV2();
            SessionState.SetBool(Pipeline, true);
            SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
        }
        public static void Run()
        {
            try
            {
                CornbergValidation.ValidateNavigation(); ValidateV2();
                SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
            deadline = Time.realtimeSinceStartup + 40; EditorApplication.update += Tick; Application.logMessageReceived += OnLog;
        }
        private static void OnLog(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            if (message.Contains("ArgumentOutOfRangeException") && stack.Contains("SearchDatabase") && stack.Contains("SearchInit.IndexationOnStartup")) return;
            Finish(1);
        }
        private static void Near(float a, float b, string label) => Require(Mathf.Abs(a - b) < 0.0001f, label + $": {a} != {b}");
        private static void Tick()
        {
            try
            {
                var actor = UnityEngine.Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
                var persistence = actor.GetComponent<ManifestationPersistence>();
                if (!persistence.Ready) { Require(Time.realtimeSinceStartup < deadline, persistence.Status); return; }
                foreach (var ai in UnityEngine.Object.FindObjectsByType<AggroBehaviour>()) ai.enabled = false;
                var bag = actor.GetComponent<CarriedInventory>(); var gear = actor.GetComponent<Equipment>(); var wallet = actor.GetComponent<GoldWallet>();
                var args = Environment.GetCommandLineArgs();
                if (Array.IndexOf(args, "-diceFreeVerifyItems") >= 0)
                {
                    Require(bag.Items.Length == 4 && gear.Slots.Length == 3 && wallet.Gold == 37, "Durable items/equipment/gold reload or duplicate grant.");
                    Require(bag.Items.Select(i => i.instanceId).Distinct().Count() == 4, "Instance identity collision on reload.");
                    Require(bag.Items.Any(i => i.definitionId == "item.future.unresolved"), "Missing definition ownership lost.");
                    Require(gear.Slots.Any(s => s.slotId == "FutureSlot"), "Unknown slot lost.");
                    var inspection = ProfileInspection.Capture(persistence);
                    Require(inspection.gold == 37 && inspection.inventory.Length == 4 && inspection.equipment.Length == 3 &&
                        inspection.preserved.Any(s => s.Contains("Unresolved item")), "Inspector missing durable item diagnostics.");
                    Require(actor.Health.Current == actor.Health.Maximum, "Load not full HP.");
                    var first = bag.Items.Select(i => i.instanceId).ToArray();
                    Require(persistence.Flush(), "Reload save failed.");
                    var loaded = new LocalEchoStore(Path.GetDirectoryName(persistence.SavePath)).Load();
                    var saved = JsonUtility.FromJson<ManifestationSave>(loaded.sections.Find(s => s.id == "manifestation:" + actor.Stats.Definition.stableId).json);
                    Require(first.SequenceEqual(saved.inventory.Select(i => i.instanceId)), "Reload regenerated identities.");
                    Debug.Log("ITEM_RELOAD_OK"); Finish(0); return;
                }
                Require(bag.Items.Length == 0 && wallet.Gold == 0, "Fresh items/gold must be empty.");
                float attack = actor.Stats.AttackSpeed, move = actor.Stats.MoveSpeed;
                float physical = actor.Stats.Defense(DamageChannel.Physical), magical = actor.Stats.Defense(DamageChannel.Magical);
                Near(MovementUnits.DisplaySpeed(5), 50, "Metric baseline"); Near(MovementUnits.DisplaySpeed(5 * 1.01f), 50.5f, "Metric percentage");
                Require(move > 4.9f && move < 5.1f, "Physical baseline retuned.");
                var gloves = bag.Definitions.Single(d => d.slot == EquipmentSlot.Hands); var shoes = bag.Definitions.Single(d => d.slot == EquipmentSlot.Feet);
                var g1 = FixedItemGrant.Grant(bag, gloves); var g2 = FixedItemGrant.Grant(bag, gloves); var shoe = FixedItemGrant.Grant(bag, shoes);
                Require(g1.instanceId != g2.instanceId && bag.Items.Length == 3, "Duplicate definitions collided.");
                Require(!gear.Equip("missing", EquipmentSlot.Hands) && !gear.Equip(g1.instanceId, EquipmentSlot.Feet), "Invalid equipment accepted.");
                for (int i = 0; i < 6; i++)
                {
                    Require(gear.Equip(g1.instanceId, EquipmentSlot.Hands), "Hands equip"); Near(actor.Stats.AttackSpeed, attack * 1.05f, "Gloves final percentage");
                    gear.Equip(g2.instanceId, EquipmentSlot.Hands); Near(actor.Stats.AttackSpeed, attack * 1.05f, "Duplicate replacement accumulated");
                    gear.Unequip("Hands"); Near(actor.Stats.AttackSpeed, attack, "Unequip baseline");
                }
                gear.Equip(shoe.instanceId, EquipmentSlot.Feet);
                Near(actor.Stats.Defense(DamageChannel.Physical), physical + 1, "Physical gear underlying");
                Near(actor.Stats.Defense(DamageChannel.Magical), magical + 1, "Magical gear underlying");
                Near(actor.Stats.MoveSpeed, move * 1.01f, "Shoes percentage");
                Require(actor.Motor.Teleport(new Vector3(36, 0, 66f / 29f)), "Road teleport");
                Near(actor.Motor.Speed, move * 1.01f * 1.15f, "Road with shoes");
                gear.Unequip("Feet"); Near(actor.Motor.Speed, move * 1.15f, "Shoe removal preserved road");
                Near(actor.Stats.Defense(DamageChannel.Physical), physical, "Defense removed");
                gear.Equip(shoe.instanceId, EquipmentSlot.Feet); gear.Equip(g2.instanceId, EquipmentSlot.Hands);
                var enemy = CombatValidationActors.Find("enemy.crop-slime");
                enemy.GetComponent<BasicAttack>().Order(actor);
                Require(actor.InCombat && gear.Unequip("Hands") && gear.Equip(g2.instanceId, EquipmentSlot.Hands), "Overworld combat equipment swap rejected.");
                enemy.GetComponent<BasicAttack>().Cancel();
                actor.Stats.ResetTransientModifiers(); Near(actor.Stats.AttackSpeed, attack * 1.05f, "Transient reset erased equipment");
                var anchor = actor.GetComponent<RespawnAtAnchor>();
                var runtimeAnchor = new SerializedObject(anchor); runtimeAnchor.FindProperty("delay").floatValue = 0; runtimeAnchor.ApplyModifiedPropertiesWithoutUndo();
                actor.Health.ApplyDamage(null, new DamageResult { mitigated = 100000 });
                Require(!actor.Alive, "Death fixture failed"); Near(actor.Motor.Speed, move * 1.01f, "Dead road contribution stuck");
                Require(anchor.Return(), "Equipped actor Return failed"); Near(actor.Motor.Speed, move * 1.01f, "Return lost shoes or retained road");
                Require(actor.GetComponent<RespawnAtAnchor>().LoadAtAnchor("invalid"), "Return/load anchor");
                Near(actor.Motor.Speed, move * 1.01f, "Anchor reset retained stale road or lost shoes");
                Require(bag.Remove(g1.instanceId) && bag.Find(g2.instanceId) != null, "Removal affected wrong copy");
                var removed = bag.Grant(gloves.stableId); gear.Equip(removed.instanceId, EquipmentSlot.Hands); bag.Remove(removed.instanceId);
                Near(actor.Stats.AttackSpeed, attack, "Removed equipped copy retained stats");
                gear.Equip(g2.instanceId, EquipmentSlot.Hands); bag.Grant(gloves.stableId);
                wallet.Grant(50); Require(wallet.Spend(13) && !wallet.Spend(38) && !wallet.Spend(-1) && wallet.Gold == 37, "Gold grant/spend");
                bool rejected = false; try { wallet.Grant(-1); } catch (ArgumentOutOfRangeException) { rejected = true; } Require(rejected, "Negative grant");
                var slots = gear.Slots;
                var unknown = new ItemInstance { instanceId = Guid.NewGuid().ToString("D"), definitionId = "item.future.unresolved" };
                bag.Restore(bag.Items.Concat(new[] { unknown }).ToArray());
                gear.Restore(slots.Concat(new[] { new EquippedItem { slotId = "FutureSlot", instanceId = unknown.instanceId } }).ToArray());
                Near(actor.Stats.AttackSpeed, attack * 1.05f, "Unknown equipment not inert");
                Require(!gear.Equip(unknown.instanceId, EquipmentSlot.Head), "Unresolved item equipped");
                Require(persistence.Flush(), "Item persistence write failed");
                Debug.Log("ITEM_FOUNDATION_OK: identity, slots, stats, roads, units, gold, unknown preservation and durable write."); Finish(0);
            }
            catch (Exception e) { Debug.LogException(e); Finish(1); }
        }
        private static void ValidateV2()
        {
            string root = Path.Combine(Path.GetTempPath(), "DiceFree-item-migration-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
            var store = new LocalEchoStore(root); File.Copy("Assets/_DiceFree/Tests/Fixtures/echo-v2.json", store.Path);
            string original = File.ReadAllText(store.Path); var current = store.Load();
            Require(current.schemaVersion == SaveMigrations.CurrentSchema, "v2 fixture migration");
            var state = JsonUtility.FromJson<ManifestationSave>(current.sections.First(s => s.id.StartsWith("manifestation:")).json);
            Require(state.inventory.Length == 0 && state.equipment.Length == 0 && state.gold == 0 && state.level == 6, "Migration defaults/progression");
            Require(File.ReadAllText(store.Path) == original && JsonUtility.ToJson(SaveMigrations.Upgrade(current)) == JsonUtility.ToJson(current), "Migration not safe/idempotent");
            current.revision++; store.Commit(current); Require(File.ReadAllText(store.Path + ".bak1") == original, "v2 original backup");
            Debug.Log("ITEM_V2_MIGRATION_OK");
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
            if (SessionState.GetBool(Pipeline, false))
            {
                SessionState.SetBool(Pipeline, false); WorkGlovesRegressionValidation.Complete(code); EditorApplication.ExitPlaymode();
            }
            else EditorApplication.Exit(code);
        }
    }
}

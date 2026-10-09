using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.Banking;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class BankBalancingValidation
    {
        private const string Key = "DiceFree.BankBalancing.";
        private static IEnumerator flow;
        private static double deadline;
        private static int checks;
        static BankBalancingValidation() => EditorApplication.playModeStateChanged += Changed;
        private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); checks++; }
        [CliCommand("dicefree.bank-balancing.playtest", "Isolated-save bank transactions, capacity, class switching and real scene reload; never uses user saves.")]
        public static object Start()
        {
            VillageArtAuthoring.EditMode();
            string root = Path.Combine(Path.GetTempPath(), "DiceFree-Bank-" + Guid.NewGuid().ToString("N"));
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root); SessionState.SetString(Key + "root", root);
            SessionState.SetString(Key + "status", "running"); SessionState.SetString(Key + "error", "");
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity"); EditorApplication.EnterPlaymode(); return Status();
        }
        [CliCommand("dicefree.bank-balancing.status", "Read actual isolated bank Play Mode results.")]
        public static object Status() => new { status = SessionState.GetString(Key + "status", "idle"), checks = SessionState.GetInt(Key + "checks", 0), error = SessionState.GetString(Key + "error", ""), saveRoot = SessionState.GetString(Key + "root", "") };
        private static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetString(Key + "status", "") == "running")
            { Application.runInBackground = true; checks = 0; deadline = EditorApplication.timeSinceStartup + 90; flow = Flow(); EditorApplication.update += Tick; Application.logMessageReceived += Log; }
            if (state == PlayModeStateChange.ExitingPlayMode && flow != null) Finish("Play Mode interrupted.", false);
        }
        private static void Tick() { try { if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Bank Play Mode timed out."); if (!flow.MoveNext()) Finish(null); } catch (Exception e) { Finish(e.ToString()); } }
        private static void Log(string text, string stack, LogType type) { if (type is LogType.Error or LogType.Exception) Finish(text + "\n" + stack); }
        private static void Finish(string error, bool stop = true)
        {
            EditorApplication.update -= Tick; Application.logMessageReceived -= Log; flow = null;
            SessionState.SetString(Key + "status", error == null ? "passed" : "failed"); SessionState.SetString(Key + "error", error ?? ""); SessionState.SetInt(Key + "checks", checks);
            if (error == null) Debug.Log("BANK_BALANCING_PLAYMODE_OK checks=" + checks);
            if (stop) EditorApplication.ExitPlaymode();
        }
        private static CombatActor Hero() => UnityEngine.Object.FindFirstObjectByType<TraversalInput>()?.GetComponent<CombatActor>();
        private static IEnumerator Access(CombatActor hero)
        {
            var banker = UnityEngine.Object.FindFirstObjectByType<BankInteraction>();
            hero.GetComponent<BasicAttack>().Cancel();
            Check(hero.Motor.Teleport(banker.ApproachPosition), "Banker approach teleport failed.");
            var interactor = hero.GetComponent<Interactor>();
            hero.GetComponent<InteractionRegistry>().RegisterExistingSceneTargets();
            Check(interactor.Order(banker), "Banker not registered/reachable.");
            do { yield return null; } while (interactor.Active != banker);
            Check(hero.GetComponent<EchoSharedBank>().TownAccess, "Actual banker did not grant town access.");
        }
        private static IEnumerator Flow()
        {
            CombatActor hero;
            do { yield return null; hero = Hero(); } while (hero == null || !hero.Motor.Ready || !hero.GetComponent<ManifestationPersistence>().Ready);
            foreach (var ai in UnityEngine.Object.FindObjectsByType<DiceFree.AI.AggroBehaviour>()) ai.enabled = false;
            var persistence = hero.GetComponent<ManifestationPersistence>(); var bank = hero.GetComponent<EchoSharedBank>();
            var inv = hero.GetComponent<CarriedInventory>(); var equipment = hero.GetComponent<Equipment>(); var wallet = hero.GetComponent<GoldWallet>();
            string originalClass = persistence.CurrentClassId; wallet.Restore(1500);
            var dagger = inv.Grant("item.bronze-dagger"); var records = inv.Items; records.Single(i => i.instanceId == dagger.instanceId).bound = true; inv.Restore(records);
            Check(equipment.Equip(dagger.instanceId, EquipmentSlot.MainHand), "Cannot equip fixture dagger.");
            Check(!bank.Deposit(dagger.instanceId, out _) && inv.Find(dagger.instanceId) != null, "Equipped item deposited.");
            Check(equipment.Unequip(EquipmentSlot.MainHand.ToString()), "Cannot explicitly unequip.");
            bool nestedRejected = false; Action nested = () => nestedRejected = !bank.Deposit(dagger.instanceId, out _); inv.Changed += nested;
            Check(bank.Deposit(dagger.instanceId, out _), "Remote item deposit failed."); inv.Changed -= nested;
            Check(nestedRejected && inv.Find(dagger.instanceId) == null && bank.Items.Single().bound, "Reentrancy or full item metadata failed.");
            Check(!bank.Deposit(dagger.instanceId, out _) && bank.Items.Length == 1, "Double deposit duplicated item.");
            Check(!bank.Withdraw(dagger.instanceId, out _) && !bank.DepositGold(100, out _) && !bank.WithdrawGold(1, out _) && !bank.Expand(out _), "Remote town-only operation allowed.");
            var shield = inv.Grant("item.slime.slime-shield"); Check(bank.Deposit(shield.instanceId, out _), "Remote other-class deposit failed.");
            var access = Access(hero); while (access.MoveNext()) yield return access.Current;
            Check(!bank.Withdraw(shield.instanceId, out _), "Wrong-class equipment withdrawn.");
            Check(bank.Withdraw(dagger.instanceId, out _) && inv.Find(dagger.instanceId).bound, "Town withdraw lost metadata.");
            Check(!bank.Withdraw(dagger.instanceId, out _) && inv.Items.Count(i => i.instanceId == dagger.instanceId) == 1, "Double withdrawal duplicated item.");
            Check(bank.Deposit(dagger.instanceId, out _), "Town redeposit failed.");
            Check(bank.DepositGold(400, out _) && wallet.Gold == 1100 && bank.Gold == 400, "Gold deposit not conserved.");
            Check(bank.WithdrawGold(125, out _) && wallet.Gold == 1225 && bank.Gold == 275, "Gold withdrawal not conserved.");
            Check(bank.Expand(out _) && bank.Capacity == 90 && wallet.Gold == 1125 && bank.ExpansionPrice == 200, "Slot expansion cost/step wrong.");
            string baseline = bank.CaptureJson(); long carryGold = wallet.Gold;
            Check(!bank.DepositGold(999999, out _) && !bank.WithdrawGold(999999, out _) && bank.CaptureJson() == baseline && wallet.Gold == carryGold, "Failed currency transaction changed balances.");
            Check(!bank.DepositGold(0, out _) && !bank.WithdrawGold(-1, out _), "Invalid amount accepted.");
            Check(bank.DepositGold(wallet.Gold, out _) && wallet.Gold == 0 && bank.Gold == 1400, "Deposit All failed.");
            Check(bank.WithdrawGold(1125, out _), "Restore carried-gold fixture failed.");
            baseline = bank.CaptureJson();
            while (bank.Items.Length < bank.Capacity) { var extra = inv.Grant("item.bronze-dagger"); Check(bank.Deposit(extra.instanceId, out _), "Cannot fill purchased bank capacity."); }
            var retained = inv.Grant("item.bronze-dagger");
            Check(!bank.Deposit(retained.instanceId, out _) && inv.Find(retained.instanceId) != null && bank.Items.Length == bank.Capacity, "Capacity failure lost or duplicated item.");
            inv.Remove(retained.instanceId); bank.RestoreJson(bank.Version, baseline);
            try { bank.RestoreJson(bank.Version, "{\"items\":[{\"instanceId\":\"bad\",\"definitionId\":\"unknown\"}],\"gold\":0}"); Check(false, "Invalid save accepted."); } catch (InvalidDataException) { Check(bank.CaptureJson() == baseline, "Invalid restore changed bank."); }
            var catalog = inv.Definitions.ToArray(); inv.Configure(Array.Empty<ItemDefinition>());
            Check(!bank.Withdraw(dagger.instanceId, out _) && bank.CaptureJson() == baseline, "Missing catalog lost item."); inv.Configure(catalog);
            Check(!BankPanel.Matches(inv.Resolve(dagger.definitionId), "[", true, out var regexError) && regexError != null, "Invalid regex escaped UI guard.");
            Check(BankPanel.Matches(inv.Resolve(dagger.definitionId), "weapons slot:MainHand", false, out _), "Semantic/structured search failed.");
            var physical = AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Progression/Physically Blessed Novice shell.asset");
            Check(persistence.TryForkAndActivate(physical, out var error), "Actual fork failed: " + error);
            Check(bank.CaptureJson() == baseline && !bank.TownAccess, "Class fork transferred shared bank or retained banker access.");
            access = Access(hero); while (access.MoveNext()) yield return access.Current;
            Check(bank.Withdraw(shield.instanceId, out _) && equipment.Equip(shield.instanceId, EquipmentSlot.OffHand), "Physical class cannot withdraw/equip shield.");
            yield return null;
            foreach (var v in hero.GetComponent<DiceFree.UI.EquipmentPresentation>().Bindings.Single(b => b.definition.stableId == shield.definitionId).visuals)
            { v.GetComponent<DiceFree.UI.ShieldSideAttachment>().Align(); var p = hero.transform.InverseTransformPoint(v.transform.position); Check(p.x <= -.54f && p.z >= .34f, "Animated shield behind player."); }
            Check(!bank.Deposit(shield.instanceId, out _), "Equipped physical shield deposited.");
            Check(equipment.Unequip(EquipmentSlot.OffHand.ToString()) && bank.Deposit(shield.instanceId, out _), "Explicit unequip/deposit failed.");
            Check(bank.DepositGold(25, out _), "Physical gold deposit failed."); long physicalGold = wallet.Gold;
            Check(persistence.TryActivateExisting(originalClass, out error), "Actual manifestation switch failed: " + error);
            Check(wallet.Gold == 1125 && bank.Gold == 300 && bank.Items.Length == 2 && inv.Find(shield.instanceId) == null, "Manifestation-specific currency/inventory or shared bank changed on switch.");
            Check(persistence.Flush(), "Shared Echo commit failed.");
            var profile = persistence.CaptureProfile(); Check(profile.sections.Count(s => s.id == bank.SectionId) == 1, "Bank not in Echo durable section.");
            string path = persistence.SavePath; Check(Path.GetFullPath(path).StartsWith(SessionState.GetString(Key + "root", ""), StringComparison.OrdinalIgnoreCase), "Harness touched nonisolated save path.");
            Check(File.Exists(path), "Echo disk save missing.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Cornberg");
            do { yield return null; hero = Hero(); } while (hero == null || !hero.Motor.Ready || !hero.GetComponent<ManifestationPersistence>().Ready);
            bank = hero.GetComponent<EchoSharedBank>(); inv = hero.GetComponent<CarriedInventory>(); persistence = hero.GetComponent<ManifestationPersistence>();
            Check(bank.Gold == 300 && bank.Capacity == 90 && bank.Items.Length == 2 && bank.Items.Single(i => i.instanceId == dagger.instanceId).bound && inv.Find(dagger.instanceId) == null, "Real scene reload lost shared state or revived deposited item.");
            Check(persistence.TryActivateExisting(physical.stableId, out error) && hero.GetComponent<GoldWallet>().Gold == physicalGold && bank.Items.Any(i => i.instanceId == shield.instanceId), "Reload lost other manifestation or shared bank.");
            var runLock = hero.GetComponent<RunLoadoutLock>() ?? hero.gameObject.AddComponent<RunLoadoutLock>(); runLock.InsideDungeon = true; runLock.Locked = true;
            var remote = inv.Grant("item.bronze-dagger"); Check(bank.Deposit(remote.instanceId, out _) && !bank.Withdraw(remote.instanceId, out _) && !bank.DepositGold(1, out _), "Dungeon remote/town rules failed.");
            runLock.InsideDungeon = false; runLock.Locked = false;
        }
    }
}

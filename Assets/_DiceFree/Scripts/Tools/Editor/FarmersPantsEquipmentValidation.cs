using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;

using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using static DiceFree.EditorTools.FarmersPantsArtValidation;

namespace DiceFree.EditorTools
{
    /// <summary>Prototype-only integration harness using Equipment and isolated schema-v4 persistence.</summary>
    [InitializeOnLoad]
    internal static class FarmersPantsEquipmentValidation
    {
        private const string Key = "DiceFree.FarmersPantsValidation.";
        private static IEnumerator flow;
        private static double deadline;
        private static GameObject player;
        private static ManifestationPersistence persistence;
        private static Equipment equipment;
        private static CarriedInventory inventory;
        private static EquipmentPresentation presentation;


        static FarmersPantsEquipmentValidation()
        {
            EditorApplication.playModeStateChanged += OnPlay;
        }

        [CliCommand("dicefree.art.pants.equipment-test", "Start isolated prototype equip/unequip, stat and scene-reload validation; poll pants.status.")]
        private static object Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Requires idle Edit Mode.");
            var root = Path.Combine(Path.GetTempPath(), "DiceFree-FarmersPants-" + Guid.NewGuid().ToString("N"));
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Key + "root", root);
            SessionState.SetString(Key + "status", "running");
            SessionState.SetString(Key + "error", "");
            SessionState.SetBool(Key + "active", true);
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.art.pants.status", "Read the most recent pants equipment validation result.")]
        private static object Status() => new { status = SessionState.GetString(Key + "status", "idle"),
            success = SessionState.GetString(Key + "status", "idle") == "passed",
            finalMarker = SessionState.GetString(Key + "status", "idle") == "passed" ? "FARMERS_PANTS_EQUIPMENT_OK" : "",
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
            try { Require(EditorApplication.timeSinceStartup < deadline, "Pants equipment validation timed out."); if (!flow.MoveNext()) Finish(null); }
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
            if (error == null) Debug.Log("FARMERS_PANTS_EQUIPMENT_OK"); else Debug.LogError(error);
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
            Require(presentation.Bindings.Single(b => b.slot == EquipmentSlot.Legs).visuals.All(g => g.activeSelf == expected), "Equipment/presentation visibility mismatch.");
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
            Require(inventory.Items.Length == 0 && equipment.Slots.Length == 0, "Fresh profile must stay empty."); Visible(false);
            var definition = inventory.Resolve(ItemId);
            var package = definition.stats;
            Require(definition.displayName == "Farmer's Pants" && definition.slot == EquipmentSlot.Legs && package.attributes.vitality == 1 && package.attributes.strength == 0 && package.attributes.agility == 0 && package.attributes.intelligence == 0 && package.attributes.spirit == 0 &&
                package.physicalDefense == 1 && package.magicalDefense == 0 && package.attackSpeedPercent == 0 && package.movementSpeedPercent == 0,
                "Pants must have exactly +1 Physical Defense and +1 Vitality.");
            var stats = player.GetComponent<ActorStats>();
            var health = player.GetComponent<Health>();
            var attributes = stats.Attributes;
            stats.SetEquipmentContribution("validation.primary-a", new EquipmentStats { attributes = new AttributeValues { vitality=1, strength=2, agility=3, intelligence=4, spirit=5 } });
            stats.SetEquipmentContribution("validation.primary-b", new EquipmentStats { attributes = new AttributeValues(1) });
            Near(stats.Attributes.vitality, attributes.vitality+2, "Generic Vitality aggregation");
            Near(stats.Attributes.strength, attributes.strength+3, "Generic Strength aggregation");
            Near(stats.Attributes.agility, attributes.agility+4, "Generic Agility aggregation");
            Near(stats.Attributes.intelligence, attributes.intelligence+5, "Generic Intelligence aggregation");
            Near(stats.Attributes.spirit, attributes.spirit+6, "Generic Spirit aggregation");
            stats.RemoveEquipmentContribution("validation.primary-a"); stats.RemoveEquipmentContribution("validation.primary-b");
            Near(stats.Attributes.Weighted(new AttributeValues(1)), attributes.Weighted(new AttributeValues(1)), "Generic attributes restored");
            Debug.Log("EQUIPMENT_PRIMARY_ATTRIBUTES_OK allFive=true sources=2");
            float physical = stats.Defense(DamageChannel.Physical), magical = stats.Defense(DamageChannel.Magical);
            float speed = stats.AttackSpeed, move = stats.MoveSpeed, hp = stats.MaximumHp, regen = stats.Regeneration;
            var novice = player.GetComponent<NovicePresentationDriver>().VisualRoot;
            var baseline = novice.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(r => r.name.StartsWith("Novice_", StringComparison.Ordinal)).ToArray();
            var meshes = baseline.Select(r => r.sharedMesh).ToArray(); var materials = baseline.Select(r => r.sharedMaterials).ToArray();
            var enabled = baseline.Select(r => r.enabled).ToArray(); var active = baseline.Select(r => r.gameObject.activeSelf).ToArray();
            var underwear = BaselineLegs(novice);
            Require(underwear.activeSelf, "Authored baseline underwear missing.");
            var pants = inventory.Grant(ItemId); string id = pants.instanceId;
            Require(!equipment.Equip("missing", EquipmentSlot.Legs) && !equipment.Equip(id, EquipmentSlot.Feet), "Invalid/wrong-slot equip accepted.");
            for (int i=0; i<3; i++)
            {
                Require(equipment.Equip(id, EquipmentSlot.Legs), "Equip failed."); Visible(true);
                Require(!underwear.activeSelf, "Underwear must be replaced.");
                Near(stats.Attributes.vitality, attributes.vitality+1, "Vitality");
                Near(stats.Attributes.strength, attributes.strength, "Strength"); Near(stats.Attributes.agility, attributes.agility, "Agility");
                Near(stats.Attributes.intelligence, attributes.intelligence, "Intelligence"); Near(stats.Attributes.spirit, attributes.spirit, "Spirit");
                Near(stats.Defense(DamageChannel.Physical),physical+1,"Physical Defense"); Near(stats.Defense(DamageChannel.Magical),magical,"Magical Defense");
                Near(stats.AttackSpeed,speed,"Attack Speed"); Near(stats.MoveSpeed,move,"Move Speed");
                Near(stats.MaximumHp,hp+15,"Vitality-derived maximum HP");
                Near(stats.Regeneration,regen+.1f,"Vitality-derived regeneration");
                Near(health.Current,health.Maximum,"Full health survives maximum change");
                stats.ResetTransientModifiers(); Near(stats.Attributes.vitality,attributes.vitality+1,"Durable vitality survives transient reset");
                Require(equipment.Unequip("Legs"),"Unequip failed."); Visible(false);
                Near(stats.MaximumHp,hp,"Maximum HP restored"); Near(health.Current,hp,"Full health restored");
                for (int r=0;r<baseline.Length;r++) Require(baseline[r].sharedMesh==meshes[r] && baseline[r].sharedMaterials.SequenceEqual(materials[r]) &&
                    baseline[r].enabled==enabled[r] && baseline[r].gameObject.activeSelf==active[r],"Exact baseline mutated.");
            }
            // Existing missing-HP contract; maximum changes neither heal a wound nor deal damage.
            health.ApplyDamage(player.GetComponent<CombatActor>(),new DamageResult { mitigated=5 });
            float missing = health.Maximum-health.Current;
            equipment.Equip(id,EquipmentSlot.Legs); Near(health.Maximum-health.Current,missing,"Missing HP preserved on equip");
            equipment.Unequip("Legs"); Near(health.Maximum-health.Current,missing,"Missing HP preserved on unequip"); health.Restore();
            equipment.Equip(id,EquipmentSlot.Legs);
            presentation.enabled=false; Visible(false); Require(underwear.activeSelf,"Disable did not restore baseline.");
            presentation.enabled=true; Visible(true); Require(!underwear.activeSelf,"Re-enable did not replace baseline.");
            // Authored inactive baselines must stay inactive; Configure must restore old references.
            var inactive = new GameObject("Inactive baseline fixture"); inactive.transform.SetParent(novice); inactive.SetActive(false);
            var originalBindings = presentation.Bindings;
            var legs = originalBindings.Single(b=>b.slot==EquipmentSlot.Legs);
            presentation.Configure(originalBindings.Where(b=>b.slot!=EquipmentSlot.Legs).Concat(new[] { new EquipmentPresentation.Binding {
                slot=EquipmentSlot.Legs,definition=definition,visuals=legs.visuals,baselineVisuals=new[] { underwear,inactive } } }).ToArray());
            equipment.Unequip("Legs"); Require(underwear.activeSelf && !inactive.activeSelf,"Inactive authored baseline was incorrectly activated.");
            presentation.Configure(originalBindings); UnityEngine.Object.Destroy(inactive);
            // Shoes are on a separate, unmerged art branch. Use explicit foot-socket fixtures to
            // exercise the generic Feet seam without importing that branch or inventing shoe art.
            var animator = novice.GetComponentInChildren<Animator>();
            var footFixtures = new[] { HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot }.Select(b => {
                var g=new GameObject("Feet presentation fixture");g.transform.SetParent(animator.GetBoneTransform(b),false);g.SetActive(false);return g; }).ToArray();
            var feetBaseline = new GameObject("Feet baseline fixture"); feetBaseline.transform.SetParent(novice); feetBaseline.SetActive(true);
            var shoesDef = inventory.Resolve("item.cornberg.forest-shoes");
            presentation.Configure(originalBindings.Concat(new[] { new EquipmentPresentation.Binding { slot=EquipmentSlot.Feet,
                definition=shoesDef,visuals=footFixtures,baselineVisuals=new[] { feetBaseline } } }).ToArray());
            var shoes=inventory.Grant(shoesDef.stableId); var gloves=inventory.Grant("item.cornberg.work-gloves");
            equipment.Equip(shoes.instanceId,EquipmentSlot.Feet); equipment.Equip(gloves.instanceId,EquipmentSlot.Hands); equipment.Equip(id,EquipmentSlot.Legs);
            Visible(true); Require(footFixtures.All(g=>g.activeSelf) && !feetBaseline.activeSelf,"Feet appearance conflict.");
            Require(presentation.Bindings.Single(b=>b.slot==EquipmentSlot.Hands).visuals.All(g=>g.activeSelf),"Gloves were hidden by pants.");
            Near(stats.MoveSpeed,move*1.01f,"Shoes coexist");Near(stats.AttackSpeed,speed*1.05f,"Gloves coexist");
            Near(stats.Defense(DamageChannel.Physical),physical+2,"Pants/shoes Defense");Near(stats.Defense(DamageChannel.Magical),magical+1,"Shoes Magical Defense");
            equipment.Unequip("Legs");Require(footFixtures.All(g=>g.activeSelf) && underwear.activeSelf,"Legs unequip disturbed Feet.");
            equipment.Equip(id,EquipmentSlot.Legs);equipment.Unequip("Feet");Visible(true);
            Require(footFixtures.All(g=>!g.activeSelf) && feetBaseline.activeSelf,"Feet unequip disturbed Legs.");
            presentation.Configure(originalBindings); foreach(var g in footFixtures) UnityEngine.Object.Destroy(g); UnityEngine.Object.Destroy(feetBaseline);
            var driver=player.GetComponent<NovicePresentationDriver>();driver.enabled=false; ValidateSkin(novice.gameObject,legs.visuals.Single());driver.enabled=true;
            equipment.Equip(shoes.instanceId,EquipmentSlot.Feet);
            for(var reload=Reload();reload.MoveNext();)yield return null;
            Require(inventory.Items.Length==3 && inventory.Find(id)!=null && equipment.Slots.Single(s=>s.slotId=="Legs").instanceId==id,"Equipped reload lost owned identity.");
            Visible(true);Require(!BaselineLegs(player.GetComponent<NovicePresentationDriver>().VisualRoot).activeSelf,"Reload left underwear visible.");
            Near(player.GetComponent<ActorStats>().Attributes.vitality,attributes.vitality+1,"Reload Vitality");
            equipment.Unequip("Legs");for(var reload=Reload();reload.MoveNext();)yield return null;
            Visible(false);Require(inventory.Find(id)!=null && !equipment.Slots.Any(s=>s.slotId=="Legs"),"Unequipped reload changed ownership.");
            Require(BaselineLegs(player.GetComponent<NovicePresentationDriver>().VisualRoot).activeSelf,"Unequipped reload lost baseline.");
            var future=new ItemInstance {instanceId=Guid.NewGuid().ToString("D"),definitionId="item.future.unresolved-pants"};
            inventory.Restore(inventory.Items.Concat(new[] {future}).ToArray());
            equipment.Restore(equipment.Slots.Concat(new[] {new EquippedItem {slotId="Legs",instanceId=future.instanceId}}).ToArray());Visible(false);
            Near(player.GetComponent<ActorStats>().Attributes.vitality,attributes.vitality,"Unresolved item is stat-inert");
            for(var reload=Reload();reload.MoveNext();)yield return null;
            Require(inventory.Find(future.instanceId)!=null && equipment.Slots.Single(s=>s.slotId=="Legs").instanceId==future.instanceId,"Unresolved record lost on reload.");Visible(false);
            equipment.Equip(id,EquipmentSlot.Legs);Visible(true);inventory.Remove(id);Visible(false);
            Require(!equipment.Slots.Any(s=>s.slotId=="Legs"),"Removal retained Legs reference.");
            Debug.Log("FARMERS_PANTS_STATS_BASELINE_FEET_RELOAD_OK exactPackage=true reloads=3 unresolvedInert=true handsFeetCoexist=true");
        }
    }
}

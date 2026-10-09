using System;
using System.Linq;
using DiceFree.Banking;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.UI;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class BankBalancingAuthoring
    {
        internal static readonly string[] Scenes = { "Cornberg", "SlimeDungeon", "DungeonRewardRoom", "NoviceMountainTrial" };
        internal static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
        internal static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        private static T Ensure<T>(GameObject obj) where T : Component => obj.GetComponent<T>() ?? obj.AddComponent<T>();
        internal static SlimeDungeonTuning Tuning => AssetDatabase.LoadAssetAtPath<SlimeDungeonTuning>("Assets/_DiceFree/Settings/Dungeons/Slime playtest tuning.asset");
        [CliCommand("dicefree.bank-balancing.install", "Scoped bank, camera, boss and service geometry installation; preserves existing scene/art.")]
        public static object Install()
        {
            VillageArtAuthoring.EditMode();
            var catalog = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_DiceFree" }).Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            Check(catalog.All(d => !string.IsNullOrWhiteSpace(d.stableId)) && catalog.GroupBy(d => d.stableId).All(g => g.Count() == 1), "Item catalog IDs must be unique.");
            var tuning = Tuning; Check(tuning != null && tuning.regent != null, "Regent definition missing.");
            tuning.bossHp = tuning.boss.baseHp = 3000; tuning.bossFragmentHp = 300; tuning.fragment.baseHp = 300; tuning.minibossScale = 2.3f * 3;
            tuning.regentHp = tuning.regent.baseHp = 16000; tuning.regentFragmentHpFraction = .10f;
            EditorUtility.SetDirty(tuning); EditorUtility.SetDirty(tuning.boss); EditorUtility.SetDirty(tuning.fragment); EditorUtility.SetDirty(tuning.regent);
            int heroes = 0, shields = 0;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (var name in Scenes)
                {
                    string path = "Assets/_DiceFree/Scenes/" + name + ".unity";
                    Check(System.IO.File.Exists(path), "Missing existing scene " + path);
                    var scene = SceneManager.GetSceneByPath(path); bool opened = !scene.isLoaded;
                    if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    foreach (var persistence in All<ManifestationPersistence>(scene))
                    {
                        Ensure<EchoSharedBank>(persistence.gameObject); Ensure<BankPanel>(persistence.gameObject);
                        var inventory = persistence.GetComponent<CarriedInventory>(); Check(inventory != null, "Hero missing inventory.");
                        inventory.Configure(catalog); EditorUtility.SetDirty(inventory); heroes++;
                    }
                    foreach (var camera in All<ExplorationCamera>(scene)) { camera.ConfigureZoom(8, 110, .60f); EditorUtility.SetDirty(camera); }
                    foreach (var presentation in All<EquipmentPresentation>(scene))
                        foreach (var binding in presentation.Bindings.Where(b => b.definition?.stableId == "item.slime.slime-shield"))
                            foreach (var visual in binding.visuals)
                            {
                                var rig = visual.GetComponentInParent<Animator>(true); Check(rig != null && rig.isHuman, "Shield lacks humanoid rig.");
                                var attachment = Ensure<ShieldSideAttachment>(visual); attachment.Configure(presentation.transform, rig); EditorUtility.SetDirty(attachment); shields++;
                            }
                    if (name == "Cornberg")
                    {
                        var banker = All<Transform>(scene).Single(t => t.name == "NPCs/NPC_Banker - 3D");
                        var smith = All<Transform>(scene).Single(t => t.name == "NPCs/NPC_Blacksmith - 3D");
                        PlaceService(scene, banker, new Vector3(13, 0, 9), "Peter Banker");
                        PlaceService(scene, smith, new Vector3(-5, 0, 7), "Cornberg Blacksmith");
                        var access = Ensure<BankInteraction>(banker.gameObject); access.ConfigureName("Peter Banker"); access.ConfigureApproach(banker.Find("Service approach"));
                        var context = banker.GetComponent<ContextualNpc>();
                        if (context != null) context.Configure("npc.peter-banker", new[] { access }.Concat(banker.GetComponents<InteractionTarget>().Where(i => i != access && i != context)).ToArray());
                        if (smith.GetComponent<InteractionTarget>() == null)
                        {
                            var talk = smith.gameObject.AddComponent<ConversationTarget>(); talk.ConfigureName("Cornberg Blacksmith");
                            talk.Configure("npc.cornberg-blacksmith", "conversation.blacksmith.greeting", "Welcome to the forge. Mind the hot hearth behind me."); talk.ConfigureApproach(smith.Find("Service approach"));
                        }
                        foreach (var target in smith.GetComponents<InteractionTarget>()) target.ConfigureApproach(smith.Find("Service approach"));
                    }
                    EditorSceneManager.MarkSceneDirty(scene); Check(EditorSceneManager.SaveScene(scene), "Could not save " + path);
                    if (opened) EditorSceneManager.CloseScene(scene, true);
                }
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Check(heroes > 0 && shields >= 3, "Hero or shield coverage missing.");
            return new { success = true, heroes, shields, marker = "BANK_BALANCING_INSTALLED" };
        }
        private static void PlaceService(Scene scene, Transform root, Vector3 desired, string display)
        {
            Check(root.GetComponent<VillageHomeWander>() == null, "Service NPC must remain stationary.");
            var navigation = All<WorldNavigation>(scene).Single(); var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
                Check(NavMesh.SamplePosition(desired, out var ground, 2, NavMesh.AllAreas), "No service standing ground for " + display);
                root.position = ground.position;
                root.rotation = Quaternion.identity;
                var approach = root.Find("Service approach");
                if (approach == null) { approach = new GameObject("Service approach").transform; approach.SetParent(root, false); }
                Check(NavMesh.SamplePosition(ground.position + Vector3.back * 1.4f, out var sample, .5f, NavMesh.AllAreas), "No public service approach.");
                approach.position = sample.position;
                Physics.SyncTransforms();
                Check(!Physics.CheckCapsule(ground.position + Vector3.up * .4f, ground.position + Vector3.up * 1.7f, .4f, 1 << 9, QueryTriggerInteraction.Ignore), "Service still overlaps scenery.");
                Check(!Physics.Linecast(approach.position + Vector3.up, ground.position + Vector3.up, 1 << 9), "Service line of sight blocked.");
                var collider = root.GetComponent<CapsuleCollider>(); Check(collider != null && collider.enabled, "Keep service click collider.");
                root.gameObject.layer = 11;
                var obstacle = root.GetComponent<NavMeshObstacle>(); Check(obstacle != null, "Keep service navigation obstacle."); obstacle.carving = true;
                foreach (var label in root.GetComponentsInChildren<TextMesh>(true)) label.text = display;
            }
            finally { if (nav.valid) nav.Remove(); }
        }
        [CliCommand("dicefree.bank-balancing.validate", "Validate authored shared bank, balance, shield positions and service access in all four existing maps.")]
        public static object Validate()
        {
            VillageArtAuthoring.EditMode(); var t = Tuning;
            Check(t.bossHp == 3000 && t.boss.baseHp == 3000, "Boss must have exactly 3000 HP.");
            Check(t.regentHp == 16000 && t.regent.baseHp == 16000 && Mathf.Approximately(t.regentFragmentHpFraction,.10f), "Regent must have 16000 HP and 10% Divide fragments.");
            Check(t.bossFragmentHp == 300 && t.fragment.baseHp == 300, "Fragments must have 300 HP.");
            Check(Mathf.Approximately(t.minibossScale, 2.3f * 3), "Miniboss must be 3x previous actor size.");
            int heroes = 0, shields = 0;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string name in Scenes)
                {
                    var scene = SceneManager.GetSceneByPath("Assets/_DiceFree/Scenes/" + name + ".unity"); bool opened = !scene.isLoaded;
                    if (opened) scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/" + name + ".unity", OpenSceneMode.Additive);
                    foreach (var p in All<ManifestationPersistence>(scene))
                    {
                        Check(p.GetComponent<EchoSharedBank>() != null && p.GetComponent<BankPanel>() != null, "Bank must be on hero before BindRuntime.");
                        var inv = p.GetComponent<CarriedInventory>();
                        Check(AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/_DiceFree" }).All(g => inv.Resolve(AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g)).stableId) != null), "Incomplete all-form item catalog."); heroes++;
                    }
                    foreach (var camera in All<ExplorationCamera>(scene)) Check(Mathf.Approximately(new SerializedObject(camera).FindProperty("zoomSensitivity").floatValue, .60f), "Zoom not increased.");
                    foreach (var presentation in All<EquipmentPresentation>(scene))
                        foreach (var v in presentation.Bindings.Where(b => b.definition?.stableId == "item.slime.slime-shield").SelectMany(b => b.visuals))
                        { Check(v.GetComponent<ShieldSideAttachment>() != null, "Missing animated shield positioning."); var pos = presentation.transform.InverseTransformPoint(v.transform.position); Check(pos.x <= -.54f && pos.z >= .34f, "Shield behind torso."); shields++; }
                    if (name == "Cornberg") ValidateServices(scene);
                    if (opened) EditorSceneManager.CloseScene(scene, true);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Check(heroes > 0 && shields >= 3, "Incomplete coverage.");
            return new { success = true, heroes, shields, marker = "BANK_BALANCING_DATA_OK" };
        }
        private static void ValidateServices(Scene scene)
        {
            var nav = NavMesh.AddNavMeshData(All<WorldNavigation>(scene).Single().Data);
            try
            {
                Physics.SyncTransforms();
                foreach (string name in new[] { "NPCs/NPC_Banker - 3D", "NPCs/NPC_Blacksmith - 3D" })
                {
                    var root = All<Transform>(scene).Single(x => x.name == name);
                    Check(root.GetComponent<VillageHomeWander>() == null && root.GetComponent<NavMeshObstacle>().carving, "Stationary service collision missing.");
                    var approach = root.Find("Service approach"); Check(approach != null, "No service approach.");
                    Check(NavMesh.SamplePosition(approach.position, out var point, .3f, NavMesh.AllAreas), "Service approach off navmesh.");
                    Check(NavMesh.SamplePosition(new Vector3(8, 0, 3), out var green, 3, NavMesh.AllAreas), "Public green missing.");
                    var path = new NavMeshPath(); Check(NavMesh.CalculatePath(green.position, point.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete, "Service unreachable from green.");
                    Check(!Physics.CheckCapsule(root.position + Vector3.up * .4f, root.position + Vector3.up * 1.7f, .4f, 1 << 9, QueryTriggerInteraction.Ignore), "NPC trapped in scenery.");
                    Check(Physics.Raycast(approach.position + Vector3.up, (root.position + Vector3.up - (approach.position + Vector3.up)).normalized, out var hit, 3, (1 << 9) | (1 << 11), QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<InteractionTarget>() != null, "NPC not publicly clickable.");
                }
                Check(All<BankInteraction>(scene).Single().DisplayName == "Peter Banker", "Peter Banker not wired.");
            }
            finally { if (nav.valid) nav.Remove(); }
        }
    }
}

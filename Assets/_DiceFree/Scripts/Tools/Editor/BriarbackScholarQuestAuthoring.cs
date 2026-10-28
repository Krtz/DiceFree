using System;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    /// <summary>Additive authoring for Trimery's optional four-element Shaman hunt.</summary>
    public static class BriarbackScholarQuestAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string RegionName = "14 - Briarback quilboar frontier";
        private const string TrimeryName = "13 - Trimery lumber village";
        private const string SiteGroupName = "Briarback elemental settlements - visual blockout - provisional";
        private const string RoleGroupName = "Approved Quillboar role prototypes - additive";
        private const string GuardianGroupName = "Briarback elemental guardians - additive";
        private const string QuestId = "quest.briarback.four-elements-hunt";
        private const string QuestPath = "Assets/_DiceFree/Settings/Briarback/The Four Elements Hunt.asset";
        private const string RewardPath = "Assets/_DiceFree/Settings/Briarback/The Four Elements Hunt reward.asset";
        private const string RecoveryPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-elemental-quest-guardians-20261028.unity";
        private const string RepairRecoveryPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-elemental-quest-guardian-placement-20261028.unity";
        private const string GiverName = "Trimery Elemental Scholar - Briarback";

        private static readonly (string site, QuilboarRole role, string definitionAsset, string killText)[] Elements =
        {
            ("Fire Shaman settlement - provisional", QuilboarRole.FireShaman, "FireShaman-L25 stats", "Defeat a Fire Shaman"),
            ("Earth Shaman settlement - provisional", QuilboarRole.EarthShaman, "EarthShaman-L25 stats", "Defeat an Earth Shaman"),
            ("Lightning Shaman settlement - provisional", QuilboarRole.LightningShaman, "LightningShaman-L25 stats", "Defeat a Lightning Shaman"),
            ("Ice Shaman settlement - provisional", QuilboarRole.IceShaman, "IceShaman-L25 stats", "Defeat an Ice Shaman")
        };

        private static void Need(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("BRIARBACK ELEMENTAL QUEST: " + message);
        }

        private static Scene RequireScene()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && EditorSceneManager.GetSceneManagerSetup().Length == 1,
                "open only the saved Cornberg scene; no other scenes will be loaded or replaced.");
            Need(!scene.isDirty, "save current scene changes before authoring.");
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode before authoring.");
            return scene;
        }

        private static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        private static bool StaticCapsuleClear(Vector3 point, Transform ignored)
        {
            return !Physics.OverlapCapsule(point + Vector3.up * .48f, point + Vector3.up * 1.42f, .43f,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore)
                .Any(collider => collider != null && collider.enabled && collider.gameObject.activeInHierarchy &&
                    !collider.transform.IsChildOf(ignored));
        }

        private static Vector3[] SitePoints(Scene scene, out NavMeshDataInstance nav)
        {
            var region = All<Transform>(scene).SingleOrDefault(value => value.name == RegionName);
            var navigation = All<WorldNavigation>(scene).SingleOrDefault();
            var siteGroup = region != null ? region.Find(SiteGroupName) : null;
            Need(region != null && navigation != null && navigation.Data != null && siteGroup != null,
                "the saved region, shared NavMesh, and four-site visual group are required.");
            nav = NavMesh.AddNavMeshData(navigation.Data);
            var points = new Vector3[Elements.Length];
            try
            {
                for (int i = 0; i < Elements.Length; i++)
                {
                    var site = siteGroup.Find(Elements[i].site);
                    Need(site != null, "elemental site is missing: " + Elements[i].site);
                    var requested = site.position + Vector3.forward * 4f;
                    Need(NavMesh.SamplePosition(requested, out var hit, 2, NavMesh.AllAreas),
                        Elements[i].site + " has no NavMesh at its authored center.");
                    Need(StaticCapsuleClear(hit.position, navigation.transform),
                        Elements[i].site + " Shaman spawn overlaps a scene collider at " + hit.position.ToString("F1"));
                    points[i] = hit.position;
                }
                var roleRoot = region.Find(RoleGroupName);
                var guardianRoot = region.Find(GuardianGroupName);
                var existingActors = All<CombatActor>(scene)
                    .Where(actor => actor != null && actor.gameObject.activeInHierarchy &&
                        (roleRoot == null || !actor.transform.IsChildOf(roleRoot)) &&
                        (guardianRoot == null || !actor.transform.IsChildOf(guardianRoot)))
                    .ToArray();
                foreach (var actor in existingActors)
                    foreach (var point in points)
                        Need(Vector3.Distance(actor.transform.position, point) >= 4f,
                            "elemental spawn is too close to existing actor " + actor.name + " at " + actor.transform.position.ToString("F1"));
                for (int i = 0; i < points.Length; i++)
                for (int j = i + 1; j < points.Length; j++)
                    Need(Vector3.Distance(points[i], points[j]) >= 20f, "two elemental Shaman spawns overlap.");
                return points;
            }
            catch
            {
                if (nav.valid) nav.Remove();
                throw;
            }
        }

        [CliCommand("dicefree.briarback.elements-quest.preview",
            "Preflight four elemental Shaman spawns at their existing Briarback sites and the Trimery scholar giver. No scene changes.")]
        public static object Preview()
        {
            var scene = RequireScene();
            var points = SitePoints(scene, out var nav);
            try
            {
                var center = new Vector3(248, CornbergLandscape.Ground(248, 180).y, 180);
                Need(NavMesh.SamplePosition(center, out var origin, 4, NavMesh.AllAreas), "Trimery's NavMesh is missing.");
                var giverCandidates = new[] { new Vector2(222, 193), new Vector2(268, 205), new Vector2(224, 158), new Vector2(222, 169) };
                var interactives = All<InteractionTarget>(scene).Select(x => x.transform.position).ToArray();
                Vector3 giver = default;
                bool found = false;
                foreach (var candidate in giverCandidates)
                {
                    var ground = CornbergLandscape.Ground(candidate.x, candidate.y);
                    if (!NavMesh.SamplePosition(ground, out var hit, 2, NavMesh.AllAreas) ||
                        interactives.Any(p => Vector3.Distance(p, hit.position) < 5) ||
                        !StaticCapsuleClear(hit.position, All<WorldNavigation>(scene).Single().transform)) continue;
                    var path = new NavMeshPath();
                    if (!NavMesh.CalculatePath(origin.position, hit.position, NavMesh.AllAreas, path) ||
                        path.status != NavMeshPathStatus.PathComplete) continue;
                    giver = hit.position;
                    found = true;
                    break;
                }
                Need(found, "no collision-free connected Trimery position is available for the scholar.");
                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENTS_QUEST_PREFLIGHT_OK",
                    sites = Elements.Select((entry, index) => new { entry.role, position = points[index].ToString("F2") }).ToArray(),
                    scholarPosition = giver.ToString("F2"),
                    navMeshChanged = false,
                    recoveryCreated = false
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.elements-quest.author",
            "Add four respawning elemental Shamans and Trimery's optional four-kill quest using guarded Unity Editor APIs.")]
        public static object Author()
        {
            var scene = RequireScene();
            Need(GameObject.Find(GiverName) == null && GameObject.Find(GuardianGroupName) == null,
                "scholar or guardian group already exists; validate instead of duplicating.");
            Need(AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestPath) == null &&
                 AssetDatabase.LoadAssetAtPath<FixedRewardDefinition>(RewardPath) == null,
                "quest or reward asset already exists; refusing to overwrite it.");
            Need(!AssetDatabase.LoadMainAssetAtPath(RecoveryPath), "the recovery copy already exists; inspect it before authoring.");

            var region = All<Transform>(scene).SingleOrDefault(value => value.name == RegionName);
            var trimery = All<Transform>(scene).SingleOrDefault(value => value.name == TrimeryName);
            var roles = region != null ? region.Find(RoleGroupName) : null;
            var template = All<Transform>(scene).SingleOrDefault(value => value.name == "NPCs/NPC_Guard - 3D");
            var journals = All<QuestJournal>(scene);
            Need(region != null && trimery != null && roles != null && template != null && journals.Length > 0,
                "Trimery, Briarback role prototypes, visible guard template, or player QuestJournal is missing.");
            Need(!journals.Any(journal => journal.Definitions.Any(definition => definition != null && definition.stableId == QuestId)),
                "the quest is already in a player journal.");

            var points = SitePoints(scene, out var nav);
            GameObject guardians = null;
            QuestDefinition quest = null;
            FixedRewardDefinition reward = null;
            bool questCreated = false, rewardCreated = false;
            int undoGroup = Undo.GetCurrentGroup();
            Undo.IncrementCurrentGroup();
            undoGroup = Undo.GetCurrentGroup();
            try
            {
                var candidates = new[] { new Vector2(222, 193), new Vector2(268, 205), new Vector2(224, 158), new Vector2(222, 169) };
                var navigation = All<WorldNavigation>(scene).Single();
                var center = new Vector3(248, CornbergLandscape.Ground(248, 180).y, 180);
                Need(NavMesh.SamplePosition(center, out var origin, 4, NavMesh.AllAreas), "Trimery's NavMesh is missing.");
                var existingInteractives = All<InteractionTarget>(scene).Select(x => x.transform.position).ToArray();
                Vector3 giverPosition = default;
                bool found = false;
                foreach (var candidate in candidates)
                {
                    var ground = CornbergLandscape.Ground(candidate.x, candidate.y);
                    if (!NavMesh.SamplePosition(ground, out var hit, 2, NavMesh.AllAreas) ||
                        existingInteractives.Any(p => Vector3.Distance(p, hit.position) < 5) ||
                        !StaticCapsuleClear(hit.position, navigation.transform)) continue;
                    var path = new NavMeshPath();
                    if (!NavMesh.CalculatePath(origin.position, hit.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                    giverPosition = hit.position;
                    found = true;
                    break;
                }
                Need(found, "no collision-free connected Trimery position is available for the scholar.");

                Directory.CreateDirectory(Path.GetDirectoryName(RecoveryPath));
                Need(EditorSceneManager.SaveScene(scene, RecoveryPath, true), "Unity could not create the pre-edit recovery copy.");
                var rewardFolder = Path.GetDirectoryName(RewardPath).Replace('\\', '/');
                var rewardFolderInfo = new DirectoryInfo(rewardFolder);
                Need(rewardFolderInfo.Exists, "Briarback settings folder is missing.");

                reward = ScriptableObject.CreateInstance<FixedRewardDefinition>();
                reward.name = "The Four Elements Hunt reward";
                reward.gold = 50;
                AssetDatabase.CreateAsset(reward, RewardPath);
                rewardCreated = true;

                quest = ScriptableObject.CreateInstance<QuestDefinition>();
                quest.name = "The Four Elements Hunt";
                quest.stableId = QuestId;
                quest.title = "The Four Elements Hunt";
                quest.version = 1;
                quest.requiredLevel = 1;
                quest.offer = "The Briarback shamans are gathering elemental forces. Defeat one Fire, Earth, Lightning and Ice Shaman, then return to me.";
                quest.activeDialogue = "You have faced all four elements. Return here and I will record what you learned.";
                quest.locationHint = "Find one Fire, Earth, Lightning and Ice Shaman at their settlements in the Briarback region, then return to Trimery's elemental scholar.";
                quest.rewardXp = 400;
                quest.reward = reward;
                quest.completeOnObjectives = false;
                quest.repeatable = false;
                quest.stages = Elements.Select((entry, index) =>
                {
                    var definition = AssetDatabase.LoadAssetAtPath<ActorDefinition>(
                        "Assets/_DiceFree/Settings/Briarback/" + entry.definitionAsset + ".asset");
                    Need(definition != null && definition.stableId.Length > 0,
                        "elemental actor definition is missing: " + entry.definitionAsset);
                    return new QuestObjective
                    {
                        kind = ObjectiveKind.Kill,
                        contentId = definition.stableId,
                        familyId = "briarback.quilboar",
                        count = 1,
                        creditMode = QuestCreditMode.NearbyParty,
                        creditRadius = 50,
                        instruction = entry.killText
                    };
                }).ToArray();
                AssetDatabase.CreateAsset(quest, QuestPath);
                questCreated = true;

                guardians = new GameObject(GuardianGroupName);
                Undo.RegisterCreatedObjectUndo(guardians, "Add Briarback elemental Shaman guardians");
                guardians.transform.SetParent(region, false);
                var prototypes = roles.GetComponentsInChildren<QuilboarCombat>(true);
                var actors = new CombatActor[Elements.Length];
                for (int i = 0; i < Elements.Length; i++)
                {
                    var prototype = prototypes.SingleOrDefault(value => value.role == Elements[i].role);
                    Need(prototype != null, "approved role prototype missing: " + Elements[i].role);
                    var rotation = Quaternion.Euler(0, (i * 90) + 45, 0);
                    var clone = Object.Instantiate(prototype.gameObject, points[i], rotation, guardians.transform);
                    Undo.RegisterCreatedObjectUndo(clone, "Add Briarback elemental guardian");
                    clone.name = Elements[i].role + " guardian - " + (i + 1) + " (Lv 25)";
                    clone.transform.position = points[i];
                    var aggro = clone.GetComponent<AggroBehaviour>();
                    Need(aggro != null, "elemental guardian is missing ordinary aggro.");
                    aggro.Configure(8, 20);
                    aggro.ConfigureAutoAggro(new AutoAggroPolicy { alwaysAutoAggro = true });
                    var respawn = clone.GetComponent<OverworldRespawn>();
                    Need(respawn?.Definition != null, "elemental guardian is missing ordinary respawn setup.");
                    actors[i] = clone.GetComponent<CombatActor>();
                    Need(actors[i] != null, "elemental guardian is missing its combat actor.");
                }

                var giver = Object.Instantiate(template.gameObject, giverPosition, Quaternion.identity, trimery);
                Undo.RegisterCreatedObjectUndo(giver, "Add Trimery elemental scholar");
                giver.name = GiverName;
                foreach (var old in giver.GetComponentsInChildren<InteractionTarget>(true))
                    Undo.DestroyObjectImmediate(old);
                var questGiver = Undo.AddComponent<QuestGiver>(giver);
                questGiver.Configure(quest);
                questGiver.ConfigureName("Trimery Elemental Scholar");
                var approach = new GameObject("Scholar conversation approach").transform;
                Undo.RegisterCreatedObjectUndo(approach.gameObject, "Add scholar conversation approach");
                approach.SetParent(giver.transform, false);
                approach.position = giverPosition + Vector3.forward * 1.6f;
                questGiver.ConfigureApproach(approach);
                Undo.AddComponent<QuestWorldMarker>(giver);

                foreach (var journal in journals)
                {
                    journal.Configure(journal.Definitions.Concat(new[] { quest }).ToArray());
                    EditorUtility.SetDirty(journal);
                }
                EditorUtility.SetDirty(quest);
                EditorUtility.SetDirty(reward);
                EditorUtility.SetDirty(questGiver);
                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the scholar and guardian scene changes.");
                AssetDatabase.SaveAssets();
                Undo.CollapseUndoOperations(undoGroup);
                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENTS_QUEST_AUTHORED",
                    questId = QuestId,
                    sequence = string.Join(" -> ", quest.stages.Select(stage => stage.contentId)),
                    scholar = giver.name,
                    scholarPosition = giverPosition.ToString("F2"),
                    guardians = actors.Length,
                    partyCredit = QuestCreditMode.NearbyParty.ToString(),
                    returnAndTurnInRequired = true,
                    completionIsCharacterDungeonEligibilityFlag = true,
                    rewardTuningPlaceholder = true,
                    rewardXp = quest.rewardXp,
                    rewardGold = reward.gold,
                    recovery = RecoveryPath,
                    navMeshChanged = false
                };
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                if (questCreated) AssetDatabase.DeleteAsset(QuestPath);
                if (rewardCreated) AssetDatabase.DeleteAsset(RewardPath);
                AssetDatabase.Refresh();
                throw;
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.elements-quest.repair",
            "Move the four existing elemental guardians clear of their central altars; verify roster and keep a scene recovery copy.")]
        public static object Repair()
        {
            var scene = RequireScene();
            var points = SitePoints(scene, out var nav);
            try
            {
                var region = All<Transform>(scene).Single(value => value.name == RegionName);
                var root = region.Find(GuardianGroupName);
                Need(root != null, "the existing elemental guardian group is missing.");
                var actors = root.GetComponentsInChildren<QuilboarCombat>(true);
                Need(actors.Length == Elements.Length && Elements.All(entry => actors.Count(c => c.role == entry.role) == 1),
                    "the guardian roster does not match all four approved elements; refusing to replace actors.");
                Need(!AssetDatabase.LoadMainAssetAtPath(RepairRecoveryPath),
                    "the placement recovery copy already exists; inspect it before another repair.");
                Directory.CreateDirectory(Path.GetDirectoryName(RepairRecoveryPath));
                Need(EditorSceneManager.SaveScene(scene, RepairRecoveryPath, true),
                    "Unity could not create the pre-placement recovery copy.");
                for (var i = 0; i < Elements.Length; i++)
                {
                    var guardian = actors.Single(value => value.role == Elements[i].role);
                    Undo.RecordObject(guardian.transform, "Move elemental guardian clear of altar");
                    guardian.transform.position = points[i];
                    EditorUtility.SetDirty(guardian.transform);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "Unity could not save the revised guardian positions.");
                return new
                {
                    marker = "DICEFRE_BRIARBACK_ELEMENTS_GUARDIAN_PLACEMENT_REPAIRED",
                    guardians = Elements.Select((entry, index) => new { entry.role, position = points[index].ToString("F2") }).ToArray(),
                    actorsReplaced = false,
                    navMeshChanged = false,
                    recovery = RepairRecoveryPath
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }
    }
}

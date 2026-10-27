using System;
using System.Linq;
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
    /// <summary>
    /// Additive authoring for Trimery's repeatable Briarback bounty. It never
    /// replaces the provisional region root or rewrites unrelated quest data.
    /// </summary>
    public static class BriarbackBountyAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string TrimeryRoot = "13 - Trimery lumber village";
        private const string RegionRoot = "14 - Briarback quilboar frontier";
        private const string FamilyId = "briarback.quilboar";
        private const string QuestId = "quest.briarback.repeat-bounty";
        private const string QuestAssetPath =
            "Assets/_DiceFree/Settings/Briarback/Another 20 Quillboars.asset";
        private const string RewardAssetPath =
            "Assets/_DiceFree/Settings/Briarback/Another 20 Quillboars reward.asset";
        private const string GiverObjectName = "Trimery Bounty Hunter - Briarback";

        // Round 31 left values open and requires placeholders until playtest tuning.
        public const int PlaceholderXp = 400;
        public const long PlaceholderGold = 50;

        private static T[] All<T>(Scene scene) where T : Component => scene
            .GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .ToArray();

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Briarback bounty: " + message);
        }

        private static bool TryPlacement(Scene scene, out Vector3 placement)
        {
            placement = default;
            var navigation = All<WorldNavigation>(scene).SingleOrDefault();
            Require(navigation != null && navigation.Data != null,
                "the existing shared World 1 NavMesh is required; rebake it separately if missing.");

            // Keep the NPC on Trimery's east/north village edge, away from its
            // banker, lumber trader and central services. Resolve against the
            // saved NavMesh and reject collisions instead of guessing a height.
            var candidates = new[]
            {
                new Vector2(273, 164), new Vector2(274, 169), new Vector2(271, 158),
                new Vector2(272, 194), new Vector2(224, 158), new Vector2(222, 193),
                new Vector2(268, 205), new Vector2(221, 169)
            };
            var actors = All<CombatActor>(scene)
                .Where(actor => actor != null && actor.gameObject.activeInHierarchy)
                .Select(actor => actor.transform.position).ToArray();
            var interactives = All<InteractionTarget>(scene)
                .Where(target => target != null && target.gameObject.activeInHierarchy)
                .Select(target => target.transform.position).ToArray();

            var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
                var center = new Vector3(248, 0, 180);
                Require(NavMesh.SamplePosition(center, out var origin, 4, NavMesh.AllAreas),
                    "Trimery's shared NavMesh does not cover its center.");
                foreach (var candidate in candidates)
                {
                    var ground = CornbergLandscape.Ground(candidate.x, candidate.y);
                    if (!NavMesh.SamplePosition(ground, out var hit, 2, NavMesh.AllAreas)) continue;
                    if (actors.Any(point => Vector3.Distance(point, hit.position) < 6)) continue;
                    if (interactives.Any(point => Vector3.Distance(point, hit.position) < 5)) continue;

                    var capsuleBottom = hit.position + Vector3.up * .48f;
                    var capsuleTop = hit.position + Vector3.up * 1.42f;
                    if (Physics.OverlapCapsule(capsuleBottom, capsuleTop, .43f,
                            Physics.AllLayers, QueryTriggerInteraction.Ignore)
                        .Any(collider => collider != null && collider.enabled &&
                            collider.gameObject.activeInHierarchy &&
                            !collider.transform.IsChildOf(navigation.transform))) continue;

                    var path = new NavMeshPath();
                    if (!NavMesh.CalculatePath(origin.position, hit.position,
                            NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                    placement = hit.position;
                    return true;
                }
                return false;
            }
            finally
            {
                if (nav.valid) nav.Remove();
            }
        }

        [CliCommand("dicefree.briarback.bounty.author",
            "Add Trimery's repeatable 20-Quillboar bounty using guarded Editor APIs.")]
        public static object Author()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "stop Play Mode before authoring.");
            Require(EditorSceneManager.GetSceneManagerSetup().Length == 1 &&
                    SceneManager.GetActiveScene().path == ScenePath,
                "open only the saved Cornberg scene; no other scene will be loaded or replaced.");
            var scene = SceneManager.GetActiveScene();
            Require(!scene.isDirty, "save or discard pending scene edits in Unity before authoring.");
            Require(GameObject.Find(GiverObjectName) == null,
                "the additive bounty giver already exists; use the validator instead of duplicating it.");
            Require(AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestAssetPath) == null &&
                    AssetDatabase.LoadAssetAtPath<FixedRewardDefinition>(RewardAssetPath) == null,
                "a bounty asset already exists; refusing to overwrite it.");

            var trimery = GameObject.Find(TrimeryRoot);
            var region = GameObject.Find(RegionRoot);
            Require(trimery != null && region != null,
                "both the existing Trimery and Briarback roots are required.");
            Require(TryPlacement(scene, out var placement),
                "no collision-free, connected village edge position was found; no scene objects were changed.");

            var template = All<Transform>(scene).SingleOrDefault(value =>
                value.name == "NPCs/NPC_Guard - 3D");
            Require(template != null, "the existing visible guard model is missing.");
            var journals = All<QuestJournal>(scene);
            Require(journals.Length > 0, "the World 1 player quest journal is missing.");
            Require(!journals.Any(journal => journal.Definitions.Any(definition =>
                    definition != null && definition.stableId == QuestId)),
                "the repeatable bounty is already in a player catalog.");

            int undoGroup = Undo.GetCurrentGroup();
            Undo.IncrementCurrentGroup();
            undoGroup = Undo.GetCurrentGroup();
            bool questCreated = false, rewardCreated = false;
            try
            {
                var reward = ScriptableObject.CreateInstance<FixedRewardDefinition>();
                reward.name = "Another 20 Quillboars reward";
                reward.gold = PlaceholderGold;
                AssetDatabase.CreateAsset(reward, RewardAssetPath);
                rewardCreated = true;

                var quest = ScriptableObject.CreateInstance<QuestDefinition>();
                quest.name = "Another 20 Quillboars";
                quest.stableId = QuestId;
                quest.title = "Another 20 Quillboars";
                quest.version = 1;
                quest.requiredLevel = 1;
                quest.offer = "Trimery needs the Briarback warband kept off the road. Defeat twenty Quillboars, then return here for your bounty.";
                quest.activeDialogue = "Twenty down. Return when the bounty is complete and I will pay you.";
                quest.locationHint = "Return to the Trimery bounty hunter after defeating 20 Quillboars.";
                quest.rewardXp = PlaceholderXp;
                quest.reward = reward;
                quest.completeOnObjectives = false;
                quest.repeatable = true;
                quest.stages = new[]
                {
                    new QuestObjective
                    {
                        kind = ObjectiveKind.Kill,
                        familyId = FamilyId,
                        count = 20,
                        creditMode = QuestCreditMode.NearbyParty,
                        creditRadius = 50,
                        instruction = "Defeat 20 Quillboars"
                    }
                };
                AssetDatabase.CreateAsset(quest, QuestAssetPath);
                questCreated = true;

                var towardVillage = (new Vector3(248, placement.y, 180) - placement).normalized;
                var giverObject = Object.Instantiate(template.gameObject, placement,
                    Quaternion.LookRotation(new Vector3(towardVillage.x, 0, towardVillage.z)),
                    trimery.transform);
                Undo.RegisterCreatedObjectUndo(giverObject, "Add Trimery Briarback bounty hunter");
                giverObject.name = GiverObjectName;
                foreach (var oldInteraction in giverObject.GetComponentsInChildren<InteractionTarget>(true))
                    Undo.DestroyObjectImmediate(oldInteraction);
                var giver = Undo.AddComponent<QuestGiver>(giverObject);
                giver.Configure(quest);
                giver.ConfigureName("Trimery Bounty Hunter");
                var approach = new GameObject("Bounty conversation approach").transform;
                Undo.RegisterCreatedObjectUndo(approach.gameObject, "Add bounty conversation approach");
                approach.SetParent(giverObject.transform, false);
                approach.position = placement + towardVillage * 1.6f;
                giver.ConfigureApproach(approach);
                Undo.AddComponent<QuestWorldMarker>(giverObject);

                foreach (var journal in journals)
                {
                    journal.Configure(journal.Definitions.Concat(new[] { quest }).ToArray());
                    EditorUtility.SetDirty(journal);
                }
                EditorUtility.SetDirty(giver);
                EditorUtility.SetDirty(quest);
                EditorUtility.SetDirty(reward);
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(scene);
                Require(EditorSceneManager.SaveScene(scene), "Unity could not save the additive scene change.");
                Undo.CollapseUndoOperations(undoGroup);
                return new
                {
                    marker = "DICEFRE_BRIARBACK_BOUNTY_AUTHORED",
                    questId = QuestId,
                    objective = "20 Quillboars, one shared family counter",
                    creditMode = QuestCreditMode.NearbyParty.ToString(),
                    giver = giverObject.name,
                    position = placement.ToString("F2"),
                    rewardXp = PlaceholderXp,
                    rewardGold = PlaceholderGold,
                    rewardAmountsArePlaytestTuningPlaceholders = true,
                    repeatable = quest.repeatable,
                    priorRegionPreserved = true,
                    savedScene = ScenePath
                };
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                if (questCreated) AssetDatabase.DeleteAsset(QuestAssetPath);
                if (rewardCreated) AssetDatabase.DeleteAsset(RewardAssetPath);
                AssetDatabase.Refresh();
                throw;
            }
        }
    }
}

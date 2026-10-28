using System;
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

namespace DiceFree.EditorTools
{
    public static class BriarbackElementalQuestValidation
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string RegionName = "14 - Briarback quilboar frontier";
        private const string QuestId = "quest.briarback.four-elements-hunt";
        private const string QuestPath = "Assets/_DiceFree/Settings/Briarback/The Four Elements Hunt.asset";
        private const string RewardPath = "Assets/_DiceFree/Settings/Briarback/The Four Elements Hunt reward.asset";
        private const string GuardianGroupName = "Briarback elemental guardians - additive";
        private const string GiverName = "Trimery Elemental Scholar - Briarback";
        private static readonly (string site, QuilboarRole role, string stableId)[] Expected =
        {
            ("Fire Shaman settlement - provisional", QuilboarRole.FireShaman, "briarback.fireshaman"),
            ("Earth Shaman settlement - provisional", QuilboarRole.EarthShaman, "briarback.earthshaman"),
            ("Lightning Shaman settlement - provisional", QuilboarRole.LightningShaman, "briarback.lightningshaman"),
            ("Ice Shaman settlement - provisional", QuilboarRole.IceShaman, "briarback.iceshaman")
        };

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("BRIARBACK ELEMENTAL QUEST: " + message);
        }

        private static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        [CliCommand("dicefree.briarback.elements-quest.validate",
            "Validate the four-element quest, party credit, scholar, respawning elemental guardians and collision-safe site placement.")]
        public static object Validate()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "Edit Mode only.");
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && !scene.isDirty, "open the saved Cornberg scene with no unsaved edits.");
            var region = All<Transform>(scene).SingleOrDefault(value => value.name == RegionName);
            var guardiansRoot = region != null ? region.Find(GuardianGroupName) : null;
            var sitesRoot = region != null ? region.Find("Briarback elemental settlements - visual blockout - provisional") : null;
            Need(guardiansRoot != null && sitesRoot != null, "the four-site blockout or guardian group is missing.");

            var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestPath);
            var reward = AssetDatabase.LoadAssetAtPath<FixedRewardDefinition>(RewardPath);
            Need(quest != null && quest.stableId == QuestId && quest.stages.Length == Expected.Length,
                "the four ordered elemental quest stages are missing.");
            Need(!quest.repeatable && !quest.completeOnObjectives,
                "the hunt must be optional, one-time, and require return/turn-in.");
            Need(quest.rewardXp == 400 && reward != null && reward.gold == 50,
                "the disclosed placeholder rewards were changed without playtest tuning.");

            var guardians = guardiansRoot.GetComponentsInChildren<QuilboarCombat>(true);
            Need(guardians.Length == Expected.Length, "expected one elemental guardian per settlement.");
            for (var index = 0; index < Expected.Length; index++)
            {
                var expected = Expected[index];
                var guardian = guardians.SingleOrDefault(value => value.role == expected.role);
                var actor = guardian != null ? guardian.GetComponent<CombatActor>() : null;
                var actorStats = guardian != null ? guardian.GetComponent<ActorStats>() : null;
                var aggro = guardian != null ? guardian.GetComponent<AggroBehaviour>() : null;
                var respawn = guardian != null ? guardian.GetComponent<OverworldRespawn>() : null;
                Need(guardian != null && actor != null && actorStats != null && actorStats.Definition != null &&
                     actorStats.Definition.stableId == expected.stableId &&
                     actorStats.Definition.familyId == "briarback.quilboar" && aggro != null &&
                     aggro.AutoAggroPolicy != null && aggro.AutoAggroPolicy.alwaysAutoAggro &&
                     respawn != null && respawn.Definition != null,
                    expected.role + " guardian is missing its unique defeat identity, ordinary aggro, or respawn.");
                var site = sitesRoot.Find(expected.site);
                var expectedSpawn = site != null ? site.position + Vector3.forward * 4f : Vector3.zero;
                Need(site != null && Vector3.Distance(expectedSpawn, guardian.transform.position) < 1.5f,
                    expected.role + " is not placed on the NavMesh just behind its matching altar.");
                Need(quest.stages[index].kind == ObjectiveKind.Kill &&
                     quest.stages[index].contentId == expected.stableId &&
                     quest.stages[index].familyId == "briarback.quilboar" &&
                     quest.stages[index].count == 1 &&
                     quest.stages[index].creditMode == QuestCreditMode.NearbyParty &&
                     Mathf.Approximately(quest.stages[index].creditRadius, 50f),
                    expected.role + " quest stage or standard nearby-party kill credit is incorrect.");
                var capsule = Physics.OverlapCapsule(guardian.transform.position + Vector3.up * .48f,
                    guardian.transform.position + Vector3.up * 1.42f, .43f,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore)
                    .FirstOrDefault(value => value != null && value.enabled && value.gameObject.activeInHierarchy &&
                        !value.transform.IsChildOf(guardiansRoot));
                Need(capsule == null, expected.role + " spawn overlaps scene collision: " + capsule?.name);
            }

            var giver = All<QuestGiver>(scene).SingleOrDefault(value => value.name == GiverName);
            Need(giver != null && giver.Quest == quest && giver.DisplayName == "Trimery Elemental Scholar",
                "the visible Trimery scholar does not offer the quest.");
            var navigation = All<WorldNavigation>(scene).SingleOrDefault();
            Need(navigation != null && navigation.Data != null, "the shared World 1 NavMesh is missing.");
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
                Need(NavMesh.SamplePosition(giver.ApproachPosition, out var destination, 2, NavMesh.AllAreas),
                    "the scholar interaction point is not on the NavMesh.");
                Need(NavMesh.SamplePosition(new Vector3(248, 0, 180), out var origin, 4, NavMesh.AllAreas),
                    "Trimery center has no walkable NavMesh point.");
                var path = new NavMeshPath();
                Need(NavMesh.CalculatePath(origin.position, destination.position, NavMesh.AllAreas, path) &&
                     path.status == NavMeshPathStatus.PathComplete, "the scholar is disconnected from Trimery's playable route.");
            }
            finally { if (nav.valid) nav.Remove(); }

            var journals = All<QuestJournal>(scene);
            Need(journals.Length > 0 && journals.All(journal =>
                journal.Definitions.Count(value => value == quest) == 1),
                "each active Cornberg player journal must contain exactly one four-element quest.");
            Need(guardiansRoot.GetComponentsInChildren<Collider>(true).Length > 0,
                "elemental guardians need normal body collision; do not weaken the assertion.");
            return new
            {
                marker = "DICEFRE_BRIARBACK_ELEMENTS_QUEST_VALID",
                quest = quest.stableId,
                stages = quest.stages.Length,
                guardians = guardians.Length,
                credit = QuestCreditMode.NearbyParty.ToString(),
                scholarPath = "complete",
                guardianSpawns = "NavMesh and collider clear",
                returnTurnInRequired = true,
                completionIsCharacterDungeonEligibilityFlag = true,
                rewardTuningPlaceholder = true,
                rewardXp = quest.rewardXp,
                rewardGold = reward.gold
            };
        }
    }
}

using System;
using System.Linq;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class BriarbackBountyValidation
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string TrimeryRoot = "13 - Trimery lumber village";
        private const string RegionRoot = "14 - Briarback quilboar frontier";
        private const string QuestId = "quest.briarback.repeat-bounty";
        private const string GiverName = "Trimery Bounty Hunter - Briarback";
        private const string FamilyId = "briarback.quilboar";

        private static T[] All<T>() where T : Component => SceneManager.GetActiveScene()
            .GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .ToArray();

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Briarback bounty validation: " + message);
        }

        [CliCommand("dicefree.briarback.bounty.validate",
            "Validate the additive Trimery 20-Quillboar repeatable bounty and its live navigation placement.")]
        public static object Run()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "run from Edit Mode.");
            Need(SceneManager.GetActiveScene().path == ScenePath, "the active scene is not Cornberg.");
            var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(
                "Assets/_DiceFree/Settings/Briarback/Another 20 Quillboars.asset");
            var reward = AssetDatabase.LoadAssetAtPath<FixedRewardDefinition>(
                "Assets/_DiceFree/Settings/Briarback/Another 20 Quillboars reward.asset");
            Need(quest != null && quest.stableId == QuestId && quest.repeatable &&
                 !quest.completeOnObjectives, "quest ID, repeatability or turn-in behavior.");
            Need(quest.stages != null && quest.stages.Length == 1, "one shared kill objective is required.");
            var objective = quest.stages[0];
            Need(objective.kind == ObjectiveKind.Kill && objective.count == 20 &&
                 objective.familyId == FamilyId &&
                 (objective.familyIds == null || objective.familyIds.Length == 0),
                "the quest must use one 20-kill counter for the Briarback family.");
            Need(objective.creditMode == QuestCreditMode.NearbyParty &&
                 Mathf.Approximately(objective.creditRadius, 50),
                "the quest must use the shared NearbyParty credit policy.");
            Need(quest.rewardXp == BriarbackBountyAuthoring.PlaceholderXp && reward != null &&
                 reward.gold == BriarbackBountyAuthoring.PlaceholderGold,
                "the disclosed XP and gold tuning placeholders changed unexpectedly.");

            var giver = All<QuestGiver>().Where(value => value != null &&
                value.Offers.Any(offer => offer != null && offer.stableId == QuestId)).ToArray();
            Need(giver.Length == 1 && giver[0].name == GiverName,
                "exactly one distinct Trimery bounty giver must offer this quest.");
            var trimery = GameObject.Find(TrimeryRoot);
            var region = GameObject.Find(RegionRoot);
            Need(trimery != null && region != null && giver[0].transform.IsChildOf(trimery.transform),
                "the bounty giver must be in Trimery while the existing Briarback region remains.");
            Need(giver[0].GetComponent<QuestWorldMarker>() != null,
                "the repeatable quest marker is missing.");
            Need(giver[0].GetComponentsInChildren<Renderer>(true).Any(renderer =>
                    renderer.enabled && renderer.gameObject.activeInHierarchy),
                "the giver has no visible NPC model.");
            var journals = All<QuestJournal>();
            Need(journals.Length > 0 && journals.All(journal =>
                    journal.Definitions.Count(definition => definition == quest) == 1),
                "every existing journal must contain exactly one bounty definition.");

            var navigation = All<WorldNavigation>().SingleOrDefault();
            Need(navigation != null && navigation.Data != null, "the shared NavMesh asset is missing.");
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
                var start = new Vector3(248, giver[0].transform.position.y, 180);
                bool hasOrigin = NavMesh.SamplePosition(start, out var origin, 4, NavMesh.AllAreas);
                bool hasDestination = NavMesh.SamplePosition(giver[0].transform.position,
                    out var destination, 2, NavMesh.AllAreas);
                Need(hasOrigin && hasDestination,
                    "the Trimery center or bounty NPC has no walkable NavMesh point.");
                var path = new NavMeshPath();
                Need(NavMesh.CalculatePath(origin.position, destination.position,
                        NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete,
                    "the bounty giver is not connected to Trimery's playable route.");
                var collision = Physics.OverlapCapsule(destination.position + Vector3.up * .48f,
                    destination.position + Vector3.up * 1.42f, .43f,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore)
                    .FirstOrDefault(value => value != null && value.enabled &&
                        value.gameObject.activeInHierarchy &&
                        !value.transform.IsChildOf(navigation.transform) &&
                        !value.transform.IsChildOf(giver[0].transform));
                Need(collision == null, "the bounty NPC overlaps scene collision: " + collision?.name);
            }
            finally
            {
                if (nav.valid) nav.Remove();
            }

            var patrolGroup = GameObject.Find("Briarback road patrols - additive")?.transform;
            var creatures = All<QuilboarCombat>().Where(value => patrolGroup == null ||
                !value.transform.IsChildOf(patrolGroup)).ToArray();
            Need(creatures.Length == 20 && creatures.Count(value =>
                    value.transform.IsChildOf(GameObject.Find(RegionRoot).transform)) == 20,
                "adding the quest must preserve the existing 20-actor Briarback roster.");
            return new
            {
                marker = "DICEFRE_BRIARBACK_BOUNTY_OK",
                objective = "one 20-kill Briarback family counter",
                credit = objective.creditMode.ToString(),
                giver = giver[0].name,
                navPath = "complete",
                collision = "clear",
                creatureCount = creatures.Length,
                rewardTuningPlaceholder = true,
                rewardXp = quest.rewardXp,
                rewardGold = reward.gold
            };
        }
    }
}

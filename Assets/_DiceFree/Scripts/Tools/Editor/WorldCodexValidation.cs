using System;
using System.Linq;
using DiceFree.Gameplay;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class WorldCodexValidation
    {
        public const string SlimeDungeonAssetPath =
            "Assets/_DiceFree/Settings/World/Dungeon - Slime.asset";

        [MenuItem("DiceFree/World/Author Slime Dungeon Definition")]
        [CliCommand(
            "dicefree.world.author-slime-dungeon",
            "Author the first Slime Dungeon definition and its conditional Regent route.",
            Tags = new[] { "world", "dungeon", "authoring" })]
        public static object AuthorSlimeDungeon()
        {
            const string folder = "Assets/_DiceFree/Settings/World";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_DiceFree/Settings"))
                    throw new InvalidOperationException("DiceFree Settings folder is missing.");
                AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "World");
            }

            var definition = AssetDatabase.LoadAssetAtPath<DungeonDefinition>(SlimeDungeonAssetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<DungeonDefinition>();
                AssetDatabase.CreateAsset(definition, SlimeDungeonAssetPath);
            }

            definition.stableId = "dungeon.slime";
            definition.sceneId = "SlimeDungeon";
            definition.stagingSeconds = 60f;
            definition.variants = new[]
            {
                new DungeonVariantRule
                {
                    stableId = "variant.slime-regent.level50",
                    priority = 100,
                    allConditions = new[]
                    {
                        new DungeonConditionSpec
                        {
                            kind = DungeonConditionKind.AveragePartyLevelAtLeast,
                            threshold = 50f
                        }
                    },
                    enableRouteIds = new[] { "route.slime-regent" },
                    enableBossIds = new[] { "boss.slime-regent" },
                    lootTableIds = new[] { "loot.slime-regent" },
                    variantTags = new[] { "secret.slime-regent" }
                }
            };

            definition.Validate();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();

            return new
            {
                success = true,
                finalMarker = "DICEFREE_SLIME_DUNGEON_DEFINITION_OK",
                dungeonId = definition.stableId,
                variant = definition.variants[0].stableId
            };
        }

        [CliCommand(
            "dicefree.world-codex.validate",
            "Validate session map exclusivity, dungeon variants and Codex discovery state.",
            Tags = new[] { "tests", "world", "dungeon", "codex" })]
        public static object Validate()
        {
            ValidateMapRegistry();
            ValidateConditions();
            ValidateCodex();

            Debug.Log(
                "DICEFREE_WORLD_CODEX_OK: concurrent world faces, single-occupancy dungeons, conditional variants and Echo Codex semantics validated.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_WORLD_CODEX_OK"
            };
        }

        private static void ValidateMapRegistry()
        {
            var registry = new SessionMapRegistry();
            DateTime now = new(2026, 10, 7, 20, 0, 0, DateTimeKind.Utc);

            registry.PlaceOnWorldFace("player.a", "world.face.1");
            registry.PlaceOnWorldFace("player.b", "world.face.4");
            Require(registry.LocationOf("player.a").mapId == "world.face.1",
                "Player A world-face location was not preserved.");
            Require(registry.LocationOf("player.b").mapId == "world.face.4",
                "Player B world-face location was not preserved.");

            Require(registry.TryClaimDungeonStaging(
                    "dungeon.slime",
                    "party.a",
                    "player.a",
                    now,
                    out var slime,
                    out var error),
                "First Slime Dungeon claimant should succeed: " + error);

            Require(!registry.TryClaimDungeonStaging(
                    "dungeon.slime",
                    "party.b",
                    "player.b",
                    now,
                    out _,
                    out _),
                "A second party must not receive another Slime Dungeon occupancy.");

            Require(registry.TryClaimDungeonStaging(
                    "dungeon.cave",
                    "party.b",
                    "player.b",
                    now,
                    out var cave,
                    out error),
                "A different dungeon should be occupiable simultaneously: " + error);
            Require(registry.OccupiedDungeonIds.Length == 2,
                "Different dungeon IDs should coexist.");

            Require(registry.TryClaimDungeonStaging(
                    "dungeon.slime",
                    "party.a",
                    "player.c",
                    now.AddSeconds(3),
                    out slime,
                    out error),
                "Same party should be able to join Slime staging: " + error);
            Require(slime.participants.SequenceEqual(new[] { "player.a", "player.c" }),
                "Slime staging roster is not stable/sorted.");

            var lowVariant = new DungeonRunVariantSnapshot
            {
                dungeonId = "dungeon.slime"
            };
            Require(registry.TryBeginDungeon(
                    "dungeon.slime",
                    slime.leaseToken,
                    lowVariant,
                    now.AddSeconds(60),
                    out var active,
                    out error),
                "Slime staging should transition to active: " + error);
            Require(active.stage == DungeonLifecycleStage.Active,
                "Slime Dungeon did not enter active state.");

            Require(!registry.TryClaimDungeonStaging(
                    "dungeon.slime",
                    "party.a",
                    "player.d",
                    now.AddSeconds(61),
                    out _,
                    out _),
                "Late participant joined an active dungeon.");

            Require(registry.ReleaseDungeon(
                    "dungeon.slime",
                    active.leaseToken,
                    "world.face.1",
                    out error),
                "Slime Dungeon release failed: " + error);
            Require(registry.Dungeon("dungeon.slime") == null,
                "Released dungeon lock remained occupied.");

            Require(registry.TryClaimDungeonStaging(
                    "dungeon.slime",
                    "party.b",
                    "player.b",
                    now.AddSeconds(70),
                    out var secondRun,
                    out error),
                "A released dungeon should become claimable again: " + error);
            Require(secondRun.leaseToken != active.leaseToken,
                "A new dungeon occupancy reused the previous lease token.");

            Require(registry.TryClaimDungeonStaging(
                    "dungeon.stale",
                    "party.stale",
                    "player.stale",
                    now.AddMinutes(-10),
                    out _,
                    out error),
                "Stale test dungeon could not be claimed: " + error);
            int recovered = registry.RecoverStaleDungeonLocks(
                now,
                TimeSpan.FromMinutes(5),
                "world.face.1");
            Require(recovered == 1 && registry.Dungeon("dungeon.stale") == null,
                "Stale dungeon lock recovery did not release exactly the stale lock.");

            // Cave remains occupied throughout; Slime/other IDs never steal its lock.
            Require(registry.Dungeon("dungeon.cave")?.leaseToken == cave.leaseToken,
                "Concurrent different-dungeon occupancy was disturbed.");
        }

        private static void ValidateConditions()
        {
            var definition = ScriptableObject.CreateInstance<DungeonDefinition>();
            try
            {
                definition.stableId = "dungeon.slime";
                definition.sceneId = "SlimeDungeon";
                definition.variants = new[]
                {
                    new DungeonVariantRule
                    {
                        stableId = "variant.slime-regent.level50",
                        priority = 100,
                        allConditions = new[]
                        {
                            new DungeonConditionSpec
                            {
                                kind = DungeonConditionKind.AveragePartyLevelAtLeast,
                                threshold = 50f
                            }
                        },
                        enableRouteIds = new[] { "route.slime-regent" },
                        enableBossIds = new[] { "boss.slime-regent" },
                        lootTableIds = new[] { "loot.slime-regent" },
                        variantTags = new[] { "secret.slime-regent" }
                    }
                };

                var low = new DungeonConditionContext
                {
                    participants = new[]
                    {
                        new DungeonParticipantSnapshot { participantId = "a", level = 40 },
                        new DungeonParticipantSnapshot { participantId = "b", level = 59 }
                    }
                };
                var lowSnapshot = DungeonConditionEvaluator.Evaluate(definition, low);
                Require(Math.Abs(lowSnapshot.averagePartyLevel - 49.5f) < 0.001f,
                    "Average party level calculation is wrong.");
                Require(!lowSnapshot.HasRoute("route.slime-regent"),
                    "Regent route opened below average level 50.");

                var high = new DungeonConditionContext
                {
                    participants = new[]
                    {
                        new DungeonParticipantSnapshot { participantId = "a", level = 40 },
                        new DungeonParticipantSnapshot { participantId = "b", level = 60 }
                    }
                };
                var highSnapshot = DungeonConditionEvaluator.Evaluate(definition, high);
                Require(Math.Abs(highSnapshot.averagePartyLevel - 50f) < 0.001f,
                    "Level-50 threshold context is wrong.");
                Require(highSnapshot.HasRoute("route.slime-regent"),
                    "Regent route did not open at average level 50.");
                Require(highSnapshot.HasBoss("boss.slime-regent"),
                    "Regent boss was not enabled with the route.");
                Require(highSnapshot.lootTableIds.Contains("loot.slime-regent"),
                    "Regent loot variant was not snapshotted.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        private static void ValidateCodex()
        {
            var go = new GameObject("Codex validation source");
            var clone = new GameObject("Codex validation restored");
            try
            {
                var codex = go.AddComponent<CodexProgression>();
                var hub = new SemanticGameEventHub();
                codex.Bind(hub);

                hub.Publish(new MonsterKilledEvent("monster.slime.green"));
                hub.Publish(new MonsterKilledEvent("monster.slime.green"));
                hub.Publish(new DropObservedEvent("monster.slime.green", "item.slime.gel"));
                hub.Publish(new DropObservedEvent("monster.slime.green", "item.slime.gel"));
                hub.Publish(new BossMechanicWitnessedEvent("boss.slime", "mechanic.slime-slam"));
                hub.Publish(new BossMechanicWitnessedEvent("boss.slime", "mechanic.slime-slam"));

                hub.Publish(new DungeonWipedEvent("dungeon.slime"));
                hub.Publish(new DungeonPlayerDiedEvent("dungeon.slime"));
                hub.Publish(new DungeonBossKilledEvent("dungeon.slime", "boss.slime"));
                hub.Publish(new DungeonSecretDiscoveredEvent(
                    "dungeon.slime",
                    "secret.slime-regent"));
                hub.Publish(new DropObservedEvent(
                    "dungeon.slime",
                    "item.slime-crown-fragment",
                    true));
                hub.Publish(new DungeonCompletedEvent(
                    "dungeon.slime",
                    "normal",
                    1,
                    123.4f,
                    true));
                hub.Publish(new DungeonCompletedEvent(
                    "dungeon.slime",
                    "normal",
                    2,
                    99f,
                    false));

                var monsters = codex.Monsters;
                Require(monsters.Length == 2,
                    "Boss mechanic discovery should create its exact boss entry alongside the slime.");
                var slime = monsters.Single(value => value.monsterId == "monster.slime.green");
                Require(slime.kills == 2, "Monster kill count is wrong.");
                Require(slime.drops.Single().timesSeen == 2,
                    "Drop times-seen count is wrong.");
                var boss = monsters.Single(value => value.monsterId == "boss.slime");
                Require(boss.mechanics.Length == 1,
                    "Mechanic discovery was not idempotent.");

                var dungeon = codex.Dungeons.Single();
                Require(dungeon.clears == 2 &&
                        dungeon.wipes == 1 &&
                        dungeon.playerDeaths == 1 &&
                        dungeon.soloClears == 1 &&
                        dungeon.noDeathClears == 1,
                    "Dungeon counters are wrong.");
                Require(dungeon.fastestClears.Length == 2 &&
                        dungeon.fastestClears.Any(value =>
                            value.partySize == 1 && Math.Abs(value.seconds - 123.4f) < 0.01f) &&
                        dungeon.fastestClears.Any(value =>
                            value.partySize == 2 && Math.Abs(value.seconds - 99f) < 0.01f),
                    "Fastest clear was not separated by party size.");
                Require(dungeon.secretDiscoveries.Contains("secret.slime-regent"),
                    "Dungeon secret discovery is missing.");
                Require(dungeon.lootDiscoveries.Contains("item.slime-crown-fragment"),
                    "Dungeon loot discovery is missing.");

                string json = codex.CaptureJson();
                var restored = clone.AddComponent<CodexProgression>();
                restored.RestoreJson(CodexProgression.CodexVersion, json);
                Require(restored.Monsters.Single(value =>
                            value.monsterId == "monster.slime.green").kills == 2,
                    "Codex monster state did not round-trip.");
                Require(restored.Dungeons.Single().clears == 2,
                    "Codex dungeon state did not round-trip.");
                Require(restored.SectionId == "echo:codex",
                    "Codex does not use the expected Echo-wide section.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(clone);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}

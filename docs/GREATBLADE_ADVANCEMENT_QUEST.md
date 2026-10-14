# Greatblade advancement quest — 2026-10-14

The retired swordswoman in Cornberg offers **The Weight of a Greatblade** to a **level-30 or higher Physically Blessed Novice**.

1. Accept the quest from her.
2. Defeat the **normal Slime Boss** in the Slime Dungeon.
3. Return to her and choose **Complete training and become a level-1 Greatblade**.

Earlier kills, the miniboss and Slime Regent do not count. Killing a normal-boss actor outside the dungeon does not count. The boss defeat makes the quest ReadyToTurnIn; it does not advance the class remotely. Progress survives saving and manifestation switching. Existing Greatblade manifestations remain usable.

Quest identity: quest.greatblade.slime-trial. Objective matches boss.slime through a dungeon-only kill objective. Required source class: class.physically-blessed-novice. Required level: 30. No extra XP/item reward is attached to the training quest; normal dungeon rewards are unchanged.

## Integration

- QuestDefinition adds optional class/level acceptance gates; existing quests remain unrestricted unless authored with those fields.
- QuestLeaf adds opt-in dungeonBossOnly. Ordinary defeat reports cannot satisfy that objective.
- SlimeDungeonRun binds each participating player's QuestJournal to its scoped DungeonBossKilledEvent stream, alongside the existing Codex stream. Returning/leaving unsubscribes. This does not attach ordinary defeat reporters to dungeon enemies or duplicate XP/drop rewards.
- AdvancementDefinition now has optional requiredQuestId; ordinary advancement requires the prerequisite quest Completed.
- GreatbladeMentor verifies local interaction and sends ready quest plus matching advancement edge through AdvancementController.
- ManifestationPersistence creates a candidate profile, completes the parent quest record, creates the level-1 child, validates both and writes them in one commit. The source level/XP, gear and progression snapshot remain available. Invalid/incomplete/duplicate turn-in does not create another manifestation.
- Development Room has an explicit development-only advancement bypass so class combat testing remains possible without doing the story quest. The normal game path stays quest-gated.
- The mentor dialog blocks pointer movement underneath it and keeps pending farmer quest acceptance/turn-in accessible.

## Evidence

DICEFREE_GREATBLADE_QUEST_OK: **35 live assertions passed** with an isolated Echo.

Checks include wrong-class/under-level eligibility, missing-quest advancement rejection, rejected incomplete turn-in without a durable mutation, pre-acceptance boss events, miniboss/Regent rejection, outside-dungeon boss actor rejection, accepted/ready progress restoration, a real dungeon miniboss and normal boss death, no advancement before mentor return, and one-commit completion with parent and child records verified from the actual on-disk save.

The full **75-check Greatblade combat suite** and ten existing data/architecture/migration/presentation validators passed again after the quest integration.

Physical model candidates were reviewed separately; no Physical player model replacement is included in this change. See PHYSICAL_MODEL_CANDIDATES_2026-10-14.md.

Final Windows Development build: seven scenes, 317,925,848 reported bytes, zero errors and zero warnings.

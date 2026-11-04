# Chieftain's Bane and early-class player motion — 2026-11-04

This milestone implements agenda items 1 and 2 on `feature/elder-quest-player-presentation`, based on `4831460`. The original 54-file working-tree snapshot is preserved separately in `Logs/ElderPlayerInitialWipHashes.json`; unrelated class/skill/UI/settings work must remain untouched and unstaged.

## Elder quest and honorific

Trimery's village elder fears a tribal invasion. The elder offers the optional **Chieftain's Bane** quest at `(268, -0.10, 170)`, with a connected conversation approach. Accept the quest, defeat the level-40 Chieftain in the northern stronghold, and return to the elder to claim XP, gold and the cosmetic title **Chieftain's Bane**. The city route remains independent of this optional hunt.

The objective matches `briarback.chieftain` specifically and uses the existing 50m NearbyParty credit policy. Neither ordinary Quillboars nor a kill before acceptance completes the objective. Boss death makes the quest ready for turn-in; it does not grant the title. Existing `FixedRewardGrant` handles the completion/reward transaction and blocks duplicate rewards. The existing bounty constants supply **400 XP / 50 gold as tuning placeholders**, not approved final boss-quest economics. Boss gear/drop tables and combat tuning are unchanged.

The new optional `QuestDefinition.rewardTitle` field supplies reward text and the honorific. `QuestHonorifics` derives earned titles from matching, version-supported **Completed** quest records. It reads the small `Resources/HonorificQuests` catalog so the title remains available across scenes even when a scene lacks the original giver. Existing saved quest records carry the award; no save-schema change or independent title counter is introduced. Available, active, ready-to-turn-in, unknown-version and unrelated-character records do not confer it. Live labels and Start Menu entries display `<Echo> the Chieftain's Bane`; characters without it retain their class title. Manifestation forks retain quest history under the existing snapshot rules.

`tools/authoring/ChieftainsBaneAuthoring.cs` records guarded additive scene/asset authoring and creates a local pre-edit recovery scene. The elder is a distinct giver based on the existing town NPC presentation; no new named character or separate dungeon-unlock conversation is invented.

The first built-player smoke exposed an inherited prefab component returning after scene reload: the elder had both the new quest and the scholar's old QuestGiver. The corrected authoring unpacks only the new elder instance before removing inherited interactions. The repair was verified after a saved scene reload, and the focused quest review now requires exactly one elder interaction. Earlier diagnostic builds/profiles are preserved and are not acceptance artifacts.

## Three player motion sets

`Resources/PlayerMotion` holds separate Novice, Physical and Magical Humanoid controllers, each with ten editable clips: Idle, Locomotion, Attack, ArmedAttack, Cast, Heavy, Guard, Heal, Hit and Death. They retime the project's current Novice Humanoid source clips and author class-specific arm/chest poses. Novice posture stays relaxed; Physical holds a more guarded stance; Magical has its own casting/support gesture and existing staff presentation.

`PlayerMotionPresentation` is installed by the existing player presentation driver. It drives only these three class IDs and leaves later class presentations on their existing paths. Basic-attack playback follows the actual wind-up and attack speed. Existing successful skill events select casting, heavy-strike, guard or healing gestures; no skill-caster WIP is edited. Movement can cancel cosmetic recovery, stun pauses motion, and health restoration clears the death state. Root motion stays disabled; no input, collision, NavMesh, damage, cooldown or resource authority moves into the presentation layer.

The existing whole-body squash effect is suppressed for these three animated forms so it does not distort their skeletal wind-ups/death poses. Original leg curves are retained for standing, attacks and locomotion; vertical death motion is baked into the pose. The first review caught excessive knee bending and corrected it before acceptance.

Equipped weapons remain governed by existing item bindings and sockets. A resolved main-hand item selects ArmedAttack. Tests equip a real bronze dagger on each form, verify active visual bindings, exercise an actual basic attack, and unequip it again. This milestone does not award free gear or replace existing item models.

The original controllers, clips, models, equipment assets and their pre-existing edits remain intact. Native Unity authoring is in `tools/authoring/PlayerMotionAuthoring.cs`; no paid generation, new vendor package or Blender normalization was used. The game-dev CLI was unavailable on this host, so production used the established project-native Unity Humanoid authoring workflow. The source hash manifest records the exact working-copy input bytes.

## Validation

`ChieftainsBaneReview` uses an isolated save and checks real elder approach, acceptance, wrong/early kill rejection, ready-without-title, remote turn-in rejection, exactly-once rewards, live name, saved menu title and completion/title after resume. Damage is scripted fixture damage and is not a balance claim.

`PlayerMotionLiveReview` exercises the actual runtime class models, normal movement with leg rotation, learned skill gestures, real equipment bindings/main-hand attacks, stun pause/recovery, settled death/restoration and unchanged body scale on all three forms. Its health/class/item fixtures are disposable. Captures include gameplay and closer presentation views. Only runtime gameplay captures are acceptance evidence. An offscreen cloned-Avatar preview produced unreliable first-frame transforms and was moved to local diagnostic logs; those images are excluded from the milestone. Roster cards retain a separate class/level line so earned honorifics do not make class selection ambiguous.

Final architecture, regional regression, Windows build/smoke and preservation results are recorded in `docs/validation/elder-player-motion-20261104.json`. No push or merge is included.

The broad regional regression passed 234 checks, including existing navigation, pursuit, quest/reward persistence and equipped menu presentation. Architecture/dependency policy, missing managed references, the existing four-element quest and six fort traversal segments passed. A final focused player run additionally verified all three settled corpse poses after 1.25 seconds, remaining above the gameplay ground reference, followed by successful restoration.

The verified final artifact is `Builds/BriarbackMilestone-20261104-214549/DiceFree.exe`, a seven-scene Windows Development build with zero errors and zero warnings. It includes the checkout's preserved pre-existing WIP and is not a clean release artifact. Authored automation configuration/shared pipeline assets were verified restored after building. Earlier `213830` and `214155` builds and their failed-smoke diagnostic profiles remain preserved.

That exact executable passed elder acceptance/Chieftain kill/return/title, three class-controller switches, separate class labels under the saved title in the roster, title persistence after resume, all four graphics presets, the original four-element hunt, eight fort walks over 216.69m, built warning/axe effects, development-room return and normal menu Quit. Its retained process handle reported exit code 0, and its Player log contained zero exception/compiler/shader-error lines. Actual Player captures of the offer, earned title, both advanced forms and titled roster were inspected and copied under `VisualBaselines/PlayerMotion/Windows`.

All 54 original tracked WIP files retained their initial byte hashes. Their animation/controller edits, combat/skills/UI work and project settings are excluded from this commit, as are local recovery scenes, raw vendor art and user saves. No remote push or merge was performed.

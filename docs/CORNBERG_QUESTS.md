# Cornberg Q1, respawn and progression PoC

This increment extends the existing saved Cornberg scene on `poc/cornberg`.
It preserves geography, navigation, duel balance, death/return and well healing.
This note describes the original Q1 slice. Local saving is covered in
[CORNBERG_SAVES.md](CORNBERG_SAVES.md); the subsequent Q2 extension is covered in
[CORNBERG_ROAD.md](CORNBERG_ROAD.md). No Tier-1 classes, multiplayer or shops are implemented.
Post-Q3 conversation content and the TalkTo extension are covered in
[CORNBERG_RUNNER.md](CORNBERG_RUNNER.md).

## Play

The placeholder **former swordswoman / farmer** stands beside the eastern
fields, near `(32, 0, 2)`. Right-click her to approach and open interaction, or
stand nearby and press **I**. Accept **Crop Slimes** in the interaction panel.
The right-hand quest tracker shows the active stage and count.

Kill three crop Slimes, then two more. The existing single Slime respawns at its
authored spawn after **10 seconds**; its corpse label shows the countdown.
Return to the farmer and choose **Turn in**. Acceptance and turn-in require a
living nearby actor with line of sight. Escape closes interaction; movement,
Stop or an attack command cancels a pending approach. I is a provisional
interaction binding, avoiding the current camera Q/E controls.

The thin bottom XP bar shows level and current/required XP. The existing HP and
target widgets update with the actor's current stats. A brief level-up message
confirms advancement. The well remains available between fights.

The debug reset button remains explicitly labelled for tests; normal play uses
the timer. It reuses the actor and cancels any pending respawn timer.

## Provisional values introduced

| Setting | Value / policy |
| --- | --- |
| Ordinary overworld respawn | 10 game seconds after death |
| Crop Slime defeat XP | 10 |
| XP needed for next level | 30 + 10 × (current level − 1) |
| Q1 objectives | 3 crop Slimes, then 2 additional crop Slimes |
| Q1 turn-in reward | 20 XP, no items/currency |
| Level-up HP | Preserve missing HP; add the increase in maximum HP to living current HP |
| Dead actor level-up | Updates maximum/stat values, never restores current HP |
| Interaction radius | 2.5 units from authored approach point, with line of sight |

The starter curve is placeholder data, not a balanced level 1–200 design.
Its configurable level limit is 200, consistent with the Novice design; no
advanced class or advancement gate is implemented.
The provisional level-up HP policy is recorded for review on
[issue #11](https://github.com/Krtz/DiceFree/issues/11#issuecomment-5876521671).

A fresh character earns 30 XP from the first three kills: level 2, 0/40 XP.
Two additional kills yield 20/40; the 20-XP turn-in reaches level 3, 0/50 XP.
Extra farming can naturally change those milestones. The duel's HP, damage,
aggro, leash, attack timing, return timing/fraction and well values are unchanged.

## Credit and quest rules

- Every actual death emits one semantic `ActorDefeated` report per life. Further
  damage to the corpse emits nothing. Respawn resets the per-life reporting latch.
- Reports snapshot a session sequence ID, defeated content/family IDs, victim,
  killer, credit owner, position and authored XP reward. Names and UI are not
  used for credit.
- Current credit policy is the lethal source actor, or its explicitly assigned
  owner. Environmental deaths without a source have no player credit. This is
  a local single-player policy; contribution, assists, party eligibility and
  network authority remain future work at this boundary.
- `KillCreditReceiver` filters to its owner and deduplicates session event IDs.
  XP and quests subscribe separately to the credited event. Combat does not
  inspect quest state, quest IDs or Slime names.
- Each credited kill grants enemy XP regardless of quest acceptance/completion.
  Quest counters advance only for active matching objectives. Q1 matches the
  stable crop-enemy content ID, not the broader Slime family or display name.
- Pre-acceptance kills are not retroactive. A kill finishing stage one does not
  also count toward stage two. Kills while awaiting turn-in or after completion
  grant XP but cannot overflow/restart the quest.
- Turn-in marks the quest completed before granting XP. Repeated acceptance or
  turn-in cannot duplicate counters or rewards. Q1 is not repeatable in this pass.

## Architecture

- `DefeatReporter` bridges Health's source-bearing death event to `DefeatEvents`.
  `BasicAttack` has no quest/progression dependencies. `KillCreditReceiver` is the
  ownership/deduplication boundary for progression consumers.
- `OverworldRespawn` and `RespawnDefinition` work for ordinary combat actors,
  not a named Slime type. The existing object is teleported to its authored
  point projected onto the NavMesh; HP, colliders, attack cooldown/command and
  AI state reset. There is no instantiate/destroy accumulation. A failed ground
  placement retries rather than restoring an actor off navigation.
- `ExperienceCurve` supplies thresholds. `ExperienceProgression` owns remainder
  XP and applies levels through `ActorStats`. Stat-change events update health
  and movement independently. Attacks read current attributes at hit time.
  Novice growth remains the existing data-defined +1 in all five attributes.
- `QuestDefinition` contains stable ID, definition version, ordered semantic
  objectives and reward. `QuestJournal` owns per-actor serializable progress:
  Available → Active stages → ReadyToTurnIn → Completed. Detached snapshots
  contain stable quest ID/version, stage and count. The subsequent local save
  slice adds disk persistence and a real v1-to-v2 migration; cross-domain
  transactions remain future work. See [save notes](CORNBERG_SAVES.md).
- `InteractionTarget` exposes a reusable authored approach point, range and
  interaction hook. `Interactor` handles approach/open/cancel and input.
  `QuestGiver` handles quest actions; UI remains in independent tracker,
  dialogue, world-prompt and XP widgets. Future shops/banks/advancement NPCs can
  supply other target implementations without inventing their own click motor.

No progression code switches on class names or introduces a class-parent tree.
Large asymmetric class graphs, Echo-wide multi-lineage requirements, resources,
discovery and class save-state policy remain independent future systems.

## Validation

Run Unity with `-batchmode -projectPath <repo> -executeMethod <method> -logFile
<path>` and omit `-quit` for the Play Mode runners:

- `DiceFree.EditorTools.CornbergQuestValidation.Run`: seven lives of the same
  actor; timed full-state respawn; exactly-once death/credit/XP; pre-accept and
  post-completion behavior; 3+2 stages; local-only one-time turn-in; level 2/3
  stats and adaptive fists; living/dead level-up HP; I and real contextual
  right-click interaction. The fixture uses lethal Health packets to isolate
  event/progression behavior; the combat suite exercises actual auto-attacks.
- `DiceFree.EditorTools.CornbergCombatValidation.Run`: existing duel, damage,
  player death/return, well, leash and both movement/control modes.
- `DiceFree.EditorTools.CornbergValidation.RunAll`: all 13 original routes,
  navigation boundaries and real movement inputs.

Build with `DiceFree.EditorTools.CornbergPlayerBuild.Build` and `-quit`.
`CornbergQuestSetup.Install` is an additive authoring migration; do not run the
old Phase 1 scene recreation helpers over the current scene.

The traversal click fixture now aims at visible ground below the quest tracker
in Unity's 640×480 batch Game View. It still sends a real right-click and asserts
the target is not covered by UI; all 13 route destinations are unchanged. The
placeholder HUD's intended playtest window is 1280×800, not the tiny batch view.

Validation result for this increment: all three Play Mode suites passed. The
Windows development build succeeded and its 12-second standalone startup smoke
reported no runtime/navigation errors. Startup smoke does not claim a full
standalone quest playthrough. The previously tracked editor-only SearchDatabase
startup exception (#17) still occurs in batch runs and is narrowly identified
separately from gameplay failures. The existing 8,409 scene objects/components
were retained; navigation and previous combat-tuning assets are unchanged apart
from adding the enemy's family ID and XP reward.

## Limits and next step

All NPC art, dialogue and HUD styling are placeholders. Spawn occupancy is not
reserved; a nearby hero can draw fresh aggro after respawn. The timer pauses
with game time and is not persisted across sessions. Quest/XP state now autosaves
through the subsequent [local save slice](CORNBERG_SAVES.md).
Credit IDs/deduplication remain session-local, not network/save IDs.

Continue playtesting the complete five-kill/return route and the save/reload flow.
Q2 now extends this flow through the [road investigation](CORNBERG_ROAD.md).
Q3 continues with the [named forest Slime](CORNBERG_FOREST.md), reusing the same
semantic objective, NPC interaction, enemy and persistence boundaries.

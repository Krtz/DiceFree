# Cornberg Q2: investigate the road

This extends the saved Cornberg scene on `poc/cornberg`, without rebuilding its
geography or navigation. Complete Q1 and speak to the same former swordswoman /
farmer. Accept **Check the Road**, reach the trail marker on the northeast road,
defeat three Road Slimes, then return for the reward. The placeholder tracker
shows both quests. There is one authored Road Slime spawn; its normal timer
supports the three kills. No dungeon, Q3, economy or advancement is included.

## Authored data and provisional balance

| Setting | Road Slime / Q2 |
| --- | --- |
| Content ID | `enemy.road-slime` |
| Shared family | `enemy-family.slime` |
| Fixed level | 3, independent of player level |
| Maximum HP | 30 |
| Primary attributes / growth | 0 / 0; fixed HP and attack data |
| Basic attack | 6 raw physical damage, no element; 1.7 s interval, 0.45 s windup, 0.6 reach |
| Movement / awareness / leash | 3.6 / 8 / 16 |
| Body scale | 1.25 times the shared placeholder |
| Defeat XP | 20, including before acceptance and after completion |
| Respawn | Existing 10 game-second timer, same actor at its home |
| Spawn | `(64, 0, 34)` on the existing road |
| Investigation | `area.cornberg.road-investigation`, center `(57, 0, 26)`, radius 4 |
| Quest ID | `quest.cornberg.investigate-road`, definition version 1 |
| Prerequisite | Completed Q1, `quest.cornberg.crop-slimes` |
| Objectives | Reach investigation area, then 3 matching Road Slime defeats |
| Turn-in reward | 50 XP, once; no currency/items |

All new enemy numbers, radius and XP rewards are provisional tuning assets in
`Assets/_DiceFree/Settings/Enemies`. Crop Slime duel tuning is unchanged.
The existing threshold formula is `30 + 10 * (level - 1)`. A normal fresh Q1
turn-in ends at level 3, 0/50 XP. Three Road kills give level 4, 10/60 XP;
Q2's reward reaches level 5, 0/70 XP. Additional farming changes that result.

Novice uses the settled defaults: +15 maximum HP and +0.1 base HP regeneration
per VIT, including during combat. Base HP remains 10: levels 1/3/5 have 25/55/85
maximum HP. Existing healing-received modifiers still apply. Independent actor
coefficient overrides and source-keyed additive/percentage modifiers are available;
there is no Novice exception. See [combat notes](CORNBERG_COMBAT.md).

## Reusable boundaries

`EnemyArchetype` supplies shared family, prefab, awareness/leash and body defaults.
`EnemyVariantDefinition` selects actor/attack/XP data, fixed level, body scale and
optional awareness/leash overrides. `EnemyVariant` applies these before stats and
health initialize. Both Slimes use the same AI, attacks, defeat reporting, XP and
respawn components. The existing Crop actor is retained and bound to its variant;
the new Road actor uses the shared prefab. Future abilities/elements can be composed
through actor/attack data without a second Slime engine.

`ReachArea` reports physical entry facts through `AreaEvents`: stable area ID,
actor, position and session sequence. It knows nothing about quests. The journal
filters to its owner and advances only the active matching semantic objective.
There is no retrospective entry history: entering before acceptance does not count;
accepting while already inside requires leaving and entering again. Staying inside
emits once; re-entry is a new fact but cannot repeat the completed one-time stage.
This small spherical area component is not a general exploration/encounter engine.

Quest definitions now support ordered Kill and ReachArea objectives and completed
quest prerequisites. Kill remains enum value zero, preserving Q1 asset semantics.
Road objectives require both the specific content ID and shared family ID. Crop
kills therefore grant their normal XP but do not advance Q2. Combat never inspects
quest IDs, display names or UI. The farmer's existing generic interaction target
selects active/ready quests before newly available offers.

## Persistence and scope

The existing version-2 manifestation section already stores stable quest ID/version,
status, stage and count. Q2 needs no new schema or migration. Old profiles acquire
an Available Q2 record; its Q1 prerequisite still controls acceptance. Unknown
records remain preserved. Completed state is committed before granting the reward,
and reload cannot replay it. Full-HP anchor loading and transient combat resets
remain unchanged. Existing v1-to-v2 migration remains the real migration path.

The original Crop duel tests explicitly isolate that actor from Road Slime, retaining
their original assertions. Traversal still tests all 13 destinations. Run the new
`DiceFree.EditorTools.CornbergRoadValidation.Run` in batch Play Mode without `-quit`,
with a fresh absolute `-diceFreeSaveRoot`. It covers prerequisites, pre-entry and
wrong-area rejection, repeated entry, wrong enemy, actual Road duel, exactly-once
credit/XP, timed same-instance respawn, fixed levels, level-five pacing and real
scene reload at accepted/investigated/one kill/two kills/ready/completed states.

`CornbergRoadSetup.Install` is an additive authoring helper. Do not run the old
scene recreation helpers. All models, sign text, dialogue and HUD remain placeholders.
No save clock, multiplayer credit, spawn occupancy reservation, inventory, Q3 or
full encounter framework is introduced. The smallest next step is a playtest of
Q1 → Q2 pacing and road readability before adding another quest/enemy mechanic.

The subsequent [Q3 named forest Slime slice](CORNBERG_FOREST.md) now extends this
route through a real farmer offer, existing forest clearing, authored stronger
variant and return toward level 6. This note records the original Q2 increment.

## Validation results — 2026-09-29

- Q2 Play Mode suite passed, including its six actual scene-reload checkpoints.
- Existing Q1 suite passed, retaining seven lives and exactly-once credit checks.
- Combat/death/Return/well suite passed, including both input modes, the settled
  Vitality defaults, independent overrides, growth and modifier addition/removal.
- All 13 traversal routes and navigation/input checks passed.
- Fresh, injured reload, dead reload and legacy migration process suites passed:
  full-HP anchors, unknown records, recovery/backups, stale writers, resource
  policy and Profile Inspector diagnostics remain green.
- Windows development build succeeded: 171,957,430 bytes. Six isolated 12-second
  standalone launches passed without runtime/navigation errors: fresh revisions
  1 → 2, completed-Q2 revisions 11 → 12 (level 5, zero XP, no reward replay), and
  legacy migration revisions 6 → 7 (identity, level 3/7 XP and unknown quest kept).
  These are startup/reload smokes, not a manual standalone quest playthrough.
- All 8,436 pre-existing scene object/component IDs are retained. The scene adds
  381 lines and removes none; baked navigation is unchanged. Known editor-only
  SearchDatabase exception #17 remains separate from gameplay validation.

Design documents were copied without merging through `setup/unity-project`
`9193c2c`, including changes published during implementation. The newer STR/INT,
AGI/SPI and Defense-curve/modifier decisions were initially preserved in design docs only;
the Q2 patch adopted Vitality and retained the other provisional runtime coefficients.
The subsequent [stat/Defense follow-up](CORNBERG_COMBAT.md) now adopts those defaults. Economy,
controller and cube-world decisions likewise remain documentation-only here.

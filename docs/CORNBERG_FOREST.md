# Cornberg Q3 — Named Slime

Q3 is the next starter beat specified by issue #6 and WORLD_1.md: locate a named
forest Slime, defeat it, return to the former swordswoman/farmer, and reach roughly
level 6. It follows Q2 and precedes the runner's return. This increment implements
that playable beat, not the dungeon, Q4 alternatives or advancement.

Repository review started from PoC `571af3d` and design `98f9917`, including all
open issues/comments and the implementation/validation notes. Relevant World 1,
quest, enemy and technical design docs already match setup/unity-project; no
branch-history merge or further combat-math expansion was needed.

## Play

Complete Q2 and return to the farmer by the eastern fields. Right-click her, or
approach and press I, then accept **Named Slime** through the existing NPC panel.
Follow the east road and take the southern woodland loop into the existing
clearing near `(86, 0, -3)`. A small world label and the quest tracker identify it.
Entering the 10-unit area advances Locate to Defeat. Defeat the larger green
Slime, return to the farmer and turn in for XP. Both existing movement modes work.

The farmer retains the latest completed authored offer when no active/available
quest remains, instead of jumping back to Q1. The existing NPC panel and tracker
provide all required acceptance, stage/count, return and completion feedback.

## Provisional content

WORLD_1 explicitly leaves Q3's exact name, location and mechanics open. **Named
Forest Slime is a placeholder display label, not a finalized lore name.** The
existing southern clearing is a reversible blockout placement. There is no new
unique boss mechanic, conditional ability or permanent world-removal rule.

| Authored setting | PoC value |
| --- | --- |
| Content ID / family | `enemy.named-forest-slime` / `enemy-family.slime` |
| Fixed level | 5; never scales to player |
| HP / base Physical and Magical Defense | 60 / 0 / 0 |
| Primaries / growth | All zero; direct monster stats |
| Basic attack | 8 raw Physical, no element |
| Interval / windup / reach | 1.8 s / 0.5 s / 0.6 units |
| Movement / awareness / leash | 3.4 / 8 / 14 units |
| Body scale | 1.6 × shared Slime model |
| Enemy XP / quest turn-in XP | 35 / 35 |
| Respawn | Existing ordinary 10-game-second timer |
| Locate area | `area.cornberg.forest-clearing`, radius 10 |
| Quest | `quest.cornberg.named-slime`, version 1 |

Normal Q2 ends at level 5 with zero XP. The next threshold is 70 XP, so one kill
plus turn-in reaches level 6 with zero remainder. Extra farming can change this.
All new numbers are exposed in data; existing enemy/quest/attack/well/Return
balance assets are untouched. Name, placement, ordinary respawn and basic-only
combat are explicitly provisional pending Q3 design/playtest refinement.

## Reuse and durable state

The new enemy uses the existing Slime archetype/prefab, EnemyVariant, AggroBehaviour,
BasicAttack, OverworldRespawn, DefeatReporter and separate XP/quest consumers.
No new enemy engine or quest-specific combat logic was necessary.

Q3 is authored entirely from completed-Q2 prerequisite, ReachArea, Kill and normal
NPC turn-in. Stable content + family predicates reject Crop/Road kills. Pre-accept
area entries/kills are not retroactive; the named kill still grants ordinary XP.
The locate objective must be active before area credit, then the kill objective
must be active before kill credit. Corpse damage never duplicates credit. Kills
after completion grant enemy XP without restarting the quest or its reward.

Existing schema/manifestation version 2 stores quest ID/version/status/stage/count.
Old profiles lacking Q3 acquire an unstarted record; Q1/Q2 and unknown records
remain intact. There is no migration or class-slot change. Loading still means
full HP at the registered anchor; enemy combat state remains transient.

The saved scene is extended additively with one prefab instance and one small
area/label. All 8,449 pre-existing serialized scene object/component IDs remain.
The baked navigation and original terrain/buildings/paths remain unchanged.
`CornbergForestSetup.Install` is an idempotent additive content authoring helper;
never run the old scene recreation helpers over this scene.

## Validation

Run `DiceFree.EditorTools.CornbergForestValidation.Run` in batch Play Mode without
`-quit`, using a fresh absolute `-diceFreeSaveRoot`.

The focused test writes a pre-Q3 v2 profile with completed Q1/Q2, level 5 and an
unknown quest, then validates: Q2 prerequisite, old-profile compatibility, NPC
approach/acceptance/turn-in, no remote interaction, pre-accept credit policy,
actual walk to the forest, locate stage, wrong Slime rejection, fixed level,
actual auto-attack duel, once-only death/XP, timed same-instance full-state respawn,
level-six pacing, latest completed conversation, and real reload checkpoints at
accepted/located/ready/completed. Identity/unknown records persist; rewards do not
replay. Q1/Q2 and the combat suite separately retain their original coverage.

Q2's fixture now finds its area by stable ID rather than arbitrary scene order.
Its respawn assertion compares the complete actor count before/after respawn,
accommodating additional authored content while retaining the same-instance and
no-duplicates checks. No test requirement was removed.

Validation on 2026-09-30 passed: Q3's four real reload checkpoints and actual duel;
existing combat/death/Return/well (including stat/Defense/crit/element/hit suites);
Q1; Q2 with six reload checkpoints; all 13 traversal routes; fresh/injured/dead/
legacy saves; migration, recovery/backups, stale writers and Profile Inspector.
Windows development build succeeded (171,981,546 bytes). The known editor-only
SearchDatabase exception is still identified separately as #17; other exceptions
fail validation. The final scene diff adds 272 lines and removes none. No existing
balance asset, combat math, persistence implementation, schema or navigation changed.
Eight isolated Windows startup/reload checks passed: fresh, pre-Q3 completed-Q2,
completed-Q3 and legacy-v1 profiles, twice each. Identity and level/XP persisted;
old Q2 acquired available Q3, completed Q3 stayed completed at level 6/0 XP,
legacy migrated to schema 2, revisions advanced and no runtime errors were logged.
These are automated editor/content checks and standalone startup/reload smokes,
not a claimed manual playthrough.

## Next smallest beat

The specified next story beat is the runner returning without reaching the next
town because Slimes blocked the road. A short real NPC conversation can prove a
reusable semantic TalkTo objective/event next. Q4's exact alternative objectives,
counts and reward parity are still open; settle those before implementing its
branching completion. No runner, Q4, dungeon, equipment or advancement system is
silently included here.

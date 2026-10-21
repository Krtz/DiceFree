# Greatblade playable prototype — 2026-10-14

> **Implementation update (2026-10-21):** the approved rank-zero point budget, weighted rank gates/caps, rank-3/5/7 Greatsword Mastery milestones, Metal shield and one-time legacy Greatblade/Mage reset/refund are implemented locally on `integration/world1-foundation`. The current Editor is blocked at a scene-backup recovery modal, so compile and runtime validation have not yet been obtained. The detailed rank audit records exact gates and formulas; do not treat this update as Unity-verified.


Greatblade is now a playable Tier II manifestation, advanced from a level-30 Physically Blessed Novice after the retired swordswoman's normal Slime Boss training quest. Accept the quest, defeat the normal boss, and return to her to complete training. See GREATBLADE_ADVANCEMENT_QUEST.md. The parent manifestation remains intact, and the new Greatblade starts at class level 1. Development Room also offers direct Greatblade selection through the same advancement graph.

This implements the approved kit in GREATBLADE_PROTOTYPE_DESIGN_V03.md. Exact balance numbers below are provisional. The mentor now uses a saved, dungeon-boss-gated training quest; narrative expansion remains future work.

## Skills and learning

| Slot | Ability | Starter/unlock | Rank gates |
|---|---|---|---|
| 1 | Wide Arc | Rank 0; first rank at level 1; cap 20 | Every level 1–20 |
| 2 | Spin Dash | Rank 0; first rank at level 1; cap 16 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76 |
| 3 | Overhead Breaker | Rank 0; first rank at level 6; cap 18 | Every level 6–23 |
| 4 | Battle Cry | Rank 0; first rank at level 18; cap 8 | 18, 26, 34, 42, 50, 58, 66, 74 |
| 5 | Greatsword Mastery | Rank 0; first rank at level 1; cap 8 | 1, 11, 21, 31, 41, 51, 61, 71 |
| 6 | Heavy Rhythm | Rank 0; first rank at level 1; cap 8 | 1, 12, 23, 34, 45, 56, 67, 78 |

Every skill starts at rank 0. Class level 1 grants one point; each level grants one until the 78-rank class budget is exhausted. A purchase requires both an unspent point and the skill's next-rank level gate. Use the action-bar plus button; keys 1–4 use the existing rebindable skill actions. Slots 7–8 remain empty; slot 9 remains reserved for Tier IV+ ultimates. Shared Attack, Interact and Return retain slots 10–12.

The generic HUD now draws abilities by their actual slot data rather than reserving slots 6–9 unconditionally. Six distinct original procedural icon sigils and live rank/cooldown/tooltips are provided.

## Combat tuning

Asset: Assets/_DiceFree/Settings/Skills/Greatblade/Greatblade combat tuning.asset

- Basic greatsword: STR scaling coefficient 2, 1.5s interval, 0.35s wind-up, 1.3m reach; raw spread 0.9–1.1. Inherited flat attack offsets and ordinary stat modifiers still apply.
- Wide Arc: coefficient 1.8 + 0.2 per additional rank, 5s cooldown, 0.35s committed wind-up, 3m reach beyond actor/target radii, 110-degree frontal cone. Actual visibility and geometry are checked at impact. Gold outline shows the committed cone.
- Spin Dash: ground destination within 12m, complete NavMesh route no longer than 15m, approximately 13m/s before contextual movement modifiers, 10s cooldown. Uses ordinary collision-safe direct movement along path corners; no teleport/ghosting. Stops on a blocking unit or obstacle. Each enemy is damaged at most once per dash within 1.8m plus target radius. Coefficient 1.2 + 0.2 per rank.
- Overhead Breaker: 7s cooldown, 0.55s wind-up, 1.2m reach beyond unit radii. Requires at least 20 Momentum and consumes all of it on commitment. Coefficient 2.5 + 0.3 per rank, multiplied by 1 + 0.015 × spent Momentum. Target range and visibility rechecked at impact; interrupted/escaped strikes do not refund.
- Battle Cry: level-18 self damage buff, +12% plus 3 percentage points per rank, 6s duration, 24s cooldown. Applies to basic greatsword and class skill damage.

Greatblade provisional base VIT/STR/AGI/INT/SPR: 14/18/9/5/5; growth: 2/3/1/0.5/0.5. Existing defenses, XP curve and movement systems remain in use.

## Momentum and passives

Momentum cap 100, starts at zero, resets on manifestation load/death. Successful damaging or shield-absorbed basic hits grant 8 base Momentum; skill hits grant 10, capped at 24 base Momentum per area cast. Misses and zero-effect/resistance-restoration hits do not generate it.

Heavy Rhythm gain factor: 1 + 0.08 × rank. Decay begins after five seconds without a connecting hit, while out of combat and not committing a class strike; base decay 8/s × max(0.3, 1 − 0.07 × rank). No threshold buffs or hit-chain resets.

Greatsword Mastery adds +2% greatsword damage and +1 STR per rank. Rank 3 unlocks Wide Arc haste (8%, +2 percentage points per further rank; 3s, +0.25s per further rank); rank 5 unlocks wider Wide Arc (15 degrees, +3 degrees per further rank); rank 7 unlocks a successful Overhead Breaker Metal shield with capacity `100 + 5% maximum HP` for 5 seconds. Elemental absorption consumes 0.8 shield HP per matching Metal damage prevented, or 1.2 for other/untyped ordinary damage; same-source recasts refresh/replace the shield. These are implemented in source but await Unity gameplay regression.

Temporary shield absorption is recorded separately from HP damage. Class buffs, haste, shield, cooldowns and committed movement are cleaned up on manifestation load. Greatblade occupies both hands: offhand equipment is returned to the child bag and cannot be equipped; the parent equipment snapshot is preserved.

## Presentation and persistence

Playable Greatblade and retired swordswoman use the existing shared Greatblade model resource, two-hand grip and animations. Existing portable fallback remains available without licensed art. Greatblade disables generic relaxed Novice arm posing so it preserves the raised two-hand grip.

Skill ranks use the existing classSkills save records and schema/manifestation version 6; no schema bump or rewrite of older Echo data. Resource profiles retain the two Tier-I Mana profiles and add reset-on-load Momentum. Tests use isolated saves, including the DevelopmentRoom subfolder.

No licensed source or derived model/animation assets are staged. Current development branch remains integration/world1-foundation.

## Verification

- DICEFREE_GREATBLADE_PLAYABLE_OK: 75 focused assertions passed in live Play Mode.
- Actual level-30 advancement, level reset, four starting ranks, gated learning and rejected invalid save ranks.
- Front pair hit / rear target excluded by Wide Arc; basic-hit generation; guaranteed misses generate no Momentum.
- Open-route dash reaches its destination; living blocker is hit once and stops travel without overlap; inaccessible/out-of-range targets consume no cooldown.
- Breaker consumes all 100 Momentum and lands damage; low-Momentum use rejected.
- Real Mastery haste, shield absorption without HP loss, Battle Cry damage increase, transient cleanup and out-of-combat decay.
- Activating Greatblade through the shared action bar cancels an in-progress Return Home channel.
- Offhand removal/rejection, preserved parent equipment, Greatblade level/rank switch roundtrip and six skill records read from the actual on-disk isolated save.
- Captured and inspected live Game view: shared model, Echo title, Momentum HUD, six icons and both passives visible.
- Existing Development Room main-menu entry, isolated profile, item controls and World 2 round trip passed.
- Advancement, Physical, Magical, Novice skills, architecture, policy, managed references, quadratic XP migration, skill tooltip and Greatblade visual/progression validators passed.
- Legacy validation assumptions updated: four classes/three advancement edges, extensible resource lists, existing schema v6, and player-class accuracy policy rather than forcing scripted enemy attacks to use basic-attack accuracy.

Screenshot: screenshots/greatblade-playable-2026-10-14.png
Machine-readable focused and regression results are stored beside this document.

Windows x64 Development build succeeded: seven scenes, 317,925,848 reported bytes, zero errors and zero warnings. Build: Builds/World1Foundation/DiceFree.exe. Long-form balance/animation-feel playtesting, multiplayer and narrative advancement quests remain future work.

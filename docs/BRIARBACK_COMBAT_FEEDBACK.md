# Briarback equipped combat and gameplay feedback — 2026-11-04

This pass continues `7323dca` on `feature/briarback-combat-feedback`. It makes combat and quest progress easier to read at the actual gameplay camera, and expands the previous unarmed Novice assessment. Existing working-tree changes are preserved separately in `Logs/CombatFeedbackInitialWipHashes.json`.

## Equipped camp assessment

`tools/validation/BriarbackEquippedCombatReview.cs` runs nine encounters in a disposable profile: Novice, Physically Blessed Novice and Magically Touched Novice at levels 25, 30 and 35. The target is the real level-25 Raider in the three-member northern camp; its Spear Thrower and Totemcaller remain active. Combat runs at normal time scale without invulnerability, fixture damage or disabled enemy AI. The policy uses real learned skills, ordinary cooldowns and resource costs, normal attacks, then a real retreat. Initial placement, ordinary XP grants, existing-item grants and between-trial restoration are explicitly setup fixtures.

All three use Bronze Dagger, Cornberg Work Gloves, Farmer's Pants, Forest Travel Shoes and Slimy Tophat. Physical additionally uses Slime Shield; Magical additionally uses Slime Orb. These are existing starter/Slime items, not a newly balanced level-25 equipment tier. Skill points are spent evenly through the normal progression APIs until the current level's budget or authored caps are exhausted.

| Early class | Player level | Combat duration | Outcome | Minimum HP / maximum HP |
| --- | ---: | ---: | --- | ---: |
| Novice | 25 | 13.41s | Retreat at health threshold | 85.02 / 490 |
| Novice | 30 | 15.58s | Retreat at health threshold | 142.13 / 580 |
| Novice | 35 | 21.71s | Raider defeated | 330.89 / 670 |
| Physical | 25 | 9.91s | Raider defeated | 310.94 / 655 |
| Physical | 30 | 6.62s | Raider defeated | 537.43 / 730 |
| Physical | 35 | 7.82s | Raider defeated | 585.13 / 805 |
| Magical | 25 | 12.90s | Raider defeated | 268.19 / 580 |
| Magical | 30 | 8.09s | Raider defeated | 392.28 / 655 |
| Magical | 35 | 9.16s | Raider defeated | 433.33 / 730 |

Every trial survived the retreat. These are one-policy, one-camp observations, not statistical class comparisons. Random damage, misses, stuns, AoE and timing affect durations. Defeating the Raider does not mean the entire camp was cleared. Outgoing damage includes other enemies hit by AoE, and normal healing means incoming damage can exceed net HP lost. The minimum HP is sampled from damage events during the fight; survival is checked again during retreat.

This does not approve final regional, Tier II, party, loot or economy balance. In particular, continuing an early class beyond the level-30 advancement threshold is a controlled comparison rather than the final progression path. No enemy damage, health, loot chance, quest reward, attack cooldown, skill tuning or gear stat is changed on the strength of these observations.

`BriarbackEquippedBossReview` separately observes all three early classes at level 40 against the real Chieftain and both guards. Each dodged one natural smash and displayed the actual countdown, but **all three died during the attempted retreat**. Their total observations, including retreat, lasted 13.58s, 16.61s and 13.61s respectively. Incoming damage was 934.51, 1108.88 and 985.84; normal healing explains totals above maximum HP. The assay's `passed` phase means its observations completed, not that survival passed or a boss was defeated. These starter/Slime loadouts and early classes do not establish the intended Tier II/group world-boss balance. Guard and Chieftain pressure remain unchanged pending that work.

## Combat readouts

`ActorFeedback` now keeps a bounded history of resolved damage, criticals, ordinary healing, absorption and elemental-resistance restoration. Separate hits rise independently; same-category packets within 75ms may coalesce. Each unit retains at most eight entries for 1.15 seconds. Text uses a dark outline, distinct colors, short labels and staggered vertical lanes above the nameplate. No per-hit GameObjects, colliders or animation authority are introduced. A real basic-attack miss is read from the existing committed miss counter, never rerolled for presentation.

`DamageResult.absorbed` can include redirection as well as temporary shields, so its number says **Absorb**, while the separate blue overhead strip reports the actual **TemporaryShield** value. Normal regeneration does not create healing-number spam. Restoring a unit clears stale numbers. Fog, camera depth, modal menus and HUD layout editing continue to suppress inappropriate readouts. World readouts also consult the existing visible HUD regions so enemies behind character/quest panels do not paint over their contents.

Some later creatures do not have the old blockout feedback component. The overhead-health service installs a readout-only component for those living actors. Its visual reference remains empty, so their skeletal presentation never receives the old squash effect. Their names appear when hurt, and readout-only units do not produce the legacy per-hit debug logs.

Overhead HP bars retain a brief amber trail after damage, then drain toward the real HP value. Passive regeneration does not immediately erase it. Numeric HP and the main fill remain authoritative. Maximum changes, death and restoration recover correctly. Shield values and a **STUNNED** label provide immediate context, while existing self/ally/enemy visibility preferences still apply.

## Boss encounter panel

`EncounterHud` is a common movable HUD widget, fed by the read-only `IEncounterHudSource` interface. The Chieftain registers its existing actor, guard liveness and actual 1.5-second smash deadline. The panel displays HP, percentage, guards standing and a countdown telling the player to leave the red strip. It appears only for an engaged, living, nearby and visible encounter, and clears on death, disable, departure or scene transition. A brief recovery grace prevents flicker while the encounter cancels its basic attack for a special.

The existing smash timer is expressed through a shared constant with the same value. Damage resolution, telegraph footprint, AI target selection, threat, guards and cooldowns remain under their original owners. This panel is not a second encounter simulation.

The death/return panel and resurrection-point attunement notice now use the current HUD theme. The death panel presents the real destination, current return delay and existing bound Return action. World names, combat numbers and overhead bars are hidden while that local death panel is active, then recover after return. Its presentation does not change resurrection timing, restored health, anchor selection or dungeon return handling. A focused fixture waits through the actual delay, uses `RespawnAtAnchor.Return`, and verifies a living player with a recovered animation.

## Quest readability and activity

The tracker shows the current step out of the total, a progress strip, a distinct ready-to-turn-in state, and the actual scene quest giver's display name when available. The journal distinguishes completed, current and later steps, and displays the title reward. Empty trackers and unsupported saved versions have explicit text.

`QuestJournal.GameplayProgressed` publishes detached before/after records only from real acceptance, objective progress and successful turn-in. `RestoreState` never publishes it. `QuestActivityFeed` writes useful milestones to the existing bounded chat history, including rewards and earned titles, without replaying saved history or rejected/duplicate actions. Partial kill-count changes remain in the tracker rather than flooding chat.

During this work, repeatable turn-in rollback was found to restore only the status after a failed reward callback, leaving the reset count behind. Rollback now restores status, stage, count and alternative counts from the original ready record. `QuestActivityReview` deliberately throws once from a gold observer, verifies the full 20-kill record and XP/gold are restored, then retries successfully with one completion message. This fixes a progress-loss edge case without changing reward amounts or the save format.

## Validation and artifacts

The focused checks cover burst coalescing, separate hits, bounded capacity, expiry, invalid values, critical/heal/absorption/resistance labels, recent-health behavior, ordered quest fractions, named scholar return text and inert unsupported versions. Live fixtures additionally cover real basic misses, runtime enemy registration, shield absorption without HP loss, restoration cleanup, quest/journal display, actual boss wind-up and guard count, disable cleanup and death cleanup. Gameplay captures are under `VisualBaselines/CombatFeedback/Live`; the nine earlier combat-assessment captures are separately labelled under `Assessment`.

The focused UI damage and quest-state fixtures are not combat victories. The nine camp encounters above are the real combat assessment. Final regional regression, executable validation, artifact path and preservation results are recorded in `docs/validation/briarback-combat-feedback-20261104.json`. Local vendor art, recovery scenes, real saves and the original working-tree changes remain outside this work. No push or merge is included.

The full regional gameplay regression passed 234 checks after the quest event/rollback changes: 38 walks over 1,033.08m, pursuit/leash, live creature motion, elite dodge/interruption, quest rewards, saved equipment and menu portrait. The later death-screen presentation, stale quest-giver cache recovery and read-only countdown clamp were covered by the final focused live review and executable smoke. The architecture/dependency/managed-reference check, four-element quest validation, six fort navigation segments and all 33 creature presentations also passed.

The final artifact is `Builds/BriarbackMilestone-20261104-223527/DiceFree.exe`: seven scenes, 449,925,799 build bytes, zero errors and zero warnings. It is a Windows Development build containing the checkout's preserved class/UI WIP. This exact executable passed combat/heal/absorption readouts, named scholar return, gameplay-only quest milestones with no save-history replay, Chieftain quest/title, actual boss countdown/effects, all three early-class controllers, real delayed death return, four quality presets, eight fort walks over 216.17m, saved identity/title/quest resume, Development Room return and normal Quit. The retained process handle reported exit code 0. Its Player log contained zero exception/compiler/shader-error lines. Final executable captures were inspected and copied into `VisualBaselines/CombatFeedback/Windows`.

The earlier `223012` build is preserved as a two-deprecation-warning diagnostic; the warning-free `223205` build also passed its smoke and exit-code check, then was superseded by the final world-readout/HUD occlusion correction. All 54 original tracked WIP files and all seven protected build-setting/shared-pipeline paths retained their original state. Only this milestone's owned sources, evidence and documentation are committed locally; no remote push or main merge was performed.

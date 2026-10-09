# 2026-10-08 playtest fixes

Implemented directly in `T:/TEMP/DiceFree-Options`, on `feature/54-world-dungeon-codex`, using Unity 6000.6.3f1. No branch switch, commit, push, merge, reset or restore. Existing equipment/icon assets, inventory presentation and village authoring remain present and pass their validators. Neither prohibited scene/art builder was executed. Scene changes were made through the Unity Editor and saved incrementally; terrain and baked navigation assets were not regenerated. Play Mode checks used fresh temporary save roots, including the save/return/resume checks.

## Implemented behavior

- The functional portal is on `08 - Slime dungeon entrance - exterior only`. Its reachable approach/click point is approximately `(81, -0.16, 54.84)`. Cave geometry clicks resolve to that portal; I searches by the approach position. The misplaced portal's component, collider and stale entry instruction are disabled; scenery meshes and transforms are retained.
- Staging creates a visible straw practice target with a bullseye, real hostile target hitbox, invulnerable health, no movement or reward payout. Start Now accepts only the living initiator during loaded staging and uses the existing occupancy lease transition and roster/class/loadout snapshot. The original 60-second automatic start remains.
- Nine generic NPCs have practical solid capsules and navigation obstacles. Stationary service NPCs carve their footprints; four existing home roamers retain wandering and check live collision footprints. Existing interactions remain intact. Their dedicated visual pivots have stronger reversible sway, bob and breathing.
- 524 Cornberg trees and 350 dungeon trees have scoped trunk-only collision and navigation footprints. Canopy bounds trigger whole-tree transparency without physical canopy colliders or forced camera zoom. Opacity is restored when clear. Material replacements while faded are preserved, including the Regent puzzle's blue enchantment. Player camera range is 8–110, with stronger scroll sensitivity.
- X enters the existing reticle controller. Enemy confirmation orders the existing autoattack; ground confirmation starts attack-move, acquires nearby visible hostiles while moving, and resumes the destination after a target dies. Escape cancels reticle/attack-move; right-click and direct movement replace the order. UI clicks do not confirm targets. Manifestation changes reset transient attack-move state.
- Slimes use reusable visual squash and impact motion. Boss Slam has anticipation, leap, impact squash and recovery; Divide has anticipation and reform. Fragment scale rises from 0.9 to 1.6. Miniboss HP is 800 (was 160), normal boss HP is 2,100 (was 420), Regent HP remains 1,600. Miniboss/normal fragment HP remains 19.2/50.4 rather than inheriting the parent HP increase. Existing thresholds remain 70%/30%.
- Overworld kills enter the existing persistent Codex through deduplicated authority kill credit. Dungeon kills retain the run's own semantic events; the overworld bridge excludes dungeon actors.
- Physically Blessed has authored autoattack endpoint offsets of −3/+3 after normal scaling. Actual attack rolls and the character-stat range agree. Spell packets do not inherit these offsets.
- Completed quests are excluded from the active tracker; ReadyToTurnIn remains. The menu group has a journal button with quest offers, current objectives and completed history, plus close/Escape behavior.
- Minimap +/− controls clamp its radius to 8–60 independently of the player camera and map heading.
- Options offers confirmed Save and Return to Start Menu. A failed save cancels return. Success closes transient targeting/UI state, releases the dungeon lease and loads the existing StartMenu. Resume retains inventory, wallet, Codex and manifestation progression.
- General Goods supports Buy and Sell. Both wallet/inventory mutations commit before notifications. Shop callbacks are guarded against reentry; missing instances and wallet overflow cannot consume/grant a sale. Equipped items, quest items, bound definitions/instances and locked run inventory cannot be sold. The provisional configurable resale rule is 50% of a known shop price, rounded down (Bronze Dagger: 7 gold), or 1 gold for other tradeable items without an authored shop price. Item policy fields are additive and old records default to unbound.
- The existing humanoid presentation driver lowers idle upper arms using actual arm directions, adds subtle independent arm motion, and preserves attack/locomotion animation and bone-attached equipment sockets.

## Actual verification

- `recompile_status`: completed without reported compilation errors on final source.
- `dicefree.playtest-fixes.install`: successful repeated idempotency runs; 9 NPCs, 524 Cornberg trees, 350 dungeon trees, same portal approach.
- `dicefree.playtest-fixes.validate`: `PLAYTEST_P0_SCENE_OK`.
- `dicefree.slime.validate`: `SLIME_DUNGEON_DATA_OK`, including party lease sharing, other-party exclusion, late-join exclusion and variant conditions.
- `dicefree.economy.validate`: `CORNBERG_ECONOMY_VALIDATED`.
- `dicefree.village-art.validate`: `VILLAGE_ART_VALIDATED`, including 4 equipment prefabs, 55 mesh parts, 4 bindings, 3 forms, 9 idle NPCs and 4 wanderers.
- `dicefree.item-icons.validate`: `ITEM_ICONS_VALIDATED`, 8 unique sprites/PNGs and 4 UI surfaces.
- `dicefree.playtest-fixes.playtest`: passed the combined isolated-save regressions. Checks exercise actual advancing frames, cave right-click and I entry, tree fading/restoration and material replacement, visible idle, dummy attacks/no rewards, X/click/UI/Escape behavior, moving acquisition, Codex deduplication/persistence, damage endpoints/spell exclusion, journal/tracker state, minimap clamps, atomic Buy/Sell/reentry/repeat/overflow/policy rejection, Start Now authorization/lock/idempotency, real slime/Slam/Divide deformation, fragment durability, menu save/resume and lease release.
- `dicefree.village-art.playtest`: passed its existing 19,301 checks for equipment, idle restoration and real wandering/home bounds.
- `dicefree.economy.playtest`: `CORNBERG_ECONOMY_PLAYTEST_OK`, 31 checks for purchase, equipped damage/visuals and persistence/reload.
- Final normal-route `dicefree.slime.playtest`: passed with real 60-second staging, Divide, ordinary lure/capture, puzzle, normal boss, reward and return lifecycle.
- Final secret Regent-route `dicefree.slime.playtest --regent true`: passed with real 60-second staging, class preparation, blue lure/capture/enchantment, Regent Divide/Roll/Bounce, reward and return lifecycle.

The camera-rendered staging capture was inspected at `Logs/PlaytestFixes-staging-dummy.png`: the straw target and bullseye are visible. Editor output is in `Logs/PlaytestFixes-Editor.log`. These are automated local-authority tests, not a live network transport test or final human art approval. Manual review should cover preferred idle strength, arm/socket clipping across equipment/attack poses, HUD layout at unusual screen sizes and party UX.

Unity preserved a startup recovery scene at `Assets/_Recovery/0.unity`; it was not removed or used to replace Cornberg.

## Repeat verification

With this project open in stopped Edit Mode, save any new user scene edits first:

```powershell
unity command set_autotick --enable true --project-path T:/TEMP/DiceFree-Options
unity command dicefree.playtest-fixes.install --project-path T:/TEMP/DiceFree-Options
unity command dicefree.playtest-fixes.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.playtest-fixes.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.playtest-fixes.status --project-path T:/TEMP/DiceFree-Options
```

Wait for each Play Mode suite to finish and return to Edit Mode before starting the next:

```powershell
unity command dicefree.slime.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --regent true --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.economy.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.village-art.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.village-art.status --project-path T:/TEMP/DiceFree-Options
```

Do not run the older scene/art generators to apply this batch.

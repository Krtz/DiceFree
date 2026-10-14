# DiceFree — October Gameplay Fixes: Verification (2026-10-14)

## Environment and scope
- Authorized Windows development PC: Kratz.
- Local Unity project: `T:\TEMP\DiceFree-Options`, Unity **6000.6.3f1**, URP.
- Branch during validation: `integration/world1-foundation`, prior head `db40b1b`.
- Scope: Oct 13–14 Trimery roads/banker approach, URP buildings, automatic revive stone, Slime Dungeon level-24 boss pursuit and 10x normal boss damage, Ironpine wildlife damage endpoints, minimap compass preferences; run broader regression and build.
- All game-play validation used isolated saves; no original user Echo progression intentionally changed.

## Bugs and final fixes
- Root-scaled dungeon minibosses could report valid pursuit paths yet remain effectively immobile. A 6.9x-scaled slime exposed a mismatch between the agent's local navigation radius and the actor's world-space combat footprint. `CombatActor.RefreshFootprint` now converts the world-space body radius to the scaled root's agent radius while keeping world-space physical/combat radii unchanged.
- `ActorFootprintSeparation` caches the agent's last position. `TraversalMotor.Teleport` and `TeleportAcrossMaps` now rebase that cache after successful intentional warps so a Slime Slam landing is not interpreted as a long normal-movement collision.
- The existing uncommitted Oct 13–14 gameplay changes are preserved, including Trimery navigation clearance/material conversion/beacon activation, explicit boss pursuit and boss damage multiplier, approved wildlife damage, and compass placement options.

## Unity live validations
- `recompile_status`: completed, `failed=false`, `compilationFailed=false`, zero errors.
- `dicefree.october-playtest.validate`: **DICEFREE_OCTOBER_PLAYTEST_DATA_OK**. Nine wildlife attack assets checked, five compass corner/off choices, v3 preferences migration and persistence.
- `dicefree.october-playtest.playmode-test`: **PASSED** in isolated Play Mode after focused repairs. All **9/9** Trimery walking waypoints completed (including former failing banker approach), **14** converted local URP materials checked with supported URP shaders; current Trimery beacon auto-activated; death-return restored player at the Trimery anchor.
- Same Play Mode fixture: both the **Miniboss** and **normal Slime Boss** actively pursued and landed melee hits on a **level-24 Novice**. Normal boss raw melee damage **40** and normal boss Slime Slam raw damage **180** (base Slam **18**, configured 10x). The final `status` returned `passed`, `error=""`, `walked=9`, `converted=14`, `bumpRaw=40`, `slamRaw=180`.
- `dicefree.trimery.validate`: **TRIMERY_VILLAGE_VALIDATED**, 3 gates / 6 guards / 7 buildings / 20 art slots / 6 routes.
- `dicefree.slime.validate`: **SLIME_DUNGEON_DATA_OK**.
- `dicefree.ironpine-wildlife.validate`: **passed**, encounter counts [10,7,4,1] and authored NavMesh spawn points.
- `dicefree.architecture.validate`: **DICEFRE_ARCHITECTURE_OK**.
- `dicefree.managed-references.validate`: **DICEFREE_MANAGED_REFERENCE_OK**.
- `dicefree.greatblade.prototype.validate`: **DICEFREE_GREATBLADE_VISUAL_AND_PROGRESS_OK**, 80 sampled visual poses and class progression checks.
- `dicefree.xp.quadratic.validate`: **passed**, formula `30 + 3*(level-1)^2`, save schema 6.

## Final Windows development build
- Command: `unity command dicefree.development.build-windows --project-path 'T:\TEMP\DiceFree-Options' --format json`.
- Result: **DICEFREE_WORLD1_FOUNDATION_BUILD / Succeeded**, **0 errors, 0 warnings**, **7 scenes**, reported size **317,880,024 bytes**.
- Output: `T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe` plus data/runtime files.
- Standalone batchmode/nographics startup smoke: Windows player process remained alive **15 seconds**; Unity 6000.6.3f1 engine, player networking, Input System and physics initialization appeared in `Library/WindowsBuildSmoke-20261014.log`; stopped test process after the interval. **Not a graphical or end-to-end standalone playthrough**.

## Remaining verification limits
- A manual Game view / shipped-player walkthrough is still recommended to judge material appearance (no magenta in actual pixels), real movement feel and camera/compass readability, banker interaction usability, visible stone effects and boss attack telegraphs at normal player speed.
- The focused harness verified pursuit/melee/Slam mechanics, not every possible arena line-of-sight, multiplayer behavior or long-running live boss fight.
- Raw builds remain ignored by version control; no third-party candidate ZIPs were imported.

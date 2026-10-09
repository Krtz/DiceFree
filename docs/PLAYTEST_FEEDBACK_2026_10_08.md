# DiceFree — 2026-10-08 hands-on playtest bug/feature batch
Source: user feedback after opening Cornberg and trying Slime Dungeon. **All items are accepted change requests**. This checklist is the source of truth for this batch. Do not overwrite previous Slime Dungeon, icon, economy or village art work.

## World, entry, visibility
- [x] **Portal placement critical:** the *correct* Slime Dungeon entrance is the existing authored cave/dungeon entrance beside the level-8 slime. Cornberg has object `08 - Slime dungeon entrance - exterior only` and another DungeonPortal. Connect the actual InteractionTarget/portal collider to the original entrance, and remove/deactivate any incorrectly positioned portal; pressing **I** or right clicking there must stage the player. Preserve cave art, terrain and nav.
- [x] **NPC collisions:** generic villagers currently appear to lack solid hitboxes. Give them appropriate capsule colliders, navigation obstacle/avoidance, and real click/interact hit targeting; avoid blocking trade UI, named quest NPCs, dungeon staging or save.
- [x] **NPC idle:** user saw NO idle even though `VillageVisualIdle` exists. Investigate binding/activation, transform model root and actual visible animated geometry; fix through playtest. Roaming NPCs should still wander in their small home area.
- [x] **Trees collidable:** trunks should physically block the player and remain reasonable for NavMesh and pathfinding. Use trunk colliders appropriate to art; not oversized canopy colliders. Preserve passable roads and accessibility.
- [x] **Tree occlusion:** do **not** zoom camera in when a tree blocks view. Make trees obstructing character-camera ray transparent/ghosted while blocking movement; restore opacity when unobstructed. Existing `CameraOcclusionFader` likely related.
- [x] **Camera zoom:** increase permitted in/out zoom range or strength; preserve player framing and obstacles.

## HUD, quests, menus, controls
- [x] **Quest helper:** completed quests disappear from active HUD quest tracker, remain available historically in journal. Quest ReadyToTurnIn still shown. Maintain saved progression.
- [x] **Quest/Journal button:** add button in existing menu/bag/skills group, opens browsable journal with active/completed quest details; close and input work.
- [x] **Minimap zoom:** add clickable + and − near minimap; change only minimap scale, not player camera or map heading; clamp.
- [x] **Options > Exit to Start Menu:** clear UI state, save, return to StartMenu, preserve character progression, no direct quit without confirmation.
- [x] **X attack-click behavior:** X should enter a *targeting reticle cursor* mode, not immediately attack selected target. Left-click enemy = attack target; left-click ground = attack-move to location, acquiring hostile nearby on approach, without disrupting right-click move/interact. ESC/cancel and UI clicks must be safe.

## Combat, balance, codex, character visuals
- [x] **Physically Blessed Novice:** baseline `-3 minimum auto-attack damage` and `+3 maximum auto-attack damage` versus its current attack range, with correct displays and no changes to spell damage.
- [x] **Overworld slime codex:** killing standard slimes counts toward monster codex, not just boss/campaign codex; ensure proper kill events from all relevant overworld slime prefabs and persistence, without duplicate rewards.
- [x] **Novice arm pose:** lower arms from near T-pose to more relaxed sides; preserve sword/armor socket positioning.
- [x] **Novice idle animation:** subtle breathing/weight shifting/arms; animate from correct rig in visual root, no collider/camera jitter. All regular slimes need clear attack animation; bosses need expressive Slam and Divide windup/execution/reform; all fragment slimes bigger/readable.
- [x] **Slime Dungeon miniboss:** 5× current health (NOT unintentional scale to fragments or changing thresholds). **Normal Slime Boss:** 5× current health; Regent health unchanged unless specifically asked.
- [x] **All slimes attack animation** (overworld and dungeon), boss abilities animated incl Slam/Divide; Divide splitting visibly shrinks/sections/recombine; split fragments significantly larger than current.

## Slime Dungeon staging
- [x] **Practice dummy missing visually/interaction:** ensure dummy exists in staging scene at reachable point, visible identifiable model, targetable and attackable, no XP/gold/items; appears for every entry.
- [x] **Start Now button:** visible during 60-second staging. Let staging initiator start early for party; lock roster and gear as if timer expired; do not cause duplicate starts or bypass occupancy; preserve existing 60s auto-start.
- [x] Enter, stage and clear via normal route and secret Regent path still need to pass existing tests.

## Economy
- [x] **Sell items to General Goods merchant:** existing merchant with Bronze Dagger should also buy carried inventory items for gold. Use real inventory removal + wallet grant atomically, confirm money display/limits, avoid selling bound/quest/equipped items when prohibited; provisional resale price (50% known buy price if no design yet) documented and configurable. No dupes with fast clicks or reload.

## Safety / verification
- Keep current `feature/54-world-dungeon-codex` branch, preserve all uncommitted 3D loot item prefabs, villager idle/wander changes, icon art and inventory UI.
- Do not git reset/restore user uncommitted work, commit, push or merge. Do not overwrite Cornberg with CornbergSceneBuilder.Build or art batch rebuild. Prefer idempotent scoped editor installers.
- Run Unity compilation and existing `dicefree.slime.validate`, `dicefree.economy.validate`, `dicefree.village-art.validate`, `dicefree.item-icons.validate` and dedicated Play Mode regressions as practical. Store results and known gaps in doc.
- Prioritize actual player-facing defects over low-value docs; if some items cannot be completed in one implementation batch, leave them visibly unchecked, report gaps and follow with further work.

## Implementation and verified results

All checklist items above are implemented and covered by the local automated checks described in [PLAYTEST_FIXES_2026_10_08_RESULTS.md](PLAYTEST_FIXES_2026_10_08_RESULTS.md). Both normal and secret Regent lifecycle tests passed. Manual art/UX review and live network transport testing remain separate from these local-authority results. Existing uncommitted art, icon, inventory and villager work was retained; no commit/push/merge or branch change was made.

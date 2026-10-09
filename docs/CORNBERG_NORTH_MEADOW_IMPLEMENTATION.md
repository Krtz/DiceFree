# Cornberg Northern Meadows & Eastern Slime Forest — installed October 9, 2026

**Branch:** feature/54-world-dungeon-codex. **Status:** scene geometry changes installed and validated in Unity 6000.6.3f1; no commit/push/merge. Preserve all prior uncommitted art, NPC, bank, game balance and inventory changes.

## User-approved layout (supersedes prior hybrid proposal)

The red-circled northern meadow, east of the stream and north of the settlement, is now agricultural land. The whole set of farmland and crop slimes moved north; **none remain in Cornberg's village blocks**. There is NO starter farm or crop slime retained inside the settlement.

- Northern field west: center **(20, 55)** in world XZ.
- Northern field east: center **(44, 55)** in world XZ.
- Two original field plots with crops and fences retained as original Unity scene objects. Preserved a total of 140 meshes/field pieces under two named plot groups; no duplicate rebuilt field.
- All five pre-existing crop slimes moved with their fields: (20,57), (41,55), (46,58), (16,54), (22,59). Existing actor identity, combat stats, IDs, death reporting and respawn behavior remain; respawn/aggro home position is captured from new transform in Awake.
- The existing **Crop Slimes** quest's dialogue and location hint now explicitly say to follow the path **north of Cornberg to the northern fields**. Same two stages: 3 kills followed by 2 kills. Quest IDs, reward XP and completion persistence remain unchanged. Changed both the live QuestDefinition .asset and the future source setup file.
- Old field positions beside the village are empty of crops and crop-slime spawns, freeing residential space.

## Eastern forest extension

The forest was expanded EASTWARD, not into the northern farmland. Existing woodland, paths, original dungeon entrance and its interaction were preserved.

- Added **six locally reachable Road Slimes** at (89,18), (101,17), (110,29), (91,43), (105,52), (119,58), using the existing Road Slime archetype, stats/respawn/kill-credit. This supplements the original Road Slime at (64,34) and distant woodland encounters, creating more nearby fights.
- Added **six distinct existing 3D tree variants**: spruce, twisted, willow, aspen, maple and oak, with collision capsules, carving NavMesh obstacles and child renderers intact. Kept corridors and combat clearings open.
- Added / preserved the eastern woodland fighting path and marker as authored scene landmarks.
- Relocated the **level-8 elite Forest Slime** from beside the dungeon cave (~77,53) to **(113,76)**, at a deeper east forest location. Dungeon cave remains at (81,59). The elite variant/stat/quest are unchanged; the named level-5 forest slime remains at its original location.
- The existing baked WorldNavigation data still provides **complete NavMesh paths from the village to both northern fields, east road encounters, elite and cave**. The newly reused trees carry runtime NavMeshObstacle carving; no destructive global NavMesh rebake was performed.

## HUD & input

- Player scroll-wheel zoom sensitivity **0.60**, increased from **0.35** (approximately 71% stronger), with prior camera occlusion fade and distance bounds retained. World scene camera updated; other dungeon/reward scenes have no standalone ExplorationCamera component.
- Minimap retains + and - independent zoom; new compact north-up **N/E/S/W compass with north arrow**. Uses ASCII letters and drawn geometric shape rather than unreliable Unicode glyphs.
- The original CornbergVillage.Fields authoring source now places field plots north, so it no longer describes the obsolete east-of-town location. **Do not rebuild the existing Cornberg scene using CornbergSceneBuilder.Build**, which would still discard extensive live scene embellishments.

## Validation

- Unity recompile completed, compilationFailed=false, errors=[].
- Scoped command `unity command dicefree.cornberg-expansion.validate --project-path T:/TEMP/DiceFree-Options --format json`: **CORNBERG_EXPANSION_DATA_OK**, 2 northern plots, 5 crop slimes, 6 local road slimes, 6 new trees, elite moved away from cave. `dicefree.cornberg-expansion.install` is idempotent: rerun returned alreadyInstalled=true without duplicates; validation repeated successfully.
- `dicefree.bank-balancing.validate`: BANK_BALANCING_DATA_OK.
- `dicefree.playtest-fixes.validate`: PLAYTEST_P0_SCENE_OK (9 NPCs, 530 trees with colliders).
- `dicefree.economy.validate`: CORNBERG_ECONOMY_VALIDATED.
- `dicefree.village-art.validate`: VILLAGE_ART_VALIDATED.
- `dicefree.item-icons.validate`: ITEM_ICONS_VALIDATED (8 unique 192px sprites).
- `dicefree.slime.validate`: SLIME_DUNGEON_DATA_OK.
- Isolated-save `dicefree.playtest-fixes.playtest`: **passed 854 checks** after the world changes, with no error.
- Calculated baked NavMesh paths from central Cornberg (32,2) to (20,55), (44,55), (89,18), (105,52), (113,76), and (81,59): **PathComplete for all six**.
- New tree prefabs inspected: six of six have at least one active non-trigger collider, enabled NavMeshObstacle, and visible renderers.

**Pending manual feel check:** Walk the farmer's quest after accepting it, inspect both field plots from the actual game camera, verify easy early-game travel and aggro spacing, and assess forest tree density and NPC/slime spawn visibility. Automated geometric/path checks do not prove attractive composition.

**Regent regression complete:** The fresh secret Regent dungeon isolated-save Play Mode run PASSED, phase 14, saveRoot `DiceFree-Slime-ffc0493594ab414486a54e302bda8bc9`, after the 16,000-HP Regent and 10%-HP (1,600 HP) fragments were implemented. The run completed staging, blue puzzle, Regent encounter, reward and return.

## Unity asset pack license clarification

Non-restricted Unity Asset Store assets may normally be incorporated, modified and distributed embedded in a standalone game under the standard EULA; they cannot be re-sold as raw packs and cannot be used for AI/ML model training without specific authorization. Third-party/restricted licenses can differ. Asset import still requires the owner to acquire the packs via Unity and inspect them. No listed third-party asset pack has been imported in this world layout pass.


## 2026-10-09 follow-up — move dense vegetation to the OUTER boundary

User asked to **remove dense vegetation blocking the newly expanded Slime Forest** and **box the newly added area in with dense vegetation instead**, noting they had added several Unity Asset Store packs to their account.

**Installed in saved Cornberg scene:**
- Deactivated the old slanting **Mossy ridge / Dense woodland** internal obstruction and its obsolete sign, leaving those original GameObjects preserved but inactive in the hierarchy.
- Cleared two broad woodland-tree connector corridors through the old seam at world X roughly 120–149, Z 23–50 and Z 59–78. The old art assets are preserved as inactive scene objects for later re-enablement or tweaking.
- Created `11 - Dense woodland outer perimeter` with **144 dense 3D trees** along two staggered rows each on the exterior **east (X≈243–249), north (Z≈177–184), and south (Z≈3–10)** sides. Reused existing 3D Cornberg woodland tree meshes/variants, textures and working tree collision/NavMeshObstacle components. Did not generate new art or use placeholder vegetation.
- Added three continuous, foliage-concealed **BoxCollider + carving NavMeshObstacle** perimeter segments at east X≈248, north Z≈183, south Z≈4 to stop actors squeezing through tree gaps. Western approach remains deliberately OPEN, connecting the older starter forest with the new eastern forest.
- The original crops, cave entrance, eastern road slimes, level-8 elite, farmhouse/NPCs, quests, paths, bank and combat tuning remain unchanged by this follow-up.

**Commands:** `unity command dicefree.cornberg-perimeter.install --project-path T:/TEMP/DiceFree-Options --format json` and `unity command dicefree.cornberg-perimeter.validate --project-path T:/TEMP/DiceFree-Options --format json`. Idempotency confirmed by second installation reporting **0 new trees, 144 total, 3 physical edges**.

**Post-install Unity validation:**
- `dicefree.cornberg-perimeter.validate`: **CORNBERG_OUTER_WOODLAND_OK**; old diagonal deactivated, 0 active blocking seam trees, 144 outer tree instances, 3 collision edges, PathComplete to east forest.
- `dicefree.cornberg-expansion.validate`, `dicefree.playtest-fixes.validate`, `dicefree.bank-balancing.validate`, `dicefree.village-art.validate`, `dicefree.economy.validate`, `dicefree.slime.validate`: all PASS after the changes.
- Isolated-save `dicefree.playtest-fixes.playtest`: **PASSED 925 checks** after new perimeter installed.
- Unity C# recompile completed with **0 errors**, Cornberg saved, editor back to Edit Mode.
- New boundary trees and walls remain scene-local and may be rearranged after manual visual playtest without replacing existing authored locations.

**New Asset Store packs:** At inspection time the **working Unity project** `T:\TEMP\DiceFree-Options\Assets` had only top-level `_DiceFree` and `_Recovery`, and Unity's `Packages/manifest.json` had no new third-party dependencies. The packs may be **added to the Unity account/My Assets but not imported into this project**. This landscaping change deliberately uses established project models; new packs should be separately reviewed and explicitly imported only after confirming their contents, style and licensing.

**Manual QA remaining:** Walk both forest connector paths and inspect the perimeter visually from ground level at player zoom. Automated tests prove presence/collision/reachability, not best-looking foliage density. Confirm frame rate in the new border (144 additional 3D tree instances).

## October 10 — Solid Forest, Shop Fronts and Line-of-Sight Fog V2

The Cornberg scene was revisited based on the in-game screenshots of the Brewery, General Goods, and dense woodland.

**Shops and village:** Brewery (-4,-14) and General Goods (17,-13) were turned 180° around their physical building footprints so the doors face the northern road. The Brewery's three original vats initially sat on its new entrance path, so they were relocated beside the east wall. The houses' world positions and other village systems remain intact.

**Tree collision:** Audited 640 active 3D tree instances across Cornberg, the expanded east forest, and the outer tree perimeter. All now have a solid, enabled `Playtest trunk collision` capsule; one previously uncollidable active tree was repaired. Existing narrow trunks and their `NavMeshObstacle` capsule equivalents were widened as necessary to a minimum **0.78m world-space radius** (1.56m diameter). Decorative canopies remain nonblocking; inactive boundary/seam trees remain inactive. This is intentionally trunk-only collision, not collision over an entire tree crown.

**Forest becoming invisible:** `CameraOcclusionFader` previously made every renderer on any tree along the camera ray nearly transparent at 22% opacity, even if far from the player. Now only foliage near the player (within 8m) can fade to ~58% opacity, and visible bark/trunks stay opaque. The old invisible-wall illusion (near-invisible trees but solid trunks) is avoided while trees can still fade if they actually obscure the player. Verified using an actual `capture_game_view --source camera` forest scene after teleporting the player to the woodland road.

**Fog of war now respects solid scenery:** `WorldFogOfWar` precomputes an obstruction grid from **physical enabled colliders** on tree trunks and named building walls. On a player-view refresh, cell-based line-of-sight blocks the area *behind* a building/tree while exposing the near face. Only actually visible cells are marked explored; the minimap and overhead labels continue using the same `IsVisible`/fog opacity state. This addresses the previous circular-reveal-through-houses bug. Visibility rays are computed on the existing 2m grid with a finite radius, and the screen mask is updated no more than once every 0.15s when moving.

**V2 testing:**
- `dicefree.fog.los.runtime-test`: **PASS**. In Play Mode, 651 physical obstruction objects were indexed; a target near a house was visible, the opposite side hidden and not prematurely explored, and at least one actual tree blocked LOS. All 640 active trees had solid trunk colliders.
- `dicefree.forest.sight.validate`: **PASS** for both road-facing shop doors, clear Brewery vats and all 640 physical tree trunks.
- `dicefree.cornberg-perimeter.validate`: **PASS** for physical forest edge, reopened former diagonal seam and eastern NavMesh.
- `dicefree.cornberg-expansion.validate`: **PASS** for roads, crops, slimes and landmarks.
- `dicefree.visual.screenshots-validate`: **PASS** for the earlier bridge, houses and pink material repairs.
- `dicefree.fog.cornberg.validate`: **PASS** for serialized player/camera binding; live LOS checks are intentionally in Play Mode.
- Unity C# compilation: zero errors or warnings.

**Scope still to build:** persistence of explored geography across save/scene loads; line of sight based on elevation/cliffs (current v2 uses level, static blocker footprints); integration across other map scenes; multiplayer party vision; perf profiling at max zoom.

Visual checks: `docs/screenshots/CornbergShopFrontsV2.png`, `docs/screenshots/CornbergForestSolidV2.png`, `docs/screenshots/CornbergFogOcclusionV2.png`.

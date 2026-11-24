# World 2 frontier expansion — 2026-11-23

## Saved content

- `WorldTwoArrival.unity` now contains five Cinderwatch buildings, a solid cedar palisade and three open gates. The north arrival road, south landmark trail and western road pass through the gates.
- Steamroot Vale is the new broadleaf river biome west of Ashfall Reach. Willowford has seven buildings, three residents, an outfitter with six existing shop offers, a healing spring and a walkable timber bridge. Both settlement names are implementation choices for this request.
- The Cornberg river continues across the shared edge. World 1's river center is `-18 + sin(z * 0.065) * 2`; its southern edge is `z = -53`. World 2 preserves the lateral offset from the Ironpine gate at `x = 163`, placing the continuation approximately 180 metres west of its arrival. Water height remains `-0.386`.
- Basalt Needle and Ember Crown remain the two World 2 pip landmarks. The larger region, roads, river, town and merchant appear on the atlas. The minimap changes its region title in Steamroot Vale.
- Production travel now pulls the camera back, turns a rendered cube through 90 degrees, and flies into the destination. Return travel reverses the turn. The same hero is retained. Actions, movement and loadout changes are held during travel, the hero is protected, and controls resume afterward. Additive scene loading runs behind this presentation.

## Authoring

`tools/authoring/AshfallReachExpansionAuthoring.cs` and its `AshfallRiverlandsAuthoring.cs` partial rebuild the region using existing project assets. Navigation authoring checks the arrival, both pips, town, bridge, riverbank trail and every western road waypoint; road waypoints also receive physical obstruction checks.

`WorldFacePreviewAuthoring.cs` rebuilds the resource prefab and its landmark proxies. It preserves existing face photographs because Cornberg replaces legacy edit-mode surfaces during gameplay startup. Refresh those photographs with `tools/validation/CaptureWorldFaceTexture.cs` through Unity's runtime `eval_file` command while the hero is on the relevant face, then rebuild. The script writes into the enclosing Unity project and restores render state afterward. Capture under daytime lighting with the exterior world active.

## Verification

An isolated Windows development player passed 57 checks with no runtime errors. Checks included the real story-unlocked passage, both rotating travel directions, restored controls and protection state, menu-exit rejection during travel, all fifteen World 2 enemies, physical palisade collision, walking the complete western road, all seven Willowford buildings, the normal outfitter interaction, spring healing, bridge traversal, water blocking away from the bridge, exact river seam position and height, atlas display, reentry population and home-spell cleanup.

The route test found a granary intersecting the western road. It was moved behind the street before the successful run. Screenshots were inspected at the gameplay camera and during rotation.

The final Windows rebuild succeeded with zero errors. A further 21-check smoke test passed forward and reverse crossings using the final textures, with no runtime errors. Unity emitted one warning covering existing and new generated collision meshes whose collision prebaking setting will need attention for future Unity versions. Current player movement and collision checks passed.

Work remains local on `feat/verdant-wilds`; this expansion has not been committed or pushed.

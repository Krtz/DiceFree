# Ironpine Forest — World 1 northeastern route (first implementation slice)

Development branch: `feature/visual-overhaul-vis00`.

## Identity and geographic place

**Ironpine Forest** is World 1's cooler, denser Nordic pine-and-spruce forest **northeast of Cornberg's slime woodland**, leading towards the as-yet unbuilt **second village**. It is not the same biome as the later tribal quilboar territory. Preserve the user's rough draw.io reference as qualitative geography, not exact scale.

Ironpine encounters planned for the next creature/combat milestone:
- **Wolves:** fast pack hunters, occasional pack howl/lunge, stronger optional alpha
- **Quillrats:** *Diablo II–style*, small animal-like thorny rats/porcupines with ranged quill attacks, not upright humanoid quilboars
- **Tribal quilboars:** saved for the area beyond Village 2; NOT part of this forest's implementation

## Installed and saved in Cornberg

- New scene group: `12 - Ironpine Forest pioneer route`, separate from original Cornberg quest and forest hierarchies.
- Continuous, winding **9-segment road** following the existing navigable northeast woodland from approximately (112,43) through (183,98) to (223,149), with a dedicated material. Road has no collider and samples the pre-existing gameplay NavMesh; where the previous road has a `TravelSurface` road-speed definition the new path shares it.
- **79 additional evergreen trees**, favoring original existing pine/spruce/cypress 3D assets. These are full existing trees with their **solid trunk colliders** and **NavMeshObstacle** components. **21** previously obstructing trees along the planned route were deactivated, not destroyed.
- Custom, cool green evergreen material palette reused on new trees and selective existing tall trees in the northeastern woodland, leaving Cornberg's sunny palette intact. Corrected repeated material conversion to resolve canonical original tree materials: no magenta/pink meshes in final screenshots.
- Reserved **Village 2 approach clearing** centered near (223,149), with a modest northbound waypost and **no buildings or NPCs yet**. The town's layout, proper geography, professions and quests require further design.
- The original village, existing farms, dungeon, current Slimes, level progression, other world groups and save system are untouched by the new content. Fog-of-war bounds already cover the new approach.

## Actual Unity verification

- `unity recompile`: **0 errors / 0 warnings**
- `dicefree.ironpine.validate`: **PASS**, 79 trees with trunk collider and NavMeshObstacle, trail 442 vertices and complete existing NavMesh path (11 path corners) from existing woodland to future village clearing
- `dicefree.cornberg-perimeter.validate` and `dicefree.cornberg-expansion.validate`: **PASS**, preserves Cornberg forest access, perimeter and original enemies/quest geography
- `dicefree.ironpine.runtime-start` and `dicefree.ironpine.runtime-status`: **PASS** on final trial. Character actually traveled **16.7m** towards Village 2 with a complete NavMesh path. Initially automated test was stalled by the unfocused Unity editor not advancing gameplay; enabled background simulation for the test and verified real movement.
- Screenshots captured using a temporary orthographic camera in the actual Unity scene, manually inspected. Iterative defects corrected: road backface invisible, jagged road section joins, grass discs looking like dark polygons, recursive palette materials turning hot pink.

## Screenshots

- `docs/screenshots/IronpinePineRoad.png`
- `docs/screenshots/IronpinePineEntrance.png`
- `docs/screenshots/IronpineVillage2Approach.png`

## Known next steps

The forest geography and travel are implemented. Ironpine Wildlife v1.4 now has a first playable implementation of wolves, quillrats, Alpha AI and projectiles: see `docs/IRONPINE_WILDLIFE_IMPLEMENTATION.md`. Procedural creature models are temporary, full animations and animal sounds still need authoring, and extended combat/balance QA remains outstanding. Likewise Village 2 is only a reserved spot on the existing forest scene, not a complete settlement. Additional map space/terrain may be needed when the proper village is designed. Longer-term visual work should add pine needles/moss/rocks with *soft, properly-grounded* materials, stronger forest canopy layering, and sound ambience; do not reinstate the rejected hard polygon carpet decals.

`main` must not be merged without explicit approval. Preserve the user's unrelated local `ProjectSettings/PackageManagerSettings.asset` changes.

# Textured trees, day/night and moving shadows

Axel requested fuller trees and a day/night cycle with shadows following the sun and moon.

## Behavior

- 857 existing broadleaf/thornwood placements use locally licensed textured oak visuals. Four palettes, three LODs, leaf alpha cutout, bark normal map and AO. Conifers retain their original silhouettes for variety.
- Original tree art remains the portable fallback. Licensed visuals add no collider; existing solid trunks, obstacles, paths and navigation remain authoritative. Foliage still fades near the player/camera.
- Cornberg and both cube test maps have a 48-minute session day, initially 10:00. Inspector fields configure cycle length/start hour; SetHour is the test/host-clock seam. No options UI or production multiplayer synchronization is claimed.
- The single URP main directional light follows a daily orbit. Above the horizon it is warm sunlight; below it, the opposite orbit becomes cool moonlight. Direct light fades to zero at horizon handover; ambient/sky/fog blend through dawn/dusk. Nights preserve a navigable ambient floor.
- Clock time continues across scenes within a session, not across application restarts. It is currently session state, not save state. No gameplay rules depend on the hour.
- Slime Dungeon transitions hold the original fixed lighting and restore the outdoor phase on return; the clock continues while indoors.
- World camera and celestial light exclude portrait studio layer 31. Previously the HUD/target portrait directional lights leaked into world rendering and washed out nighttime contrast.
- Main shadow distance is 80m on the DiceFree PC/mobile URP assets. Moving soft shadows use one directional shadow map, rather than requiring a second directional shadow map for moonlight.

## Assets and reproducibility

See SourceArt/World/Environment/README.md and tree_source_receipt.json. Existing locally downloaded Stylized Forest Kit Samples, standard Unity Asset Store EULA. Exact package/selected-byte hashes retained; licensed source and generated prefabs remain ignored. No paid provider job. Game-dev unavailable; no canonical receipt claim.

Use dicefree.environment.install in Edit Mode to rebuild the optional prefabs and scene wiring. Runtime verification: dicefree.environment.test/status. Tests use an isolated save root, assert frames/time advance, verify all tree compositions, compare real GPU shadow-on/off frames at 09:00/15:00 and 21:00/03:00, capture the actual forest at noon/dusk/midnight, and walk the existing path. Shadow centroid displacement is evidence of rendered motion, not merely a changed light transform.

The screenshots are actual in-engine camera renders; test probes are separate temporary cubes/ground and are removed. Forest captures omit overlay HUD. Final results/build evidence are recorded under docs/validation/Environment-*.

## Current verification

- Environment suite passed 4308 checks: 857 tree adapters, advancing frames/clock, fixed interior lighting, forest walking, original-renderer suppression, portrait-layer isolation and distant landmark separation.
- Actual shadow centroid displacement: sun 136.06px, moon 136.06px on a controlled 960x600 GPU probe.
- Five-class gear/skills passed 297 checks, including mantle activation and world bounds. Mage passed 127, including actual dungeon local fog, fixed indoor lighting and expanded-bounds return. Cube edge passed 95 with real round trip and automatic seam crossing.
- Ten static regression commands passed; region separately validates thirteen creatures, sixteen connected paths and animated mesh deformation.
- Final seven-scene Windows Development build succeeded with zero errors and zero warnings; isolated 15s batch/nographics startup passed. Startup is not a graphical playthrough.
- Inspected actual noon/midnight forest captures and the final northern camp view. The old 130m distant city-tree trunk occupied the new camp; its visual trunk/bough/crown moved from z330 to z560, leaving terrain dressing and collision unchanged. It remains a distant future-city landmark.
- Screenshots and JSON are retained under docs/screenshots/Environment-* and docs/validation/Environment-*. Production multiplayer clock synchronization is not implemented.

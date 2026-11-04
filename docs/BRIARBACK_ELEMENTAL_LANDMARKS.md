# Briarback elemental landmarks

The Four Elements Hunt now leads from Trimery's scholar to four distinct clearings. The approach stones, trail standards and low altar repeat across the sites so players learn what to look for. The tallest pieces sit off the center line, leaving the guardian visible behind the altar.

| Element | Landmark | Read at gameplay distance |
| --- | --- | --- |
| Fire | Ember Oath Clearing | Dark basalt, red standards, ember-lit bowls and a low central flame. |
| Earth | Splitroot Cairn | Wide, fractured boulders, ochre seams and green stones on the altar. |
| Lightning | Stormcall Stakes | Leaning copper-bound conductors, violet standards and blue points of light. |
| Ice | Rimeglass Vigil | Tall clusters of pale angular crystals and blue-grey stones. |

The scholar's offer points to the first clearing, and each active objective names the next site and gives a direction relative to the last one. The existing quest panel now uses the HUD theme, opens above the lower HUD, and shows the current objective and actual reward parts. The quest identity, version, credit, turn-in and rewards are unchanged.

The art is original procedural geometry authored through Unity. `BriarbackFireLandmark.cs` records the geometry, palettes, placement and review commands. The old site geometry is preserved in inactive child groups. The scene adds no navigation obstacles or gameplay hazards; the guardian transforms, stronghold, roads and saved NavMesh are unchanged. The two scene recovery copies are local.

Compare the matched images in `VisualBaselines/BriarbackLandmarks/` from the saved gameplay camera pose. The fire gameplay and scholar-offer images are from live Play Mode, with the Editor NavMesh visualization temporarily disabled. The larger region, including the surrounding palisades, vegetation and roads, still has blockout art. The static ember/crystal treatment is a first pass; the ritual cave and dungeon are separate work.

Validation: Unity compilation; four site routes, collider-free landmarks and existing guardian spawns; all 31 isolated Four Elements Hunt checks; and live scholar interaction plus four actual player-camera approaches. The Play Mode camera review teleports only in its disposable save, while the separate regional regression covers ordinary walking.

The broader Briarback run passed 234 checks after its visual checkpoint and resume: 38 walks, 1,033m traveled, nine defeat fixtures, slam timing/damage, reward persistence and the equipped menu portrait. Its original twenty-creature fixture now also isolates the additive Chieftain group, as it already did for patrols and elemental guardians. The dedicated Chieftain validator passed independently.

The live solo review exposed an empty optional runtime family override after a script reload. That hid the guardian's valid definition family and prevented quest credit. `CombatActor.FamilyId` now falls back for both null and empty overrides, while retaining an explicit nonempty override. The final solo review exercised all three cases on each guardian, then all four ordered kills and the scholar turn-in. `tools/validation/BriarbackLandmarkReview.cs` reproduces this review with an isolated save; invoke its `Start` and `Run` entries through Unity Pipeline, then stop Play Mode. It temporarily hides Editor-only NavMesh visualization for captures and restores that setting afterward. The scripted defeat fixtures validate credit, not combat balance.
Final checks after the family fallback and quest-panel updates: the focused elemental quest suite passed all 31 checks again; architecture, architecture-policy and managed-reference validation passed. Unity returned ready in Edit Mode. No Windows build, multiplayer or balance approval is claimed.

## Ambience follow-up

Each site now has gentle standard movement. The runtime component clones the cloth mesh, pins the pole edges and restores the original mesh/material properties when disabled. Fire emission flickers, lightning details pulse gently, and ice varies slowly; earth minerals stay steady. The fire altar has one small upward ember emitter. The original source meshes and shared materials stay unchanged.

The isolated ambience check passed four wind cases, 48 pinned-edge checks, three emission cases, four disable/re-enable cleanup cases, the single bounded emitter and valid ember normals. The initial double-sided ember mesh had cancelled normals and produced a bright artifact with the lit shader; explicit normals fixed the preview. The ordinary solo hunt and turn-in passed again with all ambience running.

Runtime log review also found a pre-existing mantle material null-reference after script reload. BriarbackMantleVisual now uses Unity-object null checks when recreating its materials, rather than C# null-coalescing. A fresh isolated session after that fix added no exception/compiler-error lines to the captured Editor log.

A four-second sampled live Game View recording is saved as VisualBaselines/BriarbackLandmarks/ambience-preview.mp4. It uses a closer gameplay camera pose, within the existing zoom range. Navigation visualization and Editor gizmos are temporarily hidden during captures and restored afterward. This is visual evidence of the recorded session, not a performance benchmark.
After ambience: all 31 focused quest checks, architecture, policy, and managed references passed again. Unity is ready in Edit Mode.

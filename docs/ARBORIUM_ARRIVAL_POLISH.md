# Arborium arrival district and World Tree — November 5

This is the next visual milestone after `9241ffe`, on `art/arborium-arrival-world-tree`. The user approved polishing Arborium's arrival district and upgrading the World Tree. Guild interiors, a new quest chain and class mentors remain separate work.

## Visible changes

The World Tree keeps its established center near X98/Z305 and monumental scale. Original procedural geometry replaces the visible cylinder, straight boughs and spherical crown with a furrowed, tapering trunk, spreading roots, eleven main branching limbs, secondary branches and a layered canopy of individually shaped leaves. Bark grain and leaf-vein textures are authored from code. The old landmark objects remain in the scene with their renderers disabled. The existing trunk collision remains; roots are a separate render/occlusion group so their wider bounds do not make the whole trunk disappear unnecessarily.

The northern district gains striped cloth market awnings, produce trays, herb bundles, crates, barrels, guild tree banners, a patterned forecourt, planted urns and physical wayfinding signs. Existing market roof renderers are hidden where the cloth replaces them. New solid goods/urn footprints are included in the shared navigation bake; principal avenues and service approaches remain open.

The northern gate's original visible geometry is copied into compact left/right/arch render sections, with the remainder of the ring wall kept separate. A tower blocking the hero on the outside approach can now fade locally. Original gate collision and original scene objects remain intact. A focused default-camera check covers the outside approach, threshold and inside avenue.

Eight citizens reuse the project's existing rigged walking-villager presentation. They remain within small home patches and drive their walk cycles from actual movement. Two gate wardens use the existing guard model and short ambient directions. A porter brings an oak handcart along a short, reversible arrival route; its wheels rotate with travelled distance. These are ambient presentation actors, with no combat, loot or quest authority. Existing shop, bank, Registrar, captain and revive-stone behavior is retained.

A quiet original nonverbal street bed and distant guild chime complement the existing outdoor audio. The city layer fades by position, follows the current Ambience gain and existing master listener volume, and stops outside the city or when the scene is disabled. No voice recording, commercial pack or paid generation was used.

## Source and provenance

`tools/authoring/ArboriumPolishAuthoring.cs` generates the project-specific meshes, material variants, textures and PCM WAV files under `Assets/_DiceFree/Art/World/ArboriumPolish`. Seed 41057 defines the tree and decoration variation. Existing NPC models/controllers/materials are reused from the already-installed project; no external package was admitted. Scene and NavMesh recovery copies remain under the local Recovery folder.

## Verification

The disposable-profile Play Mode review verifies shader/mesh references, disabled legacy tree renderers, clone-local pedestrian rigs, actual movement and walk animation, home bounds, convoy travel, ambience gain/mute/exit behavior and nine real city walk segments. Its first run exposed one citizen home on the city wall. The corrected run observed all eight citizens moving, 566 walking-animation samples, 23.7 m of convoy travel with a completed stop, and 364.1 m of hero travel. Assertion totals include repeated polling checks; segment counts and observed movement are the useful evidence.

Captures under `VisualBaselines/ArboriumPolish` include ordinary gameplay views, High/Low comparison, evening light and additional presentation views of the tree and gate. The wider tree and trunk-detail shots are composition evidence. They do not replace the gameplay-camera checks.

The original four starter-village walker validation now explicitly selects those four original NPC roots. Its rig, looping-clip and deformation checks remain intact; new city pedestrians are verified separately in Play Mode. Architecture, managed-reference, static asset/audio checks and standalone results are recorded in `validation/arborium-arrival-polish-20261105.json`.

The final seven-scene Windows Development build is `Builds/BriarbackMilestone-20261105-183232/DiceFree.exe` (459,182,239 bytes), with zero build errors and warnings. Its error-gated standalone smoke passed the new geometry/life/ambience checks, original quests and rewards, services, save/resume, five class presentations, combat effects, eight fort walks (214.7 m), Development Room return and normal menu Quit with exit code 0. The Player log had zero exception, shader, assertion or protected-mesh-access matches. Final standalone captures are under `VisualBaselines/ArboriumPolish/Windows`.

The gate split preserves all 9,688 original gate/wall triangles and adds no collision. All 36,034 pre-existing scene object IDs were retained. All 54 unrelated modified tracked files and seven protected build settings remained byte-identical. The earlier `182036` build and its diagnostic/corrected QA runs are preserved. Its initial test queried read-protected NPC meshes; that inspection now filters readable meshes and the smoke requires zero observed Unity errors. No NPC mesh import flags were changed. The generated-asset hash/provenance record is `validation/arborium-polish-assets-20261105.json`.

Remaining work includes denser district population and facades, additional purposeful city activities, interiors, mentors and the separate direct-road/Roadwarden story. The capital is an ongoing city milestone. No push or merge is included, and the 54 unrelated pre-existing modified tracked files remain separate.

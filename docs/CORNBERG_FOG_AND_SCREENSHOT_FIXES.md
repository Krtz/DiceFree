# DiceFree — Cornberg Screenshot Repairs and Fog of War V1

Branch: `feature/visual-overhaul-vis00`. Review before merging into main.

## Photographed Cornberg repairs

1. **Northern bridge tree**: relocated the complete spruce `Woodland tree` grouping (existing visual mesh AND gameplay trunk collision) from its bridge overlap. The bridge walkway/rail and their NavMesh remain unchanged.
2. **Bridge NPC**: the static `NPC_Guard` at the *southern* crossing, originally at (-18,-7) inside the bridge rail, was moved to a NavMesh-validated nearby safe bank at approximately (-10,-8). This is a scene-placement fix, not NPC AI/path regeneration.
3. **Three southern homes**: the two southern cottages at (-5,-29), (12,-29) and the abandoned locked house at (36,-26) had all authored child transforms turned 180° around the respective building centres so the main doors now face north toward the road. Service buildings occupied by merchants/quest NPCs remain at their validated original positions.
4. **Pink paths**: `North meadow farm path` and `Eastern woodland fighting path` had null mesh-material references (Unity magenta). Both now use the existing valid URP `Warm earth` road material.
5. **Giant green obstruction**: deleted `Distant wooded hill`, a 120×30×100 green sphere centred near (170,-5,160) which hid eastern playable scenery. The distant Great Tree landmark, playable terrain, and nearby vegetation were preserved. Unity before/after captures verify its removal.

These changes are idempotently installed by `dicefree.visual.screenshots-fix` and checked by `dicefree.visual.screenshots-validate`. The giant hill is removed by `dicefree.visual.remove-green-hill`. Camera baselines come from `dicefree.visual.capture-regions`.

## Warcraft III-style exploration fog (Cornberg prototype)

Installed the `WorldFogOfWar` component in the Cornberg scene linked to the actual player and `ExplorationCamera`.

- Reveal radius: 24 metres around current player location; softened around the outer 3 metres.
- **Visible now:** no screen fog.
- **Previously visited:** dim blue-black fog at approximately 60% opacity.
- **Never visited:** near-black blue fog at approximately 96% opacity.
- Camera-space 160×90 bilinear mask obscures scenery *and tall actors*; terrain-only fog planes do not.
- Minimap follows the same reveal/exploration state and hides unrevealed edges even when zoomed out.
- `ActorFeedback` enemy nameplates and `InteractionPrompt` labels are suppressed outside current player vision.
- No gameplay colliders or gameplay-targeting changes.
- Fog mask recomputes at most every 0.15s, or less often when player/camera is unchanged.
- Cornberg uses approximate bounds X -70..275, Z -60..190 and 2m exploration cells.

**V1 limitations:** explored cells are kept only for the current scene session (not saved in character slots yet). No line-of-sight obstruction by trees or walls, shared party vision, NPC intelligence/stealth, dungeon fog, or fog-aware all-world quest indicators yet. This is the first playable exploration pass, with a future persistent-map and LOS pass planned.

## Verification

- Unity C# compilation passed with 0 errors and warnings.
- `dicefree.visual.screenshots-validate` PASS (tree, guard, houses, pink path materials).
- `dicefree.fog.cornberg.validate` PASS (main player binding, radius, distant visibility).
- `dicefree.cornberg-perimeter.validate` PASS (144 perimeter trees, east forest NavMesh complete).
- `dicefree.cornberg-expansion.validate` PASS (crop fields, slimes, eastern trees, elite/cave).
- Captured and manually inspected real Cornberg scene screenshots, including removal of the giant green sphere.
- **Entered Unity Play Mode and captured the actual running Game view with new fog and minimap fog active.** The far quest-NPC text outside vision was subsequently corrected and verified in the final Play Mode screenshot.
- Further gameplay/UX review and frame profiling are still warranted before main merge.

Screenshot: `docs/screenshots/CornbergFogOfWarV1.png` and `docs/screenshots/CornbergEastHillRemoved.png`.

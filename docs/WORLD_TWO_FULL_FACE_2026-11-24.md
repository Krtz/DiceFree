# World Two full-size face — 24 November 2026

World Two now has the same **438 × 615 metre footprint** as the current World One `OverworldMapBounds`, rather than the former 290 × 190 metre arrival area. This is approximately 4.89 times World Two's previous bounded area. Terrain, collision, navigation, camera bounds, exploration, atlas routes and the rotating-world presentation use the enlarged dimensions.

## Two world-scale pips

The northwest and southeast quarter positions form the two-pip pattern across the complete face:

| Landmark | World Two local X/Z | Normalized X/Z | Scale relative to previous landmark |
| --- | --- | --- | --- |
| Basalt Needle | −138.5 / −118.75 | 0.25 / 0.75 | 3.4× |
| Ember Crown | 80.5 / −426.25 | 0.75 / 0.25 | 4.4× |

The Needle is a tall basalt spire cluster beside southern Willowford. The Crown is a large volcanic cone in the southeast, with its rim joined to the terrain by a talus apron. These landmarks remain opaque; tree canopy fading continues on vegetation. The forest terrain retains its green colour on the new rolling hills.

Cinderwatch, Willowford, their residents, the bounty warden, resurrection dice and Vaelis retain their existing locations. The expanded land has a southern road circuit, a western river road, a second timber bridge, 612 additional trees and 34 additional Ashscale patrols. Eighty-five existing trees were moved away from the new roads and landmark footprints. The continuing river retains its original shared-edge alignment with Cornberg and now reaches the southern edge of the enlarged face.

Atlas forest marks and roads cover the larger region. Both rotating-world photographs were recaptured at the new frame dimensions. Rectangular map frames use separate X/Z scaling for their cube proxies so that photographed geography, pip models and the river seam align.

## Settlement healing

Cornberg's passive fountain remains **25 base HP/second**. Cinderwatch's spring now has the missing `HealingArea`, set to **125 base HP/second within 5 metres**. Willowford's spring uses the same World Two rate. Existing click-to-treat interactions remain available.

The live benchmark measured approximately 28.0375 and 140.1874 HP/second on the same saved hero, including that hero's Healing Received modifier: a 5:1 ratio. The test counted healing-event amounts against the actual frame durations of their ticks, excluding passive regeneration events.

## Validation and authoring

Evidence is saved in `docs/validation/world-two-full-face-2026-11-24/`.

- **32 standalone-player checks:** identical world dimensions, both pip coordinates, enlarged landmark bounds, full terrain/collision coverage, population, passive healing, measured healing ratio, ordinary movement orders to both landmarks, navigation to the outer areas and both bridges, retained village services/teacher, softened fog and zero runtime errors.
- **9 final-build checks:** restored save, normal world gate, completed rotating transition, final bounds, opaque landmarks, green native terrain texture, 125 HP/second fountain and zero runtime errors.
- The final Windows development build includes fourteen scenes and succeeded with zero errors and the existing collision-prebake warning.
- The actual working Unity project imported and compiled successfully. Its pre-existing Package Manager settings change was left outside this commit.
- Final atlas, transition, fountain and landmark captures were inspected. Landmark overview captures use a separate scene camera; the final transition and atlas captures come from the final standalone player.

`tools/authoring/WorldTwoScaleAuthoring.cs` is the one-time additive migration from the prior saved scene. It validates the World One dimensions, preserves existing towns/quests, bakes and checks navigation, and rejects duplicate full-face groups. Compile it alongside the existing authoring partials in the editor authoring adapter. `RefreshFullFacePresentation` can refresh the terrain palette and landmark presentation. `WorldFacePreviewAuthoring.Batch` rebuilds the cube after photographs are refreshed with `tools/validation/CaptureWorldFaceTexture.cs`.

The legacy arrival-region `AshfallReachExpansionAuthoring.Batch` refuses to overwrite a project that already contains the full face. No main-branch merge is part of this update.

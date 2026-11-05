# Arborium gates and walkable floors — 2026-11-05

The crossing green shape was the old `Distant wooded ridge` background primitive at (30, -6, 210). Its scene object is retained inactive. The World Tree and its sacred core remain in place.

Arborium now has monumental west and east gates, cut into the ring wall and connected to the existing radial avenues. Their towers, arches, banners and limestone/copper palette reuse the authored northern gate geometry, with independent camera occlusion sections. Obsolete wall/tower colliders in those openings and three eastern boundary rocks are disabled. Each gate has a walkable paved apron; the separate broken direct Trimery road remains broken.

| Building | Walkable floors, including ground |
| --- | ---: |
| Adventurers Guild | 4 |
| Northern Exchange | 4 |
| Artisan Workshop | 3 |
| Mage Observatory | 4 |
| West Residence | 3 |
| East Residence | 3 |

The former solid building boxes are retained with their renderers/colliders disabled. Hollow replacements keep their established footprints and open northern entrances. Individual stair treads, landings, continuous sloped navigation surfaces and upper-floor stairwell openings provide actual ascent/descent. Stairs use 1.88 m flights, 1.5 m turning landings and storey heights of 3.3–4.2 m. Oak planks, runners, window trim, stair rails and wall-side shelves establish the first interior dressing pass; these rooms do not add new quests or services.

`ArboriumInterior` cuts away the roof, higher floors and camera-facing walls while the hero is inside, and displays the occupied floor. Hidden overhead walkable colliders stop intercepting mouse input. Hidden solid walls retain collision while the movement picker passes through them; queued moves and world-point targeting use the same picker. Baked stair navigation remains continuous while rendering changes, and exiting/disabling the component restores the shell. Floor navigation is rebaked into the existing Cornberg NavMesh asset. Walls, roofs and overhead slabs use obstruction fading outdoors; slabs exclude the building's interior footprint so the occupied floor stays opaque. Existing trees retain their previous fade behavior.

## Reproduction and evidence

- `tools/authoring/ArboriumFloorsAuthoring.cs`: guarded scene/mesh authoring, original palette reuse, gate UV preservation, original procedural oak texture, recovery copies, navigation rebake and bounded completion helpers.
- `tools/validation/ArboriumFloorsReview.cs`: isolated profile, actual hero walks through both gates in both directions, all six buildings/all 21 levels, descent and exit, precise vertical navigation sampling, picking/cutaway checks and gameplay screenshots. Fixture teleports only position the hero outside a building or gate, never between floors.
- `tools/validation/WindowsMilestoneSmoke.cs`: standalone gate/floor traversal, a Greatblade ascent/descent through the narrow residence, existing city services/quest/stone and broader progression/save/return regression.
- Local reports: `Logs/ArboriumFloorsReview.json`, `Logs/ArboriumFloorsAuthoring.json`; captures under `VisualBaselines/ArboriumFloors`. Standalone results live in the build's `QA` directory.

The prior 54 modified files and seven protected configuration paths are checked against pre-task hashes. Scene and navigation recovery copies precede authoring. Generated assets are original repo-native Unity meshes and texture code; no external art package was imported.

The isolated Editor geometry pass completed 281 checks, 37 real walking segments, all 21 levels and 829.6 m of movement with zero Unity errors. A separate controls pass climbed and descended the small residence using direct movement along 38 centre-line waypoints (158.5 m), verified oblique floor picking and solid-wall collision, and restored/re-enabled the full shell. Architecture, managed references, dependency policy and original villager checks passed. All 37,068 original scene object IDs were retained. Final standalone results follow below.

Final Windows build: `Builds/BriarbackMilestone-20261105-202953/DiceFree.exe`, seven scenes, 464,601,444 bytes, zero build errors/warnings. Its fresh `QA/Profile` acceptance pass passed both gates, all 21 levels and exits, occupied-floor opacity at every level, overhead collider picking, exterior slab fading, and a Greatblade ascent/descent through the narrow residence. Existing city arrival/report, bank/market, resurrection stone, four graphics presets, class motion/hand poses, combat, quest rewards, save/resume and Development Room checks also passed with zero observed Unity error events. Normal menu quit exited PID 24980 with code 0. See this build's `QA/player-smoke.json` and `QA/process-exit.json`.

Final standalone captures are the `windows-*.png` files in `VisualBaselines/ArboriumFloors`; the other images in that folder record the earlier Editor geometry pass. The final preservation audit records zero changes to the original 54 modified files or seven protected settings, zero original scene object removals, and no pre-existing WIP in the staged change. Work is local on `feature/arborium-gates-walkable-floors`; no push or merge is part of this milestone.

# World Two roads and asset-library pass

World Two's visible travel routes now consistently use the same **+15% movement speed** as World 1 roads. Ten surfaces were missing the component; all eighteen road, paved-area and bridge-deck surfaces now have it. Junctions select one speed source, so crossing overlapping roads cannot stack the bonus. Ordinary ground keeps its normal speed.

## Route coverage

| Area | Covered meshes |
| --- | --- |
| Arrival and Cinderwatch | Ashfall trail; Road to Cinderwatch; Cinderwatch to Willowford |
| Willowford | Bridge and riverbank trail; south street; timber bridge deck |
| Full-world travel | Basalt Needle road; Ember Crown road; southern world circuit; western river road; southern Steamroot bridge deck |
| Anvilreach | Crown trade road; northern trade road; foundry avenue; artisans lane; paved exchange; paved main avenue; paved guild road |

The footprint comes from the existing visible mesh rather than a large rectangular zone. The route components share `Cornberg dirt road.asset`; the change adds no trigger colliders or broad regional speed buff. Authoring helpers now attach the component whenever they create roads, paving, the arrival trail or bridge decks. Cloned southern bridge decks receive their own identity. `WorldTwoRouteAuthoring.Batch` repairs the saved scene and verifies the complete eighteen-surface roster.

## Library review and implemented replacements

Searched the local **240-package catalog** by environment function and model/prefab filenames, then rendered ten installed candidates in Unity. The catalog is an index of downloaded packages, not evidence that every package has been imported or visually approved. This pass used working assets already in the project's library and converted the market geometry into portable native Unity assets.

| Adopted source | Replaced content | Result |
| --- | --- | --- |
| Low-Poly Medieval Market: `stand_01.prefab`, `stand_02.prefab` | Three Anvilreach plain sales counters and three plain smiths benches | Actual stocked sword/shield/bow displays, with the original awnings and footprint blockers retained |
| CornbergBatch: `Tree_Willow.fbx`, `Tree_Birch.fbx` | Repeated oak silhouettes near Steamroot's river | Drooping canopies, silver trunks and a more recognizable river corridor |
| VisualBatch01: `tree_04.fbx` | Some repeated southern grove trees in Deepmoss | Branching, rooted silhouettes mixed with existing trees |
| VisualBatch01: `rock_04.fbx`, `rock_05.fbx` | Repeated round ridge stones and basalt boulders | More varied faceted shapes fitted to the original rock bounds |

Saved replacements: **6 displays/workbenches, 211 trees and 866 rocks**. Original building structures, service roots, tree trunks, solid blockers, navigation data and the two pip landmarks remain in place. Old placeholder renderers are hidden; replacement meshes add no colliders. The trees retain their original camera-occlusion component.

The market conversion writes 18 mesh assets, two URP materials and one atlas into `Assets/_DiceFree/Art/World/WorldTwoLibraryPass`. These assets have **zero dependencies on the private source-pack folders**. The raw downloaded archives and installed source prefabs are not added to this change.

Not selected: `THE_WOOD_CABIN_LOD0.prefab` has 39 missing mesh references in the installed project, so it cannot replace a functioning house. The two small VisualBatch01 bridges were previewed, but their short spans and rail proportions do not improve the existing 23m river crossings. `section_06` and the empty `table_01` were considered; stocked weapon displays better express this city's metalworking role.

## Strong candidates for subsequent replacements

These are catalog-level candidates, not newly imported or runtime-verified assets. Prioritize the first row for Anvilreach's next architectural pass.

| Pack | Specific candidates and intended replacement |
| --- | --- |
| lowpoly medieval buildings | `ironworks.fbx`, `smith.fbx`, `mine.fbx`, `quarry.fbx`, `warehouse.fbx`, `sawmill.fbx`: replace generic foundry shells, mine framing and repeated workshop cabins with recognizable industrial silhouettes |
| Stylized Medieval Building Pack 72 Modular Meshes Low Poly | Modular structures and under-pillars: larger named halls and distinct street fronts, fitted around current service approaches |
| Stylised Fantasy House / Stylized Fantasy House | Complete house models: review as named inn or wayhouse replacements, with the current building footprint retained |
| Cursed Cozy Village Free Pack | Street lanterns, hanging lanterns, windows, roof pieces, benches: replace plain town lighting and repeated facade details |
| AA Low Poly Medieval Environment | Broad tables, shelves, containers, bottles and bridge variants: workshop/inn interior dressing and settlement variety |
| Willowwood Free Low Poly Nature Pack | Willow, roots and bridge assets: richer Steamroot bank vegetation after silhouette and canopy-occlusion review |
| Kyles Rock Pack | Geological variants: Coppervein outcrops and Glasswind formations, while preserving the two world-pip landmarks |
| EMBERHOLD Free Low Poly Modular Dungeon Kit | Modular cell walls/rooms: a future architectural upgrade for the Crimson Crucible |
| Stylized Dungeon Props Pack | Props and ritual dressing: more individual boss-cell themes and dungeon entrances |

## Verification

- Windows development build: **Succeeded**, 15 scenes, zero errors, one existing mesh collision-prebake warning.
- **97 runtime checks passed**, including navigable footprint samples for every surface, more than 1,000 route centerline samples, actual actor speed measured with/without the surface source, off-road behavior, non-stacking junctions, and ordinary walking across both bridges and into Anvilreach.
- All 1,083 replacement roots have valid visible meshes/materials and no new collider components. Existing smith/banker/foreman roots remain present.
- Two additional checks passed for actual replacement-tree fading through the gameplay camera, without invoking the fade method directly.
- Static dependency review: 21 portable assets, no private source dependencies. Final authoring helpers compiled in QA. Main project import/compile exited zero with no C# errors.

These are automated development-player checks with a disposable copied profile. Screenshots render the actual gameplay camera explicitly in batch mode and omit overlay UI. No claim is made that all 240 catalog packs were visually reviewed or that this pass replaces every placeholder in the game.

[Reports and screenshots](validation/world-two-routes-library-2026-11-25/README.md).

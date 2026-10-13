# Foundation build warning cleanup - 2026-10-13

The previous seven-scene Windows Development build reported 45 warnings and zero errors. Cleanup is on integration/world1-foundation.

## Changes

- Runtime object searches use Unity 6.6's current FindAnyObjectByType and unsorted FindObjectsByType overloads. Existing active-object and unsorted selection semantics are retained.
- WorldFogOfWar's disposal helper no longer hides Object.DestroyObject.
- EscapeMenuController removes an unused bookkeeping variable while retaining all panel/cancel actions.
- Inventory development grants use Application.isEditor / Debug.isDebugBuild rather than the deprecated DEVELOPMENT_BUILD directive.
- Both procedural terrain meshes explicitly pre-bake triangle collision. Their authoring routines set that flag when rebuilding the meshes.
- The two Core GPU Resident Drawer debug shaders were tagged for HDRP in Core 17.6.0. Compatible, attributed copies preserve their shader code and target UniversalPipeline. URP global settings reference those copies.
- The optional, ignored village pack shader now guards its two Unity pass macro definitions. Only the original repair procedure is tracked; licensed shader source and its backup remain local.
- Pipeline Runtime configuration supports local diagnostics in Development builds. No ENABLE_RUNTIME_PIPELINE release opt-in is added. The custom builder sets the Development toggle during BuildPlayer so package build processors detect its intended mode, then restores the previous toggle.
- Player startup exposed additional URP warnings for stripped post-processing shaders. Unused post-processing shader stripping is disabled so the passes URP initializes retain supported shaders. Other unused-variant stripping stays enabled.

dicefree.build-warnings.repair reproduces the asset repairs and safely skips the optional village shader when that pack is absent. The Windows foundation builder calls the repair before building. No logging or compiler warning suppression was added.

## Validation

- Unity compilation completed without errors.
- DICEFRE_ARCHITECTURE_OK.
- DICEFREE_DEVELOPMENT_ROOM_PLAYMODE_OK: main-menu entry, real class/level/items/resources, same player across World 2 and back, scene unloading, isolated saves.
- DICEFREE_ADVANCEMENT_PLAYMODE_OK: normal Echo naming, advancement, equipment and save reload.
- TRIMERY_VILLAGE_VALIDATED: six complete routes, settlement services and revive anchor.
- Build and Player smoke results are recorded in foundation-warning-cleanup-report.json.

The earlier 45-warning build report is retained in docs/archive/pre-consolidation/foundation-build-report.json. Local Package Manager preferences remain outside this commit. Raw licensed art remains ignored. Main is unchanged.

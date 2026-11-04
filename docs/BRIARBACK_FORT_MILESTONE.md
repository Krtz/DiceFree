# Briarback fort, attack presentation and loading — 2026-11-04

The northern stronghold now has pointed, copper-bound stockade posts, braced towers with pitched timber roofs, an open gate header, red standards, plank bridge/ramp skins and rails, bridge braces, faceted cliff strata, a timber longhouse roof and a worn soil court. The arena stays open and visually quiet around the Chieftain and elite guards.

This is original Unity procedural artwork. `tools/authoring/BriarbackFortArt.cs` records its source and guarded authoring/capture commands. Generated assets are under `Assets/_DiceFree/Art/World/BriarbackFort/`. It adds 22 mesh batches and 21,892 triangles. Original collision objects, transforms, NavMesh and encounter placements remain authoritative; selected original renderers are hidden or given the new materials. New scenery uses Ignore Raycast and has no colliders or gameplay behaviours. The pre-edit recovery scene remains local at `Assets/_DiceFree/Settings/Recovery/Cornberg-before-fort-art-20261104.unity`.

The initial soil skin intersected the large terrace triangles. The final source samples the actual terrace collider before placing the skin 2.5cm above it. `RefreshCourtSurface` updates only the generated skin mesh. The older northern-fort validator's empty-arena capsule check now excludes `CombatActor` bodies while continuing to reject scenery blockers; it predated the additive Chieftain encounter.

## Attack presentation

`BriarbackChieftainEffects` builds an outlined red smash footprint with direction chevrons, a spinning forged axe with a short gold trail, and short stone/spark impacts. The warning remains 6m long by 1m wide, with the original victim-radius rules, 1.5s wind-up, 12s smash cooldown, 15s axe cooldown, homing sweep, damage and stun behavior. Impact effects add no collision or damage. Runtime meshes/materials belong to their effect root and are destroyed with it. Resource material templates under `Resources/BriarbackFx/` retain the shaders/variants for the development build.

`VisualBaselines/BriarbackFort/` holds matched gate/bridge/arena before/after renders and live Game View captures. `BriarbackFortReview` walks the existing gate, lower underpass, western bridge ramp, upper bridge, eastern landing ascent and arena. Its isolated fixture freezes opponent AI for traversal, then activates the real Chieftain for warning, impact and axe checks. It does not establish encounter difficulty.

## Shared loading screen

The previously local `LoadingScreen` is now a versioned reusable presentation layer for menu initialization, new Echo, saved manifestation resume, development-room entry and saved return to the main menu. Startup uses an animated indeterminate bar; scene transitions report Unity's async progress and remain covered through destination `Start` callbacks. A 250ms minimum display makes fast operations stable instead of flashing. Duplicate transitions are rejected, unavailable scenes report failure without leaving an overlay, and generated backdrop textures are released.

`tools/validation/LoadingScreenReview.cs` uses a disposable save to exercise initialization, new Echo, flush/return, resume, development entry, duplicate rejection, unavailable-scene rejection and delayed overlay cleanup. Its preview is `VisualBaselines/LoadingScreen/initializing.png`. Additive dungeon/travel state machines retain their existing lifecycle.

## Verification and Windows reproduction

The focused Chieftain suite passed 16 checks. The loading-flow review passed all listed flows. The live fort review passed eight actual motor walks over 216.27m, both bridge levels and the court; it checked the exact warning footprint, axe art, shader references and effect cleanup. Its fresh Editor log span had no exception/compiler-error lines. The northern validator passed six multilevel NavMesh segments, 156 court cells, open gate clearance and underpass headroom.

The broader regional regression passed all 234 checks on this fort/loading milestone: 38 walks over 1,033.79m, nine defeat fixtures, the existing slam damage test, reward/equipment persistence and the saved menu portrait. The focused elemental quest suite passed all 31 checks again. After the final accent winding correction, the live review passed again over 216.22m and explicitly checked that every warning mesh normal faces upward. The focused 16-check Chieftain suite also passed again. The last change before building only makes the transparent-material blend/retained shader variant explicit.

Unity normalized the previous milestone's generated clearing materials while importing/rendering this work (URP keyword, compatibility texture/color and surface/pass serialization). These owned generated-material updates are retained with the art milestone; original unrelated class/UI/settings work remains separate.

`tools/validation/BuildBriarbackMilestone.cs` creates a new timestamped Windows x64 Development build with the full enabled scene list, preserving older builds. It temporarily enables local Pipeline automation for that build and restores authored Pipeline configuration and transient resource files afterward. The build reflects this working checkout, including pre-existing unrelated WIP; it is not a clean release artifact.

Launch the resulting `DiceFree.exe` with `-diceFreeSaveRoot <build>/QA/Profile`, then evaluate `tools/validation/WindowsMilestoneSmoke.cs` via that Player's Pipeline `eval_file`. The smoke requires the exact isolated save location. It covers initialization, a new Echo, the ordered four-site hunt and scholar turn-in, both fort levels, built warning rendering, saved completion/identity after resume, development-room entry/return, overlay cleanup and the normal menu Quit action. Guardian defeats and player invulnerability are scripted fixtures; they do not validate combat balance. It writes screenshots and `QA/player-smoke.json` before requesting normal shutdown. Verify the process exit and Player log separately.

The verified artifact is `Builds/BriarbackMilestone-20261104-122641/DiceFree.exe`: seven scenes, 412,658,750 build bytes, zero errors and two warnings. The warnings concern automatic Read/Write support for the existing built-in Cylinder/Sphere particle meshes and automatic collision pre-baking for five existing fort meshes. Current Unity completed the build and both bridge levels worked in the Player. These future-import compatibility warnings are retained as follow-up work.

The actual Windows smoke passed, including all four ordered kills and scholar turn-in, eight walks over 216.34m, supported warning/axe shaders, saved quest completion and Echo identity after resume, development-room return and overlay cleanup. The normal menu Quit action was called, the process exited, and the log records Pipeline shutdown and Input System shutdown. There were zero exception/compiler/shader-error lines in that Player log. No numeric process exit code was retained. The startup/fort screenshots were visually inspected from the actual executable; selected copies are under `VisualBaselines/BriarbackFort/Windows/` and `VisualBaselines/LoadingScreen/windows.png`.

Use `unity command --runtime-path <build-directory> eval_file --file <absolute-script-path>` for reproduction. The initial typed test file could not resolve game assemblies in the Player's dynamic compiler; the final external runner uses reflection against the same public APIs. No game or build change was needed. Final source architecture, dependency policy and managed-reference checks passed. All 82 files in the pre-build protected snapshot remained byte-identical, including the unrelated WIP. Authored Pipeline runtime settings were restored to disabled after building.

Machine-readable results are in `docs/validation/briarback-fort-20261104.json`. The artifact includes the working checkout's pre-existing class/UI WIP and is a local Development build. Older builds, all recovery scenes and user saves remain preserved.

Multiplayer, long respawn timers, combat balance and graphics-preset/performance acceptance remain separate work. No remote push or main merge is part of this milestone.

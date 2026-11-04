# Shared loading screen

`DiceFree.ApplicationFlow.LoadingScreen` provides `Initialize(message, Action)` for startup work and `TryLoadScene(scene, message, onFailure)` for full-scene transitions. `Busy` prevents duplicate requests. The persistent overlay uses the DiceFree teal/gold presentation, an animated initialization bar, measured async scene progress and a 250ms minimum display. It remains through destination initialization, then releases its object and generated backdrop texture.

StartMenu uses it for startup, new Echo, saved manifestation resume and development-room entry. Saved returns use the same layer after the existing persistence/exit preparation succeeds. Additive dungeon and development-map travel retain their own state machines.

The disposable-save review is `tools/validation/LoadingScreenReview.cs`: run `Start` then `Run` through Unity Pipeline, then stop Play Mode. It passed startup/initialization, new Echo, flush/return, saved resume, development-room entry, duplicate rejection, missing-scene rejection and overlay cleanup. `VisualBaselines/LoadingScreen/` contains Editor and Windows Player captures. The Windows milestone journey also exercised these transitions through normal menu/controller APIs and closed through the normal Quit action. See `docs/BRIARBACK_FORT_MILESTONE.md` for the combined build evidence.

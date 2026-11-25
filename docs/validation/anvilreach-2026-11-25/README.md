# Anvilreach validation

Unity 6000.6.3f1; fourteen-scene Windows development player; copied QA save.

- `gameplay-review.json`: standalone service/navigation/biome review.
- `final-smoke.json`: delivered-build travel and scene-content checks.
- `build-report.json`: delivered Windows build result.
- `authoring.json`, `finishing.json`: authored counts and navigation checks.
- `source-receipt.json`: existing library inputs and hashes.
- `main-import.json`: main-project import/compilation result.
- Gameplay-camera PNGs show market, foundry, night lighting and biome trails.
  These exclude screen-space overlay UI.
- `anvilreach-overview.png`: separate high camera, not normal gameplay view.

The first 47-check pass was followed by 51 checks after paving, housing density,
market, collision and gate refinements. A wide render caught a native Texture2D
being incorrectly read as image bytes. This was fixed before delivery and the
map photograph was recaptured. Incorrect early previews are not included.

Automated gameplay and visual inspection are separate from user acceptance,
multiplayer playtesting or a performance benchmark. Existing collision-prebake
and tree LOD warnings are outside this change. Unrelated settings and recovery
scenes are excluded from the commit.

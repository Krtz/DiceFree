# DiceFree — Smooth world-space Fog of War v3

Branch: `feature/visual-overhaul-vis00`. Changes stay out of `main` until reviewed.

## Why this was changed

The prior `WorldFogOfWar` used a 160×90 screen-space texture drawn in OnGUI. Each screen pixel ray was intersected with the **ground plane**, even when the rendered pixel was a building roof. As a result, fog at ground coordinates got stamped as large black patches on otherwise visible house roofs. The low-resolution image and 2m visibility grid also produced jagged fog edges.

## Implementation

- Replaced the fullscreen IMGUI fog overlay with an original URP transparent world-aligned mesh at approximately 1.2m above Cornberg's ground. The material is `Resources/WorldFogOverlay.mat` using `DiceFree/WorldFogOverlay`. The material is serialized and included in standalone builds; no dynamic shader stripping risk. The mesh renders without a collider on `Ignore Raycast`, casts no shadows, and does not block player clicks or attack targeting.
- Renders a 512×384 world-coordinate fog texture over the Cornberg boundaries. Fog texture is bilinearly sampled with a **separable 7-tap tent blur** along each axis; smooth boundaries now cross approximately 1–3m instead of being square 160×90 screen tiles.
- Authoritative line-of-sight remains distinct from presentation: an approximately 0.85m cell grid checks house walls and solid tree trunks (651 indexed objects in the tested scene), preventing sight and exploration through occluders.
- The minimap now uses a 192×192 fog display sampled from the same smoothed world texture, instead of jagged raw LOS states at 96×96.
- Fog retains three states: visible clear; explored but not visible darkened (alpha about 155); never explored near-black (alpha 245). Current vision still updates every 0.15 seconds when the player moves.
- Performance optimization: rather than repainting all 196,608 texels, the system updates only the previous and new player-vision rectangles, including a blur halo, and uploads those partial texture regions. This also handles large-distance teleports.
- Known V3 tradeoff: the world-space fog lies below tall rooftops and tree crowns, so those upper silhouettes can remain visible in the distance. The ground and low actors are hidden/darkened correctly, and rooftops no longer receive splotchy ground-projected fog. A later depth/stencil-aware renderer could support physically accurate per-object visibility without roof discoloration.

## Verification

- Unity compilation passed with 0 errors, 0 warnings.
- `dicefree.fog.visuals.prepare` passed and confirmed the custom URP shader is supported.
- `dicefree.fog.visuals.runtime-validate` passed: a 512×384 world-space overlay with zero physical colliders and the correct shader.
- `dicefree.fog.los.runtime-test` passed: houses and actual tree trunks occlude line of sight; hidden terrain behind a house remains unexplored.
- `dicefree.fog.visuals.travel-test` passed: initial alpha 0; after traveling 60m old explored area alpha 155, new location alpha 0.
- `dicefree.cornberg-perimeter.validate`, `dicefree.forest.sight.validate`, and `dicefree.fog.cornberg.validate` passed.
- Benchmarked `RefreshVisionNow` average over 4 repeated calls in Unity Play Mode: **52.7 ms full-map prototype** versus **6.14 ms localized rectangle updates** (Editor measurements, not GPU/mobile benchmarks).
- Actual Unity Game view captured and inspected, including roof edges and smoother minimap.

## Preview

`docs/screenshots/CornbergSmoothFogV3.png`

## Future

Save explored geography between sessions, account for vertical sight and tall scenery, support party-shared vision, and do GPU frame-time profiling before adding fog to other world/dungeon scenes.

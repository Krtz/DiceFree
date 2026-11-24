# Pointer and target feedback — 24 November 2026

The pointer now has a red outline over enemies, a green outline over friendly
units and NPCs, and a gold outline over usable scenery and loot. Hovered targets
also receive a model silhouette and a ground footprint. The selected target
keeps a quieter footprint after the pointer moves away.

## Selection behavior

- Left-click selection, context attacks, NPC interactions, and ordinary unit
  spells share `WorldTargetPicker`. Corpse spells retain their existing dedicated
  corpse selection volume.
- Existing physical bodies remain selectable. Visible mesh geometry extends
  pointer selection to heads, limbs, and equipment outside those bodies.
  Readable meshes use triangle intersections, with the current skinned pose;
  imported meshes without CPU data use a bounded ellipsoid fallback.
- Actor fog visibility and hostile concealment still gate selection. Scenery
  rooted inside a tree or wall can use a visible part of its footprint or its
  approach point. Solid camera-ray obstructions still stop picking; walls cut
  away by the occupied building floor remain transparent to pointer rays.
- Collider-only dungeon entrances retain their interaction targets. Interaction
  availability, navigation, cast eligibility, and combat range keep their normal
  command checks.
- Hover feedback respects HUD coverage, input suppression, the atlas, death,
  focus loss, and component teardown. Armed skills own their targeting reticle,
  which uses the same enemy/friendly colour.

## Rendering

Renderer-only overlays share the model meshes and animated bones. An opacity
aware stencil mask limits the outline to the outer silhouette. Original model
materials and physical combat/navigation bodies are preserved. Footprint rings
follow the ground; actor rings show the actor's actual combat radius.

Layer 30 is reserved for these transient overlays and excluded from minimap and
world-face photograph cameras. Material resources keep both hover shaders in
standalone builds. `tools/authoring/WorldPointerFeedbackAuthoring.cs` regenerates
the three material assets through Unity.

## Verification

The standalone development player used an isolated copy of the Knight save.
`tools/validation/WorldPointerFeedbackReview.cs` records each successful check in
`pointer-feedback-review.json`. Its hidden window uses explicit pointer updates
to avoid OS focus changes. Left mouse events pass through the Input System;
context attacks and NPC interactions use their real command entry points.

All 31 runtime checks passed with no captured Unity runtime errors. The report
covers actor and NPC colours, animated geometry outside the body
collider, actual left-click selection, context attacks, skill cursor ownership,
solid-wall and concealment blocking, HUD protection, persistent selection rings,
unrigged merchants/bankers, usable objects, collider-only dungeon entrances,
World Two travel, cursor restoration, and Unity runtime errors.

The PNGs are player framebuffer captures and exact generated cursor textures.
Hardware cursors are outside Unity's framebuffer captures. These are automated
runtime checks and visual inspection, rather than a manual gameplay acceptance
test of every scene or class.

Build: `PointerFeedback-20261124-164552`, all 14 enabled scenes, zero build errors.
The existing warning about eight meshes without pre-baked collision remains.
The main checkout also completed a clean Unity import with exit code zero.
Unrelated local settings and recovery files were excluded from the change.

Evidence: [runtime report](pointer-feedback-review.json),
[build report](build-report.json), [friendly model](friendly-hover.png),
[enemy in World Two](world-two-enemy-hover.png), [usable object](usable-hover.png),
[red cursor](cursor-enemy.png), [green cursor](cursor-friendly.png),
[gold cursor](cursor-interactable.png).

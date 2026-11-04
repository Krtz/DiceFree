# Briarback clearing ground pass — 2026-11-04

The four elemental landmarks now sit in worn clearings rather than directly on the flat woodland material. Fire has dark ash, Earth warm ochre soil, Lightning muted stony earth, and Ice pale, uneven frost. Irregular soil edges fade into the original ground. Local road shoulders follow the existing detour, circular warband road and northern gate approach, using their original material colors. Sparse grass, ferns, pebbles and broken branches frame the sites; the centers and southern approach stones remain open.

This is original procedural artwork generated inside Unity. No external asset package or Warcraft III content was imported. The original terrain, road meshes, collision and saved NavMesh remain intact. Each site's added hierarchy is `Elemental clearing ground and trail dressing`, on Ignore Raycast with no colliders or gameplay components. The skin is 1.8cm above ground, rising to 12.3cm over existing road skins. The adjacent fort, boundary rocks and most of the wider world still use blockout art.

## Source and reproduction

`tools/authoring/BriarbackGrounding.cs` contains the source for the seeded soil textures, conforming meshes and detail geometry. Generated Unity assets and importer settings live under `Assets/_DiceFree/Art/World/BriarbackGrounding/`. Each site has one 1024px RGBA soil texture and seven mesh/material batches. Transparent URP Lit soil receives shadows; the opaque low plants have separate front/back vertices with valid normals. No new runtime script is required.

Use Unity Pipeline `run_script` with this file and an entry such as `BriarbackGrounding.Validate`, `CaptureBefore` or `CaptureAfter`. `Author` refuses existing output and first saves the local recovery scene `Assets/_DiceFree/Settings/Recovery/Cornberg-before-clearing-ground-20261104.unity`. `Rebuild` updates only the dedicated art assets and hierarchy after checking ownership. It preserves asset GUIDs and the original recovery copy. Run authoring only in clean Cornberg Edit Mode; capture another baseline before intentionally changing this art again.

`VisualBaselines/BriarbackGrounding/` contains matched `{element}-before.png` / `{element}-after.png` images rendered from the saved gameplay-camera pose and `{element}-gameplay.png` captures of the live Game View with its normal HUD. The static images show portable tree fallbacks; the live captures show the locally available tree presentation. No camera, lighting, quality preset or fog settings were saved as part of this work.

## Validation and boundaries

- Dedicated artwork validation passed at all four sites: seven meshes each, valid normals and materials, no colliders, no gameplay behaviours, all added objects Ignore Raycast.
- Existing site validation passed: four connected approach routes and no site collision. The elemental quest validator passed, including guardian spawn clearance and scholar access.
- The isolated Four Elements Hunt regression passed all 31 checks after the final art iteration.
- `tools/validation/BriarbackGroundingReview.cs` passed twelve actual motor walks, totaling 93.36m over 852 advancing frames. It uses a disposable save and temporarily freezes opponent AI for a navigation/visual review. It covers the southern approach, open center and side exit at every site. It restores AI, invulnerability and Editor visualization settings afterward; stop Play Mode after the review.
- The fresh Editor log span for that walking review contained zero exception/compiler-error lines.
- Final architecture, dependency-policy and managed-reference validation passed. Unity returned ready in clean Cornberg Edit Mode.
- Serialized scene audit against the preceding commit: no removed blocks; four existing site child lists gained the dedicated art roots. Unity also serialized URP additional-light metadata for the three existing ember lights during rendering. Existing actor, quest, transform, road and NavMesh data were unchanged.

The earlier 234-check regional run belongs to the preceding landmark milestone and was not repeated here. This pass does not establish combat balance, multiplayer behavior, graphics-preset coverage, performance measurements or a fresh Windows build. The loading-screen WIP remains separate. The next substantial art area is the northern fort: cliffs, ramps, bridge, gate and Chieftain arena.

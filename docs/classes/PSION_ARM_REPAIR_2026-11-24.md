# Psion Tier III arm repair — 24 November 2026

The first implementation screenshot still showed broken arm presentation. Its automated combat/progression checks did not catch that visual defect. This follow-up supersedes the original arm-presentation claim and screenshots.

## Cause and repair

The humanoid avatar described a T-pose while the original mesh was still bound in its A-pose. Retargeting therefore folded sleeve and hand geometry into the torso. The shared caster animation also applied arm twists and clasped-hand poses that did not fit these two skeletons.

- Bake the posed skin into a matching T-pose mesh, preserving topology, UVs, materials and skin weights; recreate its inverse bind matrices in the same space as the avatar.
- Retarget both arms from their own reference rotations, with separate positions for the main hand and offhand. Standing, movement and casting use fitted arm poses; the existing controller still drives the body, legs and action timing.
- Use an arm gesture during casting rather than retaining the incompatible arm twists. Death and two-handed stances keep their existing controller behavior.
- Calibrate attachment positions from skinned hand geometry. These rigs' hand pivots are offset from their visible palms, so pivot-based sockets left items floating.
- Refit the sword, book and smaller Ionic Stones. Use the same pose code in gameplay, HUD portraits, menu previews, teachers and reflected models.
- Keep the generic offhand correction from also modifying these already-retargeted arms. Other classes retain their existing behavior.

The source FBX files and admitted canonical GLB packages were preserved. The coherent Unity skin derivatives are saved beside the class assets and included in the native derivative hash manifest.

## Evidence

Unedited captures and reports are in [the arm-repair validation folder](../validation/psion-arm-repair-2026-11-24/).

- 97 pose checks across both models: equipped hands, valid avatars, stable segment lengths, torso clearance, three samples each of idle/movement/cast/death, and front/side renders.
- 29 save/menu checks using an isolated copied Echo, including both equipped class previews and restored progression.
- 11 live-hero checks: normal class switching, walking orders, self-spell casts and runtime presentation on both characters. Live gameplay and HUD screenshots are included.
- The repaired standalone build succeeded with zero errors and the existing collision-prebake warning. The actual working project also imported/compiled with exit code 0 and no C# compiler errors.

All final reports contain zero runtime errors. The close-up renders were visually inspected; the check counts alone are not a claim of visual correctness.

Scene integration replaces only task-owned class fittings and teacher instances. No world objects predating the Tier III implementation were removed. Original scene document order and unrelated working changes were preserved. Project settings remained byte-identical across the final import.

Implementation remains local on `feat/verdant-wilds`; no commit, push or merge was performed.

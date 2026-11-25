# Marksman delivery evidence

[Implementation, teacher, quest and combat formulas](../../MARKSMAN_IMPLEMENTATION_2026-11-25.md).

Windows executable: `T:\TEMP\DiceFree-Roadwarden-QA\Builds\Marksman-20261125-235648\DiceFree.exe`.
Its exact hash and review provenance are in [delivery.json](delivery.json). The development build includes local runtime automation for reproducible review.

| Review | Checks | Evidence |
| --- | ---: | --- |
| Saved scenes, all rank gates, Mana profiles, model/provider and navigation registration | 198 | [Static review](static-review.json) |
| Real six-town bow quest, advancement, 150 purchases and combat packets | 426 | [Combat review](combat-review.json) |
| Actual disk reload, preserved Ranger, equipment transaction and death cleanup | 20 | [Reload review](reload-review.json) |
| Final rendered aura above raised paving, paired visible/control frames | 5 | [Raster proof](aura-proof.json) |
| Final World 2 routes, surface containment, bridges, paving and scenery regression | 97 | [Road review](world-two-roads-review.json) |
| **Total** | **746** | All passed |

The full combat review ran on build `Marksman-20261125-233955`; reload ran on `Marksman-20261125-235015`. Final delivery `Marksman-20261125-235648` adds only aura surface-height projection and draw order. The affected final rendering and World 2 travel surfaces were checked on that final build. Do not interpret this as 746 tests all rerun on the final executable.

The final build succeeded with 15 scenes, zero errors and one existing collision-prebake warning. The main project also imported/compiled successfully, with exit code zero and no C# errors. [Source receipt](source-receipt.json) compares 134 task-related source assets/scripts/scenes between the main project and the tested QA project, allowing only whitespace and Unity scene record-order differences.

Reviews use disposable copied save profiles and synthetic combat targets. Other classes and travel prerequisites were seeded for setup; bow quest credit and advancement use ordinary game transactions. No manual balance or live network co-op acceptance is claimed. Batch world-camera screenshots omit screen-space UI.

The before-migration Mana audit and exact authored teacher/target coordinates are in [mana-before.txt](mana-before.txt) and [placements.txt](placements.txt). Reproduce packaging with `tools/validation/PackageMarksmanEvidence.ps1`; review harnesses are adjacent in `tools/validation/Marksman*.cs`.

## Library character and final sigil

![Farwatch Marksman](marksman-level-one.png)

![Aelwen south of Willowford](aelwen-teacher.png)

![Final green and gold sigil above the road](marksman-sigil-visible.png)

The matching [control frame](marksman-sigil-control.png) disables only the sigil; the pixel comparison confirms 3,492 visible changed pixels.

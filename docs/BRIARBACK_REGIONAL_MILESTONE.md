# Briarback regional presentation and live assessment — 2026-11-04

This continues the elemental-site and northern-fort milestones on `art/briarback-elemental-landmarks`. Visual quality at the gameplay camera takes priority over file size. The ritual cave remains a separate future area.

## Connected regional scenery

`tools/authoring/BriarbackRegionArt.cs` records the original seeded Unity artwork under `Art/World/BriarbackRegion`. Five existing trails receive rutted dirt skins and soft shoulders that follow each original road's cross-slope. Low grasses, ferns and pebbles soften the road margins. Ground variation links the clearings while leaving the four elemental sites' ground artwork exposed. Faceted boundary stone replaces selected old spherical renderers; small camp stones and earthen hide-shelter colors connect the existing camps to the woodland palette.

The regional addition has twelve mesh batches and 110,812 triangles, including terrain-following feathered trail junctions. Existing route endpoints, ground and road colliders, NavMesh, camp collision, creature placements and encounter tuning remain authoritative. New scenery uses Ignore Raycast, with no added colliders or gameplay behaviors. Matched regional renders are under `VisualBaselines/BriarbackRegion/`. Recovery scenes remain local and are excluded from the milestone commit.

## Atmosphere and graphics controls

`BriarbackRegionalAtmosphere` adds a restrained warm woodland tint, earthier ambient colors and linear distance fog, blended by the existing fog revealer's position. Its edge blend begins north of Trimery and follows the region's bounds. It reads the shared 48-minute clock; it does not create a second clock. Interior mode and leaving the region restore the underlying lighting/fog behavior.

Graphics → Visual quality offers four persistent choices. The service clones the active URP asset for the session and restores the previous assignment on destruction. Shared project pipeline assets are not edited by player choices.

| Preset | Render scale | Shadow distance | Main shadow map | MSAA |
| --- | ---: | ---: | ---: | ---: |
| Low | 0.80 | 35m | 512 | 1× |
| Medium | 1.00 | 60m | 1024 | 2× |
| High (default) | 1.00 | 90m | 2048 | 2× |
| Ultra | 1.15 | 120m | 4096 | 4× |

These are image-quality choices, not hardware performance guarantees. They retain the source asset's soft-shadow support and gameplay attack warnings. Settings live in `Settings/graphics-quality.json` below the selected save root, or the normal application data directory. Editor validation uses a disposable settings directory. Unknown versions/presets and malformed JSON fall back to High.

## Creature motion

Seven editable Generic-rig clips and a separate controller are authored under `Art/Creatures/Briarback/RegionalMotion`, assigned to all 33 existing creature presentations, including inactive prototypes. They derive from the project's original creature FBX: idle, locomotion, melee attack, casting, throwing, overhead smash and death. Original clips/controller remain available.

`BriarbackMotionAuthoring` retimes the original curves and samples chest, head and arm accents, with two-bone arm placement for raised and forward-reaching poses. `QuilboarPresentation` chooses the appropriate role gesture and adjusts playback speed to the actual attack wind-up. Stun pauses animation, death selects its dedicated clip, and restoration clears the death presentation. Cosmetic projectile launch remains tied to the real basic-attack wind-up. No damage, cooldown, collision or AI tuning is changed by the motion work.

## Live combat and lifecycle evidence

`BriarbackRegionalLiveReview` starts from the isolated profile created by `BriarbackFortReview.Start`. It grants ordinary XP to a level-24 unarmed Novice and spends 24 available Novice skill points through the normal progression API. It leaves player invulnerability off, enemy AI active and time scale at one. Combat uses ordinary basic attacks, learned skills and their normal cooldowns, followed by movement beyond the enemy leash. A separate level-40 setup observes the real boss's guard engagement, strafes out of the smash and retreats down the existing fort route.

This is a bounded baseline assessment, not final class, equipment, party or multiplayer balance approval. The level-24 player needed to retreat after 9.26 seconds, with 211.20/490 HP remaining at the low point. Fifteen incoming packets applied 543.87 damage to the player, and nine outgoing hits applied 528.83 damage to the chosen Raider. Four skills were cast. Normal healing/regeneration means total damage can exceed the HP lost. The enemy returned home and healed normally. Both boss guards engaged and the movement dodge avoided the smash.

After combat, separately labelled scripted deaths exercise the actual common, Harrowtusk, elite-guard and boss respawn definitions at normal time scale. These fixture deaths are not counted as combat victories. The script checks full health, home position, cleared attack/return state and recovery from the death animation after each real timer.

All five fixtures passed at 1× time scale. Wall-clock observation was 91.29s for the 90s Raider, 181.25s for the 180s Harrowtusk, 301.29/301.34s for the two 300s guards, and 601.25s for the 600s Chieftain. The small wall-clock offset includes editor/frame scheduling; no delay definition or timer deadline was shortened.

The same isolated session checks all four quality values, save/reload, invalid-settings fallback, cloned pipeline ownership, unchanged shared assets, regional edge blending and interior/outdoor lighting restoration. `BriarbackRegionalCapture` records the actual gameplay camera at all presets, in daylight and moonlight, with editor navigation overlays disabled.

The final broad regional regression passed all 234 checks: 38 actual walks covering 1,032.95m, level-independent pursuit, 20.57 degrees of observed leg animation, elite dodge/interruption, quest credit, exactly-once rewards, saved equipped gear and the menu portrait. Its scripted damage/invulnerability fixtures remain distinct from the normal combat assessment above. Source architecture, dependency policy, missing managed references, the four-element quest and six multilevel fort navigation segments also passed. Separate sampled skinning checks found thousands of moving vertices in each of the six non-idle motion clips.

## Build repair and reproduction

`BuildWarningRepairs` enables collision pre-baking on the four fort mesh assets containing five collider meshes. It also enables Read/Write on two already installed optional particle-mesh model importers, preserving source FBX bytes and backing up importer metadata in `Library/BuildWarningBackups`. Those local vendor files remain ignored; the repeatable repair procedure is tracked.

The first new build exposed an Editor lifecycle issue: URP's build-data cache could retain the temporary quality pipeline after Play Mode destroyed it, even though both quality-level assignments were correctly restored. `GraphicsQualityEditorCleanup` disposes that transient Core build cache after Play Mode and before URP's build preprocessor enumerates assets. It uses the installed package's public API in an Editor-only assembly; runtime code has no Editor dependency. The targeted lifecycle test deliberately seeded the cache with the session clone, then verified that exiting Play Mode restored project-only cached assets and every quality assignment. The build runner now rejects a report containing errors even if Unity labels its overall result Succeeded.

Use `BuildBriarbackMilestone.Run` from clean Edit Mode to create a new timestamped seven-scene Windows Development build. Previous artifacts are preserved and authored Pipeline automation settings are restored afterward. The build includes the existing checkout's unrelated class/UI WIP and is not a clean release artifact. `WindowsMilestoneSmoke.cs` checks all four built graphics presets, initialization/loading, the ordered elemental hunt and turn-in, both fort levels, built warning/axe shaders, saved identity/quest resume, development-room return and normal menu Quit in that build's disposable QA profile.

Final validation and artifact results are recorded in `docs/validation/briarback-region-20261104.json` after the tests finish. No remote push or main merge is part of this milestone.

The final Windows Development artifact is `Builds/BriarbackMilestone-20261104-173129/DiceFree.exe`: seven scenes, 449,230,943 build bytes, zero errors and zero warnings. Authored runtime-automation configuration and the shared PC/Mobile pipeline assets were verified unchanged after building. The earlier `172451` artifact is preserved but superseded because its successful build still logged the Editor cache exception described above.

That exact final executable passed the four-preset save/reload check, loading/new Echo, ordered four-site hunt and turn-in, eight fort walks over 216.26m, built warning/axe rendering, saved quest/identity resume, development-room return and normal menu Quit. The retained process handle confirmed exit code 0. Its Player log contained zero exception/compiler/shader-error lines. Actual executable captures of the graphics options, gate, bridge, arena and smash warning were inspected and copied into `VisualBaselines/BriarbackRegion/Windows/`.

All 53 otherwise untouched files in the original 54-file WIP snapshot still matched their original hashes. The remaining file, `OptionsPanel.cs`, retains its original text and newline formatting with only the new graphics-controls hook added. Only that hook is staged from the panel; its pre-existing quickcast work remains unstaged. Local vendor files, recovery scenes, user saves and other unrelated changes remain outside this commit.

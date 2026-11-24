# World Two traveller feedback — 24 November 2026

World Two now uses the same softened exploration grid as World One. Previously its remote coordinates selected the dungeon visibility fallback, producing hard black wedges behind every trunk on the minimap. The fog component now retains a separate exploration grid for each overworld during the session; travelling between worlds preserves the areas already explored. Dungeons retain their existing local visibility behavior.

All 573 existing World Two tree instances register canopy occlusion, and 149 additional pines form a dense northern Steamroot boundary, with boulders and an opening for the continuing river. The southwest Cornberg mountain has additional corner ridge spurs; its existing advancement gate remains in place. The river crossing and both World Two settlements remain reachable.

Willowford has a resurrection dice (`anchor.willowford`) near its central road and spring. It uses normal proximity activation, the active-pip glow, map icons and death-return behavior.

The Cinderwatch Bounty Warden, inside the north end of the palisade, offers **Another 15 Ashscales**. Spearhands and ridgebows share one fifteen-kill counter through `ashscale.lizardman`. Each turn-in rewards **19,700 XP**, equivalent to twenty standard Ashscale spearhands. The existing nearby-party credit policy applies. World Two quest definitions also register during the normal crossing, allowing new regional offers to become available to existing manifestations.

Repeatable quests restored from a legacy `Completed` record now return to `Available` when their saved definition version matches. This fixes the stranded red-slime bounty without resetting active or ready-to-turn-in counters, changing version numbers, or granting another reward during loading.

The Psionic Knight's head and neck follow the model's own avatar reference rotations instead of incompatible caster head curves. The sword arm holds a bent-elbow forward guard, with movement and casting motion, while palm-based equipment attachment remains intact. Mirage Mage fitting and death/two-handed controller behavior are retained.

Reproduce the scenery and quest additions by running `tools/authoring/WorldTwoFeedbackAuthoring.cs` in a clean scene before this feedback pass is present. The pass refuses to duplicate an already authored group. Scene integration preserves the original document order and unchanged scene documents.

Evidence is in `docs/validation/world-two-feedback-2026-11-24/`:

- 31 standalone-player checks: actual world crossing, softened map samples, canopy fading, dice activation/glow/death return, both Ashscale types, bounty turn-in and reacceptance, restored World One exploration, legacy red-slime state and a complete repeat cycle, Knight head reference and zero runtime errors.
- 97 equipped model checks across both Tier III Psion models: idle, movement, cast, death, palm attachment, valid rig geometry and zero runtime errors. Front and side captures were inspected.
- A successful Windows development build with all fourteen scenes, zero errors and the existing collision-prebake warning.
- The actual working project also imported and compiled successfully. Its pre-existing Package Manager settings change was preserved.

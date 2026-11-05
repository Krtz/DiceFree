# November 5 playtest fixes

The requested changes are on `fix/november-playtest-feedback`. Existing working-tree changes are retained; Mage caster and tooltip edits are layered over their prior local changes.

| Feedback | Result |
| --- | --- |
| Mage cannot equip Slime Orb | Mage is explicitly allowed alongside Magically Touched Novice. Other class and two-handed restrictions remain intact. Equip and save/class-reload passed. |
| Hero still hidden by trees | Default blocking-tree opacity is 12%, capped at 18% for older serialized settings. Blocking trunks fade too. Canopy bounds determine proximity, so an overhanging canopy can fade even when its root is farther away. Temporary transparent materials disable source cutout and shadow casting; authored materials restore afterward. |
| Mage spell bases slightly low | Ember Bolt, Frost Snare, Arc Lightning, Quicksand and Half-Life have base damage 20 instead of 15. INT coefficients, ranks, resource costs and cooldowns are unchanged. Tooltips read the actual attack assets. Existing local rank-flat bonuses remain separate. |
| Quicksand should channel | Existing 4 + rank duration and one-second ticks now require continued channeling. Moving, attacks, another spell, stun, death, Stop/Escape and manifestation reset interrupt it. Queued successors wait for channel completion. The common channel/overhead bars read the Foundation contract; UI keeps its compatibility interface. |
| Bristlehide Mantle needs an icon | A transparent 256px sprite renders the original scalloped mantle mesh and bone quills; its item definition uses that sprite. No third-party art is introduced. |
| Briarback roads lack bonus | All five existing road footprints, including the stronghold approach, use the same +15% dirt-road definition as Cornberg. No new collision or NavMesh bake. The bonus disappears off-road. |
| Another 20 should be Another 15 | Displayed title/objective require 15. Internal asset paths and stable ID retain save/reference compatibility. Version-1 records migrate to version 2; 15+ existing kills become ready without requiring extra kills. |
| Bounty turn-in worth 20 kills | 5,160 XP, using twenty regular level-25 Quillboars at 258 XP each as the fixed reference. Actual kill XP varies by enemy; this is not a dynamic sum of the fifteen kills. Gold remains 50. |
| Four Elements Hunt | 2,500 XP on scholar turn-in, granted once. Gold remains 50 and dungeon eligibility still requires completion. |
| Switching resurrection stones | Cornberg's old activation centre `(8,0,6)` was three metres north of its visible die `(8,.03,3)`. Activation now follows the visible die with a 4.5m radius, matching Trimery. The separate navigable respawn position is retained. |

## Verification

`NovemberFeedbackReview` passed 208 checks in a new disposable profile. It used real quest acceptance, kill credit, turn-ins and XP accounting, with scripted enemy fixture kills and disabled AI; it is not combat-balance evidence. It tested five legacy bounty counters, three automatic stone-edge activations/death returns, a full Return Home channel to the selected stone, all five road bonuses, Mage Orb persistence, channel completion and move/spell/stun/attack/death interruption, and tree material restoration. Stop/Escape cancellation is wired through the normal input paths; those physical key presses are not part of this scripted review.

Clean gameplay captures under `VisualBaselines/NovemberFeedback/Clean` show the channel bar, mantle in inventory, and the same obscuring tree opaque and faded. Cyan NavMesh-overlay captures outside `Clean` are diagnostic only.

Architecture/dependency/managed-reference validation passed. The bounty validator was updated to exclude the separately added Chieftain trio from its original twenty-creature roster check; the old UI preferences test now reads the current schema instead of hard-coding version three. Neither correction changes gameplay settings. Final results and Windows build/smoke evidence are in `docs/validation/november-playtest-feedback-20261105.json`.

Final Windows artifact: `Builds/BriarbackMilestone-20261105-095007/DiceFree.exe`. All seven scenes built with zero errors and warnings. The isolated smoke passed the new visible-stone edges, five road bindings, Mage Orb, mantle icon and Quicksand channel/interruption, plus the previous five-class, graphics, loading, quest/title, traversal, death/return, save/resume and Development Room flows. Normal shutdown returned exit code zero; no exception, compiler-error, shader-error or failed-assertion matches were found in the player log. Windows channel/icon views were inspected and are under `VisualBaselines/NovemberFeedback/Windows`.

This Development build includes the pre-existing workspace WIP as found. Fifty-two original modified files remain byte-identical; the two overlapping Mage files retain their prior local changes alongside this work, with only the new changes staged. All seven protected build paths were restored. No push or merge is included.

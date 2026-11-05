# Tier II Briarback playability — 2026-11-04

Continues `5a5f7c4` on `feature/tier-two-briarback-playability`. The original 54 modified files are recorded in `Logs/TierTwoInitialWipHashes.json` and remain separate from this milestone. Existing skill/damage WIP is exercised as found, without being silently included in the commit.

## Gameplay changes

Greatblade's Wide Arc preview now reads `GreatbladeProgressionRules.MasteryArcWidth`, the same function used by hit resolution. The former preview widened at Mastery 3 while the approved hit cone widens at Mastery 5. Measured outline angles at ranks 3, 5 and 7 are now 110°, 125° and 131°. Damage geometry, reach, rank gates and tuning are unchanged.

`PlayerMotionPresentation` now covers Mage and Greatblade as well as the three early classes. Mage uses the existing original caster controller, casts a basic-attack gesture instead of punching, and uses a guard gesture for Math Barrier. Greatblade uses a new native fallback controller referencing the project's already tracked original Physical clips, plus one original fallback spin clip. When the optional already-installed Greatblade presentation is available, a session-only override reuses its armed ready, carry walk, swing and spin clips. No commercial clip/model is copied, imported or added to Git. Runtime overrides restore the previous controller on class exit and are destroyed with the session.

Greatblade's chosen swing impact pose is timed against the actual attack or ability wind-up. Wide Arc and Overhead Breaker retain their committed motion; Spin Dash keeps its own state while the real caster remains busy. The original fallback rotates only the visual child during spin and restores its rotation on cancellation. The installed two-hand grip solver remains responsible for palms and sword placement. Mage focus props and equipment rules remain under their existing owners.

Both Tier II forms now use the authored hit/death/recovery states, pause during stun and avoid whole-body squash. A new gesture gets a 120ms cosmetic transition before fresh movement can cancel recovery, preventing the movement frame immediately before `Stop()` from erasing a successful cast. This does not delay, block or change gameplay movement, damage, resource costs or cooldowns. The old presentation hooks defer to the shared motion driver to avoid two animation owners fighting over the same model.

## Advancement and equipment evidence

The authored rank/migration suite passed **411** checks, including rank-zero starts, approved schedules/caps, Mage control ceilings, once-only legacy reset and preservation of unrelated save sections. Greatblade's actual mentor/dungeon/turn-in suite passed **35** checks. Its playable combat/resource/collision/save suite passed **78** checks again after the final presentation changes. Mage's real five-ring quest, spell, Mana/Barrier/Weaving, gear, walking and persistence suite passed **127** checks after the changes.

The first Mage run stopped at an outdated assertion comparing the restored camera against the original smaller `BriarbackAuthoring.Bounds`. The test now captures the actual authored outdoor scene bounds before dungeon entry and verifies they are restored afterward. This was a stale test; it did not require shrinking or changing the expanded map/camera.

The separate field review also advances both classes from their level-30 parents, checks that they begin at class level 1, and uses real mentor acceptance/turn-in. Dungeon miniboss and Greatblade boss deaths are explicitly scripted objective fixtures; the Mage's five captures use actual spawned puzzle slimes and rings. These are advancement checks, not dungeon combat victories.

Equipment trials use existing legal gear. Both classes wear Cornberg Work Gloves, Farmer's Pants, Forest Travel Shoes, Slimy Tophat and Bristlehide Mantle. Greatblade equips Wolf-Fang Greatsword; Mage equips Bronze Dagger. The first attempted Mage loadout correctly rejected Slime Orb, which is restricted to Magically Touched Novice. The fixture was corrected; class restrictions were not broadened to make it pass.

## Regional combat observations

Four normal-damage encounters target the real level-25 Raider in its three-enemy camp, with the Spear Thrower and Totemcaller active. Player invulnerability is off, time scale is one, enemy AI is enabled, skills spend their real resources and cooldowns, and the player retreats along a real path afterward. Initial placement, ordinary XP/item grants and between-trial restoration are setup fixtures. Legal skill points are spent through each class's normal level/rank gates.

| Class | Class level | Fight duration | Minimum / maximum HP | Outcome |
| --- | ---: | ---: | ---: | --- |
| Greatblade | 25 | 2.26s | 905.45 / 1015 | Raider defeated; retreat survived |
| Greatblade | 35 | 1.29s | 1242.58 / 1315 | Raider defeated; retreat survived |
| Mage | 25 | 1.87s | 741.04 / 790 | Raider defeated; retreat survived |
| Mage | 35 | 3.28s | 919.11 / 1015 | Raider defeated; retreat survived |

These are one-policy observations, not equal-state DPS comparisons or win rates. Damage rolls, crits, control effects, available cooldowns and AoE influence the result. Defeating the Raider does not establish that the whole camp was cleared. Normal healing can make total damage differ from HP lost. No class stat, skill damage, enemy stat, loot chance, item stat or reward is retuned on this evidence. Elite, world-boss and party balance remain separate work.

## Focused motion checks

The field review separates its cosmetic fixtures from normal combat and disables enemy AI only for that later phase. Both forms passed real movement with changing leg bones, skill gesture, stun pause/recovery, settled corpse pose above ground, unchanged visual scale and actual delayed anchor return. Greatblade additionally passed all three cleave-angle measurements, installed spin state and original fallback spin cancellation. The cancellation test waits for the preceding installed spin to settle before comparing rotations; an earlier immediate pose snapshot incorrectly included its last animated frame.

`TierTwoImpactReview` observes the Animator at an actual connected basic hit. Greatblade was at normalized 0.472 versus its chosen 0.52 impact point; Mage was at 0.462 versus 0.565. The bounded tolerance allows normal frame and cross-fade scheduling; this is animation synchronization evidence, not a claim of exact sword/target mesh contact. The same fixture confirms Greatblade's temporary override is no longer assigned after switching to Mage.

Raw `Before` and `Field` screenshots containing Editor navigation overlays are diagnostics and excluded from acceptance evidence/commit. Clean Editor captures disable and restore those overlays; final Windows captures provide the actual gameplay-camera acceptance views. Original early-class screenshot baselines are preserved separately from the regression captures.

Final shared-driver regression, architecture/build validation and executable results are recorded in `docs/validation/tier-two-briarback-20261104.json`. The Windows smoke uses clearly labelled class/XP fixtures to check all five controllers, cleave geometry, Spin Dash, Mage Barrier, both Tier II corpse/return flows and the previous loading/quest/title/graphics/save/menu flows. Real advancement remains independently covered by the Editor suites and field review above. This is a local Development milestone containing preserved workspace WIP; no remote push or main merge is included.

## Windows milestone acceptance — completed 2026-11-05

`Builds/BriarbackMilestone-20261104-233417/DiceFree.exe` built all seven enabled scenes with zero build errors or warnings. The fresh isolated Windows smoke passed all five class controllers, both Tier II death/return flows, actual cleave outline geometry, Spin Dash and Barrier, plus the existing loading, feedback, quest turn-ins, honorific, graphics persistence, save/resume, Development Room and normal menu shutdown flows. Eight fort walks covered 216.17 metres. The player exited normally with code zero; its log contains no exception, compiler-error, shader-error or failed-assertion matches.

The five Windows captures in `VisualBaselines/TierTwo/Windows` were visually reviewed at gameplay-camera framing. Death captures show the themed return panel; the separate Editor rig checks establish the underlying settled corpse pose. Build-time configuration restoration passed for all seven protected paths. All 54 pre-existing tracked modifications remain byte-identical, and no scene or original early-class motion asset was changed by this milestone. Existing unrelated class/UI work is included in the executable as found, but excluded from this milestone's source commit.

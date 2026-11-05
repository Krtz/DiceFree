# Wolf Survival — Moonlit Clearing

User-approved on 2026-11-05: ten survival waves; weak wolves cost 1 point, strong wolves 2, alphas 3; the first wave costs 3 points and the tenth is exactly four alphas (12 points). The user selected a moonlit forest clearing in Ironpine.

## Wave-survival reference — clarified scope

The user clarified that the Fight Caves comparison refers to **wave survival**, and explicitly rejected a multi-hour grind. The intended experience is a compact ten-wave wolf challenge in the moonlit clearing. This supersedes the earlier endurance/attrition interpretation. Normal regeneration, consumables and class cooldown rules remain in place.

The 3–12 point budgets, both first-wave choices and four-alpha finale are unchanged. Preparation is now 30 seconds with immediate Start waves, between-wave breaks are four seconds with an initiator-only Next wave button, and victory returns after five seconds or immediately on request. Only a fully defeated pack can advance.

Two cover clusters provide real collision, blocked sight and navigable routes around them. Weak wolves press directly; strong wolves make one brief side approach as they close, then commit to attacking instead of circling indefinitely; alphas retain their existing pounce/howl mechanics. The HUD displays the living pack composition. Wolf stats and rewards have not been retuned in this pass, and no completion item has been invented.

| Wave | Points |
| --- | ---: |
| 1 | 3 |
| 2 | 4 |
| 3 | 5 |
| 4 | 6 |
| 5 | 7 |
| 6 | 8 |
| 7 | 9 |
| 8 | 10 |
| 9 | 11 |
| 10 | 12 |

`WolfWavePlan` generates reproducible compositions from a run seed. Wave 1 is either three weak wolves or one weak plus one strong; wave 10 always contains four alphas. The intermediate implementation introduces alphas at wave 4 and requires at least two from wave 7, choosing among exact-budget packs. These intermediate composition constraints are implementation tuning, not additional user-confirmed rules. Every run spends 75 points in total.

## Playable flow

- Physical entrance on the Ironpine trail at approximately **(174, 0.05, 89)**, labelled **Moonlit Clearing — Wolf Survival**.
- Reuses the default loading screen for additive travel to `WolfSurvival`, centred at (40000, 0, 40000), separate from existing Slime/reward maps.
- Thirty-second preparation with an invulnerable, zero-XP practice target and a **Start waves** button. Existing equipment/class preparation remains available during staging; the run locks the loadout when combat begins.
- Two-second warnings mark the occupied pack-entry trails. A wave clears only after its final wolf dies; a four-second break precedes the next wave. The HUD shows wave, point budget, living wolf count and overall progress.
- Existing level-15 Grey, level-18 Dark and level-20 Alpha wolf combat/stats/animations are reused as weak/strong/alpha, with arena-wide pursuit and the compact-pass tactics above. Dungeon wolves have separate content IDs, no respawns and no overworld item-drop component. Original overworld actors/assets remain unchanged. Existing ordinary kill XP and quest-credit rules apply once through the standard defeat pipeline.
- Victory after wave 10 gives a five-second return window or immediate **Return to entrance**. A solo wipe returns to the active resurrection anchor and resets the instance; re-entry starts at wave 1. Abandoning, death return and Start Menu/desktop exit release locks and the session lease. Dungeon coordinates are not saved as an overworld resume position.
- Uses the existing local-party/staging registry; this work does not establish production network multiplayer. No new boss, item reward table, quest unlock or bonus completion payout was invented.

## Visuals and provenance

Moonlight and trail lamps illuminate original duff/earth textures, faceted rocks, low ferns and fallen timber. Existing first-party Cornberg pine/spruce meshes provide the surrounding forest. The locally installed realistic wolf overlay and animal audio remain optional, with the established portable wolf rig/generated-audio fallback. The local catalog was checked for Low Poly Pine Forest, Realistic Furry Wolf FREE, EMBERHOLD and Stylized Dungeon Props; no new package import or raw vendor redistribution was needed.

The additive arena temporarily owns world lighting, preserves portrait lights, and restores the overworld on exit. Presentation changes do not change the overworld clock settings or authored assets.

## Reproduction and validation

- `tools/authoring/WolfDungeonAuthoring.cs`: guarded templates, original scene/art, separate NavMesh, entrance and scene-list addition; a Cornberg recovery copy precedes authoring. `Polish` records the original texture/prop generation.
- `tools/validation/WolfDungeonReview.cs`: 10,000 schedule samples across 1,000 seeds, exact budgets, both initial packs, four-alpha finale; actual entrance/loading/staging, natural wolf pursuit/bite, all ten waves, normal XP once, victory, wipe/re-entry, abandon and active-run menu cleanup.
- `tools/validation/WolfClearingReview.cs`: corrected practice target, real walks to all three pack trails, accurate warnings and final presentation captures.
- `tools/validation/WindowsWolfSurvivalSmoke.cs`: isolated-profile standalone acceptance and existing Slime Dungeon travel regression.

Lifecycle tests use explicit defeat packets to clear waves, with natural attack observations separately. They verify implementation and cleanup; they do not establish that a particular class/loadout can naturally survive the full dungeon. Existing wolf strength is preserved for player balance testing.

Initial Editor lifecycle validation passed 34,413 checks with zero Unity errors. The focused clearing review verified the actual practice target and 136.7 m of navigation through all three entry trails. Architecture, managed references, dependency policy and existing villager/city checks passed. Template dependencies were verified to reference the tracked portable `Wolf.fbx` and `AlphaWolf.fbx`; optional licensed models are not embedded in the new prefabs.

Final Windows build: `Builds/BriarbackMilestone-20261105-215901/DiceFree.exe`, **eight scenes, 465,508,437 bytes, zero build errors/warnings**. The final fresh-profile `QA/wolf-survival-smoke.json` passed ten waves, 75 points, four final alphas and 42 defeated wolves for that seed. It observed a natural wolf bite and alpha howl/pounce, checked ordinary XP once, staging equipment/active lock, lighting restoration, wipe/reset, active-menu exit, saved resume and re-entry, and the active Slime cave's staging/start/exit regression. There were zero observed Unity errors. Normal menu Quit exited PID 88460 with code 0 (`QA/process-exit.json`). Earlier harness-only attempts remain preserved in sibling `QA-before-*` directories.

Captures are under `VisualBaselines/WolfSurvival`; `final-*.png` shows the finished Editor arena and `windows-*.png` records the final standalone run. All 54 pre-existing modified files and seven protected settings remain byte-identical. All 41,022 pre-existing Cornberg scene object IDs are retained; the existing seven build-scene entries are preserved and WolfSurvival is appended. Work is local on `feature/wolf-wave-survival`; no push or merge is included.

## Compact pacing and cover follow-up — 2026-11-05

`tools/authoring/WolfCompactAuthoring.cs` preserves a scene/NavMesh recovery, adds cover using the existing original boulder mesh, rebakes navigation in place and updates the timing assets. `tools/validation/WolfCompactReview.cs` passed 441 checks, including 42.75 m of real movement around cover, blocked sight with a complete navigation detour, two observed strong-wolf flank attempts followed by committed attacks, live pack counts, guarded manual progression and no forced full heal between waves. Captures are under `VisualBaselines/WolfCompact`.

The standalone wolf acceptance harness now also checks compact timers, cover geometry/pathing, natural strong-wolf flanking, living point counts and manual/automatic wave progression. Its navigation fixture temporarily holds only the test run's preparation deadline and suspends test-player autoattack while walking the routes; authored timing values remain 30/4/5 seconds and the combat checks use the actual warning/intermission logic.

Final follow-up build: `Builds/BriarbackMilestone-20261105-231903/DiceFree.exe`, eight scenes, 465,517,477 bytes, zero build errors/warnings. Fresh-profile `QA/wolf-survival-smoke.json` passed ten waves / 75 points / four final alphas, observed the natural strong flank and alpha special, and passed compact timers, sight/navigation cover, live counts, XP once, wipe/retry, active-menu exit, saved resume/re-entry, lighting restoration and the existing active Slime cave regression. Zero Unity errors were observed; normal Quit exited PID 87976 with code 0. The earlier countdown-interrupted navigation fixture is preserved in `QA-before-navigation-fixture-fix`.

The follow-up preserves all 1,024 original arena scene object IDs, all 54 pre-existing modified files and seven protected settings. The NavMesh asset is updated in place. Final captures are `windows-*.png` under `VisualBaselines/WolfCompact`. This follow-up is local on `feature/wolf-survival-pacing-and-cover`; no push or merge is included. Complete-run class/loadout balance still needs player testing; existing wolf HP/damage and normal regeneration are unchanged.

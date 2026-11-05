# Wolf Survival — Moonlit Clearing

User-approved on 2026-11-05: ten survival waves; weak wolves cost 1 point, strong wolves 2, alphas 3; the first wave costs 3 points and the tenth is exactly four alphas (12 points). The user selected a moonlit forest clearing in Ironpine.

## Fight Caves reference — user clarification

The user subsequently described the intended experience as RuneScape's **Fight Caves, but with wolves**. Use its escalating endurance-trial structure as the reference: prepare before entry, finish the waves in one attempt, and make health, consumables, cooldowns, positioning and target choice matter across the whole run. Existing wave clearing, preparation, supply restrictions and wipe/reset behaviour provide the starting implementation.

Keep the confirmed moonlit clearing, ten waves, 3–12 point budgets and four-alpha finale. The analogy does not by itself approve copying RuneScape's wave count, prayer mechanics, Jad, solo-only restrictions or reward items.

Next design proposals, not yet implemented/confirmed: make intermediate packs more recognizable and learnable; provide terrain that supports deliberate positioning and separating enemies; sharpen the different tactical roles of weak, strong and alpha wolves; and choose a distinctive completion reward. Preserve normal class regeneration and existing consumable rules until any endurance-specific balance changes are explicitly chosen. No gameplay values change in this clarification.

Reference: [TzHaar Fight Cave](https://oldschool.runescape.wiki/w/TzHaar_Fight_Cave).

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
- Sixty-second preparation with an invulnerable, zero-XP practice target and a **Start waves** button. Existing equipment/class preparation remains available during staging; the run locks the loadout when combat begins.
- Two-second warnings mark the occupied pack-entry trails. A wave clears only after its final wolf dies; an eight-second break precedes the next wave. The HUD shows wave, point budget, living wolf count and overall progress.
- Existing level-15 Grey, level-18 Dark and level-20 Alpha wolf combat/stats/animations are reused as weak/strong/alpha, with arena-wide pursuit. Dungeon wolves have separate content IDs, no respawns and no overworld item-drop component. Original overworld actors/assets remain unchanged. Existing ordinary kill XP and quest-credit rules apply once through the standard defeat pipeline.
- Victory after wave 10 gives a fifteen-second return window or immediate **Return to entrance**. A solo wipe returns to the active resurrection anchor and resets the instance; re-entry starts at wave 1. Abandoning, death return and Start Menu/desktop exit release locks and the session lease. Dungeon coordinates are not saved as an overworld resume position.
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

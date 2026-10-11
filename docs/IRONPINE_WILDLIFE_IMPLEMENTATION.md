# Ironpine Forest — Wildlife Implementation (v1.4 first playable pass)

**Branch:** `feature/visual-overhaul-vis00`  
**Design authority:** Ironpine Wildlife Design v1.4 (13 approved interview rounds)  
**Implementation date:** 2026-10-12  
**Scope:** Cornberg scene's northeastern Ironpine Forest, not Village 2.

## Authoring and scene

- Saved scene hierarchy: `12 - Ironpine Forest pioneer route / Ironpine wildlife v1.4`.
- Exactly **22 spawned wildlife actors** with authored home locations: 10 Quillrats (level 12–16), 7 Grey Wolves (14–16), 4 Dark Wolves (17–19), and one level-20 Alpha Wolf.
- Every ordinary wildlife actor stores multiple nearby candidate respawn sites; the Alpha has four hunting grounds. Sites are sampled from the existing Cornberg NavMesh.
- Individual reusable attack/actor definitions and color materials under `Assets/_DiceFree/Settings/Ironpine`.
- `Assets/_DiceFree/Scripts/Application/IronpineWildlife.cs` implements `IronpineWildlife`, `IronpineBleed`, `IronpineQuill` and `IronpinePartyCredit`.
- `Assets/_DiceFree/Scripts/Tools/Editor/IronpineWildlifeAuthoring.cs` supplies idempotent editor install/validate commands; `IronpineWildlifePlaymodeValidation.cs` supplies a Play Mode initialization smoke test.

## Mechanics

- Wolves behave independently until a valid 25m howl: 1.2-second windup, 35s cooldown, up to two existing wolves investigate; responders cannot recursively recruit. Stuns cancel windup and enable retry after 10 seconds.
- Grey bite interval 2.2s / lunge cooldown 11s. Dark bite interval 2s / lunge 10s with +15% damage and 25% Bleed chance on a successful bite.
- Bleed: 5s, one tick/second at 8% of mitigated bite reference, max three stacks per victim; additional applications refresh soonest-to-expire. Persistence after the attacker's death.
- Lunges: 0.7s initial-position lock, 1.25m landing hit radius, visible red warning ring and arcing visual leap, using NavMesh-approved destinations.
- Quillrats fire actual collision-checked 15m/s homing projectiles (240°/s, 15m range, 8s max lifetime) every 2.8s and scurry 3–4m when threatened within 4m, on a 7s cooldown. Solid obstacles destroy projectiles.
- Alpha: 650% same-level HP, 150% basic damage; its 45s Frenzy alerts up to two wolves, while it and nearby wolves within 15m receive +20% basic damage and +15% movement for 10 seconds. Crushing Pounce: 16s cooldown, 0.9s warning, predicts a single fixed landing point, 2.5m radius; applies Bleed based on mitigated normal bite damage.
- Alpha physically roams four hunting grounds with 15–30s pauses, sometimes crosses road, and leashes to the 45m corridor around its route. It disengages after six seconds without line of sight/attack opportunity.
- Respawn delay 90s ordinary / 10 minutes Alpha. Eligible sites are at least 30m from all players and out of camera view. No duplicate Alpha or howl-created wildlife.
- XP and kill credit extend to eligible, nearby living local party members within 30m; personal observed mechanics enter Codex within 25m with line of sight. Exactly three families/pages: Wolf, Quillrat, Alpha Wolf. Alpha XP is 4× same-level ordinary baseline. **No item drops**.

## Validation performed

- Validated against an isolated copy at `T:\TEMP\DiceFree-IronpineValidation` because the user's current main Unity Editor process was timing out via the editor-command connection. The original editor was not terminated or restarted.
- Isolated Unity 6000.6.3f1 compile: wildlife runtime and editor scripts compile without C# errors after cached external assembly restoration. Existing editor code produces deprecated API warnings.
- `DiceFree.EditorTools.IronpineWildlifeAuthoring.Install`: generated and saved the scene + asset definitions in the isolated project.
- `IronpineWildlifeAuthoring.Validate`: **pass**, clean Unity batch process exit code 0, exact species quota and levels.
- `IronpineWildlifePlaymodeValidation.Run`: **`IRONPINE_WILDLIFE_PLAYMODE_OK`**, 22 living actors, all movement/navmesh components initialized, no `FixedWorldDrop` components, Alpha level 20. Required batch execution without `-quit`.
- A UnityEditor.Search.SearchDatabase startup `ArgumentOutOfRangeException` was logged during isolated Play Mode but the wildlife smoke marker was reached. This is separate editor search-index startup noise.
- Additional combat/quest/Codex end-to-end playtesting was **not** completed in this first pass. In particular, correct howl reinforcement, Bleed probability, 8-second projectile trajectories, party sharing and Alpha escape must be checked under real movement.

## Visual/audio state and remaining work

Current Grey/Dark/Alpha models and three Quillrat palettes are **procedurally authored 3D placeholders**, intentionally created without unlicensed third-party assets. The leap now has a red landing ring and arcing visual travel; idle bobbing is basic. Full authored quadruped skeleton animations (sniffing, sitting, scratching, foraging, grooming and hit/death animation), wolf howls/animal sound recordings, footfalls, sound falloff, hit VFX and detailed creature material polish are **not implemented yet**. Do not call these features complete. The 1.2s howl interruption currently uses the existing `CombatActor.CanAct` hard-disable path; if future silence/incapacitation differs from stun, extend it in the shared status system.

Important engineering follow-ups before calling wildlife shippable:
1. Playtest attack cadence and tactical telegraphs with actual Novice/Tier 1 characters and full party.
2. Verify Bleed tick math, interruption and target-loss edge cases across deaths, player reconnects and despawns.
3. Confirm XP/Codex group credit, especially multiplayer and per-player sight tests.
4. Author real creature rigging, body/leg/head animations and restrained positional audio.
5. QA scene placement, road bypasses, fog, tree collisions, FPS, navmesh roaming and out-of-sight respawn across multiple local cameras.
6. Verify ordinary HP/damage/XP calibration against the intended level-12–20 difficulty; numerical multipliers are wired, absolute balance provisional.

**Safety:** This branch does not merge to `main`; preserve unrelated `ProjectSettings/PackageManagerSettings.asset` worktree modifications.

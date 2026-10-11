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

## Visual/audio state (updated October 12)

Three independently authored Blender source models (Wolf, Quillrat, and AlphaWolf), each with a 15-bone rig and 10 actual animation actions, are **installed in all 22 Ironpine enemies**. Grey/Dark wolf coat variants and Moss/Amber/Slate quillrats have distinct imported URP materials, eye/face/subcoat/coat and quill colors. Real NavMesh Walk/Run, idle behaviors, melee Bite, howl, pounce/lunge, quill shot, Hurt and Death are driven by the wildlife/CombatActor code. Scoped spatial audio uses optional locally licensed wolf recordings with original generated fallbacks. Blender previews, source-art commands, licensing safeguards, rig motion and Play Mode evidence: **`docs/IRONPINE_CREATURE_ART.md`**.

This is playable rigged creature art, but does not certify final visual quality, professionally recorded howls or complete combat playtesting. The howl interruption still uses `CombatActor.CanAct`, and sound/animation polish remains relevant.

Important engineering follow-ups before calling wildlife shippable:
1. Playtest attack cadence and tactical telegraphs with actual Novice/Tier 1 characters and full party.
2. Verify Bleed tick math, interruption and target-loss edge cases across deaths, player reconnects and despawns.
3. Confirm XP/Codex group credit, especially multiplayer and per-player sight tests.
4. Polish the installed real creature rigs/animations and spatial sound, especially foot sliding, full 3D gameplay view and natural non-placeholder quillrat/howl recordings.
5. QA scene placement, road bypasses, fog, tree collisions, FPS, navmesh roaming and out-of-sight respawn across multiple local cameras.
6. Verify ordinary HP/damage/XP calibration against the intended level-12–20 difficulty; numerical multipliers are wired, absolute balance provisional.

**Safety:** This branch does not merge to `main`; preserve unrelated `ProjectSettings/PackageManagerSettings.asset` worktree modifications.

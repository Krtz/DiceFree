# October 12, 2026 — Ironpine realistic wolves and Cornberg playtest fixes

**Branch:** `feature/visual-overhaul-vis00` (not merged to `main`).
**Status:** Implemented and compiled; headless Unity/Blender validation passed. Full visual Game-view and hands-on mouse/keyboard QA still recommended.

## What changed

### Realistic Furry Wolf FREE

- Audited the user's downloaded **Realistic Furry Wolf FREE sample REALISTIC FOREST ANIMALS** from Rip Vertices Studio. This includes `Assets/Wolf/Models/Wolf.fbx`, `Assets/Wolf/URP/Wolf/Prefab/Wolf_URP.prefab`, textures, and a complete wolf animation set.
- **Installed the pack locally on the authorized Windows PC, not in the public repository.** Raw `Assets/Wolf/` is explicitly ignored in `.gitignore`, as is its generated integration folder `Assets/_DiceFree/Resources/IronpineLicensedWolf/`.
- Editor utility `IronpineLicensedWolfInstaller.Install` generates an independently controllable local Resources prefab with Standard-to-URP Lit compatible textured fur/coat/eye materials and Animator Controller mapped to **ten of the supplied FBX clips**:
  - Idle → Idle_New
  - Walk → Walking
  - Run → Running_New
  - Bite → AttackR
  - Howl → WolfHowl_New (the authentic howling animation)
  - Pounce → Jump_New
  - Sniff → sitingIdle._New
  - Scratch → sits down._New
  - Hurt → GetHit_2
  - Death → dead
- The original vendor locomotion clips are flagged looping; bite and howl clip speeds are adjusted to match DiceFree's short telegraphs/cooldowns.
- `IronpineRealisticWolfRuntime` substitutes the local licensed prefab at scene load **without altering any enemy stats, hitbox, AI, attack effects, NavMesh, XP, loot, or respawn**. Fallback when pack is absent remains the previously authored original Blender wolf (permitting GitHub clones/builds without third-party assets).
- Grey uses normal texture; Dark uses a darker fur/material tint; Alpha is substantially larger, darker, with subtle red emissive eye materials and two faint bone-mounted eye lights. The three quillrat designs/rigs remain untouched.
- Local materials generated in isolated Unity `-noUpm` use built-in Standard when URP Lit is unavailable. At runtime they upgrade to Universal Render Pipeline/Lit if available, preserving diffuse textures and alpha-cut fur; re-running the editor installer with URP available also upgrades local assets. Use Unity menu **DiceFree → Ironpine → Refresh local realistic wolves**.
- Never commit/export the vendor's raw FBX, textures, controller, Unity package, or locally generated licensed prefab/materials. They are user-owned local art dependencies.

### X / Shift attack-move

- `CombatInput.BeginAttackTargeting` no longer invokes `CancelAttackMove()`, `attack.Cancel()` or `Interactor.Cancel()` merely when pressing X. X only arms the cursor; movement/attack/queue remains untouched until the actual destination is confirmed.
- Existing `SkillTargetingController.QueueAttackMoveFromTargeting` and `PlayerCommandQueue` append each Shift-confirmed attack-move to the tail of the ordered queue.
- Extended `CommandQueueAudioValidation.AttackMoveTargetingRegression` to dispatch an actual walk order, assert the NavMesh movement continues after X, and append **two consecutive** X + Shift attack-move destinations after it. The local Unity Play Mode smoke log reports:
  `IRONPINE_CONSECUTIVE_ATTACK_MOVE_OK {success=True, movementPreserved=True, queuedAttackMove=True, consecutiveAttackMove=True, totalOrders=3}`.
- Unshifted confirmed commands retain standard replace-queue behavior, and Stop/Escape still cancel.

### Southern bridge and road

- Bridge geometry from the real Cornberg scene: center **(-19.10, 0.02, -9.00)**, 9m east-west deck (x -23.60 to -14.60) and 5m north-south breadth (z -11.50 to -6.50). Previously the winding diagonal sand road missed the west end and dipped across/under the planks.
- `CornbergSouthernBridgeAlignment.Install` generated a dedicated road mesh at `Assets/_DiceFree/Settings/Ironpine/Southern bridge aligned road.asset` and assigned it to `Mountain to east road`. Two sand approach segments finish precisely at the west/east deck edges at y=0.215; **the timber deck stays exposed and visible** between them. Ramps blend back to surrounding ground. Bridge/rail colliders are not altered.
- `CornbergLandscape.Road` control points and scene-rebuild hook were updated to preserve the corrected route on full rebuild. The baked navigation wasn't modified.
- Bridge-specific Unity editor validation passed with return code 0: both deck edges align and NavMesh.CalculatePath produces a complete crossing between the banks. No loss of the onward road eastwards.

### Willow canopy from screenshot

- The screenshot's tree corresponds to the generated `Tree_Willow` model in `SourceArt/build_cornberg_batch.py`. The original model had bare spokes high above separate green hanging capsules.
- Retargeted the bough endpoints into broad leafy branch sleeves, added a substantial connected central crown, oriented foliage hangers around radial boughs and covered exposed branches.
- Regenerated **only** `SourceArt/World/CornbergBatch/Tree_Willow.blend` and the matching tracked `Assets/_DiceFree/Art/World/CornbergBatch/Trees/Tree_Willow.fbx`; Unity can reuse existing prefab/mesh references because the original GUID stays unchanged. No other tree variants were regenerated.
- See original Blender render (not screenshot from game): `docs/screenshots/CornbergWillowCanopyFixed.png`.

## Actual verification and limitations

- Tooling: `T:\TEMP\DiceFree-IronpineValidation` copy of Unity 6000.6.3f1 using the existing locally cached package assemblies. User's current PC/Unity app was connected through Remote Desktop Commander; the main GUI Unity editor remained closed, not force-restarted.
- `WolfInstallOct12b.log`: original vendor wolf animations and compatible local material/prefab creation, **pass**.
- `WolfValidateOct12.log`: rigged wolf prefab, animation states, shaders, looping Idle/Walk/Run, **pass**.
- `BridgeInstallOct12b.log`, `BridgeValidateOct12b.log`, `FullBridgeNavOct12.log`, `FinalCompileNavOct12.log`: bridge authored, validated, and confirmed navigable, **pass**.
- `QueueSmokeOct12.log` and `FinalOct12Smoke.log`: 22 initialized living wildlife actors, licensed realistic wolf replacement and Alpha red-eye indicators, 3-command queue with active movement preserved, **pass**.
- `Tree_Willow.blend` preview rendered successfully in Blender 5.2.2.
- Unity batch Play Mode continues to log a known `UnityEditor.Search.SearchDatabase` `ArgumentOutOfRangeException` on startup, but the required wildlife and queue checks report success and are unaffected.
- Still requires **real graphical game-view inspection** on the user's Windows PC with actual URP packages loaded, especially alpha eye intensity, fur alpha cutouts/LOD, animation speed, canopy placement in the full forest, road edge shading, and hands-on keyboard clicking with multiple visible queue destinations. No end-to-end Windows player build was produced in this pass.

**Preserve** unrelated, unstaged `ProjectSettings/PackageManagerSettings.asset` changes. No `main` merge without explicit user approval.

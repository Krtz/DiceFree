# Options and Keybindings Validation

Issue #47 branch: `feature/47-options-keybinds`, based on prototype head `6fd6beaada0767d1e5c37a1a7c365c5e6ee84368`.
Validation ran against `T:\\TEMP\\DiceFree-Options47` with Unity 6000.6.3f1. No merge is authorized.

## Live evidence — 2026-10-05

| Check | Evidence |
| --- | --- |
| Unity compile | Connected Editor recompile completed with `compilationFailed=false` and zero compiler errors after final settings-store fixes |
| Defensive settings boundaries | `DICEFREE_CONTROL_SETTINGS_BOUNDARIES_OK` |
| Runtime Input System / rebinding | `DICEFREE_CONTROLS_OK` |
| Exact defaults | `DICEFREE_CONTROLS_DEFAULTS_OK`; catalog matches the pre-#47 gameplay defaults |
| Native simple rebind | Interact rebind to J reached an already-created owner `InputAction` through native binding overrides |
| WASD composite | Move Forward composite part rebound to T and produced `Vector2.up` through the native action |
| Conflicts / reserved controls | Duplicate active binding rejected with explicit feedback; Escape and pointer delta rejected for key capture |
| Reset / persistence | Single reset, reset-all, saved override reload, corrupt-primary backup recovery, full-corruption fallback, missing-file fallback all passed |
| Future settings version | Newer settings envelope is preserved read-only and defaults are used for the session |
| Escape routing | Capture > Options > Inventory > Interaction > Clear Target > Open Options passed as one deterministic route policy |
| Existing gameplay actions | F6 mode switch, Space stop, I interact, Tab cycle, X attack, R Return-to-Revive-Point, B inventory and pointer-delta default assertions passed |
| Architecture | `DICEFRE_ARCHITECTURE_OK` |
| Architecture policy | `DICEFRE_ARCH_POLICY_SELFTEST_OK` |
| Managed references | `DICEFREE_MANAGED_REFERENCE_OK` |
| Windows Development build | `DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK`; 188,442,716 bytes, 0 errors, 7 warnings |

The Windows build spent most of its time compiling shader variants; the structured build result succeeded. Build output remains local at `%TEMP%/DiceFree-Novice-Cornberg-Issue43/DiceFree.exe`.

## Commands

Edit Mode settings validation:

```powershell
unity command dicefree.controls.settings.validate --project-path T:\TEMP\DiceFree-Options47
```

Expected: `DICEFREE_CONTROL_SETTINGS_BOUNDARIES_OK`.

Native runtime input validation:

```powershell
unity command dicefree.controls.start --project-path T:\TEMP\DiceFree-Options47
unity command dicefree.controls.validate --project-path T:\TEMP\DiceFree-Options47
unity command editor_stop --project-path T:\TEMP\DiceFree-Options47
```

Expected: `DICEFREE_CONTROLS_OK`.

## Manual UX review still requested

The automated suite proves the settings model, native Input System rebinding, conflict/reset/persistence semantics and Escape policy. Human review should still check the prototype OnGUI presentation and feel:

1. Open Options via the visible top-right button or Escape.
2. Rebind Interact and one WASD direction, close Options, and verify gameplay feel.
3. Attempt a duplicate key and Escape during capture; verify readable feedback and no accidental gameplay command.
4. Reset one binding and reset all.
5. Verify the full-screen Options overlay blocks click-to-move/world HUD interaction while open.
6. Verify successive Escape contexts feel sensible: modal/Options, inventory, interaction, target, then Options.
7. Restart the player and confirm labels/use reflect persisted overrides.

The v1 deliberately does not implement final Audio/Graphics settings, controller rebinding, ability hotkeys, per-profile binding layouts, or final visual styling. Controls is the completed page; Audio/Graphics are future placeholders.

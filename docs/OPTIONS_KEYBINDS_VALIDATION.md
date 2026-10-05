# Options / Keybinding validation — issue #48

Branch: `feature/48-options-keybinds`
Base before implementation: `6fd6beaada0767d1e5c37a1a7c365c5e6ee84368`
Issue: #48
Merge state: **unmerged; human review required**

## Scope validated

The prototype establishes one shared Unity Input System action asset, runtime binding service, local user-settings persistence, and an in-game Options / Controls panel.

The test scope covers:

- canonical default bindings;
- individual WASD composite-part rebinding;
- mouse-button rebinding;
- immediate effective-path/control resolution;
- persisted binding-override JSON reload;
- corrupt-primary recovery from backup;
- reset-to-defaults persistence;
- duplicate/shared binding conflict feedback;
- Gameplay/Camera action-map suppression while Options is open;
- UI map remaining active while modal;
- F10 opening Options in Play Mode;
- interactive Interact -> J rebinding in Play Mode;
- rebound J actually firing the live Interact action in the normal player loop;
- gameplay/camera maps resuming after Options closes;
- closed-state Escape not opening Options;
- open-state Escape closing Options;
- existing Route 12 traversal regression;
- assembly dependency policy and managed-reference checks;
- Windows x64 Development build.

## Final validation markers

- `DICEFREE_CONTROLS_ASSET_OK`
- `DICEFREE_CONTROLS_OK`
- `DICEFREE_CONTROLS_PLAYMODE_OK`
- `ISSUE36_ROUTE12_OK`
- `DICEFRE_ARCHITECTURE_OK`
- `DICEFRE_ARCH_POLICY_SELFTEST_OK`
- `DICEFREE_MANAGED_REFERENCE_OK`
- `DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK`

## Windows build evidence

Final Windows x64 Development build on the cleaned implementation tree:

- output: `%TEMP%/DiceFree-Novice-Cornberg-Issue43/DiceFree.exe`;
- size: **187,323,162 bytes**;
- errors: **0**;
- warnings reported by the build command: **4**.

The build command reports the checked Git SHA as the branch base because validation occurred on an intentionally dirty, not-yet-committed implementation tree. The implementation itself is the working-tree content described in this document.

## Persistence boundary

Control settings are intentionally separate from Echo/manifestation saves:

`Application.persistentDataPath/Settings/bindings.json`

Writes use a lock + pending file + backup replacement. Binding settings do not change the existing gameplay save schema.

## Provisional / future

- The IMGUI Options panel is a functional prototype, not final UI art/layout.
- Audio/graphics settings are not implemented here.
- Controller/gamepad rebinding is not implemented here.
- Mouse-wheel zoom and raw pointer delta are not exposed to rebinding yet.
- Close Options remains fixed to Escape for this first pass.
- Human UX review of labels/layout/rebinding feel is still required.

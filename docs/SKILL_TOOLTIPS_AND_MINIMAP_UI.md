# DiceFree — Skill Tooltips, Unified Skills Layout and Minimap Walking

Branch: `feature/visual-overhaul-vis00`. Do not merge into `main` without review.

## Hover information

- Added shared `SkillTooltips.Describe(...)` logic for **all fifteen authored skill definitions**: Novice, Magically Touched Novice and Physically Blessed Novice.
- The active `GenericActionBar` shows a description when a learned or unlearned ability is hovered, including active rank, hotkey, cost, cooldown, range, relevant duration, target behavior and key scaling values. Skill names / stats are derived from the definition and its rank methods, rather than a separate manually written tooltip for each button.
- The three skill-point allocation views show the **same actual description** on skill rows and `+1` buttons so players can understand an ability *before spending points*.
- The disabled legacy class-specific skill bars also support hover descriptions for compatibility.
- Tooltip layout widened/tallied to accommodate detailed descriptions; the new active action bar also accepts Input System mouse coordinates, so disabled/cooldown buttons remain inspectable.
- Novice all-stat passive describes its actual +1 to each of the five attributes per invested rank. Magic Sand explicitly shows the +10 flat raw damage per rank and the real debuff scaling.

## HUD layout editing

- Novice / Magical / Physical panels remain independent class-specific skill progression UIs, but all expose the **one shared movable HUD layout group `skills`**, displayed as **Skills**.
- `HudLayoutManager` draws an edit-mode outline **only for the current class's active Skills panel**, instead of showing three overlapping versions. The selected position/size is shared across class changes.
- Saved legacy layout IDs can still be read as fallback if the unified `skills` position has never been customized; new edits persist only the canonical group.
- HUD widget instance IDs remain individually unique for legacy references.

## Click to move from the minimap

- Left/right clicking the minimap image (excluding the +/- zoom buttons) now converts north-up minimap pixel coordinates into a world X/Z destination.
- A terrain-raycast and NavMesh sample ensure valid ground; existing `TraversalMotor.MoveTo` requires a **complete reachable NavMesh route**. No teleporting, wall bypass, or movement through blocked terrain.
- New map commands cancel attack-move/interactions **before** path assignment so those cancellations do not accidentally wipe the route. The familiar destination marker is updated.
- Minimap movement is disabled while targeting an ability, when movement is blocked, while a modal UI is open, or in HUD Edit Mode. It works independently of Classic/Direct control-mode preference.
- Uses a UI-owned `IMinimapNavigator` interface so there is no circular assembly dependency between HUD and gameplay code.
- The minimap camera now excludes the ground-fog mesh, because the UI already overlays its own smoothed fog mask.

## Validation

- Unity C# compilation completed with **zero errors and warnings**.
- `dicefree.ui.skills.inspect`: **PASS**; descriptions for all 15 skill definitions; all three panels share `skills` layout group and a common display name.
- Play Mode `dicefree.ui.skills.hover-coords`: **PASS**, confirmed the active action-bar slot contains a detailed real Magic Sand rank-6 tooltip and there is exactly **one** active Skills widget in the editor.
- `dicefree.hud.validate`: **PASS** (customizable HUD, equipment slots, locomotion).
- `dicefree.controls.validate`: **PASS** (control contracts).
- `dicefree.ui.minimap.travel-test`: **PASS** (center, north-up, east-right, reachable NavMesh path).
- `dicefree.ui.minimap.travel-status`: **PASS**: **2.0 metres physically traveled to the requested coordinate**, zero remaining destination distance, complete route; edit mode refused additional movement.
- `dicefree.magical.validate`: **PASS**. An unrelated existing physical validation reports `Cornberg player is not wired to Physical Mana`; do not represent it as passing without its own fix.
- A virtual mouse screenshot could not definitively confirm an IMGUI tooltip popup; actual playtesting with the physical mouse remains worthwhile.

## Playtesting

1. Hover any of the four active skill bar buttons. Try one learned and one on cooldown.
2. Open Skills with `K`, hover each row and the `+1` button. Switch class to check the class-specific data.
3. Open HUD Edit Mode: only one resizable panel should be called **Skills**.
4. Click the minimap with left/right mouse buttons. Your character should walk there if navigable; try an unreachable spot and check no path is created. Zoom and resize the minimap to test coordinate mapping.

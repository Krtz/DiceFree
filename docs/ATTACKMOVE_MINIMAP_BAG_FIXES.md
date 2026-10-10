# DiceFree â€” Shift Attack-Move, Minimap Roads, and Bag Item Menu

Development branch: `feature/visual-overhaul-vis00`. Implemented October 10, 2026.

## Shift + attack-move fix

**Reproduction:** Shift-queue a movement waypoint, press X to begin attack-move targeting, then hold Shift while clicking the attack-move destination.

**Cause:** `CombatInput.BeginAttackTargeting()` cleared existing orders when X was pressed without Shift, even though the user would hold Shift later when confirming the target. The world-target controller also interpreted any right-click as cancel, even Shift-right-click in attack-move mode.

**Solution:** Opening the attack-move cursor no longer clears the queue; normal (unshifted) confirmation replaces commands and shifted confirmation appends. Shift-left-click and Shift-right-click are accepted for queued attack-move destinations while attack-move targeting is active. `QueueAttackMoveFromTargeting` centralizes confirmation.

The new `dicefree.commands.attack-move.targeting-regression` + `...targeting-status` Play Mode checks verified an existing queued walk survives pressing X, a queued attack-move is appended, and both orders complete in sequence.

## Minimap roads

The previous fog fix excluded Unity's entire `Ignore Raycast` layer 2 from the minimap. Decorative roads and other non-colliding art also use layer 2, so they disappeared.

The fog overlay now uses a **dedicated render-only layer 29** (no collider), and the minimap camera excludes only that layer and portrait layer 31. All existing layer-2 roads, bridges and vegetation are visible again without rendering fog twice. Existing fog runtime validator was updated. `dicefree.minimap.road-layer-check`, fog and LOS validators pass. A Unity Game-view screenshot confirmed tan roads appear on the minimap.

## Inventory right-click menu

Right-click a non-equipped item in the bag to open a context menu:
- **Equip** â€” ordinary equipment rules; left-click-to-equip remains available.
- **Drop** â€” creates a physical, interactable treasure chest near the character on walkable NavMesh. Protects equipped items, quest items, defeated state and locked dungeon loadouts.
- **Send to Bank** â€” uses the existing EchoSharedBank remote-deposit transaction, including its capacity checks. The bank still requires visiting a banker for withdrawals.

The drop is a **real ownership transfer**, retaining the `ItemInstance.instanceId`, definition ID and bound flag. The chest uses existing `WorldLootVisual.Create`, can be picked up by right-click/I, and rejects duplicate pickups.

### Persistence

`WorldDroppedItemLedger` is registered as a new Echo-wide durable save section, `echo.world-dropped-items`. Each record stores the exact item instance, world position and scene name. The ledger reconstructs matching dropped chests when the saved world is reloaded, and removes the record on recovery. All inventory/bank operations retain their existing save notifications and bank transfer rules.

The UI references a small `IInventoryItemActions` interface instead of importing Application assembly types into the UI assembly.

## Verification

- Unity compile: **0 errors, 0 warnings**.
- `dicefree.items.drop.playtest`: physical chest, exact-instance pickup, duplicate rejection, durable JSON save/restore and record cleared: PASS.
- `dicefree.items.bank-from-bag.playtest`: remote deposit and unchanged ID: PASS.
- `dicefree.minimap.road-layer-check`: road layer visible and fog layer hidden: PASS.
- `dicefree.commands.attack-move.targeting-regression` + `targeting-status`: X then Shift attack-move appends and walks correctly: PASS.
- `dicefree.hud.validate`, `dicefree.controls.validate`, `dicefree.fog.cornberg.validate`, `dicefree.bank-balancing.validate`, `dicefree.cornberg-perimeter.validate` and `dicefree.ui.skills.inspect`: PASS.

## Future refinement

Add item context menus for equipped-slot items (unequip/swap) and inspect actual right-click UI interaction on the player's monitor; consider persistence for shared world drops beyond the current local Echo, plus item-drop placement options and on-screen order waypoints.

The unrelated local `ProjectSettings/PackageManagerSettings.asset` modification must remain untouched.

## Verified Unity game-view screenshots

- docs/screenshots/MinimapRoadsRestored.png — beige roads rendered on the minimap.
- docs/screenshots/BagItemContextMenu.png — Equip, Drop, Send to Bank popup visible in inventory.

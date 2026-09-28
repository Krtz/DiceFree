# DiceFree

**DiceFree** is the planned standalone 3D sequel/spiritual successor to **DiceBound**.

DiceBound takes place **inside the Dice**. DiceFree begins after the player defeats **The Last Equation** and reaches the **outside**: a strange cube-world whose six faces form the game's six main regions.

The game is intended to combine:
- the persistent co-op ORPG progression fantasy of Warcraft III custom maps such as *Twilight's Eve ORPG*;
- DiceBound's love of hidden unlocks, strange classes, build-changing loot, account progression, secrets and escalating difficulty;
- a proper standalone 3D action-RPG structure with visible equipped gear and a world that can grow for years.

## Current status

**Cornberg blockout, combat, timed respawn, XP and the first crop quest.**

The first World 1 pocket is available in `Assets/_DiceFree/Scenes/Cornberg.unity`.
Open it in Unity **6000.6.3f1** and press Play. Right-click moves in Classic mode;
F6 switches to WASD/Direct, the mouse wheel zooms, and V toggles the lookout.
Arrow keys or middle-mouse drag pan, Q/E rotate, Home recenters, and F toggles follow.
See [Cornberg blockout notes](docs/CORNBERG_BLOCKOUT.md) for scope, validation and known limitations.

The eastern crop field now contains one neutral green Slime. Left-click/Tab selects;
right-click an enemy or X attacks. R returns to Cornberg after death; the well heals
nearby living actors. See [combat playtest notes](docs/CORNBERG_COMBAT.md) for controls,
provisional tuning, architecture and validation. Art remains placeholder;
Tier-1 classes and multiplayer are not implemented. Timed Slime respawn, XP and
the first 3+2 crop quest are now playable: right-click the farmer beside the
fields, or press I nearby. See [Q1 progression notes](docs/CORNBERG_QUESTS.md).

## Current high-level canon

- DiceBound happens **inside the Dice**.
- Defeating **The Last Equation** is the bridge into DiceFree.
- DiceFree takes place on the **outside surface of the Dice**.
- The world has **six major regions**, one per face, thematically related to DiceBound's six boards.
- Characters begin as a **Novice**.
- At level 10, the first blessing offers **Physically Blessed Novice** or **Magically Touched Novice**.
- Later permanent lineages follow the current [class progression design](docs/CLASS_PROGRESSION.md).
- Advancement should feel like becoming a new class, not merely spending another talent point.
- Gear equipped by the player should be **visibly represented on the character**.
- The game should support persistent progression and eventually co-op, while remaining playable solo.
- Hidden classes, secret bosses, achievements, rare items and account-wide discoveries are core DiceFree DNA.

## Design docs

See the [docs](docs/) directory for the current living design.

## Development philosophy

1. **Design before sprawl.** Build systems that can scale before adding hundreds of pieces of content.
2. **Data-driven wherever sensible.** Classes, items, enemies, loot tables and progression should not require giant hard-coded switch statements.
3. **Visible progression.** Characters should look, play and feel meaningfully different as they advance.
4. **Secrets matter.** Discoveries should make the world feel larger than the visible UI.
5. **Leave the place nicer than we found it.** Refactors and cleanup are part of feature work, not a someday task.
6. **Do not remake Twilight's Eve.** Take inspiration from the progression structure and feeling, while making DiceFree's world, content, names, systems and assets its own.

## Repo usage

- Permanent design/canon belongs in `docs/`.
- Significant design decisions go in `docs/DECISION_LOG.md`.
- Concrete future work belongs in GitHub Issues.
- Implementation branches/PRs begin when actual development starts.

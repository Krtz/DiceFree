# DiceFree

**DiceFree** is the planned standalone 3D sequel/spiritual successor to **DiceBound**.

DiceBound takes place **inside the Dice**. DiceFree begins after the player defeats **The Last Equation** and reaches the **outside**: a strange cube-world whose six faces form the game's six main regions.

The game is intended to combine:
- the persistent co-op ORPG progression fantasy of Warcraft III custom maps such as *Twilight's Eve ORPG*;
- DiceBound's love of hidden unlocks, strange classes, build-changing loot, account progression, secrets and escalating difficulty;
- a proper standalone 3D action-RPG structure with visible equipped gear and a world that can grow for years.

## Current status

**Design phase only.**

DiceBound is still the active development priority. DiceFree exists now so ideas, decisions, lore, systems and future work have a permanent home instead of being buried in chat.

No Unity project or engine version is locked yet. When implementation begins, we will choose the current appropriate LTS/toolchain rather than prematurely freezing a version today.

## Current high-level canon

- DiceBound happens **inside the Dice**.
- Defeating **The Last Equation** is the bridge into DiceFree.
- DiceFree takes place on the **outside surface of the Dice**.
- The world has **six major regions**, one per face, thematically related to DiceBound's six boards.
- Characters begin as a **Novice**.
- Around level 10, the Novice advances into a small set of base classes; the current working model is **4 base classes**.
- Later advancement tiers branch repeatedly: **4 → 8 → 16 → 32 → 64...**
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

# ADR 0019: Modular Warcraft-Like Player HUD from the Start

- Status: Accepted
- Date: 2026-10-08

## Decision

DiceFree uses a command-panel-style isometric HUD inspired by classic Warcraft III/Twilight's Eve: small full-body 3D portrait, equipment slots, combat resources, XP/stats, command bar, square minimap, quests and fading chat. HUD modules including class/skill selection are independently movable/resizable from the beginning; don't defer editability to later. Themes, density/padding and arbitrary tinting are independent from layout; player settings persist independently from Echo progress. Preserve legibility at isometric distance and avoid a monolithic HUD image. See docs/UI_HUD.md and the accepted #53 implementation.

## Consequences

Game architecture follows this policy. Details, numerical tuning and per-feature authoring remain in the cited design files; changing the policy requires a superseding ADR.

# ADR 0020: Reusable Quest and Crafting Contracts

- Status: Accepted
- Date: 2026-10-08

## Decision

Routine quests use stable IDs, semantic event credits and reusable Kill/Interact/ReachArea/Collect/TalkTo/UseItem/CompleteDungeon objectives, with composed AND/OR requirements where needed. Events occurring before accepting a quest do not retroactively satisfy its objectives unless that behavior is explicitly authored. Custom scripting is reserved for exceptional encounter/story mechanics. Progression equipment crafting uses NPCs, authored recipes and real materials/gear/currency, rather than a universal player-side crafting menu. See docs/QUESTS_AND_INTERACTIONS.md and docs/CRAFTING_AND_PROFESSIONS.md.

## Consequences

Game architecture follows this policy. Details, numerical tuning and per-feature authoring remain in the cited design files; changing the policy requires a superseding ADR.

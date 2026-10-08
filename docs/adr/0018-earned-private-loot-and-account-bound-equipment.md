# ADR 0018: Earned Loot and Private Dungeon Reward Choice

- Status: Accepted
- Date: 2026-10-08

## Decision

Major progression gear comes from handcrafted dungeon/raid rewards, rare overworld drops, quests and NPC crafting. Successful dungeon clears present each participant a private loot-room selection: inspect offered drops, then choose one or take designated fallback EXP/gold/material reward. Independent drop rolls should not be secretly smart-looted; overworld drops remain rare and free-for-all under shared-world rules. Keep class/equipment compatibility, source-authored intrinsic effects, account-bound ownership, shared Echo storage, and transmog discovery as separate data contracts. No raid lockouts, no pay-to-win or timed loot gates. See docs/LOOT_AND_EQUIPMENT.md, docs/DUNGEONS_AND_DEATH.md and docs/CRAFTING_AND_PROFESSIONS.md.

## Consequences

Game architecture follows this policy. Details, numerical tuning and per-feature authoring remain in the cited design files; changing the policy requires a superseding ADR.

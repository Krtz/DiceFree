# ADR 0010: NPC Visual Tiering and Reusable Role Kit

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: a small reusable library of generic human NPC meshes and clothing/prop variants is the default for the world. Prioritize farmer, merchant, banker, alchemist and blacksmith; extend with guard, villager, herbalist, etc. Uniqueness is a purposeful authored distinction, not automatic for every named or quest-important character.

The runner and ordinary farmers use generic farmer/worker presentations even if narratively important. The former swordswoman and former mage adventurer couple retain temporary visuals for now and later receive distinct Tier-2 class skins; they participate in later class-advancement quests. Do not replace them with new generic NPCs as part of this batch.

NPC visual identity should be separable from quest/merchant/bank components and from progression ownership.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

# ADR 0012: First Slime Dungeon Scope and Deferred Co-Design

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: the first Slime Dungeon is a separate scene with one occupancy per dungeon ID, as in ADR 0006. First standard Slime Boss gets exactly two attacks: Slime Slam and Divide. High-level conditions may unlock a route to the Slime Regent and different loot. Slime Queen/King remain later challenging content.

The detailed dungeon map, room layouts, creature packs, pacing and gameplay rewards will be designed *with the user tomorrow*. Do not independently design/build the dungeon interior in the current overworld-art batch. Avoid adding a third boss attack or hard-coding a variant in unrelated scene components.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

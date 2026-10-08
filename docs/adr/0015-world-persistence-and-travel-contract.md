# ADR 0015: Persistent Traversal and Session Map Boundaries

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: the six cube-world faces are independently authored scenes and transition across cube edges under a convincing rotation/loading illusion. Players can inhabit different faces simultaneously. Dungeons have exactly one live occupancy per stable dungeon ID within a hosted session, with other dungeon IDs concurrently active; stale leases must recover and a 60-second staging window precedes roster lock.

World-map geography is not a global Unity scene singleton in multiplayer. Current faces, participant IDs, dungeon occupancy, condition snapshots and map return coordinates are session state. Detailed definitions and tradeoffs remain in ADR 0006; this ADR merely indexes the related broader persistence/travel contracts in docs/MULTIPLAYER.md, docs/PERSISTENCE_AND_SAVES.md and docs/TRAVEL_MOUNTS_AND_WORLD_AGGRO.md.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

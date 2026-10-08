# ADR 0014: Echo-Wide Codex Discovery Contract

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: Codex knowledge belongs to the Echo, not an individual class/manifestation. A monster is discovered only after killing that exact type. A drop appears only after it is actually seen; track times seen, not spoiler placeholder slots. A boss mechanic appears after witnessing it; dungeon stats include clears/wipes/deaths/solo/no-death, fastest clears by party size, secret and loot discoveries.

The Codex system subscribes to gameplay semantic events and persists as a versioned Echo-wide save section. Rewards may be cosmetic/title or small tightly capped advantages against an exact mob, never broad family-wide bonuses. Detailed tables and event contracts remain in docs/CODEX.md.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

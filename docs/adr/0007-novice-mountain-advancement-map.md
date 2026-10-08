# ADR 0007: Novice Mountain Advancement Is a Separate Map

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: the level-10 Novice advancement trial is a separate Unity map, not a hidden chamber positioned under Cornberg. Its entrance is a visible mountain gate in Face 1; the gate only opens for eligible level-10 Novices and links into the separate advancement map using the world/dungeon transition architecture. The class fork between Physically Blessed and Magically Touched occurs in that dedicated trial space, not merely on the open-world skill screen.

Mountain geometry is a real authored Blender asset with distinct animatable gate leaves and a reusable level/progression condition hook. A reversible open/close state is required. Later advancement maps can reuse the same transition and eligibility contracts. Do not overwrite existing class-fork persistence or allow an unqualified level-1 character to advance.

Current milestone: model and gate, separate map shell, eligibility hook. Full trial gameplay is a separate later content task.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

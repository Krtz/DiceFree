# ADR 0011: Transparent DiceBound-Inspired Slime Family

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: all slime variants are real 3D gel creatures with partly transparent bodies. Visually take VERY heavy inspiration from DiceBound's existing slime artwork and original playful slime identity while adapting to isometric world space.

Scale and visual complexity increase with enemy level/tier: tiny round crop slimes; larger road/forest slimes; increasingly visible suspended particles, bubbles, leaves, pebbles, crystals and other inclusions in elite/high-tier slimes. Some have a small bobbing antenna inspired by classic bouncing-slime JRPGs. Bosses may have luminous eyes; regent/royal variants get stronger visual ornamentation and are distinct from the first basic boss.

Crop slimes are very small spherical slimes, with approximately one population source per crop field. Slime body materials must be transparent in Unity/URP, not merely colored opaque. Keep scale tied to authored encounter tier as well as nominal model size.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

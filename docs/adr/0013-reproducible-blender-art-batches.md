# ADR 0013: Reproducible Blender Source and Prefab Production

- Status: Accepted
- Date: 2026-10-08
- Scope: DiceFree

## Decision

Decision: production of new 3D assets includes editable native Blender .blend source, repeatable modeling/export scripts when procedural, Unity-ready FBX exports, explicit runtime materials and applied scene/prefab integration. The art batch must be directly playable/inspectable in Unity, not merely a concept/reference image.

Environment trees should use varied meshes with independently recolorable trunk/foliage materials; material palettes allow multiple color variants from one base mesh. NPCs use role costumes and props. Translucent slimes need inner contents and a compatible Unity transparent material. Existing artist-created files and unique character assets must be preserved.

Prototype meshes are allowed while refining the visual style, but they must still be actual 3D content with clean source/export correspondence and documented limitations.

## Consequences

This is a stable design decision. Implementation can evolve, but changes to this policy require a superseding ADR; granular art tuning, individual room details and ordinary balance knobs remain in the design documents.

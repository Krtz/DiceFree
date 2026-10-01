# ADR-0001: Repository layout and playable build distribution

Status: Accepted
Date: 2026-10-01

## Context

DiceFree will contain both editable source art (for example Blender files) and Unity-ready runtime exports. We also need a clean way for testers/players to download only the playable game without cloning development assets.

Options considered included:
- a separate "runtime-only" Git branch;
- a separate art repository immediately;
- one source repository plus separate build artifacts/releases.

## Decision

Use one Git repository for code, Unity project content, documentation and editable source art for now.

Store native DCC source files outside Unity's `Assets/` tree and track large binary files with Git LFS.

Store exported Unity-ready runtime assets under `Assets/_DiceFree/Art/`.

Do not create a runtime-only branch.

Do not commit player builds.

Distribute playable builds as versioned build artifacts/releases. Initially GitHub pre-releases/releases are sufficient; later Steam, itch.io or another distribution channel can take over.

## Consequences

Positive:
- one source of truth;
- source/export versions live in the same history;
- no branch synchronization tax;
- players download only build output;
- Unity build machines do not need native DCC source files inside Assets.

Tradeoffs:
- developer clones still include LFS pointers/source-art history;
- LFS storage/bandwidth must be monitored;
- source and exported files both consume repository storage.

Revisit a separate art-source repository only if measured scale, permissions or CI/LFS cost justify the additional coordination complexity.

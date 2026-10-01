# ADR-0002: Runtime modules and Assembly Definitions

Status: Accepted as target architecture; implementation pending
Date: 2026-10-01

## Context

The Cornberg prototype currently has no first-party `.asmdef` files and therefore compiles primarily through Unity's predefined assembly.

That is convenient early, but a large RPG with many classes, tools, tests and systems needs explicit dependency boundaries to reduce coupling, compilation scope and accidental cross-domain references.

## Decision

Move DiceFree toward explicit first-party Assembly Definition ownership before substantially expanding beyond the Cornberg vertical slice.

Target logical assemblies:

- DiceFree.Foundation.Runtime
- DiceFree.Gameplay.Runtime
- DiceFree.Items.Runtime
- DiceFree.World.Runtime
- DiceFree.Quests.Runtime
- DiceFree.AI.Runtime
- DiceFree.Persistence.Runtime
- DiceFree.UI.Runtime
- DiceFree.Application.Runtime
- DiceFree.Editor
- separate EditMode and PlayMode test assemblies

Dependency rules are defined in `docs/ARCHITECTURE.md`.

The migration must be incremental and green after each boundary.

Domain modules must not solve dependency cycles by directly depending on UI, Editor or concrete Persistence.

## Consequences

Positive:
- dependency direction becomes mechanically enforceable;
- reduced accidental coupling;
- better compile iteration as the project grows;
- cleaner test/editor separation;
- easier future package/module extraction.

Tradeoffs:
- current prototype cross-dependencies will need small refactors;
- assembly boundaries can become bureaucracy if made too granular.

We intentionally use a modest number of domain assemblies, not one assembly per folder or type.

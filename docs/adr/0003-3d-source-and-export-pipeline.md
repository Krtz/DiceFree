# ADR-0003: Native 3D source and Unity runtime export pipeline

Status: Accepted
Date: 2026-10-01

## Context

DiceFree will begin custom 3D asset production with a single stylized one-handed sword, followed later by a shield, shoes and a hat.

Unity can import Blender files, but production use of native DCC files under Assets couples import to the DCC installation/version and causes Unity to import more source data than the game needs.

## Decision

Use Blender/native DCC files as editable source, stored under repository-root `SourceArt/` outside Unity `Assets/`.

Export Unity-ready models, initially FBX, under `Assets/_DiceFree/Art/`.

Track both native and exported binary assets with Git LFS.

Use metric scale:
- 1 Unity unit = 1 meter.

Visual direction:
- stylized fantasy;
- strong Warcraft III / Magicka-like readability;
- moderately exaggerated;
- slightly oversized equipment for isometric readability;
- original designs, not replicas of reference IP.

Prove the pipeline with exactly one sword before producing the shield/shoes/hat sequence.

## Consequences

Positive:
- builds do not depend on Blender;
- native source remains editable;
- runtime assets remain small and intentional;
- import/export can later be scripted and validated;
- art scale and attachment conventions are established early.

Tradeoffs:
- source/export synchronization must be disciplined;
- binary storage uses Git LFS;
- a model change requires an explicit export step.

The first sword task should create or freeze an export preset/script so this does not remain a manual convention.

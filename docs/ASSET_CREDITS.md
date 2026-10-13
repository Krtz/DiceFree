# DiceFree Asset Credits & License Registry

This is the source of truth for **third-party art, models, textures, animations, audio, VFX, fonts, and other distributable assets** considered for or used in DiceFree. Record provenance before committing an external asset to the Unity project. This document is a tracking aid, not proof of ownership or legal clearance.

## Workflow

1. **Candidate:** Record the original creator, exact source URL, claimed license (including version when known), intended use, and status. Do not treat candidates as shipped assets.
2. **Before import:** Verify the license at its source, preserve any original license/credit files and copyright notices, review other underlying rights (including source-image rights for reconstructed/AI-assisted meshes), and record what is actually imported.
3. **When used:** Set status to `In use`, link to the actual `SourceArt/` and/or `Assets/` paths, describe modifications, and preserve an appropriate credit line for release packaging.
4. **Before release:** Review all `In use` entries for required attribution/license notices in the game's credits, about screen, or accompanying notices. Recheck unresolved entries; never ship `Hold` or unverified `Candidate` assets.
5. **Removal:** Mark an asset `Removed` when its files are no longer distributed; keep the provenance record.

### Registry

| Asset / status | Creator | Source | License / verification | DiceFree location / changes | Release credit |
| --- | --- | --- | --- | --- | --- |
| **Catrina Skeleton Bride Figure Meshi vs SupaVoxel** — **Candidate, not imported** | Pia Fernandez (as shown by Sketchfab) | [Sketchfab model](https://sketchfab.com/3d-models/catrina-skeleton-bride-figure-meshi-vs-supavoxel-7868738c801b4edfdf9487984b0a27ea) | **CC Attribution** as shown in the download dialog on 2026-10-14; version and provenance **not independently verified**. Check original model listing and any included license/readme before use. | None. Model not downloaded or incorporated through this task. No modifications. | **Proposed if imported:** “Catrina Skeleton Bride Figure Meshi vs SupaVoxel” by Pia Fernandez — [source](https://sketchfab.com/3d-models/catrina-skeleton-bride-figure-meshi-vs-supavoxel-7868738c801b4edfdf9487984b0a27ea) — CC BY (link to **verified exact license/version**) — adaptations described when made. |

### Add a new entry

Copy this row into the registry and replace the placeholders:

`| **Asset title** — Candidate / In use / Hold / Removed | Creator / copyright holder | [Original source](https://example.com) | Exact license + version + link; verified date; restrictions | SourceArt/... and/or Assets/...; changes | Credit text + source and license URLs |`

## Practical license checks

- **CC0:** Attribution generally not required by the CC0 dedication, but keep creator/source provenance here.
- **CC BY:** Generally permits commercial use and adaptations **with attribution**. Preserve creator/title as supplied, source and exact license links, any required copyright notices, and an indication of changes; avoid implying endorsement.
- **CC BY-SA:** Requires an explicit review of share-alike obligations for adaptations before import/distribution.
- **CC BY-NC / CC BY-ND, unclear license, or unknown authorship:** Place on **Hold** pending review. Do not assume commercial-use or modification permission.
- **Marketplace, paid, or custom licenses:** Record the exact license/EULA and its distribution/seat/attribution restrictions; do not treat a receipt as an unlimited license.
- **Public domain / AI-generated:** Still check provenance, underlying source images, third-party trademarks, recognizable copyrighted designs, and any service-specific terms.

## Internal work

DiceFree's original project-authored assets do not need a third-party attribution row unless their materials incorporate third-party content. A source file's presence in Git does **not** automatically establish rights to distribute it.

Performance decisions (geometry, texture size, LODs, rigs, materials) belong in the [art pipeline](ART_PIPELINE.md), not in this license ledger.

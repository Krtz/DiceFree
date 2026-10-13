# DiceFree Asset Credits & License Registry

This is the source of truth for **third-party art, models, textures, animations, audio, VFX, fonts, and other distributable assets** considered for or used in DiceFree. Record provenance before committing an external asset to the Unity project. This document is a tracking aid, not proof of ownership or legal clearance.

## Workflow

1. **Candidate:** Record the original creator, exact source URL, claimed license (including version when known), intended use, and status. Do not treat candidates as shipped assets.
2. **Before import:** Verify the license at its source, preserve any original license/credit files and copyright notices, review other underlying rights (including source-image rights for reconstructed/AI-assisted meshes), and record what is actually imported.
3. **When used:** Set status to `In use`, link to the actual `SourceArt/` and/or `Assets/` paths, describe modifications, and preserve an appropriate credit line for release packaging.
4. **Before release:** Review all `In use` entries for required attribution/license notices in the game's credits, about screen, or accompanying notices. Recheck unresolved entries; never ship `Hold` or unverified `Candidate` assets.
5. **Removal:** Mark an asset `Removed` when its files are no longer distributed; keep the provenance record.

### Registry

Full technical inventory: [ASSET_CATALOG.md](ASSET_CATALOG.md). Staging is on the user PC at `T:\\TEMP\\ART_ASSETS`; **none of these downloaded assets has been imported into Unity**. Source-page licensing was checked for a subset only; absent files do not indicate permissive licensing.

| Asset / status | Creator | Source | License / verification | DiceFree location / changes | Release credit |
| --- | --- | --- | --- | --- | --- |
| **Catrina Skeleton Bride Figure Meshi vs SupaVoxel** — **Candidate, downloaded to staging only** | Pia Fernandez (as shown by Sketchfab) | [Sketchfab model](https://sketchfab.com/3d-models/catrina-skeleton-bride-figure-meshi-vs-supavoxel-7868738c801b4edfdf9487984b0a27ea) | **CC Attribution** as shown in the download dialog on 2026-10-14; version and provenance **not independently verified**. Check original model listing and any included license/readme before use. | Staged archive: `catrina-skeleton-bride-figure-meshi-vs-supavoxel.zip`; not imported into Unity. No modifications. | **Proposed if imported:** “Catrina Skeleton Bride Figure Meshi vs SupaVoxel” by Pia Fernandez — [source](https://sketchfab.com/3d-models/catrina-skeleton-bride-figure-meshi-vs-supavoxel-7868738c801b4edfdf9487984b0a27ea) — CC BY (link to **verified exact license/version**) — adaptations described when made. |

| **Anime Swordswoman (SupaVoxel)** — **Hold**, not imported | Unverified | Original source URL unverified | License and source-image rights unverified | Local staging only: `anime-swordswoman-image-to-3d-supavoxel.zip`. No Unity changes. | To be prepared after clearance |
| **Black Dragon with Idle Animation** — **Hold — NC**, not imported | 3DHaupt | [Sketchfab original](https://sketchfab.com/3d-models/black-dragon-with-idle-animation-fb0053a2e59b43868e934c239bf4eb36) | **CC Attribution–NonCommercial (CC BY-NC)** listed on source; no commercial DiceFree use without separate permission. NoAI listing note. | Local staging only: `black-dragon-with-idle-animation.zip`. No Unity changes. | To be prepared after clearance |
| **Blood Priest in Crimson Ritual Robes** — **Hold**, not imported | Unverified | Original source URL unverified | License and model provenance unverified; nested FBX ZIP | Local staging only: `blood-priest-in-crimson-ritual-robes.zip`. No Unity changes. | To be prepared after clearance |
| **Coffee Cup Samurai (Sloyd vs SupaVoxel)** — **Hold**, not imported | Unverified | Original source URL unverified | License and generated/source-image provenance unverified | Local staging only: `coffee-cup-samurai-sloyd-vs-supavoxel.zip`. No Unity changes. | To be prepared after clearance |
| **Corrupted Archangel Six Wings Boss** — **Hold**, not imported | Unverified | Original source URL unverified | License not independently verified; nested FBX ZIP | Local staging only: `corrupted-archangel-six-wings-boss.zip`. No Unity changes. | To be prepared after clearance |
| **Corrupted Fallen Priest Dark Fantasy** — **Hold**, not imported | Unverified | Original source URL unverified | License not independently verified; nested FBX ZIP | Local staging only: `corrupted-fallen-priest-dark-fantasy.zip`. No Unity changes. | To be prepared after clearance |
| **Cyberpunk Swordswoman (3D AI Studio vs SupaVoxel)** — **Hold**, not imported | Unverified | Original source URL unverified | License/source-image provenance unverified | Local staging only: `cyberpunk-swordswoman-3d-ai-studio-vs-supavoxel.zip`. No Unity changes. | To be prepared after clearance |
| **Dark Necromancer - Corrupted Staff & Skulls** — **Candidate**, not imported | Pigcraft | [Sketchfab original](https://sketchfab.com/3d-models/dark-necromancer-corrupted-staff-skulls-10c3bef4113b4e128ad41c9fdcab74e3) | **CC Attribution** listed on creator page; exact version/underlying rights to verify before use | Local staging only: `dark-necromancer-corrupted-staff-skulls.zip`. No Unity changes. | To be prepared after clearance |
| **Fallen Paladin in Corrupted Black Plate Armor** — **Hold**, not imported | Unverified | Original source URL unverified | License not independently verified; nested FBX ZIP | Local staging only: `fallen-paladin-in-corrupted-black-plate-armor.zip`. No Unity changes. | To be prepared after clearance |
| **Holy Knight Lady (Hyper3D vs SupaVoxel)** — **Hold**, not imported | Unverified | Original source URL unverified | License/source-image provenance unverified | Local staging only: `holy-knight-lady-character-hyper3d-vs-supavoxel.zip`. No Unity changes. | To be prepared after clearance |
| **Kindred (League of Legends) - Rigged** — **Hold — IP**, not imported | maisth (fan model); underlying character owned by third party | [Sketchfab original](https://sketchfab.com/3d-models/kindred-league-of-legends-rigged-06a32da82b0441c39950296315307400) | Creator lists **CC Attribution** for the fan model, but **does not clear the underlying League of Legends IP**. Do not ship; NoAI listing note. | Local staging only: `kindred-league-of-legends-rigged.zip`. No Unity changes. | To be prepared after clearance |
| **Redhead Anime Swordswoman (SupaVoxel)** — **Hold**, not imported | Unverified | Original source URL unverified | License and source-image rights unverified | Local staging only: `redhead-anime-swordswoman-png-to-stl-supavoxel.zip`. No Unity changes. | To be prepared after clearance |
| **Samurai Cat (Tripo vs Meshy vs SupaVoxel)** — **Hold**, not imported | Unverified | Original source URL unverified | License/source-image provenance unverified | Local staging only: `samurai-cat-tripo-3d-vs-meshy-supavoxel.zip`. No Unity changes. | To be prepared after clearance |
| **Truffle Man** — **Candidate**, not imported | dotflare (concept credited to BABAGANOOSH99) | [Sketchfab original](https://sketchfab.com/3d-models/truffle-man-e41c6781e4854a1fbe5cdf24a0029aab) | **CC Attribution** listed on creator page; confirm exact version and underlying concept rights before release | Local staging only: `truffle-man.zip`. No Unity changes. | To be prepared after clearance |
| **Viking House** — **Hold**, not imported | Unverified | Original source URL unverified | Asset and third-party photographic texture provenance unverified | Local staging only: `viking-house.zip`. No Unity changes. | To be prepared after clearance |
| **Viking Shieldmaiden 3D Model** — **Hold**, not imported | Unverified | Original source URL unverified | License/source-image provenance unverified | Local staging only: `viking-shieldmaiden-3d-model.zip`. No Unity changes. | To be prepared after clearance |
| **Warrior Girl** — **Hold**, not imported | Unverified | Original source URL unverified | License unverified; included Alembic .abc instead of GLB/FBX | Local staging only: `warrior-girl.zip`. No Unity changes. | To be prepared after clearance |
| **Wolf Guy** — **Hold**, not imported | Unverified | Original source URL unverified | License/model provenance unverified | Local staging only: `wolf-guy.zip`. No Unity changes. | To be prepared after clearance |

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

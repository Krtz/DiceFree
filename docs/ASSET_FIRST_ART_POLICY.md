# DiceFree art production policy — asset packs first

**Decision: 2026-10-12. Status: active default for new DiceFree art.**

## Source selection in order

1. **Search the locally downloaded Unity Asset Store catalog first.** Catalog: `T:\TEMP\DiceFree-AssetCatalog\packages.json` (240 packs as of last full scan); new downloads must be indexed before artwork selection. Search by function, shape, rig, texture and genre, not only pack title.
2. **Reuse a suitable existing licensed source asset**, preferring packs with a clear redistribution/build license and technical compatibility. The Diceworld fiction explicitly supports stylistic variety: a realistically furry wolf can stand next to exaggerated low-poly plants or hand-painted characters.
3. **Adapt the asset to the game**, rather than discarding a good candidate merely because it is too realistic, low poly, poorly scaled, visually mismatched or incomplete. Allowed transformations include UV/material/URP conversion, retopology, proportions, recoloring, LODs, animation retargeting, variants, mesh editing, shader/VFX overlays and Blender-derived meshes; test actual gameplay silhouette/animation.
4. **Make original art** when available packs have no suitable candidate, cannot legally be used, or would compromise game readability; keep original creation reproducible.
5. **Validate actual game integration**, not only a model or texture preview: expected scale and facing, attack telegraphs, collider/NavMesh, performance, UI visibility, URP shaders and gameplay test at intended camera zoom.

## Licensing and GitHub requirements

- Keep downloaded original Asset Store packages and any source files without confirmed redistribution rights **locally installed and gitignored**. Do not check original pack FBX, PNG, textures, audio, controllers, or copied local resource prefabs into a public source repo simply because they are free.
- Commit adapters, authoring scripts, source references/mapping metadata, gameplay integration, and an **original** fallback where appropriate. A clone missing local source packs must build/execute without missing vital models or UI.
- Confirm per-pack licenses before embedding any third-party source art in public/distributable builds. Free does not imply public-domain.
- Catalog provenance: original pack name, specific asset path, adaptation pipeline, and where it is installed locally.

## Current examples

- Ironpine realistic wolves: the existing **Realistic Furry Wolf FREE** pack, adapted scale/fur tint/eye effects/animations, with our own fallback wolves.
- UI themes: various previously downloaded GUI packs used locally, with portable code-defined palettes.
- Ability bar: 17 distinct icon pictures from **Battle Skill Icon**, with original DiceFree code-drawn fallback sigils.
- Next: check downloaded village architecture, wolf caves, NPC and advanced-class gear/rigs before building Village 2, Wolf Dungeon or Tier II manifestations.

This policy prioritizes useful assets and working in-engine integration over visual homogeneity. Keep the gameplay readable at the Warcraft III-inspired camera distance.

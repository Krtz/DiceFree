# DiceFree asset library and replacement review — 27 November 2026

Current offline, searchable catalog: **T:\TEMP\DiceFree-AssetCatalog\2026-11-27\catalog.html**. It embeds the metadata and works without a server. Search pack names or exact internal filenames; tabs cover Unity downloads, imported files, ART_ASSETS, imported animation clips, VFX prefabs, custom-art sources, replacement recommendations and Korean aura thumbnails.

The full scan covers **481 downloaded Unity packages**, including 241 absent from the old 240-pack catalog, and **291 ART_ASSETS files**, including 275 archives. All 481 package directories were read, including 60,271 file entries plus folders. SHA256 records identify the original downloads. The imported-model/clip/effect metadata comes from the open Unity editor: **239 model imports, 271 clips and 578 effect prefabs**. The RPG animation packs below are downloaded but have not yet been imported; their future clips are not included in the 271 count.

The machine-readable snapshot and compact CSVs are alongside this README. The larger internal-path inventories, raw-model details, archive members and native thumbnails remain in the local catalog folder. Nineteen raw-library records contain bounded model-inspection or nested-archive limitations; those files are listed, but are not claimed as technically validated rigs. No pack license is invented from a filename. This is an inventory and suitability review, not a new source-license audit.

## Asset-first direction and replacements

Korean Traditional Pattern Effects is the primary ground-aura source. Existing supplied models, rigs, animation clips, textures, icons and VFX take precedence over making new artwork. Small adapters, retargeting, shader/material compatibility work and functional geometry remain appropriate. Targeting boundaries, collision, navigation, stairs, fog masks, selection outlines and UI layout are not replaced by decorative prefabs.

`replacement-shortlist.csv` provides **47 areas** with specific downloaded-pack/internal-path candidates. `custom-art-source-review.csv` indexes **202 source files** containing visual construction operations, separating likely artwork from test/runtime infrastructure. These are source-based suggestions and technical triage, not claims that every candidate has received artistic approval or a deformation test. Existing approved class appearances are preserved.

The highest-value next replacements are:

- **Animation:** use `FREE - 32 RPG Animations` and `RPGAnimationsPackFREE` before making more gestures. Exact candidates include `Assets/Blink/Art/Animations/Animations_Starter_Pack/Combat/SpellCast.fbx`, `CastingLoop.fbx`, `BowShot.fbx`, and DoubleL's Unity FBXs such as `Assets/DoubleL/FBX Unity/Bow/Attack A/Bow_Attack_A_1_All.fbx`. Retarget selected clips after verifying each avatar, weapon grip, feet and gameplay timing; importing a whole demo project is unnecessary.
- **Characters:** evaluate supplied rigged bodies first, including Elven Assassin Low poly, the All Star/Polytope character libraries and the rigged female/knight packs listed in the catalog. Keep the approved Magician, Berserker and Psion-family looks; improve their rigs/clip compatibility before replacing their identity. The ART_ASSETS priest/necro/witch models are alternatives, with their recorded rig status rather than an assumption they are ready.
- **Buildings and world props:** use Medieval Village Building Pack, Low-Poly Medieval Market and matching downloaded settlement/forge packs for the visible city/outpost/house skins. Preserve existing walkable interiors, gates, collision and NPC positions. Keep the already-admitted CosmicWonder dice mesh instead of replacing it with a random portal or primitive.
- **Nature and creatures:** reuse supplied tree/rock/vegetation and creature assets. Preserve foliage transparency and gameplay colliders. The existing native wolf rig is already a source asset and does not need another handmade replacement.
- **Equipment and icons:** use the actual weapon/armor/icon libraries; retain wolf themes in wolf loot and preserve main/offhand handling and item identity.

## VFX implemented in this pass

`AssetPackVfxLibrary` and its authoring recipe bind **nine Korean ground auras and twenty native cast/impact templates**. All sixteen playable class cast paths use the shared asset router, and all **91 named skill definitions** resolve to a template family. Passive definitions are included in the coverage index; they do not gain artificial cast events. Existing aura eligibility still decides when passive auras appear.

The source recipe is `docs/assets/asset-vfx-bindings-2026-11-27.json`. Local derivatives live under `Assets/_DiceFree/Resources/AssetLibraryLicensed` and retain original particles, meshes, textures and shaders. They strip demo behaviours, gameplay colliders, rigidbodies, audio and lights, normalize visual size using the main Pattern plane instead of outlying sparks, and put aura materials above transparent road overlays. Original pack files are preserved. One-shot pattern layers use continuous loop periods for passive auras. Startup timing is measured from the source particle systems so cast effects do not disappear before emitting. Native effects also replace common starter/Priest-family impacts and Fire Mage burning fields. Functional targeting/area boundaries and unavailable-pack fallbacks remain.

This gives each class an asset-backed casting layer and replaces the central common effects. Remaining bespoke projectile trails, creature-specific telegraphs, persistent marks and unusual skill choreography are called out for subsequent per-skill visual refinement; a cast-theme mapping is not a claim that every skill has a unique finished VFX composition.

## Imported-content cleanup

`import-cleanup-review.csv` separates folders referenced by enabled scenes/Resources from unreferenced imports. An unreferenced folder is a review candidate, not proof it was accidental or safe to delete. **No downloaded pack was deleted.** Two exact raw-archive duplicate groups are recorded: Nyra and Potions.

The imports initially prevented compilation through missing uGUI/TMP demo dependencies, duplicate global example types and removed Unity APIs. `PrepareLocalVfxImports.ps1` isolates seven optional demo/legacy-script folders with `DICEFREE_ENABLE_VENDOR_DEMOS` as an opt-in flag. It preserves existing assembly metadata and source files; backups and repair receipts are local. Original effect scripts needed only for legacy self-destruction are stripped from curated game prefabs, which use the shared game lifetime instead. Re-enabling those demos requires their dependencies and Unity-version/API repairs; setting the flag alone does not port them.

The review also found **65 imported Cartoon FX prefabs with unsupported shader materials** under the current URP setup. They are cataloged and retained, but were not selected for the new library. Shader support checks are distinct from actual rendering; the Korean family thumbnails were rendered in Unity URP and the selected library is tested separately in the Windows player.

## Rigging tools

**AccuRIG 2 is the recommended first addition.** Its documented workflow covers body and hand rigging and FBX export for Unity. It offers a better starting point than improvised automatic weights. Auto-Rig Pro is the Blender-oriented alternative when investing in a more scriptable Blender rig/export workflow. Existing rigged models remain the first choice; animation packs do not repair incorrect skin weights, and auto-riggers still need checks around hands, shoulders, coat tails and props.

Official references checked for this review:

- https://manual.reallusion.com/AccuRig-2/2.0/01-welcome/welcome.htm
- https://manual.reallusion.com/AccuRig-2/2.0/09-add-motions/export.htm
- https://www.lucky3d.fr/auto-rig-pro/doc/ge_export_doc.html
- https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html (free humanoid-only alternative; large clothing/props and nonstandard proportions are documented limitations).

## Reproducing the catalog

Run `tools/assets/catalog_all_assets.py` with the project, ART_ASSETS root and output directory. The parser uses gzip's complete header handling before streaming the tar directory, since Unity package headers include extra metadata. It never extracts or executes downloaded code. `--project-only` refreshes just the imported filesystem index; `--packages-only` refreshes package contents using already-observed unchanged-file hashes. Then run `finalize_asset_catalog.py`, the read-only editor exporter and `build_asset_review.py`. The editor exporter does not open or save gameplay scenes. Catalog filtering was checked with a local JavaScript/DOM harness; this is separate from Unity visual validation.
## Delivery and validation

The final Windows build is `T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-182821\DiceFree.exe`. It has zero build errors and two warnings. [Validation evidence](../../validation/asset-first-vfx-2026-11-27/delivery.json) records **106 standalone checks**, all 29 native bindings and 179 matching main/QA source/dependency files. The checks cover the nine Korean aura styles, visibility/restart/removal, all ten cast themes, particle emission, absence of imported demo behaviours/colliders, real Fireball costs/damage and empowered direct damage plus all six field ticks. The final-size aura capture verifies the pattern remains visible beyond a one-shot lifetime.

The open main editor remains on Cornberg, outside Play Mode, with no dirty scene and no compilation failure. Unrelated package/project setting changes and recovery files remain outside this change. Original downloaded packs and non-game documents remain local; the Git export contains asset metadata and recipes, with non-game documents omitted from the raw-file CSV.

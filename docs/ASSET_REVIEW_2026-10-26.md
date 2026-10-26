# DiceFree asset review — 2026-10-26

This is a project-level review of assets currently in the `integration/world1-foundation` checkout, the locally installed Unity packs visible in that checkout, and the staged model catalog. The initial catalog pass was static; the follow-up below records one local house-art prototype made through the connected Unity Editor. It does not move or commit licensed source files.

## Inventory and boundaries

- The existing model catalog covers 112 staged files: the first 60 have a source-page/license audit; the later 52 passed archive CRC checks but still lack verified provenance and license terms. The 52 remain excluded from runtime and redistribution.
- The separate local Unity Asset Store catalog contains 240 packages. Only assets already imported and connected through project wrappers were considered usable for this review. Raw local downloads and imported vendor folders are ignored by Git; the project must keep its authored fallback visuals usable without them.
- The tracked project has authored character, weapon, creature, environment, UI and VFX sources under `Assets/_DiceFree/Art` and `SourceArt`. Most visuals are in the intended stylized low-poly direction; several are deliberately procedural fallbacks, not missing runtime assets.
- The worktree already contains unrelated WIP in gameplay, skill definitions, UI, docs, `PackageManagerSettings.asset`, and untracked `Assets/Settings`. Preserve those changes.

## Existing runtime asset coverage

| Area | Current asset path | Review |
| --- | --- | --- |
| World trees | Four optional local Oak prefabs through `DetailedTreeVisual`; more than 800 trees are covered by the existing validator | Already replaced on machines with the local pack; authored tree geometry remains the portable fallback. No new replacement needed in this pass. |
| Wolf family | Optional `IronpineLicensedWolf/RealisticWolf`; authored Blender wolf remains the fallback | Strong example of the intended license-safe pattern. Existing runtime adapter keeps AI, collision and drops separate from the visual. |
| World loot | Optional `WorldLoot/TreasureChestModel`; original DiceFree chest is the fallback | Runtime replacement exists and keeps collider/interaction ownership on the game object. |
| Trimery shops and lumber props | `TrimeryArtSlot` loads local Crate, MarketStand, MarketStand2 and TimberSupport resources | Targeted props have asset-backed art and portable fallbacks. The slot disables imported colliders and scripts. |
| Player classes and equipment | Novice source rig, Mage/Physically Blessed adapters, tier-one authored equipment, plus optional licensed models | Existing class/presentation architecture supports source models without making gameplay depend on them. Visual checks should be done per class in the existing preview harnesses. |
| HUD and skill icons | Existing licensed icon/UI atlases with original UI fallbacks | Current UI uses coherent framed panels; replace only after checking the full HUD at actual play resolution, not from atlas thumbnails alone. |
| Spell and combat effects | Authored effect prefabs and optional VFX packs | Keep effects tied to their skill-specific preview harnesses; no evidence from static inventory justifies replacing them wholesale. |
| Trimery houses and palisade | Six shells remain blockout; Timber merchant hall now has an optional wall, door and thatch visual assembled from the locally installed Medieval Village Building Kit | First prototype renders at gameplay distance. Original renderers remain as fallback and the 2 collision shapes stay in place. Do not propagate to the other six houses until the visual is reviewed. |
| NPC population | Several authored/simple silhouettes coexist with selected richer actors | Do not replace the entire population from the staged model pile. Candidate models include rigged and unrigged, incompatible, very high-poly, and license-restricted files; each needs an actor-specific import/rig/render review. |

## Ranked replacement sequence

1. **Trimery houses and gate structures.** Prototype one house using the already installed modular wall, thatch-roof, door and pillar assets. Preserve the existing footprint, NavMesh, collision, doors/interactions and a portable fallback; expand to the other buildings only after a gameplay-distance preview confirms fit and performance.
2. **Unify village NPCs.** Build a shortlist from sources with usable provenance and rig/animation evidence. Test one civilian in the existing town idle/approach validation before replacing any population.
3. **Biome-specific creature upgrades.** Consider only actors whose existing Blender or primitive model is visibly below the target and whose replacement has a valid rig/animation path. Keep class, AI and combat ownership outside the rendered model.
4. **Equipment and spell presentation.** Continue focused changes where the in-game screenshots identify an obvious mismatch. Preserve readable silhouettes and verify the final material/scale under the real gameplay camera.
5. **HUD texture changes.** Keep as the last art pass because this screen already has a consistent frame system and the screenshots show that tooltips, selection panels and information density matter more than wholesale reskinning.

## Trimery house prototype — 2026-10-26

- Unity `6000.6.3f1` is connected to the existing `DiceFree-Options` Editor and Pipeline. The prototype targets only `Timber merchant hall` in Cornberg.
- The local optional prefab is `Assets/_DiceFree/Resources/TrimeryLicensed/TimberMerchantHall.prefab`, assembled from the already installed and credited [Medieval Village Building Kit](ASSET_CREDITS.md). The prefab and vendor sources remain ignored by Git. A clean checkout uses the tracked authored fallback.
- The tracked adapter is `TrimeryArtSlot`; its new renderer list lets the local house model replace only rendering while retaining the original 8 fallback renderers and 2 colliders. The existing 8×7 m footprint, scene services and all other houses remain unchanged.
- Actual Scene-view and Game-view renders show the textured timber/thatch shell at gameplay distance. Game capture: `Temp/pipeline-screenshots/screenshot_game_20261026_201007_223.png` (local, ignored).
- Unity compile/recompile completed with no compiler errors. `dicefree.trimery.house-prototype.validate` passed (50 renderers; 8 fallback renderers; 2 collision shapes; 8×4.92×8 m local bounds). `dicefree.trimery.validate` passed all 6 navigation routes.
- The isolated `dicefree.october-playtest.playmode-test` initialized 15 local town art slots, converted their materials, and completed all 9 player walking destinations. After resume, its later boss phase failed at `Boss melee damage multiplier incorrect`; this unrelated gameplay result remains unresolved. The Editor returned to Edit Mode and the test uses an isolated temporary save root.

Next: human-review the local Game-view capture. If the style and gameplay-distance readability are approved, adapt the same optional-resource/fallback pattern to one more house, then reassess the palisade. No other buildings or assets were changed, and nothing was committed or pushed.

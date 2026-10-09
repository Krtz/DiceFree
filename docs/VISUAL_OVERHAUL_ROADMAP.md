# DiceFree — Visual Overhaul Roadmap
Status: PLANNING ONLY. Based on approved 90-question interview (see VISUAL_ART_DIRECTION.md), 2026-10-09.
Do NOT begin art imports, edit Unity scenes or merge without explicit follow-up authorization.

## Vision and constraints
DiceFree is an increasingly epic fantasy RPG, starting with simple level-1 village life and culminating in elaborate, surreal high-tier classes, enemies, equipment and environments. Realistic and stylized assets may freely coexist. Detailed, cinematic presentation must remain readable from an isometric camera and scalable via presets plus advanced user settings. Finish selected assets to polished quality rather than create large unfinished quantities.

Cornberg: small, cozy mixed-architecture farming hamlet, iconic *existing* lightly magical healing well linked to unusually large crops, realistic northern agriculture, prominent river and bridges, colorful slightly magical eastern forest where the slimes themselves remain the only overt supernatural objects. Strong landmarks, varied roads/elevation, modest quest-linked changes, accessible navigation. Important buildings are strong candidates for custom Blender work.
Slime Dungeon: deceptively peaceful, denser forest, natural paths, modest Big Slime clearing, root-twisted central tree with five integrated green glow rings, increasingly grand woodland amphitheater boss arena, comical top hat, secret royal Regent domain, out-of-world shared reward sanctuary. Puzzle and boss contract in SLIME_DUNGEON.md remains authoritative.
Classes: unique body/model for each class and high-quality class-specific animations; first art wave covers Novice, Physically Blessed Novice and Magically Touched Novice. Novice begins looking like ordinary villager. Every advancement brings noticeable class identity. Item identity outranks rarity, transmog overrides strict visible equipment when relevant. Future one class-mentor NPC per class generally gives corresponding advancement quests, secret exceptions allowed.

## Source-of-truth cross-check and related existing work
- docs/ART_PIPELINE.md documents existing Blender 5.2.2/Unity FBX skinning, rig, gloved Novice mesh and import/export validation; DO NOT recreate the pipeline wholesale. Earlier stylized-only/environment-not-photorealistic art guidance is superseded by approved mixed-background style, but silhouettes/readability still apply.
- docs/CORNBERG_COMBAT.md and CORNBERG_NORTH_MEADOW_IMPLEMENTATION.md govern well healing, farmland, routes, woodland perimeter/navmesh and current scene data.
- docs/SLIME_DUNGEON.md governs exact dungeon structure, five-ring puzzle, blue trees/slimes, main/secret boss paths, Slam and Divide, reward-room lifecycle.
- docs/SLIME_DUNGEON_ART_AND_CORNBERG_LIFE.md tracks existing models and NPC work.
- docs/ITEM_ICONS.md, UI_HUD.md and OPTIONS_KEYBINDS_VALIDATION.md provide starting points for UI, icon, options integration.
- Existing GitHub issues #37–46 cover sword, Novice rig/model, held/wearable items; #49–52 cover Novice/class implementations; #53 HUD; #54 transition/lifecycle; #55 Codex; #56 Slime Dungeon. New visual issues must extend, not duplicate, that work.
- External Asset Store catalog T:/TEMP/DiceFree-AssetCatalog/catalog.html and BLENDER_SHORTLIST.md covers 181 indexed downloaded packages. Candidates for isolated examination: Low Poly Nature Pack - PVA; Environment Forest - Low Poly 3D; AA Low Poly Medieval Environment; EMBERHOLD - FREE; Low Poly Modular Armors; Free Low Poly Modular Medieval Weapons; character meshes. Catalog paths are inventory, not license confirmation or asset approval.

## Stage 0 — Safe production foundations (VIS-00)
Create a branch from the validated checkpoint. Record current screenshot baselines (Cornberg, fields, forest, three classes, puzzle, boss room, Regent, reward room) with identical camera perspectives, graphics quality and lighting; capture existing metrics. Identify installed Blender executable or reproducible headless runner. Examine selected assets in isolated staging, inspect real meshes, compatibility, licensing and material requirements before importing selectively. Register source manifest and authored/modified outputs under SourceArt and project art tree. Establish scene/quest/NavMesh and telegraph regression harnesses. Draft Low/Medium/High/Ultra plus advanced control performance baseline before expensive shader work.
Gate: baseline files, manifest, test matrix and no scene changes.

## Stage 1 — Polished vertical sample across workstreams (VIS-01)
Deliver a SMALL but finished and cohesive representative sample, preserving the mixed-style vision:
1. Cornberg sample: custom/refined central healing well and immediately adjacent props; village role identity and one photogenic point of view, no healing changes.
2. Environment sample: one lovingly detailed segment of realistic northern farmland and one colorful forest path with deliberate navigable edges, plus a feasible river/bridge concept (do not relocate gameplay blindly).
3. Dungeon sample: one woodland clearing segment, a polished five-ring/root-tree prototype and a boss-arena composition sample. Keep blue-tree and capture-ring cues distinct.
4. Character sample: compare and polish presentation of all three current class forms, including coherent idle/movement/attack/cast examples; a small equipment identity example.
5. Rendering sample: location-aware cinematic light pass, subtle water/foliage motion, readable FFXIV-style telegraph and graphics-presets prototype at reduced effects.
Art review: same-angle before/after, short footage, original Blender sources, engine captures at Low/High, regression results, frame timing and milestone playtest. Full polish on each chosen sample; no production sprawl.
Gate: Axel explicitly approves milestone; merge only after validation + approval.

## Stage 2 — High-impact Cornberg/world environment (VIS-02)
Upgrade core village buildings as varied architecture (banker/blacksmith/merchant/alchemist/farmers), village green/well, distinct entrances/signage, northern fields and mature-crop lore expression, eastern forest tree variety/ground cover, visual river/bridges without severing paths, scenic landmarks and discoverability. Build restrained NPC bustle and subtle quest-linked changes. Check daytime/nighttime, occlusion, navigation, clipping, camera, click-to-move, spawn points and all Q1–Q5 triggers. Scope screens by walkable chunks for finished polish.
Gate: before/after scene tour, no quest regressions, visual signoff.

## Stage 3 — Slime Dungeon and royal secret (VIS-03)
Deepen believable natural woodland style, keep novice early dungeon welcoming; polish five-ring tree interaction and distinct normal/secret transformation, natural amphitheater regular boss, comical top hat and theatrical Slam/Divide, late secret royal Regent, reusable magical reward sanctuary with interchangeable décor. Preserve mob encounter counts, five-ring mechanics, >=50 blue-slime rule, boss telegraph safety, blue southern trees, mutually exclusive bosses and reward transfer. Test solo/party flow and scene reentry.
Gate: validate all mechanic routes plus readable VFX in minimum settings; manual gameplay review and signoff.

## Stage 4 — Three playable classes, gear and animation systems (VIS-04)
Design and finish distinctive polished Novice, Physical and Magical class models/rigs and class-specific idle, locomotion, melee, casting, reaction and advancement transformation. Preserve movement, collision, equipment sockets, ability mechanics, transmog logic, armor-fit and save/reload. Reevaluate existing SourceArt model-by-model; produce class-appropriate gear and item-specific transformative flourishes where warranted; class mentor visuals are a separate later track, not an urgent three-class deliverable.
Gate: pose/animation reel, gear-swap, character and battle cameras, motion readability and animation/playback test.

## Stage 5 — Lighting, water, weather and graphics performance (VIS-05)
Slow atmospheric day/night, region-authored weather, biome fog, art-directed cinematography, scalable water reflection and foliage motion. Low/Medium/High/Ultra presets AND granular controls for VFX density, shadows, fog, bloom, AO, reflections, foliage, weather, animation complexity as feasible; minimal-effects mode. Keep all vital boss telegraphs equally readable; debug FPS/GPU stats and per-scene budgets. Do not enable demanding effects by default on unsupported hardware.
Gate: preset and baseline screenshots/performance data for Cornberg and dungeon, accessibility and options persistence tests.

## Stage 6 — Growth pipeline and future regions (VIS-06, later)
Build a documented evolution ladder for Tier 1–4 and level 200, unique class mentor NPCs and exceptional secret-class discovery art, archetype/family-specific monster evolution, transformative legendary gear, and region-specific visual language across six faces. These are design/asset pipelines, not a request to implement every class now.

## Production method / acceptance shared across issues
- Every art issue includes asset inventory, permitted source/license, Blender .blend/procedural source, exported files, scene authoring steps, shader/material variants, screenshot comparisons and gameplay integration evidence.
- Preserve originals outside the game and track asset provenance; no wholesale Unitypackage imports or raw third-party redistribution.
- Reuse existing art tooling and GitHub issues; architecture/runtime gameplay changes only if needed and tested.
- Each polished deliverable has an isometric silhouette check, collision/NavMesh/quest test, class gear/transmog integration when applicable, high/low graphics capture and manual review.
- Work can be autonomous *within* approved art direction; major game-rule changes and milestone merging remain separately controlled.
- No art implementation authorized at roadmap creation.

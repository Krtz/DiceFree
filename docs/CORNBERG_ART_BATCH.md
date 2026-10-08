# Cornberg Blender asset rollout — 2026-10-08

Branch: `feature/54-world-dungeon-codex` (unmerged). This batch builds and installs actual Blender 3D assets; no generated reference pictures are production content.

## Authored files

- 27 editable, compressed native Blender files in `SourceArt/World/CornbergBatch/`.
- 27 Unity FBX imports in `Assets/_DiceFree/Art/World/CornbergBatch/`.
- One deterministic generator, `SourceArt/build_cornberg_batch.py`, with generated `asset_manifest.json`.
- Nine separate tree species (Pine, Spruce, Oak, Birch, Maple, Willow, Twisted, Aspen, Cypress), with five material palette variations for foliage.
- Eight generic NPC role meshes: Farmer, Merchant, Banker, Alchemist, Blacksmith, Villager, Guard and Herbalist.
- Nine slime varieties: Crop, Meadow, Road, Forest, Amber, Elite, Boss, Regent and future Royal. Transparent gel shell/core, internal decorative inclusions, tier-dependent bodies/details, antenna on several types and glowing eyes on higher tiers.
- Cornberg mountain with separately animatable left and right gates.

## Installed Cornberg changes

The scene was **modified additively**, not recreated. Existing quest givers, advancement components, save bindings, health/combat actors, loot definitions and story NPCs remain.

Unity installer reports:
- 395 existing woodland placeholders replaced with Blender tree meshes;
- 129 additional 3D trees in expanded northeast forest;
- 27 additional live slimes (31 slime actors total; five crop slimes across two cornfields);
- nine generic NPC role meshes placed near village functions;
- all four pre-existing slime enemies given new art;
- landscape expanded to cover the northeast zone and NavMesh rebaked;
- advancement mountain with opening gate and separate `NoviceMountainTrial.unity` map/NavMesh.

The old world geometry bounds extended approximately x=-66..134 and z=-54..82; the new ground extends x=-66..254 and z=-54..190. Cornberg's quest positions remain anchored in the southwest while playable geography grows northeast. Existing village coordinates, quests and movement routes are not arbitrarily translated.

## Progression rules

The mountain is eligible for a **level-10 Novice** via the existing advancement definitions. The actual gate has two animatable door leaves. The separate mountain trial scene contains an advancement staging area with physical/magical altars; it is **not the Slime Dungeon**. The player session/character is intended to survive additive map travel. Details of the first Slime Dungeon will be designed with the user later.

## Limitations / deliberate deferrals

- Generic NPC role models are Blender-authored static meshes at present, not fully rigged or newly interactive merchant/bank/profession behaviors. Original story couple remains untouched.
- Named high-tier slime bosses have Blender assets, not newly authored boss fights or loot tables.
- The Slime Dungeon interior, floorplan, encounters and routes have not been designed or implemented in this batch.
- Future refinement may improve vertex detail, animation, material realism, art collision LOD and individual foliage palettes; the source/export pipeline preserves editability.

## Commands and validation

`dicefree.mountain.author-trial` authors the separate map; `dicefree.cornberg.art-install` applies the 3D scene batch once; `dicefree.cornberg.art-validate` checks geometry, trees, monsters, gel transparency, navigation and mountain-map presence. Existing advancement, HUD, Codex and Novice presentation regression suites must remain green.

The install command is intentionally not idempotent: it refuses to add a second copy over a saved scene. Use a clean feature worktree/undo if you intend to re-author.

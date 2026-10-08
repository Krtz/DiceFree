# Cornberg world art batch (real Blender source)

This directory contains 27 independently editable Blender 5.2.2 source files, plus the reproducible procedural authoring manifest. The canonical generator is at `SourceArt/build_cornberg_batch.py`. Unity imports paired .fbx files from `Assets/_DiceFree/Art/World/CornbergBatch/`.

## Rebuild on the Windows development machine

From the project root:

```powershell
& 'T:\TEMP\blender.exe' -b -t 4 --python 'SourceArt\build_cornberg_batch.py'
```

This recreates the .blend and FBX pairs and `asset_manifest.json`. Source filenames, exported FBX paths and asset categories are listed in that manifest.

## Included meshes

- Trees: Pine, Spruce, Oak, Birch, Maple, Willow, Twisted, Aspen, Cypress. Each uses distinct recolorable bark/foliage/foliage-accent material names. The Unity installer distributes five shared foliage palettes.
- NPCs: Farmer, Merchant, Banker, Alchemist, Blacksmith, Villager, Guard, Herbalist. These are static 3D role meshes with recognizable trade clothing/props. **They are not yet rigged or interactive NPC controllers.** Original quest NPCs are preserved.
- Slimes: Crop, Meadow, Road, Forest, Amber, Elite, Boss, Regent and RoyalFuture. Each has a gel shell and inner core, visible eyes and larger tier-dependent interior floating details. Several higher tiers have antennae, crystals, glowing eyes or regal ornamentation.
- Mountain: CornbergAdvancementMountain with distinct DoorLeft and DoorRight meshes and scene-side opening controller.

The blender generator uses metric scaling; Unity runtime materials are authored separately in `Assets/_DiceFree/Materials/CornbergBatch/`, with URP transparent `Gel` and `GelInner` materials.

## Unity integration

`dicefree.mountain.author-trial` creates the separate level-10 Novice advancement map/NavMesh.

`dicefree.cornberg.art-install` applies Blender prefabs to the existing Cornberg world, replaces visual-only tree/slime placeholders, expands the terrain northeast and rebakes navigation. It keeps quest, collision, AI and save objects from the existing scene. This installer **must not run twice** on an already installed scene; it fails rather than duplicate scenery.

`dicefree.cornberg.art-validate` verifies the installed world, transparent gel and asset presence. Other existing validations cover HUD, Novice presentation and advancement.

## Scope and pending art polish

These are real editable geometry assets and first-pass game-ready prototypes, not concept paintings. Some static NPC character kits need future rigging/animations, facial detail and authored dialogue/merchant/bank components. The future Slime Regent/royal models are art assets only; their combat and loot will be authored when those encounters are designed.

The **Slime Dungeon interior is intentionally NOT authored in this batch**. The user will co-design it separately. The separate mountain advancement trial map is not the Slime Dungeon.

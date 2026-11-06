# Wolf dungeon icon identity — 2026-11-06

The user asked for a much stronger wolf theme after the first item icons looked too generic. All six inventory/reward illustrations are now a cohesive wolf equipment set with weathered silver, grey fur, ivory fangs, claw settings and moonstones.

| Item | Visible design | Saved icon |
| --- | --- | --- |
| Moonfang Signet | Large snarling wolf-head signet, moonstone eyes, claw-engraved band | `Assets/_DiceFree/Art/ItemIcons/WolfRewards/item.wolf-survival.moonfang-signet.png` |
| Nightpelt Vest | Shaggy wolf-fur mantle, fang clasps, wolf-head chest medallion, claw-mark buckle | `Assets/_DiceFree/Art/ItemIcons/WolfRewards/item.wolf-survival.nightpelt-vest.png` |
| Moonwell Staff | Howling wolf inside a crescent, green moonstone, fur bindings and fang charms | `Assets/_DiceFree/Art/ItemIcons/WolfRewards/item.wolf-survival.moonwell-staff.png` |
| Starfall Wand | Living silver wolf head gripping a blue moonstone, fang guard, fur grip | `Assets/_DiceFree/Art/ItemIcons/WolfRewards/item.wolf-survival.starfall-wand.png` |
| Packhunter's Fang | Hooked canine-shaped blade, wolf-head guard, curved fangs and fur-wrapped handle | `Assets/_DiceFree/Art/ItemIcons/WolfRewards/item.wolf-survival.packhunters-fang.png` |
| Duskwake Wand | Ivory wolf skull, three hooked claw prongs, violet moonstone and hanging fangs | `Assets/_DiceFree/Art/ItemIcons/WolfRewards/item.wolf-survival.duskwake-wand.png` |

## Production

The downloaded asset catalog was checked for wolf, weapon and icon candidates first. The selected new illustrations use the **built-in `image_gen` tool**, one call per item. The ring establishes the style reference for the other five assets. The exact final prompt set, generated filenames and selected PNG SHA-256 hashes are recorded in `docs/assets/wolf-dungeon-icon-art.json`.

Generated originals remain in the tool's default generated-images directory; all six selected images are copied into the project paths above. The preceding icons and import metadata are preserved locally under `Logs/WolfIconArtBefore`. No vendor package was imported.

These are inventory and reward-preview illustrations. Existing in-world class weapon/body presentations are unchanged. Item IDs, definition files, stats, class eligibility, drop rates and reward logic remain unchanged.

The source PNGs retain their generated resolution and actual alpha channel. Unity imports them as 512px uncompressed Sprites with alpha edge handling and no mipmaps. The existing texture GUIDs and item Sprite references are preserved. `WolfIconArtImport.cs` checks all six references, dimensions and alpha. The older procedural `WolfRewardsAuthoring.RenderIcons` path now respects the selected-art manifest, preventing a later authoring pass from overwriting the curated images. Calling `RebuildIcons` was verified to leave every selected PNG hash unchanged.

## Verification

All six images were inspected at full size and in the standalone game: the inventory draws them at 52px within its 58px slots, and the reward cards draw 44px previews. The staff/wand colors and the ring, vest, crescent staff, curved blade and skull silhouettes distinguish the roles at those sizes.

Final Windows build: `Builds/BriarbackMilestone-20261106-181443/DiceFree.exe`, eight scenes, 476,405,173 bytes, **zero errors and warnings**. The isolated-profile art smoke captured all six inventory icons and the normal reward cards, completed the unchanged ten-wave clear, claimed an item and verified all artwork/ownership after saved resume. It observed zero Unity errors. Normal Quit exited PID 23972 with code 0.

Standalone captures and consolidated results are in `VisualBaselines/WolfIconArt`. All existing item-definition bytes, 54 pre-existing modified files and seven protected settings are preserved. Work is local on `art/wolf-dungeon-icon-identity`; no push or merge is included.

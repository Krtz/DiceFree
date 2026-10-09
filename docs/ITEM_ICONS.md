# Equipment item icons

Eight unique 192x192 transparent PNG sprites live in `Assets/_DiceFree/Art/ItemIcons`, named by stable item ID. ItemDefinition.icon is persisted by the Unity AssetDatabase. Runtime save ItemInstance/EquippedItem JSON fields are unchanged.

## Source mapping

| Item | Source |
|---|---|
| Bronze Dagger | Art/Weapons/BronzeDagger/BronzeDagger.prefab, authored by CornbergEconomyAuthoring |
| Cornberg Work Gloves | Art/Characters/CornbergWorkGloves/Prefabs/CornbergWorkGlovesPresentation.prefab |
| Farmer's Pants | Art/Characters/FarmersPants/Prefabs/FarmersPantsPresentation.prefab, prepared by FarmersPantsArtValidation |
| Forest Travel Shoes | Icon-only procedural pair of leather boots with soles, cuffs and laces; no dedicated source art exists |
| Slime Orb | Art/Equipment/SlimeDungeon/SlimeOrb.prefab |
| Slime Shield | Art/Equipment/SlimeDungeon/SlimeShield.prefab |
| Slimy Farmer's Gloves | Art/Equipment/SlimeDungeon/SlimyFarmersGloves.prefab |
| Slimy Tophat | Art/Equipment/SlimeDungeon/SlimyTophat.prefab |

All art paths are relative to Assets/_DiceFree. Existing sources are loaded with PrefabUtility.LoadPrefabContents, cloned and unloaded without saving. Skinned art is baked in its rest pose. Work glove copies are brought together for framing; dagger is rotated upright. Shoes exist only during rendering and do not replace equipped art.

## Commands

Run from this project with the connected Editor in stable Edit Mode:

```powershell
unity command set_autotick --enable true
unity command recompile
unity command recompile_status
# Wait for completed/up_to_date with failed=false before proceeding.
unity command dicefree.item-icons.install
unity command dicefree.item-icons.validate
unity command dicefree.item-icons.gui-open
# Allow the window to repaint, then:
unity command dicefree.item-icons.gui-result
```

Install uses PreviewRenderUtility's isolated preview scene, orthographic camera, two consistent lights, gentle emission fill on temporary material copies, automatic bounds framing, 384x384 rendering downsampled to 192x192 and a subtle alpha silhouette shadow. It imports Single Sprites, sRGB, input alpha, transparency, bilinear filtering, no mipmaps and uncompressed textures. Existing PNG/meta paths are reused for stable GUIDs. Only icon references are modified through SerializedObject; Unity may materialize previously omitted default fields during serialization. Item stats/IDs/loot/price remain unchanged. No scene is opened, rebuilt or saved and no ProjectSettings or source art is modified. No AI image generator is used.

Inventory, equipped HUD, shop and private reward choice rows use ItemIconGUI. It draws alpha-composited sprite UVs inset and fitted within the slot, preserves GUI color, and has no input handling. Callers retain their buttons, tooltip regions and missing-icon abbreviations. Reward rows size to the offered item count and retain the alternative EXP/gold choice.

Validation enumerates all ItemDefinitions (minimum eight), checks unique IDs, unique sprites and PNG hashes, PNG decode/content/transparent pixels, resolution, import settings, fitted GUI geometry and static integration/button/tooltip presence in all four UI surfaces. GUI validation executes actual IMGUI repaint draws at inventory and compact equipment sizes and checks missing-icon fallback without entering Play Mode or touching player saves. It is a lightweight rendering check, not an automated end-to-end purchase/equip/reward interaction test.

Future ItemDefinitions need an explicit source mapping in ItemIconAuthoring before install will run. Atlas-packed/tight custom sprites are outside this pipeline; generated icons are standalone Single Sprites. Source rest-pose details and the handmade shoe approximation are the main art limitations.


## Verified local result

Unity 6000.6.3f1 compiled the icon code with failed=false and no compiler errors. Install and validate returned ITEM_ICONS_VALIDATED: eight definitions, eight unique sprites, eight distinct PNGs and four UI surfaces. The actual IMGUI repaint check returned ITEM_ICONS_GUI_VALIDATED with 16 renders and fallback=true. Source scene/ProjectSettings/dungeon prefab hashes matched the snapshot taken before authoring, and PNG importer meta hashes remained stable across repeated installs. The Editor stayed ready in Edit Mode; no gameplay save was touched. All eight output PNGs were visually inspected. No commit, push, merge or branch change was performed.

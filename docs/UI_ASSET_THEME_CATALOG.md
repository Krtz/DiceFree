# DiceFree UI Asset Packs and Themes — 2026-10-12

## Inventory and implementation

- 240 Unity Asset Store packs indexed locally (30 new since the prior 210-pack audit).
- All 30 new UI-related packs now have selectable theme presets; five original themes remain (35 total).
- 28 themes use actual locally installed panel/slot imagery. Two packs with no independent usable texture use procedural theme art.
- The original third-party sprites are LOCAL AND GITIGNORED under Assets/_DiceFree/Resources/UiThemesLicensed; none is committed to GitHub. Public clones have fallback color/chrome for all 35 presets.
- Existing HUD widgets, positions, sizes and theme preferences persist. Pack art decorates the HUD chrome and slots; this is not a wholesale replacement of every custom Unity UI prefab/font/animation.
- Options now has tabs for Gameplay, Audio, HUD / Themes, Keybinds and Session. Keybinds has its own scrollable page.

## Included pack presets

| Theme | Downloaded UI pack | Artwork locally installed? | Surface |
|---|---|---|---|
| Bloodlines | Bloodlines - Dark UI | Yes | panel |
| Controller | Game Input Controller Icons Free | Yes | slot |
| Dark Brown | Dark Brown GUI kit | Yes | panel |
| Fantasy Wood | Fantasy Wooden GUI Free | Yes | panel |
| RPG Spell Icons | FREE - RPG Fantasy Spell Icons | Yes | slot |
| Battle Skills | Battle Skill Icon | Yes | slot |
| Concrete | Basic UI Elements Concrete | Yes | panel |
| Metal | Basic UI Elements Metal | Yes | panel |
| Woodland Camo | Camo UI Elements Woodland | Yes | panel |
| Steampunk | SteampunkUI | Yes | panel |
| Pixel HUD | Pixel HUD UI Free Kit | Yes | panel |
| RPG Buttons | 2D RPG Button 7 | Yes | slot |
| Simple Elements | Simple UI Elements | Yes | slot |
| Color Manager | Color Manager | No (procedural) | panel |
| Simple Fantasy | SIMPLE FANTASY GUI | Yes | panel |
| Elemental Meters | Elemental Meters | Yes | slot |
| Sweet Land | Sweet Land GUI | Yes | panel |
| GUI Parts | GUI Parts | Yes | panel |
| Spellbook | SpellBook Preface | Yes | slot |
| Stone UI | Stone UI | Yes | panel |
| Adventure | Adventure UI Kit | Yes | panel |
| Colorful | Colorful UI Kit | Yes | panel |
| Jelly | Jelly pack UI | Yes | panel |
| Soft Touch | Soft Touch UI Kit Demo | Yes | panel |
| Spooky Horror | Spooky Horror UI Kit | Yes | panel |
| Magic Spell Icons | Magic Spells - 40 Fantasy Icons 512x512 HD | Yes | slot |
| Shields | Shields - 40 Fantasy Icons 512x512 HD | Yes | slot |
| Animated Wood | Wooden GUI UI Animation Extension | No (procedural) | panel |
| Unity Examples | UI Examples uGUI Sample Project | Yes | panel |
| Free UI | Free Ui Pack | Yes | slot |

## Gameplay fixes

- Minimap: X enters attack-move targeting; click the minimap to confirm; Shift appends additional attack-move orders. Normal movement clicks still work.
- Wolf and Quillrat HP increased by 10x relative to the prior implementation: Quillrats 7x base, Grey and Dark Wolves 12x, and Alpha Wolf 65x.
- All 30 Quillrat palette materials repaired to URP Lit with individual color variants intact; no longer use a built-in incompatible Standard shader.

## Validation and remaining quality checks

- Unity CLI recompile passed with no C# errors or warnings.
- Live Unity editor command dicefree.october-ui.validate passed: 35 themes, 28 local skins, five tabs, 30 URP materials.
- Isolated Play Mode test OCTOBER_MINIMAP_HP_PLAYMODE_OK passed: 22 wildlife actors with new HP and two sequential Shift-queued minimap attack moves.
- User should review art stretching, readability, slot overlays, quillrat colors and 10x HP gameplay balance in the actual Unity Game view.
- Local indexes: T:/TEMP/DiceFree-AssetCatalog/packages.json and ui_theme_manifest.json.
- Do not redistribute original downloaded Unity Asset Store PNGs in public source control; confirm rights separately before sharing a compiled game with included textures.
- Preserve unrelated ProjectSettings/PackageManagerSettings.asset, ProjectSettings/ShaderGraphSettings.asset and Assets/_DiceFree/Resources/Audio/IronpineLicensed.meta changes.

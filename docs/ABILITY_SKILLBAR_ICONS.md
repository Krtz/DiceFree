# DiceFree action-bar ability icons — October 12, 2026

Branch: `feature/visual-overhaul-vis00`. Unity 6000.6.3f1.

## What the user will see in-game

The real 12-slot **GenericActionBar** no longer shows two-line text buttons for its four occupied slots. Every playable Novice, Physically Blessed and Magically Touched active ability now renders actual square action-RPG icon art, with compact hotkey and rank badges in the corners. Cooldowns continue to darken the icon and display their remaining time. Hover tooltips, skill clicks, skill point ranks, and the other eight empty slots retain their functionality; a different class automatically looks up different icon artwork.

The art is matched by **skill kind**, not by keyboard number or slot index, in:
- `Assets/_DiceFree/Scripts/UI/SkillIconLibrary.cs` — mapping, optional local asset loading, distinct code-generated fallback sigils.
- `Assets/_DiceFree/Scripts/UI/AbilityBarSource.cs` — `SlotView.iconKey` and `SlotView.icon` for all three class progression types.
- `Assets/_DiceFree/Scripts/UI/GenericActionBar.cs` — actual icon, key/rank overlay and preserved cooldown/hit-target/tooltip handling.
- `Assets/_DiceFree/Scripts/Tools/Editor/SkillIconAuthoring.cs` — local texture importer configuration and validation.

## Authored asset bindings

The 14 icon assets below were selected from the owner's previously downloaded **Battle Skill Icon** Unity Asset Store pack, using its existing imagery instead of reillustrating the game:

| Class and skill | Icon key | Original pack art |
|---|---|---|
| Novice STR melee stun | `novice-stun` | `S_Combo_knock.png` |
| Novice Magic Sand | `magic-sand` | `S_rock_ball.png` |
| Novice AGI attack speed | `novice-quickness` | `S_Fallbreeze.png` |
| Novice SPI Heal | `novice-heal` | `S_divine.png` |
| Physically Blessed Heavy Strike | `heavy-strike` | `S_noble_hit.png` |
| Physically Blessed Guard | `guard` | `S_witch_ward.png` |
| Physically Blessed Quickening | `quickening` | `S_Forward_ray.png` |
| Physically Blessed Arrow Rain | `arrow-rain` | `S_Arrow_crowd.png` |
| Magically Touched Magic Sand | `magic-sand` | **Same identity and icon as Novice Magic Sand** |
| Magically Touched Mend | `mend` | `S_sparkle_stream.png` |
| Magically Touched Fire Imbuement | `fire-imbuement` | `S_fire_furious.png` |
| Magically Touched Ice Burst | `ice-burst` | `S_Forward_freeze.png` |
| Novice All Stat passive | `novice-passive` | `S_essence_found.png` |
| Physical Martial Aptitude passive | `physical-passive` | `S_Forward_steel_attack.png` |
| Magical Mana Attunement passive | `magical-passive` | `S_spell04.png` |

Passives are reserved for a later icon pass on the class skills panel, not force-added to the active 12-slot action bar.

## License and portability

The 14 actual Unity Asset Store PNGs are extracted **locally on Kratz's authorized Windows PC** into `Assets/_DiceFree/Resources/AbilityIconsLicensed/`. The full directory and `.meta` are gitignored, **never committed**. Local source mapping/manifest is recorded in `T:\TEMP\DiceFree-AssetCatalog\skill_icon_manifest.json`; the contact sheet is local only.

When the licensed source pack is not installed, `SkillIconLibrary` creates distinctive original 64px spell-symbol fallback textures with readable elemental colors and silhouettes; the action bar never reverts to blank icon boxes. The fallback implementation, all mappings, GUI renderer and validation **are** redistributable.

To reconfigure imported textures, use the Unity menu **DiceFree → UI → Refresh local ability icons**. `dicefree.skill-icons.install` configures them as unmipped uncompressed UI textures and `dicefree.skill-icons.validate` checks all 14 artwork assets and mappings.

## Verified

- Original downloaded pack inspection: 14 different PNG files extracted and contact sheet reviewed on the PC.
- Isolated Unity 6000.6.3f1 Editor batch: `SkillIconAuthoring.Install` and `SkillIconAuthoring.Validate`, both exit code 0 with no new compiler errors.
- Isolated Unity Play Mode: `DICEFREE_SKILLBAR_ICONS_PLAYMODE_OK`, confirmed the real `GenericActionBar` and `AbilityBarSource.View` return four correct, non-null, **pack-backed** icons, with original Ironpine smoke test also passing.
- Unity's older `SearchDatabase` indexing exception may still appear when starting the isolated headless editor; the gameplay checks reach their success markers.
- Actual desktop Game-view art legibility and layout should still be checked by the user at their selected HUD scale.

Do not merge to main without explicit approval. Preserve unrelated local soft VFX resources, package settings and ShaderGraph settings. The original generated Magic Sand/Heal/Stun/Strike art from earlier in the conversation is available as a separate direction if preferred later, but the installed first pass uses the user's existing ability-art pack.

## Later update — shared command icons (2026-10-12)

The asset library now contains **17 locally installed ability art PNGs**, including three more from the same Battle Skill Icon pack for shared action slots: Attack / Attack Move (slot 5), Interact (slot 6), and Return to Revive Stone (slot 7). All three use pack artwork when available and distinct original fallback symbols otherwise. The prior 14 art mappings remain unchanged. See `docs/SHARED_SKILLBAR_AND_RETURN_HOME.md` for implementation and validated travel behavior.

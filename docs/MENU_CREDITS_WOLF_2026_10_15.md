# Tiered start menu, equipped portraits, Credits and Alpha Wolf loot

The start menu now groups saved manifestations into tier tabs and shows each saved class as a small full-body 2D rendered portrait with its equipped items. Options includes searchable Credits. The normal-world Alpha Wolf can drop the first Wolf-Fang Greatsword.

## Player-facing behavior

- Tier 0: Novice. Tier I: Physically Blessed and Magically Touched. Tier II: Greatblade. Empty later tiers remain visible; class tiers come from the authored advancement graph.
- The last-active manifestation's tab opens by default. Each card shows the Echo/class title, saved level and Enter world. Unknown saved classes remain preserved in an Unavailable tab and cannot be launched.
- New Echo naming, safe renaming, refresh, the development room and multiplayer lab remain accessible.
- Portraits read the manifestation's own inventory/equipment. They use visual-only prefabs without CombatActor, Health, AI, navigation, input or persistence components. They render once per refresh, retain their RenderTextures until menu cleanup, and do not write saves. An invalid preview falls back to unavailable rather than breaking the menu.
- The existing session Options host is reused, avoiding duplicate buttons or modal services. Credits is available from the main menu and in-game Options.
- Credits has 40 entries on this installation: Unity plus 39 in-use asset packs. Sources, publisher names, license links and the full shader compatibility notice are included. Missing optional asset packs are filtered by their resource probes. Staged downloaded candidates are separate.

| Wolf-Fang Greatsword | Value |
| --- | --- |
| Drop source | Cornberg / Ironpine normal Alpha Wolf |
| Drop chance | 10% per life |
| Slot | Main hand, two-handed |
| Basic attack damage bonus | +10 |
| Strength | +5 |
| Class restriction | None |
| Appearance | Plain broad steel blade, dark leather grip, small ivory guard fangs and a simple wolf-head pommel |
| Ownership | Normal item instance, loot pickup and save flow |

Equipping this sword clears the offhand slot while retaining the offhand item in the bag. An offhand cannot be re-equipped while it is wielded. Switching back to a one-handed weapon releases that restriction. Saved conflicting two-hand/offhand slots normalize safely. The earlier plain model-testing offhand exception still works with one-handed gear.

The sword model and icon are first-party primitive art. No downloaded candidate model or raw vendor package was added.

## Verification

| Check | Result |
| --- | --- |
| Focused menu/credits/wolf runtime suite | 55 assertions passed |
| All eleven plain gear slots / four classes / flexible skills | 232 assertions passed |
| Greatblade playable combat/progression regression | 75 assertions passed |
| Development room, World 2 round trip and menu return | Passed |
| Static/data regressions | 12 commands passed |
| Windows standalone startup | Passed 15-second isolated batch/nographics smoke; no error matches |
| Windows Development build | Succeeded; 7 scenes, 0 errors, 0 warnings; 354,090,364 bytes |

The focused suite seeds an isolated temporary Echo with all four classes and equipped gear. It checks rendered nonempty portraits and exact saved gear IDs, no gameplay actor/persistence components in the previews, tier grouping, future-class preservation, Credits navigation and full notice retention, and byte-for-byte save stability while browsing. It also checks all-class two-handed equip, exact stat deltas, offhand restoration, real Alpha Wolf health/defeat events, random values below/at/above the configured threshold, duplicate defeat suppression, respawn rearming, pickup without duplication, saved item identity and return to the selected tier.

The static regressions cover advancement, Physical, Magical, Novice skills, architecture, dependency policy, managed references, quadratic XP/save migration, skill UI, Greatblade prototype data, all six Options tabs, and Ironpine wildlife.

Actual Editor Game-view images were inspected for both Tier I portraits, Greatblade with the new sword, and Credits. Screenshots use a synthetic isolated test profile; the Unavailable tab deliberately exercises future-class preservation. This is not a manual Windows playthrough.

Evidence: [reports](validation/) and [screenshots](screenshots/). Files for this task use the Menu prefix. Credits provenance: [registry](ASSET_CREDITS.md), [manifest](../SourceArt/Credits/credit_manifest.json), [source map](assets/runtime-credit-source-map-2026-10-15.json).

Windows executable: T:/TEMP/DiceFree-Options/Builds/World1Foundation/DiceFree.exe. Use the complete build folder.

## Reproduction / maintenance

1. Author or refresh visual-only menu prefabs, the catalog, item and Alpha loot with dicefree.menu-wolf.author in idle Edit Mode. Item references are reacquired after opening each scene; both Cornberg and DevelopmentRoom inventories include the sword.
2. Edit SourceArt/Credits/credit_manifest.json and run dicefree.credits.author to refresh the runtime CreditBook and copied notice. Preserve the source/creator/resource mapping when adding art.
3. Run dicefree.menu-wolf.test, poll dicefree.menu-wolf.status, capture at tier1_ready / greatblade_ready / credits_ready, and use dicefree.menu-wolf.resume at each pause. The alternative dicefree.menu-wolf.verify runs the same assertions without capture pauses.
4. Re-run dicefree.gear-skills.test/status/resume, dicefree.greatblade.playable.test/status/resume and dicefree.development.playmode-test/status as appropriate.
5. Build with dicefree.development.build-windows.

Save schema remains 6; the item handsRequired field is definition metadata. Existing one-handed definitions retain the default of one hand. Changes stay on integration/world1-foundation; unrelated package/settings files are excluded.

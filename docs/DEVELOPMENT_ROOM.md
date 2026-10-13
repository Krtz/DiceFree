# Development room and map-transition test

Open **Development room** from StartMenu. This creates or loads a separate test Echo under the DevelopmentRoom subdirectory of the normal save root. It never edits the normal Echo's progress. Returning to the menu ends development mode and restores the normal save selector.

Included controls:
- Novice / Physical / Magical manifestations using the real existing class systems.
- Level 1–200 changes; lowering level clears skill allocations to avoid invalid point budgets.
- Restore health/resources and reset three no-reward training targets.
- Grant catalog items, then use the existing inventory/equipment UI.
- Shared Greatblade player/retired-swordswoman visual previews.
- A real portal and a control button to a separate World 2 test scene, and return travel.

Travel loads the destination scene additively, rebinds the same player's NavMesh agent, camera/fog/minimap and revive anchor, moves session objects to the destination scene, then unloads the previous map. HP, Mana, class/level, carried/equipped item IDs, gold and the player's runtime identity remain intact. This is a development-only test of map lifecycle. Normal World 2 story, biome, access requirements and durable world-position policy remain future work.

Validation: `dicefree.development.playmode-test`, followed by `dicefree.development.playmode-status`.
The live isolated test passed `DICEFREE_DEVELOPMENT_ROOM_PLAYMODE_OK`: menu route, isolated save path, real class and level controls, equipment, two scene transitions, no duplicate player, HP/Mana continuity, scene unloading and return to unchanged normal Echo bytes.

Authoring is reproducible with `dicefree.development.author`; it edits only its two new scenes and development data, using a copy of the Cornberg player rig. The original Cornberg scene is reopened afterward.

The Windows Development build command is `dicefree.development.build-windows`. It includes all enabled scenes, checks StartMenu boots first and both test maps are included, and writes its build report next to Builds/World1Foundation/DiceFree.exe.

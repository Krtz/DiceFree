# Arborium world connections — working direction, 2026-11-06

The user proposed the following while asking for immediate wolf-loot polish. These are recorded as **tentative future direction**, not final geography, quest writing or implemented travel unlocks.

- **Arborium as the continuing hub:** consider making the city useful across later worlds, rather than abandoning it after World 1. Its guild, banking, artisans and magical services should have room to grow. The exact inter-world travel method and unlocks remain open.
- **Guild-led southern passage:** an Adventurers' Guild quest chain could involve Trimery, the forestry town, clearing enough trees to open a passage southward toward World 2. Establish a satisfying sequence of player activities and visible town progress before choosing quantities or rewards. The southern passage's exact placement and World 2 destination are undecided.
- **Restore the direct Trimery road:** a separate optional investigation could uncover what happened to the existing impassable Trimery–Arborium road, lead to a boss there, and restore it as a useful shortcut. Preserve the original north → west → south Quillboar detour and initial city access. Inspect earlier road evidence before settling its cause, boss identity or physical repair.
- **Explore beyond the city:** what lies west and north of Arborium remains open. The known northern gate and Quillboar arrival route already exist; new geography must extend that continuity, rather than overwrite it.

The immediate implementation pass concerns the visible wolf dungeon equipment. It does not create a new boss, open either passage, invent the magical order's governance, or move advancement mentors.

## Subsequent playable foundation

The next continued-development pass adds the guild timber commission, five persistent clearing sites, and a three-part road survey described in `ARBORIUM_GUILD_FORESTRY.md`. It preserves the established Roadwarden cause and mystery from `TRIMERY_IMPLEMENTATION_V1.md`. Both passage unlocks and the wider world-hub proposal remain future work.


## 2026-11-10 — Approved surrounding-region design intent and Dragon storyline continuity

The user specifically wants to **design more areas around Arborium**. Exact surrounding map, landmarks, biome, level ranges, enemies, towns and routes are **not yet selected**; capture these through design rounds before Codex builds them.

### Preserve currently established geography and project work

- Existing entry path is **Trimery → north through Briarback/Quillboar region → west → south to Arborium's fortified northern gate**.
- The existing **Trimery–Arborium damaged road** has its own Ogre Roadwarden story and restoration work. Do not redefine the Roadwarden's orders or silently move that route.
- Arborium has established **west and east gates** plus northern approach; their use for future surrounding areas is open. The tentative city-as-future-hub and southern World 2 passage remain optional proposals rather than finalized physical links.
- Existing Arborium Guild forestry commissions and survey sites are separate from **required dragon-investigation guild quests**, which are newly approved as their own line.

### Confirmed main-story context for surroundings

- A **hidden lair somewhere in World 1** is the Ancient Road Dragon's final boss location, exact placement open. Dragon gathers existing regional factions including **Quillboars and wolves**; it recognizes the Echo but refuses to explain why.
- World 1 ends with **the dragon's death**. The Verdant Order sends the adventurer on to World 2 to investigate why the original DiceBound boss appeared outside the Dice. No specific World 2 clue has been approved.
- It is permissible to reserve space for future monster-group investigations, lair approaches or wilderness regions, but **do not pick the actual terrain/positions, entrances, threats, level bands or compulsory quest gates without user decisions**.

See [Arborium design](BIG_CITY_DESIGN.md), Round 9, for user-approved CITY-047–052 and supersession of older “pursue surviving dragon” drafts. No Unity work is authorized by this update.


## Arborium Surroundings — Round 1 (2026-11-10, USER CONFIRMED)

The following decisions establish macro geography and level/quest intent. These are **design instructions for a future implementation**, not evidence that the areas, monsters, paths or instance already exist in Unity.

| ID | Topic | User-confirmed decision | Explicitly open / candidate |
| --- | --- | --- | --- |
| SURR-001 | Overall environment | Arborium's wider surroundings mix **fields, forests, rivers, hills and rocky terrain**, rather than one homogeneous terrain type. | Specific river courses, elevations, named places and boundaries remain open. |
| SURR-002 | **West of Arborium** | An extensive **ancient forest with dangerous creatures**, expanding with more forest and monster areas where players can **continue levelling after Briarback**. | Forest name, internal subzones, enemy roster, exact levels, roads and quests remain open. |
| SURR-003 | **East of Arborium** | **Trimery is to the EAST of Arborium** in the broader World 1 regional layout. Preserve the existing Trimery–Arborium road, Roadwarden history and detour through Briarback. | The idea of **Nature monsters, including living plants and mushrooms**, in an eastern area/corridor is a **user-suggested possibility**, NOT a locked biome or spawn list. Do not replace Trimery with a new region or silently move existing authored paths. |
| SURR-004 | **North beyond west forest** | There should be **another new area to the north, progressed to AFTER the western forest**. This establishes intended content order, not a final exact path, coordinate or hard gate. | User suggested **lizardmen or kobolds** with some **dragon connection** as possible enemies. Species, settlement, culture and dragon allegiance remain OPEN. Distinguish this NEW later northern area from the **already established Briarback northern approach**. |
| SURR-005 | **Northwest dragon lair** | The **Ancient Road Dragon** is fought in an **INSTANCED hidden lair to the NORTHWEST of Arborium**. | Lair entrance, instance occupancy/party rules, difficulty, repeatability rewards, map footprint, and whether northwest approach traverses west forest or northern area remain open. |
| SURR-006 | Dragon freely fightable; quest credit conditional | The dragon encounter **must be enterable/fightable even without possessing the main-story dragon-kill quest**. However, **killing the dragon awards main quest completion credit only when the player has the relevant quest active at the time of the kill**. A pre-quest kill does not retroactively fulfill a later quest. | Define party credit, kill-state transition, quest activation timing, return trips, loot and respawn/repeatability in a later round. Crucially **do not lock lair entrance behind the story quest**, while still checking the quest state when awarding story credit. |
| SURR-007 | Level ranges | Several surrounding zones should span approximately **levels 25–50**. The west-forest progression should support levelling **after Briarback**, not just overlap its lowest levels. | Specific zone level brackets and enemy scaling remain open; do not set all zones to level 25 or all to level 50. |
| SURR-008 | Exploration focus | Surrounding areas primarily provide **side quests, exploration, rare monsters and hidden treasures**, in addition to space for later story investigations. | Named rare monsters, drops, dungeon entrances, quest chains and the distribution of mandatory vs optional content are OPEN. |

### Conceptual geography, not a coordinate map

```text
                NEW NORTHERN AREA (after western forest)
                  [lizardfolk/kobolds? OPEN]
                         |
          NW: HIDDEN DRAGON LAIR INSTANCE
                  [entry independent of quest]
                         |
       WEST: ANCIENT FOREST / POST-BRIARBACK LEVELING
                         |
             ARBORIUM CAPITAL   ----   EAST: TRIMERY
                                         [nature mobs? OPEN]
```

**Important continuity:** The drawing is schematic and does **not** relocate the existing Briarback entry from the north, delete the established Trimery → Briarback → Arborium detour, or imply a straight east-west drivable road is already restored. The direct Trimery–Arborium highway's Roadwarden story already exists. The new north region is defined relative to progression from the **western forest**.

### Design implementation constraints for future Codex work

- **Dragon boss outcome stays canonical:** the ORIGINAL DiceBound Board 1 Ancient Road Dragon is slain in World 1's final Arborium quest; World 2 investigates why it appeared on the Dice's outside. Avoid inventing the answer or an obligatory dragon portal.
- **Instance is accessible out of sequence:** distinguish **encounter accessibility** from **main quest kill credit**. The player's quest must be active *on the qualifying kill*. Maintain a valid way to complete that quest even if player previously defeated the instance boss without it.
- **No retroactive credit:** a completed earlier optional dragon fight isn't sufficient after the quest becomes active.
- Existing guild forestry commissions remain optional and distinct from the required dragon investigation chain.
- **This is documentation only.** No Unity scripts, Psion, assets, scenes or builds changed by this interview.

### Next design round (OPEN)

1. Define western forest identity/name and climate/visuals.
2. Set western forest level bands and enemy families.
3. Choose what lives in the northern dragon-linked region (lizardmen, kobolds, both, or another faction).
4. Decide whether Nature creatures belong between Trimery and Arborium or as a distinct eastern subzone without rewriting existing geography.
5. Outline the lair's instance access/repeatability and baseline party rules, respecting free access and quest-only main-story credit.

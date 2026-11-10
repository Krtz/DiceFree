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


## Arborium Surroundings — Round 2 (2026-11-10, LATEST USER CONFIRMATIONS)

This round refines the already-approved west-forest / north-region / northwest-dragon geography (SURR-001–008); no Unity implementation was requested.

| ID | Question / feature | Decision | Still open |
| --- | --- | --- | --- |
| SURR-009 | Western forest character | **A lush MAGICAL forest**, rich in wildlife and unusual plants. Preserve a living, vibrant enchanted woodland identity, rather than defaulting to an exclusively dark/dead forest or a rocky barren biome. | Name, landmarks, specific colors, scenery, subzones and ambient events. |
| SURR-010 | Western forest leveling | **LEVELS 30–40**. Intended as a follow-up leveling/exploration region beyond Briarback. The larger Arborium surroundings can still encompass other levels up to 50 under SURR-007. | Per-creature level distribution, density, XP, rare spawns and transition to northern zone. |
| SURR-011 | Northern region inhabitants | **Lizardmen with a TRIBAL civilization** occupy the new region north of the west forest. Their prior proposed connection to dragons is a candidate to explore, not yet a locked worship/servitude/religion relationship. | Region name, levels, alignment, settlements, chiefs, combat roles, whether/why they associate with dragons. Kobolds were offered but **not selected** as the region's defining population. |
| SURR-012 | Nature enemies placement | **Living plants and mushroom creatures belong IN THE WESTERN FOREST** west of Arborium. The previous east-of-city/Trimery nature-monster idea was exploratory and is superseded as the chosen main placement. | Specific species, hostility, behaviors, levels, drops and frequency; no approval to remove every such species from other future biomes. |
| SURR-013 | Dragon lair dungeon lifecycle | The **northwest Ancient Road Dragon lair is fundamentally ANOTHER NORMAL DUNGEON**, with **freely available entry**, **repeatable boss fights** and **main-story kill credit ONLY if the dragon-kill quest is already active when the kill occurs**. Players may kill it without that quest; such earlier kills never count retroactively, and a later eligible kill must remain possible. | Details of normal dungeon staging timer, party/occupancy model, loot, difficulty, encounter mechanics, repeat rewards, lockouts, instance boundaries and respawn/reset require further design or reuse of existing agreed dungeon rules. Do **not** special-case the lair as a one-off non-replayable cinematic or impose a quest entry gate. |

### Current geographic progression snapshot

- **East of Arborium:** Trimery (existing direct-road/Roadwarden continuity and Briarback detour preserved). No new eastern nature biome is approved.
- **West of Arborium:** lush magical forest, **level 30–40**, with wildlife, animated/living plants, and mushroom enemies. Optional exploration, side quests, rare monsters and treasures remain core goals.
- **Further north AFTER western forest:** a separate **tribal lizardman region**. Dragon connection to be developed; exact geography and level bracket open.
- **Northwest of Arborium:** repeatable, freely accessible **Ancient Road Dragon dungeon**. Main quest requires a **fresh qualifying kill while the quest is active**. Its lair's precise entrance and path through neighboring areas are undecided.
- The original DiceBound Ancient Road Dragon dies in the **canonical World 1 main-story completion**. Repeatable instanced encounters do not imply that the dragon survives in the canon after the credited kill; use existing game dungeon/replay conventions without rewriting lore.

### Guardrails for Codex

Do not confuse **encounter availability** with **quest-state credit**, or interpret repeatable gameplay as a second canon dragon. Do not invent a second forest east of Arborium merely to host plant/mushroom enemies. Avoid committing the lizardmen to dragon worship until approved. The World Tree remains publicly reachable and primarily scenic; dragon's origin and motive stay mysteries. **Design only: no gameplay, scenes, Psion or asset changes in this round.**

### Round 3 design prompts (OPEN)

1. Name and distinctive landmarks for the western magical forest.
2. Choose western-forest living plant and mushroom enemy identities, behavior and relative frequency.
3. Choose conventional wildlife, rare monster(s), and gameplay hazards for level 30–40 forest encounters.
4. Determine the tribal lizardmen's actual connection to dragons and how friendly/hostile they are.
5. Decide how the player physically accesses the northwest dragon dungeon, keeping entry freely available and repeatable.


## Arborium Surroundings — Round 3 (2026-11-10, LATEST USER CONFIRMATIONS)

The following supersedes the outstanding questions from Round 2. This is design-only preparation for Codex, not an instruction to implement immediately.

| ID | Subject | Confirmed decision | Still open |
| --- | --- | --- | --- |
| SURR-014 | **Western forest name** | The lush magical level **30–40** forest WEST of Arborium is officially called **The Verdant Wilds**. Use this name in future region documents, quests and map labels. | Exact boundaries, paths, settlements and internal subregions. |
| SURR-015 | **Plant/mushroom behavior** | Living plants and mushrooms have **different behaviors by species**, including **some neutral/nonhostile creatures**. Do NOT make every mushroom and plant automatically hostile or share one simple attack behavior. | Species roster, aggro rules per species, attacks, movement, drops, stats. |
| SURR-016 | **Wildlife balance** | The Verdant Wilds is **predominantly inhabited by plants and mushroom creatures**, with **relatively few other animal/magical creatures**. Existing woodland wildlife may appear sparingly but should not dominate the population. | Specific secondary species and population ratio. |
| SURR-017 | **Northern lizardmen allegiance** | The **tribal lizardmen in the region NORTH of the Verdant Wilds serve the original Ancient Road Dragon DIRECTLY**. They are part of its aligned factions, not independent neutral dragon worshippers or dragon hunters. | Reasons for allegiance, ranks, dragon communication, tribal settlement, encounter design, post-dragon consequences. The broader dragon's ultimate motive remains mysterious. |
| SURR-018 | **Dragon lair approach** | The Ancient Road Dragon's repeatable instanced dungeon is reached via a **MOUNTAIN PATH NORTHWEST OF ARBORIUM**. Retain free access without a main-quest prerequisite and give dragon-kill story credit only for an eligible kill while the story quest is active. | Mountain/path geometry, hazards, path connection to Verdant Wilds and lizardman region, entry/party mechanics, difficulty and rewards. |

### Updated regional design picture

- **Arborium:** benevolent Verdant Order capital; the massive World Tree is openly reachable, primarily a landmark.
- **East:** Trimery, respecting the existing broken/restored-road and Ogre Roadwarden storyline, not a replacement eastern nature-monster zone.
- **West:** **The Verdant Wilds** (level 30–40), luminous magical forest with a mostly botanical and fungal monster ecology, including neutral species.
- **Further north beyond the western forest:** tribal **lizardmen directly serving the Ancient Road Dragon**; exact level range and society not yet designed.
- **Northwest:** mountain path to **Ancient Road Dragon's Lair**, a normal repeatable dungeon (available even without the story quest); World 1's canonical final quest requires a qualifying dragon kill. Existing lizardman allegiance does not require making the lair entrance quest-locked.
- The original DiceBound dragon's origin, recognition of the Echo, reasons for gathering enemies, and the World 2 clue remain long-term mysteries. Do not invent explanations in Codex implementation.

### Next design focus (OPEN, not yet approved)

1. Name 3–5 signature plant and mushroom species and their individual aggression/behavior.
2. Define whether neutral plants/fungi can react defensively or remain strictly passive.
3. Choose special mechanics for the botanical creatures (spores, roots, healing, camouflage etc.).
4. Pick the Verdant Wilds' distinguishing landmark/side activity and a rare monster.
5. Set any basic rules for forest routes and access to the north without changing the dragon mountain approach.

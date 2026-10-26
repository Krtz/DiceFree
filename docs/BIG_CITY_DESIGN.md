# DiceFree — World 1 Big City — Design Interview

**Status:** Design interview initiated 2026-10-26; Round 1 questions pending user answers. **No city name, architecture, scale, factions, functions or layout have been approved yet.**
**Repository branch:** `integration/world1-foundation`
**Scope:** Design only. No Unity/map/AI/quest implementation authorized.

## Established continuity (previously approved; not new decisions)

- This is the **major city beyond the World 1 Quillboar region**, reached from **Trimery by traveling through Quillboar territory**. The existing traversal is Cornberg → Ironpine → Trimery → Quillboar region → major city. Do not move the city behind a new mandatory Chieftain kill, dungeon or artificial progression gate.
- The **main Trimery travel quest** begins with a guard or road patrol captain in Trimery and **finishes by speaking to an Adventurers' Guild representative inside the big city**. The Guild rep's identity, building and exact location have yet to be designed.
- The optional **level 40 Quillboar Chieftain** is not mandatory for reaching this city.
- The separate future Quillboar Shaman ritual dungeon unlocks from the Trimery scholar's four-element quest after hand-in and is entered via a hidden cave in the Quillboar cliffs. It does not define the city.
- Earlier project design deferred the **Tier II Psion appearance and advancement mentor/quest** until major-city planning; this is a **candidate city design requirement to address explicitly**, not an approved NPC layout or quest implementation.
- Preserve existing **World 1 Great Tree** visual/geographical continuity; the huge tree is an established macro-landmark of this face but no city placement against it is decided yet.
- Treat `docs/WORLD_1.md` and `docs/QUILLBOAR_REGION_DESIGN.md` as continuity references, noting later Quillboar design rounds supersede their older draft statements.

## Design approach

Ask **five questions per round**, capture answers with explicit **USER CONFIRMED / PROVISIONAL / OPEN** labels, and update this GitHub document after **every answered round**. The first round will establish the **city name, overall identity, scale, geographical setting and initial functional role** without treating proposed answer options as approved decisions.

## Round 1 — Questions pending

1. What is the city's name?
2. What is its main architectural/cultural identity?
3. How big and important is it relative to Trimery and World 1?
4. What is the city's immediate geographical setting beyond the Quillboar region?
5. What should be its principal gameplay role for arriving adventurers?

**Important:** All five are OPEN until answered. No designs or assets have been created.


## Round 1 — 2026-10-26 (LATEST; USER CONFIRMED)

The user provided the geographical answer under **Extra thoughts** despite leaving the location multiple-choice field blank. Treat the freeform location as authoritative. The name is a **working direction**, not a formally finalized spelling.

| ID | City feature | Decision | Status |
| --- | --- | --- | --- |
| CITY-001 | **Working city name** | **Arborurbs** (or a similar nicer-sounding name), based on the Latin terms *arbor* = tree and *urbs* = city. It is a fantasy compound/name inspiration, **not claimed to be a conventional grammatical Latin phrase**. User wants to explore its sound/spelling before locking it. | USER-PREFERRED WORKING NAME; FINAL NAME OPEN |
| CITY-002 | **Architectural identity** | **Grand fantasy capital** with **towering spires, monumental buildings and magic**. Do not reduce it to a small rustic town or merely a military outpost. | USER CONFIRMED |
| CITY-003 | **World 1 significance** | **Largest and most important city on World 1**. Serves as the face's principal metropolitan hub/capital in scale and impact; exact government/political designation not yet selected. | USER CONFIRMED |
| CITY-004 | **Geography relative to World Tree** | The city is built **around the enormous World Tree near the middle of World 1**. The tree is central to the city's identity and is the established World 1 macro-landmark, not background scenery in a distant region. | USER CONFIRMED |
| CITY-005 | **Trimery → Quillboar → city travel topology** | From **Trimery travel NORTH into the Quillboar biome**, then **WEST while within the Quillboar area**, then **SOUTH from the Quillboar area into the city** surrounding the World Tree. Preserve these exact directional turns in macro world map and road layout, rather than describing the city as simply north of Trimery or placing it beyond a north-only straight road. | USER CONFIRMED |
| CITY-006 | **Broken direct road** | There **is/was a road directly connecting Trimery to the big city, but it is currently BROKEN/IMPASSABLE**. Initial travel therefore follows the northern → western → southern detour via Quillboar territory; **the direct road should exist and be connectable/restored later**. Do not delete the road, portray it as never built, or silently make it traversable at initial region progression. | USER CONFIRMED |
| CITY-007 | **City gameplay function** | **Full-service city** uniting adventurer hub, class mentors/trainers and quests, guilds, crafting, banking/trading/merchants, economy and story/lore activities. Existing plans to address **Psion Tier II appearance/mentor/advancement** during major-city design remain in scope, with specifics TBD. | USER CONFIRMED |

### Geography schematic (conceptual directions, not exact coordinates)

```text
                      Quillboar northern region
     [western Quillboar area] <--- [north of Trimery]
              |                       ^
              | SOUTH                 | NORTH
              v                       |
    [Grand city around World Tree]  [Trimery]
              ^                       |
              +---- BROKEN ROAD ------+
              (later possible restoration)
```

Schematic is only a mnemonic for directional travel; map locations and detailed road geometry remain to be authored consistent with the existing World 1 scene. **The broken road is direct; the detour goes north, then west, then south.** Access to the city does not require fighting the optional level-40 Quillboar Chieftain.

### Still open for Round 2+

- Final name pronunciation/spelling and whether to prefer `Arborurbs`, `Arboris`, `Arborum`, `Arborurbis` or another invented Latin-inspired name.
- City size/district structure and how the gigantic World Tree physically integrates with city blocks, walls and streets.
- What exactly broke the direct Trimery–city road; how/when it is eventually repaired; whether the blocked road is a visible early landmark.
- City government, architecture/materials, magical infrastructure, guild hub, Adventurers' Guild story quest rep, and deferred Psion advancement NPC.
- Large-scale city entrance position from the northern Quillboar detour and internal district network.

**Next design round:** finalize or narrow city name; decide tree integration, district composition, broken-road cause/role and city approach architecture. **DESIGN/DOCUMENTATION ONLY; no Unity changes authorized.**


## Round 2 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Topic | Approved direction | Status |
| --- | --- | --- | --- |
| CITY-008 | **Final city name** | **Arborium** — preferred fantasy capital name, superseding earlier working option **Arborurbs**. Use Arborium as the city's name in future World 1 design. | USER CONFIRMED |
| CITY-009 | **World Tree urban layout** | The city is built in **concentric rings encircling the gigantic central World Tree**. Organize radial access and urban districts with this overall ring-based form. This does **not** by itself approve elevated branch structures, root-top architecture, number of rings or district-to-ring allocations. | USER CONFIRMED |
| CITY-010 | **Distinct city districts** | Include **clearly separated districts for merchants, artisans, mages, nobles, and adventurers** within the grand fantasy capital. Distinct district identity is approved; exact radial ring assignments, count of wards and relative sizes are OPEN. | USER CONFIRMED |
| CITY-011 | **Broken direct Trimery road — respect earlier work** | The user explicitly said this has **ALREADY been decided** and declined to reselect its cause/appearance. **Preserve the established broken/direct Trimery–Arborium road and the alternate route north into Quillboar → west → south into Arborium. Do NOT select a new cause such as a destroyed bridge, collapsed mountain pass, Quillboar barricade, or aqueduct here.** Review any previous scene/project evidence for its more specific existing physical design before authoring or modifying that road. | PRE-EXISTING DECISION PRESERVED; NO NEW CHOICE MADE IN THIS ROUND |
| CITY-012 | **Arrival/entry gate** | Players arrive from the Quillboar region via an **enormous, fortified NORTHERN city gate**. This is on the route coming south from the western Quillboar sector. Preserve the city's grand fantasy architectural scale. | USER CONFIRMED |

### Round 2 consolidated design and geographic continuity

**Arborium**, the largest and most important city on World 1, is a monumental magical capital built in **concentric rings around the World Tree**, featuring separate **merchant, artisan, mage, noble, and adventurer districts**. Players entering from Quillboar territory come through a **huge fortified northern gate**. Existing route remains **Trimery → north into Quillboar → west through Quillboar → south into Arborium**. The earlier **direct Trimery–Arborium road is broken**, and the user has already discussed this; do not re-propose or overwrite its cause and structure without reviewing prior authoritative decisions.

**Review note:** Current readable `docs/WORLD_1.md` references an early blocked road related to Cornberg/Trimery starter progression and forest/slimes, but that should **not automatically be conflated with the distinct direct Trimery–Arborium road**. Current `docs/BIG_CITY_DESIGN.md` has only the broken-road connection and future-restoration fact, not its precise failure mechanism. An implementation handoff should inspect earlier scene and design decisions rather than invent the bridge/pass reason or repeatedly quiz the user about it.

### Open for Round 3

- Which urban ring contains which districts and how gates/avenues connect them.
- Is the World Tree itself accessible and what building or institution occupies the innermost ring?
- What does the Adventurers' Guild building look like, given the established main-quest turn-in to a guild representative?
- City governance, local culture and how magic appears in daily life.
- Whether the city's wider wall layout includes separate district gates or one primary exterior wall.

**DESIGN/DOCUMENTATION ONLY; no Unity implementation or existing road modification authorized.**


## Round 3 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Feature | Approved direction | Still open |
| --- | --- | --- | --- |
| CITY-013 | **World Tree center and inner civic space** | The World Tree's innermost surroundings are **a mixture of sacred gardens, grand plazas and important buildings**. Preserve a monumental spiritual/civic atmosphere, rather than making the entire center exclusively a temple, palace, or purely wild park. | Exact buildings and which parts are public or access-restricted; shrines/fountains not yet individually confirmed. |
| CITY-014 | **Concentric district ordering** | **OUTER rings:** adventurer and merchant districts. **MIDDLE rings:** artisan and mage districts. **INNER rings nearest World Tree:** noble district. The tree itself and its sacred civic core remain the central focus. This ordering supersedes other proposed district arrangements. | District boundaries, number of physical walls/rings, cross-ring traffic access and residences for non-nobles. |
| CITY-015 | **Capital governance** | Arborium is governed by **an ancient magical order whose purpose includes protecting the World Tree**. Do not introduce a king, elected mayor or guild council as the primary ruler in contradiction. | Order name, membership, leadership structure, ideology, public legitimacy, extent of magical versus civic authority, and possible internal politics. |
| CITY-016 | **Adventurers' Guild architecture and location** | A **huge fortified guildhall near Arborium's enormous northern city gate**, within/along the outer adventurer district. The Adventurers' Guild representative who completes the Trimery→Arborium main quest can be associated with this guild, but the exact representative location is still to be placed. | Guildhall size/shape, training grounds, quest board, interior, NPC. |
| CITY-017 | **Everyday magical infrastructure** | Magic is **mostly practical**, e.g. city lamps, transport and utilities, rather than making all districts uniformly full of spectacular floating structures. Magical infrastructure contributes to Arborium's advanced fantasy-capital character. | Specific transport system, power source, services, and which parts are accessible to players. |
| CITY-018 | **Mage district whimsical magic** | The mage quarters include **additional scattered, unusual/random magical sights and phenomena** beyond the city's ordinary practical enchantments. They add local personality and can be atmospheric rather than automatic hazards, special quests or mechanics. | Exact whimsical details, visual effects, interactables and potential future quests. |

### Round 3 approved macro layout

```text
     ENORMOUS FORTIFIED NORTHERN GATE
                 |
      Huge fortified Adventurers' Guild
                 |
      OUTER RING — Adventurers / Merchants
                 |
      MIDDLE RING — Artisans / Mages
           (mage-quarter oddities)
                 |
      INNER RING — Nobles
                 |
      SACRED WORLD TREE CORE
      Gardens + plazas + important buildings
```

This diagram is conceptual, not the finalized street map: actual districts occupy space around the **concentric rings**, with connecting avenues/radial streets to be designed. The city remains **Arborium**, greatest metropolis on World 1, entered via the northern gate after Trimery → north into Quillboar → west → south. Preserve the preexisting broken direct Trimery–Arborium road; its exact structural cause is not being redesigned.

**Next design round:** determine how the ancient magical order is organized/seen by citizens, how open the World Tree center is, major radial avenues/transit, the Adventurers' Guild's player services, and distinctive magical activity in the mage quarter. No city/Unity implementation authorized by this interview.

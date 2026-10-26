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

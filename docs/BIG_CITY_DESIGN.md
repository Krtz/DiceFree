# DiceFree — World 1 Big City — Design Interview

**Current status — 2026-11-05:** The user requested that implementation begin. The first arrival section is implemented locally on `feature/arborium-arrival-and-balance`, following the confirmed decisions in Rounds 1–3 below. Historical interview-only restrictions describe those earlier rounds; they do not revoke this later authorization.
**Scope:** Northern approach, monumental gate, fortified Adventurers' Guild, initial services, ring streets and a first travel-report quest. This is the beginning of Arborium, not a finished metropolis. See `ARBORIUM_ARRIVAL_AND_BALANCE.md` for implementation evidence and remaining work.

**2026-11-06 tentative world-hub direction:** See `ARBORIUM_WORLD_CONNECTIONS.md` for the user's ideas about Arborium serving later worlds, a guild/forestry quest chain opening a southern World 2 passage, and an optional boss investigation restoring the direct Trimery shortcut. These are future proposals; geography, cause, boss and quest details remain open.

**2026-11-07 continuity audit and guild commissions:** `TRIMERY_DESIGN_V03.md` and `TRIMERY_IMPLEMENTATION_V1.md` already establish that the **Ogre Roadwarden deliberately destroyed the western highway under orders**, remains there, and is approachable from Arborium's side only. The issuer and motive remain unknown; historical open-cause notes below must not override that later confirmation. `ARBORIUM_GUILD_FORESTRY.md` implements the first guild timber/clearing/survey chain while preserving both blocked passages and the later Roadwarden encounter.

**Approved visual follow-up:** The arrival district and World Tree are further developed on `art/arborium-arrival-world-tree`. See `ARBORIUM_ARRIVAL_POLISH.md` for the original tree artwork, street dressing, ambient citizens/convoy, audio and gameplay-camera checks. The confirmed district order and later story/mentor decisions remain unchanged.

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

## Later implementation authorization — 2026-11-05

Subsequent direct user requests authorized the initial city and arrival-district polish, followed by removing the green scenery intersecting the city, adding west/east gates, and making multi-storey houses accessible upstairs. This supersedes the earlier interview-only restriction for those bounded features. The six established main buildings now have three or four walkable levels each. Gate aprons connect to existing city avenues; the broken direct Trimery road is preserved. See `ARBORIUM_ARRIVAL_AND_BALANCE.md`, `ARBORIUM_ARRIVAL_POLISH.md`, and `ARBORIUM_GATES_AND_FLOORS.md` for implementation and validation boundaries. Governance names, additional district institutions, story and further regional road connections remain open.

The later request to complete agenda items 3 and 4 also authorizes furnishing these existing floors and giving them practical uses. The current pass adds room directories, a guild practice target and an indoor counter for the existing bank service. See `WOLF_VISUALS_AND_ARBORIUM_ROOMS.md`. It does not settle the open governance, magical-order, class-mentor or road-design questions.


## Round 4 — 2026-11-10 (LATEST; USER CONFIRMED)

The user's freeform note is authoritative: **the World Tree itself does not need quests or gameplay functions**, and **the magical order (not necessarily the Tree) will eventually send the adventurer toward the next world**. Do not transform a proposed World Tree quest chain/buff system into approved scope.

| ID | Topic | Approved direction | Open / guardrails |
| --- | --- | --- | --- |
| CITY-019 | **Magical order's ethics and mission** | The ancient order governing Arborium are **benevolent guardians**, actively protecting **both the World Tree and Arborium's ordinary citizens**. Do not characterize the core institution as sinister, oppressive or secretly villainous without later story approval. | Order name, leaders, ranks, visible attire, guard responsibilities and internal politics remain open. |
| CITY-020 | **Access to the World Tree** | **Anyone can walk directly up to the enormous World Tree trunk**. The landmark and its gardens/plazas are openly accessible; there is no required noble status, mage membership, reputation, or story-unlock gate just to approach the trunk. | Specific nearby buildings may have their own access rules; do not invent barriers around the trunk. |
| CITY-021 | **Public reputation of magical order** | Arborium's citizens **generally love and respect** the magical order as trusted protectors. Do not make widespread fear, resentment or class-based hostility the default city atmosphere. | Individual characters may differ in later quests, but widespread political resentment is not the baseline. |
| CITY-022 | **World Tree's gameplay role** | The World Tree is **primarily an extraordinary visual landmark and important story setting**. It **does not need to be a quest hub, repeatable activity, mandatory blessing station, buff mechanic, or central questline driver**. Story importance is compatible with remaining mostly scenic. | Any special interaction, exploration or quest at the Tree requires a later distinct approval. |
| CITY-023 | **World 1/Arborium principal conflict** | The city's biggest ongoing concern is **increasing monsters and instability outside the city**. The order's protective role and adventurers' activities should have a credible reason to respond to external danger. | Exact source, factions, escalation, events and quests remain open; don't assume the World Tree is sick or causing the instability. |
| CITY-024 | **Order-led journey to the next world** | **At a later point the magical order should be the institution that sends/directs the adventurer to the next world**. This belongs in the story/progression arc, not as an automatic Tree power or direct Tree interaction. | Timing, quest prerequisites, destination, method of crossing, cutscene, player freedoms and rewards are OPEN. Preserve earlier tentative Adventurers' Guild forestry/southern-passage world-travel ideas as *potential supporting work*, not as finalized contradictions. |

### Updated city story pillars

- **Arborium:** magnificent concentric-ring capital encircling the World Tree; public sacred gardens and plazas, merchant/adventurer outer districts, artisan/mage middle districts, noble inner districts; openly accessible Tree trunk.
- **Rulers:** an **ancient benevolent magical order**, beloved and respected by the population; they defend the citizenry as well as the Tree.
- **Atmosphere:** a safe, lively, functional metropolis using mostly practical magic, with more eccentric phenomena in mage quarters; wider regions increasingly unsafe.
- **Campaign direction:** the adventurer arrives via Briarback and the northern gate, meets the Adventurers' Guild representative, tackles threats and later receives **the order's mission to travel to another world**. The World Tree is not obligated to power the journey or dispense quests.
- **Implementation boundary:** this round **records design only**. It does not request Unity edits, Psion class work, scene creation, or a full new-world questline at this time.

### Round 5 topics (OPEN, not yet decisions)

1. Name the order (or decide later).
2. Determine how it is led.
3. Decide where its headquarters sit in Arborium.
4. Decide how it responds to worsening monster activity outside the walls.
5. Establish the high-level trigger for sending adventurers to the next world, without prematurely deciding transport mechanics or undermining existing world-crossing infrastructure.


## Round 5 — 2026-11-10 (LATEST; USER CONFIRMED)

| ID | Topic | Approved direction | Open / guardrails |
| --- | --- | --- | --- |
| CITY-025 | **Magical order name** | The ancient, benevolent order protecting Arborium and the publicly accessible World Tree is formally named **The Verdant Order**. Use that name in future city, NPC and campaign design. | Emblem, regalia, public rituals, order history and individual member names undecided. |
| CITY-026 | **Order leadership** | The Verdant Order is led/governed by **a council of SIX powerful mages**. Do not install a king, solitary archmage, immortal ruler or mayor as its primary authority without later user approval. | Whether councilors hold distinct responsibilities, vote democratically or have rotating leadership remains undecided. Don't conflate council of six with the six cube-world faces without later approval. |
| CITY-027 | **Order headquarters** | The Verdant Order's headquarters is **a GRAND MAGICAL ACADEMY in Arborium's MAGE DISTRICT** (the middle ring). This is its civic/institutional headquarters and should feel important in the capital. | Academy name separate from order name, interiors, layout, students, player services and unrestricted/restricted areas undecided. Does not replace the fortified Adventurers' Guild near northern gate. |
| CITY-028 | **External threat response** | As monster activity and instability increase OUTSIDE Arborium, the order **sends Adventurers' Guild members on investigations and expeditions** rather than primarily deploying its own battle-mage army or leading every excursion personally. | Missions, reporting system, potential magical support and guild/order chain of command not yet set. City guards and ordinary defenses can still exist; the choice establishes the preferred ACTIVE RESPONSE. |
| CITY-029 | **World 2 expedition eligibility** | The Verdant Order eventually sends **the player to World 2 BECAUSE the player has proved themselves through Arborium's main questline**. This establishes a meaningful quest/progression arc before the order entrusts the player with the expedition. | Exact mission count, tasks, reason to select World 2, travel mechanism, scene transition, prerequisites and story beats not yet approved. In-game cube/face transition architecture remains the technical reference and should not be overwritten by assumed teleportation. |

### Approved organizational relationship and story arc

- **The Verdant Order** is a benevolent and broadly beloved government/guardian institution, with **six powerful mage councilors** and a **grand academy headquarters in the middle-ring mage district**.
- The **Adventurers' Guild** retains its own huge fortified hall at the northern gate and its ordinary quests/commissions. When regional dangers intensify, the Verdant Order commissions/instructs **guild adventurers** to investigate, rather than collapsing the two organizations into one.
- Arborium's **main questline** is the player's route to earning the six councilors' trust. **After proving their capability**, the Verdant Order sends them onward to **World 2**. Neither the World Tree itself nor a direct Tree interaction is required to function as a quest giver or transport device.
- **The World Tree stays publicly approachable**, primarily a visual landmark and story setting. No World Tree buffs or compulsory tree-centered quest chain introduced.
- **Implementation boundary:** Record in GitHub for Codex to use later. No Unity code, scene or class changes authorized, and Psion is intentionally outside this design scope.

### Round 6 open questions

1. Council's six seats: distinct disciplines/responsibilities or collective generalist guardians?
2. How should adventurers first earn an audience with the Verdant Order?
3. How many connected main-story missions should establish the player's competence?
4. What category of increasing external monster danger provides the main quest's investigative focus?
5. Whether academy access should be open to the public, visitors restricted to certain wings, or progression-gated.

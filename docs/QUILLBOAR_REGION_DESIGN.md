# DiceFree — World 1 Quillboar Region — Design Interview

**Status:** ROUND 1 — USER DECISIONS CONFIRMED (2026-10-26). Initial geography, enemy faction, level band and boss direction only. Region layout, encounters, world-boss details, dungeon and quests not yet approved. **DESIGN/DOCUMENTATION ONLY — no Unity terrain, gameplay, scenes or assets changed.**

## Placement and established context

- Quillboar biome is reached **north of Trimery** according to the existing World 1 traversal plan. Do not silently relocate it or rewrite established Cornberg/Ironpine/Trimery geography.
- This is a **later World 1 region targeting approximately levels 25–35**, potentially overlapping the level-30 Tier II advancement transition. Exact stat tuning and encounter composition are still open.
- Design priority: Quillboar region first, then the major city. Deferred Psion appearance/advancement quest can be addressed with major-city planning later.

## Round 1 — user-approved decisions, 2026-10-26

| ID | Topic | Approved direction |
| --- | --- | --- |
| QUILL-001 | Geography and visuals | A varied biome using **mostly cleared/open areas** and Quillboar settlements with **some cliffs and hills**, plus forested areas. Dense **trees/forest act as biome boundaries** rather than making the interior an unbroken dense forest. Clearings and settlements are core. |
| QUILL-002 | Quillboar society | **Tribal society** with **chiefs, shamans and warriors**; organized enough for camps/settlements and a chieftain-led threat. Do not portray them merely as instinct-driven beasts. |
| QUILL-003 | Hostility | **Every Quillboar is hostile to players** in the current region design; no friendly/neutral Quillboar faction or trust system is approved. |
| QUILL-004 | Intended level band | **Levels 25–35**, challenging later World 1 play. Balance is not yet tested or implemented. |
| QUILL-005 | Principal regional threat | **A powerful Quillboar chieftain uniting the tribes**. Exact title, identity, arena, boss mechanics, encounter timing and rewards are OPEN. |
| QUILL-006 | Possible dungeon hook | **A Quillboar shaman performing a ritual could be the subject of a future dungeon**. This is a USER-SUGGESTED DIRECTION, **not a finalized or authorized dungeon design**. Keep distinct from the chieftain's regional threat until more is decided. |

## Geography and gameplay guardrails

1. Favor readable open space for encounters and sightlines in the central area, with terrain variation through hills, cliffs, outcrops and settlements. The forest/trees supply the most prominent exterior biome boundaries and make transitions from neighboring world biomes legible.
2. Keep settlements and structures recognizably tribal. Chiefs, shamans and warriors exist as faction roles; exact enemy archetypes, abilities and population counts will be decided in later rounds.
3. No presumption that forest walls are fully impassable without authoring decisions about roads, bypasses, navigation and world edge rules.
4. The chieftain uniting tribes is the **overworld/regional story direction**; the shaman ritual is a **candidate future dungeon**. Do not conflate their mechanics.
5. All Quillboars are hostile, but this alone does not authorize specific aggro radii, leash rules, AI, loot, reputation mechanics, scripted raids or dungeon implementation.

## Open questions for Round 2 and beyond

- Is the region largely a connected open overworld with distinct outposts, or a series of passages through cliffs and camps?
- What is the chieftain's role: roaming world elite, stationary overworld boss, end-of-zone encounter, or dungeon boss?
- How do warrior, shaman, scout/ranged and other roles function mechanically?
- What roads, shortcuts and safe/rest areas exist; how does the north road from Trimery enter the biome?
- How does the shaman ritual candidate relate to faction lore and the later optional dungeon?

**Next step:** continue five-question design rounds; update this document after each user-approved round. Do not implement Unity work unless separately instructed.


## Round 2 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Design choice | Confirmed specification |
| --- | --- | --- |
| QUILL-007 | **Overworld layout** | A **mixture of broad/open terrain, hostile Quillboar camps and narrow passages**. The environment retains cleared ground, hills and cliffs, while forest/tree clusters form the outer biome boundaries. No single central-camp-only layout; exact topology remains open. |
| QUILL-008 | **Chieftain encounter placement** | The **powerful tribe-uniting chieftain is a stationary boss within the largest fortified Quillboar settlement**. This is a regional boss, not a roaming world elite or dungeon boss. Boss abilities, arena, access and respawn remain open. |
| QUILL-009 | **Ordinary enemy roster** | Exactly four **initial ordinary enemy archetypes** selected by the user: **Quillboar Warrior**, **Quillboar Spear Thrower**, **Quillboar Shaman**, and **Quillboar Brute**. The user's extra thoughts explicitly specify these, superseding the questionnaire option to select them individually later. Their skills, stats, encounter roles and exact counts remain open. |
| QUILL-010 | **Population density** | **Moderately sized/scattered hostile enemy groups throughout the overworld**, with camps/settlements as denser natural concentrations if later designed. Do not infer exact numbers, aggro radii or spawn/respawn schedules. |
| QUILL-011 | **Traversal roads** | The main road **north from Trimery branches into several routes through the biome**. The branches' destinations, safety, navigation obstacles, patrol risk and exact geometry remain open. Do not silently choose a wholly safe, permanently blocked, or fully enemy-controlled road. |

### Current confirmed regional sketch

- **Geographic origin:** north of Trimery, World 1.
- **Target level range:** 25–35.
- **Visuals:** primarily cleared/open spaces, scattered camps and settlement clusters; hills, cliffs and narrower passages; dense trees/forest along biome boundaries.
- **Enemy factions:** all Quillboars hostile, tribal society including chiefs/shamans/warriors. Four regular archetypes: Warrior / Spear Thrower / Shaman / Brute.
- **Major threat:** stationary chieftain within the largest fortified Quillboar settlement.
- **Navigation:** branching road network from the Trimery approach, enabling multiple pathways.
- **Enemy distribution:** moderate grouping across the area rather than uniform heavy saturation.
- **Possible dungeon (NOT confirmed as production work):** shaman conducting a ritual, distinct from outdoor chieftain.

**Next round:** ask about each of the four enemy archetypes' combat mechanics and groups, then exact path/camp and chieftain implementation later. Design/doc only; no Unity updates, assets, map editing, quests, bosses or dungeon implementation authorized.

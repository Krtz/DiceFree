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


## Round 3 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Enemy or system | Approved behavior |
| --- | --- | --- |
| QUILL-012 | **Quillboar Warrior** | **Defensive melee fighter with shield and blocking**. It forms the front line and may defend vulnerable allies. Exact block chance, damage reduction, facing, cooldowns and reaction rules remain OPEN. Do not assume an offensive charge or bleed ability. |
| QUILL-013 | **Quillboar Spear Thrower** | **Throws spears from range and switches to melee when approached.** Define actual ranged/melee ranges, timing, weapon handling, projectile collision and retreat/positioning in later rounds. Do not silently assign a snare or pierce mechanic. |
| QUILL-014 | **Quillboar Shaman** | **Offensive elemental spellcaster**, not a healer/buffer/totem specialist by default. Its specific elements, damage spells, spell ranges, cast times, targeting and interruption behavior remain OPEN. |
| QUILL-015 | **Quillboar Brute** | **Slow, high-HP enemy with devastating basic attacks.** Its threat comes from big normal autoattack hits rather than an automatically approved ground slam, charge or enrage ability. Exact movement speed vs common baseline, attack speed, raw damage, HP and telegraphs remain OPEN; enemy-specific speed can differ from *playable class* common baseline rule. |
| QUILL-016 | **Mixed-group tactics** | **Warriors protect Shamans and Spear Throwers**. Plan their group positioning, threat behavior and protective movement accordingly; the user did not yet choose coordinated alarm calls, chain-aggro or specific formation mechanics. Brute role in mixed groups is still OPEN. |

**Encounter design direction (interpretive, not additional confirmed skills):** Warrior fronts protect vulnerable elemental Shamans and ranged Spear Throwers; thrown-spear specialists switch to melee under pressure; slower Brutes reward awareness of heavy basic attacks. All four are hostile and found in moderate groups across cleared plains, camps, branches and cliff corridors. Continue to use ordinary universal combat and projectile/shield systems unless a special mechanic is explicitly selected.

**Next design round priorities:** choose Warrior block behavior, Spear Thrower melee-switch threshold, Shaman's elemental identity, Brute attack cadence/telegraph, and exact protective Warrior group AI. No Unity code or map work authorized.


## Round 4 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Topic | User-confirmed decision |
| --- | --- | --- |
| QUILL-017 | **Warrior defensive shield** | The Warrior **periodically raises its shield** for a **short defensive window**, reducing incoming damage while raised. It is **not** currently specified as a passive random block roll or direction-only always-on block. Exact raised duration, cooldown, reduction percentage, channel/element coverage and actions while raised remain OPEN. |
| QUILL-018 | **Spear Thrower close-range behavior** | Primarily throws ranged spears and **switches to melee ONLY when physically attacked in melee**, rather than automatically when a player is within a fixed 3m/5m range. Operational trigger (landed melee hit versus melee attack attempt), duration of melee stance, return-to-range logic and melee attacks remain OPEN. Do not replace this decision with proximity-only switching. |
| QUILL-019 | **Shaman elemental affinity** | **Offensive spellcaster using a COMBINATION OF MULTIPLE existing elements**. Exact selected elements, spell kit, ranges, cooldowns, effects and casts remain OPEN. Do not silently select Fire/Lightning/Earth as confirmed choices or make Shamans heal by default. |
| QUILL-020 | **Brute attack telegraph** | **Slow, hard-hitting BASIC/normal autoattacks with a visible wind-up animation**. Telegraph should make the heavy attack readable; exact attack timing, hit reach, damage and interruption mechanics remain OPEN. This does not approve a separate slam/knockback/enrage skill. |
| QUILL-021 | **Warrior protection AI** | Warriors protect Shamans and Spear Throwers using a **combination of physical positioning and target prioritization**. They should seek to interpose and prioritize threats to vulnerable ranged allies. Exact reevaluation frequency, formation distance, leash/aggro override and body-blocking behaviors remain OPEN. No automatic taunt ability is approved. |

### Round 4 interpretation

An ordinary Quillboar encounter should present clear tactical roles: the shield-bearing Warrior takes the front line and raises a protective shield periodically; ranged Spear Throwers continue throwing until **actually attacked in melee**; multi-element offensive Shamans threaten from the back; slow Brutes deliver highly telegraphed heavy basic attacks. Warriors should try to interpose between players and nearby ranged allies and respond to threats toward them. These are **design decisions only**, not implemented enemy AI.

### Outstanding Round 5 questions

1. Warrior: shield uptime, damage-reduction value, and whether it can attack while bracing.
2. Spear Thrower: what exactly triggers the melee switch and when it returns to throwing.
3. Shaman: which specific elements and spell patterns.
4. Brute: wind-up duration and possibility of repositioning/dodging without breaking the basic-attack system.
5. Warrior: how much priority to grant ally defense versus ordinary threat targeting, and what happens when no nearby ally needs protection.

No Unity terrain, prefabs, code, scenes or encounter implementation authorized.


## Round 5 — 2026-10-26 (LATEST; USER CONFIRMED, INCLUDING FOLLOW-UP CLARIFICATION)

| ID | Topic | Decision | Status |
| --- | --- | --- | --- |
| QUILL-022 | Warrior's active shielding | Warrior periodically raises a shield for **70% incoming damage reduction lasting 2 seconds**, on a **15-second cooldown**. Exact cooldown start, whether this applies to all incoming damage types, and whether the Warrior can attack while shield is raised remain to specify; do not substitute passive chance-to-block. | USER CONFIRMED |
| QUILL-023 | Spear Thrower returns to range | It switches to melee **only when physically attacked in melee** (Round 4), then **returns to ranged spear throwing as soon as the melee attacker moves away**. Use practical contact/reach checks later; do not impose a 3s/5s recovery delay. | USER CONFIRMED |
| QUILL-024 | Shaman variants and elements | **Four distinct elemental Quillboar Shaman enemy variants**: **Fire Shaman**, **Earth Shaman**, **Lightning Shaman**, **Ice Shaman**. This directly resolves the user's Round 5 'Custom selection' clarification. Each shaman variant is primarily an offensive elemental spellcaster. **These are four variants of the already-established Shaman archetype, not four additional primary archetypes or a single Shaman cycling through four elements.** Exact spell kits, cast times, damage channels, cooldowns, NPC counts and elemental effects remain to design. | USER CONFIRMED in follow-up |
| QUILL-025 | Brute wind-up | The Brute's devastating slow basic attacks have a **clearly visible 1.2-second wind-up animation**. Exact attack cadence, hit check timing and damage are not yet specified. | USER CONFIRMED |
| QUILL-026 | Warrior protect-ally AI | Warrior **prioritizes enemies threatening nearby Shamans and Spear Throwers using protective positioning and target priority**, but **respects its leash and existing combat engagement** rather than endlessly chasing attackers or forcibly overriding every active encounter. Exact threat weighting, leash range and protection radius remain open. | USER CONFIRMED |

### Approved Quillboar opponent structure after Round 5

- Four principal ordinary enemy archetypes: **Warrior, Spear Thrower, Shaman, Brute**.
- Within the offensive Shaman archetype, **Fire / Earth / Lightning / Ice** are separate identifiable variants with independently designed spell kits to follow.
- Warriors form an intelligent defensive front with periodic high-mitigation shield and protective AI.
- Spear Throwers throw at range and engage in melee in response to *actual melee attack*; resume throwing when aggressor moves away.
- Brutes threaten with telegraphed 1.2-second heavy normal/basic attacks.
- Encounters remain primarily moderate enemy groups across the cleared open grounds, fortified settlements, hill/cliff passages and branched roads north of Trimery; chieftain remains stationary in the largest fortified settlement.

**Next design round:** define first-pass offensive spells for the four Shaman variants and their distinguishing combat mechanics, preferably five focused questions. No Unity implementation, enemy prefabs, assets, terrain or quest changes authorized.

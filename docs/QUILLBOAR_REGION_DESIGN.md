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


## Round 6 — 2026-10-26 (LATEST; USER CONFIRMED)

All four Quillboar Shaman variants are **ordinary hostile ranged spellcasting enemies**; each has **normal Magical basic attacks plus exactly TWO special spells**. One main special spell is now confirmed for each variant, while its second spell remains OPEN. Do not treat the second spell suggestions or numeric values as approved. Do not conflate the four elemental variants with one Shaman who rotates elements.

| ID | Variant | Approved first special ability | Status |
| --- | --- | --- | --- |
| QUILL-027 | **Fire Shaman** | **Fireball** — single-target fire projectile applying **Burning** status. Projectile follows normal combat rules unless later explicitly modified. Burning duration, tick damage, stacking/refresh, cast time, cooldown and range are OPEN. | USER CONFIRMED |
| QUILL-028 | **Earth Shaman** | **Stone Prison** — temporarily **roots one player** in place. Exact root duration, duration reductions through CC resistance/Tenacity, projectile/ground-target mechanics, damage (if any), break condition and cooldown are OPEN. | USER CONFIRMED |
| QUILL-029 | **Lightning Shaman** | **Chain Lightning** — magical Lightning spell that **jumps between nearby players**. Exact number of jumps, radius between targets, repeat hits, falloff, cooldown and damage coefficients are OPEN. | USER CONFIRMED |
| QUILL-030 | **Ice Shaman** | **Frost Nova** — a **close-range area attack that applies a movement slow**. Exact radius, slow amount and duration, damage/cast timing and cooldown are OPEN. | USER CONFIRMED |
| QUILL-031 | **Shared ordinary Shaman kit** | Each of the **four** Shaman types gets **two special spells PLUS normal magical basic attacks**. There is no third special spell by default. The four *second* specials are not yet chosen. | USER CONFIRMED |

### Current Shaman kit inventory

| Variant | Basic attack | Special #1 | Special #2 |
| --- | --- | --- | --- |
| Fire | Normal Magical basic | Fireball + Burning | **OPEN** |
| Earth | Normal Magical basic | Stone Prison root | **OPEN** |
| Lightning | Normal Magical basic | Chain Lightning jumps | **OPEN** |
| Ice | Normal Magical basic | Frost Nova AoE slow | **OPEN** |

**Later decisions:** individual second spells, exact damage channels/elements where not obvious, numerical values, CC behavior under universal rules, spell priority and reaction to interrupts. Keep normal enemy AI/aggro systems and previous Warrior/Thrower/Brute mechanics. **No Unity/gameplay implementation authorized.**


## Round 7 — 2026-10-26 (LATEST; USER CONFIRMED)

Each of the four Shaman variants retains **normal Magical basic attacks plus exactly two special spells**. This round finalizes each variant's **second special spell** and its tactical decision-making direction. The user's freeform details about Fire Shield and Boulder Toss supersede any overly generic reading of the questionnaire options.

| ID | Shaman / AI | Approved second special or decision | Status |
| --- | --- | --- | --- |
| QUILL-032 | **Fire Shaman — Fire Shield** | Castable on the Shaman **or nearby allied Quillboars**, not merely self-only. While active, it **damages MELEE attackers only** when they attack the shielded unit; ranged attacks/spells do not trigger it by default. Exact trigger-on-attempt versus on-landed-hit, damage value/type/element, duration, cast range, cooldown, multiple-shield stacking, and reactions to melee skills remain OPEN. It is a retaliatory buff, not general damage reflection against all incoming attacks. | USER CONFIRMED |
| QUILL-033 | **Earth Shaman — Boulder Toss** | **Long, visible casting/wind-up**, then **rolls a big boulder toward the targeted enemy** along the ground. The boulder must be **dodgeable by player movement**. Do not implement as a homing projectile that automatically tracks its target all the way to impact. Wind-up seconds, rolling trajectory targeting snapshot, boulder width/speed/lifetime, environmental collisions, AoE or single-target contact, and dodge telegraph format remain OPEN. | USER CONFIRMED |
| QUILL-034 | **Lightning Shaman — Thunderbolt** | **Single-target Lightning strike that briefly stuns**. Exact stun duration, Tenacity/CC reductions, cast tell, range, damage and cooldown remain OPEN. | USER CONFIRMED |
| QUILL-035 | **Ice Shaman — Frozen Ground** | Creates a **ground area that slows players** standing within it. Exact placement, AoE shape/radius, duration, slow magnitude, entry/exit behavior, whether damage is involved and cooldown remain OPEN. | USER CONFIRMED |
| QUILL-036 | **Tactical spell AI** | Shamans **choose specials tactically according to player positions and battlefield conditions**, instead of a strict fixed rotation or unconditional cast-on-cooldown. Conditions and priority/cooldown policy remain to design. | USER CONFIRMED |

### Four finalized ordinary Shaman kits (ability identities)

| Variant | Basic attack | Special #1 (Round 6) | Special #2 (Round 7) |
| --- | --- | --- | --- |
| **Fire** | Normal Magical | **Fireball**: single-target projectile + Burning | **Fire Shield**: self/ally buff retaliating against melee attackers only |
| **Earth** | Normal Magical | **Stone Prison**: temporarily roots player | **Boulder Toss**: big, telegraphed rolling, dodgeable boulder aimed toward target |
| **Lightning** | Normal Magical | **Chain Lightning**: jumps between nearby players | **Thunderbolt**: single-target Lightning strike + brief stun |
| **Ice** | Normal Magical | **Frost Nova**: close-range AoE slow | **Frozen Ground**: persistent slowing ground zone |

**Balance and combat framework:** retain global damage/element/projectile/collision/shield/CC rules. Do not invent precise timings or coefficients before user approval or delegated playtest tuning. Fire Shield may be especially tactically relevant on frontline Warriors and Brutes, but no automatic priority or grant to those archetypes is approved merely by that observation. Enemy spell kits are design-only; Unity behavior has not been implemented.

**Next design round:** Five focused questions can determine Fire Shield trigger and target priorities; Boulder Toss wind-up/path; Thunderbolt stun; Frozen Ground duration/slow; tactical Shaman spell selection or combos.


## Round 8 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Mechanic | Confirmed behavior | Unresolved details |
| --- | --- | --- | --- |
| QUILL-037 | **Fire Shield retaliation eligibility** | Fire Shield damages enemies **only in response to melee BASIC/AUTOATTACKS** made against its protected self or ally. **Melee abilities/skills do NOT proc Fire Shield**, and ranged basic attacks and spells do not proc it. | Whether a missed/blocked melee basic attempt triggers retaliation, nominal damage and duration. |
| QUILL-038 | **Earth Shaman Boulder Toss path** | After its previously confirmed **long, clearly visible wind-up**, the large boulder **rolls in a straight line, continuing through enemies rather than stopping at the first enemy it hits**. The path is dodgeable, non-homing, and may strike multiple separate enemies along its course. | Snapshot targeting/direction, hit-once-per-target handling, travel distance, collisions with terrain, boulder speed, width, base damage and cooldown. |
| QUILL-039 | **Lightning Shaman Thunderbolt stun** | Thunderbolt deals its single-target Lightning attack with a **0.5-second stun** as its normal baseline; follow global CC/Tenacity and immunity behavior. | Damage, cast timing, cooldown and range. |
| QUILL-040 | **Ice Shaman Frozen Ground potency** | Creates a **3-second-lasting ground zone** imposing **50% movement-speed slow while a player is physically inside it**. The user selected the option with "while inside", so no 50% slow persisting after exit unless expressly authored later. | Radius, tick/damage behavior (none yet confirmed), cooldown and terrain target range. |
| QUILL-041 | **Fire Shaman defensive target preference** | Fire Shamans **prioritize frontline Quillboar Warriors and Brutes** when deciding which ally to buff with Fire Shield. Buff may still apply to self or other allies per QUILL-032, but defensive frontline priority is explicit. | Self-defense fallback and refresh/recast policies. |
| QUILL-042 | **Enemy defense archetype split** | **Warriors and Brutes: relatively HIGH Physical Defense, LOW Magical Defense. Shamans and Spear Throwers: relatively LOW Physical Defense, HIGH Magical Defense.** This is a comparative design direction for these specific Quillboar enemies, not a universal stat conversion rule. | Actual flat PDEF/MDEF values and scaling at levels 25–35, individual subtype variance. |

### Resulting intended combat tradeoffs

- **Warrior / Brute**: physical toughness encourages Magical attacks and positioning instead of relying only on basic Physical damage.
- **Spear Thrower / Elemental Shaman**: stronger against Magical damage, more vulnerable to Physical attacks if players can reach them past the Warriors.
- Fire Shield punishes only **melee BASIC attacks** into protected frontliners; melee special abilities can avoid the retaliation trigger even when they deal melee-range damage.
- Earth Shamans create a telegraphed **line-path boulder hazard** passing through enemies, not an automatically hitting targeted projectile.
- Lightning Shamans apply a **brief 0.5s stun**, Ice Shamans create **3s areas imposing 50% slow on occupants**.

**Round 9 design priorities:** Fire Shield triggering on hit vs attempt, boulder projectile obstacle behavior and per-target collision, Frozen Ground area/radius and overlapping zones, frontline defensive baselines and/or threat/positioning specifics. Numeric stat/ability values not specified remain OPEN; do not implement Unity content under this documentation-only interview.


## Round 9 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Mechanic | Approved decision | Open / implementation caution |
| --- | --- | --- | --- |
| QUILL-043 | **Fire Shield trigger** | Retaliate **only after a melee BASIC attack successfully hits the protected Shaman/ally**, including if an eligible shield absorbs **100%** of the hit. A miss does not trigger. Melee abilities/skills, ranged basics and spells do not trigger. | Exact retaliatory damage, duration and cooldown OPEN. "Successfully hits" refers to the hit event, NOT actual HP loss. |
| QUILL-044 | **Boulder Toss terrain collision** | Straight-rolling Boulder Toss **stops and shatters upon colliding with solid terrain**. It still passes through enemies as already agreed; never treats valid units as terrain blockers. | Exact solid obstacle colliders, travel speed/size/range and whether shattering itself does damage remain OPEN (don't invent a shatter AoE). |
| QUILL-045 | **Frozen Ground radius** | Ice Shaman Frozen Ground has a **4-meter radius**. Its existing **3s duration** and **50% movement-speed slow while inside** remain unchanged. | Cast range, cooldown and damage behavior still OPEN. |
| QUILL-046 | **Overlapping Frozen Ground slow** | Follow the **game's shared/universal movement slow-stacking rules**, not an invented class-specific stack/add/merge exception. | Audit of current `docs/COMBAT.md` and `docs/STATS_AND_DAMAGE.md` did not identify an explicit universally finalized numeric slow-stacking formula. Leave exact stacking arithmetic OPEN until that shared rule is defined elsewhere; do not pretend a "strongest wins" or additive rule was chosen. |
| QUILL-047 | **Boulder Toss repeated collision** | A **single rolling boulder may damage each individual enemy at most ONCE**, even if an actor's collider is intersected for multiple frames, while the boulder can continue forward through and hit different enemies. | Use per-boulder hit-target tracking if implemented later; no special repeat-hits/ticks or shatter-hit packet authorized. |

### Round 9 consolidated details

- Fire Shield protects a Quillboar self/ally (prefer Warriors and Brutes) and triggers only on **landed melee normal basic** strikes, even with **fully shield-absorbed damage**.
- Earth Shaman Boulder Toss: long telegraphed wind-up, straight-line rolling **dodgeable** projectile, continues through units, **one contact damage packet per individual enemy**, stops/shatters on **solid terrain**.
- Ice Shaman Frozen Ground: **4m radius, 3s live zone, 50% movement slow on occupants**, slow overlap delegated to shared stacking rules, which remain to be specified at the global level.

**Next round possibilities:** Fire Shield duration/CD and retaliation amount, Boulder Toss wind-up/speed/width, Frozen Ground cast range/CD, Thunderbolt/Chain Lightning interaction, or open region-map/camp/chieftain geography. Design/documentation only; no Unity content changed.


## Round 10 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Feature | Confirmed specification | Open details |
| --- | --- | --- | --- |
| QUILL-048 | **Fire Shield duration and cooldown** | Fire Shaman's self/ally **Fire Shield lasts 5 seconds** and has a **15-second cooldown**. Preserve earlier priority for Warriors and Brutes; retaliation only on successfully landed melee **basic** attacks, including hits fully absorbed by shields. | Whether cooldown starts on cast or expiry, dispel/recast behavior and shield visual behavior follow normal ability framework unless specifically changed. |
| QUILL-049 | **Fire Shield retaliation amount** | Each qualifying landed melee basic attack triggers a **small FIXED amount of Fire-element retaliatory damage**, rather than damage proportional to the attacking unit's basic attack or Shaman rank/level as a bespoke formula. | Exact fixed numeric damage value and Physical/Magical channel of retaliatory packet remain to tune/confirm; do not invent a value or automatic level scaling. |
| QUILL-050 | **Earth Shaman Boulder Toss wind-up** | **1.5-second visible wind-up** before the Earth Shaman releases the already-confirmed large rolling straight-line boulder. Direction locks into a dodgeable trajectory; stops/shatters on solid terrain and continues through separate enemies, hitting each enemy at most once. | Trajectory aim snapshot exact timing, movement speed, max distance, projectile-versus-player collision, damage and cooldown remain OPEN. |
| QUILL-051 | **Earth Shaman Boulder Toss size** | The physical rolling boulder has a **2-meter diameter** (nominal 1m radius) and is a substantial visible hazard. Do not substitute a small generic rock or automatically change it to a 2m radius. | Collision/hit volume geometry and animation implementation will follow engine conventions once authorized. |
| QUILL-052 | **Ice Shaman Frozen Ground tactical placement** | AI should **predict target movement and favor grouped players** when choosing a ground location, rather than always targeting current position or fixed offsets. Existing ground zone lasts 3s, radius 4m, slows occupants by 50%, with shared slow-stacking rules still authoritative. | Prediction/targeting algorithm, cast range/cooldown, ability to place under cover, and prioritization weights remain OPEN. |

### Round 10 encounter implications

The **Fire Shaman** shields a nearby frontline Warrior/Brute (or another eligible ally) to punish repeated melee basic attacks for 5s; its 15s cooldown prevents permanent coverage without other mechanics. The **Earth Shaman** telegraphs its 2m-wide rolling hazard for 1.5s; players can dodge a straight-line boulder that passes through multiple units before stopping on a solid obstacle. The **Ice Shaman** actively positions Frozen Ground in predicted player paths and around clusters, creating 4m-radius temporary movement control.

**Next interview:** begin the powerful stationary **chieftain in the largest fortified settlement** and/or settlement encounter geography; final numerical tuning of minor Shaman attacks and boulder speeds can remain provisional until playtesting. This design round does not authorize Unity changes, dungeon construction or implementing bosses.


## Round 11 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Topic | Confirmed decision | Still open |
| --- | --- | --- | --- |
| QUILL-053 | **Chieftain fighting style** | A **massive two-handed axe wielder** specializing in **powerful sweeping attacks**. Visually imposing melee commander; no shield or dual-wielding style. | Exact attacks, cone/sweep arcs, ranges, attack cadence and numerical damage. |
| QUILL-054 | **Chieftain bodyguards** | The encounter **begins with exactly two elite Quillboar bodyguards** protecting the chieftain. | Their archetypes, special mechanics, positioning, threat AI, whether they respawn, and boss-bodyguard synergy. |
| QUILL-055 | **Largest settlement / stronghold** | The largest fortified Quillboar settlement combines **wooden fortifications with natural cliffs**. Chieftain is stationed inside it as previously confirmed. | Palisade/gate arrangement, paths, vantage points, stronghold extent and exact arena geometry. |
| QUILL-056 | **Boss access / exploration** | **Players may reach and engage the chieftain at any time, provided they fight through the settlement.** Do not gate him behind regional quests, camp leader kills, a special event, a locked gate, or mandatory progression flags. | Aggro/leash, how much of the settlement pulls with him, and re-entry/reset behavior. |
| QUILL-057 | **Boss encounter identity** | Boss is defined by **massive, visibly telegraphed attacks that reward movement and dodging**. Attack readability and fair reaction windows are central. | Specific tells, timings, hit areas, multi-phase behavior (NOT approved yet), avoidance rewards and damage. |

### Consolidated chieftain encounter concept

A physically imposing **stationary outdoor regional boss** in the largest enemy-held stronghold, amid **wooden defensive structures and rugged cliffs**, with **two elite bodyguards present at engagement**. The chieftain fights with an oversized **two-handed axe**, using **sweeping attacks with clear warnings**. The stronghold is an accessible overworld location: adventurous players can challenge him without unlocking a quest, but must confront the hostile Quillboar defenders on the way.

**Boundary of approval:** no extra reinforcements, battlecry buffs, phase changes, forced arena lockdown, environmental hazard mechanics, mandatory quest gates, loot rules, respawn numbers or bodyguard specializations have been approved in this round. These remain for subsequent design discussions. Preserve the separate future **Shaman ritual dungeon idea** as provisional; the chieftain is not its dungeon boss.

**Next round:** specify axe attacks and attack warnings, elite bodyguard roles, chieftain/boss leash, and whether attacks can threaten several players. Design/documentation only; no Unity changes.


## Round 12 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Chieftain mechanic | Confirmed specification | Open detail |
| --- | --- | --- | --- |
| QUILL-058 | **Signature attack: Ground-Splitting Overhead Smash** | The chieftain's signature is a **narrow but devastating overhead two-handed axe smash** that splits/strikes the ground. It is a positional dodge check, not the wide frontal/270° sweep as signature. Prior broader approval for an axe fighter with powerful sweeping attacks may still inform ordinary/secondary moves, but the **signature overhead smash takes precedence**. | Hit shape/width, length, damage, wind-up, cooldown, whether ground split lingers. |
| QUILL-059 | **Boss attack telegraphs** | Combine **clearly readable weapon wind-up animations AND RED ground danger indicators**. Both are important for the chieftain's dodge-focused encounter. | Exact indicator shapes and appearance duration for each move. |
| QUILL-060 | **Exactly two elite bodyguards** | Boss begins with **one elite Quillboar Warrior and one elite Quillboar Brute**. Retain the archetypes' existing roles (Warrior shield/protection, Brute slow high-HP heavy basic attacks), with elite stat tuning/extra mechanics still OPEN. | Positioning, pull linkage, behavior on boss reset/death, elite modifiers. |
| QUILL-061 | **Single-phase encounter** | Boss uses **one consistent moveset throughout the fight**. Do not automatically introduce HP-threshold phase transitions, damage/haste enrage or new moves at 50% HP. | Specific abilities and attack cadence within that one moveset. |
| QUILL-062 | **World-boss leash and reset** | Retreat/evade follows the **existing standard world-boss leash and reset rules**, with no invented chieftain-only full-settlement chase, arena boundary instant reset or one-off special mechanic. | Exact established global leash values and reset behavior should be taken from shared systems rather than guessed here. |

### Chieftain state after Round 12

A massive two-handed axe chieftain in a cliff-backed wooden stronghold, freely accessible to players who fight through the hostile settlement. The combat begins with **one elite Warrior and one elite Brute bodyguard**. The chieftain's primary identity is a **heavily damaging overhead ground-splitting smash** with an obvious wind-up and red ground telegraph; he can have additional sweeping moves consistent with earlier concepts, but they have **not** been selected as signature attacks. There are **no scripted phase changes**. Retreat and reset follow ordinary world-boss rules, not a special locked arena.

**Next round:** specify smash geometry/timing, other axe attacks, bodyguard engagement/reset linkage and boss rewards/respawn later. Continue user interview in roughly five-question rounds, updating this file after each. DESIGN ONLY; no Unity scene/AI/ability implementation authorized.


## Round 13 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Chieftain mechanic | Approved choice | Still to resolve |
| --- | --- | --- | --- |
| QUILL-063 | **Overhead Smash hit region** | The signature Ground-Splitting Overhead Smash creates a **long, 6-meter straight-line ground fracture** extending along its aimed direction. Preserve its narrow, dodgeable identity and the previously approved red ground indicator and weapon animation. **6m is the length**, not radius or width. | Exact fracture width, whether targeted/locked at cast start, collision and whether any ground effect persists after impact. |
| QUILL-064 | **Overhead Smash warning** | **1.5-second clearly telegraphed wind-up** before the heavy strike, communicated via weapon animation **AND** red ground attack indicator. | Animation/indicator sync, exact damage and recovery interval. |
| QUILL-065 | **Second special — Axe Throw** | The Chieftain's second special attack is **Axe Throw**, a **ranged attack used against distant players**. Do not turn this into a battle stomp/cleave or claim exact projectile behavior, damage or cooldown without later design. | Throw range, trajectory, dodgeability, recovery of axe, telegraph and target choice. |
| QUILL-066 | **Elite bodyguard pull separation** | The **elite Warrior and elite Brute guard the Chieftain but can be pulled separately**. Players may choose to isolate them before challenging the Chieftain. This *overrides any assumption of mandatory linked three-enemy aggro when any member is attacked*. | How bodyguards react when Chieftain itself is engaged, whether re-pull/add during fight, and whether separately killed bodyguards respawn during a live boss attempt. |
| QUILL-067 | **Overhead Smash on-hit penalty** | On successful hit, the 6m ground fracture deals **massive damage plus a short stun**. Apply existing shared CC/Tenacity and immunity rules; exact stun duration, damage and whether partially shielded hits stun remain OPEN. | Stun seconds, damage formula and defenses. |

### Consolidated boss gameplay after Round 13

The stationary Chieftain, located in the largest cliff-and-wood fortified settlement, fights with a **massive two-handed axe**. He has a **single consistent combat phase** and is accessible to anyone fighting through the settlement. His primary signature is a **1.5s telegraphed, narrow 6m linear overhead ground fracture that inflicts massive damage plus a short stun**; his other confirmed special is **ranged Axe Throw** to threaten distant players. The **two elite guards consist of one Warrior and one Brute**, stationed close enough to protect the boss but **independently pullable** for tactical preparation. Boss retreat uses the normal world-boss leash/reset system. Attack specifics, bodyguard linkage upon directly pulling boss, loot, respawn, and numeric tuning are not yet approved.

**Next round priorities:** choose Axe Throw targeting and travel, fracture width, stun duration, bodyguard responses when directly engaging the Chieftain, and telegraph/cooldown timing. Keep the single-phase rule, guard pull separation, and standard shared combat/CC systems. DESIGN/DOCUMENTATION ONLY: no Unity implementation authorized.


## Round 14 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Chieftain mechanic | Confirmed behavior | Open details |
| --- | --- | --- | --- |
| QUILL-068 | **Overhead Smash fracture width** | The straight-line ground fracture is **1 meter wide** and remains **6 meters long**. This is a narrow rectangular/linear positional dodge challenge, not a 1m radius. | Exact targeting lock, collision/telegraph alignment, impact/recovery time. |
| QUILL-069 | **Overhead Smash stun duration** | A successful hit deals the previously confirmed massive damage **plus 1 second of stun**, reduced or prevented by ordinary shared Tenacity/CC-immunity mechanics. Retain the 1.5-second telegraphed wind-up, red ground indicator, and weapon animation. | Actual raw damage values, stun application when damage absorbed by shields, ability cooldown. |
| QUILL-070 | **Axe Throw projectile behavior** | The Chieftain throws a **homing axe that follows its selected target**, not a locked straight-line dodgeable axe. Follow the normal targeted-projectile framework for collision/impact unless explicitly overridden later. | Projectile speed, turn rate/homing persistence, solid obstacles, ranged reach, impact effects, cooldown, whether returning axe animation matters. |
| QUILL-071 | **Bodyguard assist on Chieftain pull** | If players engage the **Chieftain directly**, **both living elite bodyguards immediately join the fight**, even though the bodyguards **can themselves be pulled separately beforehand** (QUILL-066). Guards already killed do not magically reappear just for boss pull. | Whether separated/killed guards respawn during a live boss engagement, guard reset and exact combat linkage details. |
| QUILL-072 | **Axe Throw target selection** | Choose the **highest-threat player who is OUTSIDE melee range** as the homing axe target. Do not make it random, prioritize the farthest player, or automatically target whoever has highest threat regardless of melee distance. | Actual melee/ranged eligibility threshold; behavior if no players are outside melee range; impact damage and cooldown. |

### Consolidated Chieftain mechanics after Round 14

- Stationary, freely accessible **outdoor regional chieftain** in the largest wooden-and-cliff Quillboar stronghold; massive two-handed axe and **one consistent phase**.
- **Overhead Ground Fracture Smash:** warning **1.5s**, animation plus red ground indicator, straight **6m length × 1m width**, high damage and **1s stun** on hit (normal CC rules).
- **Axe Throw:** homing axe aimed at the **highest-threat player outside melee range**. Homing is intentional; projectile should follow ordinary shared targeted-spell/projectile collision rules unless later specified.
- **Two guards:** one elite **Warrior** and one elite **Brute**. Each can be pulled separately for preparation; **attacking the chieftain causes BOTH currently living guards to assist immediately**. No phase changes or additional reinforcements automatically introduced.
- Leash/reset follows shared world-boss rules.

**Next focused design questions:** What if no eligible ranged Axe Throw target exists? How does the homing axe interact with obstacles? What are Smash cooldown, Axe Throw cooldown and attack cadence? Should a shield-absorbed Smash hit still stun? How should the guard fight/reset stay coherent with independent pulling? Keep all numerical balance subject to playtest. **DESIGN ONLY; no Unity implementation.**


## Round 15 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Chieftain mechanic | Confirmed decision | Implementation guardrail |
| --- | --- | --- | --- |
| QUILL-073 | **Axe Throw eligibility / no ranged targets** | **Do not use Axe Throw if no player is outside melee range**. The Chieftain continues normal melee combat instead of selecting a melee player, switching to lowest-HP targeting or repositioning artificially. | Use the previously approved highest-threat target *among eligible players outside melee range* when such a player exists. Exact melee-range threshold remains shared-system/encounter tuning. |
| QUILL-074 | **Axe Throw terrain collision** | The **homing axe follows the normal homing-projectile collision rules**, including terrain/obstacle handling. | Do not invent a Chieftain-only terrain-penetrating axe, navigation-around-walls or custom collision bypass. Shared homing behavior is authoritative. |
| QUILL-075 | **Overhead Smash cooldown** | **12 seconds** between uses per ordinary cooldown rules. Signature remains 1.5s telegraph, red danger indicator, 6m length × 1m width fracture, massive damage and 1s stun when applicable. | Raw damage, aiming/lock timing, shared cooldown start and impact collision details are subject to normal systems/playtesting. |
| QUILL-076 | **Axe Throw cooldown** | **15 seconds**. Maintain homing ranged attack on the highest-threat eligible player outside melee range only. | Damage, travel speed, range and wind-up remain OPEN. |
| QUILL-077 | **Overhead Smash stun on shield-absorbed hit** | **Follow normal shared damage-and-crowd-control rules**, including shield absorption, hit resolution, Tenacity and immunity. No ability-specific requirement for actual HP damage and no special guaranteed-through-shield CC rule has been approved. | If the shared rules have an unspecified shield-vs-CC case, leave it unresolved at the shared-system level rather than asserting one outcome. |

### Approved Chieftain combat snapshot through Round 15

- The boss is a **massive two-handed axe** Quillboar in the largest cliff-and-wood fortified overworld settlement, accessible without quest gates.
- **Overhead Smash**: 12s cooldown; 1.5s visible axe wind-up plus red telegraph; narrow **6m × 1m** ground fracture; massive damage and **1s stun** governed by shared damage/CC systems.
- **Axe Throw**: 15s cooldown; **homing projectile**, shared projectile collisions, selects **highest-threat player outside melee range**; if none exists, skip Axe Throw and continue melee.
- **Guards**: one elite Warrior and one elite Brute; they can be pulled separately, but both *living* guards join immediately if the Chieftain is directly engaged.
- Single-phase fight, normal world-boss leash/reset. No boss-only HP thresholds, forced reinforcement waves or custom immunity logic.

**Next design phase:** stronghold route layout, camp structure, boss encounter space, guards' respawn linkage, standard overworld encounter tuning and rewards. Design/documentation only; no Unity changes from this interview.


## Round 16 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | World / encounter feature | Confirmed decision | Still open |
| --- | --- | --- | --- |
| QUILL-078 | **Chieftain stronghold structure** | Largest fortified settlement is a **multi-level fortress built against natural cliffs**, with the Chieftain stationed at the **highest level**. Earlier wooden fortifications + natural cliff identity remains authoritative. | Exact number of terraces/levels, ramps, gates, approach routes, and fort footprint. |
| QUILL-079 | **Regional camps / settlements** | Prefer **numerous tiny Quillboar camps and fewer large settlements**, rather than a fixed count of 2–4 midsized camps. Camps should be spread through the existing open landscape / hills and maintain the tribal settlement feel. | Total camp counts, specific locations, encounter density per camp, any extra larger settlement beyond the chieftain's stronghold. |
| QUILL-080 | **Road network** | The roads from Trimery's northward approach form a **circular road linking the camps**, plus a **separate path leading to the Chieftain's stronghold**. Preserve branching travel choice and viable loops without forcing one linear camp-clear order. | Exact circuit geometry, crossroads, side trails, path elevation, alternative links. |
| QUILL-081 | **Chieftain arena** | The boss fight takes place in a **cliff-backed clearing surrounded by wooden palisades** at the **highest level** of the fortress. It is an outdoor clearing, not an enclosed hall or automatically a raised stone platform. | Arena dimensions, cover, entrance location and whether any obstacles affect the homing axe; do not invent a forced arena lock. |
| QUILL-082 | **Two elite bodyguards: resetting and deaths** | On Chieftain reset, **both bodyguards reset with him IF they are alive**. **Dead bodyguards remain dead** until their **normal respawn timers/rules**; resetting the boss never immediately resurrects them. Preserve independent pre-pulling and both living guards joining when the Chieftain itself is engaged. | Bodyguard normal respawn interval, precise spawn/patrol anchors, guard/boss leash interactions. |

### Stronghold and region navigation — latest approved outline

Players approach north from **Trimery** into the level 25–35 Quillboar region, where an interconnected **circular road** links numerous **small camps** and fewer larger settlements in mostly open terrain with hills, some cliffs and occasional narrow passages. A **separate access path** branches toward the chief's imposing **wooden-and-cliff multi-level fortress**. At the **top**, the chieftain and his two elite guards inhabit an outdoor **clearing backed by cliffs and edged with wooden palisades**. Players may reach the boss by fighting through the settlement at any time: there is **no required camp clearance, quest gate, or forced boss unlock**.

Guard handling specifically: guards are **individually pullable** prior to attacking the chief; engaging the Chieftain pulls **both currently living guards**. When Chieftain resets, **living guards reset** as well, while guards previously killed remain dead until their own **ordinary respawn**. Do not silently resurrect killed guards or invent bodyguard waves.

**Next design round:** define micro-camp composition and structures, secondary settlements, exact ascent style/crossroads, castle fortification lines, boss arena scale and elite guard placement. DESIGN ONLY; no Unity/map changes authorized.


## Round 17 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Region feature | Approved decision | Still open |
| --- | --- | --- | --- |
| QUILL-083 | **Tiny Quillboar camps** | Numerous **mixed makeshift camps**, with **varied structures and enemy-group compositions** rather than a rigid template of tents/huts and fixed 2–4 or 3–6 units. Preserve the sense of different small tribal sites across the mostly open region. | Counts, exact combinations of props and enemies, density, loot/interactables. |
| QUILL-084 | **Larger Quillboar settlements** | The fewer **larger settlements are Shaman-focused ritual settlements featuring elemental decorations**. They should reinforce the already approved four distinct Shaman variants (Fire, Earth, Lightning, Ice). | Whether each settlement belongs to one element or mixes several, ritual props, any ritual mechanics, and specific settlement sizes. **Do not treat this approval as confirmation of a separate ritual dungeon**; that remains only a candidate concept. |
| QUILL-085 | **Cliff fortress ascent** | Reach the Chieftain's highest-level arena by **wooden ramps and bridges connecting cliff terraces**. The ramps/bridges are actual traversable paths through the multi-level wooden-and-natural-cliff stronghold. | Number of terraces, bridge widths, guard encounters, alternate routes and fall hazards. |
| QUILL-086 | **Regional patrols** | **Small Quillboar patrol groups walk between camps along the circular road**. They are mobile regional traffic instead of exclusively stationary guards or massive roaming warbands. | Group size/composition, path length, patrol timing, aggro radius, respawn, and whether they stop at camps. |
| QUILL-087 | **Chieftain arena size** | The high-elevation **cliff-backed, wooden-palisade-enclosed outdoor clearing is approximately 25 meters in diameter**, offering generous maneuvering/dodging space for the boss's 6m × 1m Overhead Smash and other attacks. | Exact terrain shape, entrances and precise authored dimensions; do not reinterpret 25m diameter as radius. |

### Region flow after Round 17

A circular road branching from north of Trimery links **numerous small, varied makeshift Quillboar camps** and a smaller number of **elemental Shaman ritual settlements**. **Small Quillboar patrols** travel along the road between sites. A separate route climbs the imposing Chieftain stronghold using **wooden ramps and bridges between cliff terraces**, terminating at a roughly **25m-diameter cliff-backed, palisade-surrounded clearing**. One elite Warrior and one elite Brute protect the Chieftain; individually pullable, both living ones join immediately if the boss is engaged; resetting boss returns surviving guards, not already dead ones.

**Next interview:** ritual settlement elemental specialization, camp patrol composition, patrol reactions/assist, ramp/bridge terrain hazards, and whether the arena's 25m clearing should have cover. No Unity scene/AI/asset implementation approved; keep this a design specification.

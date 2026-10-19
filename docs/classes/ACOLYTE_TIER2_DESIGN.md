# DiceFree — Acolyte Tier II — design interview

**Created:** 2026-10-19  
**Status:** WORLD 1 STANDARD TIER II CLASS. **ROUNDS 1–3 USER DECISIONS RECORDED 2026-10-19**; flat MDEF + attack-speed Curse reduced on bosses by Tenacity; two-second proximity-based spreading Radiation infection with no back-infection of earlier hosts within the same chain; four directly controllable skeletons max, each lasting 15s; Soul Charges max8/cost4 per summon, +1 successful drain tick and +1 death while afflicted with Acolyte DoT but zero on Curse/DoT application; five-second Void Drain Life's HP-damage-based healing fraction scales with skill rank, and shield absorption generates no healing. Numeric Curse values/Tenacity rule, spread radius, actual heal curve and advancement quest remain under design. **DESIGN/DOCUMENTATION ONLY; NO UNITY IMPLEMENTATION.**  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md), [Tier II Core Classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Class Requirements and Discovery](../CLASS_REQUIREMENTS_AND_DISCOVERY.md).

## User-approved World 1 roster and design order

There are EXACTLY EIGHT normal World 1 Tier II classes, four under each Tier I parent:

- **Physically Blessed Novice →** Greatblade, Defender, Ranger, Thief.
- **Magically Touched Novice →** Mage, Priest, **Acolyte**, Psion.

**Next design interview Acolyte → then Thief → then Psion**, then Quillboar area, then big city. **Tier III is for World 2; DO NOT design any Tier III class skills/advancements now.** The user may revisit some mentor locations after the city is designed, but has NOT yet approved moving existing mentors or quests.

## Earlier foundations to preserve

1. **APPROVED AS NORMAL CLASS IN ROSTER (2026-10-19):** Acolyte belongs to **Magically Touched Novice at Tier I level 30** and starts as a new **Tier II level-1 manifestation**, preserving the Tier I parent state, in line with all standard advancement rules. Acolyte's *name as final shipping title*, specific skill kit, body, quest and numeric combat rules are still open.
2. **CONFIRMED ACOLYTE-002, OVERRIDING earlier `docs/TIER2_CORE_CLASSES_DESIGN_V02.md` concept:** Acolyte uses **wand (mainhand) + grimoire (offhand)**, but **the grimoire is a normal stat-bearing equipment item for now**. There is **NO mandatory book-specific page, charge, inscription or special offhand class mechanic**. Like all other gear, future authored item Intrinsics/effects may modify class skills, subject to later gear design. This later user choice explicitly supersedes the older 'book MUST have its own mechanic' statement.
3. **CONFIRMED ACOLYTE-001/004:** Acolyte's fighting rhythm emphasizes **SLOW sustained damage + curses/debuffs + self-sustain** compared with burst/CC Mage; the initial four specific spell pillars are **a distinct curse, a distinct damage-over-time ability, a summon, and a channeled life drain**. Make it a hybrid solo-sustainable damage/debuffer rather than Mage clone or primary Priest-style healer. **ROUND 2 NOW CONFIRMS Radiation for DoT and Void for Drain Life, controllable timed skeletons, and Curse Magical Defense/attack-speed debuffs.** Exact coefficients, summon caps, skill names and final skill count remain open.
4. **CONFIRMED ACOLYTE-003:** Acolyte uses **Mana PLUS Soul Charges**. **Successful offensive spells** generate Soul Charges; **summons consume Soul Charges**. **ACOLYTE-009 NOW CONFIRMS MAX 8 SOUL CHARGES and COST 4 per skeleton summon; each successful Drain Life damage tick generates charges, and a unit dying WHILE affected by the caster's Radiation DoT triggers Soul Charge gain.** Other offensive spells' charge gain rates, exact charges per tick/death, persistence/decay and summon Mana cost remain open. Soul Charges are Acolyte's own resource, **not a grimoire equipment mechanism**. Avoid inventing automatic charge gain from ticks that would cause abusive scaling.
5. **GLOBAL ABILITY/RANK ECONOMY:** all new Tier II skills start **rank 0**, the Tier II character starts level1 with **1 spendable skill point** and earns one per level until the total configured rank capacity. Recent Greatblade/Mage/Priest/Defender/Ranger designs target **~78–80 total class-skill ranks by class level80**, and skill-specific rank-level eligibility should permit a point purchase each class level. Exact Acolyte skills/max ranks/gates will be designed AFTER its combat kit is selected, not preselected.
6. **CONFIRMED ACOLYTE-005 STORY DIRECTION, LOCATION/OBJECTIVES NOT YET FINAL:** An NPC **at a graveyard** asks an eligible Magically Touched Novice to **go to Cornberg and Trimery, collect the bodies/remains of deceased people, return them to the graveyard for a proper burial**, and then advance to Acolyte. This is a quest CONCEPT, **not an approved final level layout, NPC location, body spawn list, quest counter, collection mechanics or dialogue**. User explicitly wants to return to quest design later, perhaps when shaping the major city. Do not invent graveyard location or move other mentors now.

## Confirmed Round 1 identity and proposed future kit framework

### User-approved combat identity

| Topic | Approved decision | Still open |
| --- | --- | --- |
| Role | **Sustained occult damage, curse-based debuffs and self-sustain**, slower ramp than Mage burst with some crowd control | Actual Curse effects, solo DPS, group debuff power, defense, healing cap and boss interactions |
| Pillar 1 | **Curse applies FLAT reductions to enemy Magical Defense and attack speed. Bosses have effects REDUCED by their Tenacity value** (not unmodified full effect). | Exact absolute debuff magnitudes and attack-speed units, duration, rank curve, Tenacity coefficient/application order and stacking remain OPEN |
| Pillar 2 | **Weak Radiation DoT spreads by proximity:** an enemy must remain near an infected enemy for **2 continuous seconds** to become infected. DoT **cannot spread back to the same enemy** previously infected in that spread lineage; no oscillating ping-pong. On infected-enemy death, caster gets **+1 Soul Charge**. | Proximity radius, infection lineage/visited-target definition, duration, tick damage, whether reapplication resets contacts, independent re-casts OPEN |
| Pillar 3 | **Directly player-controllable skeletons**, **15s lifetime each**, **MAX FOUR simultaneous active skeletons** for this Acolyte, each cast consuming **4 Soul Charges**. | Idle autoattack/stance and command behavior, spawn when cap4 reached, Mana cost, HP/damage and target ownership OPEN |
| Pillar 4 | **Drain Life is 5-second channeled Magical Void**, up to **five 1s damage ticks**, **+1 Soul Charge PER successful damage tick**. HP restored is **% of damage ACTUALLY inflicted on enemy HP**, with the percentage **growing with Drain Life skill rank**; **NO healing from damage blocked by enemy shields**. Moving/stun/explicit interrupt cancels; incoming ordinary hits do not cancel. | Initial/capped heal %, growth per rank, coefficient of tick damage, cast range/CD/Mana/target loss OPEN |
| Resources | **Mana + Soul Charges CAP8**, cost **4 per skeleton summon**. **+1 for each successful Drain Life damage tick** and **+1 on death of enemy currently afflicted with caster's Radiation DoT**. **Applying Curse or Radiation DoT gives 0 charges**; their ordinary ticks are NOT automatic charge generators. | Whether other *future direct offensive spells* earn charges (must be explicitly authored), charge decay/persistence, simultaneous death attribution OPEN |
| Equipment | **Wand mainhand + grimoire offhand**, both ordinary authored gear/stat/intrinsic carriers | Authorable drops/Intrinsics, class weapon permissions, exact item stats; **no unique grimoire charges/pages/inscriptions system** |
| Advancement | **Graveyard caretaker** concept: collect dead from **Cornberg and Trimery** for proper burial at graveyard | Exact cemetery site, NPC, remains locations/counters, story presentation and trial scripts intentionally DEFERRED |
| Future rank economy | All new class skills **rank0**, one spendable skill point at class level1, aim ~78–80 purchasable total through level80 with individual rank gates | How many final skills, rank caps/unlock levels, damage formulas, costs and animations |

**Do not conflate Curse and DoT into one action by accident:** the user's first answer lists a curse AND a separate damage-over-time skill. The final kit may also need a direct offensive spell and/or passive to provide a purposeful Soul Charge building loop. These are NOT user-approved additional abilities yet.

### Summon framework compatibility (not yet summon-specific approval)

DiceFree's shared `docs/COMBAT.md` and `docs/SUMMONS_AND_COMPANIONS.md` summon framework already supports class-authored summon quantity/lifetime, autonomous or player-controlled behavior, summon threat, owner attribution and owner-level scaling. There is **NO global summon cap**, so Acolyte needs its **own authored cap and cost** once the user decides. Do **not** assume it inherits any DiceBound Necromancer rules or its three-summon maximum. Acolyte's exact summons, whether temporary or sustained and behavior on owner death remain OPEN.

### Current confirmed four abilities — updated through Round 3

1. **Withering Curse** (working name): single-target **FLAT reduction** to both target **Magical Defense** and **attack speed**; do NOT represent this as −10% of defenses or attack rate automatically. **Bosses reduce its applied debuff through their Tenacity value**, rather than a separate blanket boss immunity or manually authored "half strength" exception. **Exact flat values and units, duration, rank scaling, and mathematical Tenacity effect/stacking remain OPEN**. Later classes may have more curses, but **Tier III is deferred to World 2**, not current design.
2. **Radiation Plague** (working name): a **relatively weak Radiation-element damage-over-time** that **propagates by spatial contact**, not by a dying enemy automatically launching a new wave. When an uninfected enemy remains **near an infected one continuously for 2 seconds**, that nearby enemy becomes infected; if they move away before 2 seconds, they do not yet qualify. **An infection must NOT spread back to the same target which already had the DoT in the current chain**; protect against A→B→A and loops/repeated ping-pong via propagation-lineage/previously-visited recipient tracking. **This does not yet forbid a new deliberate Acolyte cast from reapplying/refreshing that target; independent-cast policy OPEN**. Range for "near," duration, periodic tick rate/damage, whether spread from secondary infected recipients, and whether proximity timer resets or pauses on separation remain to agree (default suggestion: reset on separation). **+1 Soul Charge to the Acolyte when a unit dies WHILE carrying the Acolyte's Radiation DoT**, whether or not that DoT delivered the killing blow; avoid duplicate per-death rewards for multiple spread paths. DoT's ordinary ticking does NOT itself generate Soul Charges.
3. **Summon Skeleton** (working name): **up to FOUR active skeletons simultaneously per Acolyte**, **each lasting exactly 15 seconds unless killed earlier**, **one skeleton per cast ASSUMED but not explicitly selected**, each cast costing **4 Soul Charges**. Each skeleton is a **directly player-controllable owned unit** using DiceFree's preexisting RTS selection/order framework. **No global summon-cap side effect**—limit applies to Acolyte only. Because charge cap is 8, the player can summon two skeletons from a full bar, then earn more Soul Charges (e.g. +1/tick from Drain Life) to approach active cap4 before the oldest timed skeletons expire. At cap4, whether summoning is disallowed vs oldest-replaces-newest is OPEN; recommendation: disallow until a slot becomes free. Skeleton stats, commands/idle aggression, targeting and Mana cost TBD.
4. **Drain Life** (working name): a **FIVE-second CHANNEL with up to FIVE tick events at ONE per second** (candidate tick timestamps +1,+2,+3,+4,+5s, if uninterrupted). Each successful damage tick deals **Magical Void** damage and generates **+1 Soul Charge**, capped by resource8. Each tick also restores the Acolyte's HP based on a **SKILL-RANK-SCALING PERCENTAGE of damage ACTUALLY removed from target HP after all defense/resistance/shield rules**. **Zero restoration from absorbed enemy shield damage**; zero healing on ineffective/fully shielded ticks. **Movement, being stunned, or an explicitly interrupting event cancels the rest of the channel**; ordinary incoming damage alone does not. Initial heal fraction, growth/cap, tick damage and interruption expenditure remain OPEN. This is not a universal lifesteal rule for other spells.

**Soul Charge economy (ACOLYTE-009/014):** explicit bar **0–8**, +1 on each **successful Drain Life tick**, +1 for **each death while the caster's Radiation DoT is present**, **ZERO from merely applying Curse or DoT**; four charges per skeleton. General Round1 "offensive spells can generate charges" is now narrowed: only explicitly authored generator events should give charges, and hypothetical future offensive spells need their own rules. Avoid automatically paying on DoT ticks, on hits by summoned skeletons, or on reapplication, or granting multiple charges for the same death from multiple infection routes. Unspent charge decay/persistence still OPEN.

**Tenacity (ACOLYTE-011):** user explicitly requires **boss Tenacity reducing the Curse's effects**, NOT arbitrary boss-ineligible flags. Current referenced docs do not supply an obvious normative numeric Tenacity formula for flat MDEF and attack-speed debuffs, so **the formula/unit choices need separate agreement**. Do not invent a boss Tenacity percentage coefficient and present it as finalized.

**Void element (ACOLYTE-010):** user's Drain Life element is explicitly Void. The global `docs/ELEMENTS.md` already records the approved Acolyte Void-element identity and the need to verify registry existence later. Do not add Arcane/Death as substitute elements or assume Void procs are already defined.

**Grimoire:** normal wand and stat-bearing book offhand. Special spell-changing book effects are not required in Tier II; future item Intrinsics may modify spells across all classes.

**Graveyard advancement:** respectable burial of deceased from Cornberg and Trimery is a future quest concept; site/objectives remain DEFERRED until planned World 1 area/city work.

## Round 1 — five user answers recorded 2026-10-19

- **ACOLYTE-001 CONFIRMED:** the class should have **one Curse, one distinct DoT, one Summon and one channeled Life Drain** as its basic intended four combat features. No final name/skill count/coefficients yet.
- **ACOLYTE-002 CONFIRMED OVERRIDE:** equipped **grimoire is a conventional OFFHAND STAT ITEM FOR NOW** (wand + book), rather than a mandatory special class page/charge/inscription system. Like ordinary equipment, later items may modify Acolyte skills through independently authored gear effects/intrinsics. The earlier v0.2 "must have meaningful active/passive grimoire mechanic" suggestion is **SUPERSEDED**.
- **ACOLYTE-003 CONFIRMED:** **Mana AND Soul Charges**; **offensive spells generate** Soul Charges, **summons cost/spend** Soul Charges. Charge gain semantics/cap/cost and possible DoT or life-drain tick farming to decide.
- **ACOLYTE-004 CONFIRMED:** **hybrid sustained damage + curse/debuff + self-sustain**, contrasting with Mage's burst and smaller CC toolkit. Acolyte should solo effectively without substituting for Priest primary healing.
- **ACOLYTE-005 CONFIRMED STORY CONCEPT, QUEST DESIGN DEFERRED:** **graveyard NPC** asks player to collect dead bodies/remains from **both Cornberg and Trimery** and bring them to cemetery for **proper burial**. No final NPC location, body targets, quest dialogue, stage rules or implementation approved; hold until World 1 region/city planning.

## Round 2 — five user answers recorded 2026-10-19

- **ACOLYTE-006 CONFIRMED:** Acolyte's first curse reduces **Magical Defense AND Attack Speed**. Numeric debuff and duration not selected. User would like more curses in **future classes**, but **do not design Tier III in World 1**.
- **ACOLYTE-007 CONFIRMED:** a **relatively weak Radiation-element DoT that spreads** to other enemies. Spread algorithm and duration/open. Target death **while afflicted** later confirmed Soul Charge generation in ACOLYTE-009.
- **ACOLYTE-008 CONFIRMED:** summon **SKELETONS**, **directly controllable by the player**, each lasting **a fixed amount of time**. Max active count, lifespan, command defaults and scaling open.
- **ACOLYTE-009 CONFIRMED:** Soul Charges **cap8**, summon **cost4**. **Successful Drain Life ticks generate charges**; a unit **dying while afflicted by the caster's Radiation DoT** generates charges. General offensive spells can also earn charges per Round1; their event/rates open.
- **ACOLYTE-010 CONFIRMED:** **Drain Life 5-SECOND channel, one damage tick EACH SECOND**, heals based on damage done, **Void-element** damage, and standard channel cancellation on moving/stunned/explicit interruption (ordinary noninterrupting damage does not cancel). Actual heal conversion fraction, Mana/cooldown, tick charge yield open.

## Round 3 — five user answers recorded 2026-10-19

- **ACOLYTE-011 CONFIRMED:** Curse reduces enemy **Magical Defense and attack speed by FLAT VALUES**, not a fixed percentage. **Bosses' Tenacity determines their reduced susceptibility to the Curse**. Exact amounts, flat attack-speed units, effect duration, boss Tenacity coefficient and rank scaling OPEN.
- **ACOLYTE-012 CONFIRMED:** the Radiation DoT spreads to a nearby enemy only after **2 continuous seconds of proximity** to an infected enemy; DoT **cannot spread BACK to an enemy previously infected within the spread chain**. It is **NOT on-death spread**. Exact proximity radius, contact timer reset/pause, chain fanout and same-target new-cast refresh OPEN.
- **ACOLYTE-013 CONFIRMED:** **FOUR active skeletons maximum**, **15-second fixed duration each**; player-controllable as already confirmed; one skeleton per summon and what happens at active cap remain OPEN.
- **ACOLYTE-014 CONFIRMED:** **+1 Soul Charge per successful Drain Life tick** and **+1 per death of an enemy while it has Acolyte Radiation DoT**, independent of which attack finished enemy. Applying Curse or Radiation grants **0 Soul Charges**. Other hypothetical direct offensive spell generators not yet decided.
- **ACOLYTE-015 CONFIRMED:** Drain Life's self-heal is a **percentage of actual HP damage inflicted**, with healing **percentage increasing with Drain Life skill rank**. **Shield-absorbed damage provides zero healing**. Initial percent, growth and max healing cap OPEN; earlier flat **50%** suggestion is NOT approved as a universal constant.

## Round 4 — five questions (AWAITING USER)

**ACOLYTE-016 — Flat curse values and Tenacity:** How about **−5 Magical Defense (flat)** and **−0.10 attacks/second (flat attack speed)** for **10 seconds** at Curse rank1, then gradually stronger with rank? If DiceFree's attack-speed UI uses a different scale, we'll translate the −0.10 into that authored stat instead of hard-coding percentage. For boss Tenacity, shall we use an agreed **global Tenacity rule** once defined, rather than create a special boss exception just for Acolyte?

**ACOLYTE-017 — Radiation proximity and reinfection:** Should "near" mean within **3 meters**, with the **2-second contact timer resetting if enemies separate**, and can **any newly infected enemy then infect another** (A→B→C)? I'd propose **yes**, while tracking which enemies were infected by each infection chain so it cannot circle back A→B→A. Should deliberate re-casting Radiation on an already affected target refresh the duration?

**ACOLYTE-018 — Skeleton behavior at four:** When 4 skeletons are already active, should casting Summon Skeleton be **blocked without spending resources**, or **replace the oldest skeleton**? Also, should skeletons attack nearby enemies on their own when no specific player order is queued, while remaining selectable/commandable?

**ACOLYTE-019 — Life Drain heal progression:** Should Drain Life heal **25% of actual HP damage at rank1**, increasing by **+2 percentage points per skill rank** (e.g. 25% at rank1, 35% at rank6), capped at around **60–65%**? We can pick the exact cap after choosing its maximum rank. It heals nothing from absorbed shields either way.

**ACOLYTE-020 — Completing Tier II Acolyte's kit:** We have four pillars, but might want a **fifth direct-damage spell that builds Soul Charges** (e.g. a short-cooldown **Occult Bolt**) and a **sixth passive** to strengthen curses/skeletons or Mana economy. Would you prefer **six skills total**, or should we keep the kit deliberately lean with just four or five, then distribute its skill ranks across ~80 levels?

## Decision ledger

| Date | ID | Topic | State |
| --- | --- | --- | --- |
| 2026-10-19 | WORLD1-ROSTER | Acolyte one of eight **normal** Tier II classes under Magically Touched Novice; next design before Thief and Psion | **USER APPROVED** |
| Earlier | ACOLYTE-FOUNDATIONS | Wand + grimoire occult identity; older grimoire-mechanic requirement is SUPERSEDED by ACOLYTE-002 | **BASE WEAPON CONCEPT KEPT; EXTRA BOOK SYSTEM REJECTED** |
| 2026-10-19 | WORLD1-ORDER | Eight normal Tier II → Quillboar area → big city; Tier III only in World 2 | **USER APPROVED** |
| 2026-10-19 | ACOLYTE-001 | Curse, separate DoT, summon, channeled life drain | **CONFIRMED skill pillars** |
| 2026-10-19 | ACOLYTE-002 | Grimoire normal offhand stat item; future equipment effects not Acolyte-only book mechanics | **CONFIRMED; supersedes earlier unique grimoire mechanic** |
| 2026-10-19 | ACOLYTE-003 | Mana + Soul Charges gained via offensive spells, spent on summons | **CONFIRMED; resource coefficients still open** |
| 2026-10-19 | ACOLYTE-004 | Slow ramp damage, curse/debuff hybrid, self sustain vs Mage burst | **CONFIRMED** |
| 2026-10-19 | ACOLYTE-005 | Graveyard burial quest concept sourcing deceased from Cornberg and Trimery | **CONFIRMED STORY DIRECTION; QUEST DELAYED** |
| 2026-10-19 | ACOLYTE-006 | Curse lowers target Magical Defense and attack speed; future classes can have more curses | **CONFIRMED** |
| 2026-10-19 | ACOLYTE-007 | Weaker, spreading Radiation-element DoT | **CONFIRMED; spread mechanics open** |
| 2026-10-19 | ACOLYTE-008 | Controllable limited-duration skeleton summons | **CONFIRMED; active cap/lifetime open** |
| 2026-10-19 | ACOLYTE-009 | Soul Charges max8, 4 spent/summon, gain from drain ticks and death of Radiated DoT target | **CONFIRMED; per-event numbers open** |
| 2026-10-19 | ACOLYTE-010 | Void 5s channeled life drain, 1 damage+heal tick each second, same channel interrupt behavior | **CONFIRMED; heal fraction open** |
| 2026-10-19 | ACOLYTE-011 | Curse flat MDEF + attack-speed reduction; boss Tenacity reduces effects | **CONFIRMED; values and formula OPEN** |
| 2026-10-19 | ACOLYTE-012 | Radiation contact spread after 2 seconds nearby; no repeat recipient in same spread chain | **CONFIRMED; radius/contact details OPEN** |
| 2026-10-19 | ACOLYTE-013 | Max 4 simultaneously controllable skeletons; each lasts 15 seconds | **CONFIRMED; cap behavior OPEN** |
| 2026-10-19 | ACOLYTE-014 | +1 Soul Charge on Drain Life tick, +1 on afflicted target death; +0 from applying Curse/Radiation | **CONFIRMED** |
| 2026-10-19 | ACOLYTE-015 | Rank-scaling % healing based only on actual HP damage; zero from shields | **CONFIRMED; percentages OPEN** |
| 2026-10-19 | ACOLYTE-016–020 | Round 4 questions | **AWAITING USER** |

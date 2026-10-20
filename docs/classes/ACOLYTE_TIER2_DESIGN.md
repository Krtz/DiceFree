# DiceFree — Acolyte Tier II — design interview

**Created:** 2026-10-19  
**Status:** WORLD 1 STANDARD TIER II CLASS. **ROUNDS 1–8 USER DECISIONS RECORDED THROUGH 2026-10-20**. Six-skill Tier II kit now consists of Curse (rank1 −5 MDEF, −0.10 attacks/s for 10s; shared boss Tenacity attenuation), Radiation DoT (3m/2s continuous contact, timer resets, chain spreads, new manual casts refresh), player-controlled 15s skeleton summons (cap4, new summon replaces oldest, idle aggression follows hero Auto-Retaliate), 5s Void Drain Life (heals 25% of actual HP damage at rank1, +2 percentage points per rank; 60–65% vicinity suggested cap), NEW 1s Void Mortal Coil (hostile INT+SPR damage, ally/owned-summon healing), and NEW INT+small SPR/poison-on-basic passive. Mana/Soul Charge cap8/cost4 skeleton, +1 per drain tick and affected target death. Round5 CONFIRMS Void-element Mortal Coil healing on self/allies/skeletons, standard one-second channel interruption, enemy-hit +1 Soul Charge and healing +0, and +2 INT/+1 SPR per passive rank with nonstacking three-second poison refreshed on each successful wand basic attack. User APPROVED the exact 80-rank progression and its gates in ACOLYTE-026. Round6 confirms INT+SPR Mortal Coil scaling with SPR emphasis in Void healing, **one skeleton per summon costing 4 Soul ONLY and ZERO Mana**, and **MAGICAL COFFEE-element, NOT Donut, Poison-tag DoT** (corrected user message) ticking each second for 3s without spreading. **Initial cooldown and Mana values are now USER-APPROVED (Round7)**. Radiation duration is 3s at rank1 to 8s at rank16; skeleton rank improves HP/basic damage; Coffee poison refresh preserves its tick timer; Drain Life ticks grant +1 Soul and damage shield even when fully blocked, but heal 0 from absorbed damage. **Other raw damage coefficients remain designer PROPOSALS**. Global Tenacity formula and graveyard quest remain open. **DESIGN/DOCUMENTATION ONLY; NO UNITY IMPLEMENTATION.**  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md), [Tier II Core Classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Class Requirements and Discovery](../CLASS_REQUIREMENTS_AND_DISCOVERY.md).

## FINAL ROUND 8 — AUTHORITATIVE APPROVAL (2026-10-20)

**The user has completed Acolyte's Tier II design and requested Thief next. The following newer decisions OVERRIDE earlier proposals or "open" entries anywhere below. Design only; NO Unity implementation.**

- **ACOLYTE-036:** Radiation Plague duration is **3s at ranks1–3, 4s ranks4–6, 5s ranks7–9, 6s ranks10–12, 7s ranks13–15, 8s rank16**. Formula `3+floor((rank−1)/3)` seconds.
- **ACOLYTE-037:** Radiation propagation (3m proximity, 2 continuous seconds, resets upon separation) gives each new infected enemy **ONLY the source's REMAINING DoT duration at transmission**, never a fresh full duration; the chain can't infect an already infected prior host. **RADIATION DAMAGES EVERY 0.1 SECONDS (10 TICKS PER SECOND)**, not once per second. User approved lowering each tick accordingly: divide the formerly approved one-second raw tick formula by **10**, yielding `rawDamagePer0.1sTick = [(2+0.10×casterINT)×(1+0.05×(RadiationRank−1))]/10`; this maintains the intended DPS. A source that expires before the 2s contact is completed cannot spread, and ordinary DoT ticks grant no Soul Charges.
- **ACOLYTE-038:** Skeleton MAX HP uses owner's **SPR, NOT VIT**: `(55+2×ownerSPR_atSummon)×(1+0.05×(SummonRank−1))`. Skeleton normal **Physical basic attack** RAW damage: `(3+0.18×ownerINT_atSummon)×(1+0.06×(SummonRank−1))`. **SNAPSHOT owner stats at the instant skeleton is summoned**, never dynamically reevaluate from later buffs/gear. This also confirms HP+5% and attack+6% per additional Summon rank. Cap4, one per 4 Soul cast, 15s lifetime, oldest replacement, hero Auto-Retaliate and 0 Mana unchanged.
- **ACOLYTE-039:** Curse flat MDEF reduction `5+(rank−1)` and flat attack-rate reduction `0.10+0.005×(rank−1)` attacks/sec for10s. At rank14 **−18 MDEF / −0.165 attacks/sec**. Boss application remains governed by shared Tenacity, still awaiting its global system equation.
- **ACOLYTE-040:** All previously proposed RAW coefficient curves for Radiation, Drain Life, Mortal Coil enemy Void damage and allied Void healing, and MAGICAL COFFEE Poison ticks are **APPROVED FIRST-PLAYTEST BASELINES**, modified by the explicit 0.1s÷10 Radiation tick requirement above and owner-SPR skeleton HP. They are not implemented and may be balanced after testing.

### Full approved first-playtest numeric baseline

Let `r` be rank of that skill, `INT` current caster Intelligence, `SPR` current Spirit (summon OWNER stats are snapshotted). All values are initial tuning, not playtest measurements:

| Skill | Approved cap | CD / Mana | Approved raw formula, delivery and other core effects |
| --- | ---: | --- | --- |
| Withering Curse | 14 | 12s / 12 Mana | 10s duration, −(5+(r−1)) MDEF, −(0.10+0.005×(r−1)) attacks/s; global boss Tenacity |
| Radiation Plague | 16 | 10s / 15 Mana | DoT duration `3+floor((r−1)/3)`s. Magical Radiation RAW **each 0.1s**: `((2+0.10×INT)×(1+0.05×(r−1)))/10`. Contact spread3m/2 continuous seconds, new host INHERITS remaining source time; never backtracking. DoT victim death +1 Soul |
| Summon Skeleton | 12 | 3s / **0 Mana +4 Soul** | 1 controllable15s skeleton, max4/replaces oldest. **HP `(55+2×ownerSPR_atSummon)×(1+0.05×(r−1))`**; Physical basic RAW `(3+0.18×ownerINT_atSummon)×(1+0.06×(r−1))`; SNAPSHOT owner stats, not live |
| Drain Life | 20 | 14s / 24 Mana | 5s Void Magical channel, **ONE tick/s**, RAW each tick `(4+0.20×INT+0.10×SPR)×(1+0.06×(r−1))`. Resolved ticks DAMAGE shields and +1 Soul even FULLY blocked; healing `25%+2pp×(r−1)` of actual enemy HP damage only (max63%), 0 from shield absorption |
| Mortal Coil | 10 | 7s / 14 Mana | 1s interruptible cast. Enemy Magical Void RAW `(10+0.60×INT+0.30×SPR)×(1+0.08×(r−1))`; allied/self/skeleton **Void ELEMENTAL HEAL** RAW `(10+0.25×INT+0.70×SPR)×(1+0.08×(r−1))` before healing/resistance modifiers; successful hostile +1 Soul, healing0 |
| Occult Adept (working name) | 8 | Passive / 0 | +2 INT and +1 SPR per rank. Successful wand basic refreshes **one 3s nonstacking nonspreading Poison-tag Magical COFFEE DoT** with once/sec continuous tick clock; RAW/tick `1+0.06×INT+0.35×(r−1)` |

**Implementation follow-up only** (NOT grounds to continue class interview now): shared boss Tenacity numeric calculation, elemental Void/Coffee registry, round-off of frequent Radiation ticks, infection chain ownership and expiry, summon visuals/RTS controls, immune/invalid targets, actual economy tuning, graveyard mentor burial quest involving Cornberg and Trimery. **Acolyte interview complete. Thief begins now. Tier III design is excluded until World 2.**

## User-approved World 1 roster and design order

There are EXACTLY EIGHT normal World 1 Tier II classes, four under each Tier I parent:

- **Physically Blessed Novice →** Greatblade, Defender, Ranger, Thief.
- **Magically Touched Novice →** Mage, Priest, **Acolyte**, Psion.

**Next design interview Acolyte → then Thief → then Psion**, then Quillboar area, then big city. **Tier III is for World 2; DO NOT design any Tier III class skills/advancements now.** The user may revisit some mentor locations after the city is designed, but has NOT yet approved moving existing mentors or quests.

## Earlier foundations to preserve

1. **APPROVED AS NORMAL CLASS IN ROSTER (2026-10-19):** Acolyte belongs to **Magically Touched Novice at Tier I level 30** and starts as a new **Tier II level-1 manifestation**, preserving the Tier I parent state, in line with all standard advancement rules. Acolyte's *name as final shipping title*, specific skill kit, body, quest and numeric combat rules are still open.
2. **CONFIRMED ACOLYTE-002, OVERRIDING earlier `docs/TIER2_CORE_CLASSES_DESIGN_V02.md` concept:** Acolyte uses **wand (mainhand) + grimoire (offhand)**, but **the grimoire is a normal stat-bearing equipment item for now**. There is **NO mandatory book-specific page, charge, inscription or special offhand class mechanic**. Like all other gear, future authored item Intrinsics/effects may modify class skills, subject to later gear design. This later user choice explicitly supersedes the older 'book MUST have its own mechanic' statement.
3. **CONFIRMED ACOLYTE-001/004:** Acolyte's fighting rhythm emphasizes **SLOW sustained damage + curses/debuffs + self-sustain** compared with burst/CC Mage; the original four spell pillars are **a distinct curse, a distinct damage-over-time ability, a summon, and a channeled life drain**, now joined in Round 4 by **Mortal Coil (1-second Void damage/ally healing)** and **a stat+basic-attack-poison passive**, for a TOTAL SIX-SKILL working Tier II kit. Make it a hybrid solo-sustainable damage/debuffer rather than Mage clone or primary Priest-style healer. **ROUND 2 NOW CONFIRMS Radiation for DoT and Void for Drain Life, controllable timed skeletons, and Curse Magical Defense/attack-speed debuffs.** Exact coefficients, summon caps, skill names and final skill count remain open.
4. **CONFIRMED ACOLYTE-003:** Acolyte uses **Mana PLUS Soul Charges**. **Successful offensive spells** generate Soul Charges; **summons consume Soul Charges**. **ACOLYTE-009 NOW CONFIRMS MAX 8 SOUL CHARGES and COST 4 per skeleton summon; each successful Drain Life damage tick generates charges, and a unit dying WHILE affected by the caster's Radiation DoT triggers Soul Charge gain.** Other offensive spells' charge gain rates, exact charges per tick/death, persistence/decay and summon Mana cost remain open. Soul Charges are Acolyte's own resource, **not a grimoire equipment mechanism**. Avoid inventing automatic charge gain from ticks that would cause abusive scaling.
5. **GLOBAL ABILITY/RANK ECONOMY:** all new Tier II skills start **rank 0**, the Tier II character starts level1 with **1 spendable skill point** and earns one per level until the total configured rank capacity. Recent Greatblade/Mage/Priest/Defender/Ranger designs target **~78–80 total class-skill ranks by class level80**, and skill-specific rank-level eligibility should permit a point purchase each class level. Exact Acolyte skills/max ranks/gates will be designed AFTER its combat kit is selected, not preselected.
6. **CONFIRMED ACOLYTE-005 STORY DIRECTION, LOCATION/OBJECTIVES NOT YET FINAL:** An NPC **at a graveyard** asks an eligible Magically Touched Novice to **go to Cornberg and Trimery, collect the bodies/remains of deceased people, return them to the graveyard for a proper burial**, and then advance to Acolyte. This is a quest CONCEPT, **not an approved final level layout, NPC location, body spawn list, quest counter, collection mechanics or dialogue**. User explicitly wants to return to quest design later, perhaps when shaping the major city. Do not invent graveyard location or move other mentors now.

## Confirmed Round 1 identity and proposed future kit framework

### User-approved combat identity

| Topic | Approved decision | Still open |
| --- | --- | --- |
| Role | **Sustained occult damage, curse-based debuffs and self-sustain**, slower ramp than Mage burst with some crowd control | Actual Curse effects, solo DPS, group debuff power, defense, healing cap and boss interactions |
| Pillar 1 | **Curse** reduces enemy **flat 5 MDEF AND flat 0.10 attacks/second**, for **10 seconds at rank1**. Boss Tenacity reduces debuff using a SHARED global Tenacity rule (precise equation not authored yet). | Rank growth, final cooldown/Mana cost, shared Tenacity numeric formula, recast stacking/refresh OPEN |
| Pillar 2 | Weak spreading Magical Radiation DoT **3s at rank1 → 8s at rank16**; spreads after 2 continuous seconds within 3m, resets proximity on separation, chains without loops, manual recast refresh. Death while afflicted +1 Soul. | Intermediate duration ranks/new infected host duration and Radiation tick cadence OPEN; damage raw coefficient proposed |
| Pillar 3 | One controllable skeleton/cast, 4 Soul, ZERO Mana, approved 3s CD. Cap4, 15s each, replaces oldest; hero Auto-Retaliate OFF orders only/ON retaliation. **Ranks raise skeleton HP and normal basic attack damage**, not count/duration/cost. | Exact numeric HP/damage growth formulas still DESIGNER PROPOSED |
| Pillar 4 | Drain Life 5s Void channel, one damage tick/sec; **14s CD/24 Mana APPROVED**. Every resolved hit **DAMAGES eligible enemy shield and yields +1 Soul even if 100% blocked**. Heal 25% rank1 +2 percentage points/rank of ACTUAL ENEMY HP damage (max63% at rank20), ZERO heal from shield absorbed damage. | Raw tick damage coefficient and immunity behavior OPEN |
| Pillar 5 | Mortal Coil 1s interruptible channel: INT+SPR Void Magical ENEMY DAMAGE emphasizing INT, or self/ally/skeleton **VOID-ELEMENT HEALING** emphasizing SPR, both improving with rank. +1 Soul on enemy damage, 0 on healing. | Coefficients/Mana/CD proposed only |
| Pillar 6 | Passive grants +2 INT/+1 SPR per rank (approved cap8); every landed wand auto applies/refreshes one nonstacking, NONSPREADING 3s Poison-tag DoT with **MAGICAL COFFEE** damage tick1/sec (three ticks total). **User corrected mistaken Donut to Coffee.** | Raw poison damage and tick-refresh clock remain proposed/open |
| Resources | **Mana + Soul Charges CAP8**, cost **4 per skeleton summon**. **+1 for each successful Drain Life damage tick** and **+1 on death of enemy currently afflicted with caster's Radiation DoT**. **Applying Curse or Radiation DoT gives 0 charges**; their ordinary ticks are NOT automatic charge generators. | Whether other *future direct offensive spells* earn charges (must be explicitly authored), charge decay/persistence, simultaneous death attribution OPEN |
| Equipment | **Wand mainhand + grimoire offhand**, both ordinary authored gear/stat/intrinsic carriers | Authorable drops/Intrinsics, class weapon permissions, exact item stats; **no unique grimoire charges/pages/inscriptions system** |
| Advancement | **Graveyard caretaker** concept: collect dead from **Cornberg and Trimery** for proper burial at graveyard | Exact cemetery site, NPC, remains locations/counters, story presentation and trial scripts intentionally DEFERRED |
| Future rank economy | Exact 80-rank gates and caps **USER APPROVED**: Curse14, Radiation16, Skeleton12, Drain Life20, Mortal Coil10, passive8; every level1–80 has an eligible purchase, final gate at77. | Damage/CD/Mana balance unapproved and Unity implementation absent |

**Six-skill kit CONFIRMED as of Round 4:** Curse, distinct Radiation DoT, skeleton Summon, channeled Drain Life, **Mortal Coil**, and an **INT/SPR + poison-on-autoattack passive**. Earlier hypothetical Occult Bolt and Forbidden Knowledge are SUPERSEDED and should not be added as extra abilities. Soul Charge generation via enemy-directed Mortal Coil remains OPEN; no automatic yield on its friendly healing use. Spell ranks and class levels remain separate from weapon upgrades.

### Summon framework compatibility (not yet summon-specific approval)

DiceFree's shared `docs/COMBAT.md` and `docs/SUMMONS_AND_COMPANIONS.md` summon framework already supports class-authored summon quantity/lifetime, autonomous or player-controlled behavior, summon threat, owner attribution and owner-level scaling. There is **NO global summon cap**, so Acolyte needs its **own authored cap and cost** once the user decides. Do **not** assume it inherits any DiceBound Necromancer rules or its three-summon maximum. Acolyte's exact summons, whether temporary or sustained and behavior on owner death remain OPEN.

### Current SIX Tier II skills — Round 4, confirmed core mechanics

1. **Withering Curse** (working name) — at **rank1**, lower target **Magical Defense by flat 5** and **attack rate by flat 0.10 attacks per second** for **10 seconds**. It is NOT −5% MDEF/−10% attack speed. Bosses experience a reduced effect using the **shared/global Tenacity rule**, not a unique Acolyte boss modifier; actual formula for Tenacity and attack-rate clamping remain OPEN. Rank1 amounts/duration are approved; rank growth and stacking/recast refresh are not yet chosen.
2. **Radiation Plague** — weak, spreading **Magical Radiation DoT** with **USER-APPROVED duration 3 seconds at rank1 increasing to 8 seconds at rank16**. Intermediate ranks undecided; DESIGNER PROPOSAL for whole seconds is ranks1–3 3s,4–6 4s,7–9 5s,10–12 6s,13–15 7s,rank16 8s. Any uninfected target within **3m** of an afflicted enemy for **2 CONTINUOUS seconds** becomes infected; contact resets on separation, newly infected enemies may infect others, chains cannot reinfect previously infected units through spread. Deliberate recasting on an afflicted unit refreshes duration. +1 Soul Charge on afflicted target death, not on initial application or ordinary periodic ticks. **OPEN:** Radiation tick interval (earlier 1/s was a PROPOSAL), newly infected target's full vs remaining duration, raw damage/ownership, cross-caster behavior.
3. **Summon Skeleton** — summons exactly one directly controllable skeleton per cast at **3s cooldown and 4 Soul Charges, 0 Mana** (all user-approved first-playtest values). Max4 each lasting15s, casting again replaces oldest. Hero Auto-Retaliate OFF: only explicit attacks; ON: retaliate when attacked, respecting RTS orders. **USER APPROVED rank scaling to skeleton max HP and basic attack damage, NOT duration/count/Soul cost**. DESIGNER numeric suggestions `55+2×owner VIT` base HP, `3+0.18×owner INT` basic Physical raw attack, +5% HP/+6% attack damage per added rank; **numeric coefficients NOT YET separately approved**.
4. **Drain Life** — a **5-second Magical Void damage CHANNEL**, 1 damaging tick per second, approved **14s cooldown/24 Mana**. Each SUCCESSFULLY RESOLVED damage tick **deals damage to an eligible enemy shield under normal shield mechanics and awards +1 Soul Charge even when the shield blocks ALL damage**, capped at8. Healing is calculated SEPARATELY from **ACTUAL enemy HP damage** that passes mitigation/shields: heals25% at rank1, +2 percentage points per rank through 63% at approved rank20. If shield absorbs all damage, **0 healing even though shield loses HP and +1 Soul is gained**. Cancels on caster movement/stun/explicit interrupt, ordinary incoming damage not interrupting. OPEN: raw tick damage rank scaling, no valid target/full immunity and interruption costs.
5. **Mortal Coil**: one-second channel cancels on movement, stun or explicit interruption but not normal incoming damage. Enemy resolution delivers Magical Void damage scaling with **INT AND SPR, INT-weighted**, awarding **+1 Soul Charge on successful damage**. Self/ally/skeleton resolution delivers **Void-element HEALING scaling with BOTH SPR and INT, SPR-weighted**, awarding ZERO Soul. Both paths increase with skill rank. Healing is modified by recipient Void resistance and general healing bonuses. Exact raw coefficients and CD/Mana appear as unapproved balance proposals below.
6. **Passive — Occult Adept** (working name): **+2 INT and +1 SPR per rank**, approved cap8 = **+16 INT/+8 SPR**. Each successfully landed wand basic applies/refreshes one **3s NONSTACKING NONSPREADING Poison-status damage-over-time**, dealing **MAGICAL COFFEE-ELEMENT DAMAGE ONCE PER SECOND** (3 ticks if uninterrupted). Coffee replaces the user's initially mistyped Donut. Poison is the effect tag for dispels, Coffee is the element for resistance. Poison tick raw damage scales with INT and passive rank, exact coefficient and refresh-tick phase designer proposals. No Soul gains from poison.

### Established Acolyte Soul Charge economy

- **Mana + Soul Charges**, capacity **8**, **4 Soul Charges per skeleton**; details such as whether Summon also spends Mana/charges decay between combats still open.
- **+1 Soul Charge per successful Drain Life tick**, **+1 per enemy death while marked by THIS Acolyte's Radiation DoT** (irrespective of which player dealt final blow), both capped at 8.
- **ZERO Soul Charge merely from applying Curse or Radiation**. Ordinary Radiation ticking, skeleton hits and passive poison ticks are NOT approved charge generators.
- **ACOLYTE-022 confirmed:** successful enemy-damaging Mortal Coil grants **+1 Soul Charge**; any self/ally/skeleton Void-healing Mortal Coil grants **ZERO**. Aborted casts and poison ticks must not award charges.
- **Shared global Tenacity** should govern boss susceptibility to the flat Curse debuffs. No authoritative exact formula located yet in this design thread, so this is an integration question, not a numerically resolved parameter.
- **Summons are class-authored**: four-skeleton cap is not universal, 15s lifespan, older replaced at cap and same owner toggle for Auto-Retaliate. They can still be directly commanded using existing RTS selection and unit orders.
- **General Tier II skill ranks:** all 6 begin at rank0 and require purchased ranks, with ~78–80 total purchasable ranks by level80. No rank caps or skill-level gates chosen until next interview round. Details of graveyard burial quest remain deferred.

### Earlier proposals superseded

- Ghost/spirit summon, autonomous-only skeleton AI, blocked summon at cap4, solely four skill kit, Occult Bolt fifth skill, and book-specific class ability are superseded. **Mortal Coil and INT/SPR-poison passive are the new fifth/sixth skills**. No Tier III skills designed or implemented here.

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

## Round 4 — five user answers recorded 2026-10-19

- **ACOLYTE-016 CONFIRMED:** Curse rank1 **−5 flat Magical Defense AND −0.10 attacks/sec** for **10 seconds**, following the **global Tenacity rule** for bosses when its exact formula is established. Debuff rank growth and attack-speed clamping OPEN; no bespoke boss-only override.
- **ACOLYTE-017 CONFIRMED:** Radiation spreads within **3m** after **2 uninterrupted seconds** of proximity. Contact progress **RESETS if separated**; newly infected can infect others (A→B→C); track lineage against A→B→A. **Deliberate re-cast on infected target REFRESHES DoT duration**, distinct from automatic spread.
- **ACOLYTE-018 CONFIRMED:** at **four active 15s controllable skeletons**, a new cast **replaces the OLDEST**. Summon auto-retaliation/idle reaction **mirrors the HERO'S Auto-Retaliate toggle**; explicit RTS control remains available regardless. Exact initiation rules for idle attack beyond retaliation still open.
- **ACOLYTE-019 CONFIRMED:** Drain Life restoration is **25% actual enemy HP damage at skill rank1** and **+2 percentage points per additional rank**. The previously proposed **~60–65% eventual cap** is approved as a general target, but its exact value and interaction with final skill rank are not yet settled. No healing for shield-absorbed damage.
- **ACOLYTE-020 CONFIRMED ROSTER REVISION:** six current skills: (1) flat MDEF/attack-speed Curse, (2) spreading Radiation DoT, (3) controlled skeletons, (4) 5s Void Drain Life, (5) **Mortal Coil** — 1s channel dealing modest **Void damage scaled by INT+SPR on enemies**, **healing a friendly target (allies/summoned skeletons)**, and (6) passive providing **INT + lesser SPR + small poison from wand autoattacks**. Fifth skill is **NOT** Occult Bolt; no seventh skill added. Poison-proc/element and friendly heal targeting formulas OPEN.

## Round 5 — five user answers recorded 2026-10-20

- **ACOLYTE-021 CONFIRMED:** Mortal Coil **VOID-element healing** can target **SELF, allies and owned skeletons**, while an enemy receives Void Magical damage. Void healing is governed by the existing **elemental healing** rule: target Void resistance can increase/reduce the heal and extremely negative resistance can theoretically invert it; no bespoke exception approved yet.
- **ACOLYTE-022 CONFIRMED:** Mortal Coil 1s channel cancels if caster **moves, gets stunned or is explicitly interrupted**, ordinary incoming damage alone does NOT cancel. Successful ENEMY DAMAGE generates **+1 Soul Charge**; healing (self/allied/skeleton) generates **zero**.
- **ACOLYTE-023 CONFIRMED:** every **successfully landing wand BASIC/AUTOATTACK** applies **a small NONSTACKING Poison DoT lasting 3 seconds**. More wand basic hits **refresh** the single DoT; not a random chance, not extra stacks.
- **ACOLYTE-024 CONFIRMED:** passive **+2 INT and +1 SPR per passive rank**. Poison DoT damage scales with **INT and passive skill rank**, basic attacks only; poison amount and element still open.
- **ACOLYTE-025 USER DELEGATED:** create and verify a full **80-rank** six-skill schedule that allows buying a rank at every class level1–80, with individual rank caps/gates. Designer proposal below awaits approval of specific numbers.

## Acolyte 80-rank class progression — APPROVED baseline (ACOLYTE-026)

All 6 skills start at **rank0**. The class receives one spendable rank point at character class level1, then +1/class level until 80. Rank purchase requires all prior ranks of that same skill, and a minimum class level for the next individual rank. All caps, gates and intervals below were designer-selected and subsequently **USER-APPROVED as initial playtest values in ACOLYTE-026**.

| Skill | Max rank (proposed) | First gate | Interval | Last gate | Every level gate |
| --- | ---: | ---: | --- | ---: | --- |
| Withering Curse | 14 | 1 | Every 5 levels | 66 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66 |
| Radiation Plague | 16 | 1 | Every 5 levels | 76 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76 |
| Summon Skeleton | 12 | 1 | Every 6 levels | 67 | 1, 7, 13, 19, 25, 31, 37, 43, 49, 55, 61, 67 |
| Drain Life | 20 | 1 | Every 4 levels | 77 | 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69, 73, 77 |
| Mortal Coil | 10 | 6 | Every 7 levels | 69 | 6, 13, 20, 27, 34, 41, 48, 55, 62, 69 |
| Occult Adept (working name) | 8 | 1 | Every 10 levels | 71 | 1, 11, 21, 31, 41, 51, 61, 71 |
| **TOTAL** | **80** | — | — | **77** | Full 80-rank skill capacity eligible by class level77 |

**Drain Life 20-rank fit:** the USER-APPROVED healing ratio is `25% + 2 percentage points × (skillRank−1)`. This approved 20-rank cap puts the final healing conversion at `25% + 19×2%=63%` of actual HP damage dealt, inside the previously accepted approximate 60–65% cap direction. **63% is the outcome of the user-approved 20-rank cap and the earlier-approved 25% + 2 percentage points per rank formula.** With proposed passive max8, its totals would be +16 INT/+8 SPR.

### Verified one-point-every-level access

For class level `L`, `eligible(L)` = sum of all individual authored rank gates reachable by L. Every class level 1–80 must satisfy `eligible(L) >= L` for a continuous spend-one-point-per-level path; within each skill, ranks are purchased in ascending rank order and levels for those ranks strictly increase.

| Class level | Eligible rank purchases | Earned points | Cushion |
| ---: | ---: | ---: | ---: |
| 1 | 5 | 1 | 4 |
| 5 | 6 | 5 | 1 |
| 6 | 9 | 6 | 3 |
| 10 | 11 | 10 | 1 |
| 20 | 22 | 20 | 2 |
| 30 | 32 | 30 | 2 |
| 40 | 42 | 40 | 2 |
| 50 | 54 | 50 | 4 |
| 60 | 63 | 60 | 3 |
| 70 | 75 | 70 | 5 |
| 75 | 78 | 75 | 3 |
| 76 | 79 | 76 | 3 |
| 77 | 80 | 77 | 3 |
| 78 | 80 | 78 | 2 |
| 79 | 80 | 79 | 1 |
| 80 | 80 | 80 | 0 |

**Programmatic audit PASS:** across **all integer class levels from 1 through 80**, every level has at least as many eligible skill ranks as earned skill points; no gate is above80. **All 80 rank purchases are eligible by class level77**, and the last skill point arrives at level80. Skill upgrade-ready cues should be actionable, not just based on unused points. No automatic ranks on advancement, no over-cap purchase and no extra Tier III skill design during World1.

### Playtest cautions and open parameters

- Mortal Coil HEAL is **Void-element healing**, not magical Void damage cast at an ally. Global elemental healing resistance can make Void healing strong/weak/inverted depending on target resistance; do not silently override that.
- One successful enemy-damaging Coil earns +1 Soul; friendly Coil HEAL earns0. Poison automatic ticks, Curse/Radiation application, Radiation periodic ticks and skeleton hits do not inherit Soul income rules.
- Passive Poison is a NONSTACKING, NONSPREADING **three-second Poison-status DoT with MAGICAL COFFEE-element damage tick ONCE PER SECOND**, not Donut; the user's corrected answer supersedes the accidental Donut text. No separate 'Poison' damage element is being invented.
- Poison **Magical Coffee damage element and once-per-second tick cadence are approved**; first-rank poison raw coefficient/refresh semantics, Mortal Coil healing coefficients, CDs/Mana and the global Curse Tenacity formula remain open or designer-proposed.
- Grimoire stays ordinary stat item; future items/intrinsics can alter spells across all classes. Graveyard story quest from Cornberg+Trimery remains an idea reserved for region/city design. **No C#/prefab/Unity changes from this work.**

## Round 6 — user-approved decisions and explicit CORRECTION (2026-10-20)

- **ACOLYTE-026 USER APPROVED:** exact 80 rank caps/gates **Curse14/Radiation16/Skeleton12/Drain20/Coil10/Passive8**, mathematical validation across levels1–80. Drain Life rank20 heals **63%** actual HP damage; passive rank8 +16 INT/+8 SPR.
- **ACOLYTE-027 USER APPROVED:** Mortal Coil damage **INT-weighted INT+SPR**, healing **SPR-weighted SPR+INT**, both increase with Mortal Coil rank. Exact coefficients are designer proposals.
- **ACOLYTE-028 USER CORRECTION (AUTHORITATIVE):** initially typed "magical donut damage" by mistake; corrected to **MAGICAL COFFEE damage**. The passive's **Poison-tagged 3s nonstacking DoT** has **MAGICAL CHANNEL + COFFEE ELEMENT**, ticks **ONCE EACH SECOND** and **NEVER SPREADS**. Donut remains an unrelated global element, NOT Acolyte's poison.
- **ACOLYTE-029 USER APPROVED:** **exactly ONE skeleton per cast, 4 Soul Charges and ZERO Mana**. Hero Auto-Retaliate OFF ⇒ only explicit attack orders; ON ⇒ retaliate when attacked. Cap4, timed15s, replace oldest remain confirmed.
- **ACOLYTE-030 USER DELEGATED:** create cooldown/Mana/damage/heal scaling proposals across six skills; designer numeric proposals below are **NOT yet user approved**, common Tenacity formula is a global unresolved decision.

## Acolyte first-playtest balance — CDs/Mana approved; raw effect coefficients proposed

**Cooldown and Mana cost columns NOW USER APPROVED as initial playtest values; raw damage/effect rank coefficients and unconfirmed Radiation tick timing below remain DESIGNER PROPOSALS, NOT implemented Unity gameplay.** `A`=caster current INT, `S`=caster current SPR, `r`=purchased skill rank; RAW effects shown before target mitigation/resistances and normal healing modifiers. **Coffee is Coffee, not Donut.**

| Skill | Approved max | **Approved initial cooldown** | **Approved initial Mana** | Mechanics and proposed coefficient |
| --- | ---: | ---: | ---: | --- |
| **Withering Curse** | 14 | **12s APPROVED** | **12 APPROVED** | Rank1 −5 MDEF/−0.10 attacks/s for 10s user-approved; higher-rank proposal: `−(5+r−1)` flat MDEF, `−(0.10+0.005×(r−1))` attacks/s, 10s. Global boss Tenacity equation OPEN |
| **Radiation Plague** | 16 | **10s APPROVED** | **15 APPROVED** | **Duration 3s rank1 to 8s rank16 CONFIRMED**. Proposed whole-second ranks:1–3 3s,4–6 4s,7–9 5s,10–12 6s,13–15 7s,16 8s; **once/sec Magical Radiation ticks and RAW `(2+0.10×A)×(1+0.05×(r−1))` PROPOSED**, not user-signed. Contagion after continuous 2s within3m, reset on separation, no backtracking; death +1 Soul |
| **Summon Skeleton** | 12 | **3s APPROVED** | **0 Mana +4 Soul APPROVED** | One skeleton15s/cap4/replace oldest. **Ranks improve HP and BASIC ATTACK damage only**; raw HP `55+2×VIT`, Physical basic damage `3+0.18×INT`, +5% HP/+6% basic damage per rank are numeric DESIGNER proposals |
| **Drain Life** | 20 | **14s APPROVED** | **24 APPROVED** | 5s Void channel with once/sec ticks; RAW `(4+0.20×A+0.10×S)×(1+0.06×(r−1))` PROPOSED. **+1 Soul on resolved tick even if fully shielded**, shield damaged normally, heal only `(25%+2pp×(r−1))` of real enemy HP loss, cap63%; **fully blocked =0 heal** |
| **Mortal Coil** | 10 | **7s APPROVED** | **14 APPROVED** | Hostile Void Magical RAW `(10+0.60×A+0.30×S)×(1+0.08×(r−1))` and allied/self/skeleton Void healing `(10+0.25×A+0.70×S)×(1+0.08×(r−1))` still numeric PROPOSALS; normal elemental Heal/resist applies, enemy +1 Soul, healing +0 |
| **Occult Adept** (working passive name) | 8 | Passive | 0 | **APPROVED +2 INT/+1 SPR/rank**; every wand basic hit applies/refreshed one **Poison-tag** 3s nonspreading DoT with **MAGICAL COFFEE element** ticking once/sec. Proposed per-tick RAW `1+0.06×A+0.35×(r−1)`, independent of Radiation spread |

### Balancing / interaction cautions

- **Coffee** is the explicitly user-corrected ELEMENT; `Poison` is the semantic STATUS TAG. No new Poison element and no accidental reuse of Donut element. Coffee registry membership is **not yet verified** against live DiceBound; if absent, explicitly add a DiceFree element entry when implementation is authorized.
- **Void heal:** Mortal Coil allies/self/skeletons are healed with Void element using recipient resistance; Drain Life is different—heal conversion only from actual enemy HP damage, no shield-healing and no accidental double healing modifiers.
- **Skeletons cost 4 Soul ONLY, 0 Mana**, exactly one per cast; at cap4 new replaces oldest. No automatic unprovoked aggro when Auto-Retaliate ON; direct orders always work.
- **ACOLYTE-034 APPROVED**: Coffee Poison refresh sets duration back to3s but **CONTINUES already-running 1s tick clock**; never shifts/restarts tick timing, stacks or spreads.
- **ACOLYTE-035 APPROVED**: resolved Drain Life tick DAMAGES an eligible shield and generates **+1 Soul Charge even if 100% shielded**, but **0 actual enemy HP damage means 0 drain healing**. This is ordinary shield damage, not a shield bypass.
- **First-playtest cooldowns/Mana and Radiation 3s→8s duration endpoints are APPROVED**; raw damage/rank coefficients, Radiation intermediate progression/tick cadence and exact skeleton stat formulas remain DESIGNER PROPOSALS. Tenacity remains global; class design only, NO Unity changes.
- World1 scope remains Acolyte → Thief → Psion → Quillboar → city; Tier III waits World2.

## Round 7 — user decisions recorded 2026-10-20

- **ACOLYTE-031 USER APPROVED FIRST PLAYTEST COOLDOWN/MANA:** Curse12s/12 Mana, Radiation10s/15, Summon Skeleton3s/0 Mana +4 Soul Charges, Drain Life14s/24, Mortal Coil7s/14, passive no cooldown/no Mana.
- **ACOLYTE-032 USER CONFIRMED:** Radiation Plague duration **3 seconds at rank1** increasing to **8 seconds at rank16**. Intermediate duration interpolation and Radiation's once-per-second tick frequency were NOT explicitly answered; those remain DESIGNER suggestions.
- **ACOLYTE-033 USER CONFIRMED:** Skeleton skill ranks increase summoned HP and basic attack damage, without extending approved15s lifespan or increasing max4 summon limit or 4-Soul cost. Explicit approval of proposed numeric HP/attack coefficients remains pending.
- **ACOLYTE-034 USER CONFIRMED:** Coffee-element Poison-tagged 3s DoT refreshes its duration on another successful wand basic attack but its existing 1-second periodic tick TIMER CONTINUES, is not reset; DoT remains nonstacking and nonspreading.
- **ACOLYTE-035 USER CONFIRMED:** every successfully resolved Drain Life damage tick DAMAGES enemy shields and grants +1 Soul Charge (called a "soul shard" colloquially in answer), INCLUDING if shield blocks all HP damage; drain healing remains based solely on ACTUAL HP damage and therefore ZERO if fully absorbed. Prior resource name remains **Soul Charges** until user explicitly renames it.

## Round 8 — user approvals recorded 2026-10-20; interview finished

**ACOLYTE-036–040 all APPROVED**, fully specified in the authoritative first section of this file. No further Acolyte interview questions; next Thief Tier II. No Unity changes.

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
| 2026-10-19 | ACOLYTE-016 | Curse rank1 −5 flat MDEF and −0.10 attacks/s, 10s; boss attenuation via common Tenacity | **CONFIRMED; rank curve and Tenacity formula OPEN** |
| 2026-10-19 | ACOLYTE-017 | Radiation 3m spread, 2s uninterrupted contact reset on separation, chained spreads/no backtracking; recast refreshes | **CONFIRMED** |
| 2026-10-19 | ACOLYTE-018 | Cap4 skeletons, newly summoned replaces oldest, auto-retaliate mirrors hero toggle | **CONFIRMED** |
| 2026-10-19 | ACOLYTE-019 | Drain Life heal25% at rank1 +2pp/rank on actual HP damage, target max~60–65% | **CONFIRMED baseline & growth; exact cap OPEN** |
| 2026-10-19 | ACOLYTE-020 | Six skills: original four + Mortal Coil (1s INT/SPR Void dmg or ally/summon heal) + INT/lower SPR/auto-poison passive | **CONFIRMED kit; mechanics open** |
| 2026-10-20 | ACOLYTE-021 | Self/ally/skeleton Mortal Coil heals with VOID ELEMENT | **CONFIRMED** |
| 2026-10-20 | ACOLYTE-022 | 1s normal channel cancel rules; successful hostile Mortal Coil +1 Soul, healing +0 | **CONFIRMED** |
| 2026-10-20 | ACOLYTE-023 | Each successful wand auto causes 3s refreshing nonstacking Poison DoT | **CONFIRMED** |
| 2026-10-20 | ACOLYTE-024 | Passive +2 INT/+1 SPR per rank; Poison damage scales with INT and passive rank | **CONFIRMED** |
| 2026-10-20 | ACOLYTE-025 | 80-rank schedule creation delegated; 14/16/12/20/10/8 proposal audited | **DELEGATED; EXACT GATES AWAIT USER APPROVAL** |
| 2026-10-20 | ACOLYTE-026 | Exact 14/16/12/20/10/8 rank gates/caps, 63% final Drain Life healing | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-027 | Mortal Coil INT-heavy damage, SPR-heavy Void healing, both INT+SPR and rank scaling | **CONFIRMED; exact numbers PROPOSED** |
| 2026-10-20 | ACOLYTE-028 | Final user correction: **MAGICAL COFFEE damage**, Poison-tag 3s, 1 tick/sec, nonstacking nonspreading | **USER APPROVED COFFEE; DONUT TEXT SUPERSEDED** |
| 2026-10-20 | ACOLYTE-029 | One skeleton/cast, 4 Soul only, 0 Mana, hero auto-retaliate OFF orders only ON fight back | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-030 | First-pass full six-skill CD/Mana/coefficient balance delegated | **PROPOSED NUMBERS; NOT USER CONFIRMED** |
| 2026-10-20 | ACOLYTE-031 | Initial playtest CDs/Mana Curse12/12, Radiation10/15, Skeleton3/0+4Soul, Drain14/24, Coil7/14 | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-032 | Radiation duration 3s rank1 → 8s rank16 | **CONFIRMED; intermediate rank curve and ticks OPEN** |
| 2026-10-20 | ACOLYTE-033 | Summon rank increases skeleton HP/basic attack, not count/lifetime/cost | **CONFIRMED; numeric coefficients still DESIGNER-proposed** |
| 2026-10-20 | ACOLYTE-034 | Coffee Poison refresh preserves tick clock while renewing3s duration | **USER CONFIRMED** |
| 2026-10-20 | ACOLYTE-035 | Shielded Drain Life tick damages shield and yields +1 Soul; blocked HP damage heals0 | **USER CONFIRMED** |
| 2026-10-20 | ACOLYTE-036 | Radiation +1s duration at ranks4/7/10/13/16 | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-037 | Radiation transmits source remaining time and ticks 0.1s with each tick÷10 raw damage | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-038 | Skeleton HP scales with SPR not VIT, attack INT, +5%/+6% per rank, owner stats snapshot | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-039 | Flat Curse growth MDEF −1, rate −0.005 attacks/s per skill rank | **USER APPROVED** |
| 2026-10-20 | ACOLYTE-040 | All initial raw damage/heal coefficients approved with Radiation/skeleton corrections | **USER APPROVED INITIAL PLAYTEST** |
| 2026-10-20 | ACOLYTE-CLOSED | Acolyte design complete; user switches to Thief | **COMPLETE DESIGN-ONLY** |

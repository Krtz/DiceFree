# DiceFree — Acolyte Tier II — design interview

**Created:** 2026-10-19  
**Status:** WORLD 1 STANDARD TIER II CLASS. **ROUNDS 1–5 USER DECISIONS RECORDED THROUGH 2026-10-20**. Six-skill Tier II kit now consists of Curse (rank1 −5 MDEF, −0.10 attacks/s for 10s; shared boss Tenacity attenuation), Radiation DoT (3m/2s continuous contact, timer resets, chain spreads, new manual casts refresh), player-controlled 15s skeleton summons (cap4, new summon replaces oldest, idle aggression follows hero Auto-Retaliate), 5s Void Drain Life (heals 25% of actual HP damage at rank1, +2 percentage points per rank; 60–65% vicinity suggested cap), NEW 1s Void Mortal Coil (hostile INT+SPR damage, ally/owned-summon healing), and NEW INT+small SPR/poison-on-basic passive. Mana/Soul Charge cap8/cost4 skeleton, +1 per drain tick and affected target death. Round5 CONFIRMS Void-element Mortal Coil healing on self/allies/skeletons, standard one-second channel interruption, enemy-hit +1 Soul Charge and healing +0, and +2 INT/+1 SPR per passive rank with nonstacking three-second poison refreshed on each successful wand basic attack. The USER-DELEGATED 80-rank schedule is PROPOSED and needs separate approval. Mortal Coil coefficients, poison damage, shared Tenacity formula and quest details remain open. **DESIGN/DOCUMENTATION ONLY; NO UNITY IMPLEMENTATION.**  
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
| Pillar 2 | **Weak Radiation DoT** spreads via **3m proximity for 2 uninterrupted seconds**; contact timer RESETS on separation. Newly infected enemies can further spread it, with visited-target history blocking A→B→A and other re-infections in a chain. **Manual re-cast on affected target refreshes duration**. +1 Soul Charge when afflicted enemy dies. | Initial duration/tick damage/tick rate, status refresh detail and spread between different Acolytes OPEN |
| Pillar 3 | **15-second, directly controllable skeletons**, max4 active; new skeleton cast at cap4 **REPLACES OLDEST** (not blocked). Skeletons follow hero's **Auto-Retaliate setting** during idle/unscripted behavior and obey direct RTS orders. **4 Soul Charges per skeleton**. | Selection and command priority, per-cast spawned count, HP/damage, Mana, lifetimes on hero swap/death OPEN |
| Pillar 4 | **Drain Life** 5s Void channel, one damage tick/s, **+1 Soul Charge per successful tick**. Heals **25% of actual enemy HP damage at rank1**, increasing **+2 percentage points per rank**, toward a provisional **~60–65% ceiling** pending rank cap. No healing from shield-absorbed damage. | Max skill rank and whether 60 or 65% exact cap, damage coefficients, Mana/cooldown OPEN |
| Pillar 5 | **Mortal Coil** one-second channel: hostile target takes Void Magical damage scaling INT+SPR; **self/other allied heroes/owned skeletons instead receive VOID-ELEMENT HEALING**. Moving, stun or explicit interrupts cancel, ordinary damage does not. **Enemy damage +1 Soul Charge, healing +0**. | Healing/damage coefficient, Mana/CD, rank scaling, target validity and Void-resistance special cases OPEN |
| Pillar 6 | **Poison passive** grants **+2 INT and +1 SPR per rank**. Every successful wand basic attack applies or refreshes **one small nonstacking three-second Poison-tag DoT**, damage scaling with INT/passive rank. | Tick frequency, exact damage, elemental classification and proposed skill rank cap OPEN |
| Resources | **Mana + Soul Charges CAP8**, cost **4 per skeleton summon**. **+1 for each successful Drain Life damage tick** and **+1 on death of enemy currently afflicted with caster's Radiation DoT**. **Applying Curse or Radiation DoT gives 0 charges**; their ordinary ticks are NOT automatic charge generators. | Whether other *future direct offensive spells* earn charges (must be explicitly authored), charge decay/persistence, simultaneous death attribution OPEN |
| Equipment | **Wand mainhand + grimoire offhand**, both ordinary authored gear/stat/intrinsic carriers | Authorable drops/Intrinsics, class weapon permissions, exact item stats; **no unique grimoire charges/pages/inscriptions system** |
| Advancement | **Graveyard caretaker** concept: collect dead from **Cornberg and Trimery** for proper burial at graveyard | Exact cemetery site, NPC, remains locations/counters, story presentation and trial scripts intentionally DEFERRED |
| Future rank economy | Six skills begin rank0, earn one skill point/class level; user delegated an 80-rank gate schedule. **Designer proposes Curse14 / Radiation16 / Skeleton12 / Drain20 / Mortal Coil10 / Passive8**, with all 80 integer class levels audited. | Exact cap/gates await user numeric approval; no code changes |

**Six-skill kit CONFIRMED as of Round 4:** Curse, distinct Radiation DoT, skeleton Summon, channeled Drain Life, **Mortal Coil**, and an **INT/SPR + poison-on-autoattack passive**. Earlier hypothetical Occult Bolt and Forbidden Knowledge are SUPERSEDED and should not be added as extra abilities. Soul Charge generation via enemy-directed Mortal Coil remains OPEN; no automatic yield on its friendly healing use. Spell ranks and class levels remain separate from weapon upgrades.

### Summon framework compatibility (not yet summon-specific approval)

DiceFree's shared `docs/COMBAT.md` and `docs/SUMMONS_AND_COMPANIONS.md` summon framework already supports class-authored summon quantity/lifetime, autonomous or player-controlled behavior, summon threat, owner attribution and owner-level scaling. There is **NO global summon cap**, so Acolyte needs its **own authored cap and cost** once the user decides. Do **not** assume it inherits any DiceBound Necromancer rules or its three-summon maximum. Acolyte's exact summons, whether temporary or sustained and behavior on owner death remain OPEN.

### Current SIX Tier II skills — Round 4, confirmed core mechanics

1. **Withering Curse** (working name) — at **rank1**, lower target **Magical Defense by flat 5** and **attack rate by flat 0.10 attacks per second** for **10 seconds**. It is NOT −5% MDEF/−10% attack speed. Bosses experience a reduced effect using the **shared/global Tenacity rule**, not a unique Acolyte boss modifier; actual formula for Tenacity and attack-rate clamping remain OPEN. Rank1 amounts/duration are approved; rank growth and stacking/recast refresh are not yet chosen.
2. **Radiation Plague** (working name) — a **relatively weak Magical Radiation DoT**. An uninfected enemy within **3m** of an infected enemy for **2 CONTINUOUS seconds** acquires the DoT. The two-second contact timer **RESETS if enemies move farther than 3m** before completion. Secondary infected enemies can pass onward (A→B→C), but this infection chain tracks visited recipient IDs so **no enemy in its own propagation lineage is re-infected by spread** (A→B→A and cycles disallowed). The Acolyte may deliberately **recast on the same enemy to REFRESH** the DoT duration; this is a new player cast and does not mean proximity bounce-back is allowed. **Only death WHILE afflicted yields +1 Soul Charge**, not application or ordinary periodic ticks; casting Curse grants zero. **OPEN:** initial dot duration, tick cadence/damage, multiple nearby sources, infection chain identity on manual reapply, cross-caster status behavior.
3. **Summon Skeleton** (working name) — directly **RTS player-controlled**, **one skeleton per cast as a provisional implementation assumption** (user did NOT choose number per cast explicitly), per-skeleton **15-second duration**, cap **4 active** per Acolyte, Soul Charges **cost 4 each**. When another skeleton is summoned at cap, **REPLACE/DESPAWN THE OLDEST first** and create the newest (resources spent on the accepted summon). Skeletons' spontaneous fight-back behavior **mirrors the HERO'S AUTO-RETALIATE TOGGLE**, rather than always aggressive: if ON they may respond to enemy aggression, if OFF they obey direct orders without automatic retaliation. **Explicit player commands always have priority** and remain possible whichever toggle is selected. This rule is consistent with `docs/SUMMONS_AND_COMPANIONS.md` player-controllable/hybrid owned units; do not assume it creates a second independent auto-retaliate settings menu. **OPEN:** whether passive retaliation includes acquisition/autoattack of nearby targets that have not attacked (do not assume), AI threat targeting, skeleton stats, mana cost, animation, replacement expiry ordering for ties, handling hero death.
4. **Drain Life** (working name) — **5-second Magical VOID-element CHANNEL**, **one damage tick each second** (five if uninterrupted). A successful tick generates **+1 Soul Charge** capped at 8. Heal caster by **25% of actual target HP damage dealt at Drain Life rank1**, adding **+2 PERCENTAGE POINTS PER ADDITIONAL SKILL RANK** (so rank6 → 35%). The accepted earlier initial **~60–65% eventual healing cap** is a **playtest TARGET RANGE, not an exact specified cap or final skill max rank**; choose rank cap and rounding consistently later. If target shields absorb damage, that blocked amount causes **ZERO healing** (and no extra independent lifesteal). Moving/stun/explicit interruption cancels remaining channel ticks; ordinary incoming hits do not. **OPEN:** exact end cap, damage coefficient, what a "successful" zero-HP-damage tick means for Soul Charge award, Mana/cooldown/attack range.
5. **Mortal Coil** — user-suggested name. One-second channel, **normal move/stun/explicit-interrupt cancellation**, ordinary damage alone does not cancel. Against an enemy: **Void-element Magical damage** scaling with **INT and SPR**; successful damaging resolution **awards +1 Soul Charge**. Against **the Acolyte themself, an allied hero or an owned skeleton**, instead delivers **VOID-ELEMENT HEALING**, and **generates zero Soul Charges**. Void HEALING uses the target's Void resistance as an elemental healing modifier per `docs/STATS_AND_DAMAGE.md`; this is NOT a normal Magical damage instance. Exact raw healing/damage coefficients, Mana/CD and target-invalid behavior are OPEN.
6. **Passive — Occult Adept** (WORKING NAME only): each rank gives **+2 INT and +1 SPR**. Every successful Acolyte **wand BASIC attack** reliably applies or **refreshes ONE Poison-tagged DoT lasting three seconds**; no RNG proc gate, no stacks/second instance on repeated hits. Poison damage scales with **INT and passive skill rank**, without automatically granting Soul Charges or copying Radiation plague's proximity spread. `Poison` is a semantic status tag from `docs/COMBAT.md`, NOT necessarily a damage element; exact elemental type and per-tick raw damage/tick cadence remain OPEN. Proposed cap8 means +16 INT/+8 SPR, but cap not yet numeric-user-approved.

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

## Proposed Acolyte 80-rank class progression — DESIGNER PROPOSAL, NOT YET APPROVED

All 6 skills start at **rank0**. The class receives one spendable rank point at character class level1, then +1/class level until 80. Rank purchase requires all prior ranks of that same skill, and a minimum class level for the next individual rank. The following caps, gates and intervals are **designer-chosen**, not yet separately approved by the user.

| Skill | Max rank (proposed) | First gate | Interval | Last gate | Every level gate |
| --- | ---: | ---: | --- | ---: | --- |
| Withering Curse | 14 | 1 | Every 5 levels | 66 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66 |
| Radiation Plague | 16 | 1 | Every 5 levels | 76 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76 |
| Summon Skeleton | 12 | 1 | Every 6 levels | 67 | 1, 7, 13, 19, 25, 31, 37, 43, 49, 55, 61, 67 |
| Drain Life | 20 | 1 | Every 4 levels | 77 | 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69, 73, 77 |
| Mortal Coil | 10 | 6 | Every 7 levels | 69 | 6, 13, 20, 27, 34, 41, 48, 55, 62, 69 |
| Occult Adept (working name) | 8 | 1 | Every 10 levels | 71 | 1, 11, 21, 31, 41, 51, 61, 71 |
| **TOTAL** | **80** | — | — | **77** | Full 80-rank skill capacity eligible by class level77 |

**Drain Life 20-rank fit:** the USER-APPROVED healing ratio is `25% + 2 percentage points × (skillRank−1)`. This designer-proposed 20-rank cap would put the final healing conversion at `25% + 19×2%=63%` of actual HP damage dealt, inside the previously accepted approximate 60–65% cap direction. This **63% is a consequence of the proposed cap**, not yet a separate user decision. With proposed passive max8, its totals would be +16 INT/+8 SPR.

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
- Passive Poison is a **separate nonstacking three-second DoT** with `Poison` semantic STATUS tag, **not automatically a new Poison element**, not spreading by Radiation chain, no extra stacks even when repeated wand autos hit.
- Rank1 poison damage, tick interval, whether it is non-elemental Magical or existing element, Mortal Coil healing raw coefficient, mana/CD/damage rank formulas and final Curse Tenacity formula are OPEN.
- Grimoire stays ordinary stat item; future items/intrinsics can alter spells across all classes. Graveyard story quest from Cornberg+Trimery remains an idea reserved for region/city design. **No C#/prefab/Unity changes from this work.**

## Round 6 — five questions (AWAITING USER)

**ACOLYTE-026 — Approve Acolyte rank schedule:** Shall we approve the designer-proposed **Curse14 / Radiation16 / Skeleton12 / Drain Life20 / Mortal Coil10 / Passive8 = 80**, with the verified unlock intervals shown above? Drain Life max20 then heals **63% of actual HP damage**, and passive max8 grants +16 INT/+8 SPR.

**ACOLYTE-027 — Mortal Coil scaling:** Should BOTH enemy Void damage and friendly Void healing scale from **INT and SPR**, but enemy damage emphasize **INT** and healing emphasize **SPR**, with the raw coefficients improving with Mortal Coil skill rank? I'll keep exact first-pass coefficients for balancing unless you prefer specific numbers.

**ACOLYTE-028 — Poison type and tick schedule:** Should the guaranteed nonstacking **3s Poison DoT** tick **once per second (3 ticks)** and deal **non-elemental Magical damage** with Poison status tag, or should its damage use an existing element like Radiation while remaining independent from Radiation Plague's contagious DoT?

**ACOLYTE-029 — Skeleton casting and Auto-Retaliate:** Should **one Summon Skeleton cast create ONE skeleton for 4 Soul Charges**, with no additional Mana cost, or should it also consume Mana? Should hero Auto-Retaliate OFF mean skeletons only attack on direct orders, with ON permitting retaliation to enemy attacks?

**ACOLYTE-030 — Next balance pass:** May I design initial **Mana costs, cooldowns, damaging/healing rank coefficients and full high-rank curves** for all six Acolyte skills as designer proposals, while keeping boss Tenacity's exact mitigation formula global and awaiting its own system agreement?

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
| 2026-10-20 | ACOLYTE-026–030 | Round 6 five follow-up questions | **AWAITING USER** |

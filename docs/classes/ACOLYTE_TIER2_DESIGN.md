# DiceFree — Acolyte Tier II — design interview

**Created:** 2026-10-19  
**Status:** WORLD 1 STANDARD TIER II CLASS. **ROUND 1 USER DECISIONS RECORDED 2026-10-19**; exact spell effects, summon behavior, resource numbers and advancement quest remain under design. **DESIGN/DOCUMENTATION ONLY; NO UNITY IMPLEMENTATION.**  
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
3. **CONFIRMED ACOLYTE-001/004:** Acolyte's fighting rhythm emphasizes **SLOW sustained damage + curses/debuffs + self-sustain** compared with burst/CC Mage; the initial four specific spell pillars are **a distinct curse, a distinct damage-over-time ability, a summon, and a channeled life drain**. Make it a hybrid solo-sustainable damage/debuffer rather than Mage clone or primary Priest-style healer. The user has not chosen exact elements, coefficients, skill names, summon type or number of skills.
4. **CONFIRMED ACOLYTE-003:** Acolyte uses **Mana PLUS Soul Charges**. **Successful offensive spells** generate Soul Charges; **summons consume Soul Charges**. Exactly which spell events (cast completions/hit events/DoT ticks/drain ticks/summon hits), max charge capacity, persistence/decay, Mana requirements to summon and Soul Charge costs are **NOT yet decided**. Soul Charges are Acolyte's own resource, **not a grimoire equipment mechanism**. Avoid inventing automatic charge gain from ticks that would cause abusive scaling.
5. **GLOBAL ABILITY/RANK ECONOMY:** all new Tier II skills start **rank 0**, the Tier II character starts level1 with **1 spendable skill point** and earns one per level until the total configured rank capacity. Recent Greatblade/Mage/Priest/Defender/Ranger designs target **~78–80 total class-skill ranks by class level80**, and skill-specific rank-level eligibility should permit a point purchase each class level. Exact Acolyte skills/max ranks/gates will be designed AFTER its combat kit is selected, not preselected.
6. **CONFIRMED ACOLYTE-005 STORY DIRECTION, LOCATION/OBJECTIVES NOT YET FINAL:** An NPC **at a graveyard** asks an eligible Magically Touched Novice to **go to Cornberg and Trimery, collect the bodies/remains of deceased people, return them to the graveyard for a proper burial**, and then advance to Acolyte. This is a quest CONCEPT, **not an approved final level layout, NPC location, body spawn list, quest counter, collection mechanics or dialogue**. User explicitly wants to return to quest design later, perhaps when shaping the major city. Do not invent graveyard location or move other mentors now.

## Confirmed Round 1 identity and proposed future kit framework

### User-approved combat identity

| Topic | Approved decision | Still open |
| --- | --- | --- |
| Role | **Sustained occult damage, curse-based debuffs and self-sustain**, slower ramp than Mage burst with some crowd control | Actual Curse effects, solo DPS, group debuff power, defense, healing cap and boss interactions |
| Pillar 1 | **A distinct CURSE spell** weakening the enemy | Which statistic/effect, uptime, number of affected enemies, element and rank growth |
| Pillar 2 | **A distinct DAMAGE-OVER-TIME spell** doing sustained damage | Single versus area targeting, ticks, duration, snapshot or dynamic scaling, element |
| Pillar 3 | **A SUMMON ability**, **consumes Soul Charges** | Summon type, role, ownership, lifespan, AI/control, number cap, cost, sharing/transfer effects |
| Pillar 4 | **CHANNELED LIFE DRAIN**, damage plus restoration to caster | Whether HP restored is a share of actual damage, tick cadence, channel interrupt conditions, range and Soul generation |
| Resources | **Mana + Soul Charges**; offensive spells build Soul Charges, summons spend them | Charge cap, qualifying actions (especially DoT/drain ticks), whether a summon also costs Mana, decay |
| Equipment | **Wand mainhand + grimoire offhand**, both ordinary authored gear/stat/intrinsic carriers | Authorable drops/Intrinsics, class weapon permissions, exact item stats; **no unique grimoire charges/pages/inscriptions system** |
| Advancement | **Graveyard caretaker** concept: collect dead from **Cornberg and Trimery** for proper burial at graveyard | Exact cemetery site, NPC, remains locations/counters, story presentation and trial scripts intentionally DEFERRED |
| Future rank economy | All new class skills **rank0**, one spendable skill point at class level1, aim ~78–80 purchasable total through level80 with individual rank gates | How many final skills, rank caps/unlock levels, damage formulas, costs and animations |

**Do not conflate Curse and DoT into one action by accident:** the user's first answer lists a curse AND a separate damage-over-time skill. The final kit may also need a direct offensive spell and/or passive to provide a purposeful Soul Charge building loop. These are NOT user-approved additional abilities yet.

### Summon framework compatibility (not yet summon-specific approval)

DiceFree's shared `docs/COMBAT.md` and `docs/SUMMONS_AND_COMPANIONS.md` summon framework already supports class-authored summon quantity/lifetime, autonomous or player-controlled behavior, summon threat, owner attribution and owner-level scaling. There is **NO global summon cap**, so Acolyte needs its **own authored cap and cost** once the user decides. Do **not** assume it inherits any DiceBound Necromancer rules or its three-summon maximum. Acolyte's exact summons, whether temporary or sustained and behavior on owner death remain OPEN.

### Designer suggestions ONLY, not user decisions

- Working curse name **Withering Hex**: a debuff that slightly reduces enemy damage and/or defenses, ideally useful on bosses even if hard CC-immune.
- Working DoT **Soul Rot**: target takes repeating Magical damage; source stats may snapshot when applied as permitted by existing periodic framework; do not auto-assign an element.
- Working summon **Restless Spirit**: spends Soul Charges to manifest a spectral ally; autonomous attacking with a moderate per-caster cap might be simplest, but neither lifespan, form nor cap has been approved.
- Working drain **Siphon Life**: stationary targeted beam/channel, with damage ticks restoring a proportion of the actual damage dealt. Interruptibility, ratio and Soul gain OPEN.
- Optional future fifth offensive skill **Occult Bolt** (Soul Charge builder), plus optional passive **Forbidden Knowledge**; the user only confirmed **four mechanical pillars**, not the final skill count, names or whether those two should exist.
- **No compulsory item-specific Grimoire UI, pages, skill-altering book inscriptions or book charges**. Future gear items across all classes may have their own effects/intrinsics.
- Damage **Physical/Magical channels and elements** are authored per spell; **no unapproved Death or Arcane element** inserted. Acolyte can use non-elemental Magical damage or a selected existing element (e.g., Radiation) only when agreed.
- Graveyard mentor's core fantasy of **bringing the deceased home for respectful burial** is recorded; exact quest objectives are left for later World 1 location design, rather than claiming the player collects loot from active living enemies.

## Round 1 — five user answers recorded 2026-10-19

- **ACOLYTE-001 CONFIRMED:** the class should have **one Curse, one distinct DoT, one Summon and one channeled Life Drain** as its basic intended four combat features. No final name/skill count/coefficients yet.
- **ACOLYTE-002 CONFIRMED OVERRIDE:** equipped **grimoire is a conventional OFFHAND STAT ITEM FOR NOW** (wand + book), rather than a mandatory special class page/charge/inscription system. Like ordinary equipment, later items may modify Acolyte skills through independently authored gear effects/intrinsics. The earlier v0.2 "must have meaningful active/passive grimoire mechanic" suggestion is **SUPERSEDED**.
- **ACOLYTE-003 CONFIRMED:** **Mana AND Soul Charges**; **offensive spells generate** Soul Charges, **summons cost/spend** Soul Charges. Charge gain semantics/cap/cost and possible DoT or life-drain tick farming to decide.
- **ACOLYTE-004 CONFIRMED:** **hybrid sustained damage + curse/debuff + self-sustain**, contrasting with Mage's burst and smaller CC toolkit. Acolyte should solo effectively without substituting for Priest primary healing.
- **ACOLYTE-005 CONFIRMED STORY CONCEPT, QUEST DESIGN DEFERRED:** **graveyard NPC** asks player to collect dead bodies/remains from **both Cornberg and Trimery** and bring them to cemetery for **proper burial**. No final NPC location, body targets, quest dialogue, stage rules or implementation approved; hold until World 1 region/city planning.

## Round 2 — five questions (AWAITING USER)

**ACOLYTE-006 — Curse:** What should the **single-target curse** actually do: **reduce enemy outgoing damage**, **reduce their defenses** so everyone hits harder, **reduce their movement/attack speed**, or a combination? My preference is a moderate **offensive/defensive weakening curse** which works on bosses even if they resist hard CC.

**ACOLYTE-007 — Damage-over-time:** Should the DoT be a **single-target stacking affliction** that the Acolyte maintains on a boss, or a **spreadable plague** that can jump between nearby enemies? Should it use an existing element like **Radiation**, or remain **non-elemental Magical damage** with a dark visual?

**ACOLYTE-008 — Summon:** What kind of entity does Soul Charge expenditure summon: a **skeleton**, **ghost/spirit**, or something stranger? Should it **attack on its own for a limited duration**, or remain until killed/dismissed? I'd lean toward temporary autonomous summons with a low class-specific active cap, but no exact cap is assumed.

**ACOLYTE-009 — Soul Charge economy:** How about an initial **0–10 Soul Charge meter**, gaining **+1 on each successful offensive spell cast/hit** rather than on every DoT or drain tick, with summons consuming **3 charges**? Or should DoT ticks and sustained life drain also slowly grant charges, potentially with safeguards to prevent infinite fast generation?

**ACOLYTE-010 — Life Drain:** Should the drain be a **stationary, interruptible 2–3 second channel** with repeated **damage-and-self-heal ticks**, restoring health according to **actual damage inflicted**? If so, should its channel stop on movement/stun like Ranger's Arrow Rain, and should it also generate Soul Charges as an offensive spell?

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
| 2026-10-19 | ACOLYTE-006–010 | Round 2 five follow-up questions | **AWAITING USER** |

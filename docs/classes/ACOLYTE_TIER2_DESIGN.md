# DiceFree — Acolyte Tier II — design interview

**Created:** 2026-10-19  
**Status:** WORLD 1 STANDARD TIER II CLASS. **ROUNDS 1–2 USER DECISIONS RECORDED 2026-10-19**; Curse reduces Magical Defense and attack speed; Radiation DoT is spreadable and awards charges on affected unit death; skeleton summon is controllable/timed; Soul Charges max8 and cost4/summon; channeled Drain Life is 5s/1tick per second with Void elemental damage, healing and Soul gains. Exact coefficients, summon caps/control details and graveyard quest remain under design. **DESIGN/DOCUMENTATION ONLY; NO UNITY IMPLEMENTATION.**  
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
| Pillar 1 | **One Curse reducing enemy Magical Defense AND Attack Speed.** Later classes can have other curses; no need to design later classes now. | Numeric reductions, duration, boss debuff behavior, stacking/recasts and rank scaling |
| Pillar 2 | **Weaker spreading RADIATION DoT**, separate from the curse. Unit death while afflicted with the caster's DoT generates Soul Charges. | Spread timing/radius/targets, tick/duration, snapshot, death reward number, attribution and cap |
| Pillar 3 | **Summons controllable skeletons**, each lasting for a **limited set duration**, spending **4 Soul Charges per summon**. Standard DiceFree direct RTS summon controls must work. | Duration, active skeleton cap, HP/damage, movement and orders, Mana requirement, replacement/dismissal |
| Pillar 4 | **Drain Life = five-second Void-element channel**, dealing **one damage tick per second** (up to five) and healing the caster based on damage inflicted. Each successful tick generates Soul Charges. Cancels on movement, stun or explicit interrupts, not ordinary damage alone. | Damage and heal per tick, actual-damage definition, charge amount/tick, target loss, interrupted Mana/CD |
| Resources | **Mana + Soul Charges, capped at 8**. Offensive spells generate Soul Charges; successful Drain Life ticks generate charges, and a Radiation-DoT-afflicted unit's death generates them. Each skeleton summon consumes **4** charges. | Exact charge yields, DoT death credit rules, charge persistence/decay, other spells' generators |
| Equipment | **Wand mainhand + grimoire offhand**, both ordinary authored gear/stat/intrinsic carriers | Authorable drops/Intrinsics, class weapon permissions, exact item stats; **no unique grimoire charges/pages/inscriptions system** |
| Advancement | **Graveyard caretaker** concept: collect dead from **Cornberg and Trimery** for proper burial at graveyard | Exact cemetery site, NPC, remains locations/counters, story presentation and trial scripts intentionally DEFERRED |
| Future rank economy | All new class skills **rank0**, one spendable skill point at class level1, aim ~78–80 purchasable total through level80 with individual rank gates | How many final skills, rank caps/unlock levels, damage formulas, costs and animations |

**Do not conflate Curse and DoT into one action by accident:** the user's first answer lists a curse AND a separate damage-over-time skill. The final kit may also need a direct offensive spell and/or passive to provide a purposeful Soul Charge building loop. These are NOT user-approved additional abilities yet.

### Summon framework compatibility (not yet summon-specific approval)

DiceFree's shared `docs/COMBAT.md` and `docs/SUMMONS_AND_COMPANIONS.md` summon framework already supports class-authored summon quantity/lifetime, autonomous or player-controlled behavior, summon threat, owner attribution and owner-level scaling. There is **NO global summon cap**, so Acolyte needs its **own authored cap and cost** once the user decides. Do **not** assume it inherits any DiceBound Necromancer rules or its three-summon maximum. Acolyte's exact summons, whether temporary or sustained and behavior on owner death remain OPEN.

### Current confirmed four abilities — updated Round 2

1. **Withering Curse (working name):** a *single-target curse* reducing the target's **Magical Defense AND Attack Speed**. Exact potency/duration, target count, boss modifiers and overlap behavior OPEN. Future classes may offer additional curses, but DO NOT design Tier III yet.
2. **Radiation Plague (working name):** a **weaker Radiation-element damage-over-time** spell that **spreads to other enemies** according to future authored rules. An enemy dying **while carrying this Acolyte's DoT** triggers **Soul Charge generation** for that Acolyte (amount/attribution TBD). Mere damage ticks do NOT produce Soul Charges by default unless later approved; DoT death is the specifically approved event. Its periodic damage uses the game's standard authored DoT snapshot/dynamic options; exact settings TBD.
3. **Summon Skeleton (working name):** **directly controllable, temporary skeleton** spawned for **4 Soul Charges** (max resource8). Must use reusable owned-unit RTS control (see `docs/SUMMONS_AND_COMPANIONS.md`), not a background VFX-only autonomous entity. **Actual lifetime, cap, units per cast, HP/damage and detailed commands are OPEN**. Must distinguish user-required player controllability from whether skeleton also attacks automatically while idle.
4. **Drain Life (working name):** **VOID-element Magical damage** channel lasting **5 seconds**, doing **one damage tick each second** (five if uninterrupted, proposed at t1/2/3/4/5s) and healing caster according to damage ACTUALLY dealt. **A successful tick also generates Soul Charges**, exact yield open. **Movement, STUN, or explicit INTERRUPT before completion cancels remaining ticks**; normal incoming noninterrupting damage alone doesn't cancel. Channel tick damage, percent returned as health, target disappearance, line-of-sight and whether original channel consumes only Mana remain open.

**Resource economy — ACOLYTE-009:** Soul Charges capacity **8**, skeleton costs **4**. Acolyte has **Mana AND Soul Charges**. An offensive cast can generate charges (source events/rates open); life-drain tick generation and Radiation-DoT-afflicted target-death generation are specifically CONFIRMED. Charges should be attributable to owning Acolyte on co-op enemy death; repeated spread/death chains must obey resource cap. No grimoire-specific charge system.

**Void element — ACOLYTE-010:** User chose the explicit name **Void** as the damage element for Drain Life. Current DiceFree `docs/ELEMENTS.md` contains example element names but did **not explicitly list Void** before this request. Treat this as a **newly approved DiceFree Acolyte element identity**, requiring a stable registry entry and resistance/VFX hooks on implementation; **do not claim Void already exists in live DiceBound registry** or invent a default Void proc/CC effect. Damage channel can be **Magical** and element **Void**, in keeping with independent channel/element framework.

**Grimoire:** standard wand + ordinary stat-bearing book offhand as confirmed in Round1, no special book spell/modification slot now. The player's distinct soul resource is inherent to Acolyte, not the equipment.

**Earlier brainstorming superseded:** the old suggestions of a ghost/spirit summon or purely autonomous low-cap pet, short 2–3s life drain, generic damage/debuff curse, unassigned DoT element or fixed book mechanic are NOT the current user-approved direction.

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

## Round 3 — five questions (AWAITING USER)

**ACOLYTE-011 — Curse strength and duration:** How about **10 seconds** of **−10% Magical Defense and −10% attack speed** on the target at rank1, improving carefully with skill ranks? Should bosses take the **full debuffs** or should we cap the attack-speed reduction on bosses to avoid trivializing tank damage?

**ACOLYTE-012 — Radiation DoT spread:** Should the plague **spread when an afflicted enemy dies** to up to **two nearby enemies within 4 meters**, or should it periodically jump while the original target is alive? I'd lean toward **death-triggered spread** since you're already rewarding death with Soul Charges, but that would make spread much rarer on solo bosses.

**ACOLYTE-013 — Skeleton controls, cap and lifetime:** Should we start with a limit of **two active skeletons** (two casts spend the complete 8-charge bar), each lasting **20 seconds**, and allowing **RTS selection/orders** plus default autoattacks? This is an Acolyte-specific cap suggestion, NOT a global summon restriction.

**ACOLYTE-014 — Soul Charge gain rates:** I suggest **+1 per successful Drain Life tick** (five across uninterrupted channel) and **+1 when an enemy dies with your Radiation DoT**, with charges capped at8. Should successfully landing the Curse and the initial Radiation DoT also generate **+1** each, or should only direct damage and the two approved triggers generate charges?

**ACOLYTE-015 — Drain Life healing:** Should the 5-second Void drain return **50% of actual HP damage dealt** as healing (for example 20 HP damage heals 10 HP), with normal movement/stun/interrupt cancellation? Should damage absorbed by shields grant **no healing**, so the Acolyte only siphons from real HP damage?

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
| 2026-10-19 | ACOLYTE-011–015 | Round 3 five follow-up questions | **AWAITING USER** |

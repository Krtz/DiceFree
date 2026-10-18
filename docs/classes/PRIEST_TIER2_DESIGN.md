# DiceFree — Tier II Priest — Design Interview

**Status:** DESIGN INTERVIEW IN PROGRESS — Rounds 1–2 recorded; Round 3 awaiting answers. **Design only: do not implement from this document until user sign-off.**  
**Started:** 2026-10-17  
**Repository branch:** `integration/world1-foundation`  
**Design owner:** User; interview and documentation in five-question rounds, then the other DiceFree chat can implement.

**Read these existing references before implementing anything:** [recent Priest proposal](../PRIEST_TIER2_PROPOSAL_2026_10_17.md), [Tier II core classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [early Tier II proposals v0.1](../TIER2_CORE_CLASSES_DESIGN_V01.md), [Trimery healer placeholder](../TRIMERY_HEALER_2026_10_17.md), [class progression](../CLASS_PROGRESSION.md), [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md), [Mage implemented specification](MAGE.md), [combat/stat framework](../STATS_AND_DAMAGE.md), and [flexible skills](../PLAIN_GEAR_AND_FLEXIBLE_SKILLS.md).

> This document is the forthcoming authoritative **Priest-specific** design as the user answers. Prior proposed skills/quests remain **PROPOSED** until approved here. Do not mistake an implemented NPC service for an implemented playable Tier II class or advancement quest.

## Workflow

1. Ask **exactly five questions per round** and wait for user answers.
2. After each round, update this document with question IDs, **CONFIRMED** user decisions, and separate **OPEN** mechanics. Never invent numbers or claim a proposal was approved.
3. Develop class role, exact skill roster (actives and passives), Mana/Grace generation and spending, stat scaling, equipment, rank progression and unlocks, all spell targeting/ranges/cooldowns/formulas, mentor quest, visual identity and verification acceptance criteria.
4. This conversation is **design/documentation only**. The implementation chat should consume a complete reviewed specification later; no Unity/C# gameplay code, scene, balance or NPC changes during this interview.
5. Preserve existing game rules and class-independent mechanics. No arbitrary universal four-active/one-passive cap, and **no Tier II ultimate** (the reserved ultimate is for Tier IV+).
6. A Tier I **level-30 Magically Touched Novice** can eventually advance to **Tier II Priest starting level 1** on a new persistent manifestation; retain the parent and its save. The Tier II progression threshold remains level **60**.

## Previously agreed foundations — confirmed from other class discussions

| Subject | Existing commitment | Notes |
| --- | --- | --- |
| Class name/tier | **Priest, Tier II**, advancing from **Magically Touched Novice** at its level-30 quest threshold. | Quest itself not yet approved/implemented. |
| Role | **CONFIRMED PRIEST-001:** general-purpose **hybrid healer** at Tier II, mixing direct healing, shields, periodic healing and meaningful damage; solo capable. Distinct specializations are for potential Tier III Priest advancements. | Not a narrowly specialized Tier II healer. Exact relative throughput open. |
| Weapons | **Staff** is the intended Priest weapon family. | Whether staff is strictly required, offhands, other weapons and initial gear remain open. |
| Attributes | **CONFIRMED PRIEST-005:** **SPR is Priest's primary attribute for both healing AND damage**. Exact SPR coefficients, INT secondary benefits, level-1 base and per-level growth are **OPEN**. | This explicitly replaces the older split suggestion of SPR healing / INT damage. |
| Resources | **Mana + Grace**. Every spell costs Mana even when Grace-empowered; Grace is a **passive with manual active use** that empowers the next spell. **Round 2:** one Grace point per successful spell, no decay, no saved Grace on fresh game/load, requires a full meter (about 10 spells at first rank, fewer at higher Grace ranks), consumes all Grace per empowered cast. | **OPEN CONFLICT:** Round 1 said healing earns slightly more Grace; Round 2 said 1 per successful spell. Confirm whether the newer rule replaces the earlier advantage. Exact rank thresholds and activation semantics still open. |
| Without Grace | Priest retains usable damage/healing spells paid for with Mana; cannot require Grace for its basic function. | Specific ranks/ability gating open. |
| Skill system | Current flexible spell/action system accepts variable mixed active/passive skill counts; no universal fixed kit shape. No Tier II ultimate. | Existing class system is implemented for Mage, but Priest remains design-only. |
| Mentor | The **Trimery healer** is intended as Priest trainer and future quest giver. | Current healer implementation is only a free HP-service placeholder. |
| Resurrection | Town healing, a resurrection stone and any future Priest **combat resurrection** are different systems; a resurrection spell is **not** automatically in Priest's starting kit. | Future resurrection direction: expensive, long cast, stacking sickness; requires explicit design. |

## Present in the game — separate from Priest class

As of `integration/world1-foundation` commit `5a74e1112` (2026-10-17), the world has an ivory/blue **Trimery Priest placeholder** close to the village green that offers **free HP treatment to living players** via interaction. It does not refill Mana/Grace, resurrect, clear statuses, start/complete advancement, or unlock Priest. See `docs/TRIMERY_HEALER_2026_10_17.md` for scoped validation. This NPC is a future mentor candidate, not the implemented Priest class.

## Confirmed Tier II Priest kit — updated through Round 2

**Four standard spells plus one hybrid passive/active Grace skill** — **CONFIRMED PRIEST-002**. Do **not** silently add Pilgrim's Step/dash, Judgment, Mend, Sanctuary, Shared Light or a Tier II ultimate. The proposed mechanics below are authoritative where labeled **CONFIRMED**; starting values, exact ranks, spell range, magnitudes and some empowerment decisions remain **OPEN**.

| Skill | Element | Confirmed behavior | Unresolved questions |
| --- | --- | --- | --- |
| **Holy Light** | **Light** | One-target heal on ally/self; Light damage on hostile. **Round 2:** start balancing with a **longer cooldown and strong SPR scaling**, to adjust in testing. **Grace empowerment adds a chain bounce** (not merely a raw-number bonus). | Cooldown/damage/heal, cast time, number and radius of bounces, whether subsequent targets can mix ally and enemy modes, bounce falloff and individual hit eligibility. |
| **Light Burst** | **Light** | **Select a character** (ally, self or enemy, subject to normal targeting eligibility) and **center one instant AoE burst on that character**, healing allies and damaging enemies in the area. **Grace empowerment changes the cast to THREE bursts, ONE SECOND apart**. | Radius, whether area stays at cast-start position or follows target between pulses, AoE target cap, whether pulses may hit same unit multiple times and resource/Grace generation per cast. |
| **Aegis** | **OPEN: Light, Nature or Ice** | **Elemental protective shield.** Round 2 explicitly **reopened its element selection** among Light/Nature/Ice. User expects elemental shields to be more efficient against matching damage and less efficient against mismatched damage. | Choose element, shield HP, exact matching/nonmatching efficiency, self/ally targeting, duration, cooldown, how Grace empowerment changes it. |
| **Unnamed periodic spell** | **Nature favored, not final** | Single-target spell: **Nature-themed damage-over-time (DoT) on an enemy** or **Nature-themed heal-over-time (HoT) on an ally** is the user's **new preferred direction**. | Final name/element, tick duration/frequency, stacking/refreshing, direct hit or pure periodic, Grace-empowered version and resistance/heal interaction. |
| **Grace** | **Passive with active activation** | Generate Grace from successful spells. **Round 2:** **one Grace point per successful spell**; meter requires **about ten spell casts at rank 1**, with fewer casts needed at higher ranks. **No decay**; **starts at zero when loading game** (no persistence across saves/game sessions). Manually use Grace when meter is **FULL** to empower the next spell; an empowered successful cast **consumes all Grace**. | Exact rank threshold table/cap, successful-spell definition (healing vs damage and overheal), previous healing-bonus conflict, empowerment cancel/failure behavior, mana charge for activating Grace itself and spell-by-spell empowerment benefits. |

### Holy Light: Grace chain behavior

**CONFIRMED PRIEST-006:**
- Keep Holy Light as one friendly/hostile-targeting Light-element spell, with normal SPR scaling.
- Initially balance it around a **meaningful/long cooldown and strong healing and damage scaling**; numerical values **are intentionally provisional**.
- **Grace adds chain bounces**. An empowered friendly-start cast should remain a legitimate heal and a hostile-start cast legitimate damage. Whether chains remain within one allegiance or can heal/damage mixed nearby units is **OPEN** and must be chosen by user. The number of bounces and falloff are also **OPEN**.
- The chain is one spell cast for Grace generation unless later explicitly changed.

### Light Burst: Grace triples the event

**CONFIRMED PRIEST-007:**
- Target an **individual character**, not the ground, to determine the initial AoE center.
- Normal mode delivers **one instant burst**, healing eligible friendly targets and damaging eligible enemies within range.
- Empowered by Grace, deliver **three pulses, one second apart** (at cast time, +1s and +2s unless user chooses an alternate timing). Three events are a **single empowered cast**.
- **OPEN**: lock AoE at the chosen initial location or follow a moving target; hits/heals per pulse; interruption/cancellation after first pulse; whether secondary pulses generate further Grace (anti-multiplication is recommended).

### Aegis and existing elemental-shield architecture

**CONFIRMED PRIEST-008 (intent):** This must be an **elemental shield**. The choice between **Light, Nature or Ice is unresolved**. The previously provisional Light element is **no longer locked**.

**Verified in `docs/STATS_AND_DAMAGE.md` sections "Shields and absorbs" and "Shield resolution order":** the DiceFree shared shield design supports per-element efficiency multipliers, total capacity, eligibility, expiration, priority and overflow, with specific shields normally resolving before generic shields and damage normally mitigated before shields. However, **there is no documented universal fixed numeric rule** such as matching 1 shield HP per damage versus 2 or 3 against mismatched elements.

The user's illustrative concept is compatible with the framework:
- Matching element: for example, **1 shield HP consumed to prevent 1 damage**.
- Nonmatching element: for example, **2 or 3 shield HP consumed to prevent 1 damage**.
- These are **examples, not approved Aegis numbers**. The shield must be able to stop both types of eligible damage at different efficiencies if so designed.
- **OPEN**: element choice, exact per-element costs, grace empowerment, shield stacking/refreshing and self/ally targeting.

**Important elemental-healing interaction:** If the periodic Nature HoT is defined as elemental healing, it inherits the global elemental-healing rule (matching target resistance can increase or reduce healing), not a new Priest-specific formula.

### Grace: generation, threshold, activation, reset

**CONFIRMED PRIEST-009/010 insofar as details below:**
- A **single skill** acts as both the Grace generation passive and the **manual empowerment action**.
- **Round 2:** **1 Grace per successful spell cast** (whether friendly or hostile); the desired **first-rank threshold is approximately 10 spells**, and higher Grace skill ranks reduce that number.
- **No natural decay**. Do not preserve current Grace points in saves; **every game load starts at 0 Grace**.
- You may manually activate Grace **only at full Grace** to empower the **next spell**. That empowered spell consumes **the whole filled meter**. Every actual Priest spell still consumes Mana, even when empowered.
- **Potential inconsistency to clarify:** Round 1 specified healing produces **a bit more Grace** than dealing damage, but Round 2 now specifies one Grace point per successful spell without distinguishing healing from damage. **Do not silently grant an extra point for healing**, and do not declare the earlier preference formally abandoned until user confirms.
- **Proposed protections, not new user decisions:** canceled/invalid targets should not spend Grace, damage/heal ticks and triple-pulse Light Burst should not multiply Grace earned per single cast, and effective overhealing shouldn't enable endless Grace farming. Handle Grace consumption on actual successful spell execution, not when starting a target selector.

### Possible Tier III directions — ideas only

The user has floated a future **Druid or Nature-focused healer** (stronger HoTs and natural magic) and a **Hierophant / Prophet or Light-focused healer**. These are **not approved class IDs or tree edges**; they are inspiration for keeping Tier II Priest versatile and mixing Light + potentially Nature magic. Other paths remain possible.

## Advancement concept — NOT APPROVED

Recent proposal **Hands of Mercy**: level-30 eligible Magically Touched speaks with the Trimery healer, accepts training, heals three distinct wounded lumberworkers using a temporary quest heal action, protects one recovering worker from a solo-friendly threat, and returns to the same mentor for explicit advancement to level-1 Priest. This is **only a proposed quest**; no implementation authorization. Keep already-completed actions from bypassing acceptance and avoid dependencies on a particular prior skill rank. Prior Mage/Greatblade trials are different.

## Round 1 — answers recorded on 2026-10-17

- **PRIEST-001 — CONFIRMED:** mixed general-purpose healing at Tier II, offering a taste of different healing techniques; future **Tier III** Priest branches can specialize later.
- **PRIEST-002 — CONFIRMED:** the four spell actives are **Holy Light** (heal ally/damage enemy single target), **Light Burst** (targeted AoE heal/damage), **Aegis** (Light-element protective shield) and a **single-target DoT-on-enemy / HoT-on-ally spell** (name not chosen). **Grace** is the fifth class skill and combines passive + active functionality. No dash selected.
- **PRIEST-003 — CONFIRMED:** both damage and healing generate Grace, healing slightly more; formula and decay are **OPEN**.
- **PRIEST-004 — CONFIRMED:** manually **activate Grace** to empower the next spell; effect/cost amount and timing **OPEN**.
- **PRIEST-005 — CONFIRMED:** **SPR is the main scaling attribute for both damage and healing**. User did not separately confirm staff equipment restrictions or the autoattack element in this answer; keep them **OPEN** rather than assuming.

## Round 2 — answers recorded on 2026-10-18

- **PRIEST-006 — CONFIRMED:** initially longer Holy Light cooldown and strong scaling (balance targets, not numerical locks). Grace adds a **chain bounce** as its special effect.
- **PRIEST-007 — CONFIRMED:** Light Burst AoE centers on a **targeted character**; normal cast is **one instant burst**, Grace version is **three bursts one second apart**.
- **PRIEST-008 — OPEN element; CONFIRMED directional preference:** Aegis should use **Light, Nature or Ice**, with elemental matching shield efficiency. Shared shield framework supports per-element rates, but **1:1 matching vs 2–3:1 mismatched** are *illustrative possibilities*, not existing universal locked values. Grace's Aegis effect not yet specified.
- **PRIEST-009 — PREFERRED, NOT FINAL:** Nature-themed dual DoT/HoT fits Tier II Priest and possible future Nature/Druid lineage; final name and element still open.
- **PRIEST-010 — CONFIRMED:** one Grace point per successful spell; about **ten successful spells** to fill at Grace rank 1, decreasing threshold with higher ranks; **no decay**; **reset Grace to zero on load**; only **fully charged** Grace can be activated; successful next empowered cast **consumes all Grace**. Conflict with earlier higher gain from healing still open.

## Round 3 — next five questions (awaiting answers)

**PRIEST-011 — Aegis element, shield ratios and targets.** Which element should **Aegis** use at Tier II: **Light, Nature or Ice**? Should it protect **self and allies**, and do you like the illustrative efficiency of **1 shield HP per 1 matching-element damage, 2 shield HP per 1 other-element damage**, or prefer **3:1** or a different scheme? What should **Grace-empowered Aegis** do differently?

**PRIEST-012 — Grace rank thresholds and healing bonus conflict.** Should Grace rank 1 need **10** successful casts, rank 2 need **9**, rank 3 need **8**, and so on, with some minimum requirement? Does the new **1 Grace per successful spell** replace the earlier "healing gives slightly more Grace," or should healing have another small advantage without raising generation above one per cast?

**PRIEST-013 — Holy Light's empowered chain.** When you empower Holy Light, how many **bounces** should it have, and should a chain **stay friendly if you start on an ally / hostile if on enemy**, or bounce between **both allies and enemies**, automatically healing or harming as appropriate? Should each bounce have **the same strength** or some falloff?

**PRIEST-014 — Light Burst's three pulses.** When its target **moves** after the initial cast, should the second/third pulse **follow that target**, or remain at the **original location**? Can the same ally/enemy be affected by all three pulses? Are you happy treating all three as **one cast for Grace generation**?

**PRIEST-015 — Nature HoT/DoT.** Should we lock the periodic spell's element to **Nature** now? What name would you like (for example *Renewal*, *Verdant Touch* or *Living Bloom*)? Should it tick for the same duration/rate on friends and enemies, and what extra effect should a **Grace-empowered** version produce?

## Remaining design work (not yet approved)

- Full active/passive roster and rank unlock schedule through Tier II level 60; exact skill effects, targeting, geometry, queues, interruptions, Mana and Grace costs, cooldowns, strengths and tooltip breakdowns.
- Stat/defense/HP/Mana/Grace baseline and per-level growth; solo damage pacing versus Mage/Greatblade; class equipment allowances.
- Explicit stacking and shield precedence, target eligibility, heal caps/overhealing, party roles, group AOE heal balance, boss damage and resurrection eligibility.
- Design and approval of the healer's actual advancement quest, tutorialization and art/animation style; keep existing free-healing NPC service functioning.
- Integration tests: class fork/save, class swapping, Mana+Grace persistence, solo questing, real 1–4 player triage, basic attack, canceled casts, overheal/exploit safeguards and long boss fights.

## Decision log

| Date | ID | Decision | State |
| --- | --- | --- | --- |
| 2026-10-17 | Existing Tier II direction | Priest, staff, Mana+Grace; active Mana costs, Grace enhances offense and healing; good solo DPS; Trimery healer mentor | **PREVIOUSLY AGREED**, details open |
| 2026-10-17 | PRIEST-001 | Mixed all-round Tier II healer; Tier III can specialize | **CONFIRMED** |
| 2026-10-17 | PRIEST-002 | Holy Light dual target, Light Burst targeted AoE, Aegis shield, dual target DoT/HoT, Grace passive/active | **CONFIRMED**, mechanical specifics open |
| 2026-10-17 | PRIEST-003 | Both heals and damage generate Grace; healing slightly more | **CONFIRMED**, gain/decay open |
| 2026-10-17 | PRIEST-004 | Grace manually activated to empower next spell | **CONFIRMED**, cost/bonus open |
| 2026-10-17 | PRIEST-005 | SPR primary for healing AND damage | **CONFIRMED**, weapon/basic attack not answered |
| 2026-10-18 | PRIEST-006 | Holy Light starts long-CD/strong SPR; Grace causes chain bounces | **CONFIRMED**, bounce rules open |
| 2026-10-18 | PRIEST-007 | Light Burst centered on chosen character; instant normally, three bursts 1s apart with Grace | **CONFIRMED**, pulse movement/targets open |
| 2026-10-18 | PRIEST-008 | Aegis element reopened to Light/Nature/Ice; elemental shield efficiency preference | **OPEN element/ratios**; system support verified |
| 2026-10-18 | PRIEST-009 | Nature-themed single-target DoT/HoT suggested; Druid/Nature and Hierophant/Prophet/Light later ideas | **PREFERRED**, not final |
| 2026-10-18 | PRIEST-010 | 1 Grace/successful spell; ~10 casts initially, fewer at higher ranks; no decay/persistence; full meter/consumes all | **CONFIRMED**, rank thresholds + healing bonus conflict open |
| 2026-10-18 | PRIEST-011–PRIEST-015 | Round 3 questions asked | **AWAITING USER** |


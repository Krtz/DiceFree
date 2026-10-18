# DiceFree — Tier II Priest — Design Interview

**Status:** DESIGN INTERVIEW IN PROGRESS — Rounds 1–3 recorded; Round 4 awaiting answers. **Design only: do not implement from this document until user sign-off.**  
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
| Resources | **Mana + Grace**; actual spells always cost Mana. **Grace generation:** 2 per successful heal without overheal, 1 for other successful heals, shields or damage casts. Full meter requires 10 Grace at rank 1 and 6 at max Grace rank; no decay; starts at 0 after loading; manually activate full meter to empower next spell, consuming all Grace. | Exact rank count and intermediate thresholds, precise overheal eligibility and canceled-cast behavior remain open. |
| Without Grace | Priest retains usable damage/healing spells paid for with Mana; cannot require Grace for its basic function. | Specific ranks/ability gating open. |
| Skill system | Current flexible spell/action system accepts variable mixed active/passive skill counts; no universal fixed kit shape. No Tier II ultimate. | Existing class system is implemented for Mage, but Priest remains design-only. |
| Mentor | The **Trimery healer** is intended as Priest trainer and future quest giver. | Current healer implementation is only a free HP-service placeholder. |
| Resurrection | Town healing, a resurrection stone and any future Priest **combat resurrection** are different systems; a resurrection spell is **not** automatically in Priest's starting kit. | Future resurrection direction: expensive, long cast, stacking sickness; requires explicit design. |

## Present in the game — separate from Priest class

As of `integration/world1-foundation` commit `5a74e1112` (2026-10-17), the world has an ivory/blue **Trimery Priest placeholder** close to the village green that offers **free HP treatment to living players** via interaction. It does not refill Mana/Grace, resurrect, clear statuses, start/complete advancement, or unlock Priest. See `docs/TRIMERY_HEALER_2026_10_17.md` for scoped validation. This NPC is a future mentor candidate, not the implemented Priest class.

## Confirmed Tier II Priest kit — updated through Round 3

**Confirmed roster:** four standard spells and one manually activated/passive Grace skill. No additional routine abilities or ultimate are approved. Priest scales **both healing and damage from SPR**.

| Ability | Element | CONFIRMED mechanics | OPEN balance/edge cases |
| --- | --- | --- | --- |
| **Holy Light** | Light | Single target: heal friendly/self, damage enemy. Strong scaling and meaningful longer cooldown as initial balance direction. With Grace: **one chain bounce per Holy Light skill rank**; can bounce between **allies and enemies**; every successive bounce is **10% less effective**. | Damage/healing, cooldown, jump range, repeat targets, 10% falloff mathematics (additive vs multiplicative). |
| **Light Burst** | Light | Instant AoE centered on a **targeted character**, heals friends and damages enemies. With Grace: **three bursts one second apart** which **follow that same character as it moves**; the same friend/enemy may be affected by all three. | Radius, exact effects, target death/teleport and interrupt handling. |
| **Aegis** | **Ice** | **Self/ally target** shield. Against matching Ice damage, consumes **0.5 shield HP per 1 prevented damage**. Against other damage, consumes **1.5 shield HP per 1 prevented damage**. With Grace: shielded target deals a **small amount of damage around themselves**. | Shield capacity/duration, refresh, Grace AoE as pulse or aura, damage element/radius/magnitude, mixed/untyped/Pure damage eligibility. |
| **Wild Growth** | **Nature** | Single-target **HoT on ally/self** or **DoT on enemy**, **identical base tick value, tick interval and duration**. With Grace: **double HoT duration** or **apply slow on DoT**. | Exact magnitude/cadence, slow value/duration, refresh/stack behavior, cooldown/cost. |
| **Grace** | Passive + manually activated skill | **2 Grace** for a successful heal without overhealing; **1 Grace** for another successful heal, shield or offensive spell. Rank 1 full meter requires **10 Grace**, max Grace rank full meter **6 Grace**. No decay; **reset to 0 on save/game load**. Full meter only can be activated to empower next spell; the empowered cast consumes all Grace and **still spends Mana**. | Grace rank count/intermediate thresholds, whether partly overhealing still earns 2 or only 1, other eligibility, activation duration/cancel behavior. |

### Holy Light: Grace bounce details (PRIEST-013)
- A Grace-empowered Holy Light makes **r additional bounces**, where **r is Holy Light's skill rank**, **not Priest character level**. Bounce destinations can be both friendly and hostile; each destination is healed or damaged according to allegiance.
- Each consecutive bounce is **10% less effective**. **OPEN** whether 100→90→80 or 100→90→81 and whether chains may revisit the same target. Jump selection/range remains open.
- The whole chain is **one spell cast for Grace generation**, not one per bounce.

### Light Burst: following three-pulse effect (PRIEST-014)
- Standard cast: instant heal/damage circle centered on the **selected character**.
- Grace cast: three pulses **at initial impact, +1 second, +2 seconds**, recomputing the AoE center around the **original moving target** at each pulse.
- A unit may be hit/healed by **all three** pulses. **Only the FIRST pulse can generate Grace**, awarding the usual **1 or 2 based on its qualifying outcome**. Pulses 2 and 3 are effects of Grace and **generate none**. Do not grant multiple extra Grace from multiple simultaneous targets.
- **OPEN:** target death/despawn, target line of sight after initial cast and cases with both effective healing and damaging in the same first pulse.

### Aegis: elemental shield efficiency (PRIEST-011)
- **Ice** is chosen, and Aegis can shield **self or ally**.
- For incoming damage after shared mitigation: **one point of Ice damage prevented consumes 0.5 shield HP**, while **one point of non-Ice damage prevented consumes 1.5 shield HP**.
- Example: an Aegis with 100 nominal shield HP could prevent 200 Ice damage or approximately 66.7 other damage (illustrative example only; **100 capacity is not a chosen actual shield value**).
- The shared framework in `docs/STATS_AND_DAMAGE.md` allows per-element efficiency, damage filters, absorb priorities, shield duration and overflow. These coefficients are **specific to Aegis**, not a universal global elemental-shield rule.
- Grace-empowered Aegis additionally causes the **shield recipient** to inflict **small nearby damage**. Its frequency (on-cast pulse vs continuing aura/on-hit) and element are **OPEN**.

### Wild Growth: dual Nature periodic (PRIEST-015)
- Both enemy **Nature DoT** and friendly/self **Nature HoT** use the **same baseline magnitude per tick, period and duration**.
- Standard resistance/healing rules may make *final* damage and healing differ even when base amounts match.
- Grace on HoT: **double lifetime**, normal tick interval. Grace on DoT: original duration plus **movement slow**, whose amount and duration are still **OPEN**.

### Grace: generation, rank milestones and reset (PRIEST-012/014)
- **2 points** from a successfully cast **heal that does not overheal**; **1 point** from all other **successful healing, shield and damage casts**. Round 1's preference for better healing generation is therefore satisfied.
- **OPEN:** whether any wasted overheal reduces 2 to 1 or any actual HP restored earns 2; full-health no-op heal being "successful"; whether shields count on cast or only effective protection. Don't make unconfirmed anti-spam policies into locked decisions.
- Full Grace at **skill rank 1 = 10 points**; full at **highest Grace skill rank = 6 points**. The **number of ranks has NOT been chosen**, so do **not** mistake "6 points at max rank" for "maximum rank 6".
- Grace **does not decay** and **does not persist through save/game loads**; start a fresh load at **zero**.
- Grace is a **manual active power** on the same skill as its passive generation. It is only usable when full, empowers the **next successful spell**, and the empowered cast consumes **all accumulated Grace**. All real spells still cost normal Mana.
- For empowered Light Burst, **only the original/first pulse generates Grace**. Repeated pulses may still damage/heal normally; they do not create more Grace. Further multi-hit/tick exploitation safeguards need explicit design before coding.
- **OPEN:** Grace rank progression and intermediate thresholds, how long armed state lasts, interruption/target cancel handling, self-heal and instant shield event generation, any Mana cost for pressing the Grace toggle.

### Possible Tier III branches — ideas only

A future **Nature/Druid HoT specialization** and **Light/Hierophant/Prophet specialization** have been discussed, but no Tier III branches or class IDs are approved yet.

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

## Round 3 — user decisions (2026-10-18)

- **PRIEST-011 CONFIRMED:** Ice Aegis self/ally shield; 0.5 shield HP per blocked Ice damage and 1.5 per blocked other damage. Grace shielded target deals small nearby AoE damage; precise behavior open.
- **PRIEST-012 CONFIRMED:** first Grace rank requires 10 points, max rank requires 6. Non-overhealing successful heals give 2 Grace, all other successful heals/shields/damage spells give 1.
- **PRIEST-013 CONFIRMED:** Grace Holy Light bounces once per Holy Light skill rank, can bounce between friends and foes with correct heal/damage per target, each successive bounce 10% less effective.
- **PRIEST-014 CONFIRMED:** Grace Light Burst's three bursts follow the selected character; units can be struck/treated by every pulse. Only the first pulse earns a single 1- or 2-point Grace grant.
- **PRIEST-015 CONFIRMED:** periodic spell called **Wild Growth**, **Nature**, identical base tick values/interval/duration for HoT or DoT; Grace doubles HoT duration or adds slow to DoT.

## Round 4 — next five questions (awaiting user)

**PRIEST-016 — Stats and stat growth.** We confirmed that **SPR is Priest's main stat for both healing and damage**. What should Priest's **starting VIT / STR / AGI / INT / SPR** and **growth per level** be? For comparison, Magically Touched Novice currently starts at **10 / 5 / 5 / 13 / 13** with growth **+1 / +0.5 / +0.5 / +2 / +2**; the Mage Tier II prototype starts **11 / 5 / 8 / 18 / 9**, with growth **+1.5 / +0.5 / +0.75 / +3 / +1.5**. Approximate priorities are fine if you prefer us to balance numbers later.

**PRIEST-017 — Grace rank thresholds and overhealing.** Do you like **five ranks** requiring **10 / 9 / 8 / 7 / 6 Grace**, or another progression? To earn **2 Grace**, does a heal need **zero overheal at all**, or is restoring **some actual HP** sufficient? Does casting on a full-HP ally still award the baseline 1 Grace?

**PRIEST-018 — Skill unlocks and ranks.** Which spells/passives are available from **Priest level 1**, and which unlock later across Tier II levels 1–60? Should rank-1 starting skills be free and later ranks earned via level-gated points like Greatblade/Mage? Any preferred rank caps?

**PRIEST-019 — Mana economy and cooldowns.** Should Priest have a **large Mana pool but expensive casts**, or **moderate spell costs with faster natural regeneration**? Should Holy Light have the **longer, stronger heal cooldown**, Light Burst the **longest AoE cooldown**, Aegis an intermediate defensive cooldown, and Wild Growth a more maintainable periodic cast? We can leave exact numerical balance provisional.

**PRIEST-020 — Staff/basic attack and Grace.** Should Priest commit to a **staff**, with an **SPR-scaling Light basic attack** used between spell cooldowns? Should **basic attacks generate Grace**, or should only the four actual spell casts do that?

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
| 2026-10-18 | PRIEST-011 | Ice Aegis 0.5 shield HP/Ice damage, 1.5 shield HP/non-Ice damage; self/ally; Grace nearby damage | **CONFIRMED**, AoE details open |
| 2026-10-18 | PRIEST-012 | Grace requires 10 at rank 1, 6 at max; 2 for non-overhealing heals, 1 other successful spells | **CONFIRMED**, rank/overheal details open |
| 2026-10-18 | PRIEST-013 | Holy Light's Grace bounce count equals ability rank; mixed allies/enemies; 10% less each bounce | **CONFIRMED**, exact falloff open |
| 2026-10-18 | PRIEST-014 | Triple Light Burst follows target; same unit affected thrice; original pulse alone generates 1 or 2 Grace | **CONFIRMED** |
| 2026-10-18 | PRIEST-015 | Wild Growth Nature DoT/HoT same base duration/interval/potency; Grace 2× HoT duration or slows DoT | **CONFIRMED**, numbers open |
| 2026-10-18 | PRIEST-016–PRIEST-020 | Round 4 questions asked | **AWAITING USER** |


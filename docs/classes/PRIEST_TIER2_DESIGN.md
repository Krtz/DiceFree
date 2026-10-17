# DiceFree — Tier II Priest — Design Interview

**Status:** DESIGN INTERVIEW IN PROGRESS — Round 1 approved direction recorded; Round 2 awaiting answers. **Design only: do not implement from this document until user sign-off.**  
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
| Resources | **Mana + Grace**. Every spell costs Mana even when Grace-empowered. **CONFIRMED PRIEST-003–004:** both effective damage and healing generate Grace (healing earns a bit more), and **Grace is an upgradeable passive with a manually activated use** that empowers the **next spell**, rather than triggering automatically. | Gain math, exact spending, cooldown/timer and spell-by-spell bonuses remain **OPEN**. |
| Without Grace | Priest retains usable damage/healing spells paid for with Mana; cannot require Grace for its basic function. | Specific ranks/ability gating open. |
| Skill system | Current flexible spell/action system accepts variable mixed active/passive skill counts; no universal fixed kit shape. No Tier II ultimate. | Existing class system is implemented for Mage, but Priest remains design-only. |
| Mentor | The **Trimery healer** is intended as Priest trainer and future quest giver. | Current healer implementation is only a free HP-service placeholder. |
| Resurrection | Town healing, a resurrection stone and any future Priest **combat resurrection** are different systems; a resurrection spell is **not** automatically in Priest's starting kit. | Future resurrection direction: expensive, long cast, stacking sickness; requires explicit design. |

## Present in the game — separate from Priest class

As of `integration/world1-foundation` commit `5a74e1112` (2026-10-17), the world has an ivory/blue **Trimery Priest placeholder** close to the village green that offers **free HP treatment to living players** via interaction. It does not refill Mana/Grace, resurrect, clear statuses, start/complete advancement, or unlock Priest. See `docs/TRIMERY_HEALER_2026_10_17.md` for scoped validation. This NPC is a future mentor candidate, not the implemented Priest class.

## Confirmed Tier II Priest kit — Round 1

**CONFIRMED PRIEST-002:** **Four standard active spells plus one hybrid passive/active Grace skill**, whose precise formulas, ranks, level unlocks and resource costs are still **OPEN**. This selection supersedes both earlier four-spell proposals. Do **not** silently add Pilgrim's Step, Judgment, a separate Mend, a different Sanctuary or separate Shared Light passive without a later user decision.

| Skill | Element | Confirmed effect | Still to decide |
| --- | --- | --- | --- |
| **Holy Light** | **Light** | **Single targeted spell:** cast on a friendly target to **heal**, or on an enemy to deal **Light-element damage**. | Cast time, range, cooldown, Mana cost, magnitude, interaction with Grace, self-portrait targeting. |
| **Light Burst** | **Light** | **Targeted area-of-effect** spell that **heals allies and deals Light-element damage to enemies**. | Ground/targeted anchor, radius, whether both factions affected simultaneously, cast/cooldown, falloff, Mana cost, Grace effect. |
| **Aegis** | **Light** | **Protective Light-element shield**. | Self/ally target eligibility, amount, damage filters, duration, shield priority, Mana cost, cooldown, Grace effect. |
| **Working name: periodic Light spell** | **UNDECIDED** (spell element not explicitly stated by user) | **Single-target dual-function spell:** applies a **damage-over-time (DoT)** effect to an enemy or a **heal-over-time (HoT)** effect to an ally. | Name, whether Light element, tick rate, duration, refresh/stacking, damage/heal, targeting, Mana and Grace effects. |
| **Grace** | **Resource-driven passive + manual active use** | **Only one skill** with both a **passive** role (generate Grace from healing and damage, higher gain for healing) and a **clickable active action** to **empower the next spell**. | Amount earned per action versus proportional throughput; decay, cap, spending, empowered bonus, how long it is queued, rank effects. |

**Terminology/targeting acceptance requirements:**
- Holy Light chooses heal or damage **based on valid target allegiance**, not randomly. Target-friendly/self and target-hostile modes must preserve shared target confirmation and ability restrictions.
- Light Burst is **targeted AoE**, not a fixed caster-centered Holy Nova unless the user explicitly changes the target type.
- The dual DoT/HoT spell is a single skill with different friendly/enemy outcomes, not two independent skill-point/rank slots.
- Aegis is a **Light-element protective shield**; damage channel and element eligibility are not implicitly decided by this wording. Use the shared shield priority/mitigation system when designed.
- The Grace skill is both a **passive** and an **active use**. Ordinary class/provider UI must make its passive description and active activation/armed state readable. Manual empowerment should never trigger automatically merely because Grace is high.
- These four spells and the manual Grace activation remain **Mana-costing active effects where applicable**, but whether pressing Grace itself costs Mana is **OPEN**; prior blanket "every active skill costs Mana" was intended for spells and must not inadvertently force a Mana cost on passive Grace activation without asking.
- **SPR scales both healing and spell damage.** No INT-only or healing SPR/damage INT split imported from Mage.
- Without Grace, offensive and healing spells must remain viable. No separate Priest resurrection spell was selected in this round.

## Mana and Grace design — Round 1 decisions

**Mana**
- Existing approved direction: **every actual Priest spell costs Mana**, including empowered versions; Grace empowerment does not replace or waive Mana costs.
- Mana pool, regen, per-spell costs and whether activating the Grace passive has any Mana cost are **OPEN**.

**Grace**
- **CONFIRMED PRIEST-003:** gained from **both healing and damaging**, with **healing generating somewhat more Grace** than damage. Neither the relative gain ratio nor whether gain is per effective cast or actual HP throughput has been decided.
- **CONFIRMED PRIEST-004:** the **Grace passive is also an active skill**; pressing/using it **arms an empowerment on the next spell**. This is deliberate user control, not automatic spending at a threshold.
- Earlier agreement preserved: Grace can empower **both damage and healing**; an empowered spell **still costs its normal Mana**.
- **OPEN:** Grace meter cap, gain/decay, talent ranks, cast frequency controls, how much Grace is consumed, whether the activated state times out, self-cast/target-cancel effects and separate empowerment effects for each spell.
- Safeguards proposed but not yet locked: no free Grace from pure overheal, no generation on missed/invalid/dummy actions, no recursive generation from a spell that just consumed Grace, and no per-target multiplication for one AoE cast. Confirm with user before implementation.

## Advancement concept — NOT APPROVED

Recent proposal **Hands of Mercy**: level-30 eligible Magically Touched speaks with the Trimery healer, accepts training, heals three distinct wounded lumberworkers using a temporary quest heal action, protects one recovering worker from a solo-friendly threat, and returns to the same mentor for explicit advancement to level-1 Priest. This is **only a proposed quest**; no implementation authorization. Keep already-completed actions from bypassing acceptance and avoid dependencies on a particular prior skill rank. Prior Mage/Greatblade trials are different.

## Round 1 — answers recorded on 2026-10-17

- **PRIEST-001 — CONFIRMED:** mixed general-purpose healing at Tier II, offering a taste of different healing techniques; future **Tier III** Priest branches can specialize later.
- **PRIEST-002 — CONFIRMED:** the four spell actives are **Holy Light** (heal ally/damage enemy single target), **Light Burst** (targeted AoE heal/damage), **Aegis** (Light-element protective shield) and a **single-target DoT-on-enemy / HoT-on-ally spell** (name not chosen). **Grace** is the fifth class skill and combines passive + active functionality. No dash selected.
- **PRIEST-003 — CONFIRMED:** both damage and healing generate Grace, healing slightly more; formula and decay are **OPEN**.
- **PRIEST-004 — CONFIRMED:** manually **activate Grace** to empower the next spell; effect/cost amount and timing **OPEN**.
- **PRIEST-005 — CONFIRMED:** **SPR is the main scaling attribute for both damage and healing**. User did not separately confirm staff equipment restrictions or the autoattack element in this answer; keep them **OPEN** rather than assuming.

## Round 2 — five questions (awaiting answers)

**PRIEST-006 — Holy Light.** For this **one-target ally heal / enemy Light damage** spell, do you want a **quick direct cast with a cooldown** or a **slower hard cast with a bigger effect**? Should an empowered Holy Light simply hit/heal harder, or additionally gain a special effect?

**PRIEST-007 — Light Burst.** Should you **target a point on the ground** to create a circle that **heals all allies and damages all enemies inside it at the same time**, or target a friendly/enemy unit and center the AoE on that target? Should it be an **instant burst** or a field lasting a few seconds?

**PRIEST-008 — Aegis.** Can Aegis target **yourself and allies**, and should it absorb **all ordinary incoming damage** or only selected damage types/elements? Should it be a fixed-absorb shield that expires, a percentage reduction, or a mix? What should Grace empowerment add to the shield?

**PRIEST-009 — The combined DoT/HoT.** What should we **call** the single-target enemy-DoT / ally-HoT spell, and do you want both modes to last the **same duration** and tick at the **same interval**, or to have different patterns? Should the damage and healing both be **Light-element** effects?

**PRIEST-010 — Grace's core numbers and activation.** Should Grace generation be **a fixed amount per successful cast** (healing earns more) or **scale with actual damage / HP restored**? Roughly how much Grace can you hold, and should it **decay outside combat**? When you actively press Grace, should it spend **all accumulated Grace** to scale up the next spell, or a **fixed portion/threshold**?

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
| 2026-10-17 | PRIEST-006–PRIEST-010 | Round 2 questions asked | **AWAITING USER** |


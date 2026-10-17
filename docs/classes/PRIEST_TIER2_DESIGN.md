# DiceFree — Tier II Priest — Design Interview

**Status:** DESIGN INTERVIEW IN PROGRESS — Round 1 questions pending. **Design only: do not implement from this document until user sign-off.**  
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
| Role | Primary capable **healer** that also has a **meaningful offensive rotation** and can level/quest **solo**. More healing-oriented than Mage, not a helpless heal-only party alt. | Specific spells and amount of damage not decided. |
| Weapons | **Staff** is the intended Priest weapon family. | Whether staff is strictly required, offhands, other weapons and initial gear remain open. |
| Attributes | Earlier concept: **SPR-focused healing with INT also relevant**; exact split/formulas and stats are **not locked**. | Do not assume Mage's INT-only offensive scaling applies to Priest. |
| Resources | **Two real resources: Mana and Grace**. **Every active spell consumes Mana**, including Grace-empowered casts. Grace can enhance **both offensive damage and healing**, not only healing. | Generation, max, decay, rank interactions and empowerment trigger not yet approved. |
| Without Grace | Priest retains usable damage/healing spells paid for with Mana; cannot require Grace for its basic function. | Specific ranks/ability gating open. |
| Skill system | Current flexible spell/action system accepts variable mixed active/passive skill counts; no universal fixed kit shape. No Tier II ultimate. | Existing class system is implemented for Mage, but Priest remains design-only. |
| Mentor | The **Trimery healer** is intended as Priest trainer and future quest giver. | Current healer implementation is only a free HP-service placeholder. |
| Resurrection | Town healing, a resurrection stone and any future Priest **combat resurrection** are different systems; a resurrection spell is **not** automatically in Priest's starting kit. | Future resurrection direction: expensive, long cast, stacking sickness; requires explicit design. |

## Present in the game — separate from Priest class

As of `integration/world1-foundation` commit `5a74e1112` (2026-10-17), the world has an ivory/blue **Trimery Priest placeholder** close to the village green that offers **free HP treatment to living players** via interaction. It does not refill Mana/Grace, resurrect, clear statuses, start/complete advancement, or unlock Priest. See `docs/TRIMERY_HEALER_2026_10_17.md` for scoped validation. This NPC is a future mentor candidate, not the implemented Priest class.

## Skill concepts from existing proposals — NOT approved

Two historical iterations should be reconciled with the user, not silently combined:

| Role | Early Tier II draft (v0.1/v0.2) | More recent Priest proposal (2026-10-17) | Status |
| --- | --- | --- | --- |
| Ranged Light damage | **Judgment** | **Holy Light** | **PROPOSED** |
| Targeted direct healing | **Mend** | **Targeted Heal** | **PROPOSED** |
| Allies healed + enemies damaged in an area | **Sanctuary** ground field | **Holy Nova** nearby burst | **PROPOSED**, targeting/geometry differs |
| Defensive/mobility action | **Aegis** ally/self absorb shield | **Pilgrim's Step** short directional dash | **PROPOSED**, distinct roles |
| Passive | **Shared Light** building Grace from effective aid/offense | Grace resource and empowerment mechanics, passive lineup not finalized | **OPEN** |

The more recent working kit includes **Holy Light + Targeted Heal + Holy Nova + short dash**. There is no reason the kit must have exactly four active abilities; shielding could remain a later skill or be omitted if the user prefers.

## Resource design — confirmed boundaries, open formulas

**Mana**
- Every active skill costs Mana, including its Grace-empowered version.
- Mana regeneration, starting/cap values, cost/rank curves, interaction with INT/SPR and normal skill economy are **OPEN**.

**Grace**
- Grace may improve **both damage and healing**, as previously approved.
- Proposed (not yet user-approved): generate Grace from **effective offensive hits** and **useful healing/support**, avoiding overheal/missed/dummy spam; spend deliberately on enhanced casts; no generation from canceled casts; at most once per original cast so AoE recipients cannot multiply generation.
- **OPEN:** maximum meter/cap, sources/rates, solo viability, decay outside combat, spending trigger (automatic threshold vs deliberate), which spells consume it, empowerment size/effects, partial Grace, resetting/persistence on save and character switch.
- The current Priest proposal correctly warns about accidental loops: avoid empowerment that refunds its own full Grace cost and don't charge resources when a cast is canceled.

## Advancement concept — NOT APPROVED

Recent proposal **Hands of Mercy**: level-30 eligible Magically Touched speaks with the Trimery healer, accepts training, heals three distinct wounded lumberworkers using a temporary quest heal action, protects one recovering worker from a solo-friendly threat, and returns to the same mentor for explicit advancement to level-1 Priest. This is **only a proposed quest**; no implementation authorization. Keep already-completed actions from bypassing acceptance and avoid dependencies on a particular prior skill rank. Prior Mage/Greatblade trials are different.

## Round 1 — five questions for the user (awaiting answers)

**PRIEST-001 — Core fantasy and healing style.** Should Priest be primarily a **reactive direct healer** (big targeted heals, rapid emergency saves), a more **proactive shield/buff** healer, or a **hybrid**? It must still have a meaningful damage rotation for solo play. How should a Tier II Priest feel distinct from higher-tier dedicated healers?

**PRIEST-002 — Active kit.** The recent proposal suggests **Holy Light** (single-target Light damage), **Targeted Heal** (self/ally heal), **Holy Nova** (heals allies + damages enemies around Priest), and **Pilgrim's Step** (short dash). The older draft had **Aegis** (shield) instead of dash. Which skills do you want to keep/change, do you want the shield as a fifth skill, and what other skills should this Tier II class have? No fixed four-active limit.

**PRIEST-003 — Grace generation.** Should Grace be earned through **dealing damage, genuinely healing injured allies/self, both**, or something else? Should the generation reward damage/healing *casts* or the *actual amount* of damage/healing done? Do you want Grace to decay over time/out of combat?

**PRIEST-004 — Grace spending / empowerment.** Should Grace empower the next eligible spell **automatically** when enough is available, or should players choose when to spend it (e.g. dedicated empower toggle/key)? How should empowered **offensive** versus **healing** spells differ: raw bonus, added effects, AoE, extra shields or something else? Mana still costs its normal amount.

**PRIEST-005 — Attributes and staff.** Should Priest scale **healing mainly with SPR and damage mainly with INT**, or favor SPR for both? Do you confirm a **staff** as the primary weapon, and do you envision a ranged Light/basic attack between spell cooldowns? We can settle actual starting attributes and per-level growth in a later round.

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
| 2026-10-17 | PRIEST-001–PRIEST-005 | First five interview questions asked | **AWAITING USER** |


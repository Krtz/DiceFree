# DiceFree Tier II — Greatblade and Mage skill-rank audit

**Date:** 2026-10-19  
**Status:** DESIGN REVIEW — Rounds 1–3 recorded (2026-10-19). Both classes' weighted cap totals and selected rank schedules confirmed; Greatsword Mastery's **exact alternating-effect rank placement** and the **post-cap skill-point policy** remain OPEN. **No Unity C#, save, build or live skill changes; implementation handoff awaits final sign-off.**  
**Branch inspected:** `integration/world1-foundation`  
**Companion design:** [Priest Tier II interview](classes/PRIEST_TIER2_DESIGN.md)  
**Global authority:** [Class Progression: universal zero-rank rule](CLASS_PROGRESSION.md#universal-rank-zero-and-ordinary-basic-attack-rules-user-correction-2026-10-18)

## Scope and examined sources

1. [Greatblade prototype v0.3](GREATBLADE_PROTOTYPE_DESIGN_V03.md), [playable verification](GREATBLADE_PLAYABLE_IMPLEMENTATION.md), `Assets/_DiceFree/Scripts/Progression/GreatbladeProgressionRules.cs`, `GreatbladeSkillProgression.cs`.
2. [Mage Tier II design interview](classes/MAGE_TIER2_DESIGN.md), [playable Mage](classes/MAGE.md), `Assets/_DiceFree/Scripts/Progression/MageProgressionRules.cs`, `MageSkillProgression.cs`.
3. [Class progression](CLASS_PROGRESSION.md), [flexible ability framework](PLAIN_GEAR_AND_FLEXIBLE_SKILLS.md), [inline action-bar leveling](ACTION_BAR_INLINE_SKILL_LEVELING.md).

This is an audit of **current implementation source** and design docs, **not a Unity playtest**. Do not assume a proposed C# change already works. Historical Greatblade/Mage design statements are preserved as historical user decisions where applicable; the later **global rank-0 rule** supersedes free starter-rank policy.

## Confirmed global constraints, before redesign

- **Every skill of every new class manifestation starts at rank 0**, without free rank-1 starters.
- Level **1** grants **exactly one unspent point**; each subsequent class level currently grants one.
- A skill's **first eligibility level** may be gated even though its current rank is zero. Gaining eligibility is **not an automatic learned rank**.
- A plain basic attack deals **normal non-elemental damage**; this audit does not propose changing attack elements.
- Tier II can advance when the manifestation reaches level **60**, but can overlevel.
- **CONFIRMED RANK-001:** near-complete skill-point spend capacity **around level 80 applies to Greatblade AND Mage**, as already approved for Priest (Priest 78 total ranks: 4 × 18 + Grace 5 + Revive 1). Tier II advancement milestone is level 60; +33% levels means about level 80. Balance around available investments rather than arbitrary same-rank caps.
- Flexible class skills, variable actives/passives, in-bar upgrade icons and zero-rank skill representations are already supported at the shared UI/provider level. Do not add a fixed universal kit size.

## Current coded skill budget

| Character level / points | Greatblade | Mage |
| --- | ---: | ---: |
| Actual Tier II skills | 6 | 8 |
| Automatically free starter rank-1 skills | **4** | **4** |
| Total rank capacity from hard-coded rankLevels | **31** | **40** |
| Max skill points the present implementation allows the player to spend | **27** | **36** |
| Points earned at level 1 per actual old allocator | **0 spendable** | **0 spendable** |
| Points usable / leftover at level 60, current implementation | **27 / 32 left** | **36 / 23 left** |
| Points usable / leftover at level 80, current implementation | **27 / 52 left** | **36 / 43 left** |
| Corrected universal level-80 point budget after zero-rank migration | **80 points** | **80 points** |

Calculated directly from `GreatbladeProgressionRules.Skills`, `MageProgressionRules.Skills` and their `SkillProgression` implementations: `StarterRank=1` when first unlock gate is at level 1; `SpentPoints=Σ(max(0,Rank−StarterRank))`; `UnspentPoints=max(0,Level−1−SpentPoints)`. Current rank lists are exhausted at Greatblade level 30 and Mage level 42.

### Greatblade — current exact rank gates (already implemented)

| Skill | Current rank gates | Current rank cap | Free at level 1? |
| --- | --- | ---: | --- |
| Wide Arc | 1 / 6 / 12 / 18 / 24 / 30 | 6 | **Yes** |
| Spin Dash | 1 / 9 / 18 / 27 | 4 | **Yes** |
| Overhead Breaker | 6 / 12 / 18 / 24 / 30 | 5 | No |
| Battle Cry | 18 / 24 / 30 | 3 | No |
| Greatsword Mastery | 1 / 6 / 12 / 18 / 24 / 30 | 6 | **Yes** |
| Heavy Rhythm | 1 / 5 / 10 / 15 / 20 / 25 / 30 | 7 | **Yes** |
| **Total** | | **31** | **4** |

The v0.3 Greatblade design explicitly recommends *different rank-up breakpoint schedules* and says **do not permit generic spend-any-point-every-level ranking**. That is **not** merely a code accident. Revising this choice requires user discussion even though the later zero-starting-rank rule supersedes the free-starter implementation.

**High-rank design risks to consider when expanding caps:**
- `GreatswordMastery` adds **2% greatsword basic damage and +1 STR per rank**, with behavioral unlocks at ranks 2, 3 and 4. More ranks should remain meaningful, but pure linear stat inflation may crowd out gear.
- `HeavyRhythm` scales Momentum gain by `1+0.08×rank`; decay multiplier bottoms out at **0.3**, so rank benefits may plateau in one dimension at higher ranks.
- `WideArc`, `SpinDash`, `OverheadBreaker`, `BattleCry` use provisional per-rank coefficients. Additional ranks need careful cooldown, damage and utility curves rather than extending linear scalars blindly.
- Preserve level **1** first eligibility for Wide Arc, Spin Dash, Greatsword Mastery and Heavy Rhythm; level **6** first eligibility for Overhead Breaker and level **18** for Battle Cry unless user revises those separately.
- Greatblade's **Greatsword Mastery rank 4 shield vs heal** was deliberately an unresolved design choice even though the prototype implements a shield; do not silently portray prototype balancing as final canon.

### Mage — current exact rank gates (already implemented)

| Skill | Current rank gates | Current cap | Free at level 1? |
| --- | --- | ---: | --- |
| Ember Bolt | 1 / 6 / 12 / 18 / 24 | 5 | **Yes** |
| Frost Snare | 1 / 6 / 12 / 18 / 24 | 5 | **Yes** |
| Arc Lightning | 6 / 12 / 18 / 24 / 30 | 5 | No |
| Quicksand | 12 / 18 / 24 / 30 / 36 | 5 | No |
| Half-Life | 18 / 24 / 30 / 36 / 42 | 5 | No |
| Math Barrier | 6 / 12 / 18 / 24 / 30 | 5 | No |
| Elemental Weaving | 1 / 6 / 12 / 18 / 24 | 5 | **Yes** |
| Mana Attunement | 1 / 6 / 12 / 18 / 24 | 5 | **Yes** |
| **Total** | | **40** | **4** |

**High-rank mechanical risks:**
- `MathBarrier` redirects **`(25+5×rank)%`** of incoming damage, for **`(2−0.1×rank)` Mana per damage prevented**; duration = **2×rank seconds**. At **rank 15 the redirect reaches 100%**, at **rank 16 it exceeds 100%**, and at **rank 20 Mana/damage reaches zero** (negative at rank 21). This is **unsafe if the class simply receives 18+ ranks without formula changes or limits**. Even at 10 ranks, 20s duration versus the current 18s cooldown warrants balance attention.
- `ElementalWeaving` combines rotating-element stacks and a **−1 percentage point resistance aura per skill rank**. The +5% per-element stack and 0.2s cooldown reduction are not currently multiplied by the passive's rank; ensure leveling this passive remains satisfying if given many ranks.
- `ManaAttunement` adds **30 maximum Mana and 0.5 Mana/s regeneration per rank**. Large caps substantially increase total resource economy; make costs sustainable but not effortless.
- Half-Life currently lowers both defense channels **5 × rank** and could have disproportionately strong resistance/DEF effects if extrapolated to higher ranks.
- Preserve first eligibility levels from current Mage design (1/6/12/18 based on skill) unless user approves changes; early first-unlock gates and later rank allocation are distinct design choices.

## Shared code-level discrepancy (confirmed)

**Both `GreatbladeSkillProgression.cs` and `MageSkillProgression.cs` currently do all of:**
1. `StarterRank(kind)` returns **1** for a level-1 first-unlock skill.
2. `Rank(kind)` **implicitly returns that free rank** when no rank is saved.
3. `SpentPoints` subtracts free starter ranks; `UnspentPoints` uses **`Level−1−SpentPoints`**, so a new level-1 character cannot spend any points.
4. `RestoreState` raises saved ranks to at least `StarterRank`, and `CanRestore` validates existing saves against old gate tables and the old spending budget.
5. Class rank caps and rank-up gates are encoded in `rankLevels` arrays. Merely changing a class document or creating more points does not increase live caps.

### Safe migration requirements before implementation

- **First eligibility levels and rank-zero starts are now CONFIRMED for both classes (RANK-002/003)**; weighted rank caps and per-level upgrades remain subject to review. Finalize per-skill caps, formulas, eligibility-versus-rank-gate semantics and migration details before editing Unity code.
- Treat `classSkills` save records and save/restore validation as authoritative. **Never erase skill ranks, invalidate an existing manifestation or overwrite user progress** because old free ranks now count against a new point budget.
- **CONFIRMED RANK-005:** perform a **ONE-TIME RESET AND FULL SKILL-POINT REFUND** for existing Greatblade and Mage saves, including their four formerly free rank-1 skills; **NO grandfathered skill ranks**. The preserved manifestation keeps its class level/XP, attributes, gear, inventory, items, quests, currency, appearance, progression, and other unrelated state. Available unspent points after reset must be **equal to character level** under the one-point-at-level-1 and +1-per-level system, not `level−1`; do not credit a phantom extra four points for formerly free starters. Must migrate atomically, idempotently, backup/validate before writing, mark completed per save/manifestation, restore all ranks as 0 until repurchased, and never overwrite parent manifestations or other class saves. Interaction timing and player-facing notice remain for sign-off.
- Centralize rank-zero, one-point-at-level-1 semantics for both classes; update point budget, rank gates, passive activation, rank-zero tooltips and in-bar [+] eligibility.
- If expanding ranks, retune Mage Math Barrier and high-rank Greatblade Mastery/Heavy Rhythm; add anti-negative/over-100%-redirect validation, skill point overspend checks and tests around level 1, 6, 18, 30, 42, 60, 80, class fork, class switch, save load and reloading legacy saves.
- Keep old gameplay code untouched during this interview/audit. Current branch's previously verified builds do not validate a hypothetical revised implementation.

## Round 1 — five user decisions (2026-10-19)

- **RANK-001 CONFIRMED:** Greatblade and Mage share Priest's nearly-spend-all-points **around level 80** progression philosophy.
- **RANK-002 CONFIRMED:** Greatblade keeps original first-learn levels: Wide Arc, Spin Dash, Greatsword Mastery and Heavy Rhythm at 1; Overhead Breaker at 6; Battle Cry at 18. All start rank 0, level 1 grants one point.
- **RANK-003 CONFIRMED:** Mage keeps original first-learn levels: Ember Bolt, Frost Snare, Elemental Weaving and Mana Attunement at 1; Arc Lightning and Math Barrier at 6; Quicksand at 12; Half-Life at 18. All start rank 0, one initial point.
- **RANK-004 CONFIRMED:** max ranks differ per skill based on effects; **Math Barrier stays at five ranks** with existing formulas.
- **RANK-005 CONFIRMED:** existing Greatblade and Mage class skills must be **reset and refunded once**, preserving all other character state.

## Round 2 — answers recorded (2026-10-19)

- **RANK-006 CONFIRMED:** heterogeneous **upgrade cadence** per skill. Some can take another rank every character level, some require **gaps of multiple character levels**. **Half-Life every four class levels is an example/preference, not an exact finalized schedule.** First unlock levels unchanged.
- **RANK-007 CONFIRMED preference, cap not final:** Greatsword Mastery should **either have fewer maximum ranks** or reserve substantial new mechanics for separated **milestone ranks**, using **small flat stat increases** in the ordinary filler ranks. Do not assert the previous **12-rank proposed Mastery cap** is accepted.
- **RANK-008 CONFIRMED:** **Mage's weighted rank-cap proposal accepted in full**: Ember Bolt **18**, Frost Snare **12**, Arc Lightning **12**, Quicksand **10**, Half-Life **8**, Math Barrier **5 unchanged**, Elemental Weaving **8**, Mana Attunement **7** (total **80**).
- **RANK-009 CONFIRMED:** secondary utility effects may have **diminishing returns, reasonable caps and intentional plateauing** at higher ranks (freeze duration, chain target count, defense reduction, Momentum generation and similar). Higher ranks must still grant meaningful primary stat/damage/utility benefit, not dead ranks. Math Barrier's existing 5 ranks/formulas remain untouched.
- **RANK-010 CONFIRMED:** full one-time Greatblade/Mage skill reset and refund **automatically on first load of each old class save**, **NO in-game notification required**. Preserve recoverable backup and one-time migration safeguards. Never silently reset unrelated class saves.

## Updated weighted cap distributions

### Greatblade — FINAL weighted rank caps CONFIRMED RANK-011/012

**User chose exactly eight Greatsword Mastery ranks**, with **alternating substantial mechanics and flat STR/sword-damage increases**. User-approved total Greatblade capacity: **78 ranks**.

| Skill | First-learn level | Maximum rank (CONFIRMED) |
| --- | ---: | ---: |
| Wide Arc | 1 | **20** |
| Spin Dash | 1 | **16** |
| Overhead Breaker | 6 | **18** |
| Battle Cry | 18 | **8** |
| Greatsword Mastery | 1 | **8** |
| Heavy Rhythm | 1 | **8** |
| **Total** | — | **78** |

#### Greatsword Mastery — current behavior versus proposed alternate effects

The currently implemented passive has these genuine effects:
- At **every rank**: +1 STR and +2% greatsword/basic sword damage per rank (provisional existing values).
- Currently at **rank 2**: Wide Arc grants a temporary attack-speed buff (base +8%, +2 percentage points per later rank, 3s duration +0.25s per later rank, per existing prototype).
- Currently at **rank 3**: Wide Arc cone grows wider (base +15 degrees, +3 degrees per later rank in current prototype).
- Currently at **rank 4**: Overhead Breaker awards temporary **shield** (+5% max HP, 5 seconds, +0.5 percentage points later), but **shield versus heal remains a design question**. Existing shield is provisional gameplay.
- At rank 1 the passive starts giving STR/sword-damage bonuses; this is its foundational identity rather than an additional attack.

**PROPOSED layout requiring user sign-off (RANK-016):** rank **1** basic mastery (STR/damage), rank **2** stats only, rank **3** Wide Arc haste, rank **4** stats only, rank **5** wider Wide Arc, rank **6** stats only, rank **7** Overhead Breaker temporary shield (or heal, unresolved), rank **8** stats only. **All ranks improve STR/damage**. This satisfies an alternating every-other-rank idea without introducing a new unapproved mechanic, but moves existing features from live 2/3/4 → proposed 3/5/7. This rank-shift is **NOT YET APPROVED**; migration reset will help users reallocate.

### Mage — user-approved weighted cap (80 points)

| Skill | First learn | **Confirmed maximum rank** |
| --- | ---: | ---: |
| Ember Bolt | 1 | **18** |
| Frost Snare | 1 | **12** |
| Arc Lightning | 6 | **12** |
| Quicksand | 12 | **10** |
| Half-Life | 18 | **8** |
| Math Barrier | 6 | **5 (unchanged)** |
| Elemental Weaving | 1 | **8** |
| Mana Attunement | 1 | **7** |
| **Total** | — | **80** |

**CONFIRMED RANK-014:** Half-Life ranks 1–8 become eligible at Mage levels **18, 22, 26, 30, 34, 38, 42, 46** respectively (**four character levels between ranks**). This schedule is approved, not just an example.

**CONFIRMED RANK-014:** Math Barrier maintains its five-rank cap, existing formulas and exact skill-rank eligibility levels **6, 12, 18, 24, 30**.

For increased rank caps, do **not** carry freeze, jump counts, DEF reductions, Mana sustain, or cooldown benefits upward without bound. Tuned benefits should be visible on rank previews.

### Example mixed rank-up cadence (DESIGNER PROPOSAL, NOT user-approved)

| Skill family | Example progression | Rationale |
| --- | --- | --- |
| Primary direct damage abilities (Wide Arc, Overhead Breaker, Ember Bolt) | **CONFIRMED RANK-013:** upgrade each class level after first unlock, to their own caps | Main long-term investment sinks |
| Multi-target/mobility spells (Spin Dash, Arc Lightning) | 2 levels per rank | Manage mobility/target expansion |
| Status/DOT fields (Frost Snare, Quicksand) | 2–4 levels per rank **PROPOSED**; Half-Life **CONFIRMED four levels** from level 18 | Avoid exaggerated CC/DEF effects |
| Major utility/passives (Battle Cry, Heavy Rhythm, Elemental Weaving, Mana Attunement) | 3–5 levels per rank | Preserve special effects and meaningful increments |
| Greatsword Mastery | maybe fewer ranks or milestone ranks with flat-stat fillers | User specifically flagged this ability |
| Math Barrier | **CONFIRMED existing gates**: 6 / 12 / 18 / 24 / 30 | Five-rank special utility; existing formulas |

Schedules must preserve first-learn levels and allow almost all rank investments to be purchasable by approximately level 80. Some skills have per-level increases (confirmed), some have bespoke gaps. Avoid dead points and stale rank reminders once everything becomes capped. The final post-cap/no-upgrade-available notification policy is still open. 

## Implementation conditions, migration and tests

- **Confirmed migration**: on first load of each pre-update Mage/Greatblade manifestation, **automatically reset all class skill ranks to 0**, preserving class level, XP, gear, items, gold, quests, class/echo progression and parent save manifestations. **All level 80+ legacy saves are eligible for the reset (RANK-015)**. Their final **excess spendable-points policy remains OPEN** due to user's new concern about permanently unspendable points; do not quietly discard or endlessly surface surplus before it is decided. No old four free ranks survive.
- **No notification, confirmation modal or extra respec button** for this one-time migration.
- For safety: **atomic per-manifestation migration, idempotent migration marker/version, recoverable pre-migration backup**, passive re-evaluation, default rank-0 tooltips/upgrade UI and save validation after committing.
- **Neither the migration nor combat skills have been changed in code**. Final schedules and high-rank curves await user sign-off in the separate implementation handoff.

## Cross-class Round 3 — answers recorded 2026-10-19

- **RANK-011 — CONFIRMED cap/direction:** Greatsword Mastery maximum **8 ranks**. Alternate actual added mechanics and ordinary +STR/+greatsword damage ranks. The exact new placement of its currently rank-2 haste, rank-3 cone growth and rank-4 finisher protection is **OPEN**; propose milestones rank 3/5/7.
- **RANK-012 — CONFIRMED:** Greatblade caps **Wide Arc 20, Spin Dash 16, Overhead Breaker 18, Battle Cry 8, Greatsword Mastery 8, Heavy Rhythm 8**, totaling **78** ranks.
- **RANK-013 — CONFIRMED:** Wide Arc, Overhead Breaker and Ember Bolt **can gain one rank each subsequent class level after first unlock**, subject to caps. Other skills use different, individually authored spacings.
- **RANK-014 — CONFIRMED:** Mage Half-Life first/every rank eligibility levels **18/22/26/30/34/38/42/46**, Math Barrier **6/12/18/24/30**, current Math Barrier cap/formulas preserved.
- **RANK-015 — CONFIRMED support for high-level old saves, OPEN overflow policy:** user explicitly said legacy Mage/Greatblade characters at **level 80+** should receive the automatic skill reset/refund but then questioned infinite unspendable points. Two ideas surfaced: **stop awarding/spending points once every skill reaches maximum** vs **grant every class an uncapped/long-cap Novice-like all-stat passive point sink**. **Neither selected**; user wants advice. Critical UX: do not show recurring unspent-point reminders if no rank is purchasable or after all abilities are maxed. See recommendation and follow-up below.

## Skill-point overflow and rank-ready UX — new design discussion

**Identified problem:** `UnspentPoints = Level − SpentPoints` grows indefinitely when class levels exceed total possible ranks. The action-bar [+] and points reminder can show surplus numbers that **cannot be used**, especially at level 80+ after maxing every rank. Legacy high-level saves exacerbate this after migration.

**Recommended policy (NOT YET APPROVED):** **do not add a global Novice all-stat passive to every class**. Otherwise all classes gain potentially unbounded STR/INT/SPR/VIT/AGI from overleveling, eroding authored class identities, gear balance and deliberate advancement choice. Instead use a **bounded per-manifestation skill-point budget**:

- **Total allocation budget** = `min(current class level, sum of current class skill max ranks)` (e.g. Greatblade `min(level,78)`, Mage `min(level,80)`, Priest `min(level,78)`). Unspent allocation = `max(0, total budget − ranks spent)` respecting class-specific level-gated first availability/rank spacing.
- Once **all ranks are purchased**, **unspent is zero** and future level-ups **do not award surplus spendable skill points**; XP, character level, stats and overleveling continue normally. Points at a temporary rank gate may remain banked until a legal upgrade unlocks.
- The skillpoint reminder/[+] can be visible **only when there is at least one currently purchasable rank** (enough banked points AND rank gate satisfied), not merely when an unspendable point count is nonzero. Do not show per-level "you have skill points" prompts without an eligible upgrade.
- If the class later receives new skills or raised rank caps, recalculating the bounded budget from **current class level** makes any retroactively earned eligibility visible and preserves old overlevels. Skill rank spent data and migration revision markers remain authoritative; no phantom persistent overflow.
- A migrated level-100 Mage would have **up to 80** points available to reallocate within Mage's 80-rank cap; a migrated level-100 Greatblade **up to 78**, not 100 permanently unspendable points. This **does not reduce character level or earned XP**, only caps the *currently spendable skill-point budget*.
- **Still open:** special rewards that award bonus skill points beyond class-level progression, class-specific uncapped sink mechanics, respec treatment and whether to show a separate earned lifetime-points statistic (but **not** an unusable badge). Such future systems need deliberate design; do not assume.

## Cross-class Round 4 — five questions (awaiting user)

**RANK-016 — Greatsword Mastery's eight ranks.** Can we use **rank 1** to activate the base passive, **ranks 2/4/6/8** for extra STR and greatsword damage only, **rank 3** to add the temporary Wide Arc attack-speed bonus, **rank 5** to widen Wide Arc and **rank 7** to add protection after Overhead Breaker? This moves the existing early effects from ranks 2/3/4 to 3/5/7. For rank 7, do you want the currently implemented small **shield**, or **healing** instead?

**RANK-017 — Unspendable point solution.** My recommendation is **cap the spendable skill-point budget to the class's total rank capacity**, stopping further allocation when all ranks are maxed, but let the character **continue leveling and gaining stats**. Do **NOT** add a universal novice-style passive stat sink. Does that sound right?

**RANK-018 — Level-gated reminder.** If a character has banked skill points but **none of their currently unlocked skills can upgrade yet**, should the UI **hide the unspent-points notification until at least one legal upgrade becomes available**? This also fixes misleading reminder popups before the character reaches the next rank gate.

**RANK-019 — Greatblade's other rank-up spacing.** We have Wide Arc and Overhead Breaker eligible every class level after first unlock. For **Spin Dash (16 ranks)**, **Battle Cry (8)**, **Greatsword Mastery (8)** and **Heavy Rhythm (8)**, should I pick provisional rank-gate schedules tailored to each skill and balanced toward level 80, or do you want to choose any exact intervals yourself?

**RANK-020 — Higher-rank Mage pacing.** Ember Bolt increases every class level; Half-Life gets a rank every **4 levels**; Math Barrier keeps **6/12/18/24/30**. Should I choose distinct provisional intervals for Frost Snare, Arc Lightning, Quicksand, Elemental Weaving and Mana Attunement, with caps and diminishing utility effects already confirmed, for you to review?

## Decision log

| Date | Question | Result |
| --- | --- | --- |
| 2026-10-18 | Universal rank-0 and one point at class level 1 | **CONFIRMED** |
| 2026-10-19 | RANK-001–005 | Level-80 budget, first unlocks unchanged, varied caps, Math Barrier 5, one-time reset/refund | **CONFIRMED** |
| 2026-10-19 | RANK-006–010 | Per-skill rank interval, Mage 80 weighted ranks, diminishing utility effects, silent automatic migration | **CONFIRMED** |
| 2026-10-19 | RANK-011 | Greatsword Mastery max 8, alternating mechanic/stat ranks | **CONFIRMED**, actual rank milestone positions/open shield vs heal |
| 2026-10-19 | RANK-012 | Greatblade caps 20/16/18/8/8/8 = 78 | **CONFIRMED** |
| 2026-10-19 | RANK-013 | Greatblade Wide Arc and Breaker, Mage Ember Bolt rank-up every class level | **CONFIRMED** |
| 2026-10-19 | RANK-014 | Half-Life 18/22/26/30/34/38/42/46; Math Barrier 6/12/18/24/30 | **CONFIRMED** |
| 2026-10-19 | RANK-015 | High-level old saves migrate; persistent unusable-point problem flagged; bounded points vs passive sink undecided | **CONFIRMED migration**, **OPEN overflow policy** |
| 2026-10-19 | RANK-016–020 | New five questions | **AWAITING USER** |

# DiceFree Tier II — Greatblade and Mage skill-rank audit

**Date:** 2026-10-19  
**Status:** DESIGN REVIEW — Rounds 1–4 recorded (2026-10-19). Greatsword Mastery's alternating rank milestones + shield, both classes' cap totals, universal bounded skill-point budget and purchasable-upgrade-only indicator are CONFIRMED. Designer-chosen specific remaining rank-gate schedules below are playtest baselines. **No Unity C#, save, build or live skill changes; implementation handoff awaits final sign-off.**  
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
- Currently at **rank 4**: Overhead Breaker awards temporary **shield** (+5% max HP, 5 seconds, +0.5 percentage points per later rank) in the old implementation. **RANK-016 CONFIRMED** that the redesigned milestone at rank 7 remains a **shield, not a heal**. Exact high-rank shield values remain playtest balance.
- At rank 1 the passive starts giving STR/sword-damage bonuses; this is its foundational identity rather than an additional attack.

**CONFIRMED RANK-016 layout:** rank **1** initial STR/damage mastery, **2** stats only, **3** unlock Wide Arc attack-speed buff, **4** stats only, **5** expand Wide Arc hit cone, **6** stats only, **7** unlock Overhead Breaker temporary **SHIELD**, **8** stats only. **EVERY rank improves STR + greatsword damage**. Ranks 2/4/6/8 do not need distinct combat mechanics. This deliberately moves former live mechanic milestones 2/3/4 → 3/5/7; old saves migrate by resetting/refunding rather than silently upgrading these effects. Existing numeric modifiers are provisional.

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

- **Confirmed migration**: on first load of each pre-update Mage/Greatblade manifestation, **automatically reset all class skill ranks to 0**, preserving class level, XP, gear, items, gold, quests, class/echo progression and parent save manifestations. **Level 80+ saves migrate too**. **RANK-017 CONFIRMED** bounded spendable budgets: skill allocation after migration is **min(characterLevel, class total ranks)**, i.e. 78 for level 80+ Greatblade, 80 for level 80+ Mage, with 0 allocated immediately after reset. Do not discard level/XP, present phantom excess points or retain the four old free ranks.
- **No notification, confirmation modal or extra respec button** for this one-time migration.
- For safety: **atomic per-manifestation migration, idempotent migration marker/version, recoverable pre-migration backup**, passive re-evaluation, default rank-0 tooltips/upgrade UI and save validation after committing.
- **Neither the migration nor combat skills have been changed in code**. Final schedules and high-rank curves await user sign-off in the separate implementation handoff.

## Cross-class Round 3 — answers recorded 2026-10-19

- **RANK-011 — CONFIRMED cap/direction; resolved by RANK-016:** Greatsword Mastery maximum **8 ranks**, with base at 1, new mechanics at ranks 3/5/7 and STR/damage bonuses at all ranks.
- **RANK-012 — CONFIRMED:** Greatblade caps **Wide Arc 20, Spin Dash 16, Overhead Breaker 18, Battle Cry 8, Greatsword Mastery 8, Heavy Rhythm 8**, totaling **78** ranks.
- **RANK-013 — CONFIRMED:** Wide Arc, Overhead Breaker and Ember Bolt **can gain one rank each subsequent class level after first unlock**, subject to caps. Other skills use different, individually authored spacings.
- **RANK-014 — CONFIRMED:** Mage Half-Life first/every rank eligibility levels **18/22/26/30/34/38/42/46**, Math Barrier **6/12/18/24/30**, current Math Barrier cap/formulas preserved.
- **RANK-015 — CONFIRMED support for high-level old saves; resolved RANK-017/018:** level 80+ old characters migrate, but their post-migration spendable pool is capped at total class ranks; no unused surplus or dead upgrade notifications.

## Skill-point budget and upgrade-ready UI — CONFIRMED RANK-017/018

**CONFIRMED GLOBAL RULE: Do not add a generic Novice all-stat passive to every class.** Preserve authored class identities, gear balance and deliberate advancement paths. Cap *spendable class skill points* using **current class level and current total number of purchasable class skill ranks**:

- `allocationBudget = min(classLevel, Σ(maxRank of class-owned skills))`, `unspent = max(0, allocationBudget − Σ(actual purchased ranks))`.
- Greatblade budget is `min(level, 78)`; Mage `min(level, 80)`; Priest `min(level, 78)` under current approved class rosters. New level-1 manifestation has all ranks 0 and 1 point.
- When every class rank is maxed, no further spendable skill points accumulate at any later level. Character level, XP, attribute growth, gear and progression still continue.
- **Upgrade-ready reminder condition (CONFIRMED):** show the skillpoint badge/popup **ONLY when unspent > 0 AND at least one specific class ability is legally purchasable right now** (level/rank gates, skill cap, current class and points all satisfied). **Hide it** if points are banked but every possible upgrade is still gated; hide it after all available ranks are maxed. No spammy level-up skill-point notice when there is nothing to purchase.
- **Keep the banked allocation accessible in the class skill UI** for normal user review even when the reminder is hidden (proposed implementation detail; not a separate popup); do not erase banked eligible points.
- Budget is derived from current class level and authored caps, rather than a permanent counter of all historical awards. If later content adds skill caps/ranks, previously overleveled players automatically have proportional points to allocate, capped by the new class total. A migrated level-100 Greatblade has 78 available points after reset; level-100 Mage has 80, without losing levels/XP.
- Future bonuses explicitly adding extra class skill points, special uncapped skills or earned-point lifetime tracking would require authored exceptions rather than silently violating the bounded budget.

## Per-skill rank-gate schedules — designer-selected provisional playtest baselines

**User delegation RANK-019/020:** choose individual gaps per skill, but wherever possible preserve the ability to **spend one earned point on some legal upgrade on EVERY class level up to the class's maximum allocation**. Confirmed first unlocks, maximum ranks and previously user-approved gate intervals must not change. These are **specific designer-selected playtest schedules**, not immutable numeric balance and not Unity-implemented yet.

### Greatblade — 78 ranks available by level 78

| Ability | Cap | First learn | Interval | Last rank at class level | Exact eligible class levels |
| --- | ---: | ---: | ---: | ---: | --- |
| Wide Arc | 20 | 1 | 1 | 20 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 |
| Spin Dash | 16 | 1 | 5 | 76 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76 |
| Overhead Breaker | 18 | 6 | 1 | 23 | 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 |
| Battle Cry | 8 | 18 | 8 | 74 | 18, 26, 34, 42, 50, 58, 66, 74 |
| Greatsword Mastery | 8 | 1 | 10 | 71 | 1, 11, 21, 31, 41, 51, 61, 71 |
| Heavy Rhythm | 8 | 1 | 11 | 78 | 1, 12, 23, 34, 45, 56, 67, 78 |

- **User CONFIRMED:** Wide Arc and Overhead Breaker can rank up every level after first purchase. Greatsword Mastery rank-1 starts at level 1 but its *extra special effects* occur at **skill ranks 3/5/7**, not automatically when merely reaching their corresponding class levels.
- The longer cadence for Spin Dash, Battle Cry, Mastery and Heavy Rhythm is a provisional method of preserving utility identity and making future upgrades meaningful.
- **Validation:** with these gates, for **every Greatblade class level 1 through 78**, the total count of already-unlocked distinct rank purchases is **at least the character level**. Therefore a player allocating one point every level is not forced to bank points, although they may choose to. By level 78 all **78** ranks have become eligible. The spendable pool then caps at 78.

### Mage — 80 ranks available by level 79

| Ability | Cap | First learn | Interval | Last rank at class level | Exact eligible class levels |
| --- | ---: | ---: | ---: | ---: | --- |
| Ember Bolt | 18 | 1 | 1 | 18 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 |
| Frost Snare | 12 | 1 | 7 | 78 | 1, 8, 15, 22, 29, 36, 43, 50, 57, 64, 71, 78 |
| Arc Lightning | 12 | 6 | 6 | 72 | 6, 12, 18, 24, 30, 36, 42, 48, 54, 60, 66, 72 |
| Quicksand | 10 | 12 | 7 | 75 | 12, 19, 26, 33, 40, 47, 54, 61, 68, 75 |
| Half-Life | 8 | 18 | 4 | 46 | 18, 22, 26, 30, 34, 38, 42, 46 |
| Math Barrier | 5 | 6 | 6 | 30 | 6, 12, 18, 24, 30 |
| Elemental Weaving | 8 | 1 | 11 | 78 | 1, 12, 23, 34, 45, 56, 67, 78 |
| Mana Attunement | 7 | 1 | 13 | 79 | 1, 14, 27, 40, 53, 66, 79 |

- **User CONFIRMED:** Ember Bolt can rank up each Mage level after purchase; Half-Life ranks 1–8 at **18/22/26/30/34/38/42/46**; Math Barrier retains **6/12/18/24/30** and max rank 5, formulas unchanged. The other Mage skill intervals above are delegated designer choices for initial playtesting.
- **Validation:** for **every Mage class level 1 through 80**, the total number of accessible individual rank purchases is **at least that level**, so allocating one point at each level is always feasible. The last of all **80** total ranks becomes available at level **79**, ready for the final budget point at level 80.

### Mathematically checked availability milestones

These counts represent total **distinct rank purchases eligible at or below a given class level**, *not* points granted or already spent. A valid uninterrupted one-point-per-level spending path requires `unlockedRankCapacity(level) >= min(level, totalRanks)`.

| Class level | Greatblade cumulative eligible ranks | Mage cumulative eligible ranks | Capped allocation budgets (GB / Mage) |
| ---: | ---: | ---: | ---: |
| 1 | 4 | 4 | 1 / 1 |
| 6 | 11 | 11 | 6 / 6 |
| 18 | 40 | 33 | 18 / 18 |
| 30 | 52 | 46 | 30 / 30 |
| 46 | 62 | 59 | 46 / 46 |
| 60 | 68 | 68 | 60 / 60 |
| 70 | 73 | 74 | 70 / 70 |
| 75 | 76 | 77 | 75 / 75 |
| 77 | 77 | 77 | 77 / 77 |
| 78 | 78 | 79 | 78 / 78 |
| 79 | 78 | 80 | 78 / 79 |
| 80 | 78 | 80 | 78 / 80 |

**Both proposed gate tables were checked against every integer class level from 1 through their budget cap**, not just the milestone rows. Neither class has a forced dead skill point level under these schedules. If the player deliberately banks points, the reminder still follows real purchase eligibility. Revisit timings during actual gameplay if the spacing feels too slow.

## Cross-class Round 4 — answers recorded 2026-10-19

- **RANK-016 — CONFIRMED:** Greatsword Mastery **eight ranks**, base passive at rank 1; **3 Wide Arc haste, 5 wider cleave, 7 Overhead Breaker SHIELD**. Ranks 2/4/6/8 add only the baseline STR/greatsword-damage improvement. All eight ranks improve base stats/damage.
- **RANK-017 — CONFIRMED:** no universal stat-point sink; **stop new spendable points after the authored class's total purchasable ranks** (`min(classLevel, ΣmaxRanks)`). Character continues normal level/XP/stats growth.
- **RANK-018 — CONFIRMED:** show unspent-point indicator **only when at least one specific skill has a legal upgrade right now**; banked points with no eligible rank produce no badge/popup.
- **RANK-019 — CONFIRMED DELEGATION:** assistant may choose Greatblade's remaining per-skill rank gates while preserving **ability to spend one point each level through max budget**. Designer schedule and verified level-by-level capacity above.
- **RANK-020 — CONFIRMED DELEGATION:** same for Mage's Frost Snare, Arc Lightning, Quicksand, Elemental Weaving and Mana Attunement; designer schedule and verification above.

## Cross-class Round 5 — next five questions (awaiting user)

**RANK-021 — Skill rank schedules.** I've designed and checked schedules allowing a point to be invested **at every level until maxed**. Greatblade: Spin Dash every **5** levels, Battle Cry every **8**, Greatsword Mastery every **10**, Heavy Rhythm every **11**, while Wide Arc and Breaker are every level. Mage: Frost Snare every **7**, Arc Lightning every **6**, Quicksand every **7**, Elemental Weaving every **11**, Mana Attunement every **13**, keeping confirmed Half-Life, Math Barrier and Ember rules. Shall these be our provisional default schedules for the initial implementation, subject to playtesting?

**RANK-022 — Greatsword Mastery power.** For all eight ranks, the existing prototype gives **+1 STR and +2% greatsword damage per rank**, plus the new milestones at 3/5/7. Should that remain the initial baseline, or would you prefer diminishing bonuses toward ranks 6–8?

**RANK-023 — Greatblade shield strength.** At Greatsword Mastery rank 7, should Overhead Breaker's temporary shield retain the prototype's **5% maximum HP for 5 seconds** (and its small later-rank improvement), or should we start with a different amount/duration?

**RANK-024 — Mage high-rank control effects.** Would you like me to choose sensible, playtestable ceilings for high-rank Frost Snare freeze duration, Arc Lightning chain targets, Half-Life defense reduction and Quicksand slowing/damage, preserving their identity but avoiding runaway effects? Math Barrier remains unchanged.

**RANK-025 — Banked points in menus.** When points are banked for a future level gate but nothing is currently purchasable, should the skill menu still **show the exact banked point count and next eligible level**, without showing the popup/reminder? (The action bar [+] only appears for legal purchases.)

## Decision log

| Date | Question | Result |
| --- | --- | --- |
| 2026-10-18 | Universal rank-0 and one point at class level 1 | **CONFIRMED** |
| 2026-10-19 | RANK-001–005 | Level-80 budget, first unlocks unchanged, varied caps, Math Barrier 5, one-time reset/refund | **CONFIRMED** |
| 2026-10-19 | RANK-006–010 | Per-skill rank interval, Mage 80 weighted ranks, diminishing utility effects, silent automatic migration | **CONFIRMED** |
| 2026-10-19 | RANK-011–015 | Greatsword Mastery max 8, Greatblade weighted caps 78, Ember/WideArc/Breaker every-level, Half-Life/MathBarrier gates, high-level saves migrate | **CONFIRMED** |
| 2026-10-19 | RANK-016 | Mastery milestones 1/3/5/7 and shield (not heal) at rank 7 | **CONFIRMED** |
| 2026-10-19 | RANK-017 | Capped skill-point budget; no universal novice stat passive | **CONFIRMED** |
| 2026-10-19 | RANK-018 | Upgrade-ready reminder only for purchasable rank | **CONFIRMED** |
| 2026-10-19 | RANK-019–020 | Assistant designs per-skill gate schedule, ensures one spendable rank each class level till max | **CONFIRMED delegation; computed schedules provisional** |
| 2026-10-19 | RANK-021–025 | Round 5 questions asked | **AWAITING USER** |

# DiceFree Tier II — Greatblade and Mage skill-rank audit

**Date:** 2026-10-19  
**Status:** DESIGN REVIEW — Rounds 1–2 confirmed (2026-10-19). Mage's weighted caps approved; Greatblade Mastery rank shape and cap distribution under review; mixed skill-specific rank cadence approved. **No Unity C#, save, build or live skill changes yet; implementation handoff awaits final sign-off.**  
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

### Greatblade — counterproposal, not yet approved

**User's concern:** Greatsword Mastery should not need 12 ranks filled with barely differentiated bonuses. My recommended adjustment is **8 Mastery ranks**, keeping its current real milestone effects at ranks **1–4**, with later upgrades primarily increasing flat STR and greatsword-damage bonuses until we deliberately design distinct higher milestones. The four reclaimed ranks go to the two main damage abilities rather than bloating Heavy Rhythm (which already has a Momentum decay floor).

| Skill | Previous proposal | New proposal | First learn |
| --- | ---: | ---: | ---: |
| Wide Arc | 18 | **20** | 1 |
| Spin Dash | 14 | **14** | 1 |
| Overhead Breaker | 18 | **20** | 6 |
| Battle Cry | 8 | **8** | 18 |
| Greatsword Mastery | 12 | **8** | 1 |
| Heavy Rhythm | 8 | **8** | 1 |
| **Total** | **78** | **78** | — |

Alternative equally viable: keep **Mastery 12** but have **explicit milestone upgrades** separated by ranks that grant only modest STR/greatsword-damage bonuses. The user asked to weigh these options, not to silently finalize 8 vs 12. For either approach, preserve previously approved **rank 2** Wide Arc haste, **rank 3** wider cleave, **rank 4** small finisher protection (shield vs heal still open); don't replace them with fluff.

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

**Half-Life cadence example:** rank 1 at Mage level 18, then 22, 26, 30, 34, 38, 42, 46 (every four levels). This is a concrete **DESIGNER EXAMPLE based on the user's tentative suggestion**, not confirmed gate table.

**Math Barrier:** maintain its current five-rank cap and formulas, and preferably keep original gates **6, 12, 18, 24, 30** unless the user specifically wants them changed.

For increased rank caps, do **not** carry freeze, jump counts, DEF reductions, Mana sustain, or cooldown benefits upward without bound. Tuned benefits should be visible on rank previews.

### Example mixed rank-up cadence (DESIGNER PROPOSAL, NOT user-approved)

| Skill family | Example progression | Rationale |
| --- | --- | --- |
| Primary direct damage abilities (Wide Arc, Overhead Breaker, Ember Bolt) | once every class level after first unlock | Main long-term investment sinks |
| Multi-target/mobility spells (Spin Dash, Arc Lightning) | 2 levels per rank | Manage mobility/target expansion |
| Status/DOT fields (Frost Snare, Quicksand, Half-Life) | 2–4 levels per rank | Avoid exaggerated CC/DEF effects |
| Major utility/passives (Battle Cry, Heavy Rhythm, Elemental Weaving, Mana Attunement) | 3–5 levels per rank | Preserve special effects and meaningful increments |
| Greatsword Mastery | maybe fewer ranks or milestone ranks with flat-stat fillers | User specifically flagged this ability |
| Math Barrier | **existing gates proposed unchanged**: 6 / 12 / 18 / 24 / 30 | Five-rank special utility, existing formulas |

Schedules should satisfy **first-unlock levels** and allow the proposed total rank investments to become purchasable by approximately **level 80**. A cadence must not push the class's nominal max ranks well beyond 80 or leave many unusable level-80 points. 

## Implementation conditions, migration and tests

- **Confirmed migration**: on first load of each pre-update Mage/Greatblade manifestation, **automatically reset all its class skill ranks to 0** and return **exactly one spendable point per current class level**, not one per level plus the old four free starter ranks. This preserves class level, XP, gear, items, gold, quests, class/echo progression and parent save manifestations.
- **No notification, confirmation modal or extra respec button** for this one-time migration.
- For safety: **atomic per-manifestation migration, idempotent migration marker/version, recoverable pre-migration backup**, passive re-evaluation, default rank-0 tooltips/upgrade UI and save validation after committing.
- **Neither the migration nor combat skills have been changed in code**. Final schedules and high-rank curves await user sign-off in the separate implementation handoff.

## Cross-class Round 3 — five questions (awaiting user)

**RANK-011 — Greatsword Mastery rank shape.** Would you prefer **8 ranks maximum** for Greatsword Mastery (maintain its current real effects at ranks 1–4, with ranks 5–8 giving gradually better STR/greatsword bonuses), or **12 ranks with real new milestone mechanics spaced among smaller flat-stat ranks**? If you want a different cap, that's fine too.

**RANK-012 — Greatblade total budget.** If Mastery drops from 12 to **8 ranks**, should the four recovered ranks go to **Wide Arc (20 instead of 18)** and **Overhead Breaker (20 instead of 18)**, keeping Greatblade at **78 total possible ranks**? Or would you distribute them differently?

**RANK-013 — Primary spell cadence.** Should main damage spells (Wide Arc, Overhead Breaker, Ember Bolt) be rankable **every class level**, with secondary/utility skills requiring 2–5 levels between upgrades, subject to each skill's first-learn requirement and cap? Any exceptions?

**RANK-014 — Half-Life and Math Barrier schedules.** Shall we **lock Half-Life at ranks 1–8 at Mage levels 18/22/26/30/34/38/42/46**, while leaving Math Barrier at **6/12/18/24/30** and its current formulas? Or do you prefer other gaps?

**RANK-015 — Migration progress and safety.** On first loading a legacy Mage/Greatblade save, I'll **reset only class skill ranks, preserve everything else, silently refund one point per class level, and keep a recoverable backup**, without any notification. Should this migration also apply if a saved character is **already at level 80+**, with surplus points retained for potential future skill additions rather than discarded? (The ordinary +1 point/level can continue above the cap.)

## Decision log

| Date | Question | Result |
| --- | --- | --- |
| 2026-10-18 | Universal rank-0, one class point at level 1 | **CONFIRMED** |
| 2026-10-19 | RANK-001 | Greatblade/Mage level-80 near-spend target **CONFIRMED** |
| 2026-10-19 | RANK-002–003 | First-unlock levels unchanged, no starter ranks **CONFIRMED** |
| 2026-10-19 | RANK-004 | Different caps by function, Math Barrier stays rank 5 **CONFIRMED** |
| 2026-10-19 | RANK-005 | Existing class saves reset/refund once **CONFIRMED** |
| 2026-10-19 | RANK-006 | Skill-specific cadence, no universal upgrade interval **CONFIRMED**; Half-Life 4-level example provisional |
| 2026-10-19 | RANK-007 | Greatsword Mastery fewer ranks **or** sparse special milestones with stat filler **CONFIRMED direction**, cap open |
| 2026-10-19 | RANK-008 | Mage caps 18/12/12/10/8/5/8/7 **CONFIRMED** |
| 2026-10-19 | RANK-009 | Diminishing/capped secondary effects permitted **CONFIRMED** |
| 2026-10-19 | RANK-010 | Automatic silent one-time reset/refund on first legacy save load **CONFIRMED** |
| 2026-10-19 | RANK-011–RANK-015 | Round 3 questions **AWAITING USER** |

# DiceFree Tier II — Greatblade and Mage skill-rank audit

**Date:** 2026-10-19  
**Status:** DESIGN REVIEW / PROPOSALS ONLY — do **not** modify Unity C#, skill assets, save files or existing manifests until the user approves a migration and revised rank progression.  
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
- For **Priest**, the user approved **near-complete skill-point spend capacity around level 80** (= Tier II level 60 + one third of 60), now 78 ranks (4 × 18 + 5 Grace + 1 Revive). **Whether exactly the same spend-down philosophy applies to Greatblade/Mage is pending user approval**; it is likely relevant but must not be silently retroactively imposed.
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

- Explicitly approve the new first-level eligibility list, per-skill rank caps, gates, formulas and handling of existing saves before editing Unity code.
- Treat `classSkills` save records and save/restore validation as authoritative. **Never erase skill ranks, invalidate an existing manifestation or overwrite user progress** because old free ranks now count against a new point budget.
- Decide whether to **grandfather legacy free ranks**, **perform a one-time zero-rank/refund migration**, or support a **user-triggered respec of legacy Tier II manifestations**, including level-1 saves with four legacy free abilities and only one newly allowed point. This cannot be resolved by blindly treating four ranks as purchased at level 1.
- Centralize rank-zero, one-point-at-level-1 semantics for both classes; update point budget, rank gates, passive activation, rank-zero tooltips and in-bar [+] eligibility.
- If expanding ranks, retune Mage Math Barrier and high-rank Greatblade Mastery/Heavy Rhythm; add anti-negative/over-100%-redirect validation, skill point overspend checks and tests around level 1, 6, 18, 30, 42, 60, 80, class fork, class switch, save load and reloading legacy saves.
- Keep old gameplay code untouched during this interview/audit. Current branch's previously verified builds do not validate a hypothetical revised implementation.

## Suggested approaches — NOT approved

**Simple/even rank-count option (first-unlock levels can remain distinct):**
- **Greatblade:** six skills × **13 ranks each** = **78 purchasable skill ranks**, leaving **2** points at level 80. Keep the six existing unique abilities and authored first unlocks. Higher rank effects/cooldowns need revisiting.
- **Mage:** eight skills × **10 ranks each** = **80 purchasable ranks**, leaving **0** points at level 80. Mage Math Barrier's rank-10 protection would be 75% and cost 1 Mana per damage prevented under its current raw formula; even this can have overlong duration, so it requires balance/testing.

**Weighted cap option:** distribute approximately **78–80 total ranks** across the roster, assigning fewer points to niche utilities like Math Barrier or Battle Cry and more to primary active spells. This may create more meaningful progression but requires a deliberate balance table and user review.

The **first-unlock level** of a skill is distinct from the **per-level right to upgrade an already-unlocked skill**. A clean compromise is retaining Greatblade/Mage's **existing first-unlock identities**, then allowing subsequent rank-up at **each level** until its particular cap; no fixed periodic 6/12/18/24 gates. This is NOT yet a user-approved revision to Greatblade's original gated progression.

## Five questions for user — cross-class review round 1

**RANK-001:** Should the **near-spend-down by level 80** target now apply to **Greatblade and Mage**, not only Priest? Or should these classes each have a different desired level for maxing their skill investments?

**RANK-002:** Keep Greatblade's currently approved first skill unlocks (**Wide Arc, Spin Dash, Mastery, Heavy Rhythm at level 1; Overhead Breaker at level 6; Battle Cry at level 18**) but **start all at rank 0 with one level-1 point**? Would you revise any of those unlock levels?

**RANK-003:** Keep Mage's current first unlock order (**Ember/Frost/Weaving/Mana Attunement at level 1; Arc Lightning/Math Barrier at 6; Quicksand at 12; Half-Life at 18**) with zero ranks and the single initial point, or would you change any?

**RANK-004:** Once a skill becomes eligible, should both classes allow investing **one rank per class level into that skill**, without fixed periodic gates, similar to Priest? For the approximate level-80 budget, prefer **uniform caps** (Greatblade 13 each, Mage 10 each), or **weighted different caps by skill importance**?

**RANK-005:** How should **existing playable Greatblade/Mage saved characters with four grandfathered free starter ranks** migrate to the new economy: keep their current builds with grandfathered old starter ranks, or perform a **one-time safe reset/refund** of all invested skill ranks and grant the correct number of class-level skill points to redistribute? No automatic character/save mutation until chosen.

## Decision log

| Date | Subject | State |
| --- | --- | --- |
| 2026-10-18 | All skills rank 0 + one spendable point at level 1 | **CONFIRMED global design**, not yet reflected in Greatblade/Mage code |
| 2026-10-19 | Priest four regular ×18 + Grace5 + Revive1 = 78 spendable at level80 | **CONFIRMED for Priest** |
| 2026-10-19 | Greatblade/Mage free starter ranks, capped early and level-80 underspending quantified from branch code | **VERIFIED current implementation** |
| 2026-10-19 | Proposed common approach and legacy-save migration questions | **USER REVIEW PENDING** |

# DiceFree Tier II — Greatblade and Mage skill-rank audit

**Date:** 2026-10-19  
**Status:** DESIGN REVIEW — Round 1 core policies APPROVED (2026-10-19); weighted per-skill caps and implementation detail choices pending. **Do not modify Unity C#, skill assets, saves or manifestations until class-by-class formulas, gates and migration requirements receive complete implementation sign-off.**  
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

- **RANK-001 CONFIRMED:** **Greatblade and Mage follow Priest's rough level-80 nearly-spend-all-points target**.
- **RANK-002 CONFIRMED:** **Greatblade keeps its original first-eligibility levels**: Wide Arc, Spin Dash, Greatsword Mastery, Heavy Rhythm at level 1; Overhead Breaker at level 6; Battle Cry at level 18. **All ranks begin at 0**; exactly one point available at character level 1, no auto-learned skills.
- **RANK-003 CONFIRMED:** **Mage keeps its original first-eligibility levels**: Ember Bolt, Frost Snare, Elemental Weaving, Mana Attunement at 1; Arc Lightning, Math Barrier at 6; Quicksand at 12; Half-Life at 18. **All ranks begin at 0**, one point at level 1.
- **RANK-004 CONFIRMED in part:** **rank caps must vary with the ability's function and balancing needs**, rather than being uniform. Specifically **Math Barrier remains capped at its existing five ranks and keeps its established identity/formulas** for that cap. **OPEN:** user's answer did *not* explicitly address the other half of question 4, namely whether upgrades can happen **every level after first unlock** or must retain periodic rank-up gates; request separate confirmation. Other skill caps are proposed below, not selected by user.
- **RANK-005 CONFIRMED:** **existing Greatblade/Mage skill allocations must be RESET and REFUNDED** once in a safe migration; no grandfathering four free ranks. No other character or save progress should be lost.

### Weighted skill-rank cap proposal — NOT user-approved

The actual roster and first-learn gates remain unchanged. A class may have a different rank cap for each skill based on effect complexity, utility, performance and playtest scaling.

**Greatblade proposal — 78 ranks total / 80 points by level 80:**

| Ability | First eligible level (confirmed) | Current cap (code) | Suggested cap (proposal) | Why |
| --- | ---: | ---: | ---: | --- |
| Wide Arc | 1 | 6 | **18** | Primary repeatable AoE damage; strong upgrade sink |
| Spin Dash | 1 | 4 | **14** | Mixed damage/mobility, moderate cap to avoid unreasonable travel damage |
| Overhead Breaker | 6 | 5 | **18** | Core Momentum spender/finisher and second main damage sink |
| Battle Cry | 18 | 3 | **8** | Short damage utility buff, fewer ranks |
| Greatsword Mastery | 1 | 6 | **12** | Evolving passive, ranks 2/3/4 keep their specific effects |
| Heavy Rhythm | 1 | 7 | **8** | Momentum economy; avoid flattening resource decay scaling |
| **Total** | | **31** | **78** | **2 potential spare points at level 80** |

**Mage proposal — 80 ranks total / 80 points by level 80:**

| Ability | First eligible level (confirmed) | Current cap (code) | Suggested cap (proposal) | Why |
| --- | ---: | ---: | ---: | --- |
| Ember Bolt | 1 | 5 | **18** | Primary single-target damage sink |
| Frost Snare | 1 | 5 | **12** | Control + damage; freeze duration must be bounded at high rank |
| Arc Lightning | 6 | 5 | **12** | Multiple-target chain; number of jumps must be performance/target capped |
| Quicksand | 12 | 5 | **10** | AoE field and slow, avoid infinite high-rank field stacking |
| Half-Life | 18 | 5 | **8** | Strong DEF reduction DoT; tune debuff scaling |
| Math Barrier | 6 | 5 | **5 — CONFIRMED unchanged** | Keep established duration/redirection/Mana efficiency formulas and maximum |
| Elemental Weaving | 1 | 5 | **8** | Rotation/resistance passive, with controlled aura growth |
| Mana Attunement | 1 | 5 | **7** | Mana pool/regen passive, bounded to prevent excessive sustain |
| **Total** | | **40** | **80** | **0 potential spare points at level 80** |

**Important:** `18` rank caps do **not** mean all ranks retain existing linear damage/movement/CC/resource formulas. High-rank damage, cooldowns, freezes, DEF reduction, chain-count caps, aura values and durations must be tuned. **Math Barrier explicitly remains capped at 5** and retains its intended formulas at ranks 1–5; any proposed changes to Math Barrier's current balance require fresh user approval. Greatblade's passive rank-4 shield/alternate-heal question remains separately open.

### Proposed changes to level gates — awaiting user response

- Preserve the **first eligibility levels** of every class ability, which are now confirmed.
- **PROPOSED, NOT YET CONFIRMED:** once eligible/learned, allow investing a rank each subsequent **class level**, subject to its distinct cap and available skill point, rather than the existing 6/12/18/24 rank-breakpoint arrays. This matches Priest's pacing but replaces an intentionally different previous Greatblade design rule. Ask the user explicitly before changing that aspect.
- A fixed special unlock level may still be used for an authored ability or passive's milestone effect; that does not require rank-ups to be spaced out. For example, Greatsword Mastery's rank-2/3/4 effects remain bound to ranks, not new class-level gates.
- The new zero-rank semantics mean first-learned abilities only become usable once the single starting skill point is actually invested.

### Safe one-time reset/refund — CONFIRMED policy; implementation details open

Upon migration of an existing Mage or Greatblade manifestation:
1. **Preserve a recoverable original save/backup** and confirm that the class ID, saved XP/level, and rank records are valid enough to migrate. Never corrupt or delete a save just because an old saved rank exceeds the new rank-at-level allowance.
2. In one atomic/idempotent transaction **set all its class skill ranks to 0**, preserving class level and all other saved state; make its skill points available equal to **current class level** (e.g. level 1 → 1 point; level 30 → 30; level 80 → 80), not a second bonus/reward. This **refunds all purchased ranks AND former implicit free ranks into the correct class-level pool** without counting them twice.
3. Ensure the normal view and ability provider render unlearned rank-0 skills, allowed [+] purchase buttons, and cooldown/resource/passive effects only at rank ≥1. Recompute passive bonuses and derived stats after reset.
4. Guard the migration with **one-time per-manifestation version/marker**, not one-time per account; loading/playing an already-migrated save must **never reset skills again**. Handle offline/archived/older save revisions as they are encountered.
5. After migration, show an informative in-game notice (proposed wording: *Class skills were reset to match the new progression system. Your skill points have been refunded.*), and let the player allocate as normal; no long-term unlimited respec is implied.
6. Verify rollback/backup, mid-fork state, class switching, both classes on the same account, missing skill records, badly formed saves, and level-1 saves where four free skill ranks were embedded historically. Avoid changes to Novice/Tier I skill point policies unless their actual implementations conflict with the universal rank-0 rule and the user approves a broader migration.

### Code impacts for later implementation (no C# edits yet)

- Greatblade/Mage `StarterRank` calculation should not silently return one for first-unlock-level-1 skills. Update `SpentPoints`/`UnspentPoints`, `Rank`, `RestoreState`, `CanRestore` and ability providers to implement **0 starter ranks, `Level - Σ(ranks)` unspent points** and validated class-level first-eligibility gates.
- Under a revised per-level rank system, `rankLevels` arrays alone no longer express both first-unlock and maximum rank appropriately; use separate authoring fields or equivalent validated rules for **firstLearnLevel** and **maxRank**, keeping purchased-rank history and skill-point total valid on save/load.
- Retune high-rank growth to respect the separate cap of each skill. Avoid negative Mana per damage and >100% redirect; Math Barrier stays five ranks so is safe at its known ranks.
- Update tooltip next-rank previews, action-bar purchase availability, skill data/assets, save-load tests and automated validators after user approves the specific rank schedule/caps.
- **No Unity code changes or save migration were performed by the design audit.**

## Five questions for user — cross-class review round 2

**RANK-006 — Per-level upgrading:** The previous Q4 answer clearly approved **different caps**, but didn't explicitly say whether Mage and Greatblade should have **no periodic upgrade gates**. After an ability first unlocks, can the player invest **one rank per class level** into it, like Priest, until that ability reaches its own maximum cap?

**RANK-007 — Greatblade weighted caps:** How do you feel about **Wide Arc 18, Spin Dash 14, Overhead Breaker 18, Battle Cry 8, Greatsword Mastery 12, Heavy Rhythm 8**? That totals **78 ranks**, close to 80 points at level 80. We can rebalance those individual caps later.

**RANK-008 — Mage weighted caps:** How about **Ember Bolt 18, Frost Snare 12, Arc Lightning 12, Quicksand 10, Half-Life 8, Math Barrier 5 (unchanged), Elemental Weaving 8, Mana Attunement 7**? That totals **80**, matching all points at level 80 while keeping Math Barrier at five ranks.

**RANK-009 — High-rank effect scaling:** For ranks beyond today's code caps, may I propose **diminishing or capped secondary effects** (e.g., Frost Snare's freeze duration, Arc Lightning's jump count, Half-Life's DEF reduction, Greatblade Heavy Rhythm's Momentum efficiency), while still letting damage/healing/rank benefits improve? Math Barrier's five ranks would remain unchanged.

**RANK-010 — Existing-save migration UX:** Should the one-time full reset/refund happen **automatically on first load after the update**, with a clear in-game notification and a recoverable backup? It would preserve class level/XP/gear/quest progress and give the correct one point per class level, including level 1. Or would you prefer a confirmation prompt before resetting skills?

## Decision log

| Date | Subject | State |
| --- | --- | --- |
| 2026-10-18 | All skills rank 0 + one spendable point at level 1 | **CONFIRMED global design**, not yet reflected in Greatblade/Mage code |
| 2026-10-19 | Priest four regular ×18 + Grace5 + Revive1 = 78 spendable at level80 | **CONFIRMED for Priest** |
| 2026-10-19 | Greatblade/Mage free starter ranks, capped early and level-80 underspending quantified from branch code | **VERIFIED current implementation** |
| 2026-10-19 | RANK-001 | Greatblade/Mage share near-complete spend-down target by level80 | **CONFIRMED** |
| 2026-10-19 | RANK-002 | Preserve Greatblade first skill unlocks, rank 0 and 1 initial point | **CONFIRMED** |
| 2026-10-19 | RANK-003 | Preserve Mage first skill unlocks, rank 0 and 1 initial point | **CONFIRMED** |
| 2026-10-19 | RANK-004 | Different max ranks appropriate to skill; Math Barrier keeps max rank 5; per-level upgrading not explicitly answered | **CONFIRMED cap policy; gate detail OPEN** |
| 2026-10-19 | RANK-005 | Existing Mage/Greatblade characters receive a one-time skill reset/refund | **CONFIRMED policy; implementation trigger open** |
| 2026-10-19 | RANK-006–RANK-010 | Cross-class Round 2 questions asked | **AWAITING USER** |
## Decision log

| Date | Subject | State |
| --- | --- | --- |
| 2026-10-18 | All skills rank 0 + one spendable point at level 1 | **CONFIRMED global design**, not yet reflected in Greatblade/Mage code |
| 2026-10-19 | Priest four regular ×18 + Grace5 + Revive1 = 78 spendable at level80 | **CONFIRMED for Priest** |
| 2026-10-19 | Greatblade/Mage free starter ranks, capped early and level-80 underspending quantified from branch code | **VERIFIED current implementation** |
| 2026-10-19 | Proposed common approach and legacy-save migration questions | **USER REVIEW PENDING** |

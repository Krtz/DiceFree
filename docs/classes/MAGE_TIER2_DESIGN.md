# DiceFree — Mage (Tier II) — class design interview

> **Implementation update — 2026-10-16:** The user subsequently authorized a playable Mage prototype in the implementation chat. See [current implementation and provisional balance](MAGE.md) and [verification](../MAGE_TIER2_2026_10_16.md). This interview's confirmed decisions are preserved; open numeric questions are not relabeled as user-approved. Later approvals fix the Slime Dungeon puzzle/return quest and unique-element Weaving with an eight-second inactivity delay. The earlier design-only handoff below describes the interview's historical scope.

**Status:** Playable prototype implemented following later user authorization · Rounds 1–2 decisions preserved · Remaining balance/design questions still open  
**Updated:** 2026-10-16  
**Owner:** User (final decisions); the implementation chat was subsequently authorized to build the playable prototype. Provisional implementation numbers do not close unanswered interview questions.  
**Branch:** `integration/world1-foundation`.

**Prior specifications to reconcile:** [Tier II core classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [earlier Tier II proposals v0.1](../TIER2_CORE_CLASSES_DESIGN_V01.md), [class progression](../CLASS_PROGRESSION.md), [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md), [flexible skill framework](../PLAIN_GEAR_AND_FLEXIBLE_SKILLS.md), and [stat/element rules](../STATS_AND_DAMAGE.md). When older documents differ, the **user decisions in this Mage interview supersede their Mage proposals only**; do not silently alter other classes.

## Interview process and scope

1. Ask **five questions at a time**. Answers are recorded with stable IDs and explicitly marked **CONFIRMED** or **OPEN / PROPOSED**.
2. This is **design/documentation only**. Do not modify Unity/C# assets, stats, spells, quests, scene art or balance in the gameplay branch until another chat is instructed to implement a signed-off design.
3. Complete fantasy, stat growth, weapon rules, Mana economy, skills/passives with rank formulas, target patterns/casting behavior, crowd control, unlock levels, future lineage, quest/mentor, presentation and test acceptance criteria over successive rounds.
4. Preserve the existing Echo manifestation model: eligible **level-30 Magically Touched Novice** branches to **Tier II Mage level 1**, parent is preserved, and the Mage can later reach its Tier II advancement threshold at **level 60**.
5. Follow shared elemental resistance/combat rules, not bespoke Mage-only damage/resistance math. Abilities with unset numbers are **not implementation-ready**.

## Identity and progression

| Aspect | Decision |
| --- | --- |
| Official Tier II class | **Mage** — **CONFIRMED WIZ-001** |
| Parent | **Magically Touched Novice (Tier I), level 30 advancement** — existing system agreement |
| Class advancement | Tier II Mage starts level 1; parent remains as saved manifestation — existing agreement |
| Mage combat role | **Versatile elemental damage dealer**, borrowing the breadth of Magically Touched but leaning more towards dealing damage and experimenting with schools. **Not a healer**, and not a fully specialized endgame caster — **CONFIRMED WIZ-002** |
| Future specializations | **Wizard**, **Sorcerer**, **Sorceress**, **Magus**, and others are possibilities for later tiers; they might each embrace particular elements. **Ideas only**, names/tier, specialization mechanics and even existence not yet locked — **CONFIRMED WIZ-001 (direction), OPEN details** |
| Mentor | Retired mage/brewer in Cornberg, partner of retired Greatblade swordswoman — previously approved direction; exact Mage quest **OPEN** |
| Offensive spell scaling | **INT-only** as previously corrected in Tier II v0.2, unless explicitly revised in interview. Shared VIT/HP, physical/magical defense etc still apply. |
| Resources | **Regular Mana only** (no special second bar) — **CONFIRMED WIZ-005** |
| Spell schools represented | **Fire, Ice, Electric/Lightning, Math, Nature, Radiation** — **CONFIRMED WIZ-003**. Use the game's existing canonical element IDs, not new ad hoc schools. |
| Non-healing identity | No dedicated healing spells in the Mage's intended kit — **CONFIRMED WIZ-002**. Defensive Mana magic remains available. |
| Weapons | **Wand + orb** favored in Tier II v0.2 but not independently finalized in this interview. Orb as real equippable offhand is still a separate decision. |
| Regular skill count | **Six named active spells and two distinct passives** (Elemental Weaving + resistance aura together; Max Mana + Mana regen together). Rank caps, unlock order and point budget are **OPEN**. No universal 4-active/1-passive rule, and **no Tier II ultimate**. |

## Confirmed Mage skill roster — Round 2

**Six active spells + two passives.** Names and elemental identities are user-selected. The rules below are **CONFIRMED** where stated. Effects requiring interpretation, class/ability rank caps, damage coefficients, Mana upfront costs, cast times, ranges and most cooldowns remain **OPEN**.

| # | Active skill | Element | Confirmed effect | Still to specify |
| ---: | --- | --- | --- | --- |
| 1 | **Ember Bolt** | **Fire** | Ranged **single-target** direct Fire attack; quick spell with a **real cooldown**. Explicitly **not spammable**, **not a substitute for the Mage's basic autoattack**. | Exact cooldown, cast/windup, damage, range, whether it burns (original suggestion remains optional). |
| 2 | **Frost Snare** | **Ice** | Short **root/freeze**-style control, as contrasted with Quicksand's longer area denial. | Single target vs area, root/freeze exact CC category, damage, control duration, boss CC limitations. |
| 3 | **Arc Lightning** | **Electric** | Lightning attack that **chains between multiple enemies**, with a **longer cooldown than Ember Bolt**. | Number of jumps, falloff, range, damage, exact cooldown and any status effects. |
| 4 | **Math Barrier** | **Math** | **Self-cast** Mana shield: redirects a percentage of incoming damage from HP to **spent Mana** while active. **Exact rank formulas below.** | Upfront Mana cost, cooldown, mitigation ordering, behavior at partial Mana, stacking and max skill rank. |
| 5 | **Quicksand** | **Nature** | **Ground-targeted persistent area** that **slows and gradually damages enemies**; distinguish from Frost Snare's short control. | Radius, lifetime, movement slow %, DoT cadence, recast limits, cooldown, scaling and element tagging. |
| 6 | **Half-Life** | **Radiation** | **Single-target radioactive damage-over-time effect**. While affected, the target's **DEF is reduced by `5 × Half-Life skill rank`**. | Which DEF channel(s) (Physical, Magical, both), debuff and DoT durations/ticks, exact damage, stacking/refresh, dispel and cooldown. |

**Confirmed combat cadence:** Ember Bolt is a cooldown-bound spell used **alongside** the normal autoattack, not a spam filler replacing it; Arc Lightning's cooldown is longer. No healing spells added.

### Math Barrier — user-confirmed formulas (MAGE-009)

Let `r` denote **Math Barrier skill rank**, starting at rank 1:

- **Duration:** `2 × r` seconds.
- **Redirected share of incoming damage:** `25% + 5% × r` from HP to Mana. This is a portion **prevented from reducing HP**, paid from available Mana.
- **Mana efficiency / cost:** for each **1 HP damage prevented**, consume `2 − 0.1 × r` Mana.
- **At zero Mana:** the protective redirection ceases and the caster **takes the full incoming damage** normally (subject to ordinary defense/resistance as usual). No free shield when Mana is empty.
- **Self-target only**; not an ally barrier.

**Derived examples only**, *not* a decision that Mage skills cap at rank 6:

| Barrier rank | Duration | Redirected damage | Mana per 1 HP protected |
| ---: | ---: | ---: | ---: |
| 1 | 2 s | 30% | 1.9 |
| 3 | 6 s | 40% | 1.7 |
| 6 | 12 s | 55% | 1.4 |

**OPEN technical details:** Determine whether the redirect uses damage **after** ordinary damage mitigation (shared mana-shield convention suggests yes); if Mana runs out **partway** through a hit, determine partial protection vs no protection for that hit. Do not accidentally make unredirected damage disappear: the normal HP fraction always applies. Clarify whether the barrier vanishes immediately at 0 Mana or remains active and resumes if Mana regenerates during its duration. Rank ceiling must keep cost positive and percentages within valid bounds, without silently imposing a rank cap.

### Passive A — Elemental Weaving (combined with elemental resistance aura)

**CONFIRMED MAGE-010:**
- **One passive**, not two, combining the prior resistance-reduction aura with the elemental-rotation benefit.
- The **aura** reduces the elemental resistance of nearby enemies by **1 percentage point per passive skill rank** (at rank 1, −1%; rank 5, −5%). Aura radius, exact affected schools, boss eligibility and same-aura stacking with other Mages are **OPEN**.
- Casting/mixing **different elements** grants a rotational benefit: **reduces the cooldowns of other spells by 0.2 seconds** and grants **+5% damage per element**. These are the user's exact proposed units, *not* automatically 0.2 × rank or 5% × passive rank.
- **OPEN:** what qualifies as a new/different element; whether a successful hit or a completed cast triggers it; whether each newly distinct school adds another +5%, and the maximum number of elements/bonus stacks; buff duration/expiration and reset on same-school casts; whether Math Barrier counts; whether every switch reduces each other spell's remaining cooldown or only the next cast's cooldown; whether this 0.2 s occurs per switch/element and how to prevent same-frame/proc recursion; whether the aura and rotation improve with rank independently.
- Elemental passives do **not** automatically rewrite the universal elements' inherent effects.

### Passive B — Mana Attunement (working name)

**CONFIRMED MAGE-010:** a **separate passive** raises both **maximum Mana** and **Mana regeneration**. User approved both effects together; the former Magically Touched Novice also had Mana Attunement but Mage progression does **not** automatically inherit numeric ranks from Tier I.

**OPEN:** passive's actual name, flat vs percentage increases per rank, rank cap, interaction with equipment/INT and whether any ally Mana regeneration aura persists into Mage (not specified here; do not inherit automatically).

## Existing systems and design constraints

- DiceFree uses STR, AGI, INT, SPR, VIT, separate Physical/Magical Defense with diminishing returns and non-manually allocated primary attributes. Offensive Mage spells follow the existing **INT-only** design direction.
- Damage schools and resistances already exist and should include the six schools above; default elemental resistances and defense rules are defined in [stat rules](../STATS_AND_DAMAGE.md).
- Magically Touched Novice is intentionally broad with offense, Mend, utility and Mana passive. Mage becomes more offense/element-oriented with **no real healing rotation**.
- The shared ability framework supports variable actives and passives, with ordinary skills across multiple pages if needed. Tier IV+ quest ultimate slot is reserved and **not** assigned to Tier II Mage.
- Shared confirm-to-cast, ground-target reticles, self-casting, cooldown UI, buff statuses, movement rules, and tooltip math should be reused for actual implementation.
- Existing proposed **wand + orb**, the retired Cornberg mage-brewer, and high-elf caster visual inspiration remain planned directions, not new final decisions here.

## Round 1 — answers recorded on 2026-10-15

- **WIZ-001 — CONFIRMED:** Tier II **Mage**. Wizard sounds like a stronger later class; later options might be Wizard/Sorcerer/Sorceress/Magus, perhaps school-specific. Future graph **OPEN**.
- **WIZ-002 — CONFIRMED:** Mage is versatile, strongly oriented toward elemental damage; little/no healing; a stepping stone toward specialization instead of ultimate caster mastery.
- **WIZ-003 — CONFIRMED:** Six actives **Ember Bolt (Fire), Frost Snare (Ice), Arc Lightning (Electric), Math Barrier (Math), Quicksand (Nature), Half-Life (Radiation)**; elemental resistance reduction aura at **−1 percentage point per rank**. Maybe separate Mana regen and elemental combo passive, not yet mechanically finalized.
- **WIZ-004 — CONFIRMED:** Math Barrier is self-cast and redirects a **percentage of damage to Mana**. Exact ratios, costs and depletion behavior **OPEN**.
- **WIZ-005 — CONFIRMED:** Only regular Mana; a passive rewarding **mixing elements** is required. Trigger/bonus **OPEN**.

## Round 2 — answers recorded on 2026-10-15

- **MAGE-006 — CONFIRMED:** Ember Bolt is a quick single-target Fire spell with a **nontrivial cooldown**, not a spammable autoattack replacement. Arc Lightning chains between enemies, with an **even longer cooldown**. Ember burn is *not yet locked*.
- **MAGE-007 — CONFIRMED:** Frost Snare is short Ice crowd control; Quicksand is a persistent Nature slow-and-damage ground zone. Exact roots/freezes and numeric CC are still **OPEN**.
- **MAGE-008 — CONFIRMED:** Half-Life is a **single-target Radiation DoT**, applying **−5 DEF per skill rank**. Defense channel(s), duration, DoT damage/ticks and reapplication rules **OPEN**.
- **MAGE-009 — CONFIRMED:** Math Barrier self-cast: duration `2r` seconds; redirects `25% + 5r%` of incoming damage from HP to consumed Mana; each prevented HP consumes `2 − 0.1r` Mana. When Mana is empty take **full damage**. Partial-Mana hit, rank caps and standard mitigation ordering still **OPEN**.
- **MAGE-010 — CONFIRMED:** Combine elemental resistance aura and Elemental Weaving into **one passive**; keep +Max Mana and +Mana regeneration in a **second passive**. Elemental Weaving reduces other spells' cooldowns by **0.2 sec** and increases damage by **5% per element** when mixing elements. Exact trigger, stack/count, duration and skill rank scaling **OPEN**.

## Round 3 — five questions (awaiting answers)

**MAGE-011 — Base attributes and growth.** The Tier I Magically Touched Novice balance baseline at level 1 is **VIT 10 / STR 5 / AGI 5 / INT 13 / SPR 13**, with average per-level growth of **+1 VIT, +0.5 STR, +0.5 AGI, +2 INT, +2 SPR**. Mage should be a noticeably stronger **INT-focused Tier II** class with limited SPR emphasis. What level-1 stat spread and per-level growth do you want? You can state proportions/relative importance now and finalize exact numbers in a later pass.

**MAGE-012 — Mana pool, regeneration and passive growth.** Should Mage start with substantially more maximum Mana/regen than Magically Touched, and should **Mana Attunement** grant **flat bonuses**, **percent bonuses**, or both? Should that passive only buff the Mage or also emit a small ally Mana-regeneration aura like the Tier I version?

**MAGE-013 — Starting kit and unlock timeline.** Which of the **six active spells** and **two passives** should already be learned at **Mage level 1**, and which should unlock later (up to level 60)? Should all rank-1 starting skills be free, like Greatblade's beginning kit, with points spent on later ranks?

**MAGE-014 — Elemental Weaving: exact sequence.** Suppose you cast **Ember Bolt → Frost Snare → Arc Lightning**. Should this earn **+5%, then +10%, then +15%** damage according to the number of distinct elements used, or is a switch always a fixed **+5%**? How long does the bonus last, when does it reset, and is the **−0.2 s cooldown reduction** applied to **every other spell each time you switch elements**? Should Math Barrier count?

**MAGE-015 — Wand/orb and basic attack.** Do you want to commit to **wand (main hand) + equippable orb (offhand)** for Mage? What should its normal autoattack look and feel like: elemental projectile based on wand/orb, fixed non-elemental magical bolt, or something else? Any alternative weapon types allowed?

## Later design rounds — topics not yet finalized

1. Base HP, base P-DEF/M-DEF, movement, class elemental resistances, stat-to-Mana resource formula and exact skill passive values.
2. Ember Bolt / Frost Snare / Arc Lightning / Quicksand / Half-Life detailed target, damage, dot/CC duration, tick cadence, cooldown, casting and rank-specific effects.
3. Half-Life **DEF channel(s)**; Math Barrier partial-Mana hit / interruption rules, and all rank caps (including safe conversion limits).
4. Skill points, skill ranks, level gates 1–60, cooldown timing, mana costs, UI and solo/party balancing.
5. Mentor advancement quest, art silhouette, VFX readability, ally aura stacking and anti-exploit/multiplayer acceptance tests.

## Historical implementation handoff — before later prototype authorization

The other DiceFree implementation chat should use this document as the single Mage source of truth once all decisions and numeric tables are approved. Implementation will require data-driven skill definitions, class-stats/element/weapon definitions, cast animation/VFX placeholders, passive state and auras, tests for Math Barrier partial-Mana cases, aura stacking, rotations, progression, persistence, damage/cost/cooldown rank calculations, advancement integration, and NPC mentor behavior. **Do not begin implementation while this is in interview status.**

## Decision log

| Date | ID | Decision | Status |
| --- | --- | --- | --- |
| 2026-10-15 | WIZ-001 | Tier II name **Mage**; Wizard, Sorcerer, Sorceress, Magus reserved as possible future paths | **CONFIRMED**, details open |
| 2026-10-15 | WIZ-002 | Versatile elemental, damage-focused, non-healer stepping stone | **CONFIRMED** |
| 2026-10-15 | WIZ-003 | Six named elemental actives and −1 percentage-point/rank enemy resistance aura | **CONFIRMED**, mechanics open |
| 2026-10-15 | WIZ-004 | Self-only Math Barrier substitutes Mana for a % of HP damage | **CONFIRMED**, conversion open |
| 2026-10-15 | WIZ-005 | Regular Mana; rewarding different elements with passive | **CONFIRMED**, bonus open |
| 2026-10-15 | MAGE-006 | Ember Bolt cooldown-bound single target; Arc Lightning chains with longer cooldown | **CONFIRMED**, exact values open |
| 2026-10-15 | MAGE-007 | Frost Snare short Ice control; Quicksand lasting Nature slow/DoT ground area | **CONFIRMED**, exact effects open |
| 2026-10-15 | MAGE-008 | Half-Life Radiation single-target DoT; −5 DEF per skill rank | **CONFIRMED**, defense channel open |
| 2026-10-15 | MAGE-009 | Math Barrier: self-only, 2r sec, (25+5r)% redirect, (2−0.1r) Mana/HP absorbed; no Mana = full damage | **CONFIRMED**, partial-hit/cap open |
| 2026-10-15 | MAGE-010 | Combine resistance aura + Elemental Weaving; second passive Max Mana + regeneration | **CONFIRMED**, rotation semantics open |
| 2026-10-15 | MAGE-011 through MAGE-015 | Round 3 asked | **AWAITING USER** |

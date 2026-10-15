# DiceFree — Mage (Tier II) — class design interview

**Status:** ACTIVE DESIGN · Round 1 decisions recorded · Round 2 awaiting answers · **DESIGN ONLY**  
**Updated:** 2026-10-15  
**Owner:** User (final decisions); other DiceFree development chat will implement **only after the completed design is approved**.  
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
| Regular skill count | Six named actives + passive concepts below. No fixed 4-active/1-passive rule and **no Tier II ultimate**. Flexible skill pagination must follow current shared implementation. |

## Confirmed active skill roster — mechanics still to design

**Six active skills and their elements are confirmed.** Their exact effects beyond the statements below, targeting, cast/windup, intervals, cooldown, costs, ranges and level/rank progression remain open.

| # | Skill | Element | Confirmed identity | Open behavior |
| ---: | --- | --- | --- | --- |
| 1 | **Ember Bolt** | **Fire** | Offensive Mage spell | Projectile vs instant, cast length, single/AoE, DoT/burn, damage |
| 2 | **Frost Snare** | **Ice** | Offensive/control Mage spell | Snare/root vs slow, area geometry, damage, boss CC behavior |
| 3 | **Arc Lightning** | **Electric** | Offensive Mage spell | Chain targets vs straight line, hit count, stun/control, damage |
| 4 | **Math Barrier** | **Math** | **SELF-CAST** damage-to-Mana shield — **CONFIRMED WIZ-004** | For a portion of incoming damage, **Mana is lost in place of HP** (not Mana generated as a reward). Exact percentage, whether mitigation occurs before conversion, Mana-per-damage rate, duration/cooldown, what happens at 0 Mana, and stacking are **OPEN**. Preserve generic Mana/resource-shield resolution conventions unless explicitly overridden. |
| 5 | **Quicksand** | **Nature** | Additional Mage spell | Ground zone vs unit, movement slow/root/pull, damage/DoT, duration |
| 6 | **Half-Life** | **Radiation** | Additional Mage spell | Damage/DoT/decay/defense debuff/other; no behavior is inferred from the name |

The **Math Barrier** is a self-protection effect paid through Mana, not a caster healing skill. Do **not** invent an Arcane Ward, ordinary absorb shield or an ally-targeted version.

## Passives — confirmed concepts, unconfirmed allocation and effects

| Concept | Decision |
| --- | --- |
| **Elemental-resistance-reduction aura** | **CONFIRMED WIZ-003:** nearby enemies suffer an elemental resistance penalty equal to **−1 percentage point per skill rank**, i.e., rank 1 −1%, rank 2 −2%, rank 3 −3%, etc. Aura radius, max rank, affected elements, application rules, and overlap with another Mage's aura are **OPEN**. The user's wording was "level of skill x -1% elemental resistance"; do not assume the ultimate rank cap yet. |
| **Elemental combo / Elemental Weaving** | **CONFIRMED WIZ-005:** mixing different spell elements should give a beneficial passive bonus. Mechanics, trigger, benefit, duration, stacking, number of elements needed and whether Math Barrier triggers it are **OPEN**. |
| **Mana regeneration passive** | **SUGGESTED / OPEN:** The user proposed *maybe* Mana regeneration as part of the passive arrangement. It is **not yet confirmed** as a separate passive or an aura rider. |
| **Passive organization** | **OPEN:** the above may be individual passives or some effects could share an evolving passive with rank milestones; clarify before implementation. |

**Note on terminology:** “−1% elemental resistance per rank” means subtracting one **percentage point** from an enemy's elemental resistance per rank, not a 1% relative multiplication. If the user intends a different formula, correct it on next review.

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

## Round 2 — next five questions (awaiting answers)

**MAGE-006 — Fire and Lightning damage identity.** Should **Ember Bolt** be a quick single-target filler (possibly applying a burn), while **Arc Lightning** jumps across several enemies, or do you imagine different mechanics for either of them? What should distinguish Fire damage from Lightning?

**MAGE-007 — Ice vs Nature control.** We have both **Frost Snare (Ice)** and **Quicksand (Nature)**. How should they differ? For example, Frost Snare could be a short root or freeze on a target, while Quicksand is a longer ground-targeted zone that slows and gradually damages enemies. Are those the right roles, or something else?

**MAGE-008 — Radiation / Half-Life.** What do you want **Half-Life** to do? Potential directions include a single-target Radiation damage-over-time effect that decays, a spreading radioactive debuff, or a hit that reduces enemy defenses/resistances. Which feels right for Mage?

**MAGE-009 — Math Barrier resource conversion.** Roughly **what percentage** of incoming damage should Math Barrier redirect to Mana (e.g. 30%, 50%, 75%)? Should **1 prevented HP damage cost 1 Mana**, or another rate? If Mana runs out during a hit, should the unabsorbed portion hit HP normally?

**MAGE-010 — Aura and elemental-combo passives.** Should the **−1% enemy elemental resistance per rank aura** be a passive of its own, and **Elemental Weaving** another passive, or should they merge as one evolving passive? When Mage uses *different* elements in succession, should the reward be bonus damage, reduced Mana costs, regeneration, or another effect? Does your “maybe Mana regen” belong in its own passive?

## Later design rounds — topics not yet finalized

1. Starting attributes and per-level growth (VIT, STR, AGI, INT, SPR), base HP, defenses, movement, starting/cap/regeneration of Mana and class resistance.
2. Baseline attack, wand/orb equipment compatibility, alternate equipment choices, visual body/race/casting style and mentor quest.
3. Every skill's exact target pattern, range, radius/jumps, cast time, cost, cooldown, base coefficient, rank-up effects and any control/resistance interaction.
4. How many abilities are learned free at Mage level 1, when the remaining abilities/passives unlock through level 60, passive ranks/points and saved progression.
5. Elemental rotation edge cases, PvE group aura stacking, boss crowd-control diminishing returns, interrupt/death behavior, solo viability and 1–4-player party tests.

## Implementation handoff — not authorized yet

The other DiceFree implementation chat should use this document as the single Mage source of truth once all decisions and numeric tables are approved. Implementation will require data-driven skill definitions, class-stats/element/weapon definitions, cast animation/VFX placeholders, passive state and auras, tests for Math Barrier partial-Mana cases, aura stacking, rotations, progression, persistence, damage/cost/cooldown rank calculations, advancement integration, and NPC mentor behavior. **Do not begin implementation while this is in interview status.**

## Decision log

| Date | ID | Decision | Status |
| --- | --- | --- | --- |
| 2026-10-15 | WIZ-001 | Tier II name **Mage**; Wizard, Sorcerer, Sorceress, Magus reserved as possible future paths | **CONFIRMED**, details open |
| 2026-10-15 | WIZ-002 | Versatile elemental, damage-focused, non-healer stepping stone | **CONFIRMED** |
| 2026-10-15 | WIZ-003 | Six named elemental actives and −1 percentage-point/rank enemy resistance aura | **CONFIRMED**, mechanics open |
| 2026-10-15 | WIZ-004 | Self-only Math Barrier substitutes Mana for a % of HP damage | **CONFIRMED**, conversion open |
| 2026-10-15 | WIZ-005 | Regular Mana; rewarding different elements with passive | **CONFIRMED**, bonus open |
| 2026-10-15 | MAGE-006 through MAGE-010 | Round 2 asked | **AWAITING USER** |

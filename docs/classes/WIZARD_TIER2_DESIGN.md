# DiceFree — Tier II Wizard / Mage class design interview

**Status:** DESIGN INTERVIEW IN PROGRESS · round 1 unanswered · **no implementation authorization**  
**Started:** 2026-10-15  
**Maintained in:** `integration/world1-foundation`  
**Design owner:** User decides; this document records confirmed answers and clearly labels open proposals.

> **Important naming ambiguity:** Prior approved Tier II documents name this class **Mage**, while **Wizard** was mentioned as a possible later specialization. The current interview refers to "Wizard" as requested, but does **not** silently override the existing progression graph. Confirm whether Wizard is a Tier II rename or a later class before implementing. Consult [Tier II class directions v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Tier II earlier proposals v0.1](../TIER2_CORE_CLASSES_DESIGN_V01.md), [Class progression](../CLASS_PROGRESSION.md), and the [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md) specification.

## Interview workflow

- Ask the user **exactly five questions per round**, then wait for replies.
- After each reply, capture the answer in this document as a **CONFIRMED** user decision, with date and question ID. Keep any unresolved aspect explicitly **OPEN**. Update existing proposals rather than quietly presenting guesses as decisions.
- Keep stat growth, weapon compatibility, Mana economy, skills/passives and their ranks, ground/unit targeting, cooldowns, range/geometry, elemental interactions, quest unlock and implementation acceptance criteria separate as they become defined.
- Design/documentation **only**. Another development chat will implement later; do not create Unity scenes, assets, classes, code, or auto-balance numbers as part of this interview.
- Prefer meaningful differentiated active/passive effects and a class that is effective solo and useful in a 1–4-player party. Ask follow-up rounds for numerical tuning and edge cases before requesting implementation.

## Existing DiceFree agreements to preserve (not newly decided in this interview)

| Topic | Existing documented decision | Reference |
| --- | --- | --- |
| Advancement | Tier I **Magically Touched Novice**, eligible at **level 30**, advances into a Tier II manifestation that starts at **level 1**; its Tier I parent stays saved/playable. Tier II may advance again at level **60**. | `CLASS_PROGRESSION.md`, `TIER2_CORE_CLASSES_DESIGN_V02.md` |
| Character stats | STR, AGI, INT, SPR, VIT; physical and magical defense exist separately with diminishing returns. Stats are class-authored rather than manually allocated. | `STATS_AND_DAMAGE.md` |
| Offensive scaling | Earlier Mage correction: **INT-only** for its spells/offense; no SPR offensive scaling. VIT and other universal secondary attributes retain their ordinary effects. | `TIER2_CORE_CLASSES_DESIGN_V02.md` |
| Damage schools | Earlier Mage correction: **no Arcane damage/Arcane Ward**; spell schools include **Fire, Ice, Electric, Math**. Default elemental resistance/combat rules are shared with DiceFree, not invented specifically for Mage. | `TIER2_CORE_CLASSES_DESIGN_V02.md`, `STATS_AND_DAMAGE.md` |
| Equipment direction | **Wand + orb** preferred as distinctive Mage hands, with orb as meaningful offhand equipment. Not yet confirmed as a final restriction. T1 Magically Touched presently has a modest staff visual, which need not dictate Tier II weapons. | `TIER2_CORE_CLASSES_DESIGN_V02.md`; current T1 visual work |
| Resource | Existing Mage proposal uses **Mana** and may benefit from alternating elemental schools via passive **Elemental Weaving**; **no second resource agreed**. | `TIER2_CORE_CLASSES_DESIGN_V02.md` |
| Proposed kit — NOT APPROVED | **Ember Bolt** (Fire single target); **Frost Snare** (Ice slow/area); **Arc Lightning** (Electric chaining); **Math Barrier** (Math defensive shield); **Elemental Weaving** (alternating schools passive). These are interview starting points only. | `TIER2_CORE_CLASSES_DESIGN_V02.md` |
| Mentor | Existing retired **mage/brewer in Cornberg**, partner of the retired Greatblade swordswoman, proposed as Tier II class mentor. The advancement quest is not defined. | `TIER2_CORE_CLASSES_DESIGN_V02.md` |
| Skill architecture | Mixed active/passive ability counts, varying per-class rank gates, multiple ordinary skill pages allowed; no universal 4-active/1-passive rule. Tier II must **not** get a Tier IV+ quest ultimate early. Confirm targeting uses the shared explicit reticle/confirmation system. | `PLAIN_GEAR_AND_FLEXIBLE_SKILLS.md`, `GREATBLADE_PROTOTYPE_DESIGN_V03.md`, `TIER2_CORE_CLASSES_DESIGN_V02.md` |
| Visual direction | Caster silhouette must read clearly at the WC3-style game camera; any **high elf Wizard** art direction discussed previously is a possible direction, not a fixed mechanical race restriction in this document. | Earlier class discussions; not yet formally locked here |

## Round 1 — five questions (awaiting user's answers)

**WIZ-001 — Name and place in advancement tree.** Does **Wizard** replace **Mage** as the **Tier II class** from Magically Touched Novice, or should **Mage** remain Tier II and **Wizard** be a later Tier III specialization? If Tier II Wizard, the existing Cornberg mage-brewer can still be its mentor unless you change that later.

**WIZ-002 — Combat fantasy.** Should this be a versatile, all-four-elements spellcaster, a combo/element-rotation caster, a hard-cast artillery caster with big telegraphs, or a hybrid of those? What should make Wizard distinct from Magically Touched Novice and later specialist mages?

**WIZ-003 — Spell lineup.** How do you feel about the proposed starting actives **Ember Bolt / Frost Snare / Arc Lightning / Math Barrier**? Which would you keep/change/remove and what other active spell(s) do you want available early or unlockable on the road to Tier II level 60? No universal four-spell cap.

**WIZ-004 — Math defense.** What should **Math Barrier** actually do? Ideas to decide between (not decisions): ordinary INT-based absorb; shield that reduces incoming hits by a fixed/rounded amount; or protection with a special Math-element reaction/confusion effect. Should it target only the Wizard, or also allies?

**WIZ-005 — Resources and passives.** Should Wizard keep **Mana only**, with **Elemental Weaving** rewarding switching spell schools, or have a distinctive second resource or different passive? If alternating elements matters, should it be a gentle bonus or a central mechanic?

## Pending design sections — do not implement from placeholders

### Identity, role and branch
**OPEN:** official class name, tier and graph position, mentor story / unlock quest.

### Starting stats and progression
**OPEN:** level-1 values and per-level growth for STR, AGI, INT, SPR, VIT; base HP, P-DEF, M-DEF, movement, mana starting/cap/regeneration, class resistances. Preserve agreed INT-only offensive scaling unless user changes it explicitly.

### Basic attack and equipment
**OPEN:** wand + orb restriction (orb customizable offhand?), attack elements, speed, base damage, animation identity, alternate weapons, high elf cosmetic race/appearance direction.

### Skill and passive ledger
All entries below are **PROPOSAL ONLY**, not guaranteed loadout or names.

| Working skill | Category | Element | Starting idea | Unlock, ranks, range, target, cast, costs, cooldown |
| --- | --- | --- | --- | --- |
| Ember Bolt | Active attack | Fire | Regular ranged direct spell | **OPEN** |
| Frost Snare | Active control | Ice | Slowing/rooting area | **OPEN** |
| Arc Lightning | Active attack | Electric | Chain/jump across enemies | **OPEN** |
| Math Barrier | Active protection | Math | Protection whose math-specific identity remains to define | **OPEN** |
| Elemental Weaving | Passive | Multi-element | Changing schools helps efficiency/power; avoid enforced rigid combo until approved | **OPEN** |

### Elemental interactions and edge cases
**OPEN:** stacking/refreshing, repeated cast vs switch reward, how protection counts for weaving, damage resistance, bosses and crowd-control resistance, solo casting loop.

### Ability progression and later tiers
**OPEN:** level-1 free known ranks; skill learn/growth breakpoints 1–60, number of actives/passives, rank progression, possible Tier III lines. **Do not allocate a Tier II ultimate**.

### Implementation handoff checklist (once design signed off)
- Distinct class ID/display title and Tier I -> II advancement edge; preserve parent save and level-1 child.
- Exact equipment, basic attack and stat/resource formulas, with rank-by-rank skill data.
- Skill target patterns, cast/interrupt, FX placeholders, cooldowns and protections against abuse.
- Art silhouette/rig/equipped-item sockets, class portrait, NPC mentor.
- Regression acceptance criteria (solo combat, spell resistance, allies, advancement, persistence, tooltips, class switching, damage numbers).
- Explicit user's design approval before another chat converts this document into Unity features.

## Decision log

| Date | IDs | Decision | State |
| --- | --- | --- | --- |
| 2026-10-15 | WIZ-001 through WIZ-005 | First batch asked; awaiting answers | **OPEN** |


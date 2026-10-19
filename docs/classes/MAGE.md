# Mage — Tier II

> **Skill progression design update (2026-10-19; NOT YET IMPLEMENTED):** the user confirmed that every Mage skill must start at **rank 0** with **one spendable point at character level 1**, preserving current ability first-unlock levels; rank caps should vary according to skill function and aim to spend almost all 80 level-80 points. **Math Barrier remains capped at rank 5.** Approved new per-ability maximum ranks are **Ember Bolt 18 / Frost Snare 12 / Arc Lightning 12 / Quicksand 10 / Half-Life 8 / Math Barrier 5 / Elemental Weaving 8 / Mana Attunement 7**. Rank upgrades use **skill-specific level intervals**. **Confirmed:** Ember Bolt may rank up every class level; Half-Life ranks unlock at Mage levels **18/22/26/30/34/38/42/46**, and Math Barrier retains existing **6/12/18/24/30** gates. Remaining Mage rank-gate schedules have now been selected as **provisional tested level-by-level playtest baselines**, detailed in the audit: Frost Snare every 7 levels from 1, Arc Lightning every 6 from 6, Quicksand every 7 from 12, Elemental Weaving every 11 from 1, Mana Attunement every 13 from 1. These schedules preserve a feasible purchase every class level through Mage level 80. **Designer-selected provisional secondary-effect ceilings:** Frost Snare freezes up to **2.5s**, Arc Lightning chains up to **8 distinct hostiles**, Half-Life debuffs at most **−40 Physical/−40 Magical DEF**, Quicksand slows up to **40%**, and Elemental Weaving's own aura reaches **−8 percentage points** at its rank-8 cap. These ceilings are playtest proposals (not live), damage continues scaling, and Math Barrier remains unchanged. Numerical balance is not finalized. **CONFIRMED global UX:** max spendable allocation is min(class level, 80), and only show the point reminder when a rank is actually available for purchase. Existing Mage saves must receive an **automatic SILENT one-time skill reset and refund on first loading the legacy manifestation** (preserving levels/items/quests/etc.; safe backup and idempotent migration; no notification). This document's *free starting rank-1, five-rank-each and rank-gate tables describe the current playable implementation*, not the approved redesign. See [the Greatblade/Mage skill-rank audit](../TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md). Do not treat these policy changes as shipped until code, tests and save migration are completed.


Mage is an INT-based general elemental caster. Tier III can specialize later. Stable class ID: `class.mage`. A wand and a floating orb are the default focus visuals; all existing class-neutral test equipment remains usable. No ultimate is assigned before Tier IV.

## Advancement

As a level-30 Magically Touched Novice, speak to Cornberg's brewer / retired mage. Accept **A Mind for Magic**, solve the five-ring enchanted-tree puzzle in the Slime Dungeon, then return to her. Solving the puzzle makes the quest ready; only the mentor's turn-in advances the manifestation to a level-1 Mage. Earlier puzzle completions and boss kills do not substitute for this accepted trial. The parent manifestation retains its own progress.

## Abilities

Six active abilities and two passives use the extensible class ability provider. They are not limited to four actives.

| Ability | Element | Behavior | Rank unlock levels |
|---|---|---|---|
| Ember Bolt | Fire | Single-target projectile with its own cooldown | 1, 6, 12, 18, 24 |
| Frost Snare | Ice | Targeted damage and brief freeze using shared stun rules | 1, 6, 12, 18, 24 |
| Arc Lightning | Electric | Chains to distinct nearby enemies, with damage falloff | 6, 12, 18, 24, 30 |
| Quicksand | Nature | Ground field with damage ticks and a slow | 12, 18, 24, 30, 36 |
| Half-Life | Radiation | Single-target damage over time and defense reduction | 18, 24, 30, 36, 42 |
| Math Barrier | Math | Self-only damage redirection to Mana | 6, 12, 18, 24, 30 |
| Elemental Weaving | Passive | Unique-element stacks, cooldown reductions and resistance reduction | 1, 6, 12, 18, 24 |
| Mana Attunement | Passive | Maximum Mana and regeneration | 1, 6, 12, 18, 24 |

Level-1 abilities begin at rank 1 for free. Subsequent learned ranks cost one skill point; each class level after the first grants one point.

### Agreed rules

- Each unique elemental spell adds one Weaving stack. Repeating an element refreshes the shared timer without adding a duplicate stack.
- Eight seconds without an elemental cast starts decay. Damage ticks, lightning jumps, additional field targets and basic attacks never count as additional casts.
- The combined passive also reduces nearby hostile resistance to all six Mage schools by 1 percentage point per rank, without requiring a hit.
- Each Weaving stack adds 5% spell/basic damage. Each elemental cast reduces other Mage cooldowns by 0.2 seconds.
- Ember Bolt retains a cooldown. Wand Spark remains a separate, non-elemental INT-based basic attack. Arc Lightning has a longer cooldown than Ember Bolt.
- Half-Life applies **−5 DEF × rank**. The current prototype applies this to both defense channels and refreshes the same caster’s DoT rather than stacking duplicates; those channel/refresh policies remain provisional.
- Math Barrier lasts **2 × rank seconds**, redirects **(25 + 5 × rank)%** of post-shield incoming damage and costs **(2 − 0.1 × rank) Mana per damage absorbed**. Partial Mana absorbs only the affordable portion. Zero Mana leaves incoming damage unchanged. Healing does not consume Mana.
- Skills, gear and current Mana persist per manifestation. Stacks, cooldowns, fields, barrier and owned defense/resistance debuffs reset on manifestation load.

### Provisional playtest balance

These values, rank caps, rank unlock levels and starting-skill choices are implemented starting points, not additional user-approved balance decisions. Barrier shield ordering and affordable partial absorption are also current implementation policies.

| Value | Current setting |
|---|---|
| Level-1 attributes | VIT 11 / STR 5 / AGI 8 / INT 18 / SPR 9 |
| Attribute growth per level | VIT +1.5 / STR +0.5 / AGI +0.75 / INT +3 / SPR +1.5 |
| Skill rank cap | Five; unlock levels listed above |
| Half-Life channels | Both Physical and Magical DEF; same-caster refresh |
| Weaving decay | One oldest element per second after the 8-second delay |
| Resistance aura | 8m radius; six Mage schools; strongest overlapping copy wins; applies to bosses |
| Frost Snare | 0.75 + 0.25 × rank seconds of freeze through the shared stun system; respects ordinary resistance and hard immunity |
| Mana Attunement | +30 maximum Mana and +0.5 Mana/s per rank |
| Mana pool | 80 + 10 × INT, plus passive modifiers |
| Natural Mana regeneration | 2 + 0.04 × INT per second, plus passive modifiers |
| Cooldowns | Ember 5s; Frost 8s; Arc 12s; Quicksand 14s; Half-Life 10s; Barrier 18s |
| Spell range | 18m; Arc jumps within 6m |
| Quicksand | 3.5m radius, 4 + rank seconds |
| Half-Life | 6 + 2 × rank seconds |
| Arc targets | 3 + floor((rank − 1) / 2), 80% damage per jump |

Spell coefficients, costs and rank gates are centralized in `MageProgressionRules.cs`. Elemental attack definitions and presentation tuning are under `Assets/_DiceFree/Settings/Skills/Mage`. Tooltips expose the live values.

## Visual provenance and limits

The approved reference is LittleSimple's Stylized Modular Characters – Fantasy Mage Pack. That commercial pack is **not installed or included**. The current Mage uses the existing locally licensed humanoid underneath an original runtime-authored pointed hat, fitted split coat, tapered cape, wand and floating orb. It is a replaceable placeholder; it does not copy or claim to contain the referenced commercial geometry.

Existing Humanoid idle/walk/attack animations are reused. Gear hides the relevant default pieces; two-handed Wolf-Fang Greatsword hides both wand and orb. Menu cards use the same equipped class presentation and portable fallback behavior.

The multiplayer development lab remains an isolated Novice/basic-attack test. This implementation does not add production multiplayer replication of Mage skills, progression or effects.

Confirmed interview decisions are preserved in [Mage interview](MAGE_TIER2_DESIGN.md). The implementation uses canonical Nature for Quicksand. This chat additionally approved the puzzle quest, unique-element refresh and eight-second inactivity delay; remaining numeric choices above are provisional playtest values.

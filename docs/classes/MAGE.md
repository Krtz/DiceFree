# Mage — Tier II

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

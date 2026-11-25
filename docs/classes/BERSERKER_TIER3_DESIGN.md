# DiceFree — Berserker Tier III (Greatblade evolution) — design interview

**2026-11-25 — Round 1 user-approved identity; Round 2 open.** This is a **DESIGN document** and NOT a declaration of Unity implementation or balance verification.

## Lineage and core identity (Round 1 USER-APPROVED)

- **Parent:** Greatblade (Physically Blessed Tier II). **Berserker is primarily a DIRECT UPGRADE / continuation of Greatblade**, not an unrelated dual-wield, bleeding, tank or spell-sword class. Existing Tier II Greatblade remains separately preserved under the shared Tier III advancement/manifestation framework; full unlock quest and starting stats not yet authored.
- **Primary weapons:** **two-handed greatswords**. Do not silently permit dual-wielding or replace the greatsword identity; future item-specific restrictions are separate.
- **Resource:** **REPLACE Momentum with Rage**, generated when the Berserker **DEALS AND RECEIVES DAMAGE**. The detailed gain coefficients, what counts as damage, caps, decay, cost/spend mechanisms, death/out-of-combat handling and prevention of self-farming remain **OPEN**. Do not accidentally retain Momentum as a second simultaneous resource.
- **Core risk/reward:** **LOWER CURRENT HP increases outgoing damage.** User chose HP-sensitive damage over 'high Rage reduces armor', HP-spending casts or primarily burst windows. Exact damage function, HP threshold, minimum HP, cap, healing behavior, and how Rage interacts with low-HP multiplier are **OPEN**. Do not silently add a defense penalty or HP costs.
- **Combat focus:** **SUSTAINED melee damage** during longer fights. Avoid interpreting the class as primarily burst, huge AoE or a tank; normal melee cleave remains available as inherited Greatblade identity.
- **RETAIN four Greatblade abilities/identities:** **Spin Dash, Overhead Breaker, Heavy Rhythm, Wide Arc**. Evolutions of rank scaling, exact names and mechanics for these are yet to be designed, but preserve their core recognizable use. The user did NOT select Battle Cry; it is **not part of the currently approved carried-over roster**. The current Tier II Greatblade also has Greatsword Mastery; the Round 1 multiple-choice did not ask about it, so **whether a Berserker evolution keeps Greatsword Mastery is UNDECIDED**, not expressly rejected.
- User requests **assistant design guidance** on complementary Greatblade Tier III branches, to keep Berserker's identity distinct, and notes this melee archetype is not their usual playstyle. Offer proposals as **noncanonical ideas for comparison only** rather than inventing approval.
- **150 total Tier III skill ranks** is the current suggested consistency goal (as with other Tier III classes), NOT separately reconfirmed by the user's answers in Round 1. No skill caps, unlock gates, attribute numbers or advancement quest approved.

### Existing Greatblade reference (Tier II, not automatically Berserker caps)

The current class-progression documentation lists Greatblade Tier II skill caps **Wide Arc20, Spin Dash16, Overhead Breaker18, Battle Cry8, Greatsword Mastery8, Heavy Rhythm8** = **78**. This is background only. Tier III purchases start from rank0 under established Tier III principles unless otherwise explicitly designed; do not just copy 78 parent skill ranks.

### Possible sibling Greatblade Tier III directions — DESIGNER IDEAS, NOT APPROVED

1. **Berserker:** direct, sustained **greatsword melee damage**, Rage from combat and stronger offense at low HP; keeps Wide Arc, Spin Dash, Overhead Breaker and Heavy Rhythm. This is the **only confirmed** sibling.
2. **Blademaster / Sword Saint:** **technical greatsword duelist**, precise timing, parries/counterstrikes and punish windows, consistency at high HP, no Rage or health-risk focus; possibly weapon mastery or combo-based precision. Distinct from Berserker by execution rather than self-endangerment.
3. **Juggernaut / Dreadnought:** **armored unstoppable greatsword bruiser**, stagger resistance, damage absorption and deliberate heavy swings; contrasted with Berserker's risk-heavy offense. Keep separate from Defender's established dedicated shielding/tanking role by emphasizing offense and anti-interruption, not taunt/shield. Could also be a future Tier IV concept.
4. **Warbringer / Warlord:** **party-supporting battle commander** with battle cries, ally melee buffs, team positioning and control. This would be a natural home for Greatblade's *unselected* Battle Cry identity and clearly different from Berserker's personal sustained DPS.

**Avoid canonizing these alternatives or their names without user approval.** Other possibilities (e.g. lifesteal blood knight, elemental sword warrior) can be explored in later discussions.

## Round 2 — suggested five decisions (NOT ANSWERED)

1. **Rage function:** Should Rage primarily *passively increase damage* while held, be *spent* to empower abilities, or support both? Define whether Rage builds from raw/pre-mitigation vs final actual damage dealt/taken; ensure no infinite gain from shields/DoTs without design.
2. **Low-HP damage scaling:** Smooth continuous bonus with missing HP, or meaningful breakpoints (e.g. 75/50/25% HP)? Decide a conservative first-playtest cap; make bonuses update immediately when healed.
3. **Existing ability upgrades:** How should Wide Arc, Spin Dash, Overhead Breaker, and Heavy Rhythm evolve? Should Heavy Rhythm become a **Rage management passive**? Preserve parent identity without blindly copying Momentum text.
4. **Additional abilities:** Should Berserker gain a **survival tool** (e.g. brief damage reduction/lifesteal at low health), a Rage spender finisher, a frenzy attack-speed buff, or a control/anti-interruption move? No fixed roster size until agreed.
5. **Sibling Greatblade evolution priorities:** Which of **Blademaster (technical/parries)**, **Juggernaut (durable bruiser)**, **Warbringer (team commander)**, or an original alternative should be reserved as a *possible later* evolution? This is a differentiation/vision discussion, NOT commitment to designing all branches now.

## Workflow

**After EVERY answered design-question round:** update relevant GitHub design docs, commit, **push** to active agreed development branch and report commit SHAs. Preserve concurrent Codex work; do not alter Unity source/assets during design-only rounds without explicit implementation request.

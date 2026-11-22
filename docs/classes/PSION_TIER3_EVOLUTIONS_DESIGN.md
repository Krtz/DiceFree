# DiceFree — Psion Tier III evolutions — design interview

**Status:** Tier III Round 1, **USER-CONFIRMED** on **2026-11-22**. These classes are **DESIGN ONLY**, **not implemented in Unity**. The two descendant names and core identity are approved; precise ranges, ability rosters, rank gates and advancement quests remain **OPEN**.

**Authoritative prerequisites:** [Psion Tier II](PSION_TIER2_DESIGN.md), [Class Progression](../CLASS_PROGRESSION.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Combat](../COMBAT.md). Earlier admonitions not to design Tier III date from when World 2 was not yet being planned. The user's explicit **2026-11-22 request to design Psion's two evolutions supersedes that *timing restriction for these two classes only***; it is not general approval to design all Tier III or implement either class.

## Lineage and advancement constraints

```text
Magically Touched Novice (Tier I)
    └── Psion (Tier II, level 60 advancement threshold)
         ├── Psionic Knight (Tier III) — basic-attack specialist
         └── Mirage Mage    (Tier III) — spellcasting specialist
```

Per existing class progression, advancing a level-60-or-higher Psion creates a **new level-1 Tier III class manifestation**; Psion's separate saved manifestation is preserved. Reaching level 60 unlocks eligibility, but exact new-class quest/mentor/unlock paths are not yet designed. Tier III advances at level 120. Do not treat these as stances that mutate the original saved Psion or silently carry over its purchased Psion rank points.

## Round 1 — USER-CONFIRMED (2026-11-22)

| ID | Topic | Approved decision | Explicitly not decided |
| --- | --- | --- | --- |
| PSION3-001 | Tier III class names | **Psionic Knight** = Psion's **BASIC/AUTOATTACK-focused** evolution; **Mirage Mage** = Psion's **SPELLCASTING-focused** evolution. Both are normal Tier III branches FROM Psion. | Model, appearance, gear restrictions, advancement mentor and quest. |
| PSION3-002 | Psionic Knight basic attack range | Knight's basic/autoattack range becomes **SHORTER than current Psion's 5 meters**, but **LONGER than ordinary melee range**: a clear midpoint/intermediate distance, **not automatically full melee**. The user did not select a fighting-style option in the first question set; the freeform intermediate-range instruction is authoritative. | Exact meter value; whether the attacks are projected swords, telekinetic weapons, physical strikes, floating blades, or another presentation; movement and attack cadence. |
| PSION3-003 | Mirage Mage basic attack range and Tier IV direction | Mage's basic/autoattack range becomes **LONGER than Psion's 5 meters**, but **SHORTER than the game's normal spellcaster range**. Its eventual **Tier IV descendant** is intended to acquire **full normal spellcaster range**. | Exact ranges; actual Tier IV class name, kit and evolution rules; do not grant full range at Tier III. |
| PSION3-004 | Mirage Mage combat identity | **High-damage spells, barriers, illusions, mirrors and psychic doubles**. It should substantially emphasize magical damage and illusion interactions rather than basic-attack scaling. | Which elements, whether barriers support self/allies, specific spells, coefficients, mana costs and CC mechanics. |
| PSION3-005 | Different retained parent skills | **Each evolution retains DIFFERENT selected Psion abilities appropriate to its specialization**, rather than automatically carrying over all five unchanged. Both evolutions retain an **illusion/double** mechanic. | Which other Psion abilities are retained, replaced or evolved; whether parent-skill rank investments have any impact (normal separate-manifestation rules apply unless later explicitly approved). |
| PSION3-006 | Psionic Knight illusion | Knight's descendant of **Mental Image becomes MOBILE**, able to **move toward enemies to reach them and execute BASIC/AUTOATTACKS**. The Knight's illusion emphasizes repeated attacking and functioning as a combat partner rather than simply exploding. | AI targeting, follow/return/leash, speed, range, damage multiplier, number of copies, lifetime, cooldown, whether it still explodes and how much. Do not assume direct RTS control. |
| PSION3-007 | Mirage Mage illusion | Mage also retains an illusion/double, but its design emphasizes **the EXPLOSION and its spell damage** over the illusion's basic attacks. Contrasts deliberately with Knight's mobile autoattacking double. | Whether Mage illusion is stationary/mobile, actual explosion triggers, radius, damage, damage type/element, lifetime and attack function. |
| PSION3-008 | Resources | **MANA ONLY for BOTH Tier III classes.** No new Focus, Clarity, Illusion Charges, sword gauge, or secondary spendable meter as part of their baseline identity. | Exact maximum Mana, regen, ability costs, possible non-resource temporary buffs. |

### Range direction (RELATIVE ONLY; no fabricated meter values)

```text
ordinary melee  <  Psionic Knight  <  Psion basic attack (5m)
Psion (5m)      <  Mirage Mage     <  normal spellcaster
Mirage Mage     <  Tier IV caster line intended full spellcaster range
```

The project's ordinary melee range and normal spellcaster basic-attack range must be checked against active class definitions before exact middle values are proposed. A range statement concerns **basic attacks**, NOT automatically every targeted spell's cast range or every illusion's own attack range. Range progression in Tier IV refers to the mage **descendant path**, not an extra Tier III upgrade.

### Design comparisons

- **Psionic Knight:** closer to the fight; basic-attacks/critical/physical identity; **mobile ally illusion that attacks enemies**. The actual weapon model and exact Psion skills retained are still unapproved.
- **Mirage Mage:** steps farther back; **high-damage magical spells, barriers, mirrors and illusions**; illusion **explosion** is its distinguishing clone function. Do not impose a card-magic system just because an earlier *Card Magic Girl* art reference was noted for the caster.
- Both continue the Psion's established psychic/illusion lineage; both use Mana, and both have a form of illusion. Do not accidentally copy *all* five Tier II Psion skills to both evolution kits.

### Safety and implementation boundary

This documentation is a Codex backlog/design reference, **NOT authorization to add Unity scripts, classes, models, scenes, drops or progression branches**. The current branch `feat/verdant-wilds` has unrelated local work in progress; preserve it untouched. Specific tier-III combat formulas, equipment, unlocks, ranks, armor, and advancement quests must be approved in future rounds. No approval of extra elements beyond the existing game's elemental registry.

## Round 2 — proposed five questions (OPEN; do not treat options as approved)

1. What is the **Psionic Knight's basic attack visual/weapon behavior** at its intermediate range: projected mindblade, short hovering blades, telekinetic sword slashes, or another idea?
2. How does **Psionic Knight's mobile illusion** acquire targets and position: independently seek enemies in range, follow Knight and attack current target, or another system?
3. Should Psionic Knight's double **retain the parent explosion as a minor effect**, remove it, or convert death/expiry into a different effect?
4. What is Mirage Mage's **preferred damaging spell pattern**: focused single-target burst, large AoE, chained mirror spells, or a mix?
5. What is Mirage Mage's illusion **explosion trigger and role**: automatic on expiry/death, player detonation, damage after a timed setup, or another design?

## Verified current range baselines (2026-11-22; reference, NOT approved Tier III numbers)

Read directly from the connected current Unity checkout:
- Psion normal basic attack: `Assets/_DiceFree/Settings/Psion/Psionic blade.asset`, `reach: 5` meters.
- Mage conventional caster basic attack: `Assets/_DiceFree/Settings/Skills/Mage/Mage wand basic.asset`, `reach: 12` meters.
- Novice unarmed melee example: `Assets/_DiceFree/Settings/Combat/Novice fists.asset`, `reach: 0.7` meters. This is **an example, not proof of a universal melee-reach constant**.

For upcoming review only: Psionic Knight around **3m** and Mirage Mage around **8–9m** would place each at an intermediate range as requested. **Neither range is user-approved**. The Mirage Mage's eventual Tier IV caster descendant is intended to attain standard caster reach; exact 12m assumption should be revisited against the target Tier IV attack definition when designed.

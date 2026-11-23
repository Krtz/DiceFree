# DiceFree — Psion Tier III evolutions — design interview

**Status:** Tier III **Rounds 1–2 USER-CONFIRMED**, latest **2026-11-23**. These classes are **DESIGN ONLY**, **not implemented in Unity**. Identity and which parent abilities are retained are now partly approved; precise ranges, spell upgrade coefficients, new skill mechanics, rank gates and advancement quests remain **OPEN**.

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


## Round 2 — 2026-11-23 (LATEST USER-CONFIRMED; supersedes earlier Round 2 option list)

The user's additional freeform notes expressly override/expand the third multiple-choice answer: **Mirage Mage retains PSYCHIC BOLT as well**, for FOUR parent abilities, while Psionic Knight retains TWO parent abilities. Interpret the freeform instructions as the **latest authoritative decision** rather than dropping the additional Bolt.

| ID | Topic | USER-APPROVED identity and behavior | Not yet approved |
| --- | --- | --- | --- |
| PSION3-009 | **Knight basic-attack presentation** | **Swing a mindblade that projects SHORT-RANGE PSYCHIC SLASHES**. These are the presentation of the normal basic attack, rather than a wholly separate spell or independent hovering-sword attack. Keep the Tier III Knight's reduced-but-greater-than-melee basic range from Round 1. | Exact meter reach (3m was only a suggestion), swing speed, blade travel/projectile versus arc-hit detection, hit timing, visuals, damage stats/scaling and equipment permission. Damage channel remains to be decided (baseline Psion auto attacks are Physical; no new element was authorized). |
| PSION3-010 | **Knight mobile illusion target/follow** | Knight's Mental Image evolution **FOLLOWS THE KNIGHT** and **ATTACKS THE KNIGHT'S CURRENT TARGET**. Unlike Tier II's stationary image, it can move to get into attack range. Its core purpose is **BASIC ATTACKS**, not a primary explosion. | Following distance, navigation, leash, targeting when the Knight has no current target or target dies, owner death, summon location, duration, clone attacks, explosion retained or not, speed, threat, snapshot rules. The player has not requested manual RTS control. |
| PSION3-011 | **Knight inherited abilities** | Retains/evolves **Psionic Blades Mastery** and **Mental Image** from Tier II; the Knight does **not** automatically retain Psychic Bolt, Lightning Storm, or the group Metal Shield. | Tier III scaling, rank investments, mastery features, skill slots/unlock levels, exact passive structure and other new abilities. |
| PSION3-012 | **Knight personal shield** | Gains a **SELF-ONLY/PERSONAL SHIELD** as a Tier III ability. This is NOT inherited group Metal Shield, and should not grant the same party protection by default. | Shield element if any, resource cost, cooldown, absorption amount, duration, timing, synergies and how it works with all generic shield rules. |
| PSION3-013 | **Knight empowered-basic-attack buff** | Gains a **SELF BUFF that makes the next basic/autoattacks DEAL EXTRA DAMAGE and COST MANA**. It is specifically the subsequent **attacks** that carry a Mana price while empowered; do not automatically add a permanent Mana tax to all Knight basic attacks. | Whether next N attacks or attacks within X seconds, exact charge count/duration, Mana per empowered strike, what happens at insufficient Mana, damage multiplier/flat bonus, cooldown, buff activation cost, and whether illusion attacks benefit. |
| PSION3-014 | **Mage retained abilities (FOUR)** | Retains/evolves **Mental Image, Metal Shield, Lightning Storm, and Psychic Bolt** from Psion. **Does NOT retain Psionic Blades Mastery**. All four are strengthened in specified ways below. This explicitly supersedes the earlier multiple-choice selection that listed only three. | How many additional abilities or mirror-specific spell interactions Tier III adds. |
| PSION3-015 | **Mage Mental Image** | Behaves **THE SAME as current Tier II Psion Mental Image**, including **stationary/autonomous basic attacks** and automatic explosion on normal **expiration OR destruction**, but **explosion does MORE DAMAGE**. User explicitly chose automatic trigger, NOT manual detonation. | Exact increased damage, added multiplier/scaling, skill rank caps. Preserve existing parent core mechanics unless later revised: ground placement, clone attack at 10% parent basic value, caster stat snapshot, 250% more incoming damage, enemy-only 4m Magical Void explosion. Other upgrades were not approved. |
| PSION3-016 | **Mage Metal Shield** | **Same group/shared-pool Metal Shield** mechanics as Tier II, but with **BIGGER initial CAST AoE radius** and **BIGGER shield absorption values**. Preserve the parent's collective pool rules, protection after leaving the cast radius, recast behavior and normal elemental shield processing unless revised. | New radius (old 8m), new pool formula (old 100 + 30×rank + 0.75×INT), cooldown (old 30s), and whether duration changes (old 8s; not requested). |
| PSION3-017 | **Mage Lightning Storm** | Retains the parent **Lightning Storm** but with **MORE DAMAGE**. Preserve the existing base three-wave targeted 4m circular AoE and channel rules unless the user changes them. | New damage formula, area/range, cooldown and added mirror interaction; no new AoE shape automatically approved. |
| PSION3-018 | **Mage Psychic Bolt** | Retains **Psychic Bolt** but with a **LOWER COOLDOWN** and **HIGHER SCALING** than Tier II. Current parent CD is **8s**; parent raw Magical Metal damage `30 + 8×rank + 1.2×INT`. Parent stun and Tenacity handling remain the starting behavior. | Exact new CD, whether scaling improvement affects INT coefficient, rank term or both, any stun upgrade, range and cast-time changes; no unsupported new elemental affinity. |
| PSION3-019 | **Mage broader damage plan** | New/evolved spell design should combine **SINGLE-TARGET BURST, AoE and MIRROR-BASED SPELL INTERACTIONS**. Emphasis on high damage, barriers and illusions remains. | The exact distinct mirror-interaction mechanic(s), whether any new spell is required, additional casting animations, rank distribution. |

### Side-by-side inheritance ledger (latest authority)

| Tier II Psion ability | Tier III Psionic Knight | Tier III Mirage Mage |
| --- | --- | --- |
| Psychic Bolt | **Not retained** as an inherited ability | **Retained:** lower CD, higher scaling |
| Psionic Blades Mastery | **Retained:** specialized for basic attacks | **Not retained** |
| Mental Image | **Retained:** mobile, follows Knight, attacks Knight's current target | **Retained:** same stationary double, stronger expiry/death explosion |
| Lightning Storm | **Not retained** as inherited ability | **Retained:** higher damage |
| Metal Shield | **Not retained** as group skill; instead gains a **NEW personal shield** | **Retained:** wider party AoE, bigger shared absorption pool |
| New Tier III tool | **Self-buff:** upcoming basic attacks become stronger and each consumes Mana | Mirror interactions/additional mage abilities **not yet detailed** |

**Important mechanical distinction:** Both are still **Mana-only** classes. Empowered Knight attacks spend Mana; this is NOT an additional separate resource. Each class creates a new Tier III manifestation as established in `CLASS_PROGRESSION.md`. The Tier II Psion ability caps/rank gates are not automatically reused unchanged for Tier III. The precise number of future Tier III skill slots is unselected.

### Round 3: five questions to resolve next (OPEN)

1. **Exact basic-attack reaches:** accept suggested Knight **3m** / Mirage Mage **8–9m**, or specify other numbers?
2. **Knight Mental Image:** if the Knight has no current target (or target dies), should the moving clone stay beside the Knight and wait, acquire nearest hostile itself, or attack the Knight's target only after explicit acquisition?
3. **Knight personal shield:** should it be a Metal elemental shield, a neutral/generic shield, or another existing element? Any special interaction with empowered attacks?
4. **Knight empowered strike buff:** a fixed number of empowered attacks, a short-duration window, or a toggle that spends Mana on each attack?
5. **Mirage Mage mirror mechanics:** should new spells copy existing casts, bounce between illusions/enemies, or trigger explosive interactions? Clarify how this differs from automatic Mental Image expiry/death.

Do NOT treat these Round 3 options as user approvals. **Documentation/design only**; this round does not authorize Unity implementation, asset changes or local WIP cleanup.

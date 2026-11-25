# DiceFree — Psion Tier III evolutions — design interview

**2026-11-25 NEWER GLOBAL MANA RULE (CROSS-CLASS DESIGN OVERRIDE; USER APPROVED, NOT YET RUNTIME IMPLEMENTED):** During Marksman Tier III Round7, user expressly established that **ALL Mana-using classes must follow ONE GLOBAL max-Mana and regen coefficient formula**, rather than class-specific INT/SPR conversion/regen multipliers. Classes should be balanced via **starting/base Mana, starting INT, INT per-level growth, and item stats**. This policy SUPERSEDES the class-specific Mana-per-INT and regeneration formulas approved earlier here in Psionic Knight/Mirage Mage Round22 as final standing values; those previously approved HP/stat/skill effects and costs remain valid unless affected by future testing. The actual shared `ClassResourceProfile.cs` algorithm presently reads numeric coefficients from PER-CLASS assets (e.g. Ranger and Psion differ), so the new universal numeric policy needs a coordinated Codex design/code migration with existing playable-class regression testing. **Do not silently change Psion runtime Mana assets during a Marksman documentation interview**, nor claim the global coefficients are already uniform. Canonical explanation and current source audit: [Combat](../COMBAT.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Marksman Round7](MARKSMAN_TIER3_DESIGN.md). This note records LATEST cross-class instruction; historical sections remain intact for audit. No Unity assets or builds changed.

**Status:** Tier III **Rounds 1–22 APPROVED for initial playtest (2026-11-24)**. User explicitly approved ALL Round 21 proposed level-1 attributes, revised per-level growth, HP/Mana resource profiles, upgraded rank-dependent Knight Magical Void formula and concrete combat/mirror edge-case rules, in addition to previously approved exact 150-rank gates and advancement quests. These are **approved first-playtest/implementation design baselines**, not evidence of a live Unity test or final tuning. Older Round 20–21 'pending approval' phrases are historical and superseded by Round 22. Both classes' 150-rank progressions are USER APPROVED. Round 19 confirms flexible World 1/2 hidden teacher placement, level-60-only quest acceptance with class/level-specific dialogue, Codex-only teacher tracking, return-to-teacher advancement, and BOTH specialization quests active concurrently. Round 18's fresh solo dungeon clear requirements remain unchanged. **Fourfold Hollow (`Assets/_DiceFree/Scenes/FourfoldHollow.unity`) is present in the current feat/verdant-wilds source and cited in QA handover, superseding historical 'future Shaman dungeon' remarks.** This supersedes Round 17's ambiguous Wolf Cave and Shaman Cave names. Prior Round 16 balance formulas remain DESIGNER-PROVISIONAL. All costs and numeric balancing remain playtest values, not locked. These override historical Round 11/12 drafts. Both classes have 150 earnable skill points, one per class level through 150; Tier IV advancement eligibility remains level 120. Gameplay mechanics still open include spell cooldowns, Mana costs, certain attack channels and balance values. **DESIGN ONLY, not implemented in Unity.**

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


## Round 3 — 2026-11-23 (LATEST USER-CONFIRMED)

These five answers supersede the prior Round 3 option prompts. **Question 5 was intentionally left unanswered**; the user's freeform idea is a promising **proposal, not yet locked spell-copy rules**.

| ID | Topic | Approved choice / scope | Open implementation details |
| --- | --- | --- | --- |
| PSION3-020 | **Tier III basic-attack ranges** | **Psionic Knight 3 meters**, **Mirage Mage 9 meters**. These are the actual approved **normal basic/autoattack ranges**, not the parent Psion's 5m range. The Mirage Mage's future Tier IV caster evolution should achieve full normal caster basic range, pending Tier IV design. | Skill ranges, illusion attack ranges, engagement distances and range modifiers from equipment remain independently designed. |
| PSION3-021 | **Knight illusion idle targeting** | The Knight's **mobile Mental Image** follows the Knight, prioritizes **the Knight's current target**, and **AUTOMATICALLY ATTACKS nearby valid hostile enemies when the Knight has no current target**. It does not remain idle merely because no explicit target is selected. | Search radius, follow/leash range, if it may chase targets outside that range, behavior when an explicit current target becomes unreachable, attack-completion retargeting and persistence after caster death. Preserve no manual RTS control unless later expressly changed. |
| PSION3-022 | **Knight personal Void shield** | Knight gains a **SELF-ONLY Void-element shield**. It uses the project's **ordinary elemental shield rules**, not neutral 1:1 shield handling, and it is *not* a copy of Mirage Mage's ally Metal Shield. | Shield capacity, scaling, Mana cost, cooldown, duration, activation cast, whether it can cover Knight's illusion, source-recast handling. Under current global elemental-shield policy, ordinary eligible damage costs **0.8 shield HP per damage prevented when element matches Void**, otherwise **1.2 shield HP**; if global policy changes, follow global rules. |
| PSION3-023 | **Knight empowered basic-attacks as toggle** | Knight's empowered attack ability is a **PLAYER-CONTROLLED ON/OFF TOGGLE**, not a limited number of charges nor a fixed duration. **Every normal basic attack performed while toggled ON costs Mana and deals extra damage**. With toggle OFF, Knight uses normal unempowered basic attacks and does not pay this empowerment tax. **Mana-only** class still; no new class-specific resource. | Mana spent per attempted/landed attack, handling insufficient Mana, exact extra damage, whether each slash is its own basic attack event, toggle activation cost/cooldown, whether cloning gains or pays for empowered hits, auto-off on death/class swap. No free empowered attacks assumed when Mana is zero. |

### Mirage Mage mirror-spell proposal from extra thoughts (NOT YET USER-CONFIRMED AS MECHANICS)

The user suggested: **"what if mirage mage mental image casts a weaker mirror version of the spells the main casts?"** This is the proposed direction for answering unanswered Round 3 question 5. The idea is **genuinely new** beyond the already confirmed stationary image with basic attacks and enhanced automatic expiry/death explosion. **Do not mark the mirror casts as approved implementation specifications until follow-up choices establish their scope.**

Conceptual draft to review:
- Whenever Mirage Mage casts a qualifying offensive spell while its Mental Image is alive, the image **mirrors that cast at reduced effectiveness**. The caster's real spell is never replaced by the reflected copy.
- Keep **the image itself stationary** and **retain the stronger Magical Void explosion on expiry or destruction**, as already approved.
- The image's spell mirrors should be secondary damage, not a way to reproduce an entire additional full-strength Mage, shield the whole party twice without approval, generate recursive clones, or turn on a second casting/resource system.
- **Unanswered critical questions**: which spells mirror (Bolt and Storm? Metal Shield? future new spells?), mirror damage coefficient and rank scaling, whether status effects or stun are replicated, cast destination/target/line of sight, copy timing, Mana costs (likely owner only, but undecided), limits when multiple clones overlap, whether mirrored casts themselves can trigger further copies, and whether image still does its inherited 10% basic attacks.
- Preserve the approved parent image's **automatic enhanced explosion** on death/expiry; never silently replace it with a spell-copy trigger.

### Current side-by-side identity snapshot

| Property | Psionic Knight | Mirage Mage |
| --- | --- | --- |
| Tier | III, new preserved Echo manifestation | III, new preserved Echo manifestation |
| Normal basic-attack range | **3m** | **9m** |
| Primary damage | Short-range Physical-style mindblade psychic slashes; empowered attacks spend Mana | High-damage magical single target / AoE spells |
| Mental Image | **Mobile; follows Knight, attacks current target or auto-acquires nearby hostiles** | **Stationary parent-style image** with stronger automatic enemy-only Void explosion; weaker mirrored spellcasts **proposed, not finalized** |
| Retained Psion skills | Psionic Blades Mastery; Mental Image | Psychic Bolt; Mental Image; Lightning Storm; Metal Shield |
| New/changed shield | New **self-only Void elemental shield** | Enhanced **group Metal Shield** with bigger initial cast area and absorption |
| Additional class feature | On/off empowerment; every basic attack while on costs Mana and does extra damage | Mirror-based spell interaction **to design** |
| Resource | **Mana only** | **Mana only** |

### Round 4: five focused questions (OPEN, do not infer answers)

1. **Mirror copy scope:** should Mirage Mage's image copy only offensive spells (e.g., Psychic Bolt and Lightning Storm), including shield casting, or every magic spell? Should it copy future Tier III spells too?
2. **Mirror output:** what percentage of Mage's real spell damage should each image reproduce (for example 20%, 30% or 40%)? Should CC/debuff/stun be mirrored, reduced or omitted?
3. **Mirror targeting/timing:** should the image cast at the Mage's chosen target/ground point, at its own target, or use the same direction relative to the image? Instant echo versus a delay?
4. **Knight empowered-buff Mana/cadence:** fixed Mana per attack versus scaling Mana with tier/skill rank; how should it behave on low Mana and when the Knight's illusion attacks?
5. **Knight image on death:** does the mobile image retain any Void explosion on expiry/death, lose explosion entirely, or get a modest weaker burst? Exact combat tuning stays later.

**Continuity safeguard:** The Knight autoattacks' *presentation* is psychic slashes, but that alone does not authorize changing the standard Physical basic-attack damage channel. Neither class gains a secondary resource. This round is **documentation-only**. Do not modify live Unity code, uncommitted local World 2 work, gameplay assets or quests.


## Round 4 — 2026-11-23 (LATEST USER-CONFIRMED; supersedes Knight toggle)

The user's freeform response to question 4 **explicitly rescinds** Round 3's toggle. Its freeform answer is the latest authority. The user left question 4's multiple-choice answer blank but supplied a clear replacement in the extra notes. Also, the earlier Mirage Mage mirroring idea is no longer only a proposal: spell copying is now approved, but crucial implementation edge cases still await decisions.

| ID | Subject | **Approved design decision** | Implementation-relevant open points |
| --- | --- | --- | --- |
| PSION3-024 | **Mirage Mage image copies every spell** | While alive, the Mirage Mage's **Mental Image** echoes **ALL of the Mage's spells**, **including Metal Shield**, not offensive spells only. This transforms spell mirroring from a Round 3 *proposal* into a confirmed class mechanic. The original stationary image's enhanced **automatic explosion on expiry/death** remains intact. | How an image mirrors **Mental Image itself** (recursive clone issue), shield strength/targets and other non-damaging skills, which future spells are eligible, simultaneous copies and whether mirrored casts cost additional Mana. Do NOT silently exclude Metal Shield or declare the copied shield identical in potency without approval. |
| PSION3-025 | **Mirror damage scales up to 25%** | Damage from a copied spell starts **weak at lower Mental Image ranks** and **increases with Mental Image skill rank to 25% of the original spell's damage at maximum rank**. This is a percentage of the corresponding caster spell's damage, NOT an added 25% per rank. | Initial percentage, progression/rounding, whether copies use raw or final damage, shield effectiveness (damage % does NOT automatically define copied shield size/healing), copied utility durations and non-damaging spell strength. The Tier III Mental Image rank cap is not yet chosen. |
| PSION3-026 | **No mirrored control effects** | **Copied spells must not reproduce stuns or other control effects**, even if the original Psychic Bolt stuns. The Mage's original spell retains its normal effects; its mirror version deals its eligible reduced damage only, with no replicated CC. | Treatment of other non-control debuffs/status effects, any non-damage utility exceptions and presentation of control-free mirrored spells. |
| PSION3-027 | **Targeted delayed echo** | Each copy is cast **at the SAME target or targeted ground location** as the original, after a **small delay** that clearly looks like a **visible echo**. | Exact delay, line-of-sight/range validation, handling disappearing targets, and what "same target" means for untargeted/self-centered skills such as Metal Shield and Mental Image. Avoid automatically redirecting copies to the illusion's separately acquired basic-attack target. |
| PSION3-028 | **Knight empowerment: NOT a toggle** | **SUPERSEDES PSION3-023**. The Psionic Knight's empowering ability is an **active finite NEXT-ATTACKS BUFF**: the next approximately **(purchased skill rank ÷ 2)** normal basic attacks **deal additional damage and each COSTS Mana**. **Mana cost scales with the ability's purchased skill rank**. The user specifically **RESERVES the previous on/off toggle mechanic for a possible TIER IV evolution**, not Tier III Knight. | Integer rounding for fractional rank÷2, particularly rank 1 (ceil(rank/2) would ensure one empowered attack at rank 1 and is a DESIGNER PROPOSAL ONLY); skill rank cap, per-hit Mana formula, bonus damage formula, duration/expiration, on-cast Mana/CD, miss handling, premature recast and insufficient Mana behavior, and whether clone attacks consume/benefit from Knight charges. |
| PSION3-029 | **Knight image end effect replaces explosion** | The Knight's **mobile attacking Mental Image NO LONGER EXPLODES** on ending. Instead, **when the illusion ends, the KNIGHT receives a TEMPORARY BUFF**. This is a personal benefit, not an enemy-centered death burst. | Which buff (attack speed, damage, crit or another effect), strength and duration, whether it triggers both on natural expiration AND enemy destruction (both are normal parent end causes, but exact Tier III trigger eligibility should be confirmed), whether it works if Knight is dead, and multiple-clone stacking behavior. |

### Critical supersession and continuity notes

1. **Do not implement the Round 3 Knight toggle for Tier III.** Earlier choice PSION3-023 and the Round 3 comparison table are historical. Replace with a **finite rank-based buff granting approximately rank/2 stronger Mana-spending basic attacks**. A toggle is only a *future Tier IV concept*, not a Tier III feature and not an already fully specified Tier IV kit.
2. The Knight's mobile double continues **following the Knight**, attacking the Knight's current target and automatically finding nearby hostile enemies when no target is selected, **at Knight's confirmed 3m basic reach unless its own reach is later designed differently**. Its finish is a **temporary self-buff to the Knight instead of an AoE explosion**.
3. **Mirage Mage** retains **Psychic Bolt, Lightning Storm, Metal Shield, Mental Image**; its image stays **stationary**, normally attacks as a minor unit, now **casts weaker delayed copies of all spells** and **still explodes for higher-than-parent Void damage** on termination. Its base attack reach is **9m**. Existing parent-element/shield rules continue unless explicitly redesigned.
4. **Avoid assuming spell-mirror recursion is safe:** if the Mage casts Mental Image, blindly copying that summoning spell could produce further mirrors that recursively summon yet more illusions, potentially unbounded. **The user said "all spells"; this exception/handling must be decided, not invented.** Also decide how ally-targeted and self-centered spells echo.
5. Do not silently interpret "25% max" damage as shield capacity at 25% or as an extra 25% of the original stun. No CC from image copies.
6. Everything remains **design only** on branch `feat/verdant-wilds`. No Unity scripts, uncommitted World 2 changes or assets altered.

### Round 5 — next five design questions (ALL OPEN)

1. **Knight empowered-attack count:** how should rank÷2 round to whole attacks, especially at rank 1? Example **ceil(rank/2)** gives one empowered hit at ranks 1–2, two at ranks 3–4; **floor(rank/2)** gives zero at rank 1. An alternative is `1 + floor(rank/2)`. Do charges expire, or persist until spent?
2. **Knight end-of-illusion buff:** choose attack speed, damage, crit chance, defensive effect or mix; decide whether an illusion killed early still triggers it.
3. **Mirage Mage copied Metal Shield:** should image produce its own weaker separate group-shield pool, add capacity to the Mage's shared pool, shield itself, or use another design? Pick scaling separately from 25% damage mirror.
4. **Mirror's Mental Image recursion:** when Mage casts Mental Image, should the existing illusion copy it, create a harmless visual echo, add a short non-summoning effect, or exempt clone-summoning from actual repeat casts? Preserve "all spells" in spirit without infinite reproduction.
5. **Mirror echo timing and target invalidation:** choose ~0.2s/~0.4s delay or another interval, and target behavior when an enemy moves/dies before its echo; decide whether mirrors copy casts only while present at the original cast start.

Keep the next interview focused on these choices. Never mark proposed answers as approved.


## Round 5 — 2026-11-23 (LATEST USER-CONFIRMED)

This round finalizes the Knight empowered-basic attack **charge formula**, selects the Knight illusion's end buff, and defines the Mirage Mage's **shield mirroring**, **bounded secondary images**, and **echo delay**. The user's extra thoughts are authoritative even when they differ from earlier proposed examples.

| ID | Topic | USER-CONFIRMED rule | OPEN / do not assume |
| --- | --- | --- | --- |
| PSION3-030 | **Knight empowered charges: exact rank formula** | Each activation empowers the next **`1 + floor(purchasedSkillRank / 2)`** normal basic attacks, using whole-number floor rounding. Examples: rank1 → **1** attack; rank2 → **2**; rank3 → **2**; rank4 → **3**; rank5 → **3**. Each empowered attack **costs Mana, with Mana cost scaling by this skill's rank**, and adds a separate **VOID-element damage component**. The earlier toggle and earlier simple `floor(rank/2)` / `ceil(rank/2)` proposals are SUPERSEDED; toggle is reserved as an unimplemented Tier IV idea. | Exact bonus Void RAW damage/rank/scaling, Physical vs Magical **damage channel** for bonus Void component, Mana cost formula, cooldown, casting cost, charges' expiration, misses/shield consumption, insufficient Mana, stacking/recast and whether illusion attacks consume Knight charges. Preserve the ordinary non-empowered basic strike as distinct from its added Void packet, rather than assuming all normal auto attacks turn Void. |
| PSION3-031 | **Knight illusion grants attack speed on ending** | Psionic Knight's mobile, attack-focused Mental Image **does NOT explode**. Instead, **when it ends, the Knight gains a TEMPORARY ATTACK-SPEED BUFF**. | Attack-speed %/flat increase, duration, stacking/replacing, whether granted when illusion is killed early versus expires naturally, and whether Knight must be alive/nearby. No extra damage/death explosion automatically approved. |
| PSION3-032 | **Mirage Mage mirrored Metal Shield** | When Mage casts Metal Shield, eligible Mental Images produce a **SMALLER INDEPENDENT GROUP-SHIELD POOL**, **not** extra capacity added to the Mage's original shared pool, not a full-strength duplicate, and not an illusion-only shield. Preserve existing elemental Metal shield rules and group protection semantics unless further refined. | Independent pool size/formula, radius/position (image centered vs Mage centered), duration, affected targets, pool order/stacking relative to original and multiple mirrors, cast time/echo timing, resource accounting. **Do not apply the 25% damage-copy cap to shield capacity without separate user approval.** |
| PSION3-033 | **Mental Image copying Mental Image, bounded** | If a Mirage Mage image mirrors a Mage's **Mental Image** cast, it creates **ONE EXTRA WEAKER ILLUSION** that **CANNOT COPY SPELLS OR CREATE FURTHER ILLUSIONS**. The weaker extra image **EXPLODES FOR THE SAME DAMAGE as the original Mirage Mage Mental Image** when it ends, per user's explicit extra thoughts. The previously approved Mage image automatic explosion on expiry/destruction remains in force. | What makes secondary illusion 'weaker' (e.g. lower HP/basic damage/lifetime) and the secondary image's attack permissions/AI, plus how many base images may mirror one cast. **Its explosion must not be reduced to the 25%-at-max spell-copy damage amount**: user specifically approved equal explosion damage. Whether explosion radius, target limits and exact snapshot stats match the original needs tuning/confirmation; parent-class enemy-only explosion remains default reference. Bound mirror **generation depth to 1**: no self-replication chain. |
| PSION3-034 | **Echo timing** | Mirrored casts occur after a **fixed 0.4-second delay**, visibly following the Mage's original spell at the **same selected target / ground location**, where relevant. This applies as the current default to offensive casts and other mirrored spell types, subject to appropriate definition of target/context. | When to snapshot targets/stats, behavior if targets die, move or are out of range/LoS, copied self-centered spell center, whether echo resolves if originating image dies in the 0.4s wait, and whether the Mage pays extra Mana. Delay scaling with skill rank was NOT selected. |

### Current mechanics (use this instead of superseded Round 3/4 text when implementing)

**Psionic Knight (Tier III)**
- Normal basic-attack range **3m**; swings mindblade to project short psychic slashes.
- Basic-attack specialist retains **Psionic Blades Mastery + Mental Image**, with a separate **self-only Void elemental shield**.
- Empowered strike ability is a **finite buff**, NOT a toggle. Normal basic attack count empowered per activation = **`1 + floor(rank/2)`**. Each empowered basic attack consumes **rank-scaling Mana** and deals extra **Void-element** damage. Whether added Void damage is Physical or Magical is OPEN; normal basic attacks are Physical by default.
- Mobile image **follows Knight**, attacks the Knight's selected target or independently acquires nearby hostiles; when it ends it gives Knight a **temporary ATTACK SPEED buff**, with **no explosion**.

**Mirage Mage (Tier III)**
- Normal basic-attack range **9m**; Tier IV caster-descendant goal is full normal caster range.
- Retains **Psychic Bolt, Lightning Storm, Metal Shield and Mental Image**. Bolt lower CD/higher scaling, Storm higher damage, Shield larger original cast AoE/pool, Image higher explosion damage.
- Stationary image performs its existing minor basic attacks and mirrors **ALL Mage spells**, including shields and Mental Image, with **0.4s delayed same-target visible echoes**. Mirrored offensive spell damage scales from weak at lower Image rank to **25% at max Image rank**, with **NO copied stuns/control effects**.
- Mirrored Metal Shield creates a **smaller, independent shared group pool**.
- Mirrored Mental Image spawns **one weaker secondary illusion**, which **cannot echo magic or summon descendants**. Despite its general weakness, its **expiry/destruction explosion deals the SAME DAMAGE as the main Mirage Mage Mental Image**, not a scaled-down 25% mirror hit.
- Original and secondary images retain ordinary automatic end/death explosion concept, with exact radius/stat snapshot for secondary to be settled.

### Round 6 interview topics (ALL OPEN)

1. **Knight empowerment's Void damage channel and formula:** Physical vs Magical Void packet; additional damage scaling (AGI, INT, flat per rank) and Mana per empowered attack.
2. **Knight illusion's end buff:** attack-speed bonus and duration; should it trigger on both natural expiry and enemy kill, and what happens on overlapping clones?
3. **Mirage Mage image's extra illusion:** which stats and attacks are weakened while keeping full explosion damage (HP, autoattack damage, lifetime, attack speed)?
4. **Mirage Mage shield copy:** independent pool percentage or formula, AoE center (image or Mage), party membership and pool stacking/consumption order.
5. **Mirrored casts and 0.4s delay:** target dies/vanishes or Image dies before echo; Mana cost and ability to mirror the mage's newly-designed future spells.

**Scope:** This is a documentation/design round only, not Unity implementation, numeric balancing approval beyond the explicitly selected formula/0.4s, or authorization to touch the developer's uncommitted local work. Maintain compatible class-lineage progression and spell/elemental rules.


## Round 6 — 2026-11-23 (LATEST USER RESPONSE; APPROVALS AND OPEN QUESTIONS)

**Interpretation rule:** User chose to DEFER the empowered Void damage **channel** until after choosing its damage formula. Do not choose Physical or Magical yet. The answer to question 3 is **not answered**: the freeform note describes how PRIMARY Mirage Mage and Knight images compare, but does not answer what makes the Mage's **SECONDARY mirrored image** weaker. The copied Metal Shield question caused confusion; preserve both the checked rank-scaling answer and the user's expressed preference for aligning it with the spell mirror, without falsely claiming a complete shield design.

| ID | Topic | Latest approved direction | Explicit open items |
| --- | --- | --- | --- |
| PSION3-035 | **Knight Void extra damage channel deferred** | The Knight's empowered basic attacks still add **VOID-element** damage and consume Mana, but **the user wants the damage formula established BEFORE deciding whether the added Void packet is Physical or Magical**. Do not silently classify it while creating a draft formula. Base non-empowered autoattack remains normally Physical. | Void bonus damage/rank/stat formula, Mana-per-hit/rank formula, and only afterward damage channel. |
| PSION3-036 | **Knight image ending buff rank progression** | The Knight's no-explosion mobile image grants a **temporary ATTACK SPEED buff**, and now **BOTH its STRENGTH and its DURATION SCALE with the Knight's Mental Image skill rank**. | Rank 1 and cap values, exact growth curve, normal expiry vs killed trigger, stacking and owner-death behavior. |
| PSION3-037 | **Primary Mage versus Knight image basic attacks** | **Mirage Mage's ORIGINAL/PRIMARY Mental Image retains the exact Tier II Psion AUTOATTACK values/behavior**, while **Psionic Knight's mobile Mental Image has INCREASED AUTOATTACK values** compared to Tier II. As already agreed, Mage's primary image gains a more powerful expiration/death explosion; Knight's swaps explosion for attack-speed buff. | What multiplier / rank scaling gives Knight's increased clone basic damage, how Knight clone attack values combine with owner's empowered-attack charges, and whether its attack range differs from the Knight's 3m. The Mage secondary copy's reduced stats are UNANSWERED, not fixed by this choice. |
| PSION3-038 | **Copied Metal Shield strength scaling clarification** | User explicitly chose **capacity scaling with Mental Image rank**, then asked whether it should scale **the same as the illusion's mirrored damage** and said the earlier shield-specific question was unclear. Interpret this as a **preference to simplify copied Metal Shield** by matching its **rank-dependent percentage of the original shield capacity to the mirror spell damage percentage**, which starts weak and reaches **25% at max Mental Image rank**. This is a **proposed matching formula awaiting precise confirmation**, not an approved separate numeric table. Maintain its existing confirmed **smaller INDEPENDENT GROUP SHIELD pool** and the actual Mage Metal Shield's original values. | Confirmation of using the identical percentage curve (versus separate rank curve), starting %, exact progression, center/radius, target membership, shield priority and copied source identity. Explain in ordinary words that **independent pool** means each original/mirrored cast has its own total capacity and that the mirror would protect its own group without increasing the original pool. |
| PSION3-039 | **Interrupted echo canceled** | If the originating Mirage Mage illusion **dies during the 0.4-second echo delay**, its **PENDING copied spells are CANCELED immediately**. Spells whose mirror effect has already completed are not retroactively reversed; spell effects already launched/resolved should follow normal game effect rules unless later overridden. | Whether scheduled mirrors cancel on natural image expiration (user answered **dies**, not all expiration), target becoming invalid, casting outside line-of-sight/range, and Mana charged on a canceled echo. |

### Clarifying the copied Metal Shield question, without changing the established split

The Mage casts its original enlarged Metal Shield: that is one **shared pool** protecting the selected group. The illusion mirrors Metal Shield **0.4s later** and creates **another shared pool** for the affected members, **separate from the Mage's pool**, not an extra refill to the Mage's pool. The unanswered question was just how much smaller that second pool should be. The user's proposed *same-as-mirrored-damage* simplification gives the copied pool a **percentage of the original pool** that rises with Mental Image rank, **up to 25% at max rank**, matching the copy's damage percentage. **Do not apply the 25% ceiling as a blanket rule to other shields or the original.** This alternative is framed as the user's suggested alignment, not a forced independently-selected curve, until confirmed.

### Latest identity summary (authoritative over superseded prior rounds)

- **Psionic Knight**: normal basic reach 3m. Mindblade psychic slashes. Mobile image follows, attacks the Knight's target or auto-acquires nearby hostiles, has **stronger basic attacks than Tier II Psion Mental Image**; on ending, replaces explosion with **attack-speed buff of rank-scaling strength and duration**. Personal Void-element shield. Empowered attack consumes rank-scaling Mana for **`1 + floor(rank/2)`** empowered basic strikes, adding separate Void damage; the bonus **damage channel is deferred** until the damage formula is selected. No Tier III toggle.
- **Mirage Mage**: normal basic reach 9m. Original primary image keeps **Tier II image's 10%-of-caster basic attack value and attack behavior** while exploding harder than Tier II; copies *all* Mage spells after 0.4 seconds, without duplicating CC. Copy damage percentage grows to 25% by max image rank. Copied Mental Image generates **one weaker non-replicating secondary** with the **full primary Mirage Mage image explosion damage** on ending, but its precise non-explosion weakness remains open. Copied Metal Shield produces a weaker **independent group pool**; the user leans toward matching the damage mirror's rank-dependent percent, with detailed confirmation pending. Echoes pending when originating image is killed are canceled.
- Both Tier III evolutions **use Mana only**, with separate level-1 manifestations produced at the Tier II level-60 evolution threshold. Do not copy Tier II skill rank-gate tables unmodified or implement Unity content from this interview.

### Round 7 — recommended five focused questions (ALL OPEN)

1. **Knight image attacks:** how much more damage than Tier II image's 10% of owner basic damage? Options: ~20%, ~30%, ~50%, or a rank-scaling progression; confirm autoattack speed inherits ordinary owner speed or not.
2. **Knight end buff:** choose rank-1 and max-rank **attack-speed percentage and duration**; determine if both natural expiration and being killed early grant it.
3. **Mirage Mage secondary image:** define weakness relative to primary (HP, autoattack damage, duration, speed, etc.) without weakening its fully retained explosion.
4. **Knight empowered Void raw damage:** choose emphasis on **AGI+INT, STR, pure flat-per-rank or mixed** scaling and Mana cost trend. The Physical/Magical channel choice is still DEFERRED until formula.
5. **Mirage Mage shield-copy clarity:** confirm whether copied shield capacity follows the **same low-to-25%-at-max percentage scaling as mirrored damage**, and whether the new independent group shield is centered at the illusion's position or the Mage's position.

**Scope:** Round 6 only updates this authoritative **GitHub design document**. No Unity code, local branch, assets, performance claims, or Codex work modified. All exact numeric balancing values remain TBD unless user selected them.


## Round 7 — 2026-11-23 (LATEST USER-CONFIRMED; freeform corrections supersede selected options)

**Interpretation:** The user's **extra thoughts explicitly override the selected multiple-choice starting values** for Knight image attacks and attack-speed buff. Do NOT retain the previously offered 20% start, +5% haste, or 2s initial haste duration as approved. The user's extra thoughts also fix the Knight's empowered-hit AGI/INT weighting. Question 3's chosen answer concerns the **secondary Mirage Mage image**, while the user separately reiterates that the **primary Mirage Mage image uses the parent's 10% basic damage**.

| ID | Topic | USER-APPROVED decision | OPEN / implementation cautions |
| --- | --- | --- | --- |
| PSION3-040 | **Psionic Knight primary image basic damage** | Knight's mobile Mental Image normal basic attack damage scales from **10% of the Knight's normal basic-attack damage at Mental Image rank 1 up to 50% at the maximum Mental Image rank**. This SUPERSEDES the initially selected 20%-to-50% option. Knight image still follows and attacks Knight's selected target and auto-acquires nearby enemies if no selected target. | Tier III image rank cap; linear vs stepped/intermediate rank curve; owner basic raw damage snapshot vs live value; whether damage uses owner's current equipped weapons and crits. Do not invent a fixed +50% basic hit at rank 1 or assign rank cap from Tier II automatically. |
| PSION3-041 | **Knight image inherits caster passives and buffs** | Knight illusion **also benefits from the Knight owner's applicable passives** and **from buffs the Knight uses**. The copied buff strength is **scaled using the same rank-dependent percentage as the illusion's basic-attack damage** (10% at rank 1 → 50% at maximum rank). Treat these as effects on the image, not additional independent Knight buffs or a copy of the Knight's own Mana reserve. | Confirm which passives are eligible and precisely how to apply the percentage to passive effects, additive/percentage bonuses, crit effects, durations, shield effects, and temporary empowered attacks. The user's explicit wording specifies buff-effect scaling; avoid erroneously multiplying the clone's entire already-reduced attack damage by the percentage again. No automatic secondary Mana-spending attacks, charge consumption, or buff recursion without future approval. |
| PSION3-042 | **Knight illusion ending buff exact endpoints** | When the Knight image ends by **NATURAL EXPIRATION OR ENEMY DESTRUCTION**, it grants the Knight **+1% attack speed for 1 second at Mental Image rank 1**, scaling to **+50% attack speed for 5 seconds at the maximum Mental Image rank**. **NO explosion**. This SUPERSEDES the initially selected +5% for 2s → +25% for 6s example. | Exact rank cap, interpolation (linear vs rank breakpoint), rounding/tick timing, stacking or replacing concurrent ending buffs, whether the buff applies while Knight dead, and owner distance/instance rules. |
| PSION3-043 | **Mirage Mage secondary image weaker basics and HP** | The extra non-recursive Mental Image summoned by an existing Mirage Mage image has **50% of its primary image's HP and 50% of its primary image's NORMAL BASIC-ATTACK DAMAGE**, but retains **FULL SAME-DAMAGE expiry/destruction explosion** as primary image, without copying any spells or summoning more images. The **original PRIMARY Mage image remains at the Tier II Psion base: 10% of caster normal basic-attack damage**. Thus, under the same snapshot and before other modifiers, secondary basic hits correspond to **5% of the Mage's normal basic hit value**. | Whether secondary image lifetime/attack speed/reach/crit/procs exactly match primary (not answered), how to snapshot/round HP and basic damage, and generation depth limits for multiple original images. Do NOT apply 50% modifier to secondary explosion. The mirror image is stationary by the currently inherited Mage image identity absent a separate approval. |
| PSION3-044 | **Knight empowered-hit Void bonus stat emphasis** | Extra empowered-strike **Void-element** damage scales as **a flat amount per purchased empowerment skill rank PLUS AGILITY and INTELLIGENCE contribution**, using **80% AGI / 20% INT relative weight** (an **4:1 AGI:INT emphasis**). This strengthens the Knight's AGI leaning while retaining INT relevance. The normal Physical basic hit remains distinct from its bonus Void packet. | Exact raw damage formula, flat base/rank increase, overall magnitude/coefficient of the 80/20 blend (weighting alone does not select absolute damage), Mana cost per empowered strike, and Physical vs Magical channel of the added Void hit (explicitly deferred until formula settled). Empowerment still grants `1 + floor(rank/2)` charged hits, not a toggle. |
| PSION3-045 | **Mirage Mage copied group Metal Shield origin** | A Mirage Mage illusion's mirrored Metal Shield creates its **own smaller independent shared group-shield pool CENTERED ON THE ILLUSION**, protecting **nearby eligible allies**. This is not centered on the caster or automatically added into the caster's existing pool. The copy occurs 0.4 seconds after the Mage casts the original, unless canceled by the originating image's death. | Copied shield radius, allies' inclusion, pool strength curve, absorption priority when multiple independent pools cover the same ally, duration, and whether a disappeared image cancels an already applied shield. **The proposed rank-scaling shield strength matching mirror spell damage (weak→25% at max) remains a suggested simplification, not an exact separately approved numeric formula** until user confirms. |

### Confirmed contrast and safe examples

- **Knight image damage** at rank1: 0.10 × Knight basic hit (before further factors); at its future maximum rank: 0.50 × Knight basic hit. This percentage is also the **selected buff-inheritance scaling**. Determine if and how passives are scaled in the next round, and keep math from double-counting inherited buffs.
- **Knight haste on image exit**: +1% for 1s at rank1 → +50% for 5s at max rank, triggered by *both* natural image lifetime completion and being destroyed.
- **Mirage Mage image**: primary basic hits are 0.10 × Mage basic damage, and secondary basic hits are 0.50 × primary = 0.05 × Mage basic damage before further modifiers, while BOTH images' **Void expiry/death explosion damages match exactly**.
- **Mage copied Metal Shield**: a separate pool attached to the illusion's nearby group, rather than a refilled or split portion of the Mage's shared pool.
- **Knight empowerment**: finite next-attacks buff, `1 + floor(empowermentRank/2)`, per-hit Mana cost scaling with rank, separate bonus Void packet with **AGI 80% / INT 20% relative scaling**, absolute damage and channel open.
- **No Tier III implementation authorization**; only the GitHub design documentation has changed. Do not affect local in-progress game build assets.

### Round 8 — next five decision areas (NOT YET APPROVED)

1. **Knight illusion rank cap and progression:** 12/16/18/20 max ranks, with a linear 10→50% damage curve and +1→50% / 1→5s ending buff, versus authored breakpoints.
2. **Knight illusion passive/buff inheritance details:** which passive modifiers copy, whether they are reduced to the 10–50% image coefficient versus applying in full, how temporary empowered basic-attack charges interact with the image, and avoid applying the image damage ratio twice.
3. **Knight Void bonus damage formula:** propose flat base and per-rank increase plus an absolute coefficient on (0.8 AGI + 0.2 INT); the damage channel is selected **after** the formula.
4. **Mirage Mage copied Metal Shield:** confirm or decline shield capacity scaling with the **same rank progression as spell-copy damage** and whether active copies retain their own pools after image destruction.
5. **Mirage Mage secondary clone ecology:** decide secondary lifetime, attack speed/reach, what happens if original image dies immediately after spawning the weaker image, and whether secondary explosions use the full primary damage/radius and snapshot attributes.

**Document status:** only Round 7 table values above are USER CONFIRMED; Round 8 options are questions, not decisions. Preserve local uncommitted Unity work.


## Round 8 — 2026-11-23 (LATEST USER CONFIRMATIONS)

The user's freeform answer to question 3 is an **exact newly approved damage formula**, not an unanswered question. The listed alternative formula examples have been declined. Other choices supersede earlier uncertainty about passives and copied Metal Shield strength.

| ID | Topic | **USER-CONFIRMED** rule | Remaining open / implementation caution |
| --- | --- | --- | --- |
| PSION3-046 | **Knight Mental Image max rank deferred** | **Do not select the Tier III Knight Mental Image max rank until designing the ENTIRE Tier III skill-point budget**. The endpoints already established still hold: clone basic damage from 10% at rank 1 to 50% at max; end buff +1% for 1 second at rank 1 to +50% for 5 seconds at max. | Whole Tier III ability roster, max spendable rank budget, individual caps and exact interpolation/gates. Do not silently reuse Tier II's 18 Image ranks. |
| PSION3-047 | **Knight image passive versus temporary-buff inheritance** | The Knight's mobile image **inherits applicable PASSIVE bonuses in full**, while **TEMPORARY BUFF effect strengths are copied at the image's rank-dependent 10%→50% scaling**. This supersedes the Round 7 wording implying both passive bonuses and temporary buffs are reduced. | Precise list of applicable passives, duration of copied buffs, live updates vs on-spawn snapshot, how to handle toggles/empowerment charges, shield/CC/utility effects. Never double-reduce base basic damage simply because image's inherited temporary buff is itself scaled. The clone's baseline basic damage is still 10%→50% of the Knight's appropriate raw basic damage. |
| PSION3-048 | **Knight additional Void damage: EXACT formula** | Each *eligible empowered basic attack* adds a separate **VOID-element** damage packet with RAW value **`30 + 17 × skillRank + 0.5 × (0.8 × AGI + 0.2 × INT)`**, with `skillRank` the **empowered-attack ability's purchased rank**, not the Mental Image rank. This is **flat 30 base + 17/rank + AGI-weighted/INT-weighted scaling**. As algebra: `30 + 17r + 0.4AGI + 0.1INT`. It remains alongside the ordinary base basic hit, not a replacement for it. At r=1, added raw `47 + 0.4AGI + 0.1INT`; at r=10, `200 + 0.4AGI + 0.1INT`. | **Physical vs Magical damage CHANNEL deliberately still OPEN**. Do not infer from Void affinity that the channel must be Magical. Rank cap, Mana cost per empowered hit (rank-scaling per earlier decision), cooldown, charge lifecycle, crit/proc eligibility, mitigation/hit event ordering, gear interactions. The number of buffed attacks remains **`1 + floor(r/2)`**, as approved in Round 5. |
| PSION3-049 | **Mirage Mage independent mirror Metal Shield scales like copied damage** | **YES**: the independent **Metal-element shared group-shield pool** cast by an image **0.4s after the Mage's shield** has a **capacity equal to the same rank-dependent percentage of the Mage's original shield capacity used for mirrored SPELL DAMAGE**, starting weak and reaching **25% of the original shield pool at the image's maximum rank**. Its AoE is **centered on the IMAGE** and covers nearby eligible allies, not a subset of the Mage's existing pool. This now makes the prior proposed shield curve APPROVED, superseding Round 6/7 hedging about needing user confirmation. | Precise rank-1 percentage, rank progression/gates, copied AoE radius, group-target snapshot, element shield priority, whether an applied copy remains after the source image dies. The full-sized Mage shield still has its own upgraded capacity and separate cast radius. |
| PSION3-050 | **Mirage Mage secondary image lifetime and independent survival** | A **secondary illusion created by the primary image mirroring Mental Image** has **HALF THE PRIMARY IMAGE'S LIFETIME** and **continues existing if its spawning primary image is destroyed**, until its own expiration or destruction. Retain Round 7: 50% primary image HP, 50% primary image basic attack damage (primary is Psion-like 10% caster basic; secondary ~5%), same FULL-strength Mirage Mage Mental Image explosion upon secondary expiration/death, and inability to copy spells or summon further images. | Exact start/expiry timestamps, whether the 50% HP rounds up, secondary movement/range/attack speed and how buff/snapshot inheritance works. Secondary is not auto-despawned on primary destruction. No control effects from mirrored spells. |

### Status after Round 8 (implementation-facing summary)

**Psionic Knight**
- 3m basic attack range, sword-projecting short psychic slashes; regular autoattack baseline still Physical.
- Retains Psionic Blades Mastery, plus **mobile Mental Image** following the Knight and attacking its target / auto-acquiring nearby hostiles. Image basic damage scales **10%→50%** by Mental Image rank, with **full applicable passive bonuses and 10%→50%-strength copied temporary buffs**; image **does not explode** and instead grants Knight **+1% attack speed for 1s → +50% for 5s** after natural expiration or destruction.
- Gains self-only **Void-element shield**.
- Gains finite empowered-attack buff affecting **`1 + floor(r/2)`** normal basic attacks. **Each empowered attack costs rank-scaling Mana** and adds Void-element RAW damage **`30+17r+0.4AGI+0.1INT`** as a separate packet. Physical/Magical channel, Mana formula and charge expiry are NOT settled; the old toggle is reserved as a potential Tier IV ability.
- **Defer ALL Tier III max-rank allocations until a complete skill roster and global Tier III point budget are agreed**, especially Mental Image.

**Mirage Mage**
- 9m basic range, 4 inherited Psion abilities: higher-scaling lower-CD Psychic Bolt, harder-hitting Lightning Storm, larger/bigger group Metal Shield, enhanced Mental Image. Primary image still attacks at **10% of the caster's basic damage**.
- Images mirror ALL spells, including utility/shield and image summons, at **0.4s delay**, with NO mirrored control effects. Mirrored damage and copied independent Metal-shield capacity use **the same rank-based percentage rising to 25% at maximum Mental Image rank** (precise lower endpoint unselected).
- Independent copied Metal Shield is cast around the illusion's location and protects nearby eligible allies. A mirror of Mental Image creates one **non-copying, non-reproducing secondary image**, with **50% HP, 50% basic attack damage and 50% lifetime** versus primary. **Secondary's explosion is FULL damage matching the primary's explosion**; on destruction of the original image the secondary does NOT disappear.
- If a primary illusion dies during the 0.4s mirror delay, pending echoes from that illusion cancel; already applied independent shields/secondary images are not automatically erased without an explicit rule.

### Round 9 — next design focus (QUESTIONS, NOT YET LOCKED)

Since the user deferred Mental Image rank cap until the **full Tier III skill-point budget** is designed, start by deciding **how many purchased skill-rank points and how many different abilities Tier III classes should offer**, then design missing Knight/Mage skills before caps/rank gates. Potential five-question interview:
1. Target Tier III full-build rank budget around 120, 150, 160 or another figure (progression unlock at Tier II Lv60; Tier III level-120 evolution eligibility, but players can overlevel; avoid conflating unlock with final skill-point budget).
2. Tier III number of distinct active/passive abilities per class: roughly 6, 7, 8+, or class-specific flexibly.
3. Knight remaining abilities: mobility/gap closer; defense-piercing multi-hit; AoE psychic slash; other concept.
4. Mirage Mage additional abilities beyond four inherited skills: new signature high-damage mirror spell, reflective AoE, damage-amplifying mirror field, or other concept.
5. Whether Knight's empowered bonus Void packet should be Physical or Magical NOW that its formula is approved, or defer pending actual damage comparisons with real World 2 enemies.

The above are only options. **No Unity implementation or local WIP changes authorized by design approvals**.


## Round 9 — 2026-11-23 (PARTIAL USER-CONFIRMED CLASS ROSTER; rank budget still OPEN)

The user supplied two concrete changes instead of answers to the five previously suggested Round 9 prompts. **Only the following two decisions are approved**. Do not assume a Tier III total rank budget, ability cap, Knight damage channel, or other unchosen Round 9 details.

| ID | Topic | USER-CONFIRMED direction | OPEN details |
| --- | --- | --- | --- |
| PSION3-051 | **Psionic Knight new passive: personal attack and movement speed + aura** | Psionic Knight gets a **new rankable PASSIVE**. Per purchased rank `r`, the Knight itself gains **+1% ATTACK SPEED per rank** and **+1% MOVEMENT SPEED per rank** as personal passive bonuses, and provides an **AURA increasing others' ATTACK SPEED by +0.1% per rank and MOVEMENT SPEED by +0.1% per rank** (the user's words: "increases everyone's"). At rank10, the component values are personal +10% attack speed/+10% movement speed and aura +1% attack speed/+1% movement speed, *before deciding interaction/stacking of self and aura*. This is **a separate passive from retained Psionic Blades Mastery**, not an addition to or replacement for Blades Mastery. Apply modifiers to the global equal unmodified speed baseline: do NOT create a unique faster class base speed. | Passive **name**, rank cap and gates, aura radius, which entities are affected (user said **everyone**; clarify whether friendly players, self, summoned units, nearby NPCs or even hostile units), line of sight, aura activation and indicators, stacking with other Knights and the Knight's self buff, whether clones inherit aura effects or generate their own aura, skill-rank scaling limits, exact additive/multiplicative stacking rules. Avoid assuming the aura buffs enemies or self until explicitly selected. |
| PSION3-052 | **Mirage Mage has enough spells for now** | The user states **"Mage has enough spells for now."** Keep **Mirage Mage's FOUR already approved inherited/evolved Psion active abilities** as its current kit: **Psychic Bolt, Mental Image, Lightning Storm, Metal Shield**. Mirror-casting is embedded in Mental Image, not an automatic fifth standalone spell; the existing abilities receive the previously confirmed Tier III improvements. **Do NOT add a new Mirage Mage spell now solely to fill a presumed class-ability count.** | Any future Mage passive or additional skills, and final ability count/rank budget, may be revisited only if the user requests them. Exact Tier III skill ranks/caps are not set. |

### Provisional roster based on actual decisions, NOT finalized rank budget

| Psionic Knight | Category | Mirage Mage | Category |
| --- | --- | --- | --- |
| Psionic Blades Mastery (retained/evolved) | Passive | Psychic Bolt (stronger/lower CD) | Active |
| Mobile Mental Image (attacks; end grants attack-speed buff) | Active | Stationary Mental Image (copies all spells, explodes) | Active |
| Personal Void Shield (new) | Active | Lightning Storm (higher damage) | Active |
| Empowered mindblade attacks: `1 + floor(r/2)` strikes, Mana and extra Void packet (new) | Active | Metal Shield (wider/stronger shared group pool) | Active |
| **NEW personal attack-/move-speed passive with weaker AoE aura** | Passive | *No additional spell approved at this time* | — |

Thus the **currently known proposed roster count** is Knight **5 abilities** (3 actives, 2 passives) and Mage **4 abilities** (4 actives), subject to future design revisions. There is no requirement to force the two classes to equal counts, as the shared ability framework can support flexible class ability numbers. **Do not infer skill point budgets, rank caps, unlocked skills or progression from these counts.** Retain Mirage Mage's existing replicated-Mental-Image logic and shields as already approved.

### Next five design questions (OPEN)

1. Does the Knight's passive **aura** apply to *allies and Knight only*, friendly summons/pets too, or literally every nearby character including hostile enemies?
2. What **radius** should the aura have (e.g. 8m, 12m, 15m), and does it need line of sight?
3. If multiple Knights are nearby, do their auras **stack**, use highest value only, or refresh/reassign? Does Knight also benefit from their own aura in addition to the +1%/rank personal buff?
4. Are we happy with **5 Knight abilities and 4 Mage abilities for now**, possibly adding passives later, or should we design further Knight actives and Mage passives before choosing budgets?
5. What **total Tier III rank-point budget** is intended: one per class level through **120**, or a larger rank capacity earned after the level-120 advancement threshold, as was done for Tier II? Do not commit an answer without user approval.

**Design-only; no Unity implementation, local WIP changes, or unrequested skills.**


## Round 10 — 2026-11-23 (LATEST USER-CONFIRMED: SHARED AURA AND TIER III PROGRESSION)

The user answered all five follow-up questions to the new speed/aura passive. This section **supersedes** earlier Round 9 open questions on aura recipients, radius, stacking, self-inclusion and Mage skill count. No new active Mage spell was selected.

| ID | Topic | USER CONFIRMED | Still open |
| --- | --- | --- | --- |
| PSION3-053 | **Aura affects ALL ALLIES** | The speed aura benefits **all nearby allies**, **not enemies**. This includes allied player characters and friendly ally-owned units such as summons/illusions when qualifying as allies. It is not a hostile-unit buff and should not turn neutral non-allies into recipients. | Precise party/team relation implementation, whether nonparty friendly NPCs count as allies, line-of-sight and how eligibility is refreshed. |
| PSION3-054 | **Aura radius = 12 meters** | The shared aura has a **12m radius** around its source. The bonuses remain **+0.1% attack speed AND +0.1% movement speed for each purchased aura skill rank** to covered allies. | Range boundary treatment, any line-of-sight check, update tick, visuals, whether ability effects persist for a short period upon leaving (none approved). |
| PSION3-055 | **Same-name aura highest-only; Knight benefits from its own aura** | Multiple copies of an aura with the **SAME NAME DO NOT STACK**; apply **only the strongest same-name aura** covering a recipient. The Knight explicitly **receives its own aura ON TOP of its +1% attack-speed / +1% move-speed per-rank personal passive bonuses**. At Knight aura rank `r` with no stronger external aura, Knight's nominal personal bonus + aura is **+1.1% attack speed and +1.1% movement speed per rank** before considering other effects. The same-name nonstack rule applies across class sources once the Mage receives this aura too. | Aura's final common name and stable shared effect ID; resolution of equal-strength auras, effect math with other differently named effects, interactions of different aura ranks on Knight's own passive and a stronger teammate's aura, pet/illusion aura generation. |
| PSION3-056 | **Mirage Mage receives SAME AURA** | **Mirage Mage also gains the SAME NAMED 12m ALLIED ATTACK-SPEED/MOVE-SPEED AURA** as Psionic Knight, with **identical +0.1% attack speed and +0.1% movement speed per purchased rank**, and strongest-only nonstacking across Knights/Mages. This is an additional **passive/aura ability**, NOT a new active spell, and is consistent with the user's earlier "Mage has enough spells for now." Mirage Mage therefore has **four active spells + one aura passive**, a known roster of **five** skills; Knight has **three active abilities + two passives**, also five known skills. | User specified *same aura*, not explicitly that Mage also receives Knight's **separate +1% per-rank personal speed bonuses**. That personal-component parity is **OPEN for clarification**, rather than silently assuming yes/no. Whether Mage receives its own aura is expected under "all allies" but may be reaffirmed if needed. Mage's passive cap, skill gates and final name still open. |
| PSION3-057 | **Tier III skill ranks continue through class level 150** | Tier III characters must be able to **continue leveling/upgrading class skills until Tier III CLASS LEVEL 150**, rather than stopping rank growth at the level-120 Tier IV evolution eligibility threshold. This applies to **both Psionic Knight and Mirage Mage**. A player may stay with the Tier III manifestation instead of immediately evolving, consistent with existing overleveling rules. | Whether skill points accrue at exactly **one point per class level** through 150 (strong precedent from Tier II but **not explicitly answered here**); whether 150 is the intended total-purchased-rank budget, rank gates by level, final skill caps, respec behavior and whether skill progression may continue after 150. Tier III -> Tier IV advancement eligibility remains **level 120** unless separately changed: do NOT mistakenly move it to 150. |

### Updated shared-speed passive roster and examples

- **Knight speed passive:** a permanent **+1% attack speed AND +1% movement speed per passive rank** on the Knight personally **PLUS** the common aura effect **+0.1% attack and move speed per rank to all allies within 12m**. Knight is eligible for its own aura, which adds to its personal bonus. Strongest aura **of the same name** takes precedence, including one emitted by a Mirage Mage.
- **Mage aura passive:** **+0.1% attack and movement speed per rank to all allies within 12m**, strongest same-name aura only; the separate Knight-size personal +1%/rank bonuses on Mage are **not yet confirmed**.
- Example, Knight passive rank 10 and no stronger competing aura: self +10% attack/move speed from personal passive, **plus own +1% attack/move aura**, nominal total **+11%** for each; an eligible nearby ally gets +1% from this aura. If another Knight or Mage has a stronger **same-name** aura, recipient gets that stronger aura INSTEAD of the weaker one, not both. Different named buffs continue following the project's usual rules, not automatically suppressed.
- **Skill rosters currently known:** Knight **Psionic Blades Mastery; mobile Mental Image; personal Void Shield; empowered attacks; speed+aura passive** (five total). Mage **Psychic Bolt; Mental Image; Lightning Storm; Metal Shield; same-name speed aura passive** (five total). Both classes remain Mana only. This updates the earlier "Mage four abilities" summary; Mage still has only **four SPELLS**.
- **Progression:** Skills are rankable through **Tier III level 150**. Tier IV eligibility at 120 and preservation of the original class manifestation remain unchanged. Need actual 150-level rank allocation, each skill cap and corresponding gates before deciding Knight image max rank.

### Proposed next five questions (OPEN; not design approvals)

1. Does the **Mirage Mage** receive the *identical full passive* including **personal +1% attack/move speed per rank**, or **only the aura**? Should Mirage Mage benefit from its own aura just like Knight?
2. For BOTH Tier III classes, does one **skill point per level through 150** apply, starting at class level 1 for **150 total**, or should earned points work differently?
3. Do the **five established abilities per class** represent final Tier III rosters, or do you want any additional skills before allocating skill-rank caps?
4. How should **the strongest same-name aura** work when two sources are at different ranks: use largest **actual aura bonus**, and allow distinct personal bonuses independently? Do allied illusions **emit their own copies** of this shared aura or only receive it?
5. Now that the skill roster and level-150 horizon are known, would you rather **delegate skill cap/rank gate allocation** to a balanced proposal, or decide the five skill caps explicitly for each class?

**Implementation boundary:** design documents only. No Unity or local WIP edits; no new active spells, aura names, talent formulas or point budgets invented beyond confirmed decisions.

## Round 11 — 2026-11-23 (LATEST USER-APPROVED STRUCTURE; RANK-GATE TABLES PROPOSED BELOW)

The user answered **YES to all five Round 10 follow-up questions**. This approves the following specific mechanics, but "yes" to drafting rank caps **does NOT approve the designer's yet-to-be-presented numeric allocation**.

| ID | Approved decision | Explicit scope / caution |
| --- | --- | --- |
| PSION3-058 | **Mirage Mage gets the SAME FULL speed passive as Knight.** At its own rank `r`, Mage personally gains **+1% attack speed AND +1% movement speed per rank**, plus the **12m all-allies aura giving +0.1% attack and move speed per rank**. Mage also receives its own aura on top of the personal component, just like Knight. | Both classes use the SAME-NAME strongest-only aura: same-named auras from Knight and Mage never stack. Personal +1%/rank component remains independently applied to each class. Previous Round 10 question of Mage self-only bonus is settled YES. |
| PSION3-059 | **Both Tier III classes earn ONE spendable class-skill point at every Tier III class level from level 1 through LEVEL 150, totaling exactly 150 skill-rank purchases.** Skills start unranked (rank 0) in the new Tier III manifestation. | Tier III -> Tier IV advancement may still be taken at class level 120; choosing to remain Tier III allows rank-up progression through 150. Do not move Tier IV eligibility to 150. No post-150 skill points or rank-cap extension approved. |
| PSION3-060 | **Exactly FIVE currently established skills per Tier III evolution are an APPROVED COMPLETE ROSTER for now.** Knight: Psionic Blades Mastery, mobile Mental Image, personal Void Shield, empowered attacks, speed/aura passive. Mage: Psychic Bolt, stationary mirroring Mental Image, Lightning Storm, Metal Shield, identical full speed/aura passive. | Do not add additional Mage spells, Knight passives, stand-alone spell mirror action, or aura toggle just to expand the kit. Other future evolution content requires separately approved design. |
| PSION3-061 | **Illusions RECEIVE applicable allied aura buffs but DO NOT EMIT/REPLICATE the shared aura.** Only actual Knight/Mage class heroes are aura sources. | This is a specific exception to the Knight illusion's generally FULL passive inheritance: it inherits its own passive-derived effects/stat bonuses where applicable, but does not spawn a second shared aura source; avoid aura multiplication/recursion through Mage clones. |
| PSION3-062 | User asks the designer to **PROPOSE** individual caps and unlock gates and provide a rank-purchase audit. | ALL NUMERIC CAPS, first unlock levels and exact rank gates in the tables below are **DESIGNER PROPOSAL AWAITING USER APPROVAL**. They were not themselves selected by the user in this round. |

### Designer-proposed complete 150-point skill rank distribution (NOT USER APPROVED)

Rationale: Each class has exactly five approved abilities and 150 points. Make **Mental Image** the deepest 40-rank investment in both, retain a substantial 30-rank attack tool, place the personal/aura speed passive at **25 ranks** for matching cross-class scaling, and reserve **25 ranks** for defensive shielding. For Knight, Mastery30 + Image40 + Void Shield25 + Empowered Attacks30 + Speed Presence25 = **150**. For Mage, Bolt30 + Image40 + Lightning Storm30 + Metal Shield25 + Speed Presence25 = **150**. This is **not** an extension of the Tier II Psion 80-rank table.

Proposed starting unlocks deliberately offer meaningful low-level play, without gifting all rank purchases: Knight **Blades Mastery, Mental Image and Empowered Attacks at class level 1; Void Shield at level 6; Speed Presence at level 10**. Mage **Psychic Bolt and Mental Image at class level 1; Lightning Storm at level 5; Metal Shield at level 8; Speed Presence at level 10**.

All listed eligibility gates refer to **the required Tier III class level to buy that rank**, not automatic rank awards. Previous rank purchase remains a prerequisite. Every class gains 1 unspent point per level. Delayed unlocks and overlapping rank gates allow build flexibility without trapping players with no legal purchase.

#### Psionic Knight — DESIGNER PROPOSAL ONLY

| Ability | First rank unlock | Proposed max rank | Final rank eligible at | Exact eligibility levels for ranks 1 through max |
| --- | ---: | ---: | ---: | --- |
| Psionic Blades Mastery | 1 | 30 | 146 | 1, 3, 8, 16, 21, 26, 30, 35, 40, 46, 51, 56, 60, 65, 70, 76, 81, 86, 90, 95, 100, 106, 111, 116, 120, 125, 130, 136, 141, 146 |
| Mental Image | 1 | 40 | 148 | 1, 2, 5, 12, 15, 19, 23, 28, 29, 34, 39, 42, 45, 48, 53, 58, 59, 64, 68, 72, 75, 79, 83, 88, 89, 94, 98, 102, 105, 108, 113, 118, 119, 124, 129, 132, 135, 138, 143, 148 |
| Personal Void Shield | 6 | 25 | 144 | 6, 7, 13, 18, 24, 32, 37, 43, 49, 54, 62, 67, 73, 78, 84, 92, 97, 103, 109, 114, 122, 127, 133, 139, 144 |
| Empowered Attacks | 1 | 30 | 147 | 1, 4, 9, 17, 22, 27, 31, 36, 41, 47, 52, 57, 61, 66, 71, 77, 82, 87, 91, 96, 101, 107, 112, 117, 121, 126, 131, 137, 142, 147 |
| Speed Presence (personal speed + allied aura) | 10 | 25 | 145 | 10, 11, 14, 20, 25, 33, 38, 44, 50, 55, 63, 69, 74, 80, 85, 93, 99, 104, 110, 115, 123, 128, 134, 140, 145 |

**Audit examples (class level → total rank gates available):** 1 → 3; 5 → 7; 10 → 12; 30 → 32; 60 → 62; 120 → 122; 148 → 150; 149 → 150; 150 → 150.

#### Mirage Mage — DESIGNER PROPOSAL ONLY

| Ability | First rank unlock | Proposed max rank | Final rank eligible at | Exact eligibility levels for ranks 1 through max |
| --- | ---: | ---: | ---: | --- |
| Psychic Bolt | 1 | 30 | 147 | 1, 3, 7, 17, 22, 27, 31, 36, 41, 47, 52, 57, 61, 66, 71, 77, 82, 87, 91, 96, 101, 107, 112, 117, 121, 126, 131, 137, 142, 147 |
| Mental Image | 1 | 40 | 148 | 1, 2, 4, 13, 16, 19, 24, 28, 30, 35, 39, 43, 46, 49, 54, 58, 60, 65, 68, 73, 76, 79, 84, 88, 90, 95, 98, 103, 106, 109, 114, 118, 120, 125, 129, 133, 136, 139, 144, 148 |
| Lightning Storm | 5 | 30 | 149 | 5, 6, 12, 18, 23, 29, 32, 37, 42, 48, 53, 59, 62, 67, 72, 78, 83, 89, 92, 97, 102, 108, 113, 119, 122, 127, 132, 138, 143, 149 |
| Metal Shield | 8 | 25 | 145 | 8, 9, 14, 20, 25, 33, 38, 44, 50, 55, 63, 69, 74, 80, 85, 93, 99, 104, 110, 115, 123, 128, 134, 140, 145 |
| Speed Presence (personal speed + allied aura) | 10 | 25 | 146 | 10, 11, 15, 21, 26, 34, 40, 45, 51, 56, 64, 70, 75, 81, 86, 94, 100, 105, 111, 116, 124, 130, 135, 141, 146 |

**Audit examples (class level → total rank gates available):** 1 → 2; 5 → 6; 10 → 11; 30 → 31; 60 → 61; 120 → 121; 148 → 149; 149 → 150; 150 → 150.


#### Exact schedule verification (performed on these tables)

- **Point sum:** `30+40+25+30+25=150` Knight; `30+40+30+25+25=150` Mage.
- **Gate count:** Exactly 150 proposed rank gates per class, one per purchased rank; strict monotonically increasing gates within each ability; every first rank is gated by its listed unlock level. Every last rank is eligible by Tier III level 150 (Knight latest at **148**, Mage latest at **149**).
- **No wasted level points:** for **EVERY class level `L = 1..150`**, the number of cumulative eligible gates is **at least `L`**. Because each ability's rank gates appear in increasing rank order and earlier ranks are prerequisites, any legal `L-1` purchases must leave at least one currently eligible next rank from some ability, so the level-`L` skill point can be spent. This is a rank-access audit, not a claim that specific skill builds are balanced.
- **Early choices:** Knight has 3 rank-1 choices available at level1; Mage has 2. The remaining initial gates arrive through level10, and by level120 the Knight has **122** eligible rank gates, Mage **121**, enough to allocate all 120 earned points without waiting for Tier IV. At level150 all **150** may be spent.
- **Current numeric effects at *proposed* cap (NOT approved caps):** both personal speed passives at rank25 would provide **+25% self attack speed and movement speed plus +2.5% same-name aura to allies within 12m** (self receives strongest aura as well). Knight image cap40 would reach its already user-confirmed **50% owner basic attack** and **+50% attack speed for 5s** on death/expiry, while Mage image cap40 reaches its previously approved **25% max mirror spell damage and copied independent group shield strength**. Knight Empowered Attacks at proposed rank30 would empower **16 basic attacks** with `30+17×30+0.4AGI+0.1INT = 540+0.4AGI+0.1INT` extra RAW Void damage per eligible hit, assuming no other rank/gear modifiers. **Its damage channel and Mana costs are still OPEN.**
- **Balance warning:** if Tier II Psionic Blades Mastery's existing provisional **+2.5 percentage points basic crit chance per rank** were simply retained at a 30-rank cap, it would grant **+75 percentage points**, which may saturate crit chance. This and very long rank30 empowerment charge sequences need deliberate balance review. **Do not assume all Tier II per-rank numeric coefficients automatically carry over to Tier III.** Rank-based speed passive at cap25 may likewise need playtesting. No class-to-class damage comparison or Unity test has been performed.

### Pending approval and focused follow-up

The user may **approve the designer's 150-point caps/unlock tables as-is** or request revisions to individual caps/unlocks. **Do not label them confirmed before explicit approval.** After the rank table is approved, remaining engineering-facing decisions include cooldowns and Mana costs for new Knight tools, Knight Void damage channel, Mental Image scaling interpolation by rank, aura name/stack priority at equal strength, exact Mage upgraded spells, copy-shield radius and targeting, and advancement quest/content.

**Scope restriction:** This round changes design documentation only; it does not authorize Unity implementation, asset modifications, branch merges, or cleanup of local uncommitted WIP.

## Round 12 — 2026-11-23 (LATEST: user corrections, revised numeric proposal)

**IMPORTANT:** The user did NOT approve the Round 11 allocation as-is. Their newest instructions supersede its proposed 40-rank Mental Image cap and the earlier approved Knight empowerment count formula. Distinguish confirmed direction from numerical proposals below.

### User-approved corrections and direction

| ID | Confirmed | Remains OPEN |
| --- | --- | --- |
| PSION3-063 | **Mental Image MAX RANK 25 in BOTH Psionic Knight and Mirage Mage**. The 15 ranks freed by lowering the previously proposed 40-rank image to 25 must be **redistributed across the OTHER FOUR abilities in EACH class**, preserving 150 skill ranks in total. | Exact redistribution and revised caps, rank gates and first unlock levels require the user's approval of a new proposal. |
| PSION3-064 | **Knight Empowered Attacks charges per cast = `1 + floor(empowermentSkillRank / 3)`**. This replaces the OLD `1 + floor(rank/2)` formula everywhere. E.g. r1=1, r2=1, r3=2, r6=3, r30=11, r34=12. Attacks still spend rank-scaling Mana, deal regular Physical basic hit PLUS separately approved Void RAW bonus `30+17r+0.4AGI+0.1INT`. | Mana formula, cooldown and duration; whether the bonus Void packet is Physical or Magical (user deferred channel). The on/off toggle remains an unimplemented **Tier IV concept**. |
| PSION3-065 | **Psionic Blades Mastery**: reduce its **critical-chance gain per rank** compared with the Tier II provisional **+2.5 percentage points/rank**, **KEEP the +1 flat Physical Defense penetration per rank**, and **ADD a small critical-DAMAGE bonus per rank**. Interpret the user's 'tabs' as the Blades passive based on context; confirm only if they meant something else. | No exact new critical chance/damage coefficients were provided. Candidate numerical values below are designer suggestions only; base ordinary crit multiplier remains 150% pending approved Tier III tuning. |

### Revised per-skill caps: DESIGNER PROPOSAL awaiting confirmation

Allocate the **15 freed ranks per class as +4/+4/+4/+3** to the four non-Mental-Image skills. Keep Round 11 first unlock proposals until amended. The user's 25-rank Image cap itself is not provisional.

| Class | Blades/Bolt | Mental Image | Void Shield/Storm | Empowered Attacks/Metal Shield | Speed passive/aura | Sum |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Psionic Knight | **34** | **25** | **29** | **34** | **28** | **150** |
| Mirage Mage | **34** | **25** | **34** | **29** | **28** | **150** |

For Mage, the last four columns correspond to Bolt, Image, Lightning Storm, Metal Shield, and Speed Presence. For Knight, they correspond to Blades Mastery, Image, personal Void Shield, Empowered Attacks, and Speed Presence.

**Consequences ONLY IF proposed caps approved:** Knight Image rank25 reaches user-confirmed 50% normal basic-attack damage and its +50% attack speed for 5s end buff; Mirage Mage Image rank25 reaches user-confirmed 25% mirror damage / matching copied Metal Shield capacity. The shape of the intermediate image rank progression remains open. At Knight Empowered Attacks rank34 the **new confirmed count formula grants 12**, rather than **18** under the superseded rank÷2 formula. Rank30 would give **11** rather than **16**. At proposed Speed Presence rank28, personal +28% attack/movement speed and +2.8% shared allied aura in 12m, with strongest same-name aura only. Images receive aura but do not emit it.

### Blades Mastery rank-by-rank crit adjustment: DESIGNER PROPOSAL only

Propose **+0.5 percentage points of basic-attack critical chance per purchased rank** (instead of Tier II's provisional +2.5pp), **KEEP +1 flat Physical Defense bypass per rank**, and **ADD +1 percentage point of basic-attack critical damage per rank** relative to the existing 150% critical-damage baseline. At the proposed 34-rank cap this gives **+17 percentage points crit chance**, **34 flat Physical Defense penetration**, and **184% critical damage multiplier** (150% + 34 percentage points) before any other modifiers. The user approved the *direction* of lower chance/same penetration/higher crit damage, **not these numeric crit percentages**. Do NOT implement/claim them as approved without separate confirmation. Define how this interacts with the Knight's illusion inheriting passives IN FULL, and with the additional empowered Void damage packet, in future design.

### Revised full eligibility-gate schedules: DESIGNER PROPOSAL only

These **replace the Round 11 proposed numeric tables** as the latest draft. Each number is the class level where its successive rank becomes purchasable; prior ranks must be purchased first. All class skill points are earned 1 per level through150, and all skills start at rank0.

#### Psionic Knight — proposed 150-rank eligibility table

| Ability | First rank unlock | Proposed cap | Last rank available | Rank eligibility levels (in rank order) |
| --- | ---: | ---: | ---: | --- |
| Psionic Blades Mastery | 1 | 34 | 147 | 1, 2, 5, 14, 18, 23, 27, 31, 35, 40, 45, 48, 53, 58, 62, 66, 71, 75, 80, 84, 88, 93, 98, 101, 106, 111, 115, 119, 123, 128, 132, 137, 142, 147 |
| Mental Image | 1 | 25 | 144 | 1, 4, 13, 20, 25, 30, 38, 43, 51, 56, 61, 68, 74, 79, 86, 91, 96, 104, 109, 117, 122, 127, 134, 139, 144 |
| Personal Void Shield | 6 | 29 | 146 | 6, 7, 9, 16, 21, 26, 33, 37, 42, 47, 52, 57, 64, 69, 73, 78, 83, 90, 95, 100, 105, 110, 114, 121, 126, 131, 136, 141, 146 |
| Empowered Attacks | 1 | 34 | 148 | 1, 3, 8, 15, 19, 24, 28, 32, 36, 41, 46, 49, 54, 59, 63, 67, 72, 76, 81, 85, 89, 94, 99, 102, 107, 112, 116, 120, 124, 129, 133, 138, 143, 148 |
| Speed Presence | 10 | 28 | 145 | 10, 11, 12, 17, 22, 29, 34, 39, 44, 50, 55, 60, 65, 70, 77, 82, 87, 92, 97, 103, 108, 113, 118, 125, 130, 135, 140, 145 |

#### Mirage Mage — proposed 150-rank eligibility table

| Ability | First rank unlock | Proposed cap | Last rank available | Rank eligibility levels (in rank order) |
| --- | ---: | ---: | ---: | --- |
| Psychic Bolt | 1 | 34 | 148 | 1, 2, 4, 15, 19, 23, 28, 32, 36, 41, 46, 49, 54, 58, 63, 67, 72, 76, 81, 85, 89, 94, 98, 102, 107, 112, 116, 120, 124, 129, 133, 138, 143, 148 |
| Mental Image | 1 | 25 | 145 | 1, 3, 14, 21, 26, 31, 39, 44, 52, 57, 62, 69, 75, 80, 87, 92, 97, 105, 110, 118, 123, 128, 135, 140, 145 |
| Lightning Storm | 5 | 34 | 149 | 5, 6, 7, 16, 20, 24, 29, 33, 37, 42, 47, 50, 55, 59, 64, 68, 73, 77, 82, 86, 90, 95, 99, 103, 108, 113, 117, 121, 125, 130, 134, 139, 144, 149 |
| Metal Shield | 8 | 29 | 147 | 8, 9, 12, 17, 22, 27, 34, 38, 43, 48, 53, 60, 65, 70, 74, 79, 84, 91, 96, 101, 106, 111, 115, 122, 127, 132, 137, 142, 147 |
| Speed Presence | 10 | 28 | 146 | 10, 11, 13, 18, 25, 30, 35, 40, 45, 51, 56, 61, 66, 71, 78, 83, 88, 93, 100, 104, 109, 114, 119, 126, 131, 136, 141, 146 |


#### Mechanical audit and review status

- **Both plans total 150 ranks**. Image is exactly **25** in each; other 4 ranks collectively increase **15** per class. First unlock levels are **proposals**, not user-set.
- **Verified no stranded earned point:** for each class level `L = 1..150`, cumulative rank gates eligible by `L` are at least `L`; previous-rank prerequisites are ordered; there are 150 gates total and all 150 may be legally purchased by level150.
- Audit check samples: Knight **1:3, 10:12, 60:62, 120:122, 150:150**; Mage **1:2, 10:11, 60:61, 120:121, 150:150**. Each entry is class level : total eligible gate count.
- Latest gate by Knight class level **148**; Mage **149**. It is not an error for ranks to be available before the final class level; players keep earning the remaining points through150.
- At rank34 Knight empowered Void bonus is `30+17×34+0.4AGI+0.1INT = 608+0.4AGI+0.1INT` RAW **per eligible empowered attack**, up to 12 attacks per activation under proposed cap. This may need balancing against cost and cooldown and does NOT decide Physical/Magical channel.
- **All revised skill caps except Mental Image25, all detailed gate timings, and proposed new crit coefficients are PENDING USER APPROVAL**. Previous 40-rank-image schedules are obsolete proposals, not accepted requirements.

### Next approval and design questions (OPEN)

1. Approve or adjust the **new Knight 34/25/29/34/28 and Mage 34/25/34/29/28 cap allocation**, first unlock levels and audited individual gate lists.
2. Approve or adjust proposed Blades Mastery **+0.5pp crit chance, +1 flat PDEF penetration, +1pp critical damage per rank**.
3. Once confirmed, settle Knight bonus Void Physical/Magical damage channel, Mana cost/cooldown and duration, Mage mirror early-rank percentage and upgraded spell coefficient/ranges.
4. Retain all previously user-approved skills, hero-only aura emitters, clone behavior, mage shield copy, and Tier III level150 progression; no Unity/gameplay coding authorized in this round.

**GitHub design documentation only. Preserve the user's substantial uncommitted local game changes.**


## Round 13 — 2026-11-23 (LATEST USER-CONFIRMED CORRECTIONS AND REBUILT GATES)

The user explicitly revised the *Round 12 draft*: move **4 points from Knight Psionic Blades Mastery to Knight speed/aura passive**, and **4 points from Mage Lightning Storm to Mage speed/aura passive**. Also specified exact Knight critical statistics and basic-attack scaling for both Tier III descendants, plus *future Tier IV directional scaling*.

### User-approved skill rank redistribution

| Psionic Knight skill | Previous Round 12 PROPOSED cap | **NEW USER-SELECTED cap** |
| --- | ---: | ---: |
| Psionic Blades Mastery | 34 | **30** |
| Mobile Mental Image | 25 | **25** |
| Personal Void Shield | 29 | **29** |
| Empowered Attacks | 34 | **34** |
| Speed Presence + allied aura | 28 | **32** |
| **Total** | **150** | **150** |

| Mirage Mage skill | Previous Round 12 PROPOSED cap | **NEW USER-SELECTED cap** |
| --- | ---: | ---: |
| Psychic Bolt | 34 | **34** |
| Stationary mirroring Mental Image | 25 | **25** |
| Lightning Storm | 34 | **30** |
| Metal Shield | 29 | **29** |
| Speed Presence + allied aura | 28 | **32** |
| **Total** | **150** | **150** |

**These revised five cap totals are the user's latest explicit allocation.** Previous Round 12 **34 Blades / 34 Storm / 28 Speed** were *proposed* values and are now **superseded**. Preserve identical +1% *personal* and +0.1% *same-name aura* attack-speed/movement-speed PER RANK for both classes, hero-only aura emitter, strongest-name-only semantics, all allies within 12m including self. At cap32 the **nominal** personal source grants +32% attack speed and +32% movement speed and the separately sourced aura grants +3.2% of each to eligible allies (self included if strongest aura). Don't invent new movement-speed baselines or silently multiply effects without the shared stat rules.

### Knight Psionic Blades Mastery exact critical rules — USER CONFIRMED

- The passive's **rank cap = 30**.
- **Each purchased Psionic Blades Mastery rank grants +1 percentage point BASIC-ATTACK critical-hit chance**. With the game's opt-in crit baseline of **0%**, the intended class mastery component reaches **30% at rank 30**, before other explicitly authored modifiers. The previous Round 12 +0.5pp/rank designer proposal is superseded.
- **Critical damage multiplier starts at 160% before purchased ranks**, then gains **+1 percentage point per purchased Mastery rank**, reaching **190% at rank 30**. `criticalMultiplier(r)=1.60+0.01r` for `r=0...30`. Example rank1: 161%, rank15: 175%, rank30: 190%. If "start at 160%" was intended for rank1 rather than the unranked baseline, clarify next time; this definition exactly matches the user's explicit rank30 190% target with 1pp/rank increases.
- Preserve **+1 flat PHYSICAL DEFENSE bypass per purchased Mastery rank** from the parent Psion mastery, for **30 flat PDEF ignored at rank30**, on eligible normal basic attacks. Do not replace this with percentage armor penetration.
- Damage-crit permission remains **BASIC/AUTOATTACK-specific** unless separately authorized; critical status of the *separate additional Void-element empowered-hit packet* is **still not specified**. Knight illusion applies eligible passives in full, but autoattack raw damage remains its own 10–50% image-rank fraction. No universal global critical rule change intended.

### Approved class-specific BASIC/AUTOATTACK stat scaling

| Evolution | Tier | **Ordinary basic-attack stat coefficients** | Total coefficient | Status |
| --- | --- | --- | ---: | --- |
| Psion (baseline reference only) | II | `0.45×AGI + 0.45×INT` | 0.90 | Parent unchanged |
| **Psionic Knight** | **III** | **`0.60×AGI + 0.40×INT`** | **1.00** | **USER APPROVED** |
| **Mirage Mage** | **III** | **`0.60×INT + 0.40×AGI`** | **1.00** | **USER APPROVED** |
| Knight descendant (unnamed) | IV | **`0.90×AGI + 0.20×INT`** | **1.10** | **USER-APPROVED FUTURE DIRECTION ONLY; no Tier IV design/implementation authorized** |
| Mirage Mage descendant (unnamed) | IV | **`0.90×INT + 0.20×AGI`** | **1.10** | **USER-APPROVED FUTURE DIRECTION ONLY; no Tier IV design/implementation authorized** |

**Critical distinction:** These changes apply to the **ordinary BASIC/AUTOATTACK stat contribution**, not to all spell damage. They do **NOT** modify the already approved additional Knight empowered-hit **Void damage formula `30+17r+0.4AGI+0.1INT`**, nor do they change the Physical/Magical damage channel of that bonus (still OPEN). The parent Psion's basic attack remains ordinarily Physical; this stat-scaling approval alone does not change basic hit channel, attack timing, gear requirements, targeting, rank effects or illusion explosion coefficients. The class IV formulas are future lineage intent, NOT unlock/quest/skill design or blanket every-class rules.

### Rank eligibility tables recomputed for latest caps — DESIGNER PROPOSAL ONLY

**DO NOT CONFUSE the USER-SELECTED per-skill MAX CAPS above with unapproved exact eligibility gates and first unlock levels below.** First unlock levels retained from the Round 11/12 draft for continuity, but the user never explicitly accepted the numerical gates; review them before implementation. Skills rank 0 initially, one earned point per Tier III class level 1–150, earlier purchased rank is prerequisite to the next.

#### Psionic Knight — latest PROPOSED eligibility gates

| Ability | First unlock level (proposed) | Maximum ranks | Final rank gate | Exact proposed eligible class levels for each purchased rank |
| --- | ---: | ---: | ---: | --- |
| Psionic Blades Mastery | 1 | 30 | 146 | 1, 3, 8, 16, 21, 26, 30, 35, 41, 46, 51, 56, 60, 65, 70, 77, 82, 87, 91, 96, 101, 106, 112, 117, 121, 126, 131, 136, 141, 146 |
| Mental Image | 1 | 25 | 144 | 1, 4, 13, 19, 25, 31, 38, 44, 50, 55, 61, 68, 74, 79, 86, 92, 97, 103, 109, 116, 122, 128, 134, 139, 144 |
| Personal Void Shield | 6 | 29 | 145 | 6, 7, 9, 17, 22, 27, 33, 37, 42, 47, 53, 59, 64, 69, 73, 78, 83, 89, 94, 100, 105, 110, 114, 120, 125, 130, 135, 140, 145 |
| Empowered Attacks | 1 | 34 | 148 | 1, 2, 5, 14, 18, 23, 28, 32, 36, 40, 45, 49, 54, 58, 63, 67, 72, 75, 80, 84, 88, 93, 98, 102, 107, 111, 115, 119, 124, 129, 133, 138, 143, 148 |
| Speed Presence | 10 | 32 | 147 | 10, 11, 12, 15, 20, 24, 29, 34, 39, 43, 48, 52, 57, 62, 66, 71, 76, 81, 85, 90, 95, 99, 104, 108, 113, 118, 123, 127, 132, 137, 142, 147 |

**Audit:** 150 total ranks; all earned points legally spendable at every class level 1–150; latest rank gate class level 148.

#### Mirage Mage — latest PROPOSED eligibility gates

| Ability | First unlock level (proposed) | Maximum ranks | Final rank gate | Exact proposed eligible class levels for each purchased rank |
| --- | ---: | ---: | ---: | --- |
| Psychic Bolt | 1 | 34 | 148 | 1, 2, 4, 15, 19, 24, 28, 32, 36, 41, 46, 50, 55, 59, 64, 68, 73, 76, 81, 85, 89, 94, 98, 103, 108, 112, 116, 120, 125, 130, 134, 139, 144, 148 |
| Mental Image | 1 | 25 | 145 | 1, 3, 14, 21, 26, 33, 39, 45, 51, 56, 62, 69, 75, 80, 87, 93, 99, 104, 110, 117, 123, 129, 135, 140, 145 |
| Lightning Storm | 5 | 30 | 147 | 5, 6, 7, 17, 22, 27, 31, 37, 42, 47, 52, 57, 61, 66, 71, 78, 83, 88, 92, 97, 102, 107, 113, 118, 122, 127, 132, 137, 142, 147 |
| Metal Shield | 8 | 29 | 146 | 8, 9, 13, 18, 23, 29, 34, 38, 43, 49, 54, 60, 65, 70, 74, 79, 84, 90, 95, 101, 106, 111, 115, 121, 126, 131, 136, 141, 146 |
| Speed Presence | 10 | 32 | 149 | 10, 11, 12, 16, 20, 25, 30, 35, 40, 44, 48, 53, 58, 63, 67, 72, 77, 82, 86, 91, 96, 100, 105, 109, 114, 119, 124, 128, 133, 138, 143, 149 |

**Audit:** 150 total ranks; all earned points legally spendable at every class level 1–150; latest rank gate class level 149.

#### Gate audit and consequences

- **Exact totals:** each class contains exactly 150 eligible purchases; all approved skill caps above are matched. **For EVERY class level 1–150**, cumulative eligible rank purchases meet/exceed points earned, so there is **no level at which a point must be stranded**. Strict increasing rank gates and first-rank unlocks checked for each skill.
- Checkpoint eligible gate counts **Knight: 1→3, 10→12, 60→62, 120→122, 150→150**; **Mage: 1→2, 10→11, 60→61, 120→121, 150→150**. Latest gates at level **148 Knight / 149 Mage**; all 150 rank points can be spent by class level150.
- Knight Empowered Attacks still max34 and uses the **APPROVED `1+floor(rank/3)`**, hence 12 empowered strikes per cast at rank34, with rank-scaling Mana cost and bonus Void packet `30+17r+0.4AGI+0.1INT`. The **new normal 0.6AGI+0.4INT attack scaling does not replace the separate empowered Void formula**.
- Knight Image max25: basic attacks 10%→50% of caster normal basic value; its ending speed buff +1% for 1s→+50% for5s; full inherited passives and scaled temporary buffs. Mage image max25: primary basic damage 10% of owner basic; mirrors all spells in 0.4s including independent Metal group shield with copied strength reaching 25% at max; weaker half-HP / half-basic / half-duration secondary full-explosion image remains as approved.
- At shared Speed Presence max32, the self+allies values are large and require real-world playtesting. No balance simulation was performed in Unity.
- Tier III→IV eligibility remains level120; Tier III skill progression still through150, one point per class level. Tier IV scaling numbers above are future intentions only.

### Next review points

1. Approve or adjust the **exact rank eligibility gates** and unlock levels for the **newly confirmed caps**. The individual cap totals themselves were selected by the user; only purchase timing remains a proposal.
2. Confirm Knight crit multiplier baseline treatment of **160% at rank0→190% at rank30** (versus exactly160% at rank1), if relevant.
3. Decide the still-open **Physical or Magical channel** for Knight's extra Void empowerment damage, Knight Mana cost / cooldown / charge expiration, Mirage Mage improved spell damage coefficients and mirror low-rank scaling, and exact cast/clone edge cases.
4. Preserve all previously confirmed shield, aura, mana-only and world progression rules. **Documentation only; no Unity code/assets/WIP modifications or branch merges authorized.**


## Round 14 — 2026-11-23 (LATEST: EXACT RANK-GATE APPROVAL)

**USER APPROVED** the regenerated **Round 13 per-rank eligibility schedules and first-rank unlock levels AS WRITTEN** in the two "latest PROPOSED eligibility gates" tables directly above. Those tables are hereby **CONFIRMED**, not merely proposals. Earlier Round 11 / Round 12 gate tables are **HISTORICAL AND SUPERSEDED**. No reroll of the level-gate schedules is authorized here.

### Fully approved progression, effective Round 14

- **Psionic Knight approved caps (150 ranks):** Psionic Blades Mastery **30**, Mental Image **25**, personal Void Shield **29**, Empowered Attacks **34**, Speed Presence **32**.
- **Mirage Mage approved caps (150 ranks):** Psychic Bolt **34**, Mental Image **25**, Lightning Storm **30**, Metal Shield **29**, Speed Presence **32**.
- **Approved first-rank unlock levels:** Knight Mastery **1**, Image **1**, Void Shield **6**, Empowered Attacks **1**, Speed Presence **10**. Mage Psychic Bolt **1**, Image **1**, Lightning Storm **5**, Metal Shield **8**, Speed Presence **10**.
- **All 150 individual rank gates per class** in Round 13's exact five-row tables are **approved**. Every gate is a required class level for the purchased rank; prior rank must be purchased. Rank points are earned **one per Tier III class level 1–150**. Both gate tables passed the all-level audit: no earned skill point is forced to remain unspendable at any level 1–150; last eligible ranks arrive by Knight level **148** and Mage level **149**.
- Tier III level **120** continues to unlock *optional* Tier IV advancement. A player remaining Tier III may keep leveling and spending skill points through 150. The existing Psion and Novice manifestations remain saved when evolving.
- **This approval covers rank caps, unlock levels, and exact rank-purchase gates only.** It does **not** approve outstanding cooldown/Mana numbers, Knight bonus Void channel or mirrored damage lower-rank coefficients, nor grant Unity implementation, branch merges or updates to locally uncommitted gameplay assets.
- When Codex implements the class progression later, **use the exact approved Round 13 gate lists without generating different schedules**. The historical proposal wording adjacent to those tables should be read as **superseded by this later approval**.

### Next design phase (OPEN)

Resolve per-skill Mana costs/cooldowns/cast times, Knight Void packet Physical/Magical channel, Mental Image intermediate-rank interpolation and mirror copy strengths, shield scaling and the remaining targeting/snapshot edge cases. Do not reopen approved gates except by a new user instruction.


## Round 15 — 2026-11-23 (LATEST: Equipment approvals + delegated provisional combat balance)

The user explicitly **delegated selecting Mana costs and cooldowns** to the designer for now and asked for **Tier III spell damage, stat scaling and shields to exceed the parent Tier II Psion values**. The user also approved class-specific equipment-family roles and shared, cross-class weapon options. **Keep formulas below labeled DESIGNER-PROVISIONAL, not user-approved final balance.** The user's equipment intentions and delegated right to author provisional numbers ARE approved. This round **does NOT authorize Unity implementation**.

### Approved equipment identities and compatibility

| Slot / identity | **Psionic Knight** | **Mirage Mage** | Design status |
| --- | --- | --- | --- |
| Primary/signature MAIN HAND | **Mental blades / psionic blades** (name alternatives; final standardized equipment-family name pending). These can visually generate the Knight's approved short mindblade slashes. | **Tomes and cards** as the characteristic main-hand casting implement. Spell VFX should originate appropriately from these weapons. | **USER APPROVED concept** |
| Other compatible MAIN HANDS | **Generic AGI-oriented melee blades** shared with other Agility melee classes. Do NOT restrict all such blades to Knight or force it to use only Psionic-branded blades. | **Generic INT-oriented weapons**, not exclusively Mage-only cards/tomes. Actual compatible item families need authored compatibility records. | **USER APPROVED permissive design** |
| Signature OFF HAND | **Ionic stones** (user's exact name). Some favor AGI/autoattack combat, others INT/spellcasting. Knight will generally prefer AGI types, but both variants remain legitimate gear. | **The same Ionic stones**, including INT/spell-focused and AGI/basic-attack-focused variants, as a shared category. Mage generally prefers the INT types but isn't blocked from AGI types. | **USER APPROVED**, no automatic dual-wield restriction or forced same-stat pairing |
| Additional compatible OFF HANDS | **Generic AGI-oriented offhand items** used by other classes, not only Ionic stones. | **Generic INT-oriented offhand items**, not only Ionic stones. | **USER APPROVED** |
| General equipment / armor preference | **Melee Agility-oriented gear**, primarily AGI gear/stat budgeting and associated supportive attributes. | **Caster-oriented gear**, primarily INT gear/stat budgeting and caster-oriented secondary stats. | **USER APPROVED preference**. Loot docs already do NOT use a rigid universal armor-weight defense taxonomy, so don't invent a heavy/medium/light armor system or deny mixed-stat item use without an explicit compatibility rule. |

**Equipment implementation cautions:** These are **equipment-family availability and gear identity choices**, not instruction to mass-create items now. Equipment uses ordinary stat budgets/intrinsic/affix/requirements systems and shared slot/compatibility rules. Keep both classes' existing **Mana-only** resource. An Ionic stone is a normal stat-bearing **offhand** (potential Intrinsics can be authored later), NOT a required charged secondary combat resource. The user used *mental blades or psionic blades* as alternatives; do not invent two mandatory class-specific weapon families or prohibit existing parent Psion Mindblade + Codex gear. Psion parent weapon/catalog migration, exact dual-wield permissions, art, loot/drop locations, rarity distribution and active equipment eligibility filters remain implementation/content decisions requiring separate approval.

### Designer-provisional ability tuning (authorized to propose, NOT final-playtested)

All costs are **Mana**, purchased skill rank **r ≥ 1** for an unlocked active skill. Literal `base + coefficient × r` means rank 1 includes the increment. Passives cost **0 Mana** and have **no cooldown**. All abilities still respect the existing skill rank caps/approved individual rank gates.

#### Psionic Knight (Tier III)

| Ability | Cast / activation | Provisional cooldown | Provisional Mana | Provisional output / scope |
| --- | --- | --- | --- | --- |
| **Psionic Blades Mastery** (30 ranks) | Passive | None | 0 | User-approved rank effects retained exactly: **+1 percentage point BASIC crit chance/rank; +1 flat PDEF bypass/rank; base crit multiplier 160% +1 percentage point/rank** (190% max). |
| **Mobile Mental Image** (25 ranks) | **0.2s summon cast**, mobile following Knight | **12s** | **`18 + 5r`**, r25 = **143** | User-approved damage/attack-speed endpoints, not changed: clone normal basic attack = 10% at r1 →50% at r25 of Knight basic hit, passives apply fully and copied temporary buffs at image percentage. On expiry OR enemy death the clone **does not explode**, but grants Knight +1% haste for 1s at r1→+50% haste for 5s at r25. **Provisional lifetime** `1.6 + 3.4×(r−1)/24` seconds (1.6→5s), following parent's duration endpoints. |
| **Personal Void Shield** (29 ranks) | **0.2s cast**, self-only | **22s** | **`18 + 5r`**, r29 = **163** | **Void-element** shield with **one personal pool `180 + 48r + 0.8INT + 0.4AGI`** raw shield HP, **6s duration**. At r29 = **1,572 + 0.8INT + 0.4AGI**. Elemental 0.8 matching / 1.2 other damage shield-HP consumption per prevented eligible damage; shield remains strictly self-only, no ally-wide copied pool. |
| **Empowered Attacks** (34 ranks) | **0.1s activation**, next-attacks buff (NOT a toggle) | **14s** | **0 Mana when activating; `5+r` Mana EACH eligible empowered strike**, r34 = **39 per strike** | Next **`1+floor(r/3)`** basic hits (max **12** at r34) carry separate USER APPROVED raw **Void** bonus `30+17r+0.4AGI+0.1INT`. **Provisional 12-second charge expiry** after activation; cooldown begins activation. Normal base basic hit remains separate Physical channel. The extra Void packet's Physical/Magical damage CHANNEL remains OPEN and is NOT inferred from its element. |
| **Speed Presence** (32 ranks) | Passive and always-on allied aura | None | 0 | User-approved **+1% personal attack/movement speed per rank**, **+0.1% per rank to all allies including self within 12m**; strongest SAME-NAME aura only. Clones receive allied aura but cannot become additional aura emitters. |

**Knight cost behavior not yet approved:** What happens on insufficient Mana during an already empowered attack, whether misses use charges/Mana, whether image attacks can consume charges, effect of recasting with charges left, and exact stun/animation interrupt details. Provisional 12s expiry is balance-only and may be changed. The self-only Void shield uses the game's ordinary elemental-shield rules.

#### Mirage Mage (Tier III)

| Ability | Cast | Provisional cooldown | Provisional Mana | Provisional stronger Tier III output |
| --- | --- | --- | --- | --- |
| **Psychic Bolt** (34 ranks) | **0.1s cast**, usual targeted projectile | **5.5s** (faster than parent **8s**) | **`10+4r`**, r34=**146** | **Metal-element Magical RAW damage `45 + 11r + 1.5INT`**; r34 = **419+1.5INT**. Tier II parent `30+8r+1.2INT`; both per-rank and INT coefficients are higher. Original stun remains `min(0.15r,3.0s)` before Tenacity: retain parent's **3s maximum**, rather than granting unintended 5.1s stuns at r34. Image echoes do NOT replicate stuns. |
| **Mental Image** (25 ranks) | **0.2s ground placement cast**, stationary main image | **12s** | **`22+5r`**, r25=**147** | **Original image enemy-only 4m MAGICAL Void expiry/death RAW explosion `55 + 15r + 0.75AGI + 0.9INT`**, r25 = **430+0.75AGI+0.9INT**, up from Tier II parent `30+10r+0.6AGI+0.6INT`. Provisional primary duration `1.6+3.4×(r−1)/24` seconds (1.6→5.0s). Primary's ordinary basic attacks remain at **10%** of owner's basic damage and full inherited attack speed; snapshot and 3.5× incoming damage follow parent. |
| **Lightning Storm** (30 ranks) | **0.1s initial cast + 0.6s movement-interruptible channel** | **14s** (parent 15s) | **`22+6r`**, r30=**202** | Three independent **Lightning-element Magical RAW waves**, each **`25 + 7r + 0.9INT`**, r30 = **235+0.9INT per wave**. This exceeds parent's `15+5r+0.6INT` per wave. Preserve parent's ground-targeted **4m radius**, three-wave timing, live membership checks and shared channel interruption. |
| **Metal Shield** (29 ranks) | **0.2s cast**, centered on Mage | **26s** (parent 30s) | **`24+5r`**, r29=**169** | **One shared Metal shield absorption pool `160 + 42r + 1.05INT`**, r29 = **1,378+1.05INT**, cast-time protection radius **10m** (parent 8m), protection **8s** as parent. Pool belongs to caster, protects qualifying ally group (not once per member), remains if protected allies leave initial cast radius. Oldest-overlap pool order and same-caster recast rules retained. 0.8 matching / 1.2 other normal elemental shield consumption. |
| **Speed Presence** (32 ranks) | Passive and always-on allied aura | None | 0 | Exactly same personal +1% attack/move and allied +0.1% attack/move **per rank** and 12m strongest-only shared aura as Knight. No extra active spell. |

**Mage Mental Image mirror-specific provisional scaling:** copies ALL original Mage spells, including **Metal Shield** and **Mental Image**, with user-approved **fixed 0.4s delay**, at same enemy/ground position where meaningful, but duplicated Metal Shield is an **independent smaller 12m? NO**: copy remains centered on the **ILLUSION**, protecting allies near it; **do not use the common Speed Presence 12m radius for shield**. As a **designer-provisional** curve for the previously approved "weak early →25% at image rank25" goal, use **`mirrorPercent(r) = 5% + 20%×(r−1)/24`** for rank1..25, for mirrored **offensive damage AND separately spawned independent group Metal shield capacity**. At rank25 each is 25% of the corresponding original spell raw damage / shared shield pool. Proposed copied shield coverage radius **8m** about each illusion; distinct independent shared capacity per copied shield, never add it to the Mage's main shield. The copied Mental Image makes **one NON-MIRRORING secondary image** with 50% primary HP/basic damage/lifetime, but the SAME FULL damage Void death/expiry explosion as primary. Secondary survives parent image death; primary image's *pending* 0.4s echoes cancel on death. No mirrored CC or extra stun. No recursive copies.

**Provisional echo Mana policy:** Owner pays Mana only for original spell casts; image echoes **cost no additional Mana** (including secondary image summoning and copied shields), to avoid double costs from a passive copy system. Echo death-interruption and other snapshot behavior still follow previously user-approved decisions. This is **designer tuning**, not a new resource or final rule.

### Exact baseline comparisons for playtesting

| Parent Tier II Psion | Tier III Mirage Mage (designer provisional) | Reason |
| --- | --- | --- |
| Bolt damage `30+8r+1.2INT`, CD8s | `45+11r+1.5INT`, CD5.5s | Higher rank growth, higher stat coefficient, faster recharge |
| Storm per wave `15+5r+0.6INT`, CD15s | `25+7r+0.9INT`, CD14s | Higher flat, rank and INT scaling; retained three waves |
| Shared Metal Shield `100+30r+0.75INT`, cast8m, CD30s | `160+42r+1.05INT`, cast10m, CD26s | Higher per-rank and INT capacity scaling, larger cast radius |
| Image explosion `30+10r+0.6AGI+0.6INT`, r18 max | `55+15r+0.75AGI+0.9INT`, r25 max | Stronger flat/rank/stat scaling, still 4m Magical Void, enemies only |
| No parent self-only Void shield | Knight's `180+48r+0.8INT+0.4AGI` PERSONAL Void shield | Higher numerical personal protection vs parent group shield, with different scope and elemental affinity |

**Validation needed after implementation:** Actual class max Mana pools, regeneration, realistic gear attributes, cast frequency/time-to-kill, damage/defense scaling in World 2, critical interactions, overlap among mirrored independent shields, and whether spending `12×39=468` Mana for a full rank34 Empowerment is sustainable. All numbers above are **provisional**; no numerical combat simulation or Unity test has been run here. Continue respecting previously user-APPROVED rank caps and the EXACT rank gates in Round 13/14.

### Remaining Round 16 design interview: FIVE OTHER topics, not the already-answered equipment/cost choices

1. **Tier III advancement quest direction:** Do Knight and Mirage Mage share an opening Psion-lineage quest that splits into two specialist trials, use completely separate mentors/quests, or have self-selected mastery challenges? Exact World2 area/NPC still open.
2. **Knight mobility and silhouette:** Should its short 3m projected-slash style favor deliberate positioning with NO extra mobility skill, or should existing generic equipment provide a dash/teleport effect? Do NOT invent a sixth class ability without explicit revision.
3. **Primary illusion on OWNER death:** Should already summoned Mage/Knight images stay alive for their remaining duration and keep their preexisting behavior, or vanish immediately? Parent Psion image originally survives caster death; the Knight buff-to-dead-owner edge still needs design.
4. **Clothing/weapon visual identity:** Should generic AGI blades use exactly the same projected mindblade slash VFX as signature psionic blades, or have distinct visuals while keeping identical 3m damage/range rules? Should Mage cards/tomes cast with distinctive visual motions or identical spell VFX?
5. **Illusion threat and player interaction:** How should Mage's stationary image and Knight's mobile image attract enemy aggro? Parent image is a real targetable unit with collision/threat. Does the user want enemy AI to target clones normally, prioritize the real player, or recognize/projectile intercept illusions? Retain Tier II targetability unless changed.

These are potential questions to ask next; no user approval yet. **DOCUMENTATION ONLY.** Do not alter Unity implementation, preserve substantial local uncommitted World2 changes.


## Round 16 — 2026-11-23 (LATEST: USER-APPROVED MECHANICS AND REVISED DESIGNER BALANCING)

The user answered the previous five questions and added an explicit new scaling principle: **Mana should scale MUCH more strongly per purchased skill rank, roughly 20 Mana per rank for a normal cast; damage and shield stat coefficients should themselves rise with the rank**, instead of treating AGI/INT as fixed per-cast multipliers. These instructions **supersede Round 15's weak provisional Mana progression and static Tier III stat coefficients**. Preserve the approved class identity, skill caps, rank gates, equipment families, and clone/summon rules.

### Five USER-CONFIRMED mechanics

| ID | Approved mechanic | Boundaries / pending implementation detail |
| --- | --- | --- |
| PSION3-066 | **Two separate hidden teachers and exploration-led advancement:** Psion progresses to either Tier III Psionic Knight or Mirage Mage by finding the relevant **hidden teacher through world exploration**. The two teachers and advancement experiences are **separate**, not a single obvious shared trial or an automatically offered selection list. | Exact World 2 zone, NPC names/looks, environmental clues, level prerequisites beyond established Tier II Lv60→new Tier III Lv1 eligibility, steps/trials and rewards remain OPEN. Don't invent locations or force a common teacher. Preserve parent manifestation on advance. |
| PSION3-067 | **Knight mobile Mental Image SURVIVES Knight death**, keeps attacking valid hostile targets for its **remaining normal lifetime**, then ends under its normal expiry rules. It is not despawned immediately on owner death. | Knight image already grants an attack-speed buff on natural expiry/enemy destruction; when Knight is dead, do not resurrect the Knight, grant an effect to a corpse, or imply the buff persists through revival without deciding that separately. The clone's AI should not require a living owner to continue valid combat. |
| PSION3-068 | **Mirage Mage mirrored targeted spell RETARGETS the NEAREST VALID ENEMY** if the Mage's originally selected enemy dies within the **0.4-second delayed echo**. This choice supersedes "fizzle" or "last position" choices. | Validate range/LoS and what happens when no valid enemy exists (designer-proposed: fizzle); ground-targeted spells continue using their authored ground position, shields use illusion-centered friendlies, secondary image copy is not hostile retargeted. If image dies within delay, pending mirror is **CANCELED** under existing confirmed rule before attempting any retarget. |
| PSION3-069 | **Copied group Metal Shield uses OLDEST-ACTIVE-POOL FIRST**, exactly as the parent global shield consumption convention. When an ally has both Mage's ORIGINAL shared Metal pool and an illusion's independent mirrored group Metal pool, consume whichever is **older first**, then the next eligible pool. | This is NOT always original-first; if casting chronology changes, chronological active pool age controls. Mirrored pool stays **independent** and centered at image cast position, 0.4s after original, with rank-percentage-matching shield capacity. Shield-element coefficients still 0.8 matching, 1.2 nonmatching, unless global rule changes. Overlapping multi-image ties deterministic order unresolved. |
| PSION3-070 | **Knight empowered Void bonus is MAGICAL Void**: ordinary Knight autoattack damage packet is **Physical** and resolves through Physical Defense, while additional ranked Mana-spending Void bonus damage packet is **Magical** and resolves through Magical Defense, with the Void element's relevant resistance rules. **This explicitly closes the long-open Physical vs Magical bonus channel question.** | The Knight's +1 flat PDEF-bypass/rank Psionic Blades Mastery applies to the normal Physical basic packet only unless an authored exception says otherwise; do not apply PDEF reduction to the separate Magical packet. Critical-hit permission, whether the Magical bonus shares autoattack crit and combined event ordering still OPEN. |

### USER-CONFIRMED DIRECTION for rank scaling (numerical values below delegated and provisional)

- **Mana:** Previously documented Rank15 cost multipliers of +4 to +6 Mana/rank for normal spells were too small. Target about **+18 to +22 Mana per purchased rank per regular cast** in the initial Tier III prototype. Knight's empowerment pays Mana **per empowered HIT** up to 12 hits; do **NOT** blindly impose +20 Mana per rank on each hit (which would cost at least 680 Mana per strike, 8,160 Mana for 12 hits at r34). Instead increase per-hit scaling meaningfully with the total-combo cost taken into account.
- **Damage:** Tier III offensive spell coefficients must be **rank-dependent**, e.g. `base + flatIncrement × r + (startingStatCoefficient + coefficientIncrement × (r−1)) × stat`. This is mathematically a **constant stat term PLUS a rank × stat interaction** and matches the user's `flat + flat×rank + attribute×coefficient×rank` principle while giving sensible rank1 baseline values. When a spell uses multiple attributes, each term can get rank scaling, with the class' established AGI/INT priorities retained.
- **Shields:** User asked earlier for stronger Tier III shield values and scaling; use the same rank-dependent stat coefficient principle for Tier III shield capacity as a consistent design choice. The mirrored shield's capacity remains a fixed rank-dependent **percentage of original Mage Metal Shield**, max 25%, and is not separately multiplied by an additional rank coefficient after that (avoid double scaling).
- The exact numeric values are **DESIGNER-SELECTED PROVISIONAL TUNING**, not separately user-approved permanent formulas; compare with practical class Mana pools, regen, gear and enemy mitigation before any implementation. Tier II Psion baseline is unaffected.

### REVISED numerical balance sheet (ROUND 16; supersedes Round 15 proposed Mana and output formulas)

Use purchased skill rank `r`, **not class level**, for formula evaluation. **NO change to the already approved cooldown and casting concept unless explicitly specified.** Multiplying stats by rank is restricted to the listed skill effects, not generic hero autoattack scaling.

| Class / ability | Provisional Mana cost | Cooldown | Latest designer-provisional raw damage / shield capacity |
| --- | --- | --- | --- |
| Knight **Mobile Mental Image**, max25 | **`16+20r`** per summon (**516** at r25) | 12s | Image's basic attack 10%→50% of Knight physical basic attack by Image rank; full-passive / scaled-temporary-buff inheritance; +1% attack speed for1s→+50% for5s on end; **no explosion**. Provisional owner-death survival explicitly confirmed. |
| Knight **Personal Void Shield**, max29 | **`18+20r`** per cast (**598** at r29) | 22s | Self-only Void shield HP **`180+48r+(0.50+0.03r)AGI+(0.85+0.055r)INT`** (**1,572 + 1.37AGI + 2.445INT** at r29). Provisional 6s duration, ordinary element-matching shield absorption. |
| Knight **Empowered Attacks**, max34 | 0 activation Mana, **`4+2r` per empowered HIT** (**72** at r34, 12 charges → **864** total if all spent) | 14s | User-approved charge count **`1+floor(r/3)`**. The OLD explicitly approved raw packet **`30+17r+0.4AGI+0.1INT`** is the baseline. **PROPOSED rank-scaling extension, for review**: **`30+17r+[0.4+0.016(r−1)]AGI+[0.1+0.004(r−1)]INT`** Magical Void. At r1 this reproduces the old raw formula exactly; at r34 it gives **`608+0.928AGI+0.232INT`** and preserves **80% AGI / 20% INT relative weighting**. The user's latest instruction to make damage coefficients increase with rank motivates this PROPOSAL; until numerical sign-off, do not silently erase the earlier user-approved exact old packet. Standard basic attack remains Physical. Provisional empowered charges expire 12s after activation. |
| Mage **Psychic Bolt**, max34 | **`10+20r`** per cast (**690** at r34) | **5.5s** | Metal Magical single-target **`45+11r+(1.30+0.10r)INT`** (**419+4.7INT** at r34), from parent's `30+8r+1.2INT`. Keep original up-to-3s stun subject to Tenacity; mirrored copy has NO stun. |
| Mage **Mental Image**, max25 | **`20+20r`** per primary cast (**520** at r25) | 12s | Full enemy-only 4m Magical Void explosion **`55+15r+(0.65+0.04r)AGI+(0.80+0.07r)INT`** (**430+1.65AGI+2.55INT** at r25). Stronger at rank1 and beyond versus parent's `30+10r+0.6AGI+0.6INT`. Primary basic 10% caster basic; secondary full explosion with half HP/basic damage/lifetime and no recursive copying. |
| Mage **Lightning Storm**, max30 | **`20+22r`** per cast (**680** at r30) | 14s | Each of **three** Lightning Magical waves **`25+7r+(0.70+0.07r)INT`** (**235+2.8INT per wave** at r30). Parent `15+5r+0.6INT` per wave. Preserve 4m AoE, live target checks and 0.1s cast +0.6s interruptible channel. |
| Mage **Metal Shield**, max29 | **`20+22r`** per cast (**658** at r29) | 26s | One shared Metal group capacity **`160+42r+(0.85+0.055r)INT`** (**1,378+2.445INT** at r29), centered on Mage in 10m original-cast radius for 8s. Parent `100+30r+0.75INT`. Copies are separate pools and use rank-dependent 5%→25% **PROVISIONAL** image-copy percentage, centered on image; chronological **oldest active pool first** on shared ally. |
| Either class **Speed Presence** | 0 | Passive / no cooldown | Approved permanent +1% personal attack/move speed per rank, +0.1% attack/move allied 12m same-name strongest-only aura; images benefit but don't emit aura. |
| Knight **Psionic Blades Mastery** | 0 | Passive / no cooldown | Approved +1pp crit chance and +1 flat PDEF bypass per rank; basic-attack crit multiplier 160% baseline +1pp per rank to 190% at rank30. |

**Explicit comparisons / test caution:** The high max-rank Mana costs above are INTENTIONALLY much larger than Round15. They **may exceed a Tier III character's actual Mana pool**; resource feasibility has NOT been established. Do not describe these as balanced, automatically viable, or fully playtested. Mana capacity/regeneration and item resource bonuses may need design or cost retuning after measurements. Rank-dependent stat coefficients can become extremely strong at high rank/gear and require DPS/TTK validation. The original Knight empowered bonus formula was directly user-approved earlier; its new rank-interaction variant is a separately flagged provisional extension, not an undisclosed rewrite of player approval.

### Mirror-resolution rules after Round 16

1. On original Mage cast with a qualifying primary image present, schedule a copied cast for **0.4s** later, with no extra copy Mana **as current provisional**.
2. If the originating image **dies before 0.4s**, cancel its pending copies. Any already spawned secondary image remains until its own death/half-duration expiration.
3. At mirror-resolution time for a **single-target offensive spell**, use original living valid target if available. If original target **died**, **select nearest valid enemy** under ordinary range, LoS, hostility and target rules; if none exists, default provisional fizzle with no new target (needs final approval). Do **not** redirect ground AoE to a random new location, nor retarget a self/ally-centered Metal Shield to hostile entities.
4. **Mirrored Metal Shield** uses a separate shared capacity, centered on **image** with provisional 8m radius. The Mage original shield uses its own independent shared capacity and 10m original cast radius. When both cover an ally, resolve **oldest ACTIVE eligible pool first**, based on actual application order, not spell ownership or pool size.
5. Knight empowered hit resolves **ordinary Physical basic packet** and distinct **Magical Void packet** through appropriate defense/resistance channels; preserve existing hit validity and basic-attack PDEF bypass semantics.

### Five questions for the next design round (OPEN)

1. **Hidden-teacher discovery design:** should each class teacher be hidden in a unique World 2 biome, tied to optional exploration puzzles, or discovered from special gear/lore clues? Do not invent exact towns/positions yet.
2. **Quest tests:** should Knight teacher test timed basic-attack precision/illusion coordination; should Mage teacher test mirror placement, AoE, and shield timing; or prefer alternative themes?
3. **Clone owner-death behavior for Mirage Mage:** Knight clone survives by Round16 confirmation; does Mage image also inherit parent survival on Mage death as in Tier II, and should pending mirrors finish if image itself remains alive?
4. **Knight posthumous end buff:** if Knight died and its clone finishes before Knight is revived, does the attack-speed buff simply expire unused, defer until revival, or not trigger while owner dead?
5. **Rank-scaling Mana/stat formulas:** keep the new PROVISIONAL high Mana regime and proposed rank-dependent AGI/INT terms as a testing baseline, or adjust their intensity (especially 600–700 Mana per high-level cast and multiple empowered basic hits). Are there gameplay resource targets to constrain the test?

**Scope remains documentation only.** Do not alter or merge Unity gameplay/assets/WIP or rewrite the approved rank gates; no live playtest or actual repository code edits performed.


## Round 17 — 2026-11-23 (LATEST USER-CONFIRMED discovery, advancement and owner-death rules)

User answered the five Round 17 interview questions, with **extra thoughts answering two unanswered multiple choices**. Interpret **"To finish the wolf cave solo"** as the chosen Psionic Knight teacher challenge and **"To finish the shaman cave solo"** as the chosen Mirage Mage teacher challenge, respectively. These are **explicit user-selected requirements**, not merely flavor suggestions.

| ID | Topic | USER-CONFIRMED | Unspecified / important compatibility |
| --- | --- | --- | --- |
| PSION3-071 | **Finding teachers from multiple clue types** | Separate hidden Tier III teachers should be discoverable through a **COMBINATION OF RUMORS, LORE and ENVIRONMENTAL CLUES** during exploration. No mandatory obvious quest marker is implied. | Specific clues, rumors, NPCs, prerequisites, teacher placement and World 2 region remain open. |
| PSION3-072 | **All hidden teachers enter Echo-wide Codex on discovery** | **WHEN THE PLAYER ACTUALLY FINDS ANY HIDDEN CLASS TEACHER, OF ANY CLASS, RECORD THE TEACHER IN THE CODEX AT ECHO-WIDE SCOPE**, not only Psionic Knight/Mirage Mage. The discovered-teacher entry persists when switching class manifestations and survives deletion of a manifestation under the normal Echo-wide durability rules. Hidden teachers that have not been encountered do NOT appear as spoiler placeholders. | Stable teacher IDs, Codex presentation/text/portrait/rumor hints, what exactly constitutes a "found" event, teacher relocation, and distinction between discovered vs advancement completed. Discovery itself is NOT an automatic advancement/class-manifestation creation. |
| PSION3-073 | **Knight teacher challenge = solo Wolf Cave completion** | To fulfill the **Psionic Knight hidden teacher's advancement challenge**, the player must **finish the WOLF CAVE SOLO**. A successful solo full clear is the actual proof, not simply visiting or killing one wolf. | Exact Wolf Cave dungeon identity/scene and whether it maps to the already authored **Wolf Survival: Moonlit Clearing** or a DIFFERENT cave must be verified before authoring; the existing Wolf Survival is an OUTDOOR moonlit clearing in documentation, so do NOT blindly equate their names. Trial timing (preexisting clear counts or must clear after teacher quest activation), difficulty tier, whether the clear is specific to the eligible Psion manifestation, and possible entrance access remain open. |
| PSION3-074 | **Mage teacher challenge = solo Shaman Cave completion** | To fulfill the **Mirage Mage hidden teacher's advancement challenge**, the player must **finish the SHAMAN CAVE SOLO**. An actual completed solo run is required. | No Shaman Cave content/documentation was found in the consulted existing docs/code search; do NOT claim the dungeon already exists or fabricate its map, monsters, boss, rewards or location. Exact authored dungeon ID, difficulty, participation eligibility and timing of clear versus meeting teacher remain open. |
| PSION3-075 | **Mage illusions survive Mage's death** | An already created **Mirage Mage primary Mental Image and its secondary noncopying images** **continue fighting normally for their remaining lifetime even after the Mage dies**. They do not auto-despawn or explode merely because owner died. Existing natural expiration and enemy destruction still trigger the full approved explosion behavior. | Pending 0.4s spell copies may finish IF the source image itself is still alive and previously authorized to copy the cast; do not invent new casts made by the dead Mage, and retain already approved "source image death cancels pending mirrors." Specific respawn/zone transition behavior remains open. |
| PSION3-076 | **Knight illusion end buff is lost if Knight dead** | If Knight's image ends by expiry or destruction **while Knight is dead**, its normal attack-speed benefit is **LOST**. Do NOT queue it for revival, grant it to a dead character, or extend it retroactively on revival. The image itself continues fighting for remaining lifetime after owner death, per Round 16. If Knight is alive when image ends, grant the already approved rank-scaling haste buff normally. | If Knight is revived *before* image finishes, treat Knight as alive at expiry and grant buff normally. Exact timing of death/respawn events must be deterministic. Multiple overlapping clone haste buffs still open. |

### Solo-clear semantics and Echo-wide knowledge (implementation guidance, not newly invented quest mechanics)

- "SOLO" means successful completion recorded under the existing dungeon Codex's **solo clears** criterion and appropriate dungeon encounter validation, with **one player** in the party/instance at run start and throughout the qualifying clear. Treat the class's owned **Mental Image/secondary images as summoned units**, not extra human party members, unless a later rule specifically prohibits them. Exact solo proof/event metadata and whether a prior Echo-wide clear satisfies teacher's challenge require a follow-up.
- Hidden-teacher Codex discovery is **Echo-wide** (similar to known hidden Way/advancement knowledge) while live quest progress and dungeon session state remain class-manifestation-specific unless existing authoritative global rules say otherwise. A teacher found by one class is KNOWN to the Echo afterward, but a particular advancement still requires the appropriate level/class, teacher/quest and solo-trial eligibility.
- **Existing Wolf Survival caution:** `docs/WOLF_SURVIVAL_DUNGEON.md` describes 10 waves in a moonlit clearing and an ordinary solo wipe/reset. The newly requested "Wolf Cave" may or may not mean this encounter. Avoid modifying that implemented dungeon's topology or auto-crediting it toward Knight advancement without explicit confirmation. Similarly do not pretend that Shaman Cave is an already-built dungeon.
- Do not put an undiscovered teacher into the Codex or reveal the hidden path via a visible undiscovered placeholder. Teacher discovery should be an idempotent Echo-wide event keyed by stable teacher ID, separate from solo-clear evidence and separate from class/Ways unlocked.

### Round 18: proposed next five focused questions (NOT APPROVED)

1. **Wolf Cave identity:** Is the Knight's solo trial the existing **Wolf Survival: Moonlit Clearing**, or do you want a separate actual cave dungeon?
2. **Shaman Cave design:** Should its encounter primarily test resistance to shamans' magical bursts/totems, environmental puzzles, or a multi-stage shaman boss?
3. **Quest-credit timing:** Does a **solo clear before discovering the teacher** count retroactively for advancement, or must the solo completion happen AFTER accepting the teacher challenge?
4. **Discovering a teacher:** Is physically reaching/talking to them necessary, or do clues revealing the teacher's location register a Codex discovery first?
5. **Trial execution:** May the **original Psion manifestation use its own Mental Image during solo trials** (recommended to count this as solo), or are there authored restrictions?

**Scope:** These are design approvals/documentation updates only, NOT authorization to implement Wolf Cave, Shaman Cave, teachers, Codex UI, quests, Unity scenes, assets or code. Preserve local WIP and the prior agreed numeric balance and skill gates.


## Round 18 — 2026-11-23 (LATEST USER-CONFIRMED: dungeon identities, teacher proximity, solo trial attribution)

The user resolved ALL FIVE Round 18 questions. This section is **authoritative over prior Round 17 text** where Round 17 called the two dungeons "Wolf Cave" and "Shaman Cave" generically, or deferred timing/class attribution. **Do not create a separate Wolf Cave**, and do not silently insert a new shaman dungeon instead of the Quillboar Shaman ritual cave from World 1's Quillboar design.

| ID | Confirmed design rule | Implementation interpretation / scope |
| --- | --- | --- |
| PSION3-077 | **Knight trial is existing WOLF SURVIVAL: MOONLIT CLEARING dungeon** | The user's "Wolf Cave" nickname **explicitly means the already established ten-wave Wolf Survival / Moonlit Clearing dungeon** (documented in `docs/WOLF_SURVIVAL_DUNGEON.md`). To earn Psionic Knight advancement proof, the eligible Psion must **complete the existing dungeon SOLO AFTER taking the Knight teacher's quest**. No newly authored underground wolf cave or substitute instance. Existing dungeon balance, waves, reward room and ordinary loot remain unchanged. |
| PSION3-078 | **Mage trial is the existing QUILLBOAR SHAMAN dungeon / ritual cave** | The user's "Shaman Cave" means the **Quillboar Shaman ritual cave/dungeon**, rather than an arbitrary shaman boss or brand-new unrelated dungeon. World 1 `docs/QUILLBOAR_REGION_DESIGN.md` establishes that entry is via a hidden cliff-cave in the Quillboar region near elemental settlements and that the character must complete the Trimery scholar's four-element Shaman hunt / turn-in to gain permanent dungeon access. **Respect that authored unlock system**; the Mirage Mage trial must not bypass the ordinary cave access requirement. The regional doc historically calls this dungeon "future/not yet implemented"; the user's latest description calls it already existing. Therefore verify the actual latest Unity scene/dungeon stable ID before wiring quest credit; **do not invent an ID or claim code implementation was verified from this doc-only update**. The required advancement evidence is a **SOLO completion of this Quillboar Shaman dungeon AFTER taking the Mage teacher's quest on the advancing Psion**. |
| PSION3-079 | **No retroactive dungeon credit** | **Past clears DO NOT COUNT**, even if the Echo's dungeon Codex shows a solo clear or the same Psion previously won. The player must complete **a new qualifying solo run after accepting the specific hidden teacher's advancement quest**. No retroactive completion upon finding teacher, discovering a Way or accepting quest. |
| PSION3-080 | **Teacher discovery proximity = within 2 meters** | Finding **ANY hidden teacher for ANY class** is automatically triggered by the player-controlled manifestation reaching **within 2m of the teacher**. At that moment, record a stable-ID, idempotent permanent **ECHO-WIDE Codex teacher discovery**, including when the class is not eligible to advance. **No dialogue, interaction, quest acceptance or class-specific eligibility is needed to record discovery**. Discovering the teacher does **not** itself accept their quest. No Codex placeholder before proximity discovery. The precise distance is ≤2.0m using the game's ordinary unit-to-unit distance convention; no extra LOS trigger requirement specified. |
| PSION3-081 | **Exact qualification: same advancing PSION + quest first + solo CLEAR** | The dungeon clear must be earned by the **actual Tier II Psion manifestation that is trying to advance**, not by another manifestation (even another class on the same Echo) or from the Echo-wide Codex solo-clear record alone. **Accept the correct hidden teacher's advancement quest BEFORE starting the qualifying solo dungeon run**. A successful solo full clear on this Psion after acceptance satisfies the corresponding quest. A run already in progress at acceptance does not qualify. Solo means one human player in the participating run; normal Psion Mental Image(s) and other owned combat summons **do not invalidate solo**. At completion, normal advancement eligibility checks (including Psion Tier II Lv60) remain necessary and the new Tier III Lv1 child manifestation is created following existing preserved-parent rules. |

### Required progression and data-flow separation

1. **Knowledge/discovery:** Finding the teacher at distance ≤2m emits one Echo-wide durable discovery record keyed by stable teacher ID. The record is shared by *all* manifestations and persists even if the discovering class save is archived or deleted. It is the Codex's TEACHER discovery state, not an automatic class unlock, quest acceptance or completion.
2. **Quest acceptance:** The correct Tier II Psion manifestation accepts the distinct Knight or Mage teacher's advancement quest. Quest acceptance must be recorded for that **specific manifestation's quest state**, including its acceptance order/time or reliable causal state for subsequent solo-run checking. No earlier run or Echo-wide Codex summary may be substituted.
3. **Dungeon entry:** Once the quest is active, this same Psion starts a **new solo dungeon instance**: Wolf Survival: Moonlit Clearing for Knight, Quillboar Shaman ritual dungeon for Mage. Preserve the Shaman cave's established optional scholar quest access gate when applicable. Solo participation should be evaluated under normal dungeon party/entry/clear evidence rules; owned Psion illusions are *not* other players.
4. **Dungeon completion:** Require the actual correct dungeon stable ID, successful completed solo outcome, this Psion manifestation's stable identity, and quest-active-before-run-start. The **teacher quest** receives credit; the Echo-wide Codex may independently record the ordinary solo clear, but Codex history **cannot waive the fresh quest-bound requirement**.
5. **Advancement:** Following teacher quest completion and ordinary eligibility rules, create the relevant Tier III manifestation (Psionic Knight or Mirage Mage) at level1, preserving the parent Psion. Do not automatically do the other teacher's quest or create both evolutions from one dungeon completion.

### Documentation and validation note

- The user's newest explicit map **"Wolf Cave" → "Wolf Survival: Moonlit Clearing"** resolves an earlier ambiguity. No separate Wolf Cave scene is needed. The preexisting 10-wave dungeon architecture and reward behavior are untouched.
- The user's newest explicit map **"Shaman Cave" → "Quillboar Shaman ritual cave/dungeon"** resolves that name ambiguity. The GitHub `docs/QUILLBOAR_REGION_DESIGN.md` describes the hidden physical cave entrance, permanent per-character access granted by turning in the scholar's four-element quest, but calls the dungeon future/unimplemented in its historical October design. **Actual present Unity implementation and dungeon stable ID have not been verified in this GitHub documentation round**; verify against the current Unity content when coding. The user's statement of its existence is respected as design intent without falsely claiming source verification.
- This is **pure design documentation**. No Codex gameplay system, teacher NPC, quest, Unity dungeon, data schema or prefab changes authorized here.

### Next round: five NEW details requiring design choices (OPEN)

1. When the same Echo has discovered a hidden teacher with one manifestation, should that teacher remain **unmarked in the world** for the next manifestation until approached, even though its Codex entry is already known, or should all manifestations receive an optional map note?
2. Should the solo dungeon **count only Normal mode**, or any difficulty that produces a valid solo clear (if multiple difficulties exist)?
3. What happens if the player **abandons/reaccepts** the advancement quest after having a successful run: must clear again (new quest instance) or may reclaim unturned-in proof?
4. May a Psion **find both teachers and hold both advancement quests concurrently**, then choose which Tier III fork to create first, or restrict to one active specialization quest at a time?
5. Should the teacher offer the quest at **Tier II Psion Lv60** only, or let lower-level Psion accept it early while delaying final advancement until eligible? Neither changes the confirmed class-tier progression threshold.

**All Round 17 and earlier combat, weapon, Codex, rank-gate, shield, illusion and Mana-design decisions persist unless explicitly superseded above.**


## Round 19 — 2026-11-23 (LATEST USER CONFIRMED: flexible teacher placement, dialogue, quest flow; Fourfold Hollow implementation correction)

**Repository verification correcting prior mistaken assumptions:** On the current `feat/verdant-wilds` branch, `Assets/_DiceFree/Scenes/FourfoldHollow.unity` EXISTS. `docs/AGENT_HANDOVER.md` explicitly includes **Fourfold Hollow** in the saved World 1 content and states that a separate DungeonCoOp QA build exercised Fourfold Hollow in two local processes. Historical `docs/QUILLBOAR_REGION_DESIGN.md` language calling the Shaman ritual dungeon “future/not yet authored” is a 2026-10-26 planning snapshot, **NOT current implementation status**. The user explicitly corrected the assistant's claim that this dungeon was not implemented. **Treat the existing Fourfold Hollow / Quillboar Shaman dungeon as the current Mirage Mage solo-trial target.** The exact serialized Unity dungeon stable ID and the build currently running on the user's machine have not been inspected; that does not justify calling the scene unimplemented. Do NOT duplicate or rebuild its scene merely to satisfy a proposed advancement trial. Existing scholar/dungeon-access design gates should be reconciled with actual gameplay state when wiring advancement, not silently replaced.

### Five latest USER-APPROVED decisions

| ID | Topic | Confirmed behavior | Guardrails / implementation notes |
| --- | --- | --- | --- |
| PSION3-082 | **Hidden teachers may temporarily be ANYWHERE in World 1 or World 2** | **Neither Psionic Knight nor Mirage Mage teacher is fixed to World 2.** Their provisional placement may be anywhere in World 1 or World 2. The user explicitly **delegates teacher placement and subsequent relocation to Codex (development agent)** while the worlds are being built. | Avoid hard-coded permanent lore/geography/location dependencies. Stable TEACHER identity and Echo-wide Codex discovery survive physical NPC relocation. Real map content requires later implementation authorization; this round is design/handover only. |
| PSION3-083 | **Quest acceptance requires the CORRECT Tier II class AND Psion level 60+** | The teacher only OFFERS / ACCEPTS the advancement quest if talking manifestation is the correct class (**Tier II Psion** for these two) AND **level ≥60**. If class is correct but character **below level60**, respond with appropriate dialogue **like “Come back when you're stronger”** without offering the quest. If **wrong class**, respond with something **like “I have nothing to teach your kin”**, also without quest. These sample lines express tone, not locked exact localization. | Teacher can still be discovered by ANY class via 2m proximity; discovery is independent of these dialogue eligibility checks. Tier II Psion Lv60 is established eligibility to begin specialization; no arbitrary tier-III bypass. |
| PSION3-084 | **Teacher tracking in CODEX ONLY; NO auto map markers** | Once a teacher is found by 2m proximity, the **Echo-wide Codex** alone exposes the discovery for now. Do NOT automatically create World 1/2 map pins, location icons, cross-manifestation navigation markers, quest-direction hints on maps, or persistent teacher coordinates from this discovery. | Normal world-map terrain exploration stays manifestation-specific. Existing ordinary quest UI may behave as usual, but discovering a teacher must not become a permanent world-map marker. Placement may be changed later. |
| PSION3-085 | **RETURN TO THE SAME HIDDEN TEACHER after solo dungeon clear** | Completing the quest-bound solo Wolf Survival clear for Knight or the quest-bound solo Quillboar Shaman/Fourfold Hollow clear for Mage **marks that particular advancement quest's trial fulfilled**, **but does NOT automatically transform or create Tier III**. The Psion must **return to that teacher and complete/turn in the quest there**, after which normal advancement produces the relevant saved new Tier III Lv1 manifestation. | Use the same advancing Psion manifestation at solo run, quest and turn-in. Prior clears before acceptance never count. No auto-advance on boss kill/dungeon victory screen. |
| PSION3-086 | **BOTH specialization quests may be active SIMULTANEOUSLY** | A single eligible Tier II Psion manifestation may **accept and track BOTH** the Psionic Knight teacher quest and Mirage Mage teacher quest at the same time, and may complete their respective fresh solo dungeon proofs separately. **Do not impose mutually exclusive quest acceptance or automatically cancel the other** merely because one is active. | Each quest must have separate acceptance/completion state and require its own correctly attributed solo dungeon proof. When the player turns in one and branches into that Tier III, the parent Psion manifestation must remain preserved under the existing Echo manifestation rules so the OTHER quest/branch remains pursuable later. Exact quest/snapshot behavior for simultaneous turn-ins should follow existing preservation semantics, not retroactively award the other evolution. |

### Full approved teacher-advancement flow (Round 19 effective)

1. Discover either or both hidden teachers by physically entering **≤2m**. This automatically adds each to the **Echo-wide Codex** once (even if discovering manifestation is the wrong class or level). No map pin.
2. Speak to a teacher. Wrong class: appropriate rejection dialogue. Correct class below level60: “come back when stronger” style message. **Tier II Psion level60 or higher**: may accept that teacher's advancement quest. Both distinct quests may be accepted concurrently.
3. **After** accepting each quest, start and complete its specific dungeon SOLO using the **same eligible Tier II Psion manifestation**. Knight: existing **Wolf Survival: Moonlit Clearing** (10 waves). Mage: existing **Quillboar Shaman / Fourfold Hollow**. Keep separate historical dungeon-access quest requirements where applicable. No retrospective clear credit from Codex or other class.
4. Mark quest proof complete for each qualifying full solo clear; keep the quest state awaiting **teacher turn-in**. The dungeon reward room does not perform the Tier III fork.
5. **Return to the relevant hidden teacher** and complete that specific quest to create **Psionic Knight** or **Mirage Mage** as a new level-1 Tier III manifestation, preserving Tier II Psion and Echo-wide discoveries as normal. A concurrently active other specialization quest is **not invalidated automatically**.
6. Teacher placement remains flexible within World 1 or World 2, and Codex may reposition either hidden NPC later without changing stable teacher IDs or Echo-wide discovery history.

### Important historical supersession

- **Round17** "Wolf Cave"/"Shaman Cave" terminology was resolved in Round18; **Round18** uncertainty about existence of Shaman cave is **now superseded by verified Fourfold Hollow scene and QA handover**. The Quillboar **region design** is not a reliable current implementation checklist.
- **Quest acceptance** is **not** available below Psion Lv60; discovery remains possible regardless of level and class.
- **Quest completion** now explicitly requires **teacher turn-in**, not automatically creating the Tier III manifestation at dungeon completion.
- **Both quests coexist**; independent proofs; only chosen turned-in quest advances at that moment.
- No new battle, Mana, skills, art, item, quest or Unity implementation was authorized. Prior Round 16 provisional numerics, exact approved rank gates and equipment/illusion mechanics stay intact.

### Remaining later design questions (not approved)

Exact teacher placements/visuals/personas/dialogue, whether normal/harder dungeon difficulties count, details for abandoning/accepting a quest anew, local/world quest guidance that does not spoil Codex-only discovery, and implementation behavior of the second active quest when forking an Echo to the first Tier III manifestation. Continue with design-only updates until the user requests coding.


## Round 20 — 2026-11-24 (LATEST: Tier III base attributes and per-level growth; NUMBERS PROPOSED)

### User-confirmed requirements

User specifically requests that **Psionic Knight and Mirage Mage have HIGHER starting stats than parent Tier II Psion and HIGHER overall per-level attribute growth**, despite beginning a fresh Tier III class manifestation at level 1, with a sharp specialization:
- Knight: **large AGILITY growth, some INTELLIGENCE**, enough Vitality to survive near-melee 3m combat.
- Mage: **large INTELLIGENCE growth, some AGILITY**, caster survival.
- **BOTH: VERY LOW Strength and Spirit growth**. Do NOT assign full Psion-type STR/SPR per-level gains to these specialized Tier III classes. Their per-class total growth is greater than Psion, **not every individual attribute's growth**, since the latter conflicts with intentionally reduced STR/SPR.
- These are each class's OWN base attributes and growth for the new Tier III Lv1 manifestation. They do NOT add the parent Psion's level 60+ accumulated attributes on top of the new class stats; no permanent bonus or excess XP carries across advancement. Parent remains saved.

### Verified current Psion baseline (source, not a proposal)

On `feat/verdant-wilds`, the actual asset `Assets/_DiceFree/Settings/Psion/Psion class.asset` includes:
- Level-1 `baseAttributes`: **VIT12 / STR7 / AGI14 / INT18 / SPR10** = **61**.
- Per-level `growth`: **VIT1.55 / STR0.65 / AGI2.10 / INT2.95 / SPR1.10** = **8.35 total attributes per class level**.
- `baseHp=10`, `hpPerVitality=15`, `regenerationPerVitality=0.1`, `moveSpeed=5`, baseline Physical Defense0/Magical Defense0. These additional settings are **NOT** implicitly approved for Tier III inheritance.
- Psion's separate Mana profile `Assets/_DiceFree/Settings/Psion/Psion Mana.asset` currently uses `baseMaximum=84`, `+3 maximum Mana per INT`, `+1 per SPR`, `baseRegenPerSecond=4.5`, `+0.075 regen per INT` and `+0.075 per SPR`. This is a REFERENCE ONLY; Tier III Mana profiles/capacity/regeneration must be reviewed against their rank20 Mana costs before finalizing.

### Proposed level-1 class attributes (DESIGNER PROPOSAL, NOT yet user-approved NUMBERS)

| Attribute | Actual Tier II Psion | Knight PROPOSED | Mage PROPOSED |
| --- | ---: | ---: | ---: |
| Vitality | 12 | **22** | **19** |
| Strength | 7 | **8** | **8** |
| Agility | 14 | **30** | **22** |
| Intelligence | 18 | **22** | **32** |
| Spirit | 10 | **11** | **11** |
| **Total starting attributes** | **61** | **93** | **92** |

**All five starting values are strictly higher than Psion for EACH Tier III class.** The +1 Strength/Spirit base values intentionally remain barely higher, reserving budget for AGI/INT/VIT.

### Proposed per-level growth (DESIGNER PROPOSAL, NOT yet user-approved NUMBERS)

| Attribute | Actual Tier II Psion | Knight PROPOSED | Mage PROPOSED |
| --- | ---: | ---: | ---: |
| Vitality | +1.55 | **+2.30** | **+2.00** |
| Strength | +0.65 | **+0.20** | **+0.20** |
| Agility | +2.10 | **+5.00** | **+1.70** |
| Intelligence | +2.95 | **+1.70** | **+5.00** |
| Spirit | +1.10 | **+0.20** | **+0.20** |
| **Total per class level** | **+8.35** | **+9.40** | **+9.10** |

- Each class grows MOST in its signature attribute, but still gains a smaller part of the other hybrid attribute. Knight's slightly greater VIT growth reflects 3m basic-attack engagement and self-only shield, versus Mage's 9m basic reach and stronger group Metal Shield. Both get very low STR/SPR growth.
- Growth applies per **LEVEL UP**, so the expected base attribute at level `L` is `base + (L - 1) × growth`, before equipment/temporary effects, with eventual engine stat precision/rounding applied under existing systems.
- At Tier III level **60** before equipment: Knight **VIT157.7 STR19.8 AGI325 INT122.3 SPR22.8**; Mage **VIT137 STR19.8 AGI122.3 INT327 SPR22.8** (examples should be recalculated by actual growth arithmetic on use). These are examples, not caps.
- **Verified level150** with this proposal: Knight VIT364.7 STR37.8 AGI775 INT275.3 SPR40.8; Mage VIT317 STR37.8 AGI275.3 INT777 SPR40.8 (examples should be recalculated by actual growth arithmetic on use). Numerical gameplay economy must be playtested.
- Tier III base basic-attack weighting stays **Knight 0.6AGI+0.4INT**, **Mage 0.6INT+0.4AGI** (previously user confirmed). These proposed larger attributes do NOT change the already approved relative coefficients, crit mechanics, rank gates, aura +movement or Mage spell formulas. All auto attacks remain Physical; Knight bonus Void packet remains separate Magical.
- **Mana viability warning:** Knight's proposed +1.70 INT/level and extremely low +0.20 SPR/level can make sustaining the earlier provisional 20-Mana/rank costs challenging without adequate Tier III base Mana/per-INT/regen. Mage's high INT makes its resource trajectory different. Therefore **design each Tier III Mana resource profile separately** and run cost/regen/resource feasibility tests; do not silently reuse Psion's Mana profile or assert balance from raw stats alone.

### Proposed next explicit approval

1. Does the user approve **both proposed base stat rows and per-level growth values** as the initial playtest target, or want e.g. stronger Knight VIT, more Mage INT, or even less STR/SPR?
2. If approved, establish **Tier III Mana maximum/regeneration formula** per class that supports both sustained spellcasting and high-rank Mana costs (provisional Round16 ~+20 per active-skill rank).
3. Retain Round 19 remaining quest and teacher edge-case questions for subsequent design, not a new class-skill-point allocation.

**Status boundary:** USER APPROVED the specialization DIRECTION; numeric base/growth values in this Round20 are a **DESIGNER PROPOSAL AWAITING USER APPROVAL**, not implementation authorization. Do not modify the Psion baseline actor, any Unity class assets, save state or live character stats in this documentation round.


## Round 21 — 2026-11-24 (LATEST: USER-SELECTED STAT GROWTH; CODEx implementation blockers evaluated)

The user specifically requested:
1. "**Increase agi for mage by 1 and int for knight by 1**" in the context of the immediately preceding attribute **growth per level** table: interpret as **Mage AGI growth 1.70→2.70** and **Knight INT growth 1.70→2.70**, **NOT starting AGI/INT +1**.
2. "**Increase Str and spi to 1 per level for both**": **STR+1.00 and SPR+1.00 per class level for BOTH**, superseding earlier proposed +0.20.
3. "**Increase vit to 2.5 for knight**": Knight VIT growth **2.30→2.50**. Mage VIT remains +2.00.
4. The user quoted current Codex implementation planning observations requesting explicit decisions for class Mana/HP and for Knight empowerment miss/no-Mana/crit/image/haste/mirror snapshot behavior and conflicting old-vs-new damage formulas. Provide an actionable **PROVISIONAL default handoff** rather than inventing final user approval where not given.

### Exact user-selected growth changes (CONFIRMED)

| Primary attribute | Existing Tier II Psion asset reference | Knight latest selected growth | Mage latest selected growth |
| --- | ---: | ---: | ---: |
| VIT per level | 1.55 | **2.50** | **2.00** |
| STR per level | 0.65 | **1.00** | **1.00** |
| AGI per level | 2.10 | **5.00** | **2.70** |
| INT per level | 2.95 | **2.70** | **5.00** |
| SPR per level | 1.10 | **1.00** | **1.00** |
| **TOTAL per class level** | **8.35** | **12.20** | **11.70** |

Use these as the **latest USER-CORRECTED growth values**. Note "very low STR/SPR" from Round20 was relative to primary AGI/INT, and the user has now expressly selected +1.00/rank each; do not revert to +0.20 because an older line says "very low". Knight STR growth exceeds parent +0.65; Knight SPR slightly below parent +1.10; no "every individual growth must be higher" rule exists. Total growth for both is higher than Psion.

### Working level-1 base stats (prior proposal RETAINED, NOT separately confirmed numerically this round)

| Base stat | Tier II Psion actual | Knight existing working proposal | Mage existing working proposal |
| --- | ---: | ---: | ---: |
| VIT | 12 | 22 | 19 |
| STR | 7 | 8 | 8 |
| AGI | 14 | 30 | 22 |
| INT | 18 | 22 | 32 |
| SPR | 10 | 11 | 11 |
| Total | 61 | 93 | 92 |

**No explicit starting-attribute correction was requested**: do not add 1 to Knight's starting INT or Mage's starting AGI. These base numbers are the Round20 candidates carried forward, pending acceptance as first playable implementation baselines.

### Verified growth projections before equipment, formula `base + (classLevel−1)×perLevelGrowth`

| Class | Tier III Lv1 | Lv60 | Lv120 | Lv150 |
| --- | --- | --- | --- | --- |
| Knight VIT/STR/AGI/INT/SPR | 22/8/30/22/11 | 169.5/67/325/181.3/70 | 319.5/127/625/343.3/130 | **394.5/157/775/424.3/160** |
| Mage VIT/STR/AGI/INT/SPR | 19/8/22/32/11 | 137/67/181.3/327/70 | 257/127/343.3/627/130 | **317/157/424.3/777/160** |

Do **not** mutate parent Psion's current asset, or add Psion's Lv60 attributes to the freshly created Tier III Lv1 manifestation. All previously approved normal physical basic attack scaling remains Knight `0.6AGI+0.4INT`, Mage `0.6INT+0.4AGI`; future Tier IV paths remain Knight `0.9AGI+0.2INT` and Mage `0.9INT+0.2AGI`. Rank gates and ability caps remain unchanged.

### Designer-selected PROVISIONAL Tier III HP and Mana resource profiles (NOT user-approved numerical balances)

The actual Tier II Psion has **baseHp10**, `hpPerVitality15`, `regenerationPerVitality0.1`, `moveSpeed5`; its Mana profile is `baseMaximum84 + 3×INT+1×SPR` and `regenPerSecond4.5+0.075×INT+0.075×SPR`. Proposed Tier III profiles must be tested together with HIGH Round16 spell costs of **516/598 Knight** and **520/680/690/658 Mage** at skill cap. For consistency with existing `ClassResourceProfile`, use:

| Class | Provisional base HP | HP / VIT | HP regen / VIT | Provisional maximum Mana | Provisional Mana regen per second |
| --- | ---: | ---: | ---: | --- | --- |
| Knight | **30** | 15 | 0.1 | **`140 + 4.0×INT + 1.5×SPR`** | **`3.5 + 0.045×INT + 0.035×SPR`** |
| Mage | **20** | 15 | 0.1 | **`160 + 5.5×INT + 2.0×SPR`** | **`4.0 + 0.070×INT + 0.055×SPR`** |

Unmodified base movement speed remains **5** for both, by ordinary same-baseline rule (user-approved Speed Presence is the source of class-based speed improvements). Both base PDEF/MDEF **0 initially** as conservative reuse of parent defense convention; armor, VIT and self/group shields carry defense instead of inventing special class-only defense coefficients. HP assumes standard `baseHp + 15×VIT` in an ordinary actor without other overrides. Preserve engine rounding and regeneration intervals (no forced integer truncation in this design note).

| Class | Lv1 Mana / regen/s | Lv60 Mana / regen/s | Lv120 Mana / regen/s | Lv150 Mana / regen/s |
| --- | --- | --- | --- | --- |
| Knight | **244.5 / 4.875** | **970.2 / 14.1085** | **1708.2 / 23.4985** | **2077.2 / 28.1935** |
| Mage | **358 / 6.845** | **2098.5 / 30.74** | **3868.5 / 55.04** | **4753.5 / 67.19** |

**Sanity check:** With level150 no-gear stats, Knight can nominally spend `516 (Image)+598 (Void shield)+12×72 (12 ranked empowerment hits)=1,978 Mana`, within its proposed 2,077 Mana pool **if all other spending is zero**, leaving ≈99 Mana. Mage can spend `690+520+680+658=2,548 Mana` for one of each max-rank active spell, within 4,753 Mana. This demonstrates **one viable finite test sequence**, not sustainable sustained-combat balance. In actual play, note damage, movement windows, cooldowns, regen, consumables and stat gear; test level1, level60, level120 and level150. Future resource gear can change those budgets; if gameplay is frustrating, tune resource coefficients and cooldowns rather than reducing the user's explicit steep Mana cost progression without consultation.

### PROVISIONAL Knight empowered-hit edge-case implementation defaults for Codex review (NOT yet user-confirmed)

All below are **recommended test behavior** that preserves user's confirmed finite `1+floor(rank/3)` empowerment, per-hit `4+2r` Mana cost and dual Physical + separate MAGICAL Void packet. User did **not** select these additional edge-case behaviors in this message.

- **Hit vs miss:** charge and `4+2r` Mana are consumed on a **successfully landed eligible basic attack event**. A miss, evasion or invalidated attack (no landed hit) consumes **neither**. A valid hit whose damage is fully absorbed by a shield **DOES** consume both (contact occurred).
- **Insufficient Mana:** execute the normal Physical basic attack if it is otherwise legal, with **NO Magical Void bonus**, **NO empowerment charge spent and NO partial Mana spending**. Empowerment remains until a later valid empowered hit or its already proposed **12s expiry**, preventing negative-Mana/free-Void exploit. UI should indicate insufficient Mana; no endless autoattack cancel loop.
- **Critical rule:** The normal Physical basic attack may use Knight's authored mastery **30%-at-rank30 crit chance / 160→190% multiplier**, while its **additional MAGICAL Void packet does NOT crit** by default. This honors opt-in crit semantics and avoids transferring flat physical armor bypass or crit multiplier to an unapproved separate Magical damage category. Crit permissions can be widened later by explicit decision.
- **Illusions & empowerment:** Knight image receives **full compatible passive bonuses** and **10→50%-strength copies of eligible TEMPORARY buffs** as already user confirmed, but must **NOT draw down the owner's empowerment charges or owner's Mana**. Designer proposal: on image creation, snapshot the owner's ACTIVE Empowered Attacks buff as a separate, local set of the **remaining charges**, giving the image its own capped charge counter and a COPY of the separate Magical Void bonus at **one Image-rank scaling percentage**; spend the image's LOCAL charge on its landed basic hits, 0 Mana to owner/image, expire the copied charges with the illusion or original remaining buff duration (whichever ends earlier). This preserves meaningful copied temporary buff benefit without infinite extra empower hits, no duplicate aura emitters and no double-applying the image's base basic-damage multiplier to the copied buff potency. A Knight image created when empowerment is inactive receives NO charges. Need test overpowered free copies; tweak while preserving the already approved 10→50% temp-buff strength lineage.
- **Recast/death:** Recasting Knight Empowered Attacks after cooldown **replaces**, does not stack, remaining owner charges and resets proposed 12s expiry. Unused charges expire. Death cancels the Knight's own empowerment buff; pre-existing independent images persist to their normal end per user approval. At clone end while Knight dead, discard the haste bonus (already user approved).

### Explicit resolution of the OLD versus NEW Knight Magical Void damage formulas

- OLD directly approved raw damage in earlier round: **`30 + 17r + 0.4AGI + 0.1INT`**. This keeps its stat coefficients constant as ranks increase.
- The user subsequently explicitly instructed that **damage coefficients should ALSO scale with skill rank**, described as `flat + flat×rank + (AGI/INT×coefficient×rank)`. This more recent instruction requires a rank-dependent stat multiplier for damage skills, including the Knight's empowered additional packet if applying that general rule.
- **SELECT the NEW Round16 rank-scaling implementation CANDIDATE for the FIRST PROVISIONAL PLAYTEST**, pending user numeric sign-off: **`RAW_Magical_Void(r) = 30 + 17r + [0.4+0.016(r−1)]AGI + [0.1+0.004(r−1)]INT`**. At rank1 this equals the older original formula; at rank34 the coefficients become 0.928 AGI and 0.232 INT, preserving the previously approved 4:1 AGI:INT relative ratio. Knight normal Physical basic attack remains `0.6AGI+0.4INT` separately. PDEF bypass from Blades Mastery affects only the Physical hit; Mage-like Magical Defense/Void affinity processing affects only Magical packet. No implicit crit on packet.
- **Interpretation and approval boundary:** user's newest general scaling direction, coupled with Codex's call to resolve the inconsistency, supports USING this newer PROVISIONAL candidate instead of silently choosing old fixed coefficients. The exact **+0.016 / +0.004 per-rank coefficients were chosen by the designer**, not explicitly user-approved. Do not label them final balance without playtesting and numerical approval. Older exact formula remains the rank1 anchor and historical baseline, NOT a competing live implementation target.

### PROVISIONAL illusion-end haste and Mage mirror edge-case defaults for Codex review (NOT user confirmed)

- **Knight same-name Mental Image end haste:** bonuses from multiple images **DO NOT ADD**. Keep the **strongest active** end-haste amount; when a stronger new one arrives, replace the prior effect; equal amount **refreshes duration** from that trigger, weaker doesn't reduce existing magnitude or duration. Independent unrelated effects remain subject to general stacking rules. This is the end haste buff, **distinct** from the already user-confirmed strongest-only SAME-NAME *speed aura*.
- **Rank progression endpoints** are user confirmed: Knight Image r1 +1% speed for1s, r25 +50% for5s; approximate designer-linear interpolation over 24 intervening ranks, rounding only for display. Trigger on expiry OR enemy destruction. Knight must be alive at the event or buff is **LOST** (not queued after resurrection).
- **Mirage Mage primary spell-copy snapshot:** at the ORIGINAL Mage cast, record the original target/ground location, spell rank, class stats and original RAW damage/shield capacity, and relevant unique spell identity. When image's scheduled **0.4s delay** expires, confirm source image STILL alive. At resolution, use snapshot raw values multiplied **once** by the primary image's current Mental Image rank's approved curve (provisionally 5% at r1 →25% at r25). If original target died during delay, retarget nearest eligible enemy within ordinary copy targeting rules; if none valid, that particular targeted copy fizzles. Ground AoE uses its original ground position; shields center on image and create an independent shared pool; don't retarget shield to hostiles. No extra Mana and NO mirrored CC/stun (user approved behavior). A copy's independent shield, once applied, remains until its own 8s duration or exhaustion, even if source image dies thereafter. Original/copies use oldest-active shared pool first (user approved).
- **Mage mirrored Mental Image** creates **one non-copying secondary** with half original image HP / ordinary basic damage / configured lifetime, but SAME full original explosion damage (USER APPROVED). Snapshot that original approved explosion formula at secondary creation so later parent death doesn't cancel or alter damage; the secondary lasts independently and does not recursively echo any spells. Both primary and secondary survive Mage owner death until natural expiry/enemy destruction under user confirmed rules.

### Summary of implementation handoff status (do not silently auto-implement from this conversation)

**USER-CONFIRMED:** Growth changes (Knight VIT2.5/STR1/AGI5/INT2.7/SPR1; Mage VIT2/STR1/AGI2.7/INT5/SPR1), previously confirmed damage channels, clone end-death rules, class rank gates, quest and equipment identity.

**DESIGNER-PROVISIONAL pending test and approval:** Retained level1 base stats, base HP, Mana max/regen formula, missed-hit/insufficient Mana/crit/copy-charge rules, strongest-only clone-end haste, mirror snapshot/expiration precision, NEW rank-dependent empowered bonus raw coefficients. Do not claim these choices are user-approved or implemented. If Codex is separately authorized to implement, use these as a coherent first-test baseline and surface material balance disagreements after tests. No Unity assets, branch merges, or local WIP modifications made during this Round21 design update.


## Round 22 — 2026-11-24 (LATEST: USER APPROVES ALL ROUND 21 PROPOSALS FOR THE FIRST PLAYTEST)

**Explicit user decision:** "I agree on everything with you! Did you push these last decisions?" This approves the assistant's complete prior Round 21 recommendations, **including proposed level-1 attributes, HP and resource formulas, selected rank-dependent damage implementation formula, Knight empowerment resolution mechanics, illusion-end stacking, and Mirage Mage spell-copy snapshot/expiration rules**. All numbers and defaults in Round 21 transition from "DESIGNER-PROVISIONAL PENDING USER APPROVAL" to **"USER-APPROVED INITIAL PLAYTEST VALUES"**. Gameplay balance may naturally require future changes after testing; no actual Unity implementation or playtest has occurred during these documentation commits. **Do not keep asking for approval of the same Round 21 numbers, and do not follow older document comments marking these exact values pending or unapproved.**

### Locked initial implementation/test stat identities

| Stat | Psionic Knight Tier III Lv1 | Knight growth/level | Mirage Mage Tier III Lv1 | Mage growth/level |
| --- | ---: | ---: | ---: | ---: |
| Vitality | **22** | **+2.50** | **19** | **+2.00** |
| Strength | **8** | **+1.00** | **8** | **+1.00** |
| Agility | **30** | **+5.00** | **22** | **+2.70** |
| Intelligence | **22** | **+2.70** | **32** | **+5.00** |
| Spirit | **11** | **+1.00** | **11** | **+1.00** |
| **Attribute total** | **93** | **+12.20** | **92** | **+11.70** |

These start higher and grow faster **in total** than the actual Tier II Psion asset (Lv1 sum61, growth sum8.35). Each new Tier III manifestation starts at its OWN Lv1 stat budget, not inherited additive level60 parent stats. Preserve separately approved normal basic scaling (Knight 0.6 AGI+0.4 INT, Mage 0.6 INT+0.4 AGI); optional Tier IV specializations' directional coefficients remain unchanged.

### User-approved initial HP, Mana and base behavior

- **Knight:** `baseHp=30`, `hpPerVitality=15`, `regenerationPerVitality=0.1`, `maximumMana=140+4INT+1.5SPR`, `manaRegenPerSecond=3.5+0.045INT+0.035SPR`.
- **Mage:** `baseHp=20`, `hpPerVitality=15`, `regenerationPerVitality=0.1`, `maximumMana=160+5.5INT+2SPR`, `manaRegenPerSecond=4+0.070INT+0.055SPR`.
- Standard unmodified base movement speed **5** for both; base Physical Defense and Magical Defense **0** each as initial values, subject to ordinary equipment/secondary stat systems. Speed Presence passive/aura modifies speed per the already-approved rank table.
- At Tier III Lv150 without gear using these rates: Knight approx **2077.2 max Mana, 28.1935 regen/s**; Mage approx **4753.5 max Mana, 67.19 regen/s**. Validate real engine rounding and level/rank combinations. These are chosen test baselines, **not measurements from a completed test**.

### User-approved Knight Empowered Attacks detailed implementation behavior

1. **Cast/charges:** finite next-basic-hit enhancement, **`1+floor(empowermentRank/3)`** charges; Round16 **`4+2r` Mana PER empowered landed strike** as current initial test cost; 0 activation cost, cooldown14s, provisional (now user-approved test) charge expiry12s. Activating/recasting after cooldown **replaces** unused charges; never stacks a second charge pool.
2. **Landed vs missed:** An attempted basic attack that **misses, is evaded or invalidated consumes NO Mana and NO empowerment charge**. A real landed hit, **even fully absorbed by a shield**, consumes one charge and its Mana. Ordinary attack legality still applies.
3. **Insufficient Mana:** On an eligible otherwise valid basic hit without enough Mana for the empowerment tax, resolve **ordinary Physical basic damage ONLY**, with NO bonus Magical Void, NO Mana deduction and NO charge use. Charge persists until a later valid funded attack or expiry. No free bonus, negative Mana or autoattack stall loop.
4. **Critical permission:** The ordinary Physical basic attack may crit under the Knight's approved +1 percentage point crit chance/rank and `160%+1pp/rank` multiplier. The **separate Magical Void empowerment packet DOES NOT CRIT** in the initial implementation, even if Physical part crits. Flat Physical Defense bypass is for Physical basic component only.
5. **Owner versus image charges:** Knight mobile Mental Image inherits eligible passives FULLY, and temporary buffs at image-rank 10–50% effect strength. If summoned while owner empowerment is active, the clone receives **its OWN LOCAL COPY of remaining charges** and scaled Magical Void bonus, bounded by owner's remaining original buff duration and image lifetime. Clone landed attacks consume only its own local charges; they NEVER spend owner Mana or owner charge count. No duplicated aura emitters or double reduction of base damage. No copied empowerment if owner inactive at summon. Track local expiration/creation to prevent infinite echo charges. On Knight death owner empowerment ends but existing image persists, with local finite temporary effects respecting own timers.
6. **Exact selected SINGLE extra-damage packet for first implementation:** `RAW_Magical_Void(r)=30+17r+[0.4+0.016(r−1)]AGI+[0.1+0.004(r−1)]INT`; rank1 equals earlier fixed stat formula, rank34 has `608+0.928AGI+0.232INT`; **4:1 AGI:INT stat contribution maintained**. This **SUPERSEDES** the earlier fixed `30+17r+0.4AGI+0.1INT` for ranks above1 as the IMPLEMENTATION formula; **do not calculate both packets**. Ordinary Knight basic hit separately resolves Physical per `0.6AGI+0.4INT`, its PDEF/Mastery and crit rules.

### User-approved illusion-end haste and Mirage Mage echo specificity

- **Knight image haste on exit:** end-buff triggers on natural expiry or destruction only if owner Knight **alive at trigger**; otherwise LOST, not stored for revival. Rank1 +1% attack speed for1s, rank25 +50% for5s; initial approved **linear interpolation** across ranks 1..25. Multiple same-named image-end haste buffs **DO NOT ADD**: strongest active amount wins, stronger replaces, equal refreshes duration, weaker does not override. This is distinct from stronger-only SPEED PRESENCE AURA.
- **Mage mirroring:** snapshot original Mage spell rank, relevant stats, raw damage/shield capacity, target and ground cast location at **original cast**. At each primary image's **0.4s echo resolution**, verify the source image is still alive or cancel; **if original targeted enemy died, retarget nearest valid enemy**, else use surviving valid original target; no valid target => that copy fizzles. Ground AoEs use original ground location, independent copied group shields center on source image. Apply once the approved initial **`mirrorPct(imageRank)=5%+20%×(imageRank−1)/24`** to copied offensive raw damage and copied shared Metal shield pool, reaching **25% at rank25**. Copied shields protect eligible allies in provisional now-approved **8m image-centered radius**, remain after primary image dies once successfully applied and last their own duration/exhaustion; chronological oldest eligible shared group pool absorbs first. Mage echoes require **NO additional Mana** and **do not copy stun/CC**.
- **Mage secondary image:** bounded one generation, no mirroring, half primary HP/basic damage/lifetime, same FULL original Mental Image magical Void explosion; snapshots approved full-strength explosion at creation; independent of primary/owner subsequent death, ends on own expiry or enemy destruction. Primary/secondary survive Mage owner death normally, consistent with earlier user decisions.

### Approved for Codex implementation handoff, not a claim of delivery

All the above are fully approved **game design decisions and first-playtest numbers**. Other unspecified UI/asset/state integration details should follow existing project systems, but never invent incompatible stacking or charge mechanics. The existing **Round16 high Mana cost and rank-scaling spell/shield formulas and cooldowns remain current initial test values**, as previously delegated and now encompassed by user's agreement. Any operational adoption by Codex/Unity requires separate implementation, source changes, tests and QA; **this GitHub commit updates design documentation only**. Prior sections in this document retained as historical round audit are superseded when their approval statuses conflict with this latest Round22 decision.

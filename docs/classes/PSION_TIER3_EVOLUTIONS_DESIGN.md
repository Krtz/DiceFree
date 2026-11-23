# DiceFree — Psion Tier III evolutions — design interview

**Status:** Tier III **Rounds 1–11 confirmed through 2026-11-23**; one class skill point per level through 150 and complete five-ability rosters are approved. **Round 11 designer cap/rank-gate tables are proposed for user approval, not yet confirmed**. Latest confirmations supersede earlier proposals, without changing Tier II Psion. These classes are **DESIGN ONLY**, **not implemented in Unity**. Identity and which parent abilities are retained are now partly approved; precise ranges, spell upgrade coefficients, new skill mechanics, rank gates and advancement quests remain **OPEN**.

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

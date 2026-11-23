# DiceFree — Psion Tier III evolutions — design interview

**Status:** Tier III **Rounds 1–6 USER-CONFIRMED**, latest **2026-11-23**. Read the latest round as authority wherever it supersedes earlier rounds. These classes are **DESIGN ONLY**, **not implemented in Unity**. Identity and which parent abilities are retained are now partly approved; precise ranges, spell upgrade coefficients, new skill mechanics, rank gates and advancement quests remain **OPEN**.

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

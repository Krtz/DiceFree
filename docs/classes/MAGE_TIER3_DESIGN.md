# Mage Tier III — First Coverage Evolution: Design Round 1 (2026-11-26)

**STATUS: INTERVIEW OPEN. No Mage Tier III subclass identity, name, skill roster, mechanics, numbers, trainer or implementation has been user approved in this round.**

## Strategic scope (USER APPROVED PREVIOUSLY)

Design **at least one Tier III evolution for each of eight current normal Tier II classes** before designing extra branches for any already-covered parent. Current covered: Psion (Psionic Knight/Mirage Mage, user reports implemented); Ranger (Marksman, documented implemented); Greatblade (Berserker, user-approved design). Remaining first-coverage candidates: Mage, Defender, Thief, Priest and Acolyte. **User selected Mage to design next.** Greatblade extra tank/Juggernaut branch is deferred. Tier II level60+ normally advances to a separate Tier III level1 manifestation, retaining parent class history; preserve actual global advancement contracts.

## Source review: currently authored Mage Tier II

Primary source: `docs/classes/MAGE.md` and code/asset source `Assets/_DiceFree/Settings/Skills/Mage/Mage class.asset`, `Assets/_DiceFree/Scripts/Progression/MageSkillProgression.cs`, `Assets/_DiceFree/Scripts/Combat/MageCombatTuning.cs`. Older interview `docs/classes/MAGE_TIER2_DESIGN.md` contains historical open questions and must not override later authored Mage notes.

Mage is `class.mage`, **INT-based six-school elemental GENERALIST** using a wand/floating orb. Actual Mage Lv1 attrs are **VIT11 / STR5 / AGI8 / INT18 / SPR9**, per-level growth **+1.5 / +0.5 / +0.75 / +3 / +1.5**. Six actives plus two passives, total **80 rank purchases**:
- **Ember Bolt, Fire, cap18**: single-target projectile.
- **Frost Snare, Ice, cap12**: damaging freeze capped at2.5s (current authored tuning).
- **Arc Lightning, Electric, cap12**: chains through distinct enemies, cap8.
- **Math Barrier, Math, cap5**: self-only 2r-second Mana-to-HP damage redirection, redirects `(25+5r)%`, Mana consumption `2−0.1r` per prevented HP; partial available Mana works.
- **Quicksand, Nature, cap10**: ground damage/slow, current radius3.5m.
- **Half-Life, Radiation, cap8**: single-target DoT, `−5 DEF × rank` (current prototype applies both defense channels).
- **Elemental Weaving, passive, cap8**: distinct element rotation stacks (up to unique elements), each stack +5% spell/basic damage, elemental cast reduces other Mage cooldowns by0.2s; after8s without elemental casts, stacks decay, and passive includes close-range hostile resistance reduction aura up to8pp.
- **Mana Attunement, passive, cap7**: +30 maximum Mana and +0.5 Mana regen/s per rank in current implemented prototype; do not treat old Mage-only Mana capacity formula as the NEW global standard.

Mage design doc cites current prototype cooldowns **Ember5s / Frost8s / Arc12s / Quicksand14s / Half-Life10s / Math Barrier18s**. Current normal basic `Wand Spark` non-elemental INT-based and separate from skills. Parent advancement: Cornberg brewer/retired Mage mentor, `A Mind for Magic` quest plus Slime Dungeon tree five-ring puzzle. Parent level30 Magically Touched → new Mage level1. No ultimate before TierIV in current Mage description.

**IMPORTANT GLOBAL MANA RECENCY:** Earlier current Mage source/doc describes `80+10×INT` max pool and `2+0.04×INT` regen; subsequently in `docs/TIER3_ONE_PER_TIER2_ROADMAP.md` Marksman Round9 the USER approved **one global Mana max formula `ClassBaseMana+5×INT` and regeneration `4+0.1×SPR` for all Mana users** with classes varying class base/attributes/item effects, and asked for migration. Third-tier Mage must **follow the later approved global policy** rather than inventing a different special Mana/INT coefficient. Audit existing Unity implementation during authorized implementation, not now.

## Proposed Tier III identity candidates (DESIGNER ONLY, NOT USER APPROVED)

1. **Wizard — versatile multi-element specialist** (designer preference for first coverage). Deepens the generalist Mage's Elemental Weaving, encouraging alternating elemental spell schools and spell sequences for sustained magical DPS, ability synergy and perhaps reduction of cooldowns. Preserve all six elements' relevance. Needs explicit decision on how many existing abilities are inherited/evolved or replaced. Differentiates from TierII through a more complex spell-combination identity, not simply bigger Ember Bolt.
2. **Sorcerer — burst / Mana expenditure specialist.** Builds an Arcane Overload-like mechanic through investing or consuming Mana/power for high-impact bursts. Higher peak damage and lower sustain or defensive flexibility. The overload resource or mechanic, skills and numbers are wholly OPEN; do not canonize.
3. **Arcanist — Math-element specialist, geometry and tactical control.** Evolves Math Barrier into more powerful deterministic/mathematical spells, damage redirection, manipulation and positioning. Needs identity distinct from Psion and existing Mage's defensive Math Barrier; no approved ability or formula.

Reserve unchosen names/identities as future branches without treating them as accepted progression trees or promising implementation. Other directions may be proposed by user. Uniqueness and TierII continuity matter more than flavor naming.

## Opening Mage Tier III design questions (not yet answered)

1. Choose core identity **Wizard (multi-element combos)**, **Sorcerer (burst/overload)**, **Arcanist (Math/control)**, or other.
2. Preferred role: sustained magical DPS, explosive burst magical DPS, hybrid damage/control, damage/support or something else.
3. Desired relationship to parent six actives and two passives: keep/evolve some vs intentionally replace more; which signature spells must survive?
4. Should first-coverage Mage TierIII keep broad **all-six-element** access or meaningfully specialize; is Math a school to deepen?
5. Prefer familiar **Mana-only resource** under the shared global formula, or a new secondary class mechanic generated/spent by the new abilities? Do not silently assign mana formulas.

**Process:** After each ANSWERED design-question round, update relevant GitHub design documents, commit and push to `feat/verdant-wilds`. This initial task is a design interview opening, not authorization to modify Unity code/assets, merge branches or represent proposed mechanics as approved.


## Mage Tier III Round 1 USER ANSWERS — 2026-11-26: FIRE MAGE + OVERHEAT

### User-approved direction (supersedes the opening Wizard recommendation)

The user explicitly selects a **FIRE MAGE** (working descriptive class name, user says "We make a Fire mage") as **Mage's FIRST Tier III coverage evolution**, not Wizard/Sorcerer/Arcanist. They answer:
1. **Class identity:** Fire Mage, not one of the three original broad suggestions.
2. **Role:** BURST MAGICAL DPS.
3. **Skill inheritance:** MOSTLY NEW skills (not a simple larger Ember Bolt with the entire previous Mage kit), while still evolving from TierII Mage's INT/Mana lineage.
4. **Elements:** FOCUS ON FEWER ELEMENTS, primarily Fire rather than retaining all six TierII Mage schools. Do not automatically canonize the exclusive Fire-only rule or extra elemental school until user decides.
5. **Resources:** retain global MANA plus a new secondary **OVERHEAT** mechanic. **Casting several Fire spells in quick succession** must interact with Overheat and do *something* important, potentially **EMPOWER THE NEXT SPELL**. User says "maybe empower the next spell or something", so the precise trigger/threshold/consumption, resource scale and effects are NOT yet approved.

Preserve already-approved shared global Mana structure `MaxMana = ClassBaseMana + 5*INT`, `ManaRegenPerSecond = 4 + 0.1*SPR`, with class/base stats/equipment adjusting capacity; no separate Fire Mage-only Mana multiplier or regeneration equation. TierII Mage remains unchanged. TierIII normally unlocks as a new class level1 manifestation after Mage level60+ advancement; exact trainer/quest not chosen.

### Designer Overheat first-test IDEA (explicitly UNAPPROVED)

- Model secondary Overheat as a visible resource meter, suggested **0–100**, with **illustrative +25 Overheat per eligible Fire skill cast**. Both scale and award amount are PROPOSALS, not accepted numbers.
- Candidate condition: casting multiple *distinct or any* eligible Fire spells quickly builds or preserves heat; long pauses allow Overheat to decay/reset. Exact cast eligibility, timing interval, gain by skill, DoT tick eligibility, failed-cast handling and out-of-combat behavior open.
- Candidate spend: when at maximum Overheat, the **next eligible Fire spell is empowered**, consuming Overheat; its empowered effect varies by spell (extra area damage, stronger burn, stronger single-target explosion) rather than a single uniform number. This was a **user-floated possibility** and remains undecided.
- Alternative mechanisms to present for user choice: **Threshold empowered next spell**, **overheat window with temporary burst state**, or **risk/reward accumulating heat with potential overburn penalty**. No heat self-damage or Mana tax authorized by user. Do not accidentally grant passive crit chance, as Fire Mage's critical profile has not been designed.
- Existing Mage spells may inspire evolutions but exact retained skills or Fire Mage spell roster/slots, rank budget, stats, damage coefficients and skill cooldowns remain OPEN.

### Questions for Round 2 (no answers yet)

1. Overheat shape: 100-point meter/empowered next spell, stack counter, or timed temporary overheat state?
2. Eligible triggers: any consecutive Fire skill cast vs *different* Fire spells only; must attacks hit or simply cast successfully? Count basic wand fire attacks or not?
3. How does quick succession work: heat decay after a short time, hard combo timeout, or persistent heat out of combat?
4. What is the empowerment payoff: spell-dependent extra effect vs flat damage buff vs limited overheat-burst window?
5. Which TierII spells, if any, deserve Fire Mage versions? Ember Bolt can evolve; Math Barrier could become a defensive fire ward, or the new kit could be wholly new. Decide core spell themes before numbers.

**Scope:** this is design only; no Unity source/assets/build/merge/test. Commit and push these user answers before next questions.


## Fire Mage Tier III Round 2 — 2026-11-26 USER DECISIONS + NEXT SKILL ROSTER PROPOSAL

### APPROVED by the user (supersedes unresolved Round1 proposals)

User's explicit Round2 answers, reproduced faithfully:
1. **Overheat is a 0–100-point secondary meter.** At **100 Overheat**, the **NEXT eligible spell** is empowered. The exact empowered cast consumption/re-gain order and whether all Fire spells qualify for empowerment need technical choices below, not implied by this answer.
2. **EVERY SUCCESSFULLY CAST FIRE SPELL generates Overheat**, without needing to alternate different Fire spells or require a hit. Distinguish a valid completed spell cast from a failed/invalid cast. This does not automatically mean basic Wand Spark attacks, Fire DoT ticks, repeated laser ticks or proc explosions generate Heat; these are not separate casts unless the user approves.
3. **After a short inactivity pause**, Overheat begins **GRADUAL DECAY**. Exact delay in seconds, loss rate and when a cast resets the inactivity timer are not user-approved.
4. **Different eligible spells have DISTINCT empowered effects.** A single identical damage multiplier for every spell was not selected. Details, balance, special AoE/splash/DoT/buff interactions are still open.
5. **Evolve TierII Ember Bolt and replace TierII Math Barrier with a Fire-themed shield.** The new TierIII is otherwise a mostly new Fire-centric kit.

**Additionally user expressly requests these Fire Mage ability themes:** a **Fire damage-over-time spell**, a **Living Bomb-like delayed explosion**, a **Fire LAZOR** (beam/channel concept), and **a passive/aura**. The user has not approved exact names, effects, rank counts, how many passives, cooldowns, spell stats or the nature of this aura. User's FIRE-centric burst DPS identity and global Mana + Overheat resource remain locked from Round1.

### First-pass spell roster PROPOSAL — exact names/effects NOT YET USER APPROVED

| Working proposed ability | User-requested / inherited basis | Proposed normal effect | Proposed empowered effect |
| --- | --- | --- | --- |
| **Fireball** | Ember Bolt evolution (approved inheritance) | Direct Fire projectile, main repeatable offensive cast | Impact also causes sizeable Fire splash AoE; intensity/radius TBD |
| **Flame Ward** | Math Barrier → Fire shield (approved thematic transformation) | Temporary defensive Fire-element shield; exact mitigation vs reflection, targeting and Mana interactions TBD | Ward explodes into Fire AoE on break or expiration; define empowered eligibility for supportive spell |
| **Ignite** | User Fire DoT request | Apply Fire damage-over-time Burn to an enemy | Stronger and/or spreading Burn; refresh/stack rules TBD |
| **Living Bomb** | User delayed-bomb request | Attach volatile Fire debuff, explode after timed fuse | Larger splash and/or secondary explosions; prevent endless proc chaining |
| **Incineration Beam** | User Fire LAZOR request | Channeled directional Fire beam, damage ticks while maintained | Wider beam, stronger ticks and/or finishing burst; decide when Heat is granted and spent for channel |
| **Heart of the Inferno** | User passive/aura request | Fire damage support, target Fire-resistance reduction, or party Fire-damage aura | Passive has no independent cast; decide whether and how rank affects Overheat empowerment rather than attempting to auto-empower a passive |

All names are temporary designer proposals, not new user-confirmed nomenclature. These are FIVE proposed actives + ONE proposed passive, **not yet a user-approved total**, and adding an Overheat-generating passive requires its own design decision. The class is a FIRE specialty; whether it has secondary elemental spells is not locked. All damage is expected to use normal magic/global resource systems; no per-class Mana coefficient exceptions.

### Overheat lifecycle implementation questions, NOT YET APPROVED

- When a successful Fire spell brings heat from below 100 to 100, should **that cast** remain normal and **the next successful eligible Fire spell** receive empowerment? Designer suggests YES, preserving the explicit next-spell behavior.
- When a cast begins at 100, does it **consume all 100** and then grant its own ordinary Heat for successful cast, or consume 100 and generate zero Heat for itself? Designer suggests consume 100 and prevent its own cast from replenishing, to avoid immediate feedback loops; **NOT approved**.
- Heat gain per valid cast: was illustrative +25 in Round1, but NO amount has yet been user-approved. Should Flame Ward count? Its successful Fire cast appears eligible given user says every successfully cast Fire spell generates Overheat, but whether it is empowerable or can spend it needs review. Continuous/tick spell Heat should be awarded once per complete cast, not per DoT/laser tick, as a designer anti-exploit candidate.
- Does Heat grow by a single fixed amount for all spells or rank/cast-time-specific values? Decay delay, linear loss/s, combat vs out-of-combat decay, persistence across scene/manifestation loads need design. The already-approved global Mana math stays `MaxMana=ClassBaseMana+5*INT`, `ManaRegenPerSecond=4+0.1*SPR`.
- Empowered Living Bomb requires clear explosion attribution to the casting owner, no double rewards or exponential chain; Flame Ward empowered break/expiry explosion should follow shared shield rules; beam channel ticks must not each independently generate Heat.

### Proposed Round3 questions (await user response)

1. Approve/revise the six working skills, especially the **five actives + one passive** structure and any missing Fire spell or defensive escape ability.
2. How many Overheat on successful Fire spell casts: fixed **+25** to reach max after ~4 casts, or spell-specific gain / another rate?
3. When at 100: empower next **eligible** Fire spell only; expend all 100, with that empowered cast generating **no new heat**? Or allow the spent cast to produce its normal Heat after consuming?
4. Preferred Fire LAZOR: stationary continuous channel, freely aimed channel, or delayed giant beam; whether empowered beam is wider, stronger or finisher blast.
5. Passive/aura targets: personal Fire damage, allied Fire damage, hostile Fire resistance down, or a hybrid. Decide whether shield gets empowered explosion or different payoff.

**Status:** user-approved mechanics and user-supplied skill themes documented, detailed roster/numbers and empowered outcomes designer-only. NO Unity code, assets, builds or runtime testing performed. Continue the every-round documentation commit/push workflow.


## Fire Mage Tier III Round 3 — 2026-11-26 USER APPROVALS: 5 ACTIVES + 2 DISTINCT PASSIVES/AURA, 25 BASE HEAT, SINGLE-BLAST LAZOR

**AUTHORITATIVE USER ANSWERS** (supersede Round2's proposed five-actives-plus-one-passive count and any channeling assumption):

1. **APPROVES the proposed five active spell identities**: (working names) **Fireball** evolving Ember Bolt; **Flame Ward** replacing Math Barrier with Fire shielding; **Ignite** Fire DoT; **Living Bomb** delayed Fire explosion; **Incineration Beam** Fire LAZOR. No sixth active approved, and exact names, effects, percentages and numerical ranks remain provisional. This approval covers spell identity/role, not every designer-proposed empowered effect.
2. **Explicitly REVISES the other slot count**: "**one passive and one aura**" in addition to the five actives. Thus **SEVEN rankable ability concepts: 5 active spells, 1 Overheat-management passive, 1 separate resistance aura**. The previous 5+1 roster is SUPERSEDED.
3. **Overheat gain selection:** "**25 Heat**" per successfully cast Fire spell was selected as the reference amount. **However** the user also explicitly says "**the passive should determine how much heat is generated by spells**", so do NOT freeze +25 as a rank-independent universal award. Treat 25 as the chosen first-pass **reference/base figure** pending an explicit passive-rank scaling curve and whether 25 applies at passive rank0/rank1 or at cap; ask next round. Do not auto-award Heat from each DoT tick, beam effect, bomb explosion or each target hit: one distinct successful Fire SPELL cast is the approved trigger. Skill cast completion/award timing needs implementation agreement.
4. **At full 100 Overheat**, the **next eligible spell receives its spell-specific empowered result, CONSUMES THE WHOLE 100 HEAT, and the empowered cast DOES NOT generate any new Heat**. This is approved; avoid consume-then-gain loopholes and self-refilling bomb/laser procs. A cast that first fills the meter should remain ordinary and the NEXT eligible Fire spell gets empowered, consistent with previous explicit next-spell choice. The original inactivity-driven gradual Heat decay remains confirmed; delay/rate not yet decided.
5. **Fire LAZOR IS NOT A CONTINUOUS CHANNEL**: user chooses "**long windup followed by one enormous beam blast**". It may be a line/ray, piercing or blocked by geometry per later design, but is a single resolved damage event after windup, not repeated periodic damage ticks. Empowered beam-specific behavior and width/range/telegraph/cast commitment are open.
6. **RESISTANCE AURA must affect BOTH factions differently:** user explicitly clarifies "**the aura should give allies increased fire resistance and reduce enemies fire resistance**". No allied Fire damage buff selected. Should normally include caster/self if within ally eligibility, but self inclusion/radius/stacking/percentages are still unapproved and must be designed. Unlike Elemental Weaving the user's desired effects are **defensive Fire resistance for allies AND offensive resistance reduction for nearby enemies**.
7. **PASSIVE should govern Heat generation and related Overheat mechanics** rather than duplicating an aura/Fire-damage passive. Working name **Overheat Mastery**, not approved; aura working name **Infernal Presence**, not approved. Exact passive max rank, +Heat/rank and decay interaction require user agreement.
8. Preserve previously locked Mage TierIII Fire/burst focus, global Mana `ClassBaseMana+5*INT` and `4+0.1*SPR` regen formula, and separate TierIII class manifestation. Parent Mage progression/skills are not retroactively changed.

### Updated seven-ability roster: USER-APPROVED ROLES, designer working NAMES / effects pending

| Ability | Status of identity | Core confirmed direction | Specific mechanics pending |
| --- | --- | --- | --- |
| Fireball (name provisional) | **Active 1 approved concept** | Evolved Ember Bolt, Fire projectile | Raw damage, AoE vs ST, empowered splash, cast time/CD |
| Flame Ward (name provisional) | **Active 2 approved concept** | Fire shield evolved from Math Barrier | Absorb vs damage-to-Mana, shield element/mitigation, self vs ally, empowered explosion |
| Ignite (name provisional) | **Active 3 approved concept** | Fire DoT | Tick cadence/duration, damage stacking/refresh, empowered spread |
| Living Bomb (name provisional) | **Active 4 approved concept** | Delayed Fire explosion | Fuse, explosion size, detonations, empowered effect |
| Incineration Beam (name provisional) | **Active 5 approved concept** | LONG windup → ONE huge Fire LAZOR beam BLAST | Cast time/width/range, single or piercing multi-target, empowered effect |
| Overheat Mastery (name provisional) | **Passive 1 approved concept** | Sets/changes Fire spell Heat generation | Rank0 vs rank1 base25, scaling, decay, cap |
| Infernal Presence (name provisional) | **Separate aura 1 approved concept** | Allies +Fire resistance AND nearby enemies -Fire resistance | Percent/rank, radius, self inclusion, stacking/cap, always-on vs toggle |

Only the identities/roles above are LOCKED. No rank caps or 150-rank allocation have yet been approved; TierIII first-playtest precedent is 150 total purchasable ranks per class level1–150, but Mage TierIII exact allocation and rank gate schedules require their own explicit decision.

### Next design questions (Round 4) — NOT YET ANSWERED

1. **Heat passive scaling:** is 25 Heat the starting per-cast gain at passive rank0/1, with each purchased rank increasing it, or is 25 the cap at full passive rank? Proposal to test: base25 at rank0 and +1 Heat per rank, though this can make a 100-Heat buildup take 3 casts at sufficiently high passive rank; preserve chosen 25 reference. Another option is lower starting Heat ramping to25. Ask user.
2. **Heat decay:** how long after the last successful Fire cast before decay starts, and Heat lost per second? Suggest a delay5s and 10 Heat/sec as a first-test candidate, NOT approved. Does normal out-of-combat state change decay?
3. **Aura values:** propose +1 percentage point allied Fire resistance/rank and -1 percentage point hostile Fire resistance/rank with a fixed radius, but this could add lots of aura rank damage/mitigation; exact values and stacking not approved. Ask if self is included and strongest-only stacking.
4. **Flame Ward:** preserve a Mana redirect like Math Barrier, create fixed Fire-absorb shield, or make a Fire-based retaliatory protection ability; if empowered, consider shield explosion on break/expiry only when approved.
5. **Incineration Beam:** do all enemies intersected by the line take full damage (geometry-blocked) and what empowered version does? Suggest longer-range/wider vs larger flat/INT hit, not approved.

**IMPLEMENTATION STATUS:** Documentation-only design round; no Unity files, assets, tests, or implementation changed. User requires documentation update, commit and push after every answered design round.

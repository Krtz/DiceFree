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


## Fire Mage Tier III Round 4 — 2026-11-26: USER CORRECTIONS OVERRIDE FORM SELECTIONS

**LATEST USER DECISIONS ARE AUTHORITATIVE.** The separately typed "Additional notes" explicitly override the multiple-choice selection where they differ. This update supersedes the previous Round3 roster and laser placement where affected. This is DESIGN DOCUMENTATION ONLY, not Unity implementation.

### User-approved mechanics

1. **Heat generation through passive:** Heat per completed Fire spell cast begins **LOWER than 25** and rises to **25 Heat per successful Fire cast AT MAXIMUM PURCHASED PASSIVE RANK**. The user chose "Lower initial Heat rising to 25 at maximum passive rank." Exact starting Heat at passive rank0 or rank1, the rank cap and interpolation remain **UNDECIDED**; do NOT assume 25 per cast at every passive rank, 25 at rank1, or an already-approved linear curve. At full100 Heat next eligible spell consumes all100 and does not refill its own Heat, as approved Round3. No DoT/bomb proc tick should be counted as a separate cast without a later explicit design decision.
2. **Heat decay OVERRIDE:** user explicitly corrects form selection to "**begin after 10 sec losing 10 heat/sec**". Therefore inactivity wait is **10s**, NOT 5s. Once threshold passes, decrease heat **10/sec** continuously (fractional per-frame permitted, clamp at 0). Exact reset semantics on ordinary/empowered casts or combat/scene state remain for implementation design.
3. **Aura approved numeric rate:** EACH purchased aura rank gives **+0.5 percentage points Fire resistance to eligible allies** and **-0.5 percentage points Fire resistance to eligible enemies**. This is resistance percentage-point shift, NOT 0.5% damage bonus; ally protection and hostile vulnerability coexist as separate directions. Exact maximum aura ranks, radius, caster inclusion, overlapping same-name auras/stacking and resistance floors remain open.
4. **Flame Ward OVERRIDE:** preserve TierII Math Barrier's **Mana-based incoming damage absorption / damage redirection**, rather than replacing the protection with a plain separate Fire-only absorption shield. When **EMPOWERED by spending 100 Heat**, Flame Ward **ADDITIONALLY grants a Fire-element shield**. User's direct note "retain mana-based damage absorption, empowered version adds a fire shield also" supersedes the form option and earlier proposed empowered explosion on break/expiration. Exact inheritance of Math Barrier's level-specific ratios, Mana efficiency/duration/rank scaling and size/duration/element interaction of the additional Fire shield are open.
5. **Fire LAZOR / Pyromancer TierIV OVERRIDE:** the user explicitly changes mind: **Fire LAZOR is REMOVED from the TierIII Fire Mage roster**, deferred as an envisioned signature spell of **TierIV Pyromancer**. User proposes/chooses **Pyromancer** as the TierIV Fire Mage evolution name, with TierIV ability design intentionally deferred until after first-coverage TierIII paths are complete. The previously chosen long-windup, single enormous Fire beam shape is preserved as **future TierIV idea**, not an authorized TierIII active or implementation feature. The form's "broad heavy AoE beam / empowered burning ground" would pertain to the now-DEFERRED beam, **not** to TierIII. No TierIII fire laser exists after this decision.

### Current TierIII Fire Mage spell roster after laser deferral

| Kind | Working name | User-authorized theme | Mechanics awaiting numerical details |
| --- | --- | --- | --- |
| Active | Fireball | Ember Bolt evolution, direct Fire projectile | Damage, cast timing, cooldown, distinct empowered effect |
| Active | Flame Ward | Math Barrier evolution, **retains Mana-based protection** | Redirect ratio/efficiency, duration, cooldown; **empowered adds Fire-element shield**, not explosion |
| Active | Ignite | Fire DoT | Duration/tick/refresh and distinct empowered effect |
| Active | Living Bomb | Delayed Fire explosion | Fuse, AoE, refresh/explosion and distinct empowered effect |
| Passive | Overheat Mastery | Controls Heat gain per successful Fire cast, growing to **25 at max passive rank** | Rank cap and early-rank Heat formula; decay-related ranks if any |
| Aura | Infernal Presence | **+0.5pp allied Fire resistance, -0.5pp hostile Fire resistance per rank** | Radius, rank cap, caster inclusion, strongest aura stack policy |

**TierIII presently has FOUR approved active themes + ONE passive + ONE aura = SIX ability themes.** Previous "5 actives +1 passive +1 aura" is now HISTORICAL, not a mandate to invent a substitute skill. **Next explicit user decision:** either keep exactly four actives or add a fifth distinct TierIII active skill (e.g. Fire Nova area blast, Flame Step mobility, Cinderstorm area hazard, or another user-chosen ability). This also affects eventual 150-rank point budget and exact gates; no final 150 split is approved.

**TierIV future reservation:** **Pyromancer (Mage→Fire Mage→Pyromancer)** with a gigantic Fire LAZOR, whose long windup and one enormous beam were discussed, but the complete TierIV kit, name finalization in runtime, resources and advancement timing remain future design, not current implementation authorization.

**Still approved:** Fire Mage TierIII burst magical DPS, global Mana plus Overheat, 100 Heat max, successful Fire cast Heat event, spell-specific empowerment, full Heat consumption with no same-cast refill, slow inactivity decay, all Fire-themed, distinct Heat passive and resistance aura; shared game-wide max Mana `ClassBaseMana+5*INT` and regeneration `4+0.1*SPR`. No TierIV design work until coverage goal is met.

### Next question round (not answered)

1. Do we keep FOUR TierIII active spells, or add one more to replace deferred Fire LAZOR? Candidate fifth spells: Fire Nova AoE explosion, Flame Step short-range movement with fire trail, or Cinderstorm persistent burning ground. User chooses.
2. How low is starting Heat per cast at passive rank0 or first purchased rank; what passive rank cap yields 25?
3. Is Fire Ward's Mana redirect formula unchanged from Mage Math Barrier (with rank scaling adjusted for TierIII), and what is the empowered additional Fire shield's duration/capacity?
4. What maximum aura rank and radius; include caster, and strongest-only aura stacking versus additive?
5. Nail down normal/empowered Fireball, Ignite and Living Bomb effects and interaction with existing DoT/Fire resistance.

**Workflow:** GitHub docs updated, committed, pushed after answered round. No Unity source/assets changed.


## Fire Mage Tier III Round 5 — 2026-11-26: USER APPROVED SIX-ABILITY ROSTER, LOW-RANK HEAT, MANA WARD, AURA STACKING, STRONGER SINGLE-TARGET BURN

**These decisions supersede open questions and prior contradictory designer suggestions.**

### User-approved constraints

1. **Exactly FOUR active spell concepts + ONE separate Heat passive + ONE separate two-sided resistance aura** for the first TierIII Fire Mage evolution: **Fireball, Flame Ward, Ignite, Living Bomb, Overheat Mastery (working name) and Infernal Presence (working name)**. The user explicitly chooses **NO replacement for the deferred TierIV Pyromancer Fire LAZOR**. Thus SIX ability concepts total, not seven; the prior seven-ability layout was superseded when the beam moved to future TierIV. Final numerical skill-rank caps/150-point budget still pending approval.
2. **Passive Heat generation at rank0 = 10 Heat per successfully completed eligible Fire spell cast**, scaling upward to **25 Heat per cast at the passive's maximum purchased rank**. Both endpoints are NOW USER APPROVED. Rank cap and interpolation formula remain open; a simple linear curve is a DESIGNER PROPOSAL, not a user-approved coefficient. The prior Round3 `25` figure is thus the maximum-rank endpoint, NOT a constant base.
3. **Flame Ward evolves the Mage Math Barrier's PERCENTAGE-BASED MANA DAMAGE REDIRECTION / ABSORPTION mechanic**, adjusted/scaled for TierIII. It is NOT a fixed independent Mana pool as its basic effect. When empowered, Flame Ward also creates an **ADDITIONAL FIRE-element absorption shield**, per approved Round4. Exact percentage, per-HP Mana efficiency, duration/rank curve and extra Fire shield strength remain undecided. Follow shared elemental shield mechanics and shared global Mana capacity/regeneration formula; no Fire Mage-only Mana multiplier.
4. **Infernal Presence resistance aura INCLUDES THE CASTER** in allied Fire resistance. **Identical same-name auras DO NOT STACK**: strongest applicable aura wins per entity/effect, including hostile vulnerability where multiple Fire Mages overlap. Approved numeric delta remains **+0.5 percentage points allied Fire resistance per aura rank** and **-0.5 percentage points enemy Fire resistance per aura rank**. Rank cap/radius/floors still open. Hostile effect never affects allied actors.
5. **Empowered Ignite is a MUCH STRONGER SINGLE-TARGET Fire Burn/DoT**, NOT a spread to nearby units, nor an opening explosion as its defining empowerment. Exact tick damage multiplier, duration, burn refresh/stack and mitigation are open. Keep consistent with global Fire magical damage/DoT and avoid unapproved splash on empowered Ignite.

### Still-user-approved Overheat mechanics

- Heat meter 0..100; successful casts of an eligible Fire SPELL contribute Heat; ordinary successful spell cast gain is passive-rank-dependent, ranging **10 at rank0 to25 at passive cap**.
- When heat reaches100, **the NEXT eligible Fire spell** receives its own empowered effect, consumes all100, and **that empowered spell contributes zero new Heat**.
- Decay begins after **10 seconds with no successful Fire spell casting**, then falls **10 Heat/second** until zero; make fractional time accumulation safe.
- Heat is a SECONDARY gauge alongside standard Mana; for all Mana users use globally approved `MaxMana=ClassBaseMana+5*INT` and `ManaRegenPerSecond=4+0.1*SPR`, class differences via class bases, starting INT/SPR, stat growth and equipment. No new class-specific mana coefficient.
- Fireball direct Fire projectile evolving Ember Bolt; Flame Ward Mana redirect+bonus Fire shield when empowered; Ignite Fire single-target DoT with stronger single-target empowered burn; Living Bomb delayed Fire blast with its empowered effect still to be chosen.
- Fire LAZOR reserved for future **Mage→Fire Mage→Pyromancer TierIV**, not TierIII; no Unity changes in this round.

### Round6 proposed numerical starting point (DESIGNER ONLY, NOT USER APPROVED)

For the **six approved TierIII ability concepts**, follow the existing TierIII convention of **150 purchasable skill ranks total with 1 skill point/class level1..150** as a candidate requiring explicit user approval for this class. Designer split:
- **Fireball35**, **Flame Ward20**, **Ignite30**, **Living Bomb30**, **Overheat Mastery20**, **Infernal Presence15** = **150** total ranks.
- A suggested **linear Heat-per-cast formula for passive purchased rank r0..20** is `HeatPerSuccessfulFireCast(r) = 10 + 15*r/20 = 10+0.75*r` (rank0=10, rank10=17.5, rank20=25). Preserve fractional Heat internally; 100 capped. Different rounding choices change empowerment frequency. No Heat from individual DoT ticks, bomb explosions or any extra beam tick.
- Proposed aura at rank15 gives **+7.5pp allied Fire resistance and -7.5pp enemy Fire resistance** if user approves rank cap15. Aura is strongest same-name effect only, incl caster, with candidate **10m radius** (NOT approved); aura frequency/performance should use existing stat modifier architecture.
- Suggested single-target empowered Ignite outcome **2× normal Ignite total damage over the same duration**, or **+50% duration plus +50% tick damage** as alternative. No numeric multiplier approved; the only accepted constraint is *stronger single-target Burn*, no spread.
- Proposed Flame Ward normal baseline: reuse **shape** of Math Barrier redirect ratio and Mana efficiency (tierIII may rescale as ranks differ), and empowered extra Fire shield capacity scaling with own MaxHP or INT; not locked.
- Living Bomb empowerment suggested larger blast radius or stronger main detonation; no approved direction.
- Exact first-unlock levels, mathematical rank eligibility gates, INT/VIT/SPR attributes, offensive flat+INT coefficients, Mana costs and cooldowns deferred to later rounds; confirm ability cap totals before building a 150-level audit.

### Follow-up questions for user

1. Approve/adjust **150 skill-point total** and preliminary six rank caps `35/20/30/30/20/15`?
2. Approve **linear Heat 10+0.75r** if passive cap20, or choose different rank scaling/rounding?
3. Should the resistance aura be **always on** with **10m** radius, or an activated aura/range tied to class behavior?
4. Should empowered Ignite deliver **2× its ordinary total DoT damage for unchanged duration**, or more intense/longer burn?
5. Should empowered Living Bomb expand splash, intensify damage, or add a safe secondary effect (e.g. burning ground) without infinite chaining?

**Documentation ONLY; no Unity gameplay/code/assets/build performed. Update/commit/push every answered design round.**


## Fire Mage Tier III Round 6 — 2026-11-26 USER-APPROVED 150 RANK CAPS, LINEAR HEAT, 15m AURA, IGNITE x3 AND ONE-HOP LIVING BOMB

**LATEST AUTHORITATIVE USER DECISIONS:** Explicit additional notes OVERRIDE the earlier multiple-choice cap split and further ADD to Living Bomb's empowerment. Supersedes any older "proposed 35/20/30/30/20/15" rank split or generic empowered Living Bomb alternatives.

### Final first-playtest 150-rank distribution USER APPROVED

| Fire Mage TierIII ability | Max purchasable ranks | Type | Status |
| --- | ---: | --- | --- |
| Fireball | **35** | Active Fire projectile evolving Ember Bolt | User approved cap |
| Flame Ward | **25** | Active Mana-redirection defense; empowered gains Fire-element shield | User approved cap; **+5 ranks** vs Round6 form's original selection |
| Ignite | **30** | Active single-target Fire DoT | User approved cap |
| Living Bomb | **30** | Active delayed Fire explosion | User approved cap |
| Overheat Mastery | **15** | Passive for heat generation | User approved cap; **-5 ranks** vs Round6 form's original selection |
| Infernal Presence | **15** | Permanent dual-sided Fire resistance aura | User approved cap |
| **TOTAL** | **150** | 4 active + 1 passive + 1 aura | **USER APPROVED** |

Progression follows shared TierIII precedent: **one spendable skill-rank point per Fire Mage class level1..150, zero free starter ranks**. Exact first unlock levels / rank eligibility cadence need a full 150-level gate audit in a later round; do not silently assign rank-1 points or reuse Berserker's rank gates.

### Heat-per-cast final USER-APPROVED formula

User approves LINEAR interpolation between rank0 **10 Heat/successful Fire cast** and max passive rank15 **25 Heat/cast**. Therefore exact formula:
`HeatGainedOnOrdinarySuccessfulFireCast(r)=10 + r`, for integer `r=clamp(OverheatMasteryRank,0,15)`.

Examples: rank0=10, rank5=15, rank10=20, rank15=25. 0..100 Heat meter. Once at100, **NEXT eligible Fire spell** is empowered, **consumes all100** and **generates NO same-cast Heat**; the cast that reaches100 is NOT retroactively empowered. No heat from separate DoT ticks, bomb explosions or extra target hit packets without a distinct valid cast. After **10s without a successful Fire cast**, lose **10 Heat/s**, smoothly/time-delta safe; preserve previous approval.

### Aura latest USER APPROVAL

**Infernal Presence is PERMANENT, 15 METRE RADIUS**, includes caster in ally side, applies ally **+0.5 percentage points Fire resistance per aura rank** and hostile **-0.5 percentage points Fire resistance per aura rank**. At approved cap15, this becomes ally **+7.5pp**, hostile **-7.5pp** (where all rules/eligibility permit). **Strongest same-name aura only per recipient; no stacking**. Use existing aura resistance modifiers and robust combat affiliation; do not confuse percentage points with a Fire damage multiplier. Non-Fire resistance channels unaffected. Exact resistance floor/cap handling through global system; no special arbitrary bypass approved.

### Empowered Ignite latest USER APPROVAL

At100 Heat, an empowered successful **Ignite** consumes100 and applies a **SINGLE-TARGET Fire Burn with exactly THREE TIMES the total damage of normal Ignite at identical skill rank and stats, over the SAME duration**. Prefer 3x tick damage at unchanged tick count/schedule, subject to shared DoT timing architecture. **No secondary targets, no spread, no altered total duration**; previous two-times and +50% duration/tick alternatives SUPERSEDED. Exact normal DoT flat+INT scaling, duration, ticks, reapplication refresh rules and Mana cost remain undecided.

### Empowered Living Bomb latest USER APPROVAL

User selected **"Stronger detonation damage"** and additionally insists **"make empowered living bomb spread to others within the explosion radius (but can only spread once)"**. Therefore BOTH effects are REQUIRED:
- Primary empowered Living Bomb causes **stronger detonation damage** than its normal version (multiplier / formula still unapproved).
- At its explosion, it **applies a propagated Living Bomb to other eligible enemies inside that explosion radius**. Use normal appropriate target/geometry/ownership rules. **Spread is ONE GENERATION ONLY**. A bomb created by the original empowered explosion may still explode as designed, but its explosion **MUST NOT spawn more Living Bombs** or allow infinite/chained recursions. Define explicit propagated-bomb marker / hop depth, owner, duration, and duplicate target rules during implementation.
- Whether second-generation bombs have full normal detonation damage, reduced damage, inherited fuse, or detonate immediately is **NOT yet user approved**. Neither a global chain reaction nor limitless target multiplication may be inferred.
- No award of new Overheat from triggered bomb explosions/propagation, only separate successfully cast Fire spells.
- The blast radius itself and exact stronger explosion damage still to be chosen.

### Current skill structure (unchanged)

Four TierIII active skills Fireball, Flame Ward, Ignite, Living Bomb; passive Overheat Mastery; aura Infernal Presence. Fire LAZOR deferred to future **TierIV Pyromancer**, not an extra TierIII spell. Flame Ward keeps Math Barrier-style **percentage-based Mana redirection** and grants **ADDITIONAL Fire-element shield on empowerment**. Universal Mana `ClassBaseMana+5*INT`, regen `4+0.1*SPR`. No per-class Mana coefficients invented.

### Proposed Round 7 priorities (NOT YET USER APPROVED)

1. **Living Bomb propagated version:** copied bombs explode after the normal fuse or immediately? Damage at normal rank or reduced? Duplicate-target replacement rules?
2. **Normal Fireball and empowered Fireball:** define single-target base hit vs splash empowerment, AoE radius and crit eligibility, then later flat+INT coefficient per rank.
3. **Flame Ward:** adjust Math Barrier's ratio/duration/Mana-per-HP scaling to rank cap25, determine empowered extra Fire shield maxHP/INT-derived amount and duration/stack.
4. **Ignite:** normal Fire DoT duration, tick cadence and refresh policy; empowered 3x normal over same timing.
5. **Skill unlocks and rank gate budget:** choose initial unlock levels and devise audited level1–150 purchasing schedule for cap split 35/25/30/30/15/15.

**Design documentation ONLY; no Unity files, builds, tests or game implementation changed in this round.**


## Fire Mage Tier III Round 7 — 2026-11-26 USER APPROVALS: LIVING BOMB PROPAGATION, BURNING GROUND FIREBALL, INT-SCALED WARD, IGNITE TIMING, FIRST UNLOCK LEVELS

**LATEST USER ANSWERS to all five Round7 questions.** These supersede earlier designer alternatives but do NOT fill in unspecified numeric coefficients/cooldowns/secondary status rules.

1. **Empowered Living Bomb propagation CONFIRMED:** when the **original empowered Living Bomb** detonates, it places one normal Living Bomb on each other valid enemy within its explosion radius, subject to normal combat target legality. Secondary bombs inherit the **NORMAL Living Bomb fuse duration and FULL NORMAL-rank detonation damage**, **not the empowered detonation damage**. They can detonate normally, but **do NOT spread onward** (one-hop propagation only; explicit propagated marker/generation depth). Initial empowered explosion retains the previously approved stronger damage. Propagated bombs do not grant Heat; Heat only from independently successful Fire spell casts. Reapplication/duplicates and precise fuse, radius and damage coefficient remain OPEN.
2. **Empowered Fireball CONFIRMED:** normal Fireball is the single-target direct Fire projectile evolved from Mage Ember Bolt. Empowered Fireball **creates BURNING GROUND underneath the ENEMY IT HITS**. This **REPLACES the earlier designer suggestion that empowerment adds immediate AoE splash damage**. Burning ground is a persistent area with damage/Fire interaction to design: radius, duration, tick timing, placement on a moving or killed target, damage scaling and hit/miss/obstacle behavior still OPEN. Do NOT silently add immediate radial explosion on top. The empowered spell consumes 100 Heat and grants zero new Heat as previously approved.
3. **Flame Ward scaling CONFIRMED:** retains Mage Math Barrier-style **percentage-based incoming damage redirection to spent Mana**, but that redirection (or defensive strength) **scales with purchased Flame Ward rank r=1..25**. At full100 Heat empowered Ward **ADDS a separate Fire-element absorption shield whose capacity scales with caster INT** (not MaxHP). Exact flat + INT coefficient, shield duration and damage absorption ordering, normal redirect percentage and Mana cost per prevented HP need approval. Inherited parent formulas may inform but must be adjusted to rank25, NOT extended blindly to invalid >100% redirect.
4. **Ignite duration/timing CONFIRMED:** normal Ignite applies an **8-second single-target Fire DoT, ticking every 1 second**; **recasting refreshes rather than stacking another same-caster DoT**. Empowered Ignite retains the SAME 8s duration and tick cadence but deals **exactly 3× normal total DoT damage** on its target. It does not spread to nearby units. Tick origin/refresh reset timing, overwriting normal vs empowered potency, global source stacking across players, and flat+INT tick formula still to design.
5. **First available ability unlocks USER APPROVED** for TierIII Fire Mage (separate class level1 manifestation): **Fireball level1, Overheat Mastery level1, Ignite level3, Flame Ward level5, Living Bomb level8, Infernal Presence level10**. Every ability starts at **purchased rank0**, and skill ranks can only be bought if sufficient earned points and eligibility gates. Current USER-APPROVED per-ability caps: **Fireball35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15 = 150**; one earned point per Fire Mage class level1..150. Exact ranks 2+ level gates and whole schedule are NOT YET AUDITED or approved; no rank point auto-spend at level1. The special first-unlock level for Fireball and passive at level1 means 2 available choices but only 1 point, as intended.

### Previously approved Fire Mage combat model retained

- 0..100 Heat resource and global Mana; ordinary successful Fire cast generates **10 + purchased OverheatMasteryRank** Heat (rank0..15), full 100 empowers next eligible spell, consuming all100; that empowered cast generates NO Heat. Idle10s then -10 Heat/sec.
- Separate permanent **15m** resistance aura, including caster: per Infernal Presence rank **+0.5 percentage points allied Fire resistance and -0.5pp enemy Fire resistance**, strongest same-name source applies and no stacking.
- **TierIII has FOUR actives** (Fireball, Flame Ward, Ignite, Living Bomb), one passive (Overheat Mastery) and one aura (Infernal Presence). Fire LAZOR and TierIV name **Pyromancer** reserved for later, not a fifth Fire Mage active.
- Other empowered effects distinct: Ward extra INT-scaled Fire shield, Ignite 3x 8s single-target burn, Living Bomb increased primary explosion and full-normal one-hop spread, Fireball burning ground.
- Shared global Mana max = ClassBaseMana+5*INT, natural regen=4+0.1*SPR, with class base/stats/gear accounting, no special Fire Mage per-INT coefficient.

### Next numerical design questions / design work (UNAPPROVED)

1. **Burning ground:** radius, duration/ticks, and raw magic damage per tick flat + INT×rank coefficient. Should it stay anchored to original impact location? Strongly recommend fixed ground position.
2. **Living Bomb:** normal fuse (seconds), base blast radius, ordinary/empowered raw damage coefficients; reapplication and same-target bomb collision; no exponential spread.
3. **Flame Ward:** rank1..25 redirect curve with safe ceiling, Mana per absorbed-HP point, self-only vs ally casting, empowered extra shield absorb using INT and its duration.
4. **Normal Ignite:** base flat + INT scaling per tick, refresh behavior (same-caster normal vs empowered). Tick every1sec over8sec, first tick timing.
5. **Skill-point gate audit:** construct mathematical per-rank eligibility curves consistent with six first unlocks and caps, guarantee enough eligible ranks at EVERY level1..150 for one spendable point per earned class level, and user signoff prior to calling gate sequence approved. Then choose stats, cooldowns, mana costs, teacher/quest.

**Documentation only. No Unity gameplay code/assets/build/implementation changed.**


## Fire Mage Tier III Round 8 — 2026-11-26 USER APPROVALS: BURNING GROUND, BOMB FUSE, WARD ENDPOINTS, IGNITE TICKS

**LATEST USER-APPROVED ANSWERS** to Round8, superseding the earlier larger/smaller timing alternatives. These are newly agreed anchors for the first-playtest design, not evidence of Unity implementation.

1. **Empowered Fireball burning ground:** On hit, create a **FIXED WORLD-POSITION 4-metre radius Fire ground hazard**, persisting **6 seconds**, applying **one Fire-damage tick per second** to eligible enemies in its area. Follow the originally approved placement *under the target hit*, at the impact location; the ground does not follow target movement. Tick start timing (immediate vs t=1s), unique-field overlapping/stacking, exact flat + INT × skill-rank per-tick formula, and target evaluation remain UNAPPROVED. The empowered Fireball does not gain an additional instant splash explosion in lieu of the ground hazard.
2. **Living Bomb standard fuse and radius:** **3-second fuse**, **3m radial explosion**. The empowered original explosion has *stronger damage* and creates one-hop propagated Living Bomb copies on other valid targets INSIDE 3m; each copied bomb has the **same normal 3s fuse and full normal damage** (not empowered), and the copies **never re-propagate**. Exact raw flat + INT damage coefficient, enhanced detonation multiplier, per-owner same-target overwrite and duplicate propagation suppression remain UNAPPROVED. No heat gain from bomb detonation/proc.
3. **Flame Ward standard Mana redirection endpoints:** at skill rank **1 redirect 30%** of applicable incoming post-mitigation damage from HP to spent Mana; by maximum skill rank **25 redirect 65%**. Percentage-based Mana absorption from TierII Math Barrier is retained. **The endpoint percentages are USER APPROVED, but the exact interpolation between ranks1..25 is NOT separately approved.** Suggested designer-only linear first-playtest curve `redirectFraction(r) = 0.30 + 0.35*(r-1)/24` with r1=0.30, r25=0.65; no extrapolation beyond cap. Mana consumed per prevented HP, duration and ordering relative to other shield absorption remain undecided. At zero Mana, use shared Mana Barrier semantics with no HP absorption; final resource ordering should follow tested engine behavior.
4. **Empowered Flame Ward extra Fire shield:** user **requires its capacity to scale with BOTH purchased Flame Ward rank AND caster INT**, beyond ordinary Mana redirection. Its exact strength expression, whether it adds a flat base, lifespan, absorption order, Fire-specific versus universal damage absorption, and refresh/stack policy remain **to be designed**. No unranked `50+2*INT` formula approved.
5. **Ignite tick cadence:** first Fire damage tick at **t=1s** after successful application, and then once every second for **EIGHT total ticks over exactly 8 seconds** (t1,2,3,4,5,6,7,8s). Repeat Ignite from same source **refreshes** the DoT rather than adding stacked copies, as already approved. Empowered version **3× normal Ignite's full single-target damage over identical 8s / eight-tick schedule**. Exact reapplication timer restart and normal-versus-empowered potency replacement, tick formula, Mana cost and cooldown remain open.

### Previously approved core conditions still apply

TierIII Fire Mage: Fireball35 / Flame Ward25 / Ignite30 / Living Bomb30 / Overheat Mastery15 / Infernal Presence15, **total150 bought skill ranks**, one earned skill point/class level1..150, rank0 starters. Unlock first rank Fireball Lv1 / Overheat Mastery Lv1 / Ignite Lv3 / Flame Ward Lv5 / Living Bomb Lv8 / Infernal Presence Lv10. Other ranks' exact gating still need full audit and user approval.

Overheat: max100; ordinary successfully cast Fire spell awards `10+OverheatMasteryPurchasedRank` Heat (range10..25). At100 Heat, the NEXT eligible Fire cast is uniquely empowered, consumes all100, awards zero replacement Heat. After10s with no Fire spell casting Heat falls10/sec. Passive remains separate from permanent radius15m Fire resistance aura; aura +0.5 percentage points allied Fire resistance/rank and -0.5pp hostile Fire resistance/rank, includes caster, strongest copy only.

Normal Flame Ward percentage based Mana redirect and empowered extra Fire shield; empowered Ignite 3x single target; empowered Fireball ground field; empowered Living Bomb stronger first explosion+one-hop normal copied bombs. Reserve long-windup giant Fire LAZOR for future TierIV Pyromancer; not a TierIII spell. Shared Mana max `ClassBaseMana+5*INT`, regen `4+0.1*SPR` without special Fire Mage per-INT coefficient.

### Next design / numerical round proposals (NOT YET USER APPROVED)

1. Confirm/adjust linear Flame Ward redirection `0.30+0.35*(r-1)/24`. Propose a separate Mana-per-HP ratio curve consistent with safe TierII Math Barrier behavior, and a Fire shield capacity formula that **scales with INT and skill rank** plus lifespan. Require exact signoff.
2. Specify raw Fireball direct attack and empowered ground damage `flat(r)+coefficient(r)*INT` with **ALL offensive flats and INT coefficients scaling with the purchased skill rank** (consistent class design intent). Determine 6 field ticks with first at1s as a candidate.
3. Specify Living Bomb normal and empowered initial explosion formulas; the copied normal-fuse/normal-damage bombs must be free of recursive spread and duplicate-target infinite refresh loops.
4. Specify Ignite raw eight-tick total/each tick scaling and reapplication semantics; empowered triple normal damage over identical timing.
5. Generate mathematical gate schedules with approved first-unlock levels/rank caps; audit that every earned point at class levels1..150 has an eligible rank to purchase, no rank gates unreachable, and caps total150. Submit exact schedule for user's approval before treating it as canon. Then class starting stats, Mana costs, cooldowns, advancement quest/mentor.

**Only design documentation updated; no Unity code/assets/tests/build or game implementation performed.**


## Fire Mage Tier III Round 9 — 2026-11-26 USER-APPROVED WARD CURVE, INT SHIELD, 10s EXTRA SHIELD, FIREBALL FIELD TIMING + FUTURE CLASS IDEA BANK

**AUTHORITATIVE USER APPROVALS:**

1. **Flame Ward rank scaling EXACT LINEAR curve now APPROVED**: for purchased Flame Ward rank `r=1..25`, `RedirectFraction(r)=0.30+0.35*(r-1)/24`. At r1 redirect30%, at r25 redirect65%, clamp r to valid ranks. Retains percentage-based damage redirection from incoming HP loss to consuming available Mana; partial Mana safely reduces only affordable prevented damage, and at zero Mana no HP prevention. This is not a direct heal or independent flat Mana absorption pool.
2. **Mana efficiency choice:** user says **"Scale existing Math Barrier efficiency to TierIII"**, thereby approving the *rank-improving efficiency model* rather than a new constant1 Mana/HP model or unrelated curve. Actual TierII source `Assets/_DiceFree/Scripts/Progression/MageProgressionRules.cs` has `ManaPerDamage(rank)=2-0.1*rank` for Math Barrier rank1..5 (1.9 at rank1, 1.5 at rank5). **Do not directly extend `2-0.1*r` to rank25**: that becomes negative and would violate spend/absorb rules. Exact TierIII 25-rank interpolation/target efficiency is **STILL OPEN**; future designer candidate can preserve starting1.9 and endpoint1.5 across25 ranks, or choose a different endpoint with explicit user agreement. Treat the shared Math Barrier implementation as reference rather than accidental accepted TierIII numeric formula.
3. **Empowered Flame Ward EXTRA elemental Fire shield EXACT formula USER APPROVED:** for Flame Ward purchased rank `r=1..25` and current caster `INT`, `AdditionalFireShieldCapacity = 50 + (0.5 + 0.1*r)*INT`. This **scales with both INT and purchased Ward rank**, and is an ADDITIONAL Fire shield on an empowered Flame Ward cast; normal percentage-based Mana redirection remains. Example INT500 gives rank1=350, rank10=800, rank25=1550 absorb points. Existing element-specific shield handling, mitigation ordering, target rules and stacking/refresh policy still require integration design.
4. **Empowered Flame Ward additional Fire shield lifetime USER APPROVED: TEN (10) seconds**. This is the **ADDITIONAL Fire shield duration**, **NOT automatically the Mana-redirection parent buff duration or ability cooldown**; main Ward duration, cooldown and spend cost still need user approval.
5. **Empowered Fireball burning ground ticking USER APPROVED:** stationary ground at impacted target, **4m radius**, **6s duration**, Fire-damage ticks at **t=1,2,3,4,5,6 seconds after the ground is created**. Six total delayed ticks; no immediate t=0 tick. Exact damage/INT coefficient, area overlap/stacking rules and normal Fireball raw magic damage remain pending.

### Existing Fire Mage TierIII approved design retained

- Exactly SIX abilities: active Fireball35, Flame Ward25, Ignite30, Living Bomb30; passive Overheat Mastery15; aura Infernal Presence15, caps total150, one earned class point/level1..150, all rank0 initially. First unlocks Fireball Lv1, Overheat Mastery Lv1, Ignite Lv3, Flame Ward Lv5, Living Bomb Lv8, Infernal Presence Lv10; rank2+ gate audit not yet approved.
- Overheat ordinary successful eligible Fire cast adds `10+OverheatMasteryRank` Heat (rank0..15), cap100. At100, next eligible cast is uniquely empowered, spends all100 with NO same-cast Heat gain. 10s without Fire casts then gradual -10 Heat/sec.
- Permanent radius15m Infernal Presence aura includes caster: +0.5 percentage points allied Fire resistance per rank and -0.5pp hostile Fire resistance per rank; strongest same-name copy only, no stacking.
- Ignite 8s duration, ticks at t1..8, refreshes not stacks, empowered Ignite has 3x normal total DoT at same 8s single-target.
- Living Bomb 3s fuse, 3m blast. Empowered initial explosion stronger and applies one generation of NORMAL full damage, NORMAL 3s fuse Living Bomb copies to other nearby valid targets within3m. Propagated copies never spawn further bombs.
- Empowered Fireball leaves fixed-position ground at hit target, and does not gain an unapproved immediate splash detonation.
- Preserve shared global Mana rules `MaxMana=ClassBaseMana+5*INT`, `ManaRegenPerSecond=4+0.1*SPR`; do not create Fire Mage-only mana multipliers.

### USER REQUESTED FUTURE CLASS IDEA BANK — write down faithfully, DO NOT implement now

The user says "**just a thought that i want written down**" and requests:
- **Tier IV PYROMANCER** (future proposed Fire Mage advancement) could **COMBINE TierIII Infernal Presence aura + Overheat Mastery passive into ONE rankable skill**, to leave space for **the Fire LAZOR**, and possibly **a METEOR spell**. Exact merged mechanics, retained abilities, budget/rank schedule and Meteor are **future ideas**, not approved ready-to-build TierIV skill specification. The giant Fire LAZOR was previously explicitly deferred from TierIII; do not reinsert it in current Fire Mage.
- **Tier III FROST MAGE** should be designed *later* as an alternative Mage evolution, leading to **Tier IV CRYOMANCER**. This is a future additional Mage branch, not part of the present **first-TierIII-per-each-parent** design sprint. Not user request to immediately design second TierIII Mage class.
- **HIDDEN Tier IV HYDROMANCER** future secret class concept: **unlock prerequisite is having BOTH Fire Mage AND Frost Mage manifestations at LEVEL 120**. The user suggests a secret cross-element TierIV class. Whether both manifests must be on the same character, how to unlock it, special quest, progression rules, elemental abilities or class naming in runtime are NOT YET DESIGNED, and the user framed this as an idea to record rather than immediate implementation authorization. Explicit prerequisite **level120 Fire Mage + level120 Frost Mage** is the proposed unlock gating; do NOT silently alter to Pyromancer/Cryomancer levels or require either TierIV to unlock.

**Do not misrepresent these future brainstorming notes as implemented abilities or as approval to interrupt the existing TierIII coverage priority.** Keep them available for future evolution-roadmap planning.

### Next numerical design tasks / questions (NOT YET USER APPROVED)

1. Decide the exact **Flame Ward Mana-spend ratio across r1..25** based on parent `ManaPerDamage(r)=2-0.1*r` at r1..5; designer candidate `1.9-(0.4*(r-1)/24)` to preserve parent efficiency endpoints 1.9→1.5 or alternative user-defined end efficiency. Main Ward duration and cooldown still open.
2. Design rank-scaling flat and INT damage coefficients for **Fireball normal projectile** and **empowered burning ground per tick** (all coefficients grow with purchased skill rank).
3. Design **Ignite** eight tick raw Fire damage `flat(r)+INTCoeff(r)*INT` and **Living Bomb** normal/empowered single detonation raw Magic Fire damage, empowered multiplier; bomb copies retain normal damage, no recursive spread.
4. Build exact **150-level rank gate schedule** with first unlock levels and approved cap counts, computationally verify spendable rank per earned point for each class level1..150; seek user signoff.
5. Determine TierIII base INT/VIT/SPR stats and growth, default Mana base, all active Mana costs/cooldowns/cast animations, teacher/quest unlock.

**Documentation-only round. Do not edit Unity gameplay, assets or run builds without explicit authorization.**


## Fire Mage Tier III Round 10 — 2026-11-26: USER DIRECTION ON WARD EFFICIENCY AND TIMING, BOMB DOUBLE DAMAGE, OFFENSIVE RANK SCALING

**LATEST USER ANSWERS** to Round10 are authoritative, but several answers leave exact curves and the intended meaning of "7 sec" open. Do NOT infer explicit signoff on numeric values not actually approved.

1. **Flame Ward Mana efficiency improves MORE than the parent Mage**: user says "i think we should improve it a bit from the parent mage, maybe like **1.3 at rank 25**?" This is the user-selected first-test **target of 1.3 Mana per 1 HP damage prevented at Flame Ward purchased rank25**, phrased tentatively ("maybe"). Retain improving efficiency through rank1..25, modeled on parent Math Barrier. Parent verified formula `2-0.1*r` with 5-rank range r1=1.9, r5=1.5. **DESIGNER PROPOSAL, NOT YET FULL FORMULA SIGNOFF:** reuse same starting efficiency 1.9 at r1 then smoothly interpolate to suggested 1.3 at r25 as `ManaPerPreventedHP(r)=1.9-0.6*(r-1)/24`; this is safe, always positive and more efficient than the parent max1.5. Future review should confirm both the 1.9 start and exact interpolation, as well as the 1.3 endpoint if not yet final. In all cases partial Mana must absorb only an affordable fraction, and zero Mana cannot absorb HP.
2. **Flame Ward's primary Mana-redirect ward duration scales with purchased rank and reaches 10 seconds at MAX rank25**, and Flame Ward **cooldown =20 seconds** (user: "10 second at max rank with 20 sec cd sounds good"). **Ward main duration at rank1 and interpolation not specified**, so no flat10s at all ranks. Previously empowered EXTRA Fire-element shield separately has approved **10-second duration** and `50+(0.5+0.1*r)*INT` capacity. Do NOT conflate main duration with extra shield duration.
3. **Fireball is a fairly repeatable but not overly fast offensive spell** (user responds "yes, but not too quick, like maybe 7 sec at max rank?" to a question describing a "relatively quick, repeatable spell with short cast time"). **AMBIGUOUS whether 7 sec refers to cooldown or CAST TIME.** The contextual first-test assumption to offer is **7-second COOLDOWN at max Fireball rank35**, with its windup/cast-time separate and unapproved, since a 7-second cast would conflict with repeatable main spell and 10-second Overheat activity grace; DO NOT mark 7s cooldown as definitively approved until user confirms. Neither cast time nor cooldown lower-rank behavior is defined.
4. **Empowered Living Bomb detonation damage multiplier APPROVED = 2× the NORMAL explosion damage of the original empowered bomb** at the same rank and caster. Upon original detonation, propagate normal Living Bomb effects to other valid enemies inside3m radius, copies inherit normal full damage, normal3s fuse and have **ZERO further propagation hops**. Empowered explosion multiplier applies ONLY to original empowered explosion; propagated copies NEVER inherit it. No Heat from explosions, only separately successful Fire casts. Exact normal base+INT damage function still OPEN.
5. **Every damaging Fire Mage spell/packet's FLAT RAW DAMAGE and INT SCALING COEFFICIENT should BOTH grow with its OWN purchased skill rank**, including Fireball primary impact + empowered burning ground ticks, Ignite ticks, and Living Bomb normal/explosive hit. This follows user answer "yes" to Round10 question. Do not make damage scale with rank only through one side or use unranked static INT coefficient. User approved principle, not any particular coefficient numbers.

### Current locked anchors, for reference

- Fire Mage TierIII six abilities caps **Fireball35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15** =150 bought ranks, rank0 starter, one point/level1..150; first unlocks Fireball1, Overheat Mastery1, Ignite3, Flame Ward5, Living Bomb8, Infernal Presence10. Exact later rank eligibility gates still unapproved.
- Flame Ward normal redirect ratio `0.30+0.35*(r-1)/24` rank1..25 approved. Empowered EXTRA Fire-element shield capacity `50+(0.5+0.1*r)*INT`, 10sec lifetime, stacks with Mana redirection. Main Ward now duration max10s and CD20s user-selected; rank curve open.
- Overheat 0..100, ordinary successfully completed eligible Fire cast awards `10+OverheatMasteryRank` Heat (0..15 ranks); at100 next eligible Fire spell empowered consumes100 and generates none on that cast; after10s without eligible casts decays10 Heat/sec. Extra TierIV Pyromancer / Frost Mage→Cryomancer / hidden Hydromancer at BOTH Mage evolutions Lv120 remain future ideas only.
- Empowered Ignite =3x ordinary eight-tick single-target Fire DoT over8sec, tick times1..8sec; same-source recasts refresh not stack.
- Empowered Fireball leaves 4m-radius fixed-position burning ground under impacted target,6sec duration, tick1..6sec, separate from base Fireball projectile damage.
- Living Bomb normal3sec fuse and3m blast; empowered original2x normal damage, one-hop spread to NORMAL copies.
- Infernal Presence permanent15m radius ally Fire resist +0.5pp/rank, enemy Fire resist -0.5pp/rank, includes caster, strongest identical aura wins; Fire LAZOR reserved TierIV Pyromancer.

### Proposed Round11 actions/questions (NOT USER APPROVED YET)

1. Confirm whether **Fireball's "7 seconds at max rank" means cooldown rather than cast/windup**. Propose Fireball rank35 cooldown7s while cast time around1.0–1.5s, separately approved.
2. Confirm exact **Flame Ward Mana efficiency curve** rank1=1.9 to rank25=1.3 linear, or supply alternate rank1 start/curve.
3. Choose **Flame Ward MAIN duration** at rank1 and intermediate rank scale, ending at approved10s at rank25 (e.g. rank1=4s, rank25=10s, linear); cooldown20s.
4. Design complete initial raw spell formula values `flatBase+flatPerRank*(r-1)+(intBase+intPerRank*(r-1))*INT` for Fireball impact, ground tick, Ignite tick, Living Bomb original and normal copies, empowered multiplier, crit defaults and Mana costs; ask for numerical approval.
5. Generate full mathematical 150-rank gate schedule and audit with user signoff, consistent with first unlocks and rank caps. Do not silently implement source changes.

**Docs-only. No Unity gameplay files/assets/build/test changed.**


## Fire Mage Tier III Round 11 — 2026-11-26 USER APPROVES FIREBALL 0.2s CAST AND 7s CD, LINEAR WARD EFFICIENCY/DURATION, FUTURE PYROMANCER TOGGLE WARD

**USER ANSWERED all five Round11 questions; the free-text addition for Q4 is authoritative and supersedes the form's "other seconds" placeholder.** All values here refer to DESIGN, not existing Unity implementation.

### USER-APPROVED Flame Ward (main Mana redirection)

- **Mana efficiency explicitly approved EXACT LINEAR** from `1.9 Mana per 1 HP damage prevented at purchased Flame Ward rank1` to `1.3 Mana per HP prevented at rank25`. Formula for valid `r=1..25`: `ManaPerPreventedHP(r)=1.9 - 0.6*(r-1)/24`. This supersedes Round10's tentative efficiency target and earlier unapproved parent-preserving endpoint1.5. Rank25 is meaningfully more efficient than TierII Mage Math Barrier rank5=1.5; always positive, never extend beyond valid rank range. Actual prevented HP damage is constrained by both approved fraction `RedirectFraction(r)=0.30+0.35*(r-1)/24` AND current available Mana divided by `ManaPerPreventedHP(r)` (partial redirection allowed); pay Mana corresponding to actual prevented HP.
- **Normal/main Flame Ward duration explicitly approved EXACT LINEAR** from `4 seconds at r1` to `10 seconds at r25`: `WardMainDurationSeconds(r)=4 + 6*(r-1)/24`, r=1..25. This resolves previous question about intermediate ranks. **Flame Ward cooldown fixed at20 seconds**, previously accepted Round10, remains approved.
- **Empowered EXTRA elemental Fire shield** remains separate: `AdditionalFireShieldCapacity(r,INT)=50+(0.5+0.1*r)*INT`, lifespan **10 seconds**. Do not confuse its fixed10s lifespan with main rank-scaling ward lifespan or ability cooldown. The exact handling/priority of Fire shield damage-type absorption, self-only vs allied targeting, refresh/stack interaction and cooldown-start semantics still require engine rules/design confirmation.
- Sample rank table (derived): r1 efficiency1.9 duration4s; r5 efficiency1.8 duration5s; r10 efficiency1.675 duration6.25s; r15 efficiency1.55 duration7.5s; r20 efficiency1.425 duration8.75s; r25 efficiency1.3 duration10s.

### USER-APPROVED Fireball casting/cooldown

- **Fireball cast time = 0.2 SECONDS** (user typed "0.2 sec cast time fore fireball" in additional notes). This intentionally overrides all prior designer suggestions of 1–2 seconds or a seven-second cast. It is short but a cast, not necessarily instant; interrupt/animation/movement restrictions must follow shared casting architecture.
- **Fireball cooldown = 7 SECONDS AT MAXIMUM purchased Fireball rank35**. This is explicitly a COOLDOWN, not a cast time, now user-confirmed. The cooldown at ranks1..34 and its interpolation are still OPEN; do not imply a fixed seven seconds at all ranks unless the user approves that separately.
- Base Fireball direct projectile; empowered variant leaves fixed 4m-radius/6s ground fire under target hit, ticks at t1..6s, and consumes100 Heat with no same-cast gain.
- User endorsed Fireball as repeatable primary Fire attack, while delayed Living Bomb provides AoE burst. All flat magical damage and INT coefficients scale with purchased damaging skill rank; the exact formulas, costs, and other skill cooldowns are still TBD.

### Future Tier IV Pyromancer user request (NOTE / DESIGN DIRECTION, NOT IMPLEMENT NOW)

- User explicitly answers: "**Record toggle-style Mana Ward evolution as future design direction**" for TierIV **Pyromancer**. When designing Pyromancer later, evolve Fire Mage's rank-scaled timed Mana Ward into a **toggle-style Mana damage-absorption ability**, subject to future rules for activation, resource depletion, efficiency, cooldown, UI and balancing. This is **not** authorization to change Fire Mage's TierIII timed 4–10s ward into a toggle.
- Previously recorded future Pyromancer possibilities persist: combine Heat-generation passive and dual resistance aura into one skill, add giant windup Fire LAZOR, maybe Meteor. Separate Frost Mage→Cryomancer later and hidden Hydromancer requiring Fire Mage Lv120 AND Frost Mage Lv120 remain future ideas; do not create new TierIII branch or implement until coverage priority is met.

### Existing Fire Mage TierIII locked anchors (unchanged)

- Exactly six skills, total150 purchased ranks: Fireball35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15. Fireball+Heat passive first unlock Lv1, Ignite Lv3, Ward Lv5, Bomb Lv8, aura Lv10. One purchased point/class level1..150, rank0 starters. Exact rank2+ level gates still not audited/approved.
- Normal successful Fire cast Heat gain `10+OverheatMasteryRank`; at100 NEXT eligible cast is empowered, spends100 and generates zero new Heat; after10 seconds inactive decay10 Heat/s.
- Infernal Presence passive always-on radius15m, allied Fire resistance +0.5 percentage points/rank incl caster, enemy Fire resistance -0.5pp/rank, strongest identical aura only.
- Ignite eight ticks over8s at t1..8, same caster reapply refresh/no stacking, empowered does3x normal DoT total at same duration on single target.
- Living Bomb normal fuse3s, radius3m; empowered primary detonation2x normal damage, spreads normal 3s-fuse/full-normal-damage bombs one hop only, never recursively.
- Flame Ward normal Mana damage redirection percent0.30+0.35*(r-1)/24; extra empowered shield as above. No Fire LAZOR in TierIII, future TierIV Pyromancer only.
- One global Mana formula: `MaxMana=ClassBaseMana+5*INT`, natural regen `4+0.1*SPR`. Fire Mage class resource differences via class base/start INT/SPR/gear, no special class-specific mana coefficient.

### Proposed Round 12 priorities (UNAPPROVED)

1. **Fireball rank1 cooldown and curve** to rank35=7s: e.g. r1=10s improving linearly to7s, or fixed7s for all ranks. Its cast time stays approved0.2s.
2. **Spell damage flat+INT rank coefficients** for normal Fireball hit, empowered Fireball ground tick, Ignite per tick, Living Bomb normal detonation, plus actual natural Fire resistance mitigation, all increasing with own rank. Build sensible relative power per spell time/cooldown to avoid runaway AoE.
3. **Mana costs** for Fireball, Flame Ward, Ignite, Living Bomb using universal pool, and rank growth as agreed overall skill system. Explicitly check whether Ward cast cost plus spent-mana redirection should double charge and whether empowered cast shares ordinary Mana cost.
4. **Other cooldowns, cast times, movement/target legality** for Ignite and Living Bomb; Ward casting rules and cooldown start upon cast finishing.
5. **150-level purchase eligibility audit** with actual first-unlock levels and cap split; verify every class level has >=one eligible future rank and no orphan points; show exact gate plan for user approval before gameplay implementation.

**Documentation-only; NO Unity source/assets/build/tests/implementation modified.**


## Fire Mage Tier III Round 12 — 2026-11-26 USER APPROVES FIREBALL 14s→7s CD, IGNITE AND LIVING BOMB COOLDOWNS/CAST TIMES

**LATEST USER ANSWERS, authoritative.** Q1 free-text note overrides the "Different curve" placeholder, supplying the precise new Fireball cooldown start and interpolation.

### Approved first-playtest cast/cooldown mechanics

| Active spell | Purchased ranks | Cast time | Cooldown | User approval |
| --- | --- | --- | --- | --- |
| **Fireball** | r1..35 | **0.2 seconds** (previous approved) | **LINEAR 14s at r1 DOWN TO 7s at r35** | User explicit Round12 note: "14 seconds at rank 1 scaling linearly down to 7 seconds" |
| **Ignite** | r1..30 | **INSTANT** | **10 seconds**, no rank-scaling CD requested | User explicit Round12 |
| **Living Bomb** | r1..30 | **0.2 seconds** | **15 seconds**, no rank-scaling CD requested | User explicit Round12 |
| **Flame Ward** | r1..25 | **NOT YET DECIDED** | **20 seconds** (previously approved), no rank CD scaling requested | Prior round |

**Exact approved Fireball cooldown formula:** `FireballCooldownSeconds(r) = 14 - 7*(r-1)/34` with purchased rank `r=1..35` (clamp to valid rank range). Examples r1=14s, r5≈13.1765s, r10≈12.1471s, r15≈11.1176s, r20≈10.0882s, r25≈9.0588s, r30≈8.0294s, r35=7s. Do not conflate this 7 seconds with its already approved **0.2-second cast time**. Cooldown start should follow agreed global class cooldown timing after a cast finishes, not before beginning the cast.

**Ignite** is an **instant cast** Fire DoT application; its eight DoT ticks still occur at 1,2,...,8 seconds after a successful application, and same-caster reapplication refreshes rather than stacking, empowered3x normal total for unchanged duration. Instant spell can still incur cooldown/GCD/animation rules from global architecture, unapproved independently.

**Living Bomb** takes a **0.2-second cast**, with a separate **3-second fuse AFTER successful application** (do not confuse with cast time) and **15-second cooldown**. Normal detonation radius3m. Empowered original explosion2x normal damage and one-hop spread of normal bombs inside3m; copied bombs use the normal3-second fuse and normal full damage, may explode but cannot spread or grant new Heat.

No Fire Mage damage formulas/Mana costs/other cast times are accepted in this round.

### Previous Fire Mage approvals retained

- Six abilities + 150 rank split: Fireball35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15; 1 point per class level1..150; all initially purchased rank0. Initial first skill unlocks Fireball+Overheat Mastery Lv1; Ignite Lv3; Flame Ward Lv5; Living Bomb Lv8; aura Lv10. Rank 2+ eligibility gates and audited point availability still open.
- Flame Ward fraction redirected to Mana `0.30+0.35*(r-1)/24`, Mana cost per prevented HP `1.9-0.6*(r-1)/24`, main effect lifetime `4+6*(r-1)/24` seconds, cooldown20 seconds. Extra empowered Fire shield capacity `50+(0.5+0.1*r)*INT`, life10 seconds. Cast-time, mana upfront cost, targeting and damage absorption ordering open.
- Fireball empowered ground burn4m radius,6s fixed under hit target, damage ticks at t1..6sec. No immediate splash explosion assumed.
- Normal eligible Fire spell cast Heat `10+OverheatMasteryPurchasedRank`, cap100; next eligible spell after reaching100 empowered, consumes100 and self-awards zero Heat. Decay after10 seconds of Fire cast inactivity then -10 Heat/sec. Note 14-second rank1 Fireball cooldown means other successful Fire casts or idle Heat decay may matter before Fireball rank investment.
- Aura permanently15m, includes caster and allied +0.5pp Fire resistance/rank, enemy -0.5pp Fire resistance/rank, strongest identical aura only.
- Universal Mana `ClassBaseMana+5*INT`, natural regen `4+0.1*SPR` for ALL Mana users; Fire Mage adjustments through class base/stats/gear, not custom coefficients.
- Future-only Pyromancer TierIV toggle-Mana Ward, combined aura+passive, Fire LAZOR and possible Meteor; Frost Mage→Cryomancer later; hidden Hydromancer TierIV concept after both Fire Mage and Frost Mage Lv120. Remain unimplemented future plans, not current TierIII skills.

### Round13 follow-up design priorities — NOT YET APPROVED

1. Confirm Fireball raw Fire magical hit damage `flatBase + flatPerRank*(r-1) + (intBase+intPerRank*(r-1))*INT`, with all rank coefficients increasing. Choose scale against Mage TierII Ember Bolt and existing monster HP/damage expectations rather than arbitrary damage explosion.
2. Set separate empowered Fireball burning ground per-tick damage formula, increasing flat and INT coefficient with Fireball rank (6 ticks within4m).
3. Set Ignite per-tick damage (8 ticks/8s, empowered3x) and Living Bomb normal explosion formula (empowered original2x; secondary normal).
4. Decide **Mana costs of ALL FOUR active spells** and Fireball/Flame Ward upfront mana cost policy; ensure Ward redirection additionally consumes Mana during absorption, no unclear double charge. Shared global Mana max/regen must remain.
5. Determine **Flame Ward cast time**, other target restrictions, rank-gate audit for 150 points, and after numerical combat tuning the Fire Mage base stats/quest.

**Documentation only. NO Unity gameplay code, assets, builds or runtime implementation touched.**


## Fire Mage Tier III Round 13 — 2026-11-26 USER APPROVES OFFENSIVE DAMAGE RATIOS, INSTANT WARD; SUGGESTS RANK-REDUCED COOLDOWNS

**LATEST USER ANSWERS:** User accepts comparative damage profiles at intended first-playtest relative strength; supplied free-text asks for OTHER spell cooldowns to decrease with purchased rank, but does NOT specify exact cooldown endpoints/curves. Distinguish confirmed relative targets from proposed final coefficients, and speculative cooldown changes from actually adopted new timings.

### USER-APPROVED qualitative/ratiometric damage profiles

1. **Normal Fireball direct hit** should have **balanced flat raw damage and INT multiplier** (not overwhelmingly INT-biased or flat-biased). At every purchased Fireball rank r1..35 both its flat raw damage and coefficient multiplying caster INT must increase with r. Exact numeric base damage and rank steps are NOT APPROVED.
2. **Empowered Fireball burning-ground per tick** is approximately **25% of a normal Fireball direct hit** as a damage-balance target. Six ticks at t1..6 seconds, 4m radius fixed-location fire at the impacted enemy,6s field duration, so a unit standing in the whole field suffers approximately 1.5x normal Fireball direct hit worth of EXTRA ground DoT (plus the normal initial Fireball direct hit). The ground tick's own flat component AND INT coefficient MUST grow with Fireball rank; preserve the ratio as a comparative target rather than hard-coupling it to a Fireball damage value at a different rank. Exact final packet formula remains pending.
3. **Normal Ignite 8-second full DoT** has target approximately **2x a normal Fireball direct hit** in total. Its eight ticks at t1..8s therefore average around **25% of the reference normal Fireball hit per tick**. Ignite has its OWN skill rank r1..30, which drives increasing flat/INT scaling; Fireball has cap35, so ratios require a reference comparison point (similar relative rank investment, playtest DPS or aligned class level), NOT equating unrelated ranks blindly. Empowered Ignite retains 3x its OWN normal total damage against a SINGLE enemy at unchanged8s duration/8 ticks, thus ≈6x reference Fireball hit for full DoT, if ratio alignment holds. Recast refreshes, no stacking.
4. **Normal Living Bomb initial detonation** targets approximately **1.5x a normal Fireball hit PER enemy in the 3m radius**. Its own purchased rank r1..30 increases BOTH raw flat explosion damage and coefficient multiplying INT. At100 Heat the empowered ORIGINAL explosion is exactly **2x that normal Living Bomb hit** and spreads normal-damage normal3s-fuse copies to other eligible targets in 3m for ONE generation only. The spread copies are normal-strength bombs and NEVER re-spread, no Heat generated from procs.
5. **Flame Ward is an INSTANT CAST**. Retains approved 20-second cooldown until explicitly revised, main Mana redirection (30% r1 to65% r25), efficiency (1.9 Mana/HP r1 to1.3 r25), main duration (4s→10s), and empowered additional INT-and-rank Fire shield `50+(0.5+0.1*r)*INT` for10s. Zero cast windup, but still successful Fire spell cast / cooldown rules apply as globally relevant; whether Ward activates Heat and can consume Overheat was previously assumed per every successfully cast Fire spell, no special exceptions user-specified.
6. **Additional USER REQUEST, not exact numeric approval:** "i think maybe the cooldowns of the other skill should decrease with rank as well". Record STRONG DESIGN DIRECTION to give **Ignite, Living Bomb, and Flame Ward lower cooldowns at higher purchased ranks**, analogous to Fireball's explicitly approved linear14s→7s rank1..35 curve. **Exact endpoints and formulas NOT yet decided**, and existing user-selected Ignite10s, Living Bomb15s, Flame Ward20s should remain the baseline reference values pending clarification whether these are lower endpoints or another rank's values. No changes to existing accepted cooldown numbers are automatically enacted.

### Designer-only next-round cooldown alternatives (NOT user approved)

A conservative proposed set is:
- **Ignite** rank1=14 seconds, max rank30=10 seconds: `CD(r)=14-4*(r-1)/29`.
- **Living Bomb** rank1=22 seconds, max rank30=15 seconds: `CD(r)=22-7*(r-1)/29`.
- **Flame Ward** rank1=28 seconds, max rank25=20 seconds: `CD(r)=28-8*(r-1)/24`.
- **Fireball is ALREADY approved** rank1=14 seconds to rank35=7 seconds: `CD(r)=14-7*(r-1)/34`, cast0.2s.
- If the user instead intended 10/15/20s as rank1 CDs, propose alternate lower rank-cap values separately; DO NOT quietly overwrite them.
- Having all four active spells on cooldown at very low skill ranks may create long downtime with Heat decay after10s idle; balance time to reach100 Heat against casts and available Mana after selecting CD curves. The aura and passive have no ordinary skill-trigger cooldown.

### Compact current state

Fire Mage first TierIII coverage path with four actives and two nonactive skills: Fireball cap35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15; 150 purchased skill ranks total and 1 earned point/class level1..150. Fireball+Heat passive first unlock Lv1, Ignite3, Ward5, Bomb8, aura10; rank2+ gates not audited/approved. Heat max100, ordinary Fire cast gain=10+OverheatMasteryPurchasedRank, empowered next Fire cast spends100 with no same-cast Heat, idle10s then -10 Heat/sec. Aura passive permanent15m, +0.5pp allied Fire resistance/-0.5pp hostile Fire resistance per aura rank including caster, strongest identical aura only. Universal global Mana `ClassBaseMana+5*INT`, regen `4+0.1*SPR`. Future Pyromancer TierIV maybe merged aura+passive, Fire LAZOR, Meteor, toggle Ward; Frost Mage→Cryomancer; hidden Hydromancer at BOTH Fire Mage and Frost Mage level120 are FUTURE IDEA BANK only.

### Next design round priorities (NOT YET ANSWERED)

1. User approval of exact rank-varying **Ignite/Living Bomb/Flame Ward cooldown endpoints and linear curves**, with previously chosen 10/15/20s likely max-rank endpoints.
2. Quantify and approve **normal Fireball rank1 and max-rank flat and INT coefficient**; use Mage source and actual encounter durability before hard locking.
3. Quantify **normal Ignite per-tick** and **normal Living Bomb per-target explosion** maintaining approximately 2x and1.5x reference Fireball-hit power at comparable development, with independent own-rank scaling.
4. Quantify **Fireball empowered ground tick** ≈0.25x direct hit per tick, six ticks.
5. Specify **Mana costs for four actives**, and then the full150-level rank eligibility gate audit (no unspendable points), tierIII stat/quest and advancement source.

**DESIGN DOCS ONLY. No Unity source/assets/build/runtime implementation changed.**


## Fire Mage Tier III Round 14 — 2026-11-26 USER APPROVES ALL FOUR LINEAR COOLDOWNS, RANK-RISING MANA COSTS, WARD HEAT AND ABSORPTION ORDER

**LATEST USER APPROVALS:** User explicitly selects the proposed full linear rank-dependent cooldown schedule, all skill Mana costs rising by rank, a modest upfront Flame Ward Mana cost **PLUS** Mana consumed when preventing HP damage, Flame Ward as an eligible Fire spell for both generating and spending Overheat, and the empowered Fire shield absorbing damage BEFORE the Mana redirection process.

### Exact four active spell cooldown formulas (USER APPROVED)

For each bought skill rank r in that ability's valid range; cooldown begins upon completing its cast/activation under the shared cooldown-start convention, not the start of the cast:

| Fire Mage active | Valid purchased ranks | Cooldown at rank1 | Cooldown at max rank | EXACT accepted linear formula | Cast time |
| --- | --- | --- | --- | --- | --- |
| **Fireball** | 1..35 | 14 seconds | 7 seconds | `CD_Fireball(r)=14-7*(r-1)/34` | 0.2s |
| **Ignite** | 1..30 | 14 seconds | 10 seconds | `CD_Ignite(r)=14-4*(r-1)/29` | instant |
| **Living Bomb** | 1..30 | 22 seconds | 15 seconds | `CD_LivingBomb(r)=22-7*(r-1)/29` | 0.2s |
| **Flame Ward** | 1..25 | 28 seconds | 20 seconds | `CD_FlameWard(r)=28-8*(r-1)/24` | instant |

These supersede the formerly fixed 10/15/20 second cooldown descriptions for Ignite/Living Bomb/Flame Ward at all ranks; those values are now **maximum-rank endpoints**. Fireball's formerly approved 14→7s rule is unchanged. There is no ordinary cooldown for Overheat Mastery passive or permanent Infernal Presence aura. The user approved a rank-varying formula for each of the FOUR actives, not a global cooldown reduction shared among them.

### Mana economy (USER APPROVED PRINCIPLES; exact numbers STILL OPEN)

- **ALL FOUR active Fire Mage spells have Mana costs that INCREASE as that spell's purchased rank increases.** This includes Fireball, Ignite, Living Bomb and Flame Ward. Each spell has its own future Mana cost curve, to be designed; do not invent numeric base or per-rank increments as already approved.
- **Flame Ward incurs a SMALL upfront Mana cast cost** increasing with its own purchased rank, AND it **spends additional Mana when incoming damage is prevented**. These are two separate payments, expressly user approved (not an accidental implementation double charge).
- Normal Flame Ward's per-prevented-HP Mana price remains `1.9-0.6*(r-1)/24`, r1..25, and redirection percentage `0.30+0.35*(r-1)/24`, duration `4+6*(r-1)/24`. Partial Mana handles only affordable prevented HP. Empowered additional Fire shield capacity `50+(0.5+0.1*r)*INT`, duration10 seconds; exact "Fire shield" type-specific absorption/mitigation is not separately resolved, so preserve game-wide elemental shield contract without inventing immunity to other damage types.

### Overheat and shield absorption ordering (USER APPROVED)

- **EVERY ACTIVE FIRE SPELL including Flame Ward** qualifies as an eligible successful Fire spell cast to gain Heat: ordinary cast awards `10+OverheatMasteryPurchasedRank` Heat (cap100), subject to distinct cast success (NOT ticks, proc blasts or each hit). **Every active Fire spell including Flame Ward also can be the NEXT empowered cast** if at100 Heat, consuming100 Heat and awarding zero replacement Heat.
- **Empowered Flame Ward EXTRA Fire-element shield absorbs eligible incoming damage FIRST**, BEFORE evaluating Flame Ward's Mana redirection on the **remaining damage**. The main ward's percentage applies to the post-extra-shield remainder; excess damage not absorbed by either then reaches HP. Do NOT apply the full Mana redirect fraction to pre-shield damage, which would double-dip absorption or spend unnecessary Mana.
- Damage-order example conceptual: damage enters, extra Fire shield reduces the eligible portion, apply existing damage mitigation semantics and remaining Mana redirection as appropriate, then HP receives the residue. Exact interaction with p.def/m.def mitigation, other shields and Fire resistance must respect the engine's global incoming-damage pipeline and still needs implementation-stage validation.

### Prior user-approved class commitments retained

- Four actives + Overheat Mastery passive + Infernal Presence resistance aura, total150 skill caps **35/25/30/30/15/15**. First-unlock Fireball1/Heat passive1/Ignite3/Ward5/Bomb8/aura10; later rank gates not audited/user-approved.
- Fireball balanced flat+INT raw hit, empowered six burning-ground ticks each ≈25% direct-hit reference in4m radius6s under impacted target, ticks t1..6s.
- Ignite eight ticks t1..8s, normal total ≈2x reference Fireball direct hit, empowered three times its own normal total, same target8s; no stacking on self-reapply.
- Living Bomb normal 3s fuse, 3m radius, raw blast ≈1.5x Fireball hit per enemy, empowered original2x normal, spreads NORMAL-fuse/NORMAL-damage copies to valid enemies within3m ONCE, no further generation, procs grant no Heat.
- Heat max100, gain `10+passiveRank` per successful ordinary Fire spell, at100 next spell empowers/spends all/no self-refill, after10s without successful Fire spell decay10 Heat/sec.
- Aura permanent15m, includes caster, allied +0.5 percentage points Fire resistance/rank and hostile -0.5pp/rank, strongest same-name source wins.
- Shared global Mana formula max `ClassBaseMana+5*INT`, regeneration `4+0.1*SPR`. No Fire Mage-specific Mana coefficient.
- Future TierIV Pyromancer may merge aura/passive, evolve Flame Ward to toggle, gain Fire LAZOR / maybe Meteor; Frost Mage→Cryomancer later and hidden Hydromancer at Fire Mage Lv120 + Frost Mage Lv120 only future class ideas; not authorized gameplay additions.

### Round 15 possible decisions (DESIGNER ONLY UNTIL USER ANSWERS)

1. **Mana cost curves:** propose Fireball `10+2r`, Ignite `14+2r`, Living Bomb `22+3r`, Flame Ward `5+r` as affordable early-test candidates; each rises with rank. These values are NOT user approved; compare against actual TierIII Mana pool scaling and ability performance before locking.
2. **Damage formula**, choose actual `flat(r)+coefficient(r)*INT` for Fireball direct hit, using Mage TierII Ember Bolt as reference; then define rank-scaled ground tick, Ignite tick and Bomb blast based on approved approximate ratios at comparable progression, not a cross-skill rank mix-up.
3. **Ward Fire shield affinity:** shield absorbs all incoming damage or only Fire-element damage? User called it Fire shield and requested it absorb eligible damage before Mana redirect but the exact eligible damage types should be decided explicitly, using the already established global shield rule rather than inventing a new one.
4. **Mana resource/Overheat cost evaluation:** spells spend Mana upon successful cast; empowered versions spend their normal Mana cost as well as100 Heat; ask whether empowered effect has extra Mana surcharge or not. No surcharge approved yet.
5. **150-point rank-gate auditing:** author exact schedules consistent with six already-approved first-unlock class levels and caps, ensure every level1..150 can eventually spend all points; present as proposed gates for approval prior to code implementation.

**This is design-documentation work ONLY, no Unity gameplay source/assets/build/tests or branch merge approved.**


## Fire Mage Tier III Round 15 — 2026-11-26 USER APPROVALS: HIGHER MANA COSTS NEEDED, GLOBAL FIRE SHIELD, EMPOWERED NORMAL MANA COST, SELF-ONLY WARD, RANK-BY-RANK GATES

**LATEST USER DECISIONS**, with free-text correction overriding temptation to invent shield behavior: user explicitly says "**it's a fire elemental shield, we have global rules for elemental shields already in place**." This is a request to REUSE existing shared elemental-shield implementation and coefficients, not a new all-damage-at-1:1 special shield.

### USER-APPROVED constraints

1. **The initial proposed Mana costs were TOO LOW; request HIGHER mana costs across all four Fire Mage actives.** Old DESIGNER-only formulas `Fireball=10+2*r` (12..80), `Ignite=14+2*r` (16..74), `LivingBomb=22+3*r` (25..112), `FlameWard=5+r` (6..30) are **REJECTED AS INSUFFICIENT**, NOT APPROVED. The user has NOT given exact replacement Mana formulas and explicitly says "**please propose numbers**"; propose higher amounts below, clearly marking unapproved. All costs must rise with ability purchased rank. No Fire Mage-specific Mana/INT max or regen coefficients; global Mana stays `ClassBaseMana+5*INT`, regen `4+0.1*SPR`.
2. **Empowered Flame Ward's bonus shield is a TRUE FIRE-ELEMENTAL SHIELD** following ALREADY-CANONICAL global elemental-shield system. User selected "all damage types" with notes clarifying elemental rules. Source verified: `docs/STATS_AND_DAMAGE.md`, section "Elemental shield absorption efficiency — latest global rule (updated 2026-10-19; Defender Round 1)", lines555–570. **Eligible post-mitigation incoming Fire damage consumes 0.8 shield capacity per1 damage prevented; ordinary eligible non-Fire elemental or untyped/physical damage consumes1.2 shield capacity per1 damage prevented**. This shield can absorb ALL ORDINARY ELIGIBLE DAMAGE, not Fire only, but **does not ignore pre-existing global eligibility / pure/shield-piercing rules**. Nominal capacity is SHIELD HP, not raw HP damage prevented: `blocked=min(eligiblePostMitigationDamage,remainingShieldCapacity/elementalShieldCost)`, `capacityConsumed=blocked*elementalShieldCost` (0.8 matched Fire, 1.2 otherwise). Never add a special 1:1 Fire Mage override, immunity, or reinterpret the shield as elemental resistance. Source notes matching Fire blocks1.25 damage per shield HP; nonmatching prevents0.833333 damage per shield HP. The empowered Fire shield capacity formula and lifetime remain `50+(0.5+0.1*r)*INT` and10s at Ward purchased rank r1..25. **Order approved previously:** Fire elemental shield absorbs first, then Mana-based damage redirection consumes Mana on remaining damage, then HP. Damage pathway must use global health/shield mitigation rules.
3. **Empowered Fire spell Mana cost = its own NORMAL spell Mana cost at same purchased rank, PLUS spending the full100 Overheat.** NO extra Mana surcharge; not free. A successful empowered cast generates ZERO replacement Heat (previously approved), while any ordinary valid Fire spell cast incl defensive Flame Ward generates `10+OverheatMasteryRank` Heat. Initial Mana affordability / failed casts should follow the global cast/resource policy; do not consume100 Heat on a failed cast without further user agreement.
4. **Flame Ward is SELF-ONLY**, like TierII Math Barrier. No targeting allies directly in TierIII. It is an instant Fire spell, baseline small upfront rank-rising Mana cost plus ongoing Mana paid per HP prevented, and its empowered variant additionally grants self the true Fire-element shield. Ward's normal duration `4+6*(r-1)/24` seconds, fraction redirect `0.30+0.35*(r-1)/24`, efficiency `1.9-0.6*(r-1)/24` Mana/HP, cooldown `28-8*(r-1)/24` seconds, rank1..25, all previously approved.
5. **Skill rank gates are PER PURCHASED RANK with individual MINIMUM CLASS LEVEL requirements**, and **all 150 ranks must become available by Fire Mage class level150** (one earned point/level, rank0 starters). User chooses this rather than no rank gates after first unlock. First unlock levels previously user-approved: Fireball Lv1, Overheat Mastery Lv1, Ignite Lv3, Flame Ward Lv5, Living Bomb Lv8, Infernal Presence Lv10. Exact rank2+ gate schedule is **NOT YET USER APPROVED**; require mathematically audited, reachable gates, ensure sufficient aggregate capacity for all150 earned points by Lv150 and all skill caps exactly accounted.

### EXISTING GLOBAL ELEMENTAL SHIELD SOURCE (verified this round)

`docs/STATS_AND_DAMAGE.md` lines555–570 explicitly states *all ordinary eligible incoming damage* with **shield HP spent per1 post-mitigation damage blocked**: **same element0.8 / different element or untyped1.2**. It supersedes older 0.5/1.5 and previous 1.5/0.5-style ratios. `Health` checkpoint describes adoption of shared0.8/1.2 scheme and generic shield1:1; keep Fire Mage using that common behavior. Same-element Fire matching and nonmatching packets follow existing pipeline, including existing exceptions for shield-piercing or Pure-damage eligibility. Do NOT create Fire Mage-only defense code.

### DESIGNER NEXT-ROUND HIGHER MANA COST PROPOSALS — NOT USER APPROVED

User asks for HIGHER numeric formulas than rejected Round15 suggestions. Candidate first-test costs for each successful cast at purchased ability rank r:
| Active | NEW designer proposal | Rank1 | Rank cap | Max-rank cost | Rejected prior formula |
| --- | --- | ---: | ---: | ---: | --- |
| Fireball | `18+3*r` | 21 | 35 | 123 | `10+2r` (80 at cap) |
| Ignite | `24+3*r` | 27 | 30 | 114 | `14+2r` (74 at cap) |
| Living Bomb | `34+4*r` | 38 | 30 | 154 | `22+3r` (112 at cap) |
| Flame Ward | `10+2*r` | 12 | 25 | 60 | `5+r` (30 at cap) |

These exceed the old candidate at EVERY rank. **MANA COSTS ABOVE ARE DESIGNER CANDIDATES ONLY, NOT ACCEPTED**, and should be presented for user adjustment after comparing actual early/TierIII max Mana pool. Ward's upfront Mana cost is IN ADDITION TO its per-damage Mana redirection spend; normal cost paid for both normal and empowered, with empowered additionally spending100 Heat. Aim for meaningful resource economy while preserving global Mana policy, e.g., 500INT contributes2500 base Mana from INT, not a bespoke class multiplier. Do not silently add cooldown rank savings on top of already-approved cooldown formulas.

### Current six-skill Fire Mage design preserved

- Approved rank caps Fireball35, FlameWard25, Ignite30, LivingBomb30, OverheatMastery15, InfernalPresence15; total150 purchases, 1 point/class level1–150, rank0 start.
- Cooldown all linear: Fireball14→7s r1..35, Ignite14→10s r1..30, Bomb22→15s r1..30, Ward28→20s r1..25.
- Cast times Fireball0.2s, Ignite instant, Bomb0.2s, Ward instant.
- Damage profile Fireball balanced flat+INT, empowered fire ground6 ticks at25% reference Fireball hit/tick4m6s; Ignite 8 ticks/8s total≈2x reference Fireball, empowered=3x normal Ignite same duration; normal Living Bomb per target3m blast≈1.5x reference Fireball, empowered initial2x normal and one-hop normal3s-fuse/full-normal-damage bomb spread; both flat AND INT coeff increase with EACH purchased skill rank.
- 0..100 Overheat, ordinary cast gain `10+passiveRank`, full gauge empowers next eligible spell spending all100 with zero new Heat; idle10s then lose10 Heat/sec.
- Aura permanent15m, allied Fire resistance +0.5pp/rank incl caster, enemies -0.5pp/rank, identical overlapping copies strongest wins.
- Global Mana remains `ClassBaseMana+5*INT`, natural regeneration `4+0.1*SPR`.
- Future-only Pyromancer TierIV possible merged aura+heat passive, toggle Mana Ward, Fire LAZOR/Meteor; later Frost Mage→Cryomancer; secret Hydromancer once BOTH Fire Mage and Frost Mage Lv120. These are ideas, not additional implemented skills/classes.

### Next actions requiring USER SIGNOFF

1. Approve/revise above HIGHER Mana cost proposals: `Fireball18+3r`, `Ignite24+3r`, `Bomb34+4r`, `Ward10+2r`.
2. Validate **global Fire shield** type rules including 0.8/1.2 shield-HP consumption (already global); focus on any Fire Mage-specific shield interactions only if explicitly requested.
3. Present numeric **rank-by-rank Lv1–150 skill gate audit** satisfying approved first ranks/caps, with no point unspendable indefinitely and skill ranks gated appropriately. User has approved gate SYSTEM but not gate timetable.
4. Propose exact Fireball flat + INT rank damage equation then matched Ignite/Bomb/ground equations (current relative targets only) and collect user approval.
5. Decide Fire Mage base INT/SPR/VIT, Mana ClassBaseMana, class advancement quest, and spell animations when mechanics finalized.

**Documentation only: no Unity gameplay source/assets/builds modified, no merge.**


## Fire Mage Tier III Round 16 — 2026-11-26: FIREBALL DAMAGE FORMULA APPROVED; MANA COSTS DEFERRED UNTIL STATS; EVEN RANK GATING APPROACH APPROVED

**LATEST USER ANSWERS to previous three questions, authoritative:**

1. **Mana costs NOT YET ACCEPTED.** User says regarding earlier suggested Mana increases: "**i think that maybe make it like 5r or something? lets come back to this after we have done fire mages stats.**" Explicitly **DEFER numeric Mana-cost design until Fire Mage TierIII base attributes, stat growth, and class Mana base are discussed**. The `5*r` coefficient is a tentative idea, NOT an approved universal rate or a directive to apply 5r instantly to all abilities. Previous designer-only costs `18+3r`, `24+3r`, `34+4r`, `10+2r` remain UNAPPROVED. All four active Mana costs must still INCREASE with their own purchased skill rank, and Flame Ward still pays SMALL upfront cast Mana + Mana per damage prevented. Global Mana max `ClassBaseMana+5*INT` and natural regen `4+0.1*SPR` remain unchanged; differentiate via base Mana, base/growth INT and SPR, gear.
2. **Fireball exact first-playtest raw damage formula USER APPROVED** in response to proposal "**start Fireball at something like `30+8r+(0.3+0.04r)*INT`**, then balance other spells around your approved ratios": user answers "**sure**". Therefore for purchased Fireball rank `r=1..35`, before Magical/Fire defenses, `RawFireballHit(r,INT)=30+8*r+(0.3+0.04*r)*INT`. At rank1: `38+0.34*INT`; at rank35: `310+1.70*INT`. Both raw flat and INT coefficients grow by own purchased rank. This is a first-playtest target, subject to live balance later, not a currently implemented formula. Keep it separate from Fireball's six ground ticks when empowered. Normal Fireball remains direct single-target magical Fire projectile; empowered keeps same initial hit PLUS ground field. Reference comparison targets still: each of 6 ground ticks approx 25% of Fireball direct hit, Ignite full 8s DoT approx2x a *comparable reference* Fireball hit, Living Bomb normal per-target detonation approx1.5x the comparable hit. Ignite and Bomb have caps30 while Fireball cap35; do not silently force their ranks into the Fireball rank variable or count their damage as already approved exact formulas.
3. **Evenly paced rank-by-rank level gates APPROVED IN PRINCIPLE**: User says "**sure**" to "**distribute the 150 available purchases relatively evenly across class levels 1–150, while respecting each skill's approved initial unlock level**" and to mathematically audit the schedules. This approves the PROGRESSION STRATEGY. The **explicit per-rank table BELOW is a newly generated designer schedule, not independently reviewed/approved by user yet**. Individual rank min-level gates must make all150 ranks available by Lv150. Preserve rank0 at class start and 1 earned skill point per Fire Mage class level1–150.

### Exact AUDITED designer gate table, awaiting explicit user review (NOT locked)

Construction: force Fireball rank1 and Overheat Mastery rank1 available Lv1, Fireball rank2 Lv2, Ignite rank1 Lv3, Flame Ward rank1 Lv5, Living Bomb rank1 Lv8 and Infernal Presence rank1 Lv10. Leave Lv4 without a *new* gate because Lv1 initially offers TWO rank1 choices; Lv4 still has 4 total eligible ranks for its four earned points. On other levels place one new availability gate chosen by weighted deficit against each class's maximum allocation/150, spread fairly among six skills. This is a DESIGNER-generated schedule satisfying user's *approach*, not an additional user approval of exact gate placements.

| Skill | Approved purchased-rank cap | Proposed minimum Fire Mage class level for each successive purchased rank (rank1 first) |
| --- | ---: | --- |
| Fireball | 35 | 1, 2, 11, 15, 20, 24, 30, 31, 37, 41, 45, 50, 54, 60, 61, 67, 71, 75, 80, 84, 90, 91, 97, 101, 105, 110, 114, 120, 121, 127, 131, 135, 140, 144, 150 |
| Flame Ward | 25 | 5, 7, 16, 21, 27, 34, 40, 46, 51, 57, 64, 70, 76, 81, 87, 94, 100, 106, 111, 117, 124, 130, 136, 141, 147 |
| Ignite | 30 | 3, 6, 12, 18, 22, 28, 32, 38, 42, 48, 52, 58, 62, 68, 72, 78, 82, 88, 92, 98, 102, 108, 112, 118, 122, 128, 132, 138, 142, 148 |
| Living Bomb | 30 | 8, 9, 13, 19, 23, 29, 33, 39, 43, 49, 53, 59, 63, 69, 73, 79, 83, 89, 93, 99, 103, 109, 113, 119, 123, 129, 133, 139, 143, 149 |
| Overheat Mastery | 15 | 1, 17, 26, 36, 47, 56, 66, 77, 86, 96, 107, 116, 126, 137, 146 |
| Infernal Presence | 15 | 10, 14, 25, 35, 44, 55, 65, 74, 85, 95, 104, 115, 125, 134, 145 |

**Gate computation audit:** exactly `150` proposed rank entries across six skills, caps `35/25/30/30/15/15`, correct user-approved first unlock class levels `1/5/3/8/1/10` for skill order Fireball/Ward/Ignite/Bomb/Overheat/Aura, strictly increasing level gates within each skill, no gate above level150. For EVERY class level `L=1..150`, sum of rank gates at or below L is >=L, so a player hypothetically spends one earned rank point each level with no forced unspendable level. At Lv150 exactly150 purchasable ranks become eligible (all max ranks); Fireball final rank gate at Lv150. It remains normal for players who save points to purchase several earlier eligible ranks later. **No actual game code, runtime table or save data changed.**

### Next design should be FIRE MAGE TIERIII STATS first, BEFORE approving Mana costs

Actual source read this round: `Assets/_DiceFree/Scripts/Tools/Editor/MageAuthoring.cs` authored parent **TierII Mage Lv1 base** attributes `VIT11/STR5/AGI8/INT18/SPR9`, and **growth per level** `VIT+1.5/STR+0.5/AGI+0.75/INT+3/SPR+1.5`; current historic TierII Mage Mana profile `baseMaximum80, 10 Mana/INT, regen2+0.04INT` is **out of date relative to newer global approved Mana policy** and must NOT be copied to Fire Mage. TierIII Fire Mage is its OWN Lv1 manifestation, not additive Mage Lv60 stats.

**DESIGNER FIRST-TEST STAT PROPOSAL, USER HAS NOT YET APPROVED ANY FIRE MAGE TIERIII NUMBERS:** TierIII Fire Mage Lv1 `VIT19, STR7, AGI12, INT33, SPR13`; per-level `VIT+2.0, STR+0.75, AGI+1.2, INT+5.5, SPR+1.5`. Total base84, growth10.95 vs TierII base51 growth7.25. Focus on INT, some survival VIT, modest SPR to support mana regen; user may want different profiles. **Proposed ClassBaseMana=160** for max mana `160+5*INT`, global natural regen `4+0.1*SPR`, provisional new-manifestation Lv1 Mana325 at INT33 and natural regen5.3/s; class Lv150 pre-gear INT=852.5 and Mana4422.5, natural regen27.65/s with SPR236.5. Check real combat pacing and Mana cost 5r thought AFTER attributes are user-set. Note stat values may be fractional depending on current game's rounding; no coding authorized.

### Other Fire Mage established rules retained

- Fire Mage skills caps Fireball35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15 =150; first unlocks Lv1,5,3,8,1,10 in this skill order. Per-rank min class gates (principle approved), numerical schedule here proposed/audited.
- Fireball 0.2sec cast, linear CD14→7s r1..35; Ignite instant cast, CD14→10s r1..30; Living Bomb 0.2s cast, CD22→15s r1..30; Flame Ward instant self only, CD28→20s r1..25. All ordinary Mana costs increase with own rank, values OPEN.
- Flame Ward normal percentage-based Mana redirect fraction `0.30+0.35*(r-1)/24`, efficiency `1.9-0.6*(r-1)/24` Mana per damage blocked, main lifetime `4+6*(r-1)/24` secs; upfront Mana plus ongoing spent on redirection. Empowered extra true Fire-element shield capacity `50+(0.5+0.1*r)*INT`,10s, standard elemental shield capacity consumption0.8 Fire matching and1.2 other eligible ordinary damage, extra shield before Mana Ward and HP.
- Overheat ordinary cast `10+OverheatMasteryRank` Heat, empowered next cast after reaching100 spends100 Heat and normal Mana but no same-cast Heat gain, 10s idle then -10 Heat/s. Every active spell including Ward eligible. Permanent15m aura includes caster, ally Fire resist +0.5pp/rank, hostile Fire resist -0.5pp/rank, same-source strongest-only.
- Ignite8 ticks across8s t1..8, empowered3x normal DoT on one target same duration, reapply refresh; Living Bomb3s fuse/3m splash, empowered original2x raw normal detonation plus single-generation normal-damage/normal-fuse bomb spread. Empowered Fireball4m fixed burning ground6s, t1..6 six ticks. Global Fire magical damage/resist/crit rules unchanged.
- TierIV Pyromancer possible combined aura/passive, toggle Ward, Fire LAZOR and maybe Meteor; Frost Mage→Cryomancer and hidden Hydromancer requiring Fire Mage Lv120 and Frost Mage Lv120 are IDEA BANK ONLY, not now.
- **Docs-only. No Unity source/assets/build, runtime changes, merge or gameplay testing authorized.**


## Fire Mage Tier III Round 17 — 2026-11-26: USER REVISES AGI/INT GROWTH; HIGHER ~10r MANA COST IDEA

**LATEST USER RESPONSE:** "remove 0.2 agi and add to int and also increase int by another 0.3. but yeah with these mana values and mana regen we need to increase the mana costs of spells a lot. i think maybe like 10r?"

### USER-REQUESTED stat profile adjustment

The prior Round16 designer proposal was Fire Mage TierIII own new class Lv1 VIT19/STR7/AGI12/INT33/SPR13, growth VIT+2.0/STR+0.75/AGI+1.2/INT+5.5/SPR+1.5, ClassBaseMana160. The user **explicitly changes growth** AGI -0.2 and INT +0.2 +0.3, leaving other stats unchanged:

| Attribute | TierIII Fire Mage Lv1 | Per-level growth | Status |
| --- | ---: | ---: | --- |
| VIT | 19 | +2.00 | Previously proposed, no change requested |
| STR | 7 | +0.75 | Previously proposed, no change requested |
| AGI | 12 | **+1.00** | **Explicit user correction** from +1.20 |
| INT | 33 | **+6.00** | **Explicit user correction** from +5.50 |
| SPR | 13 | +1.50 | Previously proposed, no change requested |
| **Total** | **84** | **+11.25** | Lv1 total unchanged; growth +0.30 vs old10.95 |

User responds "but yeah with these mana values and mana regen" while correcting proposed stat growth; treat unchanged base stats, other growth and **ClassBaseMana160** as an **accepted first-test working baseline**, distinguish explicit correction from implicit assent. No new class-specific Mana coefficients. At class level `L`, use standalone new manifestation `Stat(L)=Base+(L-1)*Growth` rather than inherited accumulated Mage Lv60 parent attributes.

Approved previously shared Mana formulas `MaxMana=ClassBaseMana+5*INT` and `NaturalManaRegenPerSecond=4+0.1*SPR` produce at the accepted first-test profile:
- **Fire Mage Lv1:** INT33, SPR13, MaxMana `160+5*33=325`, natural regeneration `4+0.1*13=5.3 Mana/s` before gear/buffs.
- **Fire Mage Lv150:** INT `33+149*6=927`; SPR `13+149*1.5=236.5`; **MaxMana `160+5*927=4795`**; regeneration `4+0.1*236.5=27.65 Mana/s` before gear/buffs/rounding. Compare old Round16 INT852.5 / MaxMana4422.5, superseded.
- Other unmodified Lv150 stats: VIT `19+149*2=317`, STR `7+149*.75=118.75`, AGI `12+149*1=161`.
- TierII Mage underlying authoring remains historical Lv1 VIT11/STR5/AGI8/INT18/SPR9, growth+1.5/+0.5/+0.75/+3/+1.5. Do not copy its deprecated class-specific resource multipliers into TierIII Fire Mage.

### Mana cost escalation intent, exact formulas NOT APPROVED

User says the large 4795 max Mana and27.65/sec regen at Lv150 require **MUCH HIGHER Mana COSTS** than prior designer proposal, tentatively "**maybe like 10r?**" This is a **suggested order-of-magnitude / rank coefficient** of roughly **+10 Mana per purchased active skill rank**, **NOT signoff on specific cost formulas**. Prior lower drafts `Fireball18+3r, Ignite24+3r, LivingBomb34+4r, Ward10+2r` remain unapproved; do NOT silently promote them.

**Designer proposal to ask next (NOT USER-APPROVED):** for each skill's own purchased rank `r`:
- Fireball `20+10r` (rank1=30, rank35=370), maintaining short0.2s cast, CD14→7s. At Lv150 max Fireball costs ~7.7% of4795 pool and7s natural regen ~194 Mana between cooldown completions before gear/other spells; not free but less than a full pool reset.
- Ignite `30+10r` (rank1=40, rank30=330), instant, 8s DoT, CD14→10s.
- Living Bomb `50+10r` (rank1=60, rank30=350),0.2s cast,3s fuse, CD22→15s.
- Flame Ward `10+6r` (rank1=16, rank25=160) as **intentionally lower upfront cost**, since it ALSO spends Mana dynamically at `1.9-0.6*(r-1)/24` Mana per prevented HP, CD28→20s; alternative `10+10r` (rank1=20, max260) to explore if user wants uniform +10r. The user did not explicitly exempt Ward from the 10r thought; ask preference. All four still have costs rising with rank, empowered spells pay ordinary Mana cost PLUS100 Heat with no extra Mana surcharge, self-only Ward.

Costs must be tested against actual player rotation, the 10sec Heat idle period, rank gate/ability availability, skill use frequency, and actual global Mana pool/regen. Do not treat `10r` or these four example formulas as user-approved.

### Fireball damage and progression still valid

- Normal Fireball raw direct Magical Fire damage `30+8r+(0.3+0.04r)*INT` at purchased r1..35 (Round16 user approved) before mitigation. Both flat and INT coefficient scale with OWN skill rank. At max r35 with proposed new Lv150 no-gear INT927, raw damage would be `310+1.7*927=1885.9`, before any gear/resistance modifiers; this is a theoretical formula output, NOT a measured DPS figure.
- Prior approved comparative goals: burning-ground each of six ticks≈25% corresponding Fireball direct hit, Ignite eight-tick total≈2x comparable Fireball hit, Living Bomb initial normal3m explosion≈1.5x comparable Fireball hit. Exact independent INT and flat coefficients for other spells pending approval.
- User approved evenly paced per-rank minimum-level gates / total150 points and first-unlock levels; a fully 150-entry designer gate table already recorded in Round16 remains **mathematically audited but NOT separately approved as exact gates**. No change to this table in Round17.
- Six rank caps remain Fireball35 / Flame Ward25 / Ignite30 / Living Bomb30 / Overheat Mastery15 / Infernal Presence15. 1 class point/class level1..150, rank0 initial, other Fire Mage timing/Heat/Ward/aura/DoT/one-hop bomb details preserved in prior sections.
- Future TierIV Pyromancer possible merged Heat/aura, toggle Ward, Fire LAZOR/Meteor; extra Frost Mage→Cryomancer and hidden Hydromancer after both Fire Mage and Frost Mage Lv120 remain future idea bank.

### Next Round18 decisions to request

1. Confirm full Lv1 attributes & growth table above and **ClassBaseMana160** as first-playtest canon, optionally adjust other stats rather than inferring hidden changes.
2. Approve/adjust sample HIGH Mana formulas Fireball `20+10r`, Ignite `30+10r`, Bomb `50+10r`, Ward `10+6r` or `10+10r`, accounting for ongoing Ward Mana absorption.
3. Confirm exact 150-rank-gate schedule table Round16 if player likes proposed distribution, or request different pacing.
4. Decide whether damage/tick coefficient rounding is exact proportion of approved Fireball base formula or independently authored flat+INT per skill with comparable rank progression.
5. Later class advancement quest and TierIII stat resources/HP calculations with shared rules.

**Design docs only; no Unity gameplay/source/asset edits, no game build, no merge.**


## Fire Mage Tier III Round 18 — 2026-11-26: USER APPROVES STAT PROFILE AND BASE MANA, EXACT AUDITED 150 GATES, WARD MANA COST; OFFENSIVE ~15r DIRECTION

**LATEST USER APPROVALS, superseding any prior designer proposals or tentative status for these specific matters:**

1. **Fire Mage TierIII Lv1 base attributes and PER-LEVEL GROWTH definitively USER APPROVED as first-playtest baseline:**
   - Lv1: `VIT19, STR7, AGI12, INT33, SPR13`.
   - Per level: `VIT+2.00, STR+0.75, AGI+1.00, INT+6.00, SPR+1.50`. Sum level1 attributes84; growth total11.25 per class level.
   - `ClassBaseMana = 160` definitively approved. Continue to use shared global max Mana `160+5*INT` and regen/s `4+0.1*SPR`; **DO NOT** invent a Fire Mage-only per-INT Mana/regen coefficient. Lv1 no-gear INT33/SPR13 yields Mana325, regen5.3 per sec; Lv150 no-gear INT927/SPR236.5 yields Mana4795, regen27.65/s. These are theoretical pre-gear stats; use game's existing fractional/rounding policies on implementation.
   - TierIII class is its own Level1 manifestation after advancement, NOT accumulated Mage Lv60 stats pasted on top. TierII Mage source was checked and has lower Lv1 base and growth; do not modify it in this design-only round.
2. **Exact full 150-rank Fire Mage Level-gate table from Round16 is NOW USER APPROVED, not just the design principle.** This is the already-authored six-row table in the Round16 section above, headed "Exact AUDITED designer gate table, awaiting explicit user review (NOT locked)" and enumerating every rank's minimum class level. User explicitly answers "Approve the exact 150-gate table in docs/classes/MAGE_TIER3_DESIGN.md". **That exact existing table is now authoritative first-test schedule**, including its unusual level1 two rank1 choices and no new availability gate at level4. Earlier Round16/17 headings and prose calling it designer-only/unapproved are HISTORICAL and superseded by this acceptance. Retain every numerical entry unchanged.
   - Fireball cap35: first Lv1, final rank Lv150.
   - Flame Ward cap25: first Lv5, final rank Lv147.
   - Ignite cap30: first Lv3, final rank Lv148.
   - Living Bomb cap30: first Lv8, final rank Lv149.
   - Overheat Mastery cap15: first Lv1, final rank Lv146.
   - Infernal Presence cap15: first Lv10, final rank Lv145.
   - Sum 150 allowed rank purchases by ClassLv150, 1 class point per level1..150, all start purchased rank0. The already completed audit verified total eligible ranks at EVERY level L is >=L, so no permanently unspendable points while leveling, and all skill caps exactly achievable. Do not silently regenerate or change that exact table on implementation.
3. **Flame Ward upfront Mana activation cost USER APPROVED EXACTLY `ManaCost_FlameWard(r)=10+6*r`, purchased rank r1..25**. Rank1 cost16, rank25 cost160. This is distinct from additional Mana automatically spent when redirecting applicable incoming damage to the Mana pool; `ManaPerHP(r)=1.9-0.6*(r-1)/24`, `RedirectFraction(r)=0.30+0.35*(r-1)/24` both previously approved. Empowered Flame Ward costs the same up-front Mana PLUS100 Heat and continues to consume extra Mana per damage blocked by Mana ward, with bonus true Fire elemental shield first (global 0.8 matching Fire, 1.2 other ordinary eligible shield capacity consumption).
4. **Offensive spell Mana costs NOT numerically approved.** User selects option "**Suggest increasing offensive spell costs to 15 Mana per rank; formulas to finalize**" rather than approving prior `20+10r, 30+10r, 50+10r` candidates. Treat `~15*r` as user-directed higher COST RANK COEFFICIENT TARGET, not a final approved cost formula or a wholesale override of Flame Ward's expressly approved `10+6r`. Exact base additions and whether all three offensive spells share +15r are still open and need separate signoff. Previous 10r formulas remain rejected/unapproved proposals.

### DESIGNER FOLLOW-UP OFFENSIVE MANA PROPOSALS (NOT USER APPROVED)

For each SPELL'S OWN purchased rank `r`: Fireball `20+15*r` (rank1=35, rank35=545); Ignite `30+15*r` (rank1=45, rank30=480); Living Bomb `50+15*r` (rank1=65, rank30=500). Use the same first-test base terms from Round17 but increase marginal cost to user-directed 15/rank. At Lv150 with Mana4795, max-rank Fireball would spend545 Mana ≈11.37% of unmodified pool and recover ≈193.55 natural Mana over its7s cooldown at regen27.65/s, meaning rotation must manage Mana. These proposals are to be approved/revised next; do NOT implement as final values without approval. Ward stays user-approved `10+6r`.

### Core existing user-approved spell mechanics retained

- Fireball raw Magical Fire direct hit `30+8r+(0.3+0.04r)*INT`, r1..35; empowered Fireball adds fixed-position 4m burning ground six ticks t1..6 seconds, each tick approximately25% direct Fireball reference hit, exact packet coefficients still TBD.
- Ignite normal8sec DoT ticks at t1..8, normal full tick sum ≈2x reference Fireball hit at comparable progression, empowered3x normal total on one target8s. Same-caster recast refresh, not stack.
- Living Bomb fuse3s, radius3m. Normal blast per eligible enemy≈1.5x normal Fireball reference hit. Empowered original detonation2x its OWN normal blast plus one hop applying normal3s fuse, normal full damage bombs to nearby others, copies NEVER propagate.
- Spells cast time/cooldown (all linear cooldowns): Fireball0.2s/14→7s r1..35, Ignite instant/14→10s r1..30, Living Bomb0.2s/22→15s r1..30, Flame Ward instant/28→20s r1..25. No shared custom cooldowns added.
- All four active spells incl self-only Ward ordinary cast produce `10+OverheatMasteryRank` Heat, 100 Heat empowers NEXT eligible cast spending100 and no same-cast Heat; after10s without successful Fire cast Heat decays10/sec.
- Self-only Flame Ward main Mana redirection fraction `0.30+0.35*(r-1)/24`, main duration `4+6*(r-1)/24`; empowered bonus true Fire-element shield capacity `50+(0.5+0.1*r)*INT` for10sec, absorbs any ordinary eligible damage using global elemental 0.8 matching/1.2 nonmatching shield-HP rules before Mana redirect on remaining damage and finally HP.
- Infernal Presence always-on aura radius15m includes caster, ally Fire resist +0.5 percentage points/rank, enemy -0.5pp/rank; strongest matching aura only.
- Future-only TierIV Pyromancer ideas combined Heat passive+aura, toggle Ward, huge Fire LAZOR, perhaps Meteor; later Frost Mage→Cryomancer and hidden Hydromancer if Fire Mage Lv120 + Frost Mage Lv120. No TierIV implementation authorization.

### Next design pass (AWAITING USER INPUT)

1. Approve/adjust offensive Mana first-test formulas `Fireball20+15r, Ignite30+15r, LivingBomb50+15r`, leaving approved Ward `10+6r` intact.
2. Approve independent flat+INT rank-dependent packet formulas for empowered Fireball ground, Ignite eight ticks and Living Bomb normal explosion; keep ratio targets at comparable rank investment.
3. Decide application/recast interactions for Ignite: empowered DoT potency vs normal reapplication, exact refresh/tick timer.
4. Decide Living Bomb same-source reapplication/replacement/snapshot behavior and target de-duplication for one-hop bomb propagation.
5. Fire Mage TierIII advancement quest, mentoring NPC, audiovisual theme and test plan/handover after mechanics complete.

**Only docs updated. NO Unity gameplay code/assets/build/tests, no branch merge.**


## Fire Mage Tier III Round 19 — 2026-11-26 USER APPROVES THREE MANA COST CURVES AND EXACT FIREBALL GROUND TICK; INDEPENDENT IGNITE/BOMB DAMAGE

**LATEST USER DECISIONS (explicit)** to Round19: (1) offensive Mana costs `Fireball20+15r, Ignite30+15r, LivingBomb50+15r` APPROVED, (2) empowered Fireball ground tick is EXACTLY25% of its OWN Fireball rank-based raw-hit formula, (3) Ignite uses INDEPENDENT rank-scaling flat and INT coefficient calibrated to ~2x comparable Fireball hit total over eight ticks, (4) Living Bomb uses INDEPENDENT rank-scaling flat and INT coefficient calibrated to ~1.5x comparable Fireball hit per enemy. These override former proposals and resolve earlier open ground-damage numeric formula.

### USER-APPROVED rank-based Mana cast costs: ALL FOUR ACTIVES NOW LOCKED

| Skill | Purchased ranks | EXACT Mana cast cost | Rank1 Mana | Max-rank Mana | Empowered cast |
| --- | --- | --- | ---: | ---: | --- |
| **Fireball** | r1..35 | `20+15*r` | 35 | 545 | Normal cast Mana PLUS100 Heat, no Mana surcharge |
| **Ignite** | r1..30 | `30+15*r` | 45 | 480 | Normal cast Mana PLUS100 Heat, no Mana surcharge |
| **Living Bomb** | r1..30 | `50+15*r` | 65 | 500 | Normal cast Mana PLUS100 Heat, no Mana surcharge |
| **Flame Ward** | r1..25 | `10+6*r` | 16 | 160 | Normal cast Mana PLUS100 Heat; its continuing Mana redirection has additional resource spend |

All four casts award `10+OverheatMasteryPurchasedRank` Heat if ordinary successful Fire casts, and never grant replacement Heat on the empowered cast. A 100 Heat empowered cast still requires the usual Mana to complete, and failed casts should follow shared resource rules. Mana max/regen remains GLOBAL `ClassBaseMana+5*INT`, `4+0.1*SPR`; approved Fire Mage ClassBaseMana160, Lv1 VIT19/STR7/AGI12/INT33/SPR13, growth2/0.75/1/6/1.5. At Lv150 no gear Mana4795 and regen27.65/s. All three max-rank offensive spells together cost `545+480+500=1525 Mana`, ~31.8% of an unmodified Lv150 full pool before natural regen. Do not replace global Mana coefficients or silently omit Ward's separate damage-prevention Mana spending.

### Empowered Fireball GROUND TICK RAW DAMAGE EXACTLY USER APPROVED

Previously USER approved normal Fireball RAW Magical Fire direct hit (before Magical DEF, Fire resistance, buffs etc.) for its own purchased rank `r=1..35` and caster INT:
`FireballRawHit(r,INT)=30+8*r+(0.3+0.04*r)*INT`.

**Each of its six empowered burning-ground Fire ticks is EXACTLY 25% of that caster's own Fireball RAW HIT formula at the same purchased Fireball rank**:
`GroundRawFireTick(r,INT)=0.25*FireballRawHit(r,INT) = 7.5+2*r+(0.075+0.01*r)*INT`.

- At r1, tick raw `9.5+0.085*INT`; at r35, `77.5+0.425*INT`. Both flat raw damage and INT coefficient grow with purchased Fireball rank as required.
- After empowered normal projectile hits an enemy, it creates **fixed world-position 4m radius ground Fire hazard** directly under the impacted target. Ground lasts6s; pulses t=1,2,3,4,5,6sec, no t0 packet. Ground raw total across six ticks for one victim remaining in area = `1.5*FireballRawHit`, on top of the original direct Fireball hit. Each pulse is its own Magical Fire damage packet passed through standard target Magical DEF/Fire resistance/etc.; don't derive each actual post-mitigation tick from post-mitigation initial projectile hit, since independent enemy resistances/area targets exist. Whether multiple own ground patches overlap/stack remains open. No new immediate AoE explosion.
- Empowered Fireball consumes100 Heat and its ordinary Mana cost `20+15r`; ground ticks grant no extra Heat.

### Ignite and Living Bomb have INDEPENDENT rank coefficients (USER APPROVED POLICY, NOT YET NUMERIC COEFFICIENTS)

- **Ignite**, its OWN purchased `r=1..30`, must use its own formula `IgniteRawFireTick(r,INT)=FlatIgnite(r)+CoefficientIgnite(r)*INT`; BOTH flat and INT coefficient MUST increase with purchased Ignite rank. Normal Ignite has exactly8 independent ticks at elapsed t=1..8 seconds after successful application, `NormalTotalRawIgnite=8*IgniteRawFireTick` before each tick's mitigation. Relative first-playtest target total ≈2x a comparable progression Fireball *direct hit*, not 2x Fireball at equal numeric rank when rank caps differ, and do not link runtime Ignite to Fireball rank or investment. Empowered Ignite exact3x **its own** normal total, same8sec eight ticks on single target, no propagation; self-reapplication refreshes not stacks. Exact `FlatIgnite`, `CoefficientIgnite`, refresh priority and ticker restart remain OPEN.
- **Living Bomb**, its OWN purchased `r=1..30`, must use independent `LivingBombRawExplosion(r,INT)=FlatBomb(r)+CoefficientBomb(r)*INT`; both parts increase with purchased Bomb rank. Target normal explosion raw≈1.5x a comparable progression Fireball direct hit per enemy within3m, **NOT** hard-linked to Fireball's purchased rank or runtime formula. Fuse3sec after application; original empowered detonation EXACT2x its own NORMAL explosion raw, and then copies NORMAL full damage with their own normal3sec fuse onto other valid enemies within3m. Copies do not spread, and neither explosions nor propagation generate Heat. Exact `FlatBomb`, `CoefficientBomb`, reapplication/duplicate marker policy and damage snapshot timing remain OPEN.

### Designer-only example formulas to inspect in next round (NOT USER-APPROVED)

Choose independent **linear per-rank** raw damage to match the existing Fireball rank1 and max-rank endpoints at similar *normalized skill investment*, without any runtime dependency on Fireball rank:
- Ignite tick candidate (r1..30): `9.5 + (68/29)*(r-1) + [0.085 + (0.34/29)*(r-1)]*INT`. At Ignite r1 total8 ticks = `76+0.68*INT` (exactly2x Fireball r1), at Ignite r30 total = `620+3.4*INT` (exactly2x Fireball r35); interpolate own rank independently in between. Empowered ticks each3x these normal ticks.
- Living Bomb initial NORMAL explosion candidate (r1..30): `57 + (408/29)*(r-1) + [0.51 + (2.04/29)*(r-1)]*INT`. At Bomb r1 `57+0.51*INT` (exactly1.5x Fireball r1), Bomb r30 `465+2.55*INT` (exactly1.5x Fireball r35), with independent rank growth. Empowered original twice this; spawned secondaries are normal.
- They are **mathematical DESIGNER SUGGESTIONS only**, not approved exact damage coefficients. A simpler rounded formulation might be preferred over fractional increments, but changing endpoints should require user signoff. Any status comparison must align starting vs fully purchased skill progression, not assume rank30 equals rank35.

### Existing Fire Mage approvals preserved

- Six skill caps exact35 Fireball /25 Ward /30 Ignite /30 Bomb /15 Overheat Mastery /15 Infernal Presence, total150 purchased rank points from Fire Mage Lv1..150, initial rank0. **Exact six-skill 150-gate table** already listed in Round16 was USER APPROVED Round18, do not regenerate. First unlock Lv1 Fireball, Lv5 Ward, Lv3 Ignite, Lv8 Bomb, Lv1 Overheat, Lv10 aura.
- CDs: Fireball14→7s linear r1..35, Ignite14→10s r1..30, Bomb22→15s r1..30, Ward28→20s r1..25. Casts Fireball0.2s, Ignite instant, Bomb0.2s, Ward instant self only.
- Ward main time4→10s, redirect percent30→65%, Mana/HP prevented1.9→1.3, all linear r1..25. Empowered extra TRUE Fire elemental shield capacity `50+(0.5+0.1r)*INT` duration10s, ordinary eligible damage absorbed with global0.8 Fire matched/1.2 others consumption, extra shield first, Mana Ward second, HP last.
- Heat0..100, ordinary cast Heat `10+passiveRank`, next eligible Fire spell at100 empowered spends100 then gains zero replacement Heat, idle10s then decay10/s. Aura15m permanent allies incl self +0.5pp Fire resist/rank, enemies -0.5pp Fire resist/rank, overlapping identical strongest-only.
- Future TierIV Pyromancer could combine aura+Heat passive, toggle Ward, Fire LAZOR, maybe Meteor; later Frost Mage→Cryomancer and hidden Hydromancer after both TierIII Fire Mage+Frost Mage reach120, idea bank only.

### Next design questions (PENDING USER APPROVAL)

1. Approve/simplify independent Ignite tick and Living Bomb initial raw explosion example coefficients above, while preserving rank-linear flat+INT coefficients and relative 2x/1.5x balance endpoints.
2. Decide Ignite refreshed normal vs empowered precedence and timer reset, multi-caster stack rules.
3. Decide Living Bomb duplicate/recast and snapshot rules; suppress recursive one-hop secondary copies.
4. Decide empowered Fireball overlapping ground field interaction with itself and other Fire Mages.
5. Plan Fire Mage TierIII class advancement quest / teacher after spell mechanics and rank gates fully specified.

**Documentation-only; NO Unity gameplay source/assets/build/testing, no class implementation or merge.**


## Fire Mage Tier III Round 20 — 2026-11-26 USER APPROVES IGNITE AND BOMB EXACT DAMAGE CURVES, POTENCY-SAFE IGNITE REFRESH, STACKED BOMBS, OVERLAPPING GROUND

**LATEST USER ANSWERS (AUTHORITATIVE, override previous pending designer status):**
1. Exact Ignite normal tick raw Fire Magical damage formula APPROVED:
   `IgniteRawTick(r,INT) = 9.5 + 68*(r-1)/29 + [0.085 + 0.34*(r-1)/29]*INT`, where `r` is **Ignite's own purchased rank 1..30**. This is a pre-mitigation raw packet per tick, with BOTH flat and INT coefficient increasing with purchased Ignite rank. Normal Ignite ticks once per second for 8s at t1..8, so normal total RAW over eight ticks is `8*IgniteRawTick(r,INT)`; empowered Ignite ticks for **3× the normal tick RAW** on a single enemy at same ticks/duration, no empowered AoE or propagation. Rank1 normal tick `9.5+0.085INT` and 8-tick raw sum `76+0.68INT`; rank30 normal tick `77.5+0.425INT` and raw sum `620+3.4INT`. Each packet independently follows global Magic/Fire defense and resistance.
2. Exact Living Bomb NORMAL detonation RAW Magical Fire damage formula APPROVED:
   `LivingBombRawExplosion(r,INT) = 57 + 408*(r-1)/29 + [0.51 + 2.04*(r-1)/29]*INT`, where `r` is **Living Bomb's own purchased rank 1..30**. Both flat and INT multiplier increase with Bomb rank. At r1 normal `57+0.51INT`, at r30 normal `465+2.55INT`. **USER REMINDS THAT LIVING BOMB IS AREA-OF-EFFECT, NOT SINGLE-TARGET EXPLOSION!** Each individual normal detonation damages **each valid enemy within 3m radius of that bomb's detonation location** for its own normal RAW; default fuse3s after successful application, independent of0.2s cast and rank-CD22→15s. Original empowered detonation hits EACH eligible enemy within3m for **exactly2× the original Bomb normal RAW**, then creates ONE GENERATION of secondary normal bombs on OTHER nearby valid enemies within the original3m detonation radius; each secondary has its own normal3s fuse and subsequently explodes for NORMAL RAW AoE within its own3m blast, with NO secondary propagation. Never remove each secondary AoE pulse, never apply empowered2× multiplier to secondaries. Explosions/procs don't generate Heat, only successful active Fire casts.
3. **Ignite same-caster reapplication:** RESTART the 8-second duration and the tick timer from t0 on reapplication; **retain whichever DoT damage potency is stronger** rather than downgrading an existing stronger empowered or higher-stat/rank Ignite when a weaker one is cast by that SAME caster. This preserves one Ignite effect PER CASTER on the target, not two. For balancing this as a well-defined runtime mechanism, treat raw tick potency (captured on original application) as comparison metric; higher strength wins, but precise snapshot/dynamic-stat handling is still pending if spell buffs change during the DoT. An empowered reapplication can upgrade a weaker normal Ignite; a weaker normal reapplication refreshes existing empowered Ignite's stronger damage while restarting timer. When different casters reapply, only their own instances update.
4. **Ignite cross-caster behavior USER CLARIFICATION:** "**multiple ignites from different casters can exist on the same target**." Track Ignite DoT by at least caster identity and target; multiple independent caster-owned Ignite DoTs tick on the same target, can each be normal or empowered with their own timer/potency. The "only one effect" refresh rule applies **only to that caster's own Ignite**. Do not use a global single-Ignite-per-target overwrite.
5. **Living Bomb reapplication/stacking:** User selects "**Allow multiple independent bombs**" rather than replacement or ignore. Multiple distinct Bomb instances can coexist on the SAME target, including from same caster, each with independent fuse, potency, rank/empowered provenance and individual3m AoE detonation. Every normal/secondary bomb still uses shared3s fuse and its own normal3m explosion. No implicit unique-target cap or global exclusion is approved. **One-hop recursion guard remains mandatory:** secondaries created by empowered original detonations may explode, but MUST NEVER CREATE ANOTHER GENERATION. With multiple empowered bombs, each original is a separate empowered cast and may produce its own ONE permitted generation. Exact safeguards for concurrency/deduplication/snapshot remain to design and test; do not silently prohibit deliberate multiple independent bombs.
6. **Empowered Fireball ground patch overlap:** User selects "**Each area deals its ticks independently**", **INCLUDING OVERLAP**. Each empowered Fireball impact creates its own fixed4m radius,6s lifetime burning-ground area at original hit location; EACH area emits six t1..6sec tick pulses. A valid enemy standing within two or more overlapping areas receives independently mitigated RAW ground tick packets from EACH area, even when same caster. No strongest-only/non-stacking replacement or merge. User previously approved each raw tick `GroundRawTick=0.25*[30+8r+(0.3+0.04r)*INT]=7.5+2r+(0.075+0.01r)*INT` based on THAT caster's Fireball own purchased rank1..35. Other casters' ground instances behave separately; independent pulse timings retained.

### Existing locked Fire Mage cast costs & combat anchors

- User-approved Mana costs at purchased rank r: Fireball `20+15r`, Ignite `30+15r`, Living Bomb `50+15r`, Flame Ward `10+6r` UPFRONT plus ongoing Mana consumption when redirecting incoming damage. Every empowered cast consumes its normal Mana cast cost **PLUS100 Heat**, no additional Mana surcharge.
- Fireball raw hit `30+8r+(0.3+0.04r)INT`; empowered extra patch raw tick =25% own normal projectile raw hit (6 ticks, fixed4m).
- Ignite and Living Bomb exact INDEPENDENT flat+INT coefficient formulas now approved as written above; they use their own ranks, not Fireball's runtime rank. Normal total Ignite ~2x comparable progression Fireball raw and Bomb each AoE explosion ~1.5x comparable progression Fireball raw at rank endpoints.
- Cast times Fireball0.2s, Ignite instant, Living Bomb0.2s, Flame Ward instant self-only; cooldowns LINEAR Fireball14→7s r1..35, Ignite14→10s r1..30, Living Bomb22→15s r1..30, Flame Ward28→20s r1..25. Ordinary successful active Fire cast incl Ward generates Heat `10+OverheatMasteryPurchasedRank`; cap100, next empowered cast spends100 and gives no new Heat, inactivity10s then -10Heat/sec.
- Flame Ward percent redirected `0.30+0.35*(r-1)/24` and Mana-per-HP blocked `1.9-0.6*(r-1)/24`; main time `4+6*(r-1)/24`, valid r1..25. Empowered self-extra true Fire elemental shield `50+(0.5+0.1*r)*INT` nominal SHIELD HP for10s, absorbs all ordinarily eligible post-mitigation damage with global matching Fire0.8 and other1.2 shield-HP-per-damage prevention, BEFORE Mana redirect and then HP.
- Permanent15m Infernal Presence aura allied Fire resistance +0.5pp/rank incl caster, hostile -0.5pp/rank, identical copies strongest-only.
- First-test TierIII stats Lv1 VIT19/STR7/AGI12/INT33/SPR13, per-level growth +2/+0.75/+1/+6/+1.5, ClassBaseMana160. Shared Mana max `160+5INT`, regen `4+0.1SPR` so no-gear Lv150 Mana4795 and regen27.65s.
- Exactly150 skill ranks Fireball35/Ward25/Ignite30/Bomb30/Heat passive15/aura15, earned1 per level1..150, none auto-granted. FULL 150-entry individual rank minimum-level gates in earlier Round16 table USER-APPROVED Round18, do not regenerate.
- Future-only TierIV Pyromancer combination passive/aura, Fire LAZOR, possible Meteor, toggle Mana Ward; future separate Frost Mage→Cryomancer and hidden TierIV Hydromancer unlocked at BOTH Fire MageLv120 AND Frost MageLv120. No class implementation now.

### Follow-up questions left open / possible Round21 (not yet user-approved)

1. **Ignite potency sampling:** snapshot spell rank, caster INT, damage bonuses at application and compare raw tick strength, or dynamically recalculate INT each tick? Need robust consistent rule for potency-safe refresh if INT changes over8sec.
2. **Living Bomb snapshot:** capture caster INT/skill rank on successful application or at detonation (3sec later), including secondary bombs inheriting snapshot? Clarify dying target/fuse behavior and exploded bomb instance location.
3. **Explosive spread with duplicates:** how many one-hop secondaries if multiple empowered original bombs detonate closely; preserve intentional independent bomb instances but avoid a secondary re-spreading or invalid target.
4. **Cooldown/Heat cast success semantics:** instant Ward and Ignite eligible on valid casts; no Heat/100 consumption for failures, exact global GCD and target validity align.
5. **TierIII advancement quest and teacher:** specify quest origin / required proof and world-map region when relevant; no implementation yet.

**Docs-only design round; NO Unity code/assets/build/test changes, no merges.**


## Fire Mage Tier III Round 21 — 2026-11-26: USER-APPROVED DAMAGE SNAPSHOTS, DEATH DETONATION, UNLIMITED DISTINCT SECONDARIES

**LATEST USER ROUND21 APPROVALS, authoritative over all earlier open/suggested snapshot and bomb-target-death alternatives. Design only; not yet Unity runtime implementation.**

### 1. Ignite snapshot on successful application (USER APPROVED)

- On each successful Ignite application, **snapshot caster's purchased Ignite rank, INT, and currently applicable damage bonuses** at that exact application time. Compute/store the resulting RAW Magical Fire damage-per-tick potency with the accepted formula `9.5+68*(r-1)/29+(0.085+0.34*(r-1)/29)*INT` and the captured offensive bonuses. **Do NOT dynamically recalculate caster attributes or outgoing damage buffs every subsequent tick.** Each actual damage packet is still processed through target mitigation/resistance at the tick's time under standard global combat rules; caster snapshot does NOT freeze target resistance or future immunity.
- If the application is empowered, preserve its **3× tick damage** at the same8s duration and8 ticks. Every caster can have **one distinct Ignite effect per target**; different caster-owned Ignite effects coexist and tick separately.
- Prior user-approved **same-caster Ignite reapplication** remains: restart the8-second duration AND next tick timer, but keep whichever **captured DoT potency is STRONGER**. Compare potency at application: a new weaker Ignite refresh does NOT downgrade an existing stronger empowered/buffed DoT; a new stronger Ignite upgrades it. For different casters do not compare or override across owners. Exact potency tie-breaking and handling a disconnected/dead caster need ordinary combat runtime semantics, not inferred extra user requirements.

### 2. Living Bomb damage snapshot on application (USER APPROVED)

- On successful application of **EACH distinct Living Bomb instance**, snapshot the **caster's purchased Living Bomb rank, caster INT and applicable offensive damage bonuses**; store independent normal/empowered provenance and effective explosion raw damage from `57+408*(r-1)/29+(0.51+2.04*(r-1)/29)*INT` before target defenses. **Do NOT recalculate from caster's current stats when the fuse expires**. The original empowered bomb's own3m explosion remains **2× that original's normal snapshot** per valid enemy in the AoE. Ordinary bomb raw is its normal snapshot.
- **Secondary bombs** spawned by that empowered original remain **NORMAL power**, with a **normal3-second fuse** and their own subsequent **3m AoE explosion** and a hard no-propagation marker. As an integration-level DESIGNER GUIDANCE consistent with snapshot provenance, copy the parent's *normal snapshot potency* into each secondary instance rather than rerolling caster INT after the original explosion; not an independent user-approved exception. Multiple simultaneous snapshots from different casts can differ and must remain separate.
- Multiple independent bomb instances on the same enemy, including different original/secondary bombs and from same or different casters, are explicitly allowed. Do not introduce one-bomb-per-target suppression, same-caster replacement, or global deduping merely for convenience. Each bomb has its own fuse, snapshot, instance ID, event and 3m blast.
- Standard per-target post-snapshot mitigation/resistance occurs at actual detonation time; snapshots only lock offensive values, not future target defensive state.

### 3. Target dies before fuse -> immediate AoE explosion (USER APPROVED)

- If the **living bomb's attached target dies before the fuse completes**, that instance **immediately detonates at the target's death WORLD POSITION**, producing its normal full **3m AoE Magical Fire explosion**, rather than waiting, disappearing, or doing a single-target-only packet.
- If the original bomb was an **empowered original**, the immediate detonation retains its **2× normal raw explosion** and its already-approved one generation of normal secondary bombs on other nearby **living valid** enemies within3m. If this is an ordinary or propagated secondary bomb, explosion remains **NORMAL power**; propagated secondary bombs **never propagate further**, including from death-triggered explosion.
- For a death carrying SEVERAL bombs, each instance triggers independently; **explosion chain reactions** can kill other bomb-bearing enemies, and those bombs each must follow the same immediate death rule while maintaining distinct IDs and one-hop/no-recursion provenance. **Implementation safety proposal, not an extra damage rule:** mark instances detonated/deleted atomically before applying AoE damage and use bounded deterministic event processing to avoid duplicate explosions or re-entrant infinite loops. Target that was already dead should not be dealt a second HP hit; the AoE remains centered on death position, hitting other eligible recipients.

### 4. Multiple secondary bombs on one target are allowed (USER APPROVED)

- Every empowered original bomb can spawn its own **NORMAL full-damage secondary bomb** on each other eligible target within3m of its original explosion, even if that target already carries other secondary or original bomb instances, whether from same or different casters.
- Each spawned bomb has **independent snapshot,3s fuse,3m AoE**, normal power, propagation flag FALSE. No caster-level or target-level "only one secondary" uniqueness cap. Secondary detonation can trigger deaths and thus immediate detonation of bombs carried by affected enemies, but **can never spawn new bombs**, which preserves one-generation propagation. One empowered original creates at most one new secondary per other *eligible enemy* in its own detonation event, not recursively for every enemy hit by a secondary.
- No extra Mana or Heat per spawned bomb or explosion; only the original successful player cast counts toward Mana and Heat, based on previously approved rules.

### 5. Empowered Fireball ground snapshot (USER APPROVED)

- When the empowered Fireball first creates its fixed-position burning ground at the impacted target's location, **capture its OWN Fireball purchased rank and caster INT at patch creation**; the six tick packets follow frozen raw coefficient based on that captured rank/INT. Each raw tick is still exactly `0.25*(30+8*r+(0.3+0.04*r)*INT)=7.5+2*r+(0.075+0.01*r)*INT`. Do NOT dynamically recalculate rank/INT if buffs or purchased rank change during the patch lifetime.
- Each independent 4m-radius ground patch lasts6seconds and ticks at t1,2,3,4,5,6 after patch creation; NO t0 explosion. Patches overlap and ALL deal damage independently, from same/different casters. Every raw tick is separately subjected to current target Magical DEF and Fire resistance/mitigation. **Whether non-INT outgoing buffs must also snapshot for Fireball ground was NOT explicitly answered**, so defer to existing global damage-snapshot policy or confirm later; never silently pretend user specified additional bonus snapshots for ground when only rank and INT were selected.

### Current Fire Mage approved mechanics retained

- Stat profile new TierIII Lv1 VIT19 STR7 AGI12 INT33 SPR13; per-level gains2/.75/1/6/1.5, baseMana160, universal maxMana=160+5INT and regen=4+0.1SPR.
- 150 user-approved rank gates in Round16 table, six rank caps Fireball35/Ward25/Ignite30/Bomb30/Overheat15/Aura15; class rank0 starter with one earned point per Lv1..150.
- Mana cast costs `Fireball=20+15r`, `Ignite=30+15r`, `Bomb=50+15r`, `Ward=10+6r` upfront plus redirected Mana for damage Ward actually prevents. Empowered spells cost usual Mana and consume100 Heat, no additional Mana surcharge.
- Casts and CDs Fireball0.2s/14→7s, Ignite instant/14→10s, Bomb0.2s/22→15s, Ward instant self-only/28→20s, all cooldown changes linear in purchased ability rank.
- Normal Fireball projectile raw `30+8r+(0.3+0.04r)*INT`; Ignite normal eight snapshot ticks, empowered triple; Living Bomb normal3m AoE all bombs including secondaries, original empowered2× normal and one-hop normal3m secondaries; Fireball empowered ground six independent ticks each one-quarter projectile raw in4m area.
- Flame Ward main percentage-based Mana redirect and per-HP Mana spend, empowered true Fire elemental shield with capacity `50+(0.5+0.1r)*INT`, fixed10s shield lifetime; true elemental0.8 matching/1.2 otherwise eligible shield-HP consumption before Mana redirection to remainder. Infernal Presence aura15m, +.5pp ally Fire resistance per rank and -.5pp hostile, strongest identical source.
- Ordinary eligible successful active Fire cast incl self-Ward Heat=10+OverheatMastery rank, max100, next Fire cast empowered uses all100 & awards no same-cast Heat; decay10/sec after10s no successful Fire cast. All propagation/tick damage triggers NO additional Heat.
- TierIV Pyromancer combined aura/Overheat idea, Fire LAZOR, Meteor, toggle ward; parallel Frost Mage→Cryomancer, hidden Hydromancer if both Fire Mage and Frost Mage reach Lv120 remain future brainstorming, not implemented.

### Follow-up Round22 candidates (NOT USER APPROVED)

1. Confirm **Fireball ground other outgoing bonuses**: snapshot all damage bonuses at patch spawn like Ignite/Bomb, or only INT+rank and evaluate other damage modifiers per tick using global combat pipeline?
2. Specify **Living Bomb chain reaction semantics** when bomb kills its attached target through another bomb's AoE (immediate on-death detonation) and ensure no cascade duplicates / reentrancy.
3. Define **Ignite same-caster recast potency rule** if new weaker Ignite is empowered vs an existing stronger nonempowered buff snapshot; stronger actual tick raw after bonuses wins, not empowerment label alone, unless user changes policy.
4. Determine whether **Fire Mage spell casts can crit** and whether Ignite/ground/bomb AoE ticks use global spell crit behavior. Do not invent Fire Mage-only crit exceptions.
5. Plan Fire Mage TierIII **advancement NPC/quest and its exact world-map placement**, after mechanics settle.

**Design docs only. Do not implement or touch Unity code/assets/build/tests; no merge.**


## Fire Mage Tier III Round 22 — 2026-11-26 USER APPROVES FULL GROUND SNAPSHOT, BOMB DEATH CHAIN REACTIONS, STRONGEST IGNITE, NO NATIVE SPELL CRITS, NEW FIRE MENTOR

**LATEST USER ANSWERS (authoritative):** (1) "snapshot all bonuses"; (2) "yes" to independently detonating all bombs immediately when a bomb-bearing target dies; (3) "yes" to strongest ACTUAL tick potency winning same-caster Ignite recasts even if it is nonempowered; (4) "none of the fire mages abilities can currently crit. there might be spel crit items added in the late game though"; (5) "a new fire-specialist npc".

### Approved clarifications

1. **Empowered Fireball burning ground snapshots ALL offensive damage bonuses at patch creation**, in addition to that Fireball's purchased rank, caster INT and other source damage inputs. Exactly six separately mitigated Magical Fire ticks at elapsed t1..6s from each fixed-position 4m radius patch, each with base raw `0.25*(30+8*r+(0.3+0.04*r)*INT)` plus the offensive bonuses captured at patch birth. Future expiration of temporary caster buffs does NOT weaken existing patches; future newly acquired buffs do NOT retroactively strengthen a patch. Target defenses and current Fire resistances still evaluate when each tick lands. Multiple overlapping patches from any caster all tick independently. Do not accidentally apply the snapshotted bonus twice to a tick if the global damage pipeline also applies current-caster bonuses.
2. **Living Bomb immediate death chain reactions explicitly YES:** When an enemy carrying multiple bomb instances dies (including because another bomb or fire AoE kills it), **ALL attached bomb instances detonate IMMEDIATELY AND INDEPENDENTLY at that enemy's death world-position**, EACH hitting valid enemies in its own **3m AoE** using its own snapshot raw damage and empowered provenance. If one bomb-bearing enemy has three bombs, all three produce their own explosions, not just one. Any empowered-original death detonation retains double raw initial explosion and its own one generation of NORMAL full-damage secondary bombs; normal/secondary death detonations do not propagate. Secondary copies remain 3s fuses, normal 3m AoE and non-propagating even if death detonates them sooner. Implementation should atomically transition each bomb instance to detonated/removed BEFORE dispatching AoE; process death-triggered explosion events once per instance, even with nested chain reactions, to avoid reentry/double explosion or accidental recursion while allowing genuine separate instances. There is NO arbitrary cap of one bomb per caster/target.
3. **Same-caster Ignite reapplication picks the STRONGER ACTUAL SNAPSHOTTED DAMAGE PER TICK**, independent of whether the incoming or existing Ignite was empowered. E.g., a normal Ignite with strong captured bonus/INT can persist through a weaker empowered recast; the recast still refreshes the8s lifespan AND resets next-tick timer. If the new Ignite has greater actual snapshotted tick potency, replace stored potency with that stronger snapshot. Compare actual calculated offensive packet potency with snapshot bonuses and 3x multiplier already included, NOT binary empowered flag, rank alone or raw pre-bonus formula. Each different caster maintains an independent Ignite on the same target. Per-tick target mitigation remains dynamic per global rules.
4. **No inherent critical hits from ANY Fire Mage ability at present.** Fireball normal hit and ground ticks, Ignite DoT ticks, Living Bomb all original/secondary explosions, Flame Ward, passive Overheat Mastery and Infernal Presence aura **have NO class-native crit permission or crit bonuses**. As a forward-compatibility statement rather than current implementation, user may add **late-game spell-critical items**; design must allow future explicitly compatible external equipment/bonuses to grant spell crit eligibility under whatever global item crit system is later approved. Do NOT implement item crits now; do NOT claim present Fire Mage spells can already crit merely because a generic damage packet elsewhere supports crits. Do not conflate normal physical BASIC attacks with spell ability crit exceptions; this approval concerns Fire Mage abilities.
5. **Fire Mage TierIII advancement requires a NEW FIRE-SPECIALIST NPC**, NOT the existing Mage Brewer mentor reused for Fire Mage. Identity/name, exact world/settlement placement, quest objective, visuals, dialogue, required items/enemies and rewards remain OPEN. Preserve advancement eligibility (Mage TierII level60+ then Fire Mage TierIII Lv1 manifestation) and new class stats/skills, but do not silently pick a lore/location or invent a finished quest.

### Previously locked core remains unchanged

- Lv1 TierIII Fire Mage VIT19 STR7 AGI12 INT33 SPR13; per-level +2/+0.75/+1/+6/+1.5; ClassBaseMana160 and global maxMana=160+5INT, regen/sec=4+0.1SPR.
- Six rank caps Fireball35, FlameWard25, Ignite30, LivingBomb30, OverheatMastery15, InfernalPresence15, total150. Exact full 150 per-rank min-level gate table in Round16 section was approved Round18; one earned rank point/class level1..150, initial rank0.
- Mana costs Fireball20+15r, Ignite30+15r, LivingBomb50+15r, FlameWard10+6r upfront PLUS per-prevented HP Mana on Ward. Empowered active cast pays normal Mana +100 Heat. All active Fire casts incl self-only Ward can generate or consume Heat; empowered casts grant no same-cast Heat.
- Cast time Fireball0.2s/Ignite instant/Bomb0.2s/Ward instant; linear cooldown Fireball14→7s r1..35, Ignite14→10s r1..30, Bomb22→15s r1..30, Ward28→20s r1..25.
- Fireball hit raw `30+8r+(0.3+0.04r)*INT` and empowered ground exactly 25% own Fireball raw per each six ticks. Ignite independent snapshot raw tick `9.5+68*(r-1)/29+[0.085+0.34*(r-1)/29]*INT`; empowered3x normal tick; one Ignite per caster-target pair, strongest actual snapshot recast. Living Bomb independent snapshot normal3m AoE `57+408*(r-1)/29+[0.51+2.04*(r-1)/29]*INT`, empowered original2x plus one generation secondary normal full AoE, independent copies and event-based death detonation.
- Ward normal percent redirect30→65%, Mana efficiency1.9→1.3 per prevented HP, main lifetime4→10s, extra empowered true Fire elemental shield capacity `50+(0.5+0.1*r)*INT`, lifetime10s, existing elemental shield global capacity0.8 per matching Fire damage prevented and1.2 per nonmatching eligible ordinary damage, shield FIRST then Mana redirect then HP.
- Heat maximum100, gain10+OverheatMastery rank for successful normal active Fire cast, inactivity10s then decay10/s. Infernal Presence always-on aura radius15m, allied Fire resistance +0.5 percentage point/rank incl caster, enemy -0.5pp/rank, identical strongest source only.
- TierIV Pyromancer future-only Fire LAZOR, toggle Mana Ward, possible Meteor, combined aura/passive; later Frost Mage→Cryomancer and hidden Hydromancer requiring both TierIII Mage paths Lv120 are future ideas, not current TierIII design.

### Round23 suggested decisions, NOT YET APPROVED

1. Place the NEW Fire specialist NPC in an existing region (Cornberg, Trimery, mountain/other grounded World1 area) and choose why a powerful Fire specialist is there.
2. Choose NPC archetype/name and visual identity (practical fire tutor, wandering pyromancer, volcanic researcher, apprentice smith, etc.), without creating new assets now.
3. Choose TierIII Fire Mage advancement quest objective (proof of heat control, Ember ritual, survival challenge, Fire-focused encounter), with eligibility Mage level60.
4. Define quest duration/difficulty and whether solo instance or open world and any failure/retry rules, consistent with existing TierIII advancement patterns.
5. Decide how best to convey no innate spell crit but potential future equipment-granted spell crit, without implementing equipment exceptions yet.

**DESIGN DOCS ONLY. No Unity gameplay/source/assets/build/test changes, no branch merge.**


## Fire Mage Tier III Round 23 — 2026-11-26 USER REVISES LOCATION TO WORLD2 VOLCANO; SOLO 5-MIN FIRE-DODGE ESCAPE MAZE

**LATEST USER ANSWERS, AUTHORITATIVE, including free-text overrides:** Original form option said "Trimery, near a forge/training area" but user explicitly adds "**1. at the volcano in world 2**". Thus NEW Fire-specialist NPC must be located at the **World 2 volcano**, **NOT Trimery**. The user explicitly replaces the generic "fire-control trial with hazards, targets and final challenge" with "**it should be a fire dodging maze escaperoom thingy. like, a very short 'run kitty run' or 'pyramid escape' or something simiilar from wc3. you walk from point A to point B avoiding fire and walking on a path avoiding stepping in lava and stuff.**" Preserve this specific traversal/survival identity, NOT a standard DPS boss arena, gathering quest, puzzles requiring shooting targets, or conventional kill objective.

### USER-APPROVED advancement quest identity / encounter

1. **Mentor**: a BRAND NEW Fire-specialist NPC, experienced controlled-fire **pyromancer** who teaches TierII Mage → TierIII Fire Mage advancement. Do not reuse TierII Brewer/Mage mentor or make mentor a smith/researcher by default. **Position NPC at a volcano in World 2**. Name, precise dialogue, appearance, exact coordinate/settlement location, and quest-specific background NOT YET user-decided.
2. **World 2 volcano placement**: Existing grounded World2 World geography includes **Ember Crown**, the large volcanic cone in the southeast of World2's Ashfall Reach, according to `docs/WORLD_TWO_FULL_FACE_2026-11-24.md`; Basalt Needle is the other landmark, not the volcano. **Designer placement candidate: Ember Crown volcano or its accessible foot/rim**, NOT a user-approved exact coordinate. Confirm NPC accessible route without changing World2 structure. A separate unplanned World2 volcano should NOT be created from this request; use existing volcanic landmark unless user chooses otherwise. World2 access currently via southern Ironpine pass after Ancient Road Dragon quest (see `docs/ARBORIUM_WORLD_CONNECTIONS.md`), so Fire Mage advancement Lv60+ may depend on World2 story access; ensure progression gating does not unintentionally render required TierIII advancement impossible or require retroactive bypasses, and discuss whether this is intended.
3. **Quest format:** SOLO instance, not open-world cooperative completion, and no mandatory multiplayer party. Designed to take **approximately FIVE minutes**. Eligibility: prior approved at least Mage TierII Lv60 to advance to TierIII Fire Mage Lv1; quest award requires successfully reaching goal/returning to mentor under existing advancement system.
4. **Challenge/gameplay:** a **VERY SHORT fire-dodging maze/escape-room obstacle course**, inspired in FEEL by WarcraftIII custom minigames **Run Kitty Run / Pyramid Escape**. Player travels **from Point A (entrance) to Point B (exit)**, managing movement along navigable SAFE ground while **avoiding fire bursts, travelling or timed flame obstacles, lava patches or lava gaps, and stepping off safe path**. The terrain route is clear enough to read at speed; safe ground visibly distinguished from hazardous lava and active fire, with timing/pattern recognition and movement control central. Actual hazard patterns, checkpoint/failure/HP consequences, reset mechanism, movement speed modification, shortcuts and exact level layout remain TO DESIGN. Do NOT invent a boss kill, multiple-room combat, mandatory target shooting, or elaborate item collection as required winning objectives: the user specifically wants a short MOVEMENT/SURVIVAL maze.
5. **Outcome:** If successfully navigating route A→B, complete the trial toward Fire Mage advancement under normal quest turn-in/manifestation requirements; honor new Fire Mage TierIII stat/skills/level1 model. User has not specified whether exit automatically advances class or requires talking to the mentor, and whether death fails/resets checkpoint or requires restarting; leave open.
6. **Environment theme:** existing World2 volcanic/lava environment is the chosen venue. Use recognizable fire telegraphs and safe lanes, avoid instant undodgeable random damage; simple execution appropriate for approximately five minutes and nontechnical players. This is designer advice pending specific choices, not a new numerical mechanic approved by user.

### Existing World2 source context inspected for this round

- `docs/WORLD_TWO_FULL_FACE_2026-11-24.md` says the Ember Crown is a large volcanic cone in the **southeast**, whose rim is terrain-connected by talus apron.
- `docs/ARBORIUM_WORLD_CONNECTIONS.md` says World2's initial region is Ashfall Reach, and World2 is entered via **southern Ironpine pass** after Ancient Road Dragon story quest.
- `docs/WORLD_TWO_FRONTIER_2026-11-23.md` documents Ember Crown/Basalt Needle as existing landmark pair; World2 is not a blank map. This quest must not assume a new World2 zone is already created for it.

### Proposed next Round24 decisions (NOT APPROVED)

1. NPC **precise placement** at Ember Crown: safe rim access vs lower base/camp near lava vs furnace-shrine in volcanic slope, and NPC name/visual.
2. Maze **hazard failure model**: touching a flame/lava returns player to last checkpoint vs full reset vs heavy damage; how far checkpoints apart?
3. **Movement rules:** WASD/mouse movement with existing player control; whether skills, teleport, healing, invulnerability, dash or movement buffs are restricted to protect intended dodge difficulty.
4. **Maze format:** single continuous marked trail vs two or three short sections with different flame timing patterns, total ~5min.
5. Quest completion: reach finish then auto exit/advance or speak with pyromancer after exit; replayability/retry rules.

### All previously approved combat rules RETAINED

- Lv1 Fire Mage stat VIT19 STR7 AGI12 INT33 SPR13; per-level VIT+2 STR+0.75 AGI+1 INT+6 SPR+1.5, baseMana160; universal maxMana=160+5INT, regen4+0.1SPR.
- Six purchased skill caps Fireball35, Ward25, Ignite30, Bomb30, Overheat Mastery15, Infernal Presence15; 150 rank point budget, exact rank gate table user approved Round18, rank0 initial.
- Fireball/ Ignite/ Bomb/Ward approved Mana formulas `20+15r`, `30+15r`, `50+15r`, `10+6r` (Ward also consumes Mana per prevented HP). Cooldowns all linear rank-dependent14→7/14→10/22→15/28→20; casts0.2s/instant/0.2s/instant.
- Fireball raw Magical Fire `30+8r+(0.3+0.04r)INT`, empowered ground4m six ticks each exactly25% initial raw, fully snapshots offensive bonuses, independently overlapping. Ignite own independent raw tick `9.5+68*(r-1)/29+[0.085+0.34*(r-1)/29]INT`,8sec8 ticks, empowered3x, one per caster/target keeps stronger actual snapshot tick potency on recast, separate caster ignites coexist. Bomb own independent snapshot raw3m AoE detonation `57+408*(r-1)/29+[0.51+2.04*(r-1)/29]INT`,3sec fuse, empowered original2x and one-hop full NORMAL secondary AoE3m bombs, independent stacking, ALL bombs on attached dead target explode immediately independently at death location. No innate Fire Mage ability crits; potential external late-game spell-crit items FUTURE only.
- Flame Ward timed self-only Mana redirection with rank-dependent percent and efficiency and empowered separate Fire elemental shield global rules; Heat gauge ordinary cast gain10+OverheatMastery rank and100-spend empowered next cast; 15m Fire resistance Infernal Presence aura strongest matching aura only.
- TierIV Pyromancer Fire LAZOR/toggle Ward/possible Meteor/merged passive+aura, later Frost Mage→Cryomancer and hidden Hydromancer at both Fire and Frost Mage Lv120 all FUTURE IDEA BANK, not current implementation.

**DESIGN DOCS ONLY. Do not modify Unity code or scenes/assets, run builds or merge.**


## Fire Mage Tier III Round 24 — 2026-11-26 USER APPROVES EMBER CROWN BASE CAMP, SURVIVABLE HEAVY HAZARDS, NO ABILITIES, SINGLE COURSE, MENTOR TURN-IN

**LATEST USER ROUND24 ANSWERS, ALL APPROVED AS WRITTEN:**
1. **Mentor placement:** NEW experienced controlled-fire pyromancer NPC resides at a small, SAFE **training camp at the BASE of the Ember Crown volcano**, World2 southeast Ashfall Reach. This refines the Round23 "at the volcano in World2" instruction; **not** Trimery, crater rim or halfway overlook. Exact camp world coordinates, approach road, NPC name, character art/voice and exact camp props still to be designed. Existing volcanic landmark is preserved, not replaced or moved. This should remain a reachable safe outdoor interaction point with entry into a separate trial SOLO instance.
2. **Fire/lava contact consequences:** Hazard failure model is **HEAVY DAMAGE, but not guaranteed immediate death or an automatic teleport**. Player **continues walking if they survive**, allowing recovery if a player can escape the hazardous tile or flame. Do NOT instantly return a living player to start/checkpoint just for touching fire/lava; do NOT silently substitute one-shot traps or instant kill for the approved heavy-damage rule. The precise damage magnitude, whether lava deals repeated periodic damage while standing in it, Fire-element versus environmental/pure damage classification, resistance interaction, telegraph/damage cadence, and behavior upon actual HP=0 remain OPEN; use ordinary combat rules unless later overridden. The ability restriction below means this trial must remain realistically survivable with Mage TierII Lv60 baseline health and no skill healing.
3. **NO ABILITIES DURING MAZE:** While within solo trial instance, player CANNOT use **any class skills/spells, teleport/return/home abilities, movement skills/dashes/blinks**, or other activated combat abilities to bypass hazard route. Walking controls remain active using the existing primary mouse movement and optional WASD per normal player settings, not an auto-path action that walks into lava. Whether equipment consumables, passive bonuses, innate regeneration, and inventory menus are temporarily disabled remains unspecified, so do not invent exceptions or extra bans without asking. Consider strictly restore regular controls/ability permissions on exit, death or disconnect; do not modify permanent ability rank data.
4. **Maze layout:** ONE SINGLE **CONTINUOUS** Point A→Point B obstacle course, not three separate chapters or randomized reset segments. Approximate active target completion time **5 minutes** established Round23, solo. Continuous path uses visibly safe walkable ground and dangerous lava zones, timed fire bursts/walls and movement/avoidance patterns. Hazard variety along route is fine, but no required intermediary rooms or arbitrary stages, no boss battle, target shooting or item-gathering objective. Exact art/layout, hazard timings, obstacle spacing and safe ground width remain designer work awaiting approval/iteration.
5. **Quest completion:** Reaching Point B **completes the obstacle trial** but DOES NOT automatically convert the character to TierIII Fire Mage at exit. Player leaves the instance and must **REPORT BACK to the same Fire-specialist PYROMANCER MENTOR at Ember Crown base camp** to turn in quest and receive class advancement, under normal advancement validation. Need persist success flag reliably across instance exit/disconnect so successful players don't repeat solely to turn in, and prevent duplicate turn-ins under existing quest flow. Exact extraction interaction at B and return location still to design. Eligibility Mage TierII Lv60+ retained. Completion condition is reaching B alive, not a kill count or DPS check.

### Specific gameplay implications to preserve

- This is a movement-survival escape-room in the spirit of WarcraftIII Run Kitty Run / Pyramid Escape, **not a three-stage dungeon**, with one simple traversal goal and no abilities.
- Heavy damage is **survivable** by design on a reasonable mistake, but repeated hazard damage might kill. A **death/failure/retry policy has NOT been approved**. Do NOT assume live players checkpoint teleport, full level reset or permanently fail the quest until decided.
- Since abilities are disabled, do not demand combat, self-healing, active wards or fire resistance spells to progress. Other active Fire Mage skills cannot be prerequisites because player is STILL the Mage TierII during the trial.
- The volcano is existing **Ember Crown**, World2 Ashfall Reach southeast; World2 access is currently tied to the Ancient Road Dragon progression through southern Ironpine pass. Verify Mage Lv60 class advancement gate and World2 story access can coexist; avoid new shortcut/portal assumptions.
- Instance is solo: other players' positions, combat packets, buffs/wards and party follow mechanics should not solve the route for its owner. Underlying multiplayer isolation and ability lock must follow engine patterns when implemented.

### Follow-up Round25 ideas (DESIGNER SUGGESTIONS, NOT USER APPROVED)

1. **Hazard damage first-test target:** e.g., player loses 20–30% MAX HP for a brief fire-burst mistake; lava does a clear periodic damage tick while remaining exposed. User selected HEAVY but survivable, so choose actual damage rules after comparing TierII Mage health/resistances and existing hazard pipeline.
2. **True death/retry rule:** on HP0, respawn at mentor's camp entrance and restart course vs entrance to instance/checkpoint, no quest lockout. A normal living player only takes damage and continues, never auto-reset on touching hazard.
3. **Environmental hazard telegraph**: warmup/fire pulse interval, lava safety marking, traffic lane widths, and whether deterministic patterns or random timers. Favor readable repeated patterns to keep 5-minute completion possible with no mobility skills.
4. **No abilities nuance:** whether consumable healing potions, out-of-combat natural HP regen and defensive equipment passives still function or should be disabled; prevent unintended bypasses without invalidating player gear.
5. **NPC identity**: name, appearance, dialogue and test theme; checkpointless single run vs saved halfway marker and exact turn-in after Point B.

### Fire Mage TierIII canonical design unaffected

- New TierIII Lv1 VIT19 STR7 AGI12 INT33 SPR13; growth VIT+2 STR+0.75 AGI+1 INT+6 SPR+1.5, ClassBaseMana160, shared MaxMana=160+5INT, regen/s=4+.1SPR.
- Six skills/rank caps Fireball35, FlameWard25, Ignite30, LivingBomb30, OverheatMastery15, InfernalPresence15; exact 150-rank min-level gate table approved Round18; one point/level1..150 from rank0.
- Mana cast cost Fireball20+15r, Ignite30+15r, LivingBomb50+15r, Ward10+6r upfront plus ongoing Mana-per-prevented-HP, empowered casts normal Mana+100 Heat; Heat gain10+passive rank on successful ordinary active Fire cast incl Ward.
- Direct Fireball raw `30+8r+(0.3+0.04r)*INT`, empowered ground ticks 0.25× projectile raw each t1..6s in4m fixed independently overlapping patches with rank/INT/full offensive bonus snapshot.
- Ignite independent raw tick `9.5+68*(r-1)/29+(0.085+0.34*(r-1)/29)*INT`, eight ticks in8s, empowered3×; one Ignite per caster on target, recast refreshes duration/timer retaining strongest actual snapshot tick even if older normal buff stronger; different casters coexist.
- Living Bomb independent normal RAW 3m AoE explosion `57+408*(r-1)/29+(0.51+2.04*(r-1)/29)*INT`, 3s fuse, empowered original2× plus one generation of normal secondary 3m AoE bombs, all independent instances may stack; any bomb-bearing target death immediately detonates every attached bomb at death location, no recursive secondary propagation.
- Four actives cast timings Fireball0.2s, Ignite instant, Bomb0.2s, Ward instant self-only; linear CDs Fireball14→7, Ignite14→10, Bomb22→15, Ward28→20, each scales own purchased rank.
- Flame Ward main Mana redirection30→65% rank1..25, duration4→10s, 1.9→1.3 Mana per HP prevented, additional empowered true Fire elemental shield capacity `50+(0.5+0.1r)*INT` with10s life; matching Fire0.8 shieldHP/HP absorbed, other eligible1.2, shield before Mana redirection.
- Heat maximum100, spends100 for empowered next eligible Fire active cast with no gain on that empowered cast; decay10/s after10s no successful Fire spell. Infernal Presence radius15m incl caster, allied Fire resist+0.5pp/rank, hostile -.5pp/rank, strongest same-name only.
- **NO inherent criticals from any Fire Mage ability**, potential future item spell crit exception unimplemented. TierIV Pyromancer, later Frost Mage/Cryomancer and hidden Hydromancer remain future design ideas.

**DOCS ONLY. No Unity gameplay, scene, asset, or build changes; no merge.**


## Fire Mage Tier III Round 25 — 2026-11-26: USER APPROVES 25%-MAX-HP FIRE BURSTS, LAVA EVERY SECOND, ENTRANCE RETRY, CONSUMABLES, REPEATING WARNED HAZARDS; SHORT DURATION NOT STRICT 5 MIN

**LATEST USER ANSWERS, authoritative over any prior designer defaults:**
1. **Fire burst damage exact USER APPROVED: 25% of the PLAYER'S MAXIMUM HP PER HIT.** This is a percentage of max HP, NOT fixed flat damage scaled to Mage Lv60 and NOT 25% of current HP. It is the first-test hazard's nominal hit severity. Whether environmental Fire resistance, ordinary mitigation, invulnerability or special damage rules modify the hit after deriving nominal raw 25%-max-HP amount remains to be reconciled with global combat system; do not silently create unapproved bypass mechanics. Each actual separately damaging burst contact applies its own hit only according to hazard overlap/rehit policy, which still needs a fair warning/cadence design.
2. **On death during maze**, USER APPROVES **respawn at the MAZE ENTRANCE and retry immediately**, rather than at mentor camp or a midway checkpoint. A LIVING PLAYER WHO SUFFERS FIRE/LAVA DAMAGE STILL CONTINUES from current location, consistent with Round24; only actual death invokes entrance respawn. Keep trial active for retries, no quest lockout, no mandatory camp revisit on death, and no auto class advancement. Respawn HP/consumable resource restoration and any anti-exploit safeguards require later user design.
3. **Lava is a hazard which damages an exposed player ONCE PER SECOND while standing in it.** User selected this frequency, not an on-entry one-shot with temporary immunity. Its actual per-tick magnitude **NOT YET APPROVED**; do not copy the separately approved fire-burst25% max HP unless user explicitly approves that for lava too. Contact with lava does not automatically teleport/restart a surviving player. Use deterministic, clearly visible per-player one-second damage intervals so fractional overlaps do not multiply pulses accidentally; exact initial tick at entry vs after first full elapsed second still open.
4. **Healing potions and normal consumables ALLOWED** during the challenge. This EXPLICITLY OVERRIDES any proposed ban on consumables from prior round. Normal walking remains allowed, while **abilities/spells/teleports/activated movement skills remain disabled**. Passive gear effects and any normal underlying potion usage rules should remain under existing inventory/resource system; do not invent extra consumable restrictions, infinite free potions, free refills or bypass gadgets. If some consumable duplicates an expressly prohibited activated mobility/teleport ability, specific policy requires later clarification rather than globally disabling ALL consumables.
5. **Fire hazard timing is PREDICTABLE, REPEATING and CLEARLY TELEGRAPHED.** Pattern warns before each damaging wave, with readable visible indicators on tiles/lanes. Not randomized per run or hidden untelegraphed punishments. A player's run can be learned and improved through timing, safe-path navigation and movement.
6. **User duration correction (important):** "**it doesnt need to be exactly 5 minutes, but yeah, a shorter maze thing**". Thus approximate FIVE MINUTES from Rounds23–24 is now **soft creative context ONLY**, NOT a strict target/stopwatch, forced timer, guaranteed finish time, time limit or minimum/maximum. Make it a **SHORT solo obstacle course**, driven by traversing a continuous Point A→Point B marked route; different players may take more/less time due to caution, retries and potion use.

### Locked advancement venue and completion from earlier rounds

- New experienced controlled-fire **pyromancer specialist NPC** at small SAFE training CAMP at BASE of existing Ember Crown volcano, southeast World2 Ashfall Reach, not Trimery. TierII Mage Lv60+ eligible for advancement; World2 passage normally accessible via southern Ironpine pass following Ancient Road Dragon story progress.
- **Solo instanced** ONE CONTINUOUS fire/lava walk-and-dodge hazard maze inspired by classic WC3 Run Kitty Run / Pyramid Escape, no discrete three sections, no boss DPS/fetch task.
- All activated class abilities, movement skills and teleports DISABLED during instance; normal chosen movement controls allowed. Healing potions and normal consumables NOW EXPRESSLY ALLOWED.
- **Win** by reaching Point B alive, leave maze and **RETURN TO FIRE MENTOR AT CAMP to turn in**, upon which character changes to Fire Mage TierIII new Lv1 manifestation; exiting alone doesn't auto-advance.
- Death before B is entrance respawn/immediate retry. Ordinary damaging mistakes do NOT reset player if HP remains >0.

### Suggested Round26 details, NOT YET APPROVED

1. **Lava damage magnitude**: choose, e.g., 8%, 10%, 12% or 15% of MAX HP per one-second tick, balancing fire bursts25%, potion use, TierII Mage Lv60 survival and expected recovery. Avoid unapproved25%/sec equivalence.
2. **Fire burst trigger frequency**: while standing in a burst, one hit per specific hazard pulse vs multi-hit overlaps; prefer deliberate hits with clear warnings, no frame-based repeated HP loss.
3. **First lava tick timing**: immediate on entry or at 1.0sec continuous exposure, and reset tick clock upon exit/re-entry.
4. **Respawn resources**: restore full HP/Mana on maze entrance death retry, preserve spent potions and earned quest progress, or use existing death/respawn rules; user hasn't decided.
5. **Mentor identity and trial title**: NPC name and tone, quest name, details of camp and escape visuals; distinguish designer working titles from signed-off names.
6. **World2 accessibility**: confirm intentional progress requirement for TierIII mentor in World2 given main World2 access after World1 dragon.

### Existing approved Fire Mage combat systems preserved, no change

- New TierIII Lv1 VIT19 STR7 AGI12 INT33 SPR13, per-level gains2/.75/1/6/1.5 and ClassBaseMana160, universal MaxMana=160+5INT, regen/s4+.1SPR.
- Six skills Fireball cap35, FlameWard25, Ignite30, LivingBomb30, Overheat Mastery15, Infernal Presence15; exact150 purchased rank level-gate table approved Round18; one rank point/classLv1..150, rank0 starter.
- Mana casts Fireball20+15r, Ignite30+15r, LivingBomb50+15r, Ward10+6r upfront plus Mana redirect while protecting. Normal empowered cast mana+100 Heat.
- Fireball normal direct Fire Magical raw `30+8r+(0.3+0.04r)*INT`; empowered4m ground six ticks each25% own projectile raw, snapshots rank/INT/ALL outgoing damage bonuses at patch creation and overlapping areas independently tick.
- Ignite own rank tick `9.5+68*(r-1)/29+(0.085+0.34*(r-1)/29)*INT`,8 ticks over8s, empowered3x; same-caster recast refreshes timer/duration and strongest actual captured potency wins, multiple caster-owned Ignite effects coexist.
- Living Bomb own rank normal3m AoE raw `57+408*(r-1)/29+(0.51+2.04*(r-1)/29)*INT`,3s fuse; empowered original detonation2x and creates one generation normal AoE3m bombs. Independent bombs may all coexist; target death causes immediate 3m AoE detonation of all attached bombs individually, no extra propagation from secondary.
- Four active casts Fireball0.2s, Ignite instant, Bomb0.2s, Ward instant self-only, cooldowns 14→7s/14→10s/22→15s/28→20s linear across own purchased ranks.
- Ward self-only timed Mana damage redirection, 30→65% incoming HP to Mana, 1.9→1.3 Mana per prevented HP, duration4→10s, bonus empowered genuine Fire elemental shield `50+(0.5+0.1r)*INT` capacity/10s, global matching Fire0.8 shieldHP per prevented damage and other ordinary1.2, shield absorbs first then Mana Ward then HP.
- Heat successful normal active Fire cast incl Ward grants10+Overheat Mastery purchased rank, max100, next eligible empowered cast spends100 no same-cast gain, after10s inactivity lose10 Heat/s. Infernal Presence permanent15m aura +0.5pp/rank Fire resistance allies including self, -0.5pp/rank enemies, identical aura strongest only.
- **No innate crits from any Fire Mage abilities**; potential explicitly future spell-crit items only. TierIV Pyromancer ideas Fire LAZOR/Meteor/toggle Ward/combined passive+aura, Frost Mage→Cryomancer and hidden Hydromancer at Fire+FrostMage Lv120 remain future ideas only.

**DOCUMENTATION ONLY; no Unity code, scenes, assets, builds, tests or branch merge.**


## Fire Mage Tier III Round 26 — 2026-11-26 USER APPROVES LAVA 15% MAX HP INSTANT+1S, REPEATING FIRE 25% MAX HP, FULL HP/MANA RETRIES, CODEX CHOOSES MENTOR NAME

**LATEST USER ANSWERS, authoritative:**
1. **LAVA DAMAGE APPROVED:** each qualifying lava tick deals **15% of the player's MAXIMUM HP** (not current HP), immediately upon ENTERING lava and then **EVERY 1.0 SECOND while exposed**. This overrides earlier Round25 lava magnitude/tick-origin TBD, preserving the already approved sustained-exposure interval. When the player leaves lava, stop ongoing exposure ticks; on a fresh entry apply the entry tick again. Exact damage-channel/mitigation policy, potential minor collision jitter grace/edge handling, and whether multiple adjacent overlapping lava colliders count as ONE continuous exposure are integration details to make robust, not additional user-authorized damage multipliers. Living players never get automatically teleported for stepping into lava.
2. **FIRE BURSTS REPEATED DAMAGE APPROVED:** fire burst hits each deal exactly **25% PLAYER MAX HP** nominal hit severity, **REPEATED WHILE THE PLAYER REMAINS INSIDE ACTIVE FIRE** subject to a **FIXED re-hit cooldown**. This supersedes earlier designer suggestion one hit per distinct pulse only. **Cooldown duration NOT yet specified by user**: do NOT silently hard-code 1s solely because lava ticks once/sec, or claim a specific cooldown is approved. Fire patterns still must be predictable, cyclic and visibly telegraphed with safe lanes; fixed repeat cooldown prevents per-frame damage. Damage frequency must be fair versus25% max-HP hits and usual TierII Mage survivability. If the fire turns off, no hits while inactive; entering an active burst should follow the chosen fixed cadence. Specify first-hit rehit design separately in next round if necessary.
3. **DEATH / RETRY RESOURCES APPROVED:** if HP actually reaches0 during the SOLO maze, character **respawns immediately at the MAZE ENTRANCE, with FULL HP and FULL MANA**, ready to retry the same single continuous Point A→Point B route. This is the respawn location from Round25, now with explicit resource refill. **All consumables already spent STAY SPENT**, inventory costs persist: retry is not a potion refund, inventory reset or quest lockout. Do not duplicate items, roll back save, or invoke a new world instance that restores spent consumables. No manual return to mentor for retries; reporting back to mentor is required ONLY after a successful exit.
4. **FIRE MENTOR NAME/LOOK delegated to CODEX:** User chooses "Design name/appearance later" then adds "**let codex decide the name of the npc**." Therefore **do not freeze Emberis, Master Cindervale or any designer-suggested name as canonical**; CODEx is AUTHORISED TO CHOOSE THE NPC NAME during future implementation/handover. Appearance is not explicitly delegated by additional note except "name", so preserve NPC visual archetype as an experienced controlled-fire pyromancer and leave exact appearance/asset selection open for design/asset work; Codex may suggest suitable visuals, but name choice is expressly delegated. NPC location already approved: safe training camp at BASE of existing **Ember Crown VOLCANO** southeast **World2 Ashfall Reach**, NOT Trimery and NOT crater rim.
5. **TRIAL DURATION FLEXIBLE:** user earlier clarified it does NOT need to be exactly5 minutes; preserve **SHORT continuous SOLO fire/lava-dodging maze**, no fixed countdown or forced elapsed target. All active class abilities, movement spells/dashes and teleports are disabled during the instance, while normal walking and **healing potions/ordinary consumables** remain allowed. Reaching exit B registers completion; speak to fire mentor at camp for Mage TierII Lv60+ → TierIII Fire Mage Lv1 advancement.

### Important implementation test expectations for later Codex handoff (GUIDANCE only, no code authorized this round)

- Entering lava: first **15% MaxHP tick immediately** and, if still exposed, another **15% MaxHP at each following 1.0 second**. Exit prevents future ticks; re-entry is fresh contact.
- Being hit by active fire: 25% MaxHP nominal hit, then only repeat when fixed per-fire/exposure damage cooldown has elapsed; no frame-based hits or damage from inactive warning telegraphs. **Choose exact fixed re-hit cooldown after user signoff.**
- If damage would kill player, handle death via existing safe state flow, refill HP/Mana at entrance, do not refund consumed potions. Repeated deaths do not erase the accepted quest, allow immediate retries.
- Hazards have predictable repeating visible warning sequences. Combat spell lock must not disable normal inventory consumables, nor leak permanently to the player's main world after leaving/dying/disconnecting.
- Damage percentages are nominal MAX HP scaled values. **Interaction with existing magical Fire resistance/defenses and health mitigation not separately specified**; remain under global mechanics unless expressly revised. No unsupported assumptions of Pure/true damage or special bypass.

### Already approved Fire Mage design kept unchanged

- T3 Fire Mage base stats VIT19 STR7 AGI12 INT33 SPR13, growth +2/+0.75/+1/+6/+1.5; ClassBaseMana160, universal maxMana=160+5INT, regen/s=4+0.1SPR, independent TierIII level1 manifestation.
- Six purchased skill caps Fireball35, Flame Ward25, Ignite30, Living Bomb30, Overheat Mastery15, Infernal Presence15; exact 150 rank minimum-level table from Round16 USER APPROVED in Round18. Earn 1 skill rank point per class level1..150, rank0 all starters.
- Fireball cast Mana20+15r, Ignite30+15r, Living Bomb50+15r, Ward10+6r UPFRONT plus Mana spent during redirected damage, empowered costs normal Mana plus100 Heat. Fireball 0.2s cast, Ignite instant, Bomb0.2s, Ward instant self only, cooldowns linear Fireball14→7s, Ignite14→10s, Bomb22→15s, Ward28→20s.
- Fireball normal raw `30+8r+(0.3+0.04r)*INT`, empowered fixed4m ground6s with six each exactly25% normal raw ticks and all outgoing damage bonus snapshot, independent overlapping patches. Ignite own independent RAW tick `9.5+68*(r-1)/29+(0.085+0.34*(r-1)/29)*INT`,8 ticks, empowered3×; same-caster refresh keeps strongest actual snapshotted tick potency; different caster Ignite coexist.
- Living Bomb own independent RAW3m AoE `57+408*(r-1)/29+(0.51+2.04*(r-1)/29)*INT`, 3s fuse, empowered original2× with one-hop NORMAL full AoE secondaries, all instances independent, all bombs on an enemy detonate immediately and separately at its death location, including chain reaction, no secondary re-propagation. Offense snapshot on application.
- Flame Ward Mana redirect30→65% for4→10s, Mana/HP prevented1.9→1.3, empowered true Fire elemental bonus shield `50+(0.5+0.1r)*INT` lasting10s and using global matched0.8 vs other eligible1.2 shieldHP-per-prevented-damage, elemental shield before Mana redirect then HP.
- Heat ordinary successful Fire cast incl Ward gives10+Overheat passive rank, cap100; next Fire cast empowered spends100 Heat with no new same-cast Heat, after10s no successful Fire cast decay10/sec. Infernal Presence radius15m permanent Fire resist ally incl caster +0.5 percentage point per rank, hostile -0.5pp per rank, identical aura strongest only.
- None of Fire Mage abilities naturally crit; potential FUTURE explicitly spell-crit gear only. Future Pyromancer TierIV, Frost Mage/Cryomancer and secret Hydromancer are idea bank only.

### Next design options

1. Select exact fixed active-fire re-hit cooldown (e.g. 1.0, 1.5 or 2.0 seconds) with clear interaction with repeating visible hazard cycles. Do not silently impose lava's1s interval onto active fire.
2. Confirm hazard damage is standard Fire damage with global resist/defense or specially fixed post-mitigation percentage-maxHP environmental damage; avoid accidental double percent scaling.
3. Clarify lava leaving/reentering rapid edge cases, contact damage on invulnerable/respawning players and anti-spawn-camping.
4. Decide whether World2 quest-access story gate can stay in place before Mage TierII Lv60 advancement (dragon/world2 story already defined); do not invent portal bypass.
5. Codex NPC name delegated; choose trial naming and minimal distinctive visual/audio requirements and implementation handoff WHEN user requests implementation, not in this docs-only round.

**Documentation only; no Unity gameplay code, scenes, assets, builds, tests or branch merges.**

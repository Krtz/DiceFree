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

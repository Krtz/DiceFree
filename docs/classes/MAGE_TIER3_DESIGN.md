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

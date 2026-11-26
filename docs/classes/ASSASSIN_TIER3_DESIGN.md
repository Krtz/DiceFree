# Assassin Tier III — Thief evolution design

**Opened:** 2026-11-26
**Status:** Round 1 USER-APPROVED core identity only; advanced mechanics, exact skill roster, values, ranks, gates and quest TBD. Not an implementation assertion.
**Parent:** Thief, normal Tier II from Physically Blessed Novice
**Evolution:** Assassin, Tier III (first Thief-branch coverage choice)
**Related:** [Thief Tier II canonical mechanics](THIEF_TIER2_DESIGN.md), [Tier III coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md), [Class progression](../CLASS_PROGRESSION.md)

## Round 1 — explicit user decisions (APPROVED)

1. **First Tier III Thief evolution is ASSASSIN.** This resolves first coverage for Thief; does NOT disallow later Duelist/Burglar siblings.
2. **Role: PURE MELEE DPS**, not evasion tank nor DPS/utility hybrid as the primary role.
3. **Weapon family: DUAL ONE-HANDED DAGGERS**, mainhand and offhand. Preserve ordinary equipped-dagger stat contribution. Parent Thief's alternating left/right basic attack ANIMATION with a SINGLE basic attack event is the baseline; do not silently grant extra independent offhand hits or item procs on Assassin without subsequent approval.
4. **Resource: MANA ONLY.** Use approved globally shared Mana framework: MaxMana = ClassBaseMana + 5*INT; ManaRegenPerSecond = 4 + 0.1*SPR, where Assassin's own ClassBaseMana, starting stats and per-level growth are still OPEN. No Combo, Energy, Rage, Guile, Stamina or new secondary gauge.
5. **Skills: EVOLVE SELECTED Tier II THIEF ABILITIES AND REPLACE OTHERS WITH NEW SKILLS.** User has NOT yet decided which specific skills survive, which transform, which disappear or what new skills are approved. Do not claim a detailed kit exists.

## Earlier user-established class fantasy to respect (idea bank, not new Round1 skill approvals)

- From earlier October Thief TierII brainstorming, **Assassin** was the **slower, deliberate and devastating** branch: stealth, precision, positional attacks and lethal opening blows, *distinct from* nimble Duelist's evasion/quick repeated attacks and Burglar's poison/dirty-trick battlefield control. Backstabs/rear positional bonus were specifically reserved for a LATER class, not approved on TierII Thief. Here positional gameplay is a candidate for Assassin only, requiring a concrete future approval.
- This should not accidentally become a simple fast-attack Duelist or poison/spread-focused Burglar. Retaining a little existing Thief identity is allowed, but exact mechanics remain undecided.
- General TierII→TierIII framework: once Thief class Lv60+ and relevant discovery/advancement requirements are fulfilled, become separate Assassin class Lv1 rather than adding parent class levels/stats. Actual Assassin advancement NPC, quest, unlock checks and location not chosen here. Don't assume they match Fire Mage's World2 volcanic escape.
- The user wants **at least one TierIII path per TierII parent** in this coverage pass, not a permanent one-branch-per-parent limitation.

## Parent Thief reference: exact existing approved Tier II mechanics (NOT automatically Assassin Tier III skills)

- Equipped two one-handed daggers, ordinary equipment stat sticks unless an individually authored unique effect exists; single ordinary basic attack event with alternating dagger animations; offensive **AGI-only** Thief skill scaling, STR not a skill coefficient. Assassin's own stat emphasis to decide, though likely AGI, not yet approved numerically.
- Six TierII skills: **Stealth16, Envenom18, Guaranteed-Basic-Crit Buff10, Fan of Knives18, Smoke Bomb10, Crit+AGI Passive8**, 80 total Thief rank points and audited parent rank gates. Those 80 TierII ranks/limits MUST NOT be blindly copied into 150-rank TierIII Assassin.
- **Stealth** active in combat: approximately 4→8 seconds, clears95% of threat on activation (remaining5% survives enemy reacquisition), incoming damage doesn't itself reveal. First breaking BASIC or ENVEnom direct stab consumes a rank-dependent1.1x→2.0x raw opener, even on miss or full shield absorption; Envenom's attached Poison ticks are NOT opener-amplified. Ordinary movement and potions preserve stealth; actual looting/NPC/world-object interactions reveal.
- **Guaranteed basic crit active** can be cast without revealing stealth, duration1→4s ticks down normally while hidden; basic attacks only, NOT Envenom or other skills. Passive +1AGI and +1pp BASIC crit chance per purchased rank, base normal-basic crit damage1.5x.
- **Envenom** instant direct dagger stab2× normal basic RAW non-elemental Physical, plus3sec nonstacking physical Nature-tag Poison (1 tick/s; refresh preserves clock), with own AGI/rank coefficient.
- **Fan of Knives** 4m 360-degree Physical one hit per enemy, usable while moving, AGI/rank raw formula; **Smoke Bomb** ground-targeted4m radius5sec Physical Tech quarter-second DoT with 20% enemy BASIC attack miss chance only inside cloud; overlapping Smoke zones nonstacking.
- Thief first-test CDs/Mana: Stealth24sec/20 Mana, Envenom8sec/12, GuaranteedCrit18sec/16, Fan9sec/18, Smoke18sec/20. Parent values are context, NOT approved Assassin inheritance.
- Reference: [Thief TierII design](THIEF_TIER2_DESIGN.md) Round8 authoritative section overrides all older historical contradictions.

## Future-only sibling/secret-lineage concepts (NOT approved Assassin mechanics)

- **Duelist TierIII**: two daggers, fast repeated attacks, evasion/tempo; possible later separate TierIV continuation.
- **Burglar TierIII**: poison and dirty tricks; speculative **Pirate TierIV** continuation.
- **Secret Berserker+Duelist same-Echo convergence** after high progression milestones (e.g. both TierIII Lv120, or TierIV achievements, exact requirement open) possibly wielding **two swords** or even two **two-handed swords**. No approved tier, mechanics, unlocking quest or fixed weapon permissions.

## Round 2 proposed design interview (ALL ANSWERS PENDING)

1. **Assassin combat rhythm:** deliberate stealth-heavy burst with reposition/backstabs, a stealth+execute cycle, or a balanced sustained DPS between kill windows? Confirm amount of slower windup, positioning and target access desired.
2. **Selected Thief skills to evolve:** Stealth and Envenom are natural candidates but must ask user whether to keep, replace or merge each; decide which of guaranteed crit, Fan, Smoke and passive remain.
3. **Positional damage:** rear attacks only, flank+rear differentiated, or positional damage as situational reward rather than mandatory to function against bosses, solo players and immobile enemies.
4. **Core execution mechanic:** marked vulnerable target, delayed high-damage strike, stealth opener multipliers, or a new separate finisher. Do not add a SECOND resource without rescinding Mana-only approval.
5. **Attack cadence and crit scope:** keep one alternating dagger basic event from parent, choose slower basic attacks vs slower hard-hitting SKILLS and whether Assassins gain special skill crit permission (Fire Mage's no-native-ability-crit constraint is class-specific, not global).

## Implementation boundaries

**DOCUMENTATION ONLY.** No Unity gameplay code, assets/scenes, tests, builds, or branch merges are authorized in this Round1 design conversation. Codex may later implement once design/requirements approved by the user. The user's 2026-11-26 Fire Mage final acceptance closes that Mage design phase and does not imply all older roadmap prose is current.


## Round 2 — 2026-11-26 USER-APPROVED Assassin damage identity and 270-degree position zone

**The user explicitly selected the following, and their free-text note OVERRIDES an overly strict literal behind-only reading:**

1. **Damage pattern = VERY STRONG STEALTH OPENING BURST, followed by WEAKER sustained damage** until the next opening opportunity. Do not instead design balanced uniform sustained melee output, a mandatory buildup-only execute resource, or the fast repeated-strike Duelist identity. Exact damage budget / DPS ratios are UNDECIDED.
2. **EVOLVE TierII STEALTH rather than merely retaining unchanged stealth or replacing stealth wholesale.** The Assassin variant should include positional-damage interaction with stealth/ambush; exact bonuses, cooldowns, concealment duration, threat reset/reveal rules, and how an empowered Backstab consumes the opener remain UNDECIDED. Parent Thief rules (95% threat reduction, damage received does not reveal, selected stealth-breaking actions) are existing background; do not accidentally inherit all of them as newly user-locked Assassin mechanics before specific discussion.
3. **KEEP ENVENOM as a SEPARATE ability AND ADD BACKSTAB as its own NEW ability.** Backstab is not the replacement or renamed Envenom. Earlier Thief Envenom was direct stab plus nonstacking Physical Nature Poison; its precise Assassin damage/potency/skill ranks and potential stealth opener interplay remain OPEN. Do not remove Poison or merge Backstab into Envenom without later user instruction.
4. **Positional bonus from ALL SIDES AND REAR, qualifying in a forgiving 270-DEGREE ARC** around the enemy, rather than only the narrow behind axis. User's exact clarification: "**back and sides should both give the position bonus to make the system a little more forgiving. so anything in the 270º that is not the front counts as 'the back' or whatever.**" Interpret geometrically as **a 90-degree frontal nonbonus CONE centered on TARGET'S FORWARD vector**, with the complementary **270-degree side/rear arc awarding the SAME positional bonus**. No distinct stronger rear bonus or weaker flank bonus was authorized, and do not use attacker's facing to decide target-facing positional eligibility. Design candidate mathematical test for attacker position relative to target forward: `dot(normalize(attackerPosition-targetPosition), targetForward) <= cos(45deg)` means QUALIFIED side/rear, ignoring numerical boundary tolerance; `dot > cos45deg` means unqualified front90. Treat exact boundary numerics/lag/moving-target snapshot as IMPLEMENTATION detail, not user-signed new geometry rule. A side or behind attack is colloquially called "back" for proc eligibility, but don't require true geometrical rear.
5. **BOTH BASIC ATTACKS AND HARD-HITTING SKILLS are SLOWER** compared to TierII Thief. Slower **basics** and deliberate, weightier skills are both part of the class identity; not simply normal fast basics with one slower skill. To counterweight, each hit should feel correspondingly powerful, especially stealth/positional openings. Exact base basic attack interval, impact coefficients, casts/windups, ability timings and balancing are TBD, with physical damage scaling/AGI bias subject to forthcoming numeric design.
6. Core Round1 identity remains **PURE MELEE DPS**, **DUAL ONE-HANDED DAGGERS**, **MANA ONLY** (global `ClassBaseMana+5INT` Max Mana / `4+0.1SPR` natural Mana regen), evolve some TierII abilities and replace others. TierII Thief Lv60+ → new separate TierIII Assassin Lv1 under shared class-progression rules. No extra Energy/Combo etc. No new equipment family approved.
7. **ENEMY FACINGS and solo accessibility**: the positional mechanic uses TARGET forward. Whether positional bonus should be obtainable when the player is solo holding target aggro, if attacking stationary bosses or facing-turning targets, and whether certain skills can forcibly reposition/disorient remain open. No special unconditional boss exception or automatic qualifying "back" for all attacks has been approved.

### Derived current Assassin skill-status ledger (only explicit confirmations marked)

| Ability / theme | Round2 status |
| --- | --- |
| **Evolved Stealth** | **USER APPROVED identity** and positional interaction; exact move name, behavior and numerical effects pending |
| **Envenom** | **USER APPROVED retention** as distinct skill, not replaced by Backstab; detailed TierIII revision pending |
| **Backstab** | **USER APPROVED new separate ability**; exact packet coefficients, special stealth interaction and positional restriction pending |
| Slower heavy BASIC attacks | **USER APPROVED rhythm**; dual daggers and one baseline attack-event pattern, exact interval/coefficients pending |
| Slower hard-hitting skills | **USER APPROVED rhythm**; exact cast/channel/windup and cooldown pending |
| Guaranteed Basic Crit buff | Parent Thief only; **Assassin status UNDECIDED** |
| Fan of Knives | Parent Thief only; **Assassin status UNDECIDED** |
| Smoke Bomb | Parent Thief only; **Assassin status UNDECIDED** |
| Crit/AGI passive | Parent Thief only; **Assassin status UNDECIDED** |

**Do not infer the above open skills are automatically removed; the user has not yet picked which remaining Thief skills evolve or get replaced.** Do not assert Assassin will have exactly six skills or exactly150 purchasable ranks unless approved later; Fire Mage and other TierIII specifications aren't universal class caps.

### Round3 proposed decision prompts (ALL PENDING)

1. **Backstab availability**: only usable from qualifying side/rear 270° (strict), usable from front at reduced power, or eligible from anywhere with a 270° positional damage bonus? How much stealth opener synergy?
2. **Stealth enhancement**: longer duration/stronger opener vs extra movement/approach options vs stealth-refresh from successful Backstab; how to avoid near-permanent invulnerability from boss aggro drops.
3. **Slow basic attacks**: default attack interval penalty vs higher per-hit coefficients. Should Assassin preserve Thief's single alternating dagger basic event with both daggers as stat sticks? No independent dual-wield procs unless new approval.
4. **Non-core carryovers**: decide future fate of Guaranteed Basic Crit buff, Fan of Knives, Smoke Bomb, Crit/AGI passive. Need explicit retain/change/replace decisions, avoid prematurely authoring slots.
5. **Crit & precise burst**: Assassin basic and skill crit scope, first stealth Backstab behavior, position checks, and whether party/solo access needs special design. Do not apply Fire Mage's class-specific no innate spell crit to Assassin.

**No Unity code, asset changes, build/test, or branch merge. Documentation only.**

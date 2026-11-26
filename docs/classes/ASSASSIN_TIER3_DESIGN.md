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

# DiceFree — Guardian Tier III Design Interview

**Started:** 2026-11-27
**Branch:** `feat/verdant-wilds`
**Status:** DESIGN INTERVIEW OPEN; Guardian selected by user as first Tier III coverage evolution for Tier II Defender. No Guardian skills/stats/quest/implementation yet approved beyond class identity/lineage.
**Sources:** [Defender Tier II design](DEFENDER_TIER2_DESIGN.md), [One-per-Tier-II coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md), [Class progression](../CLASS_PROGRESSION.md), [Tier II original concepts](../TIER2_CORE_CLASSES_DESIGN_V01.md).

## GUARDIAN-001 — User selected Defender -> Guardian as next Tier III design (2026-11-27)

User first corrected an erroneous proposal to design SECOND Thief Tier III (Duelist/Burglar) immediately after Assassin: the agreed priority is FIRST to develop **at least one Tier III per each existing eight Tier II classes**. Roadmap coverage status at this point: Psion -> Psionic Knight / Mirage Mage (implemented as reported), Ranger -> Marksman (implemented), Greatblade -> Berserker (implemented), Mage -> Fire Mage (implemented), Thief -> Assassin (DESIGN COMPLETE, not implemented), leaving **Defender, Priest and Acolyte** without first Tier III design. The user explicitly responds "**yeah, let's do guardian**" to doing Defender's first Tier III evolution.

**NOW APPROVED:** First Tier III option for Tier II **Defender is named GUARDIAN**, and this is the next design interview. Standard Tier III progression is new level1 Guardian manifestation from eligible TierII Defender level60+, retaining parent class. **Do not treat Guardian combat kit, specializations, mentor/quest, rank caps or implementation as already user approved.**

## Tier II Defender source baseline (not new Tier III user approvals)

- Sword+shield, STR/VIT frontline tank; solo viability important. Defender's resource Resolve 0..100, earned from eligible incoming PRE-MITIGATION enemy damage and expressly Resolve-generating abilities. No in-combat decay; outside combat decreases 1 Resolve per3sec. This is user-locked Tier II baseline; Guardian's inheritance/tuning still to choose.
- Existing Tier II abilities: Shield Bash (5m short-target dash, brief stun and interrupt, respects global out-of-range preference); Challenge (AoE genuine threat / taunt); Bulwark (spend Resolve for mitigation); Shield Sweep (frontal AoE, allied PDEF/MDEF buff `rank*min(5,distinctHitEnemies)` in same5m ally area for4sec, self included; same-caster replaces; different Defenders stack); Hold the Line (5m ally aura, +1 percentage point ALL elemental resistances/rank, same-identity strongest aura wins, personal resilience/Resolve emphasis); Iron Guard (2s unable to act, +500 Physical DEF AND +500 Magical DEF per TierII skill rank, spends20 Resolve, defense persists during CC, no stored damage at TierII).
- **Historical locked future Tier III mechanism** from `docs/classes/DEFENDER_TIER2_DESIGN.md` DEFENDER-016: upgraded Iron Guard-style stance for 2s stores ALL incoming PRE-MITIGATION damage while defensive buff runs and RELEASES accumulated stored damage afterward. This is reserved for future TierIII, **not yet explicitly assigned to Guardian by user**; designer recommends discussing whether to give Guardian this mechanic. Exact damage release single-target vs AoE, cost, cap, school and target selection OPEN.
- Previously floated Guardian = enhanced allied protection; Vanguard = eventual Defender DPS alternative; Templar/Paladin = magical martial alternative; Greatblade tank Juggernaut future. Guardian alone is NOW selected, but its exact ally-protection and active-skills are still open. Don't imply future alternatives deleted.
- TierII rank caps and gates are 80 totals 18/10/16/20/10/6; these **do not automatically transfer to TierIII**. Other TierIII typical rank pool 150 and one skillpoint/class level1..150, but class skills and rank caps must be separately designed.
- Global Mana values DO NOT automatically apply to Resolve classes; existing shared resource conventions should be respected. No unique new resource is user-approved.

## GUARDIAN Round1 questions (OPEN for next user reply)

1. **Main tank identity:** dedicated protective ally tank (designer recommendation) or more reactive retaliatory countertank, or a hybrid; unlike future Defender->Vanguard DPS.
2. **Iron Guard damage-storage mechanic:** make it signature Guardian evolution of Iron Guard (strongly recommended due to already reserved TierIII concept) or reserve for another Defender line; choose single-target or area release later.
3. **Offensive/defensive class skills:** retain and evolve most Defender tools vs replacing some with new ally-target shields, intercept or barriers. Class may have flexible skill count; do not assume six skills required.
4. **Weapon/resource:** default recommend sword+shield and evolved Resolve, but get user signoff, not auto-approved.
5. **Guardian advancement quest:** choose later once core kit, data formulas and progression locked; avoid adding multi-round complexity to straightforward class unlocks.

**No Unity changes, no model or art asset selection, no gameplay implementation, build/test or merge in this naming round.**


## GUARDIAN-002 — 2026-11-27 Round 1 USER APPROVED combat identity, stored-damage Iron Guard, mixed skill inheritance, sword/shield + Resolve

### Exact user selections

1. **Combat identity: Dedicated party protector.** Guardian is fundamentally a party-protection tank, built around shielding/intercepting damage and keeping nearby allies alive, while remaining able to do viable solo gameplay. Do NOT reinterpret primary Guardian role as reactive counterattack DPS, pure self-only durability, or the future Defender -> Vanguard damage evolution.
2. **Stored-damage Iron Guard evolution: GIVE STORED-DAMAGE RETALIATION TO GUARDIAN.** Guardian now explicitly owns the historically user-approved future TierIII Defender Iron Guard concept: the approximately two-second defensive action-lock stance with increased Physical + Magical Defense also **records incoming eligible PRE-MITIGATION hostile damage while active and RELEASES stored damage afterward**. The parent Defender TierII Iron Guard has NO damage-storage/release; it remains unchanged. Stored-damage source, attribution, outgoing damage multiplier, element, per-hit/stored-total caps, and whether release is targeted/AoE are NOT yet selected for Guardian. The earlier suggestion of a new class identity counterattack tank was NOT chosen; retaliation is a signature tool subordinate to party protection. The existing TierII approved details are +500 flat PDEF AND MDEF per original IronGuard skill rank, 2s unable to voluntarily act, cost20 Resolve, full DEF persists through CC; **Guardian's new per-rank DEF, Resolve cost, cooldown, target mode and release balance still need their own decisions, not automatic numeric copies**.
3. **Skill inheritance: RETAIN KEY DEFENDER SKILLS AND ADD NEW ALLY-PROTECTION SKILLS.** User did not choose "evolve most" nor "mostly new". Preserve the core shield-tank/taunt/Resolve-combat identity; select which of Shield Bash, Challenge, Bulwark, Shield Sweep, Hold the Line and Iron Guard survive or transform together in later interview. No complete skill roster, number of skills, individual new skills, rank caps or rank gates have been user approved yet; do not assume the count must be six.
4. **Equipment/resource: KEEP ONE-HANDED SWORD + SHIELD AND RESOLVE.** No two-handed shield fantasy, new Mana mechanic or new resource meter. The TierII Resolve parent model (0..100 cap, incoming pre-mitigation hit gain, tagged skill gains, no combat decay and 1/3seconds outside-combat decay) is the established baseline. User's selection means Guardian retains RESOLVE, not necessarily that all numeric resource generation and decay details are permanently fixed for TierIII. New class stats (STR/VIT distribution, level1 and growth), Mana exclusion, and available equipment categories will be studied later without inventing user approvals.

### Existing system references, constraints and open questions

Parent Defender TierII sword+shield skills: Shield Bash (5m targeted physical charge/stun/interrupt), Challenge (AoE real aggro), Bulwark (Resolve mitigation spender), Shield Sweep (AoE and ally 5m PDEF/MDEF per target struck), Hold the Line (5m all-elemental resist aura + personal defense), Iron Guard (2s +500 PDEF/MDEF/rank, action lock, no stored damage). See [Defender source baseline](DEFENDER_TIER2_DESIGN.md). There is no need to copy every parent TierII skill unchanged: selection intentionally hybridizes inherited key mechanics with NEW party-protection tools.

Potential NEW ally tools for discussion only:
- **Intercede/Intercept**: guard a selected ally or physically dash to ally to intercept next eligible hostile attack, with Resolve expenditure, new action/cooldown rules and clear per-packet targeting.
- **Guardian Ward**: apply an elemental or general shielding effect to a selected ally or small group, with existing shield-HP and elemental shield matching/nonnmatching rules; must not conflate equipment blocking and magical shield HP.
- **Reworked Challenge or Hold the Line**: retain real threat / ally buff, perhaps expand ally-protection aura distinct from existing TierII 5m elemental resistance aura.
- **Iron Guard's defensive stance may be strong personally**, but Guardian must also have abilities to prevent allies dying; more armor alone won't fulfill the user's primary identity. Whether stored-damage release is single-target, area, or chosen enemy not yet approved.
- Balance Guardian's ability to solo standard world content despite its party utility; ensure new shields/interception cannot create invulnerable infinite loops and scale sensibly alongside a standard tank in multiplayer.
- One TierIII advancement from DefenderLv60+ -> Guardian new classLv1 manifestation. Do not design quest/mentor or new art until the core kit is clarified. User's current one-TierIII-per-parent-first roadmap means Priest and Acolyte first evolutions follow Guardian before revisiting any second Thief/Defender branch.

### Round2 recommended questions, NOT approved

A. Which old skills are essential? E.g. KEEP Shield Bash and Challenge as core active threat control, KEEP Iron Guard upgraded, choose Bulwark vs Shield Sweep vs Hold the Line as other retained skills, then add new direct ally skills.
B. Direct ally-protection tool: **Intercede (redirect damage or leap to ally)** vs **Ward (shield/absorb)** vs BOTH. User can choose both, and no arbitrary six-slot limitation.
C. Iron Guard stored damage release: **single-target selected enemy vs short-range AoE**, particularly for tank gameplay. No release coefficients chosen.
D. Preserve 100 Resolve global cap and basic gain/decay or modify? If user asks fewer questions, defer resource numeric tuning.

**Round1 DESIGN DOCS ONLY, NO GAMEPLAY IMPLEMENTATION, UNITY CHANGES, TESTS, BUILDS OR MERGES.**

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

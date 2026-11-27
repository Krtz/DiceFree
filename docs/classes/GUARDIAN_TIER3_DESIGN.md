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


## GUARDIAN-003 — 2026-11-27 Round 2 USER APPROVES CHALLENGE / IRON GUARD / BULWARK / HOLD THE LINE / INTERCEDE, PHYSICAL CLASSIC SWORD-AND-BOARD IDENTITY, AOE STORED-DAMAGE RELEASE

### Authoritative user wording and decisions

User explicitly submits:
1. "**Retained Defender abilities: Challenge, Iron Guard, Bulwark, Hold the Line**".
2. "**New protective ability concepts: Intercede**".
3. "**Iron Guard damage release: AoE explosion centered on Guardian**".
4. "**i think defender should be a classic 'boring' swoard and board class. the main knight you think of when you hear the word tank.. then the paladin or whatever can have more of a tank with some magic shield or buffs or whatever.**"

Interpretation: The user uses "defender" to describe the baseline **nonmagical classic knight tank fantasy** as this Guardian lineage evolves: straightforward armored sword+shield tanking, aggro, guarding, interception and physical counterforce. **Do not give Guardian an invented holy/magical shield/buff spell identity**. Set aside magic-flavored protective Ward, magical barriers, and magical Sacrifice fantasy for a possible later **Paladin/Templar-style** Defender evolution, not approved as active classes yet. Respect inherited TierII Hold the Line elemental resist aura as an existing parent mechanic, NOT secretly delete or replace that confirmed parent rule; whether Guardian's own Hold the Line retains the precise +1pp-all-resistance per rank or shifts toward a purely physical protective expression is still an OPEN user choice.

### Exact first-pass Guardian SKILL CONCEPT ROSTER, FIVE USER SELECTED

1. **CHALLENGE**, retained/evolved TierII Defender area taunt and actual threat generation. Guardian should hold enemies and protect others from aggro. Exact new rank scaling, radius and effects for Guardian are OPEN.
2. **IRON GUARD**, retained/evolved signature defensive stance storing incoming PRE-MITIGATION hostile damage during the approx2second action lock, then releasing damage in an **AOE EXPLOSION CENTERED ON GUARDIAN** when the stance ends. **AoE is explicitly USER-APPROVED, not single-target and not damage splitting among enemies**. This is a martial/impact-based retaliation fantasy, NOT a magical explosion requirement. The exact AoE radius, damage formula/caps, damage type, per-enemy falloff, stored-damage edge behavior, Resolve cost, cooldown, defensive buff scaling, rank eligibility and item interactions all OPEN. Ensure Guardian still primarily protects allies rather than being designed as DPS/retaliation-centric.
3. **BULWARK**, retained/evolved Resolve-spending defensive mitigation. Existing parent uses Resolve, Guardian keeps same resource; exact Guardian additional ally functionality and coefficient progression OPEN.
4. **HOLD THE LINE**, retained/evolved passive defensive resilience and nearby ally support. TierII parent has a 5m all-elemental resist aura, strongest same-type copy; Guardian should remain classic grounded knight identity. Whether the evolution retains the exact elemental aura or morphs into armor/mitigation/Resolve/team protection to avoid magical flavor is OPEN. No magical particle spells by default just because parent has an elemental-resistance numeric effect.
5. **INTERCEDE**, NEW user-selected ally-protection ability. Identity: Guardian moves to/protects a threatened allied unit or redirects an incoming hit in a physically grounded, knightly fashion. **Exact implementation is NOT yet approved**: target vs area guard, dash to ally vs stationary interception, percent redirection vs complete hit interception, duration, cooldown, Resolve cost, eligibility of damage types, shield absorption, and whether self-target is supported are open. Intercede must be meaningful in a party and no-op handling in solo content should not make Guardian unplayable.

**NOT SELECTED** from Round2's explicit checklist: inherited **Shield Bash**, **Shield Sweep**; new **Guardian Ward** (magical/general ally shielding), **Protective Barrier**, **Sacrifice**. Do not quietly add these as Guardian class abilities or treat them as current slots. They remain existing TierII skills (where applicable) or future design ideas. Five selected concepts do not automatically fix final max count, do not invent a sixth skill merely to match other classes. Guardian must remain solo viable, so deliberate review of a basic Physical attack or later melee ability is sensible, but not yet signed off.

### Scope and progression untouched

- TierII Defender classLv60+ unlocks separate Guardian TierIII classLv1 under Echo progression, retains parent. Sword+shield, Resolve0..100 baseline (incoming premitigation, tagged skill gains, 1 per3s out-of-combat decay), STR/VIT frontline. No unique Mana/holy-magic resource.
- Prior approved TierII Defender Shield Bash, Shield Sweep, Challenge, Bulwark, Hold the Line, Iron Guard continue to exist in TierII; Guardian's omission of some TierII skills doesn't erase original parent gameplay. TierII Iron Guard does not store/release damage.
- Guardian designer should follow first-pass150 rank points precedent only after user agrees to rank totals and caps. No numerics approved in this round.

### Round3 questions appropriate to these decisions (NOT YET APPROVED)

1. **Intercede behavior:** (A) targeted physical rush to ally, intercept next eligible attack, (B) stationary ally guard/redirect percentage for short duration, (C) immediate targeted ally physical damage reduction / positioning bonus. A nonmagical tackle/physical block should match user preference. Determine whether single ally target and movement distance.
2. **Hold the Line tier evolution:** preserve tierII weak 5m elemental-resist aura mechanically without flashy magical effect vs replace Guardian version with purely physical party PDEF/MDEF bonus / blocking / Resolve aid; do not rewrite TierII underlying version.
3. **Iron Guard AoE release:** all stored premitigation damage distributed? User approved AoE CENTERED ON GUARDIAN, not whether every target receives full shared stored value, % scaling, per-target caps or radius. For a tank, a modest 3–5m physical shockwave on stance completion can be suggested only, not declared decided.
4. **Solo damage:** five selected Guardian concepts are mostly defensive; decide whether class BASIC attacks + Iron Guard AoE suffice, or add a sixth simple physical attack skill. Avoid inventing a magic skill or reintroducing omitted Shield Bash/Shield Sweep without user approval.

**DESIGN-ONLY, no Unity C# or art/scene changes, no code compilation/tests/builds/merges.**


## GUARDIAN-004 — 2026-11-27 Round 3 approved stationary ally redirection + Guardian DEF buff/self cast, Guardian PDEF/MDEF aura, Physical Iron Guard shockwave, offense undecided

### Exact user selections and clarifications

User responds:
1. "**Intercede: Stationary nearby ally damage redirection**"; note "**and give a defensive buff to the defender. and be able to use it on themselves so they can use it while solo play.**"
2. "**Hold the Line evolution: Guardian version replaces elemental aura with Physical and Magical Defense bonuses**".
3. "**Iron Guard AoE retaliation: Physical shockwave around Guardian**".
4. "**Offensive skill: No extra offensive skill for now**"; note "**maybe? what skills does it have currently?**"

**USER-APPROVED INTERCEDE:** NEW Guardian ability protects a nearby ally while GUARDIAN STAYS IN PLACE, **redirecting a portion of eligible incoming damage from selected ally to Guardian**, AND **granting a defensive buff to Guardian**. User explicitly requires **SELF-TARGET casting for SOLO PLAY**: casting Intercede on Guardian self grants the Guardian DEFENSIVE BUFF, with **NO self-to-self damage redirection loop**, no duplicated damage or resource generation. This is grounded MARTIAL tank behavior, NOT a magical barrier and not a rush/dash to ally. Whether protective buff increases Physical DEF, Magical DEF, flat block/damage reduction, or both and its exact numbers/rank scaling, redirect percent, duration, cooldown, range/line-of-sight, Resolve cost, incoming damage type eligibility, effect on shield absorption, interactions with Bulwark/Iron Guard, and whether redirection creates extra Resolve are OPEN. Avoid any chance that redirect duplicates incoming hit or Resolve on both ally and Guardian. The user typed "defender" referring to recipient Guardian/Defender-line character; preserve class protection concept without treating TierII parent Defender as changed. The redirect should maintain clear target/beneficiary bookkeeping: allied incoming packet eligible portion gets reattributed to Guardian and Guardian receives its own defense buff while active, with a self-cast buff-only branch.

**USER-APPROVED HOLD THE LINE GUARDIAN EVOLUTION:** Guardian's version **REPLACES its predecessor's all-element resistance AURA** with nearby ally **Physical Defense AND Magical Defense bonuses**. It is a physical/standard knightly protective aura rather than magical shields/holy effects. Parent **TierII Defender Hold the Line continues its existing +1 percentage point per rank to all elemental resistances in 5m range**, unchanged; Guardian is a new TierIII class with its own evolved skill. Exact Guardian aura radius (could reuse5m but not explicitly reconfirmed), flat DEF amount/rank scaling, personal self-bonus, overlap/stacking among multiple Guardians, class buff interactions and VFX are still OPEN. Maintain strongest-identical-aura global policy if applicable; no invented additive Guardian stacking.

**USER-APPROVED IRON GUARD PHYSICAL SHOCKWAVE:** After its historically reserved two-second defensive action lock which accumulates incoming pre-mitigation hostile damage, Guardian Iron Guard's previously chosen centered AoE release is specifically a **PHYSICAL shockwave around Guardian**. It is a martial retaliation effect, NOT magical/elemental shield explosion, not per-enemy separate reflect packet by source nor an obligatory forward shield-slam single facing arc. Exact radius, snapshot source, stored total multiplier and cap, each-target distribution, attack/crit/defense interactions, Resolve cost and cooldown remain OPEN. Store and release damaging amount via normal shared Physical DamageResolver, no magical school assumed.

**OFFENSIVE SKILL STATUS:** User selected "**No extra offensive skill for now**" but immediately adds "**maybe? what skills does it have currently?**" They have NOT made an irrevocable ruling that Guardian may never have a sixth skill. Be candid: approved five selected concepts are **Challenge (AoE taunt/threat), Iron Guard (defensive stance + reactive physical AoE stored-damage shockwave), Bulwark (Resolve mitigation), Hold the Line (PDEF/MDEF ally aura), Intercede (ally redirection + Guardian DEF buff or self-cast buff)**. **Normal Physical sword basic attacks** exist by shared class combat; **currently NO active attack-on-demand damage skill** because Defender's parent Shield Bash and Shield Sweep were NOT selected in Round2. Solo output presently ordinary sword basics plus Iron Guard reactive AoE; WITHOUT an attacker Iron Guard may accumulate nothing. Designer recommends discussing optional one simple physical sword or shield offensive attack later (e.g. Shield Strike, name/effect PROPOSAL ONLY), potentially generating Resolve if it connects, but **DO NOT silently add a new skill or count six as user-approved**. The user's explicit question about current skills should be answered with these five and basic attacks, highlighting lack of active damage skill.

### Remaining design decisions, useful for Round4

1. Does Guardian WANT a sixth always-available Physical strike skill after seeing the roster? Options: **yes new basic sword/shield attack**, **reuse evolved Shield Bash**, **retain current 5**, or a different idea. Guard against assuming number six.
2. Intercede redirect percentage and DEF buff type/duration: recommend designer offer modest first-test numerical options after user settles whether active cost/range should differ, with simple self-cast no-transfer behavior.
3. Guardian Hold the Line PDEF/MDEF bonus should scale with purchased ranks using global flat defense unit semantics; seek exact initial % vs flat first-test preference. Should Guardian itself benefit? expected but not expressly selected as precise rule yet.
4. Iron Guard stored premitigation hit damage -> physical AoE shockwave should have sensible target defense and cap model; ask in later design, no hidden mandatory extra subclass or resource.

### Continuity

TierIII Guardian is standard new Lv1 manifestation after DefenderLv60+, sword+shield, Resolve inherited baseline, dedicated party protector (not magical Paladin), not a damage-first Vanguard. Existing Defender TierII remains UNCHANGED and its six parent skills do not automatically become Guardian skill slots. No class stat numbers, rank caps/150 rank gates, new quests, skill mana, Resolve prices or implementation have been user-approved this round.

**DOCUMENTATION ONLY. No Unity gameplay, C#, files beyond three design docs, build/test, branch merge or deployment.**


## GUARDIAN-005 — 2026-11-27 Round 4 USER APPROVES NEW DIRECT SWORD DAMAGE + SINGLE-TARGET TAUNT

### Exact user decision

User selected **"Custom offensive concept"** rather than previous designer options Shield Strike / evolved Shield Bash / no skill, specifying: "**a straightforward attack with the sword that does damage and taunts the target.**"

**NEW APPROVED SIXTH GUARDIAN SKILL IDENTITY:** an **ON-DEMAND SINGLE-TARGET SWORD STRIKE** dealing DIRECT PHYSICAL damage and **TAUNTING THE SAME TARGET**. The Guardian remains a grounded nonmagical sword-and-shield knight. This is NOT a gap-closing Shield Bash (not selected), not an AoE cleave/Shield Sweep (not selected), and not a standalone taunt with no damage. It should be usable in solo combat and in parties to establish/control threat. The parent Defender TierII skills remain unchanged.

**Two taunt roles deliberately coexist:** Guardian's retained/evolved **Challenge** supplies area-wide taunt/real threat for several enemies; the NEW sword skill combines damage with **single-target threat/taunt** for focused enemy control. Honor underlying taunt susceptibility/immune boss behavior through shared threat rules: an immune boss may ignore forced focus but ordinary threat mechanics can still work under existing encounter system. Whether taunt follows a temporary forced-target effect in addition to a threat increase, how often it can be refreshed and how it scales by skill rank are NOT yet user-approved numeric rules.

**Name not user-selected:** e.g. *Provoking Strike*, *Taunting Slash*, *Knight's Challenge* are possible designer labels only. For now reference it neutrally as **Sword Strike (working label; user approved the mechanics, not its name)**. No weapon other than a **ONE-HANDED SWORD** is needed for the strike; the Guardian retains shield equipment as locked in Round1. Do not imply a spell element, extra resource, magical effect, guaranteed-hit, multiple targets, dash, stun or damage-over-time.

**Not yet locked:** raw Physical flat and STR/VIT coefficients, rank scaling, resource cost or Resolve gain on successful hit, cooldown, attack range, animation/timing, miss/taunt validity (recommend taunt applies on successful hit rather than misses but not signed off), threat amount, whether a shield-absorbed hit counts, target boss eligibility and specific class-rank unlock timetable. All future offensive coefficients should scale with purchased skill ranks consistent with existing project-wide skill scaling policy. Do not invent a starting numeric formula or impose unapproved one.

### Guardian current agreed concepts: SIX

1. **Challenge** — evolved parent area taunt and substantive threat.
2. **Iron Guard** — two-second fortified stance storing premitigation hostile incoming damage; releases Physical AoE shockwave centered Guardian on completion.
3. **Bulwark** — evolved Resolve-spending mitigation.
4. **Hold the Line** — Guardian version grants nearby ally PDEF and MDEF buffs instead of TierII Defender elemental-resistance aura; parent TierII unchanged.
5. **Intercede** — STATIONARY redirect some nearby ally damage to Guardian; Guardian receives defensive buff, self-cast gives buff only with no redirect.
6. **Sword Strike (temporary descriptor)** — **NEW user-approved targeted Physical one-handed sword damage attack which taunts the struck target**.

All six ability identities are now approved, but names/strengths/numeric scaling/rank caps and gates remain open. The sixth is NOT Shield Bash, Shield Sweep, magical protective Ward or some revived previously rejected alternative. Guardian retains normal Physical sword autoattacks in addition to this active. Guardian is a classic martial PARTY PROTECTOR tank with legitimate solo offense, using sword+shield and Resolve. Its TierIII promotion occurs Defender Lv60+ -> new Guardian Lv1 under standard class progression; TierII Defender unchanged.

### Appropriate concise next Round5 decisions (all open)

A. Final name for new single-target sword+taunt skill (e.g. Provoking Strike vs Taunting Slash vs Guardian's Mark; the user may propose one).
B. Does taunt require landed sword damage, and should shield-absorbed valid impacts count? Recommend only a **successful impact**, respecting boss taunt immunity; no taunt on miss.
C. On-hit Resolve: a small flat amount on a successful skill strike vs cost-only/none; ensure class can build Resolve during solo when attacks are weak.
D. Numerical defensive Intercede redirect and personal DEF buff baseline and duration, for a first-playtest proposal.
E. Six-skill TierIII total ~150 ranks, one per class level 1..150, standard precedent but explicit user signoff and exact distribution pending.

**Documentation/design only; no Unity gameplay code or class implementation, no asset editing, builds, tests or branch merge.**


## GUARDIAN-006 — 2026-11-27 Round 5 USER APPROVES PROVOKING STRIKE NAME, SUCCESSFUL-IMPACT TAUNT INCLUDING SHIELD ABSORPTION, RESOLVE GENERATION ON HIT

### User-approved choices

1. **Official skill name: PROVOKING STRIKE.** This REPLACES the Round4 temporary descriptive name "Sword Strike". The two other offered name choices (Taunting Slash and Sword Strike) are NOT selected. Its Round4 core is unchanged: a straightforward, single-target ONE-HANDED SWORD melee attack with a DIRECT PHYSICAL damage packet and single-target TAUNT. It is NOT a dash, shield charge, frontal cleave, magic spell, AoE attack or new class resource.
2. **Taunt application: ONLY ON A SUCCESSFUL HIT, INCLUDING WHEN AN ENEMY SHIELD ABSORBS THE DAMAGE.** Accuracy/target validity is checked by the normal shared attack/hit system; an actual successful impact counts for taunt even if subsequent shield absorption makes HP damage zero. A MISS, dodge, invalid target, interrupted/cancelled strike or no connecting impact grants NO TAUNT. The target is the struck enemy only; the separate skill **Challenge** remains the Guardian's AoE taunt. Reuse existing real threat + boss/forced-target immunity semantics from Defender/Challenge rather than inventing per-class overrides. User has not yet selected exact threat amount or taunt duration for Provoking Strike.
3. **Resolve interaction: GENERATE RESOLVE ON A SUCCESSFUL IMPACT.** The same successful sword hit that earns the taunt qualifies for a Resolve gain, EVEN IF the hit was fully absorbed by an enemy shield; it is still a connecting hit. Miss, invalid target and canceled hit produce zero Resolve. Generate one intended skill-tagged Resolve event PER activation/connected target, never one extra gain for each shield/HP processing stage, and do not create gain via mere activation without impact. The exact Resolve gain number and how it changes with purchased skill ranks remain OPEN, not assumed. Guardian retains its source Resolve0..100 pre-mitigation incoming-hit gain/standard out-of-combat decay unless separately approved; no automatic free resource on all ordinary sword basics.
4. **Damage eligibility:** direct single-target normal Physical sword skill packet; rank-scaled raw flat and attribute coefficients should be designed under global "all coefficients scale with purchased rank" principle when numbers are approved. Per-rank raw damage, skill cooldown, weapon reach, animation windup, gear contributions, natural critical eligibility, first rank unlock and all rank gates still pending. No provisional numbers are promoted to user-approved values.

### Updated locked six-concept Guardian ability roster

| Skill | Current approved identity | Still open |
| --- | --- | --- |
| **Provoking Strike** | New one-handed sword Physical SINGLE-TARGET attack, successful hit -> taunt AND Resolve, including shield-absorbed successful impacts | Damage/rank, Resolve amount/rank, cooldown, threat and taunt duration |
| **Challenge** | Evolved AoE taunt and substantial real enemy threat | New TierIII rank coefficients, radius, cooldown, Resolve interactions |
| **Iron Guard** | Two-second defensive stance with pre-mitigation incoming damage storage, releases PHYSICAL centered AoE shockwave | Damage distribution/rank, AoE radius, defense buff, Resolve cost/CD |
| **Bulwark** | Inherited Resolve-spending mitigation | Rank coefficients, protection scope, cost/CD |
| **Hold the Line** | Guardian-specific PDEF and MDEF ally defense aura (NOT parent TierII elemental-resistance aura) | Base bonus/rank, range/stacking and self coverage |
| **Intercede** | Stationary damage redirection from nearby ally to Guardian PLUS a Guardian DEF buff; self-cast grants self buff only without self-to-self redirection | Redirect fraction/rank, DEF buff amount/rank, duration, range, Resolve cost/CD |

Normal Physical sword basic attacks remain available and TierII Defender's original six abilities remain unchanged. Guardian continues to use sword+shield and Resolve, not magic shielding, retaining the dedicated party-protection classic knight role. Advancement DefenderLv60+ -> separate GuardianTierIII Lv1.

### Next Round6 recommended decisions (all proposals, NOT approved)

1. **Provoking Strike initial Resolve gain**: e.g. +5 per successful impact (recommended starting playtest), +8, or +10; alternatively rank-scaling gain. Limit once per successful activation, including absorbed hits. Decide its cooldown separately or combine with first-test options (e.g. 6sec, 8sec, 10sec). Avoid implying any number already approved.
2. **Single-target Provoking Strike taunt duration**: e.g. 2s normal forced-focus duration, or match TierII Challenge handling, with ordinary threat preserved against taunt-immune bosses. Do not conflate user-approved taunt ON HIT with an approved exact duration.
3. **Intercede mechanics**: pick redirected damage fraction/DEF buff type and whether buff applies to Guardian self while using ally-target; self-cast continues buff-only. Resolve gain from redirected damage must not double-count same source attack.
4. **150-rank budget**: confirm whether Guardian uses one point/class level1..150 and select skill cap distribution later; do not auto-copy 80-rank TierII Defender.

**DESIGN DOCUMENTATION ONLY; no Unity gameplay changes, no code tests/builds/merges.**

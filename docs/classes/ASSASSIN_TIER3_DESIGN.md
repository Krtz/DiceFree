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


## Round 3 — 2026-11-26 USER APPROVES BACKSTAB FROM ANY ANGLE, STEALTH MOVE-SPEED, TWO-HIT BASIC SEQUENCE, RETAIN CRIT PAIR, REPLACE FAN + SMOKE

**Latest USER decisions, authoritative and explicitly superseding earlier tentative Assassin Round2 questions and the parent TierII one-hit basic event for THIS TierIII Assassin only:**

1. **Backstab is USABLE FROM ANY ANGLE**, with a **MUCH HIGHER DAMAGE BONUS when the Assassin stands anywhere in the previously approved target-relative 270-degree SIDE+REAR positional arc**. The only nonbonus sector is the enemy's forward90-degree cone. Do not disable/grey out Backstab from the front. The front cast still works and deals its as-yet-undecided normal damage, while a valid side/rear cast gains a large positional bonus. Exact base Backstab raw damage, positional multiplier, cast/windup, cooldown, Mana cost, crit rules, stealth opener stacking and timing of position evaluation remain OPEN.
2. **EVOLVED ASSASSIN STEALTH INCREASES MOVEMENT SPEED WHILE HIDDEN.** This is a newly user-approved difference from TierII Thief and helps an Assassin reach the qualifying side/rear arc. The exact movement-speed percentage or coefficient, rank curve, concealment duration, threat-drop magnitude, cooldown/Mana, whether old Thief opener becomes stronger, and interaction with other spells remain TBD. This decision replaces earlier Round3 alternative of re-entering Stealth on attacks; do not invent on-attack Stealth resets. Normal movement remains possible while concealed.
3. **DUAL-DAGGER BASIC ATTACKS ARE ONE COMMITTED TWO-HIT SEQUENCE PER BASIC ATTACK CYCLE, WITH A LONGER INTERVAL BETWEEN CYCLES**, superseding the TIERII Thief **one single basic hit with alternating left/right dagger swing animation** for TierIII Assassin. The Assassin now intentionally lands two distinct dagger blows in a basic attack *sequence*. Keep **one timed basic attack cycle**, not two autonomous alternating offhand attack timers. However, the exact number of separate damage packets/events, spacing between mainhand/offhand impacts, whether each hit individually checks miss/crit/defense and procs, and whether a stealth opener/Guaranteed Crit applies to one or both hits are **NOT YET APPROVED**. Do not silently assume either independent duplicate item effects or a fused single damage packet. Both equipped one-handed daggers remain ordinary stat-bearing items without automatic unique intrinsic procs.
4. **KEEP GUARANTEED-BASIC-CRITICAL SELF BUFF AS ITS OWN ACTIVE ABILITY AND RETAIN CRIT/AGI PASSIVE AS ITS OWN DISTINCT PASSIVE**, rather than merging into one passive or deleting/replacing both. Parent TierII guaranteed crit remains basic-only with cooldown/duration mechanics in source spec as reference, but exact TierIII evolution is TBD. Need explicitly decide whether two-strike sequence gets crit on both hits, how Crit passive ranks/AGI, and whether skill Backstab can crit; do NOT automatically treat parent numeric skill maxima, durations and exact basic-only applicability as immutable Assassin TierIII figures.
5. **REPLACE BOTH FAN OF KNIVES AND SMOKE BOMB WITH NEW ASSASSIN PRECISION SKILLS.** Neither remains as an Assassin ability for this first TierIII path. Design identities/names/effects of BOTH new skills are open; no user approval of specific replacements yet. Parent TierII Thief retains Fan/Smoke unchanged. Unlike prospective Burglar's poison/cloud/dirty-trick identity, replacements should reinforce Assassin's slow, lethal, ambush + precise damage style rather than copying Rogue poison AoE clouds.
6. **Preserve Round2 and Round1:** PURE MELEE DPS dual one-handed daggers, MANA ONLY (shared global MaxMana=ClassBaseMana+5INT, ManaRegen/s=4+0.1SPR). Extremely powerful STEALTH BURST opener and weaker sustained damage; slower heavy basic attack *cycles* and deliberate heavy attacks compared to TierII Thief. Keep ENVEnom as separate ability plus BACKSTAB as new ability; position 270-degree side/rear qualifies with equal side/rear bonus, excluding only 90-degree target-front cone. Thief TierII Lv60+ becomes separate Assassin TierIII Lv1 on future advancement; requirements/mentor/trial TBD.
7. **No skill-rank budget or final fixed skill count approved yet.** Currently confirmed concept slots: evolved **Stealth**, retained **Envenom**, new **Backstab**, retained distinct **Guaranteed Basic Crit active**, retained **Crit/AGI passive**, **TWO newly required replacement precision skills** for Fan/Smoke = **7 total concepts**, subject to later user decisions about consolidation, renaming, additions and exact ranks. Do NOT impose Fire Mage's six-skill cap or TierII Thief's80-rank cap; future TierIII 150 rank points is a reasonable DESIGNER CANDIDATE but not user-approved for Assassin.
8. **Stealth basic two-strike special caution:** Parent Thief's opening bonus was spent by first breaking basic/Envenom attempt even on miss. New Assassin double hit makes the exact consumption window OPEN. Likewise whether guaranteed Crit active checks first hit/both hits and whether a two-hit basic shares hit chances must be discussed before any implementation. Do not silently double stealth burst or crit throughput by treating both sequence hits as full inherited opener events.

### Proposed Round4 focused questions (NOT approved)

1. **Two-strike opener amplification:** only first dagger hit gets stealth opener versus both hits receive its multiplier; could select a weighted split instead.
2. **Guaranteed Crit active with two-strike basic:** both strike packets within buff duration guaranteed critical vs only first strike of each basic cycle, and what if buff expires between them.
3. **Stealth movement speed:** flat percentage bonus vs rank-scaled effect, intended first/max bonus, and whether Speed effect stacks with other global movement effects.
4. **New precision-skill identities:** suggest a deliberate direct single-target **Execution/Assassinate** and a positional reposition/debuff (e.g. Shadowstep, Expose Weakness), but DO NOT claim any new skill name/mechanics user-approved; request directions.
5. **Backstab positional bonus/Stealth interaction**: first Backstab from concealment receives evolving Stealth opener (or has its OWN bonus), from front still hits, and determine attack orientation snapshot timing/crit eligibility.
6. **Envenom TierIII**: keeps physical poison and distinct dagger stab; no user-approved numeric changes yet.

**Documentation-only design. No Unity code/assets/scenes/builds/tests or merges authorized.**


## Assassin Tier III Round 4 — 2026-11-27 USER APPROVES BOTH-HIT STEALTH OPENER, PER-IMPACT CRIT, EXECUTE, ARMOR-PEN PASSIVE, AND NO SECOND REPLACEMENT ACTIVE

**LATEST USER ANSWERS, with free-text corrections overriding multiple-choice defaults and older Round3 "two precision skills" placeholder:**

1. **Both hits of the FIRST TWO-STRIKE BASIC SEQUENCE that opens from Stealth get the STEALTH OPENER DAMAGE MULTIPLIER.** The opener is consumed by that one basic SEQUENCE, not renewed separately for each strike and not applied to multiple later cycles. Each of its TWO hits has the boosted opening multiplier; exact multiplier/rank curve for TierIII remains open. Preserve TierII rule that an attempted opener is spent even if missed or absorbed unless user later overrides. Precise hit/miss, hit processing, second-hit continuation after first hit miss or target death, or whether skill Backstab/Envenom receive the same sequence-wide bonus remain for follow-up; do not silently multiply unrelated follow-up attacks.
2. **Guaranteed Crit active is evaluated SEPARATELY at EACH DAGGER HIT'S IMPACT TIME.** If buff remains active at first impact but expires before second, only first hit is guaranteed critical by the buff (second can still normally crit through passive/global chance). If present for both, both hits can independently be guaranteed critical. Do NOT snapshot guaranteed-crit state at two-hit sequence start; no blanket "only first hit" rule. This remains distinct from the still-undefined question of critical eligibility on Backstab/Execute/Envenom skill packets; parent Thief buff applied to basics only unless changed.
3. **ASSASSIN STEALTH MOVEMENT-SPEED RANK PROGRESSION USER CORRECTION:** User explicitly wrote "**3. stealth gives -10 at rank 1 and gains +1 movespeed per rank**", overriding the selected form's proposed **+10% rank1 to +40% max**. Store the EXACT signed progression **StealthMoveSpeedModifier(r) = -10 + (r - 1) = r - 11** for integer purchased stealth rank r>=1, expressed in **movement-speed units unspecified by the user**. Rank1=-10, rank2=-9, rank10=-1, rank11=0, rank12=+1; final number depends on the yet-UNDECIDED Stealth max rank. **DO NOT ASSUME these are percentage points or m/s, DO NOT flip -10 to +10, DO NOT reapply original +40% endpoint**. This creates a rank1 slowdown even though Round3 said stealth "increases speed"; later rank growth can turn positive. Flag the unit and whether rank1 slowdown is indeed intended as one precise follow-up rather than silently correcting it. Other Stealth bonuses/duration/threat/cost and eventual cap not chosen.
4. **First new precision active = EXECUTE**, a heavy LOW-TARGET-HP FINISHER, distinct from Backstab and Envenom. User chose "Execute, heavy low-HP finisher"; exact target-health threshold, scaling and damage coefficients, cast timing, Mana/CD and crit rules OPEN.
5. **User overrides the form's second pick "Expose Weakness": NO SECOND REPLACEMENT SKILL [ACTIVE].** Specifically user wrote "**5. no second replacement skill**", and for #4 added "**add a passive that gives % armor pen.**" Thus **DO NOT ADD EXPOSE WEAKNESS**, Shadowstep, Feint or any other second new precision ACTIVE. Instead add **one new PERCENTAGE ARMOR-PENETRATION PASSIVE** as a separate proposed Assassin passive skill concept. Exact name, rank cap, percent-per-rank, application to basic attacks vs physical skills, armor vs global Physical Defense terminology, and multiplicative/additive stacking with other penetration sources OPEN. This is an approved armor-penetration passive concept, NOT an approved "Expose Weakness" active debuff.
6. **TierIII Assassin ability concept roster AFTER ROUND4** (all individual presence APPROVED, rank/cost/balance details not yet):
   - **Evolved Stealth** active, gives signed rank-scaled move-speed modifier above, enables opening burst / positional strategy.
   - **Envenom** retained distinct active (TierII poison flavor preserved unless later changed).
   - **Backstab** new active, usable from ANY angle but MUCH higher damage from enemy side/rear270° (front90° excluded).
   - **Guaranteed Crit** separate retained ACTIVE, checks buff presence on each basic hit at impact.
   - **Execute** new low-health finisher ACTIVE.
   - **Crit/AGI passive** retained separate PASSIVE.
   - **Percentage Armor Penetration** new separate PASSIVE, exact coefficients/scope pending.
   These are **5 actives + 2 passives = 7 confirmed ability concepts**, not a final capped or rank-gated roster yet. **Fan of Knives, Smoke Bomb and Expose Weakness are absent** from Assassin. Round3's "replace Fan and Smoke with TWO precision skills" is therefore SUPERSEDED: user selected just ONE new execution active plus an armor-penetration passive, and explicitly rejects a second replacement skill. Do not claim an eighth active or additional skill.
7. **Core class identity unchanged:** Thief TierII Lv60+→separate Assassin TierIII Lv1; PURE melee DPS, TWO ONE-HANDED DAGGERS; deliberately slower basic sequences and deliberate heavier-hitting skills; strongest damage in stealth opener, weaker sustained afterward. One BASIC cycle contains two sequential dagger hits and a longer period between cycles, not autonomous dual-wield timers. Side and rear270° have equal positional qualification, front90° does not. Mana ONLY, global MaxMana=ClassBaseMana+5INT and Mana Regen/s=4+0.1SPR, Assassin base/growth stats and class BaseMana TBD.
8. **Crit/armor penetration math must remain balanced:** per-impact crit buff eligibility is now approved, while independent miss/crit rolls for every strike beyond buff check, full damage split, defenses on both hits and passive armor penetration percentage apply-to list remain open. Preserving the global Physical Defense/penetration framework is preferable to adding new defensive subsystems without user direction.

### Proposed Round5 decisions (NOT YET APPROVED)

1. **Critical clarity for stealth speed:** user wrote rank1 '-10' and '+1 movespeed per rank'. Confirm **unit** (% movement speed vs absolute movement-speed stat) and whether negative first-rank slow was intentional, or a typo for +10. Record literal signed progression until then.
2. **Stealth opener + Backstab:** does a Backstab attack from concealment receive its own additional stealth opener multiplier and stack with eligible270° positional multiplier, or does Stealth opener apply ONLY to basic sequence/Envenom like TierII? Details for opening Execute too.
3. **Execute low HP curve:** e.g. bonus below30% target HP, continuous missing target HP scaling, or an execute threshold, and whether it remains usable at full enemy HP.
4. **Armor penetration passive:** applies to normal basics AND physical Assassin skills vs basics only; percentage of target armor ignored per rank with cap, interaction with existing resistance/mitigation.
5. **Two-hit packet timing/impact rules:** first/second basic attack damage split, timing between hits, and whether each impact checks miss/ordinary crit; Stealth opening applies to both hits regardless.
6. **Rank budgeting**: decide whether TierIII Assassin will use 150 point ranks across 7 skills (working goal elsewhere, not yet explicitly approved for Assassin) and then allocate caps/gates.

**DESIGN DOCS ONLY. Do not alter Unity code/assets/builds, or merge/overwrite Codex work.**


## Assassin Tier III Round 5 — 2026-11-27 USER APPROVES STEALTH %-SPEED, BACKSTAB OPENER STACK, CONTINUOUS EXECUTE, ARMOR PEN BOTH, 150 RANKS; CRIT SKILL CONSOLIDATION PROPOSED ONLY

### EXACT Round5 USER APPROVALS

1. **ASSASSIN STEALTH MOVE SPEED UNIT CONFIRMED: PERCENT**, and signed rank curve EXACT: `StealthMoveSpeedPct(r) = -10 + (r-1) = (r-11)%` for purchased Stealth rank r>=1. At rank1 **-10%**, at rank11 **0%**, at rank21 **+10%**, at rank31 **+20%** if those ranks are eligible. This intentionally includes early Stealth MOVEMENT SLOWDOWN, despite older descriptive "increased movement speed" phrasing; do NOT silently flip sign. Actual MAX Stealth rank and thus final move-speed effect TBD. This explicitly settles Round4's unspecified stat-unit ambiguity; it is percent movement-speed modifier, not flat speed.
2. **BACKSTAB receives the STEALTH OPENING DAMAGE MULTIPLIER**, **STACKING with** its qualifying **270-degree SIDE/REAR positional bonus**. Backstab remains USEABLE from ANY angle, including the front90-degree no-positional-bonus cone. This design creates strong dual-synergy opening from stealth in eligible position; exact multiplier values and whether stacking means raw multiplicative product vs another global modifier grouping remain TBD. The user has NOT authorized guaranteed crit on Backstab by merely approving opening damage; crit eligibility requires separate consideration.
3. **EXECUTE ALWAYS USABLE AT ANY TARGET HP**, with **DAMAGE INCREASING CONTINUOUSLY AS TARGET MISSING HP INCREASES**. NO hard 30% execute threshold and NO separate activation gate below some HP fraction. Actual functional curve (linear vs nonlinear), base packet, maximum bonus, rank scaling, cast duration, Mana/CD and possible stealth interaction TBD. Target missing health is based on live target current/max HP per ordinary combat rules, to be explicitly formula-designed later.
4. **% ARMOR PENETRATION PASSIVE** applies to **BOTH BASIC ATTACKS AND PHYSICAL ASSASSIN SKILLS**, not only one category. Exact percentage per purchased rank, cap and stacking with other sources TBD. Respect physical attack/defense engine, not a new magic resistance bypass; Envenom physical Nature poison ticks' inclusion is specifically undecided despite general 'physical skills' wording, as DoT periodic penetration handling can differ.
5. **TOTAL ASSASSIN SKILL RANK BUDGET USER APPROVED EXACTLY 150**, with ONE earned skill point PER ASSASSIN CLASS LEVEL from1 through150. TierIII is separate manifestation Lv1 after TierII Thief Lv60+. Skill rank caps, min-level gates, and allocation across confirmed final skills pending; DO NOT simply reuse TierII Thief80 rank gates or Fire Mage six-skill rank caps. Since crit skill consolidation below is not decided, **DO NOT LOCK A FINAL SKILL COUNT OR SPLIT THESE 150 ranks yet**.

### NEW USER IDEA / OPEN QUESTION, NOT YET A FINAL DECISION

User says: "**hmmm, either we remove guaranteed crit or moove it into the crit passive, making it both a passive and an active at the same time, mmeaning it gives passive crit chance but you can activate it for guaranteed crit for either a time or for a certain amount of hits. what you thiink about this?**" This is exploratory, NOT an authorization to delete or merge now. The Round3/Round4 retained **Guaranteed Crit ACTIVE + Crit/AGI PASSIVE as two DISTINCT skills** remains the CURRENT APPROVED roster until the user decides in next round.

**Designer recommendation (UNAPPROVED):** Merge separate Guaranteed Crit active into the existing Crit/AGI passive to create **ONE DUAL-FUNCTION RANKABLE SKILL** (working name *Killer Instinct*, not signed off). Its passive ranks would provide permanent crit chance (+ AGI if retained); its activated ability would grant guaranteed critical basic hit charges. **Prefer hit-charge activation over time duration** because Assassin has deliberately slow two-impact basic cycles, and a time-limited buff might expire between impacts. As an initial working example, an activation could grant TWO guaranteed critical strike charges, enough for ONE full two-hit basic sequence; hit-charge count, per-rank scaling, charge consumption on miss, eligibility on Backstab/Envenom/Execute, activation Mana/CD, rank cap and exact passive percentage growth all remain unapproved. Preserve general Round4 rule that when a guaranteed Crit status is time-varying, EACH individual basic dagger impact checks its OWN active eligibility, not a start-of-sequence snapshot; if ultimately switching to charges, consumption per qualifying strike must be specified separately.
- **Possible post-approval roster if merged:** Active Stealth, Envenom, Backstab, Execute; hybrid Crit/AGI Mastery (passive and activation); Armor Penetration passive = **6 rankable skill concepts** (4 purely-active + 1 hybrid + 1 passive); currently 7 if crit stays separate. User might also choose to REMOVE guaranteed crit activation entirely, leaving other ranks to allocate. Do not force this recommendation.
- This hybrid skill can be implemented with one purchased rank progression + passive stat hooks + active activation and cooldown, without requiring a second resource gauge. All class abilities still consume only Mana.

### Previously approved unchanged

- Pure dual-dagger melee DPS with slower heavy two-hit basic attack cycles and deliberate skills; opener burst stronger than sustained damage. Both dagger impacts of initial stealth-breaking basic sequence get opening multiplier, but opener is one consumed sequence (not two separate stealth charge triggers).
- Guaranteed Crit currently distinct active checks each dagger impact separately, not snapshot once at cycle start; remains current until consolidation confirmed.
- Evolved Stealth includes signed rank-dependent movement modifier `r-11` percent; Backstab usable from any facing but massive positional synergy outside enemy-front90-degree arc; user now approves STACKING opening multiplier for Backstab. Envenom retained independent physical poison stab skill, Execute always-usable continuous missing-HP scaling.
- Armor Penetration passive affects both basics and physical skill attacks; new active Expose Weakness/Shadowstep was explicitly rejected, Fan of Knives/Smoke Bomb removed from Assassin only.
- Shared global Mana max=`ClassBaseMana+5*INT`, natural regen/s=`4+0.1*SPR`; Assassin stat bases/growth and own ClassBaseMana TBD. No secondary resource.
- World2 normal story access details for ASSASSIN mentor/quest are NOT yet approved; Fire Mage's specific volcanic trial does not transfer here.

### Round6 priority questions (PENDING)

1. **Choose Crit skill architecture:** merge passive and active into one dual-mode skill (recommended), remove guaranteed crit active completely, or retain two separate skills (prior-approved).
2. **If merged, activation type:** fixed guaranteed-critical STRIKE CHARGES (e.g., 2 attacks total), fixed time buff, or rank-scaled number/duration; define buff eligibility basic-only vs certain skills and whether misses consume charges.
3. **Do passive rank bonuses retain +AGI as well as +basic critical chance?** Is active activation tied to purchased ranks, and whether skills can crit naturally?
4. **Execute missing-HP bonus curve:** linear fraction of enemy missing HP to raw damage vs another curve.
5. **150-rank allocation across final skills** after crit merge decision, and rank gating (one eligible purchase per level1..150).

**DOCUMENTS ONLY. No Unity gameplay/source/assets/builds/merge.**


## Assassin Tier III Round 6 — 2026-11-27 USER APPROVES HYBRID CRIT PASSIVE/ACTIVE, CHARGE-BASED GUARANTEED BASIC CRITS, RANK-SCALED FROM TWO CHARGES

**LATEST USER ROUND6 DECISIONS, DEFINITIVE, superseding the Round3/Round4 two-separate-crit-skills rule and Round5 merely exploratory hybrid proposal:**

1. **MERGE the old Guaranteed Crit ACTIVE and Crit/AGI PASSIVE into ONE SINGLE RANKABLE HYBRID PASSIVE + ACTIVATABLE skill.** It has ONE purchased rank progression granting passive effects AND an activation using that same purchased rank. It is NOT two skills consuming separate points. Working label **Critical Mastery / Killer Instinct** is DESIGNER-ONLY; final in-game ability name has NOT been approved. Do not allocate a standalone Guaranteed Crit skill slot/ranks.
2. **HYBRID PASSIVE must grant BOTH bonus AGI AND passive CRITICAL HIT CHANCE** with purchased ranks. User approves both attributes, not crit-only or crit damage instead. Exact AGI/crit increments per rank, cap, baseline crit damage multiplier, and whether passive critical chance affects physical skills beyond normal basic attacks remain TBD; parent TierII +1AGI/+1pp basic-crit per rank and1.5x crit raw are REFERENCE, not automatically adopted as TierIII coefficients.
3. **HYBRID ACTIVATION grants a NUMBER OF GUARANTEED CRITICAL HIT CHARGES, NOT a short fixed time window.** Each eligible successful basic dagger hit will consume or use a charge per future exact charge-spending rules, to be approved; never implement a forced short buff timeout without a new decision. Long-term state persistence, refresh on recast, logout/death handling, and whether misses consume charges remain OPEN.
4. **GUARANTEED CRIT CHARGES SCALE WITH SKILL RANK, STARTING AT EXACTLY TWO CHARGES** when the hybrid skill first reaches rank1. At rank1 two guaranteed critical basic strikes can cover the new Assassin TWO-IMPACT basic attack sequence, as long as charge eligibility/consumption is defined accordingly. More purchased ranks increase charges according to a still-UNAPPROVED progression. Do not silently freeze a two-charge maximum, grant +1 charge per every rank, or invent a specific final charge cap without consulting user.
5. **HYBRID ACTIVE guaranteed-critical charges apply ONLY TO NORMAL BASIC ATTACK DAGGER STRIKES**, NOT to Backstab, Envenom direct stab or poison ticks, Execute or other physical skills. This confirms old parent Thief's basic-only scope for the hybrid active, but does NOT settle whether any active Assassin damage SKILLS can critically hit naturally via independent crit rules. Do not have Backstab/Execute consume charges or gain guaranteed crit merely because of this buff.
6. **TWO-STRIKE BASIC INTERACTION:** The earlier approved separate-crit active checked its active state independently at EACH dagger impact. With the newly approved hybrid CHARGE model, keep **per-hit eligibility/charge accounting** rather than snapshotting a two-hit basic sequence and spending only one charge for both. Each actual eligible basic strike is one candidate for a guaranteed crit; precise miss and absorption policies remain undecided. Stealth opener still multiplies BOTH strikes of first stealth-breaking BASIC SEQUENCE, even though the stealth opening is consumed once for the entire sequence, not independently per hit.
7. **AUTHORITATIVE POST-ROUND6 ASSASSIN ROSTER: SIX RANKABLE SKILL CONCEPTS**, four purely active, one hybrid, one purely passive:
   - **Stealth** (ACTIVE, evolves TierII with rank-dependent movement modifier `(r-11)%`, strong opening damage).
   - **Envenom** (ACTIVE, retained separate poison dagger stab).
   - **Backstab** (ACTIVE, usable all angles, heavy bonus in target-side/rear270deg, Stealth opener multiplier stacks with positional bonus).
   - **Execute** (ACTIVE, usable at any target HP, damage increases continuously with target missing HP).
   - **Critical Mastery (working name)** (ONE HYBRID PASSIVE+ACTIVE; passive +AGI and +crit chance; activation rank-scaled basic-only guaranteed crit charges beginning at2).
   - **Armor Penetration** (PASSIVE, percentage of target Physical Defense bypassed on normal basic attacks and physical Assassin skills; percent/rank/cap TBD).
   **NO SEPARATE GUARANTEED CRIT ACTIVE**, NO Expose Weakness, Fan of Knives or Smoke Bomb in Assassin. Parent Thief is unchanged. Total **150 PURCHASABLE SKILL RANKS / 1 POINT ASSASSIN CLASS LEVEL1..150** signed off Round5, to be distributed among these six concepts AFTER caps and gates are designed.
8. **OTHER APPROVED CLASS RULES PRESERVED:** TierII Thief Lv60+ -> separate TierIII Assassin Lv1; PURE MELEE DPS; dual one-handed daggers; one SLOWER basic ATTACK CYCLE containing TWO distinct dagger impacts, with a slower heavy skill cadence. Very strong burst from Stealth followed by weaker sustained DPS. Positional bonus valid from enemy's side/rear270deg, with only enemy front90deg excluded. Mana ONLY, universal MaxMana = AssassinClassBaseMana + 5*INT and regen/sec = 4 + 0.1*SPR; class-specific base and growth TBD. Envenom retained, Execute continuous missing-target-HP scaling, armor pen applies to basics and physical skill attacks. Stealth rank1 -10% movement and +1 percentage point per additional purchased Stealth rank. Rank caps/first eligibility and all ability numbers NOT yet locked.

### Implementation/design caveats, UNDECIDED

- Guaranteed crit **charge consumption on a MISS or full shield absorption**: user has only approved rank-scaled GUARANTEED CRITICAL HIT CHARGES; do not imply whether attempt versus successful impact spends a charge. The original Stealth opener has separate existing consumed-on-miss behavior; these systems should not be conflated.
- Does hybrid active require Mana to activate, its cooldown, charge maximum and recharge model, persistence, overwriting/stacking on recast, and exact rank gain schedule all TBD.
- What does the passive's CRIT CHANCE affect? User affirmed crit chance without explicitly narrowing it to basics; active GUARANTEE is specifically basics only. Current parent TierII crit passive was basic-only, but new TierIII scope should be explicitly confirmed.
- No active guaranteed crit on Envenom, Backstab, Execute; natural critical chance for those still open.
- Stat bases/growth, weapon basic damage coefficient/interval and hit spacing, raw skill damage formulas, Backstab positional multiplier, Stealth opener scaling, Execute missing-health curve, armor pen percentage curve, cooldown and rank eligibility gates still need user design.

### Round7 interview ideas (NOT YET USER APPROVED)

1. **Charge gain by skill rank**: e.g. rank1=2, +1 charge every 5 purchased ranks, max depending on hybrid cap; or a faster/slower progression.
2. **Charge consumption**: consume on successful/eligible basic hit vs basic swing attempt (including misses), and whether damage fully shielded counts.
3. **Crit passive scope**: basics only vs all direct Physical skill attacks in addition to basics; keep separate from basic-only GUARANTEE active.
4. **Hybrid activation cost and CD**: choose global Mana-based cost and cooldown plus whether activation refills or adds to existing unused charges.
5. **Exact 150-rank allocation** across six skills, then audit per-rank min-level gates without forcing fully spent points before eligible purchases.

**DOCS ONLY; no Unity changes, builds/tests, or branch merges.**


## Assassin Tier III Round 7 — 2026-11-27 USER APPROVES CRIT CHARGES, SUCCESSFUL-HIT CONSUMPTION, BASIC-ONLY CRIT PASSIVE, REFILL ON RECAST, EXACT 150 SKILL-RANK CAPS

### Latest USER DECISIONS, with free-text override authoritative

1. **CRITICAL MASTERY HYBRID CHARGE PROGRESSION:** Start at **TWO guaranteed-basic-crit charges at purchased skill rank1**, gain **ONE ADDITIONAL charge every FIVE further purchased ranks**. For r=1..20 approved cap, `GuaranteedBasicCritCharges(r) = 2 + floor((r-1)/5)`; ranks1–5=2 charges,6–10=3,11–15=4,16–20=5. This ranks-to-charges interpretation means no extra charge at rank5: FIVE additional purchased ranks after first are required for first increase. All charges apply ONLY to normal BASIC dagger impacts, not Envenom, Backstab, Execute or any other skills. No short time-based expiration while holding charges.
2. **CRIT CHARGE CONSUMPTION:** A basic dagger **MISS does NOT spend** a guaranteed critical-hit charge. **Only a SUCCESSFUL BASIC DAGGER HIT consumes one** charge. With the two-hit basic attack, each impact independently checks its own eligible crit charge: hit consumes one, miss consumes zero. If first misses and second hits, second hit can use one stored charge. Distinguish "successful hit" from effective HP damage: whether a fully shield-absorbed successful contact consumes a charge is still a global combat integration question, as the user explicitly decided misses vs hits, not shield-only HP outcomes. Do not consume at basic attack sequence start, or burn one charge on a miss.
3. **PASSIVE CRITICAL-HIT CHANCE APPLIES TO BASIC ATTACKS ONLY**, keeping the parent's baseline crit-scope philosophy. Hybrid passive also continues to grant permanent bonus AGI per purchased rank (exact AGI and basic crit chance increments not chosen). Hybrid ACTIVATION remains BASIC-HIT GUARANTEE ONLY. Backstab, Envenom and Execute are not automatically eligible for this passive crit-chance bonus or guaranteed crit charges, notwithstanding their direct Physical damage and Armor Penetration passive eligibility.
4. **REACTIVATION REFILLS/REPLACES CURRENT CHARGES with FULL ALLOTMENT** for purchased Critical Mastery rank. It **does not ADD, STACK, bank, or carry over** leftover charges. Example: rank11 with1 remaining charge reactivated -> exactly4 guaranteed basic crit charges, not5. Whether activation has Mana/CD, or whether charges persist through death/logout, remains open. Recast is subject to future normal ability restrictions/CD; this approval does not imply costless, spammable activation.
5. **EXACT USER-APPROVED 150-RANK DISTRIBUTION WITH FREE-TEXT OVERRIDE:** Form proposed 25/25/35/30/20/15, but user adds "**move 5 points from execute to envenom**." Final authoritative caps are:
   - **Stealth25**
   - **Envenom30** (**+5 from form**)
   - **Backstab35**
   - **Execute25** (**-5 from form**)
   - **Critical Mastery20** (SINGLE hybrid passive+active purchased skill)
   - **Armor Penetration15**
   **SUM = 25+30+35+25+20+15 = 150**. One skill-rank purchase point earned at EACH Assassin class Lv1..150. **Cap totals are APPROVED**, but **EXACT PER-RANK CLASS MINIMUM-LEVEL ELIGIBILITY GATES NOT APPROVED OR CALCULATED IN THIS ROUND**. A legal audited 150-level gate schedule is the next workflow task.
6. **Skill roster unchanged:** four ordinary actives Evolved Stealth, Envenom, Backstab, Execute; one hybrid passive+activatable Critical Mastery; one Armor Penetration passive = SIX RANKABLE SKILLS. No standalone Guaranteed Crit skill, no Fan of Knives, Smoke Bomb or Expose Weakness. Armor Penetration continues applying to basics AND Physical Assassin skills, separate from basic-only crit passive/active.
7. **Class fantasy unchanged:** Pure melee DPS with dual one-handed daggers, intentionally slower TWO-HIT basic attack cycle and slower powerful skill strikes; strong Stealth opener then weaker sustained damage. Both initial two-hit basic impacts get opening multiplier; opener consumed once for sequence, even if the attempt misses, per parent reference. Backstab usable any angle, qualifies for large positional bonus from enemy-side/rear270deg, and stacks Stealth opening multiplier when it is the breaking opening skill. Execute always usable, stronger as target missing HP increases. Stealth gives `(rank-11)%` movement speed while concealed, so at new rank25 cap it reaches **+14%**, rank1 **-10%**, rank11 0%; this is DERIVED directly from two separate approved decisions rather than separately chosen percentage. Universal Mana-only max=`AssassinClassBaseMana+5*INT`, natural regen/s=`4+0.1*SPR`, exact class base/growth TBD.
8. **DO NOT ASSUME** that Assassin's 150 rank points imply every skill available to rank1 at classLv1. First unlock levels, follow-on rank gate intervals, complete rank gating feasibility audit, all damage formulas, Mana costs, cooldowns, class stats, mentor/quest and visual assets remain to be authored/approved.

### Quantitative approved charge and movement examples

| Rank | Critical Mastery available charge count |
| ---: | ---: |
| 1–5 | 2 |
| 6–10 | 3 |
| 11–15 | 4 |
| 16–20 | 5 |

| Stealth rank | Stealth movement modifier while concealed |
| ---: | ---: |
| 1 | -10% |
| 11 | 0% |
| 21 | +10% |
| 25 (approved cap) | +14% |

### Next round questions (NOT approved)

1. Set Critical Mastery hybrid **ACTIVATION Mana cost and cooldown**, and whether cooldown starts immediately on activation, noting charges remain until used.
2. Determine charges persist through **death, logout and swapping manifestations** or are cleared as ordinary temporary combat buffs; no hidden short timer.
3. Specify guaranteed critical successful hit vs **fully shield-absorbed hit** and whether hit charge is consumed; missed hits explicitly do NOT.
4. Decide **critical chance and AGI per Critical Mastery purchased rank**, and armor-penetration percentage/rank with15-rank cap.
5. Choose **stealth availability, rank gate pacing and first unlocks**; then generate a proven legal 150-point rank gate schedule with each level1..150 having at least one eligible spend.
6. Set two-hit basic cadence/damage split and execute curve after rank/level model.

**DOCUMENTATION ONLY. No Unity code, scenes, assets, tests, builds, merges or implementation authorized.**


## Assassin Tier III Round 8 — 2026-11-27 USER APPROVES CRIT+AGI, ARMOR PEN, 25S CRIT COOLDOWN, SHIELD-HIT CHARGE SPEND, BONUS SIXTH RANK20 CHARGE

### Authoritative Round8 USER APPROVALS (free-text custom rank20 bonus OVERRIDES previously assumed charge cap5)

1. **HYBRID CRITICAL MASTERY PASSIVE: +1 PERCENTAGE POINT BASIC ATTACK CRIT CHANCE PER PURCHASED RANK (r1..20)**, totaling **+20 percentage points** at rank20. Distinguish crit chance percentage POINTS (additive crit chance contribution), not 1% relative increase. Bonus remains BASIC ATTACKS ONLY per Round7. Does not imply any new natural crit eligibility for Backstab, Envenom, Execute or periodic poison.
2. **HYBRID CRITICAL MASTERY PASSIVE: +3 AGI PER PURCHASED RANK (r1..20)**, totaling **+60 AGI** at rank20, in addition to basic crit chance. Both passive effects derive from the SAME purchased rank progression shared with its activation, no separate rankable Guaranteed Crit active.
3. **ARMOR PENETRATION PASSIVE: IGNORE AN ADDITIONAL 1% OF TARGET PHYSICAL DEFENSE PER PURCHASED RANK (r1..15)**, totaling **15% TARGET PHYSICAL DEFENSE BYPASS** at rank15. Applies to BASIC ATTACKS and PHYSICAL ASSASSIN SKILLS, per Round5. This is % of the target's PDEF, not a flat -15 PDEF debuff and not a percentage of final HP damage. Combine with existing global physical-defense/penetration rules; any multiple-source stacking order remains a global engine integration detail, not license to invent new immunities.
4. **HYBRID CRITICAL MASTERY ACTIVATION COOLDOWN EXACTLY 25 SECONDS**, independent of skill rank unless another explicit approved change. Charges have NO short time window/timeout per Round6. Mana activation cost, time at which cooldown starts, persistence through character death/logout/manifestation switching, and charge refill on reactivation beyond approved replace rule are still TBD. User has approved 25s cooldown, NOT that activation is free.
5. **A SUCCESSFUL BASIC DAGGER HIT CONSUMES ONE GUARANTEED CRIT CHARGE EVEN IF ITS DAMAGE IS FULLY ABSORBED BY A SHIELD.** A MISS still consumes ZERO charges. A successful hit is defined by attack resolution connecting, before evaluating shield vs HP loss; a basic hit blocked/absorbed by a shield is still a successful hit. With the two-hit basic cycle each dagger impact resolves separately; 1 eligible connected impact consumes 1 charge, regardless of shield blocking final HP damage. Skills Backstab, Envenom, Execute remain INELIGIBLE for guaranteed basic-charge spending.
6. **USER CUSTOM OVERRIDE: EXTRA GUARANTEED CRIT CHARGE AT RANK20**, creating **SIX charges at skill cap20**, not five. Preserve earlier user-approved start at2 rank1 and +1 charge every5 additional purchased ranks, then ADD ONE SPECIAL EXTRA at max rank:
   `ChargeCount(r)=2+floor((r-1)/5)+(r==20 ? 1 : 0)` for purchased Critical Mastery rank r in1..20.
   Explicit charge breakpoints: ranks1–5=2;6–10=3;11–15=4;16–19=5;**rank20=6**. Do not incorrectly use an ordinary +1-per-five formula to claim rank20=5, or grant max6 at rank16. At max, six fully successful dagger hits can cover three whole TWO-HIT basic cycles, assuming charges were not spent otherwise.
7. Recast/refill Rule from Round7: when activated again after cooldown, replace any remaining charges with FULL current rank-dependent amount, not additive or stackable. Charge use is only on eligible SUCCESSFUL basic impacts, not on missed attempts and not when physical skills are cast. The hybrid passive AGI and crit chance bonus are always effective based on purchased rank and do not diminish when charges are used.

### Approved consolidated Assassin numerical ledger after Round8

| Purchased skill and cap | Numeric behavior |
| --- | --- |
| Stealth25 | Concealed movement percent modifier `(r-11)%`, rank1=-10%, rank25=+14%, exact duration/CD/stealth opener damage curve pending |
| Envenom30 | Distinct poison stab; TierIII damage/cost/cooldown/rank formulas pending |
| Backstab35 | Castable from all directions; significantly stronger in target-relative270deg side+rear and gains stacking Stealth opener multiplier; exact values pending |
| Execute25 | Always usable; damage increases continuously with missing target HP, exact curve/cost pending |
| Critical Mastery20 | Hybrid: passive +3AGI/r and +1pp basic crit/r; activated basic-only guaranteed crit CHARGES `2+floor((r-1)/5)+(r==20?1:0)`, NO short timer, **25s cooldown**, successful shield-absorbed hit spends1, missed hit spends0, reactivation resets to full allotment |
| Armor Penetration15 | Passive ignores +1% of target PDEF/rank, max15%, basics AND physical Assassin skills |

Total purchased ranks exactly **150**, one skill point/class level1..150. **Per-skill min-level rank gates not yet locked or audited**, and rank0 ability availability needs explicit handling. Mana ONLY, global `MaxMana=ClassBaseMana+5*INT`, `naturalRegen/s=4+0.1*SPR`, own class base stats/gains TBD. Pure melee two-dagger, deliberate two-hit basic sequence, heavy skills, strong stealth opening then weaker sustain, rear/side270deg positioning. Four active skills Stealth/Envenom/Backstab/Execute, one hybrid Crit Mastery and one passive Armor Pen. No extra Expose Weakness, Fan of Knives, Smoke Bomb, or separate guaranteed-crit skill.

### Proposed Round9 next decisions, NOT YET APPROVED

1. Determine **CRITICAL MASTERY MANA COST**: e.g. fixed cost or rank-scaled, noting global Mana formula remains universal.
2. Determine **charge persistence** across death, logout and class swaps, while NOT adding a hidden short timeout.
3. Determine timing of **25s cooldown** (on activation vs when charges are exhausted), plus whether rank0 hybrid is usable (no purchase means no bonuses/activation).
4. Choose **per-rank class minimum-level unlock schedule** for six approved caps summing150. Validate every class level1..150 can spend one point if desired; don't assume all ranks initially unlocked.
5. Determine Assassin classLv1 stats/growth, Envenom/Backstab/Execute raw damage rank formulas, basic cycle spacing, Stealth damage rank growth and positional bonus, and advancement quest.

**DOCUMENTATION ONLY. No Unity code/assets/scenes, builds, tests, or branch merges.**

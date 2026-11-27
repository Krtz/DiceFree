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


## Assassin Tier III Round 9 — 2026-11-27 USER APPROVES RANK-INCREASING CRIT MANA COST, CLEAR CHARGES ON DEATH/LOGOUT/CLASS CHANGE, 25S CD ON ACTIVATION

### Latest USER answers, authoritative

1. **Critical Mastery HYBRID ACTIVATION MANA COST INCREASES WITH PURCHASED SKILL RANK.** Rank-scaled, NOT fixed; the EXACT Mana-cost formula, base cost and increment PER rank are **NOT YET APPROVED**. Preserve universal Mana system (MaxMana = AssassinClassBaseMana + 5*INT, natural Mana regeneration/s = 4 + 0.1*SPR), and no new class resource. A possible formula such as `20+5r` is only a designer candidate if discussed, not canonical. Activation still requires a purchased rank; no rank0 guaranteed crit activation is authorized.
2. **UNUSED GUARANTEED-CRITICAL BASIC HIT CHARGES ARE CLEARED ON DEATH.** No charge carryover across death and respawn. Death must not magically refill charges or preserve a pre-death charge buff.
3. **UNUSED CHARGES ARE CLEARED ON LOGOUT** (including reconnect as new session; no reappearing old buff). The user approved clearing on logout, not changing purchased passive ranks/stats.
4. **UNUSED CHARGES ARE CLEARED WHEN CHANGING CLASSES / MANIFESTATIONS.** Switching out of Assassin forfeits unused temporary guaranteed-critical charges; later switching back cannot restore them. Persistent purchased Critical Mastery rank remains as class progression and resumes its PASSIVE AGI/crit benefit when appropriate.
5. **CRITICAL MASTERY ACTIVATION 25-SECOND COOLDOWN STARTS IMMEDIATELY ON ACTIVATION**, **NOT** when final charge is used. If charges remain and activation comes off cooldown, user-approved Round7 reactivation replaces remaining charges with FULL new allotment. No short expiration timer for charges while alive/logged in/remaining Assassin. Whether the active cooldown itself persists through logout/class switching follows global engine behavior, not newly specified or waived here.
6. **Current hybrid system unchanged:** One rankable hybrid Critical Mastery cap20. Passive each purchased rank +3 AGI and +1 percentage point BASIC ATTACK crit chance, rank20 +60AGI/+20pp. Active BASIC-HITS ONLY guaranteed crit, no short duration, charges `2+floor((r-1)/5)+(r==20?1:0)`: rank1–5=2, 6–10=3, 11–15=4, 16–19=5, 20=6 (user-selected extra rank20 charge). Successful basic dagger contact consumes1 even when shield absorbs all damage; misses consume0; every impact in Assassin's two-strike basic cycle resolved separately. Charge refill on activation replaces, never adds. No charge guarantees for Backstab, Execute, Envenom.
7. **Assassin class scope retained:** TierII Thief Lv60+ to distinct TierIII Assassin Lv1; pure melee DPS, dual one-handed daggers, slower basic cycles with two hits, slower heavy skills, strong Stealth opening burst, weaker sustain. Stealth25 signed concealed movement modifier `(r-11)%` (-10% rank1, +14% rank25), both opening basic dagger hits empowered; Backstab35 usable any angle, gains side/rear270° positional bonus stacking with stealth opener. Envenom30 retained poison stab; Execute25 always usable and scales continuously with target missing HP. ArmorPen15 +1% target Physical Defense ignored/rank on basics and physical Assassin skills. CriticalMastery20 hybrid. Exactly150 rank points, one each Assassin classLv1..150, per-rank min-level gates still pending.
8. **Not yet approved:** Exact rank-dependent Critical Mastery Mana cost; class base mana/starting stats/growth; each skill unlock/class-level gate table; Backstab/Envenom/Execute damage coefficients and cooldowns; precise basic two-hit timing; Stealth opener numbers and duration; ArmorPen multi-source stacking details; exact boss-facing positioning snapshots; NPC/quest/visuals. Do NOT invent values to claim implementation readiness.

### Next designer questions, not yet approved

1. **Select exact Critical Mastery Mana cost formula**, e.g. affordable fixed base + extra cost per rank, honoring existing global Mana rule.
2. **Choose first-unlock class levels and rank gate intervals** for Stealth25/Envenom30/Backstab35/Execute25/CriticalMastery20/ArmorPen15, then author full150 legal rank-by-level gate audit without unspendable class points.
3. **Define two-hit basic timeline, damage weighting and attack interval** relative to old Thief.
4. **Set Stealth opening scaling/positional Backstab multiplier and Execute missing-HP function**; balance slow burst vs limited sustained pressure.
5. **Establish Assassin Lv1 stats and per-level growth, starting Mana and TierIII mentor/trial** separately.

**DOCUMENTATION ONLY. No Unity code, assets/scenes, builds, tests, class implementation or merge.**


## Assassin Tier III Round 10 — 2026-11-27 USER OVERRIDES CRITICAL MASTERY MANA NUMBER UNTIL AFTER STATS; STAGGERED UNLOCKS AND EQUAL DAGGER IMPACT DAMAGE APPROVED

### Latest Round10 explicit user decisions and free-text priority

1. **CRITICAL MASTERY MANA COST FORMULA IS NOT APPROVED; ALL ASSASSIN MANA COST NUMBERS ARE DEFERRED UNTIL AFTER THE ASSASSIN STAT DISTRIBUTION.** User selected form's option `20+5r` then explicitly added: "**we decide mana costs after stat distrobution**." This later specific free-text instruction OVERRIDES treating `20+5r` as approved. Preserve the earlier Round9 rule that Critical Mastery activation Mana cost **increases with purchased rank**, but **do not lock base20 or +5/rank** or any exact cast-cost formula yet. No skill Mana cost for Stealth, Envenom, Backstab or Execute is to be newly finalized before choosing Assassin Lv1 attributes, per-level growth, class BaseMana and global Mana pool/regen consequences.
2. **STAGGER FIRST SKILL UNLOCKS OVER EARLY ASSASSIN CLASS LEVELS**, with individual skill-rank purchase levels distributed through class levels1–150. The user selected staggered (NOT all six available at classLv1). Which exact first-unlock levels or later purchased-rank gate schedule are approved? **NONE YET**: a fully audited DESIGNER-ONLY candidate is provided below for the next round, not signed-off. Preserve the approved six caps: Stealth25 / Envenom30 / Backstab35 / Execute25 / CriticalMastery20 / ArmorPenetration15 =150 total; one point earned at each Assassin classLv1..150.
3. **BOTH DAGGER IMPACTS IN ONE DELIBERATE TWO-HIT BASIC SEQUENCE DEAL EQUAL RAW DAMAGE, SPLIT 50% / 50% OF THE SEQUENCE'S NORMAL BASIC ATTACK DAMAGE BUDGET.** This overrides the proposed 40/60 or60/40 imbalance. The class remains slower in basic attack cycle frequency than TierII Thief, with exact attack interval and inter-impact delay still OPEN. Separate per-impact hit/crit/shield handling continues: critical charges consumed on each SUCCESSFUL BASIC HIT, including fully shield-absorbed hits, while MISSES consume0. Both basic impacts from the FIRST stealth-opening sequence receive opening multiplier as approved Round4, so apply same multiplier to both corresponding equal pre-mitigation hits; do not accidentally double the intended overall baseline merely by allocating the old single-hit damage to EACH strike.
4. **Other approved class behavior unchanged**: pure melee dual-dagger DPS, slower heavy skills and basics, very strong Stealth opening burst and weaker sustain, Backstab usable all angles but major positional damage at target sides/rear270° stacking with Stealth opener, Execute always usable scaling with target missing HP, Envenom retained distinct. Stealth purchased rank speed modifier `(r-11)%` (r1 -10%, cap25 +14%). Hybrid CriticalMastery20 passive +3AGI/rank and +1 percentage point BASIC CRIT/rank; activation's basic-only guaranteed crit charges `2+floor((r-1)/5)+(r==20?1:0)` max6,25s cooldown starts immediately on use, rank-growing Mana cost TBD, charges persist while eligible but clear on death/logout/manifestation switch; recast replaces charges. ArmorPenetration15 ignores +1% target Physical Defense/rank on basics and physical Assassin skills. Global Mana max=`AssassinClassBaseMana+5INT`, natural regen/s=`4+0.1SPR` stays universal.

### Proposed EXACT 150-rank class-level eligibility table — DESIGNER-ONLY, NOT YET USER APPROVED

The following table gives each skill purchased rank `r=1..cap` a **minimum class level** `L=gate[r]`. The user approved only **staggered unlocks and auditing**, NOT these specific individual gates. Proposed first unlocks: **Stealth Lv1, Envenom Lv2, Backstab Lv3, Critical Mastery Lv5, Execute Lv8, Armor Penetration Lv10**. Can be adjusted if user wants different pacing (e.g., Execute later, earlier defensive passive).

**AUDIT PASS:** exact150 gate records and user-approved caps; each Assassin class level1..150 has EXACTLY ONE candidate rank whose minimum class level equals that level, so a player who spends immediately has a legal skill purchase every level, including classLv1 and Lv150. For any L1..150, number of gate-eligible ranks equals L, assuming no previous purchases; rank-up requirements still require earlier ranks in that specific skill. Since each skill's gate list is strictly increasing, following the per-level gate order automatically satisfies rank prerequisites. Last rank gate reaches Lv150 (Backstab); other skills reach caps earlier than Lv150. This is a mathematical candidate and not a signed-off exact level schedule.

| Skill | Approved cap | Proposed first eligible class Lv | Proposed last rank gate Lv | Complete proposed per-rank class-level gates (ordered by purchased rank) |
| --- | ---: | ---: | ---: | --- |
| Stealth | 25 | 1 | 147 | 1, 7, 15, 21, 27, 33, 39, 45, 51, 57, 63, 69, 75, 81, 87, 93, 99, 105, 111, 117, 123, 129, 135, 141, 147 |
| Envenom | 30 | 2 | 149 | 2, 6, 13, 18, 23, 29, 32, 38, 43, 48, 53, 59, 62, 68, 73, 78, 83, 89, 92, 98, 103, 108, 113, 119, 122, 128, 133, 138, 143, 149 |
| Backstab | 35 | 3 | 150 | 3, 4, 11, 16, 20, 24, 30, 31, 37, 41, 46, 50, 54, 60, 61, 67, 71, 76, 80, 84, 90, 91, 97, 101, 106, 110, 114, 120, 121, 127, 131, 136, 140, 144, 150 |
| Execute | 25 | 8 | 148 | 8, 9, 17, 22, 28, 34, 40, 47, 52, 58, 64, 70, 77, 82, 88, 94, 100, 107, 112, 118, 124, 130, 137, 142, 148 |
| Critical Mastery | 20 | 5 | 146 | 5, 12, 19, 26, 35, 42, 49, 56, 65, 72, 79, 86, 95, 102, 109, 116, 125, 132, 139, 146 |
| Armor Penetration | 15 | 10 | 145 | 10, 14, 25, 36, 44, 55, 66, 74, 85, 96, 104, 115, 126, 134, 145 |

### Pending after Round10

- **Next design round should approve/alter starting stats before ANY numeric Mana-cost choices**, per user note. Ask for Assassin Lv1 VIT, STR, AGI, INT, SPR; per-level gains and own ClassBaseMana, then budget affordable mana skill costs later with global capacity/regen formulas.
- The exact proposed rank level-gate table above remains **NOT APPROVED**, despite successful 150-level audit; ask for acceptance or changes separately, ideally after discussing how the unlock schedule feels.
- Exact basic attack interval between the SLOWER two-hit cycles and spacing between two impacts, independent hit chance/crit rolls and death mid-sequence; raw basic attack damage coefficient and its relative relationship to TierII Thief all remain OPEN. Damage SPLIT 50/50 is approved.
- Stealth opening raw damage rank curve, Backstab raw and270° bonus, Execute continuously scaled missingHP curve, Envenom higher-rank poison damage, crit/armor effects are awaiting their class-level and damage design; none is newly locked here.
- Specific advancement quest/mentor location, assets and implementation all deferred. No assumption that Assassin's trial equals Fire Mage's volcano trial.

**Documentation only; no Unity code, assets/scenes, builds/tests or branch merge.**


## Assassin Tier III Round 11 — 2026-11-27 USER CONFIRMS AGI-ONLY OFFENSE, EXACT LV1 STARTING ATTRIBUTES, 100 CLASS BASE MANA, GLASS CANNON; GROWTH CUSTOM UNDECIDED

### USER-APPROVED ONLY

1. **ASSASSIN BASIC ATTACKS AND PHYSICAL SKILLS SCALE AGI-ONLY**, not a STR/AGI split, nor Psion-like INT hybrid. Exact numeric attack/stat damage coefficients still OPEN. Natural autoattacks remain Physical; existing class-specific ability elements/split packets require separate mechanical approval.
2. **ASSASSIN TIER III CLASS LEVEL 1 STARTING ATTRIBUTE BASES:** `STR=4, AGI=20, INT=5, SPR=4, VIT=5` (total **38**, not 39). This is the new TierIII Assassin class manifestation baseline, NOT inherited additive TierII Thief stats. The Lv1 total is far below the approved Berserker81 and FireMage84; this dramatic tradeoff is NOT being silently overwritten.
3. **ASSASSIN CLASS BASE MANA = 100.** Use the already approved UNIVERSAL formula `MaxMana=100+5*INT`; with starting INT5, classLv1 **MaxMana=125** excluding gear/temporary bonuses. Universal `ManaRegenPerSecond=4+0.1*SPR`; with starting SPR4, classLv1 **ManaRegen=4.4/s**. This approves the class base Mana, but NOT any numeric Assassin SKILL Mana costs; user explicitly deferred those until after stat distribution.
4. **CLASS SURVIVABILITY = GLASS CANNON:** comparatively low HP/defenses, compensated by heavy slow dual-dagger damage, devastating Stealth opener and position mechanics. This is NOT approval for a specific dodge chance, immune state, extra resource or evasion-tank role.
5. **PER-LEVEL ATTRIBUTE GROWTH = CUSTOM, REQUESTS A TABLE SUGGESTION BASED ON PRIOR TIER III CLASSES.** USER HAS NOT CHOSEN SPECIFIC GROWTH NUMBERS. All proposed growth numbers below are explicitly DESIGNER RECOMMENDATION, not user approval.

### Comparison researched directly from existing DiceFree docs (distinguish approved numbers vs proposals)

| Class/source | Lv1 starting VIT/STR/AGI/INT/SPR | Per-level VIT | STR | AGI | INT | SPR | Total per level | Status of growth values |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| Berserker Tier III | 25/34/10/5/7 (total81) | +3.0 | +6.0 | +1.2 | +0.5 | +0.8 | **+11.5** | Latest user-approved, `BERSERKER_TIER3_DESIGN.md` Round11 |
| Fire Mage Tier III | 19/7/12/33/13 (total84) | +2.0 | +0.75 | +1.0 | +6.0 | +1.5 | **+11.25** | Latest user-approved `MAGE_TIER3_DESIGN.md` |
| Psionic Knight Tier III | 22/8/30/22/11 (total93), *working proposed starts* | +2.5 | +1.0 | +5.0 | +2.7 | +1.0 | **+12.2** | Growth user-corrected in `PSION_TIER3_EVOLUTIONS_DESIGN.md` Round21; starts presented as proposals/working not independently re-approved there |
| Mirage Mage Tier III | 19/8/22/32/11 (total92), *working proposed starts* | +2.0 | +1.0 | +2.7 | +5.0 | +1.0 | **+11.7** | Growth user-corrected in `PSION_TIER3_EVOLUTIONS_DESIGN.md` Round21; starts presented as proposals/working not independently re-approved there |
| Marksman Tier III | 19/10/34/9/11 (total83), *proposed starts* | +2.0 | +0.9 | +6.0 | +1.0 | +1.2 | **+11.1** | `MARKSMAN_TIER3_DESIGN.md` Round6 proposal, **not** treated as confirmed final approved growth here |
| Assassin Tier III | **5/4/20/5/4 (total38), USER APPROVED** | **+1.5** | **+0.5** | **+7.0** | **+1.0** | **+1.0** | **+11.0** | **DESIGNER RECOMMENDATION ONLY; not user-approved growth** |

The above table is transcribed/computed from the class design sources, not an assertion that all numbers are implemented in Unity. The extreme low approved Assassin starting total38 versus other selected TierIII designs in81–93 range should be surfaced for user review, not corrected without permission. TierIII Assassin is a glass cannon at melee range; low initial HP makes gameplay feasibility an important review after player accepts the growth profile.

### Proposed Assassin growth and no-gear progression (NOT USER APPROVED)

**Recommended per Assassin class level up:** `VIT+1.5, STR+0.5, AGI+7, INT+1, SPR+1` = **+11.0 total/level**. Strongest offense in AGI (numerically exceeding Marksman's proposal +6 AGI/level), survivability low relative to Berserker's approved VIT+3 and Fire Mage's VIT+2. Both INT/SPR gain +1 per level to give baseline Mana growth while actual skill costs wait. This is intentionally distinct from earlier generic form options +3 or +4 AGI and honors user's request to compare real TierIII precedent rather than guessing.

| Assassin attribute | USER-APPROVED Lv1 | PROPOSED per level | PROJECTED Lv150 with proposal, no gear |
| --- | ---: | ---: | ---: |
| STR | 4 | +0.5 | 78.5 |
| AGI | 20 | +7 | 1063 |
| INT | 5 | +1 | 154 |
| SPR | 4 | +1 | 153 |
| VIT | 5 | +1.5 | 228.5 |
| Total | **38** | **+11** | **1677** |

General proposed attribute at classLv L: `base+growth*(L-1)`, engine rounding unspecified. If proposal is accepted, no-gear `MaxMana(L)=100+5*(5+L-1)=120+5L`; Lv1=125, Lv60=420, Lv150=870. `ManaRegen/s=4+0.1*(4+L-1)`; Lv1=4.4/s, Lv60=10.3/s, Lv150=19.3/s. These Lv60/Lv150 stats and Mana figures are **conditional projections, not approved class growth**. Global HP policy is existing15 HP/VIT plus engine base (compare current class code), so low VIT is deliberate but actual survivability and class equipment not tested.

### Already approved class kit remains unchanged

- Pure melee dual-dagger glass cannon, slow two-impact basic attack cycle with **equal50/50 of total raw sequence damage**, AGI-only offense; heavy skills and powerful Stealth opener. Side/rear270deg enemy-relative Backstab bonus stacks with Stealth opener, any-facing Backstab cast allowed. Envenom kept separate; Execute scales continuously with missing target HP.
- Skill caps **Stealth25 / Envenom30 / Backstab35 / Execute25 / CriticalMastery20 / ArmorPen15 =150** purchased ranks, one point per Assassin classLv1..150. Stagger first unlocks is approved; the exact 150-level gate schedule in Round10 is still **designer proposal, NOT approved**.
- Critical Mastery passive +3 AGI and +1 percentage point BASIC crit chance per rank and active BASIC-only charge guarantees rank1–5=2,6–10=3,11–15=4,16–19=5,rank20=6, successful hits even fully absorbed spend, misses do not. Rank-dependent Mana cost amount TBD,25s CD begins on use, unspent charges clear at death/logout/class change.
- Stealth modifier `(r-11)%` while hidden, from -10% at first rank to +14% at cap25. ArmorPen +1% target PDEF bypass per rank on basics and Physical Assassin skills, cap15. Universal MaxMana and regen still global, class BaseMana100 approved.

### Round12 pending decisions

1. **Choose/modify Assassin growth** in above table, perhaps redistribute points between VIT/AGI/INT/SPR, or clarify whether Lv1 starting total38 should be intentionally this far below other TierIII starts; DO NOT silently replace approved starts.
2. **Approve/change exact first unlock levels** from Round10 designer schedule (Stealth1/Envenom2/Backstab3/CritMastery5/Execute8/ArmorPen10) and whether accompanying complete rank gate table is acceptable.
3. **Only after growth is approved**, calculate no-gear Mana at levels1/60/150 and revisit every active ability's rank-scaled Mana costs.
4. Set dagger cycle attack interval and impact delay, Backstab/Stealth/Execute raw damage formulas with rank scaling.
5. Design mentor/advancement trial in World2 separately; no location/quest chosen yet.

**DOCUMENTATION ONLY, do not alter Unity game code, assets, scenes, builds/tests or merge Codex work.**


## Assassin Tier III Round 12 — 2026-11-27 USER APPROVES PER-LEVEL GROWTH WITH REDISTRIBUTIONS; WANTS HIGHER LV1 STARTS, NUMBERS UNDECIDED

### AUTHORITATIVE USER ANSWERS, with custom free-text adjustments overriding the multiple-choice initial proposed growth

1. User selected "Approve VIT+1.5 STR+0.5 AGI+7 INT+1 SPR+1 per class level", **then explicitly corrected it:** "**move 0.2 spi into vit and 0.2 int stat gain into str**". Both are redistribution of the PER-LEVEL ATTRIBUTE GAIN figures, not changes to the starting stats. Latest definitive per-level Assassin growth:
   - **VIT +1.7** (previous candidate +1.5 plus0.2 from SPR).
   - **STR +0.7** (previous candidate +0.5 plus0.2 from INT).
   - **AGI +7.0** (unchanged).
   - **INT +0.8** (previous candidate +1.0 minus0.2).
   - **SPR +0.8** (previous candidate +1.0 minus0.2).
   - **TOTAL +11.0 attribute points PER LEVEL** (unchanged aggregate). Apply for each level-up after new Assassin classLv1, subject to global existing fractional-stat rounding. This set is USER-APPROVED; supersedes Round11 +1.5/+0.5/+7/+1/+1 growth PROPOSAL.

2. User selected "**Increase starting attributes, see notes**", BUT the free-text notes specify ONLY **stat gain** transfers, not any new Lv1 starting-stat values or target starting-total budget. Thus user **APPROVES THE DIRECTION OF INCREASING ASSASSIN LEVEL1 BASE ATTRIBUTES**, but **EXACT NEW BASE VALUES ARE NOT APPROVED / UNRESOLVED**. Do NOT silently apply +0.2 adjustments to starting VIT/STR; do not invent a new starting total38->80, and do not claim previous Lv1 numbers still user-endorsed as final. **Last exact approved Lv1 values from Round11 were STR4 AGI20 INT5 SPR4 VIT5 (total38), now explicitly REQUESTED TO BE REVISED UPWARD**, with replacement numbers awaiting next design selection. Suggested candidates may be offered separately, clearly not approved.

3. Approved class BaseMana remains **100** and universal MaxMana=`100+5*INT`, natural regen/s=`4+0.1*SPR`; ONLY the eventual newly approved starting INT/SPR must be plugged into exact classLv1 and later Mana forecasts. **No numeric spell Mana COSTS yet**; Round10 explicitly deferred all Assassin costs until stat distribution is finalized. Round9 Critical Mastery activation Mana cost increases with purchased rank, precise function TBD.

### Approved growth table and conditional calculations based on LAST KNOWN numerical starts (not the requested future increase)

| Stat | Last exact approved Lv1 bases from Round11, now pending increase | FINAL USER-APPROVED per class level growth | CONDITIONAL Lv150 if old bases retained (NOT final) |
| --- | ---: | ---: | ---: |
| VIT | 5 | **+1.7** | 258.3 |
| STR | 4 | **+0.7** | 108.3 |
| AGI | 20 | **+7.0** | 1063.0 |
| INT | 5 | **+0.8** | 124.2 |
| SPR | 4 | **+0.8** | 123.2 |
| Total | 38 | **+11.0** | 1677 |

Formula `Stat(L)=NEW_APPROVED_START + growth*(L-1)`, classLv1 is a separate manifestation, not inherited accumulated Thief stat growth. Table Lv150 is scenario illustration **ONLY IF previously selected bases were kept despite latest intention to increase**. With those outdated bases, conditional MaxMana at Lv1=125, Lv60=`100+5*(5+59*.8)=361`, Lv150=`100+5*(5+149*.8)=721`. Conditional natural regen/s Lv1=4.4, Lv60=`4+.1*(4+59*.8)=9.12`, Lv150=`4+.1*(4+149*.8)=16.32`. These should be RECOMPUTED when increased starting stats are decided; never quote these as final approved class pool sizes.

### Source-relative suggestions for next round (not USER approved)

Prior TierIII references: approved Berserker Lv1 total81, Fire Mage Lv1 total84; working Psion evolution starts approximately92–93, prior Marksman starts83 (proposal). Assassin previously had only38 at Lv1 and is requested to increase. A potential **glass-cannon beginning around total80–85** with **heavy AGI bias and deliberately lower VIT than Fire Mage/Berserker** is a designer direction, NOT a selected number or binding minimum. For next user interview, offer two or three explicit revised Lv1 distribution candidates with exact sum and resulting Lv1 MaxMana, and an option to provide custom numbers. Do not modify selected +1.7/+0.7/+7/+0.8/+0.8 growth unless user instructs.

### Previous locked mechanics retained

AGI-ONLY scaling for Assassin basics and physical skills, pure melee glass cannon dual one-handed daggers, slow two-hit basic sequence with 50/50 raw damage split, devastating Stealth opening and lower sustain, any-angle Backstab with side/rear270° larger positional bonus stacking stealth opener, Envenom retained distinct poison stab, Execute always usable continuously increased damage as target missing HP rises. Stealth ranks25: `(r-11)%` concealed move speed; CriticalMastery20 hybrid passive +3AGI/r and +1pp BASIC crit/r, activation basic-only guaranteed crit charges rank1–5=2,6–10=3,11–15=4,16–19=5,20=6; successful hits even shield-absorbed spend, misses don't,25s CD starts immediately, charge clear on death/logout/class swap; ArmorPen15 +1% target PDEF bypass/rank on basics/physical skills. Six skill rank caps25/30/35/25/20/15=150, one point/classLv1–150. Staggered first skill unlocks approved but Round10 exact150-level gate proposal is still NOT signed off. No class mentor/trial/attack formulas approved beyond described principles.

### Immediate follow-up questions

1. **Choose exact increased TierIII Assassin Lv1 stat distribution:** STR/AGI/INT/SPR/VIT; preserve AGI focus and glass-cannon role without imposing equal class survivability.
2. After revised starts accepted, recompute no-gear Mana/regen and HP vs previous TierIII, then (per user instruction) design actual skill Mana costs.
3. Confirm or alter Round10 designer-first-unlock proposal and its exhaustive audited gate table.
4. Set cooldowns, attack intervals and rank-dependent raw damage curves.

**DOCUMENTATION ONLY; no Unity source/assets/scenes/builds/tests, no code merges.**


## Assassin Tier III Round 13 — 2026-11-27 USER APPROVES REVISED LEVEL1 STATS AFTER TRANSFERS; GROWTH PRESERVED

### Definitive Round13 USER-APPROVED LEVEL1 STARTING DISTRIBUTION

User selects starting candidate **C: VIT15 / STR8 / AGI40 / INT10 / SPR9 (total82)** and explicitly overrides its distribution: "**move 2 agi to str and 3 agi to vit and 4 spi into vit. it can have a bit more survivability at the start and gain more damage as it levels up.**" Apply **all three transfers ONLY to Lv1 starting stats**, not the already approved per-level gains:
- From AGI40: move2 to STR and3 to VIT => **AGI35**.
- STR8 +2 => **STR10**.
- VIT15 +3 from AGI +4 from SPR => **VIT22**.
- SPR9 -4 => **SPR5**.
- INT remains **10**.
- Total remains **VIT22 + STR10 + AGI35 + INT10 + SPR5 =82**.

**LATEST AUTHORITATIVE ASSASSIN Lv1 STATS: VIT22 / STR10 / AGI35 / INT10 / SPR5**, total82. This SUPERCEDES both original Round11 extremely low Lv1 VIT5/STR4/AGI20/INT5/SPR4 total38 and Round13 pre-adjustment C VIT15/STR8/AGI40/INT10/SPR9. Never revert to either earlier set. User explicitly wants more initial survivability with longer-term AGI-heavy damage growth, so do not portray VIT22 as an unintended tank-role change. Assassin remains a PURE MELEE DPS with glass-cannon/burst-oriented identity relative to tank roles, although its early VIT now exceeds some caster/ranged alternatives.

### Separate already USER-APPROVED Round12 stat gain remains UNCHANGED

`VIT+1.7, STR+0.7, AGI+7.0, INT+0.8, SPR+0.8` PER ASSASSIN CLASS LEVEL-UP (total+11). Round13 user explicitly restates and preserves this. Damage scaling remains **AGI-only for basics and physical skills**; extra STR at start does NOT silently add STR offensive coefficients.

| Stat | USER-APPROVED Lv1 start | USER-APPROVED growth/class level | Projected Lv60 before rounding | Projected Lv150 before rounding |
| --- | ---: | ---: | ---: | ---: |
| VIT | **22** | **+1.7** | **122.3** | **275.3** |
| STR | **10** | **+0.7** | **51.3** | **114.3** |
| AGI | **35** | **+7.0** | **448** | **1078** |
| INT | **10** | **+0.8** | **57.2** | **129.2** |
| SPR | **5** | **+0.8** | **52.2** | **124.2** |
| TOTAL | **82** | **+11.0** | **731** | **1721** |

Formula `Stat(L)=Lv1Start + Growth*(L-1)` before equipment, buffs, passive ranks, fractional storage/rounding. Hybrid CriticalMastery passive at rank20 provides **+60 AGI** beyond this base; no assumption it is already fully purchased at Lv1, Lv60 or Lv150.

### Approved globally shared resource formulas, recomputed with final Assassin stats (NO SKILL COSTS APPROVED YET)

USER APPROVED separate `AssassinClassBaseMana=100` Round11, and universal `MaxMana=100+5*INT`, natural `ManaRegenPerSecond=4+0.1*SPR`.

| Class Lv | Base INT, no equipment | Base SPR, no equipment | Max Mana before rounding | Mana regen/sec before rounding |
| ---: | ---: | ---: | ---: | ---: |
| 1 | 10 | 5 | **150** | **4.5** |
| 60 | 57.2 | 52.2 | **386** | **9.22** |
| 150 | 129.2 | 124.2 | **746** | **16.42** |

If existing HP equation `baseHP10+15*VIT` applies unchanged, prospective no-gear HP would be Lv1 **340**, Lv60 **1844.5**, Lv150 **4139.5** before rounding, which is a CALCULATION under current global HP convention rather than separate new class-specific HP approval. Actual engine rounding and skill/class HP modifiers may differ. This stat snapshot now provides a firm numeric basis to propose SKILL MANA COSTS in NEXT ROUND after final stats, satisfying Round10 user instruction. Do not treat earlier discarded `CriticalMasteryCost=20+5r` as approved just because it was selected before user's defer note.

### Other Assassin kit and progression rules unchanged

Six rankable concepts with exact caps **Stealth25 / Envenom30 / Backstab35 / Execute25 / hybrid CriticalMastery20 / ArmorPen15 =150**. One point per classLv1–150. User approved staggered skill first unlocks but Round10 full mathematically audited 150-rank level-gate table remains **DESIGNER PROPOSAL, NOT USER APPROVED**. Assassin enters TierIII Lv1 on Thief TierII Lv60+ advancement; mentor/trial not authored. Dual one-handed daggers, slower TWO-IMPACT basic sequence with equal50/50 damage split, AGI-only offense, much stronger Stealth burst than sustain; Backstab any facing, enhanced side/rear270° and stacks Stealth opener. Envenom independent poison stab, Execute continuous missing target HP scaling. Stealth concealed movement mod `(StealthPurchasedRank-11)%`. CritMastery20 passive +3AGI and+1pp BASIC crit/rank; active rank-scaled BASIC-only guaranteed crit charges 2/3/4/5/6 at milestone r1/6/11/16/20,25sec cooldown begins on activation, charges clear at death/logout/class change and reset/replace on recast, only successful basic dagger hits spend charges even if shield absorbs all damage. ArmorPen15 +1% target PDEF bypass per rank on basics and physical Assassin skills. CritMastery Mana cost must rise with rank; exact all-ability Mana formulas unresolved. Exact skill rank gates, raw coefficients/CDs, baselines and visuals remain open.

### Round14 recommended design order (NOT YET APPROVED)

1. With Lv1 stats and per-level growth locked, **choose numeric Mana cost formulas** for five actives including hybrid activation, compare against max Mana at Lv1=150/Lv60=386/Lv150=746 and regen4.5/9.22/16.42 per second. Design realistic rank curves, avoid max-rank casts costing more than entire intended mana pool at expected unlock level.
2. Confirm/adjust **audited proposed staggered six-skill first unlock levels** Stealth1/Envenom2/Backstab3/CritMastery5/Execute8/ArmorPen10 and full 150 individual gates, all still non-approved.
3. Define Assassin basic attack cycle interval and impact gap, relative total damage vs Thief.
4. Define Backstab side/rear positional % bonus, stacking stealth opening multiplier, and Execute missing-health damage curve with purchased rank scaling.

**DESIGN DOCUMENTATION ONLY. No Unity gameplay code/assets/scenes/builds/tests, no merges; commit/push docs each completed round.**


## Assassin Tier III Round 14 — 2026-11-27 USER APPROVES MODERATE MANA PHILOSOPHY, 2.5S BASIC CYCLE/0.3S HIT GAP, EXECUTE MUST BE LAST SKILL UNLOCKED

### LATEST AUTHORITATIVE USER ROUND14 ANSWERS (custom note overrides original level-gate proposal)

1. **MANA-COST PHILOSOPHY: MODERATE.** Assassin should be able to use regular active skills while conserving Mana for important burst windows, as contrasted with permanently expensive/high-commitment casts or trivial Mana costs. This is an approved pacing/design direction, NOT any exact per-skill Mana numbers; class statistics and resource capacity are now approved (Assassin Lv1 VIT22 STR10 AGI35 INT10 SPR5; growth VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8; class BaseMana100; MaxMana100+5INT; regen4+0.1SPR). No unapproved skill cost or formula should be treated as decided. Critical Mastery activation still specifically grows Mana cost with purchased skill rank, numeric function TBD.
2. **DELIBERATELY SLOW BASIC CYCLE: ONE TWO-DAGGER-HIT BASIC ATTACK SEQUENCE EVERY 2.5 SECONDS**, with **SECOND HIT 0.3 SECONDS AFTER FIRST HIT**. Previous Round10 equal split **50% + 50% of one sequence's raw attack damage** remains in force, not 100% each packet. These are nominal unmodified baseline attack cadence and impact spacing, before any global attack-speed, animation or interruption rules; exactly how general attack-speed modifiers rescale entire cycle and inter-impact gap remains to be implemented/defined globally. Separately resolve each dagger hit's hit roll, basic crit eligibility, damage mitigation, Stealth first-cycle multiplier and Critical Mastery basic guaranteed-crit charge consumption on successful hit (including shield absorbed), miss preserves charge. Do not spawn an independent offhand attack timer.
3. **EXECUTE MUST BE THE LAST ASSASSIN SKILL TO UNLOCK**, explicitly requested in user free text. User's selection was "Adjust skill unlocks/rank schedule in notes" with only correction "**execute should be the last skill to unlock**". This SUPERSEDES older Round10 **DESIGNER-ONLY** initial first-unlock sequence where Execute was Lv8 and ArmorPen Lv10, which made Execute NOT last. **EXECUTE FINAL-UNLOCK ORDER is APPROVED**, but NO NUMERIC first-unlock level for Execute or exact per-rank schedule was explicitly selected by user. The full revised gated schedule below is a rigorously audited DESIGNER PROPOSAL, NOT auto-approved by this request. Ask user to confirm/change before claiming gates are locked.

### Proposed NEW full 150-level rank-gate table, designer-only, all eligibility levels tested

Proposed first skill access: **Stealth Lv1, Envenom Lv2, Backstab Lv3, Critical Mastery Lv5, Armor Penetration Lv8, EXECUTE Lv12 (last)**. Exact values **NOT YET USER APPROVED**. The previously approved skill caps remain **Stealth25, Envenom30, Backstab35, CriticalMastery20, ArmorPen15, Execute25**, total 150 purchased ranks and one skill point per Assassin class level1..150.

Each comma-separated number at a skill represents the proposed minimum Assassin CLASS LEVEL for its consecutively purchased rank r1 through max rank. The designer audit built a legal single-purchase-per-class-level progression with rank prerequisite order maintained; **all 150 class levels 1..150 occur exactly once across the six gate rows**, so a straightforward spend-as-you-level path never has to bank a point, and all caps can be reached by class Lv150. **No specific numeric gate is represented as already user-approved.**

| Skill | User-approved cap | Proposed first unlock class Lv | Last rank eligibility class Lv | Full proposed class-level gates per rank (in ascending purchased rank) |
| --- | ---: | ---: | ---: | --- |
| Stealth | 25 | 1 | 147 | 1, 7, 15, 21, 27, 33, 39, 45, 51, 57, 63, 69, 75, 81, 87, 93, 99, 105, 111, 117, 123, 129, 135, 141, 147 |
| Envenom | 30 | 2 | 149 | 2, 6, 11, 18, 23, 29, 32, 38, 43, 48, 53, 59, 62, 68, 73, 78, 83, 89, 92, 98, 103, 108, 113, 119, 122, 128, 133, 138, 143, 149 |
| Backstab | 35 | 3 | 150 | 3, 4, 10, 16, 20, 24, 30, 31, 37, 41, 46, 50, 54, 60, 61, 67, 71, 76, 80, 84, 90, 91, 97, 101, 106, 110, 114, 120, 121, 127, 131, 136, 140, 144, 150 |
| Critical Mastery | 20 | 5 | 146 | 5, 9, 19, 26, 35, 42, 49, 56, 65, 72, 79, 86, 95, 102, 109, 116, 125, 132, 139, 146 |
| Armor Penetration | 15 | 8 | 145 | 8, 14, 25, 36, 44, 55, 66, 74, 85, 96, 104, 115, 126, 134, 145 |
| Execute | 25 | 12 | 148 | 12, 13, 17, 22, 28, 34, 40, 47, 52, 58, 64, 70, 77, 82, 88, 94, 100, 107, 112, 118, 124, 130, 137, 142, 148 |

**Validation:** six caps add to150; total exactly150 gate entries; each integer classLv1..150 occurs exactly once; per-skill rank order strictly increasing; all proposed first unlocks precede its other ranks and Execute's first Lv12 follows ArmorPen's first Lv8, making Execute last. This is an accessible, mathematical proposal rather than a user-signed-off new system.

### Already approved unchanged, do not regress

- TierIII Assassin separate Lv1 manifestation after TierII Thief Lv60+, pure melee dual one-handed daggers, AGI-only basic and Physical skill scaling, heavy positional burst from Stealth rather than high sustained autoattack throughput. Final exact starting stats VIT22 STR10 AGI35 INT10 SPR5, per-level gain VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8. ClassBaseMana100; global formulas MaxMana100+5INT (no-gear L1=150/L60=386/L150=746), ManaRegen/s4+0.1SPR (L1=4.5/L60=9.22/L150=16.42), pre-rounding.
- Stealth cap25 signed concealed move speed `(rank-11)%` from -10% at rank1 to +14% at rank25, opening two-hit basic attack sequence gains opener multiplier on BOTH hits but opener consumes once for sequence. Backstab any-angle with much higher side/rear270deg positional damage and separate Stealth opener multiplier that stacks with positional bonus; Execute always usable and grows continuously with target missingHP, distinct from Envenom physical poison stab.
- Critical Mastery20 hybrid +3AGI and +1pp BASIC crit chance per rank; active BASIC-only guaranteed critical hit charges rank1–5=2,6–10=3,11–15=4,16–19=5,rank20=6; consumes only successful BASIC dagger hits including fully shield-absorbed hits, misses preserve charges,25s cooldown begins immediately, charge refill replaces, charges clear on death/logout/class swap. Activation Mana cost grows with rank, exact cost unresolved. Armor Penetration15 ignores 1% target PDEF per rank on basics and Physical skills (max15%).
- No Fan of Knives, Smoke Bomb, Expose Weakness or separate Guaranteed Crit active. Rank caps 25/30/35/25/20/15 fixed; only class-level gate numbers still open.

### Next Round15 recommended questions, all not approved yet

1. **Approve or adjust proposed FIRST unlock order/numbers and entire audited rank-gate schedule**, distinguishing specifically that Execute must be FINAL skill unlocked from exact level12 proposal.
2. **Assassin five activated skill Mana costs** (Stealth, Envenom, Backstab, Execute, Critical Mastery): candidate *moderate* base + rank growth curves chosen against level-appropriate unlocked Mana budgets, not arbitrary high-cost skills. Per-user "decide Mana costs after stat distribution" condition now met.
3. **Core two-hit sequence attack damage coefficient versus Thief baseline** at 2.5-sec cycle and 0.3-sec gap; ensure 50%+50% combined damage rather than accidental double.
4. **Skill raw damage formulas** and cooldowns with skill-rank coefficients, Stealth and Backstab opening synergy, Execute continuous missingHP damage, Envenom physical+poison tick.
5. **Rank gating vs special charge breakpoints**, rank20 crit extra charge progression timing based on unlock table, not assumed free at TierIII Lv1.

**DOCUMENTATION ONLY. Do not edit Unity code/assets/scenes/builds/tests, or merge branches.**


## Assassin Tier III Round 15 — 2026-11-27 USER APPROVES ALL EXACT 150 SKILL RANK LEVEL GATES; DESIGNER PROPOSES RANK-SCALED MODERATE MANA COSTS (NOT APPROVED)

### USER APPROVAL, AUTHORITATIVE

User explicitly says "**Approve first unlocks Stealth1 Envenom2 Backstab3 CriticalMastery5 ArmorPen8 Execute12 and entire revised audited 150-rank gate schedule**." Therefore Round14's exact level gate table is **NOW USER-APPROVED**, not designer-only. Preserve it verbatim: every integer in each gate list below is the first Assassin class level allowing the next purchased rank, in ascending rank order, and Execute unlocks LAST at classLv12. Assassin earns one skill point each class Lv1..150. TierIII class Lv starts at1 as a separate manifestation after eligible TierII Thief advancement; do not transfer old skill ranks.

### LOCKED, AUDITED 150-RANK SKILL LEVEL-GATE TABLE

| Skill | Approved rank cap | Approved first unlock class Lv | Last rank class Lv | EXACT approved minimum class levels for purchased rank1..cap |
| --- | ---: | ---: | ---: | --- |
| Stealth | 25 | 1 | 147 | 1, 7, 15, 21, 27, 33, 39, 45, 51, 57, 63, 69, 75, 81, 87, 93, 99, 105, 111, 117, 123, 129, 135, 141, 147 |
| Envenom | 30 | 2 | 149 | 2, 6, 11, 18, 23, 29, 32, 38, 43, 48, 53, 59, 62, 68, 73, 78, 83, 89, 92, 98, 103, 108, 113, 119, 122, 128, 133, 138, 143, 149 |
| Backstab | 35 | 3 | 150 | 3, 4, 10, 16, 20, 24, 30, 31, 37, 41, 46, 50, 54, 60, 61, 67, 71, 76, 80, 84, 90, 91, 97, 101, 106, 110, 114, 120, 121, 127, 131, 136, 140, 144, 150 |
| Critical Mastery | 20 | 5 | 146 | 5, 9, 19, 26, 35, 42, 49, 56, 65, 72, 79, 86, 95, 102, 109, 116, 125, 132, 139, 146 |
| Armor Penetration | 15 | 8 | 145 | 8, 14, 25, 36, 44, 55, 66, 74, 85, 96, 104, 115, 126, 134, 145 |
| Execute | 25 | 12 | 148 | 12, 13, 17, 22, 28, 34, 40, 47, 52, 58, 64, 70, 77, 82, 88, 94, 100, 107, 112, 118, 124, 130, 137, 142, 148 |

**PROGRAMMATIC DESIGN AUDIT RE-PASSED THIS ROUND:** Skill cap counts25+30+35+20+15+25=150. Every skill's exact rank gate list length equals its approved cap, each rank's class-level gate strictly increases within that skill, the union of all gate lists equals **every integer Assassin class level 1..150 exactly once**, and Execute's first eligibility at Lv12 is later than all others' first eligibility (Stealth1 Envenom2 Backstab3 CriticalMastery5 ArmorPen8). Thus spending one point at each character-class level in this scheduled order satisfies each previous-rank prerequisite, never requires a banked point, and reaches all caps by Lv150. This is the **approved** table, not subject to further uncertainty unless user changes it. A player may choose to bank points instead of buying immediately; min-level requirements still apply.

### Round15 designer-only proposed MODERATE rank-scaled MANA cost schedule (NOT user approved)

User requests "Continue Assassin design and design rank-scaled Mana costs" but chooses ONLY gate approval and does not provide numeric Mana cost choices. The proposals below are **new DESIGNER CANDIDATES FOR NEXT USER APPROVAL**, based on earlier approved moderate resource philosophy and FINAL known Assassin stats/resource curve.

| Active ability (one Mana payment on activation) | Candidate exact formula (purchased rank r) | Rank1 cost | At approved cap | Cap cost |
| --- | --- | ---: | ---: | ---: |
| Stealth (max rank25) | `8+2*r` | 10 | r25 | 58 |
| Envenom (max rank30) | `10+2*r` | 12 | r30 | 70 |
| Backstab (max rank35) | `12+2*r` | 14 | r35 | 82 |
| Execute (max rank25) | `15+3*r` | 18 | r25 | 90 |
| Critical Mastery activation (max rank20) | `12+3*r` | 15 | r20 | 72 |
| Armor Penetration (max rank15) | PASSIVE, NO ACTIVE MANA COST | n/a | r15 | n/a |

**NONE OF THESE MANA COST NUMBERS OR FUNCTIONS ARE YET APPROVED.** They are only moderate-rank-cost candidate values for user to tune or reject in Round16. The old pre-stats selection `Critical Mastery 20+5r` was explicitly deferred/overridden in Round10 and remains NOT APPROVED; don't reintroduce as canonical. In all candidates, a skill's Mana is deducted **once per activated cast**, with no invented Mana-per-dagger-hit or additional Mana-per-poison-tick drain, unless later user changes. Critical Mastery activation cost rises with its skill rank, satisfying an existing approved principle. All physical skills remain AGI-only for damage coefficients and global Mana formula is UNCHANGED.

Ungeared before rounding, Assassin's previously approved resource trajectory is:
- Lv1: Starting VIT22 STR10 AGI35 INT10 SPR5; `MaxMana=100+5*INT=150`, regen `4+0.1SPR=4.5/s`.
- Lv12: INT=`10+11*0.8=18.8`, Mana `194`, SPR=`5+11*0.8=13.8`, regen `5.38/s`; Execute first rank becomes eligible at Lv12, candidate cast cost18 Mana.
- Lv60: Mana386, regen9.22/s.
- Lv150: Mana746, regen16.42/s.

**Five candidate max-rank cast costs TOTAL =58+70+82+90+72=372 Mana (~49.87% of classLv150 no-gear pool746)**, if all five actives are cast ONCE at max rank and there are no intervening regen effects; not a full opener rotation simulation or playtested balance. They do not automatically fire or all occur in the same instant, and actual cooldowns/combat constraints may restrict use. Candidate rank1 costs are affordable at the respective first unlock levels under approved base Mana values, without inferring actual player gearing. Verify balance at intermediate rank caps and timing once ability cooldowns and damage are locked.

### Already-approved Assassin specification remains unchanged

- Pure melee DPS, two one-handed daggers, AGI-only basic and physical skill stat scaling, class Lv1 starts VIT22 STR10 AGI35 INT10 SPR5, per level VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8, class BaseMana100, natural regen global. Durability front-loaded a bit, damage grows quickly with class AGI.
- Deliberate two-hit basic attack cycle 2.5s, second strike0.3s after first, equal 50/50 share of TOTAL raw cycle damage. Stealth first opening two-hit basic sequence multiplies BOTH impacts, but consumes a single opener. Backstab usable from any angle with huge positional bonus outside target front90deg (target side/rear270deg), and multiplier from first Stealth opening stacks with positional bonus. Envenom retains poison; Execute always usable, continuous missingHP damage curve. User-approved order first unlocks Stealth1 Envenom2 Backstab3 CritMastery5 ArmorPen8 Execute12 LAST.
- Critical Mastery hybrid rank20 passive +3AGI and +1pp BASIC crit chance per rank, BASIC-only guaranteed critical hit charges rank1–5=2/r6–10=3/r11–15=4/r16–19=5/r20=6, 25s activation cooldown starts immediately, charges lost on death/logout/class change, replenishment replaces remaining charges, each successful basic dagger impact consumes one including fully shield-absorbed; misses do not. Stealth cap25 concealed speed `(r-11)%`, ArmorPen cap15 ignores 1% target Physical Defense per purchased rank on basic attacks and Physical Assassin skill damage.
- Missing mechanics: skill damage coefficients and rank curves, precise cooldowns for Stealth/Envenom/Backstab/Execute, Stealth duration/RankIII opener multiplier, Backstab positional magnitude and timing, Execute missingHP coefficient, basic TOTAL raw sequence damage relation to TierII Thief, assets/teacher quest and exact cost choices above.

### Recommended Round16 questions, all still open

1. **Mana cost approval:** accept/tune/reject candidate 8+2r Stealth,10+2r Envenom,12+2r Backstab,15+3r Execute,12+3r Critical Mastery, or alter all to stronger/weaker economic pacing. Explicitly confirm no extra Mana on poison ticks.
2. **Skill damage coefficient balancing priority**: Backstab position bonus and Stealth opener raw rank scaling first, then Execute missing HP and Envenom DoT/direct.
3. **Cooldown pacing** for four active skills; Critical Mastery 25s already approved.
4. **Basic raw DPS vs TierII Thief**: choose raw AGI cycle coefficient at2.5s and timing gap0.3s and ensure it is slower, hard-hitting.
5. **Advancement teacher/quest** and world location remain undecided.

**DOCUMENTS ONLY. No Unity gameplay code/assets/scenes/builds/tests/merges authorized.**


## Assassin Tier III Round 16 — 2026-11-27 USER REQUESTS EXECUTE AS EXPENSIVE HIGH-DAMAGE LONG-COOLDOWN TIMED FINISHER; NUMERIC COSTS AND COEFFICIENTS PROPOSED, NOT APPROVED

### LATEST USER DECISION, authoritative

User explicitly chooses **"Custom individual Mana cost changes in notes"** and states: "**make execute a bit more expensive, it should be like a high damage longer cd, expensive move that you kiind of time to use and stuff.**"

1. **EXECUTE IS THE ASSASSIN'S INTENTIONAL, HIGH-DAMAGE, LONGER-COOLDOWN, MORE MANA-EXPENSIVE SINGLE-TARGET FINISHER**, not a cheap rotational strike. It remains usable at ANY target current HP, with damage increasing CONTINUOUSLY with the enemy's missing-HP percentage (Round5) and rank-scaled physical AGI-only coefficient. Strongest timing is after setup/positioning and as enemy becomes wounded. This is the **approved qualitative DIRECTION** for Execute Mana relative to other active attacks, cooldown and rank-scaling, with no exact user-selected numeric cost, damage or seconds this round.
2. **NO blanket approval of the other FOUR numeric Mana formulas** occurred: Round15 offered Stealth8+2r, Envenom10+2r, Backstab12+2r, CritMastery12+3r, and Execute15+3r as designer examples. User chose CUSTOM and singled out Execute for higher cost. Continue treating four other cost functions as **unchanged working DESIGNER PROPOSALS, NOT USER-APPROVED**, pending explicit response. The old Execute15+3r candidate is **SUPERSEDED AS A DESIGN DIRECTION**, not canonically replaced by any concrete new numeric formula until user accepts one. Global moderate Mana pacing for normal skills and reserved burst capacity remains approved.
3. **ALL EXACT DAMAGE COEFFICIENTS AND ACTIVE COOLDOWNS EXCEPT CRITICAL MASTERY'S ALREADY APPROVED 25S ARE NOT YET USER APPROVED.** They are modeled below to provide tangible Round17 questions without pretending the user's qualitative note decided particular numbers. In particular, do not silently lock a 35s Execute CD or a cost formula in implementation.

### Designer-only proposed numeric Mana costs and cooldowns — TO APPROVE IN ROUND17

Let `r` be each skill's own purchased rank, `B` the class's total NORMAL UNCRITTED RAW two-hit basic SEQUENCE damage before target-defense mitigation (the user approved two impacts each 0.5B separated0.3sec, sequence2.5sec). Let `m=clamp(1-targetCurrentHP/targetMaxHP,0,1)` be target's missing-health fraction. `A=AGI`, the ONLY offensive stat contributing to Assassin's basic and physical abilities (damage `B(A,weapon)` must still be finalized relative to actual global attack formula; do not assume B equals exactly AGI without review). Skill rank coefficients below are DIMENSIONLESS multiples of B, and therefore grow with skill rank, matching user's project-wide rank-scaling rule. **None are user-locked.**

| Skill | Maximum rank | PROPOSED Mana cost per use | Rank1 cost | Max-rank cost | PROPOSED cooldown |
| --- | ---: | --- | ---: | ---: | --- |
| Stealth | 25 | `8+2r` | 10 | 58 | **22s** |
| Envenom | 30 | `10+2r` | 12 | 70 | **8s** |
| Backstab | 35 | `12+2r` | 14 | 82 | **9s** |
| Execute | 25 | **`20+5r`** (stronger than discarded15+3r candidate) | **25** | **145** | **35s** |
| Critical Mastery hybrid active | 20 | `12+3r` | 15 | 72 | **25s USER APPROVED, unchanged** |
| Armor Penetration passive | 15 | none (passive only) | - | - | none |

**Execute's suggested cost max145 is ~19.4% of Assassin's no-gear Lv150 MaxMana746** (vs old cap90 ~12.1%), so timing matters. Its proposed rank1 cost25 is ~12.9% of classLv12 MaxMana194. All five candidate maximum-rank activations once cost58+70+82+145+72=**427 Mana**, ~57.2% of unmodified Lv150 pool746, before natural regen at16.42/s, and before weapon stats, CD constraints or the fact skill ranks unlock progressively. **These are theoretical budget checks, not playtest validation**. An Execute whose target is fullHP can still be cast but yields less damage for the same Mana/CD and is strategically inefficient.

### Designer-only proposed raw DAMAGE SCALING and cooldown interactions — NOT approved

**Keep all class attack and skill damage AGI-only** and derive physical scaling from shared B(A,dagger gear), preserving already approved global physical/armor system and equal basic50/50 packets. The following coefficient curves are deliberately rank-dependent and avoid defining a parallel new base AGI damage equation without verifying runtime convention:

- **Stealth opening damage multiplier proposal** on qualifying first basic sequence OR initial qualifying Backstab/Envenom, with specific eligible skills clarified separately: `StealthOpener(r) = 1.10+0.03r` (r1=1.13x, cap25=1.85x). Previously approved: BOTH dagger packets of first Stealth breaking basic sequence receive same multiplier (only ONE opener consumed) and opening Backstab gains multiplier stacking with positional buff. Whether Envenom, Execute and poison tick receive opener requires explicit design; no automatic blanket inclusion. Existing Stealth rank-based move speed `(r-11)%` and duration mechanics are separate.
- **Backstab direct Physical packet candidate:** `B * (0.90+0.07r)` (r1=0.97B, cap35=3.35B). On target SIDE/REAR 270deg eligible area, multiply by `1.25+0.02r` (r1=1.27x, cap35=1.95x), no positional multiplier for target FRONT90deg. If first Backstab out of Stealth qualifies, ALSO multiply by `StealthOpener(stealthRank)`. Separate levels determine distinct multipliers. At max unstealthed eligible side/rear packet ~=3.35*1.95=6.53B; including max Stealth opening ~=12.08B before defense, crit and other effects. These high burst values are illustrative and demand later balance review; they are **NOT authorized**. All attack skill damage coefficients scale with their own ranks, not solely flat AGI value.
- **Execute always-usable direct Physical packet candidate:** `B*(1.00+0.09r)*(1+(0.30+0.03r)*m)`; at r1 fullHP m0=1.09B, at max r25 fullHP=3.25B, at max with m0.5=4.96B, at nearly zero target HP m1~=6.66B. MissingHP increases damage continuously without a rigid30%-only gate. The rank of Execute scales BOTH base coefficient and missingHP bonus coefficient. Does NOT automatically crit through basic-only Critical Mastery charges. On same rank25 proposed **35s CD and145 Mana**, for deliberate setup and finishing.
- **Envenom initial Physical stab candidate:** `B*(0.80+0.055r)` =0.855B at r1 to2.45B r30. **Nature poison supplementary DoT total candidate** `B*(0.25+0.03r)` over existing inherited three 1-second ticks, giving0.28B total at r1 and1.15B total at r30. This separation means the immediate direct packet is Physical and the poison is a distinct Nature effect, and ArmorPen's applicability to nonphysical poison ticks is NOT implied. Exact tick rank coefficient, duration, refresh stacking, snapshots and target status still require user approval. Do not assert direct Envenom is a free critical attack just because base damage comes from B.
- **Normal two-dagger basic packet remains 0.5B each on an attack every2.5sec, with0.3sec between impacts.** `B` global normalization versus TierII Thief still OPEN; do not accidentally increase baseline by giving each of TWO impacts full B. Parent/weapon scaling and the separate Critical Mastery +3AGI/r passive, basic crit+1pp/r and basic-only guarantee charges affect B and each hit through approved existing systems.
- Proposed cooldowns are **Envenom8s, Backstab9s, Stealth22s, Execute35s**, with CritMastery25s already USER APPROVED. Proposed CDs are static across ranks unless later approved otherwise. **High Execute CD must outlast ordinary rotational attack cooldowns**, user-approved comparative principle.

### Progression, stats and prior approvals preserved

- Round15 user finally approved EXACT 150 single-rank/class-level eligibility gates from Round14, first unlocks Stealth1 Envenom2 Backstab3 CriticalMastery5 ArmorPen8 Execute12 LAST, with caps25/30/35/20/15/25. Full gate lists in Round15 authoritative table; do not recast them as only suggestions.
- Final Lv1 VIT22 STR10 AGI35 INT10 SPR5, growth VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8; ClassBaseMana100, global MaxMana100+5INT and regen/s4+0.1SPR. Pure dual-dagger AGI-only Physical DPS with intentionally meaningful Lv1 survival and fast-growing AGI, Stealth burst and positioning focus.
- Critical Mastery hybrid rank20 passive +3 AGI and +1 percentage point BASIC crit/r, active BASIC-only hit charges2/3/4/5/6 based on r1/6/11/16/20,25sec CD starts activation, charges clear on death/logout/class swap, rerun replaces leftover, successful basic dagger hits spend even if shield absorbs, misses don't. Existing ArmorPen +1% target PDEF ignored/rank (cap15) on basics and physical skills. Stealth move modifier rank-dependent `(r-11)%`. Backstab usable all angles, side/rear270deg greater damage and opener stacks with positional. Execute always usable and missingHP-continuous; rank-dependent curves remain drafts.
- No class implementation, item art or advancement teacher selected.

### Round17 decision suggestions (NOT user approved yet)

1. **Execute cost and CD:** approve/revise 20+5r (r1=25, r25=145) and35s, or user can pick another larger cost/longer cooldown.
2. **Other four costs and cooldowns:** approve/revise Stealth8+2r /22s, Envenom10+2r /8s, Backstab12+2r /9s, CritMastery12+3r /25s user-fixed. Maybe approve cost formulas as one group except Execute.
3. **Rank-scaled damage proposed**: accept/revise Backstab raw+positional, Stealth opener rank curve and Execute missingHP growth, with high burst multiplication checks.
4. **Envenom Nature poison** direct/dot coefficients and whether critical proc/Stealth opener applies; global B normalization vs Thief.
5. **Execute target state** and animation telegraph: immediate vs windup and cancellation/refund rules (not implicitly approved by "time it").

**DOCS ONLY; no Unity gameplay code/assets/builds/tests or branch merges authorized.**


## Assassin Tier III Round 17 — 2026-11-27 USER APPROVES ALL FIVE EXACT RANK-SCALED MANA COSTS AND COOLDOWNS, EXECUTE 1.2S WINDUP; ALL DAMAGE FORMULAS AWAIT INDIVIDUAL REVIEW

### LATEST AUTHORITATIVE USER APPROVALS

User says:
1. "**Approve 20+5r Mana, 35s cooldown**" for Execute.
2. "**Approve Stealth 8+2r/22s, Envenom 10+2r/8s, Backstab 12+2r/9s, Critical Mastery 12+3r/25s**".
3. "**Review damage formulas skill by skill before approval**".
4. "**1.2s windup then one dagger strike**" for Execute.

The EXACT numeric rank-scaled activation Mana costs and cooldowns are NOW USER-APPROVED, superseding the Round15/Round16 DESIGNER-ONLY labeling for these specific values:

| Active Assassin skill | User-approved Mana cost PER CAST, purchased rank r | Rank1 Mana | Cost at user-approved rank cap | User-approved cooldown |
| --- | --- | ---: | ---: | ---: |
| Stealth, cap25 | **8+2*r** | **10** | **58** | **22 seconds** |
| Envenom, cap30 | **10+2*r** | **12** | **70** | **8 seconds** |
| Backstab, cap35 | **12+2*r** | **14** | **82** | **9 seconds** |
| Execute, cap25 | **20+5*r** | **25** | **145** | **35 seconds** |
| Critical Mastery hybrid activation, cap20 | **12+3*r** | **15** | **72** | **25 seconds** (earlier approved, reaffirmed) |
| Armor Penetration, cap15 | PASSIVE, NO ACTIVE MANA CAST | n/a | n/a | n/a |

- All cost functions are **Mana paid for one activation/cast**. Do NOT create extra per-basic-dagger-impact, per-poison-tick, or per-second Mana drainage not authorized. Passive Crit Mastery stat bonuses and passive Armor Penetration do not cost Mana to remain active.
- Exact Mana resource formula stays globally `MaxMana=AssassinClassBaseMana+5*INT`, `naturalRegen/s=4+0.1*SPR`, Assassin BaseMana100, Lv1 stats VIT22 STR10 AGI35 INT10 SPR5 and per-class level VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8. No-gear Mana Lv1=150, Lv12=194, Lv60=386, Lv150=746; regen/s4.5/5.38/9.22/16.42 respectively, before rounding. The five MAX-rank cast costs total `58+70+82+145+72=427` Mana, ~57.24% of no-gear classLv150 pool746 if all cast once without intervening regeneration; theoretical resource check, NOT playtested balance. Execute max-rank Mana145 alone ~=19.44% of no-gear Lv150 pool. Execute must feel more expensive and longer cooldown than regular abilities; these values now enforce it.
- For four first-mentioned skills (Stealth/Envenom/Backstab/Execute), user approves NUMERIC cooldowns but has not separately specified the detailed cooldown START event, refund/interruption handling, and any GCD interactions. Retain existing engine-standard behavior until separately designed; do not assume these details follow from approving seconds. Critical Mastery cooldown **starts immediately on activation**, already user-approved Round9.

### EXECUTE PRESENTATION AND WINDUP NOW USER APPROVED

- **ONE 1.2-SECOND WINDUP, THEN ONE POWERFUL SINGLE-DAGGER STRIKE** as Execute's attack sequence; not instant, not 0.6-second windup, not two dagger hits. The 1.2s is the pre-impact windup (subject to game's existing animation timing integration) and the actual damage resolves on strike impact. A 35sec cooldown and more expensive 20+5r Mana reinforce deliberate timing.
- Execute remains **usable at ANY target HP**, with damage continuously increasing against targets with higher MISSING HP. It is **AGI-ONLY PHYSICAL skill scaling** by existing class rules, NOT a guaranteed-critical basic attack and DOES NOT consume hybrid Critical Mastery guaranteed basic crit charges. Its position/facing and Stealth opener eligibility are not expanded from earlier approvals without user agreement.
- **NOT YET DECIDED:** cast interruption by CC, movement and target death/LoS; whether Mana is spent at windup start or on successful strike; refunds on interruption; whether cooldown starts on cast start or successful resolution; whether cast is canceled when player changes range; attack-speed effects on windup; exact animation asset/VFX. Record as open rather than silently finalizing those behaviors.

### DAMAGE COEFFICIENTS: USER EXPLICITLY DECLINES BULK APPROVAL

User says "**Review damage formulas skill by skill before approval**". Therefore ALL prior Round16 damage functions remain **DESIGNER PROPOSALS, NOT SIGNED-OFF DAMAGE NUMBERS**:
- Stealth opener `1.10+0.03*r` (rank1 1.13x, rank25 1.85x), to review FIRST. Approved regardless of coefficients: BOTH hits of first two-hit BASIC sequence breaking Stealth receive opener multiplier; when Backstab breaks Stealth its opener stacks with qualifying side/rear positional multiplier. The opener is consumed once for the triggering attack/sequence.
- Backstab base packet `B*(0.90+0.07*r)` and eligible side/rear270deg positional multiplier `1.25+0.02*r`, multiply by Stealth opening multiplier if that Backstab is the qualifying Stealth-breaking skill; review SECOND to ensure rank35 positional burst is reasonable. Backstab usable from every angle; front90deg NO positional bonus.
- Execute missing HP packet `B*(1+0.09*r)*(1+(0.30+0.03*r)*missingHPFraction)` is candidate only, not approved. This packet would hit after its new APPROVED 1.2sec windup; review skill damage AFTER Stealth/Backstab if preferred.
- Envenom candidate immediate Physical `B*(0.80+0.055*r)` plus total Nature poison DoT `B*(0.25+0.03*r)` over current inherited tick behavior, candidate only, not user-signed-off.
- `B` = total raw UNCRITTED NORMAL two-dagger basic sequence attack budget before target mitigation; two impacts are each0.5B with 0.3sec separation, one sequence every2.5sec. Actual normalization B(AGI,dagger stats) relative to global Thief auto formulas remains OPEN. Do not falsely treat proposed skill damage coefficients as approved simply because all Mana/CD values are now approved.
- No poison tick Mana drain or extra basic crit grant is added by the damage review request.

### Skill and progression identity remains unchanged

Four pure actives Stealth25 / Envenom30 / Backstab35 / Execute25, hybrid CriticalMastery20, passive ArmorPen15, total150 purchased ranks / one per Assassin classLv1–150; first unlocks Stealth1 Envenom2 Backstab3 CriticalMastery5 ArmorPen8 Execute12 LAST. The entire exact level-gate schedule was **USER APPROVED Round15** (full table in that section). Pure AGI-only melee dual-dagger physical offense, deliberate two-hit 2.5sec basic sequence with two equal0.5B strikes separated0.3sec, Stealth opening burst, side/rear270deg Backstab bonus stacking with opening, Envenom physical stab+Nature poison, Execute continuous missing HP. Stealth rank-based concealed movement `(r-11)%`, Armor Penetration +1% target PDEF ignored/rank on basic attacks and Physical skills. Critical Mastery passive +3AGI and+1 percentage point basic crit chance/rank, activation grants basic-only charge count2/3/4/5/6 (rank milestones1/6/11/16/20), charges expire death/logout/class switch or refill/replace on recast, only successful basic dagger hits consume charges even if shield-absorbed, misses do not. No extra Fan of Knives, Smoke Bomb, Expose Weakness or separate Guaranteed Crit active.

### Next Round18 skill-by-skill review (NOT user approved yet)

Start with **STEALTH** damage opener and avoid asking approval for all skills at once. Discuss:
1. Proposed Stealth opening damage multiplier `1.10+0.03r` (r1=1.13x, r25=1.85x) vs stronger/weaker rank curves. Its value is determined by STEALTH purchased rank, separate from Backstab ranks.
2. Does Stealth's opener apply to Envenom direct stab and Execute, or exclusively first BASIC sequence and Backstab? Any Nature poison DoT excluded unless user explicitly asks.
3. First-hit basic opener BOTH impacts stay empowered, and Backstab position multiplier still stacks; review whether this multiplication creates expected intended burst.
4. Stealth duration and breaking conditions versus existing TierII parent. Existing movement `(r-11)%`, CD22s and Mana8+2r are LOCKED.
5. After Stealth user approval, move to Backstab direct/positional rank curves, then Execute/Envenom one at a time.

**DOCUMENTS ONLY. No Unity gameplay code/assets/scenes/builds/tests or merges.**


## Assassin Tier III Round 18 — 2026-11-27 STEALTH DAMAGE/ELIGIBILITY/DURATION/MISS APPROVED; ENVENOM WEAPON-COATING ALTERNATIVE OPEN

### Latest AUTHORITATIVE USER approval of STEALTH skill mechanics

User selected:
1. "**Opener multiplier: 1.10+0.05r, rank25 maximum 2.35x**".
2. "**Opener eligibility: Basic sequence and Backstab only**".
3. "**Stealth duration: 6s rank1 to15s rank25**".
4. "**On opener miss: Opening bonus consumed on attempted attack including miss**".

Thus these STEALTH-specific coefficients/rules are NOW USER-APPROVED and supersede the earlier unapproved Round16/Round17 `1.10+0.03r` proposer. Let `r` be the purchased Stealth rank1..25:
- **StealthOpenerMultiplier(r)=1.10+0.05*r**: r1=1.15x, r25=2.35x. Intermediate r values are linear and increase strictly by +0.05 multiplier units per purchased rank.
- Stealth's **duration scales from 6 seconds at rank1 to 15 seconds at rank25**. Exact between-rank interpolation is not expressly user defined; designer implementation proposal `6 + 9*(r-1)/24` seconds without rounding, with endpoints6/15 exactly; a different global duration rank policy would require confirmation, so do not falsely state interior duration formula is user-approved. Applies to this stealth buff's concealed state, not to a per-hit opener buff timer.
- **Eligible OPENING attacks ONLY:** NORMAL two-hit BASIC DAGGER ATTACK SEQUENCE (the first attack cycle from stealth, with BOTH 50% dagger impacts receiving full StealthOpenerMultiplier), OR **Backstab** used as the first stealth-breaking attack; when Backstab is positional (target side/rear270deg), its existing bonus stacks multiplicatively with the approved Stealth opener multiplier. Both skill actions and basics consume ONE Stealth opener, never one per dagger hit. Envenom's initial physical hit, Nature DoT, and Execute are **NOT eligible** for the Stealth opener multiplier. No other skill becomes opener-eligible without user approval.
- **Opening bonus is consumed on ATTACK ATTEMPT even when the attack MISSES**, not reserved until damage lands. This is consistent with prior parent convention. For two-hit first BASIC cycle, only first attempted attack SEQUENCE receives an opener; that sequence's second dagger impact remains entitled to its same opening multiplier EVEN IF the first dagger impact misses. Do not consume another opener on the second hit. If second hit misses, no preservation beyond that first opener attempt. For Backstab attempt missing, opener consumed. Normal Stealth concealment-breaking rules and which other abilities break concealment should still follow verified parent/global mechanics or await explicit design.
- Existing Stealth rank25 signed concealed move modifier `(r-11)%` means -10% at r1 and +14% at r25, intentionally negative at r1 per explicit user approval; **Stealth activation Mana8+2r and cooldown22s remain locked Round17.**
- 2.5second two-hit BASIC attack cycle, second hit0.3s after first and 50/50 normal sequence budget unchanged; AGI-only basic damage scaling unchanged.

### New design question FROM USER: Envenom as weapon-coating buff vs existing active offensive strike

User says "**hmmm, should we remake envenom into a buff instead of an active? like coat your blades in poison and do extra damage and apply dots with attacks? or is it more fun with an active offensive skill?**" This is an explicit request for **design evaluation**, not an approval to change Envenom yet. **EN VENOM'S NEW BEHAVIOR IS NOT LOCKED**. Existing approved prior ENVENOM is a stand-alone physical poison dagger stab with Nature DoT and user-approved `ManaCost=10+2*r`, cooldown8sec, rank cap30, first unlock Lv2 and fully approved 150 class-level gates unchanged. The alternative should **preserve its purchased ranks and gate schedule, Mana and cooldown unless user decides to revise**, and should NOT invent additional resource types.

Compare options:
- **A. Keep active Envenom physical strike + Nature poison DoT:** a direct damage button on CD8s that can be used for burst or continued poison damage. Benefits timing and simple upfront skill impact, but competes with Backstab and Execute as another direct attack.
- **B. Change Envenom into an ACTIVATED TEMPORARY POISON-BLADE COATING BUFF (designer preferred, NOT YET USER APPROVED)**: spending Mana once activates a timed buff on BOTH equipped one-handed daggers. On successful normal basic dagger impacts, add a separate **Nature poison damage packet** plus apply or refresh a **Nature DoT**, with coefficients BOTH increasing with Envenom purchased rank, while normal dagger packets remain Physical and the basic sequence stays2.5sec with0.3sec gap. This creates sustained poison pressure between heavy burst windows and prep/rotation, not a global passive permanent effect. Designer recommendation initially trigger on basic dagger hits to avoid automatic poison multiplication through special attack procs; user can decide whether Backstab and Execute also trigger the coating. DoT stacking vs refresh, max stacks, duration, buff uptime, whether reactivation refreshes/replaces, missed hits, shields, dual hit rules, and damage coefficient are **ALL OPEN**, not auto-approved. The **Round17 approved** cooldown8sec and ManaCost10+2r are not automatically replaced by a designer's proposed higher cooldown if buff selected; need align coat duration, cooldown and uptime intentionally.
- User approved Stealth opener attacks ONLY basics and Backstab; **a poison-coating activation does NOT itself receive the Stealth damage multiplier** and bonus Nature packets/DoT do not gain automatic opener multiplier from associated dagger strikes unless later expressly approved. The original initial poison stab would also not receive the opener bonus under user decision2.
- Alternative **C**: activated poison coating with limited charges rather than fixed duration, if user wants more deliberate window; no user approval.
- **DO NOT automatically change or implement Envenom into a buff**. Ask a clear choice and then tune rules skill-by-skill; user requests the comparison.

### Existing approved class foundations retained

TierIII Assassin separate class from Lv60 TierII Thief; six rank caps Stealth25, Envenom30, Backstab35, Execute25, Critical Mastery20, ArmorPen15 total150, entire level gate schedule USER approved Round15, first unlocks Stealth1 Envenom2 Backstab3 CritMastery5 ArmorPen8 Execute12 LAST. Lv1 VIT22 STR10 AGI35 INT10 SPR5, per-level VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8, BaseMana100, universal MaxMana100+5INT, regen4+0.1SPR. AGI-only Physical main damage, pure melee dual daggers, baseline 2.5sec one 2-hit sequence; positional Backstab from side/rear270deg bigger damage, front90deg standard; Backstab numeric damage/positional bonus still designer draft only, not approved. Execute usable at any HP and growing damage continuously with missingHP, cost20+5r, CD35s, 1.2s windup one heavy dagger strike, numeric DAMAGE coefficient pending. Critical Mastery hybrid passive +3AGI and+1pp BASIC crit/rank, active BASIC guaranteed crit charges2/3/4/5/6 with 25s CD and Mana12+3r, clears charges on death/logout/class swap; ArmorPen 1% target Physical Defense ignored per rank on basics and physical skills. All other existing cooldowns/Mana locked Round17. Stealth multiplier and opener targets are now approved THIS Round18; Backstab/Execute/Envenom damage coefficients remain pending skill-by-skill per Round17 requirement.

### Round19 recommended next questions (NOT user approved)

1. **Choose Envenom shape:** keep instant physical poison strike or turn it into an ACTIVATED coat-both-daggers poison buff, potentially a charge-based alternative. This choice must precede approving exact Envenom damage coefficients.
2. If buff chosen: specify buff duration/rank scaling relative to already locked CD8s and Mana10+2r; whether applies to both BASIC dagger packets and Backstab/Execute; initial Nature packet coefficient and DoT tick/stacking model. Ensure per-hit application and 2.5sec basic cycle not double-counted.
3. Confirm how long stealth duration grows between rank1 and rank25 (linear6+9*(r-1)/24 as proposed vs smooth rounding), if needed for implementation.
4. Then review Backstab raw base `B*(0.90+0.07r)` and side/rear positional multiplier `1.25+0.02r`, with Stealth approved multiplier max2.35x. That combination may give `3.35*1.95*2.35=15.34B` at max ranks without criticals before target defense, a BURST balance implication that user should see before locking coefficients.
5. Review Execute raw missingHP coefficient and Envenom with final design shape. No changes to skill progression and class stats.

**DOCUMENTATION ONLY. No Unity game code/assets/scenes/builds/tests or branch merges.**


## Assassin Tier III Round 19 — 2026-11-27 USER APPROVES ENVENOM AS POISON COATING BUFF, BASICS/BACKSTAB ONLY, SINGLE REFRESHED DOT; DURATION STEALTH+2S OVERRIDES 15–30S

### LATEST AUTHORITATIVE USER DECISIONS AND FREE-TEXT OVERRIDE

User approved:
1. "**Activated temporary poison coating on both daggers**" instead of the former distinct DIRECT physical poison stab. This means Envenom is still a **ranked activatable SKILL with a timed self weapon buff**, not a standalone offensive Envenom attack, and not a permanently on passive. Its approved Rank30 cap and fully approved class level gates stay.
2. "**Normal basics and Backstab, exclude Execute**": while Envenom weapon coating is active, SUCCESSFUL connecting NORMAL BASIC DAGGER impacts (both packet1 and packet2 of the deliberate two-hit basic sequence) and **connecting BACKSTAB strikes** proc coating. **EXECUTE never procs the coating**; don't add Execute poison even though it is a dagger strike. Missed hits cannot proc coating. This is a coat-proc eligibility rule, independent from Stealth opener eligibility: Stealth opener only boosts basics or Backstab, and no Stealth opening damage bonus is automatically granted to the separate Nature poison proc or DoT.
3. "**One DoT refreshed on further hits**": enemy receives ONE active Envenom poison DoT status per Assassin, new successful poison-triggering hits refresh its duration rather than stack extra copies or escalate stack count; no 3-stack/5-stack damage amplification. Whether multi-Assassin applications use separate status ownership vs one shared status still requires combat system-wide stacking convention, do not invent global ownership rules. Even with one refreshed DoT, each successful poisoned hit may deal its own separate bonus Nature damage packet in addition to refreshing the ONE DoT, as intended in user's initial proposed coating idea; exact flat and rank scaling for both packets and DoT ticks still OPEN.
4. Form selection was "**Coating duration: 15s rank1 to30s rank30**" but additional free-text note OVERRIDES IT: "**hmmm i think the applied poson buff should last as long as stealth +2 seconds. giving you time to activate it before going into stealth and not expire before you open.**" Interpret in light of stated reason: this is **DURATION OF THE ACTIVE SELF-COATING BUFF** to prep poison, enter Stealth and open, NOT the enemy's applied damage-over-time poison status duration. **AUTHORITATIVE COATING BUFF DURATION = CURRENT STEALTH CONCEALMENT DURATION + 2 SECONDS.** That means at Stealth rank1, coating lasts8 seconds; at Stealth rank25, coating lasts17 seconds. The user-approved Stealth duration endpoints are6 sec r1 to15 sec r25. This formula uses the user's **Stealth purchased rank**, not Envenom rank. The exact Stealth intermediate-rank interpolation is still not independently user-locked; if designer uses the earlier proposed linear formula `StealthDuration(rS)=6+9*(rS-1)/24` (rS1..25), then `PoisonCoatingDuration(rS)=8+9*(rS-1)/24` seconds, with user-approved endpoints8 and17. Do not erroneously lock15-30sec Envenom-rank duration nor create an enemy DoT of17sec without user approval. Snapshot current Stealth duration at Envenom activation vs dynamically update on Stealth rank changes mid-buff is implementation-detail TBD, usual rank changes don't happen mid-combat.
5. **Preserve previously APPROVED Envenom activation Mana cost `10+2*EnvenomRank`, cooldown8s, Rank30 cap and first classLv2 / remaining exact Round15 rank gates.** Changing Envenom from direct attack to self-coating DOES NOT silently change costs or CD. There is one payment at activation, not per-poisoned hit, per-DoT tick, or per-second drain. Recasting while buff remains active, allowed because cooldown8sec can elapse before max17sec buff ends, needs a stated overwrite/refresh rule; don't silently stack multiple coatings, procs or buff power. Designer default suggest refreshing/replacing one coating, but explicitly PENDING user approval.
6. The user's rationale: permit **coating blades BEFORE entering Stealth**, keep coating active through the Stealth window plus two seconds so an opener can still be made without coating wearing off. This is an intentional prep/setup gameplay flow rather than an unrelated standalone poison DoT timer.

### Round19 authoritative revised skill identity

The Assassin's six purchasable skill concepts remain Stealth25 / Envenom30 / Backstab35 / Execute25 / CriticalMastery20 / ArmorPen15, total150 ranks, first skill unlocks1/2/3/12/5/8 with EXACT 150 rank gate lists approved Round15. Existing former Envenom **initial physical stab action and its previous proposed direct-stab base-damage coefficient are RETIRED** from the TierIII Assassin design and should not accidentally be implemented as an additional attack when player presses Envenom. It is now an ACTIVATABLE WEAPON BUFF THAT ON HITS PROCS NATURE DAMAGE AND A REFRESHABLE NATURE DOT, triggered by BOTH successful BASIC hits and Backstab ONLY, NOT Execute. The other offense rules remain: normal basic physical 2 impacts 0.5B each on2.5s cycle, hit gap0.3s; Stealth opener1.10+0.05r on first attempted basic sequence BOTH packets or Backstab only and consumes on miss; Backstab side/rear270deg bonus, no front bonus. Coating's additional Nature packets and DoT do NOT automatically inherit Stealth direct Physical opener multiplier. This avoids doubling all Nature bonus damage with Stealth unless separately authorized.

### Open Envenom implementation/balance questions, designer-only suggestions NOT approved

- **On-hit BONUS NATURE DAMAGE coefficient** as a function of ENVENOM purchased rank (e.g. normalized to full raw basic sequence B, and consider per-dagger-hit 0.5B raw baseline). Each successful dagger packet may proc a distinct bonus Nature hit; Backstab procs once on its single connecting hit. Must be rank-scaled, affordable at class Lv2 and not overpowered relative to Backstab and Execute. Concrete values NOT USER APPROVED.
- **One ENEMY POISON DoT duration and interval** are still NOT user specified. Important: user specified coating SELF-buff duration Stealth+2, NOT enemy DoT duration. Candidate designer-only DoT lifetime ~4–6sec with regular1sec ticks, refreshed on new proc without stacking. Need decide whether refresh resets tick timer, clips next tick, or respects original next tick; no assumptions are canonically locked.
- **DoT coefficient** scales with Envenom rank, likely distinct from direct Nature-on-hit coefficient, and can be tuned to deliver sustained damage even if no more hits after coating expires (enemy DoT lifecycle and removal on coating expiration TBD).
- **Coating proc on shield-absorbed successful dagger strikes**, and Nature on-hit packet vs the outgoing strike's shield and resistance rules, should follow actual shared hit/proc semantics, not invented pre-approval. Backstab's cast skill crit behavior stays separately undefined; hybrid Critical Mastery guarantees BASIC hit crit ONLY, not Backstab or poison.
- **Reactivation before existing buff expires**: designer recommendation refresh/replace buff to full duration with no stacking, not user approved yet. Since original8s cooldown is shorter than some coating durations up to17sec, overlapping recasts are reachable and require consistent behavior.
- **Stealth transition and invisibility:** user specifically wants to activate coating FIRST, then enter Stealth; pressing coating before Stealth must not prevent later Stealth entry. Whether pressing Envenom WHILE ALREADY STEALTHED breaks invisibility or preserves concealment was NOT user-approved in this round. Avoid inventing Stealth break rules.
- **No additional Mana cost per hit/tick**; activated buff's current rank Mana cost locked.

### Current approved other Assassin state unchanged

- TierIII Lv1 starts VIT22 STR10 AGI35 INT10 SPR5, growth VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8, ClassBaseMana100, MaxMana100+5INT, regen4+0.1SPR. Dual wield one-handed daggers, AGI-only offensive stat, burst opener vs weak sustained normal attack.
- Stealth cap25 6sec r1 ->15sec r25, movement `(StealthRank-11)%`, opener1.10+0.05r for first BASIC sequence or Backstab only, spent on miss, Mana8+2r, CD22sec.
- Backstab35 any-facing, side/rear270deg bonus stacking with Stealth opening, user-approved Mana12+2r, CD9sec, numeric damage coefficients still designer-only. Execute25 always-usable missingHP continuous, physical AGI, 1.2sec windup followed by ONE dagger strike, cost20+5r CD35sec, NO coating proc, numeric damage coeff still designer-only. CriticalMastery20 +3AGI/r and+1pp BASIC crit/r passive, active BASIC-only guaranteed crit charges2/3/4/5/6 by ranks1/6/11/16/20, Mana12+3r CD25sec, charges clear death/logout/class swap, reactivation replenishes instead of stacking. ArmorPen15 passive ignores +1% target Physical Defense per rank on basics and physical skills, Nature coating packet/DoT not naturally Physical.
- Skill rank and class level gate audit from Round15 remains exact/user signed-off, not reopened.

### Round20 recommended questions (NOT USER APPROVED)

1. **Coating bonus Nature ON-HIT packet coefficient** and **SINGLE poison DoT tick damage coefficient**, both ramp with Envenom rank. Offer modest options and illustrations anchored in B (full basic cycle raw budget).
2. **Enemy poison DoT duration** (separate from weapon coating Stealth+2) and whether refresh resets periodic tick timing. Choose e.g.4s or6s, 1s per tick, 1 nonstacking DoT.
3. **Coating recast** on 8s cooldown while coating already active: refresh duration to Stealth+2 vs block until duration expired, with no stacking.
4. **Coating interaction with Stealth**: casting while concealed breaks Stealth or not, noting setup before Stealth approved reason but mid-Stealth entry not decided.
5. After Envenom math, next skill-by-skill Backstab and Execute damage curves, especially positional max burst with Stealth2.35x.

**DOCUMENTATION ONLY. No Unity changes/commits to game source/assets, tests, builds or merges.**


## Assassin Tier III Round 20 — 2026-11-27 USER APPROVES NATURE ON-HIT DAMAGE, 4S ENEMY DOT, ONE COATING REFRESH, STEALTH PRESERVATION; CORRECTS COATING DURATION TO INDEPENDENT ENVENOM RANK

### MOST RECENT USER EXPLICIT CORRECTION SUPERSEDES ROUND19 STEALTH-DEPENDENT COATING FORMULA

User answers:
1. "**Nature on-hit damage: B*(0.08+0.006r), max 0.26B per hit**".
2. "**Enemy poison DoT duration: 4 seconds, ticking once per second, refreshed on hit**".
3. "**Coating recast: Refresh one coating to full duration, no stacking**".
4. "**Envenom activation during Stealth: Preserves concealment and opener**".
Then the user's HIGHER-PRIORITY free text explicitly corrects earlier Round19 interpretation: "**nonono i don't mean poison duration is actually governed by stealth, i meant more as in at the max rank of both skills and at the start rank of both skills it is +2 seconds longer duration on poison**".

**AUTHORITATIVE CHANGE: Envenom WEAPON COATING duration is governed by ITS OWN ENVENOM RANK1..30, NOT Stealth rank.** At **Envenom rank1**, coating lasts **8 seconds** while **Stealth rank1** lasts6 seconds, +2sec. At **Envenom rank30**, coating lasts **17 seconds** while **Stealth rank25** lasts15 seconds, +2sec. The exact intermediate-rank increment/rounding formula not explicitly specified; a designer working recommendation is LINEAR `EnvenomCoatingDuration(rE)=8+9*(rE-1)/29` seconds for Envenom rank rE1..30 and `StealthDuration(rS)=6+9*(rS-1)/24` for Stealth rank rS1..25, but BOTH formulas for interior rank duration are **DESIGNER INTERPOLATIONS, NOT USER-SIGNED-OFF NUMERIC CURVES**; endpoints and independent rank source ARE USER APPROVED. A rank30 coating lasts17seconds whether the player's Stealth is rank1, rank25 or anything else. A rank1 Envenom lasts8seconds even if Stealth is rank25. DO NOT reintroduce `CoatingDuration=CurrentStealthDuration+2`: this formula is **explicitly REJECTED/SUPERSEDED**. The old form 15-30 seconds by Envenom rank was also superseded by endpoint8->17. The user intended Envenom's own duration to exceed corresponding starting and maximum Stealth durations by2 seconds, to permit coating before Stealth, NOT to couple every individual cross-skill rank or current duration to Stealth.

### Approved on-hit NATURE damage formula, new canonical

Let `rE` be the purchased **ENVENOM rank**,1..30, and `B` be the total raw uncritted NORMAL TWO-DAGGER BASIC ATTACK SEQUENCE budget before defenses (two normal Physical impacts0.5B each, 0.3seconds apart, one sequence per2.5seconds), based on AGI and dagger equipment through the still-to-be-finalized shared raw basic formula.

- **On each successful connecting eligible poisoned impact**, apply **ONE separate Nature damage packet `NatureHit(rE)=B*(0.08+0.006*rE)`**, i.e. rank1 **0.086B (8.6%)**, cap rank30 **0.26B (26%)**. Nature packet is in ADDITION to normal Physical damage, not a replacement; damage ignores standard physical-only Armor Penetration passive unless a separate Nature damage rule applies. Neither this on-hit Nature packet nor the poison DoT automatically receives the Stealth opener physical multiplier.
- Eligible triggers remain as previously USER APPROVED Round19: **each SUCCESSFUL NORMAL BASIC dagger impact** (both packet1 and packet2 when they connect) and **successful BACKSTAB hit** proc ONCE per connecting impact. **EXECUTE EXCLUDED**, even though it uses one dagger. Misses do not proc on-hit Nature nor apply/refill enemy poison DoT. The new damage formula is PER eligible hit, NOT per full two-hit sequence: two connecting BASIC hits yield two Nature packets at the selected rank, and each refreshes the same one DoT. Backstab hits once and gives one packet; separate Physical Backstab numeric coefficient still open.
- Poison is Nature supplemental damage, not another Physical weapon strike or an independent auto attack. Critical Mastery BASIC guaranteed crit charges belong to the normal Basic Physical hit only, not an additional charge for Nature packet; whether Nature packets can naturally critically hit needs separate damage-channel rules rather than assumed.

### Approved enemy DOT and recast/invisibility semantics

- Poisoned target has **ONE NONSTACKING Nature damage-over-time status**, enemy DOT lifetime **4 seconds** from most recent eligible connecting coated hit; ticks **ONCE EVERY 1 SECOND**. Reapplying on further successful coated hits **refreshes the SAME DoT to its full4s**, does not create extra stacks, layers, 3-stack/5-stack variants or multiple overlapping dots. **EXACT DAMAGE PER TICK IS NOT YET USER APPROVED**, nor is precise cadence on refresh (reset tick clock vs preserve next scheduled tick) or multi-source stacking; do not manufacture a coefficient.
- If coating recast becomes available again after its USER-APPROVED **8sec cooldown** and is activated while old coating still exists, **REFRESH/REPLACE ONE SELF COATING BUFF to ITS FULL ENVENOM-RANK-BASED DURATION**, not accumulate/stack two coating effects or merge damage coefficients. Cost `10+2*rE` Mana applies once again for each activation, as locked Round17. Old coating ends when replaced.
- **Envenom may be ACTIVATED WHILE ALREADY IN STEALTH WITHOUT breaking concealment, consuming the opener, or triggering an opening attack**; prepping before Stealth is also permitted. Separate Nature poisoning from eligible attacks still uses the already-approved Stealth opener eligibility (the Phys basic/Backstab gets the Stealth opener multiplier, Nature supplement and DoT do not automatically).
- Enemy poison DoT **4s lifetime is SEPARATE** from the SELF weapon-coating lifetime **8s rank1 ->17s rank30**. Expiring coating ends future poison-hit applications, and does NOT inherently imply already applied enemy poison ends immediately; retain separate status duration semantics subject to global effect cleanup.
- Coating is activated buff only, no standalone Envenom Physical stab, no extra per-hit/per-tick Mana payment. Skill cap30, approved class level gate first unlockLv2 and all 150 gates, Mana10+2r, cooldown8s are unchanged.

### Existing approved Assassin class mechanics not altered

- TierIII Assassin class Lv1 attributes VIT22 STR10 AGI35 INT10 SPR5, growth VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8; ClassBaseMana100, universal max100+5INT, regen/s4+0.1SPR. AGI-only Normal Physical/skill scaling, dual one-handed daggers, 2.5sec normal attack cycle with two equal0.5B hits spaced0.3sec. Stealth cap25: duration6sec at r1 to15sec r25, opener multiplier1.10+0.05rS on first attempted two-hit BASIC sequence (both impacts) or Backstab ONLY, even when missed, signed movement `(rS-11)%`, Mana8+2rS and CD22s. Backstab35 eligible from any direction, with side/rear270deg bonus and 9s CD cost12+2r; raw/positional damage coefficient pending. Execute25 always usable damage grows continuously with missing HP, long1.2s pre-strike windup, one dagger strike,35s CD cost20+5r; poison coating EXCLUDED. CriticalMastery20 passive +3AGI and+1pp BASIC crit per rank, active BASIC-only guaranteed crit hit charges up to6 with Mana12+3r and25s CD; ArmorPen15 ignores +1% physical defense per rank on basic/physical skills.
- Entire 150 exact rank minimum class-level eligibility gates user-locked Round15, first unlock StealthLv1, EnvenomLv2, BackstabLv3, CritMasteryLv5, ArmorPenLv8, ExecuteLv12 LAST. No progression changes.
- Round17 user explicitly requests damage review skill-by-skill; no Backstab or Execute RAW DAMAGE formula was just approved by Envenom discussion.

### Pending Round21 questions and balance (UNAPPROVED)

1. **ENEMY NATURE DoT TICK DAMAGE FORMULA**, rank-scaled and meaningful alongside already locked direct on-hit Nature packet `B*(0.08+0.006rE)`; propose e.g. `B*(0.015+0.002rE)` PER tick (r1=1.7%B/tick, cap30=7.5%B/tick, 4 ticks=30%B if not refreshed) or alternative tuned outcomes; no user approval until answered. The player might land two coated dagger contacts per2.5sec, so check effective sustain and poison DoT refresh uptime.
2. **DOT refresh timing:** restarting 1sec tick timer on each refreshed proc could repeatedly delay ticks for frequent dual hits0.3sec apart; consider retaining existing tick cadence while refreshing expiry and no instant extra tick, to prevent lost or duplicate ticks; ask for user decision.
3. **COATING DURATION INTERPOLATION** between user-approved r1=8sec and r30=17sec, e.g. linearly based on purchased Envenom rank, not on Stealth rank, optional rounding/display.
4. **Other skills damage review:** Backstab `B*(0.90+0.07rB)` base and side/rear multiplier `1.25+0.02rB` are designer proposals; with max Stealth opener2.35x resulting max eligible positional Backstab ~15.35B raw pre-defense, so validate burst balance before locking. Execute `B*(1+0.09rX)*(1+(0.30+0.03rX)*missingHPFraction)` also not approved.
5. **Effects while shield-absorbed and multi-source DoT ownership** per actual engine/global policy, not assumed.

**DOCUMENTATION ONLY. No Unity game source/assets/scenes edits, builds/tests or merges. Commit design decisions to GitHub each round.**


## Assassin Tier III Round 21 — 2026-11-27 USER APPROVES 0.5S ENVENOM DOT TICKS WITH HALF PER-TICK DAMAGE, KEEP TICK PHASE ON REFRESH, LINEAR COATING DURATION

### LATEST AUTHORITATIVE USER ROUND21, FREE-TEXT OVERRIDES THE SELECTED 1S TICK INTERVAL

User initially selects:
1. "**Enemy poison DoT damage per 1s tick: B*(0.015+0.002r), cap 0.075B/tick**".
2. "**Poison DoT refresh: Refresh 4s duration, preserve scheduled 1s tick cadence**".
3. "**Envenom coating rank duration: Linear 8+9*(r-1)/29 seconds, Envenom ranks 1 to 30**".
Then user explicitly overrides the interval/damage PER TICK: "**i think maybye do a poison tick every 0.5 seconds instead and half the damage per tick?**" Interpret this as a clear intended change to DOUBLE frequency and HALF damage per individual tick, retaining same nominal total damage across four seconds. The precise final USER-APPROVED mechanics are:

- **Enemy Nature poison DoT tick interval = EVERY 0.5 SECONDS**, NOT one second. This supersedes Round20 and Round21 selection of 1s ticks.
- **Per half-second Nature DoT tick `B*(0.0075+0.001*rE)`**, half the selected prior one-second tick formula `B*(0.015+0.002*rE)`. Here `rE=EnvenomPurchasedRank`,1..30, `B=full raw uncritted NORMAL TWO-DAGGER BASIC ATTACK SEQUENCE DAMAGE` before target defenses, not one half-hit's raw damage.
  - Envenom rank1: `0.0085B` PER 0.5s tick (0.85%B), 8 uninterrupted ticks total`0.068B` over four seconds (6.8%B).
  - Envenom rank30: `0.0375B` PER 0.5s tick (3.75%B), 8 uninterrupted ticks total`0.30B` over four seconds (30%B).
  - This preserves the earlier intended equivalent damage per second and four-second uninterrupted DoT total, while tick cadence doubles. **No secondary hidden on-tick Mana cost**. Tick damage scales with Envenom rank via +0.001B PER 0.5s tick/rank.
- **ENEMY poison DoT LIFETIME remains 4.0 SECONDS** after most recent successful eligible coated attack, independent of coating self-buff duration. One NONSTACKING DoT per target; later valid poisoned hits refresh the expiry back to four seconds, but **DO NOT reset, delay or advance the existing 0.5 SECOND TICK PHASE**, nor create an instant extra tick. Treat refresh as expiry extension without scheduling duplicate/new initial tick. Under uninterrupted4sec dot with no further applications and a tick at each elapsed0.5sec including expiry boundary, eight scheduled ticks occur; boundary/tick ordering in engine implementation should be consistent and avoid duplicate ticks. Frequent 0.3sec-spaced dagger hits should not indefinitely starve ticks.
- **SELF coating duration exact rank interpolation USER APPROVED: `CoatingDuration(rE)=8+9*(rE-1)/29 SECONDS`**, for own Envenom purchased rank1..30. r1=8s, r30=17s; intermediate ranks use the exact linear formula. This remains INDEPENDENT of Stealth rank/duration, superseding the expressly rejected Round19 `StealthDuration+2s` linkage. The endpoints are intentionally 2sec longer than Stealth's corresponding endpoints r1=6 and r25=15, but differing rank progressions do not dynamically couple.
- Existing activated Envenom rank30 ManaCost`10+2rE`, CD8sec, first unlockLv2 remain unchanged. Recasting while active refreshes ONE coating to full Envenom-rank-based duration with no buffs stacking; can cast during Stealth without breaking concealment or consuming opener. Each SUCCESSFUL normal basic dagger packet and successful Backstab hit applies per-hit Nature bonus `B*(0.08+0.006rE)`, r1=.086B, cap30=.26B, AND refreshes same one enemy poison DoT. Execute NEVER procs coating. Coating application is not a direct poison stab; former direct-stab design retired. Nature on-hit bonus packet is ADDITIONAL to ordinary AGI-only Physical attack and Nature bonus/DoT do not automatically inherit Stealth opener multiplier. Each dual basic cycle has two 0.5B normal hits separated0.3sec within2.5sec overall cycle; if both hit they each proc once and refresh existing one dot.

### Clarification and balance analysis, not additional user approvals

- 0.5-second ticks produce the SAME total intended DPS as the user's first selected once-per-second DoT by halving each tick. Full initial4sec poison if not refreshed: `8 * B*(.0075+.001rE) = B*(.06+.008rE)`, equal to `4*B*(.015+.002rE)` originally selected. Continuous poison reapplication can keep a single four-second DoT active, but each subsequent hit adds its own extra Nature on-hit damage (not extra DoT stacks). At cap30, two successful normal basic impacts deliver **2 x0.26B =0.52B extra Nature on hit** per full 2.5s basic sequence; one poison DoT may deliver0.30B over4s without further procs. Real sustained DPS and stacking with other player effects must be checked after raw B and armor/element/resistance mechanics are defined.
- Keep existing tick phase across expiry refresh but do not keep expired DoTs on targets. If effect expires and a later attack reapplies, it's a fresh DoT schedule; user only approved preserving schedule on an ACTIVE DoT REFRESH. Exact multi-source poison ownership, damage snapshot vs dynamic AGI changes, shield absorption effects on triggers and elemental resist interactions remain open. Avoid inventing crit ability for Nature packets.
- Stealth's own intermediate-rank duration linear interpolation6+9*(rS-1)/24 remains a designer recommendation unless separately approved; this round approves Envenom linear duration ONLY.

### All other class parameters remain user-approved and unchanged

- TierIII Assassin separate Lv1 after eligible Thief TierII Lv60; user-approved EXACT 150 skill rank level gates first unlock Stealth1 Envenom2 Backstab3 CritMastery5 ArmorPen8 Execute12 LAST; approved caps25/30/35/20/15/25. Lv1 VIT22 STR10 AGI35 INT10 SPR5, growth +1.7 VIT,+0.7STR,+7AGI,+0.8INT,+0.8SPR, global MaxMana100+5INT, ManaRegen4+0.1SPR, class BaseMana100. AGI-only physical scaling.
- Stealth cap25 duration6s r1..15s r25, move `(rS-11)%`, opener1.10+0.05rS on FIRST attempted normal basic two-packet sequence or Backstab only, consumed on MISS, not Envenom/Execute, Mana8+2rS/CD22sec. Backstab35 any-facing side/rear270deg physical bonus and opening multiplier stacking, Mana12+2rB/CD9sec, RAW damage and positional coefficients pending. Execute25 always usable and increasingly damaging with missing enemy HP, 1.2s windup then single dagger strike, Mana20+5rX/CD35sec, no Envenom proc, numeric damage pending. CritMastery20 hybrid passive +3AGI and+1pp basic crit/r, active guaranteed BASIC crit hit charges2/3/4/5/6 by milestones1/6/11/16/20, Mana12+3rC/CD25sec, charge lifecycle user approved; ArmorPen15 ignores +1% enemy PDEF per rank on basics/physical skills.
- No Unity changes authorized by design interview. Damage reviews remain skill by skill, with Envenom on-hit and periodic damage now approved; next review is Backstab base and positional coefficients (prior DESIGNER draft `B*(0.9+0.07rB)`, side/rear multiplier `1.25+0.02rB`). At maximum, stacking with max Stealth2.35x produces ~15.35B before mitigations under that draft; warning useful for balancing, not locked numbers. Later Execute damage coefficient to review separately.

### Round22 suggested follow-up questions, NOT USER APPROVED

1. **Backstab RAW PHYSICAL direct packet coefficient** per purchased Backstab rank, with meaningful stand-alone impact relative to full basic B.
2. **Backstab SIDE/REAR positional multiplier** r1..35 (front90deg no bonus), balancing multiplicative max Stealth opener2.35x so peak burst is deliberate but not inadvertently overpowered; compare draft15.35B total and a toned-down alternative.
3. **Backstab hit/position sampling**: at button press vs impact; if target rotates mid-action, what facing counts? Not answered yet.
4. **Stealth current duration interpolation** between6s/15s (independent of Envenom, which is NOW locked linear).
5. Later **Execute damage** missing HP curve, interrupted 1.2sec windup/refunds, etc. Do not ask to re-decide already-locked Envenom 0.5s tick damage.

**DOCUMENTATION ONLY: no Unity gameplay code/assets/scenes/builds/tests or branch merges.**


## Assassin Tier III Round 22 — 2026-11-27 USER APPROVES BACKSTAB BASE DAMAGE, CONSERVATIVE POSITIONAL MULTIPLIER, TARGET FACING SAMPLED AT HIT IMPACT

### AUTHORITATIVE LATEST USER APPROVALS

User provided:
1. "**Base Physical damage: B*(0.90+0.07r), max3.35B**".
2. "**Side/rear positional multiplier: 1.15+0.01r, max1.50x**".
3. "**Target facing evaluation: At hit impact**".
4. "**Computed max-rank side/rear raw Backstab with max Stealth: 11.809B**".
These are EXPLICITLY USER APPROVED, not mere designer recommendations. They SUPERSEDE Round16 draft positional multiplier `1.25+0.02r` (max1.95x), while APPROVING its proposed base direct physical coefficient `B*(0.90+0.07r)`. The coefficient `B` remains the RAW uncritted total ONE full two-dagger BASIC SEQUENCE damage before target mitigation, comprising two basic packets of0.5B at0.3s separation in an overall2.5s cycle. Actual B(AGI,dagger stats) normalization versus TierII Thief and shared engine damage conventions still pending.

Let `rB` be Backstab's OWN purchased rank1..35:
- **BackstabBasePhysicalDamage(rB)=B*(0.90+0.07*rB)**. r1=0.97B, r35=3.35B, each rank adds+0.07B. Single BACKSTAB physical skill hit, not two normal basic packets.
- Facing measured relative to target's FRONT-90-DEGREE arc **AT THE ACTUAL DAMAGE HIT IMPACT**. Within frontal90deg, **positional multiplier=1.0** and Backstab is STILL usable; within enemy relative SIDE+REAR combined270deg, multiply base physical damage by **`BackstabPositional(rB)=1.15+0.01*rB`**, r1=1.16x, cap35=1.50x. This supersedes earlier unapproved max1.95x alternative. No extra distinct rear-only multiplier; entire side+rear270deg shares identical bonus. The attacker/target relative positions at impact, not button press, determine eligibility; if target rotates or attacker moves before strike lands, evaluate at real impact.
- **Approved existing Stealth opener** when Backstab is FIRST qualifying attack out of concealment multiplies its Physical Backstab damage by `1.10+0.05*rS`, where `rS` is STEALTH purchased rank1..25, max2.35x. Side/rear multiplier and Stealth opener stack MULTIPLICATIVELY; the Stealth opening bonus is consumed on the attempted Backstab EVEN IF MISS, per Round18. If Backstab hits from front, it still receives Stealth opener but NOT positional. Nothing here grants auto-crit to Backstab from BASIC-only Critical Mastery guarantee, and Backstab has one attack/one Envenom proc when coated and connects.
- Rank35 side/rear without Stealth: `3.35B*1.50=5.025B` before mitigation. Rank35 side/rear with max-rank Stealth: `3.35*1.50*2.35=11.80875B`, custom user-supplied rounded **11.809B**. Rank1 side/rear without Stealth `0.97*1.16=1.1252B`; with max-rank Stealth `1.1252*2.35=2.64422B`. These are raw potential outputs, not playtested damage guarantees. If comparing Backstab rank1 with Stealth rank1 instead, use Stealth1.15x not2.35x. Envenom bonus Nature proc `B*(0.08+0.006*rE)` on successful coated Backstab is IN ADDITION and not included in these Backstab Physical products, never automatically amplified by Stealth physical opener.
- **Approved Mana cost `12+2*rB`**, rank1=14, cap35=82 Mana, cooldown **9 seconds**. No additional ability, power-up resource, positional condition that prevents cast, or new cooldown was approved. Whether physical skill hits can naturally crit under existing combat rules remains governed by actual shared engine/other skill conventions, NOT by Critical Mastery's guaranteed BASIC-only charges.
- User approves check AT HIT IMPACT, but does NOT specify Backstab ability animation/windup duration or cancellation/refund, nor boss turning/lag compensation mechanics. Preserve actual global rules, do not invent them. The user-approved Execute 1.2s windup is a DIFFERENT skill and should not be copied to Backstab by default.

### All previously approved rules retained

- TierIII Assassin Lv1 VIT22 STR10 AGI35 INT10 SPR5, per-level VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8, ClassBaseMana100; shared MaxMana100+5INT / regen4+0.1SPR. AGI-only attack/physical skill stat scaling; dual one-handed daggers, normal basic 2.5s cycle with 0.3s between exactly two equal50/50 physical hits.
- Stealth rank25, concealed duration6sec at rank1 ->15sec rank25 (intermediate interpolation still open), concealed movement `(rS-11)%`, first attempt opening multiplier `1.10+0.05rS` on ONE two-dagger basic sequence (both impacts) or Backstab only; consumed even on miss, Mana8+2rS/CD22sec.
- Envenom now an ACTIVATABLE weapon coating buff on BOTH daggers (former standalone physical poison stab RETIRED). Self coating duration user-approved `8+9*(rE-1)/29` seconds, rank1=8s, rank30=17s, governed by OWN Envenom rank independently of Stealth rank; Mana10+2rE/CD8sec. Successful basic hit per packet and successful Backstab hit (NOT Execute) each deal bonus Nature `B*(0.08+0.006*rE)` and apply/refresh ONE nonstacking enemy Nature DoT,4sec lifetime, **ticks every0.5sec** for `B*(0.0075+0.001*rE)` per tick (cap0.0375B/tick), refresh expiration without resetting half-second tick clock. Recasting coating refreshes one buff without stacking; activating while concealed preserves Stealth and opener. Poison Nature packets/DoT don't automatically inherit the direct Physical Stealth multiplier.
- Execute rank25 ALWAYS usable on any enemy HP, raw physical damage increases CONTINUOUSLY with missing target HP and skill rank, 1.2s windup then one dagger strike, Mana20+5rX/CD35sec, no Envenom coating proc, numeric DAMAGE coefficient still **DESIGNER PROPOSAL NOT USER APPROVED**. Prior candidate `B*(1+0.09rX)*(1+(0.30+0.03rX)*missingHPFraction)`.
- Critical Mastery cap20 hybrid passive +3AGI/r, +1 percentage point BASIC crit chance/r, active BASIC-only guaranteed crit charges2/3/4/5/6 by rank breakpoints1/6/11/16/20, Mana12+3rC, CD25sec immediate on use, charge lifecycle rules as prior rounds. ArmorPen cap15 +1% target PDEF bypass/rank on basic and Physical skill direct damage, not Nature poison.
- All six skill caps and EXACT 150 class-level rank eligibility gates fully USER APPROVED Round15, first unlock Stealth1 Envenom2 Backstab3 CriticalMastery5 ArmorPen8 Execute12 LAST. No gate changes.

### Round23 NEXT skill-by-skill design recommendation (NONE yet USER APPROVED)

Move to EXECUTE ONLY, whose **numerical damage coefficient still OPEN**. Proposed candidate:
`DamageExecute=B*(1+0.09*rX)*(1+(0.30+0.03*rX)*missingHPFraction)`, `missingHPFraction=clamp(1-targetHP/maxHP,0,1)`; rank1 at full target HP1.09B; cap25 at full target HP3.25B; cap25 missing50% approx4.95625B; cap25 missing100% approx6.6625B; missingHP multiplier scales at each rank and is CONTINUOUS, no threshold. Contrast stronger high-commitment options against approved huge Backstab+Stealth side/rear max11.80875B; Execute's 35s cooldown, rank-scaled Mana up to145, 1.2s windup and no Stealth opener/Envenom proc imply its risk should be reflected in expected damage. Offer balance choices for Execute base coefficient and missing-HP coefficient SEPARATELY, plus windup interruption/cost/refund timing if desired. Do NOT silently claim old Execute formula user-approved or change user's locked numeric costs/CD. No extra Execute crit guarantee from Critical Mastery.
Possible additional question: confirm linear Stealth interpolation `6+9*(rS-1)/24`, but only if needed after Execute damage.

**DOCUMENTATION ONLY. No Unity game source/assets/scenes/build/tests or merges.**


## Assassin Tier III Round 23 — 2026-11-27 USER APPROVES EXECUTE HEAVY BASE+MISSING HP DAMAGE, NORMAL CHANNEL INTERRUPTION/CANCEL REFUND; REQUESTS EVEN STRONGER FINISHER (AMBIGUOUS BACKSTAB WORDING)

### LATEST AUTHORITATIVE USER ANSWERS

User selects the following exact Execute damage formulas and cast-interruption rule:
1. "**Base damage: B*(1+0.12r), max4.00B**".
2. "**Missing HP bonus: 1+(0.40+0.07r)*missingHPFraction**".
3. "**Interrupted 1.2s windup: Cancel, refund Mana, no cooldown**".
4. Free text: "**normal channeling rules apply. i think maybe make backstab a little stronger (especially the missing hp bonus), high mana cost and high cooldown will limit the use of this skill any way.**"

**IMPORTANT TERM AMBIGUITY:** user writes "backstab" in note, but Backstab has NO missing-HP multiplier and has only Mana12+2r/9sec cooldown. Execute is specifically the high Mana20+5r, long35sec cooldown skill and is the only skill with missing-HP scaling, which user just approved in options1/2. Interpret note's context as referring to **EXECUTE's strength and missing-HP bonus**, NOT an unrequested quantitative Backstab adjustment. Preserve previous Round22 Backstab raw and positional coefficients unchanged. The user's note requests consideration of further strengthening the missing-HP finisher, but contains NO exact additional numeric coefficients; thus **the exact numerical FORM selections above are approved canonical values**, and potential additional increase is **OPEN FOR USER APPROVAL**, not auto-applied. If user later clarifies they literally meant Backstab, then revisit with user.

### USER-APPROVED EXACT EXECUTE RAW DAMAGE

Let `rX=Execute purchased rank1..25`, `B`=RAW uncritted TOTAL one normal TWO DAGGER basic attack sequence damage before target mitigation/crit, `m=clamp((TargetMaxHP-TargetCurrentHP)/TargetMaxHP,0,1)` target MISSING HP fraction at the successful impact (precise snapshot timing should follow actual engine-standard hit resolution; don't automatically invent custom snapshot semantics). User-approved formula:

`ExecuteRawPhysicalDamage(rX,m)=B*(1+0.12*rX)*(1+(0.40+0.07*rX)*m)`.

This SUPERSEDES earlier designer-only `B*(1+0.09rX)*(1+(0.30+0.03rX)*m)`. Both **the base raw coefficient and the missing HP bonus coefficient scale with own Execute rank** and target missing HP increases multiplier CONTINUOUSLY, no threshold and no execute-only cast lockout at healthy target HP.

| Target HP left | Missing HP fraction m | Execute r1 base1.12B and missingHP coeff0.47 | Execute r25 base4.00B and missingHP coeff2.15 |
| --- | ---: | ---: | ---: |
| 100% | 0 | 1.1200B | 4.0000B |
| 75% | .25 | 1.2516B | 6.1500B |
| 50% | .5 | 1.3832B | 8.3000B |
| 25% | .75 | 1.5148B | 10.4500B |
| Near 0% | 1 | 1.6464B | **12.6000B** |

The raw high-end Execute12.6B at rank25 vs maximally positional Backstab with max Stealth11.80875B is an observation for balancing, **NOT a user request to nerf either**. The 35s cooldown, one1.2s windup and Mana cost20+5r (up to145) are user-approved and deliberately make Execute a potent, high-commitment long-reuse finisher. Execute still is one Physical dagger skill impact after windup, AGI-only scaling through B and standard physical defenses, **does NOT receive Stealth opener** and **DOES NOT proc Envenom poison coating**, nor consume BASIC-only Critical Mastery guaranteed hit charges. These distinctions are locked earlier.

### USER-APPROVED INTERRUPTED EXECUTE WINDUP

- Execute winds up for **1.2 seconds** under **NORMAL GLOBAL CHANNELING / CAST INTERRUPTION RULES**, as user explicitly instructs, then performs SINGLE heavy dagger hit on successful completion (Round17). Do **NOT** create special immunity to normal hard CC or an invented custom channel system.
- On interrupted 1.2sec channel/windup: **NO DAMAGE**, cancel the strike, **refund any Execute Mana charged** and **DO NOT start/consume the35sec cooldown**. This overrides any earlier unresolved note about refund/no-CD; now approved. Follow existing engine standard behavior for movement, target validity, damage interruption etc where applicable, not invent additional exceptions. On SUCCESSFUL channel/strike use locked Execute ManaCost`20+5rX` and 35sec cooldown; exact internal debit/reserve timing can match global channeling policy so long as interrupted cast refund/no cooldown is honored. Normal/channel rule precedence should be consistent with other channeled skills, not Assassin-specific bespoke handling.
- Whether channel finishing but attack misses gets refund (unlikely), LoS/path movement/cast bar display and effects at collision should rely on verified global semantics rather than silently assume.

### OPEN USER DESIGN REQUEST: May strengthen Execute further despite approved high coefficients

Since user says "maybe make backstab a little stronger (especially the missing hp bonus), high mana cost and high cooldown will limit the use of this skill anyway", log the probable intended referent as **EXECUTE** (only missing-HP and highCD/cost ability). Both the selected rank-scaling formula and comparative balance are fixed for now; designer may offer optional **additional missingHP emphasis** for next round, e.g. preserve base `B*(1+0.12rX)` and change ONLY missingHP multiplier to `1+(0.40+0.09rX)*m` (r25 extra2.65m, near death max14.6B rather than12.6B, r1 multiplier 0.49m); this is **DESIGNER PROPOSAL, NOT USER-APPROVED**. Could also ask whether note meant literal Backstab, but explicit user context strongly points to Execute, so don't automatically alter Backstab. If user wants more finisher punch, offer targeted increase options rather than redoing all skill coefficients. Do NOT label provisional extra curve as current approved.

### Rest of assassin confirmed unchanged

- Round22 USER-APPROVED Backstab rB1..35 RAW `B*(0.90+0.07rB)`, side/rear multiplier `1.15+0.01rB` (front90deg no bonus, still castable), facing sampled AT ACTUAL IMPACT, and opening Stealth `1.10+0.05rS` stacks multiplicatively on qualifying Backstab, max with position and Stealth11.80875B; 9sec cooldown, Mana12+2rB. No missingHP bonus on Backstab.
- Stealth rS1..25 opener `1.10+0.05rS`, first attempted BASIC two-hit sequence BOTH impacts or Backstab only, spent even when missing, duration endpoint6sec r1 ->15sec r25, concealed move `(rS-11)%`, Mana8+2rS/CD22sec; exact Stealth intermediate duration interpolation still unspecified.
- Envenom as ON-BOTH-DAGGERS timed activated coating, independent duration by own rank `8+9*(rE-1)/29` seconds rE1..30, Mana10+2rE/CD8sec, one coating refreshed on recast, preserves concealment and opener if activated while Stealthed. On successful basic dagger hit EACH packet or Backstab (NOT Execute), bonus Nature `B*(0.08+0.006rE)` PER hit, and one nonstacking Nature target DoT lasting4sec refreshed on subsequent hits WITHOUT resetting tick phase; ticks every0.5sec `B*(0.0075+0.001rE)`,8 ticks per uninterrupted4s. Poison Nature bonus/DoT not boosted by physical Stealth opener.
- Assassin has pure dual one-handed dagger AGI-only melee DPS, basic attack sequence2.5sec two hits0.5B each with0.3sec separation, starting stats VIT22 STR10 AGI35 INT10 SPR5, growth VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8, class BaseMana100, global MaxMana100+5INT and regen/s4+0.1SPR. CriticalMastery20 passive+3AGI/r and+1pp basic crit/r, active BASIC-only guarantee charges2/3/4/5/6 at r1/6/11/16/20 with Mana12+3r/CD25sec; ArmorPen15 bypass+1% target PDEF/rank on basics and Physical skills. All six exact rank caps and 150 level eligibility rank gates approved Round15, initial unlock Stealth1 Envenom2 Backstab3 CritMastery5 ArmorPen8 Execute12 LAST.
- No code changes implied from design-only conversation.

### Round24 next design questions, NOT YET USER APPROVED

1. With high-CD Execute already stronger (12.6B at near-death) ask **keep SELECTED curve 12.6B or strengthen missing HP specifically** e.g. near-death14.6B with `0.40+0.09rX` at cap, or even more user custom. Explicitly state "I'm interpreting Backstab in your note as Execute due to missing HP/high CD" and allow clarification.
2. Execute channel rules now approved: 1.2sec global standard; interrupted refund/noCD. No re-approval needed.
3. If no further Execute change, confirm Stealth intermediate duration interpolation `6+9*(rS-1)/24` and tune raw B coefficient versus TierII Thief/global class normal attacks; prioritize completing class baseline and movement/crit rules.
4. Confirm Backstab animation timing only if needed, avoid undoing Round22 approved coefficients.

**DOCS ONLY; no Unity gameplay code/assets/scenes/builds/tests/merges.**


## Assassin Tier III Round 24 — 2026-11-27 USER CONFIRMS THEY MEANT EXECUTE AND APPROVES STRONGER CONTINUOUS MISSING-HP MULTIPLIER

### Newest, AUTHORITATIVE USER APPROVAL and disambiguation

User says "**Execute missing HP damage decision: Increase to 1+(0.40+0.09r)*missingHPFraction, max14.6B near death**", explicitly confirms "**Base damage stays B*(1+0.12r), max4B at full HP. Existing 35s cooldown, mana20+5r and 1.2s normal channel unchanged**", and clarifies "**yes, i did mean execute, sorry about that**". Therefore Round23 ambiguous reference to "backstab ... especially the missing hp bonus" definitively meant **EXECUTE**, NOT Backstab. No new Backstab damage or HP-scaling mechanics; preserve existing Round22 Backstab.

### FINAL USER-APPROVED Execute raw Physical damage coefficient

Let `rX` be purchased Execute rank1..25, `B` be the total RAW uncritted ONE full NORMAL two-dagger basic sequence damage before target mitigation (two separately resolved0.5B normal impacts), and `m=clamp((targetMaxHP-targetCurrentHP)/targetMaxHP,0,1)` be missing target HP fraction. **NEW CURRENT CANONICAL** Execute Physical hit raw:

`ExecuteRawPhysicalDamage(rX,m)=B*(1+0.12*rX)*(1+(0.40+0.09*rX)*m)`

SUPERSEDES the just-approved Round23 missingHP term `1+(0.40+0.07*rX)*m`. It preserves identical base `B*(1+0.12rX)` at full target HP and increases only the rank-scaled missingHP benefit. The user's note confirms exact change, rather than merely asking for an optional future suggestion.

| Target current HP | Missing fraction m | r1 base1.12B, missing coeff0.49 | r25 base4B, missing coeff2.65 |
| --- | ---: | ---: | ---: |
| 100% | 0 | 1.1200B | **4.0000B** |
| 75% | .25 | 1.2572B | **6.6500B** |
| 50% | .5 | 1.3944B | **9.3000B** |
| 25% | .75 | 1.5316B | **11.9500B** |
| Approaches 0% | 1 | 1.6688B | **14.6000B** |

Target remaining HP gate remains NONE: usable at any target HP with a smooth continuous missing HP scaling. This stays an AGI-only PHYSICAL skill, ONE heavy dagger impact after an approved **1.2-second normal global channel**, paid once Mana`20+5*rX` (r1=25, r25=145) and **35sec cooldown** on successful completion according to normal channel policy. Interruption within1.2sec cancels strike, refunds consumed Mana, and does NOT start/consume cooldown (USER approved Round23). **Execute NOT eligible for Stealth opening damage multiplier nor Envenom coating poison proc; Critical Mastery guaranteed crit charges affect BASIC dagger hits only and NOT Execute.**

### Important comparison, no new unapproved nerf/buff

At max ranks, Execute at nearly dead **14.6B raw** vs Backstab r35 side/rear first attack out of maximum Stealth **3.35*1.50*2.35=11.80875B raw**, before defenses/critical differences; these are not equivalent situations. Execute has no positional/Stealth multiplier, longer35sec cooldown and expensive145Mana, and the 1.2sec interruptible channel. Backstab remains a 9sec cooldown positional strike with raw `B*(0.90+0.07*rB)` (max3.35B), side/rear multiplier `1.15+0.01*rB` (max1.5x), target facing checked **at hit impact**, position stacks Stealth opener. It has NO missing target HP bonus. Do not alter Backstab because of ambiguous Round23 note; user resolved it Round24.

### All prior user approvals remain unchanged

- Lv1 Assassin VIT22 STR10 AGI35 INT10 SPR5, per-level VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8, class BaseMana100, MaxMana100+5INT, regen/s4+0.1SPR. Melee dual one-handed daggers, AGI-only offense, normal 2-hit attack sequence2.5s,0.3s between impacts,0.5B each.
- Stealth rS1..25, duration6s rank1 to15s rank25, between-rank interpolation still not signed off; concealed move modifier `(rS-11)%`, first qualifying attempted basic TWO-HIT sequence or Backstab only opener damage `1.10+0.05*rS`, both dagger packets eligible, attempt consumes even on miss, Mana8+2rS,22sec cooldown. Excludes Execute and Nature poison procs/DoT.
- Envenom ACTIVATABLE poison-coated BOTH blades (not direct stab) for `8+9*(rE-1)/29` seconds based on Envenom own rank1..30, reactivation replaces/refreshes coating, may cast in Stealth without breaking opener, Mana10+2rE/CD8sec. Each successful ordinary basic dagger impact OR Backstab (NEVER Execute) adds Nature `B*(0.08+0.006*rE)` and refreshes ONE nonstacking enemy poison DoT4sec; ticking0.5sec `B*(0.0075+0.001*rE)` Nature per tick, preserving original tick phase on refresh.
- Critical Mastery20 hybrid passive+3AGI/r and+1 percentage point BASIC crit/r, active BASIC-only guaranteed crit charges2/3/4/5/6 at milestone ranks1/6/11/16/20, Mana12+3r/CD25sec; successful basic hit consumes including fully shield absorbed, misses don't, cooldown begins immediately, reactivation replaces, charges clear on death/logout/class change. ArmorPen15 ignores+1% target PDEF/rank on basics and Physical skills, not poison Nature.
- EXACT approved150 individual class level/rank eligibility gates from Round15, first unlocks StealthLv1 EnvenomLv2 BackstabLv3 CritMasteryLv5 ArmorPenLv8 ExecuteLv12 LAST. Four actives, one hybrid, one pure passive, caps25/30/35/25/20/15=150.
- No Unity gameplay code/scene/asset changes authorized.

### Suggested Round25 follow-ups, DESIGNER PROPOSALS NOT USER APPROVED

1. **Stealth duration INTERMEDIATE RANK FORMULA:** approve/revise linear `StealthDuration(rS)=6+9*(rS-1)/24` seconds, preserving endpoints6sec rank1 and15sec rank25; independent from the user-locked Envenom own-rank linear `8+9*(rE-1)/29`.
2. **Basic raw `B` coefficient / NORMAL attack damage**: compare exact global physical base attack formula and TierII Thief's base damage and attack interval in source before proposing specific AGI coefficients. User approved 2.5sec two-hit cycle, 50/50 physical hits separated0.3sec, but NOT numerical B function. Evaluate slow sustain vs high skill burst, no independent per-dagger timers. If needed inspect Unity source or class design docs without changing active Unity checkout; keep work in isolated docs branch.
3. **Backstab actual animation/contact windup timing**: target facing at IMPACT approved, animation duration open; can later choose amount.
4. **Remaining Execute channel edge cases** should FOLLOW standard global channel rules, not bespoke Assassin-only deviations. No need to revisit user-locked coefficients/Mana/cooldowns.
5. Mentor/class advancement and planned world placement when combat kit fully resolved.

**DOCUMENTATION ONLY. No Unity gameplay code/assets/scenes/builds/tests or branch merges.**

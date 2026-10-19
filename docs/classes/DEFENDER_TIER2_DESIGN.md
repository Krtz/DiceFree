# DiceFree — Tier II Defender — Design interview

**Created:** 2026-10-19  
**Status:** DESIGN/DOCUMENTATION ONLY — Tier II Defender not implemented. User will instruct the separate working chat to read current documents and implement after a functioning remote connection returns. Do not edit Unity gameplay C#, character saves or builds during these interviews.  
**Branch:** `integration/world1-foundation`  
**References:** [Tier II roster v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [original five skill concepts](../TIER2_CORE_CLASSES_DESIGN_V01.md), [class progression](../CLASS_PROGRESSION.md), [stats and shields](../STATS_AND_DAMAGE.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Greatblade/Mage rank audit](../TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md).

## Confirmed Defender decisions — updated through Round 3 (2026-10-19)

| Area | Confirmed by user | Still awaiting specific design |
| --- | --- | --- |
| Class | **Defender** is the right **Tier II** baseline tank name. **Guardian**, **Vanguard** and similarly stronger/more-specialized names make sense for **future advancement alternatives**, not current class name. | Future Tier III path names/actual classes are not yet committed. |
| Lineage and loadout | Inherited approved roster: from **Physically Blessed Novice** at Tier I level 30, 1H sword + shield, Tier II new level-1 manifestation preserving parent, STR/VIT, frontline physical tank, solo viability. | Exact stat baselines, growth, visual NPC skin, gear permissions. |
| Resource | **CONFIRMED:** Resolve **0–100**; generated from taking hostile **pre-mitigation** damage and specifically tagged abilities, with sensible per-hit safeguards, **very slow decay**. Better defenses, Bulwark mitigation, and shield absorption must not reduce incoming-hit Resolve credit. | Exact gain coefficient, hit/cast cap, decay rate/delay, hit exclusions and death/load reset remain balance OPEN. |
| Elemental shields | **LATEST GLOBAL RULE** after Defender discussion: **0.8 shield HP consumed per 1 matching-element damage**, **1.2 shield HP per 1 nonmatching ordinary damage**, or equivalently **1 shield HP prevents 1.25 matching / about 0.833 nonmatching damage**. | Pure-damage eligibility per skill; not physical equipment shields by default. |
| Skills | **CONFIRMED through Round 3:** targeted short-charge/stun/interrupt Shield Bash obeys existing global **Do Nothing / Move Into Range** out-of-range behavior; Challenge AoE taunt/threat; Bulwark spends Resolve for mitigation while taking pre-mitigation hits builds Resolve; Shield Sweep AoE grants **+1 P-DEF and +1 M-DEF × Shield Sweep rank × distinct enemies hit (max FIVE)** to allies **INCLUDING Defender** within Hold the Line's radius; Hold the Line primarily personal defense/Resolve plus weak **+1 percentage point ALL elemental resistances per rank** in **5m circular aura** shared with Sweep's ally-buff reach. | Specific skill rank caps, cooldowns, buff duration/refresh, threat values, dash maximum range and some effects still open. |
| Advancement | **An instanced defend-the-gate quest at Trimery** from the distinguished veteran guard is directionally **APPROVED**, protecting people/guards from attackers. | NPC identity, threat types/waves, duration, fail/retry, when turn-in grants class. |

## Earlier approved roster and class fantasy (not new skill sign-off)

- One of four Tier II base classes: **Greatblade = physical damage**, **Defender = physical tank**, **Mage = magical damage**, **Priest = healer**.
- **Defender** is not an unkillable Defender with zero damage, nor a permanently passive stat sponge; should be able to do normal solo content. Sword-and-shield silhouette and facing/action-guard feedback are important.
- A **distinct veteran Defender among Trimery gate guards** is the mentor; **other guards remain normal**, not generic Tier II soldiers.
- Original pre-interview concepts in `TIER2_CORE_CLASSES_DESIGN_V01.md`: Shield Bash = damage/interrupt, Challenge = real aggro, Bulwark = timed mitigation, Shield Sweep = frontline AoE plus safety, Hold the Line = Resolve and resilience. **Round 2 has now CONFIRMED the core function of ALL FIVE concepts**, with Shield Bash additionally becoming a short charge, Shield Sweep giving P-DEF/M-DEF per enemy struck, and Hold the Line gaining a weak visible ally-resistance aura. Numerical starting balances, rank caps and some edge behavior are still open.
- **Potential later specialization names:** Guardian (ally protection), Vanguard (aggressive/forward pressure), Templar/Paladin (martial magic hybrid), Juggernaut (bruiser) all possibilities **only**; no Tier III routes finalized.

## Confirmed Resolve generation architecture (DEFENDER-003)

**Core:** successful hostile incoming hit events targeted to Defender generate Resolve **based on the raw eligible incoming damage before mitigation**. Resolve may also be awarded explicitly by tagged abilities, such as an actual successful Shield Bash or Shield Sweep. **Simply dealing ordinary weapon damage or spamming Challenge without an affected target must NOT automatically award Resolve** unless later user-approved.

Suggested conceptual pipeline:
1. Confirm a real hostile combat hit/action has connected; avoid free resource on missed/dodged/unapplied attacks. For blocked/absorbed incoming hits that connected, the Defender **does** earn Resolve.
2. Take the incoming damage as it is about to enter target mitigation: **before Physical/Magical Defense, elemental resistance, shield HP absorption, and Bulwark guarding reduction**. Calculate resource from this value rather than actual lost HP.
3. Apply a tuned, bounded **per-hit/per-action Resolve gain**; **a huge multi-hit boss attack and tiny 0.1-second DoT ticks should not generate pathological unlimited Resolve**. The exact number formula/cap and DOT grouping are **proposals/open**.
4. Attribute the event **only once** to the Defender taking/intercepting it; don't duplicate gain per Defense, shield, guard and HP-reduction layers. Resources cannot be farmed by intentionally damaging oneself or friendly fire unless later approved.

**Important:** a held physical equipment shield grants defense/blocking mechanics according to equipment/combat systems and **is not automatically an elemental shield HP effect**. Only skills/gear explicitly creating an elemental barrier use the shared 0.8/1.2 ratios. Heavy armor and active Bulwark should make Defender *tougher without penalizing resource generation*.

## Tier II Defender skill kit — confirmed concepts through Round 2; balance proposals distinguished

**All five previously proposed skill identities are now accepted in broad mechanical form**, but precise amounts, cooldowns, targeting geometry, Resolve scaling and rank progression are not yet approved unless separately marked. Numeric examples below are **provisional playtest proposals**, not shipped code.

### 1. Shield Bash — targeted charge obeying out-of-range setting (DEFENDER-006/011)

- **CONFIRMED:** targeted short shield-first dash into a hostile, shield impact, short stun on vulnerable enemies, interruption of separately interruptible enemy casts. Boss stun immunity must not automatically make explicitly interruptible casts immune to interruption.
- **CONFIRMED DEFENDER-011:** follow the **EXISTING player-wide out-of-range skill behavior setting**; do **NOT** add a Defender-only option. If **Do Nothing** is selected, an out-of-range Shield Bash does not initiate pursuit/automatic movement (the normal inability-to-cast feedback may apply). If **Move Into Range** is selected, approach target via ordinary pathing and then initiate the short targeted dash/bash once the target becomes valid and close enough. Re-evaluate range/target/path if target moves or dies; do not charge through walls or blocked paths.
- **PROPOSED short-dash range:** approximately **5m** (the user's reply confirmed the *setting behavior*, not the numeric 5m suggestion). At melee distance, simply perform impact without a pointless dash. Resolve proper physics/collision and never teleport or phase through obstacles.
- **PROVISIONAL:** ~0.75s vulnerable-target stun, ~8–10s cooldown, modest Physical damage, ~8 Resolve on an actual landed hit.
- **OPEN:** final dash travel range/speed, enemy/ally collision, interruption/cancellation/refund rules, cooldown commencement, line of sight and failed targets. Settings' exact saved/internal identifier should be inspected by the implementation chat, not invented here.

### 2. Challenge — CONFIRMED nearby AoE taunt/threat (DEFENDER-007)

- **CONFIRMED:** a **nearby-area taunt** rather than single-target-only. Real enemy **threat** is increased for each affected hostile, making the Defender genuinely regain targets from allies. Regular taunt-susceptible enemies temporarily focus the Defender; bosses may ignore forced target changes but still gain real threat unless encounter rules override it.
- **PROPOSED radius:** **5m**, **forced-target time ~2s**, **cooldown ~15s**, **+10 Resolve when at least one actual enemy is affected**, never on empty casts.
- **OPEN:** exact radius, duration, threat amount relative to current target, boss taunt immunity, whether rank-ups add defensive benefit/damage to keep the skill valuable when playing solo.

### 3. Bulwark — CONFIRMED Resolve spender for mitigation (DEFENDER-008)

- **CONFIRMED:** **spend Resolve** to take a **temporary defensive/mitigation stance**; it is not a free generator-only block. Incoming hits while Bulwark is active still grant Resolve according to their **full eligible pre-mitigation incoming damage**, NOT damage remaining after armor/resistance/guard/elemental shield or HP loss. This is key to not punishing tank gear and defensive play.
- **PROPOSED first-rank costs/balance:** ~**25 Resolve**, **4 seconds** duration, **30% reduction to incoming eligible ordinary damage** during effect, **20–24s cooldown**, perhaps **20% movement slowing** while guarding. These exact figures/movement constraints are **NOT user-approved**.
- **OPEN:** buff persistence during movement/casting, frontal directional guard versus all directions, protection of allies versus self only, exact rank scaling and any interaction with CC.

### 4. Shield Sweep — confirmed rank-scaled ally defense and self-inclusion (DEFENDER-009/012)

- **CONFIRMED:** frontal Physical sword-and-shield AoE that damages nearby hostiles, generates Resolve on hits, and protects **every eligible nearby ally INCLUDING THE DEFENDER THEMSELVES**. The recipient circle is exactly the **same 5m radius as Hold the Line's resistance aura**, not the frontal cone in which enemies are hit.
- **CONFIRMED BUFF FORMULA (skill rank R, distinct successfully hit enemies N):**
  `bonusPhysicalDefense = bonusMagicalDefense = R × min(N, 5)` **flat Defense points**, NOT percentages, NOT +2 per hit, and NOT scaling from Resolve or Hold the Line rank. Count a given hostile once per Shield Sweep activation; no benefit for misses/immune/invalid non-hits. At rank 8 and 5 enemies successfully hit, buff = **+40 P-DEF / +40 M-DEF** for all recipients including caster.
- **CONFIRMED:** **at most 5 monsters count** for the buff even if the frontal attack actually hits more; extra targets can still receive AoE damage per the skill definition.
- **PROVISIONAL, not yet user-approved:** ~3s buff duration; ~160–180° frontal enemy cone, ~3m enemy reach; 10–12s cooldown; +5 Resolve per unique hostile hit with +15 per cast cap; refreshing/replacing buffs from the same skill/caster rather than continually self-stacking.
- **OPEN:** exact buff lifetime, stacking/refresh for repeat casts and different Defenders, whether DEF buffs snapshot their recipient set at successful cast or follow allies moving in/out for the duration. Aura range remains the visual guide for which allies benefit *when Shield Sweep resolves*.

### 5. Hold the Line — confirmed five-meter all-element aura (DEFENDER-010/013)

- **CONFIRMED main passive:** enhance Defender's **personal defense and Resolve generation**, not primarily group threat or group mitigation. Holding still must not be required for normal positioning/dodge play.
- **CONFIRMED AURA:** within **5m circular radius around Defender**, eligible group members **including Defender** gain **+1 percentage point to EACH ELEMENTAL RESISTANCE per purchased Hold the Line skill rank**. Example: rank 5 → **+5 percentage points Fire, Ice, Metal, etc.** per eligible ally. This is not raw resistance rating and does not grant Physical/Magical DEF by itself; the latter comes from Shield Sweep when activated.
- **CONFIRMED SHARED RANGE:** the Hold the Line aura's visible area is the **same five-meter allied-recipient radius** as Shield Sweep's P-DEF/M-DEF buff. The *Shield Sweep enemy hit cone* may be narrower/shorter and does not define which allies receive defense.
- **CONFIRMED AURA STACKING:** when multiple Defenders' Hold the Line auras overlap, the strongest applicable **same-identity aura** applies, following `docs/STATS_AND_DAMAGE.md`; **do not stack separate copies additively**. Different authored aura types use their normal framework rules.
- **PROVISIONAL:** personal Physical DEF and potentially Magical DEF per rank, a small increase to qualified Resolve generation, out-of-combat aura availability and visual radius decal intensity. These still need tuning. The aura is meant to be very weak and primarily helps allies see the Sweep buff radius, not a dramatic tank-scale elemental protection bubble.
- **OPEN:** maximum passive rank, precise personal defense/Resolve bonuses, whether out-of-combat aura visibility can be toggled, and aura refresh behavior during class/zone changes.

## Resolve — confirmed 0–100 meter and very slow decay (DEFENDER-014)

- **CONFIRMED:** maximum **100 Resolve**; taking hostile incoming damaging hits and using approved Resolve-generating skills adds resource. Ordinary autoattacks do not grant a separate free Resolve bonus unless later designed as a skill-specific exception.
- **CONFIRMED:** amount gained from taking damage is based on **pre-mitigation incoming damage** (before Physical/Magical Defense, element resistance, Bulwark mitigation, or shield absorption). Damage avoidance/real miss yields no resource on an attack that never connects.
- **CONFIRMED:** prevent abuse with some **maximum gain per qualifying hit/action**, avoiding infinite resource generation from massive boss attacks and rapid separate DoT ticks. Exact formula/cap not yet determined.
- **CONFIRMED:** **very slow decay**, rather than indefinite storage. Exact start delay, per-second decay rate and whether decay pauses in combat are open for the next round; design direction suggests decay mostly/outside combat.
- **PROPOSED playtest numbers only:** gain could use `min(12, 2 + 0.03 × rawIncomingHitDamage)` Resolve per meaningful hostile hit, with DoT grouping; outside combat start after a short delay then decay **~0.5–1 Resolve/sec**. This is a designer illustration, **not** a fixed number or approval. Incoming raw damage scale must be validated before choosing a coefficient.
- **EXCLUDED unless separately agreed:** self-inflicted damage, friendly fire and duplicate Resolve per defense/shield/HP-loss stage.

## Future Tier III counterattack concept — USER IDEA, NOT A DEFENDER TIER II SKILL (DEFENDER-015)

**User's original concept, preserved without silently finalizing it:**
- For **exactly 2 seconds**, character gains **+500 Physical Defense AND +500 Magical Defense multiplied by the ability's rank**.
- During those two seconds, accumulate/store **ALL incoming PRE-MITIGATION damage** into a temporary charge instead of merely counting actual HP lost.
- At the end of the two seconds, **release the stored damage** as a powerful retaliatory attack. **Whether this release is SINGLE-TARGET or AREA-OF-EFFECT remains deliberately undecided.**
- User is **UNSURE whether to include this at Tier II or reserve it for Tier III** and leaned toward reserving as a potentially later class-defining skill; don't presume either is selected. **Working name “Reckoning” is the designer's suggestion ONLY**, not approved naming.
- My design recommendation (not user-confirmed): reserve for a specialized Tier III **Guardian (defensive payback)** or **Vanguard (offensive payback)** rather than overload baseline Tier II Defender, which already has Bulwark as a Resolve-consuming defense. This is concept design, **not** a Tier II ultimate and does not change the Tier IV+ ultimate restriction.
- **OPEN design risks:** exact damage snapshot point vs raw action coefficients (physical and magical packets, DoT, friendly hits, redirected damage); prevent duplicated damage credit from mixed multi-packet hits; whether outgoing retaliation is mitigated by normal enemy defenses, affected by crit, target selection/radius, duration ending on death, damage storage cap, ratio/scaling and Resolve cost/cooldown. Using +500 DEF/rank inside the shared diminishing-return Defense model may generate extremely high defenses but not outright invincibility; test future ranks separately.
- **NO IMPLEMENTATION, no new class identity or advancement fork approved by this idea.**

## Class skill-point and level-gate framework

- Universal class rule: every Defender ability starts **rank 0**; the new Tier II manifestation starts level **1 with one spendable skill point**; no free skill ranks on first unlock.
- No Tier II ultimate; future ultimate reserved for later tiers.
- **Bounded budget** once caps chosen: `min(classLevel, ΣDefender skill max ranks)`; skillpoint HUD only when **at least one rank can be bought immediately**; quiet banked points and next gate levels appear inside class skill menu.
- Goal based on recently confirmed other Tier II designs: approximately **78–80 total purchasable ranks near level 80**, with ability-specific max ranks and gates; **NOT YET approved specifically for Defender**.
- If future user adds/removes Defender skills or changes ranks, do not force a universal four-active/one-passive kit; the original five are concept proposals.

## Advancement quest — direction agreed, details unchosen

- **Mentor:** distinctive Defender veteran among the existing Trimery gate guards, not every guard.
- **Trial:** **instanced defense of Trimery's gate**, protecting allied villagers and guards from waves of local threats, while still Physically Blessed Novice. This tests defensive combat/interception rather than damage-race requirements; sword and physical-support abilities should suffice.
- The quest is not automatically a main-story event. Threat type, objective NPC, number of waves, ally support, failure/retry, completion conditions and mentor turn-in remain OPEN; do not copy Priest's inspector escort by accident.
- On approved final turn-in, create separate saved Defender level-1 manifestation and retain Tier I parent, per existing class fork rules.

## Round 2 — user answers recorded 2026-10-19

- **DEFENDER-006 — CONFIRMED:** Shield Bash retains a **short stun and interrupt** of interruptible casts, and becomes a **small targeted dash/charge INTO the enemy** before the shield hit. Cooldown/range and actual contact rules remain numeric design questions.
- **DEFENDER-007 — CONFIRMED:** Challenge uses the described **nearby AoE taunt**, substantive real threat and brief focus redirection, with Resolve only when enemies actually affected. The previously proposed numerical duration/radius are still playtest baselines.
- **DEFENDER-008 — CONFIRMED:** Bulwark **spends Resolve** for mitigation; the Defender **continues to generate Resolve from taking incoming damage while guarding**, based on pre-mitigation damage exactly as outside the stance.
- **DEFENDER-009 — CONFIRMED:** Shield Sweep is frontline enemy AoE Resolve generation plus a nearby-**ally** buff that grants **both Physical DEF and Magical DEF per distinct enemy struck**, NOT the former generic % reduction. Ally recipients are determined by a circular radius identical to Hold the Line's aura radius. The enemy hit area remains a forward-facing cone.
- **DEFENDER-010 — CONFIRMED:** Hold the Line prioritizes **self-defense and Resolve generation**, but also grants a **very weak +1 elemental resistance per passive rank ally aura**. Primarily acts as visible proximity/range guidance for Shield Sweep's protective buff. Exact elemental breadth/unit (all resistances vs one school; percentage points), max rank, radius, personal mitigation amounts and Resolve cap remain undecided.

## Round 3 — five answers recorded (2026-10-19)

- **DEFENDER-011 — CONFIRMED:** Shield Bash obeys the **existing out-of-range skill setting**. **Do Nothing** = do not pursue when out of range; **Move Into Range** = approach target then execute short targeted dash/bash when valid. Numeric dash range remains open; prior 5m was a proposal, not explicitly approved.
- **DEFENDER-012 — CONFIRMED:** Shield Sweep grants **+1 flat Physical DEF and +1 flat Magical DEF PER Shield Sweep skill rank × enemies successfully struck**, **counting a maximum of five distinct enemies**, and **includes the Defender themselves** among allied buff recipients. Example rank8 × five hits = +40 PDEF/+40 MDEF. Duration and stacking still open; previous +2-per-hit amount is superseded.
- **DEFENDER-013 — CONFIRMED:** Hold the Line grants a **+1 percentage point to ALL elemental resistance types per passive rank**, in a **five-meter radius** that also determines Shield Sweep ally recipients. Multiple identical Defender auras overlap using the **strongest**, not additive stacking.
- **DEFENDER-014 — CONFIRMED:** Resolve capacity **0–100**, generated by eligible **pre-mitigation** hits and designated skill generators with per-hit/cast abuse protection, and **very slow decay**. Exact coefficients/decay time remain open.
- **DEFENDER-015 — USER IDEA PRESERVED, TIER STILL UNDECIDED:** highly defensive **2-second counter skill**, grants **+500 PDEF and +500 MDEF × skill rank**, stores **all incoming pre-mitigation damage**, then unleashes the stored damage. Single-target vs AoE unknown; user asked whether it belongs to future Tier III. The assistant recommends reserving it for Tier III, but user has not yet locked the choice. No new Tier II ability automatically approved.

## Round 4 — five questions (awaiting answers)

**DEFENDER-016 — Counterattack tier:** Should we **reserve the new 2-second +500 PDEF/MDEF-per-rank damage-storage-and-release skill exclusively for Tier III**, keeping Tier II Defender to the five current skills? My recommendation is yes; we can later specialize a **Guardian/Vanguard** around it without compromising Defender's introductory tank kit. Or do you want a smaller version now in Tier II?

**DEFENDER-017 — Resolve decay:** Would **zero Resolve decay during combat** and **very slow decay once out of combat** feel right? A provisional starting point is **0.5 Resolve per second after 5 seconds without combat**, with resource cap 100. Or should it slowly decay even during combat?

**DEFENDER-018 — Shield Sweep buff duration and refresh:** With the confirmed `Sweep rank × min(enemies hit, 5)` flat PDEF/MDEF formula, should the ally buff last **3 seconds**, and should a fresh successful Sweep **replace/refresh the same Defender's earlier buff** rather than additively stacking it? Multiple Defenders could contribute separate strengths, with the strongest same-effect buff taking priority.

**DEFENDER-019 — Shield Bash dash reach:** Should we lock its maximum charge distance at **5 meters** before automatic pathing/range behavior kicks in (depending on existing options)? This would be a brief targeted gap-closer that cannot cross blocked geometry, not a long-range teleport.

**DEFENDER-020 — Advancing the design:** Next, should we design **Defender's five skill rank caps and individual rank-level gates**, aiming for approximately **78–80 purchasable ranks by level 80**, with one usable purchase per class level as with Greatblade/Mage? Or would you like to work out the **Trimery gate defense advancement quest** first?

## Decision ledger

| Date | Decision | Status |
| --- | --- | --- |
| 2026-10-19 | DEFENDER-001: latest shield costs 0.8 matching / 1.2 other shield HP per 1 blocked damage | **CONFIRMED GLOBAL** |
| 2026-10-19 | DEFENDER-002: class name Defender; stronger names Guardian/Vanguard reserved as potential later classes | **CONFIRMED** |
| 2026-10-19 | DEFENDER-003: Resolve generated from taking enemy damage measured pre-mitigation and explicitly tagged Resolve-generating skills | **CONFIRMED PRINCIPLE** |
| 2026-10-19 | DEFENDER-004: originally requested full skill details before selecting | **RESOLVED by DEFENDER-006–010: all 5 original skill concepts now accepted in core form** |
| 2026-10-19 | DEFENDER-005: an instanced Trimery gate-defense advancement trial | **CONFIRMED DIRECTION**, details open |
| 2026-10-19 | DEFENDER-006: Shield Bash short targeted charge, stun + interrupt | **CONFIRMED CORE** |
| 2026-10-19 | DEFENDER-007: Challenge AoE real taunt/threat, Resolve if affected | **CONFIRMED CORE** |
| 2026-10-19 | DEFENDER-008: Bulwark Resolve spender, incoming hit generation stays pre-mitigation | **CONFIRMED CORE** |
| 2026-10-19 | DEFENDER-009: Shield Sweep AoE Resolve + P-DEF/M-DEF buff PER target hit to nearby allies | **CONFIRMED CORE; numbers OPEN** |
| 2026-10-19 | DEFENDER-010: Hold the Line personal resilience/Resolve + weak +1 elemental resist/rank ally aura (matching Shield Sweep ally range) | **CONFIRMED CORE; resistance scope/radius OPEN** |
| 2026-10-19 | DEFENDER-011: Shield Bash follows existing Do Nothing / Move Into Range player option | **CONFIRMED** |
| 2026-10-19 | DEFENDER-012: Shield Sweep flat +1 PDEF/MDEF × skill rank × 1–5 distinct hits; includes Defender | **CONFIRMED** |
| 2026-10-19 | DEFENDER-013: Hold the Line 5m +1 percentage point to all elemental resist per rank, strongest identical aura | **CONFIRMED** |
| 2026-10-19 | DEFENDER-014: 0–100 Resolve, pre-mitigation gain, per-hit safeguard, very slow decay | **CONFIRMED; exact formula/decay OPEN** |
| 2026-10-19 | DEFENDER-015: 2s +500 DEF per rank counterskill storing pre-mitigation damage and releasing it | **USER CONCEPT; TIER/AOE/ST OPEN** |
| 2026-10-19 | DEFENDER-016–020: five Round 4 questions | **AWAITING USER** |

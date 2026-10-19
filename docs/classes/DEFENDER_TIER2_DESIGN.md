# DiceFree — Tier II Defender — Design interview

**Created:** 2026-10-19  
**Status:** DESIGN/DOCUMENTATION ONLY — Tier II Defender not implemented. User will instruct the separate working chat to read current documents and implement after a functioning remote connection returns. Do not edit Unity gameplay C#, character saves or builds during these interviews.  
**Branch:** `integration/world1-foundation`  
**References:** [Tier II roster v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [original five skill concepts](../TIER2_CORE_CLASSES_DESIGN_V01.md), [class progression](../CLASS_PROGRESSION.md), [stats and shields](../STATS_AND_DAMAGE.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Greatblade/Mage rank audit](../TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md).

## Confirmed Defender decisions — updated through Round 2 (2026-10-19)

| Area | Confirmed by user | Still awaiting specific design |
| --- | --- | --- |
| Class | **Defender** is the right **Tier II** baseline tank name. **Guardian**, **Vanguard** and similarly stronger/more-specialized names make sense for **future advancement alternatives**, not current class name. | Future Tier III path names/actual classes are not yet committed. |
| Lineage and loadout | Inherited approved roster: from **Physically Blessed Novice** at Tier I level 30, 1H sword + shield, Tier II new level-1 manifestation preserving parent, STR/VIT, frontline physical tank, solo viability. | Exact stat baselines, growth, visual NPC skin, gear permissions. |
| Resource | **Resolve generated when taking enemy damage and when using expressly Resolve-generating skills.** The damage amount used for Resolve generation must be **PRE-MITIGATION**: high Armor/Defense, resistance, damage reduction, shields and active guard do not reduce Resolve gained from the same incoming hit. | Resolve cap, per-hit coefficients/soft cap, DoT and multi-hit grouping, decay, costs. |
| Elemental shields | **LATEST GLOBAL RULE** after Defender discussion: **0.8 shield HP consumed per 1 matching-element damage**, **1.2 shield HP per 1 nonmatching ordinary damage**, or equivalently **1 shield HP prevents 1.25 matching / about 0.833 nonmatching damage**. | Pure-damage eligibility per skill; not physical equipment shields by default. |
| Skills | **CONFIRMED Round 2:** Shield Bash = short targeted charge with a brief stun and interrupt on eligible casts; Challenge = AoE real threat/brief taunt; Bulwark = Resolve-SPENDING temporary mitigation, with Resolve still earned from pre-mitigation incoming hits; Shield Sweep = frontal AoE Resolve generation plus **nearby allies gain P-DEF and M-DEF per enemy hit**; Hold the Line = **personal defense + Resolve-generation passive** with a **weak +1 elemental resistance per passive rank aura**, shared radius with Shield Sweep's ally buff. | Exact dash range, speeds, target/miss behavior, Resolve values, numerical buffs/ceilings, aura element scope/radius and skill rank-up rules OPEN. |
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

### 1. Shield Bash — CONFIRMED short targeted charge, melee impact, interrupt and stun (DEFENDER-006)

- **CONFIRMED:** Shield Bash targets an enemy and **charges/dashes a SHORT distance directly toward that enemy**, finishing with a shield impact. This is not a stationary shield slap or free-aim ground dash. It deals modest Physical damage and causes a **short stun** if target is vulnerable; **interrupts interruptible enemy casts** independently of ordinary boss stun immunity. Against hard CC-immune bosses, preserve the interrupt if the specific boss cast is marked interruptible.
- **PROPOSED range:** approximately **4–6 meters maximum**, closing to melee contact and resolving collision against other entities/world geometry safely. No phasing through walls/enemies; no teleport; no arbitrary extended distance if the target is already in melee range (just bash).
- **PROPOSED stun:** **0.75s**, respecting usual crowd-control immunity/resistance. **PROPOSED cooldown:** **8–10s**.
- **PROPOSED Resolve:** roughly **+8 on a successful enemy hit**, independent of Resolve gained from incoming hostile pre-mitigation damage.
- **OPEN:** exact max range, whether clicking out of range attempts to approach the target automatically, line-of-sight checks, what happens if target moves/dies mid-dash, collision and failed/interrupted charges, cooldown commit and whether Resolve award requires contact.
- **Visual:** short shield-forward lunge, forward body movement and a readable impact/interrupt.

### 2. Challenge — CONFIRMED nearby AoE taunt/threat (DEFENDER-007)

- **CONFIRMED:** a **nearby-area taunt** rather than single-target-only. Real enemy **threat** is increased for each affected hostile, making the Defender genuinely regain targets from allies. Regular taunt-susceptible enemies temporarily focus the Defender; bosses may ignore forced target changes but still gain real threat unless encounter rules override it.
- **PROPOSED radius:** **5m**, **forced-target time ~2s**, **cooldown ~15s**, **+10 Resolve when at least one actual enemy is affected**, never on empty casts.
- **OPEN:** exact radius, duration, threat amount relative to current target, boss taunt immunity, whether rank-ups add defensive benefit/damage to keep the skill valuable when playing solo.

### 3. Bulwark — CONFIRMED Resolve spender for mitigation (DEFENDER-008)

- **CONFIRMED:** **spend Resolve** to take a **temporary defensive/mitigation stance**; it is not a free generator-only block. Incoming hits while Bulwark is active still grant Resolve according to their **full eligible pre-mitigation incoming damage**, NOT damage remaining after armor/resistance/guard/elemental shield or HP loss. This is key to not punishing tank gear and defensive play.
- **PROPOSED first-rank costs/balance:** ~**25 Resolve**, **4 seconds** duration, **30% reduction to incoming eligible ordinary damage** during effect, **20–24s cooldown**, perhaps **20% movement slowing** while guarding. These exact figures/movement constraints are **NOT user-approved**.
- **OPEN:** buff persistence during movement/casting, frontal directional guard versus all directions, protection of allies versus self only, exact rank scaling and any interaction with CC.

### 4. Shield Sweep — CONFIRMED frontal damage/Resolve and ally DEF buff (DEFENDER-009)

- **CONFIRMED:** a short-range **frontal sword/shield AoE** against hostile targets which **generates Resolve** from successful hits and also grants **Physical Defense AND Magical Defense** to **allies near the Defender**. **Buff strength increases per distinct enemy actually hit by THAT Shield Sweep cast** (not per damage tick or each attempted target; misses do not count). This was requested instead of a generic small percentage damage-reduction buff.
- **CONFIRMED RADIUS SYNERGY:** the allied DEF buff uses the **same circular ally radius as Hold the Line's visible elemental-resistance aura**, so players can see who is positioned to receive it. **The radius of the forward-facing enemy attack cone is separate**; do not restrict allies to being in the enemy cone.
- **PROPOSED attack:** frontal **~160–180°**, **~3m reach**, modest Physical damage per target. **PROPOSED Resolve:** +5 per distinct enemy hit, maximum +15 per activation. **PROPOSED cooldown:** 10–12s.
- **PROPOSED ally buff:** **+2 Physical Defense and +2 Magical Defense per distinct hostile hit**, duration **~3 seconds**, with a sensible **per-cast stack/target cap** (e.g. count only first 5 unique targets → at most +10/+10 DEF), and an allied buff radius of **~5m**. This is NOT approved numerical balance; needs user preference or delegation. Determine whether Defender themselves also receives the defense buff.
- **OPEN:** buff per-target numerical contribution, duration, cap/stacking with multiple Defenders, group-target requirement, self-inclusion and buff display. AOE control and enemy hit confirmations must be server/authority accurate eventually.

### 5. Hold the Line — CONFIRMED personal defense/Resolve with visible weak ally aura (DEFENDER-010)

- **CONFIRMED personal passive:** increase Defender's own **defensive capability** and **Resolve generation** as the main identity; do **not** make its primary investment about party-wide threat or ally damage mitigation.
- **CONFIRMED weak aura:** every purchased Hold the Line rank adds **+1 elemental resistance** to eligible allies inside an always-visible or clearly indicated aura around Defender. Aura exists importantly as a **range visualization** for Shield Sweep's short DEF buff, which therefore shares its ally-area radius.
- **WORKING INTERPRETATION, NOT YET EXPLICITLY CONFIRMED:** that means **+1 percentage point to each elemental resistance per Hold the Line rank**, e.g., rank 5 = +5 percentage points to each element in range, rather than a bonus to a single element. This matches existing DiceFree resistance-percent convention, but exact scope needs confirmation. No rank should convert aura to a damage shield just by being called an aura.
- **PROPOSED radius:** **~5 meters circular around Defender**, matching Shield Sweep's allied buff radius; **enemy cone range stays independent**. Passive activates at Hold the Line rank ≥1 and ends on class change/unequip/defeat according to ordinary aura rules. If multiple Defenders are nearby, use the **global aura rule** in `docs/STATS_AND_DAMAGE.md`: **strongest eligible Hold the Line aura applies**, not additive copies, unless an explicit future exception is chosen.
- **PROPOSED personal bonuses:** small increases to the Defender's Physical Defense (possibly Magical Defense as well) and +% Resolve generated from qualifying incoming hits/skills, scaling gently by rank. No requirement to stand motionless; identity can depend on active combat state.
- **OPEN:** exact personal stat coefficients, the Resolve bonus magnitude, aura maximum radius/visibility, whether the passive grants ally aura while out of combat, whether it applies to Defender themselves, how to handle aura overlaps with other skill/gear sources, number of available ranks.

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

## Round 3 — five questions (awaiting answers)

**DEFENDER-011 — Shield Bash targeting and dash.** Would you like the targeted Shield Bash charge to reach **about 5 meters**, travel into melee contact and **stop at walls/units rather than ghosting through**? If an enemy is out of that range, should clicking Shield Bash **move toward them automatically and charge once in range** (following your general targeting settings), or should it simply show “out of range” without moving?

**DEFENDER-012 — Shield Sweep defense strength.** I suggest each distinct enemy hit grants nearby allies **+2 Physical DEF and +2 Magical DEF for 3 seconds**, counting up to **5 enemies** per cast (maximum +10 to each DEF). Does that sound good as an initial test? Should the Defender receive the same buff, or only other allies?

**DEFENDER-013 — Hold the Line aura.** By **+1 elemental resistance per rank**, did you mean **+1 percentage point to ALL elemental resistance types** per rank (e.g. rank 5 grants +5 Fire, Ice, Metal, etc.)? And can we set its radius to about **5 meters**, shared with Shield Sweep ally buff, with only the strongest Defender aura applying when several overlap?

**DEFENDER-014 — Resolve resource.** Shall we start at **0–100 Resolve**, with **no passive gain from regular autoattacks**, and a formula based on each real incoming hit's **pre-mitigation damage**, limiting Resolve earned from a single hit/cast so boss bursts and tiny DoT ticks don't produce unlimited Resolve? Should Resolve decay slowly outside combat, or remain until used?

**DEFENDER-015 — Next design priority.** Shall we keep these **five confirmed mechanics** as Defender's initial roster (four actives + Hold the Line) and next tackle **skill rank caps/level-gate schedules** to allow roughly 78–80 possible investments by class level 80? Or would you like to add a separate damaging Resolve spender/counterattack to make solo offense more interesting before we lock the roster?

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
| 2026-10-19 | DEFENDER-011–015: five Round 3 questions asked | **AWAITING USER** |

# DiceFree — Tier II Defender — Design interview

**Created:** 2026-10-19  
**Status:** DESIGN/DOCUMENTATION ONLY — Tier II Defender not implemented. User will instruct the separate working chat to read current documents and implement after a functioning remote connection returns. Do not edit Unity gameplay C#, character saves or builds during these interviews.  
**Branch:** `integration/world1-foundation`  
**References:** [Tier II roster v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [original five skill concepts](../TIER2_CORE_CLASSES_DESIGN_V01.md), [class progression](../CLASS_PROGRESSION.md), [stats and shields](../STATS_AND_DAMAGE.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Greatblade/Mage rank audit](../TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md).

## Status of decisions — Defender Round 1 (user answers 2026-10-19)

| Area | Confirmed by user | Still awaiting specific design |
| --- | --- | --- |
| Class | **Defender** is the right **Tier II** baseline tank name. **Guardian**, **Vanguard** and similarly stronger/more-specialized names make sense for **future advancement alternatives**, not current class name. | Future Tier III path names/actual classes are not yet committed. |
| Lineage and loadout | Inherited approved roster: from **Physically Blessed Novice** at Tier I level 30, 1H sword + shield, Tier II new level-1 manifestation preserving parent, STR/VIT, frontline physical tank, solo viability. | Exact stat baselines, growth, visual NPC skin, gear permissions. |
| Resource | **Resolve generated when taking enemy damage and when using expressly Resolve-generating skills.** The damage amount used for Resolve generation must be **PRE-MITIGATION**: high Armor/Defense, resistance, damage reduction, shields and active guard do not reduce Resolve gained from the same incoming hit. | Resolve cap, per-hit coefficients/soft cap, DoT and multi-hit grouping, decay, costs. |
| Elemental shields | **LATEST GLOBAL RULE** after Defender discussion: **0.8 shield HP consumed per 1 matching-element damage**, **1.2 shield HP per 1 nonmatching ordinary damage**, or equivalently **1 shield HP prevents 1.25 matching / about 0.833 nonmatching damage**. | Pure-damage eligibility per skill; not physical equipment shields by default. |
| Existing skill concepts | **No final roster approved in this interview yet**. User explicitly asked to hear the specific mechanics of Shield Bash, Challenge, Bulwark, Shield Sweep and Hold the Line **before** deciding to keep them. | Skill list, costs, cooldowns, unlock/rank schedules and synergy subject to approval. |
| Advancement | **An instanced defend-the-gate quest at Trimery** from the distinguished veteran guard is directionally **APPROVED**, protecting people/guards from attackers. | NPC identity, threat types/waves, duration, fail/retry, when turn-in grants class. |

## Earlier approved roster and class fantasy (not new skill sign-off)

- One of four Tier II base classes: **Greatblade = physical damage**, **Defender = physical tank**, **Mage = magical damage**, **Priest = healer**.
- **Defender** is not an unkillable Defender with zero damage, nor a permanently passive stat sponge; should be able to do normal solo content. Sword-and-shield silhouette and facing/action-guard feedback are important.
- A **distinct veteran Defender among Trimery gate guards** is the mentor; **other guards remain normal**, not generic Tier II soldiers.
- Original pre-interview, high-level ideas in `TIER2_CORE_CLASSES_DESIGN_V01.md`: Shield Bash = damage/interrupt, Challenge = actual aggro, Bulwark = timed damage reduction stance, Shield Sweep = melee AoE plus safety, Hold the Line = frontline resilience/Resolve stability. The old docs did **not** fix numerical damage, stun lengths, cooldowns, Resolve costs, taunt duration, or rank caps.
- **Potential later specialization names:** Guardian (ally protection), Vanguard (aggressive/forward pressure), Templar/Paladin (martial magic hybrid), Juggernaut (bruiser) all possibilities **only**; no Tier III routes finalized.

## Confirmed Resolve generation architecture (DEFENDER-003)

**Core:** successful hostile incoming hit events targeted to Defender generate Resolve **based on the raw eligible incoming damage before mitigation**. Resolve may also be awarded explicitly by tagged abilities, such as an actual successful Shield Bash or Shield Sweep. **Simply dealing ordinary weapon damage or spamming Challenge without an affected target must NOT automatically award Resolve** unless later user-approved.

Suggested conceptual pipeline:
1. Confirm a real hostile combat hit/action has connected; avoid free resource on missed/dodged/unapplied attacks. For blocked/absorbed incoming hits that connected, the Defender **does** earn Resolve.
2. Take the incoming damage as it is about to enter target mitigation: **before Physical/Magical Defense, elemental resistance, shield HP absorption, and Bulwark guarding reduction**. Calculate resource from this value rather than actual lost HP.
3. Apply a tuned, bounded **per-hit/per-action Resolve gain**; **a huge multi-hit boss attack and tiny 0.1-second DoT ticks should not generate pathological unlimited Resolve**. The exact number formula/cap and DOT grouping are **proposals/open**.
4. Attribute the event **only once** to the Defender taking/intercepting it; don't duplicate gain per Defense, shield, guard and HP-reduction layers. Resources cannot be farmed by intentionally damaging oneself or friendly fire unless later approved.

**Important:** a held physical equipment shield grants defense/blocking mechanics according to equipment/combat systems and **is not automatically an elemental shield HP effect**. Only skills/gear explicitly creating an elemental barrier use the shared 0.8/1.2 ratios. Heavy armor and active Bulwark should make Defender *tougher without penalizing resource generation*.

## Proposed Tier II skills — specific designer concepts, NOT YET APPROVED

All numbers in this section are **illustrative initial playtest suggestions**, not existing implementation, not chosen rank caps, and not an invented earlier agreement. The final five-skill roster itself remains unapproved pending user's reaction.

### 1. Shield Bash — active, damage, interrupt and short CC (PROPOSAL)

- **Target/range:** one hostile at **melee range** in front of Defender; physical, non-elemental shield impact.
- **Use:** deal a **moderate instant strike** (roughly one standard sword attack's damage) and **interrupt an interruptible enemy cast** if it is underway.
- **CC:** briefly **stun a non-immune normal target for ~0.75 seconds**; bosses obey general CC immunity/resistance rules, so interrupt eligibility is separate from a guaranteed boss stun.
- **Resource:** **generates approximately 8 Resolve on a successful enemy hit**, not on a miss; Resolve gained from simultaneously taking incoming damage is separate and uses pre-mitigation damage.
- **Initial cooldown target:** **8–10 seconds**, short enough to feel like a useful primary utility strike. Future ranks should chiefly improve damage and/or cooldown, with capped CC.
- **Role:** practical in solo combat and boss cast interruption. A fast shield smash rather than a slow spell.

### 2. Challenge — active, meaningful threat and enemy pressure (PROPOSAL)

- **Target/area:** instantly affects hostile enemies near Defender (**proposed ~5m radius**, facing not required).
- **Mechanic:** **big real threat injection** on affected enemies; taunt-susceptible normal monsters **focus Defender ~2 seconds**. Bosses may be immune to forced-target effects, but actual threat increase still applies, using the shared targeting/threat system.
- **Beyond party play:** grant a modest **Resolve gain (proposed ~10)** if at least one real enemy is affected, not on empty casts; optional small short enemy offense reduction if solo utility feels insufficient (not locked).
- **Initial cooldown target:** **15 seconds**. Can defend healer/caster by taking enemy attention; avoid infinite hard-taunt chains from rank increases.
- **Role:** aggro control that matters with one to four human players, not cosmetic flashing.

### 3. Bulwark — active, timed defensive stance (PROPOSAL)

- **Trigger/cost:** **spend about 25 Resolve** to raise shield into a defensive stance (unaffordable at zero). No passive free shield barrier generated solely by equipping a physical shield.
- **Effect:** for **~4 seconds**, reduce ordinary incoming damage by around **30%**; visibly brace/raise shield and perhaps **slow movement ~20%**, while retaining basic defense/counterattack interactions.
- **Resource fairness:** still **gain Resolve from hits using PRE-mitigation damage**, not reduced actual HP loss, so the build is not punished for pressing Bulwark. Prevent self-sustaining infinite defense through sensible cost/cooldown.
- **Initial cooldown target:** **20–24 seconds**. Later ranks improve mitigation or cost modestly but not permanent uptime.
- **Role:** tactical answer to boss telegraphs, especially when staying in front of allies; potential nearby-ally protection is a separate choice, not assumed.

### 4. Shield Sweep — active, frontline AoE damage and Resolve generator (PROPOSAL)

- **Target/area:** short, **~160–180° frontal cone** centered on Defender, around **3m reach**. Weapon/shield swipe hitting multiple hostiles; normal physical damage.
- **Damage:** modest per-target AoE comparable to weaker sword strikes, below dedicated Greatblade cleave burst.
- **Resource:** **generate ~5 Resolve for each distinct hostile hit, capped at ~15 Resolve per cast**, so packs empower tank play without infinite gain from huge swarms.
- **Initial cooldown target:** **10–12 seconds**. Optional brief nearby-ally protection of **~5–10% reduced damage for ~2 seconds**, if the user likes adding this support identity; this part is NOT approved.
- **Role:** manageable solo farming and snap group threat through actual AoE hits. No hidden permanent road-blocking collision volume.

### 5. Hold the Line — passive, real frontline resilience (PROPOSAL)

- **Core:** when actively engaged with nearby enemies / maintaining threat or holding an enemy's attention, improve **Resolve generation and modest physical resilience**. Avoid forcing the player to stand perfectly still because dodge-and-position gameplay is important.
- **Illustrative benefit:** at early ranks, **small +% incoming Resolve gain** and **small Physical Defense boost** when actively tanking. Future milestone ranks may add a short bonus after intercepting damage or successfully using Bulwark.
- **Ranking:** meaningful stat upgrades across levels with occasional qualitative milestones. Do not turn every rank into a new arbitrary skill and do not create indefinite invulnerability.
- **Role:** long-term identity-building passive; unlike Greatsword Mastery, not a plain sword DPS passive.

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

## Round 2 — next five questions (awaiting user)

**DEFENDER-006 — Shield Bash.** Do you like an **8–10s cooldown melee shield strike** with modest physical damage, **interrupt on eligible casts**, and about **0.75 seconds of stun** on vulnerable enemies, generating Resolve when it connects? Or should Shield Bash be more focused on control or damage?

**DEFENDER-007 — Challenge.** Should Challenge be a **short-ranged AoE taunt** (enemies around the Defender briefly target them, strong actual threat, Resolve awarded only if enemies are affected), or prefer **single-target taunt** with greater boss utility? How important should it feel while solo?

**DEFENDER-008 — Bulwark.** Should Bulwark be a **short defensive stance costing Resolve** (~25 Resolve for 4 seconds of ~30% damage reduction, with a movement tradeoff), or a **Resolve-generating block** that can be used at zero Resolve? We can give the class a separate Resolve spender later if you prefer.

**DEFENDER-009 — Shield Sweep.** Do you want a **short-range frontal AoE shield/sword sweep** that damages groups and generates Resolve, optionally granting a **tiny short protection buff to close allies**? Or would you rather keep this purely offensive / AoE threat?

**DEFENDER-010 — Hold the Line and resource feel.** Would you prefer Hold the Line to give **Resolve gain + some Physical Defense while actively tanking**, or to provide **ally protection / better threat generation** as the central passive? For Resolve, is **0–100 capacity with some per-hit gain cap and no gain from missed attacks** a reasonable provisional framework?

## Decision ledger

| Date | Decision | Status |
| --- | --- | --- |
| 2026-10-19 | DEFENDER-001: latest shield costs 0.8 matching / 1.2 other shield HP per 1 blocked damage | **CONFIRMED GLOBAL** |
| 2026-10-19 | DEFENDER-002: class name Defender; stronger names Guardian/Vanguard reserved as potential later classes | **CONFIRMED** |
| 2026-10-19 | DEFENDER-003: Resolve generated from taking enemy damage measured pre-mitigation and explicitly tagged Resolve-generating skills | **CONFIRMED PRINCIPLE** |
| 2026-10-19 | DEFENDER-004: review specifics of original 4 actives + passive before accepting class skills | **PENDING USER APPROVAL** |
| 2026-10-19 | DEFENDER-005: an instanced Trimery gate-defense advancement trial | **CONFIRMED DIRECTION**, details open |
| 2026-10-19 | DEFENDER-006–010: five Round 2 questions | **AWAITING USER** |

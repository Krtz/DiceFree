# DiceFree — Tier II Priest — Design Interview

**Status:** DESIGN INTERVIEW IN PROGRESS — Rounds 1–8 recorded; Round 9 awaiting answers. **Design only: do not implement from this document until user sign-off.**  
**Started:** 2026-10-17  
**Repository branch:** `integration/world1-foundation`  
**Design owner:** User; interview and documentation in five-question rounds, then the other DiceFree chat can implement.

**Read these existing references before implementing anything:** [recent Priest proposal](../PRIEST_TIER2_PROPOSAL_2026_10_17.md), [Tier II core classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [early Tier II proposals v0.1](../TIER2_CORE_CLASSES_DESIGN_V01.md), [Trimery healer placeholder](../TRIMERY_HEALER_2026_10_17.md), [class progression](../CLASS_PROGRESSION.md), [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md), [Mage implemented specification](MAGE.md), [combat/stat framework](../STATS_AND_DAMAGE.md), and [flexible skills](../PLAIN_GEAR_AND_FLEXIBLE_SKILLS.md).

> This document is the forthcoming authoritative **Priest-specific** design as the user answers. Prior proposed skills/quests remain **PROPOSED** until approved here. Do not mistake an implemented NPC service for an implemented playable Tier II class or advancement quest.

## Workflow

1. Ask **exactly five questions per round** and wait for user answers.
2. After each round, update this document with question IDs, **CONFIRMED** user decisions, and separate **OPEN** mechanics. Never invent numbers or claim a proposal was approved.
3. Develop class role, exact skill roster (actives and passives), Mana/Grace generation and spending, stat scaling, equipment, rank progression and unlocks, all spell targeting/ranges/cooldowns/formulas, mentor quest, visual identity and verification acceptance criteria.
4. This conversation is **design/documentation only**. The implementation chat should consume a complete reviewed specification later; no Unity/C# gameplay code, scene, balance or NPC changes during this interview.
5. Preserve existing game rules and class-independent mechanics. No arbitrary universal four-active/one-passive cap, and **no Tier II ultimate** (the reserved ultimate is for Tier IV+).
6. A Tier I **level-30 Magically Touched Novice** can eventually advance to **Tier II Priest starting level 1** on a new persistent manifestation; retain the parent and its save. The Tier II progression threshold remains level **60**.

## Previously agreed foundations — confirmed from other class discussions

| Subject | Existing commitment | Notes |
| --- | --- | --- |
| Class name/tier | **Priest, Tier II**, advancing from **Magically Touched Novice** at its level-30 quest threshold. | Quest itself not yet approved/implemented. |
| Role | **CONFIRMED PRIEST-001:** general-purpose **hybrid healer** at Tier II, mixing direct healing, shields, periodic healing and meaningful damage; solo capable. Distinct specializations are for potential Tier III Priest advancements. | Not a narrowly specialized Tier II healer. Exact relative throughput open. |
| Weapons | **CONFIRMED PRIEST-023:** Priest uses **staffs only**, with exceptions for items that explicitly have **all-classes** usability. Ordinary autoattacks remain normal/non-elemental and **never generate Grace**. | Staff combat reach/visual/basic attack scaling remains a separate question; the user did not choose an inherent Light basic attack. |
| Attributes | SPR primary for healing **and** damage. User delegated precise stat distribution. **Provisional Tier II Priest baseline:** VIT 12, STR 5, AGI 6, INT 11, SPR 18; per-level growth +1.5/+0.5/+0.75/+1.5/+3 respectively. | Playtest starting values; not exact numbers dictated by user. Tier II stats/growth exceed Tier I overall, on par with Tier II Mage. |
| Resources | Mana + Grace. **Mana management is the main long-fight balance lever**; Mana costs increase per invested rank, cooldowns decrease a little per invested rank. Numeric costs/damage/regeneration/cooldowns are **delegated to designer for provisional playtesting**, subject to later rebalancing. Grace thresholds 10/9/8/7/6 for ranks 1–5; no decay and starts at zero on load; +2 for a successful zero-overheal heal, +1 for other successful spells. | Spell rank ceiling, actual skill potency/Mana regeneration and Grace edge cases OPEN. |
| Without Grace | Priest retains usable damage/healing spells paid for with Mana; cannot require Grace for its basic function. | Specific ranks/ability gating open. |
| Skill system | Four regular spells Holy Light/Light Burst/**Ice Aegis**/Wild Growth can be purchased at Priest level 1 and ranked up at **any character level**, up to an unconfirmed maximum. Grace (5 ranks) unlocks at 5/10/15/20/25; Revive (1 rank) at 20; skills start at 0 with one point at level 1. User wants **almost all** skill points spent by the next class milestone +33%: interpreting Tier II's level-60 milestone as ~level 80, **proposal** is **18 ranks per ordinary spell (72) + Grace (5) + Revive (1) = 78 ranks out of 80 points**. | Specific 18-rank cap and level-80 interpretation **PROPOSED pending approval**, not final. |
| Mentor | Trimery healer offers level-30 Magically Touched solo-human instanced escort Trimery → Cornberg: **inspector (cannot fight; must be healed) + two guards (fight; may die)**. Several small **wolves and slimes** attacks (no Alpha Wolf or slime dungeon boss); **fully maxed Mend-only player run must be possible**. Inspector death fails and teleports player to Trimery outside instance. Cornberg arrival **permanently completes escort stage**, then player travels back to Trimery healer to explicitly turn in and create Tier II Priest. | Exact AI/routing/waves, player death, reentry, quest dialogue and title open. |
| Resurrection | Revive rank 1 at level 20; 8s cast, 4s with Grace, >50% current/max Mana to begin, all current Mana spent on success, interrupted cast costs nothing and refunds 60s cooldown. Returns ally with **15% max HP and 15% max Mana**. **Resurrection Sickness: -10% each primary stat per stack, 5-minute independent timer for EACH stack**, never cleared just by ending encounter. | Other resources, maximum sickness stacks, save/load behavior, range/target validity still OPEN. |

## Present in the game — separate from Priest class

As of `integration/world1-foundation` commit `5a74e1112` (2026-10-17), the world has an ivory/blue **Trimery Priest placeholder** close to the village green that offers **free HP treatment to living players** via interaction. It does not refill Mana/Grace, resurrect, clear statuses, start/complete advancement, or unlock Priest. See `docs/TRIMERY_HEALER_2026_10_17.md` for scoped validation. This NPC is a future mentor candidate, not the implemented Priest class.

## Confirmed Tier II Priest kit — updated through Round 8

**Confirmed roster:** **five standard spells** (including the user-added low-tier **Revive**) plus **one manually activated/passive Grace skill**. No Tier II ultimate. Priest scales **both healing and damage from SPR**; the **basic attack is normal, non-elemental and gives no Grace**, and **staffs are its exclusive class weapon** except explicitly all-classes equipment. Start new Priest with all skills **rank 0**, one spendable point at level 1. **Holy Light, Light Burst, Aegis and Wild Growth are eligible for rank 1 at level 1**; Grace first at level 5 and upgrades every five levels; **Revive** becomes eligible at **level 20** and has only **one rank**. The four regular spells can be upgraded at any Priest level, without five/six-level gates. **Provisional maximum is 18 ranks each**, producing 78 total possible ranks with Grace (5) and Revive (1), almost all 80 level-up skill points by level 80; awaiting user approval.

| Ability | Element | CONFIRMED mechanics | OPEN balance/edge cases |
| --- | --- | --- | --- |
| **Holy Light** | Light | Single-target ally heal or enemy damage. With Grace one bounce per spell rank, repeat recipients allowed, heals prioritize lowest-HP-percentage wounded allies; each hop ×0.9 previous potency. Relatively long cooldown shortened per rank and Mana costs rise per rank. | SPR coefficient and magnitude, Mana/cooldown numbers, bounce range, max rank. |
| **Light Burst** | Light | Instant heal/damage AoE centered on a chosen character, Grace grants three pulses 1 second apart following that character; target can be affected by all, only initial pulse earns Grace. Purchasable from level 1 and upgradeable at each next level pending cap. | Radius, SPR coefficients, Mana/cooldown numerical values, cap and multipulse target validity. |
| **Ice Aegis** | Ice | Single-target shield on self/ally. Matching Ice damage consumes 0.5 shield HP/1 prevented damage; non-Ice consumes 1.5. Grace adds **Ice damage every 0.1 seconds** around shield holder while shield remains, with **small per-tick damage** scaling with Priest SPR. Purchasable at level 1, can upgrade at any level (pending max rank). | Shield lifetime and capacity, aura radius, exact tick coefficient, overlapping auras, Mana costs/cooldowns, cap. |
| **Wild Growth** | Nature | Nature allied HoT / enemy DoT, baseline 8s and matching tick magnitudes. Grace: 16s HoT or slowed 8s DoT. **All periodic ticks every 0.25s**, so 32 ticks for 8s, 64 for 16s before any immediate-tick exception. **Its cooldown is >= normal 8s duration at EVERY rank, not necessarily >= empowered 16s**. DoT slow scales toward about 50% at max rank. | Damage/heal coefficient, cost/CD rank formulas, exact slow curve and maximum spell ranks. |
| **Revive** (working name; low-tier) | Resurrection | Single-rank combat rez at lv20, 8s cast (4s Grace), cast starts only above 50% Mana, spends ALL held Mana upon success; 60s cooldown refunded/no Mana on interrupt. Returns dead party member at **15% HP and 15% Mana**, with sickness **-10% each primary stat per stack**, **five-minute individual timer per stack**. | Other resource pools, sickness stacking cap, Revive range/corpse targeting rules and persistence open. |
| **Grace** | Passive + manually activated skill | **Available for first purchase at Priest level 5**; upgradeable at **levels 10, 15, 20, 25** (ranks 1–5). Full meter requirements **10/9/8/7/6**. Successful heal with zero overheal gives **2 Grace**; any other successful spell gives **1**, except **ordinary autoattacks give 0**. No natural decay, starts 0 on load. Full Grace can be manually armed to empower next spell; successful empowerment consumes full Grace and still costs Mana. | Grace use on Revive, pending cast failures/interruptions and Grace-generation on Revive, other per-cast safeguards. |

### Holy Light: Grace bounce details (PRIEST-013/025/030)
- A Grace-empowered Holy Light makes **r additional bounces**, where **r is Holy Light's skill rank**, **not Priest character level**. Bounce destinations can be both friendly and hostile; each destination is healed or damaged according to allegiance.
- **CONFIRMED PRIEST-025/030:** each bounce multiplies the previous effect by **0.90**: initial hit 100%, bounce 1 90%, bounce 2 81%, bounce 3 72.9% and so on. **Revisiting previous recipients is allowed**. Within eligible bounce range, **wounded allies ALWAYS come before enemies**, and among wounded allies, prioritize the **lowest current HP percentage** (e.g., 20/100 = 20% before 100/1000 = 10%? No: 10% is lower and is selected first). **OPEN**: bounce search range, tie-breaking, whether full-HP allies are eligible as fallback after enemies and anti-self looping other than rank-limited bounce count.
- The whole chain is **one spell cast for Grace generation**, not one per bounce.

### Light Burst: following three-pulse effect (PRIEST-014)
- Standard cast: instant heal/damage circle centered on the **selected character**.
- Grace cast: three pulses **at initial impact, +1 second, +2 seconds**, recomputing the AoE center around the **original moving target** at each pulse.
- A unit may be hit/healed by **all three** pulses. **Only the FIRST pulse can generate Grace**, awarding the usual **1 or 2 based on its qualifying outcome**. Pulses 2 and 3 are effects of Grace and **generate none**. Do not grant multiple extra Grace from multiple simultaneous targets.
- **OPEN:** target death/despawn, target line of sight after initial cast and cases with both effective healing and damaging in the same first pulse.

### Ice Aegis: elemental shield efficiency (PRIEST-011/024/030/034)
- **Ice** is chosen, and Aegis can shield **self or ally**.
- For incoming damage after shared mitigation: **one point of Ice damage prevented consumes 0.5 shield HP**, while **one point of non-Ice damage prevented consumes 1.5 shield HP**.
- Example: an Aegis with 100 nominal shield HP could prevent 200 Ice damage or approximately 66.7 other damage (illustrative example only; **100 capacity is not a chosen actual shield value**).
- The shared framework in `docs/STATS_AND_DAMAGE.md` allows per-element efficiency, damage filters, absorb priorities, shield duration and overflow. These coefficients are **specific to Aegis**, not a universal global elemental-shield rule.
- **CONFIRMED PRIEST-024/030/034:** Grace-empowered **Ice Aegis** creates a **continuous Ice-element damage aura** around the shield recipient, scaling with **Priest SPR**. **Tick interval is exactly 0.1 second (10 pulses per second)**, with **low damage on each tick**, while the shield is active. When the shield expires or is broken, damage aura ends. **OPEN**: tick potency coefficient, area radius, potential multiple-aura stacking and target cap. Provisional implementation recommendation (not user decision): calculate/aggregate ticks efficiently for server-side multiplayer/combat events; never award one Grace point per aura tick.

### Wild Growth: dual Nature periodic (PRIEST-015/034/037)
- Both enemy **Nature DoT** and friendly/self **Nature HoT** use the **same baseline magnitude per tick, period and duration**.
- Standard resistance/healing rules may make *final* damage and healing differ even when base amounts match.
- **CONFIRMED PRIEST-037:** normal duration **8s**, Grace HoT **16s**, enemy DoT still **8s**. Both HoT/DoT **tick every 0.25s**, using frequent small amounts: normally **32 ticks** and Grace HoT **64 ticks**, before any optional time-zero tick.
- **IMPORTANT CORRECTION:** Wild Growth's cooldown must be **greater than or equal to its normal eight-second duration (>=8s)** including maximum skill rank. It **does not** need to outlast the 16s empowered HoT; overlapping the Grace HoT with a later cast is permitted.
- **Confirmed intent:** Grace-empowered enemy Wild Growth slow scales with spell rank to **around 50% at maximum rank**. Earlier `10+5×rank %` wording was illustrative. For a **proposed 18-rank cap** a **provisional smooth formula** is `10% + (40% × (rank−1)/(18−1))`, reaching 10% at rank 1 and 50% at rank 18. No fixed rank cap/slow formula approved yet.

### Revive — confirmed low-tier resurrection (PRIEST-022/026/027)

- **Skill ranks:** **ONE skill rank only**. Starts rank 0 and may first be purchased at **Priest character level 20** for the normal skill-point cost; not automatically learned.
- **Eligible targets:** dead **party member**, before they self-respawn; explicitly works **DURING combat**. Other targets (NPCs, non-party players, etc.), range, line of sight, and target death/disappearance during cast are **OPEN**.
- **Cast time:** **8 seconds normally**, reduced to **4 seconds** if the Priest manually activated full **Grace** before the cast. Grace consumption follows the normal confirmed empowerment rule (full meter for empowered successful spell).
- **Mana eligibility:** initiating Revive requires **current Mana STRICTLY > 50% of max Mana** (50% does not suffice). Do not conflate '50% Mana' with '50 Mana'.
- **Mana cost:** Revive **consumes ALL currently held Mana upon successful cast completion**. A successfully cast resurrection should leave the Priest at **0 Mana**. The Priest user specifically requires **no Mana lost on interruption**. It remains **OPEN** whether dropping below 50% Mana during the cast invalidates completion or only the cast-start gate applies.
- **Cooldown:** **60 seconds** on completed Revive; if the cast is interrupted, **cooldown is refunded/removed** so the ability can be attempted again. Starting the cooldown on successful completion is an acceptable implementation approach but its precise timer timing hasn't been explicitly approved. No cooldown reduction with ranks (single-rank skill).
- **CONFIRMED revived resources:** the revived player returns with **15% of maximum HP and 15% of maximum Mana** (the latter specifically approved in Round 8), independent of the separate −10% per-stack sickness to primary stats. Other resource meters remain open.
- **CONFIRMED PRIEST-038:** global Resurrection Sickness has **individual timers for every stack, each lasting exactly 5 minutes**; encounter end does NOT clear them. Each stack reduces all **five primary stats by 10%**. Maximum stacking/attribute floor and whether sickness persists across reloads remain OPEN.
- **OPEN:** whether Recurring Revive cast success awards normal 1 Grace point, what happens when full Grace was armed and Revive is interrupted, grave/corpse visibility, within-combat corpse eligibility, and persistence of Resurrection Sickness. Grace must not bypass full Mana consumption.


### Grace: generation, rank milestones and reset (PRIEST-012/014)
- **2 points** from a successfully cast **heal that does not overheal**; **1 point** from all other **successful healing, shield and damage casts**. Round 1's preference for better healing generation is therefore satisfied.
- **OPEN:** whether any wasted overheal reduces 2 to 1 or any actual HP restored earns 2; full-health no-op heal being "successful"; whether shields count on cast or only effective protection. Don't make unconfirmed anti-spam policies into locked decisions.
- Full Grace requires **10 / 9 / 8 / 7 / 6 points** at **Grace skill ranks 1 / 2 / 3 / 4 / 5** respectively. **Five ranks** are confirmed, and each becomes eligible at Priest character levels **5 / 10 / 15 / 20 / 25**. The number **6** refers to the **Grace charge threshold at rank 5**, NOT a sixth skill rank.
- Grace **does not decay** and **does not persist through save/game loads**; start a fresh load at **zero**.
- Grace is a **manual active power** on the same skill as its passive generation. It is only usable when full, empowers the **next successful spell**, and the empowered cast consumes **all accumulated Grace**. All real spells still cost normal Mana.
- For empowered Light Burst, **only the original/first pulse generates Grace**. Repeated pulses may still damage/heal normally; they do not create more Grace. Further multi-hit/tick exploitation safeguards need explicit design before coding.
- **OPEN:** Grace rank progression and intermediate thresholds, how long armed state lasts, interruption/target cancel handling, self-heal and instant shield event generation, any Mana cost for pressing the Grace toggle.

### Rank-up and balance governance — Round 7

- **PRIEST-031 CONFIRMED:** normal actives Holy Light, Light Burst, Ice Aegis and Wild Growth have **NO rank purchase gates every N levels**. Once learned (rank 1 purchasable at level 1), they may receive another point **at every Priest character level**, if the player has a point and has not reached that skill's maximum rank. **Maximum rank still OPEN.** Grace remains specially gated and Revive one-rank-only.
- **PRIEST-032 CONFIRMED delegation:** user entrusted the designer with provisional **Mana pool, Mana regen, spell costs, cooldowns, strength and damage/heal scaling**. Balance to be tested during actual gameplay; no unexplained hard locks on live values.
- **Provisional playtest pacing (not approved fixed balance):** Holy Light ~12–14s cooldown, Light Burst ~20–22s, Ice Aegis ~18–22s, Wild Growth ~12–14s at rank 1, reducing slightly per purchased rank. **Wild Growth's minimum cooldown at any rank is its normal 8s duration, NOT Grace's 16s duration.** Balance Mana regen/costs with class sustainability.
- **Design principle:** Priest can fight solo, but mana economy must cap indefinite self-healing and make long encounter choices meaningful. Higher ranks are stronger and cost more Mana; resource regeneration should not trivialize this.

### Staff attacks, cooldowns and Mana balance (PRIEST-021/023)

- **CONFIRMED:** Priest can wield **staffs**, plus gear whose eligibility explicitly allows **all classes**. It does **not** freely wield every other class weapon.
- **CONFIRMED:** all classes' ordinary basic attacks deal **normal, non-elemental** damage; Priest's ordinary staff autoattack generates **zero Grace**. Other ordinary damage calculations and attack scaling follow shared rules (exact Priest basic attack stat coefficient remains open).
- **CONFIRMED PRIEST-021/028:** four regular spells begin with **relatively long cooldowns**. Each **rank invested into that same spell** produces a modest cooldown reduction. Exact starting numbers and reductions remain **BALANCE OPEN**. **Revive is a single-rank spell with its own 60s cooldown, not subject to a multi-rank cooldown reduction**.
- **CONFIRMED PRIEST-028:** normal spell Mana costs **increase when upgrading that spell's rank**, not merely on character level-up. Exact values and any per-rank multipliers are **BALANCE OPEN**, as are relative pools and regen.
- **CONFIRMED gameplay direction:** **Mana management**, rather than a pure cooldown ceiling or limitless healing throughput, should be the main long-fight healer constraint; a weak enemy should not be heal-tanked indefinitely for **20 minutes** merely through bottomless self-heals. Tuned mana pool, regen, spell costs, resource restoration and throughput must make this meaningful while keeping early solo play feasible.
- Grace-powered healing/burst chains remain subject to their ordinary Mana cost. Staff attacks never provide Grace and are not themselves elemental.

### Possible Tier III branches — ideas only

A future **Nature/Druid HoT specialization** and **Light/Hierophant/Prophet specialization** have been discussed, but no Tier III branches or class IDs are approved yet.

## Priest advancement quest — CONFIRMED instanced escort (PRIEST-035/039/040)

**This replaces the old wounded-lumberworkers “Hands of Mercy” proposal.** Quest title remains unchosen.

- **Who:** eligible **level-30+ Magically Touched Novice**, a **defenseless inspector** requiring healing, plus **two NPC guards who actively fight**. The player's Tier II Priest skill kit is not yet available in the instance.
- **Where:** private instance resembling the actual **Trimery → Cornberg** road/landscape, without the Alpha Wolf. **No other human players can participate**, even when the player is normally grouped. The two friendly NPC guards remain; they should fight and can die.
- **Encounters:** **several smaller attacks by wolves and slimes** on the road. No Alpha Wolf and **no slime dungeon boss**. The player can remove threats with Tier I offensive magic but **a fully healing-focused player must be able to finish by casting ONLY Mend**. That is a hard quest encounter-balance acceptance criterion: guards' damage/aggro and inspector incoming damage/Mend sustain must support the heal-only path, without requiring damaging spells.
- **Fail:** **inspector dies → mission fails → player teleported OUT of instance back to Trimery**. Guard death, including both guards, **does not automatically fail**. Details of player death, reentry/retry and the NPC situation if both guards fall remain OPEN.
- **Success:** inspector reaches **Cornberg alive** → **permanently persist the escort objective as completed**. This is **not** a Priest unlock/automatic class fork. The player independently returns to the **Trimery healer** and explicitly **turns in** the completed quest, at which point Tier II **Priest starts at level 1**, preserving the Tier I parent manifestation under global progression rules. Do not force the escort again after the step is credited, even if the user later changes scene/reloads.
- **OPEN:** detailed route and scene spawn points, escort AI pacing, number/wave tuning, NPC dialogue/identity, how to replay after inspector death, what happens if the player dies/leaves mid-instance, and quest naming.

## Round 1 — answers recorded on 2026-10-17

- **PRIEST-001 — CONFIRMED:** mixed general-purpose healing at Tier II, offering a taste of different healing techniques; future **Tier III** Priest branches can specialize later.
- **PRIEST-002 — CONFIRMED:** the four spell actives are **Holy Light** (heal ally/damage enemy single target), **Light Burst** (targeted AoE heal/damage), **Aegis** (Light-element protective shield) and a **single-target DoT-on-enemy / HoT-on-ally spell** (name not chosen). **Grace** is the fifth class skill and combines passive + active functionality. No dash selected.
- **PRIEST-003 — CONFIRMED:** both damage and healing generate Grace, healing slightly more; formula and decay are **OPEN**.
- **PRIEST-004 — CONFIRMED:** manually **activate Grace** to empower the next spell; effect/cost amount and timing **OPEN**.
- **PRIEST-005 — CONFIRMED:** **SPR is the main scaling attribute for both damage and healing**. User did not separately confirm staff equipment restrictions or the autoattack element in this answer; keep them **OPEN** rather than assuming.

## Round 2 — answers recorded on 2026-10-18

- **PRIEST-006 — CONFIRMED:** initially longer Holy Light cooldown and strong scaling (balance targets, not numerical locks). Grace adds a **chain bounce** as its special effect.
- **PRIEST-007 — CONFIRMED:** Light Burst AoE centers on a **targeted character**; normal cast is **one instant burst**, Grace version is **three bursts one second apart**.
- **PRIEST-008 — OPEN element; CONFIRMED directional preference:** Aegis should use **Light, Nature or Ice**, with elemental matching shield efficiency. Shared shield framework supports per-element rates, but **1:1 matching vs 2–3:1 mismatched** are *illustrative possibilities*, not existing universal locked values. Grace's Aegis effect not yet specified.
- **PRIEST-009 — PREFERRED, NOT FINAL:** Nature-themed dual DoT/HoT fits Tier II Priest and possible future Nature/Druid lineage; final name and element still open.
- **PRIEST-010 — CONFIRMED:** one Grace point per successful spell; about **ten successful spells** to fill at Grace rank 1, decreasing threshold with higher ranks; **no decay**; **reset Grace to zero on load**; only **fully charged** Grace can be activated; successful next empowered cast **consumes all Grace**. Conflict with earlier higher gain from healing still open.

## Round 3 — user decisions (2026-10-18)

- **PRIEST-011 CONFIRMED:** Ice Aegis self/ally shield; 0.5 shield HP per blocked Ice damage and 1.5 per blocked other damage. Grace shielded target deals small nearby AoE damage; precise behavior open.
- **PRIEST-012 CONFIRMED:** first Grace rank requires 10 points, max rank requires 6. Non-overhealing successful heals give 2 Grace, all other successful heals/shields/damage spells give 1.
- **PRIEST-013 CONFIRMED:** Grace Holy Light bounces once per Holy Light skill rank, can bounce between friends and foes with correct heal/damage per target, each successive bounce 10% less effective.
- **PRIEST-014 CONFIRMED:** Grace Light Burst's three bursts follow the selected character; units can be struck/treated by every pulse. Only the first pulse earns a single 1- or 2-point Grace grant.
- **PRIEST-015 CONFIRMED:** periodic spell called **Wild Growth**, **Nature**, identical base tick values/interval/duration for HoT or DoT; Grace doubles HoT duration or adds slow to DoT.

## Historical Round 4 — original five questions (answered except Q19; see authoritative updated answers below)

**PRIEST-016 — Stats and stat growth.** We confirmed that **SPR is Priest's main stat for both healing and damage**. What should Priest's **starting VIT / STR / AGI / INT / SPR** and **growth per level** be? For comparison, Magically Touched Novice currently starts at **10 / 5 / 5 / 13 / 13** with growth **+1 / +0.5 / +0.5 / +2 / +2**; the Mage Tier II prototype starts **11 / 5 / 8 / 18 / 9**, with growth **+1.5 / +0.5 / +0.75 / +3 / +1.5**. Approximate priorities are fine if you prefer us to balance numbers later.

**PRIEST-017 — Grace rank thresholds and overhealing.** Do you like **five ranks** requiring **10 / 9 / 8 / 7 / 6 Grace**, or another progression? To earn **2 Grace**, does a heal need **zero overheal at all**, or is restoring **some actual HP** sufficient? Does casting on a full-HP ally still award the baseline 1 Grace?

**PRIEST-018 — Skill unlocks and ranks.** Which spells/passives are available from **Priest level 1**, and which unlock later across Tier II levels 1–60? Should rank-1 starting skills be free and later ranks earned via level-gated points like Greatblade/Mage? Any preferred rank caps?

**PRIEST-019 — Mana economy and cooldowns.** Should Priest have a **large Mana pool but expensive casts**, or **moderate spell costs with faster natural regeneration**? Should Holy Light have the **longer, stronger heal cooldown**, Light Burst the **longest AoE cooldown**, Aegis an intermediate defensive cooldown, and Wild Growth a more maintainable periodic cast? We can leave exact numerical balance provisional.

**PRIEST-020 — Staff/basic attack and Grace.** Should Priest commit to a **staff**, with an **SPR-scaling Light basic attack** used between spell cooldowns? Should **basic attacks generate Grace**, or should only the four actual spell casts do that?

## Round 4 corrections and preliminary Priest stat package — authoritative for later implementation

**PRIEST-016: User confirmed the stat philosophy and delegated numerical values to the designer.** Priest prioritizes SPR for **both healing and damage**, has moderate VIT, some INT and lower STR/AGI. A higher class tier must gain more **total** base attributes and per-level growth than lower tiers. The following is a **provisional designer-selected balance recommendation**, not user-specified exact numbers:

| Stat | Tier I Magically Touched base / growth | Tier II Mage prototype base / growth | **Tier II Priest proposed base / growth** |
| --- | --- | --- | --- |
| VIT | 10 / +1 | 11 / +1.5 | **12 / +1.5** |
| STR | 5 / +0.5 | 5 / +0.5 | **5 / +0.5** |
| AGI | 5 / +0.5 | 8 / +0.75 | **6 / +0.75** |
| INT | 13 / +2 | 18 / +3 | **11 / +1.5** |
| SPR | 13 / +2 | 9 / +1.5 | **18 / +3** |
| **Total** | **46 / +6** | **51 / +7.25** | **52 / +7.25** |

Fractional growth should be applied as whole-number increments over levels in the game. Base HP/defenses/Mana and coefficients are still to be tuned.

**PRIEST-017: Grace ranks and overheal — CONFIRMED.** Grace has **five ranks**. A full meter requires **10, 9, 8, 7, 6 Grace points** at ranks **1–5**, respectively; at rank **0**, Grace is not yet learned. A successful heal with **exactly zero overheal** awards **2 Grace**. **Every other successful heal** (including a full-HP heal and partly overhealing) awards **1 Grace**, as does a successful damage or shield spell. This supersedes older general 1-per-cast descriptions. The user's choice to grant a point for full-health healing is intentional; do not silently nullify it with older anti-overheal proposals. Chain jumps/secondary Light Burst pulses still do not generate extra Grace, per previous decisions.

**PRIEST-018: GLOBAL RULE, not just Priest — CONFIRMED.** **All class skills begin at rank 0** for a freshly created manifestation, with **exactly 1 spendable skill point at character level 1** and +1 on later levels by the existing advancement economy. **No free starting rank-1 spells or passives.** Skill availability gates and individual rank progression can still vary by class. Existing Mage/Greatblade prototypes document free initial ranks; **this is a mismatch to resolve separately during implementation**, not authorization to edit gameplay in this design chat. Priest skill availability/unlock levels still **OPEN**.

**PRIEST-019: NOT ANSWERED.** Mana/cooldown pacing remains completely **OPEN**.

**PRIEST-020: GLOBAL RULE, not just Priest — CONFIRMED.** **Every class's ordinary basic/autoattack does normal, non-elemental damage**; there is no inherent Light-element autoattack for Priest. Class-dependent attack reach, visual, Physical/Magical channel, stat scaling and specifically authored elemental weapon enhancements are separate decisions. **Priest staff restriction and whether its autoattacks award Grace remain OPEN.**

> The most recent user corrections in this section **supersede any contradictory text above**, including the older Grace overview, earlier anti-overheal *recommendations*, and earlier free-starting-rank suggestions. No Unity gameplay code changed.

## Round 4 — answers recorded on 2026-10-18

- **PRIEST-016:** SPR-heavy stats with tier-appropriate total base/growth; user delegated numerical distribution. Designer proposed **12/5/6/11/18** and per-level **+1.5/+0.5/+0.75/+1.5/+3** (VIT/STR/AGI/INT/SPR), for playtesting.
- **PRIEST-017:** Exactly 5 Grace ranks, thresholds **10→9→8→7→6**. Zero-overheal heal = 2 Grace; any other successful heal, including full-HP healing, = 1; damage/shield = 1.
- **PRIEST-018:** Global correction: every skill starts **rank 0**, with 1 point at class level 1, no free starter ranks. Actual Priest skill unlock timeline unanswered.
- **PRIEST-019:** Mana/cooldown question unanswered.
- **PRIEST-020:** Global correction: all basic attacks are **normal/non-elemental**. Staff equipment and basic-attack Grace generation not answered.

## Round 5 — answers recorded 2026-10-18

- **PRIEST-021 — CONFIRMED design direction:** longer initial spell cooldowns than earlier suggestions; cooldowns shorten somewhat with skill-point/rank investment. Mana costs initially low but increase with level/progression (exact level axis **OPEN**). Mana management is **the main intended balancing lever for healers**, especially to prevent indefinite heal-stalling of fights. No actual Mana pool/regeneration/cost or cooldown values approved.
- **PRIEST-022 — CONFIRMED:** Grace rank 1 becomes eligible at **Priest level 5**, and can upgrade every five Priest levels: **ranks 1–5 at levels 5/10/15/20/25**. Also user **added low-tier Revive**: **8-second cast**, only castable while Priest's Mana is **> 50%**, and it **consumes all current Mana**. Other ability unlocks, Revive's ranks and resurrection effects **OPEN**.
- **PRIEST-023 — CONFIRMED:** Priest uses **staffs only**, except items with an **all-classes equipment exception**. Ordinary basic attacks give **no Grace** and, per earlier global correction, use **normal/non-elemental damage**.
- **PRIEST-024 — CONFIRMED:** Grace-empowered Aegis makes the shield recipient deal **continuous nearby aura damage** scaling with Priest **SPR**. Aura's element, radius, frequency and lifetime not yet approved.
- **PRIEST-025 — CONFIRMED:** Holy Light bounces use **multiplicative ×0.9** loss per successive bounce (100/90/81/72.9%...). Can **bounce back to previous recipients**, and chooses targets **favoring lowest-health allies**. Exact health comparison/chain search/fallback OPEN.

## Round 6 — answers recorded 2026-10-18

- **PRIEST-026 CONFIRMED:** Revive is **single-rank** and first purchaseable at **Priest level 20**. With Grace, Revive's **8-second cast time is halved to 4 seconds**; no ranks beyond 1.
- **PRIEST-027 CONFIRMED (with PRIEST-038 completion):** Revive works during combat on dead party members, restores **15% of target max HP** and **15% max Mana**, applies sickness with **−10% of each five primary attributes per independently-timed 5-minute stack**, and has a **60-second cooldown**. Interrupted casts spend no Mana and refund the cooldown. Completed casts spend all Priest's held Mana.
- **PRIEST-028 CONFIRMED:** the four ordinary Priest spells' **Mana costs increase per purchased spell rank**, and **cooldowns slightly decrease per rank of that same spell**; specific tuning numbers not chosen.
- **PRIEST-029 CONFIRMED:** **all four normal spells are learnable at Priest level 1** (rank 0 initially, using normal points). Grace only at Priest 5/10/15/20/25, Revive only at Priest 20. Number of ranks and rank-up gates for the four normal spells remain unanswered.
- **PRIEST-030 CONFIRMED:** Grace-empowered Ice Aegis applies a **continuous Ice damage aura that ticks while the shield remains**, scaling with SPR. Holy Light's Grace chains prioritize **wounded allies with LOWEST HP PERCENTAGE** over enemy targets within reach.

## Round 7 — answers recorded 2026-10-19

- **PRIEST-031 — CONFIRMED:** Holy Light, Light Burst, Ice Aegis, Wild Growth may each be upgraded by investing a skill point **at every character level**, unlike Grace's five-level gates or one-rank Revive. The normal skills' **maximum rank is still OPEN**; do not lock a five-rank cap or periodic rank level requirements.
- **PRIEST-032 — CONFIRMED delegation:** designer may set provisional starting cooldowns, Mana costs, pool/regen and scaling, subject to gameplay tuning and the user's Mana-limited sustainability goal. Numerical durations and actual spell strength must be considered together; do not claim untested balance is final.
- **PRIEST-033 — DURATION DIRECTION, other details OPEN:** Resurrection Sickness should last **about 5 minutes OR until the current encounter ends**. Whether this means *whichever occurs first* (recommended) needs final clarification. User did **not** explicitly approve the earlier −10% attributes per stack nor restoration of the revived player's Mana/other resources.
- **PRIEST-034 — HISTORICAL, SUPERSEDED IN PRIEST-037:** official name Ice Aegis; its Grace aura ticks Ice damage every **0.1s**. Original Wild Growth note had 8s/16s HoT and high tick rate yet incorrectly demanded cooldown >16s; **PRIEST-037 corrected the cooldown to >= NORMAL 8s, and fixed the tick rate at 0.25s**. The slow-rank curve remains a designer proposal reaching ~50% at max rank.
- **PRIEST-035 — CONFIRMED:** Priest advancement quest is a **private instanced escort from Trimery to Cornberg** of **one inspector and two guards**; controlled by the **Tier I Magically Touched character**, with healing/protection and the option of offensive spellcasting. Environment resembles the normal terrain/road but omits the Alpha Wolf. **No other human players** assist. This **replaces** the earlier lumberworker rescue quest proposal. Combat choreography and failure/retry details still OPEN.

## Round 8 — user decisions recorded 2026-10-19

- **PRIEST-036 — CONFIRMED philosophy, OPEN exact rank cap:** user wants almost all level-earned skill points spendable at **next class advancement milestone + 33% additional levels**. Interpret Tier II level-60 threshold plus one-third = **~level 80**, one point per level = 80 skill points. **DESIGN PROPOSAL**: four regular skills at 18 ranks each (72), Grace 5, Revive 1 → **78 points total**. Remaining 2 unspent at level 80. User has not yet approved the exact caps or level interpretation.
- **PRIEST-037 — CONFIRMED:** Wild Growth tick interval **0.25s**. **Cooldown >= normal 8s duration only**, even at max rank; prior incorrect >=16s rule is **superseded**. Grace-enhanced enemy DoT slow should increase with Wild Growth skill rank to around **50% at maximum rank**, with original `10 + 5×rank` only indicative and adjusted to whatever max rank is finalized.
- **PRIEST-038 — CONFIRMED:** Resurrection Sickness has **fixed 5-minute duration per independently tracked stack**, not removed when the encounter ends; every stack incurs **−10% to each primary attribute** (VIT/STR/AGI/INT/SPR). This particular Revive restores **15% of target max Mana** (as well as the previously locked 15% max HP).
- **PRIEST-039 — CONFIRMED:** guards fight, inspector defenseless and heals required. Several small enemy groups of **wolves and slimes**, without Alpha Wolf or slime dungeon boss. **A Mend-only max-rank Tier I Magically Touched player can win** by keeping inspector/guards alive while guards deal damage; offensive magic optional.
- **PRIEST-040 — CONFIRMED:** inspector death causes failure, player teleported from instance to Trimery. Guards may die. Reaching Cornberg credits escort step **permanently**; return independently to **Trimery healer** for final Priest advancement (no auto-unlock at Cornberg).

## Round 9 — five questions (awaiting answers)

**PRIEST-041 — Ordinary skill max-rank budget.** The Priest can advance to Tier III from **level 60**, so if I interpret your “+33% levels” as **level 80**, can we set the four regular skills to **18 ranks each**, plus Grace's 5 and Revive's 1? That's **78 possible investments out of 80 level-80 skill points**, leaving just two spare. Is level 80/18 ranks your intended progression target?

**PRIEST-042 — Wild Growth slow and cooldown curve.** Given 18 proposed ranks, I suggest Grace-empowered hostile Wild Growth slows by **10% at rank 1 to 50% at rank 18** using `10% + 40% × (rank−1)/17`. I would start Wild Growth's cooldown around **12 seconds at rank 1**, falling to **8 seconds by the last rank** (never below its normal 8-second duration). Does that sound good?

**PRIEST-043 — Escort movement.** Should the inspector **walk on a scripted path** from Trimery to Cornberg and stop for attacks, or **follow the player** (with guards) and let the player lead them? Should the group pause automatically when enemies approach?

**PRIEST-044 — Escort defeat and retries.** If the **player** dies in this instance, does the whole quest fail and send them back to Trimery, just like the inspector dying? Should the player be allowed **unlimited free attempts** by talking to the Trimery healer again?

**PRIEST-045 — Inspector narrative.** Who is the inspector and **why travel between Trimery and Cornberg**? Do you want a mundane mission (evaluating the road and threat to travelers), a political investigation, or something more magical? Should the healer frame it as an explicit **Priest advancement test**, or reveal that after you succeed?

## Remaining design work (not yet approved)

- Full active/passive roster and rank unlock schedule through Tier II level 60; exact skill effects, targeting, geometry, queues, interruptions, Mana and Grace costs, cooldowns, strengths and tooltip breakdowns.
- Stat/defense/HP/Mana/Grace baseline and per-level growth; solo damage pacing versus Mage/Greatblade; class equipment allowances.
- Explicit stacking and shield precedence, target eligibility, heal caps/overhealing, party roles, group AOE heal balance, boss damage and resurrection eligibility.
- Design and approval of the healer's actual advancement quest, tutorialization and art/animation style; keep existing free-healing NPC service functioning.
- Integration tests: class fork/save, class swapping, Mana+Grace persistence, solo questing, real 1–4 player triage, basic attack, canceled casts, overheal/exploit safeguards and long boss fights.

## Decision log

| Date | ID | Decision | State |
| --- | --- | --- | --- |
| 2026-10-17 | Existing Tier II direction | Priest, staff, Mana+Grace; active Mana costs, Grace enhances offense and healing; good solo DPS; Trimery healer mentor | **PREVIOUSLY AGREED**, details open |
| 2026-10-17 | PRIEST-001 | Mixed all-round Tier II healer; Tier III can specialize | **CONFIRMED** |
| 2026-10-17 | PRIEST-002 | Holy Light dual target, Light Burst targeted AoE, Aegis shield, dual target DoT/HoT, Grace passive/active | **CONFIRMED**, mechanical specifics open |
| 2026-10-17 | PRIEST-003 | Both heals and damage generate Grace; healing slightly more | **CONFIRMED**, gain/decay open |
| 2026-10-17 | PRIEST-004 | Grace manually activated to empower next spell | **CONFIRMED**, cost/bonus open |
| 2026-10-17 | PRIEST-005 | SPR primary for healing AND damage | **CONFIRMED**, weapon/basic attack not answered |
| 2026-10-18 | PRIEST-006 | Holy Light starts long-CD/strong SPR; Grace causes chain bounces | **CONFIRMED**, bounce rules open |
| 2026-10-18 | PRIEST-007 | Light Burst centered on chosen character; instant normally, three bursts 1s apart with Grace | **CONFIRMED**, pulse movement/targets open |
| 2026-10-18 | PRIEST-008 | Aegis element reopened to Light/Nature/Ice; elemental shield efficiency preference | **OPEN element/ratios**; system support verified |
| 2026-10-18 | PRIEST-009 | Nature-themed single-target DoT/HoT suggested; Druid/Nature and Hierophant/Prophet/Light later ideas | **PREFERRED**, not final |
| 2026-10-18 | PRIEST-010 | 1 Grace/successful spell; ~10 casts initially, fewer at higher ranks; no decay/persistence; full meter/consumes all | **CONFIRMED**, rank thresholds + healing bonus conflict open |
| 2026-10-18 | PRIEST-011 | Ice Aegis 0.5 shield HP/Ice damage, 1.5 shield HP/non-Ice damage; self/ally; Grace nearby damage | **CONFIRMED**, AoE details open |
| 2026-10-18 | PRIEST-012 | Grace requires 10 at rank 1, 6 at max; 2 for non-overhealing heals, 1 other successful spells | **CONFIRMED**, rank/overheal details open |
| 2026-10-18 | PRIEST-013 | Holy Light's Grace bounce count equals ability rank; mixed allies/enemies; 10% less each bounce | **CONFIRMED**, exact falloff open |
| 2026-10-18 | PRIEST-014 | Triple Light Burst follows target; same unit affected thrice; original pulse alone generates 1 or 2 Grace | **CONFIRMED** |
| 2026-10-18 | PRIEST-015 | Wild Growth Nature DoT/HoT same base duration/interval/potency; Grace 2× HoT duration or slows DoT | **CONFIRMED**, numbers open |
| 2026-10-18 | PRIEST-016 | SPR-heavy stats; suggested Tier II stat baseline/growth documented | **CONFIRMED direction; exact values delegated, provisional** |
| 2026-10-18 | PRIEST-017 | Five Grace ranks require 10/9/8/7/6; zero-overheal heal yields 2, full-HP heal yields 1 | **CONFIRMED** |
| 2026-10-18 | PRIEST-018 | All class skills start rank 0, 1 class point at level 1; no free ranks | **CONFIRMED, GLOBAL** |
| 2026-10-18 | PRIEST-019 | Mana economy and cooldown pacing unanswered | **OPEN** |
| 2026-10-18 | PRIEST-020 | All classes' autoattacks normal/non-elemental damage | **CONFIRMED, GLOBAL** |
| 2026-10-18 | PRIEST-021 | Start longer cooldowns, reduce per skill investment; Mana costs increase with progression; long-fight healing constrained by Mana | **CONFIRMED principles**, numeric mechanics open |
| 2026-10-18 | PRIEST-022 | Grace available r1 at level 5, ranks every 5 levels; new 8-sec Revive with >50% Mana requirement and full current-Mana cost | **CONFIRMED**, Revive unlock/edge cases open |
| 2026-10-18 | PRIEST-023 | Staff weapon restriction except all-classes gear; autoattacks generate no Grace and remain non-elemental | **CONFIRMED** |
| 2026-10-18 | PRIEST-024 | Grace-empowered Aegis creates continuous nearby-damage aura scaling with SPR | **CONFIRMED**, element/tick details open |
| 2026-10-18 | PRIEST-025 | Holy Light chain uses multiplicative 0.9 per bounce, can revisit prior recipients, prioritizes lowest-health allies | **CONFIRMED**, HP comparison details open |
| 2026-10-18 | PRIEST-026 | One-rank Revive available level 20; Grace halves cast time 8s→4s | **CONFIRMED** |
| 2026-10-18 | PRIEST-027 | Combat Revive on dead party member, 15% target HP, Resurrection Sickness, 60s CD; interruption spends no Mana and refunds cooldown | **CONFIRMED**, sickness specifics open |
| 2026-10-18 | PRIEST-028 | Mana costs increase per same-spell rank, cooldowns decrease per same-spell rank | **CONFIRMED**, numeric values open |
| 2026-10-18 | PRIEST-029 | All four normal spells eligible for rank 1 from Priest level 1 | **CONFIRMED**, cap/rank gates open |
| 2026-10-18 | PRIEST-030 | Aegis's Grace aura deals periodic Ice damage while shield active; HL prioritize injured allies by lowest HP percentage over enemies | **CONFIRMED**, tick/range open |
| 2026-10-19 | PRIEST-031 | Regular spells rank up at any Priest level if point available; max ranks unchosen | **CONFIRMED**, caps OPEN |
| 2026-10-19 | PRIEST-032 | Designer may choose provisional numeric balances; tune in playtest | **CONFIRMED delegation** |
| 2026-10-19 | PRIEST-033 | Resurrection Sickness lasts ~5 minutes or until encounter ends | **PROVISIONAL duration rule**, attributes/resource return OPEN |
| 2026-10-19 | PRIEST-034 | Ice Aegis name + 0.1s aura ticks, Wild Growth 8s/16s HoT duration; original >16s cooldown suggestion corrected by PRIEST-037 | **CONFIRMED**, cooldown historical value superseded |
| 2026-10-19 | PRIEST-035 | Priest advancement is solo-human instance escort: Magically Touched Novice guides inspector + 2 guards Trimery to Cornberg, no Alpha Wolf | **CONFIRMED direction**, encounters/retries OPEN |
| 2026-10-19 | PRIEST-036 | Almost all skill points spendable by next-tier milestone +33%; 18 ranks/regular spell (78/80 at lv80) proposed | **CONFIRMED principle**, cap **PROPOSED** |
| 2026-10-19 | PRIEST-037 | Wild Growth 0.25s tick; CD >= normal 8s only; Grace DoT slow reaches ~50% at max rank | **CONFIRMED**, exact curve proposed |
| 2026-10-19 | PRIEST-038 | Global Resurrection Sickness: independent 5-min timers, −10% five attributes per stack; Priest Revive restores 15% max Mana plus 15% HP | **CONFIRMED** |
| 2026-10-19 | PRIEST-039 | Inspector defenseless, guards fight, wolves/slimes small encounters, no named bosses; max-Mend-only success possible | **CONFIRMED** |
| 2026-10-19 | PRIEST-040 | Inspector death fails to Trimery, guard deaths allowed; Cornberg objective persists; return to healer to become Priest | **CONFIRMED** |
| 2026-10-19 | PRIEST-041–PRIEST-045 | Round 9 five questions asked | **AWAITING USER** |


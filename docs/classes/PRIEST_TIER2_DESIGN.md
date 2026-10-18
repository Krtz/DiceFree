# DiceFree — Tier II Priest — Design Interview

**Status:** DESIGN INTERVIEW IN PROGRESS — Rounds 1–5 recorded; Round 6 awaiting answers. **Design only: do not implement from this document until user sign-off.**  
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
| Resources | **Mana + Grace**. Mana management should be Priest's **main sustained-healing limitation**; initial Mana costs are comparatively low and **grow with progression**; spells begin with comparatively long cooldowns that **decrease on rank investments**. Grace generates 2 on zero-overheal heals, 1 on other successful spell casts; rank 1–5 thresholds 10/9/8/7/6; no decay, reset on load. | Exact starting Mana/costs, what 'level' scales costs, regen, cooldown table, cancellation/recursion details remain open. |
| Without Grace | Priest retains usable damage/healing spells paid for with Mana; cannot require Grace for its basic function. | Specific ranks/ability gating open. |
| Skill system | **Five spell actives** (Holy Light, Light Burst, Aegis, Wild Growth, low-tier Revive) **plus Grace** (one passive with manual empowerment activation). All abilities begin at rank **0**; new Priest starts level 1 with exactly **one skill point**. **Grace rank 1 unlocks at Priest level 5**, upgrading every five Priest levels through rank 5 at level 25. No Tier II ultimate. | Rank/availability gates for other spells and Revive still open. |
| Mentor | The **Trimery healer** is intended as Priest trainer and future quest giver. | Current healer implementation is only a free HP-service placeholder. |
| Resurrection | **CONFIRMED PRIEST-022:** Priest has a **low-tier Revive** spell: **8-second cast**, can begin only if current Mana is **strictly greater than 50% of max Mana**, and **uses all the Mana Priest has at the time of payment**. | Mana payment timing, interruption consequences, revive HP, range/target rules, skill unlock level and Resurrection Sickness application/open numeric details still require decisions. |

## Present in the game — separate from Priest class

As of `integration/world1-foundation` commit `5a74e1112` (2026-10-17), the world has an ivory/blue **Trimery Priest placeholder** close to the village green that offers **free HP treatment to living players** via interaction. It does not refill Mana/Grace, resurrect, clear statuses, start/complete advancement, or unlock Priest. See `docs/TRIMERY_HEALER_2026_10_17.md` for scoped validation. This NPC is a future mentor candidate, not the implemented Priest class.

## Confirmed Tier II Priest kit — updated through Round 5

**Confirmed roster:** **five standard spells** (including the user-added low-tier **Revive**) plus **one manually activated/passive Grace skill**. No Tier II ultimate. Priest scales **both healing and damage from SPR**; the **basic attack is normal, non-elemental and gives no Grace**, and **staffs are its exclusive class weapon** except explicitly all-classes equipment. Start new Priest with all skills **rank 0**, one spendable point at level 1. Only **Grace's unlock table** is currently specified (level 5 onward).

| Ability | Element | CONFIRMED mechanics | OPEN balance/edge cases |
| --- | --- | --- | --- |
| **Holy Light** | Light | Single target: heal friend/self or damage foe. Strong scaling and long initial cooldown; **cooldown decreases with invested ranks**. Grace adds **one bounce per Holy Light rank**, visiting allies **and** enemies, can **revisit previous targets** and **prioritizes allies with lowest health**. Each bounce is **×0.9** effectiveness relative to the previous hit: 100% → 90% → 81% → 72.9%… | Magnitude, cooldown/rank reductions, bounce search radius, healing priority uses lowest absolute vs percentage HP (OPEN), ties and hostility fallback. |
| **Light Burst** | Light | Instant AoE centered on a **targeted character**, heals friends and damages enemies. With Grace: **three bursts one second apart** which **follow that same character as it moves**; the same friend/enemy may be affected by all three. | Radius, exact effects, target death/teleport and interrupt handling. |
| **Aegis** | **Ice** | Self/ally Ice shield: **0.5 shield HP per 1 Ice damage prevented**, **1.5 per 1 non-Ice damage prevented**. Grace causes the shield recipient to emit a **continuous damaging aura** that **scales with Priest SPR**. | Capacity, duration, aura tick interval/radius/element/magnitude, refresh/overlap, damage eligibility and Mana cost. |
| **Wild Growth** | **Nature** | Single-target **HoT on ally/self** or **DoT on enemy**, **identical base tick value, tick interval and duration**. With Grace: **double HoT duration** or **apply slow on DoT**. | Exact magnitude/cadence, slow value/duration, refresh/stack behavior, cooldown/cost. |
| **Revive** (working name; low-tier) | **Element OPEN / resurrection effect** | **NEW PRIEST-022:** resurrect a dead player through an **8-second cast**. Cast only if **current Mana > 50% of max Mana**; consume **all currently held Mana** for this resurrection. No Grace specialization, skill rank or unlock level has yet been established. | Exact range/target, recovery HP, Mana payment time, interrupted cast, Resurrection Sickness, cooldown, rank improvements and Grace compatibility. |
| **Grace** | Passive + manually activated skill | **Available for first purchase at Priest level 5**; upgradeable at **levels 10, 15, 20, 25** (ranks 1–5). Full meter requirements **10/9/8/7/6**. Successful heal with zero overheal gives **2 Grace**; any other successful spell gives **1**, except **ordinary autoattacks give 0**. No natural decay, starts 0 on load. Full Grace can be manually armed to empower next spell; successful empowerment consumes full Grace and still costs Mana. | Grace use on Revive, pending cast failures/interruptions and Grace-generation on Revive, other per-cast safeguards. |

### Holy Light: Grace bounce details (PRIEST-013)
- A Grace-empowered Holy Light makes **r additional bounces**, where **r is Holy Light's skill rank**, **not Priest character level**. Bounce destinations can be both friendly and hostile; each destination is healed or damaged according to allegiance.
- **CONFIRMED PRIEST-025:** each consecutive bounce multiplies the previous bounce's amount by **0.90**: initial hit 100%, bounce 1 90%, bounce 2 81%, bounce 3 72.9% and so on. **Revisiting previously affected recipients is allowed**. Target search **prioritizes allies with the lowest health**, then may reach enemies as appropriate. **OPEN**: lowest current HP vs lowest HP percentage, bounce search radius, ties, maximum times per target and how to avoid loops beyond the fixed rank-limited bounce count.
- The whole chain is **one spell cast for Grace generation**, not one per bounce.

### Light Burst: following three-pulse effect (PRIEST-014)
- Standard cast: instant heal/damage circle centered on the **selected character**.
- Grace cast: three pulses **at initial impact, +1 second, +2 seconds**, recomputing the AoE center around the **original moving target** at each pulse.
- A unit may be hit/healed by **all three** pulses. **Only the FIRST pulse can generate Grace**, awarding the usual **1 or 2 based on its qualifying outcome**. Pulses 2 and 3 are effects of Grace and **generate none**. Do not grant multiple extra Grace from multiple simultaneous targets.
- **OPEN:** target death/despawn, target line of sight after initial cast and cases with both effective healing and damaging in the same first pulse.

### Aegis: elemental shield efficiency (PRIEST-011)
- **Ice** is chosen, and Aegis can shield **self or ally**.
- For incoming damage after shared mitigation: **one point of Ice damage prevented consumes 0.5 shield HP**, while **one point of non-Ice damage prevented consumes 1.5 shield HP**.
- Example: an Aegis with 100 nominal shield HP could prevent 200 Ice damage or approximately 66.7 other damage (illustrative example only; **100 capacity is not a chosen actual shield value**).
- The shared framework in `docs/STATS_AND_DAMAGE.md` allows per-element efficiency, damage filters, absorb priorities, shield duration and overflow. These coefficients are **specific to Aegis**, not a universal global elemental-shield rule.
- **CONFIRMED PRIEST-024:** empowered Aegis adds a **continuous nearby-damage aura** carried by the **shielded recipient**; damage **scales with Priest SPR**. The aura's **element** (Ice would be coherent, but user did not explicitly choose it), radius, tick interval, damage coefficient and lifetime (whether exactly the shield duration) are **OPEN**. Damage from the aura is **not** an ordinary Priest basic attack and should not grant Grace per damage tick unless approved.

### Wild Growth: dual Nature periodic (PRIEST-015)
- Both enemy **Nature DoT** and friendly/self **Nature HoT** use the **same baseline magnitude per tick, period and duration**.
- Standard resistance/healing rules may make *final* damage and healing differ even when base amounts match.
- Grace on HoT: **double lifetime**, normal tick interval. Grace on DoT: original duration plus **movement slow**, whose amount and duration are still **OPEN**.

### Revive — newly confirmed low-tier Priest spell (PRIEST-022)

- **CONFIRMED:** Priest has a **low-tier resurrection spell** in addition to the earlier four active spells.
- **Cast time:** exactly **8 seconds** (not yet ranked/scaled).
- **Cast-eligibility test:** Priest must have **strictly more than 50% of max Mana remaining** to initiate casting (for example 51% qualifies, 50% does not).
- **Cost:** **all of Priest's currently held Mana**. When the payment occurs is not yet chosen; neither upfront reservation nor consumption on completed cast may be silently assumed.
- The previous cross-class combat framework already supports **repeatable resurrection with a long cast, high resource cost, and stacking Resurrection Sickness**. `docs/COMBAT.md` says sickness affects the five main stats with a **provisional ~10% reduction per stack**; duration/stack cap are still unconfirmed. Do not silently fix those numbers into Priest.
- **OPEN:** when and how Revive becomes purchaseable starting from rank 0, whether it has ranks and what those ranks change, targeting dead allies versus yourself, range/line of sight, resurrected HP/resources, in-combat/out-of-combat use, corpse availability before respawn, Resurrection Sickness stacking, interrupted cast costs/refunds, whether it can receive Grace empowerment, whether it generates Grace and how an uninterrupted eight-second cast behaves if Mana changes during casting.
- **Mana-management fantasy:** resurrecting should leave Priest temporarily unable to heal via Mana, so cost/eligibility must not be bypassed by a cheap trick or accidental double-cast. Precise mechanisms require approval.

### Grace: generation, rank milestones and reset (PRIEST-012/014)
- **2 points** from a successfully cast **heal that does not overheal**; **1 point** from all other **successful healing, shield and damage casts**. Round 1's preference for better healing generation is therefore satisfied.
- **OPEN:** whether any wasted overheal reduces 2 to 1 or any actual HP restored earns 2; full-health no-op heal being "successful"; whether shields count on cast or only effective protection. Don't make unconfirmed anti-spam policies into locked decisions.
- Full Grace at **skill rank 1 = 10 points**; full at **highest Grace skill rank = 6 points**. The **number of ranks has NOT been chosen**, so do **not** mistake "6 points at max rank" for "maximum rank 6".
- Grace **does not decay** and **does not persist through save/game loads**; start a fresh load at **zero**.
- Grace is a **manual active power** on the same skill as its passive generation. It is only usable when full, empowers the **next successful spell**, and the empowered cast consumes **all accumulated Grace**. All real spells still cost normal Mana.
- For empowered Light Burst, **only the original/first pulse generates Grace**. Repeated pulses may still damage/heal normally; they do not create more Grace. Further multi-hit/tick exploitation safeguards need explicit design before coding.
- **OPEN:** Grace rank progression and intermediate thresholds, how long armed state lasts, interruption/target cancel handling, self-heal and instant shield event generation, any Mana cost for pressing the Grace toggle.

### Staff attacks, cooldowns and Mana balance (PRIEST-021/023)

- **CONFIRMED:** Priest can wield **staffs**, plus gear whose eligibility explicitly allows **all classes**. It does **not** freely wield every other class weapon.
- **CONFIRMED:** all classes' ordinary basic attacks deal **normal, non-elemental** damage; Priest's ordinary staff autoattack generates **zero Grace**. Other ordinary damage calculations and attack scaling follow shared rules (exact Priest basic attack stat coefficient remains open).
- **CONFIRMED:** the Priest should begin with **relatively long cooldowns**, with **small reductions each time a skill point increases that spell's rank**. Exact baseline/rank formulas are **BALANCE OPEN**. The phrase 'every skillpoint' is interpreted as investment into the specific spell, to be confirmed if different.
- **CONFIRMED:** Mana costs start **low** and **increase with progression**. **OPEN** whether this means Priest **character level**, **spell rank**, or both, and how this interacts with scaling heal power.
- **CONFIRMED gameplay direction:** **Mana management**, rather than a pure cooldown ceiling or limitless healing throughput, should be the main long-fight healer constraint; a weak enemy should not be heal-tanked indefinitely for **20 minutes** merely through bottomless self-heals. Tuned mana pool, regen, spell costs, resource restoration and throughput must make this meaningful while keeping early solo play feasible.
- Grace-powered healing/burst chains remain subject to their ordinary Mana cost. Staff attacks never provide Grace and are not themselves elemental.

### Possible Tier III branches — ideas only

A future **Nature/Druid HoT specialization** and **Light/Hierophant/Prophet specialization** have been discussed, but no Tier III branches or class IDs are approved yet.

## Advancement concept — NOT APPROVED

Recent proposal **Hands of Mercy**: level-30 eligible Magically Touched speaks with the Trimery healer, accepts training, heals three distinct wounded lumberworkers using a temporary quest heal action, protects one recovering worker from a solo-friendly threat, and returns to the same mentor for explicit advancement to level-1 Priest. This is **only a proposed quest**; no implementation authorization. Keep already-completed actions from bypassing acceptance and avoid dependencies on a particular prior skill rank. Prior Mage/Greatblade trials are different.

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

## Round 6 — five questions (awaiting answers)

**PRIEST-026 — Revive level/ranks and Grace.** At what **Priest level** should **Revive** become available for its first rank? Should it have up to five ranks like the other abilities, and if so, what should spending skill points on Revive improve (e.g. resurrected HP, range, cast time, sickness severity or cooldown)? Should the **Grace button** be able to empower Revive, and if yes, what special effect?

**PRIEST-027 — Revive targeting and penalties.** Should Revive work **during combat** on a **dead party member before they choose to respawn**, or also on NPCs/out of combat? With the existing stacking **Resurrection Sickness** concept, should a revived ally return at **low HP** and suffer that sickness? If the Priest is **interrupted during the 8-second cast**, should all Mana be lost or should it only be spent on successfully completed resurrection?

**PRIEST-028 — Starting cooldowns and Mana cost progression.** Since Mana management should cap long-term healing, would you prefer the **Mana cost to grow with each invested spell rank** (rather than passively with character level), so stronger heals cost more? Should cooldown reductions also occur **per rank in the same spell**? Any starting cooldown values you'd prefer for Holy Light, Light Burst, Aegis and Wild Growth, or should I propose provisional numbers?

**PRIEST-029 — Spell availability / rank caps.** All skills begin rank **0** and Grace specifically unlocks at level **5**. Should **Holy Light, Light Burst, Aegis and Wild Growth** all be *eligible to learn* at Priest **level 1**, or unlock gradually later? Do we want **five ranks each** for these spells, with successive ranks only purchasable at designated level breakpoints?

**PRIEST-030 — Aegis aura and Holy Light bounce priority.** For Grace-powered **Ice Aegis**, should its damaging aura also be **Ice-element**, ticking regularly for as long as the shield lasts? For Holy Light chains, does **'lowest-health ally'** mean **lowest HP percentage** (e.g. 20/100 before 100/1000), and should wounded allies always take priority over enemy hits as long as they're within bounce range?

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
| 2026-10-18 | PRIEST-026–PRIEST-030 | Round 6 questions asked | **AWAITING USER** |


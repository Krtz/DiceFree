# DiceFree — Thief Tier II — design interview

**Created:** 2026-10-20  
**Status:** Thief standard World1 Tier II — ROUNDS 1–4 CONFIRMED (2026-10-21). 5 actives + 1 passive: combat Stealth, instant poison stab Envenom, temporary guaranteed BASIC-attack critical buff, 360° Fan of Knives, ground-targeted Smoke Bomb, crit-chance passive with bonus AGI undecided. Stealth 4s→8s and first basic opener ~1.1×→2× scale GRADUALLY; taking DAMAGE does NOT break stealth; enemies cannot see hidden Thief and drop ALL OR VERY MOST of their threat (exact amount open). Envenom uses nonstacking Poison-tag PHYSICAL NATURE DoT; critical buff lasts 1s rank1→4s max, BASIC attacks only. Smoke radius4m, lasts5s, enemies inside have20% basic attack miss (immediately removed upon leaving) plus PHYSICAL TECH DoT. Mana-only/AGI-only, two ordinary stat-bearing daggers and ONE alternating-swing autoattack event. Detailed kit and Round5 questions below. **DESIGN ONLY, NO UNITY EDITS**.
**Branch:** `integration/world1-foundation`  
**Links:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II Core Classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Combat](../COMBAT.md), [Class Requirements](../CLASS_REQUIREMENTS_AND_DISCOVERY.md), [Ranger Tier II](RANGER_TIER2_DESIGN.md), [Completed Acolyte Tier II](ACOLYTE_TIER2_DESIGN.md).

## Confirmed World 1 design scope

Exactly eight ordinary Tier II classes in World 1:
- Physically Blessed Novice → Greatblade, Defender, Ranger, **Thief**.
- Magically Touched Novice → Mage, Priest, Acolyte, Psion.

**USER APPROVED CLASS AND SEQUENCE (2026-10-20):** Acolyte Tier II design is now completed as a reference baseline. **Thief is the NEXT ACTIVE design interview**, then Psion. After all eight normal Tier II designs, design the Quillboar area and major city. Some Tier II advancement mentors **MAY** move to the city, but their location and quests cannot be silently changed in this chat. **No Tier III class designing until World 2.** No secret class design during the current normal Tier II workflow.

## Grounded foundations vs unapproved ideas

**Already confirmed:**
- Thief is one of the **four standard Tier II Physically Blessed Novice branches**, eligible for advancement when the parent reaches **Tier I class level30**; a **new child manifestation at Tier II class level1**, not overwriting the parent. Keep the shared Echo class/manifestation state and ability purchasing rules.
- Physically Blessed is a broad STR/AGI novice, but **THIEF-010 USER CONFIRMED AGI-ONLY SCALING for Thief's offensive class mechanics**; no STR contribution to Thief skill coefficients, though STR's universal defensive role remains per global stats. **THIEF-004: Mana ONLY**, no new Energy, Stamina, Guile or Combo Point class resource. Exact AGI damage coefficients OPEN.
- Shared combat framework supports explicit per-class attack styles, hand-slot equipment, DoT/status tags, positional or dodge mechanics, conditional critical hits, and independent owner effects; **none are automatically unlocked on Thief**.
- The original `docs/TIER2_CORE_CLASSES_DESIGN_V02.md` suggested Rogue daggers. **THIEF-002 confirms TWO ONE-HANDED DAGGERS equipped in mainhand+offhand**. Items remain ordinary stat-bearing gear until authored effects/Intrinsics are introduced later. **THIEF-008 now confirms exactly ONE basic-attack event with ALTERNATING mainhand/offhand stab animations**, not independent offhand strikes, separate attack timers or duplicate OnHit effects. Both equipped daggers contribute their standard item stats.
- For recent Tier II classes, weighted class skill caps/gates target **roughly 80 purchasable ranks through Tier II level80**, with skills beginning rank0 and 1 point earned at level1 and per further level, but Thief's exact skill roster, rank caps and availability gates are NOT YET AUTHORED.

**DESIGNER SUGGESTIONS, NOT USER DECISIONS:**
- **AGI-leaning mobile melee physical damage dealer** with dual short blades, opportunistic positioning, quick chained basic strikes, selective dodges, and debuff/pickpocket-style tricks could differentiate Thief from Greatblade heavy Momentum attacks, Defender sword+shield tanking and Ranger physical ranged archery. But no permanent invisibility, positional damage bonus, dual wield rule, poison, crit multiplier, combo resource or special steal system should be assumed without explicit agreement.
- Potential visual class direction: dexterous nimble rogue in soft gear/hooded burglar silhouette; whether to use one male/female class body, race/species, oversized daggers and clothing are open.
- Could be **Thief** (scrappy dirty-tricks identity) rather than **Rogue** (stealthy professional) depending on user's fantasy. User currently approved name *Thief* as the branch label, while final shipping name can be changed explicitly if later desired.
- **USER-APPROVED THIEF-001:** ordinary Tier II Thief **blends fast assassin, evasive duelist and dirty-tricks burglar** traits. It is NOT forced to pick only one archetype and is not a dedicated advanced Assassin class. How much burst/dodge/trickery each six-skill kit ability has remains to decide. Preserve solo/party/boss viability.
- **USER-APPROVED THIEF-005: DO NOT DESIGN THIEF ADVANCEMENT QUEST YET.** The user wants to design its mentor, story, trial and location **AFTER the major city** is designed. Do not invent a city, NPC, quest objective or relocate other mentors now. Appearance model/race/look remains OPEN pending separate DiceFree working-chat/Codex REVIEW OF ALREADY-DOWNLOADED user assets (Rogue/Thief/Assassin/Ninja variants); the assets have not yet been inspected by this design interview and no specific model chosen.

## Thief — Round 1 five user answers recorded (2026-10-21)

- **THIEF-001 — USER CONFIRMED:** Tier II Thief is a **MIX** of **fast dagger assassin, agile duelist and dirty-tricks burglar**. User also explicitly wants to **SAVE all three as potential DIFFERENT TIER III archetypes for World 2**. These are **FUTURE CONCEPTS ONLY** — no Tier III skill kits, weapon rules, models, branching prerequisites or quests approved/designed now.
- **THIEF-002 — USER CONFIRMED:** TWO **one-handed daggers**, mainhand + offhand. **All equipment across DiceFree currently works as STAT STICKS**, without assuming unique gameplay effects until such special items are created later. No immediate dagger-specific proc/item identity/second independent autoattack schedule is approved.
- **THIEF-003 — USER CONFIRMED:** Thief has **SHORT STEALTH**. The **FIRST BASIC / NORMAL AUTOATTACK WHICH BREAKS STEALTH** deals BONUS DAMAGE. This is not automatically permanent invisibility or an always-on backstab passive. Exact stealth duration, cooldown/Mana, entrance/reveal, movement, enemy detection, combat-state permissions, positional bonuses and buff scaling need dedicated design.
- **THIEF-004 — USER CONFIRMED:** **MANA ONLY** for the class. Do NOT add combo points, energy, guile, trickery or a new Stamina bar during Tier II design unless user reverses decision. Abilities can still have timing/conditional buffs, without inventing resource systems.
- **THIEF-005 — USER CONFIRMED:** **Appearance MUST be decided in working/implementation chat with Codex** after consulting user's existing **downloaded Rogue/Thief/Assassin/Ninja art assets**. No new art generation/import or model chosen here. **Advancement NPC and quest MUST be designed AFTER the big city**, not while planning initial class skills. Some Tier II mentor locations might ultimately change there, later. The exact race/body/species/silhouette remains OPEN.

### Future-only Tier III inspiration — IDEAS BANK, not active designs

From THIEF-001, remember three future directions **for when the user starts World 2 Tier III design**:
- **Assassin archetype:** critical/ambush/burst identity (working archetype, NOT class approval).
- **Duelist archetype:** evasion/tempo/quick reactive attacks (working archetype, NOT class approval).
- **Dirty-Tricks Burglar archetype:** trickery/bleeds/debuffs/opportunism (working archetype, NOT class approval).

**None of these three is currently a committed Tier III branch or final shipping class name.** The user explicitly forbids Tier III design before World2. Do NOT expand these into kits/prerequisites/progression trees now.

### Late-night idea bank — FUTURE TIER III/IV paths and convergence secret (2026-10-21; NOT ACTIVE WORK)

**USER ASKED TO WRITE THESE DOWN BEFORE SLEEP, and will answer Thief Round 2 tomorrow. Preserve ideas faithfully; do not turn them into approved class tree, balancing numbers, quests, or technical tasks now. Tier III design itself is STILL DEFERRED UNTIL WORLD 2.**

**Provisional Thief descent idea (not locked):**

```text
Thief (standard Tier II; mixed class)
  ├── Burglar (potential Tier III) — poison and dirty tricks
  │      └── Pirate (potential Tier IV continuation)
  ├── Duelist (potential Tier III) — two one-handed daggers,
  │      nimble evasion and fast/quick successive strikes
  │      └── an as-yet-UNNAMED future Tier IV Duelist continuation
  └── Assassin (potential Tier III) — stealth, positioning,
         slower but precise, highly lethal deliberate attacks
         └── Tier IV continuation UNDECIDED
```

**IMPORTANT:** Earlier "Assassin" shorthand as **fast burst** was a loose brainstorming prompt; user's newest clearer **potential future Assassin** direction is **SLOWER but highly accurate, deadly, stealth-and-positioning attacks**. These are potential **higher-tier** identities only. The ordinary **Tier II Thief remains a mix** of the three, using **Mana, dual stat-only daggers and short stealth empowering its first breaking basic attack**. No Tier III abilities or numerical skill trees now.

**Possible ECHO-WIDE secret convergence / unlock inspiration (not a committed class):**
- On the **same Echo / account class progression record**, reach a significant milestone in **the Berserker lineage** AND in **the Duelist lineage**. The user mentioned **"having a Berserker at level 120 (or whatever Berserker Tier IV will be) and a Duelist at level 120 (or whatever Tier IV Duelist will be)"**. Therefore, leave EXACT REQUIREMENTS OPEN: whether BOTH prerequisite classes must individually hit **level 120 at Tier III**, OR whether their corresponding **Tier IV manifestations** are required, OR another advanced milestone. Do not silently treat a Tier IV class as level 120 by definition: per current progress architecture Tier III level120 is an advancement threshold toward Tier IV.
- A secret, possibly convergence class would then **dual-wield TWO SWORDS**, **or perhaps even TWO TWO-HANDED SWORDS** (both variants are explicit user brainstorming; neither equipment restriction nor slot capacity is finalized). The **secret class name, tier, exact prerequisites, class model, relative damage, stat focus, weapon restrictions and unlock method remain UNDECIDED**.
- Do not assume a Berserker is part of Thief's parent branch. Berserker has previously been discussed as a future Greatblade lineage, but THAT is also still a speculative World 2 design. The convergence should use the **existing same-Echo multi-branch milestone system** from [Class Requirements](../CLASS_REQUIREMENTS_AND_DISCOVERY.md), not require two simultaneously active classes or characters.
- Avoid adding abilities, quest chains, secret discovery clues or implementation changes now. Keep the idea accessible when World2 Tier III and subsequent Tier IV systems are eventually designed.

**User's present instruction remains unchanged: tomorrow continue exactly the **THIEF-006–010 Round 2 questions** below, without asking further questions tonight.**

### Equipment clarification — globally relevant

**USER says all equipment pieces are presently stat sticks**, including both dagger items. **Later specially authored item drops can gain UNIQUE EFFECTS/INTRINSICS**, which may modify skills for any class; no such specific effects are approved by this decision. Different item stats, offhand slot and gear restrictions are still meaningful; do not imply dual wielding automatically has two independent basic attack timers or two OnHit triggers. Weapon animation, base attack interval and dual-wield feel are character combat design, separate from item mechanics.

### Earlier brainstorming (superseded or still optional)

**Round2 supersedes suggested Rapid Stab/Quickstep/Pocket Sand as automatic choices:** user instead explicitly named **Stealth, Envenom, a crit ability, Fan of Knives, and Smoke Bomb**, plus a crit-focused passive. Do NOT add Rapid Stab/Quickstep/Pocket Sand as seventh+ abilities. No passive evasion/attack-speed bonus has been approved for Tier II. User preferred AGI ONLY over AGI+STR and multiplier over flat stealth-opening damage/automatic guaranteed crit. The detailed kit follows; remaining free-form seeds below are historical ideas only.

### Initial unapproved gameplay seeds for next discussion

- A short combat-usable Stealth spell **USER-CONFIRMED** causes current enemies to LOSE AGGRO and be unable to see/target the hidden Thief until revelation; first breaking basic hit amplified. Exact activation/reveal/damage cancellation, enemy threat-memory reset vs temporary drop, boss encounter-retarget safety and numeric rank interpolation OPEN.
- A fast multi-stab physical active, a dodge/quickstep and a debuff/disruptive dirty trick could complement the stealth skill. No separate Combo resource.
- Passive AGI, basic attack speed, evasion or positional synergy remain ideas, not approved bonuses.
- The class could eventually have around six active/passive skill ranks and ~80 purchaseable ranks for Tier II level80, **but its skill roster and max ranks must be designed before choosing an exact schedule**.

## Round 2 — five USER decisions recorded 2026-10-21

- **THIEF-006 CONFIRMED:** short Stealth has **~4-second duration at rank1 and ~8-second duration at the eventual skill cap**. User said "Start at line 4 seconds" (interpreted as "like 4 seconds") and "be like 8 seconds at max rank", so these are initial targets, not exact interpolation/gate schedule. It is not yet confirmed whether usable IN COMBAT, whether incoming damage cancels it, whether hostile AI drops target, exact cooldown/Mana, or what level/rank reaches max.
- **THIEF-007 CONFIRMED:** bonus on the FIRST **normal/basic autoattack which breaks stealth** must be a **DAMAGE MULTIPLIER**, not a flat number and NOT a guaranteed critical hit just from stealth. No extra rear-position/backstab bonus at Tier II; backstab reward belongs to later classes. The question offered `+50%` only as an example; user selected **option B** but did NOT approve an exact 1.5× coefficient.
- **THIEF-008 CONFIRMED:** two stat-only one-handed daggers equipped in mainhand + offhand. **ONE basic attack EVENT total with an alternating RIGHT/LEFT dagger stab animation** (option A). No independent offhand attacks, timers, second critical roll, double status procs or separate OnHit event per normal basic attack unless some future unique item explicitly authors a different rule.
- **THIEF-009 CONFIRMED USER-CHOSEN ABILITY NAMES/ROSTER:** **STEALTH**, **ENVENOM** (poison idea, exact application/damage/channel/effect not specified), **"A CRIT"** (a dedicated critical-hit skill, whether instant guaranteed critical attack or short buff remains OPEN), **FAN OF KNIVES** (area knife attack, range/hits/projectiles/damage OPEN), and **SMOKE BOMB** (creates damaging AREA DAMAGE-OVER-TIME and applies **MISS CHANCE TO ENEMIES INSIDE THE SMOKE**, exact damage type/radius/duration/miss penalty and effect application OPEN). No Rapid Stab, Quickstep or Pocket Sand was selected. Skills need not all start at class level1; exact unlock levels TBD.
- **THIEF-010 CONFIRMED / PARTLY OPEN:** all **Thief offensive class damage scaling = AGI ONLY, with NO STR component**. First passive's main identity is **critical-hit chance** (user chose **B**). User said **"maybe bonus agi"**, so bonus AGI is a **desired possibility NOT fixed as a numerical/guaranteed passive rule**. Passive effect eligibility (basic attacks only vs Thief skills), critical multiplier, rank growth and whether/how much AGI it grants are OPEN. DiceFree global crit baseline is **0% unless an authored explicit crit rule opts in**: see `docs/STATS_AND_DAMAGE.md`.

## Current Thief Tier II kit — FIVE active abilities + ONE passive (working names)

| Skill | User-approved concept | Precise mechanics still OPEN |
| --- | --- | --- |
| **Stealth** | Combat-usable invisibility; enemies cannot see/target Thief and DROP ALL OR MOST OF THEIR THREAT (exact % OPEN). **Taking damage DOES NOT break** it. Duration ~4s rank1→8s max GRADUALLY; first stealth-breaking basic attack ~1.1×→2× multiplier GRADUALLY, no behind bonus | Threat wipe vs reduction, precise rank cap/formula, other reveal conditions, boss behavior, cooldown/Mana OPEN |
| **Envenom** | Instant dagger stab applies a NONSTACKING **Poison-tag PHYSICAL NATURE ELEMENT DoT**, damage scaling only AGI | Initial stab's element, DoT tick rate/duration/refresh, cooldown/Mana and precise coefficients OPEN |
| **Critical buff** (name TBD) | Guarantees crit on **basic attacks ONLY** during self-buff, lasting **1 second at rank1 → 4 seconds at skill cap** | Rank cap/duration interpolation, crit damage multiplier, Mana/CD and passive interaction OPEN |
| **Fan of Knives** | **Thief-centered, FULL 360-DEGREE knife AoE**, not forward cone. Distinct from dagger autoattacks | Radius, one hit per enemy vs more, physical damage packets/AGI scaling, animations/projectile visuals, Mana/CD OPEN |
| **Smoke Bomb** | **GROUND-TARGETED 4m radius CLOUD lasting 5s**, inflicts **PHYSICAL TECH ELEMENT DoT**; enemies inside have **20% MISS CHANCE for BASIC AUTOATTACKS ONLY**. Miss penalty ENDS INSTANTLY on leaving area; reapplies on reentry | Damage cadence, stacking of multiple clouds, target range, Mana/CD, boss Tenacity interaction OPEN |
| **Critical passive** (WORKING name) | Primarily increases **critical hit chance**; user also considering **some bonus AGI** | Exact crit chance/rank, which Thief actions may crit, baseline crit damage multiplier, AGI per rank, rank cap |

**Five active + one passive** is an accurate description of the user-named concepts. Exact final shipped ability titles, whether the "crit" is an active guaranteed-hit move rather than a temporary buff, and detailed numeric properties await further answers. Since all skills start rank0 and class gains one point each class level, weighted skill caps/gates totalling ~80 remain to be designed AFTER individual mechanics.

**Tier III idea bank remains unchanged:** Future Burglar→Pirate, Duelist, Assassin and Berserker+Duelist dual-sword secret are speculative World2+/later progression only. Do NOT design those future kits now. Thief advancement quest waits until after the big city, appearance awaits the separate working chat/Codex asset review.

## Round 3 — five USER decisions recorded 2026-10-21

- **THIEF-011 USER CONFIRMED:** Stealth **IS ACTIVATABLE DURING COMBAT**; enemies **LOSE AGGRO** on Thief and **CANNOT SEE / TARGET hidden Thief**. The breaking FIRST normal/basic autoattack uses **rank1 ~1.1× damage**, scaling to **~2.0× at maximum Stealth rank**, as an APPROVED design direction / first-pass target (user said "or something similar"; exact rank growth, true fixed ceiling and final rank count not locked). **Taking damage immediately breaking Stealth was asked but NOT explicitly answered**: keep open. Also OPEN whether "lose aggro" clears threat values, suspends their target selection, transfers aggro, or changes boss scripted/fixed targeting; do not silently reset encounter/undo boss mechanics.
- **THIEF-012 USER CONFIRMED:** Envenom is **the FIRST proposed option: an INSTANT SINGLE-TARGET POISON DAGGER STAB**, not a multi-attack dagger coating or ranged poison delivery. Proposed on-hit Poison-tag DoT/nonstacking remains a *suggestion* rather than an explicit user-chosen damage duration, element or stack rule. AGI offense for Thief remains confirmed.
- **THIEF-013 USER CONFIRMED:** dedicated 'crit' skill is **A TIME-LIMITED SELF BUFF giving GUARANTEED CRITICAL HITS for x seconds**. This SUPERSEDES an instantly damaging guaranteed crit strike. Exact seconds, whether EVERY eligible autoattack / eligible active abilities crit for the window, versus ONLY NEXT hit during the window, interactions with passive crit chance and crit multiplier are undecided. General DiceFree critical strike rules are per-action opt-in in `docs/STATS_AND_DAMAGE.md`, so define explicit eligible actions and their crit override eventually.
- **THIEF-014 USER CONFIRMED:** Fan of Knives is a **FULL 360-DEGREE ATTACK AROUND THIEF**, not a forward cone. Earlier suggestion of one damage event per enemy is only an implementation/balance recommendation, not separately confirmed by user's "Yes, 360".
- **THIEF-015 USER CONFIRMED:** Smoke Bomb is **GROUND TARGETED**, creating persistent **AoE DAMAGE OVER TIME** and an enemy MISS CHANCE while **INSIDE THE CLOUD**, but this accuracy penalty affects **ENEMY BASIC/AUTOATTACKS ONLY**, **NOT enemy abilities, spells or ground-targeted mechanics**. Exact miss chance, radius, duration, damage interval/element, ally protection and boss debuff handling OPEN.

### Consolidated active combat identity

Thief remains **dual stat-stick daggers, one alternating-swing basic attack event, Mana only, AGI-only offensive scaling**. Five active ability identities: **Stealth** (true combat invisibility, enemy aggro loss and enhanced opening attack), **Envenom** (instant poison stab), **timed guaranteed-Crit Buff**, **Fan of Knives** (360° nearby burst), and **Smoke Bomb** (ground-targeted area DoT + enemy-autoattack miss). Passive: **bonus critical-hit chance**, optional additional AGI OPEN. No behind-target bonus in Tier II. The current six-skill roster is maintained (5 actives + 1 passive), no new skill added. Tier III Assassin/Duelist/Burglar/Pirate and secret Berserker+Duelist swords class remain PARKED future concepts, NOT actively designed.

## Round 4 — five USER decisions recorded 2026-10-21

- **THIEF-016 CONFIRMED:** Taking damage **DOES NOT BREAK STEALTH**; enemy threat should be **DROPPED entirely or reduced by a very large amount**, rather than simply suspending targeting. Exact 100% wipe vs numeric partial threat loss remains OPEN: the user explicitly accepted EITHER ("entirely, or at least by very much"). Enemies still cannot perceive hidden Thief. Ordinary damage still applies; no boss encounter reset, no automatic damage immunity. Basic attack breaks Stealth as previously confirmed. Which non-basic actions reveal the Thief and boss scripted exceptions OPEN.
- **THIEF-017 CONFIRMED:** Stealth lasts approximately **4 seconds rank1 → 8 seconds max**, and first breaking basic-attack damage multiplier increases from **~1.1× rank1 to ~2.0× max**. **Both grow GRADUALLY with ranks**, not whole-second tier jumps; exact cap, interpolation and skill availability gates still undecided. No behind positional bonus.
- **THIEF-018 CONFIRMED:** Envenom is an INSTANT melee poison stab applying a **NONSTACKING DoT tagged Poison**, whose periodic damage has **PHYSICAL CHANNEL and NATURE ELEMENT**. This is **Physical Nature**, not Magical Nature, Radiation, Coffee or a fictional Poison element. Whether the initial stabbing hit is Physical Nature or neutral Physical, DoT period/duration and refresh mechanics OPEN.
- **THIEF-019 CONFIRMED:** the self-buff gives **GUARANTEED CRITICAL HITS to BASIC/AUTOATTACKS ONLY**, not Envenom/Fan/Smoke; duration **1 second at first skill rank → 4 seconds at max rank**. Exact rank cap/interpolation, crit damage multiplier, mana/cooldown OPEN. Global crit baseline remains opt-in; buff guarantees crit on eligible normal basic hits for its duration.
- **THIEF-020 CONFIRMED:** ground-targeted Smoke Bomb **4 METERS RADIUS**, persists **5 SECONDS**, imposes **20% MISS CHANCE on ENEMY BASIC/AUTOATTACKS ONLY while an enemy is inside**, penalty disappears IMMEDIATELY on EXIT and reapplies on reentry. Inflicts **PHYSICAL TECH ELEMENT DoT**. No hostile spell/ability miss chance and Smoke does not make Thief invisible. Damage tick rate/coefficients and overlapping-cloud stacking OPEN.

### Elemental and gameplay integration notes

`Nature` is an explicitly documented DiceFree element and `Tech` appears in the established DiceBound-inherited element roster `docs/ELEMENTS.md`. Each damage event still has **one channel (Physical here) and one elemental affinity (Nature for Envenom DoT, Tech for Smoke DoT)**, so use Physical Defense and the matching element's resistance. Poison is a semantic status tag, not another elemental resistance or universal poison proc. No special Nature/Tech status trigger or fixed coefficient authorized merely by the name.

Thief remains **AGI-only offensive scaling**, **Mana only**, **one basic hit event** with alternating dual-dagger animation, no extra backstab benefit at Tier II. Future Burglar→Pirate, Duelist, Assassin and Berserker+Duelist secret concepts stay PARKED for later worlds. Appearance chosen by separate working-chat/Codex review of already downloaded assets; advancement quest after big city. NO implementation changes.

## Round 5 — EXACTLY FIVE questions (AWAITING USER)

**THIEF-021 — Exact Stealth threat reduction:** Should activating Stealth delete **100% of Thief's threat on every affected enemy**, or reduce it by **90%** while hiding them from enemy targeting? Either matches your previous answer. The boss encounter itself should not reset; I lean toward 90% for party fights.

**THIEF-022 — Stealth rank curve and cap:** To make both bonuses increase gradually, would you approve **20 Stealth ranks**, with linearly increasing duration **4→8 seconds** and first-basic multiplier **1.1×→2×**? This would imply `duration(r)=4+4×(r−1)/19` seconds and `damageMultiplier(r)=1.1+0.9×(r−1)/19`. These are proposals, not yet signed-off numbers; this cap would be one part of the eventual ~80 class rank budget.

**THIEF-023 — Envenom initial damage and DoT:** Should the initial dagger stab itself cause **non-elemental Physical** AGI damage, followed by **3 seconds of once-per-second Poison-tag Physical Nature DoT**, refreshing the existing nonstacking poison on reapplication? Or should the initial stab also be Physical Nature?

**THIEF-024 — Smoke periodic damage and overlap:** Should the **Physical Tech DoT** tick **once per second**, and should overlapping friendly Smoke Bomb zones **NOT STACK** their 20% basic-miss penalty (instead of stacking to 40% or more)? We can still choose whether overlapping clouds combine their damage ticks later.

**THIEF-025 — Passive crit and AGI:** Should Thief's passive give **+1 percentage point crit chance per passive rank on BASIC ATTACKS**, along with **+1 AGI per rank** (your earlier 'maybe AGI'), with **1.5× crit damage** for its eligible basic hits? Or would you prefer crit chance WITHOUT AGI? The separate timed guaranteed-crit buff overrides to 100% on normal basics during its 1–4s duration.

## Decision ledger

| Date | ID | Decision | State |
| --- | --- | --- | --- |
| 2026-10-19 | WORLD1-ROSTER | Standard Tier II Thief under Physically Blessed, one of four physical/eight normal Tier II | **USER CONFIRMED** |
| 2026-10-20 | WORLD1-ORDER | Acolyte completed → Thief now → Psion next → Quillboar → major city; Tier III World2 | **USER CONFIRMED** |
| 2026-10-20 | THIEF-FOUNDATIONS | AGI/daggers/stealth resource details not chosen; older Rogue dagger silhouette remains suggestion | **DESIGNER ONLY** |
| 2026-10-21 | THIEF-001 | Blend assassin, duelist and dirty-tricks burglar; remember three as future World2 Tier III archetypes ONLY | **USER CONFIRMED Tier II MIX; Tier III inspiration RECORDED but no design** |
| 2026-10-21 | THIEF-002 | Dual one-handed daggers equipped, all equipment currently stat sticks; item Intrinsics may come later | **USER CONFIRMED, attack event timing OPEN** |
| 2026-10-21 | THIEF-003 | Short stealth grants extra damage on FIRST normal basic autoattack that breaks stealth | **USER CONFIRMED, actual stealth rules/numbers OPEN** |
| 2026-10-21 | THIEF-004 | Mana ONLY, no new resource | **USER CONFIRMED** |
| 2026-10-21 | THIEF-005 | Look chosen later by working chat/Codex from user-downloaded assets; advancement NPC/quest AFTER big city | **USER CONFIRMED DEFERRED** |
| 2026-10-21 | THIEF-FUTURE-PATHS | PROVISIONAL Tier III paths: Burglar→Pirate(TierIV), agile dual-dagger Duelist, slower lethal stealth-positioning Assassin | **USER-BRAINSTORMED; WORLD2 FUTURE ONLY, NOT APPROVED PATH GRAPH** |
| 2026-10-21 | THIEF-FUTURE-CONVERGENCE | Possible Berserker-lineage + Duelist-lineage late milestone unlock on same Echo (perhaps both level120 or TierIV) → secret dual-sword / dual-two-handed-sword class | **USER-BRAINSTORMED; unlock criteria / tier / weapon rules UNDECIDED** |
| 2026-10-21 | THIEF-006 | Stealth approx4s rank1 to8s max | **USER CONFIRMED duration targets; rank interpolation/combat behavior OPEN** |
| 2026-10-21 | THIEF-007 | Breaking first basic attack uses MULTIPLIER, no behind damage; 50% example NOT approved | **USER CONFIRMED mechanic, coefficient OPEN** |
| 2026-10-21 | THIEF-008 | One basic attack event with alternate dagger animations; both daggers stat items | **USER CONFIRMED** |
| 2026-10-21 | THIEF-009 | Five actives: Stealth, Envenom, Crit skill, Fan of Knives, Smoke Bomb (area DoT + enemy miss) | **USER CONFIRMED skill ideas; mechanics still OPEN** |
| 2026-10-21 | THIEF-010 | AGI ONLY damage scaling; crit-chance focused passive with maybe additional AGI | **AGI-only and crit focus CONFIRMED; passive AGI OPEN** |
| 2026-10-21 | THIEF-011 | In-combat stealth; enemy drops aggro and cannot see Thief, opener ~1.1× initial → ~2× max | **USER CONFIRMED; damage-break, aggro storage/curves OPEN** |
| 2026-10-21 | THIEF-012 | Envenom is instant poison DAGGER STAB | **CONFIRMED; on-hit DoT and element OPEN** |
| 2026-10-21 | THIEF-013 | Critical ability is TIMED SELF BUFF granting guaranteed crits for x seconds | **CONFIRMED; eligible attack actions/duration OPEN** |
| 2026-10-21 | THIEF-014 | Fan of Knives is 360° around Thief | **CONFIRMED; hit count per enemy OPEN** |
| 2026-10-21 | THIEF-015 | Smoke Bomb ground-targeted AoE DoT; enemy MISS CHANCE only BASIC attacks while inside | **CONFIRMED; radius/miss chance/DOT OPEN** |
| 2026-10-21 | THIEF-016 | Taking damage does not break stealth; enemies drop all or much threat, exact percentage unresolved | **CONFIRMED / EXACT THREAT OPEN** |
| 2026-10-21 | THIEF-017 | Duration4→8s and opener1.1×→~2× grow gradually with ranks | **CONFIRMED / RANK CAP OPEN** |
| 2026-10-21 | THIEF-018 | Envenom applies nonstacking Poison-tag PHYSICAL NATURE DoT | **CONFIRMED / CADENCE AND STAB ELEMENT OPEN** |
| 2026-10-21 | THIEF-019 | Guaranteed crits basic attacks ONLY, 1s rank1→4s maximum | **CONFIRMED / RANK CAP/CD OPEN** |
| 2026-10-21 | THIEF-020 | Smoke 4m radius/5s,20% enemy basic-attack miss inside, removed immediately on exit, PHYSICAL TECH DoT | **CONFIRMED / DOT TICK OPEN** |
| 2026-10-21 | THIEF-021–025 | Round5 five follow-up questions | **AWAITING USER** |

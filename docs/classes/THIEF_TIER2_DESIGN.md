# DiceFree — Thief Tier II — design interview

**Created:** 2026-10-20  
**Status:** World 1 normal Tier II Thief — **ROUNDS 1–6 CONFIRMED** (2026-10-22). Mana only, AGI-only offense, dual stat-only daggers with single alternating basic attack event. Stealth removes95% threat and 4→8s concealment and 1.1→2× BASIC opener; incoming damage doesn't reveal. **Envenom/Fan get MORE ranks than Stealth** in new designer-authored, validated schedule 16/18/10/18/10/8=80, replacing old20/14/12/14/12/8 (exact gates are designer proposals under delegated rank mathematics). Envenom Physical Nature poison RAW AGI tick formula approved, duration refresh preserves tick timer. Smoke Physical Tech four ticks/sec with approved per-second AGI formula÷4, nonstacking. Fan is moving-usable, 4m/360°, one Physical event per enemy. **Envenom, Fan, Smoke and activating Crit Buff BREAK STEALTH**, without BASIC opener bonus. Initial cooldown/Mana numbers are pending approval. Appearance/quest deferred. **DESIGN ONLY; NO UNITY CODE/ASSET/BUILD CHANGES**.
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
| **Stealth** | Combat Stealth reduces Thief threat95%, hidden untargetable even if taking damage. Smooth 4→8s and 1.1→2× BASIC first-break multiplier over **DESIGNER-PROPOSED 16 ranks**. Envenom, Fan, Smoke, crit-buff activation and attacking BASIC reveal, but only BASIC earns opener bonus | Revised designer cap16/gates pending numeric approval; mana/CD and enemy reacquire semantics open |
| **Envenom** | Instant non-elemental Physical stab RAW **2× ordinary basic attack**, then single nonstacking 3s Poison-tag **Physical Nature DoT**, tick RAW `(2+0.10×AGI)×(1+0.05×(r−1))` once/sec. Refresh3s duration but **KEEP existing tick clock**. Casting BREAKS STEALTH without basic opener | Mana/CD and cleanup/immunity handling open |
| **Critical buff** (working name) | Basic attacks ONLY guaranteed critical for 1s rank1→4s max, 1.5× RAW basic crit; activating buff BREAKS STEALTH without opener bonus. New designer cap10 | Mana/CD and snapshot/order of crit rules open |
| **Fan of Knives** | **4m radius, full360° centered on Thief**, ONE non-elemental Physical attack event per enemy per cast, AGI and skill-rank scaling. **CASTABLE WHILE MOVING**; cast BREAKS STEALTH, no basic-only crit or basic opener bonus | Raw damage coefficient and Mana/CD OPEN |
| **Smoke Bomb** | Ground-targeted4m radius/5s zone,20% enemy BASIC miss inside only, nonstacking overlaps. Physical Tech RAW PER SECOND `(2+0.08×AGI)×(1+0.05×(r−1))`, **every0.25s TICK RAW = that /4**,20 nominal ticks/zone. Casting BREAKS STEALTH, no opener bonus | Mana/CD, strongest overlapping cloud credit and target exit timing OPEN |
| **Critical passive** (working name) | +1 AGI and +1 percentage point BASIC-attack crit chance/rank, crit 1.5× RAW, proposed cap8 yields +8 AGI/+8pp; buff temporarily guarantees basic crit | Future item crit permission interaction handled by general rules |

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

## Round 5 — user decisions recorded 2026-10-21

- **THIEF-021 USER APPROVED:** entering Stealth reduces the Thief's existing per-enemy threat by **EXACTLY 95%**; stored value after cast is `0.05×priorThreat`. This reduction is ON CAST, not every hit/periodic tick. Hidden Thief remains untargetable; damage taken does not break invisibility. Do not reset boss encounter/other party threat. Aggro can resume on reveal as ordinary threat adds.
- **THIEF-022 USER DELEGATED the rank mathematics** ("You do the rank math"). Exact caps/gates below are **designer-selected first-playtest proposals**, not an explicit user sign-off of every number. Choose Stealth max20 (as suggested last round); its linear duration `4+4×(r−1)/19` seconds and first breaking BASIC multiplier `1.1+0.9×(r−1)/19` scale smoothly, reaching 8s/2× at rank20. All80 class levels audited below.
- **THIEF-023 USER APPROVED:** instant Envenom's initial dagger hit does **DOUBLE NORMAL AUTOATTACK DAMAGE** — `EnvenomDirectRaw=2×normalBasicAttackRaw`, ordinary non-elemental Physical, then one **3-second, ONCE-PER-SECOND, NONSTACKING Poison-tag Physical Nature DoT**. Direct damage follows the FIRST offered variant ("Yes" plus explicit double-damage stipulation). Reapply refreshes duration. It is a spell/skill attack, not another normal autoattack event; avoid extra Stealth-opener/basic-only-crit interactions unless subsequently authored. Its Nature tick raw coefficient and refresh clock need tuning.
- **THIEF-024 USER APPROVED:** Smoke Bomb deals **PHYSICAL TECH DoT FOUR TICKS/SECOND**, every 0.25s, nominal20 ticks during its5s life. Individual tick damage is **one quarter of the once-per-second equivalent per-second DAMAGE budget**; no raw per-second coefficient was previously signed off. **"Non-stacking" applies to overlapping clouds**, including **20% enemy BASIC miss chance** (not additive) and the DoT (no double ticks for the same target). Source priority/credit exact behavior open. Miss ends instantly when enemy exits all smoke.
- **THIEF-025 USER APPROVED:** Thief passive grants **+1 AGI and +1 percentage point normal BASIC attack crit chance PER RANK**, and its eligible critical basic hits apply **1.5× raw damage**. The active guaranteed-crit buff overrides the chance to100% on normal basics during its rank-scaled duration; no guaranteed crit for Envenom, Fan of Knives or Smoke Bomb. Passive proposed cap8 would yield +8 AGI/+8 percentage points chance.

## Round 6 — FIVE USER DECISIONS RECORDED 2026-10-22

- **THIEF-026 USER CONFIRMED DIRECTION:** Envenom and Fan of Knives each should receive MORE purchased ranks than Stealth. Earlier 20 Stealth/14 Envenom/14 Fan designer proposal is SUPERSEDED. User previously delegated rank math; new **designer-proposed first-playtest ranks** are **Stealth16 / Envenom18 / Crit buff10 / Fan18 / Smoke10 / Crit passive8 =80**. Both active damage skills18 exceed Stealth16. At Stealth16, smoothly interpolate `duration=4+4×(r−1)/15` seconds and first NORMAL BASIC opener multiplier `1.1+0.9×(r−1)/15`. At Crit buff10 duration `1+3×(r−1)/9` seconds. Exact caps/gates are designer choices, user has not separately specified each number.
- **THIEF-027 USER APPROVED FIRST PASS NUMERIC DAMAGE:** Smoke Bomb raw Physical Tech PER SECOND `(2+0.08×AGI)×(1+0.05×(SmokeRank−1))`; per tick every 0.25s is this amount **DIVIDED BY FOUR**. The 4m, 5s, 20%-enemy-basic miss and nonstacking clouds are unchanged. Nominal20 damage ticks per full uninterrupted cloud, not fourfold DPS.
- **THIEF-028 USER APPROVED FIRST PASS NUMERIC DAMAGE/TICK CLOCK:** Envenom instant dagger raw hit equals **2× NORMAL BASIC RAW DAMAGE** (Physical, no element), plus single3s nonstacking Poison-tag Physical Nature DoT with ONE tick per second. RAW per tick `(2+0.10×AGI)×(1+0.05×(EnvenomRank−1))`. Recast **REFRESHES remaining duration to3s but DOES NOT reset existing once-per-second periodic clock**.
- **THIEF-029 USER CONFIRMED:** Fan of Knives is **SELF-CENTERED 360-DEGREE PHYSICAL AoE**, radius **4 meters**, **ONE damage instance per enemy** in radius per cast, with AGI and Fan rank scaling; **USABLE WHILE MOVING**. Fan is a SKILL event, not an additional basic attack/automatic crit proc. Fan's precise RAW initial coefficient remains OPEN.
- **THIEF-030 USER CONFIRMED REVEAL RULE & DELEGATED BALANCE DESIGN:** Casting **ENVENOM**, **FAN OF KNIVES**, **SMOKE BOMB**, or **ACTIVATING GUARANTEED-CRIT BUFF** breaks Stealth, as does using a normal basic attack. **Only a NORMAL BASIC autoattack that breaks Stealth receives its 1.1×→2× bonus.** Incoming damage still does not reveal Thief. STEALTH ACTIVATION itself grants concealment, not cancels it. User authorized proposing initial cooldown/Mana costs, but did NOT approve the concrete numbers below.

## Revised Thief 80-rank schedule — DELEGATED MATHEMATICS, VALIDATED DESIGNER BASELINE

Six skills begin at rank0 at Tier II class level1, earn1 class skill point at class level1 and one per further level until80. Skill rank purchases require prior rank and its level gate. This newer schedule redistributes ranks to make **Envenom and Fan out-rank Stealth** as the user requested. Exact gates/maxima are user-delegated designer baselines pending numeric sign-off.

| Skill | Proposed maximum rank | First eligible class level | Further upgrade interval | Last eligible gate | Every rank gate |
| --- | ---: | ---: | --- | ---: | --- |
| Stealth | 16 | 1 | Every 5 levels | 76 | 1, 6, 11, 16, 21, 26, 31, 36, 41, 46, 51, 56, 61, 66, 71, 76 |
| Envenom | 18 | 1 | Every 4 levels | 69 | 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69 |
| Guaranteed-crit buff | 10 | 5 | Every 8 levels | 77 | 5, 13, 21, 29, 37, 45, 53, 61, 69, 77 |
| Fan of Knives | 18 | 1 | Every 4 levels | 69 | 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69 |
| Smoke Bomb | 10 | 6 | Every 7 levels | 69 | 6, 13, 20, 27, 34, 41, 48, 55, 62, 69 |
| Critical passive | 8 | 1 | Every 10 levels | 71 | 1, 11, 21, 31, 41, 51, 61, 71 |
| **TOTAL** | **80** | — | — | **77** | All80 gates eligible by level77 |

**Exhaustive math audit: PASS:** for EVERY integer class level `L=1..80`, cumulative rank gates `eligible(L)≥L`; no unspendable point if player wishes to spend one each level. All gates≤77. Checkpoints:

| Class level | Eligible rank count | Earned points | Cushion |
| ---: | ---: | ---: | ---: |
| 1 | 4 | 1 | 3 |
| 4 | 4 | 4 | 0 |
| 5 | 7 | 5 | 2 |
| 6 | 9 | 6 | 3 |
| 10 | 11 | 10 | 1 |
| 20 | 21 | 20 | 1 |
| 30 | 33 | 30 | 3 |
| 40 | 42 | 40 | 2 |
| 50 | 54 | 50 | 4 |
| 60 | 63 | 60 | 3 |
| 70 | 76 | 70 | 6 |
| 75 | 78 | 75 | 3 |
| 76 | 79 | 76 | 3 |
| 77 | 80 | 77 | 3 |
| 78 | 80 | 78 | 2 |
| 79 | 80 | 79 | 1 |
| 80 | 80 | 80 | 0 |

### Rank-scaled examples under revised designer caps

- Stealth proposed max16: each purchased rank after1 adds `4/15≈0.26667s` hidden duration and `0.9/15=0.06×` BASIC opener multiplier. Rank1 4s/1.1×; rank16 8s/2×. Threat drop 95% upon activation; damage taken does not reveal.
- Crit buff proposed max10: 1→4s basic-only guaranteed crit, rank formula `1+3×(r−1)/9`; activation reveals Stealth.
- Envenom proposed max18: RAW Physical normal-basic-derived hit×2 plus three1s-ticks of Physical Nature Poison using approved RAW AGI coefficient and +5% increase per rank over1. Refresh never restarts periodic clock.
- Fan proposed max18: 4m moving-usable 360° Physical single damage packet/enemy, raw AGI/rank coefficient TBD.
- Smoke proposed max10: four0.25s Physical Tech ticks/sec, raw/tick `[(2+0.08×AGI)×(1+0.05×(r−1))]/4`, nonstacking enemy zone effects.
- Crit passive proposed max8: +8 AGI/+8 percentage points BASIC critical chance and 1.5× RAW normal-basic crits at cap.

## First-pass skill cooldown and Mana cost proposals — NOT YET USER-APPROVED

| Active | Proposed cooldown | Proposed Mana | Balance purpose |
| --- | ---: | ---: | --- |
| **Stealth** | **24s** | **20** | 95% threat dump and 4–8s untargetability, powerful basic opener; avoid spamming |
| **Envenom** | **8s** | **12** | Repeatable single-target double-basic damage plus 3s nonstacking poison |
| **Guaranteed-Crit buff** | **18s** | **16** | 1–4s buff on basic autoattacks only; activating it breaks Stealth |
| **Fan of Knives** | **9s** | **18** | Moving 4m AoE, one hit per nearby enemy |
| **Smoke Bomb** | **18s** | **20** | Five-second cloud DoT and 20% basic miss chance while inside |

**Every above Mana/CD value is a DESIGNER PLAYTEST PROPOSAL, not yet user-approved.** No class was built in this chat. No Tier III design until World 2; after Thief design, Psion is next.

## Round 7 — FIVE questions (AWAITING USER)

**THIEF-031 — Ranks:** Do you approve the revised, mathematically validated **16 Stealth / 18 Envenom / 10 Crit buff / 18 Fan / 10 Smoke / 8 Crit passive = 80** as our initial playtest ranks, with all levels1–80 spendable? Stealth max16 still reaches8s and a2× basic opener.

**THIEF-032 — Mana and cooldowns:** Do these proposed first-playtest costs look good: **Stealth24s/20 Mana, Envenom8s/12, Crit buff18s/16, Fan9s/18, Smoke18s/20**?

**THIEF-033 — Fan damage:** Would you approve initial **RAW Physical Fan damage per enemy** `(8+0.45×AGI)×(1+0.06×(FanRank−1))`? One hit per enemy within4m, moving-usable, no basic-only crit.

**THIEF-034 — Stealth exit/threat:** When Stealth breaks or times out, should normal enemy targeting resume using **the remaining 5% threat plus any NEW threat** gained thereafter, never restoring the deleted95%? Should the first basic opener damage apply on the attack that simultaneously reveals the Thief? I recommend yes.

**THIEF-035 — Smoke and Envenom target behavior:** Should enemies stop taking Smoke Bomb Physical Tech damage IMMEDIATELY upon leaving the cloud (just as its miss debuff already does), while Envenom's attached Poison DoT continues to tick after the victim walks away? I recommend yes.

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
| 2026-10-21 | THIEF-021 | Stealth reduces Thief stored threat exactly95%, retains5%; no encounter reset | **USER CONFIRMED** |
| 2026-10-21 | THIEF-022 | Rank math delegated; designer 20/14/12/14/12/8=80, all level gates audited | **DESIGNER PROPOSAL under user delegation; gates not explicitly signed off** |
| 2026-10-21 | THIEF-023 | Envenom direct Physical RAW2×normal basic; 3s/1tick-s nonstacking Poison-tag Physical Nature DoT | **USER CONFIRMED; poison RAW coefficient OPEN** |
| 2026-10-21 | THIEF-024 | Smoke Physical Tech DoT four times/sec; per tick quarter same per-second damage; overlapping effects nonstacking | **USER CONFIRMED; per-second RAW coefficient OPEN** |
| 2026-10-21 | THIEF-025 | Passive +1pp BASIC crit chance +1 AGI per rank; crit RAW multiplier1.5× | **USER CONFIRMED; passive cap8 designer proposal** |
| 2026-10-22 | THIEF-026 | More ranks Envenom+Fan than Stealth; new designer proposal 16/18/10/18/10/8=80, audit pass | **USER APPROVED DIRECTION; exact gates DESIGNER PROPOSAL** |
| 2026-10-22 | THIEF-027 | Smoke Physical Tech per-second RAW `(2+0.08×AGI)×(1+0.05×(r−1))`, each0.25s tick÷4 | **USER APPROVED INITIAL COEFFICIENT** |
| 2026-10-22 | THIEF-028 | Envenom Physical Nature per-second RAW tick `(2+0.10×AGI)×(1+0.05×(r−1))`, clock preserved on refresh | **USER APPROVED INITIAL COEFFICIENT/TIMER** |
| 2026-10-22 | THIEF-029 | 4m 360° Fan, one Physical damage event per enemy, usable while moving | **USER APPROVED, RAW coefficient OPEN** |
| 2026-10-22 | THIEF-030 | Envenom/Fan/Smoke/activating Crit buff break stealth with no basic opener; design Mana/CD proposals | **USER APPROVED REVEAL; CD/Mana DESIGNER PROPOSALS** |
| 2026-10-22 | THIEF-031–035 | Five Round7 questions | **AWAITING USER** |

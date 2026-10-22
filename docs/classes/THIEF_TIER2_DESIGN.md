# DiceFree — Thief Tier II — design interview

**Created:** 2026-10-20  
**Status:** **THIEF TIER II GAMEPLAY DESIGN COMPLETE — ROUNDS 1–8 USER-APPROVED THROUGH 2026-10-22.** First-playtest specification only; NOT implemented/tested in Unity. Final THIEF-036–040 decisions: Envenom Stealth opener multiplies ONLY the DIRECT stab packet (not subsequent attached poison ticks); guaranteed-basic-crit self-buff can be activated during Stealth without reveal, while its1–4s timer still runs normally; first breaking BASIC or Envenom attack attempt consumes opener bonus even if it misses or is fully shielded; potions and inventory/UI access do not reveal, while actual looting/NPC/world-object interaction does; mundane movement and incoming damage preserve Stealth. Approved 80-rank six-skill spec/cooldowns/Mana/rank gates and threat rules below. 3D appearance selection DEFERRED to separate working chat/Codex review of downloaded Rogue/Thief/Assassin/Ninja assets; advancement mentor/quest AFTER big city; Tier III left for World2. **PSION is NEXT active design. NO UNITY CODE, ASSETS, BUILDS OR WORLD CONTENT CHANGED HERE.**
**Branch:** `integration/world1-foundation`  
**Links:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II Core Classes v0.2](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Stats and Damage](../STATS_AND_DAMAGE.md), [Combat](../COMBAT.md), [Class Requirements](../CLASS_REQUIREMENTS_AND_DISCOVERY.md), [Ranger Tier II](RANGER_TIER2_DESIGN.md), [Completed Acolyte Tier II](ACOLYTE_TIER2_DESIGN.md).

## Confirmed World 1 design scope

Exactly eight ordinary Tier II classes in World 1:
- Physically Blessed Novice → Greatblade, Defender, Ranger, **Thief**.
- Magically Touched Novice → Mage, Priest, Acolyte, Psion.

**USER APPROVED CLASS AND SEQUENCE (2026-10-20; UPDATED 2026-10-22):** Acolyte and Thief Tier II gameplay designs are COMPLETE. **Psion is NOW the active design interview**, then Quillboar and the major city. After all eight normal Tier II designs, design the Quillboar area and major city. Some Tier II advancement mentors **MAY** move to the city, but their location and quests cannot be silently changed in this chat. **No Tier III class designing until World 2.** No secret class design during the current normal Tier II workflow.

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
| **Stealth** | **FINAL APPROVED rank16 & gates**, in-combat stealth lasting4→8s and a1.1×→2× damage multiplier on the first breaking BASIC attack OR Envenom DIRECT stab only; no poison tick multiplier. On cast reduce Thief threat95%; on exit enemies reacquire using retained5%+new threat, no old threat restoration. Incoming damage, movement, inventory/UI or potion use does NOT cancel; Crit buff activation also preserves concealment. BASIC or Envenom miss/fully shielded opening attempt consumes opener; Envenom/Fan/Smoke each reveal, Fan/Smoke without opener. Looting, NPC and world-object interactions reveal. | Fully approved gameplay first-pass; implementation details of target response/visibility are deferred |
| **Envenom** | **FINAL APPROVED rank18/gates/CD8s/12 Mana**. Direct non-elemental Physical stab RAW2×current basic attack, followed by 3s nonstacking Poison-tag Physical Nature DoT RAW/tick `(2+0.10×AGI)×(1+0.05×(r−1))`, one tick/s; reapply refreshes duration preserving tick phase; poison remains on moving enemy. If Envenom reveals from Stealth, opener multiplier1.1×→2× applies ONLY to DIRECT stab raw (at max rank16 Stealth: 4×normal basic RAW DIRECT), never Poison ticks. Opening attempt consumes bonus on miss/shield absorption. | Does NOT gain guaranteed-basic-crit buff's crit permission; no open gameplay parameters for initial baseline |
| **Critical buff** (working name) | **FINAL APPROVED rank10/gates/CD18s/16 Mana**, self-buff grants guaranteed1.5× RAW crits ONLY to normal BASIC autoattacks for 1s rank1→4s cap. **Can be cast while in Stealth WITHOUT REVEAL**, with duration ticking down normally in real time while hidden; no paused timer. | Cannot guarantee Envenom/Fan/Smoke skill crits; use ordinary buff expiration rules |
| **Fan of Knives** | **USER APPROVED cap18/gates/CD9s/18 Mana**, Thief-centered 4m/360°, 1 non-elemental Physical hit per enemy, usable while moving. **RAW per enemy `(8+0.45×AGI)×(1+0.06×(r−1))`**. Breaks stealth **without** stealth opener; only BASIC hit or Envenom gets opener | Target immunity and visuals subject to implementation |
| **Smoke Bomb** | **FINAL APPROVED rank10/gates/CD18s/20 Mana** ground-target 4m radius5s zone. Inflicts Physical Tech DoT while enemy INSIDE only, raw per0.25s tick `[(2+0.08×AGI)×(1+0.05×(r−1))]/4`, four ticks/s, stops IMMEDIATELY upon enemy exit. Enemies inside have20% miss chance on BASIC attacks only, instantly removed on exiting; overlapping smoke zones do NOT stack damage or miss. Casting Smoke reveals Stealth without opener. | Target tracking / overlapping caster source credit implementation follow-up |
| **Critical passive** (working name) | +1 AGI and +1 percentage point BASIC-attack crit chance/rank, crit 1.5× RAW, proposed cap8 yields +8 AGI/+8pp; buff temporarily guarantees basic crit | Future item crit permission interaction handled by general rules |

**Five active + one passive**

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

## USER-APPROVED Thief 80-rank schedule and level gates — Round7 (INITIAL PLAYTEST)

Six skills begin at rank0 at Tier II class level1, earn1 class skill point at class level1 and one per further level until80. Skill rank purchases require prior rank and its level gate. This newer schedule redistributes ranks to make **Envenom and Fan out-rank Stealth** as the user requested. **All six caps and their full eligibility gate schedules WERE USER APPROVED as the initial playtest allocation in THIEF-031**, after user delegated the arithmetic.

| Skill | **Approved maximum rank** | First eligible class level | Further upgrade interval | Last eligible gate | **Approved per-rank eligibility levels** |
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

### Rank-scaled examples under USER-APPROVED caps

- Stealth USER-APPROVED max16: each purchased rank after1 adds `4/15≈0.26667s` hidden duration and `0.9/15=0.06×` BASIC opener multiplier. Rank1 4s/1.1×; rank16 8s/2×. Threat drop 95% upon activation; damage taken does not reveal.
- Crit buff USER-APPROVED max10: 1→4s basic-only guaranteed crit, rank formula `1+3×(r−1)/9`; **activation does NOT reveal** and can be used while hidden to prepare a critical opening BASIC hit.
- Envenom USER-APPROVED max18: RAW Physical normal-basic-derived hit×2 plus three1s-ticks of Physical Nature Poison using approved RAW AGI coefficient and +5% increase per rank over1. Refresh never restarts periodic clock. **When Envenom breaks Stealth, its stab receives Stealth opener bonus (exact scope on DoT ticks to confirm).**
- Fan USER-APPROVED max18: 4m moving-usable 360° Physical single hit per enemy, **raw `(8+0.45×AGI)×(1+0.06×(r−1))`**.
- Smoke USER-APPROVED max10: four0.25s Physical Tech ticks/sec, raw/tick `[(2+0.08×AGI)×(1+0.05×(r−1))]/4`, nonstacking enemy zone effects.
- Crit passive USER-APPROVED max8: +8 AGI/+8 percentage points BASIC critical chance and 1.5× RAW normal-basic crits at cap.

## USER-APPROVED first-playtest cooldowns and Mana costs — Round7

| Active | **Approved cooldown** | **Approved Mana** | Balance purpose |
| --- | ---: | ---: | --- |
| **Stealth** | **24s** | **20** | 95% threat dump and 4–8s untargetability, powerful basic opener; avoid spamming |
| **Envenom** | **8s** | **12** | Repeatable single-target double-basic damage plus 3s nonstacking poison |
| **Guaranteed-Crit buff** | **18s** | **16** | 1–4s guaranteed-basic-crit buff; activating it **does NOT break Stealth**, useful before a basic opener |
| **Fan of Knives** | **9s** | **18** | Moving 4m AoE, one hit per nearby enemy |
| **Smoke Bomb** | **18s** | **20** | Five-second cloud DoT and 20% basic miss chance while inside |

**ALL FIVE above CD/Mana values ARE USER APPROVED (THIEF-032)** as first-playtest baselines and subject to later balancing. No Unity content was built in this chat. No Tier III design until World 2; after Thief design, Psion is next.

## Round 7 — FIVE USER DECISIONS RECORDED 2026-10-22

**THIEF-031 USER APPROVED EXACT FULL INITIAL PLAYTEST SKILL RANK CAPS AND GATES:** **Stealth16 / Envenom18 / guaranteed-basic-crit buff10 / Fan of Knives18 / Smoke Bomb10 / crit/AGI passive8 =80**. The entire numeric rank-gate table below is now APPROVED as the initial playtest schedule; no longer merely a designer suggestion. Skill ranks start0, one skill point per Tier II level1–80, last unlock gate at77, automated audit passed `eligible(L)≥L` for all integers1–80. Linear Stealth `duration(r)=4+4×(r−1)/15` seconds and `stealthOpenerMultiplier(r)=1.1+0.9×(r−1)/15`; rank1 4s/1.1×, rank16 8s/2×. Guaranteed-basic-crit buff rank1→10 duration `1+3×(r−1)/9` seconds (1s→4s), while the passive rank8 gives +8 AGI/+8 percentage points BASIC attack crit at1.5× RAW.

**THIEF-032 USER APPROVED ALL FIVE FIRST-PLAYTEST MANA/CD PAIRS:** **Stealth24s cooldown /20 Mana; Envenom8s/12 Mana; guaranteed-basic-crit buff18s/16 Mana; Fan of Knives9s/18 Mana; Smoke Bomb18s/20 Mana.** These are USER-APPROVED initial balance baselines, not implemented values.

**THIEF-033 USER APPROVED FAN RAW DAMAGE FORMULA:** For purchased Fan rank `r∈[1,18]`, **one non-elemental Physical RAW hit per enemy** within Thief-centered4m/360°, usable WHILE MOVING: `FanRawPerTarget=(8+0.45×currentThiefAGI)×(1+0.06×(r−1))`. This is a skill event, **not a basic attack**, so the basic-only guaranteed-crit buff/passive never automatically makes Fan crit. One damage event per target per cast.

**THIEF-034 USER APPROVED ENEMY REACQUISITION ON STEALTH EXIT:** Stealth activation once removes EXACTLY95% of Thief's threat from enemies' threat tables; this reduction PERSISTS when invisibility expires or breaks. Normal enemy target acquisition resumes based on the remaining5% plus NEW subsequent threat, NOT a restored95%, without resetting a boss encounter or other party member threat. The **first normal basic attack breaking Stealth** gets its Stealth rank-based opening-damage multiplier on that same revealing attack. Even if enemies cannot see or target the hidden Thief, this Thief can aim and strike enemies.

**THIEF-035 USER APPROVED AREA VS ATTACHED DOT:** Smoke Bomb's Physical Tech DoT **and** its20% BASIC-miss penalty apply ONLY while each enemy is PHYSICALLY INSIDE a live cloud; both end IMMEDIATELY upon leaving and resume as appropriate upon re-entry while cloud persists. Overlapping friendly clouds NEVER stack DoT or miss effects. By contrast Envenom's **attached three-second nonstacking Physical Nature Poison DoT** CONTINUES to tick on victim after moving away from the Thief and smoke. Normal DoT expiry/dispels still apply.

### Two EXPLICIT USER CORRECTIONS — NEWEST AND AUTHORITATIVE, overruling THIEF-030

**THIEF-036A — Crit buff activation DOES NOT BREAK STEALTH.** The guaranteed-basic-crit BUFF is a self-buff and may be activated **WHILE HIDDEN, retaining concealment and the ability to perform a buffed critical BASIC opener**. This directly REVERSES THIEF-030's earlier "activating buff breaks Stealth" statement; earlier combat/progression docs mentioning that are historical and superseded. The buff still only guarantees crits for **NORMAL BASIC ATTACKS**, never Envenom/Fan/Smoke, unless a later user decision expands eligibility. Buff lasts1s rank1 to4s rank10; exact expiration timing continues to use ordinary buff rules. Incoming damage also DOES NOT reveal, as already confirmed.

**THIEF-036B — ENVENOM BREAKING STEALTH DOES RECEIVE THE STEALTH OPENING MULTIPLIER.** This directly REVERSES THIEF-030's "only BASIC can receive opener" and "Envenom breaks without opener" restrictions. A normal basic attack **OR** Envenom skill stab that breaks Stealth should get the Stealth rank1→16 1.1×→2× opener multiplier. **Working implementation interpretation (ASK NEXT ROUND): apply multiplier to Envenom's DIRECT 2×normal-basic RAW stab packet ONLY, NOT to attached 3-second Physical Nature Poison DoT ticks**, to avoid multiplying its entire poison duration for a single reveal. Thus `StealthEnvenomDirectRaw = 2×normalBasicRaw×stealthOpenerMultiplier(stealthRank)`, before ordinary Physical mitigation; at max Stealth rank that's 4×normalBasicRaw DIRECT before mitigation. User approved Envenom receiving bonus, not specifically DoT eligibility, so direct-only interpretation is a **RECOMMENDATION PENDING THIEF-036** rather than falsely labeling this packet scope confirmed. Other combat abilities Fan/Smoke still break invisibility and do NOT receive opening bonus. Crit buff does NOT break and does NOT consume opening bonus.

**Important:** Old decisions are retained below as history, but Round7 (including new corrections) is AUTHORITATIVE. No Unity code, assets, scenes, builds, NPCs or quests modified in this design chat.

## Thief first-playtest status — nearly complete

- **Approved 80 ranks:** Stealth16, Envenom18, crit buff10, Fan18, Smoke10, passive8; exact gate schedule as detailed below.
- **Approved Mana/CD:** Stealth24s/20, Envenom8s/12, crit buff18s/16, Fan9s/18, Smoke18s/20.
- **Approved damage formulas:** Envenom direct2×normalBasicRaw; Physical Nature Poison DoT RAW/tick `(2+0.10×AGI)×(1+0.05×(r−1))` every1s for3s; Fan Physical RAW/target `(8+0.45×AGI)×(1+0.06×(r−1))` one hit per target; Smoke Physical Tech RAW per0.25s tick `[(2+0.08×AGI)×(1+0.05×(r−1))]/4` while inside 4m/5s cloud, overlapping zones nonstacking.
- **Stealth mechanics:** in-combat, enemy cannot see or target; takes damage without reveal; cast reduces stored Thief threat95%; on expiry retain5%+future threat; first BASIC or Envenom breaking concealment earns opener bonus, Fan/Smoke reveal without bonus, CRIT BUFF DOES NOT REVEAL.
- **Passive:** +1 AGI and +1pp BASIC attack crit chance per rank to8, eligible basic crit at1.5×; timed crit buff guarantees ONLY basic attack crits, active stealth-compatible.
- **Open implementation questions:** direct-only vs poison ticks for stealth-Envenom bonus; miss/failed target handling; buff timer/overlap ordering; whether mundane items/interactions cancel stealth; multi-caster smoke overlap and update clocks. The named Thief character visual is deferred to working chat/Codex inspecting user's assets; advancement quest remains deferred until after designing major city. Future Tier III and IV ideas remain archived only.

## FINAL ROUND 8 — FIVE USER DECISIONS CONFIRMED 2026-10-22 (AUTHORITATIVE)

- **THIEF-036 — Direct Envenom opener ONLY.** When Envenom causes Stealth to break, apply the rank-scaled Stealth opener damage multiplier to the **instant direct dagger attack damage packet** and **NOT** to the attached three-second Physical Nature Poison DoT damage ticks. Formula with `S`=purchased Stealth rank `1..16`: `stealthOpeningMult(S)=1.1+0.9×(S−1)/15`. Breaking Envenom direct RAW `2×currentNormalBasicRaw×stealthOpeningMult(S)`, so at Stealth rank16 this is `4×currentNormalBasicRaw` before defenses. The poison uses its ordinary AGI+Envenom-rank raw tick formula and is unaffected by stealth opener.
- **THIEF-037 — Crit buff timer is NORMAL REAL-TIME while hidden.** Activating the 1s→4s basic-only guaranteed-critical self-buff **DOES NOT break Stealth**, but its duration continues decreasing normally while Thief remains hidden; no pause, freeze, refunded time or extension. It remains a normal-basic-only critical permission even if the player next breaks stealth with Envenom.
- **THIEF-038 — Opening BONUS CONSUMED by first resolved attack attempt**, even if that breaking BASIC or Envenom attempt **MISSES, is fully shield-absorbed, is blocked or causes zero HP damage**. Do not preserve the opener waiting for a successful HP-damaging hit. The attack must be a valid attempted first attack from Stealth; ordinary no-target invalid input behavior follows shared controls and is not newly specified. This preserves dodge/shield counterplay, and the Stealth-breaking action still reveals the Thief.
- **THIEF-039 — Mundane Stealth-preserving versus breaking interactions.** NORMAL MOVEMENT and TAKING DAMAGE preserve Stealth (previously confirmed). Opening inventory, ordinary UI navigation and USING POTIONS also **PRESERVE** Stealth. **ACTUAL LOOTING, TALKING TO NPCs and INTERACTING WITH WORLD OBJECTS** break it, with NO stealth opener damage granted for interaction. Activating guaranteed-crit self-buff preserves Stealth. Envenom BASIC attacks reveal and get opener, Fan/Smoke reveal with none. Don't invent invisibility for Smoke.
- **THIEF-040 — USER DECLARED THIEF TIER II GAMEPLAY DESIGN COMPLETE for initial playtest** and approved moving NEXT to **Psion** as eighth/final World1 normal Tier II class. Do **NOT** treat Thief implementation/playtest as finished. **Class appearance/model selection remains DEFERRED to separate working implementation chat/Codex** after examining user's existing Rogue/Thief/Assassin/Ninja downloaded models; **advancement mentor/quest deferred until AFTER the major city**. All Tier III/IV class paths (Burglar→Pirate, Duelist, Assassin and Berserker+Duelist secret class) are brainstormed FUTURE World2+ ONLY. No Unity work authorized in this design interview.

### Final first-playtest specification — latest corrections override historical text below

| Skill | Confirmed first-playtest baseline |
| --- | --- |
| **Stealth, max16** | 24s CD /20 Mana; combat-usable4→8s, ignores incoming damage for reveal; reduces Thief threat95% once, hidden untargetable, enemy reacquisition using retained5%+new threat. BASIC or Envenom opener direct1.1×→2× rank-scaled, used on first attempt even on miss/shield absorption; Envenom poison never amplified. UI/inventory/potions/movement preserve; world interaction/loot/NPC speech break. |
| **Envenom, max18** | 8s CD/12 Mana. Instant ordinary Physical raw dagger stab2×basic RAW; from Stealth apply direct-only opener damage bonus. Nonstacking attached3s Physical Nature Poison-tag DoT raw/tick `(2+0.10×AGI)×(1+0.05×(r−1))`, ticks once/s and keeps tick phase upon refreshed duration; persists regardless of victim movement. Envenom is a skill hit and NOT a basic-only guaranteed crit. |
| **Guaranteed basic-crit buff, max10** | 18s CD/16 Mana. Only normal BASIC attacks guaranteed1.5×RAW crit, duration1s→4s linearly by rank. **Casting buff while hidden preserves Stealth; buff time ticks normally while hidden.** |
| **Fan of Knives, max18** | 9s CD/18 Mana. 4m 360°, one non-elemental Physical RAW packet per target `(8+0.45×AGI)×(1+0.06×(r−1))`; castable while moving; breaks Stealth without opener; no basic crit permission. |
| **Smoke Bomb, max10** | 18s CD/20 Mana. Ground-target4m zone lasts5s, Physical Tech DoT RAW per0.25s tick `[(2+0.08×AGI)×(1+0.05×(r−1))]/4`; ONLY targets physically in zone are damaged, and cloud overlap never stacks. 20% miss chance on enemies' BASIC attacks only while in zone; miss/DoT end on exit immediately. Casting reveals without opener. |
| **Crit+AGI passive, max8** | +1 AGI and +1 percentage point crit chance on normal BASIC attacks per rank; 1.5×RAW basic crit. Self-buff grants temporary basic-only guaranteed crit, not skill or poison DoT critical damage. |

### Exact rank distribution and approval

`Stealth16 + Envenom18 + CritBuff10 + Fan18 + Smoke10 + CritPassive8 =80`. User approved the six caps, per-skill rank eligibility gates in the already-authored table, initial cooldown/Mana and ALL approved damage baselines. Skill ranks start0 for new Tier II class, one class skill point per level1–80, **all80 levels have a legal point purchase**; mathematically audited and last rank gate77.

**Scope:** Mark the **design interview complete, not any Unity implementation, asset, save migration, test harness, NPC quest, battle test or class availability**. Class model and advancement quest are explicit future handoff items, not reasons to reopen Thief's gameplay interview.

**Next:** Psion Round1 interview in [Psion Tier II](PSION_TIER2_DESIGN.md), then after final eighth Tier II normal class, Quillboar area and major city. Do not design Tier III before World2.

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
| 2026-10-22 | THIEF-031 | Full caps/gates 16/18/10/18/10/8=80, last gate77, purchases available all class levels1–80 | **USER APPROVED INITIAL PLAYTEST** |
| 2026-10-22 | THIEF-032 | CDs/Mana: Stealth24s/20, Envenom8s/12, crit buff18s/16, Fan9s/18, Smoke18s/20 | **USER APPROVED INITIAL PLAYTEST** |
| 2026-10-22 | THIEF-033 | Fan Physical RAW per enemy `(8+0.45×AGI)×(1+0.06×(rank−1))` | **USER APPROVED INITIAL PLAYTEST** |
| 2026-10-22 | THIEF-034 | On Stealth exit enemy targeting resumes on retained5% threat +new threat, never restores removed95%; opening BASIC applies multiplier while revealing | **USER APPROVED** |
| 2026-10-22 | THIEF-035 | Smoke Physical Tech damage AND miss ends instantly on exiting cloud; Envenom attached Physical Nature poison persists on target | **USER APPROVED** |
| 2026-10-22 | THIEF-036A | Activating basic guaranteed-crit buff PRESERVES Stealth, correcting THIEF-030 | **USER EXPLICIT CORRECTION APPROVED** |
| 2026-10-22 | THIEF-036B | Envenom breaking Stealth gets opening attack damage multiplier, correcting THIEF-030 | **USER EXPLICIT CORRECTION APPROVED; damage packet scope open** |
| 2026-10-22 | THIEF-036 | Stealth Envenom multiplier applies ONLY to initial direct non-elemental Physical stab, not Poison ticks; max cap4×basic RAW DIRECT | **USER APPROVED** |
| 2026-10-22 | THIEF-037 | Crit buff DOESN'T break Stealth but timer runs normally in real time while hidden | **USER APPROVED** |
| 2026-10-22 | THIEF-038 | First breaking BASIC or Envenom attack attempt consumes opener on miss/full shield absorption/zero HP damage | **USER APPROVED** |
| 2026-10-22 | THIEF-039 | Potions/UI/inventory/movement preserve Stealth; actual looting/NPC/world-object interaction breaks | **USER APPROVED** |
| 2026-10-22 | THIEF-040 | Thief TIER II design interview COMPLETE; move to Psion; visuals and advancement quest deferred | **USER APPROVED COMPLETE DESIGN, NOT IMPLEMENTED** |

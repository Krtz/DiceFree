# DiceFree — Thief Tier II — design interview

**Created:** 2026-10-20  
**Status:** NORMAL WORLD 1 TIER II THIEF — **ROUNDS 1–3 USER DECISIONS CONFIRMED (2026-10-21)**. Combat-usable true Stealth drops enemy aggro/visibility; approximately 4s rank1 to8s max; first stealth-breaking normal basic auto gains suggested **1.1× rank1 → ~2.0× max** damage multiplier (values accepted as initial targets, exact cap/growth still adjustable). Instant Envenom poison stab; separate temporary GUARANTEED-CRIT BUFF (duration/eligible attacks open); Fan of Knives is 360° around Thief; ground-targeted Smoke Bomb area DoT causes MISS CHANCE to ENEMY BASIC ATTACKS ONLY for enemies inside. Mana only, AGI-only offensive coefficients, two stat-only daggers with ONE alternating-animation basic attack event, passive crit chance and possible bonus AGI. Appearance model/asset choice deferred to working chat/Codex; advancement quest AFTER major city. **DESIGN/DOCUMENTATION ONLY; NO UNITY C#/SCENE/ASSET/BUILD/SAVE CHANGES.**  
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
| **Stealth** | **Combat-usable** concealment lasting ~4s first rank to~8s max; enemies **lose aggro and cannot see Thief**. First normal/basic attack that BREAKS stealth deals damage at ~**1.1× rank1 → 2× max** as user's initial target, no rear-positional bonus | Precise max rank/growth, duration, Mana/CD, damage-breaking stealth, debuff/AI boss-threat re-acquisition on reveal, whether aggro erased vs suspended OPEN |
| **Envenom** | **An instant melee poison DAGGER STAB** against target, distinct from coating a string of later basics or ranged poison application | Does stab apply small poison DoT, instant direct damage, or both? Poison status tag/element, poison timing/stacking, AGI coefficient, Mana/CD OPEN |
| **Critical buff** (working label; earlier Critical Strike as attack superseded) | **Temporary BUFF granting GUARANTEED CRITICAL HITS for x seconds**, not a single instant critical strike skill. User did NOT specify x duration or whether it affects each eligible basic attack/all Thief eligible abilities versus only next hit while buff is running | Duration, action/crit eligibility, baseline crit multiplier, coexistence with crit passive, Mana/CD and rank growth OPEN |
| **Fan of Knives** | **Thief-centered, FULL 360-DEGREE knife AoE**, not forward cone. Distinct from dagger autoattacks | Radius, one hit per enemy vs more, physical damage packets/AGI scaling, animations/projectile visuals, Mana/CD OPEN |
| **Smoke Bomb** | **Ground-targeted lingering AoE cloud with damaging DoT**; enemies **INSIDE cloud** have MISS CHANCE on **THEIR BASIC/AUTOATTACKS ONLY**. Does NOT grant spell/ability miss chance | Cloud radius/height, duration, periodic damage channel/element, exact miss rate, boss Tenacity/resists, edge/leave behavior, Mana/CD OPEN |
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

## Round 4 — EXACTLY FIVE questions (AWAITING USER)

**THIEF-016 — Stealth reveal/aggro:** Should **taking damage immediately reveal Thief** while hidden, and do monsters' stored threat values RESET to zero on entering Stealth or should they only temporarily **drop Thief as a target** until reappearing (safer for boss encounter scripting)? I suggest damage breaks stealth and target acquisition drops, with boss battle progression continuing.

**THIEF-017 — Stealth rank curve:** We have approximately **4s rank1 → 8s at cap**, and opening basic hit **1.1× rank1 → roughly 2.0× at max rank**. Do you want these to increase smoothly each rank, or in visible whole-second duration steps? Should the multiplier increase linearly in rank? We can choose max skill rank when creating the full 80-point schedule.

**THIEF-018 — Envenom poison mechanics:** Should the instant AGI-scaled dagger stab also apply a **small, nonstacking Poison-tagged DoT** to the target? If yes, which DAMAGE ELEMENT should it use—**Coffee**, **Radiation**, or **non-elemental Physical/Magical**? (A status tag like Poison is distinct from the damage element; don't assume Acolyte's Coffee poison automatically applies to Thief.)

**THIEF-019 — Guaranteed crit buff:** Should the timed guaranteed-critical buff give **EVERY normal dagger autoattack** within its duration a guaranteed crit (I recommend yes), and should it also guarantee critical hits for **Envenom and Fan of Knives**, or exclusively basics? Initial duration suggestion **3 seconds at rank1**, with extra duration or stronger crit multiplier through ranks.

**THIEF-020 — Smoke Bomb accuracy/DOT:** Should the miss chance apply to enemies **only while physically inside the ground-targeted cloud**, with attacks returning to normal immediately on leaving? For first-pass tuning I'd propose a **4m radius, 5s duration, 20% enemy-basic miss chance**, plus a weak AGI-scaling DoT. Do you prefer the cloud damage to be **Physical** (e.g. irritant dust) or Magical with a chosen existing element? Exact smoke effect is not yet approved.

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
| 2026-10-21 | THIEF-016–020 | Round4 five questions | **AWAITING USER** |

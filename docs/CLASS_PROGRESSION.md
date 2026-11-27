# Class Progression

> **Implementation checkpoint — 2026-11-27:** Mage → Fire Mage now follows the approved level60 parent → separate level1 TierIII flow, with the Ember Crown mentor and solo escape trial. See [implementation and validation](FIRE_MAGE_IMPLEMENTATION_2026-11-27.md). Historical design-only rounds below remain as decision history.

**2026-11-25 MARKSMAN TIER III ROUND7, NEWEST USER-APPROVED DECISIONS / GLOBAL MANA CORRECTION:** The EXACT 150 individual Marksman skill-rank level-gate sequences authored and mathematically audited in [Marksman Round6](classes/MARKSMAN_TIER3_DESIGN.md) are NOW USER APPROVED (previous “pending” text superseded): Aimed30/Rain25/Bow20/Strafe30/Aura20/Power25, 150 total rank purchases, one point per Marksman class level1–150; all150 accessible by level148, first unlocks Aimed1/Bow1/Strafe1/Rain5/Power7/Aura10. **Every offensive Marksman skill must increase its FLAT and AGI DAMAGE COEFFICIENTS with THAT SKILL'S purchased rank**, not merely add a rank-flat bonus. Updated designer-provisional explicit formulas in [Round7](classes/MARKSMAN_TIER3_DESIGN.md): Aimed `15.2+0.60(r−1)+(2.15+0.091(r−1))AGI`, Power first target `9.2+0.4(r−1)+(1.25+0.058(r−1))AGI`, Arrow Rain EACH of6 waves `3.2+0.176(r−1)+(0.4+0.022(r−1))AGI`; numeric damage coefficients remain proposals awaiting review. User now sets the **parent Ranger normal bow RAW** `base+1.0×AGI` (actual current Ranger bow asset is base5 +1.05×AGI, so Unity change remains to be made), with proposed Marksman normal bow `8+1.0×AGI` pending its base value approval. **CRITICAL UNIVERSAL MANA RULE:** ALL Mana-using classes must follow ONE SAME global Mana maximum and regeneration coefficient formula. Tune class throughput using **base/starting Mana, base/starting INT, INT gain per level and gear/item stats**; do NOT author special per-class Mana-per-INT, Mana-per-SPR or regeneration multipliers for Marksman or other classes. The actual shared Unity `ClassResourceProfile` calculation code currently reads CLASS-SPECIFIC numeric coefficients from Mana assets, so gameplay implementation needs a coordinated normalization/refactor; numeric universal constants must be verified/chosen, do not invent or claim already implemented. **The Round6 Marksman-exclusive `180+5INT+3SPR` max and `4.5+.075INT+.075SPR` regen formulas are explicitly WITHDRAWN**. User approved only the proposed **skill Mana COSTS and cooldowns "for now"** (Aimed7s/`18+12r`; Rain22s/`20+15r`; Power12s/`18+13r`; Strafe18s/`10+9r` once on activation; passives zero), and baseHP20/+15HP per VIT. User approved **hidden Marksman teacher → fresh SOLO precision trial → return to teacher to create preserved-parent TierIII Lv1 manifestation**, details undecided; not a copy of Ranger's Alpha Wolf or Psion's dungeons. Parent Ranger and existing Unity actors/assets NOT edited during docs-only round. See [COMBAT](COMBAT.md), [STATS AND DAMAGE](STATS_AND_DAMAGE.md), [Marksman Round7](classes/MARKSMAN_TIER3_DESIGN.md). **This latest universal Mana rule supersedes older notes treating Psionic Knight/Mirage Mage's per-class Mana coefficients as a permanent global exception**; coordinate rebalancing rather than silently rewriting existing playable classes.

**2026-11-25 MARKSMAN TIER III ROUND6, LATEST USER-APPROVED ATTRIBUTES AND ROLE, WITH AUDITED DESIGNER BALANCE PROPOSALS:** Exact USER APPROVED Marksman new Lv1 class bases **VIT19 STR10 AGI34 INT9 SPR11**, and per-level growth **VIT+2.00 STR+0.90 AGI+6.00 INT+1.00 SPR+1.20** (total level1 83, level growth11.10), against actual Ranger level1 55/growth7.25. At Marksman Lv150 before Mastery/gear, **VIT317 STR144.1 AGI928 INT158 SPR189.8**; Bow Mastery20 adds its already approved +60 AGI → **988 effective AGI before equipment**. User confirms **6 Arrow Rain waves primarily increase AoE damage versus 4-wave Ranger**, retaining comparable individual wave values rather than dividing old total across six; user confirms **Aimed Shot much stronger single-target** than the first hit of **Power Shot**, which is line-multitarget. User explicitly authorized designing first-test raw damage/cooldowns/Mana pools and the FULL 150-skill-rank eligibility table. **DESIGNER FIRST-PLAYTEST PROPOSALS (NOT USER-APPROVED):** basic Physical bow RAW `B=8+1.20AGI` before item bonuses; Aimed RAW `[1.90+0.075(r−1)]B+[0.25+0.016(r−1)]AGI`; Power first RAW `[1.15+0.05(r−1)]B+[0.10+0.008(r−1)]AGI` then ×0.8 per subsequent enemy; Arrow Rain EACH of6 waves RAW `[0.400+0.022(r−1)]B` (~0.928B per wave cap25 vs parent ~0.925B per wave cap16); Aimed CD7s and Mana `18+12r`, Rain CD22s and Mana `20+15r`, Power CD12s and Mana `18+13r`; Strafe CD18s already USER APPROVED, Mana `10+9r` upfront PROPOSED; maxMana `180+5INT+3SPR`, Mana regen/s `4.5+.075INT+.075SPR`, baseHP20 & +15HP/VIT PROPOSED. At level150 no equipment full Mana1539.4 and regen30.585/s, one Aimed+Rain+Power+Strafe at max costs1396Mana before regen. **EXACT 150-rank gate PROPOSAL GENERATED AND AUDITED** in [Marksman Round6](classes/MARKSMAN_TIER3_DESIGN.md), with exact approved unlock levels Aimed1/Rain5/Mastery1/Strafe1/Aura10/Power7 and caps Aimed30/Rain25/Mastery20/Strafe30/Aura20/Power25=150; every skill gate strictly increases; every class level L1..150 has cumulative eligible rank count≥L; final unlock rank at level148. **Audit passed mathematically**, but this does NOT constitute user acceptance of specific gate integers, code implementation, or combat balance. Next stage approve/tune proposals then design Marksman mentor/solo precision trial, then Greatblade→Berserker. Parent Ranger unchanged. **Documentation only, no Unity edits.**

**2026-11-25 MARKSMAN TIER III ROUND5 LATEST USER APPROVAL:** Bow Mastery `+1.5 percentage points crit chance per purchased rank`, up to **+30pp** at rank20; **1.6× total critical damage** (overrides parent Ranger 1.4× and the formerly ambiguous "15x%" typo), crit permission for **NORMAL BASIC bow attacks and Aimed Shot only**. Bow Mastery still adds +3AGI/rank and +0.2m basic attack range/rank (11m→15m). **Arrow Rain** keeps the parent 1.5s stationary interruptible channel and 3m circle but now produces **SIX DAMAGE WAVES OVER TWO SECONDS**, continuing autonomously once channel completes. **Aimed Shot** remains 1.5s windup, +`(10%×AimedRank/30)` PDEF bypass up to10%, ordinary eligible crit **and CANNOT MISS via ordinary accuracy/evasion rolls** (valid target and regular obstacle/interrupt rules still matter). **Power Shot** line projectile hits all distinct eligible enemies intersecting its unobstructed **1.5m width** path with **no miss/evasion roll**, but stops at solid world obstacles; remains NONCRITICAL, range=basic+1m=12m→16m, RAW falloff by0.8 per subsequent enemy. **Strafe 18s cooldown**, one rank-scaled Mana payment on ACTIVATION (not on every hit); duration and equal +2%AS/−2%MS per rank unchanged. **Stats direction USER APPROVED:** Marksman higher starting stats and total level growth than actual Tier II Ranger, strongly AGI + meaningful VIT; the **EXACT numeric stats are proposed, not accepted** in [Marksman Round5](classes/MARKSMAN_TIER3_DESIGN.md): Lv1 VIT19/STR10/AGI34/INT9/SPR11; growth+2/+0.9/+6/+1/+1.2, total11.10. Exact Mana/HP formulas, raw damage, individual 150 skill-rank PURCHASE gates and mentor/trial remain open. Existing approved six ranks **Aimed30/Rain25/Mastery20/Strafe30/Aura20/Power25** and unlock Lv1/5/1/1/10/7 unchanged; all skill points one per Marksman level through150. Historical uncertainty about Bow Mastery 1.5%, old Ranger crit×1.4 or four-wave Rain is superseded. Design docs only, no Unity changes. [Complete Round5](classes/MARKSMAN_TIER3_DESIGN.md).

**2026-11-25 MARKSMAN TIER III ROUND4 LATEST USER APPROVED:** Aimed Shot is a **1.5s stationary precision windup** and ignores **exactly `(10%×AimedShotRank/30)` of target PDEF on that shot only** (10% maximum at rank30), using its **ordinary eligible crit chance and ordinary crit damage**, with NO Aimed-specific bonus crit. Power Shot **CANNOT CRIT**, retaining 1.5m width, physical line piercing, blocked by solid walls, range auto+1m (12–16m) and 80% compound raw damage per further enemy. **Bow Mastery +3 AGI per rank** (20 ranks gives +60 AGI), +0.2m basic reach/rank. **IMPORTANT CRIT NOTATION UNRESOLVED:** user wrote literal `15x% crit per rank`, which might mean +1.5 percentage points/rank, but must NOT be committed as a numeric formula until the user clarifies; parent-style **1.4× crit multiplier** and crit permission for BASIC + AIMED remain working reference. **Ranged Damage Aura +1% per rank now USER APPROVED**, cap20 grants +20% to **ALL ALLIED RANGED NORMAL/BASIC ATTACKS including Marksman self**, no skills, same-name highest only, radius follows effective attack reach11–15m as Bow Mastery ranks up. **Skill first-unlocks USER APPROVED:** Aimed Shot Lv1, Bow Mastery Lv1, Strafe Lv1, Arrow Rain Lv5, Power Shot Lv7, Ranged Damage Aura Lv10. The skill rank caps remain Aimed30/Rain25/Mastery20/Strafe30/Aura20/Power25=150, one rank point per Marksman level1–150. Detailed per-purchase level gates are still to author/audit; raw damage, Mana, cooldown, HP/base stats and advancement quest remain unconfirmed. Earlier Round3 sections are historical; see [Marksman Round4](classes/MARKSMAN_TIER3_DESIGN.md). Design docs only, not implementation.

**2026-11-25 Marksman Tier III Round3 LATEST USER-APPROVED SIX INDIVIDUAL RANK CAPS:** **Aimed Shot30 / Arrow Rain25 / Bow Mastery20 / Strafe30 / Ranged Damage Aura20 / Power Shot25 = EXACTLY 150**, at one spendable class point per Tier III Marksman level1–150. Rank transfers applied in user-specified order: Strafe +2 from Aimed, Aura +2 from Aimed and +2 from Arrow Rain, Power Shot +1 from Arrow Rain. **CONFIRMED Strafe per purchased rank:** +2% attack speed and **−2% movement speed** for `ceil(1+r/5)` seconds, at rank30 +60%/-60% for7s. The **permanent aura** affects **ALL allies' ranged BASIC/AUTOATTACK damage ONLY**, **NOT SKILLS**, and multiple instances with the same aura name **DO NOT STACK** (strongest active wins). Its radius equals the Marksman's current ranged autoattack reach, 11m + 0.2m per Bow Mastery rank, capped15m at Mastery20. **Aimed Shot** retains slow precision windup and PERCENTAGE-based PDEF bypass; user explicitly REMOVES previously proposed special critical chance/critical damage bonuses, using **normal Bow-Mastery-eligible crit chance and normal crit damage** on the shot instead. Around **10% PDEF ignored at Aimed30** is a user-floated target, NOT a finalized exact per-rank curve. **Power Shot** is a **1.5m-wide physical piercing line** that STOPS at solid walls, with **12m base +0.2m per Bow Mastery rank => 16m at Mastery20**, exactly +1m versus corresponding autoattack range; successive distinct enemies take 80% raw of the prior target's hit before independent mitigation (100%,80%,64%,...). All Mana, damage, cooldown, unlock levels, exact per-rank eligibility gates, teacher/quest and actual implementation are still open. Round2 historical claim that Aimed gains its own crit bonus is SUPERSEDED. [Marksman Round3 source of truth](classes/MARKSMAN_TIER3_DESIGN.md). No Unity changes.

**2026-11-25 Marksman Tier III Round2, LATEST USER-APPROVED SIX-SKILL ROSTER AND 150-POINT CADENCE:** Ranger→Marksman is a stationary AGI-focused ranged damage specialist with **11m class bow base reach extending to max15m through Bow Mastery skill ranks**, longbows favored but shortbows legal, **NO Hunter's Mark/Hawk Dive**. SIX approved skills: **(1) Aimed Shot** becomes a longer-windup precision arrow with **increased crit chance AND Physical Defense penetration**, (2) **Arrow Rain** retained and evolved, (3) **Bow Mastery** retained/rankable to raise range11m→15m, (4) new **Strafe** active temporarily increases attack speed and lowers movement speed by **EXACTLY THE SAME PERCENTAGE** (suggested +2%/rank, `ceil(1+r/5)` seconds not yet numerically finalized), (5) **permanent RANGED DAMAGE AURA** whose radius **ALWAYS equals Marksman's current effective basic bow attack reach**, automatically increasing with Bow Mastery ranks; original +1% eligible ranged damage/rank proposed but which allies/attack types benefit still OPEN, (6) **Power Shot**, a piercing Physical arrow hitting successive enemies in a line for **80% of the previous enemy's RAW damage** in collision order, first100%, second80%, third64%, etc., then normal per-victim mitigation. User explicitly approves **exactly ONE skill point for EACH Marksman class level1..150 (150 total)**, with exact individual caps and eligibility gates STILL TO DESIGN/AUDIT. All six skill identities are APPROVED even though aura and Power Shot were initially tentative in Round1. Marksman's parent Ranger is unchanged; all six new skill Rank0 states and Tier III Lv1 new preserved manifestation follow established class system. Detailed current [Marksman Round2](classes/MARKSMAN_TIER3_DESIGN.md). **Documentation design only, no Unity changes**.

**2026-11-25 Marksman Tier III Round1 approved direction, LATEST:** Ranger→Marksman is a **stationary bow-damage specialist**, normal range **11m at baseline upgraded via Marksman skill ranks to max 15m**, longbow-focused but shortbow-compatible. Mark/Hawk Dive NOT used. **Aimed Shot, Arrow Rain, Bow Mastery retained**, Strafe active attack-speed buff with movement penalty proposed, ranged-damage aura and piercing falloff Power Shot floated as tentative additions. Remove Ranger's old marked-target prerequisite from evolved Marksman Aimed Shot once new independent damage behavior is designed; original Tier II Ranger mechanics must remain unchanged. Range skill cap, six-skill roster, 150-point progression, resource formula, base stats, gear family authoring and advancement trials still OPEN. Full [Marksman Round 1](classes/MARKSMAN_TIER3_DESIGN.md), coverage [roadmap](TIER3_ONE_PER_TIER2_ROADMAP.md). Berserker remains next. Documentation only.

**2026-11-25 LATEST USER-AUTHORIZED TIER III COVERAGE ROADMAP:** Aim to offer **AT LEAST ONE Tier III evolution to EVERY one of the eight standard Tier II classes**, before building more optional specialization branches. **Do not interpret this as limiting a Tier II class to one Tier III forever.** User reports **Psion → Psionic Knight and Mirage Mage already fully implemented** (report only; no QA performed in this documentation pass). **NEXT DESIGN ROUNDS:** (1) **Ranger → Marksman**, (2) **Greatblade → Berserker**, then decide first Tier III evolution for the still uncovered **Defender, Thief, Mage, Priest and Acolyte**. Their previous candidate names are idea banks, NOT approvals or finalized kits. Ranger→Marksman is opened for design with its class' accurate 11m current bow reach and other Tier II details, sourced from current Unity assets; see [Marksman Tier III](classes/MARKSMAN_TIER3_DESIGN.md) and [Tier III coverage roadmap](TIER3_ONE_PER_TIER2_ROADMAP.md). The existing Tier II Lv60 → new Tier III Lv1 preserved manifestation system remains intact. No skill caps, class stats/skills, Mana, specific Rank-gate tables or quests for Marksman were approved in this message. **Design only**, no Unity/asset merges.

**2026-11-24 Psion Tier III Round22, FINAL USER APPROVAL FOR FIRST PLAYTEST (OVERRIDES prior Round20/21 'pending' text below):** The user replied **"I agree on everything with you!"** to the prior complete Round21 proposals. **CONFIRMED LEVEL1 AND GROWTH:** Knight VIT22/STR8/AGI30/INT22/SPR11, growth **+2.50/+1.00/+5.00/+2.70/+1.00** (total +12.20/level); Mage VIT19/STR8/AGI22/INT32/SPR11, growth **+2.00/+1.00/+2.70/+5.00/+1.00** (+11.70/level). **CONFIRMED HP initial test:** Knight baseHP30, Mage20, both +15HP/VIT and +0.1 regen/VIT. **CONFIRMED Mana test formulas:** Knight maximum `140+4INT+1.5SPR`, regen/sec `3.5+0.045INT+0.035SPR`; Mage maximum `160+5.5INT+2SPR`, regen/sec `4+0.07INT+0.055SPR`. Both unmodified base move speed5, base PDEF/MDEF0; actual stat engine and equipment apply. **CONFIRMED Knight empowered raw Magical Void initial damage formula:** `30+17r+[0.4+0.016(r-1)]AGI+[0.1+0.004(r-1)]INT`, SUPERSEDING old fixed-coefficient formula above rank1, one packet separate from ordinary Physical basic. **CONFIRMED combat-edge test rules:** empowerment misses/evasion consume neither Mana nor charges; landed contact incl full shield absorb consumes both; no Mana => unempowered normal basic, no charge lost; ordinary physical basic may crit but separate Magical Void bonus cannot; Knight illusion uses independent copied remaining charges/buff effectiveness and NO owner Mana; recasts replace charges; image exit haste strongest-only, equal refreshes, discarded if Knight dead; Mirage Mage mirrors snapshot on original cast and echo after 0.4s if source alive, retarget enemy upon original death, no extra Mana/stun, oldest eligible shield pool first, mirror shield 8m around image, bounded secondary images and death survival. Prior higher per-rank Mage spell/shield costs and cooldowns also approved as initial values for testing, not final tested balance. Read exact detailed [Psion Tier III Round22](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md). **These values are user-approved for first playtest, NOT evidence that Unity is implemented or the balance has been validated.** All earlier "proposal/pending user approval" language for these specific items is historical and overridden.

**2026-11-24 Psion Tier III Round21, LATEST USER-SELECTED growth and Codex testing handoff:** Knight level1 base attributes are **VIT22 STR8 AGI30 INT22 SPR11** (retained from Round20 candidate; user has NOT explicitly finalized level1 numeric stats). **CONFIRMED new Knight per-level growth VIT+2.50 STR+1.00 AGI+5.00 INT+2.70 SPR+1.00 (sum12.20)**. Mage level1 base is **VIT19 STR8 AGI22 INT32 SPR11** (unchanged working proposal). **CONFIRMED new Mage per-level growth VIT+2.00 STR+1.00 AGI+2.70 INT+5.00 SPR+1.00 (sum11.70)**. User's "increase agi for mage by 1 and int for knight by 1" is interpreted against prior GROWTH table, NOT as changes to starting base values; both "Str and spi to 1 per level" and "Vit to 2.5 for Knight" confirm this context. Parent Tier II Psion actual growth total is 8.35; both descendants substantially exceed it. The older "both STR/SPR +0.20" proposal is SUPERSEDED, as is Knight VIT2.30/Mage AGI1.70/Knight INT1.70. All approved 150 rank gates, unique base attack scaling and quest/teacher flow remain unchanged. 

**Codex implementation concerns:** [Tier III Round21](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md) now provides a first-playtest **DESIGNER-PROVISIONAL** per-class resource profile, Knight baseHP30/Mage20, Knight `ManaMax=140+4INT+1.5SPR` and `ManaRegen/s=3.5+0.045INT+0.035SPR`, Mage `ManaMax=160+5.5INT+2SPR` and `ManaRegen/s=4+0.07INT+0.055SPR`, plus explicit proposed empowerment miss/Mana/crit/image and mirrored-spell snapshot rules. With proposed level150 no-gear stats, max Mana is Knight **2077.2**, Mage **4753.5**; the full high-rank Knight skill/empower sequence costs **1978 Mana**, Mage one of each max-rank cast **2548 Mana**. These are **sanity checks only**, NOT balance validation. New intended first-playtest Knight extra Magical Void packet formula is **`30+17r+[0.4+0.016(r−1)]AGI+[0.1+0.004(r−1)]INT`**, motivated by the user's newer rank-scaling request; the earlier fixed `30+17r+0.4AGI+0.1INT` is retained as the historical/rank1 anchor, not a second simultaneous damage packet. **No numeric resource/buff/copy edge cases have been separately user-finalized**, and NO Unity changes were made in this documentation round.

**2026-11-24 Tier III Psion evolution attribute progression, latest Round20 (USER-APPROVED direction; PROPOSED numbers pending sign-off):** Psionic Knight and Mirage Mage must start as newly created Tier III level1 manifestations with **higher starting attributes in every category than their Tier II Psion parent**, and have **higher TOTAL per-level attribute growth**, sharply differentiated into Knight **AGI-primary, INT-secondary, higher VIT**, Mage **INT-primary, AGI-secondary**. Both have **VERY LOW STR/SPR growth**; higher *overall* growth does NOT mean forcing STR and SPR to exceed Psion growth. Actual `Assets/_DiceFree/Settings/Psion/Psion class.asset` is VIT12/STR7/AGI14/INT18/SPR10 at Lv1 and growth 1.55/.65/2.10/2.95/1.10 respectively. Round20 initial **DESIGNER PROPOSAL ONLY**, not yet user accepted: **Knight base VIT22/STR8/AGI30/INT22/SPR11, growth +2.30/+0.20/+5.00/+1.70/+0.20** (base sum93, per-level9.40); **Mage base VIT19/STR8/AGI22/INT32/SPR11, growth +2.00/+0.20/+1.70/+5.00/+0.20** (base sum92, per-level9.10). Compare parent total Lv1 61/growth 8.35. Preserve existing normal-basic stat coefficients Knight `0.6AGI+0.4INT`, Mage `0.6INT+0.4AGI`, and all confirmed class gates/quests unchanged. The Tier III **Mana max/regeneration resource profiles** must be designed against currently HIGH provisional ~20 Mana/active skill rank costs, especially for Knight's modest INT and minimal SPR growth; do not quietly inherit parent Psion Mana profile as final balance. See [Psion Tier III Round20](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md) for proposed table and 60/150-level projections. **Design only; Unity assets not modified.**

**2026-11-23 Psion Tier III Round19 latest teacher placement, quest and dungeon status (USER APPROVED):** Teachers for **Psionic Knight and Mirage Mage** are currently free to be located and moved by Codex **ANYWHERE IN WORLD 1 OR WORLD 2**. Any hidden teacher, across any class, is discovered automatically by **≤2m proximity**, recorded permanently **Echo-wide in Codex ONLY** with **NO automatic world-map marker**. The wrong class receives rejection dialogue; a correct Tier II Psion **below level60** is told to return when stronger; ONLY **Tier II Psion Lv60+** may ACCEPT either advancement quest. Both specialist quests **MAY BE ACTIVE SIMULTANEOUSLY**. Quest proof must be a **new SOLO run started after accepting the relevant quest and fully completed on the same advancing Tier II Psion**. Knight trial is **existing Wolf Survival: Moonlit Clearing**. Mage trial is **existing Quillboar Shaman ritual dungeon/Fourfold Hollow**; this scene is present at `Assets/_DiceFree/Scenes/FourfoldHollow.unity`, with multiplayer QA noted in `docs/AGENT_HANDOVER.md`; old Quillboar docs saying it was future-only are outdated. A qualifying clear marks its quest ready; the Psion must **RETURN TO THAT TEACHER FOR TURN-IN** to create Tier III level1 child, retaining the parent Psion manifestation and other independent quest state. No auto-advancement on dungeon victory, no retroactive credit, and no automatic mutual exclusion of these two quests. Existing permanent scholar access requirements for Fourfold Hollow should be reconciled with current runtime behavior. See [Psion Tier III Round19](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md), [Codex](CODEX.md) and [Class Requirements](CLASS_REQUIREMENTS_AND_DISCOVERY.md). **Design/handover only; no Unity edits or branch merges.**

**2026-11-23 Psion Tier III Round 18, LATEST exact hidden-teacher trials:** The previously shorthand **"Wolf Cave" IS the existing 10-wave WOLF SURVIVAL: MOONLIT CLEARING dungeon**; the previously shorthand **"Shaman Cave" IS the Quillboar Shaman ritual cave/dungeon**, preserving the normal Quillboar scholar's four-element quest/turn-in and character-specific dungeon-access requirements. Each specialization has its own hidden teacher, discoverable by approaching **within 2m**, automatically recorded in the **ECHO-WIDE Codex for ANY hidden teacher across ANY class** without needing to talk; discovery does **not** accept the quest or award advancement. The relevant advancement quest must be **ACCEPTED FIRST by the specific Tier II Psion manifestation that wants to advance**; only a **NEW SOLO DUNGEON RUN STARTED AND COMPLETED AFTER QUEST ACCEPTANCE ON THAT SAME PSION** counts. Historical solo clears from Echo-wide Codex, a different class manifestation, party runs, or a run begun before taking the quest do not satisfy the requirement. Wolf Survival is documented as an existing 10-wave clearing; the Quillboar Shaman dungeon is user-identified as existing, though older regional notes label implementation future, so verify current runtime scene/stable dungeon ID during later coding rather than guessing. See [Tier III Round 18](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md), [Class Discovery](CLASS_REQUIREMENTS_AND_DISCOVERY.md), and [Codex](CODEX.md). Tier III numerical skill caps, exact 150-point rank gates, prior combat approvals and original Psion preservation remain unchanged. **DESIGN ONLY; no Unity implementation authorized.**

**2026-11-23 Psion Tier III Round 17, latest USER-APPROVED advancement/illusion behavior:** Both descendants have SEPARATE hidden teachers located by **rumors, lore and environmental exploration clues**. **Discovering ANY hidden teacher for ANY class automatically records a permanent Echo-wide Codex teacher discovery**, including if found on another manifestation; discovery is NOT the same as automatically advancing. The Knight teacher's challenge is **solo completion of the Wolf Cave**, the Mirage Mage teacher's challenge is **solo completion of the Shaman Cave**. **Wolf Cave identity is not yet matched to the implemented Wolf Survival: Moonlit Clearing**, and existing Shaman Cave content hasn't been verified; do not invent or silently substitute dungeons. **Mage primary and secondary images continue fighting for their remaining lifetime after the Mirage Mage dies**; Knight image likewise survives the Knight's death as approved in Round 16, but **its end-triggered attack-speed buff is LOST if the Knight is dead when it ends**, not held for a resurrection. Tier II Psion Lv60 prerequisite, new Tier III Lv1 manifestation, per-class 150 rank gates, optional Tier IV Lv120 eligibility, existing mana/cooldown provisional status and all prior damage/channel decisions remain unchanged. See [Psion Tier III Round 17](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md), [Codex](CODEX.md), [Class Discovery](CLASS_REQUIREMENTS_AND_DISCOVERY.md). No Unity implementation authorized.

**2026-11-23 Psion Tier III Round 16 latest user-confirmed advancement and combat decisions:** Psionic Knight and Mirage Mage obtain their Tier III specializations by **exploring the world to discover TWO SEPARATE HIDDEN TEACHERS**, with names, zones, clues and challenges NOT YET designed. Existing Tier II level60 eligibility, new Tier III level1 preserved manifestation and 150 approved rank gates remain unchanged. **Knight's mobile Mental Image persists and attacks after the Knight dies** for its normal remaining lifetime; Mage image's delayed mirrored targeted spell **retargets the nearest valid enemy if the original target dies before the 0.4s echo** (unless the originating image itself dies and cancels the pending echo). **Independent original/copy Metal Shield groups absorb damage in OLDEST ACTIVE POOL order**. The Knight normal autoattack remains **Physical**, while its additional empowered Void packet is **MAGICAL Void** against Magical Defense. The user also directed **MUCH higher rank-dependent Mana costs (~20 Mana per rank for ordinary casts)** and **spell damage stat coefficients that grow with purchased rank**; Round 16 has **delegated DESIGNER-PROVISIONAL test formulas** in [Psion Tier III evolution design](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md). Do not mistake these numeric tests for final user-approved combat balance, and do not change Tier II formulas or Unity implementation.

**2026-11-23 Psion Tier III Round 15 latest equipment & designer balance authorization:** Both evolved classes retain their **approved 150 skill ranks and exact eligibility gates** (unchanged). User established **Psionic Knight signature main-hand mental/psionic blades, generic AGI blades, AGI-oriented melee gear; Mirage Mage signature MAIN-HAND tomes/cards and other generic INT weapons, caster-oriented gear; BOTH share offhand Ionic Stones (AGI/basic-attack vs INT/spell variants) and can use matching generic AGI/INT offhands**. The user explicitly delegated **PROVISIONAL Tier III Mana costs and cooldowns** to the designer; upgraded Mirage Mage spell damage, rank scaling, INT scaling, Metal shared shield values and Knight personal Void shield value must exceed parent Tier II Psion's relevant counterparts. New provisional formula tables and inherited status rules are in [Psion Tier III Round 15](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md) and gear-family notes in [Loot and Equipment](LOOT_AND_EQUIPMENT.md). These are **designer-selected TEST values, not user-approved final numerics**; NO Unity changes authorized and Knight extra empowered Void damage CHANNEL remains open. Generic melee AGI/caster gear preferences must not invent a new armor-weight restriction system.

**2026-11-23 Psion Tier III Round 14, latest APPROVAL:** The user APPROVED the **EXACT Round 13 rank eligibility gates and first-unlock levels** for **both Tier III descendants**, and their existing confirmed caps. **Knight:** Psionic Blades Mastery 30 (first unlocked level1), Mental Image 25 (level1), personal Void Shield 29 (level6), Empowered Attacks 34 (level1), Speed Presence 32 (level10). **Mirage Mage:** Psychic Bolt 34 (level1), Mental Image 25 (level1), Lightning Storm 30 (level5), Metal Shield 29 (level8), Speed Presence 32 (level10). The **full 150 individual rank purchase gates per class** are as recorded verbatim in [Psion Tier III Round 13 tables and Round 14 approval](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md); all were audited for legal skill-point spendability at every level 1 through150. This overrides previous "pending approval" statements in this file, but does **not** approve game implementation, skill Mana/cooldowns or other mechanics. Skills earn 1 point per Tier III class level through150; Tier IV remains optionally available at level120. No Unity changes approved.

**2026-11-23 Psion Tier III Round 13, latest USER-SELECTED caps:** Psionic Knight caps **Blades Mastery30 / Mental Image25 / personal Void Shield29 / Empowered Attacks34 / Speed Presence32** =150. Mirage Mage caps **Psychic Bolt34 / Mental Image25 / Lightning Storm30 / Metal Shield29 / Speed Presence32** =150. Knight Mastery now grants **+1 percentage point BASIC crit chance/rank (30% at rank30 from 0% opt-in baseline)**, **+1 flat Physical Defense ignored/rank** and **160% initial crit multiplier +1 percentage point per Mastery rank (190% at rank30)**. Tier III Knight NORMAL basic attacks scale **0.6×AGI+0.4×INT**, Mirage Mage **0.6×INT+0.4×AGI**; future Tier IV Knight basic **0.9×AGI+0.2×INT**, future Tier IV Mage basic **0.9×INT+0.2×AGI**, as **future design intent only**, not Tier IV implementation. Knight bonus empowered Void packet remains its SEPARATE approved formula, not altered by these basic-attack weights. The latest **individual rank eligibility/unlock schedules are designer PROPOSALS, verified at all levels1–150, but NOT yet user-approved**, as documented in [Psion Tier III Round 13](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md). No gameplay implementation authorized.

**2026-11-23 Psion Tier III Round 12, latest rank-cap revision:** User approved **Mental Image max rank 25** for **BOTH** Psionic Knight and Mirage Mage, reallocating the **15 freed ranks across the other FOUR skills** in each 150-point budget. User also changed Knight's finite Empowered Attacks to **`1 + floor(skillRank/3)`** charges (e.g., rank30 → 11 instead of 16), and requested **lower critical chance per Blades Mastery rank, unchanged +1 flat Physical Defense penetration/rank, plus small critical damage growth/rank**. **Exact allocations to other four skills and critical coefficients are NOT YET USER-APPROVED**: a mathematically audited **designer proposal** is Knight caps **34/25/29/34/28** (Blades/Image/Void Shield/Empowerment/Speed) and Mage caps **34/25/34/29/28** (Bolt/Image/Storm/Metal Shield/Speed), with candidate Blades Mastery **+0.5 percentage points crit chance, +1 flat PDEF ignored, +1 percentage point crit damage per rank**. User may revise caps and gate schedules. See [Psion Tier III Round 12](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md) for all proposed individual gates and audit. Tier III skill points continue **1 per level through150**, Tier IV advancement eligibility remains **level120**, no gameplay implementation authorized.

**2026-11-23 Psion Tier III Round 11, latest APPROVED structure:** Both **Psionic Knight** and **Mirage Mage** gain **exactly ONE skill-rank point per Tier III class level from 1 through 150, for 150 total**. Each currently has an approved **five-ability roster** (Knight: Psionic Blades Mastery, mobile Mental Image, personal Void Shield, finite empowered attacks, Speed Presence passive+aura; Mage: Psychic Bolt, stationary mirroring Mental Image, Lightning Storm, Metal Shield, the **same full Speed Presence passive+aura**). **Both classes** get **+1% personal attack speed AND movement speed per purchased speed-passive rank**, plus **+0.1% attack speed AND movement speed per rank** to **all allies within 12m**, including self. Strongest aura of the SAME NAME only; identical named auras from the two classes do not stack, personal bonuses remain. **Illusions can receive allied aura effects but DO NOT emit a new aura source**, even though Knight's image ordinarily inherits applicable passive bonuses. Tier III -> Tier IV advancement eligibility remains **level 120**, optional; those staying Tier III keep earning skill points to 150. **The proposed per-ability caps, unlock levels and exact 150-rank-gate schedules have passed a mechanical eligibility audit but are DESIGNER PROPOSALS AWAITING USER APPROVAL**, not already chosen numbers. See [Psion Tier III Round 11 and audited tables](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md). No Unity implementation authorized.

**2026-11-23 Psion Tier III progression update:** Both **Psionic Knight** and **Mirage Mage** must be able to keep **upgrading their skills through Tier III class level 150**. Existing Tier III level-120 eligibility to advance into Tier IV is **unchanged**; advancement is optional and the Tier III manifestation persists. The user has approved five known abilities for each lineage: Knight (two evolved Psion skills, personal Void shield, empowered attacks, speed-plus-aura passive), Mage (four evolved Psion spells plus the **same-named** allied speed aura passive). The aura covers **all allies within 12m**, gives **+0.1% attack and movement speed per passive rank**, and only the **strongest aura with the same name** applies; Knight also benefits from its own aura in addition to **+1% attack and move speed per rank** personal bonuses. Whether Mage receives those additional +1% personal bonuses is still open. Exact number of class skill points earned through 150 and caps/rank gates **remain to be designed**. See [Psion Tier III Rounds 9–10](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md). No Unity implementation authorized.

**2026-11-22 explicit Tier III design authorization, Psion lineage only:** The user approved starting two World 2 Tier III evolution designs, **Psionic Knight** (closer-ranged basic-attack focus, mobile attacking illusion) and **Mirage Mage** (longer-ranged high-damage spells/barriers/mirrors, explosion-focused illusion). Both use Mana only. See [Psion Tier III evolution design](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md), Round 1. Historical "do not design Tier III yet" notes predate this direct request and do **not** prohibit this newly authorized design work; no Unity implementation is authorized. Standard Tier II level 60 advancement into a new level-1 preserved-manifestation branch continues to apply.

## Current Tier II additions (2026-10-16)

Both first branches now have a level-30 quest advancement: Physically Blessed Novice to Greatblade through the retired swordswoman's normal Slime Boss trial, and Magically Touched Novice to Mage through the brewer / retired mage's Slime Dungeon puzzle trial. Both require returning to the mentor and create a level-1 child manifestation while preserving the parent.

Mage is an unspecialized INT caster with six active spells and two passives. See [Mage specification](classes/MAGE.md) for the agreed rules, provisional balance and visual provenance. The flexible ability provider supports this eight-ability kit without assigning an ultimate.

The menu now groups saved manifestations by tier and displays equipped 2D portraits. Historical prototype sections below describe earlier milestones, not the complete current feature set.


## Core concept

Every new Echo lineage begins as a **Novice**.

See `docs/classes/NOVICE.md` for the detailed starting class.

Advancement branches create alternate class manifestations rather than deleting the parent manifestation.

Each class ID has at most **one persistent save-state** for the Echo.

The original class remains playable, while the first advancement into a new class creates that class's save-state at level 1.

## Advancement cadence

```
Novice        -> reach level 10  -> branch -> new Tier I manifestation starts at level 1
Tier I        -> reach level 30  -> advance -> new class starts at level 1
Tier II       -> reach level 60  -> advance -> new class starts at level 1
Tier III      -> reach level 120 -> advance -> new class starts at level 1
Tier IV       -> reach level 200 -> advance -> new class starts at level 1
Tier V
```

A character may continue leveling beyond an advancement threshold.

When they eventually advance:
- the new class begins at level 1;
- excess XP does not carry over;
- the new class uses its own base stats and growth.

Overleveling gives no automatic permanent bonus, but can make advancement quests easier and may satisfy hidden conditions.

A level-200 Novice is intentionally possible.

It is also intended to receive a hidden stronger Novice payoff (secret skill and potentially first passive) rather than remaining permanently identical to the level-10 kit.

## Branching manifestation model

Advancement is a **branch**, not an overwrite.

When a manifestation reaches an advancement threshold and chooses a class:
- the current manifestation is preserved as a playable parent slot at the fork point;
- if the target class does not yet have a save-state, that class save-state is created for the chosen advancement;
- the game immediately continues as that new advanced manifestation in the same session/world state;
- the new advanced manifestation begins at level 1 with its own class base stats/growth;
- **all manifestation-specific state** is snapshotted into the child at the fork, including gear, carried inventory/currency, quest/world progression and relevant class/skill state;
- the parent keeps its original state and the child receives a duplicate snapshot;
- the preserved parent resumes immediately before the blessing choice if loaded later;
- Echo-wide state remains shared;
- the parent manifestation can later choose another available branch, creating another class save-state if that class has not already been created;
- reaching the same resulting class through another route does not create or overwrite a duplicate save-state.

Example after the first split:
- Novice has one preserved save-state;
- Physically Blessed Novice can gain one save-state;
- Magically Touched Novice can gain one save-state.

The class/Ways UI can serve as the start-game selector for these class save-states.

### Current prototype implementation (#50, 2026-10-06)

The first manifestation-fork foundation is implemented on `feature/50-advancement`, stacked on the completed #49 Novice-skill branch.

Current prototype behavior:
- the player build boots into a dedicated `StartMenu` scene before Cornberg;
- a fresh save offers `Start as Novice` and then enters Cornberg as the initial level-1 Novice manifestation;
- an existing Echo lists every saved manifestation with class name and saved level, marks the last-active manifestation, and lets the player choose which existing class timeline to load;
- an unresolved/missing class definition remains visible as unavailable rather than silently deleting its save section;
- choosing an existing manifestation updates only Echo-level active-roster metadata transactionally, then Cornberg loads that saved class state;
- class switching is not exposed as an arbitrary in-world gameplay button; the start menu is the normal load-time selector;
- level-10+ Novice exposes two data-driven advancement edges: Physically Blessed Novice and Magically Touched Novice;
- advancement commits the preserved parent and new child in one Echo revision before changing the live actor;
- the child starts at level 1 / 0 XP and class skill ranks reset instead of numerically carrying Novice ranks;
- manifestation-owned quests/world state, anchor, carried gold, inventory and equipment snapshot into the child;
- copied child items receive new instance IDs and equipped references are remapped, so parent/child timelines do not claim the same item instance identity;
- parent and child then diverge independently and the preserved Novice can later create the other first branch;
- duplicate creation of an already-existing target class is rejected without overwriting it;
- an additive version-1 `echo:manifestations` roster section stores active class and branch history while the overall save/manifestation schema remains v5;
- #50 originally authored Tier-1 actor shells; both first branches are now real playable manifestations: Physically Blessed was completed in #51 and integrated into `poc/cornberg` at `77716b2`, while Magically Touched is implemented on review branch `feature/52-magically-touched` with its dedicated presentation, Mana economy, five-skill kit, persistence and confirm-cast targeting.

The start-menu presentation is deliberately functional/prototype-grade. Final title art, character cards, archive/delete UX and richer class preview presentation remain future UI work.

This one-save-per-class model is part of the Echo/timeline fantasy.

Open:
- archive/delete/recreate behavior for intentionally discarded class-local saves;
- UI presentation of historical parent/route relationships.

## Core attributes

Current attribute set:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Every class defines:
- level-1 base attributes;
- per-level attribute growth;
- starting Physical Defense;
- starting Magical Defense;
- one or more primary attack attributes;
- movement speed;
- base HP;
- resource(s);
- other class-specific base values.

Later advancements generally start with higher base stats.

Players do not manually allocate stat points.

### Primary attributes

A class may use one or multiple primary attributes.

There is **no universal multi-primary formula**.

Each class defines its own attack scaling according to identity and balance.

Examples could include:
- equal STR + INT weighting;
- mostly STR with some INT;
- AGI + SPI;
- all five attributes for an exceptional secret class.

The exact coefficients are class data.

### Vitality

Vitality contributes to HP:

```
Max HP = class base HP + (Vitality × coefficient) + modifiers
```

The coefficient may be universal or class-dependent; unresolved.

### Secondary effects

Current direction:
- Vitality -> HP and HP regeneration;
- Strength -> modest Physical Defense;
- Agility -> very small Attack Speed and Movement Speed;
- Intelligence -> modest Magical Defense plus class-specific resource value;
- Spirit -> healing done and healing received with separate coefficients.

Class base stats, equipment, abilities and passives remain the main source of large combat differences.

See `docs/STATS_AND_DAMAGE.md` for the detailed model.

### Skill scaling

Skill scaling is ability-specific.

Abilities can use:
- weighted combinations of stats;
- highest of selected stats;
- highest stat overall;
- total stats;
- class-specific values.

The class's primary attribute(s) do not force every ability to use the same formula.

## Opening branches

First advancement:
- **Physically Blessed Novice**
- **Magically Touched Novice**

Working Tier-1 class sheets:
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md`
- `docs/classes/MAGICALLY_TOUCHED_NOVICE.md`

Current strong direction:
- both Tier-1 branches introduce **Mana** as a simple shared resource;
- specialized resources arrive on later classes where they reinforce identity;
- Physically Blessed remains a broad STR/AGI melee generalist;
- Magically Touched remains a broad INT/SPI magical generalist.

Current candidate level-30 branching:
- Physical: sword-and-board, rogue, ranger, two-handed melee DPS;
- Magical: shield/buff healer-support, healing-focused healer, arcane wizard, occult caster.

This four-plus-four split is still a working direction rather than locked final structure.

### World 1 normal Tier II scope and World 2 tier progression — LOCKED by user 2026-10-19

**USER-CONFIRMED FINAL NORMAL TIER II ROSTER FOR WORLD 1: EXACTLY EIGHT CLASSES, FOUR FROM EACH TIER I NOVICE.** This closes the immediate question of whether Acolyte/Thief are the last normal classes: **Psion is INCLUDED as the eighth class**. Other future ideas such as Druid are not additional normal World 1 Tier II classes unless user revises scope.

| Tier I parent | Standard World 1 Tier II class | Design status |
| --- | --- | --- |
| Physically Blessed Novice | **Greatblade** | Designed |
| Physically Blessed Novice | **Defender** | Playable through Hold the North Gate. [Defender Tier II](classes/DEFENDER_TIER2_DESIGN.md) |
| Physically Blessed Novice | **Ranger** | Playable through The Ironpine Hunt. [Ranger Tier II](classes/RANGER_TIER2_DESIGN.md) |
| Physically Blessed Novice | **Thief** | **TIER II GAMEPLAY DESIGN COMPLETE Rounds1–8**; user-approved 80-rank gates, Mana/CD and six-skill combat kit, finale Envenom-direct-only stealth opener / crit buff real-time while hidden / miss-consumption / interaction rules. Assets and quest DEFERRED, NO Unity implementation. [Completed Thief Tier II](classes/THIEF_TIER2_DESIGN.md) |
| Magically Touched Novice | **Mage** | Designed |
| Magically Touched Novice | **Priest** | Designed |
| Magically Touched Novice | **Acolyte** | **TIER II CLASS DESIGN COMPLETE, Rounds1–8**, six-skill kit with exact 80-rank user-approved gates (curse, Radiation plague, skeletons, Void Drain Life, Mortal Coil dual-target spell, INT/SPR/poison passive). [Acolyte Tier II design](classes/ACOLYTE_TIER2_DESIGN.md) |
| Magically Touched Novice | **Psion**, regular eighth World 1 Tier II class. The five-skill combat kit is playable in the Development Room. [Psion Tier II design](classes/PSION_TIER2_DESIGN.md) |

**USER-APPROVED DESIGN AND WORLD ORDER:**
1. Finish all eight NORMAL World 1 Tier II class designs, specifically **Acolyte → Thief → Psion** for the three unfinished classes.
2. Then design the **Quillboar area/region**.
3. Then design the **major/big city** and its gameplay/services.
4. Some **Tier II advancement NPCs MAY be moved to that future city** for progression pacing; this is an open later decision. Do NOT quietly relocate already-agreed mentors/quests or invent finished city geography. Keep Cornberg swordswoman/Mage brewer, Trimery Priest healer, Trimery veteran Defender guard and wounded Ranger/Alpha Wolf as currently specified unless user explicitly revises them.
5. **DO NOT DESIGN ANY TIER III CLASSES NOW. Tier III belongs to World 2.** Older Greatblade, Ranger, Priest and Defender Tier III concept names/convergence snippets are historical brainstorming only; do not expand their trees, skills or quests until World 2 design is authorized.

**ACOLYTE ROUND 1 (2026-10-19):** user chose four core combat pillars: **one Curse, one distinct damage-over-time ability, one Soul Charge-spending Summon, one channeled Life Drain**. Acolyte uses **Mana + Soul Charges**, charges earned through offensive spells; charge caps/gain and summon type remain open. **Wand + grimoire** remain the equipment concept, but the grimoire is a **normal stat-bearing offhand for now**, NOT a compulsory special spell/inscription/book-charge system; authored item Intrinsics may modify class abilities later across all classes. Role is **sustained damage, curses/debuffs and life-drain self-sustain**, contrasting with Mage burst/CC. A future **graveyard mentor** asking for deceased people from **Cornberg and Trimery** to be brought for **proper burial** is a story hook; exact NPC, geography and advancement mechanics deliberately DEFERRED until location planning. **Acolyte Round 2 (2026-10-19):** Curse reduces Magical Defense and attack speed; a weak Radiation DoT spreads; controllable skeletons last a set time; Soul Charge cap 8 with 4 spent per skeleton; offensive spells, successful Drain Life ticks, and deaths of targets afflicted by the Radiation DoT generate Soul Charges. Drain Life deals Void-element damage, heals the caster based on damage dealt, channels for 5 seconds with one tick each second, and cancels on movement, stun, or explicit interrupt (not ordinary incoming damage). **ACOLYTE-011–015 newer decisions:** Curse uses **FLAT** reductions to enemy Magical Defense and attack speed (boss Tenacity reduces their effects; specific formula/open). Radiation infects nearby enemies only after **2 seconds of continuous proximity**, never re-infecting a previously infected member within the same chain; radius/independent re-casts open. **Up to 4 active controllable skeletons, each 15s lifetime**. Soul Charges **+1 per resolved Drain Life tick** and **+1 per enemy death while carrying own Radiation DoT**; applying Curse or Radiation grants **0**. Drain Life restores a **skill-rank-scaling percentage of ACTUAL enemy HP damage**, never from shield-absorbed damage. **ACOLYTE-016–020 ROUND4 CONFIRMED (supersedes those earlier OPEN items):** Curse rank1 **−5 flat MDEF, −0.10 attacks/sec for 10s**, boss reduction via shared Tenacity rules when defined; Radiation **3m proximity/2s uninterrupted contact**, resets if separated, infected hosts can pass it onward, no chain reinfection, manual recast refreshes duration; cap4 **15s controllable skeletons**, new summon **replaces oldest**, auto-retaliate responds to the hero's toggle; Drain Life **25% actual HP damage healing at rank1 +2 percentage points per rank**, target ~60–65% final cap not exactly selected. **User added TWO abilities to make SIX total:** **Mortal Coil** (1-second INT+SPR-scaling Void offensive spell, optionally heals allied heroes/owned skeletons instead), and a **passive** granting INT + smaller SPR and weak poison on wand basic attacks. Passive poison is currently a STATUS TAG with element/stacking/proc chance undecided. Mortal Coil ally healing, self-cast and charge-generation details, and exact rank limits are OPEN. **Earlier Occult Bolt idea NOT selected.** Details: [Acolyte class design](classes/ACOLYTE_TIER2_DESIGN.md).

**ACOLYTE-021–025 ROUND 5 (2026-10-20):** Mortal Coil's **ONE-SECOND Void spell** damages enemy targets with INT+SPR and **VOID-ELEMENT HEALS the caster, allied heroes or owned skeletons**, applying the healed target's Void elemental-resistance healing modifier. Channel cancels on movement/stun/explicit interruption, not on ordinary incoming damage. **+1 Soul Charge only for successful enemy DAMAGE; +0 on self/ally/skeleton healing.** Passive grants **+2 INT/+1 SPR per purchased rank** and **guaranteed 3s nonstacking Poison-tagged DoT on every successful wand basic attack**, refreshing duration rather than stacking; poison damage scales with INT and passive rank, its damage element/rate are OPEN. User delegated an **80-rank progression**; designer **PROPOSES, awaiting numeric sign-off:** Curse14, Radiation16, Skeleton12, DrainLife20, MortalCoil10, Passive8, staggered first gates and intervals, **verified a legal skill purchase at every class level1–80**, all 80 gates eligible by level77. Proposed Drain Life cap20 yields **63% healing from actual HP damage** under approved 25%+2 percentage points/rank formula. See [Acolyte Tier II](classes/ACOLYTE_TIER2_DESIGN.md). **DESIGN ONLY, not gameplay implementation.**

**ACOLYTE-026–030 ROUND 6 — LATEST USER CONFIRMATION (2026-10-20):** The formerly proposed exact **80-rank class schedule is now APPROVED:** Withering Curse14 / Radiation Plague16 / Summon Skeleton12 / Drain Life20 / Mortal Coil10 / Occult Adept8, with audited level gates in [Acolyte design](classes/ACOLYTE_TIER2_DESIGN.md) and legal one-point purchases every class level1–80. Drain Life rank20 therefore heals **63%** actual HP damage (under approved +2pp/rank healing formula); passive rank8 gives **+16 INT / +8 SPR**. Mortal Coil enemy Magical Void damage uses both INT and SPR with **INT emphasis**; self/ally/skeleton Void-element healing uses both with **SPR emphasis**, both scale by rank. **USER CORRECTED PREVIOUS MISPRINT:** passive Poison DoT has **MAGICAL COFFEE-ELEMENT damage, NOT DONUT**, tick ONCE PER SECOND, three seconds, does NOT spread or stack, status tag Poison. Exactly **ONE skeleton per summon for 4 Soul Charges and ZERO Mana**, with Auto-Retaliate OFF attack only by explicit user order, ON reactive to being attacked. Designer-authored skill CDs/Mana/damage/healing coefficients in Acolyte spec are **FIRST-PASS PROPOSALS only**, not user-confirmed balance or implemented behavior. Coffee registry availability still to be checked; no global Donut changes.

**ACOLYTE-031–035 ROUND 7 (2026-10-20) — MOST RECENT CONFIRMED DETAILS:** Initial playtest cooldown/Mana costs are now USER-APPROVED: **Curse12s/12 Mana, Radiation10s/15 Mana, Summon Skeleton3s/0 Mana+4 Soul Charges, Drain Life14s/24 Mana, Mortal Coil7s/14 Mana, passive0**. Radiation duration is **3s at rank1 growing to 8s at rank16** (intermediate rank duration steps/tick frequency remain unapproved designer proposals). Summon Skeleton ranks improve **skeleton HP and BASIC ATTACK damage**, not quantity/15s lifetime/4-Soul cost; previous exact skeleton numeric HP/attack coefficients remain PROPOSED. Nonstacking, nonspreading three-second Poison-tag **MAGICAL COFFEE** wand DoT refreshes duration WITHOUT restarting its existing once-per-second tick clock. **Each successfully resolved Drain Life tick also damages an enemy SHIELD and awards +1 Soul Charge even when FULLY blocked**, while healing only from ACTUAL ENEMY HP damage, thus **ZERO healing from fully shielded damage**. User casually used "soul shard" for the tick reward; retain established **Soul Charges** name unless explicitly changed. All raw spell coefficients and shared Tenacity equation remain pending their own sign-off. [Acolyte design](classes/ACOLYTE_TIER2_DESIGN.md). This note is DESIGN ONLY.

**ACOLYTE FINAL ROUND 8 — USER-APPROVED 2026-10-20:** See [authoritative Acolyte final class spec](classes/ACOLYTE_TIER2_DESIGN.md), which supersedes historical open/proposed entries. Radiation duration increases at skill ranks **4,7,10,13,16** from **3s rank1** to **8s rank16**. Transmitted infection **INHERITS ONLY THE SOURCE'S REMAINING DoT TIME**, not a new full duration. Radiation DoT now ticks every **0.1 seconds (TEN PER SECOND)** with per-tick RAW Magical Radiation damage **1/10 of the previous approved one-second tick formula**, retaining expected DPS. At rank `r`: `[(2+0.10×casterINT)×(1+0.05×(r−1))]/10` RAW per 0.1s tick. Skeleton HP **`(55+2×ownerSPR_AT_SUMMON)×(1+0.05×(r−1))`** (SPR NOT VIT); Basic Physical raw attack `(3+0.18×ownerINT_AT_SUMMON)×(1+0.06×(r−1))`; owner stats **SNAPSHOT ON SUMMON**, 5% HP/6% attack rank scaling approved. Curse rank growth −1 more flat MDEF, −0.005 attacks/sec per rank (rank14 −18 MDEF/−0.165 attacks/s, 10s). **ALL** previously proposed raw damage/healing coefficient baseline values for Radiation, Drain Life, Mortal Coil and Magical Coffee poison now **USER APPROVED FOR INITIAL PLAYTEST**, with Radiation tick and SPR HP corrections above. Global Tenacity, assets, elemental registry, implementation and deferred graveyard quest remain external follow-up. **ACOLYTE INTERVIEW COMPLETE; THIEF IS NEXT.**

**THIEF ROUND 1 (2026-10-21) — LATEST USER-CONFIRMED:** World1 normal Tier II Thief blends **fast assassin + nimble duelist + dirty-tricks burglar**; user explicitly wants all three recorded as possible **FUTURE TIER III ARCHETYPE IDEAS for World2 ONLY**, NOT active design, finalized branches or approved skill trees. **Two ONE-HANDED DAGGERS equipped mainhand/offhand**; all ordinary equipment **currently provides stats only** until individually authored unique item effects/Intrinsics are introduced later. An offhand dagger is NOT automatically an independent extra attack event or an exotic passive. **SHORT STEALTH**, with bonus damage on the **FIRST BASIC AUTOATTACK THAT BREAKS STEALTH**; exact duration, detection rules, extra-damage coefficient and reposition permissions open. **MANA ONLY**, no Combo/Energy/Guile/Stamina second resource. Appearance **DEFERRED TO SEPARATE WORKING CHAT/CODEX asset review** of user already-downloaded Rogue/Thief/Assassin/Ninja models; NO art/look picked here. **Thief advancement quest/NPC NOT TO BE DESIGNED UNTIL AFTER THE MAJOR CITY** is designed, per user. Continue exact Thief skill interview only: [Thief Tier II](classes/THIEF_TIER2_DESIGN.md).

**THIEF FUTURE WORLD2+ CLASS IDEAS — USER'S LATE-NIGHT NOTE 2026-10-21, SPECULATIVE ONLY:** The normal **Tier II Thief remains a mixed** assassin/duelist/burglar (short stealth, two stat-only daggers, Mana). Potential future **Tier III Burglar** (poison/dirty tricks) → **Tier IV Pirate**. Potential future **Tier III Duelist** (two one-handed daggers, evasion, quick repeated strikes) → **UNNAMED Tier IV Duelist-lineage class**. Potential future **Tier III Assassin** (stealth/positioning, SLOWER but precise/lethal attacks), beyond which no follow-on class was named. Separately, a potential **secret cross-lineage class** unlocked after major same-Echo achievements in **Berserker** and **Duelist** lineages (user suggested reaching **level120** in each OR appropriate later Tier IV versions; precise milestone/tier REQUIREMENTS OPEN). Possible secret class **DUAL WIELDS TWO SWORDS, perhaps TWO TWO-HANDED SWORDS**, both speculative. Do not assume this is a current weapon-slot rule, confirmed secret class tier, final branch structure or actual unlocked class. The user's normal **THIEF ROUND 2 QUESTIONS WAIT UNTIL TOMORROW**. **DO NOT DESIGN ANY TIER III/IV/SECRET KIT NOW.** Canonical idea bank and wording: [Thief Tier II interview](classes/THIEF_TIER2_DESIGN.md).

**THIEF ROUND 2 — USER DECISIONS, 2026-10-21:** Thief Tier II's STEALTH lasts approximately **4 seconds at initial rank, growing to 8s at final rank**; first basic attack that breaks stealth uses a **DAMAGE MULTIPLIER** (NOT flat damage or automatic stealth crit), with **NO extra rear positional bonus** (saved for later classes). Exact bonus number/cooldown/combat usability still OPEN. **TWO stat-only one-handed DAGGERS but ONE NORMAL BASIC ATTACK EVENT**, alternating right/left dagger swing animation (not two independent offhand hits). User selected FIVE active ability identities: **STEALTH**, **ENVENOM** (poison-themed), **a dedicated CRIT ability** (exact mechanics OPEN), **FAN OF KNIVES**, and **SMOKE BOMB** (an AREA that deals damage over time and gives enemies inside a MISS CHANCE). A **CRIT-CHANCE passive** is the sixth working skill; user said **MAYBE some bonus AGI**, not locked numeric stat buff. **AGI ONLY contributes to Thief's offensive class ability scaling; no STR coefficient. MANA ONLY**, no secondary combo resource. Unique item effects and character model/advancement NPC remain deferred. The full [Thief Tier II Round2 ledger and five Round3 questions](classes/THIEF_TIER2_DESIGN.md) are authoritative. **DESIGN/DOCS ONLY; NO Unity changes; TierIII ideas remain World2 only.**

**THIEF ROUND 3 — USER-CONFIRMED 2026-10-21:** Stealth usable **IN COMBAT**; enemies **LOSE AGGRO AND CANNOT SEE/TARGET THIEF** while hidden. Its first normal basic autoattack on breaking Stealth deals **~1.1× damage at skill rank1, growing toward ~2× by maximum rank** (user said "or something similar", final interpolation/rank count still open). Taking damage revealing the Thief and full permanent threat-clear versus temporary target-drop were NOT answered and remain OPEN. **Envenom = INSTANT POISON DAGGER STAB** (Poison DoT vs instant poison delivery, element and stacking OPEN). Crit active = **SELF BUFF providing GUARANTEED CRITICAL HITS for a duration x seconds** (not a single instant guaranteed crit attack); which attacks/skills are eligible and buff time undecided. **Fan of Knives = THIEF-CENTERED 360° attack**, one or more damage packets per target still OPEN. **Smoke Bomb = GROUND TARGETED damaging lingering DoT cloud**, gives enemies inside MISS CHANCE on **BASIC/AUTOATTACKS ONLY, NOT enemy spells/abilities**, exact chance/area/duration open. Core six-skill Thief identity preserved, Mana only, AGI-only offense, two stat-only daggers / one alternating normal basic event, crit passive possible additional AGI open. World2 future branches, asset-choice and advancement-quest timing unaffected. Full [Thief Tier II Round3 / Round4 questions](classes/THIEF_TIER2_DESIGN.md). **DESIGN ONLY, no Unity edits.**

**THIEF ROUND 4 — LATEST USER DECISIONS (2026-10-21):** Taking incoming damage **DOES NOT BREAK Stealth**. Activating in-combat Stealth makes enemies drop **all or a very large fraction of the Thief's THREAT** while being unable to perceive them; **exact full reset vs severe numerical reduction OPEN**, don't assume 90% without consent. Stealth duration grows GRADUALLY **~4s rank1 → ~8s max** and first breaking normal basic hit gains gradually increasing **~1.1×→~2× damage multiplier** (precise rank cap/linear formula open). Envenom instant dagger stab applies **NONSTACKING Poison-tag DoT dealing PHYSICAL NATURE element damage** (its initial stab element and DoT ticks still open). Temporary guaranteed critical buff benefits **BASIC/AUTOATTACKS ONLY**, **1s first rank→4s max**, not Envenom/Fan or other abilities. **GROUND-TARGETED SMOKE BOMB** 4m radius, 5s duration, **20% enemy BASIC-attack miss chance ONLY while inside**, removed immediately on leaving; its damage-over-time uses **PHYSICAL TECH element**, NOT Magical Tech. Global Physical/Element system treats channel and affinity independently; no new Nature/Tech status proc implied. Six-skill roster, AGI-only offense, Mana only, stat-only dual daggers and 1 alternating-animation basic attack event remain unchanged. Appearance remains deferred to working chat/Codex downloaded-asset review, advancement quest after city, Tier III future ideas parked. See [Thief Tier II spec](classes/THIEF_TIER2_DESIGN.md). **DESIGN ONLY: no Unity edits.**
**THIEF ROUND5 CONFIRMED (2026-10-21):** Stealth activation **removes exactly 95% of the Thief's stored threat against each relevant enemy**, leaving5% and without resetting boss encounters; incoming damage STILL DOES NOT break invisibility; opening normal basic multiplier1.1×→2× and stealth duration4→8s grow gradually. Instant Envenom dagger skill hit deals **2× normal basic-attack RAW non-elemental Physical damage**, then applies **3-second ONCE-PER-SECOND nonstacking Poison-tag PHYSICAL NATURE DoT**. Envenom's skill attack is NOT automatically a basic attack event eligible for the Thief basic-only crit/stealth opener. Smoke Bomb **PHYSICAL TECH DoT now ticks FOUR TIMES PER SECOND (every0.25s; 20 ticks/5s)** with quarter-per-tick raw relative to its as-yet-unset per-second budget; OVERLAPPING SMOKE zones do NOT stack their damage or 20% enemy-basic miss effect. Crit passive now **+1 AGI and +1 percentage point BASIC attack critical chance per skill rank**, with **1.5× raw basic crit damage**, while the timed 1–4s crit buff guarantees basic critical hits (not skill crits). **User delegated rank math:** designer proposed **Stealth20 / Envenom14 / guaranteed-crit buff12 / Fan14 / Smoke12 / passive8 =80**, with specific staggered rank gates, automated legal one-point-per-level validation PASSED for every level1–80 and last gate77. Those exact numeric rank maxima/gates are designer **proposals awaiting user review**, not separately affirmed decisions. [Thief detailed design](classes/THIEF_TIER2_DESIGN.md), Round6 questions. Appearance and advancement quest remain deferred; no Unity code changes.

**THIEF ROUND 6 LATEST OVERRIDE (2026-10-22) — AUTHORITATIVE USER CHOICES:** Envenom and Fan of Knives MUST get MORE ranks than Stealth. The former designer 20/14/12/14/12/8 rank proposal is SUPERSEDED by a rebalanced, fully audited **DESIGNER PROPOSAL Stealth16 / Envenom18 / Crit buff10 / Fan18 / Smoke10 / Crit passive8 =80**. Gate plan: Stealth first level1 and every5; Envenom first1/every4; Crit buff first5/every8; Fan first1/every4; Smoke first6/every7; passive first1/every10. All levels1–80 have legal purchases (last gate77), original user-delegated rank math honored; exact caps/gates still proposal pending numeric approval. Proposed cap16 yields smooth Stealth duration `4+4×(r−1)/15` and first basic-opener multiplier `1.1+0.9×(r−1)/15`. **USER APPROVED** Smoke Bomb Physical Tech total RAW/sec `(2+0.08×AGI)×(1+0.05×(r−1))`, four quarter-size ticks/sec; **USER APPROVED** Envenom Physical Nature Poison RAW per 1s tick `(2+0.10×AGI)×(1+0.05×(r−1))`, 3s nonstacking, refresh preserves existing clock, initial normal Physical skill stab RAW2× basic. **Fan of Knives** moving-usable 4m/360°, hits each enemy exactly ONCE with non-elemental Physical damage, AGI/rank scaling (raw coefficient open). **USER APPROVED STEALTH-BREAK:** Envenom, Fan, Smoke Bomb AND ACTIVATING crit buff break concealment, while only first normal BASIC attack breaking stealth gets opener damage; incoming damage still does not break it. Initial proposed Mana/CD: Stealth24s/20, Envenom8s/12, Crit buff18s/16, Fan9s/18, Smoke18s/20 — **DESIGNER PROPOSALS NOT APPROVED**. [Active Thief design and Round7](classes/THIEF_TIER2_DESIGN.md). No Unity edits; model and advancement quest remain deferred.

**THIEF ROUND 7 — 2026-10-22 AUTHORITATIVE USER APPROVALS & TWO OVERRIDES:** Exact six-skill 80-rank caps/level gates are NOW **USER APPROVED for initial playtesting**, not merely designer proposals: Stealth16, Envenom18, guaranteed-basic-crit buff10, Fan of Knives18, Smoke Bomb10, crit+AGI passive8. The full per-rank gate table in [Thief Tier II source](classes/THIEF_TIER2_DESIGN.md) is valid with a purchase at every class level1–80; last gate77. **Approved CDs/Mana:** Stealth **24s/20**, Envenom **8s/12**, crit buff **18s/16**, Fan **9s/18**, Smoke **18s/20**. **Approved Fan RAW non-elemental Physical damage per enemy** `(8+0.45×AGI)×(1+0.06×(FanRank−1))`, once per enemy within4m, usable while moving. When Stealth ends, enemy reacquisition resumes using the remaining **5% stored threat PLUS new generated threat**; the deleted95% never returns. **Smoke's 0.25s Physical Tech damage ticks AND 20% basic miss stop IMMEDIATELY once an enemy leaves its 4m zone**, while Envenom's attached Physical Nature Poison remains on a moving victim until normal expiry. **NEW USER CORRECTION #1: activating the guaranteed-basic-crit self-buff DOES NOT BREAK STEALTH** (contradicts superseded Round6 wording). **NEW USER CORRECTION #2: Envenom STAB that BREAKS STEALTH GETS THE STEALTH OPENING DAMAGE MULTIPLIER (1.1×→2×), LIKE THE NORMAL BASIC OPENER** (contradicts old Round6 'basic only' opener). Whether Envenom's accompanying DoT ticks receive this bonus is not yet user resolved; **applying opening multiplier to direct stab only is designer suggestion pending next round**. Envenom remains a SKILL hit, NOT a normal basic for the basic-only 100%-crit buff; a buffed stealth-breaking BASIC can crit, buff itself preserves concealment. Fan and Smoke still break Stealth with NO opener bonus. All previous conflicting statements should be read as historical/superseded. **DESIGN ONLY, NO UNITY CHANGES.**

**THIEF FINAL ROUND 8 — 2026-10-22 USER-APPROVED; GAMEPLAY DESIGN COMPLETE (NOT IMPLEMENTED):** On Envenom breaking Stealth, the Stealth rank-dependent opener applies **ONLY TO THE DIRECT 2×normalBasicRaw DAGGER STAB**, never the attached 3s Poison DoT; at Stealth16 opener multiplier2× gives **4×normalBasicRaw direct** before defenses. Guaranteed-basic-Crit buff **DOES NOT BREAK STEALTH**, but its 1–4s duration **TICKS DOWN NORMALLY WHILE HIDDEN**; buff only guarantees crit on NORMAL BASIC attacks and NOT Envenom. A first BASIC or Envenom attack breaking Stealth **CONSUMES THE OPENING BONUS EVEN ON MISS OR FULL SHIELD ABSORPTION**, not held for successful HP damage. Ordinary movement, incoming damage, POTIONS, inventory and UI use preserve invisibility; actual LOOTING, NPC dialogue and WORLD-OBJECT interactions reveal with no opener. Five Round8 answers approved and **THIEF TIER II GAMEPLAY INTERVIEW IS COMPLETE**. First playtest only, not actual implementation or passing tests. User explicitly approved start of **PSION** (the final normal World1 TierII) next. Thief 3D MODEL remains DEFERRED to separate working chat/Codex review of downloaded Rogue/Thief/Assassin/Ninja assets, its advancement mentor/quest until AFTER big city. [Authoritative completed Thief Tier II](classes/THIEF_TIER2_DESIGN.md). **Psion Round1 ACTIVE:** [Psion Tier II design](classes/PSION_TIER2_DESIGN.md). After Psion: Quillboar area → major city. No TierIII design until World2. No Unity or map changes.

**PSION ROUND1 PARTIAL USER IDEAS (2026-10-22) — USER HEADING OUT, ASK AGAIN LATER:** Psion, the normal eighth TierII under Magically Touched Novice, must mix **shorter-ranged RANGED autoattacks and spells**. Future World2+ descendants envisioned: at least one **autoattack-focused** and one **spell-focused** branch; NO TierIII class names, skills or branch approvals. The user specified core concepts **Psychic Bolt: single-target MAGICAL hit with STUN, METAL element TENTATIVE ONLY**; **Psionic Blades: autoattack passive grants CRIT and ignores some FLAT DEFENSE (exact Physical vs Magical defense OPEN)**; **Mental Image: STATIC location-placed clone that attacks within range for 10% of Psion normal autoattack damage per hit and EXPLODES at duration expiry OR if killed**; **one AoE spell (unspecified)**; and **MAYBE a shield (not selected)**. Psion autoattack damage should derive from **both AGI and INT**; user floated "**0.45× normal contribution from each**" as a TENTATIVE example, NOT an exact approved coefficient. Psion RANGED BUT SHORTER RANGE than other ranged classes (exact meters OPEN). **ELEMENTS NOT YET CHOSEN**, despite Bolt's tentative Metal idea; no Psychic element approved. Resource, weapon, appearance and advancement quest unanswered. [Psion Round1 user answers / deferred Round2 five questions](classes/PSION_TIER2_DESIGN.md). User requested questions AGAIN LATER, not now; do not advance Psion to finished/implemented. No Unity changes.

**CLASS IMPLEMENTATION SCOPE:** This is an explicit **DESIGN/DOCUMENTATION-ONLY** conversation. No Unity source, scene/world creation, builds, saves, NPC moves or implementation tickets without later instruction in the separate DiceFree implementation workflow. User approval of the eight-class roster is NOT evidence eight classes are already playable or coded. Final game names of Thief/Psion/Acolyte, their visuals, weapons and kits can still be tuned by their own upcoming interviews, while preserving the currently approved class identities unless later changed.

### Long-term class-web scale

A **very large class graph/web is an explicit long-term goal**.

Do not optimize the design around keeping the final class count small.

The tree also does not need to be development-symmetric:
- one class may initially have one implemented successor;
- another may already have three;
- additional descendants can be filled in over time;
- higher-tier concepts can take longer to design and implement.

The architecture must support arbitrary branch counts, convergence requirements and partial/asymmetric content without requiring placeholder classes or giant hard-coded trees.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md` for multi-lineage requirements, visibility/discovery and class-web rules.

### No direct class creation

Unlocking/discovering a Way never creates it from the menu.

A missing class save is always created through an eligible earlier class's actual advancement quest/event.

This remains true when a much later-tier Echo-wide event unlocks an earlier-tier Way.

If a class save is permanently deleted, recreate it by advancing into it again from an appropriate earlier class; Echo-wide discovery and milestones remain intact.

### Universal rank-zero and ordinary basic-attack rules (user correction 2026-10-18)

**Approved design direction for ALL classes and tiers:**

- A newly created class manifestation begins with **every class ability/passive at rank 0**, even if that ability is eligible to learn at level 1. Do **not** automatically grant free rank-1 starter abilities.
- Every new manifestation starts at **class level 1 with exactly one unspent class skill point**; gaining levels grants more skill points under its class rules (the current common pattern is one per level).
- A player may spend that point on an eligible rank-0 skill to learn rank 1. Other skills remain rank 0 until purchased; class-specific level requirements may delay eligibility. Do not equate a visible locked rank-0 skill with an already learned skill.
- **All ordinary/basic/autoattacks deal normal, non-elemental damage**; do not implicitly make them Fire for Mage, Light for Priest, etc. A class's weapon, reach, animation, attack channel (Physical/Magical) and primary-stat scaling are independent design choices. Explicitly authored equipment/abilities may separately add elemental damage where appropriate.
- This **supersedes** prior prototype-level choices such as the Mage design's **free learned level-1 ability ranks**. Existing gameplay code and current class specs may still implement those legacy choices; treat that as **an implementation discrepancy to reconcile in the development chat**, not as a claim that every live class already conforms.
- This is a rules clarification only. No Unity code, player save or balance was modified by this documentation change.

### Tier II skill-point capacity, varied caps and legacy resets (user decisions 2026-10-19)

- **CONFIRMED for Tier II Priest, Greatblade and Mage:** the class's **total available skill-rank investment capacity should use almost all earned skill points by approximately class level 80**, i.e. the normal Tier II level-60 advancement threshold plus ~33% extra levels. This is **not** a requirement to automatically advance at 60; characters may continue leveling.
- **CONFIRMED:** individual ability **maximum ranks are based on their effects, balance and performance**, not a universal cap. A high-damage spell can have many ranks while a special utility can have few; specifically **Mage's Math Barrier retains its existing five-rank maximum**.
- **CONFIRMED:** Greatblade and Mage retain the exact first-availability/unlock levels in their existing implemented class designs, but **all skills begin at rank 0** with **one spendable point at class level 1**, no free rank-1 abilities.
- **CONFIRMED:** existing Greatblade and Mage saves under the old four-free-starting-ranks economy require a **silent automatic one-time full class-skill reset and refund**, **not grandfathered free ranks**. Preserve the same class level, XP, quest/equipment/inventory and unrelated save progress, with recoverable backup, per-manifestation migration marker and idempotence. Refunded spendable budget is **min(saved class level, total class ranks)** (Greatblade max 78, Mage max 80), not class level plus four free ranks. Level-80+ saves are included; never discard XP or levels, and never manufacture unspendable overflow.
- **CONFIRMED (2026-10-19):** Greatblade and Mage use **skill-specific rank upgrade cadences**: certain abilities may gain one rank per class level while more powerful/technical skills require multiple class levels between ranks. Do not enforce one global upgrade interval. An example of Half-Life needing approximately **4 class levels between ranks** is not yet its final gate table.
- **CONFIRMED:** Mage max ranks **Ember Bolt 18, Frost Snare 12, Arc Lightning 12, Quicksand 10, Half-Life 8, Math Barrier 5, Elemental Weaving 8, Mana Attunement 7** (total **80**); Math Barrier's five-rank formulas stay intact. Greatblade max ranks **Wide Arc 20, Spin Dash 16, Overhead Breaker 18, Battle Cry 8, Greatsword Mastery 8, Heavy Rhythm 8** (total **78**). Mastery upgrades base STR/greatsword damage at every rank; combat features unlock at skill ranks **3 (Wide Arc haste), 5 (wider cleave), 7 (Overhead Breaker shield)**; rank 1 grants initial passive; ranks 2/4/6/8 add only baseline stat/damage improvement.
- **CONFIRMED:** increasing skill rank caps is allowed to use diminishing or capped secondary effects to avoid runaway crowd control, defense debuffs and Momentum, while primary rank improvements remain meaningful.
- **CONFIRMED:** existing Greatblade/Mage legacy saves are automatically migrated **on first load**, resetting/refunding class skills **without a notification or confirmation prompt**. Keep a recoverable backup, per-manifestation idempotence, correct level-derived skill-point pool and all unrelated character/quest/equipment progress.
- **CONFIRMED Greatblade alternating passive:** Mastery rank **1** grants STR/damage, **3** unlocks temporary Wide Arc haste, **5** widens Wide Arc, **7** grants an Overhead Breaker **METAL-element shield** for **5 seconds**, capacity **100 + 5% of Greatblade maximum HP**; shield is NOT a heal. **Every Mastery rank 1–8 additionally grants +1 STR and +2% greatsword damage**. Old ranks 2/3/4 trigger effects are deliberately moved to 3/5/7. This Metal shield uses the **latest global elemental rule (Defender Round 1)**: consumes **0.8 shield HP per matching Metal damage blocked** or **1.2 shield HP per other/untyped ordinary damage blocked**, equivalent to 1.25 matching / ~0.833 nonmatching blocked per 1 shield HP. Repeated Overhead Breaker **replaces/refreshes** the existing shield. Pure damage remains separately specified by the shield framework.
- **CONFIRMED timing:** Greatblade Wide Arc and Overhead Breaker, and Mage Ember Bolt can rank up **every class level**, starting from each skill's existing first-unlock level. Mage Half-Life rank levels **18/22/26/30/34/38/42/46**; Mage Math Barrier ranks at existing **6/12/18/24/30**.
- **CONFIRMED GLOBAL POINT CAP AND UI (2026-10-19):** `spendableBudget = min(classLevel, Σ(maxRank of class-owned skills))`; `unspent = max(0, spendableBudget − Σ(purchased ranks))`. Stop accruing **spendable** points once the class reaches its complete rank budget; normal XP, leveling and stat growth continue. **Do NOT add the Novice-style all-stat passive to every class.** When new ranks/classes are introduced in later updates, budget may increase according to existing level without losing earned progression. **Only show the skillpoint HUD badge / level-up reminder when unspent > 0 AND at least one rank is actually purchasable now**; hide it when only later rank gates remain, even if points are banked, and hide it once maxed. Banked point count should remain accessible through the class skill UI.
- **CONFIRMED designer delegation:** allow one point to be spent at **every character level until reaching the class's available total**, despite different skill-specific rank intervals. The audit's provisional per-skill unlock schedules were checked across **every level 1–78 Greatblade / 1–80 Mage** to satisfy `Σ(eligible ranks by level) >= min(level,total ranks)`; the final schedules, numeric high-rank balance and C# integration should be verified in playtesting. [Full proposed gate schedule and validation](TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md).
- **DEFENDER design-only progression (2026-10-19):** six Tier II skills (Shield Bash, Challenge, Bulwark, Shield Sweep, Hold the Line, working-name Iron Guard). The user delegated weighted rank caps/gates; designer **proposed 18/10/16/20/10/6 = 80 total** with a validated one-point-per-level path to level80. **User-approved 2026-10-19 initial playtest ranks and gates**, verified for every level 1–80; Iron Guard PDEF+MDEF +500 per rank, 20 Resolve, 2s lasting through CC, with immunity vs normal CC still open. Resolve decay 1 every 3s outside combat only. Ranger's new design interview is documented at [Ranger Tier II](classes/RANGER_TIER2_DESIGN.md), using the already-decided Wood Elf identity and possible physical ranged branch. Iron Guard grants +500 DEF per rank for 2 seconds while the player cannot act, reserved Tier III damaging counter evolution. See [Defender tier II design](classes/DEFENDER_TIER2_DESIGN.md). The same bounded rank-point budget and actionable-only reminder must apply when eventually implemented.
- **Superseded workflow/status (2026-10-21):** Greatblade/Mage rank-zero budgets, approved gates/caps, actionable reminder behavior, and a silent one-time legacy reset/refund have now been implemented locally on `integration/world1-foundation`. The changes are **not yet Unity-verified** because the connected Editor is blocked by its recovery modal. Earlier lines describing this as design-only record the state as of 2026-10-19.

### Greatblade/Mage rank-zero implementation checkpoint (2026-10-21)

The implementation uses `min(class level, total rank capacity)` as the spend budget, all ranks begin at zero, and each level has an eligible purchase path. Greatblade has 78 total ranks; Mage has 80. The HUD reminder is visible only when at least one upgrade can be bought now, while banked points remain shown in class skill tooltips. The exact gates/caps and approved effect ceilings are listed in the [rank audit](TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md).

Schema 7 introduces a per-manifestation marker and silently zeros only known Greatblade/Mage skill ranks when loading a legacy section; it preserves XP, level, quest, gear, inventory, currency, resources, unknown skill entries, non-target manifestations, and opaque save sections. Before the next successful save, the store retains the exact verified original as `.pre-skill-rank-v1-original`. Idempotence and backup behavior have an Editor validator, but the validator and existing Mage/Greatblade Play Mode regressions still need to run in Unity.

### Tier-1 skill-rank pattern

Current Tier-1 direction:
- 5 skills;
- 6 ranks each;
- 1 class skill point at level 1;
- +1 class skill point per level;
- exactly 30 total points by level 30.

This clean fit is specific to Tier 1 and is **not** a universal rule for later tiers.

## Class identity

Each class defines:
- allowed equipment families;
- weapon categories;
- melee/ranged basic attack;
- attack range/speed;
- primary attribute(s);
- primary-stat coefficients;
- resource;
- fixed ability kit;
- passive mechanics;
- advancement quest;
- visual identity;
- **body/species/form identity where applicable**.

A Way is allowed to transform the Echo substantially. Classes are not constrained to one human body or even one universal humanoid rig.

Settled Tier-2 visual identities:
- **Ranger -> Wood Elf** — **Playable Tier II physical ranged DPS** from level30 Physically Blessed Novice after accepting wounded Ranger's quest near Trimery Priest, killing existing Alpha Wolf after acceptance, and returning. Two-handed bows: shortbow attacks faster with lower per-shot dmg; longbow slower with higher per-shot dmg (comparable longbow ~25% more damage and ~20% slower as ITEM design guideline); **no equippable quiver**, both share Ranger's class-based range. Move during autoattack windup cancels unfired arrow. **Mana only, no pet**; Hawk Dive is one-time hawk attack.
  - **USER-APPROVED skill roster and level-gate schedule (80 ranks):** **Aimed Shot20, Exploding Arrow18, Arrow Rain16, Hawk Dive10, Evasive Step8, Bow Mastery8**, rank0 starters with class level1 one spendable skill point and a purchase possible every level through 80; exact eligible class levels in the [Ranger Tier II class spec](classes/RANGER_TIER2_DESIGN.md).
  - **Confirmed combat:** Hawk Dive 15s CD and Hunter's Mark 8s, 10% damage bonus to that Ranger's attacks/skills, one mark target/Ranger. **Aimed Shot's extra marked-target RAW damage** = `0.5 × current AGI × (1 + 0.05 × (AimedShotRank−1))`, in addition to the mark %. **Exploding Arrow** replaced historical Multishot; **targeted arrow CANNOT MISS**, directly strikes enemy then **Physical Gun 2.5m-radius splash CENTERED ON HIT TARGET** (primary direct + splash, secondary splash); earlier assistant proposal about missing and landing elsewhere **SUPERSEDED/INVALID**. **Arrow Rain** channels **1.5s** with big upward then small upward arrow; player MOVEMENT, STUN or explicit INTERRUPT cancels channel without rain, but normal damage alone does not. Once successfully completed, **4 damage waves over 2s keep falling regardless of later Ranger CC/movement**, in **3m radius**, **0.4× current basic shot raw per wave at rank1**, increasing with rank. **Evasive Step:** 3–4m mouse-directed leap, 3s +10% move speed and +1pp basic-attack-only evasion/rank; cannot cast rooted/stunned. **Bow Mastery:** +2 AGI/rank, +0.1m normal+ Aimed Shot range/rank, +1pp crit/rank at 1.4× on basic+ Aimed ONLY; no other Ranger skills inherit crit.
  - **RANGER-030 first-playtest balance:** Aimed Shot cooldown6s/Mana10, Exploding Arrow12s/20, Arrow Rain20s/30, Hawk Dive 15s/Mana15, Evasive Step15s/12, Bow Mastery passive; damage coefficient growth and high-rank mark growth follow the first-playtest table in the [Ranger spec](classes/RANGER_TIER2_DESIGN.md). Future Tier III Beastmaster/Marksman with class-range differences, or one-handed crossbows/dual-pistol Gunslinger, remain **speculative only**, not established class branches.
- **Wizard -> High Elf**.

Tentative future directions:
- Berserker may be an Orc;
- one Tier-4 tank may be a Centaur.

Those tentative examples are not locked class canon yet.

Architecture/art must support different proportions, rig families and potentially non-biped forms without making those classes hacks or exceptions bolted onto a human-only player architecture.

### Authored body presentation

Each class has **one authored body/sex presentation** rather than player-selectable male/female variants for that Way.

A class can be visibly male, visibly female or androgynous.

The visual transformation should generally become more pronounced as the class lineage specializes:
- Novice begins deliberately neutral/androgynous;
- early branches can make modest directional changes;
- later Ways can become strongly species-, body- and silhouette-specific.

The form is part of the Way's authored identity, not a separate character-creator axis.

A descendant Way may fully redefine the body/species/form again. Visual inheritance is not a promise that every descendant remains the same species as its parent; advancement can transform the Echo again when that is part of the new Way's identity.

Body form can also carry authored gameplay footprint/collision data. Different class forms may use different hitbox/agent dimensions rather than treating size as presentation-only.

Attack/basic-attack/ability reach remains separately authored and is not automatically derived from body size.

Required world progression must remain accessible to every intended playable form. Large forms can use authored navigation/clearance forgiveness at bottlenecks rather than being locked out because of their footprint.

### Equipment permission inheritance

A descendant Way inherits its parent's allowed equipment/form-family permissions by default.

A descendant may:
- add new equipment families;
- explicitly remove/restrict inherited families when its body/class identity changes;
- override specific compatibility where an inherited appearance no longer fits.

This mirrors visual-appearance inheritance: lineage compatibility is the default, explicit divergence is the exception.

### Gear reconciliation during advancement

If the new Way/form is incompatible with equipment currently worn by the parent snapshot, advancement reconciles it safely:
1. keep compatible equipment equipped where normal slot/permission rules allow;
2. unequip incompatible/conflicting items into the child manifestation's carried inventory when there is room;
3. if carried inventory cannot safely retain them, send those items to the Echo-wide shared bank as a fallback;
4. if neither destination can safely retain every displaced item, the advancement transaction must fail before mutation rather than destroy/drop/overwrite gear.

This reconciliation is part of the atomic advancement/fork transaction.

The parent manifestation remains unchanged with its own original equipment/state.

### Optional transformation presentation

An advancement may optionally define a visual transformation presentation.

The class/manifestation fork is authoritative; transformation VFX/animation are presentation layered on top and may be simple, bespoke, or absent.

Weapon categories are created according to actual class needs rather than predefining every possible weapon.

## Fixed kits and advancement

Classes use fixed kits.

Advancement may:
- add abilities;
- evolve abilities;
- replace abilities;
- change passives;
- replace resources;
- change stats;
- alter equipment permissions;
- change visual form.

## Convergence and Echo-wide class evidence

Class progression is not limited to parent -> child lineage requirements.

The Echo permanently records meaningful class/tier milestones account-wide.

A class may require:
- one lineage milestone;
- several lineage milestones together;
- class evidence combined with quests/items/achievements/discoveries;
- nested AND/OR requirement expressions.

The same resulting class can be reached through different qualifying parent manifestations, but the target class normally uses the same advancement quest/NPC/location regardless of which parent supplies the timeline fork.

A single prerequisite combination can also unlock multiple different convergence classes.

Deleting/cleaning up an old manifestation does not erase class milestones the Echo already earned.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md`.

## Secret classes

Secret classes may branch from many points and use unusual conditions:
- achievements;
- items;
- exploration;
- encounters;
- account discoveries;
- extreme overleveling;
- strange class-specific feats.

Secret classes use the same advancement thresholds after entering their lineage.

## Healer viability

Healer/support lineages must be fully viable for solo progression.

They should have:
- a real damage rotation;
- sufficient solo damage to quest/farm/grind;
- healing/support tools that become especially valuable in groups;
- class-specific ways to connect offense and sustain where appropriate.

## Open questions

- exact archive UI and permanent-delete confirmation UX;
- exact secondary effect of each attribute;
- universal vs class-specific Vitality coefficient;
- exact second-tier classes;
- weapon categories as classes require them;
- number of character slots.

## Additional settled class-progression rules

### Attribute growth

Primary-attribute growth is determined by **class and gear**.

Players do not receive a general pool of manual VIT/STR/AGI/INT/SPI points to distribute on level-up.

This preserves strong authored class identity while still allowing build variation through gear, skills, effects and class choice.

### Respec philosophy

DiceFree does **not** need a broad always-available respec system as a core progression loop.

The early Novice/onboarding stage may provide free respec support so players can learn the game and experiment before committing to later Ways.

Later classes should not assume routine respec availability unless a particular class/content design explicitly needs it.

### Advancement events

Advancement quests/events are decided **case by case**.

The first Tier 0 -> Tier 1 advancement is intentionally simple: the Novice is told to **go to the mountain** rather than completing a conventional bespoke class trial.

Later advancements can use authored class-specific quests, trials, rituals, discoveries or other requirements as appropriate.


## 2026-10-26 — All eight normal World1 Tier II gameplay class designs complete (Psion Round 20)

**Latest user confirmation:** The final eighth regular Tier II, **Psion** (Magically Touched Novice branch), completed its **first-playtest gameplay design interview**. This supersedes earlier historical statements in this document describing Psion as Round1/ACTIVE/unanswered. Kit: **Psychic Bolt, Psionic Blades Mastery, Mental Image, Lightning Storm, Metal Shield**. The precise 80 rank-gate schedule is **USER APPROVED**: caps **20/18/18/12/12**, one spendable point at level1 and every level through80, all 80 eligibility gates by level76, audited one-legal-point-per-level access. All details: [Completed Psion design](classes/PSION_TIER2_DESIGN.md).

**Gameplay designs for the eight planned normal Tier II classes are now covered; this does NOT claim implementation or playtest balance completion.** Psion's appearance/model and advancement mentor/quest are explicitly deferred until after the Quillboar region and when designing the major city. Next content-design sequence: **Quillboar area → big city**. No Tier III design until World2.

## 2026-11-25 — Berserker Tier III Round 1, user-approved identity

After Ranger→Marksman design, next class design is **Greatblade→Berserker**, a **direct Tier III Greatblade evolution** with two-handed greatswords and **sustained melee DPS**. Instead of Tier II Momentum, Berserker gains **Rage by dealing and receiving damage**; **lower HP increases outgoing damage**, with actual scaling still to design. Retained recognizable abilities are **Wide Arc, Spin Dash, Overhead Breaker and Heavy Rhythm**. Battle Cry was unselected, while Greatsword Mastery was not in the Round 1 question list and is therefore undecided. Do not copy Tier II ranks (Wide Arc20/Spin Dash16/Overhead18/Battle Cry8/Mastery8/Heavy Rhythm8) into Tier III or treat proposed **150 Tier III ranks** as separately approved yet. Possible sibling concepts **Blademaster, Juggernaut, Warbringer** are designer suggestions only, not canon or promises. [Full Round 1 design and Round 2 questions](classes/BERSERKER_TIER3_DESIGN.md). No Unity implementation authorized by this design round.

## 2026-11-25 — Greatblade/Berserker Round 2 and previous Samurai plans

**Berserker current confirmed direction:** Rage (working name, replacing Momentum and gained dealing/taking damage), **gradual damage increase as HP falls**, sustained DPS with Wide Arc/Spin Dash/Overhead Breaker/Heavy Rhythm, **Heavy Rhythm now improves Rage gain/retention**. User prefers **a Rage-consuming heal/shield or other bounded sustain**, not conventional lifesteal tied to potentially huge damage numbers; exact healing/shield ability remains open. **Warbringer→Warlord** with Battle Cry development and placeable buff banners is a favored **possible future class line**, not approved Tier III/Tier IV class implementation. Multiple distinct viable tank archetypes welcome, but no new Juggernaut role or class lineage locked.

Older authored source [Tier II Core v0.2](TIER2_CORE_CLASSES_DESIGN_V02.md) already proposed **Greatblade→Swordmaster (Tier III)→Samurai (possible Tier IV)** and a SECRET **Witcher-style hybrid Tier III** requiring an eligible Greatblade and same-Echo **Magically Touched Novice at Lv30+** (IP-distinct naming required); proposals, not finalized class mechanics. [Tier II Core v0.1](TIER2_CORE_CLASSES_DESIGN_V01.md) originally floated **Juggernaut as a Defender successor**, which should be preserved as a candidate, not silently assigned to Greatblade. A **two-handed AXE specialization for Berserker while still equipping qualifying 2H swords**, 2H sword Swordmaster and katana Samurai are USER-RAISED IDEAS awaiting weapon-rule confirmation, not edits to existing class gear legality. [Berserker Round2](classes/BERSERKER_TIER3_DESIGN.md). No Unity code or assets modified.

## 2026-11-25 — Berserker Tier III Round 3 approved and Defender sibling candidates

**Berserker** evolves Greatblade into a **two-handed AXE specialist**, still able to equip qualified 2H swords. Rage gained from outgoing/incoming damage is a **spendable** combat resource, not a passive held-Rage damage multiplier. **Gradual missing-HP outgoing damage bonus peaks at +40% at 1% HP** (first-playtest balance target). **Timed ELEMENTAL SHIELD spends Rage**; shield element, absorb scaling, duration/cooldown/cost open. **Leftover shield→healing upon expiry** was suggested, potentially as a **Tier IV only** upgrade, not yet settled. Heavy Rhythm improves Rage gain/retention. Berserker's retained Wide Arc, Spin Dash and Overhead Breaker identities stand; current Greatsword Mastery replacement/upgrade under axe specialty undecided.

Potential other Greatblade pathways: **Swordmaster→Samurai**, **Warbringer→Warlord** (Battle Cry and banners), secret same-Echo martial/magic candidate; mechanically distinct, not implemented or globally finalized. **Juggernaut's parent remains undecided between Defender and Greatblade**, depending on class coverage. Historic Defender candidates: **Guardian** protector, **Templar/Paladin** martial-magic protector, **Juggernaut** bruiser, and later **Vanguard** interceptor; Iron Guard-inspired damage storage/retaliation reserved for later Tier III design. Uneven numbers of evolutions are acceptable but very large disparities are undesirable. See [Berserker Round 3](classes/BERSERKER_TIER3_DESIGN.md). Documentation-only decisions.

## 2026-11-25 — Berserker Tier III Round 4: 100 Rage, future shield healing, class-role flexibility

User approved **maximum Rage 100**, generated by dealing and receiving damage and **decaying outside combat**; Heavy Rhythm increases generation/retention. Timed **Rage-consuming elemental shield** is part of Berserker Tier III; **unconsumed shield strength → heal on expiration** is expressly reserved as a **future Tier IV Berserker improvement**, not Tier III. **Coffee** is the user's preferred playful shield element/theme; **Fire** is acceptable fallback, but precise gameplay element remains **UNDECIDED**. User favors **Great Weapon Master** name/nod for weapon mastery covering favored two-handed axes plus eligible two-handed swords. Four original retained skills (Wide Arc/Spin Dash/Overhead Breaker/Heavy Rhythm) + Elemental Shield + favored evolved weapon mastery = **6 current concepts**. User likes both new **Frenzy** and **Rage finisher** but skill count, duplication with Overhead Breaker and rank cap are not yet resolved; possible consolidation yields 7 instead of 8 concepts, pending approval.

User wants **Greatblade to have an eventual tank evolution** and **Defender to have an eventual DPS evolution** (Vanguard mentioned as candidate); Juggernaut may be reassigned to Greatblade depending on Defender branch distribution. External comparison inspirations include FFXIV 2026 announced **Bastion** dual-greatshield main tank and WoW Season of Discovery's Warlock/Rogue/Shaman tank roles. Those roles are **not** automatically canon DiceFree branches. Preserve distinct tank identities and avoid false balanced/identical numbers of class branches. Full [Berserker Tier III Round4](classes/BERSERKER_TIER3_DESIGN.md). No Unity implementation or gameplay verification was performed.

## 2026-11-25 — Berserker Tier III Round 5 merged mastery and Coffee element

Latest USER approval: **Rage Breaker** is Berserker's evolution of **Overhead Breaker** and its **only heavy Rage-consuming finisher**; do NOT add another. **Anger Management** (favored working name) replaces Heavy Rhythm and combines Rage generation/retention, the smooth low-HP damage modifier (up to +40% at1%HP, initial target), and former Great Weapon Master effects for all eligible two-handed axes/swords equally (no axe-only damage bonus; axe preference is a style/equipment identity). **No separate Great Weapon Master passive** in Berserker. **Coffee** is a genuine DiceFree element already present as `element.magical-coffee` and used by Acolyte poison. Berserker uses **Coffee-element Rage shield** under standard elemental shield rules, **not Fire**. Tier IV may gain remaining shield→heal on expiry; TierIII has no such heal. Current proposed set: Wide Arc, Spin Dash, Rage Breaker, Anger Management, Coffee shield, and Frenzy (Frenzy still awaiting numerical/final role confirmation). Total Berserker rank caps/skill points still to be decided, with 150-point TierIII pattern a recommendation but not yet explicit Berserker approval.

Greatblade→Juggernaut **future tank** and Defender→Vanguard **future DPS** provisionally reserved; Guardian/Templar remain other Defender possibilities. No Unity code changed. See [Berserker Round5](classes/BERSERKER_TIER3_DESIGN.md).


## 2026-11-25 — Berserker Round 6: consolidated five-skill design

**Latest user decision:** Berserker has FIVE skills, not six. **Frenzy was explicitly canceled**; instead Rage Breaker (evolution of Overhead Breaker) **gives a temporary attack-speed buff after use** while remaining the one Rage-consuming heavy finisher. Other skills: Wide Arc, Spin Dash, Anger Management (confirmed name, merged Heavy Rhythm + 2H axe/sword mastery + low-HP bonus), and Coffee Shield (Coffee-element self-only, 25 Rage / 5 seconds initial test; no expiry healing until Tier IV). Every Anger Management rank improves Rage gain, missing-HP damage and eligible 2H weapon damage, selected ranks improve Rage retention; eventual maximum bonus +40% damage at 1% HP. Rage max100, dealt/taken damage normalized against max HP for gains, out-of-combat decay, precise gain/cost/decay open. No separate Frenzy, Great Weapon Master or duplicate finisher. Berserker's 150-rank class budget/caps are not yet individually approved; follow [Round6](classes/BERSERKER_TIER3_DESIGN.md). Docs only.


## 2026-11-26 — Berserker Round 7 newest user-approved rank caps and skill defaults

Exactly **150 Tier III Berserker skill ranks with one purchase/class level 1–150** are approved: **Wide Arc 35 / Spin Dash 30 / Rage Breaker 30 / Anger Management 25 / Coffee Shield 30** (sum150). Rage Breaker is the **fixed 30-Rage** finisher that grants **4s of +2% attack speed per purchased Rage Breaker rank**, refreshes but never stacks; at rank30 +60% attack speed. User removed Frenzy as a separate button. Coffee Shield is self-only **Coffee** element, first-test **25 Rage, 5s duration, 15s cooldown**, with absorption endpoints **5% current Berserker MaxHP at rank1 to 35% MaxHP at rank30**; rank-by-rank progression formula pending approval; leftover absorption→healing is future Tier IV only. Rage max100, gains normalized by **dealt damage / enemy max HP** and **taken damage / self max HP** with per-hit caps, out-of-combat decay; exact numeric gain and decay unspecified. Anger Management max25, combines 2H axe/sword mastery, generation/retention and missing-HP bonus reaching previous max-rank +40% at1%HP. Full skill eligibility level gates, damage coefficients, attributes and advancement quest still open. See [Berserker Round7](classes/BERSERKER_TIER3_DESIGN.md). This is documentation only.


## 2026-11-26 — Berserker Round 8 latest choices and boss Rage-generation caveat

Current five-skill 150-rank Berserker budget remains **Wide Arc35 / Spin Dash30 / Rage Breaker30 / Anger Management25 / Coffee Shield30**. Round8 approves **linear Coffee Shield absorb** from 5% maxHP at skill rank1 to 35% maxHP at rank30, keeping first-test 25 Rage/5s/15s cooldown/self-only real Coffee element and no TierIII expiry heal. Rage Breaker's post-use 4s +2% attack speed per purchased rank requires **successful enemy hit**, including hits fully blocked by shields; not a cast with no hit. **Out-of-combat Rage decay 5/sec BASE**, reduced by Anger Management (exact reduction/rank open). First skill unlock levels must be **staggered**; exact rank gates and purchase-every-level audit remain open.

**Do not finalize the outgoing Rage formula without boss review.** User initially selected proposal **5 Rage / 10% target MaxHP dealt**, but rightly questioned high-HP bosses and proposed **1 Rage / 1% target MaxHP dealt**. Both are dependent on enemy HP percentage and may generate too little Rage against bosses, so outgoing coefficient/denominator are **open**, candidate solutions include fixed per-successful-hit gain or scaling to caster MaxHP/expected attack damage with per-hit caps. **Incoming first-test direction is 10 Rage per10% own MaxHP damage received**; specify applied-HP damage vs shield absorption and cap later. No Unity changes. [Berserker Round8](classes/BERSERKER_TIER3_DESIGN.md).


## 2026-11-26 — Berserker Tier III Round 9 newest Rage and unlock schedule

User approved **one base Rage plus 0.1 per Anger Management rank rounded upward per successful BASIC attack**: `1+ceil(0.1*r)`, rank0=1, 1–10=2, 11–20=3, 21–25=4. This replaces an enemy-max-HP-based base attack Rage model, which fails against bosses. Spin Dash gains **2 Rage per enemy hit up to 20 Rage per use**, Wide Arc **costs fixed 10 Rage per use**, and Coffee Shield absorbing enemy damage generates incoming Rage **at same rate as HP damage (1 Rage per 1% own MaxHP of damage absorbed/received)**. Do not count the shielded and HP portions of one damage packet twice. OOC decay starts **5 Rage/s rank0** and reaches **0.1 Rage/s rank25** of Anger Management; intermediate curve undecided. First unlocks are **Wide Arc L1, Anger Management L1, Spin Dash L3, Rage Breaker L6, Coffee Shield L8**. The five rank caps remain Wide Arc35, Spin Dash30, Rage Breaker30, Anger Management25, Coffee Shield30 (150 total), exact later rank-level gates/attributes/quest open. Earlier Rage Breaker fixed30 and post-hit attack-speed buff plus Coffee Shield self 25 Rage/5s/15s and rank1 5%→rank30 35% maxHP linear shielding stand. No Unity changes; [Berserker Round9](classes/BERSERKER_TIER3_DESIGN.md).


## 2026-11-26 — Berserker Tier III Round 10 approved Rage behavior; starting stats next

Basic attacks generate the full fixed rank-based Rage on **successful shield-absorbed hits**, with `1+ceil(0.1×AngerManagementRank)` per basic (rank0=1, ranks1–10=2, 11–20=3, 21–25=4). Anger Management OOC decay curve APPROVED LINEAR: `5−4.9×clamp(rank,0,25)/25` Rage/sec, down to 0.1/sec at max passive rank. Incoming damage grants Rage based on own MaxHP regardless of whether absorbed by self Coffee Shield or dealt to HP, **no per-hit 20-Rage cap** (only Rage max100), avoiding packet double counting. Each enemy can award **Spin Dash's +2 Rage just once per activation**, max20 total. User chose starting attributes and growth as the next design activity; **NO new Berserker stat values have been approved**.

Verified actual TierII Greatblade base **VIT14/STR18/AGI9/INT5/SPR5**, growth **2/3/1/0.5/0.5**, baseHp10 and 15 HP/VIT. Designer-only TierIII Berserker first-playtest proposal **VIT22/STR32/AGI13/INT7/SPR7**, growth **2.8/6.0/1.3/0.7/0.7**, needs explicit user acceptance. Do not describe proposal as implemented class asset or add parent accumulated stats. [Berserker Round10](classes/BERSERKER_TIER3_DESIGN.md). Documentation only.


## 2026-11-26 — Berserker Round 11 AUTHORITATIVE class stats and universal HP rule

**APPROVED Berserker Lv1 VIT25 STR34 AGI10 INT5 SPR7**, and **per Berserker class level VIT+3.0 STR+6.0 AGI+1.2 INT+0.5 SPR+0.8** (growth sum 11.5). Supersedes Round10 designer proposed VIT22 STR32 AGI13 INT7 SPR7 and growth +2.8/+6/+1.3/+0.7/+0.7. Lv150 no-gear STR928 and VIT472. The Berserker's separate TierIII manifestation starts Lv1 with these stats; it does not stack accumulated Greatblade attributes.

**USER GLOBAL RULE: 1 VIT grants 15 maximum HP for ALL playable classes**, not just Berserker and not one coefficient per class. Existing Unity `CombatTuning.hpPerVitality` default is 15 and ActorDefinition override infrastructure exists, so implementation must audit class-specific overrides and modifiers to enforce the shared player-class coefficient; no Unity gameplay files changed during this design round. Base HP, item bonuses and shared status effects remain separate from the 15 HP/VIT contribution. [Full approved Round11](classes/BERSERKER_TIER3_DESIGN.md).


## 2026-11-26 — Berserker Round 12 approved damage mechanics

**User approved basic Physical attack as weapon/base damage +1×STR**, **Rage Breaker guaranteed hit on valid targets**, damaging skills whose **FLAT damage and STR multipliers BOTH scale with their own purchased skill ranks**, and Anger Management's **missing-HP damage bonus applies to all offensive abilities AND normal basic attacks**. Keep the max passive-rank target of +40% outgoing damage at 1% HP, applying the bonus once through shared mitigation pipeline.

**Designer coefficient candidates awaiting approval, NOT LOCKED:** Wide Arc `12+4(r−1)+[1.8+0.10(r−1)]STR` max rank35; Spin Dash `8+3(r−1)+[1.2+0.08(r−1)]STR` max rank30; Rage Breaker `25+8(r−1)+[2.5+0.16(r−1)]STR` max rank30. All are raw Physical before buffs and enemy defenses, and should not add existing baseDamage twice. Previous Rage costs, attack-speed buff, Coffee Shield and Rage economy unchanged. See [Berserker Round12](classes/BERSERKER_TIER3_DESIGN.md). No Unity implementation or test.


## 2026-11-26 — Berserker Tier III Round 13 confirmed coefficients and critical-hit exclusions

**APPROVED** first-test raw Physical formulas, individual rank `r`, resolved caster STR `S`: **Wide Arc** (r1..35) `12+4(r−1)+[1.8+0.10(r−1)]S`, **Spin Dash** (r1..30) `8+3(r−1)+[1.2+0.08(r−1)]S`, **Rage Breaker** (r1..30) `25+8(r−1)+[2.5+0.16(r−1)]S`. At cap: WideArc `148+5.2S`, SpinDash `95+3.52S`, RageBreaker `257+7.14S`. **Wide Arc and Spin Dash are NEVER critical hits**; **Rage Breaker CAN critically hit using normal global crit rules**, independently from its already-approved guaranteed accuracy versus valid targets. The three skill formulas are raw Flat + STR only; equipped weapon's contribution/effects use shared equipment/combat pipeline with **no double-counted weapon base**. Anger Management's missing-HP damage bonus applies once to all damaging skills and basic attacks. All prior Rage rules, stats, shield and 150-point skill-rank totals remain unchanged. **Next: design/audit 150 ranks of first-unlock-aware eligibility gates** (proposed gates require separate user signoff). See [Berserker Round13](classes/BERSERKER_TIER3_DESIGN.md). Design only, no Unity changes.


## 2026-11-26 — Berserker Round 14 crit sources and proposed 150-rank eligibility audit

**User approved:** Berserker has **zero built-in/innate crit chance** from class or passive skills. Rage Breaker CAN critically hit only with eligible **external critical chance** from gear/buffs and a successful shared crit roll; **Wide Arc/Spin Dash NEVER crit**. Guaranteed Rage Breaker hit does not grant a guaranteed crit; basic attacks follow global crit rules without class innate critical bonuses.

**Proposed gate schedule (DESIGNER-ONLY, user approval pending):** WideArc ranks1–4 unlock at levels1/2/3/4 and next ranks every4 through rank35 Lv128; SpinDash rank1 Lv3 then every5 through rank30 Lv148; RageBreaker rank1 Lv6 then every4 through rank30 Lv122; AngerManagement rank1 Lv1 then every6 through rank25 Lv145; CoffeeShield rank1 Lv8 then every4 through rank30 Lv124. **Exhaustive 150-level count audit: no level has fewer eligible individual ranks than earned class skill points**, 150 total gates, no free starter ranks. See [full Berserker gate audit](classes/BERSERKER_TIER3_RANK_GATE_AUDIT.md). Unapproved schedule is not implemented.


## 2026-11-26 — Berserker Tier III 150-rank eligibility schedule USER APPROVED

The user approved the exact previously proposed and mathematically checked first-test progression: WideArc35 (ranks1-4 levels1/2/3/4, ranks5-35 every4 through level128), SpinDash30 (level3 then every5 through148), RageBreaker30 (level6 then every4 through122), AngerManagement25 (level1 then every6 through145), CoffeeShield30 (level8 then every4 through124). Full 150 rank purchases, one skill point per Berserker class level1-150, no free starting ranks. Every level passes enough-legal-ranks audit; rank gating remains a player choice, not auto-allocation. User approval SUPERSEDES prior pending status. [Approved Berserker gate audit](classes/BERSERKER_TIER3_RANK_GATE_AUDIT.md). Advancement quest design next; no Unity changes.


## 2026-11-26 — Berserker advancement quest proposed direction, approval pending on specifics

User favors a **retired Old Berserker NPC teacher** with a partial trial in existing **Crimson Crucible**, Crimson Mire red-slime area. Bristlebreaker is the authored boar-themed guardian **in Cell3**. Its locked cell order requires first defeating **Verdigris Globule (Cell1)** and **Moonfang Alpha (Cell2)**, but user explicitly suggests avoiding mandatory all-six clear. Quest design proposal: Greatblade eligible for TierIII chooses mentor challenge, gets credit for defeating Bristlebreaker, may exit without defeating Cells4–6, and returns to teacher for new Berserker Lv1 manifestation (no parent overwrite). Need separate persistent quest kill-credit and safe-exit handling: existing Abandon resets dungeon, while normal final treasure belongs only to all-six victory. **Only general mentor + dungeon direction is user selected; exact Bristlebreaker target, teacher location/quest dialogue/party participation/exiting not yet approved.** See [Berserker design](classes/BERSERKER_TIER3_DESIGN.md) and [current dungeon facts](CRIMSON_CRUCIBLE_2026-11-25.md). Docs only.


## 2026-11-26 — Berserker advancement quest USER APPROVED: Bristlebreaker + persistent abandon credit

TierII eligible Greatblade (ordinary TierIII advancement level60+) takes challenge from **Old Berserker mentor**. In the existing six-guardian **Crimson Crucible** gauntlet, kill **Verdigris Globule cell1**, **Moonfang Alpha cell2**, **Bristlebreaker cell3** in sequence. **Only Bristlebreaker kill is the Berserker quest objective**; no cell4–6 clear requirement, no special one-boss arena, and no early regular treasure. Player uses normal **Abandon/Leave**, the transient dungeon resets, but the **accepted quest's boss3 kill credit persists** across exit and save/load. Return to mentor to unlock TierIII Berserker Lv1 as a NEW manifestation without overwriting Greatblade or granting free starter ranks. User explicitly delegates old mentor's exact camp placement to Codex, with **Quillboar region north of Trimery** (user called bristleboar area) a suggested location in addition to Crimson Mire surroundings. All specifics must follow existing world geography and normal advancement/quest UI. Full concept and implementation acceptance criteria: [Berserker design](classes/BERSERKER_TIER3_DESIGN.md). No Unity implementation in this approval round.


## 2026-11-26 — Berserker passive damage and active cooldown candidates (pending approval)

**New designer proposals only, NOT user-approved:** Anger Management ranks0..25 grants suggested +0.5% eligible 2H axe/sword Physical damage per rank (max12.5%) and a missing-HP outgoing damage bonus `0.40×(rank/25)×clamp((1−HP/MaxHP)/0.99,0,1)`, reaching the already approved +40% missing-HP bonus at rank25/1% HP. Candidate multiplicative combination `(1+0.005rank)×(1+missingHPBonus)`; at max both +57.5% total, versus additive +52.5% alternative. Low-HP applies to all damaging skills and basics once; equipment no double-add. **Designer suggested active cooldowns** Wide Arc5s, Spin Dash10s, Rage Breaker8s, already **APPROVED Coffee Shield15s**; Anger Management passive, no cooldown. Proposed cooldown starts on accepted cast. Existing skill unlocks/caps/150-rank gate audit remain approved. Obtain user decision on damage stacking, curves and cooldowns before marking final. [Full Berserker proposal](classes/BERSERKER_TIER3_DESIGN.md). No Unity edits.


## 2026-11-26 — Berserker passive damage and cooldown timing decisions

**APPROVED** Anger Management `r=0..25`: eligible 2H axe OR sword Physical damage bonus `W=0.005r`; missing-HP outgoing damage bonus to all Berserker basic/offensive skill attacks `B=0.40*(r/25)*clamp((1-currentHP/maxHP)/0.99,0,1)`; multiply modifiers `(1+W)*(1+B)` once, max +57.5% combined at rank25/1%HP/qualifying weapon. This remains one 25-rank passive and grants NO class crit chance.

**APPROVED cooldown timing:** cooldown begins after skill cast/attack resolves/finishes, not at cast start. Coffee Shield begins cooldown after activation/cast, not after its 5s protective effect expires. **New cooldown seconds are OPEN** because user requests longer cooldowns rather than approving earlier Wide Arc5s, Spin Dash10s, Rage Breaker8s. Coffee Shield15s was previously approved and is still baseline until changed. **Designer-only candidates:** Wide Arc7s, Spin Dash14s, Rage Breaker12s, Coffee Shield20s; require explicit signoff. No Unity implementation. [Full rules](classes/BERSERKER_TIER3_DESIGN.md).


## 2026-11-26 — Berserker first-playtest cooldowns FINALIZED (all four actives)

**Latest USER APPROVED:** Wide Arc **7s**, Spin Dash **15s**, Rage Breaker **12s**, Coffee Shield **25s**. All start cooldown **after the corresponding cast/attack finishes or resolves**; Coffee Shield cooldown starts after activation is completed, **not after the 5s shield expires**. Anger Management has no activation cooldown. This explicitly supersedes earlier tentative Wide Arc5, SpinDash10/14, RageBreaker8, CoffeeShield15/20 numbers. Existing Rage actions: Wide Arc10 cost, SpinDash+2/unique enemy max20, RageBreaker30 cost plus4s post-hit haste, CoffeeShield25 cost and 5s self shield. Anger Management +0.5% two-handed physical damage/rank and linear low-HP +40% at rank25 / 1% HP combine multiplicatively for max +57.5%, both USER APPROVED in prior round. User reports "Codex is happy with everything" and wishes to move on; this is not proof of Unity implementation. [Final Berserker design](classes/BERSERKER_TIER3_DESIGN.md). No Unity files changed.


## 2026-11-26 — Mage selected as next Tier III first-coverage design

User recommits to at least one TierIII for every current TierII class before designing second branches, and picks **Mage** next. An opening design file reviews actual TierII Mage spell roster, six elements, two passives, class stats, advancement and old-vs-new universal Mana constraints. **Wizard, Sorcerer and Arcanist are designer-only potential first evolutions**, not decided names or skill kits. Begin with identity/role/inherited abilities/element specialization/resource design questions; retain normal TierII level60 to a distinct TierIII level1 manifestation model. [Mage TierIII interview](classes/MAGE_TIER3_DESIGN.md). No Unity content changed.


## 2026-11-26 — Mage Tier III first evolution: Fire Mage (user direction approved)

User chooses **Fire Mage** as the first Mage TierIII path (Mage level60+ to separate level1 TierIII manifestation under ordinary class progression). Primary role BURST magical DPS, **mostly new abilities** with fewer elements primarily FIRE, and **global Mana plus Overheat** as a secondary mechanic driven by casting multiple Fire spells in quick succession. "Empower the next spell" is only one candidate effect and is not specifically approved yet; similarly 0–100 scale, gain amount, decay window, status/burst/penalty and exact skill list remain TBD. Preserve universal Mana capacity `ClassBaseMana+5×INT`, regen `4+0.1×SPR`; no per-class magical coefficient override. Unchosen Wizard/Sorcerer/Arcanist can remain future options only. [Mage TierIII design](classes/MAGE_TIER3_DESIGN.md). Documentation round; no Unity implementation.


## 2026-11-26 — Fire Mage Tier III Round2 Overheat and proposed ability roster

User confirms **Overheat 0–100 secondary meter**, every successful Fire spell cast builds it, idle Heat gradually decays after a short pause, and reaching 100 empowers the **NEXT eligible spell with distinct spell-specific effect**. Inherit Ember Bolt as evolved Fire spell and transform Math Barrier into Fire shield, alongside user-requested **Fire DoT, Living Bomb-like delayed explosion, Fire LAZOR and passive/aura**. Detailed first-pass candidates Fireball, Flame Ward, Ignite, Living Bomb, Incineration Beam, Heart of the Inferno (5 actives +1 passive) are **DESIGNER PROPOSALS, not yet approved skill names/number/kit**. Explicit unresolved: heat gain per cast, heat spending order and whether empowered casts also grant heat, decay values, channel/DoT cast-event rules, aura benefit and empowered effects. Respect all Mage TierII lineage, separate TierIII manifestation, shared global Mana formula. [Mage TierIII interview](classes/MAGE_TIER3_DESIGN.md). No Unity implementation.


## 2026-11-26 — Fire Mage TierIII Round3 finalized ability roles and Heat consumption

USER approves roster structure of **five Fire active spells + one Heat-management passive + one Fire-resistance aura** (seven concepts; exact caps/gates not yet assigned). Actives are evolving Ember Bolt Fire projectile, evolving Math Barrier Fire shield, Fire DoT, Living Bomb delayed AoE, and long-windup **single-hit Fire LAZOR beam** (not channel ticks). **Heat cast reference=25**, but the user separately requires purchased passive ranks to control Heat gain, so treat 25 as the selected baseline pending rank formula; **at 100 next eligible spell is empowered, consumes all100, and generates NO replacement Heat itself**. Distinct empowered effects for different spells remain approved, though exact effects TBD. **Aura increases allies' Fire resistance and decreases nearby enemies' Fire resistance**, separate from Overheat passive. Earlier proposed 5+1 and single-purpose enemy-only aura are superseded. Global Mana structure unchanged. [Mage TierIII Round3](classes/MAGE_TIER3_DESIGN.md). Documentation only.


## 2026-11-26 — Fire Mage TierIII Round4 user correction: deferred Fire LAZOR to TierIV Pyromancer

Latest authoritative Fire Mage decisions: **passive Heat gain starts below25, grows to 25 successful Fire cast Heat at passive maximum** (exact formula/ranks TBD). After **10s inactivity**, heat declines **10 per second**, superseding earlier 5-second suggestion. Aura each rank: **allies +0.5 percentage points Fire resistance, enemies -0.5 percentage points Fire resistance**, radius/cap TBD. **Flame Ward retains Mana damage absorption/redirection** as evolved Math Barrier; its empowered form **ADDS a Fire-element shield** rather than exploding. User chooses **Fire LAZOR to be reserved for future TierIV Pyromancer**, NOT a TierIII active. Thus the TierIII Fire Mage currently has **four active themes + separate Heat passive + resistance aura**; whether a fifth TierIII active is needed is OPEN for next round. TierIV Pyromancer is a future named design direction, not gameplay implemented or detailed. Preserve established Mage global Mana, Overheat100 threshold/spend/no-refill, distinct empowerment effects and first-coverage priority. [Details](classes/MAGE_TIER3_DESIGN.md). Design docs only.


## 2026-11-26 — Fire Mage TierIII Round5 six-skill roster and passive/aura behavior confirmed

**USER APPROVED:** Four active spells (Fireball, Flame Ward, Ignite, Living Bomb) + Heat-generation passive + resistance aura, six total concepts; no TierIII substitute for Fire LAZOR (future TierIV Pyromancer). **Heat per eligible cast** starts **10 at passive rank0** and reaches **25 at max passive rank**; exact interpolation and rank cap TBD. **Flame Ward retains percentage-based Mana redirection inherited from Math Barrier**, adjusted for TierIII, and adds a separate elemental Fire shield when empowered by full100 Heat. **Aura includes caster**, same-name overlapping aura effects **take strongest copy only**, retains +0.5pp allied/-0.5pp hostile Fire resistance per rank. **Empowered Ignite = much stronger SINGLE-TARGET Burn**, no spreading or burst-explosion alternative approved. Earlier 10s inactivity then -10 Heat/sec, 100 Heat empower/spend/no-refill and global Mana remain approved. **Designer-only first proposed 150-rank cap split** Fireball35/FlameWard20/Ignite30/LivingBomb30/HeatPassive20/ResistAura15 (sum150) with `Heat=10+0.75*passiveRank`, still waiting for user approval. [Current Fire Mage TierIII design](classes/MAGE_TIER3_DESIGN.md). No Unity implementation.


## 2026-11-26 — Fire Mage TierIII Round6 FINAL rank caps and empowerment details (user approved)

User approves 150 purchasable points across Fireball **35**, Flame Ward **25**, Ignite **30**, Living Bomb **30**, Overheat Mastery **15**, Infernal Presence **15**. This includes explicit transfer of five planned ranks from Overheat Mastery to Flame Ward; prior 35/20/30/30/20/15 split is SUPERSEDED. One point/class level1–150, no free starting ranks; exact rank unlock gates OPEN. Overheat passive rank r0..15 generates **10+r Heat per successful ordinary Fire cast**, capped at25; empowered next cast at100 consumes all100 and adds no new Heat. Infernal Presence is always-on **15m radius** and +0.5pp allied/-0.5pp enemy Fire resistance per rank, strongest identical aura only, includes caster. Empowered Ignite = **3x normal total single-target Fire DoT damage over unchanged duration**. Empowered Living Bomb = stronger primary explosion AND spreads bombs to new targets inside its explosion radius, **maximum one propagation hop**, with propagated bomb fuse/damage still undecided. Full Fire Mage details in [design spec](classes/MAGE_TIER3_DESIGN.md). Unity implementation unchanged.


## 2026-11-26 — Fire Mage TierIII Round7 first skill unlocks and spell mechanics user approved

**USER APPROVED first unlock class levels**: Fireball1, Overheat Mastery1, Ignite3, Flame Ward5, Living Bomb8, Infernal Presence10; 150 earned class points levels1–150, starting all skill ranks at0. **Max ranks unchanged:** Fireball35/FlameWard25/Ignite30/LivingBomb30/OverheatMastery15/InfernalPresence15, exact per-rank level gates still OPEN. Empowered Fireball leaves Fire damaging ground at struck target's position (not proposed immediate splash). Empowered Living Bomb spreads one-generation NORMAL full-damage NORMAL-fuse bombs to nearby valid enemies; further spread prohibited. Flame Ward rank-scales percentage-based Mana redirection and adds INT-scaling Fire shield when empowered. Ignite ticks 1/sec for8s with refresh, not stacking; empowered =3x single-target normal total DoT over same duration. [Details](classes/MAGE_TIER3_DESIGN.md). No Unity gameplay changed.


## 2026-11-26 — Fire Mage TierIII Round8 approved AoE timing, fuse, ward endpoints, DoT cadence

USER APPROVED: empowered Fireball leaves **4m radius fixed-location burning ground for6 seconds with one damage tick/sec**, under hit target. Living Bomb fuse **3 seconds**, blast **3m radius**, propagated versions inherit normal damage/fuse, no further spreading. Flame Ward percentage-based Mana redirect at purchased rank1 **30%**, rank25 **65%**, with intermediate curve yet to approve (linear `30%+35%*(r-1)/24` is designer candidate); empowered additional Fire-element shield strength scales with **both INT and Flame Ward rank**, formula TBD. Ignite **eight damage ticks across8 seconds, at t=1..8sec**, refresh rather than stack, empowered=3x normal total damage same duration. 150 caps, rank1 unlocks, Heat100/10+r, 15m aura unchanged. [Fire Mage TierIII spec](classes/MAGE_TIER3_DESIGN.md). No Unity implementation.


## 2026-11-26 — Fire Mage TierIII Round9 Ward curve, empowered Fire shield and future evolution idea bank

USER APPROVED Flame Ward `Redirect(r)=0.30+0.35*(r-1)/24` (rank1..25), improving Mana-per-prevented-HP efficiency modeled after parent TierII Math Barrier but exact 25-rank efficiency formula still OPEN; don't extrapolate parent `2-0.1*r` beyond five ranks. Empowered additional Fire shield `50+(0.5+0.1*r)*INT` absorb capacity, **duration10s**. Empowered Fireball burning ground 4m radius for6s ticks exactly at t1..6s. Future-only evolution concepts to preserve: TierIV **Pyromancer** merges aura+heat passive, adds FIRE LAZOR and possibly Meteor; later TierIII **Frost Mage**→TierIV **Cryomancer**; HIDDEN TierIV **Hydromancer** proposed after obtaining BOTH **Fire Mage Lv120 and Frost Mage Lv120** manifestations, precise unlock workflow/character-scope TBD. These future ideas are not approval to create new skills/classes now; first cover each of the eight TierII parents with at least one TierIII path. [Mage TierIII design](classes/MAGE_TIER3_DESIGN.md). Documentation only; Unity unchanged.


## 2026-11-26 — Fire Mage TierIII Round10 Ward efficiency and cooldown, spell damage scaling

USER wants Flame Ward rank25 Mana-cost efficiency ~**1.3 Mana/HP prevented**, an improvement over parent r5=1.5; numeric endpoint phrased tentatively, exact linear proposal rank1=1.9→rank25=1.3 awaits signoff. **Flame Ward main Mana-redirect duration reaches10s at max rank25**, lower-rank curve open; **Flame Ward cooldown20s**. Extra empowered Fire shield separately stays10s lifetime. **Empowered Living Bomb initial detonation deals2x normal damage** and then creates full-normal-damage normal3s-fuse secondaries with no more propagation. **All Fire Mage offensive flat damage AND INT coefficients grow with purchased ability rank**; specific values pending. User mentions **7s Fireball "at max rank"** following "short cast time" question, so treat cooldown interpretation as proposed pending user confirmation, not approved. [Full Fire Mage notes](classes/MAGE_TIER3_DESIGN.md). Docs only, no Unity gameplay changed.


## 2026-11-26 — Fire Mage TierIII Round11 exact Ward curves and Fireball timing (USER APPROVED)

USER clarifies Fireball **cast time0.2s** and **cooldown7s at maximum rank35** (rank1..34 CD curve OPEN). Flame Ward purchased rank1..25 **Mana per prevented HP** `1.9-0.6*(r-1)/24` (to1.3 rank25); **main ward duration** `4+6*(r-1)/24` sec (4s→10s); **ability CD20s**; redirect fraction remains `0.30+0.35*(r-1)/24`. Extra empowered Fire shield separately lasts10s with capacity `50+(0.5+0.1*r)*INT`. Pyromancer TierIV may evolve this timed ability to a Mana-absorption TOGGLE, a future evolution note NOT a TierIII mechanic or implementation. Exact flat+INT damage coefficients, mana costs and per-rank purchase gates still pending. [Design details](classes/MAGE_TIER3_DESIGN.md). No Unity implementation.


## 2026-11-26 — Fire Mage TierIII Round12 exact Fireball cooldown and other cast timings

User approves Fireball cooldown `14-7*(r-1)/34` seconds for ranks1..35 (14s→7s) with existing0.2-second cast time. Ignite **instant cast,10s cooldown**; Living Bomb **0.2-second cast,15s cooldown**, distinct from normal3-second bomb fuse. Flame Ward cooldown20s previously accepted, its cast time still open. Cooldowns apply to active purchased ranks1+; no rank0 free casts. All damaging flat and INT coefficients must scale with skill rank, exact formulas/Mana costs TBD. [Fire Mage spec](classes/MAGE_TIER3_DESIGN.md). Documentation-only.


## 2026-11-26 — Fire Mage TierIII Round13 damage relationships, instant Flame Ward and rank-CD idea

**USER CONFIRMS:** Fireball balanced flat+INT damage; empowered Fireball ground each of six ticks ≈0.25x reference normal Fireball direct hit; normal Ignite 8-second DoT total≈2x Fireball direct hit, empowered3x normal Ignite against one target; normal Living Bomb explosion per enemy≈1.5x Fireball direct hit, empowered initial original2x its own normal explosion plus one-hop normal bombs. All separate spell skills increase their **own flat and INT damage coefficients with purchased rank**; the comparisons are balance targets, not a cross-skill numerical formula at mismatched ranks. **Flame Ward instant cast** approved. User proposes **other active cooldowns should fall with rank**; needs exact endpoint approval for Ignite (existing10s), Living Bomb (existing15s), Ward (existing20s). Designer candidates rank1→cap are Ignite14→10s, Living Bomb22→15s, Ward28→20s, NOT yet approved. [Fire Mage spec](classes/MAGE_TIER3_DESIGN.md). Documentation only.


## 2026-11-26 — Fire Mage TierIII Round14 four linear cooldown formulas and Mana/Heat/shield order

USER APPROVES all active cooldowns rank-dependent: Fireball `14-7*(r-1)/34` for r1..35; Ignite `14-4*(r-1)/29` for r1..30; Living Bomb `22-7*(r-1)/29` for r1..30; Flame Ward `28-8*(r-1)/24` for r1..25 (all times seconds). User approves **all four spell Mana costs rising with purchased rank**, exact amounts open. Flame Ward charges **a small upfront cast Mana cost and then additional Mana per HP prevented**. **All FOUR Fire active casts, including defensive Flame Ward, award normal Heat and can consume full100 Heat for empowerment; empowered casts grant zero replacement Heat**. For empowered Ward, **extra INT-and-rank Fire-element shield applies BEFORE the normal Mana redirection to remaining damage**, preventing double counting. Exact eligible shield damage types and numerical Mana costs not set. Previously accepted six skill caps and shared global Mana preserved. [Spec](classes/MAGE_TIER3_DESIGN.md). No Unity code changes.


## 2026-11-26 — Fire Mage TierIII Round15 global Fire shield and rank gated skill system

USER APPROVES Flame Ward **self-only**, **normal Mana cost even when empowered** (empowered costs normal Mana +100 Heat, not zero/150% Mana), and true **Fire-elemental extra shield** using existing `docs/STATS_AND_DAMAGE.md` global eligible damage rules, **0.8 shield HP consumed per Fire damage prevented and1.2 per other ordinary eligible damage prevented**, standard shield bypass exceptions. It is not Fire-only and not generic1:1. Apply extra shield FIRST, Mana redirection to remainder SECOND, HP THIRD. **All purchased skill ranks individually require a minimum class level**, all150 accessible by Fire Mage class level150; precise gate schedule not finalized. User rejects earlier small spell Mana cost candidates as too low and asks for HIGHER cost numbers, designer-only proposals: Fireball `18+3r`, Ignite `24+3r`, Living Bomb `34+4r`, Flame Ward `10+2r`, **NOT user approved yet**. Ward has small upfront cost plus per-prevented-HP Mana expenditure. Global Mana max/regeneration unchanged. [Fire Mage design](classes/MAGE_TIER3_DESIGN.md). Docs only, Unity untouched.


## 2026-11-26 — Fire Mage TierIII Round16 approved Fireball raw damage and gate strategy; Mana deferred pending class stats

USER says Mana costs need revisiting AFTER Fire Mage attributes/base Mana are agreed, mentioning possible ~5×purchased rank but NOT approving any numeric spell cost. USER accepts Fireball first-test raw Magical Fire `30+8r+(0.3+0.04r)*INT` (r1..35, rank-scaled flat and INT coefficient); its empowered burning ground remains additional. USER approves evenly spread per-rank minimum class level gates across1..150 with approved first unlocks, but concrete mathematically audited 150-entry gate array in [Mage design](classes/MAGE_TIER3_DESIGN.md) is newly proposed and awaits exact signoff; no point unspendable at any class level1..150, all skills max by150. Next decide Fire Mage new manifestation Lv1 stats/growth and ClassBaseMana, using global max `ClassBaseMana+5*INT` and regen `4+0.1*SPR`, before rank-rising Mana costs. Actual parent TierII Mage Lv1 base VIT11/STR5/AGI8/INT18/SPR9 and growth1.5/.5/.75/3/1.5; designer-only TierIII candidate Lv1 19/7/12/33/13, growth2/.75/1.2/5.5/1.5 and base Mana160. No Unity gameplay changes.


## 2026-11-26 — Fire Mage TierIII Round17 INT/AGI growth correction and high Mana spend direction

USER expressly changes proposed Fire Mage TierIII growth from AGI+1.2/INT+5.5 to **AGI+1.0 and INT+6.0 per class level** (remove0.2 AGI, add0.2 INT and another+0.3 INT). Other working Lv1 VIT19 STR7 AGI12 INT33 SPR13, growth VIT+2 STR+.75 SPR+1.5 and proposed ClassBaseMana160 carried forward as first-test baseline, no extra changes requested; total growth **11.25**. Shared maxMana `160+5INT` / regen `4+.1SPR`: Lv1 325 max Mana and5.3/s; Lv150 no-gear INT927, SPR236.5, **4795 Mana/27.65 regen/s**. User argues earlier Mana costs too cheap and muses **~10 Mana per purchased ability rank**, NOT approving exact costs. Designer-only candidates: Fireball`20+10r`, Ignite`30+10r`, LivingBomb`50+10r`, FlameWard`10+6r` upfront or`10+10r`, each subject to user approval and shared Mana affordability. Previously accepted Fireball raw damage and 150-rank progression approach stay; exact audited gate entries still awaiting explicit user acceptance. [Mage design](classes/MAGE_TIER3_DESIGN.md). Docs only, Unity unchanged.


## 2026-11-26 — Fire Mage TierIII Round18 stats, 150-rank gates and Ward Mana now USER APPROVED

**First-test exact Fire Mage new-manifestation Lv1 attributes:** VIT19, STR7, AGI12, INT33, SPR13; level growth +2/+0.75/+1/+6/+1.5, and **ClassBaseMana160**. Global max Mana=160+5INT and regen=4+.1SPR, so Lv1 no gear Mana325/5.3 per second, Lv150 no gear INT927/Spr236.5/Mana4795/regen27.65/s. **USER APPROVES the full exact 150-minimum-level gate table** in [Mage Round16](classes/MAGE_TIER3_DESIGN.md) for six skill rank caps35/25/30/30/15/15; all eligible by class Lv150, one earned point/level1..150, no free rank1. **Flame Ward upfront Mana cast cost approved `10+6r`** (r1=16, r25=160), plus redirected Mana spent by HP absorbed separately; empowering spends normal Mana plus100 Heat, extra Fire shield follows existing global rules. Other THREE offensive Mana costs must grow sharply, user suggests coefficient **~15 Mana per rank** but base/formulas NOT approved. Next designer-only candidate Fireball`20+15r`, Ignite`30+15r`, Bomb`50+15r`; user signoff pending. Docs-only.


## 2026-11-26 — Fire Mage TierIII Round19 approved Mana costs and exact empowered Fireball ground scaling

USER approves **Fireball cast Mana `20+15r`**, **Ignite `30+15r`**, **Living Bomb `50+15r`**, plus previously approved Flame Ward `10+6r`. Max-rank cast costs Fireball545/Ignite480/Bomb500/Ward160; empowered costs its normal Mana PLUS100 Heat, Ward spends additional Mana when preventing HP damage. **Empowered Fireball six burning-ground ticks EACH do exactly25% of OWN skill-rank normal Fireball raw hit `30+8r+(0.3+0.04r)INT`**, i.e. tick `7.5+2r+(0.075+0.01r)INT`, independent Magical Fire packets every second at t1..6s in fixed4m patch. **Ignite 8 tick DoT and Living Bomb normal explosion must have independent rank-rising flat+INT coefficients**, not runtime Fireball rank-derived damage, calibrated approx 2x reference Fireball total for Ignite and1.5x reference Fireball per target for Bomb. Numeric independent damage coefficients awaiting user signoff; exact proposed equations documented in [Mage design](classes/MAGE_TIER3_DESIGN.md). Approved Fire Mage stats/BaseMana and exact rank gate schedule unchanged. No Unity gameplay changes.


## 2026-11-26 — Fire Mage TierIII Round20 independent tick coefficients and multi-source DoT/AoE stacking

APPROVED Ignite exact RAW Fire tick `9.5+68*(r-1)/29+[0.085+0.34*(r-1)/29]*INT` r1..30, normal 8 ticks over8sec, empowered3x per tick. Same-caster reapply restarts8s duration/tick timer **retaining strongest potency**, and **different casters independently apply their own Ignite to same enemy**, no cross-caster overwrite. Living Bomb exact per-target NORMAL raw AoE explosion `57+408*(r-1)/29+[0.51+2.04*(r-1)/29]*INT` r1..30; bomb DETONATIONS ARE 3m AoE, original empowered is2x normal raw per target and makes one hop of NORMAL 3sec-fuse secondary bombs, each secondary detonates in its own3m AoE and never propagates again. Multiple independent Living Bomb instances may stack on one target even same caster, each tracks fuse and AoE and is not replaced/ignored by new casts. Empowered Fireball fixed4m burning grounds last6s, six independent raw ground ticks each25% own Fireball raw hit, and **multiple patches all damage independently in overlaps**, including same caster. Mana/cooldowns/stat baseline and exact 150 gate table remain as approved. [Full spec](classes/MAGE_TIER3_DESIGN.md). No implementation edits.


## 2026-11-26 — Fire Mage TierIII Round21 snapshot, death explosion and secondary stacking rules

USER APPROVES Ignite **snapshot of purchased rank, INT and damage bonuses at application** for 8 ticks (empowered3×), same-caster recasts refresh8s duration and tick timer but **keep stronger snapshot potency**, and different caster Ignite DoTs coexist. Living Bomb **snapshots offensive rank/INT/bonuses at application**, normal/empowered original detonation has **3m AoE per eligible enemy**; if attached target dies before fuse3s, bomb **detonates immediately at death location** instead of cancelling/delaying. Multiple independent secondary bomb instances, even on SAME enemy, are ALL allowed; each does own normal3-second fuse and own full normal 3m AoE, without creating further bombs. Empowered originals still detonate2× normal and each can spawn one generation on valid other enemies. Fireball empowered ground snapshots **its purchased Fireball rank and caster INT when each patch appears**; six ticks use frozen raw `0.25*(30+8r+(0.3+0.04r)*INT)`, independent overlapping patches remain independently damaging. Target mitigation/resistance stays evaluated at event time. Damage bonus snapshot for ground beyond rank+INT not independently approved. No changes to previously approved Mana costs, stats, cooldowns or 150 gate table. [Mage design](classes/MAGE_TIER3_DESIGN.md). No Unity gameplay modifications.


## 2026-11-26 — Fire Mage TierIII Round22 final snapshot and noncrit policy, new mentor selection

USER confirms **all outgoing bonuses snapshot for empowered Fireball ground at creation** (not just rank/INT); target mitigation still current per tick and overlapping grounds independently apply damage. A target death while carrying multiple independent Living Bombs triggers **every instance's immediate full3m AoE detonation**, preserving each instance's snapshot and empowered-original2x/one-hop versus nonpropagating-normal-secondary distinction. **Ignite same-caster refresh picks strongest actual snapshot tick damage, independent of empowered vs normal**, refreshes8s clock and restarts tick timer; different casters coexist. Fire Mage abilities have **NO innate spell critical-hit capability**, but late-game spell-critical equipment may be added as a future system and could override eligibility explicitly if designed later; no such items or crit behavior currently approved. TierIII Fire Mage advancement must feature a **NEW FIRE-SPECIALIST NPC** instead of the original Mage mentor; no identity/location/quest design yet. Other approved Mana, formulas, levels and gate schedule unaffected. [Mage details](classes/MAGE_TIER3_DESIGN.md). Documentation-only; Unity untouched.


## 2026-11-26 — Fire Mage TierIII Round23 World2 volcano mentor and five-minute movement trial

**USER APPROVES new pyromancer FIRE-SPECIALIST mentor in World2 volcano**, explicitly superseding initially selected Trimery forge area. Existing World2 **Ember Crown** volcanic cone in southeast Ashfall Reach is the grounded venue candidate, exact NPC position TBD. Advancement for Mage TierII class Lv60+ requires short **~5-minute SOLO instanced lava/fire hazard-dodging escape maze**, classic WC3 *Run Kitty Run* / *Pyramid Escape* movement style: navigate marked safe path from A to B, avoid stepping in lava and avoid timed fire obstacles; not a boss fight/DPS check or collection quest. Checkpoint/fail/death/ability restriction/exit handoff and NPC name still need design/approval, existing World2 map and World2 story access should be respected. Previously approved Fire Mage stat,150 skill gates, abilities/Heat/costs remain unchanged. [Detailed design](classes/MAGE_TIER3_DESIGN.md). Docs only.


## 2026-11-26 — Fire Mage TierIII Round24 approved mentor basecamp and single continuous heavy-hazard advancement maze

USER locks FIRE-specialist pyromancer mentor at **small safe training camp at base of Ember Crown volcano, World2 southeast Ashfall Reach**. TierII Mage Lv60+ enters **SOLO single continuous Point A-to-B ~5-minute fire/lava avoidance course**; not three chapters, randomized rooms or a boss fight. Fire and lava cause **heavy damage**, BUT contact itself does **not** automatically reset/teleport a surviving player: they can survive and keep moving. **No active abilities/spells/teleport/dashes/movement skills usable inside**; standard walk controls available. Reaching exit B records trial success and the player must **return to the mentor to TURN IN and become TierIII Fire Mage**, not auto-advance at exit. Exact hazard damage numbers, death/retry policy, maze path and NPC identity remain open. Previous stats, all six rank caps and150 gate schedule, Mana and combat remain approved unchanged. [Mage spec](classes/MAGE_TIER3_DESIGN.md). Documentation only.


## 2026-11-26 — Fire Mage TierIII Round25 approved volcano escape hazard behavior

TierIII Mage Lv60+ advancement trial at the **Ember Crown World2 volcano basecamp** remains solo continuous A-to-B movement dodge course, **SHORT, not fixed five minutes**. USER APPROVES **FIRE BURST nominal damage =25% of player's maximum HP per hit**, lava damages **once each second while player remains exposed** (exact magnitude OPEN), **HP death respawns at maze entrance for immediate retry**, while hazard hits on living players never themselves teleport/reset them. **Healing potions and regular consumables ALLOWED** during course, despite **activated class abilities/teleports/movement skills being DISABLED**. All fire patterns use **PREDICTABLE REPEATING telegraphed warnings**. Reaching exit only records success; return to pyromancer mentor at safe camp for actual class advancement. Mana, skills, stats, and exact 150 rank gates unchanged. [Fire Mage class spec](classes/MAGE_TIER3_DESIGN.md). Docs only.


## 2026-11-26 — Fire Mage TierIII Round26 approved lava and fire hazard cadence, retries and Codex mentor name

Solo short Ember Crown volcano escape trial: **lava deals 15% of player's MAX HP immediately on entry and once each second of continued exposure**; FIRE BURSTS deal **25% MAX HP per hit and can hit again on a FIXED cooldown while player stays in active flames**, exact cooldown duration TBD (not automatically1second). Fire patterns cyclic with visible warning telegraphs. At actual maze HP death, respawn immediately at maze entrance with **FULL HP and FULL MANA**, but potion/item consumables already spent remain spent, no refunds. Healing potions and ordinary consumables remain usable while active class abilities, dashes and teleport disabled. New controlled-fire pyromancer mentor at safe camp BASE of existing Ember Crown volcano, World2; **NPC NAME DELEGATED TO CODEX to choose later**, not any proposed name currently canonical. Course short rather than exactly five minutes, continuous A-to-B with mentor turn-in for Lv60 Mage to TierIII Fire Mage. All approved stats/skill costs/ranks unchanged. [Full Mage design](classes/MAGE_TIER3_DESIGN.md). Docs only.


## 2026-11-26 — Fire Mage TierIII Round27 fixed re-hit Fire damage, immediate lava re-entry, and standard World2 access

Fire Mage solo short A-to-B escape quest now locks **active FIRE bursts at25% player MAX HP nominal RAW per hit, repeated each1.5sec while exposed**; LAVA hits **15% player MAX HP nominal RAW immediately on entry and each1sec while exposed**, with **new immediate hit and timer reset on every genuine re-entry** (no grace). BOTH damage types are standard **Fire-element hazard packets using existing resistance/mitigation/shield rules**, not fixed postmitigation HP percentage. A false exit/entry between adjacent lava tiles that still constitute continuous exposure must not cause duplicate entry damage. USER approves retaining NORMAL World2 story-unlock requirement before accessing the NPC camp at base of Ember Crown volcano, no early Mage-only shortcut. Player retains normal movement and consumables, active spells/movement skills locked, death retries entrance fullHP/Mana with consumed items spent, final mentor turn-in; no arbitrary five-minute timer. [Full Fire Mage specification](classes/MAGE_TIER3_DESIGN.md). Documentation only.


## 2026-11-26 — Thief TierIII ASSASSIN Round1 selected first-coverage evolution

USER CONFIRMS first TierIII **Thief→Assassin**, a **PURE MELEE DPS** using **DUAL ONE-HANDED DAGGERS**, **MANA ONLY** (global MaxMana=ClassBaseMana+5INT; regen/s=4+0.1SPR), with a **MIX OF EVOLVED Thief TierII skills AND NEW REPLACEMENTS**. TierII Thief class Lv60+ → separate Assassin TierIII class Lv1 under global evolution model. Which parent skills, positional/stealth execution mechanics, crits, attack-speed tradeoffs, starting stats, costs, total rank budget and exact gate table are ALL PENDING. Earlier stealth/positioning/slow-deliberate-kill Assassin concept is an idea-bank guide rather than a finished approved roster; Duelist/Burglar remain later possible siblings. Full [Assassin TierIII design](classes/ASSASSIN_TIER3_DESIGN.md). No Unity edits.


## 2026-11-26 — Assassin TierIII Round2 stealth burst and forgiving 270-degree side/rear positioning

**APPROVED:** Thief→Assassin is a **slower BASIC and SKILL-attacking**, pure melee DPS, strongest damage immediately after STEALTH opening and weaker sustained pressure. **Stealth evolves** with positional damage mechanics. **Envenom remains its own ability; Backstab is NEW and separate**. Positional bonuses apply to **all SIDE+REAR attacker positions within 270° of the enemy**, with only a **90° FRONT cone relative to enemy facing** excluded; side and rear qualify equally. Numeric positional multipliers and Backstab trigger details pending; no automatic strict behind-only restriction. Dual one-handed daggers, Mana only/global universal Mana formulas, independent TierIII classLv1 after ThiefLv60+ retained. No Unity work. [Detailed design](classes/ASSASSIN_TIER3_DESIGN.md).


## 2026-11-26 — Assassin TierIII Round3 approved dual-hit basic, retained Crit and two new precision replacements

Assassin Backstab works from **any angle**, deals **much stronger damage in qualifying target-relative SIDE/REAR270°**, only target-front90° excluded. Assassin stealth EVOLVES to increase MOVEMENT SPEED when concealed. TierIII Assassin's dual daggers now deliver **TWO STRIKES AS ONE BASIC ATTACK SEQUENCE with LONGER interval**, specifically superseding parent TierII Thief's single-hit alternating dagger pattern for this subclass; hit/crit/proc/opening bonus distribution across two strikes TBD. Retain **Guaranteed Basic Crit ACTIVE + Crit/AGI PASSIVE separately**; REMOVE Fan of Knives and Smoke Bomb from Assassin, replacing them with **TWO as-yet-undefined precision skills**. Envenom and distinct new Backstab remain; total seven skill concepts currently, no approved final skill cap/rank/curve, no invented named replacement skills. Physical dagger pure melee DPS and Mana-only/global shared Mana maintained. [Assassin full design](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round4 confirmed both-strike Stealth bonus, impact crit checks, Execute and armor pen passive

Assassin's first BASIC attack sequence that breaks Stealth gets opener multiplier on **BOTH individual dagger strikes** of THAT sequence only. **Guaranteed Crit active is checked at EACH strike's IMPACT TIME**, not the start of two-strike cycle; buff expiry between strikes changes whether second hit is guaranteed. User corrects stealth move speed to **rank1 -10 and +1 per additional purchased rank**, symbolic modifier `r-11` (speed UNIT unspecified and potential negative starting slow flagged for clarification, not silently converted to +10%). Adds **Execute** as heavy low-target-HP finisher active. **NO second replacement active**; specifically reject proposed Expose Weakness and instead add distinct **% ARMOR PENETRATION PASSIVE** (coefficient/applicability still open). Confirmed roster concepts: 5 actives Stealth, Envenom, Backstab, Guaranteed Crit, Execute; 2 passives Crit/AGI and %ArmorPen; total7 conceptual skills, caps/gates TBD. Dual one-hand daggers, pure melee DPS, Mana-only global framework and target side/rear270° positional bonus unchanged. [Assassin detailed spec](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round5 confirms 150 purchasable ranks and burst interactions

USER LOCKS **150 TOTAL SKILL RANKS**, one rank point earned each ASSASSIN TierIII class level1..150. Stealth movement speed **(rank-11)%**, starting **-10% at rank1**, +1 percentage point/additional purchased rank; rank cap TBD. Backstab stealth opener bonus **STACKS with side/rear270-degree positional bonus**, while skill remains castable from any angle. Execute **ALWAYS usable**, damage increases continuously with target MISSING HP, no30%-only gate. Percentage Armor Penetration PASSIVE applies to both normal dagger BASIC ATTACKS and PHYSICAL ASSASSIN SKILLS. User EXPLORES deleting separate Guaranteed Crit active or MERGING IT with Crit/AGI PASSIVE into ONE HYBRID passively increasing crit and actively granting guaranteed crit by time or number of hits. **No merger/removal APPROVED YET**, so current two separate crit skills remain canonical pending Round6; six vs seven skill count unresolved, rank caps/gates must wait. [Full Assassin design](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round6 crit active/passive merged, six skill roster

USER APPROVES **ONE hybrid purchased skill** merging old separate basic Guaranteed Crit ACTIVE + Crit/AGI PASSIVE. Passive provides **AGI and CRIT CHANCE** increases with rank; activation produces **GUARANTEED CRIT BASIC-HIT CHARGES that SCALE WITH RANK, START AT 2 CHARGES**, and have **NO SHORT TIME WINDOW**. Charges are only usable on the TWO-STRIKE basic dagger attack hits, never physical skill Backstab, Execute, Envenom; per-impact processing required, exact charge expenditure/miss policy pending. Canonical Assassin **SIX rankable skill concepts** now 4 actives Stealth, Envenom, Backstab, Execute; one HYBRID Crit Mastery (working name) and one PASSIVE % Armor Penetration. No separate Guaranteed Crit purchase/slot. Previously approved **150 total purchased skill ranks and one point every Assassin classLv1..150** now will be distributed across six skills; exact caps/gates TBD. No changes to dual daggers, strong stealth burst, positional270deg, signed stealth speed `(rank-11)%`, global Mana or Execute scaling. [Assassin specification](classes/ASSASSIN_TIER3_DESIGN.md). Design only.


## 2026-11-27 — Assassin TierIII Round7 approved 150-rank exact caps and basic crit charge mechanics

ASSASSIN USER APPROVED FINAL SKILL CAPS, including overriding default form choice by moving5 from Execute to Envenom: **Stealth25, Envenom30, Backstab35, Execute25, hybrid CriticalMastery20, ArmorPenetration15**, TOTAL150 rank purchases, one point/Assassin class Lv1..150. Exact rank-level eligibility gates pending a separate audited table. Hybrid crit active charges `2+floor((purchasedRank-1)/5)`: 2 at rank1–5,3 at6–10,4 at11–15,5 at16–20, **NO short timer**. A charge is consumed ONLY on a **successful BASIC dagger hit**, NOT on a miss; each strike in one two-hit basic sequence resolves separately, full remaining charges reset/replaced on activation (never stack). Both hybrid PASSIVE crit chance and ACTIVATE guaranteed crit are **BASIC ONLY**, permanent passive AGI bonus retained. ArmorPen passive still applies basics AND physical skills. Stealth capped25 => signed movement `(StealthRank-11)%` yields+14% at max. Other class identity unchanged. [Detailed Assassin Round7](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round8 approved passive scaling, 25s crit cooldown, shield-charge handling and max6 charges

Assassin Critical Mastery hybrid cap20 passive now grants **+3AGI/rank** and **+1 percentage point BASIC CRIT CHANCE/rank**, max+60 AGI/+20pp. Hybrid activation **25-second cooldown**, GUARANTEED BASIC HIT critical charges `2+floor((rank-1)/5)+(rank==20?1:0)`; user additionally approved **BONUS SIXTH CHARGE ONLY at final RANK20**, breakpoint table1–5:2,6–10:3,11–15:4,16–19:5,20:6. A CONNECTED BASIC dagger strike spends one charge even when shield fully absorbs damage; misses never spend charges. Recast replaces remaining charges with full amount. Armor Penetration passive grants **+1% target Physical Defense bypass per rank**, max15% cap15, applying to basics and physical Assassin skills. Six skills/150 rank total unchanged, exact class-level rank gates, hybrid Mana cost and other formulas pending. [Full Assassin design](classes/ASSASSIN_TIER3_DESIGN.md). Docs-only.


## 2026-11-27 — Assassin TierIII Round9 charge persistence and Critical Mastery cooldown/cost timing

ASSASSIN Critical Mastery hybrid (cap20) ACTIVATE mana cost **INCREASES WITH SKILL RANK** (exact formula/amount TBD, no fixed cost). Its **25 SECOND COOLDOWN starts IMMEDIATELY ON ACTIVATION**, not on charge exhaustion. Unspent guaranteed-basic-crit charges **CLEAR ON DEATH, LOGOUT AND CLASS/MANIFESTATION SWAP**, but do not silently expire during continuous eligible play; reactivation after cooldown replaces remaining charges with full allotment. User-approved prior system: charges rank1–5=2 /6–10=3 /11–15=4 /16–19=5 /rank20=6, only successful basic hit spends charge even if shield prevents HP damage, misses consume0. Permanent passive AGI and basic crit by purchased ranks unaffected. Six-skill roster and exact150 rank-point caps unchanged, exact class-level rank gates TBD. [Assassin design](classes/ASSASSIN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — Assassin TierIII Round10 approves staggered unlocks and equal two-hit basic damage, defers Mana costs

Assassin has USER-APPROVED **STAGGERED first-unlock class levels** (specific Lv per skill/gate table pending user approval) and **EQUAL50/50 RAW SEQUENCE DAMAGE split across the TWO dagger strikes** of each basic attack cycle, not each hit dealing the full old single-attack budget. Six purchased skill cap totals remain150, one rank point each classLv1..150. Full Assassin design now contains **fully audited 150-level Designer gate proposal** with one eligible new rank at each class level, **NOT USER APPROVED EXACT SCHEDULE**. Even though form chose Critical Mastery `20+5r`, user's explicit note **"we decide mana costs after stat distrobution" OVERRIDES**, so no Assassin numeric spell Mana-cost formula is approved until stat distribution; retain only rank-increasing Critical Mastery activation cost concept. [Detailed Round10](classes/ASSASSIN_TIER3_DESIGN.md). No Unity changes.


## 2026-11-27 — Assassin TierIII Round11 approved AGI-only offense, Lv1 stats, BaseMana100, glass cannon

NEW USER-APPROVED Assassin TierIII separate classLv1 base **STR4 AGI20 INT5 SPR4 VIT5** (total38), **BaseMana100**, universal MaxMana = 100+5INT = **125 at classLv1**, regen/sec=4+0.1SPR = **4.4 at classLv1** before equipment; **ALL normal basic attacks and physical skills scale AGI ONLY**, no STR contribution. Class explicitly **GLASS CANNON**, low survival in melee. User chose **CUSTOM growth yet to decide**, asked to inspect earlier TierIII classes. Designer-only compared approved Berserker(+11.5 growth), Fire Mage(+11.25), Psion branches (+12.2/+11.7 user-corrected) and earlier Marksman proposal(+11.1). **Proposed NOT APPROVED Assassin growth** VIT+1.5 STR+0.5 AGI+7 INT+1 SPR+1 (total+11). Lv150 proposed AGI1063 and Mana870 contingent on approval, do not lock. Assassin starting total38 is conspicuously below other TierIII starts; preserve approved values unless user changes. Numeric skill Mana costs remain DEFERRED until stat distribution is finalized; staggered exact rank gates remain draft. [Source-based table and full Assassin Round11](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round12 approves stat gains, requests higher starting attributes without numbers

USER-APPROVED final Assassin class-per-level growth after custom corrections: **VIT+1.7, STR+0.7, AGI+7, INT+0.8, SPR+0.8**, total **+11.0 per level**, correcting Round11 proposed +1.5/+0.5/+7/+1/+1 via transferring0.2 SPR gain to VIT and0.2 INT gain to STR. User separately requests **INCREASE Level1 starting attributes** but supplies NO revised numeric starting values; exact new base ATTRIBUTES REMAIN OPEN. Previously chosen Lv1 STR4 AGI20 INT5 SPR4 VIT5 (total38) should be treated as historical last-number baseline to revise, NOT a finalized new profile. Class BaseMana100 and universal Mana max/regen equations stay; recompute Mana after replacement starts signed off, then discuss numeric Mana COSTS. Existing pure AGI-only glass cannon/dual dagger skill specifications,150 rank caps and draft gate schedule unchanged. [Full Assassin Round12](classes/ASSASSIN_TIER3_DESIGN.md). Docs-only.


## 2026-11-27 — Assassin TierIII Round13 FINAL Lv1 starting stats and Mana projections

User explicitly changes starting option C (VIT15 STR8 AGI40 INT10 SPR9) by transferring **2 AGI to STR,3 AGI to VIT,4 SPR to VIT**. FINAL APPROVED separate Assassin TierIII **Lv1 VIT22 STR10 AGI35 INT10 SPR5, sum82**. Overrides Round11 interim tiny starts and unadjusted option C. Growth remains Round12 APPROVED **VIT+1.7 STR+0.7 AGI+7 INT+0.8 SPR+0.8**, sum+11/level. Intent: **higher early durability, steep AGI-only damage growth later**; class remains pure melee DPS, not tank. No-gear pre-rounding Lv60 VIT122.3 STR51.3 AGI448 INT57.2 SPR52.2; Lv150 VIT275.3 STR114.3 AGI1078 INT129.2 SPR124.2. With ClassBaseMana100 and global formulas, **MaxMana 150/386/746** and **ManaRegen/s4.5/9.22/16.42** at Lv1/Lv60/Lv150 respectively. Mana **skill costs** still pending user design after stats, per Round10 directive; proposed exact rank gates still unapproved. [Full Assassin Round13](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round14 cadence 2.5s/0.3s and Execute must unlock last

APPROVED: Two-hit/basic sequence every **2.5 seconds**, two impacts separated **0.3 seconds**, each **50% of total raw sequence damage** before mitigation/crit/opening multiplier; no autonomous separate dagger attack clock. **Moderate Mana costs as qualitative balance goal**, actual numbers TBD. Execute is user-approved as **FINAL Assassin skill to first unlock**, superseding earlier provisional ExecuteLv8 prior to ArmorPenLv10. A **NEW 150-level gate table** in [full Assassin design](classes/ASSASSIN_TIER3_DESIGN.md) has **PROPOSED**, NOT APPROVED first unlocks Stealth1 Envenom2 Backstab3 CriticalMastery5 ArmorPen8 Execute12; all150 gates individually audited one per classLv1–150 and caps25/30/35/25/20/15. User has approved ORDER RULE, not exact numbers/gates. Final Lv1 stats VIT22 STR10 AGI35 INT10 SPR5, growth1.7/.7/7/.8/.8 and BaseMana100 remain unchanged. Design only, not gameplay implementation.


## 2026-11-27 — Assassin TierIII Round15 locks entire150-rank class level progression

USER APPROVED **ALL exact per-skill rank minimum class-level gates** from Round14 audited table. First unlocks **Stealth Lv1, Envenom Lv2, Backstab Lv3, Critical Mastery Lv5, Armor Penetration Lv8, EXECUTE LAST Lv12**, 150 total points and caps25/30/35/20/15/25 respectively. Full exact ordered gate arrays in [Assassin design](classes/ASSASSIN_TIER3_DESIGN.md), **now USER-SIGNED OFF**. Audited class Lv1..150 has exactly one new rank-gate at each level and every rank prerequisite can be met through normal single-point-per-level spending, no mandatory banking. Provisional rank-scaled Mana activation cost DESIGNER-ONLY proposals for next approval: Stealth8+2r (max58), Envenom10+2r (max70), Backstab12+2r (max82), Execute15+3r (max90), CriticalMastery12+3r (max72), passive ArmorPen none; **NOT USER APPROVED COSTS**. Existing approved stats/Mana, 2.5sec dual-dagger cycle, crit charges etc unchanged. Documentation only.


## 2026-11-27 — Assassin TierIII Round16 Execute qualitative finisher resource/damage/cooldown direction approved

USER CONFIRMS Execute ought to be **high-damage, MORE MANA EXPENSIVE, LONG-COOLDOWN, deliberately TIMED** rather than a routine/cheap attack, in addition to existing ALWAYS-USABLE and CONTINUOUS MISSING-HP scaling. They selected CUSTOM numeric Mana adjustments and specified only Execute direction: **NO EXACT Mana formula/CD seconds/damage coeff user-approved**. Designer-only suggestion for later approval: Execute cost `20+5*rank` (25 at r1,145 cap25), CD35s, physical damage normalized by raw full basic sequence B `B*(1+.09r)*(1+(.30+.03r)*missingFraction)`. Candidate separate Stealth/Envenom/Backstab/CriticalMastery costs remain unapproved, fixed existing CriticalMastery CD25s remains authoritative. Skill caps, approved 150 explicit class-rank gates, stats,2.5sec dual-hit cycle and global Mana unchanged. [Full Assassin Round16](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round17 five rank-cost and cooldown formulas now USER APPROVED, damage coefficients NOT

USER LOCKS **Stealth cost8+2r CD22s, Envenom10+2r CD8s, Backstab12+2r CD9s, Execute20+5r CD35s, CriticalMastery hybrid activation12+3r CD25s**. Cap costs58/70/82/145/72 (sum427 Mana for one each at maximum), armor pen passive free, global no-gear Lv150 Mana746. Execute now has **1.2sec windup then ONE powerful dagger strike**, not a two-hit basic sequence; precise cancel/interruption/refund rules OPEN. All raw damage formulas, Stealth opener numerical bonus, Backstab positional damage coefficients, Execute continuous missingHP coefficient and Envenom poison damage remain **DESIGNER PROPOSALS**, as user specifically requests **one-skill-at-a-time damage review**. Start with Stealth next. Approved full150 rank level gate schedule and class stats unchanged. [Detailed Assassin Round17](classes/ASSASSIN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — Assassin TierIII Round18 Stealth formula now user-approved, Envenom alternate design under discussion

Assassin STEALTH r1..25 **USER APPROVED opener multiplier `1.10+0.05r`, yielding 1.15x at r1 and2.35x at r25**. **Concealment duration6sec at r1 ->15sec at r25**, intermediate interpolation pending exact rule; opener triggers on **first basic TWO HIT sequence or Backstab ONLY**, not Envenom/Execute, opener spent **ON ATTEMPT INCLUDING MISSES** and both dagger hits in first attempted basic cycle retain multiplier even if first packet misses. Stealth move modifier, cooldown22s and Mana8+2r unchanged. User asked for evaluation of **Envenom active poison dagger stab vs activated temporary poison-coated twin dagger BUFF adding Nature packets and DoTs to successful hits**; **NO ENVENOM REWORK APPROVED YET**, retain locked cap30/first unlock Lv2/Mana10+2r/CD8s pending user decision. All other damage coefficients remain awaiting per-skill approval. [Full Round18](classes/ASSASSIN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — Assassin TierIII Round19 Envenom reworked to activated poison coat BOTH blades, one refreshing DOT

USER APPROVED **Envenom as an ACTIVATABLE temporary dual-dagger coating self-buff**, NOT a separate direct poison stab. Only SUCCESSFUL NORMAL BASIC hits and BACKSTAB, never EXECUTE, apply bonus Nature poison damage and ONE nonstacking enemy poison DoT (later contacts refresh its duration). User's free text overrides selected15->30sec based on Envenom rank: **coating self-buff duration = Stealth skill duration +2sec**, endpoint8seconds at StealthRank1 to17seconds at StealthRank25, enabling Envenom before Stealth without losing coating before opener. This is NOT enemy DoT duration; DoT length/tick damage and separate per-hit Nature amount still OPEN, as is whether recast at8sec refreshes without stacking. Preserve Envenom user-approved first unlockLv2/rank30/full150 gates/Mana10+2*EnvenomRank/CD8s, and all remaining class, Stealth opener, Execute and CriticalMastery mechanics. [Detailed Assassin Round19](classes/ASSASSIN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — Assassin TierIII Round20 Envenom rank-based coating duration correction and Nature damage

USER CORRECTS prior mistaken Stealth-linked duration: Envenom coating duration **uses its OWN rank**, **8seconds at Envenom rank1 ->17seconds at Envenom rank30**, INDEPENDENT of the player's actual Stealth rank; endpoints intentionally 2seconds longer than independent Stealth rank1=6sec and rank25=15sec. `StealthDuration+2` was a misunderstanding and is superseded. Envenom on-hit extra Nature packet per SUCCESSFUL basic dagger hit or Backstab **`B*(0.08+0.006rE)`** r1=8.6%B cap30=26%B, Execute excluded, Nature supplement no automatic physical Stealth opener multiplier. SINGLE poison enemy DoT **4seconds with1-second ticks**, new hits REFRESH not stack; **DoT tick damage NOT YET APPROVED**. Recast coating refreshes its full buff duration without stacking, casting Envenom while concealed DOES NOT break Stealth or consume opener. Envenom Mana10+2rE, CD8s, rank cap30 and all approved class-level gates unchanged. [Full authoritative Round20](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round21 Envenom half-second DoT cadence and independent linear coating duration

USER locks Envenom enemy poison **DoT ticks every0.5seconds** with **HALF the initially selected1sec per-tick damage**, final `B*(0.0075+0.001rE)` Nature PER tick, rank1=.0085B, rank30=.0375B. Enemy DoT expires4sec after last eligible successful coated BASIC dagger or BACKSTAB contact, **ONE nonstacking status**, each further hit refreshes4sec expiry **without restarting or delaying the continuing 0.5sec tick schedule**. For an uninterrupted application,8 ticks across4sec give .068B total r1 and .30B total r30, unchanged total from original1sec tick selection. USER also signs off **linear Envenom coating duration `8+9*(rE-1)/29` sec**, independent Envenom rank8s r1 to17s r30 (not Stealth rank). Nature on-hit rank scaling `B*(.08+.006rE)`, coating reactivation, concealment preservation, cap30, Mana10+2rE/CD8s, rank gates untouched. Backstab and Execute RAW damage coefficients await separate user review. [Assassin Round21](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round22 Backstab base and positional multipliers user-locked

Assassin BACKSTAB single Physical hit damage **`B*(0.90+0.07*rB)`**, r1=.97B r35=3.35B. Attack usable from ALL facing; target's frontal90deg no positional bonus, side/rear270deg applies **`1.15+0.01*rB`** (r1=1.16x, r35=1.50x), **FACING CHECKED AT ACTUAL HIT IMPACT**. Eligible first stealth-breaking Backstab multiplies both sides by Stealth opener`1.10+0.05*rS`, max2.35x. Max Backstab side/rear raw without opener5.025B, with max opener **11.80875B (~11.809B)** before defenses/crit/Envenom Nature proc. Exact earlier positional max1.95x draft superseded. Backstab Mana12+2rB / CD9s and all class rank gates/stats retained. Execute coefficient still awaiting independent approval, no gameplay implementation this round. [Authoritative Assassin Round22](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round23 Execute exact missingHP damage and standard interruption rules locked

User signs off **Execute `B*(1+0.12*rX)*(1+(0.40+0.07*rX)*m)`** for target missingHP fraction m0..1, physically AGI-only: cap25 fullHP4B, halfHP8.30B, near-death12.60B, continuous scaling with no HP gate. This is ONE heavy dagger attack after its existing1.2sec windup, and use **normal CHANNELING INTERRUPTION RULES**; on interrupted windup **CANCEL, refund Mana, consume NO cooldown**, respecting existing global casting behavior. Mana20+5r and CD35s unchanged. User's extra note says "backstab ... missing hp bonus ... high mana and cooldown"; only Execute has missingHP and expensive/long CD, so treat it as a probable request for a FUTURE Execute bonus increase WITHOUT overriding approved formula; candidate higher missingHP slope .40+.09rX would reach14.6B at cap near death, unapproved. Backstab rank35 base3.35B, side/rear max1.5x and Stealth max2.35x untouched. [Assassin Round23 authoritative](classes/ASSASSIN_TIER3_DESIGN.md). Docs-only.


## 2026-11-27 — Assassin TierIII Round24 user confirms EXECUTE finisher not Backstab and approves stronger missing HP bonus

CURRENT APPROVED Execute raw Physical damage `B*(1+0.12*rX)*(1+(0.40+0.09*rX)*missingHPFraction)`. MissingHP contribution increased from Round23's .07*rX to **.09*rX**; unchanged full-HP base at rank25=4B, rank25 nearly dead=**14.6B** (50% HP9.3B,25% HP11.95B). User clarifies that earlier request mentioning "backstab" referred to **EXECUTE**, which is longCD/high Mana and uniquely has missing HP bonus. Do NOT adjust approved Backstab coefficients/position; Backstab with maximum Stealth side/rear remains11.80875B before mitigation. Execute approved Mana20+5rX/CD35s/1.2s standard interrupted channel refund/no cooldown and no Envenom/Stealth opener unchanged. All cap/rank gates/stats unchanged, no gameplay implementation. [Assassin Round24](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round25 Stealth rank-duration locked, Thief baseline studied for unapproved normal raw B

Assassin Stealth ranks1..25 **USER APPROVES exact intermediate duration `6+9*(rS-1)/24` seconds**, from6s rank1 to15s rank25, independent of separately approved Envenom duration `8+9*(rE-1)/29`. Real current Thief source `Assets/_DiceFree/Settings/Thief/Thief dagger basic.asset` AGI-only one-hit raw core `5+AGI`, interval0.87s, windup0.27s and `CombatTuning.cs` attack-speed +0.00025/AGI verified; ThiefLv1 AGI18, growth3.1/lvl. **Designer-only recommendation, NOT signed off yet:** Assassin RAW one full two-hit normal cycle budget **`B=5+AGI`**, each dagger hit raw0.5B before per-hit crit/accuracy/mitigation, nominal2.5s cycle and0.3s intra-hit gap; raw core DPS ungeared AssassinLv1~16.14 versus ThiefLv1~26.56, no crit/poison/skills/gear/defenses. Other candidates `B=8+AGI` or `B=5+1.2AGI`. Current `BasicAttack.cs` one-impact cycle requires new shared-pipeline dual impact orchestration to implement Assassin correctly later; not yet built. All rankable Assassin skill raw damage formulas already scale relative to B, so B selection controls total outputs. [Full Round25 source-grounded analysis](classes/ASSASSIN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — Assassin TierIII Round26 USER APPROVES base B8+AGI and scaling intra-dagger gap with global AttackSpeed; higher basic DPS goal

Assassin's approved raw reference `B=8+AGI` (AGI-only, no special alternative damage engine) and full2.5sec attack cycle /0.3sec dagger spacing both divided by existing global AttackSpeed factor. USER REQUESTS Assassin basic DPS EXCEED Thief and reopens previously approved two0.5B hits as likely insufficient; mentions each hit possibly75%/90%/100% of B but **no exact fraction confirmed**. Per-dagger candidate `pB` gives actual combined NORMAL attack sequence raw `2pB` and does NOT redefine B in already-approved Backstab/Execute/poison skill formulas. Based on actual Thief asset `5+AGI`, interval.87s, default AttackSpeed1+.00025AGI, equivalent no-gear classLv1 Thief26.56 rawDPS vs Assassin p.75=26.03,p.90=31.23,p1.00=34.70; p.9 recommended but NOT signed off. ClassLv1 TierIII vs fully leveled TierII ThiefLv60 is not an equal classLv comparison. Need new dual-impact sequence handler using common accuracy/mitigation and correct poison/crit/Stealth per packet; no code implementation this docs round. [Full Round26](classes/ASSASSIN_TIER3_DESIGN.md).


## 2026-11-27 — Assassin TierIII Round27 user current90% dagger selection, reopen 80/85/90 DPS comparison vs equal-class-level Thief

User initially chooses **each physical BASIC dagger impact .90B** with two hits per cycle resulting1.8B, preserving `B=8+AGI`, nominal2.5s cycle and0.3s gap divided by shared AttackSpeed. Immediately says "90% seems best so far" but asks compare **80% vs85% vs90%** before finalizing. Thus .90 is **current user-preferred WORKING selection** and renewed comparison is open; do not silently replace with designer preferred .85. User explicitly means **Thief and Assassin at SAME class level**, not TierII Thief60 vs freshly reset TierIII Assassin1. Real code Thief AGI18+3.1(L-1), BThief5+AGI, cycle.87/AS, AS1+.00025AGI; Assassin AGI35+7(L-1), own baselineB8+AGI, cycle2.5/AS. Without crit/poison/skills/gear/defenses theoretical raw basic DPS at same classLv1 Thief26.56, Assassin p.80 27.76, p.85 29.50, p.90 31.23; atLv60 Thief248.55, Assassin p.80 324.53, p.85 344.81,p.90 365.09; atLv150 Thief624.23 vs Assassin882.35/937.50/992.65. ALL pass user's higher BASIC DPS goal vs equal-level Thief; p.85 is designer's post-comparison preference, not approved. Already approved skill formulas use B=8+AGI reference, never rebase them to actual2pB combined sequence. [Full Round27](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round28 exact two-dagger 85% base damage first-playtest approved

User FINALIZES initial-playtest Assassin normal basic damage **reference `B=8+AGI`** and **EACH connecting normal dagger hit raw0.85B**, for combined **1.70B** per full two-impact sequence (replacing prior provisional 90% and historical50%). Nominal cycle2.5sec and0.3sec between hits divided by existing shared AttackSpeed factor, individual impacts retain own hit/crit/defense, Stealth opener applies to both once per initial attempted basic sequence, Envenom triggers Nature and one DoT refresh individually on successful impacts. ALL other ranked skill damage formulas (Backstab, Execute, Envenom Nature) retain **base B=8+AGI**, NEVER multiply by1.70 due to the new combined normal-basic budget. Source-grounded ungeared no-crit/no-poison SAME CLASS LEVEL DPS comparison: ThiefLv1 26.56 vs Assassin29.50 (+11.1%), Lv60 Thief248.55 vs Assassin344.81 (+38.7%), Lv150 Thief624.23 vs Assassin937.50 (+50.2%). These are not actual playtests; user specifies numerical values should be revisited when properly playtested. Next Assassin two-hit target/movement/interruption edge cases. [Assassin Round28](classes/ASSASSIN_TIER3_DESIGN.md). No gameplay changes.


## 2026-11-27 — Assassin TierIII Round29 pending second dagger can be cancelled, no retarget/out-of-range freebies

Assassin approved TWO-HIT basic Physical sequence .85B EACH, baseline reference B8+AGI, full normal cycle2.5/AttackSpeed seconds, interhit.3/AttackSpeed seconds. **NEW USER APPROVED:** second strike **CANCELS if target dies** after first (NO AUTO RETARGET), **fails if target is out of melee range AT SECOND IMPACT** (range recheck at impact), and **CANCELS when player movement command or disabling CC occurs before second impact**. First hit's already resolved damage, Nature poison and consumed BASIC crit charge persist; no second successful-hit procs if cancelled; Stealth opening still consumed once on first sequence attempt even if first/second misses. Full cycle cooldown preservation on partially interrupted attacks and first-strike windup/cancellation are not user-decided yet. All prior class/skills/rank gates unchanged. [Authoritative Round29](classes/ASSASSIN_TIER3_DESIGN.md). No code edits.


## 2026-11-27 — Assassin TierIII Round30 dual-hit interruption/cooldown/animation first-playtest rules locked

Normal Physical basic full two-dagger sequence has reference`B=8+AGI`; EACH impact .85B, both landed total1.70B; interval`2.5/AttackSpeed` and hit gap`.3/AttackSpeed`. USER now approves **FULL ATTACK CYCLE PRESERVED EVEN IF HIT2 CANCELLED**; no shortened cooldown or free repetition. **Movement command or disabling CC during first-hit windup cancels ENTIRE sequence per normal BasicAttack interruption rules** (no unconnected damage/charges/poison). **INVALID HIT2 SECOND-SWING ANIMATION SHOULD BE ABORTED where feasible** (always no damage/procs). Previously locked target death cancels second without auto-retarget; range rechecks at second impact; movement/CC between hits cancels hit2; completed hit1 damage/poison/crit charge remains. Future gameplay implementation needs same shared hit/damage pipeline plus reliable second-impact state, cancellation and cooldown; no game code changed. All six skill rank gates/coefficient/cooldown parameters remain untouched. [Authoritative Round30](classes/ASSASSIN_TIER3_DESIGN.md). DOCS ONLY.


## 2026-11-27 — Assassin TierIII Round31 advancement quest in Arborium, disguised retired assassin, guarded top-floor assassination

Thief TierII Lv60+ promotion to NEW Assassin TierIII Lv1 requires a mentor in/near **Arborium**, World1 major city; mentor is a **RETIRED ASSASSIN WITH SECRET IDENTITY**. User signs trial concept: **ENTER MULTI-STOREY HOUSE, EVADE GUARDS ON MIDDLE FLOORS WITHOUT BEING CAUGHT, ASCEND TO TOP FLOOR AND KILL ASSIGNED TARGET, ESCAPE**. Higher-specificity target kill replaces any inference that a separate stolen-item retrieval is mandatory. Quest MUST use PRE-UNLOCK existing TierII Thief abilities (short Thief Stealth, current standard attacks/skills); cannot require Assassin Backstab or Execute before unlock. Verified existing Arborium West/East Residences have3 authored walkable floors and working stairs/cutaway, but no specific residence chosen nor guards/quest implemented. Detection rules, failure, retry, escape details, target identity and mentor fake civilian role remain user-unapproved. All prior Assassin combat math, caps and rank schedules untouched. [Detailed Round31](classes/ASSASSIN_TIER3_DESIGN.md). Documentation-only.


## 2026-11-27 — Assassin TierIII Round32 West Residence quest location and guard/target/escape rules approved

Assassin advancement from TierII Thief Lv60+ in Arborium now SPECIFICALLY uses **existing THREE-STOREY WEST RESIDENCE** (ground entry, guarded middle, top-floor target). Guards on middle floor enter **alert/investigate** when suspicious and player has SHORT CHANCE TO HIDE; suspicion is not instant failure. TOP FLOOR target initially **UNAWARE, QUICK TO ELIMINATE** using TierII Thief skills. Following assassination, RETURN TO EXIT MUST BE **UNDETECTED**; a pursued escape does not meet the selected win condition. Exact alert durations, failure/retry, guard count, target stats/identity, mentor hidden civilian guise, whether house is instanced or quest-phased and mentor hand-in remain open. Walkable three floors are source-verified in `docs/ARBORIUM_GATES_AND_FLOORS.md`; trial actors/scripts NOT yet authored. Existing Assassin six skill caps and base attack/skill formulas remain untouched. [Assassin Round32](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Assassin TierIII Round33 simple themed promotion flow, immediately eject on guard notice and reset top target

USER LOCKS pre-advancement TierII Thief Lv60+ quest in existing OPEN-WORLD QUEST-PHASED Arborium WEST RESIDENCE (three floors): **TWO SIMPLE alternating guards on SECOND FLOOR**, TierII Stealth **UP stairs**, **KILL UNAWARE QUICK TARGET on TOP FLOOR**, **WAIT for Stealth cooldown**, Stealth **DOWN stairs**, escape the house unseen. When either middle-floor guard NOTICES player at any point (before/after kill), **IMMEDIATELY TELEPORT PLAYER OUTSIDE RESIDENCE**, resetting trial; if target had died, **INSTANTLY RESPAWN/RESURRECT target ALIVE at top**, clear objective. Intent is EASY/THEMATIC, no complex stealth minigame. Explicit free-text override **REPLACES** form's 5-second investigation grace period and Round32 recoverable alert; no investigation timers/extra chases, no boss fight, no forced Assassin-only skill. Keep retired assassin mentor concealed identity in/near Arborium; post-success unlock new Assassin TierIII Lv1 and exact mentor hand-in pending. All Assassin class damage/timing/skill caps/gates unchanged; no actual Unity gameplay modification. [Round33 source detail](classes/ASSASSIN_TIER3_DESIGN.md).


## 2026-11-27 — Assassin TierIII Round34 ordinary-person target and plain gameplay reset, no magic lore

USER CLARIFIES top-floor target in Arborium West Residence TierIII Assassin advancement trial is **simply a PERSON, not a magical training construct**. Target might have intel or work for the dragon but this is OPTIONAL flavor, not an approved story requirement. A failed attempt from guard noticing player instantly teleports TierII Thief OUTSIDE and resets/respawns target ALIVE; **NO IN-UNIVERSE REASON OR LORE EXPLANATION** needed for respawn. Keep easy thematic TierII Stealth upstairs/kill/wait CD/Stealth downstairs and existing two simple guards, quest-phased three-storey West Residence. Mentor's specific false civilian occupation is still open, no obligatory bookseller. All class combat and ranks unchanged. [Assassin Round34](classes/ASSASSIN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — Guardian first TierIII coverage from Defender, user-approved lineage/name

User explicitly elects **Guardian** as NEXT TierIII design from TierII **Defender**, after completing Assassin design and correcting premature branching into Duelist/Burglar. Current coverage goal one FIRST TierIII PER each TierII; missing first designs after Guardian starts are Priest and Acolyte. Standard TierIII eligibility Defender classLv60+ -> separate Guardian classLv1, parent retained. Guardian's detailed identity, exact six/other skill roster, resource, rank pool and advancement quest are UNDECIDED and do not inherit arbitrary TierII numbers. TierII Defender sword+shield Resolve+5m ally aura and Iron Guard 2s flat PDEF/MDEF defense are source baselines; historically reserved TierIII Iron Guard variant storing incoming pre-mitigation damage for eventual release is a potential Guardian signature, but **user has not assigned it to Guardian yet**. [Guardian design interview](classes/GUARDIAN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — GUARDIAN-002 user-approved Guardian TierIII core protection identity and inheritance

DefenderLv60+ -> Guardian separate TierIII new Lv1 manifestation. User APPROVES **dedicated party-protector tank**, **SWORD + SHIELD**, same **RESOLVE** resource, **key retained parent Defender skills plus new ally-protection skills**, and specifically assigns the previously reserved TierIII **Iron Guard stored incoming pre-mitigation damage and release after stance** evolution to Guardian. Guardian still needs to be solo viable. Exact old-skill selection, new direct ally shielding/intercept identities, 2s IronGuard upgrade numeric scaling, targeted vs AoE release, starting stats,150 skill ranks, guardian mentor/quest and implementation remain OPEN and should not be invented. Parent Defender TierII Iron Guard continues no damage release. [Guardian class design Round1](classes/GUARDIAN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — GUARDIAN-003 classic martial Guardian roster and centered Iron Guard AoE user-approved

User defines Defender -> Guardian as **classic nonmagical sword-and-board knight tank**, leaving flashy magical shield/buff themes to a potential future Paladin/Templar line. Selected Guardian TierIII skills for first pass: inherited/evolved **Challenge, Iron Guard, Bulwark, Hold the Line**, NEW **Intercede**; NOT inherited as Guardian skills are Shield Bash and Shield Sweep (remain in TierII Defender), nor unselected magical Guardian Ward / Barrier / Sacrifice proposals. Iron Guard stores incoming pre-mitigation damage while guarding then releases it as an **AOE EXPLOSION CENTERED ON GUARDIAN**, not a single selected-target attack. Radius, multiplier, Resolve costs, and Intercede exact mechanics remain OPEN. Old TierII Hold the Line already provides 5m elemental-resistance aura, and whether Guardian retains/reworks it remains open; don't change existing parent class. Guardian sword+shield/Resolve dedicated party tank with solo viability. [Guardian Round2](classes/GUARDIAN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — GUARDIAN-004 Round3 mechanics and offensive skills explicitly provisional

Guardian (DefenderLv60+ -> separate TierIII Lv1) first five selected skills remain **Challenge, Iron Guard, Bulwark, Hold the Line, Intercede**, normal Physical sword basics. User selects **Intercede STATIONARY redirects share of nearby ally incoming damage to Guardian AND adds Guardian self-DEF buff; SELF-TARGET legal, self-case buff only without self-redirection**. Guardian **Hold the Line now grants PDEF and MDEF ally bonuses instead of parent elemental-resist aura**; original TierII Defender aura UNCHANGED. Iron Guard stores incoming premitigation damage then releases **Physical AoE shockwave centered on Guardian**. No exact numeric costs/durations/ranks/caps approved. User says **no extra offensive skill for now**, immediately queries current kit and suggests maybe revisiting; no sixth damage skill approved and absence not permanently locked. Guardian currently relies on ordinary sword basics and reactive Iron Guard AoE for offense; designer to present optional simple nonmagical active attack, not add silently. [Full Guardian Round3](classes/GUARDIAN_TIER3_DESIGN.md). Docs only.


## 2026-11-27 — GUARDIAN-005 Round4 approves sixth Guardian skill: target sword hit + taunt

User wants a NEW straightforward on-demand **Physical one-handed SWORD ATTACK** which deals damage and **TAUNTS THAT ONE TARGET**, to give Guardian a useful active attack in solo play and focused party aggro. This complements separately inherited **AoE Challenge**. Do not replace with inherited Shield Bash or add magic/AoE. Working descriptor `Sword Strike` only; final skill name not selected. The complete concept roster is **Challenge, Iron Guard (stored incoming -> Physical AoE counterwave), Bulwark, Hold the Line (Guardian ally PDEF/MDEF aura), Intercede (stationary redirect + personal DEF/self-cast), Sword Strike (single-target sword damage + taunt)**. Six skill identities approved; numeric rank caps/gates, damage/taunt formula, cooldown/Resolve interactions and final naming OPEN. Defender TierII abilities remain unchanged. [Guardian Round4](classes/GUARDIAN_TIER3_DESIGN.md). Documentation only.


## 2026-11-27 — Assassin canonical implementation summary published for Codex, existing class progression unchanged

User directs fixing Assassin Round34 design notes after Codex implementation readiness review. Assassin class design source now leads with authoritative **current** user-approved B=8+AGI coefficient REFERENCE, TWO Physical 0.85B basic dagger hits (total1.70B only both connect), 2.5s/AS cycle and.3s/AS spacing; full150 exact gate schedule, skill math, global Mana, Assassin Lv1 stats, simple TierII Thief Lv60+ WEST RESIDENCE guard/eject/target reset quest. Clearly distinguish ARCHIVED historical B=total-sequence / 0.50B splits from current. Source-derived provisional **implementation defaults** resolve absent user signoff on skill crit eligibility (basics yes; Backstab/Execute/Nature no), Nature poison channel **Physical Nature** (PDEF + Nature resistance, no Assassin ArmorPen), and separate one DoT per (source Assassin,target) with per-owner clocks/stats. User-approved damage coefficients untouched; raw maximum Backstab11.80875B and Execute14.6B remain for playtesting. This is Assassin docs readiness consolidation, **NOT TierIII Assassin implementation, class unlock deployment or alteration of Guardian paused at Round5**. [Current Assassin implementation canon](classes/ASSASSIN_TIER3_DESIGN.md).

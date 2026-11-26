# Class Progression

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

# DiceFree — Tier II Ranger — design interview

**Created:** 2026-10-19  
**Status:** RANGER ROUNDS 1–6 RECORDED — DESIGN ONLY, NOT IMPLEMENTED. **RANGER-021 APPROVED:** the 80-rank six-skill progression and full gate schedule; additional confirmed Round 5 decisions set Arrow Rain's 1.5s sky-arrow channel then four rain waves over 2s, Gun AoE with targeted arrow that NEVER misses (user-corrected RANGER-027), AGI-marked Aimed Shot baseline/scaling, +2 AGI per Bow Mastery rank and its basic+Aimed range bonus, and rooted/stunned Evasive Step restrictions. This is documentation-only; user will ask a separate DiceFree working chat to read all latest specifications and implement after remote desktop access returns. Do NOT change Unity C#, assets, builds or saves here.  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II original four classes](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Defender Tier II interview](DEFENDER_TIER2_DESIGN.md), [Stats and Damage](../STATS_AND_DAMAGE.md).

## Existing confirmed foundations (before Ranger Round 1)

- **CONFIRMED RANGER-005:** Ranger advances from **Physically Blessed Novice at Tier I level 30**, to its own **Tier II level-1 child manifestation** while preserving parent. A **wounded Ranger NPC near the Priest in Trimery** offers a **simple kill Alpha Wolf quest**. Returning to the wounded Ranger after defeating the Alpha Wolf completes the advancement (turn-in as normal class-fork flow). Details such as quest kill credit, boss level/respawn and exact NPC position remain open; no complex tracking or precision trial.
- `docs/CLASS_PROGRESSION.md` specifically identifies **Ranger → Wood Elf** as an agreed Tier II body/species identity. The exact equipment silhouette, sex, costume, character model and animations are not yet specified by this interview.
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md` contains **Arrow Rain**, an earlier **spectral arrow AoE** that *previews* physical ranged play without requiring a bow. Ranger's later Arrow Rain may be upgraded, replaced or merely inspired by this, but is **NOT** automatically inherited as a free Ranger rank/skill.
- The project has **physical ranged DPS** as a different archetype from Greatblade (two-handed physical damage), Defender (physical tank), Mage (elemental magical damage) and Priest (healer/support). Ranger's mechanics must permit normal solo questing and party boss mechanics.
- **Shared skill progression**: new class ranks start at 0, one spendable class skill point at class level 1, skill-specific weighted caps, bounded spendable budget, no pointless skill point reminders, and **aim for ~78–80 total purchasable skill ranks by class level 80 with a feasible rank purchase each level**. We will design Ranger's exact kit before assigning caps and gates.
- **CONFIRMED RANGER-002:** basic ranged attacks work like WC3/Dota/League: ranger **briefly plants/stops to execute each autoattack**, can reposition and kite between attacks, then stops again for the next shot. **Do not allow baseline shooting while moving**. A future skill, temporary buff or later class might permit movement attacks; none is approved yet. Standard global skill targeting option (**Do Nothing / Move Into Range**) applies.
- **CONFIRMED RANGER-006/015:** the **base** normal attack range is class-authored, not determined by shortbow versus longbow family; both have the same reach for a Ranger at the **same Bow Mastery rank**. **Bow Mastery is now CONFIRMED to add a very small amount of range PER SKILL RANK**, on top of that shared class base. Future Tier III Beastmaster (potential shorter base) and Marksman (potential longer base) were floated, NOT approved branches; eligible bows could be shared across classes.

## Ranger Round 1 — confirmed foundations and still-open balances

| Topic | User-confirmed Ranger rule | What remains open |
| --- | --- | --- |
| Bow types & stat | **Wood Elf AGI-primary PHYSICAL ranged class** using both two-handed **shortbows** (lower damage, faster attacks) and **longbows** (higher damage, slower attacks). **RANGER-006: same Tier II basic attack range regardless of bow type**, because attack reach comes primarily from the class/ability rather than being determined by bow item family. **~25% harder-hit and ~20% slower** for comparable longbows is the agreed initial *item design guideline*, not an automatic universal equipped modifier. | Actual Ranger class attack range, specific item stat curves, weapon/ability scaling, animation timing, crit behavior |
| Offhand/quiver | **Do not add equippable quivers/offhand quiver slot for ordinary Ranger**; bows themselves are two-handed. Later **Tier III/IV dual one-handed crossbows or dual pistols/Gunslinger** are speculative alternatives, not Ranger kit and not approved classes. | Optional cosmetic back quiver visual can be designed later; NOT an equippable offhand or required resource |
| Autoattacking | Base bow autoattacks require standing briefly to shoot; movement during pre-release windup cancels the pending arrow, released projectiles persist. **RANGER-014:** Evasive Step leaps briefly **toward mouse-pointer position**, then grants a **few seconds of movement-speed increase** and **evasion = +1 percentage point × Evasive Step skill rank**. | Dodge max distance, cursor-vs-direction target, collision/pathing, movespeed amount, exact buff duration and how evasion fits the shared evasion/hit system |
| Resource | **Mana only** for class active skills, no added Focus/Precision/Stamina/Tempo bars at Tier II. Existing base autoattacks do not need to cost Mana unless separately agreed. | Mana pool/regen, active skill costs and cooldowns |
| Combat pets | **No permanent pet or autonomous companion AI.** Hawk Dive is a **single-target Physical hit**, summoning a hawk for a single strike then disappearing; applies **Hunter's Mark**. **RANGER-011 approved starting tuning:** Hawk Dive **15s cooldown**, mark **8s**, **+10% Ranger-owned attack and skill damage** to marked target, with high-rank mark bonus gradually approaching ~20–25% as **provisional upper balancing ceiling**. Max one marked target per Ranger. **RANGER-012:** Aimed Shot receives an **EXTRA marked-target damage bonus beyond regular mark amplification**. | Exact mark high-rank schedule, separate Aimed Shot marked-target coefficient, rules for marks from other Rangers still open |
| Advancement | **Wounded Ranger near Priest in Trimery** offers simple **kill Alpha Wolf** quest. **RANGER-010 CONFIRMED:** wolf kill must happen **AFTER accepting the quest** to count, return to NPC for advancement; target **existing Alpha Wolf**, not a new quest-only copy. | Alpha Wolf's existing spawn/location still requires live-game verification; NPC exact placement, dialogue and multiplayer kill-credit rules |

### Two-handed bow equipment and animations (implementation notes, NOT YET CODE)

- Bow equipped in mainhand and uses both hand slots; equipping it unequips incompatible offhand items through existing two-handed gear semantics, not a Ranger-specific quiver loophole.
- **Shortbow versus longbow** damage and attack timing are authored in **item stats**, with an **approximate +25% damage per longbow shot and ~20% slower attack speed** compared with otherwise comparable shortbows as an initial balancing heuristic, not a forced coefficient. **Both share Tier II Ranger's class-defined basic attack range.** Bow stats matter: damage, speed, bonuses, intrinsics and other parameters still make each bow more than a cosmetic stick. Different tiers/classes may author different effective ranges without forcing a particular item to change its range in every class.
- **No equippable quiver required**. An on-back visual quiver can be a cosmetic prop or animation aid in the future without occupying a second gear slot or granting stats, if art warrants it.
- **RANGER-009:** basic autoattack needs a short stationary shot **windup**. **On player movement DURING the windup, cancel the arrow before release immediately**, prioritize movement (stutter-step/orb-walk style). After the shot has fired, the projectile travels to completion as normal. Don't let starting movement retroactively undo a fired arrow. Exact cancel buffer/attack cooldown reset is tuning; no baseline shots while moving.
- Tier III/IV dual crossbow or dual pistol Gunslinger is **future class brainstorming only**; ordinary Ranger cannot dual wield and no such class is currently approved.

### Ranger current six-skill kit — user-approved directions, revised Round 3

**The current six identities are accepted, but not fully numerically finalized.** Every new Tier II Ranger skill begins at rank 0. Earlier Round 2 accepted Multishot as a candidate; **RANGER-013 deliberately REPLACES Multishot with an explosive/gunpowder arrow** to foreshadow future Gunslinger/gun classes. Never implement both by silently retaining Multishot in the active kit.

1. **Aimed Shot — CONFIRMED:** heavy single-target Physical shot with stationary windup and **extra FLAT marked-target raw damage = 0.5 × current AGI × (1 + 0.05 × (AimedShotRank−1))** (RANGER-028). The existing Hunter's Mark % boost ALSO applies. Bow Mastery's 1.4× crit permission can apply to the whole eligible Aimed Shot action. Base damage, Mana and cooldown are provisional in balance section.
2. **Exploding Arrow — CONFIRMED Physical Gun:** a targeted arrow **CANNOT MISS** a valid selected enemy (explicit RANGER-027 correction). It hits that enemy for direct Physical Gun damage, then creates a **2.5-meter radius Gun-element explosion CENTERED ON THE HIT ENEMY**. Primary target takes both direct and splash; nearby enemies receive the splash only. The old assistant's imagined missed-hit spread is explicitly superseded. Final damage/Mana/CD details are designer proposals below.
3. **Arrow Rain — CONFIRMED TWO-STAGE TECHNIQUE:** select targeted ground area; the Ranger performs a **1.5s channel**, firing a **large arrow into the sky followed by a smaller one** during the channel, **before damage begins**; whether movement is allowed or cancels the channel is an **OPEN Round 6 decision**. **Only after channel completion**, arrows fall into the target area in **four waves across two seconds**. Each wave tests current occupants (including new arrivals). Whether channel is interrupted by movement/damage/CC remains open; designer recommends usual stationary interruptible channel. Tier II Arrow Rain begins rank0 regardless of Tier I spectral preview.
4. **Hawk Dive — confirmed mark opener:** one-off **single-target Physical attack** featuring a descending hawk that strikes and disappears (not a persistent actor). **Initial approved playtest:** **15-second cooldown**, Hunter's Mark **8-second duration**, +**10% damage from this Ranger's attacks/skills** to that target; higher ranks may grow toward a designer provisional maximum around **20–25%**, not a hard final cap unless user approves its exact limit. Only **one active target per casting Ranger**.
5. **Evasive Step — CONFIRMED:** **3–4 meter obstacle-safe leap toward mouse cursor**, then **3 seconds of +10% Movement Speed** and **+1 percentage point evasion chance PER EVASIVE STEP rank**. Evasion applies **ONLY to enemy basic/autoattacks**, not skills/spells/AoE. **CANNOT CAST WHILE ROOTED OR STUNNED** (RANGER-025); no escape bypass of hard movement CC. No basic shooting while moving from this buff.
6. **Bow Mastery — CONFIRMED (RANGER-024):** passive grants **+2 AGI per rank** and **+0.1m effective RANGE per rank to BOTH Ranger basic/autoattacks AND Aimed Shot** (still same class-based base reach for shortbow and longbow). Grants **+1 percentage point crit chance per Bow Mastery rank at 1.4× raw damage**, eligible **ONLY for basic attacks and Aimed Shot**, not Exploding Arrow, Arrow Rain or Hawk Dive. Crit is an explicit skill-granted rule, not universal.

#### Hunter's Mark — RANGER-008/011/012 confirmed

- A Hawk Dive hit applies **Hunter's Mark** and deals its own single-target Physical strike; hawk immediately leaves without pet AI.
- **Start-of-playtest numbers confirmed:** Hawk Dive **15-second cooldown**; Hunter's Mark lasts **8 seconds** and causes the marked enemy to take **10% additional damage from that Ranger's basic attacks and skills**. Higher-rank mark damage growth toward **~20–25%** remains a tunable guideline, not a locked formula/cap.
- Only **ONE marked enemy per Ranger at once**, enforced independently of cooldown; replacing mark on a new enemy removes that Ranger's earlier mark. Do not accidentally prevent marks from different Rangers unless user chooses global uniqueness later.
- **Aimed Shot** adds a SECOND, ability-specific marked-target **FLAT raw damage component initially +0.5 × Ranger AGI and increasing with Aimed Shot rank**, on top of Hunter's Mark's general damage bonus. **Exact rank-growth formula and calculation ordering** remain OPEN (see RANGER-023), not an extra fixed percent.
- **OPEN:** mark refresh/removal ordering, crit/proc interaction, target death and dispel behavior, mark from different Rangers, ranks and VFX/status indicator. Only caster Ranger gains mark bonus by default.

#### Evasive Step — Round 4 confirmed movement-speed and basic-attack evasion

- **Confirmed:** quick **3–4 meter** leap toward the **mouse cursor**, using collision-safe movement and normal path constraints; **3 seconds of +10% Movement Speed** following the leap; **+1 percentage point evasion per Evasive Step purchased rank**, affecting **enemy basic/autoattacks only** during the same three-second buff.
- Evasion is an **explicit effect sourced from Evasive Step**, **NOT generic Evasion from the Agility attribute**. Existing global stats documentation explicitly says generic Evasion is not committed. Apply evasion to enemy basic attack hit determination; do not apply it to enemy skill/ground-AoE damage by default or treat it as Physical Defense.
- At designer-proposed rank 8, this means **+8 percentage points** of basic-attack evasion; the +10% speed and three-second duration are **not multiplied by rank** unless specifically revised later.
- **CONFIRMED RANGER-025:** the skill **cannot be activated while Rooted OR Stunned**. No unintended escape of hard CC. **OPEN:** exact 3–4m movement distance, cooldown/Mana cost, pointing into invalid geometry, queued activation on CC expiry, interactions with unavoidable enemy autoattacks and future mobility upgrades. Moving before autoattack projectile release cancels that shot, but already-fired projectiles persist.

#### Bow Mastery — Round 4 confirmed range and crit eligibility

- **CONFIRMED RANGER-024:** **+2 AGI PER Bow Mastery rank**, and **+0.1m range × Bow Mastery rank for BOTH basic autoattacks AND Aimed Shot**. Both shortbows and longbows benefit at the same class/rank. **No automatic range increase** for Exploding Arrow, Hawk Dive, Arrow Rain ground placement or Evasive Step; those ranges are authored individually.
- **Confirmed explicit crit grant:** on normal bow basic attacks and **Aimed Shot only**, Bow Mastery grants **+1 percentage point crit chance per skill rank**, crit result **1.4× RAW damage** before mitigation. **RANGER-021 APPROVED max rank 8**, so rank 8 grants **+16 AGI, 8% crit chance at 1.4×**, and **+0.8m basic-attack AND Aimed Shot reach**.
- **NOT included in Bow Mastery's crit eligibility:** Hawk Dive, Arrow Rain, Exploding Arrow, Evasive Step or non-Ranger damage effects. They may receive their **own separately authored crit sources** in the future; no global crit stat granted.
- DiceFree's `docs/STATS_AND_DAMAGE.md` / `docs/COMBAT.md` policy is **explicit crit permissions; no universal baseline multiplier**. If the action is eligible for Bow Mastery's 1.4× rule and another item/skill crit rule, follow the system's **highest-multiplier-first independent roll**. Do not turn +rank% into a globally pooled crit chance.
- **RANGER-021/024 APPROVED:** Bow Mastery rank cap8 and **+2 AGI/rank**. At max rank8, totals **+16 AGI**, **+0.8m range** to basic attacks/Aimed Shot, and **8% crit permission at 1.4×** to those two attack types. Other authored crit rules follow the existing ordering/priority policies.

#### Exploding Arrow — ALWAYS HIT target and Gun splash (RANGER-027 correction)

- **CONFIRMED:** Exploding Arrow **CANNOT MISS** when cast successfully against a valid enemy. On valid target, arrow **HITS the enemy**, deals direct **Physical Gun** damage, then a 2.5m radius **Gun-element explosion at that enemy**, dealing one splash instance to the primary AND nearby enemies. The primary takes **direct + splash**, nearby enemies only splash.
- **RANGER-027 OVERRIDES the earlier RANGER-022 'miss' thought experiment**, which the assistant introduced without user authorization. **REMOVE any random-miss roll, miss landing-point offset or projectile-ground scatter from future implementation.** Normal no-cast outcomes for invalid/dead target, obstructed projectile, out-of-range attempts or immunity are a *separate* concern, not an accuracy-miss mechanic.
- **Physical channel and Gun element** are independent combat attributes. Apply applicable Physical Defense and Gun resistance; do not assume unrelated DiceBound Gun proc/penetration defaults.
- **Proposed rank1 damage:** `1.10 × B` direct to target plus `0.55 × B` Gun splash to every enemy inside 2.5m including that same target, `B = current basic bow attack RAW damage`. **Proposed rank scaling:** direct `(1.10 + 0.07 × (rank−1)) × B`, splash `(0.55 + 0.035 × (rank−1)) × B`. At max rank18 direct2.29×B plus splash1.145×B. **These coefficients are designer playtest PROPOSALS, not user-signed exact balance**.
- **OPEN:** timing if target becomes invalid midflight, explosion target eligibility, effects on protected invulnerable targets, Mana/cooldown balance, final animation and any special gun-style triggered effects. This ability does **not** receive Bow Mastery's crit rule.

#### Aimed Shot — Round 4 confirmed AGI flat-damage mark bonus

- **Confirmed:** Aimed Shot does its own single-target Physical strike with a brief stationary windup. Against an enemy with **this same Ranger's** Hunter's Mark, apply an **ADDITIONAL FLAT damage bonus that scales with the Ranger's current AGI**, **not** a second +20% flat percentage multiplier.
- **RANGER-023 APPROVED:** **INITIAL EXTRA FLAT BONUS** against own marked target begins at **+0.5 × current Ranger AGI raw damage**, ON TOP of Aimed Shot's normal raw shot damage and the mark's existing percent damage bonus. **The extra AGI-derived amount ALSO GROWS by +5% of its initial value for EVERY extra Aimed Shot rank** (RANGER-028 APPROVED). The raw bonus shares the single action-wide Bow Mastery crit permission (1.4× if triggered) and then gets the usual mark damage modifier on that Ranger's hit. **USER-APPROVED FORMULA (RANGER-028):** `bonusRaw = 0.5 × currentAGI × (1 + 0.05 × (aimedShotRank−1))`. At rank20 the added raw amount is **0.975×AGI** before the normal mark amplification and potential Bow Mastery crit.
- Keep one target per Ranger's Hunter's Mark; the general Hunter's Mark damage multiplier still applies to Aimed Shot whether or not any extra marked-damage bonus is active (Aimed Shot must also be useful on an unmarked enemy).
- **OPEN:** baseline shot coefficient, aimed cast/windup duration, cancellation on player movement/CC, packet/crit/modifier ordering beyond global norms and latency/cancel forgiveness.

### Tier III specialization directions — discussed, NOT CONFIRMED future branches

- User floated **Beastmaster Ranger**: perhaps shortbow-oriented with **shorter class-defined normal attack range** and (at Tier III) animal/companion features, subject to later class design.
- User floated **Marksman Ranger**: perhaps longbow-oriented with **longer class-defined normal attack range**, emphasizing precision and heavy shots, subject to later class design.
- **Bows can be cross-usable** if class equipment permissions allow it. The **class/ability determines normal range**, so a Marksman using a compatible shortbow would still get Marksman's range and a Beastmaster using a compatible longbow would get Beastmaster's range, absent explicitly authored exceptions.
- **Neither class specialization, item lock nor class range is committed yet**, just potential future fantasy direction. No tier-III skill inheritance implementation now.

## Ranger Round 1 — five user answers recorded 2026-10-19

- **RANGER-001 CONFIRMED:** AGI as primary; both **longbows (heavy/slower)** and **shortbows (light/faster)** as **two-handed bows**. Exclude equippable offhand quivers, leaving speculative **dual one-handed crossbows** or **Gunslinger dual pistols** for potential later Tier III/IV options. Do not convert speculation into approved class branches.
- **RANGER-002 CONFIRMED:** Ranger's baseline autoattack requires a **brief stop to fire**, WC3/Dota/League rhythm; possible later skill/buff/class to permit shooting while moving remains OPEN, not baseline.
- **RANGER-003 CONFIRMED:** use **Mana** as Ranger's **only** class resource for now. No Focus, Stamina or other extra bar.
- **RANGER-004 CONFIRMED:** no permanent Ranger pet or summoned companion. A temporary **Hawk Dive** attack in which a hawk appears, strikes, and leaves is favored as future pet fantasy foreshadowing, not a persistent actor; final skill selection/mechanics to design.
- **RANGER-005 CONFIRMED:** **Wounded Ranger NPC near the Priest in Trimery** offers **kill Alpha Wolf** advancement quest to eligible Tier I level-30 Physically Blessed Novice. Keep it simple; do not author an obstacle course/archery trial.

## Ranger Round 2 — five answers recorded 2026-10-19

- **RANGER-006 CONFIRMED:** Tier II shortbows and longbows have **SAME normal range**, derived from Ranger class rather than weapon item family. Typical comparable longbows **+25% per-shot damage and ~20% less attack speed** versus shortbows as a **design guideline for bow items**, not a forced universal multiplier. Future Tier III Beastmaster/Marksman class-differentiated reach and bow identity floated but NOT LOCKED.
- **RANGER-007 HISTORICAL INITIAL ROSTER:** user liked Aimed Shot, Multishot/Rapid Volley, Arrow Rain, Hawk Dive, Evasive Step, Bow Mastery at Round 2. **RANGER-013 subsequently REPLACED Multishot with Exploding Arrow**. Do NOT use the old roster as current.
- **RANGER-008 CONFIRMED:** Hawk Dive = **single-target Physical hit**, hawk appears only for strike, applies **Hunter's Mark**, temporarily amplifying **Ranger's own attacks AND skills** against that target. **One marked target at a time per Ranger**. The user noted that duration shorter than CD could achieve uniqueness, but explicitly enforcing one active mark is robust; duration, cooldown, damage bonus and mark-refresh specifics remain open.
- **RANGER-009 CONFIRMED:** player movement **cancels Ranger autoattack WINDUP before projectile release**, WC3/Dota/LoL rhythm. Once an arrow has fired, subsequent movement does not cancel it.
- **RANGER-010 CONFIRMED:** only **post-acceptance** Alpha Wolf kill is credited, with **return-to-wounded-Ranger** turn-in; use the existing Alpha Wolf as quest target rather than adding a separate quest-only boss, when confirmed present in game.

## Ranger Round 3 — five answers recorded 2026-10-19

- **RANGER-011 CONFIRMED:** Hawk Dive cooldown **15s**, Hunter's Mark duration **8s**, initially **+10%** own attacks/skills damage; higher ranks may raise toward ~20–25% (provisional top-end tuning, not exact locked final cap). One marked target per Ranger.
- **RANGER-012 CONFIRMED:** Aimed Shot deals **additional damage against Hunter's Mark** target above and beyond the normal mark amplification, in a short stationary-windup heavy physical shot. Exact extra bonus is OPEN.
- **RANGER-013 CONFIRMED ROSTER CHANGE:** **REPLACE Multishot** in Tier II six skills with **Exploding Arrow / Gunpowder Arrow** (working names), a gunpowder-themed attack to foreshadow later gun class fantasy. **Arrow Rain** remains a **ground-targeted attack dealing multiple waves across time, including enemies entering after cast**. Exploding Arrow's element was **undecided in Round 3 but RESOLVED as Gun by RANGER-016 in Round 4**; exact radius and procs remain open.
- **RANGER-014 CONFIRMED:** **Evasive Step leaps toward mouse pointer**, then grants a **few seconds of extra Movement Speed** and **+1% evasion chance PER Evasive Step skill rank**. No normal moving basic-attack permission.
- **RANGER-015 HISTORICAL CHOICE:** Bow Mastery adds AGI, tiny range/rank, and +1pp 1.4× crit/rank. **RANGER-019 later limited crit to basic attacks + Aimed Shot**, and **RANGER-021/024 later approved Bow Mastery max8, +2 AGI/rank and +0.1m/rank range for both basic and Aimed Shot**. Crit uses explicit class skill permission rather than global stats.
- **RECONCILIATION:** the earlier Ranger Round 2 plan had the same **class base range for longbows and shortbows**, and floated distinct base ranges for possible future Tier III Beastmaster/Marksman. Round 3 adds range growth via Bow Mastery and **DOES NOT** make shortbows longer/shorter than longbows for the same Tier II class/rank. Earlier proposed Bow Mastery flat bow damage/attack speed is **superseded by the AGI/range/crit set**.

## Ranger Round 4 — five user answers recorded 2026-10-19

- **RANGER-016 — CONFIRMED:** Exploding Arrow hits one hostile primary target then **Gun-element AoE explosion around that hit target**. **Primary target takes MORE damage** than surrounding enemies. No Fire/Tech substitution; DiceFree Gun element already exists. Working combat channel is **Physical Gun**, and direct-hit-plus-splash packet design is provisional.
- **RANGER-017 HISTORICAL CORE:** Aimed Shot gets marked-target AGI-scaling flat damage, not extra percent. **Subsequently RANGER-023 chose +0.5 × AGI initially and further increase by Aimed Shot rank**, with rank-growth coefficient still open.
- **RANGER-018 — CONFIRMED:** Evasive Step **3–4m cursor-directed leap**, **3 seconds +10% Movement Speed** and **+1 percentage point evasion per Evasive Step rank**, where evasion works **against enemy basic/autoattacks ONLY**, not against skill/spell/AoE attacks.
- **RANGER-019 — CONFIRMED crit scope:** Bow Mastery 1pp/rank 1.4× crit **ONLY basic/Aimed Shot**. **Subsequently RANGER-024 clarified +0.1m/rank ALSO affects Aimed Shot range and +2 AGI/rank**.
- **RANGER-020 — HISTORICAL DELEGATION, RESOLVED BY RANGER-021:** User requested designer selection of weighted max ranks and gates. **RANGER-021 subsequently APPROVED** the exact schedule **20/18/16/10/8/8 = 80**, verified to allow a point each level 1–80. **Documentation-only; not implemented**.

## Ranger six-skill rank caps and class-level gates — APPROVED playtest baseline (RANGER-021)

### Approved weighted 80-rank budget

All Tier II Ranger skills start at **rank 0**. The new class manifestation starts at **level 1 with one spendable point** and +1 class skill point every level, capped by authored total, following `docs/CLASS_PROGRESSION.md`:
`availableBudget = min(classLevel, 80)`; `unspent = max(0, availableBudget − purchasedClassRanks)`. No free starting skills. One character can progress beyond class level 80 and level 60 advancement threshold; XP/stats continue without phantom overflow points.

| Skill | APPROVED rank cap | First eligible Ranger level | Level gap between purchases | Last eligible level | Complete rank gate levels |
| --- | ---: | ---: | ---: | ---: | --- |
| Aimed Shot | 20 | 1 | 1 | 20 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 |
| Exploding Arrow | 18 | 6 | 4 | 74 | 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62, 66, 70, 74 |
| Arrow Rain | 16 | 12 | 4 | 72 | 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72 |
| Hawk Dive | 10 | 1 | 7 | 64 | 1, 8, 15, 22, 29, 36, 43, 50, 57, 64 |
| Evasive Step | 8 | 1 | 10 | 71 | 1, 11, 21, 31, 41, 51, 61, 71 |
| Bow Mastery | 8 | 1 | 11 | 78 | 1, 12, 23, 34, 45, 56, 67, 78 |
| **TOTAL** | **80** | — | — | **78** | **All 80 ranks eligible by Ranger level 78; final budget point earned at level 80** |

- **Aimed Shot20:** ranks every class level from 1–20; high-rank damage should matter, but its flat marked AGI bonus should not explode quadratically with player AGI, per-rank coefficient and mark stacking.
- **Exploding Arrow18:** first rank class level **6**, further ranks every **4 levels** through level 74. Primary-target Physical Gun impact plus nearby Gun splash scales direct damage with rank; radius and hit count should remain bounded to avoid performance escalation.
- **Arrow Rain16:** first rank at **class level 12**, every **4 levels** through level 72. **CONFIRMED channel 1.5s (big sky arrow, then smaller sky arrow) BEFORE four rain waves over 2 seconds**, each wave testing current targets including late entrants. Per-wave damage grows by rank; channel duration/ground-wave count should stay authored rather than increase indefinitely.
- **Hawk Dive10:** available from class level **1**, every **7 levels** through level 64. Rank1 mark10% for8s at15s CD; proposed rank growth `markBonusPct = min(23.5%, 10% + 1.5 percentage points × (rank−1))`; this approaches the earlier user-agreed ~20–25% upper playtest direction without unlimited multiplicative scaling. This **exact high-rank formula is designer proposal**, not a newly chosen user numeric.
- **Evasive Step8:** first class level **1**, then every **10 levels** through level71; rank1 +1pp basic-attack evasion, rank8 +8pp, +10% movement speed for3s and cursor leap3–4m always. No indefinite evasion uptime nor rank-scaled leap distance without approval.
- **Bow Mastery8:** first class level **1**, then every **11 levels** through level78. **RANGER-024 CONFIRMED: +2 AGI/rank**, **+0.1m basic AND Aimed Shot range/rank**, **+1pp 1.4× crit chance/rank** on basic/Aimed. Totals at rank8: **+16 AGI, +0.8m, 8% crit chance at 1.4×**.

### Verification — every integer Ranger class level 1–80

Count all individual skill ranks whose own level gate is reached at that Ranger class level as `eligibleCount(level)`. An uninterrupted one-point-per-level purchase path exists when `eligibleCount(level) >= level` at all levels up to the budget cap (80), assuming ranks are purchased in ascending order within each skill.

| Ranger class level | Total eligible distinct rank purchases | Earned/capped skill budget | Eligible-purchase cushion |
| ---: | ---: | ---: | ---: |
| 1 | 4 | 1 | 3 |
| 6 | 10 | 6 | 4 |
| 12 | 21 | 12 | 9 |
| 18 | 31 | 18 | 13 |
| 20 | 34 | 20 | 14 |
| 30 | 43 | 30 | 13 |
| 40 | 51 | 40 | 11 |
| 50 | 60 | 50 | 10 |
| 60 | 68 | 60 | 8 |
| 65 | 72 | 65 | 7 |
| 70 | 76 | 70 | 6 |
| 73 | 78 | 73 | 5 |
| 74 | 79 | 74 | 5 |
| 75 | 79 | 75 | 4 |
| 76 | 79 | 76 | 3 |
| 77 | 79 | 77 | 2 |
| 78 | 80 | 78 | 2 |
| 79 | 80 | 79 | 1 |
| 80 | 80 | 80 | 0 |

**USER-APPROVED RANGER-021 and designer-verified for ALL integer levels 1–80**: no level violates `eligibleCount(L) >= L`. All **80** purchasable ranks unlocked by level **78**; player earns the 80th class-skill point at level **80**. As with Greatblade/Mage/Defender, banking remains optional. The HUD upgrade-ready indicator must show only when a next rank is actually purchasable now, while the menu can quietly display banked count and future rank's next eligible level. No Unity balance/code/save implementation in this documentation run.

### Balance/implementation cautions

- **At rank8 Bow Mastery:** **+16 AGI**, **+0.8m basic attack AND Aimed Shot reach**, **+8 percentage points** chance to crit eligible autoattacks/Aimed Shot at **1.4×**. Applies equally to both bow families; future Tier III classes might start at different class-authored base ranges.
- **At rank8 Evasive Step:** +8pp **ENEMY-BASIC-AUTOATTACK-ONLY** evasion after 3–4m cursor leap, 3s +10% movespeed. **Cannot activate while rooted or stunned**. This is an authored miss/avoid rule (generic AGI evasion remains uncommitted) and does not dodge enemy abilities/spells/AoE.
- **At rank10 Hawk Dive:** provisional Hunter's Mark bonus **23.5%** for8s at15s cooldown, capped at one marked target per Ranger. Exact mark high rank coefficient may change during playtests.
- **Exploding Arrow:** **Physical channel + Gun element**, 2.5m AoE centered on the enemy it **ALWAYS hits** (if valid); primary direct + one splash, other enemies one splash each. **No misses, scatter or miss-landing locations**. Bow Mastery crit permission excludes Exploding Arrow, Arrow Rain and Hawk Dive.
- **Aimed Shot:** base Physical damage and flat AGI mark synergy get one action-wide Bow Mastery crit result if eligible; use existing highest-multiplier-first rule when other independent crit sources exist, and do not silently give Gun AoE or ground DoTs this passive crit.
- **NOTE:** this exact rank-cap/gate schedule was **approved by user in RANGER-021** as an INITIAL PLAYTEST baseline. It is **not implemented**. Other optional numeric suggestions, especially mark growth, per-skill mana costs and Aimed Shot marked-flat rank multiplier, remain **unapproved designer proposals**.

## Arrow Rain — CONFIRMED cinematic channel and four-wave rain (RANGER-025)

**This timing overrides any earlier suggestion of three quick Arrow Rain waves or an instant cast.**

- **Target:** player selects ground location in ordinary skill range. Skill has TWO DISTINCT PHASES, including animated shots leading to damage.
- **Phase 1 — exactly 1.5-second CHANNEL:** Ranger must perform an animation of **firing a BIG arrow UP into the sky**, then firing **a smaller arrow up** before any arrows rain down. Both sky-shot animation beats fit inside the full **1.5s** channel. This is not two instant ground damage packets. Actual large/small arrow asset sizes and trajectories are art tasks for the future implementation chat.
- **Phase 2 — four arrows-raining damage WAVES over 2 seconds:** starts **AFTER the 1.5-second channel successfully completes**. Ground effect deals **four pulses spread over a two-second period**, striking eligible enemies present at each individual wave (including targets that move into area after cast). The rain phase is autonomous after being launched, and is NOT a second Ranger channel. Initial full timeline from activation through final wave is approximately **3.5 seconds**.
- **RANGER-029 CONFIRMED:** **3-meter** AoE radius, **4 waves across 2 seconds**. At rank1 **each wave deals ~0.4× a basic arrow's raw damage**, and damage increases with rank. **Proposed (not user-approved) per-rank increase:** `(0.4 + 0.035 × (ArrowRainRank−1)) × B` raw damage per wave; rank16 gives 0.925×B/wave. Suggested tick offsets after channel are +0.5, +1.0, +1.5 and +2.0 seconds; exact spacing can be playtested.
- **RANGER-026 CONFIRMED:** **movement, being stunned or an explicitly interrupting action CANCELS the 1.5s channel**, causing **no rainfall**. Ordinary non-interrupting incoming damage **DOES NOT cancel** the channel. **ONCE channel COMPLETES, all FOUR waves CONTINUE over 2 seconds** even if the Ranger moves, is stunned or is interrupted afterward. The autonomous rain is not a continued channel. **OPEN:** cancellation Mana/CD refund (proposal: normal spend-on-activation unless stated otherwise), exact arrow animation beat spacing and VFX.
- **Consistency with normal combat:** `docs/COMBAT.md` says each ability determines its own cast/channel movement/interrupt behavior, and `docs/CLASS_PROGRESSION.md` says class skills begin rank 0. No extra Tier I Arrow Rain ranks transfer for free. Do not conflate the **attacker's 1.5s channel** with the **autonomous 2s ground effect**, or allow Arrow Rain to rain before the sky shots complete.

## Ranger Round 5 — user answers recorded 2026-10-19

- **RANGER-021 APPROVED:** Ranger total cap **80 ranks** and exact designer-selected six-skill gates: Aimed Shot20, Exploding Arrow18, Arrow Rain16, Hawk Dive10, Evasive Step8, Bow Mastery8, with exact gate levels in schedule table. All levels 1–80 pass purchase-accessibility proof; initial playtest baseline, not code.
- **RANGER-022 PARTIALLY HISTORICAL / SUPERSEDED BY LATEST RANGER-027:** **2.5m Gun-element explosion** around primary enemy, with primary direct+blast damage confirmed. The previous assistant's imagined **MISSED ARROW detonation** and offset-ground landing were mistakenly recorded in this older round and were **explicitly rejected by user in RANGER-027**. **AUTHORITATIVE LATEST: Exploding Arrow CANNOT MISS and always detonates at successfully hit primary target.**
- **RANGER-023 CONFIRMED:** Aimed Shot EXTRA flat marked-target damage starts at **0.5 × Ranger AGI**, **increases further with Aimed Shot skill rank** (exact rank growth formula left for designer to propose/approve). This is additional to the usual mark's percent damage bonus, not a flat 20% extra.
- **RANGER-024 CONFIRMED:** Bow Mastery grants **+2 AGI per rank**; its **+0.1m/rank range bonus applies to BOTH basic attacks and Aimed Shot**. Bow Mastery crit permission stays **basic attacks + Aimed Shot ONLY**, +1pp/rank at 1.4×, consistent with explicit crit framework.
- **RANGER-025 CONFIRMED:** **Evasive Step cannot be cast while rooted or stunned**. Arrow Rain has a **1.5 SECOND CHANNEL** where Ranger fires a **big arrow into sky then smaller arrow**; then **FOUR damaging rainfall WAVES over TWO SECONDS** in the selected ground area, hitting later-arriving enemies too. Precise channel interruption and wave tick spacing remain open.

## Ranger Round 6 — answers confirmed 2026-10-19

- **RANGER-026 CONFIRMED:** moving, being stunned, or taking an explicitly interrupting action **cancels the 1.5s Arrow Rain channel with zero resulting waves**. Ordinary non-interrupting damage does not. After full channel completion, **the 4 rain waves over 2s continue independently** despite movement, stuns or other later interruptions.
- **RANGER-027 CONFIRMED USER CORRECTION:** **EXPLODING ARROW CANNOT MISS** a valid targeted enemy; the previous missed-shot scenario was invented by the assistant and is **withdrawn/superseded**. The Gun AoE (2.5m) centers on the enemy the arrow hits, with direct+AoE on primary, AoE on secondaries.
- **RANGER-028 APPROVED:** marked-target Aimed Shot added raw damage `0.5 × current AGI × (1 + 0.05×(AimedShotRank−1))`. Rank1 extra0.5×AGI; rank20 extra0.975×AGI. Normal Hunter's Mark +damage applies in addition.
- **RANGER-029 CONFIRMED:** Arrow Rain AoE circle **3m radius**, four waves/2s **after channel**, each rank1 wave **0.4× basic arrow RAW damage** and scales at higher ranks. Exact rank growth `+0.035×B/wave per rank` below is designer-proposed, not user-approved.
- **RANGER-030 DESIGNER DELEGATION:** formulate **first-pass Mana costs, cooldowns and high-rank effects** for all six Ranger skills; store as **designer proposals** in documentation, not as confirmed numeric user decisions or live code.

## Ranger six-skill first-playtest balancing — designer proposal, not numeric user approval

Use `B` = the current normal bow basic attack's raw damage before mitigation, and `A` = current Ranger AGI (including skills/gear). Every class skill starts rank0; purchased ranks have separate level gates already APPROVED in this document. Proposed effects do not supersede previously user-confirmed values.

| Ranger skill | Approved cap | Proposed cooldown | Proposed Mana cost | Proposed rank1 effect / first-pass rank growth |
| --- | ---: | ---: | ---: | --- |
| **Aimed Shot** | 20 | **6s** | **10** | Physical direct base `(1.80 + 0.12×(rank−1)) × B`, short stationary windup; if **own Hunter's Mark** present, additionally **APPROVED** `0.5×A×(1+0.05×(rank−1))` raw marked bonus. Bow Mastery 1.4× crit eligible; mark's % gain applies too |
| **Exploding Arrow** | 18 | **12s** | **20** | **Physical Gun**, targeted arrow **CANNOT MISS**. Direct `(1.10+0.07×(rank−1))×B` to primary + one 2.5m splash `(0.55+0.035×(rank−1))×B` to primary and all nearby enemies |
| **Arrow Rain** | 16 | **20s** | **30** | **APPROVED 1.5s channel** (big then small upward arrow), then **4 independent waves in 2s**, 3m circle. Each wave `(0.4+0.035×(rank−1))×B`; rank1 0.4×B/wave APPROVED, later 0.035×B/added rank PROPOSED. Wave entrants hit based on current position |
| **Hawk Dive** | 10 | **15s — user approved** | **15** | Physical target strike `(0.85 + 0.10×(rank−1))×B` PROPOSED. Hunter's Mark **8s, +10% Ranger damage** at rank1 APPROVED; later bonus **+1.5 percentage points per rank** toward **23.5%** at rank10 PROPOSED. Max one mark target/Ranger |
| **Evasive Step** | 8 | **15s** | **12** | **APPROVED:** leap 3–4m cursor-directed; **3s +10% move speed**, **+1pp evasion/rank** for enemy basic/autoattacks only; **cannot cast rooted/stunned**. Movement/avoidance effect rank growth is already defined; CD/Mana provisional |
| **Bow Mastery** | 8 | **Passive** | **0** | **APPROVED:** +2 AGI/rank, +0.1m range/rank to normal basic attacks and Aimed Shot; +1pp crit chance/rank **at 1.4×** for basic attacks and Aimed Shot **ONLY** |

**Proposal philosophy:** Ranger uses **Mana only**, normal arrow autos cost no Mana, and relatively frequent single-target Aimed Shots plus a 15s mark define boss rotation. Gun splash and 1.5s-channel rainfall each cover packs without giving permanent AoE. Default crit remains opt-in, no universal crit multiplier, and Exploding Arrow is not eligible for Bow Mastery's 1.4× rule. High-rank damage growth and costs need real playtesting with longbow vs shortbow B values, Mana regeneration, boss Gun resistance and click-to-move cancellation. Future weapon-specialized Beastmaster/Marksman classes and dual-pistol Gunslinger remain uncommitted ideas.

**Concrete checks for future implementation chat (NOT implementation authorization):** verify 1 skill-point purchase possible each class level through level80; Arrow Rain cancels before rain on move/stun/explicit interrupt but NOT ordinary damage, and four waves continue after channel even if Ranger is stunned; Gun targeted direct shot never uses an accuracy-miss roll; mark bonus is own-Ranger-only and maximum one marked target; Bow Mastery +2 AGI/rank and crit/range eligibility only basic+Aimed; Evasive Step cannot activate while rooted/stunned and only avoids enemy basic attacks; no persistent hawk/pet AI; existing wounded-Ranger/Alpha Wolf advancement timing and parent manifestation saved.

**Next class decision not yet made.** User is choosing between occult **Acolyte**, dagger **Thief/Rogue**, and a secret-class interview. Existing speculative future class directions and original hidden Greatblade+Magically Touched Novice milestone live in `docs/TIER2_CORE_CLASSES_DESIGN_V02.md` and `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md`.

## Decision log

| Date | Topic | State |
| --- | --- | --- |
| Existing | Physically Blessed Novice physical ranged Ranger candidate | **Prior documented branch candidate** |
| Existing | Ranger body identity Wood Elf | **Prior confirmed identity** |
| Existing | Physically Blessed Novice Arrow Rain preview | **Historical implemented/working Tier I spell; not a free Ranger skill** |
| 2026-10-19 | User requested Ranger design after Defender | **CONFIRMED intention; design only** |
| 2026-10-19 | RANGER-001 | Both two-handed shortbows and longbows (speed/damage tradeoff), no quiver offhand, AGI primary; future dual crossbows/pistols just ideas | **CONFIRMED** |
| 2026-10-19 | RANGER-002 | Autoattack stationary shot rhythm; attack-on-move only possible later ability/class | **CONFIRMED** |
| 2026-10-19 | RANGER-003 | Mana-only class resource | **CONFIRMED** |
| 2026-10-19 | RANGER-004 | No pet; possible one-off Hawk Dive attack theme | **CONFIRMED direction; skill specifics open** |
| 2026-10-19 | RANGER-005 | Wounded Ranger near Trimery Priest requests killing Alpha Wolf, simple advancement | **CONFIRMED direction** |
| 2026-10-19 | RANGER-006 | Same Ranger class-defined bow range, longbow typical +25% shot damage / 20% slower vs shortbow; potential tier III class range differences only future ideas | **CONFIRMED** |
| 2026-10-19 | RANGER-007 | Initial roster included Multishot; later RANGER-013 replaced it with Exploding Arrow | **HISTORICAL; MULTISHOT SUPERSEDED** |
| 2026-10-19 | RANGER-008 | Hawk Dive single-target Physical plus Hunter's Mark, one target per Ranger, +damage from own attacks and skills | **CONFIRMED; % and uptime open** |
| 2026-10-19 | RANGER-009 | Movement interrupts pre-release autoattack windup; already launched arrows still resolve | **CONFIRMED** |
| 2026-10-19 | RANGER-010 | Quest must be accepted before existing Alpha Wolf kill, then return to wounded Ranger | **CONFIRMED** |
| 2026-10-19 | RANGER-011 | Hawk Dive 15s CD, mark 8s, +10% own damage; later toward 20–25% | **CONFIRMED first-rank tuning; high rank provisional** |
| 2026-10-19 | RANGER-012 | Aimed Shot gets additional marked-target damage above normal Hunter's Mark bonus | **CONFIRMED concept** |
| 2026-10-19 | RANGER-013 | Replace Multishot with gunpowder/Exploding Arrow; retain ground multiwave Arrow Rain | **CONFIRMED core, explosion details open** |
| 2026-10-19 | RANGER-014 | Evasive Step mouse-directed leap, temporary Movement Speed buff and +1% evasion per skill rank | **CONFIRMED core** |
| 2026-10-19 | RANGER-015 | Bow Mastery AGI + tiny range/rank + 1pp crit/rank at 1.4× | **CONFIRMED core; subsequent RANGER-019 settles +0.1m/rank and basic+Aimed crit scope** |
| 2026-10-19 | RANGER-016 | Exploding Arrow: Gun-element projectile, explosion centered on hit target, primary takes more damage | **CONFIRMED core; Physical channel is designer interpretation** |
| 2026-10-19 | RANGER-017 | Aimed Shot extra marked-target FLAT damage scaling with Ranger AGI | **CONFIRMED core; coefficient OPEN** |
| 2026-10-19 | RANGER-018 | Evasive Step 3–4m cursor leap, 3s +10% MoveSpeed, +1pp/rank evasion on basic attacks ONLY | **CONFIRMED** |
| 2026-10-19 | RANGER-019 | Bow Mastery crit basic+Aimed Shot ONLY, +1pp/rank at 1.4×; +0.1m basic range/rank; +AGI | **CONFIRMED; AGI number OPEN** |
| 2026-10-19 | RANGER-020 | Delegated six-skill cap/gate schedule design, designer proposal 20/18/16/10/8/8 = 80 | **DELEGATED; proposed schedule awaits user approval** |
| 2026-10-19 | RANGER-021 | Exact 80-rank six-skill caps/gates | **APPROVED initial playtest** |
| 2026-10-19 | RANGER-022 | Gun explosion 2.5m primary target; miss branch was later corrected and disallowed by RANGER-027 | **2.5m CONFIRMED; MISS SCENARIO HISTORICAL/SUPERSEDED** |
| 2026-10-19 | RANGER-023 | Marked Aimed Shot +0.5×AGI flat bonus plus extra skill-rank scaling | **CONFIRMED; rank coefficient OPEN** |
| 2026-10-19 | RANGER-024 | Bow Mastery +2 AGI/rank; +0.1m range/rank to basics AND Aimed; crit eligibility unchanged | **CONFIRMED** |
| 2026-10-19 | RANGER-025 | Evasive Step unavailable while rooted/stunned; Arrow Rain 1.5s big-then-small upward-arrow channel + 4 waves over 2s | **CONFIRMED; channel interrupts OPEN** |
| 2026-10-19 | RANGER-026 | Arrow Rain channel interrupted by movement/stun/explicit interrupt, not regular hits; 4 waves continue after successful channel regardless later CC | **CONFIRMED** |
| 2026-10-19 | RANGER-027 | Exploding Arrow targeted shot always hits, no miss branch; user corrected assistant's invented scenario | **CONFIRMED OVERRIDE** |
| 2026-10-19 | RANGER-028 | Marked Aimed Shot flat AGI damage rank bonus +5% of starting amount per rank, rank20 0.975×AGI | **CONFIRMED** |
| 2026-10-19 | RANGER-029 | Arrow Rain 3m radius, rank1 0.4× basic RAW damage per wave, 4 waves/2s | **CONFIRMED start; high-rank coefficient proposed** |
| 2026-10-19 | RANGER-030 | First-pass Ranger cooldown, Mana and damage balance table | **DELEGATED as designer PROPOSALS, not final approval** |
| 2026-10-19 | Class design next | Acolyte, Thief/Rogue and secret class candidates considered | **NOT SELECTED** |

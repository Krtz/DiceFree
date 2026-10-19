# DiceFree — Tier II Ranger — design interview

**Created:** 2026-10-19  
**Status:** RANGER ROUNDS 1–4 RECORDED — DESIGN ONLY, NOT IMPLEMENTED. Round 4 confirms Physical Gun Exploding Arrow with emphasized primary target, AGI-based flat marked Aimed Shot bonus, basic-attack-only Evasive Step evasion, Bow Mastery crit on basic attacks/Aimed Shot and +0.1m range per rank. Ranger six-skill weighted cap and gate timetable is a **designer proposal**, checked against all 80 levels, pending user review. This is documentation-only; user will ask a separate DiceFree working chat to read all latest specifications and implement after remote desktop access returns. Do NOT change Unity C#, assets, builds or saves here.  
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

1. **Aimed Shot — CONFIRMED flat AGI-based marked synergy:** heavy **single-target Physical shot** with short stationary windup. A target marked by the **same Ranger** takes the regular percentage Hunter's Mark damage increase AND an additional **FLAT damage amount proportional to the Ranger's AGI** (not an extra fixed percent). The exact AGI coefficient, interaction with Aimed Shot's rank and mark multiplier, windup and damage baseline are designer tuning questions. This shot and basic attacks are eligible for Bow Mastery's special 1.4× crit; other Ranger skills are not eligible from Bow Mastery.
2. **Exploding Arrow (gunpowder-themed name provisional) — CONFIRMED:** a **Physical-channel, Gun-ELEMENT** arrow strikes one hostile primary target, then causes an **AoE explosion centered ON THAT HIT TARGET**. The direct target **takes MORE total damage** than enemies merely near the blast. The explosion is a real Gun-element attack, not just an animation/VFX color. Initial damage split, radius, Mana cost, cooldown and exact miss/detonation rules remain for design. This REPLACES Multishot in the six-skill roster.
3. **Arrow Rain — confirmed distinct ground AoE:** ground-targeted arrows falling in **repeated waves over a short time**, affecting enemies entering the area after casting, rather than a one-time snapshot of enemy roster. This succeeds Tier I's Arrow Rain *thematically*, but starts at **Ranger rank 0**, does not inherit the Tier I skill points, and gets its own skill scaling.
4. **Hawk Dive — confirmed mark opener:** one-off **single-target Physical attack** featuring a descending hawk that strikes and disappears (not a persistent actor). **Initial approved playtest:** **15-second cooldown**, Hunter's Mark **8-second duration**, +**10% damage from this Ranger's attacks/skills** to that target; higher ranks may grow toward a designer provisional maximum around **20–25%**, not a hard final cap unless user approves its exact limit. Only **one active target per casting Ranger**.
5. **Evasive Step — CONFIRMED:** **3–4 meter obstacle-safe leap toward mouse cursor**, then **3 seconds of +10% Movement Speed** and **+1 percentage point evasion chance PER EVASIVE STEP rank**. Evasion applies **ONLY to enemy basic/autoattacks**, not enemy active abilities/spells/AoE, even if those are Physical. An evaded basic attack does not deal HP damage and follows normal dodge/miss rules. No basic shooting while moving from this buff.
6. **Bow Mastery — CONFIRMED:** passive raises **AGI** (amount per rank open), and adds **+0.1m effective BASIC ATTACK RANGE per Bow Mastery rank** to the shared Ranger class base for both bow types. Explicitly grants **+1 percentage point chance to crit per Bow Mastery rank at 1.4× raw damage**, eligible **ONLY for Ranger basic/autoattacks and Aimed Shot**, not Exploding Arrow, Arrow Rain or Hawk Dive. This is an explicit skill-granted crit rule, NOT generic crit chance or an automatic rule for other skills.

#### Hunter's Mark — RANGER-008/011/012 confirmed

- A Hawk Dive hit applies **Hunter's Mark** and deals its own single-target Physical strike; hawk immediately leaves without pet AI.
- **Start-of-playtest numbers confirmed:** Hawk Dive **15-second cooldown**; Hunter's Mark lasts **8 seconds** and causes the marked enemy to take **10% additional damage from that Ranger's basic attacks and skills**. Higher-rank mark damage growth toward **~20–25%** remains a tunable guideline, not a locked formula/cap.
- Only **ONE marked enemy per Ranger at once**, enforced independently of cooldown; replacing mark on a new enemy removes that Ranger's earlier mark. Do not accidentally prevent marks from different Rangers unless user chooses global uniqueness later.
- **Aimed Shot** adds a SECOND, ability-specific marked-target damage bonus **on top of Hunter's Mark's general bonus**; how the two boosts stack (add/multiply), and extra percentage, remain OPEN.
- **OPEN:** mark refresh/removal ordering, crit/proc interaction, target death and dispel behavior, mark from different Rangers, ranks and VFX/status indicator. Only caster Ranger gains mark bonus by default.

#### Evasive Step — Round 4 confirmed movement-speed and basic-attack evasion

- **Confirmed:** quick **3–4 meter** leap toward the **mouse cursor**, using collision-safe movement and normal path constraints; **3 seconds of +10% Movement Speed** following the leap; **+1 percentage point evasion per Evasive Step purchased rank**, affecting **enemy basic/autoattacks only** during the same three-second buff.
- Evasion is an **explicit effect sourced from Evasive Step**, **NOT generic Evasion from the Agility attribute**. Existing global stats documentation explicitly says generic Evasion is not committed. Apply evasion to enemy basic attack hit determination; do not apply it to enemy skill/ground-AoE damage by default or treat it as Physical Defense.
- At designer-proposed rank 8, this means **+8 percentage points** of basic-attack evasion; the +10% speed and three-second duration are **not multiplied by rank** unless specifically revised later.
- **Open:** exact leap-distance target within the 3–4m range, skill cooldown/Mana cost, what if mouse points beyond valid geometry, whether skill can dodge while stunned/rooted, interaction with unavoidable/basic attacks and later class upgrades. Movement before an autoattack's projectile release cancels that Ranger shot; projectiles already fired persist.

#### Bow Mastery — Round 4 confirmed range and crit eligibility

- **Confirmed:** +AGI per skill rank (exact flat AGI growth not yet approved); **+0.1m BASIC ATTACK RANGE × Bow Mastery rank** for Tier II Ranger, equally for **shortbows AND longbows** at the same mastery rank. Basic range remains **class-authored** rather than bow-item-defined. Range granted by Bow Mastery does **not automatically extend every skill's targeting range**.
- **Confirmed explicit crit grant:** on normal bow basic attacks and **Aimed Shot only**, Bow Mastery grants **+1 percentage point crit chance per skill rank**, crit result **1.4× RAW damage** before mitigation. If max rank is the designer-proposed 8, then Bow Mastery rank 8 grants **8% crit chance, 1.4× damage**, plus **0.8m basic attack range**.
- **NOT included in Bow Mastery's crit eligibility:** Hawk Dive, Arrow Rain, Exploding Arrow, Evasive Step or non-Ranger damage effects. They may receive their **own separately authored crit sources** in the future; no global crit stat granted.
- DiceFree's `docs/STATS_AND_DAMAGE.md` / `docs/COMBAT.md` policy is **explicit crit permissions; no universal baseline multiplier**. If the action is eligible for Bow Mastery's 1.4× rule and another item/skill crit rule, follow the system's **highest-multiplier-first independent roll**. Do not turn +rank% into a globally pooled crit chance.
- **Open:** exactly how much AGI each purchased rank adds (recommend starting **+1 AGI per rank** as designer playtest proposal), max rank approval, how ability range buffs interact with class reach and other passive crit modifiers.

#### Exploding Arrow — Round 4 confirmed Physical Gun impact and blast

- **Confirmed:** firing an **Exploding Arrow** at a **single hostile target** creates **one direct hit** and then a **Gun-ELEMENT blast centered on the hit target**, damaging nearby enemies with **Gun-element damage**, while the primary target takes greater total damage than any equally situated secondary target.
- **Working designer interpretation:** **Physical** is the damage channel, **Gun** is the element—these are independent in DiceFree's existing element rules (`docs/ELEMENTS.md`), so Physical Defense and Gun resistance apply as normal. A possible simple resolution: primary target takes a **single direct Physical Gun impact packet + one Physical Gun splash packet**, while each secondary enemy in a small radius takes **only the single splash packet**; no accidental duplicate secondary hits or separate unspecified Fire/Tech packets. Main target gets greater damage because of the extra impact. **User confirmed element/relative damage; exact packet model remains designer suggestion until approved.**
- **PROVISIONAL starting balance:** radius about **2.5m**, projectile/impact hit coefficient maybe **~1.0 bow-attack equivalent** direct, plus **~0.55 bow-attack equivalent** AoE splash to primary+nearby; exact rank scaling, damage, Mana cost and cooldown await balance decisions. Avoid automatically imposing Gun element's legacy DiceBound proc/penetration percentages without confirming the DiceFree element registry and ability permissions.
- **OPEN:** whether a missed primary shot creates no blast (my suggestion), whether splash applies after on-hit mark/crit, multiple target-hit packet attribution, spell-target auto-move into range and max blast victims. Only Hunter's Mark and Aimed Shot use the passive crit rules approved so far.

#### Aimed Shot — Round 4 confirmed AGI flat-damage mark bonus

- **Confirmed:** Aimed Shot does its own single-target Physical strike with a brief stationary windup. Against an enemy with **this same Ranger's** Hunter's Mark, apply an **ADDITIONAL FLAT damage bonus that scales with the Ranger's current AGI**, **not** a second +20% flat percentage multiplier.
- **PROVISIONAL example ONLY:** `bonusRawDamageAgainstMarkedTarget = 0.5 × RangerCurrentAGI` on top of Aimed Shot's normal raw shot damage. This extra raw damage can use the **same action-wide Bow Mastery crit result** as Aimed Shot (1.4× if crit triggers), after which the Hunter's Mark damage increase is applied to the qualifying Ranger attack. **The exact AGI coefficient and stacking order are NOT user-approved.**
- Keep one target per Ranger's Hunter's Mark; the general Hunter's Mark damage multiplier still applies to Aimed Shot whether or not any extra marked-damage bonus is active (Aimed Shot must also be useful on an unmarked enemy).
- **OPEN:** exact attack coefficient, cast/windup duration and cancellation behavior during player movement, bonus scale per Aimed Shot rank, damage splitting and latency/cancel forgiveness.

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
- **RANGER-015 CONFIRMED:** **Bow Mastery adds AGI, a small basic-attack range increase per skill rank, and +1 percentage point crit chance per rank for eligible attacks dealing 1.4× raw damage on crit**. **HISTORICAL RANGER-015:** eligibility, exact range increment and max ranks were still open at this round; **RANGER-019 later confirms basic attacks + Aimed Shot ONLY, +0.1m/rank**, while the designer-proposed Bow Mastery rank cap 8 awaits user approval. Crit uses explicit class skill rule, not generic global chance/multiplier.
- **RECONCILIATION:** the earlier Ranger Round 2 plan had the same **class base range for longbows and shortbows**, and floated distinct base ranges for possible future Tier III Beastmaster/Marksman. Round 3 adds range growth via Bow Mastery and **DOES NOT** make shortbows longer/shorter than longbows for the same Tier II class/rank. Earlier proposed Bow Mastery flat bow damage/attack speed is **superseded by the AGI/range/crit set**.

## Ranger Round 4 — five user answers recorded 2026-10-19

- **RANGER-016 — CONFIRMED:** Exploding Arrow hits one hostile primary target then **Gun-element AoE explosion around that hit target**. **Primary target takes MORE damage** than surrounding enemies. No Fire/Tech substitution; DiceFree Gun element already exists. Working combat channel is **Physical Gun**, and direct-hit-plus-splash packet design is provisional.
- **RANGER-017 — CONFIRMED:** Aimed Shot against Hunter's Mark gains **additional FLAT damage scaling with AGI**, on top of normal Hunter's Mark bonus, **NOT** the originally suggested +20% extra damage multiplier. Exact flat AGI coefficient and rank scale open.
- **RANGER-018 — CONFIRMED:** Evasive Step **3–4m cursor-directed leap**, **3 seconds +10% Movement Speed** and **+1 percentage point evasion per Evasive Step rank**, where evasion works **against enemy basic/autoattacks ONLY**, not against skill/spell/AoE attacks.
- **RANGER-019 — CONFIRMED:** Bow Mastery **+1 percentage point crit chance per rank at 1.4×** applies **ONLY to basic attacks and Aimed Shot**, not all Ranger damage abilities. **+0.1m basic attack range per Bow Mastery rank**; AGI bonus per rank still to tune.
- **RANGER-020 — DESIGNER DELEGATION:** user approves **designing** weighted Ranger max-rank caps and exact rank gates using the prior Tier II classes as precedent. Designer proposes **Aimed Shot20 / Exploding Arrow18 / Arrow Rain16 / Hawk Dive10 / Evasive Step8 / Bow Mastery8 = 80**, with all rank gates and validation shown below. This is a **proposed first playtest schedule**, NOT explicitly user-approved exact numbers and NOT implemented.

## Ranger Round 4 — designer-proposed rank caps and class-level gates (NOT YET APPROVED)

### Proposed weighted 80-rank budget

All Tier II Ranger skills start at **rank 0**. The new class manifestation starts at **level 1 with one spendable point** and +1 class skill point every level, capped by authored total, following `docs/CLASS_PROGRESSION.md`:
`availableBudget = min(classLevel, 80)`; `unspent = max(0, availableBudget − purchasedClassRanks)`. No free starting skills. One character can progress beyond class level 80 and level 60 advancement threshold; XP/stats continue without phantom overflow points.

| Skill | Proposed rank cap | First eligible Ranger level | Level gap between purchases | Last eligible level | Complete rank gate levels |
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
- **Arrow Rain16:** first rank at **class level 12**, every **4 levels** through level 72. New monsters entering ground zone are hittable by wave ticks. Damage grows by rank; duration/radius should not increase indefinitely.
- **Hawk Dive10:** available from class level **1**, every **7 levels** through level 64. Rank1 mark10% for8s at15s CD; proposed rank growth `markBonusPct = min(23.5%, 10% + 1.5 percentage points × (rank−1))`; this approaches the earlier user-agreed ~20–25% upper playtest direction without unlimited multiplicative scaling. This **exact high-rank formula is designer proposal**, not a newly chosen user numeric.
- **Evasive Step8:** first class level **1**, then every **10 levels** through level71; rank1 +1pp basic-attack evasion, rank8 +8pp, +10% movement speed for3s and cursor leap3–4m always. No indefinite evasion uptime nor rank-scaled leap distance without approval.
- **Bow Mastery8:** first class level **1**, then every **11 levels** through level78. Rank1 +0.1m basic range/+1pp crit at1.4× (basic and Aimed Shot), rank8 +0.8m/+8pp. Designer suggests **+1 AGI per rank** as starting tuning, but **user has not approved exact AGI coefficient**.

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

**Designer-computed verification PASSED for ALL integer levels 1–80**: no level violates `eligibleCount(L) >= L`. All **80** purchasable ranks unlocked by level **78**; player earns the 80th class-skill point at level **80**. As with Greatblade/Mage/Defender, banking remains optional. The HUD upgrade-ready indicator must show only when a next rank is actually purchasable now, while the menu can quietly display banked count and future rank's next eligible level. No Unity balance/code/save implementation in this documentation run.

### Balance/implementation cautions

- **At rank8 Bow Mastery:** total additive **+8 percentage points** crit chance for eligible actions, **1.4×** multiplier, **+0.8m Ranger basic attack range**, possibly +8 AGI if the provisional +1 AGI/rank is later approved. Bow range from mastery applies to either bow family; Tier III base-range changes may be class-specific.
- **At rank8 Evasive Step:** +8pp **BASIC-AUTOATTACK-ONLY** evasion after leap, 3s +10% movespeed. This is an authored miss/avoid rule (generic Agility evasion remains uncommitted) and must not make the skill a blanket dodge of bosses' active magic, physical spells or AoE.
- **At rank10 Hawk Dive:** provisional Hunter's Mark bonus **23.5%** for8s at15s cooldown, capped at one marked target per Ranger. Exact mark high rank coefficient may change during playtests.
- **Exploding Arrow:** Physical channel + Gun element distinct; both direct-hit Gun and AoE splash should avoid duplicate splash packet emission per enemy. Bow Mastery critical permission explicitly excludes Exploding Arrow, Arrow Rain and Hawk Dive.
- **Aimed Shot:** base Physical damage and flat AGI mark synergy get one action-wide Bow Mastery crit result if eligible; use existing highest-multiplier-first rule when other independent crit sources exist, and do not silently give Gun AoE or ground DoTs this passive crit.
- **NOTE:** rank caps/gates here are designer-selected because user requested us to create them; only the six skills and explicit Round 4 numbers are USER-CONFIRMED so far. Keep first-rank balances/schedules marked **proposed** until user reviews.

## Ranger Round 5 — five questions (awaiting user)

**RANGER-021 — Six-skill rank progression:** I propose **Aimed Shot20, Exploding Arrow18, Arrow Rain16, Hawk Dive10, Evasive Step8, Bow Mastery8 = 80 total ranks**. Unlock: level1 Aimed/Hawk/Evasive/Mastery, level6 Exploding Arrow, level12 Arrow Rain; individual rank gaps and full checked gate table above. Shall we approve these as Ranger's initial playtest schedule?

**RANGER-022 — Exploding Arrow splash details:** Does a **Physical Gun direct hit + a Physical Gun explosion centered on target** work, where the primary receives direct + splash and secondary targets receive only one splash packet? Initial **2.5m blast radius**, modest splash (~55% of basic arrow damage) and larger primary total damage; no Fire/Tech packets. Should the explosion still happen if the projectile misses the target?

**RANGER-023 — Aimed Shot flat AGI coefficient:** For the **extra marked-target flat bonus**, should we start with **+0.5 × current AGI raw damage**, on top of normal Aimed Shot and Hunter's Mark, then tune? Should that extra damage itself grow with Aimed Shot skill rank, or remain simply AGI-based while the base shot scales with rank?

**RANGER-024 — Bow Mastery AGI and scope:** For this eight-rank passive, I suggest **+1 AGI per rank**, **+0.1m basic attack range/rank**, and the confirmed **+1pp crit chance/rank at 1.4×** on **basic attacks and Aimed Shot only**. Does that AGI rate work, and should range growth stay restricted to basic attacks rather than increase every Ranger ability's cast range?

**RANGER-025 — Ability and control balancing:** Would you like Evasive Step's **3–4m mouse leap and 3s +10% movement / basic-only evasion** to be usable **while rooted or stunned**? My recommendation: **NO**, standard root/stun restrictions apply; movement interrupts an unfired basic attack. Should the Arrow Rain ground zone pulse about **three waves over 1–2 seconds**, or be a longer-lasting AoE field?

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
| 2026-10-19 | RANGER-021–025 | Ranger Round 5 five questions | **AWAITING USER** |

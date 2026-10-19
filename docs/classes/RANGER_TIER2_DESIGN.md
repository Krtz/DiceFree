# DiceFree — Tier II Ranger — design interview

**Created:** 2026-10-19  
**Status:** RANGER ROUNDS 1–3 RECORDED — DESIGN ONLY, NOT IMPLEMENTED. Round 3 approves Hawk Dive/mark timing, marked-target Aimed Shot synergy, explosive-arrow replacement for Multishot, moving dodge/evasion and range/crit-based Bow Mastery. This is documentation-only; user will ask a separate DiceFree working chat to read all latest specifications and implement after remote desktop access returns. Do NOT change Unity C#, assets, builds or saves here.  
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

1. **Aimed Shot — confirmed extra mark synergy:** heavy **single-target Physical shot** with short stationary windup; target afflicted with the **caster Ranger's Hunter's Mark** takes the ordinary mark damage bonus **AND an additional Aimed Shot-specific marked-target bonus**. Exact additional damage, shot coefficient and windup time still OPEN.
2. **Exploding Arrow / Gunpowder Arrow — confirmed replacement concept, NAME NOT SET:** fires a **gunpowder/explosive arrow** rather than a fan of arrows, as a thematic hint at potential later gun-based classes. **Damage channel/element, delivery (detonate on impact or ground), AoE radius, number of targets, Mana cost, cooldown and secondary effects not yet chosen.** Explosion visual flavor alone does NOT automatically confer Gun/Tech/Fire element, and must use the authored elements framework rather than inventing one. Do not retain Multishot as an additional Tier II ability.
3. **Arrow Rain — confirmed distinct ground AoE:** ground-targeted arrows falling in **repeated waves over a short time**, affecting enemies entering the area after casting, rather than a one-time snapshot of enemy roster. This succeeds Tier I's Arrow Rain *thematically*, but starts at **Ranger rank 0**, does not inherit the Tier I skill points, and gets its own skill scaling.
4. **Hawk Dive — confirmed mark opener:** one-off **single-target Physical attack** featuring a descending hawk that strikes and disappears (not a persistent actor). **Initial approved playtest:** **15-second cooldown**, Hunter's Mark **8-second duration**, +**10% damage from this Ranger's attacks/skills** to that target; higher ranks may grow toward a designer provisional maximum around **20–25%**, not a hard final cap unless user approves its exact limit. Only **one active target per casting Ranger**.
5. **Evasive Step — confirmed directional mobility:** **short leap TOWARD MOUSE POINTER**, not necessarily away from the enemy, followed by a **brief increased Movement Speed buff** and a temporary **+1% EVASION PER EVASIVE STEP SKILL RANK** (e.g. rank 5 = +5 percentage points) lasting a few seconds. Buff does NOT grant basic attacks while moving unless a later explicit skill says so. Leap direction/travel uses actual obstacle-safe movement, not a teleport through walls.
6. **Bow Mastery — confirmed AGI + range + custom crit rule:** passive **raises AGI**, very slightly increases **Ranger effective normal attack range per skill rank**, and grants **+1 percentage point critical chance PER BOW MASTERY RANK** with a **1.4× raw damage multiplier on successful critical hits**. This is an **explicit class passive crit permission/rule**, not a new global crit baseline. Applicability to **autoattacks only vs Ranger damage skills too** is OPEN and should be decided in the next round. Exact AGI gain, meters per rank, rank cap and crit interactions require balancing. No universal bow-damage or attack-speed bonus implied by this latest user description.

#### Hunter's Mark — RANGER-008/011/012 confirmed

- A Hawk Dive hit applies **Hunter's Mark** and deals its own single-target Physical strike; hawk immediately leaves without pet AI.
- **Start-of-playtest numbers confirmed:** Hawk Dive **15-second cooldown**; Hunter's Mark lasts **8 seconds** and causes the marked enemy to take **10% additional damage from that Ranger's basic attacks and skills**. Higher-rank mark damage growth toward **~20–25%** remains a tunable guideline, not a locked formula/cap.
- Only **ONE marked enemy per Ranger at once**, enforced independently of cooldown; replacing mark on a new enemy removes that Ranger's earlier mark. Do not accidentally prevent marks from different Rangers unless user chooses global uniqueness later.
- **Aimed Shot** adds a SECOND, ability-specific marked-target damage bonus **on top of Hunter's Mark's general bonus**; how the two boosts stack (add/multiply), and extra percentage, remain OPEN.
- **OPEN:** mark refresh/removal ordering, crit/proc interaction, target death and dispel behavior, mark from different Rangers, ranks and VFX/status indicator. Only caster Ranger gains mark bonus by default.

#### Evasive Step — confirmed input and evasion ranks

- **On skill activation, leap briefly toward mouse pointer** through normal collision-safe movement. This is an **ability leap**, not free shooting while moving or an unlimited instant teleport.
- Upon completing the leap, apply **temporary increased movement speed** and **evasion chance +1 percentage point × purchased Evasive Step rank** for **a few seconds**. Evasion means the existing attack-miss/avoidance system's mechanic as applicable, not shield absorption or flat Defense.
- **PROVISIONAL:** ~3-second buff duration, ~3–4m max leap reach, +8–12% Movement Speed, ~12–16s cooldown; user has NOT approved these numerical coefficients. Need confirm whether evasion affects physical autoattacks only, projectile skills and/or Magic/Pure attacks.
- Movement during an ordinary autoattack windup already cancels the unfired arrow; Evasive Step must respect that. The leap must not bypass body collision/walls or unintentionally apply auto-attack-while-moving mechanics.

#### Bow Mastery — skill-rank-based class range and explicit crit

- **Confirmed:** **AGI increase per skill rank** (exact flat AGI unknown); **small Ranger-range increase per Bow Mastery rank** on top of class base (exact meter/rank unknown), applies **equally to BOTH shortbows and longbows on the same Ranger**; **+1 percentage point crit chance per Bow Mastery rank** on eligible effects, with a **1.4× raw-damage critical multiplier**.
- Because DiceFree has **NO universal crit chance or multiplier** and uses **explicit opt-in crit sources** (`docs/STATS_AND_DAMAGE.md` and `docs/COMBAT.md`), Bow Mastery must author its own chance/rule and specify whether basic attacks, Ranger skills, physical arrow attacks or all ability damage can use it. **Do not assume all projectile/DoT skills crit by default**. If other explicit crit rules are present, use the existing highest-multiplier-first rule; don't silently stack/merge unrelated crit sources.
- **Unresolved** rank cap (the user described a per-rank effect, not infinite levels), flat AGI/rank, exact +meters per rank, and whether class buffs/skill ranks alter skill target range versus autoattack reach. Potential Tier III class range differences remain possible even though Tier II Bow Mastery boosts its normal class range.

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
- **RANGER-007 CONFIRMED:** user likes the initial **six skills** Aimed Shot, Multishot/Rapid Volley, Arrow Rain, Hawk Dive, Evasive Step, Bow Mastery, subject to next-round mechanical design and final balancing.
- **RANGER-008 CONFIRMED:** Hawk Dive = **single-target Physical hit**, hawk appears only for strike, applies **Hunter's Mark**, temporarily amplifying **Ranger's own attacks AND skills** against that target. **One marked target at a time per Ranger**. The user noted that duration shorter than CD could achieve uniqueness, but explicitly enforcing one active mark is robust; duration, cooldown, damage bonus and mark-refresh specifics remain open.
- **RANGER-009 CONFIRMED:** player movement **cancels Ranger autoattack WINDUP before projectile release**, WC3/Dota/LoL rhythm. Once an arrow has fired, subsequent movement does not cancel it.
- **RANGER-010 CONFIRMED:** only **post-acceptance** Alpha Wolf kill is credited, with **return-to-wounded-Ranger** turn-in; use the existing Alpha Wolf as quest target rather than adding a separate quest-only boss, when confirmed present in game.

## Ranger Round 3 — five answers recorded 2026-10-19

- **RANGER-011 CONFIRMED:** Hawk Dive cooldown **15s**, Hunter's Mark duration **8s**, initially **+10%** own attacks/skills damage; higher ranks may raise toward ~20–25% (provisional top-end tuning, not exact locked final cap). One marked target per Ranger.
- **RANGER-012 CONFIRMED:** Aimed Shot deals **additional damage against Hunter's Mark** target above and beyond the normal mark amplification, in a short stationary-windup heavy physical shot. Exact extra bonus is OPEN.
- **RANGER-013 CONFIRMED ROSTER CHANGE:** **REPLACE Multishot** in Tier II six skills with **Exploding Arrow / Gunpowder Arrow** (working names), a gunpowder-themed attack to foreshadow later gun class fantasy. **Arrow Rain** remains a **ground-targeted attack dealing multiple waves across time, including enemies entering after cast**. Actual explosion damage element/size/procs undecided.
- **RANGER-014 CONFIRMED:** **Evasive Step leaps toward mouse pointer**, then grants a **few seconds of extra Movement Speed** and **+1% evasion chance PER Evasive Step skill rank**. No normal moving basic-attack permission.
- **RANGER-015 CONFIRMED:** **Bow Mastery adds AGI, a small basic-attack range increase per skill rank, and +1 percentage point crit chance per rank for eligible attacks dealing 1.4× raw damage on crit**. Which attacks and skills can use the crit rule, exact AGI and range progression and max rank remain OPEN. Crit uses explicit class skill rule, not generic global chance/multiplier.
- **RECONCILIATION:** the earlier Ranger Round 2 plan had the same **class base range for longbows and shortbows**, and floated distinct base ranges for possible future Tier III Beastmaster/Marksman. Round 3 adds range growth via Bow Mastery and **DOES NOT** make shortbows longer/shorter than longbows for the same Tier II class/rank. Earlier proposed Bow Mastery flat bow damage/attack speed is **superseded by the AGI/range/crit set**.

## Ranger Round 4 — five questions (awaiting user)

**RANGER-016 — Exploding Arrow mechanics:** Should **Exploding Arrow** strike a single target with an arrow and then **explode in a small radius** around them, dealing **Physical damage to nearby enemies**? I'd propose this as a straightforward gunpowder blast, with explosion *visuals* not automatically creating a new element. Or should we explicitly assign the **Gun** or **Tech** element to the explosion?

**RANGER-017 — Aimed Shot bonus:** Hawk Dive marks for 8s with +10% general Ranger damage. On top of that, should **Aimed Shot** against a marked target gain a further **+20% of Aimed Shot's damage** initially (possibly higher by rank), or do you prefer a flat damage addition that scales with AGI?

**RANGER-018 — Evasive Step duration and mobility:** I suggest a **3–4 meter cursor-directed leap**, followed by **3 seconds of +10% Movement Speed** and **+1 percentage point evasion per Evasive Step rank**. Do those starting mobility values sound right? Should the evasion affect *all dodgeable attacks*, or **Physical only**?

**RANGER-019 — Bow Mastery crit scope and range:** Should the passive **+1% crit per rank at 1.4× damage** apply to **basic attacks and Ranger damage abilities**, including Aimed Shot, Hawk Dive, Arrow Rain and Exploding Arrow (subject to authored crit eligibility)? For the subtle range bonus, is **+0.1 meter basic-attack range per rank** a reasonable playtest starting point? AGI gain per rank still needs balancing.

**RANGER-020 — Advancing rank progression:** Can I choose the full **six-skill max rank and individual unlock schedules** for Ranger, targeting **about 80 purchasable ranks by class level 80** with at least one legal skill upgrade each level, just like the other Tier II classes? We'd then review your preferred progression pacing and high-rank side-effect caps.

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
| 2026-10-19 | RANGER-007 | Aimed Shot, Multishot, Arrow Rain, Hawk Dive, Evasive Step, Bow Mastery starting six-skill roster | **APPROVED core; exact mechanics open** |
| 2026-10-19 | RANGER-008 | Hawk Dive single-target Physical plus Hunter's Mark, one target per Ranger, +damage from own attacks and skills | **CONFIRMED; % and uptime open** |
| 2026-10-19 | RANGER-009 | Movement interrupts pre-release autoattack windup; already launched arrows still resolve | **CONFIRMED** |
| 2026-10-19 | RANGER-010 | Quest must be accepted before existing Alpha Wolf kill, then return to wounded Ranger | **CONFIRMED** |
| 2026-10-19 | RANGER-011 | Hawk Dive 15s CD, mark 8s, +10% own damage; later toward 20–25% | **CONFIRMED first-rank tuning; high rank provisional** |
| 2026-10-19 | RANGER-012 | Aimed Shot gets additional marked-target damage above normal Hunter's Mark bonus | **CONFIRMED concept** |
| 2026-10-19 | RANGER-013 | Replace Multishot with gunpowder/Exploding Arrow; retain ground multiwave Arrow Rain | **CONFIRMED core, explosion details open** |
| 2026-10-19 | RANGER-014 | Evasive Step mouse-directed leap, temporary Movement Speed buff and +1% evasion per skill rank | **CONFIRMED core** |
| 2026-10-19 | RANGER-015 | Bow Mastery AGI + tiny range per rank + 1% crit chance per rank at 1.4× damage | **CONFIRMED; exact values/scope open** |
| 2026-10-19 | RANGER-016–020 | Ranger Round 4 questions | **AWAITING USER** |

# DiceFree — Tier II Ranger — design interview

**Created:** 2026-10-19  
**Status:** RANGER ROUNDS 1–2 RECORDED — DESIGN ONLY, NOT IMPLEMENTED. Round 2 establishes class-defined bow range, initial six-skill roster, Hawk Dive single-target Hunter's Mark, shoot-windup cancellation and normal quest credit. This is documentation-only; user will ask a separate DiceFree working chat to read all latest specifications and implement after remote desktop access returns. Do NOT change Unity C#, assets, builds or saves here.  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II original four classes](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Defender Tier II interview](DEFENDER_TIER2_DESIGN.md), [Stats and Damage](../STATS_AND_DAMAGE.md).

## Existing confirmed foundations (before Ranger Round 1)

- **CONFIRMED RANGER-005:** Ranger advances from **Physically Blessed Novice at Tier I level 30**, to its own **Tier II level-1 child manifestation** while preserving parent. A **wounded Ranger NPC near the Priest in Trimery** offers a **simple kill Alpha Wolf quest**. Returning to the wounded Ranger after defeating the Alpha Wolf completes the advancement (turn-in as normal class-fork flow). Details such as quest kill credit, boss level/respawn and exact NPC position remain open; no complex tracking or precision trial.
- `docs/CLASS_PROGRESSION.md` specifically identifies **Ranger → Wood Elf** as an agreed Tier II body/species identity. The exact equipment silhouette, sex, costume, character model and animations are not yet specified by this interview.
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md` contains **Arrow Rain**, an earlier **spectral arrow AoE** that *previews* physical ranged play without requiring a bow. Ranger's later Arrow Rain may be upgraded, replaced or merely inspired by this, but is **NOT** automatically inherited as a free Ranger rank/skill.
- The project has **physical ranged DPS** as a different archetype from Greatblade (two-handed physical damage), Defender (physical tank), Mage (elemental magical damage) and Priest (healer/support). Ranger's mechanics must permit normal solo questing and party boss mechanics.
- **Shared skill progression**: new class ranks start at 0, one spendable class skill point at class level 1, skill-specific weighted caps, bounded spendable budget, no pointless skill point reminders, and **aim for ~78–80 total purchasable skill ranks by class level 80 with a feasible rank purchase each level**. We will design Ranger's exact kit before assigning caps and gates.
- **CONFIRMED RANGER-002:** basic ranged attacks work like WC3/Dota/League: ranger **briefly plants/stops to execute each autoattack**, can reposition and kite between attacks, then stops again for the next shot. **Do not allow baseline shooting while moving**. A future skill, temporary buff or later class might permit movement attacks; none is approved yet. Standard global skill targeting option (**Do Nothing / Move Into Range**) applies.
- **NEW RANGER-006:** the normal attack range is a **class/ability-authored property**, not automatically different because of bow item family. Both shortbows and longbows have the **same Tier II Ranger basic-attack range**. The user suggested possible Tier III **Beastmaster Ranger (shorter class-range, shortbow-oriented)** and **Marksman Ranger (longer class-range, longbow-oriented)**; these are concept directions, NOT approved branches/requirements, and applicable weapons may be shared between specializations.

## Ranger Round 1 — confirmed foundations and still-open balances

| Topic | User-confirmed Ranger rule | What remains open |
| --- | --- | --- |
| Bow types & stat | **Wood Elf AGI-primary PHYSICAL ranged class** using both two-handed **shortbows** (lower damage, faster attacks) and **longbows** (higher damage, slower attacks). **RANGER-006: same Tier II basic attack range regardless of bow type**, because attack reach comes primarily from the class/ability rather than being determined by bow item family. **~25% harder-hit and ~20% slower** for comparable longbows is the agreed initial *item design guideline*, not an automatic universal equipped modifier. | Actual Ranger class attack range, specific item stat curves, weapon/ability scaling, animation timing, crit behavior |
| Offhand/quiver | **Do not add equippable quivers/offhand quiver slot for ordinary Ranger**; bows themselves are two-handed. Later **Tier III/IV dual one-handed crossbows or dual pistols/Gunslinger** are speculative alternatives, not Ranger kit and not approved classes. | Optional cosmetic back quiver visual can be designed later; NOT an equippable offhand or required resource |
| Autoattacking | **Stop briefly to fire**, WC3/Dota/LoL style. **RANGER-009 CONFIRMED:** a movement command before projectile release **CANCELS** the active autoattack windup (no shot), and movement takes priority. Once an arrow is already released, it travels/resolves normally, rather than disappearing because the Ranger starts moving. | Actual windup fraction, post-fire recovery/attack cadence, cancellation/resource side effects, class reach |
| Resource | **Mana only** for class active skills, no added Focus/Precision/Stamina/Tempo bars at Tier II. Existing base autoattacks do not need to cost Mana unless separately agreed. | Mana pool/regen, active skill costs and cooldowns |
| Combat pets | **No permanent companion or pet AI.** **RANGER-008 CONFIRMED:** **Hawk Dive single-target Physical damage** with a hawk appearing, striking once, applying **Hunter's Mark** and leaving. Marked enemy takes bonus damage from **that Ranger's own basic attacks and skills** for a duration, and **only one target may be marked by that Ranger at once**. | Mark percent, duration, CD, rank growth, replacement/refresh timing, interaction with marks from DIFFERENT Rangers |
| Advancement | **Wounded Ranger near Priest in Trimery** offers simple **kill Alpha Wolf** quest. **RANGER-010 CONFIRMED:** wolf kill must happen **AFTER accepting the quest** to count, return to NPC for advancement; target **existing Alpha Wolf**, not a new quest-only copy. | Alpha Wolf's existing spawn/location still requires live-game verification; NPC exact placement, dialogue and multiplayer kill-credit rules |

### Two-handed bow equipment and animations (implementation notes, NOT YET CODE)

- Bow equipped in mainhand and uses both hand slots; equipping it unequips incompatible offhand items through existing two-handed gear semantics, not a Ranger-specific quiver loophole.
- **Shortbow versus longbow** damage and attack timing are authored in **item stats**, with an **approximate +25% damage per longbow shot and ~20% slower attack speed** compared with otherwise comparable shortbows as an initial balancing heuristic, not a forced coefficient. **Both share Tier II Ranger's class-defined basic attack range.** Bow stats matter: damage, speed, bonuses, intrinsics and other parameters still make each bow more than a cosmetic stick. Different tiers/classes may author different effective ranges without forcing a particular item to change its range in every class.
- **No equippable quiver required**. An on-back visual quiver can be a cosmetic prop or animation aid in the future without occupying a second gear slot or granting stats, if art warrants it.
- **RANGER-009:** basic autoattack needs a short stationary shot **windup**. **On player movement DURING the windup, cancel the arrow before release immediately**, prioritize movement (stutter-step/orb-walk style). After the shot has fired, the projectile travels to completion as normal. Don't let starting movement retroactively undo a fired arrow. Exact cancel buffer/attack cooldown reset is tuning; no baseline shots while moving.
- Tier III/IV dual crossbow or dual pistol Gunslinger is **future class brainstorming only**; ordinary Ranger cannot dual wield and no such class is currently approved.

### Ranger initial six-skill kit — USER APPROVED AS A STARTING ROSTER (RANGER-007)

The user **likes all six** starting concepts below. Exact skill names (particularly Rapid Volley/Multishot, Evasive Step/Disengage and Bow Mastery/Marksmanship), mechanics, damage coefficients, cooldowns, Mana costs and first-unlock/rank schedules remain under design. These start **rank 0** as separate Tier II skills, with no inherited free Tier I Arrow Rain ranks.

1. **Aimed Shot** — focused, heavy Physical single-target projectile, potentially a brief standing aim window. Should remain worthwhile with both longbows and shortbows; no mark synergy numeric approved yet.
2. **Multishot / Rapid Volley** — fast ranged AoE or arrow fan/burst striking a group, distinguished from ground-targeted Arrow Rain. Specific area pattern or number of hits undecided.
3. **Arrow Rain** — targeted ranged ground AoE, thematic successor to Tier I's spectral Arrow Rain, but **different Ranger class rank state**. Area, duration and timing open.
4. **Hawk Dive** — **CONFIRMED single-target Physical strike** with brief hawk VFX/attack animation, followed by the hawk leaving immediately, applying **Hunter's Mark** to that target; see exact identity below.
5. **Evasive Step / Disengage** — positional/reposition tool for creating space, no default autoattack-while-moving permission.
6. **Bow Mastery / Marksmanship** — passive enhancing bow use broadly, supporting both shortbow and longbow rather than automatically forcing one type.

#### Hunter's Mark — RANGER-008, core confirmed

- Hawk Dive **deals Physical single-target damage** and applies **Hunter's Mark** to the struck enemy.
- Mark is a **target debuff attributable to the casting Ranger**: that same Ranger's **normal autoattacks and active skills** deal **increased damage to that marked target** for a limited time. No effect on unrelated targets; **allies' damage is not implicitly buffed** unless later approved.
- **Only ONE target per Ranger can have that Ranger's Hunter's Mark at once.** If the Ranger somehow reapplies it to another enemy, remove/replace the former mark rather than allow two. **A short duration versus longer cooldown may support the feel but is not a substitute for explicit uniqueness enforcement**, especially with cooldown reduction/future reset effects.
- **OPEN:** Hawk Dive cooldown, Hunter's Mark duration, bonus-damage percentage, rank growth, whether mark consumption/refresh happens on hit or skill activation, exact projectile mechanics, stacking/interaction with different Rangers' distinct marks, debuff dispel and target death removal.
- Do **NOT** turn the hawk into a persistent pet or additional combatant with its own target, health, navigation, pet frame or independent AI.

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

## Ranger Round 3 — five questions (awaiting user)

**RANGER-011 — Hunter's Mark strength and uptime:** I'd propose Hawk Dive at **15-second cooldown** applying **Hunter's Mark for 8 seconds**, starting at **+10% damage** dealt by that Ranger's attacks AND abilities to the marked enemy. At higher ranks, perhaps increase the mark bonus toward a sensible ~20–25% maximum, without granting permanent 100% uptime. Does that fit, or would you prefer a shorter/stronger damage window?

**RANGER-012 — Aimed Shot interaction:** Should Aimed Shot have a brief **windup while standing still** and deal heavy single-target Physical damage, with an **additional bonus versus Hunter's Mark targets**, or simply rely on the mark's standard damage bonus like every other skill? We should avoid making Hawk Dive mandatory before every single shot if you prefer flexible rotations.

**RANGER-013 — Multishot versus Arrow Rain:** To keep both AoE skills distinct, should **Multishot** fire an immediate **frontal cone/fan of arrows** (fast burst), while **Arrow Rain** targets the ground and deals **repeated damage over a short duration**? Would you prefer each Arrow Rain wave to be able to hit enemies that enter after the cast, rather than snapshotting targets?

**RANGER-014 — Evasive Step:** Should Evasive Step be a **short backwards dodge/leap away from the currently targeted enemy** (great for kiting), or a **directional dodge toward the cursor/movement direction** (more flexible, better for dodging telegraphed boss mechanics)? I favor directional with a fairly short travel and normal obstacle collision.

**RANGER-015 — Bow Mastery and range:** Should Bow Mastery mostly grant **AGI, bow attack damage and modest attack speed** at different ranks, with **no extra attack range** at Tier II? That would keep Ranger's class range consistent between both bow types and leave range growth to future Marksman/Beastmaster identities.

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
| 2026-10-19 | RANGER-011–015 | Ranger Round 3 questions | **AWAITING USER** |

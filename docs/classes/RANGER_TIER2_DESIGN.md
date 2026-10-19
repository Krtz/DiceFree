# DiceFree — Tier II Ranger — design interview

**Created:** 2026-10-19  
**Status:** RANGER ROUND 1 USER DECISIONS RECORDED — DESIGN ONLY, NOT YET IMPLEMENTED. This is documentation-only; user will ask a separate DiceFree working chat to read all latest specifications and implement after remote desktop access returns. Do NOT change Unity C#, assets, builds or saves here.  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II original four classes](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Defender Tier II interview](DEFENDER_TIER2_DESIGN.md), [Stats and Damage](../STATS_AND_DAMAGE.md).

## Existing confirmed foundations (before Ranger Round 1)

- **CONFIRMED RANGER-005:** Ranger advances from **Physically Blessed Novice at Tier I level 30**, to its own **Tier II level-1 child manifestation** while preserving parent. A **wounded Ranger NPC near the Priest in Trimery** offers a **simple kill Alpha Wolf quest**. Returning to the wounded Ranger after defeating the Alpha Wolf completes the advancement (turn-in as normal class-fork flow). Details such as quest kill credit, boss level/respawn and exact NPC position remain open; no complex tracking or precision trial.
- `docs/CLASS_PROGRESSION.md` specifically identifies **Ranger → Wood Elf** as an agreed Tier II body/species identity. The exact equipment silhouette, sex, costume, character model and animations are not yet specified by this interview.
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md` contains **Arrow Rain**, an earlier **spectral arrow AoE** that *previews* physical ranged play without requiring a bow. Ranger's later Arrow Rain may be upgraded, replaced or merely inspired by this, but is **NOT** automatically inherited as a free Ranger rank/skill.
- The project has **physical ranged DPS** as a different archetype from Greatblade (two-handed physical damage), Defender (physical tank), Mage (elemental magical damage) and Priest (healer/support). Ranger's mechanics must permit normal solo questing and party boss mechanics.
- **Shared skill progression**: new class ranks start at 0, one spendable class skill point at class level 1, skill-specific weighted caps, bounded spendable budget, no pointless skill point reminders, and **aim for ~78–80 total purchasable skill ranks by class level 80 with a feasible rank purchase each level**. We will design Ranger's exact kit before assigning caps and gates.
- **CONFIRMED RANGER-002:** basic ranged attacks work like WC3/Dota/League: ranger **briefly plants/stops to execute each autoattack**, can reposition and kite between attacks, then stops again for the next shot. **Do not allow baseline shooting while moving**. A future skill, temporary buff or later class might permit movement attacks; none is approved yet. Standard global skill targeting option (**Do Nothing / Move Into Range**) applies.
- Ranger can later specialize into other Tier III archetypes, but no such names or branches are approved yet.

## Ranger Round 1 — confirmed foundations and still-open balances

| Topic | User-confirmed Ranger rule | What remains open |
| --- | --- | --- |
| Bow types & stat | **Wood Elf AGI-primary PHYSICAL ranged class**. **Both shortbows and longbows** usable. **Shortbows: lower damage but higher attack speed**. **Longbows: higher damage but lower attack speed**. Bow automatically occupies **both mainhand/offhand weapon slots**, like a two-handed weapon. | Actual per-weapon damage/speed baseline, animation timings, max attack range, normal crit behavior and derived AGI coefficient |
| Offhand/quiver | **Do not add equippable quivers/offhand quiver slot for ordinary Ranger**; bows themselves are two-handed. Later **Tier III/IV dual one-handed crossbows or dual pistols/Gunslinger** are speculative alternatives, not Ranger kit and not approved classes. | Optional cosmetic back quiver visual can be designed later; NOT an equippable offhand or required resource |
| Autoattacking | **Must briefly stop to fire each basic attack**, similar to Warcraft III, Dota and League; player can move/kite between attacks. Baseline Ranger **cannot attack while moving**. A later temporary skill/buff or advanced class could do so, not yet chosen. | Basic attack windup/recovery, whether moving cancels a windup, attack range, range indicators and animation blending |
| Resource | **Mana only** for class active skills, no added Focus/Precision/Stamina/Tempo bars at Tier II. Existing base autoattacks do not need to cost Mana unless separately agreed. | Mana pool/regen, active skill costs and cooldowns |
| Combat pets | **No permanent combat pet or summon AI system** for Tier II Ranger. A skill like **Hawk Dive**, visually calling a hawk from above for a strike and then leaving, is acceptable and desirable as an animal/pet-themed preview. Future Beastmaster/Hunter classes may eventually have persistent pets; nothing approved yet. | Hawk Dive's mechanics/element/damage, duration and ownership visual; future pets purely possibilities |
| Advancement | A **wounded Ranger near the Priest in Trimery** offers a **simple hunt/kill Alpha Wolf quest**. Eligible level-30 Physically Blessed Novice returns for Ranger advancement. | Exact Alpha Wolf location/credit handling, whether existing Alpha Wolf is used unchanged, NPC/dialogue/gear and failure/respawn behavior |

### Two-handed bow equipment and animations (implementation notes, NOT YET CODE)

- Bow equipped in mainhand and uses both hand slots; equipping it unequips incompatible offhand items through existing two-handed gear semantics, not a Ranger-specific quiver loophole.
- **Shortbow versus longbow** differences should come from actual weapon definitions and attack timing/damage rather than magically changing Ranger class skill identities. Both scale with AGI as primary physical ranged damage stat, using general diminishing/restriction rules where appropriate.
- **No equippable quiver required**. An on-back visual quiver can be a cosmetic prop or animation aid in the future without occupying a second gear slot or granting stats, if art warrants it.
- Basic autoattack uses a short **standing-to-shoot windup**; kiting requires movement between shots. Distinguish basic autoattacks from movement-cast ability exceptions. Respect global attack and target-selection rules.
- Tier III/IV dual crossbow or dual pistol Gunslinger is **future class brainstorming only**; ordinary Ranger cannot dual wield and no such class is currently approved.

### Ranger active ability possibilities — DESIGNER IDEAS, NOT YET USER APPROVED

The user asked for Ranger after Defender and confirmed Hawk Dive thematic direction. No final Ranger ability roster or exact numbers have been approved.

1. **Aimed Shot** — focused physical single-target damage, possibly a deliberate short aiming/windup window rewarding firing while planted; could have utility against marked or distant targets
2. **Rapid Volley / Multishot** — quick series or frontal group of arrows, Ranger's basic ranged AoE option, differentiated from the slower ground-field Arrow Rain
3. **Arrow Rain** — ground-targeted burst of arrows, a thematic successor to the Tier I Physically Blessed Novice's spectral Arrow Rain, but a separate Ranger **rank-0** skill if selected; no inherited free rank
4. **Hawk Dive** — **instant/short action** conjures a hawk to strike a target and depart; **no persistent pet, own hitbox/health/AI/autonomous turns**
5. **Evasive Step / Disengage** — short dodge/repositioning tool to create separation and kite, with no ordinary shot-while-moving permission except an expressly approved future buff
6. **Bow Mastery / Marksmanship** — passive improving both bow types, potentially supporting longbow punch versus shortbow attack frequency, without automatically favoring one so strongly that the other is obsolete

These are **candidates** rather than an enforced six-skill allocation or a promise of a pet, trap, stealth or ultimate ability. The user can replace/add/remove any skill during subsequent rounds. Attribute damage/cooldown/Mana/bow rank interactions require later sign-off.

## Ranger Round 1 — five user answers recorded 2026-10-19

- **RANGER-001 CONFIRMED:** AGI as primary; both **longbows (heavy/slower)** and **shortbows (light/faster)** as **two-handed bows**. Exclude equippable offhand quivers, leaving speculative **dual one-handed crossbows** or **Gunslinger dual pistols** for potential later Tier III/IV options. Do not convert speculation into approved class branches.
- **RANGER-002 CONFIRMED:** Ranger's baseline autoattack requires a **brief stop to fire**, WC3/Dota/League rhythm; possible later skill/buff/class to permit shooting while moving remains OPEN, not baseline.
- **RANGER-003 CONFIRMED:** use **Mana** as Ranger's **only** class resource for now. No Focus, Stamina or other extra bar.
- **RANGER-004 CONFIRMED:** no permanent Ranger pet or summoned companion. A temporary **Hawk Dive** attack in which a hawk appears, strikes, and leaves is favored as future pet fantasy foreshadowing, not a persistent actor; final skill selection/mechanics to design.
- **RANGER-005 CONFIRMED:** **Wounded Ranger NPC near the Priest in Trimery** offers **kill Alpha Wolf** advancement quest to eligible Tier I level-30 Physically Blessed Novice. Keep it simple; do not author an obstacle course/archery trial.

## Ranger Round 2 — five questions (awaiting user)

**RANGER-006 — Bow damage and attack speed:** Should shortbows and longbows have the **same maximum range** and only trade raw attack damage against attack speed? I suggest longbows **~25% more damage per arrow** but **~20% slower** than otherwise equivalent shortbows as a provisional item-tuning starting point, not a fixed universal gear modifier. Would you prefer a longer range for longbows too?

**RANGER-007 — Combat skill roster:** Do you like **Aimed Shot** (heavy single-target), **Rapid Volley/Multishot** (burst group arrows), **Arrow Rain** (ground AoE), **Hawk Dive** (one-off animal strike), **Evasive Step** (short reposition) and **Bow Mastery** (passive) as a starting six-skill kit? What would you replace/add/remove?

**RANGER-008 — Hawk Dive:** Should Hawk Dive be a **physical single-target hit** that briefly calls a hawk from above, or a **small AoE swoop** hitting nearby enemies? Could the hawk apply a brief mark/bleed, or should it be pure damage? In either case it disappears immediately after the attack.

**RANGER-009 — Shooting interruption and movement:** When the player begins moving during the **basic attack's windup**, should movement **cancel the pending arrow immediately** (responsive kiting with missed shot), or **finish firing before moving** (more reliable shot, slower responsiveness)? I favor cancelling the arrow on an early move command; a projectile already released should continue.

**RANGER-010 — Alpha Wolf advancement credit:** Should it count only if the player has **accepted the wounded Ranger's quest BEFORE defeating Alpha Wolf**, like existing DiceFree quest kill-credit rules, with an ordinary return-to-NPC turn-in? Should the existing Alpha Wolf be the objective if one already exists, rather than spawning a separate quest-only wolf?

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
| 2026-10-19 | RANGER-006–010 | Ranger Round 2 five questions | **AWAITING USER** |

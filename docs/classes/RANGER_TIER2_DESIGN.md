# DiceFree — Tier II Ranger — design interview

**Created:** 2026-10-19  
**Status:** INITIAL DESIGN INTERVIEW — NOT YET IMPLEMENTED. This is documentation-only; user will ask a separate DiceFree working chat to read all latest specifications and implement after remote desktop access returns. Do NOT change Unity C#, assets, builds or saves here.  
**Branch:** `integration/world1-foundation`  
**References:** [Class Progression](../CLASS_PROGRESSION.md), [Physically Blessed Novice](PHYSICALLY_BLESSED_NOVICE.md), [Tier II original four classes](../TIER2_CORE_CLASSES_DESIGN_V02.md), [Defender Tier II interview](DEFENDER_TIER2_DESIGN.md), [Stats and Damage](../STATS_AND_DAMAGE.md).

## Existing confirmed foundations (before Ranger Round 1)

- **Ranger is a potential Tier II physical ranged branch of Physically Blessed Novice** (listed under the Tier I level-30 branch candidates). This preserves the established novice→physical/magical fork and the rule that eligible Tier I level-30 manifestations advance to new Tier II level-1 child manifestations while preserving parents. The *exact Ranger quest and final branch graph* are not yet user-approved.
- `docs/CLASS_PROGRESSION.md` specifically identifies **Ranger → Wood Elf** as an agreed Tier II body/species identity. The exact equipment silhouette, sex, costume, character model and animations are not yet specified by this interview.
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md` contains **Arrow Rain**, an earlier **spectral arrow AoE** that *previews* physical ranged play without requiring a bow. Ranger's later Arrow Rain may be upgraded, replaced or merely inspired by this, but is **NOT** automatically inherited as a free Ranger rank/skill.
- The project has **physical ranged DPS** as a different archetype from Greatblade (two-handed physical damage), Defender (physical tank), Mage (elemental magical damage) and Priest (healer/support). Ranger's mechanics must permit normal solo questing and party boss mechanics.
- **Shared skill progression**: new class ranks start at 0, one spendable class skill point at class level 1, skill-specific weighted caps, bounded spendable budget, no pointless skill point reminders, and **aim for ~78–80 total purchasable skill ranks by class level 80 with a feasible rank purchase each level**. We will design Ranger's exact kit before assigning caps and gates.
- **Basic attacks** are integral to DiceFree. Tier II identities can vary action count; don't force a fixed four-active/one-passive split or a Tier II ultimate. Standard general out-of-range setting (**Do Nothing / Move Into Range**) applies to targeted skills.
- Ranger can later specialize into other Tier III archetypes, but no such names or branches are approved yet.

## Ranger identity candidates — for user discussion, not yet approved

- **Weapon fantasy:** longbow/shortbow with visibly oversized stylized fantasy model; AGI as probable damage-stat direction; secondary STR/VIT as balance tuning. More mobile/precise than Greatblade, less protective than Defender.
- **Playstyle directions:** aimed burst shots and target-marking; rapid-fire basic attacks and combo setup; battlefield repositioning/kiting; nature/hunter traps and area denial. These are alternatives to combine selectively, not approved features.
- **Potential resource:** Focus/Precision/Tempo built by successful bow hits and spent on strong marked-target shots, or class-specific Stamina with cooldown abilities; **unselected**.
- **Pet distinction:** a base Ranger may have no permanent companion, leaving pets and Beastmaster fantasy for a later specialized class, OR a modest hawk/wolf scouting pet. Nothing in the existing Ranger foundation requires a pet; **unselected**.
- **Quest concept:** hunter/ranger mentor near Trimery or Ironpine forest tests movement, tracking and precision through a local forest trial. Location, encounter scripts, NPC, reward and branch availability **OPEN**.
- **Appearance:** existing class architecture permits Wood Elf model while preserving Echo class manifestation. Exact sex, skin/hair, gear presentation and initial skills await user answers.

## Ranger Round 1 — five user questions (AWAITING USER)

**RANGER-001 — Combat identity and gear:** Shall Ranger be a **Wood Elf bow specialist**, with AGI-first physical ranged damage, mixing precise single-target shots with some AoE/traps? Do you picture a longbow, shortbow, or both, and should bows occupy both weapon slots?

**RANGER-002 — Shooting while moving:** For DiceFree's click-to-move/WASD controls, should Ranger's basic attacks require **brief planted shots** (stutter-step/kite between attacks), permit **shooting at reduced effectiveness while moving**, or fire normally during movement? This changes its mechanical skill ceiling and encounter balance substantially.

**RANGER-003 — Resource identity:** Should Ranger build a special **Focus/Precision** meter from successful arrows and spend it on powerful shots, use **Stamina-like resources**, or operate primarily via cooldowns and attack-speed/cast rhythms without a new class meter?

**RANGER-004 — Animal companion:** Should the baseline Tier II Ranger have **no persistent combat pet**, keeping beast companions for a possible Tier III **Beastmaster/Hunter**, or should the Ranger already have a modest owl/wolf/hawk companion? Noncombat visual scouting flavor is a third option.

**RANGER-005 — Advancement trial:** Would you like a **Wood Elf ranger mentor near the Ironpine forest** to offer a level-30 Physically Blessed Novice **tracking/precision and survival trial**, distinct from Greatblade's Slime Boss and Defender's Trimery gate defense? Or do you have a different mentor/location/quest fantasy?

## Decision log

| Date | Topic | State |
| --- | --- | --- |
| Existing | Physically Blessed Novice physical ranged Ranger candidate | **Prior documented branch candidate** |
| Existing | Ranger body identity Wood Elf | **Prior confirmed identity** |
| Existing | Physically Blessed Novice Arrow Rain preview | **Historical implemented/working Tier I spell; not a free Ranger skill** |
| 2026-10-19 | User requested Ranger design after Defender | **CONFIRMED intention; design only** |
| 2026-10-19 | RANGER-001–005 | **AWAITING USER** |

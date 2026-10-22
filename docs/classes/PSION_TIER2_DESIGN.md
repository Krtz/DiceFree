# DiceFree — Psion Tier II — design interview

**Status:** **PSION TIER II ROUND 1 ACTIVE — EXACTLY FIVE OPEN QUESTIONS, NO PSION GAMEPLAY MECHANICS APPROVED YET** (2026-10-22). Normal standard eighth/final World1 Tier II class under Magically Touched Novice. Thief Tier II rounds1–8 completed (gameplay design only, NOT implemented). The Psion title/class slot is user-approved; signature weapon, combat style, resources, attributes, spells, presentation, advancement and quests remain OPEN. **DESIGN/DOCUMENTATION ONLY. NO UNITY SOURCE/SCENE/ASSET/BUILD/SAVE CHANGES.**

**References:** [Class Progression](../CLASS_PROGRESSION.md) · [Magically Touched Novice](MAGICALLY_TOUCHED_NOVICE.md) · [Tier II Core Classes](../TIER2_CORE_CLASSES_DESIGN_V02.md) · [Combat](../COMBAT.md) · [Stats and Damage](../STATS_AND_DAMAGE.md) · [Elements](../ELEMENTS.md) · [Completed Acolyte Tier II](ACOLYTE_TIER2_DESIGN.md) · [Completed Thief Tier II](THIEF_TIER2_DESIGN.md) · [Class Requirements and Discovery](../CLASS_REQUIREMENTS_AND_DISCOVERY.md)

## Locked project-wide context — not Psion-specific approvals

- **Normal Tier II standard roster: EIGHT in World1, FOUR per Tier I parent**:
  - Physically Blessed Novice → Greatblade, Defender, Ranger, Thief.
  - **Magically Touched Novice → Mage, Priest, Acolyte, Psion.**
- The **Psion class name and normal Tier II route** are approved; it is NOT a secret class, not a Tier III specialization and not extra ninth normal Tier II.
- Existing Magically Touched Novice is an INT/SPR-leaning broad magical generalist, uses a ranged Magical basic attack and Mana, but **do not silently lock Psion's exact own stats, weapons, abilities or secondary resource from the parent**. Tier II creates its own level1 child manifestation, preserves existing parent progression, grants skill ranks beginning at0.
- Standard Tier II user-preferred initial playtest budgets aim for **~80 purchased rank points by level80, with a legal new rank purchase every level**, weighted skill caps and staged level gates, but **Psion has no selected number of skills, rank caps, unlock gates or cooldowns yet**.
- Existing **Mage** covers elemental wizard-caster attack/utility, **Priest** covers divine/support and healing, **Acolyte** covers curse/Radiation spread, limited skeleton control, Void drain and occult sustain. Psion should have its own distinct play pattern, but **what makes it distinct is for the user to decide**.
- Every damage instance has a Physical or Magical CHANNEL, and zero or one recognized ELEMENT. The registry includes unusual DiceBound elements and explicit DiceFree Nature; however **there is no user-approved 'Psychic' elemental affinity or automatic psychic mind-control mechanic** merely by having a Psion class. Status/debuff, CC/boss Tenacity, hit/miss, positional tactics, PvE threat and party balance must be explicitly authored and remain coherent with shared rules.
- Class skills must function for solo and four-player boss play where many bosses may resist/limit CC. Avoid a class whose whole contribution relies on permanently mind-controlling bosses.
- No Tier III class design until World 2. After Psion gameplay design, plan Quillboar area, then major city. The city might later determine/change specific advancement mentors/quests, so do not invent a Psion mentor or city location without user approval.
- Existing one class appearance/model question should be settled deliberately, potentially in working-chat/Codex depending on existing art inventory. This documentation interview **does not inspect/download or import models**.
- All selected gameplay identities/damage channels/resources/classes must be recorded as **USER CONFIRMED only after explicit approval**, distinct from proposals. No implementation tasks in this interview.

## Initial unapproved brainstorming seeds — OPTIONS ONLY

These directions might fit a Psion, but NONE are presently approved:
- **Telekinetic:** psychic pushes, pulls, levitation, animated objects, barriers, kinetic blasts, potentially timed repositioning. Keep boss movement immunity in mind.
- **Telepath:** thoughts, confusion, hallucinations, fear, control/interference of regular enemies, counterplay against bosses.
- **Gravity or perception manipulator:** delayed AoE, compressing/expanding zones, vulnerability and shielding, perhaps battlefield tempo.
- A distinct magical ranged attack pattern from Mage's direct elemental projectiles, Priest's heal/support and Acolyte's curse/sustain.
- Potential **INT**, **SPR**, or mixed offensive scaling; Mana-only, Mana+Focus or different class mechanic; various catalyst/empty-hand combat silhouettes are **not user approved**.
- These are *possible Tier II class identity options*, not separate Tier III paths. No exact skills, new damage elements or resource UI invented yet.

## Psion Round 1 — EXACTLY FIVE questions (AWAITING USER)

**PSION-001 — Psychic identity:** Should Psion's Tier II fantasy be mainly **telekinesis** (moving/hurling things with the mind), **telepathy** (mental disruption, hallucinations, influence), **gravity/spatial manipulation**, or **a blend**? I'd suggest a blend with a distinctive central mechanic, but not assume which you prefer.

**PSION-002 — Combat role:** Should Psion be primarily a **ranged damage dealer**, a **crowd-control/debuff specialist**, a **shielding/support hybrid**, or something in between? Bosses need meaningful counterplay, so do you want control skills to retain reduced-but-useful effects against elite/boss enemies?

**PSION-003 — Spell damage and elements:** For psychic/telekinetic effects, do you picture **Magical non-elemental**, **Magical Math/Tech** using existing affinities, **Physical kinetic damage moved by magic**, or a **brand-new named Psychic element** (which would need its own registry/resistance identity)? This is a design decision, not something already implemented.

**PSION-004 — Weapon, stats and resource:** Do you see Psion fighting with **empty hands**, a **wand and offhand focus**, a **staff**, or another implement? Should it scale mostly from **INT**, **SPR**, or both, and use **Mana only** or Mana alongside some short-lived Focus/Concentration mechanic?

**PSION-005 — Appearance and advancement:** What should a Psion LOOK like (race/body silhouette, clothing/visual cues), and should we defer choosing a model and advancement mentor/quest until the separate working chat/major-city planning, or design some of those now? We can keep appearance distinct without inventing downloaded art or a specific NPC.

## Decision ledger

| Date | ID | Decision | Status |
| --- | --- | --- | --- |
| 2026-10-19 | PSION-FOUNDATION-ROSTER | Psion is normal EIGHTH World1 Tier II, under Magically Touched Novice | **USER CONFIRMED** |
| 2026-10-22 | PSION-FOUNDATION-SEQUENCE | Thief gameplay design completed → Psion now active → Quillboar area → major city; no TierIII until World2 | **USER CONFIRMED** |
| 2026-10-22 | PSION-001–005 | Five Round1 identity/role/element/weapon+resource/appearance questions | **AWAITING USER** |

**Scope reminder:** no Unity changes, TierIII kit design, mentor quests or art generation from this file. Preserve approved user answers, mark speculative values as proposals, re-fetch after every GitHub update, and ask exactly five new numbered questions each continuing round.

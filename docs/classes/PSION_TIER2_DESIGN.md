# DiceFree — Psion Tier II — design interview

**Status:** **ROUNDS 2 AND 3 ANSWERED (2026-10-24); mechanical decisions recorded below.** Tier II design only; no Unity implementation. Shield shared-vs-independent absorption remains under discussion. Appearance/model and advancement quest deferred; future descendant specializations World2+ only.

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

## Psion Round 1 — ORIGINAL FIVE QUESTIONS (PARTIALLY ANSWERED; USER PAUSED)

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
| 2026-10-22 | PSION-001–003 | Partial answers: mixed autoattack+spell kit, relatively short ranged, elements deferred; detailed latest decisions below | **PARTIALLY USER CONFIRMED** |
| 2026-10-22 | PSION-004–005 | Weapon/resource/appearance/advancement unanswered | **AWAITING USER LATER** |
| 2026-10-22 | PSION-006–010 | Next exact five follow-up questions below | **USER REQUESTED LATER, NOT NOW** |

**Scope reminder:** no Unity changes, TierIII kit design, mentor quests or art generation from this file. Preserve approved user answers, mark speculative values as proposals, re-fetch after every GitHub update, and ask exactly five new numbered questions each continuing round.

## LATEST USER CONCEPTS — PSION ROUND 1 (2026-10-22; AUTHORITATIVE OVER EARLIER BRAINSTORMING)

User had to leave and explicitly requested the questions **again later**. DO NOT mistake partially answered questions for an invitation to quiz right now. Save user-authored gameplay directions and distinguish definitive concepts from maybes.

1. **PSION-001 — blended autoattacks + spellcasting CONFIRMED.** Psion Tier II should combine the two styles. The user envisages **AT LEAST TWO FUTURE descendant classes**, one focused on autoattacks and one focused on spells. Record as **speculative future class path inspirations for World2+ ONLY**; exact tier, class names and branches NOT approved; don't design those paths yet.
2. **Psychic Bolt — user-selected concept:** **single-target MAGICAL damage plus STUN**. User is **thinking METAL for its element**, but later explicitly said element choices NOT YET SET; treat Metal as a **TENTATIVE suggestion, never user-locked**. Stun duration, CC vs bosses/Tenacity, CD, Mana, range and damage formulas OPEN.
3. **Psionic Blades — user-selected passive concept:** **crit on normal/basic AUTOATTACKS and ignoring a FLAT defense value on those attacks**. It is not generic percent defense penetration. The basic attack's damage channel (Physical vs Magical), applicable PDEF/MDEF, crit amount/multiplier, flat points ignored and rank scaling OPEN. This does not grant crit to all skills by default.
4. **Mental Image — user-selected active concept:** **spawn an IMMOBILE/STATIC clone at a selected location**; the clone **AUTOATTACKS enemies within its range for 10% OF THE PSION'S OWN NORMAL BASIC/AUTOATTACK DAMAGE VALUE per attack**; clone **EXPLODES WHEN ITS DURATION ENDS OR IT IS KILLED**. Clone duration, HP, whether targetable (killed implies vulnerable but rules not selected), attack speed, target acquisition, range, maximum concurrent images, explosion damage/radius/channel/element, raw damage scaling, Mana/CD OPEN. Do not substitute roaming pets or full-strength duplicate.
5. **One AoE spell — user-selected core concept.** Exact name, shape, damage/elements, effects, scaling, unlocks OPEN. **Maybe a shield? — ONLY A TENTATIVE ADDITION**, not an agreed sixth active. No total six-skill count/rank caps chosen for Psion.
6. **Hybrid autoattack attribute scaling direction USER CHOSEN: BOTH AGI AND INT contribute**. The proposed idea **"maybe 0.45× normal value from both or something" is a PROVISIONAL numeric illustration**, not a locked formula. Actual AGI+INT coefficients, attack speed, crit, raw channel and how gear contributes require later approval. Do not decide spell scaling from autoattack scaling.
7. **PSION-002 — shorter RANGED CONFIRMED:** Psion is RANGED, but **closer ranged than other standard ranged classes**; not melee, exact meters undecided. User did NOT decide if primary focus is DPS, crowd-control specialist, shielding hybrid beyond the blended attacking style.
8. **PSION-003 — DAMAGE ELEMENT CHOICES EXPLICITLY DEFERRED:** use the existing elemental rules, but NO specific element has been finalized. The tentative Psychic Bolt Metal suggestion is NOT binding. Don't invent a new Psychic element. Bolt's MAGICAL channel is specified, but its element is OPEN.
9. **PSION-004/005 unanswered:** weapon, offhand, Mana-only vs secondary resource, specific spell scaling, character appearance/model and advancement mentor/quest location remain undecided. The user didn't authorize appearance or NPC decisions today.

### Round 2 — EXACTLY FIVE QUESTIONS (DEFERRED, SEND WHEN REMINDED)

**PSION-006 — Psychic Bolt:** Shall its single-target Magical stun bolt use **Metal** as the element, or keep that undecided? How should the stun scale with ranks, especially on bosses using Tenacity?

**PSION-007 — Psionic Blades:** Do normal Psion autoattacks deal Physical or Magical damage? Which matching FLAT defense (PDEF or MDEF) should Psionic Blades bypass, how should basic crit chance grow, and do we start testing the tentative 0.45×AGI+0.45×INT formula?

**PSION-008 — Mental Image:** How long does the immobile clone last; how much HP, what attack range, and can multiple exist? What should its death/expiry explosion do? Keep its user-selected attack damage at 10% of Psion basic value.

**PSION-009 — AoE and optional shield:** What should the AoE look/play like (close pulse, targeted blast or ground field), and shall a SHIELD spell be included? If yes, who can it shield? Skill count/rank budget not fixed.

**PSION-010 — Equipment/resources/appearance:** Empty hands, wand+focus, staff or other equipment? Mana only or a secondary class mechanic? Should appearance selection happen through the separate working chat's asset review and advancement quest wait until the big city?

**Do not design Tier III here. Do not perform any Unity or gameplay implementation. Continue documenting and asking exactly five questions when user resumes.**


## CURRENT AUTHORITATIVE DESIGN — ROUNDS 2–3 (2026-10-24)

This section **supersedes earlier 'unanswered', 'tentative', and 'deferred Round 2' notes above** wherever inconsistent. The earlier interview is retained as history.

| ID | User decision | Status |
| --- | --- | --- |
| PSION-006 | Psychic Bolt deals **Magical Metal** single-target damage and stuns; stun scales by rank to **3 seconds at maximum rank**, reduced by boss Tenacity. Rank count and rank-1 stun still open. | USER CONFIRMED |
| PSION-007 | **All normal autoattacks in DiceFree deal Physical damage**, although abilities/passives may add Magical damage. Psion's Physical basic attacks start at **0.45×AGI + 0.45×INT** (0.9 total), for testing. Passive basic-attack crit chance **+2.5 percentage points per rank**, crit damage **150%** (interpret as total multiplier pending validation). Passive ignores **1 flat Physical Defense per rank**. Rename to **Psionic Blades Mastery** is assistant-preferred but user proposed alternatives and has not explicitly locked the name. | MECHANICS USER CONFIRMED; NAME OPEN |
| PSION-008 | Stationary placed **Mental Image** clone lasts up to **5 seconds at max rank**, duration increases with ranks. Same HP and attack range as caster, but takes **250% MORE damage** (3.5× incoming; interpretation should be verified). Multiple clones may coexist. Clone basic attacks deal **10% of caster basic-attack damage** and **inherit caster critical chance and other basic-attack modifiers**; clone explodes for **AoE damage only** on expiry or death. Explosion magnitude/element/radius, rank count, attack speed and mana/cooldown open. | USER CONFIRMED |
| PSION-009 | AoE spell is a **Lightning-element storm**, **three lightning waves over 0.6 seconds**. A **Metal-element shield** affects Psion and allies in a nearby radius. Whether allies get independent shields or all draw on **one shared absorption pool remains undecided**; assistant recommends one shared pool, applied to nearby allies at cast time, with INT/rank scaling and no pool multiplication per ally. These implementation details are PROPOSALS, not approvals. | CORE USER CONFIRMED; SHIELD POOL OPEN |
| PSION-010 | Tier II equipment: **psionic blades** and **tome offhand**. **Mana only**. Advancement quest and class model **deferred**. | USER CONFIRMED |

**Important interpretation:** 'Psionic blades' denotes equipment; the proposed **Psionic Blades Mastery** denotes the passive. User also floated 'Mental Attacks' or similar names. No final passive rename yet. Preserve universal Physical basic-attack rule. Distinguish spell Magical channel from Lightning/Metal element.

**Round 3 shield discussion:** User felt independent shields would be easier but a shared damage-absorption pool sounded more fun and requested advice. Assistant recommends **shared absorption** for tactical party interaction. Do not record the shared pool as approved until user confirms.

**Still open:** rank caps and unlock levels, skill damage/mana/cooldowns/ranges, shield pool type/values/radius/duration, Mental Image explosion specifics, final passive name, Psion model and quest (explicitly later). No Unity modifications or World2+ descendant implementation.

**Documentation workflow:** User explicitly requests updating design documents after **every design round**. Maintain this file on `integration/world1-foundation` after each round, preserving confirmed-vs-proposed distinctions.

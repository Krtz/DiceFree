# DiceFree — Psion Tier II — design interview

**Status:** **TIER II GAMEPLAY DESIGN COMPLETE, ROUNDS 1–20 (2026-10-26). All 80 exact rank gates user approved. DESIGN ONLY — no Unity implementation or playtest validation.** Historical earlier entries below are superseded by latest rounds. Appearance/model and advancement quest deferred; future descendant specializations World2+ only.

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


## Psion Round 4 — 2026-10-26 (LATEST; supersedes conflicting Round 3 details)

| ID | User decision | Status |
| --- | --- | --- |
| PSION-011 | **Metal Shield** is centered on the Psion with an **8-meter radius**; it grants protection to the Psion and nearby allies. | USER CONFIRMED |
| PSION-012 | All protected allies draw from **one shared absorption pool**, not a separate pool per ally. Pool size, how it handles departing/re-entering players, damage order and stacking/recasting remain OPEN. | USER CONFIRMED pool model; mechanics OPEN |
| PSION-013 | Metal Shield duration is **8 seconds**. | USER CONFIRMED |
| PSION-014 | Replace the formerly selected **three-wave Lightning storm** with a **Psychic Shockwave spreading outward**. Treat the shockwave as the current AoE skill concept; exact damage channel, element, radius, expansion speed, CC and rank behavior OPEN. Do not assume Lightning remains its element. | USER CONFIRMED concept; mechanics OPEN |
| PSION-015 | **Only five abilities/skills are needed for Tier II Psion for now**: Psychic Bolt, Psionic Blades (passive; final name pending), Mental Image, Psychic Shockwave (working name), and Metal Shield. **Do not add a sixth ability**. | USER CONFIRMED |

**Important scope:** Psion remains a hybrid short-ranged AGI/INT basic-attacker and spellcaster with Mana, psionic blades+tome. Previously locked Bolt/clone/basic-attack details remain in force. Model and advancement quest are still deferred. No Tier III design or implementation authorized. Continue in batches of five design questions, and update this document after each round.


## Psion Round 5 — 2026-10-26 (LATEST)

| ID | User decision | Status |
| --- | --- | --- |
| PSION-016 | **Psychic Shockwave originates from the Psion** and expands outward in a circle. Its wave appearance is **visual presentation only**; do not infer knockback, slow, travel-distance damage scaling, or another gameplay effect. | USER CONFIRMED |
| PSION-017 | Psychic Shockwave deals **skill rank × 1 basic-attack damage value** (tentative exact implementation/wording to clarify; user's words: "give skill rank x 1 base attack damage"). Pure damage, **no additional slow/knockback**. Its damage channel and element remain OPEN; don't infer Physical merely from referencing basic-attack damage. | USER CONFIRMED damage-scaling direction; implementation details OPEN |
| PSION-018 | **Metal Shield absorption pool** scales primarily with a **flat shield value per skill rank**, plus a **smaller INT contribution**. Coefficients and pool amounts OPEN. The confirmed 8m AoE still gives everyone one shared pool lasting 8s. | USER CONFIRMED |
| PSION-019 | Metal Shield affects the Psion and allies **within the 8m radius at cast time**; those affected **retain protection after leaving** the radius until the 8s duration ends or the shared pool is depleted. No continuous entry/refresh of protected participants is implied. | USER CONFIRMED |
| PSION-020 | **Mental Image explosion** scales with **AGI + INT**, **plus flat damage per skill rank**. Precise coefficients, channel, element, AoE radius and trigger-time behavior remain OPEN; continue to explode on clone death or expiry. | USER CONFIRMED |

**Scope:** Exactly five Tier II abilities for now, no sixth. Follow-up questions should refine these five, including rank caps/unlocks, ranges, damage channels/elements, cooldowns and clone behavior. Do not implement Unity changes or invent tier III paths or advancement quests.


## Psion Round 6 — 2026-10-26 (latest clarification)

| ID | User answer | Status |
| --- | --- | --- |
| PSION-021 | AoE ability damage channel / element **not answered**. | OPEN |
| PSION-022 | AoE ability maximum radius **not answered**. | OPEN |
| PSION-023 | **Mental Image explosion radius = 4 meters.** | USER CONFIRMED |
| PSION-024 | Recasting **Metal Shield replaces the existing shield with a fresh shared pool**, rather than stacking pools. Exact cross-caster interaction and shield targeting still open. | USER CONFIRMED |
| PSION-025 | Psion should have **unevenly distributed skill ranks** across its five abilities, in the direction of the standard ~80-rank Tier II budget. Individual caps/gates not yet selected. | USER CONFIRMED qualitative distribution; exact budget still pending |

### AoE ability correction/ambiguity — resolve before more mechanical decisions
User restated their intended FIVE-skill kit as **Psychic Bolt, Psionic Blades Mastery, Mental Image, AoE lightning storm (three waves in 0.6s), Metal Shield** and asked **"I don't know where the psychic shockwave comes from?"** The "Psychic Shockwave" naming and outward-expanding pulse came from prior assistant questions and user's Round 4 and 5 selections; the assistant may have wrongly interpreted that as replacing the original lightning storm. **Treat the current AoE slot as DISPUTED, not a sixth ability**. Ask whether the user prefers restoring the three-wave Lightning storm, the expanding shockwave, or a hybrid effect. Until clarified, do NOT describe Psychic Shockwave as the final approved skill, and do NOT remove historical lightning-storm details. The Round 5 rank×basic-attack damage suggestion may or may not be carried to the eventual chosen AoE; clarify if necessary.

**No Unity implementation or Tier III designs; keep documenting after each completed round.**


## Psion Round 7 — 2026-10-26 (LATEST; resolves AoE ambiguity)

**Authoritative five-ability Tier II kit:** Psychic Bolt, Psionic Blades Mastery (passive; name still subject to final approval), Mental Image, **Lightning Storm**, Metal Shield. No sixth ability. The prior Psychic Shockwave proposal is **NOT a separate skill and NOT the selected AoE**. Its outward-expanding look, rank×basic-attack damage and speculative mechanics are **superseded**, not current design.

| ID | User decision | Status |
| --- | --- | --- |
| PSION-026 | **Restore original Lightning Storm**: three waves of Lightning-element AoE damage over **0.6 seconds**. The temporary Psychic Shockwave replacement is discarded. | USER CONFIRMED |
| PSION-027 | Lightning Storm scales with **INT-based damage PLUS flat damage per purchased skill rank**. It does **not** use rank × basic-attack damage. Exact coefficients, rank caps, AoE radius, cooldown and Mana cost remain OPEN. | USER CONFIRMED |
| PSION-028 | **Mental Image explosion deals Void damage**. This explicit extra note overrides the earlier questionnaire selection "Undecided" for its element. Explosion retains 4m radius and scales with AGI + INT + flat damage per rank. Exact damage channel remains to be formally specified (Void is an element, not automatically a channel). | USER CONFIRMED |
| PSION-029 | If two different Psions shield the same character, **each Psion maintains a separate shield absorption pool**. Within each caster's shield, affected allies share that caster's pool; re-casting by the same Psion replaces their existing pool with a fresh pool. Cross-pool damage order and cross-caster stacking edge cases still OPEN. | USER CONFIRMED |
| PSION-030 | **Uneven skill ranks**: Psychic Bolt, Psionic Blades Mastery and Mental Image receive relatively more purchasable ranks; **Lightning Storm and Metal Shield both get fewer ranks**. Exact caps, sum (~80 target) and level gates remain OPEN. | USER CONFIRMED |

**Carry forward unchanged:** Psychic Bolt Magical Metal and rank-scaling up-to-3s stun (boss Tenacity); Physical AGI+INT basic attacks at provisional 0.45+0.45 weighting, Psionic Blades Mastery +2.5 percentage points crit chance and 1 flat PDEF ignored per rank (150% crit damage); multiple stationary Mental Images attack for 10% caster basic-attack damage and inherit crit/basic attack modifiers, take 250% more damage, explode upon expiration/death, maximum rank duration 5 seconds; Metal Shield cast-time 8m AoE, affected members keep protection outside radius for 8 seconds, shared absorption pool per caster based on flat per rank plus small INT, re-cast replaces own pool. No Unity implementation or Tier III design.

**Next round:** determine exact relative rank caps/total; Lightning Storm damage application (three separately resolved waves); clone Void explosion channel and formula; Metal Shield overlapping pool damage-order; Psychic Bolt rank progression. Keep exactly five focused questions and update this file after each round.


## Psion Round 8 — 2026-10-26 (LATEST; user-confirmed)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-031 | **Exactly 80 purchased ranks total** allocated: **Psychic Bolt 20**, **Psionic Blades Mastery 18**, **Mental Image 18**, **Lightning Storm 12**, **Metal Shield 12**. This supersedes all illustrative rank distribution options. Unlock-level gates and initial rank accessibility remain open. | USER CONFIRMED |
| PSION-032 | **Lightning Storm has three waves over 0.6 seconds; EACH wave deals the full listed spell damage**, rather than dividing displayed damage by three. Spell's damage formula is INT-based + flat damage per skill rank. Exact coefficients and rank-1 values open; distinguish damage per wave from full cast total (3 × per-wave damage when all hit). | USER CONFIRMED |
| PSION-033 | **Mental Image explosion damage channel = Magical and element = Void**, with 4m AoE radius, AGI + INT + flat damage/rank, on death or expiry. Clone's basic attacks still inherit Physical basic-attack channel from universal rules; only explosion is Magical Void. | USER CONFIRMED |
| PSION-034 | When multiple Psions apply distinct Metal Shield pools to one target, damage is absorbed from **the oldest shield pool first**. Each caster retains one shared pool across their protected allies; recasting replaces that caster's old pool. Ordering/tie edge cases open. | USER CONFIRMED |
| PSION-035 | Psychic Bolt stun begins at **0.15 seconds at rank 1**, and gains **+0.15 seconds for each purchased rank**: formula **0.15s × rank**; rank 20 reaches **3.0 seconds** before boss Tenacity adjustment. This explicit user note **supersedes questionnaire's 0.5s start**. | USER CONFIRMED |

### Rank caps table (authoritative)

| Ability | Max purchased ranks |
| --- | ---: |
| Psychic Bolt | 20 |
| Psionic Blades Mastery | 18 |
| Mental Image | 18 |
| Lightning Storm | 12 |
| Metal Shield | 12 |
| **Total** | **80** |

**Implementation caution:** Each Lightning Storm wave deals the full per-wave listed damage; if all three land, a cast deals three times that value, before mitigation. Confirm timing and hit behavior during the next rounds. Neither an extra sixth ability nor Tier III content is authorized. Continue five-question design rounds and update this file after each completed round.


## Psion Round 9 — 2026-10-26 (LATEST; user-confirmed)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-036 | At Tier II Psion **level 1**, only **Psychic Bolt** and **Psionic Blades Mastery** are unlocked. Mental Image, Lightning Storm and Metal Shield unlock later; exact levels are OPEN. | USER CONFIRMED |
| PSION-037 | **Psychic Bolt cooldown = 8 seconds**. Previous rank-1 stun 0.15s and +0.15s per rank to max 3s remain locked, boss Tenacity applies. | USER CONFIRMED |
| PSION-038 | **Mental Image has no hard simultaneous-clone cap**. The number active is limited organically by its Mana cost, cooldown and lifetime (up to 5s at max rank); exact cooldown/Mana values remain OPEN. Do not invent a 2/3/4 clone cap. | USER CONFIRMED |
| PSION-039 | **Mental Image clones inherit Psion's current attack speed (100%)**, alongside the previously confirmed 10% per-hit basic-attack damage and inherited crit/basic-attack modifiers. | USER CONFIRMED |
| PSION-040 | **Lightning Storm is a targeted circular ground AoE**, dealing three individually full-damage Lightning waves over 0.6s with INT scaling plus flat damage per rank. Exact targeting range, circle radius and casting/movement rules OPEN. | USER CONFIRMED |

**Rank caps unchanged:** Bolt 20, Blades 18, Image 18, Storm 12, Shield 12 = 80. Exactly five abilities. No Unity implementation, no Tier III class work. Next interview round should settle remaining ability unlock levels and cooldown/Mana/range decisions. Keep the five-question cadence; update this file after each completed round.


## Psion Round 10 — 2026-10-26 (LATEST; user-confirmed)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-041 | **Mental Image unlocks at Psion level 5** (not level 1). | USER CONFIRMED |
| PSION-042 | **Lightning Storm unlocks at Psion level 8**. | USER CONFIRMED |
| PSION-043 | **Metal Shield unlocks at Psion level 10**. | USER CONFIRMED |
| PSION-044 | **Mental Image cooldown = 12 seconds**. | USER CONFIRMED |
| PSION-045 | **Lightning Storm cooldown = 15 seconds**. | USER CONFIRMED |

### Ability unlock schedule (authoritative)

| Ability | Unlock level | Max purchased ranks | Cooldown |
| --- | ---: | ---: | --- |
| Psychic Bolt | 1 | 20 | 8s |
| Psionic Blades Mastery (passive) | 1 | 18 | Passive |
| Mental Image | 5 | 18 | 12s |
| Lightning Storm | 8 | 12 | 15s |
| Metal Shield | 10 | 12 | OPEN |

**Balance clarification needed:** Mental Image's maximum lifetime is 5 seconds while its cooldown is now 12 seconds. Under ordinary single-charge cooldown rules, one Psion cannot have more than one active clone simultaneously. Earlier user decision explicitly removed any **hard clone cap**; preserve that rule, but do not imply that simultaneous clones are achievable without future cooldown reductions, charges, resets, recasts or other mechanics. No new enabling mechanic approved yet; ask later if needed.

**Still open:** Metal Shield cooldown, Mana costs, damage coefficients, Lightning Storm AoE radius, Mental Image cast range and target behavior, remaining rank eligibility/gates, and other tuning. Exactly five Tier II abilities; no Unity implementation or Tier III work. Continue with a batch of approximately five questions, updating this document after the completed design round.


## Psion Round 11 — 2026-10-26 (LATEST; user-confirmed)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-046 | **Metal Shield cooldown = 30 seconds**; shield remains cast-time 8m radius, 8s duration, flat-per-rank-dominant shared absorption pool, with INT contribution. | USER CONFIRMED |
| PSION-047 | **Mental Image duration = 1.6s at purchased rank 1 + 0.2s for every additional rank**; formula `1.6 + 0.2 × (rank − 1)` seconds, reaching **5.0s at rank 18**. | USER CONFIRMED |
| PSION-048 | **Mental Image cooldown stays at 12s**, with no simultaneous clone cap; **cooldown-reducing gear may enable overlapping clones later**. Do not invent a charge system, automatic cooldown reduction per rank or lower default cooldown. | USER CONFIRMED |
| PSION-049 | **Lightning Storm ground-targeted circular AoE has a 4m radius**; three full-damage waves over 0.6s, INT-based + flat per rank, 15s cooldown. | USER CONFIRMED |
| PSION-050 | **Metal Shield shared absorption pool scales primarily with flat shield amount per purchased rank plus a moderate INT contribution**; flat-rank portion should remain dominant at ordinary target gear levels. Exact coefficients/formula remain OPEN. | USER CONFIRMED |

### Ability overview after Round 11

| Ability | Unlock | Max ranks | Cooldown | Additional facts |
| --- | ---: | ---: | --- | --- |
| Psychic Bolt | 1 | 20 | 8s | Magical Metal; stun = 0.15s × rank before Tenacity |
| Psionic Blades Mastery | 1 | 18 | Passive | +2.5 percentage points basic crit and 1 flat PDEF ignored per rank |
| Mental Image | 5 | 18 | 12s | Duration 1.6→5s; stationary; full caster attack speed; attacks for 10% basic value; 4m Magical Void explosion; no hard cap |
| Lightning Storm | 8 | 12 | 15s | Ground-targeted 4m AoE; 3 waves in 0.6s, each full INT + flat-rank damage |
| Metal Shield | 10 | 12 | 30s | At cast: Psion and allies within 8m protected for 8s, retain protection after leaving; one shared absorption pool per caster; flat ranks dominant + moderate INT; oldest pool consumed first if overlapping, recast replaces own pool |

**Still to specify**: exact numbers for mana costs, damage/shield coefficients, ranges, cast times and eligible rank-purchase levels, plus advanced corner cases. No Unity implementation nor Tier III class designing. Continue with five-question rounds and update the design doc each round.


## Psion Round 12 — 2026-10-26 (LATEST; user-confirmed)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-051 | **Psychic Bolt listed damage = 30 + 8 × purchased rank + 1.2 × INT.** Single-target Magical Metal damage with 8s cooldown; stun = 0.15s × rank, reduced by boss Tenacity. This implies 38 + 1.2×INT at rank 1 and 190 + 1.2×INT at rank 20 under the literal rank formula. | USER CONFIRMED |
| PSION-052 | **Lightning Storm damage PER EACH of its three waves = 15 + 5 × purchased rank + 0.6 × INT**; all three waves deal their own full listed damage over 0.6s. Ground-targeted circle radius 4m; 15s cooldown; Lightning-element Magical damage. Literal rank 1 per-wave = 20 + 0.6×INT; rank 12 = 75 + 0.6×INT. | USER CONFIRMED |
| PSION-053 | **Metal Shield one shared absorption pool per casting Psion = 100 + 30 × purchased rank + 0.75 × INT**. This absorbs total shared damage across all allies covered by that particular cast, NOT once for each ally. Flat/rank portion dominates; cooldown 30s, 8m initial cast-time radius, 8s duration. Literal rank 1 pool = 130 + 0.75×INT; rank 12 pool = 460 + 0.75×INT. | USER CONFIRMED |
| PSION-054 | **Psychic Bolt uses a very short NONZERO cast time**: user initially chose instant, then explicitly corrected this in extra thoughts: "i don't want instant, i want like 0.1 or 0.2 cast time." **Exact 0.1 vs 0.2 seconds OPEN**, do not record instant. | USER CONFIRMED nonzero short cast; exact timing OPEN |
| PSION-055 | **Mental Image remains ground-targeted placement**, but also follows user's correction away from truly instant casts; approximately **0.1–0.2s nonzero cast time**, exact value OPEN. Earlier instant placement answer is superseded only with respect to cast duration, not ground targeting. | USER CONFIRMED ground targeting and nonzero short cast; exact timing OPEN |

**Interpretation note:** Three numerical formulas above are written literally as "base + coefficient × rank + stat multiplier" rather than silently converting them to (rank−1). Both cast-time selections came from the same round and user provided one general correction; confirm whether both should share the same exact cast time, and whether other spells follow the same minimum-cast principle. All five ability identities, 80 rank allocation, cooldowns and other prior approvals remain unchanged. No Unity implementation/Tier III content authorized. Continue approximately five-question design batches and update after each round.


## Psion Round 13 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-056 | **Psychic Bolt cast time = 0.1 seconds**. This supersedes the previously open short-cast range and the earlier instant-cast option. | USER CONFIRMED |
| PSION-057 | **Mental Image cast time = 0.2 seconds**, using **ground-targeted placement** as previously confirmed. | USER CONFIRMED |
| PSION-058 | **Lightning Storm cast time = 0.2 seconds**, targeted 4m-radius ground AoE with three full-damage waves over 0.6s. | USER CONFIRMED |
| PSION-059 | **Metal Shield cast time = 0.2 seconds**, centered on caster, affecting eligible allies in 8m radius at cast resolution. | USER CONFIRMED |
| PSION-060 | **Mental Image targeting:** prioritize the Psion's current target if it is inside the stationary clone's attack range and valid to attack; otherwise attack the nearest valid enemy in range. Further retarget-frequency details are open. | USER CONFIRMED |
| PSION-061 | **Mana costs increase significantly with purchased skill rank** for active skills, rather than remaining fixed. Exact starting costs, per-rank growth and whether each skill uses different coefficients remain OPEN. Psionic Blades Mastery is passive and has no cast Mana cost. | USER CONFIRMED general Mana progression; exact numbers OPEN |

### Current cast times

| Active ability | Cast time |
| --- | ---: |
| Psychic Bolt | 0.1s |
| Mental Image | 0.2s |
| Lightning Storm | 0.2s |
| Metal Shield | 0.2s |

**Next design round:** determine Mana-cost progression and any remaining important cast/target behavior. Preserve 80 total ranks, existing unlocks, cooldowns, ranges, shield sharing and damage formulas. No Tier III class or Unity work authorized. Maintain five-question interview batches and update this file after each round.


## Psion Round 14 — 2026-10-26 (LATEST)

User explicitly **delegated numerical Mana-formula selection to the designer**, acknowledging that the values require playtesting and balancing. Therefore, mana coefficients below are **DESIGNER-SELECTED PROVISIONAL PLAYTEST VALUES (AUTHORIZED TO PROPOSE), NOT USER-APPROVED FINAL BALANCE**. These honor the user's confirmed rule that Mana increases substantially with purchased skill rank.

| ID | Ability | Provisional Mana cost (r = purchased rank) | Rank 1 | Maximum rank cost | Status |
| --- | --- | --- | ---: | ---: | --- |
| PSION-062 | Psychic Bolt (max rank 20) | **8 + 3 × r** | 11 | 68 | DESIGNER PROVISIONAL — PLAYTEST |
| PSION-063 | Mental Image (max rank 18) | **14 + 5 × r** | 19 | 104 | DESIGNER PROVISIONAL — PLAYTEST |
| PSION-064 | Lightning Storm (max rank 12) | **20 + 7 × r** | 27 | 104 | DESIGNER PROVISIONAL — PLAYTEST |
| PSION-065 | Metal Shield (max rank 12) | **18 + 6 × r** | 24 | 90 | DESIGNER PROVISIONAL — PLAYTEST |

| ID | Decision | Status |
| --- | --- | --- |
| PSION-066 | **Mental Image retargeting:** when the Psion changes targets, the clone **finishes its current basic attack before changing targets**, if the new Psion target is valid and in clone attack range. Otherwise, it selects the nearest valid enemy in range as previously agreed. | USER CONFIRMED |

**Important validation notes:** Psion's final level-dependent Mana budget/regen is not specified, so costs require simulation and real playtesting against actual resources, cast frequency, rank gates and time-to-kill. Literal formula uses rank r, so rank 1 costs include one rank increment. Psionic Blades Mastery passive has no cast Mana cost. Do not silently reclassify these designer-provisional coefficients as user-confirmed permanent balance. Preserve no-hard-clone-cap and cooldown/gear interactions. Gameplay kit still exactly five abilities; no Unity implementation or World2 Tier III work authorized.


## Psion Round 15 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-067 | **Use the other Tier II classes' skill-rank progression convention**: new class skills begin at rank 0; character earns one spendable class skill point at Tier II level 1 and one per level until the 80-rank budget is reached; eligible purchases require their per-skill level gate and previous rank. Author and audit an 80-rank schedule with an eligible purchase each level 1–80, respecting Psion's existing ability unlock levels. The **specific Psion gate table is NOT YET APPROVED**. Consult Acolyte/Thief progression for reference; do not simply copy their different skill distributions. | USER CONFIRMED progression model; detailed gates TO DESIGN |
| PSION-068 | **Mental Image copies Psion's maximum HP at cast time** and takes **250% increased incoming damage** (3.5× normal damage, subject to mitigation-rule implementation). This remains true regardless of Mental Image rank; no separate rank-scaling clone HP bonus was approved. | USER CONFIRMED |
| PSION-069 | **Mental Image snapshots its stats at summon time**, including stats determined by gear. Later equipment swaps do **not** retroactively update existing clones. At creation clone inherits caster's current HP capacity, basic-attack raw value, attack speed, range, crit chance and basic-attack modifiers as previously specified; clone HP starts at caster's max HP unless revised. | USER CONFIRMED snapshot behavior; detailed snapshot fields inferred from already-approved inheritance |
| PSION-070 | **Lightning Storm's wave sequence is movement-committed.** After its 0.2s cast, moving the Psion during the three waves over 0.6s **cancels the remaining waves**. Resolved prior waves remain dealt; no automatic refund or continued waves. Damage interruption behavior not yet decided. | USER CONFIRMED |
| PSION-071 | **Psychic Bolt against stun-immune bosses follows the game's universal Tenacity and immunity rules**. Damage still resolves normally under damage defenses; do not invent custom slow, alternate interrupt or extra CC when stun is immune. Shared global Tenacity exact equation remains separately open. | USER CONFIRMED |

**Tier II reference check (2026-10-26):** Acolyte and Thief both use one spendable point per level 1–80, all skills initially rank 0, explicit increasing rank eligibility levels, and arithmetic proof that every level has at least one eligible purchase; their specific caps and rank-gate tables were separately approved. Psion's max ranks remain Bolt20 / Blades18 / Image18 / Storm12 / Shield12, unlock levels 1/1/5/8/10. Design its own eligibility gates and validate; do not claim any user-approved exact table yet.

**Remaining design:** Psion skill gate schedule, Mental Image explosion trigger and lifetime interaction, Storm non-movement interruption rules, practical ranges, resource/max Mana baselines and last presentation/quest deferrals. Mana coefficients are provisional designer test numbers. No Unity implementation or Tier III creation.


## Psion Round 16 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-072 | Psion **basic-attack range = 5 meters**. Psion remains a genuinely short-ranged ranged hybrid. | USER CONFIRMED |
| PSION-073 | **Psychic Bolt range = basic-attack range + 2 meters**, thus **7 meters** at the approved 5-meter baseline. | USER CONFIRMED |
| PSION-074 | **Mental Image placement range = 6 meters from the Psion**. This is ground-target placement distance, separate from the clone's inherited **5m basic-attack range**. | USER CONFIRMED |
| PSION-075 | **Lightning Storm follows normal channel rules**. The prior 0.2s cast value is explicitly **SUPERSEDED**: the spell now has **0.1 seconds of initial casting, then channels the actual three-wave storm over 0.6 seconds**. Movement or stun during channel cancels remaining waves, and explicit interrupts follow normal channel rules. Ordinary incoming damage should follow shared channel handling, which is per-ability in COMBAT.md; unless globally decided, do not silently assume every hit interrupts. | USER CONFIRMED |
| PSION-076 | When Mental Image expires while mid-attack, **only attacks whose projectile has already been launched continue to resolve**. An unlaunched attack is canceled on expiry and the clone explodes immediately; a launched projectile remains in flight according to normal projectile resolution. | USER CONFIRMED |

**Casting/channel clarification:** The Lightning Storm's **0.1s initial cast + 0.6s active channel** gives a 0.7s full execution window if uninterrupted, not the older 0.2s cast + independent storm. Three independently full-damage Lightning waves occur within the 0.6s channel; interruption retains previous resolved waves and cancels the remaining waves. Unlike Ranger Arrow Rain's independently resolving ground rain, Lightning Storm requires ongoing channeling. Combat docs affirm each ability defines movement, damage and interruption behavior; they do not define a universal automatic cancel-on-any-damage rule. Existing 4m ground-targeted circle, INT + flat damage/rank per wave, 15s cooldown remain.

**Open:** Lightning Storm ordinary incoming-damage interruption choice if not otherwise settled, per-wave exact trigger timestamps, Psion skill-rank gate table, Mental Image explosion damage coefficient and shield/practical balance, other target/range edge cases. No Unity implementation, no Tier III designs. Continue five-question design batches and update this document after each completed round.


## Psion Round 17 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-077 | **Lightning Storm uses normal shared channel-interruption rules**, in addition to explicit previously approved movement/stun cancellation. Do not invent a custom any-hit interruption rule. | USER CONFIRMED |
| PSION-078 | Its three full-damage Lightning waves occur at **0.0s, 0.3s and 0.6s into the channel**, following the initial **0.1s cast**. On interruption, already resolved waves remain; subsequent scheduled waves are cancelled. Boundary handling at exactly 0.6s must be deterministic. | USER CONFIRMED |
| PSION-079 | **Mental Image explosion Magical Void RAW damage** is **30 + 10 × skill rank + 0.6 × AGI + 0.6 × INT** within 4m on clone expiry or death. The clone's stats including AGI/INT are snapshotted at summon; exact rank1 at zero attributes =40, rank18 =210, before stats/mitigation. | USER CONFIRMED |
| PSION-080 | **Metal Shield follows the existing universal elemental shield rules**, including normal shield piercing and eligibility, **NOT an invented class-specific absorption exception**. As a Metal-element shield, against ordinary eligible post-mitigation incoming Metal damage use **0.8 shield HP per damage prevented**; other elemental/untyped ordinary damage uses **1.2 shield HP per damage prevented**. This applies to ONE shared capacity per caster, with oldest pool first between casters, same-caster recast replacing. The shield capacity formula is still 100 + 30×rank + 0.75×INT, not multiplied per covered member. | USER CONFIRMED |
| PSION-081 | **All classes share the same unmodified baseline movement speed**. The existing project baseline is **5 m/s / 50 displayed Move Speed**. Attribute, gear, ability and temporary movement modifiers remain eligible, but don't give Psion or other classes inherently different unmodified baseline speeds. Supersedes historical statements that classes can have individually different base movement speeds. | USER CONFIRMED GAME-WIDE |

**Documentation source checks:** `docs/STATS_AND_DAMAGE.md` confirms the current shared elemental shield coefficients 0.8 matching / 1.2 nonmatching, current generic shield implementation 1:1, and the existing 5 m/s / 50 display baseline. `docs/COMBAT.md` describes per-ability channel interruption instead of a universal any-damage-cancels rule. Retain class-specific modifiers to final speed, but enforce equal baseline speed across all classes. Next rounds should focus on the Psion gate schedule and remaining material mechanical open points; numeric Mana remains delegated playtest values. No Unity implementation/Tier III authorized.


## Psion Round 18 — 2026-10-26 (LATEST; USER CONFIRMED)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-082 | **Mental Image's 4m Magical Void explosion damages enemies only**; it does not damage friendly players, the casting Psion, or friendly owned units under ordinary friendly-fire assumptions. | USER CONFIRMED |
| PSION-083 | **Mental Image always explodes for full listed damage when killed by enemies**, just as on natural expiry. Damage does not scale down with remaining lifetime or current HP. Explosion raw damage remains `30 + 10 × rank + 0.6 × AGI + 0.6 × INT` (snapshotted stats), and radius remains 4m. | USER CONFIRMED |
| PSION-084 | **Every Lightning Storm wave independently checks which enemies are currently inside the 4m targeted ground AoE**. Enemies entering before a later wave can be hit by that wave; those who leave are not hit by subsequent waves while outside. Three full-damage waves at 0.0, 0.3, 0.6s into the channel if not interrupted. | USER CONFIRMED |
| PSION-085 | When a protected ally dies, **their Metal Shield protection ends, but the remaining shared pool persists for the other surviving protected allies** for its remaining duration/capacity. Death does not destroy or replenish the entire pool. | USER CONFIRMED |
| PSION-086 | **Psychic Bolt follows the existing normal targeted-spell projectile system** for travel, terrain/obstacle interaction, targeting and hit resolution; do not invent class-specific instant-hit or obstacle-piercing behavior. | USER CONFIRMED |

**Open priority:** author and mathematically validate Psion's exact 80-rank skill gate progression against class unlocks (Bolt1 / Blades1 / Image5 / Storm8 / Shield10) and caps (20/18/18/12/12). Present for user review before calling exact gates approved. Numerical Mana costs remain designer-provisional. Defer model and advancement mentor/quest until later World1 content planning. No Unity implementation or Tier III design authorized.


## Psion Round 19 — 2026-10-26 (LATEST)

| ID | Decision | Status |
| --- | --- | --- |
| PSION-087 | Use **the same general rank-gate spacing approach as Acolyte and Thief**: Tier II level 1 starts with one spendable point, one more each level through 80, 80 purchased ranks total, rank-specific gates and predecessor-rank prerequisites. Author Psion-specific gates to honor level-1 Bolt/Blades, Image level5, Storm level8, Shield level10. | USER CONFIRMED DIRECTION; exact gate schedule is DESIGNER PLAYTEST PROPOSAL BELOW |
| PSION-088 | Retain **+2.5 percentage points basic-attack critical chance per Psionic Blades Mastery rank**, but **mark the coefficient provisional until gameplay playtesting** (18 ranks would currently yield +45pp). Existing 1 flat PDEF bypass/rank and 150% basic crit damage remain. | USER CONFIRMED PROVISIONAL |
| PSION-089 | **No manual-dismissal ability** for Mental Image. It expires normally or can be killed, triggering its full enemies-only Magical Void explosion; no invented dismissal control. | USER CONFIRMED |
| PSION-090 | **Mental Image is a normal targetable unit** with ordinary threat and real collision, rather than being only incidentally hittable. Its earlier 250% increased incoming damage rule, caster-max-HP snapshot, immobility and 4m full-damage explosion remain. | USER CONFIRMED |
| PSION-091 | **Psion Physical basic attack uses 0.45× AGI + 0.45× INT as the intended baseline**, retaining normal shared basic-attack systems. Coefficients are a starting balance baseline subject to normal playtesting, not a reason to shift toward 0.5/0.5. | USER CONFIRMED |

### Designer-proposed 80-rank eligibility schedule — REVIEW BASELINE, NOT YET USER APPROVED EXACT GATES

In accordance with PSION-087, follow the Acolyte/Thief structure with approximately 4 levels between upgrades on three core skills and 6 levels on Storm/Shield, plus early front-loaded Bolt/Blades upgrades to make initial levels feasible. A gate makes a rank eligible, **not automatically purchased**. Previous ranks must be purchased before the next rank of that skill. All five class skills start rank0.

| Skill | Max ranks | First eligible level | Final gate | Every proposed individual rank gate |
| --- | ---: | ---: | ---: | --- |
| Psychic Bolt | 20 | 1 | 75 | 1, 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51, 55, 59, 63, 67, 71, 75 |
| Psionic Blades Mastery | 18 | 1 | 66 | 1, 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54, 58, 62, 66 |
| Mental Image | 18 | 5 | 73 | 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49, 53, 57, 61, 65, 69, 73 |
| Lightning Storm | 12 | 8 | 74 | 8, 14, 20, 26, 32, 38, 44, 50, 56, 62, 68, 74 |
| Metal Shield | 12 | 10 | 76 | 10, 16, 22, 28, 34, 40, 46, 52, 58, 64, 70, 76 |
| **TOTAL** | **80** | — | **76** | All rank gates reachable by level 76 |

**Arithmetic check performed:** For EVERY class level `L=1..80`, cumulative gates `eligible(L) >= L`; hence a player earning one class point per level has at least one legal path to spend one per level. Each skill's gates are strictly increasing, none occur before the ability's existing unlock, and all 80 ranks are eligible by level76. Validation PASS for all 80 class levels; no enforced class rank purchases and no additional skill ranks beyond caps.

| Class level | Cumulative eligible rank gates | Cushion over points earned |
| ---: | ---: | ---: |
| 1 | 2 | 1 |
| 2 | 3 | 1 |
| 3 | 4 | 1 |
| 4 | 4 | 0 |
| 5 | 5 | 0 |
| 8 | 8 | 0 |
| 10 | 11 | 1 |
| 20 | 21 | 1 |
| 30 | 32 | 2 |
| 40 | 43 | 3 |
| 50 | 54 | 4 |
| 60 | 64 | 4 |
| 70 | 75 | 5 |
| 75 | 79 | 4 |
| 76 | 80 | 4 |
| 77 | 80 | 3 |
| 78 | 80 | 2 |
| 79 | 80 | 1 |
| 80 | 80 | 0 |

**Next step:** user can approve the specific schedule or ask for changes, and final balancing/playtests can tune cooldowns/mana/damage. Psionic Blades critical coefficient and existing designer Mana costs remain provisional for playtesting. Psion class appearance and advancement quest remain deferred to later region/city design; do not start Tier III class design or modify Unity implementation from this interview.


## Psion Round 20 — 2026-10-26 — TIER II GAMEPLAY DESIGN COMPLETE

**STATUS: USER APPROVED ALL 80 EXACT SKILL RANK GATES. Psion's five-ability Tier II gameplay design interview is COMPLETE as the initial playtest specification, NOT IMPLEMENTED IN UNITY and NOT YET PLAYTEST-BALANCED.** This section is the latest authority over outdated "proposed" / "pending approval" gate labels above. Earlier brainstorms are historical.

| ID | Decision | Status |
| --- | --- | --- |
| PSION-092 | **APPROVE all 80 EXACT rank eligibility gates** in the designer-audited Round 19 table as first-playtest design. One class skill point per level 1–80, no auto ranks, prior rank prerequisite, all points have available legal purchases at every level. Caps Bolt20 / Blades18 / Image18 / Storm12 / Shield12. Unlocks Bolt1, Blades1, Image5, Storm8, Shield10. Every gate is available by level76. | USER APPROVED EXACT TABLE |
| PSION-093 | **Mental Image is fully autonomous**: use previously approved caster-current-target-then-nearest target selection, finishing its current attack before retargeting. NO direct player RTS command/control of clone. | USER CONFIRMED |
| PSION-094 | If the **Psion dies**, an existing Mental Image **remains in the world until its ordinary scheduled expiration**, unless killed first; regular targeting/combat/collision and full Void explosion on either cause continue. Do not auto-despawn it on owner death. | USER CONFIRMED |
| PSION-095 | **Overlapping Lightning Storms from multiple Psions each resolve all their own full damage independently**, per normal overlapping AoE rules. No reduction, mutual cancellation or replacement solely due to overlap. User explicitly confirms this should be the *default global behavior for independent damaging AoEs*, not a Psion-only privilege. Exceptions must be authored explicitly; status/effect stacking can still have their own rules. | USER CONFIRMED GLOBAL |
| PSION-096 | **Defer Psion appearance/model AND advancement mentor/quest** until planning the major city, after the Quillboar region. Do not invent or implement them as part of completing gameplay design. | USER CONFIRMED |

### Final rank-gate approval

The **entire five-row, individual rank eligibility list in Round 19** is now **USER APPROVED (PSION-092)**, not merely designer-proposed. The computed rank-gate audit PASSED for all 80 character levels. All gates unlock by level76, but all 80 skill points are earned through level80. Keep the **+2.5 percentage points per Psionic Blades Mastery rank** as a provisional playtest coefficient and the four Mana-cost formulas as designer-selected first-playtest values; skill identity, mechanics, approved gates and other user-confirmed details are not reopened automatically.

### Next content-design sequence

**Psion gameplay complete → Quillboar region → major city → deferred appearance/advancement quests when geographically appropriate.** Normal World1 Tier II roster design complete; do not design Tier III descendants until World2. No Unity gameplay implementation or test execution has been authorized by this design conversation.

# DiceFree — Marksman (Tier III Ranger evolution) — design interview

**2026-11-25 — ROUNDS 1–3 USER DECISIONS RECORDED; ROUND 4 OPEN.** User explicitly selected **Ranger → Marksman** as the FIRST Tier III evolution to design in the new **at least one Tier III per Tier II class** coverage pass. **Greatblade → Berserker follows next**, then choose first evolution for the five other uncovered Tier II parents. This is an AUTHORIZED DESIGN INTERVIEW, **NOT Unity code/asset/build implementation permission**.

**Status:** Rounds 1–3 establish Marksman as a **stationary ranged damage dealer** with **11m→15m bow autoattack reach via Bow Mastery20**, Power Shot **12m→16m** reach with **1.5m width and solid-wall collision**, longbows favored while shortbows remain equippable, **NO Hawk Dive/Hunter's Mark**, exact **six skill rank caps 30/25/20/30/20/25 =150** and 1 earned point per level1–150. Strafe gains +2% attack speed/rank with −2% move speed/rank and rank-scaling duration; aura improves allied RANGED BASIC ATTACKS ONLY, strongest same-name source, no skills; Aimed Shot has a slower windup, NORMAL existing crit handling and percentage PDEF penetration (about10% at cap as a USER-FLOATED target, not exact locked formula). The permanent **Ranged Damage Aura** and 80%-per-target piercing **Power Shot** are now **USER-APPROVED ROSTER MEMBERS**, along with the other four skills. All 150 skill-rank points are earned 1 per level 1–150. Exact per-ability caps/rank gates and several damage/resource numbers remain open. The **Marksman class name and the choice of Ranger as its parent in this rollout are USER CONFIRMED**. A precision-focused, longer-ranged longbow archetype was discussed in Ranger's earlier Tier II interviews as an **IDEA**; it is NOT YET an approved detailed Tier III skill set or a numerical range. Questions below are open, with suggested directions clearly marked as suggestions. Do not confuse this with an already implemented Tier III class.

**Source documents:** [Tier III coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md), [Ranger Tier II design](RANGER_TIER2_DESIGN.md), [Class Progression](../CLASS_PROGRESSION.md), [Stats and Damage](../STATS_AND_DAMAGE.md).

## Parent Ranger baseline to preserve

On `feat/verdant-wilds` the actual actor YAML `Assets/_DiceFree/Settings/Ranger/Ranger class.asset` has **VIT13 / STR8 / AGI18 / INT7 / SPR9** at Level1, growth **VIT1.50 / STR0.70 / AGI3.10 / INT0.80 / SPR1.15** per level; total starting attributes55, growth7.25/level. Base move speed5, base HP10, +15 HP per VIT, base PDEF/MDEF0.

`Assets/_DiceFree/Settings/Ranger/Ranger bow basic.asset` gives:
- Ordinary Ranger bow attack **Physical** (channel0 in actor asset), **AGI primary** attack raw weights 1.05 AGI, **baseDamage5**, interval **1.42s**, windup **0.47s**, **base range11m**;
- `requiresAccuracy=1`; normal shots may miss under ordinary combat rules, whereas Ranger **Exploding Arrow is EXPLICITLY guaranteed to hit** under the Tier II user correction;
- Current both shortbows and longbows are **TWO-HANDED**, share class-authored base attack range, and have item-level attack-speed vs raw attack-power tradeoffs; no equipped quiver offhand.
- Normal attacks do **NOT** cost Mana. Ranger uses Mana only; `Assets/_DiceFree/Settings/Ranger/Ranger Mana.asset` current profile max = `60+1×INT` and regeneration/sec = `4+0.1×SPR`.
- Ranger's existing six skills and exact 80-class-rank gates are **approved for its Tier II manifestation**: **Aimed Shot20**, **Exploding Arrow18**, **Arrow Rain16**, **Hawk Dive10**, **Evasive Step8**, **Bow Mastery8**.
- **Aimed Shot** rewards attacking the target marked by the Ranger, with the user's confirmed +`0.5×AGI×(1+0.05×(r−1))` extra raw marked-target bonus; Bow Mastery's crit and +0.1m/rank bonus apply to Aimed Shot and basics.
- **Hawk Dive** marks the enemy for own additional damage, normal 8s first-rank Hunter's Mark, only one marked target per Ranger.
- **Exploding Arrow** deals direct PLUS Gun-element area damage around the successfully struck primary enemy, **cannot miss**; do not invent miss/detonation-on-ground behavior.
- **Arrow Rain** channels 1.5s firing large then small arrows skyward; **four area damage waves over two seconds** after successful completion, 3m circle, channel canceled by movement or stun but rain persists after channel completes.
- **Evasive Step** leaping reposition + temporary dodge and movespeed; cannot use when rooted/stunned. **Bow Mastery** grants AGI, short range extension, crit chance at 1.4x for BASIC+Aimed Shot ONLY.

The new Tier III must have its OWN starting stats, growth, resources, abilities, gear permissions and advancement path. It begins as a new class Level1 manifestation after eligible Tier II Ranger Level60 advancement, preserving the Ranger parent. Do NOT copy an existing Tier III's skill caps, exact gates, Mana or quest without a new design round.

## Current identity direction (suggestion, NOT yet a locked detailed kit)

**Marksman** would evolve Ranger's bow expertise into a class specializing in **precise ranged hits, long sightlines, higher single-target damage and exploiting marked targets**. Suggested tradeoff: less instant burst movement/area disruption than Ranger or future Beastmaster, in exchange for reach and stronger punish windows. **A longbow-oriented visual identity is a proposal; whether shortbows and other compatible bows are allowed is a user choice.**

Suggested initial metrics only, **not authorized values**:
- Range beyond Ranger's actual current **11m**, e.g., 14m / 16m / 18m, from Marksman's CLASS attack definition; preserve the normal rule that compatible different bows at the same Marksman rank have the same base reach unless user explicitly changes it.
- Strength focused on AGI, with meaningful VIT for survivability. No stat numbers approved yet.
- Retain bow-shot readability in Warcraft III/LoL-style click movement and cast controls. Whether attacks require standing still, an upgrade to Hunter's Mark, or partial movement while drawing are design choices.

## Round 1: FIVE QUESTIONS TO ASK USER (OPEN)

1. **Combat rhythm:** Should Marksman primarily be an **immobile long-range sniper** with heavy windups and massive payoff, a **mobile accurate archer** with useful repositioning, or **balanced** (powerful stationary shots plus modest mobility)? User may propose other combat style.
2. **Base range and weapon identity:** Current Ranger bow reach is **11m**. Should Marksman start at **14m**, **16m**, **18m**, or another value? Should Marksman use **both longbows and shortbows**, with longbows *favored*, or **longbows exclusively**? Future crossbow/guns should NOT be silently added.
3. **Hunter's Mark evolution:** Should the Marksman keep the Ranger-style **Hawk Dive to apply a single target mark**, place it with a new aimed attack, or get a more elaborate **target tracking/weak-point stacking** system? Mark's damage bonus and eventual multi-target limits are undecided.
4. **Inherited skills:** From **Aimed Shot, Exploding Arrow, Arrow Rain, Hawk Dive, Evasive Step and Bow Mastery**, which feel essential to **retain and improve** for Marksman and which should be **replaced by new advanced skills**? There is no mandatory five-/six-ability cap until the user decides.
5. **Signature new Marksman mechanic:** Which deserves a signature skill/passive: **charged armor-piercing shot**, **critical weak-point exploitation**, **long-range piercing/multi-target line shot**, **focus/steady-aim stance**, or a different idea? Do not add a new spendable Focus resource without user's choice.

## Next design stages after questions answered

- Confirm inherited vs new active and passive roster, exact rank caps/rank gates (Tier III per-class level1–150 pattern from the established Psion precedent is a useful candidate, but MUST be explicitly designed/audited for Marksman), elements, Physical/Magical channels and crit permission.
- Define rank-growing AGI damage formulas, cooldowns/Mana scaling and a separate class Mana/HP profile appropriate to user-approved higher-tier growth direction, using actual Ranger baseline and Psion Tier III for comparison, not blindly copying Psion's huge Mana numbers.
- Choose AGI stat budget and level growth stronger than Ranger, final bow equipment compatibility, visuals, quest teacher/location, a suitable solo trial only if user wants one, and extra shot/mark/projectile/LoS edge cases.
- Present decisions in five-question rounds and **update this GitHub design doc after each completed round**, following user's standing workflow.
- After Marksman: [Tier III coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md) schedules **Greatblade → Berserker next**.


## Round 1 — 2026-11-25 (LATEST user answers; replaces initial open interview assumptions)

**Actual user response, unambiguously approved class fundamentals:**
1. **Combat style: STATIONARY ARCHER**, dedicated ranged damage dealer. Do not convert Marksman into a run-and-gun moving shooter or a melee hybrid. Its ordinary arrow autoattack and deliberate shots honor stationary draw/windup gameplay. Existing shared controls/movement interruption concepts remain the point of reference. Strafe below is a firing-speed effect, **not** permission to attack while moving.
2. **Attack range:** tier III Marksman begins with **the same 11m base bow range as parent Ranger**, NOT 14m/16m/18m. Skill ranks increase its relevant bow range to a hard **15m MAXIMUM**. Suggested design interface: **Bow Mastery** is the rankable source of +4m TOTAL at its maximum purchased Marksman rank (candidate `baseReach=11m+4m×(BowMasteryRank/BowMasteryCap)`, cap15m); **the exact Marksman mastery rank cap and ramp/eligibility gates remain OPEN**, so this curve is a recommendation, not an approved exact +m/rank. Parent Ranger's own +0.1m/rank mastery is NOT retroactively changed.
3. **Bows:** **longbows favored** for Marksman's theme, but **shortbows remain equippable**. Keep class-defined reach rather than a general "longbow = longer range" rule. Distinct items may have class restrictions and others may be shared cross-class later. The user also said a future **Beastmaster should favor shortbows** and wrote "but still able to equip shortbows" again; this repeats the same family, so the *other* compatible bow type for Beastmaster (probably longbows) needs later explicit clarification rather than silently inventing a restriction or correcting a quote. No Beastmaster class kit authorized by this round.
4. **NO Hawk Dive/Hunter's Mark** as Marksman's mechanic. Do NOT make Hawk Dive, Hawk pet strike, Hunter's Mark, hidden target debuff, tracking stack or mark synergy a required Marksman ability. The Ranger's **Aimed Shot** historically receives extra marked-target damage, but Marksman **must not retain that conditional bonus as-is** without the absent mark; reauthor it for strong independent single-target raw AGI scaling.
5. **Retention from parent explicitly selected:** **Aimed Shot**, **Arrow Rain**, and **Bow Mastery** remain as Marksman skills, expected to receive Tier III ranks/appropriate improvements. **Exploding Arrow, Hawk Dive, Evasive Step were not selected to carry over**. Do not automatically port them into the Marksman six-skill roster or assume a free movement dash. Do not transfer Ranger skill ranks or invent a fixed 4/5/6-skill restriction without discussion.
6. **New Strafe skill selected as a combat concept:** a **timed self-buff increasing attack speed while reducing movement speed**. User floated approximately **+2% attack speed × purchased Strafe skill rank**, for approximately **`ceil(1 + rank/5)` seconds**, but prefaced these with "something like", so **the exact coefficients, rounding, movement penalty and cooldown are PROVISIONAL proposals, NOT user-approved final numerics**. Mathematically `ceil(1+r/5)` gives 2 seconds at rank1 and rank5, 3 seconds at rank6; if intended behavior instead is `1+ceil(r/5)`, it gives the SAME integer progression for positive integer ranks. (NB these two equivalent expressions apply for integer ranks; keep one consistent convention.) The buff can slow repositioning even though firing remains stationary; it should not unintentionally add an auto-aim/strafing locomotion feature just because of its name.
7. **Ranged damage AURA is TENTATIVE:** "maybe an aura that increases nearby ranged damage by rank × 1%" suggests a second new rankable buff/passive or active aura. Draft baseline **+1 percentage point to eligible allied RANGED DAMAGE per purchased aura skill rank**, in a radius **not yet selected**; who counts as ally (including caster and summons), what counts as ranged damage (basic-only versus ranged skills and spells), duration/stacking and rank cap remain OPEN. Must NOT grant generic bonus to all magic spells or all damage merely because the caster stands at range.
8. **Piercing POWER SHOT is TENTATIVE:** "maybe a powershot doing damage in a line piercing monsters (and doing reduced damage for each monster pierced)." Draft as a **line projectile** dealing Physical arrow damage in sequential target hit order, subsequent hit instances diminished as the projectile passes monsters. OPEN: width/length, collision geometry, damage falloff for each pierced target, hit caps, projectile speed, Mana/CD, crit eligibility and stop-on-world-obstacle rules. No automatic wall penetration or multi-hit of the same target. Distinguish from Ranger's ground AoE Arrow Rain.
9. **Primary objective:** The class is essentially a **ranged damage dealer**. Do not inflate it into a complicated weak-point resource, ranged support healer or mandatory third meter; Mana is the likely inherited Tier II starting resource but the exact Marksman test profile remains OPEN. No Hawk's mark, no forced Focus bar.

### Current Marksman proposed roster and approval states

| Ability | Current status | Role / design intention |
| --- | --- | --- |
| **Aimed Shot** | **USER-SELECTED retention** | Strong single-target ranged Physical AGI shot, **NO mark dependency**. Rework parent marked-target extra damage to unconditional higher-rank accuracy/precision/damage when designed. |
| **Arrow Rain** | **USER-SELECTED retention** | Stationary channel then multiwave raining arrows. Parent 1.5s channel + four waves/2s is a useful baseline; Tier III larger rank-scaled damage/radius and channel progression UNDECIDED. |
| **Bow Mastery** | **USER-SELECTED retention** | Marksman's stat/critical/range passive, intended to yield +4m reach **from 11m to 15m by max rank**, with exact cap and rank curve to be designed. |
| **Strafe** | **USER-SELECTED NEW concept** | Temporary faster firing at cost of move speed. Floated +2% AS/rank, duration `ceil(1+r/5)` sec; exact numbers and penalty pending. |
| **Ranged Damage Aura** (name pending) | **USER-FLOATED, TENTATIVE** | +1% ranged damage/rank for nearby eligible allies, radius and stacking/eligibility undecided. Might be passive or active. |
| **Power Shot** (name pending) | **USER-FLOATED, TENTATIVE** | Heavy piercing arrow through multiple enemy units along a line, each subsequent enemy receives reduced damage. Targeting/falloff undecided. |

The obvious six-slot working draft is these **six identities** (Aimed, Rain, Mastery, Strafe, Aura, Power Shot), but **the user said "maybe"** for Aura/Power Shot; **do not claim final approval of all six**, exact caps, level gates, costs, attack reach for non-basic skills or the entire roster. Ranger's past Exploding Arrow/Mark/Hawk/Evasive mechanics remain playable on the **preserved Tier II Ranger parent** only.

### Boundaries relevant to implementation

- **Ranger original unchanged:** Tier II Ranger normal basic reach11m and mastery +0.1m/rank; no alteration to its Hunter's Mark, Hawk Dive, Exploding Arrow and Evasive Step.
- **Marksman Level1 start:** new Tier III class manifestation after eligible Ranger Lv60, preserved Ranger parent; no inherited old ranger skill ranks.
- **Stat budget:** must establish own stronger starting attributes and overall stat growth, ideally AGI-heavy, based on actual Ranger base and growth in opening section. Do not borrow Knight/Mage stats directly.
- **Range scaling:** 11m base and 15m full-skill ceiling. Autoattack reach and Aimed Shot reach can be aligned as in parent, but Power Shot length, Arrow Rain placement and aura radius are authored per skill, not automatically equal to bow auto range.
- **Stationary firing:** despite ability name "Strafe", Marksman does NOT automatically gain the ability to fire while moving.
- **No implementation yet:** no Unity classes/assets/scenes/quests, builds, code merges or gameplay balance changes were requested. This round is documentation-only.

### Round 2: FIVE focused questions (OPEN for user)

1. **Strafe slowing and uptime:** Should Strafe reduce movement speed by a **flat 30%**, **50%**, or **root the Marksman for the buff duration**, and do you want to keep the floated **+2% attack speed per rank**, `ceil(1+rank/5)` second duration? A movement penalty while shooting stationary mainly affects repositioning, not shots themselves.
2. **Ranged aura detail:** Do you want the **Ranged Damage Aura as a permanent passive (with perhaps 12m radius),** an activated timed party buff, or to remove the aura idea? And should it affect eligible **ranged autoattacks only**, or both physical ranged basic attacks **and ranged physical attack skills**? Avoid auto-including all spell damage.
3. **Power Shot falloff:** Keep the proposed piercing line skill? If yes, should each additional enemy take e.g. **80% of previous enemy's damage** (100%,80%,64%,...) or subtract a **flat 15 percentage points per enemy** (100%,85%,70%,...) with a floor? What should happen when it hits a wall? As usual, no ability to pass solid obstacles without specific approval.
4. **Aimed Shot without Hunter's Mark:** Should the evolved shot become simply **a harder-hitting direct AGI-scaled arrow** or a **long windup precision attack with bonus critical chance/defense penetration**? Retain the parent's Bow Mastery crit permission only if chosen; no invented automatic guaranteed crit.
5. **Tier III six-skill plan and progression:** Does the roster **Aimed Shot / Arrow Rain / Bow Mastery / Strafe / Ranged Aura / Power Shot** feel right as the six Marksman abilities, and should we use **one rank point per Marksman class level through 150**, as with Psionic Knight/Mirage Mage? Exact caps/gates would then be designed and audited, not copied from the Psion class.


## Round 2 — 2026-11-25 (LATEST FIVE USER-CONFIRMED DECISIONS)

This round answers ALL FIVE Round 2 questions and converts the tentative aura and Power Shot into **CONFIRMED CORE MARKSMAN ABILITIES**. Older Round 1 text saying the *roster itself* is tentative is superseded here. No Unity implementation is authorized by the design interview.

| ID | Design choice | USER-CONFIRMED | Specific unanswered tuning |
| --- | --- | --- | --- |
| MARKSMAN3-001 | **Strafe speed-for-mobility exchange** | During Strafe, the **movement-speed percentage reduction equals the ATTACK-SPEED percentage increase** at every purchased Strafe rank. Example ONLY with the previously floated `+2% AS/rank`: rank 10 grants **+20% attack speed and −20% move speed** while active; rank 20 grants **+40% AS and −40% MS**. It is a **timed self-buff**, not permission to shoot while running. **No root or independent fixed 30/50% penalty**. | The exact attack-speed growth `+2%×rank` and `ceil(1+rank/5)` seconds were floated by user in Round1 and left unaltered in Round2, but were **not independently reconfirmed in the text of this answer**; treat as working numerical candidates for explicit approval in Round3. Skill rank cap, unlock, cooldown, Mana cost, GCD, reactivation and move-speed floor also open. |
| MARKSMAN3-002 | **Permanent Ranged Damage Aura with LIVE range from Bow Mastery** | Aura is **PERMANENT PASSIVE**, not a temporary activation. Its **RADIUS MUST EQUAL MARKSMAN'S CURRENT EFFECTIVE ATTACK RANGE**, not a fixed 12m. At 0 Bow Mastery ranks it follows **11m**, and when the Marksman reaches the approved max bow reach it is **15m**. Any Bow Mastery range increase must **update aura radius dynamically**, without a separate aura rank range requirement. User initially proposed aura strength **+1% ranged damage per purchased aura rank**, retained as the proposed starting formula. | Does it affect **ranged Physical basic hits only, or ranged Physical abilities too**? Does it include caster self, friendly players, owned summons; same-name auras strongest-only vs stack? Damage channel semantics, line-of-sight, effect only while caster alive, rank cap and UI indicator open. **Do NOT buff all spells just for being cast from afar**, and do not silently fix eligibility. |
| MARKSMAN3-003 | **Power Shot piercing 80% compound falloff** | User expressly **APPROVES** a projectile damaging successive **ENEMY ACTORS in a line**, with each next monster hit for **80% of the prior monster's RAW projectile hit** in collision order. Thus for first enemy raw D, next values are D, 0.8D, 0.64D, 0.512D, 0.4096D, ...; mathematically `rawHit(n) = D×0.8^(n−1)`, for **n=1** first damaged enemy, subject to each target's own defense/mitigation afterward. Not a flat 20-percentage-point decrease and not reamplified by defenses of earlier victims. Each victim hit once per projectile. | Exact base D/rank/AGI, projectile length/width/speed, obstruction collision, number of pierces, aiming mode, cooldown, Mana, crit, PDEF penetration, line-of-sight, whether projectile can hit destructible terrain OPEN. Do not infer wall penetration; default world-collision convention is preserved until decided. |
| MARKSMAN3-004 | **Aimed Shot upgraded precision identity** | Aimed Shot is a **LONGER-WINDUP PRECISION BOW SHOT** with **INCREASED CRITICAL CHANCE and PHYSICAL DEFENSE PENETRATION**. It is no longer a mark-reliant Ranger skill; there is **NO Hawk Dive / Hunter's Mark** for Marksman. Preserve normal Physical arrow damage classification, AGI-oriented scaling and stationary attack behavior. | Exact windup duration, bonus critical chance/rank, crit multiplier (inherit Bow Mastery 1.4×? stack chance?), PDEF penetration (flat points vs percent reduction), damage formula, Mana/CD and whether this shot can miss remain OPEN. Never silently import parent marked-target multiplier or give auto-guaranteed crit. |
| MARKSMAN3-005 | **COMPLETE SIX-SKILL MARKSMAN ROSTER and exact TOTAL progression** | User **APPROVES ALL SIX SKILL IDENTITIES:** **Aimed Shot, Arrow Rain, Bow Mastery, Strafe, permanent Ranged Damage Aura, Power Shot**. Class earns **ONE spendable skill-rank point at EACH Tier III LEVEL 1–150, exactly 150 total**, just as with Psionic Knight/Mirage Mage. Skills begin at rank0 and purchase ranks subject to later class-authored level eligibility gates. | **INDIVIDUAL six rank caps, first unlock levels and 150 exact per-rank eligibility gates NOT YET APPROVED**; must be designed and audited (sum caps=150; for every level L ensure sufficient eligible skill ranks to spend up to L; all rank-150 purchases reachable by Lv150). No inherited Tier II Ranger purchased ranks. |

### Confirmed functional kit (supersedes Round1 tentative entries)

| Skill | Type | Core effect |
| --- | --- | --- |
| Aimed Shot | Active Physical ranged precision attack | **Long windup**, **extra crit chance**, **Physical Defense penetration**; independent of any mark |
| Arrow Rain | Active stationary channel / ground damage | Evolves existing Ranger's multiwave Arrow Rain; detailed Tier III ranks/output open |
| Bow Mastery | Passive | Evolves Ranger's AGI/crit/range expertise; extends basic bow range **11m→15m**, and the aura automatically inherits that reach |
| Strafe | Timed self-buff active | Attack speed +X%, movement speed **−X% same magnitude** while active; proposed X=2%×Strafe rank, duration `ceil(1+r/5)` seconds until finalized |
| Ranged Damage Aura (working name) | **Permanent passive aura** | +1% ranged damage / purchased rank **proposed starting strength**; radius = **CURRENT MARKSMAN BASIC BOW ATTACK RANGE**, dynamically 11m→15m with Bow Mastery. Which ranged events/allies are eligible is open |
| Power Shot | Active piercing Physical line attack | Each successive enemy along shot line receives **0.8^(n−1)** of first target RAW, then normal per-target mitigation; precise geometry and output open |

### Aura and range implementation invariant (user-approved)

`MarksmanAuraRadiusMetres = MarksmanEffectiveBasicBowAttackRangeMetres`.

With normal Marksman class bow reach **11m** at 0 Bow Mastery and approved **15m** maximum by max Bow Mastery, a working interpolation can be `11 + 4×(purchasedBowMasteryRank / maxBowMasteryRank)` metres. **Exact mastery rank cap/range increments are not yet approved**. Aura's range must NOT be a disconnected hard-coded 12m value, nor follow a weapon's "longbow" flavor text independently of class-authoritative bow reach. The aura has its OWN rank for potency, independent of mastery rank's influence on its radius. Both shortbows and longbows are equippable; weapon item family does not unilaterally change range.

### Five Round 3 questions (OPEN, not user-approved)

1. **Lock Strafe numbers:** Confirm `+2% ATTACK SPEED per Strafe rank`, exactly matching `−2% MOVE SPEED per rank`, and `ceil(1 + rank/5)` seconds. How should Strafe refresh or recast while active, and should cooldown begin on activation?
2. **Ranged aura eligibility and stacking:** Should +1% ranged damage/rank include **Marksman's own ranged basics and physical arrow skills**, allied ranged heroes' basics/skills, and **ranged summoned units**? Should same-name Marksman aura sources use the **highest rank only** like existing strongest-only named auras?
3. **Aimed Shot precision detail:** Does crit bonus start from Bow Mastery's ordinary crit chance **PLUS an extra Aimed-only chance per Aimed rank**, or does Aimed Shot set/override its own crit chance? Choose **flat PDEF ignored**, **percentage PDEF penetration**, or some hybrid; consider bosses.
4. **Power Shot skill geometry:** What **maximum line length** (e.g. same 11→15m range, or extra reach), **line width** (e.g. 1–2m), and wall collision rule should it use? Should Power Shot share the Marksman's optional Aimed Shot bonus crit/penetration, or stay a separate noncritical Physical skill?
5. **150-point skill allocation:** Initial designer suggestion to review: **Aimed Shot34 / Arrow Rain28 / Bow Mastery20 / Strafe28 / Ranged Damage Aura16 / Power Shot24 =150**; Bow Mastery20 gives exactly **+0.2m per purchased rank**, reaching15m. Strafe28 with floated +2%/rank gives +56% attack speed/−56% move speed and `ceil(1+28/5)=7s` at cap. Aura16 implies a 16% potential ranged damage group aura (significant! check raid/party balance). Do you want those caps or to redistribute? Need subsequently author and audit exact eligibility gates.

**Important:** The parent Ranger's original class skills, 80 ranks, mark and 11m basic range remain unchanged. The new six-skill roster is user-approved, but exact costs, damage, crit/defense, level1 base stats/growth, Mana profile, visuals, advancement teacher/quest and Unity implementation remain unapproved/not implemented in this design round.


## Round 3 — 2026-11-25 (LATEST USER-APPROVED CAPS, AURA, STRAFE AND SHOT BEHAVIOR)

All five user answers are incorporated. The user's first answer is the **sole authoritative rank-redistribution instruction**, and answer 5 explicitly points back to it. This section **supersedes** earlier Round 2 statements suggesting bonus Aimed Shot critical chance, possible aura buffs to ranged *skills*, Power Shot using identical autoattack range, or previous 34/28/20/28/16/24 proposed caps.

### Exact user-selected ranks: 150 total

Starting designer proposal was **Aimed34 / Arrow Rain28 / Bow Mastery20 / Strafe28 / Aura16 / Power Shot24**.
Apply user's movements **in order**:

1. Increase **Strafe 28→30** by moving **2 from Aimed Shot 34→32**.
2. Move **2 MORE from Aimed Shot 32→30** to **Ranged Damage Aura 16→18**.
3. Move **2 from Arrow Rain 28→26** to **Ranged Damage Aura 18→20**.
4. Move **1 from Arrow Rain 26→25** to **Power Shot 24→25**.
5. Bow Mastery unchanged at **20**.

| Ability | USER-APPROVED FINAL MAX RANK | Status |
| --- | ---: | --- |
| **Aimed Shot** | **30** | Ranged Physical precision attack, longer windup, percentage PDEF ignore, normal crit |
| **Arrow Rain** | **25** | Ground multiwave Physical ranged ability, no mark requirement |
| **Bow Mastery** | **20** | Range AGI/crit passive; **+0.2m base attack reach PER purchased mastery rank** (11m at0, 15m at20); keep ordinary ranged basic + Aimed crit permission |
| **Strafe** | **30** | Speed-for-mobility timed self buff |
| **Ranged Damage Aura** | **20** | Permanent ranged basic-attack damage aura, dynamic radius, strongest same-name only |
| **Power Shot** | **25** | Piercing line Physical ranged skill, 1.5m wide, solid-wall stops, auto range+1m |
| **TOTAL** | **150** | **One spendable class skill point per Tier III Marksman level1–150** |

The **individual purchased-rank level eligibility gates and ability first-unlock levels are NOT YET APPROVED**. Design/audit them separately using approved caps, with strict rank order and spendability at every class level1–150. None of Ranger Tier II's historical 80 skill purchases transfer automatically into Marksman.

### MARKSMAN3-006 — Strafe: numerical speed/duration now USER APPROVED

The user says "**Strafe should have 30 levels, but keep the values**", referencing the previously proposed `+2% attack speed/rank`, its exactly matching movement-speed penalty and `ceil(1 + purchasedRank/5)` seconds. These are now CONFIRMED for first playtest:

- For purchased Strafe rank **r=1..30**, **attack speed +2r percentage points**, **movement speed −2r percentage points** for **`ceil(1+r/5)` seconds**.
- At rank1 **+2% AS/−2% MS 2s**, rank10 **+20%/−20% 3s**, rank20 **+40%/−40% 5s**, rank30 **+60%/−60% 7s**.
- It does **not** allow attacking while walking/running; Marksman remains a stationary archer. Slowing is actual movement-speed reduction if repositioning during the buff, not a root. Percentages apply to ordinary speed modifier pipeline (exact stacking with other sources / speed floors under global movement rules).
- Cooldown, Mana cost, animation, interruption, reactivation and interaction with attack-speed caps are **still OPEN**, and should not be fabricated as already approved.

### MARKSMAN3-007 — Permanent aura: ALL allied ranged ATTACKS, NO SKILLS, strongest-only

- Permanent **passive** Ranged Damage Aura potency was originally **suggested at +1% ranged attack damage per aura skill rank**; Round3 confirms **what it affects and how it stacks**, not an explicitly final numerical percentage. Treat `+1%×auraRank` as the retained FIRST-PLAYTEST POTENCY CANDIDATE pending final signoff, yielding possible **+20%** at aura rank20.
- It affects **ranged BASIC/AUTOATTACK damage from ALL eligible allied units with ranged attacks** within range. This includes other allied ranged heroes and appropriate allied ranged summons/units, and may include owner Marksman's ranged normal attacks under standard friendly self-inclusion. Ordinary autoattack definition governs eligibility rather than whether the actor happens to be standing far away.
- **NOT SKILLS**: No buff to Aimed Shot, Arrow Rain waves, Power Shot, Exploding Arrow or any other active ability damage; no automatic spell damage buff, healing, ground DoTs or ranged spell damage merely because skill uses a projectile. If an ability calls a generic basic attack event, the implementation must distinguish the **actual BASIC hit from skill-specific damage** to avoid double benefits; exact engine semantics should be tested.
- **NONSTACKING:** Multiple sources of the **same named Ranged Damage Aura** in overlapping range grant **ONLY THE STRONGEST eligible effect**. Prefer highest active damage bonus magnitude/rank, with deterministic tie selection; no additive/multiplicative stacking of identical aura names.
- Radius is always the **current Marksman BASIC/AUTOATTACK reach**, not a separately authored fixed 12m aura. Bow Mastery r=0→20: `basicReach=11+0.2r` metres, `auraRadius=basicReach` (11m→15m), live updated on passive rank purchase/equipment/stat changes where ordinary class effective range applies. Aura strength rank is independent of Bow Mastery rank.

### MARKSMAN3-008 — Aimed Shot: NO SPECIAL CRIT, percentage-based Physical Defense penetration

- Round2's "**increased critical chance**" was **EXPLICITLY REVERSED**: user changed mind and says **"not improved crit chance or crit bonus, just have the normal crit chance and crit damage apply to the shot as well"**.
- Hence Aimed Shot can critically hit using **its usual applicable crit chance and crit damage**, including the ordinary Bow Mastery opt-in crit rules as they apply to the basic/Aimed pair; **NO Aimed-Shot-specific bonus chance and NO Aimed-Shot-specific crit damage multiplier**.
- Retain **LONGER WINDUP** and meaningful **PERCENTAGE of target Physical Defense ignored**, not flat PDEF points. Working high-rank balance target: **around 10% of target PDEF ignored at Aimed Shot rank30**, as user said "**like 10% at max rank or something**". Exact rank curve, whether r1 starts at 0/0.33/other and effects of boss resistance are **NOT YET STRICTLY LOCKED NUMERICALLY**. Suggested candidate `PDEF_ignoredPct(r)=0.10*(r/30)` for r1..30 (1/3 percentage point per rank) pending review. Do not silently change Physical Defense with a permanent debuff; this is a per-Aimed-Shot mitigation calculation.
- Damage remains **ranged Physical AGI-based**, independent of Hunter's Mark/Hawk Dive. Exact damage formula, whether default accuracy-miss rules apply, cooldown, Mana cost and windup length remain to be designed.

### MARKSMAN3-009 — Power Shot: 1m extra reach, 1.5m wide, blocked by solid walls

- **Base Power Shot reach 12 metres**, **current autoattack reach +1 metre**, extending with identical Bow Mastery EXTRA RANGE; with mastery rank0→20 at +0.2m/rank, `PowerShotReach=12+0.2×BowMasteryRank`, hence **12m→16m**. This does **NOT** increase Marksman's normal basic attack range beyond its fixed 15m maximum.
- **PROJECTILE WIDTH = 1.5 metres** along its damaging line (precise capsule/rectangle hit detection and collision tie priority follow shared combat geometry when implemented).
- **SOLID WORLD WALLS STOP the projectile**. No piercing through solid cover, cliffs or blocking terrain; no hit behind the first solid obstruction. Does pierce successive ENEMY UNITS in unobstructed line.
- Approved previous compound falloff remains unchanged: sequential distinct enemies in collision order receive `rawHit(n)=BasePowerShotRaw×0.8^(n-1)` (first100%, second80%, third64%, fourth51.2%, etc.); each victim's own Physical Defense and other applicable mitigation apply afterward. No implicit hit more than once per enemy per projectile.
- Mana, cooldown, cast/windup/channel timing, initial raw damage/rank AGI scaling, projectile speed, crit eligibility and maximum units pierced remain open. If changing projectiles from aiming at an enemy to a cursor direction requires design, choose and confirm later; do not author new shooting controls here.

### Resulting six-skill design summary, prioritizing current decisions

- **Aimed Shot r30:** stationary longer windup, regular eligible crit and crit multiplier, percent PDEF ignored (~10% at cap as target), no Hunter's Mark.
- **Arrow Rain r25:** retain the stationary sky-arrow channel and successive area waves as the class foundation; stronger Tier III formulas to design.
- **Bow Mastery r20:** AGI/crit/range passive; **11m +0.2m per rank** to 15m; buffs radius of the aura and length of Power Shot by exactly the range gained.
- **Strafe r30:** **+2%AS/−2%MS/rank**, duration `ceil(1+r/5)` secs (cap +60/−60 for7s).
- **Ranged Damage Aura r20:** permanent, all allied ranged BASIC attacks only, never active-skill damage; strongest same-name aura wins; radius11m→15m.
- **Power Shot r25:** Physical piercing line, width1.5m, effective reach12m→16m, stops at walls, raw per victim D,0.8D,0.64D,...

### Round 4 next FIVE open design questions (NOT YET USER APPROVED)

1. **Aimed Shot percent PDEF:** Shall we lock exact **`PDEF ignored = 10%×(AimedShotRank/30)`**, reaching 10% at rank30? Or prefer, e.g., **5% at rank1→20% at rank30**? What windup length should it have, e.g. 1.0s or 1.5s?
2. **Bow Mastery AGI + crit:** Should it retain Tier II's **+2 AGI and +1 percentage point applicable basic/Aimed crit chance PER mastery rank** with the same **1.4× crit damage**, or increase this as a Tier III evolution? (+0.2m range/rank is already fixed.)
3. **Aura potency:** Finalize the originally floated **+1% ranged BASIC damage per aura rank**, rank20=**+20%** for eligible ranged basic attacks? Should the Marksman himself benefit (default recommendation: YES), alongside all other allied ranged attackers/summons?
4. **Power Shot damage and criticals:** Should it be a **heavy AGI-scaling Physical hit without crit** (simpler), or use normal applicable crit on each enemy? Does its target mitigation occur separately after each 80% raw reduction (current approved interpretation)?
5. **First-unlock levels / progression:** Shall the six skills become available at **Aimed Shot Lv1 / Bow Mastery Lv1 / Strafe Lv1 / Arrow Rain Lv5 / Power Shot Lv7 / Aura Lv10**, then design and audit **all 150 per-rank level eligibility gates** against the approved 30/25/20/30/20/25 caps? Other unlock level arrangements welcome.

**Source and scope:** these decisions are from user Round3, not Unity implementation. Previous Round2 history remains for audit, but superseded statements should NOT appear as current design. Advance Greatblade→Berserker only AFTER the Marksman design is completed, following [coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md).

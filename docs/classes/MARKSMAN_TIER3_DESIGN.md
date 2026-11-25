# DiceFree — Marksman (Tier III Ranger evolution) — design interview

**2026-11-25 — ROUNDS 1–7 USER DECISIONS RECORDED; ROUND 8 OPEN.** User explicitly selected **Ranger → Marksman** as the FIRST Tier III evolution to design in the new **at least one Tier III per Tier II class** coverage pass. **Greatblade → Berserker follows next**, then choose first evolution for the five other uncovered Tier II parents. This is an AUTHORIZED DESIGN INTERVIEW, **NOT Unity code/asset/build implementation permission**.

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


## Round 4 — 2026-11-25 (LATEST USER ANSWERS; Aimed, Mastery, Aura, Power Shot and unlock levels)

The user has responded to **all five** Round 4 questions. These **supercede prior historical** remarks saying Aimed Shot % PDEF, aura +1%/rank, self-inclusion, noncrit Power Shot, and first unlock levels remain unapproved. The exact **"15x%"** notation in the Bow Mastery answer, however, **needs clarification rather than silently storing a numeric crit chance**.

### Five responses and implementation consequences

| ID | Latest user decision | Status / detail |
| --- | --- | --- |
| **MARKSMAN3-010** | **Aimed Shot ignores `10% × (purchasedAimedShotRank / 30)` of the target's current Physical Defense, and has an exact 1.5 SECOND windup.** | **USER APPROVED**. At r1 ignore `0.333333...%` PDEF, at r15 `5%`, at r30 `10%`. During this specific Physical shot resolve effective `PDEF = targetPDEF × (1 − 0.1r/30)` before ordinary Physical mitigation; no permanent debuff to target, no effect on other attacker damage or Magical packets. Windup 1.5s, stationary and cancels/interrupts according to class spell/combat channel conventions. Normal Bow-Mastery-eligible crit applies, with **no added Aimed-only crit chance or damage**. Original Ranger Hunter's Mark conditional bonus does not carry over. Actual Aimed Shot raw damage / AGI scaling / Mana / cooldown are still OPEN. |
| **MARKSMAN3-011** | **Bow Mastery gains +3 AGI per purchased rank** instead of parent Ranger +2. The user typed **"15x% crit per rank"** in place of the previously proposed +1 percentage point/rank. | **+3 AGI/rank USER APPROVED**: cap20 provides +60 AGI. **CRIT GROWTH NUMERIC UNRESOLVED**: preserve the exact user wording `15x% crit per rank` until confirmation; plausible intended reading `+1.5 percentage points per rank` (cap20 +30pp), but it could be a different multiplier or notation. **Do not encode +15%, +1.5%, 15×something, or an arbitrary chance in Unity until clarified**. Retain parent-style **eligible BASIC and Aimed Shot crit permission** and parent **1.4× crit damage** as working defaults because user did not change crit multiplier. Bow Mastery already approved +0.2m reach/rank, cap20=+4m from11m→15m; aura radius and Power Shot reach scale in tandem. |
| **MARKSMAN3-012** | **Permanent Ranged Damage Aura +1% damage PER purchased aura rank**, with the **Marksman itself included**, plus all other eligible allied ranged basic attackers. | **USER APPROVED**. At aura rank20 grants **+20% ranged BASIC ATTACK DAMAGE**. This is not bonus to Aimed Shot/Arrow Rain/Power Shot or any skill damage. Other allies with ranged ordinary basic attacks (including summoned ranged allies) qualify under pre-existing friend/attack semantics. Dynamically radius=Marksman's current autoattack range11m+0.2m×BowMasteryRank, capped15m. Multiple same-name aura instances **strongest only** with no stacking. Initial aura is permanent passive, not activatable. |
| **MARKSMAN3-013** | **Power Shot is NONCRITICAL.** | **USER APPROVED**. No critical chance roll or crit damage multiplier is applied to Power Shot as an ability, even if attacker has Bow Mastery/basic crit benefits. Its ordinary Physical projectile still pierces sequential enemies (first100%, next80%, next64%...), width1.5m, solid walls stop projectile, reach=12m+0.2m×BowMasteryRank (12m→16m), each target mitigates own PDEF after per-enemy RAW falloff. Distinguish from Aimed Shot, which IS eligible for basic-style crit. |
| **MARKSMAN3-014** | **First-availability levels approved exactly**: `Aimed Shot Lv1`, `Bow Mastery Lv1`, `Strafe Lv1`, `Arrow Rain Lv5`, `Power Shot Lv7`, `Ranged Damage Aura Lv10`. | **USER APPROVED**. Skills start at purchased rank0 and become purchasable at their unlock levels. Maintain exact previously approved rank caps Aimed30, Rain25, Mastery20, Strafe30, Aura20, Power25 =150, and one skill rank point per Marksman class level1–150. Full increasing purchase eligibility gates beyond first ranks are STILL TO DESIGN AND AUDIT. |

### Authoritative currently accepted Marksman six-skill first-rank/unlock baseline

| Skill | Max rank (user approved) | First-available level (user approved) | Latest identity |
| --- | ---: | ---: | --- |
| **Aimed Shot** | 30 | **1** | 1.5s longer precision windup, `r/300` fraction PDEF bypass (10% max), normal eligible crit (Bow Mastery), no mark |
| **Arrow Rain** | 25 | **5** | Tier III improved multiwave AoE; inherited Ranger flow unless individually redesigned |
| **Bow Mastery** | 20 | **1** | +3 AGI/rank, +0.2m bow basic reach/rank; **crit growth must be clarified due to "15x%"**. Parent basic/Aimed eligible and 1.4× crit multiplier default unless user revises |
| **Strafe** | 30 | **1** | +2% AS and −2% movement speed per rank; `ceil(1+r/5)` seconds |
| **Ranged Damage Aura** | 20 | **10** | PERMANENT +1% per rank ranged basic attack damage incl self/allies, strongest same-name only; radius11m→15m with Mastery |
| **Power Shot** | 25 | **7** | NONCRITICAL Physical piercing line; width1.5m, 12→16m range, blocked by walls, each next target 80% previous RAW |

### Still to design after current round

**Crit clarification needed:** Did `15x% crit per rank` mean **+1.5 percentage points critical chance per Bow Mastery rank**? If yes, +30 percentage points at Mastery rank20 and no new crit damage multiplier; otherwise record user's clarified formula exactly.

Other OPEN areas: full 150 individual rank-eligibility gates (must be spendable at every level and first rank not before skill unlock), Marksman Level1 attributes/growth/HP/Mana and costs vs Tier II Ranger, Aimed/Power/Rain AGI damage formulas, Strafe duration/AS cost and cooldown, Aimed aim/miss/interruption, Power projectile behavior and boss/arena balance, plus hidden mentor/trial. Do not modify any Unity game files on the basis of a design interview.

### Round 5 questions for next user response (UNANSWERED)

1. **Bow Mastery crit clarification:** Does `15x% crit per rank` mean **1.5 percentage points added crit chance per Bow Mastery rank** (max +30pp), with the existing 1.4× critical damage multiplier? If not, what exact chance formula?
2. **Arrow Rain Tier III:** Keep **1.5s channel, 4 waves over2s and 3m circle**, upgraded AGI/rank damage, or should Marksman gain more waves, a wider circle, or longer channel?
3. **Aimed Shot vs Power Shot:** Should Aimed's 1.5s windup and projectile hit **ever miss** under normal accuracy/evasion? Should Power Shot be direction/cursor-aimed and whether it can miss an individual enemy after geometry collision remain to decide.
4. **Strafe cooldown/resource:** How much cooldown would feel good, e.g. 18s/24s/30s? Should its buff activation cost one Mana payment (rank-scaled) rather than Mana per autoattack?
5. **Tier III AGI/stat growth:** Knight and Mirage Mage used stronger starting attributes and total growth than parent Psion. Would you like the Marksman to be similarly **AGI-heavy**, with modest VIT, STR, INT and SPR? Next designer can propose exact Lv1 stats and per-level gains from the actual Ranger values.

**All approvals above are user-decided design specifications, not a claim that Marksman is implemented or tested.** Existing Ranger Tier II asset, Bow Mastery, Hawk Dive/mark, abilities and old skill ranks remain unchanged on their separate saved parent manifestation.


## Round 5 — 2026-11-25 (LATEST USER APPROVAL: crit coefficient, Arrow Rain six waves, guaranteed hits, Strafe cost/CD and AGI growth)

The user answered all FIVE Round 5 questions. This round **SUPERSEDES** previous Round4 “15x% crit ambiguous” language and prior 1.4× critical-multiplier assumptions. All approved core facts below are first-playtest design requirements, **not claims of Unity implementation**.

| ID | Latest USER-APPROVED decision | Exact implementation target / guardrail |
| --- | --- | --- |
| **MARKSMAN3-015** | The earlier “15x% crit per rank” typo meant **+1.5 percentage points critical chance per Bow Mastery purchased rank**, and change its critical damage to **1.6× total raw crit multiplier**. | **Bow Mastery rank r (1..20): +1.5r percentage points crit chance**, cap **+30pp** at rank20. Applies to eligible **ordinary RANGED BASIC attacks and Marksman's Aimed Shot**; neither Power Shot (explicitly NONCRITICAL) nor Arrow Rain gains crit permission. **Crit multiplier for those eligible attack events: 1.6×**, NOT Ranger's Tier II 1.4×; ignore the previous numeric ambiguity and all historical tentative 15x / 15%. Together with Bow Mastery r×3 AGI and +0.2m bow reach/rank, r20 gives **+60 AGI, +30pp critical chance, +4m bow range**. Other crit sources add under existing standard chance rules, normal caps/defenses apply; no extra *Aimed-only* crit chance or crit damage. |
| **MARKSMAN3-016** | **Arrow Rain becomes SIX damage waves OVER TWO SECONDS**, instead of Ranger's four. | Retain **existing 1.5 SECOND stationary “big arrow then small arrow skyward” channel** and existing **3m RADIUS** because no change to channel/radius was requested. Only after the full channel resolves, **6 ground waves over 2s**, affecting targets in circle at each wave; movement/stun/explicit interrupt during channel cancels ALL waves, and after successful release the six waves persist autonomously even if Marksman moves or is stunned. Exact 6-wave tick timestamps/intermediate spacing follow an explicitly chosen implementation convention; do not silently increase the circle or channel. **Damage PER wave and aggregate six-wave DPS need new Tier III tuning**; cloning parent 4-wave damage coefficients to all6 would increase total damage 50%, before rank/attribute changes. |
| **MARKSMAN3-017** | **Aimed Shot is a SKILL and CANNOT MISS**; **Power Shot hits every enemy that physically intersects its projectile** subject to terrain collision. | Aimed Shot is a targeted Physical precision **ability** with guaranteed accuracy against an eligible valid target (no ordinary basic-attack miss/evasion roll); the projectile still needs valid targeting and usual interruption/obstacle rules. Its 1.5s stationary windup and per-rank 10%-at-cap PDEF mitigation penetration are unchanged; criticals still eligible as specified above. For Power Shot: cursor/direction line trace hits each distinct VALID enemy actor whose hurtbox intersects the **1.5m-wide** projectile, up to **12m+0.2m×BowMasteryRank (max16m)**, and stops at **first solid blocking world wall/obstacle**. There is **no additional accuracy/evasion roll** that can negate a valid unobstructed geometric hit. A collider absent from the line is NOT hit, unlike targeted Aimed Shot; ordered victim raw damage stays `D×0.8^(hitIndex−1)`, then each target's own physical mitigation; Power Shot remains NONCRITICAL. No through-wall shots. **Do not globally rewrite other classes' abilities from “it's a skill so cannot miss” without a separate global-system decision**. |
| **MARKSMAN3-018** | **Strafe cooldown is exactly 18 SECONDS**, and uses **one rank-scaled Mana payment on ACTIVATION**, not per autoattack. | Existing approved `+2%AS/−2%MS per purchased Strafe rank` for `ceil(1+r/5)` seconds remains. CD18s from activation as standard skill CD unless combat engine defines a consistent alternative; no ongoing Mana drain and no Mana per ranged basic hit. **Exact Mana amount/formula is OPEN**; next design round should decide / delegate it, avoiding “no Mana cost” or phantom per-hit taxation. Aimed/Arrow/Power Skill Mana/CD are separate decisions. |
| **MARKSMAN3-019** | Marksman Tier III **MUST have stronger starting attributes and higher per-level total attribute growth than actual Tier II Ranger, AGILITY-heavy with meaningful Vitality**. | Actual parent Ranger asset: **VIT13 STR8 AGI18 INT7 SPR9** at level1; growth **+1.5 VIT/+0.7 STR/+3.1 AGI/+0.8 INT/+1.15 SPR**, total **+7.25 attributes/level**. Set **new** Marksman level1 base total and total growth **higher** than parent, dominate with AGI and secondarily VIT, and separately plan HP and Mana; do not inherit Ranger level60 accumulated attributes or copy Psion Tier III stat profiles. **Precise numerical Marksman base stats and growth are NOT yet user approved**. |

### Latest six-skill spec snapshot (supersedes older rounds where conflicting)

| Skill | Approved max rank | Approved first unlock | Current approved effects | Outstanding |
| --- | ---: | ---: | --- | --- |
| **Aimed Shot** | **30** | **1** | Physical targeted guaranteed hit, **1.5s windup**; PDEF ignored = `10%×(AimedRank/30)`; eligible **+1.5pp/rank Bow Mastery** chance, **1.6× critical damage** on crit, no Aimed-only crit bonuses or Hunter's Mark | Base damage/AGI rank scaling, cooldown, Mana, hit validation/LoS details |
| **Arrow Rain** | **25** | **5** | Stationary 1.5s channel, **6 waves over2s**, 3m circle; channel interruption by move/stun/explicit interrupt, ground effect persists after successful channel | New six-wave rank/AGI damage and cast cost/CD, precise six-tick timing |
| **Bow Mastery** | **20** | **1** | **+3 AGI/rank, +1.5pp crit chance/rank** for basic+Aimed, **1.6× total crit damage**, +0.2m basic bow reach/rank (11m→15m). Dynamically also scales aura radius and Power Shot reach | Rank gate schedule; otherwise numeric fundamentals now resolved |
| **Strafe** | **30** | **1** | **+2% AS and −2% MS per rank** for `ceil(1+r/5)` seconds; cooldown **18s**; **one rank-scaling Mana payment upon activation**, never per-arrow | Exact Mana formula, recast/expiry effects, speed-clamp behavior under core speed rules |
| **Ranged Damage Aura** | **20** | **10** | Permanent **+1% ranged BASIC attack damage/rank**, affects owner and all eligible allied ranged basic attackers/summons, excludes skills; highest same-name effect only; live radius11→15m according to Bow Mastery | Exact interaction with weapon item attack modifiers, mitigation ordering, valid ally edge cases |
| **Power Shot** | **25** | **7** | NONCRITICAL Physical piercing projectile, **guaranteed hit** against intersected valid enemy geometry, 1.5m width, 12m→16m range via Mastery, **first solid wall stops**, hit falloff `D×0.8^(n−1)` per distinct enemy in contact order then own PDEF | Base damage/rank/AGI, Mana/CD, projectile speed/targeting API and collision tie ordering |

**Total** = **150 spendable Marksman skill points**, one per Tier III Marksman class level1–150. EXACT per-rank level-eligibility purchase gates still need implementation design and arithmetic audit; no points transfer from parent's Ranger rank80 kit. Class Tier II Ranger level60 → new Tier III Marksman level1, parent preserved, as globally established.

### Suggested preliminary AGI-focused stat profile for Round 6 consideration (DESIGNER PROPOSAL, NOT USER APPROVED NUMBERS)

The user's request authorizes proposing superior Tier III stats, **not choosing the exact levels**. A balanced FIRST-TUNING candidate from Ranger's actual actor:

| Attribute (VIT / STR / AGI / INT / SPR) | Tier II Ranger ACTUAL Lv1 | Tier II Ranger ACTUAL growth | Marksman PROPOSED Lv1 | Marksman PROPOSED growth |
| --- | ---: | ---: | ---: | ---: |
| VIT | 13 | +1.50 | **19** | **+2.00** |
| STR | 8 | +0.70 | **10** | **+0.90** |
| AGI | 18 | +3.10 | **34** | **+6.00** |
| INT | 7 | +0.80 | **9** | **+1.00** |
| SPR | 9 | +1.15 | **11** | **+1.20** |
| **TOTAL** | **55** | **+7.25** | **83** | **+11.10** |

This puts Marksman above its parent in EVERY Lv1 attribute, strongly AGI primary, meaningful VIT but not melee-Knight VIT, still some STR/INT/SPR. It is significantly stronger per-level than Ranger but distinct from the Psionic Knight and Mirage Mage. With this proposal Marksman level150 no-gear would be **VIT317 / STR144.1 / AGI928 / INT158 / SPR189.8**. **Do not claim user approval, playtested balance or Unity implementation.** Starting HP, 15HP per VIT convention, movement speed5, and separate Mana max/regen formulas still need first-test selection. If 6×Arrow Rain plus +30pp crit chance and +20% allied ranged basic aura are too powerful, tune raw skills and rank gates without altering the user's approved core mechanics.

### Round 6: FIVE next questions (OPEN)

1. **Stat profile approval:** Approve the proposed Marksman starting attributes **VIT19 / STR10 / AGI34 / INT9 / SPR11** and per-level growth **+2.00 / +0.90 / +6.00 / +1.00 / +1.20**, or want more VIT/AGI or different secondary stat growth?
2. **Arrow Rain damage:** Six waves are locked. Should Marksman use **6 weaker individual waves** with roughly unchanged overall Ranger-style raw output, or **6 upgraded powerful waves** for deliberately higher Tier III area DPS? We can propose a precise per-wave AGI×rank formula once direction is selected.
3. **Aimed and Power Shot raw damage:** Should Aimed Shot have **much higher single-target raw damage than Power Shot's FIRST target**, while Power Shot is the efficient multi-enemy line skill? Or do you want Power Shot first-target damage competitive with Aimed Shot despite hitting multiple units?
4. **Mana and cooldown selection:** Shall I propose provisional **one-activation Strafe Mana**, plus Mana/cooldowns for Aimed/Arrow/Power, and a separate Marksman Mana max/regen profile matching those costs? Or do you want to set particular numbers?
5. **Rank eligibility & advancement:** Shall I **author and mathematically validate the complete 150-point skill-rank gate schedule** against approved unlock levels, then work on the Marksman hidden teacher / advancement trial? Existing Tier II Ranger's Alpha Wolf quest should not be silently reused as the Tier III challenge.

**Status boundary:** The user approved Round5 mechanics, not numerical Marksman base/growth, skill raw damage, per-rank full gate schedule, Mana formulas or quest. Do not edit Unity or declare implementation done in this design-only round.


## Round 6 — 2026-11-25 (LATEST: USER-APPROVED STATS AND DAMAGE ROLES; DESIGNER'S FIRST NUMERICAL PLAYTEST AND AUDITED RANK-GATE PROPOSALS)

### Five USER-APPROVED answers (MARKSMAN3-020–024)

1. **MARKSMAN3-020 — Exact starting attributes and growth APPROVED.** User explicitly approved the prior Round5 suggested values: **Marksman Tier III Lv1 VIT19 / STR10 / AGI34 / INT9 / SPR11**, and **growth per level VIT+2.00 / STR+0.90 / AGI+6.00 / INT+1.00 / SPR+1.20**. **Total base =83, total growth=11.10 per class level**, from parent Ranger actual base55/growth7.25. These are now USER APPROVED, NOT still proposals. A new Marksman starts at its OWN Lv1 class values, preserving Ranger manifestation and NOT inheriting additive Lv60 parent attributes.
2. **MARKSMAN3-021 — Arrow Rain's SIX waves are a TRUE raw-damage upgrade mainly because there are TWO EXTRA WAVES.** Do not divide the previous four-wave damage evenly into six smaller waves and pretend the longer sequence is stronger. Preserve **1.5-second interruptible stationary channel**, **six waves across two seconds after completion**, and **3m radius**. Start from per-wave damage roughly in line with the parent Ranger's per-wave values, rather than doubling each pulse AND increasing number of pulses. Exact Tier III per-wave scaling remains a designer first-test recommendation, not yet user-approved.
3. **MARKSMAN3-022 — Aimed Shot stronger SINGLE TARGET; Power Shot efficient MULTI-ENEMY line damage.** Aimed should have substantially HIGHER raw first-target damage than Power Shot, even before the Aimed-only eligible 1.6× crit and per-rank 10%-at-cap target PDEF ignore; Power is noncritical and progressively falls off 80% per new enemy. Those damage roles are USER CONFIRMED; the exact multipliers below are DESIGNER-PROPOSED.
4. **MARKSMAN3-023 — DESIGNER AUTHORIZED to present first-playtest Mana costs, cooldowns and Mana pool/regen for the entire kit.** Numeric proposals and plausibility checks below are **NOT YET USER APPROVED**. Strafe's prior 18s cooldown and single rank-scaled Mana payment on activation, rather than per hit, are ALREADY USER APPROVED and remain fixed.
5. **MARKSMAN3-024 — DESIGNER AUTHORIZED to construct, mathematically verify, and present all 150 individual rank eligibility gates**, using exact approved caps/unlock levels, then proceed to teacher/trial design AFTER progression. Tables below are a **DESIGNER PROPOSAL awaiting numeric/gate approval**, not silently locked by permission to author them.

### Locked stat budget and projected no-equipment base values

The ordinary class attribute function is `stat(L)=initialStat+(L−1)×growthPerLevel`, before Bow Mastery, equipment and temporary effects. Parents and child manifestations use separate class level.

| Stat | Tier II Ranger actual Lv1 | Tier II Ranger growth | Marksman USER-APPROVED Lv1 | Marksman USER-APPROVED growth | Marksman Tier III Lv150 BEFORE passives |
| --- | ---: | ---: | ---: | ---: | ---: |
| Vitality | 13 | +1.50 | **19** | **+2.00** | **317** |
| Strength | 8 | +0.70 | **10** | **+0.90** | **144.1** |
| Agility | 18 | +3.10 | **34** | **+6.00** | **928** |
| Intelligence | 7 | +0.80 | **9** | **+1.00** | **158** |
| Spirit | 9 | +1.15 | **11** | **+1.20** | **189.8** |
| **TOTAL** | **55** | **+7.25** | **83** | **+11.10** | **1736.9** |

**Bow Mastery** supplies +3 AGI per purchased rank (USER APPROVED), so at Tier III Lv150 with Bow Mastery20, **effective AGI=928+60=988 BEFORE EQUIPMENT**, not 928. Its +1.5 percentage point critical chance per rank, 1.6× eligible crit damage, and +0.2m range/rank also stay locked.

### Designer's first-playtest basic-attack and ability damage baselines (NOT USER APPROVED NUMERIC FORMULAS)

Notation `r` = purchased rank of THAT specific skill, `A` = CURRENT effective AGI (including Bow Mastery +3 per rank and ordinary gear, excluding temporary unrelated damage multipliers), and `B` = Marksman's uncritical ordinary **Physical ranged bow BASIC RAW damage** before the ranged-damage aura, critical multiplier or target PDEF mitigation.

**PROPOSED Marksman bow basic for early QA:** `B = 8 + 1.20×A` before item contributions; baseline attack interval **1.42s** and windup **0.47s** (Ranger's current rhythm), physical/normal non-elemental, initial reach **11m + 0.2m×BowMasteryRank**, capped15m. Ordinary shortbow/longbow item stats modify bow power/speed but should not make their intrinsic category alter CLASS range. This is a NEW TIER III PROPOSED ACTOR definition, not a rewrite of Ranger's current baseDamage5 +1.05AGI.

| Skill | Proposed RAW output formula (per user-approved roles) | Why |
| --- | --- | --- |
| **Aimed Shot**, r1–30 | **`RAW_Aimed = [1.90+0.075(r−1)]×B + [0.25+0.016(r−1)]×A`**, with **1.5s stationary windup** | r1: `1.90B+0.25A`; r30: **`4.075B+0.714A`**; single-target heavy bow shot. Approved guaranteed accuracy, normal eligible Bow Mastery crit at 1.6× on crit, ignores **`0.10×r/30` fraction target PDEF** on this attack; never imports Ranger Hunter's Mark. |
| **Arrow Rain**, r1–25 | **RAW EACH of 6 waves** `[0.400+0.022(r−1)]×B` | r1 each **0.400B**, total **2.4B** if all six hit; cap r25 each **0.928B**, total **5.568B** over two seconds. Tier II Ranger former r16 cap had `[0.4+0.035(16−1)]B =0.925B/wave×4=3.70B`; thus the Marksman cap has comparable per-wave power but **~50% more aggregate unmitigated waves against a stationary full-duration target with SAME B**, before its stronger Tier III attributes. **This directly respects user's 'primarily 2 waves making it stronger'**. No crit permission, no aura damage bonus to skill waves. |
| **Power Shot**, r1–25 | **First target RAW** `[1.15+0.05(r−1)]×B + [0.10+0.008(r−1)]×A`, then **`RAW_target_n=RAW_first×0.80^(n−1)`** for each successive distinct target in geometric collision order | r1 first **`1.15B+0.10A`**; r25 first **`2.35B+0.292A`**, purposely below Aimed first-target raw at comparable investment. Approved noncritical Physical 1.5m-wide projectile reach `12+0.2BowMasteryRank` metres (12→16m); all unobstructed intersected hostiles hit, first SOLID wall stops; target-specific Physical mitigation occurs AFTER raw falloff. |
| **Bow Mastery** | **Passive USER APPROVED** +3 AGI, +1.5pp basic+Aimed crit, +0.2m bow range per rank; eligible crit is **1.6×** | At mastery20 +60 AGI/+30pp crit and +4m reach; aura range auto-follows bow range, Power line has bow range+1m. |
| **Strafe** | **Timed buff USER APPROVED** +2r% attack speed, −2r% move speed for `ceil(1+r/5)` seconds, 18s CD | No cast Mana-per-hit; activation-only cost. Rank30: +60%AS/−60%MS 7s; still stationary auto attacks. |
| **Ranged Damage Aura** | **Passive USER APPROVED** +1% ranged BASIC attack RAW damage per aura rank (max +20%) for self/allied ranged basic attackers; **strongest same-name only**; aura radius=current 11–15m normal attack reach | Does NOT boost Aimed, Arrow Rain, Power Shot or any skill packet. Apply to eligible RAW ranged BASIC once before critical and Physical/Magical defense; do not double-buff the same attack event; for nonphysical ranged basics use that basic's ordinary damage channel/defense. |

All class skill damage numbers above are **designer-chosen QA proposals**, not approval by the user to change normal game-wide Physical crit/armor/mana logic. Explicit noncritical Power Shot and Arrow Rain; eligible Aimed crit follows user-approved Bow Mastery only.

### Designer's first-playtest cooldown and Mana suggestions (NOT YET USER-APPROVED except already-locked Strafe CD)

| Ability | First unlock | Max rank | Cooldown candidate | Single activation/cast Mana candidate | Cost at skill max |
| --- | ---: | ---: | --- | --- | ---: |
| **Aimed Shot** | 1 | 30 | **7 seconds PROPOSED** | **`18+12r`** | **378** |
| **Arrow Rain** | 5 | 25 | **22 seconds PROPOSED** | **`20+15r`** | **395** |
| **Power Shot** | 7 | 25 | **12 seconds PROPOSED** | **`18+13r`** | **343** |
| **Strafe** | 1 | 30 | **18 seconds USER APPROVED** | **`10+9r`** on activation once, NO per-attack charge | **280** |
| **Bow Mastery** | 1 | 20 | Passive | **0** | **0** |
| **Ranged Damage Aura** | 10 | 20 | Permanent passive | **0** | **0** |

**Recast/interrupt notes for initial QA:** Strafe CD begins when activated; buff cannot be stacked with a second instance from the same owner; reactivation while active would replace/refresh if such timing ever becomes possible, but its 18s CD exceeds max7s uptime. Aimed Shot is a 1.5s windup requiring target validity and line of sight, canceled on movement/stun/explicit interrupts under usual player cast framework. Arrow Rain's 1.5s channel likewise cancels before successful release on ordinary movement/stun; independent SIX waves keep going if source moves after channel. The *precise* spell-cast interrupt handling/cooldown-on-interrupt should follow existing common game rules until author separately approves a class-specific change.

### Designer's initial HP/Mana class resource proposal (NOT USER-APPROVED)

All following numbers should be test-validated using class resource asset fields currently supported in the Unity project.

- **Base HP:** `20`; `hpPerVitality=15`, `regenerationPerVitality=0.1` (same VIT-to-HP approach as parent Ranger), unmodified movement speed5 and base PDEF/MDEF0.
- **Maximum Mana:** `180 + 5.0×INT + 3.0×SPR`, independent of AGI unless gear gives Mana explicitly. Current Ranger profile is only `60+1×INT`, so copying its Mana formula would not fund the user's enhanced Tier III high-rank abilities.
- **Mana regen/sec:** `4.5 + 0.075×INT + 0.075×SPR`, independent of AGI. Mana resources remain **Mana ONLY**, no Focus/Ammo secondary resource invented.
- At level1 no gear: VIT19/INT9/SPR11: **HP305**, **Mana258**, **Mana regen6.0/sec**. At level150 no gear: VIT317/INT158/SPR189.8: **HP4775**, **Mana1539.4**, **Mana regen30.585/sec**. Ordinary mana/HP engine rounding can affect display; these are analytical projections, not build-test measurements.
- At max skill rank, one of each of Aimed, Arrow Rain, Power Shot and Strafe costs **378+395+343+280 =1,396 Mana**, below the proposed no-gear lvl150 maximum Mana1539.4 **before regen**; repeat-spam over long boss fights may still exceed recovery, so test in real combat. Cooldowns should be evaluated with boss movement and shot cancellation, longbow/shortbow item speed variations, Strafe uptime and ranged-buff stacking. First-playtest threshold viability is NOT proof of sustained-combat balance.

### DESIGNER-PROPOSED EXACT 150 individual rank eligibility gates (mathematically AUDITED; NOT YET USER APPROVED)

Authoring intent:
- Lock the exact user-approved caps **Aimed30 / ArrowRain25 / BowMastery20 / Strafe30 / Aura20 / PowerShot25** =150 and the exact user-approved first purchase unlock levels **Aimed1, Rain5, Mastery1, Strafe1, Aura10, Power7**.
- Skills begin at rank0. Each rank purchase requires owning previous rank, being that class and meeting its individual level gate; no auto-upgrading or Ranger skill rank transfer.
- The table gives **gates in purchased-rank order**: the 1st integer is rank1's minimum Marksman class level, second is rank2's, ...; all strictly increase per skill. A player's spend order may vary after their ranks are available; this gate schedule does NOT force a purchase at a particular level.
- For each class level from2 through148, one **additional** rank becomes eligible globally, while three Tier III level1 first ranks were already available at level1. Late unlock skills have rank1 gates EXACTLY at5/7/10. All rank gates available by **level148**; last two earned points at levels149 and150 can purchase any remaining legal ranks, provided prerequisite rank ordering.
- Deterministic designer distribution picks the lowest **current progress ratio** `purchasedEligibleRanks / abilityCap` among skills already unlocked that have remaining cap, with ties by larger skill cap then earlier unlock and skill name; special initial rank1 unlocks are forced to the user's first-availability level. This is only an eligibility authoring strategy, NOT any requirement that a player spend point(s) according to this schedule.

| Skill | Cap | Rank1 eligible at | **Exact consecutive rank1 → max-rank eligibility gates (proposal)** |
| --- | ---: | ---: | --- |
| Aimed Shot | 30 | 1 | `1, 2, 9, 15, 21, 27, 29, 35, 41, 45, 51, 57, 59, 65, 71, 75, 81, 87, 89, 95, 101, 105, 111, 117, 119, 125, 131, 135, 141, 147` |
| Arrow Rain | 25 | 5 | `5, 6, 13, 19, 25, 31, 37, 43, 49, 55, 61, 67, 73, 79, 85, 91, 97, 103, 109, 115, 121, 127, 133, 139, 145` |
| Bow Mastery | 20 | 1 | `1, 4, 17, 23, 33, 39, 47, 53, 63, 69, 77, 83, 93, 99, 107, 113, 123, 129, 137, 143` |
| Strafe | 30 | 1 | `1, 3, 12, 16, 22, 28, 30, 36, 42, 46, 52, 58, 60, 66, 72, 76, 82, 88, 90, 96, 102, 106, 112, 118, 120, 126, 132, 136, 142, 148` |
| Ranged Damage Aura | 20 | 10 | `10, 11, 18, 24, 34, 40, 48, 54, 64, 70, 78, 84, 94, 100, 108, 114, 124, 130, 138, 144` |
| Power Shot | 25 | 7 | `7, 8, 14, 20, 26, 32, 38, 44, 50, 56, 62, 68, 74, 80, 86, 92, 98, 104, 110, 116, 122, 128, 134, 140, 146` |

**Audit PASS**, independently enforced while generating this document:
1. **150 distinct skill-rank entries**; per-ability entries 30+25+20+30+20+25 exactly sum150.
2. For all six skills, rank1 gate EXACTLY equals its user-selected unlock level; every subsequent rank has a STRICTLY HIGHER gate than its preceding rank.
3. Every gate is within class levels1–148; no skill purchased before its unlock and **no rank past its cap**.
4. For **EVERY level L = 1..150**, total eligible rank slots with gate≤L satisfies `eligible(L)≥L`, hence there is a valid path to spend **one new class skill point every level** (or store then spend later). At level1 three different rank1 choices are available. Checkpoints: **1: 3 eligible / 1 earned; 5: 7 eligible / 5 earned; 7: 9 eligible / 7 earned; 10: 12 eligible / 10 earned; 30: 32 eligible / 30 earned; 60: 62 eligible / 60 earned; 80: 82 eligible / 80 earned; 100: 102 eligible / 100 earned; 120: 122 eligible / 120 earned; 140: 142 eligible / 140 earned; 148: 150 eligible / 148 earned; 150: 150 eligible / 150 earned**.
5. At level148 all150 ranks are eligible, and the earned points count is148; levels149–150 yield the final2 points and all150 ranks can be filled. Last rank gates: **Aimed147, ArrowRain145, BowMastery143, Strafe148, Aura144, PowerShot146**. All 150 legal purchases are possible by Marksman level150 without requiring additional late levels, Tier IV evolution or hidden prerequisite.

**Important distinction:** Arithmetic audit proves *level-gate feasibility* and authoring consistency. It does **not** validate actual game implementation, UI point-spending, combat balance, or use of player-chosen class-specific purchase order. **The user authorized preparing and auditing these exact gates; they have NOT yet approved the designer's specific numbers**. Do not promote them to a final implementation contract until the user reviews and approves the schedule.

### Round 7: Next targeted design approvals (UNANSWERED)

1. **Exact gate schedule:** Approve the audited six rank-gate sequences above as the official first-playtest Marksman 150-point rank schedule, or request adjustments (for example, faster early Bow Mastery ranks)?
2. **Aimed Shot and Power Shot damage:** Approve designer candidate RAW formulas `Aimed=[1.90+0.075(r−1)]B+[0.25+0.016(r−1)]A` and `Power=[1.15+0.05(r−1)]B+[0.10+0.008(r−1)]A` (Power then ×0.8 for each further enemy), along with basic `B=8+1.20A` before item contributions? The Aimed skill is intentionally much stronger against a single target; Power cannot crit.
3. **Arrow Rain power:** Approve `[0.400+0.022(r−1)]B` RAW EACH of the **six** waves, keeping six total waves in2s, 1.5s channel and 3m circle? This gives ~50% more per-target full-duration raw than the four-wave Ranger at their respective skill caps with equal B.
4. **Mana/cooldowns:** Approve initial Aimed **7s / 18+12r Mana**, Arrow Rain **22s / 20+15r**, Power Shot **12s / 18+13r**, Strafe **18s already approved / 10+9r Mana on activation once**, plus Marksman maximum Mana `180+5INT+3SPR`, regen/s `4.5+.075INT+.075SPR`, baseHP20 and HP15/VIT?
5. **Teacher/trial direction:** Shall Marksman advance from Ranger Lv60 via **a hidden teacher found in World1/World2 through exploration** and then complete a **solo precision challenge** (suggested) before returning to teacher? Or use a non-dungeon dedicated shooting-range/survival-trial quest? Don't assume every Tier III must copy Psion's Wolf or Shaman dungeons; Ranger's original Alpha Wolf quest is Tier II only.

**NEXT IN ROSTER:** after Marksman design completion, Greatblade→Berserker, per [coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md).


## Round 7 — 2026-11-25 (LATEST USER ANSWERS: FULL GATES APPROVED; GLOBAL MANA MODEL; RANK-SCALING DAMAGE COEFFICIENTS; HIDDEN TEACHER TRIAL)

**This is the newest authoritative round.** It supersedes the older Round 6 "proposed gates pending" language, the separate Marksman-specific INT/SPR Mana and regen coefficients, and `8+1.2AGI` as a current bow-basic proposal. The older Round 6 balance table remains history; its numerical damage expressions are reformulated below for the new scaling rule. The user approved initial cooldowns/Mana **COSTS** "for now" but did **NOT** authorize unique class-specific resource conversion/regeneration multipliers or implementation of Unity gameplay assets.

### USER-APPROVED results (MARKSMAN3-025 through 030)

1. **ALL 150 EXACT individual rank eligibility gates APPROVED:** The six integer sequences authored, published and mathematically audited in Round6 immediately above are the **official INITIAL PLAYTEST skill purchase gates**: Aimed Shot30 / Arrow Rain25 / Bow Mastery20 / Strafe30 / Ranged Damage Aura20 / Power Shot25, exactly 150. First unlocks Aimed1, Rain5, Mastery1, Strafe1, Aura10, Power7. For every class level1–150 the number of eligible ranks is at least the earned class points; all150 eligible by level148. **Do not silently regenerate or change these approved exact gate integers** when implementing class. Point earn rate remains one each Marksman level 1–150.
2. **GLOBAL RANK-DEPENDENT DAMAGING SKILL COEFFICIENTS:** The user expressly requires **ALL skill damage coefficients scale with the RANK OF THAT SKILL**, rather than a constant AGI coefficient with only a flat +damage per rank. For any damaging Marksman skill `r`, explicitly write `RAW = (flatBase + flatPerRank*(r−1)) + (agiCoefficientBase + agiCoefficientPerRank*(r−1))×currentAGI`, or an algebraically equivalent rank-dependent multiplier of normal BASIC RAW that makes both flat and stat contribution rank-dependent. **Normal BASIC autoattack itself is NOT a rankable skill** and retains a constant AGI weight, controlled by the actor attack definition and passive effects. Power's additional 80% *per target* falloff is distinct from its *rank*-driven raw damage coefficients. This is an authored skill-design rule, not a retroactive global rewrite of every existing class's code/damage without separate implementation.
3. **RANGER BASIC bow damage user correction:** "ranger auto attack should be base + (axi x 1)" is interpreted as **`RangerNormalBowBasicRAW = RangerAuthoredBase + 1.0×RangerCurrentAGI`**, with typo "axi" = AGI by context. **Actual current Ranger asset** `Assets/_DiceFree/Settings/Ranger/Ranger bow basic.asset` is `baseDamage=5`, `weights.agility=1.05`, `coefficient=1`; therefore **DESIRED normal-bow formula is `5 + 1.0×AGI` before item/damage-range modifications**, and this **differs from currently checked-in UNITY ASSET `5+1.05×AGI`**. Code/asset will need an **explicit authorized gameplay update**; this documentation round does not modify the asset. For Marksman, recommend the same `1.0×AGI` basic damage coefficient (class family consistency), with a proposed higher initial BASIC baseDamage8, i.e. **`B=8+1.0×A`** before items; **the Marksman's BASIC base8 is still DESIGNER-PROPOSED, not user-finalized**. Ordinary basic AGI weight does not grow with skill rank, though Bow Mastery raises current AGI by3/rank.
4. **MANA MUST FOLLOW ONE COMMON MODEL ACROSS ALL MANA-USING CLASSES; NO bespoke Marksman maximum/regen coefficients.** The user says the Mana formula already exists globally and the classes should use that ONE and SAME formula. Balance knobs allowed: **starting/base Mana, starting INT, INT gain per level, and items/equipment Mana stats**. Accordingly, withdraw Round6's Marksman-exclusive `MaximumMana=180+5×INT+3×SPR` and `ManaRegen/s=4.5+0.075×INT+0.075×SPR` as class-specific formulas; they are **SUPERSEDED and NOT IMPLEMENTATION TARGETS**. A tentative starting Mana **180** may remain as a class-specific base Mana balance knob under a truly unified formula, but **do not infer the final global per-INT coefficient, per-SPR contribution or regen formula from that old proposal**. Use the existing shared game-system rule, not new unique multipliers; if the true authoritative shared numeric coefficients are unclear, confirm rather than choose arbitrarily.
   - **Repository implementation verification as of this round:** `Assets/_DiceFree/Scripts/Combat/ClassResourceProfile.cs` already has a shared *computation shape* `Maximum = max(0, baseMaximum + INT×maximumPerIntelligence + SPR×maximumPerSpirit)` and `Regeneration=max(0, baseRegenPerSecond + INT×regenPerIntelligence + SPR×regenPerSpirit)`. **However the current numeric coefficients are stored PER-CLASS** in ClassResourceProfile assets, not centrally enforced globally. Example current Ranger Mana asset: baseMaximum60, +1Mana/INT, +0Mana/SPR, baseRegen4, +0/INT, +0.1/SPR; current Psion Mana asset: baseMaximum84, +3Mana/INT, +1Mana/SPR, baseRegen4.5, +0.075/INT and +0.075/SPR. Therefore **the user's new game-wide unified-formula INTENT is NOT already met by the verified current assets**, even though the calculation framework is shared. Codex must reconcile these profiles and establish one canonical shared numeric coefficient/regen rule without casually overwriting working class balance or misrepresenting the present state. This is a **separate global implementation/refactor task**, not a new Marksman-only formula. Update old Psion evolution-specific per-INT Mana/regen assumptions during that coordinated global pass if they conflict with the newly user-selected global rule; do NOT change Unity assets in this docs-only design round.
5. **COOLDOWNS AND SKILL MANA COSTS conditionally approved "for now":** Preserve Round6's proposed Aimed7s/`18+12r` Mana, Arrow Rain22s/`20+15r` Mana, Power Shot12s/`18+13r` Mana, and the already-approved Strafe18s/`10+9r` Mana paid ONLY once on activation. Bow Mastery and Ranged Damage Aura are permanent passives with 0 cost. **Mana max/regen is NOT approved under Round6's proposed per-class multipliers**, and prior computed Lv150 Mana1539.4, regen30.585/s and the "1396-cost full sequence is affordable" statement are **NOT VALIDATED against the unified Mana model**. Retain base HP20 and standard +15HP/VIT as initial playtest values as user said "sure, for now"; base move speed5 and PDEF/MDEF0 remain neutral actor defaults unless globally changed.
6. **HIDDEN MARKSMAN TEACHER AND SOLO PRECISION CHALLENGE APPROVED IN PRINCIPLE:** Tier II Ranger at Lv60+ may discover a hidden Marksman teacher by exploration (World1 OR World2 flexible placement, following hidden-teacher philosophy where relevant), accept a distinct **SOLO precision challenge** with shooting targets at different distances and potentially moving targets, then **RETURN to that teacher to turn in** and create **Marksman new TierIII Lv1 manifestation** while preserving the original Ranger. Do NOT reuse Ranger's TierII Alpha Wolf advancement quest or Psion's survival/Shaman dungeon by default. **Exact site, teacher name, challenge flow/targets, scoring, time limit, failure rules, quest access, class-specific dialogue and whether discovery Codex-wide need their own design round**; do not label them settled.

### Revised offensive numerical candidates (DESIGNER-PROPOSED FIRST TEST, explicitly rank-scaling ALL skill damage coefficients)

Define **`r` as the purchased rank of the skill being cast (not Marksman class level)**, `A` as current AGI **including Bow Mastery +3AGI/rank**, and proposed Marksman normal bow BASIC RAW `B=8+1.0A` before weapon/gear modifiers and aura. Ranger's normal/basic `5+1.0AGI` is a USER-REQUESTED class-specific correction to the parent actor, not to this proposed Marksman basic actor base8.

The new formulas are algebraic expansions of the earlier Round6 candidate damage multipliers after adopting `B=8+1.0A`: **BOTH flat scaling and AGI scaling explicitly increase with the PURCHASED SKILL RANK**. These are FIRST-PLAYTEST NUMERICAL DESIGNER PROPOSALS pending approval, not already approved because the user approved their DIRECTION.

| Damaging skill | New explicit rank-scaling RAW formula | r1 | Skill cap | Details retained |
| --- | --- | --- | --- | --- |
| **Aimed Shot (r1–30)** | **`(15.2+0.60(r−1)) + (2.15+0.091(r−1))×A`** | `15.2+2.15A` | **`32.6+4.789A` at r30** | Guaranteed targeted Physical hit, 1.5s stationary windup, usual eligible Bow Mastery crit chance and 1.6× crit damage, ignores `10%×r/30` target PDEF only on this attack; no Hunter's Mark |
| **Power Shot first target (r1–25)** | **`(9.2+0.40(r−1)) + (1.25+0.058(r−1))×A`** | `9.2+1.25A` | **`18.8+2.642A` at r25** | NONCRITICAL, physical piercing 1.5m width, 12→16m maximum line, first solid wall stops, guaranteed on each valid geometric intersection. Subsequent `n`th enemy `RAW_n=RAW_first×0.8^(n−1)`, each enemy's PDEF separately mitigates post-falloff |
| **Arrow Rain EACH of six waves (r1–25)** | **`(3.2+0.176(r−1)) + (0.40+0.022(r−1))×A`** | `3.2+0.40A` | **`7.424+0.928A` per wave at r25** | Six hits spread over 2s after 1.5s interruptible channel, 3m radius; no crit or basic-only aura bonus; at cap SIX waves total `44.544+5.568A` on a target remaining in area, before defense/other effects |

**Proof of actual rank-dependent attribute scaling:** Aimed AGI coefficient **2.15 at rank1 → 4.789 at rank30**; Power **1.25 →2.642 at r25**; Rain per-wave **0.40→0.928 at r25**. This directly matches the user requirement and does NOT merely increase a rank-flat term while leaving the AGI coefficient fixed. The PDEF penetration, +AS/−MS, Aura damage bonus and range attributes already separately scale with their respective own skill ranks; no new attack-crit permission for Power/Arrow.

**Important tuning caveat:** These are **designer proposals, not tested**. Skill RAW formulas depend on the author's chosen Marksman basic B and the engine's attack/weapon modifier conventions. Equipping shortbow vs longbow should preserve ordinary item-dependent damage and speed effects. Existing global Physical mitigation/crit handling applies as authored; avoid letting Power/Rain borrow Bow Mastery crit or Ranged BASIC-only aura.

### Initial test skill Mana costs and cooldowns (APPROVED "FOR NOW", no bespoke Mana regen)

| Active | Cooldown | Mana COST once on cast |
| --- | ---: | ---: |
| Aimed Shot | **7 seconds** | **`18+12r`** (r30=378) |
| Arrow Rain | **22 seconds** | **`20+15r`** (r25=395) |
| Power Shot | **12 seconds** | **`18+13r`** (r25=343) |
| Strafe | **18 seconds** | **`10+9r`** (r30=280), once on activation, not per shot |
| Bow Mastery / Ranged Damage Aura | Passive | 0 |

**Do not reuse** old proposed per-class `5Mana/INT` or `0.075ManaRegen/INT` as the new approved Marksman coefficients; those class-specific values are withdrawn. The generic system/global formula must be verified and standardized as per user instruction; only then project real level1/60/150 Mana affordability. The total once-each rank-max Mana COST remains **1396**, but available total Mana is **unknown until the global formula and base Mana/item tuning are resolved**.

### FULL 150 RANK ELIGIBILITY SCHEDULE: USER APPROVED

**The EXACT six rank-gate sequences in Round 6 immediately above are APPROVED as written.** Do not regenerate them with new arbitrary numbers; all remain strictly increasing, accessible at their specified first unlock levels (Aimed1, Bow1, Strafe1, Rain5, Power7, Aura10), and every class level from1–150 has enough eligible purchased ranks to spend one new point. All 150 available by class level148, with 150 earned by level150. The earlier Round6 label "not yet user approved" applies historically only and is explicitly superseded here.

### Round 8: FIVE next questions (NOT ANSWERED YET)

1. **Rank-scaled offensive damage:** Approve/revise first-test Aimed `15.2+0.60(r−1)+(2.15+0.091(r−1))AGI`, Power `9.2+0.40(r−1)+(1.25+0.058(r−1))AGI`, and Arrow Rain EACH wave `3.2+0.176(r−1)+(0.40+0.022(r−1))AGI` (six waves)? Does proposed Marksman BASIC `8+1.0×AGI` fit the agreed shared 1.0×AGI basic lineage?
2. **Global Mana numeric rule:** The engine currently applies a common algorithm but has DIFFERENT Mana-per-INT/SPR and regen coefficients on per-class assets. Which currently intended GLOBAL fixed numerical coefficient and regen behavior should all Mana-using classes share? If the user delegates selecting the single value, propose a consistent whole-roster migration after inspecting current Mana assets; do not publish class-specific exceptions.
3. **Quest mechanics:** Should hidden Marksman precision trial be **timed target-hitting in one dedicated instance**, a **stationary survival/shooting lane**, or a **sequence of progressively harder targets (fixed, moving and long-distance)**? Avoid demanding a combat dungeon by default.
4. **Precision challenge win conditions:** Decide target accuracy/score threshold, target sequence, whether failure can retry, whether solo means only one human manifestation and whether summoned allies are allowed, and if any Mana/gear limits apply.
5. **Tier III implementation handoff:** Once the final skill RAW and global Mana decisions are settled, do you want to hand off these documents to Codex to implement Marksman, including Ranger basic AGI coefficient 1.05→1.0 and unified Mana resource formula, while preserving existing Psion Tier III class behavior until tests? **NOT implemented or merged by this design round**.

**Status boundary:** This round authorizes and confirms the DESIGN, exact previously proposed rank gates, the skill Mana cost proposals "for now", and the hidden-mentor/solo-precision goal. It **does not** constitute an implementation of the global Mana refactor, the Ranger attack-asset change, new Marksman assets, scene content or quest.

## Round 8 — 2026-11-25 (USER DECISIONS: offensive coefficients, global Mana, open-world advancement)

This section **supersedes the Round 7 "Round 8 questions OPEN" prompts** above where answered. This is the agreed **first-playtest design**, not a claim that Unity gameplay has been changed or tested.

1. **Damage and basic attack APPROVED for first playtest.** Marksman regular bow BASIC RAW uses **`8+1.0×AGI` before equipment**. For purchased damaging skill rank `r` and current AGI `A` (including Bow Mastery):
   - **Aimed Shot** r1–30: **`15.2+0.60(r−1)+(2.15+0.091(r−1))×A`** RAW. Retains normal eligible crit, 1.6× crit damage, 1.5s windup, no ordinary miss/evasion roll, per-shot percentage PDEF ignore `10%×r/30`.
   - **Power Shot** r1–25: **`9.2+0.40(r−1)+(1.25+0.058(r−1))×A`** RAW on first enemy; n-th distinct enemy RAW ×`0.8^(n−1)`, then per-target mitigation. Noncritical, width1.5m, effective reach basic range+1m, stops at first solid world wall, hits geometrically intersected enemies without evasion roll.
   - **Arrow Rain** r1–25: **`3.2+0.176(r−1)+(0.40+0.022(r−1))×A`** RAW **per wave**; six waves over2s after 1.5s stationary channel, 3m radius, noncritical.
   - **Critical design invariant:** BOTH flat and attribute coefficients grow with purchased skill rank. These are approved **first-playtest** constants, subject to changes after actual gameplay balancing. **Ranger Tier II basic should separately become base+1.0×AGI** per prior user direction, without copying Marksman's proposed base8 to Ranger.
2. **GLOBAL Mana direction, numerical partial approval:** User says **"at least 5 Mana per INT"**. Set a **shared +5 maximum Mana per INT as the initial minimum/test coefficient**, used consistently by ALL Mana-using classes; consider increasing globally if balancing reveals undercapacity, not via bespoke Mana/INT coefficients. Classes may differ by **class starting/base Mana**, starting/growing INT and equipment. The assistant offered a shared baseline `Global Base Regen + 0.10×SPR`; user says **"base regen sounds good"**. Treat **one global base regen term** as approved in principle, but **the precise numeric base regeneration constant and whether +0.10×SPR is final are not yet fully specified**. Do not falsely claim an existing engine-wide uniform Mana rule: current assets configure divergent coefficients. Define/test one common regeneration formula and migrate all Mana users in a coordinated implementation; tune after playtesting.
3. **ADVANCEMENT QUEST: OPEN WORLD, NOT INSTANCED/TRIAL.** After Tier II Ranger Lv60+ **accepts the hidden Marksman teacher's quest**, **small target objects APPEAR in the persistent/world exploration spaces**, and the Ranger must **FIND AND SHOOT** them around the world. The user suggests **approximately one target in every village/city** as a direction, not an exact enumerated settlement list or target count. This **supersedes the previously suggested dedicated three-stage range, timed test, arrow budget, accuracy score, and standalone shooting instance**. Use ordinary in-world target/quest interaction, tracking and completion; then **return to teacher** for completion and TierIII Marksman Lv1 manifestation, preserving the TierII Ranger. Location/teacher identity, exact eligible settlements/target count, whether each is globally visible only to quest owners, target hit validation, tracking/Codex feedback, hints and reset behavior remain open and must be authored based on actual World1/World2 settled locations. **Do not invent a time limit, limited arrows, penalty or instanced arena.**
4. **COORDINATED IMPLEMENTATION:** User explicitly requests **Marksman class implementation and game-wide uniform Mana formula migration AT THE SAME TIME**, not separated into independent stages; plan integration/testing together so other Mana classes (Mage, Priest, Psion and evolutions, etc.) remain functional. Also apply Ranger basic AGI coefficient correction. User agrees later playtesting may require numerical retuning. **This document update does not itself implement or verify Unity changes.**

**Confirmed up to Round 8:** skills/caps/rank gates from Rounds 1–7 remain approved; first-playtest damage now approved, while shared Mana base regeneration constants and open-world quest's exact target placements remain under design.

## Round 9 — 2026-11-25 (USER APPROVED: cross-world settlement targets, journal, shared Mana coefficients)

**Supersedes** the remaining Round 8 open questions on quest geography, target hiding difficulty, and shared Mana numeric coefficients. These are USER-APPROVED FIRST-PLAYTEST DESIGN decisions, not an assertion that Unity implementation has been completed.

### Open-world Marksman teacher quest: visit BOTH worlds

- After a qualifying Tier II Ranger **accepts the hidden Marksman teacher quest**, spawn/enable **one small shootable archery target in EACH established village/city in both World 1 AND World 2**. The exact list must be derived from the actual canonical settlement definitions/scenes before implementation. **Do not invent missing villages, duplicate target count across neighborhoods, or require non-settlement dungeon/biome locations as part of "each village/city."**
- A **quest journal lists settlement names and marks which target locations are complete vs still remaining**. The list makes required destinations discoverable; players should know which settlements still need visits, while finding the actual target at the settlement supplies a light exploration element. The aim is **world touring**, NOT difficult target hiding.
- Targets should be **reasonably visible and straightforward to locate on arrival**, not tucked into obscure rooftops, concealed trees or punishing pixel hunts. Tougher searching is an optional idea for **Tier IV later**, NOT Marksman Tier III.
- A qualifying Ranger shoots each local target using ordinary eligible ranged attack/quest-hit rules, records progress once per settlement in that player's appropriate quest state, then visits the remaining settlements in either world. **No instanced shooting range, timer, fixed-arrow budget, accuracy-score requirement or separate precision minigame**.
- On completion of all listed settlements, **return to the teacher** to advance into a new Tier III **Marksman level 1 manifestation**, preserving original Ranger and existing established class-advancement rules.
- Quest authoring/implementation follow-up: inspect actual World 1/World 2 settlements; agree explicit eligible list and whether special hubs count; determine exact quest-target spawn placement, interactability/visibility for nonquest players, multiplayer progression ownership, travel accessibility, quest persistence and journal UI implementation. Do not claim these details already designed or built.

### Shared Mana baseline: APPROVED FOR FIRST PLAYTEST

All **Mana-using** classes follow the SAME global resource formulas, without individual class-specific attribute coefficients:

`MaximumMana = ClassBaseMana + 5 × INT`

`ManaRegenPerSecond = 4 + 0.1 × SPR`

- **+5 Mana per INT** is the approved initial game-wide coefficient and matches the user's "at least 5" preference. Treat it as a baseline/minimum for a future **GLOBAL** playtest retune, not a permanent upper limit.
- **Global regeneration base 4 and +0.1 per SPR** now expressly approved as first-playtest numbers, superseding Round 8 provisional note that these exact regeneration constants were undecided.
- Individual **ClassBaseMana**, starting INT, INT growth, SPR from actor stats, equipment, buffs and skill costs can remain class-authored according to the global engine rules; **do not introduce different per-class Mana-per-INT or Mana-per-SPR/regeneration coefficients**. Confirm how equipped stats and general-purpose additive buffs integrate using the existing engine pipeline.
- Migrate and test **ALL Mana users** along with Marksman and Ranger basic AGI correction as **ONE coordinated implementation scope** per user's Round 8 instruction. Check Priest, Mage, Psion and their evolutions as well as any other Mana users for affordability and regressions; current heterogeneous source asset coefficients are not proof of completion.
- Values **subject to playtest adjustments**. Document current formulas before changes, validate in combat and test rather than assuming current skill costs already balanced.

**Status:** Marksman design rounds 1–9 decisions recorded. Mark initial-playtest numbers as approved and outstanding scene/quest implementation details as open; no Unity/gameplay implementation or manual playtest performed by this documentation-only update.

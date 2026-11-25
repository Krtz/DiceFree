# DiceFree — Marksman (Tier III Ranger evolution) — design interview

**2026-11-25 — ROUND 1 USER RESPONSES RECORDED; ROUND 2 OPEN.** User explicitly selected **Ranger → Marksman** as the FIRST Tier III evolution to design in the new **at least one Tier III per Tier II class** coverage pass. **Greatblade → Berserker follows next**, then choose first evolution for the five other uncovered Tier II parents. This is an AUTHORIZED DESIGN INTERVIEW, **NOT Unity code/asset/build implementation permission**.

**Status:** Round 1 establishes Marksman as a **stationary ranged damage dealer**, with **11m initial range growing to 15m via skill ranks**, compatible with both longbows and shortbows (longbows favored), **NO Hawk Dive / Hunter's Mark**, and retained **Aimed Shot, Arrow Rain, Bow Mastery** plus the proposed **Strafe** active. A ranged-damage aura and piercing Power Shot are promising **TENTATIVE** skills pending confirmation. The **Marksman class name and the choice of Ranger as its parent in this rollout are USER CONFIRMED**. A precision-focused, longer-ranged longbow archetype was discussed in Ranger's earlier Tier II interviews as an **IDEA**; it is NOT YET an approved detailed Tier III skill set or a numerical range. Questions below are open, with suggested directions clearly marked as suggestions. Do not confuse this with an already implemented Tier III class.

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

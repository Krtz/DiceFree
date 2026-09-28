# Magically Touched Novice

## Status

**Working Tier 1 direction.** Exact skills, coefficients, mana costs and equipment lists are not yet final.

## Identity

Magically Touched Novice is still a broad magical blank slate rather than a finished Wizard/Healer/Occultist.

It is:
- primarily Intelligence/Spirit driven;
- capable of both damage and support;
- deliberately generalist so the player samples several magical roles before specializing.

Current intended Tier-1 kit shape is **5 skills, 6 ranks each**:
1. single-target magical damage/debuff;
2. heal;
3. buff/support active;
4. AoE magical damage;
5. Mana passive.

Tier 1 is still broad. It should preview later magical identities without already being a full healer or full barrier-support class.

### Skill-point cadence

Tier 1 restarts its own skill progression:
- level 1 begins with 1 Tier-1 skill point;
- gain 1 skill point per level;
- each of the 5 skills has 6 ranks;
- reaching level 30 provides exactly 30 Tier-1 skill points, enough to max all five skills.

Novice ranks do not numerically transfer; evolved skills restart as Tier-1 skills.

## Resource direction

Strong current direction: **Mana**.

Tier 1 is a good point to introduce resource management generically, before later classes replace or modify Mana with specialized resources.

Magically Touched should:
- have a **larger Mana pool** than Physically Blessed;
- spend **more Mana per spell**;
- see Mana costs grow more meaningfully with stronger/ranked spells;
- care substantially more about natural Mana regeneration and INT-linked Mana scaling.

This creates a real reason for INT/resource progression without making the physical Tier-1 class resource-starved.

## Basic attack and equipment

Magically Touched uses a simple **ranged magical basic attack**.

Basic-attack scaling:
- use **whichever is higher of INT or SPI**;
- this keeps both damage-leaning and healing/support-leaning builds comfortable before specialization.

Equipment direction:
- broad access to **ordinary low-level magical weapons**;
- examples can include staves, wands, magical focuses/orbs, caster daggers and similar introductory caster equipment as those families are introduced;
- exact weapon categories remain data-driven rather than predeclared all at once.

The Tier-1 class should remain visually and mechanically general rather than already looking like a full Wizard.

## Working skill identities

### 1. Magic Sand — evolved single-target spell
- preserve the Novice Magic Sand lineage rather than deleting it;
- real **Nature-element** ranged magical attack;
- damage scales from **INT**;
- miss-chance debuff scales at **+7.5% per skill rank**;
- debuff duration stays fixed at **5 seconds** across all six ranks;
- ranks improve damage/debuff magnitude rather than duration.

Mana cost, damage coefficient and cooldown remain balance work.

### 2. Mend
- direct heal;
- scales from **SPI only**;
- self/ally target;
- previews the future healing/cleanse lineage;
- current cooldown target: **~10 seconds**;
- uses a meaningful but not enormous Mana cost;
- should not by itself make Magically Touched a complete healer.

Exact heal coefficient and Mana cost remain balance work.

### 3. Elemental Imbuement / magical buff
Preferred over giving Tier 1 both a heal and a barrier.

Working fantasy:
- buff self or ally;
- add additional elemental damage to the target's basic attacks;
- introduces support/buff gameplay;
- previews the future barrier/buff support lineage;
- Tier 1 should remain a broad magical blank slate rather than commit deeply to one elemental identity.

Element is **Fire**.

Exact bonus, duration, Mana cost and rank scaling remain open.

### 4. Ice Burst / delayed magical AoE
- ground-targeted **Ice-element** AoE;
- uses a **small delayed explosion** rather than an instant blast or long-duration zone;
- applies a Movement Speed slow of **2% per skill rank**;
- rank 6 therefore targets **12% slow** before any later resistance/immunity rules;
- deliberately teaches delayed ground targeting and elemental control at Tier 1.

Exact delay, radius, damage, slow duration, Mana cost and cooldown remain balance work.

### 5. Mana Attunement
Passive:
- increases **maximum Mana** by a flat amount per rank;
- increases **personal Mana regeneration** by a flat amount per rank;
- also emits a **small nearby Mana-regeneration aura** for allies.

Aura direction:
- self receives the full personal passive benefit;
- nearby allies receive only a small Mana-regeneration benefit;
- affects Mana only, not other class resources;
- multiple copies should not stack additively; strongest applicable aura should win.

This intentionally introduces the aura concept at Tier 1 without making the class a dedicated aura/support specialist.

Exact flat values, aura radius and ally share remain open.

## Mana-cost growth direction

Magically Touched spells should become **substantially more expensive as skill rank rises**.

Current philosophy:
- rank-1 spells can be cheap enough to use comfortably;
- rank-6 spells can cost dramatically more because the class has also gained levels, INT, Mana Attunement ranks and better gear;
- a representative offensive-spell direction is roughly **10 Mana at rank 1 -> ~100 Mana at rank 6**;
- exact curves do not need to be linear.

This steep cost growth is part of why Magically Touched cares about Max Mana and regeneration much more than Physically Blessed.

## Starting stats — current balance target

Level 1:
- Vitality: **10**
- Strength: **5**
- Agility: **5**
- Intelligence: **13**
- Spirit: **13**

Total starting primary stats: 46.

## Level growth — current balance target

Average per level:
- +1 VIT
- +0.5 STR
- +0.5 AGI
- +2 INT
- +2 SPI

Implementation/UI preference:
- award +1 STR and +1 AGI every second level rather than exposing half-stat values.

Total average growth: 6 primary-stat points per level.

## Tier-1 visual direction

Magically Touched should look **slightly smaller/frailer** than Novice.

Do **not** add dramatic magical glow, glowing eyes/hands, large rune effects or a finished Wizard silhouette at Tier 1.

The visual message is simply:
- physically less robust;
- subtly more caster-like through proportions/equipment;
- still a low-tier blank magical slate.

Flashier magical transformations belong to later specializations.

## Possible level-30 branches

Current candidate four-way split:
1. **Barrier/buff support archetype** — prevention, barriers, enhancements;
2. **Healing/cleanse archetype** — restoration, cleanses, reactive healing;
3. **Arcane-focused wizard archetype**;
4. **Esoteric/occult caster archetype**.

The fourth category is a broad feeling, not a locked class name. Later descendants can branch into very different magical traditions.

A **Nature Shaman** is explicitly desired as a later descendant somewhere in this esoteric/occult side of the tree.

Development does not need symmetrical breadth at every tier; some branches may receive more successors earlier than others.

## Open questions

- exact passive/no-passive rule;
- exact four active abilities;
- which Novice abilities evolve into this kit;
- basic attack and weapon/focus identity;
- base HP/defenses/movement;
- equipment permissions;
- Mana pool/regeneration/cost model;
- final names and exact distinction between the two healer branches;
- exact identity boundary between Arcane and Occult.

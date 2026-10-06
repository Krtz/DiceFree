# Class Progression

## Core concept

Every new Echo lineage begins as a **Novice**.

See `docs/classes/NOVICE.md` for the detailed starting class.

Advancement branches create alternate class manifestations rather than deleting the parent manifestation.

Each class ID has at most **one persistent save-state** for the Echo.

The original class remains playable, while the first advancement into a new class creates that class's save-state at level 1.

## Advancement cadence

```
Novice        -> reach level 10  -> branch -> new Tier I manifestation starts at level 1
Tier I        -> reach level 30  -> advance -> new class starts at level 1
Tier II       -> reach level 60  -> advance -> new class starts at level 1
Tier III      -> reach level 120 -> advance -> new class starts at level 1
Tier IV       -> reach level 200 -> advance -> new class starts at level 1
Tier V
```

A character may continue leveling beyond an advancement threshold.

When they eventually advance:
- the new class begins at level 1;
- excess XP does not carry over;
- the new class uses its own base stats and growth.

Overleveling gives no automatic permanent bonus, but can make advancement quests easier and may satisfy hidden conditions.

A level-200 Novice is intentionally possible.

It is also intended to receive a hidden stronger Novice payoff (secret skill and potentially first passive) rather than remaining permanently identical to the level-10 kit.

## Branching manifestation model

Advancement is a **branch**, not an overwrite.

When a manifestation reaches an advancement threshold and chooses a class:
- the current manifestation is preserved as a playable parent slot at the fork point;
- if the target class does not yet have a save-state, that class save-state is created for the chosen advancement;
- the game immediately continues as that new advanced manifestation in the same session/world state;
- the new advanced manifestation begins at level 1 with its own class base stats/growth;
- **all manifestation-specific state** is snapshotted into the child at the fork, including gear, carried inventory/currency, quest/world progression and relevant class/skill state;
- the parent keeps its original state and the child receives a duplicate snapshot;
- the preserved parent resumes immediately before the blessing choice if loaded later;
- Echo-wide state remains shared;
- the parent manifestation can later choose another available branch, creating another class save-state if that class has not already been created;
- reaching the same resulting class through another route does not create or overwrite a duplicate save-state.

Example after the first split:
- Novice has one preserved save-state;
- Physically Blessed Novice can gain one save-state;
- Magically Touched Novice can gain one save-state.

The class/Ways UI can serve as the start-game selector for these class save-states.

### Current prototype implementation (#50, 2026-10-06)

The first manifestation-fork foundation is implemented on `feature/50-advancement`, stacked on the completed #49 Novice-skill branch.

Current prototype behavior:
- the player build boots into a dedicated `StartMenu` scene before Cornberg;
- a fresh save offers `Start as Novice` and then enters Cornberg as the initial level-1 Novice manifestation;
- an existing Echo lists every saved manifestation with class name and saved level, marks the last-active manifestation, and lets the player choose which existing class timeline to load;
- an unresolved/missing class definition remains visible as unavailable rather than silently deleting its save section;
- choosing an existing manifestation updates only Echo-level active-roster metadata transactionally, then Cornberg loads that saved class state;
- class switching is not exposed as an arbitrary in-world gameplay button; the start menu is the normal load-time selector;
- level-10+ Novice exposes two data-driven advancement edges: Physically Blessed Novice and Magically Touched Novice;
- advancement commits the preserved parent and new child in one Echo revision before changing the live actor;
- the child starts at level 1 / 0 XP and class skill ranks reset instead of numerically carrying Novice ranks;
- manifestation-owned quests/world state, anchor, carried gold, inventory and equipment snapshot into the child;
- copied child items receive new instance IDs and equipped references are remapped, so parent/child timelines do not claim the same item instance identity;
- parent and child then diverge independently and the preserved Novice can later create the other first branch;
- duplicate creation of an already-existing target class is rejected without overwriting it;
- an additive version-1 `echo:manifestations` roster section stores active class and branch history while the overall save/manifestation schema remains v5;
- current Tier-1 actor assets are intentional **class shells** with the settled starting/growth attributes; final models, Mana, attacks and skill kits belong to the upcoming class implementation slices rather than #50.

The start-menu presentation is deliberately functional/prototype-grade. Final title art, character cards, archive/delete UX and richer class preview presentation remain future UI work.

This one-save-per-class model is part of the Echo/timeline fantasy.

Open:
- archive/delete/recreate behavior for intentionally discarded class-local saves;
- UI presentation of historical parent/route relationships.

## Core attributes

Current attribute set:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Every class defines:
- level-1 base attributes;
- per-level attribute growth;
- starting Physical Defense;
- starting Magical Defense;
- one or more primary attack attributes;
- movement speed;
- base HP;
- resource(s);
- other class-specific base values.

Later advancements generally start with higher base stats.

Players do not manually allocate stat points.

### Primary attributes

A class may use one or multiple primary attributes.

There is **no universal multi-primary formula**.

Each class defines its own attack scaling according to identity and balance.

Examples could include:
- equal STR + INT weighting;
- mostly STR with some INT;
- AGI + SPI;
- all five attributes for an exceptional secret class.

The exact coefficients are class data.

### Vitality

Vitality contributes to HP:

```
Max HP = class base HP + (Vitality × coefficient) + modifiers
```

The coefficient may be universal or class-dependent; unresolved.

### Secondary effects

Current direction:
- Vitality -> HP and HP regeneration;
- Strength -> modest Physical Defense;
- Agility -> very small Attack Speed and Movement Speed;
- Intelligence -> modest Magical Defense plus class-specific resource value;
- Spirit -> healing done and healing received with separate coefficients.

Class base stats, equipment, abilities and passives remain the main source of large combat differences.

See `docs/STATS_AND_DAMAGE.md` for the detailed model.

### Skill scaling

Skill scaling is ability-specific.

Abilities can use:
- weighted combinations of stats;
- highest of selected stats;
- highest stat overall;
- total stats;
- class-specific values.

The class's primary attribute(s) do not force every ability to use the same formula.

## Opening branches

First advancement:
- **Physically Blessed Novice**
- **Magically Touched Novice**

Working Tier-1 class sheets:
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md`
- `docs/classes/MAGICALLY_TOUCHED_NOVICE.md`

Current strong direction:
- both Tier-1 branches introduce **Mana** as a simple shared resource;
- specialized resources arrive on later classes where they reinforce identity;
- Physically Blessed remains a broad STR/AGI melee generalist;
- Magically Touched remains a broad INT/SPI magical generalist.

Current candidate level-30 branching:
- Physical: sword-and-board, rogue, ranger, two-handed melee DPS;
- Magical: shield/buff healer-support, healing-focused healer, arcane wizard, occult caster.

This four-plus-four split is still a working direction rather than locked final structure.

### Long-term class-web scale

A **very large class graph/web is an explicit long-term goal**.

Do not optimize the design around keeping the final class count small.

The tree also does not need to be development-symmetric:
- one class may initially have one implemented successor;
- another may already have three;
- additional descendants can be filled in over time;
- higher-tier concepts can take longer to design and implement.

The architecture must support arbitrary branch counts, convergence requirements and partial/asymmetric content without requiring placeholder classes or giant hard-coded trees.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md` for multi-lineage requirements, visibility/discovery and class-web rules.

### No direct class creation

Unlocking/discovering a Way never creates it from the menu.

A missing class save is always created through an eligible earlier class's actual advancement quest/event.

This remains true when a much later-tier Echo-wide event unlocks an earlier-tier Way.

If a class save is permanently deleted, recreate it by advancing into it again from an appropriate earlier class; Echo-wide discovery and milestones remain intact.

### Tier-1 skill-rank pattern

Current Tier-1 direction:
- 5 skills;
- 6 ranks each;
- 1 class skill point at level 1;
- +1 class skill point per level;
- exactly 30 total points by level 30.

This clean fit is specific to Tier 1 and is **not** a universal rule for later tiers.

## Class identity

Each class defines:
- allowed equipment families;
- weapon categories;
- melee/ranged basic attack;
- attack range/speed;
- primary attribute(s);
- primary-stat coefficients;
- resource;
- fixed ability kit;
- passive mechanics;
- advancement quest;
- visual identity;
- **body/species/form identity where applicable**.

A Way is allowed to transform the Echo substantially. Classes are not constrained to one human body or even one universal humanoid rig.

Settled Tier-2 visual identities:
- **Ranger -> Wood Elf**;
- **Wizard -> High Elf**.

Tentative future directions:
- Berserker may be an Orc;
- one Tier-4 tank may be a Centaur.

Those tentative examples are not locked class canon yet.

Architecture/art must support different proportions, rig families and potentially non-biped forms without making those classes hacks or exceptions bolted onto a human-only player architecture.

### Authored body presentation

Each class has **one authored body/sex presentation** rather than player-selectable male/female variants for that Way.

A class can be visibly male, visibly female or androgynous.

The visual transformation should generally become more pronounced as the class lineage specializes:
- Novice begins deliberately neutral/androgynous;
- early branches can make modest directional changes;
- later Ways can become strongly species-, body- and silhouette-specific.

The form is part of the Way's authored identity, not a separate character-creator axis.

A descendant Way may fully redefine the body/species/form again. Visual inheritance is not a promise that every descendant remains the same species as its parent; advancement can transform the Echo again when that is part of the new Way's identity.

Body form can also carry authored gameplay footprint/collision data. Different class forms may use different hitbox/agent dimensions rather than treating size as presentation-only.

Attack/basic-attack/ability reach remains separately authored and is not automatically derived from body size.

Required world progression must remain accessible to every intended playable form. Large forms can use authored navigation/clearance forgiveness at bottlenecks rather than being locked out because of their footprint.

### Equipment permission inheritance

A descendant Way inherits its parent's allowed equipment/form-family permissions by default.

A descendant may:
- add new equipment families;
- explicitly remove/restrict inherited families when its body/class identity changes;
- override specific compatibility where an inherited appearance no longer fits.

This mirrors visual-appearance inheritance: lineage compatibility is the default, explicit divergence is the exception.

### Gear reconciliation during advancement

If the new Way/form is incompatible with equipment currently worn by the parent snapshot, advancement reconciles it safely:
1. keep compatible equipment equipped where normal slot/permission rules allow;
2. unequip incompatible/conflicting items into the child manifestation's carried inventory when there is room;
3. if carried inventory cannot safely retain them, send those items to the Echo-wide shared bank as a fallback;
4. if neither destination can safely retain every displaced item, the advancement transaction must fail before mutation rather than destroy/drop/overwrite gear.

This reconciliation is part of the atomic advancement/fork transaction.

The parent manifestation remains unchanged with its own original equipment/state.

### Optional transformation presentation

An advancement may optionally define a visual transformation presentation.

The class/manifestation fork is authoritative; transformation VFX/animation are presentation layered on top and may be simple, bespoke, or absent.

Weapon categories are created according to actual class needs rather than predefining every possible weapon.

## Fixed kits and advancement

Classes use fixed kits.

Advancement may:
- add abilities;
- evolve abilities;
- replace abilities;
- change passives;
- replace resources;
- change stats;
- alter equipment permissions;
- change visual form.

## Convergence and Echo-wide class evidence

Class progression is not limited to parent -> child lineage requirements.

The Echo permanently records meaningful class/tier milestones account-wide.

A class may require:
- one lineage milestone;
- several lineage milestones together;
- class evidence combined with quests/items/achievements/discoveries;
- nested AND/OR requirement expressions.

The same resulting class can be reached through different qualifying parent manifestations, but the target class normally uses the same advancement quest/NPC/location regardless of which parent supplies the timeline fork.

A single prerequisite combination can also unlock multiple different convergence classes.

Deleting/cleaning up an old manifestation does not erase class milestones the Echo already earned.

See `docs/CLASS_REQUIREMENTS_AND_DISCOVERY.md`.

## Secret classes

Secret classes may branch from many points and use unusual conditions:
- achievements;
- items;
- exploration;
- encounters;
- account discoveries;
- extreme overleveling;
- strange class-specific feats.

Secret classes use the same advancement thresholds after entering their lineage.

## Healer viability

Healer/support lineages must be fully viable for solo progression.

They should have:
- a real damage rotation;
- sufficient solo damage to quest/farm/grind;
- healing/support tools that become especially valuable in groups;
- class-specific ways to connect offense and sustain where appropriate.

## Open questions

- exact archive UI and permanent-delete confirmation UX;
- exact secondary effect of each attribute;
- universal vs class-specific Vitality coefficient;
- exact second-tier classes;
- weapon categories as classes require them;
- number of character slots.

## Additional settled class-progression rules

### Attribute growth

Primary-attribute growth is determined by **class and gear**.

Players do not receive a general pool of manual VIT/STR/AGI/INT/SPI points to distribute on level-up.

This preserves strong authored class identity while still allowing build variation through gear, skills, effects and class choice.

### Respec philosophy

DiceFree does **not** need a broad always-available respec system as a core progression loop.

The early Novice/onboarding stage may provide free respec support so players can learn the game and experiment before committing to later Ways.

Later classes should not assume routine respec availability unless a particular class/content design explicitly needs it.

### Advancement events

Advancement quests/events are decided **case by case**.

The first Tier 0 -> Tier 1 advancement is intentionally simple: the Novice is told to **go to the mountain** rather than completing a conventional bespoke class trial.

Later advancements can use authored class-specific quests, trials, rituals, discoveries or other requirements as appropriate.

# Class Requirements, Convergence and Discovery

## Progression shape

DiceFree's class progression is best treated as a **directed class graph / web**, not a strict tree.

A class may:
- have one successor;
- have several successors;
- have no successor;
- converge with milestones from other lineages;
- be unlocked through non-class requirements;
- expose different advancement routes that all create the same resulting class.

Linear examples are valid:
- Priest -> High Priest -> Arch Priest.

Highly branching and convergent examples are also valid.

There is no requirement for sibling classes to have matching branch counts or matching implementation schedules.

## Echo-wide class milestones

When the player reaches a meaningful class/tier milestone, the Echo records that achievement **permanently at account/Echo scope**.

Example:
- "Reached Tier 2 Wizard."

This milestone:
- does not require the corresponding manifestation to remain in the character list;
- survives deletion/cleanup of that manifestation;
- may satisfy future class requirements;
- represents the Echo having genuinely embodied that Way in at least one timeline.

The exact milestone granularity is class-data driven. A requirement may care about:
- having reached a class;
- reaching a specific tier;
- reaching a specific level;
- completing an advancement quest;
- another explicit class-specific achievement.

## Convergence classes

A class can require evidence from multiple lineages.

Example concept:
- Mystic Knight requires a qualifying Wizard milestone **and** a qualifying two-handed Fighter milestone.

Once the Echo has both pieces of evidence, an eligible manifestation can potentially advance into Mystic Knight.

### Same class from multiple parent routes

A convergence class can be reachable from either prerequisite side.

For example:
- qualifying Wizard -> Mystic Knight;
- qualifying 2H Fighter -> Mystic Knight.

The resulting Mystic Knight uses the **same class ID, stats, kit and class definition**.

However, the route into it can differ:
- different NPC;
- different advancement quest;
- different location;
- different dialogue/lore;
- different presentation.

The manifestation that actually takes the advancement supplies the timeline snapshot/fork as normal.

### Multiple convergence outcomes

The same prerequisite combination may unlock more than one class when the designs justify it.

Example in principle:
- Wizard + Knight evidence could unlock both Mystic Knight and Battle Mage.

The requirement system must therefore model **independent class definitions and requirements**, not assume one unique convergence result per prerequisite pair.

## Requirement expressions

Class unlock/advancement requirements must be data-driven and support composition.

Required logical forms:
- AND;
- OR;
- nested AND/OR groups.

Requirement types should be extensible and may include:
- Echo-wide class/tier milestones;
- current manifestation class/tier;
- quests;
- achievements;
- discoveries;
- boss/encounter clears;
- items;
- equipped items;
- professions;
- elemental milestones;
- world/region discoveries;
- class-specific feats;
- other future requirement predicates.

Example:

```
(Wizard Tier 2 OR Occult Tier 2)
AND Ranger Tier 2
AND Nature Discovery
```

Avoid special-casing individual secret classes in code.

## Discovery and visibility modes

Each class can choose its own discovery presentation.

Supported modes:

### Explicit
The class is visible and tells the player what is required.

Example:
- visible class name/icon;
- requirements listed directly.

### Known unknown
The class is visible/teased but its unlock method is hidden.

Example:
- class name may be shown;
- or class appears as ???;
- player knows something exists but not how to unlock it.

### Fully hidden
The class does not appear in normal progression UI until discovered/unlocked/become-able according to its design.

Some classes may reveal themselves only once the player actually becomes them.

Visibility and requirement disclosure are **class data**, not one global policy.

## Way discovery

Discovering/unlocking knowledge that a Way exists is Echo-wide.

Once a Way is discovered:
- the Echo retains that knowledge permanently;
- future eligible manifestations can see/use that discovery according to the class's rules;
- becoming the class is still manifestation-specific and occurs through the normal timeline fork.

Unlock knowledge does not automatically make every manifestation eligible. The current manifestation must still be at an appropriate advancement point and satisfy any manifestation-specific requirements.

## Higher-tier convergence

Cross-lineage requirements are allowed at any tier.

A convergence class can itself:
- continue linearly;
- split normally;
- unlock later convergence classes;
- require additional Echo-wide evidence from unrelated lineages;
- end with no successor.

These decisions are class-by-class.

## Character slots

Design target: **effectively unlimited manifestation slots** for normal player use rather than a small gameplay cap.

Players may still want cleanup/organization tools because a large class web can create many manifestations.

If manifestations can be deleted/archived:
- deleting one must never erase earned Echo-wide class milestones;
- deleting one must never erase discovered Ways;
- shared-bank/account progress remains intact;
- only manifestation-specific state is removed.

Practical technical/storage limits may exist, but they should not function as an intended class-progression constraint.

## UI direction

Do not force the entire system into one literal branching-tree screen.

Promising directions include:
- tier tabs;
- a graph/web view;
- current-lineage view;
- discovered-Ways codex;
- filters/search;
- hover/details panels.

Per-class visibility still applies:
- explicit requirements can be shown;
- hidden requirements can display ???;
- fully hidden classes should not leak through graph layout, counters, empty slots or completion percentages unless deliberately designed to.

Exact UI is not yet locked.

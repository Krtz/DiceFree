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

The **target class owns its advancement quest/ritual**.

If Wizard and 2H Fighter can both become Mystic Knight, both normally pursue the same Mystic Knight quest, NPC/location and advancement event once their Echo-wide requirements are satisfied.

The qualifying parent determines which manifestation supplies the timeline snapshot, not which version of Mystic Knight is created.

### Multiple convergence outcomes

The same prerequisite combination may unlock more than one class when the designs justify it.

Example in principle:
- Wizard + Knight evidence could unlock both Mystic Knight and Battle Mage.

The requirement system must therefore model **independent class definitions and requirements**, not assume one unique convergence result per prerequisite pair.

## Requirement scopes and class-family predicates

Requirements may refer to either **specific classes** or **broader class history**.

Examples:
- reached Tier 2 Wizard;
- reached Tier 2 anywhere in the Magically Touched lineage;
- reached Tier 3 in any Nature-tagged class;
- reached Tier 3 in three different elemental caster Ways;
- unlocked Death Knight specifically.

Class/tag/category predicates should be data-driven rather than inferred from display names.

Useful class metadata can include tags such as:
- lineage/family;
- role;
- damage style;
- element/theme;
- armor/weapon identity;
- other explicit semantic categories.

A requirement may count distinct qualifying Ways where needed.

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

## Consumable and event requirements

Item requirements support both:
- **check-only** requirements: possess/equip/show an item;
- **consuming** requirements: sacrifice/turn in/destroy an item as part of an authored event.

The permanent class-relevant outcome should normally be recorded as an **Echo-wide event/milestone** once completed.

Example:
- "Sacrifice Excalibur to the Woman in the Lake";
- the item is consumed/replaced as authored;
- an Echo-wide milestone records that the event happened;
- that milestone can unlock a class now or much later.

Requirement timing is unrestricted by tier.

A Tier-5 event can unlock a previously hidden **Tier-1 class** for future play.

## No permanent content lockouts

DiceFree should avoid irreversible choices that permanently prevent the player/Echo from accessing another class, Way or major progression path.

Do not use "choose A, permanently lose B" as normal class-progression structure.

Different quests/routes may have different flavor and sequencing, but players should be able to pursue the other unlocks later.

The Echo/timeline framework exists to encourage exploration of possibilities, not punish curiosity.

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

## Unlocking never direct-starts a class

Echo-wide discovery/unlock makes a Way **available to pursue**. It does not create that class save and does not allow direct-starting the class from the title/class screen.

To create a missing class save:
1. load an appropriate earlier-tier/prerequisite class save;
2. travel to and complete the target class's advancement quest/event;
3. advance normally;
4. the new class save is created from that parent timeline snapshot.

Example:
- a Tier-5 event reveals/unlocks a hidden Tier-1 Way;
- the player loads the preserved Tier-0 Novice;
- travels to that newly available Tier-1 advancement quest;
- completes it;
- the Tier-1 class save is created.

The preserved lower-tier saves are therefore important parts of the class-web progression loop.

## Way discovery

Discovering/unlocking knowledge that a Way exists is Echo-wide.

Once a Way is discovered:
- the Echo retains that knowledge permanently;
- future eligible manifestations can see/use that discovery according to the class's rules;
- discovery does **not** mean eligibility;
- eligibility does **not** mean a save-state already exists;
- the first time the Echo actually becomes that class, its one class save-state is created through the normal timeline fork.

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

## One save-state per class

Current direction: the Echo has **at most one persistent manifestation/save-state per class ID**.

The roster therefore grows by unlocking and becoming new classes rather than by creating duplicate copies of the same class.

Examples:
- one Ranger save-state;
- one Mystic Knight save-state;
- one Wizard save-state.

If the same class can be reached through multiple convergence routes:
- the **first time** the Echo becomes that class, its class save-state is created from the advancing manifestation's normal timeline snapshot;
- later qualifying routes do not create a second copy of the class and do not overwrite the existing class save-state;
- route-specific quests/dialogue/discoveries may still exist independently where useful.

This prevents duplicate Ranger #1 / Ranger #2 / Ranger #3 timelines while preserving a huge class roster.

### Starting / switching classes

The unlocked/discovered class UI can double as the **start-game manifestation selector**.

The player chooses an existing class save-state and continues that class's timeline.

This replaces the need for a traditional fixed-size character-slot screen.

### Archive and cleanup

Because the class roster can become enormous:
- **Archive** is the preferred non-destructive cleanup tool;
- archived class save-states are hidden from the normal/default picker but remain recoverable;
- permanent deletion may also exist for players who truly want to discard class-local progress.

If a class save is permanently deleted:
- the Way remains discovered/unlocked Echo-wide;
- all Echo-wide milestones/events earned through that class remain;
- the deleted class cannot be recreated directly;
- the player must load an appropriate earlier-tier/prerequisite class save and complete that class's advancement again.

Deleting or archiving a class save-state must never erase:
- earned Echo-wide class/tier milestones;
- discovered Ways;
- Echo-wide unlock events;
- bank/account progression.

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

## Visibility presentation refinements

Class visibility is deliberately allowed to vary much more than a single global hidden/visible rule.

Examples that the data/UI framework should support include:
- visible class silhouette with the exact unlock requirements shown;
- visible silhouette with cryptic requirements/hints;
- a deliberately opaque large 3D question-mark presentation;
- a class absent from the Ways screen until the player discovers **how** it can be unlocked;
- a class absent from the selection/Ways UI until it has actually been unlocked;
- other authored combinations of identity visibility and requirement disclosure.

Do not infer that two hidden classes must use the same secrecy pattern.

The presentation itself may be part of the mystery and should be authored per Way.

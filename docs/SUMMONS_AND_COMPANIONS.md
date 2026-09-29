# Summons, Pets and Controllable Units

## Philosophy

DiceFree should treat summons, pets, companions, turrets, clones, charmed units and other owned actors as variations on one reusable ownership/control framework.

Simple autonomous summons should be easy to author.

Complex directly-controlled companions must also be possible without creating a separate combat engine.

## Party slots

Owned summons/companions do **not** consume the normal 1-4 player party slots.

A four-player party may therefore also contain whatever owned units each class/content definition permits.

## No universal gameplay summon cap

There is **no game-wide gameplay summon cap** such as "maximum 3 summons."

Summon limits belong to the summoning class/ability/content definition.

Common limiting patterns include:
- maximum active instances of a summon;
- maximum active instances across a summon family/tag;
- temporary lifetime;
- resource/upkeep limitation;
- replacement of oldest/current summon;
- other authored rules.

Examples:
- Necromancer may cap Skeletons at 3;
- another class may have one permanent companion;
- another ability may summon 20 temporary creatures for 5 seconds.

The generic framework should enforce authored limits cleanly without inventing a global design cap.

## Ownership and control

Every owned unit can identify:
- immediate actor instance;
- owner;
- current controller;
- ownership/control tags.

Owner and controller are not necessarily the same actor/player.

This allows:
- normal player-owned summons;
- temporary mind control;
- control transfer;
- borrowed companion control;
- charmed enemies;
- encounter-driven control swaps.

Control transfer does not imply ownership transfer unless explicitly authored.

## Control modes

Support:
- **autonomous** — AI controls the unit;
- **directly controllable** — selectable and commandable like an RTS unit;
- **hybrid** — autonomous behavior plus selected stance/order commands.

A unit exposes only the controls appropriate to its design.

## Selection and command UI

When a directly controllable owned unit is selected:
- the shared WC3-style command grid presents that unit's available commands/abilities;
- generic hero controls should not be duplicated in a second bespoke UI.

Current selection direction:
- **F1** selects/returns to the hero;
- **F2+** can address/cycle owned controllable units as the final key model is refined.

The exact hotkey layout remains part of input/UI design.

## Generic orders

Framework-level unit orders should support at least:
- Move;
- Attack;
- Stop;
- Hold Position;
- Follow;
- Attack-Move where appropriate.

Owned units may additionally expose authored abilities/commands.

Not every summon must support every generic order.

## Stances

Generic stance vocabulary:
- Aggressive;
- Defensive;
- Passive;
- Hold Position.

A summon/companion can expose only the stances it actually supports.

Specific classes may define additional bespoke stances.

## Stats and inheritance

Owned units support multiple stat models:

### Fully authored stats
The summon has its own authored base/combat stats.

### Snapshot inheritance
Relevant owner stats are sampled when the unit is created.

### Dynamic inheritance
Relevant owner stats are recalculated/live-linked while the unit exists.

### Mixed formulas
Definitions can combine authored base values and owner scaling.

Example:

```
Summon Max HP = 100 + (Owner VIT x 4)
Summon Damage = 20 + (Owner INT x 0.7)
```

The framework must not assume all inherited stats use one universal percentage.

## Level

Owned summons/companions do **not** have independent character levels.

They use the **owner's current level** for any level-based rules/scaling.

Persistent companions may still have other persistent systems later, such as:
- equipment;
- traits;
- bond/progression flags;
- unlocked abilities;

but not a separate XP/character-level track unless this design is explicitly revisited.

## Resources

Owned units may:
- have their own resource pools;
- share/consume an owner's resource;
- use multiple generic resources;
- have no resource.

Use the shared generic resource framework.

## Threat

Owned units have their own threat entries by default.

Content can additionally define threat relationships such as:
- transfer a percentage/all generated threat to owner;
- transfer owner threat to summon;
- redirect threat;
- suppress threat;
- inherit/copy threat under explicit mechanics.

This supports tank pets and unusual aggro mechanics without making all pets behave the same way.

## Death and revival

Normal summon death is **not player death**.

A summon may:
- die/despawn;
- leave a corpse;
- become disabled;
- become revivable by an ability;
- be resummoned after cooldown/resource cost;
- follow other authored behavior.

Summons do not interact with resurrection stones unless a specific mechanic explicitly says so.

## Lifetime and persistence

Owned units may use different lifetime scopes:

- **temporary** — duration/action-limited;
- **combat-persistent** — remains through a combat/encounter then disappears;
- **session-persistent** — remains during the current game/session;
- **manifestation-persistent** — durable companion belongs to that class manifestation across saves.

Definitions may add more specific lifecycle rules.

## Identity

Temporary disposable units may use runtime-only instance identity.

Persistent companions require stable persistent instance IDs.

Stable identity enables:
- saved equipment;
- persistent traits/state;
- references from quests/abilities;
- clean save migrations.

## Equipment

The framework allows owned companions to equip gear.

Most summons need not expose equipment slots.

Equipment-capable companions should use the normal item/stat/effect/eligibility systems where practical rather than a parallel pet-item system.

## Inventory

Owned units have **no generic inventory requirement**.

The framework may permit an authored companion to have inventory/storage if a future class/mechanic genuinely needs it.

Do not build all summons around inventory assumptions.

## Friendly targeting

Owned summons/companions are valid allied units for healing/buffing by default.

Abilities can include/exclude them through semantic target tags such as:
- Player;
- Summon;
- Pet;
- Companion;
- NPC;
- Construct;
- Undead;
- other authored tags.

This allows both broad ally heals and deliberately player-only/support-specific abilities.

## Progression / quest credit

Owned-unit actions retain both:
- immediate source;
- owner/controller attribution.

Normal player XP/quest credit can therefore resolve summon kills/actions to the owner.

Specific requirements may deliberately ask for:
- the summon itself to land the kill;
- a named companion action;
- pet-origin damage;
- other source-sensitive conditions.

## Collision

Owned units follow the same normal physical unit-collision/pathing rules as other actors by default.

No special party/summon soft-collision exception is committed yet.

If real playtesting shows crowd/pathing problems, solve the observed problem deliberately rather than pre-authoring an invisible exception.

Individual units/effects can still explicitly gain ghosting/phasing through the existing collision framework.

## Networking / authority

In multiplayer:
- host is authoritative for summon state and autonomous AI;
- controlling clients submit commands/intentions for units they control;
- owner/controller attribution remains explicit;
- host migration transfers owned-unit live state with the rest of the recoverable session state;
- reconnect restores control of surviving/reserved owned units where applicable.

Persistent companion state ultimately belongs to the owning player's manifestation save, while live session state remains host-authoritative.

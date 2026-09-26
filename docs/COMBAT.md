# Combat

## Direction

DiceFree should use real-time 3D isometric action-RPG combat.

Current working assumptions:
- PC-first controls;
- **mouse-driven click-to-move is the primary/default control style and primary balance target**;
- WASD/direct movement is also supported as an alternative control style;
- mouse aiming/targeting/context interaction;
- a compact but class-dependent active toolkit;
- readable enemy attacks and boss mechanics;
- distinct class resources;
- strong hit feedback without unreadable visual noise.

## Control schemes

DiceFree supports both Classic click-to-move and Direct WASD movement.

### Classic / mouse movement

The primary control scheme, inspired by Warcraft III, Dota and League of Legends.

Typical behaviour:
- click the ground to move;
- movement uses pathfinding around valid obstacles;
- mouse input handles targeting and contextual actions;
- abilities receive targets/positions independently of the movement system.

This is the **design and balance baseline** for movement speed, encounter spacing, boss telegraphs, kiting, attack ranges, cast times and other movement-sensitive mechanics.

### Direct / WASD movement

Players may instead control movement directly with WASD.

WASD is a supported first-class option, but it should not become the reference point that causes encounter design to assume action-game movement precision unavailable to click-to-move players.

### Shared movement architecture

```
Classic click-to-move ─┐
                       ├─> movement intent -> character motor -> gameplay
Direct WASD movement ──┘
```

Stuns, roots, slows, speed modifiers, combat restrictions, animation and networking should operate consistently regardless of control style.

A future Hybrid option that accepts both click-to-move and WASD simultaneously may be useful, but it is **not yet a committed requirement**.

## Movement and mobility

There is **no universal dodge/roll**.

Mobility is part of class identity:
- some classes may have dashes, teleports, rolls or other evasive tools;
- some may have none;
- classes may have different base movement speeds;
- stronger mobility should come with appropriate tradeoffs elsewhere in the kit.

## Enemy aggro and leashing

Overworld enemies have:
- an aggro/acquisition range;
- a maximum pursuit/leash distance or equivalent return-home rule.

Enemies can follow a player across a cube edge **if the player remains within their valid pursuit rules**.

Once the enemy has chased too far from its home/encounter area, it gives up and returns/reset to its home state.

The goal is to let the cube-world behave physically without producing absurd cases where a starter-zone slime follows somebody across three faces into endgame territory.

## Ability loadouts

DiceFree should keep the number of buttons relatively small, but there is **no requirement that every class has the same number of active abilities**.

A class may lean toward:
- more active abilities;
- more passive abilities;
- transformations;
- summons;
- stance/resource mechanics;
- a very compact core kit.

Advancement may add, evolve or replace abilities depending on that class.

## Resources

Different classes can use different resources.

Later advancements may retain, modify or completely replace the previous resource system.

There is no universal resource that every class is required to use.

## Party design

The initial multiplayer target is **up to 4 players**.

Classes may naturally lean toward:
- durability/tanking;
- support/healing;
- ranged damage;
- melee burst;
- summoning;
- control;
- hybrid roles.

Ordinary progression should not require a fixed MMO trinity composition.

## Abilities

Abilities should be modular enough to support:
- direct damage;
- area damage;
- projectiles;
- summons;
- damage over time;
- healing/shields;
- movement;
- crowd control;
- resource generation/spending;
- conditional effects;
- item-modified variants;
- resurrection.

Ability behaviour should not depend on whether the player uses Classic or Direct movement.

## Resurrection

Healer-type resurrection can be repeatable, but intentionally expensive/risky:
- long cast time;
- high mana/resource cost;
- stacking Resurrection Sickness applied to the revived player.

This creates room for recovery without making repeated deaths free.

## Bosses

Boss encounters should contain recognizable mechanics rather than only inflated health.

Harder dungeon/raid modes should be able to add:
- new attacks/mechanics;
- tighter timings;
- modified arena hazards;
- stronger enemy combinations;
- increased stats;
- altered/new loot tables.

Movement-sensitive mechanics must be tested primarily with Classic click-to-move so WASD does not quietly become required for reliable execution.

## Open questions

- Exact click/attack/context command conventions.
- Whether Hybrid mode should ship.
- Exact targeting behaviour.
- Potion/item hotkeys.
- Controller support expectations.
- Threat/taunt system depth.
- Whether basic attack is universal or class-specific.

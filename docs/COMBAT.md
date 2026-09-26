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

## Basic attacks

Every class normally has a basic attack.

The default interaction should feel like Warcraft III / League:
- command an attack on an enemy;
- the character moves into valid attack range if needed;
- once in range, the character repeatedly performs its basic attack while the target remains valid;
- ranged classes fire from range;
- melee classes close into melee.

Most caster classes are expected to have ranged basic attacks, while some magical/melee classes can use melee basic attacks.

A future class with no conventional basic attack is possible, but would be an intentional special-case class mechanic rather than a normal rule.

Every class has a **primary stat** used in its basic-attack scaling. The exact stat list and formula remain to be designed.

## Health and primary stats

**Vitality** contributes to maximum HP.

Current conceptual formula:

```
Max HP = class base HP + (Vitality × HP coefficient) + other modifiers
```

Open design question: whether the Vitality-to-HP coefficient is universal or class-specific.

Each class has a primary stat used for base/basic attack scaling.

Other primary/derived stats are not yet finalized.

## Defense and equipment families

There is one common defense model rather than multiple armor-weight defense types.

There is no generic Cloth/Leather/Mail/Plate defense hierarchy.

Classes instead differ through:
- which equipment families they are allowed to equip;
- the stat packages on those equipment families;
- item requirements;
- class abilities/passives.

Equipment **Intrinsics** can provide larger fixed stat packages than their DiceBound equivalents and are an important part of gear identity.

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

Enemies can follow a player across a cube edge if the player remains inside valid pursuit rules.

Once they exceed their leash, they return/reset to their home state.

## Threat

DiceFree will have a real **threat/aggro system** so tank-oriented classes can intentionally control enemies.

Bosses and special enemies may override ordinary threat with mechanics such as:
- forced target swaps;
- random/marked targets;
- proximity targeting;
- scripted fixates;
- untankable mechanics.

Threat is a system, not a promise that every enemy always attacks the highest-threat character.

## Ability loadouts

DiceFree should keep the number of buttons relatively small, but classes do not need identical active/passive counts.

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

## Party design

The initial multiplayer target is **up to 4 players**.

Ordinary progression should not require a fixed MMO trinity composition, but tanking/healing/support identities are absolutely allowed and supported.

## Healing/support targeting

Support abilities should support both:
- targeting characters in the 3D world;
- targeting eligible allies through party frames/UI.

Party-frame targeting is important so healing does not require pixel-hunting moving characters in an isometric battlefield.

## Consumables

Consumables use cooldowns.

Different consumables can have different cooldown rules.

Classes, passives, equipment or profession-related effects may modify how consumables work, including cooldowns or effectiveness.

## Resurrection

Healer-type resurrection can be repeatable, but intentionally expensive/risky:
- long cast time;
- high mana/resource cost;
- stacking Resurrection Sickness applied to the revived player.

## Enemy level display

Enemy levels should normally be visible.

For enemies dramatically beyond the current character's level, displaying **???** instead of the exact level is an available design option to preserve mystery/threat. The threshold/rule is not yet decided.

## Bosses

Boss encounters should contain recognizable mechanics rather than only inflated health.

Harder dungeon/raid modes should add mechanics as well as stats.

## Open questions

- Exact mouse-button/context conventions.
- Whether Hybrid mode should ship.
- Exact targeting behaviour for offensive abilities.
- Potion/item hotkeys.
- Controller support expectations.
- Exact threat formula/taunt rules.
- Final primary-stat list.
- Universal vs class-specific Vitality-to-HP coefficient.
- Exact enemy-level threshold for displaying ???.

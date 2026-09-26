# Regions

DiceFree is planned around **six major overworld regions**, corresponding to the six outer faces of the Dice.

## Dice-face identity

The six faces are ordered like the faces of a **d6**.

The **one-dot face** is the starting region, the **two-dot face** is the next intended region, continuing through the six-dot face.

Each overworld should contain large, visually distinct landmarks arranged so the region/map/minimap can evoke that face's pip arrangement.

The goal is that zooming out or looking at the regional map makes the world itself read like a die face rather than merely naming the regions "one through six."

## Progression and route choice

The six faces have an intended progression order expressed through:
- fixed regional enemy levels/difficulty;
- lightweight main-story guidance;
- class-specific advancement quests;
- encounter difficulty;
- loot progression.

Players may travel into later regions early.

**Enemies do not scale to the player.**

Higher-level and endgame pockets can exist inside earlier regions. Returning to the one-dot starting face at endgame should still reveal dangerous hidden content.

Example tone: a perpetually burning house in the starting town might eventually reveal a secret dungeon containing something like the Pale Devil.

## Quest structure

The main quest is lightweight and primarily points the player through the world.

Side quests provide leveling, gear, resources, worldbuilding and discoveries.

Advancement quests are class-specific and can deliberately violate the normal region order.

## Towns and camps

### Towns

Towns provide the broad service set:
- vendors;
- NPC gear crafters;
- profession/resource services;
- bank withdrawal/access;
- healers;
- resurrection-point setting;
- quest NPCs;
- class advancement;
- other major services.

### Camps

Camps provide only a subset of town services.

They can act as:
- resurrection points;
- local rest/healing points;
- limited vendors/services;
- travel points where appropriate.

A camp is not a replacement for a proper town.

## Fast travel

Fast travel is **travel-point to travel-point** rather than teleport-from-anywhere.

Most travel points connect into useful networks, but not every point connects directly to every other point.

Some missing/limited connections should have in-world/lore explanations rather than feeling arbitrary.

## Traversal between faces

The intended fantasy remains that faces physically meet at cube edges and gravity/orientation changes when crossing.

However, **continuous everywhere-accessible edge traversal is not yet a hard technical commitment**.

World geometry may deliberately restrict crossings using:
- mountains;
- forests;
- cliffs;
- walls;
- dangerous terrain;
- roads/passes;
- settlements/bridges.

This creates authored crossing routes and reduces technical/pathfinding/camera chaos.

We should prototype both:
1. a genuinely continuous edge crossing;
2. a more segmented/streamed world where crossing routes transition between face spaces while visually preserving the cube-world illusion.

The final implementation should prioritize game feel, stability and production feasibility over proving that every centimeter of every cube edge is physically traversable.

## Camera direction

If continuous edge traversal is used, the preferred camera behaviour is:
- align ultimately to the new face's local gravity/up;
- smoothly blend through the 90-degree change;
- preserve player framing/isometric angle;
- avoid gratuitous rolling/spinning.

## Projectiles and surface effects

Preferred fantasy, subject to technical validation:
- projectiles/gravity-sensitive effects should behave coherently as local gravity/orientation changes;
- effects that are intended to follow terrain should conform to the local surface;
- behavior at face boundaries must be prototyped rather than hard-coded from assumptions.

If fully continuous curved projectile behaviour becomes technically expensive or unreadable, authored crossing zones/segmented faces are an acceptable solution.

## Enemy traversal

Enemies may follow across face transitions only while within normal aggro/leash rules.

They never chase indefinitely across the world.

## Respawning

Normal monsters respawn after a content-defined timer.

Named enemies/elites use longer timers.

Exact timers are encounter/content-specific.

## Hidden high-level content

Endgame and secret content should be distributed throughout all six faces rather than concentrated only in the final region.

Earlier zones can hide:
- endgame dungeons;
- secret bosses;
- unusual advancement quests;
- late-game crafting requirements;
- account-wide secrets;
- dangerous subzones.

## To define

1. Exact DiceBound-board theme mapped to each pip face.
2. Regional level bands.
3. Major landmarks/pip arrangement for each face.
4. Edge/corner treatment.
5. Continuous cube vs segmented/streamed implementation after prototype.
6. Day/night, sky and horizon behaviour.

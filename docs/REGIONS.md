# Regions

DiceFree is planned around **six major overworld regions**, corresponding to the six outer faces of the Dice.

## Region rule

Each face should be inspired by the identity of its matching DiceBound board, but translated into a full 3D RPG region with its own culture, ecology, dungeons and progression.

There are six primary overworld zones in total, but each face can contain many towns, dungeons, raids, caves, interiors and other sub-areas.

## Progression and route choice

The six faces have an intended progression order expressed through:
- fixed regional enemy levels/difficulty;
- the lightweight main quest;
- class-specific advancement quests;
- encounter difficulty;
- loot progression.

Players can choose alternate routes and physically travel elsewhere earlier if they want.

**Enemies do not scale to the player.** Zones have fixed intended difficulty. A low-level character can enter a much later face and encounter enemies far beyond their ability to fight.

This is intentional.

## Quest structure

The main quest is relatively lightweight and primarily exists to:
- provide context;
- point players toward the intended next areas;
- introduce important towns/dungeons;
- move the broader Dice story forward.

Side quests provide:
- leveling help;
- equipment;
- resources;
- worldbuilding;
- optional discoveries.

Advancement quests are class-specific and can intentionally break regional order. A stealth-oriented class could, for example, be sent to infiltrate a high-level capital long before it could fight the surrounding enemies directly.

## Towns and settlements

Towns can provide:
- vendors;
- crafting;
- resurrection points;
- healers;
- quest/NPC services;
- class advancement;
- shared bank access.

Exact town count per face is not fixed.

## Traversal between faces

Faces are physically connected.

At an edge, the player can walk over onto the neighboring face and gravity rotates/changes to match the new surface.

There is no normal gateway/portal between adjacent faces.

### Camera direction

Current preferred camera behaviour for edge traversal:

- the camera is ultimately aligned to the **local gravity/up direction** of the face the player is standing on;
- crossing an edge blends smoothly between the old and new face orientation;
- the camera should travel through a controlled quarter-turn rather than snap 90 degrees;
- player framing, distance and isometric viewing angle should remain as stable as possible during the blend;
- the transition should visibly communicate that the player has walked around a cube edge without gratuitous spinning/rolling.

This must be prototyped early. Comfort and readability take priority over preserving a mathematically literal camera orientation.

## Enemy traversal

Enemies can physically follow players across edges when their aggro/leash rules permit it.

They should not pursue indefinitely. Once outside their allowed pursuit range, they return to their home/encounter area.

## Working structure for each region

Potential content per face:
- one or more towns/hubs;
- overworld questing;
- class-specific advancement content;
- enemy families;
- handcrafted dungeons;
- occasional procedural/randomized dungeons;
- handcrafted raids;
- named elites and secret bosses;
- region-specific item sets and artifacts;
- secrets that deepen the Dice mystery.

## To define

1. Map each DiceBound board to an outer-face theme.
2. Decide the first face players emerge onto after The Last Equation.
3. Define intended level/progression bands for all six faces.
4. Decide whether edges/corners contain special content beyond traversal.
5. Prototype gravity/camera/navigation behaviour at a cube edge.
6. Decide how day/night and the sky/horizon work across faces.

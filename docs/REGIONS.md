# Regions

DiceFree is planned around **six major overworld regions**, corresponding to the six outer faces of the Dice.

## Region rule

Each face should be inspired by the identity of its matching DiceBound board, but translated into a full 3D RPG region with its own culture, ecology, dungeons and progression.

There are six primary overworld zones in total, but each face can contain many towns, dungeons, raids, caves, interiors and other sub-areas.

## Progression and route choice

The six faces have an intended progression order expressed through:
- level/difficulty scaling;
- the main quest;
- class-advancement locations/quests;
- encounter difficulty;
- loot progression.

Players can choose alternate routes and physically travel elsewhere earlier if they want. The game should encourage the intended order rather than enforce it with arbitrary locks wherever possible.

Smaller side quests can be scattered across all regions.

## Traversal between faces

Faces are physically connected.

At an edge, the player can walk over onto the neighboring face and gravity rotates/changes to match the new surface.

There is no normal gateway/portal between adjacent faces.

This requires an early technical prototype because it affects:
- player orientation;
- camera behaviour;
- navigation/pathfinding;
- physics/gravity;
- enemies crossing edges;
- mounts/pets if later added;
- visual horizon and sky treatment.

## Working structure for each region

Potential content per face:
- one or more towns/hubs;
- overworld questing;
- advancement content placed according to intended progression;
- enemy families;
- handcrafted dungeons;
- occasional procedural/randomized dungeons;
- raids/raid-like encounters;
- named elites and secret bosses;
- region-specific item sets and artifacts;
- secrets that deepen the Dice mystery.

Exact counts are not locked.

## Dungeons

Most dungeons should be handcrafted.

Some dungeon types may use procedural/RNG-generated layouts or content for replayability.

Dungeon completion is intended to be one of the major sources of equipment progression.

## To define

1. Map each DiceBound board to an outer-face theme.
2. Decide the first face players emerge onto after The Last Equation.
3. Define intended level/progression bands for all six faces.
4. Decide whether edges/corners also contain special content beyond traversal.
5. Prototype gravity/camera/navmesh behaviour at a cube edge.
6. Decide how day/night and the sky/horizon work across faces.

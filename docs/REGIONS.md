# Regions

DiceFree is planned around **six major overworld regions**, corresponding to the six outer faces of the Dice.

## DiceBound visual inheritance

Each DiceFree face takes its visual/environmental DNA from the matching DiceBound Board screen background **and** Normal combat background.

See `docs/DICEBOUND_WORLD_VISUAL_REFERENCES.md`.

The mapping is now fixed:
- Face 1 <- Board 1 / Green Road / welcoming woodland;
- Face 2 <- Board 2 / Astral Road / misty rocky forest;
- Face 3 <- Board 3 / Fractured Road / desolate ruined-fortress/graveyard mood;
- Face 4 <- Board 4 / Crown Road / enchanted cursed swamp and forgotten ruins;
- Face 5 <- Board 5 / Oblivion Ringroad / volcanic mountain fortress;
- Face 6 <- Board 6 / End of Mathematics / emberlit ashen gothic citadel.

These are inspiration anchors, not literal 1:1 recreations.

## Biome blending between faces

A face's DiceBound-derived theme is a **dominant regional identity**, not the only biome allowed on that face.

Faces may contain multiple biomes.

Preferred world logic:
- the central/interior portions of a face express that face's strongest visual identity;
- terrain closer to an edge can gradually pick up environmental influence from the neighboring face;
- corners can blend influences from three faces and are allowed to become especially strange;
- transitions should feel like one continuous world rather than six themed maps touching at hard seams.

Examples:
- a green Face 1 forest near an edge bordering a colder/mistier face might become rockier, foggier and less pastoral;
- a border toward a volcanic face might become drier, ashier or geothermally active before the gravity transition itself.

This is a worldbuilding/design principle, not a demand for uniform gradient blending everywhere. Mountains, rivers, climate, magic and local geography can create sharper transitions where appropriate.

## d6 identity

The one-dot face is the starting region, then two-dot through six-dot in intended progression order.

Regions should contain distinct landmarks arranged so the map/minimap can evoke the corresponding d6 pip arrangement.

Exactly how literally each pip position maps to a city/mountain/lake/etc. is still open.

## What inhabitants believe

The inhabitants **do not generally understand that their world is a die**.

To them, this is simply what a world is like.

People know:
- other regions/faces exist;
- roads/trade routes can cross between them;
- gravity/orientation behaves as their world normally behaves.

They do **not** normally interpret the whole world as a manufactured gaming die.

A fun lore possibility is an NPC who insists the world is literally a die and is widely regarded as a crank/lunatic.

## Trade and politics

NPCs routinely travel between faces where routes permit.

There can be:
- trade;
- diplomacy;
- wars;
- alliances;
- migration;
- roads/passes crossing boundaries;
- political relationships spanning multiple faces.

The six regions should feel connected, not like six sealed dimensions.

## Progression

Enemy levels are fixed by zone; no player scaling.

Later/endgame content can be hidden in any face, including the starter face.

## Towns and camps

Towns provide full services.

Camps provide subsets.

## Fast travel

Travel-point to travel-point.

Not all connections need to exist; missing links should have lore/world reasons.

## Face crossing

The fantasy remains physical adjacency between faces, but implementation remains open between seamless local-gravity traversal and segmented/streamed transitions.

Authored geography may restrict crossings to selected roads/passes.

## Projectiles and effects

Preferred fantasy: projectiles and terrain-bound effects behave coherently across face transitions if the implementation supports it.

Prototype before committing to technically expensive global behaviour.

## Respawning

Normal monsters respawn on timers. Named/elites respawn more slowly.

## Open questions

- Exact DiceBound-board theme per face.
- Regional level bands.
- How literally landmarks map to pip positions.
- Corner treatment where three faces meet.
- Seamless vs segmented traversal.
- Day/night, sky and horizon rules.


## Production priority: build Face 1 first

Do **not** attempt to fully design all six faces in parallel.

The current world-design priority is:
1. define the one-dot starter face;
2. build a small starting area;
3. prove town/quest/combat/dungeon traversal there;
4. expand that face outward in authored chunks;
5. only then use lessons from Face 1 to shape later faces.

The other five faces can retain high-level thematic placeholders until World 1 is working.

## Incremental world expansion

World 1 should initially expose only a deliberately small playable area.

Undeveloped future territory can be blocked using believable world geometry such as:
- dense forests;
- mountains;
- collapsed roads;
- cliffs;
- rivers;
- walls/gates;
- dangerous terrain;
- landslides;
- construction;
- story-appropriate obstacles.

As development expands the region, those blockers can be removed/reworked to open new roads and subregions.

This is both a production strategy and a world-design rule:
- avoid invisible walls where natural blockers work;
- keep new expansions connected to the existing geography;
- let roads, sightlines and landmarks hint that the world continues beyond the current playable boundary.

## Face 1 goals

Before seriously designing Face 2, Face 1 should have a coherent answer for:
- starting arrival point;
- starter town;
- first resurrection point;
- first bank/vendor/healer/crafter services;
- first outdoor combat loop;
- first enemy families;
- first side quests;
- first main-quest guidance;
- level-10 advancement path into Physically Blessed Novice / Magically Touched Novice;
- first dungeon entrance;
- first boss;
- first meaningful loot-room reward;
- first secret/late-game tease;
- travel-point rules;
- visual landmark/pip identity;
- at least one future expansion boundary that can later be opened naturally.

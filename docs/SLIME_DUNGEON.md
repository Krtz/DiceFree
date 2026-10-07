# First Slime Dungeon

Status: initial vertical-slice design target.

## Purpose

The first Slime Dungeon proves the complete DiceFree dungeon loop rather than trying to be a huge content piece.

Target first blind clear: roughly **8–12 minutes**.

Player loop:

World Face 1 / Cornberg-side entrance -> staging -> short dungeon -> Slime Boss -> private loot room -> world return.

## Session rules

- Physical entrance in the overworld.
- Separate Unity map/scene: `SlimeDungeon`.
- Stable dungeon ID: `dungeon.slime`.
- One live occupancy for `dungeon.slime` per hosted session.
- If another party/run already owns it, entry is blocked.
- Other dungeon IDs may run simultaneously.
- Other players may remain on any overworld face simultaneously.
- Default staging countdown: 60 seconds.

## Initial layout

The first blockout should stay compact:

1. **Staging room / cave mouth**
   - test dummy;
   - final equipment preparation;
   - visible route into the dungeon after run start.

2. **Slime approach**
   - 2–3 small slime encounters;
   - teach target selection, AoE and positioning without a major mechanic.

3. **Short traversal / split path space**
   - establishes enough physical room for the later conditional Regent route;
   - ordinary low-level path remains obvious.

4. **Slime Boss arena**
   - compact readable arena;
   - boss has exactly two attacks in the first version.

5. **Private loot room**
   - one reward choice per eligible participant;
   - reward must be resolved before normal exit.

6. **Exit**
   - returns to the authored world destination.

Do not inflate the first blockout with extra rooms just to make it feel like a 'real dungeon'. The vertical slice exists to validate the complete system.

## First Slime Boss

Working stable ID: `boss.slime`.

The first Slime Boss has exactly two attacks:

### 1. Slime Slam

- clear telegraph;
- physical impact around/in front of the boss depending on final animation;
- teaches moving out of a readable danger area;
- witnessing it discovers the Codex mechanic `mechanic.slime-slam`.

### 2. Divide

- boss divides/spawns smaller slimes;
- the mechanic should change target priority rather than merely add visual clutter;
- exact recombine/heal tuning is a later encounter-balance decision;
- witnessing it discovers the Codex mechanic `mechanic.divide`.

No puddle/trail attack in this first boss version.

## High-level conditional route: Slime Regent

The dungeon condition framework, not scene-specific code, controls this route.

Initial authored condition:

**average participating player level >= 50**

At or above the threshold:

- blocking trees/roots/geometry on the traversal section disappear/open;
- `route.slime-regent` becomes accessible;
- `boss.slime-regent` becomes eligible;
- `loot.slime-regent` becomes part of the run variant;
- discovering the route records `secret.slime-regent` in the Codex.

The condition result is snapshotted when the run begins. The route does not pop in/out because party state changes after that.

The **Slime Regent** is a higher-level optional route/boss, but is not the final hard slime boss concept.

**Slime Queen / Slime King** remain later higher-level hard-boss content.

## Loot

Normal Slime Boss and Slime Regent use distinct authored loot definitions.

Initial vertical-slice requirement:

- normal completion reaches a private loot room;
- chosen equipment reward becomes durable only through the successful completion/reward flow;
- observed dungeon loot feeds Echo-wide Codex discovery;
- Regent route can supply a different reward pool without creating a separate dungeon ID.

Exact item list is still to be designed.

## Codex events exercised by this dungeon

- exact slime monster kill;
- monster drop observed;
- Slime Slam witnessed;
- Divide witnessed;
- dungeon wipe;
- dungeon player death;
- Slime Boss defeated;
- dungeon completed;
- loot observed;
- Regent route discovered;
- Regent defeated/loot observed later.

## Reset behavior

- trash remains dead during an active successful attempt;
- failed boss pull resets that encounter;
- full party wipe resets the full dungeon by default;
- abandon destroys/resets the run and releases occupancy;
- completed/finished cleanup releases occupancy;
- stale-lock recovery exists for failed cleanup/crashes.

## Not in the first blockout

- Slime Queen;
- Slime King;
- multiple difficulty modes;
- elaborate dungeon art;
- final loot tables;
- final Codex art;
- multiple simultaneous copies of Slime Dungeon;
- extra boss attacks beyond Slime Slam and Divide.

## Definition already authored

`Assets/_DiceFree/Settings/World/Dungeon - Slime.asset` currently defines:

- `dungeon.slime`;
- scene ID `SlimeDungeon`;
- 60-second staging;
- conditional variant `variant.slime-regent.level50`;
- route `route.slime-regent`;
- boss `boss.slime-regent`;
- loot `loot.slime-regent`;
- tag `secret.slime-regent`.
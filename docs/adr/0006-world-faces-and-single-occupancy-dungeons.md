# ADR 0006: Separate World-Face Maps and Single-Occupancy Dungeons

- Status: Accepted
- Date: 2026-10-07

## Context

DiceFree presents its overworld as the exterior of a cube-shaped world with six traversable faces. Building one literal six-sided gravity/navmesh world would tightly couple camera, traversal, physics, navigation and content authoring to a visual trick that does not need to be physically real.

Multiplayer also requires players in one hosted session to be able to occupy different parts of the world at the same time.

Dungeons are physical places in that world, but DiceFree should retain the old-school ORPG feeling that a dungeon is a shared place rather than an unlimited per-party copy factory.

## Decision

### Six world faces

The six cube faces are authored as **six separate maps/scenes**.

Crossing a world edge is presented as one continuous cube-world traversal:
- player reaches an authored edge;
- movement briefly hands off to the transition;
- the world/camera performs a cube-rotation transition;
- destination map content is made ready;
- the player appears at the linked destination edge/spawn.

The transition deliberately hides the technical scene/map boundary.

Do not implement literal cube gravity or one six-sided NavMesh merely to preserve the illusion.

### Concurrent face occupancy

World-face maps are **not exclusive**.

Within one hosted session:
- Player A may be on Face 1;
- Player B may be on Face 4;
- other players may occupy either face or other faces.

The host/session therefore tracks participant location independently from the local player's currently rendered map.

Map runtime/loading must be capable of keeping the logical simulation for all occupied maps alive. The exact scene/physics loading implementation may evolve behind this contract.

### Dungeon maps

Dungeons are separate authored maps/scenes entered through physical world entrances.

A dungeon template is identified by stable dungeon ID.

### One live occupancy per dungeon ID

A hosted session may have **at most one live occupancy for a given dungeon ID**.

Example:
- Party A enters dungeon.slime;
- dungeon.slime is now occupied;
- Party B cannot create or enter another simultaneous copy of dungeon.slime;
- Party B may still enter dungeon.cave if that different dungeon is free;
- other players may remain on any world face.

This is an intentional design rule, not an implementation limitation.

The occupancy uses an ownership/lease token so stale clients cannot release a newer run.

Abandon/reset/completion cleanup must release the occupancy. A stale-lock recovery mechanism must exist for crashes/failed cleanup.

### Staging and active run

The existing staging design remains:
- first eligible entrant claims the dungeon and enters staging;
- 60-second countdown by default;
- eligible party members may join the same staging occupancy;
- run start snapshots the participating roster;
- equipment/class locks begin at run start;
- new participants cannot join after staging closes.

### Conditional dungeon variants

Dungeon layout/content may change from deterministic authored conditions.

Supported condition categories include:
- average participating player level;
- class composition;
- quest/event state;
- Echo-wide flags/discovery;
- session/world flags;
- required items/keys;
- achievements.

Conditions may enable/alter:
- blockers, trees and doors;
- routes and rooms;
- enemies and bosses;
- mechanics/phases;
- loot definitions;
- Codex discoveries.

When a run begins, the relevant condition outcome is captured as a **DungeonRunVariantSnapshot**. The active run should not unpredictably reshape itself because a player's state changes later.

### Slime Dungeon example

The first low-level Slime Dungeon ends at the basic **Slime Boss**.

The first Slime Boss has exactly two attacks:
1. Slime Slam;
2. Divide.

A reusable high-level conditional route will be prototyped with:
- condition: average participating player level >= 50;
- blocking trees/geometry disappear or open;
- hidden route becomes accessible;
- the route can lead to the **Slime Regent**;
- the Regent route uses different loot and Codex discovery.

The Slime Regent is not the final expression of high-level slime content.

**Slime Queen / Slime King** remain future higher-level hard-boss content.

## Consequences

### Positive

- Six-face fantasy is preserved without six-face physics complexity.
- Faces can be authored, baked, lit and optimized independently.
- Different players can legitimately be on different faces.
- Dungeon exclusivity feels like a shared ORPG world rather than an MMO instance factory.
- Different dungeons can run simultaneously.
- Conditional routes are reusable data rather than one-off scene scripts.
- High-level revisits can reveal genuinely new dungeon content without duplicating dungeon definitions.

### Costs / follow-up

- Hosted sessions need a map-presence/runtime layer rather than one global active scene.
- Network replication must scope actors/world state by map/location.
- Scene loading/physics isolation for multiple simultaneously active maps remains an implementation detail to harden.
- Disconnect/host-migration recovery must carry dungeon occupancy/run state.
- World-edge transition presentation still needs authored animation/VFX.
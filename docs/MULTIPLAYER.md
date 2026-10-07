# Multiplayer and Sessions

## Core model

DiceFree is a **lobby/session co-op game**, not an MMO.

Initial target: **1–4 players**.

Flow:
1. create/join lobby;
2. invite/join friends including Steam invites;
3. start session;
4. adventure together.

## Network authority

DiceFree uses a **host-authoritative session model**.

During a hosted session:
- clients send gameplay intent/commands;
- the host resolves authoritative world state;
- the host resolves enemies/AI;
- the host resolves damage/healing/effects;
- the host resolves guest HP/resources/cooldowns while they are in the session;
- the host resolves live quest/event/world actions;
- the host resolves loot/drop outcomes for the session.

Guest save ownership remains separate from live simulation authority. After authoritative outcomes are earned, eligible durable progress is persisted to each player's own Echo/manifestation.

Solo, LAN and online multiplayer should share the same gameplay command/event paths where practical. Do not maintain an unrelated "single-player combat engine" and "multiplayer combat engine."

## World-map concurrency

The cube overworld is implemented as six separate world-face maps/scenes while being presented as one connected world.

Players in the same hosted session may occupy different world faces at the same time. World faces are not exclusive session instances.

Crossing an authored face edge uses a hidden cube-rotation/load transition and moves only that participant to the linked face/location; it does not globally change the whole party's map.

See `docs/adr/0006-world-faces-and-single-occupancy-dungeons.md`.

## Joining started sessions

Players may join an already-started overworld session.

They spawn at the latest resurrection point registered by the class save they joined with, even if the existing party is currently inside a dungeon.

They do not teleport directly to the party.

## Switching class saves

Class switching is available **only through resurrection stones**.

There is no generic town/menu class swap away from a stone.

When switching during a multiplayer session:
- the party/session remains intact;
- the old class manifestation leaves play;
- the chosen existing class save enters the session at **that class's own latest registered resurrection point**;
- switching never creates a missing class save; new classes still require their advancement quest;
- switching does not move items between class inventories or the shared bank.

Starting/loading an existing class uses the same spawn rule: appear at that class's latest registered resurrection point.

If that saved resurrection point is unavailable/invalid for the current game/session, **Cornberg is the fallback spawn**.

Title-screen selection of existing class saves is also supported.

Class level does not restrict switching. A player may switch to a much higher- or lower-level class save; multiplayer scaling/mentor rules are a separate system.

## Host migration

**Host migration is a core multiplayer requirement**, not a nice-to-have.

If the current host disconnects/crashes:
- another eligible connected participant should be able to become host;
- the party/session should continue whenever technically recoverable;
- already-earned durable player progress must not be discarded.

### Session-state preservation

Preferred behavior is to preserve the **current live session state**, including relevant:
- host-world event state;
- defeated/alive enemies;
- encounter state;
- transient world objects;
- current party state;
- active dungeon state when applicable.

Architecture should therefore avoid keeping the only copy of essential session state solely in the host process.

A suitable implementation may use replicated authoritative snapshots/checkpoints plus deterministic/ordered event state as needed.

If perfect live-state migration cannot be recovered after a failure:
- fall back to the safest recoverable session/checkpoint state;
- preserve each player's already-earned durable XP, gold, loot, quest/event credit and other committed progression;
- do **not** silently throw away hours of durable session progress.

Exact replication cadence, election mechanism and failure fallback remain technical design.

### Dungeon host migration

Host migration should also work during active dungeons/raids.

A long dungeon should not automatically fail because the original host disconnected.

Preserve the active run state where recoverable; use safe recovery behavior rather than defaulting to total progress loss.

## Dungeon joins

Dungeon participation uses a staging phase and a **single occupancy per stable dungeon ID**.

There are no simultaneous per-party copies of the same dungeon inside one hosted session. If that dungeon ID is occupied by another party/run, entry is blocked. Different dungeon IDs may still be active at the same time.

When the first party member enters a free dungeon:
- that player enters the shared dungeon waiting room;
- a 60-second countdown begins;
- other eligible party members may enter the same waiting room during that window.

When the countdown ends:
- the dungeon run actually starts;
- participating players are fixed;
- equipment becomes locked for the run;
- no additional player may join that active dungeon/raid.

A player joining the broader overworld session after the dungeon has started does not teleport into or join the active run.

## Temporary disconnect and reconnect

A temporarily disconnected player remains represented in the live session during a reconnect grace period.

Direction:
- their actor remains physically in the world;
- the actor does not become invulnerable merely because the connection dropped;
- it may remain idle and can die/be affected normally;
- the player's roster/session slot is reserved during the reconnect window;
- another player cannot take a locked dungeon participant's slot.

If the player reconnects before timeout:
- they resume the same live actor/state at its current position/state.

If the grace period expires:
- their actor leaves the active session according to normal cleanup;
- a later join follows normal join/spawn rules;
- active-dungeon participation cannot be replaced by another player.

Exact reconnect grace duration remains tuning/technical work.

## Quest credit

Quest/event credit is **policy-driven per objective/reward**, not one universal multiplayer rule.

Supported directions include:
- global session/party credit;
- nearby-radius credit;
- threat/combat-participation credit;
- authoritative last-hit credit.

A single enemy death can therefore be consumed differently by different systems.

Eligibility is still evaluated separately from world-state availability.

See `docs/QUESTS_AND_INTERACTIONS.md`.

### Configurable credit policies

Combat should expose enough semantic participation data for quest/XP/achievement consumers to decide their own credit rules.

Do not encode "everyone nearby gets credit" or "last hit wins" directly into enemy death logic.

## Experience in parties

Players should not be punished simply for grouping with friends.

Eligible nearby/participating players receive their own XP reward rather than dividing one finite XP pool between party members.

XP may be reduced based on **level difference between the character and the defeated enemy**.

Exact level-gap formula remains a balance problem.

## Veteran + new-player co-op

### Optional mentor scaling

The preferred direction is **optional mentor scaling**, not mandatory universal down-scaling.

A veteran can choose to enter an assist/mentor mode when playing with lower-level friends.

Old content still remains naturally overpowerable when the veteran does not opt into mentor scaling.

Exact mentor formulas, activation UX, reward handling and how it interacts with dungeons remain open.



A core social goal is:

**a veteran player should be able to meaningfully play with a new player without either character's progression being ruined.**

DiceFree should support veterans helping newer friends while preserving:
- the new player's sense of progression;
- the veteran's ability to use their existing manifestation;
- the identity of old content;
- sane XP gains;
- useful group play.

Possible tools include:
- level-difference XP curves;
- optional or content-specific level sync;
- temporary stat normalization for selected activities;
- mentor-style scaling;
- reward normalization;
- dungeon/mode-specific sync rules.

No single sync solution is committed yet.

## Powerleveling

Powerleveling is allowed in principle.

A veteran should be able to help a lower-level manifestation progress faster.

However, powerleveling should not collapse the entire leveling curve into trivial instant jumps.

The final model should distinguish **playing together/helping a friend** from **breaking progression completely**.

## Old content

High-level players can normally return to old content and overpower it.

There is no universal mandatory down-sync for the entire game.

If sync exists, it should be selective enough that becoming powerful still feels meaningful.

## World/session state

Some state is manifestation-specific, some Echo-wide, some session-based.

### Host-authoritative world presentation

The **host's world/quest state determines the live session's physical world and available world events**.

Guests see the host's current reality.

Example:
- if the host completed an event where a house burned down, every player sees the burned remains;
- if a guest still has a quest to talk to an NPC who only existed inside the intact house, that quest is simply **not completable in this game/session**;
- the guest's own quest state is not failed, completed or overwritten;
- the guest can do it later in another game/session whose world state still supports that quest step.

This follows the same general session logic as an action-RPG game where a boss already killed in the host's game cannot be killed again in that same game.

### Guest quest/world progress when compatible

Guests can permanently progress their own timelines while playing in another player's hosted world.

Examples:
- if the host already finished a Slime-kill quest but Slimes still exist naturally, an eligible guest can kill them for their own quest;
- if host and guest both participate in the event where a house burns down and both receive eligible event/world-state credit, **both players' own timelines record that outcome**;
- either player can later host a game where their own timeline contains the burned house;
- if a guest joins only after that event already happened, they merely see the host's burned house and do **not** automatically inherit the event.

Rules:
- host state determines the physical world currently shown;
- guests persist XP, loot, quest progress, milestones and world/event outcomes they legitimately earn;
- events can define persistence-recipient/credit policy;
- ordinary cooperative progression should normally credit eligible participants rather than the host alone;
- joining never blindly copies the host's timeline into the guest's save;
- if the needed NPC/object/event no longer exists in the host's world, that quest step is unavailable for that session;
- no per-player contradictory version of the same physical object is required inside one session.

## Pause

### Solo
Solo play supports a **true game pause**.

Opening the appropriate pause state can stop world/combat simulation.

### Multiplayer
Multiplayer supports **vote pause**.

A successful pause vote pauses the shared gameplay simulation for everyone.

Exact vote threshold, timeout, cooldown and anti-abuse rules remain open.

## Lobby visibility and discovery

Support multiple session discovery/join modes:
- Steam friends/invites;
- Invite Only;
- Friends;
- **Public stranger lobbies**;
- LAN discovery;
- direct connection/address-style joining where technically appropriate.

Public multiplayer is intentionally supported; DiceFree is not restricted to premade friend groups.

LAN must work without cloud/internet dependency.

Direct/LAN networking should remain compatible with ordinary local/private networking setups, including virtual-LAN tools that present peers as reachable local/private endpoints.

## Vote kick

Multiplayer supports **vote kick**.

Vote-kick exists so public/stranger sessions are not dependent on the host manually policing every disruptive player.

Exact rules remain open, including:
- vote threshold;
- who may initiate;
- cooldown;
- whether the target votes/counts;
- host-target behavior;
- behavior inside active dungeon runs;
- post-kick rejoin restrictions.

## Steam

Steam lobby/invite integration is a PC-release target.

## Not planned

- MMO shard/open world;
- random strangers appearing without explicitly joining a session;
- raid lockouts;
- unrestricted item economy.

# Multiplayer and Sessions

## Core model

DiceFree is a **lobby/session co-op game**, not an MMO.

Initial target: **1–4 players**.

Flow:
1. create/join lobby;
2. invite/join friends including Steam invites;
3. start session;
4. adventure together.

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

## Dungeon joins

Dungeon participation uses a staging phase.

When the first party member enters:
- that player enters the shared dungeon waiting room;
- a 60-second countdown begins;
- other eligible party members may enter the same waiting room during that window.

When the countdown ends:
- the dungeon run actually starts;
- participating players are fixed;
- equipment becomes locked for the run;
- no additional player may join that active dungeon/raid.

A player joining the broader overworld session after the dungeon has started does not teleport into or join the active run.

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

## Steam

Steam lobby/invite integration is a PC-release target.

## Not planned

- MMO shard/open world;
- random strangers appearing without joining session;
- raid lockouts;
- unrestricted item economy.

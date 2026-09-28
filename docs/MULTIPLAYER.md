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

## Switching class saves in a session

Current direction: players may switch to another existing class save **from a safe town** without leaving the multiplayer session.

When switching:
- the party/session remains intact;
- the old class manifestation leaves play;
- the chosen class save enters the session at **that class's own latest registered resurrection point**;
- switching never creates a missing class save; new classes still require their advancement quest.

Starting a new game/session with an existing class uses the same rule: spawn at that class's latest registered resurrection point.

Title-screen class switching remains naturally supported as well.

## Dungeon joins

A player cannot join a dungeon/raid already in progress.

Participation is fixed when the run starts.

## Quest credit

Eligible players receive quest credit.

Ineligible players do not.

This applies to side quests as well as main/other quests.

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

The **host's world/quest state determines the live session's world presentation**.

Example:
- if a host has completed an event where a house burns down, everyone in that hosted session sees the burned remains;
- a guest whose own class save has not completed that event does not get a separate intact house rendered only for them.

Joining another player's world does **not** overwrite the guest's manifestation-specific quest/world progression.

Quest/event credit remains eligibility-based.

The exact interaction rules for cases where the host's world state removes or changes an NPC/object that a guest's personal quest still expects are a separate design problem and must not be solved by silently advancing/locking out the guest.

## Steam

Steam lobby/invite integration is a PC-release target.

## Not planned

- MMO shard/open world;
- random strangers appearing without joining session;
- raid lockouts;
- unrestricted item economy.

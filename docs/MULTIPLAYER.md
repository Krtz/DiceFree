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

They spawn at the resurrection point their manifestation last explicitly selected, even if the existing party is currently inside a dungeon.

They do not teleport directly to the party.

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

## Steam

Steam lobby/invite integration is a PC-release target.

## Not planned

- MMO shard/open world;
- random strangers appearing without joining session;
- raid lockouts;
- unrestricted item economy.

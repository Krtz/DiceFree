# Multiplayer and Sessions

## Core model

DiceFree is a **lobby/session co-op game**, not an MMO.

Initial target: **1–4 players**.

The intended flow is similar in spirit to Warcraft III custom games:
1. create or join a lobby;
2. invite/join friends, including Steam invites;
3. start the game session together;
4. continue adventuring in the shared session.

## Joining a game already in progress

Players may join an overworld session after it has already started.

When joining:
- the joining manifestation loads into the session;
- they spawn at the **town/resurrection point that manifestation most recently set**;
- they do not teleport directly to the existing party.

This remains true even if the current party is inside a dungeon.

## Dungeon and raid joins

A player **cannot join a dungeon or raid that has already started**.

Dungeon participation is locked to the eligible party members present when the run begins.

This protects:
- wipe/reset rules;
- encounter state;
- reward eligibility;
- private loot-room results;
- mechanic scaling.

A late joiner can remain in the overworld/town and join the party after the current run ends.

## Quest credit

Multiplayer quest credit is eligibility-aware.

If a player is eligible for a quest/objective when the party completes it, they receive appropriate credit.

If they are not eligible, they do not receive that quest completion merely because they were present.

Exact rules can vary by quest type.

## World ownership/state

Some quest/event state belongs to the manifestation, some to the Echo account, and some may be session/world state.

The exact authority model should support:
- manifestation-specific quests;
- account-wide discoveries;
- shared session encounters;
- host/session progression where necessary.

Do not assume every piece of world state must belong to the host permanently.

## Steam

Steam lobby/invite integration is a target for PC release.

The exact networking stack/transport is not decided yet.

## Scaling

Encounter systems should support scaling based on current valid party size.

Actual scaling formulas are a playtest/balance problem, not a pre-production assumption.

## Not planned

At the current stage:
- no MMO shard/world;
- no strangers wandering through your overworld unless they joined your lobby/session;
- no raid lockouts;
- no mandatory matchmaking service requirement;
- no unrestricted player-to-player item economy.

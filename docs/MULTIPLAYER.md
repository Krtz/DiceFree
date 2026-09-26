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

They spawn at the **resurrection point their manifestation last explicitly selected**, even if the existing party is currently inside a dungeon.

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

Eligible nearby/participating players receive their own full XP reward rather than dividing one finite XP pool between party members.

However, XP can be reduced by **level difference between the character and the defeated enemy**.

Exact level-gap formula remains a balance problem.

## Powerleveling

Powerleveling is allowed in principle.

A high-level player should be able to help a lower-level manifestation level faster.

However, it should not trivialize the entire progression ladder.

Possible tools to prototype later include:
- enemy/character level-gap XP curves;
- maximum useful XP per kill;
- FFXIV-style level sync for selected content;
- minimum participation/range rules;
- dungeon/mode-specific sync.

No specific sync model is committed yet.

## World/session state

Some state is manifestation-specific, some Echo-wide, some session-based.

## Steam

Steam lobby/invite integration is a PC-release target.

## Not planned

- MMO shard/open world;
- random strangers appearing without joining session;
- raid lockouts;
- unrestricted item economy.

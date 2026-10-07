# Codex

## Ownership

Codex progression is **Echo-wide**.

Switching manifestation/class does not create a new Codex.

Map terrain exploration remains manifestation-specific and is not Codex progression.

## Discovery principle

The Codex records what the player has actually discovered.

Do not spoil unseen content with mystery placeholder rows. If the Echo has not discovered something yet, it normally does not appear.

## Monsters

Monster families may group related variants, but exact variants keep exact records.

An exact monster/variant is discovered when the Echo kills it.

Track at least:
- exact monster ID;
- kill count;
- observed drops;
- witnessed boss mechanics where applicable.

## Drops

A drop is added only after it has actually been observed dropping.

For each discovered drop track:
- stable drop/item ID;
- times seen drop.

When all authored drops for an entry have been observed, UI may state that all drops are discovered.

Undiscovered drops remain absent rather than occupying mystery slots.

## Boss mechanics

Boss mechanics are recorded when witnessed.

Store stable mechanic IDs and let authored content provide the player-facing text.

Mechanic discovery is idempotent: seeing Slime Slam fifty times still discovers one Slime Slam mechanic entry.

Mechanics may later be grouped by phase/state.

## Dungeons

Track at least:
- clears;
- wipes;
- bosses killed, including counts by boss ID;
- player deaths;
- completed modes;
- solo clears;
- no-death clears;
- fastest clear separately by party size;
- loot discoveries;
- authored secret route/room/boss discoveries.

Normal clean/gold dungeon completion requires:
- every authored mode completed;
- every authored drop observed.

Specific dungeons may add extra completion requirements or rewards.

## Rewards

Optional per-entry rewards may include:
- title;
- cosmetic;
- achievement;
- tiny damage bonus against the **exact mob**;
- combinations;
- nothing.

Exact-mob damage bonuses must remain tiny and tightly capped.

Do not create broad family damage bonuses that turn Codex completion into mandatory endgame optimization.

Rewards trigger automatically when their authored requirement is satisfied.

## Semantic events

Codex should consume semantic gameplay events rather than being hard-coded into each encounter.

Initial event vocabulary:
- monster killed;
- drop observed;
- boss mechanic witnessed;
- dungeon wiped;
- dungeon player death;
- dungeon boss killed;
- dungeon secret discovered;
- dungeon completed.

Quest, achievement, class-requirement and Codex consumers can eventually listen to the same semantic event stream where their eligibility rules overlap.

## Persistence

Codex uses the generic Echo-wide durable-state contract.

Current section:
- ID: echo:codex;
- version: 1.

It is additive to existing Echo saves.

Missing Codex section means an empty Codex and is not save corruption.

Class fork/switch/autosave commits include Echo-wide durable sections atomically with the rest of the Echo revision.

## First UI

The first UI is deliberately functional.

Initial tabs:
- Monsters;
- Dungeons.

It shows discovered entries only.

Final art, search/filtering, family navigation and completion presentation remain later polish.

## First authored content target

Start with:
- Cornberg slime family;
- first Slime Dungeon;
- first Slime Boss;
- Slime Slam;
- Divide;
- high-level Slime Regent route discovery;
- dungeon loot discoveries.

Slime Queen / Slime King remain later high-level hard-boss Codex content.
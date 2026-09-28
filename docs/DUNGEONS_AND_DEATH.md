# Dungeons, Raids, Death and Failure

## Party size

Initial multiplayer target: 1–4 players.

## Dungeons and raids

Raids are larger/more ambitious handcrafted dungeons, not lockout-scheduled MMO raids.

Dungeon size is content-specific:
- single boss;
- short dungeon;
- multi-boss run;
- very long raid-like content.

No raid lockouts.

## Entrances

Dungeons have physical world entrances.

Entry requirements are decided case by case and may include:
- level;
- quest;
- key/item;
- achievement;
- class/secret condition;
- mode unlock.

Recommended level/difficulty can normally be shown, but secret content may deliberately obscure information.

### Staging / waiting room

The first eligible player who enters a dungeon is moved into that dungeon's **staging room**.

Rules:
- a **60-second countdown** begins when the first player enters;
- other eligible party members who enter during the countdown arrive in the same staging room;
- the staging room contains at least a **test dummy** so players can check attacks/build feel while waiting;
- the dungeon run has **not started yet** during staging;
- equipment is not yet locked during staging;
- players may make their final loadout preparations before the countdown ends.

When the 60-second countdown expires:
- the actual dungeon run starts;
- the participating roster is fixed for that run;
- equipment is locked according to the dungeon/raid equipment-lock rule;
- no new player may join that active run.

The waiting room is part of dungeon lifecycle infrastructure, not merely a loading screen.

## Active-run state and reset

### Trash / normal encounters

Defeated dungeon trash normally **does not respawn on a timer** during an active attempt.

It remains defeated until the run resets.

### Boss reset

If a boss encounter ends without victory, the boss resets cleanly:
- full HP;
- adds/minions reset;
- phases reset;
- arena/encounter state resets;
- authored boss-event state resets;
- temporary encounter state is cleared.

A party cannot slowly chip permanent boss progress across failed pulls.

### Full wipe

Default behavior: a **full party wipe resets the entire dungeon run**.

That means, by default:
- trash respawns/reset;
- bosses reset;
- encounter events reset;
- run-local state returns to its initial state.

The framework must allow explicit authored exceptions later.

### Voluntary abandonment

If the active party abandons/leaves the dungeon run, the run is destroyed/reset.

A half-cleared dungeon is not parked indefinitely for later continuation.

## Abandoning and re-entry

A run can be abandoned.

An abandoned run cannot be re-entered.

## Quitting / persistence during a dungeon

Active dungeon run state is not permanently saved.

Quitting/leaving abandons the active run and it will not resume from the same trash/boss state later.

However, durable rewards already legitimately earned remain saved, including:
- XP earned;
- gold earned.

This does not bypass the rule that equipment rewards are granted only through successful completion/private loot rooms.

Loot-room reconnect remains a short-lived recovery exception rather than normal dungeon persistence.

## Completion reward

The dungeon is considered successfully completed when its authored completion condition is met, normally defeating the final boss.

On completion:
- each participating eligible player receives access to their own **private loot room/reward choice**;
- reward generation uses that dungeon/mode's authored loot definition;
- the player must resolve/choose their reward before leaving the loot room;
- there is no normal "leave now, choose the reward days later" pending-choice flow.

### Loot-room reconnect

If a player disconnects while in the loot room, they may reconnect back into that loot room **within an authored/time-limited reconnect window**.

The exact reconnect duration remains to be tuned.

After the allowed reconnect window expires, unresolved reward handling remains a separate policy/design question.

Equipment is awarded only after successful completion via private loot rooms.

## Individual death inside a dungeon

A dead player remains resurrectable by healer resurrection **until that player chooses to self-respawn**.

Once self-respawn is chosen, the dungeon's authored death/return policy decides where the player appears.

Supported dungeon policies include:
- a dungeon-start/checkpoint resurrection point;
- the player's currently registered world resurrection point;
- other explicit dungeon-specific checkpoint behavior.

This is intentionally **case by case**. Not every dungeon provides an internal checkpoint.

The framework must preserve the distinction between:
- healer resurrection inside the active run;
- voluntary self-respawn through the dungeon's return policy.

Exact re-entry/participation consequences for dungeon policies that return a player outside the active run should remain explicit per dungeon rather than inferred globally.

## Overworld death

On overworld death:
- allies can resurrect before respawn is accepted;
- player can explicitly accept respawn;
- respawn occurs at the explicitly set resurrection point;
- the overworld does not reset.

## Resurrection points

Set explicitly by interacting with an appropriate object/location such as a shrine, fountain, stone, beacon, statue or other region-specific structure.

## Equipment, consumables and class identity during a run

When the staging countdown ends and the run begins:
- equipped gear/loadout becomes locked for the run;
- equipment cannot be swapped inside the active dungeon/raid;
- ordinary consumables/items remain usable;
- carried consumables can still be rearranged/managed as normal where the inventory rules allow it;
- class switching is disabled for the active run;
- resurrection-stone class switching is not available inside the dungeon.

The class manifestation that enters the run is the manifestation used for that run.

## Dungeon wipe

Default: full run reset, as described above. Explicit future dungeons may define exceptions.

## Resurrection

Healer resurrection:
- repeatable;
- long cast;
- high resource cost;
- applies stacking Resurrection Sickness.

Current working sickness:
- each stack gives roughly **-10% to the five primary attributes only**:
  - Vitality
  - Strength
  - Agility
  - Intelligence
  - Spirit

Derived combat values then fall naturally from those attributes.

Exact duration, cap and stacking math remain open.

## Death losses

Carried gold loses a percentage and the lost amount disappears.

Banked gold is safe.

Other materials/currencies decide death-risk rules individually.

## Old content

High-level characters can normally return to old dungeons and overpower them.

There is no automatic universal down-sync for old content.

Selected content may later use sync/mentor rules where helping lower-level players or preserving encounter design benefits from it.

## Difficulty modes

Harder modes can add:
- mechanics;
- phases;
- enemies;
- stat pressure;
- altered loot tables.

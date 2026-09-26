# Dungeons, Raids, Death and Failure

## Party size

The initial multiplayer target is **up to 4 players**.

Systems should be written so encounter scaling by party size is possible, but the exact balance formulas are deliberately left for playtesting.

## Dungeons and raids

A raid is fundamentally a larger/more ambitious **handcrafted dungeon**, not a separate MMO schedule system.

Current principles:
- most dungeons are handcrafted;
- some dungeon types may use procedural/RNG-generated layouts or content;
- raids are handcrafted;
- there are **no raid lockout timers**;
- replaying content for loot is allowed.

## Completion rewards

Successfully completing a dungeon/raid sends each player to their own **private loot room**.

Each player chooses one reward from a generated selection based on that dungeon/raid/mode's drop table.

The main reward therefore comes from **finishing the run**, not from competing with party members for one chest drop.

A full wipe means the group never receives that completion reward.

## Death outside a wipe

When a character dies:
- another player can resurrect them if an available class/ability allows it;
- some classes may have self-revive mechanics;
- otherwise the character respawns at an appropriate camp/resurrection point.

Death carries a modest penalty:
- lose some gold;
- lose a **small** amount of experience.

Exact percentages and whether experience loss can reduce a level remain to be balanced later.

## Resurrection

Healer-type classes may be able to resurrect repeatedly rather than having a universal hard encounter limit.

Repeated resurrection is controlled through cost and risk:
- resurrection has a **long cast time**;
- resurrection has a **high mana/resource cost**;
- revived characters gain a **stacking Resurrection Sickness** debuff.

The exact sickness penalties, duration and whether stacks can expire during combat remain to be designed.

Some classes may have self-revive mechanics.

## Dungeon wipes

The default rule for **all dungeons and raids** is a **full reset on party wipe**.

That means:
- restart the run from the beginning;
- no completion loot from the failed run;
- previously defeated encounters do not stay cleared.

Exceptional content may deliberately use different rules later, but full reset is the baseline.

Procedural dungeon seeds may be regenerated or preserved on wipe depending on the eventual dungeon design; this remains an implementation/content decision.

## Scaling

Encounter systems should be built so party-size scaling is possible.

The exact scaling model will be determined through balancing and playtesting rather than locked before the combat game exists.

Potential scaling dimensions include:
- enemy health;
- damage;
- number of enemies;
- mechanic target counts;
- mechanic timing;
- role requirements.

Scaling should not become a purely mathematical HP sponge multiplier.

## Difficulty modes

Dungeons and raids can have multiple modes.

Harder modes should increase more than raw stats. They can add:
- new boss mechanics;
- altered phases;
- tighter timing;
- additional enemy behaviours;
- changed arena hazards;
- stronger stat tuning;
- mode-specific loot tables/rewards.

Exact mode names and progression are not yet locked.

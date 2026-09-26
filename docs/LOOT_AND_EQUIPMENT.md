# Loot and Equipment

## Principle

Loot should be relatively meaningful rather than constantly exploding from every enemy.

DiceFree will use deliberate **drop tables**.

The primary route to significant equipment upgrades is **successfully finishing dungeons and raids**, supplemented by rare overworld enemy drops, quests, crafting and other systems added later.

## Dungeon and raid rewards

When a dungeon or raid is successfully completed, **each player is sent to a private loot room**.

In that room, the player is presented with a generated selection from the relevant completion drop table and may choose **one** reward.

Important implications:
- each player receives their own reward choice;
- party members do not compete through Need/Greed for the main completion reward;
- class/equipment restrictions can be considered when designing loot-table quality;
- harder modes can have distinct or expanded reward tables;
- wiping means never reaching the private loot room.

Exact choice counts, table sizes and rarity weighting remain to be designed.

## Overworld drops

Enemies in the overworld can drop items, but these drops should be **quite rare**.

Overworld enemy drops are **free-for-all** rather than privately instanced.

Aggro/leash rules should prevent players from dragging ordinary enemies indefinitely across the world merely to farm or grief.

## Binding

**All items are account-bound.**

Items can move between the player's character manifestations through the shared account bank, subject to their normal class/type/level/stat requirements.

There is no current plan for unrestricted player-to-player equipment trading.

## Equipment slots

Current planned equipment slots:

1. Head
2. Shoulders
3. Chest
4. Hands
5. Legs
6. Feet
7. Main Hand
8. Off Hand
9. Ring
10. Amulet
11. Back

The Back slot covers things such as cloaks, capes and other suitable back-mounted items.

Not every slot needs to be usable by every class, and not every slot must always have a visible mesh if doing so would produce poor visual results.

## Visible equipment

Equipped gear should be represented on the character wherever practical.

All modular character equipment should be designed around shared rig/attachment conventions where the involved class form allows it.

## Equipment requirements

Equipment is **not universally usable**.

Items may have one or more requirements such as:
- class;
- class family/type;
- equipment proficiency;
- level;
- stats;
- other special conditions.

Advancement can change what equipment a character is allowed to use.

The system should support interesting restrictions without turning every item into unusable clutter.

## Stats

Each class has its own level-1 base stats and class-specific automatic stat growth.

Later advancement tiers generally begin from a higher level-1 base stat package.

Players do not manually allocate stat points.

Equipment provides additional stats and can also provide special effects.

## Technical art rule

Character, armor and weapon standards must be decided early so we do not later discover that hundreds of items cannot fit the supported class forms.

Likely concepts:
- shared humanoid rig conventions where practical;
- skinned armor meshes for body equipment;
- attachment sockets for weapons/shields/back items;
- hide-body/hide-hair rules where necessary;
- swappable materials and variants.

The exact body/presentation strategy must account for the fact that **class form is the main character visual identity**.

## Rarity direction

Exact rarity names are not locked, but DiceBound's progression philosophy is useful:
- ordinary statistical items;
- stronger rare items;
- build-changing legendary-style items;
- named/mythical items with fixed identity/intrinsics;
- extremely rare boss/dungeon artifacts.

## Item design

A memorable item can alter:
- an ability;
- a summon;
- status application;
- resource economy;
- movement;
- survivability;
- party interaction;
- class passive.

## Storage

A **shared account bank** is committed.

Other storage systems may include:
- character inventory;
- character-specific bags;
- collection/codex;
- heirloom-like account items.

Exact bank size, tabs and expansion systems are not yet locked.

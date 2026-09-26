# Loot and Equipment

## Principle

Loot should be relatively meaningful rather than constantly exploding from every enemy.

DiceFree uses deliberate **drop tables**.

The primary route to significant equipment upgrades is **successfully finishing dungeons and raids**, supplemented by rare overworld enemy drops, quests, NPC crafting and other systems.

## Dungeon and raid rewards

When a dungeon or raid is successfully completed, **each player is sent to a private loot room**.

In that room, the player is presented with a generated selection from the relevant completion drop table and may choose **one** reward.

The number of offered choices is content/difficulty dependent rather than globally fixed.

The selection may contain equipment intended for other classes/manifestations. Because items are account-bound, the player may choose such an item and send it to the shared bank.

Important implications:
- each player receives their own reward choice;
- party members do not compete through Need/Greed for the main completion reward;
- loot tables are authored per content/mode;
- wiping means never reaching the private loot room;
- finishing the dungeon is what produces equipment loot.

## Bosses inside dungeons

Dungeon/raid bosses can reward:
- experience;
- gold;
- crafting materials/resources;
- progression/quest credit.

They do **not** directly award equipment before the run is complete.

Equipment is awarded through successful completion/private loot-room rewards so farming an early boss and resetting cannot bypass the dungeon-clear requirement.

## Overworld drops

Enemies in the overworld can drop equipment, but these drops should be **quite rare**.

Overworld equipment drops are **free-for-all** rather than privately instanced.

Normal overworld monsters respawn after a timer. Named/elites use longer respawn timers.

## Item generation

DiceFree uses **both** randomized and handcrafted equipment.

DiceBound is a major reference for:
- stat rolls;
- rarity rolls;
- randomized affixes/effects;
- unusual combinations.

DiceFree should also contain more deliberately authored gear:
- named items;
- boss/dungeon items;
- crafted transformation items;
- Mythical/Artifact-like gear;
- items with fixed identity or bespoke effects.

The two approaches can coexist: some named items may have a fixed identity while still rolling within controlled ranges.

## Binding

**All items are account-bound.**

Items can move between manifestations through the shared bank, subject to normal class/type/level/stat requirements.

There is no current plan for unrestricted player-to-player equipment trading.

## Equipment slots

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

The Back slot covers cloaks, capes and other suitable back-mounted items.

## Bank and inventory

Inventory/bank design should be generous rather than built around constant inventory friction.

Rules:
- items may be **sent/deposited to the account bank from anywhere**;
- items may only be **withdrawn from the bank while in a town**;
- the bank itself should have generous capacity;
- crafting materials/resources should generally behave more like currencies/resource counters than physical inventory-stack clutter.

This lets dungeon loot be preserved for another manifestation without turning every run into inventory Tetris, while towns still matter because withdrawing/organizing stored gear requires returning to civilization.

## Durability

**There is no equipment durability system.**

No repair tax. No durability loss on death.

## Equipment requirements

Equipment is not universally usable.

Items may require:
- class;
- class family/type;
- equipment proficiency;
- level;
- stats;
- other special conditions.

Advancement can change available equipment categories.

## Stats

Each class has its own level-1 base stats and class-specific automatic stat growth.

Later advancement tiers generally begin from a higher level-1 base stat package.

Players do not manually allocate stat points.

Equipment provides additional stats and special effects.

## NPC gear crafting

Major equipment crafting is performed by **NPC crafters**, not as a player gear-crafting profession.

Crafting should often transform meaningful existing items/resources into a new authored reward.

Example structure:

```
Longsword from Dungeon 4
+ Heart of the Dragon raid boss
+ 500 rarity crystals
= Longsword of the Dragon
```

Exact recipes can require:
- specific equipment;
- boss trophies/materials;
- rarity currencies;
- quest items;
- gold.

Crafted gear should complement dungeon progression rather than obsolete it, often requiring dungeon/raid loot as ingredients.

## Player professions

Player professions should focus on consumables/resources rather than primary equipment crafting.

Possible areas:
- potions;
- food;
- temporary buffs;
- gathering/resource production;
- support consumables.

Exact professions remain to be designed.

## Visible equipment

Equipped gear should be represented on the character wherever practical.

Character/armor standards must be prototyped before mass asset production.

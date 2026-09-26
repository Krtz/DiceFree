# Loot and Equipment

## Principle

Loot should be relatively meaningful rather than constantly exploding from every enemy.

DiceFree uses deliberate **drop tables**.

Major equipment progression comes primarily from successfully finishing dungeons/raids, supplemented by rare overworld equipment drops, quests and NPC crafting.

## Dungeon and raid rewards

Successful completion sends each player to a **private loot room**.

The room generates a content-specific number of reward choices from that dungeon/raid/mode's tables.

There is **no universal baseline number of choices**. A short dungeon, huge raid, secret boss dungeon and hard mode can all use different offer counts.

The player:
- can fully inspect every offered item;
- chooses exactly one equipment reward;
- may equip/keep it or send it directly to the shared bank;
- may choose gear intended for another manifestation/class;
- loses all unchosen offers when leaving.

If none of the equipment offers are desirable, the player can instead take an alternate reward such as:
- experience;
- gold;
- materials/resources.

Exact fallback amounts are content-specific.

### Rare jackpots

Loot tables may contain extremely rare jackpot items.

Whether an item:
- can appear on normal mode at microscopic odds;
- is exclusive to a harder mode;
- has better odds on harder modes;
- requires special conditions

is decided **case by case per dungeon/item**.

## Bosses inside dungeons

Dungeon/raid bosses award:
- experience;
- gold;
- materials/resources;
- progression/quest credit.

They do **not** award equipment before successful completion.

## Overworld drops

Overworld equipment drops are comparatively rare and free-for-all.

Normal monsters respawn on timers; named/elites use longer timers.

## Item generation

DiceFree uses both randomized and handcrafted equipment.

DiceBound is a reference for:
- stat rolls;
- rarity rolls;
- randomized affixes/effects;
- strange combinations.

DiceFree should also contain more bespoke gear:
- named items;
- dungeon/boss themed gear;
- crafted transformations;
- Mythical/Artifact-like items;
- fixed/bespoke effects.

## Intrinsics and equipment families

DiceFree keeps the idea of **Intrinsics**, but they can represent larger fixed stat packages than in DiceBound.

There is one defense type rather than armor-weight defense types.

Different equipment families instead distinguish themselves through:
- intrinsic stat packages;
- affix pools;
- class permissions;
- special effects.

For example, two chest-piece families can provide very different offensive/support stat profiles while still feeding the same underlying Defense stat.

## Binding

All items are **account-bound**.

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

## Bank and inventory

- generous inventory/bank philosophy;
- send/deposit items to bank from anywhere;
- withdraw only in town;
- materials generally behave like currencies/resources instead of inventory junk.

## Durability

There is **no durability system**.

## Requirements

Items may require:
- class;
- class family/type;
- equipment proficiency;
- level;
- stats;
- other special conditions.

## NPC gear crafting

NPC crafters transform meaningful equipment/material combinations into authored upgrades.

Example:

```
Longsword from Dungeon 4
+ Heart of Dragon Raid Boss
+ 500 rarity crystals
= Longsword of the Dragon
```

## Visible equipment

Equipped gear should visibly affect the character wherever practical.

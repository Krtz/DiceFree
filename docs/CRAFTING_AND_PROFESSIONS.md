# Crafting, Professions and Resources

## Gear crafting

Primary equipment crafting is handled by **NPC crafters**.

The player does not choose a blacksmith/armorsmith profession in order to create progression gear.

NPC gear crafting is intended to create deterministic or semi-authored upgrade paths that combine meaningful pieces of content.

Example:

```
Longsword from Dungeon 4
+ Heart of the Dragon raid boss
+ 500 rarity crystals
= Longsword of the Dragon
```

Recipes may consume:
- a specific piece of equipment;
- boss trophies;
- dungeon/raid materials;
- rarity currencies;
- quest resources;
- gold;
- other special ingredients.

This lets crafting extend dungeon progression instead of competing with it.

## Materials as resources

Most common crafting materials should behave more like **currencies/resources** than physical inventory objects.

Goals:
- reduce inventory clutter;
- make long-term accumulation readable;
- support account-level storage where appropriate;
- let recipes request large quantities without forcing stacks of junk into bags.

Rare unique ingredients can still exist as named items when the identity matters.

## Player professions

Player professions focus on **consumables and supporting resources**, not core equipment progression.

Possible profession areas:
- potions/alchemy;
- cooking/food;
- buff consumables;
- gathering;
- resource refinement;
- utility items.

Exact profession count, leveling and specialization are not yet decided.

## Town dependency

Major NPC crafting happens in towns or appropriate special crafting locations.

This reinforces towns as service hubs while still allowing the player to send found equipment/material value toward account storage during adventures.

## Design principle

Dungeon/raid loot should remain exciting.

A crafted weapon should usually require meaningful content rewards rather than allow the player to bypass that content through repetitive low-risk gathering.

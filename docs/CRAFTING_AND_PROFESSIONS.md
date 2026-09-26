# Crafting, Professions and Resources

## Gear crafting

Primary progression-equipment crafting is handled by **NPC crafters**.

Recipes can combine:
- existing gear;
- boss trophies;
- raid/dungeon materials;
- currencies/resources;
- quest items;
- gold.

Example:

```
Longsword from Dungeon 4
+ Heart of Dragon Raid Boss
+ 500 rarity crystals
= Longsword of the Dragon
```

## Recipe discovery

Recipes use multiple discovery methods.

Some are:
- visible from the beginning;
- unlocked by quests;
- secret;
- discovered by bringing/obtaining relevant ingredients;
- unlocked through exploration/achievements;
- Echo-wide discoveries.

Recipe visibility is intentionally content-specific.

## Materials

Most common materials should behave like resource/currency counters rather than physical inventory stacks.

Some may be safe on death; others may use carried/stored risk rules. This is decided when the resource is created.

## Player professions

Professions focus on supporting systems rather than core gear crafting.

Examples:
- potions;
- food;
- temporary buffs;
- gathering;
- resource refinement;
- utility consumables.

Profession progression/knowledge is **Echo-wide**.

## Consumables

Consumables have cooldowns.

Different consumables may:
- use different cooldown durations;
- have shared cooldown groups;
- be modified by specific classes/passives/equipment;
- gain profession-related bonuses.

Exact categories and cooldowns are designed with the consumables themselves.

## Future currencies

DiceFree will eventually support more currencies/resources than gold.

Do not design the endgame currency ecosystem prematurely, but technical systems should support multiple currency types, carried/stored states, and individual death-loss rules.

# Loot and Equipment

## Principle

Loot should be meaningful rather than constant.

Major equipment progression comes primarily from successful dungeon/raid completion, supplemented by rare overworld drops, quests and NPC crafting.

## Dungeon and raid rewards

Successful completion sends each player to a private loot room.

Each dungeon/mode defines:
- its own loot tables;
- its own number of offered choices;
- its own rare jackpot rules;
- its own fallback rewards.

Players can inspect all offers before choosing one.

The chosen item can be kept/equipped or sent directly to the shared bank.

Unchosen rewards vanish.

If the gear choices are undesirable, content may instead offer XP, gold or materials/resources.

## Equipment only on completion

Dungeon bosses can award:
- XP;
- gold;
- materials/resources;
- quest/progression credit.

They do not award equipment before the run is successfully completed.

## Overworld loot

Rare overworld equipment drops are **immediately free-for-all**.

There is no temporary ownership/reservation window.

## Randomized and handcrafted gear

DiceFree uses both randomized and handcrafted gear, but **handcrafted gear should be the majority of meaningful equipment progression**, especially later/endgame.

This is intentionally closer to the spirit of Twilight's Eve ORPG:
- players should remember where important items come from;
- dungeons/bosses/quests can have authored identities;
- named gear can be designed around specific class/build opportunities;
- procedural loot should not drown the game in disposable stat soup.

### Randomized gear

Randomized gear remains a real supported system inspired by DiceBound.

It should be **deliberate about where it can drop and what pools are allowed**.

Content can define:
- which encounters/areas/modes may generate randomized items;
- allowed slots/families;
- item-level range;
- rarity range;
- affix/intrinsic pools;
- element/theme/class-tag filters;
- jackpot/special rules.

Randomized drops are therefore authored loot-table decisions, not a universal "every monster sprays random gear" rule.

### Randomization as an authoring tool

The same generator should be usable by developers/designers to help create handcrafted equipment.

Example workflow:
1. request a level-35 Epic weapon constrained to an intended content/theme;
2. generator produces a valid item using the real budget/affix system;
3. designer rejects/rerolls weak or uninteresting results;
4. designer accepts a promising roll;
5. accepted roll is **frozen into a normal authored item definition**;
6. designer can then edit name, art, intrinsic, stats, effects, requirements and flavor deliberately.

The final handcrafted item does **not** need to remain procedurally rerolled at runtime.

The randomizer is therefore both:
- a controlled player-facing loot mechanic;
- an internal content-authoring accelerator.

## Item level, rarity and stat budget

DiceFree uses a **gear-score/stat-budget model inspired by DiceBound**.

Core direction:
- item level contributes a base point/stat budget;
- rarity modifies/increases the available budget and may unlock additional structural features;
- Intrinsics, primary/secondary stats, affixes and other numeric bonuses consume budget according to weighted costs;
- different stats can have different point costs;
- item family/slot can influence valid budget distribution and intrinsic expectations.

The exact formulas are balance data and remain open.

A generated item should be reproducible/explainable from:
- item level;
- rarity;
- base family/slot;
- intrinsic package;
- affix/effect choices;
- budget spent.

Handcrafted items may deliberately bend or override normal generation rules when the design calls for it.

### Rarity is structural, not only numerical

Higher rarity can do more than provide larger numbers or additional affixes.

Rarity may unlock things such as:
- more/larger affix budget;
- additional Intrinsics;
- special effect slots;
- sockets;
- unusual affix categories;
- bespoke proc/effect permissions;
- other rarity-specific structure.

Exact rarity rules remain design/balance data.

## Equipment families

There is no armor-weight defense taxonomy.

Families can strongly differentiate stat identity through Intrinsics and affixes, including:
- Physical Defense;
- Magical Defense;
- Vitality/Strength/Agility/Intelligence/Spirit;
- elemental resistance;
- Attack Speed;
- Movement Speed;
- resource stats;
- healing/support stats;
- elemental penetration;
- other derived stats.

## Affix framework

Affix pools are data-driven and filterable by semantic metadata such as:
- item slot;
- weapon/equipment family;
- item level/tier;
- rarity;
- element/theme;
- class/class-family tags;
- intended role;
- content/drop-source tags;
- other authored constraints.

Affixes have:
- stable IDs;
- valid-item predicates;
- point/budget cost;
- roll range/scaling rules;
- optional weighting/rarity within a pool.

### Affix weighting

Support weighted affix pools.

Exact weighting philosophy remains open; not every pool needs unequal weights.

### Roll ranges

Use a **hybrid** model:
- formula/data-driven baseline scaling from item level/tier;
- authored overrides/ranges when a particular item/content bracket needs deliberate tuning.

## Intrinsics

Intrinsics are more important numerically than in DiceBound.

They can provide significant fixed stat packages that strongly establish what an equipment family/item is "for" before randomized affixes are applied.

## Handcrafted items

Handcrafted equipment uses the same underlying item/effect/stat systems but is authored directly.

A handcrafted item may lock:
- stable item ID;
- name;
- visual/base art;
- slot/family;
- item level;
- rarity;
- Intrinsics;
- exact stat rolls;
- affixes;
- proc/effect definitions;
- eligibility requirements;
- lore/flavor;
- drop/source.

It is **not** merely a generated item with a special label.

Handcrafted gear can intentionally exceed or bend ordinary randomized-generation rules when appropriate, but such exceptions should be explicit in data.

## Sockets, socketables and runeword-like systems

The equipment framework should support sockets even if socketable loot is not part of the earliest PoC.

Sockets can accept authored modifier items such as gems/runes/other socketables.

The framework should also leave room for a **runeword-like system**:
- specific socketable combinations/sequences can produce additional authored effects or transform item behavior;
- eligibility can depend on item family, socket count/order, tier or other tags;
- the resulting effect should use the normal item/effect/proc framework rather than special hard-coded weapon logic.

Exact runeword rules, order sensitivity, permanence/removal and crafting UX remain open.

## Item sets

Support authored item-set identity and threshold bonuses.

Examples:
- 2-piece;
- 3-piece;
- 4-piece;
- other custom thresholds.

Set bonuses use the normal ability/effect/proc/stat framework.

Sets may be rare; support does not imply every content tier needs them.

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

## Inventory and bank

Because items can be remotely sent to the bank, active carrying capacity does **not** need to be huge.

Current direction:
- relatively limited carried inventory;
- generous account bank;
- items can be sent/deposited to bank from anywhere, including inside dungeons;
- items can only be withdrawn from the bank while in town;
- **gold/currency cannot be remotely deposited**;
- carried gold must be manually deposited in town;
- town bank UI should include a convenient **Deposit All** action.

This keeps town banking relevant without forcing players to throw away valuable items during long adventures.

## Currency loss

Carried gold lost on death simply disappears.

It does not create a recoverable corpse pile.

Banked gold is safe.

## Durability

There is no durability system.

## Identification

Equipment drops **identified**.

There is no default unidentified-item / identify-scroll loop.

The game may introduce a special identification-like mechanic for a specific class/item/event later, but it is not part of normal loot friction.

## Equipment swapping

Equipment swapping rules depend on content context.

### Overworld
Gear may be swapped freely, **including during combat**.

This deliberately allows flexible overworld experimentation and does not try to police every optimization.

### Dungeons / raids
Equipment swapping is **disabled for the duration of an active dungeon/raid run**.

Players choose/prep their equipment before the run begins.

This avoids encounter-by-encounter resistance/stat wardrobe swapping inside instanced progression content.

Exact UI messaging and whether any exceptional dungeon mechanic can override this remain open.

## Binding

All items are account-bound.

## NPC crafting

NPC crafters combine authored gear and resources into new authored equipment.

## Visible equipment

Equipped gear should visibly affect the character wherever practical.


## Equipment eligibility and tags

Equipment eligibility uses semantic data requirements rather than maintaining giant per-item allowlists of every future class.

Classes can expose capability/identity tags such as:
- allowed weapon families;
- shield use;
- caster weapon access;
- melee/ranged capability;
- armor/equipment families;
- tier/advancement metadata;
- specific semantic class tags.

Items can require:
- one or more capability/taxonomy tags;
- specific weapon/equipment families;
- minimum tier/level where appropriate;
- an exact class ID when a deliberately narrow class-specific item is desired;
- **minimum primary attributes/stat values** such as Strength;
- combinations of semantic requirements.

Attribute/stat requirements are supported but need not be the dominant equipment-gating model.

Specific classes remain valid eligibility predicates. Scalability does **not** remove the ability to make niche class-only gear.

Requirements should use stable IDs/tags, not display-name parsing.


## Item effects and procs

Item effects use the same generic ability/effect/trigger/proc architecture as classes where practical.

Equipment can therefore react to semantic events such as:
- basic attacks;
- hits/crits;
- damage dealt/received;
- healing;
- resource changes;
- kills;
- effect application;
- thorns/reflection;
- other extensible triggers.

Do not build a parallel one-off "item proc engine."

## Generated-item provenance

For generated items, retain enough structured provenance/debug data to understand how the item was produced:
- generator ruleset/version where useful;
- item level;
- rarity;
- selected pools;
- budget available/spent;
- chosen affixes/intrinsics/effects.

A handcrafted item created from a generated candidate becomes an authored item with its own stable ID. Keeping the original generation seed/provenance for designer/debugging purposes is useful but should not constrain later manual edits.

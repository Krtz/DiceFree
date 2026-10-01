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

The reward choice must be resolved before leaving the loot room.

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

### Item level is source-defined

Item level comes from the authored content/source, not from the current player's level.

Examples:
- a level-35 dungeon drops roughly level-35 equipment even if a level-100 character clears it;
- old content keeps its own progression identity;
- player level does not silently upscale legacy loot.

Specific content may deliberately define exceptions, but player scaling is not the default.

### Budget is a design/debug metric, not a player truth

The point budget exists to help designers, tooling and balancing.

It is **not** a player-facing item-quality score and should not tell the player which item is "better."

A high-budget item can still be niche, awkward or intentionally strange.

Example:
- +50 HP regeneration and 0 damage may consume more budget than +20 damage and +20 HP regeneration;
- the latter may be more generally useful;
- the former may enable a weird sustain build.

Players should evaluate actual stats/effects and build fit.

Budget totals, over-budget deltas and internal power diagnostics belong in **debug/designer views only**.

Handcrafted content may intentionally exceed ordinary budget expectations. The tooling should report the deviation, not forbid it.

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

Designer/debug tooling should expose normal expected budget vs actual authored budget/delta so deliberate outliers are visible during development without exposing that number to players.

## Sockets, socketables and runeword-like systems

The equipment framework should support sockets even if socketable loot is not part of the earliest PoC.

Sockets can accept authored modifier items such as gems/runes/other socketables.

The framework should also support a **runeword-like system**:
- specific socketable **ordered sequences** can produce authored additional effects;
- **order matters**;
- eligibility can depend on item family, socket count, tier or other tags;
- runewords can activate on already-magical/rare/epic/etc. equipment — they do **not** require a plain/non-magical base item;
- the base item remains itself and keeps its existing stats/affixes/effects;
- the runeword adds to that item rather than transforming it into an unrelated replacement item;
- the resulting effect uses the normal item/effect/proc framework rather than special hard-coded weapon logic.

This means finding an excellent rare/epic item with a useful socket layout can be especially exciting because it can also host a runeword.

### Socket removal / recovery services

Socket changes are performed through authored NPC/services and have real cost/consequences.

Framework direction supports at least two destructive service patterns:
- preserve the **item**, remove/reset its socketables, and destroy the removed socketables permanently;
- preserve/recover the **socketables**, but destroy the base item permanently.

Exact NPCs, currencies/material costs and which services are available where remain content design.

## Duplicate named items and unique-equipped rules

Players may obtain multiple copies of the same named handcrafted item.

There is no universal "you may only own one" rule.

Because most equipment slots are singular, duplicate-equipping is naturally impossible for many item types.

The framework should still support an explicit **Unique Equipped** limit for edge cases such as:
- future multiple-ring-slot designs;
- dual-wielding identical one-handed named weapons;
- other items that can occupy multiple simultaneous eligible slots.

This is an item-specific restriction, not a default rule.

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

The lock begins when the dungeon's 60-second staging countdown finishes and the active run starts.

Players may still change equipment during the staging/waiting room.

Consumables remain usable/manageable during the run; the lock applies to equipped gear, not ordinary inventory consumption.

This avoids encounter-by-encounter resistance/stat wardrobe swapping inside instanced progression content.

Exact UI messaging and whether any exceptional dungeon mechanic can override this remain open.

## Binding

All items are account-bound.

## Player-to-player transfer policy

There is **no general player-to-player gear trading**.

Rules/direction:
- equipment already picked up into a player's inventory is normally that player's item;
- private dungeon rewards are personal;
- free-for-all overworld drops can still be socially assigned before pickup ("you take it").

The item framework should support **category-specific trade permission** for selected non-equipment items.

Candidate intentionally tradeable categories include:
- food;
- potions;
- similar ordinary consumables.

Exact tradeable consumable/material categories remain open.

There is no default direct gold-transfer or unrestricted material economy between players.

## Early Cornberg economy anchors

The overall DiceFree economy is not finalized, but Cornberg has working early-game anchors for prototyping.

### Crop Slime gold

The weakest Crop Slimes should yield roughly **1–3 carried gold** each.

This is a working early-economy target and can be tuned after playtesting.

### Cornberg general vendor

Vendor inventories are **fixed/authored**, not procedurally regenerated.

Current example starter weapon price ladder:
- +1 damage sword: **100 gold**;
- +2 damage sword: **250 gold**;
- +3 damage sword: **450 gold**.

Cornberg can also sell basic low-stat defensive equipment, for example an item with approximately **+1 Defense**. Exact defensive item/family/price remains to be authored.

These values are economy anchors, not a final global pricing formula.

## Selling unwanted equipment

There is **no general equipment salvage/disenchant system**.

Unwanted equipment is primarily sold to vendors for gold.

If crafting/material acquisition needs another sink/source later, materials can be sold by NPCs for gold or introduced through authored content rather than requiring every unwanted item to become crafting dust.

Named/handcrafted equipment does not automatically dismantle into special content-specific materials.

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

## Early authored quest reward — Cornberg Q4

Cornberg Q4 currently guarantees an authored **Hands/gloves** reward with **+5% Attack Speed** alongside XP and gold.

Rules:
- this is a fixed authored quest reward, not a randomized item roll;
- both Q4 completion paths grant the same glove reward;
- the reward can only be granted once per eligible Q4 completion;
- exact item name, item level, rarity, art and flavor remain open;
- the gloves grant +5% Attack Speed only, with no other gameplay stats.

This is a useful early proof that quest rewards can award memorable handcrafted equipment rather than only currency/XP.

## Early elite drop — deep-forest Slime shoes

The dangerous deep-forest Slime used by Cornberg Q4 has a unique repeatable/farmable shoe drop.

Current stats:
- **+1 Physical Defense**;
- **+1 Magical Defense**;
- **+1% Movement Speed**.

Drop chance:
- **20% per elite kill**.

Exact shoe name, item level, rarity, art and flavor remain open.

This item is separate from Q4's guaranteed +5% Attack Speed glove reward.



## Equipment slot occupancy

DiceFree keeps the same semantic equipment slot set across classes/forms:

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

A class/form does not replace the slot model merely because its body plan is unusual.

Items may occupy **multiple slots**.

Canonical example:
- a two-handed weapon occupies **Main Hand + Off Hand** while equipped.

Implementation direction:
- item data declares its primary/equip slot and all occupied slots;
- equip validation must ensure every required slot is available;
- equipping a multi-slot item unequips/conflicts with items occupying any required slot according to normal equip rules;
- UI derives occupancy from item data rather than special-casing weapon names;
- future unusual equipment may use the same occupancy mechanism if needed.

When equipping an item that occupies slots currently used by other equipment:
- automatically unequip the conflicting item(s) back into carried inventory **if inventory has room**;
- perform the change atomically;
- if all displaced items cannot be retained safely in inventory, reject the equip;
- never destroy/drop/overwrite displaced gear as a side effect of equipping.

Example:
- player has a one-handed sword in Main Hand and shield in Off Hand;
- equipping a two-handed sword automatically returns both conflicting items as needed to inventory and occupies Main Hand + Off Hand, provided inventory capacity permits.

## Equipment appearance compatibility

Equipment appearance is authored, not automatically morphed to every class body.

The same visual mesh should keep its designed proportions.

Compatibility/restriction data decides which Ways/forms can equip/use that item or appearance.

Default lineage rule:
- equipment authored/allowed for a class/form is usable by that class **and its descendants**;
- descendants inherit the parent's equipment-family permissions by default;
- descendants may add permissions or explicitly opt out/override inherited permissions when a later transformation/class identity requires it;
- unrelated lineages do not inherit compatibility automatically.

Examples:
- a Ranger/Wood-Elf hat can be sized for that lineage and descendants;
- a future Centaur hat can have different authored proportions;
- these do not need to be one universal hat mesh dynamically resized across both forms.

Prefer class/form-family compatibility tags and explicit exceptions over runtime mesh deformation machinery.

## Wearable visual replacement

For visible armor slots, the equipped item's authored appearance normally **replaces** the class form's baseline visual for that slot rather than being universally layered on top.

Layering convention:
- Chest replaces baseline torso/chest;
- Head, Hands, Legs and Feet replace their corresponding baseline presentation;
- Shoulders layer independently over the current Chest;
- Back layers independently over the current torso/Chest;
- weapons/offhands remain independent equipped presentation.

The class/form provides the underlying body and default/baseline presentation for empty slots.

A baseline slot can either have authored default clothing/armor or intentionally expose the underlying body. Both are supported per class/form and per slot.

This remains a presentation rule; gameplay ownership/stats still come from the equipped item model.


## Advancement equipment reconciliation

A new Way may lose compatibility with gear inherited in the parent's snapshot.

On advancement:
- compatible gear may remain equipped;
- incompatible/conflicting equipped gear is automatically unequipped into the child manifestation's carried inventory when capacity permits;
- if carried inventory lacks capacity, the system may automatically deposit the displaced account-bound gear into the Echo-wide shared bank;
- if neither inventory nor bank can safely retain all displaced items, advancement fails atomically before ownership/equipment mutation.

Never destroy, drop into the world, or silently discard gear because a transformation changed equipment compatibility.

The preserved parent manifestation keeps its original equipment and ownership state unchanged.

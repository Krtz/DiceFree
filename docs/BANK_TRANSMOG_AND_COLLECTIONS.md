# Bank, Transmog and Shared Collections

## Shared bank

The bank is **Echo-wide only**. There is no separate manifestation-specific stash.

The player begins with a relatively modest bank and can buy many additional slots/pages using gold.

Expansion pricing should be **progressive** and is expected to need substantial balance tuning. The system should support a very large eventual capacity and act as a meaningful long-term gold sink.

Items can still be remotely sent/deposited to the shared bank from the world under the existing inventory rules. Withdrawal remains a town/bank service.

## Bank organization

Use one large expandable storage rather than many mandatory category tabs.

Required tools:
- text search;
- auto-sort;
- search by semantic categories/types;
- advanced filter syntax;
- optional regular-expression mode.

Example friendly searches:
- `weapons`
- `swords`
- `potions`
- `rare`
- `legendary`
- `legendary swords`

Advanced search should support structured filters such as:
- `slot:weapon`
- `type:sword`
- `rarity:legendary`
- `class:...`
- other semantic item metadata.

Regex is enabled with a **visible Regex toggle next to the search bar** rather than implicit regex parsing.

Search/filter logic must operate on stable item metadata/tags where possible rather than localized display text alone.

## Class equipment permissions

Classes have explicit authored permissions for:
- weapon types;
- armor/equipment types.

Many individual items may additionally be restricted to:
- a specific class;
- a class family/type/tag;
- another explicit eligibility predicate.

There is no smart-loot weighting toward the player's current class or party composition.

A player may receive/loot gear the current manifestation cannot equip and can:
- send it to the shared bank;
- transfer it to another manifestation through normal bank use;
- sell it;
- sacrifice it for transmog.

## Transmog collection

Transmog appearance unlocks are **Echo-wide**.

To permanently add an appearance to the collection, the player **sacrifices/destroys an item**.

The game must warn before sacrificing important items, especially:
- the last owned copy of a unique/handcrafted item;
- otherwise difficult-to-replace authored items.

Applying a transmog is performed through an authored **NPC/service**, not freely from anywhere by default.

### Eligibility

An appearance can normally only be applied if the current class can use/equip the corresponding gear type.

Example:
- a class that cannot use swords cannot normally transmog its weapon into a sword.

The framework should support explicit authored exceptions.

### Dyes

DiceFree does **not** use a general dye system.

## Consumables

Consumables share the normal carried inventory/bag with gear.

Consumable cooldown behavior is **case-by-case**, but the framework should support authored cooldown families/categories so multiple items can share one cooldown.

General intent: combat consumables are mostly **emergency buttons**, not something the expected combat loop requires players to chain constantly.

## Dungeon reward choice

On successful dungeon completion, each eligible player gets an **independent private reward roll** with zero competition between party members.

General direction:
- roll one or two item offers;
- show full item details before choosing;
- player chooses one item **or** an alternate reward;
- alternate rewards are rolled/contextual based on the dungeon and may include XP, gold, materials or other authored rewards;
- no unidentified-item mechanic;
- unchosen offers disappear.

All dungeon equipment loot is awarded through the final private loot room rather than dropping directly from bosses during the run.

Dungeons may be farmed indefinitely at full eligibility unless a specific dungeon explicitly defines a different rule.

Dungeon-specific materials may be used for authored crafting recipes, named equipment, cosmetics, vendor purchases or similar content.

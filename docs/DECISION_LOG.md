# Decision Log

This file records decisions that are sufficiently settled to design around. It is not a dumping ground for every brainstorm.

## 2026-09-25 — Project name

**Decision:** The game is called **DiceFree**.

## 2026-09-25 — Relationship to DiceBound

**Decision:** DiceBound takes place inside the Dice. DiceFree begins after defeating The Last Equation and reaching the outside of the Dice.

## 2026-09-25 — World structure

**Decision:** DiceFree has six major regions/faces, thematically connected to DiceBound's six boards.

## 2026-09-25 — Visible equipment

**Decision:** Equipped gear should visibly change the player character. The character/armor pipeline must therefore be modular from the beginning.

## 2026-09-25 — Development priority

**Decision:** DiceBound remains the active implementation priority. DiceFree may accumulate design, lore and backlog work now, but active game development begins after DiceBound is sufficiently polished.

## 2026-09-25 — Source control

**Decision:** DiceFree uses GitHub from the design phase onward.

## 2026-09-25 — Engine direction

**Decision:** Unity is the preferred engine direction. The actual initialized project uses Unity 6000.6.3f1 / URP on the setup branch.

## 2026-09-26 — Player movement control styles

**Decision:** DiceFree supports both **mouse-driven click-to-move** and **WASD/direct movement**.

**Primary balance target:** Classic mouse movement is the default/reference control scheme. Movement speed, encounter spacing, telegraphs, kiting, attack ranges and other movement-sensitive mechanics should be designed and tested primarily around click-to-move so direct movement does not become implicitly required.

**Architecture rule:** Both control styles should feed shared movement/combat systems rather than become separate character-controller implementations.

**Open:** A simultaneous Hybrid mode may be added later, but is not yet required.

## 2026-09-26 — Advancement cadence and permanence

**Decision:** Advancement thresholds are currently **10 -> 30 -> 60 -> 120 -> 200**. When a character advances, their current class level resets.

**Decision:** Advancement choices are permanent for that character.

**Decision:** Abilities can be added, evolved or replaced on advancement depending on the class; there is no universal carry-forward rule.

**Decision:** All normal and secret classes use the same advancement-level cadence.

## 2026-09-26 — Class-tree shape

**Decision:** The class tree should be kept reasonably even where practical, but it is **not strictly binary**.

**Decision:** Secret classes can branch from different points in the normal tree.

**Decision:** A class does not need a completely new name at every advancement; some lineages may preserve an identity and become an advanced version of that identity.

**Working direction, not final:** The first split is currently envisioned as **Physical vs Magic**, with possible later melee/ranged distinctions.

## 2026-09-26 — Character slots and bank

**Decision:** Players will have multiple character slots to explore permanent class branches.

**Decision:** The bank is shared account-wide.

## 2026-09-26 — Death and dungeon failure

**Decision:** On ordinary death, another player may resurrect the character if their class allows it; some classes may have self-revive mechanics. Otherwise the player respawns at a camp/checkpoint.

**Decision:** Death causes a modest gold loss and a very small experience loss.

**Decision:** A full dungeon/raid party wipe resets the run and forfeits completion loot.

## 2026-09-26 — Mobility and ability-count philosophy

**Decision:** There is no universal dodge/roll.

**Decision:** Some classes may have mobility abilities, and classes can have different base movement speeds. Mobility should involve class-specific tradeoffs.

**Decision:** Ability bars should stay relatively compact, but classes are not required to have the same active/passive count.

## 2026-09-26 — Character visual identity

**Decision:** The class itself is the primary player-character identity and visual form. Equipped gear further individualizes it.

**Decision:** A broad traditional character creator is not planned.

**Open:** A minimal boy/girl/undefined presentation option may be explored only if it does not create unreasonable art/rigging costs.

## 2026-09-26 — Echo identity

**Decision:** The DiceFree player is the survivor of the events inside DiceBound, existing outside as an **Echo**.

**Decision:** As in DiceBound, the Echo gains a shape/identity through class choice.

## 2026-09-26 — World routing and cube traversal

**Decision:** The six faces have an intended progression order through story, scaling and advancement placement, but players may choose other routes.

**Decision:** There are six primary overworld zones, with many towns, dungeons, raids and interiors attached to them.

**Decision:** Adjacent faces are physically connected. Players can walk over an edge and gravity changes to the new face; normal face-to-face traversal does not use a gateway/portal.

## 2026-09-26 — Dungeon construction and loot

**Decision:** Most dungeons are handcrafted, while some dungeon types may be procedural/RNG-generated.

**Decision:** Loot should be relatively uncommon/meaningful and driven by explicit drop tables.

**Decision:** Dungeon/raid completion is intended to be a major source of equipment progression.

**Decision:** Harder dungeon modes should add mechanics as well as stronger stats.

## 2026-09-26 — Equipment and stats

**Decision:** Gear can have class/type restrictions, level requirements, stat requirements and other conditions.

**Decision:** Classes have different automatic stat gains on level-up.

**Decision:** Equipment also provides stats.

**Decision:** Players do not manually allocate stat points.

## 2026-09-26 — Questing

**Decision:** DiceFree includes regular quests and dedicated advancement quests.

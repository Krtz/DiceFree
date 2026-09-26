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


## 2026-09-26 — Advancement stat model

**Decision:** Each class/advancement has its own level-1 base stats and its own stat growth. Later advancement tiers generally start from higher base stats.

**Decision:** When a class advances, the new class begins at level 1 using that class's own stat package. Stats are not simply accumulated forever from prior classes.

**Decision:** A character may continue leveling past the advancement threshold, but excess experience does **not** carry into the new class after advancement.

## 2026-09-26 — Class resources

**Decision:** Different classes can use different resources.

**Decision:** Later advancements may keep, modify or completely replace the previous class resource.

## 2026-09-26 — Equipment slots and binding

**Decision:** Planned equipment slots are Head, Shoulders, Chest, Hands, Legs, Feet, Main Hand, Off Hand, Ring, Amulet and Back.

**Decision:** All items are account-bound.

## 2026-09-26 — Dungeon reward rooms

**Decision:** On successful dungeon/raid completion, each player is sent to a private loot room and chooses one reward from a generated selection tied to that content's drop table.

**Decision:** Overworld enemy drops are free-for-all and comparatively rare.

## 2026-09-26 — Multiplayer and scaling

**Decision:** Initial multiplayer target is up to 4 players.

**Decision:** Systems should support party-size encounter scaling, but exact scaling formulas are deferred to playtesting/balance work.

## 2026-09-26 — Wipe reset baseline

**Decision:** The default rule for dungeons and raids is a full reset on party wipe.

**Decision:** Exceptional content may deliberately use different reset rules later.

## 2026-09-26 — Resurrection

**Decision:** Healer-type classes may resurrect repeatedly.

**Decision:** Resurrection is balanced through a long cast, high mana/resource cost and a stacking Resurrection Sickness debuff on revived characters.

## 2026-09-26 — Quest structure

**Decision:** Main quests are relatively lightweight and primarily guide players through the world.

**Decision:** Side quests support leveling, gear, resources and worldbuilding.

**Decision:** Advancement quests are designed class-by-class and may deliberately send a class into much higher-level regions if that class has tools to survive the task.

## 2026-09-26 — Town services

**Decision:** Towns may provide vendors, crafting, resurrection points, healers, NPC/quest services, class advancement and shared bank access.

## 2026-09-26 — Enemy aggro and leashing

**Decision:** Overworld enemies use aggro ranges and pursuit/leash limits.

**Decision:** Enemies may follow across cube edges while inside their pursuit rules, but eventually give up and return home rather than chase indefinitely across the world.

## 2026-09-26 — Fixed zone difficulty

**Decision:** Enemy difficulty/levels are fixed by zone. Enemies do not scale to the player.

**Decision:** Players may enter higher-level regions early and face enemies far beyond their current power.

## 2026-09-26 — Raids

**Decision:** Raids are essentially larger/more ambitious handcrafted dungeons.

**Decision:** There are no raid lockout timers.

## 2026-09-26 — Echo manifestations and persistence

**Decision:** All character slots are alternate manifestations/timelines of the same underlying Echo.

**Decision:** Some quests/events are account-level and persist across manifestations; others are intentionally replayed on each character slot.

## 2026-09-26 — Edge-transition camera direction

**Working decision:** The camera should ultimately align to the local gravity of the current face and smoothly blend through the 90-degree orientation change when crossing an edge. It should preserve stable player framing/isometric readability and avoid gratuitous spinning. This must be validated by prototype before being treated as final.


## 2026-09-26 — Overleveling and secret-class hooks

**Decision:** Reaching an advancement threshold does not stop further leveling.

**Decision:** Overleveling has no inherent long-term bonus beyond being stronger for the current class/advancement quest.

**Decision:** Extreme overleveling is allowed. A level-200 Novice should be possible and is an explicit example of a future secret-class unlock condition.

## 2026-09-26 — First advancement identities

**Decision:** The two current first-advancement identities are **Physically Blessed Novice** and **Magically Touched Novice**.

**Open:** Their following melee/ranged branches are not yet finalized.

## 2026-09-26 — Completion-only equipment rewards

**Decision:** Dungeon/raid bosses may grant experience, gold, materials/resources and progression credit during the run, but equipment loot is awarded only after successful completion through the private loot room.

**Decision:** Loot-room offer counts may vary by content/difficulty, and offers may include items useful to other manifestations/classes.

## 2026-09-26 — Inventory, bank and materials

**Decision:** Inventory/bank friction should be low.

**Decision:** Items can be sent/deposited to the shared bank from anywhere, but can only be withdrawn while in town.

**Decision:** The bank should be generous.

**Decision:** Common crafting materials should generally behave as currencies/resources rather than physical inventory clutter.

## 2026-09-26 — Crafting and professions

**Decision:** Progression gear crafting is handled by NPC crafters using recipes that may consume existing dungeon/raid items, boss materials, currencies and gold.

**Decision:** Player professions focus on potions, food, consumables, gathering/resources and similar support systems rather than main equipment crafting.

## 2026-09-26 — Durability

**Decision:** DiceFree has **no equipment durability system**.

## 2026-09-26 — Overworld respawns

**Decision:** Normal overworld enemies respawn on timers.

**Decision:** Named/elites use longer respawn timers.

## 2026-09-26 — Multiplayer lobby/session model

**Decision:** DiceFree is lobby/session-based co-op, initially up to four players, rather than an MMO/open shared world.

**Decision:** Steam invites/lobbies are a target.

**Decision:** Players may join a started overworld session and spawn at their manifestation's last-set town/resurrection point.

**Decision:** Players cannot join a dungeon/raid that has already started.

**Decision:** Quest credit is granted only when the joining/participating manifestation is eligible.

## 2026-09-26 — Towns, camps and travel

**Decision:** Towns provide the full service set; camps provide only a subset.

**Decision:** Fast travel is travel-point to travel-point, not teleport-from-anywhere.

**Decision:** Most travel points are connected, but not all; disconnected routes should have in-world/lore justification.

## 2026-09-26 — d6 regional map identity

**Decision:** The one-dot face is the starting region, then two-dot through six-dot in intended progression order.

**Decision:** Each region should use distinct landmarks arranged so its map/minimap evokes the corresponding d6 pip layout.

**Decision:** High-level/endgame/secret content can be hidden in any region, including the starting face.

## 2026-09-26 — Cube traversal implementation remains open

**Decision:** The fantasy is still a six-faced physical Dice world, but fully seamless traversal along every edge is **not** technically mandatory.

**Decision:** Mountains, forests, cliffs, roads and other authored geography may restrict crossings to selected routes.

**Decision:** We will prototype seamless local-gravity cube-edge traversal versus segmented/streamed face spaces and choose the implementation that best preserves feel, readability and production feasibility.


## 2026-09-26 — Health and primary-stat direction

**Decision:** Vitality contributes to maximum HP using a class base plus Vitality-derived amount.

**Open:** Whether the Vitality-to-HP coefficient is universal or class-specific.

**Decision:** Every normal class has a primary stat used for its basic-attack scaling.

## 2026-09-26 — Defense and equipment families

**Decision:** DiceFree has one underlying Defense type rather than separate cloth/leather/mail/plate defense systems.

**Decision:** Classes can equip different equipment families, and those families differ through stats, Intrinsics, affix pools and class permissions.

**Decision:** Equipment Intrinsics can provide larger stat packages than in DiceBound.

## 2026-09-26 — Basic attacks

**Decision:** Every normal class has a Warcraft III / League-style basic attack: attacking a target moves into range if necessary and repeatedly attacks while the target remains valid.

**Decision:** Basic attacks may be melee or ranged depending on class.

**Open:** A future no-basic-attack class is allowed only as an intentional special case.

## 2026-09-26 — Loot-room inspection and fallback

**Decision:** Loot-room offers can be fully inspected before choosing.

**Decision:** Unchosen offers disappear.

**Decision:** The chosen reward may be sent directly to the shared bank.

**Decision:** If equipment offers are undesirable, content may offer a fallback reward such as XP, gold or materials.

**Decision:** There is no universal baseline number of loot choices; offer counts are content-specific.

**Decision:** Ultra-rare jackpot items, hard-mode exclusivity and drop odds are decided per item/dungeon.

## 2026-09-26 — Recipe discovery and professions

**Decision:** NPC crafting recipes can be initially visible, quest rewards, secret, discovered through ingredients/exploration, or Echo-wide discoveries.

**Decision:** Player profession progression/knowledge is Echo-wide.

## 2026-09-26 — Consumable cooldowns

**Decision:** Consumables have cooldowns, with category/item-specific rules.

**Decision:** Classes, passives, equipment and profession effects may modify consumable cooldowns/effectiveness.

## 2026-09-26 — Currency risk model

**Decision:** Carried gold is manifestation-specific and may lose a percentage on death.

**Decision:** Banked gold is Echo-wide and safe.

**Decision:** Future materials/currencies may individually be safe or losable and may use similar carried/stored rules.

**Decision:** Systems should support additional future currencies without defining the endgame currency ecosystem now.

## 2026-09-26 — Resurrection points and overworld death

**Decision:** Resurrection points are explicitly selected by interacting with a lore-appropriate world object/location; simply passing a camp does not overwrite the selection.

**Decision:** Overworld death does not reset the world. Allies can resurrect the player before they accept respawn.

## 2026-09-26 — Dungeon abandonment and entrances

**Decision:** A dungeon/raid run may be abandoned, but the abandoned run cannot be re-entered.

**Decision:** Dungeons have physical world entrances.

**Decision:** Entry requirements are content-specific and may include levels, quests, keys, items, achievements or other conditions.

## 2026-09-26 — Powerleveling and party XP

**Decision:** Powerleveling is allowed in principle but should not trivialize the entire progression ladder.

**Decision:** Party XP is not divided as a finite pool; participating/eligible players receive their own XP.

**Decision:** Character-vs-enemy level difference can reduce XP.

**Open:** Exact anti-trivialization/sync system, including possible FFXIV-style sync for selected content.

## 2026-09-26 — Threat and support targeting

**Decision:** DiceFree has a threat system so tank classes can intentionally control enemies.

**Decision:** Bosses may override ordinary threat with encounter mechanics.

**Decision:** Healing/support abilities can target both world characters and party frames.

## 2026-09-26 — Enemy level display

**Decision:** Enemy levels are normally visible.

**Open:** Enemies far beyond the character may display ??? instead of an exact level to preserve mystery.

## 2026-09-26 — World knowledge and inter-face society

**Decision:** NPCs do not normally understand that their world is a die; to them its geometry is simply normal.

**Decision:** NPCs can travel between faces and there can be trade, politics, migration, wars and alliances across face boundaries.

**Idea:** An NPC who claims the world is literally a die and is treated as crazy is explicitly welcome.

## 2026-09-26 — Endgame philosophy

**Direction:** Endgame follows an ARPG-style progression loop: acquire stronger/unique gear to access harder content, which rewards stronger/more build-enabling gear, while secret/unique items enable new builds.

**Note:** Detailed endgame structure is intentionally deferred.

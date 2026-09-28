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


## 2026-09-27 — Core attribute set

**Decision:** Core attributes are **Vitality, Strength, Agility, Intelligence and Spirit**.

**Decision:** Every attribute should provide a useful secondary effect.

**Decision:** Every class has one or more primary attributes used for basic-attack scaling.

**Decision:** Hybrid classes may use multiple primary attributes, and exceptional/secret classes may potentially use all attributes as primary.

**Open:** Exact secondary effect of Strength, Agility, Intelligence and Spirit.

## 2026-09-27 — Physical/Magical defense and elemental resistance

**Decision:** DiceFree uses separate **Physical Defense** and **Magical Defense** systems.

**Decision:** Both defenses use diminishing returns.

**Decision:** Elements are independent of damage channel. Any element can be Physical or Magical depending on the attack.

**Decision:** Elemental resistance applies as a separate multiplicative mitigation layer after/beside the broad Physical/Magical mitigation layer.

**Example:** 100 Physical Coffee damage against 50% Physical mitigation and 50% Coffee resistance deals 25 final damage.

**Open:** Exact diminishing-return formulas, resistance caps, penetration and negative-resistance rules.

## 2026-09-27 — Attack speed

**Decision:** Basic attacks have attack-speed values.

**Decision:** Classes can have different base attack speeds and attack animations.

## 2026-09-27 — Critical-hit system

**Open:** Critical hits are not yet tied to any attribute or made universal. Crit may ultimately be broadly available, class/passive-driven, item-driven, or use different rules for attacks/spells/healing/DoTs.

## 2026-09-27 — Fixed class kits

**Decision:** Classes use fixed ability kits rather than freely swapping from a large skill library.

**Decision:** Advancement and gear can evolve/modify that fixed kit.

## 2026-09-27 — Inventory and remote banking

**Decision:** Active carrying capacity can be relatively limited because items may be remotely sent to the shared bank.

**Decision:** Items may be banked remotely even inside dungeons.

**Decision:** Bank withdrawals remain town-only.

**Decision:** Gold/currency cannot be remotely deposited; carried gold is manually deposited in town.

**Decision:** Town banking should include a convenient Deposit All action.

## 2026-09-27 — Death currency loss

**Decision:** Carried gold lost on death disappears rather than creating a recoverable ground/corpse pile.

## 2026-09-27 — Resurrection Sickness direction

**Working decision:** Each Resurrection Sickness stack gives approximately a **10% reduction to all stats**.

**Open:** Exact stacking math, duration and maximum stack count.

## 2026-09-27 — Threat details and healer damage

**Decision:** Healing generates threat.

**Decision:** Combat actions can have individually defined threat values/coefficient rather than all damage/healing creating identical threat.

**Decision:** Healers are expected to contribute damage, closer to Final Fantasy XIV's healer philosophy than pure heal-only gameplay.

**Decision:** Tank tools may include both temporary forced-target taunts and large threat-building abilities.

## 2026-09-27 — Enemy level mystery

**Working direction:** Enemies far above the current character may show **???** instead of exact level. Around 20+ levels higher is a candidate threshold.

## 2026-09-27 — Old-content power

**Decision:** High-level characters can normally return to old dungeons and overpower them.

**Decision:** There is no universal automatic level sync for old content.

**Open:** Selected content may use sync where preserving intended challenge is useful; exact powerleveling controls remain unresolved.

## 2026-09-27 — Overworld loot ownership

**Decision:** Rare overworld equipment drops are immediately free-for-all with no temporary ownership/reservation window.

## 2026-09-27 — Alternate-Echo encounter idea

**Idea:** A future alternative-timeline dungeon where another manifestation of the Echo became evil is explicitly welcome, but this is late-game/story content and not a current implementation priority.


## 2026-09-27 — Multi-primary class scaling

**Decision:** Classes with multiple primary attributes define their attack-scaling weights **case by case**.

**Decision:** There is no universal hybrid-stat formula.

## 2026-09-27 — Separate damage instances

**Decision:** Mixed damage packets are represented as separate damage instances for calculation, resistance, triggers, combat logging and debugging.

**Decision:** If one attack deals both Physical and Magical damage, those are separate instances.

**Decision:** If one attack uses multiple elements, those elemental portions are also separate instances rather than one blended multi-element packet.

## 2026-09-27 — Defense sources

**Decision:** The meaningful bulk of Physical/Magical Defense comes from class base values, gear and abilities/passives.

**Decision:** Attributes may contribute to defense as secondary effects, but should not replace those systems.

## 2026-09-27 — Attribute secondary effects

**Decision:** Spirit's healing-given and healing-received scaling, if both are used, may use different coefficients.

**Decision:** Intelligence must retain useful secondary value even for non-Mana classes, but it does not universally govern resource regeneration.

## 2026-09-27 — Resurrection Sickness scope

**Decision:** Resurrection Sickness reduces the five primary attributes only: Vitality, Strength, Agility, Intelligence and Spirit.

**Working value:** roughly 10% per stack.

## 2026-09-27 — Crit philosophy

**Direction:** Crit is not assumed to be universally available to every class by default.

**Decision:** Crit may be granted/enabled through class mechanics, passives, abilities, gear or other systems, with different rules by effect type.

## 2026-09-27 — Healer combat identity

**Decision:** Healers have real damage rotations and are expected to contribute DPS in group play.

**Decision:** Healers must remain viable for solo questing, farming and grinding.

## 2026-09-27 — Veteran/new-player co-op goal

**Decision:** DiceFree should let veteran players meaningfully play with new players without making old content universally auto-scaled or making progression trivial.

**Open:** Exact solution. Candidate tools include optional/content-specific level sync, mentor-style normalization, level-gap XP curves and reward normalization.

**Decision:** High-level characters can still normally return to old content and overpower it when not using a special sync/mentor rule.


## 2026-09-27 — Core attribute secondary direction

**Decision:** Keep Vitality as its own attribute rather than folding HP into Strength.

**Decision:** Current secondary direction:
- Vitality -> HP and HP regeneration;
- Strength -> modest Physical Defense;
- Agility -> very small Attack Speed and Movement Speed gains;
- Intelligence -> modest Magical Defense, plus class-specific resource interactions where appropriate;
- Spirit -> healing done and healing received with separate scaling.

**Decision:** Large differences in defenses/speeds/resources come primarily from class base values, gear, abilities and passives rather than attribute secondaries.

## 2026-09-27 — Ability scaling and adaptive scaling

**Decision:** Abilities define their own stat coefficients.

**Decision:** Abilities may use weighted multi-stat scaling or adaptive rules such as highest of STR/AGI, highest attribute overall, or other class-specific formulas.

## 2026-09-27 — Tooltip detail

**Decision:** Default ability tooltips prioritize final readable values such as damage, resource cost and cooldown.

**Decision:** Holding a modifier key should reveal detailed formulas/scaling, League-style.

## 2026-09-27 — Elemental resistance baseline

**Decision:** Players and enemies can begin with negative elemental resistance.

**Direction:** Elemental resistances should likely be displayed as direct percentages with a normal cap, while special classes/items/effects may raise that cap.

**Open:** Exact starting negative resistance, cap, floor and whether direct percentages remain preferable to a rating/diminishing-return conversion.

## 2026-09-27 — Elemental healing

**Decision:** Healing has no Physical/Magical channel.

**Decision:** Healing may have an element.

**Decision:** Matching elemental resistance increases matching elemental healing, while negative resistance reduces it.

**Example:** +50% Fire resistance -> 1.5x Fire healing; -20% Fire resistance -> 0.8x Fire healing.

## 2026-09-27 — Resistance auras and vulnerabilities

**Decision:** The system must support positive/negative elemental resistance auras, temporary all-resistance buffs, enemy resistance debuffs and monster-specific elemental weaknesses/strengths.

## 2026-09-27 — Combat telemetry and floating numbers

**Decision:** Build exact combat-resolution telemetry/debug logging from the beginning, including raw values, mitigation, resistance, coefficients and final values.

**Open:** Whether detailed combat logs/meters are exposed to normal players.

**Decision:** Floating combat numbers are supported and player-toggleable.

**Direction:** Prepare the framework so rapid multi-instance attacks can later aggregate numbers if visual noise becomes a problem.

## 2026-09-27 — Threat UI

**Decision:** Threat UI is toggleable.

**Default:** simple color/state indicators.

**Optional:** exact threat values/rankings for players who enable detailed information.


## 2026-09-27 — Elemental resistance defaults

**Decision:** Default elemental resistance baseline is **-10%**.

**Decision:** Normal elemental resistance cap is **75%**.

**Decision:** Elemental resistance is displayed/handled as a direct percentage rather than a hidden rating conversion.

**Decision:** Exceptional classes/items/effects may raise the normal 75% cap.

## 2026-09-27 — Penetration models

**Decision:** Systems support both flat and percentage Physical/Magical Defense penetration.

**Decision:** Elemental penetration/reduction can likewise use different models where appropriate.

**Decision:** Which model a class/ability uses is case-by-case; later advancements may upgrade/change penetration style.

## 2026-09-27 — HP regeneration

**Decision:** Vitality provides **flat HP regeneration**, not percentage-max-HP regeneration by default.

## 2026-09-27 — Healing interaction rules

**Decision:** Spirit-based healing-done/healing-received effects apply to all healing unless an effect explicitly excludes them.

**Decision:** Whether a heal is elemental is ability-specific.

**Decision:** Non-elemental healing ignores elemental resistance.

**Decision:** Elemental healing uses matching elemental resistance unless explicitly overridden.

## 2026-09-27 — DoT mitigation

**Decision:** Every DoT tick is a separate damage instance and recalculates using the target's current defenses/resistances at the time of the tick.

## 2026-09-27 — Adaptive scaling visibility

**Decision:** Detailed/Shift tooltips for adaptive-scaling abilities show which attribute is currently selected and the active calculation.

## 2026-09-27 — Stat transparency

**Decision:** Character-sheet/stat hover details should expose the current mechanical contribution of attributes/derived stats even if the default UI remains concise.

## 2026-09-27 — Resistance-manipulation rotations

**Decision:** Resistance buffs/debuffs are intended to support rotational gameplay, not just passive gearing.

**Example design space:** an ability may raise an ally's Nature resistance by 30% or lower an enemy's Nature resistance by 30% for 10 seconds, allowing the same class mechanic to set up defense, elemental healing or elemental damage depending on target and timing.


## 2026-09-27 — Class-defined defenses

**Decision:** Every class has defined starting Physical Defense and Magical Defense as part of its base stat package.

**Decision:** Physical and Magical Defense use the same general diminishing-return formula structure; class/build differences come from values/modifiers rather than separate formula families.

## 2026-09-27 — Negative resistance and cursed healing

**Decision:** Elemental resistance currently has **no hard negative floor**.

**Decision:** Extremely negative elemental resistance can reduce matching elemental healing to zero and then invert it into damage below -100%.

**Decision:** This inverted elemental-healing behaviour is intentional design space.

## 2026-09-27 — Penetration/healing interaction remains open

**Open:** Exact interaction between elemental penetration and elemental healing.

**Current intuition:** elemental penetration on a healing source may act as the inverse of damage penetration, effectively treating the target as having additional matching resistance for that heal.

## 2026-09-27 — Damage/defense resolution buckets

**Decision:** Combat math uses explicit deterministic resolution buckets rather than arbitrary modifier order.

**Direction:** separate source scaling, instance definition, target mitigation, final modifiers and post-resolution triggers.

**Open:** exact mathematical order within/between those buckets until playable combat exists.

## 2026-09-27 — Regeneration

**Decision:** HP regeneration is always active in combat.

**Decision:** Items/classes may add special out-of-combat regeneration bonuses.

**Decision:** Resource regeneration is entirely class/resource-specific.

## 2026-09-27 — Shields and absorbs

**Decision:** Generic shield/absorb framework is required.

**Decision:** Shields may filter or scale by channel/element and can have bespoke behaviours, e.g. 500 Fire absorb but only 250 absorb for other damage.

## 2026-09-27 — Overhealing

**Decision:** Default overhealing disappears.

**Decision:** Classes/items/passives may explicitly convert overhealing into shields, resources, buffs, damage or other effects.

## 2026-09-27 — Lifesteal / damage-to-healing

**Decision:** Lifesteal, spell-vamp and damage-to-healing are framework-level mechanics rather than one-off class hacks.

## 2026-09-27 — Reflection / thorns

**Decision:** Reflection/thorns is framework-level.

**Decision:** Reflected damage carries context/tags preventing accidental infinite reflection loops.

## 2026-09-27 — Immunity and targetability flags

**Decision:** Framework must support statuses such as Invulnerable, Untargetable, Physical/Magical/Element Immune, Damage Immune, and CC-category immunities.

## 2026-09-27 — Crowd-control resistance and DR

**Decision:** Framework must support resistances to CC categories and class/item/passive bonuses to those resistances.

**Decision:** Repeated crowd-control chains require diminishing-returns/anti-lock framework so coordinated stun-heavy parties cannot permanently disable bosses.

**Open:** exact DR model and whether/how these resistances appear on the default character sheet.


## 2026-09-27 — Shield resolution order

**Decision:** Explicit shield priority overrides all normal ordering.

**Decision:** Otherwise, the most specific eligible shield is consumed before generic shields.

**Decision:** Among equally eligible generic shields, first-applied is consumed first.

**Decision:** Normal damage mitigation/resistance resolves before shield absorption unless a shield explicitly overrides this.

## 2026-09-27 — Pure Damage

**Decision:** Framework includes a rare **Pure Damage** type for exceptional mechanics.

**Decision:** Pure Damage ignores ordinary Physical Defense, Magical Defense and elemental resistance and deals its stated amount unless an effect explicitly interacts with Pure Damage.

**Decision:** Pure Damage is not a normal everyday class damage channel.

## 2026-09-27 — Healing inversion damage

**Decision:** Elemental healing inverted by extreme negative resistance becomes damage with no Physical/Magical channel.

## 2026-09-27 — Lifesteal defaults

**Decision:** Lifesteal uses final damage after mitigation unless explicitly overridden.

**Decision:** Default lifesteal is capped by actual HP damage inflicted, not theoretical overkill.

## 2026-09-27 — Reflection proc defaults

**Decision:** Reflected damage does not trigger normal lifesteal, on-hit, reflection or ordinary attack-proc chains unless explicitly enabled.

## 2026-09-27 — Aura stacking

**Decision:** Multiple copies of the same aura identity normally do not stack; the strongest eligible copy applies.

## 2026-09-27 — Dispel strengths

**Decision:** Effects define dispel behavior individually using Weak Dispel, Strong Dispel or Undispellable rules.

## 2026-09-27 — Death prevention and summons

**Decision:** Generic death-prevention framework is required.

**Decision:** Summons support both snapshot and dynamically inherited owner stats.

**Decision:** Summons have their own threat entries unless explicitly designed otherwise.

## 2026-09-27 — Combat state

**Decision:** Threat-list membership / active aggro is the primary rule for being in combat.

## 2026-09-27 — Target dummies

**Decision:** Training/test dummies exist both as development tools and in-world objects.

**Direction:** Starter-town dummies are simple/weak; later towns can provide tougher or more specialized dummies.

## 2026-09-27 — Floating-number visual language

**Decision:** Floating combat number color represents Physical / Magical / Pure / Healing.

**Decision:** Elements are communicated primarily through icons rather than assigning every element its own floating-number color.

## 2026-09-27 — World 1 first

**Decision:** Detailed world design focuses on the one-dot starter face before the other five faces.

**Decision:** Initial World 1 implementation is deliberately small and expands outward in connected chunks.

**Decision:** Undeveloped territory should be blocked by believable natural/world obstacles such as forests, mountains, collapsed roads, cliffs or gates; these can be removed/reworked as the region expands.

**Decision:** Face 1 should prove the starter town, outdoor loop, first dungeon, level-10 advancement, world services and long-term return hooks before later faces receive equivalent detail.


## 2026-09-27 — DiceBound visual inheritance

**Decision:** Each DiceFree face uses the corresponding DiceBound Board background and Normal combat background as its primary visual/environmental inspiration.

**Mapping:** Face 1 Green Road/woodland; Face 2 Astral/misty rocky forest; Face 3 Fractured/desolate ruined fortress; Face 4 Crown/cursed enchanted swamp; Face 5 Oblivion/volcanic mountain fortress; Face 6 End of Mathematics/ashen emberlit gothic citadel.

## 2026-09-27 — World 1 visual identity

**Decision:** World 1 is a green, welcoming start-of-adventure region of roads, forests, hills and pastoral terrain, deliberately evoking the feel of the Shire while remaining original.

**Decision:** A gigantic Yggdrasil-scale tree stands around the middle of World 1 and serves as the one-pip central landmark.

## 2026-09-27 — World 1 opening

**Decision:** The Echo emerges in southwest World 1 when a mountain expels/spits them out.

**Decision:** A short path leads from the emergence mountain to a small self-reliant mountain farming village.

## 2026-09-27 — Starter village

**Decision:** Starter village includes at least a resurrection point, account bank access, NPCs/quests, starter NPC crafting and crop fields.

**Decision:** The village is not a major economic hub; it is largely self-reliant.

## 2026-09-27 — Initial road and forest

**Decision:** One main road leaves the village eastward/slightly northward into forest, avoiding an immediate sightline/run toward the Dice-face edge.

**Decision:** Dense southern forest/terrain blocks early edge access.

**Decision:** The earliest development build ends the road at a believable dense-forest blocker which can later be removed/reworked as World 1 expands.

## 2026-09-27 — Opening Slime ecosystem

**Decision:** Slimes are the only initial enemy family.

**Decision:** Opening slimes are green and element-neutral.

**Decision:** Difficulty/visual escalation through the forest uses DiceBound Slime Board progression as inspiration: Board-1-like crop slimes, Board-2-like road slimes, Board-3-like deeper-forest slimes, Board-4-like dangerous pockets and potentially a Board-5-like elite.

## 2026-09-27 — Crop Slime quest

**Decision:** An early quest sends the player to kill Slimes damaging village crops.

**Direction:** quest XP plus required/natural slime kills should bring a fresh player to roughly level 4–5.

## 2026-09-27 — First Slime dungeon

**Decision:** The first dungeon is Slime-themed and physically located/discovered in the forest.

**Decision:** Its normal early boss is not the true apex/source of the Slimes.

**Decision:** The dungeon contains hints of a hidden route/condition leading eventually to a much stronger true Slime ruler/source (Mother/King/Queen/etc.; identity not finalized).

## 2026-09-27 — Abandoned-house mystery

**Decision:** Starter village contains an old abandoned locked house avoided by NPCs.

**Decision:** NPC dialogue can mention strange noises.

**Decision:** Access requires a key/condition found much later in the game; exact contents remain open.

## 2026-09-27 — First blessing at emergence mountain

**Decision:** On reaching level 10, the Echo feels an urge/call to return to the mountain that originally expelled them.

**Decision:** The first class blessing/advancement choice between Physically Blessed Novice and Magically Touched Novice occurs at that mountain.

## 2026-09-27 — Starter-village onward quest

**Decision:** A village NPC eventually sends/points the player toward the next town/settlement, providing the lightweight main-quest nudge out of the starter pocket.


## 2026-09-27 — Face biome diversity and edge blending

**Decision:** A DiceBound-derived face theme is a dominant identity, not a one-biome restriction.

**Decision:** Faces can contain multiple biomes.

**Direction:** The middle/interior of a face should generally express that face's strongest visual identity, while edges can increasingly reflect neighboring faces. Corners may combine influence from three faces and can become especially unusual.

## 2026-09-27 — Echo death is timeline continuation

**Decision:** Ordinary inhabitants do not resurrect.

**Decision:** When an Echo "dies", it is understood metaphysically as the Echo continuing into another timeline/manifestation where the death did not occur, rather than a corpse returning to life.

## 2026-09-27 — Resurrection-stone form

**Decision:** Resurrection points are ancient stone d6s resting on one corner.

**Decision:** The face/pip pattern corresponding to the current world is presented toward the player/camera and lights up when that stone is selected as the active resurrection point.

**Decision:** Ordinary inhabitants do not know these stones possess Echo-related power.

## 2026-09-27 — Cornberg

**Decision:** The starter settlement is named **Cornberg**: a small self-reliant farming village near the southwest corner mountain, with the name intentionally evoking corner mountain/Cornberg and local corn farming.

**Direction:** Roughly 10–15 main buildings plus farms/barns/utilities; perceived scale matters more than literal structure count.

**Decision:** Cornberg sits in a mountain-foot valley/bowl and uses local timber and stone construction.

**Decision:** A mountain stream feeds a mildly magical water source/well that improves crops, invigorates farmers and heals/restores nearby people.

## 2026-09-27 — Cornberg civic structure

**Decision:** Cornberg has no mayor or permanent village authority.

**Decision:** Once per week the villagers hold an open meeting in the central village green/square and rotate who serves as meeting president/chair.

## 2026-09-27 — Cornberg services

**Decision:** Cornberg includes a general-goods merchant selling starter equipment.

**Decision:** A small blacksmith introduces NPC gear crafting with early recipes somewhat stronger than general-store gear and/or covering gear slots the store does not sell.

**Decision:** Cornberg has Alchemist, Fisherman and Cook profession teachers.

**Decision:** Profession training eligibility is any Tier 1+ class OR a level-25+ Novice. Novice is Tier 0.

**Decision:** Cornberg has no conventional tavern/inn; it has a small working brewery with communal tables/chairs around the brewing area.

## 2026-09-27 — Cornberg retired-adventurer couple

**Direction:** Cornberg may contain a retired magical adventurer and her girlfriend, a former sword-wielder who put down her sword to become a farmer.

**Direction:** They can later guide Magically Touched and Physically Blessed lines toward class-specific teachers/quests/advancements. Exact class branches and responsibilities remain open.

## 2026-09-27 — Cornberg Slime situation

**Decision:** Slimes are normally known local pests that stay outside the crops/village.

**Decision:** The opening infestation is unusually severe and brings them into the crop fields, quietly foreshadowing a deeper Slime problem.

## 2026-09-27 — Cornberg arrival

**Decision:** Nobody witnesses the Echo emerge from the mountain; the Echo walks into Cornberg alone.

## 2026-09-27 — Cornberg mountain folklore

**Decision:** The mountain has never previously been known to spit out a person.

**Decision:** It nevertheless has abundant local folklore, helped by the magical water and normal village mythmaking.

## 2026-09-27 — Cornberg abandoned house

**Decision:** Villagers give contradictory accounts of the abandoned house's history/former owner.

## 2026-09-27 — Cornberg runner

**Decision:** Cornberg sends one runner to the next settlement each week to carry and return with important news before the weekly open session.

**Direction:** The runner returns shortly after the initial Slime quest/follow-up and becomes the first natural main-quest bridge toward the next town.


## 2026-09-27 — First Cornberg resurrection-stone activation

**Decision:** On the Echo's first arrival in Cornberg, a short camera focus/zoom presents the stone d6 and the one-pip side lights automatically.

**Decision:** The first stone sets Cornberg as the active respawn point automatically; a concise UI message may confirm this.

## 2026-09-27 — Cornberg well

**Decision:** The magical well is Cornberg's normal healing/restoration point.

**Decision:** It is mechanically distinct from the resurrection stone.

## 2026-09-27 — Hidden level-25 Novice profession access

**Decision:** Level-25+ Novices can learn professions despite remaining Tier 0.

**Decision:** This eligibility is deliberately hidden/not explicitly advertised to players.

## 2026-09-27 — Cornberg opening quest progression

**Direction:** Initial XP/progression targets are:
- first 3 crop Slimes -> around level 2;
- 2 more crop Slimes + first quest turn-in -> around level 3;
- investigate road + kill 3 stronger Slimes + turn-in -> around level 5;
- named Slime quest + turn-in -> around level 6;
- final anti-Slime objective(s) + onward-runner setup -> around level 10.

**Decision:** Exact XP values require playtesting.

## 2026-09-27 — Multi-path anti-Slime quest

**Direction:** The final local Slime-problem quest can be completed through alternative objectives such as clearing the Slime dungeon, killing the elite Slime, or reaching a large total Slime-kill count.

**Decision:** This is intended as an early proof of multi-path quest completion.

## 2026-09-27 — Cornberg runner transition

**Decision:** The weekly runner returns because excessive Slime activity prevented them reaching the next town.

**Decision:** Cornberg then asks the Echo to become the runner because the Echo has proved capable of handling the road.

## 2026-09-27 — Cornberg repeatable Slime work

**Decision:** After the initial quest chain, the former swordswoman can offer repeatable Slime-related sidequests/bounties.

## 2026-09-27 — Peter Banker

**Decision:** Cornberg's banker is **Peter Banker**, with the working gag/title "your friendly neighborhood banker-man."

## 2026-09-27 — Separate Cornberg blacksmith

**Decision:** The blacksmith/crafter has a separate forge/building from the general-goods store.

## 2026-09-27 — Retired magic-user village role

**Decision:** The retired magical adventurer is not the dedicated healer NPC; the magical well fills that role.

**Open:** Her mundane Cornberg role may be herbalist, brewery helper, housewife/general magical helper or another fitting village role.


## 2026-09-28 — HUD default style

**Decision:** DiceFree's default HUD is primarily Warcraft III-inspired: compact but information-rich, with player portrait/model and command/ability area at the bottom.

## 2026-09-28 — Player HUD panel

**Decision:** Player portrait or 3D model appears in the bottom HUD.

**Decision:** HP numbers use current/max format.

## 2026-09-28 — World-space HP/nameplates

**Decision:** Units can show name, level and HP bar above their world model.

**Decision:** HP-bar visibility supports Always / Only when hurt / Never.

**Decision:** Visibility settings are independently configurable for self/own units, allies and enemies.

## 2026-09-28 — Ability-grid capacity

**Decision:** Default lower-right HUD reserves a maximum visible 12-slot ability/command grid arranged as 3 rows x 4 columns.

**Decision:** This is layout capacity, not a requirement that classes have 12 active abilities.

## 2026-09-28 — Party, target, minimap and quest positions

**Decision:** Party frames default top-left and include the player's own frame.

**Decision:** Target frame defaults top-center.

**Decision:** Minimap defaults to a square panel top-right.

**Decision:** Quest tracker sits below the minimap and can be hidden.

## 2026-09-28 — Modular HUD architecture

**Decision:** HUD architecture must be panel/widget-based, movable, scalable and hideable long-term.

**Direction:** Players should eventually be able to create layouts inspired by WC3, Diablo, WoW, RuneScape or their own arrangement.

**Decision:** The POC only needs one strong default layout plus a framework that does not prevent later customization; a full HUD editor is deferred.

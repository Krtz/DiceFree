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


## 2026-09-28 — Live player model in HUD

**Decision:** The default bottom player frame uses a live real-time 3D player model/portrait, reflecting class and visible equipped gear where practical.

## 2026-09-28 — XP bar

**Decision:** Default XP presentation is a long thin bar across the bottom of the screen.

## 2026-09-28 — Buff/debuff placement

**Decision:** Player buffs/debuffs default around the bottom player frame.

**Decision:** Target buffs/debuffs default below/around the top-center target frame.

**Decision:** These widgets must be movable and scalable long-term.

## 2026-09-28 — Cast-bar layers

**Decision:** Units may show compact world-space cast bars above their models.

**Decision:** World-space cast-bar visibility is independently toggleable for self/own units, allies and enemies.

**Decision:** The player also has a dedicated movable/scalable cast bar above the bottom-center player information.

**Decision:** The selected target has a cast bar below the top-center target frame.

## 2026-09-28 — Target of target

**Decision:** Target-of-target is supported and can be toggled in options.

## 2026-09-28 — System/menu buttons

**Decision:** The default HUD exposes only a few visible system/menu buttons, Diablo-like in spirit; most actions also use hotkeys.

## 2026-09-28 — Interaction prompts

**Decision:** Interaction prompts should appear near the relevant world object/NPC, with a subtle screen-space fallback where needed.


## 2026-09-28 — Party-frame information

**Decision:** Party frames default to portrait/class icon, name, level, HP bar/current-max, useful resource, important effects, threat indicator, resurrection sickness and a small cast bar.

**Decision:** Party-frame cast bars are toggleable.

## 2026-09-28 — Buff/debuff filtering

**Decision:** Default effect filtering prioritizes important class buffs, harmful debuffs, encounter mechanics and effects the current character can actually remove.

**Decision:** Cleanse/purge highlighting is capability-based, not healer-role-based.

**Decision:** Players can opt into showing all effects.

## 2026-09-28 — Minimap orientation

**Decision:** Square minimap defaults to fixed north-up orientation.

**Decision:** Rotating minimap mode is available as an option.

## 2026-09-28 — Ground loot presentation

**Decision:** Overworld loot is scarce/meaningful and should use a Warcraft III-like readable ground-drop presentation rather than Diablo-style loot spam assumptions.

## 2026-09-28 — Carried gold HUD

**Decision:** Carried gold has a small HUD display by default because it is exposed to death loss.

**Decision:** Bank gold remains hidden from the persistent HUD.

**Decision:** Carried-gold HUD display can be disabled.

## 2026-09-28 — Chat / system / combat log

**Direction:** Default chat/log presentation is a bottom-left fading panel with Chat/System/Combat tabs or modes.

## 2026-09-28 — Character/inventory windows

**Decision:** Character/inventory and similar interfaces are movable overlay windows over the live world rather than full-screen pause menus.

## 2026-09-28 — Contextual cursor

**Decision:** Mouse cursor uses Warcraft III-like contextual states for move, attack, talk, loot, interact and unavailable actions.


## 2026-09-28 — Boss HUD

**Decision:** Bosses use a larger top-center frame with large HP, cast bar, relevant phase/mechanic information and support for multiple active boss bars when needed.

## 2026-09-28 — Boss mechanic communication

**Decision:** Important mechanics can use large center-screen warnings.

**Decision:** Dangerous ground mechanics should also use strong world-space telegraphs such as red circles/cones/lines ("ouch circles").

## 2026-09-28 — Quest markers and journal

**Decision:** Quest markers use standard **!** for available quests and **?** for turn-in/completion.

**Decision:** A quest journal lets players choose which active quests are pinned to the HUD tracker.

**Direction:** Default tracker capacity should be modest, roughly 3–5 quests.

## 2026-09-28 — Fog of war

**Decision:** Map/minimap uses Warcraft III-style fog of war.

**Decision:** Unexplored terrain is black/hidden.

**Decision:** Seen terrain remains revealed for static geography/objects, while current dynamic activity still requires present vision.

**Decision:** Party/allied players share vision.

## 2026-09-28 — Threat-state indicator

**Decision:** Target/party UI uses simple threat-state color/icon indicators by default.

**Direction:** green = safe/low, yellow = rising/contested, red = aggro/current primary threat.

## 2026-09-28 — Low-health feedback

**Decision:** Default low-health warning centers on the player frame: redder frame/portrait treatment, hurt-looking model and increasingly red HP text.

**Decision:** Optional bloody/red screen-edge vignette is supported and toggleable.


## 2026-09-28 — Contextual mouse controls

**Decision:** Classic Mouse uses Warcraft III-style contextual right-click for movement, attack and interaction.

**Decision:** Left-click remains the primary selection/UI click.

## 2026-09-28 — Selection safety

**Decision:** Selection controls must be configurable between ARPG-like hero focus and more RTS-like unit selection.

**Decision:** Accidental selection of allies/summons/other units must not prevent responsive emergency movement in combat.

**Direction:** Framework should support selection locks/filters/modifier requirements and controlled-unit categories.

## 2026-09-28 — Attack-Move / Stop / Hold

**Decision:** Dedicated Attack-Move is required.

**Decision:** Dedicated Stop and Hold Position commands are required.

**Direction:** Classic defaults may use S for Stop and H for Hold, subject to rebinding/profile design.

## 2026-09-28 — Context basic attack

**Decision:** Right-clicking a hostile target commands a basic attack.

**Decision:** A dedicated Warcraft III-style Attack command/button also exists.

## 2026-09-28 — Input profiles

**Decision:** Classic Mouse and Direct/WASD ship with different default keybind profiles.

**Decision:** All bindings remain configurable.

**Open:** Exact ability/interact bindings for each profile.

## 2026-09-28 — Target cycling

**Decision:** Default hostile target cycling supports Tab forward and Shift+Tab backward.

**Decision:** Friendly/allied target cycling can have separate configurable bindings.

## 2026-09-28 — Free camera and hero follow

**Decision:** Camera can move independently of the hero, Warcraft III-style.

**Decision:** Players can observe allies/other areas and control/send summons away from the hero.

**Decision:** A dedicated action/hotkey returns/follows the hero, and optional hero-follow behavior is supported.


## 2026-09-28 — Hero remains primary command focus

**Decision:** The hero is the default primary controlled/selected unit.

**Decision:** Targeting allies/enemies does not have to transfer primary command focus away from the hero.

## 2026-09-28 — Summon control groups and drag selection

**Decision:** Controllable summons/units support direct selection.

**Decision:** Framework supports RTS-style control groups.

**Decision:** Framework supports drag/box selection of multiple controlled units, with player options to disable or limit it.

## 2026-09-28 — Three casting modes

**Decision:** DiceFree supports three targeted-ability casting modes:
1. Normal/confirm cast — press hotkey, preview targeting, click to cast.
2. Semi Quick Cast — hold hotkey to preview, release to cast.
3. Quick Cast — cast immediately on hotkey press.

**Decision:** Casting style is configurable and should be capable of per-ability customization.

## 2026-09-28 — Out-of-range casting

**Decision:** If an otherwise valid cast target/location is out of range, the unit normally moves toward a valid casting position and executes the queued ability once in range.

**Decision:** Individual abilities may explicitly override this.

## 2026-09-28 — Self-cast input

**Decision:** Eligible abilities support configurable self-cast input.

**Direction:** Alt+ability is a good default candidate.

**Decision:** Double-press-to-self-cast is supported as an option.

## 2026-09-28 — Camera movement options

**Decision:** Camera framework supports WC3-style edge scrolling, manual panning, zoom/rotation, hero recenter and persistent hero-follow.

**Decision:** These behaviors are configurable, including disabling edge scrolling.


## 2026-09-28 — Direct summon selection remains available

**Decision:** Controllable summons can still be directly left-click selected.

**Decision:** Hero-focused selection safety is configurable rather than hard-locking summons out of direct selection.

## 2026-09-28 — Function-key unit selection

**Decision:** Default unit-selection shortcuts use function keys:
- F1 = hero;
- F2 = summon/unit 1;
- F3 = summon/unit 2;
- etc.

**Decision:** Bindings remain configurable.

## 2026-09-28 — Selected-unit action bar

**Decision:** The action/command bar changes to the abilities/commands of the currently selected controllable unit.

## 2026-09-28 — Autonomous summon framework

**Decision:** Framework also supports autonomous summons/minions that are not directly micro-controlled peers.

**Decision:** Autonomous summons can use stance systems such as Aggressive / Defensive / Passive / Hold.

## 2026-09-28 — Target-cycle priority

**Decision:** Hostile target cycling prioritizes nearby/relevant enemies around the hero, with engaged threats and elites/bosses favored over distant trivial targets.

## 2026-09-28 — Unit collision and ghosting

**Decision:** Heroes, enemies and normal controllable units occupy physical space and use real collision/pathing.

**Decision:** They do not freely walk through one another by default.

**Decision:** Ghosting/phasing is a rare explicit mechanic for specific classes, enemies, passives or effects.

## 2026-09-28 — Target persistence

**Decision:** Current target persists when the player clicks empty terrain/moves.

**Decision:** Target-persistence behavior is configurable.

## 2026-09-28 — Auto-retarget after kill

**Decision:** Attack-Move can automatically acquire a new nearby enemy after a kill.

**Decision:** Explicit single-target attack commands do not auto-chain by default.

**Decision:** Retarget behavior is configurable.


## 2026-09-28 — Novice identity

**Decision:** Novice is a Tier 0 blank-slate class intended to sample four broad future combat identities without specializing.

**Decision:** Novice has exactly four normal active skills and no normal passive.

**Decision:** Each normal Novice skill can be raised to skill level 10.

## 2026-09-28 — Novice starting stats and HP

**Decision:** Level-1 Novice begins with 1 Vitality, 1 Strength, 1 Agility, 1 Intelligence and 1 Spirit.

**Direction:** Starting HP is 10 base + the normal Vitality-derived HP contribution.

## 2026-09-28 — Novice basic attack scaling

**Decision:** Novice basic attack scales from the highest of all five primary attributes.

## 2026-09-28 — Novice resource

**Decision:** Novice has no class resource; its four skills use cooldowns.

**Direction:** Novice cooldowns should be somewhat long so the starter kit remains simple/weak.

## 2026-09-28 — Novice skill prototypes

**Direction / balance targets:**
- STR melee stun: STR × 2 damage; stun = skill level × 0.2.
- INT ranged magic sand: INT × 2 damage; attack miss chance = skill level × 7.5%.
- AGI attack-speed buff: 10 + (skill level × 0.2 × AGI)% current formula direction.
- SPI heal: Spirit × 10 healing.

**Decision:** These formulas are provisional balance targets and may change after playtesting.

## 2026-09-28 — Level-200 Novice payoff

**Decision:** Level-200 Novice receives a hidden stronger payoff.

**Direction:** This includes a stronger secret active skill and may include Novice's first passive.

**Decision:** The secret active does not appear in the normal skill-level menu.

## 2026-09-28 — Novice equipment and appearance

**Decision:** Fresh Novice starts with no equipped gear.

**Decision:** Baseline appearance is androgynous underwear + T-shirt.

**Decision:** Novice can equip only gear explicitly permitted for Novice.

## 2026-09-28 — Advancement creates a new manifestation slot

**Decision:** Choosing an advancement does not overwrite/delete the parent manifestation.

**Decision:** The parent Novice remains playable and the selected advanced class is created as a new level-1 character/manifestation slot.

**Decision:** The same Novice can later choose another available branch, producing another manifestation.

**Open:** exact inventory/equipment/quest-state inheritance and total slot limits.


## 2026-09-28 — Novice skill-point cadence

**Decision:** Fresh level-1 Novice starts with 1 unspent skill point and all four normal skills at rank 0.

**Decision:** Rank 1 must be purchased.

**Decision:** Novice gains 1 skill point per character level.

## 2026-09-28 — Novice stat growth

**Decision:** Novice gains +1 to each of VIT/STR/AGI/INT/SPI every level.

## 2026-09-28 — Novice unarmed basic attack

**Decision:** Novice's baseline basic attack is an unarmed/fist attack.

**Decision:** It scales from the highest of all five attributes while remaining physically presented as a simple melee hit.

## 2026-09-28 — Novice skill behavior refinements

**Decision:** STR stun duration is skill level × 0.2 seconds, up to 2.0 seconds at rank 10, and remains melee-range.

**Direction:** Magic Sand miss debuff currently targets ~5 seconds; 10 seconds may be tested.

**Decision:** Magic Sand miss chance affects actions tagged as requiring accuracy.

**Direction:** AGI attack-speed buff currently targets ~5 seconds and can target self or allies.

**Decision:** SPI heal is instant and can target self or allies.

## 2026-09-28 — Advancement is an in-session timeline fork

**Decision:** Advancement occurs inside the current game/session and immediately continues play as the newly created advanced manifestation.

**Decision:** The parent manifestation remains preserved at the branch point for later play.

**Decision:** Manifestation-specific quest/world state is copied at branch creation and can diverge independently afterward.

## 2026-09-28 — Equipped gear duplicates on advancement fork

**Decision:** Equipped gear is duplicated into the new child manifestation.

**Decision:** Parent keeps the originals; child receives copies.

**Decision:** Duplicates follow normal account-bound rules and normal equipment requirements.

**Open:** whether carried inventory beyond equipped gear is also duplicated.


## 2026-09-28 — Novice all-stat passive

**Decision:** Novice skill points can also be spent in a passive that grants +1 to all five primary stats per point invested.

**Open:** exact rank cap, if any.

## 2026-09-28 — Novice skill-point notification

**Decision:** Level-up should visibly notify the player when a skill point is available, but spending it is never mandatory.

## 2026-09-28 — Novice-only respec

**Decision:** Cornberg's retired adventurer couple can respec/reallocate Novice skill points.

**Decision:** This service is Novice-only.

**Open:** cost/cooldown/presentation.

## 2026-09-28 — Magic Sand duration

**Decision:** Magic Sand's miss debuff uses **5 seconds** as the current implementation value.

## 2026-09-28 — Novice stun ignores CC resistance

**Decision:** The Novice STR stun ignores ordinary CC resistance and applies its listed stun duration to valid targets.

**Open:** interaction with explicit hard stun-immunity flags.

## 2026-09-28 — Full advancement snapshot

**Decision:** Advancement snapshots all manifestation-specific state into the new child manifestation, including equipment, carried inventory/currency, quest/world progression and relevant skill/class state.

**Decision:** Parent keeps the original state; child gets a duplicate snapshot; both diverge afterward.

## 2026-09-28 — Parent resumes before blessing

**Decision:** Returning to the preserved parent manifestation resumes immediately before the blessing/branch choice that created the child, allowing another branch to be chosen later.


## 2026-09-28 — Novice passive cap / respec / stun immunity boundary

**Decision:** Novice all-stat passive caps at 160 ranks.

**Decision:** Cornberg retired-adventurer Novice respec is free.

**Decision:** Novice STR stun bypasses ordinary CC resistance/DR but does not bypass explicit hard Stun Immunity.

## 2026-09-28 — Tier 1 physical stat direction

**Working balance target:** Physically Blessed Novice starts 12 VIT / 12 STR / 12 AGI / 5 INT / 5 SPI.

**Working growth target:** +1 VIT, +2 STR, +2 AGI, +0.5 INT, +0.5 SPI per level on average.

**Implementation direction:** represent half-stat growth as +1 every two levels rather than visible fractional stats.

## 2026-09-28 — Tier 1 magical stat direction

**Working balance target:** Magically Touched Novice starts 10 VIT / 5 STR / 5 AGI / 13 INT / 13 SPI.

**Working growth target:** +1 VIT, +0.5 STR, +0.5 AGI, +2 INT, +2 SPI per level on average.

**Implementation direction:** represent half-stat growth as +1 every two levels.

## 2026-09-28 — Tier 1 Mana direction

**Strong working direction:** Both first-advancement classes use Mana.

**Reason:** Tier 1 teaches resource management through a simple shared system; later specialized classes may replace Mana with Rage, Combo Points/Energy, Focus or other bespoke resources.

## 2026-09-28 — Candidate level-30 branch breadth

**Working direction:** Consider four physical and four magical branches at level 30.

Physical candidates:
- sword-and-board tank;
- rogue/agile melee;
- ranger/physical ranged;
- two-handed melee DPS.

Magical candidates:
- shield/buff healer-support;
- healing-focused healer;
- arcane wizard;
- occult caster.

This is not yet locked because of class-count/content-production implications.


## 2026-09-28 — Large class count is intentional

**Decision:** A very large class tree is an explicit long-term goal, not a design failure to avoid.

**Decision:** Development does not need symmetric branch completion. Different classes may have different numbers of implemented successors at a given time.

**Architecture requirement:** advancement/class systems must support arbitrary branch counts and partially filled trees.

## 2026-09-28 — Tier 1 skill-rank pattern

**Working decision:** Physically Blessed Novice and Magically Touched Novice each use 5 skills with 6 ranks.

**Decision:** Tier 1 starts with 1 skill point at level 1 and gains 1 per level, giving exactly 30 points by level 30.

**Decision:** Novice skill ranks do not numerically carry into Tier 1; evolved abilities restart as Tier-1 skills.

## 2026-09-28 — Physically Blessed working kit

**Working direction:** five identities:
- adaptive STR/AGI Heavy Strike with ordinary-resistance stun;
- Guard/Brace defensive active;
- Quickening attack-speed/tempo skill;
- ground-targeted Arrow Rain physical AoE as Ranger foreshadowing;
- broad Martial Aptitude passive.

**Working decision:** Heavy Strike damage uses the higher of STR or AGI rather than splitting damage/stun formulas across different stats unless playtesting justifies the extra complexity.

## 2026-09-28 — Magically Touched working kit

**Working direction:** five identities:
- evolved Magic Sand as Nature-element single-target damage/debuff;
- Mend heal;
- elemental/support buff rather than an early barrier;
- Arcane-style AoE damage;
- Mana Attunement passive increasing max Mana and Mana regeneration.

**Direction:** Tier-2 barrier/buff support should emphasize prevention/enhancement, while the healing branch emphasizes healing/cleanse.

## 2026-09-28 — Tier 1 Mana asymmetry

**Decision:** Both Tier-1 classes use Mana as the introductory resource.

**Direction:** Magically Touched has a larger pool, higher spell costs and stronger cost growth, and cares more about INT/Mana regeneration.

**Direction:** Physically Blessed abilities cost less Mana and the class is less regeneration-dependent; low INT must not make it nonfunctional.

## 2026-09-28 — DiceBound element inheritance

**Decision:** DiceFree uses the full canonical DiceBound element roster as its baseline vocabulary, adapted for real-time combat rather than blindly copying DiceBound numbers.

**Decision:** Nature is explicitly a DiceFree element regardless of DiceBound's eventual exact roster.

**Direction:** A later Nature Shaman descendant belongs somewhere in the broad esoteric/occult magical lineage.


## 2026-09-28 — Class progression is a graph/web

**Decision:** DiceFree class progression is not required to be a strict tree. It supports linear paths, splits and convergence.

**Decision:** Some classes may not split at all (e.g. Priest -> High Priest -> Arch Priest), while others may branch heavily.

## 2026-09-28 — Echo-wide class milestones

**Decision:** Reaching meaningful class/tier milestones is permanently recorded Echo-wide.

**Decision:** Deleting/cleaning up the manifestation that earned a milestone does not remove that milestone.

## 2026-09-28 — Multi-lineage convergence requirements

**Decision:** A class may require milestones from multiple separate manifestations/lineages.

**Decision:** Requirement logic supports AND, OR and nested combinations plus non-class predicates such as quests, items, achievements and discoveries.

## 2026-09-28 — Convergence routes

**Decision:** The same convergence class may be entered from multiple qualifying parent lineages.

**Decision:** Different parent routes may use different NPCs, quests, locations and advancement presentation while producing the same resulting class ID/kit.

**Decision:** The same prerequisite combination may unlock multiple different convergence classes when appropriate.

## 2026-09-28 — Secret class visibility modes

**Decision:** Class visibility/disclosure is per class.

Supported design modes include:
- visible with explicit requirements;
- visible/teased with hidden requirements or ???;
- completely hidden until discovered/unlocked/become-able.

## 2026-09-28 — Way discovery persistence

**Decision:** Discovering/unlocking knowledge of a Way is Echo-wide and permanent.

**Decision:** Becoming that class remains manifestation-specific and still requires an eligible advancement route.

## 2026-09-28 — Higher-tier convergence

**Decision:** Cross-lineage requirements may continue at higher tiers and are decided class-by-class.

A convergence class may itself split, continue linearly, converge again or have no successor.

## 2026-09-28 — Manifestation slot philosophy

**Decision:** Manifestation slots should be effectively unlimited for ordinary play rather than intentionally capped as a progression constraint.

**Decision:** Cleanup/organization/archive/delete tools are desirable for manageability.

**Decision:** Removing a manifestation never removes permanent Echo-wide class milestones or discovered Ways.

## 2026-09-28 — Class-web UI direction

**Direction:** Do not force progression into one literal tree UI.

Tier tabs, graph/web views, current-lineage views and discovered-Ways views are all valid approaches.

**Decision:** Hidden classes must not accidentally leak through UI structure unless that class is intentionally teased.


## 2026-09-28 — One persistent save-state per class

**Decision:** The Echo has at most one persistent manifestation/save-state for each class ID.

**Decision:** The same class cannot coexist as multiple duplicate timelines/save-slots.

**Decision:** Reaching the same convergence class through another valid parent route does not create or overwrite another copy.

**Direction:** The class/Ways UI doubles as the start-game selector for existing class save-states.

## 2026-09-28 — Archive preferred over duplicate-slot cleanup

**Direction:** Archive is the preferred way to hide/manage class saves in a very large roster while keeping them recoverable.

**Decision:** Deleting/archiving class-local state never removes Echo-wide milestones, discoveries or unlock-event history.

**Open:** exact fresh/recovery behavior if an unlocked class save is permanently deleted.

## 2026-09-28 — Discovery, eligibility and save existence are separate

**Decision:** "Way discovered", "requirements satisfied", and "class save-state exists" are separate pieces of state.

A discovered hidden class may become visible with unknown requirements before the Echo is eligible to become it.

## 2026-09-28 — Specific and category class-history requirements

**Decision:** Requirements can reference exact classes or broader tagged history.

Examples include:
- reach Tier 2 Wizard;
- reach Tier 2 anywhere in the Magically Touched tree;
- reach Tier 3 in any Nature class;
- reach Tier 3 in three distinct elemental caster Ways;
- unlock a specific class such as Death Knight.

## 2026-09-28 — Check-only vs consuming requirements

**Decision:** Item/event requirements support both possession/check-only predicates and authored consuming/sacrifice events.

**Decision:** A completed class-relevant event can create a permanent Echo-wide milestone.

**Decision:** A late-tier event may unlock an earlier-tier class for future play.

## 2026-09-28 — No permanent class/content lockouts

**Decision:** DiceFree should not use irreversible choices that permanently lock the Echo out of another class/Way or major progression route.

Different routes may have distinct quests and flavor, but the player must be able to pursue other unlocks later.


## 2026-09-28 — Way unlock does not allow direct class start

**Decision:** Discovering/unlocking a class Echo-wide only makes its advancement available.

**Decision:** A missing class save must always be created by loading an appropriate earlier/prerequisite class and completing the target class's advancement quest/event.

**Example:** A Tier-5 event may unlock a Tier-1 Way, but the player then loads Novice and performs that Tier-1 advancement.

## 2026-09-28 — Convergence classes share their advancement quest

**Decision:** A convergence class normally has one target-class advancement quest/event regardless of which qualifying parent lineage is used.

The parent lineage supplies the timeline snapshot, not a different version of the resulting class.

## 2026-09-28 — Deleted class saves must be re-earned

**Decision:** Permanent deletion removes only that class's local save-state.

**Decision:** Echo-wide Way discovery, class milestones and unlock events remain.

**Decision:** To recreate the class, the player must load an appropriate earlier/prerequisite class and advance into it again.

## 2026-09-28 — Archive remains non-destructive

**Decision:** Archived class saves remain fully valid and continue to count through their already-recorded Echo-wide milestones.

Archive is presentation/organization only.

## 2026-09-28 — Class switching in town

**Decision:** Players can switch between existing class saves from a safe town.

**Decision:** In multiplayer, the party/session remains intact during the switch.

**Decision:** The newly selected class appears at that class save's own latest registered resurrection point.

## 2026-09-28 — Per-class resurrection point

**Decision:** Every class save stores its own latest registered resurrection point.

**Decision:** Starting/loading that class normally spawns at that point.

## 2026-09-28 — Host world state controls session presentation

**Decision:** In multiplayer, the host's world/quest state determines the live world's physical/presentation state for all players.

**Decision:** A guest retains their own manifestation-specific quest/world progression; joining the host does not overwrite it.

**Decision:** Quest/event credit remains eligibility-based.

**Open:** how guest quests interact with host-state NPCs/objects that have been removed or transformed.


## 2026-09-28 — Way unlock does not allow direct class start

**Decision:** Discovering/unlocking a class Echo-wide only makes its advancement available.

**Decision:** A missing class save must always be created by loading an appropriate earlier/prerequisite class and completing the target class's advancement quest/event.

**Example:** A Tier-5 event may unlock a Tier-1 Way, but the player then loads Novice and performs that Tier-1 advancement.

## 2026-09-28 — Convergence classes share their advancement quest

**Decision:** A convergence class normally has one target-class advancement quest/event regardless of which qualifying parent lineage is used.

The parent lineage supplies the timeline snapshot, not a different version of the resulting class.

## 2026-09-28 — Deleted class saves must be re-earned

**Decision:** Permanent deletion removes only that class's local save-state.

**Decision:** Echo-wide Way discovery, class milestones and unlock events remain.

**Decision:** To recreate the class, the player must load an appropriate earlier/prerequisite class and advance into it again.

## 2026-09-28 — Archive remains non-destructive

**Decision:** Archived class saves remain fully valid and continue to count through their already-recorded Echo-wide milestones.

Archive is presentation/organization only.

## 2026-09-28 — Class switching in town

**Decision:** Players can switch between existing class saves from a safe town.

**Decision:** In multiplayer, the party/session remains intact during the switch.

**Decision:** The newly selected class appears at that class save's own latest registered resurrection point.

## 2026-09-28 — Per-class resurrection point

**Decision:** Every class save stores its own latest registered resurrection point.

**Decision:** Starting/loading that class normally spawns at that point.

## 2026-09-28 — Host world state controls session presentation

**Decision:** In multiplayer, the host's world/quest state determines the live world's physical/presentation state for all players.

**Decision:** A guest retains their own manifestation-specific quest/world progression; joining the host does not overwrite it.

**Decision:** Quest/event credit remains eligibility-based.

**Open:** how guest quests interact with host-state NPCs/objects that have been removed or transformed.


## 2026-09-28 — Resurrection-stone-only class switching

**Decision:** In-world class switching is available only through resurrection stones.

**Decision:** Switching preserves the multiplayer session and loads the chosen existing class at that class save's own registered resurrection point.

**Decision:** Class level does not restrict switching; co-op scaling is a separate concern.

## 2026-09-28 — Advancement inherits resurrection point

**Decision:** A newly-created class save inherits the parent class's currently registered resurrection point.

**Decision:** If a class's remembered resurrection point is unavailable/invalid in the current game/session, Cornberg is the fallback.

## 2026-09-28 — Switching does not transfer bank/inventory state

**Decision:** Each class keeps its own carried inventory/equipment/currency.

**Decision:** Switching classes does not automatically transfer items or open/use the shared bank.

The player must visit a bank normally to withdraw shared items.

## 2026-09-28 — Host-session quest availability

**Decision:** The host's world state determines which NPCs, objects, bosses and events physically exist in the current multiplayer game.

**Decision:** If a guest's quest requires something absent because of host progression, that quest step is simply unavailable in that session.

**Decision:** The guest's own quest state remains unchanged and can be completed in another compatible game/session.

**Decision:** When the host world still supports the required action, eligible guests may progress their own quests even if the host has already completed those quests.


## 2026-09-28 — Physically Blessed basic attack and weapon breadth

**Decision:** Physically Blessed basic attacks scale from the higher of STR or AGI.

**Decision:** Tier 1 can use essentially all ordinary low-level melee weapon families as they are introduced, while remaining a melee class without true ranged basic attacks.

## 2026-09-28 — Physically Blessed Heavy Strike target values

**Decision:** Heavy Strike damage uses the higher of STR or AGI.

**Working balance target:** stun is 0.4 seconds at rank 1 and gains +0.2 seconds per additional rank, reaching 1.4 seconds at rank 6.

**Decision:** Heavy Strike uses normal CC resistance/DR and true-immunity rules.

## 2026-09-28 — Physically Blessed Guard / Brace target values

**Working balance target:** ~3 second duration.

**Working balance target:** 15% Physical/Magical damage reduction at rank 1, +5 percentage points per additional rank, reaching 40% at rank 6.

## 2026-09-28 — Physically Blessed Quickening

**Decision:** Quickening is self-only.

**Direction:** It grants Attack Speed plus a small Movement Speed bonus for roughly 5 seconds.

## 2026-09-28 — Physically Blessed Arrow Rain

**Decision:** Arrow Rain is a short-burst, ground-targeted physical AoE using higher-of-STR/AGI scaling.

**Decision:** The arrows are manifested/spectral enough that Physically Blessed does not need to equip a bow.

## 2026-09-28 — Physically Blessed Martial Aptitude

**Decision:** Keep the Tier-1 passive deliberately simple: increase basic-attack damage and Physical Defense.

More exotic class mechanics are deferred to later specializations.

## 2026-09-28 — Magically Touched Elemental Imbuement direction

**Decision:** The Tier-1 support skill buffs self/allies by adding elemental damage to basic attacks.

**Direction:** The element should be either **Light or Fire**; exact choice remains open.

The skill should demonstrate elemental/support gameplay without over-specializing the blank-slate Tier-1 class.


## 2026-09-28 — Magically Touched basic attack / equipment

**Decision:** Magically Touched uses a ranged magical basic attack scaling from the higher of INT or SPI.

**Decision:** Tier 1 has broad access to ordinary low-level magical weapon families such as staves, wands, focuses/orbs and caster-style introductory weapons as they are introduced.

## 2026-09-28 — Magic Sand Tier-1 refinement

**Decision:** Magic Sand is Nature-element, ranged and INT-scaled.

**Decision:** Miss chance increases by 7.5 percentage points per rank.

**Decision:** Debuff duration stays fixed at 5 seconds across all six ranks.

## 2026-09-28 — Mend Tier-1 refinement

**Decision:** Mend scales from SPI only and targets self/allies.

**Working balance target:** approximately 10 second cooldown plus a meaningful Mana cost.

## 2026-09-28 — Elemental Imbuement uses Fire

**Decision:** Magically Touched Elemental Imbuement adds **Fire-element** damage to basic attacks.

## 2026-09-28 — Ice delayed AoE

**Decision:** Magically Touched AoE is a small delayed ground-targeted **Ice** explosion.

**Decision:** It applies 2% Movement Speed slow per skill rank, reaching 12% at rank 6 before later resistance/immunity rules.

## 2026-09-28 — Mana Attunement aura

**Direction:** Mana Attunement grants flat Max Mana and flat personal Mana regeneration per rank.

**Direction:** It also projects a small nearby **Mana-regeneration aura** to allies, weaker than the caster's personal benefit.

**Direction:** Mana aura affects Mana only and strongest applicable aura should win rather than stacking additively.

## 2026-09-28 — Magically Touched Mana-cost scaling

**Direction:** Spell Mana costs grow steeply with skill rank.

A representative offensive-spell target is roughly **10 Mana at rank 1 -> ~100 Mana at rank 6**, with a non-linear curve allowed.

## 2026-09-28 — Tier-1 physical/magical visual contrast

**Decision:** Physically Blessed becomes modestly larger/stronger-looking than Novice.

**Decision:** Magically Touched becomes modestly smaller/frailer-looking than Novice.

**Decision:** Tier 1 gets no dramatic magical glow effects or highly specialized class silhouettes; flashy identity comes later.


## 2026-09-28 — Reusable quest objective framework

**Decision:** Ordinary quests should be composed from reusable semantic objective types such as Kill, Interact, ReachArea, Collect, TalkTo, UseItem and CompleteDungeon, with AND/OR composition.

Custom scripts remain an escape hatch for genuinely unusual quests.

## 2026-09-28 — Quest persistence scopes

**Decision:** Quest/event content can declare different scopes:
- Echo-once;
- session-instance once;
- timeline/class-save once;
- repeatable.

Ordinary story/side quests normally use timeline scope unless authored otherwise.

## 2026-09-28 — Configurable event / kill credit

**Decision:** There is no universal kill-credit rule.

Content can choose policies including:
- global;
- nearby;
- threat/combat participation;
- last hit.

The authoritative death/event record supplies facts; individual quest/XP/achievement consumers decide credit.

## 2026-09-28 — NPC interaction actions

**Decision:** NPCs/world interactables expose semantic actions and can support an interaction menu.

**Decision:** A default action may execute directly when appropriate.

One NPC may therefore support combinations such as Talk / Quest / Shop / Bank / Respec / Advancement without duplicating the NPC.

## 2026-09-28 — Effect stacking policy

**Decision:** Effects define stacking/refresh behavior individually.

Supported policy families include unique-per-source, refresh, replace-stronger, capped stacks, independent instances and strongest-wins.

**Direction:** unique-per-source is a common/default pattern, not a universal restriction.

## 2026-09-28 — Dispel hierarchy and effect tags

**Decision:** The main dispel hierarchy is Weak Dispel / Strong Dispel / Undispellable.

**Decision:** Semantic effect tags such as Poison, Curse, Disease, Bleed, Magic and CC are still supported for situational cleanses, consumables and specialized classes.

## 2026-09-28 — Tag-driven equipment eligibility

**Decision:** Equipment eligibility uses stable semantic tags/requirements for scalability.

**Decision:** Exact class IDs remain valid requirements for intentionally niche/class-specific gear.

## 2026-09-28 — Composable abilities with custom escape hatch

**Decision:** Abilities should primarily compose generic targeting, resource, cooldown, damage/heal, elemental, status, movement, summon, threat and resource-generation primitives.

**Decision:** Custom code remains available for genuinely unusual class mechanics.

## 2026-09-28 — Cooldown philosophy

**Decision:** No universal GCD by default.

**Decision:** Framework supports arbitrary shared cooldown groups and optional class-specific/GCD-like groups.

**Decision:** Exceptional mechanics may share a cooldown across different players.

## 2026-09-28 — Basic attacks use common combat hooks

**Decision:** Basic attacks use the common combat-action/effect pipeline where practical.

This must support later mechanics such as generating Combo Points/resources from auto-attacks and spending them on skills.


## 2026-09-28 — Multiple simultaneous class resources

**Decision:** A class may use multiple resources at once.

Examples include Energy + Combo Points or Mana + Souls.

Resources are independently modeled rather than forced through one universal resource type.

## 2026-09-28 — Resource lifecycle policies

**Decision:** Each resource defines its own persistence/reset/decay/regeneration behavior.

Resources may persist, reset on combat/death, decay, regenerate, or be action-generated according to content.

## 2026-09-28 — Target-bound and owner-bound resources

**Decision:** Framework supports both target-bound and player/owner-bound Combo-Point-style resources.

## 2026-09-28 — Composite ability costs

**Decision:** One ability may consume multiple resource types and/or HP/items/charges simultaneously.

## 2026-09-28 — Ability charge recharge models

**Decision:** Charged abilities support:
- independent per-charge recharge;
- sequential recharge;
- one cooldown restoring all charges.

## 2026-09-28 — Casting/channel policy

**Decision:** Movement interruption, damage interruption, movement-while-casting, cast/channel type and related behavior are defined per ability.

**Decision:** Spell pushback is supported but is not a universal default.

## 2026-09-28 — Broad semantic trigger framework

**Decision:** Combat/gameplay supports a broad extensible trigger vocabulary for classes/items/effects, including hit, crit, damage, healing, resource, effect, kill/death, cast, thorns and custom authored triggers.

## 2026-09-28 — Proc recursion safety with intentional escape hatch

**Decision:** Generated events carry origin/context and accidental proc recursion is prevented by default.

**Decision:** Controlled recursion is explicitly allowed for future classes/items when authored with bounded rules.

## 2026-09-28 — Snapshot and dynamic periodic effects

**Decision:** DoTs/HoTs may either snapshot source state on application or dynamically recalculate on ticks.

## 2026-09-28 — Summon source + owner attribution

**Decision:** Summon events retain both immediate summon source and owning actor/player.

Threat, quest credit, procs and logs can choose the appropriate attribution.

## 2026-09-28 — Generic aura emitter model

**Decision:** Auras are reusable effect emitters defined by source, range/shape, target filters and emitted effect.

Existing effect stacking policies resolve overlapping aura contributions.


## 2026-09-28 — Handcrafted gear is the primary loot identity

**Decision:** Handcrafted equipment should make up the majority of meaningful gear progression, especially in later/endgame content.

Randomized gear remains supported but should appear through deliberately authored sources/pools rather than universal random drops.

## 2026-09-28 — Gear-score / stat-budget generation

**Decision:** Item level and rarity contribute to a DiceBound-inspired point/stat budget.

Stats, Intrinsics, affixes and numeric bonuses consume weighted portions of that budget.

Exact budget formulas remain balance work.

## 2026-09-28 — Randomizer as designer authoring tool

**Decision:** The randomized-item generator is also an internal content-authoring tool.

Designers can request/reroll constrained candidates and freeze a good roll into a stable handcrafted item, then manually edit it.

## 2026-09-28 — Affix pool/filter architecture

**Decision:** Affixes can be filtered by slot, family, tier/item level, rarity, element/theme, class tags, role and drop-source/content tags.

**Decision:** Weighted affix pools are supported; exact use/weights are content-specific.

**Decision:** Roll scaling uses a hybrid formula-driven baseline plus authored overrides.

## 2026-09-28 — Rarity can change item structure

**Decision:** Rarity is allowed to unlock structural item features, not merely bigger numbers.

Possible features include extra Intrinsics, sockets, special effect slots or unusual affix categories.

## 2026-09-28 — Handcrafted item framework

**Decision:** Handcrafted items use the shared item/stat/effect framework but have deliberately fixed authored definitions.

They may explicitly bend normal procedural generation constraints when the design calls for it.

## 2026-09-28 — Sockets and runeword-like system support

**Decision:** Equipment architecture supports sockets/socketables.

**Direction:** Leave room for authored runeword-like combinations/sequences using the normal item/effect/proc framework.

Exact runeword rules remain open.

## 2026-09-28 — Item sets

**Decision:** Support item sets and authored threshold bonuses such as 2/3/4-piece effects.

## 2026-09-28 — Gear procs reuse combat trigger framework

**Decision:** Equipment effects/procs use the generic combat trigger/effect architecture rather than a separate item-proc engine.

## 2026-09-28 — Equipment stat requirements

**Decision:** Items may require primary-stat thresholds in addition to class/tag/tier/family requirements.

Stat requirements are supported but need not be common.

## 2026-09-28 — Equipment swapping context

**Decision:** Equipment can be swapped during overworld combat.

**Decision:** Equipment swapping is disabled during active dungeon/raid runs.

## 2026-09-28 — No normal unidentified-item loop

**Decision:** Equipment drops identified.

Identification is not a normal loot-management mechanic.


## 2026-09-28 — Item level belongs to content source

**Decision:** Item level is defined by the authored drop/source/content, not dynamically scaled to the player's current level by default.

Old content therefore retains its progression identity.

## 2026-09-28 — Gear budget is hidden balancing/debug data

**Decision:** Gear-score/stat-budget totals are not player-facing item-quality ratings.

Players judge actual stats/effects/build fit.

**Decision:** Debug/designer tooling exposes expected budget, actual budget and over/under-budget deltas.

**Decision:** Handcrafted items may intentionally exceed ordinary budget expectations.

## 2026-09-28 — Runewords work on valuable existing gear

**Decision:** Runeword-like effects can be created on magical/rare/epic/etc. socketed items.

They do not require plain/non-magical bases.

**Decision:** Rune/socketable order matters.

**Decision:** The base item remains the same item and keeps its existing stats/affixes; the runeword adds an authored effect/package.

## 2026-09-28 — Destructive socket services

**Decision:** Socket removal/recovery is NPC/service-based and costs resources.

Framework supports:
- preserve item, destroy removed socketables;
- preserve/recover socketables, destroy the base item.

## 2026-09-28 — Duplicate named items

**Decision:** Multiple copies of the same named item may be owned.

**Decision:** Unique Equipped is optional item data for multi-slot/dual-wield edge cases, not a universal restriction.

## 2026-09-28 — No general salvage system

**Decision:** Unwanted equipment is sold for gold rather than salvaged/disenchanted into materials.

Materials may instead be purchasable for gold or obtained through authored content if needed later.

**Decision:** Named gear does not automatically yield special content-specific salvage materials.


## 2026-09-28 — Dungeon staging room

**Decision:** First player entering a dungeon begins a shared **60-second staging-room countdown**.

Other eligible party members entering during that minute join the same waiting room.

**Decision:** The staging room includes a test dummy.

**Decision:** The actual run, roster lock and equipment lock begin when the countdown expires.

## 2026-09-28 — Dungeon death / self-respawn policy

**Decision:** A dead dungeon player can be healer-resurrected until they choose self-respawn.

**Decision:** Self-respawn destination is dungeon-specific.

Supported cases include a dungeon-start/checkpoint spawn or the player's last registered resurrection point.

## 2026-09-28 — Dungeon full wipe and abandonment

**Decision:** Default full wipe resets the entire dungeon state: trash, bosses and run-local encounters.

Explicit future exceptions are allowed.

**Decision:** If the party abandons/leaves the active run, the run resets/destroys rather than remaining parked.

## 2026-09-28 — Dungeon boss and trash reset rules

**Decision:** Bosses reset fully after a failed/disengaged attempt.

**Decision:** Dungeon trash normally has no timed respawn during an active attempt; it returns on a run reset.

## 2026-09-28 — Dungeon loot-room completion

**Decision:** Successful completion sends each participant to a private loot room.

**Decision:** Reward choice must be made before leaving.

**Decision:** A player disconnected in the loot room can reconnect back into it within a limited reconnect window. Exact duration remains open.

## 2026-09-28 — Dungeon equipment and class lock

**Decision:** Equipment remains changeable during the staging countdown and becomes locked when the active run starts.

**Decision:** Ordinary consumables remain usable/manageable during the run.

**Decision:** Class switching is unavailable during an active dungeon/raid run.

## 2026-09-28 — Dungeon semantic events

**Decision:** Dungeon/encounter milestones emit generic semantic events usable by quests, achievements, class requirements and secret unlocks.

Examples include run start, boss defeat, completion, no-death completion and secret-room discovery.


## 2026-09-28 — Aggressive autosave

**Decision:** Durable progression autosaves on meaningful state changes; no required traditional manual Save Game flow.

## 2026-09-28 — Loading spawns at resurrection point

**Decision:** Exact quit position is not the authoritative saved spawn.

Loading a class places it at its latest registered resurrection point, with Cornberg fallback when required.

## 2026-09-28 — Active dungeon runs are not persisted

**Decision:** Quitting an active dungeon abandons the run.

**Decision:** Persistent XP and gold already earned during the run remain saved.

## 2026-09-28 — Echo vs manifestation save ownership

**Decision:** Echo-wide state and per-class manifestation state have explicit ownership boundaries even if serialized together.

Transient session/dungeon state is separate.

## 2026-09-28 — Atomic advancement and bank operations

**Decision:** Advancement/class creation is transactional.

**Decision:** Inventory/bank ownership transfers are transactional.

Crashes must not produce half-created classes, duplicated items or deleted items.

## 2026-09-28 — Versioned saves and migrations

**Decision:** Persistent references use stable IDs and explicit save/schema versions.

**Decision:** Runtime changes use explicit migrations rather than assuming old saves match current data.

## 2026-09-28 — Preserve unresolved save records

**Decision:** Missing/unknown class/item/content references are preserved inertly where practical rather than silently deleted.

## 2026-09-28 — Rotating local save backups

**Decision:** Maintain automatic rotating backups of recent successful save revisions.

## 2026-09-28 — Cloud/user architecture

**Decision:** Prepare provider-neutral user/profile and cloud-save abstractions.

Initial PC direction includes Steam Cloud.

Gameplay systems must not depend directly on a platform-specific user ID or cloud API.

## 2026-09-28 — No anti-save-scumming requirement

**Decision:** DiceFree does not need anti-save-scumming/competitive save protection or leaderboard integrity systems.

Persistence protects against accidental loss/corruption, not deliberate local rollback.

## 2026-09-28 — Cooperative world progress persists to eligible guests

**Decision:** Host state controls live world presentation, but eligible guests can permanently record quest/world outcomes they actually participate in.

**Decision:** Merely joining after an event already happened does not copy that outcome into the guest's timeline.

**Direction:** Event definitions can choose persistence recipients; ordinary co-op progression should normally credit eligible participants.


## 2026-09-28 — Bob's house is canon now

**Decision:** The repeated multiplayer persistence example becomes a real future sidequest/event: **Bob's house burns down**.

Bob and his house will exist somewhere in World 1 content.

Exact quest cause/reward/consequences remain intentionally open.

The event is separate from Cornberg's locked abandoned-house mystery unless later explicitly connected.

## 2026-09-28 — Multiple Echo profiles, hidden from normal flow

**Decision:** One user may own multiple separate Echo profiles.

**Decision:** Normal start/continue UX centers the current primary Echo.

Creating/switching Echoes is a deliberate settings/profile-management action so new players do not accidentally create new Echoes.

## 2026-09-28 — Internal user and Echo UUIDs

**Decision:** DiceFree uses stable internal user IDs and Echo IDs.

Steam/platform identity links to the internal user rather than becoming the gameplay save primary key.

## 2026-09-28 — No separate DiceFree login requirement initially

**Decision:** Steam PC release does not require a separate DiceFree email/password account.

Identity architecture can attach additional providers later.

## 2026-09-28 — Cloud conflict policy

**Decision:** Divergent local/cloud Echo revisions are not automatically field-merged.

Present revision information, let the user choose, and preserve backups where practical.

## 2026-09-28 — Echo is the logical cloud revision unit

**Decision:** An Echo profile syncs/version-controls as one coherent cloud revision even though its save package is internally chunked.

## 2026-09-28 — Database is metadata/service-first

**Decision:** Initial/future online database use should focus on users, linked identities, Echo IDs, cloud revision metadata and service metadata.

Ordinary RPG state remains in versioned Echo save packages rather than requiring every gameplay field in an online database.

## 2026-09-28 — Player save remains progression authority

**Decision:** No server-authoritative progression requirement is planned for ordinary play.

There are no leaderboards/economy/PvP integrity requirements driving such a system.

## 2026-09-28 — Offline and LAN support

**Decision:** Core game works offline.

**Decision:** Architecture should support local-LAN multiplayer without requiring internet/cloud services.

## 2026-09-28 — Recoverable Echo deletion

**Direction:** Whole-Echo deletion uses explicit confirmation plus a limited recoverable local/cloud deletion period before permanent purge.

Exact retention period remains open.

## 2026-09-28 — Cross-platform identity readiness

**Decision:** Internal identity/save architecture allows another platform/provider to be linked later.

## 2026-09-28 — Internally chunked save package

**Decision:** Each Echo remains one logical save/cloud revision but uses internal sections/chunks for manifestations, shared state, bank, migrations and diagnostics.

## 2026-09-28 — Developer Profile Inspector

**Decision:** Provide debug/developer-only profile inspection tooling for save versions, revisions, manifestations, backups, migrations, unresolved records, transactions and hidden item-budget/provenance data.


## 2026-09-28 — Host-authoritative simulation

**Decision:** Host is authoritative for live world/combat/session state, including guest combat state while connected.

Clients submit intentions; authoritative outcomes drive persistence.

**Decision:** Solo, LAN and online modes should reuse common gameplay command/event paths where practical.

## 2026-09-28 — Host migration is required

**Decision:** Host migration must exist for overworld sessions.

The session should continue under another participant when the original host disconnects/crashes.

**Direction:** Preserve live session state whenever recoverable and never discard already-earned durable progression simply because host migration failed.

## 2026-09-28 — Dungeon host migration

**Decision:** Host migration also applies to active dungeon/raid runs.

A long run should not automatically fail because the original host disconnects.

## 2026-09-28 — Disconnect grace and slot reservation

**Decision:** Temporarily disconnected players remain represented in-world during a reconnect grace period and may still be harmed/killed.

**Decision:** Reconnecting within the grace period resumes the same live actor.

**Decision:** Dungeon roster slots remain reserved and cannot be replaced during that window.

## 2026-09-28 — Pause rules

**Decision:** Solo supports true pause.

**Decision:** Multiplayer supports vote pause.

Exact vote rules remain open.

## 2026-09-28 — Public lobbies, LAN and direct connection

**Decision:** DiceFree supports truly public stranger lobbies in addition to friends/invite-only games.

**Decision:** Support LAN discovery and direct connection-style joining.

LAN/private networking must not require internet/cloud availability and should work with normal local/private/virtual-LAN setups.

## 2026-09-28 — Vote kick

**Decision:** Multiplayer supports vote kick.

Exact thresholds, host-target behavior and dungeon restrictions remain open.

## 2026-09-28 — No ordinary equipment trading

**Decision:** No general player-to-player equipment trading after pickup.

FFA overworld drops can still be socially assigned before pickup.

**Direction:** Framework supports explicitly tradeable item categories; food/potions are candidate tradeable consumables.

**Decision:** No default direct gold transfer/unrestricted player economy.


## 2026-09-28 — Enemy archetypes and authored variants

**Decision:** Enemy definitions support reusable base archetypes plus authored variants/overrides.

Keep composition readable; avoid deep opaque inheritance chains.

## 2026-09-28 — Fixed enemy levels

**Decision:** Enemies use fixed authored levels by default.

There is no universal player-level scaling.

## 2026-09-28 — Enemy stat flexibility

**Decision:** Enemies may use the five primary attributes where useful and/or direct monster stats.

Both feed the common combat pipeline.

## 2026-09-28 — Semantic enemy roles/tags

**Decision:** Enemy role/behavior metadata uses extensible semantic tags such as Melee, Ranged, Caster, Healer, Summoner, Ambusher, Elite and Boss.

Tags do not themselves hard-code behavior.

## 2026-09-28 — Scalable AI decision rules

**Decision:** AI supports extremely simple rules such as nearest-target/basic attack as well as weighted conditional decision logic using nested AND/OR/NOT predicates.

Complex future enemies/bosses may have very large authored condition sets.

## 2026-09-28 — Threat and alternate targeting

**Decision:** Highest threat is a normal target policy, not a universal restriction.

Enemies may target by nearest/farthest/lowest HP/random/role/effect/fixation or other authored criteria.

## 2026-09-28 — Authored leash/reset rules

**Decision:** Overworld enemies define home/leash/reset behavior per archetype/enemy.

Some overworld monsters can use special nonstandard leash/chase rules.

## 2026-09-28 — Enemy packs / patrol groups

**Decision:** Support authored pack definitions for composition, formation, patrol, linked aggro and shared reset behavior.

## 2026-09-28 — Boss phase condition framework

**Decision:** Boss/encounter phase transitions can use generic predicates such as HP, time, add deaths, world objects, player state and nested logical conditions.

## 2026-09-28 — Encounter definitions own fight-wide mechanics

**Decision:** Arena hazards, doors, add waves, timers, dialogue, encounter variables and fight completion/reset belong to encounter definitions rather than being crammed into boss actor code.

## 2026-09-28 — Difficulty modes can change mechanics

**Decision:** Harder difficulty modes can add/change encounter mechanics, phases, enemies, AI, hazards, timers and rewards rather than only scaling HP/damage.

## 2026-09-28 — Reusable enemy modifiers

**Decision:** Support reusable enemy modifiers that can alter stats, abilities, AI, effects, presentation and rewards.

This does not imply ubiquitous random elite affixes.

## 2026-09-28 — Encounter reset with explicit exceptions

**Decision:** Default reset restores all authored encounter-local state.

Specific encounters may explicitly preserve selected state across attempts.

## 2026-09-28 — Encounter debug harness

**Decision:** Provide development tooling to spawn/test encounters, force phases/abilities/HP, inspect AI/threat/variables and simulate relevant party-count conditions.


## 2026-09-29 — Owned units do not consume party slots

**Decision:** Summons/pets/companions do not consume the normal four player slots.

## 2026-09-29 — No universal summon cap

**Decision:** DiceFree has no game-wide gameplay summon cap.

Each class/ability defines its own constraints through active-count limits, lifetime or other authored rules.

## 2026-09-29 — Autonomous, controllable and hybrid summons

**Decision:** Owned units support autonomous AI, direct RTS-style control and hybrid stance/order control.

## 2026-09-29 — Ownership and control can differ

**Decision:** Owner and current controller are distinct.

Control can be transferred temporarily without necessarily changing ownership.

## 2026-09-29 — Summon selection and command UI

**Decision:** Selecting a directly controllable owned unit uses the shared WC3-style command grid.

**Direction:** F1 returns to hero; F2+ addresses/cycles controllable owned units, subject to final input tuning.

## 2026-09-29 — Generic stances and orders

**Decision:** Framework supports Aggressive / Defensive / Passive / Hold Position stances plus generic Move / Attack / Stop / Hold / Follow / Attack-Move orders where relevant.

Units expose only applicable controls.

## 2026-09-29 — Flexible summon stat inheritance

**Decision:** Owned units can use authored, snapshot, dynamic or mixed owner-stat formulas.

## 2026-09-29 — Summons share owner level

**Decision:** Summons/companions do not have independent character levels or XP tracks.

They use the owner's level for level-based scaling.

Persistent companions may still have other non-level progression systems later.

## 2026-09-29 — Summon resources

**Decision:** Owned units may have their own resources, share owner resources or have none.

## 2026-09-29 — Summon threat transfer support

**Decision:** Owned units have independent threat entries by default.

**Decision:** Content may transfer/redirect threat between owner and summon for authored mechanics such as tank pets.

## 2026-09-29 — Summon death is not player death

**Decision:** Normal summon death/despawn/revival is class/ability behavior and does not use player resurrection-stone rules by default.

## 2026-09-29 — Summon persistence scopes

**Decision:** Support temporary, combat-persistent, session-persistent and manifestation-persistent owned units.

## 2026-09-29 — Persistent companion identity

**Decision:** Disposable units can use runtime identity; persistent companions use stable persistent instance IDs.

## 2026-09-29 — Companion equipment and optional inventory

**Decision:** Framework permits equipment-capable companions using normal item/stat systems.

**Decision:** No generic summon inventory is required, but authored companions may support one.

## 2026-09-29 — Summon ally targeting

**Decision:** Summons/companions are valid allies by default for heals/buffs, with semantic tag filters allowing abilities to include/exclude them.

## 2026-09-29 — Summon credit attribution

**Decision:** Normal XP/quest credit may resolve summon actions to the owner, while source-sensitive requirements can still inspect the summon itself.

## 2026-09-29 — No special summon collision rule yet

**Decision:** Summons use normal physical actor collision/pathing by default.

Do not add special soft collision until real testing demonstrates a need.

## 2026-09-29 — Networked summon authority

**Decision:** Host owns authoritative summon AI/live state; clients send commands for units they control.

Summon state participates in host migration/reconnect.


## 2026-09-29 — Load at full HP

**Decision:** Loading a manifestation at its registered resurrection point restores it to full HP regardless of saved living/dead HP state.

## 2026-09-29 — Cooldowns reset on load

**Decision:** Ordinary ability and consumable cooldowns reset on load.

Explicit persistent long-duration cooldowns may exist later but must opt in.

## 2026-09-29 — Buffs/debuffs reset on load

**Decision:** Ordinary timed buffs/debuffs/effects reset on load.

Persistent world/event buffs should be recreated from persistent world state/aura sources rather than by serializing transient effect instances.

## 2026-09-29 — Resources reset by default on load

**Decision:** Resources reset on load by default.

**Decision:** Resource definitions can explicitly opt into persistence for rare/difficult-to-acquire resource mechanics.

## 2026-09-29 — Combat state never resumes across load

**Decision:** Threat, targets, aggro, casts, projectiles, enemy combat state and ordinary temporary summons are session state and reset on load.

## 2026-09-29 — Persistent companions reload cleanly

**Decision:** Manifestation-persistent companions reload with durable identity/equipment/traits, but their transient combat state resets.

## 2026-09-29 — Timer clock domains

**Decision:** Timed content can explicitly use session/gameplay time, played time or wall-clock time.

DiceFree is not designed around weekly/live-service events, but wall-clock timing remains supported for future authored exceptions.

## 2026-09-29 — Solo quit can escape danger

**Decision:** No logout timer is required in solo.

Loading at the resurrection point/full HP means quit/reload can function as an escape; this is acceptable.

## 2026-09-29 — Multiplayer Leave Session uses vulnerable grace

**Direction:** Intentional multiplayer leave during combat should use the same/equivalent vulnerable grace semantics as disconnect rather than instant disappearance.


## 2026-09-29 — Default Vitality coefficients

**Decision:** Default Vitality scaling is **+15 maximum HP per VIT** and **+0.1 HP/sec regeneration per VIT**.

Classes may explicitly override either coefficient.

Items/passives/effects may further modify the coefficients or add independent HP/regeneration.

## 2026-09-29 — Early Cornberg economy anchors

**Working targets:** Crop Slimes drop roughly **1-3 gold**.

**Working Cornberg fixed-vendor examples:**
- +1 damage sword: 100 gold;
- +2 damage sword: 250 gold;
- +3 damage sword: 450 gold;
- basic low-stat defensive items also exist.

**Decision:** Vendor inventories are fixed/authored.

These values anchor early prototyping rather than defining the whole economy.

## 2026-09-29 — Explicit-only blacksmith upgrades

**Decision:** Blacksmith/item upgrading applies only to explicitly authored upgradeable items/recipes.

**Direction:** Item upgrading is primarily a midgame-to-endgame system, not a universal early item-level upgrade mechanic.

## 2026-09-29 — Optional mentor scaling

**Decision:** Veteran/new-player scaling should use an optional mentor/assist mode rather than universal mandatory down-scaling.

Exact formulas and rewards remain open.

## 2026-09-29 — Controller support with mouse reference design

**Decision:** Controller/gamepad support is a target.

**Decision:** Classic mouse click-to-move remains DiceFree's primary/reference control and balance target.

## 2026-09-29 — Prototype cube-world traversal after Cornberg

**Decision:** After the Cornberg vertical-slice PoC is sufficiently proven, build a focused cube-world traversal PoC before expanding full world production.

The PoC determines whether seamless 90-degree cube-face traversal is practical/fun or should be represented with authored/streamed transitions.


## 2026-09-29 — Default Vitality coefficients

**Decision:** Default Vitality scaling is **+15 maximum HP per VIT** and **+0.1 HP/sec regeneration per VIT**.

Classes may explicitly override either coefficient.

Items/passives/effects may further modify the coefficients or add independent HP/regeneration.

## 2026-09-29 — Early Cornberg economy anchors

**Working targets:** Crop Slimes drop roughly **1-3 gold**.

**Working Cornberg fixed-vendor examples:**
- +1 damage sword: 100 gold;
- +2 damage sword: 250 gold;
- +3 damage sword: 450 gold;
- basic low-stat defensive items also exist.

**Decision:** Vendor inventories are fixed/authored.

These values anchor early prototyping rather than defining the whole economy.

## 2026-09-29 — Explicit-only blacksmith upgrades

**Decision:** Blacksmith/item upgrading applies only to explicitly authored upgradeable items/recipes.

**Direction:** Item upgrading is primarily a midgame-to-endgame system, not a universal early item-level upgrade mechanic.

## 2026-09-29 — Optional mentor scaling

**Decision:** Veteran/new-player scaling should use an optional mentor/assist mode rather than universal mandatory down-scaling.

Exact formulas and rewards remain open.

## 2026-09-29 — Controller support with mouse reference design

**Decision:** Controller/gamepad support is a target.

**Decision:** Classic mouse click-to-move remains DiceFree's primary/reference control and balance target.

## 2026-09-29 — Prototype cube-world traversal after Cornberg

**Decision:** After the Cornberg vertical-slice PoC is sufficiently proven, build a focused cube-world traversal PoC before expanding full world production.

The PoC determines whether seamless 90-degree cube-face traversal is practical/fun or should be represented with authored/streamed transitions.


## 2026-09-29 — Default Strength to Physical Defense coefficient

**Decision:** Default Strength secondary scaling is **+0.2 Physical Defense per STR**.

This is a default coefficient, not a hard universal constant.

Classes/items/passives/effects may explicitly override or modify it.


## 2026-09-29 — Default Intelligence to Magical Defense coefficient

**Decision:** Default Intelligence secondary scaling is **+0.2 Magical Defense per INT**.

This mirrors Strength -> Physical Defense.

This is a default coefficient, not a hard universal constant.

Classes/items/passives/effects may explicitly override or modify it.


## 2026-09-29 — Default Agility to Attack Speed coefficient

**Decision:** Default Agility secondary scaling is **+0.025% Attack Speed per AGI**.

This is intentionally a very small passive contribution.

Classes/items/passives/effects may explicitly override or modify it.

Movement Speed scaling from Agility remains open.


## 2026-09-29 — Default Agility to Movement Speed coefficient

**Decision:** Default Agility secondary scaling is **+0.01% Movement Speed per AGI**.

This is intentionally a very small passive contribution.

**Balance note:** this and the other current primary-attribute secondary coefficients are working/default tuning values, not immutable canon. Playtesting may change them while preserving the same stat-system architecture.


## 2026-09-29 — Default Spirit to Healing Done coefficient

**Decision:** Default Spirit secondary scaling is **+0.15% Healing Done per SPI**.

Healing Received scaling from Spirit remains a separate open decision.

**Balance note:** this is a current tuning default and may change through playtesting without changing the stat architecture.


## 2026-09-29 — Default Spirit to Healing Received coefficient

**Decision:** Default Spirit secondary scaling is **+0.075% Healing Received per SPI**.

This is intentionally **half** of Spirit's current Healing Done coefficient.

**Balance note:** this is a current tuning default and may change through playtesting without changing the stat architecture.


## 2026-09-29 — Physical/Magical Defense curve

**Decision:** Physical and Magical Defense use the same current balance-default diminishing-return curve.

```
q = (abs(Defense) / 300)^0.7
```

Positive Defense:
```
DamageTakenMultiplier = 1 / (1 + q)
```

Negative Defense is symmetrical vulnerability:
```
DamageTakenMultiplier = 1 + q / (1 + q)
```

Current anchors:
- +300 Defense = 50% mitigation;
- -300 Defense = 150% damage taken;
- extremely negative Defense approaches 200% damage taken from Defense alone.

**Balance note:** the 300 scale and 0.7 exponent are working tuning defaults and may change after playtesting.


## 2026-09-29 — Defense penetration order

**Decision:** Percentage Physical/Magical Defense penetration is applied before flat Defense penetration.

Example:
- 100 Defense;
- 20% penetration -> 80;
- 10 flat penetration -> 70 effective Defense.

This is the default combat-resolution order.


## 2026-09-29 — Defense penetration can create negative Defense

**Decision:** Defense penetration is not clamped at zero.

If penetration exceeds current Physical/Magical Defense, effective Defense becomes negative and uses the normal vulnerability curve.

Example:
- 5 Defense;
- 10 flat penetration;
- -5 effective Defense.


## 2026-09-29 — Defense reduction resolves before penetration

**Decision:** Physical/Magical Defense reduction/debuffs apply before attacker-specific penetration.

Default order:
1. Defense reduction;
2. percentage penetration;
3. flat penetration;
4. evaluate final Defense through the mitigation/vulnerability curve.

Example: 100 -> 80 after -20 reduction -> 64 after 20% penetration -> 54 after 10 flat penetration.


## 2026-09-29 — Percentage Defense reduction before flat reduction

**Decision:** Within the Defense-reduction layer, percentage reduction resolves before flat reduction.

Full default order:
1. percentage Defense reduction;
2. flat Defense reduction;
3. percentage Defense penetration;
4. flat Defense penetration;
5. final mitigation/vulnerability calculation.


## 2026-09-29 — Percentage Defense effects use original Defense

**Decision:** Percentage Defense reduction and percentage Defense penetration are calculated from the target's original positive Defense value for the resolution, not from the intermediate remainder.

Consequences:
- multiple percentage Defense reductions are additive against original Defense;
- example: 100 Defense with 50% reduction and 60% reduction becomes -10 before other modifiers;
- flat Defense reduction can push Defense below zero;
- percentage penetration also references original positive Defense;
- if original/base Defense is negative, percentage Defense reduction and percentage Defense penetration do not apply;
- negative base Defense is considered an unusual authoring edge case rather than a normal target state.

Default conceptual order:
1. original Defense;
2. percentage reduction(s) from original Defense;
3. flat reduction;
4. percentage penetration from original Defense;
5. flat penetration;
6. mitigation/vulnerability curve.


## 2026-09-29 — Defense reference, positive buffs, and >100% shred

**Decision:**
- percentage Defense reduction/penetration use underlying Defense before temporary positive Defense buffs as their reference;
- positive Defense buffs resolve percentage first, then flat;
- percentage Defense reduction and penetration may exceed 100%.

Example:
- underlying Defense 100;
- +20% Defense then +50 flat Defense => current Defense 170;
- enemy 20% reduction still subtracts 20, because the reference is the underlying 100;
- 120% penetration against underlying 100 subtracts 120 and can create negative effective Defense.


## 2026-09-29 — Additive Defense percentage stacking

**Decision:**
- multiple positive percentage Defense buffs stack additively;
- multiple percentage Defense reductions stack additively;
- multiple percentage Defense penetration sources stack additively.

Examples:
- +20% Defense and +30% Defense = +50% total;
- 50% and 60% Defense reduction = 110% total reduction;
- 20% and 15% penetration = 35% total penetration.

Reduction and penetration remain allowed above 100%.


## 2026-09-29 — Defense reference stability and fractional precision

**Decision:** Temporary negative Defense effects never rewrite the underlying Defense reference used by percentage Defense calculations.

They affect current/effective Defense only.

**Decision:** Defense calculations preserve fractional values internally. Intermediate values are not rounded for gameplay resolution; UI may round for display.

**Open:** Exact membership of the underlying Defense reference (for example whether gear/passive Defense is included alongside class base + attribute-derived Defense) remains intentionally undecided.


## 2026-09-29 — Crit is explicitly authorized per source

**Decision:** DiceFree has no universal baseline crit chance and no universal crit-damage multiplier.

- baseline crit chance is 0%;
- only an explicit skill/passive/item/effect can authorize a crit;
- the authorizing source defines its chance and multiplier/behavior;
- direct damage, DoTs, healing and other effect categories do not crit by default;
- any of those categories may crit when explicitly enabled.

Examples intentionally supported by the design:
- 50% chance for 2x damage;
- 12.5% chance for 8x damage.

**Open:** how multiple simultaneous crit-granting sources interact is not yet decided.


## 2026-09-29 — Multiple crit-rule resolution

**Decision:** Multiple applicable ordinary crit rules are rolled from highest multiplier to lowest multiplier.

- highest multiplier rolls first;
- if it succeeds, stop and use that crit result;
- if it fails, roll the next rule;
- rules are not merged into one combined crit chance.

**Exception model:** A very rare source may explicitly state that it stacks with or multiplies another crit result. This is never implicit/default behavior.

**Decision:** Explicit crit-rule modifiers are supported. A source may explicitly modify another eligible crit rule's chance, multiplier or permission rather than creating a new roll. Such interactions must be explicitly authored.


## 2026-09-29 — Crit modifier order, raw-packet placement, and chance cap

**Decision:** Explicit crit modifiers resolve before ordinary crit rules are sorted. Sorting uses the final modified multiplier.

**Decision:** Default crit multiplication applies to the raw damage packet before Defense and elemental resistance.

**Decision:** Ordinary crit chance is capped at 100%.

Values above 100% do not automatically create extra rolls, overflow conversion, or super-crits. Such mechanics require an explicit authored exception.

Rare sources may explicitly override the default crit placement in the damage pipeline.


## 2026-09-30 — Crit ties and multi-packet scope

**Decision:** Equal final crit multipliers use explicit authored priority when present; otherwise use stable deterministic source-ID order.

**Decision:** Crit resolution is action-wide by default. Multi-packet actions share one crit result across eligible raw packets before each packet resolves its own mitigation/resistance.

**Decision:** Default critical-hit signaling is one critical-hit event per action, not one per packet.

Per-packet crit resolution/events remain possible only when explicitly authored.


## 2026-09-30 — Crit modifier restraint and provenance

**Decision:** Crit-rule modifiers are supported but should be used sparingly.

When used, modifier math must be explicit. Distinguish operations such as:
- +percentage points vs percentage-of-existing chance;
- +multiplier amount vs percentage-of-existing multiplier.

Prefer simple authored crit rules when possible rather than unnecessary modifier layers.

**Decision:** A critical action preserves both:
- action/source provenance;
- winning crit-rule/source provenance.

These identities remain separate for future trigger logic.


## 2026-09-30 — Hit-before-crit, critical result vs damage, and inheritance

**Decision:** Hit/miss resolution occurs before crit resolution. Misses do not consume crit rolls.

**Decision:** A successful critical result is distinct from actual critical HP damage dealt. A crit can occur even if later mitigation/prevention results in zero HP damage.

**Decision:** Secondary/child effects do not inherit parent crit results by default. Inheritance requires an explicit authored exception.


## 2026-09-30 — Elemental penetration in healing

**Decision:** Elemental penetration may affect elemental healing when the authored effect explicitly says it does.

For healing it acts opposite to damage penetration:
- damage penetration lowers effective matching resistance;
- healing penetration raises effective matching resistance.

Example:
- target has +50% Fire resistance;
- healer has +10 percentage points Fire penetration that applies to healing;
- Fire heal resolves using +60% effective Fire resistance.

**Decision:** Real resistance debuffs change target resistance itself and therefore affect both elemental damage and elemental healing.

**Reaffirmed:** Negative elemental resistance is uncapped. At -100% matching resistance elemental healing becomes zero; below -100% it inverts into damage.

**Design rule:** Penetration effects must state clearly which contexts they affect. Avoid ambiguous generic wording.


## 2026-09-30 — Resistance overcap and additive stacking

**Decision:** Elemental resistance stores an uncapped raw value; the normal cap applies only to effective positive resistance during resolution.

**Decision:** Resistance debuffs/penetration operate on raw uncapped resistance before the cap.

Example:
- 110% raw Fire resistance;
- -20 percentage-point debuff => 90% raw;
- 75% normal cap => still 75% effective.

**Decision:** Ordinary resistance bonuses/debuffs stack additively.

**Reaffirmed:** Negative elemental resistance remains uncapped.


## 2026-09-30 — Resistance percentage semantics and >100% inversion

**Decision:** Elemental resistance and elemental resistance penetration are percentage mechanics and must always be written with explicit `%` notation.

**Decision:** Elemental penetration values represent percentage points removed from matching resistance, not multiplicative reduction of the current resistance.

**Decision:** Multiple elemental penetration sources stack additively.

**Decision:** Explicit resistance-cap increases may raise the normal 75% cap and the framework permits caps above 100%.

**Decision:** If effective matching elemental resistance exceeds 100%, matching elemental damage becomes healing.

Example:
- 105% Fire Resistance => matching Fire damage heals for 5% of the damage that would have resolved at 0% Fire Resistance.

This is intentionally supported as a rare/emergent interaction rather than a normal balance target.


## 2026-09-30 — Damage-inversion healing semantics

**Decision:** HP restoration caused by >100% effective elemental resistance is not modified by Healing Received.

**Decision:** The target still counts as being hit by the originating elemental action, but does not count as taking damage.

**Decision:** Resistance-inversion HP restoration is distinct from ordinary healing for trigger purposes unless an effect explicitly includes it.

**Decision:** Damage shields do not consume capacity when the post-resistance result is healing rather than positive damage.


## 2026-09-30 — Mixed packet damage/healing outcomes

**Decision:** Damage packets resolve independently.

A single action may both deal HP damage and restore HP through resistance inversion when different packets resolve differently.

**Decision:** Preserve actual HP damage and resistance-inversion HP restoration as separate totals/outcomes. Do not collapse them into a single net damage value for trigger logic.

If any packet deals actual HP damage, the target counts as having taken damage from the action.

**Decision:** Resistance-inversion HP restoration is capped by missing HP. Excess is discarded by default.


## 2026-09-30 — One element per packet and resistance-cap modifiers

**Decision:** One damage packet may have at most one element. Multi-element actions use separate packets.

**Decision:** Resistance-cap modifiers may be global or element-specific and stack additively in percentage points by default.

**Decision:** Resistance-cap reductions are supported as a separate mechanic from raw resistance reduction.

Example:
- 110% raw Fire Resistance;
- 55% effective Fire Resistance cap;
- effective Fire Resistance = 55%.

Cap changes do not alter the stored/raw resistance value.


## 2026-09-30 — Resistance-cap floor, healing cap, and no accidental resurrection

**Decision:** Effective elemental Resistance Cap has a default floor of 0%.

Cap reduction alone cannot create negative resistance/vulnerability.

**Decision:** Elemental healing uses the same effective elemental Resistance Cap as damage after context-specific resistance/penetration math.

**Decision:** Resistance-inversion HP restoration cannot resurrect dead targets by default. Resurrection requires an explicit resurrection/death-prevention mechanic.


## 2026-09-30 — Default hit and miss-chance semantics

**Decision:** Actions hit by default. There is no universal baseline miss chance.

**Decision:** Miss resolution is action-wide by default. If an action misses, its packets miss and crit does not roll.

**Decision:** Explicit miss chance/modifiers are percentages, use additive percentage points by default, and clamp to 0-100%.

Per-packet/projectile miss rolls require an explicit authored exception.

## 2026-10-01 — Architecture contract, DCC source layout, and build distribution

**Decision:** DiceFree now has an explicit architecture contract in `docs/ARCHITECTURE.md`. Significant architectural changes use ADRs under `docs/adr/`.

**Decision:** The current lack of first-party Unity Assembly Definitions is accepted Cornberg PoC debt, but not the production target. Before substantial post-Cornberg expansion, migrate incrementally to explicit domain/runtime, Editor and test assemblies with one-way dependency rules.

**Decision:** 3D art direction is stylized, moderately exaggerated fantasy readability, in the general readability space of Warcraft III / Magicka rather than photorealism. Equipment should be slightly oversized for isometric readability while remaining recognizably proportioned.

**Decision:** Custom 3D production begins with exactly one original one-handed sword. After that pipeline is proven, intended validation order is shield -> shoes -> hat.

**Decision:** Editable native DCC source files stay in the same Git repository for now, under repository-root `SourceArt/` outside Unity `Assets/`. Unity-ready exports live under `Assets/_DiceFree/Art/`. Both native/exported binary art use Git LFS.

**Decision:** Do not create a "runtime-only" Git branch. Branches are development history, not player packaging. Playable builds are distributed as build artifacts/releases. A separate art-source repository is only reconsidered if measured size, permissions, LFS cost or CI checkout cost justify it.

See:
- `docs/ARCHITECTURE.md`
- `docs/ART_PIPELINE.md`
- ADR-0001
- ADR-0002
- ADR-0003

## 2026-10-01 — Equipment visual progression and non-human class forms

**Decision:** The first custom sword should be humble Cornberg equipment: a chunky, practical piece of metal rather than ornate hero gear.

**Decision:** Equipment rarity/progression may be reflected through geometry, materials and VFX. Higher-end/endgame gear can use glow/emissive effects, particles, trails and similar presentation where appropriate, while preserving combat readability.

**Decision:** DiceFree classes/Ways are not restricted to human forms or one universal human rig. Advancement may transform the Echo's species/body form.

Settled examples:
- Tier-2 Ranger is a **Wood Elf**.
- Tier-2 Wizard is a **High Elf**.

Tentative examples, not locked:
- Berserker may be an Orc.
- A Tier-4 tank may be a Centaur.

**Decision:** Equipped weapons do not need baseline sheathing. Always-held presentation is acceptable. Do not build sheath sockets/animations/state until a future feature explicitly needs them.

**Decision:** Architecture hardening issue #36 should happen **immediately**, before MSQ5 and before further growth in abilities/resources/classes.

## 2026-10-01 — Fixed class body presentations and multi-slot equipment

**Decision:** Each class/Way has one authored body/sex presentation. A class may be visibly male, visibly female or androgynous. DiceFree does not require male/female variants of every class.

**Decision:** As class lineages advance and specialize, their models should generally become increasingly distinct in the direction of the class fantasy.

**Decision:** Equipment appearance does not automatically resize/remesh/morph itself across incompatible body forms. Gear remains visually authored at its intended proportions; class/form compatibility and restrictions determine who can equip/use it. Descendant access is controlled through ordinary compatibility/inheritance data.

**Decision:** All playable forms retain the same semantic eleven equipment slots, including radical body plans.

**Decision:** Equipment may occupy multiple slots. A two-handed weapon occupies **Main Hand + Off Hand**. Implement occupied slots as data rather than scattered two-handed special cases.

**Decision:** Architecture hardening #36 is authorized as a substantial controlled refactor now, provided behavior remains equivalent, the migration is incremental/inspectable, and validation stays green.

## 2026-10-01 — Descendant transformations, rig families, armor replacement and form hitboxes

**Decision:** A descendant Way may fully transform species/body/form again. A parent class becoming a Wood Elf does not force all descendants to remain that form.

**Decision:** Visible wearable armor normally replaces the class form's baseline presentation for the occupied slot rather than universally layering on top.

**Decision:** Similar forms should share rig/animation families where practical. Human/High-Elf/Wood-Elf-like bodies may share a humanoid family; materially different forms may use different families; radically different/non-biped forms such as a possible Centaur use an appropriate separate rig.

**Decision:** Equipping a multi-slot item automatically unequips conflicting gear to carried inventory when capacity permits. If displaced gear cannot be retained safely, the equip fails atomically rather than destroying/dropping items.

**Decision:** Class/body size is not presentation-only. Different class forms may use different authored gameplay hitboxes/physical/navigation footprints. Do not derive these automatically from renderer bounds; they are explicit form data.

## 2026-10-01 — Reach, navigation forgiveness, inherited gear and transformation presentation

**Decision:** Class/body hitbox size does not automatically determine melee/basic-attack reach. Attack/ability ranges remain separately authored.

**Decision:** Large playable forms receive authored navigation/clearance forgiveness when needed. Required progression must not become inaccessible merely because a class has a larger footprint.

**Decision:** Descendant Ways inherit their parent's equipment-family permissions by default, with explicit additions/removals/overrides allowed.

**Decision:** Visible armor layering uses slot semantics: Chest, Head, Hands, Legs and Feet replace their baseline slot visuals; Shoulders and Back layer independently over the current torso/chest presentation.

**Decision:** Way transformation presentation is optional per advancement. It may be bespoke, simple, or absent; gameplay advancement state does not depend on the effect.

## 2026-10-01 — Body blocking, advancement gear preservation, and baseline exposure

**Decision:** Larger class-form hitboxes are real combat volumes. Where enemy/projectile hit logic uses spatial collision/overlap, larger forms can be easier to hit.

**Decision:** Player characters physically block one another.

**Decision:** Enemies physically block players. There is no generic anti-stuck/anti-box-in escape rule; being surrounded is intentional positioning pressure and the player must fight/use authored tools to escape.

**Decision:** This does not remove navigation forgiveness for required world routes. Doorway/route accessibility and combat body blocking are separate concerns.

**Decision:** When advancement makes inherited equipped gear incompatible, preserve it atomically: move to the child inventory first, then the Echo-wide bank if inventory lacks room. If neither can safely retain all displaced items, abort advancement before mutation. Never destroy/drop gear because of a transformation.

**Decision:** Empty equipment slots support both authored default clothing/armor and intentionally exposed body, chosen per class/form and slot.

## 2026-10-01 — Player blocking, Return ability, friendly projectiles, AoE overlap, and movement

**Decision:** Player-vs-player collision is solid blocking with normal collision sliding. Walking into another player does not push/force-move them.

**Decision:** There is no generic unstuck feature.

**Decision:** DiceFree should instead have an explicit **Return to current revive point** ability/action with a target cooldown of roughly **10 minutes**. It returns to the manifestation's currently registered resurrection point. Cast/combat/interruption/cooldown-persistence details remain open.

**Decision:** Friendly projectiles do not collide with allied player bodies by default.

**Decision:** Ordinary AoE uses gameplay hit-volume overlap: if any qualifying part of the actor's authored hit volume overlaps the AoE, the actor is affected unless that effect explicitly uses another rule.

**Decision:** Movement Speed remains class-authored/stat-derived and is not automatically inferred from body form, species, leg count or physical size.

## 2026-10-01 — Return-to-revive final core rules

**Decision:** Return to current revive point has a **10-second cast**.

**Decision:** Return cannot be started while the manifestation has active aggro/threat.

**Decision:** Taking damage during the Return cast interrupts/cancels it.

**Decision:** Return is disabled during an active dungeon run.

**Decision:** Return targets the manifestation's currently registered revive point.

**Decision:** Cooldown target is roughly **10 minutes**.

**Decision:** Return cooldown resets on **death, logout, and reload**. It is not durably persisted.

**Reaffirmed:** Return is not an unstuck feature and does not bypass ordinary player/enemy body blocking.

## 2026-10-01 — Return rooting and friendly actor collision

**Decision:** Return roots the caster for its 10-second cast. Movement cancels the cast.

**Decision:** Return cooldown begins only after a successful teleport. Interrupted or manually canceled casts do not consume it.

**Decision:** Dead actors stop body-blocking immediately, even if a corpse visual remains.

**Decision:** Friendly summons/pets/companions physically block their owner and allied players by default.

**Decision:** Ordinary friendly NPCs such as villagers, quest givers and vendors physically block players by default.

**Reaffirmed:** Solid movement collision uses normal sliding with no pushing/force-shoving unless an explicit mechanic says otherwise.



## 2026-10-06 — Start menu owns normal manifestation selection

**Decision:** DiceFree boots through a dedicated start menu. A fresh Echo starts as Novice; an existing Echo chooses among its saved class manifestations before entering the world.

**Decision:** Do not expose arbitrary live manifestation swapping as ordinary in-world gameplay. In-world advancement creates a new child manifestation; selecting an already-existing manifestation is normally a load/start-menu operation.

**Decision:** Store active manifestation and parent/child branch history as additive Echo-level roster metadata while each class continues to own one independent manifestation section.

**Decision:** Advancement copies owned inventory into the child with new item-instance IDs and remaps equipped references. Parent and child timelines must not claim the same item instance identity.

See ADR 0005 and docs/ADVANCEMENT_VALIDATION.md.

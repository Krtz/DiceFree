# First Slime Dungeon

Status: initial vertical-slice design target.

## Purpose

The first Slime Dungeon proves the complete DiceFree dungeon loop rather than trying to be a huge content piece.

Target first blind clear: roughly **8–12 minutes**.

Player loop:

World Face 1 / Cornberg-side entrance -> staging -> short dungeon -> Slime Boss -> private loot room -> world return.

## Session rules

- Physical entrance in the overworld.
- Separate Unity map/scene: `SlimeDungeon`.
- Stable dungeon ID: `dungeon.slime`.
- One live occupancy for `dungeon.slime` per hosted session.
- If another party/run already owns it, entry is blocked.
- Other dungeon IDs may run simultaneously.
- Other players may remain on any overworld face simultaneously.
- Default staging countdown: 60 seconds.

## Authored layout direction (Axel's draw.io map, 2026-10-08)

Use Axel's provided `Slime dungeon.drawio` forest layout as the spatial source of truth when authoring the scene. **No interior scene/blockout implementation is authorized yet.** The diagram uses tree walls/woodland partitions, not a sequence of enclosed cave rooms.

Mandatory standard route:
1. **Entrance** at the southern end of the left-hand lane; the existing 60-second staging lifecycle still applies.
2. **Trash mobs** in the left-hand woodland approach.
3. **Mandatory Big Slime miniboss** in the northwestern clearing. Defeating it unlocks the central puzzle room; players cannot skip the miniboss.
4. **Puzzle room** in the central lane. A large central tree is surrounded by **five green rings**. Players lure **five slimes** into the rings using normal enemy aggro, **one at a time, with no simultaneous-positioning requirement**. Any slime that reaches a qualifying ring becomes locked in place, invulnerable, and turns to face the central tree. If an uncaptured slime is killed, another spawns so the puzzle can always be retried. The exact completion effect, spawn distribution and ring assignment are still to be co-designed. The room unlocks only after the miniboss dies.
5. **Main Slime Boss** in the northeastern clearing.
6. **Private reward room and return** after completion. Their exact placement/geometry is not yet marked on Axel's map and must be agreed before scene authoring.

A **southern/eastern Slime Regent arena** branches off the central lane behind trees that disappear when the run's high-level condition is met. Keep this route visually secret and condition-gated. The Regent is not a replacement for the standard boss. The final sequence/requirement for clearing optional Regent content is not yet settled.

Retain dense, diverse natural tree boundaries, with isometric readability, traversable corridors and room for Slam landing telegraphs. Do not silently invent additional mandatory encounters, puzzles, doors, or rewards.

## Boss encounter teaching progression (decisions 2026-10-08)

Two slime encounters teach the **same two mechanics**. The mandatory miniboss teaches an easier form, and the larger main boss escalates it. These are the only two authored attack types for the first main boss: **Slime Slam** and **Divide**. No puddle/trail third attack.

### Shared Slime Slam: player-targeted jump

- A circular ground danger area appears **at a player's position**.
- The slime jumps into the air, then lands in that indicated circle; the target must run away from the telegraphed landing spot.
- The miniboss uses the weaker, smaller version; the main boss uses a stronger, larger circular area.
- Exact telegraph timing, damage and repeat frequency remain open.
- Seeing the move discovers Codex mechanic `mechanic.slime-slam`.

### Mandatory miniboss: Big Slime

- Visually a big slime, less imposing than the main boss.
- Has the smaller/weaker player-targeted jumping Slam.
- **Divide** produces **two** smaller slimes at opposite sides of the arena that slowly approach each other.
- Players must kill **at least one** before the two meet.
- On success, the surviving slime is the boss-like combatant and **resumes attacking at exactly the miniboss HP recorded immediately before Divide**. Killing the fragment does not itself reduce boss HP, and the final fragment is not an additional kill requirement.
- On failure, the two fuse and the miniboss heals by **the sum of the two slimes' remaining HP**. Exactly how fragment HP is budgeted versus boss HP remains to be specified; normal boss max-HP limit applies unless future design says otherwise.
- Defeating the miniboss opens access to the puzzle room.
- Award EXP and gold on boss defeat. No equipment/item drops.

### Main Slime Boss

Working stable ID: `boss.slime`.

- A **larger slime with a distinctive hat or similar visual feature**.
- Uses the same player-targeted jump Slam, but with a larger danger circle and stronger damage.
- **Divide** produces **three** smaller slimes; players must kill **at least two** before the divided slimes reunite.
- On success, the remaining slime stays boss-like and **resumes attacking at exactly the boss HP recorded immediately before Divide**. Players do not need to kill every fragment, and defeated fragments do not directly reduce boss HP.
- On failed Divide/reunion, the boss heals by **the combined remaining HP of its three fragments**, matching the miniboss's heal rule. Fragment HP budgets, how the three recombine, and exact proximity/timing remain to be tuned.
- Award EXP and gold on boss defeat. No equipment/item drops.
- Seeing Divide discovers Codex mechanic `mechanic.divide`.

**Boss rewards:** Each participating player gets their own **full EXP and full gold reward**; neither is split among party members. Gear/item rewards remain exclusively in the dungeon-end private reward room.

### Main-boss Divide pressure: aggressive tiny slime spawns

Only the **main boss** adds this complication: while its three larger fragments creep toward one another (and do not chase players), they spawn **tiny aggressive slimes** that attack players. The mandatory miniboss does **not** spawn these attackers during Divide, keeping the introductory encounter simpler. This is part of the main boss's Divide mechanic, not a third boss attack. Spawn frequency, number caps, and whether remaining attackers disappear or persist after Divide are still to be tuned.

### Introductory puzzle: five slimes around the tree

- **Room centerpiece:** one big tree with **five distinct green rings** placed around it.
- Slimes spawn in the room; use **ordinary aggro** to lure them toward the central tree. No bait or special luring action.
- A slime reaching a qualifying empty ring is **automatically captured**: it becomes invulnerable, cannot move, and **faces the tree**.
- Fill all **five** rings with five slimes. Each stays captured, so players can solve it **sequentially**, without coordinating simultaneous arrivals. The exact ring-matching requirements (if any) are undecided.
- A slime killed before capture **respawns/replenishes automatically**, preventing accidental damage from making the puzzle impossible.
- The tree's activation presentation, how it unlocks the next area, slime-spawn timing, and puzzle reset behavior remain to be defined together.
- Purpose: test the reusable dungeon-puzzle system and introduce players to simple enemy-positioning puzzles before harder later dungeon mechanics.

## High-level conditional route: Slime Regent

The dungeon condition framework, not scene-specific code, controls this route.

Initial authored condition:

**average participating player level >= 50**

At or above the threshold:

- blocking trees/roots/geometry on the traversal section disappear/open;
- `route.slime-regent` becomes accessible;
- `boss.slime-regent` becomes eligible;
- `loot.slime-regent` becomes part of the run variant;
- discovering the route records `secret.slime-regent` in the Codex.

The condition result is snapshotted when the run begins. The route does not pop in/out because party state changes after that.

The **Slime Regent** is a higher-level optional route/boss, but is not the final hard slime boss concept.

**Slime Queen / Slime King** remain later higher-level hard-boss content.

## EXP, gold and dungeon-completion loot

- Defeating the miniboss and main boss grants EXP and gold. Every participating player receives **their own full EXP and full gold reward**; neither award is divided among party members.
- **No equipment or item loot drops during the dungeon**, including from bosses. All item/equipment rewards are awarded only **after dungeon completion in the private reward room**.
- Each eligible player independently chooses a reward in the private loot room; preserve the existing independent-roll/no-smart-loot contract.
- Normal Slime Boss and optional Slime Regent content may affect the authored **reward-room pools**, not spawn world loot from those bosses.
- Chosen equipment becomes durable through the successful completion/reward flow.
- Observing offered/awarded dungeon reward items feeds Echo-wide Codex loot discovery; monster-drop sightings should not be fabricated for monsters with no drops.

Exact reward items, whether trash grants ordinary EXP/gold, and the Regent reward-room offer rules remain to be decided.

## Codex events exercised by this dungeon

- exact slime monster kill;
- dungeon reward item observed (no monster item drops in this dungeon);
- Slime Slam witnessed;
- Divide witnessed;
- dungeon wipe;
- dungeon player death;
- Slime Boss defeated;
- dungeon completed;
- loot observed;
- Regent route discovered;
- Regent defeated/loot observed later.

## Reset behavior

- trash remains dead during an active successful attempt;
- failed boss pull resets that encounter;
- full party wipe resets the full dungeon by default;
- abandon destroys/resets the run and releases occupancy;
- completed/finished cleanup releases occupancy;
- stale-lock recovery exists for failed cleanup/crashes.

## Not in the first blockout

- Slime Queen;
- Slime King;
- multiple difficulty modes;
- elaborate dungeon art;
- final loot tables;
- final Codex art;
- multiple simultaneous copies of Slime Dungeon;
- extra boss attacks beyond Slime Slam and Divide.

## Definition already authored

`Assets/_DiceFree/Settings/World/Dungeon - Slime.asset` currently defines:

- `dungeon.slime`;
- scene ID `SlimeDungeon`;
- 60-second staging;
- conditional variant `variant.slime-regent.level50`;
- route `route.slime-regent`;
- boss `boss.slime-regent`;
- loot `loot.slime-regent`;
- tag `secret.slime-regent`.
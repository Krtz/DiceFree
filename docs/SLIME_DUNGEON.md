# First Slime Dungeon

Status: **design approved for implementation** as a playable vertical slice. Balance targets remain provisional; the scene/runtime/editor tooling can now be authored on this feature branch.

## Purpose

The first Slime Dungeon proves the complete DiceFree dungeon loop rather than trying to be a huge content piece.

Target first blind clear: roughly **8–12 minutes**.

Player loop:

World Face 1 / Cornberg-side entrance -> staging -> approach -> mandatory miniboss -> five-ring tree puzzle -> choose regular Slime Boss or (if unlocked by blue-slime solution) optional Slime Regent -> private loot room -> world return. Defeating either end boss completes the run, triggering an approximately **15-second countdown** before automatic transfer into the private reward room. Fighting both final bosses in one run is not permitted.

## Session rules

- Physical entrance in the overworld.
- **Shared waiting/staging room with a target practice dummy.** The first entrant claims dungeon occupancy and starts the 60-second clock; subsequent eligible party members join the same waiting room. Players can practice attacks against the dummy, adjust gear, and prepare during staging. When the timer expires, the participant roster, gear and class lock and the dungeon run begins. Consumables remain usable during the run. The staging room is real dungeon infrastructure, not a loading screen.
- Separate Unity map/scene: `SlimeDungeon` (plus reusable reward scene).
- Stable dungeon ID: `dungeon.slime`.
- One live occupancy for `dungeon.slime` per hosted session.
- If another party/run already owns it, entry is blocked.
- Other dungeon IDs may run simultaneously.
- Other players may remain on any overworld face simultaneously.
- Default staging countdown: 60 seconds.

## Authored layout direction (Axel's draw.io map, 2026-10-08)

Use the available forest layout diagram/screenshot from Axel's earlier dungeon design as spatial guidance when authoring the scene. **Scene/blockout implementation is now authorized.** The diagram uses tree walls/woodland partitions, not enclosed cave rooms. Do not assume the editable `.drawio` source exists on disk unless verified.

Mandatory standard route:
1. **Entrance** at the southern end of the left-hand lane; the existing 60-second staging lifecycle still applies.
2. **Trash mobs** in the left-hand woodland approach: **ordinary green slimes only** for the first playable version. They grant **normal overworld-style EXP and gold** when defeated, but **no equipment/item drops**. No stronger trash variants yet.
3. **Mandatory Big Slime miniboss** in the northwestern clearing. Defeating it unlocks the central puzzle room; players cannot skip the miniboss.
4. **Puzzle room** in the central lane. A large central tree is surrounded by **five green rings**. Players lure five slimes using normal enemy aggro, one at a time, into **any empty ring**. Maintain **three free slimes** while unsolved, replacing captured or killed ones. Captured slimes lock in place, become invulnerable, and face the tree; killed slimes respawn and those losing aggro return to their starting spots. Once five rings are occupied, **the tree grows and opens the normal boss route**; free slimes disappear and spawning stops. A high-level variant introduces **one blue slime among the three available at a time**: capturing **five blue slimes** (not just any five) opens **both the normal and secret Regent routes**. There is **no release/reset of captured slimes**: if any ordinary slime fills a ring, the secret solution is unavailable for this run, although normal completion still works. The room unlocks only after the miniboss dies.
5. **Main Slime Boss** in the northeastern clearing.
6. **Private reward room and return** after completion. The reward room is its **own separate reusable Unity scene**, entered after victory rather than being physically attached to the boss arena; it must be reusable for future dungeons with different reward pools. After resolving the individual reward choice, return the player to this dungeon's physical entrance in the overworld. Use **one shared neutral reward-room appearance for now**, but author the scene/controller so **future per-dungeon décor/visual variants** can be configured without copying the underlying reward logic. Naming and presentation details remain flexible.

A **southern/eastern Slime Regent arena** branches off the central lane behind enchanted trees/roots. Its barrier should open **only when the high-level puzzle is solved with five blue slimes**, not merely when the level condition is met. Keep this route visually secret and condition-gated. After entering the secret branch, the Regent can be fought without defeating the regular boss first; defeating **either** final boss unlocks the reward room. Defeating either end boss ends further combat progression; after a roughly 15-second countdown the party is moved to the reward room, so both cannot be fought in one run.

Retain dense, diverse natural tree boundaries, with isometric readability, traversable corridors and room for Slam landing telegraphs. **Art palette exception:** trees at the **bottom/southern edge of the puzzle area are blue**, while **all other trees throughout the dungeon are green**. The central puzzle tree is green before activation and **turns blue only after the five-blue-slime secret solution**. Keep the distinction between these permanently blue trees and the central tree's dynamic transformation. Do not silently invent additional mandatory encounters, puzzles, doors, or rewards.

## Boss encounter teaching progression (decisions 2026-10-08)

Two slime encounters teach the **same two mechanics**. The mandatory miniboss teaches an easier form, and the larger main boss escalates it. These are the only two authored attack types for the first main boss: **Slime Slam** and **Divide**. No puddle/trail third attack.

### Shared Slime Slam: player-targeted jump

- A circular ground danger area appears **at a player's position**.
- The slime jumps into the air, then lands in that indicated circle; the target must run away from the telegraphed landing spot.
- The miniboss uses the weaker, smaller version; the main boss uses a stronger, larger circular area.
- Pick the target **at random from living participating players**, not based on the aggro target. Solo play naturally targets the sole living player.
- **Initial telegraph-to-impact duration: 3 seconds**, subject to gameplay testing. Exact damage and repeat frequency remain open.
- Seeing the move discovers Codex mechanic `mechanic.slime-slam`.

### Mandatory miniboss: Big Slime

- Visually a big slime, less imposing than the main boss.
- Has the smaller/weaker player-targeted jumping Slam.
- **Divide** produces **two** smaller slimes at opposite sides of the arena that slowly approach each other.
- Players must kill **at least one** before the two meet.
- On success, the surviving slime is the boss-like combatant and **resumes attacking at exactly the miniboss HP recorded immediately before Divide**. Killing the fragment does not itself reduce boss HP, and the final fragment is not an additional kill requirement.
- On failure, the two fuse and the miniboss heals by **the sum of the two slimes' remaining HP**. Exactly how fragment HP is budgeted versus boss HP remains to be specified; normal boss max-HP limit applies unless future design says otherwise.
- **Initial Divide reunion target: approximately 10 seconds**, to be tuned with actual playtesting; achieve via arena separation/movement rather than hard-coding an untested speed.
- Defeating the miniboss opens access to the puzzle room.
- Award EXP and gold on boss defeat. No equipment/item drops.

### Main Slime Boss

Working stable ID: `boss.slime`.

- A **larger translucent slime wearing a top hat**, visually more impressive than the miniboss.
- Uses the same player-targeted jump Slam, but with a larger danger circle and stronger damage.
- **Divide** produces **three** smaller slimes; players must kill **at least two** before the divided slimes reunite.
- On success, the remaining slime stays boss-like and **resumes attacking at exactly the boss HP recorded immediately before Divide**. Players do not need to kill every fragment, and defeated fragments do not directly reduce boss HP.
- On failed Divide/reunion, the boss heals by **the combined remaining HP of its three fragments**, matching the miniboss's heal rule. Fragment HP budgets and how the three recombine remain to be tuned. **Initial Divide reunion target: approximately 12 seconds**, subject to playtesting.
- Award EXP and gold on boss defeat. No equipment/item drops.
- Seeing Divide discovers Codex mechanic `mechanic.divide`.

### Divide activation thresholds and limits

- **Each boss separately** Divides at **70% and 30% HP**, maximum **two Divide activations per boss encounter**, one at each threshold. This prevents infinite loops when a failed Divide heals the boss back above a previously crossed threshold.
- Mark each threshold as consumed for the encounter. Healing above it and crossing it again never repeats that Divide.
- Each divided fragment's HP is **proportional to the corresponding boss's maximum HP**, not a fixed flat amount. Specific fraction(s) still need balancing.
- The exact timing of jump vs Divide in the attack queue and what happens if one hit skips both HP thresholds are still to be tuned.

**Boss rewards:** Each participating player gets their own **full EXP and full gold reward**; neither is split among party members. Gear/item rewards remain exclusively in the dungeon-end private reward room.

### Main-boss Divide pressure: aggressive tiny slime spawns

During Divide, the **normal boss and Regent spawn one small aggressive slime every 2 seconds**, with **no cap on how many are alive simultaneously**. The Regent's small slimes have more HP and deal more damage than the normal boss's adds. Spawning stops when Divide ends, but already spawned adds remain until killed. The miniboss creates no such adds. This remains part of Divide, not an additional regular-boss attack. Performance and balance need playtesting; do not silently introduce a cap. **Spawned Divide adds award no EXP or gold** so prolonged phases cannot be used to farm.

### Introductory puzzle: five slimes around the tree

- **Room centerpiece:** one big tree with **five distinct green rings** placed around it.
- Maintain **three free (uncaptured) slimes at all times** while the puzzle is unsolved: when a free slime is captured or killed, spawn a replacement so three free ones are available again. Do not count the permanently captured slimes toward the three.
- **High-level secret variant:** when average party level at run start is **>= 50**, ensure **exactly one of the three available free slimes is blue** (and replenish a blue one after capturing or killing it so collecting five blue slimes remains possible). The other two free slimes are ordinary green slimes. **Blue slimes have 25x the HP of ordinary green puzzle slimes** but share their normal movement, aggro, attacks, and capture behavior. **Purpose:** players may use AoE damage to kill ordinary green slimes before they approach the central tree, whereas the blue slimes survive those AoEs and can be lured into rings. This is a combat/puzzle interaction, not merely a generic tougher variant.
- Use **ordinary aggro** to lure any of these slimes toward the central tree; no bait, special luring action or slime-to-ring matching. **Any slime works in any empty ring.**
- A slime stepping into an empty ring is **automatically captured**: it becomes invulnerable, cannot move, and **faces the tree**.
- Fill all **five** rings; captured slimes stay put, allowing completion **one slime at a time** without synchronized player positioning. A failed/killed uncaptured slime can be replaced automatically.
- A puzzle slime that **loses aggro** before reaching a ring simply **returns to its starting position**, so it can be pulled again.
- **Ordinary completion:** when any five slimes occupy the five rings, the **tree grows and magically opens the normal door** to the Slime Boss.
- **Secret completion (high-level only):** when **all five captured slimes are blue**, the central tree performs the **same growth/magical opening** as the ordinary solution but **turns blue**; both the normal route and the secret Slime Regent route open. The five green ring colors do not change as part of this effect. The blue trees already at the southern/bottom edge of the puzzle area are static level art, distinct from the transforming central tree. Merely meeting the level threshold does *not* open the Regent barrier.
- **Captured slimes can never be released or replaced during the run.** In a level-50+ run, capturing even one ordinary slime makes the all-blue secret solution impossible for that attempt; finishing the remaining rings with any colors opens only the normal route. This is intentional, not a puzzle reset opportunity.
- As soon as the puzzle completes, **stop spawning and make all leftover free/roaming puzzle slimes disappear**. No way to repair or restart a mixed-color puzzle before leaving the dungeon.
- Capture order never needs to be simultaneous and any blue slime fits any ring.
- **Puzzle slime defeats grant zero EXP and zero gold**, including repeat kills of replenishing green or blue slimes. No other item loot either; repeated kills must not permit farming.
- **Finishing the five-ring puzzle awards an EXP chunk** to each participating player. The **five-blue-slime secret completion awards more puzzle-completion EXP** than ordinary completion. Both absolute numbers are deferred until balancing.
- Purpose: test the reusable dungeon-puzzle system and introduce players to simple enemy-positioning puzzles before harder later dungeon mechanics. Fine detail of the tree growth animation, ring feedback, slime replenishment delay and wipe/reset handling remains to be tuned.

## High-level conditional route: Slime Regent

The dungeon condition framework, not scene-specific code, controls this route.

Initial authored condition:

**average participating player level >= 50**

At or above the threshold:

- **blue puzzle slimes** become available among the three free slimes during the five-ring puzzle;
- **capturing five blue slimes** (one per ring) causes the puzzle tree to open the secret barrier/route in addition to the normal route;
- `route.slime-regent` becomes accessible and `boss.slime-regent` becomes eligible **after** the blue puzzle solution, not simply upon entering at high level;
- `loot.slime-regent` becomes eligible as a reward-room variant if the Regent is defeated;
- discovering the route records `secret.slime-regent` in the Codex.

The **high-level puzzle variant eligibility** is snapshotted when the run begins. Party-level changes mid-run do not change slime colors or puzzle eligibility. Route-open state then follows whether the all-blue ring puzzle has actually been solved. The threshold is confirmed as **>= 50**, including parties whose average is exactly level 50.

The **Slime Regent** is a higher-level optional route/boss, but is not the final hard slime boss concept. Once the five-blue-slime puzzle has opened the secret barrier, **players can fight the Regent immediately without first defeating the regular Slime Boss**. Defeating **either** the Regent or the normal Slime Boss satisfies the final-boss requirement for access to the reward room; they are alternatives, not mandatory successive fights. Killing the first final boss immediately triggers dungeon completion and a roughly **15-second reward-room countdown**; the other end boss cannot be fought during the same run.

### Slime Regent appearance and mechanics (authored direction)

- **Appearance:** larger even than the regular top-hatted Slime Boss; wears a **crown** and has **visible gold floating inside its translucent gel body**.
- **Larger Slam:** retains the player-targeted jump-and-circular-telegraph Slime Slam, but the Regent's AoE is even bigger. Keep the existing starting 3-second warning duration as a playtest baseline unless this encounter needs separate tuning.
- **Four-way Divide:** splits into **four major slimes** creeping toward one another. The party must kill **at least three before they reunite**; the remaining one reforms into the Regent. It also spawns **tougher aggressive small slimes with both higher HP and higher damage** than the regular Slime Boss's Divide (precise scaling and spawn frequency TBD). The small adds follow the established rule of staying active until killed after Divide resolves. The Regent Divides at **70% and 30% HP**, each threshold triggering **only once per boss fight** even if the Regent heals back across it. On success, the sole surviving fragment reforms into the Regent with **exactly the HP it had immediately before Divide**, just like the two smaller boss encounters; killing the fragments doesn't reduce that stored boss HP. If all four fragments reunite before the party kills three, the Regent **heals by the sum of their remaining HP**, capped by normal boss max HP unless later design changes the cap. The proportion of Regent max HP given to each fragment still needs tuning. **Initial Divide reunion target: approximately 15 seconds**, subject to gameplay testing.
- **Rolling Attack:** selects the **furthest-away living player when the attack starts** and locks onto that player's **position at the start**, then rolls toward that fixed position without homing/tracking later player movements. It leaves a **damaging slime trail** persisting **5 seconds**. Roll telegraph, damage, travel speed and collision/pathing are yet to be tuned.
- **Bouncing Attack:** at cast start, snapshots each participating player's position and places a **smaller AoE circle** at each marked location. The Regent then **bounces between these marked locations one after another**, not simultaneously; players can move away from the static marked circles. Exact player/marker ordering, solo behavior, timing per bounce and damage are still to be tuned.
- The Regent's roll/trail and bouncing abilities are **Regent-only additional attacks**. The regular Slime Boss retains only Slam and Divide in the first release.
- These abilities should add challenge while remaining readable in both solo and multiplayer play. Exact values and counterplay remain for encounter design/playtesting.

**Slime Queen / Slime King** remain later higher-level hard-boss content.

## EXP, gold and dungeon-completion loot

- Defeating the miniboss and **either the normal Slime Boss or the Slime Regent** grants EXP and gold. Every participating player receives **their own full EXP and full gold reward**; neither award is divided among party members.
- Defeating **either** final boss **immediately completes the dungeon** and starts an approximately **15-second countdown**. Players **can move freely and cast abilities, including Resurrection on fallen allies** during this window; this is not a frozen victory cutscene. **At expiry, living/revived party members transfer to the private reward room, while anyone still dead is sent to the Cornberg respawn point instead and receives no end-of-dungeon reward.** No deferred equipment, reward-room bonus EXP or reward-room bonus gold is offered to dead players. **EXP and gold already earned from defeated enemies or bosses are retained**, even if the player is dead at transfer time; dying only forfeits the final reward-room choice. Both final bosses cannot be defeated in the same run. Exact UI and whether the countdown can be skipped remain to be decided.
- **No equipment or item loot drops during the dungeon**, including from bosses. All item/equipment rewards are awarded only **after dungeon completion in the private reward room**.
- Each eligible player is offered **one or two independently rolled equipment rewards** from the appropriate dungeon reward pool and may **choose exactly one**. When the rare lucky drop does not occur, give **one normal equipment option 75% of the time and two options 25% of the time**, rolled independently for each eligible player.
- **Rare lucky drop override:** the rare **Slimy Tophat** has a **5% chance per eligible player's reward-room roll/completion**. When it appears, it is **the only equipment option** offered; there is no second normal equipment item alongside it. Exact order/implementation of the rare roll versus normal loot rolls can be chosen at implementation as long as the overall rate stays 5%.
- Instead of equipment, the eligible player can **decline the item offer and choose bonus EXP and gold together**. These are fixed authored amounts **per final boss choice**: normal Slime Boss and Slime Regent each have their own amounts (yet to be specified), rather than a single universally fixed payout.
- Preserve **independent rolls per player** and **no smart loot**: items can be offered regardless of which class can equip them.
- **Normal Slime Boss reward pool (first-playtest stats, explicitly subject to future balance changes):**
  - **Slime Orb** — offhand for **Magically Touched** characters: **+3 INT, +3 SPI, +2 VIT, +3 magical defense, +1 maximum auto-attack damage**. The damage bonus affects the upper end of auto-attack damage, **not spells or skills**.
  - **Slime Shield** — offhand for **Physically Blessed** characters: **+5 physical defense, +3 magical defense, +5 VIT, +2 STR**.
  - **Slimy Farmer's Gloves** — gloves, **equippable by everyone**: **+5% attack speed, +3 AGI**. The percentage is a provisional balancing target.
  - **Slimy Tophat** — hat, **equippable by everyone**, rare lucky drop (**5%**): **+5 magical defense, +5 physical defense, +10 HP per second regeneration, +1 to all stats**. When rolled it overrides the usual one/two-item equipment offer.
- **Slime Regent reward pool:** a **distinct authored table** that nevertheless **includes all normal Slime Boss items**, plus exclusive Regent items not yet designed. Specific probabilities, exclusives, and Intrinsics remain TBD. Neither final boss drops items during combat; all rolls happen at the end reward room.
- Chosen equipment becomes durable through the successful completion/reward flow.
- Observing offered/awarded dungeon reward items feeds Echo-wide Codex loot discovery; monster-drop sightings should not be fabricated for monsters with no drops.

The **item identities and numeric bonuses above are current first-playtest design targets, explicitly open to rebalancing**, not yet implemented item assets/drop definitions. **No additional special effects or Intrinsics are intended for these four items for now beyond their listed stats**; adapt any equipment-schema requirements without introducing unapproved effects. The normal reward-room roll is **75% one option / 25% two**, with a separate **5% rare Tophat override**. Exact fixed bonus EXP/gold amounts for both final bosses are **deliberately deferred until balancing**. Ordinary green approach slimes grant normal EXP and gold; **puzzle slimes and spawned Divide adds grant no EXP or gold**. **Puzzle completion grants EXP, with a higher amount for the all-blue solution**; the exact values are deferred. Slime Regent-exclusive equipment remains to be designed.

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
- if a boss encounter disengages without a full party wipe, that boss encounter resets; a **full-party wipe always terminates the dungeon run** as described below;
- **any full-party wipe in this Slime Dungeon ends the run and sends every party member to the Cornberg respawn point** (including wipes in the puzzle room), resetting the instance and releasing its occupancy; no restart/checkpoint inside the dungeon;
- abandon destroys/resets the run and releases occupancy;
- defeating either final boss begins the ~15-second transition to the private reward room; **after an eligible player resolves their individual reward choice, automatically return that player to the physical Slime Dungeon entrance in the overworld** (not the Cornberg respawn point). Players still dead at countdown expiry go to Cornberg instead, with no reward room visit. Completed/finished cleanup releases occupancy, with exact release timing still subject to session-flow implementation;
- stale-lock recovery exists for failed cleanup/crashes.

## Not in the first blockout

- Slime Queen;
- Slime King;
- multiple difficulty modes;
- elaborate dungeon art;
- final loot tables;
- final Codex art;
- multiple simultaneous copies of Slime Dungeon;
- extra attacks beyond Slime Slam and Divide **for the regular main Slime Boss** (Regent-specific Rolling and Bouncing are separate higher-level encounter mechanics).

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

## 2026-10-09 follow-up — secret Regent superboss balance (provisional)

- The normal Slime Boss now has **3,000 maximum HP**; its Divide fragments and the mandatory miniboss's Divide fragments each have a fixed **300 maximum HP**.
- The **Slime Regent** now has **16,000 maximum HP**, unlocked only via the already-authored level-50 five-blue puzzle. This is intentionally a secret high-level challenge, not a normal-level boss.
- Each Regent Divide fragment has **10% of the Regent's maximum HP** at spawn, i.e. **1,600 HP** for the current 16,000-HP Regent. Four fragments appear, of which at least three must be defeated before reunion; two Divide phases at 70% and 30% boss HP remain.
- The existing **15-second Regent reunion window** is unchanged pending an actual high-level party balance playtest. This implies at least 4,800 fragment HP must be cleared per successful Divide, alongside attacking adds; a solo character may not be able to pass this damage check. Do not silently turn down fragment HP to make the automated test pass.
- The changes above supersede the earlier provisional paragraph stating all fragments are proportional to every boss's max HP. Normal/miniboss fragments are now intentionally flat 300 HP; only Regent is proportional.
- Implementation: `SlimeDungeonTuning.regentFragmentHpFraction` and the conditional fragment maximum HP in `DungeonSlime.Divide()`; `Slime Regent.asset` and `Slime playtest tuning.asset` updated; editor validation assertions changed. **The fresh real Regent-route isolated-save Play Mode test passed on 2026-10-09 with 16,000 Regent HP and 1,600 per Divide fragment** (`dicefree.slime.playtest --regent true`, phase 14); manual level-50 damage-check balancing remains necessary.

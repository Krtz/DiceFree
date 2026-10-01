# DiceFree Agent Handover

## Purpose

This is the operational handover document for any ChatGPT/Codex/other agent working on DiceFree.

A new agent should be able to read this file, inspect the linked repository state, and continue the project without requiring Axel to reconstruct the previous chat.

**This file is living project state, not static documentation. Keep it updated.**

---

## NON-NEGOTIABLE AGENT WORKFLOW

### Before doing meaningful work

Every agent must first:

1. Read this file fully.
2. Read the relevant open GitHub issues.
3. Read **all relevant comments** on those issues.
4. Read the relevant design documents in `docs/`.
5. Inspect the relevant branch and latest commits.
6. Check whether another agent recently changed implementation or design.
7. Treat the repository/documented decisions as the durable source of truth instead of relying on remembered chat context.

Do not jump directly into coding from an old prompt.

### After EVERY Axel design answer

Whenever Axel answers design questions or settles/changes a rule:

1. Put the answer into the appropriate canonical design document(s).
2. Update relevant issue(s)/issue comments if implementation is affected.
3. Update this handover file if the decision changes current state, priorities, or important system rules.
4. Do this **before moving on and asking the next batch of design questions**.

Do not allow settled design to exist only inside a chat.

### After EVERY meaningful Codex/agent action

After coding, refactoring, testing, documenting, investigating, or changing GitHub state:

1. Update relevant issue comments with:
   - what changed;
   - what remains;
   - what is provisional;
   - validation performed;
   - commit SHA.
2. Update relevant canonical docs if behavior/design changed.
3. Update this handover file with current implementation/work-in-progress state.

**Codex: update this document after every meaningful action/work cycle.**

### At the end of EVERY prompt/work cycle

Refresh:
- Current implementation status
- Current work in progress
- Known risks/provisional areas
- Recommended next smallest step

The goal is that a new chat can simply be told:

> Read `docs/AGENT_HANDOVER.md`, then read the relevant issues/comments/docs and continue.

---

# Repository / branch model

Repository: `Krtz/DiceFree`

## Branch roles

- `main` — stable/older baseline. Not the active Cornberg implementation truth.
- `setup/unity-project` — living design/documentation source of truth.
- `poc/cornberg` — active playable Cornberg vertical-slice implementation branch.

Other focused branches may exist. Inspect before assuming they are current.

## Branch rules

- Do **not** merge `poc/cornberg` to `main` unless Axel explicitly approves it.
- Do **not** wholesale-merge `setup/unity-project` into `poc/cornberg`.
- Reconcile the specific design/implementation changes needed.
- Keep systems modular.
- Avoid giant manager classes / monolithic god objects.
- Leave touched code cleaner than you found it.

---

# Current project focus

The current priority is the **Cornberg vertical slice**: build a real playable early game while creating reusable systems only as they become necessary.

Preferred order:

1. real Cornberg progression/content;
2. reusable framework needed by that content;
3. validation;
4. docs/issues/handover updated immediately;
5. then the next smallest coherent playable increment.

Avoid disappearing into abstract framework work that the next playable step does not need.

Do not prematurely jump to:
- full cube traversal;
- full networking/host migration implementation;
- huge endgame systems;
- mature dungeon infrastructure before Cornberg needs it;
- unrelated combat-math rabbit holes.

---

# Current implementation status

## Latest completed increment: Cornberg Q4

Implemented, validated and pushed on `poc/cornberg` in **39ebbf29c809bf08e7d4bc2626f80714159e85e5** (Add Cornberg Q4 surge alternatives elite and world loot).

- `quest.cornberg.break-slime-surge` follows completed `story.cornberg.runner-returns`.
- Different farmer `npc.cornberg.surge-farmer`, provisionally **Mira**, stands near (12,0,-8) before/after Q4 with reusable proximity barks. Name/art/location/dialogue remain provisional.
- Shallow semantic ANY objective: dangerous elite OR 30 eligible tagged Slimes, simultaneously active; no retroactive credit; ReadyToTurnIn then farmer reward. `quest-credit.cornberg.surge-slime` is on Road, Named Forest and elite definitions, not Crop Slime.
- Nearby quest credit uses a provider-neutral local party seam and inclusive 50m Euclidean radius from defeat. Existing owner XP is unchanged; no networking.
- `enemy.forest-elite-slime` uses the shared Slime archetype in the separate existing deep clearing near (77,0,53), without an exact quest marker. Provisional level 8, 240 HP, 12 damage, 1.6s interval, 0.5s windup, 0.6m reach, 3.4m/s, 8m awareness, 16m leash, 80 XP. Fixed 450s respawn.
- Authored 20% shoe roll creates a transient FFA world pickup. Any valid nearby manifestation can claim a new owned instance; no killer reservation and no durable ownership until pickup.
- Shared provisional reward: **200 XP, 100 gold, one `item.cornberg.work-gloves`**. Completion/reward mutation uses prevalidation, reentrancy guard and deferred persistence writes.
- Schema/manifestation **v4** adds per-alternative counts. Supported v1/v2/v3 saves migrate; genuine item-bearing v3 fixture, unknown records, original backups and recovery remain covered. Profile Inspector shows alternative IDs/counts.
- Measured from level 6/0 XP: elite route **level 9/10 XP**; 30 Road Slimes at unchanged 20 XP each **level 13/30 XP**. Do not silently suppress incidental XP to erase this difference.
- Real combat fixtures: solo 6 loses, solo 8/9 win (~25/~52 HP left). Stationary two-level-6 math fixture wins in ~9.89s with ~23.68 tank HP; not a networked co-op playthrough.

Passed Q4 focused validation/seven reloads, both completion orders, partial+elite, pre-accept exclusion, barks/range, 49/50/>50m party credit with no shared XP, reward prevalidation/reentrancy, non-killer pickup, per-life rolls and actual 450s respawn. Existing item/equipment/gold, separate reload, road speed, trivial aggro, combat/death/Return/well, Q1/Q2/Q3/Runner, all 13 traversal routes/both controls, persistence/migrations/recovery/backups/stale writers/Profile Inspector passed. Windows development build: **172,036,002 bytes**. Two isolated standalone startup/reload smokes passed: completed Q4, 100 gold, one glove/one shoe, stable GUIDs/XP, no runtime errors. No manual playthrough claimed; exact editor SearchDatabase #17 remains separate.

Initial elite placement overlapped the Road Slime and failed Q2; only the new elite moved to the separate clearing, then regressions passed. Scene changes are additive; geography/navigation and existing balance were preserved. See `docs/CORNBERG_Q4.md` on PoC. Issues **#6/#15/#22/#28/#24/#26** now contain SHA, scope, validation and pacing. Nothing merged.

**Current next step:** Axel's Q4 playtest/pacing review, especially the level-13 mass route. MSQ5 is a separate next content increment requiring its own scope; it is not implemented. Dungeon, mounts, networking and mature item/encounter systems remain out of scope.

## Implemented foundation on Cornberg direction

Known working/implemented areas include:

- Unity project foundation
- Cornberg blockout/playable starter area
- Classic Mouse click-to-move
- Direct/WASD movement
- camera/traversal controls
- basic player combat
- Slime enemy combat
- aggro / leash / home reset
- actor collision
- death / Return
- Cornberg healing well
- enemy respawn
- XP / leveling
- Q1 crop progression
- Q2 road progression
- persistence/save foundation
- schema/versioning/migration work
- backups/corruption recovery/stale-writer protections
- Profile Inspector/dev persistence tooling
- stat / Defense / crit / element foundations
- semantic quest credit/events direction

## Confirmed Q3 implementation

Q3 is implemented and pushed on `poc/cornberg` in `4289854b096b790cbb8df847f7e4c8309b2033d7` (Add Cornberg Q3 named forest Slime progression).

Farmer offers Q3 after Q2; ReachArea -> named forest Slime Kill -> return uses the existing woodland clearing and shared combat/respawn/credit/XP/persistence systems. Farmer conversation retains the latest completed offer. Name, location and tuning remain provisional; see `docs/CORNBERG_FOREST.md` on the implementation branch.

Passed: focused Q3 with four reload checkpoints, Q1/Q2, combat, all 13 traversal routes, persistence/migration/recovery/stale-writer/Profile Inspector, Windows development build and eight isolated startup/reload smokes. No manual playthrough claimed. Issues #6/#15/#22 contain the implementation handoff. Navigation/save schema/existing balance preserved.

# Current work in progress

Q1 through Q4, including Runner Returns, are complete on `poc/cornberg`. No implementation work remains in progress for this Q4 cycle. The sections below retain earlier increment history; the Q4 status above supersedes earlier future-work wording.

Runner Returns landed in `2de1a58703e845e1018bcdd5cfdf3f185f16a916` (`Add Runner Returns and reusable TalkTo story credit`).

It adds:
- provisional `Cornberg Runner` NPC after Q3;
- stable NPC/conversation/story IDs;
- reusable semantic `TalkTo` / acknowledged-conversation facts;
- prerequisite gating;
- conversation panel / explicit acknowledgment;
- durable completion using existing schema v2;
- focused reload/semantic regression validation;
- `docs/CORNBERG_RUNNER.md`.

Q4 is now implemented as recorded above. Its settled completion structure remains:

- path A: kill the dangerous elite Slime;
- path B: kill 30 eligible Slimes after accepting Q4;
- crop-field Slimes do not count;
- stronger non-crop starter-region Slimes count;
- eligible Slimes inside the Slime dungeon count;
- pre-Q4 kills are not retroactive;
- the Slime dungeon itself is side content / a sidequest, not a Q4 completion path;
- both Q4 paths award the same main quest reward;
- completing one route completes Q4 and cannot duplicate the main reward;
- incidental elite/dungeon/ordinary-kill rewards remain separate;
- target pacing after Q4 is roughly level 9–10, with exact XP left to gameplay testing;
- both alternatives are active simultaneously; the player never locks a route;
- the elite must be killed after Q4 begins, just like the 30-kill route is non-retroactive;
- quest guidance only says the elite is somewhere in the Slime Forest;
- this first elite is deliberately a stat-check fight with no bespoke phase/split/pool/enrage mechanics;
- satisfying either route sets Q4 ready-to-turn-in; the player returns to Cornberg for completion/reward.

Q4 uses a fixed deep-forest clearing, 7.5-minute respawn, 50m nearby credit and a different named farmer already present as ambient flavor. Exact elite stats, XP/gold payouts and farmer presentation still need authoring/playtest. Do not reintroduce the dungeon as a third Q4 route.

## Trivial-enemy auto-aggro — complete

Implemented and pushed on `poc/cornberg` in `e6d5cefc9d594b1ed2a8fc4cd9ed06a9da3ed7c8` (Add configurable trivial-enemy auto-aggro policy).

`AutoAggroPolicy` is a small serialized policy consulted only by proactive awareness scanning. Default `trivialLevelGap = 10` is explicitly provisional. Gaps 0-9 retain normal acquisition; gaps 10+ suppress it. `alwaysAutoAggro` is an authored exception. Archetypes supply defaults, variants can override, and standalone enemies can configure their own policy. A virtual eligibility method leaves a seam for future scaled policies or region/difficulty selection without implementing those systems now.

Retaliation, existing targets, hostility/LOS/radius, leash/home, fixed enemy levels, kill/XP/quest credit and respawn are preserved. No scene/nav, existing balance, combat-math or save-schema change. See `docs/CORNBERG_AGGRO.md` on the implementation branch.

Validation passed: real-AI gaps 0/9/10/20, live level changes, retaliation, explicit attack/kill/XP, always-aggro and custom thresholds, hostility/LOS/awareness, leash, reset/respawn and archetype/variant selection. Cornberg combat/death/Return/well, Q1, Q2 (six reloads), Q3 (four), Runner Returns (four), all 13 traversal routes, fresh/save-reload, storage/migration/recovery/backups/stale writers/Profile Inspector passed. Windows development build passed (171,990,466 bytes); two isolated standalone startup/reload smokes retained identity and completed runner at level 6/0 XP with no runtime errors. No manual playthrough claimed. Known SearchDatabase #17 remains separate.

This delivers only the trivial-aggro slice of #35. Mounts, collection, maps/achievements and the broader #28 encounter engine remain unimplemented. Road-speed surfaces are now implemented below. Q4 core route decisions have since been settled above; remaining authored details still need resolution. No new separate blocker was found.

---
## Road/surface speed — complete

Implemented on `poc/cornberg` in `9f1d6f8634813cb8f2ab2a68e7c81121d8c5597f`. Existing east/forest road now grants a provisional **+15%** player movement speed. `TravelSurface` samples its readable mesh; player-only `SurfaceTravel` supplies a named factor. `ActorStats -> CombatActor -> TraversalMotor` retains the current baseline; effective speed is shared by Classic/NavMesh and Direct. Factor composition and overlap priority/stable-ID selection remain provisional, not final mount stacking design.

Passed: focused road tests (real mouse/WASD, combat, live stats, enter/exit, overlaps, teleport, death/Return/load); all 13 traversal routes; combat/death/well; trivial aggro; Q1; Q2 (six reloads); Q3 and Runner Returns (four each). Windows development build: 171,996,594 bytes. Two isolated standalone startup/reload smokes: zero errors, identity/level 6/0 XP/completed Runner retained, revisions 9 -> 10. No human playthrough claimed. Known SearchDatabase #17 remains separate. Full storage/migration/recovery suite was not separately rerun because durable code is untouched.

Scene adds only two components (33 lines); geography/nav/enemy tuning/save schema unchanged. See `docs/CORNBERG_SURFACES.md` on PoC for IDs, static-mesh limits and tests. Issues #35/#15 document the handoff. Trivial aggro is complete, mounts are not started, and Q4 is not implemented; preserve the newer Q4 decisions and resolve remaining authored details before its own patch.

## Handcrafted items / equipment / carried gold — complete

Implemented and pushed on `poc/cornberg` in `ba29d5a46f6f126bacb8b5fb5365196f98393423` (Add durable handcrafted items equipment and carried gold).

- Authored ItemDefinition with all eleven slots, fixed stat package and provisional metadata; owned GUID instances distinct from content IDs, duplicate definitions allowed.
- Manifestation CarriedInventory, Equipment references and GoldWallet; fixed-item grant boundary; source-keyed equipment stats separate from transient effects; free overworld/combat swaps.
- `item.cornberg.work-gloves`: provisional Cornberg Work Gloves, Hands, +5% Attack Speed only.
- `item.cornberg.forest-shoes`: provisional Forest Travel Shoes, Feet, +1 Physical Defense, +1 Magical Defense, +1% Movement Speed. The later 20% drop is NOT implemented.
- Gear Defense joins underlying Defense. Gear speed affects runtime stats, not AGI coefficients; road remains independent. 1 unit = 1m; displayed Move Speed = physical m/s ×10. No motor retune.
- Inventory overlay uses provisional B (I remains interaction), data-derived labels, equip/unequip and explicitly development-only grants. New games start empty.
- Echo/manifestation v3 migrations from supported v1/v2; old inventory/equipment/gold default empty/zero. Unknown sections/resources/quests remain preserved; missing item definitions/unknown slots round-trip inertly. Profile Inspector exposes new ownership/currency diagnostics.

Passed: focused inventory/equipment/gold/stat/metric/road/death/Return/combat-swap and process reload checks; road; trivial aggro; combat/death/Return/well and math; all 13 traversal routes/both controls; Q1; Q2 (six reloads); Q3 and Runner (four each, including v2 migration); fresh/injured/dead/legacy persistence, migrations, recovery/backups/stale writers and Inspector. Windows development build: 172,013,950 bytes. Two isolated standalone smokes retained four instance IDs, three slot references (including inert test data) and 37 test gold, revisions 3 -> 4, no errors. No human playthrough claimed; exact known SearchDatabase #17 remains separate.

Scene only adds four player components (55 lines); geography/navigation/enemy/quest tuning unchanged. See `docs/CORNBERG_ITEMS.md` on PoC. Issues #24/#26/#15 receive the handoff. #24 remains open: no generation/budgets/affixes/sockets/sets/procs, equipment art, vendors/bank/crafting or advanced eligibility. No Q4, elite, drop roll, ambient barks, mounts or networking implemented.

Next smallest gameplay patch is Q4's OR objective/reward integration, using the now-durable grant boundary and ensuring quest completion/XP/gold/gloves are captured together. No item-foundation blocker remains. Farmer name/art/barks and exact elite level/stats/XP/gold require authored choices and tuning, not a new global policy. The 50m cooperative credit rule is settled; actual networking stays out of scope.

# High-level game / narrative context

DiceFree is an isometric co-op action RPG inspired strongly by Warcraft III custom ORPGs such as Twilight's Eve.

DiceBound occurs inside the Dice. DiceFree begins outside, on the exterior/cube-world.

The player is an **Echo** in both games.

Classes are Ways: an Echo becomes what reality has enough evidence to recognize.

The outside world has six main faces/regions echoing DiceBound's six boards.

Face 1 / Green Road begins welcoming/pastoral and contains Cornberg.

---

# Core class progression

## Advancement levels

Current advancement thresholds:

- 10
- 30
- 60
- 120
- 200

Advancing creates a new class manifestation at level 1.

No XP carries into the child class.

The parent manifestation remains playable.

At most one persistent manifestation/save-state exists per class ID.

## Tier 0 / Novice

Novice:
- starts from base attributes 1/1/1/1/1;
- gets class-authored stat growth;
- has no generic manual attribute-point allocation;
- has no normal class resource;
- basic attack uses the highest primary attribute;
- gets the early experimentation/respec convenience.

General attribute progression is determined by **class + gear**, not manual VIT/STR/AGI/INT/SPI point spending.

## Respec

There is no need for a broad always-available respec system as a core feature.

Early Novice/onboarding can support respec so players can experiment.

Later classes should not assume routine respec unless explicitly authored.

## First advancement

Tier 0 -> Tier 1 is intentionally simple:
- go to the mountain.

Later advancement rituals/quests/trials are decided case by case.

---

# Ways / discovery

Class progression is a graph/web, not necessarily a symmetric tree.

Class visibility is authored per class.

A Way may:
- show a silhouette + exact requirements;
- show a silhouette + cryptic requirements;
- appear as a large 3D question mark;
- be hidden until the player discovers how to unlock it;
- be absent until actually unlocked;
- use another deliberately authored secrecy mode.

Do not impose one global reveal pattern.

Way discovery/unlock knowledge is Echo-wide.

Unlocking does not automatically create a manifestation; the player still advances into that class from an eligible parent manifestation.

---

# Quests / interactions

Default quest NPC markers:
- `!` available quest
- `?` follow-up/turn-in

Normal quests should currently tell the player what to do plainly.

Individual quests may deliberately be vague later.

There is **no active quest cap**.

Do not invent one.

Quest/world persistence can be manifestation-specific or Echo-wide depending on authored intent.

---

# Stats / combat foundation

Primary attributes:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

General class/gear direction:
- VIT supports HP/regeneration
- STR contributes Physical Defense
- AGI contributes attack/movement speed
- INT contributes Magical Defense
- SPI contributes healing done/received

See `STATS_AND_DAMAGE.md` for exact current formulas.

Physical/Magical Defense and elemental resistance are separate systems.

One packet has at most one element.

Mixed channel/element attacks resolve as separate packets.

Crit is explicit opt-in:
- baseline crit chance 0;
- no universal crit multiplier.

Hit/miss has no hidden default miss baseline.

## Resistance inversion clarification

When elemental resistance inversion produces restoration:
- it can restore a **living** target;
- it does **not** resurrect a dead target;
- resurrection requires an explicit resurrection/death-prevention mechanic.

Do not accidentally revive dead actors through mixed/inverted packets.

---

# Equipment / loot

Equipment slots:

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

Handcrafted gear should be the majority of meaningful progression, especially later.

Randomized gear still exists, but only from authored/controlled sources.

Item level is source-authored rather than scaling to player level.

No durability.

No normal unidentified-item system.

No smart-loot weighting.

Classes have explicit allowed weapon/equipment types.

Items may also be:
- class-specific;
- class-family/type restricted;
- restricted by other semantic eligibility.

Off-class loot can still be:
- banked;
- transferred via bank;
- sold;
- sacrificed for transmog.

---

# Inventory / shared bank

Carried inventories are manifestation-specific.

Shared bank is **Echo-wide only**.

There is no separate personal stash in addition to the shared bank.

Bank direction:
- starts relatively modest;
- huge eventual capacity;
- buy many additional slots/pages with gold;
- pricing should become progressively more expensive;
- intended as a useful gold sink;
- tuning remains open.

Bank UI should support:
- normal text search;
- auto-sort;
- semantic type searches such as `weapons`, `swords`, `potions`, `rare`, `legendary`;
- structured filters such as `type:`, `slot:`, `rarity:`, `class:`;
- regular expressions;
- a visible **Regex toggle next to the search field**.

Items may be sent to bank remotely.

Withdrawals require being in town/at appropriate bank access.

Carried gold cannot be remotely banked.

Banked gold is safe from ordinary death loss.

---

# Transmog

Transmog collection is Echo-wide.

Unlock appearance by **sacrificing/destroying the item**.

Warn before sacrificing:
- last owned copy of a unique/handcrafted item;
- other clearly difficult-to-replace authored equipment.

Applying transmog happens through an NPC/service.

A class can normally only use an appearance corresponding to gear it can equip.

Framework may support explicit exceptions.

No general dye system.

---

# Consumables

Consumables occupy the normal bag.

Cooldown handling is authored case by case.

Support cooldown families/categories shared by multiple consumables.

General intent:
- consumables are mostly emergency buttons;
- they should not be mandatory rotational spam.

---

# Dungeon structure

Dungeons have physical entrances.

No fast-travel-to-dungeon button.

The player travels there through the world.

Initial party target: 1–4 players.

Entry staging uses the established waiting-room/countdown model.

Equipment locks when the active run begins.

Bosses reset on failed encounters.

Default full-party wipe resets the dungeon run.

No universal raid lockouts.

Old content can generally be overpowered later rather than universally downscaled.

Each dungeon defines its own modes.

Many dungeons may have only one mode.

---

# Dungeon loot

All dungeon equipment loot is awarded at successful completion through the final private loot room.

Each eligible player gets their **own independent private reward roll**.

There is zero competition between players for these rewards.

General current reward direction:
- roll one or two item offers;
- show complete item details;
- player chooses one item;
- OR player chooses an alternate rolled/contextual reward such as XP, gold or materials.

No unidentified reward loop.

Unchosen rewards disappear.

Repeated dungeon farming is allowed indefinitely unless a specific dungeon explicitly says otherwise.

Dungeon-specific materials may feed:
- crafting;
- named equipment;
- vendors;
- cosmetics;
- other authored rewards.

---

# Enemy / rare design

Enemies use fixed authored levels by default.

No universal level scaling.

Overworld named/rare enemies are supported.

Rare behavior is case by case:
- fixed spawn;
- multiple possible spawns;
- patrol;
- other.

They are intentionally farmable.

Long respawn timers are allowed.

Some can be soloed around intended level.

Some are deliberately:
- bring friends;
- come back later.

Rewards/purpose may include:
- handcrafted loot;
- achievements;
- lore;
- bonus XP/gold;
- mounts/cosmetics;
- challenge only.

---

# Trivial enemy aggro

Initial working rule:

An ordinary enemy should stop automatically aggroing a player who is **10 or more levels above it**.

The system must support later scaling/tuning rather than hard-coding one permanent rule.

Possible override dimensions:
- absolute level difference;
- proportional/scaled level difference;
- enemy archetype;
- region;
- difficulty;
- explicit encounter flag.

Authored enemies can always aggro regardless of level difference:
- bosses;
- guards;
- territorial enemies;
- ambush enemies;
- quest/event enemies;
- other explicit exceptions.

---

# Codex

Codex progression is Echo-wide.

## Monster discovery

Monster families have subpages/variants.

A monster is discovered when the Echo kills it.

## Drops

Drops appear only once the player has actually seen them drop.

Do not show undiscovered drops as `???` slots.

Each discovered drop tracks its own **times seen drop** count.

Once every authored drop has been observed, Codex can state **all drops discovered**.

## Boss mechanics

Boss mechanics are recorded when witnessed.

They can be organized by phase/state.

Normally describe observed mechanics plainly and usefully, with authored exceptions allowed.

## Dungeon Codex

Track at least:
- clears;
- wipes;
- bosses killed;
- player deaths;
- mode completions;
- solo clears;
- no-death clears;
- fastest clear;
- loot discoveries.

Fastest clear is tracked separately by party size.

A dungeon gets its normal clean gold completion check when:
- all its authored modes are completed;
- all its authored drops are observed.

Some dungeons may additionally award titles/cosmetics/etc.

## Codex rewards

Optional per-entry rewards may include:
- title;
- cosmetic;
- achievement;
- tiny damage bonus against the **exact mob**;
- combinations;
- nothing.

Damage bonuses should be tiny and tightly capped.

Do not make broad family bonuses that turn unrelated monster completion into required endgame optimization.

Rewards trigger automatically.

---

# Achievements

Achievements are Echo-wide.

Achievements are grouped into authored categories such as Exploration, Combat, Dungeons, Classes/Ways, Professions, Collections and Secrets.

Hidden achievements are supported.

Once a hidden achievement is earned/revealed, its normal name/description stays revealed. Do not keep completed hidden achievements mysteriously concealed by default.

Achievement score/points exist as an overall completion/bragging metric.

Exact reward policy can vary by achievement.

---

# Exploration / map

Unlike Codex/achievements, **map terrain exploration is manifestation-specific**.

Every manifestation must explore the world for itself.

Reveal should be fairly precise to where that manifestation (or its currently allied party vision) has actually explored, not huge region-wide chunks.

Allied/party exploration can reveal terrain for the current manifestation while playing together.

Current direction: important discovered POI **icons may be Echo-wide knowledge** even when another manifestation has not uncovered the surrounding terrain. Exact icon-sharing categories remain open.

Some deliberately secret content may opt out of normal marker sharing/visibility.

---

# Mounts / roads / travel

Mount collection is Echo-wide.

A manifestation may actually mount when it meets one of:
- Tier 2+;
- Novice level 75+;
- Tier 1 level 75+.

The two level-75 routes are hidden progression possibilities.

Ordinary mounts share the same base speed.

General mount speed may increase through progression/collection.

Current idea: each collected mount may add a small amount of general mount speed, with approximately +2% per mount discussed as an example.

**Do not treat +2% as final tuning. Build it data-driven.**

Mounting:
- has a cast time;
- is impossible while the player has active aggro/threat;
- taking damage while mounted dismounts.

Mounts are generally:
- allowed in open world;
- disabled in dungeons;
- disabled in buildings/caves/special areas unless explicitly allowed.

Road surfaces provide a movement-speed bonus simply for standing/moving on the road surface.

Road speed stacks with mount speed.

The road bonus remains active **in combat**.

No need to detect whether travel direction follows the road spline.

---

# Account / Echo scope quick reference

Usually Echo-wide:
- Way discovery;
- class milestone evidence;
- shared bank;
- banked currency;
- profession progression;
- Codex;
- achievement state;
- achievement score;
- transmog collection;
- mount collection.

Usually manifestation-specific:
- current class/level;
- carried inventory;
- equipped gear;
- carried currency;
- resurrection point;
- ordinary quest/world progression;
- map exploration.

Individual content can explicitly define exceptions.

---

# Current GitHub implementation/design issues worth knowing

The repository has broad design/implementation issues covering:
- World 1
- class branches/progression
- class discovery/UI
- Cornberg vertical slice
- combat
- quests
- persistence
- inventory/items
- dungeons
- multiplayer
- enemy/encounter framework
- summons
- cube traversal

Recent dedicated issues also include:

- Codex / completion framework
- expandable shared bank/search/storage progression
- Echo-wide transmog collection / NPC service
- mount / road-speed / trivial-enemy aggro framework

**Read issue comments, not only issue bodies.**

---

# Known provisional / open areas

Do not accidentally treat these as final:

- exact Cornberg Q3 enemy name/tuning;
- many exact stat numbers;
- exact resurrection-sickness tuning;
- exact bank expansion pricing;
- exact mount speed bonuses/caps;
- exact road speed bonus;
- exact aggro scaling beyond initial 10-level working threshold;
- later dungeon mode designs;
- final art direction/production rig;
- final HUD polish;
- controller implementation;
- networking implementation;
- Steam/cloud;
- cube-edge traversal solution.

---

# Validation discipline

Run validation appropriate to touched systems.

Examples:
- quest flow changed -> rerun Cornberg quest/progression tests;
- combat changed -> rerun focused combat tests;
- navigation/scene changed -> rerun traversal routes;
- persistence changed -> rerun reload/migration/recovery tests;
- broad code change -> produce Windows development build where practical.

Known Unity editor issue:
- SearchDatabase `ArgumentOutOfRangeException` can appear in Unity 6000.6.3f1 batch startup.
- Validation may recognize that exact known editor-only issue.
- Never use it to hide unrelated new errors.

Do not claim a manual playthrough unless a human/manual playthrough actually occurred.

---

# Required issue comment format after implementation work

When a task is completed or paused, relevant issue comments should say:

- branch;
- commit SHA;
- what was implemented;
- what remains;
- what is provisional;
- tests/validation run;
- known risks;
- recommended next increment.

---

# Agent continuation checklist

When entering a new chat/session:

1. Read this entire file.
2. Inspect latest `setup/unity-project` docs.
3. Inspect latest `poc/cornberg` commit history.
4. Read relevant open issues and comments.
5. Confirm whether the previous agent's reported work actually landed.
6. Update this file if the repo is ahead of it.
7. Continue the next smallest coherent step.
8. Update docs/issues/this handover again before ending the cycle.

---

# Architecture / 3D pipeline decisions — 2026-10-01

Canonical:
- `docs/ARCHITECTURE.md`
- `docs/ART_PIPELINE.md`
- `docs/adr/0001-repository-and-build-distribution.md`
- `docs/adr/0002-runtime-modules-and-asmdefs.md`
- `docs/adr/0003-3d-source-and-export-pipeline.md`

Axel wants architecture enforced early so DiceFree does not grow into another monolith.

Settled:
- implementation agents must read/check `ARCHITECTURE.md`;
- material architecture changes use ADRs;
- current absence of first-party `.asmdef` files is explicit prototype debt; migrate incrementally before major post-Cornberg expansion;
- gameplay domains should not depend on UI, Editor or concrete persistence implementations;
- authored ScriptableObjects are definitions, not mutable player/session truth;
- prefer typed semantic facts/commands and deterministic random/time seams;
- production scene direction is prefab-first and additive where appropriate;
- debugging and automated validation are architecture requirements;
- current reward code's direct dependency on concrete persistence is a known boundary to clean up during asmdef hardening.

3D/art:
- stylized moderately exaggerated fantasy readability, broadly WC3/Magicka-like in feel but original;
- equipment slightly oversized for isometric readability;
- first custom model is exactly one one-handed sword; later shield, shoes, hat;
- 1 Unity unit = 1 meter; DCC scenes metric;
- native DCC source under repository-root `SourceArt/`, outside Unity `Assets/`;
- Unity-ready exports under `Assets/_DiceFree/Art/`;
- binary source/exports use Git LFS;
- keep one repository for now;
- no runtime-only Git branch;
- player/tester builds are release/build artifacts, not repo clones;
- reconsider a separate art repo only after measured size, permission, LFS or CI pressure;
- first sword should be humble/chunky Cornberg metal, not ornate hero gear;
- rarity may escalate through geometry/materials/VFX; endgame gear may glow/use particles/trails while keeping combat readable;
- no baseline weapon-sheathing system; held-all-the-time is acceptable;
- class forms are not human-only: Tier-2 Ranger = Wood Elf and Tier-2 Wizard = High Elf; Berserker-as-Orc and a Tier-4 Centaur tank remain tentative;
- character presentation must support multiple rig/form families and even non-biped forms through semantic socket/form adapters.

Do not mass-produce art until the sword proves scale/export/import/pivot/material/prefab validation conventions.

Additional settled class/form/equipment rules:
- each Way has **one authored body/sex presentation**: visibly male, visibly female or androgynous;
- no requirement for male/female variants of every class;
- forms generally become progressively more class-specific/distinct at later tiers;
- equipment visuals do **not** auto-fit/morph across incompatible forms; by default an appearance is compatible with its authored class/form **and descendants**, with explicit descendant overrides allowed;
- all forms keep the same 11 semantic equipment slots;
- items may occupy multiple slots; two-handed weapons occupy Main Hand + Off Hand;
- use a general occupied-slot data model rather than scattered two-handed checks;
- ADR-0004 records the class-form/equipment-compatibility decision.

Tracking:
- **#36** architecture hardening / asmdef dependency enforcement — now the immediate next implementation and explicitly allowed to be a substantial controlled refactor;
- **#37** first stylized sword / 3D pipeline proof.

# Current recommended next implementation step

**Immediate next implementation step: issue #36 architecture hardening.** Axel explicitly wants the asmdef/dependency-boundary work done now, before MSQ5 or further abilities/resources/classes. Preserve all current behavior while moving toward the architecture contract incrementally and keeping the project green after each boundary.

Q4 pacing remains pending human review: elite route reaches level 9/10 XP and the 30-Road-Slime route reaches level 13/30 XP from level 6/0 XP. Do not rebalance it during architecture work.

Latest completed gameplay implementation: **39ebbf29c809bf08e7d4bc2626f80714159e85e5**, Q4 with reusable OR objectives, nearby quest credit, barks, reward integration and FFA shoe pickup. MSQ5 waits until architecture hardening is complete enough to safely resume feature growth.

# Latest handover update

- **Date:** 2026-10-01
- **Branch updated:** `setup/unity-project`
- **Confirmed Q3:** `4289854b096b790cbb8df847f7e4c8309b2033d7`
- **Confirmed Runner Returns:** `2de1a58703e845e1018bcdd5cfdf3f185f16a916`
- **Runner status:** implemented with reusable acknowledged-conversation / TalkTo semantic credit and existing schema-v2 persistence
- **Confirmed trivial aggro:** `e6d5cefc9d594b1ed2a8fc4cd9ed06a9da3ed7c8`; provisional ten-level awareness suppression, authored always-aggro and policy extension seam; focused/regression/build/smoke validation passed
- **New design recorded:** achievement categories + permanent reveal after hidden achievements unlock; precise manifestation-specific map exploration with allied reveal; possible Echo-wide POI icon knowledge; road speed remains active in combat
- **Q4 design now settled:** both routes active simultaneously; elite Slime OR 30 post-accept eligible non-crop Slime kills; no retroactive elite/kill credit; dungeon is side content but eligible dungeon Slimes can count; elite guidance says somewhere in Slime Forest; elite is a stat check only; either route sets ready-to-turn-in back in Cornberg; same Q4 reward either route; target around level 9–10 with exact XP tuned by playtest.
- **Q4 elite:** fixed deep-forest clearing, repeatable/farmable, 7.5-minute respawn.
- **Q4 co-op credit:** eligible nearby party members with Q4 active share qualifying ordinary Slime kills and the elite kill using authored Nearby policy; Q4 radius is **50 meters**.
- **Q4 reward:** XP + gold (amounts tuned by playtest) + guaranteed authored Hands/gloves with **+5% Attack Speed**. Glove name/item level/rarity/art remain provisional; no extra gameplay stats.
- **Q4 giver:** a different named Cornberg farmer, present as ambient flavor before Q4; exact name/art/barks remain open.
- **Q4 authoring status:** provisional Mira/elite/rewards are implemented; 50m Nearby credit and 20% FFA shoe drops are validated. Next is playtest/pacing review, not reimplementation.

- **Confirmed road speed:** `9f1d6f8634813cb8f2ab2a68e7c81121d8c5597f`; +15% provisional, focused/regression/build/two standalone smokes green. Mount work remains unstarted.

- **Confirmed item foundation:** `ba29d5a46f6f126bacb8b5fb5365196f98393423`; originally schema/records v3, now migrated to v4 by Q4.
- **Confirmed Q4:** `39ebbf29c809bf08e7d4bc2626f80714159e85e5`; focused/regression/build/two standalone reload smokes green. Issues #6/#15/#22/#28/#24/#26 updated. MSQ5 is not implemented.

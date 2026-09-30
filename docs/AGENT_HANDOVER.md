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

## Latest known Codex work before usage-limit interruption

Last visible Codex update reported:

- Q3 content was authored.
- The farmer offers Q3 after Q2.
- The objective leads to the existing southern woodland clearing.
- A larger fixed-level green Slime is used.
- It uses shared combat, respawn, kill-credit and XP pipelines.
- Its current display name and tuning are explicitly provisional.
- Focused test work was exercising:
  - the route;
  - an actual duel;
  - NPC acceptance/turn-in;
  - four reload checkpoints.
- Farmer conversation selection was fixed so finishing the latest quest no longer falls back to Q1.

**Important:** Codex hit its usage limit immediately after this report.

The next implementation agent must inspect the latest `poc/cornberg` commits and issue comments to confirm exactly what was pushed/landed before continuing.

---

# Current work in progress

Primary active implementation direction:
- verify Q3 landed state;
- finish/validate Q3 if needed;
- continue the next smallest coherent Cornberg progression beat.

The current slice should keep using real game systems rather than quest-specific hacks.

---

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

Hidden achievements are supported.

Achievement score/points exist as an overall completion/bragging metric.

Exact reward policy can vary by achievement.

---

# Exploration / map

Unlike Codex/achievements, **map exploration is manifestation-specific**.

Every manifestation must explore the world for itself.

Discovered POIs should appear on that manifestation's map.

Some deliberately secret content may use special marker rules.

Fog/map reveal details remain open unless another doc settles them.

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

# Current recommended next implementation step

At the time this handover file was created:

1. Inspect latest `poc/cornberg` state.
2. Confirm Q3 implementation and validation actually landed after the last Codex session.
3. Finish/fix Q3 if necessary.
4. Update this file with the confirmed Q3 commit and status.
5. Continue with the next smallest Cornberg progression increment.

---

# Latest handover update

- **Date:** 2026-09-30
- **Branch updated:** `setup/unity-project`
- **Purpose:** first durable agent handover
- **Current implementation activity:** Cornberg Q3 / post-Q2 progression
- **Last external Codex state:** Q3 authored and undergoing focused route/duel/NPC/reload validation when usage limit interrupted the session
- **Next agent responsibility:** verify repository truth before assuming that work completed

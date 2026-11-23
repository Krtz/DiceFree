# Codex

## Ownership

Codex progression is **Echo-wide**.

Switching manifestation/class does not create a new Codex.

Map terrain exploration remains manifestation-specific and is not Codex progression.

## Discovery principle

The Codex records what the player has actually discovered.

Do not spoil unseen content with mystery placeholder rows. If the Echo has not discovered something yet, it normally does not appear.

## Monsters
## Hidden class teachers (USER-APPROVED, 2026-11-23)

**Every hidden teacher for ANY class**, not just the Psion lineage, earns a permanent **Echo-wide Codex discovery** when actually found by a player manifestation. This discovery is independent of which class is exploring, and remains visible after changing, archiving or deleting class-specific manifestations because it belongs to the Echo. The hidden teacher discovery itself must NOT automatically grant its target class or count as a completed advancement challenge.

- Stable hidden teacher identity: one idempotent discovery keyed by **teacher ID**. The Codex shows genuinely **discovered teachers only**, consistent with the global discovery principle: no silhouettes, question-mark filler entries, or spoiled teacher lists before discovery.
- A hidden teacher's **discovered** status is distinct from knowing rumors, seeing lore clues, meeting class-level eligibility, starting an advancement quest, and **finishing** its challenge. Those may be separate records/requirements; "discovered" must be tied to an actual teacher-find event, not automatically triggered by merely reading a rumor, unless separately authored and approved.
- For **Psionic Knight** and **Mirage Mage** specifically, exploration uses rumors, lore and environment clues to find **separate teachers**. Their advancement trials are respectively **solo Wolf Cave completion** and **solo Shaman Cave completion**. Trial progress must not be silently treated as the same thing as teacher discovery. See [Psion Tier III Round 17](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md).
- Codex dungeon section already records **solo clears**. Consume the ordinary dungeon completion+solo participation event to provide advancement evidence when the relevant trial is defined, without modifying generic dungeon reward rules. **Wolf Cave's precise identity is currently unconfirmed**, because existing Wolf Survival is explicitly a moonlit clearing; do not assume the same dungeon. Shaman Cave content is not yet verified/located. Whether past solo clears retroactively satisfy a newly discovered teacher's request remains OPEN.
- Suggested semantic-event name for future implementation: `hidden_teacher_discovered(teacherId, echoId)` as an **interface proposal only**; do not assume an existing code event or API. Use standard Echo-wide durable-state save transactions and idempotent discovery handling when implementation is separately authorized.


Monster families may group related variants, but exact variants keep exact records.

An exact monster/variant is discovered when the Echo kills it.

Track at least:
- exact monster ID;
- kill count;
- observed drops;
- witnessed boss mechanics where applicable.

## Drops

A drop is added only after it has actually been observed dropping.

For each discovered drop track:
- stable drop/item ID;
- times seen drop.

When all authored drops for an entry have been observed, UI may state that all drops are discovered.

Undiscovered drops remain absent rather than occupying mystery slots.

## Boss mechanics

Boss mechanics are recorded when witnessed.

Store stable mechanic IDs and let authored content provide the player-facing text.

Mechanic discovery is idempotent: seeing Slime Slam fifty times still discovers one Slime Slam mechanic entry.

Mechanics may later be grouped by phase/state.

## Dungeons

Track at least:
- clears;
- wipes;
- bosses killed, including counts by boss ID;
- player deaths;
- completed modes;
- solo clears;
- no-death clears;
- fastest clear separately by party size;
- loot discoveries;
- authored secret route/room/boss discoveries.

Normal clean/gold dungeon completion requires:
- every authored mode completed;
- every authored drop observed.

Specific dungeons may add extra completion requirements or rewards.

## Rewards

Optional per-entry rewards may include:
- title;
- cosmetic;
- achievement;
- tiny damage bonus against the **exact mob**;
- combinations;
- nothing.

Exact-mob damage bonuses must remain tiny and tightly capped.

Do not create broad family damage bonuses that turn Codex completion into mandatory endgame optimization.

Rewards trigger automatically when their authored requirement is satisfied.

## Semantic events

Codex should consume semantic gameplay events rather than being hard-coded into each encounter.

Initial event vocabulary:
- monster killed;
- drop observed;
- boss mechanic witnessed;
- dungeon wiped;
- dungeon player death;
- dungeon boss killed;
- dungeon secret discovered;
- dungeon completed.

Quest, achievement, class-requirement and Codex consumers can eventually listen to the same semantic event stream where their eligibility rules overlap.

## Persistence

Codex uses the generic Echo-wide durable-state contract.

Current section:
- ID: echo:codex;
- version: 1.

It is additive to existing Echo saves.

Missing Codex section means an empty Codex and is not save corruption.

Class fork/switch/autosave commits include Echo-wide durable sections atomically with the rest of the Echo revision.

## First UI

The first UI is deliberately functional.

Initial tabs:
- Monsters;
- Dungeons.

It shows discovered entries only.

Final art, search/filtering, family navigation and completion presentation remain later polish.

## First authored content target

Start with:
- Cornberg slime family;
- first Slime Dungeon;
- first Slime Boss;
- Slime Slam;
- Divide;
- high-level Slime Regent route discovery;
- dungeon loot discoveries.

Slime Queen / Slime King remain later high-level hard-boss Codex content.
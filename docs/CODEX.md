# Codex

**2026-11-23 hidden-teacher discovery and map presentation (latest Round19, USER APPROVED):** Any hidden teacher for ANY class is permanently discovered by any manifestation within **2m** and recorded in this **Echo-wide Codex** even if that manifestation cannot accept their quest. The **Codex is the only automatic UI record** of teacher discovery for now: **NO map pins/markers** are generated from this Echo-wide knowledge, and teacher positions may be moved freely by the Codex development agent within World 1 or World 2 during world construction. For the two Psion Tier III teachers, interaction/quest access is restricted to **Tier II Psion level60+**; wrong-class and below-level encounters get different rejection dialogue. Either eligible Psion may have BOTH advancement quests active concurrently. Each quest needs a newly initiated **SOLO dungeon completion** after quest acceptance on the same Psion, followed by **RETURN TO ITS TEACHER** for advancement; Codex solo-clear records alone cannot fulfill quest proof. Knight: Wolf Survival: Moonlit Clearing; Mage: EXISTING Quillboar Shaman/Fourfold Hollow dungeon, which has a Unity scene and QA handover on the current branch. See [Tier III Round19](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md).

## Ownership

Codex progression is **Echo-wide**.

Switching manifestation/class does not create a new Codex.

Map terrain exploration remains manifestation-specific and is not Codex progression.

## Discovery principle

The Codex records what the player has actually discovered.

Do not spoil unseen content with mystery placeholder rows. If the Echo has not discovered something yet, it normally does not appear.

## Hidden class teachers (USER-APPROVED; latest Round 18, 2026-11-23)

A player manifestation finding **ANY hidden class teacher for ANY class** records a durable, **ECHO-WIDE CODEX discovery** keyed by stable teacher identity.

- **Exact finding trigger:** the player-controlled manifestation comes **within 2 meters** of the teacher (distance ≤ 2.0m), whether the player talks to that teacher or not. No interaction, conversation, class level or quest acceptance is required. Discoveries are idempotent and saved Echo-wide, persisting across manifestation switches, archiving and deletion.
- **Never spoil unseen teachers:** undiscovered hidden teachers are absent from the Codex; rumors, lore clues and environment hints help exploration but **do not count as finding** a teacher until the actual physical proximity trigger is met.
- **Teacher knowledge versus advancement:** discovery does **NOT** accept an advancement quest, grant the resulting Way, or complete its dungeon trial. The related quest must be specifically accepted by the qualifying manifestation. General Echo-wide Codex dungeon solo-clear history is informational, not automatic quest-completion proof.
- **Psion Tier III specific:** the Psionic Knight hidden teacher requires an eligible Tier II **Psion** to accept their quest and then **SOLO CLEAR the existing Wolf Survival: Moonlit Clearing dungeon**. The Mirage Mage teacher requires that same appropriate Tier II Psion manifestation to accept its *different* quest and then **SOLO CLEAR the Quillboar Shaman ritual cave/dungeon**. The run must START only AFTER that manifestation accepted the relevant teacher quest, and be completed solo by that same manifestation. **No retroactive clears**, even from the same Psion or another class in this Echo.
- **Dungeon ID and content guard:** Wolf Survival: Moonlit Clearing is established in `WOLF_SURVIVAL_DUNGEON.md`. The Quillboar Shaman cave is described in `QUILLBOAR_REGION_DESIGN.md` as a hidden cliff entrance whose permanent *per-character dungeon access* follows the Trimery scholar's four-element Shaman hunt turn-in; user identifies this as the existing dungeon, although the historical region documentation had called its implementation future. Verify its current runtime dungeon identifier before quest wiring; **do not create a duplicate cave or waive its scholar access gate**.
- Suggested semantic discovery event is `hidden_teacher_discovered(teacherId, echoId)` only as a design-interface example, not evidence an implementation exists. Use normal Echo-wide durable save and semantic event systems when authorized.

See [Psion Tier III design, Round 18](classes/PSION_TIER3_EVOLUTIONS_DESIGN.md) and [Class Requirements and Discovery](CLASS_REQUIREMENTS_AND_DISCOVERY.md).

## Monsters

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
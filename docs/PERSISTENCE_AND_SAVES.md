# Persistence, Saves, Users and Cloud

## Philosophy

DiceFree should autosave aggressively and recover safely.

The save system protects player progress and data integrity; it is **not** an anti-cheat system.

There is no competitive leaderboard/economy requirement that justifies hostile save protection. If a player deliberately restores an old local save to alter their own progression, that is not a design problem we need to police.

## User -> Echo -> manifestation hierarchy

DiceFree separates:
1. **user/account identity**;
2. one or more **Echo profiles** owned by that user;
3. many class manifestations/save-states inside each Echo.

### Multiple Echoes, intentionally unobtrusive

A user may own multiple fully separate Echo profiles.

However, the normal player experience should treat the current Echo as **the game/profile**, not repeatedly ask the player to create/select one.

Default UX direction:
- first launch creates/uses one primary Echo;
- normal **Continue / Play** resumes that Echo;
- creating or switching to another Echo lives in a deliberate profile-management/settings path;
- do not put a prominent "New Echo" flow in the normal start loop;
- require clear confirmation before creating a fresh Echo.

This preserves clean-start/replay flexibility without letting new players accidentally fragment their progress across many Echoes.

## Save ownership model

Persistence is split conceptually by ownership even if serialized into one package.

### Echo/account-wide state

Examples:
- shared bank;
- banked currency;
- discovered Ways;
- permanent class/tier milestones;
- Echo-wide unlock/event records;
- professions;
- account-wide recipes/discoveries;
- account settings/cosmetics where appropriate.

### Manifestation/class-save state

Each class ID has at most one persistent manifestation.

Examples:
- level / XP;
- skill progression;
- equipped gear;
- carried inventory;
- carried gold/currencies;
- manifestation-specific quests;
- manifestation-specific world/event state;
- registered resurrection point;
- class-local resources/state that should persist.

### Session-instance state

Examples:
- current host-session world instance;
- active dungeon run;
- transient encounter state;
- temporary party/session data;
- staging-room countdown;
- active boss/trash state.

Session-instance state is normally **not** part of permanent player saves.

## Autosave

There is no traditional required manual Save Game flow.

Autosave should occur on meaningful durable changes such as:
- quest/event progress/completion;
- XP/level changes;
- loot/item acquisition/removal;
- gold/currency changes;
- bank transactions;
- resurrection-point changes;
- profession progression;
- Way/class milestones;
- class advancement;
- inventory/equipment changes where practical;
- other durable progression changes.

Autosave implementation may batch nearby writes for performance, but design semantics should behave as though completed durable operations are saved promptly.

## Load/spawn position

World position is not the authoritative persistent spawn location.

When loading/starting a class manifestation:
- spawn at that class save's latest registered resurrection point;
- if unavailable/invalid, use the established fallback (Cornberg).

Quitting in the middle of a forest does not resume at the exact forest coordinate.

## Load-state reset policy

Loading a manifestation is a fresh combat/session start at its registered resurrection point, not a frame-perfect restoration of the previous session.

### HP

A loaded manifestation starts at **full HP**.

This applies whether the previous save snapshot was alive or dead.

Saved current HP is therefore not authoritative across a normal load.

### Return-to-revive ability

DiceFree includes an explicit player ability/action that returns the current manifestation to its **registered resurrection/revive point**.

Settled behavior:
- **10-second cast**;
- cannot be started while the player has active aggro/threat;
- taking damage interrupts the cast;
- disabled during an active dungeon run;
- target cooldown is roughly **10 minutes**;
- the cooldown **resets on death, logout, and reload**;
- the cooldown is therefore not part of durable manifestation persistence.

This is distinct from an unstuck feature.

The destination comes from the manifestation's current registered resurrection point rather than a hard-coded town.

Do not persist this cooldown in save data unless the design is deliberately changed later.

### Cooldowns

Ordinary ability and consumable cooldowns reset on load.

The framework may support explicitly persistent long-duration cooldowns later, but persistence must be opt-in rather than assumed.

### Buffs / debuffs

Ordinary timed buffs, debuffs, crowd control, poisons, temporary combat effects and similar session effects reset on load.

If a world/seasonal/event state should continuously grant a buff, represent the underlying event/world source and re-apply the effect when appropriate rather than persisting the transient buff instance itself.

Example:
- an event can spawn/activate a hidden global aura source;
- loading reconstructs world state;
- the aura then applies the buff normally.

### Resources

Resources **reset on load by default** according to their authored reset/load policy.

The resource framework must also support explicit persistence for unusual resources whose acquisition is intentionally durable/rare.

Example:
- a rare resource earned only from a difficult special kill may opt into persistence.

Do not infer persistence merely because a value is called a resource.

### Combat/session state

Do not persist:
- threat lists;
- targets/selections;
- aggro;
- casts/channels;
- projectiles;
- ordinary temporary summons;
- enemy current HP/state;
- transient encounter state;
- ordinary combat cooldowns;
- ordinary temporary effects.

### Persistent companions

Manifestation-persistent companions reload with their owning manifestation at the resurrection point.

Persist durable companion state such as:
- stable identity;
- equipment;
- authored persistent traits/state.

Reset moment-to-moment combat state such as:
- current target;
- threat;
- casts;
- temporary buffs/debuffs;
- transient positioning/combat commands.

## Dungeon persistence

Active dungeon/raid run state is **not persisted across quitting**.

Quitting/leaving an active run abandons that run.

Persistent rewards already legitimately earned before quitting remain saved.

In particular:
- XP earned remains;
- gold earned remains.

Dungeon equipment rewards still follow the separate completion/private-loot-room rules.

Loot-room reconnect is a short-lived session recovery mechanism, not permanent dungeon-save persistence.

## Atomic / transactional operations

Operations that move/create important durable state must be atomic.

### Advancement

Class advancement should commit as one logical transaction:
1. validate advancement;
2. snapshot parent manifestation;
3. create/update child manifestation;
4. record Echo-wide milestones/unlocks;
5. commit.

A crash/failure must recover to a consistent before-or-after state, never a half-created manifestation.

### Bank / inventory transfers

Moving items/currency between manifestation inventory and Echo-wide bank must be atomic.

A crash cannot duplicate or destroy an item through partial transfer.

The same principle applies to other ownership-changing operations.

## Timer clock domains

Timed content should declare what clock it uses.

Supported timing semantics should include:
- **session/gameplay time** — advances only while the relevant session/content is active;
- **played time** — advances while the player/profile is actively being played, according to authored rules;
- **wall-clock time** — based on real-world time and can elapse while the game is closed.

DiceFree does **not** currently plan to depend heavily on weekly/daily live-service events.

Wall-clock support exists so unusual future content can use it without corrupting the general save model.

Ordinary combat timers/cooldowns are not wall-clock persistent.

## Stable IDs and save versions

Persistent references use stable IDs, not display names.

Save data includes explicit schema/version information.

Renaming presentation text must not break saves.

When data structures change:
- migrations upgrade old save versions;
- migrations are explicit/testable;
- a save is not assumed to magically match current runtime data.

## Unknown/missing content safety

If a save references content that the current build cannot resolve:
- do not silently delete the record;
- preserve the unknown serialized record where practical;
- mark it unresolved/inert;
- surface debug/migration diagnostics;
- restore it if a later migration/content definition can resolve it.

This is especially important during development when class/item definitions may be renamed/reworked.

## Backups and recovery

Maintain rotating automatic local backups of recent successful save revisions.

Goals:
- recover from corruption;
- recover from interrupted migration/write;
- avoid one bad write destroying a large Echo/class roster.

Exact backup count/retention remains implementation tuning.

## Cloud saves

Cloud persistence should be designed behind an abstraction rather than hard-wired to one provider.

Initial PC direction includes Steam Cloud support.

Cloud sync should operate on the player's save package/profile and support:
- stable user/profile identity;
- save revision/version;
- timestamp;
- checksum/integrity metadata;
- conflict detection;
- explicit/local-safe resolution behavior;
- restoration from local backup where needed.

Active multiplayer/dungeon session state is not uploaded as permanent account progression.

## Cloud revision unit and conflicts

Each **Echo profile is one logical cloud revision unit**.

Internally, its data may be chunked into sections, but cloud sync/version/conflict semantics should treat the Echo as one coherent revision.

This avoids impossible merges such as:
- one machine updates Ranger inventory;
- another updates Wizard inventory;
- both independently modify the same shared bank.

### Divergent save conflicts

Do **not** attempt automatic field-by-field merging of two independently changed Echo revisions.

When local and cloud histories diverge:
- show clear revision metadata such as timestamp, progression summary and save version;
- let the user deliberately choose which revision becomes current;
- preserve backups of both sides before resolving where practical.

Protect progress rather than silently guessing.

## Users / identity / database direction

Prepare a provider-neutral user/profile layer.

A user/account should have:
- stable internal **user UUID**;
- linked platform identity/identities where supported;
- one or more Echo profile IDs;
- cloud/account metadata;
- optional future service/database metadata.

Each Echo profile should have:
- stable internal **Echo UUID**;
- display/profile metadata;
- current cloud/local revision;
- save schema version;
- checksum/integrity metadata;
- timestamps;
- deletion/recovery state where applicable.

Initial PC identity can use Steam authentication, but DiceFree's internal UUIDs remain authoritative identifiers.

Do not make gameplay systems depend directly on Steam IDs or a specific backend vendor.

### No required DiceFree login at PC launch

The initial Steam PC version should **not** require a separate DiceFree email/password account.

Steam identity is enough for the normal first-party PC login flow.

The identity layer should still permit attaching additional providers later without changing gameplay save IDs.

If a future database/backend is introduced, it should store service/account metadata and cloud-save coordination cleanly rather than becoming the authoritative runtime combat/world database.

The game should remain playable from local durable saves without requiring a permanent MMO-style backend.

## Database scope

If/when an online database is introduced, its initial purpose should primarily be **account/service metadata**, not authoritative moment-to-moment RPG state.

Good database candidates:
- internal user UUID;
- linked platform identities;
- Echo profile IDs;
- cloud revision metadata;
- timestamps/checksums;
- deletion/recovery metadata;
- future friend/service metadata.

The actual Echo game state can remain a versioned save package/blob in cloud storage.

Do not normalize every quest flag, sword stat and Slime kill into an online relational database unless a future feature genuinely requires it.

Player saves remain authoritative for ordinary progression; there is no server-authoritative progression requirement for leaderboards/economy/PvP because those systems are not planned.

## Multiplayer persistence

The host controls the **live physical presentation** of the shared session.

That does not mean guests fail to progress.

### Persistent credit for shared events

When a guest legitimately participates in and is eligible for a quest/world event, that event may persist to the guest's own manifestation/Echo according to the event's persistence policy.

Example:
- host and guest both participate in the event where Bob's house burns;
- both are eligible and receive completion/world-state credit;
- both personal timelines record that outcome;
- either can later host a timeline where the house is burned.

### Merely observing host state is not completion

If the guest joins **after** the host already completed the event:
- guest sees the burned house because host state controls presentation;
- guest does not automatically receive that event/milestone;
- guest's own timeline remains unchanged unless the event has another authored credit path.

Thus long-term co-op can progress both players while still preserving different histories for late joins/missed events.

### Persistence-recipient policy

World/quest events should be able to define who records their persistent outcome, for example:
- host only;
- all eligible participants;
- eligible nearby participants;
- explicitly credited participants;
- other authored policy.

Default ordinary cooperative progression should favor **eligible participants**, not host-only persistence.

## Offline and LAN play

The game must support offline play.

Local autosave, class progression, quests, inventory and other non-online gameplay continue without internet access.

When connectivity returns, cloud sync reconciles revisions through the normal conflict rules.

Local-network multiplayer should also be architecturally possible **without internet/cloud availability**.

LAN/session discovery/transport details remain technical implementation work, but account/cloud services must not be a hard dependency for local multiplayer gameplay.

## Echo deletion / recovery

Deleting an entire Echo profile should not immediately and irreversibly destroy every cloud/local copy.

Direction:
- explicit confirmation;
- mark profile deleted;
- retain a recoverable cloud/local tombstone/backup for a limited period;
- allow permanent purge later.

Exact recovery duration remains open.

This is separate from deleting/archive of a class manifestation inside an Echo.

## Cross-platform identity direction

Internal user and Echo UUIDs must allow another platform identity/provider to be linked later.

Steam is the first provider, not the database primary key of reality.

Actual cross-platform release/support remains future scope.

## Save package structure

An Echo is one logical revision, but its storage should be **internally chunked/sectioned**.

Candidate sections:
- Echo header/shared milestones;
- bank/shared inventory;
- professions/account progression;
- per-manifestation records;
- quest/world state;
- settings/profile metadata;
- unresolved/legacy records.

Goals:
- targeted migrations;
- easier corruption recovery;
- easier debugging;
- avoid one gigantic opaque serialized object as the class roster grows.

A physical implementation may still package these chunks into one archive/blob for atomic/cloud operations.

## Developer Profile Inspector

Provide developer/debug-only tooling to inspect an Echo/profile.

Useful information includes:
- internal user/Echo IDs;
- save/schema version;
- cloud/local revision;
- last successful write;
- backup history;
- migration history;
- manifestation/class IDs;
- unresolved records;
- pending/last transactions;
- bank/inventory ownership;
- item generation/budget provenance;
- event/quest persistence state;
- integrity/checksum status.

This is **not** normal player-facing UI.

It exists for Axel, developers and development agents/tools such as Codex to diagnose and safely evolve persistent data.

## Quitting as escape / multiplayer leave semantics

### Solo

No combat logout timer is required.

Because normal loading returns the manifestation to its registered resurrection point at full HP, quitting can function as an escape from danger.

This is acceptable for a non-competitive game and does not require anti-abuse machinery.

### Multiplayer

Intentional **Leave Session** during combat should not become an instant invulnerability/escape exploit.

Direction:
- use the same or equivalent vulnerable exit/disconnect grace behavior as an unexpected disconnect;
- the actor can remain present and vulnerable during the grace period;
- persistence still protects already-earned durable progress.

Exact multiplayer timing remains part of the reconnect/network implementation.

## Save-scumming / anti-cheat

No special anti-save-scumming system is planned.

Goals are:
- prevent accidental loss;
- prevent corruption;
- preserve transactional integrity;
- make migrations safe.

Not goals:
- preventing local backup restoration;
- competitive anti-cheat;
- leaderboards;
- policing how someone plays their own save.

## Local user controls settings (issue #47)

Control bindings live in `Application.persistentDataPath/user-controls-v1.json`, separately from Echo progression. The version-1 DTO contains stable catalog entry IDs and native Input System override paths. No Echo schema or manifestation records changed. ControlsComposition loads settings before scene gameplay Awake/OnEnable, then saves successful user binding changes.

LocalControlSettingsStore validates the whole candidate before applying overrides. Missing/corrupt/invalid files restore defaults, with recovery from `.bak` when valid. A newer version uses defaults, preserves the file and makes edits session-only. Load is silent with respect to save notifications. Writes use an exclusive `.lock`, flushed `.pending` file and atomic replacement with `.bak`; failures leave the prior primary available and report feedback. These files belong to local user preferences across Echoes. Cloud sync and additional settings categories are outside v1.

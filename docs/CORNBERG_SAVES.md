# Cornberg local save slice

This is the first local slice of issue #26, following Q1. The design documents
were reconciled from `setup/unity-project` at `cb3afd1`. Existing Cornberg geography,
navigation, combat tuning and Q1 remain intact. Two components were added to the
saved scene: persistence on the player and a stable ID on the resurrection anchor.

## Playtest behavior

- First launch creates one primary Echo with internal user and Echo UUIDs and
  starts at the existing emergence path.
- Level, XP, Q1 and the registered anchor autosave. Reopening resumes progression
  at the registered point; an unavailable point falls back to Cornberg.
- Manifestations use the actor definition's stable class ID, not a hard-coded name.
- Completed Q1 remains completed. Loading does not replay XP, level-up rewards,
  defeat credit or quest turn-in rewards.
- Enemy lives, aggro, targets, attack timers, arbitrary coordinates and dungeon
  sessions are transient. New sessions use authored enemy state.

Living HP fraction is preserved. A saved dead player loads with the existing
return fraction (50%). This policy is **provisional**, tracked in
[issue #29](https://github.com/Krtz/DiceFree/issues/29). Existing combat/return/well
tuning was not changed.

## Storage and recovery

Windows uses `Application.persistentDataPath/Profiles/primary-echo.json`.
Editor play uses a separate `EditorProfiles` directory. Runtime `SavePath`,
`Status` and `Revision` expose diagnostics; a full Profile Inspector is future work.

Each Echo is one logical revision with schema version, UUIDs, revision number,
UTC timestamp and SHA-256 checksum. Versioned sections hold JSON payloads.
Unknown sections, including future manifestations, round-trip without parsing.
Unresolved or incompatible quest records are retained inertly. An incompatible
authored quest shows a diagnostic rather than indexing an invalid objective or
offering rewards.

Writes flush a candidate then atomically replace the primary on the same local
filesystem. **Three** recent successful primary revisions rotate as backups.
Interrupted candidates are ignored. A corrupt primary loads the first readable
backup; replacing it preserves the corrupt file separately. If all copies are
unreadable, autosave stops and preserves the files. Unsupported schemas are never
silently downgraded to backups. Invalid manifestation state also stops autosave.
An on-screen message identifies stopped autosave; play remains possible.
A write lock and revision/identity check reject stale competing writers. This is
local file protection, not a cloud conflict-resolution implementation.

Gameplay events mark state dirty. A late-frame snapshot captures the whole
manifestation after synchronous XP/quest handlers complete. The provisional
debounce is **0.25 seconds**. Pause and orderly quit flush current state. A crash
inside that window may lose uncommitted progress. Passive regeneration is captured
on the next save/quit instead of writing every frame.

## Boundaries

`ExperienceProgression` and `QuestJournal` validate/restore detached records without
file I/O. `ManifestationPersistence` binds them to snapshots. `LocalEchoStore` owns
integrity and recovery. `ResurrectionAnchor` supplies stable identity and
`RespawnAtAnchor` resolves spawn positions and clears player combat/interaction state.

There is still one authored registered anchor. Additional-stone registration,
manifestation switching, profile management, Steam/cloud sync, bank/fork transactions,
multiplayer persistence and Echo deletion remain future work. No historical save
schema has shipped: future versions fail closed until explicit migrations exist.
Unknown fields inside recognized version-1 records are not a general forward-compatible
contract; changed records need new versions. This does not complete issue #26.

## Validation

`CornbergSaveValidation.Run` tests storage and a two-process save/reload fixture.
Pass a fresh absolute `-diceFreeSaveRoot` directory first, then the same root plus
`-diceFreeVerifyReload` in a second process. Checks cover:

- fresh emergence, level 1 and automatic saves after quest acceptance/XP;
- level/stat restoration without reward replay;
- stage-1, stage-2, ready and completed quest records;
- invalid counts and detached snapshots;
- completed reward idempotency across process restart;
- unknown quest/section preservation and anchor fallback;
- clean player combat state;
- checksum recovery, three backups and interrupted candidates;
- unsupported candidate and stale writer preserving the current revision;
- total corruption failing closed.

Batch runs disable persistence unless an isolated root is explicitly supplied.
Existing traversal/combat/Q1 editor validation entry points also disable persistence
to protect user files and fresh-character fixtures. Invalid root arguments never
fall back to a real profile. The known Unity SearchDatabase exception remains #17.

Validation on 2026-09-29 passed: both persistence process runs, the existing Q1
suite, combat/death/well suite and all 13 traversal routes. The Windows development
build succeeded (171,933,258 bytes). Two 12-second standalone launches against an
isolated profile produced revisions 1 and 2 with unchanged user/Echo identities
and no runtime/navigation errors. This is startup/reload smoke coverage, not a
complete standalone quest playthrough. Navigation and tuning assets are unchanged;
the scene diff adds 28 lines and removes none.

## Next smallest increment

Confirm reload HP behavior, then prove one real schema migration with a small
developer Profile Inspector. Transaction and cloud-conflict fixtures are separate
pieces of #26. Q2 is not implemented here.

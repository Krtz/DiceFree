# World 1 development consolidation

Canonical development branch: `integration/world1-foundation`, starting from `1769a4e` (latest visual-overhaul and Echo-name work).

Scope reviewed: all 24 GitHub branch heads, local branch heads and local tracked/untracked differences. The pre-consolidation branch and recovery inventories are under `docs/archive/pre-consolidation/`.

Resolution policy:
- The latest playable runtime, scene, body/animation, save-schema and approved design revisions remain authoritative.
- Unique original shield, shoes and hat source art, models, materials, prefabs and preview artifacts are recovered into their original paths.
- Branch-only old validation and superseded feature/47 Options code are preserved under `SourceArt/LegacyValidation/`, outside compiled Assets.
- Earlier bank/Codex/pacing documents are preserved as historical evidence in the documentation archive. In particular, the old linear-XP audit does not supersede the current quadratic curve.
- All prior branch tips are incorporated as Git parents, preserving original common-file versions and history. A history merge keeps the reviewed current working tree; unique usable work is restored explicitly before that merge.
- The 32 meaningful local first-party material differences (URP Quillrat palettes and early skill VFX) are captured in the consolidated baseline. Line-ending-only blockout changes normalize when staged.
- Local Package Manager preference differences are preserved in `local-project-preferences.patch` and remain on this PC; they do not change portable project defaults.
- Licensed downloads, licensed generated resources, user saves and machine-generated caches stay local. Their existing paths and ignores are retained; the missed licensed-audio folder meta is now ignored.

New foundation:
- Development room accessible from the main menu with its own test Echo.
- Real class/level/restore/item controls, three training targets and shared Greatblade visual previews.
- Separate World 2 test map with same-player scene travel and return.
- Reproducible authoring, isolated Play Mode validation and an all-enabled-scenes Windows Development build command.

Validation completed so far:
- Unity compilation succeeds.
- Architecture validation passes `DICEFRE_ARCHITECTURE_OK`.
- Development-room/World2 round-trip validation passes `DICEFREE_DEVELOPMENT_ROOM_PLAYMODE_OK`.
- Original recovered prefabs import and asset GUID checks pass.

The next development milestones and production direction are recorded in `WORLD1_FOUNDATION_MILESTONES.md`. This consolidation does not finish World 1 content or implement Greatblade combat.

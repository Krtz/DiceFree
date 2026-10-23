## 2026-10-21 — Greatblade/Mage rank-zero implementation checkpoint

- Continue on `integration/world1-foundation`; GitHub's read-only refs API confirmed the remote baseline `d088534a95fdf0704207da90286082d23d17d4c8`. Local commits `a82f1d4` (rank-zero Greatblade/Mage + schema-7 migration), `9e56885` (October 21 asset intake), and the separate handover update were pushed. GitHub's refs API then verified the branch at `900bb2223a1f121fd35fb1284214f8cdcec978cc`; this sentence is a follow-up correction to that handover commit. Regular sandbox `git fetch` was blocked, but the authorized push succeeded. No branch switch or main merge.
- The approved next implementation step is now in the working tree: Greatblade has 78 ranks and Mage 80; both start with every skill at rank 0, one point at level 1, one earned point per level up to total rank capacity, skill-specific gates, and restore validation against both gates and spend budget. Greatblade/Mage existing saves receive a silent, per-manifestation, idempotent one-time reset of only their known skill ranks. Save schema/manifest section version is now 7. Before first successful save after that reset, the exact verified source envelope is retained as `primary-echo.json.pre-skill-rank-v1-original`; normal `.bak1` rotation is also verified by the new Editor validator.
- Greatblade Mastery milestones are rank 3 Wide Arc haste, rank 5 widened cleave and rank 7 five-second Metal shield with `100 + 5% max HP` capacity. Elemental shields consume 0.8 shield HP per matching damage prevented and 1.2 for other/untyped ordinary damage; same-source reapplication replaces/refreshes. Mage approved caps are implemented for freeze duration, chain count, Half-Life DEF, Quicksand slow and Weaving aura; the current Quicksand ramp is 4 percentage points/rank to the approved 40% ceiling and is an implementation choice, not a separately approved curve. Math Barrier retains its existing formulas. HUD notification appears only when a rank can be purchased now; point totals remain visible in ability tooltips.
- Added `ClassSkillRankValidation` / Pipeline command `dicefree.skills.rank-migration.validate`, with exact gate schedule, allocation feasibility/cap, approved effect ceilings, schema-6 migration, preservation, idempotence and backup checks. Updated existing Mage/Greatblade gameplay harness expectations and adjacent save-schema validator assumptions rather than introducing another gameplay test flow. Unknown skill-rank entries are retained during normal active-manifestation saves. Static PowerShell parsing of the actual C# schedule literals confirmed six Greatblade schedules / 78 ranks and eight Mage schedules / 80 ranks; a legal purchase path exists at every level through each cap. CSV/JSON/review counts agree at 112 total / 52 new; `git diff --check` passed and no runtime `UnityEditor` references were found.
- **Unity compile, Editor validator, and Play Mode checks are not verified.** Unity CLI `1.0.0-beta.13` reports `STATUS_DESCRIPTOR_UNREADABLE` for the Editor discovery file; `unity command --project-path ... --query dicefree` returns `No Pipeline instance found`. The exact Unity 6000.6.3f1 Editor is stopped at its `Assets/_Recovery` modal asking whether to keep or delete scene backups; no backup was deleted and no Editor/process was restarted. Screenshot/UI automation had no accessible modal surface. The connected Desktop Commander device reported Offline (last seen 68h ago); command-line Unity licensing has historically failed before execution. Do not claim rank/Mage/Greatblade tests passed. Next: once the recovery choice is safely available, keep the backups, wait for compile, run `dicefree.skills.rank-migration.validate`, `dicefree.mage.test`, and `dicefree.greatblade.test`, then architecture/policy/managed-reference checks and a fresh Windows Development build if source compilation or regressions require it.
- Local asset catalog intake is now 112 files: the original 60 remain covered by the 2026-10-17 source-page audit; 52 newer archives have an outer CRC/structural inventory but no verified source-page/license. The candidate leads for Free Magical Assassin, Shrewd Scoundrel, and Elven Assassin Bow/Dagger are clearly marked unverified and held; no raw licensed assets were copied into public Git. Files: `docs/assets/downloaded-assets-2026-10-21.{json,csv}`, `docs/assets/review-2026-10-21.json`, plus updated asset catalog/credits.
- The prior VFX milestone is already complete and remains as recorded below: `dicefree.mage.vfx.test` 104 checks, Mage gameplay 127 checks, architecture/policy/managed refs passed and Windows build passed 0 errors / 3 warnings. Do not repeat that VFX work unless a regression appears.
- Preserve Axel's unrelated pre-existing `ProjectSettings/PackageManagerSettings.asset`, untracked `Assets/Settings.meta`, and `Assets/Settings/`; do not stage or revert them. The recoverable pre-edit checkpoint remains at `T:\TEMP\DiceFree-Options-Checkpoints\Oct21-Design-Sync-20261021`. Public changes should be committed by explicit path only. No main merge.

## 2026-10-20 — Mage VFX prototype completion

- Branch: `integration/world1-foundation`. Fetched and integrated the then-current `origin/integration/world1-foundation` at `3d53f28ee5074b5c7bdf4b3a60a9f03c45d48b5b` by fast-forward before continuing. Mage VFX implementation is commit `c2d3b8bed9dcc81780c27a0a159447b772c4b5c8`; this handover update follows it. No main merge.
- Created recoverable pre-edit checkpoint at `T:\TEMP\DiceFree-Options-Checkpoints\MageVfx-20261020-164054` (204 files / 8,716,751 bytes; patch, manifest and SHA-256 list). It includes the then-owned uncommitted WIP plus ignored licensed Mage VFX vendor/resources. The unrelated dirty `ProjectSettings/PackageManagerSettings.asset` and `Assets/Settings*` were excluded from the checkpointed patch where unrelated, were never staged, and remain untouched/uncommitted. Licensed vendor/resources remain ignored and are not in public Git.
- Completed the Mage VFX parenting, palette and credits pass: Radiation follows its target; Math Barrier follows its owner; effect lifetimes/cleanup are tracked; Quicksand particle profile is capped to the field; Radiation is green; both owned asset-pack sources are represented in the generated Credits book and `SourceArt/Credits/credit_manifest.json`. Source receipts and the local preparation script are tracked; generated licensed prefabs remain local.
- Connected Unity: Unity 6000.6.3f1, Unity CLI `1.0.0-beta.13`, Pipeline `0.8.0-exp.1`, project `T:\TEMP\DiceFree-Options`, Editor PID 18124 / port 7800. Pipeline access initially failed under the sandbox account because it could not read the Editor discovery file; using the authorized local Windows context restored `unity status` and Pipeline access. No Editor restart or unrelated Unity process termination was needed.
- Focused `dicefree.mage.vfx.test`: structured status `passed`, 104 checks, 13 screenshots; `Library/MageVfxResults.json` reports peak particles 90 and maximum effect span 16.266 m. Inspected the close isometric effect images and actual gameplay-camera captures. Ember, Frost, Arc Lightning, contained Quicksand, green Half-Life and Math Barrier read distinctly at the preview/game camera scale. Six review copies are in `T:\TEMP\DiceFree-MageVfx-Review-20261020`.
- `dicefree.mage.test`: structured status `passed`, 127 checks after inspecting its visual-ready Mage capture and resuming; includes puzzle progression, six elements, 3 chain targets, Barrier, walking/equipment and save/cleanup. `dicefree.mage.validate` returned `DICEFREE_MAGE_DATA_OK` (2 players, 8 abilities, 6 actives/elements).
- Architecture `DICEFRE_ARCHITECTURE_OK`, policy self-test `DICEFRE_ARCH_POLICY_SELFTEST_OK`, and managed-reference `DICEFREE_MANAGED_REFERENCE_OK` all passed.
- Fresh connected-Pipeline Windows x64 Development build passed: `Builds/MageVfx-20261020/DiceFree.exe`, executable 667,136 bytes, complete output 379,068,713 bytes, 41.991 s, 0 errors / 3 warnings. Warnings: two particle-renderer primitive meshes have Read/Write disabled (Unity currently auto-enables in build), and imported `Mage URP Smoke26` / `Mage URP Circle` object names do not match their hash filenames. No warning was promoted to an error.
- Startup smoke launched that exact player with isolated save root `T:\TEMP\DiceFree-MageVfx-Smoke-20261020`; it stayed alive after 15 s, initialized Unity 6000.6.3f1 / D3D11 / AMD RX 6800 XT, and started the player Pipeline runtime server. The process did not exit after a normal close request, so only the exact smoke-test player process (PID 4568) was stopped; Editor shutdown was not affected. The isolated root is available for inspection.
- No changes to gameplay design, skill ranks, rank migrations, or new classes. Next approved implementation step: implement shared rank-zero/one-point-at-level-1 skill progression and the confirmed Greatblade/Mage weighted caps, with safe one-time silent rank reset/refund migration preserving all other manifestation data; use `docs/CLASS_PROGRESSION.md`, `docs/classes/MAGE.md`, and `docs/TIER2_GREATBLADE_MAGE_SKILL_RANK_AUDIT_2026_10_19.md`. Keep Acolyte unresolved decisions and do not start new classes.
- Preserve the remaining unrelated uncommitted `ProjectSettings/PackageManagerSettings.asset` and `Assets/Settings*`; no main merge.

## 2026-10-17 - Complete Sketchfab page-license audit

## 2026-10-23 — Cornberg first-player fixes in progress

Active checkout: `T:\TEMP\DiceFree-Options`, branch
`integration/world1-foundation`, base HEAD `ceba89cead42e2d4c05bcfa7c226bc0863aa5cc8`.
The current working tree contains user-owned visual/design documentation and
Package Manager settings edits from before this pass; keep them. New, still
uncommitted prototype fixes add a Graphics options tab (resolution and four
window modes), turn off player-build Pipeline auto-start to prevent the
read-only install-folder descriptor error, keep time running while unfocused,
scope hover tooltip display to the current hovered control, show only active
and ready-to-turn-in quests in the sidebar, and fade all renderers belonging
to an occluding tree. Mountain entry/return uses the shared Interact binding
(default I).

Quest and item changes add the repeatable `Another 15 Slimes` objective with
400 XP (20 road slimes at the authored 20 XP each), credit dungeon trash slimes
through the dungeon event hub, and migrate the existing v1 slime quest progress
to v2 while capping it at 15. The repeatable `Another 15 Wolves` offer is added
after `Break the Slime Surge` and accepts 15 Ironpine wolves or 15 Alpha Wolves;
its 400 XP reward is provisional. Cornberg Brewer now offers prototype minor
health/mana potions at 12/15 gold, and a new potion can be consumed from the
inventory for +30 health or +25 mana. The Big Slime authored offsets currently
resolve to 4x minimum / 5x maximum raw attack damage (16–20 from a 4 base).
CodexPanel now has a book-like paper/cover treatment.

The follow-up combat/input pass keeps auto-retaliation idle-only while the player is travelling or has queued orders, displays attack ranges as whole numbers, halves Magically Touched's natural mana regeneration (5 to 2.5 base and 0.15 to 0.075 per INT/SPI), supports repeat attack-move targeting after Shift+X while Shift remains held, and applies primary-attribute scaling at 99% on the minimum end versus 100% on the maximum end. The repeat targeting flag is cleared whenever targeting is cancelled. These changes are static-reviewed only; the blocked Editor prevented compile and gameplay validation.

Editor regression source was extended in `PlaytestFixesValidation.cs`, but it
has NOT run. The connected Unity CLI/Editor is currently unavailable from this
session; no successful compile, focused Play Mode run, architecture check,
visual inspection, or fresh Windows build has been obtained. Treat every
runtime/UI/quest change in this section as unverified until those checks are
run on Unity 6000.6.3f1. No commit, push, or merge was made. `main` was not
changed. Preserve the untracked `Assets/Settings/` and Package Manager settings
changes. Next step: restore the connected licensed Unity Editor/CLI and run the
project compile/import followed by the focused playtest-fixes harness, then
review screenshots and fix any actual errors before producing a new Windows
Development build.

- User explicitly requested all downloaded assets' listed website licenses. Completed all60 files (59ZIP + WarriorGirl standaloneGLB), covering59 distinct model pages. Removed provisional license-unknown language from current catalog/registry and separated page licenses from provenance/technical concerns.
- Stated license totals by FILE: 55 CC BY4.0, 2 FreeStandard (DuatGolem and MedievalCivilian3), BlackDragon1 CC BY-NC4.0 (original3DHaupt page), WarriorGirl ZIP+GLB2 CC BY-NC-SA4.0 (NORBERTO-3D, one model/two formats). Every row now links exact source, creator, versioned terms and matching evidence.
- Generic Knight, both MedievalHouses and MedievalKnightModel resolved by deeper search/geometry. All GLB embedded asset metadata inspected read-only, including the large WarriorGirl via mmap. Exact embedded source/license for WarriorGirl, shared Alembic texture set verified. MedievalGuard Taylor source matched all29 texture-name stems; RAR decode remains unrelated technical limitation. SkeletonAnimated matched One_Boy by1118vertices/title/oneanimation; different triangle counts documented. Tyranno selected the downloadable T reey page matching11animations; upstream re-upload notes retained separately.
- Primary docs: docs/assets/MODEL_PAGE_LICENSES_2026_10_17.md; docs/ASSET_CATALOG.md and ASSET_CREDITS.md now full current tables. Full official API evidence, embedded metadata, hash-bound verified source map and all60-row CSV checked in. Earlier dated technical receipts remain historical; current audit supersedes preliminary license lookups.
- Repro: inspect_embedded_model_sources.py --inventory ... --output ... ; audit_sketchfab_licenses.py --inventory docs/assets/downloaded-assets-2026-10-17.json --source-map docs/assets/verified-model-source-map-2026-10-17.json --output Library/LicenseRecheck.json. Optional --only filenames. Source-map hash must match inventory. Source URLs/API licenses verified; ownership/concept/game-IP concerns remain distinct rather than replacing known license labels with unknown.
- Verified60/60 table/CSV coverage,59 sourcepages,0 unresolved license lookups; scripts compile and five formerly ambiguous source mappings rechecked. Raw downloads untouched; no game/Unity changes or new runtime credits entries; no engine build needed.
- Continue integration/world1-foundation; main unchanged. Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings*.

## 2026-10-17 - Downloaded asset catalog refreshed (17 new ZIPs)

- Reviewed T:\TEMP\ART_ASSETS against the Oct15 inventory. Now 59 ZIPs plus warrior_girl.glb (60 files). All 43 existing hashes unchanged; 17 new outer ZIPs passed full member CRC checks. Original downloads untouched; no Unity/gameplay changes.
- Added every new archive to docs/ASSET_CATALOG.md and ASSET_CREDITS.md; new full inventory/CSV, source metadata and per-asset review in docs/assets/*2026-10-17*. Nine FBXs have skins + animation curves, one skinned FBX has empty animation stack, four static ~1.9-2M triangle FBXs, one nested OBJ character, one compressed Blender scene, one oversized RAR FBX.
- Best technical leads: DJMaesen Treeman (8616tris/2materials) and Lizardman (6582/1), both matched CC BY4 listings with rigs/curves. Tyranno (7366tris/11 stacks) has multiple identical uploads/upstream rroobboo credit, so source hold. Skeleton Animated (2094tris) exact source unresolved. Generic Medieval Civilian3 (21410tris/7materials) FreeStandard/Fuse/Mixamo source-term review. MushroomWarrior (14135tris/4materials) concept artist Matthew McAngus review.
- CatWitch has Waspsalad/Minecraft concept provenance; both HIT2 downloads identify third-party game content; holds recorded. Blade1 is Ch_Chosun_A OBJ character, not a confirmed sword. Ouroboros has compressed Blender header303; Blender unavailable, no rig/scene claims. Hundra RAR FBX exceeds768MiB cap. Four Pigcraft undead/fungal models unrigged/high-poly references.
- Twelve unique filename-context + geometry source candidates (11 CC BY4, 1 FreeStandard), not ownership clearance; duplicate/source-only leads explicitly distinguished. No new bundled license/readme identified. game-dev unavailable; no canonical admission receipt or runtime Credits additions claimed.
- Added repeatable public Sketchfab metadata research and OBJ/PNG/header/CRC supplement tools; added 30s watchdog around RAR decoder reads. Scripts compile; catalog/registry/CSV cover all17, fullCSV60, source/review counts verified. No new runtime build required for docs/inventory tooling.
- Continue integration/world1-foundation; main unchanged. Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings*. Current healer and Priest draft remain as prior handover; Priest not implemented.

## 2026-10-17 - Trimery healer placeholder and Priest design draft

- User played/liked the current build, reported no healer in Trimery ("timerly"), requested a healer blob and then designing Tier II Priest with an advancement quest from this NPC.
- Implemented free living-player HP treatment only: ivory/blue primitive blob, staff, gold emblem, idle breathing at x244/z190; separate road approach x246/z190. InteractionPrompt + existing registry/right-click/nearest-key path. No passive aura, resurrection, Mana/Grace refill, charge, or quest/class unlock. Health.Heal preserves shields/status and received-healing modifiers. Faction0/living/range/LOS gates; layer10 selection, not nav obstacle9.
- TrimeryHealerAuthoring InstallInto is included in the fresh village installer; separate .install changes only its own named root/materials. Scene saved with Editor APIs, no YAML edits. Semantic comparison: all 28,720 old blocks retained, 42 new blocks, one old parent child-list change.
- Live isolated test passed 48 checks: five real walking checkpoints, live frames, click/nearest treatment, full-HP feedback behavior, shield preservation, faction/disabled/range/dead rejection and no resurrection. Main-camera capture inspected. New seven-scene Windows Development build succeeded 0 errors/0 warnings; isolated 15s batch/nographics startup passed (not a graphical player playthrough).
- Priest is DESIGN ONLY. Do not tell user the class or quest is implemented. Confirmed earlier: level30 Magically Touched -> TierII Priest, staff, Mana every active cast even empowered, separate Grace boosts healing AND damage, solo-capable reactive healer. Latest preferred core: Holy Light / Targeted Heal / Heal Nova / Dash. Earlier Aegis kit is not the latest preference. Preserve flexible arbitrary skill architecture.
- Proposed (unapproved): deliberate Grace empowerment; Hands of Mercy trial: borrowed temporary healing action, three distinct wounded lumberworkers, protect recovering worker in short solo encounter, return/explicit turn-in -> Priest1. No Grace cap/generation/spending/decay or spell numbers locked. Discuss these before full class/quest implementation; no invented resurrection implementation.
- Read docs/TRIMERY_HEALER_2026_10_17.md and PRIEST_TIER2_PROPOSAL_2026_10_17.md. Evidence docs/validation/TrimeryHealer-*, screenshot docs/screenshots/Trimery-Healer.png. Reproduce dicefree.trimery.healer.install/test/status.
- Continue integration/world1-foundation; main unchanged. Preserve/exclude unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings*. No new third-party art/licenses. Build path Builds/World1Foundation/DiceFree.exe (complete folder).

## 2026-10-16 - Briarback region, textured trees and day/night

- Completed Axel's selected northern quilboar gameplay loop and his follow-up tree/day-night request. Continue integration/world1-foundation; main unchanged. Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings*.
- Provisional names/balance: Briarback north of Trimery, Scout Runa, The Northern Trail, Harrowtusk, Bristlehide Mantle. Four three-enemy camps (Raider/Spear Thrower/Totemcaller levels21/23/25/27), level30 elite, original skinned/animated creature. Quest watchpost -> 8 quilboars -> elite -> scout, reward150gold/1800XP/bound Back mantle +3PDEF/+2MDEF/+3VIT for every class. No Big City or Roadwarden encounter/repair added; WEST road stays blocked.
- Extended terrain/navigation/camera/fog bounds (-85,-70)..(353,406). Moved obsolete northern invisible nav wall to z400. Dungeon local visibility retains overworld exploration; actual return restores expanded bounds. Scout has a separate roadside conversation approach. Inactive development player's catalogs now receive the item/quest.
- 857 optional textured oak replacements from existing licensed Stylized Forest Kit Samples (four palettes/three LODs/URP alpha-cutout normal/AO). Original tree/collision fallback retained; conifers preserved. Sources/generated licensed prefabs ignored. SourceArt/World/Environment has source/byte receipt; game-dev unavailable, no canonical admission claim. Credit entry added, now 41.
- WorldDayNightCycle in Cornberg and both cube test maps: 48-minute session day, 10:00 start, sun/moon single directional-shadow orbit, sky/ambient/fog blending, fixed dungeon lighting with outdoor phase restored. Session clock is not persisted or network-authoritative. World camera/light exclude portrait layer31, fixing studio-light leakage. Main shadow distance80m.
- Final visual pass found the OLD giant future-city backdrop occupying the new elite camp. Moved its non-colliding trunk/bough/crown from z330 to z560; distant terrain dressing unchanged. Mantle dimensions were normal; added per-class world-size assertions rather than changing gear scale.
- Verified: region 164 checks/30 actual checkpoints/607.8m, level-independent pursuit and real animated feet, elite dodge/impact160raw/interrupt, isolated credit/reward/save/equipped menu; environment 4308 including GPU sun/moon shadow displacement; five-class gear 297; Mage 127 including real dungeon fog/lighting return; cube edge 95; ten static regressions plus region topology. Scripted kill fixtures are not a full manual combat playthrough. Noon/night/camp/creature renders inspected.
- Final seven-scene Windows Development build succeeded with 0 errors/0 warnings. Builds/World1Foundation/DiceFree.exe (complete folder). Isolated 15s batch/nographics startup passed; no graphical player playthrough claimed.
- See docs/BRIARBACK_REGION_2026_10_16.md, docs/WORLD_ENVIRONMENT_2026_10_16.md, docs/validation/Briarback-* and Environment-*, docs/screenshots equivalents. Reproduce dicefree.briarback.author/validate/test/status/resume and dicefree.environment.install/test/status.
- Next: Axel's feel/difficulty/art playtest, then two-computer multiplayer validation. Existing production networking limitations remain; original Novice lab was not extended.

## 2026-10-16 - Tier II Mage, puzzle advancement and elemental kit

- Level-30 Magically Touched Novice accepts A Mind for Magic from Cornberg's brewer / retired mage, solves the actual Slime Dungeon five-ring tree puzzle and returns for atomic advancement to level-1 Mage. Parent retained; earlier puzzle clears, boss kills, wrong manifestation and duplicate turn-ins cannot bypass it.
- Six active spells: Ember Bolt, Frost Snare, Arc Lightning, Quicksand, Half-Life, Math Barrier. Two separate passives: unique-element Weaving/resistance reduction and Mana Attunement. INT scaling, wand basic attack, flexible eight-entry provider, rank/point gates, no ultimate. Save schema remains 6.
- Weaving repeats refresh the shared 8s delay without duplicate elements; ticks/jumps/targets never add casts. +5% damage/stack and other Mage cooldowns -0.2s per cast. Decay 1 oldest element/s, aura 8m/strongest-copy overlap, Mana passive +30/+0.5 regen per rank are provisional starting balance.
- Half-Life reduces both defenses by 5 x rank. Self-only Math Barrier lasts 2 x rank seconds, redirects (25+5 x rank)% of post-shield damage at (2-0.1 x rank) Mana/damage. Full/partial/empty Mana, healing and shield priority tested.
- Both player scenes, five-class catalog, dev-room selector, saves, all eleven slots and equipped Tier II menu previews wired. Original pointed hat/split coat/tapered cape/wand/orb placeholder over the existing licensed humanoid; approved commercial LittleSimple pack is NOT installed. Existing Humanoid animations reused. Some icons/effects remain prototype.
- Fixed resource restore to include passive modifiers and to occur after equipped attribute bonuses. Full Mana survives class switching with passive+gear capacity. Fixed portrait initialization so Mage defaults hide behind equipped items, including two-hand suppression of both wand and orb.
- Verified: Mage 123, five-class gear/skills 277, Greatblade combat 75, swordswoman quest 35, equipped menu/loot 65; dev/menu/World2 round trip and fourteen static/data validators passed. Final Mage/architecture/managed references rechecked after portrait fix. Actual default/HUD/equipped portrait captures inspected.
- Seven-scene Windows Development build succeeded, 0 errors/0 warnings, 354,689,133 bytes. Builds/World1Foundation/DiceFree.exe; use complete folder. Isolated 15s batch/nographics startup smoke passed without error matches; not a full graphical playthrough.
- Read docs/classes/MAGE.md and docs/MAGE_TIER2_2026_10_16.md. Evidence: docs/validation/Mage-*.json and docs/screenshots/Mage-*.png. Reproduce dicefree.mage.test/status/resume and .validate; .author then dicefree.menu-wolf.author to rebuild authored data/previews.
- Production Mage multiplayer is not implemented; existing isolated Novice/basic-attack network lab unchanged. Continue integration/world1-foundation; main unchanged. Preserve unrelated PackageManagerSettings.asset and Assets/Settings*.

- Preserved and merged parallel design-only commits through 7f556b6. Aligned Quicksand to existing canonical Nature, Frost Snare to brief freeze using shared stun resistance/immunity, and Weaving resistance reduction to a true six-school nearby aura (-1 percentage point/rank). Added range/overlap/disabled-source and freeze-expiry/immunity coverage. Historical interview notes retained with implementation cross-reference.

## 2026-10-15 - Tiered equipped menu, Credits and Alpha Wolf greatsword

- StartMenu now has tier tabs derived from the advancement graph, saved manifestation cards with Echo/title/level and read-only full-body 2D portraits showing each form's own equipment. Empty tiers and future/unavailable saves are handled. New/rename/refresh/dev-room/multiplayer entry points remain.
- Visual-only preview prefabs retain portable fallback class art; licensed visual replacement components still resolve optional local resources. No CombatActor/Health/AI/persistence/input in preview roots; one light, deferred one-shot rendering, full RT/root cleanup and safe invalid-preview fallback. Menu browsing leaves save bytes untouched.
- Reuses InputRuntimeHost's OptionsPanel; no duplicate persistent Options service. Added searchable Credits tab in menu and gameplay with 40 entries locally (Unity + 39 used packs), source/license links and verbatim Unity Companion shader notice. SourceArt/Credits/credit_manifest.json is editable; resource probes omit absent optional packs. docs/ASSET_CREDITS.md separates runtime usage from staged candidate holds; no new third-party art import.
- Alpha Wolf has 10% per-life FixedWorldDrop for item.alpha-wolf.greatsword (Wolf-Fang Greatsword), MainHand/handsRequired=2, +10 basic attack damage and +5 STR, all classes. First-party plain steel/leather/fang/wolf-pommel model and icon. Offhand returns to bag, cannot re-equip until main hand changes; conflicting saved slots normalize without losing owned items. Existing plain model-test offhand works with one-hand gear.
- Both Cornberg and DevelopmentRoom item catalogs contain the new definition; reacquire item assets after scene opens during authoring. Repeat wildlife authoring preserves the Alpha reward; updated obsolete no-wildlife-drop assertion. Save schema remains 6.
- Verified: focused suite 55; gear/flexible skills 232; Greatblade combat 75; menu/dev-room/World2 round trip passed; 12 static/data commands passed. Inspected Tier I, Greatblade and Credits Game-view captures. Temporary synthetic profile only; future-class tab in screenshots is a deliberate fixture.
- Seven-scene Windows Development build succeeded, 0 errors/0 warnings, 354,090,364 bytes. Builds/World1Foundation/DiceFree.exe; complete folder needed. Isolated Windows startup smoke remained running 15 seconds with no error matches, batch/nographics only.
- Read docs/MENU_CREDITS_WOLF_2026_10_15.md; reports docs/validation/Menu-*, captures docs/screenshots/Menu*. Reproduce dicefree.menu-wolf.test/status/resume (visual pauses) or .verify (same checks without pauses), .author and dicefree.credits.author.
- Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings*. Continue integration/world1-foundation; no main merge.

## 2026-10-15 - Downloaded 3D catalog refreshed

- Brought forward the original ASSET_CATALOG.md and ASSET_CREDITS.md from poc/cornberg 608e20a into integration/world1-foundation, then extended them for 23 new ZIPs in T:\TEMP\ART_ASSETS. Total: 42 ZIPs plus completed warrior_girl.glb. Originals remain unchanged; no DiceFree Unity import or model replacement.
- New batch: 13 GLB sources (two skinned/animated), nine FBX sources (unskinned, no animation curves), one RAR-wrapped Medieval Guard. Windows tar lists the RAR but cannot decode its FBX filters; recorded as unverified, not a successful rig inspection.
- Recorded hashes, sizes, member paths/CRCs and packaged geometry/rig counts in docs/assets/downloaded-assets-2026-10-15.json, flat CSV, and repeatable read-only tools/assets/inspect_downloaded_assets.py. Completed 1 GB Warrior Girl GLB has valid header but its 201 MiB JSON exceeds the 64 MiB bounded parser limit; no full accessor claim.
- Official public Sketchfab API title/context plus triangle matches found 18 source candidates: 17 list CC BY 4.0; Duat Golem lists Free Standard. Five generic sources unresolved. Preserve rights holds: Duat is third-party-series content, and Mountain Dragon has a possible PROTOFACTOR upstream match (same filename/triangle count) despite Alexey's CC BY listing. This is a provenance lead, not an infringement finding.
- Practical new technical shortlist: 10k-triangle Medieval Man; ~19k owl/lion/monster-dragon; 44k spear/shield knight. All are unrigged. Two distinct house downloads: 2,148 vs 23,700 tris, both source-unresolved. Well and several plague/abyss characters ~2m triangles, appearance/retopology references.
- New entries and all 19 old registry rows retained; 43 files accounted for in Markdown/CSV, 23 new credit rows, source records included. No Unity build required for docs/offline inventory. Local game-dev CLI unavailable; no canonical package/admission/lock receipt claim.
- Existing unrelated PackageManagerSettings.asset and Assets/Settings* left untouched. Continue integration/world1-foundation; main unchanged.
## 2026-10-15 - Walking animations, edge pads and enemy-pass Spin Dash

- Shared Novice FBX Idle/Locomotion now loops explicitly for Novice/Physically Blessed/Magically Touched. Authoring preserves it. Shared driver resolves actual class state names (including Greatblade), recovers stale walk states and scales only locomotion cadence. Greatblade portrait Idle now uses a valid state.
- Four existing Cornberg wanderers (two farmers, villager, herbalist) now use derived first-party rigged versions of their original appearances. Actual-displacement animation follows their existing navigation; gameplay roots/colliders/identity/interactions stay intact. Static NPCs retain existing visuals. Original source assets unchanged. Reproducible Blender/Unity authoring and verified export hashes recorded under SourceArt/World/WalkingVillagers.
- Seam activation now begins at 20.5m instead of 22.2m, so reaching the 21m pad crosses. Arrival stays disarmed until walking inward below 19.5m, avoiding bounce. Both directions verified.
- Spin Dash uses a named enemy-only passage lease honored by direct-movement physics and symmetric actor separation, temporarily disables native unit avoidance, and restores only its own state. Enemies pass; walls/objects/friendlies/NavMesh boundaries still block. Exact hit counts and interrupt/other-owner cleanup verified. Tooltip updated.
- Focused suite passed 76 runtime assertions, including all four class poses, four actual NPC walkers, two-way automatic crossing and dash cases. Cube 95, Greatblade 75, development/menu round trip, ten static/data checks, wildlife and village skinned-pose checks passed. NPC Unity pose image inspected.
- Rebuilt seven-scene Windows Development player: 0 errors, 0 warnings, 353,319,456 bytes. Builds/World1Foundation/DiceFree.exe (use full folder).
- Rebuilt standalone multiplayer lab passed 22/22 loopback checks. Harness now walks inward to rearm and relies on walking-only crossings; coordinate waits accept observed authoritative travel to avoid missing the auto-trigger frame. All previous roster/combat/reconnect checks retained. No laptop/LAN or production class/skill replication claim.
- Read docs/WALKING_TRAVERSAL_FIXES_2026_10_15.md; results under docs/validation/Walking-* and docs/screenshots/WalkingVillagers-Poses.png. Reproduce with dicefree.traversal-fixes.test/status, dicefree.villager-walk.validate, and tools/validation/Invoke-NetworkWorldLab.ps1.
- Preserve pre-existing ProjectSettings/PackageManagerSettings.asset and Assets/Settings*. Continue integration/world1-foundation; no main merge.
## 2026-10-15 â€” local multiplayer world lab

- Added Main Menu -> Multiplayer development test. Direct/LAN host/join through NGO 2.13.3 and Editor-resolved built-in Unity Transport 6.6.0.
- Host-authoritative intent validation, existing TraversalMotor/BasicAttack/Health simulation, independent per-participant two-face quarter-turns without unloading occupied host maps, late join, four reserved slots, 30-second vulnerable reconnect grace and expiry cleanup.
- Every lab launch uses a fresh temporary development save override; normal Echo/class saves are not imported. Menu cleanup explicitly verified no remaining override/session/maps.
- 21/21 focused checks passed in separate Windows processes, including simultaneous combat on both faces, four players, fifth-player rejection, reconnect preserving actor/missing HP, and expiry/fresh rejoin. All five final process logs clean of runtime/navigation/fog errors.
- Original cube regression 95 passed; Greatblade combat 75 passed; original dev round-trip passed; ten static/data checks passed; World/Codex cleanup/dungeon-lease regression passed.
- Graphical Editor host and separate graphical Windows guest connected; telemetry and host screenshot verify split-face residency. Guest window itself has not had a manual visual walkthrough.
- Both test maps currently remain loaded on every peer. Guest replicas use primitive shapes; only basic attacks, isolated Novices (500 test HP) and sentinels (2000 HP). This is not multiplayer support for Cornberg, skills/gear/quests/loot/progression, production streaming or host migration.
- Windows build remains Builds/World1Foundation/DiceFree.exe; use the entire folder on the laptop. Host on desktop, join its LAN IPv4 and matching port (7777). No LAN/firewall/router changes made.
- Read docs/MULTIPLAYER_WORLD_LAB.md and docs/MULTIPLAYER_WORLD_LAB_VERIFICATION.md. Evidence and screenshots are committed under docs/validation and docs/screenshots. Reproduce through tools/validation/Invoke-NetworkWorldLab.ps1.
- Serialized only the six existing test-target NavMeshAgents disabled to avoid native activation before map NavMesh registration; TraversalMotor.Start enables them after registration. Headless fog bypasses rendering-only initialization.
- Preserve pre-existing ProjectSettings/PackageManagerSettings.asset and untracked Assets/Settings*. No main merge.

## 2026-10-14 - Visible cube faces and quarter-turn test crossing

- Development Room and World2TransitionTest are now adjacent 48m square faces with north/south seam gates, one/two golden pips, rebaked navigation and renderer-only face preview prefabs. Approach the blue face's north seam to see the purple face; crossing rotates the world 90 degrees beneath a presentation-only Echo.
- CubeEdgeMap uses reciprocal WorldEdgeLink/CubeRotate spawn metadata. CubeEdgePresentation rotates visual surrogates while real physics/NavMeshes remain flat. Same player/camera/HUD/state move between additive maps; old maps unload. 2.2s turn plus short preparation/settling, no global pause or camera barrel roll.
- Named actor action/motion blocks, temporary loadout lock and input/behaviour snapshots gate commands. Other owners' locks and previously disabled inputs survive. Invalid destination metadata restores presentation/controls without committing travel. Arrival stays inside the seam and requires moving inward before rearming automatic crossing.
- Focused isolated suite passed 95 assertions: camera-visible neighbouring geometry, no preview actors/colliders/NavMesh, exact midpoint/animated +/-90 turns, same-player round trip, actual seam walking, failure recovery, classes/levels/items/ranks/gold/HP/Mana/cooldown continuity and normal-save sentinel protection.
- Transport-only fixture disables ordinary HP/Mana regeneration for exact conservation. Existing dev-room/menu regression now checks enabled natural regeneration against elapsed simulation time rather than old instantaneous-trip percentage limits; it passed. Greatblade combat passed 75 and all ten data/architecture/progression regressions passed.
- Approach/midpoint/arrival actual Game-view screenshots inspected and archived under docs/screenshots/CubeEdge-*. See docs/CUBE_EDGE_PROTOTYPE.md and docs/validation/CubeEdge*.
- Production Cornberg routes, six-face topology and gameplay art are unchanged. This is a linked two-face visual/loading prototype, not full six-face implementation, seamless cube physics, multiplayer traversal or final cinematic polish.
- Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings. Continue integration/world1-foundation; no main merge.

## 2026-10-14 - Smaller cloth Magically Touched model integrated

- User-approved Polytope free modular male cloth00 outfit, head01, hair20, short 01_A cape; indigo/violet cloth and modest Adventure Forge Fantasy Staff Lite. New MagicallyTouchedVisual loads optional ignored MagicalLicensed/MagicalHero; original form remains source-absence fallback.
- Live idle geometry: Magical height 1.647m vs Physical 1.843m (~11% shorter), shoulder span .295m vs .405m (~27% narrower). Visual proportions only; gameplay colliders/reach/stats remain unchanged.
- URP/Lit derived materials, valid Humanoid avatar, existing idle/walk/attack controller, no root motion/colliders. Existing gear sockets and bronze dagger follow the new rig. Mainhand/back gear hides staff/cape; unequip restores them.
- PlainGearAttachment moved into its own Unity script file for authored-prefab serialization. Old secondary-class layout worked for runtime plain gear but lost the saved staff component. New focused test verifies no missing scripts and portrait fit; all-class suite checks the shared helper.
- Model test passed 78 assertions, existing Magical five-skill/Mana/save/menu-reload suite passed, full gear/flexible-skill suite passed 232, ten static validators passed. Clear actual Game view and same-scale Editor comparison preview inspected. No full human playthrough or bespoke staff-attack animation claim.
- Windows Development build passed with seven scenes, 0 errors/0 warnings, 348,117,424 reported bytes. See docs/MAGICAL_CLOTH_MODEL_IMPLEMENTATION.md, docs/screenshots/MagicalModel-* and docs/validation/MagicalModel*.
- Archive/member hash rosters, licence/provenance and copied-byte verification recorded in docs/assets/Magical-source-receipt-2026-10-14.json. Source packs and derived resources stay ignored; local game-dev CLI unavailable, so no Game Development Studio lock receipt claim.
- Cube design remains six face maps, adjacent face visible at an edge, smooth quarter-turn hiding loading. Existing World 2 state-preserving transition test is not yet the visual cube-edge flip prototype.
- Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings; continue integration/world1-foundation, no main merge.


## 2026-10-14 - Plain all-slot gear and flexible class abilities

- Cornberg merchant sells eleven plain level-1, common, all-class fitting items at 100 gold each; no normal player grants. Dev-room item list includes them. Tiny per-slot bonuses, original primitive bone-follow visuals, portrait fitting and scrollable buy/sell lists.
- Greatblade keeps normal offhand restrictions; only explicitly flagged modelTestingItem fitting gear can occupy its offhand. Plain sword suppresses baseline Greatblade weapon while equipped.
- IClassAbilityProvider and built-in adapters replace fixed four-active/one-passive bar dispatch. Authored order, any kit size, eight regular skills per page, optional pinned ultimate, varying positive Physical/Magical rank caps. Keys 1â€“8/page and T/ultimate; old 66 input identity/binding records preserved. Existing kits, combat targeting/queueing and save schema unchanged.
- Combined isolated suite passed 232 checks, Greatblade combat 75, Physical model 47, Physical/Magical five-skill/Mana/menu-reload suites passed, ten static validators passed, dev-room/World 2 round trip passed. Saved Cornberg and DevelopmentRoom catalogues contain all eleven items after reopen. Clear dev-room full-set and merchant screenshots inspected.
- Seven-scene Windows Development build succeeded, 0 errors/0 warnings, 344,700,296 bytes. See docs/PLAIN_GEAR_AND_FLEXIBLE_SKILLS.md and docs/validation/*PlainGear*.
- Magical recommendation is candidate-only: owned Polytope free modular human cloth00, blue/violet, short cape, modest Fantasy Staff Lite. Actual cache inspected; no Unity avatar/material/retarget validation or replacement performed. UMA robes require more integration. See docs/MAGICAL_MODEL_REVIEW_2026-10-14.md.
- Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings. Licensed sources/derived resources remain ignored. Continue integration/world1-foundation; main untouched.
## 2026-10-14 — Physically Blessed starter-armour model integrated

- Approved Blink FREE Modular Character Fantasy RPG Human Male now replaces the visible Physical model in Cornberg and Development Room. Helmet hidden; leather/padded chest, trousers, boots, bracers and modest shoulders. Existing class data and normal-boss advancement quest are preserved.
- Reproducible authoring: dicefree.physical.model.author. Source prefab and URP-derived PhysicalLicensed resource stay local and ignored; portable original presentation remains usable when the resource is absent. Do not regenerate through the older PhysicalBlessedAuthoring.Prepare, which would overwrite the wider four-class composition.
- PhysicalBlessedVisual loads once on activation, transfers existing item sockets to the new Humanoid bones and adds the existing bronze dagger's Physical visual. Animator discovery skips the disabled fallback. HUD clones reuse prepared art and frame actual baked visible geometry, including equipment and character rotation.
- Final model test passed 47 assertions, including unavailable-source fallback, idle/walking/attack, dagger/shield/tophat/gloves, new-rig attachments, class switching and portrait clipping. Full Physical five-skill/Mana/menu-reload regression passed. Greatblade quest passed 35; Greatblade combat passed 75; ten static/regression validators passed.
- Quest test now explicitly selects the active Slime gameplay portal; FindAny sometimes selected the inactive decorative exterior portal. This changes the fixture, not portal gameplay.
- Actual Game view and temporary equipped close-up inspected. Windows seven-scene Development build succeeded with 0 errors/0 warnings (344,658,024 reported bytes); non-graphical startup remained responsive with StartMenu and advancing frames beyond 23 seconds. See docs/PHYSICAL_STARTER_ARMOUR_IMPLEMENTATION.md, docs/validation/PhysicalModel*-2026-10-14.json and docs/screenshots/PhysicalModel-Equipped.png.
- Shared adaptive basic-attack animation remains; bespoke weapon-specific clips and final transmog fitting are future work. No full human playthrough claim.
- Preserve unrelated ProjectSettings/PackageManagerSettings.asset and Assets/Settings. Continue integration/world1-foundation; main remains untouched.

## 2026-10-14 — Greatblade normal Slime Boss advancement quest

- Supersedes the earlier direct advancement offer. Cornberg's retired swordswoman offers The Weight of a Greatblade only to Physical level 30+. Explicit acceptance, normal Slime Boss defeat, then return to her advances to level-1 Greatblade.
- Earlier kills/miniboss/Regent/outside-dungeon boss actors do not count. Quest uses dungeonBossOnly and the participating player's scoped DungeonBossKilledEvent stream; no duplicate XP/drop reporter. Leaving the run unbinds the quest listener.
- Completion and fork are one validated durable profile commit; parent level/gear/progression and completed quest are preserved. Existing Greatblade manifestations remain available. No save-schema bump.
- Development Room retains an explicit DevelopmentSession-only class selection bypass; normal advancement is quest-gated.
- Final isolated live quest test passed 35 assertions, including an actual dungeon normal-boss death and on-disk parent/child verification. Revised Greatblade combat suite passed 75 assertions; all ten regression validators passed.
- Physical model review is candidate-only. Recommend installed Blink FREE Modular Character Fantasy RPG Human Male, starter leather/padded armour, visible face and modest weapon. Valid Humanoid avatar, 21 skinned meshes; URP preview captured. Source animations already installed, but live integration/retargeting not yet validated. See docs/PHYSICAL_MODEL_CANDIDATES_2026-10-14.md.
- Preserve unrelated PackageManagerSettings, Assets/Settings and local licensed sources. Continue integration/world1-foundation; no main merge.

## 2026-10-14 — Greatblade is now playable

- Branch integration/world1-foundation. See docs/GREATBLADE_PLAYABLE_IMPLEMENTATION.md for mechanics, provisional tuning, learning economy and evidence.
- Greatblade is Tier II from Physical level 30, starts level 1; retired Cornberg swordswoman offers explicit advancement. Dev room can select it through the graph. Parent snapshots stay intact.
- Six skills: Wide Arc and ground-target collision-safe Spin Dash; Overhead Breaker L6 spending all Momentum; Battle Cry L18; evolving Greatsword Mastery and separate Heavy Rhythm. Four free starting ranks, subsequent gated ranks cost points from levels after 1. Six HUD slots, distinct icons, two visible passives; slot9 stays Tier IV ultimate-only.
- Momentum 0–100, builds on actual hits, no threshold buffs, delayed out-of-combat decay, resets on load/death. Mastery rank4 uses provisional 5%-HP / 5-second shield; shield versus heal still not permanently decided.
- Offhand blocked/unequipped for Greatblade; parent gear unchanged. Shared existing model/animations; generic Novice relaxed arm posing skipped for Greatblade.
- Final focused live test passed 75 assertions; actual Game view inspected with six icons/passives and Momentum. Ten data/architecture/migration/presentation validators passed. Existing isolated dev-room / World2 round-trip test passed. Windows seven-scene Development build passed, 0 errors/0 warnings, 317,919,332 reported bytes.
- No schema bump (existing v6); no licensed source/derived art committed; preserve unrelated PackageManagerSettings and untracked Assets/Settings. Main remains untouched.
- Next useful work: human combat feel/balance pass, mentor narrative quest, then another Tier II kit. No production balance or multiplayer claim.

## 2026-10-14 — October gameplay repairs verified in Unity

- Workspace `T:\TEMP\DiceFree-Options`, branch `integration/world1-foundation`. Read [OCT14_GAMEPLAY_FIX_VERIFICATION.md](OCT14_GAMEPLAY_FIX_VERIFICATION.md) for complete test evidence, build output, and limitations.
- Final focused isolated Play Mode: **PASSED**, including 9/9 Trimery walking points (banker approach), 14 URP material conversions, Trimery revive stone activation/death return, level-24 Novice vs both dungeon bosses pursuing and landing melee, and normal boss Slam raw damage 180 (10x).
- Root cause fixed: scaled boss NavMesh agent radius mismatch; restored large-boss chase. Intentional teleports now rebase `ActorFootprintSeparation` so Slime Slam cannot create a false movement-separation impulse.
- Additional Trimery/Slime/Ironpine/architecture/managed-reference/Greatblade/quadratic-XP validators passed. Windows x64 **7-scene Development build succeeded, 0 errors / 0 warnings**, at `Builds/World1Foundation/DiceFree.exe` (reported 317,880,024 bytes). 15-second standalone batchmode/nographics engine-start smoke stayed alive.
- Remaining: manual graphical Game view/Windows-player QA (visual pink-material absence, attack readability, long player traversal); runtime test results do **not** prove all such visual/gameplay qualities. Preserve local untracked third-party source art and generated `Assets/Settings` files; no merge to `main` is authorized.

## 2026-10-13 — Greatblade visual repair completed

- Branch remains `feature/visual-overhaul-vis00`; no merge to main. Greatblade combat/class advancement is still prototype work. See `docs/GREATBLADE_PROTOTYPE_IMPLEMENTATION.md` for the current visual setup, evidence and next slice.
- Restored missing original A03 texture inputs from the local licensed cache. Derived textured body uses one body LOD; source Longsword hidden. Vendor FBX, source clips and materials remain intact.
- Rebuilt retargeting from a temporary T-pose: 51 mapped human bones, all 30 finger joints. New ignored Avatar is `GreatbladeFemaleTposeAvatar.asset`; do not overwrite the old Avatar held open by a Node process.
- Added raised two-hand ready/carry clips, closed-finger swing/spin derivatives, measured sword handle anchors and `GreatbladeTwoHandGrip` LateUpdate arm solving. Sword is 2.75 units long while grip girth retains source scale. Both class/mentor appearances share the same resource.
- Final Unity compile succeeded. The prototype validator passed 80 poses across all four motions; the live runtime adapter test passed 3,730 frames with both actors, all four states, palm errors below 0.003 mm and no actor-root movement. Live ready/swing captures were inspected; immediate preview-scene skin renders can lag behind manual pose updates.
- Evidence/backups/source receipt: `Library/GreatbladeVisualRepair20261013/`. Final Unity image: `Greatblade-Repaired-2026-10-13.png`. Temporary scripts are outside Assets.
- Unity is open with the local Package Manager service via `-upmIpcPath Upm-Diagnostic`. Default UPM auto-spawning failed earlier; that workaround depends on the service remaining available. No security settings were changed.
- Preserve unrelated material/settings edits and player profiles. Never stage licensed source/derived art. Visual authoring preserves the open gameplay scene; runtime tests stopped and the prior Cornberg scene was restored.
- Next: playable Wide Arc plus Heavy Rhythm/Momentum feedback, then collision-safe Spin Dash and Momentum-consuming Overhead Breaker. No threshold Momentum buffs; rank-4 Mastery shield vs heal remains open.

# DiceFree Agent Handover

## Issue #48 Options / persistent key rebinding - 2026-10-05 (review-ready)

Working branch: `feature/48-options-keybinds`, based on the reviewed Farmer's Pants validation head `6fd6beaada0767d1e5c37a1a7c365c5e6ee84368`. Issue #48 remains open. **No merge is authorized.**

The Cornberg prototype now uses a shared authored Unity Input System asset at `Assets/_DiceFree/Resources/DiceFreeControls.inputactions` instead of independent hard-coded action definitions in traversal/combat/camera/inventory code. A new dependency-light `DiceFree.Input.Runtime` assembly owns `InputBindings` and `BindingSettingsStore`. UI/Application/Editor may depend on Input; gameplay/foundation do not gain a UI dependency.

`InputRuntimeHost` composes one persistent runtime binding service + `OptionsPanel`. F10 or the visible HUD button opens Options; Escape closes it only while open. Gameplay and Camera maps are disabled under the modal while UI stays active. Existing gameplay Escape defaults are preserved while the menu is closed. Active traversal/attack/interaction commands are canceled when suppression begins, and HUD widgets/pointer actions are blocked behind Options.

Keyboard/mouse actions exposed in this first pass include WASD composite parts, move destination, mode switch, stop, interaction, targeting/attack/clear/return/inventory, camera pan/drag/recenter/follow/rotation/look, and the Options toggle. Interactive rebinding uses Unity Input System APIs, Backspace cancels capture, shared bindings are allowed with explicit conflict text, and Reset Defaults persists. Zoom, raw pointer delta, and fixed Escape-to-close-Options remain non-rebindable for now.

Overrides persist outside Echo saves at `Application.persistentDataPath/Settings/bindings.json` with lock/pending/backup handling. Corrupt primary settings recover from backup; no gameplay save-schema change was made.

Final automated evidence on the cleaned implementation tree:

- `DICEFREE_CONTROLS_ASSET_OK`
- `DICEFREE_CONTROLS_OK`
- `DICEFREE_CONTROLS_PLAYMODE_OK` (F10 open, modal suppression, interactive Interact -> J rebind, live J action firing, gameplay resume, Escape behavior, reset defaults)
- `ISSUE36_ROUTE12_OK`
- `DICEFRE_ARCHITECTURE_OK`
- `DICEFRE_ARCH_POLICY_SELFTEST_OK`
- `DICEFREE_MANAGED_REFERENCE_OK`
- Windows Development build `DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK`: **187,323,162 bytes**, **0 errors**, **4 warnings**.

Detailed evidence: [OPTIONS_KEYBINDS_VALIDATION.md](OPTIONS_KEYBINDS_VALIDATION.md). Canonical behavior: [INPUT_CONTROLS.md](INPUT_CONTROLS.md).

Human review is still required for Options layout/labels/rebinding feel. Audio/graphics options, controller rebinding, final control profiles, and production menu UI are explicitly out of scope. Nothing is merged.

## Issue #46 Farmer's Pants implementation - 2026-10-05 (pushed for review)

Working on `art/46-farmers-pants` from verified clean glove head
`e7ce68909307159737fa83b57cd503a1f7a25389`; issue #46 and #43/#45 bodies and
comments inspected. No merge is authorized. Settled item is
`item.cornberg.farmers-pants`, **Farmer's Pants**, Legs, exactly +1 Physical
Defense and +1 Vitality. Prototype only: existing explicit development grants,
no vendor/quest/dungeon/drop balance or startup grant.

The existing equipment stat package lacked primary attributes. Use a generic all-five AttributeValues contribution through ActorStats equipment sources; existing
Vitality HP/regeneration and highest-attribute attack formulas apply normally.
Preserve missing HP on maximum changes; no independent extra bonus. Extend the
#45 EquipmentPresentation binding with authored baseline visuals and exact
active-state restoration; gloves retain empty baseline replacement lists.
Blender source/export uses the unchanged, previously repaired Novice rig.
Two builds match geometry/weights fingerprint
`8a22ff06453727d91ecfa992c8832944ebd7f882fc2c4eb630be28486174e22d`.
No additional skeleton/weight repair is currently required. Existing clips
rotate thighs but have no independent knee articulation; add a separately
labelled synthetic knee-flex check. Feet art remains on its separate unmerged
branch, so coexistence uses authored test-only foot-socket fixtures.
Implementation is pushed at `7f77b79724b2135674ca248548552f4c6d9289a1`.
Unity import, focused pants runtime/reload validation, glove regressions,
existing item/Q4 suites, Novice pipeline/player-root, preview and architecture
gates passed. Final Windows Development build: 188,417,911 bytes, zero errors,
one warning. Earlier bridge-timeout/compile-overlap build attempts are
superseded by the successful structured response. See
[validation evidence](FARMERS_PANTS_VALIDATION.md) and
[source reproduction](../SourceArt/Characters/FarmersPants/README.md).
Regenerated containers/scene IDs and incidental Unity settings were preserved
in an ignored local snapshot, then restored to the pushed assets.
Next: human preview/cloth review. Feet uses test fixtures, knee articulation
includes a labelled synthetic check, and production optimization/cross-form
fitting remain provisional. Issue #46 remains open; nothing is merged.


## Issue #45 implementation and validation - 2026-10-05

Branch `art/45-cornberg-work-gloves` starts at `b407ee3` (the same head as
`art/43-novice-cornberg`). Issue #45 and its empty comment list were inspected,
as were the separate shoe/hat branches; neither branch is merged.

Pose review found a concrete pre-existing Novice skinning defect before fixing
it: `make_skinned_family` compares mesh-local `vertex.co` to character-space
bone endpoints. Joined meshes retain the first part's translated origin, so
the nearest-bone assignment is wrong. Actual hand bones move away from visible
baseline hands, and locomotion splits the head. A coordinate-space correction
is necessary for gloves to follow the real hands. Preserve the original mesh
geometry/materials/clothing objects and gameplay root; fix skin weights only.
No Q4 reward or attack-speed formula defect was found or changed.

Implemented paired worn-leather/canvas skinned glove overlays, reproducible
Blender source/export, Unity materials/wrapper and an authored Cornberg binding.
The small UI `EquipmentPresentation` seam maps slot plus stable ItemDefinition
ID to visuals, responds to equipment changes and preserves the four baseline
renderers. No quest/name parsing, runtime bone-name lookup, extra Animator,
collider, save schema or gameplay-authority change. Empty Hands remain bare.

Validated: two matching Blender exports, closed/outward meshes, clean Unity
compile/import, five samples each of idle/locomotion/attack on both hands,
actual skinned-hand/glove centroid error below 4 cm, equip/unequip and disable/
re-enable, exactly 1.05 attack multiplier and no extra stats, Q4 exactly-one
reward, equipped/unequipped reload with stable GUID, unresolved-item inertness
and equipped-item removal. Existing item and full Q4 harnesses passed (both
routes, seven reloads and real 450-second respawn); existing two-phase save/
migration/recovery, architecture, policy, managed-reference, Novice pipeline
and Cornberg player-root checks passed. No assertions were weakened.

Windows x64 Development build: 187,073,485 bytes, zero errors, two existing
warnings (no runtime Pipeline config and InventoryPanel UAC0009). Output is
`%TEMP%/DiceFree-Novice-Cornberg-Issue43/DiceFree.exe`, using the existing build
command. A 29.5-second isolated standalone startup/reload smoke passed with
schema 4, revision 2 -> 3, four owned instances, three equipment references,
the same equipped Hands GUID and 37 gold, with no runtime errors. No manual
playthrough or standalone visual inspection is claimed.

Unity review PNG:
`Assets/_DiceFree/Art/Validation/Previews/CornbergWorkGloves_Unity.png`.
Detailed reproduction, repair evidence and limitations are in
`SourceArt/Characters/CornbergWorkGloves/README.md` and
`docs/WORK_GLOVES_VALIDATION.md`. Next: Axel reviews the art and the necessary
skin-weight correction on this focused branch. Issue #45 stays open; nothing
is merged. Human art approval, animation/clothing polish and production mesh
optimization remain provisional. Shoe/hat branches remain separate.

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

## Latest completed increment: MSQ5 Runner departure bridge

Implemented and pushed on `poc/cornberg` in **69efbe9775d4fe00af5fd66356c1ca4871f6c6fc**. Q5 is
`quest.cornberg.become-runner`; it uses the existing `npc.cornberg.runner` and
requires only Q4 `quest.cornberg.break-slime-surge` completion. Level 10 is not a
prerequisite. A reusable `ContextualNpc` provides one authored, deterministic
interaction target that routes to the currently available story or quest action;
the preserved Runner Returns conversation remains available in its prior phase.

Q5 becomes Active with the future objective “Travel to the next settlement and
deliver Cornberg's message,” keyed by `area.world1.next-settlement-arrival`. There
is no matching Cornberg scene trigger, so it intentionally remains Active. The
message is represented by quest state; no reward or save-schema change is made.
No next-settlement lore, Q4 rebalance, mountain blessing, or advancement was
added. See `docs/CORNBERG_MSQ5.md` on PoC.

Focused Q5 and Runner Returns validation, Q1–Q4, item/equipment/gold, schema v1–v4
migration/recovery, road speed, trivial aggro, combat/death/Return/well, all 13
traversal routes in both control modes, legacy/item/dead reload checks, Windows
development build and two standalone Q4-completed/Q5-active reload smokes passed.
Q4 pacing remains unchanged (elite: level 9/10 XP; mass route: level 13/30 XP).
No manual playthrough is claimed; the known editor SearchDatabase #17 remains
separate.

## Issue #36 architecture hardening — finalization review branch

The controlled refactor branch was created from verified PoC `1b574291fafc6b90d2fac1365eb4931f4bd59fa0` and remains separate from both `poc/cornberg` and `main`. The finalization review starts at `090e5016082987d67b1d1a4ec968b56043e00057`; latest authoritative setup used was `f5a1ad999f5047be0326d3224adc7de73cf2ac05`.

Review commits:
- `25d50e94ff0806a18d3824fc0970c732fb7d9b40` — centralize/fix dependency policy and split Foundation contracts.
- `b3085bd26ee0441e254ac019e284ae57fa67bf44` — finalize architecture review and dynamic interaction registration contract.

Final implementation commit before handover-only sync: `b3085bd26ee0441e254ac019e284ae57fa67bf44`.

The validator defect is fixed: all first-party edge decisions now use one `Allowed` policy. This explicitly accepts Application -> UI/Persistence and Editor -> runtime while rejecting domain -> UI/Persistence/Application, runtime -> Editor, and all other unlisted first-party edges. Pure policy self-tests pass for the allowed and forbidden examples, A -> B -> A cycle detection, missing source ownership, and an invalid asmref target. The self-test helper is shared by the actual validator and was executed separately in an isolated .NET harness.

Offline inspection of actual project files found 10 first-party assemblies, four resolving asmrefs, and 132 explicitly owned first-party C# files (91 runtime, 41 Editor). There is no Assembly-CSharp leakage, no first-party dependency cycle, no domain -> concrete Persistence/UI/Application edge, and no runtime -> UnityEditor edge. The assembly graph remains Foundation -> Gameplay -> Items/World -> Quests/AI; Persistence and UI depend on domain assemblies; Application composes runtime domains, Persistence, and UI; Editor references runtime. The actual graph omits allowed-but-unused edges such as AI -> World.

Foundation now keeps `IDurableMutationCoordinator` and `ITraversalUiState` in separate focused files. Rewards still flow QuestJournal -> FixedRewardGrant -> the Foundation mutation contract, implemented by ManifestationPersistence; prevalidation, completion-before-reward-event reentrancy protection, deferred durable snapshots, rollback, and reward idempotency remain. Persistence schema v4 is unchanged.

`FixedWorldDrop` and `WorldEquipmentPickup` remain in namespace `DiceFree.Items` while physically owned by Application. This is deliberate: namespace expresses item feature vocabulary and assembly ownership enforces dependency direction; renaming would create needless type/serialization churn.

The local `InteractionRegistry` remains instance-owned. Existing authored targets get one startup compatibility bootstrap, never an input-time scan. Dynamic world-content factories must explicitly register/unregister root targets; child contextual actions are not catalog targets. Explicit unregister detaches the target; ordinary disable removes membership but preserves its binding for re-enable. Click and keyboard continue to query the same catalog.

Compatibility static state intentionally remains: `CombatActor.All`, Defeat/Area/Conversation typed fact streams, `TravelSurface.Active`, and HUD pointer widget state. SubsystemRegistration resets session state; lifecycle handlers unsubscribe/remove disabled entries; destroyed HUD entries are pruned. Disabled actors are unregistered; dead enabled actors remain available for respawn but report not alive and have root colliders disabled until restore. Full instance-owned session/event publishers, actor registry, and authored travel-surface provider remain documented debt.

Source-group Roslyn compilation passed for all ten assemblies, with only existing UAC0009 / FindFirstObjectByType deprecation and tooling warnings. Offline real-manifest inspection and policy self-tests passed. A fresh Unity batch attempt reloaded scripts but failed Licensing Client IPC initialization; `DiceFree.EditorTools.ArchitectureValidation.Run` did not execute. Gameplay/scene/content regressions, Windows build, and standalone Q4-completed/Q5-active reload are therefore pending. No Cornberg scene, NavMesh, gameplay content/balance, or save schema changed. Q1-Q5 behavior is intended to remain unchanged.

**Readiness: case B — structurally reviewed, still blocked on Unity validation. Not ready for integration.** Do not merge this branch into PoC or main until Unity architecture validation, gameplay suites, Windows build, and standalone reload pass and Axel reviews the result.
## Previous completed increment: Cornberg Q4

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

**Current next step:** finish #36 Unity validation on `refactor/architecture-hardening` after Licensing Client IPC is available. The branch is structurally reviewed but not ready for integration. After that review, continue MSQ5 only when the next-settlement destination and delivery/reward continuation are authored. Q4 pacing remains available for Axel's human playtest review and must not be changed during architecture work.

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

Q1 through Q4, Runner Returns and the MSQ5 departure bridge are complete on
`poc/cornberg`. Q5 is intentionally Active at the current world boundary. The
sections below retain earlier increment history; the Q5 status above supersedes
older future-work wording.

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

Canonical design: `docs/CODEX.md`.

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
- equipping multi-slot gear auto-unequips conflicts to inventory when capacity permits; otherwise the equip fails atomically;
- use a general occupied-slot data model rather than scattered two-handed checks;
- descendant Ways may fully transform body/species/rig again;
- similar bodies should share rig/animation families where practical, while substantially different/non-biped forms may use separate families;
- visible armor normally replaces the class form's baseline visual for that slot;
- class form affects authored gameplay hitbox/physical/navigation footprint; body size is not presentation-only;
- attack/basic-attack/ability reach remains separately authored from body footprint;
- large player forms get authored navigation/clearance forgiveness where needed so required routes never soft-lock them;
- descendant Ways inherit parent equipment-family permissions by default, with explicit add/remove/override support;
- visible armor layering: Chest/Head/Hands/Legs/Feet replace baseline slot visuals; Shoulders and Back remain independent layers;
- transformation presentation is optional per advancement and never owns the actual class/manifestation mutation;
- larger authored form hitboxes are real combat volumes and may be easier for spatial enemy/projectile collision to hit;
- players physically block other players with solid collision/sliding and **no pushing**; enemies physically block players;
- no generic anti-box-in or unstuck escape rule: combat encirclement is intentional, while required world-route navigation forgiveness remains separate;
- explicit **Return to current revive point** ability direction is now settled: 10-second rooted cast, movement/damage interrupt, cannot start with active aggro/threat, disabled in active dungeons, ~10-minute cooldown only on successful Return, canceled/interrupted casts do not consume it, and cooldown resets on death/logout/reload rather than persisting;
- friendly projectiles pass through allied player bodies by default;
- ordinary AoE uses authored hit-volume overlap rather than actor-center-only checks;
- movement speed is class-authored/stat-derived, never inferred automatically from body/species/leg count;
- advancement that invalidates equipped gear preserves it atomically: child inventory first, Echo-wide bank fallback, otherwise abort before mutation;
- unequipped visual slots may show authored default clothing/armor or intentionally exposed body, per form/slot;
- ADR-0004 records the class-form/equipment-compatibility decision.

Tracking:
- **#36** architecture hardening / asmdef dependency enforcement — implementation and structural review are pushed; Unity validation is pending Licensing Client IPC and the branch is not yet ready for integration;
- **#37** first stylized sword / 3D pipeline proof.

# Current recommended next implementation step

**Next step: restore a working licensed Unity batch/editor environment and finish #36 validation.** Run `DiceFree.EditorTools.ArchitectureValidation.Run`, then the required Q1-Q5, item/reward, persistence/recovery, combat/traversal, scene/content, Windows build, and isolated Q4-completed/Q5-active reload checks on `refactor/architecture-hardening`. The branch is structurally reviewed but is **not ready for integration** until those checks pass. Axel/ChatGPT should review the final diff before any integration decision. Do not start #37 sword work during this validation cycle.

Q4 pacing remains unchanged and pending human playtest review: elite route reaches level 9/10 XP and the 30-Road-Slime route reaches level 13/30 XP from level 6/0 XP. Do not rebalance it during architecture finalization.
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
- **Confirmed Q4:** `39ebbf29c809bf08e7d4bc2626f80714159e85e5`; focused/regression/build/two standalone reload smokes green. Issues #6/#15/#22/#28/#24/#26 updated. Q4 remains unchanged by MSQ5.

# MSQ5 implementation handover — 2026-10-01

- **Implementation branch/SHA:** `poc/cornberg`, `69efbe9775d4fe00af5fd66356c1ca4871f6c6fc` (Add MSQ5 Runner departure bridge).
- **Stable IDs:** Q5 `quest.cornberg.become-runner`; existing Runner `npc.cornberg.runner`; unresolved future destination `area.world1.next-settlement-arrival`.
- **Availability:** Q4 `quest.cornberg.break-slime-surge` Completed. There is no level-10 prerequisite.
- **Interaction:** the Runner now has one discoverable `ContextualNpc` target with explicit available-action ordering. It preserves Runner Returns before Q4 and selects the Q5 action after Q4 completion; Q5 active conversation has no offer marker.
- **Endpoint:** Q5 persists as Active because no destination ReachArea exists in Cornberg. It grants no acceptance/completion reward and does not fake arrival. No physical message item is created.
- **Persistence:** save/manifestation schema v4 is unchanged; five focused Q5 reload checkpoints and two standalone reloads passed. Q4 state/reward and item instance remain exactly once.
- **Validation/build:** focused Q5 and Runner Returns; Q1-Q4; item/equipment/gold; schema v1-v4 migration/recovery/backups/stale writers; road speed; trivial aggro; combat/death/Return/well; all 13 routes and both controls; legacy/item/dead reloads; Windows development build (172,039,389 bytes); two standalone startup/reload checks passed. No manual playthrough claimed. The known editor SearchDatabase #17 remains separate.
- **Q4:** unchanged; measured level 9/10 XP via elite, level 13/30 XP via 30-Road-Slime route. No rebalancing.
- **Issues:** #6/#15/#22 updated with implementation SHA, states, future destination, validation, unchanged Q4 pacing and deferred boundaries.
- **Next:** issue #36 architecture hardening is the immediate engineering priority recorded in the current design direction. Q5 progression beyond Cornberg still needs the next-settlement destination and delivery/reward authoring; do not invent those details.


Additional collision rules settled 2026-10-01:
- dead actors stop body-blocking immediately;
- friendly summons/pets/companions block owners and allies by default;
- ordinary friendly NPCs block players by default;
- these use solid/no-push collision unless an authored exception says otherwise.


# Post-MSQ5 code review — 2026-10-01

Verified implementation state remains `poc/cornberg` `1b574291fafc6b90d2fac1365eb4931f4bd59fa0`. Q5 `quest.cornberg.become-runner` uses the existing `npc.cornberg.runner`, is gated only by completed Q4, and intentionally remains Active at `area.world1.next-settlement-arrival`. It has no reward or level-10 prerequisite. Q4 pacing/content remains unchanged.

The earlier issue #36 review found the validator/UI policy contradiction and requested the Foundation interface split. Both findings are fixed on the separate `refactor/architecture-hardening` branch described above. The finalization status and validation boundary are in the following handover section. Q1-Q5 gameplay and save schema v4 were preserved; no merge has occurred.
# Architecture branch finalization — 2026-10-01

- Finalization implementation commit on `refactor/architecture-hardening` (before handover-only sync): `b3085bd26ee0441e254ac019e284ae57fa67bf44`.
- Review-fix commits: `25d50e94ff0806a18d3824fc0970c732fb7d9b40`, `b3085bd26ee0441e254ac019e284ae57fa67bf44`.
- Validator contradiction fixed; pure policy self-tests and offline real-manifest/source-ownership inspection passed (10 assemblies, 4 asmrefs, 132/132 sources owned, no cycles or forbidden first-party edges).
- Foundation contracts split; reward/persistence decoupling and transaction semantics retained; interaction registration lifecycle documented and explicit for dynamic factories; remaining static session compatibility seams remain documented.
- Source groups compile with existing warnings. Unity architecture validator and gameplay suites did not execute because Licensing Client IPC was refused. Windows build and standalone Q4-completed/Q5-active reload are pending.
- Scene/NavMesh, Q1-Q5 behavior/content, authored balance, and schema v4 are unchanged.
- **Readiness B:** structurally reviewed but not Unity-green and not ready for integration. Nothing merged.
- **Next:** obtain working Unity licensing, run the full requested validation, then request Axel's review. Do not begin #37 before this cycle is complete.


# Architecture follow-up review — code blockers cleared, Unity validation still pending

Reviewed `refactor/architecture-hardening` at
`0a9fb6435a3c02b71e0316fe0171bc74c0ce689c`.

The previous validator-policy defect is fixed:
- dependency policy is centralized in `AssemblyDependencyPolicy`;
- Application -> UI/Persistence remains explicitly allowed;
- domain -> UI/Persistence/Application remains forbidden by policy;
- focused policy self-tests cover intended allowed/forbidden edges, cycles, source ownership and asmref validity;
- `ITraversalUiState` was split out of `IDurableMutationCoordinator.cs`;
- InteractionRegistry lifecycle registration/unregistration behavior was clarified and is coherent.

No additional code-level architecture blocker was found in this review.

The branch is still **not ready for integration** solely because Unity-dependent evidence is missing:
- real in-Unity `ArchitectureValidation.Run`;
- Q1-Q5 / item / persistence / combat / traversal regression suites;
- Missing Script/content validation;
- Windows development build;
- isolated Q4-completed/Q5-active standalone reload.

The repeated blocker is Unity Licensing Client IPC failure, not a known test failure.

Next Codex cycle should focus on validation/environment recovery only. Do not continue architecture churn unless a real Unity validation run exposes a defect. Do not merge #36 or begin dependent gameplay-system growth before Unity-green evidence and final review.


# Issue #36 validation-only retry — Unity licensing remains blocked (2026-10-01)

The validation-only retry was performed against `refactor/architecture-hardening`
`0a9fb6435a3c02b71e0316fe0171bc74c0ce689c`, using setup handover
`1029a1f5c419e109ff7caa80cd89940291534969`. Unity Editor
`6000.6.3f1` (`T:\TEMP\unity\6000.6.3f1\Editor\Unity.exe`) was not running
before the retry, so no user-owned project lock was present. Unity Hub remained
running (PIDs 3848, 12844, 20440, 22368, 23800, 27084); the Licensing Client
remained running (PID 22096). No Hub or licensing process, license file, or
preference was changed.

The one retry ran:

`Unity.exe -batchmode -nographics -projectPath T:\TEMP\DiceFree\DiceFree -executeMethod DiceFree.EditorTools.ArchitectureValidation.Run -logFile T:\TEMP\dicefree-architecture-validation-issue36-retry.log`

Unity reached `Scripting: successfully reloaded assembly`, but did not reach the
requested architecture method. The log records `Connection to channel
LicenseClient-Axel refused`, a 60-second wait timeout, licensing initialization
failure after 74.81 seconds, a relaunch of the Licensing Client, another refused
reconnection, and `Error: 'com.unity.editor.headless' was not found.` The
Licensing Client's own log at
`C:\Users\Axel\AppData\Local\Unity\Unity.Licensing.Client.log` shows successful
local entitlement resolution/activation through Unity Hub, so the precise
failure is the Editor batch process not establishing its expected licensing IPC
channel/entitlement; it is not evidence that the architecture validator failed.
The known log from the previous attempt is
`T:\TEMP\dicefree-architecture-validation-final.log` and records the same
`LicenseClient-Axel` refusal and initialization timeout. The headless Unity
process started for this retry was stopped after it remained blocked; no user
Editor was terminated.

Actual in-Unity architecture validation, Unity gameplay regressions, scene
Missing Script/content validation, Windows development build, and standalone
save/reload smoke remain **not executed**. Assembly reload alone is not being
reported as a successful Unity compile/import validation. No code was changed;
no scene, NavMesh, gameplay data, or schema v4 content was changed. The branch is
still structurally reviewed, **not Unity-green, and not ready for integration**.

Smallest safe user action: open Unity Hub, launch this DiceFree project normally
once, and confirm that the Editor opens with its license recognized; then close
that Editor and retry the prescribed batch validation. No reinstall or license
reset is indicated by the available logs. Do not begin #37 until the required
Unity evidence is green.


# Post-popup Unity retry — licensing IPC still blocked (2026-10-01)

After Axel reported accepting a Unity popup, a fresh environment check found no
open Unity Editor. The Unity Hub processes and Licensing Client PID 22096 were
still running. One new batch retry was made against the unchanged tested code
SHA; it again reached `Scripting: successfully reloaded assembly` but failed
before `ArchitectureValidation.Run` with `Connection to channel
LicenseClient-Axel refused`, the 60-second initialization timeout, and
`'com.unity.editor.headless' was not found.` Full log:
`T:\TEMP\dicefree-architecture-validation-after-popup.log`. The retry's own
headless Editor process was stopped; Hub and Licensing Client were left alone.

The popup acceptance did not resolve the batch IPC failure. No code or project
content changed. All Unity-dependent architecture/gameplay validation, Windows
build, and standalone reload remain unexecuted; readiness remains **B: not
Unity-green, not ready for integration**. Next safe action: launch this exact
project normally from Unity Hub and confirm the Editor opens with the license
recognized, then close it and retry batch validation. Do not reset licensing or
reinstall based on this evidence alone.

# Unity CLI + Pipeline automation bridge (2026-10-03)

Unity CLI `1.0.0-beta.12` is installed and authenticated; Unity Personal license
is active, and Unity Editor `6000.6.3f1` was opened through Unity CLI and
reported ready with `com.unity.pipeline` connected. Agent validation/build work
must use the connected CLI + Pipeline path described in
[`UNITY_AUTOMATION.md`](UNITY_AUTOMATION.md); do not ask Axel to click Unity
validation menus unless the official route is unavailable or requires a
one-time authentication/consent action. The project-local Unity CLI and
Pipeline skills are installed under `.agents/skills/`, and the official Codex
MCP integration is configured for this project.

The Editor-only Pipeline command surface includes architecture validation,
policy self-tests, managed-reference validation, Route 12, dedicated Runner,
two-phase save/reload, and task status. Connected Editor runs passed the three
architecture checks and all three Issue #36 focused checks. Runner/save test
fixtures were explicitly registered in their Editor harnesses; no gameplay or
save-schema code changed. The save/reload task crosses Edit/Play Mode by an
idempotent SessionState-backed phase transition and returns machine-readable
results. Implementation commits: `9d9fb5a` (Pipeline package) and `df9189d` (validation command surface). The final branch head also includes this documentation update.
Nothing has been merged; #36 remains open pending integration review.

## Issue #36 integration follow-up — 2026-10-03

The reviewed `refactor/architecture-hardening` candidate
`189e59f2b9db7a5662d23f4783eb54209182b0b1` is a direct descendant of the
previous `poc/cornberg` head `1b574291fafc6b90d2fac1365eb4931f4bd59fa0`
(19 commits ahead, zero behind). Integration preserves that history by
fast-forward; it does not squash or create an unnecessary merge commit.

On the connected Unity `6000.6.3f1` Editor at this integrated code SHA, the
architecture validator, architecture policy self-tests, managed-reference
validator, Route 12, Runner Returns, and two-phase save/reload all passed:
`DICEFRE_ARCHITECTURE_OK`, `DICEFRE_ARCH_POLICY_SELFTEST_OK`,
`DICEFREE_MANAGED_REFERENCE_OK`, `ISSUE36_ROUTE12_OK`,
`ISSUE36_RUNNER_OK`, and `ISSUE36_SAVE_RELOAD_OK`. The previously recorded
Windows x64 Development Build also passed on this exact code SHA with zero
errors. The follow-up PoC commit carries documentation updates only.

The current canonical art proof order is sword, actual Novice body/model,
shield, shoes, then hat. The actual Novice model should establish the real rig,
sockets, body scale and baseline presentation before more wearables are made.
The normal first-advancement path targets level 10 in roughly 30 minutes or
less and should not approach an hour; the optional level-200 Novice path stays
available. The current `ART_PIPELINE.md` and `classes/NOVICE.md` reflect the
latest setup decisions. `UNITY_AUTOMATION.md` from the reviewed #36 candidate
is retained; setup's newer handover refers to it but does not contain a newer
copy. Architecture and technical-direction docs from #36 remain unchanged.

This follow-up supersedes earlier pending-integration status above. Issue #36
is closed after the integrated checks passed. No main merge was performed and
#37 or Novice modeling work has not started.


# Issue #49 — complete Novice skill kit implementation (2026-10-06)

Current review branch: `feature/49-novice-skills`, stacked directly on review-ready #48 base `e0c26b5c3cb195147918075a5f9dc86931566707`. Nothing is merged to `poc/cornberg` or `main`.

Implemented the complete currently designed Tier-0 Novice systems slice:
- four rank-0..10 actives plus the 160-rank all-stat passive;
- one available skill point at level 1 / total available points equal current Novice level;
- STR x2 melee damage + rank x0.2s stun, bypassing ordinary stun resistance but not hard immunity;
- Magic Sand INT x2 + rank x7.5% miss chance for explicit accuracy-tagged actions for 5s;
- AGI self/ally attack-speed buff `10 + rank x 0.2 x AGI` percent for 5s;
- SPI self/ally instant heal `SPI x 10` through ordinary healing modifiers, no resurrection;
- stable-effect refresh/replace semantics for Magic Sand and the AGI buff;
- provisional cooldowns 10s / 12s / 15s / 20s;
- explicit `requiresAccuracy` tagging on existing basic attacks plus a dedicated accuracy resolver; no hidden universal miss roll;
- source-keyed temporary attack-speed modifiers, authored stun resistance/hard immunity, and source-keyed movement blocking for stun;
- friendly target retention for support casts while hostile cycling/context attacks remain hostile-only;
- canonical #48 input asset extended with 1-4 skill slots and K skill-allocation menu, using the existing rebinding/settings service;
- lightweight Novice skill bar/allocation UI and point-available notification;
- free-respec progression operation; NPC/service presentation is not authored yet;
- schema/manifestation **v5** adds stable-ID `classSkills` rank records. v1-v4 migrate explicitly; old manifests receive an empty allocation; v4 Q4 alternatives and v3 item-bearing durable state are preserved; unknown sections remain opaque.

Focused validation passed in connected Unity 6000.6.3f1:
`DICEFREE_NOVICE_SKILLS_AUTHORED_OK`, `DICEFREE_NOVICE_SKILLS_DATA_OK`, `DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`, `DICEFRE_ARCHITECTURE_OK`, `DICEFRE_ARCH_POLICY_SELFTEST_OK`, `DICEFREE_MANAGED_REFERENCE_OK`, `DICEFREE_CONTROLS_OK`, `DICEFREE_CONTROLS_PLAYMODE_OK`, and `DICEFRE_NOVICE_CORNBERG_PRESENTATION_OK`.

Existing regressions passed: items, Q4 surge/reward/elite/world-drop, `ISSUE36_ROUTE12_OK`, `ISSUE36_RUNNER_OK`, and `ISSUE36_SAVE_RELOAD_OK`. The #49 Play Mode harness uses real Cornberg actors and a two-phase isolated save/reload; it proves all four casts, support targeting, stun resistance/immunity behavior, deterministic Magic Sand accuracy boundaries, refresh-not-stack effects, passive/skill-point accounting, v5 persistence/reload and free respec.

Windows x64 Development Build passed: `DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK`, 188,493,665 bytes total, 0 errors, 5 existing prototype/build-environment warnings. Detailed evidence: `docs/NOVICE_SKILLS_VALIDATION.md`.

Still provisional/out of scope: final names for the three non-Magic-Sand actives, final cooldown/balance values, skill VFX/audio/animation/icons, production HUD/editor, retired-adventurer respec presentation, and the undefined level-200 secret.

Tooling note for this machine: Desktop Commander Remote was restarted non-elevated to avoid Unity's Administrator warning. In that normal remote process environment `PROGRAMDATA` was absent; Unity Package Manager 24.18.0 crashes in `getLocalConfigFolder()` when that variable is missing. Supplying `PROGRAMDATA=C:\ProgramData` only to the Unity CLI/Editor process tree restored UPM/Pipeline without changing Windows globally.


# Issue #50 — manifestation advancement fork + start menu (2026-10-06)

Current review branch: `feature/50-advancement`, based directly on completed #49 commit `63593b1ff979b7c545edd81c718f34469a91947e`. Nothing is merged to `poc/cornberg` or `main`.

Implemented:
- data-driven current three-class catalog;
- data-driven level-10 Novice edges to Physically Blessed and Magically Touched;
- Tier-1 actor shells with settled base/growth attributes only (real kits/models/Mana remain future slices);
- Echo-level `echo:manifestations` roster v1 inside existing schema v5;
- atomic parent-preserving class fork;
- child level 1 / XP 0 / reset target-class skill state;
- snapshot of manifestation-owned quests/world state, anchor, gold, inventory and equipment;
- child item instance-ID regeneration with equipped-reference remap;
- duplicate target-class creation rejection;
- independent parent/child divergence and preserved-parent second branching;
- class-skill runtime seam so Novice-specific passive/UI/casting deactivates on non-Novice shells;
- shared save-root policy for gameplay persistence + menu;
- dedicated `StartMenu` scene as build scene 0;
- fresh-profile `Start as Novice`;
- existing-Echo manifestation list with saved levels, last-active marker and transactional selection;
- unresolved class saves surface unavailable rather than being deleted;
- normal in-world UI only creates new advancements; arbitrary live manifestation switching is not exposed.

ADR: `docs/adr/0005-manifestation-roster-and-load-selection.md`.
Detailed evidence: `docs/ADVANCEMENT_VALIDATION.md`.

Validation passed:
`DICEFREE_ADVANCEMENT_AUTHORED_OK`,
`DICEFREE_ADVANCEMENT_DATA_OK`,
`DICEFREE_ADVANCEMENT_PLAYMODE_OK`,
`DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`,
`DICEFREE_CONTROLS_PLAYMODE_OK`,
`DICEFRE_ARCHITECTURE_OK`,
`DICEFRE_ARCH_POLICY_SELFTEST_OK`,
`DICEFREE_MANAGED_REFERENCE_OK`,
items regression,
Q4 regression,
`ISSUE36_ROUTE12_OK`,
`ISSUE36_RUNNER_OK`,
and `ISSUE36_SAVE_RELOAD_OK`.

The #50 Play Mode harness now starts from the real StartMenu on an empty isolated root, enters Cornberg through `Start as Novice`, creates both Tier-1 branches, proves independent timeline state, exits, boots StartMenu again, verifies all three saved levels/active marker, chooses Physical through the menu, and proves Cornberg loads the selected Physical save.

Windows x64 Development Build passed: `DICEFREE_ADVANCEMENT_PLAYER_BUILD_OK`, 188,534,398 bytes, 0 errors, 5 existing prototype/build-environment warnings, StartMenu scene 0 + Cornberg scene 1.

Built-player smoke test also passed outside the Editor: fresh isolated save visibly booted the menu; clicking `Start as Novice` entered Cornberg as level-1 Novice and wrote the save; relaunching visibly showed `Choose a manifestation`, Novice level 1, last-active marker and `Play Novice`.

Legacy save regression needed one harness-only adjustment: #50 makes `AdvancementController` require `ManifestationPersistence`, so the test removes its temporary advancement UI/controller before intentionally destroying persistence. The original save-test semantics remain intact.

Still out of scope: final blessing ritual/quest presentation, real Tier-1 kits/Mana/models/basic attacks/equipment permissions, final title/menu art, class preview cards, archive/delete/recreate UI, multiple-Echo selector, production HUD and multiplayer.


# Issue #51 — Physically Blessed Novice complete playable Tier-1 (2026-10-07)

Current review branch: `feature/51-physically-blessed`, stacked directly on #50 commit `66160948e615b15feb2732c306f0cd57c29b02ad`. Nothing is merged to `poc/cornberg` or `main`.

Physically Blessed Novice is now a real playable manifestation rather than the #50 shell:
- dedicated slightly larger/stronger Novice-family presentation prefab with shared humanoid rig and no gameplay colliders/root motion;
- level-1 stats 12 VIT / 12 STR / 12 AGI / 5 INT / 5 SPI and documented average growth;
- adaptive physical basic attack from the higher of STR/AGI;
- reusable class-resource runtime/persistence framework;
- Physical Mana: maximum `60 + INT`, regeneration/s `4 + 0.1 x SPI`, explicit Persist policy;
- five six-rank Physical skills with one class skill point per Physical level;
- Heavy Strike, Guard, Quickening, ground-targeted three-wave Arrow Rain, and passive Martial Aptitude;
- source-keyed movement-speed, incoming-damage, basic-attack-damage and Physical-Defense modifier seams;
- prototype Physical skill bar/allocation panel/resource bar using the shared 1-4 input actions;
- class-presentation switching across manifestation activation;
- Physical skill ranks + Mana capture/restore through manifestation state while cooldowns/buffs remain transient.

Important defect caught by the #51 Play Mode gate: `ClassResourceProfile` was initially a second ScriptableObject type inside `ResourceDefinition.cs`, producing a runtime-unloadable asset with `m_Script: {fileID: 0}`. It was split into its own correctly named script and the Physical Mana asset now has a real MonoScript reference. Cold Editor restart + rerun passed.

Focused validation passed:
`DICEFREE_PHYSICAL_AUTHORED_OK`,
`DICEFREE_PHYSICAL_DATA_OK`,
`DICEFREE_PHYSICAL_PLAYMODE_OK`,
`DICEFRE_ARCHITECTURE_OK`,
`DICEFRE_ARCH_POLICY_SELFTEST_OK`,
`DICEFREE_MANAGED_REFERENCE_OK`,
and `DICEFREE_CONTROLS_OK`.

The dedicated Physical Play Mode harness proves real advancement from level-10 Novice, dedicated model activation, Mana, five-point level-5 allocation, Martial Aptitude, all four active casts against real Cornberg hostiles, ordinary-resistance Heavy Strike stun, Guard dual-channel mitigation, Quickening Attack/Movement Speed, multi-target/multi-wave Arrow Rain, save, StartMenu selection and Physical reload with class ranks/Mana/model intact and cooldowns reset.

Existing regressions also passed:
`DICEFREE_ADVANCEMENT_PLAYMODE_OK`,
`DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`,
`DICEFREE_CONTROLS_PLAYMODE_OK`,
`ISSUE36_SAVE_RELOAD_OK`,
items,
Q4,
`ISSUE36_ROUTE12_OK`,
and `ISSUE36_RUNNER_OK`.

Windows x64 Development build passed:
`DICEFREE_PHYSICAL_PLAYER_BUILD_OK`, 188,590,953 bytes, 0 errors, 5 existing prototype/build-environment warnings, StartMenu scene 0 + Cornberg scene 1.

Detailed evidence: `docs/PHYSICALLY_BLESSED_VALIDATION.md`.

Still provisional/out of scope: final numerical balance, bespoke skill icons/VFX/audio/animations, full weapon-family permission rules, production HUD, level-30 descendant implementations and Magically Touched Novice.


# Issue #52 — Magically Touched Novice + shared confirm-cast targeting (2026-10-07)

Integration base: `poc/cornberg` is already fast-forwarded through completed #51 commit `77716b2f4a7b102407bd582862fd5c3231c74bac`. Current review branch is `feature/52-magically-touched`; do not merge without Axel's explicit instruction.

Magically Touched Novice is now a complete playable first-tier manifestation:
- dedicated slightly smaller/frailer Novice-family presentation;
- level-1 10 VIT / 5 STR / 5 AGI / 13 INT / 13 SPI with documented growth;
- ranged magical basic attack scaling from higher INT/SPI;
- class-specific persisted Mana profile: 80 + 2*INT + SPI maximum, 5 + 0.15*INT + 0.15*SPI regeneration/s;
- five six-rank skills: Magic Sand, Mend, Fire Elemental Imbuement, Ice Burst, Mana Attunement;
- reusable Nature/Fire/Ice elemental assets and source-keyed basic-attack augment/movement-speed/Mana-aura seams;
- strongest-wins Mana-regeneration aura semantics;
- full three-manifestation Novice/Physical/Magical save + StartMenu coexistence.

User playtest feedback changed casting UX during #52. Skills must not auto-cast on the ordinary selected target. Shared `SkillTargetingController` now provides normal/confirm casting across all current classes:
- targeted hotkey enters hostile/friendly/ground reticle mode;
- left click confirms;
- RMB/Escape cancels without resource/cooldown;
- selection/basic attack remains independent;
- movement/ordinary selection input is suppressed for the confirmation/cancel frame;
- casters themselves no longer fall back to `TargetSelection.Selected` or implicit self-targeting for targeted support abilities.
Self-only Physical Guard/Quickening remain immediate.

Final focused/regression evidence on the hardened code:
`DICEFREE_MAGICAL_AUTHORED_OK`, `DICEFREE_MAGICAL_DATA_OK`, `DICEFREE_MAGICAL_PLAYMODE_OK`, `DICEFREE_SKILL_TARGETING_OK`, `DICEFREE_PHYSICAL_PLAYMODE_OK`, `DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`, `DICEFREE_ADVANCEMENT_PLAYMODE_OK`, `DICEFREE_CONTROLS_PLAYMODE_OK`, `ISSUE36_SAVE_RELOAD_OK`, `ISSUE36_ROUTE12_OK`, `ISSUE36_RUNNER_OK`, items, Q4, `DICEFRE_ARCHITECTURE_OK`, `DICEFRE_ARCH_POLICY_SELFTEST_OK`, `DICEFREE_MANAGED_REFERENCE_OK`.

The remote Play Mode harnesses can inherit Editor pause/scaled-time state. Magical, Physical, controls and Novice harnesses now explicitly restore the needed normal Play Mode state where appropriate. An intermittent Magical validation timeout was an inherited Editor pause/time-scale problem, not an Ice Burst gameplay deadlock. A one-off Novice AGI validator failure was not reproducible in a clean session; after harness hardening, the suite passed again.

Final Windows x64 Development build passed on the strict targeting/caster code: `DICEFREE_MAGICAL_PLAYER_BUILD_OK`, 188,638,878 bytes, 0 errors, 5 existing Unity 6.6 editor-tool deprecation/build-environment warnings, StartMenu scene 0 + Cornberg scene 1.

Detailed evidence: `docs/MAGICALLY_TOUCHED_VALIDATION.md`. Casting controls: `docs/INPUT_CONTROLS.md`. Class sheet: `docs/classes/MAGICALLY_TOUCHED_NOVICE.md`.

Next intended major milestone after #52 review/merge: proper generic HUD, then first slime dungeon vertical slice, then narrow two-player multiplayer PoC.

---

## 2026-10-07 world/dungeon/Codex foundation checkpoint

Active stacked branch: `feature/54-world-dungeon-codex` (based on unmerged #53).

Implemented foundation:
- six-face world represented by separate map definitions/edge links;
- session participant map registry supporting different players on different world faces;
- single live occupancy per stable dungeon ID with lease ownership and stale-lock recovery;
- different dungeon IDs can be occupied simultaneously;
- reusable dungeon condition evaluator and snapshotted run variants;
- authored `dungeon.slime` definition with average-party-level >= 50 Slime Regent route;
- generic `IEchoWideDurableState` persistence contract;
- Echo-wide `CodexProgression` section `echo:codex` v1;
- semantic gameplay event vocabulary for monster/drop/mechanic/dungeon discovery;
- functional movable Codex HUD panel;
- Codex button added to HUD menu strip;
- canonical docs in `docs/CODEX.md`, `docs/SLIME_DUNGEON.md`, and ADR 0006.

First Slime Boss scope is locked to **Slime Slam + Divide only**.

Slime Regent is a conditional higher-level route/boss. Slime Queen / Slime King remain later high-level hard-boss content.

Validation markers reached before checkpoint:
- `DICEFREE_WORLD_CODEX_OK`;
- `DICEFREE_HUD_DATA_OK` with 12 customizable HUD widgets;
- `ISSUE36_SAVE_RELOAD_OK`.

Do not merge the stacked branch without Axel's explicit approval. Next implementation target is the physical Slime Dungeon scene/blockout and reusable entrance/map-transition runtime.

## 2026-10-12 — Ironpine Wildlife v1.4 first playable pass

The Ironpine Forest now has authored 22 wildlife slots (10 Quillrats, 7 Grey Wolves, 4 Dark Wolves and one Alpha) on `feature/visual-overhaul-vis00`, with wildlife AI, combat, homing quills, Bleed, howl/Frenzy, telegraphed leap, roaming, respawns, local party credit and Codex hooks. See `docs/IRONPINE_WILDLIFE_IMPLEMENTATION.md` for complete details, verification and remaining polish. Isolated Unity compile, scene count validator, Play Mode startup smoke, and Ironpine original route validator passed; focused combat/party correctness playtests still pending. The models are procedural placeholders; animal audio and full authored animations are not yet built. Do not merge main without approval. Preserve unrelated modified `ProjectSettings/PackageManagerSettings.asset`.

 
## 2026-10-12 — Real Ironpine wildlife art, animation and local audio

Follow-up on commit b918bdf (wildlife v1.4). The user explicitly authorized shutting down the old unresponsive Unity GUI editor; it was closed without touching unrelated local project settings. The 210-pack local Asset Catalog was reviewed for wolves and quillrats. Willowwood's static lowpoly wolf was a style reference, while realistic wolf and PixeliusVita monster packs were mismatched. For portable/redistributable sources, all three actual creature models were authored from scratch in Blender: Wolf, Quillrat, AlphaWolf (.blend and FBX), with 15-bone skinning, 10 actions each, species material variants and complete scene replacement for 22 wildlife CombatActors. Runtime animation bridge chooses movement and action states; positional creature audio uses original procedural sounds and optionally three locally licensed SoundBits dog growl/bark/group recordings, kept gitignored under Resources/Audio/IronpineLicensed. In isolated Unity 6000.6.3f1, actor/art validation and `IRONPINE_WILDLIFE_PLAYMODE_OK` passed; skeleton-to-skinned-mesh validation reported 227 moving vertices for Wolf/Alpha and 158 for Quillrat. See `docs/IRONPINE_CREATURE_ART.md` and preview screenshots. Unrelated PackageManagerSettings.asset modifications remain unstaged; main untouched. Next: hands-on game-view art assessment and full encounter-combat tests before declaring wildlife final.


## 2026-10-12 — Realistic wolf visuals, X queue, southern bridge, willow foliage

The user explicitly approved use of the locally downloaded **Realistic Furry Wolf FREE** pack and the mixed-style Diceworld lore. Fully implemented in branch `feature/visual-overhaul-vis00`, with details/commands/tests in `docs/IRONPINE_OCT12_WOLF_BRIDGE_QOL.md`.

- The genuine Unity Asset Store wolf FBX, URP model, textures and original source animation set are installed **locally only** under gitignored `Assets/Wolf/`. A local prefab/controller/materials is generated under ignored `Assets/_DiceFree/Resources/IronpineLicensedWolf/`. **Never commit raw or generated licensed art.** Clones and computers without that pack automatically use the original Blender wolf family; quillrats remain original.
- Runtime swaps licensed Grey/Dark/Alpha wolf models, scales/tints them and creates subtle red Alpha eyes; editor installer maps ten real clips including WolfHowl_New, Jump_New, Bite, death and locomotion, with loops correctly flagged. Standard fallback materials convert to URP Lit at runtime if needed.
- Fixed X cancelling active commands: X enters attack-move targeting without cancelling the current action or command queue; Shift+X confirmations append multiple destinations. Extended regression verifies three ordered commands and preserved movement.
- Rebuilt southern road mesh to end at west/east edges of timber bridge, corrected ramps and kept plank deck visible; preserved corrected mesh on scene rebuild. Both deck edges and an actual across-bridge NavMesh path validated.
- Rebuilt Tree_Willow canopy Blender source/FBX so hanging foliage is attached to leafy boughs instead of floating below exposed bare spokes. Before/after design reference: `docs/screenshots/CornbergWillowCanopyFixed.png` (new Blender render).
- **Validated using isolated Unity 6000.6.3f1 batch**, including wolf installer/controller/shader validation, bridge geometry and NavMesh check, 22-creature Play Mode smoke, Alpha eye effect and 3-command X queue regression. An unrelated existing Unity Editor SearchDatabase startup indexing exception remains; tests themselves passed. Follow-up: visual Game-view QA with full local URP, real hands-on keyboard inputs, fur/LOD appearance, alpha eye intensity and road terrain blending.
- Preserve unrelated unstaged `ProjectSettings/PackageManagerSettings.asset`. No merge into `main` unless user approves.

## 2026-10-12 — Ironpine HP, URP quillrats, minimap attack-move, UI themes

Indexed 30 newly downloaded UI Asset Store packs; existing local catalog now has 240. Details in docs/UI_ASSET_THEME_CATALOG.md. Added 30 pack theme presets to five original themes; 28 original pack skins installed locally and gitignored, two procedural-only. HUD chrome now uses the optional pack panel/slot textures; all fallback on clones. Options reorganized into Gameplay, Audio, HUD/Themes, Keybinds, Session tabs. Converted all 30 Quillrat palettes to URP Lit. Increased quillrat HP from .70x to 7x, ordinary wolves from 1.2x to 12x, Alpha from 6.5x to 65x. X attack-move now works through minimap; Shift appends. Editor-side validation and isolated Play Mode minimap/HP probes pass. Preserve other local package/ShaderGraph settings and third-party audio metas; do not merge main without approval.

## 2026-10-12 follow-up playtest
See docs/OCT12_FOREST_PLAYTEST_FOLLOWUP.md for wolf visual forward orientation, doubled Alpha scale, improved enemy target facing and walking animations, 23 obsolete Ironpine slimes disabled, scoped high-tree collider cleanup, and five audio volume sliders. Preserve unrelated local changes; main remains unmerged.

## 2026-10-12 — shared actions, Return Home, new world design

See docs/SHARED_SKILLBAR_AND_RETURN_HOME.md: 4 class + 3 shared action slots, Attack/Attack Move X, Interact I, Return Home R (10s rooted cast/10m cooldown/no death heal, damage/movement/threat/dungeon restrictions, death/reload reset), three Battle Skill Icon pack artworks installed locally, 17 total, fallback original icons. Anchors autoactivate on approach and publish durable home ID; active wildlife aggro veto. Verified compile and isolated Play Mode. New art rule docs/ASSET_FIRST_ART_POLICY.md. Village 2, Tier II and Wolf Dungeon are a user-decision draft only in docs/WORLD1_VILLAGE2_TIER2_WOLF_DUNGEON_INTERVIEW.md; no world/class/dungeon content built. Keep local licensed art and unrelated project changes untouched.

## 2026-10-12 — Action Bar v2: embedded skill upgrades and persistent passives

User requested fixed shared commands in slots 7/8/9, not 5/6/7.
Novice, Physical and Magical now display their original four active skills in slots 1–4 and their class passive/aura in slot 5; slot 6 and 10–12 stay empty. A green '+' upgrade control overlays any active/passive skill with unused point budget; upgrades call the existing class progression Spend methods and preserve saves/rank caps. The separate skill allocation windows are retired (components kept for scene compatibility) and old keybind hidden. Tooltip preview is source-derived. See docs/ACTION_BAR_INLINE_SKILL_LEVELING.md. Unity compilation 0 errors/warnings; isolated Play Mode successfully exercised all 3 class skill allocations and fixed commands (marker DICEFREE_ACTIONBAR_REVAMP_PLAYMODE_OK); 17 local artwork textures still validated. Preserve all unrelated local files and do not merge to main without user approval.

## 2026-10-12 — Slot positions 10-12, Ironpine wolf balance, XP ladder audit, Village 2 discovery

- Per user, shared Attack/Interact/Return to Revive Stone moved **from 7-9 to 10-12** (1-based). First four active class icons and slot-5 passive remain; slots 6-9 are now reserved for later skills. Unity compilation and isolated all-three-class action bar Play Mode tests pass.
- Every Grey and Dark Wolf max HP multiplier doubled 12x -> 24x baseline, Alpha 65x -> 130x. Wolf basic bite/lunge/pounce damage +50%; Hunting Frenzy remains an extra +20% relative buff by scaling its contribution 20->30 alongside base 50. Quillrat HP, quill damage, XP/loot and 22-creature composition unchanged. Unity Play Mode passes strict stats multipliers for all 22 animals.
- XP ladder is still the provisional linear 30+10*(current level-1) curve, with 200 cap and reset on class advancement. Ironpine enemy kill XP is high vs these requirements; **audit only**, no XP changes. See docs/XP_LADDER_AUDIT_OCT12.md.
- Village 2 **design started**, not scene-authored. Review actual packs, safe arrival, Runner MSQ, revive stone, and Alpha proximity in docs/VILLAGE2_STARTING_PLAN.md. User input needed on village identity, safety and size. Do not invent theme or lock quest content before approval.
- Preserve unrelated user VFX and Unity project settings; main not merged.

## 2026-10-12 — Trimery identity and quadratic XP proposal

User canonized Village 2 as **Trimery**, lumber exporter to the Big City. Direct trade road is blocked by an unspecified event; players must detour through Biome 3 quilboar territory to reach the Big City. Later Big City quest reopens Trimery's direct road. Distinguish Ironpine animal quillrats from humanoid quilboars. Existing reserved location near X223/Z149, plus Alpha roaming ground nearby, creates a village safety question. See docs/TRIMERY_DESIGN_V01.md and top of docs/VILLAGE2_STARTING_PLAN.md. **No Unity world changes yet.**
User proposes changing provisional linear XP to quadratic (example a*Level^2). Recommendation to discuss is 30+b*(L-1)^2, b≈1 starter. Never change actual curve or XP awards until user approves coefficients and progression/save migration plan. See docs/QUADRATIC_XP_PROPOSAL.md. Existing + quota slot and wolf tuning code remains on feature/visual-overhaul-vis00; unrelated local files preserved.

## 2026-10-12 — Larger actor footprints, non-overlap, blue stun VFX, harder XP proposal

Gameplay code added real larger capsule + NavMeshAgent radii for players and all enemies; actor Configure() refreshes post-start values regardless of Ironpine Start order. Normal enemies +40% radius (min .54), player +50% (from .45 to .675); large bosses only +.30 max. ActorFootprintSeparation corrects overlap by moving only mover, with explicit future ghosting exception. TraversalMotor WASD sphere casts respect actor ghosting and still block on the world. CombatStatusController now spawns StunSpiralVisual (blue orbiting rings/motes) when stunned, clears on expiry/restore/death; optional glow from locally gitignored licensed URP VFX pack. See docs/ACTOR_COLLISION_AND_STUN_VFX_OCT12.md. Isolated test marker DICEFREE_UNIT_COLLISION_AND_STUN_OK confirmed 37 actors, wolf overlap correction and stun graphic life cycle. Manual pathing/art verification required, especially narrow bridges and boss arenas.
User says b=1 quadratic XP still too easy; docs/QUADRATIC_XP_PROPOSAL.md compares b=2 (recommended) and b=3. **XP NOT IMPLEMENTED.** Protect unrelated licensed files and Unity user settings. Main not merged.

## 2026-10-13 — shared channel progress and configurable overhead HP

ReturnHomeAbility implements IChannelingAbility and drives a movable/resizable themed ChannelBarHud; future spells use the same interface. OverheadUnitHealthBars draws all visible combat unit HP (and channel bars) above model height, obeying fog culling; self/ally/enemy default WhenHurt/WhenHurt/Always. Options > HUD has separate Always/Only when hurt/Never selectors. GameplayPreferences v1->v2 migration preserves previous input choices; unrelated Echo save schema remains v6. ActorFeedback nameplates continue with name/level and floating damage. Main Unity compile 0 errors and warnings; isolated PlayMode marker DICEFREE_HEALTH_CHANNEL_UI_OK; editor marker DICEFREE_HEALTH_CHANNEL_DATA_OK; existing dicefree.hud.validate and dicefree.ui.skills.inspect passed. Earlier pending dicefree.xp.quadratic.validate also passed after fast-forward sync. See docs/HEALTH_CHANNEL_HUD_OCT13.md. Manual visual QA still needed. Preserve unrelated local VFX/settings; no main merge.

## 2026-10-13 — Cornberg / Ironpine large QoL pass

See docs/OCT13_CORNBERG_IRONPINE_QOL.md. New real 3D animated quest !/? over QuestGivers; HP overhead +0.59m; north-up(default)/camera-up minimap, fog/compass/click mapping rotate; Quillrat rig yaw 180 and front muzzle projectile; new repeatable Mira quest Another Thirty Slimes after surge completed, 30 any-family slime kills, 120XP, reset after turn-in; Alpha auto 2.025x vs previous 1.5x (+35%); lower-left 3D target inspection for clicked actors/NPCs, actual combat stats; novice fists interval 1.1→1.35; 5 physical trees relocated off Ironpine road, 702 trunks validated; merchant and brewer/alchemist moved to front doors. Main Unity compile 0 errors/warnings; isolated Play Mode marker DICEFREE_OCT13_GAMEPLAY_OK with real NPC and wolf preview clones + repeat cycle twice; Ironpine route reachable, three sub-meter NavMesh sample gaps remain for manual barrier verification. Settings/gameplay.json migration v3 retains older options. Maintain independent user saves and unrelated VFX/Shader/Pack settings. Do not merge to main.

## 2026-10-13 — repeatable quest blue punctuation and Trimery planning

QuestWorldMarker renders distinct blue animated 3D ! when available quest.repeatable=true, instead of gold ! for fresh non-repeatable quests. Any quest ready for turn-in retains green ?. Priority green turn-in > gold main/new > blue repeatable. Unity compile zero errors/warnings; isolated play mode test exercised full blue/gold/green 3D state and existing repeatable turn-in. In-flight Trimery planning is captured in docs/TRIMERY_DESIGN_V01.md and docs/VILLAGE2_STARTING_PLAN.md. Confirmed lumber producer/exporter, blocked city road, required humanoid quilboar biome3 route and later city quest to reopen; village itself not yet authored and Alpha patrol proximity at (212,143) remains a key safety/layout blocker. User currently at work (cannot personally playtest); continue design discussion before building village.

## 2026-10-13 — Trimery three roads + Board-1 Roadwarden crossover brainstorm

User confirmed Trimery believable timber palisade/guarded entrance and exactly three roads: SOUTH to Ironpine/Cornberg, WEST to Big City (blocked), NORTH through quilboars to Big City. Important: BIG CITY itself is circular/ringed around a huge central tree, NOT Trimery. Ogre Roadwarden from DiceBound Board 1 Green Road tile50 is proposed as west-road disaster plot hook, but causality is NOT decided. DiceBound working Story Bible says Roadwarden has odd authority over Road and ancient stones recognize it. May provide a subtle World 2 thread and callbacks for DiceBound veterans; keep accessible to newcomers. Propose physically broken/warped passage and city quest repairs, not an invented stream/mountain. Details and open questions in docs/TRIMERY_DESIGN_V02.md. No scene implementation authorized until user picks disaster/story direction. No main merge.

## 2026-10-13 — confirmed Roadwarden damaged Trimery west road on mysterious orders

User explicitly confirmed Ogre Roadwarden destroyed Trimery's WEST trade route and was following orders, with major mystery WHO ORDERED IT and why. This is now canon; disallow prior theory that he merely guarded existing destruction. Also rejected magical ancient stone reacting to Echo—do not implement. DiceBound recognizability optional while standalone DiceFree story necessary. Full status/pitches and undecided Roadwarden visible-vs-vanished placement in docs/TRIMERY_DESIGN_V03.md. No Unity implementation or main merge yet.

## 2026-10-13 — Trimery first playable implementation (branch vis00)

User confirms Ogre Roadwarden remains at destroyed WEST road but accessible only later from Big City side. Encounter design deferred. Actual Trimery authored into Cornberg map at X248/Z180: three gates south/west/north, timber palisade, 6 watchtowers & 6 existing guard models, 7 lumber buildings, sawmill/log stock and market + 2 carts, Merchant/Banker copied and functional, Dice revive stone stable anchor.trimery at safe NavMesh X250/Z181.8, Cornberg Runner arrival trigger near south gate. Walkable terrain and all 3 road meshes; conditional larger CornbergNavMesh bake. Portable fallback art and locally ignored 4 asset-pack prefabs via TrimeryArtSlot. Unity compile 0 errors/warnings; dicefree.trimery.validate reports TRIMERY_VILLAGE_VALIDATED with six full NavMesh paths, bank/economy/perimeter/ironpine old validations passed. Isolated headless Play Mode marker TRIMERY_PLAYMODE_OK verified stone activation, Runner arrival, merchant/bank. No real user save used. Details docs/TRIMERY_IMPLEMENTATION_V1.md. Manual world graphics and Alpha safe-zone/road rubble bypass check still needed. No Biome3, Big City, Roadwarden boss built. Keep raw licensed art and unrelated material/settings changes ignored; main untouched.

## 2026-10-13 — Tier II v0.2 approved design corrections and BlendKit review

See docs/TIER2_CORE_CLASSES_DESIGN_V02.md. Greatblade is the **approved Tier II name**, 2H sword/Momentum. Later possible Greatblade->Berserker, Greatblade->Swordmaster->Samurai, and secret Greatblade->Witcher if *same Echo* has Magically Touched Novice manifestation milestone L30+ (working Witcher name; keep IP-distinct). Mage is INT-only, no SPR scaling and NO Arcane spells: Fire bolt, Ice Snare, Lightning chain, **Math-element shield**; user suggests wand+orb. Future Acolyte wand+book, Priest/Druid staff. Priest explicitly **two resources Mana + Grace**: EVERY cast costs Mana, Grace improves both healing and damage; exact gain/spend balance unresolved. Defender approved unchanged. IMPORTANT skillbar: not every class 4 actives+1 passive. Slots 1–8 flexible any combination active/passive/auras; **slot 9 reserved for quest-unlocked ultimate**, slots 10–12 Attack/Interact/Return fixed. This is design-only and requires future ability bar refactor. Evaluated BlendKit (formerly BlenderKit): its free rigged category is promising but rigged does NOT promise animations; licenses CC0 or Royalty Free, both allow commercial games but RF redistribution/extractability restrictions, favor CC0 for public GitHub, keep RF raw art out of repo. No new art or class implementations yet; do not merge main.

## 2026-10-13 — Greatblade refined kit + first Unity female humanoid visual prototype

User revised Greatblade: L1 Wide Arc + **ground-target Spin Dash** + Heavy Rhythm passive, also evolving Greatsword Mastery passive. Overhead Breaker first at L6, Battle Cry later. Spin Dash replaces Driving Charge, deals AoE along traversed path (actual damage/pathing NOT yet implemented). **Simple Momentum** generated on hits, spent on finisher, no threshold/combo bonus. Heavy Rhythm gains Momentum / reduces decay as ranked, maybe increases max. Greatsword Mastery rank1 slight greatsword damage+STR; rank2 improves both plus brief attack-speed after Wide Arc; rank3 improves earlier + widens Wide Arc; rank4 improves earlier + small finisher shield OR heal (undecided); rank5+ to be designed. **Ultimates exclusively Tier IV+ slot9**, skill unlock levels and upgrade rank breakpoints are NOT uniform. The retired swordswoman shares the exact base model with playable Greatblade, equipment differentiates eventually. See docs/GREATBLADE_PROTOTYPE_DESIGN_V03.md.

FIRST REAL UNITY VISUAL PROTOTYPE INSTALLED: Assets/_DiceFree/Scenes/GreatbladePrototype.unity (tracked portable preview scene with class+retired swordswoman sharing a runtime art source); portable fallback component GreatbladePrototypeVisual; local gitignored Female Warrior Princess A03.FBX, Blink Humanoid 2H/swing/spin/walk clips, PurePoly PP_Sword_0363 prefab. Manually mapped 21/64 bones for valid Humanoid Avatar after automatic importer rejected it; saved local controller, female avatar and prefab under gitignored Resources/GreatbladeLicensed. Unity compile zero errors/warnings; dicefree.greatblade.rig-human valid, prototype.install avatarHumanoid true, prototype.validate DICEFREE_GREATBLADE_VISUAL_AND_PROGRESS_OK (Humanoid motion clips, sword on R Hand, shared NPC/player base, stage preview and leveling gate tests). No actual Greatblade combat, Spin Dash damage, Tier II manifestation skills saving, quest or live playable class yet. See docs/GREATBLADE_PROTOTYPE_IMPLEMENTATION.md. Do not merge main. Keep licensed assets local and preserve unrelated uncommitted shader/material files.


## 2026-10-13 - Echo names and dynamic class titles

User wants the playable character named after the Echo followed by the current class title, e.g. `Krtz the Novice` / `ChatGPT the Greatblade`. Added optional Echo-wide `echo.identity` v1 section; existing schema v6 and all manifestation data stay compatible. StartMenu accepts the new Echo name and can rename an existing Echo; roster entries, overhead nameplates, stats header and selected-unit inspector use the same dynamic actor DisplayName. Titles come from the current class definition, so advancement and returning to a preserved manifestation update automatically. Existing unnamed Echoes default to `Echo` until named in StartMenu. UTF-16 name limit 32 characters; trims surrounding whitespace and rejects empty/control characters. Data validation and the isolated advancement/name/reload test are recorded in the implementation note. Preserve unrelated material/settings changes; do not merge main without approval.


## 2026-10-13 - World 1 foundation, development room and branch consolidation

User requested a main-menu development room, consolidated branch/local work, a complete World 1 prototype milestone and a small World 2 transition test. Work is on integration/world1-foundation, based on 1769a4e. Development room has an isolated Echo profile, real class/level/restore/item controls, three training targets and Greatblade visual previews. World2TransitionTest loads/unloads actual scenes while moving the same live session/player. Live isolated validation passed DICEFREE_DEVELOPMENT_ROOM_PLAYMODE_OK; architecture passed DICEFRE_ARCHITECTURE_OK; normal Echo naming/advancement/reload regression passed DICEFREE_ADVANCEMENT_PLAYMODE_OK. Restored historical shield/shoes/hat assets, source and preview artifacts; old alternate Options/validation code and old design documents are archived with inventories. Captured the 32 meaningful local first-party material differences. Local Package Manager preferences stay applied and are archived as a patch; raw licensed content stays ignored. Main is not the consolidation target. See DEVELOPMENT_ROOM.md, BRANCH_CONSOLIDATION_2026-10-13.md and WORLD1_FOUNDATION_MILESTONES.md. Greatblade combat, full World 1 content and multiplayer remain future work.


## 2026-10-13 - Foundation build warnings cleared

User requested cleanup of the 45 foundation Windows build warnings. Runtime Unity object searches use current APIs; fog disposal no longer hides the engine method; Escape removes unused bookkeeping; Inventory grants use runtime development checks. Both terrain meshes and their authoring routines enable triangle pre-baking. Core 17.6 GPU debug shaders have attributed URP-compatible copies and resource references; the optional ignored village shader gets an idempotent local macro guard repair. Post-processing shader stripping is disabled after a real Player launch exposed stripped-pass startup warnings. Pipeline diagnostics are configured for Development builds, and the custom builder sets/restores the Development toggle for package build processors. The latest 7-scene Windows build passed with zero errors and ZERO WARNINGS, 317898556 bytes. Real Player smoke reached StartMenu with advancing frames, active diagnostics and both compatible shaders supported; startup had no warnings/errors. Dev room/World2 isolated test, normal advancement/name/reload test, architecture and six Trimery routes passed. See BUILD_WARNING_CLEANUP_2026-10-13.md and foundation-warning-cleanup-report.json. Package Manager preferences remain unstaged, raw licensed files remain ignored, and main is unchanged.
## 2026-10-17 - Complete Sketchfab page-license audit

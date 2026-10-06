# Novice skill implementation validation (#49)

Validated on 2026-10-06 in Unity 6000.6.3f1 on `feature/49-novice-skills`, based on `feature/48-options-keybinds` commit `e0c26b5c3cb195147918075a5f9dc86931566707`.

## Implemented slice

- Four data-driven Novice active skills plus the 160-rank all-stat passive.
- Skill points: one available at level 1 and one additional point per character level; active ranks cap at 10.
- STR skill: Strength x2 physical damage, melee range, rank x0.2s stun, ignores ordinary stun resistance but respects hard Stun Immunity.
- Magic Sand: Intelligence x2 magical damage plus rank x7.5% miss chance for explicitly accuracy-tagged actions for 5s.
- AGI skill: self/ally attack-speed buff `10 + rank x 0.2 x Agility` percent for 5s.
- SPI skill: instant self/ally heal of `Spirit x 10`, using ordinary Healing Done/Received rules and never resurrecting.
- Same stable Magic Sand / AGI effect reapplication refreshes/replaces rather than duplicate-stacking.
- Provisional cooldowns: 10s / 12s / 15s / 20s respectively.
- Shared #48 input asset extended with 1-4 active slots and K skill-allocation menu; the new actions participate in the same rebinding system.
- Friendly world targets can remain selected for support skills; hostile Tab cycling and context/basic-attack validation remain hostile-only.
- Generic transient status seams cover stun, accuracy penalty and temporary attack-speed modifiers without introducing a global effect framework.
- Save/manifestation schema v5 adds stable-ID `classSkills` rank records. v1-v4 migration initializes missing skill state empty while preserving existing durable/unknown data.
- Free Novice respec is implemented as a progression operation; retired-adventurer NPC/service presentation remains future content.

## Focused validation

Passed markers:

- `DICEFREE_NOVICE_SKILLS_AUTHORED_OK`
- `DICEFREE_NOVICE_SKILLS_DATA_OK`
- `DICEFREE_NOVICE_SKILLS_PLAYMODE_OK`
- `DICEFRE_ARCHITECTURE_OK`
- `DICEFRE_ARCH_POLICY_SELFTEST_OK`
- `DICEFREE_MANAGED_REFERENCE_OK`
- `DICEFREE_CONTROLS_OK`
- `DICEFREE_CONTROLS_PLAYMODE_OK`
- `DICEFRE_NOVICE_CORNBERG_PRESENTATION_OK`

The Novice Play Mode harness uses real Cornberg actors and an isolated save root. It proves passive stat application, level-up point notification, all four real casts, selected-friendly support targeting, ordinary-resistance stun bypass vs hard immunity, deterministic Magic Sand hit/miss resolution around the rank-1 7.5% boundary, refresh-not-stack behavior, cooldown start, schema-v5 save, second-Play-Mode reload, and free-respec point refund.

## Existing regressions

Passed:

- existing items/inventory/equipment/gold regression (`items` suite);
- existing Q4 surge/reward/elite/world-drop regression (`q4` suite);
- `ISSUE36_ROUTE12_OK` traversal return-to-mountain check;
- `ISSUE36_RUNNER_OK` dedicated Runner Returns check;
- `ISSUE36_SAVE_RELOAD_OK` two-phase persistence/reload check, including genuine v1/v3 migration coverage updated for schema v5.

The migration validator also confirms the v3 item-bearing fixture migrates to the current schema with empty class skills, and the focused #49 data validator checks a synthetic v4 Q4-style manifestation retains its alternative-objective progress while moving to v5.

## Windows Development build

`DICEFRE_NOVICE_CORNBERG_PLAYER_BUILD_OK` passed with Unity 6000.6.3f1:

- result: Succeeded
- errors: 0
- warnings: 5
- total output: 188,493,665 bytes
- executable: `%TEMP%/DiceFree-Novice-Cornberg-Issue43/DiceFree.exe`

The warning count is from the existing prototype/build environment; no #49 compiler error or build failure was reported.

## Explicitly still provisional / out of scope

- The three non-Magic-Sand display names remain descriptive placeholders, not lore-canon names.
- Cooldowns and other numeric balance targets still require human playtesting.
- Final skill VFX/animation/audio/icon art is not part of this systems slice.
- The allocation/bar IMGUI is prototype UI, not the final 3x4 HUD editor.
- The retired-adventurer respec interaction is not authored yet; only the free respec operation exists.
- The level-200 Novice secret is not implemented because its design remains open.
- No merge to `main` or `poc/cornberg` is implied by this validation document.

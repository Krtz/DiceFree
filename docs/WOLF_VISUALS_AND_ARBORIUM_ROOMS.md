# Wolf clearing presentation and inhabited Arborium rooms

Scope: agenda items 3 and 4, requested 2026-11-05. Wolf completion drops and the requested seven loot entries are recorded in `WOLF_SURVIVAL_DUNGEON.md` for the subsequent reward pass. This change adds no loot, completion bonus, class progression or new city lore.

## Moonlit clearing

- Original 2048px terrain albedo blends irregular packed earth into moss and leaf litter. The walkable clearing and surrounding forest share world-aligned UVs; the old overlay disk and straight trail renders are retained but hidden.
- Three trails have open navigation through the inner trees. Nine existing trees are moved sideways with their matching boundary colliders, retaining original scene identities. Wolves spawn beyond the old warning pads, distributed laterally along the forest entrances, then pursue through the clearing normally.
- Wave banners and an existing wildlife-audio howl accompany each wave warning. The finale announces four alphas. Victory shows the completed wave/wolf counts and frozen clear time, with the existing ordinary kill XP described accurately. The existing five-second return window is unchanged.
- Exact 3–12 point budgets, ten waves, class preparation, combat stats, normal regeneration, original overworld wolves and compact timers remain unchanged.

## Arborium

Six existing walkable buildings contain 21 furnished rooms. Furniture uses the established first-party city palette and oak texture, batched by room/material, with separate solid collision. Existing stairwells, landings and door approaches remain clear. Furniture follows the occupied-floor cutaway and outdoor camera fading. Six warm ground-floor lamps follow the same visibility lifecycle. Every building has an interactable room directory using the existing conversation UI.

| Building | Floors from ground upward |
| --- | --- |
| Adventurers Guild | Reception, common room, training room, planning room |
| Northern Exchange | Exchange reception, records office, counting room, reserve archive |
| Artisan Workshop | Workshop, assembly room, storeroom |
| Mage Observatory | Study, library, experiment room, observatory |
| West and East Residences | Sitting room, dining and study, bedroom |

The original exchange banker now stands behind the indoor reception counter. The existing bank identity, shared inventory, transactions and access rules are reused. The guild training room contains an attackable, invulnerable practice actor with zero XP and no defeat/drop reporter or autonomous attack. It does not grant quest or completion rewards. The existing registrar and class mentors retain their roles and locations.

The first traversal pass caught crates across a narrow artisan stair exit. The storage layout was reduced to the front side bays; the rear workshop rack and library shelf were turned against the wall. `RepairLandingClearance` is bounded to these newly authored furnishing meshes/colliders and can safely finish a partially applied repair.

## Authoring and asset provenance

`tools/authoring/WolfPresentationAuthoring.cs` and `ArboriumRoomsAuthoring.cs` preserve recovery scenes/NavMeshes before their one-time authoring. New textures and furniture are original Unity-authored art. The existing local asset catalog was checked; no additional vendor package or paid-provider asset was imported. Existing licensed wolf visuals retain their portable fallback.

Navigation assets are rebaked in place, retaining GUIDs. All 41,050 original Cornberg object IDs and 1,054 original wolf-scene IDs are retained. All 54 pre-existing modified files and seven protected settings remain byte-identical (see `Logs/WorldPolishPreservation.json`). Earlier recoveries, vendor packages and unrelated WIP are preserved.

## Verification

The focused Editor acceptance walks all 21 floors by actual stairs, checks occupied-floor picking and exterior restoration, opens six directories, deposits/withdraws an item through the indoor bank, and attacks the guild practice target. Wolf presentation acceptance checks all 12 possible entry slots, walks all three forest entrances, checks banners and frozen victory time, and captures the result from the gameplay camera.

`tools/validation/WindowsWorldPolishSmoke.cs` extends the existing wolf standalone acceptance with these city-room/service checks. Combat lifecycle fixtures use explicit defeat packets to clear waves; separately observed natural bites, flanking and alpha specials establish that existing combat still runs. These checks do not establish a naturally completed, balanced full run for every class/loadout.

Editor city acceptance passed 358 checks, 39 actual movement orders and 888.3 m of traversal, reaching all 21 floors. It opened all six directories, deposited and withdrew an item through the indoor bank and attacked the invulnerable practice target, with zero Unity errors. Wolf presentation acceptance passed 1,014 checks, including all 12 entry slots, 166.1 m of actual trail movement, treeline spawning, announcements and a frozen victory duration. Gameplay captures are under `VisualBaselines/WorldPolish`.

The Editor trail-walking fixture holds only that test run's preparation deadline at 180 seconds. Authored preparation remains 30 seconds, also checked in the standalone acceptance and shown in its staging capture.

The gameplay view also exposed a distant corner beyond the old decorative ground disk. The visual-only backdrop was extended to 108.8 m and its shared texture clamped at the outer grass border; playable navigation remains unchanged.

Final Windows build: `Builds/BriarbackMilestone-20261106-000831/DiceFree.exe`, eight scenes, 470,002,873 bytes, **zero build errors and warnings**. The isolated-profile standalone acceptance passed ten waves / 75 points / four final alphas (38 wolves for this seed), natural bites/flanking/alpha special, ordinary XP once, staging/active equipment rules, wipe/reset, active-menu exit, saved resume, re-entry, lighting restoration and existing Slime Dungeon travel. It then walked all 21 furnished floors, checked picking and six directories, deposited/withdrew at the indoor bank and attacked the guild target without gaining XP. There were zero observed Unity errors. Normal menu Quit exited PID 88880 with code 0.

Final standalone captures are `windows-*.png` under `VisualBaselines/WorldPolish`; `validation.json` consolidates the final build, Editor and standalone results. The build includes the existing unrelated workspace WIP, preserved unchanged. This work is local on `feature/wolf-visuals-and-arborium-interiors`; no push or merge is included.

Architecture, managed-reference compatibility, assembly ownership and dependency policy passed (`DICEFRE_ARCHITECTURE_OK`, 11 assemblies). The CLI request exceeded its 160-second timeout while scanning historical recovery scenes; the Editor continued and logged the successful result before the build began. The audit was not rerun or treated as passed merely because its request timed out.

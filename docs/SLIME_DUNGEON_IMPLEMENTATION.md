# Slime Dungeon implementation / playtest notes

Entry: open the existing **Cornberg** scene and interact with the signed Slime Dungeon entrance (near the northeast road, authored NavMesh-snapped position). Right-click or the existing interaction binding enters the separate map. The first entrant starts the real 60-second shared staging countdown; the practice target is attackable and grants nothing. Gear/class changes lock only when the run starts. Existing movement, basic attacks and class abilities remain the player controls. The dungeon HUD shows staging/objectives/clear countdown and voluntary abandonment.

Separate authored maps:
- `Assets/_DiceFree/Scenes/SlimeDungeon.unity`
- `Assets/_DiceFree/Scenes/DungeonRewardRoom.unity`

`SlimeDungeonAuthoring` authors only these generated maps and their navigation. Cornberg installation is incremental and idempotent: it adds a portal root, existing twisted-tree art, collider and sign, and never regenerates/rebakes/deletes Cornberg. The Cornberg diff should contain additions only. Dungeon gates use live carving; forest boundaries use the existing diverse Blender tree models. Only the southern puzzle boundary is permanently blue. The central tree becomes blue only on an all-blue solution. Normal boss/Regent use the existing top-hat/crowned Blender models and shared art materials.

Runtime ownership: `SlimeDungeonRun` orchestrates occupancy, participants, lifecycle, map travel, rewards, wipe and semantic events; `DungeonSlime` owns local encounter attacks/thresholds and resets; `SlimeTreePuzzle` owns immutable captures and replenishment; `DungeonRewardRoom` owns independent, one-shot offers. `DungeonRewardPool` has a separate Regent table and optional visual-variant prefab. Regent exclusives are intentionally absent pending design. No item drops occur in combat. Dead/withdrawn participants cannot receive a reward-room offer; dead actors remain in place during the clear countdown and can be restored by the existing Health API. Self-respawn leaves the attempt; a wipe/abandon returns to Cornberg. Reward resolution returns to the actual entrance.

All EXP/gold payouts, HP/damage, fragment HP fraction and movement/attack timings in `Assets/_DiceFree/Settings/Dungeons/Slime playtest tuning.asset` are **provisional test values, not approved balance**. Fragment HP is proportional to boss maximum HP. Only two HP thresholds can spawn Divide, so the uncapped adds cannot accrue through an endless healing/threshold loop. Dead spawned objects are pruned from run/encounter collections; hazards/fragments are cleaned on reset, clear and unload. Adds persist after a successful/failed Divide until killed or the encounter/run ends. Encounter reset restores boss HP, phases, hazards and adds when the party leaves the arena.

Stats-only item schema additions support Tophat flat regeneration and Orb upper-end basic-attack damage; neither affects spell damage. Orb/Shield eligibility uses the existing Magically Touched/Physically Blessed Novice class IDs. No smart-loot filtering, procs or unapproved exclusives are added.

Unity CLI commands (target this checkout explicitly):

```powershell
unity command set_autotick --enable true --project-path T:/TEMP/DiceFree-Options
unity command recompile --project-path T:/TEMP/DiceFree-Options
unity command recompile_status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.author --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.validate --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.status --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --regent true --project-path T:/TEMP/DiceFree-Options
unity command dicefree.slime.playtest --wipe true --project-path T:/TEMP/DiceFree-Options
```

**Validation status (2026-10-08):** Unity compile/recompile status passed with zero reported compilation errors. `dicefree.slime.validate` passed 20,000 independent simulated reward rolls (1,025 rare Tophats; 4,752 two-item non-rare offers). The isolated-save `dicefree.slime.playtest` **normal-boss run passed** (phase 14); `dicefree.slime.playtest --regent true` **secret-Regent run passed** (phase 14); and `dicefree.slime.playtest --wipe true` **wipe/respawn run passed** (phase 20). These are scripted in-Editor integration tests rather than a multiplayer or manual full-length balancing playthrough.

Authoring requires stopped Edit Mode and saved scenes. Architecture validation also requires Edit Mode. Playtests use fresh temporary save roots, run the full staging countdown, and automatically stop Play Mode. They use existing basic attacks and a Novice skill against the dummy, and ordinary aggro/click-to-move navigation for the first ring capture, then scripted damage/relocation to exercise lifecycle and encounter rules; these are automated integration checks, **not a claim of a manually played 8–12 minute blind clear**. Tuning needs a human combat/balance pass.

Current limits: the existing class kits have no healer resurrection cast yet; this slice preserves dead actors/eligibility and the 15-second opportunity, but does not invent a Novice resurrection ability. The repository has local party membership and host-session occupancy logic, but no network transport/host migration/reconnect adapter. Independent offers exist in the shared neutral reward scene, but network-private visibility and timed reconnect recovery are not implemented. Active dungeon state is not saved; already-earned XP/gold/items use existing durable progression/inventory events. An unresolved reward is session-only; there is no invented expiry payout policy. A standalone launch directly into SlimeDungeon does not create a replacement player/session: enter through Cornberg.

Verification on Unity 6000.6.3f1: connected-Editor compilation and architecture/managed-reference validation passed; 20,000 reward rolls passed distribution checks (1,025 rare overrides, 4,752 two-item normal offers). Isolated-save normal and Regent lifecycle tests passed real staging, existing attacks/Novice skill, gear/class locks, Divide HP rules, ordinary aggro/navigation capture, all-blue route, Regent snapshot attacks, item/bonus reward choice and entrance return. Full-wipe test passed Cornberg respawn, retained XP/gold, participation cleanup, and Codex death/wipe events. These checks do not validate networking or final balance.

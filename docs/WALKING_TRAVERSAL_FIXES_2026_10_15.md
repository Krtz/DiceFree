# Walking, cube crossing and Spin Dash fixes

User request: repair walking animations, make walking onto the World 1/World 2 edge cross correctly, and let Greatblade Spin Dash pass enemies while respecting scenery.

## Changes

The shared Novice FBX's Idle/Locomotion takes were imported without looping. Novice, Physically Blessed and Magically Touched all use that locomotion. Their late walk samples continued translating after the clip had finished. The importer now explicitly loops those takes; the original art authoring command preserves that setting. Existing portable clips already had the requested loop settings.

The shared presentation driver resolves each active controller's actual Idle/Walk state names, including Greatblade's named states. It recovers stale movement states after interrupted actions, scales only walking cadence from actual speed, and preserves committed combat/cast timing. Greatblade portrait initialization also uses its real Idle state instead of issuing a missing-state warning.

Cornberg's four wandering NPCs had static unrigged meshes with whole-body sway. Three derived original-art models now add weighted legs/arms and in-place looping Idle/Walk clips. Two farmers, one villager and one herbalist use them. A displacement-driven animation component follows their existing wandering. Original NPC roots, colliders, navigation, identity and interactions remain; stationary villagers keep their existing presentations. Original source assets are untouched. See `SourceArt/World/WalkingVillagers/README.md` and `provenance.json`.

The crossing pad's intended destination was 21m from the face center, while automatic crossing required 22.2m. Reaching the pad therefore stopped short. Crossing now begins at 20.5m within the gate; arrival remains disarmed until the player moves inward beyond 19.5m. Both directions are covered. The existing vista-only test stages outside the new activation zone.

Spin Dash acquires a named enemy-passage lease and temporarily disables its native unit avoidance. Direct movement and symmetric actor separation honor that lease for opposing factions. Static props/walls, friendly bodies and NavMesh limits still block. Finish/cancel/disable removes only this ability's lease and restores prior avoidance; other owners' effects survive. Damage stays once per enemy along the actual route. The skill tooltip and old blocker assertion now describe/verify enemy passage. The older finisher fixture places the actor explicitly within melee reach rather than depending on displacement from the previous blocked dash.

## Evidence

- `docs/validation/Walking-Baseline-2026-10-15.json`: original class/controller/clip measurements; first three forms reported non-looping locomotion.
- `docs/validation/Walking-Traversal-2026-10-15.json`: 76 focused runtime assertions passed using an isolated development save. Sustained poses, click/direct movement, stop/recovery for all four forms; six-metre dash through two enemies with exact hit counts; wall/friendly blocking; invalid-ground rejection; cancellation/other-owner cleanup; automatic two-way crossing without bounce or actor replacement; all four NPCs visibly articulate their leg bones during actual navigation and settle to Idle.
- `docs/validation/Walking-CubeRegression-2026-10-15.json`: 95 existing transition assertions passed, including state conservation, controls, animated quarter-turns and failure recovery.
- `docs/screenshots/WalkingVillagers-Poses.png`: actual Unity-rendered original appearances at Idle and opposite walking phases, inspected for missing/pink materials and visible deformation.

These are the two development test faces. This does not introduce complete production World 2 content or multiplayer class/skill replication. Two-machine laptop testing remains a separate follow-up.

Additional regressions passed:

- Greatblade playable combat/progression: 75 assertions (`Walking-GreatbladeRegression-2026-10-15.json`).
- Original development-room/menu/state-preserving World 2 round trip (`Walking-DevelopmentRegression-2026-10-15.json`).
- Ten data/architecture/managed-reference/XP/skill/progression validators (`Walking-StaticRegressions-2026-10-15.json`).
- Existing three wildlife rigs: 612 changed skinned vertices in motion validation (`Walking-WildlifePose-2026-10-15.json`). Their walk/run clips were already looping and were retained.
- New village pose validation: four walkers, 3,528 changed clothing vertices (`Walking-VillagerPose-2026-10-15.json`).
- Seven-scene Windows Development build: Succeeded, 0 errors, 0 warnings, 353,319,456 reported bytes (`Walking-WindowsBuild-2026-10-15.json`). Executable: `T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe`. Use the entire build folder.

The old Greatblade regression initially failed its deliberately superseded enemy-blocking assertion, then its finisher setup relied on the old blocker's displacement. Both fixture assumptions were updated explicitly; no production finisher mechanics changed. An initial automatic-only multiplayer return attempt remained disarmed because its old combat route stayed along the arrival edge. The harness now explicitly walks inward before testing the return pad, as required by the no-bounce gate.

The automatic-only harness also replaces the old exact pad-coordinate waits with a check that accepts reaching the pad OR observing the authoritative transition. Snapshots can skip the final half-metre as automatic crossing starts; requiring the old final position could report a timeout after the guest had already returned successfully. The final two-way face/flip/identity assertions remain mandatory.

Final rebuilt-player multiplayer regression: 22/22 checks passed in separate Windows loopback processes. Walking alone triggered both crossings; stepping inward rearmed the return; host movement continued during the guest flip; identity survived the round trip. Simultaneous combat, late join, four players, overflow rejection, vulnerable reconnect grace, same-actor reconnect and expiry/fresh join all passed. All five player logs passed the existing runtime/navigation error checks. See `docs/validation/Walking-NetworkStandalone-2026-10-15.json` and `tools/validation/Invoke-NetworkWorldLab.ps1`. These remain local-process tests, not two-machine LAN or internet validation.
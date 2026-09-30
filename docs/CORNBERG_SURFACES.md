# Cornberg road/surface movement speed

This slice adds the settled player-facing road bonus to the existing east/forest
road. It does not implement mounts or Q4. The provisional Cornberg bonus is
**+15% movement speed**, authored in `Settings/Travel/Cornberg dirt road.asset`.

## Real-world coverage and authoring

`TravelSurfaceDefinition` contains a stable semantic surface ID, display label,
bonus in percent and selection priority. `TravelSurface` references the existing
visible road MeshFilter. It tests triangle coverage in mesh-local X/Z and the
interpolated surface height, with a 0.75-unit standing-height tolerance. Mesh
vertices/indices are cached; transform changes are reflected in the local query.
No collider, trigger callback, duplicate road geometry or navigation bake is needed.

The original Mountain to east road mesh is clipped by an authored local region:
center `(72,0,25)`, size `(94,10,70)`. Coverage begins at X=25 and follows the
visible winding road into the forest, ending with the existing mesh/blockout.
Mountain approach, village green and woodland side loops do not receive a bonus
in this small proof. Road direction is never examined.

- Surface definition: `surface.cornberg.dirt-road`.
- Surface area: `surface-area.cornberg.east-road`.
- Motor contribution: `travel.surface`.

Disable a TravelSurface component/object to disable its coverage. Overlapping
surfaces select **one** winner: highest priority, then ordinal stable area ID.
Stable area IDs must be unique. This is a provisional PoC authoring convention,
not a settled general terrain/effect stacking rule. Overlaps do not accumulate
bonuses. Removing a winner exposes the remaining eligible surface.

The authoring helper `CornbergSurfaceSetup.Install` is additive/idempotent and
operates on the saved scene. Do not use scene recreation helpers. The scene adds
33 lines and removes none, preserving all 8,470 existing serialized IDs, road
geometry, quest destinations, enemy positions and baked navigation.

## Base and effective speed boundary

`ActorStats.MoveSpeed -> CombatActor -> TraversalMotor.SetSpeed` remains the
baseline source. SetSpeed updates **BaseSpeed**, not a cached road-adjusted value.
`TraversalMotor` holds source-keyed resolved speed factors. A source replaces its
own factor; removal only removes that source. The current calculation is:

`effective speed = current base speed × product of resolved source factors`

Road contributes `1 + authored bonus percent / 100`, currently `1.15`.
Multiplication of resolved factors is a small provisional composition boundary;
it does **not** settle future mount/collection/buff stacking math. Future sources
retain their own identity and can resolve their authored rules before contributing,
or extend this calculation boundary when that design is settled. No mount state,
unlock, collection, cast, or gameplay modifier engine exists in this patch.

`SurfaceTravel` is an explicit actor opt-in component. Only the Cornberg player
has it. Enemies retain their original speed and no enemy road policy is invented.
It supplies one named contribution when the motor refreshes contextual speed.
The motor refreshes each update, before movement commands/direct steps and speed
reads, when stats set the base speed, and immediately after teleport or motion
permission changes. It derives coverage from current position rather than keeping
enter/exit counters that could stick after warps or missed trigger exits.

Both control schemes consume that effective motor speed: NavMeshAgent.speed for
Classic paths, and the same Speed value for Direct displacement. TraversalInput,
combat input, actor stats and enemy AI do not contain road checks. Existing path,
collision and NavMesh constraint logic remains unchanged.

Combat does not suppress the contribution. Level/AGI/coefficient changes update
the base and recalculate the road result. Death clears the road contribution via
the existing motion-permission path; Return/load/teleport resample the new location.
Disabling the consumer removes only its own contribution. No travel state is saved
and no persistence code/schema/migration changes are needed.

The existing blockout overlay displays the surface name and bonus while on-road,
so the effect is readable without adding another HUD panel.

## Validation

Run `DiceFree.EditorTools.SurfaceSpeedValidation.Run` in batch Play Mode without
`-quit`. It checks exact off-road/base and on-road values; six enter/exit cycles;
actual walking on/off; real Classic right-click path velocity; real Direct D-key
displacement independent of road direction; combat; level/AGI and coefficient
changes; independent-source preservation; overlaps/priority/stable ties; disabled
surface/consumer; standing-height rejection; teleport, death, Return and load reset;
and explicit player-only eligibility with unchanged enemy speed.

No manual/human playthrough is claimed. Known editor SearchDatabase #17 is separate
from gameplay failures; unrelated errors fail the focused suite.

2026-09-30: focused road tests passed, including walking onto/off the road and
actual mouse/WASD input. All 13 traversal routes and both controls passed without
changing the existing traversal assertions. Combat/death/Return/well, trivial
aggro, Q1, Q2 (six reload checkpoints), Q3 (four) and Runner Returns (four) passed.
The Windows development build succeeded: 171,996,594 bytes. Durable persistence
code is untouched; the quest suites retain their old-profile/unknown-record and
reload coverage. No save-schema migration was introduced.
Two isolated standalone startup/reload smokes also passed with zero runtime errors;
identity and completed Runner state persisted, and save revision advanced 9 -> 10.
The full storage/migration/recovery suite was not separately rerun because durable
code was untouched. These were automated checks, not a human playthrough.

## Limits and next step

Mesh coverage assumes readable static terrain meshes and local +Y standing height.
The existing road asset is readable. Deforming meshes, arbitrary cube gravity,
streaming and optimized broad-phase queries are future needs, not implemented here.
The current small road query caches mesh arrays and allocates no new mesh per frame.

Trivial aggro remains complete. Issue #35 still includes mount ownership/access,
mounting restrictions, mounted movement and mount-speed progression. Q4 remains
blocked on alternatives/progress/rewards/XP design. Next smallest step: playtest
the provisional road bonus at road edges/in combat; then scope a separate mount
slice only after its composition/tuning requirements are explicit. Do not silently
start mounts or Q4 from this patch.

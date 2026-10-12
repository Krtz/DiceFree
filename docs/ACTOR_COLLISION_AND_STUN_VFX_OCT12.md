# Larger circular unit footprints and blue stun spirals — October 12, 2026

Status: Implemented on feature/visual-overhaul-vis00. No XP, dungeon scenes, or village geometry altered.

## Physical collision and pathing

- **All living CombatActors** (player and enemies) have a single authoritative circular radius, consistently applied to `CombatActor.Radius`, `NavMeshAgent.radius` and the physical root `CapsuleCollider.radius` on startup **and on subsequent `Configure` calls**. This fixes an actual script-order bug with Ironpine wolves reassigning their radius after other scripts started.
- Normal player base 0.45m becomes approximately **0.675m**; ordinary wolves 0.52m become **0.728m**; quillrats 0.35m become **0.54m**; Alpha 0.75m becomes **1.05m**. Normal-sized enemies are 40% wider in radius, with a 0.54m minimum; player uses 50%. These are **radii** rather than diameters.
- Already enormous dungeon bosses (source radius greater than 1.4m) increase at most 0.30m, to avoid making existing dungeon corridors/arena navigation impassable.
- NavMeshAgents use high-quality local obstacle avoidance for moving units, with priority distinction between player and enemies. The small `ActorFootprintSeparation` script corrects residual horizontal overlaps when a unit itself moves, without physically pushing other actors. It respects different height/floors, active/alive state, and uses the NavMesh rather than teleporting through world obstacles.
- Direct WASD movement's existing sphere-cast now explicitly knows when a character is **ghosting** and can ignore actor collision for that effect, while still respecting world-solid geometry. `CombatActor.SetGhosting(bool)` is the future spell/condition API; **no ghosting ability was added**. Ordinary combat/quest logic continues to recognize actor radius.
- Dead characters' old collider-disable behavior is preserved.

## Stun visual (one shared effect for all crowd-control sources)

- Applying `CombatStatusController.TryApplyStun` automatically attaches `StunSpiralVisual` to the affected actor, whether player, wolf, slime or another enemy. This uses the **actual resolved stun timer**, including immunity/resistance and repeated stun extensions.
- The effect is **two animated blue circular/spiralling helix strokes and three orbiting glow sprites**, above the character model. Its positioning scales with the model's visible height/capsule and with its combat radius; all effect objects have **disabled colliders**.
- Effect clears immediately on `ClearTransient` (including death/restore/reset), and automatically ends when the stun timer expires. It has no control over the mechanical stun and does not add gameplay hitboxes.
- Source-art-first: optional `StunSoftGlow.png` was extracted from the user's existing **Free Game VFX CollectionURP** pack (`Assets/Eric VFX Studio/Resource/Textures/Soft Circle Pulse.png`), installed locally in `Assets/_DiceFree/Resources/StatusVfxLicensed/`, and **gitignored**. Source material is not published on GitHub. A code-drawn ring/glow substitute works when the original pack art is absent.

## Validation

- `unity recompile`: **0 compilation errors and warnings**.
- Isolated Unity 6000.6.3f1 Play Mode with `-diceFreeSaveRoot`: validated **37 active actors**, authoritative capsule/agent radius consistency, enlarged player/wolf/Alpha/quillrat footprint, and automatically attached overlap controller.
- Test deliberately placed a player on a wolf, invoked the movement separation resolver, and verified the actor was moved out **without pushing the wolf**; ghosting mode opt-in/out checked.
- Stun applied to a real wolf, created blue overhead VFX, confirmed its child objects have no active colliders, cleared status to hide VFX, reapplied stun and cleared again. Marker: `DICEFREE_UNIT_COLLISION_AND_STUN_OK`.
- Source for reproducible smoke test: `Assets/_DiceFree/Scripts/Tools/Editor/StunFootprintPlaymodeProbe.cs`. This test refuses to start without `-diceFreeSaveRoot`, protecting user profiles.
- A separate Unity `SearchDatabase` headless editor indexing exception can appear during startup, unrelated to successful scene Play Mode validation.
- **Still requires manual visual and pathing playtest**, particularly narrow Cornberg footbridges, slime dungeon boss arenas, Alpha body overlap, minimum HUD zoom, and whether the blue head swirl is high enough on every model. Do not claim this test proves every possible collision scenario. Static world NavMesh has not been rebaked.

## XP scope

No XP curve was changed. User thinks `30+(L-1)^2` is still too easy; alternative `30+2(L-1)^2` and `30+3(L-1)^2` are documented in `docs/QUADRATIC_XP_PROPOSAL.md` for approval, including Cornberg pacing/save migration considerations.

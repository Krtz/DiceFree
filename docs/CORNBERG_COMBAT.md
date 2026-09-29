# Cornberg combat increment

The following combat foundation now also supports timed respawn, XP/leveling
and Q1. See [Cornberg quest/progression notes](CORNBERG_QUESTS.md) for the current
five-kill starter flow, added components, provisional values and validation.
The [Q2 road slice](CORNBERG_ROAD.md) adds a stronger authored Slime variant and
investigation objective through the same combat/credit/respawn pipeline.
The subsequent [local save slice](CORNBERG_SAVES.md) adds progression/quest/anchor
persistence and records the newer design-document sync through `cb3afd1`.

This extends the saved Phase 1 Cornberg pocket on `poc/cornberg`. The latest six
design-document changes from `setup/unity-project` at `a5879e9` were copied into
this branch in `f454373` before implementation. A second documentation-only
sync (`819c60d`) incorporated updates through `e2e5de2` that arrived during work,
including class convergence, one save per class, and per-class resurrection
points. Neither branch was merged into main. The scene was extended additively,
not regenerated; its existing 8,363
serialized objects/components and baked navigation asset were preserved.

## Play the loop

Open `Assets/_DiceFree/Scenes/Cornberg.unity` in Unity 6000.6.3f1, or build/run
`Builds/Cornberg/DiceFree.exe` with its accompanying runtime files.

1. Walk from the emergence path through Cornberg to the eastern crop field.
   The one small green Slime starts at approximately `(47, 0, -10)`.
2. Left-click the enemy or press Tab to select it; Shift+Tab cycles backward.
   Right-click the enemy or press X to order repeating fist attacks. The hero
   approaches until in range, winds up, hits, then recovers before repeating.
3. Right-click open ground in Classic mode to cancel the attack and move.
   In Direct mode, WASD cancels the attack and moves through the same motor.
   F6 switches modes. Space cancels movement/attack; Escape also clears target.
   Movement keeps the selected target. Dead/out-of-scope targets are cleared.
4. Let the Slime attack without fighting back to test death. After a short
   delay, press R or use **Return to Cornberg** to return beside the stone d6.
   Death disables movement/attacks; ordinary healing cannot revive a dead actor.
   If the camera was panned away, Home recenters it on the hero.
5. Move close to the magical well, west of the stone. Living actors heal over
   time, independently of the resurrection point. HP regeneration remains active
   during combat too.
6. The Slime chases within a short home leash and returns/restores HP when the
   player escapes. A defeated Slime now respawns after a configurable timer
   (currently 10 seconds). The **DEBUG: reset the single crop Slime** button
   remains an optional test control; normal quest play uses timed respawn.

These combat keys are provisional PoC bindings. Full rebinding/profile UI,
Attack-Move, Hold, allied/summon selection and ability casting remain deferred.

## Implemented pieces

- `ActorDefinition`, `ActorStats`, `AttributeValues`: stable content IDs, five
  base attributes, independent per-level growth, HP/defenses/speed/regeneration.
  Novice starts with one of each attribute and gains one of each per level.
  XP/leveling is now provided by separate progression components. No class
  resource mechanic is added.
- `AttackDefinition`: base value, weighted or highest-attribute scaling,
  Physical/Magical channel, optional element, reach, wind-up and interval.
  Novice fists use the highest of **all five** attributes and remain Physical.
- `ElementDefinition`: data asset with stable ID, name and icon hook. Null means
  no element. Both attacks here have no element. Resistance entries are keyed
  by stable element ID; no hard-coded roster or element switch is introduced.
  The full DiceBound roster import and elemental effects are deferred.
- `DamageResolver`: explicit raw construction, defense/resistance mitigation
  and HP application stages. Debug hit records include raw value, defense,
  element, resistance, resolved value and actual HP damage.
- `Health`: HP, regeneration, damage/heal/death/restore events, overkill and
  overheal clamping. Healing-received scaling applies once, including to
  regeneration. No healing resurrection. This pass uses non-elemental
  environmental healing; elemental healing/inversion and source-side healer
  bonuses are not implemented.
- `CombatActor`, `TargetSelection`, `BasicAttack`: actor identity/hostility,
  selection independent of commands, cancelable wind-up, repeating actions,
  range and world-solid line-of-sight checks, shared-motor approach. Cooldown
  survives cancellation/retargeting so commands cannot grant free hits.
- `AggroBehaviour`: generic nearest-hostile acquisition, retaliation, leash,
  home return and reset. Combat state uses active attack/aggro relationships.
  This is intentionally not the full multi-source threat table from COMBAT.md.
- `RespawnAtAnchor`, `HealingArea`: separate return and proximity-healing
  behaviours. Cornberg is the single available anchor. The arrival cinematic,
  additional-anchor selection and death penalties remain deferred. The current
  anchor now persists through the local save slice.
- Separate player, target, command and actor-feedback widgets: current/max HP,
  level, selected target, combat/action state, low-HP color, world labels,
  damage numbers and simple squash/wind-up/death feedback. Nameplate visibility
  has Always / When Hurt / Never settings; floating numbers can be disabled.
  Widgets block world clicks through their panels. The full portrait, party,
  minimap and 12-slot HUD layout remain deferred. Separate basic XP and quest
  widgets are now available in the Q1 increment.

Movement keeps NavMeshAgent avoidance for path movement. Direct movement also
sweeps against actor colliders to prevent walking through live unit footprints.
Static terrain/building/stream constraints still come from the original bake.

## Settled Vitality defaults and provisional duel tuning

The shared `Cornberg provisional tuning` asset and actor/attack assets under
`Assets/_DiceFree/Settings/Combat/` expose balance values:

| Parameter | Current PoC value |
| --- | --- |
| Novice HP | 10 base + 15 per VIT = 25 at level 1 |
| Flat regeneration | 0.1 HP/s per VIT before healing received, also in combat |
| Physical / Magical Defense | base + 1 per STR / INT |
| Defense multiplier | 100 / (100 + nonnegative Defense) |
| AGI contribution | +0.5% attack speed and +0.1% movement speed per point |
| SPI healing received | +1% per point |
| Fists | 1 + 2 × highest attribute; 1.1 s interval; 0.25 s wind-up |
| Slime | 12 HP; 3 raw Physical damage; 1.7 s interval; 0.45 s wind-up |
| Slime movement / awareness / leash | 3.4 units/s / 7 units / 13 units from home |
| Fist / Slime reach | 0.7 / 0.6 units beyond the two body radii |
| Well | 4-unit radius; 5 HP/s before healing-received multiplier |
| Return | after 1.5 s, on player acceptance, at 50% HP |

The VIT defaults above are settled by #11; the former +5 HP / 0.05 HP/s values
are obsolete. Novice has no exception. Each actor definition can independently
override either coefficient. `ActorStats` accepts source-keyed `VitalityModifier`
entries: effective coefficient = max(0, (base + additive deltas) × max(0, 1 +
percentage deltas)). Removing a source removes its contribution. This is a small
stat boundary, not an item/passive/effect framework. Other primary coefficients
remain unchanged. Derived-stat changes notify Health/movement through `Changed`.
Missing HP is preserved where possible; lowering maximum HP alone does not kill
a living actor (minimum 1 HP), and dead actors stay dead. This edge policy remains
provisional with the existing level-up HP policy under #11.

Vitality increment validation: default/override/modifier math, in-combat regen,
Q1 growth, combat/death/return/well and legacy migration/full-HP load passed.
No Crop Slime, fist, well or return tuning changed to compensate for the larger
Novice HP pool. Design docs were copied through `setup/unity-project` `cc674f5`;
economy/controller/cube-world changes are documentation only.

The settled -10% elemental baseline and 75% normal cap are represented in
tuning data. They do not affect these no-element attacks. Final coefficients
belong to [issue #11](https://github.com/Krtz/DiceFree/issues/11); the encounter,
well and return values are tracked for review in
[issue #19](https://github.com/Krtz/DiceFree/issues/19) before broader use.

## Extension boundaries

Combat actors consume stat and attack packages rather than class-name branches.
There is no class ancestry tree or parent-class requirement embedded in this
code. A future class definition can reference this combat package plus separate
resource, skill and advancement data. Resources should be independently
composed, not a universal Mana field or a fixed resource enum on every actor.

Advancement requirements must remain a separate composable condition model over
Echo-wide progress: nested AND/OR predicates and multiple milestones from
**different manifestations** may be required. Resulting class identity, route,
discovery and eligibility must remain distinct. One persistent save per class
and per-class resurrection points now have a first local persistence slice;
advancement and manifestation switching remain future work.
An ancestry/display relationship must not substitute for
eligibility. No single-parent prerequisite, fixed child count, tier width or
eager requirement that all descendants exist is introduced. Mystic Knight and
advancement evaluation are not implemented. See issue #18.

Health events and damage-resolution boundaries are extension points, not claims
that the full effect framework already exists. Shields, Pure Damage, crit,
penetration, CC, lifesteal, reflection, resource costs, saving and network
authority remain future work. Do not hook future proc chains directly into
presentation callbacks.

## Validation

Run with `-batchmode -projectPath <repo> -executeMethod <method> -logFile <path>`:

- `DiceFree.EditorTools.CornbergCombatValidation.Run`: stat/growth/adaptive
  scaling, arbitrary data-defined element, neutral damage, receiver healing
  scaling and overheal clamping, actual Play Mode
  aggro/repeat timing, Classic right-click attack, enemy death, player death,
  blocked dead movement/healing, R return, well restoration, leash/exit from
  combat, actor collision, Tab/X, WASD cancellation and Direct-mode duel.
- `DiceFree.EditorTools.CornbergValidation.RunAll`: the original navigation and
  actual traversal/input suite. The Slime is disabled only inside this isolated
  traversal pass; the combat suite tests with the encounter active.

Omit `-quit` for these two runners; each exits with success/failure itself.
The existing narrowly identified Unity SearchDatabase batch-startup exception
is tracked in issue #17; gameplay errors still fail validation.

Verified on 2026-09-28: the focused combat suite (including centralized receiver
healing scaling) and original 13-route traversal suite passed. The Windows
development build and a standalone startup smoke test also passed. Startup
smoke is not a full automated standalone duel; combat/input checks run in real
Editor Play Mode. The crop-field render is under ignored
`Logs/Cornberg/crop-combat.png` and excludes the IMGUI overlay.

Build using `DiceFree.EditorTools.CornbergPlayerBuild.Build` with `-quit`, or the
existing Windows playtest menu. Do **not** run Recreate/RecreateAndBuild or the
old BuildAndRun generator to test this scene: those replace the blockout with
its Phase 1 recipe. `CornbergCombatSetup.Install` is an additive editor migration
for the saved scene and leaves existing assets' tuned values intact.

## Remaining risks and smallest next step

The single-agent duel has no crowd-load, network or cube-edge validation.
Persistence validation is described in the save note. Direct collision uses local +Y sweeps; larger bodies and moving
crowds need further navigation work. The runtime actor registry and IMGUI are
small local-PoC implementations, not final large-session or shipping UI systems.
The capsule Echo, ellipsoid Slime, squash feedback, and all environment art
remain placeholders. There are no fist rigs, audio or loot drops.

Timed respawn, semantic kill credit, XP and Q1 are now implemented; see the
quest/progression notes for the next small step. Duel tuning remains under
Axel's separate playtest review; this extension does not finalize issue #19.

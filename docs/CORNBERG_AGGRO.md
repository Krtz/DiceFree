# Cornberg trivial-enemy auto-aggro

Ordinary enemies now skip proactive awareness acquisition when a candidate is
**10 or more levels above the enemy**. Ten is provisional tuning, not a permanent
rule. Gaps below ten retain normal acquisition. Actual ActorStats levels are read
on each scan; no display names, class names or player scaling are involved.

## Policy and authoring

`AI/AutoAggroPolicy.cs` owns the level eligibility calculation. Its serialized
`trivialLevelGap` defaults to 10. `alwaysAutoAggro` explicitly bypasses only this
level suppression, and can also represent content where suppression is disabled.
It never bypasses hostility, alive state, LOS, awareness or leash checks.

`AggroBehaviour` has a serialized policy for standalone enemy actors (including
the existing Crop Slime). `EnemyArchetype` provides shared defaults; a variant may
set `overrideAutoAggro` and provide its own policy. `EnemyVariant` installs the
selected policy before actor initialization. Road and Named Forest Slimes inherit
the ordinary default; being named does not silently grant an exception.

The policy is a small serializable value object, held with `SerializeReference`.
Its virtual `Allows(enemy, candidate)` is the extension seam for a later authored
scaled/proportional policy. A future region/difficulty/encounter authoring layer
can select an appropriate policy through `ConfigureAutoAggro`; no precedence or
stacking system for those future layers is implemented here. Existing policies
are treated as authored data, not mutated as combat state. No generic rule engine,
global difficulty service or region framework was added.

## Proactive acquisition only

The policy is checked only while the enemy has no target and scans awareness.
An existing target is not discarded when levels change. `OnDamaged` retaliation
does not consult this policy. A high-level actor can still target, attack, damage
and kill an enemy; ordinary defeat, XP and quest consumers remain unchanged.
Existing hostility, LOS, home/leash, BasicAttack and respawn behavior is preserved.

The present scan considers hostile CombatActor candidates; today the relevant
candidate is the player. The seam uses actor levels rather than binding to an
input component or class. Future target-role distinctions belong in an explicitly
authored policy rather than assumed player/companion rules.

Reset and respawn clear combat state but retain authored policy selection. The
policy is not saved in the player profile. No save-schema or migration change,
scene/nav edit, enemy level/stat change, quest reward retune or combat-math change.

## Validation

Run `DiceFree.EditorTools.TrivialAggroValidation.Run` in batch Play Mode, without
`-quit`. It isolates the existing Crop actor and exercises the real AI loop:

- gaps 0, 9, exactly 10 and 20;
- lowering player level permits acquisition without recreating the enemy;
- raising player level never cancels legitimate engagement;
- damage retaliation and explicit attack/kill/XP at the suppressed threshold;
- authored always-aggro and a separately configured three-level threshold;
- hostility, awareness and a physical LOS blocker still reject candidates;
- leash/return-home restores state;
- timed respawn retains default suppression and fixed enemy level;
- debug reset retains an explicit override;
- archetype inheritance and explicit variant policy selection.

No human/manual playthrough is claimed. Exact known Unity editor SearchDatabase
#17 remains separate from gameplay failures.

2026-09-30 results: focused real-AI suite passed; combat/death/Return/well and
existing combat-math checks passed; Q1, Q2 (six reload checkpoints), Q3 (four) and
Runner Returns (four) passed; all 13 traversal routes/input checks passed.
Fresh/save-reload plus storage recovery/backups, stale writers, migration and
Profile Inspector checks passed. Windows development build succeeded:
171,990,466 bytes. Scene/nav, save schema and authored balance assets are unchanged.
Two isolated standalone startup/reload smokes passed with no runtime errors,
retaining Echo identity, completed runner state and level 6/0 XP while save
revisions advanced from 9 to 10.

## Scope and next step

Q1 through Runner Returns remain the implemented story. Q4 still needs completion
alternatives, kill-count/progress semantics, reward parity and XP/level payout.
No Q4, mounts, collection, road-speed surfaces, map, achievements, elite/dungeon,
multiplayer or cube traversal implementation is included. Next: playtest traversal
at the provisional boundary and settle Q4 before further story implementation.

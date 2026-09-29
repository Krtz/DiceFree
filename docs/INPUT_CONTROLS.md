# Input and Controls

## Philosophy

DiceFree supports two first-class movement/control styles:

1. **Classic Mouse** — Warcraft III / ARPG-style click-to-move
2. **Direct Movement** — WASD/direct character movement

Both use the same underlying movement/combat systems.

Neither control scheme should feel like an afterthought.

## Default keybind profiles

DiceFree should ship with **different default keybind layouts for Classic Mouse and Direct/WASD**, because reserving WASD for movement fundamentally changes comfortable ability-key placement.

All controls remain fully rebindable.

Switching control style may offer to switch to the matching default keybind profile, but custom bindings should not be silently overwritten.

### Classic Mouse profile

Current direction:
- right-click contextual move/attack/interact;
- left-click selection/UI;
- dedicated Attack-Move;
- Stop;
- Hold Position;
- ability bindings can freely use keys near WASD because WASD is not reserved for movement.

Candidate ability-key arrangement remains open, with layouts such as:
- `1 2 3 4`
- `Q W E R`
- `A S D F`

matching the 3x4 command-grid concept.

### Direct/WASD profile

WASD is reserved for movement.

Ability/utility defaults must therefore shift elsewhere.

Exact default bindings remain open and should be playtested rather than forced prematurely.

## Contextual right-click

Classic Mouse uses Warcraft III-style contextual right-click.

Right-click can:
- move to terrain;
- attack an enemy;
- interact with an NPC/object;
- collect/activate eligible world objects;
- issue context actions to controlled summons/units where relevant.

The contextual cursor communicates the intended action before click.

## Hero focus and selection

The player's hero remains the **primary controlled/selected unit by default**, but controllable summons can still be directly left-click selected.

Selection behavior is highly configurable.

Important distinction:
- **targeting** a unit for attacks/heals/support does not have to change command focus;
- **selecting** a controllable summon/unit changes the active command unit when the player's settings allow it.

The default should protect combat responsiveness while still preserving Warcraft III-style direct summon control.

## Selection safety

DiceFree mixes ARPG immediacy with some Warcraft III-style unit selection/control.

Combat must be protected against accidental selection mistakes.

Design requirement:
- clicking nearby allies/summons/enemies should not make emergency movement unresponsive;
- players need options controlling what world clicks can select;
- selection rules should be configurable enough to support both an ARPG-like "my hero remains primary" experience and a more RTS-like unit-control style.

Potential settings/framework hooks:
- lock primary hero selection during combat;
- require modifier key to select allied units/summons;
- disable click-selection of allies while hostile combat is active;
- selection filters by unit category;
- click-through options for selected categories;
- dedicated controlled-unit selection groups/hotkeys.

Exact UX remains open, but **accidental unit selection must never trap the player in a danger telegraph**.

## Attack-Move

A dedicated Attack-Move command is required.

Default behavior:
- activate Attack-Move command/hotkey;
- click destination;
- character/selected controllable unit moves toward it and attacks eligible hostile targets encountered according to its targeting rules.

Exact acquisition priority and leash rules remain to be designed.

## Stop and Hold Position

Dedicated commands:
- **Stop** immediately cancels current move/attack/cast command where cancellable;
- **Hold Position** prevents movement while still allowing appropriate actions/attacks according to class/unit rules.

Suggested classic defaults:
- Stop: `S`
- Hold: `H`

These can be rebound.

## Basic Attack

Basic attacks:
- automatically begin when context-right-clicking an enemy;
- also have a dedicated Attack command/button, Warcraft III-style.

This allows explicit command use even when context-click behavior is undesirable.

## Ability casting modes

Every targeted/ground-targeted ability can use one of three player-selected casting modes unless the ability explicitly requires a particular presentation.

### 1. Normal / confirm cast

Warcraft III-style:
1. press ability hotkey;
2. targeting cursor/area/range preview appears;
3. left-click confirms the target/location;
4. right-click/Escape/cancel input cancels.

### 2. Semi quick cast

Hold-and-release mode:
1. press and hold ability hotkey;
2. targeting cursor/area/range preview appears while held;
3. releasing the hotkey casts at the current target/location;
4. cancel behavior must be supported.

### 3. Quick cast

Immediate mode:
- pressing the ability hotkey immediately casts toward/at the current cursor/target according to the skill's targeting rules.

Casting mode should be configurable:
- globally;
- ideally per ability;
- and/or per keybind/profile if useful later.

The system should not hard-code one cast style for all players.

## Targeting previews and range

Targeted/ground abilities should display their relevant targeting information before confirmation in Normal/Semi Quick modes:
- circle;
- cone;
- line;
- placement area;
- range;
- invalid target/location feedback.

If the selected target/location is out of cast range, the default behavior is:
- issue movement toward a valid cast position;
- once in range and still valid, execute the queued cast.

Individual abilities may explicitly override this behavior.

## Self-cast

Self-cast is supported for eligible abilities.

Default direction:
- modifier + ability, e.g. Alt + ability.

Self-cast method is configurable.

Options can include:
- modifier + ability;
- double-press/double-tap ability;
- other rebound input.

## Interaction key

A universal keyboard interact action is required in addition to contextual right-click.

The exact default key is **not yet fixed** because Direct/WASD bindings may need keys such as E/Q for abilities or movement-adjacent actions.

The interaction action itself is fully rebindable.

## Target cycling

Default targeting supports:
- `Tab` to cycle hostile targets;
- `Shift+Tab` to cycle backward.

Separate configurable bindings can cycle allies/friendly units where useful.

Default hostile cycling priority should favor:
1. enemies currently relevant/near the hero;
2. enemies actively engaged with the party;
3. elites/bosses over distant trivial targets;
4. then other valid nearby hostiles.

Exact weights/range remain tunable.

## Target persistence and retargeting

Default targeting behavior:
- selected target **persists** when clicking empty terrain to move;
- target is cleared only when it becomes invalid/dies/out-of-scope or the player deliberately changes/clears it.

This behavior is configurable.

### Auto-retargeting after kills

Default:
- Attack-Move may automatically acquire another nearby valid enemy after a kill;
- explicit single-target attack commands do **not** automatically chain to a new enemy.

This behavior is configurable.

## Camera

Camera control should feel Warcraft III-like rather than locked directly to the hero.

Players can:
- pan the camera around independently of the hero;
- inspect other areas/party activity;
- monitor/control summons or other controllable units away from the hero;
- return camera focus to the hero with a dedicated hotkey/action;
- optionally enable hero-follow behavior.

The camera is therefore **not permanently hero-centered**.

Camera movement must still respect the isometric readability goals and future cube-face/local-gravity transitions.

Camera control framework supports:
- screen-edge scrolling;
- middle-mouse drag/pan;
- keyboard pan;
- zoom;
- rotation;
- hero recenter;
- persistent hero-follow toggle.

All of these are configurable.

Edge scrolling can be disabled.

Hero camera behavior should support both:
- **recenter/snap to hero**;
- **toggle persistent follow**.

Exact default keys remain open.

## Controlled units and summons

The input architecture must anticipate classes with controllable summons/units.

Requirements:
- selection and command system cannot assume only the hero ever receives orders;
- controlled units can potentially be sent somewhere while the camera follows them;
- hero selection/follow can be restored quickly;
- ARPG-oriented players can simplify/limit selection behavior through options.

### Direct summon selection

Controllable summons/units can be directly left-click selected when that behavior is enabled.

### Function-key unit selection

Default direct-unit selection uses function keys:
- **F1** = hero;
- **F2** = summon/unit 1;
- **F3** = summon/unit 2;
- and so on as supported.

Exact maximum count and remapping are configurable.

Traditional RTS control groups can still be supported later if useful, but function-key access is the primary intended default for hero/summon selection.

### Selected-unit action bar

The lower-right action/command grid is **context-sensitive to the currently selected controllable unit**.

When the hero is selected:
- show hero/class abilities and commands.

When a controllable summon/unit is selected:
- show that unit's abilities/commands instead.

This is Warcraft III-style unit control rather than a permanently hero-only action bar.

### Autonomous summons

Not every summon must be a directly commanded peer unit.

Framework also supports autonomous/minion-style summons that act primarily through AI.

Possible stances:
- Aggressive;
- Defensive;
- Passive;
- Hold.

A class can intentionally use these more independent summons without requiring direct micromanagement.

### Drag selection

Box/drag selection of multiple controllable summons/units is supported by the framework.

This behavior should be configurable:
- always enabled;
- disabled;
- potentially limited by control profile/unit category.

Most classes will not need it, but summoner-heavy classes should not require a bespoke input system later.

Exact formation/multi-command behavior remains future design work.

## Unit collision, pathing and ghosting

Heroes, enemies and ordinary controllable units **occupy physical gameplay space**.

Default:
- units have collision/pathing footprints;
- characters do not freely walk through one another;
- movement/pathfinding must route around occupied space where possible.

Allied selection/click-through options do **not** imply physical ghosting.

### Ghosting

Ghosting/phasing through units is a rare explicit mechanic.

Examples:
- a Wraith enemy;
- a class passive;
- a temporary spectral state;
- a special movement ability.

Framework should support it as a flag/effect rather than making all units non-solid.

## Open questions

- exact Classic Mouse default ability hotkeys;
- exact Direct/WASD default ability hotkeys;
- default interaction key for each profile;
- exact default casting mode;
- exact per-ability casting-mode configuration UX;
- camera pan/rotate/zoom bindings;
- hero-follow/recenter defaults;
- selection-safety defaults;
- exact F-key summon capacity/mapping;
- hostile/friendly target-cycle priority;
- controller scheme.


## Controller support

Controller/gamepad input is a supported target.

However:
- **Classic mouse click-to-move remains the primary/reference control scheme** for encounter and interaction design;
- Direct/WASD remains a first-class keyboard alternative;
- controller support must map onto the same movement/combat/action systems rather than becoming a separate simplified game mode.

Controller-specific targeting, command-grid navigation, ground targeting, summon/unit selection and UI navigation remain to be designed in smaller focused decisions.

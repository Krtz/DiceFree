# Cube-edge transition prototype

The main-menu Development Room and World 2 test map now act as adjacent **48 x 48 metre cube faces**. Walk toward the north seam of the blue World 1 test face to reveal the purple World 2 face below its edge. Crossing the glowing pad and continuing through the seam turns the world a quarter-turn beneath the Echo. World 2 becomes the ground; World 1 is visible on its neighbouring face for the reverse trip.

The existing dev controls and interactive seam pad also provide a travel shortcut. Normal Cornberg/world routes and the production six-face topology are unchanged.

## How to try

1. Start menu -> Development Room.
2. Walk or click toward the glowing north-edge pad, or use Travel to World 2 test.
3. The camera reveals both faces; movement, actions and gear changes lock briefly.
4. The world rotates 90 degrees, then the camera settles on the destination face.
5. On World 2, use the south seam or Return to World 1 test room. Arrival does not immediately bounce you back; move inward before reapproaching the seam.

Blue face has one golden pip; purple face has two. These are functional test-room shapes, not final region environment art.

## Architecture

- CubeEdgeMap stores the paired edge/spawn metadata, CubeRotate style and renderer-only preview assets.
- CubeEdgePresentation renders the neighbouring face without actors, colliders or navigation. During crossing, a temporary world pivot rotates two visual surrogates and a presentation-only Echo.
- Real map transforms and NavMeshes stay flat and stationary. The same player, camera, HUD and gameplay state move between additive scenes; the old map unloads.
- Rotation is 2.2 seconds, with short camera preparation/settling around it. No camera barrel roll or global time pause.
- Destination spawn is the matching opposite seam. Preview coordinates map onto native destination coordinates at commit, including the existing World 2 offset of 40 km.
- Named action/movement locks, a temporary loadout lock and snapshots of previously enabled input/behaviour states prevent commands during the transition. Other owners' locks and previously disabled actions remain intact.
- Busy travel rejects duplicate requests and menu exits. Invalid destination metadata rolls back presentation, releases owned locks and unloads the failed destination.
- Core ExplorationCamera presentation override is inactive outside this dev prototype. Normal zoom/follow configuration is retained and travel recentres as before.

This proves the linked-face visual/loading illusion, not continuous cube-surface physics, all six production faces, multiplayer traversal or final cinematic polish.

## Verification

Focused isolated test passed **95 checks**, including neighbouring geometry inside the camera frustum, square faces, no preview gameplay components, an exact 45-degree midpoint, animated +/-90-degree turns, same-player round trip, actual walking through the seam, failure recovery, input/lock restoration and clean menu return.

Eleven equipped test items, their instance IDs, skill ranks, class/level, gold and a seeded Return Home cooldown survive the crossings. A normal-save sentinel remains byte-for-byte unchanged.

The conservation fixture disables ordinary Health/Mana regeneration so transport itself is checked exactly. A separate existing development-room regression measures the naturally enabled regeneration against elapsed time; the old near-instant-travel percentage threshold was unsuitable for animated crossings. No healing/refill or timer reset was added to transport.

Actual Unity Game-view captures were inspected at approach, midpoint and arrival:
- [Before: World 2 visible from World 1](screenshots/CubeEdge-Before.png)
- [Midpoint: world turning](screenshots/CubeEdge-Midpoint.png)
- [After: World 2 underfoot](screenshots/CubeEdge-After.png)

Authoring command: dicefree.cube-edge.author. It changes only the two test scenes, their navigation and original renderer-only face previews. Do not rerun the older DevelopmentRoomAuthoring builder to update this feature, because it recreates the wider session/player composition.

Focused commands: dicefree.cube-edge.test / status / resume. Capture pauses are Editor-only; no hold hook exists in the Windows player.

The existing menu/dev-room round-trip suite passed with enabled natural health/Mana regeneration measured against elapsed time. Greatblade's 75-check combat suite and all ten data/architecture/progression validators also passed. These are automated functional checks and inspected Editor Game views, not a full human playthrough.

Windows Development build succeeded with seven scenes, 0 errors and 0 warnings (348,175,132 reported bytes). Executable: T:\TEMP\DiceFree-Options\Builds\World1Foundation\DiceFree.exe.

The isolated non-graphical Windows player also passed actual menu entry and two animated crossings: World 1 -> World 2 -> World 1, two completed flips, same player entity, and released action/movement locks. This is standalone functional verification, not a standalone GPU/rendering or performance claim.

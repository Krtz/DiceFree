# Cornberg blockout — issue #15, Phase 1

This is the first authored World 1 pocket, based on issues #6, #14, #15 and
`WORLD_1.md`. It is not a separate mechanics room. East is +X, north is +Z;
Cornberg's green is near the origin. The southwest mountain remains reachable
on the return journey. The Great Tree is northeast, outside the playable pocket.

## Play

Open `Assets/_DiceFree/Scenes/Cornberg.unity` in Unity 6000.6.3f1 and press Play.
Cornberg is also the enabled build scene. The original empty Main scene remains
available as the foundation's reference scene.

- **Classic (default):** right-click open ground to issue a complete path.
- **F6:** switch between Classic and Direct. Switching cancels the old command.
- **Direct:** camera-relative WASD, using the same motor, speed and navigation.
- **Space:** stop. There is no jump, sprint, dodge or teleport binding.
- **Wheel:** zoom. **V:** toggle a lower lookout angle toward World 1's interior.
- **Arrows / middle-mouse drag:** pan independently; panning cancels follow.
- **Q/E:** rotate. **Home:** recenter. **F:** toggle persistent hero follow.

The input/camera design added to `setup/unity-project` during this task is preserved
in ancestry. Tab remains reserved for future hostile target cycling. This pass
implements a minimal camera-control subset, not the future selection, summons,
edge-scroll options, rebind UI, or ability-command framework.

The controls panel and floating location signs are blockout evaluation aids.
Building interiors, interactions, NPCs and gameplay are not implemented. The
well and single lit pip are visual placeholders, not healing/respawn logic.

## Geography

| Area | Approximate X/Z | Current representation |
| --- | --- | --- |
| Emergence | -42 / -30 | Southwest mountain recess, low rise, short dirt path |
| Cornberg green | 3 / 2 | Open green, well, corner-resting stone d6, benches |
| General goods | 17 / -13 | Separate timber/stone exterior |
| Blacksmith | -5 / 16 | Separate forge, chimney and hearth |
| Bank | 13 / 17 | Storehouse exterior and chest |
| Brewery | -4 / -14 | Working vats, communal tables and benches; no inn |
| Village outskirts | Around green | Five homes, barn, abandoned locked house; 11 buildings total |
| Abandoned house | 36 / -26 | Boarded door and lock, no invented contents or owner |
| Corn fields | 47 / -12 and 47 / 15 | Two tilled plots, corn rows and fences |
| Mountain stream | Roughly -18 / north–south | Recessed channel and two timber bridges |
| East/northeast road | 25 / 0 → 117 / 48 | Winding road framed by forest |
| Forest loops | North and south of road | Several openings, clearing pockets and reconnecting paths |
| Future Slime dungeon | 81 / 59 | Physical rock entrance, sealed exterior placeholder |
| Expansion boundary | Northeast diagonal | Continuous mossy ridge/thicket with trees; no accessible world edge |
| Great Tree | 300 / 330 | Huge trunk, branches and crown silhouette; distant meadow/hills are visual backdrop only |

The terrain is a low-relief editable mesh with a mountain-foot bowl, stream bed,
and southwest rise. Primitive geometry and flat materials deliberately stand in
for timber, fieldstone, soil, water and deciduous woodland. No external assets
were imported. DiceBound's Green Road and sunlit woodland battle reference were
visually inspected for the warm earth / green canopy / open road composition.

## Implementation boundaries

`TraversalInput` owns input modes and commands; `TraversalMotor` owns locomotion;
`ExplorationCamera` owns follow/zoom/lookout; `WorldNavigation` registers baked
navigation; `TraversalOverlay` displays the evaluation controls. No game manager,
combat, stat model, class tree, save system or multiplayer authority was added.

Only Unity's built-in AI and Audio modules were added (Audio enables the camera's
standard listener; no audio content is included). Navigation uses
`NavMeshBuilder` and `NavMeshAgent`; it does not require the AI Navigation package.
The agent uses a baked 0.5 m clearance, 1.8 m height, 40° slope limit and 0.4 m step.
Direct movement clamps diagonal input and projects movement onto that same mesh.
Clicks on solids or incomplete routes are rejected without replacing a valid path.

Layer 8 (`Walkable`) supplies terrain/bridges. Layer 9 (`WorldSolid`) supplies
building, water, tree-trunk and boundary collision. Decorative roofs, foliage and
crop stalks do not obstruct click picking. If scene geometry changes, rebake:
**DiceFree → Blockout → Rebake current Cornberg navigation**.

The serialized scene is the editable deliverable; nothing recreates it at runtime.
Editor authoring helpers are split between shapes/materials, landscape, village,
and scene composition. **Recreate Cornberg** is explicitly destructive and asks
for confirmation; use rebake after manual scene edits instead. Binary NavMeshData
is stored with Git LFS even though ordinary Unity YAML assets remain diffable.

## Provisional choices and risks

- Layout, 1 unit ≈ 1 m, traversal speed (5 m/s), mouse binding and camera values
  are inspector/code playtest parameters, not final balance or control canon.
- Classic and Direct are separate modes. Simultaneous Hybrid remains undecided.
- An elevated isometric view cannot reliably show a distant skyline. The V
  lookout is an evaluation option; the final landmark/camera treatment needs
  design review rather than silently changing the intended camera style.
  Tracked in [issue #16](https://github.com/Krtz/DiceFree/issues/16).
- Local +Y navigation proves only this pocket. It does not settle cube-edge
  gravity, streaming, network authority or movement prediction.
- Collision is authored/baked and static. Moving obstacles and runtime terrain
  changes will need navigation updates; larger characters need separate bake rules.
- Primitive canopy/roof occlusion and the flat-color lighting are not final art.
  Building interiors remain blocked. Streams can only be crossed at bridges.
- The historical level-4–5 crop-quest text conflicts with the later detailed
  ~2 → ~3 → ~5 → ~6 → ~10 chain. Phase 1 implements neither; use issue #6/latest
  decisions when adding progression, and reconcile old prose before tuning.
- Unity's batch Editor startup produced a SearchDatabase indexing exception,
  tracked in [issue #17](https://github.com/Krtz/DiceFree/issues/17). Validation
  records that exact editor-only stack separately; other errors still fail.

## Validation and next increment

`CornbergValidation.RunAll` is a batch-mode editor validation entry point. It
checks connected destinations, the return route, blocked water/building/boundary,
missing scripts, actual Play Mode motor traversal and Input System mouse/WASD
bindings, mode switching, diagonal speed and direct movement against the stream.
It writes evaluation renders under ignored `Logs/Cornberg/`.

The Windows development build is generated by **DiceFree → Build → Windows
Cornberg playtest** under ignored `Builds/Cornberg/DiceFree.exe`. It opens in a
1280×800 window. Keep the accompanying Data/runtime files alongside the executable.

Batch validation (omit `-quit`; the validation exits with success/failure itself):

```text
Unity.exe -batchmode -projectPath <repository> -executeMethod DiceFree.EditorTools.CornbergValidation.RunAll -logFile <log-path>
```

During the first Windows build, Unity also serialized current-version defaults
into the existing project/URP settings and recalculated shader prefilter fields.
The editor version and render-pipeline choice are unchanged.

Verified on 2026-09-28: all 13 destinations and the mountain return passed;
mouse/WASD input, mode changes, diagonal speed and stream collision passed in
Play Mode; the Windows development build succeeded and its standalone startup
smoke check reported no navigation/runtime errors. The standalone check caught
and fixed a native-agent initialization ordering problem: the agent is serialized
disabled and enabled in Start after world navigation registration.

The next increment is now implemented without regenerating this geography:
see [Cornberg combat notes](CORNBERG_COMBAT.md) for the one-Slime attack,
death/return/well loop, provisional values and validation. This document records
the Phase 1 baseline; the combat notes describe the current playable scope.

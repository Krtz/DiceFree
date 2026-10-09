# DiceFree â€” Village & Slime Animation Pass

Branch: feature/visual-overhaul-vis00
Status: Implemented, reviewed in Unity; not merged into main.

## Shipped behavior

### Village NPCs (Cornberg, 9 authored actors)

Role-specific secondary facial/hair/hat/property gestures attach to `VillageVisualIdle` without touching gameplay transforms, navigation, collisions, or interactions. Farmer Ã—2, Villager, Merchant, Banker, Alchemist, Blacksmith, Guard, Herbalist. Four already-wandering NPCs retain their NavMesh home routines; the five service NPCs remain stationary. Blacksmith hammer, alchemist vial and merchant accessory receive small contextual motions. Props return to authored transforms when disabled. Install and verify using `dicefree.motion.npcs.install`, `dicefree.motion.npcs.validate`.

**Art limitation:** the 8 currently authored village NPC models are rigid material-separated meshes, not true humanoid skinned rigs. The overall model has procedural breathing/weight-shift/walk bob and props get role cues; arms/legs do NOT have independent joint animation yet. The next necessary quality milestone is replacing the NPC models with rigged/skinned character versions and authored walk/gesture clips. Avoid marketing this pass as full humanoid animation.

### Slime monsters (Cornberg + runtime Slime Dungeon)
`SlimeVisualMotion` now includes idle gelatinous breathing, slow lateral wobble, movement-driven hop/squash/stretch, attack anticipation/recovery, stagger/recoil, deterministic desynchronized eye blinks and independent floating-gem motion where the authored mesh supports them. Includes different motion tempo for tiny, crop, elite and boss types. Monster movement and combat are unaffected because only child presentation transforms move.

IMPORTANT: `DungeonSlime.Busy` delegates entirely to boss-authored Slam, Divide, Roll and Bounce sequences; don't override them. The code auto-applies to dynamically spawned dungeon slimes because spawning already attaches `SlimeVisualMotion`. The 37 existing Cornberg slime actors already have the component.

### Visual QA
`dicefree.motion.capture` generates a two-row, 5-actor isolated orthographic contact sheet using actual Cornberg meshes at idle and active poses: Farmer / Blacksmith / Alchemist / Road Slime / Elite Forest Slime. Artifact:
`Assets/_DiceFree/Art/Validation/Previews/VillageAndSlimeMotion_Unity.png`.
Camera renders only the isolated preview clones, never the actual village scene.

### Regression gates
- Unity compilation clean.
- Village art validation, outer woodland NavMesh validation, Cornberg expansion validation and static slime dungeon validation passed.
- Village playtest passed 19,301 checks.
- Slime Dungeon playtest PASSED end-to-end after a lifecycle fix: slime VisualMotion now detects destroyed/replaced Presentation children during puzzle transitions and safely rebinds or skips the frame. An earlier run was polluted by an Editor-only command during Play Mode; an intermediate run exposed the real stale-visual defect. The final isolated rerun passed without errors.

### Next
- Rig the existing 8 unique NPC models (including correct hand-held props), author separate idle/walk/working/interaction cycles.
- Improve slime surface transparency, animated inner highlights, landing effects and distinct boss squash/stretch; ensure readability/performance at gameplay zoom.
- Add matching animation phases for NPC quest/merchant interactions and boss telegraphs.

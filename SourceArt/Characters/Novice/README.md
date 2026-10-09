# Novice Character Pipeline Prototype

Issue #45 corrects nearest-bone weighting to compare mesh vertices and bones in
the same character space. The old joined mesh origin offset produced incorrect
weights (including hands that did not follow their actual bones). The existing
source was migrated without changing mesh coordinates/topology/materials or the
four baseline objects; the corrected `build_novice.py` also reproduces the fixed
calculation. Evidence and the idempotent migration are in
`../CornbergWorkGloves/novice_weight_repair.json` and
`../CornbergWorkGloves/repair_novice_skin_weights.py`. Animation deformation is
intentionally corrected; baseline geometry is retained exactly.

This is the first DiceFree humanoid-rig and character-presentation proof, not final production art. It establishes one androgynous, blank-slate Echo with a simple T-shirt and baseline underwear presentation. The normal Novice starts without equipped gear; the sword is present only in a validation fixture.

`Novice.blend` is the editable Blender source. `build_novice.py` reconstructs the source, writes a humanoid FBX to `Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx`, and writes the authored collision profile and source counts. Blender units are metric, Z-up and +Y-forward; Unity import is responsible for the axis conversion. Run with Blender 5.2.2 LTS:

```powershell
blender --background --python SourceArt/Characters/Novice/build_novice.py
```

The FBX contains one armature, separate body/face/T-shirt/underwear skinned meshes, named rig bones, and animation actions for Idle, Locomotion and UnarmedAttack. Blender empties named `RightHandWeapon`, `LeftHandOffhand`, `Head`, `LeftFoot` and `RightFoot` are authoring/reference helpers only; the FBX export intentionally includes only armature and mesh object types, so those empties are not exported. Unity derives the presentation socket contract from the imported Humanoid bones and authors hand sockets in the prefab. The right-hand socket position is `(0, 0.025, 0)` relative to the imported `RightHand`; its rotation is converted from the intended character-space sword presentation basis through the imported bind-pose hand rotation. Consumers parent `CornbergFieldSwordPresentation` to `RightHandWeapon` with local position zero, rotation identity and scale one. The Editor validator checks Grip position and Grip-to-Tip direction against the socket, and the preview uses this same identity attachment. The sword fixture does not alter the sword prefab or mesh.

The prototype capsule profile is authored explicitly in `novice_body_profile.json` (1.72 m height, 0.28 m radius, center at 0.86 m); it is not derived from the render bounds and is not embedded in the art prefab. This proves separation of authored profile data from render geometry only; current Cornberg gameplay does not consume this JSON. Baseline T-shirt and underwear meshes remain separately skinned for inspection, with empty visual-slot roots as markers; this is not a working wearable replacement system.

The Editor validator samples the imported Locomotion clip, checks that the humanoid arm bone moves, and bakes the body skinned mesh before and during that sample. It requires a meaningful set of baked vertex positions to change, proving animation-to-bone-to-skin propagation. The prototype has 24,988 imported vertices and 49,936 triangles across four skinned meshes; these are recorded as a workflow baseline, not a production budget.

## Adventurer presentation upgrade (2026-10-09)
The canonical generator now builds a smooth, 34,624-vertex / 68,824-triangle skinned Novice with characterful chestnut hair, scarf, fitted travel vest, diagonal satchel strap, utility belt, fitted travel trousers and ankle boots. It keeps the existing four skinned renderer names, Human Avatar bones and weapon/offhand socket contract. The former `Novice_Baseline_Underwear` renderer now carries the baseline trousers and boots; the equipment-slot identifier was intentionally kept stable. Nine Blender material identities are authored across the skinned families; Unity's NoviceArtPipelineValidation maps submesh indices to external URP Lit materials. Source generation is deterministic and editable from this folder.

Idle, Locomotion and UnarmedAttack have new authored character-specific keyed poses; the `dicefree.art.novice.motion-validate` Editor command samples all three and checks meaningful rotation changes. The prefab, isolated character preview and Cornberg player preview were regenerated and visually inspected. Successful checks: `dicefree.art.novice.validate`, `.cornberg.validate`, `.motion-validate`, Unity compilation. Gear-swap harness checks (Work Gloves and Farmers Pants) currently encounter duplicate Hands/Legs equipment-slot bindings in the broader scene; this art milestone does not certify those regressions as fixed. The update is not a final facial sculpt or complete animation library.

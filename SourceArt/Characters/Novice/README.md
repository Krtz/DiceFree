# Novice Character Pipeline Prototype

This is the first DiceFree humanoid-rig and character-presentation proof, not final production art. It establishes one androgynous, blank-slate Echo with a simple T-shirt and baseline underwear presentation. The normal Novice starts without equipped gear; the sword is present only in a validation fixture.

`Novice.blend` is the editable Blender source. `build_novice.py` reconstructs the source, writes a humanoid FBX to `Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx`, and writes the authored collision profile and source counts. Blender units are metric, Z-up and +Y-forward; Unity import is responsible for the axis conversion. Run with Blender 5.2.2 LTS:

```powershell
blender --background --python SourceArt/Characters/Novice/build_novice.py
```

The FBX contains one armature and separate body/face/T-shirt/underwear/baseline-feet skinned meshes, named rig bones, and animation actions for Idle, Locomotion and UnarmedAttack. Baseline feet are separated from `Novice_BodySkin` so a visible footwear fixture can replace the unequipped foot surfaces. Blender empties named `RightHandWeapon`, `LeftHandOffhand`, `Head`, `LeftFoot` and `RightFoot` are authoring/reference helpers only; the FBX export intentionally includes only armature and mesh object types, so those empties are not exported. Unity derives presentation references from the imported Humanoid bones and authors hand sockets in the prefab. The right-hand socket position is `(0, 0.025, 0)` relative to the imported `RightHand`; its rotation is converted from the intended character-space sword presentation basis through the imported bind-pose hand rotation. Consumers parent `CornbergFieldSwordPresentation` to `RightHandWeapon` with local position zero, rotation identity and scale one. The Editor validator checks Grip position and Grip-to-Tip direction against the socket, and the preview uses this same identity attachment. The sword fixture does not alter the sword prefab or mesh.

The prototype capsule profile is authored explicitly in `novice_body_profile.json` (1.72 m height, 0.28 m radius, center at 0.86 m); it is not derived from the render bounds and is not embedded in the art prefab. This proves separation of authored profile data from render geometry only; current Cornberg gameplay does not consume this JSON. Baseline T-shirt, underwear and feet meshes remain separately skinned. The feet slot is used by the focused prototype fixture to demonstrate replacement; the empty T-shirt/underwear slot markers do not form a general wearable system.

The Editor validator samples the imported Locomotion clip, checks that the humanoid arm bone moves, and bakes the body skinned mesh before and during that sample. It requires a meaningful set of baked vertex positions to change, proving animation-to-bone-to-skin propagation. Mesh complexity after separating baseline feet is recorded by the validation output as prototype data, not a production budget.

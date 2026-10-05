# Farmer's Pants (#46)

Original humble Cornberg workwear, authored for the existing Novice form only.
Blender 5.2.2 LTS source `FarmersPants.blend` and generator use the real
`SourceArt/Characters/Novice/Novice.blend` armature, with its #45 coordinate-space
weight repair already present. No baseline mesh, materials, rig or clips are
modified by this pipeline. No texture/provider dependency or AI bitmap is used.

## Reproduce

From the repository root:

```powershell
& T:/TEMP/blender.exe --background --python SourceArt/Characters/FarmersPants/build_farmers_pants.py
```

Two clean runs matched geometry + semantic weight fingerprint
`8a22ff06453727d91ecfa992c8832944ebd7f882fc2c4eb630be28486174e22d`.
Source dependency SHA-256:
`1b64cf2281621b5e5af84b8bbff49416a01eeab987845149cc06df0a26d810d1`.
The generator checks closed/manifold meshes, outward winding and nondegenerate
faces. Fingerprints canonicalize geometry, topology/material assignments and
weights; FBX/blend container bytes can differ. Metrics: 19,532 source vertices,
39,048 triangles, four meshes. Voxel union is deliberately prototype quality;
production retopology/optimization remains deferred.

Metric, Z-up/+Y-forward source; FBX -Z forward/Y up, scale 1, no leaf bones,
no exported animations. One continuous canvas pelvis/leg shell, two worn hems
and a waistband. Broad silhouette/folds, muted dusty olive/brown canvas;
no armor, ornament or emissive material. Calf hems end above the feet. Weights
blend through Hips, Left/RightUpLeg and Left/RightLeg only. Waist stays on Hips;
no foot weights and no attachment to a duplicate runtime skeleton.

## Unity authoring

Use the CLI at `C:/Users/Axel/AppData/Local/Unity/bin/unity.exe`, with the
connected Unity 6000.6.3f1 Editor and explicit
`--project-path T:/TEMP/DiceFree-FarmersPants --format json` for every command:

```text
command set_autotick --enable true
command recompile
command recompile_status
command dicefree.art.pants.prepare
command dicefree.art.pants.validate
command dicefree.art.pants.capture-preview
command dicefree.art.pants.equipment-test
command dicefree.art.pants.status
```

Wait for successful compile and final test markers. `.prepare` creates/reuses
the item definition, URP/Lit canvas/hem materials, model wrapper and additive
Cornberg Legs binding, preserving the existing Hands binding. The shared
Editor-only `EquipmentArtAuthoring.Bind` resolves source bone names once while
authoring and removes the duplicate export skeleton. Runtime owns serialized
real bone references and uses the existing Animator; no runtime name lookup.

Importer: Generic skin, scale 1, readable for validation, authored normals;
no clips, blend shapes, cameras, lights, embedded materials or colliders.
Ordinary backface culling. Runtime exports/materials/prefab are under
`Assets/_DiceFree/Art/Characters/FarmersPants/`.

`EquipmentPresentation.Binding.baselineVisuals` hides only the baseline
underwear GameObject while these pants are resolved/equipped. It captures
activeSelf and restores that exact state on unequip, disable and reconfigure.
Body skin, T-shirt, face, feet and glove visuals/materials are preserved.
Independent bindings can replace Chest or layer Back without pants owning them.

## Validation limits

See `docs/FARMERS_PANTS_VALIDATION.md` for final evidence. Five samples per
Idle/Locomotion/UnarmedAttack verify actual pelvis/thigh/knee-chain alignment
and baked skin propagation. Existing clips contain thigh motion but no
independent knee articulation; a separately labelled synthetic 25-degree
left-knee flex proves calf propagation. It is not a new animation clip or a
claim of production cloth deformation. Feet coexistence uses temporary authored
foot-socket visuals because the shoe art branch remains separate/unmerged.

Preview: `Assets/_DiceFree/Art/Validation/Previews/FarmersPants_Unity.png`.
Human approval, cloth polish, cross-form fitting and mesh optimization remain
provisional. No manual gameplay playthrough is claimed.

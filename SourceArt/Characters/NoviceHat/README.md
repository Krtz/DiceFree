# Novice Hat prototype

This asset is a small proof of the Novice's empty Head wearable slot. It is a
prototype cap, not production character art.

## Source and export

- `NoviceHat.blend` is the editable Blender 5.2.2 LTS source.
- `build_novice_hat.py` rebuilds the mesh and exports `NoviceHat.fbx` into the
  Unity art folder. Its geometry fingerprint is
  `6532649e582a3bfcd79f497d299d823daa3a7760270005de7ece8ded1605ee4a`.
- The export has 264 vertices, 524 triangles, two meshes (crown and band), and
  two materials. Unity imports at scale 1 with normals and without animation,
  cameras, lights, blend shapes, embedded materials, or a generated collider.

The exported root is the attachment pivot. The cap geometry is authored above
that origin so the root can be parented directly to the semantic Humanoid Head
bone. The reusable `NoviceHatPresentation` prefab keeps an identity root and
uses Unity-authored URP/Lit materials with ordinary backface culling.

## Attachment and validation

The Novice has an intentionally empty baseline Head slot. A validation fixture
parents `NoviceHatPresentation` directly to the Avatar's Head bone using local
position zero, identity rotation, and unit scale. Removing the fixture restores
the empty baseline. The validation samples a transient Head pose and checks that
the hat follows the bone; this is presentation validation only and does not
change Novice starting equipment or gameplay.

The reproducible side-by-side preview is
`Assets/_DiceFree/Art/Validation/Previews/NoviceHat_Unity.png`. Pipeline
commands are `dicefree.art.hat.prepare`, `dicefree.art.hat.validate`,
`dicefree.art.hat.capture-preview`, and `dicefree.art.hat.build-windows`.

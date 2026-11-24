# Psion Tier III native asset derivatives

The original input archives were the user's local `warrior-mage.zip` and `card-magic-girl.zip`. Their GLBs have native skinning but no gameplay animation clips. The admitted canonical packages have verified hashes and vendor receipts in `.game-dev`; native FBX, textures, materials, avatars and equipment previews are in `Assets/_DiceFree/Settings/PsionEvolutions`.

Package identities:

- Knight: `pkg_631a2ca226a6b9374186a0bf`, `dicefree_psionic_knight/1.0.0`.
- Mage: `pkg_607fd1941c9d4133637e283a`, `dicefree_mirage_mage/1.0.0`.

Licensing is recorded as `LicenseRef-UserSupplied`, not an invented CC grant. Original attribution metadata should be filled in when the project's credits are assembled.

## Reproduction

Use Blender 4.5.3 and Node. Prepare an external workspace with the extracted archive paths `warrior-mage/source/Warrior Mage.glb` and `card-magic-girl/source/Card Magic Girl.glb`. Pass the workspace path after Blender's `--` separator:

```powershell
& $blender -b --python tools/assets/psion-tier3/convert_models.py -- $artWorkspace
& $blender -b --python tools/assets/psion-tier3/export_portable.py -- $artWorkspace
node tools/assets/psion-tier3/bake_uv.cjs $artWorkspace
& $blender -b --python tools/assets/psion-tier3/export_baked.py -- $artWorkspace
& $blender -b --python tools/assets/psion-tier3/repair_knight.py -- $artWorkspace
```

The UV step bakes matching `KHR_texture_transform` transforms into the UV coordinates before removing that required extension, then Blender regenerates tangents. It retains skinning. Use `PsionicKnight-repaired.fbx` for the Knight's native `PsionicKnight.fbx`; use `MirageMage.fbx` for the Mage. Copy the corresponding extracted PNG textures beside each native FBX, retaining existing Unity `.meta` files when updating.

The Knight repair makes mesh and skeleton rest positions coherent, normalizes their transforms, replaces legacy root weights and aligns the pelvis horizontally with the recentered visible mesh. The unmodified canonical GLB package remains available for provenance.

For authoring, compile `tools/authoring/PsionEvolutionAuthoring.cs` and `PsionEvolutionGearAuthoring.cs` in an isolated Unity Editor assembly, then run `PsionEvolutionAuthoring.Batch`. It updates the two class definitions, profiles, trials, class/menu catalogs, scene composition, native avatars/materials, hidden teachers and equipment assets. Run on a backed-up checkout; it saves Cornberg, DevelopmentRoom and WorldTwoArrival.

`native-derivatives.json` records the delivered FBX/PNG hashes. See `docs/classes/PSION_TIER3_IMPLEMENTATION_2026-11-24.md` for runtime validation and the unedited model previews.

The Unity authoring step additionally bakes coherent T-pose skin assets and creates matching humanoid avatars. Both the mesh and avatar must share that reference pose; a T-pose avatar over the old A-pose skin recreates the folded-arm bug. Arm reference rotations and palm positions are authored into `PsionEvolutionHandPose`, which supplies the fitted standing, movement and casting presentation. Palm positions come from the skinned geometry rather than these models' offset hand pivots. The manifest also records the native skin, avatar and form-prefab hashes. See `docs/classes/PSION_ARM_REPAIR_2026-11-24.md` for the corrective validation.

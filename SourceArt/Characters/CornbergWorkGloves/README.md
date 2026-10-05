# Cornberg Work Gloves - issue #45

Original humble farmer workwear for `item.cornberg.work-gloves` / Hands:
rounded worn brown leather palms/thumbs and short tan canvas cuffs. No metal,
ornament, paid provider, generated texture, or added gameplay stat.

From the repository root, with Blender 5.2.2 LTS:

```powershell
& 'T:\TEMP\blender.exe' --background --python SourceArt/Characters/CornbergWorkGloves/build_work_gloves.py
```

The script reads the existing `Novice.blend` rig without modifying it, clears
helper geometry/actions, authors the pair at the actual hand/wrist positions,
and exports one armature plus four skinned meshes in meters (FBX -Z forward,
Y up). Skin weights use character-space nearest hand/forearm segments, matching
the Novice. Source and runtime FBX are under Git LFS. Unity needs no Blender.

Two clean runs match the canonical geometry/material/weight fingerprint
`dfc8d252050dcf08149f94e13eaf9e52a15eda5f715f82ba1c3dc01935b80e10`.
The fingerprint sorts geometry to ignore voxel-remesher index ordering and
rounds coordinates to 0.00001 m. FBX/source container bytes can vary.
`export_manifest.json` includes the Novice source SHA and counts: 6,480 source
vertices, 12,944 triangles, four closed meshes with outward winding. The script
checks manifold edges, positive signed volume and nondegenerate faces.

## Necessary Novice skin-weight repair

Before issue #45, the Novice generator compared mesh-local coordinates against
character-space bone endpoints. The first joined primitive's translated origin
made weights incorrect: the real animated hand bones could move away from the
visible hands, and locomotion distorted the head. `build_novice.py` now converts
vertices to character space before measuring bone distances.

`repair_novice_skin_weights.py` migrates the already checked-in Novice source
without rebuilding its geometry, then exports it with the original preset.
The migration is idempotent for weights. `novice_weight_repair.json` proves the
exact source mesh coordinates/topology/material indices/object matrices match
before and after at SHA-256
`dda3889c96feec8cdb589f7e49bfed095dc36e9c689493497579d4cf2d7f42ad`.
Run the repair only when migrating a source with the old weight calculation;
ordinary glove regeneration uses only `build_work_gloves.py`.

## Unity reproduction and review

Use the connected Unity 6000.6.3f1 Editor and the project automation instructions:

```powershell
unity command dicefree.art.gloves.prepare --project-path T:\TEMP\DiceFree-WorkGloves --format json
unity command dicefree.art.gloves.validate --project-path T:\TEMP\DiceFree-WorkGloves --format json
unity command dicefree.art.gloves.capture-preview --project-path T:\TEMP\DiceFree-WorkGloves --format json
unity command dicefree.art.gloves.equipment-test --project-path T:\TEMP\DiceFree-WorkGloves --format json
unity command dicefree.art.gloves.status --project-path T:\TEMP\DiceFree-WorkGloves --format json
```

Preparation creates the URP/Lit materials and standalone authoring wrapper,
then rebinds its renderer bones to the actual Novice rig once in the Cornberg
scene. The exported duplicate rig is removed from the bound scene appearance.
Runtime performs no bone-name lookup. The saved glove visual is inactive.
`EquipmentPresentation` uses slot plus the resolved definition's stable ID;
it reads equipment changes and never grants items, changes stats, or reads quests.
The glove overlay leaves all four original baseline renderers intact.

The checked-in Unity preview is
`Assets/_DiceFree/Art/Validation/Previews/CornbergWorkGloves_Unity.png`.
It shows baseline, equipped idle, locomotion and attack fixtures. The separate
focused Play Mode harness proves the actual player-root equipment binding and
Q4/reload behavior using temporary saves, rather than treating that image as a
gameplay test. Art and rig remain prototypes pending human review; dense meshes,
single-bone clothing weights and animation polish are not production budgets.

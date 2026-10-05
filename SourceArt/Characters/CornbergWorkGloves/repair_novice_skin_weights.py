"""Repair existing Novice weights without rebuilding or altering any geometry.

The canonical Novice generator now uses this same coordinate-space correction.
This migration preserves the exact existing source mesh coordinates/topology.
"""
from pathlib import Path
import hashlib
import json
import bpy

ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / "SourceArt/Characters/Novice/Novice.blend"
EXPORT = ROOT / "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx"


def geometry(meshes):
    records = [(obj.name, [tuple(v.co) for v in obj.data.vertices],
                [tuple(p.vertices) for p in obj.data.polygons],
                [p.material_index for p in obj.data.polygons],
                [tuple(row) for row in obj.matrix_world]) for obj in sorted(meshes, key=lambda o: o.name)]
    return hashlib.sha256(json.dumps(records).encode()).hexdigest()


def main():
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = "NONE"
    rig = bpy.data.objects["Novice_Armature"]
    meshes = [o for o in bpy.data.objects if o.type == "MESH"]
    before = geometry(meshes)
    for obj in meshes:
        names = [g.name for g in obj.vertex_groups]
        for group in obj.vertex_groups:
            group.remove(range(len(obj.data.vertices)))
        for vertex in obj.data.vertices:
            point = obj.matrix_world @ vertex.co
            distances = {}
            for name in names:
                bone = rig.data.bones[name]
                a, b = bone.head_local, bone.tail_local
                axis = b - a
                t = max(0, min(1, (point - a).dot(axis) / axis.length_squared))
                distances[name] = (point - (a + axis * t)).length_squared
            obj.vertex_groups[min(distances, key=distances.get)].add([vertex.index], 1, "REPLACE")
    after = geometry(meshes)
    assert before == after, "Repair modified baseline geometry!"
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
    bpy.ops.object.select_all(action="DESELECT")
    rig.select_set(True)
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(filepath=str(EXPORT), use_selection=True, object_types={"ARMATURE", "MESH"},
        use_mesh_modifiers=True, add_leaf_bones=False, primary_bone_axis="Y", secondary_bone_axis="X",
        axis_forward="-Z", axis_up="Y", apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0, path_mode="AUTO", embed_textures=False)
    proof = dict(baselineGeometryBefore=before, baselineGeometryAfter=after, geometryUnchanged=before == after,
                 correction="mesh-local vertex transformed to character-space before nearest-bone weighting")
    (Path(__file__).parent / "novice_weight_repair.json").write_text(json.dumps(proof, indent=2) + "\n")
    print("NOVICE_WEIGHT_REPAIR_OK " + json.dumps(proof))


if __name__ == "__main__":
    main()

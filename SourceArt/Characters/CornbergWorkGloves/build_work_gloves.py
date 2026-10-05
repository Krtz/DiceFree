"""Blender 5.2.2: reproduce workwear shells against the unchanged Novice source rig.

Run from any directory: blender --background --python <this file>.
No external provider, texture, or baseline-body modification is required.
"""
from pathlib import Path
import hashlib
import json
import bpy
import bmesh

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SOURCE = ROOT / "SourceArt/Characters/Novice/Novice.blend"
EXPORT = ROOT / "Assets/_DiceFree/Art/Characters/CornbergWorkGloves/Models/CornbergWorkGloves.fbx"


def material(name, color):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*color, 1)
    mat.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = .9
    return mat


def ellipsoid(name, center, radii, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=10, location=center)
    obj = bpy.context.object
    obj.name = name
    obj.scale = radii
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.data.materials.append(mat)
    return obj


def main():
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = "NONE"
    rig = bpy.data.objects["Novice_Armature"]
    rig.animation_data_clear()
    for bone in rig.pose.bones:
        bone.matrix_basis.identity()
    for obj in list(bpy.data.objects):
        if obj != rig:
            bpy.data.objects.remove(obj, do_unlink=True)
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1
    leather = material("WorkGloves_WornLeather", (.37, .22, .105))
    cuff = material("WorkGloves_CanvasCuff", (.58, .44, .26))
    meshes = []
    for side, sign in (("Left", -1), ("Right", 1)):
        # Rounded broad palm encloses the actual 0.064/0.052/0.074 m hand.
        # Modest thumb and rolled cuff read as farm workwear at game distance.
        parts = [ellipsoid(side + "GlovePalm", (sign * .515, 0, 1.025), (.075, .063, .087), leather),
                 ellipsoid(side + "GloveThumb", (sign * .475, .048, 1.03), (.033, .035, .053), leather)]
        bpy.ops.object.select_all(action="DESELECT")
        for obj in parts:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = parts[0]
        bpy.ops.object.join()
        palm = bpy.context.object
        union = palm.modifiers.new("Continuous leather shell", "REMESH")
        union.mode = "VOXEL"
        union.voxel_size = .006
        bpy.ops.object.modifier_apply(modifier=union.name)
        # Cuff along the forearm near its wrist; all geometry shares the source
        # body's nearest-segment weights to avoid divergent wrist animation.
        band = ellipsoid(side + "GloveCuff", (sign * .465, 0, 1.063), (.071, .071, .052), cuff)
        for obj in (palm, band):
            obj.name = side + ("GloveLeather" if obj == palm else "GloveCuff")
            groups = {n: obj.vertex_groups.new(name=n) for n in (side + "Hand", side + "ForeArm")}
            for vertex in obj.data.vertices:
                distances = {}
                for n in groups:
                    bone = rig.data.bones[n]
                    a, b = bone.head_local, bone.tail_local
                    axis = b - a
                    t = max(0, min(1, (vertex.co - a).dot(axis) / axis.length_squared))
                    distances[n] = (vertex.co - (a + axis * t)).length_squared
                groups[min(distances, key=distances.get)].add([vertex.index], 1, "REPLACE")
            for face in obj.data.polygons:
                face.use_smooth = True
            obj.parent = rig
            mod = obj.modifiers.new("Actual Novice rig", "ARMATURE")
            mod.object = rig
            meshes.append(obj)
    digest = hashlib.sha256()
    for obj in meshes:
        bm = bmesh.new()
        bm.from_mesh(obj.data)
        assert all(e.is_manifold for e in bm.edges), obj.name + " has open/nonmanifold edges"
        assert bm.calc_volume(signed=True) > 0, obj.name + " has inward winding"
        assert all(f.calc_area() > 1e-10 for f in bm.faces), obj.name + " has degenerate faces"
        bm.free()
        digest.update(obj.name.encode())
        # Voxel remeshing can reorder vertex/face indices between clean runs.
        # Hash canonical geometry plus semantic weights, not incidental indices.
        coords = [tuple(round(float(c), 5) for c in v.co) for v in obj.data.vertices]
        vertices = sorted((coords[v.index], tuple(sorted((obj.vertex_groups[w.group].name, round(w.weight, 5)) for w in v.groups))) for v in obj.data.vertices)
        faces = sorted((p.material_index, tuple(sorted(coords[i] for i in p.vertices))) for p in obj.data.polygons)
        digest.update(json.dumps([vertices, faces], separators=(",", ":")).encode())
    EXPORT.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(HERE / "CornbergWorkGloves.blend"))
    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(filepath=str(EXPORT), use_selection=True,
        object_types={"ARMATURE", "MESH"}, add_leaf_bones=False,
        primary_bone_axis="Y", secondary_bone_axis="X", axis_forward="-Z", axis_up="Y",
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL", bake_anim=False,
        use_mesh_modifiers=True, path_mode="AUTO", embed_textures=False)
    manifest = dict(itemId="item.cornberg.work-gloves", slot="Hands", units="meters",
                    noviceSourceSha256=hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
                    geometryWeightsSha256=digest.hexdigest(),
                    closedOutwardMeshes=True,
                    vertices=sum(len(o.data.vertices) for o in meshes),
                    triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes),
                    meshes=[o.name for o in meshes])
    (HERE / "export_manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print("WORK_GLOVES_EXPORT_OK " + json.dumps(manifest))


if __name__ == "__main__":
    main()

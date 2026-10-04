"""Build a simple paired Novice footwear proof in the character's source axes."""

from pathlib import Path
import hashlib
import struct
import bpy

ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / "SourceArt/Characters/NoviceShoes/NoviceShoes.blend"
FBX = ROOT / "Assets/_DiceFree/Art/Characters/NoviceShoes/Models/NoviceShoes.fbx"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def material(name, color, roughness):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def rounded_box(name, center, dimensions, bevel, mat):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=center)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    modifier = obj.modifiers.new("Soft prototype edges", "BEVEL")
    modifier.width = bevel
    modifier.segments = 2
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    for poly in obj.data.polygons:
        poly.use_smooth = True
    return obj


def geometry_fingerprint(meshes):
    digest = hashlib.sha256()
    for obj in sorted(meshes, key=lambda item: item.name):
        digest.update(obj.name.encode("utf-8"))
        for vertex in obj.data.vertices:
            digest.update(struct.pack("<3f", *vertex.co))
        for polygon in obj.data.polygons:
            digest.update(struct.pack("<I", polygon.material_index))
            digest.update(struct.pack("<I", len(polygon.vertices)))
            for index in polygon.vertices:
                digest.update(struct.pack("<I", index))
    return digest.hexdigest()


def main():
    FBX.parent.mkdir(parents=True, exist_ok=True)
    clear_scene()
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    bpy.context.preferences.filepaths.save_version = 0

    upper = material("Novice_Shoe_Warm_Brown", (0.22, 0.105, 0.052), 0.82)
    sole = material("Novice_Shoe_Dark_Sole", (0.075, 0.055, 0.043), 0.91)
    root = bpy.data.objects.new("NoviceShoes_Root", None)
    bpy.context.collection.objects.link(root)
    all_meshes = []
    for side, sign in (("Left", -1), ("Right", 1)):
        group = bpy.data.objects.new(side + "ShoeGroup", None)
        bpy.context.collection.objects.link(group)
        group.parent = root
        group.location = (sign * 0.12, 0.0, 0.20)  # authored Novice foot-bone head
        attachment = bpy.data.objects.new(side + "ShoeAttachment", None)
        bpy.context.collection.objects.link(attachment)
        attachment.parent = group
        attachment.location = (0.0, 0.0, 0.0)

        # Build around the authored Novice foot in Blender Z-up/+Y-forward axes.
        local_x = sign * 0.12
        parts = [
            rounded_box(side + "Shoe_Sole", (local_x, 0.09, 0.025), (0.195, 0.32, 0.050), 0.018, sole),
            rounded_box(side + "Shoe_Upper", (local_x, 0.075, 0.092), (0.170, 0.275, 0.105), 0.035, upper),
        ]
        for obj in parts:
            # Keep the authored character-root-space placement while grouping
            # each shoe so Unity can bind the complete shoe to its foot bone.
            obj.parent = group
            obj.matrix_parent_inverse = group.matrix_world.inverted()
            all_meshes.append(obj)

    fingerprint = geometry_fingerprint(all_meshes)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in [root] + [item for item in bpy.data.objects if item.type in {"EMPTY", "MESH"}]:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
    bpy.ops.export_scene.fbx(
        filepath=str(FBX), use_selection=True, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True, bake_space_transform=False,
        object_types={"EMPTY", "MESH"}, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", use_tspace=False, use_custom_props=False,
        use_metadata=False, add_leaf_bones=False, bake_anim=False,
        path_mode="AUTO", axis_forward="-Z", axis_up="Y")
    vertices = sum(len(obj.data.vertices) for obj in all_meshes)
    triangles = sum(sum(max(1, len(poly.vertices) - 2) for poly in obj.data.polygons) for obj in all_meshes)
    print("DICEFRE_SHOES_EXPORT_OK")
    print("DICEFRE_SHOES_GEOMETRY_SHA256=" + fingerprint)
    print("DICEFRE_SHOES_SOURCE=" + str(SOURCE))
    print("DICEFRE_SHOES_FBX=" + str(FBX))
    print("DICEFRE_SHOES_COUNTS vertices=%d triangles=%d meshParts=%d" % (vertices, triangles, len(all_meshes)))


if __name__ == "__main__":
    main()

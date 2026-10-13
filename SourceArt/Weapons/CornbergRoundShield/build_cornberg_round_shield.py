"""Build the low-detail Cornberg Round Shield proof asset with Blender 5.2.2 LTS."""

from pathlib import Path
import hashlib
import math
import bmesh
import bpy


HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
BLEND_PATH = HERE / "CornbergRoundShield.blend"
FBX_PATH = REPO / "Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Models/CornbergRoundShield.fbx"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        if collection.name != "Collection":
            bpy.data.collections.remove(collection)
    scene = bpy.context.scene
    export = bpy.data.collections.get("Export") or bpy.data.collections.new("Export")
    if export.name not in scene.collection.children:
        scene.collection.children.link(export)
    return export


def material(name, color, metallic, roughness):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    shader = value.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    return value


def move_to_collection(obj, collection):
    for old_collection in list(obj.users_collection):
        old_collection.objects.unlink(obj)
    collection.objects.link(obj)


def finish_mesh(mesh):
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.triangulate(bm, faces=[face for face in bm.faces if len(face.verts) > 4])
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()


def bevel_object(obj, width):
    bevel = obj.modifiers.new("Broad softened prototype edges", "BEVEL")
    bevel.width = width
    bevel.segments = 1
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    obj.select_set(False)


def plate(name, outline, center_y, depth, mat, collection, bevel_width):
    count = len(outline)
    back_y = center_y - depth * 0.5
    front_y = center_y + depth * 0.5
    verts = [(x, y, z) for y in (back_y, front_y) for x, z in outline]
    faces = [tuple(reversed(range(count))), tuple(range(count, count * 2))]
    for index in range(count):
        nxt = (index + 1) % count
        faces.append((index, nxt, nxt + count, index + count))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(mat)
    finish_mesh(mesh)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    if bevel_width:
        bevel_object(obj, bevel_width)
        finish_mesh(obj.data)
    return obj


def rim(name, outline, center_y, depth, mat, collection):
    center_x = sum(point[0] for point in outline) / len(outline)
    center_z = sum(point[1] for point in outline) / len(outline)
    inner = [(center_x + (x - center_x) * 0.91, center_z + (z - center_z) * 0.91) for x, z in outline]
    count = len(outline)
    back_y = center_y - depth * 0.5
    front_y = center_y + depth * 0.5
    verts = ([(x, back_y, z) for x, z in outline] +
             [(x, front_y, z) for x, z in outline] +
             [(x, back_y, z) for x, z in inner] +
             [(x, front_y, z) for x, z in inner])
    ob, of, ib, inf = 0, count, count * 2, count * 3
    faces = []
    for index in range(count):
        nxt = (index + 1) % count
        faces.extend((
            (of + index, of + nxt, inf + nxt, inf + index),
            (ob + nxt, ob + index, ib + index, ib + nxt),
            (ob + index, ob + nxt, of + nxt, of + index),
            (ib + nxt, ib + index, inf + index, inf + nxt),
        ))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(mat)
    finish_mesh(mesh)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    return obj


def closed_meshes(collection):
    for obj in (item for item in collection.objects if item.type == "MESH"):
        bm = bmesh.new()
        try:
            bm.from_mesh(obj.data)
            open_edges = sum(not edge.is_manifold for edge in bm.edges)
            volume = bm.calc_volume(signed=True)
            if open_edges or volume <= 0.0:
                raise RuntimeError("%s is not a closed outward-facing mesh (openEdges=%d signedVolume=%f)" % (obj.name, open_edges, volume))
        finally:
            bm.free()


def face_shield_front_toward_blender_minus_y(collection):
    # FBX converts source -Y into Unity local +Z, matching the shield
    # presentation's canonical Grip->ShieldFront axis.
    for obj in (item for item in collection.objects if item.type == "MESH"):
        bm = bmesh.new()
        try:
            bm.from_mesh(obj.data)
            for vertex in bm.verts:
                vertex.co.y = -vertex.co.y
            bmesh.ops.reverse_faces(bm, faces=list(bm.faces))
            bm.to_mesh(obj.data)
            obj.data.update()
        finally:
            bm.free()


def fingerprint(collection):
    digest = hashlib.sha256()
    for obj in sorted((item for item in collection.objects if item.type == "MESH"), key=lambda item: item.name):
        digest.update((obj.name + "\n").encode("utf-8"))
        for vertex in obj.data.vertices:
            digest.update(("%.6f,%.6f,%.6f\n" % tuple(vertex.co)).encode("ascii"))
        for polygon in obj.data.polygons:
            digest.update(("%d:%s:%s\n" % (polygon.material_index,
                ",".join(str(index) for index in polygon.vertices),
                ",".join("%.6f" % value for value in polygon.normal))).encode("ascii"))
    return digest.hexdigest()


def main():
    FBX_PATH.parent.mkdir(parents=True, exist_ok=True)
    collection = clear_scene()
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    bpy.context.preferences.filepaths.save_version = 0

    wood = material("Cornberg_Shield_Warm_Oak", (0.30, 0.16, 0.075), 0.0, 0.82)
    iron = material("Cornberg_Shield_Forged_Iron", (0.22, 0.27, 0.29), 0.38, 0.62)
    boss_mat = material("Cornberg_Shield_Iron_Boss", (0.31, 0.37, 0.39), 0.48, 0.5)
    leather = material("Cornberg_Shield_Dark_Leather", (0.085, 0.042, 0.024), 0.0, 0.88)

    root = bpy.data.objects.new("CornbergRoundShield_Root", None)
    root.empty_display_type = "CUBE"
    root.empty_display_size = 0.035
    collection.objects.link(root)

    # A plain tapered heater silhouette: broad shoulders, blunt lower point.
    outline = [(-0.235, 0.365), (0.235, 0.365), (0.305, 0.255),
               (0.285, 0.015), (0.205, -0.225), (0.0, -0.390),
               (-0.205, -0.225), (-0.285, 0.015), (-0.305, 0.255)]
    objects = [
        plate("CornbergRoundShield_Board", outline, 0.105, 0.075, wood, collection, 0.012),
        rim("CornbergRoundShield_Rim", outline, 0.155, 0.045, iron, collection),
    ]

    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.078, depth=0.036,
        location=(0.0, 0.190, 0.025), rotation=(math.pi * 0.5, 0.0, 0.0))
    boss = bpy.context.object
    boss.name = "CornbergRoundShield_Boss"
    boss.data.name = boss.name + "Mesh"
    move_to_collection(boss, collection)
    boss.data.materials.append(boss_mat)
    finish_mesh(boss.data)
    objects.append(boss)

    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0.0, -0.025, 0.0))
    handle = bpy.context.object
    handle.name = "CornbergRoundShield_Handle"
    handle.dimensions = (0.058, 0.190, 0.050)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    move_to_collection(handle, collection)
    handle.data.materials.append(leather)
    bevel_object(handle, 0.008)
    finish_mesh(handle.data)
    objects.append(handle)

    for obj in objects:
        obj.parent = root

    grip = bpy.data.objects.new("Grip", None)
    grip.parent = root
    collection.objects.link(grip)
    front = bpy.data.objects.new("ShieldFront", None)
    front.parent = root
    # Keep the semantic front marker on the grip plane so Grip->ShieldFront
    # measures orientation without a vertical offset.
    front.location = (0.0, -0.230, 0.0)
    collection.objects.link(front)

    face_shield_front_toward_blender_minus_y(collection)
    closed_meshes(collection)
    digest = fingerprint(collection)
    bpy.ops.object.select_all(action="DESELECT")
    export_objects = list(collection.objects)
    for obj in export_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))

    # The FBX axis conversion maps Blender -Y front to Unity local +Z. The
    # hand socket then aims that canonical prop axis along character forward.
    bpy.ops.export_scene.fbx(
        filepath=str(FBX_PATH), use_selection=True, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True, bake_space_transform=False,
        object_types={"EMPTY", "MESH"}, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", use_tspace=False, use_custom_props=False,
        use_metadata=False, add_leaf_bones=False, bake_anim=False,
        path_mode="AUTO", axis_forward="-Z", axis_up="Y")

    print("DICEFRE_SHIELD_EXPORT_OK")
    print("DICEFRE_SHIELD_GEOMETRY_SHA256=" + digest)
    print("DICEFRE_SHIELD_SOURCE=" + str(BLEND_PATH))
    print("DICEFRE_SHIELD_FBX=" + str(FBX_PATH))
    print("DICEFRE_SHIELD_COUNTS meshes=%d vertices=%d triangles=%d" % (
        len([obj for obj in collection.objects if obj.type == "MESH"]),
        sum(len(obj.data.vertices) for obj in collection.objects if obj.type == "MESH"),
        sum(sum(max(1, len(poly.vertices) - 2) for poly in obj.data.polygons) for obj in collection.objects if obj.type == "MESH")))


if __name__ == "__main__":
    main()

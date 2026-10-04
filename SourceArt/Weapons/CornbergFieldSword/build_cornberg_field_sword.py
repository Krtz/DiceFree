"""Deterministically build and export the Cornberg Field Sword proof asset.

Run with Blender 5.2.2 LTS:
    blender --background --python SourceArt/Weapons/CornbergFieldSword/build_cornberg_field_sword.py
"""

from pathlib import Path
import hashlib
import math
import bmesh
import bpy
from mathutils import Matrix, Vector


HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
BLEND_PATH = HERE / "CornbergFieldSword.blend"
FBX_PATH = REPO / "Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Models/CornbergFieldSword.fbx"
PREVIEW_PATH = HERE / "Previews/CornbergFieldSword_Isometric.png"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        if collection.name != "Collection":
            bpy.data.collections.remove(collection)
    scene = bpy.context.scene
    export = bpy.data.collections.get("Export") or bpy.data.collections.new("Export")
    preview = bpy.data.collections.get("Preview") or bpy.data.collections.new("Preview")
    if export.name not in scene.collection.children:
        scene.collection.children.link(export)
    if preview.name not in scene.collection.children:
        scene.collection.children.link(preview)
    return export, preview


def move_to_collection(obj, collection):
    for old in list(obj.users_collection):
        old.objects.unlink(obj)
    collection.objects.link(obj)


def make_material(name, color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    return mat


def recalculate_normals(mesh):
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()


def triangulate_ngons(mesh):
    bm = bmesh.new()
    bm.from_mesh(mesh)
    ngons = [face for face in bm.faces if len(face.verts) > 4]
    if ngons:
        bmesh.ops.triangulate(bm, faces=ngons, quad_method="BEAUTY", ngon_method="BEAUTY")
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()


def validate_closed_export_meshes(collection):
    """Fail the export if a rigid sword mesh has open/non-manifold edges."""
    for obj in (item for item in collection.objects if item.type == "MESH"):
        bm = bmesh.new()
        try:
            bm.from_mesh(obj.data)
            open_edges = [edge for edge in bm.edges if not edge.is_manifold]
            if open_edges:
                raise RuntimeError("%s has %d open or non-manifold edges" % (obj.name, len(open_edges)))
            signed_volume = bm.calc_volume(signed=True)
            if signed_volume <= 0.0:
                raise RuntimeError("%s does not have outward-facing closed surface normals (signed volume %f)" % (obj.name, signed_volume))
        finally:
            bm.free()


def set_origin_at_root(obj):
    bpy.context.scene.cursor.location = (0.0, 0.0, 0.0)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")


def bevelled_box(name, dimensions, location, material, collection, bevel):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel > 0:
        mod = obj.modifiers.new("Broad edge bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        mod.affect = "EDGES"
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.data.materials.append(material)
    move_to_collection(obj, collection)
    set_origin_at_root(obj)
    return obj


def prism_along_y(name, radius, depth, location, material, collection, sides=8):
    cx, cy, cz = location
    verts = []
    for y in (cy - depth * 0.5, cy + depth * 0.5):
        for i in range(sides):
            angle = 2.0 * math.pi * i / sides + math.pi / sides
            verts.append((cx + math.cos(angle) * radius, y, cz + math.sin(angle) * radius))
    faces = [tuple(range(sides - 1, -1, -1)), tuple(range(sides, sides * 2))]
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((i, j, j + sides, i + sides))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(material)
    recalculate_normals(mesh)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    set_origin_at_root(obj)
    return obj


def extruded_guard(material, collection):
    # Broad, slightly swept-down quillons: sturdy local work, not jewelry.
    outline = [
        (-0.176, 0.205), (-0.150, 0.222), (-0.084, 0.177),
        (-0.040, 0.159), (0.000, 0.153), (0.040, 0.159),
        (0.084, 0.177), (0.150, 0.222), (0.176, 0.205),
        (0.163, 0.171), (0.105, 0.130), (0.066, 0.126),
        (0.000, 0.148), (-0.066, 0.126), (-0.105, 0.130),
        (-0.163, 0.171),
    ]
    thickness = 0.050
    count = len(outline)
    verts = [(x, y, z) for z in (-thickness * 0.5, thickness * 0.5) for x, y in outline]
    faces = [tuple(reversed(range(count))), tuple(range(count, count * 2))]
    for i in range(count):
        j = (i + 1) % count
        faces.append((i, j, j + count, i + count))
    mesh = bpy.data.meshes.new("CornbergGuardMesh")
    mesh.from_pydata(verts, [], faces)
    triangulate_ngons(mesh)
    recalculate_normals(mesh)
    mesh.materials.append(material)
    recalculate_normals(mesh)
    obj = bpy.data.objects.new("CornbergFieldSword_Hilt", mesh)
    collection.objects.link(obj)
    bevel = obj.modifiers.new("Soft forged edges", "BEVEL")
    bevel.width = 0.006
    bevel.segments = 1
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    obj.select_set(False)
    set_origin_at_root(obj)
    return obj


def make_blade(steel, edge, collection):
    # Editable source follows Blender +Y; the deterministic FBX export pass below
    # applies its documented half-turn so the Unity presentation faces local +Z.
    rows = [(0.174, 0.066), (0.295, 0.078), (0.665, 0.061)]
    across = (-1.0, -0.34, 0.0, 0.34, 1.0)
    heights = (0.003, 0.010, 0.015, 0.010, 0.003)
    verts = []
    row_size = len(across) * 2
    for y, width in rows:
        for face_sign in (1.0, -1.0):
            for x_unit, height in zip(across, heights):
                verts.append((x_unit * width * 0.5, y, face_sign * height))
    tip_index = len(verts)
    verts.append((0.0, 0.905, 0.0))
    faces = []
    material_ids = []
    # Close the blade's root cross-section beneath the guard. Backface culling
    # remains enabled in Unity, so the rigid blade is a closed solid surface.
    root_ring = tuple(range(len(across))) + tuple(range(len(across) * 2 - 1, len(across) - 1, -1))
    faces.append(root_ring)
    material_ids.append(0)
    row_count = len(rows)
    for r in range(row_count - 1):
        for side in range(2):
            base0 = r * row_size + side * len(across)
            base1 = (r + 1) * row_size + side * len(across)
            for x in range(len(across) - 1):
                faces.append((base0 + x, base1 + x, base1 + x + 1, base0 + x + 1))
                # Lighter material is a broad edge plane, not fine engraving.
                material_ids.append(1 if x in (0, 3) else 0)
    last = (row_count - 1) * row_size
    for side in range(2):
        base = last + side * len(across)
        for x in range(len(across) - 1):
            faces.append((base + x, tip_index, base + x + 1))
            material_ids.append(1 if x in (0, 3) else 0)
    # Narrow edge faces close the steel cross-section.
    for r in range(row_count - 1):
        a = r * row_size
        b = (r + 1) * row_size
        faces.append((a, a + len(across), b + len(across), b))
        material_ids.append(1)
        ar = a + len(across) - 1
        br = b + len(across) - 1
        faces.append((ar, br, br + len(across), ar + len(across)))
        material_ids.append(1)
    for edge_index in (0, len(across) - 1):
        back = last + len(across) + edge_index
        front = last + edge_index
        faces.append((front, back, tip_index))
        material_ids.append(1)
    mesh = bpy.data.meshes.new("CornbergBladeMesh")
    mesh.from_pydata(verts, [], faces)
    triangulate_ngons(mesh)
    mesh.materials.append(steel)
    mesh.materials.append(edge)
    recalculate_normals(mesh)
    blade = bpy.data.objects.new("CornbergFieldSword_Blade", mesh)
    collection.objects.link(blade)
    for polygon, material_id in zip(mesh.polygons, material_ids):
        polygon.material_index = material_id
        polygon.use_smooth = False
    set_origin_at_root(blade)
    return blade


def join_meshes(objects, name, collection):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    active = objects[0]
    bpy.context.view_layer.objects.active = active
    bpy.ops.object.join()
    active.name = name
    active.data.name = name + "Mesh"
    move_to_collection(active, collection)
    set_origin_at_root(active)
    for poly in active.data.polygons:
        poly.use_smooth = False
    return active


def add_empty(name, parent, location, collection, size=0.055):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = size
    collection.objects.link(obj)
    obj.parent = parent
    obj.location = location
    obj.rotation_euler = (0.0, 0.0, 0.0)
    obj.scale = (1.0, 1.0, 1.0)
    return obj


def add_preview_stage(collection, preview_material):
    bpy.ops.mesh.primitive_plane_add(size=2.2, location=(0.0, 0.52, -0.075))
    floor = bpy.context.object
    floor.name = "PreviewStage_NotExported"
    floor.dimensions = (3.8, 4.0, 1.0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    floor.data.materials.append(preview_material)
    move_to_collection(floor, collection)

    ruler = bevelled_box("ScaleReference_1p8m_NotExported", (0.012, 1.80, 0.012), (0.34, 0.50, -0.052), preview_material, collection, 0.002)
    # Short 10 cm tick bars make the meter-scale reference legible without text.
    ticks = []
    for i in range(19):
        y = -0.40 + i * 0.10
        width = 0.034 if i % 5 == 0 else 0.018
        ticks.append(bevelled_box("ScaleTick_NotExported", (width, 0.008, 0.008), (0.34, y, -0.044), preview_material, collection, 0.001))
    return [floor, ruler] + ticks


def add_camera_and_lights(collection):
    bpy.ops.object.camera_add(location=(2.25, -0.10, 3.9))
    camera = bpy.context.object
    camera.name = "IsometricPreviewCamera_NotExported"
    target = Vector((0.08, 0.50, 0.0))
    direction = target - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 2.45
    camera.data.lens = 55
    bpy.context.scene.camera = camera
    move_to_collection(camera, collection)

    lights = []
    for name, location, power, size, color in (
        ("KeyArea_NotExported", (1.7, -0.2, 3.0), 360, 3.5, (1.0, 0.86, 0.70)),
        ("FillArea_NotExported", (-2.0, 1.5, 2.3), 220, 3.0, (0.68, 0.80, 1.0)),
    ):
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.name = name
        light.data.energy = power
        light.data.shape = "DISK"
        light.data.size = size
        light.data.color = color
        light.rotation_euler = (Vector((0.0, 0.4, 0.0)) - light.location).to_track_quat("-Z", "Y").to_euler()
        move_to_collection(light, collection)
        lights.append(light)
    return [camera] + lights


def geometry_fingerprint(collection):
    digest = hashlib.sha256()
    for obj in sorted((item for item in collection.objects if item.type == "MESH"), key=lambda item: item.name):
        digest.update((obj.name + "\n").encode("utf-8"))
        for vertex in obj.data.vertices:
            digest.update(("%.6f,%.6f,%.6f\n" % tuple(vertex.co)).encode("ascii"))
        for polygon in obj.data.polygons:
            digest.update(("%d:%s:%s\n" % (
                polygon.material_index,
                ",".join(str(i) for i in polygon.vertices),
                ",".join("%.6f" % component for component in polygon.normal),
            )).encode("ascii"))
    return digest.hexdigest()


def main():
    FBX_PATH.parent.mkdir(parents=True, exist_ok=True)
    PREVIEW_PATH.parent.mkdir(parents=True, exist_ok=True)
    export_collection, preview_collection = clear_scene()
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0

    steel = make_material("Cornberg_Practical_Steel", (0.25, 0.34, 0.39), 0.62, 0.34)
    edge = make_material("Cornberg_Polished_Edge", (0.48, 0.57, 0.60), 0.58, 0.30)
    bronze = make_material("Cornberg_Warm_Forged_Iron", (0.28, 0.145, 0.065), 0.48, 0.44)
    leather = make_material("Cornberg_Dark_Grip_Leather", (0.085, 0.042, 0.026), 0.02, 0.76)
    preview_mat = make_material("Preview_Slate_NotExported", (0.095, 0.115, 0.13), 0.04, 0.88)

    root = bpy.data.objects.new("CornbergFieldSword_Root", None)
    root.empty_display_type = "CUBE"
    root.empty_display_size = 0.035
    export_collection.objects.link(root)
    blade = make_blade(steel, edge, export_collection)

    handle_parts = [
        bevelled_box("GripBody", (0.048, 0.250, 0.048), (0.0, -0.004, 0.0), leather, export_collection, 0.008),
        bevelled_box("GripLowerFerrule", (0.061, 0.035, 0.060), (0.0, -0.125, 0.0), bronze, export_collection, 0.006),
        bevelled_box("GripUpperFerrule", (0.061, 0.035, 0.060), (0.0, 0.112, 0.0), bronze, export_collection, 0.006),
    ]
    # A few broad raised leather bands imply a hand-wrapped grip at game scale.
    for index, y in enumerate((-0.073, -0.015, 0.043)):
        handle_parts.append(bevelled_box("GripWrapBand_%02d" % (index + 1), (0.050, 0.018, 0.050), (0.0, y, 0.0), leather, export_collection, 0.004))
    grip = join_meshes(handle_parts, "CornbergFieldSword_Grip", export_collection)

    metal_parts = [extruded_guard(bronze, export_collection)]
    metal_parts.append(prism_along_y("PlainEightSidedPommel", 0.052, 0.055, (0.0, -0.169, 0.0), bronze, export_collection))
    hilt = join_meshes(metal_parts, "CornbergFieldSword_Hilt", export_collection)

    for obj in (blade, grip, hilt):
        obj.parent = root
        obj.location = (0.0, 0.0, 0.0)
        obj.rotation_euler = (0.0, 0.0, 0.0)
        obj.scale = (1.0, 1.0, 1.0)
    add_empty("Grip", root, (0.0, 0.0, 0.0), export_collection)
    tip_anchor = add_empty("Tip", root, (0.0, 0.905, 0.0), export_collection)

    validate_closed_export_meshes(export_collection)

    add_preview_stage(preview_collection, preview_mat)
    add_camera_and_lights(preview_collection)
    fingerprint = geometry_fingerprint(export_collection)
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1100
    scene.render.resolution_y = 900
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.film_transparent = False
    scene.view_settings.view_transform = "AgX"
    scene.world.color = (0.12, 0.14, 0.16)
    scene.render.filepath = str(PREVIEW_PATH)

    # Freeze the object set before each output operation; only the Export
    # collection reaches FBX, while the camera, stage and ruler remain source-only.
    bpy.ops.object.select_all(action="DESELECT")
    export_objects = list(export_collection.objects)
    for obj in export_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    # Blender's standard Y-up/-Z-forward FBX basis maps this source's +Y blade
    # direction to Unity -Z. Bake a proper half-turn for the export only so the
    # imported blade points +Z while the editable Blender source stays +Y-up.
    export_turn = Matrix.Rotation(math.pi, 4, "Z")
    for obj in export_objects:
        if obj.type == "MESH":
            obj.data.transform(export_turn)
    tip_anchor.location.y = -0.905
    try:
        bpy.ops.export_scene.fbx(
            filepath=str(FBX_PATH),
            use_selection=True,
            global_scale=1.0,
            apply_unit_scale=True,
            apply_scale_options="FBX_SCALE_UNITS",
            use_space_transform=True,
            bake_space_transform=False,
            object_types={"EMPTY", "MESH"},
            use_mesh_modifiers=True,
            mesh_smooth_type="OFF",
            colors_type="SRGB",
            use_tspace=False,
            use_custom_props=False,
            use_metadata=False,
            add_leaf_bones=False,
            bake_anim=False,
            path_mode="AUTO",
            axis_forward="-Z",
            axis_up="Y",
        )
    finally:
        for obj in export_objects:
            if obj.type == "MESH":
                obj.data.transform(export_turn)
        tip_anchor.location.y = 0.905
    bpy.ops.object.select_all(action="DESELECT")
    scene.render.filepath = str(PREVIEW_PATH)
    bpy.ops.render.render(write_still=True)
    print("CORNBERG_SWORD_EXPORT_OK")
    print("GEOMETRY_SHA256=" + fingerprint)
    print("BLEND=" + str(BLEND_PATH))
    print("FBX=" + str(FBX_PATH))
    print("PREVIEW=" + str(PREVIEW_PATH))


if __name__ == "__main__":
    main()

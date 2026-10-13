"""Build one simple Novice cap whose DCC root origin attaches to the Head bone."""
from pathlib import Path
import hashlib
import struct
import bpy

ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / "SourceArt/Characters/NoviceHat/NoviceHat.blend"
FBX = ROOT / "Assets/_DiceFree/Art/Characters/NoviceHat/Models/NoviceHat.fbx"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in (bpy.data.meshes, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(collection):
            if datablock.users == 0:
                collection.remove(datablock)


def material(name, color, roughness):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def lathe(name, rings, segments, mat):
    """Closed rotational surface around local Blender Z; profile is radius,z."""
    vertices = []
    for radius, z in rings:
        for i in range(segments):
            angle = 2.0 * 3.141592653589793 * i / segments
            vertices.append((radius * __import__("math").cos(angle), radius * __import__("math").sin(angle), z))
    faces = []
    for ring in range(len(rings) - 1):
        for i in range(segments):
            a = ring * segments + i
            b = ring * segments + (i + 1) % segments
            c = (ring + 1) * segments + (i + 1) % segments
            d = (ring + 1) * segments + i
            faces.append((a, b, c, d))
    faces.append(tuple(reversed(range(segments))))
    last = (len(rings) - 1) * segments
    faces.append(tuple(last + i for i in range(segments)))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.materials.append(mat)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    return obj


def band_mesh(name, outer_radius, inner_radius, bottom, top, segments, mat):
    vertices = []
    for radius, z in ((outer_radius, bottom), (outer_radius, top), (inner_radius, top), (inner_radius, bottom)):
        for i in range(segments):
            angle = 2.0 * 3.141592653589793 * i / segments
            vertices.append((radius * __import__("math").cos(angle), radius * __import__("math").sin(angle), z))
    faces = []
    for ring in range(4):
        nxt = (ring + 1) % 4
        for i in range(segments):
            a = ring * segments + i
            b = ring * segments + (i + 1) % segments
            c = nxt * segments + (i + 1) % segments
            d = nxt * segments + i
            faces.append((a, b, c, d))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.materials.append(mat)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    return obj


def fingerprint(meshes):
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

    # The exported mesh origin is the semantic mount: place it at the imported
    # Novice Head bone origin. The cap itself rises above that origin in Z-up DCC.
    fabric = material("Novice_Hat_Muted_Green_Fabric", (0.20, 0.27, 0.22), 0.86)
    band = material("Novice_Hat_Warm_Brown_Band", (0.24, 0.13, 0.075), 0.9)
    crown = lathe("NoviceHat_Crown", [(0.122, 0.255), (0.137, 0.275), (0.132, 0.31), (0.112, 0.355), (0.078, 0.392), (0.035, 0.414), (0.008, 0.418)], 24, fabric)
    lower_band = band_mesh("NoviceHat_Band", 0.151, 0.119, 0.244, 0.271, 24, band)
    meshes = [crown, lower_band]

    digest = fingerprint(meshes)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = crown
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
    bpy.ops.export_scene.fbx(
        filepath=str(FBX), use_selection=True, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True, bake_space_transform=False,
        object_types={"MESH"}, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", use_tspace=False, use_custom_props=False,
        use_metadata=False, add_leaf_bones=False, bake_anim=False,
        path_mode="AUTO", axis_forward="-Z", axis_up="Y")
    vertices = sum(len(obj.data.vertices) for obj in meshes)
    triangles = sum(sum(max(1, len(poly.vertices) - 2) for poly in obj.data.polygons) for obj in meshes)
    print("DICEFRE_NOVICE_HAT_EXPORT_OK")
    print("DICEFRE_NOVICE_HAT_GEOMETRY_SHA256=" + digest)
    print("DICEFRE_NOVICE_HAT_SOURCE=" + str(SOURCE))
    print("DICEFRE_NOVICE_HAT_FBX=" + str(FBX))
    print("DICEFRE_NOVICE_HAT_COUNTS vertices=%d triangles=%d meshes=%d materials=2" % (vertices, triangles, len(meshes)))


if __name__ == "__main__":
    main()

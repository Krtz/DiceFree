"""Build the DiceFree Novice character source, humanoid rig and FBX export.

Run from the repository root with Blender 5.2.2 LTS:
    blender --background --python SourceArt/Characters/Novice/build_novice.py

The source uses metric units, Blender Z-up and +Y forward. Unity's FBX importer
converts the authored front to its conventional +Z-facing presentation.
"""

from pathlib import Path
import hashlib
import json
import math
import struct
import bpy
from mathutils import Vector


HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
BLEND = HERE / "Novice.blend"
FBX = REPO / "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx"
PROFILE = HERE / "novice_body_profile.json"


def ensure_dirs():
    (HERE / "Previews").mkdir(parents=True, exist_ok=True)
    FBX.parent.mkdir(parents=True, exist_ok=True)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.armatures, bpy.data.actions):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)


def material(name, color, roughness=0.72):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*color, 1.0)
    mat.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = roughness
    return mat


def uv_ellipsoid(name, center, radii, mat, segments=16, rings=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=1.0, location=center)
    obj = bpy.context.object
    obj.name = name
    obj.scale = radii
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    for poly in obj.data.polygons:
        poly.use_smooth = True
    return obj


def capsule_segment(name, start, end, width, depth, mat, segments=14, rings=8):
    a, b = Vector(start), Vector(end)
    direction = b - a
    center = (a + b) * 0.5
    obj = uv_ellipsoid(name, center, (width, depth, direction.length * 0.5 + width * 0.35), mat, segments, rings)
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = direction.to_track_quat("Z", "Y")
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    return obj


def make_rig():
    arm_data = bpy.data.armatures.new("Novice_Humanoid_Rig")
    rig = bpy.data.objects.new("Novice_Armature", arm_data)
    bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    defs = {
        "Root": ((0, 0, 0.00), (0, 0, 0.12), None),
        "Hips": ((0, 0, 0.82), (0, 0, 1.04), "Root"),
        "Spine": ((0, 0, 1.00), (0, 0, 1.22), "Hips"),
        "Chest": ((0, 0, 1.20), (0, 0, 1.42), "Spine"),
        "Neck": ((0, 0, 1.39), (0, 0, 1.52), "Chest"),
        "Head": ((0, 0, 1.50), (0, 0, 1.77), "Neck"),
    }
    for side, sign in (("Left", -1), ("Right", 1)):
        defs[side + "Shoulder"] = ((sign * 0.13, 0, 1.36), (sign * 0.21, 0, 1.32), "Chest")
        defs[side + "Arm"] = ((sign * 0.20, 0, 1.32), (sign * 0.34, 0, 1.20), side + "Shoulder")
        defs[side + "ForeArm"] = ((sign * 0.34, 0, 1.20), (sign * 0.48, 0, 1.04), side + "Arm")
        defs[side + "Hand"] = ((sign * 0.48, 0, 1.04), (sign * 0.55, 0, 1.02), side + "ForeArm")
        defs[side + "UpLeg"] = ((sign * 0.10, 0, 0.91), (sign * 0.12, 0, 0.56), "Hips")
        defs[side + "Leg"] = ((sign * 0.12, 0, 0.57), (sign * 0.12, 0, 0.19), side + "UpLeg")
        defs[side + "Foot"] = ((sign * 0.12, 0, 0.20), (sign * 0.12, 0.14, 0.08), side + "Leg")
        defs[side + "Toes"] = ((sign * 0.12, 0.10, 0.08), (sign * 0.12, 0.22, 0.08), side + "Foot")
    for name, (head, tail, parent) in defs.items():
        bone = arm_data.edit_bones.new(name)
        bone.head, bone.tail = head, tail
        if parent:
            bone.parent = arm_data.edit_bones[parent]
            bone.use_connect = False
    bpy.ops.object.mode_set(mode="OBJECT")
    rig.show_in_front = True
    rig.data.display_type = "OCTAHEDRAL"
    return rig, defs


def make_skinned_family(name, parts, rig, bone_defs, weighted_bones):
    created = []
    for part_name, bone, builder in parts:
        obj = builder()
        obj.name = part_name
        created.append(obj)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in created:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = created[0]
    bpy.ops.object.join()
    combined = bpy.context.object
    combined.name = name
    combined.data.name = name + "_Mesh"
    # Fuse overlapping authoring forms so the prototype reads as continuous
    # clothing/body surfaces instead of separate primitive capsules.
    remesh = combined.modifiers.new("PrototypeSurfaceUnion", "REMESH")
    remesh.mode = "VOXEL"
    remesh.voxel_size = 0.012
    remesh.use_smooth_shade = True
    bpy.context.view_layer.objects.active = combined
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    groups = {bone: combined.vertex_groups.new(name=bone) for bone in weighted_bones}
    for vertex in combined.data.vertices:
        best_name, best_distance = None, float("inf")
        point = Vector(vertex.co)
        for bone_name in weighted_bones:
            head, tail, _ = bone_defs[bone_name]
            start, end = Vector(head), Vector(tail)
            axis = end - start
            t = max(0.0, min(1.0, (point - start).dot(axis) / max(axis.length_squared, 1e-9)))
            distance = (point - (start + axis * t)).length_squared
            if distance < best_distance:
                best_name, best_distance = bone_name, distance
        groups[best_name].add([vertex.index], 1.0, "REPLACE")
    for polygon in combined.data.polygons:
        polygon.use_smooth = True
    mod = combined.modifiers.new("Novice_Rig", "ARMATURE")
    mod.object = rig
    combined.parent = rig
    combined.matrix_parent_inverse = rig.matrix_world.inverted()
    return combined


def build_model():
    skin = material("Novice_Skin", (0.66, 0.48, 0.36), 0.78)
    shirt = material("Novice_Baseline_Tee", (0.22, 0.39, 0.41), 0.78)
    cloth = material("Novice_Baseline_Underwear", (0.29, 0.25, 0.24), 0.82)
    eye = material("Novice_Eyes", (0.08, 0.07, 0.065), 0.42)
    rig, rig_defs = make_rig()

    skin_parts = []
    def add(family, label, bone, fn):
        family.append((label, bone, fn))

    add(skin_parts, "Head", "Head", lambda: uv_ellipsoid("Head", (0, 0.006, 1.63), (0.128, 0.116, 0.158), skin, 20, 12))
    add(skin_parts, "Neck", "Neck", lambda: uv_ellipsoid("Neck", (0, 0, 1.425), (0.066, 0.065, 0.105), skin))
    for side, sign in (("Left", -1), ("Right", 1)):
        add(skin_parts, side + "Ear", "Head", lambda s=sign: uv_ellipsoid("Ear", (s * 0.132, 0, 1.63), (0.025, 0.035, 0.045), skin, 12, 8))
        add(skin_parts, side + "Forearm", side + "ForeArm", lambda s=sign: capsule_segment("Forearm", (s * 0.31, 0, 1.23), (s * 0.49, 0, 1.035), 0.059, 0.064, skin))
        add(skin_parts, side + "Hand", side + "Hand", lambda s=sign: uv_ellipsoid("Hand", (s * 0.515, 0, 1.025), (0.064, 0.052, 0.074), skin))
        add(skin_parts, side + "Thigh", side + "UpLeg", lambda s=sign: capsule_segment("Thigh", (s * 0.105, 0, 0.91), (s * 0.12, 0, 0.55), 0.088, 0.095, skin))
        add(skin_parts, side + "LowerLeg", side + "Leg", lambda s=sign: capsule_segment("LowerLeg", (s * 0.12, 0, 0.60), (s * 0.12, 0, 0.18), 0.066, 0.073, skin))
        add(skin_parts, side + "Foot", side + "Foot", lambda s=sign: uv_ellipsoid("Foot", (s * 0.12, 0.085, 0.105), (0.081, 0.15, 0.066), skin))
    face_parts = []
    for side, sign in (("Left", -1), ("Right", 1)):
        add(face_parts, side + "Eye", "Head", lambda s=sign: uv_ellipsoid("Eye", (s * 0.047, 0.108, 1.65), (0.021, 0.010, 0.014), eye, 12, 8))
    add(face_parts, "Mouth", "Head", lambda: uv_ellipsoid("Mouth", (0, 0.112, 1.600), (0.018, 0.009, 0.005), eye, 12, 8))

    shirt_parts = []
    add(shirt_parts, "TShirt_Torso", "Chest", lambda: uv_ellipsoid("TShirt_Torso", (0, 0, 1.225), (0.215, 0.135, 0.24), shirt, 20, 12))
    add(shirt_parts, "TShirt_Lower", "Spine", lambda: uv_ellipsoid("TShirt_Lower", (0, 0, 1.075), (0.202, 0.128, 0.155), shirt, 18, 10))
    for side, sign in (("Left", -1), ("Right", 1)):
        add(shirt_parts, side + "Sleeve", side + "Arm", lambda s=sign: capsule_segment("Sleeve", (s * 0.12, 0, 1.35), (s * 0.35, 0, 1.205), 0.086, 0.094, shirt))
    shorts_parts = []
    add(shorts_parts, "Underwear_Shorts", "Hips", lambda: uv_ellipsoid("Underwear_Shorts", (0, -0.002, 0.90), (0.19, 0.118, 0.135), cloth, 18, 10))
    for side, sign in (("Left", -1), ("Right", 1)):
        add(shorts_parts, side + "ShortLeg", side + "UpLeg", lambda s=sign: capsule_segment("ShortLeg", (s * 0.095, 0, 0.91), (s * 0.105, 0, 0.79), 0.091, 0.12, cloth))

    skin_obj = make_skinned_family("Novice_BodySkin", skin_parts, rig, rig_defs,
        ["Head", "Neck", "LeftForeArm", "LeftHand", "RightForeArm", "RightHand", "LeftUpLeg", "LeftLeg", "LeftFoot", "RightUpLeg", "RightLeg", "RightFoot"])
    face_obj = make_skinned_family("Novice_FaceDetails", face_parts, rig, rig_defs, ["Head"])
    shirt_obj = make_skinned_family("Novice_Baseline_TShirt", shirt_parts, rig, rig_defs, ["Spine", "Chest", "LeftArm", "RightArm"])
    shorts_obj = make_skinned_family("Novice_Baseline_Underwear", shorts_parts, rig, rig_defs, ["Hips", "LeftUpLeg", "RightUpLeg"])
    # Preserve separate slot objects while sharing one armature and authored groups.
    for obj in (skin_obj, face_obj, shirt_obj, shorts_obj):
        obj["presentation_slot"] = "Baseline" if obj != skin_obj else "Body"

    # DCC socket empties are authoring/reference helpers only. The FBX export
    # intentionally includes ARMATURE and MESH objects only; Unity derives its
    # semantic presentation sockets from the imported humanoid bones.
    for side, sign in (("Left", -1), ("Right", 1)):
        socket = bpy.data.objects.new("RightHandWeapon" if side == "Right" else "LeftHandOffhand", None)
        bpy.context.collection.objects.link(socket)
        socket.empty_display_type = "SPHERE"
        socket.empty_display_size = 0.035
        socket.parent = rig
        socket.parent_type = "BONE"
        socket.parent_bone = side + "Hand"
        socket.location = (0, 0.035, 0)
    for name, bone, local in (
        ("Head", "Head", (0, 0, 0.12)),
        ("LeftFoot", "LeftFoot", (0, 0, 0)),
        ("RightFoot", "RightFoot", (0, 0, 0)),
    ):
        socket = bpy.data.objects.new(name, None)
        bpy.context.collection.objects.link(socket)
        socket.empty_display_type = "CIRCLE"
        socket.empty_display_size = 0.025
        socket.parent = rig
        socket.parent_type = "BONE"
        socket.parent_bone = bone
        socket.location = local

    return rig, (skin_obj, face_obj, shirt_obj, shorts_obj)


def key_pose(rig, name, poses, frame_start, frame_end):
    scene = bpy.context.scene
    action = bpy.data.actions.new(name)
    rig.animation_data_create()
    rig.animation_data.action = action
    for frame, values in poses:
        scene.frame_set(frame)
        for bone_name in ("LeftArm", "RightArm", "LeftForeArm", "RightForeArm", "LeftUpLeg", "RightUpLeg"):
            bone = rig.pose.bones.get(bone_name)
            bone.rotation_mode = "XYZ"
            bone.rotation_euler = values.get(bone_name, (0.0, 0.0, 0.0))
            bone.keyframe_insert(data_path="rotation_euler", frame=frame, group=bone_name)
    action.use_fake_user = True
    scene.frame_start, scene.frame_end = frame_start, frame_end
    rig.animation_data.action = None
    return action


def build_actions(rig):
    idle = {"LeftArm": (0, 0, -0.06), "RightArm": (0, 0, 0.06)}
    walk_a = {"LeftArm": (-0.16, 0, -0.08), "RightArm": (0.16, 0, 0.08), "LeftUpLeg": (0.20, 0, 0), "RightUpLeg": (-0.20, 0, 0)}
    walk_b = {"LeftArm": (0.16, 0, -0.08), "RightArm": (-0.16, 0, 0.08), "LeftUpLeg": (-0.20, 0, 0), "RightUpLeg": (0.20, 0, 0)}
    attack = {"RightArm": (-0.55, 0, -0.28), "RightForeArm": (-0.48, 0, 0)}
    actions = [
        key_pose(rig, "Novice|Idle", [(1, idle), (31, idle)], 1, 31),
        key_pose(rig, "Novice|Locomotion", [(1, walk_a), (7, walk_b), (13, walk_a)], 1, 13),
        key_pose(rig, "Novice|UnarmedAttack", [(1, {}), (7, attack), (13, {})], 1, 13),
    ]
    return actions


def fingerprint(objects):
    digest = hashlib.sha256()
    for obj in sorted(objects, key=lambda item: item.name):
        if obj.type != "MESH":
            continue
        mesh = obj.data
        digest.update(obj.name.encode("utf-8"))
        for vertex in mesh.vertices:
            digest.update(struct.pack("<3f", *vertex.co))
        for polygon in mesh.polygons:
            digest.update(struct.pack("<I", len(polygon.vertices)))
            for index in polygon.vertices:
                digest.update(struct.pack("<I", index))
        for group in sorted(obj.vertex_groups, key=lambda item: item.name):
            digest.update(group.name.encode("utf-8"))
    return digest.hexdigest()


def main():
    ensure_dirs()
    clear_scene()
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.render.engine = "CYCLES"
    scene.render.resolution_x, scene.render.resolution_y = 1024, 1024
    if hasattr(bpy.context.preferences.filepaths, "save_preview_images"):
        bpy.context.preferences.filepaths.save_preview_images = False
    rig, meshes = build_model()
    actions = build_actions(rig)

    # The working scene retains all authored objects and actions for editing.
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))

    bpy.ops.object.select_all(action="DESELECT")
    rig.select_set(True)
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(
        filepath=str(FBX), use_selection=True, object_types={"ARMATURE", "MESH"},
        use_mesh_modifiers=True, add_leaf_bones=False, primary_bone_axis="Y",
        secondary_bone_axis="X", axis_forward="-Z", axis_up="Y",
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
        bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0,
        path_mode="AUTO", embed_textures=False,
    )
    vertices = sum(len(obj.data.vertices) for obj in meshes)
    triangles = sum(sum(max(1, len(poly.vertices) - 2) for poly in obj.data.polygons) for obj in meshes)
    data = {
        "name": "Novice",
        "source": "Novice.blend",
        "export": "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx",
        "units": "meters",
        "sourceAxes": "Z up, +Y forward",
        "heightMeters": 1.78,
        "bodyWidthMeters": 0.42,
        "authoredGameplayCapsule": {"heightMeters": 1.72, "radiusMeters": 0.28, "centerMeters": [0, 0, 0.86]},
        "clothingSlots": ["BaselineTShirt", "BaselineUnderwear"],
        "animationActions": [action.name for action in actions],
        "vertices": vertices,
        "triangles": triangles,
        "meshObjects": len(meshes),
        "materialNames": sorted({mat.name for obj in meshes for mat in obj.data.materials}),
    }
    PROFILE.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
    print("DICEFRE_NOVICE_SOURCE_SAVED=" + str(BLEND))
    print("DICEFRE_NOVICE_EXPORT=" + str(FBX))
    print("DICEFRE_NOVICE_GEOMETRY_SHA256=" + fingerprint(meshes))
    print("DICEFRE_NOVICE_COUNTS vertices=%d triangles=%d meshes=%d materials=%d" % (vertices, triangles, len(meshes), len(data["materialNames"])))
    print("DICEFRE_NOVICE_ACTIONS=" + ",".join(action.name for action in actions))


if __name__ == "__main__":
    main()

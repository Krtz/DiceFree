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


def fitted_garment(name, material, lower, upper, waist, chest, shoulder, back_depth=.125):
    """Editable sculpted tailoring, not a spherical placeholder. Smooth 64x32 quad shell."""
    rings, slices = 32, 64
    vertices, faces = [], []
    for row in range(rings+1):
        t=row/rings
        radius_x = waist*(1-t)+shoulder*t + (chest-(waist+shoulder)*.5)*math.sin(math.pi*t)
        radius_y = back_depth + .012*math.sin(math.pi*t)
        for j in range(slices):
            a=2*math.pi*j/slices
            fold=.0020*math.sin(a*9+t*15)*math.sin(math.pi*t)
            x=(radius_x+fold)*math.cos(a)
            y=.013+(radius_y+fold)*math.sin(a)
            z=lower+(upper-lower)*t+.003*math.cos(a*4+t*12)*math.sin(math.pi*t)
            vertices.append((x,y,z))
    for row in range(rings):
        for j in range(slices):
            n=row*slices+j
            nxt=row*slices+(j+1)%slices
            faces.append((n,nxt,nxt+slices,n+slices))
    mesh=bpy.data.meshes.new(name+"_Mesh")
    mesh.from_pydata(vertices,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh)
    bpy.context.collection.objects.link(obj);mesh.materials.append(material)
    for poly in mesh.polygons:poly.use_smooth=True
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
    # Preserve individually authored fabric, leather, hair and metal surface colors.
    # Blender's voxel remesher discards material indices; use it only for
    # single-material anatomical skin, never for multi-material clothing.
    distinct_materials = {mat.name for mat in combined.data.materials}
    if len(distinct_materials) == 1:
        remesh = combined.modifiers.new("AnatomicalSurfaceUnion", "REMESH")
        remesh.mode = "VOXEL"
        remesh.voxel_size = 0.012
        remesh.use_smooth_shade = True
        bpy.context.view_layer.objects.active = combined
        bpy.ops.object.modifier_apply(modifier=remesh.name)
    groups = {bone: combined.vertex_groups.new(name=bone) for bone in weighted_bones}
    for vertex in combined.data.vertices:
        best_name, best_distance = None, float("inf")
        # Joined primitives retain the first part's translated object origin.
        # Bone definitions are character-space, so compare in that same space.
        point = combined.matrix_world @ vertex.co
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
    hair = material("Novice_Chestnut_Hair", (0.19, 0.095, 0.053), 0.67)
    leather = material("Novice_Leather", (0.24, 0.14, 0.088), 0.73)
    vest = material("Novice_Travel_Vest", (0.35, 0.30, 0.245), 0.89)
    pants = material("Novice_Travel_Trousers", (0.25, 0.285, 0.23), 0.88)
    scarf = material("Novice_Warm_Scarf", (0.56, 0.265, 0.155), 0.93)
    brass = material("Novice_Dull_Brass", (0.62, 0.46, 0.20), 0.39)
    rig, rig_defs = make_rig()

    skin_parts = []
    def add(family, label, bone, fn):
        family.append((label, bone, fn))

    add(skin_parts, "Head", "Head", lambda: uv_ellipsoid("Head", (0, 0.006, 1.63), (0.128, 0.116, 0.158), skin, 20, 12))
    add(skin_parts, "Nose", "Head", lambda: uv_ellipsoid("Nose", (0, 0.118, 1.619), (.021,.028,.027), skin, 20, 14))
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
    add(face_parts, "Mouth", "Head", lambda: uv_ellipsoid("Mouth", (0, 0.114, 1.571), (0.024, 0.009, 0.005), eye, 16, 12))
    # Distinct, gently tousled hair—not a helmet-shaped bald mannequin.
    add(face_parts, "HairCrown", "Head", lambda: uv_ellipsoid("HairCrown", (0,-.016,1.745),(.130,.115,.079),hair,36,22))
    add(face_parts, "HairBack", "Head", lambda: uv_ellipsoid("HairBack", (0,-.080,1.690),(.117,.047,.096),hair,30,18))
    add(face_parts, "HairLeftFringe", "Head", lambda: capsule_segment("HairLeftFringe",(-.091,.086,1.760),(-.041,.121,1.698),.035,.029,hair,24,14))
    add(face_parts, "HairSideSweep", "Head", lambda: capsule_segment("HairSideSweep",(.012,.097,1.774),(.074,.099,1.737),.034,.024,hair,24,14))
    for side,sign in (("Left",-1),("Right",1)):
        add(face_parts,side+"Eyebrow","Head",lambda t=sign: uv_ellipsoid("Eyebrow",(t*.052,.117,1.677),(.029,.011,.008),hair,22,12))
        add(face_parts,side+"TempleHair","Head",lambda t=sign: uv_ellipsoid("TempleHair",(t*.121,-.009,1.698),(.020,.054,.074),hair,22,14))

    shirt_parts = []
    add(shirt_parts, "TShirt_Torso", "Chest", lambda: uv_ellipsoid("TShirt_Torso", (0, 0, 1.225), (0.215, 0.135, 0.24), shirt, 20, 12))
    add(shirt_parts, "TShirt_Lower", "Spine", lambda: uv_ellipsoid("TShirt_Lower", (0, 0, 1.075), (0.202, 0.128, 0.155), shirt, 18, 10))
    for side, sign in (("Left", -1), ("Right", 1)):
        add(shirt_parts, side + "Sleeve", side + "Arm", lambda s=sign: capsule_segment("Sleeve", (s * 0.12, 0, 1.35), (s * 0.35, 0, 1.205), 0.086, 0.094, shirt))
    # Practical layered clothing with handcrafted accents: vest, scarf, worn leather
    # shoulder strap and satchel. All are skinned to the same functioning Humanoid.
    add(shirt_parts,"CanvasTravelVest","Chest",lambda: fitted_garment("CanvasTravelVest",vest,.99,1.405,.19,.223,.143,.151))
    add(shirt_parts,"ScarfNeckWrap","Chest",lambda: uv_ellipsoid("ScarfNeckWrap",(0,.007,1.425),(.140,.125,.039),scarf,40,22))
    add(shirt_parts,"ScarfFold","Chest",lambda: capsule_segment("ScarfFold",(-.055,.129,1.429),(.033,.143,1.380),.031,.019,scarf,24,16))
    add(shirt_parts,"SatchelDiagonalStrap","Chest",lambda: capsule_segment("SatchelDiagonalStrap",(-.163,.205,1.367),(.185,.205,1.047),.022,.017,leather,30,18))
    add(shirt_parts,"UtilityBelt","Spine",lambda: uv_ellipsoid("UtilityBelt",(0,.018,1.014),(.221,.178,.039),leather,48,18))
    add(shirt_parts,"BeltBrassBuckle","Spine",lambda: uv_ellipsoid("BeltBrassBuckle",(0,.193,1.014),(.037,.014,.030),brass,28,16))
    add(shirt_parts,"SideSatchelBody","Spine",lambda: uv_ellipsoid("SideSatchelBody",(.235,-.032,.977),(.114,.071,.127),leather,32,22))
    add(shirt_parts,"SatchelFlap","Spine",lambda: uv_ellipsoid("SatchelFlap",(.235,.005,1.038),(.111,.051,.049),vest,32,18))
    add(shirt_parts,"SatchelClasp","Spine",lambda: uv_ellipsoid("SatchelClasp",(.236,.053,1.015),(.018,.009,.022),brass,20,14))
    for z in (1.19,1.12):
        add(shirt_parts,"VestButton_"+str(z),"Chest",lambda zz=z:uv_ellipsoid("VestButton",(0,.164,zz),(.014,.011,.014),brass,20,12))
    shorts_parts = []
    # Long, tapered trousers and worn boots replace the un-dressed prototype legs.
    add(shorts_parts,"TrousersHip","Hips",lambda: fitted_garment("TrousersHip",pants,.775,1.027,.185,.205,.178,.139))
    for side,sign in (("Left",-1),("Right",1)):
        add(shorts_parts,side+"TravelTrouserUpper",side+"UpLeg",lambda t=sign: capsule_segment("TrouserUpper",(t*.105,0,.87),(t*.12,0,.535),.096,.101,pants,28,18))
        add(shorts_parts,side+"TravelTrouserLower",side+"Leg",lambda t=sign: capsule_segment("TrouserLower",(t*.12,0,.58),(t*.12,0,.202),.077,.083,pants,28,18))
        add(shorts_parts,side+"WornBootCuff",side+"Leg",lambda t=sign:uv_ellipsoid("BootCuff",(t*.12,.008,.285),(.084,.090,.052),leather,30,18))
        add(shorts_parts,side+"BootUpper",side+"Leg",lambda t=sign:capsule_segment("BootUpper",(t*.12,.009,.28),(t*.12,.022,.125),.077,.086,leather,32,20))
        add(shorts_parts,side+"BootFoot",side+"Foot",lambda t=sign:uv_ellipsoid("BootFoot",(t*.12,.096,.093),(.087,.162,.075),leather,40,24))
        add(shorts_parts,side+"BootSole",side+"Foot",lambda t=sign:uv_ellipsoid("BootSole",(t*.12,.102,.050),(.089,.167,.018),vest,38,16))
    skin_obj = make_skinned_family("Novice_BodySkin", skin_parts, rig, rig_defs,
        ["Head", "Neck", "LeftForeArm", "LeftHand", "RightForeArm", "RightHand", "LeftUpLeg", "LeftLeg", "LeftFoot", "RightUpLeg", "RightLeg", "RightFoot"])
    face_obj = make_skinned_family("Novice_FaceDetails", face_parts, rig, rig_defs, ["Head"])
    shirt_obj = make_skinned_family("Novice_Baseline_TShirt", shirt_parts, rig, rig_defs, ["Spine", "Chest", "LeftArm", "RightArm"])
    shorts_obj = make_skinned_family("Novice_Baseline_Underwear", shorts_parts, rig, rig_defs, ["Hips", "LeftUpLeg", "RightUpLeg", "LeftLeg", "RightLeg", "LeftFoot", "RightFoot"])
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
    scene=bpy.context.scene
    action=bpy.data.actions.new(name)
    rig.animation_data_create()
    rig.animation_data.action=action
    affected=("Head","Neck","Spine","Chest","LeftArm","RightArm","LeftForeArm",
              "RightForeArm","LeftUpLeg","RightUpLeg","LeftLeg","RightLeg","LeftFoot","RightFoot")
    for frame,values in poses:
        scene.frame_set(frame)
        for name_ in affected:
            bone=rig.pose.bones.get(name_)
            bone.rotation_mode="XYZ"
            bone.rotation_euler=values.get(name_,(0.0,0.0,0.0))
            bone.keyframe_insert(data_path="rotation_euler",frame=frame,group=name_)
    action.use_fake_user=True
    scene.frame_start,scene.frame_end=frame_start,frame_end
    rig.animation_data.action=None
    return action


def build_actions(rig):
    # A small-town adventurer. Curious, alert, optimistic; not a heroic parade stance.
    idle_a={"LeftArm":(-.07,0,-.09),"RightArm":(-.05,0,.09),
            "Spine":(.016,0,.012),"Head":(-.025,0,-.06)}
    idle_b={"LeftArm":(-.075,0,-.08),"RightArm":(-.035,0,.08),
            "Spine":(-.01,0,-.010),"Head":(.015,.09,.06)}
    idle_c={"LeftArm":(-.055,0,-.10),"RightArm":(-.065,0,.07),
            "Spine":(.014,0,.025),"Head":(-.01,-.12,.06)}
    # Natural contralateral arm swing, knee recovery and a little shoulder twist.
    walk_a={"LeftArm":(.37,0,-.075),"RightArm":(-.39,0,.075),
            "LeftUpLeg":(-.36,0,0),"RightUpLeg":(.36,0,0),
            "LeftLeg":(.14,0,0),"RightLeg":(.09,0,0),
            "Chest":(.012,0,-.055),"Head":(-.022,0,.018)}
    walk_b={"LeftArm":(-.38,0,-.075),"RightArm":(.37,0,.075),
            "LeftUpLeg":(.36,0,0),"RightUpLeg":(-.36,0,0),
            "LeftLeg":(.09,0,0),"RightLeg":(.14,0,0),
            "Chest":(.012,0,.055),"Head":(-.022,0,-.018)}
    recovery={"LeftArm":(.09,0,-.08),"RightArm":(-.11,0,.08),
              "LeftUpLeg":(-.04,0,0),"RightUpLeg":(.04,0,0),"Chest":(.02,0,0)}
    windup={"RightArm":(.19,-.28,.18),"RightForeArm":(-.36,0,.05),
            "LeftArm":(-.11,0,-.19),"Spine":(.04,0,-.10)}
    hit={"RightArm":(-.78,.12,-.29),"RightForeArm":(-.51,0,0),
         "LeftArm":(-.15,0,-.18),"Chest":(.11,0,.10),"Head":(-.07,0,.06)}
    return [
        key_pose(rig,"Novice|Idle",[(1,idle_a),(15,idle_b),(31,idle_a),(49,idle_c),(73,idle_a)],1,73),
        key_pose(rig,"Novice|Locomotion",[(1,walk_a),(7,recovery),(13,walk_b),(19,recovery),(25,walk_a)],1,25),
        key_pose(rig,"Novice|UnarmedAttack",[(1,idle_a),(5,windup),(10,hit),(15,recovery),(22,idle_a)],1,22)
    ]


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


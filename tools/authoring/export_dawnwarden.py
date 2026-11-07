"""Export the approved Dawnwarden source to Unity FBX with gameplay motion."""
import bpy
import hashlib
import json
import math
import re
import sys
from pathlib import Path
from mathutils import Euler, Matrix, Vector

source, output = (Path(arg) for arg in sys.argv[sys.argv.index("--")+1:])
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(source))
scene = bpy.context.scene
scene.render.fps = 30
rig = next(ob for ob in scene.objects if ob.type == "ARMATURE")
rig.name = "DawnwardenRig"
character = [ob for ob in rig.children if ob.type == "MESH"]
staff = [ob for ob in bpy.data.collections if "dawn lantern" in ob.name.lower() and any(o.type == "MESH" for o in ob.objects)]
assert len(staff) == 1, [(ob.name, len(ob.objects)) for ob in bpy.data.collections]
staff = [ob for ob in staff[0].objects if ob.type == "MESH"]

def pack(objects, name, reduce=False):
    bpy.ops.object.select_all(action="DESELECT")
    copies = []
    for original in objects:
        ob = original.copy()
        ob.data = original.data.copy()
        scene.collection.objects.link(ob)
        mesh_space = (rig.matrix_world.inverted() @ original.matrix_world if reduce
                      else original.matrix_parent_inverse @ original.matrix_basis)
        bpy.context.view_layer.objects.active = ob
        for mod in list(ob.modifiers):
            if mod.type != "ARMATURE":
                bpy.ops.object.modifier_apply(modifier=mod.name)
        if reduce and len(ob.data.polygons) > 80:
            mod = ob.modifiers.new("Game tessellation", "DECIMATE")
            mod.ratio = .25 if original.name.startswith("Continuous hand ") else .77 if original.name == "Dawnwarden face" else .55
            bpy.ops.object.modifier_apply(modifier=mod.name)
        ob.data.transform(mesh_space)
        ob.parent = rig if reduce else None
        ob.matrix_parent_inverse = Matrix.Identity(4)
        ob.matrix_basis = Matrix.Identity(4)
        ob.select_set(True)
        copies.append(ob)
    bpy.context.view_layer.update()
    bpy.context.view_layer.objects.active = copies[0]
    bpy.ops.object.join()
    packed = bpy.context.object
    packed.name = packed.data.name = name
    return packed

def aim(name, direction):
    bone = rig.pose.bones[name]
    rest = bone.bone.matrix_local.to_quaternion()
    if bone.parent:
        rest = bone.parent.matrix.to_quaternion() @ bone.parent.bone.matrix_local.to_quaternion().inverted() @ rest
    bone.rotation_mode = "QUATERNION"
    bone.rotation_quaternion = Vector((0,1,0)).rotation_difference(rest.inverted() @ Vector(direction).normalized())
    bpy.context.view_layer.update()

def wrist(side, direction, dorsal):
    bone = rig.pose.bones["Hand."+side]
    along = Vector(direction).normalized()
    across = along.cross(Vector(dorsal)).normalized()
    frame = Matrix((across, along, across.cross(along))).transposed()
    parent = bone.parent
    rest = parent.matrix.to_quaternion() @ parent.bone.matrix_local.to_quaternion().inverted() @ bone.bone.matrix_local.to_quaternion()
    bone.rotation_quaternion = rest.inverted() @ frame.to_quaternion()
    bpy.context.view_layer.update()

def curl(side, gripping):
    digits = [(46,49,28),(49,48,31),(49,47,29),(32,48,31),(17,25,14)] if gripping else [(3,10,8),(5,12,8),(9,15,9),(15,18,10),(6,12,8)]
    for digit, angles in zip(("Index","Middle","Ring","Little","Thumb"), digits):
        for i, angle in enumerate(angles):
            bone = rig.pose.bones[f"{digit}.{i+1:02}.{side}"]
            twist = .22 if gripping and digit == "Thumb" and i == 0 else 0
            bone.rotation_quaternion = Euler((math.radians(-angle), 0, twist)).to_quaternion()

rig.animation_data_clear()
for bone in rig.pose.bones:
    bone.matrix_basis = Matrix.Identity(4)
    bone.rotation_mode = "QUATERNION"
bpy.context.view_layer.update()
body = pack(character, "Dawnwarden")
lantern = pack(staff, "Dawn Lantern")
staff_matrix = rig.data.bones["Hand.R"].matrix_local @ Matrix.Translation((0,.067,-.031)) @ Vector((0,0,1)).rotation_difference(Vector((-1,0,0))).to_matrix().to_4x4() @ Matrix.Translation((0,0,-.865))
lantern.data.transform(staff_matrix)
lantern.parent = rig
group = lantern.vertex_groups.new(name="Hand.R")
group.add(list(range(len(lantern.data.vertices))), 1, "REPLACE")
lantern.modifiers.new("Hand grip", "ARMATURE").object = rig

# Expose Unity attachment transforms at the actual grip locations.
bpy.ops.object.select_all(action="DESELECT")
rig.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode="EDIT")
for side in ("R", "L"):
    hand = rig.data.edit_bones["Hand."+side]
    socket = rig.data.edit_bones.new("Grip."+side)
    socket.parent = hand
    socket.head = hand.matrix @ Vector((0,.067,-.031))
    socket.tail = socket.head + hand.matrix.to_3x3() @ Vector((-.13 if side=="R" else .13,0,0))
    socket.use_deform = False
bpy.ops.object.mode_set(mode="OBJECT")

def pose(frame, stride=0, lift=0, reach=0, swing=0, brace=0, hit=0, fallen=0, breath=0):
    for bone in rig.pose.bones:
        bone.matrix_basis = Matrix.Identity(4)
    rig.pose.bones["Hips"].location.y = -.02*abs(stride) - .015*brace + .009*breath
    rig.pose.bones["Root"].rotation_quaternion = Euler((fallen*1.52,0,-fallen*.4)).to_quaternion()
    rig.pose.bones["Chest"].rotation_quaternion = Euler((-.12*lift+.13*hit,.22*swing+.05*stride,-.10*swing)).to_quaternion()
    bpy.context.view_layer.update()
    aim("UpperArm.R",(-.41-.26*swing,-.15-.25*lift,-.91+.39*lift))
    aim("Forearm.R",(-.54+.48*swing,-.77,.20+.48*lift))
    wrist("R",(-.69+.38*swing,-.72,.06+.27*lift),(-.70,.68-.6*swing,.06+.54*swing))
    curl("R",True)
    aim("UpperArm.L",(.45+.13*reach,-.24-.35*reach,-.86+.54*reach))
    aim("Forearm.L",(.46-.60*brace,-.83-.10*reach+.12*stride,.13+.70*lift+.36*reach))
    wrist("L",(.77-.64*brace,-.59,.14+.22*lift),(.03,-.20,-1))
    curl("L",brace>.5)
    aim("Head",(.015+.08*swing,-.065-.22*hit,1))
    for side, direction in (("R",1),("L",-1)):
        step = direction*stride
        rig.pose.bones["Thigh."+side].rotation_quaternion = Euler((step*.36,0,0)).to_quaternion()
        rig.pose.bones["Shin."+side].rotation_quaternion = Euler((max(0,-step)*.47,0,0)).to_quaternion()
        rig.pose.bones["Foot."+side].rotation_quaternion = Euler((-step*.20,0,0)).to_quaternion()
        rig.pose.bones["Skirt."+side].rotation_quaternion = Euler((step*.13,0,0)).to_quaternion()
    rig.pose.bones["Cape"].rotation_quaternion = Euler((.10*abs(stride)+.12*lift,.10*stride,0)).to_quaternion()
    rig.pose.bones["FrontDrape"].rotation_quaternion = Euler((-.11*abs(stride),0,.08*stride)).to_quaternion()
    for bone in rig.pose.bones:
        bone.keyframe_insert("rotation_quaternion", frame=frame, group=bone.name)
    rig.pose.bones["Hips"].keyframe_insert("location", frame=frame, group="Hips")

clips = []
def clip(name, first, length, frames, loop=False):
    for i, settings in frames:
        pose(first+i, **settings)
    clips.append(dict(name=name, first=first, last=first+length, loop=loop))

clip("Idle",1,60,[(0,{}),(30,dict(breath=1)),(60,{})],True)
clip("Locomotion",66,24,[(0,dict(stride=1)),(6,{}),(12,dict(stride=-1)),(18,{}),(24,dict(stride=1))],True)
clip("Attack",95,26,[(0,{}),(8,dict(swing=-.7,lift=.35)),(14,dict(swing=1,reach=.2)),(26,{})])
clip("ArmedAttack",126,27,[(0,{}),(8,dict(swing=-.75,lift=.48,brace=.8)),(14,dict(swing=1,brace=1)),(27,{})])
clip("Heal",158,25,[(0,{}),(9,dict(reach=.65,lift=.6)),(14,dict(reach=1,lift=.9)),(25,{})])
clip("Cast",188,25,[(0,{}),(8,dict(lift=1,swing=-.25)),(14,dict(reach=1,lift=.65,swing=.36)),(25,{})])
clip("Guard",218,23,[(0,{}),(9,dict(brace=.72,lift=.42,reach=.8)),(15,dict(brace=.7,lift=.45,reach=.55)),(23,{})])
clip("Revive",246,36,[(0,dict(lift=1,reach=.9)),(18,dict(lift=.85,reach=.75,breath=1)),(36,dict(lift=1,reach=.9))],True)
clip("Hit",287,12,[(0,{}),(5,dict(hit=1,swing=-.3)),(12,{})])
clip("Death",304,28,[(0,{}),(9,dict(hit=1,fallen=.30)),(28,dict(fallen=1))])
scene.frame_start, scene.frame_end = 1, 332
scene.frame_set(1)
# Joining the Blender source must keep face details at head height in the idle pose.
evaluated = body.evaluated_get(bpy.context.evaluated_depsgraph_get())
face_materials = {i for i, mat in enumerate(body.data.materials) if mat.name in ("Honey iris", "Dark iris limbus", "Warm sclera")}
assert face_materials and all(
    (evaluated.matrix_world @ evaluated.data.vertices[index].co).z > 1.4
    for polygon in evaluated.data.polygons if polygon.material_index in face_materials
    for index in polygon.vertices
), "Dawnwarden eye details drifted from their bind pose"

materials = []
images = {}
def texture_for(socket, visited=None):
    visited = set() if visited is None else visited
    for link in socket.links:
        node = link.from_node
        if node in visited:
            continue
        visited.add(node)
        if node.type == "TEX_IMAGE" and node.image:
            image = node.image
            if image.name not in images:
                stem = re.sub(r"[^A-Za-z0-9]+","-",image.name).strip("-")
                filename = stem + ".png"
                if image.packed_file:
                    data = bytes(image.packed_file.data)
                    if data.startswith(b"\x89PNG"):
                        (output/filename).write_bytes(data)
                    elif data.startswith(b"\xff\xd8"):
                        filename=stem+".jpg"
                        (output/filename).write_bytes(data)
                    else:
                        image.save(filepath=str(output/filename))
                else:
                    image.save(filepath=str(output/filename))
                images[image.name]=filename
            return images[image.name]
        for input in node.inputs:
            result = texture_for(input, visited)
            if result:
                return result
    return ""

for material in dict.fromkeys([*body.data.materials,*lantern.data.materials]):
    material.name = re.sub(r"[^A-Za-z0-9]+","-",material.name).strip("-")
    bsdf = next(node for node in material.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
    materials.append(dict(name=material.name, color=list(bsdf.inputs["Base Color"].default_value),
        baseMap=texture_for(bsdf.inputs["Base Color"]), normalMap=texture_for(bsdf.inputs["Normal"]),
        metallic=bsdf.inputs["Metallic"].default_value, smoothness=1-bsdf.inputs["Roughness"].default_value,
        emission=list(bsdf.inputs["Emission Color"].default_value), emissionStrength=bsdf.inputs["Emission Strength"].default_value))

bpy.ops.object.select_all(action="DESELECT")
for ob in (rig, body, lantern):
    ob.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.export_scene.fbx(filepath=str(output/"Dawnwarden.fbx"), use_selection=True,
    axis_forward="-Z", axis_up="Y", apply_unit_scale=True, add_leaf_bones=False,
    use_mesh_modifiers=True, bake_anim=True, bake_anim_use_nla_strips=False,
    bake_anim_use_all_actions=False, bake_anim_simplify_factor=.12)
(output/"DawnwardenImport.json").write_text(json.dumps(dict(
    sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(), materials=materials, clips=clips), indent=2))

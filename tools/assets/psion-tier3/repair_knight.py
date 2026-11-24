import bpy,json
from pathlib import Path
from mathutils import Matrix
import sys
base=Path(sys.argv[sys.argv.index('--')+1])/'PsionicKnight'
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(base/'PsionicKnight-baked.glb'))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.find_armature()]
# glTF carries a posed skeleton that differs from its inverse-bind rest skeleton.
# Bake the visible pose into both the mesh and the armature rest pose before FBX.
for mesh in meshes:
    bpy.context.view_layer.objects.active=mesh
    for modifier in list(mesh.modifiers):
        if modifier.type=='ARMATURE':bpy.ops.object.modifier_apply(modifier=modifier.name)
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True);bpy.context.view_layer.objects.active=arm
bpy.ops.object.mode_set(mode='POSE');bpy.ops.pose.select_all(action='SELECT');bpy.ops.pose.armature_apply(selected=False);bpy.ops.object.mode_set(mode='OBJECT')
for mesh in meshes:
    modifier=mesh.modifiers.new('Coherent rest rig','ARMATURE');modifier.object=arm
for o in meshes+[arm]:
    world=o.matrix_world.copy();o.parent=None;o.matrix_world=world
bpy.ops.object.select_all(action='DESELECT')
for o in meshes+[arm]:o.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
# Legacy export roots are outside the animated humanoid. Move their skin weights
# onto the pelvis, and keep accessory chains parented to that same animated frame.
for mesh in meshes:
    pelvis=mesh.vertex_groups.get('Bip001-Pelvis') or mesh.vertex_groups.new(name='Bip001-Pelvis')
    for old_name in ('Dummy_Root','Bip001'):
        old=mesh.vertex_groups.get(old_name)
        if not old:continue
        for v in mesh.data.vertices:
            weight=sum(g.weight for g in v.groups if g.group==old.index)
            if weight>0:pelvis.add([v.index],weight,'ADD')
        mesh.vertex_groups.remove(old)
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True);bpy.context.view_layer.objects.active=arm
bpy.ops.object.mode_set(mode='EDIT')
hips=arm.data.edit_bones['Bip001-Pelvis']
hips.parent=None
for old_name in ('Bip001','Dummy_Root'):
    bone=arm.data.edit_bones.get(old_name)
    if bone:
        for child in list(bone.children):
            if child!=hips:child.parent=hips
        arm.data.edit_bones.remove(bone)
# The supplied mesh was recentered independently of its skin's bone positions.
# Align the skeleton's pelvis horizontally with the visible mesh before binding.
delta=hips.head.copy();delta.x=-delta.x;delta.y=-delta.y;delta.z=0
positions=[(b,b.head.copy()+delta,b.tail.copy()+delta) for b in arm.data.edit_bones]
for bone,head,tail in positions:bone.head=head;bone.tail=tail
bpy.ops.object.mode_set(mode='OBJECT')
for o in list(bpy.context.scene.objects):
    if o not in meshes+[arm]:bpy.data.objects.remove(o,do_unlink=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=str(base/'PsionicKnight-repaired.fbx'),object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',path_mode='STRIP')
bpy.ops.wm.save_as_mainfile(filepath=str(base/'PsionicKnight-repaired.blend'))
print('KNIGHT_RIG_REPAIRED',[(o.name,tuple(o.location),tuple(o.scale)) for o in meshes+[arm]])



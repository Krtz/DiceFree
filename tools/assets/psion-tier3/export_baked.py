import bpy
from pathlib import Path
import sys
root=Path(sys.argv[sys.argv.index('--')+1])
for key in ('PsionicKnight','MirageMage'):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(root/key/(key+'-baked.glb')))
    for obj in list(bpy.data.objects):
        if obj.type=='MESH' and obj.find_armature() is None:bpy.data.objects.remove(obj,do_unlink=True)
    bpy.ops.export_scene.fbx(filepath=str(root/key/(key+'.fbx')),object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,bake_anim=False,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',path_mode='STRIP')
    bpy.ops.export_scene.gltf(filepath=str(root/key/(key+'-canonical.glb')),export_format='GLB',export_animations=False,export_tangents=True,export_yup=True)

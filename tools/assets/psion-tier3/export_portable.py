import bpy
from pathlib import Path
import sys
root=Path(sys.argv[sys.argv.index('--')+1])
for key in ('PsionicKnight','MirageMage'):
    bpy.ops.wm.open_mainfile(filepath=str(root/key/(key+'.blend')))
    for obj in list(bpy.data.objects):
        if obj.type=='MESH' and obj.hide_render:bpy.data.objects.remove(obj,do_unlink=True)
    bpy.ops.export_scene.gltf(filepath=str(root/key/(key+'.glb')),export_format='GLB',export_animations=False,export_tangents=True,export_yup=True)

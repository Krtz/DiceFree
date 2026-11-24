import bpy,json,sys
from pathlib import Path
from mathutils import Vector
import sys
root=Path(sys.argv[sys.argv.index('--')+1])
for key,source in [('PsionicKnight','warrior-mage/source/Warrior Mage.glb'),('MirageMage','card-magic-girl/source/Card Magic Girl.glb')]:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(root/source))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    if key=='PsionicKnight':
        chosen=next((o for o in meshes if 'GuildWar' in o.name),meshes[-1])
        keep=chosen.find_armature()
        for o in list(bpy.context.scene.objects):
            if o.type=='MESH' and o!=chosen:bpy.data.objects.remove(o,do_unlink=True)
        for o in list(bpy.context.scene.objects):
            if o.type=='ARMATURE' and o!=keep:bpy.data.objects.remove(o,do_unlink=True)
    for helper in list(bpy.context.scene.objects):
        if helper.type=='MESH' and helper.find_armature() is None:bpy.data.objects.remove(helper,do_unlink=True)
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    points=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
    lo=Vector([min(p[i] for p in points) for i in range(3)]);hi=Vector([max(p[i] for p in points) for i in range(3)])
    scale=1.85/(hi.z-lo.z)
    roots=[o for o in bpy.context.scene.objects if o.parent is None]
    anchor=bpy.data.objects.new(key,None);bpy.context.collection.objects.link(anchor)
    for o in roots:o.parent=anchor
    anchor.scale=(scale,)*3;anchor.location=(-(lo.x+hi.x)*.5*scale,-(lo.y+hi.y)*.5*scale,-lo.z*scale)
    folder=root/key;folder.mkdir(exist_ok=True)
    textures={}
    for image in bpy.data.images:
        if image.size[0]<=0:continue
        safe=''.join(c if c.isalnum() or c in '-_' else '_' for c in image.name)
        path=folder/(safe+'.png');image.filepath_raw=str(path);image.file_format='PNG';image.save();textures[image.name]=path.name
    materials=[]
    for mat in bpy.data.materials:
        refs=[n.image.name for n in mat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image] if mat.use_nodes else []
        materials.append({'name':mat.name,'images':[{ 'name':n,'file':textures.get(n)} for n in refs]})
    print('TIER3_MODEL',key,[(o.name,len(o.data.vertices)) for o in meshes],scale)
    bpy.ops.export_scene.fbx(filepath=str(folder/(key+'.fbx')),use_selection=False,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,bake_anim=False,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',path_mode='STRIP')
    bpy.ops.wm.save_as_mainfile(filepath=str(folder/(key+'.blend')))
    (folder/'materials.json').write_text(json.dumps(materials,indent=2))
    (folder/'rig.json').write_text(json.dumps([{'name':o.name,'bones':[b.name for b in o.data.bones]} for o in bpy.context.scene.objects if o.type=='ARMATURE'],indent=2))


import bpy,json,hashlib,shutil
from pathlib import Path
from mathutils import Vector,Matrix
root=Path('T:/TEMP/DiceFree-BerserkerArt');out=root/'Prepared';out.mkdir(exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(root/'Source/Character.fbx'))
# Rebuild a clean bind pose from the evaluated source. Maya control animation
# moves separate mesh objects; deleting those tracks before baking corrupts it.
bpy.context.scene.frame_set(1);bpy.context.view_layer.update()
source_rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE' and len(o.data.bones)>50)
originals=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name not in ['Spear','pCylinder1']]
dg=bpy.context.evaluated_depsgraph_get()
points=[]
for o in originals:
 ev=o.evaluated_get(dg);me=ev.to_mesh()
 points.extend(o.matrix_world@v.co for v in me.vertices)
 ev.to_mesh_clear()
low=Vector([min(v[i] for v in points) for i in range(3)]);high=Vector([max(v[i] for v in points) for i in range(3)])
scale=2.08/(high.z-low.z);offset=Vector(((low.x+high.x)/2,(low.y+high.y)/2,low.z))
transform=Matrix.Scale(scale,4)@Matrix.Translation(-offset)
rig_data=bpy.data.armatures.new('AshscarOrcRig');rig=bpy.data.objects.new('AshscarOrc',rig_data);bpy.context.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
for p in source_rig.pose.bones:
 b=rig_data.edit_bones.new(p.name)
 b.head=transform@(source_rig.matrix_world@p.head);b.tail=transform@(source_rig.matrix_world@p.tail)
 b.align_roll((source_rig.matrix_world.to_3x3()@p.matrix.to_3x3().col[2]).normalized())
for p in source_rig.pose.bones:
 if p.parent:rig_data.edit_bones[p.name].parent=rig_data.edit_bones[p.parent.name]
bpy.ops.object.mode_set(mode='OBJECT')
keep=[]
for o in originals:
 ev=o.evaluated_get(dg);me=bpy.data.meshes.new_from_object(ev,preserve_all_data_layers=True,depsgraph=dg)
 name=o.name
 for v in me.vertices:v.co=transform@(o.matrix_world@v.co)
 new=bpy.data.objects.new('Ashscar_'+name,me);bpy.context.collection.objects.link(new)
 for g in o.vertex_groups:new.vertex_groups.new(name=g.name)
 modifier=new.modifiers.new('Ashscar weighted rig','ARMATURE');modifier.object=rig
 keep.append(new)
for o in list(bpy.context.scene.objects):
 if o not in keep and o!=rig:bpy.data.objects.remove(o,do_unlink=True)
for action in list(bpy.data.actions):bpy.data.actions.remove(action)
bpy.ops.object.select_all(action='DESELECT')
for o in [rig]+keep:o.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.context.view_layer.update()
materials=[]
for m in bpy.data.materials:
 if m.name not in ['lambert1','lambert3','lambert4']:continue
 name={'lambert1':'Orc_Armour','lambert3':'Orc_Body','lambert4':'Orc_Head'}[m.name]
 m.name=name;m.use_nodes=True
 surface=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
 surface.inputs['Metallic'].default_value=0;surface.inputs['Roughness'].default_value=.82
 images=[n for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
 for n in images:
  src=root/'Source'/Path(n.image.filepath).name
  if src.exists():n.image.filepath=str(src);n.image.reload()
  else:raise Exception('Missing texture '+str(src))
  filename=name+'.jpg';shutil.copyfile(src,out/filename);n.image.filepath=str(out/filename);n.image.reload()
  if not surface.inputs['Base Color'].is_linked:m.node_tree.links.new(n.outputs['Color'],surface.inputs['Base Color'])
 materials.append({'name':name,'baseMap':filename})
# Both formats come from this same inspected weighted rig. No source overwrite.
bpy.ops.export_scene.gltf(filepath=str(out/'AshscarOrc.glb'),export_format='GLB',use_selection=True,export_animations=False,export_yup=True)
bpy.ops.export_scene.fbx(filepath=str(out/'AshscarOrc.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',use_armature_deform_only=False)
report={'sourceArchive':'orc-warrior (2).zip','sourceSha256':hashlib.sha256(Path('T:/TEMP/ART_ASSETS/orc-warrior (2).zip').read_bytes()).hexdigest(),'sourceUrl':'https://sketchfab.com/3d-models/orc-warrior-21eaa407bf164cdea21135a1d1b2b8f3','author':'Callan McEntee','license':'CC-BY-4.0','heightMeters':2.08,'boneCount':len(rig.data.bones),'meshes':[o.name for o in keep],'materials':materials,'transforms':['Removed source spear and display plinth','Removed the source idle animation tracks for humanoid retargeting','Baked source control transforms into a clean weighted bind pose','Normalized grounded model to 2.08 metres','Preserved existing skin weights and UVs','Converted textured Lambert materials to rough PBR'],'files':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in out.iterdir() if p.is_file()}}
(out/'source-receipt.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print('BERSERKER_ART_PREPARED '+json.dumps(report),flush=True)
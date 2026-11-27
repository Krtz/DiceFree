import bpy, bmesh, json, math, hashlib, struct, os
from pathlib import Path
from mathutils import Vector, Matrix, Quaternion

ROOT=Path(os.environ.get('DICEFREE_MAGICIAN_ART','T:/TEMP/DiceFree-MagicianArt'));OUT=ROOT/'Prepared';OUT.mkdir(parents=True,exist_ok=True)
SOURCE=Path(os.environ.get('DICEFREE_MAGICIAN_SOURCE','T:/TEMP/DiceFree-FireMageAssetReview/Sources/magician/source/magician.glb'))
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(SOURCE))
source=next(o for o in bpy.context.scene.objects if o.type=='MESH')
source_ids=source.data.attributes.new('_source_vertex','INT','POINT')
for i,data in enumerate(source_ids.data):data.value=i
# Classify disconnected anatomy across UV seams without modifying source topology.
points=[source.matrix_world@v.co for v in source.data.vertices]
parents=list(range(len(points)))
def find(i):
    while parents[i]!=i:parents[i]=parents[parents[i]];i=parents[i]
    return i
def union(a,b):parents[find(a)]=find(b)
coincident={}
for i,p in enumerate(points):
    key=tuple(round(n,5) for n in p)
    if key in coincident:union(i,coincident[key])
    else:coincident[key]=i
for edge in source.data.edges:union(*edge.vertices)
connected={}
for i in range(len(points)):connected.setdefault(find(i),[]).append(i)
components=[{'indices':group} for group in sorted(connected.values(),key=len,reverse=True)]
bpy.context.preferences.filepaths.save_version=0
component_by_vertex={index:component for component,group in enumerate(components) for index in group['indices']}
world=[source.matrix_world@v.co for v in source.data.vertices]
body_indices={i for i,p in enumerate(world) if p.x>-.52}
floor=min(world[i].z for i in body_indices);height=max(world[i].z for i in body_indices)-floor
scale=1.85/height;offset=Vector((0,-.14,floor))
normalize=Matrix.Scale(scale,4)@Matrix.Translation(-offset)

# Keep every UV, silhouette and painted detail from the selected model. The
# disconnected original staff becomes a separate hand-held presentation mesh.
def portion(name,keep,transform):
    mesh=source.data.copy();mesh.name=name
    mesh.transform(transform@source.matrix_world)
    bm=bmesh.new();bm.from_mesh(mesh);bm.verts.ensure_lookup_table()
    bmesh.ops.delete(bm,geom=[v for v in bm.verts if v.index not in keep],context='VERTS');bm.to_mesh(mesh);bm.free();mesh.update()
    ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob);return ob
body=portion('EmberMagician',body_indices,normalize)
staff_grip=Vector((-.705,-.20,.25))
staff=portion('EmberMagicianStaff',set(range(len(world)))-body_indices,Matrix.Scale(scale,4)@Matrix.Translation(-staff_grip))
bpy.data.objects.remove(source,do_unlink=True)

# Extract the original embedded atlas unchanged, then expose it as a conventional
# base-color map for Unity rather than relying on source emissive-only shading.
raw=SOURCE.read_bytes();length=struct.unpack_from('<I',raw,12)[0];doc=json.loads(raw[20:20+length]);start=20+length+8
image=doc['images'][0];view=doc['bufferViews'][image['bufferView']];atlas=raw[start+view.get('byteOffset',0):start+view.get('byteOffset',0)+view['byteLength']]
(OUT/'Magician_Albedo.png').write_bytes(atlas)
material=bpy.data.materials.new('Magician painted atlas');material.use_nodes=True;bsdf=material.node_tree.nodes.get('Principled BSDF')
tex=material.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(OUT/'Magician_Albedo.png'));material.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);bsdf.inputs['Roughness'].default_value=.85
for ob in (body,staff):ob.data.materials.clear();ob.data.materials.append(material)

rig_data=bpy.data.armatures.new('EmberMagicianRig');rig=bpy.data.objects.new('EmberMagicianRig',rig_data);bpy.context.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
segments={}
def bone(name,head,tail,parent=None):
    b=rig_data.edit_bones.new(name);b.head=normalize@Vector(head);b.tail=normalize@Vector(tail)
    if parent:b.parent=rig_data.edit_bones[parent]
    b.use_deform=name!='Root';segments[name]=(Vector(head),Vector(tail));return b
bone('Root',(0,-.14,0),(0,-.14,.12))
bone('Hips',(0,-.14,.43),(0,-.14,.53),'Root')
bone('Spine',(0,-.14,.53),(0,-.14,.65),'Hips')
bone('Chest',(0,-.14,.65),(0,-.14,.81),'Spine')
bone('Neck',(0,-.14,.81),(0,-.14,.91),'Chest')
bone('Head',(0,-.14,.91),(0,-.14,1.17),'Neck')
for side,sign in [('Left',1),('Right',-1)]:
    bone(side+'Shoulder',(0,-.14,.765),(.18*sign,-.14,.765),'Chest')
    bone(side+'UpperArm',(.18*sign,-.14,.765),(.325*sign,-.145,.625),side+'Shoulder')
    bone(side+'LowerArm',(.325*sign,-.145,.625),(.425*sign,-.15,.49),side+'UpperArm')
    bone(side+'Hand',(.425*sign,-.15,.49),(.475*sign,-.15,.44),side+'LowerArm')
    bone(side+'UpperLeg',(.095*sign,-.14,.43),(.105*sign,-.14,.265),'Hips')
    bone(side+'LowerLeg',(.105*sign,-.14,.265),(.105*sign,-.135,.105),side+'UpperLeg')
    bone(side+'Foot',(.105*sign,-.135,.105),(.105*sign,-.235,.055),side+'LowerLeg')
    bone(side+'Toes',(.105*sign,-.235,.055),(.105*sign,-.28,.055),side+'Foot')
bpy.ops.object.mode_set(mode='OBJECT')

groups={name:body.vertex_groups.new(name=name) for name in segments if name!='Root'}
def assign(index,weights):
    total=sum(weights.values())
    for name,value in weights.items():
        if value>0:groups[name].add([index],value/total,'REPLACE')
def closest(point,names):
    scores=[]
    for name in names:
        a,b=segments[name];delta=b-a;t=max(0,min(1,(point-a).dot(delta)/delta.length_squared));distance=(point-(a+t*delta)).length
        scores.append((name,1/(distance+.025)**4))
    scores=sorted(scores,key=lambda p:p[1],reverse=True)[:3];return dict(scores)

# Attach rigid accessories to the torso/head; blend soft sleeves and coat hems
# across their anatomical joints. Coincident UV seams receive identical weights.
inverse=normalize.inverted()
for vertex in body.data.vertices:
    p=inverse@vertex.co;x,y,z=p;side='Left' if x>=0 else 'Right'
    source_index=body.data.attributes['_source_vertex'].data[vertex.index].value
    component=component_by_vertex[source_index]
    arm_floor=.56 if component==1 else .425
    if component in (2,5,16,17) or z>.89:weights={'Head':1}
    elif component in (7,10,11,13):weights={'Hips':1}
    elif abs(x)>.215 and z>arm_floor and z<.90:
        weights=closest(p,[side+'Shoulder',side+'UpperArm',side+'LowerArm',side+'Hand'])
    elif z>.81:
        t=max(0,min(1,(z-.81)/.08));weights={'Neck':1-t,'Head':t}
    elif z>.65:weights={'Chest':1}
    elif z>.51:
        t=(z-.51)/.14;weights={'Spine':1-t,'Chest':t}
    elif z>.42:weights={'Hips':.7,'Spine':.3}
    elif abs(x)>.19 or y>-.025:
        # Loose orange tails follow the thigh without turning into rigid trousers.
        t=max(0,min(.65,(.43-z)*1.8));weights={'Hips':1-t,side+'UpperLeg':t}
    elif z<.17:weights={side+'Foot':1}
    else:weights=closest(p,[side+'UpperLeg',side+'LowerLeg',side+'Foot'])
    assign(vertex.index,weights)

mod=body.modifiers.new('Weighted humanoid deformation','ARMATURE');mod.object=rig;mod.use_deform_preserve_volume=True
body.parent=rig
# Bake a true T bind pose, rather than asking humanoid retargeting to interpret
# the unrigged source's downward arms as horizontal arms.
for side,sign in [('Left',1),('Right',-1)]:
    pb=rig.pose.bones[side+'UpperArm'];direction=(pb.tail-pb.head).normalized();desired=Vector((sign,0,0))
    rotation=direction.rotation_difference(desired);matrix=rotation.to_matrix().to_4x4()@pb.matrix;matrix.translation=pb.head;pb.matrix=matrix
bpy.context.view_layer.update();bpy.context.view_layer.objects.active=body;bpy.ops.object.modifier_apply(modifier=mod.name)
bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='POSE');bpy.ops.pose.armature_apply(selected=False);bpy.ops.object.mode_set(mode='OBJECT')
mod=body.modifiers.new('Weighted humanoid deformation','ARMATURE');mod.object=rig;mod.use_deform_preserve_volume=True

def select(objects):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in objects:ob.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
select([rig,body])
bpy.ops.export_scene.fbx(filepath=str(OUT/'EmberMagician.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',use_armature_deform_only=False)
select([staff]);bpy.ops.export_scene.fbx(filepath=str(OUT/'EmberMagicianStaff.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
staff.parent=rig;staff.parent_type='BONE';staff.parent_bone='RightHand';staff.matrix_world=Matrix.Translation(rig.matrix_world@rig.pose.bones['RightHand'].head)
select([rig,body,staff]);staff.hide_render=False
bpy.ops.export_scene.gltf(filepath=str(OUT/'EmberMagician.glb'),export_format='GLB',use_selection=True,export_animations=False,export_yup=True,export_tangents=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'EmberMagician-rig.blend'))
report={'sourceArchive':'magician.zip','sourceSha256':hashlib.sha256(Path(os.environ.get('DICEFREE_MAGICIAN_ARCHIVE','T:/TEMP/ART_ASSETS/magician.zip')).read_bytes()).hexdigest(),'sourceGlbSha256':hashlib.sha256(raw).hexdigest(),'license':'LicenseRef-UserSupplied','rightsNote':'User-selected local library asset; original author and exact source license remain unverified. User explicitly directed use of this asset.','bodyHeightMeters':1.85,'bones':len(rig.data.bones),'weightedVertices':len(body.data.vertices),'bodyTriangles':sum(len(p.vertices)-2 for p in body.data.polygons),'staffTriangles':sum(len(p.vertices)-2 for p in staff.data.polygons),'transforms':['Preserved original orange painted atlas and silhouette','Separated existing staff from the body','Created weighted humanoid skeleton and baked T bind pose','Normalized body height to1.85m','Converted original emissive-only atlas to conventional base-color material'],'files':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in OUT.iterdir() if p.is_file() and p.suffix in ('.fbx','.glb','.png','.blend')}}
(OUT/'source-receipt.json').write_text(json.dumps(report,indent=2));print('MAGICIAN_PREPARED '+json.dumps(report),flush=True)

"""Original DiceFree quilboar; first-party Novice rig/actions, new creature geometry."""
import bpy,math,json,hashlib,importlib.util
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[3]
spec=importlib.util.spec_from_file_location('novice_source',ROOT/'SourceArt/Characters/Novice/build_novice.py')
n=importlib.util.module_from_spec(spec);spec.loader.exec_module(n)
OUT=ROOT/'Assets/_DiceFree/Art/Creatures/Briarback/Models/Quilboar.fbx'
bpy.ops.wm.read_factory_settings(use_empty=True)
rig,defs=n.make_rig()
materials={
 'Hide':n.material('Briarback_Hide',(.37,.22,.14)),
 'Mane':n.material('Briarback_Mane',(.12,.07,.035)),
 'Leather':n.material('Briarback_Leather',(.22,.15,.08)),
 'Bone':n.material('Briarback_Bone',(.83,.73,.49)),
 'Hoof':n.material('Briarback_Hoof',(.07,.06,.055)),
 'Eye':n.material('Briarback_Eye',(.95,.43,.05))}
parts=[]
def attach(obj,bone,mat):
 obj.data.materials.clear();obj.data.materials.append(materials[mat])
 g=obj.vertex_groups.new(name=bone);g.add(list(range(len(obj.data.vertices))),1,'REPLACE')
 parts.append(obj);return obj
def ball(name,p,scale,bone,mat):
 return attach(n.uv_ellipsoid(name,p,scale,materials[mat],12,8),bone,mat)
def limb(name,a,b,w,d,bone,mat):
 return attach(n.capsule_segment(name,a,b,w,d,materials[mat],10,6),bone,mat)
def spike(name,a,b,r,bone,mat='Bone'):
 a,b=Vector(a),Vector(b);delta=b-a
 bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=r,radius2=0,depth=delta.length,location=(a+b)/2)
 o=bpy.context.object;o.name=name;o.rotation_mode='QUATERNION';o.rotation_quaternion=delta.to_track_quat('Z','Y')
 bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 return attach(o,bone,mat)
ball('Heavy chest',(0,0,1.22),(.31,.25,.34),'Chest','Hide')
ball('Round belly',(0,.035,.99),(.29,.27,.30),'Spine','Hide')
ball('Leather kilt',(0,0,.86),(.31,.24,.18),'Hips','Leather')
ball('Boar skull',(0,.08,1.62),(.26,.24,.27),'Head','Hide')
ball('Long broad snout',(0,.31,1.52),(.22,.25,.15),'Head','Hide')
ball('Nose plate',(0,.505,1.52),(.18,.045,.11),'Head','Mane')
for sign,side in ((-1,'Left'),(1,'Right')):
 ball('Nostril',(sign*.075,.548,1.54),(.033,.016,.025),'Head','Hoof')
 ball('Amber eye',(sign*.195,.26,1.70),(.040,.027,.031),'Head','Eye')
 ball('Heavy brow',(sign*.19,.255,1.745),(.085,.055,.035),'Head','Mane')
 spike('Broad pointed ear',(sign*.19,-.01,1.77),(sign*.36,-.06,1.96),.10,'Head','Hide')
 spike('Lower tusk',(sign*.18,.34,1.40),(sign*.235,.43,1.50),.055,'Head')
 spike('Upturned tusk',(sign*.235,.43,1.50),(sign*.25,.43,1.67),.038,'Head')
 ball('Shoulder',(sign*.235,0,1.33),(.18,.17,.20),side+'Arm','Hide')
 limb('Upper arm',(sign*.22,0,1.32),(sign*.35,0,1.18),.13,.12,side+'Arm','Hide')
 limb('Forearm',(sign*.35,0,1.18),(sign*.49,0,1.04),.11,.10,side+'ForeArm','Hide')
 ball('Wrist binding',(sign*.47,0,1.06),(.10,.105,.07),side+'ForeArm','Leather')
 ball('Fist',(sign*.53,.015,1.02),(.09,.10,.10),side+'Hand','Hide')
 limb('Thigh',(sign*.12,0,.90),(sign*.13,0,.55),.15,.15,side+'UpLeg','Hide')
 limb('Shin',(sign*.13,0,.55),(sign*.13,.01,.18),.10,.105,side+'Leg','Hide')
 ball('Cloven hoof',(sign*.13,.10,.12),(.115,.18,.10),side+'Foot','Hoof')
 for i in (-1,1):
  ball('Hoof toe',(sign*.13+i*.055,.235,.11),(.050,.070,.075),side+'Toes','Hoof')
for i in range(7):
 z=1.08+i*.105
 bone='Chest' if z>1.26 else 'Spine'
 spike('Bristled back quill',(0,-.22,z),(0,-.43,z+.27),.065,bone)
for sign in (-1,1):
 for i in range(3):
  spike('Shoulder bristle',(sign*(.20+i*.05),-.10,1.42),(sign*(.28+i*.08),-.22,1.65),.045,'Chest')
bpy.ops.object.select_all(action='DESELECT')
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join()
body=bpy.context.object;body.name='Quilboar original skinned body'
mod=body.modifiers.new('Quilboar skeleton','ARMATURE');mod.object=rig;body.parent=rig
for p in body.data.polygons:p.use_smooth=False
actions=n.build_actions(rig)
for a in actions:a.name=a.name.replace('Novice|','Quilboar|').replace('UnarmedAttack','Attack')
n.key_pose(rig,'Quilboar|Slam',[(1,{'RightArm':(.15,0,.1)}),(12,{'RightArm':(1.4,-.2,.2),'RightForeArm':(-.6,0,0),'Chest':(-.16,0,-.1)}),(23,{'RightArm':(-.9,.2,-.3),'Chest':(.25,0,.1)}),(34,{})],1,34)
n.key_pose(rig,'Quilboar|Death',[(1,{}),(18,{'Spine':(.9,0,0),'Chest':(.4,0,0),'LeftUpLeg':(-.7,0,0),'RightUpLeg':(-.7,0,0),'LeftLeg':(1.2,0,0),'RightLeg':(1.2,0,0)}),(36,{'Spine':(1.25,0,0),'Chest':(.5,0,0),'LeftUpLeg':(-.9,0,0),'RightUpLeg':(-.9,0,0),'LeftLeg':(1.5,0,0),'RightLeg':(1.5,0,0)})],1,36)
bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);body.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.wm.save_as_mainfile(filepath=str(Path(__file__).with_name('Quilboar.blend')))
OUT.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(OUT),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,primary_bone_axis='Y',secondary_bone_axis='X',axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='AUTO',embed_textures=False)
manifest={'name':'Briarback Quilboar','source':'Original DiceFree geometry; first-party Novice rig and animation code reused','license':'DiceFree original project art','meters':True,'vertices':len(body.data.vertices),'triangles':sum(len(p.vertices)-2 for p in body.data.polygons),'bones':len(rig.data.bones),'actions':[a.name for a in bpy.data.actions],'fbxSha256':hashlib.sha256(OUT.read_bytes()).hexdigest(),'provider':'offline Blender; no paid generation or canonical game-dev package'}
Path(__file__).with_name('asset_manifest.json').write_text(json.dumps(manifest,indent=2))
print('DICEFREE_QUILBOAR_EXPORT_OK',json.dumps(manifest))

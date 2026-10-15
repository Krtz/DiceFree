"""Rig the original first-party Cornberg farmer, villager and herbalist without replacing their appearance.
Inputs: SourceArt/build_cornberg_batch.py function definitions (originals remain untouched).
Outputs: this directory's derived .blend files and Art/World/WalkingVillagers/Models/*.fbx.
Blender 5.2.2 LTS. No external models, textures, providers or credentials.
"""
import ast, bpy, bmesh, math, json, hashlib
from pathlib import Path
from mathutils import Vector
ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
EXPORT = ROOT / "Assets/_DiceFree/Art/World/WalkingVillagers/Models"
EXPORT.mkdir(parents=True, exist_ok=True)
source = ROOT / "SourceArt/build_cornberg_batch.py"
tree = ast.parse(source.read_text(encoding="utf8"))
nodes = [n for n in tree.body if isinstance(n, (ast.Import, ast.ImportFrom, ast.FunctionDef))]
ns = {}
exec(compile(ast.Module(body=nodes, type_ignores=[]), str(source), "exec"), ns)
ns["save"] = lambda *args, **kwargs: None
ROLES = [("Farmer",833),("Villager",838),("Herbalist",840)]
receipt = {"source":str(source.relative_to(ROOT)), "sourceSha256":hashlib.sha256(source.read_bytes()).hexdigest(),
           "license":"First-party DiceFree original art; no third-party input","blender":bpy.app.version_string,"assets":[]}

def rig():
    data=bpy.data.armatures.new("VillageWalkingRig")
    obj=bpy.data.objects.new("VillageArmature",data);bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    defs={"Root":((0,0,0),(0,0,.15),None),
          "Hips":((0,0,.87),(0,0,1.02),"Root"),
          "Spine":((0,0,1.02),(0,0,1.50),"Hips"),
          "Head":((0,0,1.50),(0,0,2.14),"Spine")}
    for side,sign in (("Left",-1),("Right",1)):
        defs[side+"UpperLeg"]=((sign*.18,0,.87),(sign*.18,0,.55),"Hips")
        defs[side+"LowerLeg"]=((sign*.18,0,.55),(sign*.18,0,.24),side+"UpperLeg")
        defs[side+"Foot"]=((sign*.18,0,.24),(sign*.18,-.25,.10),side+"LowerLeg")
        defs[side+"UpperArm"]=((sign*.32,0,1.38),(sign*.40,-.01,1.14),"Spine")
        defs[side+"LowerArm"]=((sign*.40,-.01,1.14),(sign*.47,-.02,.85),side+"UpperArm")
        defs[side+"Hand"]=((sign*.47,-.02,.85),(sign*.47,-.08,.79),side+"LowerArm")
    for name,(head,tail,parent) in defs.items():
        bone=data.edit_bones.new(name);bone.head=head;bone.tail=tail
        if parent:bone.parent=data.edit_bones[parent]
    bpy.ops.object.mode_set(mode="OBJECT")
    return obj

def bind(obj,arm):
    name=obj.name.split(".")[0]
    if name in ("Trouser","Sleeve"):
        bm=bmesh.new();bm.from_mesh(obj.data)
        bmesh.ops.subdivide_edges(bm,edges=list(bm.edges),cuts=2,use_grid_fill=True)
        bm.to_mesh(obj.data);bm.free()
    groups={n:obj.vertex_groups.new(name=n) for n in arm.data.bones.keys()}
    sign="Left" if obj.location.x<0 else "Right"
    for v in obj.data.vertices:
        point=obj.matrix_world@v.co
        weights={"Spine":1.0}
        if name=="Boot":weights={sign+"Foot":1.0}
        elif name=="Trouser":
            w=max(0,min(1,(point.z-.44)/.21));weights={sign+"UpperLeg":w,sign+"LowerLeg":1-w}
        elif name=="Sleeve":
            w=max(0,min(1,(point.z-1.02)/.22));weights={sign+"UpperArm":w,sign+"LowerArm":1-w}
        elif name=="Hand":weights={sign+"Hand":1.0}
        elif name in ("Neck","Head","HairCap","Eye","HatCrown","HatBrim"):weights={"Head":1.0}
        elif name.startswith("Pitchfork"):weights={"RightHand":1.0}
        for bone,w in weights.items():
            if w>0:groups[bone].add([v.index],w,"REPLACE")
    modifier=obj.modifiers.new("VillageWalkingSkin","ARMATURE");modifier.object=arm
    obj.parent=arm
    for poly in obj.data.polygons:poly.use_smooth=True

def action(arm,name,length,walking,role):
    clip=bpy.data.actions.new(name);clip.use_fake_user=True
    arm.animation_data_create();arm.animation_data.action=clip
    for frame in range(length+1):
        phase=frame/length*math.tau
        for bone in arm.pose.bones:
            bone.rotation_mode="XYZ";bone.rotation_euler=(0,0,0);bone.location=(0,0,0)
        arm.pose.bones["Head"].rotation_euler.z=math.sin(phase)*.022
        arm.pose.bones["Spine"].rotation_euler.x=math.sin(phase)*.008
        if walking:
            arm.pose.bones["Hips"].location.z=abs(math.sin(phase*2))*.012
            for side,offset in (("Left",0),("Right",math.pi)):
                s=math.sin(phase+offset)
                arm.pose.bones[side+"UpperLeg"].rotation_euler.x=s*.43
                arm.pose.bones[side+"LowerLeg"].rotation_euler.x=-max(0,s)*.62
                arm.pose.bones[side+"Foot"].rotation_euler.x=-s*.12
                # The farmer keeps the pitchfork upright while the free hand swings.
                amount=.035 if role=="Farmer" and side=="Right" else .18
                arm.pose.bones[side+"UpperArm"].rotation_euler.x=-s*amount
                arm.pose.bones[side+"LowerArm"].rotation_euler.x=-.035
        for bone in arm.pose.bones:
            bone.keyframe_insert("rotation_euler",frame=frame,group=bone.name)
            bone.keyframe_insert("location",frame=frame,group=bone.name)
    arm.animation_data.action=None
    return clip

for role,seed in ROLES:
    for a in list(bpy.data.actions):bpy.data.actions.remove(a)
    ns["npc"]("NPC_"+role,role,seed)
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    arm=rig()
    for obj in meshes:bind(obj,arm)
    ns["join_material"]()
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    for obj in meshes:
        # Joining same-material parts preserves their named bone-weight groups.
        obj.parent=arm
    action(arm,"Idle",60,False,role);action(arm,"Walk",30,True,role)
    bpy.context.scene.render.fps=30;bpy.context.scene.frame_start=0;bpy.context.scene.frame_end=30
    bpy.context.scene.frame_set(0)
    for bone in arm.pose.bones:bone.rotation_euler=(0,0,0);bone.location=(0,0,0)
    blend=HERE/("NPC_"+role+".blend")
    bpy.ops.wm.save_as_mainfile(filepath=str(blend),compress=True)
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True)
    for obj in meshes:obj.select_set(True)
    bpy.context.view_layer.objects.active=arm
    output=EXPORT/("NPC_"+role+".fbx")
    bpy.ops.export_scene.fbx(filepath=str(output),use_selection=True,object_types={"MESH","ARMATURE"},
        axis_forward="-Z",axis_up="Y",apply_unit_scale=True,apply_scale_options="FBX_SCALE_ALL",
        add_leaf_bones=False,use_mesh_modifiers=True,mesh_smooth_type="FACE",
        bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0.0)
    unweighted=sum(1 for o in meshes for v in o.data.vertices if not v.groups)
    if unweighted:raise RuntimeError(str(unweighted)+" unweighted vertices")
    receipt["assets"].append({"role":role,"sourceSeed":seed,"blend":str(blend.relative_to(ROOT)),
        "fbx":str(output.relative_to(ROOT)),"sha256":hashlib.sha256(output.read_bytes()).hexdigest(),
        "meshes":len(meshes),"vertices":sum(len(o.data.vertices) for o in meshes),
        "bones":len(arm.data.bones),"unweightedVertices":unweighted,"actions":["Idle","Walk"]})
    print("DICEFREE_WALKING_VILLAGER_EXPORTED "+role,flush=True)
(HERE/"provenance.json").write_text(json.dumps(receipt,indent=2),encoding="utf8")

# SPDX-License-Identifier: MIT
# Original DiceFree wildlife; CC0-style original authoring, no third-party geometry.
import bpy, math, random, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(r"T:\TEMP\DiceFree-Options")
SRC=ROOT/"SourceArt"/"Creatures"/"Ironpine"
OUT=ROOT/"Assets"/"_DiceFree"/"Art"/"Creatures"/"Ironpine"
SRC.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
rig=None; defs={}; parts=[]; mats={}; species=""
def reset(kind):
    global rig,defs,parts,mats,species
    species=kind
    bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
    for a in list(bpy.data.actions):bpy.data.actions.remove(a)
    for m in list(bpy.data.materials):bpy.data.materials.remove(m)
    defs={};parts=[];mats={}
def mat(name,c):
    if name in mats:return mats[name]
    m=bpy.data.materials.new("Ironpine_"+name);m.diffuse_color=(*c,1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*c,1)
    bs.inputs["Roughness"].default_value=.82
    mats[name]=m;return m
def add(name,v,faces,m,bone,smooth=True):
    mesh=bpy.data.meshes.new(name+"_Geometry");mesh.from_pydata(v,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o)
    mesh.materials.append(m)
    for f in mesh.polygons:f.use_smooth=smooth
    parts.append((o,bone));return o
def ell(name,c,r,m,bone):
    v=[];f=[];u=14;w=10
    for k in range(w+1):
        th=math.pi*k/w
        for j in range(u):
            ph=math.tau*j/u
            v.append((c[0]+math.sin(th)*math.cos(ph)*r[0],
                c[1]+math.sin(th)*math.sin(ph)*r[1],c[2]+math.cos(th)*r[2]))
    for k in range(w):
        for j in range(u):
            a=k*u+j;b=k*u+(j+1)%u
            f.append((a,b,b+u,a+u))
    return add(name,v,f,m,bone)
def loft(name,rows,m,bone,n=16):
    v=[];f=[]
    for y,z,wx,h in rows:
        for i in range(n):
            angle=math.tau*i/n
            v.append((wx*math.cos(angle),y,z+h*math.sin(angle)))
    for k in range(len(rows)-1):
        for i in range(n):
            a=k*n+i;b=k*n+(i+1)%n
            f.append((a,b,b+n,a+n))
    f.append(tuple(reversed(range(n))))
    f.append(tuple((len(rows)-1)*n+i for i in range(n)))
    return add(name,v,f,m,bone)
def tapered(name,points,widths,m,bone,n=9):
    v=[];f=[]
    for k,p in enumerate(points):
        direction=Vector(points[min(k+1,len(points)-1)])-Vector(points[max(k-1,0)])
        direction.normalize()
        side=direction.cross(Vector((1,0,0)))
        if side.length<.1:side=direction.cross(Vector((0,1,0)))
        side.normalize();other=direction.cross(side).normalized()
        for i in range(n):
            a=math.tau*i/n;q=Vector(p)+widths[k]*(side*math.cos(a)+other*math.sin(a))
            v.append(tuple(q))
    for k in range(len(points)-1):
        for i in range(n):
            a=k*n+i;b=k*n+(i+1)%n
            f.append((a,b,b+n,a+n))
    f.append(tuple(reversed(range(n))))
    f.append(tuple((len(points)-1)*n+i for i in range(n)))
    return add(name,v,f,m,bone)
def point(name,start,finish,r,m,bone,n=5):
    base,tip=Vector(start),Vector(finish);direction=(tip-base).normalized()
    one=direction.cross(Vector((1,0,0)))
    if one.length<.1:one=direction.cross(Vector((0,1,0)))
    one.normalize();two=direction.cross(one).normalized()
    v=[tuple(base+r*(one*math.cos(math.tau*i/n)+two*math.sin(math.tau*i/n))) for i in range(n)]
    v.append(tuple(tip))
    return add(name,v,[tuple(reversed(range(n)))]+[(i,(i+1)%n,n) for i in range(n)],m,bone,False)
def setup_rig(kind):
    global rig,defs
    wolf=kind!="Quillrat"
    defs={
      "Root":((0,0,.10),(0,0,.30),None),
      "Torso":((0,-.13,.78 if wolf else .40),(0,.26,.80 if wolf else .42),"Root"),
      "Neck":((0,.34,.87 if wolf else .43),(0,.62,1.06 if wolf else .45),"Torso"),
      "Head":((0,.65,1.10 if wolf else .46),(0,.97,1.09 if wolf else .47),"Neck"),
      "Jaw":((0,.94,.90 if wolf else .34),(0,1.18,.89 if wolf else .33),"Head"),
      "Tail":((0,-.66,.78 if wolf else .35),(0,-1,.89 if wolf else .26),"Torso"),
      "TailTip":((0,-1,.89 if wolf else .26),(0,-1.23,1.03 if wolf else .22),"Tail"),
    }
    for pair in ("Front","Hind"):
        for side,sg in (("L",-1),("R",1)):
            front=pair=="Front"
            y=(.45 if front else -.54) if wolf else (.29 if front else -.39)
            x=(.33 if wolf else .27)*sg
            upper=(x,y,.79 if wolf and front else .71 if wolf else .34)
            knee=(x,y+(.05 if front else -.10),.43 if wolf else .18)
            foot=(x,y+.17,.11 if wolf else .06)
            defs[pair+side+"Upper"]=(upper,knee,"Torso")
            defs[pair+side+"Lower"]=(knee,foot,pair+side+"Upper")
    data=bpy.data.armatures.new("Ironpine"+kind+"Rig")
    rig=bpy.data.objects.new("Ironpine"+kind+"Armature",data)
    bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;rig.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    for key,(head,tail,parent) in defs.items():
        bone=data.edit_bones.new(key);bone.head=head;bone.tail=tail
        if parent:bone.parent=data.edit_bones[parent]
        bone.use_connect=False
    bpy.ops.object.mode_set(mode="OBJECT")
def wolf_mesh():
    coat=mat("WolfCoat",(.46,.49,.51))
    shadow=mat("WolfShadow",(.23,.27,.30))
    face=mat("WolfFace",(.51,.54,.57))
    white=mat("WolfChin",(.75,.72,.62))
    eye=mat("GoldenEye",(.94,.71,.29))
    black=mat("NoseAndPupil",(.16,.16,.17))
    inner=mat("InnerEar",(.47,.29,.28))
    loft("Strong almond barrel",[(-.75,.77,.07,.11),(-.62,.77,.32,.29),
        (-.36,.80,.38,.33),(.07,.79,.34,.32),(.36,.84,.40,.39),
        (.54,.94,.34,.36),(.69,1.0,.16,.21)],coat,"Torso")
    loft("Pale throat and breast",[(.33,.71,.10,.05),(.48,.76,.29,.25),
        (.61,.88,.27,.35),(.72,.98,.12,.19)],white,"Neck")
    loft("Strong mane",[(.31,.94,.1,.08),(.52,1.0,.30,.32),(.73,1.05,.25,.26),
        (.84,1.11,.07,.06)],coat,"Neck")
    loft("Long wedge head",[(.60,1.10,.08,.1),(.74,1.12,.28,.28),
        (.94,1.09,.28,.22),(1.1,1.01,.20,.17),(1.24,.98,.07,.08)],face,"Head")
    loft("Long pale jaw",[(.86,.90,.12,.09),(1.1,.91,.19,.09),(1.26,.94,.10,.07)],white,"Jaw")
    ell("Nose leather",(0,1.27,1.02),(.14,.11,.085),black,"Head")
    for sg,name in ((-1,"L"),(1,"R")):
        ell("Expressive eye "+name,(sg*.237,.927,1.238),(.047,.028,.060),eye,"Head")
        ell("Pupil "+name,(sg*.246,.947,1.247),(.022,.015,.029),black,"Head")
        point("Tall alert ear "+name,(sg*.22,.70,1.30),(sg*.31,.67,1.69),.19,coat,"Head")
        point("Ear pink"+name,(sg*.22,.78,1.34),(sg*.31,.77,1.58),.10,inner,"Head")
        for pair in ("Front","Hind"):
            upper,knee,_=defs[pair+name+"Upper"]
            _,foot,_=defs[pair+name+"Lower"]
            tapered(pair+name+" strong thigh",[upper,tuple(Vector(upper).lerp(Vector(knee),.5)),knee],
                [.18 if pair=="Front" else .26,.16,.12],coat,pair+name+"Upper")
            tapered(pair+name+" straight shin",[knee,tuple(Vector(knee).lerp(Vector(foot),.57)),foot],
                [.12,.105,.074],shadow,pair+name+"Lower")
            ell(pair+name+" broad paw",(foot[0],foot[1]+.12,.10),(.15,.22,.10),
                shadow,pair+name+"Lower")
            for toe in (-1,0,1):
                point("Dark claw",(foot[0]+toe*.074,foot[1]+.27,.085),
                  (foot[0]+toe*.074,foot[1]+.33,.045),.035,black,pair+name+"Lower")
    tapered("Heavy tail tuft",[(0,-.65,.79),(0,-.90,.86),(0,-1.08,.97)],
        [.17,.25,.18],coat,"Tail")
    tapered("Wolf tail tip",[(0,-1.06,.94),(0,-1.27,1.09),(0,-1.34,1.11)],
        [.18,.12,.008],shadow,"TailTip")
    for i in range(36):
        sg=(-1 if i%2 else 1);y=-.55+(i%9)*.13
        x=sg*(.25+i%3*.033);z=.91+(.04 if i%4 else .11)
        point("Directional shoulder fur",(x,y,z),(x+sg*.09,y-.06,z+.095),
            .069,shadow if i%7==0 else coat,"Torso")
    if species=="AlphaWolf":
        mane=mat("AlphaMane",(.73,.72,.61))
        for i in range(36):
            theta=math.tau*i/36;sg=math.cos(theta);height=math.sin(theta)
            base=(sg*.28,.47,.97+height*.28)
            tip=(sg*.42,.27,.94+height*.43)
            point("Alpha crown ruff",base,tip,.135,mane,"Neck")
        stripe=mat("AlphaScar",(.85,.81,.72))
        tapered("Old pale cheek scar",[(.19,.91,1.31),(.21,.96,1.20)],
            [.023,.005],stripe,"Head",5)
    for i in range(14):
        sg=(-1 if i%2 else 1);y=.38+(i%7)*.058
        point("Cream ruff feather",(sg*.17,y,.75),(sg*.20,y-.10,.60),
              .09,white,"Neck")
def quillrat_mesh():
    coat=mat("RatCoat",(.38,.54,.28));shadow=mat("RatShadow",(.24,.36,.22))
    cream=mat("RatBelly",(.77,.79,.46));face=mat("RatFace",(.51,.61,.35))
    quill=mat("RatGoldQuill",(.89,.75,.34));band=mat("RatQuillBand",(.39,.32,.22))
    black=mat("RatNose",(.26,.19,.17));eye=mat("RatEye",(.92,.86,.28))
    inner=mat("RatEar",(.66,.38,.36))
    loft("Quillrat deep round body",[(-.69,.40,.05,.08),(-.54,.42,.28,.29),
        (-.23,.43,.45,.39),(.10,.43,.46,.37),(.34,.41,.32,.28),
        (.49,.40,.15,.14)],coat,"Torso")
    ell("Soft belly",(0,.04,.24),(.33,.42,.19),cream,"Torso")
    loft("Wide cheeked head",[(.33,.41,.10,.11),(.48,.44,.28,.27),
       (.64,.40,.28,.23),(.73,.34,.16,.12)],face,"Head")
    loft("Taper snout",[(.66,.34,.18,.11),(.81,.31,.17,.09),(.95,.29,.065,.04)],cream,"Jaw")
    ell("Pink nose",(0,.965,.33),(.095,.07,.062),black,"Head")
    for sg,name in ((-1,"L"),(1,"R")):
        ell("Beady eye "+name,(sg*.242,.66,.51),(.068,.047,.071),eye,"Head")
        ell("Little pupil "+name,(sg*.262,.69,.52),(.028,.02,.03),black,"Head")
        ell("Round ear "+name,(sg*.27,.42,.67),(.15,.085,.18),face,"Head")
        ell("Inner ear "+name,(sg*.28,.465,.69),(.092,.025,.125),inner,"Head")
        for pair in ("Front","Hind"):
            top,knee,_=defs[pair+name+"Upper"];_,foot,_=defs[pair+name+"Lower"]
            tapered(pair+name+" stout leg",[top,knee],[.13,.09],coat,pair+name+"Upper")
            tapered(pair+name+" slim leg",[knee,foot],[.09,.065],shadow,pair+name+"Lower")
            ell(pair+name+" furry foot",(foot[0],foot[1]+.10,.056),
                 (.12,.14,.055),coat,pair+name+"Lower")
        for v in range(3):
            tapered("Curved whisker",[(sg*.10,.83,.31),(sg*(.25+v*.04),.90+v*.015,.30+v*.045)],
                [.008,.002],cream,"Head",5)
    random.seed(19026)
    for row in range(11):
        y=-.57+row*.088
        shape=math.sin(math.pi*(row+.5)/11)
        for col in range(9):
            lateral=(col-4)/4
            x=lateral*.40*shape
            z=.45+.35*math.sqrt(max(.05,1-lateral*lateral))*shape
            length=(.23+.29*shape)*(1-.22*abs(lateral))
            point("Raised gold quill",(x,y,z),
                (x+lateral*.16,y-.12-.13*shape,z+length),
                .040 if col%3 else .055,
                band if (row+col)%6==0 else quill,"Torso")
    tapered("Flexible skinny tail",[(0,-.57,.32),(0,-.78,.26),(0,-.94,.21)],
        [.11,.075,.04],shadow,"Tail")
    point("Quillrat tail tip",(0,-.94,.21),(0,-1.01,.18),.04,coat,"TailTip")
def skin():
    meshes=[]
    for bone in defs:
        items=[o for o,label in parts if label==bone]
        if not items:continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in items:o.select_set(True)
        bpy.context.view_layer.objects.active=items[0]
        bpy.ops.object.join()
        combined=bpy.context.object
        combined.name="Ironpine_"+species+"_"+bone+"_Mesh"
        vg=combined.vertex_groups.new(name=bone)
        vg.add(list(range(len(combined.data.vertices))),1.,'REPLACE')
        modifier=combined.modifiers.new("IronpineSkin","ARMATURE")
        modifier.object=rig;combined.parent=rig
        meshes.append(combined)
    return meshes
def animation(name,frames):
    rig.animation_data_create()
    action=bpy.data.actions.new(species+"|"+name)
    rig.animation_data.action=action
    for frame,pose in frames:
        for bone in rig.pose.bones:
            bone.rotation_mode="XYZ"
            bone.rotation_euler=pose.get(bone.name,(0,0,0))
            bone.keyframe_insert(data_path="rotation_euler",frame=frame,group=bone.name)
    action.use_fake_user=True
    rig.animation_data.action=None
    return action
def gait(s):
    return {"FrontLUpper":(s,0,0),"FrontRUpper":(-s,0,0),
        "HindLUpper":(-s*.7,0,0),"HindRUpper":(s*.7,0,0),
        "FrontLLower":(-abs(s)*.50,0,0),"FrontRLower":(-abs(s)*.50,0,0),
        "HindLLower":(abs(s)*.35,0,0),"HindRLower":(abs(s)*.35,0,0),
        "Head":(.032,0,0),"Tail":(0,0,-s*.16)}
def build_actions(kind):
    wolf=kind!="Quillrat"
    return [
        animation("Idle",[(1,{}),(22,{"Torso":(.027,0,0),
         "Head":(-.047,0,.05),"Tail":(0,0,.16)}),(43,{})]),
        animation("Walk",[(1,gait(.32)),(7,gait(0)),(13,gait(-.32)),
          (19,gait(0)),(25,gait(.32))]),
        animation("Run",[(1,gait(.59)),(5,{"Torso":(.10,0,0),**gait(0)}),
          (9,gait(-.59)),(13,{"Torso":(-.08,0,0),**gait(0)}),(17,gait(.59))]),
        animation("Bite",[(1,{}),(5,{"Neck":(-.21,0,0),
          "Head":(.21,0,0),"Jaw":(.32,0,0)}),
          (10,{"Neck":(.26,0,0),"Head":(-.14,0,0),"Jaw":(-.37,0,0)}),
          (17,{})]),
        animation("Howl" if wolf else "QuillShot",[(1,{}),
          (8,{"Neck":(-.40,0,0) if wolf else (.20,0,0),
             "Head":(-.48,0,0) if wolf else (-.25,0,0),
             "Jaw":(.23,0,0)}),
          (21,{"Head":(-.56,0,.1) if wolf else (.25,0,0),"Tail":(0,.1,0)}),
          (32,{})]),
        animation("Pounce" if wolf else "Scurry",[(1,{}),
          (5,{"Torso":(.15,0,0),**gait(.32)}),
          (9,{"Torso":(-.24,0,0),**gait(-.44)}),(17,{})]),
        animation("Sniff" if wolf else "Forage",[(1,{}),
          (10,{"Neck":(.30,0,0),"Head":(.33,0,.07)}),
          (22,{"Neck":(.32,0,0),"Head":(.39,0,-.12)}),(34,{})]),
        animation("Scratch",[(1,{}),(7,{"Head":(.05,0,.1),
          "HindLUpper":(.47,0,0),"HindLLower":(-.42,0,0)}),
          (11,{"HindLLower":(-.12,0,0)}),
          (16,{"HindLLower":(-.42,0,0)}),(25,{})]),
        animation("Hurt",[(1,{}),(5,{"Torso":(-.18,0,0),
            "Head":(.21,0,0)}),(17,{})]),
        animation("Death",[(1,{}),(9,{"Torso":(.25,0,.29),
            "Head":(.20,0,.24)}),
          (17,{"Torso":(.50,0,1.22),"Head":(.30,0,.16),
            "Tail":(0,.14,0)}),(38,{"Torso":(.50,0,1.22),
            "Head":(.30,0,.16)})]),
    ]
def build(kind):
    reset(kind)
    setup_rig(kind)
    if kind!="Quillrat":wolf_mesh()
    else:quillrat_mesh()
    meshes=skin()
    clips=build_actions(kind)
    for bone in rig.pose.bones:
        bone.rotation_euler=(0,0,0)
    bpy.context.scene.frame_set(0)
    bpy.context.view_layer.update()
    bpy.ops.wm.save_as_mainfile(filepath=str(SRC/(kind+".blend")),compress=True)
    bpy.ops.object.select_all(action="DESELECT")
    rig.select_set(True)
    for m in meshes:m.select_set(True)
    bpy.context.view_layer.objects.active=rig
    fbx=OUT/(kind+".fbx")
    bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,
      object_types={"ARMATURE","MESH"},axis_forward="-Z",axis_up="Y",
      add_leaf_bones=False,use_mesh_modifiers=True,
      primary_bone_axis="Y",secondary_bone_axis="X",
      bake_anim=True,bake_anim_use_all_actions=True,
      bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,
      bake_anim_simplify_factor=0.)
    verts=sum(len(o.data.vertices) for o in meshes)
    tri=sum(sum(max(1,len(poly.vertices)-2) for poly in o.data.polygons) for o in meshes)
    print("IRONPINE_ORIGINAL_MODEL",kind,"verts",verts,"tri",tri,
         "bones",len(defs),"animations",[a.name for a in clips],flush=True)
    return dict(species=kind,vertices=verts,triangles=tri,bones=list(defs.keys()),
        animation_actions=[a.name for a in clips],
        source=str(SRC/(kind+".blend")),export=str(fbx))
if __name__=="__main__":
    bpy.context.scene.unit_settings.system="METRIC"
    manifest=[build(k) for k in ("Wolf","Quillrat","AlphaWolf")]
    (SRC/"asset_manifest.json").write_text(json.dumps(manifest,indent=2),encoding="utf-8")

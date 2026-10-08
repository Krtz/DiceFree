# Reproducible Blender authoring for DiceFree Cornberg art (Blender 5.2 LTS)
import bpy, math, random, os, json
from mathutils import Vector
from pathlib import Path
ROOT=Path(r"T:\TEMP\DiceFree-Options")
SOURCE=ROOT/"SourceArt"/"World"/"CornbergBatch"
EXPORT=ROOT/"Assets"/"_DiceFree"/"Art"/"World"/"CornbergBatch"
SOURCE.mkdir(parents=True,exist_ok=True); EXPORT.mkdir(parents=True,exist_ok=True)
random.seed(1506)
MANIFEST=[]

def clear():
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    for item in list(bpy.data.materials): bpy.data.materials.remove(item)
def mat(name,rgb,alpha=1.,rough=.72,metal=0,emission=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*rgb,alpha); m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs['Base Color'].default_value=(*rgb,alpha)
    bs.inputs['Roughness'].default_value=rough; bs.inputs['Metallic'].default_value=metal
    bs.inputs['Alpha'].default_value=alpha
    bs.inputs['Transmission Weight'].default_value=0.34 if alpha<.99 else 0.
    if emission:
        bs.inputs['Emission Color'].default_value=(*rgb,1)
        bs.inputs['Emission Strength'].default_value=emission
    return m
def uv(name,loc,scale,material,segments=12,rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,location=loc)
    o=bpy.context.object;o.name=name; o.scale=scale; o.data.materials.append(material); return o
def cube(name,loc,scale,m):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object;o.name=name;o.dimensions=scale; o.data.materials.append(m)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True);return o
def cylinder(name,a,b,r1,r2,m,verts=9):
    a,b=Vector(a),Vector(b);delta=b-a; midpoint=(a+b)/2
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r1,radius2=r2,depth=delta.length,location=midpoint)
    o=bpy.context.object;o.name=name; o.rotation_euler=delta.to_track_quat('Z','Y').to_euler()
    o.data.materials.append(m); return o
def join_material():
    # Combine static meshes of one material to limit Unity renderer count.
    groups={}
    for o in list(bpy.context.scene.objects):
        if o.type=="MESH" and o.data.materials:
            if o.name.startswith("Door"): continue  # required as individual gate objects
            groups.setdefault(o.data.materials[0].name,[]).append(o)
    for key, items in groups.items():
        if len(items)<2: continue
        bpy.ops.object.select_all(action='DESELECT')
        for o in items:o.select_set(True)
        bpy.context.view_layer.objects.active=items[0]
        bpy.ops.object.join()
        items[0].name=key.replace("DF_","")+"_mesh"
def save(kind,name,scale=None,info=None):
    join_material()
    bpy.ops.object.select_all(action='SELECT')
    file=SOURCE/(name+".blend")
    bpy.ops.wm.save_as_mainfile(filepath=str(file),compress=True)
    fbxfolder=EXPORT/kind;fbxfolder.mkdir(parents=True,exist_ok=True)
    output=fbxfolder/(name+".fbx")
    bpy.ops.export_scene.fbx(filepath=str(output),use_selection=False,
        axis_forward="-Z",axis_up="Y",object_types={'MESH'},apply_unit_scale=True,
        add_leaf_bones=False,use_mesh_modifiers=True,bake_anim=False,
        mesh_smooth_type='FACE')
    stat={"type":kind,"name":name,"blend":str(file.relative_to(ROOT)).replace("\\","/"),
          "fbx":str(output.relative_to(ROOT)).replace("\\","/"),"objects":len(bpy.context.scene.objects),
          "scale_m":scale,"notes":info}
    MANIFEST.append(stat);print("EXPORTED",name,len(bpy.context.scene.objects),flush=True)

def tree(name,shape,seed):
    clear();r=random.Random(seed)
    bark=mat("DF_Bark",(0.24,0.15,0.09))
    dark=mat("DF_BarkDeep",(0.13,0.10,0.08))
    leaf=mat("DF_Foliage",(0.17,0.42,0.19))
    accent=mat("DF_LeafAccent",(0.25,0.57,0.22))
    scar=mat("DF_Moss",(0.38,0.48,0.19))
    trunkheight={"Pine":4.5,"Spruce":3.8,"Oak":3.1,"Birch":4.3,"Maple":3.0,"Willow":3.2,"Twisted":3.4,"Aspen":5.2,"Cypress":4.4}[shape]
    cylinder("Trunk",(0,0,0),(r.uniform(-.18,.18),0,trunkheight),.36 if shape!="Aspen" else .21,.13,bark,11)
    for j in range(5):
        a=j*math.tau/5
        cylinder("Root",(0,0,.38),(math.cos(a)*(.60+r.random()*.3),math.sin(a)*(.6+r.random()*.3),.04),.15,.04,bark)
    if shape in ("Pine","Spruce","Cypress"):
        levels=8 if shape!="Cypress" else 10
        for i in range(levels):
            z=.65+i*.48
            length=(1.5 if shape=="Pine" else 1.25)*(1-i/levels*.84)
            count=7 if shape!="Cypress" else 5
            for k in range(count):
                a=k*math.tau/count+i*.36
                endpoint=(math.cos(a)*length,math.sin(a)*length,z-.2)
                cylinder("EvergreenBough",(.05,0,z),endpoint,.075,.022,bark,6)
                uv("EvergreenNeedles",endpoint,(length*.37,length*.34,.30 if shape!="Cypress" else .49),
                   accent if (i+k)%4==0 else leaf,8,5)
    elif shape=="Willow":
        for i in range(7):
            a=i*math.tau/7
            x=math.cos(a)*2.1;y=math.sin(a)*1.8
            cylinder("WillowBranch",(0,0,2.6),(x,y,4.1),.15,.07,bark)
            for j in range(5):
                dx=x+(j-2)*.19
                uv("WillowDrape",(dx,y,2.55+j*.14),(.34,.29,1.03),accent if j%3==0 else leaf,9,6)
    else:
        crowns={"Oak":(2.2,2.3,1.30),"Birch":(1.05,1.4,.75),"Maple":(2.1,2.0,1.1),
                "Twisted":(1.5,1.2,.68),"Aspen":(.82,1.10,.65)}[shape]
        if shape=="Birch":
            bark.diffuse_color=(.82,.79,.68,1)
            bark.node_tree.nodes.get("Principled BSDF").inputs['Base Color'].default_value=(.83,.81,.73,1)
            for j in range(11):
                z=.4+j*.37
                cube("BirchBarkMark",(r.uniform(-.16,.16),-.19,z),(.22,.05,.07),dark)
        for i in range(8 if shape!="Aspen" else 6):
            a=i*math.tau/8
            zz=trunkheight-.7+(i%3)*.32
            ex=math.cos(a)*(1.5 if shape!="Birch" else .85)
            ey=math.sin(a)*(1.5 if shape!="Birch" else .85)
            cylinder("CanopyBough",(0,0,zz),(ex,ey,trunkheight+.8),.15,.07,bark)
            if shape=="Twisted" and i%2==0:continue
            uv("Crown",(ex,ey,trunkheight+.8+(i%2)*.18),
               (crowns[0]*.57,crowns[1]*.52,crowns[2]),
               accent if i%3==0 else leaf,10,7)
        uv("TopCrown",(0,0,trunkheight+1.55),crowns,leaf,14,9)
    if shape=="Twisted":
        for i in range(4):
            a=i*math.tau/4
            cylinder("BareSpike",(math.cos(a)*.3,math.sin(a)*.3,trunkheight),
                    (math.cos(a)*2,math.sin(a)*2,trunkheight+2.3),.10,.008,bark)
    save("Trees",name,scale=trunkheight,info="Recolor Foliage/LeafAccent materials in Unity; shared bark/filler geometry.")

def npc(name,role,seed):
    clear();r=random.Random(seed)
    skin=mat("DF_Skin",(0.74,0.53,0.40))
    skinhi=mat("DF_SkinHighlight",(0.87,.68,.52))
    hair=mat("DF_Hair",(0.21,.13,.08) if seed%2 else (.39,.27,.13))
    shirt=mat("DF_ClothPrimary",{"Farmer":(.36,.45,.24),"Merchant":(.60,.31,.23),
        "Banker":(.16,.28,.43),"Alchemist":(.25,.43,.36),"Blacksmith":(.31,.29,.29),
        "Villager":(.53,.49,.31),"Guard":(.39,.43,.48),"Herbalist":(.35,.54,.33)}[role])
    secondary=mat("DF_ClothTrim",(.83,.75,.56))
    leather=mat("DF_Leather",(.24,.14,.09))
    metal=mat("DF_Metal",(.55,.57,.56),metal=.56)
    potion=mat("DF_Potion",(.22,.8,.56),alpha=.65,emission=.28)
    # stylized but recognisably 3d and world-scale (1.8m adults)
    for side in (-1,1):
        uv("Boot",(.19*side,-.05,.13),(.19,.30,.17),leather)
        cylinder("Trouser",(.18*side,0,.28),(.18*side,0,.87),.14,.16,shirt)
    uv("Torso",(0,0,1.12),(.42,.27,.49),shirt)
    uv("Neck",(0,0,1.54),(.15,.15,.15),skin)
    uv("Head",(0,-.02,1.73),(.26,.22,.28),skin,14,9)
    uv("HairCap",(0,.02,1.94),(.265,.23,.095),hair)
    for side in (-1,1):
        uv("Eye",(.095*side,-.221,1.76),(.045,.035,.063),hair)
        cylinder("Sleeve",(.32*side,0,1.38),(.46*side,-.015,.89),.14,.09,shirt)
        uv("Hand",(.47*side,-.02,.85),(.11,.10,.13),skin)
    if role in ("Farmer","Villager","Herbalist"):
        cylinder("HatCrown",(0,0,1.98),(0,0,2.14),.20,.15,leather)
        cylinder("HatBrim",(0,0,1.99),(0,0,2.00),.40,.40,secondary)
    if role=="Farmer":
        cylinder("PitchforkHandle",(.60,-.05,.10),(.60,-.05,2.05),.035,.035,leather)
        for i in range(3):
            cylinder("PitchforkTine",(.49+i*.11,-.06,1.9),(.49+i*.11,-.06,2.22),.026,.016,metal)
    elif role=="Merchant":
        cube("TradeSatchel",(-.43,-.20,.85),(.38,.20,.47),leather)
        uv("CoinToken",(-.43,-.31,.94),(.09,.035,.09),secondary)
        uv("Bundle",(0,-.33,1.08),(.19,.1,.17),secondary)
    elif role=="Banker":
        cube("Ledger",(0,-.39,1.12),(.40,.08,.30),leather)
        uv("LedgerEmblem",(0,-.45,1.13),(.09,.015,.09),metal)
        for i in range(3):uv("Keys",(.49,.00,.66+i*.12),(.042,.025,.07),metal)
    elif role=="Alchemist":
        for i,clr in enumerate([potion,secondary]):
            uv("Vial",(-.40+i*.20,-.17,.78),(.09,.09,.17),clr)
            cylinder("VialNeck",(-.40+i*.20,-.17,.95),(-.40+i*.20,-.17,1.02),.045,.045,metal)
        cube("Apron",(0,-.29,1.04),(.44,.075,.62),secondary)
    elif role=="Blacksmith":
        cube("ForgeApron",(0,-.31,1.00),(.49,.09,.80),leather)
        cylinder("HammerHandle",(.58,0,.60),(.59,0,1.28),.052,.052,leather)
        cube("HammerHead",(.59,0,1.33),(.38,.16,.17),metal)
    elif role=="Guard":
        uv("Helmet",(0,0,1.93),(.29,.26,.20),metal)
        cube("Shield",(-.53,-.18,1.02),(.36,.11,.51),metal)
    elif role=="Herbalist":
        uv("HerbSatchel",(.42,-.21,.75),(.18,.13,.25),leather)
        for i in range(4):uv("Plant",(.42,-.21,.98+i*.05),(.13,.10,.10),shirt)
    save("NPCs",name,scale=2.05,info="Generic stylized static 3D NPC role kit; currently not rigged. Preserve unique Tier-2 couple.")

def slime(name,size,seed,theme):
    clear(); r=random.Random(seed)
    c={"crop":(.30,.78,.29),"meadow":(.29,.84,.47),"road":(.32,.72,.78),
        "forest":(.19,.69,.34),"elite":(.57,.46,.90),"boss":(.27,.83,.57),
        "regent":(.48,.35,.85),"amber":(.90,.60,.19),"royal":(.64,.32,.82)}[theme]
    body=mat("DF_Gel",c,alpha=.36 if theme!="crop" else .44,rough=.10)
    inner=mat("DF_GelInner",tuple(min(1,v*1.3) for v in c),alpha=.40,rough=.2)
    eyes=mat("DF_Eyes",(.03,.08,.08) if theme not in ("boss","regent","royal") else (.92,.74,.19),
             emission=.75 if theme in ("boss","regent","royal") else 0)
    shard=mat("DF_FloatingCrystals",(.79,.86,.89),metal=.10)
    leaf=mat("DF_LeafBits",(.27,.60,.22))
    glow=mat("DF_Glow",(.96,.83,.26),emission=.75)
    # translucent gel surface and inset core, raised front-facing eyes
    uv("GelShell",(0,0,size*.55),(size*.68,size*.61,size*.56),body,24,16)
    uv("GelCore",(0,0,size*.57),(size*.50,size*.45,size*.41),inner,16,10)
    for side in (-1,1):
        uv("Eye",(.20*size*side,-.56*size,.65*size),(.095*size,.035*size,.14*size),eyes,10,7)
        uv("EyeGlimmer",(.17*size*side,-.594*size,.70*size),(.024*size,.012*size,.040*size),glow,8,5)
    if theme in ("road","forest","elite","boss","regent","royal"):
        # antenna with visible bob on top, for higher-tier slimes
        cylinder("Antenna",(0,0,1.04*size),(.12*size,0,1.35*size),.04*size,.024*size,body)
        uv("AntennaTip",(.12*size,0,1.39*size),(.11*size,.11*size,.11*size),inner)
    count={"crop":0,"meadow":2,"road":4,"forest":6,"elite":12,"boss":15,"regent":24,"amber":5,"royal":27}[theme]
    for i in range(count):
        angle=r.uniform(0,math.tau);z=r.uniform(.35,.84)*size
        rr=r.uniform(.10,.41)*size
        x=math.cos(angle)*rr;y=math.sin(angle)*rr
        if i%3==0:
            uv("FloatingLeaf",(x,y,z),(.11*size,.05*size,.035*size),leaf,8,5)
        elif i%3==1:
            uv("FloatingBubble",(x,y,z),(.07*size,.07*size,.08*size),inner,9,7)
        else:
            cylinder("FloatingGem",(x,y,z-.10*size),(x+.03*size,y,z+.10*size),.065*size,.02*size,shard,5)
    if theme in ("elite","boss","regent","royal"):
        for i in range(5 if theme=="elite" else 8):
            angle=i*math.tau/(5 if theme=="elite" else 8)
            x=math.cos(angle)*size*.43;y=math.sin(angle)*size*.43
            cylinder("OuterCrystal",(x,y,size*.60),(x*1.15,y*1.15,size*(.95 if theme=="elite" else 1.13)),
                .10*size,.006*size,shard,5)
    if theme in ("regent","royal"):
        for i in range(6):
            angle=i*math.tau/6
            x=math.cos(angle)*size*.37;y=math.sin(angle)*size*.37
            cylinder("RegalCrown",(x,y,size*.95),(x*1.10,y*1.10,size*1.29),.07*size,.007*size,glow,5)
    save("Slimes",name,scale=size,
       info="Real 3D semi-transparent gel shell with interior floating contents; later tiers larger and more complex.")

def mountain():
    clear()
    rock=mat("DF_Rock",(.36,.39,.41))
    shadow=mat("DF_DeepRock",(.21,.24,.27))
    ledge=mat("DF_Ledge",(.52,.51,.46))
    moss=mat("DF_Foliage",(.24,.39,.18))
    bronze=mat("DF_Bronze",(.66,.43,.18),metal=.58)
    seal=mat("DF_SealedLight",(.95,.63,.19),emission=1.5)
    rockrng=random.Random(884)
    # 14 m wide, 13 m high, dark passage forward-facing at negative Y.
    for i in range(23):
        a=i*2.399
        x=math.cos(a)*(2+(i%6)*1.4)
        y=math.sin(a)*2.3 + (i%3)*1.1
        h=rockrng.uniform(4.2,11.5)
        radius=rockrng.uniform(1.8,3.7)
        cylinder("MountainSpire",(x,y,0),(x+.6*math.sin(a),y+.25,h),radius,.04,rock if i%3 else shadow,6)
    for i in range(12):
        a=i*math.tau/12
        uv("BasalBoulder",(math.cos(a)*6.1,math.sin(a)*3.6,.8),
            (2.4,1.75,1.2),ledge,8,6)
    # independent doors: Unity may animate these transforms
    for side in (-1,1):
        cx=side*1.45
        door=cube("DoorLeft" if side<0 else "DoorRight",(cx,-4.02,2.42),(2.8,.35,4.5),shadow)
        cube("DoorFraming",(cx,-4.33,4.64),(2.85,.48,.31),bronze)
        for j in range(4):
            cube("RunicInscription",(cx+side*.20,-4.24,.92+j*.82),(.12,.085,.42),seal)
    for side in (-1,1):
        cylinder("GatePillar",(3.25*side,-3.95,0),(3.25*side,-3.95,5.8),.51,.36,ledge,8)
        uv("GateTorch",(3.3*side,-4.38,1.35),(.19,.15,.32),seal)
    cube("GateLintel",(0,-4.0,5.1),(7.7,1.10,.8),ledge)
    cylinder("GateKeystone",(0,-4.60,5.22),(0,-4.60,6.17),.32,.12,bronze,6)
    for i in range(5):
        uv("TerraceShrub",(math.cos(i)*4.0,-2.+i*.35,1.+i*.26),(.7,.7,.6),moss,10,6)
    save("Mountain","CornbergAdvancementMountain",scale=13.0,info="Separate DoorLeft/DoorRight transforms; open at level 10; destination must be its own advancement map.")

for i,s in enumerate(("Pine","Spruce","Oak","Birch","Maple","Willow","Twisted","Aspen","Cypress")):
    tree("Tree_"+s,s,511+i)
for i,r in enumerate(("Farmer","Merchant","Banker","Alchemist","Blacksmith","Villager","Guard","Herbalist")):
    npc("NPC_"+r,r,833+i)
for i,(name,size,theme) in enumerate([
    ("Slime_Crop",.33,"crop"),("Slime_Meadow",.78,"meadow"),
    ("Slime_Road",1.05,"road"),("Slime_Forest",1.38,"forest"),
    ("Slime_Amber",1.14,"amber"),("Slime_Elite",1.88,"elite"),
    ("Slime_Boss",2.65,"boss"),("Slime_Regent",3.35,"regent"),
    ("Slime_RoyalFuture",4.0,"royal")]):
    slime(name,size,1239+i,theme)
mountain()
(SOURCE/"asset_manifest.json").write_text(json.dumps(MANIFEST,indent=2),encoding="utf8")
print("COMPLETE",len(MANIFEST),"Blender source/FBX pairs",flush=True)

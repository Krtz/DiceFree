import bpy, math, os, json
from mathutils import Vector
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
root=r"T:\TEMP\DiceFree-Options\SourceArt\World\CornbergHealingWell"
os.makedirs(root,exist_ok=True)
def mat(name,color,rough=.8,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=m.node_tree.nodes.get("Principled BSDF");p.inputs["Base Color"].default_value=(*color,1);p.inputs["Roughness"].default_value=rough;p.inputs["Metallic"].default_value=metal
 return m
stone=[mat("Old limestone",(.42,.43,.37)),mat("Mossy stone",(.24,.36,.23)),mat("Warm limestone",(.56,.49,.37))]
wood=mat("Weathered oak",(.25,.13,.07));dark=mat("Oak cut grain",(.17,.085,.045));iron=mat("Wrought iron",(.12,.14,.16),.43,.65)
water=mat("Gentle healing water",(.16,.59,.67),.2,.05); gold=mat("Faint warm enchanted trim",(.68,.58,.31),.35,.3); moss=mat("Soft moss",(.18,.33,.11))
def cube(name,location,scale,material,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=location);o=bpy.context.object;o.name=name;o.dimensions=scale;bpy.ops.object.transform_apply(location=False, rotation=False, scale=True);o.data.materials.append(material)
 if bevel: mod=o.modifiers.new("Soft hand-worked edges","BEVEL");mod.width=bevel;mod.segments=2;o.modifiers.new("Weighted corners","WEIGHTED_NORMAL")
 return o
def cyl(name,loc,r,depth,material,vertices=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=loc);o=bpy.context.object;o.name=name;o.data.materials.append(material);return o
# Well silhouette: 2m diameter, low hand-built stones around actual reachable healing radius
for tier,z in enumerate((.14,.41,.67)):
 for i in range(12):
  a=math.tau*i/12+(0.02 if tier%2 else 0)
  block=cube("Well wall individual weathered masonry %02d %d"%(i,tier),(math.cos(a)*.87,math.sin(a)*.87,z),(.52,.31,.23),stone[(i+2*tier)%3],.045)
  block.rotation_euler[2]=a+math.pi/2
# black opening, distinct readable inner water
cyl("Dark inner well", (0,0,.72),.68,.025,dark,32)
cyl("Healing spring reflective surface",(0,0,.745),.63,.018,water,32)
# Four restrained colored inlays on upper rim
for i in range(8):
 a=math.tau*i/8
 o=cube("Old brass inset %d"%i,(math.cos(a)*.89,math.sin(a)*.89,.825),(.13,.10,.018),gold,.005);o.rotation_euler[2]=a
# sturdy posts, roof beam, pitched oak roof to give it distinctive landmark silhouette
for x in (-.87,.87):
 cube("Timber roof support %s"%x,(x,0,1.61),(.17,.18,1.72),wood,.03)
 cube("Support cap %s"%x,(x,0,2.45),(.29,.27,.12),dark)
ridge=cube("Roof timber ridge",(0,0,2.72),(2.22,.16,.16),wood)
for side in (-1,1):
 roof=cube("Pitched shingled roof %d"%side,(0,side*.46,2.67),(2.18,1.15,.09),wood,.016);roof.rotation_euler[0]=side*.49
 for x in (-.72,-.23,.26,.75):
  for y in (.0,.24,.48):
   sh=cube("Individual roof shake",(x,side*(.13+y),2.80-(.13+y)*.43),(.43,.28,.032),stone[(int((x+1)*5)+int(y*6))%3] if False else dark,.006);sh.rotation_euler[0]=side*.49
# winding wheel
wheel=cyl("Wooden crank wheel",(1.02,0,2.15),.23,.14,dark,16);wheel.rotation_euler[1]=math.pi/2
cube("Crank handle",(1.12,-.18,2.16),(.16,.08,.08),wood)
# rope, hook and wooden bucket above opening
cyl("Hanging rope",(0,0,1.83),.022,.97,gold,12)
cyl("Lowered wooden bucket",(0,0,1.20),.20,.30,wood,12)
for z in (1.08,1.31):cyl("Bucket metal hoops",(0,0,z),.205,.02,iron,16)
# partial moss pads along well footprint and enlarged farming-life cues
for j in range(16):
 a=math.tau*j/16
 if j%4==0:continue
 m=cube("Soft moss patch", (math.cos(a)*1.04,math.sin(a)*1.04,.70),(.17,.22,.046),moss,.025);m.rotation_euler[2]=a
# nearby carved healthy wheat symbol of well's subtle effect
plaque=cube("Healing well engraved symbol plaque",(0,-1.05,.37),(.30,.04,.32),stone[0])
stem=cube("Wheat stalk relief",(0,-1.080,.38),(.024,.020,.19),gold,.0)
for s in (-1,1):
 for k in range(3):
  ear=cube("Wheat kernel relief",(s*(.035+k*.016),-1.09,.35+k*.07),(.07,.02,.03),gold,.009);ear.rotation_euler[1]=s*.45
# safe FBX mesh-only export for isolated inspection, no scene placement
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,"CornbergHealingWell.blend"))
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.context.scene.objects:
 if o.type=="MESH":o.select_set(True)
bpy.ops.export_scene.fbx(filepath=os.path.join(root,"CornbergHealingWell.fbx"),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
with open(os.path.join(root,"manifest.json"),"w") as f:json.dump({"name":"CornbergHealingWell","authored":"Original procedural Blender model","source":"CornbergHealingWell.blend","export":"CornbergHealingWell.fbx","placement":"not integrated; standalone art proof","mesh_count":len([o for o in bpy.context.scene.objects if o.type=="MESH"])},f,indent=2)
print("VIS_WELL_ASSET_OK",len([o for o in bpy.context.scene.objects if o.type=="MESH"]))

import bpy,math,os,json,random
from mathutils import Vector
R=random.Random(2609)
root=r"T:\TEMP\DiceFree-Options\SourceArt\World\VisualBatch01"
os.makedirs(root,exist_ok=True)
def material(name,c):
 m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*c,1);return m
M={k:material(k,v) for k,v in {'Oak':(.30,.17,.09),'OakDark':(.17,.09,.04),'Stone':(.46,.45,.39),'StoneLight':(.63,.60,.51),'Moss':(.23,.38,.16),'Leaf':(.24,.43,.19),'LeafLight':(.41,.55,.20),'LeafGold':(.75,.50,.20),'Bark':(.22,.15,.09),'Iron':(.17,.19,.21),'Rope':(.69,.57,.32),'Crop':(.64,.61,.25),'Soil':(.27,.18,.10),'Slime':(.18,.65,.39),'Blue':(.18,.38,.76),'Shroom':(.70,.29,.19),'White':(.85,.83,.70)}.items()}
def cube(n,p,s,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(M[m]);return o
def cyl(n,p,r,h,m,verts=10):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=p);o=bpy.context.object;o.name=n;o.data.materials.append(M[m]);return o
def uv(n,p,r,m,segments=12,rings=6):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=r,location=p);o=bpy.context.object;o.name=n;o.data.materials.append(M[m]);return o
def trunk(n,h=3.3):
 cyl(n+' trunk',(0,0,h*.45),.22,h*.9,'Bark')
def make(kind,variant):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 t=variant
 if kind=='tree':
  trunk('Tree',3.1+t*.24)
  for i in range(7+t%3):
   a=i*2.399+t*.3; z=2.25+(i%3)*.47
   r=.75+(.15 if t%3==0 else 0)
   ob=uv('Organic foliage crown',(math.cos(a)*.48,math.sin(a)*.48,z),r,['Leaf','LeafLight','LeafGold'][t%3]);ob.scale=(1.05,.85,1.1)
 elif kind=='pine':
  cyl('Pine trunk',(0,0,1.9),.20,3.8,'Bark')
  for i in range(5):
   ob=uv('Pine branch layer',(0,0,1.2+i*.62),1.1-i*.15,'Leaf' if t%2==0 else 'LeafLight');ob.scale=(1,1,.42)
 elif kind=='rock':
  for i in range(3+t%3):
   ob=uv('Broken granite',(math.cos(i*2.4)*.38,math.sin(i*2.4)*.31,.42),.47+R.random()*.15,['Stone','StoneLight','Moss'][i%3]);ob.scale=(1.0,.85,.63);ob.rotation_euler=(.2*i,.13*i,.37*i)
 elif kind=='stump':
  cyl('Old cut stump',(0,0,.38),.43,.75,'Bark');cyl('Exposed rings',(0,0,.765),.43,.024,'Oak')
  for i in range(3):cube('Bark fracture',(math.cos(i*2.1)*.41,math.sin(i*2.1)*.41,.4),(.1,.10,.54),'OakDark')
 elif kind=='log':
  ob=cyl('Fallen forest log',(0,0,.34),.34,2.45,'Bark',14);ob.rotation_euler[1]=math.pi/2
  for x in (-1.2,1.2):
   o=cyl('Exposed pale timber',(x,0,.34),.32,.03,'Oak',14);o.rotation_euler[1]=math.pi/2
 elif kind=='crate':
  cube('Shipping crate',(0,0,.5),(1,1,1),'Oak')
  for z in (.18,.81):
   for y in (-.52,.52):cube('Crate reinforcing batten',(0,y,z),(.98,.08,.1),'OakDark')
  for x in (-.52,.52):cube('Side brace',(x,0,.5),(.08,.98,.11),'OakDark')
 elif kind=='barrel':
  cyl('Barrel staves',(0,0,.52),.43,1.04,'Oak',12)
  for z in (.17,.82):cyl('Metal barrel band',(0,0,z),.446,.075,'Iron',12)
  cyl('Lid',(0,0,1.04),.43,.04,'OakDark',12)
 elif kind=='cart':
  cube('Cart bed',(0,0,.77),(2.0,1.2,.2),'Oak')
  for y in (-.59,.59):cube('Cart side',(0,y,1.03),(2.0,.10,.42),'OakDark')
  for x in (-.63,.63):
   for y in (-.71,.71):
    ob=cyl('Spoked cart wheel',(x,y,.55),.43,.12,'OakDark',16);ob.rotation_euler[0]=math.pi/2
  for y in (-.39,.39):cube('Pull shaft',(1.62,y,.7),(1.5,.09,.10),'Oak')
 elif kind=='fence':
  for x in (-.95,0,.95):cube('Fence post',(x,0,.68),(.16,.18,1.35),'Oak')
  for z in (.49,1.03):cube('Fence rail',(0,0,z),(2.05,.10,.13),'OakDark')
 elif kind=='sign':
  cube('Ground sign post',(0,0,1.25),(.17,.20,2.5),'Oak')
  cube('Wooden marker',(0,.10,2.10),(1.30,.11,.43),'OakDark')
  cube('Inset writing surface',(0,.17,2.10),(1.09,.025,.27),'StoneLight')
 elif kind=='wheat':
  for i in range(9):
   x=(i%3-.95)*.30;y=(i//3-1)*.32
   cyl('Wheat stalk',(x,y,.52),.023,1.04,'Crop',6)
   for j in range(5):uv('Heavy grain',(x+.025,y,1.01+j*.073),.055,'Crop',8,4)
 elif kind=='mushroom':
  for i in range(4+t%3):
   x=math.cos(i*2.4)*.34;y=math.sin(i*2.4)*.30
   cyl('Mushroom stem',(x,y,.18),.065,.35,'White',8)
   cap=uv('Mushroom cap',(x,y,.38),.19,'Shroom' if t%2 else 'StoneLight');cap.scale=(1.2,1.2,.45)
 elif kind=='lantern':
  cube('Lantern foot',(0,0,.10),(.39,.39,.2),'Iron')
  for x in (-.15,.15):
   for y in (-.15,.15):cube('Lantern frame',(x,y,.43),(.04,.04,.64),'Iron')
  cube('Light-colored translucent insert',(0,0,.42),(.28,.28,.52),'Crop')
  cube('Lantern cap',(0,0,.79),(.45,.45,.13),'Iron')
 elif kind=='anvil':
  cube('Iron anvil base',(0,0,.37),(.53,.45,.30),'Iron')
  cube('Iron anvil saddle',(0,0,.73),(.9,.44,.18),'Iron')
  ob=cube('Anvil horn',(.64,0,.73),(.60,.21,.14),'Iron')
 elif kind=='hay':
  cube('Compressed straw bale',(0,0,.4),(1.35,.72,.8),'Crop')
  for y in (-.21,.21):cube('Binding rope',(0,y,.81),(.1,.05,.06),'Rope')
 elif kind=='bridge':
  for i in range(9):
   cube('River crossing board',(0,-2+i*.5,.48),(2.6,.44,.16),'Oak')
  for x in (-1.3,1.3):
   for y in (-1.9,0,1.9):cube('Bridge rail post',(x,y,1.13),(.15,.15,1.43),'OakDark')
   cube('Bridge rail',(x,0,1.75),(.14,4.2,.13),'Oak')
 elif kind=='slimejar':
  cyl('Slime sample sealed vessel',(0,0,.33),.32,.66,'StoneLight',16)
  uv('Suspended slime mass',(0,0,.33),.22,'Slime',12,6)
  cyl('Wax seal',(0,0,.70),.32,.10,'Shroom',16)
 elif kind=='root':
  for i in range(5):
   angle=i*math.tau/5
   o=cyl('Ancient crossing root',(math.cos(angle)*.8,math.sin(angle)*.8,.22),.13,2,'Bark',8);o.rotation_euler[1]=.95;o.rotation_euler[2]=angle
  trunk('Twisted sacred root',1.7)
 elif kind=='altar':
  cyl('Old stone plinth',(0,0,.24),1.0,.48,'Stone',10)
  cyl('Ceremonial upper stone',(0,0,.55),.84,.17,'StoneLight',10)
  for i in range(5):
   a=i*math.tau/5;ob=uv('Five ring etched gem',(math.cos(a)*.7,math.sin(a)*.7,.65),.12,'Slime');ob.scale=(1,1,.22)
 if t>0 and kind in ('crate','barrel','sign','cart','hay','fence','rock'):
  for obj in bpy.context.scene.objects:
   if obj.type=='MESH':obj.rotation_euler[2]+=.06*t
 out=os.path.join(root,kind+'_'+str(variant+1).zfill(2))
 bpy.ops.wm.save_as_mainfile(filepath=out+'.blend')
 bpy.ops.object.select_all(action='SELECT')
 bpy.ops.export_scene.fbx(filepath=out+'.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
 return {'name':kind+'_'+str(variant+1).zfill(2),'meshes':sum(o.type=='MESH' for o in bpy.context.scene.objects),'source':os.path.basename(out)+'.blend','fbx':os.path.basename(out)+'.fbx'}
assets=[]
counts={'tree':8,'pine':4,'rock':5,'stump':3,'log':3,'crate':4,'barrel':4,'cart':2,'fence':4,'sign':3,'wheat':5,'mushroom':4,'lantern':3,'anvil':2,'hay':3,'bridge':2,'slimejar':3,'root':2,'altar':2}
for k,count in counts.items():
 for i in range(count):
  x=make(k,i);assets.append(x);print('ART_OK',x['name'],flush=True)
with open(os.path.join(root,'manifest.json'),'w') as f:json.dump({'generator':'SourceArt/build_visual_batch_01.py','author':'Original procedural Blender geometry; no third-party assets used','status':'standalone source models only - not integrated into scenes','asset_count':len(assets),'items':assets},f,indent=2)
print('BATCH_DONE',len(assets),flush=True)

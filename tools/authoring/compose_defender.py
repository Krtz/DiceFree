"""Build the Trimery Defender from DiceFree's original hand and body rig."""
from pathlib import Path
import sys
source, target_path = map(Path, sys.argv[1:])
s = source.read_text(encoding="utf8")
for a,b in {
    "Ironpine Ranger":"Trimery Defender","Ironpine rig":"Trimery rig","Ironpine skeleton":"Trimery skeleton",
    "Ironpine-Ranger":"Trimery-Defender","Ironpine-Shortbow":"Trimery-Gate-Sword","Ironpine-Longbow":"Trimery-Gate-Shield",
    "02 | Shortbow":"02 | Gate sword","03 | Longbow":"03 | Gate shield","Ranger-Hero":"Defender-Hero",
    "344f37":"234458","604a2d":"364f62","293830":"222c3b","70783b":"502e38",
    "bdb078":"b19b77","4b2b20":"211a1b","9c6240":"533630",
    "Green glass bead":"Blue enamel","79af99":"589bc8","477d72":"2d526d",
}.items():
    s=s.replace(a,b)
start=s.index("# The Acolyte's high")
end=s.index('for s in (-1,1):\n    tube("Tapered sideburn"',start)
s=s[:start]+s[end:]
start=s.index('gem("Brow clasp setting"')
end=s.index("def bind_rig",start)
s=s[:start]+'''
# Steel plates and an enamelled Trimery crest over a padded blue arming coat.
steel=material("Blue-tempered plate","597185",.35,.76)
darksteel=material("Blackened steel edges","273844",.40,.73)
edge=material("Hand polished plate rim","a8bfca",.31,.82)
enamel=material("Trimery shield enamel","244b68",.37,.38)

loft("Rounded steel crown",[(1.708,0,.010,.137,.137),(1.750,0,.012,.148,.143),
     (1.815,0,.018,.138,.133),(1.879,0,.029,.091,.095),(1.901,0,.038,.017,.023)],
     steel,"Head",n=64,levels=28)
band("Steel brow visor",curve_path([(-.140,.066,1.700),(-.116,-.077,1.717),(0,-.147,1.723),
    (.116,-.077,1.717),(.140,.066,1.700)],50),.033,darksteel,"Head",depth=.013)
line("Etched brow rim",curve_path([(-.140,.058,1.716),(-.104,-.092,1.733),
    (0,-.150,1.738),(.104,-.092,1.733),(.140,.058,1.716)],55),.005,edge,"Head")
for side in (-1,1):
    x=side
    cheek=[(x*.136,-.065,1.724),(x*.144,.052,1.716),(x*.121,.083,1.558),
           (x*.095,-.003,1.541),(x*.113,-.060,1.598)]
    model("Articulated cheek guard",cheek,[(0,1,2),(0,2,3),(0,3,4)],[steel],
          weights="Head",smooth=False,depth=.015)
    line("Cheek guard rim",cheek,.005,edge,"Head",loop=True)
    oval("Visor pivot",(x*.146,-.040,1.698),(.011,.010,.011),brass,"Head",20)
line("Crown spine",curve_path([(0,.153,1.749),(0,.109,1.872),(0,.017,1.908),
    (0,-.095,1.843),(0,-.121,1.748)],37),.006,edge,"Head")

loft("Fluted upper cuirass",[(1.048,0,.015,.156,.115),(1.113,0,.010,.181,.129),
    (1.216,0,.004,.210,.143),(1.317,0,.004,.223,.144),(1.391,0,.011,.167,.119)],
    steel,body,n=68,levels=34)
for sign in (-1,1):
    for j in range(3):
        x=sign*(.037+j*.045)
        line("Raised cuirass fluting",curve_path([(x*.72,-.106,1.083),(x,-.134+j*.005,1.203),
            (x*1.15,-.141+j*.007,1.288),(x*1.36,-.113+j*.009,1.352)],22),.0038,edge,body)
    for k in range(3):
        shoulder=(sign*(.193+k*.038),-.005,1.359-k*.033)
        plate=oval("Overlapping steel pauldron",shoulder,(.113-k*.008,.123-k*.008,.053),steel,"Clavicle."+("L" if sign>0 else "R"),32)
        plate.rotation_euler.y=sign*.45
        oval("Brass shoulder rivet",(shoulder[0],-.123+k*.008,shoulder[2]+.014),(.014,.008,.014),
             brass,"Clavicle."+("L" if sign>0 else "R"),22)
    ext="L" if sign>0 else "R"
    for j in range(3):
        plate=oval("Fluted gauntlet plate",(sign*(.359+.041*j),-.047,1.154-j*.044),(.060,.046,.044),steel,"Forearm."+ext,24)
        plate.rotation_euler.y=sign*.52
    for j in range(4):
        plate=oval("Layered hip tasset",(sign*(.116+.011*j),-.124+.006*j,1.034-j*.047),(.084,.024,.039),
             steel,"Skirt."+ext,24)
        plate.rotation_euler.y=-sign*.19
    oval("Polished knee cop",(sign*.108,-.055,.485),(.078,.057,.091),steel,"Shin."+ext,32)
    loft("Gilded steel greave",[(.164,sign*.116,-.005,.055,.043),(.25,sign*.113,-.012,.065,.052),
         (.38,sign*.110,-.009,.073,.054),(.46,sign*.109,-.006,.070,.051)],steel,"Shin."+ext,n=32,levels=20)
    line("Greave ridge",[(sign*.116,-.051,.176),(sign*.114,-.069,.29),(sign*.110,-.063,.427)],.005,
         edge,"Shin."+ext)
box("Trimery cuirass badge",(.003,-.155,1.258),(.116,.022,.113),enamel,body,bevel=.008)
box("Badge lintel",(.003,-.168,1.280),(.084,.011,.014),brass,body,bevel=.002)
for x in (-.026,.003,.032):
    box("Badge pillar",(x,-.168,1.249),(.011,.011,.046),bright,body,bevel=.002)

target=wand
loft("Sword leather grip",[(-.108,0,0,.019,.017),(-.069,0,0,.024,.019),(.063,0,0,.021,.017)],
     hide,None,n=36,levels=16)
for i in range(8):
    line("Grip brass thread",[(.022*cos(j*TAU/28),.019*sin(j*TAU/28),-.087+i*.017+.008*sin(j*TAU/28)) for j in range(28)],
         .0022,brass,loop=True)
oval("Faceted counterweight",(0,0,-.125),(.037,.025,.028),brass,None,28)
line("Swept guard",curve_path([(-.143,-.006,.070),(-.080,0,.082),(0,0,.079),(.085,0,.077),
    (.145,-.007,.053)],32),.021,brass)
stations=[(.086,.048),(.15,.057),(.46,.045),(.66,.032),(.763,.0007)]
vertices=[(x,y,z) for z,width in stations for x,y in [(-width,0),(0,-.016),(width,0),(0,.014)]]
faces=[(i*4+j,i*4+(j+1)%4,(i+1)*4+(j+1)%4,(i+1)*4+j) for i in range(4) for j in range(4)]
model("Honed high-carbon blade",vertices,faces,[steel,edge],smooth=False,surface=lambda i:i%2)
line("Sword fuller",[(0,-.017,.117),(0,-.017,.415),(0,-.015,.619)],.004,darksteel)

target=grimoire
outline=[(-.296,.340),(-.181,.390),(.181,.390),(.296,.340),(.320,.134),(.267,-.180),
         (.139,-.356),(0,-.487),(-.139,-.356),(-.267,-.180),(-.320,.134)]
def shield_panel(name,scale,mat,depth):
    border=[(x*scale,-.078+abs(x)*.31,z*scale) for x,z in outline]
    points=border+[(0,-.102,0)]
    model(name,points,[(i,(i+1)%len(border),len(border)) for i in range(len(border))],
         [mat],smooth=False,depth=depth)
    return border
rim=shield_panel("Steel shield shell",1.0,darksteel,.043)
shield_panel("Blue enamel shield face",.90,enamel,.021)
line("Hammered brass shield rim",rim,.017,brass,loop=True)
for x,y,z in rim:
    oval("Shield rim rivet",(x*.95,y-.016,z*.95),(.013,.010,.013),edge,None,20)
line("Crest lintel",[(-.143,-.106,.128),(0,-.118,.142),(.143,-.106,.128)],.026,brass)
for x in (-.100,0,.100):
    line("Trimery crest pillar",[(x,-.112,-.101),(x,-.116,.135)],.018,bright)
line("Gate footing",[(-.135,-.102,-.123),(0,-.119,-.132),(.135,-.102,-.123)],.019,brass)
line("Shield hand grip",[(-.105,.035,0),(-.077,.075,0),(.077,.075,0),(.105,.035,0)],.023,hide)
target=character

'''+s[end:]
start=s.index('hand=rig.pose.bones["Hand.L"].matrix')
end=s.index("target=studio",start)
s=s[:start]+'''
for side,group in (("R",wand),("L",grimoire)):
    hand=rig.pose.bones["Hand."+side].matrix
    position=hand @ Vector((0,.060,-.026))
    rotation=Quaternion(Vector((0,1,0)), -.61) if side=="R" else Quaternion(Vector((0,0,1)),.10)
    set_equipment_pose(group,"Trimery "+side+" grip",position,rotation)

'''+s[end:]
target_path.write_text(s,encoding="utf8")
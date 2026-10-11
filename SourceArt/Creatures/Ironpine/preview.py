import bpy,sys,math
from mathutils import Vector
from pathlib import Path
kind=sys.argv[-1]
out=Path(r"T:\TEMP\DiceFree-Options\SourceArt\Creatures\Ironpine")
bpy.context.scene.render.engine='CYCLES'
bpy.context.scene.cycles.samples=24
bpy.context.scene.render.resolution_x=820
bpy.context.scene.render.resolution_y=620
bpy.context.scene.render.resolution_percentage=100
world=bpy.context.scene.world or bpy.data.worlds.new('White')
bpy.context.scene.world=world
world.color=(.25,.32,.38)
for obj in list(bpy.data.objects):
    if obj.type=='CAMERA' or obj.type=='LIGHT':bpy.data.objects.remove(obj,do_unlink=True)
bpy.ops.object.camera_add(location=(3.0,4.5,3.0) if kind!="Quillrat" else (2.5,3.4,2.1))
cam=bpy.context.object
target=Vector((0,0,.75 if kind!="Quillrat" else .43))
cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.type='ORTHO';cam.data.ortho_scale=3.8 if kind!="Quillrat" else 2.8
bpy.context.scene.camera=cam
for loc,pow,size in [((1.5,1.5,5),120,4),((-3,0,4),100,4),((1,-3,3),130,3)]:
    bpy.ops.object.light_add(type='AREA',location=loc);a=bpy.context.object;a.data.energy=pow;a.data.shape='DISK';a.data.size=size
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.025))
floor=bpy.context.object
mat=bpy.data.materials.new('Forest-floor');mat.diffuse_color=(.15,.22,.15,1)
floor.data.materials.append(mat)
bpy.context.scene.render.image_settings.file_format='PNG'
bpy.context.scene.render.filepath=str(out/(kind+'_preview.png'))
bpy.ops.render.render(write_still=True)
print('IRONPINE_PREVIEW',str(out/(kind+'_preview.png')))

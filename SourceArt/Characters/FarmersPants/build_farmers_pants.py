"""Blender 5.2.2: deterministic Cornberg workwear on the actual Novice rig."""
from pathlib import Path
import math, json, hashlib
import bpy, bmesh
from mathutils import Vector
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SOURCE = ROOT / 'SourceArt/Characters/Novice/Novice.blend'
EXPORT = ROOT / 'Assets/_DiceFree/Art/Characters/FarmersPants/Models/FarmersPants.fbx'

def mat(name, color):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (*color, 1)
    m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value = .95
    return m

def tube(name, rings, material):
    vertices, faces = [], []
    count = 24
    for cx, z, rx, ry in rings:
        for i in range(count):
            a = i * math.tau / count
            # Broad cloth folds rather than texture-scale noise.
            fold = 1 + .025 * math.cos(6*a)
            vertices.append((cx + rx*math.cos(a)*fold, ry*math.sin(a)*fold, z))
    for j in range(len(rings)-1):
        for i in range(count):
            a, b = j*count+i, j*count+(i+1)%count
            faces.append((a,b,b+count,a+count))
    faces += [tuple(reversed(range(count))), tuple((len(rings)-1)*count+i for i in range(count))]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces); mesh.update()
    obj = bpy.data.objects.new(name, mesh); bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj

def main():
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = 'NONE'
    rig = bpy.data.objects['Novice_Armature']; rig.animation_data_clear()
    for b in rig.pose.bones: b.matrix_basis.identity()
    for o in list(bpy.data.objects):
        if o != rig: bpy.data.objects.remove(o, do_unlink=True)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    canvas = mat('FarmersPants_DustyCanvas', (.34,.30,.21))
    trim = mat('FarmersPants_WornHem', (.43,.37,.25))
    # Pelvis shell intersects both leg tubes; union gives one closed garment.
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=16, location=(0,0,.886))
    pelvis = bpy.context.object; pelvis.name = 'FarmersPants_Canvas'
    pelvis.scale = (.219,.149,.155)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    pelvis.data.materials.append(canvas)
    parts = [pelvis]
    for side, s in [('Left',-1),('Right',1)]:
        parts.append(tube(side+'Leg', [(s*.12,.22,.087,.095),(s*.12,.28,.092,.102),
            (s*.12,.40,.100,.111),(s*.12,.53,.112,.123),(s*.12,.60,.121,.132),
            (s*.113,.72,.128,.143),(s*.103,.85,.130,.143),(s*.10,.95,.120,.130)], canvas))
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active = pelvis; bpy.ops.object.join()
    remesh = pelvis.modifiers.new('Continuous cloth shell','REMESH'); remesh.mode='VOXEL'; remesh.voxel_size=.009
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    meshes=[pelvis]
    for side,s in [('Left',-1),('Right',1)]:
        meshes.append(tube(side+'PantsHem',[(s*.12,.218,.089,.098),(s*.12,.242,.091,.100),(s*.12,.264,.094,.103)],trim))
    meshes.append(tube('PantsWaistband',[(0,.962,.202,.139),(0,.990,.198,.137),(0,1.012,.185,.128)],trim))
    bone_names=['Hips','LeftUpLeg','LeftLeg','RightUpLeg','RightLeg']
    for o in meshes:
        groups={n:o.vertex_groups.new(name=n) for n in bone_names}
        for v in o.data.vertices:
            z=v.co.z; side='Left' if v.co.x<0 else 'Right'
            hip=max(0,min(1,(z-.83)/.13))
            knee=max(0,min(1,(z-.49)/.14))
            values={'Hips':hip,side+'UpLeg':(1-hip)*knee,side+'Leg':(1-hip)*(1-knee)}
            for n,w in values.items():
                if w>0: groups[n].add([v.index],w,'REPLACE')
        for p in o.data.polygons: p.use_smooth=True
        bm=bmesh.new(); bm.from_mesh(o.data)
        bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(o.data)
        assert all(e.is_manifold for e in bm.edges),o.name
        assert bm.calc_volume(signed=True)>0,o.name
        assert all(f.calc_area()>1e-10 for f in bm.faces),o.name
        bm.free()
        o.parent=rig; mod=o.modifiers.new('Actual Novice lower body','ARMATURE'); mod.object=rig
    digest=hashlib.sha256()
    for o in meshes:
        coords=[tuple(round(float(c),5) for c in v.co) for v in o.data.vertices]
        vertices=sorted((coords[v.index],tuple(sorted((o.vertex_groups[w.group].name,round(w.weight,5)) for w in v.groups))) for v in o.data.vertices)
        faces=sorted((p.material_index,tuple(sorted(coords[i] for i in p.vertices))) for p in o.data.polygons)
        digest.update(json.dumps([o.name,vertices,faces],separators=(',',':')).encode())
    EXPORT.parent.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(HERE/'FarmersPants.blend'))
    bpy.ops.object.select_all(action='SELECT'); bpy.context.view_layer.objects.active=rig
    bpy.ops.export_scene.fbx(filepath=str(EXPORT),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,
        primary_bone_axis='Y',secondary_bone_axis='X',axis_forward='-Z',axis_up='Y',apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL',bake_anim=False,use_mesh_modifiers=True,path_mode='AUTO',embed_textures=False)
    manifest=dict(itemId='item.cornberg.farmers-pants',slot='Legs',units='meters',blenderVersion=bpy.app.version_string,
        noviceSourceSha256=hashlib.sha256(SOURCE.read_bytes()).hexdigest(),geometryWeightsSha256=digest.hexdigest(),
        closedOutwardMeshes=True,weightedBones=bone_names,vertices=sum(len(o.data.vertices) for o in meshes),
        triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes),meshes=[o.name for o in meshes])
    (HERE/'export_manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    print('FARMERS_PANTS_EXPORT_OK '+json.dumps(manifest))
if __name__=='__main__': main()

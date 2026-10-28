import bpy
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=r'T:\TEMP\DiceFree-Options\SourceArt\Vendored\CosmicWonderSimpleBoardgameDice\model.glb')
bpy.ops.export_scene.fbx(filepath=r'T:\TEMP\DiceFree-DiceAsset\CosmicWonderDice.fbx',use_selection=False,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')

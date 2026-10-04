# Novice Character Pipeline Prototype

This is the first DiceFree humanoid-rig and character-presentation proof, not final production art. It establishes one androgynous, blank-slate Echo with a simple T-shirt and baseline underwear presentation. The normal Novice starts without equipped gear; the sword is present only in a validation fixture.

`Novice.blend` is the editable Blender source. `build_novice.py` reconstructs the source, writes a humanoid FBX to `Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx`, and writes the authored collision profile and source counts. Blender units are metric, Z-up and +Y-forward; Unity import is responsible for the axis conversion. Run with Blender 5.2.2 LTS:

```powershell
blender --background --python SourceArt/Characters/Novice/build_novice.py
```

The FBX contains one armature, separate body/T-shirt/underwear skinned meshes, named rig bones, animation actions for Idle, Locomotion and UnarmedAttack, and DCC reference empties for `RightHandWeapon`, `LeftHandOffhand`, `Head`, `LeftFoot` and `RightFoot`. The presentation prefab copies the socket contract onto matching imported bones and adds baseline slot roots. The sword fixture uses the existing Cornberg Field Sword prefab without altering its mesh.

The gameplay capsule profile is authored explicitly in `novice_body_profile.json`; it is not derived from the render bounds and is not embedded in the art prefab. No gameplay systems consume this art proof yet.

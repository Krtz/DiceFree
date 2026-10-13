# Cornberg Round Shield

One modest prototype heater shield used to prove the Novice left-hand offhand presentation contract. This is presentation-only art; it does not change gameplay equipment, blocking, stats or the Novice's unequipped starting state.

## Source and export

- Editable source: `CornbergRoundShield.blend`
- Rebuild script: `build_cornberg_round_shield.py`
- Unity export: `Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Models/CornbergRoundShield.fbx`
- Unity presentation prefab: `Assets/_DiceFree/Art/Weapons/CornbergRoundShield/Prefabs/CornbergRoundShieldPresentation.prefab`
- Validation scene: `Assets/_DiceFree/Art/Validation/Scenes/NoviceShieldPipelinePreview.unity`
- Unity preview: `Assets/_DiceFree/Art/Validation/Previews/NoviceShield_Unity.png`

Rebuild from the repository root with Blender 5.2.2 LTS:

```powershell
blender --background --python SourceArt/Weapons/CornbergRoundShield/build_cornberg_round_shield.py
```

The generator checks that the four shield meshes are closed and outward-facing, then reports a geometry fingerprint. Two clean runs produced `6f4d0e258c28e8177cd36f92dbbd2df5f34e39c739183c9cd3164de61bce4849`. The source is metric. The Unity import measured `0.61 × 0.75 × 0.39 m`, 472 vertices, 256 triangles and 4 MeshFilters. These measurements establish a prototype baseline only.

The shield's broad face is authored toward Blender local `-Y`, exported with the project's `-Z` forward / `Y` up FBX settings, and imported toward Unity local `+Z`. DCC-authored `Grip` and `ShieldFront` are exported and remain the canonical anchors. Unity wrapper aliases are copied from those imported transforms. The presentation root stays identity at Grip.

## Offhand presentation contract

The Novice prefab owns `LeftHandOffhand` as a child of its imported Humanoid LeftHand bone. The socket's position and orientation are authored relative to that bind pose to face the Novice's forward direction. The shield presentation is parented to this socket with local position zero, identity local rotation and unit local scale; its Grip coincides with the socket, and Grip-to-ShieldFront follows the socket forward axis. The Editor validator samples Locomotion and checks that the socket and shield anchors move together.

The shield fixture exists only in the isolated validation scene. Ordinary Novice gameplay remains unequipped. Editor commands are `dicefree.art.shield.prepare`, `dicefree.art.shield.validate`, `dicefree.art.shield.capture-preview` and `dicefree.art.shield.build-windows`.

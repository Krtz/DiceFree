# Cornberg Field Sword

One original, humble one-handed sword used to prove the DiceFree DCC-to-Unity pipeline. It is deliberately a practical local smith's tool: broad straight blade, stout plain guard, dark wrapped grip and a squat pommel. No glow, runes, filigree or borrowed franchise silhouette.

## Source and export

- Native editable scene: `CornbergFieldSword.blend`
- Reproducible Blender source/export script: `build_cornberg_field_sword.py`
- Isometric proportion/readability preview: `Previews/CornbergFieldSword_Isometric.png`
- Unity export: `Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Models/CornbergFieldSword.fbx`
- Unity presentation: `Assets/_DiceFree/Art/Weapons/CornbergFieldSword/Prefabs/CornbergFieldSwordPresentation.prefab`

Rebuild the editable scene, FBX and preview from the checked-in script with Blender 5.2.2 LTS:

```powershell
blender --background --python SourceArt/Weapons/CornbergFieldSword/build_cornberg_field_sword.py
```

The script is repository-relative and uses fixed mesh dimensions, materials, camera, lights and FBX exporter settings. Rebuilding reports a geometry SHA-256 over the named mesh vertices, faces and material assignments. Two verification runs produced the same fingerprint: `da4ec476c442701299ce537f9c645ee43609e8012fc5ffa519f7b52d6d6c3b1e`. This verifies deterministic generated geometry; FBX container bytes may vary between Blender exports. The FBX contains the sword root, blade, hilt, grip and named `Grip` / `Tip` anchors. The preview stage and scale rod are excluded from FBX export.

## Model conventions

- Metric; 1 Blender unit = 1 meter = 1 Unity world unit.
- Grip pivot is the root origin.
- Editable Blender source points along Blender +Y. The export script applies a temporary half-turn around the source up axis before the standardized FBX axis conversion (`-Z` forward, `Y` up), making the Unity blade tip point along local +Z without changing the saved source orientation.
- Root/import scale is 1,1,1; no animation or collider is authored.
- The model is intentionally around 1.1 m overall: a little oversized for isometric legibility, still proportioned as a one-handed sword.
- Separate broad materials: cool practical steel, lighter edge planes, warm plain guard metal, and dark brown grip leather.

## Unity boundary

The FBX is imported synchronously with Unity 6000.6.3f1 using unit scale 1, animation disabled and embedded material import disabled. The Editor-only `dicefree.art.sword.prepare` command creates the URP/Lit materials, presentation prefab, and preview scene; `dicefree.art.sword.validate` checks import scale/orientation, grip pivot, tip reach, material/shader assignments, and the collider-free presentation boundary. `dicefree.art.sword.build-windows` builds only the preview scene as a Windows Development player. The preview scene is not added to gameplay build settings.

Item stats, gameplay hit logic, and the future character socket are outside this art-only proof. The sword is intentionally standalone until the actual Novice model establishes a hand socket.

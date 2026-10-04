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

The script is repository-relative and uses fixed mesh dimensions, materials, camera, lights and FBX exporter settings. Rebuilding reports a geometry SHA-256 over the named mesh vertices, faces, material assignments and polygon normals. Two clean verification runs produced the same fingerprint: `26dc5d60219a37e71dc2836d30f9fb8e19345fde2520e370a8710348cbc788d6`. This verifies deterministic generated geometry/content; FBX container bytes may vary between Blender exports. Before export, the script checks that every sword mesh is closed/manifold and has positive signed volume, indicating outward-facing surfaces. The FBX contains the sword root, blade, hilt, grip and DCC-authored `Grip` / `Tip` anchors. The preview stage and scale rod are excluded from FBX export.

## Model conventions

- Metric; 1 Blender unit = 1 meter = 1 Unity world unit.
- The DCC-authored `Grip` at `(0, 0, 0)` is the canonical pivot. Unity validates the imported anchor and copies it to a top-level wrapper alias; the wrapper never estimates a grip from mesh bounds.
- The DCC-authored `Tip` is `(0, 0.905, 0)` in Blender local coordinates. Unity validates it arrives about 0.905 m forward from Grip and copies it to a top-level wrapper alias. The imported FBX anchors remain canonical.
- Editable Blender source points along Blender +Y. The export script applies a temporary half-turn around the source up axis before the standardized FBX axis conversion (`-Z` forward, `Y` up), making the Unity blade tip point along local +Z without changing the saved source orientation.
- Root/import scale is 1,1,1; no animation or collider is authored. The imported sword is 1.1015 m long overall.
- The model is intentionally around 1.1 m overall: a little oversized for isometric legibility, still proportioned as a one-handed sword.
- Separate broad materials: cool practical steel, lighter edge planes, warm plain guard metal, and dark brown grip leather.

## Unity boundary

The FBX is imported synchronously with Unity 6000.6.3f1 using unit scale 1, animation/clips, cameras, lights, blend shapes, generated colliders and embedded material import disabled. Imported normals are used. The FBX includes Unity's approximately 270-degree X-axis conversion on the visual child; the presentation wrapper root remains identity at Grip and preserves this conversion.

The Editor-only commands are:

- `dicefree.art.sword.prepare` creates the URP/Lit materials, presentation prefab and preview scene. The imported FBX anchors are canonical; wrapper Grip/Tip objects are position/rotation aliases copied from those imported transforms.
- `dicefree.art.sword.validate` checks source/export naming and presence, the expected Blade/Hilt/Grip parts, imported anchors, bounds, mesh counts/normals, importer settings, four unique materials and the collider-free presentation boundary. The tested Unity import has dimensions `0.3485 × 0.0961 × 1.1015 m`, 1,028 vertices, 502 triangles, 3 MeshFilters and 4 unique presentation materials.
- `dicefree.art.sword.capture-preview` renders the dedicated preview camera at 1280×800 into `Assets/_DiceFree/Art/Validation/Previews/CornbergSword_Unity.png`. It uses the isolated scene's 1.8 m scale rod and does not require a character model.
- `dicefree.art.sword.build-windows` builds only the preview scene as a Windows Development player. The preview scene is not added to gameplay build settings.

The generator triangulates the closed blade root cap and uses recalculated outward normals. Unity materials retain standard URP backface culling (`Cull Back`); a missing face must be repaired in source geometry rather than hidden by double-sided rendering. These settings and counts are established by this sword proof and are not yet a universal model-import preset.

Item stats, gameplay hit logic, and the future character socket are outside this art-only proof. The sword is intentionally standalone until the actual Novice model establishes a hand socket.

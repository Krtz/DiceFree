# Original Briarback quilboar prototype

Provisional humanoid quilboar creature, distinct from animal quillrats. Original DiceFree geometry: boar skull/snout/tusks, amber eyes, ears, shoulder/back quills, heavy body, kilt and cloven hooves.

build_quilboar.py reuses the first-party SourceArt/Characters/Novice/build_novice.py skeleton and action helpers, with new creature geometry. Reproduce with Blender in background mode from the project root; the script writes the working blend, FBX and asset_manifest.json. No external provider or paid generation. Unity's BriarbackAuthoring creates URP materials, Generic animation controller, role palettes and original hand equipment.

The catalog's 240 downloaded packs were searched for boar/quilboar/orc/goblin/tribal creature candidates first; none matched this creature. This original source has no Asset Store raw-redistribution restriction.

Authoring geometry: 2651 vertices, 5098 triangles, 22 bones. Idle, Locomotion, Attack, Slam and Death. Five actions share the existing navigation/attack system; visual state driver observes actual displacement and windups. No root-motion gameplay authority. Working provenance and FBX hash are in asset_manifest.json.

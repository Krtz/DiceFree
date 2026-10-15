# Walking villagers

Derived first-party DiceFree visuals for the four existing Cornberg `VillageHomeWander` NPCs (two farmers, a villager and an herbalist). The original unrigged Cornberg assets remain available and unchanged.

`build_walking_villagers.py` loads the original primitive construction functions from `SourceArt/build_cornberg_batch.py` with the original role seeds. It adds a 16-bone Generic rig, weighted clothing with knee/elbow subdivisions, and in-place Idle/Walk actions. Pitchforks follow the holding hand. Existing Unity URP materials are reused by renderer name. Gameplay root objects retain navigation, colliders, NPC identity and interactions.

Rebuild from the project root using Blender 5.2.2 LTS:

```powershell
& 'T:\TEMP\blender.exe' -b -t 4 --python SourceArt/World/WalkingVillagers/build_walking_villagers.py
```

Then run the Unity Editor commands `dicefree.villager-walk.install` and `dicefree.villager-walk.validate`. The install command authors importer settings, controllers, prefabs and scene references through Unity APIs. `dicefree.villager-walk.capture` produces the pose inspection image at `docs/screenshots/WalkingVillagers-Poses.png`.

`provenance.json` records the original generator hash, seeds, Blender version, exported FBX hashes, bone/vertex counts and zero unweighted vertices. These are original project assets with no third-party input or license dependency. The local game-dev packaging CLI was unavailable; this uses the project's existing Blender/Unity authoring workflow, with no canonical package or lock-receipt claim.

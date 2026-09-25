# Technical Direction

This document records direction, not a frozen implementation contract.

## Engine

Current preferred direction: **Unity**, 3D, isometric/angled camera, PC-first.

We will choose the current appropriate Unity LTS when active development begins rather than lock an engine version during the design-only phase.

## Source control

GitHub remains the central project home.

When the Unity project begins:
- use a Unity-aware `.gitignore`;
- configure Git LFS before adding large binary art assets;
- keep generated Unity folders out of source control;
- use branches and pull requests for implementation work.

## Architecture principles

### Data-driven content
Classes, items, enemies, loot tables, regions and advancement requirements should primarily be data definitions rather than hard-coded chains.

### Modular systems
Keep combat, character stats, classes, items, world, quests, dungeons, UI and networking separated enough to evolve independently.

### No future monolith
Do not repeat the early DiceBound pattern of allowing one giant file to become the game.

### Multiplayer-aware, not multiplayer-first
Core game rules should avoid assumptions that make networking impossible later, but the first vertical slice can be built and proven locally before full co-op infrastructure is added.

## Likely high-level runtime areas

```
Core/
Characters/
Combat/
Classes/
Abilities/
Items/
World/
Quests/
Dungeons/
AI/
Multiplayer/
UI/
Persistence/
Tools/
```

## Character art pipeline

Before producing large quantities of wearable gear:
1. define scale;
2. define base humanoid rig;
3. define bone/socket names;
4. define armor slots;
5. define skinned-mesh rules;
6. define export/import conventions;
7. test several radically different armor sets.

## Tooling likely needed when development starts

- Unity Hub + chosen Unity LTS
- C# IDE (Visual Studio or Rider)
- Git
- Git LFS
- Blender
- GitHub

Additional tools should be justified by an actual production need.

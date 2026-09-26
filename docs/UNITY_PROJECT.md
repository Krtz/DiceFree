# Unity project setup

The repository now contains an engine project, but gameplay remains unimplemented.
Earlier design-phase documents are preserved; this document records the concrete
engine setup that supersedes their pending engine-selection/setup instructions.

## Open the project

Use **Unity 6000.6.3f1** (Unity 6), recorded in
`ProjectSettings/ProjectVersion.txt`. In Unity Hub, use **Add project from disk**
and select the repository root: the directory containing `.git`, `Assets`,
`Packages`, and `ProjectSettings`. Do not create another project inside it.

The initial scene is `Assets/_DiceFree/Scenes/Main.unity`. It contains only the
Universal 3D template's camera, directional light, and global volume.

## Rendering and packages

Based on the installed **Universal 3D / URP Blank 17.2.1** template, with
**Universal Render Pipeline 17.6.0** and **Input System 1.19.0** as direct
dependencies. Unity resolves their required dependencies in `packages-lock.json`.
The template's tutorial, sample input actions, collaboration integration,
IDE integrations, navigation, Timeline, test framework, and Visual Scripting
were not included as direct dependencies. Add tooling when it is actually needed.

The template's PC and Mobile renderers, quality levels, and volume settings are
retained under `_DiceFree/Settings`. PC remains the initial target; the Mobile
quality preset does not imply a commitment to mobile development.

## Source control

- Visible Meta Files: `ProjectSettings/VersionControlSettings.asset`.
- Force Text: `m_SerializationMode: 2` in `ProjectSettings/EditorSettings.asset`.
- Commit assets together with their `.meta` files. Move them through Unity, or
  move each asset and its `.meta` together to preserve GUID references.
- Empty organizational directories use `.gitkeep` plus Unity folder metadata.
- Existing `.gitignore` and `.gitattributes` remain authoritative; `.slnx` is
  also ignored for modern generated IDE solutions. Caches and
  local settings stay untracked; binary art/media use Git LFS.
- Run `git lfs install` after cloning on another computer.

## Organization and future work

Project-owned assets live under `Assets/_DiceFree`: Art, Audio, Materials,
Prefabs, Scenes, Scripts, Settings, and VFX. Scripts has Core, Characters,
Combat, Classes, Items, World, UI, and Tools directories without placeholder code.
Add further domains when implementation needs them; no runtime architecture or
assembly boundaries are imposed by this setup.

Keep future content data-driven. No character rigs, scale overrides, equipment
slots, sockets, networking systems, or import presets are defined yet. Establish
and test the shared humanoid/modular equipment and Blender import conventions
before producing character assets, as described in `TECHNICAL_DIRECTION.md`.

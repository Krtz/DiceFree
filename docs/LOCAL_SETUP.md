# Local Development Setup

## Before active development

Install:
- Unity Hub
- the chosen Unity LTS editor
- Visual Studio or Rider with Unity/C# support
- Git
- Git LFS
- GitHub Desktop (optional but convenient)
- Blender

## Clone

Clone `Krtz/DiceFree` locally before adding the Unity project.

Recommended local path:

```
C:\Dev\DiceFree
```

Avoid cloud-synced folders such as OneDrive for the working copy.

## Unity project placement

The Unity project should live at the **repository root** so the repo eventually contains:

```
DiceFree/
├── Assets/
├── Packages/
├── ProjectSettings/
├── docs/
├── .github/
├── .gitignore
├── .gitattributes
└── README.md
```

Do not create `DiceFree/DiceFree/Assets`.

If Unity Hub refuses to create a project directly in the non-empty cloned repository, create a temporary project elsewhere, close Unity, then copy only `Assets/`, `Packages/`, and `ProjectSettings/` into the cloned DiceFree repository and open that repository folder from Unity Hub.

## Unity project settings

Once the project exists, confirm:

- **Version Control / Meta Files:** visible meta files
- **Asset Serialization:** Force Text

These make Unity scenes, prefabs, materials and other serialized assets much friendlier to Git.

## Git LFS

Run once on the computer:

```powershell
git lfs install
```

The repo's `.gitattributes` already marks common 3D, texture, audio and video formats for LFS.

## First commit

The first Unity commit should contain only the clean generated project skeleton and configuration. Do not import asset packs yet.

Expected tracked Unity folders:

- `Assets/`
- `Packages/`
- `ProjectSettings/`

Expected ignored folders include:

- `Library/`
- `Temp/`
- `Obj/`
- `Logs/`
- `UserSettings/`
- local IDE-generated files

## Rule

Before mass-producing visible armor or character models, prototype and lock the base humanoid rig, slots, sockets and Blender → Unity import pipeline.

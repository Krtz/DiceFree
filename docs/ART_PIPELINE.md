# DiceFree 3D Art and DCC Pipeline

This document defines the working 3D-content pipeline for DiceFree.

It is intentionally small enough for the current project while establishing conventions before large quantities of weapons, armor and characters exist.

## Visual direction

DiceFree should use a **stylized, readable, moderately exaggerated fantasy look**.

Reference feeling:
- Warcraft III / Warcraft-like strong silhouettes;
- Magicka-like playful readable fantasy proportions;
- not photorealistic;
- not super-deformed/cartoon-only.

References are mood/readability targets only. Do not copy identifiable proprietary weapon, armor, character or texture designs.

At the isometric gameplay camera:
- silhouettes matter more than tiny surface detail;
- weapons may be slightly oversized;
- shapes should read quickly;
- material groups should be clear;
- excessive micro-detail is wasted.

## First validation sequence

Do not mass-produce equipment yet.

Validate the pipeline in this order:

1. one original one-handed sword;
2. one shield;
3. one pair of shoes;
4. one hat.

Only the sword is authorized as the first model.

Each step should expose a new pipeline problem before larger production:
- sword: rigid held prop / grip / silhouette / import;
- shield: second rigid attachment orientation;
- shoes: paired wearable/foot alignment and later rig implications;
- hat: head attachment/character silhouette.

## World scale

DiceFree world scale is:

**1 Unity world unit = 1 meter.**

DCC source scenes use metric scale.

Model against a consistent ~1.8m human reference/mannequin unless the target class form says otherwise.

Weapons are allowed to be **slightly oversized for isometric readability**.

For the first one-handed sword, use roughly realistic one-handed proportions with a modest stylized exaggeration rather than a giant anime-sized weapon.

The first sword's fantasy is intentionally humble: **a chunky, practical piece of Cornberg metal** rather than ornate adventurer/hero gear. It should look locally made, sturdy and readable, not prestigious.

Exact dimensions remain prototype tuning.

## Repository layout

Keep source and runtime exports in the SAME Git repository for now.

### Editable DCC sources

Store native working files outside Unity's `Assets/` directory, for example:

```
SourceArt/
  Weapons/
    CornbergSwordPrototype/
      CornbergSwordPrototype.blend
      references/
      README.md
```

These files are version-controlled through Git LFS.

Unity must not import native `.blend` files directly as the production runtime asset pipeline.

### Unity-ready exports

Export runtime-ready files into the Unity project:

```
Assets/_DiceFree/Art/
  Weapons/
    CornbergSwordPrototype/
      Models/
        CornbergSwordPrototype.fbx
      Materials/
      Textures/
      Prefabs/
```

The Unity-ready export is also version controlled, with large binary formats under Git LFS according to `.gitattributes`.

Source file + exported runtime asset are both kept so:
- future edits remain possible;
- builds do not require Blender;
- import behavior is reproducible;
- broken exports can be diagnosed.

## Why not a separate "runtime-only" Git branch?

Branches represent lines of source development, not packaging tiers.

A runtime-only branch would create:
- constant merge/synchronization work;
- risk that source and exported assets drift;
- ambiguous source of truth;
- unnecessary history duplication.

Players should never need to clone the development repository to play DiceFree.

Playable builds are distributed as build artifacts/releases.

## When a separate art-source repository might make sense later

Do not split now.

Reconsider a dedicated art-source repository only if there is a concrete reason such as:
- source art becomes extremely large;
- Git LFS storage/bandwidth materially harms developer workflow;
- external artists need different repository permissions;
- source-art history must be retained/distributed independently;
- CI checkout time becomes a demonstrated problem.

If a split happens, use a repository-level boundary with an explicit version/export contract. Do not emulate it with a permanently divergent branch.

## Player distribution

Do not commit Windows builds to source control.

`Build/` and `Builds/` remain ignored.

For development/pre-alpha distribution:
- build a Windows player;
- zip the complete build output;
- attach the zip as a GitHub Release/pre-release asset or use a build-distribution service.

Later public distribution can use Steam/itch.io/etc.

A player download contains the built game, not:
- `.blend` source files;
- tests;
- editor tools;
- design docs;
- raw Unity project.

Unity's build pipeline determines runtime content from build scenes/references and any explicitly managed runtime content systems.

## Blender working convention

Working baseline:

- Unit system: Metric.
- Model to real-world-ish meter scale.
- Apply object transforms before final export.
- Keep source origin/orientation intentional.
- Remove accidental hidden/helper geometry from export.
- Name exported objects/materials intentionally.
- Export only required runtime objects.

Blender is Z-up while Unity is Y-up.

Use a standardized FBX export preset/script rather than hand-tweaking settings for every asset.

Working FBX orientation target:
- Forward: -Z
- Up: Y
- unit/transform conversion configured consistently.

The exact exporter preset should be proven with the first sword and then frozen into tooling.

## Rigid weapon convention

For rigid one-handed weapons:

- pivot/root at the intended primary-hand grip center;
- local +Z points approximately from grip toward the weapon's functional forward/blade-tip direction;
- local +Y is the weapon's authored up direction;
- scale is 1,1,1 after import;
- no unexplained 90-degree corrective rotation should be required on every instance.

The equipment prefab may contain an authored attachment offset relative to the character hand socket. Do not modify the mesh source differently for every character.

## Attachment/socket direction

Weapons and armor should attach through named character sockets/bones, not scene-specific transforms.

Target socket vocabulary will be finalized with the base character rig.

Expected early concepts include:
- right-hand weapon socket;
- left-hand/offhand socket;
- head socket;
- left/right foot/rig bones as appropriate.

Do not lock production armor around the current capsule placeholder.

The first sword can validate mesh/import/prefab conventions before the final humanoid rig exists; final held alignment waits for a real/shared character rig or a dedicated attachment-test rig.

## Visual progression / rarity readability

Equipment rarity and progression should be visible in presentation, not only in tooltip color.

Direction:
- early/common gear can be plain, practical and materially simple;
- higher rarity may gain more distinctive silhouette, ornament, material treatment and authored detail;
- endgame/high-end equipment may use **glow/emissive effects, particles, trails or other VFX** where appropriate;
- visual escalation should reinforce rarity without making every higher-rarity item visually noisy;
- geometry, materials and VFX can all participate; do not force rarity to be represented by only one of them.

Keep gameplay readability first. VFX should not obscure telegraphs or combat state.

## Character forms are not one humanoid race

Do **not** architect DiceFree around every class sharing one human body.

Class advancement may change the Echo's species/body form as part of class identity.

Settled examples:
- the Tier-2 **Ranger is a Wood Elf**;
- the Tier-2 **Wizard is a High Elf**.

Current possibilities, not yet locked:
- Berserker may be an Orc;
- a Tier-4 tank may be a Centaur.

Therefore the art/rig/equipment pipeline must eventually support:
- multiple humanoid proportions;
- potentially non-human humanoids;
- non-biped body plans;
- class-specific rigs or rig families where needed;
- shared semantic equipment attachment concepts mapped onto different rig/socket layouts;
- explicit equipment/form compatibility restrictions when an item's authored model does not fit a body plan.

### One authored body presentation per class

Each class/Way has **one authored body/sex presentation**.

A class may be:
- visibly male;
- visibly female;
- deliberately androgynous.

There is no requirement to make male/female body variants of every class.

The class form itself is part of the Way's identity.

As the Echo advances into more specialized Ways, the model should generally become **more visually distinct in the direction of that class fantasy**. Early forms can remain closer to the blank-slate Echo; later forms can become increasingly species/body/silhouette specific.

A descendant Way may **fully transform the body/species again**. A Ranger becoming a Wood Elf does not permanently lock every descendant to that exact body. Later advancement may replace the form with something substantially different if that Way's identity calls for it.

### Gear appearance does not auto-adapt between body forms

An individual equipment appearance keeps its authored visual form.

Do **not** build automatic body-form scaling, morphing, remeshing, alternate-mesh selection or "fit this same hat to every race" machinery as the default.

Instead:
- gameplay items/appearances declare compatible classes, form families or semantic eligibility;
- a Ranger-compatible hat can be authored at Wood-Elf proportions;
- a Centaur-compatible hat can be authored for that class/form;
- equipment authored for a class/form family is **compatible with that form and its descendants by default**;
- a descendant may explicitly narrow/override compatibility where its new body plan makes the inherited appearance unsuitable;
- unrelated/incompatible forms cannot equip/use that appearance unless explicitly authored.

This preserves deliberate silhouettes and prevents every piece of equipment from becoming a multi-rig content burden.

### Armor slot visual replacement

Visible wearable equipment normally **replaces the class-form presentation for that equipment slot** rather than merely layering on top of a permanent class outfit.

Examples:
- equipped Chest replaces the visible chest clothing/armor presentation;
- equipped Head replaces the visible head-slot equipment presentation;
- equipped Feet replaces the visible footwear presentation.

The underlying body/form remains class-authored. Slots without visible equipment use that form's authored baseline presentation.

This does not require every body form to support every item; compatibility restrictions still decide what can be equipped.

Do not require every class to deform onto one universal human skeleton merely to simplify tooling.

Reuse rigs/animation families where it is genuinely compatible, but allow a class to own a radically different form.

Default rig strategy:
- visually/structurally similar forms should share a rig/animation family where practical;
- Human / High Elf / Wood Elf-like humanoids may share a compatible humanoid family if testing proves it works cleanly;
- materially different forms such as Orc-like bodies may use a different humanoid family when proportions/animation quality warrant it;
- radically different/non-biped forms such as a possible Centaur use an appropriate separate rig family.

Do not force universal-rig reuse when it damages silhouette or animation quality, and do not create a unique rig per class when an existing family genuinely fits.

## Weapon state

Do **not** plan a sheathing system as a baseline requirement.

For the current direction, equipped weapons may simply remain visibly held while equipped.

Do not create hip/back sheath sockets, draw/sheathe state machines, or sheathing animations unless a later feature explicitly needs them.

## Materials and textures

Initial art should target URP-compatible PBR materials.

Prefer:
- a small, clear material count;
- readable broad material separation;
- reusable materials where appropriate;
- texture resolution proportional to actual isometric screen size.

Do not produce 4K textures by default for tiny props.

Exact texture-size/texel-density budgets will be established from real camera tests.

## Model import standards

Use Unity ModelImporter defaults/presets or an AssetPostprocessor once the first few assets prove the desired settings.

The goal is consistent:
- scale;
- normals/tangents;
- material handling;
- animation import disabled for rigid props;
- mesh optimization/readability choices;
- collider policy.

Do not rely on each contributor remembering checkbox settings manually.

## Collider policy

Visual meshes and gameplay collision are separate concerns.

A weapon model does not automatically need a detailed MeshCollider.

Combat hit/range logic should use authored combat rules/hit shapes, not triangle collision from decorative weapon geometry unless a future mechanic explicitly needs it.

### Character form hitboxes

Playable class forms are allowed to have **different gameplay hitboxes/physical footprints**.

A larger or differently shaped body is not presentation-only.

Each class/form profile should author collision/selection/navigation footprint data appropriate to its body, for example:
- capsule/shape dimensions;
- selection radius;
- NavMesh/agent radius or equivalent traversal footprint where needed;
- other body-space interaction measurements that genuinely depend on physical form.

Do not derive gameplay collision automatically from renderer bounds or arbitrary mesh triangles. The hitbox/footprint is explicit authored gameplay data associated with the class form.

Changing Way/body form may therefore change the gameplay hitbox. Balance consequences should be deliberate and testable.

## Prefab boundary

The imported FBX is raw art.

A Unity equipment prefab owns game-facing presentation such as:
- mesh renderer;
- material assignment;
- attachment offset;
- optional VFX anchors;
- optional audio/VFX metadata;
- future cosmetic variation hooks.

Gameplay item definitions should reference presentation assets through a clean presentation field/adapter rather than contain rendering logic.

Item stats must not live in the model/prefab.

## VFX attachment points

When useful, rigid weapon prefabs may expose named child transforms such as:
- grip/origin;
- tip;
- impact/trail endpoints.

Do not invent dozens of sockets until effects actually need them.

The first sword should at least make grip and tip positions inspectable.

## Validation scene/tool

Create a small editor/art validation setup rather than judging assets only inside full gameplay.

It should eventually allow:
- known human-scale reference;
- isometric gameplay camera preview;
- turntable/free camera;
- socket preview;
- material inspection;
- bounds/scale/pivot diagnostics.

Do not make Cornberg the only place to test art.

## Source-control rules

Native DCC files and exported binary art use Git LFS.

Unity `.meta` files stay in source control.

Unity serialization remains text for diffable serialized assets.

Never move a Unity asset without its `.meta` file.

Do not commit:
- Blender autosave/temp files;
- rendered scratch output;
- Unity Library/Temp;
- local build output.

## Automated validation direction

Once the first sword pipeline is proven, add editor validation for:
- expected import scale;
- nonzero mesh bounds;
- sane physical dimensions;
- required prefab/socket names;
- missing material references;
- unexpected animation clips on rigid props;
- source/export naming consistency.

Later character/wearable validation should check rig/bone/socket contracts.

## Research basis

This pipeline follows current Unity/GitHub/Blender guidance:

- Unity recommends keeping important source content in version control and commonly outside `Assets/` so native DCC files are not imported automatically.
- Unity recommends exporting production models to FBX rather than using native DCC formats directly in the project.
- Unity expects 1 meter in the world to correspond to 1 Unity unit for physics/lighting scale.
- Unity recommends prefab-first, modular content and version-control-friendly project organization.
- Git LFS is appropriate for large binary source/media files.
- GitHub Releases/build artifacts are the distribution mechanism for playable binaries rather than source branches.

# Ironpine creature art, animation, and spatial sound — October 12, 2026

Branch: `feature/visual-overhaul-vis00`. Builds on the existing first-playable Ironpine Wildlife v1.4 AI implementation. **This is playable original art, not the final production-quality animal pass.**

## Existing Unity Asset Store collection reviewed

The local offline DiceFree Asset Catalog contains 210 downloaded packages. We searched its real model/file listings and examined the relevant candidates:

- **Willowwood – Free Low Poly Nature Pack** (BluBlu Games): a suitable stylized wolf static FBX and fox model. Useful as an aesthetic/scale reference, but no reusable matching full combat animation rig was available.
- **Realistic Furry Wolf FREE sample** (Rip Vertices Studio): anatomically useful high-detail wolf, but too realistic and fur-shader-dependent for the exaggerated DiceFree direction.
- **PixeliusVita FreeTrial 30 Monster** and **Monster Evo packs**: plenty of animated monsters; none is a ready-to-drop-in Diablo II-style animal quillrat or appropriate wolf.
- **SoundBits Free Sound FX Collection**: natural dog barks, growls and group sounds; installed on the authorized PC as an **optional, locally licensed** audio pack.
- **Monster Roars** and **DeadlyCreaturesPack1v1**: fantasy roars available; rejected as baseline small woodland wildlife voices.

**License boundary:** No downloaded third-party FBX, textures, animations or raw WAVs have been committed to GitHub. All three creatures, meshes, rigs, actions, palette materials and generated audio fallbacks are our original authored source. The optional SoundBits WAVs are unpacked to `Assets/_DiceFree/Resources/Audio/IronpineLicensed/` on the original PC and **gitignored**. They may be embedded in authorized game builds under their applicable license, but must not be published as raw source files. Clone/build without these assets uses original generated sound fallbacks.

## Authored real 3D monsters

All editable originals: `SourceArt/Creatures/Ironpine/`:
- `Wolf.blend`, `Quillrat.blend`, `AlphaWolf.blend` are editable Blender 5.2 sources.
- `build_ironpine_creatures.py` deterministically rebuilds meshes, 15-bone skeletons, vertex weights and actions, then exports FBXs.
- `asset_manifest.json` records topology, source/export and animation actions.

The three exported FBXs live in `Assets/_DiceFree/Art/Creatures/Ironpine/`. Each has ten authored actions, including **Idle, Walk, Run, Bite, a species special (Howl or QuillShot), Pounce/Scurry, Sniff/Forage, Scratch, Hurt and Death**. The imported FBX animations are real transforms on the skeleton, skinning the actual geometry. No Unity primitives are used as the creature models.

Grey and Dark Wolves share wolf geometry, with different fur/face palettes and scale; the Alpha has its own extra mane, pale face scar, scale and combat silhouette. Three individual quillrat coat palettes (moss, amber, slate) have separate belly, eyes and quill materials. The authored quillrat remains a four-legged thorny animal, **never a humanoid quilboar**.

Preview renders from the original Blender source, **not screenshots of game runtime**:

![Rigged Grey Wolf](screenshots/IronpineWolfRigPreview.png)

![Rigged Quillrat](screenshots/IronpineQuillratRigPreview.png)

![Rigged Alpha Wolf](screenshots/IronpineAlphaRigPreview.png)

## Integrated runtime

- All **22 pre-existing wildlife CombatActors** in `Cornberg.unity` had only their procedural blockout models replaced. Their home habitats, level definitions, movement, combat, damage, rewards and saved identity remain in place.
- Each entity has a skinned FBX instance, an Animator with dedicated `Wolf_Animator.controller`, `Quillrat_Animator.controller`, or `AlphaWolf_Animator.controller`, plus original URP material overrides.
- `IronpineCreaturePresentation` chooses actual Walk/Run from NavMeshAgent velocity; ambient Sniff/Forage/Scratch from cooldown; Bite from BasicAttack wind-up; Howl, Pounce, Scurry and QuillShot from AI actions; Hurt and Death from real HP events; Alive respawn resets state. Animator root motion is disabled so animation never teleports gameplay actors.
- `IronpineCreatureAudio` uses a spatial logarithmic 3D AudioSource (2.2m min / 28m max) with subdued random pitch/volume. Wolf sounds prefer optional licensed recordings, but everything has on-demand authored procedural fallbacks. Quillrat squeaks, quill travel sound, howl, sniff/rustle, wounds and death all resolve without the optional pack.
- The Unity Editor install/validate code is `IronpineCreatureArtAuthoring.cs`; detailed deformation validator in `IronpineRigMotionValidation.cs`. Installer remains safe to run on already-rigged creatures without generating duplicates.

## Verified in isolated Unity 6000.6.3f1 batch (on original PC)

- **C# compile:** no errors; Unity's existing obsolete-API warnings are nonfatal.
- **Scene art validation:** PASS, 22 correct rigged enemies, 10 mapped animation states each.
- **Imported locomotion loops:** Idle, Walk and Run are explicitly configured to loop by the FBX ModelImporter for all three species. Unity `IronpineCreatureArtAuthoring.Validate` now checks those three imported clips on all 22 actors; strict post-fix batch validation returned code 0.
- **Real skin deformation:** `IRONPINE_RIG_MOTION_OK` for Wolf (**227 moving vertices**), Alpha (**227**), Quillrat (**158**). Walk sampling moves the FrontLUpper leg bone ~24° and measurably deforms skinned vertices.
- **Runtime Play Mode smoke:** `IRONPINE_WILDLIFE_PLAYMODE_OK`. All 22 living, NavMesh-ready actors have models, controllers and spatial audio; none carries a loot drop component.
- These tests used isolated copy `T:\TEMP\DiceFree-IronpineValidation` because the previous locked Windows GUI Editor did not respond to remote commands. The original GUI Unity process was **explicitly closed with user approval** before this work. Temporary third-party Unity package assembly shims in the validation project were **not** copied into the production repo.
- During Play Mode, Unity Editor's pre-existing `SearchDatabase` startup indexing raises an `ArgumentOutOfRangeException` unrelated to these scripts. Creature smoke marker was reached.

## Additional polish still needed

These are real mesh/rig/animation implementations, not mock-ups, but a full quality target remains: inspect playthrough visuals in the desktop Game view, tune animation foot sliding, polish Alpha mane/expressiveness, replace synthetic quillrat squeaks/howl with professionally authored game audio, adjust environmental occlusion, run real 1–4-player wolf/quillrat/Alpha encounter tests, tune fight balance and visual effects. Work on Village 2 and humanoid tribal quilboars only when specifically designed later.

Run the generator after editing the meshes:

```powershell
& "T:\TEMP\blender.exe" --background --python "T:\TEMP\DiceFree-Options\SourceArt\Creatures\Ironpine\build_ironpine_creatures.py"
```

Never merge to main without approval; preserve unrelated `ProjectSettings/PackageManagerSettings.asset` modifications.

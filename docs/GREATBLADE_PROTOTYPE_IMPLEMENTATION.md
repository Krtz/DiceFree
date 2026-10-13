# Greatblade Unity prototype — implementation notes (2026-10-13)

**Branch:** `feature/visual-overhaul-vis00`. The textured visual prototype is repaired and tested in Unity. Greatblade combat and class advancement remain unfinished.

## Visual setup

- The playable class and retired swordswoman use the same local resource, `GreatbladeLicensed/GreatbladeHero`. The tracked `Assets/_DiceFree/Scenes/GreatbladePrototype.unity` scene displays both appearances. Open it and enter Play Mode to instantiate the models.
- Female Warrior Princess source: `Assets/AllStarCharacterLibrary/Models/Characters/A03.FBX`. Missing original A03 diffuse and normal textures were restored with their original metadata from the user's cached Unity package. The derived body material uses that atlas in URP. Only the full body renders; the overlapping A03_LOD and the character's native Longsword are disabled.
- `GreatbladeHumanoidRig` builds a Humanoid Avatar from a temporarily normalized T-pose, mapping 51 human bones including 30 finger joints. It leaves the vendor FBX and bind pose intact. The derived asset is `Assets/_DiceFree/Resources/GreatbladeLicensed/GreatbladeFemaleTposeAvatar.asset`. The old Avatar is retained locally because another process held it open.
- Blink's WalkForward, MeeleeAttack_TwoHanded and SpinToWin_TwoHanded supply the motion. Local derived clips add a raised two-handed ready stance, preserve that upper-body guard while walking, and close the fingers around the grip during swing and spin. Original animation files remain unchanged.
- The oversized weapon uses `Assets/PurePoly/Free_Swords/Prefabs/PP_Sword_0363.prefab`. Its length is 2.75 world units; width and thickness retain source scale so the handle fits the hands. Measured leather-handle anchors replace the original blade-centred pivot attachment.
- `GreatbladeTwoHandGrip` aligns the sword with the authored palm line after animation, fits the fixed handle within both arms' reach, and solves the actual arm bones in LateUpdate. This affects visual bones only. It does not move the actor root or implement damage, movement or class rules.
- The portrait stage has its own original neutral material, front camera and directional light. Authoring uses an additive scene and validation uses a preview scene, preserving the current gameplay scene.

## Recreate and inspect

With licensed sources installed, run:

```text
unity command dicefree.greatblade.rig-human --project-path T:\TEMP\DiceFree-Options --format json
unity command dicefree.greatblade.prototype.install --project-path T:\TEMP\DiceFree-Options --format json
unity command dicefree.greatblade.prototype.validate --project-path T:\TEMP\DiceFree-Options --format json
```

The standalone `dicefree.greatblade.visuals.repair` command refreshes the derived visual assets after prototype installation. Asset recovery from the user's package cache was a local repair; these commands do not download or redistribute licensed art.

## Validation completed

- Unity 6000.6.3f1 compiled the final runtime and editor changes with zero compilation errors. The final Console contained 115 compiler warnings in unchanged code (114 obsolete-API warnings and one unreachable-code warning), with none from the repaired files.
- `dicefree.greatblade.prototype.validate` returned `DICEFREE_GREATBLADE_VISUAL_AND_PROGRESS_OK`: original texture assigned, one body, one sword, all 30 finger joints, valid Humanoid rig, and 80 sampled poses across ready, walk, swing and spin. Maximum sampled palm-to-grip error was under 0.003 mm.
- A live Play Mode test drove both runtime visual adapters through all four states. It measured 3,730 frames after LateUpdate, with maximum right-hand error 0.0022 mm and left-hand error 0.0028 mm. Neither actor root moved. These metrics measure configured palm anchors; live screenshots were also inspected for visible skin and hand placement.
- The final ready view and swing were captured from the live scene. An immediate preview-scene render showed a displaced swing hand that was not present in the live view; live Play Mode confirmed the actual grip. Do not treat those immediate offscreen pose screenshots as authoritative for skin placement.
- The local review evidence, source receipt, backups and live report are under `Library/GreatbladeVisualRepair20261013/`. The final preview is `Greatblade-Repaired-2026-10-13.png`.
- The existing static progression checks still pass: four L1 entries (two active skills and two distinct passives), Overhead Breaker at L6, uneven rank cadence, and ultimate slot 9 restricted to Tier IV+.
- No full player build, actual Greatblade skill damage, collision-safe Spin Dash travel, Momentum consumption, skill-rank spending or mentor quest was implemented or tested by this visual repair.

## Design and next implementation

`docs/GREATBLADE_PROTOTYPE_DESIGN_V03.md` remains the class design source. Greatblade is Tier II after Physical L30 and begins at class level 1. The six initial abilities are Wide Arc, Spin Dash, Overhead Breaker, Battle Cry, Greatsword Mastery and Heavy Rhythm. Momentum gains from attacks/skills and is consumed by the finisher; there are no threshold Momentum buffs. Greatsword Mastery rank 4's shield versus heal remains undecided.

The next useful slice is a playable Wide Arc with Heavy Rhythm/Momentum feedback against a test target. Then implement ground-targeted Spin Dash with collision-safe travel and route hits, followed by the Momentum-consuming Overhead Breaker. Connect class advancement, equipment and the retired mentor after the combat loop is demonstrable.

## Local startup recovery

The existing Avatar file was open in a Node process; the new Avatar path avoids the blocked replacement without terminating that process. Unity's automatic Package Manager launch also failed on this PC. The editor successfully reopened using the already running local Package Manager service:

```text
T:\TEMP\unity\6000.6.3f1\Editor\Unity.exe -projectPath T:\TEMP\DiceFree-Options -upmIpcPath Upm-Diagnostic
```

That command depends on the local `Unity-Upm-Diagnostic` service still running. No antivirus exclusion or security setting was changed.

## Licensed art and public repository

These paths remain gitignored and local only: `Assets/AllStarCharacterLibrary/`, `Assets/Blink/`, `Assets/PurePoly/`, and `Assets/_DiceFree/Resources/GreatbladeLicensed/`. This includes the derived Avatar, clips, material, controller and prefab. Never stage their bytes or original cached Unity packages. Public clones use the existing original code-drawn fallback.

Preserve `EditorProfiles/primary-echo.json`, unrelated material/settings edits, and Cornberg gameplay. This repair changes no save schema, combat progression or action-bar layout. Do not merge to main without approval.
